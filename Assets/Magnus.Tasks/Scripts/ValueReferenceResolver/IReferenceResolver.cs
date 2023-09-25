using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Utilities;

namespace Rhinox.Magnus.Tasks
{
    public interface IUseReferenceGuid
    {
        bool UsesGuid(SerializableGuid guid);
        void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement);
    }
    
    public interface IValueReferenceResolverProvider
    {
        IReferenceResolver GetReferenceResolver();
    }
    
    public interface IReadOnlyReferenceResolver
    {
        bool Resolve(SerializableGuid key, out object value);
        bool Resolve<T>(SerializableGuid key, out T value);
    }

    public interface IReferenceResolver : IReadOnlyReferenceResolver
    {
        bool RegisterDefault(FieldInfo field, SerializableGuid newDefaultGuid, bool overwriteIfExists = true);

        SerializableGuid Register(string defaultName, IValueResolver resolver, bool overwriteOnNotNull = false);

        ICollection<ReferenceKey> GetKeysFor(Type t);

        IReadOnlyCollection<ReferenceKey> GetKeys();
        SerializableGuid GetDefault(SerializableType keyType, FieldInfo field);
        ReferenceKey FindKey(SerializableGuid guid);
        IValueResolver FindResolverByName(string key);
        IValueResolver FindResolverByID(SerializableGuid id);
        IEnumerable<IValueResolver> FindResolversByType(Type resolveTargetType);
    }
}