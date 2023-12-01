using System.Collections.Generic;
using System.Linq;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    /// <summary>
    /// Script that is placed next to the TaskManager.
    /// When active, it will whitelist or blacklist (depending on SkipTags) the given tags
    /// </summary>
    [RequireComponent(typeof(TaskManager))]
    public class FilteredTaskLoader : MonoBehaviour
    {
        public List<string> Tags;
        public bool SkipTags;

        private TaskManager _taskManager;
        private bool _checkStep;

        private void Awake()
        {
            _taskManager = GetComponent<TaskManager>();
            _taskManager.TaskStarted += OnTaskStarted;
        }

        private void OnDestroy()
        {
            _taskManager.TaskStarted -= OnTaskStarted;
        }

        private void OnTaskStarted(ITaskState task)
        {
            if (!AutoCompletor.HasInstance)
            {
                PLog.Error<MagnusLogger>("Filtering disabled, no AutoCompletor in the scene");
                return;
            }

            var containsTag = task.TagContainer.HasAnyTag(Tags);

            // return if it skips contained tags & it does not contain it
            // OR when it contains the tag and preserves those tags
            if (SkipTags && !containsTag || !containsTag) 
                return;

            // TODO what about other types of steps?
            foreach (var step in task.EnumerateStepNodes().OfType<ConditionStepState>()) // TODO: this will break
            {
                PLog.TraceDetailed<MagnusLogger>($"Enqueuing Autocomplete for step '{step}'");
                AutoCompletor.Instance.Autocomplete(step);
            }
        }

        private void Update()
        {
            if (_checkStep && AutoCompletor.HasInstance)
            {

            }
        }
    }
}
