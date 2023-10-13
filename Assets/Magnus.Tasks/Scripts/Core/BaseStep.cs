using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Rhinox.Utilities.Attributes;
using UnityEngine.Events;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Scripting;

namespace Rhinox.Magnus.Tasks
{
	[ExecuteAfter(typeof(BaseTask)), RefactoringOldNamespace("Rhinox.VOLT.Training", "com.rhinox.volt.training")]
	public abstract class BaseStep : MonoBehaviour, IReadOnlyReferenceResolver, IIdentifiable
	{
		[Title("Info"), VerticalGroup("CoreSettings", -100)]
		public TagContainer TagContainer = new TagContainer();

		[LabelWidth(50), VerticalGroup("CoreSettings", -100)]
		public string Title;
		[TextArea(1,3), VerticalGroup("CoreSettings", -100)]
		public string Description;
		
		// Currently only filled in from TaskObject (to keep a reference from where it came)
		// TODO probably don't want it to be a public setter
		public SerializableGuid ID { get; set; }

		public StepContainer Container { get; private set; }
		
		[PropertySpace, SerializeReference]
		[ListDrawerSettings(Expanded = true), TabGroup("Settings", order: -100)]
		public IStepTimingEvent[] StepTimingEvents = new IStepTimingEvent[] { };
		
		[ShowInInspector, ReadOnly]
		public ProcessState State { get; private set; }
		[ShowInInspector, ReadOnly]
		public CompletionState CompletionState { get; private set; }
		
		protected IReferenceResolver _valueResolver;
		
		private IEnumerable<StepContainer.AwaitStepEvent> _postStopStepHandlers;
		private IEnumerable<StepContainer.AwaitStepEvent> _preStartStepHandlers;
		
		//==============================================================================================================
		// Events
		[Preserve]
		public string EventsHeader => $"Events ({TotalEventCount})";

		private int TotalEventCount => (StepStarted?.GetPersistentEventCount() ?? 0) + (StepCompleted?.GetPersistentEventCount() ?? 0);

		public event Action StepLoading;
		public event Action StepCleaningUp;
		
		[TabGroup("$EventsHeader"), PropertyOrder(1000)]
		public UnityEvent StepStarted;
		[TabGroup("$EventsHeader"), PropertyOrder(1000)]
		public UnityEvent StepCompleted;
		
		//==============================================================================================================
		// Methods
		
		public void BindContainer(StepContainer container)
		{
			if (container == Container)
				return;
			
			if (Container != null)
				UnbindContainer();
			
			Container = container;
			_preStartStepHandlers = container.GetPreStartStepHandlers();
			_postStopStepHandlers = container.GetPostStopStepHandlers();
			State = ProcessState.None;
			CompletionState = CompletionState.None;
		}

		public void UnbindContainer()
		{
			if (Container == null)
				return;
			
			_postStopStepHandlers = null;
			_preStartStepHandlers = null;
			Container = null;
		}

		/// <summary>
		/// A Step in initialized when its Task initializes
		/// </summary>
		public void Initialize()
		{
			OnInitialize();
			State = ProcessState.Initialized;
			CompletionState = CompletionState.None;
		}

		protected abstract void OnInitialize();

		/// <summary>
		/// A Step is terminated when its Task stops
		/// </summary>
		public void Terminate()
		{
			OnTerminate();
		}

		protected abstract void OnTerminate();
		
		public bool StartStep()
		{
			if (State.HasStarted())
				return false;
			ManagedCoroutine.Begin(StartStepRoutine()); // TODO: What if we stop the task in the same frame
			return true;
		}
		
		private IEnumerator StartStepRoutine()
		{
			State = ProcessState.Loading;
			
			StepLoading?.Invoke();

			yield return null;
			
			foreach (IStepTimingEvent stepEvent in StepTimingEvents)
				stepEvent.Initialize(this);

			if (_preStartStepHandlers != null)
			{
				foreach (var invocation in _preStartStepHandlers)
					yield return invocation.Invoke(this);
			}
			
			State = ProcessState.Running;
			
			OnStepStarted();
			
			TriggerStartedEvents();
		}

		protected abstract void OnStepStarted();

		public abstract void HandleUpdate();

		protected bool SetCompleted(bool failed = false)
		{
			if (State.IsFinishingOrFinished())
				return false;

			ManagedCoroutine.Begin(FinishStepRoutine(failed));
			return true;
		}

		private IEnumerator FinishStepRoutine(bool failed = false)
		{
			State = ProcessState.CleaningUp;
			
			StepCleaningUp?.Invoke();

			yield return null;
			
			foreach (IStepTimingEvent stepEvent in StepTimingEvents)
				stepEvent.Initialize(this);

			if (_postStopStepHandlers != null)
			{
				foreach (var invocation in _postStopStepHandlers)
					yield return invocation.Invoke(this);
			}
			
			State = ProcessState.Finished;
			
			OnStepCompleted();

			TriggerCompletedEvents();
			
			CompletionState = failed ? CompletionState.Failure : CompletionState.Success;
		}

		protected virtual void OnStepCompleted()
		{
		}

		private void TriggerStartedEvents()
		{
			StepStarted?.Invoke();
			Container.NotifyStepStarted(this);
		}

		private void TriggerCompletedEvents()
		{
			StepCompleted?.Invoke();
			Container.NotifyStepCompleted(this);
		}

		public void ResetStep()
		{
			if (State == ProcessState.None)
				return;

			State = ProcessState.Initialized;
			OnResetStep();
		}

		protected virtual void OnResetStep()
		{
		}
		
		//==============================================================================================================
		// Value resolution

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
		
		//==============================================================================================================
		// Unity Editor
		
		protected virtual void OnValidate()
		{
			if (TagContainer == null)
				TagContainer = new TagContainer();
			TagContainer.RemoveDoubles();
		}
	}

	public static class StepExtensions
	{
		public static int GetIndex(this BaseStep step)
		{
			if (!step.gameObject.activeSelf || step.Container == null || step.Container.Steps == null) 
				return -1;
			
			return step.Container.Steps.IndexOf(step);
		} 
	}
}