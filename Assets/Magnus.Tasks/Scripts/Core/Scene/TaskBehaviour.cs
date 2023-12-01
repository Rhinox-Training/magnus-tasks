using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
    public enum PopulationMode
    {
        None,
        LoadStepsFromChildren,
        LinearAssemblerFromChildren
    }
    
    public abstract class TaskBehaviour : MonoBehaviour
    {
        public TaskObject TaskData;

        public ITaskState TaskState => _taskState;
        public TaskState State => TaskState.State;

        private TaskStateObject _taskState;

        public bool StartTask(ITaskManager taskManager)
        {
            if (TaskData == null)
            {
                PLog.Warn<MagnusLogger>($"Cannot start task, no TaskData configured on '{this.name}'...");
                return false;
            }

            if (taskManager == null)
            {
                PLog.Error<MagnusLogger>($"Cannot start task without a TaskManager, argument was null...");
                return false;
            }
            
            _taskState = new TaskStateObject(TaskData);
            _taskState.Initialize(taskManager);

            return _taskState.StartTask();
        }

        protected virtual void OnDestroy()
        {
            if (_taskState != null)
            {
                _taskState.StopTask();
                _taskState = null;
            }
        }
    }
}