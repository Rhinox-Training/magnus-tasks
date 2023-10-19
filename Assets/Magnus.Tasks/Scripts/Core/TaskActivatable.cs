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
    public class TaskActivatable : MonoBehaviour
    {
        [Serializable]
        public class ProgressEvent : UnityEvent<BaseStep, float>
        {
        }

        [ToggleButtonOpposite("Data Task"), HideInPlayMode]
        public bool SceneTask;

        [ShowIf(nameof(SceneTask))] public BaseTask Task;

        [HideIf(nameof(SceneTask)), TaskSelector]
        public int DataTask = -1;

        [ToggleButtonOpposite("Region"), HideIf(nameof(HasRegionData))]
        public bool Always;

        protected bool ShowSceneTaskRegion => SceneTask && !Always;
        protected bool ShowDataTaskRegion => !SceneTask && !Always;

        protected bool HasRegionData => !Always &&
                                        ((!SceneTask && (!StartStepIndex.IsNullOrEmpty() ||
                                                         !EndStepIndex.IsNullOrEmpty())) ||
                                         (SceneTask && (StartStep != null || EndStep != null)));

        // ================================================================================================================
        // Regular Task Data
        [ShowIf(nameof(ShowSceneTaskRegion))] public BaseStep StartStep;
        [ShowIf(nameof(ShowSceneTaskRegion))] public BaseStep EndStep;

        // ================================================================================================================
        // Data Task Data
        [ShowIf(nameof(ShowDataTaskRegion)), StepSelector(nameof(DataTask))]
        public SerializableGuid StartStepIndex;

        [ShowIf(nameof(ShowDataTaskRegion)), StepSelector(nameof(DataTask))]
        public SerializableGuid EndStepIndex;

        // ================================================================================================================
        // Other
        public ProgressEvent OnStepChainProgress;

        private List<BaseStep> _validStartSteps;
        private List<BaseStep> _validEndSteps;

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode]
        private BaseStep _currentStart;

        [FoldoutContainer("Debug"), ShowReadOnlyInPlayMode]
        private BaseStep _currentEnd;

        protected StepContainer _activeTask;

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
            _validStartSteps = new List<BaseStep>();
            _validEndSteps = new List<BaseStep>();
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
            if (activeStartStep)
            {
                RegisterStartAndFindEndStep(activeStartStep);

                Activate();
            }
        }

        private BaseStep FindStartStepForConfiguration(BaseTask[] tasks)
        {
            BaseStep activeStartStep = null;
            if (!SceneTask)
            {
                var dataTasks = tasks.SelectMany(x => x.GetComponentsInChildren<IDataTaskIdentifier>());
                foreach (var dataTask in dataTasks)
                {
                    var startedStep = ConfigureForTask(dataTask);
                    if (startedStep != null && activeStartStep == null)
                    {
                        activeStartStep = startedStep;
                        break;
                    }
                }
            }
            else
            {
                foreach (var task in tasks)
                {
                    var startedStep = ConfigureForTask(task);
                    if (startedStep != null && activeStartStep == null)
                        activeStartStep = startedStep;
                }
            }

            return activeStartStep;
        }

        private void OnTaskCompleted(BaseTask task)
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

        protected virtual BaseStep ConfigureForTask(BaseTask task)
        {
            // If it's not the right task; return
            if (task != Task) return null;

            BaseStep startStep;

            // If always; just ignore the data and choose first & last steps
            if (Always)
            {
                startStep = task.GetStepNodes().First();
                // Add the steps to the valid ones
                _validStartSteps.Add(startStep);
                _validEndSteps.Add(task.GetStepNodes().Last());

                return task.State == TaskState.Running ? startStep : null;
            }

            startStep = StartStep;
            var endStep = EndStep;

            if (startStep == null) startStep = task.GetStepNodes().First();
            if (endStep == null) endStep = task.GetStepNodes().Last();

            _validStartSteps.Add(startStep);
            _validEndSteps.Add(endStep);

            return task.State == TaskState.Running ? startStep : null;
        }

        protected virtual BaseStep ConfigureForTask(IDataTaskIdentifier task)
        {
            // If it's not the right task; return
            var dataTask = task.GetDataTask();
            if (dataTask.ID != DataTask) 
                return null;

            BaseStep startStep;

            // If always; just ignore the data and choose first & last steps
            if (Always)
            {
                startStep = task.Steps.First();
                // Add the steps to the valid ones
                _validStartSteps.Add(startStep);
                _validEndSteps.Add(task.Steps.Last());

                return task.IsActive ? startStep : null;
            }

            /*// Find the indices of the steps in the data
            int startStepI = -1, endStepI = -1;
            for (var i = 0; i < dataTask.Steps.Count; i++)
            {
                var step = dataTask.Steps[i];
                if (step.ID == StartStepIndex)
                    startStepI = i;
    
                if (step.ID == EndStepIndex)
                {
                    endStepI = i;
                    break; // EndStep found so breaking is fine
                }
            }
    
            // Check if data was actually found
            if (!StartStepIndex.IsNullOrEmpty() && startStepI < 0 || !EndStepIndex.IsNullOrEmpty() && endStepI < 0)
                return null;
    
            // Add the steps to the valid ones
            startStep = task.Steps.HasIndex(startStepI) ? task.Steps[startStepI] : task.Steps.First();
            var endStep = task.Steps.HasIndex(endStepI) ? task.Steps[endStepI] : task.Steps.Last();
            */
            startStep = task.Steps.FirstOrDefault(x => x.ID == StartStepIndex) ?? task.Steps.First();
            var endStep = task.Steps.FirstOrDefault(x => x.ID == EndStepIndex) ?? task.Steps.Last();

            _validStartSteps.Add(startStep);
            _validEndSteps.Add(endStep);

            if (task.IsActive && startStep.State == ProcessState.Finished && !endStep.State.IsFinishingOrFinished())
                return startStep;
            return null;
        }

        protected virtual void OnStepStarted(BaseStep step)
        {
            if (IsActive)
                return;
            
            if (!ShouldActivate(step)) 
                return;

            Activate();

            OnStepChainProgress?.Invoke(step, 0f);
        }

        protected virtual void OnStepCompleted(BaseStep step)
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

        private bool ShouldActivate(BaseStep step)
        {
            if (!_validStartSteps.Contains(step))
                return false;

            RegisterStartAndFindEndStep(step);

            if (!_currentEnd)
                PLog.Error<MagnusLogger>("Could not resolve an End for TaskActivatable!", this);
            return true;
        }

        private bool ShouldDeactivate(BaseStep step)
        {
            return _currentEnd == step;
        }

        private void RegisterStartAndFindEndStep(BaseStep startStep)
        {
            _currentStart = startStep;
            _currentEnd = null;

            // Find end step
            foreach (var potentialEndStep in startStep.Container.GetStepNodes())
            {
                if (!_validEndSteps.Contains(potentialEndStep)) 
                    continue;

                _currentEnd = potentialEndStep;
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