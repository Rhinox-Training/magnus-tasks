using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.VOLT.Training;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public static class TaskViewerSettings
{
    public class TypeSettings
    {
        private Type _type;
        private List<MemberInfo> _memberInfos;

        public Type Type => _type;
        public string AssemblyQualifiedName => Type?.AssemblyQualifiedName;

        public IReadOnlyList<MemberInfo> Members => _memberInfos;

        public TypeSettings(Type type)
        {
            _memberInfos = new List<MemberInfo>();
            Load(type);
        }
        
        public void AddToBuilder(StringBuilder builder, object target)
        {
            foreach (var member in _memberInfos)
            {
                var val = member.GetValue(target);
                builder.Append(val);
            }
        }
        
        public void FindAndAddToBuilder(StringBuilder builder, GameObject host)
        {
            var comps = host.GetComponentsInChildren(_type);
            foreach (var comp in comps)
                AddToBuilder(builder, comp);
        }

        public void Save()
        {
            EditorPrefs.SetString(GetKey(), this.ToString());
        }

        private string GetKey()
        {
            return PrefixKey + _type.FullName  + TypeFieldsPostFixKey;
        }

        public void Load(Type type)
        {
            _type = type;
            var strings = EditorPrefs.GetString(GetKey(), this.ToString());
            if (string.IsNullOrWhiteSpace(strings)) return;

            foreach (var str in strings.Split(KeySeparator))
                AddMemberIfItExists(str);
        }

        public override string ToString()
        {
            var names = _memberInfos.Select(x => x.Name).ToArray();
            return string.Join(KeySeparator.ToString(), names);
        }

        public void RemoveMember(MemberInfo member)
        {
            _memberInfos.Remove(member);
        }
        
        public bool AddMemberIfItExists(string memberName)
        {
            var found = ReflectionUtility.TryGetMember(_type, memberName, out MemberInfo member);
            if (found && !_memberInfos.Contains(member)) _memberInfos.Add(member);
            return found;
        }

        public void Delete()
        {
            EditorPrefs.DeleteKey(GetKey());
        }

        public IEnumerable<MemberInfo> GetMemberOptions()
        {
            return _type.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy)
                .Where(x => !Members.Contains(x))
                .Where(x => x.MemberType.EqualsOneOf(MemberTypes.Field, MemberTypes.Property))
                .Where(x => !TypeExtensions.InheritsFrom(x.GetReturnType(), typeof(Object)));
        }
    }
    
    
    private const string PrefixKey = "RHINOX_";
    private const string TypesKey = PrefixKey + "TaskViewerSettings_Types";
    private const string TypeFieldsPostFixKey = "_SearchFields";
    private const char KeySeparator = ';';

    public static bool HasData => _settings != null;

    private static Dictionary<Type, TypeSettings> _settings;

    public static IReadOnlyCollection<TypeSettings> All => _settings.Values;

    public static bool Exists(Type t) => _settings != null && _settings.ContainsKey(t);

    public static TypeSettings Get(Type type)
    {
        if (!HasData) LoadAll();
        
        if (_settings.ContainsKey(type)) return _settings[type];
        
        return _settings[type] = new TypeSettings(type);
    }
    
    public static TypeSettings AddType(Type t, params string[] members)
    {
        var setting = Get(t);
        
        foreach (var member in members)
            setting.AddMemberIfItExists(member);
        
        return setting;

    }

    public static void LoadAll()
    {
        _settings = new Dictionary<Type, TypeSettings>();
        
        var typesString = EditorPrefs.GetString(TypesKey);
        if (string.IsNullOrWhiteSpace(typesString))
        {
            AddType(typeof(BaseStep), "name", "Title", "Description");
            return;
        }
        
        var typeNames = typesString.Split(KeySeparator);
        
        foreach (var typeName in typeNames)
        {
            if (ReflectionUtility.TryParseType(typeName, out Type t))
                Get(t);
        }
    }

    public static void Save()
    {
        var typeNames = All
            .SelectNonNull(x => x?.AssemblyQualifiedName)
            .ToArray();
        
        EditorPrefs.SetString(TypesKey, string.Join(KeySeparator.ToString(), typeNames));
        
        foreach (var value in All)
            value.Save();
    }

    public static void Remove(TypeSettings set)
    {
        set.Delete();
        _settings.Remove(set.Type);
    }
}

public class TaskViewerSettingsUI : PagerPage
{
    private List<Type> _availableTypes;
    
    public TaskViewerSettingsUI(SlidePagedWindowNavigationHelper<object> pager) : base(pager)
    {
        if (!TaskViewerSettings.HasData)
            TaskViewerSettings.LoadAll();
        
        _availableTypes = ReflectionUtility.GetTypesInheritingFrom(typeof(Component))
            .Where(x => !TaskViewerSettings.Exists(x))
            .ToList();
    }

    protected override void OnDraw()
    {
        EditorGUILayout.HelpBox("Types below will be fetched on the Step object and shown in the viewer. The fields define what can be used to search.", MessageType.Info);
        
        foreach (TaskViewerSettings.TypeSettings set in TaskViewerSettings.All)
        {
            EditorGUILayout.BeginHorizontal(TaskViewer.HeaderStyle);
            using (new eUtility.IndentedLayout())
            {
                GUILayout.Label(GUIContentHelper.TempContent(set.Type.Name));
                GUILayout.FlexibleSpace();
                GUILayout.Label(GUIContentHelper.TempContent(set.Type.Namespace), CustomGUIStyles.MiniLabelRight);
                if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Times"), tooltip: "Remove type"))
                    EditorApplication.delayCall += () => TaskViewerSettings.Remove(set);

            }
            EditorGUILayout.EndHorizontal();

            GUIContentHelper.PushIndentLevel(EditorGUI.indentLevel + 1);

            for (var i = set.Members.Count - 1; i >= 0; i--)
            {
                var member = set.Members[i];
                using (new eUtility.HorizontalGroup())
                {
                    GUILayout.Label(member.Name);
                    GUILayout.FlexibleSpace();
                    if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Times"), tooltip: "Remove property"))
                        set.RemoveMember(member);
                }
            }

            using (new eUtility.HorizontalGroup())
            {
                var members = set.GetMemberOptions();
                var memberNames = members.Select(x => x.Name).ToArray();

                if (EditorGUILayout.DropdownButton(GUIContentHelper.TempContent("Add Member"), FocusType.Passive))
                {
                    var genericMenu = new GenericMenu();
                    foreach (var memberName in memberNames)
                    {
                        genericMenu.AddItem(memberName, (x) => { set.AddMemberIfItExists(x); });
                    }

                    genericMenu.DropDown(EditorGUILayout.GetControlRect());
                }
                // var i = SirenixEditorFields.Dropdown("Add Member", -1, memberNames);
                // if (i >= 0)
                //     set.AddMemberIfItExists(memberNames[i]);
            }

            GUIContentHelper.PopIndentLevel();
        }
        
        GUILayout.Space(5);
        CustomEditorGUI.HorizontalLine(CustomGUIStyles.BorderColor);
        GUILayout.Space(5);
        
        using (new eUtility.HorizontalGroup())
        {
            GUILayout.Label(GUIContentHelper.TempContent("Add Type"));
            if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Plus")))
            {
                var selector = new TypeSelector(_availableTypes, false);
                selector.SelectionConfirmed += l =>
                {
                    var t = l.FirstOrDefault();
                    if (t == null) 
                        return;
                    
                    TaskViewerSettings.Get(t);
                    _availableTypes.Remove(t);
                };
                selector.ShowInPopup();
            }
        }
    }
}