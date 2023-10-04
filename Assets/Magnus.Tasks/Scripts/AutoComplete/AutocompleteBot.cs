using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Magnus.Tasks;
using Rhinox.Utilities;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public interface ITaskCreator
    {
        ManagedCoroutine CreateTask(object obj);
    }

    [AssignableTypeFilter, Serializable]
    public class AutocompleteBot // TODO: better way to register autocompletion?
    {
        private Dictionary<Type, ITaskCreator> _taskCreators = new Dictionary<Type, ITaskCreator>();
        private bool _initialized;

        public void Initialize()
        {
            if (_initialized)
                return;

            if (_taskCreators == null)
                _taskCreators = new Dictionary<Type, ITaskCreator>();

            OnInitialize();
            _initialized = true;
        }

        protected virtual void OnInitialize()
        {

        }


        public ITaskCreator FetchActionCreator(Type key)
        {
            if (!_taskCreators.ContainsKey(key))
                throw new InvalidOperationException($"No task creator for type {key.Name}");

            return _taskCreators[key];
        }

        public bool Register<T>(AutocompleteTask<T> taskCreator) where T : BaseCondition
        {
            var key = typeof(T);
            if (_taskCreators.ContainsKey(key))
                return false;

            taskCreator.Bot = this;
            _taskCreators.Add(key, taskCreator);
            return true;
        }
    }

    public abstract class AutocompleteTask<T> : ITaskCreator where T : BaseCondition
    {
        public AutocompleteBot Bot;

        protected abstract IEnumerator AutoComplete(T condition);

        public virtual ManagedCoroutine CreateTask(T condition)
        {
            return new ManagedCoroutine(CreateCoroutine(condition));
        }

        private IEnumerator CreateCoroutine(T condition)
        {
            var coroutine = AutoComplete(condition);
            yield return coroutine.Current;
            while (coroutine.MoveNext())
                yield return coroutine.Current;

            while (!condition.IsMet)
                yield return new WaitForEndOfFrame();
        }

        public ManagedCoroutine CreateTask(object obj)
        {
            return CreateTask((T) obj);
        }
    }

    public class SkipComplete<T> : AutocompleteTask<T> where T : BaseCondition
    {
        protected override IEnumerator AutoComplete(T condition)
        {
            condition.SetConditionMet();
            yield return null;
        }
    }
}