using System;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable, RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class ConstValueResolver : BaseValueResolver, ISerializationCallbackReceiver
    {
        public override string SimpleName => "Constant";
        public override string ComplexName => $"Constant Value of Type '{_type.Name}'";

        [SerializeField, HideInInspector]
        private SerializableType _type;

        [HideLabel, NonSerialized, ShowInInspector, DrawAsType(nameof(_type))]
        public object Value;

        [SerializeField, HideInInspector] private string _jsonData;

        public ConstValueResolver(Type type, object value = null)
        {
            _type = new SerializableType(type);
            Value = value;
        }

        public override Type GetTargetType() => _type;

        public override bool TryResolve(ref object value)
        {
            value = Value;
            return true;
        }

        public override bool Equals(IValueResolver other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (other.GetType() != this.GetType()) return false;
            return Equals((ConstValueResolver) other);
        }

        protected bool Equals(ConstValueResolver other)
        {
            return Equals(Value, other.Value);
        }

        public void OnBeforeSerialize()
        {
            _jsonData = Utility.ToJson(Value);
        }

        public void OnAfterDeserialize()
        {
            Value = Utility.FromJson(_jsonData, _type);
        }
    }

}

