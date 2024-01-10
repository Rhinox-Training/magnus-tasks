using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
	public class TaskManager : Singleton<TaskManager>, ITaskManager
	{
		private List<TaskObject> _tasks;
		private Dictionary<TaskObject, ITaskObjectState> _taskState;

		//==============================================================================================================
		// Events
		
		public delegate void TaskStateEvent(ITaskObjectState task);
		public delegate void TaskEvent(TaskObject task);

		public event TaskEvent TaskAdded;
		public event TaskStateEvent TaskStarted;
		public event TaskStateEvent TaskStopped;
		public event TaskStateEvent TaskCompleted;

		public delegate void StepEvent(BaseStepState step);

		public event StepEvent StepStarted;
		public event StepEvent StepCompleted;

		public delegate void GlobalStepEvent(TaskManager sender, BaseStepState step);

		public static event GlobalStepEvent GlobalStepStarted;
		public static event GlobalStepEvent GlobalStepCompleted;
 
		//==============================================================================================================
		// Methods
		
		private void Awake()
		{
			SceneReadyHandler.YieldToggleControl(this);
		}

		protected override void OnDestroy()
		{
			SceneReadyHandler.RevertToggleControl(this);

			base.OnDestroy();
		}

		public IList<TaskObject> GetTasks()
		{
			return (IList<TaskObject>) _tasks ?? Array.Empty<TaskObject>();
		}
		
		/// <summary>
		/// Use with caution and only when you know what you're doing
		/// </summary>
		public bool StartTask(TaskObject taskData, bool registerIfNotListed = false)
		{
			if (taskData == null)
			{
				PLog.Warn<MagnusLogger>($"Cannot start task which is null, skipping...");
			}
			
			if (_tasks == null)
				_tasks = new List<TaskObject>();

			if (!_tasks.Contains(taskData))
			{
				if (registerIfNotListed)
					RegisterTasks(taskData);
				else
				{
					PLog.Error<MagnusLogger>($"Cannot start task {taskData.Name}, not listed in registered tasks...");
					return false;
				}
			}
			
			if (_taskState == null)
				_taskState = new Dictionary<TaskObject, ITaskObjectState>();

			if (_taskState.ContainsKey(taskData))
			{
				var taskState = _taskState[taskData];
				if (taskState.State == TaskState.None || taskState.State == TaskState.Initialized)
				{
					return taskState.StartTask();
				}

				PLog.Warn<MagnusLogger>($"Task '{taskData.Name}' is already in state '{taskState.State}', cannot start task...");
				return false;
			}

			if (!TryStartTask(taskData, out var taskObjectState))
			{
				PLog.Error<MagnusLogger>($"Cannot start task {taskData.Name}...");
				return false;
			}

			_taskState.Add(taskData, taskObjectState);

			return true;
		}

		public bool RegisterTasks(params TaskObject[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return false;
			
			if (_tasks == null)
				_tasks = new List<TaskObject>();
			
			PLog.TraceDetailed<MagnusLogger>($"Appending {tasks.Length} tasks to TaskManager with already had {_tasks.Count} active tasks.");

			bool changed = false;
			foreach (var task in tasks)
			{
				if (_tasks.AddUnique(task))
				{
					TaskAdded?.Invoke(task);
					changed = true;
				}
			}
			return changed;
		}

		public void ClearTasks()
		{
			PLog.TraceDetailed<MagnusLogger>($"Clearing tasks from TaskManager which had {_tasks?.Count ?? 0} tasks.");
			if (_taskState != null)
			{
				foreach (var taskState in _taskState.Values)
				{
					if (taskState == null)
						continue;
					taskState.StopTask();
				}
				_taskState.Clear();
			}

			if (_tasks != null)
				_tasks.Clear();
		}

		public bool CancelTask(TaskObject taskData)
		{
			if (taskData == null)
			{
				// Clear all none tasks
				int removedStates = _taskState != null ? _taskState.RemoveAll(x => x.Key == null) : 0;
				int removed = _tasks != null ? _tasks.RemoveAll(x => x == null) : 0;
				return (removed + removedStates) > 0;
			}

			if (_tasks == null || !_tasks.Contains(taskData))
			{
				PLog.Warn<MagnusLogger>($"Cannot cancel task, task was not managed by TaskManager {this.name}...");
				return false;
			}

			if (_taskState != null && _taskState.ContainsKey(taskData))
			{
				_taskState[taskData].StopTask();
				_taskState.Remove(taskData);
			}
			_tasks.Remove(taskData);
			return true;
		}
		
		private bool TryStartTask(TaskObject taskData, out ITaskObjectState state)
		{
			if (taskData == null)
			{
				PLog.Warn<MagnusLogger>($"Cannot start task, taskData was null...");
				state = null;
				return false;
			}
			
			var tso = new TaskStateObject(taskData);
			tso.Initialize(this);

			if (!tso.StartTask())
			{
				PLog.Error<MagnusLogger>($"Could not start task '{taskData.Name}'...");
				state = null;
				return false;
			}
			
			state = tso;
			return true;
		}

		public void NotifyStepStarted(ITaskObjectState task, BaseStepState step)
		{
			StepStarted?.Invoke(step);
			GlobalStepStarted?.Invoke(this, step);
		}

		public void NotifyStepCompleted(ITaskObjectState task, BaseStepState step)
		{
			StepCompleted?.Invoke(step);
			GlobalStepCompleted?.Invoke(this, step);
		}

		public void NotifyTaskCompleted(ITaskObjectState task, bool hasFailed)
		{
			PLog.Info<MagnusLogger>($"Task ({task.Name}) has {(hasFailed ? "failed" : "completed")}.");
			TaskCompleted?.Invoke(task);
		}

		public void NotifyTaskStopped(ITaskObjectState task)
		{
			PLog.Info<MagnusLogger>($"Task ({task}) Stopped.");
			TaskStopped?.Invoke(task);
		}

		public ITaskObjectState GetTaskState(TaskObject task)
		{
			if (_taskState == null || !_taskState.ContainsKey(task))
				return null;

			return _taskState[task];
		}

		public BaseStepState GetStateForStep(StepData stepData)
		{
			if (stepData == null)
			{
				PLog.Warn<MagnusLogger>($"Cannot get state for null step, returning null...");
				return null;
			}
			
			if (_tasks == null)
			{
				PLog.Warn<MagnusLogger>($"No tasks registered in TaskManager, no state could be found for step {stepData.ID}/'{stepData.Name}'...");
				return null;
			}

			foreach (var task in _tasks)
			{
				if (task == null)
					continue;

				if (!task.HasStep(stepData.ID))
					continue;

				var taskState = GetTaskState(task);
				if (taskState == null)
				{
					PLog.Debug<MagnusLogger>($"No active state available for step {stepData.ID}/'{stepData.Name}' from task {task.Name}...");
					break;
				}

				var stepState = taskState.GetStepState(stepData.ID);
				return stepState;
			}

			// NOTE: nothing found
			return null;
		}

		private void Update()
		{
			if (_taskState != null)
			{
				foreach (var state in _taskState.Values)
				{
					if (state == null)
						continue;
					state.Update();
				}
			}
		}
	}
}
