#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeDataRow : TreeDataRowBase
    {
        protected override void OnGenerateVisibleColumns(VisibleLinesCollection visibleColumnLines)
        {
            this.VisibleColumns.Clear();
            for (int i = 0; i < 2; i++)
            {
                int StartColumnIndex = 0;
                int EndColumnIndex = 0;
                if (i == 0)
                {
                    if (visibleColumnLines.FirstBodyVisibleIndex <= 0)
                        continue;
                    StartColumnIndex = 0;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex - 1].LineIndex;
                }
                else if (i == 1)
                {
                    if (visibleColumnLines.FirstBodyVisibleIndex <= 0 && visibleColumnLines.LastBodyVisibleIndex < 0)
                        continue;
                    if (visibleColumnLines.Count > visibleColumnLines.firstBodyVisibleIndex)
                        StartColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex;
                    else
                        continue;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;
                }
                for (int index = StartColumnIndex; index <= EndColumnIndex; index++)
                {
                    if (TreeGrid.showRowHeader && index == 0)
                    {
                        if (!this.VisibleColumns.Any(col => col.ColumnIndex == index))
                            CreateRowHeaderColumn(index);
                        continue;
                    }
                    var dc = CreateColumn(index);
                    this.VisibleColumns.Add(dc);
                }
            }
        }

        internal override void EnsureColumns(VisibleLinesCollection visibleColumnLines)
        {
            // Initially all the columns will be IsEnsured false. we need to create the column to be view and that will be ensuered.
            this.VisibleColumns.ForEach(column => column.IsEnsured = false);

            var needToUpdateCurrentCell = this.TreeGrid.SelectionMode != GridSelectionMode.None && this.RowIndex != -1 && this.TreeGrid.NavigationMode == NavigationMode.Cell && !this.TreeGrid.SelectionController.CurrentCellManager.HasCurrentCell && this.RowIndex == this.TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex;

            //StartBodyColumnIndex - Which will make sure the actual column index.
            var StartBodyColumnIndex = (visibleColumnLines.firstBodyVisibleIndex < visibleColumnLines.Count) ? visibleColumnLines[visibleColumnLines.firstBodyVisibleIndex].LineIndex : visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;
            for (int i = 0; i < 2; i++)
            {
                int StartColumnIndex = 0;
                int EndColumnIndex = 0;
                if (i == 0)
                {
                    if (visibleColumnLines.FirstBodyVisibleIndex <= 0)
                        continue;
                    StartColumnIndex = 0;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex - 1].LineIndex;
                }
                // Below will make sure the start and end column index of row. which includes only data column.
                else if (i == 1)
                {
                    if (visibleColumnLines.FirstBodyVisibleIndex <= 0 && visibleColumnLines.LastBodyVisibleIndex < 0)
                        continue;
                    if (visibleColumnLines.Count > visibleColumnLines.firstBodyVisibleIndex)
                        StartColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex;
                    else
                        continue;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;
                }

                for (int index = StartColumnIndex; index <= EndColumnIndex; index++)
                {
                    if (visibleColumnLines.All(row => row.LineIndex != index))
                        continue;
                    if (TreeGrid.showRowHeader && index == 0)
                    {
                        // Reuse the row header column by checking ColumnType instead of ColumnIndex (since we reset column index while removing row header column if ShowRowHeader is False).
                        var rhc = this.VisibleColumns.FirstOrDefault(column => column.ColumnType == TreeColumnType.RowHeader);
                        if (rhc != null)
                        {
                            if (rhc.ColumnVisibility == Visibility.Collapsed)
                                rhc.ColumnVisibility = Visibility.Visible;
                            rhc.IsEnsured = true;
                            // To reuse the column, need to set the column index here.
                            rhc.ColumnIndex = 0;
                        }
                        else
                            CreateRowHeaderColumn(index);
                        continue;
                    }

                    if (this.VisibleColumns.All(column => column.ColumnIndex != index || this.isDirty))
                    {
                        var hasExpanderColumn = this.VisibleColumns.Any(c => c.ColumnType == TreeColumnType.ExpanderColumn);
                        var isExpanderColumn = IsExpanderColumn(index);
                        // ColumnIndex needs to be checked with StartBodyColumnIndex and EndColumnIndex - Due to avoid reusing freezed columns from header and footer.
                        TreeDataColumnBase dataColumn;
                        if (hasExpanderColumn && isExpanderColumn)
                        {
                            dataColumn = this.VisibleColumns.FirstOrDefault(
                            column => ((column.ColumnIndex < StartBodyColumnIndex || column.ColumnIndex > EndColumnIndex || this.isDirty) &&
                                        !column.IsEnsured && !column.IsEditing && (!column.IsCurrentCell || this.isDirty) && column.ColumnType == TreeColumnType.ExpanderColumn));
                        }
                        else
                            dataColumn = this.VisibleColumns.FirstOrDefault(
                            column => ((column.ColumnIndex < StartBodyColumnIndex || column.ColumnIndex > EndColumnIndex || this.isDirty) &&
                                        !column.IsEnsured && column.ColumnType != TreeColumnType.RowHeader && !column.IsEditing && (!column.IsCurrentCell || this.isDirty)
                                        && (column.ColumnType != TreeColumnType.ExpanderColumn || (column.ColumnType == TreeColumnType.ExpanderColumn && isExpanderColumn))));

                        if (dataColumn != null && (!isExpanderColumn || dataColumn.ColumnType == TreeColumnType.ExpanderColumn))
                        {
                            // which will reuse the column element fully, or load its element with different one.
                            UpdateColumn(dataColumn, index);
                        }
                    }

                    var dc = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                    if (dc != null)
                    {
                        if (dc.ColumnVisibility == Visibility.Collapsed)
                        {
                            dc.ColumnVisibility = Visibility.Visible;
                            dc.UpdateCellStyle();
                            if (dc.Renderer != null && (dc.Renderer.HasCurrentCellState && (dc.IsEditing || dc.TreeGridColumn.CanFocus())))
                                dc.Renderer.SetFocus(true);
                        }
                        if (needToUpdateCurrentCell)
                            this.UpdateCurrentCellSelection(dc);
                        dc.IsEnsured = true;
                    }
                    else
                    {
                        if (index >= this.TreeGrid.TreeGridPanel.ColumnCount)
                            continue;
                        var datacolumn = CreateColumn(index);

                        if (needToUpdateCurrentCell)
                            this.UpdateCurrentCellSelection(datacolumn);
                        datacolumn.IsEnsured = true;
                        this.VisibleColumns.Add(datacolumn);
                    }
                }
            }
            this.VisibleColumns.ForEach(column =>
            {
                if (!column.IsEnsured)
                {
                    CollapseColumn(column);
                }
            });
            this.isDirty = false;
            Panel panel = this.RowElement.ItemsPanel;

            if (panel != null && this.TreeGrid.IsLoaded)
                panel.InvalidateMeasure();
        }

        internal bool IsExpanderColumn(int index)
        {
            if (this.RowType == TreeRowType.HeaderRow)
                return false;         
            if (TreeGrid.expanderColumnIndex == index)
                return true;
            return false;
        }
        internal override void UpdateRowStyles(ContentControl row)
        {
            if (row != null && this.RowType != TreeRowType.HeaderRow)
            {
                this.ApplyRowStyles(row);
            }
        }

        internal virtual void ApplyRowStyles(ContentControl row)
        {
            if (TreeGrid == null || row == null)
                return;
            Style newStyle = null;
            var hasRowStyleSelector = TreeGrid.hasRowStyleSelector;
            var hasRowStyle = TreeGrid.hasRowStyle;

            if (!hasRowStyle && !hasRowStyleSelector)
            {
                if (row.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
                    row.ClearValue(FrameworkElement.StyleProperty);
                return;
            }

            if (hasRowStyleSelector && hasRowStyle)
            {
                newStyle = TreeGrid.RowStyleSelector.SelectStyle(this, row);
                newStyle = newStyle ?? TreeGrid.RowStyle;
            }
            else if (hasRowStyleSelector)
            {
                newStyle = TreeGrid.RowStyleSelector.SelectStyle(this, row);
            }
            else if (hasRowStyle)
            {
                newStyle = TreeGrid.RowStyle;
            }
            row.Style = newStyle;
        }

        internal override TreeDataColumnBase CreateColumn(int index)
        {
            var dc = new TreeDataColumn();
            dc.DataRow = this;
            dc.ColumnIndex = index;
            var columnIndex = this.TreeGrid.ResolveToGridVisibleColumnIndex(index);
            dc.TreeGridColumn = this.TreeGrid.Columns[columnIndex];
            if (this.RowIndex < this.TreeGrid.HeaderLineCount && this.RowIndex >= 0)
            {
                dc.Renderer = this.TreeGrid.CellRenderers["Header"];
                this.RowData = this.TreeGrid.Columns[columnIndex];
            }
            else
                dc.Renderer = dc.TreeGridColumn.CellType != string.Empty
                               ? this.TreeGrid.CellRenderers[dc.TreeGridColumn.CellType]
                               : this.TreeGrid.CellRenderers["Static"];

            if (this.RowType == TreeRowType.DefaultRow)
            {
                var expanderColumnIndex = TreeGrid.expanderColumnIndex;
                if (expanderColumnIndex == index)
                    dc.ColumnType = TreeColumnType.ExpanderColumn;
            }
            else if (this.RowType == TreeRowType.HeaderRow)
                dc.ColumnType = TreeColumnType.ColumnHeader;

            dc.InitializeColumnElement(this.RowData, false);
            if (dc.ColumnType == TreeColumnType.ExpanderColumn)
            {
                var expanderCell = dc.ColumnElement as TreeGridExpanderCell;
                UpdateExpanderCellProperties(expanderCell);
            }
            SetCellBindings(dc);
            if (dc.TreeGridColumn.GridValidationMode != GridValidationMode.None)
            {
                if (this.RowIndex >= this.TreeGrid.headerLineCount)
                {
                    this.TreeGrid.ValidationHelper.ValidateColumn(this.RowData, dc.TreeGridColumn.MappingName, dc.ColumnElement as TreeGridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
            return dc;
        }

        internal virtual void UpdateColumn(TreeDataColumnBase dc, int index)
        {
            if (index < 0 || index >= this.TreeGrid.TreeGridPanel.ColumnCount)
            {
                dc.ColumnVisibility = Visibility.Collapsed;
            }
            else
            {
                dc.ColumnIndex = index;
                var gridCell = dc.ColumnElement as TreeGridCell;
                if (gridCell != null && gridCell.HasError)
                    gridCell.RemoveAll();
                var currentColumn = this.TreeGrid.Columns[this.TreeGrid.ResolveToGridVisibleColumnIndex(index)];

                bool isElementUnloaded = this.UpdateRenderer(dc, currentColumn);
                dc.TreeGridColumn = currentColumn;

                if (isElementUnloaded)
                {
                    if (dc.ColumnElement != null)
                    {
                        dc.ColumnElement.ClearValue(FrameworkElement.DataContextProperty);
                        if (dc.ColumnVisibility == Visibility.Collapsed)
                            dc.ColumnVisibility = Visibility.Visible;
                    }
                    dc.InitializeColumnElement(this.RowData, dc.IsEditing);
                    dc.UpdateCellStyle();
                }
                else
                {
                    if (dc.ColumnVisibility == Visibility.Collapsed)
                        dc.ColumnVisibility = Visibility.Visible;
                }
                if (dc.ColumnType == TreeColumnType.ExpanderColumn)
                {
                    var expanderCell = dc.ColumnElement as TreeGridExpanderCell;
                    UpdateExpanderCellProperties(expanderCell);
                }
                dc.UpdateBinding(this.RowData);

                if (dc.TreeGridColumn.GridValidationMode != GridValidationMode.None)
                {
                    if (this.RowType == TreeRowType.DefaultRow)
                        this.TreeGrid.ValidationHelper.ValidateColumn(this.RowData, dc.TreeGridColumn.MappingName, dc.ColumnElement as TreeGridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
        }

        private void UpdateCurrentCellSelection(TreeDataColumnBase column)
        {
            if (this.TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == column.ColumnIndex)
            {
                column.IsCurrentCell = true;
                if (TreeGrid.SelectionController.CurrentCellManager.CurrentCell == null)
                    TreeGrid.SelectionController.CurrentCellManager.SetCurrentColumnBase(column, true);
            }
            else
            {
                column.IsCurrentCell = false;
            }
        }

        /// <summary>
        /// Update Renderer and UnloadUIElement if needed
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal virtual bool UpdateRenderer(TreeDataColumnBase dataColumn, TreeGridColumn column)
        {
            ITreeGridCellRenderer newRenderer = null;
            var update = (dataColumn.TreeGridColumn.hasCellTemplateSelector || dataColumn.TreeGridColumn.hasCellTemplate) || (column.hasCellTemplate || column.hasCellTemplateSelector);

            if (this.TreeGrid.GetHeaderIndex() == dataColumn.RowIndex)
            {
                newRenderer = this.TreeGrid.CellRenderers["Header"];
                update = dataColumn.TreeGridColumn.hasHeaderTemplate || column.hasHeaderTemplate;
            }
            else
            {
                newRenderer = column.CellType != string.Empty
                                  ? this.TreeGrid.CellRenderers[column.CellType]
                                  : this.TreeGrid.CellRenderers["Static"];
            }

            if (dataColumn.Renderer == null)
                return false;

            //If both are different renderer then we will unload UIElements.
            //The column going to reuse and the column which uses the existing column when has CellTemplates
            // Existing Column  -   New Column     -   Action
            //  CellTemplate    -   CellTemplate    -   Unload
            //  CellTemplate    -   None            -   Unload
            //  None            -   CellTemplate    -   Unload
            //  None            -   None            -   Reuse
            // DataHeader will have same renderer always 
            if (dataColumn.Renderer != newRenderer)
            {
                dataColumn.Renderer.UnloadUIElements(dataColumn);
                dataColumn.Renderer = newRenderer;
                return true;
            }
            if (update)
            {
                dataColumn.Renderer.UnloadUIElements(dataColumn);
                return true;
            }
            return false;
        }
        internal override void UpdateCurrentCellSelection()
        {
            if (this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            if (TreeGrid.SelectionController.CurrentCellManager.CurrentCell == null || TreeGrid.SelectionController.CurrentCellManager.CurrentCell.IsCurrentCell)
            {
                var dataColumn =
                    this.VisibleColumns.FirstOrDefault(
                        item => item.ColumnIndex == this.TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                if (dataColumn != null)
                {
                    if (!dataColumn.IsCurrentCell)
                    {
                        dataColumn.IsCurrentCell = true;
                        TreeGrid.SelectionController.CurrentCellManager.SetCurrentColumnBase(dataColumn, true);
                    }
                }
            }
        }

        internal void UpdateExpanderCell()
        {
            if (this.RowType != TreeRowType.DefaultRow)
                return;
            this.HasChildNodes = Node.HasVisibleChildNodes;
            this.Level = Node.Level;
            var dc = this.VisibleColumns.FirstOrDefault(column => column.ColumnType == TreeColumnType.ExpanderColumn);
            if (dc == null)
                return;
            var expanderCell = dc.ColumnElement as TreeGridExpanderCell;
            if (expanderCell == null) return;
            UpdateExpanderCellProperties(expanderCell);
        }

        internal void UpdateExpanderCellProperties(TreeGridExpanderCell expanderCell)
        {
            expanderCell.SuspendChangedAction = true;
            expanderCell.IsExpanded = Node.IsExpanded;
            expanderCell.SuspendChangedAction = false;
        }
    }
}

