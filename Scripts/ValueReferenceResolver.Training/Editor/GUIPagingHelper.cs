using System;
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
    private Rect prevRect;
    private bool isEnabled = true;
    private int elementCount;
    [SerializeField]
    private int currentPage;
    private int startIndex;
    private int endIndex;
    private int pageCount;
    private int numberOfItemsPrPage;
    private int? nextPageNumber;
    private bool? nextIsExpanded;
    /// <summary>Disables the paging, and show all elements.</summary>
    [SerializeField]
    public bool IsExpanded;

    /// <summary>
    /// Initializes a new instance of the <see cref="T:Sirenix.Utilities.Editor.GUIPagingHelper" /> class.
    /// </summary>
    public GUIPagingHelper() => this.numberOfItemsPrPage = 1;

    /// <summary>
    /// Updates all values based on <paramref name="elementCount" /> and <see cref="!:NumberOfItemsPrPage" />.
    /// </summary>
    /// <remarks>
    /// Call update right before using <see cref="P:Sirenix.Utilities.Editor.GUIPagingHelper.StartIndex" /> and <see cref="P:Sirenix.Utilities.Editor.GUIPagingHelper.EndIndex" /> in your for loop.
    /// </remarks>
    /// <param name="elementCount">The total number of elements to apply paging for.</param>
    public void Update(int elementCount)
    {
      this.elementCount = elementCount >= 0 ? elementCount : throw new ArgumentOutOfRangeException("Non-negative number required.");
      if (this.isEnabled)
      {
        this.pageCount = Mathf.Max(1, Mathf.CeilToInt((float) this.elementCount / (float) this.numberOfItemsPrPage));
        this.currentPage = Mathf.Clamp(this.currentPage, 0, this.pageCount - 1);
        this.startIndex = this.currentPage * this.numberOfItemsPrPage;
        this.endIndex = Mathf.Min(this.elementCount, this.startIndex + this.numberOfItemsPrPage);
      }
      else
      {
        this.startIndex = 0;
        this.endIndex = this.elementCount;
      }
      if (Event.current.type != UnityEngine.EventType.Layout)
        return;
      if (this.nextPageNumber.HasValue)
      {
        this.currentPage = this.nextPageNumber.Value;
        this.nextPageNumber = new int?();
      }
      if (!this.nextIsExpanded.HasValue)
        return;
      this.IsExpanded = this.nextIsExpanded.Value;
      this.nextIsExpanded = new bool?();
    }

    /// <summary>
    /// Gets or sets a value indicating whether this instance is enabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is enabled; otherwise, <c>false</c>.
    /// </value>
    public bool IsEnabled
    {
      get => this.isEnabled;
      set => this.isEnabled = value;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is on the frist page.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is on frist page; otherwise, <c>false</c>.
    /// </value>
    public bool IsOnFirstPage => this.currentPage == 0;

    /// <summary>
    /// Gets a value indicating whether this instance is on the last page.
    /// </summary>
    /// <value>
    /// <c>true</c> if this instance is on last page; otherwise, <c>false</c>.
    /// </value>
    public bool IsOnLastPage => this.currentPage == this.pageCount - 1;

    /// <summary>Gets or sets the number of items per page.</summary>
    /// <value>The number of items pr page.</value>
    public int NumberOfItemsPerPage
    {
      get => this.numberOfItemsPrPage;
      set => this.numberOfItemsPrPage = Mathf.Max(value, 0);
    }

    /// <summary>Gets or sets the current page.</summary>
    /// <value>The current page.</value>
    public int CurrentPage
    {
      get => this.currentPage;
      set => this.currentPage = Mathf.Clamp(value, 0, this.PageCount - 1);
    }

    /// <summary>Gets the start index.</summary>
    /// <value>The start index.</value>
    public int StartIndex => this.IsExpanded ? 0 : this.startIndex;

    /// <summary>Gets the end index.</summary>
    /// <value>The end index.</value>
    public int EndIndex => this.IsExpanded ? this.elementCount : this.endIndex;

    /// <summary>Gets or sets the page count.</summary>
    /// <value>The page count.</value>
    public int PageCount => this.pageCount;

    /// <summary>
    /// Gets the total number of elements.
    /// Use <see cref="M:Sirenix.Utilities.Editor.GUIPagingHelper.Update(System.Int32)" /> to change the value.
    /// </summary>
    public int ElementCount => this.elementCount;

    /// <summary>Draws right-aligned toolbar paging buttons.</summary>
    public void DrawToolbarPagingButtons(
      ref Rect toolbarRect,
      bool showPaging,
      bool showItemCount,
      int btnWidth = 23)
    {
      if ((double) this.prevRect.height == 0.0)
      {
        if (Event.current.type != UnityEngine.EventType.Repaint)
          return;
        this.prevRect = toolbarRect;
      }
      else
      {
        bool flag1 = ((!this.isEnabled ? 0 : (!this.IsExpanded ? 1 : 0)) & (showPaging ? 1 : 0)) != 0 && this.pageCount > 1;
        bool flag2 = this.isEnabled && this.pageCount > 1;
        bool flag3 = flag1;
        if (flag2)
        {
          Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
          toolbarRect.xMax = rect.xMin;
          if (GUI.Button(rect, GUIContent.none, RhinoxGUIStyles.ToolbarButton))
          {
            GUIHelper.RemoveFocusControl();
            this.nextIsExpanded = new bool?(!this.IsExpanded);
          }
          //(this.IsExpanded ? EditorIcons.TriangleUp : EditorIcons.TriangleDown).Draw(rect, 16f);
        }
        int? nextPageNumber;
        if (flag1)
        {
          Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
          if (GUI.Button(rect, GUIContent.none, RhinoxGUIStyles.ToolbarButton))
          {
            GUIHelper.RemoveFocusControl();
            if (Event.current.button == 1)
            {
              this.nextPageNumber = new int?(this.PageCount - 1);
            }
            else
            {
              this.nextPageNumber = new int?(this.currentPage + 1);
              nextPageNumber = this.nextPageNumber;
              int pageCount = this.pageCount;
              if (nextPageNumber.GetValueOrDefault() >= pageCount & nextPageNumber.HasValue)
                this.nextPageNumber = new int?(0);
            }
          }
          //EditorIcons.TriangleRight.Draw(rect, 16f);
          toolbarRect.xMax = rect.xMin;
        }
        int num1;
        if (flag3)
        {
          num1 = this.PageCount;
          string text = "/ " + num1.ToString();
          float x = RhinoxGUIStyles.Label.CalcSize(new GUIContent(text)).x;
          Rect rect1 = toolbarRect.AlignRight(x + 5f, true);
          toolbarRect.xMax = rect1.xMin;
          Rect rect2 = toolbarRect.AlignRight(x, true);
          toolbarRect.xMax = rect2.xMin;
          rect2.xMin += 4f;
          --rect2.y;
          GUI.Label(rect1, text, RhinoxGUIStyles.LabelCentered);
          int num2 = RhinoxEditorGUI.SlideRectInt(rect1, 0, this.CurrentPage);
          if (num2 != this.CurrentPage)
            this.nextPageNumber = new int?(num2);
          int num3 = EditorGUI.IntField(rect2.AlignCenterY(15f), this.CurrentPage + 1) - 1;
          if (num3 != this.CurrentPage)
            this.nextPageNumber = new int?(num3);
        }
        if (flag1)
        {
          Rect rect = toolbarRect.AlignRight((float) btnWidth, true);
          if (GUI.Button(rect, GUIContent.none, SirenixGUIStyles.ToolbarButton))
          {
            GUIHelper.RemoveFocusControl();
            if (Event.current.button == 1)
            {
              this.nextPageNumber = new int?(0);
            }
            else
            {
              this.nextPageNumber = new int?(this.currentPage - 1);
              nextPageNumber = this.nextPageNumber;
              num1 = 0;
              if (nextPageNumber.GetValueOrDefault() < num1 & nextPageNumber.HasValue)
                this.nextPageNumber = new int?(this.pageCount - 1);
            }
          }
          EditorIcons.TriangleLeft.Draw(rect, 16f);
          toolbarRect.xMax = rect.xMin;
        }
        if (showItemCount && Event.current.type != UnityEngine.EventType.Layout)
        {
          string text;
          if (this.ElementCount != 0)
          {
            num1 = this.ElementCount;
            text = num1.ToString() + " items";
          }
          else
            text = "Empty";
          GUIContent content = new GUIContent(text);
          float width = SirenixGUIStyles.LeftAlignedGreyMiniLabel.CalcSize(content).x + 5f;
          Rect position = toolbarRect.AlignRight(width);
          GUI.Label(position, content, SirenixGUIStyles.LeftAlignedGreyMiniLabel);
          toolbarRect.xMax = position.xMin;
        }
        if (Event.current.type != UnityEngine.EventType.Repaint)
          return;
        this.prevRect = toolbarRect;
      }
    }
  }
}