using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public abstract class BaseTask : MonoBehaviour, ITask
    {
        [PropertyOrder(-1)]
        public TagContainer TagContainer = new TagContainer();
        
        public abstract IReadOnlyList<BaseStep> Steps { get; }

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public BaseStep ActiveStep { get; private set; }
        
        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public int CurrentStepId { get; protected set; }
        
        public bool IsIdle { get; private set; }

        public delegate void TaskEvent();
        
        public event TaskEvent TaskStarted;
        public event TaskEvent TaskStopped;
        public event TaskEvent TaskCompleted;

        public delegate void StepEvent(BaseStep step);

        public event StepEvent StepStarted;
        public event StepEvent StepCompleted;
        
        public delegate IEnumerator AwaitStepEvent(BaseStep step);

        public event AwaitStepEvent PreStartStep;
        public event AwaitStepEvent PreStopStep;

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public bool IsActive { get; protected set; }
        
        protected bool _initialized;
        
        protected virtual void Awake()
        {
            CurrentStepId = -1;

            foreach (var step in Steps)
                step.Initialize(this);

            IsIdle = true;

            OnAwake();

            _initialized = true;
        }

        protected virtual void Start()
        {
            foreach (var step in Steps)
                step.Initialize();
        }

        protected virtual void OnDestroy()
        {
            if (Steps != null)
                foreach (var step in Steps)
                    step.CleanUp();
        }

        protected virtual void OnAwake()
        {
            
        }

        private void OnDisable()
        {
            // task need to clean up itself (mostly for conditions)
            // if tasks need to be disabled and preserve their state this call can be moved to 'OnDestroy'
            StopTask();
        }

        protected virtual void Update()
        {
            if (!IsActive)
                return;

            if (CheckTaskCompleted())
                return;
            
            var currentStep = GetCurrentStep();
            
            if (currentStep == null)
            {
                ActiveStep = null;
                PLog.Trace<MagnusLogger>($"CurrentStep is null for Task \"{this.name}\".");
                return;
            }

            if (IsIdle)
            {
                // Step stop is already over, move to the next
                if (!ActiveStep && currentStep.IsStarted)
                {
                    ++CurrentStepId;
                    PLog.Info<MagnusLogger>($"Moving on to next step ({CurrentStepId})....");
                }
                // if a new step; boot it up
                else if (ActiveStep != currentStep)
                {
                    ActiveStep = currentStep;
                    HandleStepStart();
                } 
                
                // Actually start it once ready
                else if (ActiveStep)
                {
                    if (!ActiveStep.IsStarted)
                        ActiveStep.StartStep();
                    
                    // Otherwise shut it down; step is done
                    else if (!ActiveStep.IsActive)
                    {
                        HandleStepStop();
                        ActiveStep = null;
                    }
                }
            } 
            
            if (ActiveStep != null)
                ActiveStep.CheckProgress();
        }

        public void StartTask()
        {
            if (IsActive)
                return;

            IsActive = true;
            
            if (Steps == null)
                PLog.Error<MagnusLogger>($"Steps for task '{this.name}' are null");

            CurrentStepId = 0;

            TaskStarted?.Invoke();
            OnStart();
        }

        protected virtual void OnStart()
        {
            
        }
        
        public void ResetTask()
        {
            IsActive = false;

            for (var i = CurrentStepId; i >= 0; --i)
                Steps[i].ResetStep();
        }

        public void StopTask()
        {
            if (!IsActive)
                return;

            IsActive = false;

            if (ActiveStep)
                PLog.Info<MagnusLogger>($"[BasicTask::StopTask] Step {CurrentStepId + 1}: {ActiveStep.name}", ActiveStep);

            foreach (var step in Steps)
            {
                step.PostStepCompleted -= HandleStepCompleted;
                step.Terminate();
            }

            CurrentStepId = -1;
            ActiveStep = null;
            
            TaskStopped?.Invoke();
        }

        protected virtual void OnStop()
        {
        }

        private BaseStep GetCurrentStep()
        {
            if (!IsActive || CurrentStepId == -1)
                return null;

            BaseStep currentStep = Steps.GetAtIndex(CurrentStepId);

            // Skip disabled steps
            while (currentStep != null && !currentStep.isActiveAndEnabled)
                currentStep = Steps.GetAtIndex(++CurrentStepId);
            
            return currentStep;
        }

        public virtual void HandleStepCompleted()
        {
            IsIdle = true;
        }
        
        private void HandleStepStart()
        {
            PLog.TraceDetailed<MagnusLogger>("Triggered");
            
            IsIdle = false;

            ActiveStep.PostStepCompleted -= HandleStepCompleted;
            ActiveStep.PostStepCompleted += HandleStepCompleted;

            StepStarted?.Invoke(ActiveStep);

            if (!IsActive || CurrentStepId == -1)
                return;

            var go = ActiveStep.gameObject;
            PLog.Info<MagnusLogger>($"[BasicTask::OnStepStarted] Moving on to step {CurrentStepId + 1}: {go.name}", associatedObject: go);

            var coroutine = ManagedCoroutine.Begin(PreStartStepHandler(ActiveStep));
            // coroutine.OnFinished += x =>
            // {
            //     // Might be null when the coroutine is terminated due to end of game
            //     if (ActiveStep) ActiveStep.StartStep();
            // };
        }

        private bool CheckTaskCompleted()
        {
            if (CurrentStepId < Steps.Count)
                return false;
            
            TaskCompleted?.Invoke();
            OnCompleted();
            
            IsActive = false;
            
            return true;
        }

        protected virtual void OnCompleted()
        {
            
        }

        private void HandleStepStop()
        {
            PLog.TraceDetailed<MagnusLogger>("Triggered");
            
            IsIdle = false;

            StepCompleted?.Invoke(ActiveStep);

            if (!IsActive || CurrentStepId == -1) // Events can stop the current task
                return;

            var coroutine = ManagedCoroutine.Begin(PreStopStepHandler(ActiveStep));
            // coroutine.OnFinished += x =>
            // {
            //     ++CurrentStepId;
            //     PLog.Info<VOLTLogger>($"Moving on to next step ({CurrentStepId})....");
            // };
        }

        private IEnumerator PreStartStepHandler(BaseStep step)
        {
            PLog.TraceDetailed<MagnusLogger>("Triggered");
            
            if (PreStartStep == null)
            {
                IsIdle = true;
                yield break;
            }
            
            foreach (AwaitStepEvent del in PreStartStep.GetInvocationList())
                yield return del.Invoke(step);

            IsIdle = true;
        }
        
        private IEnumerator PreStopStepHandler(BaseStep step)
        {
            PLog.TraceDetailed<MagnusLogger>("Triggered");
            
            if (PreStopStep == null)
            {
                IsIdle = true;
                yield break;
            }
            
            foreach (AwaitStepEvent del in PreStopStep.GetInvocationList())
                yield return del.Invoke(step);
            
            IsIdle = true;
        }

        private void OnValidate()
        {
            if (TagContainer == null)
                TagContainer = new TagContainer();
            TagContainer.RemoveDoubles();
        }

        public int GetStepIndex(BaseStep startStep)
        {
            return Steps.IndexOf(startStep);
        }
        
        public bool HasPassed(BaseStep step)
        {
            var stepI = Steps.IndexOf(step);
            return CurrentStepId >= stepI;
        }
    }
}