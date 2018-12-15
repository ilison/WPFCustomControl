#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Syncfusion.Data.Extensions;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Reflection;
using System.Diagnostics;
using Syncfusion.UI.Xaml.Grid.Cells;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class  OrientedCellsPanel : Panel, IDisposable
    {
        #region Fields

        internal Func<DataRowBase> GetDataRow;
        private bool isdisposed = false;
        #endregion

        #region Ctor

        public OrientedCellsPanel()
        {

        }

        #endregion

        #region override Methods

        #region OnRenderOverride

#if WPF
        protected override void OnRender(DrawingContext dc)
        {            
            if (this.GetDataRow == null)
            {
                base.OnRender(dc); 
                return;
            }
            var dataRow = this.GetDataRow();
            if (dataRow.RowIndex < 0 || ! (dataRow as GridDataRow).DataGrid.useDrawing)
                return;

            var visibleColumns = dataRow.GetVisibleColumns();
            
            var propertyInfo = typeof(Control).GetProperty("PreviousArrangeRect", BindingFlags.Instance | BindingFlags.NonPublic);
            var hasClip = dataRow.WholeRowElement.RowBorderState.Contains("FixedCaption");

            foreach (var column in visibleColumns)
            {                
                if (column.ColumnElement.Visibility != Visibility.Visible) continue;

                var rectSize = (Rect)propertyInfo.GetValue(column.ColumnElement, null);
                if(column.Renderer != null)
                    column.Renderer.RenderCell(dc, rectSize, column, dataRow.RowData);                                                      
                else
                     //Render Fixed caption rows's indent border.                   
                    column.OnRender(dc, rectSize, column.ColumnElement as GridCell,hasClip);               
            }            
            base.OnRender(dc);                        
        }
#endif

        #endregion

        #region MeasureOverride

        protected override Size MeasureOverride(Size availableSize)
        {
            if (this.GetDataRow == null)
                return base.MeasureOverride(availableSize);
            EnsureItems(availableSize);
            return availableSize;
        }

        #endregion

        #region ArrangeOverride

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this.GetDataRow == null)
                return base.ArrangeOverride(finalSize);
            ArrangeColumns(finalSize);
            return finalSize;
        }

        #endregion

        #endregion

        #region private methods

        private void EnsureItems(Size availableSize)
        {
            double rowHeight = availableSize.Height;
            var dataRow = this.GetDataRow();

            if (dataRow.RowIndex < 0)
                return;

            var visibleColumns = dataRow.VisibleColumns;

            //if (VisibleColumns.Count != this.Children.Count)
            foreach (var column in visibleColumns)
            {
                if (column.ColumnIndex < 0 || column.ColumnElement.Visibility != Visibility.Visible)
                    continue;

                if (column.isnewElement)
                {
                    if (!this.Children.Contains(column.ColumnElement))
                    {
                        this.Children.Add(column.ColumnElement);
                        //WPF-35398 CellStyle applied for UnboundRow in SfDataGrid while using UnBoundRowCellStyle Property
                        if(dataRow.RowData != null || dataRow.RowType == RowType.AddNewRow || dataRow.RowType == RowType.FilterRow || dataRow.RowType == RowType.HeaderRow || dataRow.RowType == RowType.UnBoundRow)
                            column.UpdateCellStyle(dataRow.RowData);
                    }
                    column.isnewElement = false;
                }

                if (column.IsSpannedColumn && !CanAddChild(column))
                {
                    column.ColumnElement.Visibility = Visibility.Collapsed;
                    column.ColumnVisibility = Visibility.Collapsed;
                    continue;
                }
                else
                {
                    Size size;
                    if (column.IsSpannedColumn)
                        size = new Size(dataRow.GetColumnSize(column.ColumnIndex, false), dataRow.GetRowSize(column, false));
                    else
                        size = new Size(dataRow.GetColumnSize(column.ColumnIndex, false), rowHeight);

                    if (column.Renderer != null) //Right now SpannedRows won't have the CellRenderers
                        column.Renderer.Measure(new RowColumnIndex(column.RowIndex, column.ColumnIndex), column.ColumnElement, size);
                    else
                        column.ColumnElement.Measure(size);
                }
            }
        }

        /// <summary>
        /// Method that decides the column element can be added to panel or not.
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        bool CanAddChild(IColumnElement column)
        {
            if (!(column is DataColumnBase))
                return false;

            var DataColumnBase = (column as DataColumnBase);

            if (DataColumnBase.Renderer != null && DataColumnBase.Renderer.DataGrid != null)
            {
                var coveredCellInfo = DataColumnBase.Renderer.DataGrid.CoveredCells.GetCoveredCellInfo(DataColumnBase);

                if (coveredCellInfo == null)
                    return false;

                DataColumnBase dataColumn = null;
                // get the bottom row if its in view.               
                var dataRow = DataColumnBase.Renderer.DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Top && item.IsEnsured && item.RowVisibility == Visibility.Visible);

                if (dataRow != null)
                {
                    var dataRowLineInfo = DataColumnBase.GridColumn.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(dataRow.RowIndex);

                    if (dataRowLineInfo != null && dataRowLineInfo.IsClippedOrigin)
                    {
                        // return false for the column element that's row has the clipped corner
                        if (dataRow.RowIndex == DataColumnBase.RowIndex && !dataRowLineInfo.IsClipped)
                            return false;
                        dataRow = DataColumnBase.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top || item.RowIndex == coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, DataColumnBase.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);
                    }
                }

                if (dataRow != null && (dataRow.RowType != RowType.DefaultRow && dataRow.RowType != RowType.UnBoundRow))
                    throw new Exception(String.Format("Given range {0} is not a data row {1}", coveredCellInfo, dataRow));

                // get the previous bottom row when the bottom is out of view. while scroll towards down.
                var nextTopRow = DataColumnBase.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top) && coveredCellInfo.Contains(item.RowIndex, DataColumnBase.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);

                if (nextTopRow != null)
                {
                    var previousBottomRowLineInfo = DataColumnBase.GridColumn.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextTopRow.RowIndex);
                    if (previousBottomRowLineInfo != null && previousBottomRowLineInfo.IsClippedOrigin)
                    {
                        // return false for the column element that's row has the clipped corner.
                        if (nextTopRow.RowIndex == DataColumnBase.RowIndex && !previousBottomRowLineInfo.IsClipped)
                            return false;
                        nextTopRow = DataColumnBase.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > nextTopRow.RowIndex || item.RowIndex == coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, DataColumnBase.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);
                    }
                }

                // get the index of that row to compare with covered cell bottom index.               
                var bottomRowIndex = dataRow == null ? (nextTopRow != null ? nextTopRow.RowIndex : DataColumnBase.RowIndex) : dataRow.RowIndex;

                // get column from bottom row that was in view.
                if (dataRow != null)
                {
                    // get the left column .  
                    dataColumn = dataRow.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                             spannedColumn.ColumnVisibility == Visibility.Visible &&
                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                             spannedColumn.GridColumn != null && !spannedColumn.GridColumn.IsHidden);

                    if (dataColumn != null)
                    {
                        var visibleLineInfo = dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex);

                        if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                        {
                            // get the next left column while the left column has not in view or it is clipped.                             
                            dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                             (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                             coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                             item.IsEnsured &&
                                                                             item.ColumnVisibility == Visibility.Visible &&
                                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                             item.GridColumn != null && !item.GridColumn.IsHidden);
                        }
                    }
                    else
                    {
                        dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                             (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                             coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                             item.IsEnsured &&
                                                                             item.ColumnVisibility == Visibility.Visible &&
                                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                             item.GridColumn != null && !item.GridColumn.IsHidden);

                        var visibleLineInfo = dataColumn != null ? dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex) : null;

                        if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                        {
                            // get the next left column while the left column has not in view or it is clipped.                             
                            dataColumn = dataRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                             (item.ColumnIndex > dataColumn.ColumnIndex || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                             coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                             item.IsEnsured &&
                                                                             item.ColumnVisibility == Visibility.Visible &&
                                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                             item.GridColumn != null && !item.GridColumn.IsHidden);
                        }
                    }
                }
                // get column from previous bottom row when bottom row is not in view.
                else if (nextTopRow != null)
                {
                    // get the left column .  
                    dataColumn = nextTopRow.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                             spannedColumn.IsEnsured &&
                                                             spannedColumn.ColumnVisibility == Visibility.Visible &&
                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                             spannedColumn.GridColumn != null && !spannedColumn.GridColumn.IsHidden);

                    if (dataColumn != null)
                    {
                        var visibleLineInfo = dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex);

                        if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                        {
                            // get the next left column while the left column has not in view or it is clipped.                             
                            dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                             (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                             coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                             item.IsEnsured &&
                                                                             item.ColumnVisibility == Visibility.Visible &&
                                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                             item.GridColumn != null && !item.GridColumn.IsHidden);
                        }
                    }
                    else
                    {
                        dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                            (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                            coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                            item.IsEnsured &&
                                                                            item.ColumnVisibility == Visibility.Visible &&
                                                                            DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                            !item.GridColumn.IsHidden);

                        var visibleLineInfo = dataColumn != null ? dataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(dataColumn.ColumnIndex) : null;

                        if (visibleLineInfo != null && visibleLineInfo.IsClippedOrigin)
                        {
                            // get the next left column while the left column has not in view or it is clipped.                             
                            dataColumn = nextTopRow.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                                             (item.ColumnIndex > dataColumn.ColumnIndex || item.ColumnIndex == coveredCellInfo.Right) &&
                                                                             coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                             item.IsEnsured &&
                                                                             item.ColumnVisibility == Visibility.Visible &&
                                                                             DataColumnBase.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                             !item.GridColumn.IsHidden);
                        }

                    }
                }

                // throws exception when the left column is not a grid cell column
                if (dataColumn != null && !DataColumnBase.Renderer.DataGrid.MergedCellManager.CanQueryColumn(dataColumn))            
                    throw new Exception(String.Format("Given range {0} is not a data column {1}", coveredCellInfo, dataColumn));

                // get the index of the column to compare with covered cell left index.
                var leftColumnIndex = dataColumn == null ? DataColumnBase.ColumnIndex : dataColumn.ColumnIndex;

                return leftColumnIndex == DataColumnBase.ColumnIndex && bottomRowIndex == DataColumnBase.RowIndex;
            }

            return false;
        }        

        private void ArrangeColumns(Size finalSize)
        {
            var rowHeight = finalSize.Height;
            var dataRow = this.GetDataRow();
            if (dataRow.RowIndex < 0)
                return;
            var visibleColumns = dataRow.VisibleColumns;
#if UWP
            //UWP-5800 - Header cell border gets flickered while resizing - If background color applied to the cells
            // then next cell background color overlapped on the current cell right border while resizing
            //To avoid this behavior, we have set ZIndex to the header cells in reverse order
            int panelIndex = -1;
            if (dataRow.RowType == RowType.HeaderRow || dataRow.RowType == RowType.StackedHeaderRow)
                panelIndex = dataRow.VisibleColumns.Count;
#endif
            foreach (var column in visibleColumns)
            {
                if (column.ColumnElement == null || column.ColumnElement.Visibility != Visibility.Visible) continue;
#if UWP
                //UWP-5800 - Header cell border gets flickered while resizing - If background color applied to the cells
                // then next cell background color overlapped on the current cell right border while resizing
                //To avoid this behavior, we have set ZIndex to the header cells in reverse order
                if (panelIndex > -1)
                {
                    column.ColumnElement.SetValue(Canvas.ZIndexProperty, panelIndex);
                    panelIndex--;
                }
#endif
                double ClippedWidth = 0;
                var lineInfo = new List<VisibleLineInfo>();
                var line = dataRow.GetColumnVisibleLineInfo(column.ColumnIndex);

                var lineSize = dataRow.GetColumnSize(column.ColumnIndex, false);
                double clippedSize = 0;
                double width = 0;

                //UWP-4733 when FooterColumnCount is set, the StackedHeader overlaps footercolumn. Since clip is not calculated correctly.
                //The below condition is applicable for all SummaryRows and StackedHeaderRow.
                if (dataRow is SpannedDataRow && column.Renderer != null)
                {
                    var firstVisibleStackedColIndex = column.ColumnIndex;
                    var hasClippedCorner = false;
                    var hasClippedBody = false;

                    clippedSize = GetColumnsNotInViewSize(column, dataRow as DataRowBase, out firstVisibleStackedColIndex);
                    width = GetClippedWidth(column, dataRow as SpannedDataRow, out hasClippedCorner, out hasClippedBody);

                    var visibleline = dataRow.GetColumnVisibleLineInfo(firstVisibleStackedColIndex);
                    if (visibleline != null)
                    {
                        double cliprowheight = (rowHeight + (column.RowSpan * rowHeight));

                        // WPF-35923 -> 0.3 is added for clipped rows, because the right and bottom border are appearing light instead of dark
                        if (visibleline.IsClippedOrigin && visibleline.IsClippedCorner)
                        {
                            var cliporgin = clippedSize + visibleline.Size - (visibleline.ClippedSize + visibleline.ClippedCornerExtent);
                            var cliprect = new Rect(cliporgin, 0, width + 0.3, cliprowheight);
                            column.ColumnElement.Clip = new RectangleGeometry() { Rect = cliprect };
                        }
                        else if (visibleline.IsClippedOrigin)
                        {
                            var cliporgin = clippedSize + visibleline.Size - visibleline.ClippedSize;
                            var cliprect = new Rect(cliporgin, 0, width + 0.3, cliprowheight + 0.3);
                            column.ColumnElement.Clip = new RectangleGeometry() { Rect = cliprect };
                        }
                        //(WPF - 35923) hasClippedBody is considered because the childgrid's right and bottom border is not appearing properly 
                        else if (hasClippedCorner && hasClippedBody)
                        {
                            var cliprect = new Rect(clippedSize, 0, width + 0.3, cliprowheight);
                            column.ColumnElement.Clip = new RectangleGeometry() { Rect = cliprect };
                        }
                        else
                        {
                            // WPF-35923 clip is not properly applied for caption summary row 
                            //while grouping the columns.
                            if (clippedSize != 0)
                            {
                                var cliprect = new Rect(clippedSize, 0, width + 0.3, cliprowheight);
                                column.ColumnElement.Clip = new RectangleGeometry() { Rect = cliprect };
                            }
                            else if (column.ColumnElement.Clip != null)
                                column.ColumnElement.Clip = null;
                        }

                        var top = column.RowSpan != 0 ? -GetSpannedRowHeight(column) : 0;
                        var rect = new Rect(visibleline.Origin - clippedSize, top, lineSize, cliprowheight);
                        column.Renderer.Arrange(new RowColumnIndex(column.RowIndex, column.ColumnIndex), column.ColumnElement, rect);
                    }
                    else
                    {
                        //GroupCaptionSummaryRow will fall on this category when scrolling. (ShowSummaryInRow - True)
                        //Where only one SpannedColumn will be created.
                        ArrangeSpannedColumnNotInView(column, dataRow, lineSize, rowHeight);
                    }
                }
                // if the column is not a covered column
                else if (line != null)
                {
                    if (!column.IsSpannedColumn)
                    {
                        if (line.IsClippedBody && line.IsClippedOrigin && line.IsClippedCorner)
                        {
                            if (clippedSize == 0)
                                column.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(line.Size - (line.ClippedSize + line.ClippedCornerExtent), 0, line.ClippedSize, (rowHeight + (column.RowSpan * rowHeight) + 0.3)) };
                            else
                                column.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(lineSize - (clippedSize + line.ClippedCornerExtent), 0, line.ClippedSize, (rowHeight + (column.RowSpan * rowHeight) + 0.3)) };
                        }
                        else if (line.IsClippedBody && line.IsClippedOrigin)
                        {
                            if (clippedSize == 0)
                                column.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(line.Size - line.ClippedSize, 0, lineSize, (rowHeight + (column.RowSpan * rowHeight) + 0.3)) };
                            else
                                column.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(lineSize - clippedSize, 0, lineSize, (rowHeight + (column.RowSpan * rowHeight) + 0.3)) };
                        }
                        else if (line.IsClippedBody && line.IsClippedCorner)
                            column.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(0, 0, line.ClippedSize, (rowHeight + (column.RowSpan * rowHeight) + 0.3)) };
                        else
                        {
                            if (column.ColumnElement.Clip != null)
                                column.ColumnElement.Clip = null;
                        }
                    }
                    Rect rect;
                    if (column.RowSpan != 0)
                    {
                        var spannedRowHeight = GetSpannedRowHeight(column);
                        rect = new Rect(line.Origin, -spannedRowHeight, line.Size, spannedRowHeight + rowHeight);
                    }
                    else
                        rect = new Rect(line.Origin, 0, line.Size, rowHeight);

                    rect.Width = dataRow.GetColumnSize(column.ColumnIndex, false) - ClippedWidth;

                    //IsSpannedColumn will be true only for coveredcells.
                    if (column.IsSpannedColumn)
                    {
                        rect = GetRect(column, rect);
                    }                  

                    if (column.Renderer != null)
                    {
                        column.Renderer.Arrange(new RowColumnIndex(column.RowIndex, column.ColumnIndex), column.ColumnElement, rect);
                    }
                    else
                        column.ColumnElement.Arrange(rect);
                }
                else
                {
                    //WPF-35735 - Columns are not properly arranged when the visiblelineinfo is null. 
                    //For arranging columns in out of view.
                    var rect = new Rect(finalSize.Width + 100, 0, 100, 1000);
                    if (column.Renderer != null)
                        column.Renderer.Arrange(new RowColumnIndex(column.RowIndex, column.ColumnIndex), column.ColumnElement, rect);
                    else
                        column.ColumnElement.Arrange(rect);
                }
            }
        }

        /// <summary>
        /// To get clip for the column that is not in view.
        /// </summary>
        /// <param name="datacolumn"></param>
        /// <param name="datarow"></param>
        /// <param name="linesize"></param>
        /// <param name="rowheight"></param>
        private void ArrangeSpannedColumnNotInView(DataColumnBase datacolumn, DataRowBase datarow, double linesize, double rowheight)
        {
            //WPF-35735 While grouping the last column in view overlaps the previous column
            int index = 0;

            var origin = -GetColumnsNotInViewSize(datacolumn, datarow, out index);
            var top = datacolumn.RowSpan != 0 ? -GetSpannedRowHeight(datacolumn) : 0;

            var rect = new Rect(origin, top, linesize, rowheight + (datacolumn.RowSpan * rowheight));

            if (datacolumn.Renderer != null)
                datacolumn.Renderer.Arrange(new RowColumnIndex(datacolumn.RowIndex, datacolumn.ColumnIndex), datacolumn.ColumnElement, rect);
            else
                datacolumn.ColumnElement.Arrange(rect);
        }

        /// <summary>
        /// Method to get the size of the columns which are not in view for orgin calculation.
        /// </summary>
        /// <param name="datacolumn"></param>
        /// <param name="datarow"></param>
        /// <param name="firstvisibleindex"></param>
        /// <returns></returns>
        private double GetColumnsNotInViewSize(DataColumnBase datacolumn, DataRowBase datarow, out int firstvisibleindex)
        {
            var left = datacolumn.ColumnIndex;
            var right = datacolumn.ColumnIndex + datacolumn.ColumnSpan;

            double clippedsize = 0;
            firstvisibleindex = left;

            for (int index = left; index <= right; index++)
            {
                var newline = datarow.GetColumnVisibleLineInfo(index);
                if (newline == null)
                    clippedsize += datarow.GetColumnSize(index, true);
                else
                {
                    firstvisibleindex = index;
                    break;
                }
            }
            return clippedsize;
        }

        /// <summary>
        /// Method to get the width of clipped spanned cell in SpannedDataRow.
        /// </summary>
        /// <param name="datacolumn"></param>
        /// <param name="dataRow"></param>
        /// <param name="isclippedcorner"></param>
        /// <returns></returns>
        private double GetClippedWidth(DataColumnBase datacolumn, SpannedDataRow dataRow, out bool isclippedcorner, out bool isclippedbody)
        {
            var left = datacolumn.ColumnIndex;
            var right = datacolumn.ColumnIndex + datacolumn.ColumnSpan;
            double clippedwidth = 0;
            isclippedcorner = false;
            isclippedbody = false;
            for (int index = left; index <= right; index++)
            {
                var newline = dataRow.GetColumnVisibleLineInfo(index);

                if (newline != null)
                {
                    clippedwidth += newline.IsClipped ? newline.ClippedSize : newline.Size;
                    isclippedcorner = newline.IsClippedCorner;
                    isclippedbody = newline.IsClippedBody;
                }
            }
            return clippedwidth;
        }

        /// <summary>
        ///  Method used to returns the cumulative height of the SpannedRow
        /// </summary>
        /// <param name="column"></param>
        private double GetSpannedRowHeight(IColumnElement column)
        {
            double resultHeight = 0;
            for (int i = column.RowIndex - 1; i > column.RowIndex - column.RowSpan - 1; i--)
            {
                resultHeight += column.Renderer.DataGrid.VisualContainer.RowHeights[i];
            }
            return Math.Round(resultHeight);
        }
    
        #endregion

        Rect GetRect(DataColumnBase dc, Rect rect)
        {                        
            // Whole width of merged range (left to right).
            double totalMergedCellWidth = 0.0;
            // WHole height of merged rows (top to bottom)
            double totalMergedCellHeight = 0.0;

            // The height of the merged cell thats in view -while scroll vertically.
            double mergedCellViewHeight = 0.0;
            // The height of the merged cell thats in out of view while scroll vertically (partially or fully).
            double mergedCellScrolledHeight = 0.0;

            // The width of the merged cell thats in view - while scroll horizontally.
            double mergedCellViewWidth = 0.0;
            // The width of the merged cell thats in out of view while scroll horizontally.
            double mergedCellScrolledWidth = 0.0;

            // Associated covered cell info with the spanned column that going to wrapped for the range within it.
            var coveredCellInfo = dc.Renderer.DataGrid.CoveredCells.GetCoveredCellInfo((dc));
                            
            // take top row to know that it was in view.
            var topRow = dc.Renderer.DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Top && item.IsEnsured && item.RowVisibility == Visibility.Visible);
            // taking next top row to get the view height when the top (covered cell top) has scrolled.
            var nextTopRow = dc.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top) && coveredCellInfo.Contains(item.RowIndex, dc.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);
            // taking bottom row to know that it was in view.
            var bottomRow = dc.Renderer.DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Bottom && item.IsEnsured && item.RowVisibility == Visibility.Visible);
            // taking next bottom row to get the view height when the bottom row (covered cell bottom) has scrolled.
            var nextBottomRow = dc.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).LastOrDefault(item => (item.RowIndex < coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, dc.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);

            // Gets the visible info of associated rows to know the rows were clipped at top or bottom.

            var topVisibleInfo = topRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(topRow.RowIndex) : null;
            var nextTopVisibleInfo = nextTopRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextTopRow.RowIndex) : null;
            var bottomVisibleInfo = bottomRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(bottomRow.RowIndex) : null;
            var nextBottomVisibleInfo = nextBottomRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextBottomRow.RowIndex) : null;
                        
            // Know that the grid has frozen columns 
            var hasFrozenColumn = dc.Renderer.DataGrid.VisualContainer.FrozenColumns != 0;
            
            // Know that the grid has footer columns.
            var hasfooterColumns = dc.Renderer.DataGrid.VisualContainer.FooterColumns != 0;
            
            bool isClippedCorner = false;            
            double verticalScrolledValue = 0.0;
            double horizontalScrolledValue = 0.0;

            if (topRow != null)
            {               
                // calculate height of merged cell thats in view, total covered range size (top to bottom), scrolled size - vertically.

                totalMergedCellHeight = topRow.GetRowSize(dc, false);
                mergedCellScrolledHeight = topRow.GetMergedCellRowSize(coveredCellInfo.Top, dc.RowIndex - 1);
                mergedCellViewHeight = GetviewRowSize(dc);


                // Calculate width of the merged cell thats in view, total covered size - (left to right) scrolled size - horizontally
                totalMergedCellWidth = topRow.GetColumnSize(dc.ColumnIndex, false);
                mergedCellScrolledWidth = GetScrolledColumnsWidth(dc, topRow);
                mergedCellViewWidth = GetViewColumnSize(out isClippedCorner, out horizontalScrolledValue, dc, topRow);

                
                //Apply clip for the column element when the top row has clipped by origin.(based on mergedCellViewheight with totalMergedCellHeight )                                       
                if (topVisibleInfo != null)
                {                
                    verticalScrolledValue = topVisibleInfo.ScrollOffset;
                    dc.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(0, verticalScrolledValue, rect.Width, mergedCellViewHeight) };
                }
              
                rect.Y -= mergedCellScrolledHeight;
                rect.Height = totalMergedCellHeight;

                rect.X = -mergedCellScrolledWidth;
                rect.Width = totalMergedCellWidth;
                

                // APply clip based on frozen column.
                if (hasFrozenColumn)
                    dc.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(isClippedCorner ? horizontalScrolledValue : totalMergedCellWidth - mergedCellViewWidth, verticalScrolledValue, mergedCellViewWidth, mergedCellViewHeight) };                
            }
            else
            {
                if (nextTopRow != null)
                {
                    // calculate height of merged cell thats in view, total covered range size (top to bottom), scrolled size - vertically.
                    totalMergedCellHeight = nextTopRow.GetRowSize(dc, false); 
                    mergedCellScrolledHeight = nextTopRow.GetMergedCellRowSize(coveredCellInfo.Top, dc.RowIndex - 1);
                    mergedCellViewHeight = GetviewRowSize(dc);

                    // Calculate width of the merged cell thats in view, total covered size - (left to right) scroleld size - horizontally
                    totalMergedCellWidth = nextTopRow.GetColumnSize(dc.ColumnIndex, false);
                    mergedCellScrolledWidth = GetScrolledColumnsWidth(dc, nextTopRow);
                    mergedCellViewWidth = GetViewColumnSize(out isClippedCorner,out horizontalScrolledValue, dc, nextTopRow);

                    // Apply clip for the column element when the top row has clipped by origin.(based on mergedCellViewheight with totalMergedCellHeight ) 
                    if (nextTopVisibleInfo != null)
                    {                                               
                        verticalScrolledValue = nextTopRow.GetMergedCellRowSize(coveredCellInfo.Top, nextTopVisibleInfo.LineIndex - 1) + nextTopVisibleInfo.ScrollOffset;                                                
                        dc.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(0, totalMergedCellHeight - mergedCellViewHeight, rect.Width, mergedCellViewHeight) };
                    }


                    rect.Y -= mergedCellScrolledHeight;
                    rect.Height = totalMergedCellHeight;

                    rect.X = -mergedCellScrolledWidth;
                    rect.Width = totalMergedCellWidth;                   

                    //Apply clip based on the frozen column.
                    if (hasFrozenColumn)
                        dc.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(isClippedCorner ? horizontalScrolledValue : totalMergedCellWidth - mergedCellViewWidth, totalMergedCellHeight - mergedCellViewHeight, mergedCellViewWidth, mergedCellViewHeight) };                                                        
                }
                else                                    
                    rect.Height = GetDataRow().GetRowSize(dc, false);                                                                        
            }

            if (dc.Renderer.DataGrid.VisualContainer.FooterRows != 0)
            {
                if((bottomRow != null && bottomVisibleInfo != null && bottomVisibleInfo.IsClippedCorner) 
                    ||  
                    (nextBottomRow != null && nextBottomVisibleInfo != null && nextBottomVisibleInfo.IsClippedCorner))
                    dc.ColumnElement.Clip = new RectangleGeometry() { Rect = new Rect(hasFrozenColumn ? isClippedCorner ? 0 : totalMergedCellWidth - mergedCellViewWidth : 0, verticalScrolledValue, hasFrozenColumn ? mergedCellViewWidth : rect.Width, mergedCellViewHeight) };                                                                                                                      
            }
            return rect;
        }

        /// <summary>
        /// Gets the origin that needs to be arranged out of view.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        internal double GetScrolledColumnsWidth(DataColumnBase dc, DataRowBase dr)
        {
            // covered cell region of column taht present in it.
            var coveredCellInfo = dc.Renderer.DataGrid.CoveredCells.GetCoveredCellInfo(dc);

            var hasFrozenColumn = dc.Renderer.DataGrid.VisualContainer.FrozenColumns != 0;

            var frozenColumnsWidth = hasFrozenColumn ? dc.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(0, dc.Renderer.DataGrid.VisualContainer.FrozenColumns - 1) : 0.0;
            // whole merged  cell width.
            var mergedCellWidth = dr.GetColumnSize(dc.ColumnIndex, false);
            double origin = 0.0;
            
            var hasFooterColumn = dc.Renderer.DataGrid.VisualContainer.FooterColumns != 0;

            if (dr != null)
            {
                var leftDataColumn = dr.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                                spannedColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                                !spannedColumn.GridColumn.IsHidden);

                var nextLeftDataColumn = dr.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                            (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                            coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                            item.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                            !item.GridColumn.IsHidden);
                if (leftDataColumn != null)
                {
                    // return scrolled width when the covered cell left column is in view.
                    var columnVisibleLineInfo = leftDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(leftDataColumn.ColumnIndex);                
                    if (columnVisibleLineInfo != null)                       
                        origin = columnVisibleLineInfo.ScrollOffset == 0.0 ? - columnVisibleLineInfo.ClippedOrigin : hasFrozenColumn ? -(frozenColumnsWidth - columnVisibleLineInfo.ScrollOffset) : columnVisibleLineInfo.ScrollOffset;                    
                }
                else if (nextLeftDataColumn != null)
                {                    
                    // return scrolled width till the current column has arranged.
                    var nextColumnVisibleLineInfo = nextLeftDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(nextLeftDataColumn.ColumnIndex);                     

                    if (nextColumnVisibleLineInfo != null)
                    {
                        double width = 0.0;
                        int columnIndex = dc.ColumnIndex - 1;

                        while (coveredCellInfo.Left != columnIndex)
                        {
                            var visibleInfo = nextLeftDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
                            if (visibleInfo == null)
                            {
                                width += nextLeftDataColumn.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(columnIndex, columnIndex);
                                columnIndex--;
                            }
                            else
                            {
                                if (visibleInfo.ScrollOffset != 0.0)
                                {
                                    width += visibleInfo.ScrollOffset;
                                    columnIndex--;
                                }
                                else
                                    break;
                            }                            
                        }

                        // above calculation start form the left of current column to avoid adding current column width to that.
                        if (columnIndex == coveredCellInfo.Left)                        
                            width += nextLeftDataColumn.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(columnIndex, columnIndex);                                                    
                        
                        // add the scrolled width to calculate hidden width
                        if(dc.ColumnIndex == nextColumnVisibleLineInfo.LineIndex)
                            width += nextColumnVisibleLineInfo.ScrollOffset;

                        origin = width;
                    }

                    if (hasFrozenColumn)
                        origin -= frozenColumnsWidth;
                }               
            }

            return origin;
        }

        // gets the row height thats is in view
        internal double GetviewRowSize(DataColumnBase dc)
        {
            // covered cell region of column taht present in it.
            var coveredCellInfo = dc.Renderer.DataGrid.CoveredCells.GetCoveredCellInfo(dc);

            var topRow = dc.Renderer.DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Top && item.IsEnsured && item.RowVisibility == Visibility.Visible);
            var nextTopRow = dc.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).FirstOrDefault(item => (item.RowIndex > coveredCellInfo.Top) && coveredCellInfo.Contains(item.RowIndex, dc.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);            
            var bottomRow = dc.Renderer.DataGrid.RowGenerator.Items.Find(item => item.RowIndex == coveredCellInfo.Bottom && item.IsEnsured && item.RowVisibility == Visibility.Visible);           
            var nextBottomRow = dc.GridColumn.DataGrid.RowGenerator.Items.OrderBy(item => item.RowIndex).LastOrDefault(item => (item.RowIndex < coveredCellInfo.Bottom) && coveredCellInfo.Contains(item.RowIndex, dc.ColumnIndex) && item.IsEnsured && item.RowVisibility == Visibility.Visible);


            var topVisibleInfo = topRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(topRow.RowIndex) : null;
            var nextTopVisibleInfo = nextTopRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextTopRow.RowIndex) : null;


            var bottomVisibleInfo = bottomRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(bottomRow.RowIndex) : null;
            var nextBottomVisibleInfo = nextBottomRow != null ? dc.Renderer.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(nextBottomRow.RowIndex) : null;

            var viewSize = 0.0;
            int rowIndex = 0;
            
            // Calculate view height by clippedOrigin.
            if(topRow != null)           
            {
                var mergedCellHeight = topRow.GetRowSize(dc, false);
                viewSize = topVisibleInfo != null && topVisibleInfo.IsClippedOrigin ? mergedCellHeight - topVisibleInfo.ScrollOffset : mergedCellHeight;
            }
            else if (nextTopRow != null)
            {
                if(nextTopVisibleInfo != null)
                {
                    rowIndex = nextTopVisibleInfo.LineIndex;
                    if(nextTopVisibleInfo.IsClippedOrigin)
                    {                                                                  
                        if (coveredCellInfo.Bottom == rowIndex)
                            return nextTopVisibleInfo.ClippedSize;

                        viewSize += nextTopVisibleInfo.ClippedSize;
                        rowIndex++;
                    }

                    var height = dc.Renderer.DataGrid.RowGenerator.GetCoveredRowSize(rowIndex, coveredCellInfo.Bottom);
#if !WP
                    if(dc.Renderer.DataGrid is DetailsViewDataGrid)
                    {
                        if (height != dc.Renderer.DataGrid.RowHeight * (coveredCellInfo.Bottom - rowIndex  + 1))
                            height = dc.Renderer.DataGrid.RowHeight * (coveredCellInfo.Bottom - rowIndex + 1);                        
                    }
#endif
                    viewSize += height;
                }
            }

            // Calculate height by ClipedCornerExtent.
            if (dc.Renderer.DataGrid.VisualContainer.FooterRows > 0)
            {                
                if (bottomRow != null)
                    viewSize -= bottomVisibleInfo.ClippedCornerExtent;
                else if (nextBottomRow != null)
                {
                    if (nextBottomVisibleInfo != null)
                    {
                        rowIndex = nextBottomVisibleInfo.LineIndex;
                        if (nextBottomVisibleInfo.IsClippedCorner)                        
                            viewSize -= nextBottomVisibleInfo.ClippedCornerExtent;
                        rowIndex++;                                                                            
                        var height = dc.Renderer.DataGrid.RowGenerator.GetCoveredRowSize(rowIndex, coveredCellInfo.Bottom);
#if !WP
                        if (dc.Renderer.DataGrid is DetailsViewDataGrid)
                        {
                            if (height != dc.Renderer.DataGrid.RowHeight * (coveredCellInfo.Bottom - rowIndex + 1))
                                height = dc.Renderer.DataGrid.RowHeight * (coveredCellInfo.Bottom - rowIndex + 1);
                        }
#endif
                        viewSize -= height;
                    }
                }
            }

            return viewSize;           
        }        

        /// <summary>
        /// Gets the width of the merged column thats in view.
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="dr"></param>
        /// <returns></returns>
        internal double GetViewColumnSize(out bool isClippedCorner,out double horizontalScrolledValue, DataColumnBase dc, DataRowBase dr)
        {            
            // covered cell region of column that present in it.
            var coveredCellInfo = dc.Renderer.DataGrid.CoveredCells.GetCoveredCellInfo(dc);
            
            // whole merged  cell width.
            var mergedCellWidth = dr.GetColumnSize(dc.ColumnIndex, false);
            double viewSize = 0.0;
            isClippedCorner = false;
            horizontalScrolledValue = 0.0;
            var hasFooterColumn = dc.Renderer.DataGrid.VisualContainer.FooterColumns != 0 ;

            if (dr != null)
            {
                var leftDataColumn = dr.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Left &&
                                                                spannedColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                                                !spannedColumn.GridColumn.IsHidden);

                var nextLeftDataColumn = dr.VisibleColumns.OrderBy(item => item.ColumnIndex).FirstOrDefault(item =>
                                                            (item.ColumnIndex > coveredCellInfo.Left || item.ColumnIndex == coveredCellInfo.Right) &&
                                                            coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                            item.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                            !item.GridColumn.IsHidden);

                var columnVisibleLineInfo = leftDataColumn != null ? leftDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(leftDataColumn.ColumnIndex) : null ;
                var nextColumnVisibleLineInfo = nextLeftDataColumn != null ? nextLeftDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(nextLeftDataColumn.ColumnIndex) : null;

                if (leftDataColumn != null)
                {
                    if (columnVisibleLineInfo != null)
                    {
                        viewSize = columnVisibleLineInfo.ScrollOffset == 0.0 ? mergedCellWidth : mergedCellWidth - columnVisibleLineInfo.ScrollOffset;
                        horizontalScrolledValue = columnVisibleLineInfo.ScrollOffset;
                    }
                }
                else if (nextLeftDataColumn != null)
                {                    
                    if (nextColumnVisibleLineInfo != null)
                    {
                        double width = 0.0;
                        int columnIndex = dc.ColumnIndex;
                        
                        width = nextLeftDataColumn.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(columnIndex, coveredCellInfo.Right);

                        if (nextColumnVisibleLineInfo.ScrollOffset != 0.0)
                        {
                            horizontalScrolledValue = nextColumnVisibleLineInfo.ScrollOffset + nextLeftDataColumn.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(coveredCellInfo.Left, columnIndex);
                            width += nextColumnVisibleLineInfo.ClippedSize;
                        }

                        if (nextColumnVisibleLineInfo.LineIndex == coveredCellInfo.Right)
                            width = nextColumnVisibleLineInfo.ClippedSize;

                        viewSize = width;                        
                    }
                }
               

                if(hasFooterColumn)
                {                    
                    var rightDataColumn = dr.VisibleColumns.Find(spannedColumn => spannedColumn.ColumnIndex == coveredCellInfo.Right &&
                                spannedColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(spannedColumn.ColumnIndex) != null &&
                                !spannedColumn.GridColumn.IsHidden);

                    var nextRightDataColumn = dr.VisibleColumns.OrderBy(item => item.ColumnIndex).LastOrDefault(item =>
                                                                (item.ColumnIndex < coveredCellInfo.Right || item.ColumnIndex == coveredCellInfo.Left) &&
                                                                coveredCellInfo.Contains(item.RowIndex, item.ColumnIndex) &&
                                                                item.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(item.ColumnIndex) != null &&
                                                                !item.GridColumn.IsHidden);

                    var rightColumnVisibleLineInfo = rightDataColumn != null ? rightDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(rightDataColumn.ColumnIndex) : null;
                    var nextRightColumnVisibleLineInfo = nextRightDataColumn != null ? nextRightDataColumn.Renderer.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(nextRightDataColumn.ColumnIndex) : null;

                    if (rightDataColumn != null)
                    {                        
                        if(rightColumnVisibleLineInfo != null)
                        {
                            viewSize -= rightColumnVisibleLineInfo.ClippedCornerExtent;
                            if (rightColumnVisibleLineInfo.ClippedCornerExtent != 0.0)
                                isClippedCorner = true;
                        }
                    }
                    else if (nextRightDataColumn != null)
                    {
                        if (nextRightColumnVisibleLineInfo != null)
                        {
                            double width = 0.0;
                            int columnIndex = dc.ColumnIndex + 1;

                            width = nextRightDataColumn.Renderer.DataGrid.RowGenerator.GetCoveredColumnSize(columnIndex , coveredCellInfo.Right);

                            if (nextRightColumnVisibleLineInfo.ClippedCornerExtent != 0.0)
                            {
                                width += nextRightColumnVisibleLineInfo.ClippedCornerExtent;
                                isClippedCorner = true;
                            }

                            viewSize -= width;
                        }
                    }
                }                                
            }

            return viewSize;        
        }   
     
        /// <summary>
        /// Get width when the column has clipped while scroll horizontally.
        /// </summary>
        /// <param name="columnVisibleLineInfo"></param>
        /// <returns></returns>
        double GetWidth(VisibleLineInfo columnVisibleLineInfo)
        {
            double width = 0.0;

            if (columnVisibleLineInfo.IsClippedCorner && columnVisibleLineInfo.IsClippedBody && columnVisibleLineInfo.IsClippedOrigin)
                width = columnVisibleLineInfo.Size - (columnVisibleLineInfo.ClippedSize + columnVisibleLineInfo.ClippedCornerExtent);
            else if (columnVisibleLineInfo.IsClippedBody && columnVisibleLineInfo.IsClippedOrigin)
                width = columnVisibleLineInfo.Size - columnVisibleLineInfo.ClippedSize;
            else if (columnVisibleLineInfo.IsClippedCorner && columnVisibleLineInfo.IsClippedBody)
                width = columnVisibleLineInfo.ClippedSize;

            return width;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.OrientedCellsPanel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                this.GetDataRow = null;
                this.Children.Clear();
            }
            isdisposed = true;
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridHeaderCellControl"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
