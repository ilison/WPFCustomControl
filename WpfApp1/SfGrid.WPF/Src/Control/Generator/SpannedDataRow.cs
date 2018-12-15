#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using System.Linq;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#else
using System.Windows;
using System.Windows.Controls;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class SpannedDataRow : GridDataRow
    {
        #region Fields

        List<CoveredCellInfo> coveredCells;
        internal Func<int,int,double> GetCoveredColumnSize;
        private bool isdisposed = false;
        #endregion

        #region Property

        internal List<CoveredCellInfo> CoveredCells
        {
            get { return coveredCells; }
        }

        #endregion

        #region Ctor

        public SpannedDataRow()
        {
            coveredCells = new List<CoveredCellInfo>();
        }

        #endregion

        #region override methods

        protected bool ShowRowHeader()
        {
            return this.DataGrid.ShowRowHeader;
        }

        protected internal override void OnRowIndexChanged()
        {
            base.OnRowIndexChanged();

            if (DataGrid != null && DataGrid.ShowRowHeader)
            {
                var dc = this.VisibleColumns.FirstOrDefault();
                if (dc == null || this.RowIndex <= 0) return;
                var rowHeaderCell = (dc.ColumnElement as GridRowHeaderCell);
                if (rowHeaderCell != null)
                    rowHeaderCell.RowIndex = this.RowIndex;
            }
        }

        //µÃµ½HeaderRowControl
        protected override VirtualizingCellsControl OnCreateRowElement()
        {
            if (this.RowType== RowType.CaptionRow || this.RowType== RowType.CaptionCoveredRow)
            {
                var captionRow = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<CaptionSummaryRowControl>() as CaptionSummaryRowControl;
                captionRow.DataContext = this.RowData;
                captionRow.Visibility = this.RowVisibility;                
                this.UpdateRowStyles(captionRow);
                captionRow.GetVisibleLineOrigin = GetVisibleLineOrigin;
                captionRow.UpdateVisibleColumn(GetDataRow,this.ShowRowHeader, this.GetColumnVisibleLineInfo, GetColumnSize,null);
                captionRow.IsExpandedChanged = IsExpandedChanged;
                captionRow.CheckForValidation = CheckForValidation;
                captionRow.GetRowGenerator = () => this.DataGrid.RowGenerator;
                SetSelectionBorderBindings(captionRow);
                return captionRow;
            }
            else if (this.RowType == RowType.SummaryCoveredRow || this.RowType == RowType.SummaryRow)
            {
                var summaryRow = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<GroupSummaryRowControl>() as GroupSummaryRowControl;
                summaryRow.DataContext = this.RowData;
                this.UpdateRowStyles(summaryRow);
                summaryRow.GetVisibleLineOrigin = GetVisibleLineOrigin;
                summaryRow.UpdateVisibleColumns(GetDataRow, this.GetColumnVisibleLineInfo, GetColumnSize);
                summaryRow.GetRowGenerator = () => this.DataGrid.RowGenerator;
                SetSelectionBorderBindings(summaryRow);
                return summaryRow;
            }
            else if (RowType == Grid.RowType.TableSummaryCoveredRow || RowType == Grid.RowType.TableSummaryRow)
            {
                var footerRow = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<TableSummaryRowControl>();
                footerRow.DataContext = this.RowData;
                this.UpdateRowStyles(footerRow);
                footerRow.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(footerRow);
                footerRow.GetRowGenerator = () => this.DataGrid.RowGenerator;
                return footerRow;
            }
            else if (this.RowType == Grid.RowType.HeaderRow)
            {
                var row = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<HeaderRowControl>();
                row.DataContext = this.RowData;
                row.Visibility = this.RowVisibility;
                row.InitializeVirtualizingRowControl(GetDataRow);
                row.GetRowGenerator = () => this.DataGrid.RowGenerator;
                return row;
            }
            else
            {
                var footerRow = this.DataGrid.RowGenerator.GetVirtualizingCellsControl<TableSummaryRowControl>();
                footerRow.DataContext = this.RowData;
                this.UpdateRowStyles(footerRow);
                footerRow.InitializeVirtualizingRowControl(GetDataRow);
                SetSelectionBorderBindings(footerRow);
                footerRow.GetRowGenerator = () => this.DataGrid.RowGenerator;
                return footerRow;
            }
        }

        protected internal override double GetColumnSize(int index, bool lineNull)
        {
            if (!lineNull)
            {
                foreach (var item in this.CoveredCells)
                {
                    if (index >= item.Left && index <= item.Right)
                    {
                        return GetCoveredColumnSize(item.Left, item.Right);
                    }
                }
            }
            return base.GetColumnSize(index,lineNull);
        }

        protected override void OnGenerateVisibleColumns(VisibleLinesCollection visibleColumnLines)
        {
            if (visibleColumnLines.Count <= 0)
                return;
            int startColumnIndex;
            int endColumnIndex;
            
            this.VisibleColumns.Clear();

            if (this.CoveredCells.Count > 0)
            {
                foreach (var coveredCell in this.CoveredCells)
                {
                    int columnHeightIncrementation = 0;
                    columnHeightIncrementation = coveredCell.RowSpan;
                    var dc = CreateColumn(coveredCell,coveredCell.Left, columnHeightIncrementation,coveredCell.Right - coveredCell.Left);
                    this.VisibleColumns.Add(dc);
                }
                int startIndex = 0;

                if (this.DataGrid.ShowRowHeader)
                {
                    this.CreateRowHeaderColumn(startIndex);
                    startIndex++;
                }
                int index;
                // WPF-20041 - Need to increment indent column index
                var compareIndex = this.RowType == Grid.RowType.CaptionCoveredRow ? this.RowLevel + startIndex                    
                    :(this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0 )+ startIndex;
                for (index = startIndex; index < compareIndex ; index++)
                {
                    if (!visibleColumnLines.Any(col => col.LineIndex == index))
                        continue;

                    this.CreateIndentInCoveredRow(index);
                }
                startIndex = index;

                if (!visibleColumnLines.Any(col => col.LineIndex == index))
                    return;

                if (this.DataGrid.DetailsViewManager.HasDetailsView)
                {
                    if (this.RowIndex < this.DataGrid.StackedHeaderRows.Count)
                    {
                        this.VisibleColumns.Add(this.CreateIndentColumn(startIndex));
                    }                    
                    else if (this.DataGrid.View != null && (this.RowType == RowType.SummaryRow || this.RowType == RowType.CaptionRow || this.RowType == RowType.TableSummaryRow))
                    {
                        this.VisibleColumns.Add(CreateDetailsViewIndentColumn(startIndex));
                    }
                }

                this.ResetLastColumnBorderThickness(this.VisibleColumns.LastOrDefault(visibleColumn =>
                {
                    if ((visibleColumn.ColumnElement is GridGroupSummaryCell || visibleColumn.ColumnElement is GridTableSummaryCell
                    || visibleColumn.ColumnElement is GridCaptionSummaryCell) && visibleColumn.ColumnElement.Visibility != Visibility.Collapsed)
                      
                        return visibleColumn.ColumnSpan > 0 ? true : (visibleColumn.GridColumn!=null && !visibleColumn.GridColumn.IsHidden);
                    else
                        return false;
                }), true);
            }
            else
            {
                startColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex - this.DataGrid.VisualContainer.FrozenColumns].LineIndex;
                endColumnIndex = visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;
                for (int index = startColumnIndex; index <= endColumnIndex; index++)
                {
                    if (!visibleColumnLines.Any(col => col.LineIndex == index))
                        return;

                    if (index < (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0))
                    {
                        this.CreateIndentInCoveredRow(index);
                        continue;
                    }
                    //var dc = CreateColumn(null, index, 0, 0);
                    //dc.IsFixedColumn = false;
                    //this.VisibleColumns.Add(dc);
                    //this.ResetLastColumnBorderThickness(dc, index == endColumnIndex);
                }
            }
        }

        internal override void UpdateCurrentCellSelection()
        {
            if (this.DataGrid.SelectionMode != GridSelectionMode.None && DataGrid.SelectionController.CurrentCellManager.CurrentCell == null)
            {
                var dataColumn =
                    this.VisibleColumns.FirstOrDefault(column => column is SpannedDataColumn && (column.ColumnElement as GridCell).CanSelectCurrentCell());
                if (dataColumn != null)
                {
                    if (!dataColumn.IsCurrentCell)
                    {
                        dataColumn.IsCurrentCell = true;
                        DataGrid.SelectionController.CurrentCellManager.SetCurrentColumnBase(dataColumn, true);
                    }
                }
            }
        }

        internal override void EnsureColumns(VisibleLinesCollection visibleColumnLines)
        {
            if (this.RowIndex == -1)
            {
                this.RowLevel = -1;
                return;
            }
            this.VisibleColumns.ForEach(column => column.IsEnsured = false);
            var StartBodyColumnIndex = (visibleColumnLines.firstBodyVisibleIndex < visibleColumnLines.Count) ? visibleColumnLines[visibleColumnLines.firstBodyVisibleIndex].LineIndex : visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;
            if (this.CoveredCells.Count > 0 || this.RowIndex < this.DataGrid.HeaderLineCount)
            {
                for (int i = 0; i < 3; i++)
                {
                    int startColumnIndex;
                    int endColumnIndex;
                    if (i == 0)
                    {
                        if (visibleColumnLines.FirstBodyVisibleIndex <= 0)
                            continue;
                        startColumnIndex = 0;
                        endColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex - 1].LineIndex;
                    }
                    else if (i == 1)
                    {
                        if (visibleColumnLines.FirstBodyVisibleIndex <= 0 && visibleColumnLines.LastBodyVisibleIndex < 0)
                            continue;
                        if (visibleColumnLines.Count > visibleColumnLines.firstBodyVisibleIndex)
                            startColumnIndex = visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex;
                        else
                            continue;
                        endColumnIndex = visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex;

                    }
                    else
                    {
                        if (visibleColumnLines.FirstFooterVisibleIndex >= visibleColumnLines.Count)
                            continue;
                        startColumnIndex = visibleColumnLines[visibleColumnLines.FirstFooterVisibleIndex].LineIndex;
                        endColumnIndex = visibleColumnLines[visibleColumnLines.Count - 1].LineIndex;
                    }

                    for (int index = startColumnIndex; index <= endColumnIndex; index++)
                    {
                        if (visibleColumnLines.All(row => row.LineIndex != index))
                            continue;
                        if (DataGrid.ShowRowHeader && index == 0)
                        {
                            var rhc = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                            if (rhc != null)
                            {
                                if (rhc.ColumnVisibility == Visibility.Collapsed)
                                    rhc.ColumnVisibility = Visibility.Visible;
                                rhc.IsEnsured = true;
                            }
                            else
                                CreateRowHeaderColumn(index);
                            continue;
                        }
                        if (this.DataGrid.View != null && this.DataGrid.DetailsViewManager.HasDetailsView && index == (this.DataGrid.ShowRowHeader ? this.DataGrid.View.GroupDescriptions.Count + 1 : this.DataGrid.View.GroupDescriptions.Count))
                        {
                            var dvc = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);
                            if (dvc != null)
                            {
                                if (dvc.ColumnVisibility == Visibility.Collapsed)
                                    dvc.ColumnVisibility = Visibility.Visible;
                                dvc.IsEnsured = true;
                                continue;
                            }

                            if (this.RowIndex < this.DataGrid.StackedHeaderRows.Count)
                            {
                                this.VisibleColumns.Add(this.CreateIndentColumn(index));
                            }
                            else if (this.RowType == RowType.SummaryRow || this.RowType == RowType.CaptionRow ||
                                     this.RowType == RowType.TableSummaryRow)
                            {
                                this.VisibleColumns.Add(CreateDetailsViewIndentColumn(index));
                            }
                            continue;
                        }

                        var rowHeaderIndex = this.DataGrid.ShowRowHeader ? 1 : 0;
                        var indentCellIndex = -1;
                        if (this.RowType == RowType.CaptionCoveredRow)
                            indentCellIndex = this.DataGrid.View != null ? this.RowLevel + rowHeaderIndex : rowHeaderIndex;
                        else
                            indentCellIndex = this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count + rowHeaderIndex : rowHeaderIndex;

                        if (index < indentCellIndex)
                        {
                            this.EnsureIndentColumns(visibleColumnLines, index);
                            this.CheckAvailablity(index, true);
                            continue;
                        }
                        else
                        {
                            CoveredCellInfo coveredCellItem = null;
                            if (this.RowType == RowType.CaptionCoveredRow || this.RowType == RowType.SummaryCoveredRow || this.RowType == RowType.TableSummaryCoveredRow)
                            {
                                coveredCellItem = this.CoveredCells.FirstOrDefault();
                            }
                            else
                                coveredCellItem = this.CoveredCells.FirstOrDefault(item => item.Left <= index && item.Right >= index);
                            var actualIndex = coveredCellItem != null ? coveredCellItem.Left : index;
                            if ((this.RowRegion == RowRegion.Body || RowType == RowType.TableSummaryCoveredRow || RowType == RowType.TableSummaryRow) && this.VisibleColumns.All(column => column.ColumnIndex != actualIndex))
                            {
                                if (this.VisibleColumns.Any(column => ((column.ColumnIndex < StartBodyColumnIndex || column.ColumnIndex > endColumnIndex) && !column.IsEnsured && !column.IsIndentColumn)))
                                {
                                    var datacolumn = this.VisibleColumns.FirstOrDefault(column => ((column.ColumnIndex < StartBodyColumnIndex || column.ColumnIndex > endColumnIndex) && !column.IsEnsured && !column.IsIndentColumn && column.Renderer != null && column.Renderer != this.DataGrid.CellRenderers["RowHeader"]));
                                    if (datacolumn != null)
                                    {
                                        UpdateColumn(datacolumn, index);
                                    }
                                }
                            }
                            this.CheckAvailablity(actualIndex, false);
                        }
                    }
                }
                var orderedColumns = this.VisibleColumns.OrderBy(item => item.ColumnIndex);


                this.ResetLastColumnBorderThickness(orderedColumns.LastOrDefault(col =>
                    {
                        if ((col.ColumnElement is GridGroupSummaryCell || col.ColumnElement is GridTableSummaryCell
                        || col.ColumnElement is GridCaptionSummaryCell) && col.ColumnElement.Visibility != Visibility.Collapsed )
                        {
                            if(col.ColumnSpan>0)
                            {
                                return true;
                            }
                            else
                            {

                                return col.GridColumn!=null && !col.GridColumn.IsHidden;
                            }
                        }
                        else
                            return false;
                    }), true);
            }

            if (this.RowType != Grid.RowType.TableSummaryRow || this.RowType != Grid.RowType.TableSummaryCoveredRow)
            {
                if (this.IsSelectedRow)
                {
                    this.WholeRowElement.UpdateSelectionBorderClip();
                }
                //When scrolling horizontally the Focus border margin is not changed which sets the margin including the IndentCell Width, 
                //hence the below code is added.
                else if(this.IsCurrentRow && this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                    this.WholeRowElement.UpdateFocusRowPosition();
            }
            this.VisibleColumns.ForEach(column =>
            {
                if (!column.IsEnsured)
                { 
                    CollapseColumn(column); 
                }
            });
            this.InvalidateMeasure();
        }

        internal override void UpdateRowStyles(ContentControl row)
        {
            if (row != null)
            {
                if (row is CaptionSummaryRowControl)
                {
                    this.ApplyCaptionSummaryRowStyles(row);
                }
                else if (row is GroupSummaryRowControl)
                {
                    this.ApplyGroupSummaryRowStyles(row);
                }
                else
                {
                    this.ApplyTableSummaryRowStyles(row);
                }
            }
        }

        protected override DataColumnBase CreateIndentColumn(int index)
        {
            DataColumnBase dc = new SpannedDataColumn();
            dc.IsIndentColumn = true;
            dc.IsEnsured = true;
            dc.IsEditing = false;
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.GridColumn = null;
            dc.SelectionController = this.DataGrid.SelectionController;
            if (this.RowRegion == Grid.RowRegion.Header && this.RowType == Grid.RowType.HeaderRow)
                dc.ColumnElement = new GridHeaderIndentCell() { ColumnBase = dc };
            else
                dc.InitializeColumnElement(this.RowData, false);
            if (this.RowType == RowType.TableSummaryRow || this.RowType == RowType.TableSummaryCoveredRow)
            {
                (dc.ColumnElement as GridIndentCell).ColumnType = IndentColumnType.InTableSummaryRow;
                dc.IndentColumnType = IndentColumnType.InTableSummaryRow;
            }
#if WPF
            if (this.DataGrid.useDrawing)
                (dc.ColumnElement as GridCell).UseDrawing = DataGrid.useDrawing;
#endif
            return dc;
        }

        #endregion

        #region Private Methods

        private void EnsureIndentColumns(VisibleLinesCollection visibleColumns, int index)
        {
            var startColumnIndex = visibleColumns[0].LineIndex + (this.DataGrid.ShowRowHeader ? 1 : 0);
            var endColumnIndex = visibleColumns[visibleColumns.LastBodyVisibleIndex].LineIndex;
            if ((index < (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0)) || (index <= (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0) && this.DataGrid.ShowRowHeader))
            {
                if ((index < (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0)) || (this.DataGrid.ShowRowHeader && index <= (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0)))
                {
                    if (index > this.RowLevel && (this.RowType == RowType.CaptionCoveredRow || this.RowType == RowType.SummaryCoveredRow || this.RowType == RowType.TableSummaryCoveredRow))
                    {
                        return;
                    }
                }
                if (!this.VisibleColumns.Any(column => column.ColumnIndex == index && column.IsIndentColumn))
                {
                    var dataColumn = this.VisibleColumns.FirstOrDefault(column => ((column.ColumnIndex < startColumnIndex || column.ColumnIndex > endColumnIndex) && !column.IsEnsured && column.IsIndentColumn));
                    if (dataColumn != null)
                    {
                        if (index < 0)
                        {
                            dataColumn.ColumnVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            dataColumn.ColumnIndex = index;
                            //WPF-31663 - IndentColumnType should be set based on index since, any indent cells can be reused for anything.
                            SetIndentColumnType(dataColumn);
                            if (dataColumn.ColumnVisibility == Visibility.Collapsed)
                                dataColumn.ColumnVisibility = Visibility.Visible;
                        }
                    }
                    else
                        this.CreateIndentInCoveredRow(index);
                }
                else
                {
                    var indentColumn = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index && column.IsIndentColumn);
                    if (indentColumn != null)
                    {
                        //WPF-31663 - IndentColumnType should be set based on index since, any indent cells can be reused for anything.
                        SetIndentColumnType(indentColumn);
                        indentColumn.IsEnsured = true;
                    }
                }
            }
        }

        private void UpdateColumn(DataColumnBase dc, int index)
        {
            if (index < 0 || index >= this.DataGrid.VisualContainer.ColumnCount)
            {
                dc.ColumnVisibility = Visibility.Collapsed;
            }
            else
            {
                dc.ColumnIndex = index;
                var columnElement = dc.ColumnElement as GridCell;
                if (columnElement!=null)
                    columnElement.GridCellRegion = "NormalCell";
                dc.GridColumn = this.DataGrid.Columns[ResolveToGridColumnIndex(index)];
                if (dc.ColumnVisibility == Visibility.Collapsed)
                    dc.ColumnVisibility = Visibility.Visible;
                dc.ColumnElement.ClearValue(FrameworkElement.DataContextProperty);
                //dc.ColumnElement.DataContext = this.RowData;
                dc.UpdateBinding(this.RowData);
            }
        }

        private SpannedDataColumn CreateColumn(CoveredCellInfo cc, int index,int heightIncrementation, int widthIncrementation)
        {
            var dc = new SpannedDataColumn
                {
                    ColumnIndex = index,
                    RowSpan = heightIncrementation,
                    ColumnSpan = widthIncrementation,  
                    // WPF-19238- if grid has no columns and summary row is added, need to set null for grid column
                    GridColumn = this.DataGrid.Columns.Count > 0 ? this.DataGrid.Columns[ResolveToGridColumnIndex(index)] : null,
                    RowIndex = this.RowIndex,
                    SelectionController = this.DataGrid.SelectionController
                };
            if (this.RowType == RowType.TableSummaryRow || this.RowType == RowType.TableSummaryCoveredRow)
                dc.Renderer = this.DataGrid.CellRenderers["TableSummary"];
            else if (this.RowType == RowType.CaptionRow || this.RowType == RowType.CaptionCoveredRow)
                dc.Renderer = this.DataGrid.CellRenderers["CaptionSummary"];
            else if (this.RowType == Grid.RowType.HeaderRow)
            {
                if (dc.RowIndex < this.DataGrid.GetHeaderIndex())
                {
                    dc.GridColumn = null;
                    dc.Renderer = this.DataGrid.CellRenderers["StackedHeader"];
                    this.RowData = this.DataGrid.StackedHeaderRows[this.RowIndex].StackedColumns.FirstOrDefault(col => col.HeaderText == cc.Name);
                }
                else
                    dc.Renderer = this.DataGrid.CellRenderers["Header"];
            }
            else //(this.RowType == RowType.SummaryRow || this.RowType == RowType.SummaryCoveredRow)
                dc.Renderer = this.DataGrid.CellRenderers["GroupSummary"];

            dc.InitializeColumnElement(this.RowData, false);
#if WPF
            if (DataGrid.useDrawing && dc.ColumnElement is GridCell)
                (dc.ColumnElement as GridCell).UseDrawing = DataGrid.useDrawing;
#endif
            return dc;
        }

        private void InvalidateMeasure()
        {
            Panel panel = this.WholeRowElement.ItemsPanel;
            if (this.WholeRowElement is CaptionSummaryRowControl)
                this.WholeRowElement.InvalidateMeasure();
            if (panel != null)
                panel.InvalidateMeasure();
#if WPF
            if (this.DataGrid.useDrawing)
                panel.InvalidateVisual();
#endif
        }

        private void CreateIndentInCoveredRow(int index)
        {
            var indentColumn = CreateIndentColumn(index);
            if (this.RowData is Group)            
                SetIndentColumnType(indentColumn);            
            else if (this.RowType == RowType.SummaryRow || this.RowType == RowType.SummaryCoveredRow)
            {
                int recordIndex = this.DataGrid.ResolveToRecordIndex(this.RowIndex);
                var isLastRow = this.DataGrid.RowGenerator.IsLastRow(recordIndex + 1);
                var nextEntry = this.DataGrid.View.TopLevelGroup.DisplayElements[recordIndex + 1];
                if (isLastRow)
                    indentColumn.IndentColumnType = IndentColumnType.InLastGroupRow;
                else if (nextEntry is Group) // LastGroupRow
                {
                    var groupStartIndex = nextEntry.Level - 1 + (this.DataGrid.ShowRowHeader ? 1 : 0);
                    indentColumn.IndentColumnType = index < groupStartIndex ? IndentColumnType.InSummaryRow : IndentColumnType.InLastGroupRow;
                }
                else
                    indentColumn.IndentColumnType = IndentColumnType.InSummaryRow;
            }
            else if (this.RowType == RowType.TableSummaryRow || this.RowType == RowType.TableSummaryCoveredRow)
                indentColumn.IndentColumnType = IndentColumnType.InTableSummaryRow;

            indentColumn.IsEnsured = true;
            this.VisibleColumns.Add(indentColumn);
        }

        private void SetIndentColumnType(DataColumnBase indentColumn)
        {
            var group = this.RowData as Group;
            if (group == null)
                return;

            int lastGroupLevel = -1;
            if (!group.IsExpanded)
            {
                bool isLastRow = this.DataGrid.RowGenerator.IsLastRow(group, this.RowIndex, ref lastGroupLevel);
                bool isLastGroup = this.DataGrid.RowGenerator.IsLastGroup(group);
                lastGroupLevel += (this.DataGrid.ShowRowHeader ? 1 : 0);
                if (indentColumn.ColumnIndex == ((this.Level + (this.DataGrid.ShowRowHeader ? 1 : 0)) - 1))
                {
                    indentColumn.IndentColumnType = IndentColumnType.InExpanderCollapsed;
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Collapsed)
                        indentColumn.ColumnVisibility = Visibility.Visible;
                }
                else if (indentColumn.ColumnIndex < (this.Level + (this.DataGrid.ShowRowHeader ? 1 : 0) - 1))
                {
                    indentColumn.IndentColumnType = indentColumn.ColumnIndex < lastGroupLevel ? (isLastRow ? IndentColumnType.InLastGroupRow : IndentColumnType.BeforeExpander) : (isLastGroup ? IndentColumnType.InLastGroupRow : IndentColumnType.BeforeExpander);
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Collapsed)
                        indentColumn.ColumnVisibility = Visibility.Visible;
                }
                else
                {
                    indentColumn.IndentColumnType = IndentColumnType.AfterExpander;
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Visible)
                        indentColumn.ColumnVisibility = Visibility.Collapsed;
                }
            }
            else
            {
                if (indentColumn.ColumnIndex == (this.Level + (this.DataGrid.ShowRowHeader ? 1 : 0) - 1))
                {
                    indentColumn.IndentColumnType = IndentColumnType.InExpanderExpanded;
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Collapsed)
                        indentColumn.ColumnVisibility = Visibility.Visible;
                }
                else if (indentColumn.ColumnIndex < (this.Level + (this.DataGrid.ShowRowHeader ? 1 : 0) - 1))
                {
                    indentColumn.IndentColumnType = IndentColumnType.BeforeExpander;
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Collapsed)
                        indentColumn.ColumnVisibility = Visibility.Visible;
                }
                else
                {
                    indentColumn.IndentColumnType = IndentColumnType.AfterExpander;
                    if (this.RowType == RowType.CaptionCoveredRow && indentColumn.ColumnElement.Visibility == Visibility.Visible)
                        indentColumn.ColumnVisibility = Visibility.Collapsed;
                }
            }
        }
        private bool CheckAvailablity(int index, bool forIndentColumn)
        {
            if (index >= this.DataGrid.VisualContainer.ColumnCount)
                return false;

            if (forIndentColumn && ((index < (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0)) || (this.DataGrid.ShowRowHeader && index <= (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0))))
            {
                if (index > this.RowLevel && (this.RowType == RowType.CaptionCoveredRow || this.RowType == RowType.SummaryCoveredRow || this.RowType == RowType.TableSummaryCoveredRow))
                {
                    return false;
                }
            }

            var cc = this.CoveredCells.FirstOrDefault(cell => cell.Left == index);

            DataColumnBase dataColumn = this.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == index);

            if (dataColumn != null)
            {
                if (dataColumn.ColumnVisibility == Visibility.Collapsed)
                    dataColumn.ColumnVisibility = Visibility.Visible;
                dataColumn.IsEnsured = true;
                var columnElement = dataColumn.ColumnElement as GridCell;
                if (columnElement != null)
                    columnElement.GridCellRegion="NormalCell";
                if (this.RowType!= Grid.RowType.CaptionRow && this.RowType!= Grid.RowType.SummaryRow && (dataColumn.ColumnElement is GridIndentCell) && dataColumn.IndentColumnType == IndentColumnType.AfterExpander)
                    dataColumn.ColumnVisibility = Visibility.Collapsed;
                return true;
            }
            else if (cc!=null && this.DataGrid.Columns.Count > 0)
            {
                var dc = CreateColumn(cc, index, cc.RowSpan, cc.Right - cc.Left);
                dc.IsEnsured = true;
                this.VisibleColumns.Add(dc);
            }
            else if (this.DataGrid.Columns.Count > 0 && this.RowRegion!= Grid.RowRegion.Header)
            {
                var dc = CreateColumn(null, index, 0, 0);
                dc.IsEnsured = true;
                this.VisibleColumns.Add(dc);
            }
            return false;
        }

        private int ResolveToGridColumnIndex(int index)
        {
            int resolvedIndex = 0;
            if (this.RowType == RowType.SummaryCoveredRow || this.RowType == RowType.CaptionCoveredRow || this.RowType == RowType.TableSummaryCoveredRow)
            {
                var group = this.RowData as Group;
                if (group != null)
                    resolvedIndex = index - group.Level - (this.DataGrid.ShowRowHeader ? 1 : 0);
            }
            else
                resolvedIndex = index - (this.DataGrid.View != null ? this.DataGrid.View.GroupDescriptions.Count : 0) - (this.DataGrid.DetailsViewDefinition.Count > 0 ? 1 : 0) - (this.DataGrid.ShowRowHeader ? 1 : 0);

            return resolvedIndex;
        }

        private void ResetLastColumnBorderThickness(DataColumnBase column, bool isLast)
        {
            if (column != null)
            {
                bool isTableSummaryRow = this.RowType == Grid.RowType.TableSummaryRow || this.RowType == Grid.RowType.TableSummaryCoveredRow;
                if (isLast)
                {
                        (column.ColumnElement as GridCell).GridCellRegion = "LastColumnCell";
                }
                else
                {
                    (column.ColumnElement as GridCell).GridCellRegion = "NormalCell";
                }
            }
        }

        private void IsExpandedChanged(bool isExpanded)
        {
            bool isExpandedStateChanged;
            if (isExpanded)
                isExpandedStateChanged = this.DataGrid.GridModel.ExpandGroup(this.RowData as Group);
            else
                isExpandedStateChanged = this.DataGrid.GridModel.CollapseGroup(this.RowData as Group);

            //this.dataGrid.UpdateRowCountAndScrollBars();
            if (isExpandedStateChanged)
                this.DataGrid.GridModel.RefreshDataRowForGroup(this.RowIndex, true);        
        }

        private void ApplyCaptionSummaryRowStyles(ContentControl row)
        {
            

            if (DataGrid == null || row == null)
                return;

            bool hasCaptionSummaryRowStyle = DataGrid.hasCaptionSummaryRowStyle;
            bool hasCaptionSummaryRowStyleSelector = DataGrid.hasCaptionSummaryRowStyleSelector;

            if (!hasCaptionSummaryRowStyleSelector && !hasCaptionSummaryRowStyle)
                return;

            Style newStyle = null;

            if (hasCaptionSummaryRowStyleSelector && hasCaptionSummaryRowStyle)
            {
                newStyle = DataGrid.CaptionSummaryRowStyleSelector.SelectStyle(this, row);
                newStyle = newStyle ?? DataGrid.CaptionSummaryRowStyle;             
            }
            else if (hasCaptionSummaryRowStyleSelector)
            {
                newStyle = DataGrid.CaptionSummaryRowStyleSelector.SelectStyle(this, row);
            }
            else if (hasCaptionSummaryRowStyle)
            {
                newStyle = DataGrid.CaptionSummaryRowStyle;
            }

            if (newStyle != null)
                row.Style = newStyle;
            else
                row.ClearValue(FrameworkElement.StyleProperty);
        }

        private void ApplyGroupSummaryRowStyles(ContentControl row)
        {

            if (DataGrid == null || row == null)
                return;

            bool hasGroupSummaryRowStyleSelector = DataGrid.hasGroupSummaryRowStyleSelector;
            bool hasGroupSummaryRowStyle = DataGrid.hasGroupSummaryRowStyle;

            if (!hasGroupSummaryRowStyleSelector && !hasGroupSummaryRowStyle)
                return;

            Style newStyle = null;

            if (hasGroupSummaryRowStyleSelector && hasGroupSummaryRowStyle)
            {
                newStyle = DataGrid.GroupSummaryRowStyleSelector.SelectStyle(this, row);
                newStyle = newStyle ?? DataGrid.GroupSummaryRowStyle;             
            }
            else if (hasGroupSummaryRowStyleSelector)
            {
                newStyle = DataGrid.GroupSummaryRowStyleSelector.SelectStyle(this, row);
            }
            else if (hasGroupSummaryRowStyle)
            {
                newStyle = DataGrid.GroupSummaryRowStyle;
            }

            if (newStyle != null)
                row.Style = newStyle;
            else
                row.ClearValue(FrameworkElement.StyleProperty);
        }

        private void ApplyTableSummaryRowStyles(ContentControl row)
        {

            if (DataGrid == null || row == null)
                return;

            bool hasTableSummaryRowStyleSelector = DataGrid.hasTableSummaryRowStyleSelector;
            bool hasTableSummaryRowStyle = DataGrid.hasTableSummaryRowStyle;
            if (!hasTableSummaryRowStyle && !hasTableSummaryRowStyleSelector)
                return;

            Style newStyle = null;
            if (hasTableSummaryRowStyleSelector && hasTableSummaryRowStyle)
            {
                newStyle = DataGrid.TableSummaryRowStyleSelector.SelectStyle(this, row);
                newStyle = newStyle ?? DataGrid.TableSummaryRowStyle;             
            }
            else if (hasTableSummaryRowStyleSelector)
            {
                newStyle = DataGrid.TableSummaryRowStyleSelector.SelectStyle(this, row);
            }
            else if (DataGrid.TableSummaryRowStyle != null)
            {
                newStyle = DataGrid.TableSummaryRowStyle;
            }

            if (newStyle != null)
                row.Style = newStyle;
            else
                row.ClearValue(FrameworkElement.StyleProperty);

        }

        private bool CheckForValidation()
        {
            return this.DataGrid.ValidationHelper.CheckForValidation(false);
        }

        #endregion

        #region Internal Methods

        internal void ApplyFixedRowVisualState(bool isfixed)
        {
            if (isfixed)
            {
                foreach (var cell in VisibleColumns.Select(column => column.ColumnElement as GridCell))
                {

                    if (cell.GridCellRegion != "LastColumnCell")
                    {
                        if (cell is GridIndentCell)
                        {
                            if ((cell as GridIndentCell).ColumnType == IndentColumnType.AfterExpander)
                                VisualStateManager.GoToState(cell, "Fixed_NormalCell", false);
                        }
                        else
                        {
#if WPF
                            if (DataGrid.useDrawing && cell is GridCaptionSummaryCell)
                                cell.GridCellRegion = "Fixed_NormalCell";
#endif
                            VisualStateManager.GoToState(cell, "Fixed_NormalCell", false);
                        }
                    }
                    else
                    {
#if WPF
                        if (DataGrid.useDrawing && cell is GridCaptionSummaryCell)
                            cell.GridCellRegion = "Fixed_LastCell";
#endif
                        VisualStateManager.GoToState(cell, "Fixed_LastCell", false);
                    }
                }
            }
            else
            {
                foreach (var cell in VisibleColumns.Select(column => column.ColumnElement as GridCell))
                {
                    if (cell is GridIndentCell)
                    {
                        var indentCell = cell as GridIndentCell;
                        indentCell.ApplyIndentVisualState(indentCell.ColumnType);
                    }
                    else
                    {
                        cell.ApplyGridCellVisualStates(cell.GridCellRegion);
                    }
                }
            }
        }

        private DataColumnBase CreateDetailsViewIndentColumn(int index)
        {
            DataColumnBase dc = new DataColumn();
            dc.IsEnsured = true;
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.IsEditing = false;
            dc.GridColumn = null;
            dc.SelectionController = this.DataGrid.SelectionController;
            var intentcell = new GridDetailsViewIndentCell();
            if (this.RowType != Grid.RowType.TableSummaryCoveredRow && this.RowType != Grid.RowType.TableSummaryRow)
                intentcell.ApplyVisualState("LastCell");
            dc.ColumnElement = intentcell;
            return dc;
        }

#endregion

#region IDisposable        

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SpannedDataRow"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (isDisposing)
            {
                if (this.coveredCells != null)
                {
                    this.coveredCells.Clear();
                    this.coveredCells = null;
                }
                this.GetCoveredColumnSize = null;
            }
            base.Dispose(isDisposing);
            isdisposed = true;
        }

#endregion
    }
}
