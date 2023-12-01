using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks
{
    public static class ConditionDataHelper
    {
        public static BaseObjectDataContainer FromCondition<T>(T condition, bool checkValueImporter = false)
            where T : BaseCondition
        {
            Type cType = condition.GetType();

            if (cType.ImplementsOpenGenericClass(typeof(IDataDrivenObject<>)))
            {
                var argumentTypes = cType.GetArgumentsOfInheritedOpenGenericClass(typeof(IDataDrivenObject<>));
                var argumentType = argumentTypes[0];

                var dataDriverObject = (BaseDataDriverObject)Activator.CreateInstance(argumentType);
                var containerData = new ManagedObjectDataContainer()
                {
                    Type = new SerializableType(cType),
                    ManagedData = dataDriverObject
                };
                return containerData;
            }

            var conditionData = ObjectDataContainer.Create(cType, condition, checkValueImporter);
            return conditionData;
        }
        
        // public static Type GetParamType(this ConditionData data, MemberContainer param)
        // {
        //     var conditionType = data.ConditionType.Type;
        //     switch (param.MemberType)
        //     {
        //         case MemberTypes.Field:
        //             var fieldInfo = conditionType.GetField(param.Name, param.Flags);
        //             if (fieldInfo == null)
        //                 return null;
        //
        //             return fieldInfo.FieldType;
        //         case MemberTypes.Property: 
        //             var propertyInfo = conditionType.GetProperty(param.Name, param.Flags);
        //             if (propertyInfo == null)
        //                 return null;
        //             return propertyInfo.PropertyType;
        //         default:
        //             return null;
        //     }
        // }
        //
        // public static T GetParamAttribute<T>(this ConditionData data, MemberContainer param) where T : Attribute
        // {
        //     var memberInfo = GetMemberInfo(data, param);
        //     if (memberInfo == null)
        //         return null;
        //     return memberInfo.GetCustomAttribute<T>();
        // }
        //
        // public static MemberInfo GetMemberInfo(this ConditionData data, MemberContainer param)
        // {
        //     var conditionType = data.ConditionType.Type;
        //     switch (param.MemberType)
        //     {
        //         case MemberTypes.Field:
        //             var fieldInfo = conditionType.GetField(param.Name, param.Flags);
        //             if (fieldInfo == null)
        //                 return null;
        //
        //             return fieldInfo;
        //         case MemberTypes.Property: 
        //             var propertyInfo = conditionType.GetProperty(param.Name, param.Flags);
        //             if (propertyInfo == null)
        //                 return null;
        //             return propertyInfo;
        //         default:
        //             return null;
        //     }
        // }
    }
}