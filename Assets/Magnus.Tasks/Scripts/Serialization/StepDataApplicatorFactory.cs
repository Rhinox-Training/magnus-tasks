using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    public class RegisterApplicatorAttribute : Attribute
    {
        public Type DataType;

        public RegisterApplicatorAttribute(Type dataType)
        {
            DataType = dataType;
        }
    }
    
    /// <summary>
    /// Unfortunately the StepData is located in an assembly where there is no reference of BaseStep etc
    /// This makes it harder to create an overridable apply data thing; This is a workaround for that
    /// </summary>
    public static class StepDataApplicatorFactory
    {
        private static Dictionary<Type, Type> _applicatorByDataType;
        private static bool _initialized;
        
        private static readonly MethodInfo CreateGenericMethod = typeof(StepDataApplicatorFactory)
            .GetGenericMethod(nameof(CreateApplicator), BindingFlags.Static | BindingFlags.Public);

        private static void Init()
        {
            var stepDataApplicators = ReflectionUtility.GetTypesInheritingFrom(typeof(IStepDataApplicator<>));
            
            _applicatorByDataType = new Dictionary<Type, Type>();
            
            foreach (var applicatorType in stepDataApplicators)
            {
                var attribute = applicatorType.GetCustomAttribute<RegisterApplicatorAttribute>();
                
                if (attribute != null)
                    _applicatorByDataType[attribute.DataType] = applicatorType;
            }

            _initialized = true;
        }

        public static bool CreateApplicator(object data, out IStepDataApplicator applicator)
        {
            var dataType = data.GetType();
            var createTypedApplicator = CreateGenericMethod.MakeGenericMethod(dataType);
            var parameters = new[] {data, null};
            bool result = (bool) createTypedApplicator.Invoke(null, parameters);
            applicator = parameters[1] as IStepDataApplicator; // Retrieve the 'out' parameter
            return result;
        }

        public static bool CreateApplicator<T>(T data, out IStepDataApplicator<T> applicator)
        {
            if (!_initialized)
                Init();
            
            if (!_applicatorByDataType.TryGetValue(data.GetType(), out Type applicatorType))
            {
                applicator = null;
                return false;
            }
            
            applicator = (IStepDataApplicator<T>) Activator.CreateInstance(applicatorType);
            applicator.Init(data);
            return true;
        }
    }
}