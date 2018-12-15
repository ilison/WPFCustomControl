#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
#if UWP
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Input;
    using Windows.UI.Core;
    using System.Diagnostics;
    using Windows.Foundation;
    using Windows.UI.Xaml;
#endif
    /// <summary>
    /// Provides the base implementation for column resizing operations in SfTreeGrid.
    /// </summary>
    public class TreeGridColumnResizingController : IDisposable
    {

        #region Resizing Fields

        SfTreeGrid treeGrid;
        private const double HitTestPrecision = 4.0;
        private const double HitTestHiddenColPrecision = 6.0;
        private bool isFirstColumnHidden;

        //To indicate the column which is in resizing currently
        private VisibleLineInfo currentResizingColumn;

        #endregion

        #region Internal property

        internal bool isHovering { get; set; }
        internal VisibleLineInfo dragLine { get; set; }
        internal int HiddenLineIndex { get; set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        public TreeGridColumnResizingController(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
        }

#if UWP
        private CoreCursorType resizingCursor = CoreCursorType.SizeWestEast;
        private CoreCursorType unhideCursor = CoreCursorType.SizeNorthwestSoutheast;

        public CoreCursorType ResizingCursor
        {
            get { return resizingCursor; }
            set { resizingCursor = value; }
        }

        public CoreCursorType UnhideCursor
        {
            get { return unhideCursor; }
            set { unhideCursor = value; }
        }
#else
        private Cursor resizingCursor = Cursors.SizeWE;
        private Cursor unhideCursor = Cursors.SizeNWSE;

        /// <summary>
        /// Gets or sets the cursor that is displayed when the column is resized using mouse pointer in SfTreeGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Input.Cursor"/> that represents the cursor to display when the column is resized. 
        /// </value>
        public Cursor ResizingCursor
        {
            get { return resizingCursor; }
            set { resizingCursor = value; }
        }

        /// <summary>
        /// Gets or sets the cursor that is displayed when the column is hidden using mouse pointer in SfTreeGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Input.Cursor"/> that represents the cursor to display when the column is hidden.
        /// </value>
        public Cursor UnhideCursor
        {
            get { return unhideCursor; }
            set { unhideCursor = value; }
        }
#endif

        #region Resizing

        /// <summary>
        /// Ensures the VSM for hidden columns OnTreeColumnCollectionChanged
        /// </summary>
        /// <param name="OldStartingIndex"></param>
        /// <param name="NewStartingIndex"></param>
        /// <remarks></remarks>
        internal void EnsureVSMOnColumnCollectionChanged(int OldStartingIndex, int NewStartingIndex)
        {
            if (OldStartingIndex != -1 && OldStartingIndex >= 0 && OldStartingIndex < this.treeGrid.Columns.Count)
                this.ProcessResizeStateManager(this.treeGrid.Columns[OldStartingIndex]);
            if (NewStartingIndex != -1 && NewStartingIndex >= 0 && NewStartingIndex < this.treeGrid.Columns.Count)
                this.ProcessResizeStateManager(this.treeGrid.Columns[NewStartingIndex]);
        }

        #region Resizing by mouse

        /// <summary>
        /// Returns the VisibleLine on the pointer hitting point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        /// <remarks></remarks>
#if UWP
        internal VisibleLineInfo HitTest(Point point, out CoreCursorType cursor)
#else
        internal VisibleLineInfo HitTest(Point point, out Cursor cursor)
#endif
        {
            VisibleLineInfo info = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtPoint(point.X);
            info = (info != null && (point.X == 0.0 || HitTestPrecision > info.ClippedSize / 2)) ? info :
                   treeGrid.TreeGridPanel.ScrollColumns.GetLineNearCorner(point.X, HitTestPrecision);
#if UWP
            if (info != null && info.LineIndex > this.treeGrid.ResolveToStartColumnIndex() - 1)
                cursor = ResizingCursor;
            else
                cursor = CoreCursorType.Arrow;
#else
            cursor = resizingCursor;
#endif
            if (treeGrid.AllowResizingHiddenColumns && info == null)
            {
                var lineInfo = treeGrid.TreeGridPanel.ScrollColumns.GetLineNearCorner(point.X, HitTestHiddenColPrecision, CornerSide.Bottom);
                if (lineInfo == null)
                {
                    var lines = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines();
                    var visibleLine = lines.GetVisibleLineAtPoint(point.X);
                    if (visibleLine != null)
                    {
                        var count = 0;
                        if (this.treeGrid.TreeGridPanel.ColumnWidths.GetHidden(0, out count))
                        {
                            var d = visibleLine.ClippedOrigin - point.X;
                            if (Math.Abs(d) <= HitTestHiddenColPrecision)
                            {
                                isFirstColumnHidden = true;
                                lineInfo = visibleLine;
                            }
                        }
                    }
                }
                if (lineInfo != null)
                {
                    var lineIndex = lineInfo.LineIndex;
                    lineIndex = (isFirstColumnHidden && lineIndex > 0) ? lineIndex - 1 : lineIndex + 1;
                    int rc;

                    if (treeGrid.TreeGridPanel.ColumnWidths.GetHidden(lineIndex, out rc) ||
                        treeGrid.TreeGridPanel.ColumnWidths[lineIndex] == 0.0)

                        cursor = UnhideCursor;
                    info = lineInfo;
                }
            }

            var indentCellCount = this.treeGrid.ResolveToScrollColumnIndex(0);
            if (info != null && (info.LineIndex < indentCellCount && !treeGrid.AllowResizingHiddenColumns))
                return null;
            return info;
        }

        /// <summary>
        /// Computing width on resizing with Minimum & MaximumWidth constrains
        /// </summary>
        /// <param name="column"></param>
        /// <param name="Width"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private double ComputeResizingWidth(TreeGridColumn column, double Width)
        {
            var computedWidth = 0d;
            var colIndex = this.treeGrid.ResolveToScrollColumnIndex(this.treeGrid.Columns.IndexOf(column));

            var expColumnIndex = this.treeGrid.expanderColumnIndex;
            var indentWidth = ((this.treeGrid.View.Nodes.MaxLevel + 1) * this.treeGrid.TreeGridColumnSizer.ExpanderWidth) + 2;
            if (treeGrid.ShowCheckBox)
                indentWidth += this.treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;

            if (!double.IsNaN(column.MinimumWidth) || !double.IsNaN(column.MaximumWidth))
            {
                if (!double.IsNaN(column.MinimumWidth) && !double.IsNaN(column.MaximumWidth))
                {
                    if (colIndex == expColumnIndex)
                    {
                        if (column.MinimumWidth + indentWidth < Width &&
                            column.MaximumWidth + indentWidth > Width)
                            computedWidth = Width;
                        else if (column.MinimumWidth + indentWidth < Width)
                            computedWidth = column.MaximumWidth + indentWidth;
                        else
                            computedWidth = column.MinimumWidth + indentWidth;
                    }
                    else if (column.MinimumWidth < Width && column.MaximumWidth > Width)
                    {
                        computedWidth = Width;
                    }
                    else if (column.MinimumWidth < Width)
                    {
                        computedWidth = column.MaximumWidth;
                    }
                    else
                        computedWidth = column.MinimumWidth;
                }
                else if (!double.IsNaN(column.MinimumWidth) && double.IsNaN(column.MaximumWidth))
                {
                    if (colIndex == expColumnIndex)
                    {
                        if (column.MinimumWidth + indentWidth < Width)
                            computedWidth = Width;
                        else
                            computedWidth = column.MinimumWidth + indentWidth;
                    }
                    else if (column.MinimumWidth < Width)
                        computedWidth = Width;
                    else
                        computedWidth = column.MinimumWidth;
                }
                else if (double.IsNaN(column.MinimumWidth) && !double.IsNaN(column.MaximumWidth))
                {
                    if (colIndex == expColumnIndex)
                    {
                        if (column.MaximumWidth + indentWidth > Width)
                            computedWidth = Width;
                        else
                            computedWidth = column.MaximumWidth + indentWidth;
                    }
                    else if (column.MaximumWidth > Width)
                    {
                        computedWidth = Width;
                    }
                    else
                    {
                        computedWidth = column.MaximumWidth;
                    }
                }
            }
            else
            {
                computedWidth = Width;
            }

            if ((this.treeGrid.ResolveToGridVisibleColumnIndex(this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines().Count)) == 1 && computedWidth <= 5)
                computedWidth = 5;
            return computedWidth;
        }

        /// <summary>
        /// performs mouse move action on TreeGridHeaderCell
        /// </summary>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
#if UWP
        internal void DoActionOnMouseMove(PointerPoint pp, TreeGridHeaderCell headerCell)
#else
        internal void DoActionOnMouseMove(Point point, TreeGridHeaderCell headerCell)
#endif
        {
#if UWP
            double hScrollChange = 0;

            for (int i = 0; i < this.treeGrid.TreeGridPanel.FrozenColumns; i++)
            {
                hScrollChange += this.treeGrid.TreeGridPanel.ColumnWidths[i];
            }

            var pointerPoint = new Point((pp.Position.X - (treeGrid.TreeGridPanel.HScrollBar.Value - hScrollChange)), Math.Abs(pp.Position.Y - treeGrid.TreeGridPanel.VScrollBar.Value));
            if (this.isHovering && pp.Properties.IsLeftButtonPressed && this.dragLine != null)
            {
                var delta = 0.0d;
                int repeatCount;
                bool isHidden;
                isHidden = isFirstColumnHidden ? treeGrid.TreeGridPanel.ColumnWidths.GetHidden(dragLine.LineIndex - 1, out repeatCount)
                                               : treeGrid.TreeGridPanel.ColumnWidths.GetHidden(dragLine.LineIndex + 1, out repeatCount);
                delta = isFirstColumnHidden ? pointerPoint.X
                                            : pointerPoint.X - dragLine.Corner;
                delta = Math.Floor(delta);
#else
            if (this.isHovering && this.dragLine != null && (headerCell.isMouseLeftButtonPressed || headerCell.isTouchPressed))
            {
                var delta = 0.0d;
                int repeatCount;
                bool isHidden;
                isHidden = isFirstColumnHidden ? treeGrid.TreeGridPanel.ColumnWidths.GetHidden(dragLine.LineIndex - 1, out repeatCount)
                                               : treeGrid.TreeGridPanel.ColumnWidths.GetHidden(dragLine.LineIndex + 1, out repeatCount);
                delta = isFirstColumnHidden ? point.X
                                            : point.X - dragLine.Corner;
#endif
                double width = Math.Max(0, dragLine.Size + delta);

                //Should not allow full resizing for the expander column
                var expColumnIndex = this.treeGrid.expanderColumnIndex;
                var expanderWidth = this.treeGrid.View != null ? (this.treeGrid.TreeGridColumnSizer.ExpanderWidth * (this.treeGrid.View.Nodes.MaxLevel + 1) + 2) : 0;
                if (treeGrid.ShowCheckBox)
                    expanderWidth += treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
                if (dragLine.LineIndex == expColumnIndex)
                    width = this.treeGrid.View != null ? Math.Max(width, expanderWidth) : width;

                var args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = dragLine.LineIndex, Width = width };
                if (isHidden && treeGrid.AllowResizingHiddenColumns &&
#if WPF
 headerCell.Cursor == unhideCursor
#else
                    Window.Current.CoreWindow.PointerCursor.Type == UnhideCursor
#endif
)
                {
                    var hiddenLineIndex = dragLine.LineIndex + 1;
                    if (isFirstColumnHidden)
                        hiddenLineIndex = 0;
                    var columnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(hiddenLineIndex);

                    treeGrid.TreeGridColumnSizer.Suspend();
                    treeGrid.Columns[columnIndex].Width = delta;
                    treeGrid.TreeGridColumnSizer.Resume();
                    if (treeGrid.Columns[columnIndex].IsHidden && delta <= 0)
                    {
                        treeGrid.Columns[columnIndex].Width = -delta;
                        treeGrid.TreeGridColumnSizer.Suspend();
                        treeGrid.Columns[columnIndex].IsHidden = false;
                        treeGrid.TreeGridColumnSizer.Resume();
                        if (!treeGrid.Columns[columnIndex].IsHidden)
                        {
                            var index = this.treeGrid.ResolveToScrollColumnIndex(this.treeGrid.Columns.IndexOf(treeGrid.Columns[columnIndex]));
                            this.treeGrid.TreeGridPanel.ColumnWidths.SetHidden(index, index, false);
                        }
                        if (treeGrid.AllowResizingColumns && treeGrid.AllowResizingHiddenColumns)
                            this.treeGrid.ColumnResizingController.ProcessResizeStateManager(treeGrid.Columns[columnIndex]);
                        this.treeGrid.TreeGridPanel.NeedToRefreshColumn = true;
                        this.treeGrid.TreeGridPanel.InvalidateMeasureInfo();
                        if (this.treeGrid.ColumnDragDropController != null)
                            this.treeGrid.ColumnDragDropController.ColumnHiddenChanged(treeGrid.Columns[columnIndex]);
                    }
                    else
                        treeGrid.Columns[columnIndex].IsHidden = false;
#if UWP
                    var cursor = CoreCursorType.Arrow;
                    if (delta >= 0)
                        dragLine = HitTest(new Point(pointerPoint.X, pointerPoint.Y), out cursor);
                    else
                    {
                        dragLine = treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(hiddenLineIndex);
                        cursor = ResizingCursor;
                    }
                    if (cursor != CoreCursorType.Arrow)
                        SetPointerCursor(cursor);
#else
                    var cursor = headerCell.Cursor;
                    if (dragLine != null && dragLine.LineIndex != hiddenLineIndex)
                    {
                        dragLine = treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(hiddenLineIndex);
                        cursor = ResizingCursor;
                    }
                    SetPointerCursor(cursor, headerCell);
#endif
                    if (!double.IsNaN(headerCell.Column.MinimumWidth))
                        delta = Math.Max(delta, headerCell.Column.MinimumWidth);
                    if (!double.IsNaN(headerCell.Column.MaximumWidth))
                        delta = Math.Min(delta, headerCell.Column.MaximumWidth);
                    if ((dragLine != null) && delta <= 0)
                        args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = dragLine.LineIndex, Width = -delta };
                    else if (dragLine != null)
                        args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = dragLine.LineIndex, Width = delta };
                }
                if (dragLine != null && !this.treeGrid.RaiseResizingColumnsEvent(args))
                {
                    var headerRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);
                    var columnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == (dragLine.LineIndex));
                    if (columnBase != null && columnBase.ColumnElement is TreeGridHeaderCell)
                    {
                        var column = (columnBase.ColumnElement as TreeGridHeaderCell).Column;
                        if (dragLine.LineIndex > treeGrid.FrozenColumnCount)
                            treeGrid.TreeGridPanel.ScrollColumns.SetLineResize(dragLine.LineIndex, ComputeResizingWidth(column, args.Width != 0 ? args.Width : 1));
                        // UWP - 2610 - Issue 3 - to avoid flickering need to call below method for expander column.
                        else if (treeGrid.expanderColumnIndex == dragLine.LineIndex)
                            treeGrid.TreeGridPanel.ScrollColumns.SetLineResize(dragLine.LineIndex, ComputeResizingWidth(column, args.Width != 0 ? args.Width : 1));
                        else
                            column.Width = ComputeResizingWidth(column, args.Width != 0 ? args.Width : 1);

                    }
                    isFirstColumnHidden = false;
                }
            }
            else
            {
                this.isHovering = false;
#if UWP
                Point point = pointerPoint;
                var cursor = ResizingCursor;
#else
                Point pp = point;
                var cursor = headerCell.Cursor;
#endif

#if UWP
                var hit = HitTest(pointerPoint, out cursor);
                if (hit != null && cursor != CoreCursorType.Arrow)
#else
                var hit = HitTest(pp, out cursor);
                if (hit != null && (cursor == UnhideCursor || hit.LineIndex >= 0))
#endif
                {
                    var lineIndex = hit.LineIndex;

                    if (cursor == UnhideCursor)
                    {
                        lineIndex++;
                        int count;
                        if (this.treeGrid.TreeGridPanel.ColumnWidths.GetHidden(lineIndex, out count))
                            lineIndex += count - 1;
                        else
                            lineIndex--;
                    }
                    var index = this.treeGrid.ResolveToGridVisibleColumnIndex(lineIndex);
                    if (index >= 0 && index < this.treeGrid.Columns.Count)
                    {
                        TreeGridColumn column = this.treeGrid.Columns[index];
                        var lineindex = this.treeGrid.ResolveToScrollColumnIndex(this.treeGrid.Columns.IndexOf(column));
                        if (!column.AllowResizing)
                            return;
#if WPF
                        this.SetPointerCursor(cursor, headerCell);
#else
                        this.SetPointerCursor(cursor);
#endif
                    }
                }
                bool edgeHit = point.X < 5.0 && point.X > 0;
                if (hit == null && edgeHit)
                {
                    hit = new VisibleLineInfo(0, 0, 0, point.X, 0, true, false);
                }
                if (hit != null || edgeHit)
                {
                    var row = this.treeGrid.TreeGridPanel.ScrollRows.GetVisibleLineAtPoint(point.Y);
                    if (row != null && row.IsHeader)
                        this.isHovering = true;
                }
#if UWP
                if (!isHovering && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                    SetPointerCursor(CoreCursorType.Arrow);
#else
                if (!isHovering && headerCell.Cursor != Cursors.Arrow && (!headerCell.isMouseLeftButtonPressed || !headerCell.isTouchPressed))
                    SetPointerCursor(Cursors.Arrow, headerCell);
#endif
            }
        }

        /// <summary>
        /// Performs MouseUp operation on TreeGridHeaderCell
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Input.MouseButtonEventArgs">MouseButtonEventArgs</see> that contains the event data.</param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
        internal void DoActionOnMouseUp(Point pp, TreeGridHeaderCell headerCell)
        {
#if UWP
            double hScrollChange = 0;

            for (int i = 0; i < this.treeGrid.TreeGridPanel.FrozenColumns; i++)
            {
                hScrollChange += this.treeGrid.TreeGridPanel.ColumnWidths[i];
            }

            var pointerPoint = new Point((pp.X - (treeGrid.TreeGridPanel.HScrollBar.Value - hScrollChange)), Math.Abs(pp.Y - treeGrid.TreeGridPanel.VScrollBar.Value));
            double delta = pointerPoint.X - dragLine.Corner;
#else
            double delta = pp.X - dragLine.Corner;
#endif
            var columnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex);
            double width = Math.Max(0, dragLine.Size + delta);

            if (!(columnIndex >= 0 && columnIndex < this.treeGrid.Columns.Count))
                return;
            width = ComputeResizingWidth(this.treeGrid.Columns[columnIndex], width);

            // Need to subtract the expander width if resizing width is larger than expander width for Expander Column
            var expColumnIndex = this.treeGrid.expanderColumnIndex;
            var expanderWidth = this.treeGrid.View != null ? (this.treeGrid.TreeGridColumnSizer.ExpanderWidth * (this.treeGrid.View.Nodes.MaxLevel + 1) + 2) : 0;
            if (treeGrid.ShowCheckBox)
                expanderWidth += treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
            if (dragLine.LineIndex == expColumnIndex)
            {
                if (treeGrid.View != null)
                {
                    width = Math.Max(width, expanderWidth);
                    width -= expanderWidth;
                }
            }

            this.treeGrid.TreeGridPanel.ScrollColumns.ResetLineResize();
            var args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = dragLine.LineIndex, Width = width };
            if (columnIndex >= 0 && !treeGrid.RaiseResizingColumnsEvent(args))
            {
                this.treeGrid.Columns[columnIndex].IsHidden = (args.Width == 0 || args.Width == 1);
                if ((args.Width == 0) || (args.Width == 1))
                {
                    this.treeGrid.Columns[columnIndex].Width = Math.Round(args.Width);
                }
                else
                {
                    if ((this.treeGrid.ResolveToGridVisibleColumnIndex(this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines().Count) == 1) && this.treeGrid.Columns[columnIndex].Width == args.Width)
                        args.Width += 1;
                    this.treeGrid.Columns[columnIndex].Width = Math.Round(args.Width) == 0 ? 20 : Math.Round(args.Width);
                }
            }
            if ((args.Width == 0 || columnIndex >= 0 && treeGrid.Columns[columnIndex].IsHidden) && treeGrid.AllowResizingHiddenColumns && treeGrid.TreeGridPanel.ColumnCount == this.treeGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex + 1))
            {
                var headerRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);
                var columnBase = headerRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == dragLine.LineIndex + 1);
                var prevHiddenIndex = columnIndex - 1;
                while (prevHiddenIndex > 0 && treeGrid.Columns[prevHiddenIndex].IsHidden)
                    prevHiddenIndex--;
                if (columnBase != null)
                {
                    var prevColumnHeader = (columnBase.ColumnElement as TreeGridHeaderCell);
                    if (prevHiddenIndex != columnIndex)
                    {
                        columnBase = headerRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == prevHiddenIndex);
                        prevColumnHeader = columnBase.ColumnElement as TreeGridHeaderCell;
                    }
                    treeGrid.TreeGridPanel.ColumnWidths.SetHidden(dragLine.LineIndex, dragLine.LineIndex, true);
                    treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex)].IsHidden = true;
                    treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex)].Width = 0.0;
                }
            }
            if (treeGrid.SelectionController.CurrentCellManager.HasCurrentCell && treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == dragLine.LineIndex)
            {
#if !WPF
                treeGrid.Focus(FocusState.Programmatic);
#endif
            }
            dragLine = null;
