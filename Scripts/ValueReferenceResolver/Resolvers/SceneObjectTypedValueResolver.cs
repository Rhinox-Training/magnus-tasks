using System;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class SceneObjectTypedValueResolver : BaseTypedValueResolver<Object>
    {
        private string TargetType => ComponentType?.Name ?? "GameObject";
        public override string SimpleName => "Scene Object";
        public override string ComplexName => $"Scene Object of type [{TargetType}]";

        [Tooltip("Optional; If null will return the GameObject")]
        public SerializableType ComponentType;

        public string ScenePath;
        public bool FindLoosely;

        public override bool TryResolveGeneric(ref Object value)
        {
            // var newSW = Stopwatch.StartNew();
            Object result;
            var obj = SceneHierarchyTree.Find(ScenePath);
            // newSW.Stop();

            // Debug.Log($"[NEW] {obj?.name} ({newSW.ElapsedMilliseconds}ms)");
            // GameObject obj = Utility.FindInScene(ScenePath, FindLoosely);

            if (obj != null && ComponentType != null && ComponentType.Type.InheritsFrom(typeof(Component)))
                result = obj.GetComponent(ComponentType);
            else result = obj;

            if (result == null)
                return false;
            value = result;
            return true;
        }

        public override bool Equals(IValueResolver other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return Equals((SceneObjectTypedValueResolver) other);
        }

        protected bool Equals(SceneObjectTypedValueResolver other)
        {
            // If null && other is not, return false; if null and other is also null, check scenepath
            if (ComponentType == null && other.ComponentType != null)
                return false;
            if (ComponentType != null && !ComponentType.Equals(other.ComponentType))
                return false;
            return Equals(ScenePath, other.ScenePath);
        }
    }
}