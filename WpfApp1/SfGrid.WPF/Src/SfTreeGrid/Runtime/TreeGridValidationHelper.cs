#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if UWP
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{

    /// <summary>
    /// Represents a class that implements validation behaviors of SfTreeGrid.
    /// </summary>
    public class TreeGridValidationHelper : IDisposable
    {
        #region fields

        private static bool isCurrentCellValidated = true;
        private static bool isCurrentRowValidated = true;
        private SfTreeGrid treeGrid;
        private bool isdisposed = false;
        private Dictionary<string, string> errorMessages;
        int rowIndex = -1;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        public TreeGridValidationHelper(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
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
        /// Gets or sets the reference to the currently active SfTreeGrid during validation.
        /// </summary>
        /// <value>
        /// The reference to the currently active SfTreeGrid.
        /// </value>
        public static SfTreeGrid ActiveGrid { get; set; }

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper.ActiveGrid"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper.IsCurrentRowValidated"/> value.
        /// </summary>
        /// <param name="isRowValidated">
        /// Indicates whether the current row validation is successful.
        /// </param>
        public void SetCurrentRowValidated(bool isRowValidated)
        {
            ActiveGrid = treeGrid;
            IsCurrentRowValidated = isRowValidated;
        }

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper.ActiveGrid"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper.IsCurrentCellValidated"/> value.
        /// </summary>
        /// <param name="isRowValidated">
        /// Indicates whether the current cell validation is successful.
        /// </param>
        public void SetCurrentCellValidated(bool isCellValidated)
        {
            ActiveGrid = treeGrid;
            IsCurrentCellValidated = isCellValidated;
        }
        #endregion

        #region Methods
        internal void ResetValidations(bool canResetRow)
        {
            ResetValidation(treeGrid, canResetRow);
        }
        private void ResetValidation(SfTreeGrid grid, bool canResetRow)
        {
            var rowColumnIndex = grid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
            var dataRow = grid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow == null) return;
            var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
            if (dataColumn != null && dataColumn.ColumnElement != null)
            {
                SetCurrentCellValidated(true);
                if (!canResetRow) return;
                SetCurrentRowValidated(true);
                dataRow.VisibleColumns.ForEach
                    (item =>
                    {
                        if (!(item.ColumnElement is TreeGridCell)) return;
                        var gridcell = item.ColumnElement as TreeGridCell;
                        if (gridcell.eventErrorMessage != null)
                            gridcell.eventErrorMessage = string.Empty;
                        gridcell.ApplyValidationVisualState();
                    });
            }
        }

        internal bool RaiseRowValidate(RowColumnIndex currentCellIndex)
        {
            if (IsCurrentRowValidated)
                return true;

            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentCellIndex.RowIndex);
            if (dataRow == null) return true;
            errorMessages = new Dictionary<string, string>();
            rowIndex = dataRow.RowIndex;
            var treeNode = treeGrid.View.Nodes.GetNode(dataRow.RowElement.DataContext);
            var validatingArgs = new TreeGridRowValidatingEventArgs(dataRow.RowElement.DataContext, currentCellIndex.RowIndex, errorMessages, treeGrid, treeNode);

            if (!treeGrid.RaiseRowValidatingEvent(validatingArgs))
            {
                dataRow.VisibleColumns.ForEach
                (dataColumnBase =>
                {
                    if (dataColumnBase.TreeGridColumn == null || dataColumnBase.ColumnElement == null)
                        return;

                    var currentCell = dataColumnBase.ColumnElement as TreeGridCell;
                    if (currentCell == null)
                        return;

                    if (validatingArgs.ErrorMessages.ContainsKey(dataColumnBase.TreeGridColumn.MappingName))
                        currentCell.SetError(validatingArgs.ErrorMessages[dataColumnBase.TreeGridColumn.MappingName], false);
                    else
                    {
                        if (currentCell.HasError)
                            currentCell.RemoveError(false);
                    }
                });

                (dataRow.RowElement as TreeGridRowControl).SetError();
                if (!dataRow.IsEditing)
                    treeGrid.SelectionController.CurrentCellManager.BeginEdit();
                SetCurrentCellValidated(false);
                return false;
            }
            else
            {
                RemoveError(dataRow, false);
            }

            var args = new TreeGridRowValidatedEventArgs(validatingArgs.RowData, validatingArgs.RowIndex, validatingArgs.ErrorMessages, treeGrid, treeNode);
            treeGrid.RaiseRowValidatedEvent(args);
            SetCurrentRowValidated(true);

            errorMessages = null;
            rowIndex = -1;
            return true;
        }

        internal void RemoveError(TreeDataRowBase dataRow, bool removeAll)
        {
            if (dataRow == null)
                return;
            foreach (TreeDataColumnBase column in dataRow.VisibleColumns)
            {
                if (column.ColumnElement is TreeGridCell)
                {
                    if (removeAll)
                        (column.ColumnElement as TreeGridCell).RemoveAll();
                    else
                        (column.ColumnElement as TreeGridCell).RemoveError(false);
                }
            }
            (dataRow.RowElement as TreeGridRowControl).RemoveError();
        }

        internal bool RaiseCellValidate(RowColumnIndex currentCellIndex, ITreeGridCellRenderer renderer, bool allowattributeValidation)
        {
            if (IsCurrentCellValidated)
                return true;

            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == currentCellIndex.RowIndex);

            if (dataRow == null)
                return false;
            var columnBase = dataRow.VisibleColumns.FirstOrDefault(x => x.ColumnIndex == currentCellIndex.ColumnIndex);
            if (columnBase == null)
                return false;
            renderer = renderer ?? columnBase.Renderer;

            if (renderer == null)
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

            oldValue = this.treeGrid.SelectionController.CurrentCellManager.oldCellValue;
            var newCellValue = renderer.GetControlValue();
            var treeNode = treeGrid.View.Nodes.GetNode(dataRow.RowData);
            if (this.RaiseCurrentCellValidatingEvent(oldValue, newCellValue, columnBase.TreeGridColumn, out changedNewValue, currentCellIndex, columnBase.ColumnElement as FrameworkElement, out errorMessage, dataRow.RowData, treeNode))
            {
                bool isValid = true;
                if (newCellValue != changedNewValue)
                    renderer.SetControlValue(changedNewValue);
                if (allowattributeValidation)
                {
                    if ((columnBase.ColumnElement is TreeGridCell) && (columnBase.ColumnElement as TreeGridCell).Content != null && (columnBase.ColumnElement as TreeGridCell).Content is FrameworkElement)
                        renderer.UpdateSource((columnBase.ColumnElement as TreeGridCell).Content as FrameworkElement);
                    if (columnBase.TreeGridColumn.GridValidationMode != GridValidationMode.None)
                    {
                        isValid = this.treeGrid.ValidationHelper.ValidateColumn(dataRow.RowData, columnBase.TreeGridColumn.MappingName, (columnBase.ColumnElement as TreeGridCell), currentCellIndex)
                            && TreeGridDataValidation.Validate((columnBase.ColumnElement as TreeGridCell), columnBase.TreeGridColumn.MappingName, columnBase.ColumnElement.DataContext);
                    }
#if WPF
                    if (!isValid && columnBase.TreeGridColumn.GridValidationMode == GridValidationMode.InEdit)
                        return false;                    
#endif
                }

                this.RaiseCurrentCellValidatedEvent(oldValue, columnBase.Renderer.GetControlValue(), columnBase.TreeGridColumn, errorMessage, dataRow.RowData, treeNode);
                SetCurrentCellValidated(true);
                return true;
            }
            return false;
        }

        internal bool RaiseCellValidate(RowColumnIndex currentCellIndex)
        {
            return this.RaiseCellValidate(currentCellIndex, null, true);
        }

        internal bool RaiseCurrentCellValidatingEvent(object oldValue, object newValue, TreeGridColumn column, out object changedNewValue, RowColumnIndex currentCellIndex, FrameworkElement currentCell, out string errorMessage, object rowData, TreeNode node)
        {
            var e = new TreeGridCurrentCellValidatingEventArgs(treeGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                IsValid = true,
                RowData = rowData,
                Node = node
            };
            var isValid = treeGrid.RaiseCurrentCellValidatingEvent(e);
            changedNewValue = e.NewValue;

            var cell = currentCell as TreeGridCell;
            if (!isValid)
                cell.SetError(e.ErrorMessage, false);
            else if (errorMessages == null || !(errorMessages.Keys.Any(x => x == column.MappingName)))
                cell.RemoveError(false);

            errorMessage = e.ErrorMessage;
            return isValid;
        }

        internal void RaiseCurrentCellValidatedEvent(object oldValue, object newValue, TreeGridColumn column, string errorMessage, object rowData, TreeNode treeNode)
        {
            var e = new TreeGridCurrentCellValidatedEventArgs(treeGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                ErrorMessage = errorMessage,
                RowData = rowData,
                Node = treeNode
            };
            treeGrid.RaiseCurrentCellValidatedEvent(e);
        }

        internal bool ValidateColumn(object rowData, string columnName, TreeGridCell currentCell, RowColumnIndex currentCellIndex)
        {
            var propertyName = columnName;
            bool isValid = true;
            var errorMessage = string.Empty;
            bool isAttributeError = false;
            if (rowData == null || string.IsNullOrEmpty(columnName) || currentCell == null || currentCellIndex == RowColumnIndex.Empty)
                return isValid;
#if UWP
            PropertyInfoCollection itemProperties;
#else
            PropertyDescriptorCollection itemProperties;
#endif
            if (this.treeGrid.HasView)
            {
                itemProperties = this.treeGrid.View.GetItemProperties();
                if (itemProperties == null)
                    return isValid;
            }
            else
            {
#if WPF
                itemProperties = TypeDescriptor.GetProperties(rowData.GetType());
#else
                itemProperties = new PropertyInfoCollection(rowData.GetType());
#endif
            }

            if (columnName.Contains('.'))
            {
                var propNames = columnName.Split('.');
                columnName = propNames[propNames.Length - 1];
                Array.Resize(ref propNames, propNames.Length - 1);
                var pName = string.Join(".", propNames);
#if WPF
                rowData = PropertyDescriptorExtensions.GetValue(itemProperties, rowData, pName);
#else
                rowData = Syncfusion.Data.PropertyInfoExtensions.GetValue(itemProperties, rowData, pName);
#endif
            }

            PropertyInfo propertyinfo = null;
            ValidationContext validationContext = null;
#if WPF
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
            if ((this.treeGrid.Columns[propertyName] as TreeGridColumn).GridValidationMode != GridValidationMode.None)
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
                    catch (Exception e)
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
                    currentCell.SetError(errorMessage, isAttributeError);
                else
                    currentCell.RemoveError(true);
            }
