using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Odin.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Collections;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using GUILayoutOptions = Sirenix.Utilities.GUILayoutOptions;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks.Editor.Odin
{
    public class ValueReferenceLookupDrawer : SimpleOdinValueDrawer<ValueReferenceLookup>
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
        private InspectorProperty _selectedPropertyToInspect;

        private InspectorProperty _keysProperty;
        private IPropertyValueEntry<List<ReferenceKey>> _keysValueEntry;
        private InspectorProperty _resolversProperty;
        private IPropertyValueEntry<Dictionary<SerializableGuid, IValueResolver>> _resolversValueEntry;
        private InspectorProperty _defaultsProperty;
        private IPropertyValueEntry<Dictionary<SerializableType, DefaultTypeReferenceKey[]>> _defaultsValueEntry;

        private Dictionary<ReferenceKey, InspectorProperty> _resolverPropertyByKey;

        private List<int> _validKeyIds;
        private bool _requiresRefresh;

        protected override void Initialize()
        {
            _expanded = true;
            _requiresRefresh = true;

            _activeTab = Tabs.Initial;

            _resolverPropertyByKey = new Dictionary<ReferenceKey, InspectorProperty>();

            _pager = new SearchablePagedDrawerHelper(10, true);
            _pager.SearchTextChanged += UpdateKeyFilter;
            _defaultsPager = new SearchablePagedDrawerHelper(5);

            _validKeyIds = new List<int>();
        }

        private void InitSelectedProperty()
        {
            _selectedPropertyToInspect = _defaultsProperty.Children.FirstOrDefault(childProp =>
            {
                GetChildProperty<SerializableType>("Key", childProp, out var keyEntry);
                return keyEntry.SmartValue == _selectedDefaultFilter;
            });
        }

        protected override void OnCustomDrawPropertyLayout(GUIContent label,
            IPropertyValueEntry<ValueReferenceLookup> valueEntry)
        {
            if (valueEntry.SmartValue == null)
            {
                _requiresRefresh = true;
                return;
            }

            if (_requiresRefresh)
            {
                _keysProperty = GetChildProperty(nameof(ValueReferenceLookup.Keys), out _keysValueEntry);
                _resolversProperty = GetChildProperty(nameof(ValueReferenceLookup.ValueResolversByKey),
                    out _resolversValueEntry);
                _defaultsProperty =
                    GetChildProperty(nameof(ValueReferenceLookup.DefaultsByType), out _defaultsValueEntry);

                _selectedDefaultFilter = GetRegisteredReferenceKeyTypes().FirstOrDefault();
                if (_defaultsProperty != null)
                    InitSelectedProperty();

                UpdateKeyFilter(string.Empty);
                _requiresRefresh = false;
            }

            SirenixEditorGUI.BeginHorizontalToolbar();
            _expanded = SirenixEditorGUI.ToolbarToggle(_expanded,
                _expanded ? EditorIcons.TriangleDown : EditorIcons.TriangleRight);
            foreach (var tab in Tabs.All)
            {
                if (SirenixEditorGUI.ToolbarTab(tab == _activeTab, tab))
                    _activeTab = tab;
            }

            SirenixEditorGUI.EndHorizontalToolbar();

            if (_expanded)
            {
                if (_activeTab == Tabs.ResolversTab)
                    DrawKeys();
                else if (_activeTab == Tabs.DefaultsTab)
                    DrawDefaults();
            }
        }

        private void RefreshDefaultValueSet()
        {
            var dictChild = GetChildProperty(nameof(ValueReferenceLookup.DefaultsByType));
            _selectedPropertyToInspect = dictChild.Children.FirstOrDefault(childProp =>
            {
                GetChildProperty<SerializableType>("Key", childProp, out var keyEntry);
                return keyEntry.SmartValue == _selectedDefaultFilter;
            });
        }

        private InspectorProperty FindResolverPropertyForKey(ReferenceKey key)
        {
            var pairProperty = _resolversProperty
                .FindChild(x => x.Children.Count > 0 && Equals(x.Children[0].ValueEntry.WeakSmartValue, key.Guid),
                    false);
            return pairProperty.Children[1];
        }

        private ICollection<SerializableType> GetRegisteredReferenceKeyTypes()
        {
            if (ValueEntry.SmartValue == null) return Array.Empty<SerializableType>();
            return ValueEntry.SmartValue.GetKeys().Select(x => x.ValueType).Distinct().ToArray();
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
                    var resolver = ValueEntry.SmartValue.FindResolverByID(key.Guid);
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

        private void DrawKeys()
        {
            if (_keysValueEntry == null || _keysProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Cannot find keys property...");
                return;
            }

            var host = Property.ParentValues.FirstOrDefault();

            if (_keysValueEntry.SmartValue == null)
                _keysValueEntry.SmartValue = new List<ReferenceKey>();

            SirenixEditorGUI.BeginVerticalList(false, false);

            var toolbarRect = _pager.BeginDrawPager(_validKeyIds);

            for (int pageI = _pager.StartIndex; pageI < _pager.EndIndex; ++pageI)
            {
                if (!_validKeyIds.HasIndex(pageI)) continue;

                var i = _validKeyIds[pageI];
                var key = _keysValueEntry.SmartValue[i];

                SirenixEditorGUI.BeginListItem();

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (!_resolverPropertyByKey.ContainsKey(key))
                        _resolverPropertyByKey[key] = FindResolverPropertyForKey(key);

                    var resolverProp = _resolverPropertyByKey[key];

                    using (new EditorGUILayout.VerticalScope())
                    {
                        _keysProperty.Children[i].Draw();
                        resolverProp.Draw();
                    }

                    using (new EditorGUILayout.VerticalScope(GUILayoutOptions.Width(20)))
                    {
                        // Delete
                        if (SirenixEditorGUI.IconButton(EditorIcons.X))
                        {
                            ValueEntry.SmartValue.Deregister(_keysValueEntry.SmartValue[i].Guid);
                            RefreshData();
                        }

                        // Check usages
                        var hasUsageData =
                            Property.ParentType.EqualsOneOf(typeof(TaskObject), typeof(TaskEditViewPage));
                        if (hasUsageData && host != null && SirenixEditorGUI.IconButton(EditorIcons.StarPointer))
                        {
                            var usagesInfo = new ValueReferenceInfo(host, _keysValueEntry.SmartValue[i].Guid);
                            usagesInfo.MakeUsages();
                            OdinEditorWindow.InspectObjectInDropDown(usagesInfo);
                        }
                    }
                }

                SirenixEditorGUI.EndListItem();
            }

            DrawKeyListButtons(host);

            _pager.EndDrawPager();
            SirenixEditorGUI.EndVerticalList();

        }

        private void DrawKeyListButtons(object host)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Add new Key", SirenixGUIStyles.ButtonLeft))
            {
                EditorInputDialog.Create("Create ReferenceKey", "Input the default name for the ReferenceKey:",
                        "Add New Key")
                    .TextField(null, out var name)
                    .Dropdown(null, GetReferenceKeyTypeOptions(), t => t.Name, out var type)
                    .OnAccept(() =>
                    {
                        ValueEntry.SmartValue.Register(name, type); // TODO: Test duplicate key + add logging
                        RefreshData();
                    })
                    .ShowInPopup();
            }

            var content = GUIContentHelper.TempContent("Simplify",
                "If multiple keys refer to the same object, condenses those into 1 and rectifies all usages.");
            if (host != null &&
                GUILayout.Button(content, SirenixGUIStyles.ButtonRight, GUILayoutOptions.ExpandWidth(false)))
            {
                var replacementKeyByKey = new Dictionary<SerializableGuid, SerializableGuid>();
                var keyResolverPairToKeep = new PairList<SerializableGuid, IValueResolver>();
                foreach (var (guid, resolver) in ValueEntry.SmartValue.ValueResolversByKey)
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
                    PLog.Info<MagnusLogger>($"Removing {replacementKeyByKey.Count} duplicate Resolvers...");
                    foreach (var (guid, replacementGuid) in replacementKeyByKey)
                    {
                        var usagesInfo = new ValueReferenceInfo(host, guid);
                        usagesInfo.ReplaceUsages(replacementGuid);
                        ValueEntry.SmartValue.Deregister(guid);
                    }

                    RefreshData();
                    PLog.Info<MagnusLogger>($"Resolvers successfully optimized!");
                }
                else
                {
                    PLog.Info<MagnusLogger>("Nothing to simplify...");
                }
            }

            GUILayout.EndHorizontal();
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

        private void DrawDefaults()
        {
            if (_defaultsValueEntry == null || _defaultsProperty == null)
            {
                SirenixEditorGUI.ErrorMessageBox("Cannot find defaults property...");
                return;
            }

            if (_defaultsValueEntry.SmartValue == null)
                _defaultsValueEntry.SmartValue = new Dictionary<SerializableType, DefaultTypeReferenceKey[]>();

            Rect rect = EditorGUILayout.GetControlRect();
            GetLabelFieldRects(rect, out Rect labelRect, out Rect fieldRect);
            GUI.Label(labelRect, "Choose a ReferenceKey Type:");

            if (GUI.Button(fieldRect, _selectedDefaultFilter != null ? _selectedDefaultFilter.Name : "null"))
            {
                DrawDropdown(rect, "", GetRegisteredReferenceKeyTypes(), (x) =>
                {
                    _selectedDefaultFilter = x;
                    RefreshDefaultValueSet();
                });
            }

            if (_selectedPropertyToInspect != null)
            {
                var property = GetChildProperty("#Value", _selectedPropertyToInspect);
                _defaultsPager.BeginDrawPager(_defaultsValueEntry.SmartValue[_selectedDefaultFilter]);

                for (int i = _defaultsPager.StartIndex; i < _defaultsPager.EndIndex; ++i)
                {
                    if (i >= property.Children.Count)
                    {
                        // NOTE: refresh needed, when deleting items and reading otherwise drawer is not up to date
                        RefreshDefaultValueSet();
                        property = GetChildProperty("#Value", _selectedPropertyToInspect);
                        if (i >= property.Children.Count)
                            continue;
                    }

                    var child = property.Children[i];
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.BeginVertical();
                    child.Draw(null);
                    EditorGUILayout.EndVertical();
                    if (GUILayout.Button("X", EditorStyles.toolbarButton))
                    {
                        // Remove
                        ValueEntry.SmartValue.RemoveDefault(_selectedDefaultFilter,
                            ((DefaultTypeReferenceKey) child.ValueEntry.WeakSmartValue).FieldData.Field);
                        _defaultsPager.Refresh();
                    }

                    EditorGUILayout.EndHorizontal();
                }

                _defaultsPager.EndDrawPager();
            }
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