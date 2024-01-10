using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;

namespace Rhinox.Magnus.Tasks
{
    
    [Serializable]
    public abstract class BaseObjectDataContainer
    {
        [Serializable]
        public class ReferenceDataEntry
        {
            public SerializableType ReferenceType;
            public SerializableGuid Key;
            public bool IsEnabled;
        }

        public List<ReferenceDataEntry> ReferenceDatas;

        protected BaseObjectDataContainer()
        {
            ReferenceDatas = new List<ReferenceDataEntry>();
        }

        public abstract bool TryGetObjectType(out Type type, out string error);

        public ReferenceDataEntry FindOrCreateReference()
        {
            throw new NotImplementedException();
        }

        public ReferenceDataEntry FindReference(SerializableMemberInfo memberInfo, bool createIfNotFound = false)
        {
            foreach (var data in ReferenceDatas)
            {
                if (data.ReferenceType == memberInfo.GetMemberInfo().GetReturnType())
                    return data;
            }

            if (createIfNotFound)
            {
                var data = new ReferenceDataEntry()
                {
                    IsEnabled = true,
                    ReferenceType = new SerializableType(memberInfo.GetMemberInfo().GetReturnType()),
                    Key = SerializableGuid.Empty
                };
                ReferenceDatas.Add(data);
                return data;
            }

            return null;
        }
    }
}