#if !SyncfusionFramework4_0 || UWP
            if (rowData is INotifyDataErrorInfo)
                isValid = isValid && TreeGridDataValidation.ValidateINotifyDataErrorInfo(currentCell, columnName, rowData);
#endif
            return isValid;
        }

        internal void ValidateColumns(TreeDataRowBase dr)
        {
            foreach (var column in dr.VisibleColumns)
                if (column.TreeGridColumn != null)
                    this.ValidateColumn(dr.RowData, column.TreeGridColumn.MappingName, column.ColumnElement as TreeGridCell, new RowColumnIndex(column.RowIndex, column.ColumnIndex));
        }

        /// <summary>
        /// Check validation by raising cell validating and row validating events.
        /// </summary>
        /// <param name="canRaiseEvent">if it is true, cell validating and row validating events will be raised. Else, IsCurrentCellValidated and IsCurrentRowValidated only will be checked.</param>
        /// <returns>returns true if validation succeeded, else false</returns>
        internal bool CheckForValidation(bool canRaiseEvent)
        {
            if (!canRaiseEvent)
                return IsCurrentCellValidated && IsCurrentRowValidated;

            if (!IsCurrentCellValidated || !IsCurrentRowValidated)
                if (this.RaiseCellValidate(treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex))
                    return this.RaiseRowValidate(treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex);
                else
                    return false;
            return true;
        }


        #endregion

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridValidationHelper"/> class.
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
                treeGrid = null;
            }
            isdisposed = true;
        }
    }
}
