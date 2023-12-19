using System;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    /// <summary>
    /// A helper class to control paging of n number of elements in various situations.
    /// </summary>
    public class PagedDrawerHelper
    {
        public int NumberOfItemsPerPage => _itemsPerPage;

        public int CurrentPage
        {
            get => this._currentPage;
            set => this._currentPage = Mathf.Clamp(value, 0, this.PageCount - 1);
        }

        public int StartIndex => this.ExpandAllPages ? 0 : this._startIndex;

        public int EndIndex => this.ExpandAllPages ? this._elementCount : this._endIndex;

        public int PageCount { get; private set; }

        public int ElementCount { get; private set; }

        
        
        public bool ExpandAllPages;

        private Rect _prevRect;
        private int _elementCount;
        private int _currentPage;
        private int _startIndex;
        private int _endIndex;
        private int _pageCount;
        private int _itemsPerPage;
        private int? _nextPageNumber;
        private bool? _nextIsExpanded;

        public PagedDrawerHelper(int itemsPerPage = 1)
        {
            this._itemsPerPage = itemsPerPage;
        }

        public void DrawHeaderPagingButtons(ref Rect toolbarRect, bool showPaging, bool showItemCount, int btnWidth = 23)
        {
            if (!toolbarRect.IsValid())
                return;
            
            // if (!_prevRect.IsValid())
            // {
            //     if (Event.current.type == UnityEngine.EventType.Repaint)
            //         this._prevRect = toolbarRect;
            // }
            // else
            {
                if (this._pageCount > 1)
                {
                    toolbarRect.SplitX(toolbarRect.width - btnWidth, out toolbarRect, out Rect rect);
                    if (GUI.Button(rect, GUIContent.none, CustomGUIStyles.ToolbarButtonCentered))
                    {
                        CustomEditorGUI.RemoveFocusControl();
                        this._nextIsExpanded = new bool?(!this.ExpandAllPages);
                    }
                }

                bool drawPagerButtons = !this.ExpandAllPages && showPaging && _pageCount > 1;
                if (drawPagerButtons)
                    DrawPagerButtons(toolbarRect, btnWidth);

                if (showItemCount)
                {
                    var content = GUIContentHelper.TempContent(ElementCount != 0 ? $"{ElementCount} items" : "Empty");
                    float lblWidth = CustomGUIStyles.MiniLabel.CalcSize(content).x + 5f;
                    toolbarRect.SplitX(toolbarRect.width - lblWidth, out toolbarRect, out Rect lblRect);
                    GUI.Label(lblRect, content, CustomGUIStyles.MiniLabel);
                }

                // if (Event.current.type != UnityEngine.EventType.Repaint)
                //     return;
                // this._prevRect = toolbarRect;
            }
        }

        private void DrawPagerButtons(Rect toolbarRect, int btnWidth)
        {
            int? nextPageNumber;
            Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
            if (GUI.Button(rect, GUIContent.none, CustomGUIStyles.ToolbarButtonCentered))
            {
                CustomEditorGUI.RemoveFocusControl();
                if (Event.current.button == 1) // Right mouse click
                {
                    this._nextPageNumber = new int?(this.PageCount - 1);
                }
                else
                {
                    this._nextPageNumber = new int?(this._currentPage + 1);
                    nextPageNumber = this._nextPageNumber;
                    if (nextPageNumber.GetValueOrDefault() >= _pageCount & nextPageNumber.HasValue)
                        this._nextPageNumber = new int?(0);
                }
            }

            toolbarRect.xMax = rect.xMin;

            int pageCount = this.PageCount;
            string text = "/ " + pageCount.ToString();

            // Rect rect1 = toolbarRect.AlignRightForText(text, CustomGUIStyles.Label, 5f);
            // toolbarRect.xMax = rect1.xMin;
            // Rect rect2 = toolbarRect.AlignRightBefore(x, rect1);
            // toolbarRect.xMax = rect2.xMin;

            float x = CustomGUIStyles.Label.CalcSize(new GUIContent(text)).x;
            Rect rect1 = toolbarRect.AlignRight(x + 5f, true);
            toolbarRect.xMax = rect1.xMin;
            Rect rect2 = toolbarRect.AlignRight(x - 4f, true);
            toolbarRect.xMax = rect2.xMin;
            rect2.xMin += 4f;
            --rect2.y;
            GUI.Label(rect1, text, CustomGUIStyles.CenteredLabel);
            int pageIndex = CustomEditorGUI.TrackMouseDragForIntegerChange(rect1, 0, this.CurrentPage);
            if (pageIndex != this.CurrentPage)
                this._nextPageNumber = new int?(pageIndex);
            int num3 = EditorGUI.IntField(rect2.AlignCenterVertical(15f), this.CurrentPage + 1) - 1;
            if (num3 != this.CurrentPage)
                this._nextPageNumber = new int?(num3);

            rect = toolbarRect.AlignRight((float) btnWidth, true);
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
                    pageCount = 0;
                    if (nextPageNumber.GetValueOrDefault() < pageCount & nextPageNumber.HasValue)
                        this._nextPageNumber = new int?(this._pageCount - 1);
                }
            }

            Texture t = UnityIcon.AssetIcon("Fa_AngleLeft");
            EditorGUI.DrawTextureTransparent(rect, t);
            toolbarRect.xMax = rect.xMin;
        }

        protected void Resize(int elementCount)
        {
            this._elementCount = elementCount >= 0
                ? elementCount
                : throw new ArgumentOutOfRangeException("Non-negative number required.");

            this._pageCount = Mathf.Max(1,
                Mathf.CeilToInt((float) this._elementCount / (float) this._itemsPerPage));
            this._currentPage = Mathf.Clamp(this._currentPage, 0, this._pageCount - 1);
            this._startIndex = this._currentPage * this._itemsPerPage;
            this._endIndex = Mathf.Min(this._elementCount, this._startIndex + this._itemsPerPage);

            if (Event.current.type != UnityEngine.EventType.Layout)
                return;
            if (this._nextPageNumber.HasValue)
            {
                this._currentPage = this._nextPageNumber.Value;
                this._nextPageNumber = new int?();
            }

            if (!this._nextIsExpanded.HasValue)
                return;
            this.ExpandAllPages = this._nextIsExpanded.Value;
            this._nextIsExpanded = new bool?();
        }
    }
}