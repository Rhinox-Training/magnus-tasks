using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed.Reflection;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    [CustomPropertyDrawer(typeof(ValueReferenceLookup))]
    public class ValueReferenceLookupDrawer : BasePropertyDrawer<ValueReferenceLookup>
    {
        private static class Tabs
        {
            public static readonly GUIContent ResolversTab = new GUIContent("Value Resolvers");
            public static readonly GUIContent DefaultsTab = new GUIContent("Defaults");

            public static readonly GUIContent Initial = ResolversTab;
        
            public static readonly GUIContent[] All = new []
            {
                ResolversTab,
                DefaultsTab
            };
        }

        private GUIContent _activeTab = null;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _activeTab = Tabs.Initial;
        }

        private int _index = 0;

        protected override float GetPropertyHeight(GUIContent label, in GenericHostInfo data)
        {
            return 0;
            return base.GetPropertyHeight(label, in data);
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            //throw new System.NotImplementedException();

            //foreach (var tab in Tabs.All)
            //{

            GUILayout.BeginVertical(CustomGUIStyles.Clean);
            //EditorGUILayout.BeginFadeGroup()
            _index = GUILayout.Toolbar(_index, Tabs.All);
            
            

            //for (var i = 0; i < _drawableMemberChildren.Count; i++)
            //{
            //    var childDrawable = _drawableMemberChildren[i];
  
            //    if (childDrawable == null || !childDrawable.IsVisible)
            //        continue;
  
            //    if (!_dic[activeTab].Contains(childDrawable)) continue;

            //    childDrawable.Draw(childDrawable.Label);

            //    if (_drawableMemberChildren.Count - 1 != i)
            //        GUILayout.Space(CustomGUIUtility.Padding); // padding
            //}

            GUILayout.EndVertical();
            //}
        }

        
    }
}