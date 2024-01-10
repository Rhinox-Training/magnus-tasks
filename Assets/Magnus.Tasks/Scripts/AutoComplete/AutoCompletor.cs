using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [ExecuteBefore(typeof(TaskManager)) ]
    public class AutoCompletor : Singleton<AutoCompletor>
    {
        [SerializeField, SerializeReference]
        private AutocompleteBot _autocompleteBot;
        public AutocompleteBot AutocompleteBot => _autocompleteBot;

        public bool UseKeyboardBind = false;

        [ShowReadOnly]
        private Queue<AutocompleteAction> _queue = new Queue<AutocompleteAction>();
        private ConditionStepState _conditionsStep;
        private int _atStep = -1;

        [ShowReadOnly]
        public bool Running => _runningAutocompleteAction != null;
        private AutocompleteAction _runningAutocompleteAction;

        public bool IsIdle => !Running && _queue.Count == 0;
        public bool CanRun => _conditionsStep != null && _conditionsStep.State != ProcessState.Finished;

        public delegate void StepAction(ConditionStepState step);
        public event StepAction BeforeAutocomplete;

        private void Start()
        {
            if (_autocompleteBot == null)
            {
                PLog.Error<MagnusLogger>("No bot selected, autocompletor disabled");
                return;
            }
            
            _autocompleteBot.Initialize();
            
            _atStep = -1;
            if (TaskManager.HasInstance)
            {
                TaskManager.Instance.StepStarted += OnStepStarted;
                TaskManager.Instance.StepCompleted += OnStepCompleted;
            }
        }

        protected override void OnDestroy()
        {
            if (TaskManager.HasInstance)
            {
                TaskManager.Instance.StepStarted -= OnStepStarted;
                TaskManager.Instance.StepCompleted -= OnStepCompleted;
            }
            
            base.OnDestroy();
        }

        private void Update()
        {
            // TODO: why is this needed
            if (Running && _runningAutocompleteAction.Condition.IsMet)
                _runningAutocompleteAction = null;
            
            if (UseKeyboardBind && Input.GetKeyDown(KeyCode.C) && !Running)
            {
                PLog.TraceDetailed<MagnusLogger>($"Received keyboard bind autocomplete");
                Autocomplete();
            }
        }

        public void SetAutocompleteBot(AutocompleteBot bot)
        {
            if (bot == null)
                return;
            _autocompleteBot = bot;
            _autocompleteBot.Initialize();
        }

        private void RunNextTask()
        {
            if (_queue.Count == 0) return;

            _runningAutocompleteAction = _queue.Dequeue();
            _runningAutocompleteAction.Trigger(OnTaskFinished);
        }

        private void OnTaskFinished(bool manual)
        {
            _runningAutocompleteAction = null;
            RunNextTask();
        }

        private void OnStepStarted(BaseStepState step)
        {
            _atStep++;
            var conditionStep = step as ConditionStepState;
            if (conditionStep == null)
                return;

            _conditionsStep = conditionStep;
        }

        private void OnStepCompleted(BaseStepState step)
        {
            _conditionsStep = null;
        }

        [Button("Auto Complete step"), PropertyOrder(3)]
        public void Autocomplete()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                PLog.Warn<MagnusLogger>("Cannot use this button, application needs to be playing.");
                return;
            }

            PLog.Trace<MagnusLogger>($"[AutoCompletor] Received Enqueuing request for AutoCompleting step '{_conditionsStep}'");
            Autocomplete(_conditionsStep);
        }
        
        public void Autocomplete(ConditionStepState step)
        {
            // We want to get this signal even if the step is null; mostly due to audio coming after the step is completed
            BeforeAutocomplete?.Invoke(step);

            if (step == null)
            {
                PLog.Warn<MagnusLogger>("Cannot use AutoComplete, step is null");
                return;
            }
            
            foreach (var condition in step.Conditions)
            {
                if (condition.IsMet)
                    continue;

                bool enqueuedOrRunning = _queue.Any(x => x.Condition == condition) ||
                                         (_runningAutocompleteAction != null && _runningAutocompleteAction.Condition != condition);
                if (!enqueuedOrRunning)
                    _queue.Enqueue(new AutocompleteAction(condition, _autocompleteBot));
            }
            
            if (!Running)
                RunNextTask();
        }
    }
}