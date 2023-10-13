using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BaseTask : StepContainer, ITask
    {
        [PropertyOrder(-1)]
        public TagContainer TagContainer = new TagContainer();

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public BaseStep ActiveStep { get; private set; }
        
        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public int CurrentStepId { get; protected set; }

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public TaskState State { get; protected set; }

        public bool IsActive => State == TaskState.Running;
        
        protected bool _initialized;
        
        //==============================================================================================================
        // Events

        public delegate void TaskEvent();
        
        public event TaskEvent TaskStarted;
        public event TaskEvent TaskStopped;
        public event TaskEvent TaskCompleted;

        public delegate void StepEvent(BaseStep step);

        public event StepEvent StepStarted;
        public event StepEvent StepCompleted;
        
        //==============================================================================================================
        // Methods
        
        protected virtual void Awake()
        {
            CurrentStepId = -1;

            foreach (var step in Steps)
                step.BindContainer(this);

            OnAwake();

            _initialized = true;
        }

        protected virtual void OnAwake()
        {
            
        }

        protected virtual void Start()
        {
            foreach (var step in Steps)
                step.Initialize();
            State = TaskState.Initialized;
        }

        private void OnDisable()
        {
            // task need to clean up itself (mostly for conditions)
            // if tasks need to be disabled and preserve their state this call can be moved to 'OnDestroy'
            StopTask();
        }

        protected virtual void OnDestroy()
        {
            if (Steps != null)
            {
                foreach (var step in Steps)
                    step.UnbindContainer();
            }
        }

        public void StartTask()
        {
            if (State != TaskState.Initialized)
                return;

            State = TaskState.Running;
            
            if (Steps == null)
                PLog.Error<MagnusLogger>($"Steps for task '{this.name}' are null");

            OnStart();
            
            TaskStarted?.Invoke();
        }

        protected virtual void OnStart()
        {
            
        }

        public void StopTask()
        {
            if (State != TaskState.Running && State != TaskState.Paused)
                return;

            State = TaskState.Initialized;

            if (ActiveStep)
                PLog.Info<MagnusLogger>($"[BasicTask::StopTask] Step {CurrentStepId + 1}: {ActiveStep.name}", ActiveStep);

            foreach (var step in Steps)
                step.Terminate();

            CurrentStepId = -1;
            ActiveStep = null;

            OnStop();
            
            TaskStopped?.Invoke();
        }

        protected virtual void OnStop()
        {
        }

        protected virtual void Update()
        {
            if (State != TaskState.Running)
                return;
            
            // If task completed
            if (CurrentStepId >= Steps.Count)
            {
                TaskCompleted?.Invoke();
                OnCompleted();

                State = TaskState.Finished;
                return;
            }

            if (ActiveStep == null || ActiveStep.State == ProcessState.Finished)
                TryHandleStepAdvancement();
            
            if (ActiveStep != null)
                ActiveStep.HandleUpdate();
        }

        private void TryHandleStepAdvancement()
        {
            if ((ActiveStep == null && CurrentStepId == -1) || (ActiveStep !=  null && ActiveStep.State == ProcessState.Finished))
            {
                IncrementStepToNextAvailable();

                PLog.Info<MagnusLogger>($"Moving on to next step ({CurrentStepId})....");
                ActiveStep = GetCurrentStep();
                
                if (ActiveStep == null)
                {
                    PLog.Trace<MagnusLogger>($"CurrentStep is null for Task \"{this.name}\".");
                    return;
                }
                
                ActiveStep.StartStep();
            }
        }

        private void IncrementStepToNextAvailable()
        {
            ++CurrentStepId;

            // Skip disabled steps
            var currentStep = GetCurrentStep();
            while (currentStep != null && !currentStep.isActiveAndEnabled)
                currentStep = Steps.GetAtIndex(++CurrentStepId);
        }
        
        protected virtual void OnCompleted()
        {
            
        }
        
        public void ResetTask()
        {
            State = TaskState.Initialized;
            
            // Optimized reset, reset only what has ran
            for (var i = CurrentStepId; i >= 0; --i)
                Steps[i].ResetStep();
        }

        private BaseStep GetCurrentStep()
        {
            if (State != TaskState.Running || CurrentStepId == -1)
                return null;

            BaseStep currentStep = Steps.GetAtIndex(CurrentStepId);
            return currentStep;
        }

        public int GetStepIndex(BaseStep startStep)
        {
            return Steps.IndexOf(startStep);
        }
        
        public override void NotifyStepStarted(BaseStep baseStep)
        {
            StepStarted?.Invoke(baseStep);
            if (TaskManager.HasInstance)
                TaskManager.Instance.TriggerStepStarted(baseStep);
        }

        public override void NotifyStepCompleted(BaseStep baseStep)
        {
            StepCompleted?.Invoke(baseStep);
            if (TaskManager.HasInstance)
                TaskManager.Instance.TriggerStepCompleted(baseStep);
        }

        private void OnValidate()
        {
            if (TagContainer == null)
                TagContainer = new TagContainer();
            TagContainer.RemoveDoubles();
        }
    }
}