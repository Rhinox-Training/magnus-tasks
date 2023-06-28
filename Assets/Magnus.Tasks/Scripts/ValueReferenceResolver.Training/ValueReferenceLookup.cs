using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.Vortex;
using UnityEngine;

[Serializable]
public struct DefaultTypeReferenceKey
{
    public ValueReferenceFieldData FieldData;
    public SerializableGuid KeyGuid;

    public bool Apply(object target)
    {
        if (FieldData.Field.DeclaringType != target.GetType())
            return false;
        
        if (FieldData.Field.FieldType != typeof(SerializableGuid))
            return false;
        
        FieldData.Field.FieldInfo.SetValue(target, KeyGuid);
        return true;
    }
}

[Serializable]
// [ShowOdinSerializedPropertiesInInspector]
public class ValueReferenceLookup : IReferenceResolver
{

    public Dictionary<SerializableType, DefaultTypeReferenceKey[]> DefaultsByType = new Dictionary<SerializableType, DefaultTypeReferenceKey[]>();
    
    public Dictionary<SerializableGuid, IValueResolver> ValueResolversByKey = new Dictionary<SerializableGuid, IValueResolver>();
    
    public List<ReferenceKey> Keys = new List<ReferenceKey>();

    public bool RegisterDefault(FieldInfo field, SerializableGuid newDefaultGuid, bool overwriteIfExists = true)
    {
        if (field == null || newDefaultGuid.Equals(null))
            return false;
        
        var valRefAttr = field.GetCustomAttribute<ValueReferenceAttribute>();
        if (valRefAttr == null)
        {
            PLog.Error<VortexLogger>($"Cannot RegisterDefault for field {field.Name} in {field.DeclaringType?.Name}, is not a ValueReference (attribute missing)");
            return false;
        }

        var type = valRefAttr.ReferenceType;
        
        if (valRefAttr.TargetField != null)
        {
            var declaringType = field.DeclaringType;
            if (ReflectionUtility.TryGetMember(declaringType, valRefAttr.TargetField, out MemberInfo member))
                type = member.GetReturnType();
            else
            {
                PLog.Error<VortexLogger>($"Unable to RegisterDefault for field {field.Name}, TargetField not found ({valRefAttr.TargetField})");
                return false;
            }
        }
        
        if (type == null)
        {
            PLog.Error<VortexLogger>($"Unable to RegisterDefault for field {field.Name}, ReferenceType specified by string instead of type (str: {valRefAttr.TypeLookup})");
            return false;
        }
        
        var fieldData = ValueReferenceFieldData.Create(field);
        
        var defaultRefKey = new DefaultTypeReferenceKey()
        {
            KeyGuid = newDefaultGuid,
            FieldData = fieldData
        };

        var keyType = new SerializableType(valRefAttr.ReferenceType);
        if (!DefaultsByType.ContainsKey(keyType))
            DefaultsByType.Add(keyType, new [] { defaultRefKey });
        else
        {
            var entryIndex = DefaultsByType[keyType].FindIndex(x => x.FieldData.Field.Equals(field));
            if (entryIndex == -1)
            {
                var arr = DefaultsByType[keyType].Union(new [] { defaultRefKey }).ToArray();
                DefaultsByType[keyType] = arr;
            }
            else
            {
                if (!overwriteIfExists)
                    return false;
                
                var entry = DefaultsByType[keyType][entryIndex];
                entry.KeyGuid = newDefaultGuid;
                DefaultsByType[keyType][entryIndex] = entry;
            }
        }
        return true;
    }

    public IValueResolver FindResolverByName(string defaultName)
    {
        var key = Keys.FirstOrDefault(x => x.Name.Equals(defaultName, StringComparison.InvariantCulture));
        if (key != null)
            return ValueResolversByKey[key.Guid];
        return null;
    }

    public IValueResolver FindResolverByID(SerializableGuid id)
    {
        // TODO: does this work?
        // return ValueResolversByKey.GetOrDefault(id);
        // TODO: might not due to being a class but it implements equal/hash so should be ok
        var key = Keys.FirstOrDefault(x => x.Guid.Equals(id));
        if (key != null)
            return ValueResolversByKey[key.Guid];
        return null;
    }

    public IEnumerable<IValueResolver> FindResolversByType(Type resolveTargetType)
    {
        var keys = Keys.Where(x => x.ValueType.Type == resolveTargetType);
        foreach (var key in keys)
        {
            yield return ValueResolversByKey[key.Guid];
        }
    }
    
    // TODO: Test duplicate key
    public SerializableGuid Register(string defaultName, IValueResolver resolver, bool overwriteOnNotNull = false)
    {
        if (resolver == null)
            throw new ArgumentException($"Cannot register '{defaultName}' with no resolver.");
        
        var key = Keys.FirstOrDefault(x => x.Name.Equals(defaultName, StringComparison.InvariantCulture));
        if (key != null)
        {
            var valueResolver = ValueResolversByKey[key.Guid];
            if (valueResolver == null || overwriteOnNotNull)
            {
                ValueResolversByKey[key.Guid] = resolver;
                key.ChangeType(resolver.GetTargetType());
            }
            return key.Guid;
        }

        ReferenceKey newKey = new ReferenceKey(resolver.GetTargetType(), defaultName);
        Keys.Add(newKey);
        
        ValueResolversByKey.Add(newKey.Guid, resolver);
        return newKey.Guid;
    }
    
