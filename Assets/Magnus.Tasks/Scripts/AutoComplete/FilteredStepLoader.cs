using System.Collections.Generic;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using UnityEngine;


namespace Rhinox.Magnus.Tasks
{
    [RequireComponent(typeof(TaskBehaviour))]
    public class FilteredStepLoader : MonoBehaviour
    {
        private TaskBehaviour _task;

        public bool SkipTags;

        public List<string> Tags;
        private bool _checkAutoComplete = false;

        private void Awake()
        {
            _checkAutoComplete = false;

            _task = GetComponent<TaskBehaviour>();
            _task.StepStarted += OnStepStarted;
        }

        private void OnStepStarted(BaseStepState step)
        {
            _checkAutoComplete = true;
        }

        private void AutoCompleteStep()
        {
            var step = _task.ActiveStep;
            var containsTag = step.TagContainer.HasAnyTag(Tags);

            // return if it skips contained tags & it does not contain it
            // OR when it contains the tag and preserves those tags
            if (SkipTags && !containsTag || !containsTag) return;

            PLog.TraceDetailed<MagnusLogger>($"Triggering autocomplete.");
            AutoCompletor.Instance.Autocomplete();
        }

        private void Update()
        {
            if (_checkAutoComplete && AutoCompletor.HasInstance)
            {
                AutoCompleteStep();
                _checkAutoComplete = false;
            }
        }
    }
}