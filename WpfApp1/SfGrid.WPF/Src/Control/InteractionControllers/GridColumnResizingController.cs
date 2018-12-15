#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WPF
using Windows.Foundation;
using Windows.UI.Core;
using Syncfusion.UI.Xaml.Utils;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if !WPF
    using MouseEventArgs = PointerRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using Windows.UI.Input;
    using Windows.UI.Core;
    using System.Diagnostics;
#endif
    /// <summary>
    /// Provides the base implementation for column resizing operations in SfDataGrid.
    /// </summary>
    public class GridColumnResizingController : IDisposable
    {
        #region Resizing Fields
        SfDataGrid dataGrid;
        private const double HitTestPrecision = 4.0;
        private const double HitTestHiddenColPrecision = 6.0;
        private bool isFirstColumnHidden;

        #endregion

        #region Internal property
        internal bool isHovering { get; set; }
        internal VisibleLineInfo dragLine { get; set; }
        internal int HiddenLineIndex { get; set; }
        #endregion
        private bool isdisposed = false;

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class.
        /// </summary>
        public GridColumnResizingController()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        public GridColumnResizingController(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
        }

#if !WPF
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
        /// Gets or sets the cursor that is displayed when the column is resized using mouse pointer in SfDataGrid.
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
        /// Gets or sets the cursor that is displayed when the column is hidden using mouse pointer in SfDataGrid.
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

        internal bool CanResizeColumn(GridColumn column)
        {
            if (column == null)
                return false;

            bool canResizeColumn = false;
            var resizeColumn = column.ReadLocalValue(GridColumn.AllowResizingProperty);
            if (resizeColumn != DependencyProperty.UnsetValue)
                canResizeColumn = column.AllowResizing;
            if ((resizeColumn == DependencyProperty.UnsetValue) && this.dataGrid.AllowResizingColumns)
                canResizeColumn = true;
            return canResizeColumn;
        }

        /// <summary>
        /// Ensures the VSM for hidden columns OnGridColumnCollectionChanged
        /// </summary>
        /// <param name="OldStartingIndex"></param>
        /// <param name="NewStartingIndex"></param>
        /// <remarks></remarks>
        internal void EnsureVSMOnColumnCollectionChanged(int OldStartingIndex, int NewStartingIndex)
        {
            if (OldStartingIndex != -1 && OldStartingIndex >= 0 && OldStartingIndex < this.dataGrid.Columns.Count)
                this.ProcessResizeStateManager(this.dataGrid.Columns[OldStartingIndex]);
            if (NewStartingIndex != -1 && NewStartingIndex >= 0 && NewStartingIndex < this.dataGrid.Columns.Count)
                this.ProcessResizeStateManager(this.dataGrid.Columns[NewStartingIndex]);
        }

        #region Resizing by mouse

        /// <summary>
        /// Returns the VisibleLine on the pointer hitting point
        /// </summary>
        /// <param name="point"></param>
        /// <param name="cursor"></param>
        /// <returns></returns>
        /// <remarks></remarks>
#if !WPF
        internal VisibleLineInfo HitTest(Point point,out CoreCursorType cursor)
#else
        internal VisibleLineInfo HitTest(Point point, out Cursor cursor)
#endif
        {
           // VisibleLineInfo info = point.X == 0.0 ? this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtPoint(point.X)
                                                 // : dataGrid.VisualContainer.ScrollColumns.GetLineNearCorner(point.X, HitTestPrecision);
            //Changed the code for WPF-16038 
            //To get the visibleline at point when hide all the columns and try to resizing
          
            VisibleLineInfo info = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtPoint(point.X);

            info = (info != null && (point.X == 0.0 || HitTestPrecision > info.ClippedSize / 2)) ? info : 
                dataGrid.VisualContainer.ScrollColumns.GetLineNearCorner(point.X, HitTestPrecision);

#if !WPF
            if (info != null && info.LineIndex > this.dataGrid.ResolveToStartColumnIndex() - 1)
                cursor = ResizingCursor;            
            else            
                cursor = CoreCursorType.Arrow;            
#else
            cursor = ResizingCursor;
#endif
            if (dataGrid.AllowResizingHiddenColumns && info == null)
            {
                var lineInfo = dataGrid.VisualContainer.ScrollColumns.GetLineNearCorner(point.X, HitTestHiddenColPrecision, CornerSide.Bottom);
                if (lineInfo == null)
                {
                    var lines = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
                    var visibleLine = lines.GetVisibleLineAtPoint(point.X);
                    if (visibleLine != null)
                    {
                        var count = 0;
                        if (this.dataGrid.VisualContainer.ColumnWidths.GetHidden(0, out count))
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

                    if (dataGrid.VisualContainer.ColumnWidths.GetHidden(lineIndex, out rc) ||
                        dataGrid.VisualContainer.ColumnWidths[lineIndex] == 0.0 || lineInfo.Size == 0)

                        cursor = UnhideCursor; 
                        info = lineInfo;
                }
            }

            var indentCellCount = this.dataGrid.ResolveToScrollColumnIndex(0);
            if (info != null && (info.LineIndex < indentCellCount && !dataGrid.AllowResizingHiddenColumns))
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
        private double ComputeResizingWidth(GridColumn column, double Width)
        {
            var computedWidth = 0d;
            var colIndex = this.dataGrid.ResolveToScrollColumnIndex(this.dataGrid.Columns.IndexOf(column));
            if (!double.IsNaN(column.MinimumWidth) || !double.IsNaN(column.MaximumWidth))
            {
                if (!double.IsNaN(column.MinimumWidth) && !double.IsNaN(column.MaximumWidth))
                {
                    if (column.MinimumWidth < Width && column.MaximumWidth > Width)
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
                    if (column.MinimumWidth < Width)
                        computedWidth = Width;
                    else
                        computedWidth = column.MinimumWidth;
                }
                else if(double.IsNaN(column.MinimumWidth) && !double.IsNaN(column.MaximumWidth))
                {
                    if (column.MaximumWidth > Width)
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
            if ((this.dataGrid.ResolveToGridVisibleColumnIndex(this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines().Count) == 1) && computedWidth <= 5)
                computedWidth = 5;
            return computedWidth;
        }

        /// <summary>
        /// performs mouse move action on GridHeaderCellControl
        /// </summary>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.Input.PointerRoutedEventArgs">PointerRoutedEventArgs</see> that contains the event data.</param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
#if !WPF
        internal void DoActionOnMouseMove(PointerPoint pp, GridHeaderCellControl headerCell)
#else
        internal void DoActionOnMouseMove(Point pp, GridHeaderCellControl headerCell)
#endif
        {
            bool isLastColumnHidden = false;
#if !WPF
            double hScrollChange = 0;

            for (int i = 0; i < this.dataGrid.VisualContainer.FrozenColumns; i++)
            {
                hScrollChange += this.dataGrid.VisualContainer.ColumnWidths[i];
            }
			// since panel x moves to negative the resizing not properly working, so Abs removed for x calculation WRT-1311, skipping for negative shrinking for first column.
            var pointerPoint = new Point((pp.Position.X - (dataGrid.VisualContainer.HScrollBar.Value - hScrollChange)), Math.Abs(pp.Position.Y - dataGrid.VisualContainer.VScrollBar.Value));
            if (this.isHovering && pp.Properties.IsLeftButtonPressed && this.dragLine != null)
            {
                var delta = 0.0d;
                int repeatCount;
                bool isHidden;
                isHidden = isFirstColumnHidden ? dataGrid.VisualContainer.ColumnWidths.GetHidden(dragLine.LineIndex - 1, out repeatCount)
                                               : dataGrid.VisualContainer.ColumnWidths.GetHidden(dragLine.LineIndex + 1, out repeatCount);
                delta = isFirstColumnHidden ? pointerPoint.X
                                            : pointerPoint.X - dragLine.Corner;
                delta = Math.Floor(delta);
#else
            if (this.isHovering && this.dragLine != null && (headerCell.isMouseLeftButtonPressed || headerCell.isTouchPressed))
            {
                //After hiding the column by resizing the column in AddNewRow with edit mode, when pressing enter key rendering issue has been shown in
                //above AddNewRow because the CurrentRowIndex is not updated due the Current column is in Hidden state. Hence the current cell is 
                //edited and New row value has been committed.
                CurrentCellEndEdit();
                //Above code raises CurrentCellValidating if customer shows MessageBox then release capture lost and dragline will be null. 
                if (dragLine == null)
                {
                    return;
                }
                var delta = 0.0d;
                int repeatCount;
                bool isHidden;
                isHidden = isFirstColumnHidden ? dataGrid.VisualContainer.ColumnWidths.GetHidden(dragLine.LineIndex - 1, out repeatCount)
                                               : dataGrid.VisualContainer.ColumnWidths.GetHidden(dragLine.LineIndex + 1, out repeatCount);
                delta = isFirstColumnHidden ? pp.X
                                            : pp.X - dragLine.Corner;
#endif
                double width = Math.Max(0, dragLine.Size + delta);
                var args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = dragLine.LineIndex, Width = width };
                if (isHidden && dataGrid.AllowResizingHiddenColumns &&
#if !WPF
                    Window.Current.CoreWindow.PointerCursor.Type == UnhideCursor
#else
                    headerCell.Cursor == UnhideCursor
#endif
                    )
                {
                    if (delta <= 0) //WPF-16041 added this condition to sync cursor and action when unhidden last column
                        isLastColumnHidden = true;
                    if (delta <= 2 && !isLastColumnHidden)
                        return;
                    var hiddenLineIndex = dragLine.LineIndex + 1;
                    if (isFirstColumnHidden)
                        hiddenLineIndex = 0;
                    var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(hiddenLineIndex);

                    dataGrid.GridColumnSizer.Suspend();
                    dataGrid.Columns[columnIndex].Width = delta;
                    dataGrid.GridColumnSizer.Resume();
                    if (dataGrid.Columns[columnIndex].IsHidden && delta <= 0)
                    {
                        dataGrid.GridColumnSizer.Suspend();
                        dataGrid.Columns[columnIndex].IsHidden = false;
                        dataGrid.GridColumnSizer.Resume();
                        if (!dataGrid.Columns[columnIndex].IsHidden)
                        {
                            var index = this.dataGrid.ResolveToScrollColumnIndex(this.dataGrid.Columns.IndexOf(dataGrid.Columns[columnIndex]));
                            this.dataGrid.VisualContainer.ColumnWidths.SetHidden(index, index, false);
                            //Need to call ScrollInView to bring the hidden column into view while hiding/unhiding 
                            this.dataGrid.VisualContainer.ScrollColumns.ScrollInView(index, -delta);
                        }
                        if (dataGrid.AllowResizingColumns && dataGrid.AllowResizingHiddenColumns)
                            this.dataGrid.ColumnResizingController.ProcessResizeStateManager(dataGrid.Columns[columnIndex]);
                        this.dataGrid.VisualContainer.NeedToRefreshColumn = true;
                        this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                        if (this.dataGrid.GridColumnDragDropController != null)
                            this.dataGrid.GridColumnDragDropController.ColumnHiddenChanged(dataGrid.Columns[columnIndex]);
                    }
                    else
                        dataGrid.Columns[columnIndex].IsHidden = false;
                    this.dataGrid.RowGenerator.UpdateCellStyle(columnIndex);
#if !WPF
                    var cursor = CoreCursorType.Arrow;
                    if(delta >= 0)
                        dragLine = HitTest(new Point(pointerPoint.X, pointerPoint.Y),out cursor);
                    else // WPF-16041 Since delta value is negative,need to calculate the dragline through GetVisibleAtLineIndex() for the lastcolumn
                    {
                        dragLine = dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(hiddenLineIndex);
                        cursor = ResizingCursor;
                    }
                    if (cursor != CoreCursorType.Arrow)
                        SetPointerCursor(cursor);
#else
                    var cursor = headerCell.Cursor;
                    if (dragLine != null && dragLine.LineIndex != hiddenLineIndex)
                    {
                        //dragline will be null when unhiding last column with ColumnSizer - AutoLastColumnFill as it is not inside ViewPort
                        dragLine = dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(hiddenLineIndex);
                        cursor = ResizingCursor;
                    }
                    SetPointerCursor(cursor, headerCell);
#endif
                    if (!double.IsNaN(headerCell.Column.MinimumWidth))
                        delta = Math.Max(delta, headerCell.Column.MinimumWidth);
                    if (!double.IsNaN(headerCell.Column.MaximumWidth))
                        delta = Math.Min(delta, headerCell.Column.MaximumWidth);
                    //WPF-16041 Since delta is negative for last column, change the delta sign and set to Width
                    //delta value switched to positive only when last column is hidden. Other cases, it won't hit
                    if ((dragLine != null) && delta <= 0)   
                        args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = dragLine.LineIndex, Width = -delta };
                    else if (dragLine != null)
                        args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = dragLine.LineIndex, Width = delta };
                }
                if (dragLine != null && !this.dataGrid.RaiseResizingColumnsEvent(args))
                {
                    var headerRow = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == dataGrid.GetHeaderIndex());
                    var columnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == (dragLine.LineIndex));
                    if (columnBase != null && columnBase.ColumnElement is GridHeaderCellControl)
                    {
                        var column = (columnBase.ColumnElement as GridHeaderCellControl).Column;
                        if (dragLine.LineIndex > dataGrid.FrozenColumnCount)
                            dataGrid.VisualContainer.ScrollColumns.SetLineResize(dragLine.LineIndex, ComputeResizingWidth(column, args.Width != 0 ? args.Width : 1));
                        else
                            column.Width = ComputeResizingWidth(column, args.Width != 0 ? args.Width : 1);
                    }
                    isFirstColumnHidden = false;
                }
            }
            else
            {
                this.isHovering = false;
#if !WPF
                Point point = pointerPoint;
                var cursor = ResizingCursor;
#else
                Point point = pp;
                var cursor = headerCell.Cursor;
#endif

#if !WPF
                var hit = HitTest(pointerPoint,out cursor);
                if(hit != null && cursor != CoreCursorType.Arrow)  
#else
                var hit = HitTest(pp,out cursor);               

                //When LineIndex returns the first column index, this condition fails hence the greater than or equal to condition is added.
                if (hit != null && (cursor == UnhideCursor || hit.LineIndex >= dataGrid.GetFirstColumnIndex()))                                                                                          
#endif        
                {
                    var lineIndex = hit.LineIndex;
                   
                    if(cursor == UnhideCursor)
                    {
                        lineIndex++;
                        int count;
                        if (this.dataGrid.VisualContainer.ColumnWidths.GetHidden(lineIndex, out count))
                            lineIndex += count - 1;
                        else
                            lineIndex--;
                    }
                    var index = this.dataGrid.ResolveToGridVisibleColumnIndex(lineIndex);
                    if (index >= 0 && index < this.dataGrid.Columns.Count)
                    {
                        GridColumn column = this.dataGrid.Columns[index];
                        var lineindex = this.dataGrid.ResolveToScrollColumnIndex(this.dataGrid.Columns.IndexOf(column));                       
                        if (!this.CanResizeColumn(column))
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
                    var row = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLineAtPoint(point.Y);
                    if (row != null && row.IsHeader)
                        this.isHovering = true;
                }
#if !WPF
                if (!isHovering && Window.Current.CoreWindow.PointerCursor.Type != CoreCursorType.Arrow)
                    SetPointerCursor(CoreCursorType.Arrow);
#else
                if (!isHovering && headerCell.Cursor != Cursors.Arrow && (!headerCell.isMouseLeftButtonPressed || !headerCell.isTouchPressed))
                    SetPointerCursor(Cursors.Arrow,headerCell);
#endif
            }
        }

        /// <summary>
        /// Perfoms MouseUp operation on GridHeaderCellControl
        /// </summary>
        /// <param name="e">An <see cref="T:System.Windows.Input.MouseButtonEventArgs">MouseButtonEventArgs</see> that contains the event data.</param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
        internal void DoActionOnMouseUp(MouseEventArgs e, GridHeaderCellControl headerCell)
        {
#if !WPF
            PointerPoint pp = e.GetCurrentPoint(this.dataGrid.VisualContainer);
            double hScrollChange = 0;

            for (int i = 0; i < this.dataGrid.VisualContainer.FrozenColumns; i++)
            {
                hScrollChange += this.dataGrid.VisualContainer.ColumnWidths[i];
            }
            var pointerPoint = new Point(pp.Position.X - (dataGrid.VisualContainer.HScrollBar.Value - hScrollChange), pp.Position.Y - dataGrid.VisualContainer.VScrollBar.Value);
            double delta = pointerPoint.X - dragLine.Corner;
#else
            Point pp = e.GetPosition(this.dataGrid.VisualContainer);
            double delta = pp.X - dragLine.Corner;
#endif
            var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex);
            double width = Math.Max(0, dragLine.Size + delta);
            if (!(columnIndex >= 0 && columnIndex < this.dataGrid.Columns.Count))
                return;
            width = ComputeResizingWidth(this.dataGrid.Columns[columnIndex], width);
            this.dataGrid.VisualContainer.ScrollColumns.ResetLineResize();
            var args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = dragLine.LineIndex, Width = width };
            if (columnIndex >= 0 && !dataGrid.RaiseResizingColumnsEvent(args))
            {
                this.dataGrid.Columns[columnIndex].IsHidden = (args.Width == 0 || args.Width == 1);
                if ((args.Width == 0) || (args.Width == 1))
                {
                    this.dataGrid.Columns[columnIndex].Width = Math.Round(args.Width);
                    //Reset the extendedwidth when column is in Hidden because of Resizing.
                    this.dataGrid.Columns[columnIndex].ExtendedWidth = double.NaN;
                }
                else
                {
                    if (args.Width != this.dataGrid.Columns[columnIndex].ActualWidth && this.dataGrid.Columns[columnIndex].ExtendedWidth > 0)
                    {
                        var actualWidth = this.dataGrid.Columns[columnIndex].ActualWidth;
                        var extendedWidth = this.dataGrid.Columns[columnIndex].ExtendedWidth;
                        var differenceInExtendedWidth = ((args.Width - actualWidth) < extendedWidth) ? (args.Width - actualWidth) : 0;
                        this.dataGrid.Columns[columnIndex].ExtendedWidth = (differenceInExtendedWidth > 0) ? (extendedWidth - differenceInExtendedWidth) : double.NaN;
                    }
                    if ((this.dataGrid.ResolveToGridVisibleColumnIndex(this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines().Count) == 1) && this.dataGrid.Columns[columnIndex].Width == args.Width) 
                        args.Width += 1;
                    this.dataGrid.Columns[columnIndex].Width = Math.Round(args.Width) == 0 ? 20 : Math.Round(args.Width);
                }
            }
            if ((args.Width == 0 || columnIndex >= 0 && dataGrid.Columns[columnIndex].IsHidden) && dataGrid.AllowResizingHiddenColumns && dataGrid.VisualContainer.ColumnCount == this.dataGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex + 1))
            {
                var headerRow = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == dataGrid.GetHeaderIndex());
                var columnBase = headerRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == dragLine.LineIndex + 1);
                var prevHiddenIndex = columnIndex - 1;
                while (prevHiddenIndex > 0 && dataGrid.Columns[prevHiddenIndex].IsHidden)
                    prevHiddenIndex--;
                if (columnBase != null)
                {
                    var prevColumnHeader = (columnBase.ColumnElement as GridHeaderCellControl);
                    if (prevHiddenIndex != columnIndex)
                    {
                        columnBase = headerRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == prevHiddenIndex);
                        prevColumnHeader = columnBase.ColumnElement as GridHeaderCellControl;
                    }
                    dataGrid.VisualContainer.ColumnWidths.SetHidden(dragLine.LineIndex, dragLine.LineIndex, true);
                    dataGrid.Columns[dataGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex)].IsHidden = true;
                    dataGrid.Columns[dataGrid.ResolveToGridVisibleColumnIndex(dragLine.LineIndex)].Width = 0.0;
                }
            }
            if (dataGrid.SelectionController.CurrentCellManager.HasCurrentCell && dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == dragLine.LineIndex)
            {
#if !WPF
                dataGrid.Focus(FocusState.Programmatic);
#endif
            }
            dragLine = null;
