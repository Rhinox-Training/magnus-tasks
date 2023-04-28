using System;
using System.Linq;
using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using Rhinox.Utilities;
using Rhinox.VOLT;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class ButtonClickCondition : BaseCondition
{
    [NotConvertedToDataLayer]
    public Button ButtonToPress;
    private IValueResolver ImportButtonToPress() => UnityValueResolver<Button>.Create(ButtonToPress);

    [ValueReference(typeof(Button), "Button")]
    [ImportValueForValueReference(nameof(ImportButtonToPress))]
    public SerializableGuid ButtonIdentifier;
    
    protected override bool OnInit()
    {
        Resolve(ButtonIdentifier, ref ButtonToPress);
        return ButtonToPress != null;
    }
    
    public override void Terminate()
    {
        base.Terminate();
        // ButtonToPress.interactable = false;
        if (ButtonToPress != null)
            ButtonToPress.onClick.RemoveListener(SetConditionMet);
    }
    
    public override void Start()
    {
        base.Start();
        
        // ensure it is enabled
        ButtonToPress.gameObject.SetActive(true);
        ButtonToPress.interactable = true;

        if (!ButtonToPress.isActiveAndEnabled)
        {
            SetConditionMet();
            PLog.Error<VOLTLogger>("Autocompleted ButtonClickCondition due to unavailable button.", associatedObject: Step);
            return;
        }
        
        ButtonToPress.onClick.AddListener(SetConditionMet);
    }
}