#if ODIN_INSPECTOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using MemberFinder = Sirenix.Utilities.MemberFinder;
using Rhinox.Vortex;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

namespace Rhinox.VOLT.Editor
{
    public static class TaskExportHelper
    {
        public const string OldTaskExtension = "rxtk";
        public const string BackupExtension = "taskbackup";

        //==============================================================================================================
        // EXPORT/BACKUP
        
        public static void BackupSingleTask(ICollection<TaskObject> taskOptions)
        {
            if (taskOptions == null || taskOptions.Count == 0)
            {
                Debug.LogWarning("Cannot export, no options provided...");
                return;
            }

            EditorInputDialog.Create("Export task", "Choose a task to export:", "Export")
                .Dropdown(null, taskOptions, x => x.Name, out var selection)
                .OnAccept(() =>
                {
                    var folder = EditorUtility.SaveFolderPanel("Backup task", ".", "Tasks");

                    if (string.IsNullOrWhiteSpace(folder)) 
                        return;

                    Directory.CreateDirectory(folder);

                    // Tasks
                    var data = selection.Value;
                    var bytes = SerializedUnityReferencesObject.Pack(data);
                    var fileName = $"{data.Name}.{BackupExtension}";
                    File.WriteAllBytes(Path.Combine(folder, fileName), bytes);
                })
                .Show();
        }
        
        public static void BackupTask(TaskObject task, DataLayerConfig datalayerTargetValue)
        {
            bool success = ImportTaskIntoDataLayer(task, datalayerTargetValue, true);
            if (!success)
                Debug.LogError($"Failed to save BasicTask {task.Name} in DataLayer");
            else
                Debug.Log($"Successfully saved BasicTask {task.Name}");
        }
        
        //==============================================================================================================
        // IMPORT/BACKUP

        public static void ImportFromScene(Action callback)
        {
            var taskOptions = Object.FindObjectsOfType<BaseTask>();
            if (taskOptions.Length == 0)
                return;
            
            EditorInputDialog.Create("Import task from scene", "Choose a task to import into the DataLayer:", "Import")
                .Dropdown(null, taskOptions, x => x.name, out var task)
                .Dropdown(null, DataLayerHelper.GetConfigList(), out var datalayer, null)
                .OnAccept(() =>
                {
                    ImportTaskFromMonoBehaviour(task, datalayer);
                    callback?.Invoke();
                })
                .Show();
        }
        private static void ImportTaskFromMonoBehaviour(BaseTask task, DataLayerConfig dataLayer)
        {
            TaskObject taskObject = ConvertToTaskObject(task);
            
            bool success = ImportTaskIntoDataLayer(taskObject, dataLayer, true);
            if (!success)
                Debug.LogError($"Failed to save BasicTask {task.GetFullName()} in DataLayer");
            else
                Debug.Log($"Successfully saved BasicTask {task.GetFullName()}");
        }
        
        public static void Import(Action callback)
        {
            var path = EditorUtility.OpenFilePanelWithFilters("Import task", ".", 
                new []
                {
                    "Old Task", OldTaskExtension,
                    "Backup System", BackupExtension,
                    "All Files", "*"
                });

            if (string.IsNullOrWhiteSpace(path)) 
                return;
            
            var data = File.ReadAllBytes(path);

            if (data.Length == 0)
            {
                PLog.Warn<MagnusLogger>($"File at path {path} is empty");
                return;
            }
            var task = SerializedUnityReferencesObject.Unpack<TaskObject>(data);
            if (task == null)
            {
                PLog.Warn<MagnusLogger>($"File at path {path}, could not be read...");
                return;
            }

            bool generateNewID = path.EndsWith(OldTaskExtension);

            EditorInputDialog.Create("DataLayer Target", "Choose DataLayer as import target:", "Import")
                .Dropdown(null, DataLayerHelper.GetConfigList(), out var datalayer, null)
                .OnAccept(() =>
                {
                    bool success = ImportTaskIntoDataLayer(task, datalayer.Value, generateNewID);
                    if (!success)
                        Debug.LogError($"Failed to save TaskObject {task.Name} in DataLayer");
                    else
                        Debug.Log($"Successfully saved TaskObject {task.Name}");
                    
                    callback?.Invoke();
                })
                .Show();
        }
        
        //==============================================================================================================
        // Utility

