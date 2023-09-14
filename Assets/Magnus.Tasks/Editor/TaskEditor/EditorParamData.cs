using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.VOLT.Editor
{
    public static class Cloner
    {
        private static Dictionary<Type, Func<object, object>> cloners = new Dictionary<Type, Func<object, object>>();

        private static Func<object, object> CreateCloner(Type getType)
        {
            var cloneMethod = new DynamicMethod("CloneImplementation", getType, new Type[] { getType }, true);
            var defaultCtor = getType.GetConstructor(new Type[] { });

            var generator = cloneMethod .GetILGenerator();

            var loc1 = generator.DeclareLocal(getType);

            generator.Emit(OpCodes.Newobj, defaultCtor);
            generator.Emit(OpCodes.Stloc, loc1);

            foreach (var field in getType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                generator.Emit(OpCodes.Ldloc, loc1);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Stfld, field);
            }

            generator.Emit(OpCodes.Ldloc, loc1);
            generator.Emit(OpCodes.Ret);

            return ((Func<object, object>)cloneMethod.CreateDelegate(typeof(Func<object, object>)));
        }

        public static object Clone(object myObject)
        {
            if (!cloners.ContainsKey(myObject.GetType()))
                cloners.Add(myObject.GetType(), CreateCloner(myObject.GetType()));
            return cloners[myObject.GetType()](myObject);
        }
    }
    
    public static class Cloner<T>
    {
        private static Func<T, T> cloner = CreateCloner();

        private static Func<T, T> CreateCloner()
        {
            var cloneMethod = new DynamicMethod("CloneImplementation", typeof(T), new Type[] { typeof(T) }, true);
            var defaultCtor = typeof(T).GetConstructor(new Type[] { });

            var generator = cloneMethod .GetILGenerator();

            var loc1 = generator.DeclareLocal(typeof(T));

            generator.Emit(OpCodes.Newobj, defaultCtor);
            generator.Emit(OpCodes.Stloc, loc1);

            foreach (var field in typeof(T).GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
            {
                generator.Emit(OpCodes.Ldloc, loc1);
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, field);
                generator.Emit(OpCodes.Stfld, field);
            }

            generator.Emit(OpCodes.Ldloc, loc1);
            generator.Emit(OpCodes.Ret);

            return ((Func<T, T>)cloneMethod.CreateDelegate(typeof(Func<T, T>)));
        }

        public static T Clone(T myObject)
        {
            return cloner(myObject);
        }
    }
    
    [HideReferenceObjectPicker]
    public class EditorParamData<T> : ParamData
    {
        [HideDuplicateReferenceBox, OnValueChanged(nameof(SetMemberData))]
        public T SmartValue;

        public static EditorParamData<T> Create(ParamData pd)
        {
            if (pd == null)
                return null;

#if ODIN_INSPECTOR
            var memberData = Sirenix.Serialization.SerializationUtility.CreateCopy(pd.MemberData);
#else
            var memberData = Cloner.Clone(pd.MemberData);
#endif
            var editorParam = new EditorParamData<T>()
            {
                Name = pd.Name,
                Flags = pd.Flags,
                MemberType = pd.MemberType,
                Type = pd.Type,
                MemberData = memberData,
                SmartValue = (T) memberData
            };
            return editorParam;
        }

        private void SetMemberData()
        {
            MemberData = SmartValue;
        }
    }

    public static class EditorParamDataHelper
    {
        public static void ConvertToEditor(ref ConditionData data)
        {
            data.Params = Convert(data.Params).ToArray();
        }
        
        public static void RevertFromEditor(ref ConditionData data)
        {
            data.Params = data.Params.Select(x => x.Clone()).ToArray();
        }
        
        public static ICollection<ParamData> Convert(ICollection<ParamData> paramDatas)
        {
            List<ParamData> result = new List<ParamData>();
            foreach (var originalPD in paramDatas)
                result.Add(Convert(originalPD));
            
            return result;
        }

        public static ParamData Convert(ParamData originalPD)
        {
            if (originalPD.GetType() == typeof(EditorParamData<>) || originalPD.Type == null)
                return originalPD;

            var editorType = typeof(EditorParamData<>).MakeGenericType(originalPD.Type);

            var methodInfo = editorType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

            return methodInfo.Invoke(null, new[] {originalPD}) as ParamData;
        }
    }
}