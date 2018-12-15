#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WinRT && !UNIVERSAL
using System.Windows;
using System.Data;
#else
using Windows.Foundation;
#endif
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Collections;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Grid;
using System.Linq;
using System.ComponentModel;
using System.Dynamic;
using Syncfusion.Dynamic;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public abstract class DetailsViewManager : IDisposable
    {
        #region Fields
        /// <summary>
        /// Parent DataGrid
        /// </summary>
        internal SfDataGrid DataGrid;

        // For maintaining extended width and details view width
        List<double> extendedWidthList = new List<double>();
        internal List<double> detailsViewColumnWidthList = new List<double>();

        internal bool HasDetailsView
        {
            get { return DataGrid.DetailsViewDefinition != null && DataGrid.DetailsViewDefinition.Any(); }
        }

        /// <summary>
        /// Dispose the details view itemssource (ICollectionView) when the row moves out of the view.
        /// </summary>
        public static bool AllowDisposeCollectionView { get; set; }
        private bool isdisposed = false;

        #endregion

        #region Ctor

        internal DetailsViewManager(SfDataGrid dataGrid)
        {
            DataGrid = dataGrid;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Expand all the records in SfDataGrid 
        /// </summary>
        public void ExpandAllDetailsView()
        {
            // -1 will be passed as level if we need to expand all the records
            ExpandAllDetailsView(-1);
        }


        /// <summary>
        /// Expand all the records in SfDataGrid upto given level
        /// </summary>
        /// <param name="level">nested level</param>
        internal void ExpandAllDetailsView(int level)
        {
            if (this.DataGrid.View == null || !this.HasDetailsView) return;
            var lineSizeCollection = this.DataGrid.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            lineSizeCollection.ResetNestedLines();
            var lastRowIndex = this.DataGrid.VisualContainer.RowCount - this.DataGrid.DetailsViewDefinition.Count 
                - (this.DataGrid.VisualContainer.FooterRows - this.DataGrid.FooterRowsCount) 
                - ((this.DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom) ? 1 : 0)
                - ((this.DataGrid.FilterRowPosition == FilterRowPosition.Bottom) ? 1 : 0)
                - this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);
            var firstRowIndex = this.DataGrid.HeaderLineCount + this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true)
                + (DataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0)
                + (DataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0);
            for (var rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            {
                RecordEntry record;
                if (this.DataGrid.GridModel.HasGroup)
                    record = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowIndex)] as RecordEntry;
                else
                    record = this.DataGrid.View.Records[this.DataGrid.ResolveToRecordIndex(rowIndex)];
                if (record == null) continue;
                record.IsExpanded = true;
                ExpandAllDetailsViewOfRecord(level, record, rowIndex);
                rowIndex += this.DataGrid.DetailsViewDefinition.Count;
            }
            lineSizeCollection.ResumeUpdates();
            this.DataGrid.GridModel.RefreshDataRow();

            //if the DataGrid is Child Grid not parent Grid then refresh the corresponding Parent DataGrid WPF-18089
            if (this.DataGrid.NotifyListener != null)
                this.RefreshParentDataGrid(this.DataGrid);
        }

        /// <summary>
        /// Collapse all the records in SfDataGrid.
        /// </summary>
        public void CollapseAllDetailsView()
        {
            if (this.DataGrid.View == null || !this.HasDetailsView) return;
            var lineSizeCollection = this.DataGrid.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            ResetExpandedLevel(this.DataGrid);
            var lastRowIndex = this.DataGrid.VisualContainer.RowCount - this.DataGrid.DetailsViewDefinition.Count
                - (this.DataGrid.VisualContainer.FooterRows - this.DataGrid.FooterRowsCount)
                - ((this.DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom) ? 1 : 0)
                - ((this.DataGrid.FilterRowPosition == FilterRowPosition.Bottom) ? 1 : 0)
                - this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);

            var firstRowIndex = this.DataGrid.HeaderLineCount + this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true)
                + (DataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0)
                + (DataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0);
            for (var rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)          
            {
                RecordEntry record;
                if (this.DataGrid.GridModel.HasGroup)
                    record = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowIndex)] as RecordEntry;
                else
                    record = this.DataGrid.View.Records[this.DataGrid.ResolveToRecordIndex(rowIndex)];
                if (record == null) continue;
                record.IsExpanded = false;                
                var actualRowIndex = rowIndex;
                this.CollapseAllInsideRecord(record, actualRowIndex, this.DataGrid);
                rowIndex += this.DataGrid.DetailsViewDefinition.Count;
            }
            lineSizeCollection.ResumeUpdates();
            // Reset DetailsViewDataRow index and change Expanded state of the DetailsViewExpanderCell and DataRow
            DataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row is DetailsViewDataRow)
                    row.RowIndex = -1;
                if (row is DataRow)
                {
                    //WPF-19990(issue 2) need to call corresponding parent grid to change the state.
                    DataGrid.DetailsViewManager.ChangeDetailsViewExpanderState(row.RowIndex, false);
                }
            });
            this.DataGrid.GridModel.RefreshDataRow();

            //if the DataGrid is Child Grid Not Parent Grid  then refresh the corresponding Parent DataGrid WPF-18089
            if (this.DataGrid.NotifyListener != null)
                this.RefreshParentDataGrid(this.DataGrid);
        }

        /// <summary>
        /// Expands the record at specified record index.
        /// </summary>
        /// <param name="recordIndex">Index of the record.</param>
        /// <returns> Returns <b>true</b> if the record is expanded. otherwise <b>false</b>.</returns>
        public bool ExpandDetailsViewAt(int recordIndex)
        {
            if (!HasDetailsView || this.DataGrid.View == null)
                return false;
            var rowIndex = this.DataGrid.ResolveToRowIndex(recordIndex);
            var dr = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
            if (dr != null)
                dr.IsExpanded = true;
            else
            {
                RecordEntry record;
                if (this.DataGrid.GridModel.HasGroup)
                    record = this.DataGrid.View.TopLevelGroup.DisplayElements[recordIndex] as RecordEntry;
                else
                    record = this.DataGrid.View.Records[recordIndex];
                if (record == null) return false;
                if (record.IsExpanded) return true;

                bool canExpand = false;
                foreach (var viewDefinition in this.DataGrid.DetailsViewDefinition)
                {
                    var gridViewDefinition = viewDefinition as GridViewDefinition;
                    var nestedLinesitemsource = GetChildSource(record.Data, gridViewDefinition.RelationalColumn);
                    if (nestedLinesitemsource != null)
                    {
                        canExpand = true;
                        break;
                    }
                }

                if (!canExpand && DataGrid.HideEmptyGridViewDefinition)
                    return canExpand;

                record.IsExpanded = true;
                ResetExpandedLevel(this.DataGrid);
                var actualRowIndex = rowIndex;
                int level = 0;
                foreach (var viewDefinition in this.DataGrid.DetailsViewDefinition)
                {
                    actualRowIndex++;
                    level++;
                    if (viewDefinition is GridViewDefinition)
                    {
                        var gridViewDefinition = viewDefinition as GridViewDefinition;
                        if (!record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn))
                        {
                            record.ChildViews.Add(gridViewDefinition.RelationalColumn, new NestedRecordEntry(record, record.Level) { NestedLevel = (actualRowIndex - rowIndex) - 1 });
                            ResetTopLevelGroupCache();
                        }
                        var nestedLinesitemsource = GetChildSource(record.Data, gridViewDefinition.RelationalColumn);
                        var count = nestedLinesitemsource != null ? DetailsViewHelper.GetChildSourceCount(nestedLinesitemsource) * (DataGrid.DetailsViewDefinition.Count + 1) : 0;                                           

                        //AddNewRow StackedHeaders, UnBoundRows and TableSummaryRows count added here
                        count += gridViewDefinition.DataGrid.StackedHeaderRows != null ? gridViewDefinition.DataGrid.StackedHeaderRows.Count : 0;
                        count += gridViewDefinition.DataGrid.UnBoundRows != null ? gridViewDefinition.DataGrid.UnBoundRows.Count : 0;
                        count += gridViewDefinition.DataGrid.TableSummaryRows != null ? gridViewDefinition.DataGrid.TableSummaryRows.Count : 0;
                        count += gridViewDefinition.DataGrid.AddNewRowPosition != AddNewRowPosition.None ? 1 : 0;

                        var lines = new LineSizeCollection { LineCount = count + 1, DefaultLineSize = this.DataGrid.RowHeight };
                        // Below code is to set the PaddingDistance based on GridViewDefintion level
                        if (this.DataGrid.AllowDetailsViewPadding)                     
                            SetDetailsViewPadding(lines, record.Data, rowIndex, actualRowIndex, this.DataGrid);
                       
                        this.DataGrid.VisualContainer.RowHeights.SetNestedLines(actualRowIndex, lines);
                        this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, false);                        
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Collapses the DetailsViewDataGrid at specified record index.
        /// </summary>
        /// <param name="recordIndex">Index of the record.</param>
        public void CollapseDetailsViewAt(int recordIndex)
        {
            if (!HasDetailsView || this.DataGrid.View == null)
                return;

            var rowIndex = this.DataGrid.ResolveToRowIndex(recordIndex);

            var dr = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
            RecordEntry record;           
            if (this.DataGrid.GridModel.HasGroup)
                record = this.DataGrid.View.TopLevelGroup.DisplayElements[recordIndex] as RecordEntry;
            else
                record = this.DataGrid.View.Records[recordIndex];
            CollapseDetailsViewAt(rowIndex, dr, record);
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Create DetailsViewDataRow at the specified index
        /// </summary>
        /// <param name="rowIndex">rowIndex</param>
        /// <returns>DetailsViewDataRow</returns>
        internal DetailsViewDataRow CreateDetailsViewDataRow(int rowIndex)
        {
            return CreateDetailsViewDataRow(rowIndex, this.GetDetailsViewRecord(rowIndex), null);
        }

        /// <summary>
        /// Update DetailsViewDataRow in the specified index
        /// </summary>
        /// <param name="rows">DetailsViewDataRows available for reusing</param>
        /// <param name="rowIndex">rowIndex</param>
        internal void UpdateDetailsViewDataRow(IEnumerable<DataRowBase> rows, int rowIndex)
        {
            int detailsViewIndex = this.DataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);
            var detailsView = this.GetGridViewDefinition(detailsViewIndex);
            RecordEntry record = null;
            record = this.GetDetailsViewRecord(rowIndex);
            record.IsExpanded = true;
            CreateOrUpdateDetailsViewDataRow(rows, rowIndex, record, detailsView, detailsViewIndex, null);
            var dataRow = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == rowIndex);
            // While calling ExpandAllDetailsView method, need to raise DetailsViewLoading event from here
            if (dataRow != null)
                DetailsViewManager.RaiseDetailsViewEvents(dataRow, Visibility.Visible);
            }

        internal void OnDetailsViewExpanderStateChanged(RowColumnIndex rowColumnIndex, bool isExpanded)
        {
            if (rowColumnIndex.RowIndex <= 0) return;
            var recordIndex = DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex);
            var node = DataGrid.GridModel.HasGroup ? DataGrid.View.TopLevelGroup.DisplayElements[recordIndex] : DataGrid.View.Records[recordIndex];
            RecordEntry record = null;
            if (node is RecordEntry)
                record = node as RecordEntry;
            if (record == null) return;
            if (record.IsExpanded == isExpanded) return;
            var updateDataRow = new List<int>();
            if (isExpanded)
            {
                var detailsViewItemsSource = new Dictionary<string, IEnumerable>();
                foreach (var detailsView in this.DataGrid.DetailsViewDefinition)
                {
                    IEnumerable itemsSource = null;
                    if (record.ChildViews != null && record.ChildViews.ContainsKey(detailsView.RelationalColumn) && record.ChildViews[detailsView.RelationalColumn].View != null)
                        itemsSource = record.ChildViews[detailsView.RelationalColumn].View.SourceCollection;
                    if (itemsSource == null)
                        itemsSource = GetChildSource(record.Data, detailsView.RelationalColumn);
                    detailsViewItemsSource.Add(detailsView.RelationalColumn, itemsSource);
                }
                var expandingEventArg = new GridDetailsViewExpandingEventArgs(this.DataGrid) { Record = record.Data, DetailsViewItemsSource = detailsViewItemsSource };
                if (!this.DataGrid.RaiseDetailsViewExpanding(expandingEventArg))
                {
                    //WPF-19990(issue 2) need to send corresponding parent grid to change the state.
                    this.DataGrid.DetailsViewManager.ChangeDetailsViewExpanderState(rowColumnIndex.RowIndex, false);
                    return;
                }
                updateDataRow = ExpandDetailsView(rowColumnIndex.RowIndex, record, detailsViewItemsSource);
                var expandedEventArg = new GridDetailsViewExpandedEventArgs(this.DataGrid) { Record = record.Data, DetailsViewItemsSource = detailsViewItemsSource };
                this.DataGrid.RaiseDetailsViewExpanded(expandedEventArg);
            }
            else
            {
                var collapsingEventArgs = new GridDetailsViewCollapsingEventArgs(this.DataGrid) { Record = record.Data };
                if (!this.DataGrid.RaiseDetailsViewCollapsing(collapsingEventArgs))
                {
                    //WPF-19990(issue 2) need to send corresponding partent grid to change the state.
                    this.DataGrid.DetailsViewManager.ChangeDetailsViewExpanderState(rowColumnIndex.RowIndex, true);
                    return;
                }
                CollapseDetailsView(rowColumnIndex.RowIndex, record);
                var collapsedEventArg = new GridDetailsViewCollapsedEventArgs(this.DataGrid) { Record = record.Data };
                this.DataGrid.RaiseDetailsViewCollapsed(collapsedEventArg);
            }
            this.RefreshDataRow(updateDataRow);
            this.RefreshParentDataGrid(this.DataGrid);
        }


        /// <summary>
        /// Reset nested lines and DetailsViewDataRow's CatchedRowIndex
        /// </summary>
        internal void ResetExpandedDetailsView(bool canReset = false)
        {
            if (!HasDetailsView && !canReset) return;
            this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                {   
                    row.CatchedRowIndex = -1;
                });
            // WPF-19724 - While resetting DetailsView, need to reset ExtendedWidth also
            this.DataGrid.DetailsViewManager.UpdateExtendedWidth();
            var lineSizeCollection = this.DataGrid.VisualContainer.RowHeights as LineSizeCollection;
            if (lineSizeCollection == null) return;
            lineSizeCollection.SuspendUpdates();
            lineSizeCollection.ResetNestedLines();
            lineSizeCollection.ResumeUpdates();
        }

        /// <summary>
        /// When DetailsViewDataRow visibility is collapsed, need to dispose child views if AllowDisposeCollectionView is true
        /// </summary>
        internal void CollapsingDetailsViewDataRow(DetailsViewDataRow dataRow)
        {
            if (!AllowDisposeCollectionView) return;
            var rowIndex = dataRow.CatchedRowIndex;
            // WPF-20042 - When view is refreshed grid, rowIndex will be -1. so skipper here
            if (rowIndex == -1)
                return;
            // In grouping case, need to get parent record index based on DetailsViewDefintion order
            var order = this.DataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);
            var recordIndex = this.DataGrid.ResolveToRecordIndex(rowIndex - (order + 1));
            var record = this.DataGrid.GridModel.HasGroup ? this.DataGrid.View.TopLevelGroup.DisplayElements[recordIndex] : this.DataGrid.View.Records[recordIndex];
            if (record is RecordEntry)
            {
                var nestedRecord = record as RecordEntry;
                if (nestedRecord.ChildViews != null)
                {
                    var relationalColumn = this.GetGridViewDefinition(order).RelationalColumn;
                    foreach (var item in nestedRecord.ChildViews.ToList())
                    {
                        if (item.Key.Equals(relationalColumn))
                        {
                            item.Value.Dispose();
                            nestedRecord.ChildViews.Remove(item.Key);
                        }
                    }                    
                    ResetTopLevelGroupCache();
                }
            }
        }
      
        /// <summary>
        /// Need to extend parent grid last column width to accommodate child grid
        /// </summary>
        /// <param name="parentDataGrid">parent DataGrid</param>
        /// <param name="detailsViewDataGrid">detailsView DataGrid</param>
        /// <param name="detailsViewDataRowIndex">detailsViewDataRowIndex</param>
        internal static void AdjustParentsWidth(SfDataGrid parentDataGrid, SfDataGrid detailsViewDataGrid,int detailsViewDataRowIndex=-1)
        {
            var rowIndex = parentDataGrid.GetGridDetailsViewRowIndex(detailsViewDataGrid as DetailsViewDataGrid);
            // When DetailsViewDataRow is not added in RowGenerator items, rowIndex will be -1. So need to set detailsViewDataRowIndex
            if (rowIndex == -1)
                rowIndex = detailsViewDataRowIndex;
            // Order of the DetailsViewDataGrid
            int detailsViewIndex = parentDataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);            
            var detailsViewColumnWidth = detailsViewDataGrid.VisualContainer.ColumnWidths.TotalExtent +
                                    (parentDataGrid.DetailsViewPadding.Left + parentDataGrid.DetailsViewPadding.Right);
            var indentColumnWidth = ((parentDataGrid.View != null ? parentDataGrid.View.GroupDescriptions.Count : 0 )* parentDataGrid.IndentColumnWidth) +
                ((parentDataGrid.DetailsViewManager.HasDetailsView) ? parentDataGrid.ExpanderColumnWidth : 0) +
                ((parentDataGrid.showRowHeader) ? parentDataGrid.RowHeaderWidth : 0);
            // parentDataGrid column widths
            var ownerColumnWith = parentDataGrid.VisualContainer.ColumnWidths.TotalExtent - indentColumnWidth;
            // Extended width is set for last visible column
            var lastColumn = parentDataGrid.Columns.LastOrDefault(column => !column.IsHidden);
            var lastIndex = parentDataGrid.ResolveToScrollColumnIndex(parentDataGrid.Columns.IndexOf(lastColumn));

            // Reset extended width for previous last column
            var oldlastcolumn = parentDataGrid.Columns.FirstOrDefault(column => !double.IsNaN(column.ExtendedWidth) && column != lastColumn);
            if (oldlastcolumn != null)
            {
                var oldlastIndex = parentDataGrid.ResolveToScrollColumnIndex(parentDataGrid.Columns.IndexOf(oldlastcolumn));
                parentDataGrid.VisualContainer.ColumnWidths[oldlastIndex] -= oldlastcolumn.ExtendedWidth;
                oldlastcolumn.ActualWidth = parentDataGrid.VisualContainer.ColumnWidths[oldlastIndex];
                ownerColumnWith -= oldlastcolumn.ExtendedWidth;
                oldlastcolumn.ExtendedWidth = double.NaN;
            }
            //(WPF - 37043) when child grid's columnsizer is star, parentgrid and child grid's width is adjusted recursively.
            // Because of this, grid's width is increased while scrolling or resizing the window.
            if (lastColumn != null && !double.IsNaN(lastColumn.ExtendedWidth) && detailsViewDataGrid.ColumnSizer != GridLengthUnitType.Star)
                ownerColumnWith -= lastColumn.ExtendedWidth;
            double maxWidth = detailsViewColumnWidth;
            // detailsViewColumnWidth is stored in detailsViewColumnWidthList based on the order
            if (parentDataGrid.DetailsViewManager.detailsViewColumnWidthList.Count > detailsViewIndex)
            {
                var gridViewDefinition = parentDataGrid.DetailsViewManager.GetGridViewDefinition(detailsViewIndex) as GridViewDefinition;
                if (gridViewDefinition != null && gridViewDefinition.NotifyListener != null)
                {
                    // Based on other details view in the same level, need to update detailsViewColumnsWidth
                    foreach (var grid in gridViewDefinition.NotifyListener.ClonedDataGrid)
                    {
                        if (grid.VisualContainer == null)
                            continue;
                        var detailsViewColumnsWidth = grid.VisualContainer.ColumnWidths.TotalExtent +
                                     (parentDataGrid.DetailsViewPadding.Left + parentDataGrid.DetailsViewPadding.Right);
                        if (maxWidth < detailsViewColumnsWidth)
                            maxWidth = detailsViewColumnsWidth;
                    }
                }
                parentDataGrid.DetailsViewManager.detailsViewColumnWidthList[detailsViewIndex] = maxWidth;
            }
            else
                parentDataGrid.DetailsViewManager.detailsViewColumnWidthList.Add(detailsViewColumnWidth);
            // Based on maxdetailsViewColumnWidth, Extended width will be changed
            var maxdetailsViewColumnWidth = parentDataGrid.DetailsViewManager.detailsViewColumnWidthList.Max();
            var maxIndex = parentDataGrid.DetailsViewManager.detailsViewColumnWidthList.IndexOf(maxdetailsViewColumnWidth);
            if (!(ownerColumnWith < maxdetailsViewColumnWidth))
            {
                //(WPF - 37043) when child grid's columnsizer is star, parentgrid and child grid's width is adjusted recursively.
                // Because of this, grid's width is increased while scrolling or resizing the window.
                if (lastColumn != null && !double.IsNaN(lastColumn.ExtendedWidth) && detailsViewDataGrid.ColumnSizer!=GridLengthUnitType.Star)
                {
                    var lastDetailsViewColumn = detailsViewDataGrid.Columns.LastOrDefault(column => !column.IsHidden);
                    if (lastDetailsViewColumn != null && !double.IsNaN(lastDetailsViewColumn.ExtendedWidth))
                        return;
                    var maximumExtendedWidth = parentDataGrid.DetailsViewManager.extendedWidthList[maxIndex];
                    if (!double.IsNaN(maximumExtendedWidth) && maximumExtendedWidth > 0 && maxIndex != detailsViewIndex)
                    {
                        var newWidth = maximumExtendedWidth - lastColumn.ExtendedWidth;
                        parentDataGrid.VisualContainer.ColumnWidths[lastIndex] -= newWidth;
                        lastColumn.ActualWidth = parentDataGrid.VisualContainer.ColumnWidths[lastIndex];
                        lastColumn.ExtendedWidth = maximumExtendedWidth;
                        var tempWidth = parentDataGrid.DetailsViewManager.extendedWidthList[detailsViewIndex] != maximumExtendedWidth ? double.NaN : maximumExtendedWidth;
                        UpdateExtendedWidthList(parentDataGrid, detailsViewIndex, tempWidth);
                    }
                    else
                    {
                        parentDataGrid.VisualContainer.ColumnWidths[lastIndex] -= lastColumn.ExtendedWidth;
                        lastColumn.ActualWidth = parentDataGrid.VisualContainer.ColumnWidths[lastIndex];
                        lastColumn.ExtendedWidth = double.NaN;
                        UpdateExtendedWidthList(parentDataGrid, detailsViewIndex, lastColumn.ExtendedWidth);
                    }
                }
                else
                {
                    UpdateExtendedWidthList(parentDataGrid, detailsViewIndex, double.NaN);
                }
                // Need to adjust last column width of parentDataGrid based on the ExtendedWidth
                if (parentDataGrid.NotifyListener != null)
                    DetailsViewManager.AdjustParentsWidth(parentDataGrid.NotifyListener.GetParentDataGrid(), parentDataGrid);
                return;
            }
            var extendedWidth = (maxWidth - ownerColumnWith) +
                                (parentDataGrid.DetailsViewPadding.Left + parentDataGrid.DetailsViewPadding.Right);
            UpdateExtendedWidthList(parentDataGrid, detailsViewIndex, extendedWidth);
            var maxExtendedWidth = parentDataGrid.DetailsViewManager.extendedWidthList.Max();
            var newExtendedWidth = maxExtendedWidth;
            if (lastColumn != null && !double.IsNaN(lastColumn.ExtendedWidth))
            {
                newExtendedWidth = maxExtendedWidth - lastColumn.ExtendedWidth;
            }
            parentDataGrid.VisualContainer.ColumnWidths[lastIndex] += newExtendedWidth;
            // When the grid does not have any column, last column will be null
            if (lastColumn == null)
                return;
            lastColumn.ExtendedWidth = maxExtendedWidth;
            lastColumn.ActualWidth = parentDataGrid.VisualContainer.ColumnWidths[lastIndex];
            // Need to adjust last column width of parentDataGrid based on the ExtendedWidth
            if (parentDataGrid.NotifyListener != null)
                DetailsViewManager.AdjustParentsWidth(parentDataGrid.NotifyListener.GetParentDataGrid(), parentDataGrid);
        }

        /// <summary>
        /// Adjust parent with based on ExtendedWidth to accommodate DetailsViewDataGrid 
        /// </summary>
        public void AdjustParentDataGridWidth()
        {
            if (this.DataGrid.View == null || !this.DataGrid.DetailsViewDefinition.Any() || !this.DataGrid.View.Records.Any(r => r.IsExpanded))
                return;
            // If any record is expanded(Any DetailsViewDataGrid is in view), we need to adjust ExtendedWidth.
            var detailsViewDataRow = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex != -1);
            if (detailsViewDataRow != null)
                DetailsViewManager.AdjustParentsWidth(this.DataGrid, detailsViewDataRow.DetailsViewDataGrid);
        }

        /// <summary>
        /// Extended width of each details view data grid will be added in extendedWidthList to set proper extended width for parent grid last column
        /// </summary>
        /// <param name="parentDataGrid">parentDataGrid</param>
        /// <param name="index">index</param>
        /// <param name="width">width</param>
        internal static void UpdateExtendedWidthList(SfDataGrid parentDataGrid, int index, double width)
        {
            if (parentDataGrid.DetailsViewManager.extendedWidthList.Count > index)
                parentDataGrid.DetailsViewManager.extendedWidthList[index] = width;
            else
                parentDataGrid.DetailsViewManager.extendedWidthList.Add(width);
        }

        /// <summary>
        /// Raise DetailsView Loading/ Unloading Events based on the visibility
        /// </summary>
        /// <param name="row">DetailsViewDataRow</param>
        /// <param name="visibility">DetailsViewDataRow visibility</param>
        internal static void RaiseDetailsViewEvents(DetailsViewDataRow row, Visibility visibility)
        {
            // row.DataGrid - Parent grid
            // row.DetailsViewDataGrid - DetailsViewDataGrid to be loaded/ unloaded
            if (visibility == Visibility.Collapsed)
                row.DataGrid.RaiseDetailsViewUnloading(new DetailsViewLoadingAndUnloadingEventArgs(row.DataGrid, row.DetailsViewDataGrid));
            else
                row.DataGrid.RaiseDetailsViewLoading(new DetailsViewLoadingAndUnloadingEventArgs(row.DataGrid, row.DetailsViewDataGrid));
        }

        /// <summary>
        /// Refresh parent DataGrids recursively if there is change in DetailsViewDataGrid(Filtering, Adding, Removing, Grouping records)
        /// </summary>
        /// <param name="datagrid">datagrid</param>
        internal void RefreshParentDataGrid(SfDataGrid datagrid)
        {
            if (!(datagrid is DetailsViewDataGrid) || datagrid.NotifyListener == null) return;
            var parentGrid = datagrid.NotifyListener.GetParentDataGrid();
            if (parentGrid.VisualContainer == null || parentGrid.VisualContainer.RowHeights == null || parentGrid.VisualContainer.ScrollRows == null)
                return;
            var index = parentGrid.GetGridDetailsViewRowIndex(datagrid as DetailsViewDataGrid);
            if (index > 0)
            {
                var lineCollection = (parentGrid.VisualContainer.RowHeights as LineSizeCollection);
                if (lineCollection != null && lineCollection.Distances is DistanceRangeCounterCollection)
                    (lineCollection.Distances as DistanceRangeCounterCollection).InvalidateNestedEntry(index);
                int valueCount;
                var isHidden = parentGrid.VisualContainer.RowHeights.GetHidden(index, out valueCount);
                var parentRecord = parentGrid.GetGridDetailsViewRecord(datagrid as DetailsViewDataGrid);
                if (!isHidden)
                {
                    //  To refreh the parent grid height in case of deleting all the records from child grid when HideEmptyGridViewDefinition is false in case of multiple details view                   
                    if (parentGrid.HideEmptyGridViewDefinition && DetailsViewHelper.GetChildSourceCount(datagrid.View.SourceCollection) == 0)
                    {  
                        if (parentRecord is RecordEntry)                                                                            
                            parentGrid.DetailsViewManager.ApplyVisualState(index, parentRecord as RecordEntry, parentGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == index), true);                        
                        parentGrid.VisualContainer.RowHeights.SetHidden(index, index, true);
                    }
                }
                else
                {
                    // While adding records in child grid, need to unhide parent grid details view data row-WPF-17552                    
                    if (DetailsViewHelper.GetChildSourceCount(datagrid.View.SourceCollection) > 0 && parentRecord != null && (parentRecord as RecordEntry).IsExpanded)
                        parentGrid.VisualContainer.RowHeights.SetHidden(index, index, false);
                }
            }
            RefreshParentDataGrid(parentGrid);
            parentGrid.VisualContainer.ScrollRows.MarkDirty();
            parentGrid.VisualContainer.InvalidateMeasure();
        }

        /// <summary>
        /// WPF-19724 - To update ExtendedWidth when DetailsView or parent grid record Expanded State is changed
        /// </summary>
        internal void UpdateExtendedWidth(RecordEntry recordEntry = null, int index = -1)
        {
            //WPF-21372 - While clearing all the records in child grid, need to update extdend width by checking other record's epanded state also
            if (this.DataGrid.View == null || (recordEntry == null && this.DataGrid.View.Records.Any(record => record.IsExpanded)) || this.DataGrid.View.Records.Any(record => record.IsExpanded && !record.Equals(recordEntry)))
                return;
            UpdateExtendedWidth(index);
        }

        /// <summary>
        /// Update ExtendedWidth based when DetailsView or parent grid record Expanded State is changed. 
        /// </summary>
        /// <param name="index">Index of the extended with list</param>
        internal void UpdateExtendedWidth(int index)
        {
            var column = this.DataGrid.Columns.FirstOrDefault(col => !double.IsNaN(col.ExtendedWidth));
            if (column == null)
                return;
            var lastIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
            // If index=-1, need to reset all Extended width, else need to update the ExtendedWidth in the particular index only
            if (index == -1)
            {
                if (this.DataGrid.DetailsViewDefinition != null)
                {
                    for (int i = 0; i < this.DataGrid.DetailsViewDefinition.Count; i++)
                        DetailsViewManager.UpdateExtendedWidthList(this.DataGrid, i, double.NaN);
                }
            }
            else
                DetailsViewManager.UpdateExtendedWidthList(this.DataGrid, index, double.NaN);

            // Set maxExtendedWidth for columns's ExtendedWidth
            var maxExtendedWidth = this.extendedWidthList.Max();
            var newExtendedWidth = (double.IsNaN(maxExtendedWidth) ? 0 : maxExtendedWidth) - column.ExtendedWidth;
            column.ExtendedWidth = maxExtendedWidth;
            // WPF-20466 - Before resetting columns, in some cases UpdateExtendedWidth is called, so below condition called to prevent crash
            if (this.DataGrid.VisualContainer != null && lastIndex < this.DataGrid.VisualContainer.ColumnCount && lastIndex >= 0)
            {               
                this.DataGrid.VisualContainer.ColumnWidths[lastIndex] += newExtendedWidth;                
                column.ActualWidth = this.DataGrid.VisualContainer.ColumnWidths[lastIndex];
            }
        }

        /// <summary>
        /// Bring the DetailsViewDataGrid having the specified  row Index
        /// </summary>
        /// <param name="parentRowIndex">parentRowIndex</param>
        public void BringIntoView(int parentRowIndex)
        {
            UpdateDetailsViewDataRow(this.DataGrid.RowGenerator.Items, parentRowIndex);
            var detailsViewRow = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == parentRowIndex);
            if (detailsViewRow!=null)
            {
                var detailsViewGrid = detailsViewRow.DetailsViewDataGrid;
                //While navigating to DetailsViewDataRow and if the DetailsViewDataRow contains the first row as UnBoundRow
                //then the when getting UnBoundRow its return null because the UnBoundRow will be initialized on EnsureRows.
                //Hence this code has been added.
                detailsViewGrid.RefreshUnBoundRows();
                //When we scrolling the grid the line collection is already be there in VisibleLines hence the scrolling is not worked properly.
                var height = detailsViewGrid.VisualContainer.ScrollRows.HeaderExtent +
                             detailsViewGrid.VisualContainer.ScrollRows.FooterExtent;// +
                //detailsViewGrid.VisualContainer.ScrollRows.DefaultLineSize;
                detailsViewGrid.VisualContainer.ScrollRows.RenderSize = height;
            }
        }

        #endregion

        #region private Methods

        /// <summary>
        /// Reset ExpandedLevel of the ChildView recursively
        /// ExpandedLevel will be reset when expanding/collapsing the records individually
        /// If ExpandedLevel is not set, individaully expanded/collpased records will lost its state as DetailsViewDataGrid will be ensured based on ExpandedLevel        
        /// </summary>
        /// <param name="grid">SfDataGrid</param>
        internal void ResetExpandedLevel(SfDataGrid grid)
        {
            if (grid is DetailsViewDataGrid && grid.NotifyListener != null)
            {
                var parentGrid = grid.NotifyListener.GetParentDataGrid();
                var detailsViewDataRow = parentGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == grid);
                if (detailsViewDataRow == null)
                    return;
                var order = parentGrid.GetOrderForDetailsViewBasedOnIndex(detailsViewDataRow.RowIndex - 1);
                var recordIndex = parentGrid.ResolveToRecordIndex(detailsViewDataRow.RowIndex - (order + 1));
                if (recordIndex == -1)
                    return;
                var record = parentGrid.GridModel.HasGroup ? parentGrid.View.TopLevelGroup.DisplayElements[recordIndex] : parentGrid.View.Records[recordIndex];
                var detailsViewDefintion = parentGrid.DetailsViewManager.GetGridViewDefinition(order);
                if (record is RecordEntry && (record as RecordEntry).ChildViews != null && (record as RecordEntry).ChildViews.ContainsKey(detailsViewDefintion.RelationalColumn))
                {
                    (record as RecordEntry).ChildViews[detailsViewDefintion.RelationalColumn].ExpandedLevel = -1;
                    (record as RecordEntry).ChildViews[detailsViewDefintion.RelationalColumn].IsNestedLevelExpanded = false;
                    ResetExpandedLevel(parentGrid);
                }
            }
        }

        /// <summary>
        /// While adding child view, need to Reset TopLevelGroupCache 
        /// </summary>
        internal void ResetTopLevelGroupCache()
        {
           if (this.DataGrid.View.GroupDescriptions.Any() && this.DataGrid.GroupSummaryRows.Any())
                this.DataGrid.View.TopLevelGroup.ResetCache = true;
        }

        /// <summary>
        /// Create new DetailsViewDataRow with specified rowIndex
        /// </summary>
        internal DetailsViewDataRow CreateDetailsViewDataRow(int rowIndex, RecordEntry record, IEnumerable itemsSource)
        {
            var detailsViewDataRow = new DetailsViewDataRow();
            detailsViewDataRow.DataGrid = this.DataGrid;
            detailsViewDataRow.RowRegion = RowRegion.Body;
            detailsViewDataRow.RowLevel = 0;

            int detailsViewIndex = this.DataGrid.GetOrderForDetailsViewBasedOnIndex(rowIndex - 1);
            var detailsView = this.GetGridViewDefinition(detailsViewIndex);

            InitializeDetailsViewDataRow(rowIndex, record, detailsViewDataRow, detailsViewIndex, detailsView, itemsSource);

            detailsViewDataRow.InitializeDataRow(this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLines());
            //Passed the argument detailsViewDataRow to the ChecVisualState method otherwise there the detailsviewDatarow is set as null
            //Hence the Nullreference Exception is thrown.
            if (this.DataGrid.HideEmptyGridViewDefinition)
                ApplyVisualState(rowIndex, record, detailsViewDataRow);
            else
                detailsViewDataRow.ApplyContentVisualState(detailsViewIndex ==
                                                            this.DataGrid.DetailsViewDefinition.Count - 1
                                                                ? "LastCell"
                                                                : "NormalCell");
            return detailsViewDataRow;
        }

        /// <summary>
        /// Apply visual state for DetailsViewDataRow
        /// </summary>
        /// <param name="rowIndex"> DetailsViewDataRow index</param>
        /// <param name="record">parent record</param>
        /// <param name="detailsViewDataRow">DetailsViewDataRow</param>
        /// <param name="lastCellVisualState">true if last visul state should be applied</param>
        internal void ApplyVisualState(int rowIndex, RecordEntry record, DetailsViewDataRow detailsViewDataRow, bool lastCellVisualState = false)
        {
            int count = this.DataGrid.DetailsViewDefinition.Count;
            var parentRowIndex = this.DataGrid.ResolveToRowIndex(record);            
            var provider = this.DataGrid.View.GetPropertyAccessProvider();
            for (int i = count - 1; i >= 0; i--)
            {
                var gridViewDefinition = this.DataGrid.DetailsViewDefinition[i];
                var childSource = provider.GetValue(record.Data, gridViewDefinition.RelationalColumn) as IEnumerable;
                if (childSource != null && DetailsViewHelper.GetChildSourceCount(childSource) > 0)
                {
                    if ((parentRowIndex + i + 1) == rowIndex)
                        detailsViewDataRow.ApplyContentVisualState("LastCell");
                    else
                    {
                        if (lastCellVisualState)
                        {
                            var lastdetailsviewRow = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == (parentRowIndex + i + 1));
                            if (lastdetailsviewRow!=null)
                            {
                                lastdetailsviewRow.ApplyContentVisualState("LastCell");
                                // WPF-19921 Reset lastdetailsviewRow index to update indent column
                                if (this.DataGrid.View.GroupDescriptions.Any())
                                    lastdetailsviewRow.RowIndex = -1;
                                if (lastdetailsviewRow.DetailsViewDataGrid != null)
                                    lastdetailsviewRow.DetailsViewDataGrid.VisualContainer.RowHeights.PaddingDistance = this.DataGrid.DetailsViewPadding.Top + this.DataGrid.DetailsViewPadding.Bottom + 2;
                            }
                        }
                        else
                            detailsViewDataRow.ApplyContentVisualState("NormalCell");
                    }
                    return;
                }
            }
            // WPF-19921 Reset parent data row index to update indent column if there is no child grid 
            if (this.DataGrid.View.GroupDescriptions.Any())
            {
                var parentRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(r => r.RowIndex == parentRowIndex);
                if (parentRow != null)
                    parentRow.RowIndex = -1;
            }
        }

        /// <summary>
        /// Set DetailsView padding for RowHeights linesize collection
        /// </summary>
        /// <param name="lines">linesize collection</param>
        /// <param name="record">parent record</param>
        /// <param name="parentRowIndex">parent RowIndex</param>
        /// <param name="childRowIndex">child RowIndex</param>
        /// <param name="dataGrid">parent grid</param>        
        internal void SetDetailsViewPadding(LineSizeCollection lines, object record, int parentRowIndex, int childRowIndex, SfDataGrid dataGrid)
        {
            if (!dataGrid.HideEmptyGridViewDefinition)
            {
                //WPF-22676 Set the DetailsViewDataGrid top padding to LineSizeCollection TopPaddingDistance
                if (childRowIndex - parentRowIndex == this.DataGrid.DetailsViewDefinition.Count)
                    lines.PaddingDistance = this.DataGrid.DetailsViewPadding.Top + this.DataGrid.DetailsViewPadding.Bottom + 2;
                else
                    lines.PaddingDistance = this.DataGrid.DetailsViewPadding.Top + this.DataGrid.DetailsViewPadding.Bottom + 1;
                return;
            }

            var count = dataGrid.DetailsViewDefinition.Count;
            for (int i = count - 1; i >= 0; i--)
            {
                var gridViewDefinition = dataGrid.DetailsViewDefinition[i];
                var childSource = GetChildSource(record, gridViewDefinition.RelationalColumn) as IEnumerable;
                if (childSource != null && DetailsViewHelper.GetChildSourceCount(childSource) > 0)
                {
                    //WPF-22676 Set the DetailsViewDataGrid top padding to LineSizeCollection TopPaddingDistance
                    if ((parentRowIndex + i + 1) == childRowIndex)
                        lines.PaddingDistance = dataGrid.DetailsViewPadding.Top + dataGrid.DetailsViewPadding.Bottom + 2;
                    else
                        lines.PaddingDistance = dataGrid.DetailsViewPadding.Top + dataGrid.DetailsViewPadding.Bottom + 1;
                    return;
                }
            }
        }           

        /// <summary>
        /// Initialize DetailsViewDataRow 
        /// Set itemssource for DetailsViewDataGrid 
        /// Apply visual state, filtering and adjust parent width
        /// </summary>
        internal void InitializeDetailsViewDataRow(int rowIndex, RecordEntry record, DetailsViewDataRow detailsViewDataRow, int detailsViewIndex, ViewDefinition detailsView, IEnumerable itemsSource)
        {
            detailsViewDataRow.RowIndex = rowIndex;
            detailsViewDataRow.OnRowIndexChanged();
            detailsViewDataRow.CatchedRowIndex = rowIndex;
            detailsViewDataRow.IsExpanded = true;
            InitializeDetailsViewRow(rowIndex, record, detailsViewDataRow, detailsViewIndex, detailsView, itemsSource);
        }

        // While reusing DetailsViewDataRow, need to remove that DetailsViewDataGrid's child grid from clonedDataGrid
        internal void UpdateClonedDataGrid(DetailsViewDataRow detailsViewDataRow)
        {
            if (this.DataGrid.ReuseRowsOnItemssourceChange)
                return;
            if (detailsViewDataRow.DetailsViewDataGrid.RowGenerator.Items.Any(row => row is DetailsViewDataRow))
            {
                foreach (var row in detailsViewDataRow.DetailsViewDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>())
                {
                    var clonedDataGrid = (row.DetailsViewDataGrid.NotifyListener.SourceDataGrid.NotifyListener as DetailsViewNotifyListener).ClonedDataGrid;
                    if (clonedDataGrid.Contains(row.DetailsViewDataGrid))
                    {
                        clonedDataGrid.Remove(row.DetailsViewDataGrid);
                        // WPF-20080 - Recursively remove DetailsViewDataGrid from ClonedDataGrid
                        if (row.DetailsViewDataGrid.DetailsViewManager.HasDetailsView)
                            this.UpdateClonedDataGrid(row);
                    }
                }
            }
        }

        ///// <summary>
        ///// While reusing DetailsViewDataRow, need to ensure properties of DetailsViewDataGrid
        ///// </summary>
        ///// <param name="detailsViewDataGrid">DetailsViewDataGrid</param>
        //private void EnsureProperties(DetailsViewDataGrid detailsViewDataGrid)
        //{
        //    (detailsViewDataGrid as IDetailsViewNotifier).SuspendNotifyListener();
        //    var sourceDataGrid = detailsViewDataGrid.NotifyListener.SourceDataGrid;
        //    var isAdded = false;
        //    foreach (var sourceColumn in sourceDataGrid.Columns)
        //    {
        //        var detailsViewColumn = detailsViewDataGrid.Columns.FirstOrDefault(column => column.MappingName == sourceColumn.MappingName);
        //        if (detailsViewColumn != null)
        //        {
        //            if (detailsViewColumn.GetType() == sourceColumn.GetType())
        //            {
        //                CloneHelper.CloneProperties(sourceColumn, detailsViewColumn, typeof(GridColumn));
        //                // If there is FilterPredicates in detailsViewColumn, after clearing this we need to clone FilterPredicates from sourceColumn
        //                if ((detailsViewColumn as GridColumn).FilterPredicates != null && (detailsViewColumn as GridColumn).FilterPredicates.Any())
        //                    (detailsViewColumn as GridColumn).FilterPredicates.Clear();
        //                CloneHelper.CloneCollection(sourceColumn.FilterPredicates, (detailsViewColumn as GridColumn).FilterPredicates, typeof(FilterPredicate));
        //            }
        //            else
        //            {
        //                detailsViewDataGrid.Columns.Remove(detailsViewColumn);
        //                var destinationItem = CloneHelper.CreateClonedInstance(sourceColumn, typeof(GridColumn));
        //                CloneHelper.CloneCollection(sourceColumn.FilterPredicates, (destinationItem as GridColumn).FilterPredicates, typeof(FilterPredicate));
        //                detailsViewDataGrid.Columns.Add(destinationItem as GridColumn);
        //                isAdded = true;
        //            }
        //        }
        //        else
        //        {
        //            var destinationItem = CloneHelper.CreateClonedInstance(sourceColumn, typeof(GridColumn));
        //            CloneHelper.CloneCollection(sourceColumn.FilterPredicates, (destinationItem as GridColumn).FilterPredicates, typeof(FilterPredicate));
        //            detailsViewDataGrid.Columns.Add(destinationItem as GridColumn);
        //            isAdded = true;
        //        }
        //    }
        //    // Need to clone GroupColumnDescriptions first. Else, SortColumnDescriptions will be changed when GroupColumnDescriptions is changed
        //    CloneHelper.EnsureCollection<GroupColumnDescriptions, GroupColumnDescription>(sourceDataGrid.GroupColumnDescriptions, detailsViewDataGrid.GroupColumnDescriptions, (target, source) => target.ColumnName == source.ColumnName);
        //    CloneHelper.EnsureCollection<SortColumnDescriptions, SortColumnDescription>(sourceDataGrid.SortColumnDescriptions, detailsViewDataGrid.SortColumnDescriptions, (target, source) => target.ColumnName == source.ColumnName && target.SortDirection == source.SortDirection);

        //    if (detailsViewDataGrid.VisualContainer.RowCount >= detailsViewDataGrid.HeaderLineCount)
        //    {
        //        var endHeaderIndex = detailsViewDataGrid.HeaderLineCount - (detailsViewDataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top)
        //            + (detailsViewDataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop ? 1 : 0)
        //            + (detailsViewDataGrid.FilterRowPosition == FilterRowPosition.FixedTop ? 1 : 0));
        //        for (var i = 0; i < endHeaderIndex; i++)
        //            detailsViewDataGrid.VisualContainer.RowHeights[i] = detailsViewDataGrid.HeaderRowHeight;
        //    }

        //    if (isAdded)
        //        AdjustParentsWidth(this.DataGrid, detailsViewDataGrid);

        //    (detailsViewDataGrid as IDetailsViewNotifier).ResumeNotifyListener();
        //}

        /// <summary>
        /// Expand nested lines while expanding all records
        /// </summary>      
        internal void ExpandNestedLines(object record, LineSizeCollection lines, GridViewDefinition gridViewDefinition, RecordEntry recordEntry, int level)
        {
            var itemsource = GetChildSource(record, gridViewDefinition.RelationalColumn);
            if (recordEntry != null && recordEntry.ChildViews != null &&
                recordEntry.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) && recordEntry.ChildViews[gridViewDefinition.RelationalColumn].View != null)
            {
                itemsource = recordEntry.ChildViews[gridViewDefinition.RelationalColumn].View.Records;
            }
            if (itemsource == null)
                return;
            ExpandNestedLinesofRecord(itemsource, lines, gridViewDefinition, recordEntry, level);
        }

        /// <summary>
        /// While collapding all records, need to collapse expanded lines
        /// </summary>

        internal void CollapeseNestedLines(SfDataGrid dataGrid, GridViewDefinition gridViewDefinition)
        {
            if (gridViewDefinition.DataGrid.DetailsViewDefinition == null || !gridViewDefinition.DataGrid.DetailsViewDefinition.Any())
                return;
            var lines = dataGrid.VisualContainer.RowHeights as LineSizeCollection;
            lines.SuspendUpdates();
            this.ResetExpandedLevel(dataGrid);
            var lastRowIndex = dataGrid.VisualContainer.RowCount - dataGrid.DetailsViewDefinition.Count
                - (dataGrid.VisualContainer.FooterRows - dataGrid.FooterRowsCount)
                - ((dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom) ? 1 : 0)
                - ((dataGrid.FilterRowPosition == FilterRowPosition.Bottom) ? 1 : 0)
                - dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);
            var firstRowIndex = dataGrid.HeaderLineCount + dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true)
                + (dataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0)
                + (dataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0);
            for (var rowIndex = firstRowIndex; rowIndex < lastRowIndex; rowIndex++)
            {
                RecordEntry record;
                if (dataGrid.GridModel.HasGroup)
                    record = dataGrid.View.TopLevelGroup.DisplayElements[dataGrid.ResolveToRecordIndex(rowIndex)] as RecordEntry;
                else
                    record = dataGrid.View.Records[dataGrid.ResolveToRecordIndex(rowIndex)];
                if (record == null)
                    continue;
                record.IsExpanded = false;                
                var actualRowIndex = rowIndex;
                this.CollapseAllInsideRecord(record, actualRowIndex, dataGrid);

                rowIndex += gridViewDefinition.DataGrid.DetailsViewDefinition.Count;
            }
            lines.ResumeUpdates();
            // Reset DetailsViewDataRow index and change Expanded state of the DetailsViewExpanderCell and DataRow
            dataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row is DetailsViewDataRow)
                    row.RowIndex = -1;
                if (row is DataRow)
                {
                    // WPF-19990(issue 2) need to call corresponding partent grid to change the state.
                    dataGrid.DetailsViewManager.ChangeDetailsViewExpanderState(row.RowIndex, false);
                }
            });
        }

        /// <summary>
        /// Change DetailsViewExpanderState according to record IsExpanded value
        /// </summary>
        /// <param name="rowIndex">rowIndex</param>
        /// <param name="isExpanded">Record isExpanded property</param>
        private void ChangeDetailsViewExpanderState(int rowIndex, bool isExpanded)
        {
            var dr = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
            if (dr == null) return;
            var dc = dr.VisibleColumns.FirstOrDefault(column => column.IsExpanderColumn);
            if (dc == null) return;
            var expander = dc.ColumnElement as GridDetailsViewExpanderCell;
            if (expander == null) return;
            expander.SuspendChangedAction = true;
            dr.IsExpanded = isExpanded;
            expander.SuspendChangedAction = false;
        }

        /// <summary>
        /// Refresh all data rows except some rows
        /// </summary>
        /// <param name="skipRowIndex">collection row indices need to be skipped while refreshing</param>
        private void RefreshDataRow(ICollection<int> skipRowIndex)
        {
            if (this.DataGrid.VisualContainer == null) return;
            this.DataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (skipRowIndex.Contains(row.RowIndex) && row is DetailsViewDataRow) return;
                if (row.RowRegion == RowRegion.Body || row.RowRegion == RowRegion.Footer)
                    row.RowIndex = -1;
            });
            this.DataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Get child itemssource for the record
        /// </summary>
        /// <param name="record">record</param>
        /// <param name="relationName">relationName</param>
        /// <returns>IEnumerable source</returns>
        public IEnumerable GetChildSource(object record, string relationName)
        {
            if (string.IsNullOrEmpty(relationName))
                throw new InvalidOperationException("RelationalColumn cannot be null, must set the RelationalColumn.");
#if WPF
            // If record is DataRow/DataRowView
            if (record is System.Data.DataRow || record is DataRowView)
            {
                //if (PropertyDescriptorExtensions.ContainsSpecialChars(relationName))
                //{
                //    DataTable dt = null;
                //    if (record is System.Data.DataRow)
                //        dt = ((System.Data.DataRow)record).Table;
                //    else
                //        dt = ((DataRowView)record).DataView.Tabl;

                //    if (!dt.Columns.Contains(ColumnName))
                //        return null;
                //}
                return ItemPropertiesProvider.GetDataTableValue(record, relationName) as IEnumerable;
            }
            else
#endif
            // If record is Dynamic type
            if (DynamicHelper.CheckIsDynamicObject(record.GetType()))
            {
                return new DynamicHelper().GetValue(record, relationName) as IEnumerable;
                //return DynamicPropertiesProvider.GetDynamicValue(record, relationName) as IEnumerable;
            }
            else
            {
#if WPF
                PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(record.GetType());
                var pinfo = pdc.GetPropertyDescriptor(relationName);
                if (pinfo == null)
                    return null;
                var source = pdc.GetValue(record, relationName);
                return source as IEnumerable;
#else
                PropertyInfoCollection descriptor = new PropertyInfoCollection(record.GetType());
                var pinfo = descriptor.GetPropertyDescriptor(relationName);
                if (pinfo == null)
                    return null; 
                var source = descriptor.GetValue(record, relationName);
                return source as IEnumerable;
#endif
            }
        }

        //private static int ScrollDetailsViewParent(SfDataGrid dataGrid, double point)
        //{
        //    var parentGrid = dataGrid.NotifyListener.GetParentDataGrid();
        //    var lines = parentGrid.VisualContainer.ScrollColumns.GetVisibleLines();
        //    var startingPoint = lines[lines.firstBodyVisibleIndex].ClippedCorner + parentGrid.VisualContainer.HorizontalOffset;
        //    //var startingPoint = parentGrid.VisualContainer.HorizontalOffset - lines[lines.firstBodyVisibleIndex].ClippedCorner;
        //    var lastpoint = lines[lines.LastBodyVisibleIndex].ClippedCorner;
        //    if (point < lastpoint && point > startingPoint)
        //        return -1;
        //    var index = 0;
        //    if (point > lastpoint)
        //    {
        //        index = lines[lines.LastBodyVisibleIndex].LineIndex;
        //        do
        //        {
        //            int repeatValueConut;
        //            index += 1;
        //            if (index >= parentGrid.VisualContainer.ScrollColumns.LineCount)
        //            {
        //                index -= 1;
        //                break;
        //            }
        //            lastpoint += parentGrid.VisualContainer.ColumnWidths.GetSize(index, out repeatValueConut);
        //        } while (point > lastpoint);
        //    }
        //    else if (point < startingPoint)
        //    {
        //        index = lines[lines.firstBodyVisibleIndex].LineIndex;
        //        do
        //        {
        //            int repeatValueCount;
        //            index -= 1;
        //            if (index <= 0)
        //            {
        //                index += 1;
        //                break;
        //            }
        //            startingPoint -= parentGrid.VisualContainer.ColumnWidths.GetSize(index, out repeatValueCount);
        //        } while (point < startingPoint);
        //    }
        //    parentGrid.VisualContainer.ScrollColumns.ScrollInView(index);
        //    parentGrid.VisualContainer.InvalidateMeasure();
        //    return index;
        //}

        #region AttachedProperty
        /// <summary>
        /// if DetailsView is used and DisableLastColumnResizing is true, it will disbale the parent grid last column resizing when parent grid width is less than that of child grid
        /// </summary>

        public static readonly DependencyProperty DisableLastColumnResizingProperty =
          DependencyProperty.RegisterAttached("DisableLastColumnResizing", typeof(bool), typeof(DetailsViewManager), new PropertyMetadata(false, OnDisableLastColumnResizingChanged));

        public static void SetDisableLastColumnResizing(SfDataGrid element, bool value)
        {
            element.SetValue(DisableLastColumnResizingProperty, value);
        }

        public static bool GetDisableLastColumnResizing(SfDataGrid element)
        {
            return (bool)element.GetValue(DisableLastColumnResizingProperty);
        }
        private static void OnDisableLastColumnResizingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            grid.DetailsViewManager.WireOrUnwireEvent((bool)e.NewValue);
        }

        private void WireOrUnwireEvent(bool needsToWire)
        {
            if (needsToWire)
                this.DataGrid.ResizingColumns += DataGrid_ResizingColumns;
            else
                this.DataGrid.ResizingColumns -= DataGrid_ResizingColumns;
        }

        void DataGrid_ResizingColumns(object sender, ResizingColumnsEventArgs e)
        {
            var grid = sender as SfDataGrid;
            // For detailsview grid, sender will be SourceDataGrid. So need to get OriginalSender
            if (e.OriginalSender is DetailsViewDataGrid)
                grid = e.OriginalSender as SfDataGrid;
            if (grid.View == null)
                return;
            var columnIndex = grid.ResolveToGridVisibleColumnIndex(e.ColumnIndex);
            if (columnIndex != grid.Columns.Count - 1)
                return;
            double totalwidth = 0;
            foreach (var column in grid.Columns)
                totalwidth += column.ActualWidth;
            var startcolumnnIndex = grid.ResolveToStartColumnIndex();
            double indentcolumnsWidth = startcolumnnIndex * grid.IndentColumnWidth;
            totalwidth += indentcolumnsWidth;
            double remainingWidth = totalwidth - grid.Columns[columnIndex].ActualWidth;
            if (grid.DetailsViewDefinition.Any())
            {
                var records = grid.View.Records.Where(r => r.IsExpanded);
                foreach (var record in records)
                {
                    if (record != null)
                    {
                        double maxWidth = 0;
                        int index;
                        if (grid.GridModel.HasGroup)
                            index = grid.View.TopLevelGroup.DisplayElements.IndexOf(record);
                        else
                            index = grid.View.Records.IndexOf(record);
                        foreach (var def in grid.DetailsViewDefinition)
                        {
                            var childgrid = grid.GetDetailsViewGrid(index, def.RelationalColumn);
                            if (childgrid != null)
                            {
                                double childColumnTotalWidth = 0;
                                foreach (var column in childgrid.Columns)
                                    childColumnTotalWidth += column.ActualWidth;
                                var childstartcolumnnIndex = childgrid.ResolveToStartColumnIndex();
                                childColumnTotalWidth += childstartcolumnnIndex * childgrid.ExpanderColumnWidth;
                                if (childColumnTotalWidth >= maxWidth)
                                    maxWidth = childColumnTotalWidth;
                            }
                        }
                        if (remainingWidth + e.Width < maxWidth + indentcolumnsWidth)
                        {
                            e.Cancel = true;
                            break;
                        }
                    }
                }
            }
        }


        #endregion

        #endregion

        #region Abstract methods

        /// <summary>
        /// If there is any available DetailsViewDataRow, it will be used. else new DetailsViewDataRow will be created
        /// </summary>
        internal abstract void CreateOrUpdateDetailsViewDataRow(IEnumerable<DataRowBase> rows, int rowIndex, RecordEntry record, ViewDefinition detailsView, int detailsViewIndex, IEnumerable itemsSource);

        internal abstract void InitializeDetailsViewRow(int rowIndex, RecordEntry record, DetailsViewDataRow detailsViewDataRow, int detailsViewIndex, ViewDefinition detailsView, IEnumerable itemsSource);

        internal abstract void ExpandAllDetailsViewOfRecord(int level, RecordEntry record, int rowIndex);

        /// <summary>
        /// Expand DetailsView of particular record
        /// </summary>
        /// <param name="rowIndex">Row Index</param>
        /// <param name="record">Record</param>
        /// <param name="detailsViewItemsSource">detailsViewItemsSource</param>
        /// <returns>list of details view index</returns>
        internal abstract List<int> ExpandDetailsView(int rowIndex, RecordEntry record, Dictionary<string, IEnumerable> detailsViewItemsSource);

        internal abstract void ExpandNestedLinesofRecord(IEnumerable itemsSource, LineSizeCollection lines, GridViewDefinition gridViewDefinition, RecordEntry recordEntry, int level);

        internal abstract void CollapseDetailsViewAt(int rowIndex, DataRowBase dr, RecordEntry record);

        /// <summary>
        /// Collapse the particular record's DetailsView
        /// </summary>
        /// <param name="rowIndex">rowIndex of the parent record</param>
        /// <param name="record">parent record</param>
        internal abstract void CollapseDetailsView(int rowIndex, RecordEntry record);

        internal abstract void CollapseAllInsideRecord(RecordEntry record, int actualRowIndex, SfDataGrid dataGrid);

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewManager"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewManager"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if(isDisposing)
                DataGrid = null;
            isdisposed = true;
        }

        #endregion
    }
}
