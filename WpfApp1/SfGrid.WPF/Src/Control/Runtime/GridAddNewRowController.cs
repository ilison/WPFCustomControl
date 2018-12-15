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
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Collections;
using System.ComponentModel;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that implements AddNewRow operations in SfDataGrid .
    /// </summary>
    public class GridAddNewRowController: IDisposable
    {
        #region Fields

        SfDataGrid dataGrid;
        private bool isdisposed = false;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> class.
        /// </summary>
        /// <param name="grid">
        /// The SfDataGrid.
        /// </param>
        public GridAddNewRowController(SfDataGrid grid)
        {
            dataGrid = grid;
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Invoked when the new item is initiated to add into source collection.        
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the new item has initiated in AddNewRow; otherwise <b>false</b>.
        /// </returns>        
        /// <remarks>
        /// AddNew invoked when the cell in AddNewRow goes edit mode .
        /// </remarks>        
        public virtual bool AddNew()
        {
            if (dataGrid.View == null || !dataGrid.View.CanAddNew)
                return false;
            if (dataGrid.View.IsAddingNew)
                throw new InvalidOperationException("CurrentAddItem should be null when AddNew calls");
            if (dataGrid.AddNewRowPosition != AddNewRowPosition.None)
            {
                dataGrid.View.AddNew();
                var addNewRowEventArgs = new AddNewRowInitiatingEventArgs(this.dataGrid) { NewObject = dataGrid.View.CurrentAddItem };
                var addNewObject = dataGrid.RaiseAddNewRowInitiatingEvent(addNewRowEventArgs);
                // WPF-19376 - when dynamic itemssource is used, need to set CurrentAddItem based on the addNewObject 
                if (dataGrid.View is CollectionViewAdv)
                {   
                    if ((dataGrid.View as CollectionViewAdv).CurrentAddItem != addNewObject)
                         (dataGrid.View as CollectionViewAdv).CurrentAddItem = addNewObject;                        
                }
                var addNewRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
                if (addNewRow != null)
                {
                    VisualStateManager.GoToState(addNewRow.WholeRowElement, "Edit", true);
                    addNewRow.RowData = addNewObject;
                }
            }
            dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.AddNewRow, new AddNewRowOperationHandlerArgs(AddNewRowOperation.AddNew,null)));
            return true;
        }

        /// <summary>
        /// Invoked when the new item is canceled before adding it to source collection.        
        /// </summary>
        /// <remarks>
        /// CancelAddNew invoked when Esc key is pressed twice in AddNewRow.
        /// </remarks>
        public virtual void CancelAddNew()
        {
            dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.AddNewRow, new AddNewRowOperationHandlerArgs(AddNewRowOperation.CancelNew, null)));
            if (this.dataGrid.View != null)
                this.dataGrid.View.CancelNew();
            //WPF-35311 - Resets the validation error message by calling ResetAddNewRow method itself.
            ResetAddNewRow(false);
        }


        /// <summary>
        /// Commits the new item into the source collection.
        /// </summary>
        /// <param name="changeState">
        /// Indicates whether the AddNewRow changed to normal state.
        /// </param>      
        public virtual void CommitAddNew(bool changeState = true)
        {
            dataGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.AddNewRow, new AddNewRowOperationHandlerArgs(AddNewRowOperation.CommitNew, null)));
            if (this.dataGrid.View != null && this.dataGrid.View.IsAddingNew)
                this.dataGrid.View.CommitNew();
            if (this.dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                dataGrid.ScrollInView(new RowColumnIndex(GetAddNewRowIndex(), this.dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));

            ResetAddNewRow(changeState);

        }

        /// <summary>
        /// Method which helps to Set the mode for AddNewRow water mark.
        /// </summary>
        /// <param name="inEdit"></param>
        internal void SetAddNewMode(bool inEdit)
        {
            var addNewRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
            if (addNewRow != null)
            {
                if (inEdit)
                     (addNewRow.WholeRowElement as AddNewRowControl).SetEditMode();

                else
                {
                    (addNewRow.WholeRowElement as AddNewRowControl).RemoveEditMode();
                    (addNewRow.WholeRowElement as AddNewRowControl).UpdateTextBorder();
                }
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Method which helps to reset the AddNewRow after commiting or canceling.
        /// </summary>
        /// <param name="changeState"> The bool variable used to set the state of AddNewRow controller</param>
        private void ResetAddNewRow(bool changeState)
        {
            var addNewRow = this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
            if (addNewRow != null)
            {
                addNewRow.RowData = this.dataGrid.View.CurrentAddItem;
                (addNewRow.WholeRowElement as AddNewRowControl).UpdateTextBorder();
                (addNewRow as GridDataRow).ApplyRowHeaderVisualState();
                this.dataGrid.ValidationHelper.RemoveError(addNewRow, true);
                if (changeState)
                    VisualStateManager.GoToState(addNewRow.WholeRowElement, "Normal", true);
            }
        }

        /// <summary>
        /// Gets the row index of the AddNewRow.
        /// </summary>
        /// <returns>
        /// Returns row index of the AddNewRow.
        /// </returns>
        public int GetAddNewRowIndex()
        {
            if (dataGrid.AddNewRowPosition == AddNewRowPosition.None)
                return -1;

            var footerCount = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true) 
                + (dataGrid.FilterRowPosition == FilterRowPosition.Bottom ? 1 : 0);//, RowRegion.Footer);
            
            if (dataGrid.AddNewRowPosition == AddNewRowPosition.FixedTop)
                return dataGrid.HeaderLineCount - 1;
            else if (dataGrid.AddNewRowPosition == AddNewRowPosition.Top)
                return dataGrid.HeaderLineCount + (dataGrid.FilterRowPosition == FilterRowPosition.Top ? 1 : 0);
            else
                return dataGrid.VisualContainer.RowCount - (dataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + 1 + footerCount);
        }

        #endregion
        /// <summary>
        /// Release all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridAddNewRowController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
                this.dataGrid = null;
            isdisposed = true;
        }
    }
 
    /// <summary>
    /// Provides data for handling AddNewRow operation in SfDataGrid.
    /// </summary>
    public class AddNewRowOperationHandlerArgs
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowOperationHandlerArgs"/> class.
        /// </summary>
        /// <param name="operation">
        /// The corresponding operation related to AddNewRow.
        /// </param>
        /// <param name="operationArgs">
        /// The corresponding AddNewRow operation argument to handle.
        /// </param>
        public AddNewRowOperationHandlerArgs(AddNewRowOperation operation, object operationArgs)
        {
            AddNewRowOperation = operation;
            OperationArgs = operationArgs;
        }

        #endregion

        /// <summary>
        /// Gets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowOperation"/> that was performed.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowOperation"/> .
        /// </value>
        public AddNewRowOperation AddNewRowOperation { get; private set; }

        /// <summary>
        /// Gets the event argument related to the AddNewRow operation.
        /// </summary>
        public object OperationArgs { get; private set; }
    }

    /// <summary>
    /// Defines the constants that specify the possible operation in AddNewRow.
    /// </summary>
    public enum AddNewRowOperation
    {
        /// <summary>
        /// The new item is initiated in AddNewRow.
        /// </summary>
        AddNew,

        /// <summary>
        /// The new item is canceled before committing.
        /// </summary>
        CancelNew,

        /// <summary>
        /// The new item is committed into the underlying source collection.
        /// </summary>
        CommitNew,

        /// <summary>
        /// The AddNewRow position is changed at runtime.
        /// </summary>
        PlacementChange
    }
}
