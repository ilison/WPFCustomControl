#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Data.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if UWP
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public class DetailsViewGridManager : DetailsViewManager
    {
        public DetailsViewGridManager(SfDataGrid dataGrid) : base(dataGrid)
        {

        }

        internal override void InitializeDetailsViewRow(int rowIndex, RecordEntry record, DetailsViewDataRow detailsViewDataRow, int detailsViewIndex, ViewDefinition detailsView, IEnumerable itemsSource)
        {
            if (detailsView is GridViewDefinition)
            {
                var gridViewDefinition = detailsView as GridViewDefinition;
                if (gridViewDefinition.NotifyListener == null)
                    gridViewDefinition.NotifyListener = new DetailsViewNotifyListener(gridViewDefinition.DataGrid, this.DataGrid);
                int repeatValueCount;
                var isHidden = this.DataGrid.VisualContainer.RowHeights.GetHidden(rowIndex, out repeatValueCount);
                if (detailsViewDataRow.DetailsViewDataGrid == null)
                {
                    var grid = gridViewDefinition.NotifyListener.CopyPropertiesFromSourceDataGrid(gridViewDefinition.DataGrid);
                    (grid.NotifyListener as DetailsViewNotifyListener)._parentDataGrid = this.DataGrid;
                    // Apply style for DetailsViewDataGrid
                    if (this.DataGrid.ReadLocalValue(SfDataGrid.DetailsViewDataGridStyleProperty) != DependencyProperty.UnsetValue)
                        grid.Style = this.DataGrid.DetailsViewDataGridStyle;
                    //grid.VisualContainer.ScrollOwner = this.DataGrid.VisualContainer.ScrollOwner;
                    detailsViewDataRow.DetailsViewDataGrid = grid;
                }
               (detailsViewDataRow.DetailsViewDataGrid.NotifyListener as DetailsViewNotifyListener)._parentDataGrid = this.DataGrid;

                bool needtoinvalidatelinecollection = false;

                if (record.ChildViews != null && record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) && record.ChildViews[gridViewDefinition.RelationalColumn].View != null)
                {
                    var recordView = record.ChildViews[gridViewDefinition.RelationalColumn].View;
                    if (detailsViewDataRow.DetailsViewDataGrid.View == recordView)
                    {
                        // If we assign new RowHeights in CreateOrUpdateDetailsViewDataRow, need to update Row and Column count here 
                        if (detailsViewDataRow.DetailsViewDataGrid.VisualContainer.RowHeights.TotalExtent == 0)
                        {
                            detailsViewDataRow.DetailsViewDataGrid.RefreshHeaderLineCount();
                            detailsViewDataRow.DetailsViewDataGrid.UpdateRowAndColumnCount(false);
                            detailsViewDataRow.DetailsViewDataGrid.RowGenerator.Items.ForEach(item => item.RowIndex = -1);
                        }
                        // Need to update DataGrid in CollectionView
                        if (detailsViewDataRow.DetailsViewDataGrid.View is IGridViewNotifier)
                        {
                            (detailsViewDataRow.DetailsViewDataGrid.View as IGridViewNotifier).AttachGridView(detailsViewDataRow.DetailsViewDataGrid);
                        }

                    }
                    if (detailsViewDataRow.DetailsViewDataGrid.View != recordView)
                    {
                        detailsViewDataRow.RowData = recordView.SourceCollection;
                        // Remove detailsview grid from ClonedDataGrid list
                        UpdateClonedDataGrid(detailsViewDataRow);

                        // WPF-22862 - in some cases, recordView is view of some DetailsViewDataGrid. But it will not be taken for reuse. 
                        // So need to unwire events from that DetailsViewDataGrid
                        if (recordView is IGridViewNotifier)
                        {
                            var grid = (recordView as IGridViewNotifier).GetDataGrid();
                            if (grid is SfDataGrid)
                            {
                                var dataGrid = grid as SfDataGrid;
                                dataGrid.GridModel.UnWireEvents(false);
                                (recordView as IGridViewNotifier).DetachGridView();
                                dataGrid.suspendItemsSourceChanged = true;
                                dataGrid.ItemsSource = null;
                                dataGrid.View = null;
                                dataGrid.RefreshHeaderLineCount();
                                dataGrid.UpdateRowAndColumnCount(false);
                                dataGrid.suspendItemsSourceChanged = false;
                            }
                        }

                        // While reusing view, need to clear previous view properties. Because properties will be updated in DetailsViewDataGrid only
                        DetailsViewHelper.ClearViewProperties(recordView);
                        detailsViewDataRow.DetailsViewDataGrid.ItemsSource = recordView;
                    }

                    // Expand the records in DetailsViewDataGrid based on IsNestedLevelExpanded and ExpandedLevel
                    if (record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded || record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel > 1)
                        detailsViewDataRow.DetailsViewDataGrid.DetailsViewManager.ExpandAllDetailsView(record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded ? -1 : record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel - 1);
                    else
                    {
                        // If DetailsViewDataGrid's record is already expanded, need to collapse it
                        if (record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel != -1)
                            detailsViewDataRow.DetailsViewDataGrid.CollapseAllDetailsView();
                    }
                    if (detailsViewDataRow.DetailsViewDataGrid.View != null)
                    {
                        foreach (var childrecord in detailsViewDataRow.DetailsViewDataGrid.View.Records.Where(r => r.IsExpanded))
                        {
                            for (int i = 1; i <= detailsViewDataRow.DetailsViewDataGrid.DetailsViewDefinition.Count; i++)
                            {
                                var childRowIndex = detailsViewDataRow.DetailsViewDataGrid.ResolveToRowIndex(childrecord);
                                int valueCount;
                                if (detailsViewDataRow.DetailsViewDataGrid.VisualContainer.RowHeights.GetHidden(childRowIndex + i, out valueCount))
                                    detailsViewDataRow.DetailsViewDataGrid.VisualContainer.RowHeights.SetHidden(childRowIndex + i, childRowIndex + i, false);
                            }
                        }
                    }
                }
                else
                {
                    if (itemsSource == null)
                        itemsSource = GetChildSource(record.Data, gridViewDefinition.RelationalColumn);

                    // If itemssource is same, we need to set itemssource as null.
                    if (detailsViewDataRow.DetailsViewDataGrid.ItemsSource == itemsSource)
                    {
                        // Remove detailsview grid from ClonedDataGrid list
                        UpdateClonedDataGrid(detailsViewDataRow);
                        detailsViewDataRow.DetailsViewDataGrid.ItemsSource = null;
                        // if itemsSource and DetailsViewDataGrid's itemssource is null, need to update RowCount here(since itemssourcechanged callback will not be called)
                        if (itemsSource == null)
                        {
                            detailsViewDataRow.DetailsViewDataGrid.RefreshHeaderLineCount();
                            detailsViewDataRow.DetailsViewDataGrid.UpdateRowAndColumnCount(false);
                        }
                    }
                    else
                        needtoinvalidatelinecollection = true;
                    detailsViewDataRow.RowData = itemsSource;
                    // Remove detailsview grid from ClonedDataGrid list
                    UpdateClonedDataGrid(detailsViewDataRow);
                    detailsViewDataRow.DetailsViewDataGrid.ItemsSource = detailsViewDataRow.RowData;

                    // Expand the records in DetailsViewDataGrid based on IsNestedLevelExpanded and ExpandedLevel
                    if (!isHidden && ((record.ChildViews != null && record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) && record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel > 1) || (record.ChildViews != null && record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) && record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded)))
                        detailsViewDataRow.DetailsViewDataGrid.DetailsViewManager.ExpandAllDetailsView(record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded ? -1 : record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel - 1);
                    else
                    {
                        // If DetailsViewDataGrid's record is already expanded, need to collapse it
                        if (record.ChildViews != null && record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) && record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel != -1)
                            detailsViewDataRow.DetailsViewDataGrid.CollapseAllDetailsView();
                    }
                    var isNestedExpanded = false;
                    var level = 0;
                    if (record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn))
                    {
                        isNestedExpanded = record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded;
                        level = record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel;
                    }
                    record.ChildViews.Remove(gridViewDefinition.RelationalColumn);
                    record.PopulateChildView(detailsViewDataRow.DetailsViewDataGrid.View, detailsViewIndex, gridViewDefinition.RelationalColumn, isNestedExpanded);
                    record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel = level;
                    ResetTopLevelGroupCache();
                }

                if (detailsViewDataRow.WholeRowElement != null)
                {
                    if (this.DataGrid.HideEmptyGridViewDefinition)
                        ApplyVisualState(rowIndex, record, detailsViewDataRow);
                    else
                        detailsViewDataRow.ApplyContentVisualState(detailsViewIndex ==
                                                                    this.DataGrid.DetailsViewDefinition.Count - 1
                                                                        ? "LastCell"
                                                                        : "NormalCell");
                }
                // Set PaddingDistance for DetailsViewDataGrid
                if (detailsViewDataRow.DetailsViewDataGrid.AllowDetailsViewPadding)
                {
                    var paddingDistance = detailsViewDataRow.DetailsViewDataGrid.DetailsViewPadding.Top + detailsViewDataRow.DetailsViewDataGrid.DetailsViewPadding.Bottom + 1;
                    if (detailsViewIndex == this.DataGrid.DetailsViewDefinition.Count - 1 || detailsViewDataRow.DetailsViewContentPresenter.CurrentVisualState == "LastCell")
                        paddingDistance += 1;
                    detailsViewDataRow.DetailsViewDataGrid.VisualContainer.RowHeights.PaddingDistance = paddingDistance;
                }

                this.DataGrid.VisualContainer.RowHeights.SetNestedLines(rowIndex, detailsViewDataRow.DetailsViewDataGrid.VisualContainer.RowHeights);

                if (detailsViewDataRow.DetailsViewDataGrid.View != null && detailsViewDataRow.DetailsViewDataGrid.Columns.Any(column => column.FilterPredicates.Any()))
                {
                    detailsViewDataRow.DetailsViewDataGrid.GridModel.ApplyFilter();
                    // WPF-18240 - While expanding the record fist time, detailsview data row will not be added to parent grid. So unable to invalidate nested lines in RefreshParentDataGrid method
                    // Below code is added to invalidate nested lines when filtering is applied
                    //var lineCollection = (this.DataGrid.VisualContainer.RowHeights as LineSizeCollection);
                    //if (lineCollection != null && lineCollection.Distances is DistanceRangeCounterCollection)
                    //    (lineCollection.Distances as DistanceRangeCounterCollection).InvalidateNestedEntry(rowIndex);
                    needtoinvalidatelinecollection = true;
                }
