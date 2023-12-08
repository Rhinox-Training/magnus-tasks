using System;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomEditor(typeof(TaskManager))]
    public class TaskManagerDrawer : DefaultEditorExtender<TaskManager>
    {
        private SimpleTableView _tableView;

        // public class TaskManagerState
        // {
        //     public GenericHostInfo HostInfo;
        //     public SimpleTableView TableView;
        // }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();

            var SmartValue = this.target as TaskManager;

            if (_tableView == null)
                _tableView = new SimpleTableView("ID", "Name", "State", "");
            
            _tableView.BeginDraw();

            var tasks = SmartValue.GetTasks();
            for (int index = 0; index < tasks.Count; ++index)
            {
                var task = tasks[index];
                if (task == null)
                {
                    _tableView.DrawRow(index, "<null>", TaskState.None, null);
                    continue;
                }
                _tableView.DrawRow(index, task.Name, SmartValue.GetTaskState(task)?.State ?? TaskState.None, new Action<GUILayoutOption[]>((layout) =>
                {
                    var state = SmartValue.GetTaskState(task);
                    EditorGUI.BeginDisabledGroup(!EditorApplication.isPlaying);
                    if (state == null || state.State == TaskState.Initialized || state.State == TaskState.None)
                    {
                        if (GUILayout.Button("Start", layout))
                        {
                            SmartValue.StartTask(task);
                        }
                    }
                    else if (state.State == TaskState.Running || state.State == TaskState.Paused)
                    {
                           
                        if (GUILayout.Button("Stop", layout))
                        {
                            SmartValue.CancelTask(task);
                        }
                    }
                    EditorGUI.EndDisabledGroup();
                }));
            }

            _tableView.EndDraw();
        }

        // protected override TaskManagerState CreateData(GenericHostInfo info)
        // {
        //     return new TaskManagerState()
        //     {
        //         HostInfo = info,
        //         TableView = new SimpleTableView("ID", "Name", "State", "")
        //     };
        // }
        //
        // protected override GenericHostInfo GetHostInfo(TaskManagerState data)
        // {
        //     return data.HostInfo;
        // }
    }
}