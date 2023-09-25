using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Editor;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class SearchablePagedDrawerHelper : PagedDrawerHelper
    {
        private ICollection _optionsCache;

        public bool SearchField;
        public string SearchText;
        public bool RequiresRefresh { get; private set; }

        private Rect _prevRect;

        public delegate void SearchTextChangedHandler(string searchText);
        public event SearchTextChangedHandler SearchTextChanged;

        public SearchablePagedDrawerHelper(int itemsPerPage, bool searchField = false)
            : base(itemsPerPage)
        {
            this.SearchField = searchField;
        }
        
        public Rect BeginDrawPager(ICollection options, bool showPaging = true, bool showItemCount = true)
        {
            _optionsCache = options;
            Resize(_optionsCache.Count);
            
            EditorGUILayout.BeginVertical();
            var toolbarRect = CustomEditorGUI.BeginHorizontalToolbar();
            {
                DrawSearchField();
                DrawHeaderPagingButtons(ref toolbarRect, showPaging, showItemCount);
            }
            CustomEditorGUI.EndHorizontalToolbar();
            
            if (Event.current.type == EventType.Repaint)
                _prevRect = toolbarRect;
            
            return toolbarRect;
        }

        private void DrawSearchField()
        {
            if (_prevRect.height == 0.0)
                return;
            
            if (SearchField)
            {
                var searchRect = _prevRect;
                searchRect.yMin += 3;

                var newSearchText = CustomEditorGUI.ToolbarSearchField(searchRect, SearchText);
                if (newSearchText != SearchText)
                {
                    SearchText = newSearchText;
                    RequiresRefresh = true;
                }
            }
        }

        public void Refresh()
        {
            if (_optionsCache == null)
                Resize(0);
            else
                Resize(_optionsCache.Count);
        }

        public void EndDrawPager()
        {
            Resize(_optionsCache.Count);
            _optionsCache = null;
            EditorGUILayout.EndVertical();

            if (RequiresRefresh)
            {
                SearchTextChanged?.Invoke(SearchText);
                RequiresRefresh = false;
            }
        }
    }
}