using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    [RequireComponent(typeof(TaskBehaviour))]
    public class TaskUnityEvents : MonoBehaviour
    {
        [TabGroup("Events")] public UnityEvent OnTaskStarted;
        [TabGroup("Events")] public UnityEvent OnTaskStopped;
        [TabGroup("Events")] public UnityEvent OnTaskCompleted;
        
        private TaskBehaviour _baseTask;

        private void Awake()
        {
            _baseTask = GetComponent<TaskBehaviour>();
            _baseTask.TaskStarted += TriggerTaskStarted;
            _baseTask.TaskStopped += TriggerTaskStopped;
            _baseTask.TaskCompleted += TriggerTaskCompleted;
        }

        private void OnDestroy()
        {
            if (_baseTask != null)
            {
                _baseTask.TaskStarted -= TriggerTaskStarted;
                _baseTask.TaskStopped -= TriggerTaskStopped;
                _baseTask.TaskCompleted -= TriggerTaskCompleted;
            }
        }

        private void TriggerTaskStarted()
        {
            OnTaskStarted?.Invoke();
        }

        private void TriggerTaskStopped()
        {
            OnTaskStopped?.Invoke();
        }

        private void TriggerTaskCompleted(bool hasFailed)
        {
            OnTaskCompleted?.Invoke();
        }
    }
}