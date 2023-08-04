using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils.Editor;
using Rhinox.Utilities.Editor;
using Rhinox.VOLT.Data;
using UnityEditor;
using UnityEngine;

namespace Rhinox.VOLT.Editor
{
    public class BetterGUIPagingHelper : GUIPagingHelper
    {
        private ICollection _optionsCache;

        public bool SearchField;
        public string SearchText;
        public bool RequiresRefresh { get; private set; }

        private Rect prevRect;

        public delegate void SearchTextChangedHandler(string searchText);
        public event SearchTextChangedHandler SearchTextChanged;

        public BetterGUIPagingHelper(int itemsPerPage, bool searchField = false)
        {
            this.NumberOfItemsPerPage = itemsPerPage;
            this.SearchField = searchField;
        }
        
        public Rect BeginDrawPager(ICollection options, bool showPaging = true, bool showItemCount = true)
        {
            _optionsCache = options;
            Update(_optionsCache.Count);
            
            //EditorGUILayout.BeginToolbarBox();
            EditorGUILayout.BeginVertical();
            var toolbarRect = CustomEditorGUI.BeginHorizontalToolbar();
            
            DrawSearchField();
            DrawToolbarPagingButtons(ref toolbarRect, showPaging, showItemCount);
            
            CustomEditorGUI.EndHorizontalToolbar();
            
            if (Event.current.type == EventType.Repaint)
                prevRect = toolbarRect;
            
            return toolbarRect;
        }

        private void DrawSearchField()
        {
            if (prevRect.height == 0.0)
                return;
            
            if (SearchField)
            {
                var searchRect = prevRect;
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
                Update(0);
            else
                Update(_optionsCache.Count);
        }

        public void EndDrawPager()
        {
            Update(_optionsCache.Count);
            _optionsCache = null;
            //SirenixEditorGUI.EndToolbarBox();
            EditorGUILayout.EndVertical();

            if (RequiresRefresh)
            {
                SearchTextChanged?.Invoke(SearchText);
                RequiresRefresh = false;
            }
        }
    }
}