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
using Syncfusion.UI.Xaml.Grid.RowFilter;
using System.ComponentModel.DataAnnotations;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class DataRow : GridDataRow
    {
        internal int GroupRecordIndex { get; set; }

        #region Ctor

        public DataRow()
        {

        }

        #endregion

        #region override methods
        
        protected override VirtualizingCellsControl OnCreateRowElement()
        {
            if (this.RowType == RowType.AddNewRow)
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<AddNewRowControl>();
                row.DataContext = this.RowData;
                row.Visibility = this.RowVisibility;
                row.GetVisibleLineOrigin = GetVisibleLineOrigin;
                row.GetRowGenerator = () => this.DataGrid.RowGenerator;
                row.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(row);
                return row;
            }
            else if(this.RowType == RowType.UnBoundRow)
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<UnBoundRowControl>();
                row.DataContext = this.RowData;
                row.Visibility = this.RowVisibility;
                UpdateRowStyles(row);
                row.GetVisibleLineOrigin = GetVisibleLineOrigin;
                row.AllowRowHoverHighlighting = AllowRowHoverHighlighting;
                row.GetRowGenerator = () => this.DataGrid.RowGenerator;
                row.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(row);
                return row;                
            }

            else if (this.RowType == RowType.FilterRow)
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<FilterRowControl>();
                row.DataContext = this.RowData;
                row.Visibility = this.RowVisibility;
                row.GetVisibleLineOrigin = GetVisibleLineOrigin;
                row.GetRowGenerator = () => this.DataGrid.RowGenerator;
                row.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(row);
                return row;
            }
            else if (this.RowType == Grid.RowType.HeaderRow)
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<HeaderRowControl>();
                row.Visibility = this.RowVisibility;
                row.InitializeVirtualizingRowControl(GetDataRow);
                return row;
            }
            else if (this.RowRegion == RowRegion.Footer && (this.RowType  == RowType.TableSummaryRow || this.RowType == RowType.TableSummaryCoveredRow))
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<TableSummaryRowControl>();
                row.DataContext = this.RowData;
                row.Visibility = this.RowVisibility;
                UpdateRowStyles(row);
                row.InitializeVirtualizingRowControl(GetDataRow);
                return row;
            }
            else
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<VirtualizingCellsControl>();
#if !WPF
                row.DataContext = this.RowData;
#endif
                row.Visibility = this.RowVisibility;
                
#if WPF
                // IsAsync is property is not set, so the binding is created again in OnRowDataChanged.
                var isAsync = this.DataGrid.ScrollMode == ScrollMode.Async;
                var dataContextBinding = new Binding() { Path = new PropertyPath("RowData"), Source = this, IsAsync = isAsync, NotifyOnTargetUpdated = isAsync };
                row.SetBinding(ContentControl.DataContextProperty, dataContextBinding);

                if (isAsync)
                    row.OnCreateScrollAnimation();
#endif
                UpdateRowStyles(row);
                row.GetVisibleLineOrigin = GetVisibleLineOrigin;
                row.AllowRowHoverHighlighting = AllowRowHoverHighlighting;
                row.GetRowGenerator = () => this.DataGrid.RowGenerator;
                row.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(row);
                return row;
            }
        }

#if WPF
        protected override void OnRowDataChanged()        
        {
            if (this.WholeRowElement == null)
                return;

            if (this.RowType != RowType.DefaultRow)
            {
                base.OnRowDataChanged();
                return;
            }

            if (DataGrid.useDrawing)
                this.WholeRowElement.ItemsPanel.InvalidateVisual();

            var bindingExpression = this.WholeRowElement.GetBindingExpression(ContentControl.DataContextProperty); 
            var rowBinding = bindingExpression != null ? bindingExpression.ParentBinding : null;            
            var isAsync = this.DataGrid.ScrollMode == ScrollMode.Async;
            
            if (rowBinding == null || rowBinding.IsAsync != isAsync)
            {
                if (this.WholeRowElement.ScrollAnimation == null)
                    this.WholeRowElement.OnCreateScrollAnimation();

                var dataContextBinding = new Binding() { Path = new PropertyPath("RowData"), Source = this, IsAsync = isAsync, NotifyOnTargetUpdated = isAsync };
                this.WholeRowElement.SetBinding(ContentControl.DataContextProperty, dataContextBinding);
            }            
            foreach (var dataColumn in this.VisibleColumns.Where(column => column.Renderer != null && column.Renderer.CanUpdateBinding(column.GridColumn)))
            {
                dataColumn.UpdateBinding(this.RowData, false);
            }  
        }
