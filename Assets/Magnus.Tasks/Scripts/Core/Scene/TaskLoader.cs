using System.Collections.Generic;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [RequireComponent(typeof(TaskBehaviour))]
    public class TaskLoader : MonoBehaviour
    {
        public void Awake()
        {
            if (!TaskManager.HasInstance)
                return;
            
            foreach (var beh in GetComponents<TaskBehaviour>())
            {
                if (beh == null)
                    continue;

                if (beh.TaskData == null)
                    continue;

                TaskManager.Instance.RegisterTasks(beh.TaskData);
            }
        }
    }
}