#if UWP
            headerCell.ReleasePointerCaptures();
            if (Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
            {
                Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
            }
#else
            headerCell.isMouseLeftButtonPressed = false;
            headerCell.ReleaseMouseCapture();
            if (headerCell.Cursor != Cursors.Arrow)
                SetPointerCursor(Cursors.Arrow, headerCell);
            this.isHovering = false;
#endif
        }

        /// <summary>
        /// Sets the Cursor for the Pointer
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
#if UWP
        internal void SetPointerCursor(CoreCursorType cursorType)
#else
        internal void SetPointerCursor(Cursor cursor, TreeGridHeaderCell headerCell)
#endif
        {
#if WPF
            headerCell.Cursor = cursor;
#else
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursorType, 1);
#endif
        }

        #endregion

        #region Popup Resizing

        /// <summary>
        /// Calls on popup resizing by touch
        /// </summary>
        /// <param name="IsinLeft">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool OnPopupContentResizing(bool IsinLeft, double delta)
        {
            var expColumnIndex = this.treeGrid.expanderColumnIndex;
            var expanderWidth = this.treeGrid.View != null ? (this.treeGrid.TreeGridColumnSizer.ExpanderWidth * (this.treeGrid.View.Nodes.MaxLevel + 1) + 2) : 0;
            if (treeGrid.ShowCheckBox)
                expanderWidth += treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
            if (this.treeGrid.FlowDirection == FlowDirection.RightToLeft)
            {
                IsinLeft = !IsinLeft;
                delta = -delta;
            }
            if (this.treeGrid.ColumnDragDropController == null)
                return false;
            if (this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen)
            {
                double width;
                if (IsinLeft && this.treeGrid.ColumnDragDropController.DragLeftLine != null)
                {
                    width = Math.Max(0, this.treeGrid.ColumnDragDropController.PreviousLeftLineSize + delta);

                    //Should not allow full resizing for the expander column
                    if (this.treeGrid.ColumnDragDropController.DragLeftLine.LineIndex == expColumnIndex)
                        width = this.treeGrid.View != null ? Math.Max(width, expanderWidth) : width;

                    if (ResizingColumn(this.treeGrid.ColumnDragDropController.DragLeftLine.LineIndex, width))
                    {
                        this.treeGrid.ColumnDragDropController.PreviousLeftLineSize = width;
                        this.treeGrid.ColumnDragDropController.needToUpdatePosition = true;
                        return false;
                    }
                    return false;
                }
                else if (this.treeGrid.ColumnDragDropController.DragRightLine != null)
                {
                    width = Math.Max(0, this.treeGrid.ColumnDragDropController.PreviousRightLineSize + delta);

                    //UWP-4605 While resizing first column using leftthumb(rightthumb when FlowDirection - RightToLeft)
                    //column width increases as we decrease Popup size as DragLeftLine is null. So IsinLeft is checked 
                    // and corresponding width is calculated.
                    if (IsinLeft)
                        width = Math.Max(0, this.treeGrid.ColumnDragDropController.PreviousRightLineSize - delta);

                    //Should not allow full resizing for the expander column
                    if (this.treeGrid.ColumnDragDropController.DragRightLine.LineIndex == expColumnIndex)
                        width = this.treeGrid.View != null ? Math.Max(width, expanderWidth) : width;

                    if (ResizingColumn(this.treeGrid.ColumnDragDropController.DragRightLine.LineIndex, width))
                    {
                        this.treeGrid.ColumnDragDropController.PreviousRightLineSize = width;
                        treeGrid.ColumnDragDropController.needToUpdatePosition = true;
                        if (this.treeGrid.View != null && width == expanderWidth)
                            return false;
                        else
                            return true;
                    }
                    return false;
                }
                else if (this.treeGrid.ColumnDragDropController.DragRightLine == null && this.HiddenLineIndex >= 0 && this.treeGrid.AllowResizingHiddenColumns)
                {
                    if (this.treeGrid.ColumnDragDropController.PreviousRightLineSize > delta)
                        this.treeGrid.ColumnDragDropController.PreviousRightLineSize = delta;
                    width = Math.Max(0, this.treeGrid.ColumnDragDropController.PreviousRightLineSize + delta);
                    var columnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(this.HiddenLineIndex);
                    treeGrid.TreeGridColumnSizer.Suspend();
                    treeGrid.Columns[columnIndex].Width = width;
                    treeGrid.TreeGridColumnSizer.Resume();
                    treeGrid.Columns[columnIndex].IsHidden = false;
                    var lines = this.treeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines();
                    var lineInfo = lines.GetVisibleLineAtLineIndex(this.HiddenLineIndex);
                    this.treeGrid.ColumnDragDropController.DragRightLine = lineInfo;
                    if (columnIndex == 0 && IsinLeft)
                        return false;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// calls popup resized by touch
        /// </summary>
        /// <param name="IsinLeft">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="delta"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool OnPopupContentResized(bool IsinLeft, double delta)
        {
            var expColumnIndex = this.treeGrid.expanderColumnIndex;
            var expanderWidth = this.treeGrid.View != null ? (this.treeGrid.TreeGridColumnSizer.ExpanderWidth * (this.treeGrid.View.Nodes.MaxLevel + 1) + 2) : 0;
            if (treeGrid.ShowCheckBox)
                expanderWidth += treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
            if (this.treeGrid.FlowDirection == FlowDirection.RightToLeft)
                IsinLeft = !IsinLeft;
            if (this.treeGrid.ColumnDragDropController == null)
                return false;
            if (this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen)
            {
                double width;
                if (IsinLeft && this.treeGrid.ColumnDragDropController.DragLeftLine != null)
                {
                    width = Math.Max(0, treeGrid.ColumnDragDropController.PreviousLeftLineSize);

                    var columnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(treeGrid.ColumnDragDropController.DragLeftLine.LineIndex);
                    width = columnIndex > 0 ? ComputeResizingWidth(this.treeGrid.Columns[columnIndex - 1], width) : ComputeResizingWidth(this.treeGrid.Columns[columnIndex], width);

                    // Need to subtract the expander width if width is larger than expander width for Expander Column
                    if (this.treeGrid.ColumnDragDropController.DragLeftLine.LineIndex == expColumnIndex)
                    {
                        if (treeGrid.View != null)
                        {
                            width = Math.Max(width, expanderWidth);
                            width -= expanderWidth;
                        }
                    }

                    this.treeGrid.TreeGridPanel.ScrollColumns.ResetLineResize();

                    if (!this.IsExpanderColumn(this.treeGrid.Columns[columnIndex]))
                    {
                        if (columnIndex >= 0 && width >= 0 && width <= 1)
                        {
                            width = 0;
                            this.treeGrid.Columns[columnIndex].IsHidden = true;
                            this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
                        }
                    }
                    else
                    {
                        var indentWidth = this.treeGrid.TreeGridColumnSizer.ExpanderWidth;
                        if (treeGrid.ShowCheckBox)
                            indentWidth += this.treeGrid.TreeGridColumnSizer.CheckBoxWidth + 1;
                        if (columnIndex >= 0 && width <= indentWidth + 2)
                        {
                            this.treeGrid.Columns[columnIndex].IsHidden = true;
                            this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
                        }
                        else if (width > indentWidth)
                            this.treeGrid.Columns[columnIndex].IsHidden = false;
                    }

                    var result = SetColumnWidth(treeGrid.ColumnDragDropController.DragLeftLine.LineIndex, width);
                    treeGrid.ColumnDragDropController.needToUpdatePosition = true;
                    currentResizingColumn = this.treeGrid.ColumnDragDropController.DragLeftLine;
                    return result;
                }
                else if (this.treeGrid.ColumnDragDropController.DragRightLine != null)
                {
                    width = Math.Max(0, treeGrid.ColumnDragDropController.PreviousRightLineSize);

                    //The below code will helps to set the Hidden state for the resized column when width=0
                    var columnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(treeGrid.ColumnDragDropController.DragRightLine.LineIndex);
                    width = ComputeResizingWidth(this.treeGrid.Columns[columnIndex], width);

                    // Need to subtract the expander width if width is larger than expander width for Expander Column
                    if (this.treeGrid.ColumnDragDropController.DragRightLine.LineIndex == expColumnIndex)
                    {
                        if (this.treeGrid.View != null)
                        {
                            width = Math.Max(width, expanderWidth);
                            width -= expanderWidth;
                        }
                    }

                    this.treeGrid.TreeGridPanel.ScrollColumns.ResetLineResize();

                    if (!this.IsExpanderColumn(this.treeGrid.Columns[columnIndex]))
                    {
                        if (columnIndex >= 0 && width >= 0 && width <= 1)
                        {
                            width = 0;
                            this.treeGrid.Columns[columnIndex].IsHidden = true;
                            this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
                        }
                    }
                    else
                    {
                        if (columnIndex >= 0 && width <= expanderWidth)
                        {
                            this.treeGrid.Columns[columnIndex].IsHidden = true;
                            this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
                        }
                        else if (width > expanderWidth)
                            this.treeGrid.Columns[columnIndex].IsHidden = false;
                    }

                    var result = SetColumnWidth(treeGrid.ColumnDragDropController.DragRightLine.LineIndex, width);
                    treeGrid.ColumnDragDropController.needToUpdatePosition = true;
                    currentResizingColumn = this.treeGrid.ColumnDragDropController.DragRightLine;
                    return result;
                }
                else
                {
                    this.treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(currentResizingColumn.LineIndex)].IsHidden = true;
                    this.treeGrid.ColumnDragDropController.DraggablePopup.IsOpen = false;
                    this.treeGrid.ColumnDragDropController.PreviousRightLineSize = delta;
                }
            }
            return false;
        }

        /// <summary>
        /// perform Resizing column for given index and sets the width 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool ResizingColumn(int index, double width)
        {
            if (width <= 0)
                return false;
            int columnindex = this.treeGrid.ResolveToGridVisibleColumnIndex(index);
            var gridcolumn = this.treeGrid.Columns[columnindex];

            var args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = index, Width = width };
            if (this.treeGrid.RaiseResizingColumnsEvent(args))
                return false;

            width = ComputeResizingWidth(gridcolumn, args.Width);
            this.treeGrid.TreeGridPanel.ScrollColumns.SetLineResize(index, ComputeResizingWidth(gridcolumn, width));
            if (width != args.Width)
                return false;
            return true;
        }

        /// <summary>
        /// sets the width for column for given column index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="width"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool SetColumnWidth(int index, double width)
        {
            int columnindex = this.treeGrid.ResolveToGridVisibleColumnIndex(index);
            var gridcolumn = this.treeGrid.Columns[columnindex];

            var args = new ResizingColumnsEventArgs(this.treeGrid) { ColumnIndex = index, Width = Math.Round(width, 2) };
            if (this.treeGrid.RaiseResizingColumnsEvent(args))
                return false;

            width = args.Width;

            this.treeGrid.TreeGridPanel.ScrollColumns.ResetLineResize();
            gridcolumn.Width = width;
            return true;
        }

        #endregion

        #region Hidden Resizing VSM

        /// <summary>
        /// Applies the Hidden State VSM if the Column in hidden.
        /// </summary>
        /// <param name="column">The column.</param>
        private void UpdateVisualState(bool isPreviousColumnHidden, bool isNextColumnHidden, TreeDataColumnBase columnBase)
        {
            var columnElement = (TreeGridHeaderCell)columnBase.ColumnElement;
            if (isPreviousColumnHidden && isNextColumnHidden)
                VisualStateManager.GoToState(columnElement, "HiddenState", true);
            else if (isPreviousColumnHidden)
                VisualStateManager.GoToState(columnElement, "PreviousColumnHidden", true);
            else if (isNextColumnHidden)
                VisualStateManager.GoToState(columnElement, "LastColumnHidden", true);
            else
                VisualStateManager.GoToState(columnElement, "NormalState", true);
        }

        internal void ProcessResizeStateManager(TreeGridColumn column)
        {
            var headerRow = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);
            if (headerRow == null)
                return;

            var columnIndex = this.treeGrid.Columns.IndexOf(column);
            var previousColumnIndex = columnIndex - 1;
            var nextColumnIndex = columnIndex + 1;

            var lastColumn = this.treeGrid.Columns.LastOrDefault(col => (!col.IsHidden || (col.IsHidden && this.IsExpanderColumn(col))));
            var lastColumnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.TreeGridColumn == lastColumn);

            if (previousColumnIndex >= 0)
            {
                var index = this.treeGrid.ResolveToScrollColumnIndex(previousColumnIndex);
                var columnBase = headerRow.VisibleColumns.OrderBy(x => x.ColumnIndex).LastOrDefault(col => col.ColumnIndex <= index && (col.ColumnElement is TreeGridHeaderCell) && (!(col.ColumnElement as TreeGridHeaderCell).Column.IsHidden || col.ColumnIndex == this.treeGrid.expanderColumnIndex));

                if (columnBase != null)
                {
                    var resolvedColumnIndex = this.treeGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex);
                    if (resolvedColumnIndex >= 0)
                    {
                        var isPreviousColumnHidden = false;
                        if (resolvedColumnIndex != 0)
                            isPreviousColumnHidden = this.treeGrid.Columns[resolvedColumnIndex - 1].IsHidden &&
                                                      !this.IsExpanderColumn(this.treeGrid.Columns[resolvedColumnIndex - 1]);
                        var isNextColumnHidden = (column.IsHidden && !this.IsExpanderColumn(column)) && lastColumnBase != null && columnBase == lastColumnBase;
                        this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                    }
                    else
                        VisualStateManager.GoToState((TreeGridHeaderCell)columnBase.ColumnElement, "NormalState", true);
                }
            }

            if (columnIndex >= 0 && columnIndex < this.treeGrid.Columns.Count)
            {
                // WPF-34271 - Expander column may be hidden.But it will be displayed in view. So below expander column check is added.
                if (!column.IsHidden || this.treeGrid.ResolveToScrollColumnIndex(columnIndex) == treeGrid.expanderColumnIndex)
                {
                    var index = this.treeGrid.ResolveToScrollColumnIndex(columnIndex);
                    var columnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == index && col.ColumnElement is TreeGridHeaderCell);
                    if (columnBase == null)
                        return;
                    var isPreviousColumnHidden = columnIndex > 0 && this.treeGrid.Columns[columnIndex - 1].IsHidden &&
                                                 !this.IsExpanderColumn(this.treeGrid.Columns[columnIndex - 1]);
                    var lastColumnIndex = this.treeGrid.Columns.IndexOf(this.treeGrid.Columns.LastOrDefault(col => !col.IsHidden));
                    var isNextColumnHidden = ((columnIndex == lastColumnIndex) && (lastColumnIndex < this.treeGrid.Columns.Count - 1) && (this.treeGrid.Columns[lastColumnIndex + 1].IsHidden && !this.IsExpanderColumn(this.treeGrid.Columns[lastColumnIndex + 1])));
                    this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                }
            }

            if (nextColumnIndex < this.treeGrid.Columns.Count)
            {
                var index = this.treeGrid.ResolveToScrollColumnIndex(nextColumnIndex);
                var columnBase = headerRow.VisibleColumns.OrderBy(x => x.ColumnIndex).FirstOrDefault(col => col.ColumnIndex >= index && (col.ColumnElement is TreeGridHeaderCell) && (!(col.ColumnElement as TreeGridHeaderCell).Column.IsHidden || col.ColumnIndex == this.treeGrid.expanderColumnIndex));
                if (columnBase == null)
                    return;
                if (column.IsHidden && !this.IsExpanderColumn(column))
                    if (columnBase == lastColumnBase && lastColumn != this.treeGrid.Columns.LastOrDefault())
                        VisualStateManager.GoToState((TreeGridHeaderCell)columnBase.ColumnElement, "HiddenState", true);
                    else
                        VisualStateManager.GoToState((TreeGridHeaderCell)columnBase.ColumnElement, "PreviousColumnHidden", true);
                else
                {
                    var isPreviousColumnHidden = columnIndex >= 0 && this.treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) - 1].IsHidden &&
                                                  !this.IsExpanderColumn(this.treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) - 1]);
                    var isNextColumnHidden = lastColumnBase != null && (columnBase == lastColumnBase) && (this.treeGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) + 1 < this.treeGrid.Columns.Count) && (this.treeGrid.Columns[this.treeGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) + 1].IsHidden);
                    this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                }
            }
        }

        /// <summary>
        /// Checks whether the column is Expander column or not
        /// </summary>
        /// <param name="column"></param>
        /// <returns>
        /// Returns true if the column is Expander column otherwise returns false
        /// </returns>
        internal bool IsExpanderColumn(TreeGridColumn column)
        {
            bool isHidden = false;
            if ((column == this.treeGrid.TreeGridColumnSizer.GetExpanderColumn() && this.treeGrid.View != null))
                isHidden = true;
            return isHidden;
        }

        #endregion

        #endregion

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            dragLine = null;
            this.currentResizingColumn = null;
            treeGrid = null;
        }
    }
}
