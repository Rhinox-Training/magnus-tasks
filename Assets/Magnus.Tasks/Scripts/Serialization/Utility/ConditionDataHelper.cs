using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public static class ConditionDataHelper
    {
        private static bool IsBlackListed(Type t)
        {
            return typeof(UnityEvent) == t ||
                   typeof(BetterEvent) == t;//||
                   //t.InheritsFrom(typeof(UnityEngine.Object));
        }

        public static ConditionData FromCondition<T>(T condition, bool checkValueImporter = false)
            where T : BaseCondition
        {
            Type cType = condition.GetType();
            List<ParamData> parameterData = new List<ParamData>();

            foreach (var info in GetParamDataFields(cType))
            {
                bool imported = false;
                ParamData pd = default;
                if (checkValueImporter)
                {
                    var importAttr = info.GetCustomAttribute<ImportToDataLayerAttribute>();
                    if (importAttr != null)
                    {
                        object val = ReflectionUtility.FetchValuePropertyHelper(info.GetReturnType(), info.DeclaringType, importAttr.MemberName, condition);
                        pd = ParamData.CreateWithValue(info, val);
                        imported = true;
                    }
                }

                if (!imported)
                    pd = ParamData.CreateFromInstance(info, condition);
                parameterData.Add(pd);
            }
            
            return new ConditionData(cType, parameterData);
        }

        public static IEnumerable<MemberInfo> GetParamDataFields(Type t)
        {
            foreach (var member in FilterMembersForParamData(t.GetFieldOptions()))
                yield return member;
            
            foreach (var member in FilterMembersForParamData(t.GetPropertyOptions()))
                yield return member;
        }

        private static IEnumerable<MemberInfo> FilterMembersForParamData(ICollection<MemberInfo> infos)
        {
            foreach (var info in infos)
            {
                if (!info.IsSerialized())
                    continue;
                
                if (IsBlackListed(info.GetReturnType()))
                    continue;

                yield return info;
            }
        }
        
        public static BaseCondition ToCondition(ConditionData data)
        {
            Type conditionType = data.ConditionType.Type;
            if (conditionType == null)
            {
                PLog.Warn<MagnusLogger>($"Could not find type of condition {data.ConditionType.AssemblyQualifiedName}, returning null");
                return null;
            }

            if (!conditionType.InheritsFrom(typeof(BaseCondition)))
            {
                PLog.Error<MagnusLogger>($"ConditionType {conditionType.FullName} does not inherit from BaseCondition, returning null");
                return null;
            }

            BaseCondition condition = Activator.CreateInstance(conditionType) as BaseCondition;
            foreach (var param in data.Params)
            {
                if (param.TryFindMemberInfoOn(conditionType, out var member, out string error))
                    member.SetValue(condition, param.MemberData);
                else
                    PLog.Trace<MagnusLogger>(error);
            }

            return condition;
        }

        public static Type GetParamType(this ConditionData data, ParamData param)
        {
            var conditionType = data.ConditionType.Type;
            switch (param.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = conditionType.GetField(param.Name, param.Flags);
                    if (fieldInfo == null)
                        return null;

                    return fieldInfo.FieldType;
                case MemberTypes.Property: 
                    var propertyInfo = conditionType.GetProperty(param.Name, param.Flags);
                    if (propertyInfo == null)
                        return null;
                    return propertyInfo.PropertyType;
                default:
                    return null;
            }
        }
        
        public static T GetParamAttribute<T>(this ConditionData data, ParamData param) where T : Attribute
        {
            var memberInfo = GetMemberInfo(data, param);
            if (memberInfo == null)
                return null;
            return memberInfo.GetCustomAttribute<T>();
        }

        public static MemberInfo GetMemberInfo(this ConditionData data, ParamData param)
        {
            var conditionType = data.ConditionType.Type;
            switch (param.MemberType)
            {
                case MemberTypes.Field:
                    var fieldInfo = conditionType.GetField(param.Name, param.Flags);
                    if (fieldInfo == null)
                        return null;

                    return fieldInfo;
                case MemberTypes.Property: 
                    var propertyInfo = conditionType.GetProperty(param.Name, param.Flags);
                    if (propertyInfo == null)
                        return null;
                    return propertyInfo;
                default:
                    return null;
            }
        }
    }
}