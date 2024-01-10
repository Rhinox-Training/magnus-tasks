using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public abstract class BaseStepRegion
    {
        [ToggleButtonOpposite("Region")]
        public bool Always;
        
        public SerializableGuid StartStepIndex { get; }
        public SerializableGuid EndStepIndex { get; }
        public abstract bool IsForTask(ITaskObjectState task);
    }

    [Serializable]
    public class SceneTaskRegion : BaseStepRegion
    {
        public TaskBehaviour Task;
        [HideIf(nameof(Always))]
        public BaseStepState StartStep;
        [HideIf(nameof(Always))]
        public BaseStepState EndStep;

        public SerializableGuid StartStepIndex => StartStep != null ? StartStep.ID : SerializableGuid.Empty;
        public SerializableGuid EndStepIndex => EndStep != null ? EndStep.ID : SerializableGuid.Empty;
        public override bool IsForTask(ITaskObjectState task)
        {
            return ReferenceEquals(task, Task);
        }
    }

    [Serializable]
    public class DataTaskRegion : BaseStepRegion
    {
        [TaskSelector]
        public int DataTask = -1;
        
        [SerializeField, StepSelector(nameof(DataTask)), HideIf(nameof(Always))]
        public SerializableGuid _startStepIndex;

        [SerializeField, StepSelector(nameof(DataTask)), HideIf(nameof(Always))]
        private SerializableGuid _endStepIndex;

        public SerializableGuid StartStepIndex => _startStepIndex;
        public SerializableGuid EndStepIndex => _endStepIndex;
        public override bool IsForTask(ITaskObjectState task)
        {
            if (task is DataTask dataTask && dataTask == task)
                return true;

            if (task is TaskBehaviour taskBehaviour)
            {
                foreach (var subtask in
                         taskBehaviour.GetComponentsInChildren<SubDataTask>()) // TODO: how not to depend on GetComponent here?
                {
                    if (subtask.TaskId == DataTask)
                        return true;
                }
            }

            return false;
        }
    }
    
    [SmartFallbackDrawn]
    public class TaskActivatable : MonoBehaviour
    {
        [Serializable]
        public class ProgressEvent : UnityEvent<BaseStepState, float>
        {
        }

        [SerializeReference, HideLabel]
        public BaseStepRegion StepRegion;

        // ================================================================================================================
        // Other
        public ProgressEvent OnStepChainProgress;

        private List<StepData> _validStartSteps;
        private List<StepData> _validEndSteps;

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode]
        private BaseStepState _currentStart;

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode]
        private BaseStepState _currentEnd;

        protected ITaskObjectState _activeTask;

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode]
        [HorizontalGroup("Debug/State"), ToggleLeft]
        public bool IsActive { get; protected set; }

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode] 
        [HorizontalGroup("Debug/State"), ToggleLeft]
        protected bool _isInitialised;

        public delegate void ActivatableStateAction(TaskActivatable activatable, bool state);

        public event ActivatableStateAction StateChanged;

        // ================================================================================================================
        // METHODS
        private void Awake()
        {
            // Required due to datatasks only spawning in later
            SceneReadyHandler.YieldToggleControl(this);

            // lists are needed as we can have the same datatask multiple times
            _validStartSteps = new List<StepData>();
            _validEndSteps = new List<StepData>();
        }

        private void Start()
        {
            if (!TaskManager.HasInstance)
                return;

            TaskManager.Instance.StepStarted += OnStepStarted;
            TaskManager.Instance.StepCompleted += OnStepCompleted;
            TaskManager.Instance.TaskCompleted += OnTaskCompleted;

            var tasks = TaskManager.Instance.GetTasks();
            var activeStartStep = FindStartStepForConfiguration(tasks);

            _isInitialised = true;

            // Always deactivate on start
            Deactivate();

            // Reactive in case we have an already active step (would not pass by StartStep and activate then)
            if (activeStartStep != null)
            {
                RegisterStartAndFindEndStep(activeStartStep);

                Activate();
            }
        }

        private BaseStepState FindStartStepForConfiguration(ICollection<TaskObject> tasks)
        {
            BaseStepState activeStartStep = null;
            foreach (var task in tasks)
            {
                var startedStep = ConfigureForTask(task, StepRegion);
                if (startedStep != null)
                {
                    activeStartStep = startedStep;
                    break;
                }
            }

            return activeStartStep;
        }

        private BaseStepState ConfigureForTask(TaskObject task, BaseStepRegion stepRegion)
        {
            // if (!stepRegion.IsForTask(task))
            //     return null;

            return null;
            // if (stepRegion.Always)
            // {
            //     var firstStep = task.EnumerateStepNodes().First();
            //     // Add the steps to the valid ones
            //     _validStartSteps.Add(firstStep);
            //     _validEndSteps.Add(task.EnumerateStepNodes().Last());
            //
            //     return task.State == TaskState.Running ? firstStep : null;
            // }
            //
            // var steps = task.EnumerateStepNodes();
            // var startStep = steps.FirstOrDefault(x => stepRegion.StartStepIndex == x.ID);
            // var endStep = steps.FirstOrDefault(x => stepRegion.EndStepIndex == x.ID);
            //
            // if (startStep == null) 
            //     startStep = task.EnumerateStepNodes().First();
            // if (endStep == null) 
            //     endStep = task.EnumerateStepNodes().Last();
            //
            // _validStartSteps.Add(startStep);
            // _validEndSteps.Add(endStep);
            //
            // return task.State == TaskState.Running ? startStep : null;
        }

        private void OnTaskCompleted(ITaskObjectState task)
        {
            if (ReferenceEquals(task, _activeTask))
                Deactivate();
        }

        private void OnDestroy()
        {
            SceneReadyHandler.RevertToggleControl(this);

            if (TaskManager.HasInstance)
            {
                TaskManager.Instance.StepStarted -= OnStepStarted;
                TaskManager.Instance.StepCompleted -= OnStepCompleted;
            }
        }
        
        protected virtual void OnStepStarted(BaseStepState step)
        {
            if (IsActive)
                return;
            
            if (!ShouldActivate(step)) 
                return;

            Activate();

            OnStepChainProgress?.Invoke(step, 0f);
        }

        protected virtual void OnStepCompleted(BaseStepState step)
        {
            if (!IsActive)
                return;

            float progress = 0f;
            if (ShouldDeactivate(step))
            {
                Deactivate();
                progress = 1f;
            }
            else
            {
                // All of these should be present & not null, if not there is an issue
                int currentDistance = StepPathPlanner.CalculateDistance(_currentStart, step);
                int totalDistance = StepPathPlanner.CalculateDistance(_currentStart, _currentEnd);
                
                progress = currentDistance / (float) totalDistance;
            }

            OnStepChainProgress?.Invoke(step, progress);
        }

        private bool ShouldActivate(BaseStepState step)
        {
            if (!_validStartSteps.Contains(step.Data))
                return false;

            RegisterStartAndFindEndStep(step);

            if (_currentEnd == null)
                PLog.Error<MagnusLogger>("Could not resolve an End for TaskActivatable!", this);
            return true;
        }

        private bool ShouldDeactivate(BaseStepState step)
        {
            return _currentEnd == step;
        }

        private void RegisterStartAndFindEndStep(BaseStepState startStep)
        {
            _currentStart = startStep;
            _currentEnd = null;

            // Find end step
            foreach (var potentialEndStep in startStep.Container.EnumerateStepNodes())
            {
                if (!_validEndSteps.Contains(potentialEndStep)) 
                    continue;

                _currentEnd.Data = potentialEndStep;
                break;
            }
        }


        protected virtual void Activate()
        {
            _activeTask = _currentStart.Container;
            IsActive = true;
            TriggerStateChanged();
        }

        protected virtual void Deactivate()
        {
            _activeTask = null;
            IsActive = false;
            TriggerStateChanged();
        }

        protected virtual void TriggerStateChanged()
        {
            gameObject.SetActive(IsActive);

            StateChanged?.Invoke(this, IsActive);
        }
    }
}