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
        public string CurrentStepId => ActiveStep != null && ActiveStep.ID != null ? ActiveStep.ID.ToString() : string.Empty;

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public TaskState State { get; protected set; }
        
        public CompletionState CompletionState { get; protected set; }

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
        
        public void Initialize()
        {
            if (_initialized)
                return;

            OnPreInitialize();
            
            foreach (var step in GetStepNodes())
                step.BindContainer(this);

            foreach (var step in GetStepNodes())
                step.Initialize();

            OnInitialize();
            
            State = TaskState.Initialized;
            _initialized = true;
        }
        
        protected virtual void OnPreInitialize()
        {
            
        }

        protected virtual void OnInitialize()
        {
            
        }

        private void OnDisable()
        {
            // task need to clean up itself (mostly for conditions)
            // if tasks need to be disabled and preserve their state this call can be moved to 'OnDestroy'
            StopTask();
        }

        public void Terminate()
        {
            if (!_initialized)
                return;
            
            foreach (var step in GetStepNodes())
                step.UnbindContainer();
            _initialized = false;
        }

        protected virtual void OnDestroy()
        {
            Terminate();
        }

        public bool StartTask()
        {
            if (State != TaskState.Initialized)
                return false;

            if (StartStep == null)
            {
                PLog.Error<MagnusLogger>($"No StartStep defined for task '{this.name}', cannot start...");
                return false;
            }

            State = TaskState.Running;
            

            OnStart();
            
            TaskStarted?.Invoke();
            return true;
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

            foreach (var step in GetStepNodes())
                step.Terminate();

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

            if (ActiveStep == null || ActiveStep.State == ProcessState.Finished)
            {
                // If task completed
                if (ActiveStep != null && !ActiveStep.HasNextStep())
                {
                    TaskCompleted?.Invoke();
                    OnCompleted(ActiveStep.CompletionState == CompletionState.Failure);

                    State = TaskState.Finished;
                    CompletionState = ActiveStep.CompletionState == CompletionState.Failure
                        ? Tasks.CompletionState.Failure
                        : CompletionState.Success;
                    return;
                }
                else
                    TryHandleStepAdvancement();
            }

            if (ActiveStep != null)
                ActiveStep.HandleUpdate();
        }

        private void TryHandleStepAdvancement()
        {
            if (ActiveStep == null)
            {
                ActiveStep = StartStep;
                if (ActiveStep == null)
                {
                    PLog.Trace<MagnusLogger>($"CurrentStep is null for Task \"{this.name}\".");
                    return;
                }
                ActiveStep.StartStep();
            }
            else if (ActiveStep != null && ActiveStep.State == ProcessState.Finished)
            {
                ActiveStep = ActiveStep.GetNextStep();
                PLog.Info<MagnusLogger>($"Moving on to next step ({ActiveStep.ID})....");

                if (ActiveStep == null)
                {
                    PLog.Trace<MagnusLogger>($"CurrentStep is null for Task \"{this.name}\".");
                    return;
                }
                
                ActiveStep.StartStep();
            }
        }
        
        protected virtual void OnCompleted(bool failed = false)
        {
            
        }
        
        public void ResetTask()
        {
            State = TaskState.Initialized;
            
            // Optimized reset, reset only what has ran
            // for (var i = CurrentStepId; i >= 0; --i)
            //     Steps[i].ResetStep();
            foreach (var step in GetStepNodes())
                step.ResetStep();
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

        public BaseStep FindStep(SerializableGuid stepIDToSkipTo)
        {
            throw new System.NotImplementedException();
        }
    }
}