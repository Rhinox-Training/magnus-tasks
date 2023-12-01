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
	[SmartFallbackDrawn(false)]
	public class TaskManager : Singleton<TaskManager>, ITaskManager
	{
		[InlineIconButton("Refresh", nameof(RefreshTasks))]
		public bool AutoLoadTasks;

		[SerializeField, HideIf(nameof(AutoLoadTasks)), FormerlySerializedAs("Tasks"), SerializeReference]
		private TaskBehaviour[] _tasks;

		[ShowInInspector, ReadOnly] 
		public TaskBehaviour CurrentTask { get; private set; }

		public bool RunTaskOnStart = true;

		//==============================================================================================================
		// Events
		
		public delegate void TaskEvent(ITaskState task);

		public event TaskEvent TaskSelected;
		public event TaskEvent TaskStarted;
		public event TaskEvent TaskStopped;
		public event TaskEvent TaskCompleted;

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

		private void Start()
		{
			if (AutoLoadTasks)
				RefreshTasks();

			if (RunTaskOnStart)
				StartCurrentTask();
		}

		private void RefreshTasks()
		{
			_tasks = GetComponentsInChildren<TaskBehaviour>();
		}

		public TaskBehaviour[] GetTasks()
		{
			if (AutoLoadTasks)
				return GetComponentsInChildren<TaskBehaviour>();
			return _tasks ?? Array.Empty<TaskBehaviour>();
		}

		[Button(ButtonSizes.Medium)]
		public bool StartCurrentTask()
		{
			if (CurrentTask != null && CurrentTask.State == TaskState.Running)
			{
				PLog.Warn<MagnusLogger>($"Cannot start task, task {CurrentTask.name} already running");
				return false;
			}

			PLog.TraceDetailed<MagnusLogger>("Triggered");

			if (CurrentTask == null)
				CurrentTask = _tasks?.FirstOrDefault();

			TaskSelected?.Invoke(CurrentTask);

			if (CurrentTask == null)
				return false;
			
			CurrentTask.Initialize(this);

			if (!CurrentTask.StartTask())
			{
				PLog.Warn<MagnusLogger>($"Cannot start task {CurrentTask.name}...");
				return false;
			}
			
			TaskStarted?.Invoke(CurrentTask);
			return true;
		}

		[Button(ButtonSizes.Medium)]
		public void StopCurrentTask()
		{
			PLog.Info<MagnusLogger>($"[{nameof(TaskManager)}::{nameof(StopCurrentTask)}] Triggered");

			if (CurrentTask == null)
				return;

			//TaskSelected?.Invoke(null); // TODO: Can this call be fired with null ? (Since we are switching to empty)

			CurrentTask.StopTask();

			CurrentTask = null;
		}

		/// <summary>
		/// Use with caution and only when you know what you're doing
		/// </summary>
		public void ForceStartTask(TaskBehaviour task)
		{
			PLog.Info<MagnusLogger>($"Forcefully starting task '{task}'", associatedObject: task);
			CurrentTask = task;
			StartCurrentTask();
		}

		public void NotifyStepStarted(ITaskState task, BaseStepState step)
		{
			StepStarted?.Invoke(step);
			GlobalStepStarted?.Invoke(this, step);
		}

		public void NotifyStepCompleted(ITaskState task, BaseStepState step)
		{
			StepCompleted?.Invoke(step);
			GlobalStepCompleted?.Invoke(this, step);
		}

		public void NotifyTaskCompleted(ITaskState task, bool hasFailed)
		{
			PLog.Info<MagnusLogger>($"Task ({CurrentTask}) has {(hasFailed ? "failed" : "completed")}.");
			TaskCompleted?.Invoke(task);
		}

		public void NotifyTaskStopped(ITaskState task)
		{
			PLog.Info<MagnusLogger>($"Task ({task}) Stopped.");
			TaskStopped?.Invoke(task);
		}

		public bool LoadTasks(params TaskBehaviour[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return false;
			
			PLog.TraceDetailed<MagnusLogger>($"Loading {tasks.Length} tasks");

			if (CurrentTask != null)
				StopCurrentTask();

			_tasks = tasks;
			CurrentTask = _tasks.FirstOrDefault();

			TaskSelected?.Invoke(CurrentTask);
			return true;
		}

		public bool AppendTasks(params TaskBehaviour[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return false;
			
			PLog.TraceDetailed<MagnusLogger>($"Appending {tasks.Length} tasks to TaskManager with already had {_tasks?.Length ?? 0} active tasks.");

			if (CurrentTask != null)
				StopCurrentTask();

			var list = _tasks?.ToList() ?? new List<TaskBehaviour>();
			bool changed = false;
			foreach (var task in tasks)
			{
				if (list.AddUnique(task))
					changed = true;
			}

			if (!changed)
				return false;
			
			_tasks = list.ToArray();
			if (CurrentTask == null || !_tasks.Contains(CurrentTask))
				CurrentTask = _tasks.FirstOrDefault();

			TaskSelected?.Invoke(CurrentTask);
			return true;
		}

		public void ClearTasks()
		{
			PLog.TraceDetailed<MagnusLogger>($"Clearing tasks from TaskManager which had {_tasks?.Length ?? 0} tasks.");
			if (CurrentTask != null)
				StopCurrentTask();

			_tasks = Array.Empty<TaskBehaviour>();
		}
	}
}
