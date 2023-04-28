using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils.Editor;
using Rhinox.GUIUtils.Editor.Helpers;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Rhinox.Vortex;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

// unity 2021.3 has introduced their own SerializationUtility, so scope it

namespace Rhinox.VOLT.Editor
{
    [DataLayer, ShowOdinSerializedPropertiesInInspector]
    [SerializedGuidProcessor(nameof(Lookup))]
    [DataLayerConfigResolver(nameof(Configuration))]
    public class TaskEditViewPage : PagerPage
    {
        [CustomValueDrawer(nameof(TaskHeaderDrawer))]
        public TaskData TaskData;

        [TabGroup("Lookup")]
        public ValueReferenceLookup Lookup;

        public DataLayerConfig Configuration => TaskData?.ParentSet?.Configuration;

        [TabGroup("Steps")]
        [ListDrawerSettings(
            ShowPaging = true, NumberOfItemsPerPage = 1,
            // ShowIndexLabels = true, ListElementLabelName = "Name",
            Expanded = true, OnTitleBarGUI = nameof(OnTitleBarGUI))]
        public List<StepData> Steps;

        private TaskData _taskData;
        private string _editingName;

        private int _indexMarkedForRemoval;

        public TaskEditViewPage(SlidePagedWindowNavigationHelper<object> pager, TaskData data) : base(pager)
        {
            _taskData = data;
            if (data.Task == null)
            {
                Debug.LogWarning("Trying to edit taskObject without task reference, exiting edit view...");
                EditorApplication.delayCall += pager.NavigateBack;
                return;
            }

            TaskData = data;
            Lookup = data.Task.Lookup;
            var taskSteps = data.Task.Steps ?? new List<StepData>();
            ConvertToSerialization(taskSteps); // Safety, ensure it is not editorparams TODO: can be removed again
            Steps = ConvertToEditor(taskSteps);
            _indexMarkedForRemoval = -1;
        }

        public override void Update()
        {
            if (_indexMarkedForRemoval >= 0)
            {
                Steps.RemoveAt(_indexMarkedForRemoval);
                _indexMarkedForRemoval = -1;
            }
        }

        private List<StepData> ConvertToEditor(List<StepData> taskSteps)
        {
            List<StepData> result = new List<StepData>();
            for (var i = 0; i < taskSteps.Count; i++)
            {
                // Ensure it is not a reference
                var step = (StepData) SerializationUtility.CreateCopy(taskSteps[i]);

                // Ensure step has a guid
                if (step.ID == null)
                    step.ID = SerializableGuid.CreateNew();

                if (step is ConditionStepObject conditionStep)
                {
                    for (var condI = 0; condI < conditionStep.Conditions.Count; condI++)
                    {
                        var c = conditionStep.Conditions[condI];
                        EditorParamDataHelper.ConvertToEditor(ref c);
                    }

                    result.Add(conditionStep);
                }
                else
                {
                    result.Add(step);
                }
            }

            return result;
        }
        
        private List<StepData> ConvertToSerialization(List<StepData> taskSteps)
        {
            List<StepData> result = new List<StepData>();
            foreach (var step in taskSteps)
            {
                if (step is ConditionStepObject conditionStep)
                {
                    conditionStep.Conditions.ForEach(x => EditorParamDataHelper.RevertFromEditor(ref x));
                    result.Add(conditionStep);
                }
                else
                {
                    result.Add(step);
                }
            }

            return result;
        }

        private TaskData TaskHeaderDrawer(TaskData value, GUIContent content)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(value.ParentSet.DisplayName, GUILayout.ExpandWidth(false));
            GUILayout.Label("-", GUILayout.ExpandWidth(false));
            var isEditing = _editingName != null;
            if (isEditing)
            {
                _editingName = EditorGUILayout.TextField(_editingName, GUILayout.ExpandWidth(true));
            }
            else
            {
                GUILayout.Label(value.TaskName, EditorStyles.boldLabel);
                GUILayout.FlexibleSpace();
            }

            if (isEditing)
            {
                if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Times")))
                    _editingName = null;
                else if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Check")))
                {
                    value.Task.Name = _editingName;
                    _editingName = null;
                }
            }
            else if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Pen")))
                _editingName = value.TaskName;
            
#if ODIN_INSPECTOR
            if (GUILayout.Button("Export Partial", GUILayout.ExpandWidth(false)))
            {
                var dialog = EditorInputDialog.Create("Export Data", "Configure for export:")
                    .TextField("Task Name:", out var taskName)
                    .Dropdown("Start Index (Incl.):", GetStepIndexOptions(), out var startIndex)
                    .Dropdown("End Index (Incl.):", GetStepIndexOptions(), out var endIndex)
                    .Dropdown("DataLayer:", DataLayerHelper.GetConfigList(), out var datalayer, null)
                    .OnAccept(() =>
                    {
                        ExportPartialTask(taskName, startIndex, endIndex, datalayer);
                    })
                    .ShowInPopup();

                dialog.Width = 600;
            }
#endif
            GUILayout.EndHorizontal();
            return value;
        }
        
#if ODIN_INSPECTOR
        private void ExportPartialTask(string newTaskName, int startIndex, int endIndex, DataLayerConfig dataLayerTarget)
        {
            TaskObject to = new TaskObject()
            {
                Name = newTaskName
            };
            
            var filteredSteps = Steps
                .Skip(Math.Max(0, startIndex))
                .Take(endIndex - startIndex + 1)
                .Select( x => (StepData) SerializationUtility.CreateCopy(x) )
                .ToList();
            
            List<SerializableGuid> usedGuids = new List<SerializableGuid>();
            foreach (var step in filteredSteps)
            {
                if (!(step is ConditionStepObject conditionStep)) continue;
                
                foreach (var condition in conditionStep.Conditions)
                {
                    if (condition.Params == null) continue;

                    usedGuids.AddRange(condition.Params
                        .Where(x => x.Type.Type == typeof(SerializableGuid))
                        .Select(x => x.MemberData as SerializableGuid));
                    
                    ParamData param = condition.Params.FirstOrDefault(x => x.Name == nameof(BaseCondition.OnBetterConditionMet));
                    if (param == null || !(param.MemberData is ValueReferenceEvent eventMember))
                        continue;

                    ParseEvent(ref usedGuids, eventMember);
                }
                        
                // TODO: check events for additional used guids
                ParseEvent(ref usedGuids, conditionStep.OnStarted);
                ParseEvent(ref usedGuids, conditionStep.OnCompleted);
            }

            //  TODO: create filtered lookup
            usedGuids = usedGuids.Where(x => x != null && !x.Equals(SerializableGuid.Empty)).ToList();
            var newLookup = new ValueReferenceLookup();
            foreach (var entry in Lookup.Keys)
            {
                if (!usedGuids.Contains(entry.Guid))
                    continue;

                newLookup.Register(entry.Guid, entry.Name, Lookup.FindResolverByID(entry.Guid));
            }


            to.Lookup = newLookup;
            to.Steps = filteredSteps;

            TaskExportHelper.BackupTask(to, dataLayerTarget);
        }
#endif
        
