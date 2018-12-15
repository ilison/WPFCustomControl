#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Syncfusion.Data.Extensions;
#if UWP
using Windows.UI.Core;
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
#endif
    public class TreeGridModel : IDisposable
    {
        internal SfTreeGrid treeGrid;
        /// <summary>
        /// Represents a wrapper class for SfTreeGrid control to handle the collection and view related operations.
        /// </summary>
        /// <remarks>
        /// TreeGridModel class listens to the collection changes in a SfTreeGrid control and responds to them.
        /// It updates the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.View"/> in response to the collection changes.
        /// </remarks> 
        public TreeGridModel(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
        }

        private TreeGridView View
        {
            get { return this.treeGrid.View; }
        }

        internal void ExpandNode(TreeNode node, int rowIndex, bool reExpand = false)
        {
            if (node == null)
                return;

            if (!reExpand)
                CommitCurrentRow();
            //After committing, corresponding node's row index will be changes. so recalculating row index.
            rowIndex = this.treeGrid.ResolveToRowIndex(node);
            if (!reExpand)
            {
                if (node.IsExpanded)
                    return;
                var expandingArgs = this.treeGrid.RaiseNodeExpanding(new NodeExpandingEventArgs() { Node = node });
                if (expandingArgs.Cancel)
                {
                    ChangeExpanderState(rowIndex, false);
                    return;
                }
                else
                {
                    // While expanding node programmatically, need to change expander cell's Expanded state.
                    ChangeExpanderState(rowIndex, true);
                }
            }

            if (reExpand)
            {
                node.SetDirtyOnExpandOrCollapse();
                UpdateExpander(node);
            }
            var itemsCount = View.ExpandNode(node, reExpand);
            if (!suspend && rowIndex != -1 && node.IsExpanded)
                InsertRows(rowIndex, itemsCount);
            if (!reExpand)
                this.treeGrid.RaiseNodeExpanded(new NodeExpandedEventArgs() { Node = node });
            if (!suspend && treeGrid.isGridLoaded)
            {
                treeGrid.TreeGridPanel.UpdateScrollBars();
                treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
            }
        }

        internal void UpdateRows(TreeNode node, int oldItemscount)
        {
            var rowIndex = treeGrid.ResolveToRowIndex(node);
            var insertedItems = node.GetYAmountCache() - 1 - oldItemscount;
            InsertRows(rowIndex, insertedItems);
            treeGrid.TreeGridPanel.UpdateScrollBars();
            treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
        }

        internal void RefreshDataRowOnExpand(int fromRowIndex)
        {
            this.treeGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowIndex > fromRowIndex)
                    ResetRowIndex(row);
            });
            this.treeGrid.TreeGridPanel.InvalidateMeasureInfo();
        }
        internal void ExpandNode(TreeNode node, bool reExpand)
        {
            var rowIndex = this.treeGrid.ResolveToRowIndex(node);
            ExpandNode(node, rowIndex, reExpand);
        }

        private void ResetRowIndex(TreeDataRowBase dr)
        {
            dr.RowIndex = -1;
        }

        /// <summary>
        /// Change expander cell's state based on node's expander state.
        /// </summary>
        /// <param name="rowIndex">rowIndex.</param>
        /// <param name="isExpanded">Expanded state of the node.</param>
        private void ChangeExpanderState(int rowIndex, bool isExpanded)
        {
            var dr = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
            if (dr == null) return;
            var dc = dr.VisibleColumns.FirstOrDefault(column => column.ColumnType == TreeColumnType.ExpanderColumn);
            if (dc == null) return;
            var expander = dc.ColumnElement as TreeGridExpanderCell;
            if (expander == null) return;
            expander.SuspendChangedAction = true;
            if (expander.IsExpanded != isExpanded)
            {
                expander.IsExpanded = isExpanded;
                // Set TreeGridExpander's IsExpanded while canceling expand/collapse operation in event (UWP - 1905).
                expander.SetExpanderState();
            }
            expander.SuspendChangedAction = false;
        }
        internal void CollapseNode(TreeNode node, int rowIndex)
        {
            if (node == null || !node.IsExpanded)
                return;
            CommitCurrentRow();
            // After committing, corresponding node's row index will be changed. so recalculating row index.
            rowIndex = this.treeGrid.ResolveToRowIndex(node);
            if (!this.treeGrid.RaiseNodeCollapsing(new NodeCollapsingEventArgs() { Node = node }))
            {
                ChangeExpanderState(rowIndex, true);
                return;
            }
            else
                // While collapsing node programmatically, need to change expander cell's Expanded state.
                ChangeExpanderState(rowIndex, false);
            int count = View.CollapseNode(node);
            if (rowIndex != -1)
                RemoveRows(rowIndex, count);
            this.treeGrid.RaiseNodeCollapsed(new NodeCollapsedEventArgs() { Node = node });
            if (treeGrid.TreeGridPanel != null)
                treeGrid.TreeGridPanel.UpdateScrollBars();
            treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
        }

        internal void ExpandAllNodes(bool updateRowCount = true)
        {
            CommitCurrentRow();
            foreach (var node in treeGrid.View.Nodes.RootNodes)
            {
                ExpandAllNodes(node, false);
            }
            if (updateRowCount)
                UpdateRowCountOnExpandAll();
        }


        internal void CollapseAllNodes()
        {
            CommitCurrentRow();
            foreach (var node in treeGrid.View.Nodes.RootNodes)
            {
                CollapseAllNodes(node, false);
            }
            treeGrid.UpdateRowCount();
            treeGrid.TreeGridPanel.ScrollRows.MarkDirty();
            RefreshDataRows();
            treeGrid.TreeGridPanel.UpdateScrollBars();
            (treeGrid.SelectionController as TreeGridBaseSelectionController).HandleNodeOperations(NodeOperation.NodesCollapsed);
            treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
        }

        // While expanding all rows, no need to insert rows separately. So insertRows is passed as False. can update row count once.
        internal void ExpandAllNodes(TreeNode node, bool insertRows, int level = -1)
        {
            if (node == null)
                return;
            int rowIndex = -1;
            // UWP-5276 - no need to calculate row index for all nodes, while expanding all the nodes.
            if (insertRows)
            {
                CommitCurrentRow();
                rowIndex = this.treeGrid.ResolveToRowIndex(node);
            }
            var oldItemsCount = node.GetYAmountCache() - 1;
            ChangeExpanderState(rowIndex, true);
            int count = View.ExpandAllNodes(node, level);
            if (insertRows && rowIndex != -1)
                InsertRows(rowIndex, count - oldItemsCount);
        }

        // When SelfRelationalUpdateBehavior is used, need to suspend inserting rows since multiple nodes expanded. After resuming, Row count will be updated finally.
        internal bool suspend = false;
        internal void InsertRows(int rowIndex, int count)
        {
            var insertedItems = count;
            var startIndex = rowIndex + 1;
            var lineSizeCollection = treeGrid.TreeGridPanel.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            treeGrid.TreeGridPanel.InsertRows(startIndex, insertedItems);
            treeGrid.SelectionController.HandleNodeExpandCollapse(startIndex, insertedItems, true);
            lineSizeCollection.ResumeUpdates();
            treeGrid.TreeGridPanel.ScrollRows.MarkDirty();
            RefreshDataRowOnExpand(rowIndex);
        }

        internal void ExpandAllNodes(int level, bool updateRowCount = true)
        {
            if (level < 0)
                return;
            CommitCurrentRow();
            foreach (var node in treeGrid.View.Nodes.RootNodes)
            {
                ExpandAllNodes(node, false, level);
            }
            if (updateRowCount)
                UpdateRowCountOnExpandAll();
        }

        private void UpdateRowCountOnExpandAll()
        {
            treeGrid.UpdateRowCount();
            treeGrid.TreeGridPanel.ScrollRows.MarkDirty();
            RefreshDataRows();
            treeGrid.TreeGridPanel.UpdateScrollBars();
            (treeGrid.SelectionController as TreeGridBaseSelectionController).HandleNodeOperations(NodeOperation.NodesExpanded);
            treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
        }

        internal void CollapseAllNodes(TreeNode node, bool removeRows)
        {
            if (node == null || !node.IsExpanded)
                return;
            int rowIndex = -1;
            // UWP-5276 - no need to calculate row index for all nodes, while collpasing all the nodes.
            if (removeRows)
            {
                CommitCurrentRow();
                rowIndex = this.treeGrid.ResolveToRowIndex(node);
            }
            ChangeExpanderState(rowIndex, false);
            int count = View.CollapseAllNodes(node);
            if (removeRows && rowIndex != -1)
                RemoveRows(rowIndex, count);
        }

        internal void BringUpdatedItemIntoView(TreeNode node, string propertyName)
        {
            var column = this.treeGrid.Columns.FirstOrDefault(c => c.MappingName == propertyName);
            if (column != null)
            {
                var scrollColumnIndex = treeGrid.ResolveToScrollColumnIndex(treeGrid.Columns.IndexOf(column));
                var rowIndex = treeGrid.ResolveToRowIndex(node.Item);
                treeGrid.ScrollInView(new RowColumnIndex(rowIndex, scrollColumnIndex));
            }
        }
        internal void RemoveRows(int rowIndex, int count)
        {
            var startIndex = rowIndex + 1;
            var lineSizeCollection = treeGrid.TreeGridPanel.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            treeGrid.TreeGridPanel.RemoveRows(startIndex, count);
            treeGrid.SelectionController.HandleNodeExpandCollapse(startIndex, count, false);
            lineSizeCollection.ResumeUpdates();
            treeGrid.TreeGridPanel.ScrollRows.MarkDirty();
            RefreshDataRowOnExpand(rowIndex);
        }

        /// <summary>
        /// Commit the Current editing row 
        /// </summary>       
        internal void CommitCurrentRow()
        {
            if (treeGrid == null)
                return;

            var cmanager = treeGrid.SelectionController.CurrentCellManager;
            if (!cmanager.HasCurrentCell)
                return;
            cmanager.EndEdit();
            treeGrid.SelectionController.ResetSelectionHandled();
        }

        internal void MakeSort(TreeGridColumn column)
        {
            if (this.View == null)
                return;
            if (column.MappingName == null)
            {
                throw new InvalidOperationException("Mapping Name is necessary for Sorting");
            }
            if (!this.treeGrid.CheckColumnNameinItemProperties(column))
                return;
            CommitCurrentRow();
            var sortColumName = column.MappingName;
            var cancelScroll = false;
            var allowMultiSort = SelectionHelper.CheckControlKeyPressed();
            IsInSort = true;
            if (treeGrid.SortColumnDescriptions.Any() && allowMultiSort)
            {
                var sortedColumn = this.treeGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                if (sortedColumn == default(SortColumnDescription))
                {
                    var newSortColumn = new SortColumnDescription { ColumnName = sortColumName, SortDirection = ListSortDirection.Ascending };
                    if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>() { newSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add, out cancelScroll))
                    {
                        this.treeGrid.SortColumnDescriptions.Add(newSortColumn);
                        this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>() { newSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add);
                    }
                }
                else
                {
                    if (sortedColumn.SortDirection == ListSortDirection.Descending && this.treeGrid.AllowTriStateSorting)
                    {
                        var removedSortColumn = this.treeGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                        if (removedSortColumn != null)
                        {
                            if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>(), new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Remove, out cancelScroll))
                            {
                                this.treeGrid.SortColumnDescriptions.Remove(removedSortColumn);
                                this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>(), new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Remove);
                            }
                        }
                    }
                    else
                    {
                        sortedColumn.SortDirection = sortedColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        var removedSortColumn = this.treeGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortedColumn.ColumnName);
                        if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription> { sortedColumn }, new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Replace, out cancelScroll))
                        {
                            this.treeGrid.SortColumnDescriptions.Remove(removedSortColumn);
                            this.treeGrid.SortColumnDescriptions.Add(sortedColumn);
                            this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription> { sortedColumn }, new List<SortColumnDescription>() { removedSortColumn }, NotifyCollectionChangedAction.Replace);
                        }
                    }
                }
            }
            else
            {
                var currentSortColumn = this.treeGrid.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == sortColumName);
                if (currentSortColumn != default(SortColumnDescription))
                {
                    if (currentSortColumn.SortDirection == ListSortDirection.Descending && this.treeGrid.AllowTriStateSorting)
                    {
                        var sortColumnsClone = this.treeGrid.SortColumnDescriptions.ToList();
                        if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>(), sortColumnsClone, NotifyCollectionChangedAction.Remove, out cancelScroll))
                        {
                            this.treeGrid.SortColumnDescriptions.Clear();
                            this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>(), sortColumnsClone, NotifyCollectionChangedAction.Remove);
                        }
                    }
                    else
                    {
                        currentSortColumn.SortDirection = currentSortColumn.SortDirection == ListSortDirection.Ascending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                        if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace, out cancelScroll))
                        {
                            this.treeGrid.SortColumnDescriptions.Clear();
                            this.treeGrid.SortColumnDescriptions.Add(currentSortColumn);
                            this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>() { currentSortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Replace);
                        }
                    }
                }
                else
                {
                    var sortColumn = new SortColumnDescription()
                    {
                        ColumnName = sortColumName,
                        SortDirection = ListSortDirection.Ascending
                    };

                    if (this.treeGrid.SortColumnDescriptions.Any())
                    {
                        var sortColumnsClone = this.treeGrid.SortColumnDescriptions.ToList();
                        if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, sortColumnsClone, NotifyCollectionChangedAction.Add, out cancelScroll))
                        {
                            this.treeGrid.SortColumnDescriptions.Clear();
                            this.treeGrid.SortColumnDescriptions.Add(sortColumn);
                            this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, sortColumnsClone, NotifyCollectionChangedAction.Add);
                        }
                    }
                    else
                    {
                        if (this.treeGrid.RaiseSortColumnsChanging(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add, out cancelScroll))
                        {
                            this.treeGrid.SortColumnDescriptions.Add(sortColumn);
                            this.treeGrid.RaiseSortColumnsChanged(new List<SortColumnDescription>() { sortColumn }, new List<SortColumnDescription>(), NotifyCollectionChangedAction.Add);
                        }
                    }
                }
            }
            RefreshAfterSorting(cancelScroll);
            IsInSort = false;
        }

        internal void RefreshAfterSorting(bool cancelScroll)
        {
            if (View == null)
                return;
            this.treeGrid.SelectionController.SuspendUpdates();
            View.RefreshSorting();
            this.treeGrid.SelectionController.ResumeUpdates();
            if (!View.IsInDeferRefresh)
            {
                this.treeGrid.SelectionController.HandleGridOperations(new TreeGridOperationsHandlerArgs(TreeGridOperation.Sorting,
            new SortColumnChangedHandle()
            {
                ScrollToCurrentItem = !cancelScroll
            }));
            }
            this.UpdateHeaderCells();
        }

        /// <summary>
        /// Refreshes the DataRows in view when the grid operations is performed.
        /// </summary>
        public void RefreshDataRows()
        {
            if (this.treeGrid.TreeGridPanel == null)
                return;
            this.treeGrid.RowGenerator.Items.ForEach((row) =>
            {
                if (row.RowType != TreeRowType.HeaderRow)
                    ResetRowIndex(row);
            });
            this.treeGrid.TreeGridPanel.InvalidateMeasure();
        }
        internal void RefreshView()
        {
            if (this.View == null || this.treeGrid.TreeGridPanel == null)
                return;
            this.RefreshDataRows();
            this.treeGrid.UpdateRowCount();
            this.treeGrid.TreeGridPanel.UpdateScrollBars();
        }
        internal void UpdateDataRow(int index)
        {
            if (this.treeGrid.TreeGridPanel == null || index == -1)
                return;

            var dataRow = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == index);
            if (dataRow != null)
            {
                ResetRowIndex(dataRow);
                this.treeGrid.TreeGridPanel.InvalidateMeasureInfo();
            }
        }

        internal bool IsInSort { get; set; }

        internal void OnSortColumnsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (this.treeGrid == null || this.treeGrid.View == null || IsInSort) return;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (SortColumnDescription newItem in e.NewItems)
                    {
                        if (this.treeGrid.SortColumnDescriptions.Count(col => col.ColumnName == newItem.ColumnName) > 1)
                        {
                            throw new InvalidOperationException("SortColumnDescription already exist in SfTreeGrid.SortColumnDescriptions");
                        }
                    }
                    break;
            }
            RefreshAfterSorting(false);
        }

        internal void CollapseSortNumber()
        {
            var items = this.treeGrid.RowGenerator.Items;
            if (items == null) return;
            var headerIndex = 0;
            foreach (var visibleColumn in items[headerIndex].VisibleColumns)
            {
                var headerCell = visibleColumn.ColumnElement as TreeGridHeaderCell;
                if (headerCell != null)
                    headerCell.SortNumberVisibility = Visibility.Collapsed;
            }
        }

        internal void ShowSortNumbers()
        {
            var items = this.treeGrid.RowGenerator.Items;
            if (!items.Any()) return;
            var sortNumber = 1;
            var headerIndex = 0;
            foreach (var item in treeGrid.SortColumnDescriptions)
            {
                var sortColumn = items[headerIndex].VisibleColumns.FirstOrDefault(x => x.TreeGridColumn != null && x.TreeGridColumn.MappingName == item.ColumnName);
                if (sortColumn != null)
                {
                    var headerCell = sortColumn.ColumnElement as TreeGridHeaderCell;
                    if (headerCell != null)
                    {
                        headerCell.SortNumber = sortNumber.ToString();
                        headerCell.SortNumberVisibility = Visibility.Visible;
                    }
                }
                sortNumber += 1;
            }
        }

        internal void UpdateHeaderCells()
        {
            var headerRow = this.treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowType == TreeRowType.HeaderRow);
            if (headerRow == null)
                return;
            foreach (var column in headerRow.VisibleColumns)
            {
                var headerCell = column.ColumnElement as TreeGridHeaderCell;
                if (headerCell != null)
                    headerCell.Update();
            }
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridModel"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridModel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnwireEvents();
                this.treeGrid = null;
            }
        }
        internal void WireEvents()
        {
            if (View != null)
            {
                this.View.NodeCollectionChanged += OnNodeCollectionChanged;
                View.SourceCollectionChanged += OnSourceCollectionChanged;
                View.CurrentChanged += OnViewCurrentChanged;
                View.RecordPropertyChanged += OnRecordPropertyChanged;
            }
            if (treeGrid != null)
            {
                if (this.treeGrid.SortColumnDescriptions != null)
                    this.treeGrid.SortColumnDescriptions.CollectionChanged += OnSortColumnsChanged;
            }
        }

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.ResetEditItem(e);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        var addedrowindex = this.treeGrid.ResolveToRowIndex(e.NewStartingIndex);
                        var count = e.NewItems.Count;
                        if (this.treeGrid.RowGenerator.Items.Any(row => row.RowIndex == addedrowindex))
                            UpdateDataRow(addedrowindex, count, e.Action);
                        else
                            RefreshView(addedrowindex, count, e.Action);
                        break;
                    }
                case NotifyCollectionChangedAction.Remove:
                    {
                        var rowindex = this.treeGrid.ResolveToRowIndex(e.OldStartingIndex);
                        var count = e.OldItems.Count;
                        if (this.treeGrid.RowGenerator.Items.Any(row => row.RowIndex == rowindex))
                            UpdateDataRow(rowindex, count, e.Action);
                        else
                            RefreshView(rowindex, count, e.Action);
                        break;
                    }

                case NotifyCollectionChangedAction.Reset:
                    {
                        RefreshView();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var node = e.OldItems[0] as TreeNode;
                        // rows need to be removed  , we are adding 1 item. so no need to remove the parent node
                        var count = node.GetYAmountCache() - 1;
                        var startIndex = this.treeGrid.ResolveToRowIndex(e.NewStartingIndex);
                        var endIndex = this.treeGrid.ResolveToRowIndex(count + startIndex - 1);
                        for (int i = startIndex; i <= endIndex; i++)
                        {
                            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == i);
                            if (dataRow != null)
                                dataRow.RowIndex = -1;
                        }

                        var rowItems = this.treeGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex > endIndex).OrderBy(item => item.RowIndex);
                        rowItems.ForEach(row =>
                        {
                            row.RowIndex -= count;
                        });

                        treeGrid.TreeGridPanel.RemoveRows(startIndex, count);
                        treeGrid.TreeGridPanel.UpdateScrollBars();
                        treeGrid.TreeGridPanel.InvalidateMeasure();
                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    {
                        var node = e.NewItems[0] as TreeNode;
                        var count = node.GetYAmountCache();
                        var oldStartIndex = this.treeGrid.ResolveToRowIndex(e.OldStartingIndex);
                        var oldEndIndex = this.treeGrid.ResolveToRowIndex(count + oldStartIndex - 2);

                        var newStartIndex = this.treeGrid.ResolveToRowIndex(e.NewStartingIndex);
                        var newEndIndex = this.treeGrid.ResolveToRowIndex(count + newStartIndex - 2);
                        for (int i = oldStartIndex; i <= oldEndIndex; i++)
                        {
                            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == i);
                            if (dataRow != null)
                                dataRow.RowIndex = -1;
                        }
                        var rowItems = this.treeGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex > oldEndIndex).OrderBy(item => item.RowIndex);
                        rowItems.ForEach(row =>
                        {
                            row.RowIndex -= count;
                        });

                        for (int i = newStartIndex; i <= newEndIndex; i++)
                        {
                            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == i);
                            if (dataRow != null)
                                dataRow.RowIndex = -1;
                        }

                        var rowItems1 = this.treeGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex > newEndIndex).OrderBy(item => item.RowIndex);
                        rowItems1.ForEach(row =>
                        {
                            row.RowIndex += count;
                        });

                        treeGrid.TreeGridPanel.InvalidateMeasure();
                    }
                    break;
            }
            this.treeGrid.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.NodeCollectionChanged);
            treeGrid.TreeGridPanel.UpdateScrollBars();
            this.treeGrid.TreeGridColumnSizer.ChangeExpanderColumnWidth();
        }

        private void OnSourceCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            this.ResetEditItem(args);
            this.treeGrid.SelectionController.HandleCollectionChanged(args, TreeGridCollectionChangedReason.SourceCollectionChanged);
        }
        private void ResetEditItem(NotifyCollectionChangedEventArgs args)
        {
            if (this.View.IsEditingItem)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove)
                {
                    if (args.OldItems.Contains(View.CurrentEditItem))
                        this.EndEdit(true);
                }
                else if (args.Action == NotifyCollectionChangedAction.Move)
                {
                    if (args.NewItems[0] == View.CurrentEditItem)
                        this.EndEdit();
                }
                else if (args.Action == NotifyCollectionChangedAction.Replace)
                {
                    if (args.NewItems.Contains(View.CurrentEditItem))
                        this.EndEdit();
                }
                else if (args.Action == NotifyCollectionChangedAction.Reset)
                    this.EndEdit();
            }
        }
        internal void EndEdit(bool canResetBinding = false)
        {
            var collectionview = this.View as TreeGridView;

            if (collectionview == null || !collectionview.IsEditingItem) return;
            var datarow = this.treeGrid.RowGenerator.Items.FirstOrDefault(x => x.IsEditing && x.RowData.Equals(View.CurrentEditItem));

            if (datarow != null)
            {
                var column = datarow.VisibleColumns.FirstOrDefault(col => col.IsEditing);
                if (column != null && column.Renderer != null && column.Renderer.IsEditable)
                {
                    column.Renderer.EndEdit(column, datarow.RowData, canResetBinding);
                    column.IsEditing = false;
                }
                datarow.IsEditing = false;
            }
            collectionview.EndEdit();
        }

        /// <summary>
        /// Method which helps to Synchronize the CurrentItem between view and TreeGrid.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
