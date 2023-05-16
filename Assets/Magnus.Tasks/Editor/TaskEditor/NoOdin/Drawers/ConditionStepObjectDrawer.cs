using Rhinox.GUIUtils.Editor;
using Rhinox.VOLT.Data;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor.NoOdin
{
    [CustomPropertyDrawer(typeof(ConditionStepObject))]
    public class ConditionStepObjectDrawer : BasePropertyDrawer<ConditionStepObject>
    {
        private float _labelHeight;
        private float _horizontalMargin;
        private DrawablePropertyView _basePropertyView;

        protected override void Initialize()
        {
            base.Initialize();
            _labelHeight = base.GetPropertyHeight(new GUIContent("SAMPLE_LABEL"));
            _horizontalMargin = 0.05f;

            if (_basePropertyView == null)
            { 
                //_basePropertyView = new DrawablePropertyView(SmartValue);
            }
        }

        protected override void DrawProperty(Rect position, GUIContent label)
        {
            if (_basePropertyView != null)
            {
                //_basePropertyView.Draw(position);
            }
            else
            {
                Rect controlRect = GetControlRect(position);
                Rect prefixRect = controlRect;
                prefixRect.width /= 4;
                prefixRect.height = _labelHeight;

                Rect valueRect = controlRect;
                valueRect.x = prefixRect.width * 2;
                valueRect.width = valueRect.width / 4 * 2;
                valueRect.height = _labelHeight;

                GUI.Label(prefixRect, "ID");
                GUI.Label(valueRect, SmartValue.ID.ToString());
                prefixRect.y += _labelHeight;
                valueRect.y += _labelHeight;

                GUI.Label(prefixRect, "Name");
                SmartValue.Name = GUI.TextField(valueRect, SmartValue.Name);
                prefixRect.y += _labelHeight;
                valueRect.y += _labelHeight;
                valueRect.height = 3 * _labelHeight;

                GUI.Label(prefixRect, "Description");
                SmartValue.Description = GUI.TextField(valueRect, SmartValue.Description);
                prefixRect.y += _labelHeight;
                valueRect.y += valueRect.height;

                valueRect.x = prefixRect.x;
                valueRect.height = _labelHeight + SmartValue.SubStepData.Count * _labelHeight;
                
            }
        }

        private Rect GetControlRect(Rect position)
        {
            int horizontalMarginPx = (int)(position.width * _horizontalMargin);
            Rect controlRect = new Rect()
            {
                x = position.x + horizontalMarginPx,
                y = position.y,
                width = position.width - 2 * horizontalMarginPx,
                height = position.height
            };

            return controlRect;
        }

        protected override float GetPropertyHeight(GUIContent label)
        {
            float fieldHeight = 5 * _labelHeight;
            float subStepDataHeight = _labelHeight + SmartValue.SubStepData.Count * _labelHeight;
            return fieldHeight + subStepDataHeight;
        }
    }
}