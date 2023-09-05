using System;
using Rhinox.Lightspeed;
using Rhinox.Magnus.Tasks;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.VOLT.Training
{
	[ExecuteAfter(typeof(BaseTask)), RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
	public abstract class BaseStep : MonoBehaviour, IReadOnlyReferenceResolver, IIdentifiable
	{
		[Title("Base Settings")]
		public TagContainer TagContainer = new TagContainer();

		[LabelWidth(50)]
		public string Title;
		[TextArea(1,3)]
		public string Description;
		
		// Currently only filled in from TaskObject (to keep a reference from where it came)
		// TODO probably don't want it to be a public setter
		public SerializableGuid ID { get; set; }
		
		[PropertySpace, SerializeReference]
		[ListDrawerSettings(Expanded = true), TabGroup("Settings")]
		public IStepTimingEvent[] StepTimingEvents = new IStepTimingEvent[] { };
		
		public string EventsHeader => $"Events ({TotalEventCount})";

		private int TotalEventCount => (StepStarted?.GetPersistentEventCount() ?? 0) + (StepCompleted?.GetPersistentEventCount() ?? 0);

		[TabGroup("$EventsHeader"), PropertyOrder(1000)]
		public UnityEvent StepStarted;
		[TabGroup("$EventsHeader"), PropertyOrder(1000)]
		public UnityEvent StepCompleted;
		
		public BaseTask Task { get; private set; }
		
		public bool IsActive { get; private set; } // Active as in, this is the 'active' step of the Task
		public bool IsStarted { get; private set; } // Started as in, the step is being tracked
		
		protected IReferenceResolver _valueResolver;

		public event Action PostStepCompleted;

		public void Initialize(BaseTask task)
		{
			if (task == Task)
				return;
			Task = task;
			Task.StepStarted += OnStepInitialized;
		}
		
		public void CleanUp()
		{
			if (Task == null)
				return;
			
			Task.StepStarted -= OnStepInitialized;
			Task = null;
		}

		public void SetValueResolver(IReferenceResolver valueReferenceLookup)
		{
			_valueResolver = valueReferenceLookup;
		}

		public bool Resolve(SerializableGuid key, out object value)
		{
			if (_valueResolver != null)
				return _valueResolver.Resolve(key, out value);
			
			value = default;
			return false;
		}
		
		public bool Resolve<T>(SerializableGuid key, out T value)
		{
			if (_valueResolver != null)
				return _valueResolver.Resolve(key, out value);
			
			value = default;
			return false;
		}

		private void OnStepInitialized(BaseStep step)
		{
			if (step == this)
				OnStepInitialized();
		}
		
		protected virtual void OnStepInitialized()
		{
			IsActive = true;
			
			foreach (IStepTimingEvent stepEvent in StepTimingEvents)
				stepEvent.Initialize(this);
			
			TaskManager.Instance.TriggerStepStarted(this);
			StepStarted?.Invoke();
		}

		protected void StopStep()
		{
			if (!IsActive)
				return;
			
			if (TaskManager.HasInstance)
				TaskManager.Instance.TriggerStepCompleted(this);
			
			OnStepCompleted();
			PostStepCompleted?.Invoke();
			
			IsActive = false;
		}

		protected virtual void OnStepCompleted()
		{
			StepCompleted?.Invoke();
		}

		public int GetIndex()
		{
			if (!gameObject.activeSelf || Task == null || Task.Steps == null) return -1;
			
			return Task.Steps.IndexOf(this);
		} 
		
		protected virtual void OnValidate()
		{
			if (TagContainer == null)
				TagContainer = new TagContainer();
			TagContainer.RemoveDoubles();
		}

		public void StartStep()
		{
			IsStarted = true;
			OnStartStep();
		}

		protected abstract void OnStartStep();
		public abstract void ResetStep();
		public abstract void CheckProgress();
		public abstract void CheckStepCompleted();
		public abstract bool IsStepCompleted();
		
		/// <summary>
		/// A Step in initialized when its Task initializes
		/// </summary>
		public abstract void Initialize();
		/// <summary>
		/// A Step is terminated when its Task stops
		/// </summary>
		public abstract void Terminate();
	}
}