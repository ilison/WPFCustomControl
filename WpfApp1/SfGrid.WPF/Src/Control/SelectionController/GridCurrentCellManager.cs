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
using System.Linq;
using System.Text;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.Data.Helper;
namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    using PropertyDescriptorCollection = Syncfusion.Data.PropertyInfoCollection;
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
    using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.Devices.Input;
    using Syncfusion.UI.Xaml.Grid.Cells;
    using Windows.UI.Xaml;
    using Windows.UI.Core;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Globalization;
#else
    using Syncfusion.UI.Xaml.Grid.Cells;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using System.Windows.Data;
#endif
    /// <summary>
    /// Represents a class that manages the current cell operation in SfDataGrid.
    /// </summary>
    public class GridCurrentCellManager : IDisposable
    {
        #region Fields

        private RowColumnIndex currentRowColumnIndex;
        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> class.
        /// </summary>
        protected SfDataGrid dataGrid;
        internal object oldCellValue;
        internal RowColumnIndex previousRowColumnIndex;
        private bool isdisposed = false;
        Key lastPressedKey = Key.Space;
        bool setFocusForGrid = true;

        /// <summary>
        /// Gets or sets the action that encapsulates the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SuspendUpdates"/> method.
        /// </summary>
        protected internal Action SuspendUpdates;

        /// <summary>
        /// Gets or sets the action that encapsulates the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.ResumeUpdates"/> method.
        /// </summary>
        protected internal Action ResumeUpdates;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellManager"/> class.
        /// </summary>
        /// <param name="grid">
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> class.
        /// </param>
        public GridCurrentCellManager(SfDataGrid grid)
        {
            dataGrid = grid;
            CurrentRowColumnIndex = new RowColumnIndex(-1, -1);
            previousRowColumnIndex = new RowColumnIndex(-1, -1);
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is placed on the AddNewRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current cell is placed on the AddNewRow; otherwise, <b>false</b>.
        /// </value>
        protected internal bool IsAddNewRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is placed on the FilterRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current cell is placed on the FilterRow; otherwise, <b>false</b>.
        /// </value>
        protected internal bool IsFilterRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is placed on the UnBoundRow.
        /// </summary>
        /// <value>
        /// <value>
        /// <b>true</b> if the current cell is placed on the UnBoundRow; otherwise, <b>false</b>.
        /// </value>
        protected internal bool IsUnBoundRow
        {
            get;
            set;
        }

        #endregion

        #region Public Properties
        /// <summary>
        /// Gets the current <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the row or cell that contains the current cell.
        /// </summary>
        public RowColumnIndex CurrentRowColumnIndex
        {
            get { return currentRowColumnIndex; }
            internal set { currentRowColumnIndex = value; }
        }

        /// <summary>
        /// Gets or sets the currently active cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.DataColumnBase"/> that represents the current cell. 
        /// Returns null if there is no currently active cell.
        /// </value>
        private DataColumnBase _currentCell;
        public DataColumnBase CurrentCell
        {
            get
            {
                return _currentCell;
            }
            set
            {
                _currentCell = value;
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> of corresponding unbound row that contains current cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> of unbound row that contains current cell.
        /// Returns <b>null</b> if there is no current cell in unbound row.      
        /// </value>      
        protected internal GridUnBoundRow CurrentUnboundRowInfo
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a value that indicates whether the SfDataGrid that contains the currently active cell.
        /// </summary>
        public bool HasCurrentCell
        {
            get { return CurrentCell != null && CurrentCell is DataColumn; }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Updates the CurrentCell and when doing maipulation operation in TableSummarries and UnBoundrows, StackedHeaders
        /// </summary>
        /// <param name="isAddNewRow"></param>
        /// <param name="action"></param>
        /// <param name="count"></param>
        internal void ProcessOnDataRowCollectionChanged(bool needToAdd, int count)
        {
            if ((this.IsAddNewRow && this.dataGrid.AddNewRowPosition == AddNewRowPosition.None) ||
                (this.IsFilterRow && this.dataGrid.FilterRowPosition == FilterRowPosition.None))
            {
                this.RemoveCurrentCell(this.CurrentRowColumnIndex);
                this.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentRowColumnIndex.ColumnIndex));
            }
            else
            {
                int rowIndex = this.CurrentRowColumnIndex.RowIndex;
                if (needToAdd)
                    rowIndex += count;
                else
                    rowIndex -= count;

                if (rowIndex != this.CurrentRowColumnIndex.RowIndex)
                    this.SetCurrentRowIndex(rowIndex);
            }
        }

        internal void UpdateCurrentCellInfo(RowColumnIndex rowColumnIndex)
        {
            GridCellInfo cellInfo = null;
            if (rowColumnIndex.RowIndex > 0)
            {
                cellInfo = this.dataGrid.GetGridCellInfo(rowColumnIndex);
            }
            else if (this.dataGrid.CurrentColumn != null)
            {
                cellInfo = new GridCellInfo(this.dataGrid.CurrentColumn, null, null);
            }
            this.dataGrid.CurrentCellInfo = cellInfo;
        }

        protected internal void UpdateGridProperties(RowColumnIndex rowColumnIndex)
        {
            bool needToMove = true;
            SetCurrentRowColumnIndex(rowColumnIndex);
            //Here we have checked whether the rowIndex is UnBoundRow or not.
            IsUnBoundRow = this.dataGrid.IsUnBoundRow(rowColumnIndex.RowIndex);
            //Here we have check the RowColumnIndex is AddNewRow or Not
            this.IsAddNewRow = this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex);
            this.IsFilterRow = this.dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex);

            //UnBoundRow details has been stored when the CurrentCell is in UnBoundRow and it is to maintain the CurrentCell
            //on real time updates.
            if (IsUnBoundRow)
                this.CurrentUnboundRowInfo = this.dataGrid.GetUnBoundRow(this.CurrentRowColumnIndex.RowIndex);
            else
                this.CurrentUnboundRowInfo = null;

            //While changing the NavigationMode to Row the CurrentColumn is maintained.
            if (this.dataGrid.NavigationMode == NavigationMode.Cell || this.dataGrid.CurrentColumn != null)
                this.dataGrid.CurrentColumn = this.dataGrid.GetGridColumn(rowColumnIndex.ColumnIndex);

            if (this.dataGrid.SelectionUnit != GridSelectionUnit.Row)
                this.UpdateCurrentCellInfo(rowColumnIndex);

            if (rowColumnIndex.RowIndex < 0 || this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
            {
                needToMove = false;
            }
            else if (this.dataGrid.GridModel.HasGroup)
            {
                var record = this.dataGrid.View.TopLevelGroup.DisplayElements[this.dataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex)];
                if (!(record is RecordEntry || record is NestedRecordEntry))
                    needToMove = false;
            }

            this.SuspendUpdates();

            if (this.dataGrid.View != null)
            {
                if (needToMove)
                    this.dataGrid.View.MoveCurrentToPosition(this.dataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex));
                else
                    this.dataGrid.View.MoveCurrentToPosition(-1);
            }
            else
                this.dataGrid.CurrentItem = null;

            this.dataGrid.UpdateRowHeaderState();
            this.ResumeUpdates();
        }

        internal void ResetCurrentRowColumnIndex()
        {
            UpdateGridProperties(new RowColumnIndex(-1, -1));
            this.dataGrid.CurrentItem = null;
            this.previousRowColumnIndex = new RowColumnIndex(-1, -1);
        }

        internal void UpdatePreviousIndex()
        {
            previousRowColumnIndex = currentRowColumnIndex;
        }

        internal void SetCurrentRowIndex(int rowIndex)
        {
            UpdatePreviousIndex();
            currentRowColumnIndex.RowIndex = rowIndex;
        }

        internal void SetCurrentColumnIndex(int columnIndex)
        {
            UpdatePreviousIndex();
            currentRowColumnIndex.ColumnIndex = columnIndex;

            if (!this.HasCurrentCell)
                return;

            if (this.CurrentCell.Renderer != null && (this.CurrentCell.Renderer as GridCellRendererBase).CurrentCellIndex.ColumnIndex != columnIndex)
                this.CurrentCell.Renderer.SetCurrentCellState(CurrentRowColumnIndex, this.CurrentCell.ColumnElement, this.CurrentCell.IsEditing, false, this.CurrentCell.GridColumn, this.CurrentCell);
        }

        internal void SetCurrentColumnBase(DataColumnBase column, bool setFocus)
        {
            CurrentCell = column;
            if (CurrentCell.Renderer != null && !CurrentCell.Renderer.HasCurrentCellState && (!CurrentCell.IsEditing || this.IsFilterRow))
            {
                CurrentCell.Renderer.SetCurrentCellState(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex, CurrentCell.ColumnElement, CurrentCell.IsEditing, setFocusForGrid, CurrentCell.GridColumn, column);
                //Fix for WPF-16724 - OldCellValue was not mainbtained since its called from EnsureColumns for the Column not inview.
                SetOldCellValue(column, CurrentRowColumnIndex);
            }

            if ((this.IsAddNewRow && dataGrid.View.IsAddingNew) && lastPressedKey == Key.Tab)
                this.BeginEdit();
            setFocusForGrid = false;
        }
        /// <summary>
        /// Sets the current rowcolumnindex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The rowcolumnindex.
        /// </param>
        internal protected void SetCurrentRowColumnIndex(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex != currentRowColumnIndex)
                UpdatePreviousIndex();

            CurrentRowColumnIndex = rowColumnIndex;
        }

        #region CommitValue
        internal void CommitCellValue(bool isNewValue)
        {
            var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == this.CurrentRowColumnIndex.RowIndex);
            if (dataRow == null || !dataRow.IsEditing && !dataGrid.HasView) return;
            if (dataRow.RowType == RowType.UnBoundRow || dataRow.RowType == RowType.FilterRow) return;

            var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.ColumnIndex == this.CurrentRowColumnIndex.ColumnIndex);
            if (dataColumn == null || !dataColumn.IsEditing) return;

            if (dataColumn.GridColumn.IsTemplate)
                return;

            object cellvalue;
            if (isNewValue)
                cellvalue = dataColumn.Renderer.GetControlValue();
            else
                cellvalue = oldCellValue;

            var propertyAccessProvider = dataGrid.View.GetPropertyAccessProvider();
            var mappingName = dataColumn.GridColumn.MappingName;
            //UWP - 2558 if mapping name is differed from value binding property path then committed value must be in value binding path property.
            //if (!dataColumn.GridColumn.isvaluebindingcreated && dataColumn.GridColumn.ValueBinding != null && !dataColumn.GridColumn.UseBindingValue)
            //    mappingName = (dataColumn.GridColumn.ValueBinding as Binding).Path.Path;

            if (!dataColumn.GridColumn.isvaluebindingcreated || dataColumn.GridColumn.UseBindingValue)
            {
                if (dataColumn.GridColumn.ValueBinding != null)
                    cellvalue = propertyAccessProvider.GetConvertBackValue(dataColumn.GridColumn.ValueBinding as Binding, cellvalue);
                propertyAccessProvider.SetValue(dataRow.RowData, dataColumn.GridColumn.MappingName, cellvalue);
                return;
            }

            var propertyinfo = dataGrid.View.GetItemProperties().Find(mappingName, false);
            if (propertyinfo == null)
            {
                //If we have use the GetPropertyInfo extension for getting the property, its return the particular 
                //Column propertyinfo for all types of binding like complexproperty, indexerbindig.
                PropertyDescriptorCollection pdc = this.dataGrid.View.GetItemProperties();
                propertyinfo = pdc.GetPropertyDescriptor(mappingName);
            }

            if (propertyinfo != null)
            {
                var value = Syncfusion.Data.Helper.ValueConvert.ChangeType(cellvalue, propertyinfo.PropertyType, null);
                if (value != null || (NullableHelperInternal.IsNullableType(propertyinfo.PropertyType) && value == null))
                    propertyAccessProvider.SetValue(dataRow.RowData, mappingName, value);
                return;
            }

            if (dataGrid.View.IsDynamicBound)
            {
                var type = (cellvalue == null) ? null : cellvalue.GetType();
                var convertedValue = Syncfusion.Data.Helper.ValueConvert.ChangeType(cellvalue, type, null);
                if (convertedValue != null || (NullableHelperInternal.IsNullableType(type) && convertedValue == null))
                    propertyAccessProvider.SetValue(dataRow.RowData, mappingName, convertedValue);
            }
            else
                propertyAccessProvider.SetValue(dataRow.RowData, mappingName, cellvalue, true);
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="canCommit"></param>
        /// <returns></returns>
        internal protected bool RaiseValidationAndEndEdit(bool canCommit = false)
        {
            //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
            if (this.HasCurrentCell && (this.CurrentCell.IsEditing || this.CurrentCell.Renderer is GridCellCheckBoxRenderer))
            {
                if (!(this.CurrentCell.Renderer is GridCellCheckBoxRenderer) && !dataGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex))
                    return false;

                EndEdit(canCommit);
            }
            return true;
        }

        /// <summary>
        /// Determines whether the validation is applied to the current cell.
        /// </summary>
        /// <param name="cancommit">Indicates whether cell value should be commit in record.</param>
        /// <returns>
        /// Returns <b>true</b> if the current cell validation is successful and the cell is allowed for end edit operation;
        /// otherwise, <b>false</b>.
        /// </returns>
        internal bool CheckValidationAndEndEdit(bool cancommit)
        {
            if (!dataGrid.ValidationHelper.CheckForValidation(true))
                return false;
            //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
            if (this.HasCurrentCell && (this.CurrentCell.IsEditing || this.CurrentCell.Renderer is GridCellCheckBoxRenderer
                || (this.dataGrid.HasView && this.dataGrid.View.IsEditingItem)))
                EndEdit();
            return true;
        }

        /// <summary>
        /// Determines whether the validation is applied to the current cell.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the current cell validation is successful and the cell is allowed for end edit operation;
        /// otherwise, <b>false</b>.
        /// </returns>       
        public bool CheckValidationAndEndEdit()
        {
            return this.CheckValidationAndEndEdit(false);
        }

        /// <summary>
        /// Gets the column for the specified <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/>.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index to get the column.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> for the specified row and column index; otherwise , return null.
        /// </returns>
        protected GridColumn GetGridColumn(RowColumnIndex rowColumnIndex)
        {
            var columnIndex = this.dataGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex);
            var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow != null)
            {
                var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
                if (dataColumn.GridColumn != null)
                    return dataColumn.GridColumn;
            }
            if (columnIndex >= 0)
                return this.dataGrid.Columns[columnIndex];
            return null;
        }
        
        internal bool CanMoveCurrentCell(RowColumnIndex rowColumnIndex)
        {
            if (!AllowFocus(rowColumnIndex) || dataGrid.NavigationMode == NavigationMode.Row || CurrentRowColumnIndex == rowColumnIndex)
                return false;

            //The Below condition is used to cancel the MoveCurrentCell process when the value RowIndex value is Greater than the LastRowIndex value or the ColumnIndex value is Greater than the LastCell Index value.
            if ((rowColumnIndex.RowIndex > -1 && rowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex())
                || (rowColumnIndex.ColumnIndex > -1 && rowColumnIndex.ColumnIndex < this.dataGrid.GetFirstColumnIndex())
                || rowColumnIndex.ColumnIndex > this.dataGrid.GetLastColumnIndex() || rowColumnIndex.RowIndex > this.dataGrid.GetLastNavigatingRowIndex())
                throw new InvalidOperationException("InValid RowColumnIndex");

            if (!dataGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex) || CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex && !dataGrid.ValidationHelper.RaiseRowValidate(CurrentRowColumnIndex))
                return false;

            //WPF-25715- Skip the CurrentCell when the CurrentCell in same SummaryRow.
            if (CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex && this.dataGrid.GridModel.HasGroup)
            {
                if (this.dataGrid.RowGenerator.Items.Any(item => item.RowIndex == CurrentRowColumnIndex.RowIndex))
                {
                    var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentRowColumnIndex.RowIndex);
                    if (dataRow is SpannedDataRow)
                        return false;
                }
                else
                {
                    var newObj = this.dataGrid.View.TopLevelGroup.DisplayElements[this.dataGrid.ResolveToRecordIndex(CurrentRowColumnIndex.RowIndex)];
                    if (newObj is SummaryRecordEntry || newObj is Group)
                        return false;
                }
            }
            EndEdit();

            return true;
        }

        internal bool IsInNonDataRows()
        {
            return this.IsAddNewRow || this.IsFilterRow || this.IsUnBoundRow;
        }

        /// <summary>
        /// Method which helps to decide whether we can select the current row or not. 
        /// This method used when we need to select the focused row.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        internal bool CanSelectRow(int rowIndex)
        {
            if (CurrentRowColumnIndex.RowIndex == rowIndex)
            {
                var row = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                if (row != null)
                    return !row.IsSelectedRow;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the selection can be processed for row or cell corresponding to the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index to perform row or cell selection.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the row or cell selection can be processed; otherwise, <b>false</b>.
        /// </returns>
        protected bool CanSelectRowOrCell(RowColumnIndex rowColumnIndex)
        {
            if (this.dataGrid.SelectionUnit == GridSelectionUnit.Row)
            {
                //Selection is not cleared in parent grid when setting the CurrentItem in DetailsViewGrid directly. Hence added to code to check whether the selection is added to any NestedGrid using rowIndex.
                return (dataGrid.SelectionMode == GridSelectionMode.Extended && (!this.dataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex) && (SelectionHelper.CheckControlKeyPressed() || (this.dataGrid.SelectionController.SelectedRows.Count > 1 && !SelectionHelper.CheckShiftKeyPressed()))) || (this.dataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex) && !this.dataGrid.SelectionController.SelectedRows.Any(item => item.RowIndex == rowColumnIndex.RowIndex))) || CanSelectRow(rowColumnIndex.RowIndex);
            }
            else
            {
                var selectionController = this.dataGrid.SelectionController as GridCellSelectionController;
                //Selection is not cleared in parent grid when setting the CurrentItem in DetailsViewGrid directly. Hence added to code to check whether the selection is added to any NestedGrid using rowIndex.
                return (dataGrid.SelectionMode == GridSelectionMode.Extended && (!this.dataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex) && (SelectionHelper.CheckControlKeyPressed() || (selectionController.SelectedCells.Count > 1 && !SelectionHelper.CheckShiftKeyPressed()))) || (this.dataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex) && !selectionController.SelectedCells.Any(item => item.RowIndex == rowColumnIndex.RowIndex)));
            }
        }

        #endregion

        #region Public Methods

        #region Virtual Methods
        /// <summary>
        /// Handles the current cell selection when any of <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> performed in cell.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the mouse action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the cell.
        /// </param>        
        /// <returns>
        /// Returns <b>true</b> if the pointer operation should be handled on the current cell; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool HandlePointerOperation(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            //WPF-19590 Throws exception while processing selection in invalid row index. Hence the process is returned here itself
            //without doing futher operation in SelectionController. This issue reported in incident I137265, customer doesnt share the
            //exact senario to reproduce the issue.
            //CurrentCell is updated when clicking on RowHeader when there is no columns in view, hence the below GetFirstColumnIndex method condition is added.
            if (rowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex() || rowColumnIndex.RowIndex > this.dataGrid.GetLastNavigatingRowIndex()
                || this.dataGrid.GetFirstColumnIndex() < 0)
                return false;

            if (CurrentRowColumnIndex != rowColumnIndex && AllowFocus(rowColumnIndex) && (dataGrid.NavigationMode != NavigationMode.Row
                || dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || dataGrid.IsAddNewIndex(currentRowColumnIndex.RowIndex)
                || dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex) || dataGrid.IsFilterRowIndex(currentRowColumnIndex.RowIndex)))
            {
                if (!dataGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex) || CurrentRowColumnIndex.RowIndex > -1 && CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex && !dataGrid.ValidationHelper.RaiseRowValidate(CurrentRowColumnIndex))
                    return false;

                if (this.HasCurrentCell && (this.CurrentCell.IsEditing
                    || (this.dataGrid.HasView && (this.dataGrid.View.IsEditingItem || this.dataGrid.View.IsAddingNew))))
                {
                    NodeEntry nodeEntry = null;
                    if (this.dataGrid.HasView && this.dataGrid.View.IsEditingItem && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping))
                        nodeEntry = this.dataGrid.GetNodeEntry(rowColumnIndex.RowIndex);

                    EndEdit(CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex || this.dataGrid.SelectionUnit != GridSelectionUnit.Row);

                    //The Below condition is Added to Check the AddNewRow is in Edit or Not Because if it is getting Committed the RowColumnIndex will be Changed.
                    //So we can process the CurrentRowColumnIndex to select the row or cell.
                    if (this.IsAddNewRow && CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex)
                    {
                        if (dataGrid.HasView && dataGrid.View.IsAddingNew)
                            (this.dataGrid.SelectionController as GridBaseSelectionController).CommitAddNew();
                    }

                    if (nodeEntry != null)
                    {
                        //The Below condition is used to check the Data Row is Removed or Not in the Grouping Cases when Edinting the Data.
                        var rowIndex = this.dataGrid.ResolveToRowIndex(nodeEntry);
                        //When clicking the same group SummaryRow the rowIndex will be -1 hence the below condition is added by checking 0.
                        if (rowIndex > 0 && rowColumnIndex.RowIndex != rowIndex)
                            rowColumnIndex.RowIndex = rowIndex;
                    }
                }

                //WPF-31152-When we clicking RowHeader it is not selecting the row while the SelectionUnit is Any
                if (CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex && rowColumnIndex.ColumnIndex == 0 && dataGrid.ShowRowHeader && dataGrid.SelectionUnit == GridSelectionUnit.Any)
                    return true;

                //The column index will be less than first column index when clicking on Caption row which is resolved in this code.
                if (rowColumnIndex.ColumnIndex < this.dataGrid.GetFirstColumnIndex() && !this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                {
                    rowColumnIndex.ColumnIndex = this.dataGrid.ResolveColumnIndex(rowColumnIndex);
                }

                //When selection mode is multiple we have to perform selection, hence the MultipleSelection mode condition is added.
                bool cancelSelection = dataGrid.SelectionUnit == GridSelectionUnit.Row ?
                                        (CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex &&
                                        (this.dataGrid.SelectionMode == GridSelectionMode.Single ||
                                        (!CanSelectRowOrCell(rowColumnIndex) && dataGrid.SelectionMode != GridSelectionMode.Multiple)))
                                            : (rowColumnIndex == this.CurrentRowColumnIndex && this.dataGrid.SelectionMode != GridSelectionMode.Multiple);

#if WinRT
                var activationTrigger = ActivationTrigger.Mouse;
                if (args != null)
                    activationTrigger = SelectionHelper.ConvertPointerDeviceTypeToActivationTrigger(args.Pointer.PointerDeviceType);
                if (CurrentRowColumnIndex != rowColumnIndex && (dataGrid.NavigationMode != NavigationMode.Row || dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex)) && !ProcessCurrentCellSelection(rowColumnIndex, activationTrigger))
