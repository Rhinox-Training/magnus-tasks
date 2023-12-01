using System;
using System.Collections.Generic;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEngine.Events;

namespace Rhinox.Magnus.Tasks
{
    public static class MemberDataHelper
    {
        public static IEnumerable<ObjectMemberData> FindMembers(Type instancetype, object instance, bool checkValueImporter = false)
        {
            foreach (var info in GetParamDataFields(instancetype))
            {
                bool imported = false;
                ObjectMemberData container = null;
                if (checkValueImporter)
                {
                    var importAttr = info.GetCustomAttribute<ImportToDataLayerAttribute>();
                    if (importAttr != null)
                    {
                        object val = ReflectionUtility.FetchValuePropertyHelper(info.GetReturnType(), info.DeclaringType, importAttr.MemberName, instance);
                        container = CreateWithValue(info, val);
                        imported = true;
                    }
                }

                if (!imported)
                    container = CreateFromInstance(info, instance);

                if (container != null)
                    yield return container;
            }
        }
       
        public static ObjectMemberData CreateWithValue(MemberInfo info, object value)
        {
            var returnType = info.GetReturnType();
            if (value == null && !returnType.IsClass)
                value = returnType.GetDefault();

            SerializableMemberInfo serializableMemberInfo = null;
            if (info is FieldInfo fieldInfo)
                serializableMemberInfo = new SerializableFieldInfo(fieldInfo);
            else if (info is PropertyInfo propertyInfo)
                serializableMemberInfo = new SerializablePropertyInfo(propertyInfo);
            else
            {
                PLog.Error<MagnusLogger>($"MemberInfo '{info.GetType().Name}' not supported...");
                return null;
            }

            IMemberDataSource dataSource = null;
            if (value is IMemberDataSource valueSource)
            {
                dataSource = valueSource;
            }
            else if (DataSourceHelper.TryCreate(value, returnType, out var convertedSource))
            {
                dataSource = convertedSource;
            }
            else
            {
                PLog.Error<MagnusLogger>($"DataSource not supported '{value?.GetType()?.GetCSharpName() ?? returnType.GetCSharpName()}'...");
                return null;
            }

            var memberContainer = new ObjectMemberData()
            {
                MemberInfo = serializableMemberInfo,
                MemberData = dataSource
            };
            return memberContainer;
        }

        public static ObjectMemberData CreateFromInstance(MemberInfo info, object instance)
            => CreateWithValue(info, info.GetValue(instance));
        
        private static bool IsBlackListed(Type t)
        {
            return typeof(UnityEvent) == t ||
                   typeof(BetterEvent) == t;//||
            //t.InheritsFrom(typeof(UnityEngine.Object));
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
    }
}