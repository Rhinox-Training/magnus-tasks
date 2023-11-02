using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Utilities;

namespace Rhinox.Magnus.Tasks
{
    public struct ValueReferenceInfo
    {
        public MemberInfo IdentifierMember;
        public MemberInfo ValueMember;

        public Type ReferenceType;
        public string DefaultName;
    }

    public static class ValueReferenceHelper
    {
        private static Dictionary<Type, ValueReferenceFieldData[]> _valueReferenceDataByConditionType;

        public static ValueReferenceFieldData[] GetValueReferenceDataForCondition(Type conditionType)
        {
            if (conditionType == null || conditionType.InheritsFrom<BaseCondition>())
                return Array.Empty<ValueReferenceFieldData>();
            
            if (_valueReferenceDataByConditionType == null)
                _valueReferenceDataByConditionType = new Dictionary<Type, ValueReferenceFieldData[]>();

            if (_valueReferenceDataByConditionType.ContainsKey(conditionType))
                return _valueReferenceDataByConditionType[conditionType];


            var fieldsWithAttr = conditionType.GetFieldsWithAttribute<ValueReferenceAttribute>();
            if (fieldsWithAttr.Length > 0)
            {
                var fieldDatas = new ValueReferenceFieldData[fieldsWithAttr.Length];
                for (int i = 0; i < fieldsWithAttr.Length; ++i)
                {
                    var field = fieldsWithAttr[i];
                    fieldDatas[i] = ValueReferenceFieldData.Create(field);
                }

                _valueReferenceDataByConditionType.Add(conditionType, fieldDatas);
            }
            else
                _valueReferenceDataByConditionType.Add(conditionType, Array.Empty<ValueReferenceFieldData>());
            
            return _valueReferenceDataByConditionType[conditionType];
        }
        
        public static bool TryGetValueReference(this ValueReferenceAttribute valueRefAttr, Type declaringType,
            out ValueReferenceInfo info, object instance = null)
        {
            info = new ValueReferenceInfo
            {
                DefaultName = string.Empty
            };

            if (valueRefAttr == null)
                return false;

            if (!string.IsNullOrWhiteSpace(valueRefAttr.TargetField))
            {
                if (!ReflectionUtility.TryGetMember(declaringType, valueRefAttr.TargetField,
                        out MemberInfo valueMember))
                    throw new InvalidDataException(
                        $"Cannot find '{valueRefAttr.TargetField}' in '{declaringType?.Name}'.");

                info.ValueMember = valueMember;

                info.ReferenceType = valueMember.GetReturnType();
                info.DefaultName = valueMember.Name;

                return true;
            }

            // Resolve type if it is null
            if (valueRefAttr.ReferenceType == null)
            {
                if (string.IsNullOrWhiteSpace(valueRefAttr.TypeLookup))
                    throw new InvalidDataException("Cannot find ReferenceType for ValueLookup, MethodLookup is empty");

                info.ReferenceType =
                    ReflectionUtility.FetchValuePropertyHelper<Type>(declaringType, valueRefAttr.TypeLookup, instance);
            }
            else
            {
                info.ReferenceType = valueRefAttr.ReferenceType;
            }

            info.DefaultName = valueRefAttr.DefaultKeyName;
            return true;
        }

        public static bool TryGetValueReference(MemberInfo member, out ValueReferenceInfo info, object instance = null)
        {
            var valueRefAttr = member != null ? member.GetCustomAttribute<ValueReferenceAttribute>() : null;

            var success = TryGetValueReference(valueRefAttr, member?.DeclaringType, out info, instance);

            info.IdentifierMember = member;

            return success;
        }
    }
}