using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.VOLT.Data;
using Rhinox.Vortex;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class TaskEditorWindow : PagerEditorWindow<TaskEditorWindow>
    {
        [MenuItem("Rhinox/Task Editor", priority = 3500)]
        public static void OpenWindow()
        {
            TaskEditorWindow window = CreateWindow<TaskEditorWindow>();

            window.name = "Task Editor";
            window.titleContent = new GUIContent("Task Editor", UnityIcon.AssetIcon("Fa_ListUl"));
        }

        private TaskListViewerPage _main;
        protected override object RootPage => _main ?? (_main = new TaskListViewerPage(_pager));
        protected override string RootPageName => "Overview";

        private Texture _loadIcon;
        private Texture _saveIcon;

        /// ================================================================================================================
        /// METHODS
        protected override void Initialize()
        {
            base.Initialize();

            //TaskViewerSettings.LoadAll();

            // Otherwise the scrollbar might pop up and move some things (like the next, previous btn) 
            _alwaysShowVerticalScrollbar = true;

            _loadIcon = UnityIcon.AssetIcon("load");
            _saveIcon = UnityIcon.AssetIcon("save");
        }

        protected override int DrawHeaderEditor()
        {
            //base.OnBeginDrawEditors();

            int defaultHeight = 22;
            CustomEditorGUI.BeginHorizontalToolbar(defaultHeight);
            GUILayout.FlexibleSpace();
            
#if ODIN_INSPECTOR 
            if (CustomEditorGUI.ToolbarButton("Load from Scene"))
                LoadScene();
            
            if (CustomEditorGUI.ToolbarButton(nameof(Import)))
                Import();

            EditorGUI.BeginDisabledGroup(!HasTasks());
            if (CustomEditorGUI.ToolbarButton(nameof(Backup)))
                Backup();
            EditorGUI.EndDisabledGroup();
#endif
            
            if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Redo"), 22))
                Refresh();
            
            CustomEditorGUI.EndHorizontalToolbar();
            return defaultHeight;
        }
        
#if ODIN_INSPECTOR 
        private void LoadScene()
        {
            TaskExportHelper.ImportFromScene(Refresh);
        }

        private void Import()
        {
            EditorApplication.delayCall += () => TaskExportHelper.Import(Refresh);
        }

        private void Backup()
        {
            EditorApplication.delayCall += () => TaskExportHelper.BackupSingleTask(GetTasks());
        }
#endif

        private ICollection<TaskObject> GetTasks()
        {
            if (_main == null) return Array.Empty<TaskObject>();
            
            return _main.Data
                .SelectMany(x => x.Tasks)
                .Select(x => x.Task)
                .ToArray();
        }

        private bool HasTasks()
        {
            return _main != null && _main.Data.Any(x => !x.Tasks.IsNullOrEmpty());
        }
        
        internal void SaveAll()
        {
            foreach (var data in _main.Data)
            {
                DataLayer.PushEndPointFromConfigOrDefault(data.Configuration);
                foreach (var task in data.Tasks)
                {
                    DataLayer.GetTable<TaskObject>().StoreData(task.Task, true);
                }
                DataLayer.PopEndPoint();
            }
        }
    }
}