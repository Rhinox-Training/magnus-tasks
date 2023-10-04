using System;
using System.Collections.Generic;
using System.Linq;
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
    public BaseTask Task;
    public int StepToSkipTo = -1;
    private const int _frameWait = 5;

    private void Update()
    {
        if (Time.frameCount % _frameWait != 0)
            return;

        if (!AutoCompletor.Instance.IsIdle)
            return;
        
        if (!ShouldAutoCompleteStep())
            return;
        
        PLog.TraceDetailed<MagnusLogger>($"Enqueuing Autocomplete");
        AutoCompletor.Instance.Autocomplete();
    }

    private bool ShouldAutoCompleteStep()
    {
        if (!TaskManager.HasInstance || TaskManager.Instance.CurrentTask == null)
            return false;

        if (TaskManager.Instance.CurrentTask != Task)
            return false;

        return StepToSkipTo < 0 || TaskManager.Instance.CurrentTask.CurrentStepId < StepToSkipTo;
    }

    private ICollection<ValueDropdownItem> GetTasks()
    {
        if (!TaskManager.HasInstance)
            return Array.Empty<ValueDropdownItem>();

        return TaskManager.Instance.GetTasks().Select(x => new ValueDropdownItem(x.name, x)).ToArray();
    }
}