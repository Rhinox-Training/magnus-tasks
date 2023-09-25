using System;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;

namespace Rhinox
{
    public static class BetterSerializedObjectExtensions
    {
        public static T BetterGetAttribute<T>(this SerializedProperty property) where T : Attribute
        {
            if (property == null) return default(T);
            
            // Internal members that are defined in c++ don't have attributes that can be resolved
            if (property.IsInternal()) return default(T);
            
            if (property.propertyPath.Contains(".Array.data[") || property.propertyPath.Contains("."))
            {
                var hostInfo = property.GetHostInfo();
                return hostInfo.GetAttribute<T>();
            }
            
            System.Type parentType = property.serializedObject.targetObject.GetType();
            ReflectionUtility.TryGetField(parentType, property.propertyPath, out FieldInfo fi);

            if (fi == null) return default(T);
            
            return fi.GetCustomAttribute<T>();
        }

        
        // returns if the property is defined in c++ or not;
        public static bool IsInternal(this SerializedProperty prop)
        {
           return prop.propertyPath.Contains("m_");

        }
    }
}