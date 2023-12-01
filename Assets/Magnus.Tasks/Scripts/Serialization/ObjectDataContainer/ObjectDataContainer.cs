using System;
using System.Collections.Generic;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using UnityEngine.Serialization;

namespace Rhinox.Magnus.Tasks
{
    [Serializable]
    public class ObjectDataContainer : BaseObjectDataContainer
    {
        public SerializableType ObjectType;
        public List<ObjectMemberData> Params;
        
        private ObjectDataContainer(Type t)
        {
            ObjectType = new SerializableType(t);
            Params = new List<ObjectMemberData>();
        }

        public static ObjectDataContainer Create(Type t, object instance = null, bool checkValueImporter = false)
        {
            var container = new ObjectDataContainer(t);

            foreach (var member in MemberDataHelper.FindMembers(t, instance, checkValueImporter))
            {
                if (member == null)
                    continue;

                container.Register(member);
            }

            return container;
        }
        
        private bool Register(ObjectMemberData member)
        {
            if (member == null)
                return false;
            
            if (Params == null)
                Params = new List<ObjectMemberData>();

            Params.AddUnique(member);
            return true;
        }

        private bool CopyDataTo(object instance)
        {
            if (instance == null)
                return false;

            if (Params == null)
                return true;

            var instanceType = instance.GetType();
            if (!ObjectType.Type.IsAssignableFrom(instanceType))
                return false;
            
            foreach (var param in Params)
            {
                var memberInfo = param.MemberInfo.GetMemberInfo();
                memberInfo.SetValue(instance, param.MemberData.GetValue()); // TODO: this won't work for containered data
            }
            return true;
        }
        
        public static T BuildInstance<T>(BaseObjectDataContainer data)
        {
            if (data.TryGetObjectType(out Type instanceType, out string error))
            {
                PLog.Warn<MagnusLogger>(error);
                return default(T);
            }

            if (!instanceType.InheritsFrom(typeof(T)))
            {
                PLog.Error<MagnusLogger>($"ConditionType {instanceType.FullName} does not inherit from BaseCondition, returning null");
                return default(T);
            }

            T instance = (T)Activator.CreateInstance(instanceType);
            if (data is ObjectDataContainer objectDataContainer)
            {
                objectDataContainer.CopyDataTo(instance);
            }

            return instance;
        }

        public override bool TryGetObjectType(out Type type, out string error)
        {
            type = ObjectType;
            if (type == null)
            {
                error = $"Could not deserialize type '{ObjectType.AssemblyQualifiedName}', returning null";
                return false;
            }

            error = string.Empty;
            return true;
        }
    }
}