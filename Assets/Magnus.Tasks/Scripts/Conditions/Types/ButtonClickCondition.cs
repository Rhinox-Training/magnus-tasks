using Rhinox.Lightspeed;
using Rhinox.Perceptor;
using UnityEngine.UI;

namespace Rhinox.Magnus.Tasks
{
    [RefactoringOldNamespace("", "com.rhinox.volt.domain")]
    public class ButtonClickCondition : BaseCondition
    {
        public Button ButtonToPress;
        // private IValueResolver ImportButtonToPress() => UnityValueResolver<Button>.Create(ButtonToPress);
        //
        // [ValueReference(typeof(Button), "Button")] [ImportValueForValueReference(nameof(ImportButtonToPress))]
        // public SerializableGuid ButtonIdentifier;

        protected override bool OnInit()
        {
            //Resolve(ButtonIdentifier, ref ButtonToPress);
            return ButtonToPress != null;
        }

        public override void Terminate()
        {
            base.Terminate();
            // ButtonToPress.interactable = false;
            if (ButtonToPress != null)
                ButtonToPress.onClick.RemoveListener(HandleClick);
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
                PLog.Error<MagnusLogger>("Autocompleted ButtonClickCondition due to unavailable button.");
                return;
            }

            ButtonToPress.onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            SetConditionMet();
        }
    }
}