        private static void ParseEvent(ref List<SerializableGuid> usedGuids, ValueReferenceEvent e)
        {
            if (e.Events == null) return;
            
            foreach (var entry in e.Events)
                ParseEventEntry(ref usedGuids, entry);
        }

        private static void ParseEventEntry(ref List<SerializableGuid> usedGuids, ValueReferenceEventEntry eventEntry)
        {
            usedGuids.Add(eventEntry.Target);

            if (eventEntry.Action == null)
                return;

            var actionType = eventEntry.Action.GetType();
            if (actionType.IsGenericType && actionType.GetGenericTypeDefinition() == typeof(DynamicMethodEventAction<>))
            {
                var fieldInfo = actionType.GetField("Data");
                var args = fieldInfo.GetValue(eventEntry.Action) as DynamicMethodArgument[];

                if (args != null)
                {
                    foreach (var arg in args)
                    {
                        if (arg.Data is SerializableGuid serial)
                            usedGuids.Add(serial);
                    }
                }
            }
        }

        private ValueDropdownList<int> GetStepIndexOptions()
        {
            var steps = new ValueDropdownList<int>();
            for (int i = 0; i < Steps.Count; ++i)
                steps.Add(Steps[i].Name, i);
            return steps;
        }

        private void DrawStepsItemBegin(int i)
        {
            GUILayout.BeginHorizontal();
            GUILayout.BeginVertical(GUILayout.ExpandWidth());
        }
        
        private void DrawStepsItemEnd(int i)
        {
            GUILayout.EndVertical();
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Crosshairs")))
                {
                    Steps.Insert(i + 1, new ConditionStepObject());
                }

                if (CustomEditorGUI.IconButton(UnityIcon.AssetIcon("Fa_Times")))
                {
                    if (!Steps[i].HasData() || EditorUtility.DisplayDialog("Confirm Deletion",
                        "Are you sure you want to delete this element?",
                        "Yes", "No"))
                    {
                        _indexMarkedForRemoval = i;
                    }
                    
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
        }

        private void OnTitleBarGUI()
        {
            if (CustomEditorGUI.ToolbarButton("Insert"))
            {
                var types = ReflectionUtility.GetTypesInheritingFrom(typeof(StepData));
                EditorInputDialog.Create("Insert Step", "Insert step at index")
                    .IntField("Index", out var index)
                    .Dropdown("Type", types, x => x.Name, out var type, types.First())
                    .OnAccept(() =>
                    {
                        EditorApplication.delayCall += () =>
                        {
                            int i = Mathf.Clamp(index, 0, Steps.Count);
                            var data = (StepData) Activator.CreateInstance(type.Value);
                            Steps.Insert(i, data);
                        };
                    })
                    .ShowInPopup();
            }
        }

        [HorizontalGroup("Buttons", .7f), Button(ButtonSizes.Large)]
        //[DisableIf("HasChanges")]
        public void Save()
        {
            _taskData.Task.Steps = ConvertToSerialization(Steps);
            
            _taskData.ParentSet.Save(_taskData.Task);
        }

#if ODIN_INSPECTOR
        [HorizontalGroup("Buttons"), Button(ButtonSizes.Large)]
        //[DisableIf("HasChanges")]
        public void SaveAs()
        {
            DataLayer.PushEndPointFromConfigOrDefault(Configuration);
            var table = DataLayer.GetTable<TaskObject>();

            EditorInputDialog.Create("Save as", "Save task as new task")
                .IntField("ID", out var id, table.GetIDs().MaxOrDefault(-1) + 1)
                .TextField("Name", out var name, _taskData.TaskName)
                .OnAccept(() =>
                {
                    EditorApplication.delayCall += () =>
                    {
                        var task = (TaskObject) SerializationUtility.CreateCopy(_taskData.Task);
                        task.ID = id;
                        task.Name = name;
                        task.Steps = ConvertToSerialization(Steps);
                        
                        _taskData.ParentSet.Save(task);
                    };
                })
                .ShowInPopup();
            
        }
#endif
    }
}