#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if WinRT || UNIVERSAL
using System.Globalization;
using Windows.Devices.Input;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Syncfusion.UI.Xaml.Utility;
#else
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using Syncfusion.Windows.Shared;
using System.ComponentModel.DataAnnotations;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
#else
    using System.Windows.Threading;
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif
    /// <summary>
    /// Provides the base implementation for selection in SfDataGrid.
    /// </summary>
    public abstract class GridBaseSelectionController : IGridSelectionController, IDisposable
    {
        #region Fields
        internal bool isSuspended;
        internal SfDataGrid dataGrid;
        GridSelectedRowsCollection selectedRows;
        GridSelectedCellsCollection selectedCells;
        Brush rowHoverBackgroundBrush;
        private Brush rowSelectionBrush;
        private Brush groupRowSelectionBrush;
        GridCurrentCellManager currentCellManager;
        
        //Behaviour: To hold Selection Changed Event that is raised from SourceCollectionChanged,
        //When deleting Multiple Rows
        internal bool cancelSelectionChangedEvent=false;
        internal bool isSourceCollectionChanged = false;
        private bool isdisposed = false;

        //Behaviour: To cancel the drag selection whiel dragging from Indent cell or UnFocus column
        internal bool cancelDragSelection = false;
        //Behaviour: In Silverlight there is not support for getting Mouse pressed state, hence this property is used.
        internal bool isMousePressed = false;
        //Behaviour: Whether the VisualContainer has been captured or not while dragging.
        internal bool isMouseCaptured = false;

        /// <summary>
        /// Gets or sets the last pressed key.
        /// </summary>
        protected Key lastPressedKey = Key.None;

        /// <summary>
        /// Gets or sets the currently pressed key.
        /// </summary>
        protected Key currentKey = Key.None;

        internal Point pressedPosition;
#if UWP
        
        // Gets or sets the vertical offset of current pointer pressed location.
        internal double pressedVerticalOffset;
#endif
        private RowColumnIndex _pressedRowColumnIndex;
        /// <summary>
        /// Gets or sets the pressed RowColumnIndex of SfDataGrid.
        /// </summary>
        protected internal RowColumnIndex PressedRowColumnIndex
        {
            get { return _pressedRowColumnIndex; }
            set { _pressedRowColumnIndex = value; }
        }

        #endregion

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.PressedRowColumnIndex"/> of SfDataGird.
        /// </summary>
        /// <param name="rowcolumnIndex">
        /// Indicates the starting position of drag selection.
        /// </param>
        /// <remarks>
        /// This method helps to initialize the starting position of drag selection.
        /// </remarks>
        protected virtual void SetPressedIndex(RowColumnIndex rowcolumnIndex)
        {
            this.PressedRowColumnIndex = rowcolumnIndex;
        }

        #region IGridSelectionController 

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> that contains the information of selected rows.        
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/>.
        /// </value>
        public GridSelectedRowsCollection SelectedRows
        {
            get { return selectedRows; }
            protected set { selectedRows = value; }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsInfo"/> that contains the information of selected cells.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsInfo"/>. 
        /// </value>
        public GridSelectedCellsCollection SelectedCells
        {
            get { return selectedCells; }
            protected set { selectedCells = value; }
        }

        /// <summary>
        /// Gets or sets a brush that highlights the background of the currently selected row or cell.
        /// </summary>
        /// <value>
        /// The brush that highlights the background of the selected row or cell.
        /// </value>
        [Obsolete]
        public Brush RowSelectionBrush
        {
            get { return rowSelectionBrush; }
            set
            {
                rowSelectionBrush = value;
                OnPropertyChanged("RowSelectionBrush");
            }
        }

        /// <summary>
        /// Gets or sets a brush that highlights the background of currently selected group caption and group summary rows.
        /// </summary>    
        /// <value>
        /// The brush that highlights the background of currently selected group row.
        /// </value>
        [Obsolete]
        public Brush GroupRowSelectionBrush
        {
            get { return groupRowSelectionBrush; }
            set
            {
                groupRowSelectionBrush = value;
                OnPropertyChanged("GroupRowSelectionBrush");
            }
        }

        /// <summary>
        /// Gets or sets a brush that highlights the background of data row is being hovered. 
        /// </summary>
        /// <value>
        /// The brush that highlights the data row is being hovered.
        /// </value>
        [Obsolete]
        public Brush RowHoverBackgroundBrush
        {
            get { return rowHoverBackgroundBrush; }
            set
            {
                rowHoverBackgroundBrush = value;
                OnPropertyChanged("RowHoverBackgroundBrush");
            }
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellManager"/> instance which holds the current cell information.
        /// </summary>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellManager"/> creates the current cell and manages the current cell related operations.
        /// </remarks>
        public GridCurrentCellManager CurrentCellManager
        {
            get { return currentCellManager; }
            protected set { currentCellManager = value; }
        }

        /// <summary>
        /// Selects the rows corresponding to the specified start and end index of the row.
        /// </summary>
        /// <param name="startRowIndex">
        /// The start index of the row to select.
        /// </param>
        /// <param name="endRowIndex">
        /// The end index of the row to select.
        /// </param>
        public abstract void SelectRows(int startRowIndex, int endRowIndex);

        /// <summary>
        /// Selects all the cells in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public abstract void SelectAll();

        /// <summary>
        /// Clears the selected cells or rows in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current selection can be cleared or not.
        /// </param>        
        public abstract void ClearSelections(bool exceptCurrentRow);

        /// <summary>
        /// Moves the current cell to the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current selection is cleared while moving the current cell.
        /// </param>     
        /// <remarks>
        /// This method is not applicable when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> is Row.
        /// </remarks>     
        public abstract void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true);

        /// <summary>
        /// Commits the new item initialized on the AddNewRow to the underlying data source.
        /// </summary>
        /// <param name="changeState">
        /// Indicates whether watermark is enabled on the AddNewRow.
        /// </param>
        protected internal abstract void CommitAddNew(bool changeState = true);

        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in SfDataGrid; otherwise, <b>false</b>.
        /// </returns>
        public abstract bool HandleKeyDown(KeyEventArgs args);

        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed in DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in DetailsViewDataGrid; otherwise, <b>false</b>.
        /// </returns>
        public abstract bool HandleDetailsViewGridKeyDown(KeyEventArgs args);

        /// <summary>
        /// Handles selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.GridOperation"/> such as sorting, filtering, grouping and etc performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridOperationsHandlerArgs"/> that contains the type of grid operations and its arguments.
        /// </param>
        /// <remarks>
        /// This method is called when any of the <see cref="Syncfusion.UI.Xaml.Grid.GridOperation"/> are performed in SfDataGrid.
        /// </remarks>
        public virtual void HandleGridOperations(GridOperationsHandlerArgs args)
        {
            switch (args.Operation)
            {
                case GridOperation.Sorting:
                    ProcessOnSortChanged(args.OperationArgs as SortColumnChangedHandle);
                    break;
                case GridOperation.Filtering:
                    ProcessOnFilterApplied(args.OperationArgs as GridFilteringEventArgs);
                    break;
                case GridOperation.FilterPopupOpening:
                    ProcessOnFilterPopupOpened();
                    break;
                case GridOperation.Paging:
                    ProcessOnPageChanged();
                    break;
                case GridOperation.Grouping:
                    ProcessOnGroupChanged(args.OperationArgs as GridGroupingEventArgs);
                    break;
                case GridOperation.TableSummary:
                    ProcessTableSummaryChanged(args.OperationArgs as TableSummaryPositionChangedEventArgs);
                    break;
                case GridOperation.RowHeaderChanged:
                    ProcessOnRowHeaderChanged();
                    break;
                case GridOperation.FilterRow:
                    ProcessOnFilterRowPositionChanged((DependencyPropertyChangedEventArgs)args.OperationArgs);
                    break;
                case GridOperation.Paste:
                    ProcessOnPaste(args.OperationArgs as List<object>);
                    break;
                case GridOperation.AddNewRow:
                    ProcessOnAddNewRow((AddNewRowOperationHandlerArgs)args.OperationArgs);
                    break;
                case GridOperation.UnBoundDataRow:
                    ProcessUnBoundRowChanged(args.OperationArgs as UnBoundDataRowCollectionChangedEventArgs);
                    break;
                case GridOperation.StackedHeaderRow:
                    ProcessOnStackedHeaderRows(args.OperationArgs as StackedHeaderCollectionChangedEventArgs);
                    break;
            }
        }

        /// <summary>
        /// Handles selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridPointerEventArgs"/> that contains the type of pointer operations and its arguments.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the cell.
        /// </param>
        /// <remarks>
        /// This method is invoked when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> are performed in SfDataGrid.
        /// </remarks>
        public abstract void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex);

        /// <summary>
        /// Handles selection when the selection property such as SelectedIndex,SelectedItem and SelectionMode value changes occurred.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> that contains the corresponding property name and its value changes.
        /// </param>
        /// <remarks>
        /// This method is invoked when the selection property values such as SelectedIndex,SelectedItem and SelectionMode are changed in SfDataGrid.
        /// </remarks>
        public virtual void HandleSelectionPropertyChanges(SelectionPropertyChangedHandlerArgs handle)
        {
            switch (handle.PropertyName)
            {
                case "SelectedItem":
                    ProcessSelectedItemChanged(handle);
                    break;
                case "SelectedIndex":
                    ProcessSelectedIndexChanged(handle);
                    break;
                case "SelectionMode":
                    ProcessSelectionModeChanged(handle);
                    break;
                case "NavigationMode":
                    ProcessNavigationModeChanged(handle);
                    break;
                case "CurrentItem":
                    ProcessCurrentItemChanged(handle);
                    break;
            }
        }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
        /// </param>
        protected internal virtual void ProcessSelectedItemChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex"/> property value changes.
        /// </param>
        protected internal virtual void ProcessSelectedIndexChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> property changes.
        /// </param>
        protected virtual void ProcessSelectionModeChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> property value changes.
        /// </param>
        protected virtual void ProcessNavigationModeChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </param>
        protected internal virtual void ProcessCurrentItemChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Handles the selection when the SelectedItems, Columns and DataSource property collection changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for collection changes.
        /// </param>
        /// <param name="reason">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.CollectionChangedReason"/> contains reason for the collection changes.
        /// </param>
        /// <remarks>
        /// This method is called when the collection changes on SelectedItems, Columns and DataSource properties in SfDataGrid.
        /// </remarks>
        public abstract void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, CollectionChangedReason reason);

        /// <summary>
        /// Handles the selection when the group is expanded or collapsed in SfDataGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the group.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Specifies whether the group is expanded or not.
        /// </param>
        /// <remarks>
        /// This method is invoked when the group is expanded or collapsed.
        /// </remarks>
        public abstract void HandleGroupExpandCollapse(int index, int count, bool isExpanded);

        /// <summary>
        /// Occurs when a property value changes in <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invoked when the property value changes in <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController"/> class.
        /// </summary>
        /// <param name="propertyName">
        /// The property name.
        /// </param>
        internal protected virtual void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of <see cref="GridBaseSelectionController"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding <see cref="SfDataGrid"/> instance.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GridBaseSelectionController(SfDataGrid dataGrid)
        {
            this.dataGrid = dataGrid;
            SelectedRows = new GridSelectedRowsCollection();
            SelectedCells = new GridSelectedCellsCollection();
            CurrentCellManager = CreateCurrentCellManager();
            this.WireEvents();
            if(this.dataGrid.VisualContainer != null)
            {
                UnWireVisualContainerEvents();
                WireVisualContainerEvents();
            }
        }
        
        /// <summary>
        /// Gets the current DetailsViewDataGrid that contains the current cell.
        /// </summary>
        /// <param name="parentGrid">
        /// Indicates the corresponding parent grid.
        /// </param>        
        /// <returns>
        /// Returns the corresponding DetailsViewDataGrid that contains the current cell.
        /// </returns>
        public SfDataGrid GetCurrentDetailsViewGrid(SfDataGrid parentGrid)
        {
            if (this.dataGrid.SelectionMode == GridSelectionMode.Multiple)
            {
                var detailsViewGrid = parentGrid.GetDetailsViewGrid(parentGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                if (detailsViewGrid != null && detailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex > -1)
                    return detailsViewGrid;
                else
                    return null;
            }
            else
                return parentGrid.SelectedDetailsViewGrid;           
        }

        /// <summary>
        /// Creates the current cell and manages the current cell related operations in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellManager"/> instance.
        /// </returns>
        protected virtual GridCurrentCellManager CreateCurrentCellManager()
        {
            var _currentCellManager = new GridCurrentCellManager(this.dataGrid)
                {
                    SuspendUpdates = SuspendUpdates,
                    ResumeUpdates = ResumeUpdates
                };
            return _currentCellManager;
        }

        #endregion

        #region Grid Operations Methods
        /// <summary>
        /// Processes the selection when the column is sorted in SfDataGrid.
        /// </summary>
        /// <param name="sortcolumnHandle">
        /// Contains information related to the sorting action.
        /// </param>        
        /// <remarks>
        /// This method refreshes the selection while sorting the column in SfDataGrid and clear the selection in summary rows.
        /// </remarks>
        protected abstract void ProcessOnSortChanged(SortColumnChangedHandle sortcolumnHandle);

        /// <summary>
        /// Processes the selection when the column is grouped in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for collection changes during grouping action.
        /// </param>
        /// <remarks>
        /// This method refreshes the selection while grouping the column in SfDataGrid.
        /// </remarks>
        protected abstract void ProcessOnGroupChanged(GridGroupingEventArgs args);

        /// <summary>
        /// Processes the selection when the filter popup is opened.
        /// </summary>
        /// <remarks>
        /// This method refreshes the selection while opening the filter popup in SfDataGrid.
        /// </remarks>
        protected abstract void ProcessOnFilterPopupOpened();

        /// <summary>
        /// Processes the selection when the filtering is applied in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to filtering operation.
        /// </param>
        protected abstract void ProcessOnFilterApplied(GridFilteringEventArgs args);

        /// <summary>
        /// Processes the selection while navigating from one page in to another page in SfDataPager.
        /// </summary>
        protected abstract void ProcessOnPageChanged();

        /// <summary>
        /// Processes the selection when the table summary row position is changed.
        /// </summary>
        /// <param name="args">
        /// Contains the data for the table summary row position changes.
        /// </param>
        protected abstract void ProcessTableSummaryChanged(TableSummaryPositionChangedEventArgs args);

        /// <summary>
        /// Processes the selection when the UnBoundRow collection changes.
        /// </summary>
        /// <param name="args">
        /// Contains data for the UnBoundRow collection changes.
        /// </param>
        protected abstract void ProcessUnBoundRowChanged(UnBoundDataRowCollectionChangedEventArgs args);

        /// <summary>
        /// Processes the selection when the position of FilterRow is changed.
        /// </summary>
        /// <param name="args">
        /// Contains data for the FilterRow position changed.
        /// </param>
        protected abstract void ProcessOnFilterRowPositionChanged(DependencyPropertyChangedEventArgs args);

        /// <summary>
        /// Processes the selection when new row is initiated or committed and the position of AddNewRow is changed.
        /// </summary>
        /// <param name="handle">
        /// Contains data for the AddNewRow operation.
        /// </param>
        protected abstract void ProcessOnAddNewRow(AddNewRowOperationHandlerArgs handle);

        /// <summary>
        /// Processes the selection when the records is pasted on SfDataGrid.
        /// </summary>
        /// <param name="records">
        /// Contains the list of records that is pasted in to SfDataGrid.
        /// </param>
        protected abstract void ProcessOnPaste(List<object> records);

        /// <summary>
        /// Processes the selection when the stacked header collection changes in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data for stacked header collection changes.
        /// </param>
        protected abstract void ProcessOnStackedHeaderRows(StackedHeaderCollectionChangedEventArgs args);

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowRowHeader"/> property value changes.
        /// </summary>
        public virtual void ProcessOnRowHeaderChanged()
        {
            //WPF-23996(Issue 1) : If we didn't maintain any currentcell but we have enabling and disabling RowHeader means the 
            //ColumnIndex is updated based ShowRowHeader so update the ColumnIndex when ColumnIndex is greater than 0.
            if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex >= 0)
            {
                if (this.DataGrid.showRowHeader)
                    this.CurrentCellManager.SetCurrentColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex + 1);
                else
                    this.CurrentCellManager.SetCurrentColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex - 1);
            }
        }
        
        #endregion

        #region Helper Methods

        /// <summary>
        /// Method which helps to set the SelectedDetailsViewGrid and ProcessParentGridSelection when we call SelectAll method.
        /// </summary>
        internal bool ProcessParentGridSelection()
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowcolIndex = new RowColumnIndex(detailsViewDataRow.RowIndex, colIndex);
            if (this.DataGrid.SelectionUnit == GridSelectionUnit.Row)
            {
                (parentDataGrid.SelectionController as GridSelectionController).ProcessPointerSelection(null, rowcolIndex);
                parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
                if (parentDataGrid.NotifyListener != null)
                    return (parentDataGrid.SelectionController as GridSelectionController).ProcessParentGridSelection();
            }
            else
            {
                (parentDataGrid.SelectionController as GridCellSelectionController).ProcessPointerSelection(null, rowcolIndex, false);
                parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
                if (parentDataGrid.NotifyListener != null)
                    return (parentDataGrid.SelectionController as GridCellSelectionController).ProcessParentGridSelection();
            }
            if (!ValidationHelper.IsCurrentCellValidated || !ValidationHelper.IsCurrentRowValidated)
                return false;
            return true;
        }

        /// <summary>
        /// Determines whether the auto scroller is enabled when the drag selection reaches the out of visible boundaries in SfDataGrid.
        /// </summary>
        /// <param name="point">
        /// Represents the corresponding position of mouse to decide whether the selection is dragged out of view in SfDataGrid.
        /// </param>
        /// <param name="args">
        /// Contains the data for mouse related events.
        /// </param>
        /// <returns>
        /// <b>true</b> if the mouse point reaches the out of visible boundaries; Returns <b>false</b> , if the mouse point inside the visible boundary or the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> is single or none . 
        /// </returns>
        protected bool CanStartAutoScroller(Point point, MouseEventArgs args)
        {         
            if (this.DataGrid.SelectionMode == GridSelectionMode.Single || this.DataGrid.SelectionMode == GridSelectionMode.None || !this.isMousePressed)
                return false;
            // WPF - 37358 - AutoScrolling is skipped while resize the SfMultiColumnDropDownControl Popup
            if (this.PressedRowColumnIndex == this.currentCellManager.CurrentRowColumnIndex && this.currentCellManager.HasCurrentCell && this.currentCellManager.CurrentCell.IsEditing)
                return false;

#if WPF
                var pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            //Point is calculated based on VisualContainer like WPF.
            Point pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.dataGrid.VisualContainer);
