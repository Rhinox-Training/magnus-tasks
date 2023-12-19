using System;
using System.Collections;
using System.Collections.Generic;
using Rhinox.GUIUtils;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using UnityEditor;
using UnityEngine;

namespace Rhinox.Magnus.Tasks.Editor
{
    public class SearchablePagedDrawerHelper : PagedDrawerHelper
    {
        private ICollection _optionsCache;

        public bool SearchFieldEnabled;
        public string SearchText;
        public bool RequiresRefresh { get; private set; }
        public int VisibleLines => Math.Min(NumberOfItemsPerPage, _optionsCache?.Count ?? 0);

        private Rect _prevRect;

        public delegate void SearchTextChangedHandler(string searchText);
        public event SearchTextChangedHandler SearchTextChanged;

        public SearchablePagedDrawerHelper(int itemsPerPage, bool searchFieldEnabled = false)
            : base(itemsPerPage)
        {
            this.SearchFieldEnabled = searchFieldEnabled;
        }
        
        public Rect BeginDrawPagerLayout(ICollection options, bool showPaging = true, bool showItemCount = true)
        {
            _optionsCache = options;
            Resize(_optionsCache.Count);
            
            EditorGUILayout.BeginVertical();
            var toolbarRect = CustomEditorGUI.BeginHorizontalToolbar();
            {
                DrawHeaderPagingButtons(ref toolbarRect, showPaging, showItemCount);
                DrawSearchField(toolbarRect);
            }
            CustomEditorGUI.EndHorizontalToolbar();
            
            if (Event.current.type == EventType.Repaint)
                _prevRect = toolbarRect;
            
            return toolbarRect;
        }
        
        public void BeginDrawPager(ref Rect pageRect, ICollection options, bool showPaging = true, bool showItemCount = true)
        {
            _optionsCache = options;
            Resize(_optionsCache.Count);

            var headerRect = pageRect.SetHeight(EditorGUIUtility.singleLineHeight);
            CustomGUIStyles.ToolbarBackground.Draw(headerRect);
            DrawHeaderPagingButtons(ref headerRect, showPaging, showItemCount);
            DrawSearchField(headerRect);
        }

        private void DrawSearchField(Rect searchFieldRect)
        {
            if (!SearchFieldEnabled || !searchFieldRect.IsValid())
                return;

            var position = searchFieldRect;
            position.height = EditorGUIUtility.singleLineHeight;
            position.yMin += 3;

            var newSearchText = CustomEditorGUI.ToolbarSearchField(position, SearchText);
            if (newSearchText != SearchText)
            {
                SearchText = newSearchText;
                RequiresRefresh = true;
            }

        }

        public void Refresh()
        {
            if (_optionsCache == null)
                Resize(0);
            else
                Resize(_optionsCache.Count);
        }

        public void EndDrawPagerLayout()
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

        public void EndDrawPager()
        {
            Resize(_optionsCache.Count);
            _optionsCache = null;
            
            if (RequiresRefresh)
            {
                SearchTextChanged?.Invoke(SearchText);
                RequiresRefresh = false;
            }
        }
    }
}