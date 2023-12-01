using System;
using Rhinox.Lightspeed;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker, Serializable, GenericTypeGeneration]
    public class ConstantDataSource<T> : IMemberDataSource
    {
        [SerializeReference] public T Data;
        
        public bool ResetValue()
        {
            if (Data.IsDefault())
                return false;
            Data = default(T);
            return true;
        }

        public object GetValue()
        {
            return Data;
        }

        public void SetValue(object val)
        {
            Data = (T)val;
        }

        public Type GetMemberType()
        {
            return typeof(T);
        }
    }
}