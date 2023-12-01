using System;
using Rhinox.Lightspeed;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public class ManagedObjectDataContainer : BaseObjectDataContainer
    {
        public SerializableType Type;

        [SerializeReference]
        public BaseDataDriverObject ManagedData;
        
        public override bool TryGetObjectType(out Type type, out string error)
        {
            type = ManagedData.GetType();
            error = "";
            return true;
        }
    }
}