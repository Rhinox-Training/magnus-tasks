using System;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    [RefactoringOldNamespace("", "com.rhinox.volt.training")]
    public class ReferenceKey
    {
        public SerializableGuid Guid;
        public string Name;
        public SerializableType ValueType;
        public int OverrideIndex;
        public string CustomName;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(CustomName))
                    return Name;
                return CustomName;
            }
        }

        public ReferenceKey(Type type, string name)
        {
            Guid = SerializableGuid.CreateNew();
            Name = name;
            ValueType = new SerializableType(type);
        }

        public ReferenceKey(SerializableGuid guid, Type type, string name)
        {
            Guid = guid;
            Name = name;
            ValueType = new SerializableType(type);
        }

        public static ReferenceKey Create(Type ownerType, FieldInfo f, ValueReferenceAttribute attr)
        {
            return new ReferenceKey(ownerType, attr.DefaultKeyName);
        }

        public static ReferenceKey Create<T>(FieldInfo f, ValueReferenceAttribute attr)
        {
            return new ReferenceKey(typeof(T), attr.DefaultKeyName);
        }

        // TODO: is this used? or should this still be implemented?
        // public ReferenceKey CreateOverride()
        // {
        //     return new ReferenceKey(ValueType, Name)
        //     {
        //         OverrideIndex = this.OverrideIndex + 1
        //     };
        // }

        public void ChangeType(Type type)
        {
            ValueType = new SerializableType(type);
        }
    }

// [Serializable]
// Can't be Serializable due to the object field :(
    [HideReferenceObjectPicker]
    public class ReferenceKeyOverride
    {
        [SerializeField, HideInInspector] protected string _name;

        [SerializeField, HideInInspector] protected SerializableGuid _guid;
        [SerializeField, HideInInspector] protected SerializableType _valueType;

        [SerializeField, NotConvertedToDataLayer]
        private object _value;

        [ValueReference("ValueType", "OverrideValue")]
        public SerializableGuid ValueIdentifier;

        public SerializableGuid Guid => _guid;
        public Type ValueType => _valueType.Type;
        public string Name => _name;

        public ReferenceKeyOverride(ReferenceKey key, object value = null)
        {
            ValueIdentifier = SerializableGuid.Empty;

            _guid = key.Guid;
            _valueType = key.ValueType;
            _value = value;

            Update(key);
        }

        public void Update(ReferenceKey key)
        {
            _name = key.DisplayName;
        }

        public bool GetValue(IReferenceResolver resolver, out object value)
        {
            value = _value;

            if (ValueIdentifier.IsNullOrEmpty() || resolver == null)
                return true;

            if (!resolver.Resolve(ValueIdentifier, out object resolvedValue))
                return false;

            value = resolvedValue;
            return true;
        }

        public bool GetValue<T>(IReferenceResolver resolver, out T value)
        {
            value = default;

            if (ValueIdentifier.IsNullOrEmpty() || resolver == null)
            {
                if (_valueType != typeof(T))
                    return false;
                value = (T) _value;
                return true;
            }

            if (!resolver.Resolve(ValueIdentifier, out T resolvedValue))
                return false;
            value = resolvedValue;
            return true;
        }
    }
}