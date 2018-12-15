#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Windows;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
#else
using System.Windows.Controls;
using System.Windows.Data;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Implements validation for user input through built-in validation or events of SfDataGrid.
    /// </summary>
    public class ValidationHelper : IDisposable
    {
        #region fields

        private static bool isCurrentCellValidated = true;
        private static bool isCurrentRowValidated = true;
        private SfDataGrid dataGrid;
        private static bool isFocusSetBack = false;
        private bool isdisposed = false;
        private Dictionary<string, string> errorMessages;
        int rowIndex = -1;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper"/> class.
        /// </summary>
        /// <param name="datagrid">
        /// The SfDataGrid.
        /// </param>
        public ValidationHelper(SfDataGrid datagrid)
        {
            dataGrid = datagrid;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the value that indicates the validation status of current cell.
        /// </summary>    
        /// <value>
        /// Returns <b>true</b> if the current cell validation is successful; otherwise, <b>false</b>.
        /// </value>
        public static bool IsCurrentCellValidated
        {
            get { return isCurrentCellValidated; }
            internal set { isCurrentCellValidated = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates the validation status of current row.
        /// </summary>     
        /// <value>
        /// Returns <b>true</b> if the current row validation is successful; otherwise, <b>false</b>.
        /// </value>
        public static bool IsCurrentRowValidated
        {
            get { return isCurrentRowValidated; }
            internal set { isCurrentRowValidated = value; }
        }

        /// <summary>
        /// Gets or sets the value that indicates to prevent the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidating"/> events to be raised more than once when click on FilterToggleButton. 
        /// </summary>
        /// <remarks>If IsFocusSetBack true then end user can't move out of the current cell by Pointer pressed on GridCell.
        /// While do key navigation and click on FilterToggleButton IsFocusSetBack needs to be in false to do Validation.
        /// </remarks>
        public static bool IsFocusSetBack
        {
            get { return isFocusSetBack; }
            internal set { isFocusSetBack = value; }
        }
        /// <summary>
        /// Gets or sets the reference to the currently active SfDataGrid during validation.
        /// </summary>
        /// <value>
        /// The reference to the currently active SfDataGrid.
        /// </value>
        public static SfDataGrid ActiveGrid { get; set; }

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper.ActiveGrid"/> and <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper.IsCurrentRowValidated"/> value.
        /// </summary>
        /// <param name="isRowValidated">
        /// Indicates whether the current row validation is successful.
        /// </param>
        public void SetCurrentRowValidated(bool isRowValidated)
        {
            ActiveGrid = dataGrid;
            IsCurrentRowValidated = isRowValidated;
        }

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper.ActiveGrid"/> and <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper.IsCurrentCellValidated"/> value.
        /// </summary>
        /// <param name="isRowValidated">
        /// Indicates whether the current cell validation is successful.
        /// </param>
        public void SetCurrentCellValidated(bool isCellValidated)
        {
            ActiveGrid = dataGrid;
            IsCurrentCellValidated = isCellValidated;
        }
        #endregion

        #region Methods
        
        internal static void SetFocusSetBack(bool isFocusSetBack)
        {
            if (!isFocusSetBack)
            {
                IsFocusSetBack = isFocusSetBack;
                return;
            }

            if (ActiveGrid == null || ActiveGrid.VisualContainer == null)
                return;

            // Need to focus the pressed cell in the current row if the cell is not in view while do row validation. Need to reset IsFocusSetBack property.
            var visibleInfo = ActiveGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(ActiveGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
            if (visibleInfo == null)
                IsFocusSetBack = false;
            else
                IsFocusSetBack = isFocusSetBack;
        }

        //WPF-26265 Here we have to check the Validation for parentDataGrid when parentGrid as a DetailsViewGrid
        internal bool CheckForValidation()
        {
            var parentDataGrid = this.dataGrid.GetParentDataGrid();
            if (parentDataGrid != null)
            {
                if (parentDataGrid.SelectedDetailsViewGrid == null && parentDataGrid is DetailsViewDataGrid)
                    return parentDataGrid.ValidationHelper.CheckForValidation();
                else if (this.dataGrid != parentDataGrid.SelectedDetailsViewGrid)
                {
                    if (parentDataGrid.ValidationHelper.RaiseCellValidate(parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex))
                        return parentDataGrid.ValidationHelper.RaiseRowValidate(parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
                    return false;
                }
            }
            return this.dataGrid.ValidationHelper.CheckForValidation(true);
        }

        internal void ResetValidations(bool canResetRow)
        {
            ResetValidation(dataGrid, canResetRow);
        }

        /// <summary>
        /// Method which helps to reset the row and cell validation of SfDataGrid.
        /// </summary>
        /// <param name="grid"></param>
        /// <param name="canResetRow">
        /// true, if need to reset row validation error message. Otherwise, false.
        /// </param>
        private void ResetValidation(SfDataGrid grid, bool canResetRow)
        {
            var rowColumnIndex = grid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
            var dataRow = grid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow == null)
            {
                SetCurrentCellValidated(true);
                SetCurrentRowValidated(true);
                return;
            }

            if (dataRow is DetailsViewDataRow)
            {
                ResetValidation((dataRow as DetailsViewDataRow).DetailsViewDataGrid, canResetRow);
                return;
            }

            var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
            if (dataColumn != null && dataColumn.ColumnElement != null)
            {
                SetCurrentCellValidated(true);
                if (!canResetRow) //In Else, CellValidation Error Message will be removed by calling RemoveError method below.
                {
                    dataRow.VisibleColumns.ForEach
                      (item =>
                      {
                          if (!(item.ColumnElement is GridCell)) return;
                          var gridcell = item.ColumnElement as GridCell;
                          if (!string.IsNullOrEmpty(gridcell.celleventErrorMessage))
                              gridcell.RemoveCellValidationError(false);
                      });
                    return;
                }
                SetCurrentRowValidated(true);
                this.dataGrid.ValidationHelper.RemoveError(dataRow, false);
            }
        }

        internal bool RaiseRowValidate(RowColumnIndex currentCellIndex)
        {
            if (IsCurrentRowValidated)
                return true;
#if WinRT || UNIVERSAL
            // In winrt and universal , the datagrid is from click notification. where we need to call parent validation to raise the cell validation for child grid.
            if (dataGrid is DetailsViewDataGrid)
            {
                var parentDataGrid = dataGrid.GetParentDataGrid();
                if (parentDataGrid.SelectedDetailsViewGrid == null || (parentDataGrid.SelectedDetailsViewGrid != null && dataGrid != parentDataGrid.SelectedDetailsViewGrid))
                    return parentDataGrid.ValidationHelper.RaiseRowValidate(parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
            }
#endif
            if (dataGrid.IsInDetailsViewIndex(currentCellIndex.RowIndex))
            {
                if (dataGrid.SelectionController is GridBaseSelectionController)
                {
                    var currentDetailsViewGrid = (dataGrid.SelectionController as GridBaseSelectionController).GetCurrentDetailsViewGrid(dataGrid);
                    return currentDetailsViewGrid.ValidationHelper.RaiseRowValidate(currentDetailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
                }
            }

            var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentCellIndex.RowIndex);
            if (dataRow == null) return true;
            errorMessages = new Dictionary<string, string>();
            rowIndex = dataRow.RowIndex;
            var validatingArgs = new RowValidatingEventArgs(dataRow.WholeRowElement.DataContext, currentCellIndex.RowIndex, errorMessages,dataGrid);

            if (!dataGrid.RaiseRowValidatingEvent(validatingArgs))
            {         
                dataRow.VisibleColumns.ForEach
                (datacolbase => 
                    {
                        if (datacolbase.GridColumn == null) return;
                        if (datacolbase.ColumnElement == null) return;

                        var currentCell = datacolbase.ColumnElement as GridCell;
                        if (currentCell == null)
                            return;

                        if (validatingArgs.ErrorMessages.ContainsKey(datacolbase.GridColumn.MappingName))
                            currentCell.SetRowValidationError(validatingArgs.ErrorMessages[datacolbase.GridColumn.MappingName]);
                        else
                        {
                            if (currentCell.HasError)
                                currentCell.RemoveRowValidationError();
                        }
                    });             

                dataRow.WholeRowElement.SetError();
                if (!dataRow.IsEditing)
                    dataGrid.SelectionController.CurrentCellManager.BeginEdit();
                //WPF-31495 - Set CurrentCellVaildate only for isEditing datarow.
                if (dataRow.IsEditing)
                    SetCurrentCellValidated(false);
                return false;
            }
            else
            {
                RemoveError(dataRow, false);
            }

            var args = new RowValidatedEventArgs(validatingArgs.RowData, validatingArgs.RowIndex, validatingArgs.ErrorMessages,dataGrid);
            dataGrid.RaiseRowValidatedEvent(args);
            SetCurrentRowValidated(true);
            //this.dataGrid.VisualContainer.SuspendManipulationScroll = false;
            errorMessages = null;
            rowIndex = -1;
            return true;
        }

        /// <summary>        
        /// Method which help to update the error message for particular row based on validation applied on SfDataGrid.
        /// </summary>
        /// <param name="dataRow">
        /// Contains the current datarow of SfDataGrid
        /// </param>   
        /// <param name="removeAll">The bool variable used to remove validation error message</param>
        internal void RemoveError(DataRowBase dataRow, bool removeAll)
        {
            if (dataRow == null)
                return;
            foreach (DataColumnBase column in dataRow.VisibleColumns)
            {
                if (column.ColumnElement is GridCell)
                {
                    var gridcell = column.ColumnElement as GridCell;
                    if (removeAll)
                        gridcell.RemoveAll();
                    else
                    {
                        gridcell.RemoveRowValidationError();
                        gridcell.RemoveCellValidationError(false);
                    }
                }
            }
            dataRow.WholeRowElement.RemoveError();
        }
        internal bool RaiseCellValidate(RowColumnIndex currentCellIndex, IGridCellRenderer renderer, bool allowattributeValidation)
        {
            if (IsCurrentCellValidated)
                return true;

#if WinRT || UNIVERSAL
            // In winrt and universal , the datagrid is from click notification. where we need to call parent validation to raise the cell validation for child grid.
            //In Multiple SelectionMode, the SelectedDetailsViewDataGrid will be null when navigating through CurrentCell, hence we have added the condition by checking CurrentCell.
            if (dataGrid is DetailsViewDataGrid && !dataGrid.SelectionController.CurrentCellManager.HasCurrentCell)
            {                
                var parentDataGrid = dataGrid.GetParentDataGrid();
                if (parentDataGrid.SelectedDetailsViewGrid == null || (parentDataGrid.SelectedDetailsViewGrid != null && dataGrid != parentDataGrid.SelectedDetailsViewGrid))
                    return parentDataGrid.ValidationHelper.RaiseCellValidate(parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex, null, true);
            }
#endif
            if (dataGrid.IsInDetailsViewIndex(currentCellIndex.RowIndex))
            {
                if (dataGrid.SelectionController is GridBaseSelectionController)
                {
                    var currentDetailsViewGrid = (dataGrid.SelectionController as GridBaseSelectionController).GetCurrentDetailsViewGrid(dataGrid);
                    return currentDetailsViewGrid.ValidationHelper.RaiseCellValidate(currentDetailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex, null, true);
                }
            }                                                 
            // WPF-34211 - Need to avoid the row taken from RowGenerator items when the current row index is -1.                     
            var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentCellIndex.RowIndex &&  currentCellIndex.RowIndex != -1);
            if (dataRow == null) 
                return false;
            // WPF-34211 - Need to avoid the column taken from VisibleColumns when the current column index is -1.
            var columnBase = dataRow.VisibleColumns.FirstOrDefault(x => x.ColumnIndex == currentCellIndex.ColumnIndex && currentCellIndex.ColumnIndex != -1);
            if(columnBase == null)
                return false;
            renderer = renderer ?? columnBase.Renderer;

            if (renderer == null)
                return false;
            if (IsFocusSetBack)
                return false;

			//WPF-36608 - CurrenctCellvalidating event are not fired, when we select cell on treeGrid template column.
            if (!columnBase.Renderer.CanValidate())
            {
                SetCurrentCellValidated(true);
                return true;
            }
            object oldValue = null;
            object changedNewValue;
            string errorMessage;
            if(this.dataGrid.SelectionController is GridBaseSelectionController)
                oldValue = (this.dataGrid.SelectionController as GridBaseSelectionController).CurrentCellManager.oldCellValue;
            var newCellValue = renderer.GetControlValue();
            if (this.RaiseCurrentCellValidatingEvent(oldValue, newCellValue , columnBase.GridColumn,out changedNewValue, currentCellIndex,columnBase.ColumnElement as FrameworkElement,out errorMessage,dataRow.RowData))
            {
                bool isValid = true;
                if (newCellValue != changedNewValue)
                    renderer.SetControlValue(changedNewValue);
                if (allowattributeValidation && this.dataGrid.MergedCellManager.CanRasieEvent)
                {
                        if ((columnBase.ColumnElement is GridCell) && (columnBase.ColumnElement as GridCell).Content != null && (columnBase.ColumnElement as GridCell).Content is FrameworkElement)
                            renderer.UpdateSource((columnBase.ColumnElement as GridCell).Content as FrameworkElement);
                        if (columnBase.GridColumn.GridValidationMode != GridValidationMode.None)
                        {
                            isValid = this.dataGrid.ValidationHelper.ValidateColumn(dataRow.RowData, columnBase.GridColumn.MappingName, (columnBase.ColumnElement as GridCell), currentCellIndex)
                                && DataValidation.Validate((columnBase.ColumnElement as GridCell), columnBase.GridColumn.MappingName, columnBase.ColumnElement.DataContext);
                        }
#if !WinRT && !UNIVERSAL
                        if (!isValid && columnBase.GridColumn.GridValidationMode == GridValidationMode.InEdit)
                            return false;                                          
#endif
                }
                if (this.dataGrid.MergedCellManager.CanRasieEvent) 
                    this.RaiseCurrentCellValidatedEvent(oldValue, columnBase.Renderer.GetControlValue(), columnBase.GridColumn, errorMessage, dataRow.RowData);
                SetCurrentCellValidated(true);
                return true;
            }
            return false;
        }

        internal bool RaiseCellValidate(RowColumnIndex currentCellIndex)
        {
            return this.RaiseCellValidate(currentCellIndex, null, true);
        }


        internal bool RaiseCurrentCellValidatingEvent(object oldValue, object newValue, GridColumn column, out object changedNewValue, RowColumnIndex currentCellIndex, FrameworkElement currentCell, out string errorMessage, object rowData)
        {
            if (this.dataGrid.CanQueryCoveredRange() && !this.dataGrid.MergedCellManager.CanRasieEvent)
            {
                changedNewValue = newValue;
                errorMessage = string.Empty;
                return true;
            }
            
            var e = new CurrentCellValidatingEventArgs(dataGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                IsValid = true,
                RowData = rowData
            };
            var isValid = dataGrid.RaiseCurrentCellValidatingEvent(e);
            changedNewValue = e.NewValue;

            var cell = currentCell as GridCell;
            if (!isValid)
                cell.SetCellValidationError(e.ErrorMessage, false);
            else if (errorMessages == null || !(errorMessages.Keys.Any(x => x == column.MappingName)))
                cell.RemoveCellValidationError(false);


            errorMessage = e.ErrorMessage;
            return isValid;
        }

        internal void RaiseCurrentCellValidatedEvent(object oldValue, object newValue, GridColumn column, string errorMessage, object rowData)
        {
            var e = new CurrentCellValidatedEventArgs(dataGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                ErrorMessage = errorMessage,
                RowData = rowData
            };
            dataGrid.RaiseCurrentCellValidatedEvent(e);
        }

        internal bool ValidateColumn(object rowData, string columnName, GridCell currentCell, RowColumnIndex currentCellIndex)
        {
            var propertyName = columnName;
            bool isValid = true;
            var errorMessage = string.Empty;
            bool isAttributeError = false;
            if (rowData == null || string.IsNullOrEmpty(columnName) || currentCell == null || currentCellIndex == RowColumnIndex.Empty)
                return isValid;
            var itemproperties = this.dataGrid.View.GetItemProperties();
            if (itemproperties == null)
                return isValid;
         
            // WPF-25016 Using PropertyDescriptorExtensions for WPF and PropertyInfoExtensions for WinRT, the codes are cleaned up
            if (columnName.Contains('.'))
            {
                var propNames = columnName.Split('.');
                columnName = propNames[propNames.Length - 1];
                Array.Resize(ref propNames, propNames.Length - 1);
                var pName = string.Join(".", propNames);
#if WPF
                rowData = PropertyDescriptorExtensions.GetValue(itemproperties, rowData, pName);
#else
                rowData = Syncfusion.Data.PropertyInfoExtensions.GetValue(itemproperties, rowData, pName);
#endif
            }

            PropertyInfo propertyinfo = null;
            ValidationContext validationContext = null;
            
#if !WinRT
            if (rowData != null)
            {
                propertyinfo = rowData.GetType().GetProperty(columnName);
                validationContext = new ValidationContext(rowData, null, null) { MemberName = columnName };
            }
#else
            if (rowData != null)
            {
               propertyinfo = rowData.GetType().GetRuntimeProperties().FirstOrDefault(x => x.Name == columnName);
                validationContext = new ValidationContext(rowData) { MemberName = columnName };
            }
#endif

            if (errorMessages != null && rowIndex == currentCellIndex.RowIndex && errorMessages.Keys.Contains(columnName) && IsCurrentRowValidated)
                errorMessage = errorMessages[columnName];
            if ((this.dataGrid.Columns[propertyName] as GridColumn).GridValidationMode != GridValidationMode.None)
            {
               if (propertyinfo != null)
                {
                    var validationAttribute = propertyinfo.GetCustomAttributes(false).OfType<ValidationAttribute>();
                    var value = propertyinfo.GetValue(rowData, null);
                    var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
                    try
                    {
                        if (!Validator.TryValidateValue(value, validationContext, results, validationAttribute))
                        {
                            foreach (var result in results)
                                errorMessage = !string.IsNullOrEmpty(errorMessage) ? errorMessage + string.Format("\n") + result.ErrorMessage : errorMessage + result.ErrorMessage;
                            isValid = false;
                            isAttributeError = true;
                        }
                    }
                    catch(Exception e)
                    {
                        errorMessage = e.Message;
                        isValid = false;
                        isAttributeError = true;
                    }
                }
            }

            if (currentCell != null)
            {
                if (!isValid || !string.IsNullOrEmpty(errorMessage))
                    currentCell.SetCellValidationError(errorMessage, isAttributeError);
                else
                    currentCell.RemoveCellValidationError(true);
            }
#if !SyncfusionFramework4_0 && !SyncfusionFramework3_5 || UWP
            if (rowData is INotifyDataErrorInfo)
                isValid = isValid && DataValidation.ValidateINotifyDataErrorInfo(currentCell, columnName, rowData);
#endif
            return isValid;
        }
        
        internal void ValidateColumns(DataRowBase dr)
        {
            foreach (var column in dr.VisibleColumns)
                if (column.GridColumn != null)
                    this.ValidateColumn(dr.RowData, column.GridColumn.MappingName, column.ColumnElement as GridCell, new RowColumnIndex(column.RowIndex, column.ColumnIndex));
        }

        internal bool CheckForValidation(bool canRaiseEvent) 
        {
            if (!canRaiseEvent)
                return IsCurrentCellValidated && IsCurrentRowValidated;

            if (!IsCurrentCellValidated || !IsCurrentRowValidated)
            {
                if (this.RaiseCellValidate(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex))
                    return this.RaiseRowValidate(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
                return false;
            }
            return true;
        }



        #endregion
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.ValidationHelper"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                if (errorMessages != null)
                {
                    errorMessages.Clear();
                    errorMessages = null;
                }
                ActiveGrid = null;
                dataGrid = null;
            }
            isdisposed = true;
        }
    }
}