#endif

        protected internal override double GetColumnSize(int index, bool lineNull)
        {
            if(!lineNull && this.IsSpannedRow)
            {               
                var dataColumnBase = this.VisibleColumns.FirstOrDefault(item=> item.ColumnIndex == index);                
                var columnIndex = dataColumnBase.ColumnIndex;
                var coveredCellInfo = this.DataGrid.CoveredCells.GetCoveredCellInfo(dataColumnBase);
                if (coveredCellInfo != null)
                {
                    this.DataGrid.CoveredCells.ContainsRow(coveredCellInfo);

                    if (coveredCellInfo.Contains(dataColumnBase.RowIndex, dataColumnBase.ColumnIndex))
                    {
                        this.DataGrid.CoveredCells.ContainsColumn(this, coveredCellInfo);
                        return this.GetMergedCellColumnSize(coveredCellInfo.Left, coveredCellInfo.Right);                                                                            
                    }
                }                
            }
            return base.GetColumnSize(index, lineNull);
        }

        protected internal override double GetRowSize(DataColumnBase dataColumn, bool lineNull)
        {
            var height = 0.0;
            var columnIndex = dataColumn.ColumnIndex;
            var rowIndex = dataColumn.RowIndex;
            var coveredCellInfo = this.DataGrid.CoveredCells.GetCoveredCellInfo(dataColumn);
            if (this.IsSpannedRow && !lineNull && coveredCellInfo != null)
            {
                if (coveredCellInfo.Contains(rowIndex, columnIndex))
                {
                    this.DataGrid.CoveredCells.ContainsRow(coveredCellInfo);                    
                    height = this.GetMergedCellRowSize(coveredCellInfo.Top, coveredCellInfo.Bottom);                                            
                }
                if(this.DataGrid is DetailsViewDataGrid)
                {
                    if (height != this.DataGrid.RowHeight * (coveredCellInfo.Height))
                        height = this.DataGrid.RowHeight * (coveredCellInfo.Height);
                }
                return height;
            }
            return base.GetRowSize(dataColumn, lineNull);
        }

        internal virtual DataColumnBase EnsuringLastRow(int index)
        {
            var indexers = this.DataGrid.ResolveToRecordIndex(RowIndex + 1);

            var detailsViewCount = (!(this.IsExpanded) && this.DataGrid.DetailsViewManager.HasDetailsView) ? this.DataGrid.DetailsViewDefinition.Count : 0;

            var isLastRow = this.DataGrid.RowGenerator.IsLastRow(indexers + detailsViewCount);
            var isLastGroupRow = this.DataGrid.View.TopLevelGroup.DisplayElements[indexers + detailsViewCount] is Group;
            var indentColumn = CreateIndentColumn(index);
            indentColumn.IsEnsured = true;
            int lastGroupLevel = -1;
            //RecordEntry is retrieved to get the parent group of the particular row.
            var recordEntry = this.RowType != Grid.RowType.HeaderRow ? this.DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.ResolveToRecordIndex(this.RowIndex)] as RecordEntry : null;
            if (!isLastGroupRow)
                isLastGroupRow = this.DataGrid.View.TopLevelGroup.DisplayElements.Count == indexers + detailsViewCount ? true : false;

            //The below code is used to check whether the current row is in last row of the TopLevelGroup.
            if (isLastGroupRow && recordEntry != null)
                isLastRow = this.DataGrid.RowGenerator.IsLastRow(recordEntry, this.RowIndex, ref lastGroupLevel);

            if (isLastRow || this.RowType == RowType.AddNewRow)
                indentColumn.IndentColumnType = IndentColumnType.InLastGroupRow;
            else if (isLastGroupRow && recordEntry != null)
            {
                var group = recordEntry.Parent as Group;
                //To check whether the group is the lastr group of the TopLevelGroup.
                bool isLastGroup = (group.Parent != null && group.Parent is Group) ? group.Equals((group.Parent as Group).Groups.LastOrDefault()) : false;
                this.DataGrid.RowGenerator.CheckIsLastRow(indentColumn, lastGroupLevel, group, isLastRow, isLastGroup);
            }
            else
                indentColumn.IndentColumnType = IndentColumnType.InDataRow;
            return indentColumn;
        }

        protected override void OnGenerateVisibleColumns(VisibleLinesCollection visibleColumnLines)
        {
            this.VisibleColumns.Clear();
            var canIncrementHeight = this.RowRegion == RowRegion.Header && this.DataGrid.StackedHeaderRows != null &&
                        this.DataGrid.StackedHeaderRows.Count > 0 && this.RowType == RowType.HeaderRow;
            for (int i = 0; i < 3; i++)
            {
                int StartColumnIndex;
                int EndColumnIndex;
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
                else
                {
                    if (visibleColumnLines.FirstFooterVisibleIndex >= visibleColumnLines.Count)
                        continue;
                    StartColumnIndex = visibleColumnLines[visibleColumnLines.FirstFooterVisibleIndex].LineIndex;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.Count - 1].LineIndex;
                }

                //生成原本datagrid表头
                for (int index = StartColumnIndex; index <= EndColumnIndex; index++)
                {
                    if (DataGrid.showRowHeader && index == 0)
                    {
                        if (!this.VisibleColumns.Any(col => col.ColumnIndex == index))
                        {
                            CreateRowHeaderColumn(index);
                        }
                        continue;
                    }
                    else if (this.DataGrid.View != null && ((index < this.DataGrid.View.GroupDescriptions.Count) || (index <= this.DataGrid.View.GroupDescriptions.Count && DataGrid.showRowHeader)))
                    {
                        if (!this.VisibleColumns.Any(col => col.ColumnIndex == index))
                        {
                            this.VisibleColumns.Add(EnsuringLastRow(index));
                        }
                        continue;
                    }

                    var expanderindex = (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count + 1 : 1) + (DataGrid.showRowHeader ? 1 : 0);
                    if (this.DataGrid.DetailsViewManager.HasDetailsView && (index < expanderindex))
                    {
                        var expanderColumn = CreateDetailsViewExpanderColumn(index);
                        VisibleColumns.Add(expanderColumn);
                        continue;
                    }

                    int heightIncrementation = 0;
                    if (canIncrementHeight)                    
                        heightIncrementation = this.DataGrid.GetHeightIncrementationLimit(new CoveredCellInfo(index, index, 0, 0), this.RowIndex - 1);                    
                    var dc = CreateColumn(index, heightIncrementation);
                    var columnIndex =this.DataGrid.ResolveToGridVisibleColumnIndex(index);
                    if (this.DataGrid.Columns[columnIndex].IsHidden)
                        dc.ColumnVisibility = Visibility.Collapsed;
                    this.VisibleColumns.Add(dc);
                    //While vertical scrolling the columns are created with normal cell border hence this code is added.
                    //WPF-20188 - Border thickness for before FooterColumnCount is increased, because the BeforeFooterColumnCell is applied based on the previous column index,  
                    //the previous column is calculated based on the VisibleColumns of DataGird, so the VisibleColumn is added before UpdateFixedColumnState
                    this.UpdateFixedColumnState(dc);
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the VisibleColumn can be update or not.
        /// </summary>
        /// <param name="dataColumn">Corresponding datacolumn</param>
        /// <returns> true when Column can update for reusing otherwise false. </returns>
        protected virtual bool CanUpdateColumn(DataColumnBase dataColumn)
        {
            return this.isDirty;
        }

        internal override void EnsureColumns(VisibleLinesCollection visibleColumnLines)
        {
            // Initially all the columns will be IsEnsured false. we need to create the column to be view and that will be ensured.
            this.VisibleColumns.ForEach(column => column.IsEnsured = false);
            //StartBodyColumnIndex - Which will make sure the actual column index.
            var StartBodyColumnIndex = (visibleColumnLines.firstBodyVisibleIndex < visibleColumnLines.Count) ? visibleColumnLines[visibleColumnLines.firstBodyVisibleIndex].LineIndex : visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;             
            var needToUpdateCurrentCell = (this.RowIndex != -1 && (this.DataGrid.NavigationMode == NavigationMode.Cell || this.DataGrid.IsAddNewIndex(this.RowIndex) || this.DataGrid.IsFilterRowIndex(this.RowIndex)) 
                            && ((!this.DataGrid.SelectionController.CurrentCellManager.HasCurrentCell && this.RowIndex == this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)));
            var canIncrementHeight = this.RowType == Grid.RowType.HeaderRow && this.DataGrid.StackedHeaderRows.Count > 0;

            for (int i = 0; i < 3; i++)
            {
                int StartColumnIndex;
                int EndColumnIndex;
                // Below condition make sure the Header of the row. will include Frozen columns also.
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

                    this.DataGrid.CoveredCells.RemoveColumnRange(StartColumnIndex, EndColumnIndex);
                }
                // which make sure the footer of the grid. which includes footer columns
                else
                {
                    if (visibleColumnLines.FirstFooterVisibleIndex >= visibleColumnLines.Count)
                        continue;
                    StartColumnIndex = visibleColumnLines[visibleColumnLines.FirstFooterVisibleIndex].LineIndex;
                    EndColumnIndex = visibleColumnLines[visibleColumnLines.Count - 1].LineIndex;
                }

                for (int index = StartColumnIndex; index <= EndColumnIndex; index++)
                {
                    if (visibleColumnLines.All(row => row.LineIndex != index))
                        continue;

                    if (DataGrid.showRowHeader && index == 0)
                    {
                        var rhc = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                        if (rhc != null)
                        {
                            // when we change ShowRowHeader at run time, will remove from VisibleColumns and create it again.No Need to check Visibility state here.
                            if (rhc.ColumnVisibility == Visibility.Collapsed) 
                                rhc.ColumnVisibility = Visibility.Visible;
                            rhc.IsEnsured = true;
                        }
                        else
                            CreateRowHeaderColumn(index);
                        continue;
                    }
                    else if (this.DataGrid.View != null && ((index < this.DataGrid.View.GroupDescriptions.Count) || (index <= this.DataGrid.View.GroupDescriptions.Count && DataGrid.showRowHeader)))
                    {
                        if (!this.VisibleColumns.Any(col => col.ColumnIndex == index))
                        {
                            // When we have grouping with RowHeader/without row header. the indent type has been set by here and state is applied correctly through this.
                            this.VisibleColumns.Add(EnsuringLastRow(index));
                        }
                        var ic = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                        if (ic != null)
                        {
                            //while scrolling we have to make it visible, since we collapse it when it is in not in view.
                            if (ic.ColumnVisibility == Visibility.Collapsed)
                                ic.ColumnVisibility = Visibility.Visible;
                            ic.IsEnsured = true; // the column thats in visible ,needs to be ensured.
                        }
                        continue;
                    }

                    if (this.DataGrid.DetailsViewManager.HasDetailsView && (index < (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0) + 1 + (DataGrid.showRowHeader ? 1 : 0)))
                    {
                        DataColumnBase expanderColumn;
                        if (this.RowType == RowType.DefaultRow)
                            expanderColumn = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index && column.IsExpanderColumn);
                        else
                            expanderColumn = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index && column.IsIndentColumn);

                        if (expanderColumn == null)
                        {
                            expanderColumn = CreateDetailsViewExpanderColumn(index);
                            expanderColumn.IsEnsured = true;
                            VisibleColumns.Add(expanderColumn);
                        }
                        else
                        {
                            // When we scroll horizontally , expander column will be collapsed. we should get back to visible state while it comes to view.
                            expanderColumn.IsEnsured = true;
                            if (expanderColumn.ColumnVisibility == Visibility.Collapsed)
                                expanderColumn.ColumnVisibility = Visibility.Visible;
                        }
                        continue;
                    }
                    
                    //WPF-22091 - We have reset the Column index while calling InValidateUnBoundRow method for raising the QueryUnBoundRow event.
                    //but we didn't update the CurrentCell so new column is created every click, instead of reset the ColumnIndex as -1 in InValidateUnBoundRow
                    //we use isDirty property for raising the QueryUnBoundRow event for each column.
                   if (this.VisibleColumns.All(column => column.ColumnIndex != index || CanUpdateColumn(column)))
                    // will reuse the row, that goes to out of view.
                    {
                        DataColumnBase datacolumn = null;
                        //WPF-28945 Reused the datacolumn for the CurrentCell which is already maintained as current cell.
                        if (this.IsCurrentRow && this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == index)
                            datacolumn = this.VisibleColumns.FirstOrDefault(column => column.IsCurrentCell);

                        if (datacolumn == null)
                            // ColumnIndex needs to be checked with StartBodyColumnIndex and EndColumnIndex - Due to avoid reusing frozen columns from header and footer.
                            datacolumn = this.VisibleColumns.FirstOrDefault(
                                column => ((column.ColumnIndex < StartBodyColumnIndex || column.ColumnIndex > EndColumnIndex || CanUpdateColumn(column)) &&
                                            !column.IsEnsured && !column.IsIndentColumn && column.Renderer != null && !this.DataGrid.IsRowHeaderIndex(column.ColumnIndex) && // Its needs to be chcek and avoid RwoHeader to be reused, when view of dataGrid is null, this cell will not get set IsEnsured as true.
                                            !column.IsExpanderColumn && !column.IsEditing && (!column.IsCurrentCell || CanUpdateColumn(column))));
                         if (datacolumn != null && !(this.IsCurrentRow && datacolumn.ColumnElement is GridCell && !(string.IsNullOrEmpty((datacolumn.ColumnElement as GridCell).celleventErrorMessage + (datacolumn.ColumnElement as GridCell).roweventErrorMessage))))                       
                        {
                            if (datacolumn.IsSpannedColumn && !datacolumn.IsIndentColumn)
                            {
                                if (datacolumn.ColumnElement.Visibility == Visibility.Collapsed)
                                    datacolumn.ColumnElement.Visibility = Visibility.Visible;
                                datacolumn.IsSelectedColumn = false;
                                datacolumn.isSpannedColumn = false;                                
                            }

                            // which will reuse the column element fully, or load its element with different one.
                            UpdateColumn(datacolumn, index);
                        }
                    }
                    
                    var dc = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                    GridColumn gc = null;
                    var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(index);
                    if (this.DataGrid.Columns.Count > columnIndex)
                        gc = this.DataGrid.Columns[columnIndex];

                    if (dc != null)
                    {
                        if (dc.ColumnVisibility == Visibility.Collapsed && gc != null && !gc.IsHidden)
                        {
                            // the column that going to reuse will be in collapsed state.
                            dc.ColumnVisibility = Visibility.Visible;
                            //Fix for WPF-13413 - when column visibility is changed from Collapsed to visible Cell style is not updating.
                            if(this.RowData != null || this.RowType == RowType.AddNewRow || this.RowType == RowType.FilterRow || this.RowType == RowType.HeaderRow)
                                dc.UpdateCellStyle(this.RowData);
                            if (dc.Renderer != null && (dc.Renderer.HasCurrentCellState && (dc.IsEditing || dc.GridColumn.CanFocus())))
                                dc.Renderer.SetFocus(true); // Need to set focus for editor when comes to view.
                        }
                        if (this.DataGrid.StackedHeaderRows!=null && this.DataGrid.StackedHeaderRows.Count > 0 && this.RowType == Grid.RowType.HeaderRow)
                            dc.RowSpan =
                                this.DataGrid.GetHeightIncrementationLimit(new CoveredCellInfo(index, index,0,0),
                                                                                         this.RowIndex - 1);

                        // Need to set current cell for the column that comes to view.
                        //WPF-28914 Add condition to check FilterRowIndex, the FilterRow is also maintain currentcell when navigation is row.
                        if (needToUpdateCurrentCell)
                            this.UpdateCurrentCellSelection(dc);

                        //Updates grid cell border for FreezePanes
                        this.UpdateFixedColumnState(dc);

                        dc.IsEnsured = true;
                    }
                    else
                    {
                        if (index >= this.DataGrid.VisualContainer.ColumnCount)
                            continue;
                        int heightIncrementation = 0;
                        //Added the condition of RowType to calculate only for Header rows.
                        if (canIncrementHeight)
                            heightIncrementation =
                                this.DataGrid.GetHeightIncrementationLimit(new CoveredCellInfo(index, index,0,0),
                                                                                         this.RowIndex - 1);
                        var datacolumn = CreateColumn(index, heightIncrementation);

                        // Need to set current cell for the column that has been created newly.
                        //Added the Condition when the navigation mode as Row we have to skip the CurrentCell Selection. 
                        //So checked the Navigation Mode as Cell or Not.
                        if (needToUpdateCurrentCell)
                            this.UpdateCurrentCellSelection(datacolumn);

                        datacolumn.IsEnsured = true;
                        this.VisibleColumns.Add(datacolumn);
                        if (gc != null && gc.IsHidden)
                            datacolumn.ColumnVisibility = Visibility.Collapsed;
                        //Updates grid cell border for FreezePanes
                        this.UpdateFixedColumnState(datacolumn);
                    }
                }
            }

            // when reusing selected cell need to set 
            if (this.DataGrid.SelectionUnit != GridSelectionUnit.Row )
                this.DataGrid.RowGenerator.EnsureCellSelection(this);            
            
            this.VisibleColumns.ForEach(column =>
                {
                    if (!column.IsEnsured)
                    {
                        CollapseColumn(column);
                    }
                });

            // Query raised when the column has not reused but indeed to update covered range
            if (this.DataGrid.CanQueryCoveredRange())
            {
                if (this.IsEnsured && (this.RowType == Grid.RowType.DefaultRow || this.RowType == Grid.RowType.UnBoundRow))                
                    this.DataGrid.MergedCellManager.InitializeMergedRow(this);                

                if (this.IsSpannedRow)
                { 
                    this.DataGrid.MergedCellManager.UpdateMappedColumnIndex(this);

                    this.DataGrid.RowGenerator.EnsureMergedCellSelection(this);

                    // to ensure the merged cell has been set with IsSelectedColum true.                
                    this.VisibleColumns.ForEach(column =>
                        {
                            if (column.IsSpannedColumn)
                            {                                                                                                                                     
                                if (column.IsCurrentCell && column.ColumnVisibility == Visibility.Collapsed)
                                {
                                    column.isSpannedColumn = false;
                                    column.IsCurrentCell = false;
                                }
                            }
                        });
                }                
            }

            //Reset the flag after update the columns.
            this.isDirty = false;
            Panel panel = this.WholeRowElement.ItemsPanel;
            // all operation needs to call this, need to arrange while grouping,selection, resizing            
            // We can avoid only on loading, if none of the action was occurred on gridcell/VirtualizingCellsControl. But it will improve the loading performance.

            if (panel != null && this.DataGrid.IsLoaded)
            {                
                panel.InvalidateMeasure();
#if WPF
                if(this.DataGrid.useDrawing)
                    panel.InvalidateVisual();
#endif
            }
        }


        internal override void UpdateCurrentCellSelection()
        {                        
            if (this.DataGrid.SelectionMode == GridSelectionMode.None)
                return;

            if ((DataGrid.SelectionController.CurrentCellManager.CurrentCell == null || DataGrid.SelectionController.CurrentCellManager.CurrentCell.IsCurrentCell || 
                (DataGrid.SelectionController.CurrentCellManager.CurrentCell.IsSelectedColumn && DataGrid.CanQueryCoveredRange())))
            {
                var dataColumn =
                    this.VisibleColumns.FirstOrDefault(
                        item => item.ColumnIndex == this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);                
                if (dataColumn != null)
                {                  
                    if (!dataColumn.IsCurrentCell)
                    {
                        if (this.DataGrid.CanQueryCoveredRange() && dataColumn.IsSpannedColumn)
                            return;

                        dataColumn.IsCurrentCell = true;

                        //WPF-22527 in detailsview  when addnewrow is not in view. while we try to navigate AddNewRow,we Cant able to set AddNewRowMode.
                        //because we cant able to get datarow from row generator . in that case we  handled below.
                        if (DataGrid.SelectionController.CurrentCellManager.IsAddNewRow)
                        {
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        }

                        DataGrid.SelectionController.CurrentCellManager.SetCurrentColumnBase(dataColumn, true);
                    }   
                 
                }
            }
        }
        
        protected internal override void OnRowIndexChanged()
        {
            base.OnRowIndexChanged();
            if (suspendUpdateStyle && (DataGrid.hasAlternatingRowStyle || DataGrid.hasAlternatingRowStyleSelector))
                this.ApplyRowStyles(this.WholeRowElement);

            if (DataGrid != null && DataGrid.showRowHeader)
            {
                var visiblecolumn = this.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == 0);
                if (visiblecolumn == null || this.RowIndex < 0) return;
                var rowHeaderCell = (visiblecolumn.ColumnElement as GridRowHeaderCell);
                if (rowHeaderCell != null)
                    rowHeaderCell.RowIndex = this.RowIndex;
            }

            var dc = this.VisibleColumns.FirstOrDefault(column => column.IsExpanderColumn);
            if (dc == null || this.RowIndex <= 0) return;
            var expander = (dc.ColumnElement as GridDetailsViewExpanderCell);
            if (expander != null)
                expander.RowColumnIndex = new RowColumnIndex(this.RowIndex, dc.ColumnIndex);
        }

        #endregion

        #region private method

        internal virtual DataColumnBase CreateColumn(int index, int columnHeightIncrementation)
        {
            DataColumnBase dc = new DataColumn();
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.RowSpan = columnHeightIncrementation;
            var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(index);
            dc.GridColumn = this.DataGrid.Columns[columnIndex];

            if (this.RowIndex <= this.DataGrid.GetHeaderIndex() && this.RowIndex >= 0)
            {
                dc.Renderer = this.DataGrid.CellRenderers["Header"];
                // WPF-32388 - GridComboBoxColumn ItemsSource is null in DirectTrac while changing System resolution
                // No need to set RowData for HeaderRow.
                // this.RowData = this.DataGrid.Columns[columnIndex];
            }
            else if (dc.GridColumn.IsUnbound)
            {
                if ((dc.GridColumn as GridUnBoundColumn).hasEditTemplate|| (dc.GridColumn as GridUnBoundColumn).hasEditTemplateSelector)
                    dc.Renderer = this.DataGrid.CellRenderers["UnBoundTemplateColumn"];
                else
                    dc.Renderer = this.DataGrid.CellRenderers["UnBoundTextColumn"];
            }
            else
                dc.Renderer = dc.GridColumn.CellType != string.Empty
                                  ? this.DataGrid.CellRenderers[dc.GridColumn.CellType]
                                  : this.DataGrid.CellRenderers["Static"];

            dc.IsEditing = false;
            dc.InitializeColumnElement(this.RowData, false);
            dc.SelectionController = this.DataGrid.SelectionController;

            SetCurrentCellBorderBinding(dc.ColumnElement);

            if (dc.GridColumn.GridValidationMode != GridValidationMode.None)
            {                
                if(this.RowIndex >= this.DataGrid.GetFirstRowIndex() && this.RowIndex >= 0)
                {
                    this.DataGrid.ValidationHelper.ValidateColumn(this.RowData, dc.GridColumn.MappingName, dc.ColumnElement as GridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
            return dc;
        }

        internal virtual void UpdateColumn(DataColumnBase dc, int index)
        {            
            if (index < 0 || index >= this.DataGrid.VisualContainer.ColumnCount)
            {
                dc.ColumnVisibility = Visibility.Collapsed;
            }
            else
            {
                dc.ColumnIndex = index;
                //Skipped the RowSpan calculation for DataRow using the RowType
                if (this.DataGrid.StackedHeaderRows.Count > 0 && this.RowType == Grid.RowType.HeaderRow)
                    dc.RowSpan = this.DataGrid.GetHeightIncrementationLimit(new CoveredCellInfo(index, index,0,0), this.RowIndex - 1);
                else
                    dc.RowSpan = 0;

                var gridCell = dc.ColumnElement as GridCell;
                if (gridCell != null && gridCell.HasError)
                    gridCell.RemoveAll();

                var currentColumn = this.DataGrid.Columns[this.DataGrid.ResolveToGridVisibleColumnIndex(index)];
                bool isElementUnloaded = this.UpdateRenderer(dc, currentColumn);

                dc.GridColumn = currentColumn;

                if (isElementUnloaded)
                {
                    if (dc.columnElement != null)
                    {
                        dc.ColumnElement.ClearValue(FrameworkElement.DataContextProperty);
                        if (dc.ColumnVisibility == Visibility.Collapsed)
                            dc.ColumnVisibility = Visibility.Visible;
                    }
                    dc.InitializeColumnElement(this.RowData, dc.IsEditing);
                    if(this.RowData != null || this.RowType == RowType.AddNewRow || this.RowType == RowType.FilterRow || this.RowType == RowType.HeaderRow)
                    	dc.UpdateCellStyle(this.RowData);
                }
                else
                {
                    if (dc.ColumnVisibility == Visibility.Collapsed)
                        dc.ColumnVisibility = Visibility.Visible;
                    if (this.RowType != RowType.UnBoundRow)
                        dc.UpdateBinding(this.RowData);
                }
                
                //WPF - 27647 - update the grid cells when columns are reused while scrolling horizontally.
                if (gridCell != null)
                    gridCell.OnColumnChanged();

                if (dc.GridColumn.GridValidationMode != GridValidationMode.None)
                {                   
                    if(this.RowIndex >= this.DataGrid.GetFirstRowIndex() && this.RowIndex >= 0)
                        this.DataGrid.ValidationHelper.ValidateColumn(this.RowData, dc.GridColumn.MappingName, dc.ColumnElement as GridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
        }
        /// <summary>
        /// Update Renderer and UnloadUIElement if needed
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal virtual bool UpdateRenderer(DataColumnBase dataColumn, GridColumn column)
        {
            IGridCellRenderer newRenderer = null;
            var update = (dataColumn.GridColumn.hasCellTemplateSelector || dataColumn.GridColumn.hasCellTemplate) || (column.hasCellTemplate || column.hasCellTemplateSelector);

            if (this.DataGrid.GetHeaderIndex() == dataColumn.RowIndex)
            {
                newRenderer = this.DataGrid.CellRenderers["Header"];
                update = dataColumn.GridColumn.hasHeaderTemplate || column.hasHeaderTemplate;
            }
            else
            {
                if (column.IsUnbound)
                {
                    var unboundcol = (column as GridUnBoundColumn);
                    if (unboundcol.hasEditTemplate || unboundcol.hasEditTemplateSelector)
                        newRenderer = this.DataGrid.CellRenderers["UnBoundTemplateColumn"];
                    else
                        newRenderer = this.DataGrid.CellRenderers["UnBoundTextColumn"];                    
                }
                else
                    newRenderer = column.CellType != string.Empty
                                  ? this.DataGrid.CellRenderers[column.CellType]
                                  : this.DataGrid.CellRenderers["Static"];
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

        private void UpdateCurrentCellSelection(DataColumnBase column)
        {
            if (this.DataGrid.SelectionMode != GridSelectionMode.None)
            {
                if (this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == column.ColumnIndex)
                {
                    column.IsCurrentCell = true;
                    if (DataGrid.SelectionController.CurrentCellManager.CurrentCell == null)
                        DataGrid.SelectionController.CurrentCellManager.SetCurrentColumnBase(column, true);
                }
                else
                {
                    column.IsCurrentCell = false;
                }
            }
        }

        internal virtual void ApplyRowStyles(ContentControl row)
        {

            if (DataGrid == null || row == null || this.RowType == RowType.AddNewRow)
                return;

            var hasRowStyleSelector = DataGrid.hasRowStyleSelector;
            var hasRowStyle = DataGrid.hasRowStyle;
            var hasAlternatingRowStyle = DataGrid.hasAlternatingRowStyle;
            var hasAlternatingRowStyleSelector = DataGrid.hasAlternatingRowStyleSelector;

            if (!hasRowStyle && !hasRowStyleSelector && !hasAlternatingRowStyle && !hasAlternatingRowStyleSelector)
                return;

            Style newStyle = null;
            if (hasAlternatingRowStyle || hasAlternatingRowStyleSelector)
            {
                int index;
                if (DataGrid.GridModel.HasGroup)
                    index = GroupRecordIndex;
                else
                {
                    index = RowIndex - DataGrid.GetFirstNavigatingRowIndex();
                    if (this.DataGrid.DetailsViewManager.HasDetailsView)
                        index = index / (this.DataGrid.DetailsViewDefinition.Count + 1);
                }
                index += 1;
                var canApplyAlternatingRowStyle = (index % DataGrid.AlternationCount) == 0;
                if (canApplyAlternatingRowStyle)
                {
                    if (hasAlternatingRowStyle && hasAlternatingRowStyleSelector)
                    {
                        newStyle = DataGrid.AlternatingRowStyleSelector.SelectStyle(this, row);
                        newStyle = newStyle ?? DataGrid.AlternatingRowStyle;                     
                    }
                    else if (hasAlternatingRowStyleSelector)
                    {
                        newStyle = DataGrid.AlternatingRowStyleSelector.SelectStyle(this, row);
                    }
                    else if (hasAlternatingRowStyle)
                    {
                        newStyle = DataGrid.AlternatingRowStyle;
                    }

                    if (newStyle != null)
                        row.Style = newStyle;
                    else
                        row.ClearValue(FrameworkElement.StyleProperty);
                    return;
                }
            }

            if (!hasRowStyle && !hasRowStyleSelector)
            {
                if(row.ReadLocalValue(FrameworkElement.StyleProperty)!= DependencyProperty.UnsetValue)
                    row.ClearValue(FrameworkElement.StyleProperty);
                return;
            }

            if (hasRowStyleSelector && hasRowStyle)
            {
                newStyle = DataGrid.RowStyleSelector.SelectStyle(this, row);
                newStyle = newStyle ?? DataGrid.RowStyle;             
            }
            else if (hasRowStyleSelector)
            {
                newStyle = DataGrid.RowStyleSelector.SelectStyle(this, row);
            }
            else if (hasRowStyle)
            {
                newStyle = DataGrid.RowStyle;
            }

            if (newStyle != null)
                row.Style = newStyle;
            else
                row.ClearValue(FrameworkElement.StyleProperty);
        }
        
        internal override void UpdateRowStyles(ContentControl row)
        {
            if (row != null && this.RowType != RowType.HeaderRow)
            {
                this.ApplyRowStyles(row);
            }
        }

        protected override DataColumnBase CreateIndentColumn(int index)
        {
            DataColumnBase dc = new DataColumn();
            dc.IsIndentColumn = true;
            dc.IsEnsured = true;
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.IsEditing = false;
            dc.GridColumn = null;
            dc.SelectionController = this.DataGrid.SelectionController;
            //Checked whether it is DataRow using RowType to create HeaderIncentCell.
            if (this.RowRegion == RowRegion.Header && this.RowType != Grid.RowType.DefaultRow && this.RowType != Grid.RowType.AddNewRow && this.RowType != Grid.RowType.UnBoundRow && this.RowType != Grid.RowType.FilterRow)
                dc.ColumnElement = new GridHeaderIndentCell() { ColumnBase = dc };
            else
                dc.InitializeColumnElement(this.RowData, false);
#if WPF
            if (this.DataGrid.useDrawing)
                (dc.ColumnElement as GridCell).UseDrawing = DataGrid.useDrawing;
#endif
            return dc;
        }

        private DataColumnBase CreateDetailsViewExpanderColumn(int index)
        {
            DataColumnBase dc = new DataColumn();
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.GridColumn = null;
            dc.IsEditing = false;
            dc.SelectionController = this.DataGrid.SelectionController;
            // Need to initialize the ColumnBase because we have process the Selection using ColumnBase
            if(this.RowType == RowType.AddNewRow)
            {
                dc.ColumnElement = new GridIndentCell() { ColumnType = IndentColumnType.InAddNewRow, ColumnBase = dc };
                dc.IsIndentColumn = true;
            }
            else if(this.RowType == RowType.UnBoundRow)
            {
                dc.ColumnElement = new GridIndentCell() {   ColumnType = IndentColumnType.InUnBoundRow, ColumnBase = dc };
                dc.IsIndentColumn = true;
            }
            else if (this.RowType == RowType.FilterRow)
            {
                dc.ColumnElement = new GridIndentCell() { ColumnType = IndentColumnType.InFilterRow, ColumnBase = dc };
                dc.IsIndentColumn = true;
            }
            else if (this.RowType == Grid.RowType.HeaderRow)
            {
                dc.ColumnElement = new GridHeaderIndentCell() { ColumnBase = dc };
                dc.IsIndentColumn = true;
            }
            else
            {
                dc.IsExpanderColumn = true;
                dc.Renderer = this.DataGrid.CellRenderers["DetailsViewExpander"];
                dc.InitializeColumnElement(this.RowData, false);
                var expander = (dc.ColumnElement as GridDetailsViewExpanderCell);
                if (expander != null)
                {
                    expander.columnBase = dc;
                    if (expander.IsExpanded != this.IsExpanded)
                        expander.IsExpanded = this.IsExpanded;
                }
                //In this method, we have bind the CurrentCellBorderBrush properties only for GridCell, for expander cell it wont be 
                //executed, hence this method is removed.
                //SetCurrentCellBorderBinding(dc.ColumnElement);
            }
            return dc;
        }

        internal void CheckForDetailsViewExpanderVisibilty()
        {
            var dc = this.VisibleColumns.FirstOrDefault(column => column.IsExpanderColumn);
            if (dc == null || this.DataGrid.View == null) return;
            var expander = (dc.ColumnElement as GridDetailsViewExpanderCell);
            if (expander == null) return;
            if (this.RowType == RowType.AddNewRow || this.RowType == RowType.UnBoundRow || this.RowType == RowType.FilterRow)
            {
                expander.ExpanderIconVisibility = Visibility.Collapsed;
                return;
            }
            var record = this.DataGrid.DetailsViewManager.GetDetailsViewRecord(this.RowIndex);
            if (!this.DataGrid.HideEmptyGridViewDefinition)
            {
                expander.ExpanderIconVisibility = Visibility.Visible;
                if (record != null && expander.IsExpanded != record.IsExpanded)
                {
                    expander.SuspendChangedAction = true;
                    expander.IsExpanded = record.IsExpanded;
                    expander.SuspendChangedAction = false;
                }
                if (expander.IsExpanded)
                {
                    for (int i = 1; i <= this.DataGrid.DetailsViewDefinition.Count; i++)
                    {
                        var row = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(datarow => datarow.RowIndex == i + this.RowIndex);
                        if (row != null)
                        {
                            if (i == this.DataGrid.DetailsViewDefinition.Count)
                                row.ApplyContentVisualState("LastCell");
                            else
                                row.ApplyContentVisualState("NormalCell");
                        }
                    }
                }
                return;
            }
            var provider = this.DataGrid.View.GetPropertyAccessProvider();
            if (provider == null) return;
            int count = 0;
            bool IsExpanderVisible = false;
            int detailsrowIndex = -1;
            foreach (var gridViewDefinition in this.DataGrid.DetailsViewDefinition.OfType<GridViewDefinition>())
            {
                var childSource = provider.GetValue(this.RowData, gridViewDefinition.RelationalColumn) as IEnumerable;
                //if (childSource != null && childSource.AsQueryable().Count() > 0)
                count++;
                if (childSource != null && DetailsViewHelper.GetChildSourceCount(childSource) > 0)
                {
                    expander.ExpanderIconVisibility = Visibility.Visible;
                    IsExpanderVisible = true;
                    detailsrowIndex = count;
                }
                else
                {
                    int valueCount;
                    if (!this.DataGrid.VisualContainer.RowHeights.GetHidden(this.RowIndex + count, out valueCount))
                        this.DataGrid.VisualContainer.RowHeights.SetHidden(this.RowIndex + count, this.RowIndex + count, true);
                }
            }
            if (!IsExpanderVisible)
            {
                expander.SuspendChangedAction = true;
                if (expander.IsExpanded)
                    expander.IsExpanded = false;
                if (record != null)
                    record.IsExpanded = false;
                expander.SuspendChangedAction = false;
                expander.ExpanderIconVisibility = Visibility.Collapsed;
            }
            else
            {
                var row = this.DataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(datarow => datarow.RowIndex == detailsrowIndex + this.RowIndex);
                if (row != null)
                    row.ApplyContentVisualState("LastCell");
            }
        }

        #endregion

        #region internal methods
        /// <summary>
        /// Update the value of UnBoundColumn
        /// </summary>
        /// <param name="dr"></param>
        /// <remarks></remarks>
        internal void UpdateUnBoundColumn()
        {
            if (VisibleColumns.Any(col => col.GridColumn != null && col.GridColumn.IsUnbound))
            {
                VisibleColumns.ForEach(col =>
                {
                    if (col.GridColumn != null && col.GridColumn.IsUnbound)
                    {
                        col.ColumnElement.DataContext = this.RowData;
                         col.UpdateBinding(this.RowData);
                    }
                });
            }
        }

        #endregion
    }
    /// <summary>
    /// The class that going to deals with UnBoundDataRow 's column creation and updating it's renderer and initialize it element.
    /// </summary>
    public class UnBoundRow : DataRow
    {
        #region ctr
        public UnBoundRow()
        {

        }
        #endregion

        internal override void EnsureColumns(VisibleLinesCollection visibleColumnLines)
        {
            // WPF-25569 - When we cleared the columns we should not access the visible lines of columns.
            if (this.DataGrid.UnBoundRows.Count == 0 || this.DataGrid.Columns.Count == 0)
                return; 

            base.EnsureColumns(visibleColumnLines);
        }

        protected override bool CanUpdateColumn(DataColumnBase dataColumn)
        {
            if (this.isDirty)
                return true;
            return false;
        }

        /// <summary>
        /// Method which used to create the column for the UnBound DataRow.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="columnHeightIncrementation"></param>
        /// <returns></returns>
        internal override DataColumnBase CreateColumn(int index, int columnHeightIncrementation)
        {
            DataColumnBase dc = new DataColumn();
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.isUnBoundRowCell = true;
            dc.RowSpan = columnHeightIncrementation;
            var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(index);
            dc.GridColumn = this.DataGrid.Columns[columnIndex];            

            GridUnBoundRow row = DataGrid.GetUnBoundRow(this.RowIndex);
            if (row != null)
            {
                row.RowIndex = this.RowIndex;
                // New Queried UnbOundDataRow args definition.
                GridUnBoundRowEventsArgs queriedColumn = null;
                if (this.DataGrid.CanQueryUnBoundRow())
                    queriedColumn = this.DataGrid.RaiseQueryUnBoundRow(row, UnBoundActions.QueryData, this.RowData, dc.GridColumn, dc.GridColumn.CellType, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));

                if (queriedColumn != null)
                {
                    // if user has set edit template we need to provide behavior like UnBoundTemplate Column.
                    dc.Renderer = this.DataGrid.UnBoundRowCellRenderers.ContainsKey(queriedColumn.CellType) && !string.IsNullOrEmpty(queriedColumn.CellType) ? this.DataGrid.UnBoundRowCellRenderers[queriedColumn.CellType]
                        : queriedColumn.hasEditTemplate ? this.DataGrid.UnBoundRowCellRenderers["UnBoundTemplateColumn"]
                        : this.DataGrid.UnBoundRowCellRenderers["UnBoundTextColumn"];
                }
                else
                {
                    // if the event has not handled by user will get null for queriedColumn. 
                    //so we are creating with default args value to load default column - UnBoundTextColumn.
                    dc.Renderer = this.DataGrid.UnBoundRowCellRenderers["UnBoundTextColumn"];
                    queriedColumn = new GridUnBoundRowEventsArgs(row, UnBoundActions.QueryData, null, dc.GridColumn, dc.GridColumn.CellType, this, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }

                // which will maintain UnBoundDataRow event args which helps in Initialize element and update cell's binding, style and fire event when complete editing.
                dc.GridUnBoundRowEventsArgs = queriedColumn;
                dc.IsEditing = false;
            }
            else
            {
                if (dc.GridColumn.IsUnbound)
                {
                    if ((dc.GridColumn as GridUnBoundColumn).hasEditTemplate|| (dc.GridColumn as GridUnBoundColumn).hasEditTemplateSelector)
                        dc.Renderer = this.DataGrid.CellRenderers["UnBoundTemplateColumn"];
                    else
                        dc.Renderer = this.DataGrid.CellRenderers["UnBoundTextColumn"];
                }
                else
                    dc.Renderer = dc.GridColumn.CellType != string.Empty
                                      ? this.DataGrid.CellRenderers[dc.GridColumn.CellType]
                                      : this.DataGrid.CellRenderers["Static"];
            }
            dc.InitializeColumnElement(this.RowData, false);            
            dc.ColumnElement.DataContext = dc.GridUnBoundRowEventsArgs;
            dc.SelectionController = this.DataGrid.SelectionController;

            SetCurrentCellBorderBinding(dc.ColumnElement);

            if (dc.GridColumn.GridValidationMode != GridValidationMode.None)
            {
                if (this.RowIndex >= this.DataGrid.GetFirstRowIndex() && this.RowIndex >= 0)
                {
                    this.DataGrid.ValidationHelper.ValidateColumn(this.RowData, dc.GridColumn.MappingName, dc.ColumnElement as GridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }

            return dc;
        }

        /// <summary>
        ///  update the column for UnBoundDataRow
        /// </summary>
        /// <param name="dc"></param>
        /// <param name="index"></param>
        internal override void UpdateColumn(DataColumnBase dc, int index)
        {
            if (index < 0 || index >= this.DataGrid.VisualContainer.ColumnCount)
            {
                dc.ColumnVisibility = Visibility.Collapsed;
            }
            else
            {
                dc.ColumnIndex = index;                           
                dc.RowSpan = 0;

                var gridCell = dc.ColumnElement as GridCell;
                if (gridCell != null && gridCell.HasError)
                    gridCell.RemoveAll();

                var currentColumn = this.DataGrid.Columns[this.DataGrid.ResolveToGridVisibleColumnIndex(index)];
                this.UpdateRenderer(dc, currentColumn);
                
                dc.GridColumn = currentColumn;

                if (dc.ColumnVisibility == Visibility.Collapsed)
                    dc.ColumnVisibility = Visibility.Visible;
                                  
                dc.InitializeColumnElement(this.RowData, dc.IsEditing);
                dc.UpdateCellStyle(this.RowData);                
                //WPF - 27647 - update the UnboundRowCell when columns are reused while scrolling horizontally.
                if (gridCell != null)
                    gridCell.OnColumnChanged();

                if (dc.GridColumn.GridValidationMode != GridValidationMode.None)
                {
                    if (this.RowIndex >= this.DataGrid.GetFirstRowIndex() && this.RowIndex >= 0)
                        this.DataGrid.ValidationHelper.ValidateColumn(this.RowData, dc.GridColumn.MappingName, dc.ColumnElement as GridCell, new RowColumnIndex(dc.RowIndex, dc.ColumnIndex));
                }
            }
        }

        /// <summary>
        /// Update Renderer and UnloadUIElement if needed for UnBoundDataRow
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal override bool UpdateRenderer(DataColumnBase dataColumn, GridColumn column)
        {
            IGridCellRenderer newRenderer = null;
            var row = DataGrid.GetUnBoundRow(this.RowIndex);
            if (row == null) return false;

            row.RowIndex = this.RowIndex;
            // New Queried UnbOundDataRow args definition.
            GridUnBoundRowEventsArgs queriedColumn = null;
            if (this.DataGrid.CanQueryUnBoundRow())
                queriedColumn = this.DataGrid.RaiseQueryUnBoundRow(row, UnBoundActions.QueryData, this.RowData, column, dataColumn.previousGridUnBoundRowEventsArgs != null ? dataColumn.previousGridUnBoundRowEventsArgs.CellType :dataColumn.GridUnBoundRowEventsArgs != null ? dataColumn.GridUnBoundRowEventsArgs.CellType : string.Empty, new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex));

            if (queriedColumn != null)
            {
                // if user has set edit template we need to provide behavior like UnBoundTemplate Column.
                newRenderer = this.DataGrid.UnBoundRowCellRenderers.ContainsKey(queriedColumn.CellType) && !string.IsNullOrEmpty(queriedColumn.CellType) ? this.DataGrid.UnBoundRowCellRenderers[queriedColumn.CellType]
                    : queriedColumn.hasEditTemplate ? this.DataGrid.UnBoundRowCellRenderers["UnBoundTemplateColumn"]
                        : this.DataGrid.UnBoundRowCellRenderers["UnBoundTextColumn"];
            }
            else
            {
                // if the event has not handled by user will get null for queriedColumn. 
                //so we are creating with default args value to load default column - UnBoundTextColumn.
                newRenderer = dataColumn.previousGridUnBoundRowEventsArgs != null ? this.DataGrid.UnBoundRowCellRenderers[dataColumn.previousGridUnBoundRowEventsArgs.CellType] : this.DataGrid.UnBoundRowCellRenderers["UnBoundTextColumn"];
                queriedColumn = new GridUnBoundRowEventsArgs(row, UnBoundActions.QueryData, null, dataColumn.GridColumn, column.CellType, this, new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex));
            }

            dataColumn.previousGridUnBoundRowEventsArgs = dataColumn.GridUnBoundRowEventsArgs;
            // which will maintain UnBoundDataRow event args which helps in Initialize element and update cell's binding, style and fire event when complete editing.
            dataColumn.GridUnBoundRowEventsArgs = queriedColumn;

            var update = (dataColumn.GridUnBoundRowEventsArgs.hasCellTemplate || dataColumn.previousGridUnBoundRowEventsArgs.hasCellTemplate);
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

        internal override DataColumnBase EnsuringLastRow(int index)
        {            
            var indentColumn = CreateIndentColumn(index);
            indentColumn.IsEnsured = true;                  
            indentColumn.IndentColumnType = IndentColumnType.InLastGroupRow;         
            return indentColumn;            
        }        

        /// <summary>
        /// Method used to apply the style to UnBoundRow.
        /// </summary>
        /// <param name="row"></param>
        internal override void ApplyRowStyles(ContentControl row)
        {

            if (DataGrid == null || row == null)
                return;
            
            if (!DataGrid.hasUnBoundRowStyle)
                return;

            if (DataGrid.UnBoundRowStyle != null)
                row.Style = DataGrid.UnBoundRowStyle;
            else
                row.ClearValue(FrameworkElement.StyleProperty);            
        }

        protected override bool AllowRowHoverHighlighting()
        {
            if (!DataGrid.AllowRowHoverHighlighting)
                return false;

            var unboundRow = this.DataGrid.GetUnBoundRow(this.RowIndex);
            if (unboundRow == null)
                return false;
            return unboundRow.Position == UnBoundRowsPosition.Top ? (this.RowIndex > (this.DataGrid.GetHeaderIndex() + this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false)))
                : (this.RowIndex < (this.DataGrid.VisualContainer.RowCount - this.DataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true)));
        }
    }
}