#if !WPF
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
            //WRT-4573 - While clicking on HeaderCell border on Resizing, the sorting operation is done in WinRT. Hence
            //the flag is skipped to set false which will be set on mouse move.
			this.isHovering = false;       
#endif               
        }

        /// <summary>
        /// Sets the Cursor for the Pointer
        /// </summary>
        /// <param name="cursor"></param>
        /// <param name="headerCell"></param>
        /// <remarks></remarks>
#if !WPF
        internal void SetPointerCursor(CoreCursorType cursorType)
#else
        internal void SetPointerCursor(Cursor cursor, GridHeaderCellControl headerCell)
#endif
        {
#if !WPF
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(cursorType, 1);
#else
            headerCell.Cursor = cursor;
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
            if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
            {
                IsinLeft = !IsinLeft;
                delta = -delta;
            }
            if (this.dataGrid.GridColumnDragDropController == null)
                return false;
            if (this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen)
            {
                //After hiding the column by resizing the column in AddNewRow with edit mode, when pressing enter key rendering issue has been shown in
                //above AddNewRow because the CurrentRowIndex is not updated due the Current column is in Hidden state. Hence the current cell is 
                //edited and New row value has been committed.
                CurrentCellEndEdit();
                double width;
                if (IsinLeft && this.dataGrid.GridColumnDragDropController.DragLeftLine != null)
                {
                    width = Math.Max(0, this.dataGrid.GridColumnDragDropController.PreviousLeftLineSize + delta);
                    if (ResizingColumn(this.dataGrid.GridColumnDragDropController.DragLeftLine.LineIndex, width))
                    {
                        this.dataGrid.GridColumnDragDropController.PreviousLeftLineSize = width;
                        this.dataGrid.GridColumnDragDropController.needToUpdatePosition = true;
                        return false;
                    }
                    return false;
                }
                else if (this.dataGrid.GridColumnDragDropController.DragRightLine != null)
                {
                    width = Math.Max(0, this.dataGrid.GridColumnDragDropController.PreviousRightLineSize + delta);

                    //UWP-4605 While resizing first column using leftthumb(rightthumb when FlowDirection - RightToLeft)
                    //column width increases as we decrease Popup size as DragLeftLine is null. So IsinLeft is checked 
                    // and corresponding width is calculated.
                    if (IsinLeft)
                        width = Math.Max(0, this.dataGrid.GridColumnDragDropController.PreviousRightLineSize - delta);  

                    if (ResizingColumn(this.dataGrid.GridColumnDragDropController.DragRightLine.LineIndex, width))
                    {
                        this.dataGrid.GridColumnDragDropController.PreviousRightLineSize = width;
                        dataGrid.GridColumnDragDropController.needToUpdatePosition = true;
                        return true;
                    }
                    return false;
                }
                //The Below Code will helps to find the DragRightLine. When we hide the Column the DragRightLine will be set as Null 
                //so we can find the Index through the HiddenLineIndex.
                else if(this.dataGrid.GridColumnDragDropController.DragRightLine==null && this.HiddenLineIndex >= 0 && this.dataGrid.AllowResizingHiddenColumns)
                {
                    //When Hiding two or more columns the SecondResize column getting the value as first resized column value while unhiding.
                    //so here we have reset the previousReightLineSize value.
                    if (this.dataGrid.GridColumnDragDropController.PreviousRightLineSize > delta)
                        this.dataGrid.GridColumnDragDropController.PreviousRightLineSize = delta;
                    width = Math.Max(0, this.dataGrid.GridColumnDragDropController.PreviousRightLineSize + delta);
                    var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(this.HiddenLineIndex);
                    dataGrid.GridColumnSizer.Suspend();
                    dataGrid.Columns[columnIndex].Width = width;
                    dataGrid.GridColumnSizer.Resume();
                    dataGrid.Columns[columnIndex].IsHidden = false;
                    //We can set the delta value to the ColumnIndex. after that we can get the DragRightLine.
                    var lines = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
                    var lineInf = lines.GetVisibleLineAtLineIndex(this.HiddenLineIndex);
                    this.dataGrid.GridColumnDragDropController.DragRightLine = lineInf;
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
            if (this.dataGrid.FlowDirection == FlowDirection.RightToLeft)
                IsinLeft = !IsinLeft;
            if (this.dataGrid.GridColumnDragDropController == null)
                return false;
            if (this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen)
            {
                double width;
                if (IsinLeft && this.dataGrid.GridColumnDragDropController.DragLeftLine != null)
                {
                    width = Math.Max(0, dataGrid.GridColumnDragDropController.PreviousLeftLineSize);
                    //This code will helps to set the Hiddenstate for the resized column when width=0
                    var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(dataGrid.GridColumnDragDropController.DragLeftLine.LineIndex);
                    //When the First Column is hidden the ColumnIdex will be getting Zero. so when we resize with the Left Thumb Value we can pass the ColumnIndex.
                    width = columnIndex > 0 ? ComputeResizingWidth(this.dataGrid.Columns[columnIndex - 1], width) : ComputeResizingWidth(this.dataGrid.Columns[columnIndex], width);
                    this.dataGrid.VisualContainer.ScrollColumns.ResetLineResize();
                    if (columnIndex >= 0 && width >= 0 && width <= 1)
                    {
                        width = 0;
                        this.dataGrid.Columns[columnIndex].IsHidden = true;
                        this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen = false;
                    }
                    var result = SetColumnWidth(dataGrid.GridColumnDragDropController.DragLeftLine.LineIndex, width);
                    dataGrid.GridColumnDragDropController.needToUpdatePosition = true;
                    return result;
                }
                else if (this.dataGrid.GridColumnDragDropController.DragRightLine != null)
                {
                    width = Math.Max(0, dataGrid.GridColumnDragDropController.PreviousRightLineSize);
                    //The below code will helps to set the Hiddenstate for the resized column when width=0
                    var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(dataGrid.GridColumnDragDropController.DragRightLine.LineIndex);
                    width = ComputeResizingWidth(this.dataGrid.Columns[columnIndex], width);
                    this.dataGrid.VisualContainer.ScrollColumns.ResetLineResize();
                    if (columnIndex >= 0 && width >= 0 && width <= 1)
                    {
                        width = 0;
                        this.dataGrid.Columns[columnIndex].IsHidden = true;
                        this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen = false;
                    }
                    var result = SetColumnWidth(dataGrid.GridColumnDragDropController.DragRightLine.LineIndex, width);
                    dataGrid.GridColumnDragDropController.needToUpdatePosition = true;
                    return result;
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
            int columnindex = this.dataGrid.ResolveToGridVisibleColumnIndex(index);
            var gridcolumn = this.dataGrid.Columns[columnindex];

            var args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = index, Width = width };
            if (this.dataGrid.RaiseResizingColumnsEvent(args))
                return false;

            width = ComputeResizingWidth(gridcolumn, args.Width);
            this.dataGrid.VisualContainer.ScrollColumns.SetLineResize(index, ComputeResizingWidth(gridcolumn, width));
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
            int columnindex = this.dataGrid.ResolveToGridVisibleColumnIndex(index);
            var gridcolumn = this.dataGrid.Columns[columnindex];

            var args = new ResizingColumnsEventArgs(this.dataGrid) { ColumnIndex = index, Width = Math.Round(width, 2) };
            if (this.dataGrid.RaiseResizingColumnsEvent(args))
                return false;

            width = args.Width;

            this.dataGrid.VisualContainer.ScrollColumns.ResetLineResize();
            gridcolumn.Width = width;
            return true;
        }

        /// <summary>
        /// End edits the CurrentCell when resizing any column.
        /// </summary>
        private void CurrentCellEndEdit()
        {
            if (this.dataGrid.SelectionController.CurrentCellManager.HasCurrentCell && !this.dataGrid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                return;

            this.dataGrid.SelectionController.CurrentCellManager.EndEdit();
            if (this.dataGrid.SelectionController.CurrentCellManager.IsAddNewRow)
                this.dataGrid.GridModel.addNewRowController.CommitAddNew(false);
        }

        #endregion

        #region Hidden Resizinig VSM

        /// <summary>
        /// Applies the Hidden State VSM if the Column in hidden.
        /// </summary>
        /// <param name="column">The column.</param>

        private void UpdateVisualState(bool isPreviousColumnHidden, bool isNextColumnHidden, DataColumnBase columnBase)
        {
            var columnElement = (GridHeaderCellControl)columnBase.ColumnElement;
            if (isPreviousColumnHidden && isNextColumnHidden)
                VisualStateManager.GoToState(columnElement, "HiddenState", true);
            else if (isPreviousColumnHidden)
                VisualStateManager.GoToState(columnElement, "PreviousColumnHidden", true);
            else if (isNextColumnHidden)
                VisualStateManager.GoToState(columnElement, "LastColumnHidden", true);
            else 
                VisualStateManager.GoToState(columnElement, "NormalState", true);
        }

        internal void ProcessResizeStateManager(GridColumn column)
        {
            var headerRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.dataGrid.GetHeaderIndex());
            if (headerRow == null)
                return;

            var columnIndex = this.dataGrid.Columns.IndexOf(column);
            var previousColumnIndex = columnIndex - 1;
            var nextColumnIndex = columnIndex + 1;

            var lastColumn = this.dataGrid.Columns.LastOrDefault(col => !col.IsHidden);
            var lastColumnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.GridColumn == lastColumn);

            if (previousColumnIndex >= 0)
            {
                var index = this.dataGrid.ResolveToScrollColumnIndex(previousColumnIndex);
                var columnBase = headerRow.VisibleColumns.OrderBy(x => x.ColumnIndex).LastOrDefault(col => col.ColumnIndex <= index && (col.ColumnElement is GridHeaderCellControl) && !(col.ColumnElement as GridHeaderCellControl).Column.IsHidden);

                if (columnBase != null)
                {
                    var resolvedColumnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex);
                    if (resolvedColumnIndex >= 0)
                    {
                        var isPreviousColumnHidden = false;
                        if (resolvedColumnIndex != 0)
                            isPreviousColumnHidden = this.dataGrid.Columns[resolvedColumnIndex - 1].IsHidden;
                        var isNextColumnHidden = column.IsHidden && lastColumnBase != null && columnBase == lastColumnBase;
                        this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                    }
                    else
                        VisualStateManager.GoToState((GridHeaderCellControl)columnBase.ColumnElement, "NormalState", true);
                }
            }

            if (columnIndex >= 0 && columnIndex < this.dataGrid.Columns.Count)
            {
                if (!column.IsHidden)
                {
                    var index = this.dataGrid.ResolveToScrollColumnIndex(columnIndex);
                     //WPF-23367 - we have to apply the visual states only to the GridHeaderCellControl. 
                    var columnBase = headerRow.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == index && col.ColumnElement is GridHeaderCellControl);
                    if (columnBase == null)
                        return;
                    var isPreviousColumnHidden = columnIndex > 0 && this.dataGrid.Columns[columnIndex - 1].IsHidden;
                    var lastColumnIndex = this.dataGrid.Columns.IndexOf(this.dataGrid.Columns.LastOrDefault(col => !col.IsHidden));
                    var isNextColumnHidden = ((columnIndex == lastColumnIndex) && (lastColumnIndex < this.dataGrid.Columns.Count - 1) && this.dataGrid.Columns[lastColumnIndex + 1].IsHidden);
                    this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                }
            }

            if (nextColumnIndex < this.dataGrid.Columns.Count)
            {
                var index = this.dataGrid.ResolveToScrollColumnIndex(nextColumnIndex);
                var columnBase = headerRow.VisibleColumns.OrderBy(x => x.ColumnIndex).FirstOrDefault(col => col.ColumnIndex >= index && (col.ColumnElement is GridHeaderCellControl) && !(col.ColumnElement as GridHeaderCellControl).Column.IsHidden);
                if (columnBase == null)
                    return;
                if (column != null && column.IsHidden)
                    if (columnBase == lastColumnBase && lastColumn != this.dataGrid.Columns.LastOrDefault())
                        VisualStateManager.GoToState((GridHeaderCellControl)columnBase.ColumnElement, "HiddenState", true);
                    else
                        VisualStateManager.GoToState((GridHeaderCellControl)columnBase.ColumnElement, "PreviousColumnHidden", true);
                else
                {
                    var isPreviousColumnHidden = columnIndex >= 0 && this.dataGrid.Columns[this.dataGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) - 1].IsHidden;
                    var isNextColumnHidden = lastColumnBase != null && (columnBase == lastColumnBase) && (this.dataGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex + 1) < this.dataGrid.Columns.Count) && (this.dataGrid.Columns[this.dataGrid.ResolveToGridVisibleColumnIndex(columnBase.ColumnIndex) + 1].IsHidden);
                    this.UpdateVisualState(isPreviousColumnHidden, isNextColumnHidden, columnBase);
                }
            }
        }
#endregion

#endregion
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                dragLine = null;
                dataGrid = null;
            }
            isdisposed = true;
        }
    }
}
