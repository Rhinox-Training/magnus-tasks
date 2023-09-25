using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
	[SmartFallbackDrawn(false)]
	public class TaskManager : Singleton<TaskManager>
	{
		[InlineIconButton("Refresh", nameof(RefreshTasks))]
		public bool AutoLoadTasks;

		[SerializeField, HideIf(nameof(AutoLoadTasks)), FormerlySerializedAs("Tasks"), SerializeReference]
		private BaseTask[] _tasks;

		[ShowInInspector, ReadOnly] 
		public BaseTask CurrentTask { get; private set; }

		public bool RunTaskOnStart = true;

		public delegate void TaskEvent(BaseTask task);

		public event TaskEvent TaskSelected;
		public event TaskEvent TaskStarted;
		public event TaskEvent TaskStopped;
		public event TaskEvent TaskCompleted;

		public delegate void StepEvent(BaseStep step);

		public event StepEvent StepStarted;
		public event StepEvent StepCompleted;

		public delegate void GlobalStepEvent(TaskManager sender, BaseStep step);

		public static event GlobalStepEvent GlobalStepStarted;
		public static event GlobalStepEvent GlobalStepCompleted;

		public static event Action GlobalTrainingStarted;
		public static event Action GlobalTrainingEnded;

		public UnityEvent TasksCompleted;

		private void Awake()
		{
			SceneReadyHandler.YieldToggleControl(this);

			GlobalTrainingStarted?.Invoke();
		}

		protected override void OnDestroy()
		{
			GlobalTrainingEnded?.Invoke();

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
			_tasks = GetComponentsInChildren<BaseTask>();
		}

		public BaseTask[] GetTasks()
		{
			if (AutoLoadTasks)
				return GetComponentsInChildren<BaseTask>();
			return _tasks ?? Array.Empty<BaseTask>();
		}

		[Button(ButtonSizes.Medium)]
		public void StartCurrentTask()
		{
			if (CurrentTask != null && CurrentTask.IsActive)
			{
				PLog.Warn<MagnusLogger>($"Cannot start task, task {CurrentTask.name} already running");
				return;
			}

			PLog.TraceDetailed<MagnusLogger>("Triggered");

			if (CurrentTask == null)
				CurrentTask = _tasks?.FirstOrDefault();

			TaskSelected?.Invoke(CurrentTask);

			if (CurrentTask == null) return;

			CurrentTask.StartTask();
			CurrentTask.TaskStopped += OnTaskStopped;
			CurrentTask.TaskCompleted += OnTaskCompleted;

			TaskStarted?.Invoke(CurrentTask);
		}

		[Button(ButtonSizes.Medium)]
		public void StopCurrentTask()
		{
			PLog.Info<MagnusLogger>($"[{nameof(TaskManager)}::{nameof(StopCurrentTask)}] Triggered");

			if (CurrentTask == null)
				return;

			//TaskSelected?.Invoke(null); // TODO: Can this call be fired with null ? (Since we are switching to empty)

			CurrentTask.StopTask();

			CurrentTask.TaskStopped -= OnTaskStopped;
			CurrentTask.TaskCompleted -= OnTaskCompleted;

			CurrentTask = null;
		}

		/// <summary>
		/// Use with caution and only when you know what you're doing
		/// </summary>
		public void ForceStartTask(BaseTask task)
		{
			PLog.Info<MagnusLogger>($"Forcefully starting task '{task}'", associatedObject: task);
			CurrentTask = task;
			StartCurrentTask();
		}

		private void OnTaskStopped()
		{
			PLog.Info<MagnusLogger>($"Task ({CurrentTask}) Stopped.");
			TaskStopped?.Invoke(CurrentTask);
			CurrentTask.TaskStopped -= OnTaskStopped; // TODO: why?
		}

		private void OnTaskCompleted()
		{
			PLog.Info<MagnusLogger>($"Task ({CurrentTask}) Completed.");
			TaskCompleted?.Invoke(CurrentTask);
			CurrentTask.TaskCompleted -= OnTaskCompleted;
			int currTaskIndex = Array.IndexOf(_tasks, CurrentTask);

			// Check if there are any more tasks after this one
			if (currTaskIndex < 0 || currTaskIndex > _tasks.Length - 2)
			{
				TasksCompleted?.Invoke();
				return;
			}

			CurrentTask = _tasks[++currTaskIndex];
			StartCurrentTask();
		}

		internal void TriggerStepStarted(BaseStep step)
		{
			StepStarted?.Invoke(step);
			GlobalStepStarted?.Invoke(this, step);
		}

		internal void TriggerStepCompleted(BaseStep step)
		{
			StepCompleted?.Invoke(step);
			GlobalStepCompleted?.Invoke(this, step);
		}

		public bool LoadTasks(params BaseTask[] tasks)
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

		public bool AppendTasks(params BaseTask[] tasks)
		{
			if (tasks == null || tasks.Length == 0)
				return false;
			
			PLog.TraceDetailed<MagnusLogger>($"Appending {tasks.Length} tasks to TaskManager with already had {_tasks?.Length ?? 0} active tasks.");

			if (CurrentTask != null)
				StopCurrentTask();

			var list = _tasks?.ToList() ?? new List<BaseTask>();
			list.AddRange(tasks);
			
			_tasks = list.ToArray();
			if (CurrentTask == null || !_tasks.Contains(CurrentTask))
				CurrentTask = _tasks.FirstOrDefault();

			TaskSelected?.Invoke(CurrentTask);
			return true;
		}
	}
}
