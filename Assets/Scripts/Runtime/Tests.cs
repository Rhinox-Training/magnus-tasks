
using System;
using System.Collections.Generic;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Magnus.Tasks;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;



namespace Rhinox
{
    
    [Serializable] public class TestDic : SerializableDictionary<int, string> {}
    [Serializable] public class KeyDictionary : SerializableDictionary<SerializableType, DefaultTypeReferenceKey[]> {}

    [SmartFallbackDrawn]
    [ExecuteAlways]
    public class Tests : MonoBehaviour
    {
        [SerializeField]  private ValueReferenceLookup _l;
        
        [SerializeField] private SerializableType t;
        [SerializeField] public KeyDictionary Dic = new KeyDictionary();
        [SerializeField] public TestDic TestDicd = new TestDic();

        //[SerializeField] public TestDic _dic;

        //[SerializeField] private int _int;
        //[Serializable] public class SerialzableTest : SerializableDictionary<int, string>{}
        [SerializeField] private int _i;
        [SerializeField] private Dictionary<int, string> _d;

        private void OnEnable()
        {
            Debug.Log("Test Enabled");
        }
    }


    [CustomEditor(typeof(Tests))]
    public class TestEditor : Editor
    {
        private void OnEnable()
        {
            var prop = serializedObject.FindProperty("_d");
            Debug.Log(prop == null ? "prop null" : "prop not null");
        }
    }
}
