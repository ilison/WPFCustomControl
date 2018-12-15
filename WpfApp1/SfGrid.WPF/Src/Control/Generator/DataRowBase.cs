#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Syncfusion.UI.Xaml.Utility;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Syncfusion.UI.Xaml.Utility;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public abstract class DataRowBase : IRowElement, IDisposable
    {
        #region Fields
        internal bool isDataRowDirty = false;
        bool isSelectedRow = false;
        List<DataColumnBase> visibleColumns = new List<DataColumnBase>();
        object rowData = null;
        private bool isEditing;
        int rowIndex = -1;
        Visibility rowVisibility = Visibility.Visible;
        RowType rowType = RowType.DefaultRow;
        bool isFixedRow = false;
        private bool isdisposed = false;
        Rect arrangeRect;
        protected bool suspendUpdateStyle;
        internal bool IsEnsured;
        internal int RowLevel = 1;
        internal RowRegion rowRegion = RowRegion.Body;

        internal Func<int, int, double> GetMergedCellColumnSize;
        internal Func<int, int, double> GetMergedCellRowSize;
        internal bool isSpannedRow;
        /// <summary>
        /// Property that uses to decide the UnBoundRows to be reset it again at run time. <see cref="Syncfusion.UI.Xaml.Grid.InValidateUnBoundDataRow"/>
        /// in DataGrid.cs file. where this flag will be enabled for the row index.
        /// And Also if isDirty is true, In EnsureColumns to update corresponding row columns even if column index is already in  existing visible columns.
        /// </summary>
        internal bool isDirty = false;

        #endregion

        #region Property

        /// <summary>
        /// Gets the row element <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/> of SfDataGrid.
        /// </summary>
        protected internal VirtualizingCellsControl WholeRowElement { get; private set; }

        private bool isExpanded;
        public bool IsExpanded
        {
            get { return isExpanded; }
            set
            {
                isExpanded = value;
                OnExpandedStateChanged();
            }
        }

        public bool IsEditing
        {
            get { return isEditing; }
            set { isEditing = value; }
        }

        public object RowData
        {
            get
            {
                return rowData;
            }
            set
            {
                this.rowData = value;
#if WPF                                                
                OnPropertyChanged("RowData");
#endif
                OnRowDataChanged();
            }
        }

        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
            internal set
            {
                rowIndex = value;
            }
        }

        public RowType RowType
        {
            get { return rowType; }
            internal set { rowType = value; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this row is selected or not
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsSelectedRow
        {
            get
            {
                return isSelectedRow;
            }
            set
            {
                if (value == isSelectedRow) return;
                isSelectedRow = value;
                OnIsSelectedRowChanged();
                OnPropertyChanged("IsSelectedRow");
            }
        }

        private bool isFocusedRow;

        /// <summary>
        /// Get or sets the value indicating row is focused or not.
        /// </summary>
        public bool IsFocusedRow
        {
            get { return isFocusedRow; }
            set
            {
                if (value == isFocusedRow) return;
                isFocusedRow = value;
                OnIsFocusedRowChanged();
                OnPropertyChanged("IsFocusedRow");
            }
        }


        private bool isCurrentRow;

        public bool IsCurrentRow
        {
            get
            {
                return isCurrentRow;
            }
            set
            {
                isCurrentRow = value;
            }
        }

        /// <summary>
        /// This flag will be set to True only for FixedGroupCaptions.
        /// </summary>
        public bool IsFixedRow
        {
            get { return isFixedRow; }
            internal set { isFixedRow = value; }
        }

        //WPF-21874 Provision to make VisibleColumns as Public 
        /// <summary>
        ///Get the List of DataColumnBase maintained in DataRow
        /// </summary>
        public List<DataColumnBase> VisibleColumns
        {
            get { return visibleColumns; }
        }

        #endregion

        #region internal Properties

        internal Visibility RowVisibility
        {
            get
            {
                return rowVisibility;
            }
            set
            {
                // If row is DetailsViewDataRow, need to raise DetailsViewLoading/ Unloading event based on the visibility
                if (this is DetailsViewDataRow)
                {
                    var row = this as DetailsViewDataRow;
                    if (value != rowVisibility || (value == Visibility.Visible && !row.DetailsViewDataGrid.IsLoadedEventFired))
                        DetailsViewManager.RaiseDetailsViewEvents(row, value);
                }
                rowVisibility = value;
                OnRowVisibilityChanged();
            }
        }

        public bool IsSpannedRow
        {
            get { return isSpannedRow; }
        }

        #endregion

        #region Property changed

        protected virtual void OnRowDataChanged()
        {
            if (this.WholeRowElement == null)
                return;

            foreach (var dataColumn in this.VisibleColumns.Where(column => column.Renderer != null && column.Renderer.CanUpdateBinding(column.GridColumn)))
            {
                dataColumn.UpdateBinding(this.rowData, false);
            }
            this.WholeRowElement.DataContext = this.rowData;
        }

        protected internal virtual void OnRowIndexChanged()
        {
            if (this.RowIndex <= 0)
                return;

            this.VisibleColumns.ForEach(col =>
            {
                col.RowIndex = this.RowIndex;
            });

            if (suspendUpdateStyle)
                return;

            if (this.RowData != null || this.RowType == RowType.AddNewRow || this.RowType == RowType.FilterRow)
            {
                this.VisibleColumns.ForEach(col =>
                {
                    col.RowIndex = this.RowIndex;
                    col.UpdateCellStyle(this.RowData);
                });
            }

            this.UpdateRowStyles(this.WholeRowElement);
        }

        internal virtual void OnRowVisibilityChanged()
        {
            this.WholeRowElement.Visibility = this.rowVisibility;
            if (this.RowVisibility == Visibility.Visible)
            {
                if (this.WholeRowElement.ItemsPanel != null)
                    this.WholeRowElement.ItemsPanel.InvalidateMeasure();
                if (this.IsCurrentRow)
                {
                    var column = this.VisibleColumns.FirstOrDefault(dataColumn => dataColumn.IsCurrentCell);
                    if (column != null && ((column.IsEditing && column.Renderer != null && column.Renderer.HasCurrentCellState) || (column.GridColumn != null && column.GridColumn.CanFocus())))
                        column.Renderer.SetFocus(true);
                }
            }
        }

        private void OnIsSelectedRowChanged()
        {
            var rowElement = this.WholeRowElement as VirtualizingCellsControl;
            if (rowElement == null)
                return;

            if (this.RowType == Grid.RowType.HeaderRow || this.RowType == Grid.RowType.DetailsViewRow)
                return;

            rowElement.SelectionBorderVisiblity = this.IsSelectedRow ? Visibility.Visible : Visibility.Collapsed;
        }

        private void OnIsFocusedRowChanged()
        {
            var rowElement = this.WholeRowElement as VirtualizingCellsControl;
            if (rowElement == null)
                return;

            if (this.RowType == Grid.RowType.HeaderRow || this.RowType == Grid.RowType.DetailsViewRow)
                return;

            rowElement.CurrentFocusRowVisibility = this.IsFocusedRow ? Visibility.Visible : Visibility.Collapsed;
        }
        
        protected virtual void OnExpandedStateChanged()
        {

        }

        #endregion

        #region Ctor

        protected DataRowBase()
        {

        }

        #endregion

        #region internal methods

        internal void InitializeDataRow(VisibleLinesCollection visibleColumns)
        {
            this.OnGenerateVisibleColumns(visibleColumns);
            //µÃµ½ HeaderRowControl
            this.WholeRowElement = OnCreateRowElement();
        }

        internal void SuspendUpdateStyle()
        {
            this.suspendUpdateStyle = true;
        }

        internal void ResumeUpdateStyle()
        {
            this.suspendUpdateStyle = false;
        }

        #endregion

        #region abstract methods

        protected abstract VirtualizingCellsControl OnCreateRowElement();

        protected abstract void OnGenerateVisibleColumns(VisibleLinesCollection visibleColumnLines);

        internal abstract void EnsureColumns(VisibleLinesCollection visibleColumnLines);

        internal abstract void UpdateRowStyles(ContentControl row);

        internal abstract void UpdateFixedColumnState(DataColumnBase dc);

        internal abstract void UpdateFixedRowState();

        #endregion

        #region protected methods

        protected virtual DataRowBase GetDataRow()
        {
            return this;
        }

        protected internal IList<DataColumnBase> GetVisibleColumns()
        {
            return VisibleColumns.ToList<DataColumnBase>();
        }

        protected virtual void CollapseColumn(DataColumnBase column)
        {
            column.IsEnsured = true;
            column.ColumnVisibility = Visibility.Collapsed;
        }

        public virtual void MeasureElement(Size size)
        {
            this.WholeRowElement.Measure(size);
        }

        public virtual void ArrangeElement(Rect rect)
        {
            this.WholeRowElement.Arrange(rect);
        }

        internal virtual void UpdateCurrentCellSelection()
        {

        }

        protected abstract DataColumnBase CreateIndentColumn(int index);


        protected internal virtual VisibleLineInfo GetColumnVisibleLineInfo(int index)
        {
            return null;
        }

        protected internal virtual VisibleLineInfo GetRowVisibleLineInfo(int index)
        {
            return null;  
        }

        protected internal virtual double GetColumnSize(int index, bool lineNull)
        {
            return 0;
        }

        protected internal virtual double GetRowSize(DataColumnBase dataColumn, bool lineNull)
        {
            return 0;
        }

        #endregion

        #region IDataRow

        public FrameworkElement Element
        {
            get { return this.WholeRowElement; }
        }

        public int Index
        {
            get { return this.rowIndex; }
        }

        public RowRegion RowRegion
        {
            get { return this.rowRegion; }
            internal set { this.rowRegion = value; }
        }

        public int Level
        {
            get { return RowLevel; }
        }

        public Rect ArrangeRect
        {
            get { return arrangeRect; }
            set { arrangeRect = value; }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region IComparable
        public int CompareTo(object obj)
        {
            IElement thisdr = this;
            var dr = obj as IElement;
            if (dr != null)
            {
                if (thisdr.Index > dr.Index)
                    return 1;
                else if (thisdr.Index < dr.Index)
                    return -1;
                else
                    return 0;
            }
            return 0;
        }
        #endregion

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DataRowBase"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DataRowBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                if (this.visibleColumns != null)
                {
                    foreach (var item in visibleColumns)
                        item.Dispose();
                    visibleColumns.Clear();
                    visibleColumns = null;
                }
                if (this.WholeRowElement != null)
                {
                    WholeRowElement.Dispose();
                    this.WholeRowElement = null;
                }
                this.rowData = null;
            }
            isdisposed = true;
        }

        #endregion
    }

    public abstract class GridDataRow : DataRowBase
    {
        private bool isdisposed = false;

        #region Property

        internal SfDataGrid DataGrid
        {
            get;
            set;
        }

        #endregion

        #region Protected Methods

        protected override void OnExpandedStateChanged()
        {
            if (DataGrid == null || DataGrid.DetailsViewDefinition == null || DataGrid.DetailsViewDefinition.Count <= 0)
                return;
            var expanderColumn = this.VisibleColumns.FirstOrDefault(col => col.IsExpanderColumn);
            if (this.RowRegion == Grid.RowRegion.Header)
                return;
            else if (expanderColumn != null)
            {
                var control = expanderColumn.ColumnElement as GridDetailsViewExpanderCell;
                if (control != null) control.IsExpanded = IsExpanded;
            }
            else
            {
                //UWP-4547 DetailsViewDataGrid is expanded while we setting ExpandercolumnWidth as zero.              
                int startColumnIndex = this.DataGrid.ShowRowHeader ? 1 : 0;
                startColumnIndex += this.DataGrid.View.GroupDescriptions.Count;

                // Skip the row for expanding, whether the RowType is DetailsViewRow.
                if (this.RowType == RowType.DetailsViewRow)
                    return;
                DataGrid.DetailsViewManager.OnDetailsViewExpanderStateChanged(new RowColumnIndex(this.RowIndex, startColumnIndex), this.IsExpanded);
            }
        }

        protected internal override VisibleLineInfo GetColumnVisibleLineInfo(int index)
        {
            VisibleLineInfo line = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(index);
            return line;
        }

        protected internal override VisibleLineInfo GetRowVisibleLineInfo(int index)
        {
            VisibleLineInfo line = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(index);
            return line;
        }

        protected double GetVisibleLineOrigin()
        {
            int repeatSizeCount;
            int columnIndex = this.RowData is Group ? this.RowLevel - 1 : this.RowLevel;
            if (this.DataGrid.showRowHeader)
                columnIndex += 1;

            //WPF-25764 When we remove the selection in UnBoundRow with Multiple selection, dotted border is applied for indentcell,
            //we have Update the FocusRow based on ClippedOrigin based on the ColumnIndex so calculate the columnindex for selected and unselected rows.
            if (this.DataGrid.HasView && (this.RowType == RowType.UnBoundRow || ((this.RowType == RowType.AddNewRow || this.RowType == RowType.FilterRow) && this.IsSelectedRow)))
            {
                columnIndex += this.DataGrid.View.GroupDescriptions.Count;
                columnIndex = this.DataGrid.DetailsViewManager.HasDetailsView ? columnIndex + 1 : columnIndex;
            }

            if (this.DataGrid.DetailsViewManager.HasDetailsView && (this.RowType == RowType.DefaultRow || RowType == RowType.DetailsViewRow))
                columnIndex += 1;

            if (this.RowLevel < 0)
                columnIndex = 0;
            if (this.DataGrid.VisualContainer.ColumnWidths.GetHidden(columnIndex, out repeatSizeCount))
            {
                columnIndex += repeatSizeCount;
            }
            var lineInfo = this.GetColumnVisibleLineInfo(columnIndex);
            if (lineInfo == null)
            {
                var lines = this.DataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
                if (lines.FirstBodyVisibleIndex > 0)
                {
                    lineInfo = lines[lines.FirstBodyVisibleIndex - 1];
                    if (lineInfo != null)
                        return lineInfo.Corner;
                }

                if (DataGrid.showRowHeader)
                    return DataGrid.RowHeaderWidth;
                else
                    return 0d;
            }
            return lineInfo.ClippedOrigin;
        }

        protected virtual bool AllowRowHoverHighlighting()
        {
            return DataGrid.AllowRowHoverHighlighting;
        }

        protected internal override double GetColumnSize(int index, bool lineNull)
        {
            if (lineNull)
            {
                DoubleSpan[] CurrentPos = this.DataGrid.VisualContainer.ScrollColumns.RangeToRegionPoints(index, index, true);
                return CurrentPos[1].Length;
            }
            var line = GetColumnVisibleLineInfo(index);
            if (line == null)
                return 0;
            return line.Size;
        }

        protected internal override double GetRowSize(DataColumnBase dataColumn, bool lineNull)
        {
            var index = dataColumn.RowIndex;
            if (lineNull)
            {
                DoubleSpan[] CurrentPos = this.DataGrid.VisualContainer.ScrollRows.RangeToRegionPoints(index, index, true);
                return CurrentPos[1].Length;
            }
            var line = GetRowVisibleLineInfo(index);
            if (line == null)
                return 0;
            return line.Size;
        }

        protected void SetSelectionBorderBindings(VirtualizingCellsControl cellsControl)
        {                 
            if (VirtualizingCellsControl.RowSelectionBrushProperty.GetMetadata(typeof(FrameworkElement)).DefaultValue == cellsControl.RowSelectionBrush)            
                cellsControl.RowSelectionBrush = DataGrid.RowSelectionBrush;

            cellsControl.SelectionBorderVisiblity = this.IsSelectedRow ? Visibility.Visible : Visibility.Collapsed;
            cellsControl.CurrentFocusRowVisibility = this.IsFocusedRow ? Visibility.Visible : Visibility.Collapsed;
            cellsControl.GroupRowSelectionBrush = DataGrid.GroupRowSelectionBrush;
            cellsControl.RowHoverBackgroundBrush = DataGrid.RowHoverHighlightingBrush;         
        }

        protected void SetCurrentCellBorderBinding(UIElement columnElement)
        {
            var gridCell = columnElement as GridCell;
            if (gridCell == null)
                return;                       
            gridCell.CurrentCellBorderThickness = DataGrid.CurrentCellBorderThickness;
            gridCell.CurrentCellBorderBrush = DataGrid.CurrentCellBorderBrush;
            gridCell.CellSelectionBrush = DataGrid.RowSelectionBrush;                       
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.DataRowBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            base.Dispose(isDisposing);
            if (isDisposing)
                this.DataGrid = null;
            isdisposed = true;
        }

        protected void CreateRowHeaderColumn(int index)
        {
            DataColumnBase dc = new DataColumn();
            if (this.RowType == RowType.UnBoundRow)
                dc.isUnBoundRowCell = true;
            dc.IsEnsured = true;
            dc.RowIndex = this.RowIndex;
            dc.ColumnIndex = index;
            dc.IsEditing = false;
            dc.GridColumn = null;
            dc.Renderer = this.DataGrid.CellRenderers["RowHeader"];
            dc.SelectionController = this.DataGrid.SelectionController;
            if (this.RowIndex >= 0 && this.RowIndex <= this.DataGrid.GetHeaderIndex())
            {
                dc.Renderer = null;
                dc.ColumnElement = new GridRowHeaderIndentCell();
            }
            else
                dc.InitializeColumnElement(this.RowData, false);
            this.VisibleColumns.Add(dc);
            ApplyRowHeaderVisualState();
        }

        protected override void CollapseColumn(DataColumnBase column)
        {
            base.CollapseColumn(column);
        }

        #endregion

        #region Internal Methods
        internal void ApplyRowHeaderVisualState()
        {
            if (this.DataGrid != null && !this.DataGrid.showRowHeader)
                return;
            var columnBase = this.VisibleColumns.FirstOrDefault(col => col.ColumnElement is GridRowHeaderCell);
            if (columnBase == null)
                return;
            GridRowHeaderCell rowHeaderCell = columnBase.ColumnElement as GridRowHeaderCell;
            if (rowHeaderCell == null)
                return;
            if (this.IsCurrentRow)
                rowHeaderCell.State = "CurrentRow";
            else if (this.RowType == RowType.AddNewRow)
                rowHeaderCell.State = "AddNewRow";
            else if (this.RowType == RowType.FilterRow)
                rowHeaderCell.State = "FilterRow";
            else
                rowHeaderCell.State = "Normal";

            if (this.IsEditing)
                rowHeaderCell.State = "EditingRow";

            var dataValidation = this.RowData;

            if (dataValidation != null)
            {
#if !SyncfusionFramework4_0 && !SyncfusionFramework3_5 || UWP
                if ((dataValidation as INotifyDataErrorInfo) != null)
                {
                    if (DataValidation.ValidateRowINotifyDataErrorInfo(this.RowData))
                    {
                        rowHeaderCell.RowErrorMessage = GridResourceWrapper.RowErrorMessage;
                        if (!rowHeaderCell.State.Equals("Normal"))
                            rowHeaderCell.State = "Error_CurrentRow";
                        else
                            rowHeaderCell.State = "Error";
                        rowHeaderCell.ApplyVisualState();
                        return;
                    }
                    else
                    {

                        if ((this.DataGrid.View.CurrentItem == null && (RowType == RowType.AddNewRow && !this.IsEditing)))
                            rowHeaderCell.State = "Normal";
                    }
                }
#endif
#if WPF
                if ((dataValidation as IDataErrorInfo) != null)
                {
                    if (DataValidation.ValidateRow(this.RowData))
                    {
                        rowHeaderCell.RowErrorMessage = (dataValidation as IDataErrorInfo).Error;
                        if (!rowHeaderCell.State.Equals("Normal"))
                            rowHeaderCell.State = "Error_CurrentRow";
                        else
                            rowHeaderCell.State = "Error";
                        rowHeaderCell.ApplyVisualState();
                        return;
                    }
                    else
                    {
                        if ((this.DataGrid.View != null && this.DataGrid.View.CurrentItem == null) && this.RowType != RowType.AddNewRow)
                            rowHeaderCell.State = "Normal";
                    }
                }
#endif
            }
            if (this.RowType == Grid.RowType.TableSummaryRow || this.RowType == Grid.RowType.TableSummaryCoveredRow)
            {
                rowHeaderCell.State = "Normal";
                rowHeaderCell.GridCellRegion = "TableSummaryCell";
            }
            rowHeaderCell.ApplyVisualState();

        }

        /// <summary>
        /// To Get the previous column index to show the border of the first Footer column.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int GetPreviousColumnIndex(int index)
        {
            var column = this.VisibleColumns.FirstOrDefault(dataColumn => dataColumn.ColumnIndex == index - 1);
            if (column != null && column.GridColumn != null)
            {
                if (!column.GridColumn.IsHidden)
                    return column.ColumnIndex;
                else
                    return GetPreviousColumnIndex(column.ColumnIndex);
            }
            return -1;
        }

        /// <summary>
        /// Updates the Row border state for Frozen and Footer rows.
        /// </summary>
        internal override void UpdateFixedRowState()
        {
            if (this.RowRegion == RowRegion.Body)
                this.WholeRowElement.RowBorderState = "NormalRow";
            if (this.RowRegion == RowRegion.Footer && this.RowIndex == this.DataGrid.VisualContainer.RowCount - this.DataGrid.VisualContainer.FooterRows)
                this.WholeRowElement.RowBorderState = "FooterRow";
            else if (this.RowRegion == RowRegion.Header && this.RowIndex == this.DataGrid.VisualContainer.FrozenRows - 1 && !(this.WholeRowElement is TableSummaryRowControl) && !(this.WholeRowElement is AddNewRowControl))
                this.WholeRowElement.RowBorderState = "FrozenRow";
            else
                this.WholeRowElement.RowBorderState = "NormalRow";
        }

        /// <summary>
        /// Updates the Cell border state for footer columns and before footer columns.
        /// </summary>
        /// <param name="dc"></param>
        internal override void UpdateFixedColumnState(DataColumnBase dc)
        {
            if (this.DataGrid.VisualContainer.FooterColumns == 0 || dc.ColumnIndex <= this.DataGrid.VisualContainer.FrozenColumns - 1)
            {
                //When footer column is set to zero then all the cells would be normal cell.
                if (this.RowType != Grid.RowType.HeaderRow)
                {
                    if (dc.ColumnIndex == this.DataGrid.VisualContainer.FrozenColumns - 1)
                        dc.OnGridCellRegionChanged(GridCellRegion.FrozenColumnCell);
                    else
                        dc.OnGridCellRegionChanged(GridCellRegion.NormalCell);                        
                }
                else
                {
                    if (dc.ColumnIndex == this.DataGrid.VisualContainer.FrozenColumns - 1)
                        (dc.ColumnElement as GridHeaderCellControl).GridCellRegion = GridCellRegion.FrozenColumnCell;
                    else
                        (dc.ColumnElement as GridHeaderCellControl).GridCellRegion = GridCellRegion.NormalCell;
                }
                return;
            }

            var columnCount = this.DataGrid.VisualContainer.ColumnCount;
            int previousColumnIndex = this.GetPreviousColumnIndex(columnCount - this.DataGrid.FooterColumnCount);
            //lastCellIndex is used to draw right border for the last body column when all the footer columns are hidden.
            var lastCellIndex = this.DataGrid.GetLastColumnIndex();
            //isFooterColumnHidden is used to draw the right border for previous of last body column when the last body column is in hidden.
            bool isFooterColumnHidden = false;

            if (dc.ColumnIndex > columnCount - this.DataGrid.FooterColumnCount)
            {
                isFooterColumnHidden = GetPreviousColumnIndex(dc.ColumnIndex) < columnCount - this.DataGrid.FooterColumnCount;
            }
            if (this.RowType != Grid.RowType.HeaderRow)
            {
                if (dc.ColumnIndex == previousColumnIndex && dc.ColumnIndex != lastCellIndex)
                    dc.OnGridCellRegionChanged(GridCellRegion.BeforeFooterColumnCell);
                else if (dc.ColumnIndex == columnCount - this.DataGrid.FooterColumnCount || isFooterColumnHidden)
                    dc.OnGridCellRegionChanged(GridCellRegion.FooterColumnCell);
                else
                    dc.OnGridCellRegionChanged(GridCellRegion.NormalCell);

                if (this.IsSpannedRow)
                {
                    var coveredCellInfo = this.DataGrid.CoveredCells.GetCoveredCellInfo(dc);
                    if (coveredCellInfo != null && previousColumnIndex == coveredCellInfo.Right)
                        dc.OnGridCellRegionChanged(GridCellRegion.BeforeFooterColumnCell);
                }
            }
            else if (dc.ColumnElement is GridHeaderCellControl)
            {
                if (dc.ColumnIndex == columnCount - this.DataGrid.FooterColumnCount || isFooterColumnHidden)
                    (dc.ColumnElement as GridHeaderCellControl).GridCellRegion = GridCellRegion.FooterColumnCell;
                else if (dc.ColumnIndex == previousColumnIndex && dc.ColumnIndex != lastCellIndex)
                    (dc.ColumnElement as GridHeaderCellControl).GridCellRegion = GridCellRegion.BeforeFooterColumnCell;
                else
                    (dc.ColumnElement as GridHeaderCellControl).GridCellRegion = GridCellRegion.NormalCell;
            }
        }
#if WPF
        internal override void OnRowVisibilityChanged()
        {
            base.OnRowVisibilityChanged();
            if (this.RowVisibility == Visibility.Visible)
            {
                if (this.DataGrid.useDrawing)
                    this.WholeRowElement.ItemsPanel.InvalidateVisual();
            }
        }
#endif
        #endregion
    }
}
