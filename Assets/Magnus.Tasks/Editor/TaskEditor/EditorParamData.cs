using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Rhinox.Utilities;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Rhinox.Magnus.Tasks.Editor
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
}