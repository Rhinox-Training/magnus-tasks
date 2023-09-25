using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Attributes;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Perceptor;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    // [Serializable]
    [HideReferenceObjectPicker, DataLayerConfigResolver(nameof(Configuration))]
    public class EndPointTaskSet
    {
        [ShowInInspector]
        [CustomValueDrawer(nameof(ConfigurationNameDrawer))]
        [HorizontalGroup("TitleBar")]
        public DataLayerConfig Configuration;

        public string DisplayName => Configuration == null ? "[Default]" : Configuration.name;
        
        [UnfoldList]
        // [ListDrawerSettings(Expanded = true, HideAddButton = true, HideRemoveButton = true, DraggableItems = false)]
        public List<TaskData> Tasks;

        private List<TaskObject> _tasksFromDataLayer;
        internal TaskListViewerPage ParentPage;

        public EndPointTaskSet(TaskListViewerPage parentPage, DataLayerConfig config = null, params TaskObject[] tasks)
        {
            ParentPage = parentPage;
            Configuration = config;
            Tasks = new List<TaskData>();
            foreach (var task in tasks)
                Add(task);
        }
        
        public void Add(TaskObject task)
        {
            Tasks.Add(new TaskData(this, task));
        }

        [HorizontalGroup("TitleBar"), Button("Add", ButtonSizes.Small)]
        private void AddEditor()
        {
            Tasks.Add(new TaskData(this, new TaskObject(GenerateTaskID())));
        }

        private int GenerateTaskID()
        {
            if (Tasks.Count == 0)  return 0;
            return Tasks[Tasks.Count -1].TaskID + 1;
        }

        [HorizontalGroup("TitleBar"), Button(ButtonSizes.Small)]
        public void Refresh()
        {
            if (_tasksFromDataLayer != null)
            {
                foreach (var task in _tasksFromDataLayer)
                    Tasks.RemoveAll(x => x.Task == task);
            }

            DataLayer.PushEndPointFromConfigOrDefault(Configuration);
            var table = DataLayer.GetTable<TaskObject>();
            if (table == null)
            {
                DataLayer.PopEndPoint();
                PLog.Error<MagnusLogger>("Cannot find table for TaskObject.");
                return;
            }
            _tasksFromDataLayer = table.GetAllData().ToList();
            foreach (var task in _tasksFromDataLayer)
                Tasks.Add(new TaskData(this, task));
            DataLayer.PopEndPoint();
        }

        private DataLayerConfig ConfigurationNameDrawer(DataLayerConfig value, GUIContent label)
        {
            EditorGUILayout.LabelField(DisplayName);
            return value;
        }

        public void Remove(TaskData taskData)
        {
            Tasks.Remove(taskData);
            ParentPage.NotifyRemoval(Configuration, taskData);
        }

        public void Save(TaskObject task)
        {
            DataLayer.PushEndPointFromConfigOrDefault(Configuration);
            var table = DataLayer.GetTable<TaskObject>();
            
            if (task.ID < 0)
                task.ID = table.GetIDs().MaxOrDefault(-1) + 1;
            
            table.StoreData(task, true);
            
            _tasksFromDataLayer.Add(task);
            // Refresh is needed. optional TODO: maybe just remove original from _tasksFromDataLayer
            Refresh();
            DataLayer.PopEndPoint();
        }
    }
    
    public class TaskListViewerPage : PagerPage, IRefreshable
    {
        [ShowInInspector, UnfoldList, HideLabel]
        public List<EndPointTaskSet> Data;

        public TaskListViewerPage(SlidePageNavigationHelper<object> pager) : base(pager)
        {
            Data = new List<EndPointTaskSet>();
            
            var defaultTaskSet = new EndPointTaskSet(this);
            defaultTaskSet.Refresh();
            Data.Add(defaultTaskSet);
            
            foreach (var overrideDataConfig in DataLayerHelper.FindOverrides())
            {
                var taskSet = new EndPointTaskSet(this, overrideDataConfig);
                taskSet.Refresh();
                Data.Add(taskSet);
            }
        }
        
        internal void EditPage(TaskData data)
        {
            _pager.PushPage(new TaskEditViewPage(_pager, data), "Add Database Entry");
        }

        public void NotifyRemoval(DataLayerConfig configuration, TaskData taskData)
        {
            DataLayer.PushEndPointFromConfigOrDefault(configuration);
            DataLayer.GetTable<TaskObject>().RemoveData(taskData.Task.ID);
            DataLayer.PopEndPoint();
        }

        public void Refresh()
        {
            if (Data == null) return;
            
            foreach (var set in Data)
                set.Refresh();
        }
    }
}