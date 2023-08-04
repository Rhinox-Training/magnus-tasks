using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Perceptor;
using Rhinox.VOLT.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    [CustomPropertyDrawer(typeof(ValueReferenceLookup))]
    public class ValueReferenceLookupDrawer : BasePropertyDrawer<ValueReferenceLookup>
    {
        public class ValueReferenceInfo
        {
            public SerializableGuid Guid;

            public List<IUseReferenceGuid> PotentialUsers;
            public IUseReferenceGuid[] Users;

            [UnfoldList, DisplayAsString] public List<string> Usages;

            public ValueReferenceInfo(object target, SerializableGuid key)
            {
                PotentialUsers = new List<IUseReferenceGuid>();
                Usages = new List<string>();
                Guid = key;

                switch (target)
                {
                    case TaskEditViewPage taskEditor:
                        PotentialUsers.AddRange(taskEditor.Steps);
                        break;
                    case TaskObject task:
                        PotentialUsers.AddRange(task.Steps);
                        break;
                }

                Users = PotentialUsers.Where(x => x.UsesGuid(Guid)).ToArray();
            }

            public void MakeUsages()
            {
                Usages.Clear();

                for (var i = 0; i < Users.Length; i++)
                {
                    int stepIndex = PotentialUsers.IndexOf(Users[i]);
                    Usages.Add($"Used in Step {stepIndex + 1}");
                }

                if (!Usages.Any())
                    Usages.Add("No usages found...");
            }

            public void ReplaceUsages(SerializableGuid replacement)
            {
                for (var i = 0; i < Users.Length; i++)
                    Users[i].ReplaceGuid(Guid, replacement);
            }
        }

        private static class Tabs
        {
            public static readonly GUIContent ResolversTab = new GUIContent("Value Resolvers");
            public static readonly GUIContent DefaultsTab = new GUIContent("Defaults");

            public static readonly GUIContent Initial = ResolversTab;

            public static readonly GUIContent[] All = new[]
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
            _pager = new BetterGUIPagingHelper(10, true);
            _pager.SearchTextChanged += UpdateKeyFilter;
            _defaultsPager = new BetterGUIPagingHelper(5);
            _validKeyIds = new List<int>();


            //_resolversProperty = new DrawablePropertyView(SmartValue.ValueResolversByKey);
        }

        private void UpdateKeyFilter(string searchtext)
        {
        }

        private int _index = 0;
        private BetterGUIPagingHelper _pager;
        private BetterGUIPagingHelper _defaultsPager;
        private SerializableType _selectedDefaultFilter;
        private List<int> _validKeyIds;
        private Dictionary<ReferenceKey, DrawablePropertyView> _resolverPropertyByKey;
        private DrawablePropertyView _resolversProperty;

        protected override float GetPropertyHeight(GUIContent label, in GenericHostInfo data)
        {
            if (SmartValue == null)
                return base.GetPropertyHeight(label, in data);
            return SmartValue.ValueResolversByKey.Count * EditorGUIUtility.singleLineHeight;
            return 0; // TODO: whut
            return base.GetPropertyHeight(label, in data);
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            GUILayout.BeginVertical(CustomGUIStyles.Clean);
            {
                _index = GUILayout.Toolbar(_index, Tabs.All);
                _activeTab = Tabs.All[_index];

                if (SmartValue == null)
                {
                    SmartValue = new ValueReferenceLookup();
                    //Apply();
                }

                if (_activeTab == Tabs.ResolversTab)
                    DrawKeys(SmartValue.Keys);
                else if (_activeTab == Tabs.DefaultsTab)
                    DrawDefaults(SmartValue.DefaultsByType);
            }
            GUILayout.EndVertical();
        }

        private ICollection<SerializableType> GetRegisteredReferenceKeyTypes()
        {
            if (SmartValue == null)
                return Array.Empty<SerializableType>();
            return SmartValue.GetKeys().Select(x => x.ValueType).Distinct().ToArray();
        }

        private void DrawDefaults(Dictionary<SerializableType, DefaultTypeReferenceKey[]> defaultsValueEntry)
        {
            if (defaultsValueEntry == null)
            {
                EditorGUILayout.HelpBox("Cannot find defaults property...", MessageType.Error);
                return;
            }

            if (defaultsValueEntry == null)
                defaultsValueEntry = new Dictionary<SerializableType, DefaultTypeReferenceKey[]>();

            Rect rect = EditorGUILayout.GetControlRect();
            rect.SplitX(rect.width * 0.4f, out Rect labelRect, out Rect fieldRect);
            GUI.Label(labelRect, "Choose a ReferenceKey Type:");

            if (GUI.Button(fieldRect, _selectedDefaultFilter != null ? _selectedDefaultFilter.Name : "null"))
            {
                var genericMenu = new GenericMenu();
                foreach (var entry in GetRegisteredReferenceKeyTypes())
                {
                    genericMenu.AddItem(entry, (x) =>
                    {
                        _selectedDefaultFilter = x;
                        //RefreshDefaultValueSet(); // TODO:
                    });
                }

                genericMenu.DropDown(rect);
            }

            if (_selectedDefaultFilter != null)
            {
                //var property = GetChildProperty("#Value", _selectedPropertyToInspect);
                _defaultsPager.BeginDrawPager(defaultsValueEntry[_selectedDefaultFilter]);

                for (int i = _defaultsPager.StartIndex; i < _defaultsPager.EndIndex; ++i)
                {
                    // TODO:
                    // if (i >= property.Children.Count)
                    // {
                    //     // NOTE: refresh needed, when deleting items and reading otherwise drawer is not up to date
                    //     RefreshDefaultValueSet();
                    //     property = GetChildProperty("#Value", _selectedPropertyToInspect);
                    //     if (i >= property.Children.Count)
                    //         continue;
                    // }

                    //var child = property.Children[i];
                    EditorGUILayout.BeginHorizontal();
                    // EditorGUILayout.BeginVertical();
                    // child.Draw(null);
                    // EditorGUILayout.EndVertical();
                    if (GUILayout.Button("X", EditorStyles.toolbarButton))
                    {
                        // Remove
                        // TODO:
                        //SmartValue.RemoveDefault(_selectedDefaultFilter,
                        //   ((DefaultTypeReferenceKey) child.ValueEntry.WeakSmartValue).FieldData.Field);
                        _defaultsPager.Refresh();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                _defaultsPager.EndDrawPager();
            }
        }

        private DrawablePropertyView FindResolverPropertyForKey(ReferenceKey key)
        {
            return null;
            //     var pairProperty = _resolversProperty
            //         .FindChild(x => x.Children.Count > 0 && Equals(x.Children[0].ValueEntry.WeakSmartValue, key.Guid),
            //             false);
            //     return pairProperty.Children[1];
        }

        private void DrawKeys(List<ReferenceKey> keyValues)
        {
            if (keyValues == null)
            {
                EditorGUILayout.HelpBox("Cannot find keys property...", MessageType.Error);
                return;
            }

            //var host = Property.ParentValues.FirstOrDefault();

            //SirenixEditorGUI.BeginVerticalList(false, false);
            EditorGUILayout.BeginVertical();
            {
                var toolbarRect = _pager.BeginDrawPager(_validKeyIds);

                for (int pageI = _pager.StartIndex; pageI < _pager.EndIndex; ++pageI)
                {
                    if (!_validKeyIds.HasIndex(pageI))
                        continue;

                    var i = _validKeyIds[pageI];
                    var key = keyValues[i];

                    //SirenixEditorGUI.BeginListItem();
                    EditorGUILayout.BeginVertical();

                    using (new EditorGUILayout.HorizontalScope())
                    {
                        // TODO:
                        if (!_resolverPropertyByKey.ContainsKey(key))
                            _resolverPropertyByKey[key] = FindResolverPropertyForKey(key);
                        //
                        // var resolverProp = _resolverPropertyByKey[key];
                        //
                        // using (new EditorGUILayout.VerticalScope())
                        // {
                        //     _keysProperty.Children[i].Draw();
                        //     resolverProp.Draw();
                        // }

                        using (new EditorGUILayout.VerticalScope(GUILayout.Width(20)))
                        {
                            // Delete
                            if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Times")))
                            {
                                SmartValue.Deregister(keyValues[i].Guid);
                                Apply();
                            }

                            // Check usages TODO
                            // var hasUsageData = Property.ParentType.EqualsOneOf(typeof(TaskObject), typeof(TaskEditViewPage));
                            // if (hasUsageData && host != null &&
                            //     CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Asterisk")))
                            // {
                            //     var usagesInfo = new ValueReferenceInfo(host, keyValues[i].Guid);
                            //     usagesInfo.MakeUsages();
                            //     OdinEditorWindow.InspectObjectInDropDown(usagesInfo);
                            // }
                        }
                    }

                    //SirenixEditorGUI.EndListItem();
                    EditorGUILayout.EndVertical();
                }

                // TODO
                DrawKeyListButtons(null);

                _pager.EndDrawPager();
            }
            EditorGUILayout.EndVertical();
            //SirenixEditorGUI.EndVerticalList();
        }

        private void DrawKeyListButtons(object host)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add new Key", CustomGUIStyles.ButtonLeft))
            {
                EditorInputDialog.Create("Create ReferenceKey", "Input the default name for the ReferenceKey:",
                        "Add New Key")
                    .TextField(null, out var name)
                    .Dropdown(null, GetReferenceKeyTypeOptions(), t => t.Name, out var type)
                    .OnAccept(() =>
                    {
                        SmartValue.Register(name, type); // TODO: Test duplicate key + add logging
                        // TODO:
                        //RefreshData();
                    })
                    .ShowInPopup();
            }

            var content = GUIContentHelper.TempContent("Simplify",
                "If multiple keys refer to the same object, condenses those into 1 and rectifies all usages.");
            if (host != null &&
                GUILayout.Button(content, CustomGUIStyles.ButtonRight, GUILayout.ExpandWidth(false)))
            {
                CleanupDuplicateKeys(host);
            }

            GUILayout.EndHorizontal();
        }

        private void CleanupDuplicateKeys(object host)
        {
            var replacementKeyByKey = new Dictionary<SerializableGuid, SerializableGuid>();
            var keyResolverPairToKeep = new PairList<SerializableGuid, IValueResolver>();
            foreach (var (guid, resolver) in SmartValue.ValueResolversByKey)
            {
                // Find a resolver that matches the given resolver
                var matchedPairI = keyResolverPairToKeep.FindIndex(pair => pair.V2.Equals(resolver));

                if (matchedPairI >= 0)
                {
                    // If we find one, mark it as a replacement
                    var replacementKey = keyResolverPairToKeep[matchedPairI].V1;
                    replacementKeyByKey.Add(guid, replacementKey);
                }
                else
                {
                    // Else, mark it as an original
                    keyResolverPairToKeep.Add(guid, resolver);
                }
            }

            if (replacementKeyByKey.Any())
            {
                PLog.Info($"Removing {replacementKeyByKey.Count} duplicate Resolvers...");
                foreach (var (guid, replacementGuid) in replacementKeyByKey)
                {
                    var usagesInfo = new ValueReferenceInfo(host, guid);
                    usagesInfo.ReplaceUsages(replacementGuid);
                    SmartValue.Deregister(guid);
                }

                //TODO:?
                //RefreshData();
                PLog.Info($"Resolvers successfully optimized!");
            }
            else
            {
                PLog.Info("Nothing to simplify...");
            }
        }


        private ICollection<Type> GetReferenceKeyTypeOptions()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => ReflectionUtility.IsSimpleType(x) || typeof(UnityEngine.Object).IsAssignableFrom(x))
                .ToArray();
            return types;
        }
    }
}