    // TODO merge with above (when exporting partial task we want to use the same guids; that or update all data to the new guid)
    public SerializableGuid Register(SerializableGuid guid, string defaultName, IValueResolver resolver, bool overwriteOnNotNull = false)
    {
        if (resolver == null)
            throw new ArgumentException($"Cannot register '{defaultName}' with no resolver.");
        
        if (guid == null)
            throw new ArgumentException($"Cannot register '{defaultName}' with no valid guid.");
        
        var key = Keys.FirstOrDefault(x => x.Name.Equals(defaultName, StringComparison.InvariantCulture));
        if (key != null && guid.Equals(key.Guid))
        {
            var valueResolver = ValueResolversByKey[key.Guid];
            if (valueResolver == null || overwriteOnNotNull)
            {
                ValueResolversByKey[key.Guid] = resolver;
                key.ChangeType(resolver.GetTargetType());
            }
            return key.Guid;
        }

        ReferenceKey newKey = new ReferenceKey(guid, resolver.GetTargetType(), defaultName);
        Keys.Add(newKey);
        
        ValueResolversByKey.Add(newKey.Guid, resolver);
        return newKey.Guid;
    }

    public SerializableGuid Register<T>(string defaultName)
    {
        if (typeof(T).InheritsFrom<UnityEngine.Object>())
            return Register(defaultName, ValueResolverHelper.Create(typeof(T)));
        return Register(defaultName, new ConstValueResolver<T>());
    }

    public SerializableGuid Register(string defaultName, Type objectType)
    {
        var methods = GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
        var registerMethod = methods.FirstOrDefault(x => x.Name.Equals(nameof(Register)) && x.IsGenericMethod && x.GetParameters().Length == 1);
        
        if (registerMethod == null)
        {
            PLog.Error<VortexLogger>($"Cannot find generic RegisterMethod for Type: {objectType}");
            return null;
        }
        var concreteMethod = registerMethod.MakeGenericMethod(objectType);
        return concreteMethod.Invoke(this, new[] {defaultName}) as SerializableGuid;
    }

    public bool Deregister(SerializableGuid guid)
    {
        if (guid == null)
            return false;
        var key = Keys.FirstOrDefault(x => x.Guid.Equals(guid));
        if (key == null)
            return false;
        Keys.Remove(key);
        ValueResolversByKey.Remove(guid);
        foreach (var defaults in DefaultsByType)
        {
            int index = 0;
            foreach (var defKey in defaults.Value)
            {
                if (guid.Equals(defKey.KeyGuid))
                {
                    var cache = defKey;
                    cache.KeyGuid = null;
                    DefaultsByType[defaults.Key][index] = cache;
                }

                index++;
            }
            
        }
        return true;
    }
    
    public bool Resolve(SerializableGuid key, out object value)
    {
        value = default;

        if (!ValueResolversByKey.TryGetValue(key, out IValueResolver resolver))
            return false;

        return resolver.TryResolve(ref value);
    }
    
    public bool Resolve<T>(SerializableGuid key, out T value)
    {
        value = default;
        if (!ValueResolversByKey.TryGetValue(key, out IValueResolver resolver))
            return false;

        object resolvedValue = default;
        if (!resolver.TryResolve(ref resolvedValue))
            return false;

        if (!(resolvedValue is T typedResult))
            return false;
        value = typedResult;
        return true;
    }

    public void ResolveDefaults(object value)
    {
        var valueType = new SerializableType(value.GetType());
        if (!DefaultsByType.ContainsKey(valueType)) return;
        var defaults = DefaultsByType[valueType];
        
        foreach (var d in defaults)
        {
            if (!d.Apply(value))
                PLog.Warn<VortexLogger>("Could not apply default key to type.");
        }
    }

    public ICollection<ReferenceKey> GetKeysFor(Type t)
    {
        return Keys.Where(x => x.ValueType.Type.InheritsFrom(t)).ToArray();
    }

    public IReadOnlyCollection<ReferenceKey> GetKeys()
    {
        return Keys.ToArray();
    }

    public SerializableGuid GetDefault(SerializableType keyType, FieldInfo field)
    {
        if (keyType == null || field == null)
            return null;
        if (!DefaultsByType.ContainsKey(keyType))
            return null;
        var defaultTypeReferenceKeys = DefaultsByType[keyType];
        if (defaultTypeReferenceKeys == null)
            return null;
        var defaultEntry = defaultTypeReferenceKeys.FirstOrDefault(x => x.FieldData.Field.Equals(field));
        if (defaultEntry.Equals(default(DefaultTypeReferenceKey)))
            return null;
        return defaultEntry.KeyGuid;
    }

    public ReferenceKey FindKey(SerializableGuid guid)
    {
        if (Keys == null || Keys.Count == 0)
            return null;

        return Keys.FirstOrDefault(x => x.Guid.Equals(guid));
    }

    public bool RemoveDefault(SerializableType keyType, FieldInfo field)
    {
        if (keyType == null || field == null)
            return false;
        if (!DefaultsByType.ContainsKey(keyType))
            return false;
        var defaultTypeReferenceKeys = DefaultsByType[keyType];
        if (defaultTypeReferenceKeys == null)
            return false;
        int count = defaultTypeReferenceKeys.Length;
        defaultTypeReferenceKeys = defaultTypeReferenceKeys.Where(x => !x.FieldData.Field.Equals(field)).ToArray();
        int newCount = defaultTypeReferenceKeys.Length;
        DefaultsByType[keyType] = defaultTypeReferenceKeys;
        return newCount != count;
    }
}