#endif

            var xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            var yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);

            return (xPosChange > 10 || yPosChange > 10) && this.DataGrid.AutoScroller.AutoScrolling != AutoScrollOrientation.Both
                && !this.DataGrid.AutoScroller.InsideScrollBounds.Contains(point) && this.DataGrid.SelectedDetailsViewGrid == null && this.isMousePressed;
        }

        internal bool CheckFreezePanes()
        {
            return this.DataGrid.FrozenRowsCount > 0 || this.DataGrid.FooterRowsCount > 0 || this.DataGrid.FrozenColumnCount > 0 || this.DataGrid.FooterColumnCount > 0;                 
        }

        #region Scrolling Helper


        /// <summary>
        /// Scrolls the specified row index to view in SfDataGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to scroll the row.
        /// </param>       
        /// <remarks>
        /// This method helps to scroll the row into view when the row is not present in the view area of SfDataGrid. 
        /// If the rowIndex is DetailsViewDataRow, the current row index of the particular DetailsViewDataGrid will 
        /// be scrolled to view.
        /// </remarks>
        protected void ScrollToRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dataGrid.VisualContainer.ScrollRows.LineCount)
                return;
            var visibleLines = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            int firstBodyVisibleIndex = -1;
            var SelectionController = this.DataGrid.SelectionController as GridBaseSelectionController;
            VisibleLineInfo lineInfo = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowIndex);
            if (lineInfo == null)
            {
                if (visibleLines.FirstBodyVisibleIndex < visibleLines.Count)
                    firstBodyVisibleIndex = visibleLines[visibleLines.FirstBodyVisibleIndex].LineIndex;
            }
            var isInOutOfView = rowIndex > this.DataGrid.VisualContainer.ScrollRows.LastBodyVisibleLineIndex + 1 || (firstBodyVisibleIndex >= 0 && rowIndex < firstBodyVisibleIndex - 1);

            if (visibleLines.FirstBodyVisibleIndex < visibleLines.Count)
                firstBodyVisibleIndex = visibleLines[visibleLines.FirstBodyVisibleIndex].LineIndex;

            if (this.DataGrid.GridModel.HasGroup && this.DataGrid.AllowFrozenGroupHeaders && rowIndex != this.DataGrid.GetFirstNavigatingRowIndex() && rowIndex <= firstBodyVisibleIndex + this.DataGrid.View.GroupDescriptions.Count)
            {
                var delta = 0.0;
                //WPF - 18503 - When we naviagte the DataGrid using Up key When the FrozenGroupHeader is true while the DataRowisClipped,
                //that DataRow is not comes to view, because based on FirstBodyVisibleIndex we have scroll(clipped row is consider as not clipped) so calculate the delta for FrozenGroupHeaders.
                var row = DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
                var rowSize = DataGrid.VisualContainer.ScrollRows.GetLineSize(rowIndex);
                if (row != null && row.WholeRowElement.Clip != null && rowIndex > firstBodyVisibleIndex)
                {
                    delta += row.WholeRowElement.Clip.Bounds.Top; 
                    if(delta>0)
                    {
                        this.dataGrid.VisualContainer.VScrollBar.Value -= delta;
                        this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                    }                                                                    
                    return;
                }
                else 
                {
                    delta = this.DataGrid.VisualContainer.ScrollRows.Distances.GetCumulatedDistanceAt(rowIndex);
                    this.dataGrid.VisualContainer.VScrollBar.Value = delta - (DataGrid.View.GroupDescriptions.Count * rowSize);
                    this.dataGrid.VisualContainer.InvalidateMeasureInfo();
                    return;
                }
            }
            else if (isInOutOfView || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                var detailsViewGrid = SelectionController.GetCurrentDetailsViewGrid(this.DataGrid);
                if (this.DataGrid.IsInDetailsViewIndex(rowIndex) && detailsViewGrid != null && !(this.DataGrid is DetailsViewDataGrid))
                {
                    var delta = this.DataGrid.VisualContainer.ScrollRows.Distances.GetCumulatedDistanceAt(rowIndex);
                    int count = 0;
                    var selectedGrid = detailsViewGrid;
                    double headerExtent = this.DataGrid.VisualContainer.ScrollRows.HeaderExtent;
                    double footerExtent = 0;
                    while (selectedGrid != null)
                    {
                        detailsViewGrid = selectedGrid;
                        delta += detailsViewGrid.VisualContainer.ScrollRows.Distances.GetCumulatedDistanceAt(detailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        headerExtent += detailsViewGrid.VisualContainer.ScrollRows.HeaderExtent;
                        footerExtent += detailsViewGrid.VisualContainer.ScrollRows.FooterExtent;
                        selectedGrid = SelectionController.GetCurrentDetailsViewGrid(detailsViewGrid);
                        count++;
                    }
                    double topPaddingValue = 0;
                    double bottomPaddingValue = 0;
                    if (count > 0)
                    {
                        topPaddingValue += (this.DataGrid.DetailsViewPadding.Top + detailsViewGrid.BorderThickness.Top) * count;
                        bottomPaddingValue += (this.DataGrid.DetailsViewPadding.Bottom + 1) * count;
                    }
                    delta += topPaddingValue + bottomPaddingValue + detailsViewGrid.VisualContainer.ScrollRows.GetLineSize(detailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                    if ((delta - (topPaddingValue + headerExtent)) > this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value)
                    {
                        delta = (delta + footerExtent) - this.DataGrid.VisualContainer.ScrollRows.ScrollBar.LargeChange - this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value;
                        if (delta > 0)
                            this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value += delta;
                    }
                    else
                    {
                        var selectedLine = detailsViewGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(detailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        if (selectedLine != null && selectedLine.IsHeader)
                            return;
                        delta -= (topPaddingValue + bottomPaddingValue + headerExtent);
                        this.DataGrid.VisualContainer.ScrollRows.ScrollBar.Value = delta;
                    }
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    return;
                }

                if (!(this.DataGrid is DetailsViewDataGrid))
                {
                    this.DataGrid.VisualContainer.ScrollRows.ScrollInView(rowIndex);
                    this.DataGrid.VisualContainer.InvalidateMeasureInfo();
                    if (isInOutOfView && this.DataGrid.SelectedDetailsViewGrid != null)
                        this.ScrollToRowIndex(rowIndex);
                }
            }
        }

        /// <summary>
        /// Scrolls the specified row index from the bottom to top direction of view in SfDataGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to scroll the row.
        /// </param>
        /// <param name="pressedKey">
        /// The corresponding key that was pressed.
        /// </param>        
        /// <remarks>
        /// This method helps to scroll the row into view when the row is not present in the view area of SfDataGrid.
        /// </remarks>
        [Obsolete("This method is marked as Obsolete, use ScrollToRowIndex method instead")]
        protected void ScrollInViewFromBottom(int rowIndex, Key pressedKey)
        {
            this.ScrollToRowIndex(rowIndex);
        }

        /// <summary>
        /// Scrolls the specified row index from top to bottom direction of view in SfDataGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to scroll the row
        /// </param>
        /// <param name="pressedKey">
        /// The corresponding key that was pressed.
        /// </param>
        /// <remarks>
        /// This method helps to scroll the row into view when the row is not present in the view area of SfDataGrid.
        /// </remarks>
        [Obsolete("This method is marked as Obsolete, use ScrollToRowIndex method instead")]
        protected void ScrollInViewFromTop(int rowIndex, Key pressedKey)
        {
            this.ScrollToRowIndex(rowIndex);
        }
        
        /// <summary>
        /// Scroll the ParentDataGrid when DetailsViewDataRow is clipped while process on Tapped.
        /// </summary>
        /// <param name="rowColumnIndex"></param>
        internal void DetailsViewScrollinView(RowColumnIndex rowColumnIndex)
        {
            var visibleRowLines = DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
            //var rowLineInfo = DataGrid.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowColumnIndex.RowIndex);
            if (visibleRowLines.FirstBodyVisibleIndex < visibleRowLines.Count)
                ParentGridScrollInView();
            var visibleColumnLines = DataGrid.VisualContainer.ScrollColumns.GetVisibleLines();
            //var columnLineInfo = DataGrid.VisualContainer.ScrollColumns.GetVisibleLineAtLineIndex(rowColumnIndex.ColumnIndex);

            if (visibleColumnLines.FirstBodyVisibleIndex < visibleColumnLines.Count)
            {
                if (rowColumnIndex.ColumnIndex <= visibleColumnLines[visibleColumnLines.FirstBodyVisibleIndex].LineIndex)
                {
                    DataGrid.SelectionController.CurrentCellManager.ScrollInViewFromLeft(rowColumnIndex.ColumnIndex);
                    return;
                }
                else if (rowColumnIndex.ColumnIndex >= visibleColumnLines[visibleColumnLines.LastBodyVisibleIndex].LineIndex)
                {
                    DataGrid.SelectionController.CurrentCellManager.ScrollInViewFromRight(rowColumnIndex.ColumnIndex);
                    return;
                }
            }
        }
        
        internal void ParentGridScrollInView()
        {            
            var selectionController= (GridBaseSelectionController)this.DataGrid.GetTopLevelParentDataGrid().SelectionController;
            selectionController.ScrollToRowIndex(selectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
        }
        #endregion

        /// <summary>
        /// Gets or sets a value that indicates whether the drag selection is disabled or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if the drag selection is disabled in SfDataGrid; otherwise ,<b>false</b>.
        /// </value>
        public bool SuspendAutoScrolling
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value to suspend the selection operations that are performed internally.
        /// </summary>
        /// <value>
        /// <b>true</b> if the selection operations is suspended; otherwise , <b>false</b>.
        /// </value>
        public bool IsSuspended
        {
            get { return isSuspended; }
        }

        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </value>
        public SfDataGrid DataGrid
        {
            get { return dataGrid; }
            protected set { dataGrid = value; }
        }

        private int suspendcount = -1;

        /// <summary>
        /// Temporarily suspends the updates for the selection operation in SfDataGrid.
        /// </summary>
        protected internal void SuspendUpdates()
        {
            suspendcount++;
            this.isSuspended = true;
        }

        /// <summary>
        /// Resumes usual selection operation in SfDataGrid
        /// </summary>
        protected internal void ResumeUpdates()
        {
            if (suspendcount == 0)
                this.isSuspended = false;
            suspendcount--;
        }

        /// <summary>
        /// Clears the row header state based on the current row .
        /// </summary>
        /// <remarks>
        /// This method helps to update the visual state of row header based on current row.
        /// </remarks>
        protected void ClearRowHeader()
        {
            bool rowSelection = DataGrid.SelectionUnit == GridSelectionUnit.Row;
            DataGrid.RowGenerator.Items.ForEach(row =>
            {
                //While changing the SelectionMode from Multiple to Single the CurrentRow is cleared, hence the below rowSelection 
                //and IsSelectedColumn condition is added.
                if ((rowSelection && !row.IsSelectedRow) || (!rowSelection && !row.VisibleColumns.Any(col=>col.IsSelectedColumn)))
                {
                    if (row.IsCurrentRow)
                    {
                        row.IsCurrentRow = false;
                        (row as GridDataRow).ApplyRowHeaderVisualState();
                    }
                }
            });
        }

        /// <summary>
        /// Determines whether the currently navigated row is last row.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the navigated row is last row; otherwise, <b>false</b>
        /// </returns>
        protected bool CheckIsLastRow(SfDataGrid dataGrid)
        {
            SfDataGrid parentGrid;
            if (dataGrid.NotifyListener == null)
                parentGrid = dataGrid;
            else
               parentGrid = dataGrid.NotifyListener.GetParentDataGrid();
            if (parentGrid != null && parentGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex == parentGrid.GetLastNavigatingRowIndex())
            {
                if (parentGrid.NotifyListener != null)
                    return CheckIsLastRow(parentGrid);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// Changes the flow direction of key navigation based on the <see cref="System.Windows.FlowDirection"/> in SfDataGrid.
        /// </summary>
        /// <param name="currentKey">
        /// Contains the key that was pressed.
        /// </param>
        protected internal void ChangeFlowDirectionKey(ref Key currentKey)
        {            
            switch (currentKey)
            {
                case Key.Right:
                    currentKey = Key.Left;
                    break;
                case Key.Left:
                    currentKey = Key.Right;
                    break;
                case Key.Home:
                    {
                        if (!SelectionHelper.CheckControlKeyPressed())
                            currentKey = Key.End;
                    }
                    break;
                case Key.End:
                    {
                        if (!SelectionHelper.CheckControlKeyPressed())
                            currentKey = Key.Home;
                    }
                    break;
            }
        }

        internal GridRowInfo GetGridSelectedRow(object data)
        {
            if (data != null)
            {
                var rowIndex = this.DataGrid.ResolveToRowIndex(data);
                var nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                if (nodeEntry != null)
                    return new GridRowInfo(rowIndex, data, nodeEntry);
            }
            return null;
        }

        /// <summary>
        /// Gets the selected row information based on the specified row index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get selected row information.
        /// </param>
        /// <returns>
        /// Returns the selected row information.
        /// </returns>
        public GridRowInfo GetGridSelectedRow(int rowIndex)
        {
            if (rowIndex != -1)
            {
                GridRowInfo rowInfo = null;
                var nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                //To check whether the given rowIndex is UnBoundRow or Not.
                if (nodeEntry == null && dataGrid.IsUnBoundRow(rowIndex))
                {
                    var unBoundRow = this.DataGrid.GetUnBoundRow(rowIndex);
                    if (unBoundRow != null)
                        rowInfo = new GridRowInfo(rowIndex, unBoundRow);
                }
                else if (nodeEntry == null && DataGrid.IsFilterRowIndex(rowIndex))
                    rowInfo = new GridRowInfo(true, rowIndex);
                else if (nodeEntry == null && this.DataGrid.IsAddNewIndex(rowIndex))
                    rowInfo = new GridRowInfo(rowIndex, true);
                else if(nodeEntry != null)
                {
                    object data = null;
                    if (nodeEntry is NestedRecordEntry)
                        data = ((RecordEntry)nodeEntry.Parent).Data;
                    else if (nodeEntry is RecordEntry)
                        data = ((RecordEntry)nodeEntry).Data;
                    rowInfo = new GridRowInfo(rowIndex, data, nodeEntry);
                }
                return rowInfo;
            }
            return null;
        }

        internal bool ClearSelectedDetailsViewGrid()
        {
            //In Multiple mode the Selection is in DetailsViewGrid and the CurrentCell is in parentGrid, 
            //when we select the another row in ParentGrid Selection maintains in both DetailsViewGrid and ParentGrid.
            //So that added the Condition while the CurrentRowColumnIndex is DetailsViewGridIndex or not.
            var detailsViewGrid = this.dataGrid.SelectionMode == GridSelectionMode.Multiple && dataGrid.IsInDetailsViewIndex(currentCellManager.CurrentRowColumnIndex.RowIndex) ? dataGrid.GetDetailsViewGrid(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex) : dataGrid.SelectedDetailsViewGrid;
            if (detailsViewGrid != null)
            {
                if (!this.ClearDetailsViewGridSelections(detailsViewGrid as DetailsViewDataGrid))
                    return false;
                this.DataGrid.SelectedDetailsViewGrid = null;
            }
            return true;
        }

        /// <summary>
        /// Removes the child grid current cell when changing the current item in parent grid.
        /// </summary>
        internal void RemoveChildGridCurrentCell()
        {
            if (!this.DataGrid.IsInDetailsViewIndex(this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                return;

            //Gets the DetailsViewDataRow which is the CurrentRow of the current datagrid.
            var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(this.DataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            if (detailsViewGrid == null)
                return;

            var childGridSelectionController = detailsViewGrid.SelectionController as GridBaseSelectionController;
            //Removes the current cell if the DetailsViewDataGrid contains child grid.
            childGridSelectionController.RemoveChildGridCurrentCell();
            childGridSelectionController.CurrentCellManager.RemoveCurrentCell(childGridSelectionController.CurrentCellManager.CurrentRowColumnIndex);
            childGridSelectionController.CurrentCellManager.ResetCurrentRowColumnIndex();
        }

        /// <summary>
        /// Clears all the selection for the specified DetailsViewDataGrid.
        /// </summary>
        /// <param name="detailsViewDataGrid">
        /// The corresponding DetailsViewDataGrid to clear the selection.
        /// </param>
        protected virtual bool ClearDetailsViewGridSelections(SfDataGrid detailsViewDataGrid)
        {
            //Here we have checked with the DetailsViewGrid having the SelectedDetailsViewGrid or Not.
            //while making the selection in the ParentGrid which having the DetailsViewGrid means we have to clear the selection.
            var detailsViewGrid = detailsViewDataGrid.SelectedDetailsViewGrid;
            if (detailsViewGrid != null)
            {
                if (!ClearDetailsViewGridSelections(detailsViewGrid))
                    return false;
                detailsViewDataGrid.SelectedDetailsViewGrid = null;
            }
            if (!detailsViewDataGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
                return false;
            
            if (detailsViewDataGrid.HasView && detailsViewDataGrid.View.IsAddingNew)
                (detailsViewDataGrid.SelectionController as GridBaseSelectionController).CommitAddNew();

            detailsViewDataGrid.SelectionController.ClearSelections(false);
            detailsViewDataGrid.RowGenerator.Items.ForEach(row =>
            {
                if (row.IsCurrentRow)
                {
                    row.IsCurrentRow = false;
                    (row as GridDataRow).ApplyRowHeaderVisualState();
                }
            });
            return true;
        }

        /// <summary>
        /// Clears the DetailsViewGrid selections when the selection or current cell is maintained in that particular grid.
        /// </summary>
        /// <param name="rowColumnIndex"></param>
        /// <returns></returns>
        internal bool ClearChildGridSelections(RowColumnIndex rowColumnIndex)
        {
            //Clears the NestedGridSelection when the CurrentCell is present in corresponding NestedGrid.
            if (this.DataGrid.SelectionMode != GridSelectionMode.Single && this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
            {
                //In Multiple mode selection, there is possible to move the current cell to one NestedGrid but the selection will 
                //be in some other nestedGrid. Hence the current cell selection will be cleared by using correspondin rowColumn index
                //particular NetedGrid.
                var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(rowColumnIndex.RowIndex);
                if (detailsViewGrid != null)
                {
                    if (!this.ClearDetailsViewGridSelections(detailsViewGrid))
                        return false;
                    if (this.DataGrid.SelectedDetailsViewGrid == detailsViewGrid)
                        this.DataGrid.SelectedDetailsViewGrid = null;
                }
            }
            //If the CurrentCell and Selection is in Differnt nested grids then the CurrentCell selection will be cleared in above code
            //the SelectedDetailsViewGrid will be cleared from below code.
            if (this.DataGrid.SelectedDetailsViewGrid != null)
            {
                if (!this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                    return false;
                this.DataGrid.SelectedDetailsViewGrid = null;
            }
            return true;
        }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes in DetailsViewDataGrid.
        /// </summary>
        /// <remarks>
        /// This method remove the selection from its parent grid when the SelectionMode is Single. For Multiple or Extended , the current cell is reset from parent grid to child grid.
        /// </remarks>
        protected void ProcessDetailsViewCurrentItemChanged()
        {
            SfDataGrid detailsViewGrid = this.DataGrid;
            //Added while loop to change the parentGrid currentcell recursively.
            while (detailsViewGrid.NotifyListener != null)
            {
                var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
                if (parentDataGrid == null)
                    break;
                var selectioncController = parentDataGrid.SelectionController as GridBaseSelectionController;
                var detailsViewDataRow = parentDataGrid.RowGenerator.Items.FirstOrDefault(row => (row is DetailsViewDataRow) && (row as DetailsViewDataRow).DetailsViewDataGrid == this.DataGrid);
                var detailsViewIndex = detailsViewDataRow.RowIndex;
                //To check whether the parentGrid current row is in current DetailsViewDataRow using CurrentRowIndex and SelectedDetailsViewGrid.
                if (parentDataGrid.SelectedDetailsViewGrid != this.DataGrid || selectioncController.CurrentCellManager.CurrentRowColumnIndex.RowIndex != detailsViewIndex)
                {
                    //To clear the selection when the SelectionMode is single
                    if (parentDataGrid.SelectedDetailsViewGrid != null && this.DataGrid.SelectionMode == GridSelectionMode.Single)
                    {
                        if (!this.ClearDetailsViewGridSelections(parentDataGrid.SelectedDetailsViewGrid))
                            return;
                        parentDataGrid.SelectedDetailsViewGrid = null;
                    }
                    //To remove the current cell of the child Grid which contains the current cell.
                    else if (selectioncController.CurrentCellManager.CurrentRowColumnIndex.RowIndex != detailsViewIndex)
                    {
                        selectioncController.RemoveChildGridCurrentCell();
                    }

                    var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                                   ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                                   : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                    var rowcolIndex = new RowColumnIndex(detailsViewIndex, colIndex);
                    //To add the selection to DetailsViewDataRow when the SelectionMode is singel.
                    if (parentDataGrid.SelectionMode == GridSelectionMode.Single)
                    {
                        List<object> rItems;
                        if (this.DataGrid.SelectionUnit == GridSelectionUnit.Row)
                        {
                            rItems = selectioncController.SelectedRows.ToList<object>();
                            (selectioncController as GridSelectionController).ResetSelection(rowcolIndex.RowIndex, rItems, false);
                        }
                        else
                        {
                            rItems = selectioncController.SelectedCells.ToList<object>();
                            (selectioncController as GridCellSelectionController).ResetSelection(rowcolIndex, rItems, false);
                        }
                        parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
                    }
                    //Removes the parengrid current cell when SelectionMode is Mutiple or Extended.
                    else if (selectioncController.CurrentCellManager.CurrentRowColumnIndex.RowIndex != detailsViewIndex)
                    {
                        selectioncController.CurrentCellManager.RemoveCurrentCell(selectioncController.CurrentCellManager.CurrentRowColumnIndex);
                        selectioncController.CurrentCellManager.SelectCurrentCell(rowcolIndex);
                    }
                }
                detailsViewGrid = parentDataGrid;
            }
        }

        #endregion
        
        #region WireAndUnwire Events

        /// <summary>
        /// Wires the events associated with selection operation in SfDataGrid. 
        /// </summary>
        protected internal virtual void WireEvents()
        {
            this.DataGrid.AutoScroller.AutoScrollerValueChanged -= AutoScrollerValueChanged;
            this.DataGrid.AutoScroller.AutoScrollerValueChanged += AutoScrollerValueChanged;
            this.DataGrid.Loaded += DataGrid_Loaded;
        }

        /// <summary>
        /// Unwires the events associated with selection operation in SfDataGrid. 
        /// </summary>
        protected internal virtual void UnWireEvents()
        {
            if (this.DataGrid != null)
            {
                this.DataGrid.AutoScroller.AutoScrollerValueChanged -= AutoScrollerValueChanged;
                if (this.DataGrid.VisualContainer != null)
                    this.UnWireVisualContainerEvents();
            }
        }

        /// <summary>
        /// Method which is helps to Unhook the VisualContainer based events.
        /// </summary>
        /// <remarks></remarks>
        internal void WireVisualContainerEvents()
        {
#if WPF
            this.DataGrid.VisualContainer.MouseLeave += VisualContainer_MouseLeave;
            this.DataGrid.VisualContainer.MouseLeftButtonUp += VisualContainer_MouseLeftButtonUp;
            this.DataGrid.VisualContainer.MouseMove += VisualContainer_MouseMove;
#else
            this.DataGrid.VisualContainer.PointerReleased += VisualContainer_PointerReleased;
            this.DataGrid.VisualContainer.PointerEntered += VisualContainer_PointerEntered;
            this.DataGrid.VisualContainer.PointerExited += VisualContainer_PointerExited;
            this.DataGrid.VisualContainer.PointerMoved += VisualContainer_PointerMoved;
#endif
        }

        /// <summary>
        /// Method which is helps to Unhook the VisualContainer based events.
        /// </summary>
        /// <remarks></remarks>
        internal void UnWireVisualContainerEvents()
        {
#if WPF
            this.DataGrid.VisualContainer.MouseLeave -= VisualContainer_MouseLeave;
            this.DataGrid.VisualContainer.MouseLeftButtonUp -= VisualContainer_MouseLeftButtonUp;
            this.DataGrid.VisualContainer.MouseMove -= VisualContainer_MouseMove;
#else
            this.DataGrid.VisualContainer.PointerReleased -= VisualContainer_PointerReleased;
            this.DataGrid.VisualContainer.PointerEntered -= VisualContainer_PointerEntered;
            this.DataGrid.VisualContainer.PointerExited -= VisualContainer_PointerExited;
            this.DataGrid.VisualContainer.PointerMoved -= VisualContainer_PointerMoved;
#endif
        }

        void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataGrid != null && this.DataGrid.VisualContainer != null)
            {
                UnWireVisualContainerEvents();
                WireVisualContainerEvents();
            }
        }

#endregion
        #region AutoScroller

        /// <summary>
        /// Invoked when the selection is dragged out of view area in SfDataGrid.
        /// </summary>
        /// <param name="sender">
        /// The sender that contains the SfDataGrid.
        /// </param>
        /// <param name="args">
        /// Contains the event data.   
        /// </param>
        protected virtual void AutoScrollerValueChanged(object sender, AutoScrollerValueChangedEventArgs args)
        {            
            if (this.DataGrid == null || this.DataGrid.VisualContainer == null || SuspendAutoScrolling)
                return;
            RowColumnIndex  rowcolIndex= this.currentCellManager.CurrentRowColumnIndex;
            //WPF-21709- Selection with AutoScrolling is processed based on CurrentRowColumn index 
            //insted of PressedRowColumnIndex so calculate the RowColumnIndex for drag selection based on PressedIndex.
            if (args.IsLineDown)
            {
                rowcolIndex = args.RowColumnIndex.RowIndex < PressedRowColumnIndex.RowIndex ? PressedRowColumnIndex : args.RowColumnIndex;
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > rowcolIndex.RowIndex)
                    return;
            }

            if (args.IsLineUp)
            {
                rowcolIndex = args.RowColumnIndex.RowIndex > PressedRowColumnIndex.RowIndex ? PressedRowColumnIndex : args.RowColumnIndex;
            }

            if (args.IsLineLeft || args.IsLineRight)
            {
                rowcolIndex.ColumnIndex = args.RowColumnIndex.ColumnIndex;
            }

            if (rowcolIndex.RowIndex != this.CurrentCellManager.CurrentRowColumnIndex.RowIndex || rowcolIndex.ColumnIndex != this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex)
            {
                //Added condition to check SelectedDetailsViewGrid to skip the selection in Parent grid while dragging from Child grid to out of window.
                if (!this.DataGrid.IsInDetailsViewIndex(rowcolIndex.RowIndex) && this.DataGrid.SelectedDetailsViewGrid == null)
                {
                    MouseEventArgs e = null;
                    this.ProcessDragSelection(e, rowcolIndex);
                }
            }
        }

        /// <summary>
        /// Processes the selection when the dragging operation is performed on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data for mouse events.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index to perform drag selection.
        /// </param>
        /// <remarks>
        /// This method will be raised when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> is Multiple or Extended.
        /// </remarks>
        protected abstract void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex);

        #endregion

        #region AutoScroller Event Handlers

#if WPF

        void VisualContainer_MouseLeave(object sender, MouseEventArgs args)
        {
            //Added the Condition to check the Selection Mode is Multiple or Extended to avoid the Scrolling for none other cases.
            if (!this.DataGrid.AutoScroller.IsEnabled || (this.DataGrid.SelectionMode != GridSelectionMode.Multiple && this.DataGrid.SelectionMode != GridSelectionMode.Extended))
                return;

			//131467 - When we DragandDrop the selected records, the MouseCapture() are triggered contionusly, so it crashed. To avoid this, we have used Dispatcher.
            this.DataGrid.Dispatcher.BeginInvoke(new Action(() => {
            //Added the Condition SuspendAutoScrolling is true or not to check the DragSelection is Enabled or Not.
            if (args.LeftButton == MouseButtonState.Pressed && isMousePressed && this.DataGrid.SelectedDetailsViewGrid == null && !this.SuspendAutoScrolling)

            {
                this.DataGrid.VisualContainer.CaptureMouse();                
                this.isMouseCaptured = true;
            }
            }), DispatcherPriority.ApplicationIdle);
        }

        void VisualContainer_MouseMove(object sender, MouseEventArgs args)
        {
            if (!(this.isMouseCaptured || this.isMousePressed) || this.CurrentCellManager.IsAddNewRow || this.CurrentCellManager.IsFilterRow
                || this.DataGrid.SelectionMode == GridSelectionMode.None || this.DataGrid.SelectionMode == GridSelectionMode.Single)
                return;

            var point = args.GetPosition(this.DataGrid.VisualContainer);

            if (this.DataGrid.AutoScroller.AutoScrolling != AutoScrollOrientation.None)
                this.DataGrid.AutoScroller.MouseMovePosition = point;
            if (CanStartAutoScroller(point, args))
            {
                this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.Both;
                //While Starting the ParentGrid AutoScroller it will move the Scrolling for the ParentGird also. It will not stop the AutoScroller.
                //this.DataGrid.GetParentGrid().AutoScroller.AutoScrolling = AutoScrollOrientation.Both;
            }
            //Added condition to check SelectedDetailsViewGrid to skip the selection in Parent grid while dragging from Child grid to out of window.  
            //Checked the Condition this.SuspendAutoScrolling is true or not to avoid the Selection in the parentgrid while Dragging the rows.
            if (!this.DataGrid.AutoScroller.InsideScrollBounds.Contains(point) && this.isMouseCaptured && !this.SuspendAutoScrolling && this.DataGrid.SelectedDetailsViewGrid == null)
            {
                var rowColumnIndex = this.DataGrid.VisualContainer.PointToCellRowColumnIndex(point, true);
                if (rowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() || rowColumnIndex.RowIndex >= this.DataGrid.GetLastNavigatingRowIndex())
                    rowColumnIndex.RowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;

                if (rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex())
                    rowColumnIndex.ColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                ProcessDragSelection(args, rowColumnIndex);
            }
        }

        void VisualContainer_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.isMouseCaptured || this.isMousePressed)
            {
                this.DataGrid.VisualContainer.ReleaseMouseCapture();
                this.isMouseCaptured = false;
                this.isMousePressed = false;
            }

            //Need to skip AutoScrolling when the pointer released the Left button.
            if (this.DataGrid.AutoScroller.AutoScrolling != AutoScrollOrientation.None)
                this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
        }
#else
        void VisualContainer_PointerExited(object sender, PointerRoutedEventArgs args)
        {
            if (!this.DataGrid.AutoScroller.IsEnabled)
                return;
            //commented below, hence we wont use this.
            //Point pointerMovedPosition = SelectionHelper.GetPointPosition(args, null);
            //Need to start AutoScrolling when the pointer leaves the grid.
            if (args.Pointer.IsInContact && this.isMousePressed && this.DataGrid.SelectedDetailsViewGrid == null && (this.DataGrid.SelectionMode == GridSelectionMode.Multiple || this.DataGrid.SelectionMode == GridSelectionMode.Extended))
            {
                this.DataGrid.VisualContainer.CapturePointer(args.Pointer);
                this.isMouseCaptured = true;
            }
        }

        void VisualContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.isMouseCaptured)
            {
                this.isMouseCaptured = false;
                this.DataGrid.VisualContainer.ReleasePointerCapture(e.Pointer);
            }
        }

        void VisualContainer_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (this.isMouseCaptured || this.isMousePressed)
            {
                this.isMousePressed = false;
                this.isMouseCaptured = false;
                this.DataGrid.VisualContainer.ReleasePointerCapture(e.Pointer);
            }
            //Need to skip AutoScrolling when the pointer released the Left button.
            if (this.DataGrid.AutoScroller.AutoScrolling != AutoScrollOrientation.None)
                this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
        }

        void VisualContainer_PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            Window current = Window.Current;
            Point point;
            try
            {
                point = current.CoreWindow.PointerPosition;
            }
            catch (UnauthorizedAccessException)
            {
                this.DataGrid.AutoScroller.MouseMovePosition = new Point(double.NegativeInfinity, double.NegativeInfinity);
            }

            point = this.DataGrid is DetailsViewDataGrid ? args.GetCurrentPoint(this.DataGrid.VisualContainer).Position : args.GetCurrentPoint(this.dataGrid).Position;
            Point containerpoint;
            if ((sender as VisualContainer).ScrollOwner != null)
                containerpoint = (sender as VisualContainer).ScrollOwner.TransformToVisual(null).TransformPoint(new Point(0, 0));
            else if ((sender as VisualContainer).ScrollableOwner != null)
                containerpoint = (sender as VisualContainer).ScrollableOwner.TransformToVisual(null).TransformPoint(new Point(0, 0));

            if (CanStartAutoScroller(point, args))
                this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.Both;

            //MouseMovePosition is sets based on the DataGrid position.
            this.DataGrid.AutoScroller.MouseMovePosition = point;
            var rowColumnIndex = this.DataGrid.VisualContainer.PointToCellRowColumnIndex(point, true);
            //Added condition to check SelectedDetailsViewGrid to skip the selection in Parent grid while dragging from Child grid to out of window.
            // If mouse is captured then only, should access InsideScrollBounds. so isMouseCaptured condition moved to first
            if (this.isMouseCaptured && !this.DataGrid.AutoScroller.InsideScrollBounds.Contains(point) && this.DataGrid.SelectedDetailsViewGrid == null)
            {
                //To check whether the rowIndex and columnIndex reaches the first and last row or column indexes.
                if (rowColumnIndex.RowIndex < this.DataGrid.GetFirstDataRowIndex() || rowColumnIndex.RowIndex >= this.DataGrid.GetLastNavigatingRowIndex())
                    rowColumnIndex.RowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;

                if (rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex())
                    rowColumnIndex.ColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

                //ProcessDragSelection is called for auto scrolling like WPF.
                ProcessDragSelection(args, rowColumnIndex);
            }
        }

#endif


        #endregion

        #region Selection Changing Event Methods

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanging"/> event in SfDataGrid.
        /// </summary>
        /// <param name="addedItems">
        /// Contains the items that were selected.
        /// </param>
        /// <param name="removedItems">
        /// Contains the items that were unselected.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanging"/> is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseSelectionChanging(List<object> addedItems, List<object> removedItems)
        {
            var args = new GridSelectionChangingEventArgs(this.DataGrid)
            {
                AddedItems = addedItems,
                RemovedItems = removedItems,
            };
            return this.DataGrid.RaiseSelectionChagingEvent(args);
        }

    
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanged"/> event.
        /// </summary>
        /// <param name="addedItems">
        /// Contains the items that were selected.
        /// </param>
        /// <param name="removedItems">
        /// Contains the items that were unselected.
        /// </param>
        protected void RaiseSelectionChanged(List<object> addedItems, List<object> removedItems)
        {
            if (!ValidationHelper.IsCurrentCellValidated)
                return;
            var args = new GridSelectionChangedEventArgs(this.DataGrid)
            {
                AddedItems = addedItems,
                RemovedItems = removedItems,
            };
            this.DataGrid.RaiseSelectionChangedEvent(args);
        }

        #endregion

        #region IDisposable
        /// <summary>
        /// Releases all resources used by the <see cref="T:Syncfusion.UI.Xaml.Grid.GridBaseSelectionController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="T:Syncfusion.UI.Xaml.Grid.GridBaseSelectionController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;

            this.UnWireEvents();
            if (isDisposing)
            {
                this.dataGrid = null;

                if (currentCellManager != null)
                {
                    this.currentCellManager.Dispose();
                    this.currentCellManager = null;
                }

                if (this.selectedRows != null)
                {
                    this.selectedRows.Clear();
                    this.selectedRows = null;
                }

                if (this.selectedCells != null)
                {
                    this.selectedCells.Clear();
                    this.selectedCells = null;
                }
            }
            isdisposed = true;
        }

        #endregion  
    }
}
