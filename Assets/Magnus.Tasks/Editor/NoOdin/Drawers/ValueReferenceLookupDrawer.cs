using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
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

        private bool _expanded;

        private GUIContent _activeTab;
        private SearchablePagedDrawerHelper _pager;
        private SearchablePagedDrawerHelper _defaultsPager;

        private SerializableType _selectedDefaultFilter;

        private TypedHostInfoWrapper<List<ReferenceKey>> _keysValueEntry;
        // private InspectorProperty _selectedPropertyToInspect;
        // private InspectorProperty _keysProperty;
        // private InspectorProperty _resolversProperty;
        // private IPropertyValueEntry<Dictionary<SerializableGuid, IValueResolver>> _resolversValueEntry;
        // private InspectorProperty _defaultsProperty;
        // private IPropertyValueEntry<Dictionary<SerializableType, DefaultTypeReferenceKey[]>> _defaultsValueEntry;

        private Dictionary<ReferenceKey, IEditorDrawable> _resolverPropertyByKey;

        private List<int> _validKeyIds;
        private bool _requiresRefresh;

        protected override void OnInitialize()
        {
            base.OnInitialize();
            _expanded = true;
            _requiresRefresh = true;

            _activeTab = Tabs.Initial;

            _resolverPropertyByKey = new Dictionary<ReferenceKey, IEditorDrawable>();

            _pager = new SearchablePagedDrawerHelper(10, true);
            _pager.SearchTextChanged += UpdateKeyFilter;
            _defaultsPager = new SearchablePagedDrawerHelper(5);

            _validKeyIds = new List<int>();
        }

        private void InitSelectedProperty()
        {
            // _selectedPropertyToInspect = _defaultsProperty.Children.FirstOrDefault(childProp =>
            // {
            //     GetChildProperty<SerializableType>("Key", childProp, out var keyEntry);
            //     return keyEntry.SmartValue == _selectedDefaultFilter;
            // });
        }

        protected override void DrawProperty(Rect position, ref GenericHostInfo data, GUIContent label)
        {
            if (SmartValue == null)
            {
                _requiresRefresh = true;
                return;
            }

            if (_requiresRefresh)
            {
                HostInfo.TryGetChild(nameof(ValueReferenceLookup.Keys), out _keysValueEntry);
                // _resolversProperty = GetChildProperty(nameof(ValueReferenceLookup.ValueResolversByKey), out _resolversValueEntry);
                // _defaultsProperty = GetChildProperty(nameof(ValueReferenceLookup.DefaultsByType), out _defaultsValueEntry);
                //
                _selectedDefaultFilter = GetRegisteredReferenceKeyTypes().FirstOrDefault();
                // if (_defaultsProperty != null)
                //     InitSelectedProperty();

                UpdateKeyFilter(string.Empty);
                _requiresRefresh = false;
            }

            int oldIndex = Tabs.All.IndexOf(_activeTab);
            int newIndex = GUI.Toolbar(position.SetHeight(EditorGUIUtility.singleLineHeight), oldIndex, Tabs.All);
            if (newIndex != oldIndex)
                _activeTab = Tabs.All[newIndex];

            if (_expanded)
            {
                
                var pageRect = position.MoveDownLine(autoClamp: true);
                
                if (_activeTab == Tabs.ResolversTab)
                    DrawKeys(pageRect);
                else if (_activeTab == Tabs.DefaultsTab)
                    DrawDefaults(pageRect);
            }
        }

        // private void RefreshDefaultValueSet()
        // {
        //     var dictChild = GetChildProperty(nameof(ValueReferenceLookup.DefaultsByType));
        //     _selectedPropertyToInspect = dictChild.Children.FirstOrDefault(childProp =>
        //     {
        //         GetChildProperty<SerializableType>("Key", childProp, out var keyEntry);
        //         return keyEntry.SmartValue == _selectedDefaultFilter;
        //     });
        // }

        private IEditorDrawable FindResolverPropertyForKey(ReferenceKey key)
        {
            HostInfo.TryGetChild(nameof(ValueReferenceLookup.ValueResolversByKey),
                out TypedHostInfoWrapper<Dictionary<SerializableGuid, IValueResolver>> valueResolversByKey);
            valueResolversByKey.HostInfo.TryGetChild("Values", out var horribleHack);

            if (!valueResolversByKey.SmartValue.ContainsKey(key.Guid))
                return null;
            
            var actualValue = valueResolversByKey.SmartValue[key.Guid];
            for (int i = 0; i < valueResolversByKey.SmartValue.Count; ++i)
            {
                if (horribleHack.TryGetChild<IValueResolver>(i, out var horribleHackChild))
                {
                    if (!ReferenceEquals(horribleHackChild.SmartValue, actualValue))
                        continue;
                    return GetChildDrawer(horribleHackChild.HostInfo);
                }
                
            }

            return null;
            // valueResolversByKey.SmartValue.
            //
            // var pairProperty = _resolversProperty
            //     .FindChild(x => x.Children.Count > 0 && Equals(x.Children[0].ValueEntry.WeakSmartValue, key.Guid), false);
            // return pairProperty.Children[1];
        }

        private ICollection<SerializableType> GetRegisteredReferenceKeyTypes()
        {
            if (SmartValue == null) return Array.Empty<SerializableType>();
            return SmartValue.GetKeys().Select(x => x.ValueType).Distinct().ToArray();
        }

        private bool MatchesSearchString(ReferenceKey key, string searchText)
        {
            if (string.IsNullOrWhiteSpace(searchText)) return true;

            var keywords = searchText.Split(new[] {' ', ';', ','});
            foreach (var keyword in keywords)
            {
                var searchStr = keyword;
                var compareStr = "";
                if (keyword.StartsWith("t:"))
                {
                    searchStr = keyword.Substring(2);
                    compareStr = key.ValueType.Type.Name;
                }
                else
                {
                    compareStr = key.DisplayName;
                    var resolver = SmartValue.FindResolverByID(key.Guid);
                    object value = null;
                    resolver.TryResolve(ref value);
                    if (value != null)
                        compareStr += " " + value;
                }

                if (!compareStr.Contains(searchStr, StringComparison.InvariantCultureIgnoreCase))
                    return false;
            }

            return true;
        }

        protected override float GetPropertyHeight(GUIContent label, in GenericHostInfo data)
        {
            if (_expanded)
            {
                float tabHeaderHeight = EditorGUIUtility.singleLineHeight;

                if (_activeTab == Tabs.ResolversTab)
                {
                    float buttonFooter = EditorGUIUtility.singleLineHeight;
                    float elementHeight = 2.0f * EditorGUIUtility.singleLineHeight;
                    float visibleElements = elementHeight * _pager.VisibleLines;
                    float pageHeight = visibleElements + EditorGUIUtility.singleLineHeight;
                    return pageHeight + buttonFooter + tabHeaderHeight;
                }
                else if (_activeTab == Tabs.DefaultsTab)
                    return EditorGUIUtility.singleLineHeight;
            }
            return base.GetPropertyHeight(label, in data);
        }

        private void DrawKeys(Rect pageRect)
        {
            if (_keysValueEntry == null /* || _keysProperty == null*/)
            {
                EditorGUI.HelpBox(pageRect, "Cannot find keys property...", MessageType.Error);
                return;
            }

            object host = HostInfo.Parent != null ? HostInfo.Parent.GetValue() : null;

            if (_keysValueEntry.SmartValue == null)
                _keysValueEntry.SmartValue = new List<ReferenceKey>();

            // Leave space for buttons at end
            if (pageRect.IsValid())
                pageRect.yMax -= EditorGUIUtility.singleLineHeight;
            
            _pager.BeginDrawPager(pageRect, _validKeyIds);

            int elementCount = _pager.EndIndex - _pager.StartIndex;
            var elementRect = pageRect.BeginList(elementCount);
            
            for (int elementIndex = _pager.StartIndex; elementIndex < _pager.EndIndex; ++elementIndex)
            {
                if (!_validKeyIds.HasIndex(elementIndex)) 
                    continue;

                var i = _validKeyIds[elementIndex];
                var key = _keysValueEntry.SmartValue[i];

                
                if (!_resolverPropertyByKey.ContainsKey(key) || _resolverPropertyByKey[key] == null)
                    _resolverPropertyByKey[key] = FindResolverPropertyForKey(key);

                var resolverProp = _resolverPropertyByKey[key];
                
                var elementFirstLine = elementRect.SetHeight(EditorGUIUtility.singleLineHeight);    

                //_keysProperty.Children[i].Draw();
                var elementSecondLine = elementFirstLine.MoveDownLine();
                if (resolverProp != null)
                    resolverProp.Draw(elementSecondLine, GUIContent.none);

                var iconRect = elementSecondLine.AlignRight(18.0f);
                // Delete
                if (CustomEditorGUI.IconButton(iconRect, UnityIcon.AssetIcon("Fa_Times")))
                {
                    SmartValue.Deregister(_keysValueEntry.SmartValue[i].Guid);
                    RefreshData();
                }

                    // Check usages
//                         var hasUsageData = HostInfo.Parent.GetReturnType()
//                             .EqualsOneOf(typeof(TaskObject), typeof(TaskEditViewPage));
//                         if (hasUsageData && host != null &&
//                             CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Asterisk")))
//                         {
//                             var usagesInfo = new ValueReferenceInfo(host, _keysValueEntry.SmartValue[i].Guid);
//                             usagesInfo.MakeUsages();
//                             // TODO: how to inspect
// #if ODIN_INSPECTOR
//                         OdinEditorWindow.InspectObjectInDropDown(usagesInfo);
// #endif
//                         }

                elementRect = elementRect.MoveNext(pageRect);
            }

            _pager.EndDrawPager();



            var keylistButtonsRect = pageRect.MoveBeneath(EditorGUIUtility.singleLineHeight);
            DrawKeyListButtons(keylistButtonsRect, host);
            //SirenixEditorGUI.EndVerticalList();

        }

        private void DrawKeyListButtons(Rect buttonRect, object host)
        {
            buttonRect.SplitX(buttonRect.width / 2.0f, out Rect addRect, out Rect otherRect);
            if (GUI.Button(addRect, "Add new Key", CustomGUIStyles.ButtonLeft))
            {
                EditorInputDialog.Create("Create ReferenceKey", "Input the default name for the ReferenceKey:",
                        "Add New Key")
                    .TextField(null, out var name)
                    .Dropdown(null, GetReferenceKeyTypeOptions(), t => t.Name, out var type)
                    .OnAccept(() =>
                    {
                        SmartValue.Register(name, type); // TODO: Test duplicate key + add logging
                        HostInfo.ForceNotifyValueChanged();
                        RefreshData();
                    })
                    .ShowInPopup();
            }

            var content = GUIContentHelper.TempContent("Simplify",
                "If multiple keys refer to the same object, condenses those into 1 and rectifies all usages.");
            if (host != null && GUI.Button(otherRect, content, CustomGUIStyles.ButtonRight))
            {
                var replacementKeyByKey = new Dictionary<SerializableGuid, SerializableGuid>();
                var keyResolverPairToKeep = new PairList<SerializableGuid, IValueResolver>();
                foreach (var (guid, resolver) in SmartValue.ValueResolversByKey)
                {
                    // Find a resolver that matches the given resolver
                    var matchedPairI = keyResolverPairToKeep.FindIndex(pair => pair.V2 != null && pair.V2.Equals(resolver));

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
                    PLog.Info<MagnusLogger>($"Removing {replacementKeyByKey.Count} duplicate Resolvers...");
                    foreach (var (guid, replacementGuid) in replacementKeyByKey)
                    {
                        var usagesInfo = new ValueReferenceInfo(host, guid);
                        usagesInfo.ReplaceUsages(replacementGuid);
                        SmartValue.Deregister(guid);
                    }

                    RefreshData();
                    PLog.Info<MagnusLogger>($"Resolvers successfully optimized!");
                }
                else
                {
                    PLog.Info<MagnusLogger>("Nothing to simplify...");
                }
            }
        }

        private void RefreshData()
        {
            UpdateKeyFilter(_pager.SearchText);
            _pager.Refresh();
        }

        private void UpdateKeyFilter(string searchText)
        {
            _validKeyIds.Clear();

            if (_keysValueEntry.SmartValue == null)
                _keysValueEntry.SmartValue = new List<ReferenceKey>();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                // fill list with all IDs
                _validKeyIds.AddRange(Enumerable.Range(0, _keysValueEntry.SmartValue.Count));
                return;
            }

            for (int i = 0; i < _keysValueEntry.SmartValue.Count; ++i)
            {
                if (MatchesSearchString(_keysValueEntry.SmartValue[i], searchText))
                    _validKeyIds.Add(i);
            }
        }

        private void DrawDefaults(Rect position)
        {
            // if (_defaultsValueEntry == null || _defaultsProperty == null)
            // {
            //     EditorGUI.HelpBox(position, "Cannot find defaults property...", MessageType.Error);
            //     return;
            // }
            //
            // if (_defaultsSmartValue == null)
            //     _defaultsSmartValue = new Dictionary<SerializableType, DefaultTypeReferenceKey[]>();

            Rect labelRect = default, fieldRect = default;
            if (position.IsValid())
                position.SplitX(0.5f * position.width, out labelRect, out fieldRect);
            GUI.Label(labelRect, "Choose a ReferenceKey Type:");
            
            if (GUI.Button(fieldRect, _selectedDefaultFilter != null ? _selectedDefaultFilter.Name : "null"))
            {
                GenericPicker.Show(position, GetRegisteredReferenceKeyTypes(), (x) =>
                {
                    _selectedDefaultFilter = x;
                    //RefreshDefaultValueSet();
                });
                // DrawDropdown(position, "", GetRegisteredReferenceKeyTypes(), (x) =>
                // {
                //     _selectedDefaultFilter = x;
                //     //RefreshDefaultValueSet();
                // });
            }
        
            // if (_selectedPropertyToInspect != null)
            // {
            //     var property = GetChildProperty("#Value", _selectedPropertyToInspect);
            //     _defaultsPager.BeginDrawPager(_defaultsSmartValue[_selectedDefaultFilter]);
            //     
            //     for (int i = _defaultsPager.StartIndex; i < _defaultsPager.EndIndex; ++i)
            //     {
            //         if (i >= property.Children.Count)
            //         {
            //             // NOTE: refresh needed, when deleting items and reading otherwise drawer is not up to date
            //             RefreshDefaultValueSet();
            //             property = GetChildProperty("#Value", _selectedPropertyToInspect);
            //             if (i >= property.Children.Count)
            //                 continue;
            //         }
            //
            //         var child = property.Children[i];
            //         EditorGUILayout.BeginHorizontal();
            //         EditorGUILayout.BeginVertical();
            //         child.Draw(null);
            //         EditorGUILayout.EndVertical();
            //         if (GUILayout.Button("X", EditorStyles.toolbarButton))
            //         {
            //             // Remove
            //             SmartValue.RemoveDefault(_selectedDefaultFilter, ((DefaultTypeReferenceKey)child.ValueEntry.WeakSmartValue).FieldData.Field);
            //             _defaultsPager.Refresh();
            //         }
            //         EditorGUILayout.EndHorizontal();
            //     }
            //
            //     _defaultsPager.EndDrawPager();
            // }
        }

        private ICollection<Type> GetReferenceKeyTypeOptions()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => !x.IsAbstract)
                .Where(x => !x.IsGenericTypeDefinition)
                .Where(x => ReflectionUtility.IsSimpleType(x) || typeof(Object).IsAssignableFrom(x))
                .ToArray();
            return types;
        }

    }
}