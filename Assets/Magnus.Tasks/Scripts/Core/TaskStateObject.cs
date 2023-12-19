using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
    public class TaskStateObject : ITaskObjectState
    {
        public TaskObject TaskData;
        
        [FormerlySerializedAs("TagContainer"), SerializeField] 
        [PropertyOrder(-1)]
        private TagContainer _tagContainer = new TagContainer();
        public ITagContainer TagContainer => _tagContainer;

        public string Name => TaskData.Name;

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public BaseStepState ActiveStepState { get; private set; }

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public string CurrentStepId => ActiveStepState != null && ActiveStepState.ID != null ? ActiveStepState.ID.ToString() : string.Empty;

        [ShowInInspector, ReadOnly, HideInEditorMode]
        [TabGroup("State")]
        public TaskState State { get; protected set; }
        
        public CompletionState CompletionState { get; protected set; }

        // [SerializeField]
        // private StepData _startStep;
        //
        // public BaseStepState StartStep
        // {
        //     get { return _startStep; }
        //     set { _startStep = value; }
        // }

        public bool IsActive => State == TaskState.Running;
        
        protected bool _initialized;
        private ITaskManager _taskManager;
        private List<BaseStepState> _stepStates;

        //==============================================================================================================
        // Events

        public delegate void TaskEvent();
        public delegate void TaskCompletionEvent(bool hasFailed);
        
        public event TaskEvent TaskStarted;
        public event TaskEvent TaskStopped;
        public event TaskCompletionEvent TaskCompleted;

        public delegate void StepEvent(BaseStepState step);

        public event StepEvent StepStarted;
        public event StepEvent StepCompleted;
        
        public event BaseStepState.AwaitStepEvent PreStartStep;
        public event BaseStepState.AwaitStepEvent PreStopStep;

        protected IEnumerable<BaseStepState.AwaitStepEvent> GetPreStartStepHandlers()
        {
            return PreStartStep?.GetInvocationList()?.OfType<BaseStepState.AwaitStepEvent>() ?? Array.Empty<BaseStepState.AwaitStepEvent>();
        }
        
        protected IEnumerable<BaseStepState.AwaitStepEvent> GetPreStopStepHandlers()
        {
            return PreStopStep?.GetInvocationList()?.OfType<BaseStepState.AwaitStepEvent>() ?? Array.Empty<BaseStepState.AwaitStepEvent>();
        }
        
        //==============================================================================================================
        // Methods

        public TaskStateObject(TaskObject taskData)
        {
            TaskData = taskData;
        }
        
        public void Initialize(ITaskManager taskManager)
        {
            if (_initialized)
                return;

            _taskManager = taskManager;
            
            OnPreInitialize();
            
            OnInitialize();
            
            State = TaskState.Initialized;
            _initialized = true;
        }
        
        public void Terminate()
        {
            if (!_initialized)
                return;

            _stepStates = null;
            
            _taskManager = null;
            _initialized = false;
        }


        public StepData StartStep { get; }

        public IEnumerable<StepData> EnumerateStepNodes()
        {
            return TaskData != null ? (IEnumerable<StepData>) TaskData.Steps : Array.Empty<StepData>();
        }
        
        private void BindContainer(BaseStepState step)
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
        
        

        public bool StartTask()
        {
            if (State != TaskState.Initialized)
                return false;

            if (TaskData.StartStep == null)
            {
                PLog.Error<MagnusLogger>($"No StartStep defined for task '{TaskData.Name}', cannot start...");
                return false;
            }

            State = TaskState.Running;
            

            OnStart();
            
            TaskStarted?.Invoke();
            return true;
        }

        public bool IsFor(TaskObject taskData)
        {
            return TaskData == taskData;
        }

        protected virtual void OnStart()
        {
            
        }

        public void StopTask()
        {
            if (State != TaskState.Running && State != TaskState.Paused)
                return;

            State = TaskState.Finished;

            if (ActiveStepState != null)
                PLog.Info<MagnusLogger>($"[BasicTask::StopTask] Step {CurrentStepId + 1}: {ActiveStepState.Data.Name}");

            ActiveStepState = null;

            OnStop();
            
            NotifyTaskStopped();
        }

        protected virtual void OnStop()
        {
        }

        protected BaseStepState BuildStepState(StepData data)
        {
            if (!TaskObjectUtility.TryCreateStepState(data, TaskData.Lookup, out var stepState))
            {
                PLog.Error<MagnusLogger>($"Failed to create stepState for data: {data.Name}");
                return null;
            }
            
            BindContainer(stepState);
            stepState.Initialize();
            return stepState;
        }

        public virtual void Update()
        {
            if (State != TaskState.Running)
                return;

            if (ActiveStepState == null || ActiveStepState.State == ProcessState.Finished)
            {
                // If task completed
                if (ActiveStepState != null && !ActiveStepState.HasNextStep())
                {
                    bool hasFailed = ActiveStepState.CompletionState.HasFailed();
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

            if (ActiveStepState != null)
                ActiveStepState.HandleUpdate();
        }

        public BaseStepState GetStepState(SerializableGuid stepId)
        {
            if (_stepStates == null)
                return null;

            return _stepStates.Find(x => x.Data != null && x.Data.ID == stepId);

        }

        private void TryHandleStepAdvancement()
        {
            if (_stepStates == null)
                _stepStates = new List<BaseStepState>();
            
            if (ActiveStepState == null && _stepStates.Count == 0)
            {
                var stepState = BuildStepState(TaskData.StartStep);
                if (stepState == null)
                {
                    PLog.Trace<MagnusLogger>($"Start step failed to build state for Task '{TaskData.Name}'.");
                    return;
                }
                ActiveStepState = stepState;
                _stepStates.Add(ActiveStepState);
                ActiveStepState.StartStep();
            }
            else if (ActiveStepState != null && ActiveStepState.State == ProcessState.Finished)
            {
                StepData stepData = ActiveStepState.GetNextStep();

                CloseStep(ActiveStepState);
                
                var nextStepState = BuildStepState(stepData);
                if (nextStepState == null)
                {
                    ActiveStepState = null; // Set active step to null, to prevent loops in this logic tree
                    PLog.Debug<MagnusLogger>($"Next proposed active step state is null for Task '{TaskData.Name}'.");
                    return;
                }

                ActiveStepState = nextStepState;
                _stepStates.Add(ActiveStepState);
                PLog.Info<MagnusLogger>($"Moving on to next step ({ActiveStepState.ID})....");
                
                ActiveStepState.StartStep();
            }
        }

        private bool CloseStep(BaseStepState activeStepState)
        {
            if (activeStepState == null)
                return false;
            
            activeStepState.UnbindContainer();
            activeStepState.Terminate();
            return true;
        }

        protected virtual void OnCompleted(bool failed = false)
        {
            
        }
        
        public void ResetTask()
        {
            State = TaskState.Initialized;

            ActiveStepState = null; // TODO: is this all?
            // Optimized reset, reset only what has ran
            // for (var i = CurrentStepId; i >= 0; --i)
            // //     Steps[i].ResetStep();
            // foreach (var step in EnumerateStepNodes())
            //     step.ResetStep();
        }
        
        public void NotifyStepStarted(BaseStepState baseStep)
        {
            StepStarted?.Invoke(baseStep);
            if (_taskManager != null)
                _taskManager.NotifyStepStarted(this, baseStep);
        }

        public void NotifyStepCompleted(BaseStepState baseStep)
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

        public BaseStepState FindStep(SerializableGuid stepIDToSkipTo)
        {
            throw new System.NotImplementedException();
        }
    }
}