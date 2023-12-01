using System;
using Rhinox.Perceptor;
using Sirenix.OdinInspector;
using UnityEngine.UI;

namespace Rhinox.Magnus.Tasks
{
    public class OtherButtonClickCondition : BaseCondition, IDataDrivenObject<OtherButtonClickCondition.ButtonClickConditionData>
    {
        [Serializable]
        public class ButtonClickConditionData : BaseDataDriverObject
        {
            public Button ButtonToPress;
        }

        public ButtonClickConditionData Data;

        protected override bool OnInit()
        {
            return Data.ButtonToPress != null;
        }

        public override void Terminate()
        {
            base.Terminate();
            if (Data.ButtonToPress != null)
                Data.ButtonToPress.onClick.RemoveListener(HandleClick);
        }

        public override void Start()
        {
            base.Start();

            // ensure it is enabled
            Data.ButtonToPress.gameObject.SetActive(true);
            Data.ButtonToPress.interactable = true;

            if (!Data.ButtonToPress.isActiveAndEnabled)
            {
                SetConditionMet();
                PLog.Error<MagnusLogger>("Autocompleted ButtonClickCondition due to unavailable button.",
                    associatedObject: Step);
                return;
            }

            Data.ButtonToPress.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            SetConditionMet();
        }

        public ButtonClickConditionData FindData()
        {
            return Data;
        }

        public object FindDataGeneric()
        {
            return Data;
        }
    }
}