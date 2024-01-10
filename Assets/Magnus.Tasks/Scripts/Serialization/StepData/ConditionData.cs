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
using UnityEngine.UI;

namespace Rhinox.Magnus.Tasks
{

    [Serializable]
    [RefactoringOldNamespace("Rhinox.VOLT.Data", "com.rhinox.volt")]
    [Obsolete]
    public class ConditionData : IUseReferenceGuid
    {
        public SerializableType ConditionType;
        public List<ObjectMemberData> Params;

        public ConditionData(Type t)
        {
            ConditionType = new SerializableType(t);
            Params = new List<ObjectMemberData>();
        }
        
        public T GetParam<T>(SerializableFieldInfo field)
        {
            for (int i = 0; i < Params.Count; ++i)
            {
                if (Params[i].MemberInfo.Equals(field))
                {
                    return (T) Params[i].MemberData;
                }
            }

            return default(T);
        }

        public bool SetParam(SerializableFieldInfo field, object data)
        {
            for (int i = 0; i < Params.Count; ++i)
            {
                if (Params[i].MemberInfo.Equals(field))
                {
                    throw new NotImplementedException();
                    //Params[i].MemberData = data; // TODO:
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
                {
                    throw new NotImplementedException();
                    //param.MemberData = replacement; // TODO:
                }

                if (param.MemberData is IUseReferenceGuid e)
                    e.ReplaceGuid(guid, replacement);
            }
        }

        public bool Register(ObjectMemberData member)
        {
            if (member == null)
                return false;
            
            if (Params == null)
                Params = new List<ObjectMemberData>();

            Params.AddUnique(member);
            return true;
        }

        public bool CopyDataTo(object instance)
        {
            if (instance == null)
                return false;

            if (Params == null)
                return true;

            var instanceType = instance.GetType();
            if (!ConditionType.Type.IsAssignableFrom(instanceType))
                return false;
            
            foreach (var param in Params)
            {
                var memberInfo = param.MemberInfo.GetMemberInfo();
                memberInfo.SetValue(instance, param.MemberData.GetValue()); // TODO: this won't work for containered data
            }
            return true;
        }
    }
}