#if WPF
                if (detailsViewDataRow.DetailsViewDataGrid.SearchHelper.AllowFiltering)
                {
                    detailsViewDataRow.DetailsViewDataGrid.View.Filter = detailsViewDataRow.DetailsViewDataGrid.SearchHelper.FilterRecords;
                    detailsViewDataRow.DetailsViewDataGrid.View.RefreshFilter();
                    needtoinvalidatelinecollection = true;
                }
#endif          
                if (needtoinvalidatelinecollection)
                {
                    var lineCollection = (this.DataGrid.VisualContainer.RowHeights as LineSizeCollection);
                    if (lineCollection != null && lineCollection.Distances is DistanceRangeCounterCollection)
                        (lineCollection.Distances as DistanceRangeCounterCollection).InvalidateNestedEntry(rowIndex);
                }
                detailsViewDataRow.DetailsViewDataGrid.DetailsViewManager.UpdateExtendedWidth();
                //(WPF - 37043) When Child grid's columnsizer is Star, Parent Grid's width need not to be adjusted.
                if (detailsViewDataRow.DetailsViewDataGrid.ColumnSizer != GridLengthUnitType.Star)
                    AdjustParentsWidth(this.DataGrid, detailsViewDataRow.DetailsViewDataGrid, rowIndex);
                // To refresh parent grid height in case of multi level nested grid and grouping
                DetailsViewHelper.RefreshParentDataGridRows(detailsViewDataRow.DetailsViewDataGrid);
            }
        }

        internal override void ExpandAllDetailsViewOfRecord(int level, RecordEntry record, int rowIndex)
        {
            var actualRowIndex = rowIndex;
            int index = 0;
            foreach (var viewDefinition in this.DataGrid.DetailsViewDefinition)
            {
                actualRowIndex++;
                if (viewDefinition is GridViewDefinition)
                {
                    index++;
                    var gridViewDefinition = viewDefinition as GridViewDefinition;
                    int repeatValueCount;
                    var isHidden = this.DataGrid.VisualContainer.RowHeights.GetHidden(actualRowIndex, out repeatValueCount);
                    LineSizeCollection lines = null;
                    if (!isHidden)
                        lines = this.DataGrid.VisualContainer.RowHeights.GetNestedLines(actualRowIndex) as LineSizeCollection;
                    //WPF-18239 - If nested lines are reset, lines will be null(even though it is visible).
                    // Below code is added to create nested lines
                    if (lines == null)
                    {
                        var itemsource = GetChildSource(record.Data, gridViewDefinition.RelationalColumn);
                        int count = 0;
                        // If need to expand inner levels also, DetailsViewDefinition count is added with SourceCollection count
                        if (level > 1 || level == -1)
                            count = itemsource != null ? DetailsViewHelper.GetChildSourceCount(itemsource) * (gridViewDefinition.DataGrid.DetailsViewDefinition.Count + 1) : 0;
                        else
                            count = itemsource != null ? DetailsViewHelper.GetChildSourceCount(itemsource) : 0;

                        if (this.DataGrid.HideEmptyGridViewDefinition && count == 0)
                        {
                            // WPF-19997 - Create childView only if HideEmptyGridViewDefinition
                            if (!record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn))
                            {
                                record.ChildViews.Add(gridViewDefinition.RelationalColumn, new NestedRecordEntry(record, record.Level) { NestedLevel = (actualRowIndex - rowIndex) - 1 });
                                this.ResetTopLevelGroupCache();
                            }
                            continue;
                        }
                        // AddNewRow, StackedHeaders, UnBoundRows and TableSummaryRows count added here
                        count += gridViewDefinition.DataGrid.StackedHeaderRows != null ? gridViewDefinition.DataGrid.StackedHeaderRows.Count : 0;
                        count += gridViewDefinition.DataGrid.UnBoundRows != null ? gridViewDefinition.DataGrid.UnBoundRows.Count : 0;
                        count += gridViewDefinition.DataGrid.TableSummaryRows != null ? gridViewDefinition.DataGrid.TableSummaryRows.Count : 0;
                        count += gridViewDefinition.DataGrid.AddNewRowPosition != AddNewRowPosition.None ? 1 : 0;
                        count += gridViewDefinition.DataGrid.FilterRowPosition != FilterRowPosition.None ? 1 : 0;

                        // DefaultLineSize should be set based on the SourceDataGrid's RowHeight
                        lines = new LineSizeCollection { LineCount = count + 1, DefaultLineSize = gridViewDefinition.DataGrid.RowHeight };
                    }
                    // Below code is to set the PaddingDistance based on GridViewDefintion level
                    if (this.DataGrid.AllowDetailsViewPadding)
                        SetDetailsViewPadding(lines, record.Data, rowIndex, actualRowIndex, this.DataGrid);
                    // Need to expand nested lines also if level is greater than 1 or need to expand all records (level is -1)                     
                    if (level > 1 || level == -1)
                        ExpandNestedLines(record.Data, lines, gridViewDefinition, record, level);
                    this.DataGrid.VisualContainer.RowHeights.SetNestedLines(actualRowIndex, lines);
                    this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, false);

                    if (!record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn))
                    {
                        record.ChildViews.Add(gridViewDefinition.RelationalColumn, new NestedRecordEntry(record, record.Level) { NestedLevel = (actualRowIndex - rowIndex) - 1 });
                        ResetTopLevelGroupCache();
                    }
                    record.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel = level;

                    // Set IsNestedLevelExpanded if level is -1 (ExpandAllDetailsView is called)
                    if (level == -1)
                        record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded = true;
                    else
                        record.ChildViews[gridViewDefinition.RelationalColumn].IsNestedLevelExpanded = false;
                }
            }
        }

        internal override void CollapseAllInsideRecord(RecordEntry record, int actualRowIndex, SfDataGrid dataGrid)
        {
            foreach (var viewDefinition in dataGrid.DetailsViewDefinition)
            {
                if (!(viewDefinition is GridViewDefinition)) continue;
                var gridViewDefinition = viewDefinition as GridViewDefinition;
                if (record.ChildViews != null && record.ChildViews.ContainsKey(viewDefinition.RelationalColumn))
                {
                    record.ChildViews[viewDefinition.RelationalColumn].IsNestedLevelExpanded = false;
                    record.ChildViews[viewDefinition.RelationalColumn].ExpandedLevel = -1;
                }
                actualRowIndex++;
                int repeatValueCount;
                var isHidden = dataGrid.VisualContainer.RowHeights.GetHidden(actualRowIndex, out repeatValueCount);
                if (isHidden) continue;
                var dr = dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.RowIndex == actualRowIndex);
                if (dr != null) dataGrid.DetailsViewManager.CollapeseNestedLines(dr.DetailsViewDataGrid, gridViewDefinition);
                dataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, true);
                dataGrid.VisualContainer.RowHeights.SetNestedLines(actualRowIndex, null);
            }
        }

        internal override void CollapseDetailsViewAt(int rowIndex, DataRowBase dr, RecordEntry record)
        {
            if (dr != null)
            {
                dr.IsExpanded = false;
                if (record == null)
                    return;
                foreach (var viewDefinition in this.DataGrid.DetailsViewDefinition)
                {
                    if (viewDefinition is GridViewDefinition)
                    {
                        if (record.ChildViews != null && record.ChildViews.ContainsKey(viewDefinition.RelationalColumn))
                        {
                            record.ChildViews[viewDefinition.RelationalColumn].IsNestedLevelExpanded = false;
                            record.ChildViews[viewDefinition.RelationalColumn].ExpandedLevel = -1;
                        }
                    }
                }
            }
            else
            {
                if (record == null) return;
                if (!record.IsExpanded) return;
                record.IsExpanded = false;
                ResetExpandedLevel(this.DataGrid);
                var actualRowIndex = rowIndex;
                foreach (var viewDefinition in this.DataGrid.DetailsViewDefinition)
                {
                    actualRowIndex++;
                    if (viewDefinition is GridViewDefinition)
                    {
                        int repeatValueCount;
                        if (record.ChildViews != null && record.ChildViews.ContainsKey(viewDefinition.RelationalColumn))
                        {
                            record.ChildViews[viewDefinition.RelationalColumn].IsNestedLevelExpanded = false;
                            record.ChildViews[viewDefinition.RelationalColumn].ExpandedLevel = -1;
                        }
                        var isHidden = this.DataGrid.VisualContainer.RowHeights.GetHidden(actualRowIndex, out repeatValueCount);
                        if (!isHidden)
                        {
                            this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, true);
                            this.DataGrid.VisualContainer.RowHeights.SetNestedLines(actualRowIndex, null);
                        }
                    }
                }
            }
        }

        internal override void ExpandNestedLinesofRecord(IEnumerable itemsSource, LineSizeCollection lines, GridViewDefinition gridViewDefinition, RecordEntry recordEntry, int level)
        {
            if (gridViewDefinition.DataGrid.DetailsViewDefinition != null &&
                gridViewDefinition.DataGrid.DetailsViewDefinition.Any())
            {
                var rowIndex = 1;
                foreach (var item in itemsSource)
                {
                    var actualRowIndex = rowIndex;
                    int index = 0;
                    foreach (var nestedViewDefinition in gridViewDefinition.DataGrid.DetailsViewDefinition)
                    {
                        if (nestedViewDefinition is GridViewDefinition)
                        {
                            index++;
                            var nestedgridViewDefinition = nestedViewDefinition as GridViewDefinition;
                            actualRowIndex++;

                            int repeatValueCount;
                            var isHidden = lines.GetHidden(actualRowIndex, out repeatValueCount);
                            LineSizeCollection nestedLines = null;
                            if (!isHidden)
                                nestedLines = lines.GetNestedLines(actualRowIndex) as LineSizeCollection;

                            int nestedLevel = recordEntry != null && recordEntry.ChildViews != null && recordEntry.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn) ? recordEntry.ChildViews[gridViewDefinition.RelationalColumn].ExpandedLevel : level;
                            if (nestedLines == null)
                            {
                                var detailsViewLinesitemsource = GetChildSource((item is RecordEntry) ? (item as RecordEntry).Data : item, nestedgridViewDefinition.RelationalColumn);
                                int count = 0;
                                // If need to expand inner levels also, DetailsViewDefinition count is added with SourceCollection count
                                if (nestedLevel > this.DataGrid.DetailsViewManager.GetDefinitionLevel(nestedgridViewDefinition) || nestedLevel == -1)
                                    count = detailsViewLinesitemsource != null ? DetailsViewHelper.GetChildSourceCount(detailsViewLinesitemsource) * (nestedgridViewDefinition.DataGrid.DetailsViewDefinition.Count + 1) : 0;
                                else
                                    count = detailsViewLinesitemsource != null ? DetailsViewHelper.GetChildSourceCount(detailsViewLinesitemsource) : 0;

                                // While creating lines, LineCount is set by considering DetailsViewDataRow also. But if we HideEmptyGridViewDefintion is true, we need to hide it(RowHeight = 0)
                                if (nestedgridViewDefinition.DataGrid.HideEmptyGridViewDefinition && count == 0)
                                {
                                    lines.SetHidden(actualRowIndex, actualRowIndex, true);
                                    continue;
                                }
                                //AddNewRow, StackedHeaders, UnBoundRows and TableSummaryRows count added here
                                count += nestedgridViewDefinition.DataGrid.StackedHeaderRows != null ? nestedgridViewDefinition.DataGrid.StackedHeaderRows.Count : 0;
                                count += nestedgridViewDefinition.DataGrid.UnBoundRows != null ? nestedgridViewDefinition.DataGrid.UnBoundRows.Count : 0;
                                count += nestedgridViewDefinition.DataGrid.TableSummaryRows != null ? nestedgridViewDefinition.DataGrid.TableSummaryRows.Count : 0;
                                count += nestedgridViewDefinition.DataGrid.AddNewRowPosition != AddNewRowPosition.None ? 1 : 0;

                                nestedLines = new LineSizeCollection { LineCount = count + 1, DefaultLineSize = nestedgridViewDefinition.DataGrid.RowHeight };
                            }
                            // Below code is to set the PaddingDistance based on GridViewDefintion level
                            if (nestedgridViewDefinition.DataGrid.AllowDetailsViewPadding)
                                SetDetailsViewPadding(nestedLines, (item is RecordEntry) ? (item as RecordEntry).Data : item, rowIndex, actualRowIndex, gridViewDefinition.DataGrid);
                            if (nestedLevel > this.DataGrid.DetailsViewManager.GetDefinitionLevel(nestedgridViewDefinition) || nestedLevel == -1)
                                ExpandNestedLines((item is RecordEntry) ? (item as RecordEntry).Data : item, nestedLines, nestedgridViewDefinition, (item is RecordEntry) ? item as RecordEntry : null, nestedLevel);
                            lines.SetNestedLines(actualRowIndex, nestedLines);
                            lines.SetHidden(actualRowIndex, actualRowIndex, false);
                        }
                    }
                    rowIndex += gridViewDefinition.DataGrid.DetailsViewDefinition.Count + 1;
                }
            }
        }

        internal override void CreateOrUpdateDetailsViewDataRow(IEnumerable<DataRowBase> rows, int rowIndex, RecordEntry record, ViewDefinition detailsView, int detailsViewIndex, IEnumerable itemsSource)
        {
            // In grouping, 2 detailsview data row may have same CatchedRowIndex.So we need to check NotifyListener ClonedDataGrid. Then only it will return proper details view data row in case of multiple details view           
            var dr = rows.OfType<DetailsViewDataRow>().FirstOrDefault(r => r.CatchedRowIndex == rowIndex && ((detailsView is GridViewDefinition) ? (detailsView as GridViewDefinition).NotifyListener.ClonedDataGrid.Contains((r as DetailsViewDataRow).DetailsViewDataGrid) : true));
            if (dr != null)
            {
                InitializeDetailsViewDataRow(rowIndex, record, dr, detailsViewIndex, detailsView, itemsSource);
                // We will update the changes immediately in all DetailsViewDataGrids using ClonedDataGrid.So below code is commented
                //EnsureProperties(dr.DetailsViewDataGrid);
                return;
            }

            if (detailsView is GridViewDefinition)
            {
                var gridViewDefinition = detailsView as GridViewDefinition;
                if (gridViewDefinition.NotifyListener != null)
                {
                    //Added the condition check CurrentItem of the DetailsViewDataGrid to preserve from reuse. If we reuse the currentcell maintained grid we cant get the grid after scrolling.
                    dr = rows.OfType<DetailsViewDataRow>().FirstOrDefault(r => !r.IsEnsured && r.DetailsViewDataGrid != this.DataGrid.SelectedDetailsViewGrid && r.DetailsViewDataGrid.SelectionController.CurrentCellManager.CurrentCell == null && gridViewDefinition.NotifyListener.ClonedDataGrid.Contains(r.DetailsViewDataGrid));
                    if (dr != null)
                    {
                        int repeatValueCount;
                        var isHidden = this.DataGrid.VisualContainer.RowHeights.GetHidden(rowIndex, out repeatValueCount);
                        LineSizeCollection lines = null;
                        if (!isHidden)
                            lines = this.DataGrid.VisualContainer.RowHeights.GetNestedLines(rowIndex) as LineSizeCollection;
                        // While updating rowHeightsProvider, need to unwire the events. It will be hooked it InitialRefresh method
                        dr.DetailsViewDataGrid.GridColumnSizer.UnwireEvents();
                        dr.DetailsViewDataGrid.VisualContainer.UpdateRowInfo(lines, dr.DetailsViewDataGrid.RowHeight);
                        InitializeDetailsViewDataRow(rowIndex, record, dr, detailsViewIndex, detailsView, itemsSource);
                        // We will update the changes immediately in all DetailsViewDataGrids using ClonedDataGrid.So below code is commented
                        //EnsureProperties(dr.DetailsViewDataGrid);
                        return;
                    }
                }
            }
            dr = CreateDetailsViewDataRow(rowIndex, record, itemsSource);
            var detailsViewDataGrid = dr.DetailsViewDataGrid;
            // We will update the changes immediately in all DetailsViewDataGrids using ClonedDataGrid.So below code is commented
            //if (detailsViewDataGrid != null && detailsViewDataGrid.AutoGenerateColumns && detailsViewDataGrid.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
            //    EnsureProperties(detailsViewDataGrid);
            this.DataGrid.RowGenerator.Items.Add(dr);
        }

        internal override List<int> ExpandDetailsView(int rowIndex, RecordEntry record, Dictionary<string, IEnumerable> detailsViewItemsSource)
        {
            var actualRowIndex = rowIndex;
            record.IsExpanded = true;
            ResetExpandedLevel(this.DataGrid);
            var count = 0;
            var updateDataRow = new List<int>();
            var DetailsViewDefinition = this.DataGrid.DetailsViewDefinition;
            // If the grid is DetailsViewDataGrid, need to access SourceDataGrid's DetailsViewDefinition(Since NotifyListener is assigned for SourceDataGrid's DetailsViewDefinition only)
            if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                DetailsViewDefinition = this.DataGrid.NotifyListener.SourceDataGrid.DetailsViewDefinition;
            foreach (var detailsView in DetailsViewDefinition)
            {
                actualRowIndex++;
                var gridViewDefinition = detailsView as GridViewDefinition;
                IEnumerable itemsSource;
                if (detailsViewItemsSource.TryGetValue(gridViewDefinition.RelationalColumn, out itemsSource) && itemsSource != null)
                    CreateOrUpdateDetailsViewDataRow(this.DataGrid.RowGenerator.Items, actualRowIndex, record, detailsView, count, itemsSource);
                else if (!this.DataGrid.HideEmptyGridViewDefinition)
                    CreateOrUpdateDetailsViewDataRow(this.DataGrid.RowGenerator.Items, actualRowIndex, record, detailsView, count, null);
                // WPF-20080 - When HideEmptyGridViewDefinition is true and there is no records in details view, need to hide DetailsViewDataRow
                else
                {
                    // WPF-19997 - Create childView only if HideEmptyGridViewDefinition
                    if (!record.ChildViews.ContainsKey(gridViewDefinition.RelationalColumn))
                    {
                        record.ChildViews.Add(gridViewDefinition.RelationalColumn, new NestedRecordEntry(record, record.Level) { NestedLevel = (actualRowIndex - rowIndex) - 1 });
                        ResetTopLevelGroupCache();
                    }
                    count++;
                    updateDataRow.Add(actualRowIndex);
                    this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, true);
                    continue;
                }
                updateDataRow.Add(actualRowIndex);
                var view = record.ChildViews[gridViewDefinition.RelationalColumn];
                if (view != null && !this.DataGrid.HideEmptyGridViewDefinition)
                    this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, false);
                else if (view != null && DetailsViewHelper.GetChildSourceCount(view.View.SourceCollection) > 0)
                    this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, false);
                else
                    this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIndex, actualRowIndex, true);
                count++;
            }

            if (this.DataGrid.HideEmptyGridViewDefinition)
            {
                if (record.ChildViews.Count == this.DataGrid.DetailsViewDefinition.Count)
                {
                    var isExpanded = record.ChildViews.Any(view => view.Value.View != null && DetailsViewHelper.GetChildSourceCount(view.Value.View.SourceCollection) > 0);
                    if (!isExpanded)
                    {
                        var recordIndex = this.DataGrid.ResolveToRecordIndex(rowIndex);
                        this.DataGrid.DetailsViewManager.CollapseDetailsViewAt(recordIndex);
                    }
                }
            }


            return updateDataRow;
        }

        internal static int GetChildSourceCount(object source)
        {
            //(WPF -37043) To avoid break the below method is called here.
            return DetailsViewHelper.GetChildSourceCount(source);
        }

        internal override void CollapseDetailsView(int rowIndex, RecordEntry record)
        {
            var actualRowIdx = rowIndex;
            record.IsExpanded = false;
            ResetExpandedLevel(this.DataGrid);
            foreach (var detailsView in this.DataGrid.DetailsViewDefinition)
            {
                actualRowIdx++;
                if (record.ChildViews.ContainsKey(detailsView.RelationalColumn))
                {
                    record.ChildViews[detailsView.RelationalColumn].IsNestedLevelExpanded = false;
                    record.ChildViews[detailsView.RelationalColumn].ExpandedLevel = -1;
                }
                this.DataGrid.VisualContainer.RowHeights.SetHidden(actualRowIdx, actualRowIdx, true);
                this.DataGrid.VisualContainer.RowHeights.SetNestedLines(actualRowIdx, null);
            }
        }
    }
}
