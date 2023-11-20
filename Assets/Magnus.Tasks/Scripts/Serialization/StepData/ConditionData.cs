using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Attributes;
using Rhinox.Utilities;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [HideReferenceObjectPicker, Serializable]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    public class ParamData
    {
        public string Name;
        public BindingFlags Flags;
        public MemberTypes MemberType;
        [DoNotDrawAsReference, HostInfoTypeHint(nameof(Type))]
        [SerializeReference]
        public object MemberData;
        public SerializableType Type;

        public static ParamData CreateWithValue(MemberInfo info, object value)
        {
            var returnType = info.GetReturnType();
            if (value == null && !returnType.IsClass)
                value = returnType.GetDefault();
                
            return new ParamData()
            {
                Name = info.Name,
                Flags = info.GetFlags(),
                MemberType = info.MemberType,
                MemberData = value,
                Type = new SerializableType(returnType)
            };
        }
        
        public static ParamData CreateFromInstance(MemberInfo info, object instance)
            => CreateWithValue(info, info.GetValue(instance));
        
        protected bool Equals(ParamData other)
        {
            return Name == other.Name && Flags == other.Flags 
                                      && MemberType == other.MemberType 
                                      && Equals(MemberData, other.MemberData) 
                                      && Equals(Type, other.Type);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ParamData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Flags;
                hashCode = (hashCode * 397) ^ (int) MemberType;
                hashCode = (hashCode * 397) ^ (MemberData != null ? MemberData.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Type != null ? Type.GetHashCode() : 0);
                return hashCode;
            }
        }
        
        public ParamData Clone()
        {
            var pd = new ParamData()
            {
                Name = Name,
                Flags = Flags,
                MemberType = MemberType,
                Type = Type,
                MemberData = MemberData,
            };
            return pd;
        }

        public bool IsField(SerializableFieldInfo fieldInfo)
        {
            var bindingFlags = fieldInfo.IsPublic ? BindingFlags.Public : BindingFlags.NonPublic;
            bindingFlags |= (fieldInfo.IsStatic ? BindingFlags.Static : BindingFlags.Instance);
            
            return Name == fieldInfo.Name && ((Flags & bindingFlags) != 0)
                                          && MemberType == MemberTypes.Field 
                                          && Type == fieldInfo.FieldType;
        }

        public bool TryFindMemberInfoOn(Type targetType, out MemberInfo memberInfo, out string errorMessage)
        {
            errorMessage = string.Empty;
            switch (MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = targetType.GetField(Name, Flags);
                    if (fieldInfo == null)
                        errorMessage = $"Failed to find Field {Name} with flags {Flags} on Type {targetType.Name}";
                    memberInfo = fieldInfo;
                    break;
                case MemberTypes.Property: 
                    var propertyInfo = targetType.GetProperty(Name, Flags);
                    if (propertyInfo == null)
                        errorMessage = $"Failed to find Property {Name} with flags {Flags} on Type {targetType.Name}";
                    memberInfo = propertyInfo;
                    break;
                default:
                    errorMessage = $"Member '{Name}' not found on '{targetType.Name}'";
                    memberInfo = null;
                    break;
            }
            return memberInfo != null;
        }

    }
    
    [Serializable]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    public class ConditionData : IUseReferenceGuid
    {
        public SerializableType ConditionType;
        public ParamData[] Params;

        public ConditionData(Type t, IEnumerable<ParamData> data)
        {
            ConditionType = new SerializableType(t);
            Params = data.ToArray();
        }
        
        public T GetParam<T>(SerializableFieldInfo field)
        {
            for (int i = 0; i < Params.Length; ++i)
            {
                if (Params[i].IsField(field))
                {
                    return (T) Params[i].MemberData;
                }
            }

            return default(T);
        }

        public bool SetParam(SerializableFieldInfo field, object data)
        {
            for (int i = 0; i < Params.Length; ++i)
            {
                if (Params[i].IsField(field))
                {
                    Params[i].MemberData = data;
                    return true;
                }
            }

            return false;
        }

        public bool UsesGuid(SerializableGuid guid)
        {
            if (Params == null) return false;

            foreach (var param in Params)
            {
                if (param.MemberData == null) 
                    continue;

                if (param.MemberData.Equals(guid))
                    return true;

                if (param.MemberData is IUseReferenceGuid e && e.UsesGuid(guid))
                    return true;
            }
            return false;
        }

        public void ReplaceGuid(SerializableGuid guid, SerializableGuid replacement)
        {
            if (Params == null) return;
            
            foreach (var param in Params)
            {
                if (param.MemberData == null) continue;

                if (param.MemberData.Equals(guid))
                    param.MemberData = replacement;

                if (param.MemberData is IUseReferenceGuid e)
                    e.ReplaceGuid(guid, replacement);
            }
        }
    }
}