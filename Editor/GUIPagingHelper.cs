using System;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Data
{
    /// <summary>
    /// A helper class to control paging of n number of elements in various situations.
    /// </summary>
    public class GUIPagingHelper
    {
        private Rect _prevRect;
        private int _elementCount;
        private int _currentPage;
        private int _startIndex;
        private int _endIndex;
        private int _pageCount;
        private int _itemsPerPage;
        private int? _nextPageNumber;
        private bool? _nextIsExpanded;

        /// <summary>Disables the paging, and show all elements.</summary>
        [SerializeField] public bool IsExpanded;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sirenix.Utilities.Editor.GUIPagingHelper" /> class.
        /// </summary>
        public GUIPagingHelper()
        {
            this._itemsPerPage = 1;
        }

        public void Update(int elementCount)
        {
            this._elementCount = elementCount >= 0
                ? elementCount
                : throw new ArgumentOutOfRangeException("Non-negative number required.");
            if (this.IsEnabled)
            {
                this._pageCount = Mathf.Max(1,
                    Mathf.CeilToInt((float) this._elementCount / (float) this._itemsPerPage));
                this._currentPage = Mathf.Clamp(this._currentPage, 0, this._pageCount - 1);
                this._startIndex = this._currentPage * this._itemsPerPage;
                this._endIndex = Mathf.Min(this._elementCount, this._startIndex + this._itemsPerPage);
            }
            else
            {
                this._startIndex = 0;
                this._endIndex = this._elementCount;
            }

            if (Event.current.type != UnityEngine.EventType.Layout)
                return;
            if (this._nextPageNumber.HasValue)
            {
                this._currentPage = this._nextPageNumber.Value;
                this._nextPageNumber = new int?();
            }

            if (!this._nextIsExpanded.HasValue)
                return;
            this.IsExpanded = this._nextIsExpanded.Value;
            this._nextIsExpanded = new bool?();
        }

        public bool IsEnabled { get; set; }
        
        /// <summary>Gets or sets the number of items per page.</summary>
        /// <value>The number of items pr page.</value>
        public int NumberOfItemsPerPage { get; protected set; }

        public int CurrentPage
        {
            get => this._currentPage;
            set => this._currentPage = Mathf.Clamp(value, 0, this.PageCount - 1);
        }

        public int StartIndex => this.IsExpanded ? 0 : this._startIndex;

        public int EndIndex => this.IsExpanded ? this._elementCount : this._endIndex;

        public int PageCount { get; private set; }

        public int ElementCount { get; private set; }

        /// <summary>Draws right-aligned toolbar paging buttons.</summary>
        public void DrawToolbarPagingButtons(ref Rect toolbarRect, bool showPaging, bool showItemCount, int btnWidth = 23)
        {
            if ((double) this._prevRect.height == 0.0)
            {
                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                this._prevRect = toolbarRect;
            }
            else
            {
                bool altFlag1 = (IsEnabled && !IsExpanded && showPaging) && _pageCount > 1;
                bool flag1 = ((this.IsEnabled ? (this.IsExpanded ? 0 : 1) : 0) 
                              & (showPaging ? 1 : 0)) != 0 &&
                             this._pageCount > 1;
                bool flag2 = this.IsEnabled && this._pageCount > 1;
                if (flag2)
                {
                    Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
                    toolbarRect.xMax = rect.xMin;
                    if (GUI.Button(rect, GUIContent.none, CustomGUIStyles.ToolbarButtonCentered))
                    {
                        CustomEditorGUI.RemoveFocusControl();
                        this._nextIsExpanded = new bool?(!this.IsExpanded);
                    }
                    //(this.IsExpanded ? EditorIcons.TriangleUp : EditorIcons.TriangleDown).Draw(rect, 16f);
                }

                int? nextPageNumber;
                if (flag1)
                {
                    Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
                    if (GUI.Button(rect, GUIContent.none, CustomGUIStyles.ToolbarButtonCentered))
                    {
                        CustomEditorGUI.RemoveFocusControl();
                        if (Event.current.button == 1)
                        {
                            this._nextPageNumber = new int?(this.PageCount - 1);
                        }
                        else
                        {
                            this._nextPageNumber = new int?(this._currentPage + 1);
                            nextPageNumber = this._nextPageNumber;
                            int pageCount = this._pageCount;
                            if (nextPageNumber.GetValueOrDefault() >= pageCount & nextPageNumber.HasValue)
                                this._nextPageNumber = new int?(0);
                        }
                    }

                    //EditorIcons.TriangleRight.Draw(rect, 16f);
                    toolbarRect.xMax = rect.xMin;
                }

                int num1;
                if (flag1)
                {
                    num1 = this.PageCount;
                    string text = "/ " + num1.ToString();
                    float x = CustomGUIStyles.Label.CalcSize(new GUIContent(text)).x;
                    Rect rect1 = toolbarRect.AlignRight(x + 5f, true);
                    toolbarRect.xMax = rect1.xMin;
                    Rect rect2 = toolbarRect.AlignRight(x, true);
                    toolbarRect.xMax = rect2.xMin;
                    rect2.xMin += 4f;
                    --rect2.y;
                    GUI.Label(rect1, text, CustomGUIStyles.CenteredLabel);
                    int num2 = CustomEditorGUI.SlideRectInt(rect1, 0, this.CurrentPage);
                    if (num2 != this.CurrentPage)
                        this._nextPageNumber = new int?(num2);
                    int num3 = EditorGUI.IntField(rect2.AlignCenterVertical(15f), this.CurrentPage + 1) - 1;
                    if (num3 != this.CurrentPage)
                        this._nextPageNumber = new int?(num3);
                    
                    Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
                    if (GUI.Button(rect, GUIContent.none, CustomGUIStyles.ToolbarTab))
                    {
                        CustomEditorGUI.RemoveFocusControl();
                        if (Event.current.button == 1)
                        {
                            this._nextPageNumber = new int?(0);
                        }
                        else
                        {
                            this._nextPageNumber = new int?(this._currentPage - 1);
                            nextPageNumber = this._nextPageNumber;
                            num1 = 0;
                            if (nextPageNumber.GetValueOrDefault() < num1 & nextPageNumber.HasValue)
                                this._nextPageNumber = new int?(this._pageCount - 1);
                        }
                    }

                    Texture t = UnityIcon.AssetIcon("Fa_AngleLeft");
                    EditorGUI.DrawTextureTransparent(rect, t);
                    toolbarRect.xMax = rect.xMin;
                }

                if (showItemCount && Event.current.type != UnityEngine.EventType.Layout)
                {
                    string text = ElementCount != 0 ? $"{ElementCount} items" : "Empty";

                    GUIContent content = new GUIContent(text);
                    float width = CustomGUIStyles.MiniLabel.CalcSize(content).x + 5f;
                    Rect position = toolbarRect.AlignRight(width);
                    GUI.Label(position, content, CustomGUIStyles.MiniLabel);
                    toolbarRect.xMax = position.xMin;
                }

                if (Event.current.type != UnityEngine.EventType.Repaint)
                    return;
                this._prevRect = toolbarRect;
            }
        }
    }
}