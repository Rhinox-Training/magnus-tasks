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
    public abstract class TaskBehaviour : MonoBehaviour, ITask
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

        [SerializeField]
        private BaseStep _startStep;

        public BaseStep StartStep
        {
            get { return _startStep; }
            set { _startStep = value; }
        }

        public bool IsActive => State == TaskState.Running;
        
        protected bool _initialized;
        private ITaskManager _taskManager;

        //==============================================================================================================
        // Events

        public delegate void TaskEvent();
        public delegate void TaskCompletionEvent(bool hasFailed);
        
        public event TaskEvent TaskStarted;
        public event TaskEvent TaskStopped;
        public event TaskCompletionEvent TaskCompleted;

        public delegate void StepEvent(BaseStep step);

        public event StepEvent StepStarted;
        public event StepEvent StepCompleted;
        
        public event BaseStep.AwaitStepEvent PreStartStep;
        public event BaseStep.AwaitStepEvent PreStopStep;

        protected IEnumerable<BaseStep.AwaitStepEvent> GetPreStartStepHandlers()
        {
            return PreStartStep?.GetInvocationList()?.OfType<BaseStep.AwaitStepEvent>();
        }
        
        protected IEnumerable<BaseStep.AwaitStepEvent> GetPreStopStepHandlers()
        {
            return PreStopStep?.GetInvocationList()?.OfType<BaseStep.AwaitStepEvent>();
        }
        
        //==============================================================================================================
        // Methods
        
        public void Initialize(ITaskManager taskManager)
        {
            if (_initialized)
                return;

            _taskManager = taskManager;
            
            OnPreInitialize();
            
            foreach (var step in EnumerateStepNodes())
                BindContainer(step);

            foreach (var step in EnumerateStepNodes())
                step.Initialize();

            OnInitialize();
            
            State = TaskState.Initialized;
            _initialized = true;
        }


        public abstract IEnumerable<BaseStep> EnumerateStepNodes();
        private void BindContainer(BaseStep step)
        {
            step.BindContainer(this);
            foreach (var invocation in GetPreStartStepHandlers())
                step.RegisterPreStart(invocation);
            foreach (var invocation in GetPreStopStepHandlers())
                step.RegisterPreStop(invocation);
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
            
            foreach (var step in EnumerateStepNodes())
                step.UnbindContainer();
            
            _taskManager = null;
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

            foreach (var step in EnumerateStepNodes())
                step.Terminate();

            ActiveStep = null;

            OnStop();
            
            NotifyTaskStopped();
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
                    bool hasFailed = ActiveStep.CompletionState.HasFailed();
                    NotifyTaskCompleted(hasFailed);

                    OnCompleted(hasFailed);

                    State = TaskState.Finished;
                    CompletionState = hasFailed
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
            foreach (var step in EnumerateStepNodes())
                step.ResetStep();
        }
        
        public void NotifyStepStarted(BaseStep baseStep)
        {
            StepStarted?.Invoke(baseStep);
            if (_taskManager != null)
                _taskManager.NotifyStepStarted(this, baseStep);
        }

        public void NotifyStepCompleted(BaseStep baseStep)
        {
            StepCompleted?.Invoke(baseStep);
            if (_taskManager != null)
                _taskManager.NotifyStepCompleted(this, baseStep);
        }

        private void NotifyTaskCompleted(bool hasFailed)
        {
            TaskCompleted?.Invoke(hasFailed);
            if (_taskManager != null)
                _taskManager.NotifyTaskCompleted(this, hasFailed);
        }

        private void NotifyTaskStopped()
        {
            TaskStopped?.Invoke();
            if (_taskManager != null)
                _taskManager.NotifyTaskStopped(this);
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