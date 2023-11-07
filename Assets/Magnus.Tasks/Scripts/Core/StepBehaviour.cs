using Rhinox.GUIUtils.Attributes;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Rhinox.Magnus.Tasks
{
    [SmartFallbackDrawn]
    public class StepBehaviour : MonoBehaviour
    {
        [SerializeReference, HideLabel]
        public StepData Step;
    }
}