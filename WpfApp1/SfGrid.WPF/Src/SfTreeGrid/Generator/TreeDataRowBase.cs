#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
#if UWP
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public abstract class TreeDataRowBase : ITreeDataRowElement, INotifyPropertyChanged, IDisposable
    {
        #region Fields

        private List<TreeDataColumnBase> visibleColumns = new List<TreeDataColumnBase>();

        object rowData = null;
        private bool isEditing;
        int rowIndex = -1;
        TreeNode node = null;
        Visibility rowVisibility = Visibility.Visible;
        TreeRowType rowType = TreeRowType.DefaultRow;
        internal bool IsEnsured;
        internal bool suspendUpdateStyle;

        protected internal TreeGridRowControlBase RowElement { get; set; }

        protected internal SfTreeGrid TreeGrid { get; set; }
		/// <summary>
        /// Gets or sets the value that indicates whether to update the VisibleColumn when it re-used for the same column index.
        /// </summary>
        /// <value><see langword="true"/> specifies the Column can update for reusing; otherwise, <see langword="false"/>.</value>
        internal bool isDirty = false;

        #endregion

        #region Property

        public object RowData
        {
            get { return rowData; }
            set
            {
                this.rowData = value;
                OnRowDataChanged();
            }
        }

        public TreeNode Node
        {
            get { return node; }
            set
            {
                this.node = value;
                OnPropertyChanged("Node");
            }
        }

        private int _level = -1;
        public int Level
        {
            get { return _level; }
            set
            {
                _level = value;
                OnPropertyChanged("Level");
            }
        }

        private bool _haschildnodes = false;
        public bool HasChildNodes
        {
            get { return _haschildnodes; }
            set
            {
                _haschildnodes = value;
                OnPropertyChanged("HasChildNodes");
            }
        }

        public int RowIndex
        {
            get { return rowIndex; }
            internal set
            {
                rowIndex = value;
#if WPF
                // UWP - 3529 - Issue 1 - Row index needs to updated in binding.
                OnPropertyChanged("RowIndex");
#endif
                OnRowIndexChanged();
            }
        }

        public bool IsEditing
        {
            get { return isEditing; }
            set { isEditing = value; }
        }

        public TreeRowType RowType
        {
            get { return rowType; }
            internal set { rowType = value; }
        }

        /// <summary>
        /// Get the List of DataColumnBase maintained in DataRow
        /// </summary>
        public List<TreeDataColumnBase> VisibleColumns
        {
            get { return visibleColumns; }
        }

        #endregion

        internal Visibility RowVisibility
        {
            get { return rowVisibility; }
            set
            {
                rowVisibility = value;
                OnRowVisibilityChanged();
            }
        }

        bool isSelectedRow = false;
        /// <summary>
        /// Gets or sets a value indicating whether this row is selected or not
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsSelectedRow
        {
            get { return isSelectedRow; }
            set
            {
                if (value == isSelectedRow) return;
                isSelectedRow = value;
                OnIsSelectedRowChanged();
                OnPropertyChanged("IsSelectedRow");
            }
        }

        private bool isCurrentRow;
        public bool IsCurrentRow
        {
            get { return isCurrentRow; }
            set { isCurrentRow = value; }
        }

        internal bool updateExpander = false;
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
        protected virtual TreeGridRowControlBase OnCreateRowElement()
        {
            if (this.RowType == TreeRowType.HeaderRow)
            {
                var row = new TreeGridHeaderRowControl
                {
                    Visibility = this.RowVisibility
                };
                row.DataRow = this;
                return row;
            }
            else
            {
                var row = new TreeGridRowControl
                {
                    DataContext = this.RowData,
                    Visibility = this.RowVisibility
                };
                row.DataRow = this;
                UpdateRowStyles(row);
                SetRowBindings(row);
                return row;
            }
        }

        protected abstract void OnGenerateVisibleColumns(VisibleLinesCollection visibleColumnLines);

        internal abstract void EnsureColumns(VisibleLinesCollection visibleColumnLines);

        internal abstract void UpdateRowStyles(ContentControl row);

        private void OnRowDataChanged()
        {
            if (this.RowElement != null)
                this.RowElement.DataContext = this.rowData;
            foreach (var dataColumn in this.VisibleColumns.Where(column => column.Renderer != null && column.Renderer.CanUpdateBinding(column.TreeGridColumn)))
            {
                dataColumn.UpdateBinding(this.rowData, false);
            }
        }

        protected virtual void OnRowIndexChanged()
        {
            if (this.RowIndex > 0)
            {
                this.VisibleColumns.ForEach(col => { col.UpdateCellStyle(); });
                if (!suspendUpdateStyle)
                    this.UpdateRowStyles(this.RowElement);
            }
        }

        private void OnRowVisibilityChanged()
        {
            this.RowElement.Visibility = this.rowVisibility;
            if (this.RowVisibility == Visibility.Visible)
            {
                if (this.RowElement.ItemsPanel != null)
                    this.RowElement.ItemsPanel.InvalidateMeasure();
                if (this.IsCurrentRow)
                {
                    var column = this.VisibleColumns.FirstOrDefault(dataColumn => dataColumn.IsCurrentCell);
                    if (column != null && ((column.IsEditing && column.Renderer != null && column.Renderer.HasCurrentCellState) || (column.TreeGridColumn != null && column.TreeGridColumn.CanFocus())))
                        column.Renderer.SetFocus(true);
                }
            }
        }

        private void OnIsSelectedRowChanged()
        {
            var rowElement = this.RowElement as TreeGridRowControl;
            if (rowElement == null)
                return;

            rowElement.IsSelected = this.IsSelectedRow;
        }

        private void OnIsFocusedRowChanged()
        {
            var rowElement = this.RowElement as TreeGridRowControl;
            if (rowElement == null)
                return;

            rowElement.IsFocusedRow = this.IsFocusedRow;
        }

        private void OnLevelChanged()
        {
            var rowElement = this.RowElement as TreeGridRowControl;
            if (rowElement == null)
                return;

            rowElement.IsFocusedRow = this.IsFocusedRow;
        }
        internal VisibleLineInfo GetColumnVisibleLineInfo(int index)
        {
            return this.TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLineAtLineIndex(index);
        }

        internal VisibleLineInfo GetRowVisibleLineInfo(int index)
        {
            return this.TreeGrid.TreeGridPanel.ScrollRows.GetVisibleLineAtLineIndex(index);
        }

        internal void InitializeTreeRow(VisibleLinesCollection visibleColumns)
        {
            this.OnGenerateVisibleColumns(visibleColumns);
            this.RowElement = OnCreateRowElement();
        }

        internal void SuspendUpdateStyle()
        {
            this.suspendUpdateStyle = true;
        }

        internal void ResumeUpdateStyle()
        {
            this.suspendUpdateStyle = false;
        }


        internal double GetVisibleLineOrigin()
        {
            int repeatSizeCount;
            int columnIndex = 0;
            if (this.TreeGrid.showRowHeader)
                columnIndex += 1;

            if (this.TreeGrid.TreeGridPanel.ColumnWidths.GetHidden(columnIndex, out repeatSizeCount))
            {
                columnIndex += repeatSizeCount;
            }
            var lineInfo = this.GetColumnVisibleLineInfo(columnIndex);
            if (lineInfo == null)
            {
                var lines = this.TreeGrid.TreeGridPanel.ScrollColumns.GetVisibleLines();
                if (lines.Count > lines.FirstBodyVisibleIndex)
                    lineInfo = lines[lines.FirstBodyVisibleIndex];
                else
                    return 0;
            }
            return lineInfo.ClippedOrigin;
        }

        protected internal IList<ITreeDataColumnElement> GetVisibleColumns()
        {
            return VisibleColumns.ToList<ITreeDataColumnElement>();
        }

        protected virtual void CollapseColumn(TreeDataColumnBase column)
        {
            column.IsEnsured = true;
            column.ColumnVisibility = Visibility.Collapsed;
        }

        internal virtual void UpdateCurrentCellSelection()
        {

        }

        protected virtual void SetRowBindings(TreeGridRowControl rowControl)
        {
            if (this.RowType == TreeRowType.HeaderRow)
                return;

            if (TreeGridRowControl.SelectionBackgroundProperty.GetMetadata(typeof(FrameworkElement)).DefaultValue == rowControl.SelectionBackground)
                rowControl.SelectionBackground = TreeGrid.SelectionBackground;

            rowControl.IsSelected = this.IsSelectedRow;
            rowControl.IsFocusedRow = this.IsFocusedRow;
        }

        protected virtual void SetCellBindings(TreeDataColumnBase datacolumn)
        {
            if (datacolumn.ColumnType == TreeColumnType.RowHeader || datacolumn.ColumnType == TreeColumnType.ColumnHeader || datacolumn.ColumnElement == null)
                return;

            var columnElement = datacolumn.ColumnElement as TreeGridCell;
            if (columnElement == null)
                return;

            columnElement.CurrentCellBorderBrush = TreeGrid.CurrentCellBorderBrush;
            columnElement.CurrentCellBorderThickness = TreeGrid.CurrentCellBorderThickness;

            if (datacolumn.ColumnType == TreeColumnType.ExpanderColumn)
            {
                var bind = new Binding
                {
                    Path = new PropertyPath("Level"),
                    Source = this,
                    Mode = BindingMode.TwoWay
                };
                columnElement.SetBinding(TreeGridExpanderCell.LevelProperty, bind);

                bind = new Binding
                {
                    Path = new PropertyPath("HasChildNodes"),
                    Source = this,
                    Mode = BindingMode.TwoWay
                };
                columnElement.SetBinding(TreeGridExpanderCell.HasChildNodesProperty, bind);
                bind = new Binding
                {
                    Path = new PropertyPath("Node.IsChecked"),
                    Source = this,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                };
                columnElement.SetBinding(TreeGridExpanderCell.IsCheckedProperty, bind);
            }
        }

        internal virtual TreeDataColumnBase CreateColumn(int index)
        {
            throw new NotImplementedException();
        }

        internal virtual void CreateRowHeaderColumn(int index)
        {
            var dc = new TreeDataColumn();
            dc.IsEnsured = true;
            dc.DataRow = this;
            dc.ColumnIndex = index;
            dc.TreeGridColumn = null;
            dc.Renderer = this.TreeGrid.CellRenderers["RowHeader"];
            dc.ColumnType = TreeColumnType.RowHeader;
            if (this.RowIndex >= 0 && this.RowIndex <= this.TreeGrid.GetHeaderIndex())
            {
                dc.Renderer = null;
                dc.ColumnElement = new TreeGridRowHeaderIndentCell();
            }
            else
                dc.InitializeColumnElement(this.RowData, false);

            this.VisibleColumns.Add(dc);
            ApplyRowHeaderVisualState();
        }

        internal void ApplyRowHeaderVisualState()
        {
            if (!TreeGrid.showRowHeader || this.RowType == TreeRowType.HeaderRow)
                return;

            var columnBase = this.VisibleColumns.FirstOrDefault(col => col.ColumnType == TreeColumnType.RowHeader);
            if (columnBase == null)
                return;

            var rowHeaderCell = columnBase.ColumnElement as TreeGridRowHeaderCell;
            if (rowHeaderCell == null)
                return;
            if (this.IsCurrentRow)
                rowHeaderCell.State = "CurrentRow";
            else
                rowHeaderCell.State = "Normal";
            if (this.IsEditing)
                rowHeaderCell.State = "EditingRow";
            rowHeaderCell.ApplyVisualState();

            var dataValidation = this.RowData;
            if (dataValidation != null)
            {
#if !SyncfusionFramework4_0 || UWP
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

                }
#endif
#if WPF
                    if ((dataValidation as IDataErrorInfo) != null)
                    {
                        if (TreeGridDataValidation.ValidateRow(this.RowData))
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
                            if (this.TreeGrid.View != null && this.TreeGrid.View.CurrentItem == null)
                                rowHeaderCell.State = "Normal";
                        }
                    }
#endif
            }
            rowHeaderCell.ApplyVisualState();
        }

        internal protected virtual double GetColumnSize(int index, bool lineNull)
        {
            if (lineNull)
            {
                DoubleSpan[] CurrentPos = this.TreeGrid.TreeGridPanel.ScrollColumns.RangeToRegionPoints(index, index, true);
                return CurrentPos[1].Length;
            }
            var line = GetColumnVisibleLineInfo(index);
            if (line == null)
                return 0;
            return line.Size;
        }

        public virtual void MeasureElement(Size size)
        {
            this.RowElement.Measure(size);
        }

        public virtual void ArrangeElement(Rect rect)
        {
            this.RowElement.Arrange(rect);
        }

        #region IDataRow

        public FrameworkElement Element
        {
            get { return this.RowElement; }
        }

        public int Index
        {
            get { return this.rowIndex; }
        }

        #endregion

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

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataRowBase"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataRowBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.visibleColumns != null)
                {
                    foreach (var item in visibleColumns)
                        item.Dispose();
                    visibleColumns.Clear();
                    visibleColumns = null;
                }
                if (this.RowElement != null)
                {
                    RowElement.Dispose();
                    this.RowElement = null;
                }
                this.rowData = null;
                this.TreeGrid = null;
                this.node = null;
            }
        }
        #endregion
    }
}
