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
using Syncfusion.UI.Xaml.Grid.RowFilter;
using System.Linq;
#if WinRT || UNIVERSAL
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


namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class RowGenerator : IRowGenerator, IDisposable
    {
        #region Fields

        public SfDataGrid Owner { get; set; }
        internal bool ForceUpdateBinding;
        private List<DataRowBase> _Items = new List<DataRowBase>();
        private int lastFetcheSize = -1;
        private bool isdisposed = false;

        ////When CanResetDataContext is true for 1,00,000 records
        ////Grouping with one lakh records - 937
        ////Ungrouping with one lakh records - 485
        ////Adding one lakh records - 34025
        ////Remove all - 34
        ////Remove one lakh records - 19701

        ////When CanResetDataContext is false for 1,00,000 records
        ////Grouping with one lakh records - 837
        ////Ungrouping with one lakh records - 497
        ////Adding with one lakh records - 31819
        ////Remove all -55
        ////Remove one lakh records - 18525
        /// <summary>
        /// Denotes whether reset the DataContext for DataRow when it's going out of view. 
        /// </summary>
        public bool CanResetDataContext { get; set; }
        #endregion

        #region Property

        public ICollectionViewAdv View
        {
            get { return this.Owner.View; }
        }

        public List<DataRowBase> Items
        {
            get { return _Items; }
            set { _Items = value; }
        }

        #endregion

        #region Ctor

        public RowGenerator(SfDataGrid owner)
        {
            this.Owner = owner;
        }

        #endregion

        #region Virtual methods

        /// <summary>
        /// Get the GridCell.
        /// </summary>
        /// <typeparam name="T">Type of the cell.</typeparam>
        /// <returns>GridCell</returns>
        protected internal virtual GridCell GetGridCell<T>() where T : GridCell, new()
        {
            return new T()
            {
#if WPF                
                    UseDrawing = this.Owner.useDrawing
#endif
            };
        }

        /// <summary>
        /// Get the data row.
        /// </summary>
        /// <typeparam name="T">type of the row.</typeparam>
        /// <param name="rowType">Type of the row.</param>
        /// <returns>DataRow</returns>
        protected internal virtual GridDataRow GetDataRow<T>(RowType rowType) where T : GridDataRow, new()
        {
            return new T();
        }

        /// <summary>
        /// Get the row element for the corresponding data row.
        /// </summary>
        /// <typeparam name="T">type of the row.</typeparam>
        /// <returns>VirtualizingCellsControl</returns>
        protected internal virtual VirtualizingCellsControl GetVirtualizingCellsControl<T>() where T : VirtualizingCellsControl, new()
        {
            return new T();
        }

        #endregion

        #region Internal Methods
        /// <summary>
        /// Updates the Binding Information for the DataColumn, when Editor APIs are Changed
        /// </summary>
        /// <param name="dataContext">The data context.</param>
        internal void UpdateBinding(GridColumn gridColumn)
        {
            foreach (var item in Items)
            {
                if (item.RowIndex == -1)
                    continue;

#if WPF
                if (this.Owner.useDrawing && item.RowType != RowType.DetailsViewRow)
                    item.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif

                //WPF-26262 - GroupCaption text is not update while we changing the culture at runtime
                //GridColumn will be null from 2nd column due to SpannedDataRow so UpdateBinding method was not called and values not updated
                if (item.RowType == RowType.CaptionCoveredRow)
                {
                    item.VisibleColumns.ForEach(datacolumn =>
                    {
                        datacolumn.UpdateBinding(item.RowData, false);
                    });
                }
                else
                {
                    var dataColumn = item.VisibleColumns.FirstOrDefault(datacolumn => datacolumn.GridColumn == gridColumn);
                    if (dataColumn != null)
                        dataColumn.UpdateBinding(item.RowData, false);
                }
            }
        }
        #endregion

        #region Public methods

        public void OnItemSourceChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.Items.Count <= 0)
                return;

            if (args.OldValue != null)
                this.UnWireViewEvents();

            // If Owner is SfDataGrid (not DetailsViewDataGrid) or ReuseRowsOnItemssourceChange is false or  IsInDeserialize is true, DataRows are disposed and cleared from RowGenerator
            //if (this.Owner.IsInDeserialize || !this.Owner.ReuseRowsOnItemssourceChange)
            if (!(this.Owner is DetailsViewDataGrid) || this.Owner.IsInDeserialize || !this.Owner.ReuseRowsOnItemssourceChange || this.Owner.ItemsSource == null)
            {
                foreach (var item in Items)
                {
                    if (item is DetailsViewDataRow)
                    {
                        //WPF - 35440 When dispose the DetailsViewDataRow we need to remove the clonedDataGrid.
                        //ItemsSource for DetailsViewDataGrid set to null from GridCollectionViewWrapper.Dispose, when RecordEntry get Disposed.
                        var detailsViewDataGrid = (item as DetailsViewDataRow).DetailsViewDataGrid;
                        if (detailsViewDataGrid != null && detailsViewDataGrid.NotifyListener != null && detailsViewDataGrid.NotifyListener.SourceDataGrid != null && detailsViewDataGrid.NotifyListener.SourceDataGrid.NotifyListener != null)
                        {
                            var clonedDataGrid = (detailsViewDataGrid.NotifyListener.SourceDataGrid.NotifyListener as DetailsViewNotifyListener).ClonedDataGrid;
                            if (clonedDataGrid != null && clonedDataGrid.Contains(detailsViewDataGrid))
                                clonedDataGrid.Remove(detailsViewDataGrid);
                        }
                    }
                    item.Dispose();
                }
                this.Items.Clear();
                foreach (var cellRenderer in this.Owner.CellRenderers.Values)
                {
                    var renderer = cellRenderer as IGridCellRenderer;
                    if (renderer != null)
                        renderer.ClearRecycleBin();
                }
                foreach (var unBoundCellRenderer in this.Owner.UnBoundRowCellRenderers.Values)
                {
                    var renderer = unBoundCellRenderer as IGridCellRenderer;
                    if (renderer != null)
                        renderer.ClearRecycleBin();
                }
                foreach (var filterrenderer in this.Owner.FilterRowCellRenderers.Values)
                {
                    var renderer = filterrenderer as IGridCellRenderer;
                    if (renderer != null)
                        renderer.ClearRecycleBin();
                }
            }
            else
            {
                foreach (var item in Items)
                {
                    item.RowIndex = -1;
                    // To update the GridColumn, need to reset ColumnIndex
                    foreach (var column in item.VisibleColumns)
                    {
                        if (column.GridColumn != null)
                            column.ColumnIndex = -1;
                    }
                    if (item is DetailsViewDataRow)
                    {
                        (item as DetailsViewDataRow).CatchedRowIndex = -1;
                    }
                }
            }
            if (args.NewValue != null)
                this.WireViewEvents();
        }

        /// <summary>
        /// Refreshing StackedHeaders after the changes in GridColumn collection
        /// </summary>
        /// <remarks></remarks>
        public void RefreshStackedHeaders()
        {
            if (this.Owner.StackedHeaderRows != null && this.Owner.StackedHeaderRows.Count > 0)
            {
                foreach (var header in this.Owner.StackedHeaderRows)
                {
                    var sdr = (this.Items.FirstOrDefault(row => row.RowIndex == this.Owner.StackedHeaderRows.IndexOf(header)) as SpannedDataRow);
                    if (sdr != null)
                    {
                        sdr.CoveredCells.Clear();
                        sdr.VisibleColumns.ForEach(col =>
                        {
                            if (this.Owner.View != null && col.ColumnIndex >= this.Owner.View.GroupDescriptions.Count)
                                this.UnloadUIElements(sdr, col);
                        });
                        sdr.VisibleColumns.RemoveAll(col => this.Owner.View != null && col.ColumnIndex >= this.Owner.View.GroupDescriptions.Count);
                        CreateStackedCoveredCells(sdr, header, this.Owner.StackedHeaderRows.IndexOf(header));
                    }
                }
            }
        }

        #endregion

        //WPF_33924 Header Rows are not arranged properly while setting ShowRowHeader using binding. 
        //So the header row is refreshed here.
        internal void RefreshHeaders()
        {
            var headerRow = this.Items.FirstOrDefault(row => row.RowIndex == this.Owner.GetHeaderIndex()) as DataRow;
            if (headerRow != null)
                headerRow.WholeRowElement.ItemsPanel.InvalidateMeasure();
        }

        #region internal Properties

        internal VisualContainer Container
        {
            get { return this.Owner.VisualContainer; }
        }

        #endregion

        #region Private methods

        /// <summary>
        /// 为堆栈头创建CoveredCells
        /// Create CoveredCells for the StackedHeaders
        /// </summary>
        /// <param name="sdr"></param>
        /// <param name="header"></param>
        /// <param name="rowIndex"></param>
        /// <remarks></remarks>
        private void CreateStackedCoveredCells(SpannedDataRow sdr, StackedHeaderRow header, int rowIndex)
        {
            foreach (var column in header.StackedColumns)
            {
                int colIndex = header.StackedColumns.IndexOf(column);
                List<int> childSequence = this.Owner.GetChildSequence(column, rowIndex);
                childSequence.Sort();

                childSequence = childSequence.Except(this.Owner.IntersectedChildColumn(childSequence, header, column)).ToList();
                if (rowIndex - 1 >= 0)
                {
                    var newSequence = this.Owner.CheckChildSequence(childSequence, this.Owner.StackedHeaderRows[rowIndex - 1], column);
                    childSequence = newSequence.Intersect(childSequence).ToList();
                }

                column.ChildColumnsIndex = childSequence;

                var sequence = childSequence.GroupBy(num => childSequence.Where(candidate => candidate >= num)
                                      .OrderBy(candidate => candidate)
                                      .TakeWhile((candidate, index) => candidate == num + index)
                                      .Last())
                 .Select(seq => seq.OrderBy(num => num));
                foreach (var item in sequence)
                {
                    var columnList = item.ToList();
                    int right = columnList.Max() + this.Owner.ResolveToScrollColumnIndex(0);
                    int left = columnList.Min() + this.Owner.ResolveToScrollColumnIndex(0);
                    sdr.CoveredCells.Add(new CoveredCellInfo(column.HeaderText, left, right,0,0));
                }
            }
            if (rowIndex >= 0)
            {
                foreach (var currCell in sdr.CoveredCells)
                {
                    currCell.RowSpan = this.Owner.GetHeightIncrementationLimit(currCell, rowIndex - 1);
                }
            }
        }

        private void RemoveColumnbyIndex(DataRowBase row, int columnindex)
        {
            var datacolumn = row.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == columnindex);
            row.VisibleColumns.Remove(datacolumn);
        }

        internal void UnloadUIElements(DataRowBase row, DataColumnBase col)
        {
            if (col.Renderer != null)
                col.Renderer.UnloadUIElements(col);
            if (col.ColumnElement is GridHeaderCellControl)
                (col.ColumnElement as GridHeaderCellControl).Content = null;
            else if (col.ColumnElement is GridCell)
                (col.ColumnElement as GridCell).Content = null;
            else if (col.ColumnElement is DetailsViewContentPresenter)
                (col.ColumnElement as DetailsViewContentPresenter).Content = null;

            row.WholeRowElement.ItemsPanel.Children.Remove(col.ColumnElement);
        }

        private DataRowBase CreateHeaderRow(int rowIndex, VisibleLinesCollection visibleColumns)
        {
            if (Owner.AddNewRowPosition == AddNewRowPosition.FixedTop && rowIndex == this.Owner.HeaderLineCount - 1 && Owner.View != null)
            {
                var dr = CreateAddNewRow(rowIndex, visibleColumns, RowRegion.Header);
                if (dr.WholeRowElement is AddNewRowControl)
                {
                    dr.VisibleColumns.ForEach(col =>
                        {
                            if (col.ColumnElement is GridIndentCell)
                                (col.ColumnElement as GridIndentCell).ColumnType = IndentColumnType.InAddNewRow;
                        });
                }
                return dr;
            }
            else if(Owner.FilterRowPosition == FilterRowPosition.FixedTop && this.Owner.IsFilterRowIndex(rowIndex))
            {
                var dr = new FilterRow();
                if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                {
                    dr.RowVisibility = Visibility.Collapsed;
                }
                else
                {
                    dr.RowIndex = rowIndex;
                    dr.OnRowIndexChanged();
                    dr.DataGrid = this.Owner;
                    dr.RowRegion = RowRegion.Header;
                    dr.RowType = RowType.FilterRow;
                    dr.RowLevel = 0;
                    dr.InitializeDataRow(visibleColumns);
                }
                return dr;
            }
            else if (rowIndex == this.Owner.GetHeaderIndex())
            {
                var dr = new DataRow();
                if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                {
                    dr.RowVisibility = Visibility.Collapsed;
                }
                dr.RowIndex = rowIndex;
                dr.OnRowIndexChanged();
                dr.DataGrid = this.Owner;
                dr.RowRegion = RowRegion.Header;
                dr.RowType = RowType.HeaderRow;
                dr.RowLevel = 0;
                dr.InitializeDataRow(visibleColumns);
                return dr;
            }
            else if (rowIndex < this.Owner.StackedHeaderRows.Count)
            {
                var sdr = new SpannedDataRow() { RowIndex = rowIndex, GetCoveredColumnSize = GetCoveredColumnSize };
                var header = this.Owner.StackedHeaderRows[rowIndex];

                CreateStackedCoveredCells(sdr, header, rowIndex);

                sdr.RowData = header;
                sdr.RowRegion = RowRegion.Header;
                sdr.RowType = RowType.HeaderRow;
                sdr.RowLevel = 0;
                sdr.DataGrid = this.Owner;
                sdr.InitializeDataRow(visibleColumns);
                return sdr;
            }
            else if (rowIndex >= this.Owner.HeaderLineCount || (rowIndex < this.Owner.VisualContainer.FrozenRows && Owner.IsUnBoundRow(rowIndex)))
            {
                DataRowBase dr = null;
                dr = CreateDataRow(rowIndex, visibleColumns, RowRegion.Header);
                return dr;
            }
            else
            {
                DataRowBase dr = null;
                dr = MakeTableSummaryRow(dr, rowIndex, TableSummaryRowPosition.Top);
                dr.RowRegion = RowRegion.Header;
                dr.RowIndex = rowIndex;
                dr.OnRowIndexChanged();
                dr.InitializeDataRow(visibleColumns);
                if (dr.WholeRowElement is TableSummaryRowControl)
                {
                    dr.VisibleColumns.ForEach(col =>
                        {
                            if (col.ColumnElement is GridIndentCell)
                                (col.ColumnElement as GridIndentCell).ColumnType = IndentColumnType.InTableSummaryRow;
                        });
                }
                return dr;
            }

        }

        private DataRowBase CreateDataRow(int rowIndex, VisibleLinesCollection visibleColumns, RowRegion region = RowRegion.Body)
        {
            if (Owner.IsAddNewIndex(rowIndex))
            {
                return CreateAddNewRow(rowIndex, visibleColumns, region);
            }
            else if (Owner.UnBoundRows.Any() && Owner.IsUnBoundRow(rowIndex))
            {
                var dr = this.GetDataRow<UnBoundRow>(RowType.UnBoundRow);
                if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                {
                    dr.RowVisibility = Visibility.Collapsed;
                }
                else
                {
                    dr.RowIndex = rowIndex;
                    dr.OnRowIndexChanged();
                    dr.DataGrid = this.Owner;
                    dr.RowRegion = region;
                    dr.RowType = RowType.UnBoundRow;
                    dr.RowLevel = 0;
                    dr.InitializeDataRow(visibleColumns);

                    if (dr.DataGrid.CanQueryCoveredRange())
                    {
                        dr.GetMergedCellColumnSize = GetCoveredColumnSize;
                        dr.GetMergedCellRowSize = GetCoveredRowSize;                   
                    }
                    this.CheckForSelection(dr);
                }
                return dr;
            }
            else if(rowIndex == Owner.GetFilterRowIndex())
            {
                var dr = this.GetDataRow<FilterRow>(RowType.FilterRow);
                if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                {
                    dr.RowVisibility = Visibility.Collapsed;
                }
                else
                {
                    dr.RowIndex = rowIndex;
                    dr.OnRowIndexChanged();
                    dr.DataGrid = this.Owner;
                    dr.RowRegion = region;
                    dr.RowType = RowType.FilterRow;
                    dr.RowLevel = 0;
                    dr.InitializeDataRow(visibleColumns);
                    this.CheckForSelection(dr);
                }
                return dr;
            }
            else
            {
                // Create DetailsViewDataRow if rowIndex is DetailsView index
                if (this.Owner.IsInDetailsViewIndex(rowIndex))
                {
                    RecordEntry record = null;
                    record = this.Owner.DetailsViewManager.GetDetailsViewRecord(rowIndex);
                    var datarow = this.Items.FirstOrDefault(row => row.RowData == record.Data);
                    if (datarow != null && !datarow.IsExpanded)
                        datarow.IsExpanded = true;
                    // While calling ExpandAllDetailsView method, need to raise DetailsViewLoading event from here
                    var detailsViewDataRow = this.Owner.DetailsViewManager.CreateDetailsViewDataRow(rowIndex);
                    DetailsViewManager.RaiseDetailsViewEvents(detailsViewDataRow, Visibility.Visible);
                    return detailsViewDataRow;
                }

                if (this.View.GroupDescriptions.Count == 0)
                {
                    var dr = this.GetDataRow<DataRow>(RowType.DefaultRow);
                    if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                    {
                        dr.RowVisibility = Visibility.Collapsed;
                    }
                    else
                    {
                        var record = this.View.Records[this.Owner.ResolveToRecordIndex(rowIndex)];
                        dr.IsExpanded = record == null ? false : record.IsExpanded;
                        dr.RowIndex = rowIndex;
                        dr.DataGrid = this.Owner;
                        dr.RowRegion = region;
                        dr.RowType = RowType.DefaultRow;
                        dr.RowLevel = 0;
                        dr.RowData = record == null ? null : record.Data;
                        dr.OnRowIndexChanged();
                        dr.InitializeDataRow(visibleColumns);
                        
                        if(dr.DataGrid.CanQueryCoveredRange())
                        {
                            dr.GetMergedCellColumnSize = GetCoveredColumnSize;
                            dr.GetMergedCellRowSize = GetCoveredRowSize;            
                        }

                        this.CheckForSelection(dr);
                    }
                    return dr;
                }
                else
                {
                    var groupelement = this.View.TopLevelGroup.DisplayElements[this.Owner.ResolveToRecordIndex(rowIndex)];
                    if (groupelement is RecordEntry)
                    {
                        var dr = new DataRow();
                        var record = groupelement as RecordEntry;

                        var group = record == null ? null : record.Parent as Group;
                        if (group != null) dr.GroupRecordIndex = group.Records.IndexOf(record);
                        dr.RowIndex = rowIndex;
                        dr.IsExpanded = record == null ? false : record.IsExpanded;
                        dr.RowType = RowType.DefaultRow;
                        dr.DataGrid = this.Owner;
                        dr.RowRegion = region;
                        dr.RowLevel = record == null ? 0 : (record.Parent as Group).Level;
                        dr.RowData = record == null ? null : record.Data;
                        dr.OnRowIndexChanged();
                        dr.InitializeDataRow(visibleColumns);

                        if (dr.DataGrid.CanQueryCoveredRange())
                        {
                            dr.GetMergedCellColumnSize = GetCoveredColumnSize;
                            dr.GetMergedCellRowSize = GetCoveredRowSize;
                            dr.DataGrid.MergedCellManager.InitializeMergedRow(dr);
                        }
                        this.CheckForSelection(dr);
                        return dr;
                    }
                    else if (groupelement is SummaryRecordEntry)
                    {
                        var record = groupelement as SummaryRecordEntry;
                        var row = this.GetDataRow<SpannedDataRow>(RowType.SummaryRow);
                        var sdr = row as SpannedDataRow;
                        sdr.RowIndex = rowIndex;
                        sdr.OnRowIndexChanged();
                        sdr.GetCoveredColumnSize = GetCoveredColumnSize;                      

                        if ((groupelement as SummaryRecordEntry).SummaryRow.ShowSummaryInRow)
                        {
                            var cc = new CoveredCellInfo(record.Level + (this.Owner.ShowRowHeader ? 1 : 0), this.Owner.VisualContainer.ColumnCount,0,0);
                            sdr.CoveredCells.Add(cc);
                            sdr.RowType = RowType.SummaryCoveredRow;
                        }
                        else
                        {
                            var startIndex = this.Owner.View.GroupDescriptions.Count + (this.Owner.DetailsViewDefinition.Count > 0 ? 1 : 0) + (this.Owner.ShowRowHeader ? 1 : 0);

                            for (int i = startIndex; i < this.Owner.VisualContainer.ColumnCount; i++)
                            {
                                var cc = new CoveredCellInfo(i, i,0,0);
                                sdr.CoveredCells.Add(cc);
                            }
                            sdr.RowType = RowType.SummaryRow;
                        }
                        
                        sdr.RowLevel = record == null ? 0 : record.Level;
                        sdr.RowIndex = rowIndex;
                        sdr.RowRegion = region;
                        sdr.DataGrid = this.Owner;
                        sdr.RowData = groupelement;
                        sdr.OnRowIndexChanged();
                        sdr.InitializeDataRow(visibleColumns);
                        this.CheckForSelection(sdr);
                        return sdr;
                    }
                    else
                    {
                        var group = groupelement as Group;
                        var row = this.GetDataRow<SpannedDataRow>(RowType.CaptionRow);
                        var sdr = row as SpannedDataRow;
                        sdr.RowLevel = @group.Level;
                        sdr.RowIndex = rowIndex;
                        sdr.OnRowIndexChanged();
                        sdr.GetCoveredColumnSize = GetCoveredColumnSize;
                        if (this.Owner.CaptionSummaryRow == null || this.Owner.CaptionSummaryRow.ShowSummaryInRow)
                        {
                            var cc = new CoveredCellInfo(group.Level + (this.Owner.ShowRowHeader ? 1 : 0), this.Owner.VisualContainer.ColumnCount,0,0);
                            sdr.CoveredCells.Add(cc);
                            sdr.RowType = RowType.CaptionCoveredRow;
                        }
                        else
                        {
                            var startIndex = this.Owner.View.GroupDescriptions.Count + (this.Owner.DetailsViewDefinition.Count > 0 ? 1 : 0) + (this.Owner.ShowRowHeader ? 1 : 0);
                            for (int i = startIndex; i < this.Owner.VisualContainer.ColumnCount; i++)
                            {
                                var cc = new CoveredCellInfo(i, i,0,0);
                                sdr.CoveredCells.Add(cc);
                            }
                            sdr.RowType = RowType.CaptionRow;
                        }
                        sdr.RowIndex = rowIndex;
                        sdr.RowRegion = region;
                        sdr.DataGrid = this.Owner;
                        sdr.RowData = groupelement;
                        sdr.OnRowIndexChanged();
                        sdr.InitializeDataRow(visibleColumns);
                        this.CheckForSelection(sdr);
                        return sdr;
                    }
                }
            }
        }

        private DataRowBase CreateFooterRow(int rowIndex, VisibleLinesCollection visibleColumns)
        {
            DataRowBase dr = null;
            var footerCount = Owner.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);

            if (!Owner.IsTableSummaryIndex(rowIndex))
            {
                dr = CreateDataRow(rowIndex, visibleColumns, RowRegion.Footer);
            }
            else if (this.Owner.GetTableSummaryCount(TableSummaryRowPosition.Bottom) == 0)
            {
                dr = new DataRow() { DataGrid = this.Owner };
                if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                {
                    dr.RowVisibility = Visibility.Collapsed;
                }
#if WinRT || UNIVERSAL
                var record = this.View[this.Owner.ResolveToRecordIndex(rowIndex)] as RecordEntry;
#else
                var record = this.View.Records[this.Owner.ResolveToRecordIndex(rowIndex)] as RecordEntry;
#endif
                dr.RowIndex = rowIndex;
                dr.RowRegion = RowRegion.Footer;
                dr.RowData = record == null ? null : record.Data;
                dr.OnRowIndexChanged();
                dr.InitializeDataRow(visibleColumns);
            }
            else
            {
                dr = MakeTableSummaryRow(dr, rowIndex, TableSummaryRowPosition.Bottom);
                dr.RowIndex = rowIndex;
                dr.RowRegion = RowRegion.Footer;
                dr.OnRowIndexChanged();
                dr.InitializeDataRow(visibleColumns);

                if (dr.WholeRowElement is TableSummaryRowControl)
                {
                    if (this.Owner.VisualContainer.FooterRows == 1)
                        dr.WholeRowElement.RowBorderState = "FooterRow";

                    dr.VisibleColumns.ForEach(col =>
                    {
                        if (col.ColumnElement is GridIndentCell)
                            (col.ColumnElement as GridIndentCell).ColumnType = IndentColumnType.InTableSummaryRow;
                    });
                }
            }
            return dr;
        }

        private DataRowBase MakeTableSummaryRow(DataRowBase dr, int rowIndex, TableSummaryRowPosition position)
        {
            if (dr == null)
            {
                dr = this.GetDataRow<SpannedDataRow>(RowType.TableSummaryRow);
                (dr as SpannedDataRow).RowIndex = rowIndex;
                (dr as SpannedDataRow).OnRowIndexChanged();
                (dr as SpannedDataRow).DataGrid = this.Owner;
            }

            (dr as SpannedDataRow).GetCoveredColumnSize = GetCoveredColumnSize;
            if (this.View != null)
            {
                var record = this.View.Records.TableSummaries.FirstOrDefault(rec => rec.SummaryRow.Equals(GetSummaryRow(rowIndex, position)));
                (dr as SpannedDataRow).CoveredCells.Clear();
                if (record.SummaryRow.ShowSummaryInRow)
                {
                    var cc = new CoveredCellInfo(this.Owner.ResolveToScrollColumnIndex(0), this.Owner.VisualContainer.ColumnCount,0,0);                    
                    (dr as SpannedDataRow).CoveredCells.Add(cc);
                    dr.RowData = record;
                    dr.RowType = RowType.TableSummaryCoveredRow;
                }
                else
                {
                    var startIndex = this.Owner.View.GroupDescriptions.Count + (this.Owner.DetailsViewDefinition.Count > 0 ? 1 : 0) + (this.Owner.ShowRowHeader ? 1 : 0);
                    for (int i = startIndex; i < this.Owner.VisualContainer.ColumnCount; i++)
                    {
                        var cc = new CoveredCellInfo(i, i,0,0);
                        (dr as SpannedDataRow).CoveredCells.Add(cc);
                    }
                    dr.RowData = record;
                    dr.RowType = RowType.TableSummaryRow;
                }
            }
            return dr;
        }

        private DataRowBase CreateAddNewRow(int rowIndex, VisibleLinesCollection visibleColumns, RowRegion region)
        {
            var dr = this.GetDataRow<DataRow>(RowType.AddNewRow);
            dr.DataGrid = this.Owner;
            if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
            {
                dr.RowVisibility = Visibility.Collapsed;
            }
            dr.RowIndex = rowIndex;
            dr.RowLevel = 0;
            dr.RowRegion = region;
            dr.RowType = RowType.AddNewRow;
            if (View != null && View.IsAddingNew)
                dr.RowData = View.CurrentAddItem;
            dr.OnRowIndexChanged();

            if (rowIndex == Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                dr.IsSelectedRow = true;
            
            dr.InitializeDataRow(visibleColumns);
            return dr;
        }     
        internal void RemoveStackedHeader(int stackedHeaderRowIndex = -1)
        {
            if (stackedHeaderRowIndex != -1)
            {
                //UWP - 5190 Get the spannedDataRow based on RemoveAt index
                SpannedDataRow sdr = this.Items.FirstOrDefault(row => (row.RowIndex == stackedHeaderRowIndex) && (row.RowType == RowType.HeaderRow)) as SpannedDataRow;
                if (sdr != null)
                    ResetRowIndex(sdr);
            }             
            else
            {
                for (int rowindex = 0; rowindex < this.Owner.GetHeaderIndex(); rowindex++)
                {
                    //UWP - 5190 Get the spannedDataRow based on 0 index because we have reset the rowIndex after removed the StackedHeaderRow.                    
                    SpannedDataRow sdr = this.Items.FirstOrDefault(row => (row.RowIndex == 0) && (row.RowType == RowType.HeaderRow)) as SpannedDataRow;
                    if (sdr != null)
                        ResetRowIndex(sdr);
                }                  
            }
        }

        internal void ResetRowIndex(SpannedDataRow sdr)
        {
            //UWP - 5190 while clear the StackedHeaderRows, we need to reset the RowIndex.                   
            this.Items.ForEach(row =>
            {
                if (row.RowIndex > sdr.RowIndex)
                {
                    row.RowIndex -= 1;
                    var visibleColumns = row.VisibleColumns;
                    foreach (var visibleColumn in visibleColumns)
                        visibleColumn.RowIndex -= 1;
                }
            });
            RemoveSpannedRow(sdr);
            this.Items.Remove(sdr);
        }

        private void RemoveSpannedRow(SpannedDataRow sdr)
        {
            if (this.Owner.View == null)
                return;
            sdr.CoveredCells.Clear();
            sdr.VisibleColumns.ForEach(col =>
            {
                if (col.ColumnIndex >= this.Owner.View.GroupDescriptions.Count)
                    this.UnloadUIElements(sdr, col);
            });
            sdr.VisibleColumns.RemoveAll(col => col.ColumnIndex >= this.Owner.View.GroupDescriptions.Count);
            sdr.WholeRowElement.Dispose();
            this.Container.Children.Remove(sdr.WholeRowElement);                      
        }

        private void Updatebinding(DataRow dr)
        {
            dr.VisibleColumns.ForEach(col =>
            {
                if (col.GridColumn != null)
                {
                    col.UpdateBinding(dr.RowData, false);
                }
            });
        }

        internal void UpdateSelectionController()
        {
            foreach (var item in Items)
            {
                item.VisibleColumns.ForEach(col => col.SelectionController = this.Owner.SelectionController);
            }
        }

        private void UpdateDataRow(IEnumerable<DataRowBase> rows, int rowIndex, RowRegion region)
        {
            var isQueryCoveredRangeEventWired = this.Owner.CanQueryCoveredRange();
            if (Owner.IsAddNewIndex(rowIndex))
            {
                if (Items.Any(row => row.RowType == RowType.AddNewRow))
                {
                    var dr = Items.FirstOrDefault(row => row.RowType == RowType.AddNewRow) as DataRow;
                    dr.RowIndex = rowIndex;
                    dr.OnRowIndexChanged();
                }
                else
                {
                    var dr = CreateAddNewRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                    this.Items.Add(dr);
                }
            }
            else if (Owner.IsUnBoundRow(rowIndex))
            {
                rows = rows.Where(item => item.RowType == RowType.UnBoundRow);
                if (rows.Any())
                {
                    var dr = rows.FirstOrDefault(row => row.RowType == RowType.UnBoundRow) as UnBoundRow;
                    var visibleColumns = this.Container.ScrollColumns.GetVisibleLines();
                    dr.RowIndex = rowIndex;
                    dr.OnRowIndexChanged();
                    if (dr.isDirty)
                    {
                        // when do invalidate unbound data row, IsDirty property will set to true. based on that will again doing updation.
                        dr.EnsureColumns(visibleColumns);
                        dr.isDirty = false;
                    }
                    else
                    {
                        //Procees of Updating UIElement - While reusing existing UnBoundDataRow to a another UnBoundDataRow, we need to update the UIElement.
                        dr.VisibleColumns.ForEach(
                            item =>
                            {
                                if (!item.IsExpanderColumn && !item.IsIndentColumn && !(item.ColumnElement is GridRowHeaderCell))
                                {
                                    item.ColumnIndex = -1;
                                    item.IsEnsured = false;
                                }
                            });
                        dr.EnsureColumns(visibleColumns);                        
                    }

                    if (dr.RowVisibility == Visibility.Collapsed)
                        dr.RowVisibility = Visibility.Visible;

                    if(region == RowRegion.Body)
                    {
                        if (isQueryCoveredRangeEventWired)
                        {
                            dr.GetMergedCellColumnSize = GetCoveredColumnSize;
                            dr.GetMergedCellRowSize = GetCoveredRowSize;
                            this.Owner.MergedCellManager.InitializeMergedRow(dr);                          
                        }
                    }
                    this.CheckForSelection(dr);
                    dr.ApplyRowHeaderVisualState();
                    dr.WholeRowElement.UpdateRowBackgroundClip();
                }
                else
                {
                    var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                    this.Items.Add(dr);
                    
                }                
            }
            else if(rowIndex == Owner.GetFilterRowIndex())
            {
                if (Items.Any(row => row.RowType == RowType.FilterRow))
                {
                    var dr = Items.FirstOrDefault(row => row.RowType == RowType.FilterRow);
                    dr.RowIndex = rowIndex;
                    dr.RowLevel = 0;
                    dr.OnRowIndexChanged();
                    this.CheckForSelection(dr);
                }
                else
                {
                    var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                    this.Items.Add(dr);
                }
            }
            else
            {
                if (this.View.GroupDescriptions.Count == 0)
                {
                    if (rows.Any(row => row.RowType == RowType.DefaultRow))
                    {
                        var dr = rows.FirstOrDefault(row => row.RowType == RowType.DefaultRow) as DataRow;
                        
                        if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                        {
                            dr.RowVisibility = Visibility.Collapsed;
                        }
                        else
                        {
                            var record = this.View.Records[this.Owner.ResolveToRecordIndex(rowIndex)];
                            if (isQueryCoveredRangeEventWired && dr.IsSpannedRow)
                            {
                                this.Owner.MergedCellManager.ResetCoveredRows(dr);
                                dr.WholeRowElement.ItemsPanel.InvalidateMeasure();
                                dr.WholeRowElement.ItemsPanel.InvalidateArrange();
#if WPF
                                if(this.Owner.useDrawing)
                                    dr.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                            }
                            // WPF-27647 - Need to update rowindex before changing its record while reusing. 
                            //Because in datacontext changed event of customized gridcell, need to made conditions based on rowindex.
                            dr.RowIndex = rowIndex;
                            dr.RowLevel = 0;
                            dr.RowRegion = region;
                            dr.RowData = record == null ? null : record.Data;
                            dr.OnRowIndexChanged();
                            if (this.ForceUpdateBinding)
                                this.Updatebinding(dr);
                            dr.IsExpanded = record == null ? false : record.IsExpanded;

                            if (dr.RowVisibility == Visibility.Collapsed)
                                dr.RowVisibility = Visibility.Visible;
                            this.CheckForSelection(dr);

                            if (this.Owner.Columns.Any(col => col.GridValidationMode != GridValidationMode.None))
                            {
                                this.Owner.ValidationHelper.ValidateColumns(dr);
                            }
                            dr.ApplyRowHeaderVisualState();
                            dr.WholeRowElement.UpdateRowBackgroundClip();

                            if (isQueryCoveredRangeEventWired)
                            {
                                dr.GetMergedCellColumnSize = GetCoveredColumnSize;
                                dr.GetMergedCellRowSize = GetCoveredRowSize;
                                this.Owner.MergedCellManager.InitializeMergedRow(dr);                            

                                // while reuse vertically up/down, the merged row can be uses by another normal or merged row. if covered range false with that column index can be collapsed. need to set the visibility for that.
                                if (dr.IsSpannedRow)
                                    this.Owner.MergedCellManager.RefreshMergedRows(dr);
                            }
                        }
                    }
                    else
                    {
                        var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                        this.Items.Add(dr);
                        if (isQueryCoveredRangeEventWired)
                        {
                            if (dr.IsSpannedRow)
                                this.Owner.MergedCellManager.RefreshMergedRows(dr);
                        }
                    }
                }
                else
                {
                    var groupelement = this.View.TopLevelGroup.DisplayElements[this.Owner.ResolveToRecordIndex(rowIndex)];

                    if (groupelement is RecordEntry)
                    {
                        if (rows.Any(row => row.RowType == RowType.DefaultRow))
                        {
                            var dr = rows.First(row => row.RowType == RowType.DefaultRow) as DataRow;
                            var record = groupelement as RecordEntry;

                            if(isQueryCoveredRangeEventWired && dr.IsSpannedRow)
                            {
                                this.Owner.MergedCellManager.ResetCoveredRows(dr);
                                dr.WholeRowElement.ItemsPanel.InvalidateMeasure();
                                dr.WholeRowElement.ItemsPanel.InvalidateArrange();
#if WPF
                                if(this.Owner.useDrawing)
                                    dr.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                            }

                            dr.RowIndex = rowIndex;
                            dr.RowData = record == null ? null : record.Data;
                            var group = record == null ? null : record.Parent as Group;
                            if (group != null) dr.GroupRecordIndex = group.Records.IndexOf(record);
                            dr.RowLevel = record == null ? 0 : (record.Parent as Group).Level;
                            dr.OnRowIndexChanged();
                            if (this.ForceUpdateBinding)
                                this.Updatebinding(dr);
                            dr.IsExpanded = record == null ? false : record.IsExpanded;
                            if (dr.RowVisibility == Visibility.Collapsed)
                                dr.RowVisibility = Visibility.Visible;
                            this.CheckForSelection(dr);
                            UpdateIndentCells(dr, record);
                            dr.UpdateUnBoundColumn();
                            if (this.Owner.Columns.Any(col => col.GridValidationMode != GridValidationMode.None))
                            {
                                this.Owner.ValidationHelper.ValidateColumns(dr);
                            }
                            dr.ApplyRowHeaderVisualState();
                            dr.WholeRowElement.UpdateRowBackgroundClip();

                            if (isQueryCoveredRangeEventWired)
                            {
                                this.Owner.MergedCellManager.InitializeMergedRow(dr);                               
                                if (dr.IsSpannedRow)
                                    this.Owner.MergedCellManager.RefreshMergedRows(dr);
                            }
                        }
                        else
                        {
                            var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                            this.Items.Add(dr);

                            if (isQueryCoveredRangeEventWired)
                            {
                                if (dr.IsSpannedRow)
                                    this.Owner.MergedCellManager.RefreshMergedRows(dr);
                            }
                        }
                    }
                    else if (groupelement is SummaryRecordEntry)
                    {
                        if (rows.Any(row => row.RowType == RowType.SummaryCoveredRow || row.RowType == RowType.SummaryRow))
                        {
                            var record = groupelement as SummaryRecordEntry;
                            SpannedDataRow dr;

                            if (record.SummaryRow.ShowSummaryInRow)
                            {
                                dr = rows.FirstOrDefault(row => row.RowType == RowType.SummaryCoveredRow) as SpannedDataRow;
                                this.UpdateCoveredRow(dr, record);
                            }
                            else
                                dr = rows.FirstOrDefault(row => row.RowType == RowType.SummaryRow) as SpannedDataRow;

                            if (dr == null)
                            {
                                dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines()) as SpannedDataRow;
                                this.UpdateIndentCells(dr, record);
                                this.Items.Add(dr);
                                return;
                            }
                            var needToEnsure = dr.RowIndex != rowIndex;
                            dr.RowIndex = rowIndex;
                            dr.RowLevel = record == null ? 0 : (record.Parent as Group).Level;
                            dr.RowData = groupelement;
                            dr.OnRowIndexChanged();
                            if (dr.RowVisibility == Visibility.Collapsed)
                                dr.RowVisibility = Visibility.Visible;
                            if (needToEnsure)
                                dr.EnsureColumns(this.Owner.VisualContainer.ScrollColumns.GetVisibleLines());
                            this.CheckForSelection(dr);
                            this.UpdateIndentCells(dr, record);
                            dr.ApplyRowHeaderVisualState();
                            dr.WholeRowElement.UpdateRowBackgroundClip();
                        }
                        else
                        {
                            var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                            this.Items.Add(dr);
                        }
                    }
                    else
                    {
                        if (rows.Any(row => row.RowType == RowType.CaptionRow || row.RowType == RowType.CaptionCoveredRow))
                        {
                            var group = groupelement as Group;
                            SpannedDataRow dr;
                            if (this.Owner.CaptionSummaryRow == null || this.Owner.CaptionSummaryRow.ShowSummaryInRow)
                            {
                                dr = rows.FirstOrDefault(row => row.RowType == RowType.CaptionCoveredRow) as SpannedDataRow;
                                this.UpdateCoveredRow(dr, group);
                            }
                            else
                                dr = rows.FirstOrDefault(row => row.RowType == RowType.CaptionRow) as SpannedDataRow;

                            if (dr == null)
                            {
                                DataRowBase sdr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines());
                                this.Items.Add(sdr);
                                return;
                            }
                            var needToEnsure = dr.RowLevel != 0 && dr.RowLevel != group.Level;
                            dr.RowLevel = group.Level;
                            dr.RowIndex = rowIndex;
                            dr.RowData = groupelement;
                            dr.OnRowIndexChanged();
                            if (needToEnsure)
                                dr.EnsureColumns(this.Owner.VisualContainer.ScrollColumns.GetVisibleLines());
                            this.UpdateGroupExpander(dr, group);
                            this.UpdateIndentCells(dr, group);
                            if (dr.RowVisibility == Visibility.Collapsed)
                                dr.RowVisibility = Visibility.Visible;
                            this.CheckForSelection(dr);
                            dr.WholeRowElement.Clip = null;

                            dr.ApplyRowHeaderVisualState();
                            dr.WholeRowElement.UpdateRowBackgroundClip();
                        }
                        else
                        {
                            var dr = CreateDataRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines(), region);
                            this.Items.Add(dr);
                        }
                    }
                }
            }
        }

        private void UpdateRow(IEnumerable<DataRowBase> rows, int rowIndex, RowRegion region)
        {
            if (region == RowRegion.Header)
            {
                if (rowIndex == this.Owner.GetHeaderIndex())
                {
                    DataRowBase dr = rows.FirstOrDefault(r => r.RowType == RowType.HeaderRow && !(r is SpannedDataRow));                    
                    if (dr != null)
                    {
                        if (rowIndex < 0 || rowIndex >= this.Owner.VisualContainer.ScrollRows.LineCount)
                            dr.RowVisibility = Visibility.Collapsed;

                        dr.RowIndex = rowIndex;
                        dr.OnRowIndexChanged();
                        (dr as GridDataRow).DataGrid = this.Owner;
                        dr.RowRegion = RowRegion.Header;
                        dr.RowType = RowType.HeaderRow;
                        if (this.Owner.HeaderLineCount > 1)
                        {
                            foreach (var column in this.Owner.Columns)
                            {
                                var index = this.Owner.ResolveToScrollColumnIndex(this.Owner.Columns.IndexOf(column));
                                var visibleColumn = dr.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == index);
                                if (visibleColumn != null)
                                    visibleColumn.RowSpan = this.Owner.GetHeightIncrementationLimit(new CoveredCellInfo(index, index,0,0), rowIndex - 1);
                            }
                        }
                        else
                            dr.VisibleColumns.ForEach(column => column.RowSpan = 0);
                        dr.WholeRowElement.UpdateRowBackgroundClip();
                        dr.WholeRowElement.ItemsPanel.InvalidateMeasure();
#if WPF
                        if(this.Owner.useDrawing)
                            dr.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                    }
                    else
                    {
                        var row = CreateHeaderRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines());
                        this.Items.Add(row);
                    }
                }
                else if (rowIndex < this.Owner.StackedHeaderRows.Count)
                {
                    var sdr = rows.FirstOrDefault(r => r.RowType == RowType.HeaderRow) as SpannedDataRow;
                    var header = this.Owner.StackedHeaderRows[rowIndex];
                    if (sdr != null)
                    {
                        RemoveSpannedRow(sdr);
                        CreateStackedCoveredCells(sdr, header, rowIndex);
                        sdr.RowIndex = rowIndex;
                        sdr.RowRegion = RowRegion.Header;
                        sdr.DataGrid = this.Owner;
                        sdr.RowData = header;
                        sdr.RowLevel = 0;
                        sdr.OnRowIndexChanged();
                        sdr.InitializeDataRow(Container.ScrollColumns.GetVisibleLines());
                        sdr.WholeRowElement.UpdateRowBackgroundClip();
                    }
                    else
                    {
                        sdr = CreateHeaderRow(rowIndex, Container.ScrollColumns.GetVisibleLines()) as SpannedDataRow;
                        this.Items.Add(sdr);
                    }
                }
                else
                {
                    if (Owner.IsUnBoundRow(rowIndex) || Owner.IsAddNewIndex(rowIndex) ||
                        Owner.IsFilterRowIndex(rowIndex) || rowIndex >= Owner.HeaderLineCount)
                    {
                        UpdateDataRow(rows, rowIndex, region);
                    }
                    else
                    {
                        var record = this.View.Records.TableSummaries.FirstOrDefault(rec => rec.SummaryRow.Equals(GetSummaryRow(rowIndex, TableSummaryRowPosition.Top)));
                        var rowtype = record.SummaryRow.ShowSummaryInRow ? RowType.TableSummaryCoveredRow : RowType.TableSummaryRow;
                        var dr = rows.FirstOrDefault(row => row.RowType == rowtype) as SpannedDataRow;
                        if (dr == null)
                        {
                            var datarow = CreateHeaderRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines());
                            this.Items.Add(datarow);
                            return;
                        }
                        var needToEnsure = dr.RowIndex != rowIndex;
                        MakeTableSummaryRow(dr, rowIndex, TableSummaryRowPosition.Top);
                        dr.RowIndex = rowIndex;
                        dr.RowData = record;
                        dr.OnRowIndexChanged();
                        //WPF-33611 - Columns are not ensured when reusing the DetailsViewDataGrid on Scrolling. Hence the below code is added.
                        if (needToEnsure)
                            dr.EnsureColumns(this.Owner.VisualContainer.ScrollColumns.GetVisibleLines());
                    }
                }
            }

            else if (region != RowRegion.Footer || Owner.IsUnBoundRow(rowIndex))
            {
                UpdateDataRow(rows, rowIndex, region);
            }
            else
            {
                var footerCount = Owner.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);//, RowRegion.Footer);
                var startTableSummaryIndex = Owner.VisualContainer.RowCount - (Owner.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount);
                if (rowIndex >= startTableSummaryIndex)
                {
                    var record = this.View.Records.TableSummaries.FirstOrDefault(rec => rec.SummaryRow.Equals(GetSummaryRow(rowIndex, TableSummaryRowPosition.Bottom)));
                    var rowtype = record.SummaryRow.ShowSummaryInRow ? RowType.TableSummaryCoveredRow : RowType.TableSummaryRow;
                    SpannedDataRow dr = rows.FirstOrDefault(row => row.RowType == rowtype) as SpannedDataRow;
                    if (dr == null)
                    {
                        var datarow = CreateFooterRow(rowIndex, this.Container.ScrollColumns.GetVisibleLines());
                        this.Items.Add(datarow);
                        return;
                    }
                    var needToEnsure = dr.RowIndex != rowIndex;
                    dr = MakeTableSummaryRow(dr, rowIndex, TableSummaryRowPosition.Bottom) as SpannedDataRow;
                    dr.RowIndex = rowIndex;
                    dr.RowData = record;
                    dr.OnRowIndexChanged();
                    //WPF-33611 - Columns are not ensured when reusing the DetailsViewDataGrid on Scrolling. Hence the below code is added.
                    if (needToEnsure)
                        dr.EnsureColumns(this.Owner.VisualContainer.ScrollColumns.GetVisibleLines());
                }
                else
                {
                    UpdateDataRow(rows, rowIndex, region);
                }
            }
        }

        /// <summary>
        /// Method which helps to update the expander position and expanded state
        /// </summary>
        /// <param name="row"></param>
        /// <param name="group"></param>
        /// <remarks></remarks>
        private void UpdateGroupExpander(SpannedDataRow row, Group group)
        {
            var captionRow = row.WholeRowElement as CaptionSummaryRowControl;
            captionRow.IsExpanded = group.IsExpanded;
            captionRow.InvalidateMeasure();
            captionRow.ItemsPanel.InvalidateMeasure();
#if WPF
            if(this.Owner.useDrawing)
                captionRow.ItemsPanel.InvalidateVisual();
#endif
        }

        private void UpdateIndentCells(DataRowBase row, object dataContext)
        {
            var indentCells = row.VisibleColumns.FindAll(col => col.IsIndentColumn);
            var visiblelines = this.Owner.VisualContainer.ScrollColumns.GetVisibleLines();            

            if (row.RowType == RowType.AddNewRow)
            {
                foreach (var item in indentCells)
                {
                    if (!visiblelines.Any(column => column.LineIndex == item.ColumnIndex))
                        continue;
                    item.IndentColumnType = IndentColumnType.InAddNewRow;
                }
                return;
            }

            // WPF-29757 If we edit and commit the cell after grouping the IndentCells is updated but the DataContext 
            //for FilterRow is null so we need to return if the row is FilerRow
            else if (row.RowType == RowType.FilterRow)
            {
                foreach (var item in indentCells)
                {
                    if (!visiblelines.Any(column => column.LineIndex == item.ColumnIndex))
                        continue;
                    item.IndentColumnType = IndentColumnType.InFilterRow;
                }
                return;
            }

            if (row.RowData is Group)
            {
                var group = row.RowData as Group;
                int lastGroupLevel = -1;
                if (!group.IsExpanded)
                {
                    bool isLastRow = this.IsLastRow(group, row.RowIndex, ref lastGroupLevel);
                    bool isLastGroup = IsLastGroup(group);
                    lastGroupLevel += (this.Owner.ShowRowHeader ? 1 : 0);
                    foreach(var cell in indentCells)
                    {
                        if (cell.ColumnIndex < 0)
                            continue;

                        if (!visiblelines.Any(column => column.LineIndex == cell.ColumnIndex))
                            continue;

                        if (cell.ColumnIndex == (row.Level + (this.Owner.ShowRowHeader ? 1 : 0) - 1))
                        {
                            cell.IndentColumnType = IndentColumnType.InExpanderCollapsed;
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Collapsed)
                                cell.ColumnVisibility = Visibility.Visible;
                        }
                        else if (cell.ColumnIndex < (row.Level + (this.Owner.ShowRowHeader ? 1 : 0) - 1))
                        {
                            cell.IndentColumnType = cell.ColumnIndex < lastGroupLevel ? (isLastRow ? IndentColumnType.InLastGroupRow : IndentColumnType.BeforeExpander) : (isLastGroup ? IndentColumnType.InLastGroupRow : IndentColumnType.BeforeExpander);
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Collapsed)
                                cell.ColumnVisibility = Visibility.Visible;
                        }
                        else
                        {
                            cell.IndentColumnType = IndentColumnType.AfterExpander;
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Visible)
                                cell.ColumnVisibility = Visibility.Collapsed;
                        }
                    }
                }
                else
                {
                    foreach(var cell in indentCells)
                    {
                        if (cell.ColumnIndex < 0)
                            continue;

                        if (!visiblelines.Any(column => column.LineIndex == cell.ColumnIndex))
                            continue;

                        if (cell.ColumnIndex == (row.Level + (this.Owner.ShowRowHeader ? 1 : 0) - 1))
                        {
                            cell.IndentColumnType = IndentColumnType.InExpanderExpanded;
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Collapsed)
                                cell.ColumnVisibility = Visibility.Visible;
                        }
                        else if (cell.ColumnIndex < (row.Level + (this.Owner.ShowRowHeader ? 1 : 0) - 1))
                        {
                            cell.IndentColumnType = IndentColumnType.BeforeExpander;
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Collapsed)
                                cell.ColumnVisibility = Visibility.Visible;
                        }
                        else
                        {
                            cell.IndentColumnType = IndentColumnType.AfterExpander;
                            if (row.RowType == RowType.CaptionCoveredRow && cell.ColumnElement.Visibility == Visibility.Visible)
                                cell.ColumnVisibility = Visibility.Collapsed;
                        }
                    };
                }
            }
            else if (row.RowData is SummaryRecordEntry)
            {
                int recordIndex = this.Owner.ResolveToRecordIndex(row.RowIndex);
                bool isLastRow = this.IsLastRow(recordIndex + 1);
                var nextEntry = this.View.TopLevelGroup.DisplayElements[recordIndex + 1];
                if (isLastRow)
                {
                    indentCells.ForEach(cell => { cell.IndentColumnType = IndentColumnType.InLastGroupRow; });
                }
                else if (nextEntry is Group) // LastGroupRow
                {
                    var groupStartIndex = nextEntry.Level - 1 + (this.Owner.ShowRowHeader ? 1 : 0);
                    indentCells.ForEach(cell =>
                    {
                        cell.IndentColumnType = cell.ColumnIndex < groupStartIndex ?
                            IndentColumnType.InDataRow : IndentColumnType.InLastGroupRow;
                    });
                }
                else
                    indentCells.ForEach(cell => { cell.IndentColumnType = IndentColumnType.InSummaryRow; });
            }
            else if(row.RowType != RowType.HeaderRow)
            {
                var record = dataContext as RecordEntry;
                var group = record == null ? null : record.Parent as Group;
                int lastGroupLevel = -1;
                NodeEntry lastRecord = group.Records[group.Records.Count - 1];
                bool isLastRow = false;
                bool isLastGroupRow = lastRecord.Equals(record);
                bool isLastGroup = (group.Parent != null && group.Parent is Group) ? group.Equals((group.Parent as Group).Groups.LastOrDefault()) : false;
                if (isLastGroupRow)
                    isLastRow = this.IsLastRow(record, row.RowIndex, ref lastGroupLevel);
                if (isLastRow)
                {
                    indentCells.ForEach(cell => { cell.IndentColumnType = IndentColumnType.InLastGroupRow; });
                }
                else if (isLastGroupRow && this.Owner.GroupSummaryRows.Count == 0)
                {
                    indentCells.ForEach(cell =>
                    {
                        //Same code needed to use in DataRow EnusuringLastRow method, hence used the seperate method.
                        this.CheckIsLastRow(cell, lastGroupLevel, group, isLastRow, isLastGroup);
                    });
                }
                else
                    indentCells.ForEach(cell => { cell.IndentColumnType = IndentColumnType.InDataRow; });
            }
        }

        /// <summary>
        /// To check the indent cell whether it is in last row of a group.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="lastGroupLevel"></param>
        /// <param name="group"></param>
        /// <param name="isLastRow"></param>
        /// <param name="isLastGroup"></param>
        internal void CheckIsLastRow(DataColumnBase cell, int lastGroupLevel, Group group, bool isLastRow, bool isLastGroup)
        {
            if (cell.ColumnIndex < lastGroupLevel + (this.Owner.ShowRowHeader ? 1 : 0))
            {
                cell.IndentColumnType = isLastRow ? IndentColumnType.InLastGroupRow : IndentColumnType.InDataRow;
            }
            else if (cell.ColumnIndex < group.Level + (this.Owner.ShowRowHeader ? 1 : 0) - 1)
            {
                cell.IndentColumnType = isLastGroup ? IndentColumnType.InLastGroupRow : IndentColumnType.InDataRow;
            }
            else
            {
                cell.IndentColumnType = IndentColumnType.InLastGroupRow;
            }
        }

        internal bool IsLastRow(int nextRecordIndex)
        {
            object nextRecord = nextRecordIndex < this.View.TopLevelGroup.DisplayElements.Count ? this.View.TopLevelGroup.DisplayElements[nextRecordIndex] : null;
            if (nextRecord == null)
                return true;

            if (nextRecord is Group)
            {
                return (nextRecord as Group).Parent is TopLevelGroup;
            }
            return false;
        }

        internal bool IsLastRow(NodeEntry record, int rowIndex, ref int level)
        {
            bool isLast = false;
            //var recordIndex = this.View.TopLevelGroup.DisplayElements.IndexOf(record);
            if (this.Owner.GroupSummaryRows.Count > 0)
            {
                int originalRowIndex = rowIndex;
                if (record is RecordEntry)
                {
                    if (!(record as RecordEntry).IsExpanded)
                    {
                        originalRowIndex = rowIndex + (record.Parent as Group).GetRelationsCount();
                    }
                }
                var rec = this.View.TopLevelGroup.DisplayElements[this.Owner.ResolveToRecordIndex(originalRowIndex + 1)];
                if (rec is SummaryRecordEntry)
                    return isLast;
            }
            var parentRecord = record.Parent as Group;
            if (parentRecord != null)
            {
                if (record is RecordEntry)
                {
                    var lastIndex = parentRecord.GetRecordCount();
                    if (!(record as RecordEntry).IsExpanded)
                    {
                        isLast = parentRecord.Records[parentRecord.Records.Count - 1].Equals(record);
                    }
                    else
                    {
                        var recordIndex = this.View.TopLevelGroup.DisplayElements.IndexOf(record);
#if WP
                         var rowInx = this.Owner.ResolveToRowIndex(recordIndex) + parentRecord.GetRelationsCount();
                        if (rowInx == rowIndex)
                            isLast = true;
#else
                        // If HideEmptyGridViewDefinition is false, need to consider all detailsviewdatarow index
                        if (!this.Owner.DetailsViewManager.HasDetailsView || !this.Owner.HideEmptyGridViewDefinition)
                        {
                            var rowInx = this.Owner.ResolveToRowIndex(recordIndex) + parentRecord.GetRelationsCount();
                            if (rowInx == rowIndex)
                                isLast = true;
                        }
                        else
                        {
                            var dataRow = this.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                            // If dataRow is DetailsViewDataRow, check visual state 
                            if (dataRow is DetailsViewDataRow && (dataRow as DetailsViewDataRow).DetailsViewContentPresenter.CurrentVisualState == "LastCell")
                                isLast = true;
                            else
                            {
                                // If parent record contains no child grid having records, need to apply visual state of
                                if (!this.Owner.DetailsViewManager.HasChildSource(record as RecordEntry))
                                    isLast = true;
                            }
                        }

#endif
                    }

                }
                else if (record is Group)
                {
                    if (parentRecord.Groups.Count() != 0)
                    {
                        var lastrecord = GetLastVisibleGroup(record as Group);
                        isLast = record == lastrecord;
                    }
                }

                if (isLast && !parentRecord.IsTopLevelGroup && !(parentRecord.Parent is TopLevelGroup))
                {
                    isLast = this.IsLastRow(parentRecord, rowIndex, ref level);
                }
                if (level < 0)
                    level = parentRecord.Level;
            }
            return isLast;
        }

        internal bool IsLastGroup(Group group)
        {
            return group == GetLastVisibleGroup(group);
        }

        private Group GetLastVisibleGroup(Group group)
        {
            var parentGroup = group.Parent as Group;                        

            for (int i = parentGroup.Groups.Count - 1; i > 0; i--)
            {
                var lastGroup = parentGroup.Groups[i];
                if (lastGroup.ItemsCount > 0)
                    return lastGroup;
                else
                    continue;
            }
            return group;
        }

        /// <summary>
        /// Method which helps to update the GroupCaption column index
        /// </summary>
        /// <param name="row"></param>
        /// <param name="group"></param>
        /// <remarks></remarks>
        private void UpdateCoveredRow(SpannedDataRow row, NodeEntry dataContext)
        {
            if (row != null)
            {
                if (row.RowType != RowType.TableSummaryCoveredRow && row.RowType != RowType.TableSummaryRow)
                {
                    var cellInfo = new CoveredCellInfo(dataContext.Level + (this.Owner.ShowRowHeader ? 1 : 0), this.Container.ScrollColumns.LineCount,0,0);
                    row.CoveredCells.Clear();
                    row.CoveredCells.Add(cellInfo);
                    row.VisibleColumns.ForEach(col =>
                    {
                        if (col.ColumnElement is GridGroupSummaryCell)
                            col.ColumnIndex = dataContext.Level + (this.Owner.ShowRowHeader ? 1 : 0);
                        else if (col.ColumnElement is GridCaptionSummaryCell)
                            col.ColumnIndex = dataContext.Level + (this.Owner.ShowRowHeader ? 1 : 0);
                        col.ColumnSpan = cellInfo.Right - cellInfo.Left;
                    });
                }
                else
                {
                    var cellInfo = new CoveredCellInfo(Owner.ResolveToScrollColumnIndex(0), this.Container.ScrollColumns.LineCount,0,0);
                    row.CoveredCells.Clear();
                    row.CoveredCells.Add(cellInfo);
                    row.VisibleColumns.ForEach(col =>
                    {
                        if (col.ColumnElement is GridTableSummaryCell)
                            col.ColumnIndex = Owner.ResolveToScrollColumnIndex(0);
                        col.ColumnSpan = cellInfo.Right - cellInfo.Left;
                    });
                }
            }
        }

        /// <summary>
        /// Update StackedHeader Covered Row after Grouping and UnGrouping
        /// </summary>
        /// <param name="row"></param>
        /// <param name="increasedIndexValue"></param>
        /// <remarks></remarks>
        internal void UpdateStackedheaderCoveredRow(SpannedDataRow row, int increasedIndexValue)
        {
            if (row != null)
            {
                List<CoveredCellInfo> coveredCells = new List<CoveredCellInfo>();
                row.CoveredCells.ForEach(cell => coveredCells.Add(cell));
                row.CoveredCells.Clear();
                coveredCells.ForEach(cell =>
                {
                    var cc = new CoveredCellInfo(cell.Row, cell.Left + increasedIndexValue, cell.Right + increasedIndexValue,0,0);
                    row.CoveredCells.Add(cc);
                });
            }
        }

        private void CollapseRow(DataRowBase row)
        {
#if WinRT || UNIVERSAL
            //WRT-4919 - Skipped RowVisibility for CurrentRow to avoid on screen keyboard issue
            if (row.IsEditing)
                return;
#endif
            //WPF-19912 - CanResetDataContext property introduced.
            if (CanResetDataContext)
            {
                row.RowIndex = -1;
                row.RowData = null;
                if (row.WholeRowElement != null)
                    row.WholeRowElement.DataContext = null;
            }
            row.RowVisibility = Visibility.Collapsed;

            if (row is DetailsViewDataRow)
                this.Owner.DetailsViewManager.CollapsingDetailsViewDataRow((row as DetailsViewDataRow));
        }

        /// <summary>
        /// Method which will ensure whether the row is selected or not.
        /// </summary>
        /// <param name="rowIndex">Corresponding Row Index</param>
        /// <returns>Whether row is selected or not</returns>
        /// <remarks></remarks>
        private void CheckForSelection(DataRowBase row)
        {
            //Exception throws when checking selection for header row also hence added the code to check whether the row is HeaderRow
            if (this.Owner.SelectionMode == GridSelectionMode.None || row.RowType == RowType.HeaderRow)
                return;

            if (this.Owner.SelectionUnit == GridSelectionUnit.Row)
            {
                row.IsSelectedRow = this.Owner.SelectionController.SelectedRows.Contains(row.RowIndex);
                //No Need to set focused row for the row has been selected by mutiple option.
                if ((row.RowType == RowType.DefaultRow || row.RowType == RowType.DetailsViewRow || row.RowType == RowType.AddNewRow || 
                    row.RowType == RowType.UnBoundRow || row.RowType == RowType.FilterRow) && this.Owner.NavigationMode == NavigationMode.Cell)
                    return;

                if (!row.IsSelectedRow && this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == row.RowIndex)
                    row.IsFocusedRow = true;
                else
                    row.IsFocusedRow = false;
            }
            else
                EnsureCellSelection(row);
        }

        internal void EnsureCellSelection(DataRowBase row)
        {
            if (row != null && (row.RowType== RowType.DefaultRow || row.RowType == RowType.DetailsViewRow || row.RowType== RowType.AddNewRow 
                || row.RowType == RowType.UnBoundRow || row.RowType == RowType.FilterRow))
            {
                row.VisibleColumns.ForEach(col =>
                {
                    if (col.GridColumn != null)
                        col.IsSelectedColumn = IsCellInfoAvailable(col.GridColumn, row);
                });
            }
            else if (row != null)
            {
                row.IsSelectedRow = this.Owner.SelectionController.SelectedCells.Any(item => item.RowIndex == row.RowIndex);

                if (!row.IsSelectedRow && this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == row.RowIndex)
                    row.IsFocusedRow = true;
                else
                    row.IsFocusedRow = false;
            }
        }


        /// <summary>
        /// Enusre the selection for merged rows based on coveredcell info. and make sure the editing while scroll the row.
        /// </summary>
        /// <param name="row"></param>
        internal void EnsureMergedCellSelection(DataRowBase row)
        {
            if (!this.Owner.SelectionController.CurrentCellManager.HasCurrentCell)
            {
                if (row.RowIndex != this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                    return;

                var dataColumn =row.VisibleColumns.FirstOrDefault(item => item.ColumnIndex == this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex && item.IsSpannedColumn);

                if (dataColumn != null)
                {
                    if (dataColumn != null && !dataColumn.IsCurrentCell)
                    {
                        dataColumn.IsCurrentCell = true;
                        dataColumn.IsSelectedColumn = true;
                        this.Owner.SelectionController.CurrentCellManager.SetCurrentColumnBase(dataColumn, true);
                    }
                }
                return;
            }

            var currentCoveredCellInfo = this.Owner.CoveredCells.GetCoveredCellInfo(row.RowIndex, this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
            
            if (currentCoveredCellInfo == null)
                return;

            if(!currentCoveredCellInfo.ContainsRow(this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                return;

            var canSetCurrentColumn = currentCoveredCellInfo != null;

            if (this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == row.RowIndex || canSetCurrentColumn)
            {                
                var currentColumn = this.Owner.SelectionController.CurrentCellManager.CurrentCell;
                var dataColumnBase = this.Owner.GetDataColumnBase(new RowColumnIndex(row.RowIndex, this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));

                if (dataColumnBase != null)
                {
                    if (currentColumn != null && !dataColumnBase.Equals(currentColumn))
                    {
                        currentColumn.IsCurrentCell = false;
                        currentColumn.IsSelectedColumn = false;                            
                    }

                    if (dataColumnBase.IsCurrentCell && dataColumnBase.IsSelectedColumn)
                        return;
                                                 
                    var isInEditing = this.Owner.SelectionController.CurrentCellManager.CurrentCell.IsEditing;
                    if (isInEditing)
                    {
                        this.Owner.MergedCellManager.CanRasieEvent = false;
                        this.Owner.SelectionController.CurrentCellManager.EndEdit(true);
                    }

                    this.Owner.SelectionController.CurrentCellManager.CurrentCell = dataColumnBase;                            
                    dataColumnBase.IsCurrentCell = true;
                    dataColumnBase.IsSelectedColumn = true;                           

                    if (isInEditing)
                    {                                
                        this.Owner.SelectionController.CurrentCellManager.SetCurrentColumnBase(dataColumnBase, true);
                        this.Owner.SelectionController.CurrentCellManager.BeginEdit();
                        this.Owner.MergedCellManager.CanRasieEvent = true;
                    }         
                }
            }                            
        }
        private bool IsCellInfoAvailable(GridColumn column, DataRowBase row)
        {
            var selectionController=this.Owner.SelectionController;
            if (selectionController.SelectedCells.Count > 0)
            {
                GridSelectedCellsInfo selectedCellInfo = null;
                if (row.RowType == RowType.UnBoundRow)
                    //Added the code to get the UnBoundDataRow if the SelectedCells contains given rowIndex.
                    selectedCellInfo = selectionController.SelectedCells.FirstOrDefault(cellInfo => cellInfo.IsUnBoundRow && cellInfo.RowIndex == row.RowIndex);
                else if (row.RowType == RowType.FilterRow)
                    selectedCellInfo = selectionController.SelectedCells.FirstOrDefault(cellInfo => cellInfo.IsFilterRow);
                else
                    //Added code for AddNewRow selection by the flag exposed in GridCellInfo called IsAddNewRow.
                    selectedCellInfo = row.RowType == RowType.AddNewRow ? selectionController.SelectedCells.FirstOrDefault(cellInfo => cellInfo.IsAddNewRow) : selectionController.SelectedCells.Find(row.RowData);

                if (selectedCellInfo != null && selectedCellInfo.ColumnCollection.Count > 0)
                {
                    if(selectedCellInfo.ColumnCollection.Any(item => item == column))
                    {
#if WPF
                        if (this.Owner.useDrawing)
                            row.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                        return true;
                    }
                }
            }
            return false;
        }

        internal double GetCoveredColumnSize(int start, int end)
        {
            DoubleSpan[] CurrentPos = this.Container.ScrollColumns.RangeToRegionPoints(start, end, true);
            return CurrentPos[1].Length;
        }

        internal double GetCoveredRowSize(int start, int end)
        {
            DoubleSpan[] CurrentPos = this.Container.ScrollRows.RangeToRegionPoints(start, end, true);
            return CurrentPos[1].Length;
        }

        private void WireViewEvents()
        {

        }

        private void UnWireViewEvents()
        {

        }

        private DataRowBase EnsureGroupCaption(int rowIndex, int actualStartIndex, int actualEndIndex)
        {
            if (rowIndex > -1)
            {
                var datarow = this.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                if (datarow != null)
                {
                    datarow.IsEnsured = true;
                    datarow.IsFixedRow = true;
                    datarow.WholeRowElement.Clip = null;
                    if (datarow.RowVisibility == Visibility.Collapsed)
                        datarow.RowVisibility = Visibility.Visible;
                    return datarow;
                }
                else
                {
                    var rows = this.Items.Where(row => ((row.RowIndex < 0 || row.RowIndex < actualStartIndex || row.RowIndex > actualEndIndex) && row.RowRegion == RowRegion.Body && !row.IsEnsured)).ToList();
                    UpdateRow(rows, rowIndex, RowRegion.Body);
                    var newrow = this.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                    newrow.IsEnsured = true;
                    newrow.IsFixedRow = true;
                    newrow.WholeRowElement.Clip = null;
                    return newrow;
                }
            }
            return null;
        }

#if !SyncfusionFramework4_0
        private async void InitializeIncrementalSource(int visibleRowCount)
#else
        private void InitializeIncrementalSource(int visibleRowCount)
#endif        
        {
            var initialLoadAmount = visibleRowCount * 2;
            if (View.HasMoreItems && CanUpdateSource(initialLoadAmount))
            {
                var loadSize = this.Owner.DataFetchSize;
                do
                {     
#if !SyncfusionFramework4_0
                    await 
#endif
                    View.LoadMoreItemsAsync((uint)loadSize);                                        
                    loadSize *= 10;
                } while (loadSize <= initialLoadAmount);
                lastFetcheSize = loadSize;
            }
        }

        bool isFetching = false;

#if !SyncfusionFramework4_0
        private async void UpdateIncrementalSource()        
        {
#else
        private void UpdateIncrementalSource() 
        {
#endif
            if (View.HasMoreItems)
            {
                isFetching = true;
#if !SyncfusionFramework4_0
                await 
#endif
                View.LoadMoreItemsAsync((uint)this.Owner.DataFetchSize);
#if !SyncfusionFramework4_0
                var rowCount = this.Owner.VisualContainer.ViewportHeight / this.Owner.RowHeight;
                if ((this.View as CollectionViewAdv).Count < rowCount && View.HasMoreItems)
                    await View.LoadMoreItemsAsync((uint)this.Owner.DataFetchSize);
#endif
                isFetching = false;
            }
        }

        private bool CanUpdateSource(int endIndex)
        {
            var pivotIndex = (this.View as CollectionViewAdv).Count > this.Owner.DataFetchSize ? (this.View as CollectionViewAdv).Count - this.Owner.DataFetchSize : (this.View as CollectionViewAdv).Count;
            return endIndex >= pivotIndex && this.View.HasMoreItems && !isFetching;
        }

        private GridSummaryRow GetSummaryRow(int rowIndex, TableSummaryRowPosition position)
        {
            if (position == TableSummaryRowPosition.Bottom)
            {
                var footerCount = Owner.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
                var startTableSummaryIndex = Owner.VisualContainer.RowCount - (Owner.GetTableSummaryCount(position) + footerCount);
                var indexInCollection = rowIndex - startTableSummaryIndex;
                var bottomSummaries = Owner.TableSummaryRows.Where(row => (row is GridSummaryRow && !(row is GridTableSummaryRow)) || (row is GridTableSummaryRow && (row as GridTableSummaryRow).Position != TableSummaryRowPosition.Top));
                var summarRow = bottomSummaries.ElementAt(indexInCollection) as GridSummaryRow;
                return summarRow;
            }
            else
            {
                var frozenCount = Owner.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
                var startTableSummaryIndex = Owner.GetHeaderIndex() + frozenCount + 1;
                var indexInCollection = rowIndex - startTableSummaryIndex;
                var topSummaries = Owner.TableSummaryRows.Where(row => (row is GridTableSummaryRow && (row as GridTableSummaryRow).Position == TableSummaryRowPosition.Top));
                var summaryRow = topSummaries.ElementAt(indexInCollection) as GridSummaryRow;
                return summaryRow;
            }
        }

        private void EnsureFrozenGroupHeaders(VisibleLinesCollection visibleRows)
        {
            if (visibleRows.FirstBodyVisibleIndex <= 0 && visibleRows.LastBodyVisibleIndex < 0)
                return;
            if (visibleRows.Count <= visibleRows.FirstBodyVisibleIndex)
                return;
            var ActualStartIndex = visibleRows[visibleRows.FirstBodyVisibleIndex].LineIndex;
            var ActualEndIndex = visibleRows[visibleRows.LastBodyVisibleIndex].LineIndex;

            var startindex = ActualStartIndex;
            var fixedRows = new List<DataRowBase>();
            while (true)
            {
                if (startindex > ActualEndIndex)
                    break;
                var recordindex = this.Owner.ResolveToRecordIndex(startindex);
                var startingRecord = this.View.TopLevelGroup.DisplayElements[recordindex];
                if (startingRecord != null)
                {
                    if (startingRecord is NestedRecordEntry)
                        startingRecord = startingRecord.Parent.Parent;
                    var record = startingRecord;
                    if (!record.IsGroups)
                        record = record.Parent;
                    if (record.IsGroups)
                    {
                        var index = this.View.TopLevelGroup.DisplayElements.IndexOf(record);
                        while (index > -1)
                        {
                            int rowIndex = this.Owner.ResolveToRowIndex(index);
                            if (fixedRows.All(frow => frow.RowIndex != rowIndex))
                            {
                                var group = record as Group;
                                if (!group.IsBottomLevel || (group.IsBottomLevel && rowIndex + this.Owner.GroupSummaryRows.Count + group.GetRecordCount() >= (ActualStartIndex + (group.Level - 1))))
                                {
                                    var row = EnsureGroupCaption(rowIndex, ActualStartIndex, ActualEndIndex);
                                    fixedRows.Add(row);
                                }
                            }
                            record = record.Parent;
                            if (record != null && record.IsGroups)
                                index = this.View.TopLevelGroup.DisplayElements.IndexOf(record);
                            else
                                index = -1;
                        }
                    }

                    if (startingRecord.Parent != null && startingRecord.Parent.IsGroups)
                    {
                        if (startingRecord.IsRecords)
                        {
                            var fixedrowscount = fixedRows.GroupBy(frow => frow.Level).Count();
                            if (ActualStartIndex + fixedrowscount <= startindex)
                                break;
                            else
                                startindex++;
                        }
                        else
                            startindex++;
                    }
                }
                else
                    break;
            }

            var fixeditems = this.Items.Where(item => item.IsFixedRow);
            foreach (var item in fixeditems)
            {
                if (item.RowData is Group)
                {
                    if (!(item.RowData as Group).IsExpanded)
                    {
                        item.IsFixedRow = false;
                        item.IsEnsured = false;
                    }
                }
            }

            RectangleGeometry previousClip = null;
            foreach (var captionRow in Items.Where(item => item.RowType == RowType.CaptionRow || item.RowType == RowType.CaptionCoveredRow))
            {
                if (captionRow.RowVisibility == Visibility.Collapsed)
                    continue;
                if (previousClip == null || previousClip.Rect.IsEmpty)
                {
                    VisualStateManager.GoToState(captionRow.WholeRowElement, "Normal", false);
                    (captionRow as SpannedDataRow).ApplyFixedRowVisualState(false);
                }
                previousClip = captionRow.WholeRowElement.Clip as RectangleGeometry;
            }
        }

#endregion

#region IRowGenerator

        IList<IRowElement> IRowGenerator.Items
        {
            get { return this.Items.ToList<IRowElement>(); }
        }
        public void PregenerateRows(VisibleLinesCollection visibleRows, VisibleLinesCollection visibleColumns)
        {
            this.Owner.GridModel.InitializeGrouping();
            this.Owner.GridModel.InitialFiltering();

            if (this.View != null && this.View.HasMoreItems)
            {
                this.InitializeIncrementalSource(visibleRows.Count);
            }

            for (var i = 0; i < visibleRows.Count; i++)
            {
                var line = visibleRows[i];
                DataRowBase dr = null;
                switch (line.Region)
                {
                    case ScrollAxisRegion.Header:
                        dr = CreateHeaderRow(line.LineIndex, visibleColumns);
                        break;
                    case ScrollAxisRegion.Body:
                        dr = CreateDataRow(line.LineIndex, visibleColumns);
                        break;
                    case ScrollAxisRegion.Footer:
                        dr = CreateFooterRow(line.LineIndex, visibleColumns);
                        break;
                }
                if (dr != null)
                    this.Items.Add(dr);
            }
        }

        /// <summary>
        /// Finds the start and end index of region and return true. If region doesn't have any rows, returns false.
        /// </summary>
        /// <param name="visibleRows"></param>
        /// <param name="i"></param>
        /// <param name="ActualStartIndex"></param>
        /// <param name="ActualEndIndex"></param>
        /// <param name="region"></param>
        /// <returns></returns>
        private bool GetRegionStartEndIndex(VisibleLinesCollection visibleRows, int i, ref int ActualStartIndex, ref int ActualEndIndex, ref RowRegion region)
        {
            if (i == 0)// Below condition make sure the Header of the rows. will include Frozen rows, Table summaries at top, AddNewRow at Top and Headers.
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
            else if (i == 1)// Below will make sure the start and end rows. which includes only datarows.
            {
                if (visibleRows.FirstBodyVisibleIndex <= 0 && visibleRows.LastBodyVisibleIndex < 0)
                    return false;
                if (visibleRows.Count > visibleRows.FirstBodyVisibleIndex)
                    ActualStartIndex = visibleRows[visibleRows.FirstBodyVisibleIndex].LineIndex;
                else
                    return false;
                ActualEndIndex = visibleRows[visibleRows.LastBodyVisibleIndex].LineIndex;
                region = RowRegion.Body;

                if (this.Owner.CanQueryCoveredRange())
                    this.Owner.CoveredCells.RemoveRowRange(ActualStartIndex, ActualEndIndex);
            }
            else// which make sure the footer of the grid. which includes footer rows, Table summary at bottom, default table summary.
            {
                if (visibleRows.firstFooterVisibleIndex < visibleRows.Count)
                {
                    ActualStartIndex = visibleRows[visibleRows.firstFooterVisibleIndex].LineIndex;
                    ActualEndIndex = visibleRows[visibleRows.Count - 1].LineIndex;
                }
                else
                {
                    ActualStartIndex = 0;
                    ActualEndIndex = -1;
                }
                region = RowRegion.Footer;
            }
            return true;
        }

        private void ReUseRow(int index, int ActualStartIndex, int ActualEndIndex, RowRegion region)
        {
            if (this.Items.All(row => row.RowIndex != index))
            {
                if (this.Items.Any(row => (row.RowIndex < 0 || row.RowIndex < ActualStartIndex || row.RowIndex > ActualEndIndex) && !row.IsEnsured))
                {
                    // we wont reuse rows that was current row, it was in editing, it was an addnew row.
                    IEnumerable<DataRowBase> rows;
                    if (this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= 0 && this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == index && !Owner.IsAddNewIndex(index))
                    {
                        rows = this.Items.Where(row => row.IsCurrentRow);
                        if (!rows.Any())
                        {
                            // will get rows to reuse based on some conditions , doing with key navigation(Enter/Down/Up to bring single row from unview row to view.).
                            rows = this.Items.Where(row => ((row.RowIndex < 0 || row.RowIndex < ActualStartIndex ||
                                    row.RowIndex > ActualEndIndex) && !row.IsEnsured && !row.IsEditing) && !row.IsCurrentRow && row.RowType != RowType.AddNewRow);
                        }
                    }
                    else
                    {
                        //will get rows to reuse based on some conditions , called while scroll vertically - bulk rows has been taken for reuse.
                        rows = this.Items.Where(row => ((row.RowIndex < 0 || row.RowIndex < ActualStartIndex ||
                                    row.RowIndex > ActualEndIndex) && !row.IsEnsured && !row.IsEditing) && !row.IsCurrentRow && row.RowType != RowType.AddNewRow);
                    }
                    if (rows != null && rows.Any())
                    {
                        if (this.Owner.CanQueryCoveredRange())
                        {
                            // Merged row will have current cell where he current row column index is differ. we should not reuse that row.
                            var dataGrid = this.Owner.GetDataGrid();

                            if (dataGrid != null && dataGrid.SelectionController.CurrentCellManager.HasCurrentCell)
                            {
                                var currentCell = dataGrid.SelectionController.CurrentCellManager.CurrentCell;
                                if (currentCell != null)
                                    rows = rows.Where(row => currentCell.RowIndex != row.RowIndex);
                            }
                        }

                        if (region != RowRegion.Footer && this.Owner.DetailsViewManager.HasDetailsView && this.Owner.IsInDetailsViewIndex(index))
                            this.Owner.DetailsViewManager.UpdateDetailsViewDataRow(rows, index);
                        else
                        {
                            //Call for reusing rows taken from above codes.
                            UpdateRow(rows, index, region);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Method to ensure or update the row associated properties like RowIndex, RowData, row state and its selection  while scrolling and Data Manipulation Operation based on VisibleRows.
        /// </summary>
        /// <param name="visibleRows"></param>
        public virtual void EnsureRows(VisibleLinesCollection visibleRows)
        {
			    var isQueryCoveredRangeEventWired = this.Owner.CanQueryCoveredRange();            
                if (visibleRows.Count <= 0)
                {
                this.Items.ForEach(row =>
                {
                    if (row.RowRegion == RowRegion.Header || (row.RowRegion == RowRegion.Footer && this.Owner.GetTableSummaryCount(TableSummaryRowPosition.Bottom) > 0))
                        row.IsEnsured = true;
                    else
                        row.IsEnsured = false;
                });
                goto Finish;
            }
            
            var ActualStartIndex = 0;
            var ActualEndIndex = 0;
            // Initially will set IsEnsured to false. and create will set again to true in following case.
            this.Items.ForEach(row => { row.IsEnsured = false; row.IsFixedRow = false; });
                
            // Sets the MappedRowColumnIndex to -1, -1.
            if (isQueryCoveredRangeEventWired)
            {
                this.Owner.CoveredCells.ForEach(coveredRange =>
                {
                    if (coveredRange.MappedRowColumnIndex == new RowColumnIndex(-1, -1)) coveredRange.MappedRowColumnIndex = new RowColumnIndex(-1, -1);
                });
            }

            if (visibleRows.LastBodyVisibleIndex > 0)
            {
                ActualEndIndex = visibleRows[visibleRows.LastBodyVisibleIndex].LineIndex;
                // Call for Incremental loading to populate data to  view.
                if (this.View != null && CanUpdateSource(ActualEndIndex))
                    this.UpdateIncrementalSource();
            }

            //Handling FrozenGroup Headers
            if (this.View != null && this.View.GroupDescriptions.Count > 0 && this.Owner.AllowFrozenGroupHeaders)
                EnsureFrozenGroupHeaders(visibleRows);
                
            var region = RowRegion.Header;
            for (int i = 0; i < 3; i++)
            {
                if (!GetRegionStartEndIndex(visibleRows, i, ref ActualStartIndex, ref ActualEndIndex, ref region))
                    continue;

                for (int index = ActualStartIndex; index <= ActualEndIndex; index++)
                {
                    if (visibleRows.All(row => row.LineIndex != index))
                        continue; // When SfDataGrid has DetailsViewdefinition the nested hidden index should not processed.

                    this.ReUseRow(index, ActualStartIndex, ActualEndIndex, region);

                    //While deleting in the record in Header or Footer records the DataRow will be updated to Footer or Header 
                    //it is not correct behavior, hence added this condition to update the region.
                    var dr = this.Items.FirstOrDefault(row => row.RowIndex == index); // && row.RowRegion == region);

                    if (dr != null)
                    {
                        if (dr.RowRegion != region)
                            dr.RowRegion = region;
                        dr.UpdateFixedRowState();
                    }

                    if (dr != null && this.Owner.GridModel.HasGroup && (dr.isDataRowDirty || this.Owner.IsInDetailsViewIndex(index)))
                    {
                        var inx = this.Owner.ResolveToGroupRecordIndexForDetailsView(index);
                        var nodeEntry = this.View.TopLevelGroup.DisplayElements[inx];
                        UpdateIndentCells(dr, nodeEntry); // need to update the indent cells while grouping
                    }

                    //Grid not loaded since dr is null. so null check added
                    if (dr != null)
                        dr.isDataRowDirty = false;
                    if (Owner.IsAddNewIndex(index))
                    {
                        if (dr != null)
                        {
                            // While Set AddNewRowPosition to bottom, it will not to be in view. we need to scroll down to bring to view. where we need to set visibility and border clip.
                            if (dr.RowVisibility == Visibility.Collapsed)
                            {
                                dr.RowVisibility = Visibility.Visible;
                                //Fix for AlternateRow background was clipped.
                                dr.WholeRowElement.UpdateRowBackgroundClip();
                            }
                        }
                        else
                        {
                            // processed when AddNewRow position to Top.
                            dr = this.Items.FirstOrDefault(row => row.RowType == RowType.AddNewRow);
                            if (dr != null && View != null)
                            {
                                dr.RowIndex = index;
                                dr.RowLevel = 0;
                                dr.RowRegion = this.Owner.AddNewRowPosition == AddNewRowPosition.FixedTop ? RowRegion.Header : RowRegion.Body;
                                if (View.IsAddingNew)
                                    dr.RowData = View.CurrentAddItem;
                                dr.OnRowIndexChanged();
                                    
                                if (!dr.IsSelectedRow && Owner.SelectionUnit == GridSelectionUnit.Row && index == Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                                    dr.IsSelectedRow = true;
                                    
                            }
                        }
                    }

                    if (dr != null)
                    {
                        if (dr.RowVisibility == Visibility.Collapsed)
                        {
                            dr.RowVisibility = Visibility.Visible;
                            //Fix for AlternateRow background was clipped.
                            dr.WholeRowElement.UpdateRowBackgroundClip();
                        }
                    }
                    else
                    {
                        // Needs to be there - Code for add rows at run time. 
                        if (region == RowRegion.Header)
                            dr = CreateHeaderRow(index, this.Container.ScrollColumns.GetVisibleLines());
                        else if (region == RowRegion.Footer)
                            dr = CreateFooterRow(index, this.Container.ScrollColumns.GetVisibleLines());
                        else
                            dr = CreateDataRow(index, this.Container.ScrollColumns.GetVisibleLines());
                        // Since the newly created footer unboundrow will not be in Footer state. to reset iyts state its needs to be call here.
                        dr.UpdateFixedRowState();
                        this.Items.Add(dr);
                    }
                    if (dr.RowType == RowType.AddNewRow)
                    {
                        // Add New Row will not get select. need to check selection here while add new is at bottom.
                        CheckForSelection(dr);
                        if (Owner.View != null && Owner.View.GroupDescriptions.Count > 0)
                            UpdateIndentCells(dr, null);
                        (dr.WholeRowElement as AddNewRowControl).UpdateTextBorder();
                    }
                    if (dr.IsCurrentRow)
                    {
                        // selection need to check moving left to right , 
                        this.CheckForSelection(dr);
                        dr.IsCurrentRow = false;
                        (dr as GridDataRow).ApplyRowHeaderVisualState();
                    }
                    if (dr.RowType == RowType.UnBoundRow && !dr.IsSelectedRow)
                    {
                        this.CheckForSelection(dr);
                        dr.IsCurrentRow = false;
                        (dr as GridDataRow).ApplyRowHeaderVisualState();
                    }
                    dr.IsEnsured = true;
                    if (dr.IsSelectedRow)                                            
                        dr.WholeRowElement.UpdateSelectionBorderClip();                    
                    //WPF-23996(Issue 5) - DottedBorder is not shown properly for other than DataColumn, because CurrentFocusBorderMargin 
                    //is not updated while ensure, so update the CurrentFocusBorderMargin by using UpdateFocusRowPosition.
                    if (dr.IsFocusedRow && this.Owner.SelectionMode == GridSelectionMode.Multiple)
                        dr.WholeRowElement.UpdateFocusRowPosition();
                    //when scroll vertically, the new row can be created with new curerntcell for that row. where we need to set the currentcell using this call.

                    if (dr.RowIndex == this.Owner.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex && (this.Owner.NavigationMode == NavigationMode.Cell || Owner.IsAddNewIndex(dr.RowIndex) || Owner.IsFilterRowIndex(dr.RowIndex) || (this.Owner.NavigationMode == NavigationMode.Cell && this.Owner.SelectionMode == GridSelectionMode.Multiple && this.Owner.CurrentItem != null)))
                    {
                        dr.UpdateCurrentCellSelection();
                    }

                    if (this.Owner.DetailsViewManager.HasDetailsView && dr is DataRow)
                        (dr as DataRow).CheckForDetailsViewExpanderVisibilty();

                    if (isQueryCoveredRangeEventWired)
                    {
                        // Raise query covered range event for the row that has no covered cells info
                        if ((dr.RowType == RowType.DefaultRow || dr.RowType == RowType.UnBoundRow))
                            this.Owner.MergedCellManager.InitializeMergedRow(dr);

                        // Map the visible row index in covered range info.
                        this.Owner.MergedCellManager.UpdateMappedRowIndex(dr, ActualStartIndex);

                        // Update the merged row properties with datarow
                        this.Owner.MergedCellManager.UpdateMergedRow(dr);

                        if (!this.Owner.IsInDetailsViewIndex(dr.RowIndex))
                        {
                            dr.WholeRowElement.ItemsPanel.InvalidateMeasure();
                            dr.WholeRowElement.ItemsPanel.InvalidateArrange();
#if WPF
                            if (this.Owner.useDrawing)
                                dr.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                        }

                    }                                                                                                 
                }
            }

Finish:
            this.Items.Where(item => item.RowRegion == RowRegion.Footer).ForEach(row => row.IsFixedRow = true);
            // Update ExtendedWidth if no records are expanded
            if (this.Owner.DetailsViewManager.HasDetailsView && this.View != null)
            {
                if (this.View.Records.All(record => !record.IsExpanded))
                {
                    this.Owner.DetailsViewManager.UpdateExtendedWidth();
                }
            }

            if (isQueryCoveredRangeEventWired)
            {
                foreach (var dr in this.Items)
                {
                    if (dr.IsEnsured && (dr.RowType == RowType.DefaultRow || dr.RowType == RowType.UnBoundRow))
                    {
                        EnsureMergedCellSelection(dr);
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
                    (currentRow as GridDataRow).ApplyRowHeaderVisualState();
                }
            }
            this.ForceUpdateBinding = false;
            this.Items.ForEach(row => { if (!row.IsEnsured) { CollapseRow(row); } });

            if (isQueryCoveredRangeEventWired)
            {
                // Refresh the rows when the range given while bottom index is appears.
                foreach (var dr in this.Items)
                {
                    if (!dr.IsSpannedRow)
                        continue;
                    if (this.Owner.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(dr.RowIndex) != null)
                    {
                        var coveredCellInfoCollection = this.Owner.CoveredCells.GetCoveredCellsRange(dr);
                        if (coveredCellInfoCollection.Count == 0)
                            continue;

                        int bottomIndex = coveredCellInfoCollection.OrderBy(column => column.Bottom).FirstOrDefault().Bottom;
                        int topIndex = coveredCellInfoCollection.OrderBy(column => column.Top).FirstOrDefault().Top;
                        var dataRow = this.Items.Find(row => !row.IsSpannedRow && row.RowIndex >= topIndex && row.RowIndex <= bottomIndex);
                        if (dataRow != null)
                        {
                            this.Owner.MergedCellManager.RefreshMergedRows(dataRow);
                            break;
                        }
                    }
                }
            }            
        }
        
        public bool QueryRowHeight(int rowIndex, ref double height)
        {            
            if (this.Owner.DetailsViewManager.HasDetailsView)
                if (!this.Owner.IsTableSummaryIndex(rowIndex))
                    return false;

            var args = new QueryRowHeightEventArgs(rowIndex, height, null);
            this.Owner.RaiseQueryRowHeight(args);
            if (args.Handled)
                height = args.Height;
            return args.Handled;
        }

        public void ResetSelection()
        {
            this.Items.ForEach(item =>
            {
                item.IsSelectedRow = false;
                item.IsFocusedRow = false;
                item.IsCurrentRow = false;
                if (this.Owner.SelectionUnit != GridSelectionUnit.Row)
                {
                    item.VisibleColumns.ForEach(col =>
                    {
                        col.IsSelectedColumn = false;
                    });
                }
            });
        }

        public void EnsureColumns(VisibleLinesCollection visibleColumns)
        {
            foreach (var gridRow in this.Items)
            {
                gridRow.EnsureColumns(visibleColumns);
                gridRow.WholeRowElement.UpdateRowBackgroundClip();
            }
        }

        internal void UpdateCellStyle(int index)
        {
            var row = this.Items.Where(r => r is DataRow).LastOrDefault(x => x != null);
            var colIndex = row.VisibleColumns.IndexOf(row.VisibleColumns.LastOrDefault(x => x.ColumnIndex == index));
            if (colIndex >= 0)
                foreach (var datarow in this.Items)
                {
                    if (datarow is DataRow && (datarow.RowData != null || datarow.RowType == RowType.AddNewRow || datarow.RowType == RowType.FilterRow || datarow.RowType == RowType.HeaderRow))            
                        datarow.VisibleColumns[colIndex].UpdateCellStyle(datarow.RowData);
                }
        }

        public void ApplyFixedRowVisualState(int index, bool canapply)
        {
            var row = Items.FirstOrDefault(item => item.RowIndex == index);
            var spannedrow = row as SpannedDataRow;
            if (spannedrow == null)
                return;
#if WPF
            if(this.Owner.useDrawing)
                row.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
            if (canapply)
            {
                row.WholeRowElement.RowBorderState = "FixedCaption";
                spannedrow.ApplyFixedRowVisualState(true);
            }
            else
            {
                row.WholeRowElement.RowBorderState = "Normal";
                spannedrow.ApplyFixedRowVisualState(false);
            }
        }

        public void ColumnHiddenChanged(HiddenRangeChangedEventArgs args)
        {
            //WPF-32768(Issue 5) - No need to change the visibility here, as the visibility of column gets changed in EnsureColumn of DataRow.
            //NeedToRefreshColumn set to true, in OnScrollColumnsChanged method of VisualContainer.

            //this.Items.ForEach(row => row.VisibleColumns.ForEach(column =>
            //{
            //    if (!(row is DetailsViewDataRow))
            //        if (column.ColumnIndex >= args.From && column.ColumnIndex <= args.To)
            //        {
            //            column.ColumnVisibility = args.Hide ? Visibility.Collapsed : Visibility.Visible;
            //        }
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
            var startColumnIndex = this.Owner.ResolveToStartColumnIndex();
            if (this.Items.Count > 0)
            {
                int visiblecolumnIndex = this.Owner.ResolveToGridVisibleColumnIndex(index);
                this.Items.ForEach(row =>
                {
                    if (!(row is DetailsViewDataRow) || (row is DetailsViewDataRow && visiblecolumnIndex < 0 && !this.Owner.inDetailsViewIndentChange))
                    {
                        row.VisibleColumns.ForEach(col =>
                        {
                            if (index <= col.ColumnIndex)
                            {
                                if (this.Owner.CoveredCells.Count != 0)
                                    this.Owner.coveredCells.Clear();

                                if (col.IsSpannedColumn)
                                {
                                    col.isSpannedColumn = false;
                                    col.IsSelectedColumn = false;
                                    col.ColumnVisibility = Visibility.Visible;
                                }
                                // If inedx == col.ColumnIndex, we should check whether it is spanned column, if it is spanned column and indent column is added(index < startColumnIndex), index should be incremented
                                // If it is not spanned column (ColumnSpan = 0), we can increment column index
                                if (index != col.ColumnIndex || (index < startColumnIndex || col.ColumnSpan == 0))
                                    col.ColumnIndex += count;
                            }
                        });
                    }
                    
                    if (row.RowType == RowType.SummaryCoveredRow || row.RowType == RowType.CaptionCoveredRow
                        || row.RowType == RowType.TableSummaryCoveredRow)
                    {
                        var prevCoverdCells = new List<CoveredCellInfo>();
                        foreach (var item in (row as SpannedDataRow).CoveredCells)
                        {
                            prevCoverdCells.Add(item);
                        }
                        (row as SpannedDataRow).CoveredCells.Clear();
                        prevCoverdCells.ForEach(cell =>
                            {
                                //WRT-4360 - When enable RowHeader at run time we need to increase the left of covered cell.
                                if ((index == 0 && this.Owner.inRowHeaderChange) || index < cell.Left)
                                    (row as SpannedDataRow).CoveredCells.Add(new CoveredCellInfo(cell.Left + count, cell.Right + count,0,0));
                                //Code changes for WPF-14434
                                else if (index <= cell.Right)
                                    (row as SpannedDataRow).CoveredCells.Add(new CoveredCellInfo(cell.Left, cell.Right + count,0,0));
                            });
                    }
                });
            }
        }

        public void ColumnRemoved(int index, int count)
        {
            // Based on canUnloadDetailsViewCell, DetailsViewContentPresenter, DetailsViewExpanderCell, covered row will be unloaded and removed from visible columns           
            // If all grid columns are cleared, need to remove DetailsViewContentPresenter, DetailsViewExpanderCell and covered row
            bool canUnloadDetailsViewandCoveredCell = count == this.Owner.VisualContainer.ColumnCount;

            int endIndex = index + count - 1;
            int visiblecolumnIndex = this.Owner.ResolveToGridVisibleColumnIndex(index);

            Func<DataRowBase, bool> canAdjust = row =>
            {
                // WPF-19472 - If all columns are removed, need to clear CoveredRow visible columns also
                if ((row.RowType == RowType.SummaryCoveredRow
                    || row.RowType == RowType.CaptionCoveredRow || row.RowType == RowType.TableSummaryCoveredRow) && canUnloadDetailsViewandCoveredCell)
                    return true;
                else if ((this.Owner.inRowHeaderChange && index == 0) || (row.RowType != RowType.SummaryCoveredRow
                    && row.RowType != RowType.CaptionCoveredRow && row.RowType != RowType.TableSummaryCoveredRow))
                    return true;
                return false;
            };

            this.Items.ForEach(row =>
            {
                if (canAdjust(row))
                {
                    if (row is DetailsViewDataRow)
                    {
                        row.VisibleColumns.ForEach(col =>
                        {
                            if (col.ColumnIndex >= index && col.ColumnIndex <= endIndex && (!(col.ColumnElement is DetailsViewContentPresenter) || (col.ColumnElement is DetailsViewContentPresenter && canUnloadDetailsViewandCoveredCell)))
                                this.UnloadUIElements(row, col);
                        });
                    }
                    else
                        row.VisibleColumns.ForEach(col =>
                        {
                            if (col.ColumnIndex >= index && col.ColumnIndex <= endIndex && (!(col.ColumnElement is GridDetailsViewExpanderCell) || (col.ColumnElement is GridDetailsViewExpanderCell && (canUnloadDetailsViewandCoveredCell || (this.Owner.inDetailsViewIndentChange && (this.Owner.DetailsViewDefinition == null || this.Owner.DetailsViewDefinition.Count == 0))))))
                                this.UnloadUIElements(row, col);
                        });
                }
            });

            for (int columnindex = index; columnindex <= endIndex; columnindex++)
            {
                this.Items.ForEach(row =>
                {
                    if (canAdjust(row))
                    {
                        if (row is DetailsViewDataRow)
                        {
                            var isIndentcolumn = row.VisibleColumns.FirstOrDefault(col => col.ColumnIndex == columnindex && (!(col.ColumnElement is DetailsViewContentPresenter) || (col.ColumnElement is DetailsViewContentPresenter) && canUnloadDetailsViewandCoveredCell));
                            if (isIndentcolumn != null)
                                row.VisibleColumns.Remove(isIndentcolumn);
                        }
                        else
                            RemoveColumnbyIndex(row, columnindex);
                    }

                });
            }

            this.Items.ForEach(row =>
            {
                if (row.RowType == RowType.SummaryCoveredRow || row.RowType == RowType.CaptionCoveredRow
                    || row.RowType == RowType.TableSummaryCoveredRow)
                {
                    var prevCoverdCells = new List<CoveredCellInfo>();
                    foreach (var item in (row as SpannedDataRow).CoveredCells)
                    {
                        prevCoverdCells.Add(item);
                    }
                    (row as SpannedDataRow).CoveredCells.Clear();
                    prevCoverdCells.ForEach(cell =>
                    {
                        if (endIndex < cell.Left)
                            (row as SpannedDataRow).CoveredCells.Add(new CoveredCellInfo(cell.Left - count, cell.Right - count,0,0));
                        else if (endIndex < cell.Right)
                            (row as SpannedDataRow).CoveredCells.Add(new CoveredCellInfo(cell.Left, cell.Right - count,0,0));
                    });
                }

                if (!(row is DetailsViewDataRow) || (row is DetailsViewDataRow && visiblecolumnIndex < 0 && !this.Owner.inDetailsViewIndentChange))
                {
                    row.VisibleColumns.ForEach(x =>
                    {
                        if (endIndex <= x.ColumnIndex)                        
                        {
                            if (this.Owner.CoveredCells.Count != 0)
                                this.Owner.coveredCells.Clear();

                            if (x.IsSpannedColumn)
                            {
                                x.isSpannedColumn = false;
                                x.IsSelectedColumn = false;
                                x.ColumnVisibility = Visibility.Visible;
                            }
                            if (x.ColumnIndex == endIndex)
                                x.ColumnIndex = -1;
                            else
                                x.ColumnIndex = x.ColumnIndex - count;
                        }
                    });
                }
            });

            canAdjust = null;
        }

        public void ApplyColumnSizeronInitial(double availableWidth)
        {
            this.Owner.GridColumnSizer.InitialRefresh(availableWidth);
        }

        public void RowsArranged(Size finalSize)
        {
            if (this.Owner.GroupDropArea != null && this.Owner.GridColumnDragDropController != null)
                this.Owner.GridColumnDragDropController.UpdatePopupPosition();
#if WPF
            if (!string.IsNullOrEmpty(this.Owner.SearchHelper.SearchText) && this.Owner.SearchHelper.CanHighlightSearchText)
                this.Items.ForEach(row => this.Owner.SearchHelper.SearchRow(row));
#endif
        }

        public void LineSizeChanged()
        {
            if (Owner.NotifyListener != null && Owner is DetailsViewDataGrid)
            {
                var parentGrid = Owner.NotifyListener.GetParentDataGrid();
                DetailsViewManager.AdjustParentsWidth(parentGrid, Owner);
                var dr = parentGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(
                    row => row.DetailsViewDataGrid == Owner);
                if (dr != null && dr.WholeRowElement.ItemsPanel != null)
                {
                    dr.WholeRowElement.ItemsPanel.InvalidateMeasure();
#if WPF
                    if(this.Owner.useDrawing)
                        dr.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                }
            }
        }
#endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
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
            isdisposed = true;
        }
    }
}

