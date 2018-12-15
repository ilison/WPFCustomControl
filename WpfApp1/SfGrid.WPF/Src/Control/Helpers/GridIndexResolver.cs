#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WPF
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents an extension class that provides an set of index resolver helper methods in SfDataGrid.
    /// </summary>
    public static class GridIndexResolver
    {
        /// <summary>
        /// Resolves the table summary index corresponding to the specified row index in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// Specifies the corresponding row index to get its table summary index.
        /// </param>
        /// <returns>
        /// The table summary index corresponding to the specified row index ; returns -1 , if the VisualContainer of the SfDataGrid is null.
        /// </returns>
        public static int ResolveToTableSummaryIndex(this SfDataGrid dataGrid, int rowIndex)
        {
            if (dataGrid.VisualContainer != null)
            {
                var endindex = dataGrid.VisualContainer.RowCount - 1;
                var startindex = dataGrid.VisualContainer.RowCount 
                    - dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) 
                    - dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);

                if (rowIndex >= startindex && rowIndex <= endindex)
                    return rowIndex - startindex;

                if (rowIndex >= 0)
                    return rowIndex;
            }
            return -1;
        }

        /// <summary>
        /// Resolves the record index corresponding to the specified row index in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// Specifies the row index to get its corresponding record index.
        /// </param>
        /// <returns>
        /// Returns the record index of the specified row index in SfDataGrid.
        /// </returns>
        public static int ResolveToRecordIndex(this SfDataGrid dataGrid, int rowIndex)
        {
            if (dataGrid.VisualContainer != null && dataGrid.HasView && rowIndex != -1)
            {
                rowIndex = rowIndex - dataGrid.ResolveStartIndexBasedOnPosition();
                if (rowIndex < 0)
                    return -1;

                if (dataGrid.GridModel.HasGroup)
                {
                    //When rowIndex is greater than DisplayElements count which is invalid rowIndex, hence returned -1.
                    if (rowIndex >= dataGrid.View.TopLevelGroup.DisplayElements.Count)
                        return -1;
                }
                else
                {
                    if(dataGrid.DetailsViewManager.HasDetailsView)
                        rowIndex = rowIndex/(dataGrid.DetailsViewDefinition.Count + 1);
                    // Fix for WPF-17142 issue 2 - The rowIndex sholud return -1 for other than record rows.
                    if (rowIndex >= dataGrid.View.Records.Count)
                        rowIndex = -1;
                }
                if (rowIndex >= 0)
                    return rowIndex;
            }
            return -1;
        }
        
        /// <summary>
        /// Resolves the row index corresponding to the specified record index in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="recordIndex">
        /// Specifies record index to get its corresponding row index.
        /// </param>
        /// <returns>
        /// Returns the row index of the specified record index in SfDataGrid.
        /// </returns>
        public static int ResolveToRowIndex(this SfDataGrid dataGrid, int recordIndex)
        {
            if (recordIndex <= -1)
                return -1;

            if (dataGrid.GridModel.HasGroup)
            {
                if (recordIndex > -1)
                {
                    var groupPos = recordIndex + dataGrid.ResolveStartIndexBasedOnPosition();
                    return groupPos;
                }
                return -1;
            }
            else
            {
                var index = recordIndex * (dataGrid.DetailsViewDefinition != null ? dataGrid.DetailsViewDefinition.Count + 1 : 1);
                return index + dataGrid.ResolveStartIndexBasedOnPosition();
            }
        }

        /// <summary>
        /// Get the Index value for the previous focused GridColumn
        /// </summary>
        /// <param name="DataGrid"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        internal static int GetPreviousFocusGridColumnIndex(this SfDataGrid DataGrid, int columnIndex, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return GetNextFocusGridColumnIndex(DataGrid, columnIndex);
            var index = columnIndex;
            if (index < 0 || index >= DataGrid.Columns.Count)
                return -1;
            var gridColumn = DataGrid.Columns[index];
            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || gridColumn.IsHidden)
            {
                index = GetPreviousFocusGridColumnIndex(DataGrid, columnIndex - 1);
            }
            return index;
        }

        /// <summary>
        /// Get the index value for the next focused GridColumn.
        /// </summary>
        /// <param name="DataGrid"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        internal static int GetNextFocusGridColumnIndex(this SfDataGrid DataGrid,int columnIndex , FlowDirection flowdirection = FlowDirection.LeftToRight )
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return GetPreviousFocusGridColumnIndex(DataGrid, columnIndex);
            var index = columnIndex;
            if (index < 0 || index >= DataGrid.Columns.Count)
                return -1;
            var gridColumn = DataGrid.Columns[index];

            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || gridColumn.IsHidden)
            {
                if (columnIndex + 1 < DataGrid.GetLastColumnIndex())
                    index = GetNextFocusGridColumnIndex(DataGrid, columnIndex + 1);
            }            
            return index;            
        }

        /// <summary>
        /// Resolves the row index corresponding to the specified record.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="recordItem">
        /// Specifies the record to get its corresponding row index.
        /// </param>
        /// <returns>
        /// Returns the row index of the specified record.
        /// </returns>
        public static int ResolveToRowIndex(this SfDataGrid dataGrid, object recordItem)
        {
            if (dataGrid.GetRecordsCount(false) == 0)
                return -1;

            // Need to get the record or record index from DisplayElements when the Source is IQueryable. Since we will keep null record entries View.Records. Entries will be kept in Group.Records alone.
            var recordIndex = dataGrid.GridModel.HasGroup && dataGrid.isIQueryable ?
                dataGrid.View.TopLevelGroup.DisplayElements.IndexOf(recordItem) :
                dataGrid.View.Records.IndexOfRecord(recordItem);           

            if (recordIndex < 0)
                return -1;

            if (!dataGrid.GridModel.HasGroup)
            {
                if (dataGrid.DetailsViewManager.HasDetailsView)
                    return (recordIndex * (dataGrid.DetailsViewDefinition.Count+1)) + dataGrid.ResolveStartIndexBasedOnPosition();
                else
                    return recordIndex + dataGrid.ResolveStartIndexBasedOnPosition();
            }
            else
            {
                var record = dataGrid.isIQueryable ? dataGrid.View.TopLevelGroup.DisplayElements.GetItem(recordItem): 
                             dataGrid.View.Records.GetRecord(recordItem) ;
                if (record.Parent != null)
                {
                    var grpRecordIndex = dataGrid.View.TopLevelGroup.DisplayElements.IndexOf(record);
                    if (grpRecordIndex > -1)
                    {
                        var groupPos = grpRecordIndex + dataGrid.ResolveStartIndexBasedOnPosition();
                        return groupPos;
                    }
                }
            }
            return -1;
        }

        /// <summary>
        /// Resolves the row index corresponding to the specified node entry.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="nodeEntry">
        /// Specifies the node entry to get its corresponding row index.
        /// </param>
        /// <returns>
        /// Returns the row index of the specified node entry.
        /// </returns>
        public static int ResolveToRowIndex(this SfDataGrid DataGrid, NodeEntry nodeEntry)
        {
            if (nodeEntry == null || DataGrid.GetRecordsCount(false) == 0)
                return -1;

            if (!DataGrid.GridModel.HasGroup)
            {
                //The Below Condition "(NestedRecordentry).nodeEntry.Parent != null" is added while the parent  becomes null when we call the ExpandAll().
                //So here we have return the rowindex as -1 when the parent is null.
                var rowIndex = nodeEntry is NestedRecordEntry ? (((NestedRecordEntry)nodeEntry).Parent != null ? ResolveToRowIndex(DataGrid, (((NestedRecordEntry)nodeEntry).Parent as RecordEntry).Data) : -1) : ResolveToRowIndex(DataGrid, (nodeEntry as RecordEntry).Data);
                if(rowIndex > -1)
                {
                    if (nodeEntry is NestedRecordEntry)
                    {
                        var parentRecordEntry = (nodeEntry as NestedRecordEntry).Parent as RecordEntry;
                        var Key = parentRecordEntry.ChildViews.Where(kvp => kvp.Value == nodeEntry).Select(kvp => kvp.Key).FirstOrDefault();
                        int index = parentRecordEntry.ChildViews.Keys.ToList().IndexOf(Key);
                        return rowIndex + index + 1;
                    }
                    else
                        return rowIndex;
                }
            }
            else
            {
                var grpRecordIndex = DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(nodeEntry);
                if (grpRecordIndex > -1)
                {
                    var groupPos = grpRecordIndex + DataGrid.ResolveStartIndexBasedOnPosition();
                    return groupPos;
                }
            }
            return -1;

        }

        /// <summary>
        /// Resolves the start index of the specified group.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="group">
        /// Specifies the group to get its corresponding start index.
        /// </param>
        /// <returns>
        /// The start index of the specified group; returns -1; if the column is not grouped in SfDataGrid.
        /// </returns>
        public static int ResolveStartIndexOfGroup(this SfDataGrid dataGrid, Group group)
        {
            if (dataGrid.GridModel.HasGroup && dataGrid.View != null)
            {
                var startIndex = dataGrid.ResolveStartIndexBasedOnPosition();
                var grpIdx = dataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group);
                return grpIdx + startIndex;
            }
            return -1;
        }

        /// <summary>
        /// Resolves the start index position in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// Returns start index position in SfDataGrid.
        /// </returns>
        public static int ResolveStartIndexBasedOnPosition(this SfDataGrid dataGrid)
        {
            var topBodyCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);
            return dataGrid.HeaderLineCount + topBodyCount + (dataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0) 
                + (dataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0);
        }

        /// <summary>
        /// Resolves the visible column index for the specified column index in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="visibleColumnIndex">
        /// The visibleColumnIndex.
        /// </param>
        /// <returns>
        /// Returns the corresponding visible column index for the specified column index.
        /// </returns>
        public static int ResolveToGridVisibleColumnIndex(this SfDataGrid dataGrid, int visibleColumnIndex)
        {
            //UWP-1450-after clearing columns, return -1 .
            if (dataGrid.Columns.Count == 0)
                return -1;
            var indentColumnCount = (dataGrid.View != null ? dataGrid.View.GroupDescriptions.Count : 0) +
                (dataGrid.DetailsViewManager.HasDetailsView ? 1 : 0);
            int resolvedIndex = visibleColumnIndex - (indentColumnCount + (dataGrid.showRowHeader ? 1 : 0));
            return resolvedIndex;
        }

        /// <summary>
        /// Resolves the scroll column index for the specified column index in SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="gridColumnIndex">
        /// The corresponding column index to get the scroll column index.
        /// </param>
        /// <returns>
        /// Returns the scroll column index for the specified column index.
        /// </returns>
        public static int ResolveToScrollColumnIndex(this SfDataGrid dataGrid, int gridColumnIndex)
        {
            var indentColumnCount = (dataGrid.DetailsViewManager.HasDetailsView ? 1 : 0) +
                (dataGrid.View != null ? dataGrid.View.GroupDescriptions.Count : 0);
            return ((dataGrid.showRowHeader ? 1 : 0) + indentColumnCount) + gridColumnIndex;
        }
        
        /// <summary>
        /// Resolves the group index for the specified row index when the SfDataGrid has Details View. 
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to get group index.
        /// </param>
        /// <returns>
        /// Returns the group index of the specified row index when the SfDataGrid has Details View.
        /// </returns>
        public static int ResolveToGroupRecordIndexForDetailsView(this SfDataGrid dataGrid, int rowIndex)
        {
            rowIndex = rowIndex - dataGrid.ResolveStartIndexBasedOnPosition();
            if (!dataGrid.GridModel.HasGroup)
                return -1;

            if (dataGrid.DetailsViewDefinition != null && dataGrid.DetailsViewDefinition.Count > 0)
            {
                var indexToReturn = -1;
                if (rowIndex > -1 && rowIndex < dataGrid.View.TopLevelGroup.DisplayElements.Count)
                {
                    var displayEl = dataGrid.View.TopLevelGroup.DisplayElements[rowIndex];
                    var isRecord = (displayEl is RecordEntry) && !(displayEl is NestedRecordEntry);
                    if (isRecord)
                    {
                        var record = (displayEl as RecordEntry);
                        indexToReturn = dataGrid.View.TopLevelGroup.DisplayElements.IndexOf(record);
                    }
                    else if (displayEl is NestedRecordEntry)
                    {
                        var parent = (displayEl as NestedRecordEntry).Parent as RecordEntry;
                        indexToReturn = dataGrid.View.TopLevelGroup.DisplayElements.IndexOf(parent);
                    }
                    rowIndex = indexToReturn;
                }
            }
            return rowIndex;
        }

        internal static int GetOrderForDetailsViewBasedOnIndex(this SfDataGrid dataGrid, int actualRowIdx)
        {
            // simply find the index which is not in DetailsView Index
            var counter0 = 0;
            for (int i = actualRowIdx; i > 0; i--)
            {
                if (!dataGrid.IsInDetailsViewIndex(i))
                {
                    break;
                }
                counter0++;
            }
            return counter0;
        }

        /// <summary>
        /// Determines whether the specified row index is associated with DetailsViewDataGrid row.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIdx">
        /// The corresponding row index to determine whether the row index in Details View DataGrid.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified row index is  Details View index; otherwise, <b>false</b>.
        /// </returns>
        public static bool IsInDetailsViewIndex(this SfDataGrid dataGrid, int rowIdx)
        {
            if (!dataGrid.DetailsViewManager.HasDetailsView || rowIdx < 0)
                return false;

            if (dataGrid.IsAddNewIndex(rowIdx) || dataGrid.IsFilterRowIndex(rowIdx) || dataGrid.IsUnBoundRow(rowIdx) || dataGrid.IsTableSummaryIndex(rowIdx))
                return false;

            var startIdx = dataGrid.ResolveStartIndexBasedOnPosition();
            var counter0 = Math.Max((rowIdx - startIdx), 0);

            if (dataGrid.GridModel.HasGroup)
            {
                var displayEl = dataGrid.View.TopLevelGroup.DisplayElements[counter0];
                return displayEl is NestedRecordEntry;
            }

            return (counter0 % (dataGrid.DetailsViewDefinition.Count + 1)) != 0;
        }

        /// <summary>
        /// Gets the row index of the specified DetailsViewDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="detailsViewDataGrid">
        /// The DetailsViewDataGrid to get its corresponding row index.
        /// </param>
        /// <returns>
        /// Returns the row index of the corresponding DetailsViewDataGrid.
        /// </returns>
        public static int GetGridDetailsViewRowIndex(this SfDataGrid dataGrid, DetailsViewDataGrid detailsViewDataGrid)
        {
            if (detailsViewDataGrid == null)
                return -1;

            var dataRow = dataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(
                row => row.DetailsViewDataGrid.Equals(detailsViewDataGrid));
            if (dataRow != null) return dataRow.RowIndex;
            return -1;
        }

        /// <summary>
        /// Gets the record associated with the specified DetailsViewDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="detailsViewDataGrid">
        /// Specifies the DetailsViewDataGrid to get its corresponding record entry.
        /// </param>
        /// <returns>
        /// The record associated with the specified DetailsViewDataGrid; returns null, if the DetailsViewDataGrid is null.
        /// </returns>
        public static object GetGridDetailsViewRecord(this SfDataGrid dataGrid, DetailsViewDataGrid detailsViewDataGrid)
        {
            if (detailsViewDataGrid == null || dataGrid.View == null)
                return null;

            var index = dataGrid.GetGridDetailsViewRowIndex(detailsViewDataGrid);
            if (index == -1)
                return null;

            RecordEntry record = null;
            if (dataGrid.GridModel.HasGroup)
            {
                var recordIndex = dataGrid.ResolveToGroupRecordIndexForDetailsView(index);
                record = dataGrid.View.TopLevelGroup.DisplayElements[recordIndex] as RecordEntry;
            }
            else
            {
                var recordIndex = dataGrid.ResolveToRecordIndex(index);
                record = dataGrid.View.Records[recordIndex];
            }
            return record;
        }
        
        /// <summary>
        /// Resolves the start column index of the SfDataGrid.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// Returns the start column index of the SfDataGrid.
        /// </returns>
        public static int ResolveToStartColumnIndex(this SfDataGrid dataGrid)
        {
            int startIndex = 0;
            if (dataGrid.showRowHeader)
                startIndex += 1;
            if (dataGrid.GridModel.HasGroup)
                startIndex += dataGrid.View.GroupDescriptions.Count;
            if (dataGrid.DetailsViewManager.HasDetailsView)
                startIndex += 1;
            return startIndex;
        }

        /// <summary>
        /// Gets the total number of table summary rows in SfDataGrid according to the specified <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowPosition"/>.
        /// </summary>
        /// <param name="grid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="position">
        /// Specifies the position of table summary row to get its count.
        /// </param>
        /// <returns>
        /// Returns the number of table summary rows in SfDataGrid.
        /// </returns>
        public static int GetTableSummaryCount(this SfDataGrid grid, TableSummaryRowPosition position)
        {
            //WPF-20773 avoid Designer Issue
            if (grid != null && grid.HasView && grid.TableSummaryRows != null)
                return position == TableSummaryRowPosition.Top ? (grid.View.TableSummaryRows.Where(row => (row is GridTableSummaryRow && (row as GridTableSummaryRow).Position == TableSummaryRowPosition.Top)).Count()) : (grid.View.TableSummaryRows.Count - grid.GetTableSummaryCount(TableSummaryRowPosition.Top));

            return 0;
        }
        
        /// <summary>
        /// Gets the corresponding row index of the FilterRow.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// Returns an index of a FilterRow.
        /// </returns>
        public static int GetFilterRowIndex(this SfDataGrid dataGrid)
        {
            var footerCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            var frozenCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);

            if (dataGrid.FilterRowPosition == FilterRowPosition.FixedTop)
                return dataGrid.HeaderLineCount - (1 + (dataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop ? 1 : 0));
            else if (dataGrid.FilterRowPosition == FilterRowPosition.Top)
                return dataGrid.HeaderLineCount;
            else if(dataGrid.FilterRowPosition == FilterRowPosition.Bottom)
                return dataGrid.VisualContainer.RowCount - (dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + 1 + footerCount);
            return -1;
        }

        /// <summary>
        /// Gets the corresponding row index of the AddNewRow.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// Returns an index of the AddNewRow.
        /// </returns>
        public static int GetAddNewRowIndex(this SfDataGrid dataGrid)
        {
            return dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
        }
        
        /// <summary>
        /// Gets the header index of SfDataGrid.
        /// </summary>
        /// <param name="grid">
        /// The SfDataGrid.
        /// </param>
        /// <returns>
        /// Returns the header index of the SfDataGrid.
        /// </returns>
        public static int GetHeaderIndex(this SfDataGrid grid)
        {
            var frozenCount = grid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);//, RowRegion.Header);
            int rowCount = (grid.AddNewRowPosition == AddNewRowPosition.FixedTop ? 1 : 0) + (grid.FilterRowPosition == FilterRowPosition.FixedTop ? 1 : 0);
            return (grid.HeaderLineCount - (rowCount + 1)) 
                - (grid.GetTableSummaryCount(TableSummaryRowPosition.Top)) - frozenCount;
        }
        
        /// <summary>
        /// Gets the total number of UnboundRows in SfDataGrid according to the specified <see cref="Syncfusion.UI.Xaml.Grid.UnBoundRowsPosition"/> and summary location.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="position">
        /// Specifies the position of UnBoundRows to get its count.
        /// </param>
        /// <param name="belowSummary">
        /// Specifies whether the UnBoundRow is placed above or below summary row.
        /// </param>
        /// <returns>
        /// Returns the total number of UnBoundRows in SfDataGrid.
        /// </returns>
        public static int GetUnBoundRowsCount(this SfDataGrid dataGrid, UnBoundRowsPosition position, bool belowSummary)
        {
            if (position == UnBoundRowsPosition.Top)
                return belowSummary ? dataGrid.UnBoundRows.TopBodyUnboundRowCount : dataGrid.UnBoundRows.FrozenUnboundRowCount;
            else
                return belowSummary ? dataGrid.UnBoundRows.FooterUnboundRowCount : dataGrid.UnBoundRows.BottomBodyUnboundRowCount;
        }

        /// <summary>
        /// Gets the total number of UnBoundRows in SfDataGrid according to the specified <see cref="Syncfusion.UI.Xaml.Grid.UnBoundRowsPosition"/>.
        /// </summary>
        /// <param name="grid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="position">
        /// Specifies the position of unbound row to get its count.
        /// </param>
        /// <returns>
        /// Returns the total number of UnBoundRows in SfDataGrid.
        /// </returns>
        public static int GetUnBoundRowsCount(this SfDataGrid dataGrid, UnBoundRowsPosition position)
        {
            if (position == UnBoundRowsPosition.Top)
                return dataGrid.UnBoundRows.TopBodyUnboundRowCount + dataGrid.UnBoundRows.FrozenUnboundRowCount;
            else
                return dataGrid.UnBoundRows.BottomBodyUnboundRowCount + dataGrid.UnBoundRows.FooterUnboundRowCount;
        }

        /// <summary>
        /// Gets the UnBoundRow for the specified row index.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The row index to get the UnBoundRow.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> for the specified row index.
        /// </returns>
        public static GridUnBoundRow GetUnBoundRow(this SfDataGrid dataGrid, int rowIndex)
        {
            if (!dataGrid.UnBoundRows.Any())
                return null;

            var row = dataGrid.UnBoundRows.FirstOrDefault(urow => urow.RowIndex == rowIndex);

            if (row != null)
                return row;
            else
            {
                var frozenCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);//, RowRegion.Header);
                var footerCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);//, RowRegion.Footer);
                var topBodyCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);//, RowRegion.Body);
                var bottomBodyCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);//, RowRegion.Body);

                var topTableSummaryRowsCount = dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top);
                var bottomTableSummaryRowsCount = !dataGrid.HasView ? 0 : dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom);
                var gridRows = !dataGrid.HasView ? 0 : (dataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0)
                  + (dataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0);
                var headerLineCount = (!dataGrid.HasView ? dataGrid.VisualContainer.FrozenRows : dataGrid.HeaderLineCount) + gridRows;
                var headerIndex = !dataGrid.HasView ? dataGrid.StackedHeaderRows.Count : dataGrid.GetHeaderIndex();

                var rowCount = dataGrid.VisualContainer.RowCount;
                var bottomAddNewRow = !dataGrid.HasView ? 0 : dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0;
                bottomAddNewRow += !dataGrid.HasView ? 0 : dataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0;

                if (rowIndex > headerIndex && rowIndex <= (headerIndex + frozenCount)) // FrozenCount Condition Checking
                    return dataGrid.UnBoundRows.FirstOrDefault(item => !item.ShowBelowSummary && item.Position == UnBoundRowsPosition.Top && item.UnBoundRowIndex == (rowIndex - (headerIndex + 1)));

                else if (rowIndex >= headerLineCount && rowIndex < (headerLineCount + topBodyCount)) // TopBodyCount Condition Checking                                    
                    return dataGrid.UnBoundRows.FirstOrDefault(item => item.ShowBelowSummary && item.Position == UnBoundRowsPosition.Top && item.UnBoundRowIndex == (rowIndex - headerLineCount));                

                else if (rowIndex >= (rowCount - footerCount) && rowIndex < rowCount) // Footer count condition checking.                        
                    return dataGrid.UnBoundRows.FirstOrDefault(item => item.ShowBelowSummary && item.Position == UnBoundRowsPosition.Bottom && item.UnBoundRowIndex == ((rowIndex + footerCount) - rowCount));

                else if ((rowIndex < (rowCount - (bottomTableSummaryRowsCount + footerCount + bottomAddNewRow))) && (rowIndex >= (rowCount - (bottomTableSummaryRowsCount + footerCount + bottomBodyCount + bottomAddNewRow)))) // Bootom body condition chceking                                    
                    return dataGrid.UnBoundRows.FirstOrDefault(item => !item.ShowBelowSummary && item.Position == UnBoundRowsPosition.Bottom && item.UnBoundRowIndex == ((rowIndex + bottomBodyCount + bottomAddNewRow + bottomTableSummaryRowsCount + footerCount) - rowCount));                         
            }
            return null;
        }

        /// <summary>
        /// Resolves row index for the specified <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/>.
        /// </summary>
        /// <param name="DataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="unBoundRow">
        /// Specifies the UnBoundRow to get its corresponding row index.
        /// </param>
        /// <returns>
        /// Returns the row index of the specified UnBoundRow.
        /// </returns>
        public static int ResolveUnboundRowToRowIndex(this SfDataGrid DataGrid, GridUnBoundRow unBoundRow)
        {
            if (!DataGrid.UnBoundRows.Any())
                return -1;

            if (unBoundRow.Position == UnBoundRowsPosition.Top)
            {
                //var rows = DataGrid.UnBoundRows.Where(ubr => ubr.Position == unBoundRow.Position && ubr.ShowBelowSummary == unBoundRow.ShowBelowSummary);
                if (!unBoundRow.ShowBelowSummary)
                    return DataGrid.GetHeaderIndex() + unBoundRow.UnBoundRowIndex + 1;
                else
                    return DataGrid.HeaderLineCount + (DataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0) +
                        (DataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0) + unBoundRow.UnBoundRowIndex;
            }
            else
            {
                if (!unBoundRow.ShowBelowSummary)
                {
                    return DataGrid.VisualContainer.RowCount -
                        (DataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) +
                        (DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0) +
                        (DataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0) +
                        DataGrid.GetUnBoundRowsCount(unBoundRow.Position))
                        + unBoundRow.UnBoundRowIndex;
                }
                else
                {
                    return DataGrid.VisualContainer.RowCount - DataGrid.UnBoundRows.FooterUnboundRowCount + unBoundRow.UnBoundRowIndex;
                }
            }
        }

        /// <summary>
        /// Determines whether the specified row index is associated with any UnBoundRow's.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding rowIndex to determine the UnBoundRow.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified row index is UnBoundRow; otherwise, <b>false</b>.
        /// </returns>
        public static bool IsUnBoundRow(this SfDataGrid dataGrid, int rowIndex)
        {
            if (!dataGrid.UnBoundRows.Any())
                return false;

            var topUnBoundDataRowsCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top);
            var bottomUnBoundDataRowsCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom);            
            var rowCount = dataGrid.VisualContainer.RowCount;              

            var topTableSummaryRowsCount = dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top);
            var bottomTableSummaryRowsCount = dataGrid.View == null ? 0 : dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom);

            var frozenCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);//, RowRegion.Header);
            var footerCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);//,RowRegion.Footer);
            var topBodyCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top,true);//, RowRegion.Body);
            var bottomBodyCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom,false);//, RowRegion.Body);
            var bottomAddNewRow = !dataGrid.HasView ? 0 : dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0;
            bottomAddNewRow += !dataGrid.HasView ? 0 : dataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0;
            var gridRows = !dataGrid.HasView ? 0 : (dataGrid.AddNewRowPosition == AddNewRowPosition.Top ? 1 : 0)
                  + (dataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0);
            var headerLineCount = (!dataGrid.HasView ? dataGrid.VisualContainer.FrozenRows : dataGrid.HeaderLineCount)
                + gridRows;
            var headerIndex = !dataGrid.HasView ? dataGrid.StackedHeaderRows.Count : dataGrid.GetHeaderIndex();

            if
            ((rowIndex >= headerLineCount && rowIndex < (headerLineCount + topBodyCount)) // TopBodyCount Condition Checking
            ||
            (rowIndex > headerIndex && rowIndex <= (headerIndex + frozenCount)) // FrozenCount Condition Checking
            ||
            ((rowIndex < (rowCount - (bottomTableSummaryRowsCount + footerCount + bottomAddNewRow))) && (rowIndex >= (rowCount - (bottomTableSummaryRowsCount + footerCount + bottomBodyCount + bottomAddNewRow)))) // Bootom body condition chceking
            ||
            (rowIndex >= (rowCount - footerCount) && rowIndex < rowCount)) // Footer count condition checking.
            {
                return true;
            }
            return false;    
        }

        /// <summary>
        /// Determines whether the specified row index is associated with AddNewRow.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding rowIndex to determine the AddNewRow.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified row index is AddNewRow; otherwise , <b>false</b>.
        /// </returns>
        public static bool IsAddNewIndex(this SfDataGrid dataGrid,int rowIndex)
        {
            if (dataGrid.AddNewRowPosition == AddNewRowPosition.None)
                return false;

            // If AddNewRowPosition is Bottom and View is null, current cell row index will be -1. So it returns true and crashes while creating binding
            // so below code is added to skip this
            if (dataGrid.View == null || rowIndex < 0)
                return false;
            return dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() == rowIndex;
        }

        /// <summary>
        /// Determines whether the specified column index is associated with RowHeader column.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding columnIndex to determine the RowHeader column.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified column index is RowHeader; otherwise , <b>false</b>.
        /// </returns>
        internal static bool IsRowHeaderIndex(this SfDataGrid dataGrid, int columnIndex)
        {
            return dataGrid.showRowHeader ? columnIndex == 0 : false;
        }

        /// <summary>
        /// Determines whether the specified row index is associated with FilterRow.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding rowIndex to determine the FilterRow.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified row index is FilterRow; otherwise , <b>false</b>.
        /// </returns>
        public static bool IsFilterRowIndex(this SfDataGrid dataGrid, int rowIndex)
        {
            if (rowIndex < 0)
                return false;

            return dataGrid.GetFilterRowIndex() == rowIndex;
        }

        /// <summary>
        /// Determines whether the specified row index is associated with TableSummaryRow.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding rowIndex to determine the TableSummaryRow.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the specified row index is TableSummaryRow; otherwise, <b>false</b>.
        /// </returns>
        public static bool IsTableSummaryIndex(this SfDataGrid dataGrid, int rowIndex)
        {         
             if (rowIndex < dataGrid.HeaderLineCount)
            {
                var frozenCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
                var topTableSummariesCount = dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Top);
                var tableSummaryStartIndex = dataGrid.StackedHeaderRows.Count + frozenCount + 1; //1 added for Column Header
                var tableSummaryEndIndex = dataGrid.StackedHeaderRows.Count +frozenCount + topTableSummariesCount + 1;
                if (rowIndex >= tableSummaryStartIndex && rowIndex < tableSummaryEndIndex)
                    return true;
            }
            else
            {
                var footerCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
                var tableSummaryEndIndex = dataGrid.VisualContainer.RowCount - footerCount;
                var tableSummaryStartIndex = dataGrid.VisualContainer.RowCount - (dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) +footerCount);
                if (rowIndex >= tableSummaryStartIndex && rowIndex < tableSummaryEndIndex)
                    return true;
            }            
            return false;
        }

        /// <summary>
        /// Determines whether the TableSummaryRow is at the top position of SfDataGrid for the specified row index.
        /// </summary>
        /// <param name="dataGrid">
        /// The SfDataGrid.        
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to determine the position of TableSummaryRow.        
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the TableSummaryRow placed at the top position of SfDataGrid.
        /// </returns>
        public static bool IsHeaderTableSummaryRow(this SfDataGrid dataGrid, int rowIndex)
        {
            var frozenCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
            return (rowIndex <= dataGrid.HeaderLineCount - 1 - ((dataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop ? 1 : 0)
                + (dataGrid.FilterRowPosition == FilterRowPosition.FixedTop ? 1 : 0))
                && rowIndex > dataGrid.StackedHeaderRows.Count + frozenCount);
        }

    }
}


