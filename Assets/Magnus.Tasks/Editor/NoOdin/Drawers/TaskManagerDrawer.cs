using System;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(TaskManager))]
    public class TaskManagerDrawer : BasePropertyDrawer<TaskManager, TaskManagerDrawer.TaskManagerState>
    {
        public class TaskManagerState
        {
            public GenericHostInfo HostInfo;
            public SimpleTableView TableView;
        }


        protected override void DrawProperty(Rect position, ref TaskManagerState data, GUIContent label)
        {
            CallInnerDrawer(position, label);

            var tableView = data.TableView;
            
            tableView.BeginDraw();

            var tasks = SmartValue.GetTasks();
            for (int index = 0; index < tasks.Length; ++index)
            {
                var task = tasks[index];
                tableView.DrawRow(index, task.name, task.State, new Action<GUILayoutOption[]>((layout) =>
                {
                    if (GUILayout.Button("Start", layout))
                    {
                        Debug.Log("Foobar");
                    }
                }));
            }

            tableView.EndDraw();
        }

        protected override TaskManagerState CreateData(GenericHostInfo info)
        {
            return new TaskManagerState()
            {
                HostInfo = info,
                TableView = new SimpleTableView("ID", "Name", "State", "");
            };
        }

        protected override GenericHostInfo GetHostInfo(TaskManagerState data)
        {
            return data.HostInfo;
        }
    }
}