        private static bool ImportTaskIntoDataLayer(TaskObject taskObject, DataLayerConfig dataLayer = null, bool generateNewID = false)
        {
            DataLayer.PushEndPointFromConfigOrDefault(dataLayer);
            var taskObjectTable = DataLayer.GetTable<TaskObject>();
            if (generateNewID)
            {
                int newId = taskObjectTable.GetIDs().MaxOrDefault(-1) + 1;
                taskObject.ID = newId;
            }

            bool containsId = taskObjectTable.GetData(taskObject.ID) != null;
            bool overwrite = containsId && EditorUtility.DisplayDialog("Confirm",
                $"DataTable already contains task with id {taskObject.ID}. Allow to overwrite?", "Confirm", "Cancel");

            bool success = taskObjectTable.StoreData(taskObject, overwrite);
            DataLayer.PopEndPoint();
            return success;
        }
        
        private static TaskObject ConvertToTaskObject(BaseTask task)
        {
            // TODO: TEMP code needs to be changed when no longer monoBehaviour
            var potentialSteps = task.GetComponentsInChildren<MonoBehaviour>()
                .Where(x => x.gameObject.activeSelf)
                .ToArray();

            // Construct field information for each BaseCondition Type
            var conditionTypeFieldOverviews = new Dictionary<Type, ValueReferenceFieldData[]>();
            var allConditionTypes = AppDomain.CurrentDomain.GetDefinedTypesOfType<BaseCondition>();
            foreach (var conditionType in allConditionTypes)
            {
                var fieldOverviews = new List<ValueReferenceFieldData>();
                foreach (var field in conditionType.GetFieldsWithAttribute<ValueReferenceAttribute>())
                    fieldOverviews.Add(ValueReferenceFieldData.Create(field));

                if (fieldOverviews.Count > 0)
                    conditionTypeFieldOverviews.Add(conditionType, fieldOverviews.ToArray());
            }
            
            // Construct StepData Attribute dict
            var stepDataGeneratorByType = new Dictionary<Type, StepDataGeneratorAttribute>();
            foreach (var t in AppDomain.CurrentDomain.GetDefinedTypesOfType<MonoBehaviour>())
            {
                var attr = t.GetCustomAttribute<StepDataGeneratorAttribute>();
                if (attr != null)
                    stepDataGeneratorByType[t] = attr;
            }

            // Convert steps
            TaskObject to = new TaskObject();
            to.Name = task.gameObject.name;
            
            var stepDatas = new List<StepData>();
            var constantOverridesToImport = new Dictionary<string, IValueResolver>(); // TODO
            int stepIndex = 0;
            foreach (var potentialStep  in potentialSteps)
            {
                if (potentialStep is ConditionStep conditionStep)
                    ConvertConditionStep(conditionStep, conditionTypeFieldOverviews, to, stepIndex, stepDatas);
                else if (stepDataGeneratorByType.TryGetValue(potentialStep.GetType(), out StepDataGeneratorAttribute attr))
                {
                    var memberFinder = MemberFinder.Start(potentialStep.GetType())
                        .IsMethod()
                        .IsNamed(attr.ConvertMethodName)
                        .HasNoParameters()
                        .HasConvertableReturnType(typeof(StepData));
                    
                    if (!memberFinder.TryGetMember(out MethodInfo info, out string error))
                        continue;

                    var stepData = (StepData) info.Invoke(potentialStep, Array.Empty<object>());

                    stepDatas.Add(stepData);
                }
                else
                    continue;

                stepIndex++;
            }

            to.Steps = stepDatas;

            return to;
        }

