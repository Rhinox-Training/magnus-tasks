using System;
using Rhinox.Lightspeed;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public class ReferenceDataSource : IMemberDataSource
    {
        public SerializableGuid ID;

        public Type ReferenceType { get; }

        public ReferenceDataSource(Type referenceType)
        {
            ReferenceType = referenceType;
        }

        public bool ResetValue()
        {
            if (ID.IsNullOrEmpty())
                return false;
            ID = SerializableGuid.Empty;
            return true;
        }

        public object GetValue()
        {
            return ID;
        }

        public void SetValue(object val)
        {
            ID = (SerializableGuid) val;
        }

        public Type GetMemberType()
        {
            return ReferenceType; // TODO: this or typeof(SerializableGuid)
        }

        public static ReferenceDataSource CreateReference(IMemberDataSource datasource)
        {
            if (datasource is ReferenceDataSource refDataSource)
                return refDataSource;
            return new ReferenceDataSource(datasource.GetMemberType());
        }

        public IMemberDataSource BuildConstant()
        {
            DataSourceHelper.TryCreate(null, ReferenceType, out var source);
            return source;
        }
    }
}