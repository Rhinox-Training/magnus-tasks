using System;
using System.Text;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Sirenix.OdinInspector;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
using Sirenix.Serialization;
using SerializationUtility = Sirenix.Serialization.SerializationUtility;
#endif
using UnityEditor;

// unity 2021.3 has introduced their own SerializationUtility, so scope it

namespace Rhinox.Magnus.Tasks.Editor
{
    public class ValueRefTestEditorWindow : CustomEditorWindow
    {
        [MenuItem("Rhinox/Value References Test")]
        public static void GetWindow()
        {
            var window = EditorWindow.GetWindow<ValueRefTestEditorWindow>();
            window.Show();
        }

        [OnValueChanged(nameof(ParseCondition)), AssignableTypeFilter]
        public BaseCondition Condition;

        public object ConditionData;

        public ValueReferenceLookup Lookup;


        private void ParseCondition()
        {
            if (Condition == null)
            {
                ConditionData = null;
                return;
            }

            ConditionData = ConditionDataHelper.FromCondition(Condition);
        }

#if ODIN_INSPECTOR
    [Button]
    private void Serialize()
    {
        if (Lookup == null)
        {
            LookUpSerialized = string.Empty;
            return;
        }
        
        byte[] json = SerializationUtility.SerializeValue(new [] {Lookup}, DataFormat.JSON);
        LookUpSerialized = ASCIIEncoding.UTF8.GetString(json);

    }
#endif

        [MultiLineProperty(20)] public string LookUpSerialized;
    }
}