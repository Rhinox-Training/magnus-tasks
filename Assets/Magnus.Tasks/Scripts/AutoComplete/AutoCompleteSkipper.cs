using System;
using System.Collections.Generic;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using Rhinox.Utilities.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

[ExecuteAfter(typeof(AutoCompletor))]
public class AutoCompleteSkipper : MonoBehaviour
{
    [ValueDropdown(nameof(GetTasks))]
    public ITaskObjectState Task;
    public SerializableGuid StepIDToSkipTo;
    private const int _frameWait = 5;

    private void Update()
    {
        if (Time.frameCount % _frameWait != 0)
            return;

        if (!AutoCompletor.Instance.IsIdle)
            return;
        
        if (!Task.DoesActiveStepPrecede(StepIDToSkipTo))
            return;
        
        PLog.TraceDetailed<MagnusLogger>($"Enqueuing Autocomplete");
        AutoCompletor.Instance.Autocomplete();
    }

    private ICollection<ValueDropdownItem> GetTasks()
    {
        if (!TaskManager.HasInstance)
            return Array.Empty<ValueDropdownItem>();

        return TaskManager.Instance.GetTasks().Select(x => new ValueDropdownItem(x.Name, x)).ToArray();
    }
}