#if WPF
        void OnViewCurrentChanged(object sender, EventArgs e)
#else
        void OnViewCurrentChanged(object sender, object e)
#endif
        {
            if (this.View.CurrentItem != null)
            {
                var node = this.View.CurrentItem as TreeNode;
                if (node != null)
                    this.treeGrid.CurrentItem = node.Item;
                else
                    this.treeGrid.CurrentItem = null;
            }
            else
                this.treeGrid.CurrentItem = null;
        }


        private void RefreshView(int rowIndex, int count, NotifyCollectionChangedAction action, int recordIndex = 0, int recordCount = 0)
        {
            if (this.View == null || this.treeGrid.TreeGridPanel == null)
                return;

            if (action == NotifyCollectionChangedAction.Add)
                this.treeGrid.TreeGridPanel.InsertRows(rowIndex, count);
            else if (action == NotifyCollectionChangedAction.Remove)
                this.treeGrid.TreeGridPanel.RemoveRows(rowIndex, count);
            else
                throw new NotImplementedException("RefreshView not implemented for" + action.ToString());
            this.RefreshDataRows();
            this.treeGrid.TreeGridPanel.UpdateScrollBars();
        }

        internal void UpdateExpander(TreeNode treeNode)
        {
            var row = treeGrid.RowGenerator.Items.FirstOrDefault(r => r.Node == treeNode);
            if (row != null)
                row.updateExpander = true;
        }

        internal void UpdateParentNode(TreeNode node)
        {
            var rowIndex = treeGrid.ResolveToRowIndex(node);
            UpdateDataRow(rowIndex);
        }
        private void UpdateDataRow(int index, int count, NotifyCollectionChangedAction action)
        {
            if (this.treeGrid.TreeGridPanel == null)
                return;

            var datarow = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == index);
            var level = count;

            if (datarow != null)
            {
                if (action == NotifyCollectionChangedAction.Add)
                {
                    var rowItems = this.treeGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex >= index).OrderBy(item => item.RowIndex);
                    rowItems.ForEach(row =>
                    {
                        row.SuspendUpdateStyle();
                        if (row.RowType != TreeRowType.HeaderRow)
                            row.RowIndex += level;
                        row.ResumeUpdateStyle();
                    });
                }
                else if (action == NotifyCollectionChangedAction.Remove)
                {
                    datarow.RowIndex = -1;
                    var rowItems = this.treeGrid.RowGenerator.Items.Where(rowInfo => rowInfo.RowIndex >= (index + count)).OrderBy(item => item.RowIndex);
                    var removedItems = this.treeGrid.RowGenerator.Items.Where(rowInfo => ((rowInfo.RowIndex < (index + count)) && rowInfo.RowIndex >= index));
                    removedItems.ForEach(ResetRowIndex);
                    rowItems.ForEach(row =>
                    {
                        row.SuspendUpdateStyle();
                        if (row.RowType != TreeRowType.HeaderRow)
                            row.RowIndex -= level;
                        row.ResumeUpdateStyle();
                    });
                }
                else
                    throw new NotImplementedException("UpdateDataRow is not implemented for" + action.ToString());
            }
            UpdateView(index, count, action);
        }

        private void UpdateView(int rowIndex, int count, NotifyCollectionChangedAction action)
        {
            if (this.View == null || this.treeGrid.TreeGridPanel == null)
                return;
            if (action == NotifyCollectionChangedAction.Add)
                this.treeGrid.TreeGridPanel.InsertRows(rowIndex, count);
            else if (action == NotifyCollectionChangedAction.Remove)
                this.treeGrid.TreeGridPanel.RemoveRows(rowIndex, count);
            else
                throw new NotImplementedException("UpdateView not implemented for" + action.ToString());
            this.treeGrid.TreeGridPanel.UpdateScrollBars();
            this.treeGrid.TreeGridPanel.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Method which helps to update the TableSummary Values when  change the Record Property Change
        /// </summary>
        /// <param name="e">An <see cref="T:System.ComponentModel.PropertyChangedEventArgs">PropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dataRowBase = this.treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowData == sender);

            if (dataRowBase != null && this.treeGrid.Columns.Any(col => col.GridValidationMode != GridValidationMode.None))
            {
                var dc = dataRowBase.VisibleColumns.FirstOrDefault(x => x.TreeGridColumn != null && x.TreeGridColumn.MappingName == e.PropertyName);
                //WPF-25806 - For template column alone validation will be handled here. for other columns validation will be handled in ValidationHelper.
                if (dc != null && (!dc.IsEditing || dc.TreeGridColumn.IsTemplate))
                {
                    (dataRowBase as TreeDataRow).ApplyRowHeaderVisualState();
                    this.treeGrid.ValidationHelper.ValidateColumn(dataRowBase.RowData, e.PropertyName, dc.ColumnElement as TreeGridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
        }
        internal void UnwireEvents()
        {
            if (this.View != null)
            {
                this.View.NodeCollectionChanged -= OnNodeCollectionChanged;
                View.SourceCollectionChanged -= OnSourceCollectionChanged;
                View.CurrentChanged -= OnViewCurrentChanged;
                View.RecordPropertyChanged -= OnRecordPropertyChanged;
            }
            if (treeGrid != null)
            {
                if (this.treeGrid.SortColumnDescriptions != null)
                    this.treeGrid.SortColumnDescriptions.CollectionChanged -= OnSortColumnsChanged;
            }
        }
    }

}
