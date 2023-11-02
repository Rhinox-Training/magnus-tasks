using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class TaskViewer : PagerMenuEditorWindow<TaskViewer>
    {
        /// ================================================================================================================
        /// PROPERTIES
        private IList<TaskBehaviour> _tasks;

        private static GUIStyle _headerStyle;

        public static GUIStyle HeaderStyle => _headerStyle ?? (_headerStyle =
            new GUIStyle(CustomGUIStyles.TitleBackground)
            {
                fixedHeight = 26
            });

        private TaskViewerBase _main;
        protected override object RootPage => _main ?? (_main = new TaskViewerBase(_pager));
        protected override string RootPageName => "Overview";

        protected override bool IsMenuAvailable => _pager.IsOnFirstPage;

        private TaskViewerSettingsUI _settings;
        public TaskViewerSettingsUI Settings => _settings ?? (_settings = new TaskViewerSettingsUI(_pager));

        private Texture _loadIcon;
        private Texture _saveIcon;

        /// ================================================================================================================
        /// METHODS
        protected override void Initialize()
        {
            base.Initialize();

            TaskViewerSettings.LoadAll();

            _loadIcon = UnityIcon.AssetIcon("load");
            _saveIcon = UnityIcon.AssetIcon("save");
        }

        [MenuItem("Rhinox/Task Viewer", priority = 3500)]
        public static void OpenWindow()
        {
            TaskViewer window;
            if (!GetOrCreateWindow(out window)) return;

            window.name = "Task Viewer";
            window.titleContent = new GUIContent("Task Viewer", UnityIcon.AssetIcon("Fa_ListUl"));
        }

        [MenuItem("CONTEXT/BaseStep/Open in Task Viewer")]
        public static void OpenOnStep(MenuCommand command)
        {
            OpenWindow();
            EditorApplication.delayCall += () => Select(command.context);
        }

        private static void Select(Object value)
        {
            var window = GetWindow<TaskViewer>();
            var menuItem = window.MenuTree.MenuItems.FirstOrDefault(x => x.RawValue.Equals(value));
            menuItem?.Select();
        }
#if ODIN_INSPECTOR
    private void Export()
    {
        var folder = EditorUtility.SaveFolderPanel("Save task", ".", "Tasks");

        if (string.IsNullOrWhiteSpace(folder)) return;

        Directory.CreateDirectory(folder);

        int i = 0;
        foreach (var task in _tasks)
        {
            var data = new TaskObject(i++, task.name);
            foreach (var step in task.GetComponentsInChildren<ConditionStep>())
            {
                var dataStep = new ConditionStepObject(step.Title);
                dataStep.Description = step.Description;

                if (step.Conditions != null)
                {
                    foreach (var condition in step.Conditions)
                    {
                        if (condition == null)
                            continue;

                        var conditionData = ConditionDataHelper.FromCondition(condition, database: false);
                        dataStep.Conditions.Add(conditionData);
                    }
                }

                data.Add(dataStep);
            }
            
            // Tasks
            var bytes = SerializedUnityReferencesObject.Pack(data);
            var fileName = $"{data.Name}.{TaskConstants.TaskExtension}";
            File.WriteAllBytes(Path.Combine(folder, fileName), bytes);
        }
    }
    
    private void Import()
    {
        var path = EditorUtility.OpenFilePanel("Import task", ".", TaskConstants.TaskExtension);

        if (string.IsNullOrWhiteSpace(path)) return;

        var data = File.ReadAllBytes(path);

        if (data.Length == 0)
        {
            PLog.Warn<MagnusLogger>($"File at path {path} is empty");
            return;
        }

        var task = SerializedUnityReferencesObject.Unpack<TaskObject>(data);
        BaseTask targetTask = null;

        var existingTask = _tasks.FirstOrDefault(x => x.name == task.Name);
        if (existingTask != null)
        {
            var overwrite = EditorUtility.DisplayDialog(
                "Override Task?",
                $"A task with the name '{task.Name}' already exists. Do you wish to merge with it?",
                "Yes",
                "No");

            if (overwrite)
                targetTask = existingTask;
        }

        if (targetTask == null)
        {
            var host = Utility.Create(task.Name, TaskManager.Instance.transform);
            targetTask = host.AddComponent<BasicTask>();
        }

        var existingSteps = targetTask.GetComponentsInChildren<ConditionStep>();

        for (var i = 0; i < task.Steps.Count; i++)
        {
            var name = task.Steps[i].Name;
            var targetStep = existingSteps.FirstOrDefault(x => x.name == name);
            if (targetStep == null)
            {
                var newObj = Utility.Create(name, targetTask.transform);
                targetStep = newObj.AddComponent<ConditionStep>();
            }

            targetStep.Conditions =
 (task.Steps[i] as ConditionStepObject).Conditions.Select(ConditionDataHelper.ToCondition).ToList();
        }
    }
#endif

        protected override void DrawToolbarIcons(int toolbarHeight)
        {
            if (_pager.IsOnFirstPage)
            {
#if ODIN_INSPECTOR
            if (TaskManager.HasInstance && CustomEditorGUI.ToolbarButton("Import"))
                EditorApplication.delayCall += Import;
            
            if (_tasks.Any() && CustomEditorGUI.ToolbarButton("Export"))
                EditorApplication.delayCall += Export;
#endif

                if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Cog"), toolbarHeight - 2, toolbarHeight - 2,
                        "Settings"))
                    _pager.PushPage(Settings, "Settings");
            }
            else
            {
                if (CustomEditorGUI.IconButton(_loadIcon, toolbarHeight - 4, toolbarHeight - 4,
                        "Load (Will Reset to Prefs)"))
                {
                    TaskViewerSettings.LoadAll();
                    ForceMenuTreeRebuild();
                }

                GUILayout.Space(10);

                if (CustomEditorGUI.IconButton(_saveIcon, toolbarHeight - 4, toolbarHeight - 4, "Save"))
                {
                    TaskViewerSettings.Save();
                    ForceMenuTreeRebuild();
                    _pager.NavigateBack();
                }
            }
        }

        protected override CustomMenuTree BuildMenuTree()
        {
            var tree = new CustomMenuTree();

#if ODIN_INSPECTOR
        tree.Config.DrawSearchToolbar = true;
        tree.Config.SearchFunction = SimpleSearch;
#endif

            _tasks = new List<TaskBehaviour>();
            Utility.FindSceneObjectsOfTypeAll(_tasks);
            for (var i = 0; i < _tasks.Count; i++)
            {
                var task = _tasks[i];
                tree.Add(task.name, task);

                foreach (var step in task.GetComponentsInChildren<BaseStep>())
                {
                    var item = new UIMenuItem(tree, task.name + "/" + step.name, step);
                    //TODO : support search string
                    //item.SearchString = GenerateSearchString(task, step);
                    tree.AddCustom(item);
                }
            }

            tree.SelectionChanged += OnSelectionChanged;
            return tree;
        }

        private string GenerateSearchString(TaskBehaviour task, BaseStep step)
        {
            var builder = new StringBuilder();
            builder.Append(task.name);
            foreach (var searchList in TaskViewerSettings.All)
            {
                if (searchList.Type == typeof(BaseStep))
                    searchList.AddToBuilder(builder, step);
                else
                    searchList.FindAndAddToBuilder(builder, step.gameObject);
            }

            return builder.ToString();
        }

        private void OnSelectionChanged(Rhinox.GUIUtils.Editor.SelectionChangedType type)
        {
            // Selection can not be more than 1
            IMenuItem selection = MenuTree.Selection.FirstOrDefault();
            // TODO: how to support Toggled with new GUIUtils
            // if (selection?.RawValue is BaseTask)
            //     selection.Toggled = true;
            _main.SetTarget(selection?.RawValue);
        }
    }
}