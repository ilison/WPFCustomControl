#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.Data.Helper;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid;
using System.Linq;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls.Primitives;
#else
using System.Windows;
using System.Windows.Media;
using System.ComponentModel.DataAnnotations;
using System.Windows.Data;
using System.Threading;
using System.Diagnostics;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridRowGenerator : ITreeGridRowGenerator, IDisposable
    {
        public SfTreeGrid Owner { get; set; }
        private List<TreeDataRowBase> _Items = new List<TreeDataRowBase>();

        public List<TreeDataRowBase> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        public TreeGridRowGenerator(SfTreeGrid owner)
        {
            this.Owner = owner;
        }

        internal TreeGridPanel TreePanel
        {
            get { return this.Owner.TreeGridPanel; }
        }

        private TreeDataRowBase CreateHeaderRow(int rowIndex, VisibleLinesCollection visibleColumns)
        {
            var dr = new TreeDataRow();
            dr.RowIndex = rowIndex;
            dr.TreeGrid = this.Owner;
            dr.RowType = TreeRowType.HeaderRow;
            dr.InitializeTreeRow(visibleColumns);
            return dr;
        }

        private TreeDataRowBase CreateDataRow(int rowIndex, VisibleLinesCollection visibleColumns)
        {
            var treeRow = new TreeDataRow();
            treeRow.RowIndex = rowIndex;
            treeRow.RowType = TreeRowType.DefaultRow;
            treeRow.TreeGrid = this.Owner;
            var node = this.Owner.GetNodeAtRowIndex(rowIndex);
            treeRow.Node = node;
            treeRow.Level = node.Level;
            treeRow.HasChildNodes = node.HasVisibleChildNodes;
            treeRow.RowData = node.Item;
            treeRow.InitializeTreeRow(visibleColumns);
            treeRow.RowElement.UpdateIndentMargin();
            return treeRow;
        }

        private void UpdateDataRow(IEnumerable<TreeDataRowBase> rows, int rowIndex, RowRegion region)
        {
            if (rows.Any(row => row.RowType == TreeRowType.DefaultRow))
            {
                var dr = rows.FirstOrDefault(r => r.RowType == TreeRowType.DefaultRow);
                var node = Owner.GetNodeAtRowIndex(rowIndex);
                if (node != null)
                {
                    dr.Node = node;
                    dr.Level = node.Level;
                    dr.HasChildNodes = node.HasVisibleChildNodes;
                    dr.RowData = node.Item;
                }
                dr.RowIndex = rowIndex;
                if (dr.RowVisibility == Visibility.Collapsed)
                    dr.RowVisibility = Visibility.Visible;
                this.CheckForSelection(dr);
                if (this.Owner.Columns.Any(col => col.GridValidationMode != GridValidationMode.None))
                {
                    this.Owner.ValidationHelper.ValidateColumns(dr);
                }
                dr.ApplyRowHeaderVisualState();
                dr.RowElement.UpdateIndentMargin();
            }
            else
            {
                var dr = CreateDataRow(rowIndex, this.TreePanel.ScrollColumns.GetVisibleLines());
                this.Items.Add(dr);
            }
        }

        /// <summary>
        /// Updates the Binding Information for the TreeDataColumn, when Editor APIs are Changed
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        internal void UpdateBinding(TreeGridColumn gridColumn)
        {
            foreach (var item in Items)
            {
                if (item.RowIndex == -1)
                    continue;

                var dataColumn = item.VisibleColumns.FirstOrDefault(datacolumn => datacolumn.TreeGridColumn == gridColumn);
                if (dataColumn != null)
                    dataColumn.UpdateBinding(item.RowData, false);
            }
        }

        private void UpdateRow(IEnumerable<TreeDataRowBase> rows, int rowIndex, RowRegion region)
        {
            if (rowIndex == Owner.GetHeaderIndex())
            {
                var headerRow = rows.FirstOrDefault(r => r.RowType == TreeRowType.HeaderRow);
                if (headerRow != null)
                {
                    headerRow.RowIndex = rowIndex;
                    headerRow.TreeGrid = this.Owner;
                    if (headerRow.RowVisibility == Visibility.Collapsed)
                        headerRow.RowVisibility = Visibility.Visible;
                }
                else
                {
                    headerRow = CreateHeaderRow(rowIndex, TreePanel.ScrollColumns.GetVisibleLines());
                    this.Items.Add(headerRow);
                }
            }
            else
                UpdateDataRow(rows, rowIndex, region);
        }

        private void CollapseRow(TreeDataRowBase row)
        {
            row.RowVisibility = Visibility.Collapsed;
        }

        IList<ITreeDataRowElement> ITreeGridRowGenerator.Items
        {
            get { return this.Items.ToList<ITreeDataRowElement>(); }
        }

        private void RemoveColumn(TreeDataRowBase row, TreeDataColumnBase datacolumn)
        {
            datacolumn.ColumnElement = null;
            row.VisibleColumns.Remove(datacolumn);
        }

        internal void UnloadUIElements(TreeDataRowBase row, TreeDataColumnBase col)
        {
            if (col.Renderer != null)
                col.Renderer.UnloadUIElements(col);
            if (col.ColumnElement is TreeGridHeaderCell)
            {
                var treeGridHeaderCell = col.ColumnElement as TreeGridHeaderCell;
                treeGridHeaderCell.Dispose();
                treeGridHeaderCell.Content = null;
            }
            else if (col.ColumnElement is TreeGridCell)
            {
                var gridCell = col.ColumnElement as TreeGridCell;
                gridCell.Dispose();
                gridCell.Content = null;
            }
                col.DataRow = null;
                col.TreeGridColumn = null;
            row.RowElement.ItemsPanel.Children.Remove(col.ColumnElement);
        }

        public void PregenerateRows(VisibleLinesCollection visibleRows, VisibleLinesCollection visibleColumns)
        {
            if (this.Items.Count != 0 || this.Owner.TreeGridPanel.RowCount <= 0) return;

            for (var i = 0; i < visibleRows.Count; i++)
            {
                var line = visibleRows[i];
                TreeDataRowBase dr = null;
                switch (line.Region)
                {
                    case ScrollAxisRegion.Header:
                        dr = CreateHeaderRow(line.LineIndex, visibleColumns);
                        break;
                    case ScrollAxisRegion.Body:
                        dr = CreateDataRow(line.LineIndex, visibleColumns);
                        break;
                }
                if (dr != null)
                    this.Items.Add(dr);
            }
        }

        /// <summary>
        /// Method to ensure or update the row associated properties like RowIndex, RowData, row state and its selection  while scrolling and Data Manipulation Operation based on VisibleRows.
        /// </summary>
        /// <param name="visibleRows"></param>
        public virtual void EnsureRows(VisibleLinesCollection visibleRows)
        {
            if (visibleRows.Count > 0)
            {
                var ActualStartIndex = 0;
                var ActualEndIndex = 0;
                // Initially will set IsEnsured to false. and create will set again to true in following case.
                this.Items.ForEach(row => { row.IsEnsured = false; });

                var region = RowRegion.Header;
                for (int i = 0; i < 2; i++)
                {
                    // Below condition make sure the Header of the rows. will include Frozen rows, Table summaries at top, AddNewRow at Top and Headers.
                    if (i == 0)
                    {
                        if (visibleRows.firstBodyVisibleIndex > 0)
                        {
                            ActualStartIndex = 0;
                            ActualEndIndex = visibleRows[visibleRows.FirstBodyVisibleIndex - 1].LineIndex;
                        }
                        else
                        {
                            ActualStartIndex = 0;
                            ActualEndIndex = -1;
                        }
                        region = RowRegion.Header;
                    }
                    // Below will make sure the start and end rows. which includes only datarows.
                    else if (i == 1)
                    {
                        if (visibleRows.FirstBodyVisibleIndex <= 0 && visibleRows.LastBodyVisibleIndex < 0)
                            continue;
                        if (visibleRows.Count > visibleRows.FirstBodyVisibleIndex)
                            ActualStartIndex = visibleRows[visibleRows.FirstBodyVisibleIndex].LineIndex;
                        else
                            continue;
                        ActualEndIndex = visibleRows[visibleRows.LastBodyVisibleIndex].LineIndex;
                        region = RowRegion.Body;
                    }

                    for (int index = ActualStartIndex; index <= ActualEndIndex; index++)
                    {
                        if (this.Items.All(row => row.RowIndex != index))
                        {
                            if (this.Items.Any(row => (row.RowIndex < 0 || row.RowIndex < ActualStartIndex || row.RowIndex > ActualEndIndex) && !row.IsEnsured))
                            {
                                // we wont reuse rows that was current row, it was in editing
                                IEnumerable<TreeDataRowBase> rows;
                                if (this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= 0 && this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == index)
                                {
                                    rows = this.Items.Where(row => row.IsCurrentRow);
                                    if (!rows.Any())
                                    {
                                        // will get rows to reuse based on some conditions , doing with key navigation(Enter/Down/Up to bring single row from unview row to view.).
                                        rows = this.Items.Where(row => ((row.RowIndex < 0 || row.RowIndex < ActualStartIndex ||
                                              row.RowIndex > ActualEndIndex) && !row.IsEnsured && !row.IsEditing) && !row.IsCurrentRow);
                                    }
                                }
                                else
                                {
                                    //will get rows to reuse based on some conditions , called while scroll vertically - bulk rows has been taken for reuse.
                                    rows = this.Items.Where(row => ((row.RowIndex < 0 || row.RowIndex < ActualStartIndex ||
                                              row.RowIndex > ActualEndIndex) && !row.IsEnsured && !row.IsEditing) && !row.IsCurrentRow);
                                }
                                if (rows != null && rows.Any())
                                {
                                    //Call for reusing rows taken from above codes.
                                    UpdateRow(rows, index, region);
                                    var updatedRow = this.Items.FirstOrDefault(row => row.RowIndex == index);
                                    if (updatedRow is TreeDataRow)
                                    {
                                        (updatedRow as TreeDataRow).UpdateExpanderCell();
                                    }
                                }
                            }
                        }
                        var dr = this.Items.FirstOrDefault(row => row.RowIndex == index);

                        if (dr != null)
                        {
                            if (dr.RowVisibility == Visibility.Collapsed)
                            {
                                dr.RowVisibility = Visibility.Visible;
                            }
                        }
                        else
                        {
                            // Code for adding rows at run time. 
                            if (region == RowRegion.Header)
                                dr = CreateHeaderRow(index, this.TreePanel.ScrollColumns.GetVisibleLines());
                            else
                                dr = CreateDataRow(index, this.TreePanel.ScrollColumns.GetVisibleLines());
                            this.Items.Add(dr);
                        }
                        dr.RowElement.UpdateIndentMargin();
                        if (dr.updateExpander)
                        {
                            (dr as TreeDataRow).UpdateExpanderCell();
                            dr.updateExpander = false;
                        }
                        if (dr.IsCurrentRow)
                        {
                            // selection need to check moving left to right , 
                            this.CheckForSelection(dr);
                            dr.IsCurrentRow = false;
                            (dr as TreeDataRowBase).ApplyRowHeaderVisualState();
                        }
                        if (!dr.IsSelectedRow)
                        {
                            this.CheckForSelection(dr);
                            dr.IsCurrentRow = false;
                            (dr as TreeDataRowBase).ApplyRowHeaderVisualState();
                        }
                        dr.IsEnsured = true;

                        //when scroll vertically, the new row can be created with new curerntcell for that row. where we need to set the currentcell using this call.
                        if (dr.RowIndex == this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex && (this.Owner.NavigationMode == NavigationMode.Cell || (this.Owner.NavigationMode == NavigationMode.Cell && this.Owner.SelectionMode == GridSelectionMode.Multiple && this.Owner.CurrentItem != null)))
                        {
                            dr.UpdateCurrentCellSelection();
                        }
                        dr.IsEnsured = true;
                    }
                }
            }
            if (this.Owner.SelectionController.CurrentCellManager.HasCurrentCell || this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= this.Owner.GetHeaderIndex())
            {
                // while reusing rows, we need to update the row header cell if that row is a current row.
                var currentRow = this.Items.FirstOrDefault(row => row.RowIndex == this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                if (currentRow != null)
                {
                    currentRow.IsCurrentRow = true;
                    (currentRow as TreeDataRowBase).ApplyRowHeaderVisualState();
                }
            }
            this.Items.ForEach(row => { if (!row.IsEnsured) { CollapseRow(row); } });
        }


        public void ResetSelection()
        {
            this.Items.ForEach(item =>
            {
                item.IsSelectedRow = false;
                item.IsFocusedRow = false;
                item.IsCurrentRow = false;
            });
        }

        /// <summary>
        /// Method which will ensure whether the row is selected or not.
        /// </summary>
        /// <param name="rowIndex">Corresponding Row Index</param>
        /// <returns>Whether row is selected or not</returns>
        /// <remarks></remarks>
        private void CheckForSelection(TreeDataRowBase row)
        {
            if (this.Owner.SelectionMode == GridSelectionMode.None || row.RowType == TreeRowType.HeaderRow)
                return;

            row.IsSelectedRow = this.Owner.SelectionController.SelectedRows.Contains(row.RowIndex);

            if (this.Owner.SelectionMode == GridSelectionMode.Multiple && this.Owner.NavigationMode == NavigationMode.Row)
            {
                if (!row.IsSelectedRow && this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == row.RowIndex)
                    row.IsFocusedRow = true;
                else
                    row.IsFocusedRow = false;
            }
        }

        public void EnsureColumns(VisibleLinesCollection visibleColumns)
        {
            foreach (var gridRow in this.Items)
            {
                gridRow.EnsureColumns(visibleColumns);
            }
        }

        //WPF_33924 Header Rows are not arranged properly while setting ShowRowHeader using binding. 
        //So the header row is refreshed here.
        internal void RefreshHeaders()
        {
            var headerRow = this.Items.FirstOrDefault(row => row.RowType == TreeRowType.HeaderRow);
            if (headerRow != null)
                headerRow.RowElement.ItemsPanel.InvalidateMeasure();
        }

        internal void UpdateRowIndentMargin()
        {
            foreach (var gridRow in this.Items.Where(r => r.IsEnsured))
            {
                gridRow.RowElement.UpdateIndentMargin();
            }
        }
        public void ColumnHiddenChanged(HiddenRangeChangedEventArgs args)
        {
            // Merged from SfDataGrid 
            // No need to change the visibility here, as the visibility of column gets changed in EnsureColumn of DataRow.
            //NeedToRefreshColumn set to true, in OnScrollColumnsChanged method of VisualContainer.

            // if the cell is in edit mode and hides the column, after unhiding focus should be in text box.

            //this.Items.ForEach(row => row.VisibleColumns.ForEach(column =>
            //{
            //    if (column.ColumnIndex >= args.From && column.ColumnIndex <= args.To)
            //    {
            //        column.ColumnVisibility = args.Hide ? Visibility.Collapsed : Visibility.Visible;
            //    }
            //}));
        }

        public void RowHiddenChanged(HiddenRangeChangedEventArgs args)
        {
            this.Items.ForEach(row =>
            {
                if (row.RowIndex >= args.From && row.RowIndex <= args.To)
                {
                    row.RowVisibility = args.Hide ? Visibility.Collapsed : Visibility.Visible;
                }
            });
        }

        public void ColumnInserted(int index, int count)
        {
            if (this.Items.Count == 0)
                return;

            var expanderColumnIndex = Owner.expanderColumnIndex;
            var needToResetExpanderColumn = (expanderColumnIndex >= index && expanderColumnIndex < (index + count));
            this.Items.ForEach(row =>
            {
                row.VisibleColumns.ForEach(col =>
                {
                    if (index <= col.ColumnIndex)
                    {
                        col.ColumnIndex += count;
                    }
                });
                // need to reset expander column index for all data rows.
                if (needToResetExpanderColumn && row.RowType == TreeRowType.DefaultRow)
                {
                    var oldExpanderColumn = row.VisibleColumns.FirstOrDefault(c => c.ColumnType == TreeColumnType.ExpanderColumn);
                    if (oldExpanderColumn != null)
                    {
                        // UWP-3376 - If expander cell is current cell, to reuse the expander cell, we need to remove current cell. Current cell will be again set from Ensure rows.
                        if (oldExpanderColumn.IsCurrentCell)
                            Owner.SelectionController.CurrentCellManager.RemoveCurrentCell(new RowColumnIndex(row.RowIndex, oldExpanderColumn.ColumnIndex));
                        oldExpanderColumn.ColumnIndex = -1;
                    }
                }
            });
        }

        public void ColumnRemoved(int index, int count)
        {
            var endIndex = index + count - 1;
            var isExpanderColumnValid = Owner.IsExpanderColumnValid();

            this.Items.ForEach(row =>
            {
                for (int i = index; i <= endIndex; i++)
                {
                    var datacolumn = row.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == i);
                    if (datacolumn != null)
                    {
                        datacolumn.ColumnIndex = -1;
                        //UnloadUIElements(row, datacolumn);
                        //RemoveColumn(row, datacolumn);
                    }
                }

                row.VisibleColumns.ForEach(x =>
                {
                    if (endIndex < x.ColumnIndex)
                    {
                        x.ColumnIndex = x.ColumnIndex - count;
                    }
                });

                // need to reset expander column index for all data rows.
                if (!isExpanderColumnValid)
                {
                    if (row.RowType == TreeRowType.DefaultRow && row.VisibleColumns.Any())
                    {
                        //Below condition commented as expander column re-used now. In the first if, Column index set to -1 without removing the DataColumn.
                        //if (!row.VisibleColumns.Any(col => col.ColumnType == TreeColumnType.ExpanderColumn))
                        //{
                        var columnIndex = Owner.expanderColumnIndex;
                        // WPF-34222 - Issue 2 - Expander column current cell is removed unnecessarily here. To fix this, column type check added here.
                        // We should reset column index and remove current cell of the column which is going to be a expander column.
                        var column = row.VisibleColumns.FirstOrDefault(c => c.ColumnIndex == columnIndex && c.ColumnType != TreeColumnType.ExpanderColumn);
                        if (column != null)
                        {
                            column.ColumnIndex = -1;
                            // UWP-3376 - If expander cell is current cell, to reuse the expander cell, we need to remove current cell. Current cell will be again set from Ensure rows.
                            if (column.IsCurrentCell)
                            {
                                Owner.SelectionController.CurrentCellManager.RemoveCurrentCell(new RowColumnIndex(column.RowIndex, column.ColumnIndex));
                            }
                        }
                    }
                }
            });
        }

        internal void OnItemSourceChanged()
        {
            if (!this.Items.Any())
                return;

            foreach (var item in Items)
                item.Dispose();
            this.Items.Clear();
            foreach (var cellRenderer in this.Owner.CellRenderers.Values)
            {
                var renderer = cellRenderer as ITreeGridCellRenderer;
                if (renderer != null)
                    renderer.ClearRecycleBin();
            }
        }

        public void ApplyColumnSizerOnInitial(double availableWidth)
        {
            this.Owner.TreeGridColumnSizer.InitialRefresh(availableWidth);
        }


        public void RowsArranged()
        {
            if (this.Owner.ColumnDragDropController != null)
                this.Owner.ColumnDragDropController.UpdatePopupPosition();
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowGenerator"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowGenerator"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (this.Items != null)
            {
                foreach (var item in Items)
                    item.Dispose();
                this.Items.Clear();
                this.Items = null;
            }
            this.Owner = null;
        }
    }
}

