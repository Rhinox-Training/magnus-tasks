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
            if (_baseTask == null || _baseTask.TaskData == null)
                return;
            
            if (TaskManager.HasInstance)
            {
                TaskManager.Instance.TaskStarted += TriggerTaskStarted;
                TaskManager.Instance.TaskStopped += TriggerTaskStopped;
                TaskManager.Instance.TaskCompleted += TriggerTaskCompleted;
            }
        }

        private void OnDestroy()
        {
            if (TaskManager.HasInstance)
            {
                TaskManager.Instance.TaskStarted -= TriggerTaskStarted;
                TaskManager.Instance.TaskStopped -= TriggerTaskStopped;
                TaskManager.Instance.TaskCompleted -= TriggerTaskCompleted;
            }
        }

        private void TriggerTaskStarted(ITaskObjectState task)
        {
            if (task == null || !task.IsFor(_baseTask.TaskData))
                return;
            OnTaskStarted?.Invoke();
        }

        private void TriggerTaskStopped(ITaskObjectState task)
        {
            if (task == null || !task.IsFor(_baseTask.TaskData))
                return;
            OnTaskStopped?.Invoke();
        }

        private void TriggerTaskCompleted(ITaskObjectState task)
        {
            if (task == null || !task.IsFor(_baseTask.TaskData))
                return;
            OnTaskCompleted?.Invoke();
        }
    }
}