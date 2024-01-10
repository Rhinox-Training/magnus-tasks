using System;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
#endif

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain"), GenericTypeGeneration]
    public class UnityValueResolver<T> : BaseValueResolver<T> where T : UnityEngine.Object
    {
        public override string SimpleName => $"Unity Object";
        public override string ComplexName => $"Unity Object of type [{typeof(T).Name}]";

        [HideInInspector] public IObjectReferenceResolver Resolver;

        // Keep track of resolution as it's an expensive operation
        private bool _resolved;
        private T _resolvedValue;

        [ShowInInspector, DisableIf(nameof(_failedToLoad))]
        [HideLabelAttribute]
        [OnInspectorGUI(nameof(DrawFailedToResolve), false)]
        public T Value
        {
            get
            {
                if (_resolved) return _resolvedValue;
                return TryResolve();
            }
            set
            {
                _resolvedValue = value;
                ValueChanged();
            }
        }

        private bool _failedToLoad;
        public bool IsLocked => _failedToLoad;

        protected virtual string LoadErrorMessage => Resolver?.ErrorMessage;

        public override bool TryResolve(ref T value)
        {
            T result = Resolver?.Resolve() as T;
            if (result == null)
                return false;
            value = result;
            return true;
        }

        private void ValueChanged()
        {
            Resolver = SerializedUnityReferencesObjectManager.TryEncode(_resolvedValue);
        }

        private void DrawFailedToResolve()
        {
#if UNITY_EDITOR
            if (!_failedToLoad) return;

            GUILayout.BeginHorizontal();
            EditorGUILayout.HelpBox($"Failed to resolve, owner scene is probably not open.\n{LoadErrorMessage}",
                MessageType.Error);
            GUIContentHelper.PushDisabled(false);
            if (GUILayout.Button("Edit", GUILayout.ExpandWidth(false)))
                _failedToLoad = false;
            GUIContentHelper.PopDisabled();
            GUILayout.EndHorizontal();
#endif
        }

        private T TryResolve()
        {
            _resolvedValue = Resolver?.Resolve() as T;
            if (Resolver != null && _resolvedValue == null)
                _failedToLoad = true;
            _resolved = true;
            return _resolvedValue;
        }

        public static IValueResolver Create(T instance = null)
        {
            var valueResolver = new UnityValueResolver<T>();
            if (instance != null)
            {
                var resolver = SerializedUnityReferencesObjectManager.TryEncode(instance);
                valueResolver.Resolver = resolver;
                valueResolver._resolvedValue = instance;
                valueResolver._resolved = true;
            }

            return valueResolver;
        }

        protected bool Equals(UnityValueResolver<T> other)
        {
            if (ReferenceEquals(Resolver, other.Resolver)) return true;
            if (Resolver == null) return other.Resolver == null;
            return Resolver.Equals(other.Resolver);
        }

        public override bool Equals(IValueResolver other) => Equals((object) other);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((UnityValueResolver<T>) obj);
        }

        public override int GetHashCode()
        {
            return (Resolver != null ? Resolver.GetHashCode() : 0);
        }
    }
}