using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

public class TagContainerDrawer : OdinValueDrawer<TagContainer>
{
    private DataLayerConfig _dataLayerConfig;

    public static class Styles
    {
        private static GUIStyle _tagCardStyle;
        public static GUIStyle TagCardStyle => _tagCardStyle ?? (_tagCardStyle = new GUIStyle(SirenixGUIStyles.CardStyle)
        {
            padding = new RectOffset(0, 0, 0, 0),
            
        });
        
        private static GUIStyle _tagLabelStyle;
        public static GUIStyle TagLabelStyle => _tagLabelStyle ?? (_tagLabelStyle = new GUIStyle(SirenixGUIStyles.CenteredWhiteMiniLabel)
        {
            margin = new RectOffset(2, 0, 0, 0),
        });
    }
    
    
    protected override void Initialize()
    {
        base.Initialize();
        _dataLayerConfig = Property.FindDataLayerResolver();
    }

    protected override void DrawPropertyLayout(GUIContent label)
    {
        TagContainer container = ValueEntry.SmartValue;
        // SirenixEditorGUI.HorizontalLineSeparator but it respects indent
        var r = GUILayoutUtility.GetRect(1, 1, GUILayoutOptions.ExpandWidth(true));
        r = EditorGUI.IndentedRect(r);
        SirenixEditorGUI.DrawSolidRect(r, SirenixGUIStyles.BorderColor);
        
        List<string> _tagsToRemove = new List<string>();
        
        using (var group = new eUtility.HorizontalGroup())
        {
            SirenixEditorGUI.IndentSpace();
            using (new eUtility.VerticalGroup())
            {
                GUILayout.Space(2);

                var maxWidth = GUIHelper.ContextWidth - 40; // - margins & btn
                var currentWidth = 0f;
                // Draw all Tags
                using (new eUtility.HorizontalGroup())
                {
                    if (container.Tags.IsNullOrEmpty())
                        GUILayout.Label(GUIHelper.TempContent("Click the + to add some tags"), SirenixGUIStyles.LeftAlignedGreyMiniLabel);
                    
                    foreach (var tag in container.Tags)
                    {
                        const int iconSize = 14;
                        
                        var content = GUIHelper.TempContent(tag);
                        var width = Styles.TagCardStyle.CalcSize(GUIContent.none).x + Styles.TagLabelStyle.CalcSize(content).x;
                        width += iconSize + 2; // icon & spacer 
                        if (currentWidth + width > maxWidth)
                        {
                            EditorGUILayout.EndHorizontal();
                            GUILayout.Space(2);
                            GUILayout.BeginHorizontal();
                            currentWidth = 0f;
                        }
                    
                        GUILayout.BeginHorizontal(Styles.TagCardStyle);
                        GUILayout.Label(content, Styles.TagLabelStyle);
                        if (SirenixEditorGUI.IconButton(EditorIcons.X, iconSize, iconSize))
                            _tagsToRemove.Add(tag);
                        GUILayout.EndHorizontal();
                        GUILayout.Space(2);

                        currentWidth += width;
                    }
                
                    container.Tags.RemoveRange(_tagsToRemove);
                    _tagsToRemove.Clear();
                    
                    GUILayout.FlexibleSpace();
                }
                
                GUILayout.Space(3);
            }
            
            // Draw Add btn
            SirenixEditorGUI.BeginBox();
            if (SirenixEditorGUI.IconButton(EditorIcons.Plus, 14, 14))
            {
                OpenSelector(group.Rect, container);
            }
            SirenixEditorGUI.EndBox();
        }
        
    }

    private void OpenSelector(Rect r, TagContainer container)
    {
        // this.ReloadDropdownCollections();
        r = new Rect(r) { y = Event.current.mousePosition.y };

        var selector = ShowSelector(container, r);
        selector.SelectionConfirmed += x =>
        {
            container.Tags.AddRange(x);
        };
    }

    private OdinSelector<string> ShowSelector(TagContainer container, Rect rect)
    {
        GenericSelector<string> selector = this.CreateSelector(container);
        var btnRect = GUIHelper.GetCurrentLayoutRect();
        rect.x = (int) rect.x;
        rect.y = btnRect.yMin - btnRect.height;
        rect.width = (int) rect.width;
        rect.height = (int) rect.height;
        // if (this.Attribute.AppendNextDrawer && !this.isList)
        rect.xMax = btnRect.xMin;
        selector.ShowInPopup(rect, Vector2.zero);
        return selector;
    }

    private ICollection<ValueDropdownItem<string>> GetValues(TagContainer container)
    {
        DataLayer.PushEndPointFromConfigOrDefault(_dataLayerConfig);
        var table = DataLayer.GetTable<TagData>();
        if (table == null)
        {
            DataLayer.PopEndPoint();
            PLog.Warn<MagnusLogger>("No Tag table set up");
            return Array.Empty<ValueDropdownItem<string>>();
        }
        DataLayer.PopEndPoint();
        return table.GetAllData()
            .Where(x => !container.Tags.Contains(x.Name))
            .Select(x => new ValueDropdownItem<string>(x.Name, x.Name))
            .ToArray();
    }

    private GenericSelector<string> CreateSelector(TagContainer container)
    {
        IEnumerable<ValueDropdownItem<string>> source = GetValues(container) ?? Enumerable.Empty<ValueDropdownItem<string>>();

        
        GenericSelector<string> genericSelector = new GenericSelector<string>(
            "Add new Tag", false,
            source.Select(x => new GenericSelectorItem<string>(x.Text, x.Value))
        );
        
        return genericSelector;
    }
}