#else
                if (CurrentRowColumnIndex != rowColumnIndex && (dataGrid.NavigationMode != NavigationMode.Row || dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex)) && !ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Mouse))
#endif
                    return false;
                //WRT-3657 when CurrentRow is AddNewRow EndEdit Didn't call and validation not be applied
                //RemoveCurrentCell method for remove the previous columnindex in CurrentRow
                //trying to edit a add new row in navigation mode as row so RowColumnIndex need to be update  
                else if (dataGrid.NavigationMode == NavigationMode.Row && currentRowColumnIndex != rowColumnIndex && (dataGrid.IsAddNewIndex(currentRowColumnIndex.RowIndex) || dataGrid.IsFilterRowIndex(currentRowColumnIndex.RowIndex)))
                {
                    this.RemoveCurrentCell(CurrentRowColumnIndex);
                    this.UpdateGridProperties(rowColumnIndex);
                }
                return !cancelSelection;
            }
            else if (dataGrid.NavigationMode == NavigationMode.Row)
            {
                if (rowColumnIndex.ColumnIndex < this.GetFirstCellIndex())
                    rowColumnIndex.ColumnIndex = this.GetFirstCellIndex();

                UpdateGridProperties(rowColumnIndex);
                return true;
            }

            //Clicking inside the edit element should not affect the selection
            if (HasCurrentCell && this.CurrentCell.IsEditing && dataGrid.SelectionMode == GridSelectionMode.Multiple && CurrentRowColumnIndex == rowColumnIndex)
            {
                return false;
            }

            if (CurrentRowColumnIndex == rowColumnIndex && (CanSelectRowOrCell(rowColumnIndex) || dataGrid.SelectionMode == GridSelectionMode.Multiple))
                return true;

            return false;
        }

        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed current cell.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool HandleKeyDown(KeyEventArgs args)
        {
            if (HasCurrentCell)
            {
                if (CurrentCell.Renderer != null && !CurrentCell.Renderer.ShouldGridTryToHandleKeyDown(args))
                {
#if WinRT || UNIVERSAL
                    if (char.IsLetterOrDigit(args.Key.ToString(), 0) && this.dataGrid.AllowEditing)
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                            return false;
                        if (!(args.Key >= Key.A && args.Key <= Key.Z) && !(args.Key >= Key.Number0 && args.Key <= Key.Number9) && !(args.Key >= Key.NumberPad0 && args.Key <= Key.NumberPad9))
                        {
                            if (args.Key == Key.Up || args.Key == Key.Down || args.Key == Key.Home || args.Key == Key.End || args.Key == Key.Left || args.Key == Key.Right)
                                args.Handled = true;  // Fix for Scolling happens when press down or up key in Editted cell.
                            return false;
                        }
                        if (CurrentCell.Renderer != null && CurrentCell.GridColumn != null && !(CurrentCell.GridColumn.IsTemplate))
                        {
                            if (!CurrentCell.IsEditing)
                            {
                                if (BeginEdit())
                                    CurrentCell.Renderer.PreviewTextInput(args);
                            }
                        }
                    }
#endif
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Handles the selection when the key navigation is processed on the current cell.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index where the key navigation occurs.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the navigation processed; otherwise , <b>false</b>.
        /// </returns>
        /// <remarks>
        /// Override this method , to customize navigation behavior of current cell in SfDataGrid.
        /// </remarks>
        public virtual bool HandleKeyNavigation(KeyEventArgs args, RowColumnIndex rowColumnIndex)
        {
            Key currentKey = args.Key;
            if (dataGrid.FlowDirection == FlowDirection.RightToLeft)
                (this.dataGrid.SelectionController as GridBaseSelectionController).ChangeFlowDirectionKey(ref currentKey);

            int rowIndex, columnIndex;
            switch (currentKey)
            {
                case Key.Home:
                case Key.End:
                case Key.Enter:
                case Key.Up:
                case Key.Down:
                case Key.PageDown:
                case Key.PageUp:
                    {
                        if (rowColumnIndex != this.CurrentRowColumnIndex)
                        {
                            if (this.dataGrid.NavigationMode == NavigationMode.Cell || dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
                            {
                                //Added the condition to check the column index because when the column index is -1 the AllowFocus method,
                                //will return false. So that the ProcessCurrenCellSelection method will not invoked. We have resolved the columnIndex,
                                //within that method.
                                if ((rowColumnIndex.ColumnIndex >= this.dataGrid.GetFirstColumnIndex() && !this.dataGrid.AllowFocus(rowColumnIndex)) || !ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                                    return false;

                                return true;
                            }

                            if (dataGrid.NavigationMode == NavigationMode.Row)
                            {
                                if (this.IsAddNewRow || this.IsFilterRow)
                                    this.RemoveCurrentCell(this.CurrentRowColumnIndex);
                                this.UpdateGridProperties(rowColumnIndex);
                                return true;
                            }
                        }
                        return false;
                    }
                case Key.F2:
                    {
                        if (CurrentCell != null && CurrentCell.IsEditing)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() || !RaiseValidationAndEndEdit())
                                return false;
                        }
                        else if (CurrentCell != null)
                        {
                            BeginEdit();
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Right:
                    {
                        if (!RaiseValidationAndEndEdit())
                            return false;
                        rowIndex = CurrentRowColumnIndex.RowIndex;
                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetLastColumnIndex() : this.GetLastCellIndex();
                        else
                            columnIndex = this.GetNextCellIndex();

                        if (CurrentRowColumnIndex.ColumnIndex == columnIndex)
                            return false;

                        if (columnIndex != -1)
                        {
                            rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                            if (ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                            {
                                this.ScrollInViewFromRight(columnIndex);
                            }
                            else
                            {
                                return false;
                            }
                        }
                        lastPressedKey = Key.Right;
                        args.Handled = true;
                    }
                    break;
                case Key.Left:
                    {
                        if (!RaiseValidationAndEndEdit())
                            return false;
                        rowIndex = CurrentRowColumnIndex.RowIndex;

                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetFirstColumnIndex() : this.GetFirstCellIndex();
                        else
                            columnIndex = this.GetPreviousCellIndex();

                        if (CurrentRowColumnIndex.ColumnIndex == columnIndex)
                        {
                            if (columnIndex == this.GetFirstCellIndex())
                                this.ScrollInViewFromLeft(this.dataGrid.ShowRowHeader ? 1 : 0);
                            return false;
                        }

                        if (columnIndex != -1)
                        {
                            rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                            if (ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                            {
                                this.ScrollInViewFromLeft(columnIndex);
                            }
                            else
                                return false;
                        }
                        args.Handled = true;
                        lastPressedKey = Key.Right;
                    }
                    break;
                case Key.Escape:
                    if (dataGrid.NavigationMode == NavigationMode.Cell || dataGrid.IsAddNewIndex(CurrentRowColumnIndex.RowIndex))
                    {
                        if (ValidationHelper.IsCurrentCellValidated && lastPressedKey == Key.Escape)
                            dataGrid.ValidationHelper.ResetValidations(true);
                        else
                            dataGrid.ValidationHelper.ResetValidations(false);
                        if (HasCurrentCell && CurrentCell.IsEditing)
                        {
                            CommitCellValue(false);
                            //Validate current cell and update validation messages when press Esc key
                            UpdateValidationMessagesForCurrentCell();
                            EndEdit(false);
                            args.Handled = true;
                        }
                        else
                        {
                            if (this.dataGrid.HasView && this.dataGrid.View.IsEditingItem)
                            {
                                dataGrid.View.CancelEdit();
                                dataGrid.ValidationHelper.SetCurrentCellValidated(true);
                                dataGrid.ValidationHelper.SetCurrentRowValidated(true);
#if WPF
                                if (dataGrid.View.IsLegacyDataTable)
                                {
                                    var currentRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                                    if (currentRow != null)
                                    {
                                        foreach (var col in currentRow.VisibleColumns)
                                            col.UpdateBinding(currentRow.RowData);
                                    }
                                }
#endif
                                if (dataGrid.IsAddNewIndex(CurrentRowColumnIndex.RowIndex) && dataGrid.View.IsAddingNew)
                                    dataGrid.GridModel.AddNewRowController.CancelAddNew();
                                args.Handled = true;
                            }
                        }
                        lastPressedKey = Key.Escape;                     
                    }
                    break;
                case Key.Tab:
                    {
                        //In GridCheckBox column, IsEditing flag is always false hence the rendere has been checked for next cell editing.
                        //If the IsEditing flag is false, After the CheckBox column the CurrentCell gets the Editing State. So the DataGrid.View.IsEditingitem condition is added to check the Editing.
                        var previousCellEditStatus = CurrentCell != null && dataGrid.SelectionUnit == GridSelectionUnit.Row ? (CurrentCell.IsEditing || (CurrentCell.Renderer is GridCellCheckBoxRenderer && this.dataGrid.View.IsEditingItem)) : false;
                        if (!RaiseValidationAndEndEdit(this.dataGrid.SelectionUnit != GridSelectionUnit.Row))
                        {
                            args.Handled = true;
                            return false;
                        }

                        rowIndex = CurrentRowColumnIndex.RowIndex;
                        var getPrevious = (SelectionHelper.CheckShiftKeyPressed() && dataGrid.FlowDirection == FlowDirection.LeftToRight) || (!SelectionHelper.CheckShiftKeyPressed() && dataGrid.FlowDirection == FlowDirection.RightToLeft);
                        lastPressedKey = Key.Tab;
                        var row = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentRowColumnIndex.RowIndex);
                        if (getPrevious)
                        {
                            columnIndex = GetPreviousCellIndex();
                            //When the first column is set as AllowFocus false and while pressing shift + tab in AddNewRow(bottom) the new value
                            //not been committed. Hence the new GetFirstCellIndex method is added.
                            if ((columnIndex == GetFirstCellIndex(CurrentRowColumnIndex.RowIndex)
                                && CurrentRowColumnIndex.ColumnIndex == columnIndex)
                                || (row != null && row.RowType != RowType.AddNewRow && row.RowType != RowType.DetailsViewRow && row.RowType != RowType.DefaultRow
                                && row.RowType != RowType.FilterRow && row.RowType != RowType.UnBoundRow)
                                || (dataGrid is DetailsViewDataGrid && rowIndex < 0))
                                return false;
                        }
                        else
                        {
                            columnIndex = GetNextCellIndex();
                            //When the last column is set as AllowFocus false and while pressing tab in AddNewRow(bottom) the new value
                            //not been committed. Hence the new GetLastCellIndex method is added.
                            if ((columnIndex == GetLastCellIndex(CurrentRowColumnIndex.RowIndex)
                                && CurrentRowColumnIndex.ColumnIndex == columnIndex)
                                || (row != null && row.RowType != RowType.DefaultRow && row.RowType != RowType.DetailsViewRow && row.RowType != RowType.AddNewRow
                                && row.RowType != RowType.FilterRow && row.RowType != RowType.UnBoundRow)
                                || (dataGrid is DetailsViewDataGrid && rowIndex < 0))
                            {
                                return false;
                            }
                        }
                        rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                        if (!ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                        {
                            args.Handled = true;
                            return false;
                        }
                        if (previousCellEditStatus && this.IsAddNewRow)
                            BeginEdit();
                        if (getPrevious)
                        {
                            if (columnIndex == this.GetFirstCellIndex())
                                this.ScrollInViewFromLeft(this.dataGrid.ShowRowHeader ? 1 : 0);
                            this.ScrollInViewFromLeft(columnIndex);
                        }
                        else
                            this.ScrollInViewFromRight(columnIndex);

                        //ScrollInView(rowColumnIndex);
                        args.Handled = true;
                    }
                    break;
            }
            return args.Handled;
        }

        /// <summary>
        /// Validate current cell and update validation messages when press Esc key
        /// </summary>
        private void UpdateValidationMessagesForCurrentCell()
        {
            var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentRowColumnIndex.RowIndex && currentRowColumnIndex.RowIndex != -1);
            if (dataRow == null)
                return;
            if (CurrentCell == null)
                return;
            if (CurrentCell.GridColumn.GridValidationMode != GridValidationMode.None)
            {
                this.dataGrid.ValidationHelper.ValidateColumn(dataRow.RowData, CurrentCell.GridColumn.MappingName, (CurrentCell.ColumnElement as GridCell), currentRowColumnIndex);
            }
        }
        /// <summary>
        /// Processes the selection when the mouse point is tapped on the current cell. 
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>    
        /// <remarks>
        /// This method invoked to process selection and end edit the current cell when <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnTap"/>.
        /// </remarks>
        public virtual void ProcessOnTapped(TappedEventArgs e, RowColumnIndex currentRowColumnIndex)
        {
            if ((dataGrid.EditTrigger == EditTrigger.OnTap || this.IsFilterRow) && this.AllowFocus(currentRowColumnIndex))
            {
                if (this.HasCurrentCell && dataGrid.HasView && dataGrid.View.IsEditingItem)
                {
                    if (CurrentRowColumnIndex.RowIndex != currentRowColumnIndex.RowIndex)
                        CheckValidationAndEndEdit();
                    else
                        RaiseValidationAndEndEdit();
                }
                if (this.HasCurrentCell && !this.CurrentCell.IsEditing)
                    this.BeginEdit();
            }
            // Fix for WPF-16876 - Need to scroll when the tap on seleceted row was hit.
            //Checked the Condition while scroll to the selected row when Navigation with the UP/Down key.
            //WPF-20218 - ScrollInView method seperately called for DetailsViewGrid. Because if we have a selection in DetailView
            //and we have scroll means detailsview only scrolled and currentcell is clipped, so update the DetailsViewGrid and ParentGrid ScrollBar value.          
            if (CurrentRowColumnIndex == currentRowColumnIndex)
            {
                if (!(this.dataGrid is DetailsViewDataGrid))
                    ScrollInView(currentRowColumnIndex);
                else
                    (this.dataGrid.SelectionController as GridBaseSelectionController).DetailsViewScrollinView(currentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the selection when the mouse point is double tapped on the current cell.
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the double tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>  
        /// <remarks>
        /// This method invoked to process selection and end edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>.
        /// </remarks>
        public virtual void ProcessOnDoubleTapped(DoubleTappedEventArgs e)
        {
            if (CurrentCell != null && CurrentCell.Renderer != null)
            {
                if (CurrentCell.Renderer.HasCurrentCellState && !CurrentCell.IsEditing)
                {
                    this.BeginEdit();
#if WPF
                    //WPF-36696 We have handled the DoubleTapped event, when currentcell is in editmode to avoid validation.
                    e.Handled = true;
#endif
                }
            }
        }

        /// <summary>
        /// Handles the current cell selection when the columns is added or removed at run time.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the collection changed action in columns collection.
        /// </param>
        public virtual void HandleColumnsCollectionChanged(NotifyCollectionChangedEventArgs args)
        {
            if (CurrentCell != null)
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        ProcessColumnRemoveAndInsert(args.NewItems.Count > 0 && args.NewItems[0] != null ? (GridColumn)args.NewItems[0] : null, dataGrid.ResolveToScrollColumnIndex(args.NewStartingIndex), NotifyCollectionChangedAction.Add);
                        break;
                    case NotifyCollectionChangedAction.Remove:
                        ProcessColumnRemoveAndInsert(args.OldItems.Count > 0 && args.OldItems[0] != null ? (GridColumn)args.OldItems[0] : null, dataGrid.ResolveToScrollColumnIndex(args.OldStartingIndex), NotifyCollectionChangedAction.Remove);
                        break;
                    case NotifyCollectionChangedAction.Replace:
                        ProcessColumnRemoveAndInsert(args.OldItems.Count > 0 ? (GridColumn)args.OldItems[0] : null, dataGrid.ResolveToScrollColumnIndex(args.OldStartingIndex), NotifyCollectionChangedAction.Replace);
                        break;
                }
            }

            if (args.Action == NotifyCollectionChangedAction.Move)
            {
#if WinRT || UNIVERSAL
                bool needToMove = this.CurrentCell == null && this.CurrentRowColumnIndex.RowIndex != -1 && this.CurrentRowColumnIndex.ColumnIndex != -1;
#else
                bool needToMove = this.CurrentCell == null && this.CurrentRowColumnIndex.RowIndex != -1 && this.CurrentRowColumnIndex.ColumnIndex != -1 && SfDataGrid.suspendForColumnMove;
#endif
                if (needToMove || this.CurrentCell != null)
                {
                    this.ProcessColumnPositionChanged(args, needToMove);
                }
            }
        }

        #endregion

        #region Editing Methods

        /// <summary>
        /// Initiates the edit operation on the current cell.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the current cell entering into edit mode; otherwise, <b>false</b> .
        /// </returns>       
        public bool BeginEdit()
        {
            if (!HasCurrentCell) return false;
            var currentCell = CurrentCell.ColumnElement as GridCell;

            if (currentCell == null || !currentCell.IsEditable)
                return false;

            // If UnBoundRowCell does n't have a edit template , we should not allow the cell to load edit element.
            if (CurrentCell.isUnBoundRowCell && CurrentCell.GridUnBoundRowEventsArgs!=null && CurrentCell.GridUnBoundRowEventsArgs.CellType.Equals("UnBoundTemplateColumn") && !CurrentCell.GridUnBoundRowEventsArgs.hasEditTemplate)
                return false;

            //WPF-28122 In some columns, we have to edit the cell in FilterRow, hence the new isFilterRow has been introduced.
            if (CurrentCell.GridColumn == null || !CurrentCell.GridColumn.CanEditCell(this.currentRowColumnIndex.RowIndex))
                return false;

            if (CurrentCell.GridColumn != null && (CurrentCell.GridColumn.AllowEditing || this.IsAddNewRow || this.IsFilterRow))
            {
                if (CurrentCell.Renderer != null && CurrentCell.Renderer.IsEditable)
                {
                    if (dataGrid.IsAddNewIndex(CurrentRowColumnIndex.RowIndex) && !dataGrid.View.IsAddingNew)
                    {
                        if (!dataGrid.GridModel.AddNewRowController.AddNew())
                            return false;
                    }
                    var hasMergedCell = this.dataGrid.CoveredCells.GetCoveredCellInfo(this.CurrentCell.RowIndex, this.CurrentCell.ColumnIndex) != null;
                    var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(row => hasMergedCell ? row.RowIndex == CurrentRowColumnIndex.RowIndex : row.RowIndex == CurrentCell.RowIndex);
                    if (dataRow == null)
                        return false;

                    if (ValidationHelper.IsCurrentCellValidated && (!this.dataGrid.MergedCellManager.CanRasieEvent || !RaiseCurrentCellBeginEditEvent(new RowColumnIndex(CurrentCell.RowIndex, CurrentCell.ColumnIndex), CurrentCell.GridColumn)))
                    {
                        var isinedit = CurrentCell.Renderer.BeginEdit(new RowColumnIndex(CurrentCell.RowIndex, CurrentCell.ColumnIndex), currentCell, CurrentCell.GridColumn, currentCell.DataContext);
                        if (isinedit)
                        {
                            CurrentCell.IsEditing = isinedit;
                            dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == CurrentRowColumnIndex.RowIndex);
                            if (dataRow != null)
                            {
                                if (dataRow.RowType != RowType.UnBoundRow && dataRow.RowType != RowType.FilterRow && (!dataGrid.View.IsEditingItem || !dataGrid.View.CurrentEditItem.Equals(dataRow.RowData)))
                                    dataGrid.View.EditItem(dataRow.RowData);
                                dataRow.IsEditing = isinedit;
                                if (dataRow.RowType != RowType.UnBoundRow && dataRow.RowType != RowType.FilterRow)
                                {
                                    if (CurrentCell.Renderer.CanValidate())
                                    {
                                        dataGrid.ValidationHelper.SetCurrentCellValidated(!isinedit);
                                    }
                                    dataGrid.ValidationHelper.SetCurrentRowValidated(!isinedit);
                                }
                                (dataRow as GridDataRow).ApplyRowHeaderVisualState();
                            }
                        }
                        return isinedit;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Ends the edit operation on the current cell.
        /// </summary>
        /// <param name="canCommit">
        /// Specifies whether the value can be committed to the current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the edit operation is ended; otherwise, <b>false</b>.
        /// </returns>
        public bool EndEdit(bool canCommit = true)
        {
            var datarow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsEditing);

            if (!HasCurrentCell) return true;
            //WPF-31496 Skip to call the RaiseValidationAndEndEdit for CheckBox renderer for recursive calling while
            //pressing tab key in rowvalidation.
            if (!ValidationHelper.IsCurrentCellValidated && this.CurrentCell.IsEditing && !dataGrid.ValidationHelper.RaiseCellValidate(CurrentRowColumnIndex))
                return false;
            var currentCell = CurrentCell.ColumnElement as GridCell;
            var currentCellRenderer = CurrentCell.Renderer;
            if (currentCell == null || !currentCell.IsEditable)
                return true;
            if (currentCellRenderer != null && currentCellRenderer.IsEditable)
            {
                if (CurrentCell.IsEditing)
                {
                    var currentDataColumn = this.CurrentCell;
                    var isinedit = currentCellRenderer.EndEdit(this.CurrentCell, currentCell.DataContext);
                    if (isinedit)
                    {
                        if (datarow != null)
                            datarow.IsEditing = !isinedit;
                        if (currentDataColumn != null)
                            currentDataColumn.IsEditing = !isinedit;

                        if (this.dataGrid.HasView && !(datarow is UnBoundRow))
                        {
                            if (canCommit && dataGrid.View.IsEditingItem)
                                dataGrid.View.CommitEdit();
                            else
                            {
                                // WPF-25805 Upate the Unbound column's binding when edit and change its associated expression column's value of AddNewRow.For other rows binding will be updated through OnRecordCollectionChanged event.
                                if (this.dataGrid.HasUnboundColumns && this.dataGrid.View.IsAddingNew)
                                    this.dataGrid.UpdateUnboundColumn(datarow, currentDataColumn.GridColumn.MappingName);
                            }
                        }

                        if (this.dataGrid.MergedCellManager.CanRasieEvent)
                            RaiseCurrentCellEndEditEvent(currentDataColumn != null ? new RowColumnIndex(currentDataColumn.RowIndex, currentDataColumn.ColumnIndex) : CurrentRowColumnIndex);
                        if (datarow != null)
                            (datarow as GridDataRow).ApplyRowHeaderVisualState();
#if WPF
                        if (dataGrid.useDrawing && datarow != null)
                            datarow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                    }
                }
                else if (dataGrid.HasView && dataGrid.View.IsEditingItem)
                {
                    if (canCommit && !(datarow is UnBoundRow))
                        dataGrid.View.CommitEdit();
                }

#if WPF
                if (!string.IsNullOrEmpty(this.dataGrid.SearchHelper.SearchText) && this.dataGrid.SearchHelper.CanHighlightSearchText)
                    this.dataGrid.SearchHelper.SearchRow(datarow);
#endif

            }
            else if (dataGrid.HasView && dataGrid.View.IsEditingItem)
            {
                if (this.CurrentCell.GridColumn.CanEndEditColumn())
                    currentCellRenderer.EndEdit(this.CurrentCell, currentCell.DataContext);
                if (canCommit && !(datarow is UnBoundRow))
                    dataGrid.View.CommitEdit();
            }
            //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
            //hence the below condition is added.
            else if (this.CurrentCell.GridColumn.CanEndEditColumn())
                currentCellRenderer.EndEdit(this.CurrentCell, currentCell.DataContext);

            return true;
        }

        #endregion

        #endregion

        #region Private Methods
        /// <summary>
        /// Processes the current cell when column added or removed at runtime.
        /// </summary>
        /// <param name="changedColumn">
        /// Contains the corresponding changed column.
        /// </param>
        /// <param name="changedIndex">
        /// The corresponding index of the column.
        /// </param>
        /// <param name="action">
        /// Corresponding collection changed action performed on columns.
        /// </param>
        protected void ProcessColumnRemoveAndInsert(GridColumn changedColumn, int changedIndex, NotifyCollectionChangedAction action)
        {
            int nextCellIndex = -1;
            if (changedColumn != null && CurrentCell.GridColumn != null && changedColumn.MappingName == CurrentCell.GridColumn.MappingName)
            {
                if (action == NotifyCollectionChangedAction.Remove)
                {
                    //WPF-17756 while deleting the column the DataGrid Maintain the selection, So here we have Maintain the Selection
                    //only for the Selection Unit as Row otherwise the Selection will be removed.
                    if (this.dataGrid.SelectionUnit == GridSelectionUnit.Row)
                    {
                        RemoveCurrentCell(CurrentRowColumnIndex);
                        nextCellIndex = this.GetFirstCellIndex();
                        SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
                    }
                    else
                    {
                        RemoveCurrentCell(CurrentRowColumnIndex);
                        UpdateGridProperties(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, -1));

                    }
                }
#if !WinRT && !UNIVERSAL
                else if (action == NotifyCollectionChangedAction.Add)
                {
                    // changed index is already resolved. So below code is commented
                    // nextCellIndex = dataGrid.ResolveToScrollColumnIndex(changedIndex);                   
                    nextCellIndex = changedIndex;
                    RemoveCurrentCell(CurrentRowColumnIndex);
                    SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
                }

                else if (action == NotifyCollectionChangedAction.Replace)
                {
                    RemoveCurrentCell(CurrentRowColumnIndex);
                    SelectCurrentCell(CurrentRowColumnIndex);
                }
#endif
            }
            else
            {
                if (changedIndex <= CurrentCell.ColumnIndex)
                    nextCellIndex = action == NotifyCollectionChangedAction.Add ? CurrentRowColumnIndex.ColumnIndex + 1 : CurrentRowColumnIndex.ColumnIndex - 1;
            }
            if (nextCellIndex != -1)
                this.SetCurrentColumnIndex(nextCellIndex);
        }

        private void ProcessColumnPositionChanged(NotifyCollectionChangedEventArgs args, bool needToMove)
        {
            int nextCellIndex = -1;
            int oldColumnIndex = dataGrid.ResolveToScrollColumnIndex(args.OldStartingIndex);
            if (oldColumnIndex > CurrentRowColumnIndex.ColumnIndex && dataGrid.ResolveToScrollColumnIndex(args.NewStartingIndex) <= CurrentRowColumnIndex.ColumnIndex)
                nextCellIndex = CurrentRowColumnIndex.ColumnIndex + 1;
            else if (dataGrid.ResolveToScrollColumnIndex(args.NewStartingIndex) >= CurrentRowColumnIndex.ColumnIndex && oldColumnIndex < CurrentRowColumnIndex.ColumnIndex)
                nextCellIndex = CurrentRowColumnIndex.ColumnIndex - 1;
            else if (needToMove || ((GridColumn)args.OldItems[0]).MappingName == CurrentCell.GridColumn.MappingName)
            {
                nextCellIndex = dataGrid.ResolveToScrollColumnIndex(args.NewStartingIndex);
                RemoveCurrentCell(CurrentRowColumnIndex);
                SelectCurrentCell(new RowColumnIndex(CurrentRowColumnIndex.RowIndex, nextCellIndex));
            }
            if (nextCellIndex != -1)
                SetCurrentColumnIndex(nextCellIndex);
        }

        #endregion

        #region Protected Methods
        /// <summary>
        /// Removes the current cell based on the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to remove the current cell.
        /// </param>
        /// <remarks>
        /// This method helps to remove the current cell corresponding to the specified row and column index.
        /// </remarks>
        protected internal void RemoveCurrentCell(RowColumnIndex rowColumnIndex)
        {
            if (CurrentCell != null)
            {
                CurrentCell.IsCurrentCell = false;
                if (CurrentCell.Renderer != null)
                {
                    var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsEditing);
                    if (CurrentCell.IsEditing && !this.IsFilterRow)
                    {
                        CurrentCell.Renderer.EndEdit(CurrentCell, CurrentCell.ColumnElement.DataContext);
                        if (dataRow.RowType != RowType.UnBoundRow && dataRow.RowType != RowType.AddNewRow)
                        {
                            dataGrid.ValidationHelper.ResetValidations(true);
                            if (dataGrid.View != null)
                                dataGrid.View.CommitEdit();
                        }
                        CurrentCell.IsEditing = false;

                        dataRow.IsEditing = false;
                        oldCellValue = null;
                    }
#if WPF
                    if (dataRow == null && dataGrid.useDrawing)
                    {
                        dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
                        if(dataRow != null)                                                    
                            dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();                        
                    }
#endif
                    CurrentCell.Renderer.ResetCurrentCellState();
                }
                CurrentCell = null;
            }
            var dataColumnBase = this.dataGrid.GetDataColumnBase(new RowColumnIndex(this.CurrentRowColumnIndex.RowIndex, this.CurrentRowColumnIndex.ColumnIndex));
            if (dataColumnBase != null && this.dataGrid.CanQueryCoveredRange())
            {
                if (dataColumnBase.IsCurrentCell)
                    dataColumnBase.IsCurrentCell = false;
            }
        }

        /// <summary>
        /// Scrolls the row in to view at the specified row index.
        /// </summary>
        /// <param name="rowIndex">
        /// The zero-based row index to scroll the row into view.
        /// </param>
        protected internal void ScrollToRowIndex(int rowIndex)
        {
            if (rowIndex < 0)
                return;

            var visibleRowLines = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            var rowLineInfo = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowIndex);
            bool isScrolled = false;

            if (visibleRowLines.FirstBodyVisibleIndex < visibleRowLines.Count)
            {
                if (rowIndex < visibleRowLines[visibleRowLines.FirstBodyVisibleIndex].LineIndex || rowIndex > visibleRowLines[visibleRowLines.LastBodyVisibleIndex].LineIndex || (rowLineInfo != null && rowLineInfo.IsClipped))
                {
                    this.dataGrid.VisualContainer.ScrollRows.ScrollInView(rowIndex);
                    isScrolled = true;
                }
            }

            if (isScrolled)
                this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Scrolls the SfDataGrid vertically and horizontally to display a cell for the specified RowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the rowColumnIndex of the cell to bring into view.
        /// </param>
        protected internal void ScrollInView(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.RowIndex < 0)
                return;
            var visibleRowLines = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            var rowLineInfo = this.dataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowColumnIndex.RowIndex);
            bool isScrolled = false;

            if (visibleRowLines.FirstBodyVisibleIndex < visibleRowLines.Count)
            {
                //UWP - 154 - When LiveDataUpdateMode is AllowDataShaping and when click the check box after grouping, the ScrollRows is updated. 
                //So the row index is not present in the VisualContainer. Hence the null check is added here.
                if (rowColumnIndex.RowIndex < visibleRowLines[visibleRowLines.FirstBodyVisibleIndex].LineIndex || rowColumnIndex.RowIndex > visibleRowLines[visibleRowLines.LastBodyVisibleIndex].LineIndex || (rowLineInfo != null && rowLineInfo.IsClipped))
                {
                    this.dataGrid.VisualContainer.ScrollRows.ScrollInView(rowColumnIndex.RowIndex);
                    isScrolled = true;
                }
            }

            if (rowColumnIndex.ColumnIndex < 0)
                return;

            var visibleColumnLines = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
            var columnLineInfo = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(rowColumnIndex.ColumnIndex);

            if (visibleColumnLines.FirstBodyVisibleIndex < visibleColumnLines.Count)
            {
                if (rowColumnIndex.ColumnIndex < visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex || rowColumnIndex.ColumnIndex > visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex || (columnLineInfo != null && columnLineInfo.IsClipped))
                {
                    this.dataGrid.VisualContainer.ScrollColumns.ScrollInView(rowColumnIndex.ColumnIndex);
                    isScrolled = true;
                }
            }

            if (isScrolled)
                this.dataGrid.VisualContainer.InvalidateMeasureInfo();
        }

        /// <summary>
        /// Selects the current cell for the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to select the current cell.
        /// </param>
        /// <param name="setFocus">
        /// Decides whether the focus is set to current cell.
        /// </param>
        protected internal void SelectCurrentCell(RowColumnIndex rowColumnIndex, bool setFocus = true)
        {
            var dataColumn = this.dataGrid.GetDataColumnBase(rowColumnIndex);
            this.setFocusForGrid = setFocus;

            if (dataColumn != null && !(rowColumnIndex.RowIndex < 0) && (this.dataGrid.NavigationMode == NavigationMode.Cell || this.dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex) || this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex)))
            {
                dataColumn.IsCurrentCell = true;
                if (dataColumn.Renderer != null)
                {
                    dataColumn.Renderer.SetCurrentCellState(rowColumnIndex, dataColumn.ColumnElement, dataColumn.IsEditing, setFocusForGrid, dataColumn.GridColumn, dataColumn);
                    SetOldCellValue(dataColumn, rowColumnIndex);
                    setFocusForGrid = false;
                }
                this.CurrentCell = dataColumn;
            }

            if ((dataColumn == null || dataColumn is SpannedDataColumn) && !this.dataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex) && setFocus)
#if WinRT || UNIVERSAL
                this.dataGrid.Focus(FocusState.Programmatic);
#else
                this.dataGrid.Focus();
#endif
            this.UpdateGridProperties(rowColumnIndex);

        }
        private void SetOldCellValue(DataColumnBase dataColumn, RowColumnIndex rowColumnIndex)
        {
            var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowColumnIndex.RowIndex);
#if WPF
            if(dataGrid.useDrawing)
                dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
            if (dataRow != null && dataRow is DataRow && dataRow.RowType != RowType.UnBoundRow && dataColumn.GridColumn != null)
            {
                if (dataColumn.GridColumn.MappingName != null)
                    oldCellValue = this.dataGrid.View.GetPropertyAccessProvider().GetValue(dataRow.RowData, dataColumn.GridColumn.MappingName, true);
                else
                    oldCellValue = dataColumn.Renderer.GetControlValue();
            }
        }

        /// <summary>
        /// Scrolls the specified column index in to view from the right direction to the SfDataGrid.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to scroll the column into view.
        /// </param>  
        /// <remarks>
        /// This method helps to scroll the column into view if it is not present in the view area of SfDataGrid.
        /// </remarks>
        public void ScrollInViewFromRight(int columnIndex)
        {
            VisibleLineInfo lineInfo = dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);

            //Here we checking whether the column is not in visible or clipped
            if (columnIndex > dataGrid.VisualContainer.ScrollColumns.LastBodyVisibleLineIndex || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                dataGrid.VisualContainer.ScrollColumns.ScrollInView(columnIndex);
                lineInfo = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
                if (lineInfo != null && dataGrid is DetailsViewDataGrid && dataGrid.NotifyListener != null)
                    DetailsViewHelper.ScrollInViewAllDetailsViewParent(dataGrid);
                dataGrid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        /// <summary>
        /// Scrolls the specified column index in to view from the left direction to the SfDataGrid.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to scroll the column into view.
        /// </param>  
        /// <remarks>
        /// This method helps to scroll the column into view when the column is not present in the view area of SfDataGrid.
        /// </remarks>
        public void ScrollInViewFromLeft(int columnIndex)
        {
            VisibleLinesCollection lineCollection = dataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
            VisibleLineInfo lineInfo = dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
            int firstBodyVisibleIndex = -1;
            if (lineCollection.FirstBodyVisibleIndex < lineCollection.Count)
                firstBodyVisibleIndex = lineCollection[lineCollection.FirstBodyVisibleIndex].LineIndex;

            //Here we checking whether the column is not in visible or clipped
            if (columnIndex >= 0 && (firstBodyVisibleIndex >= 0 && columnIndex < firstBodyVisibleIndex) || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                dataGrid.VisualContainer.ScrollColumns.ScrollInView(columnIndex);
                lineInfo = this.dataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(columnIndex);
                if (lineInfo != null && dataGrid is DetailsViewDataGrid && dataGrid.NotifyListener != null)
                    DetailsViewHelper.ScrollInViewAllDetailsViewParent(dataGrid);
                dataGrid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        /// <summary>
        /// Determines whether the focus is allowed for the specified rowcolumnindex .
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex. 
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the focus is allowed for the specified rowcolumnindex; otherwise, <b>false.</b>
        /// </returns>
        protected internal bool AllowFocus(RowColumnIndex rowColumnIndex)
        {
            var dataColumn = this.dataGrid.GetDataColumnBase(rowColumnIndex);
            if (dataColumn != null && !(dataColumn is SpannedDataColumn) && dataColumn.GridColumn != null)
                return this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || this.dataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex) ? !dataColumn.GridColumn.IsHidden : dataColumn.GridColumn.AllowFocus && !dataColumn.GridColumn.IsHidden;
            return true;
        }

        /// <summary>
        /// Determines whether the selection is allowed for the specified rowcolumnindex of current cell.
        /// </summary>
        /// <param name="currentCellIndex">
        /// The corresponding rowcolumnindex.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the selection is allowed to the current cell; otherwise , <b>false</b>.
        /// </returns>
        protected bool AllowCurrentCellSelection(RowColumnIndex currentCellIndex)
        {
            if (this.dataGrid.RowGenerator.Items.Any(item => item.RowIndex == currentCellIndex.RowIndex))
            {
                var dataColumn = this.dataGrid.GetDataColumnBase(currentCellIndex);
                if (dataColumn != null && dataColumn.Renderer != null && (dataColumn.Renderer is GridCellCheckBoxRenderer || dataColumn.Renderer.IsEditable || dataColumn is SpannedDataColumn))
                    return true;
                else if (dataColumn == null)
                {
                    var dataRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentCellIndex.RowIndex);
                    return (dataRow is DataRow || dataRow is SpannedDataRow);
                }
            }
            else
            {
                if (this.dataGrid.GridModel.HasGroup)
                {
                    var newObj = this.dataGrid.View.TopLevelGroup.DisplayElements[this.dataGrid.ResolveToRecordIndex(currentCellIndex.RowIndex)];
                    return newObj is RecordEntry || newObj is SummaryRecordEntry || newObj is Group || dataGrid.IsUnBoundRow(currentCellIndex.RowIndex);
                }
                else if (!this.dataGrid.IsInDetailsViewIndex(currentCellIndex.RowIndex))
                {
                    var resolvedIndex = this.dataGrid.ResolveToRecordIndex(currentCellIndex.RowIndex);
                    var recordsCount = this.dataGrid.GetRecordsCount();
                    var data = (recordsCount > 0 && resolvedIndex >= 0 && resolvedIndex < recordsCount)
                               ? this.dataGrid.View.Records[resolvedIndex].Data
                               : null;
                    return data != null || dataGrid.IsUnBoundRow(currentCellIndex.RowIndex);
                }
            }
            return false;
        }

        /// <summary>
        /// Gets the index of the last focused cell corresponding to the specified direction.
        /// </summary>
        /// <param name="direction">
        /// The corresponding direction of the cell to get its index.
        /// </param>
        /// <returns>
        /// Returns the corresponding index of last focused cell.
        /// </returns>
        protected internal int GetLastCellIndex(FlowDirection direction = FlowDirection.LeftToRight)
        {
            if (direction == FlowDirection.RightToLeft)
                return this.GetFirstCellIndex();

            int lastIndex = dataGrid.Columns.IndexOf(dataGrid.Columns.LastOrDefault(col => (col.AllowFocus && !col.IsHidden && col.ActualWidth != 0d)));
            lastIndex += dataGrid.HasView ? dataGrid.View.GroupDescriptions.Count : 0;
            if (dataGrid.DetailsViewManager.HasDetailsView)
                lastIndex += 1;

            if (dataGrid.ShowRowHeader)
                lastIndex += 1;

            CoveredCellInfo range = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out range, this.currentRowColumnIndex.RowIndex, lastIndex);

            if (range != null)
                return range.Left;
            return lastIndex;
        }

        /// <summary>
        /// Gets the index of the next row corresponding to the specified row index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the index of next row.
        /// </param>
        /// <returns>
        /// The index of next row; Returns , -1 when the row index is last row index.
        /// </returns>
        protected internal int GetNextRowIndex(int rowIndex)
        {
            if (rowIndex < this.dataGrid.HeaderLineCount)
                return this.dataGrid.HeaderLineCount;

            if (rowIndex >= this.dataGrid.VisualContainer.ScrollRows.LineCount)
                return -1;

            return this.dataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(rowIndex);
        }

        /// <summary>
        /// Gets the index of previous row corresponding to the specified row index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the previous row index.
        /// </param>
        /// <returns>
        /// Returns the index of previous row.
        /// </returns>
        protected internal int GetPreviousRowIndex(int rowIndex)
        {
            if (rowIndex <= this.dataGrid.HeaderLineCount)
                return this.dataGrid.HeaderLineCount;

            return this.dataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(rowIndex);
        }

        /// <summary>
        /// Gets the index of the next focused cell corresponding to the specified direction.
        /// </summary>
        /// <param name="flowdirection">
        /// The corresponding direction to get the index of next focused cell.
        /// </param>
        /// <returns>
        /// Returns the index of next focused cell.
        /// </returns>
        protected internal int GetNextCellIndex(FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return this.GetPreviousCellIndex();
            int nextCellIndex = CurrentRowColumnIndex.ColumnIndex;
            int lastCellIndex = dataGrid.GetLastColumnIndex();

            GridColumn column = this.GetNextFocusGridColumn(nextCellIndex + 1, MoveDirection.Right);
            if (column != null)
            {
                nextCellIndex = dataGrid.ResolveToScrollColumnIndex(dataGrid.Columns.IndexOf(column));
            }

            nextCellIndex = (nextCellIndex > lastCellIndex) ? lastCellIndex : nextCellIndex;

            return nextCellIndex;
        }

        /// <summary>
        /// Gets the index of next focused cell corresponding to the specified row and column index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get the next cell index.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get the next cell index.
        /// </param>
        /// <returns>
        /// Returns the index of next focused cell.
        /// </returns>
        protected internal int GetNextCellIndex(int rowIndex, int columnIndex)
        {
            if (!this.dataGrid.CanQueryCoveredRange())
                return columnIndex;

            var resolvedIndex = columnIndex;

            CoveredCellInfo inRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, rowIndex, columnIndex);

            CoveredCellInfo currentRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out currentRange, this.CurrentRowColumnIndex.RowIndex, this.CurrentRowColumnIndex.ColumnIndex);

            if (inRange != null)
            {
                if (columnIndex != this.CurrentRowColumnIndex.ColumnIndex && columnIndex == inRange.Left)
                    return columnIndex;

                if (columnIndex >= inRange.Left && columnIndex <= inRange.Right)
                {
                    if (columnIndex == this.CurrentRowColumnIndex.ColumnIndex && rowIndex == this.CurrentRowColumnIndex.RowIndex)
                        resolvedIndex = inRange.Left - 1;
                    else
                        resolvedIndex = inRange.Left;
                }

                if (resolvedIndex < 0 || resolvedIndex >= dataGrid.Columns.Count)
                    return columnIndex;

                return resolvedIndex;
            }
            else
                return resolvedIndex;
        }

        /// <summary>
        /// Gets the next focused row index corresponding to the specified row and column index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get next focused row index.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get next focused row index.
        /// </param>
        /// <param name="scrolling">
        /// Indicates whether the entire cell need to be scrolled in view , when the specified row index is merged cell.
        /// </param>
        /// <returns>
        /// The next focused row index of the specified row and column index.
        /// </returns>
        protected internal int GetNextRowIndex(int rowIndex, int columnIndex, bool scrolling = false)
        {
            if (!this.dataGrid.CanQueryCoveredRange())
                return rowIndex;

            var resolvedIndex = rowIndex;

            CoveredCellInfo inRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, rowIndex, columnIndex);

            if (inRange != null)
            {
                if (scrolling)
                    resolvedIndex = resolvedIndex == inRange.Top ? inRange.Bottom : resolvedIndex;

                resolvedIndex = resolvedIndex == inRange.Top ? resolvedIndex : (inRange.Bottom + 1) > (this.dataGrid.VisualContainer.RowCount - (this.dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + this.dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false))) ? inRange.Top : inRange.Bottom + 1;

                if (resolvedIndex >= this.dataGrid.GetLastNavigatingRowIndex())
                    return inRange.Top;
            }

            if (resolvedIndex >= this.dataGrid.GetLastNavigatingRowIndex())
                return this.dataGrid.GetLastNavigatingRowIndex();

            return resolvedIndex;
        }

        /// <summary>
        /// Gets the index of previous row corresponding to the specified row and column index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get the index of previous row.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get the index of previous row.
        /// </param>
        /// <returns>
        /// Returns the index of previous row.
        /// </returns>
        protected internal int GetPreviousRowIndex(int rowIndex, int columnIndex)
        {
            if (!this.dataGrid.CanQueryCoveredRange())
                return rowIndex;

            var resolvedIndex = rowIndex;

            CoveredCellInfo inRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, rowIndex, columnIndex);

            // WPF-35962 While getting previous row index in UP key Navigation we didn't consider the MergeCell in both DetailsViewDataGrid and ParentGrid.
            // So, the previous rowindex wrongly calculated when moving from childgrid to parentgrid and parentgrid to childgrid in UP key Navigation.           
            // Based on, detailsviewdefinition we are calculating the previous rowindex with or without mergecell in UP key Navigation.           
            if (inRange != null)
            {               
                if (resolvedIndex != inRange.Bottom)
                {
                    if (this.dataGrid.IsInDetailsViewIndex(inRange.Top - 1) && !(this.dataGrid.RowGenerator.Items[inRange.Top - 1]).IsExpanded)
                        return (inRange.Top - this.dataGrid.DetailsViewDefinition.Count - 1) > 0 ? (inRange.Top - this.dataGrid.DetailsViewDefinition.Count - 1) : (inRange.Top - this.dataGrid.DetailsViewDefinition.Count);
                    else
                        return (inRange.Top - 1) > 0 ? inRange.Top - 1 : inRange.Top;
                }
                else
                    return inRange.Top;
            }          


            return resolvedIndex;
        }

        /// <summary>
        /// Gets the index of the row positioned at the start of the previous page that is not currently in view of SfDataGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get previous page index.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get previous page index.
        /// </param>
        /// <returns>
        /// The start index of previous page.
        /// </returns>
        protected internal int GetPreviousPageIndex(int rowIndex, int columnIndex)
        {
            if (!this.dataGrid.CanQueryCoveredRange())
                return rowIndex;

            var resolvedIndex = rowIndex;

            CoveredCellInfo inRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, rowIndex, columnIndex);

            if (inRange != null)
                return resolvedIndex <= inRange.Top ? inRange.Top : resolvedIndex;

            return resolvedIndex;
        }

        /// <summary>
        /// Gets the index of the row positioned at the end of next page that is not currently in view of SfDataGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get next page index.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get next page index.
        /// </param>
        /// <returns>
        /// The end row index of next page.
        /// </returns>
        protected internal int GetNextPageIndex(int rowIndex, int columnIndex, bool scrolling = false)
        {
            if (!this.dataGrid.CanQueryCoveredRange())
                return rowIndex;

            var resolvedIndex = rowIndex;

            CoveredCellInfo inRange = null;
            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, rowIndex, columnIndex);

            if (inRange != null)
            {
                if (scrolling)
                    return resolvedIndex == inRange.Top ? inRange.Bottom : resolvedIndex;

                return resolvedIndex >= inRange.Top ? inRange.Top : resolvedIndex;
            }

            return resolvedIndex;
        }

        /// <summary>
        /// Gets the index of previous cell corresponding to the specified flow direction.
        /// </summary>
        /// <param name="flowdirection">
        /// The corresponding direction to get previous cell index.
        /// </param>
        /// <returns>
        /// Returns the index of previous cell.
        /// </returns>
        protected internal int GetPreviousCellIndex(FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return this.GetNextCellIndex();
            int previousCellIndex = CurrentRowColumnIndex.ColumnIndex;

            GridColumn column = GetNextFocusGridColumn(previousCellIndex - 1, MoveDirection.Left);
            if (column != null)
            {
                previousCellIndex = dataGrid.ResolveToScrollColumnIndex(dataGrid.Columns.IndexOf(column));
            }

            previousCellIndex = dataGrid.HasView && previousCellIndex < dataGrid.View.GroupDescriptions.Count
                                        ? CurrentRowColumnIndex.ColumnIndex
                                        : previousCellIndex;

            if (this.dataGrid.CanQueryCoveredRange())
            {
                CoveredCellInfo range = null;

                this.dataGrid.CoveredCells.GetCoveredCellInfo(out range, this.currentRowColumnIndex.RowIndex, previousCellIndex);

                if (range != null)
                {
                    if (previousCellIndex == this.CurrentCell.ColumnIndex)
                        return range.Left - 1 < GetFirstCellIndex() ? previousCellIndex : range.Left - 1;
                    else
                        return range.Left;
                }
            }
            return previousCellIndex;
        }

        /// <summary>
        /// Gets the next focused grid column for the specified column index and direction.
        /// </summary>
        /// <param name="columnIndex">
        /// The corresponding column index to get the next focused column.
        /// </param>
        /// <param name="direction">
        /// Specifies flow direction to get the next focused column.
        /// </param>
        /// <returns>
        /// The next focused <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> for specified column index and direction. 
        /// Returns <b>null</b>, if the specified column index is last column.
        /// </returns>
        protected internal GridColumn GetNextFocusGridColumn(int columnIndex, MoveDirection direction)
        {
            var resolvedIndex = dataGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            if (resolvedIndex < 0 || resolvedIndex >= dataGrid.Columns.Count)
                return null;

            var gridColumn = dataGrid.Columns[resolvedIndex];

            if (this.dataGrid.CanQueryCoveredRange())
                gridColumn = GetMergedColumn(gridColumn, columnIndex, direction);

            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || gridColumn.IsHidden)
            {
                if (this.dataGrid.IsAddNewIndex(this.CurrentRowColumnIndex.RowIndex) && gridColumn != null && gridColumn.ActualWidth != 0.0 && !gridColumn.IsHidden)
                    return gridColumn;
                else
                    gridColumn = this.GetNextFocusGridColumn(direction == MoveDirection.Right ? columnIndex + 1 : columnIndex - 1, direction);
            }
            return gridColumn;
        }

        /// <summary>
        /// Gets the merged column for the specified range.
        /// </summary>
        /// <param name="gridcolumn">
        /// The corresponding grid column to get merged column.
        /// </param>
        /// <param name="columnIndex">
        /// The corresponding column index to get merged column
        /// </param>
        /// <param name="direction">
        /// Contains the corresponding direction to get merged column.
        /// </param>
        /// <returns>
        /// Returns the corresponding merged column based on the specified range.        
        /// </returns>
        protected internal GridColumn GetMergedColumn(GridColumn gridcolumn, int columnIndex, MoveDirection direction)
        {
            var resolvedIndex = columnIndex;

            foreach (var range in dataGrid.CoveredCells)
            {
                if (range.Contains(this.CurrentRowColumnIndex.RowIndex, columnIndex))
                {
                    if (columnIndex != this.CurrentCell.ColumnIndex && columnIndex == range.Left)
                        return gridcolumn;

                    if (direction == MoveDirection.Right)
                    {
                        resolvedIndex = dataGrid.ResolveToGridVisibleColumnIndex(range.Right + 1);
                        var lastCellIndex = GetLastCellIndex();

                        if (resolvedIndex < 0 || resolvedIndex >= dataGrid.Columns.Count)
                        {
                            return null;
                        }

                        gridcolumn = dataGrid.Columns[resolvedIndex];

                        return gridcolumn;
                    }
                    else if (direction == MoveDirection.Left)
                    {
                        // to navigate to the left of merged cell if curernt cell is already a first left cell.
                        if (columnIndex >= range.Left && columnIndex <= range.Right && columnIndex != this.CurrentCell.ColumnIndex)
                            resolvedIndex = dataGrid.ResolveToGridVisibleColumnIndex(range.Left);
                        else
                            resolvedIndex = dataGrid.ResolveToGridVisibleColumnIndex(range.Left - 1);

                        if (resolvedIndex < 0 || resolvedIndex >= dataGrid.Columns.Count)
                            return null;

                        gridcolumn = dataGrid.Columns[resolvedIndex];

                        return gridcolumn;
                    }
                }
            }
            return gridcolumn;
        }

        /// <summary>
        /// Gets the index of first cell in SfDataGrid for the specified flow direction.
        /// </summary>
        /// <param name="direction">
        /// Contains the direction to get first cell index in SfDataGrid.
        /// </param>
        /// <returns>
        /// The first cell index in SfDataGrid for the specified flow direction.
        /// </returns>
        protected internal int GetFirstCellIndex(FlowDirection direction = FlowDirection.LeftToRight)
        {
            if (direction == FlowDirection.RightToLeft)
                return this.GetLastCellIndex();

            int firstColumnIndex = dataGrid.Columns.IndexOf(dataGrid.Columns.FirstOrDefault(col => (col.AllowFocus && !col.IsHidden && col.ActualWidth != 0d)));
            if (dataGrid.DetailsViewManager.HasDetailsView)
                firstColumnIndex += 1;

            if (dataGrid.HasView)
                firstColumnIndex += dataGrid.View.GroupDescriptions.Count;
            if (dataGrid.ShowRowHeader)
                firstColumnIndex += 1;
            return firstColumnIndex;
        }

        /// <summary>
        /// Gets the FirstCell index of the particular rowIndex.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        internal int GetFirstCellIndex(int rowIndex)
        {
            if (rowIndex < 0)
                return -1;

            if (this.dataGrid.IsAddNewIndex(rowIndex))
                return this.dataGrid.GetFirstColumnIndex();
            else
                return this.GetFirstCellIndex();
        }

        /// <summary>
        /// Gets the LastCell index of the particular rowIndex.
        /// </summary>
        /// <param name="rowIndex"></param>
        /// <returns></returns>
        internal int GetLastCellIndex(int rowIndex)
        {
            if (rowIndex < 0)
                return -1;

            if (this.dataGrid.IsAddNewIndex(rowIndex))
                return this.dataGrid.GetLastColumnIndex();
            else
                return this.GetLastCellIndex();
        }

        #region VirtualMethods
        /// <summary>
        /// Processes the selection for current cell corresponding to its rowcolumnindex and activation trigger. 
        /// </summary>
        /// <param name="newRowColumnIndex">
        /// The corresponding rowcolumnindex to process the current cell selection.
        /// </param>
        /// <param name="activationTriggger">
        /// Indicates how the current cell is activated.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the selection is processed on the current cell; otherwise, <b>false</b>.
        /// </returns>
        protected internal virtual bool ProcessCurrentCellSelection(RowColumnIndex newRowColumnIndex, ActivationTrigger activationTriggger)
        {
            if (newRowColumnIndex.ColumnIndex < GetFirstCellIndex() && !this.dataGrid.IsFilterRowIndex(newRowColumnIndex.RowIndex) && !this.dataGrid.IsAddNewIndex(newRowColumnIndex.RowIndex))
            {
                bool isAnySelection = newRowColumnIndex.ColumnIndex == 0 && this.dataGrid.SelectionUnit == GridSelectionUnit.Any;
                var nextFocusGridColumn = GetNextFocusGridColumn(GetFirstCellIndex(), MoveDirection.Right);
                var nextCellIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? dataGrid.ResolveToScrollColumnIndex(dataGrid.Columns.IndexOf(nextFocusGridColumn)) : CurrentRowColumnIndex.ColumnIndex;
                newRowColumnIndex.ColumnIndex = nextCellIndex;

                if (!isAnySelection && newRowColumnIndex == CurrentRowColumnIndex && this.dataGrid.SelectionMode != GridSelectionMode.Multiple)
                    return false;
            }
            else if (this.dataGrid.IsAddNewIndex(newRowColumnIndex.RowIndex) || this.dataGrid.IsFilterRowIndex(newRowColumnIndex.RowIndex))
            {
                if (newRowColumnIndex.ColumnIndex < this.dataGrid.GetFirstColumnIndex())
                    newRowColumnIndex.ColumnIndex = CurrentRowColumnIndex.ColumnIndex < 0 ? this.dataGrid.GetFirstColumnIndex() : CurrentRowColumnIndex.ColumnIndex;

                if (newRowColumnIndex.RowIndex == CurrentRowColumnIndex.RowIndex && newRowColumnIndex.ColumnIndex == CurrentRowColumnIndex.ColumnIndex && this.dataGrid.SelectionMode != GridSelectionMode.Multiple)
                    return false;
            }
            //Added the Condition while the AddNewRow is True or Not when the NavigationMode as Row
            if ((this.dataGrid.NavigationMode == NavigationMode.Cell || this.IsFilterRow || this.IsAddNewRow || this.dataGrid.IsFilterRowIndex(newRowColumnIndex.RowIndex) || this.dataGrid.IsAddNewIndex(newRowColumnIndex.RowIndex)) && newRowColumnIndex != CurrentRowColumnIndex)
            {
                bool isRecordCell = AllowCurrentCellSelection(newRowColumnIndex);
                if (isRecordCell && RaiseCurrentCellActivatingEvent(CurrentRowColumnIndex, newRowColumnIndex, activationTriggger))
                    return false;
                RemoveCurrentCell(CurrentRowColumnIndex);
                SelectCurrentCell(newRowColumnIndex, activationTriggger != ActivationTrigger.Program);

                if (isRecordCell)
                    RaiseCurrentCellActivatedEvent(newRowColumnIndex, previousRowColumnIndex, activationTriggger);
                return true;
            }
            return false;
        }

        #endregion

        #endregion

        #region Events & Event Raising Methods

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivating"/> event in SfDataGrid.
        /// </summary>
        /// <param name="previousRowColumnIndex">
        /// The rowcolumnindex of the precious active cell.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The rowcolumnindex of the currently active cell.                
        /// </param>
        /// <param name="activationTrigger">
        /// The activation trigger for current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivating"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseCurrentCellActivatingEvent(RowColumnIndex previousRowColumnIndex, RowColumnIndex currentRowColumnIndex, ActivationTrigger activationTrigger)
        {
            if (!ValidationHelper.IsCurrentCellValidated)
                return false;

            if (previousRowColumnIndex.RowIndex < 0 || previousRowColumnIndex.ColumnIndex < 0)
                previousRowColumnIndex = new RowColumnIndex(0, 0);

            var args = new CurrentCellActivatingEventArgs(dataGrid)
            {
                PreviousRowColumnIndex = previousRowColumnIndex,
                CurrentRowColumnIndex = currentRowColumnIndex,
                ActivationTrigger = activationTrigger
            };
            return dataGrid.RaiseCurrentCellActivatingEvent(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivated"/> event in SfDataGrid.
        /// </summary>
        /// <param name="previousRowColumnIndex">
        /// The rowcolumnindex of the precious active cell.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The rowcolumnindex of the currently active cell.                
        /// </param>
        /// <param name="activationTrigger">
        /// The activation trigger for current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivated"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected void RaiseCurrentCellActivatedEvent(RowColumnIndex rowColumnIndex, RowColumnIndex previousRowColumnIndex, ActivationTrigger activationTrigger)
        {
            if (!ValidationHelper.IsCurrentCellValidated)
                return;

            if (previousRowColumnIndex.RowIndex < 0 || previousRowColumnIndex.ColumnIndex < 0)
                previousRowColumnIndex = new RowColumnIndex(0, 0);

            var e = new CurrentCellActivatedEventArgs(dataGrid)
            {
                CurrentRowColumnIndex = rowColumnIndex,
                PreviousRowColumnIndex = previousRowColumnIndex,
                ActivationTrigger = activationTrigger
            };
            dataGrid.RaiseCurrentCellActivatedEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellEndEdit"/> event.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The rowcolumnindex of the current cell to raise the event.
        /// </param>
        protected void RaiseCurrentCellEndEditEvent(RowColumnIndex rowColumnIndex)
        {
            if (!ValidationHelper.IsCurrentCellValidated)
                return;

            var args = new CurrentCellEndEditEventArgs(dataGrid)
            {
                RowColumnIndex = rowColumnIndex
            };
            dataGrid.RaiseCurrentCellEndEditEvent(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBeginEdit"/> event.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex of the current cell.
        /// </param>
        /// <param name="gridColumn">
        /// The corresponding column that contains the current cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBeginEdit"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseCurrentCellBeginEditEvent(RowColumnIndex rowColumnIndex, GridColumn gridColumn)
        {
            if (!ValidationHelper.IsCurrentCellValidated)
                return false;

            var args = new CurrentCellBeginEditEventArgs(dataGrid)
            {
                RowColumnIndex = rowColumnIndex,
                Column = gridColumn
            };
            return dataGrid.RaiseCurrentCellBeginEditEvent(args);
        }

        #endregion

        #region Dispose

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                this.CurrentCell = null;
                this.dataGrid = null;
            }
            isdisposed = true;
        }

        #endregion

    }
}