        private static void ConvertConditionStep(ConditionStep conditionStep, Dictionary<Type, ValueReferenceFieldData[]> conditionTypeFieldOverviews, TaskObject to, int stepIndex, List<StepData> stepDatas)
        {
            var conditionDatas = new List<ConditionData>();
            for (int i = 0; i < conditionStep.Conditions.Count; ++i)
            {
                var condition = conditionStep.Conditions[i];
                if (condition == null)
                    continue;
                var conditionDataObject = ConditionDataHelper.FromCondition(condition, true);
                var condType = condition.GetType();
                // Unparsed conditionType, can't process fields for ValueReferences
                if (conditionTypeFieldOverviews.ContainsKey(condType))
                {
                    ValueReferenceFieldData[] fieldDatas = conditionTypeFieldOverviews[condType];
                    foreach (ValueReferenceFieldData field in fieldDatas)
                    {
                        IValueResolver fieldValue = field.FindImportData(condition);

                        var availableKey = FindFreeKeyName(to.Lookup, field.DefaultKey, fieldValue);

                        var newOrExistingGuid = to.Lookup.Register(availableKey, fieldValue);
                        conditionDataObject.SetParam(field.Field, newOrExistingGuid);
                    }
                }

                // Unique base name (each entry will get small append)
                var baseName = $"step{stepIndex}:cond{i}:{nameof(condition.OnConditionMet)}";

                // Get ConditionMet event in parameters
                var type = conditionDataObject.ConditionType.Type;
                var condMetEventField = new SerializableFieldInfo(type.GetField(nameof(BaseCondition.OnBetterConditionMet),
                    BindingFlags.Public | BindingFlags.Instance));
                ValueReferenceEvent convertedOnCompleteEvent = conditionDataObject.GetParam<ValueReferenceEvent>(condMetEventField);

                // TODO: remove OnBetterConditionMet from basecondition; weird state limbo
                convertedOnCompleteEvent.Events.Clear();
                AddToValueReferenceEvent(condition.OnConditionMet, baseName, to, ref convertedOnCompleteEvent);

                // It's a struct so needs to be set again
                conditionDataObject.SetParam(condMetEventField, convertedOnCompleteEvent);

                conditionDatas.Add(conditionDataObject);
            }

            // Init the data object
            var conditionStepData = new ConditionStepObject(conditionStep.Title)
            {
                Description = conditionStep.Description
            };
            conditionStepData.Conditions = conditionDatas;

            var uEventName = $"step{stepIndex}:{nameof(conditionStep.StepStarted)}";
            AddToValueReferenceEvent(conditionStep.StepStarted, uEventName, to, ref conditionStepData.OnStarted);
            uEventName = $"step{stepIndex}:{nameof(conditionStep.StepCompleted)}";
            AddToValueReferenceEvent(conditionStep.StepCompleted, uEventName, to, ref conditionStepData.OnCompleted);

            // Convert Sub Data TODO: move this to a general place (not just for conditionData)
            conditionStepData.SubStepData = new List<BaseSubStepData>();
            var siblings = conditionStep.GetComponents<MonoBehaviour>();
            foreach (var sibling in siblings)
            {
                var type = sibling.GetType();
                var subDataAttribute = CustomAttributeExtensions.GetCustomAttribute<SubDataContainerAttribute>(type);
                if (subDataAttribute == null) continue;
                var memberFinder = MemberFinder.Start(type)
                    .IsMethod()
                    .IsNamed(subDataAttribute.ConvertMethodName)
                    .HasNoParameters()
                    .HasConvertableReturnType(typeof(BaseSubStepData));

                if (!memberFinder.TryGetMember(out MethodInfo info, out string error))
                    continue;

                var subdata = (BaseSubStepData) info.Invoke(sibling, Array.Empty<object>());

                conditionStepData.SubStepData.Add(subdata);
            }

            stepDatas.Add(conditionStepData);
        }

        private static void AddToValueReferenceEvent(BetterEvent betterEvent, string baseName, TaskObject to, ref ValueReferenceEvent convertedEvent)
        {
            for (int eventIndex = 0; eventIndex < betterEvent.Events.Count; ++eventIndex)
            {
                var entry = betterEvent.Events[eventIndex];
                var generatedName = $"{baseName}{eventIndex}:Target";
                var convertedEntry = ConvertToValueReferenceEventEntry(entry, to, generatedName);

                convertedEvent.AddListener(convertedEntry);
            }
        }

        private static ValueReferenceEventEntry ConvertToValueReferenceEventEntry(BetterEventEntry entry, TaskObject to, string keyName)
        {
            var convertedEntry = new ValueReferenceEventEntry();
            object target = entry.Delegate.Target;
            Type keyType = target.GetType();

            IValueResolver targetResolver = BuildUnityResolver(target, keyType);
            
            var targetGuid = FindCreateResolverEntry(to.Lookup, keyName, keyType, targetResolver);
            convertedEntry.Target = targetGuid;
            
            var actionInstance = CreateDynamicEventAction(entry.Delegate.Method.Name, keyType, out var dynamicConvertedArgs);
            convertedEntry.Action = actionInstance;

            if (entry.ParameterValues == null)
                return convertedEntry;
            
            var originalParameters = entry.Delegate.Method.GetParameters();
            for (int argIndex = 0; argIndex < dynamicConvertedArgs.Length; ++argIndex)
            {
                var convertedArg = dynamicConvertedArgs[argIndex];
                if (entry.ParameterValues.Length <= argIndex)
                    break;
                var srcArg = entry.ParameterValues[argIndex];

                var originalParamInfo = originalParameters[argIndex];
                    
                SaveParameter(to, $"{keyName}:Arg{argIndex}", originalParamInfo.ParameterType, srcArg, ref convertedArg);
            }

            return convertedEntry;
        }

