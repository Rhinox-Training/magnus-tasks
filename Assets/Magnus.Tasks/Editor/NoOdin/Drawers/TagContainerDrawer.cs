using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Utilities;
using Rhinox.Lightspeed;
using Rhinox.Magnus;
using Rhinox.Magnus.Tasks;
using Rhinox.Perceptor;
using Rhinox.VOLT;
using Rhinox.VOLT.Data;
using Rhinox.VOLT.Training;
using Rhinox.Vortex;
using Rhinox.Vortex.Editor;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    [CustomPropertyDrawer(typeof(TagContainer), true)]
    public class TagContainerDrawer : BasePropertyDrawer<TagContainer>
    {
        private DataLayerConfig _dataLayerConfig;

        public static class Styles
        {
            private static GUIStyle _tagCardStyle;

            public static GUIStyle TagCardStyle => _tagCardStyle ?? (_tagCardStyle =
                new GUIStyle(CustomGUIStyles.Box)
                {
                    padding = new RectOffset(0, 0, 0, 0),

                });

            private static GUIStyle _tagLabelStyle;

            public static GUIStyle TagLabelStyle => _tagLabelStyle ?? (_tagLabelStyle =
                new GUIStyle(CustomGUIStyles.MiniLabelCentered)
                {
                    margin = new RectOffset(2, 0, 0, 0),
                });
        }

        protected override void OnUpdateActiveData()
        {
            base.OnUpdateActiveData();
            _dataLayerConfig = FindDataLayerResolver();
        }

        private DataLayerConfig FindDataLayerResolver()
        {
            if (!FindAttribute<DataLayerConfigResolverAttribute>(out var hostInfo, out var processorAttribute))
                return null;

            var propertyHelper = MemberHelper.Create<DataLayerConfig>(hostInfo.GetValue(), processorAttribute.MemberName);
            var referenceResolver = propertyHelper.ForceGetValue();
            return referenceResolver;
        }

        protected override void DrawProperty(Rect r, ref GenericHostInfo data, GUIContent label)
        {
            TagContainer container = SmartValue;
            // SirenixEditorGUI.HorizontalLineSeparator but it respects indent
            r = EditorGUI.IndentedRect(r);
            CustomEditorGUI.DrawSolidRect(r, CustomGUIStyles.BorderColor);

            List<string> _tagsToRemove = new List<string>();

            using (var group = new eUtility.HorizontalGroup())
            {
                var innerRect = r.SetHeight(r.height - 5.0f).AddY(2.0f).SetWidth(r.width - 4.0f).AddX(2.0f);
                var maxWidth = CustomEditorGUI.ContextWidth() - 40; // - margins & btn
                var currentWidth = 0f;
                // Draw all Tags
                if (container.Tags.IsNullOrEmpty())
                {
                    GUI.Label(innerRect, GUIContentHelper.TempContent("Click the + to add some tags"), CustomGUIStyles.MiniLabel);
                }
                else
                {
                    var tagRect = innerRect;
                    foreach (var tag in container.Tags)
                    {
                        const int iconSize = 14;

                        var content = GUIContentHelper.TempContent(tag);
                        var width = Styles.TagCardStyle.CalcSize(GUIContent.none).x + Styles.TagLabelStyle.CalcSize(content).x;
                        width += iconSize + 4; // icon & spacer 
                        if (currentWidth + width > maxWidth)
                        {
                            // TODO: support multiline
                            // EditorGUILayout.EndHorizontal();
                            // GUILayout.Space(2);
                            // GUILayout.BeginHorizontal();
                            // currentWidth = 0f;
                        }

                        tagRect = tagRect.AlignLeft(width);

                        GUI.Box(tagRect, GUIContent.none, Styles.TagCardStyle);
                        GUI.Label(tagRect.HorizontalPadding(2.0f).PadRight(iconSize), content, Styles.TagLabelStyle);
                        if (CustomEditorGUI.IconButton(tagRect.AlignRight(iconSize), UnityIcon.AssetIcon("Fa_Times")))
                            _tagsToRemove.Add(tag);

                        tagRect = tagRect.AddX(width + 2.0f);

                        currentWidth += width;
                    }
                }

                container.Tags.RemoveRange(_tagsToRemove);
                _tagsToRemove.Clear();

                // Draw Add btn
                //CustomEditorGUI.BeginBox();
                var plusIconRect = innerRect.SetHeight(14).AlignRight(14);
                if (CustomEditorGUI.IconButton(plusIconRect, UnityIcon.AssetIcon("Fa_Plus")))
                {
                    OpenSelector(r, container);
                }
                //SirenixEditorGUI.EndBox();
            }

        }

        private void OpenSelector(Rect r, TagContainer container)
        {
            r = new Rect(r) {y = Event.current.mousePosition.y};

            ICollection<string> source = GetValues(container) ?? Array.Empty<string>();
            GenericPicker.Show<string>(r, source, x =>
            {
                if (!string.IsNullOrEmpty(x))
                    container.Tags.Add(x);
            });
        }

        private ICollection<string> GetValues(TagContainer container)
        {
            DataLayer.PushEndPointFromConfigOrDefault(_dataLayerConfig);
            var table = DataLayer.GetTable<TagData>();
            if (table == null)
            {
                DataLayer.PopEndPoint();
                PLog.Warn<MagnusLogger>("No Tag table set up");
                return Array.Empty<string>();
            }

            DataLayer.PopEndPoint();
            return table.GetAllData()
                .Where(x => !container.Tags.Contains(x.Name))
                .Select(x => x.Name)
                .ToArray();
        }
    }
}