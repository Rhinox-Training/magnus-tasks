// using System;
// using System.Linq;
// using System.Reflection;
// using System.Reflection.Emit;
// using Rhinox.GUIUtils.Attributes;
// using Rhinox.Lightspeed.Reflection;
// using Rhinox.Magnus.Tasks;
// using Sirenix.OdinInspector;
// using UnityEditor;
// using UnityEngine;
//
// namespace Rhinox
// {
//     public static class DynamicTypeBuilder
//     {
//         private static readonly AssemblyBuilder _assembly;
//         private static readonly ModuleBuilder _module;
//         private static readonly object _syncBlk = new object();
//
//         static DynamicTypeBuilder()
//         {
//             _assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName("com.rhinox.open.magnus.tasks.dynamic-serialization"), AssemblyBuilderAccess.Run);
//             _module = _assembly.DefineDynamicModule("DataModule");
//         }
//
//         public static TypeBuilder CreateTypeBuilder(string typeName, Type baseType, params Type[] genericArguments)
//         {
//             if (baseType == null)
//                 return null;
//             
//             lock (_syncBlk)
//             {
//                 if (baseType.IsGenericType && baseType.IsGenericTypeDefinition)
//                     baseType = baseType.MakeGenericType(genericArguments);
//                 
//                 var typeBuilder
//                     = _module.DefineType(
//                         typeName,
//                         TypeAttributes.Public
//                         | TypeAttributes.Class
//                         | TypeAttributes.AutoClass
//                         | TypeAttributes.AnsiClass
//                         | TypeAttributes.ExplicitLayout,
//                         baseType);
//                 
//                 
//                 typeBuilder.DefineDefaultConstructor(MethodAttributes.Public);
//                 return typeBuilder;
//             }
//         }
//         
//         public static CustomAttributeBuilder BuildCustomAttribute(System.Attribute attribute)
//         {
//             Type type = attribute.GetType();
//             var constructor = type.GetConstructor(Type.EmptyTypes);
//             var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
//             var fields = type.GetFields(BindingFlags.Public | BindingFlags.Instance);
//
//             var propertyValues = from p in properties
//                 select p.GetValue(attribute, null);
//             var fieldValues = from f in fields
//                 select f.GetValue(attribute);
//
//             return new CustomAttributeBuilder(constructor, 
//                 Type.EmptyTypes,
//                 properties,
//                 propertyValues.ToArray(),
//                 fields,
//                 fieldValues.ToArray());
//         }
//     }
//     
//     [SmartFallbackDrawn]
//     public class ILGenerationTest : MonoBehaviour
//     {
//         [AssignableTypeFilter, SerializeReference]
//         public MemberContainer Data;
//         
//         [InitializeOnLoadMethod]
//         static void OnProjectLoadedInEditor()
//         {
//             
//             AssemblyReloadEvents.afterAssemblyReload -= OnBeforeAssemblyReload;
//             AssemblyReloadEvents.afterAssemblyReload += OnBeforeAssemblyReload;
//         }
//
//         private static void OnBeforeAssemblyReload()
//         {
//             Debug.Log("Project loaded in Unity Editor");
//             GenerateClass();
//         }
//
//         [Button]
//         public static void GenerateClass()
//         {
//             var typeBuilder = DynamicTypeBuilder.CreateTypeBuilder("WOLOLO", typeof(ParamData<>), typeof(bool));
//             
//             typeBuilder.SetCustomAttribute(new CustomAttributeBuilder(
//                 typeof(SerializableAttribute).GetConstructor(Type.EmptyTypes), 
//                 Type.EmptyTypes, 
//                 new FieldInfo[0], 
//                 new object[0]));
//             //var attrBuilder = DynamicTypeBuilder.BuildCustomAttribute(new SerializableAttribute());
//             //typeBuilder.SetCustomAttribute(attrBuilder);
//             typeBuilder.CreateType();
//         }
//
//         [Button]
//         public void PrintInheritingClasses()
//         {
//             Debug.Log("[START PARAM]");
//             foreach (var type in AppDomain.CurrentDomain.GetDefinedTypesOfType(typeof(ParamData<>)))
//             {
//                 Debug.Log($"\t{type.AssemblyQualifiedName}");
//             }
//             Debug.Log("[END PARAM]");
//         }
//
//         [Button]
//         public void PrintInheritingClasses2()
//         {
//             Debug.Log("[START PARAM]");
//             foreach (var type in TypeCache.GetTypesDerivedFrom(typeof(ParamData<>)))
//             {
//                 Debug.Log($"\t{type.AssemblyQualifiedName}");
//             }
//             Debug.Log("[END PARAM]");
//         }
//     }
// }