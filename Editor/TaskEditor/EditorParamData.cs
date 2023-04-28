#if ODIN_INSPECTOR
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

namespace Rhinox.VOLT.Editor
{
    
    [HideReferenceObjectPicker]
    public class EditorParamData<T> : ParamData
    {
        [HideDuplicateReferenceBox, OnValueChanged(nameof(SetMemberData))]
        public T SmartValue;

        public static EditorParamData<T> Create(ParamData pd)
        {
            if (pd == null)
                return null;

            var memberData = SerializationUtility.CreateCopy(pd.MemberData);
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
#endif