        private static void SaveParameter(TaskObject to, string keyName, Type paramType, object srcArg, ref DynamicMethodArgument convertedArg)
        {
            if (paramType.InheritsFrom(typeof(UnityEngine.Object)))
            {
                var argResolver = BuildUnityResolver(srcArg, paramType);
                var argGuid = FindCreateResolverEntry(to.Lookup, keyName, paramType, argResolver);
                convertedArg.DataContainer.SetData(argGuid);
            }
            else
            {
                if (paramType.IsValueType && srcArg == null)
                    srcArg = paramType.GetDefault();

                convertedArg.DataContainer.SetData(srcArg);
            }
        }

        private static ValueReferenceEventAction CreateDynamicEventAction(string methodName, Type keytype, out DynamicMethodArgument[] dynamicConvertedArgs)
        {
            var dynamicEventType = typeof(DynamicMethodEventAction<>).MakeGenericType(keytype);
            var actionInstance = Activator.CreateInstance(dynamicEventType) as ValueReferenceEventAction;
            dynamicEventType.InvokeMethod(actionInstance, "SetMethodInfo", new[] { methodName });

            dynamicConvertedArgs = dynamicEventType.GetFieldValue<DynamicMethodArgument[]>(actionInstance, "Data");
            return actionInstance;
        }

        private static void AddToValueReferenceEvent(UnityEventBase uEvent, string baseName, TaskObject to, ref ValueReferenceEvent convertedEvent)
        {
            if (convertedEvent.Events == null)
                convertedEvent.Events = new List<ValueReferenceEventEntry>();
            
            var listeners = uEvent.GetPersistentListeners();

            for (var i = 0; i < listeners.Count; i++)
            {
                var convertedEntry = new ValueReferenceEventEntry();
                
                var listener = listeners[i];
                Type keyType = listener.Target.GetType();
                IValueResolver targetResolver = BuildUnityResolver(listener.Target, keyType);

                var generatedName = $"{baseName}{i}:Target";
                var targetGuid = FindCreateResolverEntry(to.Lookup, generatedName, keyType, targetResolver);
                convertedEntry.Target = targetGuid;
                
                var actionInstance = CreateDynamicEventAction(listener.Method.Name, keyType, out var dynamicConvertedArgs);
                convertedEntry.Action = actionInstance;

                if (!dynamicConvertedArgs.IsNullOrEmpty())
                {
                    dynamicConvertedArgs[0].DataContainer.SetData(listener.Argument);
                }


                convertedEvent.AddListener(convertedEntry);
            }
        }

        private static SerializableGuid FindCreateResolverEntry(IReferenceResolver resolver, string newName, Type keyType, IValueResolver targetResolver)
        {
            SerializableGuid targetGuid = SerializableGuid.Empty;
            if (targetResolver == null)
                throw new NullReferenceException($"No targetResolver for {newName} of type {keyType?.Name}");
            
            // Find if a similar resolver already exists
            foreach (var refKey in resolver.GetKeysFor(keyType))
            {
                var registeredResolver = resolver.FindResolverByID(refKey.Guid);
                if (targetResolver.Equals(registeredResolver))
                {
                    targetGuid = refKey.Guid;
                    break;
                }
            }

            // If not, register it
            if (targetGuid.Equals(SerializableGuid.Empty))
            {
                targetGuid = resolver.Register(newName, targetResolver);
            }

            return targetGuid;
        }
        
        private static IValueResolver BuildUnityResolver(object target, Type keytype)
        {
            if (keytype == typeof(GameObject))
                return UnityValueResolver<GameObject>.Create(target as GameObject);
            else if (keytype.InheritsFrom(typeof(Component)))
            {
                var valueResolverType = typeof(UnityValueResolver<>).MakeGenericType(keytype);
                var createMethod = valueResolverType.GetMethod("Create", BindingFlags.Public | BindingFlags.Static);

                var valueResolver = createMethod.Invoke(null, new [] { target });
                return valueResolver as IValueResolver;
            }

            return null;
        }
        
        private static string FindFreeKeyName(IReferenceResolver resolver, string baseKey, IValueResolver fieldValue)
        {
            string key = baseKey;

            IValueResolver currentResolverForKey = resolver.FindResolverByName(key);
            int overrideNumber = 2;
            while (currentResolverForKey != null && !currentResolverForKey.Equals(fieldValue))
            {
                key = baseKey + overrideNumber;
                ++overrideNumber;
                currentResolverForKey = resolver.FindResolverByName(key);
            }

            return key;
        }
    }
}
#endif