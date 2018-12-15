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
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif

    /*
     * Work Flow and Initialization Process
     1. Validation & EndEdit
     2. Removing Selections for other Grid's //If Selection is performed in Nested Grid
     * Adding Selection(DetailsViewDataRow) to Parent Grid as per the following order. //If Selection is performed in Nested Grid
     3. CurrentCell
     4. CurrentItem & CurrentColumn
     5. SelectedRows
     6. SelectedItems
     7. SelectedIndex
     8. SelectedItem
     
      
     *DetailsView Work Flow For Mouse
     1. HandlePointerOperations- For ChildGrid when clicking on ChildGrid
     2. ProcessDetailsViewGridPointerOperations- To proces for parent grid.
     3. HandlePointerOperations- ParentGrid
     
     *Details View Flow For KeyBoard
     1. HandleKeyDown- For ParentGrid when pressing on ChildGrid
           ->ProcessKeyDown- For Corresponding Grid
     2. HandleDetailsViewKeyDown- If CurrentCellIndex is DetailsViewIndex
           ->HandleKeyDown- For ChildGrid.
     3. ProcessDetailsViewKeyDown- If NewRowIndex is DetailsViewIndex
           ->ProcessDetailsViewIndex
           ->HandelDetailsViewKeyDown
     */

    /// <summary>
    /// Represents a class that implements the selection behavior of row in SfDataGrid.
    /// </summary>
    public class GridSelectionController : GridBaseSelectionController
    {
        /// <summary>
        /// Gets or sets the pressed index in SfDataGrid.
        /// </summary>
        [Obsolete("This pressedIndex property is obsolete; use PressedRowColumnIndex instead")]        
        protected internal int pressedIndex = -1;

        #region OverrideMethods
        /// <summary>
        /// Sets the PressedRowColumnIndex of SfDataGird.
        /// </summary>
        /// <param name="rowcolumnIndex">
        /// Indicates the starting position of drag selection.
        /// </param>
        /// <remarks>
        /// This method helps to initialize the starting position of drag selection.
        /// </remarks>
        protected override void SetPressedIndex(RowColumnIndex rowcolumnIndex)
        {
            base.SetPressedIndex(rowcolumnIndex);
            _pshiftselectionrowindex = -1;
        }

        #endregion
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionController"/> class.
        /// </summary>
        /// <param name="dataGrid">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/>.
        /// </param>
        public GridSelectionController(SfDataGrid dataGrid) : base(dataGrid) { }
        #endregion

        #region ISelectionController

        /// <summary>
        /// Selects all the rows in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public override void SelectAll()
        {
            // if DataGrid View is null or VirtualizingCollectionView then just return the contorl WPF-18089
            if (this.DataGrid.View == null || this.DataGrid.View is VirtualizingCollectionView)
                return;
            

            if (this.DataGrid.SelectionMode != GridSelectionMode.None && this.DataGrid.SelectionMode != GridSelectionMode.Single)
            {
                int currentIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                int columnIndex=this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex<this.CurrentCellManager.GetFirstCellIndex()?this.CurrentCellManager.GetFirstCellIndex():CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                
                if (this.DataGrid.SelectedDetailsViewGrid != null)
                {
                    if (!ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                        return;

                    //In Multiple selection mode, when pressing Ctrl + A in parent grid with selection in Child grid an Current Cell alone,
                    //Parent grid, the two two current cell has been created because the currentIndex is set to -1. Hence the below
                    //code is used.
                    if(this.DataGrid.IsInDetailsViewIndex(currentIndex))
                    {
                        var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(currentIndex);
                        if (this.DataGrid.SelectedDetailsViewGrid == detailsViewGrid)
                            currentIndex = -1;
                    }
                    this.DataGrid.SelectedDetailsViewGrid = null;
                }

                if (this.DataGrid.NotifyListener != null && !this.ProcessParentGridSelection())
                    return;

                if (currentIndex < 0)
                    currentIndex = this.DataGrid.GetFirstDataRowIndex();

                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(currentIndex, columnIndex));

                this.SuspendUpdates();

                var addedItems = this.SelectedRows.ToList<object>();
                int rowIndex = this.DataGrid.GetFirstDataRowIndex();
                var rowCount = this.DataGrid.GetLastDataRowIndex();

                var rowIndexes = this.SelectedRows.GetRowIndexes();

                var selectedrowindex = this.dataGrid.ResolveToRowIndex(this.dataGrid.SelectedIndex);

                if (!this.DataGrid.GridModel.HasGroup)
                {
                    int recordindex = 0;
                    GridRowInfo rowInfo = null;                  
                    foreach (var record in this.DataGrid.View.Records)
                    {
                        rowIndex = this.DataGrid.ResolveToRowIndex(recordindex);
                        if (!rowIndexes.Contains(rowIndex))
                        {
                            this.DataGrid.SelectedItems.Add(record.Data);
                            rowInfo = new GridRowInfo(rowIndex, record.Data, record);
                            this.SelectedRows.Add(rowInfo);
                        }
                        recordindex++;
                    }                    
                }
                else
                {
                    while (rowIndex <= rowCount)
                    {
                        var nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                        GridRowInfo rowInfo = null;
                        if (!rowIndexes.Contains(rowIndex) && !(nodeEntry is NestedRecordEntry))
                        {
                            var recordntry = nodeEntry as RecordEntry; 
                            if (recordntry != null)
                            {
                                rowInfo = new GridRowInfo(rowIndex, recordntry.Data, recordntry);
                                //Added the record entry in the selectedItems 
                                this.dataGrid.SelectedItems.Add(recordntry.Data);
                            }
                            else
                                rowInfo = new GridRowInfo(rowIndex, null, nodeEntry);
                            //Added the all the rows in the display except table summary row, AddNewRow.
                            this.SelectedRows.Add(rowInfo);
                        }
                        // Get the nextrowindex need to select in the view by using GetNextScrollLineIndex
                        rowIndex = rowIndex >= rowCount ? ++rowIndex : this.DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(rowIndex);
                    }
                }

                this.ShowAllRowSelectionBorder();                
                var aItems = this.SelectedRows.Except(addedItems).ToList();
                this.RefreshSelectedIndex();
                this.ResumeUpdates();
                this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                var args = new GridSelectionChangedEventArgs(this.DataGrid) { AddedItems = aItems, RemovedItems = new List<object>() };
                this.DataGrid.RaiseSelectionChangedEvent(args);
            }
        }

        /// <summary>
        /// Moves the current cell for the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current row selection should remove or not while moving the current cell.
        /// </param>     
        /// <remarks>
        /// This method is not applicable when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> is Row.
        /// </remarks>     
        public override void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true)
        {
            if (dataGrid.NavigationMode != NavigationMode.Cell || this.DataGrid.SelectionMode == GridSelectionMode.None || !CurrentCellManager.CanMoveCurrentCell(rowColumnIndex))
                return;

            if (!ClearSelectedDetailsViewGrid())
                return;

            if (this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                throw new InvalidOperationException("Cant Move the current cell to DetailsViewIndex.");

            if (!CurrentCellManager.ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Program))
                return;
            
            //WPF-25372 When we call the MoveCurrentCell more than once with SelectionMode except Single, with same RowIndex,
            //SelectedRows and SelectedItems having dupilcate value, so before adding SelectedRows and SelectedItems we check that rowindex is contains.
            if (this.SelectedRows.Contains(rowColumnIndex.RowIndex))
                return;

            //WPF-25047 Reset the PressedIndex
            SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            //WPF-25372 When we call the MoveCurrentCell more than once with SelectionMode except Single, with same RowIndex,
            //SelectedRows and SelectedItems having dupilcate value, so before adding SelectedRows and SelectedItems we check that rowindex is contains.
            if (this.SelectedRows.Contains(rowColumnIndex.RowIndex))
                return;

            if (rowColumnIndex.RowIndex != this.CurrentCellManager.previousRowColumnIndex.RowIndex && this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
            {
                //While the CurrentCell is Moving from the AddNewRow we have to check that is in Editing state or Not.
                if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)                
                    this.DataGrid.GridModel.AddNewRowController.CommitAddNew();                
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
            }

            this.SuspendUpdates();

            if (needToClearSelection || this.DataGrid.SelectionMode == GridSelectionMode.Single)
                this.RemoveSelection(rowColumnIndex.RowIndex, this.SelectedRows.Cast<object>().ToList<object>(), SelectionReason.MovingCurrentCell);
            //The below condition is used to check whether the RowColumnIndex is greaterthan the FirstRowColumnIndex or not Because in this case we could not maintain the SelectedRows.           
            if (rowColumnIndex.RowIndex >= this.DataGrid.GetFirstNavigatingRowIndex() && rowColumnIndex.ColumnIndex >= this.DataGrid.GetFirstColumnIndex())
            {
                this.SelectedRows.Add(GetGridSelectedRow(rowColumnIndex.RowIndex));
                this.DataGrid.ShowRowSelection(rowColumnIndex.RowIndex);
                var rowData = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
                if (rowData != null)
                {
                    this.dataGrid.SelectedItems.Add(rowData);
                }
                //WPF-25771 Reset the parentgrid selection when the selection in DetailsViewGrid
                ResetParentGridSelection();
                //The below code is added while set the CurrentCell in AddNewRow the SelectedItem is maintained.                
                RefreshSelectedIndex();

                if (this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                    this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            }            
            this.ResumeUpdates();
        }

        /// <summary>
        /// Selects the rows corresponding to the specified start and end index of the row.
        /// </summary>
        /// <param name="startRowIndex">
        /// The start index of the row.
        /// </param>
        /// <param name="endRowIndex">
        /// The end index of the row.
        /// </param>
        public override void SelectRows(int startRowIndex, int endRowIndex)
        {
            if (startRowIndex < 0 || endRowIndex < 0)
                return;

            if (startRowIndex > endRowIndex)
            {
                var temp = startRowIndex;
                startRowIndex = endRowIndex;
                endRowIndex = temp;
            }

            if (this.DataGrid.SelectionMode == GridSelectionMode.None ||
                this.DataGrid.SelectionMode == GridSelectionMode.Single) 
                return;

            var isSelectedRowsContains = this.SelectedRows.Any();
                     
            if (!isSelectedRowsContains)
            {
                // WPF-33338 Update CurrentRowIndex while initial loading when NavigationMode is Row.
                if (this.dataGrid.NavigationMode == NavigationMode.Cell)
                    this.CurrentCellManager.ProcessCurrentCellSelection(new RowColumnIndex(endRowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), ActivationTrigger.Program);
                else
                    this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(endRowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
            }

            this.SuspendUpdates();
            var addedItem = new List<object>();
            int rowIndex = startRowIndex;

            //WPF-25771 Reset the parentgrid selection when the selection in DetailsViewGrid
            ResetParentGridSelection();

            while (rowIndex <= endRowIndex)
            {
                object rowData = this.DataGrid.GetRecordAtRowIndex(rowIndex);
                if (!this.SelectedRows.Contains(rowIndex) && !this.DataGrid.IsInDetailsViewIndex(rowIndex))
                    this.SelectedRows.Add(GetGridSelectedRow(rowIndex));

                this.DataGrid.ShowRowSelection(rowIndex);
                if (rowData != null && !this.DataGrid.SelectedItems.Contains(rowData))
                {
                    addedItem.Add(rowData);
                    this.DataGrid.SelectedItems.Add(rowData);
                }
                rowIndex++;
            }

            if (!isSelectedRowsContains)
            {
                this.RefreshSelectedIndex();
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Handles the row selection when the collection changes on SelectedItems, Columns and DataSource properties in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for collection changes.
        /// </param>
        /// <param name="reason">
        /// Contains the <see cref="Syncfusion.UI.Xaml.Grid.CollectionChangedReason"/> for collection changes.
        /// </param>
        /// <remarks>
        /// This method is called when the collection changes on SelectedItems, Columns and DataSource properties in SfDataGrid.
        /// </remarks>
        public override void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, CollectionChangedReason reason)
        {
            switch (reason)
            {
                case CollectionChangedReason.SelectedItemsCollection:
                    ProcessSelectedItemsChanged(e);
                    break;
                case CollectionChangedReason.ColumnsCollection:
                    CurrentCellManager.HandleColumnsCollectionChanged(e);
                    break;
                case CollectionChangedReason.DataReorder:
                    ProcessDataReorder(e.OldItems[0], e.Action);
                    break;
                default:
                    ProcessSourceCollectionChanged(e, reason);
                    break;
            }
        }

        /// <summary>
        /// Handles the row selection when the group is expanded or collapsed in SfDataGrid.
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
        public override void HandleGroupExpandCollapse(int index, int count, bool isExpanded)
        {
            if (isExpanded)
                ProcessGroupExpanded(index, count);
            else
                ProcessGroupCollapsed(index, count);
        }

        #endregion

        #region Protected Methods

        #region Pointer Operation Methods

        /// <summary>
        /// Handles the row selection when any of <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridPointerEventArgs"/> that contains the type of pointer operations and its arguments.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex to handle pointer operation in row.
        /// </param>
        /// <remarks>
        /// This method is invoked when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> are performed in SfDataGrid.
        /// </remarks>
        public override void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex)
        {
            switch (args.Operation)
            {
                case PointerOperation.Pressed:
                    this.ProcessPointerPressed(args.OriginalEventArgs as MouseButtonEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.Released:
                    this.ProcessPointerReleased(args.OriginalEventArgs as MouseButtonEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.Move:
                    ProcessPointerMoved(args.OriginalEventArgs as MouseEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.Tapped:
                    this.ProcessOnTapped(args.OriginalEventArgs as TappedEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.DoubleTapped:
                    this.ProcessOnDoubleTapped(args.OriginalEventArgs as DoubleTappedEventArgs,rowColumnIndex);
                    break;
#if WPF
                case PointerOperation.Wheel:
                    this.ProcessPointerWheel(args.OriginalEventArgs as MouseWheelEventArgs, rowColumnIndex);
                    break;
#endif
            }
        }

#if !WinRT && !WP && !UNIVERSAL
        /// <summary>
        /// Processes the row selection while scrolling the mouse wheel in SfDataGrid.
        /// </summary>
        /// <param name="mouseButtonEventArgs">
        /// An <see cref="T:System.Windows.Input.MouseButtonEventArgs"/>that contains the event data.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex where the mouse wheel interaction occurs.    
        /// </param>        
        protected virtual void ProcessPointerWheel(MouseWheelEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var currentCell = CurrentCellManager.CurrentCell;
            if (currentCell != null && CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowColumnIndex.RowIndex
                && currentCell.GridColumn.CanAllowSpinOnMouseScroll())
            {
                if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                {
                    var parent = this.DataGrid.NotifyListener.GetParentDataGrid();
                    parent.VisualContainer.SuspendManipulationScroll = currentCell.IsEditing;
                }
                else
                    DataGrid.VisualContainer.SuspendManipulationScroll = currentCell.IsEditing;
            }
            else
            {
                if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                {
                    var parent = this.DataGrid.NotifyListener.GetParentDataGrid();
                    parent.VisualContainer.SuspendManipulationScroll = false;
                }
                else
                    DataGrid.VisualContainer.SuspendManipulationScroll = false;
            }
        }
#endif

        /// <summary>
        /// Processes the row selection when the mouse pointer is pressed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the pressed point location.    
        /// </param>
        /// <remarks>
        /// This method will be invoked when the pointer is pressed using touch or mouse in SfDataGrid.
        /// The selection is initialized in pointer pressed state when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSelectionPointerPressed"/> set as true.
        /// </remarks>
        protected virtual void ProcessPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.None || rowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex())
                return;
#if WPF
            if (args != null && this.DataGrid.AllowSelectionOnPointerPressed && this.SelectedRows.Contains(rowColumnIndex.RowIndex) && args.ChangedButton == MouseButton.Right)
                return;

            pressedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            pressedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
                        
            pressedVerticalOffset = this.dataGrid.VisualContainer.ScrollOwner != null ? this.dataGrid.VisualContainer.ScrollOwner.VerticalOffset : 0;
#endif

            cancelDragSelection = false;
            //While dragging from footer UnBoundRow the Drag selection has been processed hence LastNavigatingRowIndex condition is added.
            if (!this.CurrentCellManager.AllowFocus(rowColumnIndex) || this.DataGrid.IsTableSummaryIndex(rowColumnIndex.RowIndex)
                || rowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() 
                || rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex()// cancel drag selection on Unbound DataRow when it is in freezed area.
                || (rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() 
                && !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex)
                && ((this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex)) 
                || rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex())))
            {
                //This flag is set to skip the drag selection when drag from indent cell or AllowFocus false column.
                cancelDragSelection = true;
                return;
            }

            if (rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() && !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
            {
                cancelDragSelection = rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() && !(rowColumnIndex.ColumnIndex == 0 && this.DataGrid.SelectionUnit == GridSelectionUnit.Any);
            }

            if (CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex || this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
            {
                if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                {
                    this.ProcessDetailsViewGridPointerPressed(args, rowColumnIndex);
                }
            }                    
            this.isMousePressed = true;           
               
            if (!SelectionHelper.CheckShiftKeyPressed())
                this.SetPressedIndex(rowColumnIndex);

            if (this.DataGrid.AllowSelectionOnPointerPressed)
            {
                ProcessPointerSelection(args, rowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the row selection when the mouse pointer is released from SfDataGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse released point.
        /// </param>
        /// <remarks>
        /// The selection is initialized in pointer released state when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSelectionPointerPressed"/> set as false.        
        /// </remarks>
        protected virtual void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            this.isMousePressed = false;
            var selectedDetailsViewGrid = this.DataGrid.SelectedDetailsViewGrid;
            while (selectedDetailsViewGrid != null)
            {
                if (selectedDetailsViewGrid.SelectedDetailsViewGrid != null)
                    selectedDetailsViewGrid = selectedDetailsViewGrid.SelectedDetailsViewGrid;
                else
                {
                    selectedDetailsViewGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
                    selectedDetailsViewGrid = null;
                }
            }

            this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;

            if (IsSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None || this.DataGrid.AllowSelectionOnPointerPressed || rowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex())
                return;

#if WPF
            var pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            //Point is calculated based on VisualContainer like WPF.
            Point pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);

            var releasedVerticalOffset = this.dataGrid.VisualContainer.ScrollOwner != null ? this.dataGrid.VisualContainer.ScrollOwner.VerticalOffset : 0;

            //  get the difference of pressed and released vertical offset value.
            var verticalOffsetChange = Math.Abs(releasedVerticalOffset - pressedVerticalOffset);
#endif

            var xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            var yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);

            //Here we checking the pointer pressed position and pointer released position. Because we don't selec the row while manipulate scrolling.
#if !WPF 
            //Selection need to be skipped if the scrolling happens in SfDataGrid. In UWP while Scrolling whole VisualContainer panel has moved.
            //So pressed and released position of vertical offset value is same. In this case we need to skip selection.So that difference of pressed and released position vertical offset has checked.
            //if verticalOffsetChange is lessthan 10 then  allow selection other than consider it has verticaly scrolling in SfDataGrid.
            if (xPosChange < 20 && yPosChange < 20 && verticalOffsetChange < 10)
            {
#else
            bool isValidated = ValidationHelper.IsCurrentRowValidated || ValidationHelper.IsCurrentCellValidated;
            if (xPosChange < 20 && yPosChange < 20) 
            {

                if (args != null && (this.SelectedRows.Contains(rowColumnIndex.RowIndex) || !isValidated) &&
                    args.ChangedButton == MouseButton.Right)
                {
                    //WPF-17423 Here we can handled the contextmenu when CurrentRow not Validated 
                    //WPF-25106 - Check rowColumnIndex with CurrentCellManager.CurrentRowColumnIndex to enable editor context menu.  (WPF-17423  break)
                    if (!isValidated && !rowColumnIndex.Equals((CurrentCellManager.CurrentRowColumnIndex)))
                        args.Handled = true;
                    return;
                }
#endif
                //WPF-17423 Here we can handled the contextmenu when pressedIndex and rowColumnIndex is different and mouse button click on right 
                if (this.PressedRowColumnIndex.RowIndex != rowColumnIndex.RowIndex && !SelectionHelper.CheckShiftKeyPressed())
                {
#if WPF
                    if (args == null) return;
                   if(args.ChangedButton==MouseButton.Right)
                       args.Handled = true;
#endif
                    return;
                }

                if (rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() && this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                    return;

                if (CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex || this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                {
                    if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                    {
                        this.ProcessDetailsViewGridPointerReleased(args, rowColumnIndex);
                    }
                }
                ProcessPointerSelection(args, rowColumnIndex);
            }
#if WPF
            else if (args.ChangedButton == MouseButton.Right && (!isValidated||this.PressedRowColumnIndex.RowIndex != rowColumnIndex.RowIndex))
            {
                args.Handled = true;
            }
#endif
        }
 
        /// <summary>
        /// Processes the row selection when the mouse pointer is pressed on the DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse pressed point.     
        /// </param>
        /// <remarks>
        /// This method invoked when the mouse point is pressed using touch or mouse in DetailsViewDataGrid.
        /// </remarks>
        protected virtual void ProcessDetailsViewGridPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowcolIndex = new RowColumnIndex(detailsViewDataRow.RowIndex, colIndex);
            parentDataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, args), rowcolIndex);
            //WPF-20222 - If we Cancel the selection in SelectionChanging event, we try to select child grid means the selection is not process.
            //If we process the selection means we can set the SelectedDetailsViewGrid, otherwise its should be null.
            if (this.DataGrid.AllowSelectionOnPointerPressed && rowcolIndex.RowIndex == parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
        }

        /// <summary>
        /// Processes the row selection when the mouse pointer is released from the DetailsViewDataGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer related changes.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse released point.
        /// </param>
        protected virtual void ProcessDetailsViewGridPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                               : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowcolIndex = new RowColumnIndex(detailsViewDataRow.RowIndex, colIndex);
            parentDataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, args), rowcolIndex);
            //WPF-20222 - If we Cancel the selection in SelectionChanging event, we try to select child grid means the selection is not process.
            //If we process the selection means we can set the SelectedDetailsViewGrid, otherwise its should be null.
            if (rowcolIndex.RowIndex == parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
        }
       
        /// <summary>
        /// Processes the row selection when the mouse point is tapped on the particular cell in a row. 
        /// </summary>
        /// <param name="e">
        /// Contains the data related to the tap interaction.
        /// </param>
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>    
        /// <remarks>
        /// This method invoked to process selection and end edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnTap"/>.
        /// </remarks>
        protected virtual void ProcessOnTapped(TappedEventArgs e, RowColumnIndex currentRowColumnIndex)
        {
            //Added the condition check whether the column can edited or not using AllowFocus method. In previous code, we have checked
            //with FirstCellIndex which is failed in AddNewRow.
            if (!ValidationHelper.IsCurrentCellValidated || IsSuspended || !this.DataGrid.AllowFocus(currentRowColumnIndex)  || this.DataGrid.SelectionMode == GridSelectionMode.None || (this.DataGrid.NavigationMode == NavigationMode.Row && !CurrentCellManager.IsAddNewRow && !CurrentCellManager.IsFilterRow))
                return;
#if WPF
            Point pointerReleasedRowPosition = e == null ? new Point() : SelectionHelper.GetPointPosition(e, this.DataGrid.VisualContainer);
#else
            Point pointerReleasedRowPosition = e == null ? new Point() : e.GetPosition(this.DataGrid.VisualContainer);
#endif

            var xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            var yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);
            //Here we checking the pointer pressed position and pointer released position. Because we don't selec the row while manipulate scrolling.
            if (xPosChange < 20 && yPosChange < 20)
            {
                CurrentCellManager.ProcessOnTapped(e, currentRowColumnIndex);
            }
            else
            {
                CurrentCellManager.ScrollInView(currentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the selection when the mouse point is double tapped on the particular cell in a row.
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
        protected virtual void ProcessOnDoubleTapped(DoubleTappedEventArgs e,RowColumnIndex currentRowColumnIndex)
        {
            if (!ValidationHelper.IsCurrentCellValidated || IsSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None || this.CurrentCellManager.CurrentRowColumnIndex != currentRowColumnIndex || (this.DataGrid.NavigationMode == NavigationMode.Row && !CurrentCellManager.IsAddNewRow && !CurrentCellManager.IsFilterRow))
                return;

            if (this.DataGrid.EditTrigger == EditTrigger.OnDoubleTap)
            {
                if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                    this.AddSelection(new List<object>() { GetGridSelectedRow(CurrentCellManager.CurrentRowColumnIndex.RowIndex) }, SelectionReason.PointerReleased);
                CurrentCellManager.ProcessOnDoubleTapped(e);
            }
        }

        /// <summary>
        /// Processes the row selection when the pointer pressed or released interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point.
        /// </param>
        protected internal void ProcessPointerSelection(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var previousCurrentCellIndex = CurrentCellManager.CurrentRowColumnIndex;

            if (!CurrentCellManager.HandlePointerOperation(args, rowColumnIndex))
                return;

            rowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;            
            int detailsViewIndex = this.DataGrid.GetSelectedDetailsViewGridRowIndex();            
            //In SelectionMode - Multiple, CurrentCell will be in some other NestedGrid or parentGrid when moving via Keyboard. So added condition to check detailsViewIndex and rowColumnIndex.RowIndex 
            //along with previousCurrentCellIndex.RowIndex != rowColumnIndex.RowIndex
            if ((detailsViewIndex > -1 && detailsViewIndex != rowColumnIndex.RowIndex) || previousCurrentCellIndex.RowIndex != rowColumnIndex.RowIndex)
            {
                if (this.DataGrid.SelectionMode != GridSelectionMode.Single && this.DataGrid.IsInDetailsViewIndex(previousCurrentCellIndex.RowIndex) && previousCurrentCellIndex.RowIndex != rowColumnIndex.RowIndex)
                {
                    //In Multiple mode selection, there is possible to move the current cell to one NestedGrid but the selection will 
                    //be in some other nestedGrid. Hence the current cell selection will be cleared by using correspondin rowColumn index
                    //particular NetedGrid.
                    var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(previousCurrentCellIndex.RowIndex);
                    if (detailsViewGrid != null)
                    {
                        if (!this.ClearDetailsViewGridSelections(detailsViewGrid))
                            return;
                        if (this.DataGrid.SelectedDetailsViewGrid == detailsViewGrid)
                            this.DataGrid.SelectedDetailsViewGrid = null;
                    }
                }
                //If the CurrentCell and Selection is in Different nested grids then the CurrentCell selection will be cleared in above code
                //the SelectedDetailsViewGrid will be cleared from below code.

                //When the SelectionMode as Multiple we have selected the first Nestedgrid and move the current cell to another nested grid, and 
                //again select the first nestedgrid means the previous selection is cleared and the new seletcion is maintained. So we need to 
                //Check the detailsViewIndex with rowindex and then clear the selection.
                if (this.DataGrid.SelectedDetailsViewGrid != null && detailsViewIndex != rowColumnIndex.RowIndex)
                {
                    if (!this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                        return;
                    this.DataGrid.SelectedDetailsViewGrid = null;
                }
            }

            if (this.DataGrid.SelectionMode == GridSelectionMode.Extended && SelectionHelper.CheckShiftKeyPressed() && this.SelectedRows.Count > 0)
            {
                if (ValidationHelper.IsCurrentCellValidated)
                    this.ProcessShiftSelection(rowColumnIndex.RowIndex, SelectionReason.PointerReleased);
            }
            else
            {
                var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
                if (ValidationHelper.IsCurrentCellValidated || skipValidation)
                    this.ProcessSelection(rowColumnIndex.RowIndex, previousCurrentCellIndex, SelectionReason.PointerReleased);
            }

            if (DataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                this.dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            else if (DataGrid.IsAddNewIndex(previousCurrentCellIndex.RowIndex))
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
        }

        #endregion
        
        #region Key Navigation Methods
        /// <summary>
        /// Handles the row selection when the keyboard interactions that are performed in DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in DetailsViewDataGrid; otherwise, <b>false</b>.
        /// </returns>
        public override bool HandleDetailsViewGridKeyDown(KeyEventArgs args)
        {
            var lastRowIndex = this.DataGrid.GetLastNavigatingRowIndex();
            var firstRowIndex = this.DataGrid.GetFirstNavigatingRowIndex();
            switch (args.Key)
            {
                case Key.Left:
                case Key.Right:
                case Key.PageUp:
                case Key.PageDown:
                case Key.Home:
                case Key.End:
                case Key.Escape:
                    {
                        this.ProcessKeyDown(args);
                        return true;
                    }
                case Key.F2:
                    {
                        if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                        {
                            this.ProcessKeyDown(args);
                            return true;
                        }
                        return false;
                    }
                case Key.Space:
                    {
                        this.ProcessKeyDown(args);
                        //Return false to process the selection in Parent grid.
                        return false;
                    }
                case Key.A:
                case Key.C:
                case Key.V:
                case Key.X:
                case Key.Insert:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.ProcessKeyDown(args);
                            return true;
                        }
                        return false;
                    }
                case Key.Delete:
                    {
                        this.ProcessKeyDown(args);
                        return true;                        
                    }
                case Key.Up:
                    {
                        if (firstRowIndex == -1)
                            return false;

                        if (this.CurrentCellManager.CurrentRowColumnIndex.IsEmpty || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == -1)
                        {
                            this.CurrentCellManager.SetCurrentRowColumnIndex(new RowColumnIndex(lastRowIndex + 1, this.CurrentCellManager.GetFirstCellIndex()));
                            if (PressedRowColumnIndex.RowIndex < 0)
                                this.SetPressedIndex(new RowColumnIndex(this.DataGrid.GetLastNavigatingRowIndex() + 1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }
                        var previousRowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        if ((previousRowIndex == firstRowIndex && previousRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                        {
                            if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                            {
                                //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                                if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                    return false;
                            }

                            //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                            if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow)
                            {
                                if (this.DataGrid.View.IsAddingNew)
                                    CommitAddNew();

                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                            }

                            //Clears the selection by when the current row index in first row while the SelectionMode is not multiple.
                            if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                this.ClearSelections(false);
                            else
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                //The Dotted border is shown when pressing the Up key  when the Navigation is moves from the DetailsViewDataRow to ParentGridDataRow and selectionMode as Multiple.                               
                                this.dataGrid.HideRowFocusBorder();
                            }
                            this.SetPressedIndex(new RowColumnIndex(-1,this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            return false;
                        }
                        this.ProcessKeyDown(args);
                        return args.Handled;
                    }
                case Key.Enter:
                case Key.Down:
                    {
                        if (PressedRowColumnIndex.RowIndex < 0)
                            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);

                        var nextRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < firstRowIndex
                                               ? firstRowIndex
                                               : this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        //needToMoveToLastRow flag check with the RowIndex if it is LastRowIndex then it will be Handled by the ParentGrid. If the lastRowIndex is DetailsViewIndex means we have to move the selection to the ParentGrid.

                        bool needToMoveToLastRow = SelectionHelper.CheckControlKeyPressed() && nextRowIndex == lastRowIndex && this.DataGrid.IsInDetailsViewIndex(nextRowIndex);
#if !WPF
                        if ((((nextRowIndex == lastRowIndex && nextRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || nextRowIndex == -1 || nextRowIndex > lastRowIndex) && !this.CheckIsLastRow(this.DataGrid)) || needToMoveToLastRow)
#else
                        if (((nextRowIndex == lastRowIndex && nextRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || nextRowIndex == -1 || nextRowIndex > lastRowIndex) || needToMoveToLastRow)
#endif
                        {
                            bool isLastRow = CheckIsLastRow(this.DataGrid);
                            if (isLastRow && args.Key == Key.Enter && this.dataGrid.HasView && this.dataGrid.View.IsAddingNew)
                            {
                                this.ProcessKeyDown(args);
                                return args.Handled;
                            }

                            if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                            {
                                //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                                if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                    return false;
                            }

                            //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                            if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow && !CheckIsLastRow(this.DataGrid))
                            {
                                if (this.DataGrid.View.IsAddingNew)
                                    CommitAddNew();

                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                            }

                            //WPF-18281 this condition is Added to check the clear the Selection in the Parent Grid.
                            if (this.DataGrid.IsInDetailsViewIndex(nextRowIndex) && !needToMoveToLastRow)
                            {
                                this.ProcessKeyDown(args);
                                if (args.Handled)
                                    return true;
                            }                            

                            if (isLastRow)
                            {
                                args.Handled = true;
                                return false;
                            }
                            //Clears the selection when the Current row index is in Last row while the selection mode not in multiple.
                            if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                            {
                                this.ClearSelections(false);
                            }
                            else
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                //The Dotted border is shown when pressing the Down key when the Navigation moves from DetailsViewDataRow to ParentGrid DataRow and selectionMode as Multiple.
                                this.dataGrid.HideRowFocusBorder();
                            }
                            this.SetPressedIndex(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            return false;
                        }
                        this.ProcessKeyDown(args);
                        return args.Handled;
                    }
                case Key.Tab:
                    {
                        if (DataGrid.GetRecordsCount() < 1 && DataGrid.AddNewRowPosition== AddNewRowPosition.None)
                        {
                            this.ClearSelections(false);
                            return false;
                        }
                        if (SelectionHelper.CheckShiftKeyPressed())
                        {
                            if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex())
                                this.CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.GetLastCellIndex() + 1);
                            
                            var rowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                            //WPF-25307 Need to calculate columnIndex based on flow direction flow direction
                            var columnIndex = CurrentCellManager.GetPreviousCellIndex(dataGrid.FlowDirection);
                            if (columnIndex == this.CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection) && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex)
                            {
                                if (rowIndex <= this.DataGrid.GetFirstNavigatingRowIndex() && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                                {
                                    if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                                    {
                                        //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                                        if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                            return false;
                                    }

                                    //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                                    if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow)
                                    {
                                        if (this.DataGrid.View.IsAddingNew)
                                            CommitAddNew();

                                        dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                    }

                                    //Clears the selection when the current row in First row of the DeatailsView grid.
                                    if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                        this.ClearSelections(false);
                                    else
                                    {
                                        this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                        this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                    }
                                    return false;
                                }
                            }
                        }
                        else
                        {
                            if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex())
                                this.CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.GetFirstCellIndex());
                            var columnIndex = CurrentCellManager.GetNextCellIndex(dataGrid.FlowDirection);                            
                            var rowIndex = this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            if (columnIndex == this.CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection) && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex || this.dataGrid.IsInDetailsViewIndex(rowIndex))
                            {                               
                                if (rowIndex >= lastRowIndex && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                                {
                                    bool isLastRow = CheckIsLastRow(this.DataGrid);
                                    if (isLastRow && this.dataGrid.HasView && this.dataGrid.View.IsAddingNew)
                                    {
                                        this.ProcessKeyDown(args);
                                        return args.Handled;
                                    }

                                    if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                                    {
                                        if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                            return false;
                                    }

                                    //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                                    if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow)
                                    {
                                        if (this.DataGrid.View.IsAddingNew)
                                            CommitAddNew();

                                        dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                    }

                                    //WPF-18281 this condition is Added to check the clear the Selection in the Parent Grid.
                                    if (this.DataGrid.IsInDetailsViewIndex(rowIndex))
                                    {
                                        this.ProcessKeyDown(args);
                                        if (args.Handled)
                                            return true;
                                    }

                                    //Checking whether this is the last row for whole grid to skip the navigation.
                                    if (isLastRow)
                                    {
                                        args.Handled = true;
                                        return false;
                                    }

                                    //Clears the selection when the current row is in Last row of the DetailsViewGrid.
                                    if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                        this.ClearSelections(false);
                                    else
                                    {
                                        this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                        this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                    }
                                    return false;
                                }
                            }
                        }
                        this.ProcessKeyDown(args);
                        return args.Handled;
                    }
                default:
                    return false;

            }
        }
     
        /// <summary>
        /// Handles the row selection when the keyboard interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in SfDataGrid; otherwise, <b>false</b>.
        /// </returns>
        public override bool HandleKeyDown(KeyEventArgs args)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.None)
                return args.Handled;

            //Checked whether the current row index is DetailsViewIndex to make a call for ShouldGridTryToHandleKeyDown 
            //method in current cell renderer.
            if ((this.dataGrid.NavigationMode == NavigationMode.Cell || this.CurrentCellManager.IsAddNewRow ||
                this.CurrentCellManager.IsFilterRow) && !this.DataGrid.IsInDetailsViewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
            {
                if (CurrentCellManager.HandleKeyDown(args))
                {
                    //Need to process HandleDetailsViewGridKeyDown method when the DataGrid is DetailsViewGrid.
                    if (this.DataGrid is DetailsViewDataGrid)
                        return this.HandleDetailsViewGridKeyDown(args);
                    else
                        ProcessKeyDown(args);
                }
            }
            else
            {
                //Need to process HandleDetailsViewGridKeyDown method when the DataGrid is DetailsViewGrid.
                if (this.DataGrid is DetailsViewDataGrid)
                    return this.HandleDetailsViewGridKeyDown(args);
                else
                    ProcessKeyDown(args);
            }
            return args.Handled;
        }

        /// <summary>
        /// Processes the row selection when the keyboard interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the information about the key that was pressed.
        /// </param>
        /// <remarks>
        /// This method helps to customize the keyboard interaction behavior in SfDataGrid.
        /// </remarks>      
        protected virtual void ProcessKeyDown(KeyEventArgs args)
        {
            var currentKey = args.Key;
            //Space key is ensured in switch loop itsel this code is not needed for space key.
            if (this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
            {
                var dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                if (dataRow is DetailsViewDataRow)
                {
                    var detailsViewDataRow = dataRow as DetailsViewDataRow;
                    //When pressing Ctrl+Home key after editing the TemplateColumn, the IsCurrentCellValidated and IsCurrentRowValidated property
                    //is set to true, hence the this method is continue the selection in parent grid. Therefore the grid will maintain two row of 
                    //Selection in ChildGrid and ParentGrid.
                    if (detailsViewDataRow.DetailsViewDataGrid.View != null && detailsViewDataRow.DetailsViewDataGrid.SelectionController.CurrentCellManager.HasCurrentCell &&
                        detailsViewDataRow.DetailsViewDataGrid.SelectionController.CurrentCellManager.CurrentCell.GridColumn.IsTemplate)
                    {
                        if (!detailsViewDataRow.DetailsViewDataGrid.SelectionController.CurrentCellManager.HandleKeyDown(args))
                            return;
                    }
                    //WPF-18257 Here added the condition, while the DetailsViewGrid has the Records in view or not.
                    //WPF-28233 Need to navigate when grid has AddNewRow even record is set zero.
                    if ((detailsViewDataRow.DetailsViewDataGrid.GetRecordsCount() != 0
                        || (detailsViewDataRow.DetailsViewDataGrid.HasView && (detailsViewDataRow.DetailsViewDataGrid.FilterRowPosition != FilterRowPosition.None 
                        || detailsViewDataRow.DetailsViewDataGrid.AddNewRowPosition != AddNewRowPosition.None))) 
                        && detailsViewDataRow.DetailsViewDataGrid.SelectionController.HandleKeyDown(args))
                    {
                        //Here the key operations additionally check for the Home, End, PageUP and PageDown key because it will access the ScrollInView method while the selection is in the DetailsViewGrid.
                        //WPF-25695 Scrolling is not processed when we press Up and Down Navigation, because we have skip ScrollInViewFromTop and ScrollInViewFromBottom
                        //for Up, Down and Enter for the issue WRT 5200 this is aplicable for only WinRT not WPF.
                        if (
#if WPF
                            //Need to scroll for tab key also, hene the Key.Tab condition is added.
                            currentKey == Key.Up || currentKey == Key.Down || currentKey == Key.Enter || currentKey == Key.Tab ||
#endif
                            ((currentKey == Key.Home || currentKey == Key.End) && SelectionHelper.CheckControlKeyPressed())
                            || currentKey == Key.PageUp || currentKey == Key.PageDown)

                            this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        return;
                    }
                    else
                    {
                        if ((currentKey == Key.Tab && !SelectionHelper.CheckShiftKeyPressed()) || currentKey == Key.Down || currentKey == Key.Enter)
                        {
                            //var nextRowIndex = GetNextRowIndex(this.CurrentCellManager.CurrentCellIndex.RowIndex);
                            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= this.DataGrid.GetLastNavigatingRowIndex()
                                && this.DataGrid.AddNewRowPosition != AddNewRowPosition.Bottom && this.DataGrid.FilterRowPosition != FilterRowPosition.Bottom)
                                return;
                        }

                        //While in FilterRow is in editing we need to handle the event, hence the below code is added.
                        var selectedDetailsView = this.DataGrid.GetSelectedDetailsViewDataGrid();
                        if (selectedDetailsView != null && selectedDetailsView.SelectionController.CurrentCellManager.IsFilterRow &&
                            selectedDetailsView.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                            return;

                        //WPF-23335 Multiple selection is maintained when pressing space key after editing in DetailsViewGrid with only current cell.
                        //In Multiple SelectionMode, we will not validate any cell when pressing Space key after editing with current cell
                        // we will not reset the IsCurrentCellValidated or IsCurrentRowValided proeprties for unboudn row cells.
                        if (currentKey != Key.Space && (!ValidationHelper.IsCurrentCellValidated || !ValidationHelper.IsCurrentRowValidated))
                            return;

                        if (currentKey == Key.Tab)
                        {
                            var colIndex = SelectionHelper.CheckShiftKeyPressed() ? this.CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection) : this.CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection);
                            this.CurrentCellManager.SetCurrentColumnIndex(colIndex);
                        }
                            //If already the selection is processed for Parent grid, need to skip the process.
                        else if(currentKey == Key.Space)
                        {
                            if (this.DataGrid.SelectedDetailsViewGrid == detailsViewDataRow.DetailsViewDataGrid)
                                return;
                        }
                    }
                }
            }

            if (DataGrid.FlowDirection == FlowDirection.RightToLeft)
                ChangeFlowDirectionKey(ref currentKey);

            ValidationHelper.SetFocusSetBack(false);
            int rowIndex, columnIndex;
            bool isAddNewRow = dataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            bool needToScroll = true;
            var previousCurrentCellIndex = this.CurrentCellManager.CurrentRowColumnIndex;
            int lastNavigatingIndex = this.DataGrid.GetLastNavigatingRowIndex();
            int firstNavigatingIndex = this.DataGrid.GetFirstNavigatingRowIndex();
            switch (currentKey)
            {
                case Key.Escape:
                    {
                        if (CurrentCellManager.HandleKeyNavigation(args, CurrentCellManager.CurrentRowColumnIndex))
                            args.Handled = true;
                    }
                    break;
                case Key.F2:
                    {
                        if (CurrentCellManager.HandleKeyNavigation(args, CurrentCellManager.CurrentRowColumnIndex))
                            args.Handled = true;
                    }
                    break;
                case Key.C:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.DataGrid.GridCopyPaste.Copy();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.V:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.DataGrid.GridCopyPaste.Paste();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.X:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.DataGrid.GridCopyPaste.Cut();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.Insert:
                    {
                        if (SelectionHelper.CheckShiftKeyPressed())
                        {
                            this.DataGrid.GridCopyPaste.Paste();
                            args.Handled = true;
                        }
                    }
                    break;
                case Key.Enter:
                    if (SelectionHelper.CheckControlKeyPressed())
                    {
                        CurrentCellManager.CheckValidationAndEndEdit();
                        return;
                    }
                    else
                        goto case Key.Down;
                case Key.Down:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex > this.DataGrid.GetLastDataRowIndex()
                                ? lastNavigatingIndex : this.DataGrid.GetLastDataRowIndex();
                            //WPF-20498 While navigate the datagrid using Ctrl+Down key returns the GetLastDataRowIndex to ScrollToRowIndex method when using FooterRows and FrozonRows 
                            //so scrolling is not happened properly also out of view records are not come to view,so decrease the FooterRowsCount from lastNavigatingIndex.
                            int lastBodyVisibleIndex = lastNavigatingIndex - dataGrid.FooterRowsCount; ;
                            if (this.dataGrid.FooterRowsCount > 0 && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < lastBodyVisibleIndex && rowIndex > lastBodyVisibleIndex)
                                rowIndex = lastBodyVisibleIndex;
                        }                          
                        else
                            rowIndex = this.GetNextRowIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex);
						//The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0)
                            return;

                        // If the LastRow of the DataGrid is DetailViewIndex means the Scroll Bar distance only apply for the DetailsViewGrid only.so it will Move to the ParentGrid.
                        if (SelectionHelper.CheckControlKeyPressed() && this.dataGrid.DetailsViewManager.HasDetailsView)
                            rowIndex = this.DataGrid.GetLastParentRowIndex(rowIndex);

                        if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing || 
                            (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)))
                        {
                            //The Record is getting out of view when Editing the Record,while the particular column is in sorting. So that the selection will be set to some other row because the edited row will
							//rearranged in Grid. Hence using GetNextRowInfo the next row will stored in rowInfo, to get the exact next rowIndex.                            
                            GridRowInfo rowInfo = null;
                            //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
                            //hence the condition "(this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)" added.
                            if (this.DataGrid.HasView && (this.dataGrid.View.IsEditingItem || (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)) && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping) && !isAddNewRow)
                                rowInfo = this.DataGrid.GetNextRowInfo(rowIndex);

                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                            {
                                args.Handled = true;
                                return;
                            }
                            //Below compararIndex has been used to get the next row index to process after committing the edtited row.
                            int compararIndex = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetLastDataRowIndex() : (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == -1 ? this.DataGrid.GetFirstRowIndex() : this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex));
                            if (compararIndex != rowIndex && rowInfo != null)
                            {
                                //In the below codes, the exact row index to process will be resolved using NodeEntry that have been stored GridRowInfo.
                                if (rowInfo.NodeEntry != null)
                                {
                                    int rIndex = this.dataGrid.ResolveToRowIndex(rowInfo.NodeEntry);
                                    if (rIndex < 0 && !rowInfo.IsDataRow) //Sets last rowIndex to skip CurrentRowIndex being set as -1 When NodeEntry does not exist in topLevelGroups (while grouping) or not in View.Records 
                                        rIndex = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetLastDataRowIndex() : this.GetNextRowIndex(rowInfo.RowIndex);                                    
                                    rowIndex = rIndex;
                                }
                                else if (isAddNewRow || rowInfo.IsAddNewRow)
                                    rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                else if (rowInfo.IsFilterRow)
                                    rowIndex = this.DataGrid.GetFilterRowIndex();

                            }
                        }



                        if (isAddNewRow && (CurrentCellManager.CurrentRowColumnIndex.RowIndex != lastNavigatingIndex
                            || (CurrentCellManager.CurrentRowColumnIndex.RowIndex == lastNavigatingIndex && currentKey == Key.Enter)))
                        {
                            if (this.DataGrid.HasView && dataGrid.View.IsAddingNew)
                            {
                                this.CommitAddNew();
                                //Row index will be changed when pressing the enter key in AddNewRow which is set in Bottom.
                                rowIndex = DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom && currentKey == Key.Enter ? this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() : rowIndex;
                            }
                            // We have committed the new values entered in AddNewRow within ProcessOnAddNewRow method and we have removed the current cell also so there is no need to check CurrentCellManager.CurrentCell as null here 
                            if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple && DataGrid.AddNewRowPosition != AddNewRowPosition.Bottom)
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, new List<object>() { GetGridSelectedRow(CurrentCellManager.CurrentRowColumnIndex.RowIndex) }, SelectionReason.KeyPressed);
                            }
                        }

                        int lastRowIndex = this.DataGrid.GetLastNavigatingRowIndex();

                        //The Below GetLastDataRowIndex() condition is added because the CurrentCell is in the LastRow when pressing the Control + down key the currentcell moves to the Unbound row.
                        if (rowIndex > lastRowIndex || (rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && rowIndex == lastRowIndex)
                            || (SelectionHelper.CheckShiftKeyPressed() && (this.DataGrid.IsAddNewIndex(rowIndex) || this.DataGrid.IsFilterRowIndex(rowIndex)))
                            || (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == this.dataGrid.GetLastDataRowIndex() && SelectionHelper.CheckControlKeyPressed()))
                        {
                            args.Handled = true;
                            return;
                        }                       


                        //Below mehtod will process the Key.Down operation in DetailsViewGrid when the next row index is in DetailsViewIndex.
                        //If the DetailsViewGrid itself handles the key then the further operation will be skipped.
                        if (!this.ProcessDetailsViewKeyDown(args, ref rowIndex, Key.Down) && args.Handled)
                            return;

                        int actualRowIndex = rowIndex;
                        //WPF-28233 - Need to get exact rowIndex when the detailsview grid doesnt has records. Hence the below
                        //code is added after DetailsViewKeyDown.
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex)
                            actualRowIndex = this.CurrentCellManager.GetNextRowIndex(rowIndex);

                        bool needShiftSelection = false;
                        if (this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > this.DataGrid.GetFirstDataRowIndex())
                        {
                            //This code helps to select more than one row when the below procedure is performed
                            //1. In grouping Shift + UP arrow and Expanding any group with Shift key.
                            //2. A pressing Shift + Down need to select whole row in that group.
                            //Another usecase after changing CurrentItem the current cell border is only moved after that if we
                            //use Shift + Down it will not select the current row.
                            Group group = null;
                            if (this.DataGrid.GridModel.HasGroup)
                                group = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)] as Group;
                            if (!this.SelectedRows.Contains(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || (group != null && SelectionHelper.CheckShiftKeyPressed() && this.PressedRowColumnIndex.RowIndex > actualRowIndex && !this.SelectedRows.Contains(actualRowIndex)))
                            {
                                needShiftSelection = true;
                                rowIndex = actualRowIndex;
                            }
                        }

                        var rowColumnIndex = new RowColumnIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            if (dataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && (SelectionHelper.CheckControlKeyPressed() || needShiftSelection) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                            }
                            else
                            {
                                this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);                                
                            }
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        if (CurrentCellManager.IsAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if(isAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
#if !WPF
                        if (this.DataGrid is DetailsViewDataGrid)
                            this.ParentGridScrollInView();   
                        else
#endif

                        this.ScrollToRowIndex(rowIndex);

                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.Down;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Up:
                    {                       
                        if (SelectionHelper.CheckControlKeyPressed())
                        {                                                     
                            rowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetFirstDataRowIndex()
                                       ? firstNavigatingIndex : this.DataGrid.GetFirstDataRowIndex();
                            //WPF-20498 While navigate the datagrid using Ctrl+Up key returns the GetFirstDataRowIndex to ScrollToRowIndex method when using FooterRows and FrozonRows 
                            //so scrolling is not happened properly also out of view records are not come to view,so returns the firstBodyVisibleIndex.                                                                                                         
                            var visibleLines = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLines();                           
                            if (this.dataGrid.FrozenRowsCount > 0 && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > visibleLines.firstBodyVisibleIndex && rowIndex < visibleLines.firstBodyVisibleIndex)                                         
                                rowIndex = visibleLines.firstBodyVisibleIndex;                          
                        }                           
                        else
                            rowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        GridRowInfo rowInfo = null;
                        if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing || 
                            (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)))
                        {
                            //The Record is getting out of view when Editing the Record,while the particular column is in sorting. So that the selection will be set to some other row because the edited row will
                            //rearranged in Grid. Hence using GetNextRowInfo the next row will stored in rowInfo, to get the exact next rowIndex.

                            //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
                            //hence the condition "(this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)" added.
                            if (this.DataGrid.HasView && (this.dataGrid.View.IsEditingItem || (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)) && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping))
                                rowInfo = this.DataGrid.GetPreviousRowInfo(rowIndex);

                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                            {
                                args.Handled = true;
                                return;
                            }
                            //Below index has been used to get the next row index to process after committing the edtited row.
                            int index = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetFirstDataRowIndex() : this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            if (rowIndex != index && rowInfo != null)
                            {
                                //In the below codes, the exact row index to process will be resolved using NodeEntry that have been stored GridRowInfo.
                                if (rowInfo.NodeEntry != null)
                                {
                                    int rIndex = this.dataGrid.ResolveToRowIndex(rowInfo.NodeEntry);
                                    rowIndex = rIndex;
                                }
                                else if (isAddNewRow || rowInfo.IsAddNewRow)
                                    rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                else if (rowInfo.IsFilterRow)
                                    rowIndex = this.DataGrid.GetFilterRowIndex();
                            }
                        }

                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() && rowInfo == null)
                        {
                            args.Handled = true;
                            return;
                        }

                        if (this.DataGrid.HasView && dataGrid.View.IsAddingNew 
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != firstNavigatingIndex)
                        {                            
                            this.CommitAddNew();                                                        
                        }

                        //The Below GetFirstDataRowIndex() condition is added because the CurrentCell is in the First row when pressing the Control + up key the currentcell moves to the Unbound row.
                        if ((rowIndex == this.DataGrid.GetFirstNavigatingRowIndex() && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                            || (SelectionHelper.CheckShiftKeyPressed() && (this.DataGrid.IsAddNewIndex(rowIndex) || this.DataGrid.IsFilterRowIndex(rowIndex)))
                            || (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == this.dataGrid.GetFirstDataRowIndex() && SelectionHelper.CheckControlKeyPressed()))
                        {
                            args.Handled = true;
                            return;
                        }

                        //Below mehtod will process the Key.Up operation in DetailsViewGrid when the previous row index is in DetailsViewIndex.
                        //If the DetailsViewGrid itself handles the key then the further operation will be skipped in the current grd.
                        if (!this.ProcessDetailsViewKeyDown(args, ref rowIndex, Key.Up) && args.Handled)
                            return;

                        int actualRowIndex = rowIndex;
                        //WPF-28233 - Need to get exact rowIndex when the detailsview grid doesnt has records. Hence the below
                        //code is added after DetailsViewKeyDown.
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended &&
                            this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex)
                            actualRowIndex = this.CurrentCellManager.GetPreviousRowIndex(rowIndex);

                        var rowColumnIndex = new RowColumnIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            //Removed the code that we have set selected by resolving the AddNewRowIndes which is unwanted code.
                            if (isAddNewRow)
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);                            
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && SelectionHelper.CheckControlKeyPressed() &&
                               this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                            }
                            else
                            {
                                this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);
                            }
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        if (CurrentCellManager.IsAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
#if !WPF
                        if(this.DataGrid is DetailsViewDataGrid)
                            this.ParentGridScrollInView();
                        else
#endif
                        this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) 
                            && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.Up;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex,this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Right:
                    {
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.dataGrid.GetHeaderIndex())
                            return;

                        Group group = null;
                        if (this.DataGrid.GridModel.HasGroup)
                        {
                            group = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)] as Group;
                        }
                        if (group != null)
                        {
                            this.ExpandOrCollapseGroup(group, true);
                        }
                        else if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow || this.CurrentCellManager.IsFilterRow)
                        {
                            if (!CurrentCellManager.HandleKeyNavigation(args, CurrentCellManager.CurrentRowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }
                        }

                        if (this.SelectedRows.Count > 1 && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !SelectionHelper.CheckShiftKeyPressed())
                        {
                            var currentRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                            this.ClearSelections(true);
                            //Removed the code that we have set selected by resolving the currentrowindex which is unwanted code.
                            //Because the selection already will be in same rowIndex.
                            if (group != null)
                                this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }

                        this.lastPressedKey = Key.Right;
                        args.Handled = true;
                    }
                    break;

                case Key.Left:
                    {
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.dataGrid.GetHeaderIndex())
                            return;
                        Group group = null;

                        var rowColumnIndex = new RowColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, this.CurrentCellManager.GetNextCellIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));

                        if (this.DataGrid.GridModel.HasGroup)
                        {
                            group = this.DataGrid.View.TopLevelGroup.DisplayElements[
                                this.DataGrid.ResolveToRecordIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)] as Group;
                        }
                        if (group != null)
                        {
                            this.ExpandOrCollapseGroup(group, false);
                        }
                        else if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow || this.CurrentCellManager.IsFilterRow)
                        {
                            if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }
                        }

                        if (this.SelectedRows.Count > 1 && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !SelectionHelper.CheckShiftKeyPressed())
                        {
                            var currentRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                            this.ClearSelections(true);
                            //Removed the code that we have set selected by resolving the currentrowindex which is unwanted code.
                            //Because the selection already will be in same rowIndex.
                            if (group != null)
                                this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }
                        this.lastPressedKey = Key.Left;
                        args.Handled = true;
                    }
                    break;

                case Key.PageDown:
                    {
                        //The GetNextPageIndex method returns the rowIndex after reordering the edited data which returns invalid rowIndex
                        //when the edited column is in sorting.
                        rowIndex = this.DataGrid.GetNextPageIndex();
                        if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing || 
                            (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)))
                        {
                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                            {
                                args.Handled = true;
                                return;
                            }
                        }

                        var lastRowIndex = this.DataGrid.GetLastNavigatingRowIndex();

                        //The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0)
                            return;

                        if ((rowIndex > lastRowIndex || (rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && rowIndex == lastRowIndex)) && !this.dataGrid.IsAddNewIndex(rowIndex))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (CurrentCellManager.IsAddNewRow 
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != lastNavigatingIndex)
                        {
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                            if (this.DataGrid.HasView && DataGrid.View.IsAddingNew)
                            {
                                this.CommitAddNew();
                            }
                        }

                        var colIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        var rowColumnIndex = new RowColumnIndex(rowIndex, colIndex);
                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            if (dataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                            }
                            else
                            {
                                this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);                                
                            }
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        this.ScrollToRowIndex(rowIndex);
                        //PressedIndex value not updated properly while Selection happening through the ShiftSelection.
                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.PageDown;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex,this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }                        
                        args.Handled = true;
                    }
                    break;
                case Key.PageUp:
                    {
                        //The GetPreviousPageIndex method returns the rowIndex after reordering the edited data which returns invalid rowIndex
                        //when the edited column is in sorting.
                        rowIndex = this.DataGrid.GetPreviousPageIndex();
                        if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing ||
                            (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)))
                        {
                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                            {
                                args.Handled = true;
                                return;
                            }
                        }

                        //When rowIndex is in -1, it returns after checking in CurrentCellManager, hence here itself added the condition less than to return.
                        if (rowIndex <= this.DataGrid.GetFirstNavigatingRowIndex() && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                        {
                            args.Handled = true;
                            return;
                        }

                        if (this.CurrentCellManager.IsAddNewRow 
                            && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex != firstNavigatingIndex)
                        {
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                            if (this.DataGrid.HasView && dataGrid.View.IsAddingNew)
                            {
                                dataGrid.GridModel.AddNewRowController.CommitAddNew();
                            }
                        }

                        var colIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;                        

                        var rowColumnIndex = new RowColumnIndex(rowIndex, colIndex);

                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            if (dataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                            }
                            else
                            {
                                this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);                                
                            }
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        this.ScrollToRowIndex(rowIndex);
                        //PressedIndex value not updated properly while Selection happening through the ShiftSelection.
                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.PageUp;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex,this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }                         
                        args.Handled = true;
                    }
                    break;
                case Key.Home:
                    {
                        //While AddNewRow is in Editing mode, pressing the Control+End key it will Commit the Edited row through CommitAddNew method.
                        if (this.DataGrid.HasView && DataGrid.View.IsAddingNew && SelectionHelper.CheckControlKeyPressed()
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != firstNavigatingIndex)
                        {
                            this.CommitAddNew();
                        }

                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.dataGrid.GetHeaderIndex())
                            return;
                        int firstDataRowIndex = this.DataGrid.GetFirstDataRowIndex();
                        //Added the condition to check whether the CurrentCell is in AddNewRow and AddNewRowPosition as Top 
                        //while pressing the Control+Home key the selection should be in AddNewRow.
                        //If there is no DataRow, the home key should works only in current row. hence the below firstDataRowIndex condition is added.
                        rowIndex = SelectionHelper.CheckControlKeyPressed() && firstDataRowIndex > 0 && !(CurrentCellManager.CurrentRowColumnIndex.RowIndex <= firstDataRowIndex)
                            ? firstDataRowIndex : this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;                        

                        //The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0)
                            return;
                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        if (this.DataGrid.NavigationMode == NavigationMode.Row && !isAddNewRow)
                            rowIndex = this.DataGrid.GetFirstDataRowIndex();

                        if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                        {
                            var validationResult = rowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex ? CurrentCellManager.RaiseValidationAndEndEdit() : CurrentCellManager.CheckValidationAndEndEdit();
                            if (!validationResult)
                                return;
                        }

                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow)
                        {
                            if (SelectionHelper.CheckControlKeyPressed())
                                columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex(dataGrid.FlowDirection) : CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);
                            else
                                columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex() : CurrentCellManager.GetFirstCellIndex();
                        }
                        else
                            columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

                        if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex)
                        {
                            args.Handled = true;
                            return;
                        }

                       
                        var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);                        
                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        //When pressing Home key, the current cell only have to change but whole selection process is repeated.
                        //Hence the below condition is added and also when pressing Home key with mutilple rows selected, we need to
                        //clear the selection hence the SelectedRows count has been added.
                        if (rowIndex != previousCurrentCellIndex.RowIndex || this.SelectedRows.Count > 1)
                        {
                            if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                            {
                                if (SelectionHelper.CheckShiftKeyPressed() && SelectionHelper.CheckControlKeyPressed() &&
                                    this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                                {
                                    this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                                }
                                else
                                {
                                    needToScroll = this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);                                    
                                }
                            }
                            else
                                this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                            //The Below Codes is added when we Navigate through Control+End when the CurrentCell is in AddNewRow WaterMark has been disappeared.
                            if (this.CurrentCellManager.IsAddNewRow)
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                            else if (isAddNewRow)
                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

                           // Added the Condition to Scrolling should be happen or not. when the Cancel Selection is set as true.
                            //Scrolling happened for the Control + Home key.
                            if (needToScroll)
                                this.ScrollToRowIndex(rowIndex);
                        }
                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        //WPF-20222 - If we cancel the selection in SelectionChanging event, the currentcell is not changed so we don't scroll the horizontal bar
                        //to leftside of the grid while pressing Ctrl+Home, so check we need to scroll or not.
                        if (needToScroll && (this.DataGrid.NavigationMode == NavigationMode.Cell || this.CurrentCellManager.IsAddNewRow || this.CurrentCellManager.IsFilterRow))
                            this.CurrentCellManager.ScrollInViewFromLeft(this.DataGrid.showRowHeader ? 1 : 0);
                        //PressedIndex value not updated properly while Selection happening through the ShiftSelection.
                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.Home;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.End:
                    {
                        //While AddNewRow is in Editing mode, pressing the Control+End key it will Commit the Edited row through CommitAddNew method.
                        if (this.DataGrid.HasView && DataGrid.View.IsAddingNew && SelectionHelper.CheckControlKeyPressed()
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != lastNavigatingIndex)
                            this.CommitAddNew();
                        int lastDataRowIndex = this.DataGrid.GetLastDataRowIndex();
                        //Added the condition to check whether the CurrentCell is in AddNewRow and AddNewRow as Bottom 
                        //while pressing the Control+End key the selection should be in AddNewRow.
                        rowIndex = SelectionHelper.CheckControlKeyPressed() && lastDataRowIndex > 0 
                            && !(CurrentCellManager.CurrentRowColumnIndex.RowIndex >= lastDataRowIndex)
                            ? lastDataRowIndex : CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                        
                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        if (this.DataGrid.NavigationMode == NavigationMode.Row && !isAddNewRow)
                            rowIndex = this.DataGrid.GetLastDataRowIndex();
                        if (this.DataGrid.NavigationMode == NavigationMode.Cell)
                        {
                            var validationResult = rowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex ? CurrentCellManager.RaiseValidationAndEndEdit() : CurrentCellManager.CheckValidationAndEndEdit();
                            if (!validationResult)
                                return;
                        }

                        //The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0 || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.dataGrid.GetHeaderIndex())
                            return;

                        // If the LastRow of the DataGrid is DetailViewIndex means the Scroll Bar distance only apply for the DetailsViewGrid only.so it will Move to the ParentGrid.
                        if (SelectionHelper.CheckControlKeyPressed() && this.dataGrid.DetailsViewManager.HasDetailsView)
                            rowIndex = this.DataGrid.GetLastParentRowIndex(rowIndex);

                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow)
                            columnIndex =  this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetLastColumnIndex() : CurrentCellManager.GetLastCellIndex();
                        else
                            columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

                        if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow)
                        {
                            if (dataGrid.FlowDirection == FlowDirection.RightToLeft && SelectionHelper.CheckControlKeyPressed())
                                columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex() : CurrentCellManager.GetFirstCellIndex();                            
                        }
                        var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);                        
                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        //When pressing End key, the current cell only have to change but whole selection process is repeated.
                        //Hence the below condition is added and also when pressing End key with mutilple rows selected, we need to
                        //clear the selection hence the SelectedRows count has been added.
                        if (rowIndex != previousCurrentCellIndex.RowIndex || this.SelectedRows.Count > 1)
                        {
                            if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                            {
                                if (SelectionHelper.CheckShiftKeyPressed() && SelectionHelper.CheckControlKeyPressed() &&
                                    this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                                    this.ProcessShiftSelection(rowIndex, SelectionReason.KeyPressed);
                                else
                                {
                                    needToScroll = this.ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);                                    
                                }
                            }
                            else
                            this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        //The Below Codes is added when we Navigate through Control+Home when the CurrentCell is in AddNewRow WaterMark has been disappeared.
                        if (this.CurrentCellManager.IsAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

                            // Added the Condition to Scrolling should be happen or not. when the Cancel Selection is set as true.
                            //Scrolling happened for the Control + End key.
                            if (needToScroll)
                                this.ScrollToRowIndex(rowIndex);
                        }
                        //The below condition is Added while the CurrentCell is in AddNewRow and in Editing mode and Navigation mode is Row
                        //while pressing End key it should not goes to the last row.
                        //WPF-20222 - If we cancel the selection in SelectionChanging event, the currentcell is not changed so we don't scroll the horizontal bar based on
                        //ColumnIndex towards right while pressing Ctrl+End, so check we need to scroll or not.
                        if (needToScroll && (this.DataGrid.NavigationMode == NavigationMode.Cell || this.CurrentCellManager.IsAddNewRow || this.CurrentCellManager.IsFilterRow))
                            this.CurrentCellManager.ScrollInViewFromRight(columnIndex);
                        //PressedIndex value not updated properly while Selection happening through the ShiftSelection.
                        if (SelectionHelper.CheckShiftKeyPressed() && !this.DataGrid.IsInDetailsViewIndex(rowIndex) && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            this.lastPressedKey = Key.End;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }                       
                        args.Handled = true;
                    }
                    break;
                case Key.Delete:
                    args.Handled = RemoveRows();
                    break;
                case Key.A:
                    {
                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            this.SelectAll();
                            this.lastPressedKey = Key.A;
                            args.Handled = true;
                        }
                    }
                    break;

                case Key.Tab:
                    {
                        if (CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex() && !(this.DataGrid is DetailsViewDataGrid))
                            return;
                        this.lastPressedKey = Key.Tab;
                        GridRowInfo rowInfo = null;
                        if (this.DataGrid.NavigationMode == NavigationMode.Cell || CurrentCellManager.IsAddNewRow || CurrentCellManager.IsFilterRow)
                        {
                            if (!CurrentCellManager.HandleKeyNavigation(args, CurrentCellManager.CurrentRowColumnIndex))
                            {
                                if (args.Handled)
                                    return;
                                rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                                if (SelectionHelper.CheckShiftKeyPressed())
                                {
                                    if (rowIndex < 0)
                                        rowIndex = this.DataGrid.GetLastNavigatingRowIndex();
                                    else
                                        rowIndex = this.GetPreviousRowIndex(rowIndex);
                                    //The Grid is scrolled to LastColumn when the new rowIndex is in Summary or in Group row.
                                    if (dataGrid.IsAddNewIndex(rowIndex))
                                        columnIndex = dataGrid.GetLastColumnIndex(dataGrid.FlowDirection);
                                    else
                                        columnIndex = !this.DataGrid.IsInSummarryRow(rowIndex) ? CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection) : CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);

                                    if (!this.DataGrid.ValidationHelper.RaiseRowValidate(CurrentCellManager.CurrentRowColumnIndex))
                                    {
                                        args.Handled = true;
                                        return;
                                    }
                                    if (this.DataGrid.HasView && DataGrid.View.IsEditingItem)
                                    {
                                        //The Record is getting out of view when Editing the Record,while the particular column is in sorting. So that the selection will be set to some other row because the edited row will
                                        //rearranged in Grid. Hence using GetNextRowInfo the next row will stored in rowInfo, to get the exact next rowIndex.
                                        if (this.dataGrid.View.IsEditingItem && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping))
                                            rowInfo = this.DataGrid.GetPreviousRowInfo(rowIndex);
                                        DataGrid.View.CommitEdit();
                                        //Below index has been used to get the next row index to process after committing the edtited row.
                                        int index = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetFirstNavigatingRowIndex() : this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                                        if (rowIndex != index && rowInfo != null)
                                        {
                                            //In the below codes, the exact row index to process will be resolved using NodeEntry that have been stored GridRowInfo.
                                            if (rowInfo.NodeEntry != null )
                                            {
                                                int rIndex = this.dataGrid.ResolveToRowIndex(rowInfo.NodeEntry);
                                                rowIndex = rIndex;
                                            }
                                            else if (isAddNewRow || rowInfo.IsAddNewRow)
                                                rowIndex = dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                            else if (rowInfo.IsFilterRow)
                                                rowIndex = this.DataGrid.GetFilterRowIndex();
                                        }
                                    }
                                    if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                                        this.CommitAddNew(firstNavigatingIndex != DataGrid.GridModel.addNewRowController.GetAddNewRowIndex());

                                    if (rowIndex <= this.DataGrid.GetFirstNavigatingRowIndex() && CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex && !(this.DataGrid is DetailsViewDataGrid))
                                        return;

                                    //When Records count is zero and the AddNewRow position is bottom and while pressing Shift+Tab the,
                                    //the AddNewRowMode is changed hence the below code is used after the above condition.
                                    if (isAddNewRow)
                                    {
                                        //WPF-18329 AddNewRow watermark disappear when pressing tab key in AddNewRow.
                                        if (dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                                            this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                        else if (dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() == this.DataGrid.GetFirstNavigatingRowIndex())
                                        {
                                            rowIndex = dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                            columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                                        }
                                    }

                                    //Below mehtod will process the Shift + Key.Tab operation in DetailsViewGrid when the previous row index is in DetailsViewIndex.
                                    //If the DetailsViewGrid itself handles the key then the further operation will be skipped in the current grd.
                                    if (!this.ProcessDetailsViewKeyDown(args, ref rowIndex, Key.Tab) && args.Handled)
                                        return;
                                }
                                else
                                {
                                    if (CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0)
                                        rowIndex = this.DataGrid.GetFirstNavigatingRowIndex();
                                    else
                                        rowIndex = this.GetNextRowIndex(rowIndex);
                                    columnIndex = dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetFirstColumnIndex(dataGrid.FlowDirection) : CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);
                                    if (!this.DataGrid.ValidationHelper.RaiseRowValidate(CurrentCellManager.CurrentRowColumnIndex))
                                    {
                                        args.Handled = true;
                                        return;
                                    }
                                    if (this.DataGrid.HasView && DataGrid.View.IsEditingItem)
                                    {
                                        //The Record is getting out of view when Editing the Record,while the particular column is in sorting. So that the selection will be set to some other row because the edited row will
                                        //rearranged in Grid. Hence using GetNextRowInfo the next row will stored in rowInfo, to get the exact next rowIndex.
                                        if (this.dataGrid.View.IsEditingItem && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping))
                                            rowInfo = this.DataGrid.GetNextRowInfo(rowIndex);
                                        DataGrid.View.CommitEdit();
                                        //Below index has been used to get the next row index to process after committing the edtited row.
                                        int index = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetLastNavigatingRowIndex() : this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                                        if (index != rowIndex && rowInfo != null)
                                        {
                                            //In the below codes, the exact row index to process will be resolved using NodeEntry that have been stored GridRowInfo.
                                            if (rowInfo.NodeEntry != null)
                                            {
                                                int rIndex = this.dataGrid.ResolveToRowIndex(rowInfo.NodeEntry);
                                                rowIndex = rIndex;
                                            }
                                            else if (isAddNewRow || rowInfo.IsAddNewRow)
                                                rowIndex = dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                            else if (rowInfo.IsFilterRow)
                                                rowIndex = this.DataGrid.GetFilterRowIndex();
                                        }
                                    }

                                    var isAddingNew = this.DataGrid.View.IsAddingNew;
                                    if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                                        this.CommitAddNew(dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() != lastNavigatingIndex);

                                    if (!this.DataGrid.IsAddNewIndex(rowIndex) && rowIndex >= this.DataGrid.GetLastNavigatingRowIndex() && (CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex || this.DataGrid.VisualContainer.RowCount == rowIndex || rowIndex > this.DataGrid.GetLastNavigatingRowIndex()) && !(this.DataGrid is DetailsViewDataGrid))
                                        return;

                                    //When Records count is zero and the AddNewRow position is top and while pressing Tab the,
                                    //the AddNewRowMode is changed hence the below code is used after the above condition.
                                    if (isAddNewRow)
                                    {
                                        //WPF-18329 AddNewRow watermark disappear when pressing tab key in AddNewRow.
                                        if (dataGrid.GetAddNewRowPosition() == AddNewRowPosition.Top)
                                        {
                                            this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                        }
                                        else if (dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                                        {
                                            rowIndex = dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                                            columnIndex = isAddingNew ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                                        }
                                    }

                                    //Below mehtod will process the Key.Tab operation in DetailsViewGrid when the previous row index is in DetailsViewIndex.
                                    //If the DetailsViewGrid itself handles the key then the further operation will be skipped in the current grd.
                                    if (!this.ProcessDetailsViewKeyDown(args, ref rowIndex, Key.Tab) && args.Handled)
                                        return;

                                }

                                if (previousCurrentCellIndex.RowIndex != this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() && rowInfo != null && !rowInfo.IsAddNewRow)
                                    return;

                                var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);

                                if (this.DataGrid.NavigationMode == NavigationMode.Cell || isAddNewRow || this.CurrentCellManager.IsFilterRow)
                                {
                                    if (!CurrentCellManager.ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                                        return;
                                }
                                else
                                    this.CurrentCellManager.UpdateGridProperties(rowColumnIndex);

                                this.DataGrid.HideRowFocusBorder();
                                if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                {
                                    //When we Cancel the selection through the SelectionChanging Event we have to check the ProcessSelection returned value.
                                    //Because the next or previuos row is selected or not.
                                    needToScroll = ProcessSelection(rowIndex, previousCurrentCellIndex, SelectionReason.KeyPressed);
                                    this.SetPressedIndex(rowColumnIndex);
                                }
                                else
                                    this.DataGrid.ShowRowFocusBorder(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                                if (dataGrid.IsAddNewIndex(rowIndex))
                                    dataGrid.GridModel.AddNewRowController.SetAddNewMode(true);

                                if (SelectionHelper.CheckShiftKeyPressed())
                                {
                                    //If the previous row is not selected then avoid the ScrollInViewFromLeft method
                                    if (needToScroll)
                                        this.CurrentCellManager.ScrollInViewFromLeft(columnIndex);
                                }
                                else
                                {
                                    columnIndex = columnIndex == this.CurrentCellManager.GetFirstCellIndex() ? (this.DataGrid.showRowHeader ? 1 : 0): columnIndex;
                                    //If the NextRow is Not selected then avoid the ScrollInViewFromRight method.
                                    if (needToScroll)
                                        CurrentCellManager.ScrollInViewFromRight(columnIndex);
                                }
                                if (!(this.DataGrid is DetailsViewDataGrid))
                                    this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            }
                            this.lastPressedKey = Key.None;
                            args.Handled = true;
                        }
                        break;
                    }
                case Key.Space:
                    {
                        if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > this.DataGrid.GetHeaderIndex())
                        {
                            var addedItems = new List<object>();
                            var removedItems = new List<object>();
                            var currentRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                            if (currentRow != null)
                            {
                                //When selection in DetailsViewGrid and the current cell in another DetailsViewGrid, the selected DetailsViewGrid
                                //selection is not removed in parent grid. Hence the below flag is introduced.
                                bool needToRemove = currentRow is DetailsViewDataRow || this.DataGrid.SelectedDetailsViewGrid != null;
                                //To clear the DetailsViewGrid selection when pressing space in another row.
                                if (this.DataGrid.SelectedDetailsViewGrid != null)
                                {
                                    if (!ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                                        return;
                                    this.DataGrid.SelectedDetailsViewGrid = null;
                                }

                                //To set the SelectedDetailsViewGrid when pressing in DetailsViewGrid.
                                if (this.DataGrid.IsInDetailsViewIndex(currentRow.RowIndex) && this.DataGrid.SelectedDetailsViewGrid == null)
                                {
                                    var dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == currentRow.RowIndex);
                                    if (dataRow is DetailsViewDataRow)
                                    {
                                        this.DataGrid.SelectedDetailsViewGrid = (dataRow as DetailsViewDataRow).DetailsViewDataGrid;
                                    }
                                }

                                if (currentRow.IsSelectedRow && !(currentRow is DetailsViewDataRow))
                                {
                                    removedItems.Add(this.SelectedRows.Find(currentRow.RowIndex));
                                    if (RaiseSelectionChanging(addedItems, removedItems))
                                        return;
                                    if (this.DataGrid.NavigationMode == NavigationMode.Row && DataGrid.IsAddNewIndex(currentRow.RowIndex))
                                        CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                                    RemoveSelection(currentRow.RowIndex, removedItems, SelectionReason.KeyPressed);
                                    this.DataGrid.ShowRowFocusBorder(CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                                }
                                else
                                {
                                    //When the selection is in DetailsViewGrid the Selection not be removed, hence the needToRemove property is used.
                                    if (needToRemove)
                                    {
                                        removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                                        addedItems.Add(GetGridSelectedRow(currentRow.RowIndex));
                                    }
                                    else
                                        addedItems.Add(GetGridSelectedRow(currentRow.RowIndex));

                                    if (RaiseSelectionChanging(addedItems, removedItems))
                                        return;
                                    if (this.DataGrid.NavigationMode == NavigationMode.Row && DataGrid.IsAddNewIndex(currentRow.RowIndex))
                                        CurrentCellManager.SelectCurrentCell(new RowColumnIndex(currentRow.RowIndex, CurrentCellManager.GetFirstCellIndex()));

                                    if (currentRow is DetailsViewDataRow)
                                        RemoveSelection(currentRow.RowIndex, removedItems, SelectionReason.KeyPressed);

                                    AddSelection(addedItems, SelectionReason.KeyPressed);
                                    this.DataGrid.HideRowFocusBorder();
                                }
                                RaiseSelectionChanged(addedItems, removedItems);
                                //This condition is added to Check and UnCheck the check box when pressing space key in CheckBoxColumn.
                                if (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)
                                    args.Handled = false;
                                else
                                    args.Handled = true;
                            }
                        }
                        break;
                    }

            }
        }

        /// <summary>
        /// Processes the row selection while navigating the cell from parent grid to child grid .
        /// </summary>
        /// <param name="args">
        /// Contains data for key navigation. 
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index where the key navigation occurs.
        /// </param>
        /// <param name="processKey">
        /// The corresponding key that was processed.
        /// </param>
        /// <param name="returnIndex">
        /// The returnIndex to select the next row when DetailsViewDataGrid doesn't handle the cell selection.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the pressed key was processed in DetailsViewDataGrid; otherwise, <b>false</b>.
        /// </returns>
        protected bool ProcessDetailsViewIndex(KeyEventArgs args, int rowIndex, Key processKey, out int returnIndex)
        {
            if (this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowIndex))
            {
                var dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex && row.IsEnsured);
                if (dataRow == null)
                {
                    this.DataGrid.DetailsViewManager.BringIntoView(rowIndex);
                    dataRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == rowIndex);
                }
                if (dataRow is DetailsViewDataRow)
                {
                    var detailsViewDataRow = dataRow as DetailsViewDataRow;
                    //When view is null, we have to process selection in UnBoundRow, hence GetRecordsCount method is used instead of checking view.
                    var result = (detailsViewDataRow.DetailsViewDataGrid.GetRecordsCount() > 0
                        || (detailsViewDataRow.DetailsViewDataGrid.HasView && (detailsViewDataRow.DetailsViewDataGrid.FilterRowPosition != FilterRowPosition.None 
                        || detailsViewDataRow.DetailsViewDataGrid.AddNewRowPosition != AddNewRowPosition.None))) 
                        ? detailsViewDataRow.DetailsViewDataGrid.SelectionController.HandleKeyDown(args) : false;
                    if (result)
                    {
                        returnIndex = rowIndex;
                        //SelectedDetailsViewGrid should set when navigatin in Multiple selection.
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple || (this.DataGrid.SelectionMode == GridSelectionMode.Multiple && processKey == Key.Space))
                            this.DataGrid.SelectedDetailsViewGrid = detailsViewDataRow.DetailsViewDataGrid;
                        return true;
                    }
                    else
                    {
                        int currentIndex = rowIndex;
                        if (processKey == Key.Up || (processKey == Key.Tab && SelectionHelper.CheckShiftKeyPressed()))
                            rowIndex = this.GetPreviousRowIndex(rowIndex);
                        else if (processKey == Key.Down || processKey == Key.Tab)
                            rowIndex = this.GetNextRowIndex(rowIndex);
                        //Here the rowIndex is checked with the LastNavigatingRowIndex instead of GetLastDataRowIndex because the last detailsviewgrid is having Empty records means the lastRowIndex should be the previous RowcolumnIndex.
                        if ((rowIndex == currentIndex && rowIndex == this.DataGrid.GetLastNavigatingRowIndex()) || ProcessDetailsViewIndex(args, rowIndex, processKey, out returnIndex))
                        {
                            returnIndex = rowIndex;
                            return true;
                        }
                        else
                            this.DataGrid.SelectedDetailsViewGrid = null;
                    }
                }
            }
            returnIndex = rowIndex;
            return false;
        }
      
        /// <summary>
        /// Processes the row selection when the keyboard interactions that are performed in DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data for key navigation. 
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index where the key navigation occurs.
        /// </param>
        /// <param name="processKey">
        /// The corresponding key that was processed.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the keyboard interaction that are processed in DetailsViewDataGrid; otherwise, <b>false</b>.
        /// </returns>      
        protected bool ProcessDetailsViewKeyDown(KeyEventArgs args,ref int rowIndex, Key processKey)
        {
            if (this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowIndex) && !(rowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex && this.DataGrid.SelectionMode == GridSelectionMode.Extended && SelectionHelper.CheckShiftKeyPressed()))
            {
                int outRowIndex;
                if (!ProcessDetailsViewIndex(args, rowIndex, processKey, out outRowIndex))
                {
                    rowIndex = outRowIndex;
                    this.DataGrid.SelectedDetailsViewGrid = null;
                    return false;
                }
                else
                    return true;
            }
            //Selection should not be cleared when navigate in DetailsView because the current cell only traversed over the grid.
            else if (this.DataGrid.SelectedDetailsViewGrid != null && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
            {
                this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid);
                this.DataGrid.SelectedDetailsViewGrid = null;
            }
            return false;
        }

        #endregion

        #region Grid Operations Methods
       
        /// <summary>
        /// Processes the row selection while navigating from one page in to another page in SfDataPager.
        /// </summary>
        protected override void ProcessOnPageChanged()
        {
            ClearSelectedDetailsViewGrid();
            this.ClearSelections(false);
        }
      
        /// <summary>
        /// Processes the row selection when filtering is applied in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to filtering operation.
        /// </param>
        protected override void ProcessOnFilterApplied(GridFilteringEventArgs args)
        {
            if (!this.DataGrid.HasView)
                return;
            // isValidRow bool flag is used to check the currentItem is Maintaining in the Filtered Records or AddNewRow.
            //if the CurrentCell is Maintaining in the Filtered Records the Selection will not give to that Row.
            bool isValidRow = this.dataGrid.CurrentItem != null;
            bool isInEdit=CurrentCellManager.IsFilterRow && CurrentCellManager.HasCurrentCell
                && CurrentCellManager.CurrentCell.IsEditing;

            if (this.DataGrid is DetailsViewDataGrid && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex())
                return;
            if (this.DataGrid.HasView && dataGrid.View.IsAddingNew)
                this.CommitAddNew(false);

            ClearSelectedDetailsViewGrid();

            var removedItems = new List<object>();
            RefreshSelectedItems(ref removedItems);
            if (isInEdit)
                this.CurrentCellManager.SetCurrentRowIndex(this.DataGrid.GetFilterRowIndex());
            else
                UpdateCurrentRowIndex(canFocusGrid: !(args.PopupIsOpen || isInEdit) && !args.IsProgrammatic);

            //WPF 18781- Checked with the Records count value if it is 0.then there is no Record so we can return the value.
            if (!this.CurrentCellManager.IsAddNewRow && !this.CurrentCellManager.IsFilterRow && this.DataGrid.GetRecordsCount() == 0)
                return;

            //WPF 18238 - isValidRow added to retain old CurrentItem in case if it is in View, Otherwise select first item as Current/SelectedItem if SelectedRows empty. Otherwise selects items from SelectedRows
            //WPF 18781 - removedItems condtion added to select first item as selected item when the caption/group summary selected.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex() && (isValidRow || removedItems.Count > 0))
            {
                // The Below condition is Added to check the SelectedItem is Maintaining in the Grid or Not.If it is Maintaining the selection will Move to the SelectedItem while the CurrentItem is Filtered.
                var rowIndex = -1;
                if(this.SelectedRows.Any())
                {
                    var selectedRow = this.SelectedRows.FirstOrDefault();
                    if (selectedRow.IsUnBoundRow)
                        rowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(selectedRow.GridUnboundRowInfo);
                    else
                        rowIndex = this.DataGrid.ResolveToRowIndex(selectedRow.RowData);
                }
                else
                    rowIndex = this.DataGrid.GetFirstDataRowIndex();

                //UWP-175(Issue 1)
                if (rowIndex == -1)
                    return;

                // This Condition is Added while the CurrentCell is in AddNewRow we have to Maintain the CurrrentCell Not Add to Selection.
                //WPF 18781 - removedItems condition also checked with the Grouping cases while the selection is maintained in the Caption/group Summary Row after applying the filter it could maintain the selection.                
                if (isValidRow || removedItems.Count > 0)
                    // WPF-35275 - Focus maintained in grid based on filtering applied by programmatically or UI interaction.
                    this.ResetSelection(rowIndex, removedItems, setFocuForGrid: !args.PopupIsOpen && !args.IsProgrammatic);
                else
                {
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                }               
            }
            this.CurrentCellManager.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        /// <summary>
        /// Processes the row selection when the filter popup is opened.
        /// </summary>
        /// <remarks>
        /// This method refreshes the row selection while opening the filter popup in SfDataGrid.
        /// </remarks>
        protected override void ProcessOnFilterPopupOpened()
        {
            int rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;

            if (this.dataGrid.View == null)
                return;

            if (this.dataGrid.NavigationMode == NavigationMode.Cell || dataGrid.IsAddNewIndex(rowIndex))
            {
                if (!this.CurrentCellManager.CheckValidationAndEndEdit(true))
                {
                    ValidationHelper.SetFocusSetBack(false);
                    return;
                }
            }
            if (this.DataGrid.HasView && this.dataGrid.View.IsAddingNew && dataGrid.IsAddNewIndex(rowIndex))
            {
                this.CommitAddNew(false);
                UpdateCurrentRowIndex();
                RefreshSelectedRows();
            }
        }
     
        /// <summary>
        /// Processes the row selection when the column is sorted in SfDataGrid.
        /// </summary>
        /// <param name="sortcolumnHandle">
        /// Contains information related to the sorting action.
        /// </param>        
        /// <remarks>
        /// This method refreshes the selection while sorting the column in SfDataGrid and clear the selection in summary rows.
        /// </remarks>
        protected override void ProcessOnSortChanged(SortColumnChangedHandle sortcolumnHandle)
        {
            ClearSelectedDetailsViewGrid();

            var removedItems = new List<object>();
            this.RefreshSelectedItems(ref removedItems);
            this.SuspendUpdates();
            UpdateCurrentRowIndex(canFocusGrid: !sortcolumnHandle.IsProgrammatic);

            // WPF-20152 - Selection cleared and moved to first row when the View is VirtualizingCollectionView
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() && (this.CurrentCellManager.IsAddNewRow || this.SelectedRows.Count > 0 || this.DataGrid.View is VirtualizingCollectionView))
            {
                int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : DataGrid.GetFirstDataRowIndex();
                rowIndex = this.CurrentCellManager.IsAddNewRow ? this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() : rowIndex;
                ResetSelection(rowIndex, removedItems);
            }

            if (this.CurrentCellManager.IsAddNewRow)
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);

            if (sortcolumnHandle.ScrollToCurrentItem)
                this.CurrentCellManager.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.ResumeUpdates();
        }
       
        /// <summary>
        /// Processes the row selection when the column is grouped in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the grouping operation.
        /// </param>
        /// <remarks>
        /// This method refreshes the selection while grouping the column in SfDataGrid.
        /// </remarks>
        protected override void ProcessOnGroupChanged(GridGroupingEventArgs args)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.None)
                return;

            ClearSelectedDetailsViewGrid();

            var removedItems = new List<object>();
            this.RefreshSelectedItems(ref removedItems);
            //WPF-24301- If we group by the column when selection is in CaptionSummary row,
            //we need to scroll the HorizontalBar to ColumnIndex of CaptionSummary.So introduced 
            //new flag for store columnindex to scroll.
            int columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

            if ((this.DataGrid.GetRecordsCount() != 0 || this.CurrentCellManager.IsAddNewRow || this.CurrentCellManager.IsFilterRow))
            {
                //Coulumn Index is not updated wrongly when grouping any column without selection any cell, hence the below condition is
                //added and the first cell index is set to the current column index.

                // When we set the ShowColumnWhenGrouped as False and Group with selection on first column, 
                // GetFirstColumnIndex and GetFirstCellIndex which returns only the non hidden columns. We have used
                // this method to check the whether the CurrentColumnIndex is not -1 hence we used direct value.
                // WPF-23735- Current cell moved to next cell in cell selection when ShowColumnWhenGrouped is false
                if (CurrentCellManager.CurrentRowColumnIndex.ColumnIndex >= 0 && args != null)
                {
                    switch (args.CollectionChangedEventArgs.Action)
                    {
                        case NotifyCollectionChangedAction.Add:
                            CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentRowColumnIndex.ColumnIndex +
                                                                     args.CollectionChangedEventArgs.NewItems.Count);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentRowColumnIndex.ColumnIndex -
                                                                     args.CollectionChangedEventArgs.OldItems.Count);
                            break;
                        //WPF-20955 - If we clear GroupColumnDescriptions means the NotifyCollectionChangedAction is Reset,
                        //we need to set the CurrentCell based on CurrentColumn index.
                        case NotifyCollectionChangedAction.Reset:
                            if (CurrentCellManager.HasCurrentCell)
                                CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentCell.ColumnIndex);
                            break;
                    }
                    columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                }
                //Added the Condition Because the CurrentCell is maintained while Expanding all the Groups at run-Time.
                else if (CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.dataGrid.GetFirstColumnIndex()) 
                {
                    //WPF-24301- if the Selection is in CaptionSummary, CurrentColumnIndex is -1 so we 
                    // need to move the scroll bar to 0th index or 1st index based on ShowRowHeader
                    columnIndex = DataGrid.showRowHeader ? 1 : 0;
                    CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.GetFirstCellIndex());
                }
                UpdateCurrentRowIndex(canFocusGrid:args!=null && !args.IsProgrammatic);
                //WPF 17907 Here we have to ignore the first GroupCaption Summary Row Selection for DetailsViewGrid.

            if (this.DataGrid.CurrentItem == null && this.DataGrid.GetRecordsCount(false) != 0 &&
                    !this.CurrentCellManager.IsInNonDataRows() 
                    && !(this.DataGrid is DetailsViewDataGrid))

                this.ResetSelection(this.DataGrid.GetFirstDataRowIndex(), removedItems,args!=null && !args.IsProgrammatic);
            }

            //When records count is zero and after grouping the AddNewRow mode is in Normal hence the below code is used after the
            //condition of recordCount > 0
            if (this.CurrentCellManager.IsAddNewRow)
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);

            //WPF-24301- Scroll the HorizontalScroll bar based on RowColumn index
            this.CurrentCellManager.ScrollInView(new RowColumnIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex, columnIndex));
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        /// <summary>
        /// Processes the row selection when the records is pasted from the clipboard into SfDataGrid.
        /// </summary>
        /// <param name="records">
        /// Contains the list of records that is pasted into SfDataGrid.
        /// </param>
        protected override void ProcessOnPaste(List<object> records)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.None)
                return;
            if (this.DataGrid.SelectionMode != GridSelectionMode.Single)
            {
                var addedItems = new List<object>();
                for (int i = 0; i < records.Count;i++ )
                {
                   if(this.GetGridSelectedRow(records[i]) !=null)
                   addedItems.Add(this.GetGridSelectedRow(records[i]));
                }
                this.AddSelection(addedItems, SelectionReason.KeyPressed);
            }
            else
            {
                if (this.DataGrid.HasView && this.DataGrid.View.FilterRecord(records.FirstOrDefault()))
                    this.DataGrid.SelectedItem = records.FirstOrDefault();
            }
        }

        #endregion

        #region Selection PropertyChanged Method

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedMode"/> property changes.
        /// </param>
        protected override void ProcessSelectionModeChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (handle.NewValue == null)
                return;

            this.SuspendUpdates();
            if (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.IsEditing)
                CurrentCellManager.EndEdit();
            this.DataGrid.HideRowFocusBorder();

            switch ((GridSelectionMode)handle.NewValue)
            {
                case GridSelectionMode.None:
                    if (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                    {
                        if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                            this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);
                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                    }
                    this.ClearSelections(false);
                    this.DataGrid.UpdateRowHeaderState();
                    this.DataGrid.RowGenerator.ResetSelection();
                    break;
                case GridSelectionMode.Single:
                    //After deleting all rows and changing selection mode throws exception 
                    //hence added the condition to check the current row is selected or not.
                    //When DetailsViewGrid records is zero then the FirstNavigatingRowIndex will be -1 hence the condition is checked with zero.
                    if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= 0)
                    {
                        //In Multiple Selection mode, when navigating from DetailsView to another DetailsView and changing the Selection
                        //mode, the SelectedDetailsView is not updated hence the below code is added.
                        if (this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                        {
                            var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            if (this.DataGrid.SelectedDetailsViewGrid != detailsViewGrid)
                                this.DataGrid.SelectedDetailsViewGrid = detailsViewGrid;
                        }
                        this.ClearSelections(true, false);
                    }
                    else
                        this.ClearSelections(false);
                    break;
                case GridSelectionMode.Extended:
                    {
                        var row = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                        if (row != null && !row.IsSelectedRow)
                        {
                            //The Below Codes will check the Selection, which is in the DetailsViewGrid when the CurrentCell is in ParentGrid and vice versa.
                            //While Changing the Selection Mode as Extended from Multiple.
                            if (this.CurrentCellManager.HasCurrentCell)
                            {
                                if (this.DataGrid.SelectedDetailsViewGrid != null)
                                {
                                    this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid);
                                }
                                else if (this.DataGrid is DetailsViewDataGrid)
                                {
                                    var childGrid = this.dataGrid;
                                    while (childGrid.NotifyListener != null)
                                    {
                                        var parentDataGrid = childGrid.NotifyListener.GetParentDataGrid();
                                        if (parentDataGrid.SelectedDetailsViewGrid != childGrid)
                                        {
                                            if(parentDataGrid.SelectedDetailsViewGrid != null)
                                                this.ClearDetailsViewGridSelections(parentDataGrid.SelectedDetailsViewGrid);
                                            parentDataGrid.SelectedDetailsViewGrid = childGrid;
                                            parentDataGrid.ClearSelections(true);
                                        }
                                        childGrid = parentDataGrid;
                                    }
                                }
                            }

                            row.IsSelectedRow = true;
                            var rowInfo = GetGridSelectedRow(row.RowIndex);
                            if (rowInfo != null)
                            {
                                this.SelectedRows.Add(rowInfo);
                                if (row.RowData != null && !(row.RowData is Group) && !(row.RowData is SummaryRecordEntry))
                                    this.DataGrid.SelectedItems.Add(row.RowData);
                            }
                        }
                    }
                    break;
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> property value changes.
        /// </param>
        protected override void ProcessNavigationModeChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            this.ClearSelections(false);
            if (this.DataGrid.HasView)
            {
                SuspendUpdates();
                this.DataGrid.View.MoveCurrentToPosition(this.DataGrid.ResolveToRecordIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex));
                ResumeUpdates();
            }
            this.DataGrid.UpdateRowHeaderState();
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </param>
        protected internal override void ProcessCurrentItemChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (IsSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None)
                return;

            var rowIndex = this.DataGrid.ResolveToRowIndex(handle.NewValue);
            if (rowIndex < this.DataGrid.GetHeaderIndex())
                return;
            
            //While changing the CurrentItem the EndEdit is not called so called the EndEdit after checking the EditItem 
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            //We have to clear the nested grid selection when selection mode is single and have to remove the current cell 
            //when selection mode is multiple or extended because when changing the CurrentItem in Extended or Multiple selection mode
            //the current cell only moved to corresponding cell.
            if (this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
            {
                if (this.DataGrid.SelectionMode == GridSelectionMode.Single)
                {
                    if (!ClearSelectedDetailsViewGrid())
                        return;
                }
                else
                {
                    this.RemoveChildGridCurrentCell();
                }
            }

            if (this.DataGrid is DetailsViewDataGrid)
            {
                this.ProcessDetailsViewCurrentItemChanged();
            }

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            var oldCurrentCellColumnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() ? CurrentCellManager.GetFirstCellIndex() : CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, oldCurrentCellColumnIndex), false);
            if(this.dataGrid.SelectionMode== GridSelectionMode.Single)
            {
                if (SelectedRows.Count > 0)
                    removedItems.Add(this.SelectedRows.FirstOrDefault());
                
                RemoveSelection(CurrentCellManager.CurrentRowColumnIndex.RowIndex, removedItems, SelectionReason.CollectionChanged);
                addedItems.Add(this.GetGridSelectedRow(handle.NewValue));
                
                AddSelection(addedItems, SelectionReason.CollectionChanged);
                this.CurrentCellManager.SetCurrentRowIndex(rowIndex);
                this.RaiseSelectionChanged(addedItems, removedItems);
            }
            else if(this.DataGrid.NavigationMode == NavigationMode.Row)
            {
                this.DataGrid.HideRowFocusBorder();
                //While Setting the CurrentItem through the Button Click FocusBorder has been set if it is Selected.
                //So here we ave changed the row.IsFocused instead of ShowRowFocusBorder.
                this.DataGrid.ShowRowFocusBorder(rowIndex);
            }
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex"/> property value changes.
        /// </param>
        protected internal override void ProcessSelectedIndexChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            var newValue = (int)handle.NewValue;
            var oldValue = (int)handle.OldValue;

            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);            
            if (IsSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None || (!ValidationHelper.IsCurrentCellValidated && !skipValidation))
                return;

            if (this.DataGrid.GetRecordsCount(false) == 0)
                throw new InvalidOperationException("SelectedIndex " + dataGrid.SelectedIndex.ToString() + " doesn't fall with in expected range");

            //Clear the EditItem when its having, by calling EndEdit method
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            int rowIndex = this.dataGrid.ResolveToRowIndex(newValue);

            if (rowIndex == -1 || !this.SelectedRows.Contains(rowIndex))
            {
                //If the selection is any NestedGrid and clears the selection in the below method.
                if (!ClearSelectedDetailsViewGrid() || this.DataGrid.IsInDetailsViewIndex(rowIndex))
                    return;

                if (newValue == -1)
                {
                    //WPF - 33943 Selection should be clear when SelectedIndex is set to -1, hence the below condition is added.
                    ClearSelections(false);
                    return;
                }

                //This code is added to clear the parent grid selections when directly changing the SelectedIndex in DetailsViewGrid.
                if (this.DataGrid is DetailsViewDataGrid)
                {
                    UpdateSelectedDetailsViewGrid();
                }

                this.SuspendUpdates();
                object rowData = this.DataGrid.GetRecordAtRowIndex(rowIndex);

                var addedItems = new List<object>();
                var removedItems = new List<object>();

                var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, currentColumnIndex), false);

                //Selection should be maintain when the SelectionMode is Multiple, hence added the condition to check the SelectionMode.
                if (this.SelectedRows.Count > 0 && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                {
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    this.RemoveSelection(rowIndex, removedItems, SelectionReason.SelectedIndexChanged);
                }

                var rowInfo = this.GetGridSelectedRow(rowIndex);
                if (rowInfo != null)
                {
                    this.SelectedRows.Add(rowInfo);
                    addedItems.Add(rowInfo);
                }
                if (rowData != null)
                    this.DataGrid.SelectedItems.Add(rowData);
                this.DataGrid.HideRowFocusBorder();
                this.DataGrid.ShowRowSelection(rowIndex);
                this.DataGrid.SelectedItem = rowData;

                this.ResumeUpdates();
                if (!cancelSelectionChangedEvent)
                {
                    this.RaiseSelectionChanged(addedItems, removedItems);
                }
                //WPF-25047 Reset the PressedIndex
                SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
        /// </param>
        protected internal override void ProcessSelectedItemChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
            if (IsSuspended || (handle.NewValue == null && handle.OldValue == null) || this.DataGrid.SelectionMode == GridSelectionMode.None || (!ValidationHelper.IsCurrentCellValidated && !skipValidation))
                return;

            //Clear the EditItem when its having, by calling EndEdit method
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            int rowIndex = this.DataGrid.ResolveToRowIndex(handle.NewValue);
            //Handle.NewValue condition is added when SelectedItem is set to null in sample the selection is not cleared, because the rowIndex will -1;
            if (rowIndex < this.DataGrid.GetHeaderIndex() && handle.NewValue != null)
                return;

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            if (handle.NewValue == null)
            {
                if (this.SelectedRows.Count > 0)
                {
                    this.SuspendUpdates();
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                    this.RemoveSelection(rowIndex, removedItems, SelectionReason.SelectedIndexChanged);
                    this.RefreshSelectedIndex();
                    this.RaiseSelectionChanged(new List<object>(), removedItems);
                    this.ResumeUpdates();
                    //WPF-25047 Reset the PressedIndex
                    SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
                }
                return;
            }

            if (!this.DataGrid.SelectedItems.Contains(handle.NewValue))
            {
                if (!ClearSelectedDetailsViewGrid())
                    return;

                //This code is added to clear the parent grid selections when directly changing the SelectedItem in DetailsViewGrid.
                if (this.DataGrid is DetailsViewDataGrid)
                {
                    UpdateSelectedDetailsViewGrid();
                }
                this.DataGrid.HideRowFocusBorder(); 

                var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, currentColumnIndex), false);

                this.SuspendUpdates();
                //Resetting SelectedItems when changing SelectedItem in GridSelectionMode.Multiple case also as per ListView behavior.
                if (this.SelectedRows.Count > 0 )
                {
                    removedItems = this.SelectedRows.Cast<object>().ToList<object>();
                    this.RemoveSelection(rowIndex, removedItems, SelectionReason.SelectedIndexChanged);
                }

                var rowInfo = GetGridSelectedRow(rowIndex);
                this.SelectedRows.Add(rowInfo);
                this.DataGrid.ShowRowSelection(rowIndex);
                this.DataGrid.SelectedItems.Add(handle.NewValue);
                this.DataGrid.SelectedIndex = this.DataGrid.ResolveToRecordIndex(rowIndex);
                addedItems.Add(rowInfo);
                this.ResumeUpdates();
                this.RaiseSelectionChanged(addedItems, removedItems);
                //WPF-25047 Reset the PressedIndex
                SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            }
        }

        #endregion

        #region Collection Changed Operation Methods
        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSource"/> property value changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for source collection changes.
        /// </param>
        /// <param name="reason">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.CollectionChangedReason"/> contains reason for the source collection changes.
        /// </param>
        protected virtual void ProcessSourceCollectionChanged(NotifyCollectionChangedEventArgs e, CollectionChangedReason reason)
        {
            if (IsSuspended)
                return;

            this.SuspendUpdates();
            var removedItems = new List<object>();

            //When replacing the CurrentItem with editing the cell and the exception will be thrown when editing another item. Because
            //the previous edited item is not yet commit.
            if (this.CurrentCellManager.HasCurrentCell &&  DataGrid.HasView && DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            if (reason == CollectionChangedReason.SourceCollectionChanged)
            {
                if (this.DataGrid.HasView && DataGrid.View.IsAddingNew)
                    DataGrid.GridModel.AddNewRowController.CommitAddNew();
            }
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            var newItem = e.NewItems.ToList<object>().FirstOrDefault(item => item is RecordEntry || !(item is NodeEntry));
                            newItem = (newItem is RecordEntry) ? (newItem as RecordEntry).Data : newItem;
                            //Here added the Condition to check the olditem has the Value or Not.While Replacing the DataRow this one will be set as IsDirty.
                            var oldItem = this.SelectedRows.FirstOrDefault(item => item.IsDirty && (item.IsDataRow || item.NodeEntry is NestedRecordEntry));
                            //While Removing the Record IsDirty Flag is set as True. so that the Newly added record could be Select.
                            if (oldItem != null && newItem != null)
                            {
                                var rowIndex = this.DataGrid.ResolveToRowIndex(newItem);
                                var index = this.SelectedRows.IndexOf(oldItem);
                                //index value checked whether it is less than zero to avoid the index out of range exception WPF-20287
                                if(index<0)
                                    return;
                                this.SelectedRows[index] = this.GetGridSelectedRow(rowIndex);
                                this.RefreshSelectedRows();
                                if (this.DataGrid.CurrentItem == null)
                                {
                                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                }
                                else
                                    UpdateCurrentRowIndex(canFocusGrid: false);
                            }
                            else
                            {
                                this.RefreshSelectedRows();
                                UpdateCurrentRowIndex(false, false);
                            }
                            RefreshSelectedIndex();
                            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        //while removing the selected item, condition to be checked the olditems contains any record to avoid the argument out of range exception WPF-20287
                        if (e.OldItems.Count == 0)
                            return;
                        if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            if (isSourceCollectionChanged)//if (e.OldItems.Contains(this.dataGrid.SelectedItem))
                            {
                                this.RefreshSelectedRows();
                                UpdateCurrentRowIndex(false, false);
                                RefreshSelectedIndex();
                                isSourceCollectionChanged = false;
                            }
                            else
                            {
                                foreach (var item in e.OldItems)
                                {
                                    //While replacing the Record, The selected Record could be Marked as IsDirty as True.
                                    //After Removing the Record the New Record Could be added so it have to be select.
                                    var rowInfo = this.SelectedRows.Find(item as NodeEntry);
                                    if (rowInfo != null)
                                        rowInfo.IsDirty = true;
                                }
                            }
                        }
                        else
                        {
                            isSourceCollectionChanged = true;
                            object removedData = e.OldItems[0] is RecordEntry ? (e.OldItems[0] as RecordEntry).Data : e.OldItems[0];
                            if (this.SelectedRows.ContainsObject(removedData))
                            {
                                if (!ClearSelectedDetailsViewGrid())
                                    return;
                                int removedIndex = this.DataGrid.ResolveToRowIndex(removedData);
                                removedItems.Add(GetGridSelectedRow(removedIndex));
                                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.RemoveSelection(removedIndex, removedItems, SelectionReason.CollectionChanged, true);
                                this.RefreshSelectedIndex();
                                if (!cancelSelectionChangedEvent)
                                    this.RaiseSelectionChanged(new List<object>(), removedItems);
                            }
                        }
                        this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (reason == CollectionChangedReason.SourceCollectionChanged)
                        {
                            if (DataGrid.SelectedItems.Contains(e.OldItems[0]))
                            {
                                if (!ClearSelectedDetailsViewGrid())
                                    return;
                                //while replacing the selection of olditem with newitem condition is checked for whether old/new items contains record
                                if (e.OldItems.Count == 0 || e.NewItems.Count == 0)
                                    return;
                                var index = DataGrid.SelectedItems.IndexOf(e.OldItems[0]);
                                DataGrid.SelectedItems[index] = e.NewItems[0];
                                this.SelectedRows.FindRowData(e.OldItems[0]).IsDirty = true;
                                if (DataGrid.SelectedItem == e.OldItems[0])
                                    DataGrid.SelectedItem = e.NewItems[0];
                                if (this.DataGrid.CurrentItem == e.OldItems[0])
                                    DataGrid.CurrentItem = e.NewItems[0];
                            }
                        }
                        //When replacing the Data, the Replace (RecordCollectionChanged) is fired instead of Add hence the below code is added.
                        else if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            if(this.SelectedRows.Any(item => item.IsDirty))
                            {
                                var newItem = e.NewItems.ToList<object>().FirstOrDefault(item => item is RecordEntry || !(item is NodeEntry));
                                newItem = (newItem is RecordEntry) ? (newItem as RecordEntry).Data : newItem;
                                var oldItem = this.SelectedRows.FirstOrDefault(item => item.IsDirty && (item.IsDataRow || item.NodeEntry is NestedRecordEntry));
                                var rowIndex = this.DataGrid.ResolveToRowIndex(newItem);
                                var index = this.SelectedRows.IndexOf(oldItem);
                                
                                if (index < 0 || rowIndex < 0)
                                    return;

                                this.SelectedRows[index] = this.GetGridSelectedRow(rowIndex);
                                this.RefreshSelectedRows();
                                if (this.DataGrid.CurrentItem == null)
                                {
                                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                }
                                else
                                    UpdateCurrentRowIndex(canFocusGrid: false);
                                RefreshSelectedIndex();
                                this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                            }
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    {
                        if (reason == CollectionChangedReason.SourceCollectionChanged)
                        {
                            isSourceCollectionChanged = true;
                        }
                        else if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            if(isSourceCollectionChanged)
                            {
                                //WPF - 33944 DetailsViewGrid selection is not cleared when the source has been cleared.
                                if (this.DataGrid.SelectedDetailsViewGrid != null || this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                                    ClearChildGridSelections(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.RefreshSelectedItems(ref removedItems);
                                this.RefreshSelectedRows();
                                isSourceCollectionChanged = false;
                            }
                            else 
                            {
                                var args = e as NotifyCollectionChangedEventArgsExt;
                                if(args != null && args.IsProgrammatic)
                                {                                                          
                                    this.RefreshSelectedItems(ref removedItems);
                                    this.UpdateCurrentRowIndex(canFocusGrid:false);
                                }
                            }
                        }
                    }
                    break;
            }            
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the row selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItems"/> property value changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for SelectedItems collection changes.
        /// </param>
        protected virtual void ProcessSelectedItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None)
                return;
            this.SuspendUpdates();
            var addedItems = new List<object>();
            var removedItems = new List<object>();

            //Clear the EditItem when its having, by calling EndEdit method
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (this.DataGrid.SelectionMode == GridSelectionMode.Single && this.DataGrid.SelectedItems.Count > 1)
                            throw new InvalidOperationException("Cannot able to Add more than one item in SelectedItems collection when SelectionMode is 'Single'");

                        foreach (var item in e.NewItems)
                        {
                            var rowInfo = GetGridSelectedRow(this.DataGrid.ResolveToRowIndex(item));
                            if (rowInfo != null)
                            {
                                addedItems.Add(rowInfo);
                                this.SelectedRows.Add(rowInfo);
                                this.DataGrid.ShowRowSelection(rowInfo.RowIndex);
                            }
                        }
                        if (addedItems.Count > 0)
                        {
                            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                            int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(((GridRowInfo)addedItems.LastOrDefault()).RowIndex, columnIndex), false);
                        }
                        this.RefreshSelectedIndex();
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        int rowIndex = this.DataGrid.ResolveToRowIndex(e.OldItems[0]);
                        //WPF-28601 Record which is not in view will not be added in SelectedRows, so RemoveSelection process is skipped by adding below condition.
                        if (rowIndex > -1)
                        {
                            removedItems.Add(this.SelectedRows.FindRowData(e.OldItems[0]));
                            this.RemoveSelection(rowIndex, removedItems, SelectionReason.SelectedItemsChanged);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    {
                        removedItems = this.DataGrid.SelectedItems.ToList();

                        ClearSelectedDetailsViewGrid();

                        this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                        this.SelectedRows.Clear();                      
                        HideAllRowSelectionBorder(false);
                        this.DataGrid.SelectedItem = null;
                        this.DataGrid.SelectedIndex = -1;
                        this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                    }
                    break;
            }
            this.DataGrid.HideRowFocusBorder();
            this.ResumeUpdates();
            //WPF-25047 Reset the PressedIndex
            SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            this.RaiseSelectionChanged(addedItems, removedItems);
        }

        /// <summary>
        /// Processes the row selection when the data is reordered in SfDataGrid.
        /// </summary>
        /// <param name="value">
        /// The corresponding value to check whether the reordered data that contains selection.
        /// </param>
        /// <param name="action">
        /// Indicates the corresponding collection changed actions performed on the data.
        /// </param>
        protected virtual void ProcessDataReorder(object value, NotifyCollectionChangedAction action)
        {
            this.SuspendUpdates();

            if (action == NotifyCollectionChangedAction.Remove)
            {
                if (this.SelectedRows.ContainsObject(value))
                {
                    var rowInfo= this.SelectedRows.FindRowData(value);
                    RefreshSelectedRows();
                    this.UpdateCurrentRowIndex();
                    RefreshSelectedIndex();
                    RaiseSelectionChanged(new List<object>(), new List<object>() { rowInfo });
                }
            }
            this.ResumeUpdates();
        }

        #endregion

        #region Group Expand/Collapse


        /// <summary>
        /// Processes the row selection when the group is expanded in SfDataGrid.
        /// </summary>
        /// <param name="insertIndex">
        /// The corresponding index of the group that is expanded in to view.
        /// </param>
        /// <param name="count">
        /// The number of expanded rows count .
        /// </param>        
        protected virtual void ProcessGroupExpanded(int insertIndex, int count)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple || (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.SelectedRows.Count > 0) || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > insertIndex)
            {
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > insertIndex)
                    this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + count);
                this.RefreshSelectedRows();
                if (this.PressedRowColumnIndex.RowIndex > (insertIndex - 1))
                    this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex + count, this.PressedRowColumnIndex.ColumnIndex));
            }
            //When expanding the Group using button click, the selection is not updated which maintains in same row index.
            else
            {
                this.RefreshSelectedRows();
                //This condition is added to update current row index based on row count in group when the current row index is greater than expanded index.
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (insertIndex - 1))
                    this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + count);
            }
        }

        /// <summary>
        /// Processes the row selection when the group is collapsed from view.
        /// </summary>
        /// <param name="removeAtIndex">
        /// The corresponding index of the group that is collapsed from the view.
        /// </param>
        /// <param name="count">
        /// The number of collapsed rows count .
        /// </param>   
        protected virtual void ProcessGroupCollapsed(int removeAtIndex, int count)
        {
            //var updateCurrentIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1) &&
            //        this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < (removeAtIndex - 1 + count);
//#if !WP
//            //Clears the ChildGrid selection when the parentgrid has collapsed the group which contains the Child
//            if (updateCurrentIndex && !this.ClearChildGridSelections(this.CurrentCellManager.CurrentRowColumnIndex))
//                return;
//#endif
            this.RefreshSelectedRows();
            //UpdateCurrentRowIndex method will update the current cell based on current item. When the current row is in Collapsing
            //Group which is need to be remove, hence this method is used to clear the current cell. 
            //WPF-23126 - Need to UpdateCurrentRowIndex for SelectedRow with in the Collapsed group,
            //if we have select the last record in group means row column index is not updated 
            //so change the condition by remove -1 from (removeAtIndex -1) because if we -1 from removeAtIndex
            //its skips the last record in the group.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1) &&
               this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < (removeAtIndex + count))
                this.UpdateCurrentRowIndex();
            //This condition is added to update current row index based on row count in collapsed group when the 
            //current row index is greater than collapsed group index.
            else if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1 + count))
                this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex - count);

            //WPF-23126 Pressed row index is upadted only for CheckShiftKeyPressed with Extended selection not for Muliple Selection
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.SelectedRows.Count > 0)
            {
                if (this.PressedRowColumnIndex.RowIndex > (removeAtIndex - 1 + count))
                    this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex - count, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                //WPF-23126 - Need to pressed RowIndex for 
                //so change the condition by remove -1 from (removeAtIndex -1) because if we -1 from removeAtIndex
                //its skips the last record in the group.
                else if (this.PressedRowColumnIndex.RowIndex < (removeAtIndex + count) && this.PressedRowColumnIndex.RowIndex > (removeAtIndex - 1))
                    this.SetPressedIndex(new RowColumnIndex(removeAtIndex - 1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
            }
        }

        #endregion

        #region TableSummary
        /// <summary>
        /// Processes the row selection when the table summary row position has changed.
        /// </summary>
        /// <param name="args">
        /// Contains the data for the table summary row position changes.
        /// </param>
        protected override void ProcessTableSummaryChanged(TableSummaryPositionChangedEventArgs args)
        {
            //When the changes done in bottom TableSummary there is no need to update the selection and current cell, hence the below 
            //args.NewPostion condition check is added.
            //WPF-23624 - If we set CurrentCell in FirstRow and added the TableSummary at Top in RunTime
            //Current cell is clear, we didn't update the current cell because we have checked 
            //CurrentRowIndex with GetFirstNavigatingRowIndex, it returns the UpdatedRowIndex so if the selection
            // is in first row condition satisfied its returs here, so check with GetHeaderIndex.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetHeaderIndex() || args.Count == 0 || (args.NewPosition == TableSummaryRowPosition.Bottom && args.Action != NotifyCollectionChangedAction.Move))
                return;

            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;

            if (this.CurrentCellManager.HasCurrentCell && DataGrid.HasView && DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedRows.Count > 0)
                RefreshSelectedRows();

            if (isAddNewRow)
            {
                this.CurrentCellManager.SelectCurrentCell(
                    new RowColumnIndex(this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex(),
                        this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                if (this.SelectedRows.Count > 0)
                    this.DataGrid.ShowRowSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            }
            else
            {
                bool needToAdd = (args.Action == NotifyCollectionChangedAction.Add || (args.Action == NotifyCollectionChangedAction.Add
                    && args.NewPosition == TableSummaryRowPosition.Top));

                this.CurrentCellManager.ProcessOnDataRowCollectionChanged(needToAdd, args.Count);
            }
            
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        #endregion

        #region Add New Row
 
        /// <summary>
        /// Processes the row selection when new row is initiated or committed and the position of AddNewRow is changed.
        /// </summary>
        /// <param name="handle">
        /// Contains data for the AddNewRow operation.
        /// </param>
        protected override void ProcessOnAddNewRow(AddNewRowOperationHandlerArgs handle)
        {            
            switch (handle.AddNewRowOperation)
            {
                case AddNewRowOperation.CommitNew:
                    {
                        if (DataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                        {
                            this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                            this.DataGrid.HideRowSelection(CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        }
                    }
                    break;
                case AddNewRowOperation.PlacementChange:
                    {
                        //When changing the position without selecting any row the current row index is updated, hence added this condition.
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                            return;

                        var args = (DependencyPropertyChangedEventArgs)handle.OperationArgs;
                        if (this.SelectedRows.Count > 0)
                            this.RefreshSelectedRows();

                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                        var newValue = (AddNewRowPosition)args.NewValue;
                        if ((this.CurrentCellManager.IsAddNewRow && newValue != AddNewRowPosition.None) || this.CurrentCellManager.IsFilterRow)
                        {
                            int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : this.DataGrid.GetAddNewRowIndex();
                            //When AddNewRow is in editing  and changing the position from bottom to top adds the selection in newly added row instead of in AddNewRow and current cell is also not maintained anywhere because the old current cell is not removed.
                            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            if (this.SelectedRows.Count > 0)
                                this.DataGrid.ShowRowSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                            if (this.CurrentCellManager.IsAddNewRow)
                                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        }
                        else
                        {
                            var oldValue = (AddNewRowPosition)args.OldValue;
                            if (this.CurrentCellManager.IsAddNewRow || (DataGrid.GetAddNewRowPosition() == AddNewRowPosition.Top
                                && oldValue != AddNewRowPosition.Top && oldValue != AddNewRowPosition.FixedTop) || (DataGrid.GetAddNewRowPosition() != AddNewRowPosition.Top 
                                && (oldValue == AddNewRowPosition.FixedTop || oldValue == AddNewRowPosition.Top)))
                            {
                                bool needToAdd = (newValue == AddNewRowPosition.FixedTop || newValue == AddNewRowPosition.Top);
                                this.CurrentCellManager.ProcessOnDataRowCollectionChanged(needToAdd, 1);
                            }                            
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Processes the selection when the position of FilterRow is changed.
        /// </summary>
        /// <param name="args">
        /// Contains data for the FilterRow position changed.
        /// </param>
        protected override void ProcessOnFilterRowPositionChanged(DependencyPropertyChangedEventArgs args)
        {
            //When changing the position without selecting any row the current row index is updated, hence added this condition.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                return;

            if (this.SelectedRows.Count > 0)
                this.RefreshSelectedRows();

            var newValue = (FilterRowPosition)args.NewValue;
            if ((CurrentCellManager.IsFilterRow && newValue != FilterRowPosition.None) || this.CurrentCellManager.IsAddNewRow)
            {
                int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : this.DataGrid.GetAddNewRowIndex();
                
                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                if (this.SelectedRows.Count > 0)
                    this.DataGrid.ShowRowSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            }
            else
            {
                var oldValue = (FilterRowPosition)args.OldValue;
                if (this.CurrentCellManager.IsFilterRow || (DataGrid.GetFilterRowPosition() == FilterRowPosition.Top
                        && oldValue != FilterRowPosition.Top && oldValue != FilterRowPosition.FixedTop) || (DataGrid.GetFilterRowPosition() != FilterRowPosition.Top
                        && (oldValue == FilterRowPosition.FixedTop || oldValue == FilterRowPosition.Top)))
                {
                    bool needToAdd = (newValue == FilterRowPosition.FixedTop || newValue == FilterRowPosition.Top);
                    this.CurrentCellManager.ProcessOnDataRowCollectionChanged(needToAdd, 1);
                }
            }
        }

        #endregion

        #region UnBoundDataRow
        
        /// <summary>
        /// Processes the row selection when the UnBoundRow collection changes.
        /// </summary>
        /// <param name="args">
        /// Contains data for the UnBoundRow collection changes.
        /// </param>
        protected override void ProcessUnBoundRowChanged(UnBoundDataRowCollectionChangedEventArgs args)
        {
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0 && this.SelectedRows.Count == 0)                            
                return;

            var currentRowIndex = args.Action == NotifyCollectionChangedAction.Add ? this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + args.Count : this.CurrentCellManager.CurrentRowColumnIndex.RowIndex - args.Count;
            bool isAddNewRow = this.DataGrid.IsAddNewIndex(currentRowIndex);

            if (this.CurrentCellManager.HasCurrentCell && this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedRows.Count > 0)
            {
                if(args.Action == NotifyCollectionChangedAction.Remove && this.SelectedRows.Any(item=>item.IsUnBoundRow && item.RowIndex == args.RowIndex))
                {
                    var rowInfo = this.SelectedRows.FirstOrDefault(item => item.IsUnBoundRow && item.RowIndex == args.RowIndex);
                    this.DataGrid.HideRowSelection(rowInfo.RowIndex);
                    this.SelectedRows.Remove(rowInfo);
                }
                RefreshSelectedRows();
            }

            if (isAddNewRow)
            {
                this.CurrentCellManager.SelectCurrentCell(
                    new RowColumnIndex(this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex(),
                        this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), false);
            }
            else 
            {                
                if (args.RowIndex < this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                    this.CurrentCellManager.ProcessOnDataRowCollectionChanged(args.Action == NotifyCollectionChangedAction.Add, args.Count);
                else if (args.RowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                {
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    int rowIndex = -1;
                    if (this.SelectedRows.Count > 0)
                        rowIndex = this.SelectedRows.LastOrDefault().RowIndex;
                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                }
                    
            }
           this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }
        
        #endregion

        #region Stacked Header Rows
       
        /// <summary>
        /// Processes the cell selection when the stacked header collection changes in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data for stacked header collection changes.
        /// </param>
        protected override void ProcessOnStackedHeaderRows(StackedHeaderCollectionChangedEventArgs args)
        {
            if(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0 && this.SelectedRows.Count == 0)
                return;

            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;
            if (this.CurrentCellManager.HasCurrentCell && DataGrid.HasView && DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedRows.Count > 0)
                RefreshSelectedRows();

            if (isAddNewRow)
            {
                this.CurrentCellManager.SelectCurrentCell(
                    new RowColumnIndex(this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex(),
                        this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                if (this.SelectedRows.Count > 0)
                    this.DataGrid.ShowRowSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            }
            else                         
                this.CurrentCellManager.ProcessOnDataRowCollectionChanged(args.Action == NotifyCollectionChangedAction.Add, args.Count);            

            this.SetPressedIndex(new RowColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, this.PressedRowColumnIndex.ColumnIndex));
        }
        #endregion

        #region Clear Selections

        /// <summary>
        /// Clears all the selected rows in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentCell">
        /// Decides whether the current cell selection need to be removed when the selections are cleared.
        /// </param>
        /// <remarks>
        /// This method helps to clear the selection programmatically.
        /// </remarks>
        public override void ClearSelections(bool exceptCurrentRow)
        {
            this.ClearSelections(exceptCurrentRow, true);
        }

        /// <summary>
        /// Clears all the selected rows in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current row selection need to be removed when the selections are cleared.
        /// </param>
        /// <param name="removeCurrentCellSelection">
        /// Decides whether the current cell selection need to be removed when the selections are cleared.
        /// </param>             
        protected virtual void ClearSelections(bool exceptCurrentRow, bool removeCurrentCellSelection = true)
        {
            //While running sample changing SelectionUnit as cell, the clear selection is called and the VisualContainer is null which is
            //checked in GetFirstNavigatingRowIndex
            if(this.DataGrid.VisualContainer == null || !this.DataGrid.HasView)
                return;

            this.SuspendUpdates();
            //When changing the SelectionMode to Single from Multiple with editing the any row, the EditItem is still maintianed in View,
            //Which throws exception when editing other cells. Hence the EndEdit is called.
            if (this.CurrentCellManager.HasCurrentCell && (this.CurrentCellManager.CurrentCell.IsEditing || (this.dataGrid.HasView && this.dataGrid.View.IsEditingItem)))
                this.CurrentCellManager.EndEdit();

            var removedItems = new List<object>();
            removedItems = this.SelectedRows.Cast<object>().ToList<object>();

            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex >= this.DataGrid.GetFirstNavigatingRowIndex() && exceptCurrentRow)
            {
                object currentData = this.DataGrid.CurrentItem;
                int currentRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                GridRowInfo rowInfo = SelectedRows.Find(currentRowIndex);
                this.SelectedRows.Clear();
                this.DataGrid.SelectedItems.Clear();
                this.HideAllRowSelectionBorder(exceptCurrentRow);               
                this.SelectedRows.Add(GetGridSelectedRow(currentRowIndex));
                if (rowInfo != null)                
                    removedItems.Remove(rowInfo);
                if(currentData  != null)
                    this.DataGrid.SelectedItems.Add(currentData);
                //When changing from Multiple to single in DetailsViewGrid, the Selection is not updated.
                this.DataGrid.ShowRowSelection(currentRowIndex);
                this.RefreshSelectedIndex();
                //The SetCurrentRowIndex method is invoked to set the current row index which is removed because the currentRowIndex is not
                //changed. Hence no need to set same currentRowIndex.
            }
            else
            {
                if (!ClearSelectedDetailsViewGrid())
                    return;

                if (removeCurrentCellSelection)
                {
                    CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                    //The Below Condition is check while the selection is in AddNewRow, When we clear the Selection Watermark is Disappeared.
                    if (this.CurrentCellManager.IsAddNewRow)
                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                    this.CurrentCellManager.ResetCurrentRowColumnIndex();
                }
                this.HideAllRowSelectionBorder(false);
                this.SelectedRows.Clear();
                this.DataGrid.SelectedItems.Clear();
                if (this.DataGrid.SelectedItem != null)
                    this.DataGrid.SelectedItem = null;
                if (this.DataGrid.SelectedIndex != -1)
                    this.DataGrid.SelectedIndex = -1;
            }
            if (!cancelSelectionChangedEvent && removedItems.Count > 0)
                this.RaiseSelectionChanged(new List<object>(), removedItems);
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.DataGrid.HideRowFocusBorder();
            //When changing the SelectionMode to single from Multiple the RowHeaderState has been changed, hence the below condition is added.
            if (!exceptCurrentRow)
                ClearRowHeader();
            this.ResumeUpdates();
        }

        #endregion
        
        #endregion

        #region Helper Methods

        #region General Helpers

        /// <summary>
        /// Removes the selected rows from the SfDataGrid control.
        /// </summary>      
        protected bool RemoveRows()
        {
            if (!this.DataGrid.HasView)
                return false;

            var selectionController = (GridSelectionController)this.DataGrid.SelectionController;
            var currentCell = selectionController.CurrentCellManager.CurrentCell;
            bool canUserDeleteRows;
            var currentCellIndex = selectionController.CurrentCellManager.CurrentRowColumnIndex;
            if (this.DataGrid.NavigationMode == NavigationMode.Cell)
            {
                var dataRow = dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == selectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                //Need to check whether the deleting row is DataRow, hence the below default row condition is added.
                canUserDeleteRows = (this.DataGrid.AllowDeleting) && this.DataGrid.SelectedItems.Count > 0 && (currentCell != null && !currentCell.IsEditing && dataRow != null && dataRow.RowType == RowType.DefaultRow);
            }
            else
                canUserDeleteRows = (this.DataGrid.AllowDeleting) && this.DataGrid.SelectedItems.Count > 0;

            if (!canUserDeleteRows || (this.DataGrid.IsAddNewIndex(currentCellIndex.RowIndex) && this.DataGrid.SelectedItems.Count < 1))
                return false;

            var recordsToRemove = this.DataGrid.SelectedItems.ToList();
            //Get the currentcell rowindex when the pressedindex is greater than rowindex.
            var selectedRowIndex = selectionController.PressedRowColumnIndex.RowIndex > currentCellIndex.RowIndex ? currentCellIndex.RowIndex : ((GridSelectionController)this.DataGrid.SelectionController).PressedRowColumnIndex.RowIndex;

            var deletingEventArgs = new RecordDeletingEventArgs(this.DataGrid) { Items = this.DataGrid.SelectedItems.ToList() };
            if (this.DataGrid.RaiseRecordDeletingEvent(deletingEventArgs))
                return false;

            var itemsToRemove = deletingEventArgs.Items;
            cancelSelectionChangedEvent = true;

            selectionController.SuspendUpdates();
            foreach (var item in itemsToRemove.Where(item => this.DataGrid.View.Contains(item)))
            {
                //The CurrentCell is removed after deleting the particular row has changed the DataContext of the particular DataRow,
                //When resusing the DataRow, the CurrentCell is maintained some times. Hence the below Method is invoked before deleting the Row.
                if(this.CurrentCellManager.CurrentCell != null)
                    selectionController.CurrentCellManager.RemoveCurrentCell(selectionController.CurrentCellManager.CurrentRowColumnIndex);

                this.DataGrid.View.Remove(item);
                var rowinfo = this.SelectedRows.FindRowData(item);
                if (rowinfo != null)
                    rowinfo.IsDirty = true;
            }
            selectionController.ResumeUpdates();

            var deletedEventArgs = new RecordDeletedEventArgs(this.DataGrid)
            {
                Items = itemsToRemove,           
                SelectedIndex = selectedRowIndex
            };
            this.DataGrid.RaiseRecordDeletedEvent(deletedEventArgs);
            var rowIndex = -1;
            if(this.SelectedRows.Count > 0)
                rowIndex = this.DataGrid.SelectionController.SelectedRows.FirstOrDefault().RowIndex;           
            if (itemsToRemove.Count != recordsToRemove.Count)
            {
                if (itemsToRemove.Count == 0)
                    return false;
                currentCellIndex = new RowColumnIndex(rowIndex, currentCell.ColumnIndex);
            }
            //To ensure the selected rows in SelectedRows collection.
            selectionController.RefreshSelectedRows();

            if (this.DataGrid.GetRecordsCount() >= 0 || selectionController.CurrentCellManager.CurrentRowColumnIndex == currentCellIndex)
            {
                int index = 0;
                //In Mutliple SelectionMode, when deleting all rows with CurrentCell in AddNewRow the selection is not done, hence the below condition is added.
                if (selectionController.CurrentCellManager.IsAddNewRow)
                    index = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                else
                    index = deletedEventArgs.SelectedIndex > this.DataGrid.GetLastNavigatingRowIndex() ? this.DataGrid.GetLastNavigatingRowIndex() : deletedEventArgs.SelectedIndex;              
                currentCellIndex.RowIndex = this.DataGrid.IsInDetailsViewIndex(index) ? this.DataGrid.ResolveToRowIndex(this.DataGrid.DetailsViewManager.GetDetailsViewRecord(index)) : index;
                selectionController.CurrentCellManager.SelectCurrentCell(currentCellIndex);
                if (index > 0)
                {
                    selectionController.AddSelection(new List<object>() { selectionController.GetGridSelectedRow(currentCellIndex.RowIndex) }, SelectionReason.CollectionChanged);
                    selectionController.CurrentCellManager.ScrollToRowIndex(currentCellIndex.RowIndex);
                }
                if (selectionController.CurrentCellManager.IsAddNewRow)
                    this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);                

                this.SetPressedIndex(currentCellIndex);
                cancelSelectionChangedEvent = false;
            }

            var e = new GridSelectionChangedEventArgs(this.DataGrid)
            {
                AddedItems = this.SelectedRows.Cast<object>().ToList<object>(),
                RemovedItems = itemsToRemove,
            };
            this.DataGrid.RaiseSelectionChangedEvent(e);
            return true;
        }

        private void RefreshSelectedIndex()
        {
            this.DataGrid.SelectedIndex = this.SelectedRows.Count > 0 ? this.DataGrid.ResolveToRecordIndex(this.SelectedRows[0].RowIndex) : -1;
            this.DataGrid.SelectedItem = this.DataGrid.SelectedItems.Count > 0 ? this.DataGrid.SelectedItems[0] : null;
        }

        /// <summary>
        /// Resets the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedRows"/> based on the selection added or removed in SfDataGrid .
        /// </summary>
        /// <param name="canUpdateCurrentRow">
        /// Indicates whether the current row is updated based on CurrentItem.
        /// </param>
        /// <remarks>
        /// The SelectedRows collection updated only for data rows and other selection will be removed from SfDataGrid.
        /// </remarks>
        public void ResetSelectedRows(bool canUpdateCurrentRow = true)
        {
            if (canUpdateCurrentRow)
                this.UpdateCurrentRowIndex(canFocusGrid:false);
            var removedItems = new List<object>();
            this.RefreshSelectedItems(ref removedItems);
            this.RefreshSelectedRows();
        }

        /// <summary>
        /// Refreshes the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedRows"/> collection in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method refresh the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedRows"/> collection when the grid related operations are performed.
        /// </remarks>
        protected void RefreshSelectedRows()
        {
            if (this.SelectedRows.Count > 0 || (this.CurrentCellManager.CurrentRowColumnIndex != default(RowColumnIndex) && this.dataGrid.SelectionMode == GridSelectionMode.Multiple))
            {
                if (this.DataGrid.HasView && this.DataGrid.View.CurrentEditItem != null)
                    CurrentCellManager.EndEdit();

                int index = this.SelectedRows.Count - 1;
                while(index >= 0)
                {
                    var nodeEntry = this.SelectedRows[index].NodeEntry;
                    int rowIndex = -1;
                    if (nodeEntry != null && !this.SelectedRows[index].IsDirty)
                        rowIndex = this.DataGrid.ResolveToRowIndex(SelectedRows[index].NodeEntry);
                    else if (this.SelectedRows[index].IsAddNewRow && this.DataGrid.AddNewRowPosition != AddNewRowPosition.None)
                        rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                    else if (this.SelectedRows[index].IsFilterRow && this.DataGrid.FilterRowPosition != FilterRowPosition.None)
                        rowIndex = this.DataGrid.GetFilterRowIndex();
                    else if (SelectedRows[index].IsUnBoundRow)
                        //RowIndex will be updated when the selected row is UnBoundRow.
                        rowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(SelectedRows[index].GridUnboundRowInfo);
                    if(rowIndex < 0)
                    {
                        this.DataGrid.HideRowSelection(this.SelectedRows[index].RowIndex);
                        this.SelectedRows.RemoveAt(index);
                    }
                    else
                        this.SelectedRows[index].RowIndex = rowIndex;
                    index--;
                }
                SuspendUpdates();
                this.DataGrid.SelectedItems.Clear();
                this.SelectedRows.ForEach(rowInfo =>
                    {
                        if (rowInfo.IsDataRow || rowInfo.NodeEntry is NestedRecordEntry)
                            this.DataGrid.SelectedItems.Add(rowInfo.RowData);
                    });

                this.RefreshSelectedIndex();
                ResumeUpdates();
            }
        }

        /// <summary>
        /// Refreshes the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItems"/> collection in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method refresh the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItems"/> collection when the grid related operations performed.
        /// </remarks>
        protected void RefreshSelectedItems(ref List<object> removedItems)
        {
            if (!this.DataGrid.HasView)
                return;

            this.SuspendUpdates();
            int index = this.DataGrid.SelectedItems.Count - 1;
            while (index >= 0)
            {
                object item = this.DataGrid.SelectedItems[index];
                bool isAvailable = false;
                // Need to get the record or record index from DisplayElements when the Source is IQueryable. Since we will keep null record entries View.Records. Entries will be kept in Group.Records alone.
                var recordentry = dataGrid.isIQueryable && dataGrid.GridModel.HasGroup ? dataGrid.View.TopLevelGroup.DisplayElements.GetItem(item) : this.DataGrid.View.Records.GetRecord(item);
                if (recordentry != null)
                {
                    if (this.DataGrid.GridModel.HasGroup)
                    {
                        isAvailable = this.DataGrid.CheckGroupExpanded(recordentry.Parent as Group);
                    }
                    else
                    {
                        isAvailable = true;
                    }
                }
                if (!isAvailable)
                {
                    this.DataGrid.SelectedItems.Remove(item);
                }
                index--;
            }
            var removedRows = this.SelectedRows.ToList<GridRowInfo>();
            //In this method the DataRow only been udated, hence added the below code update the RowIndex of UnBoundRow and AddNewRow
            if(!this.SelectedRows.Any(rowInfo=>rowInfo.IsUnBoundRow || rowInfo.IsAddNewRow || rowInfo.IsFilterRow))
                this.SelectedRows.Clear();
            else
            {
                this.SelectedRows.RemoveAll(item => !item.IsUnBoundRow && !item.IsAddNewRow && !item.IsFilterRow);
                if (this.SelectedRows.Count > 0)
                    this.SelectedRows.ForEach(rowInfo =>
                    {
                        if (rowInfo.IsFilterRow)
                            rowInfo.RowIndex = this.DataGrid.GetFilterRowIndex();
                        else
                            rowInfo.RowIndex = rowInfo.IsUnBoundRow
                                ? this.DataGrid.ResolveUnboundRowToRowIndex(rowInfo.GridUnboundRowInfo)
                                : this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                    });
            }
            
            index = 0;
            while (index < this.DataGrid.SelectedItems.Count)
            {
                this.SelectedRows.Add(this.GetGridSelectedRow(this.DataGrid.ResolveToRowIndex(this.DataGrid.SelectedItems[index])));
                index++;
            }                            
            removedItems = removedRows.Where(item => !item.IsUnBoundRow && (!item.IsDataRow || !this.SelectedRows.ContainsObject(item.RowData))).Cast<object>().ToList<object>();
            RefreshSelectedIndex();
            this.ResumeUpdates();
        }
   
        /// <summary>
        /// Commits the new item initialized on the AddNewRow to the underlying data source.
        /// </summary>
        /// <param name="changeState">
        /// Indicates whether watermark should be enabled on the AddNewRow.
        /// </param>
        protected internal override void CommitAddNew(bool changeState = true)
        {
            this.SuspendUpdates();
            this.DataGrid.GridModel.AddNewRowController.CommitAddNew(changeState);
            this.RefreshSelectedRows();
            this.ResumeUpdates();
        }
       
        /// <summary>
        /// Expands or collapses the group when the Right or Left arrow key is pressed.
        /// </summary>
        /// <param name="group">
        /// The corresponding group to expand or collapse.
        /// </param>
        /// <param name="isExpanded">
        /// Indicates the whether the group is expanded.
        /// </param>
        protected void ExpandOrCollapseGroup(Group group, bool isExpanded)
        {
            var captionRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex && (item.RowType == RowType.CaptionCoveredRow || item.RowType == RowType.CaptionRow));
            if (captionRow != null)
            {
                var captionRowControl = captionRow.WholeRowElement as CaptionSummaryRowControl;
                if (isExpanded)
                {
                    if (!group.IsExpanded)
                    {
                        this.DataGrid.GridModel.ExpandGroup(group);
                        captionRowControl.IsExpanded = true;
                    }
                }
                else
                {
                    if (group.IsExpanded)
                    {
                        this.DataGrid.GridModel.CollapseGroup(group);
                        captionRowControl.IsExpanded = false;
                    }
                }
                this.DataGrid.GridModel.RefreshDataRow(CurrentCellManager.CurrentRowColumnIndex.RowIndex, true);
            }
        }

        /// <summary>
        /// Method which decides whether we can remove the selection in same row.
        /// </summary>
        /// <returns></returns>
        private bool CheckCanRemoveSameRow()
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.Extended)
            {
#if UWP
                if ((Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift).HasFlag(CoreVirtualKeyStates.Down)) && (Window.Current.CoreWindow.GetAsyncKeyState(Key.Home).HasFlag(CoreVirtualKeyStates.Down) || Window.Current.CoreWindow.GetAsyncKeyState(Key.End).HasFlag(CoreVirtualKeyStates.Down)))
#else
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.Home) || Keyboard.IsKeyDown(Key.End)))
#endif
                    return false;
            }
            return true;
        }
       
        internal GridRowInfo GetGridSelectedRow(NodeEntry nodeEntry, int rowIndex)
        {
            if (nodeEntry != null)
            {
                if (nodeEntry is RecordEntry)
                    return new GridRowInfo(rowIndex, (nodeEntry as RecordEntry).Data, nodeEntry);
                else
                    return new GridRowInfo(rowIndex, null, nodeEntry);
            }
            return null;
        }

        internal GridRowInfo GetGridSelectedRow(NodeEntry nodeEntry)
        {
            if (nodeEntry != null)
            {
                var rowIndex = this.DataGrid.ResolveToRowIndex(nodeEntry);
                if (nodeEntry is RecordEntry)
                    return new GridRowInfo(rowIndex, ((RecordEntry)nodeEntry).Data, nodeEntry);
                else
                    return new GridRowInfo(rowIndex, null, nodeEntry);
            }
            return null;
        }

        #endregion

        #region Selection Helpers

        /// <summary>
        /// Reset the ParentDataGrid selection to the corresponding DetailsViewIndex when the selection is moved to DetailsViewDataGrid.
        /// </summary>
        private void ResetParentGridSelection()
        {
            if (!(this.DataGrid is DetailsViewDataGrid))
                return;
            var parentDataGrid = this.DataGrid.GetParentDataGrid();
            var selectionController = parentDataGrid.SelectionController as GridSelectionController;
            var removedItems = selectionController.SelectedRows.ToList<object>();
            var rowIndex = parentDataGrid.GetGridDetailsViewRowIndex(this.DataGrid as DetailsViewDataGrid);
            selectionController.ResetSelection(rowIndex, removedItems, true);
            if (parentDataGrid.NotifyListener != null)
                selectionController.ResetParentGridSelection();
        }
      
        /// <summary>
        /// Processes the row selection for the specified row index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding index to select the row.
        /// </param>
        /// <param name="previousCurrentCellIndex">
        /// The corresponding previous current cell index.
        /// </param>
        /// <param name="reason">
        /// Contains the reason for process selection.
        /// </param>       
        /// <returns>
        /// Returns <b>true</b> if the cell selection is processed to corresponding row and column index; otherwise, <b>false</b>.
        /// </returns>
        protected bool ProcessSelection(int rowIndex, RowColumnIndex previousCurrentCellIndex, SelectionReason reason)
        {
            //Added this flag to clear the selection on AddNewRow when SelectionMode is multiple or when pressing Ctrl key.
            bool needToRemoveNonDataRows = (this.DataGrid.IsAddNewIndex(previousCurrentCellIndex.RowIndex) || this.DataGrid.IsFilterRowIndex(previousCurrentCellIndex.RowIndex)) && ((this.DataGrid.SelectionMode == GridSelectionMode.Extended && (SelectionHelper.CheckControlKeyPressed() || SelectionHelper.CheckShiftKeyPressed())) || this.DataGrid.SelectionMode == GridSelectionMode.Multiple);
            //Added the condition for removing the selection from addnewrow when SelectionMode is multiple. 
            //It is done by checking the previous current rowIndex whether it is in AddNewRow.
            //Added the condition in Multiple Selection mode to maintain the selection in parent grid when the current cell only moved to DetailsViewGrid
            bool needToRemove = this.DataGrid.SelectionMode == GridSelectionMode.Single || (this.DataGrid.SelectionMode == GridSelectionMode.Extended && (this.DataGrid.IsInDetailsViewIndex(rowIndex) || (!SelectionHelper.CheckControlKeyPressed() && (reason == SelectionReason.PointerReleased || reason == SelectionReason.PointerPressed)) || (reason == SelectionReason.KeyPressed && !SelectionHelper.CheckShiftKeyPressed()) || (SelectionHelper.CheckShiftKeyPressed() && (this.lastPressedKey == Key.Tab || this.lastPressedKey == Key.A)))) 
                || ((reason == SelectionReason.KeyPressed || this.DataGrid.IsInDetailsViewIndex(rowIndex) || this.DataGrid.IsAddNewIndex(previousCurrentCellIndex.RowIndex) || (this.DataGrid.IsInDetailsViewIndex(previousCurrentCellIndex.RowIndex) && rowIndex != previousCurrentCellIndex.RowIndex && this.SelectedRows.Any(item=>item.NodeEntry is NestedRecordEntry))) && this.DataGrid.SelectionMode == GridSelectionMode.Multiple);
            bool needToRemoveSameRow = ((this.DataGrid.SelectionMode == GridSelectionMode.Extended && ((SelectionHelper.CheckControlKeyPressed() && reason != SelectionReason.KeyPressed) || (reason == SelectionReason.KeyPressed && SelectionHelper.CheckShiftKeyPressed() && this.lastPressedKey != Key.Tab && this.lastPressedKey != Key.A))) || this.DataGrid.SelectionMode == GridSelectionMode.Multiple) && this.SelectedRows.Contains(rowIndex) && !this.DataGrid.IsInDetailsViewIndex(rowIndex);
            
            if (needToRemoveSameRow)
                needToRemoveSameRow = CheckCanRemoveSameRow();

            var data = this.DataGrid.GetRecordAtRowIndex(rowIndex);
            if (needToRemoveSameRow && (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && ((this.PressedRowColumnIndex.RowIndex < rowIndex && previousCurrentCellIndex.RowIndex < rowIndex) || (this.PressedRowColumnIndex.RowIndex > rowIndex && previousCurrentCellIndex.RowIndex > rowIndex))))
                return true;

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            var selectedRow = this.GetGridSelectedRow(rowIndex);
            if (selectedRow != null)
                addedItems.Add(selectedRow);
            if (previousCurrentCellIndex.RowIndex > this.DataGrid.GetHeaderIndex() && (!needToRemoveSameRow || needToRemoveNonDataRows))//&& needToRemove)
            {
                var rItems = this.SelectedRows.ToList();
                if (this.lastPressedKey == Key.A && SelectionHelper.CheckShiftKeyPressed() && reason == SelectionReason.KeyPressed && this.SelectedRows.Contains(this.CurrentCellManager.previousRowColumnIndex.RowIndex))
                {
                    rItems.Remove(this.SelectedRows.Find(this.CurrentCellManager.previousRowColumnIndex.RowIndex));
                }

                //To Remove the selection on AddNewRow when the SelectionMode is Multiple.
                //Checked whether the rowIndex is in DetailViewGrid to clear the selection in multiple.
                if (needToRemoveNonDataRows && !this.DataGrid.IsInDetailsViewIndex(rowIndex))
                {
                    removedItems.Add(this.SelectedRows.FirstOrDefault(item => item.IsAddNewRow || item.IsFilterRow));
                    needToRemove = true;
                }
                else
                    removedItems = rItems.Cast<object>().ToList<object>();
            }

            if (needToRemoveSameRow)
            {
                //When unselecting the row where the CurrentRow on AddNewRow we need to clear both AddNewRow and clicked row hence the below
                //condition is added.
                if (!needToRemoveNonDataRows)
                    removedItems.Clear();
                removedItems.AddRange(addedItems);
                
                if (this.RaiseSelectionChanging(null, removedItems))
                {
                    //Fix for WPF-16876 - No need to remove the selection for Same row.
                    if(CurrentCellManager.CurrentRowColumnIndex.RowIndex != previousCurrentCellIndex.RowIndex)
                    {
                        CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                        CurrentCellManager.SelectCurrentCell(previousCurrentCellIndex);                    
                    }
                    return false;
                }

                this.DataGrid.HideRowFocusBorder();
                this.RemoveSelection(rowIndex, removedItems, reason, needToRemoveSameRow: true);

                if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                    this.DataGrid.ShowRowFocusBorder(rowIndex);

                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();

                
                this.RaiseSelectionChanged(new List<object>(), removedItems);
                return true;
            }

            if (!needToRemove)
            {
                removedItems.Clear();
            }
            if (addedItems.Count > 0 || removedItems.Count > 0)
            {
                if (this.RaiseSelectionChanging(addedItems, removedItems))
                {
                    CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    CurrentCellManager.SelectCurrentCell(previousCurrentCellIndex);
                    //When Cancel the Selection through the Selection Changing Event the pressed index will update when press any DataRow through MouseClick.
                    this.SetPressedIndex(previousCurrentCellIndex);
                    return false;
                }
                if (needToRemove)
                {
                    if (this.SelectedRows.Count > 1 && !SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                    {
                        this.SuspendUpdates();
                        this.SelectedRows.Clear();
                        this.DataGrid.SelectedItems.Clear();
                        this.ResumeUpdates();
                        this.HideAllRowSelectionBorder(false);
                        this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                    }
                    else
                        this.RemoveSelection(rowIndex, removedItems, reason);
                }

                if (addedItems.Count > 0)
                    this.AddSelection(addedItems, reason);

                if (this.PressedRowColumnIndex.RowIndex < this.DataGrid.HeaderLineCount)
                    this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);

                this.DataGrid.HideRowFocusBorder();
                this.RaiseSelectionChanged(addedItems, removedItems);
                return true;
            }

            return false;
        }
    
        public List<GridRowInfo> GetRowInfo()
        {
            var rowinfo = new List<GridRowInfo>();
            this.SelectedRows.ForEach(item=>
            {
                rowinfo.Add(new GridRowInfo(item.RowIndex, item.RowData, item.NodeEntry,item.IsAddNewRow));
            });
            return rowinfo;
        }
        //To get last selected index of the row while dragging.
        int _pshiftselectionrowindex = -1;

        private List<GridRowInfo> GetSelectedRows(int startindex, int rowIndex)
        {
            var addedItems = new List<GridRowInfo>();
            var currentIndex = startindex;
            //The Below Condition is added while the addedItem has skipped in the DragSelection for ParentGrid.
            if (this.DataGrid.IsInDetailsViewIndex(this.PressedRowColumnIndex.RowIndex) && !SelectionHelper.CheckShiftKeyPressed())
            {
                addedItems.Add(this.GetGridSelectedRow(rowIndex));
            }
            else
            {
                //The Above if condition has been removed because we have changed the behaviour. while pressing Control+Shift+Down or Up key the selection doesnot happen.
                //when the currentcell is in the DetailsviewDataGrid. Now we have given the selection to that operation.
                if (this.PressedRowColumnIndex.RowIndex < rowIndex)
                {
                    while (currentIndex <= rowIndex)
                    {
                        if (!this.DataGrid.IsInDetailsViewIndex(currentIndex))
                            addedItems.Add(GetGridSelectedRow(currentIndex));
                        currentIndex = currentIndex >= this.DataGrid.GetLastRowIndex() ? ++currentIndex : this.GetNextRowIndex(currentIndex);
                    }
                }
                else
                {
                    while (currentIndex >= rowIndex)
                    {
                        if (!this.DataGrid.IsInDetailsViewIndex(currentIndex))
                            addedItems.Add(GetGridSelectedRow(currentIndex));
                        currentIndex = currentIndex <= this.DataGrid.GetFirstRowIndex() ? --currentIndex : this.GetPreviousRowIndex(currentIndex);
                    }
                }
            }            
            return addedItems;
        }

        /// <summary>
        /// Processes the row selection when the rows selected by using Shift key.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to perform Shift selection.
        /// </param>
        /// <param name="reason">
        /// Contains the reason for processing the Shift selection.
        /// </param>
        protected void ProcessShiftSelection(int rowIndex, SelectionReason reason)
        {

            if (_pshiftselectionrowindex == rowIndex)
                return;

            //object pressedRowData = this.DataGrid.GetRecordAtRowIndex(this.pressedIndex);
            var addedItems = new List<GridRowInfo>();
            var removedItems = new List<GridRowInfo>();

            //WPF-29687 - Previous Row Selection maintained if Ctrl key pressed while performing a drag selection
            if ((this.DataGrid.SelectionMode != GridSelectionMode.Multiple && !SelectionHelper.CheckControlKeyPressed()) || (this.DataGrid.IsInDetailsViewIndex(this.PressedRowColumnIndex.RowIndex)&&!this.dataGrid.AllowSelectionOnPointerPressed))
                removedItems = this.SelectedRows.ToList<GridRowInfo>();
            else
            {
                if (_pshiftselectionrowindex != -1)
                    //Get the removed items from pressed to last selected row otherwise removed items maintained from all selected rows 
                    removedItems = this.GetSelectedRows(this.PressedRowColumnIndex.RowIndex, _pshiftselectionrowindex);
            }

            _pshiftselectionrowindex = rowIndex;
            //var previousIndex = CurrentCellManager.previousRowColumnIndex;
            //int currentIndex = this.pressedIndex;
            addedItems = this.GetSelectedRows(this.PressedRowColumnIndex.RowIndex, rowIndex);

            List<GridRowInfo> commonItems = addedItems.Intersect(removedItems).ToList();
            List<GridRowInfo> addedItem = addedItems.Except(commonItems).ToList();
            var removedItem = removedItems.Except(commonItems).ToList<object>();
            List<object> newAddedItems;

            //If the SelectionMode is multiple ,need to skip the SelectedRows from addedItem 
            if(this.DataGrid.SelectionMode==GridSelectionMode.Multiple)
            {
                newAddedItems = addedItem.Except(SelectedRows.ToList<GridRowInfo>()).ToList<object>();            
            }
            else 
            {
                newAddedItems = addedItem.ToList<object>();
            }

            if (this.RaiseSelectionChanging(newAddedItems, removedItem))
                return;
           
            this.RemoveSelection(rowIndex, removedItem, reason);

            this.SuspendUpdates();

            this.AddSelection(newAddedItems, reason);

            this.RefreshSelectedIndex();
            this.ResumeUpdates();
            RaiseSelectionChanged(newAddedItems, removedItem);
        }

        /// <summary>
        /// Adds the row selection for the specified list of items.
        /// </summary>
        /// <param name="addedItems">
        /// The corresponding list of items to add the selection.
        /// </param>
        /// <param name="reason">
        /// Contains the reason to add the selection.
        /// </param>
        protected void AddSelection(List<object> addedItems, SelectionReason reason)
        {
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
            if (!ValidationHelper.IsCurrentCellValidated && !skipValidation)
                return;

            this.SuspendUpdates();

            if (addedItems != null)
            {
                addedItems.ForEach(item =>
                {
                    var rowInfo = (GridRowInfo)item;
                    if (item != null && !this.SelectedRows.Contains(rowInfo))
                    {
                        this.SelectedRows.Add(rowInfo);
                        this.DataGrid.ShowRowSelection(rowInfo.RowIndex);
                        if (rowInfo.RowData != null && !this.DataGrid.SelectedItems.Contains(rowInfo.RowData))
                            this.DataGrid.SelectedItems.Add(rowInfo.RowData);
                    }
                });
            }

            RefreshSelectedIndex();

            this.ResumeUpdates();
        }

        /// <summary>
        /// Removes the selection for the specified list of items.
        /// </summary>
        /// <param name="rowIndex">
        /// The rowIndex.
        /// </param>
        /// <param name="removedItems">
        /// The corresponding list of items to remove the selection. 
        /// </param>
        /// <param name="reason">
        /// Contains the corresponding reason to remove the selection.
        /// </param>
        /// <param name="needToRemoveSameRow">
        /// Indicates whether the selection need to be removed for the same row.
        /// </param>
        protected void RemoveSelection(int rowIndex, List<object> removedItems, SelectionReason reason, bool needToRemoveSameRow = false)
        {
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this.DataGrid);
            if (!ValidationHelper.IsCurrentCellValidated && !skipValidation)
                return;

            this.SuspendUpdates();
            removedItems.ForEach(item =>
            {
                var rowInfo = (GridRowInfo)item;
                if (this.SelectedRows.Remove(rowInfo))
                {
                    this.DataGrid.HideRowSelection(rowInfo.RowIndex);
                    if (rowInfo.RowData != null)
                        this.DataGrid.SelectedItems.Remove(rowInfo.RowData);
                }

            });

            if (needToRemoveSameRow && this.CurrentCellManager.CurrentCell == null && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
            {
                int index = -1;
                if (SelectedRows.Count > 0)
                {
                    index = this.SelectedRows.LastOrDefault().RowIndex;
                }
                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(index, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Resets the row selection for the specified row index and list of removed items.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to reset the selection
        /// </param>
        /// <param name="removedItems">
        /// The list of items to reset the selection. 
        /// </param>
        /// <param name="setFocuForGrid">
        /// Indicates whether the focus is set to SfDataGrid or not.
        /// </param>
        protected internal void ResetSelection(int rowIndex, List<object> removedItems, bool setFocuForGrid = true)
        {
            var addedItems = new List<object>();
            var rowInfo = this.GetGridSelectedRow(rowIndex);

            if (!this.SelectedRows.Contains(rowInfo))
                addedItems.Add(rowInfo);

            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), setFocuForGrid);
            this.DataGrid.HideRowFocusBorder();
            if (removedItems != null && removedItems.Count > 0)
            {
                this.RemoveSelection(rowIndex, removedItems, SelectionReason.GridOperations);
            }

            if (addedItems.Count > 0)
                this.AddSelection(addedItems, SelectionReason.GridOperations);

            //To cancel the Selection changed event when changing the CurrentItem.
            if (!this.cancelSelectionChangedEvent)
                this.RaiseSelectionChanged(addedItems, removedItems);
        }

        /// <summary>
        /// To update update the parent grid selection to DetailsViewGrid when directly changing the SelectedIndex and SelectedItem of the DetailsViewGrid.
        /// </summary>
        private void UpdateSelectedDetailsViewGrid()
        {
            SfDataGrid detailsViewGrid = this.DataGrid;
            while (detailsViewGrid.NotifyListener != null)
            {
                var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
                if (parentDataGrid == null)
                    return;

                var selectioncController = parentDataGrid.SelectionController as GridSelectionController;
                var detailsViewDataRow = parentDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row =>row.DetailsViewDataGrid == this.DataGrid);
                var detailsViewIndex = detailsViewDataRow.RowIndex;
                //Check to whether the NestedGrid contains any Selection or CurrentCell.
                if (parentDataGrid.SelectedDetailsViewGrid != this.DataGrid || selectioncController.CurrentCellManager.CurrentRowColumnIndex.RowIndex != detailsViewIndex)
                {
                    //Clears the ChildGrid selection when the parentgrid has selection anyother child grid.
                    if (!selectioncController.ClearChildGridSelections(selectioncController.CurrentCellManager.CurrentRowColumnIndex))
                        return;

                    parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
                    List<object> rItems = null;

                    //Removes the selection when the parentgrid contains any selection.
                    if (selectioncController.SelectedRows.Count > 0)
                        rItems = selectioncController.SelectedRows.ToList<object>();
                    selectioncController.cancelSelectionChangedEvent = true;
                    //Adds the selection to the specified detailsViewIndex.
                    selectioncController.ResetSelection(detailsViewIndex, rItems, false);
                    selectioncController.cancelSelectionChangedEvent = false;
                    detailsViewGrid = parentDataGrid;
                    continue;
                }
                break;
            }
        }

        #endregion

        #region Selection Visiblity Helper

        /// <summary>
        /// Hides the row selection border for all rows in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">        
        /// Indicates whether the selection border should be hidden except for current row.
        /// </param>
        protected void HideAllRowSelectionBorder(bool exceptCurrentRow)
        {
            if (!exceptCurrentRow)
                this.DataGrid.RowGenerator.Items.ForEach(item => { if (item.IsSelectedRow) item.IsSelectedRow = false; });
            else
                this.DataGrid.RowGenerator.Items.ForEach(item => { if (!item.IsCurrentRow && item.IsSelectedRow) item.IsSelectedRow = false; });
        }

        /// <summary>
        /// Shows the selection border for all rows in SfDataGrid.
        /// </summary>
        protected void ShowAllRowSelectionBorder()
        {
            var FirstRowIndex = this.DataGrid.GetFirstDataRowIndex();
            var LastRowIndex = this.DataGrid.GetLastDataRowIndex();
            this.DataGrid.RowGenerator.Items.ForEach(item => 
            {                 
                if (!item.IsSelectedRow && item.RowIndex >= FirstRowIndex && item.RowIndex <= LastRowIndex) 
                    item.IsSelectedRow = true; 
            });
        }

        #endregion

        #region Index Helper

        /// <summary>
        /// Gets the index of next row corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the next row index.
        /// </param>
        /// <returns>
        /// Returns the index of next row.
        /// </returns>
        protected virtual int GetNextRowIndex(int index)
        {
            //When there is no records, then the index will be -1 and thei LastNavigatinRowIndex also returns -1, hence the below code is added.
            //It is also used when passing the last rowIndex as the index the same index has to be return.
            if (index >= this.DataGrid.GetLastNavigatingRowIndex()) return this.DataGrid.GetLastNavigatingRowIndex();

            //When View is null with AddNewRowPosition on Top and with UnboundRows, returns wrong value. Hence the GetFirstRowIndex
            //method is used.
            int firstRowIndex = this.DataGrid.GetFirstNavigatingRowIndex();
            if (index < firstRowIndex)
                return firstRowIndex;

            if (index >= this.DataGrid.VisualContainer.ScrollRows.LineCount)
                return -1;
            var nextIndex=0;
            //WRT-5949 While pressing the pagedown key with Custom RowHeight, next page index will be calculated by using RowHeights.SetRange method.
            //Instead of using GetNextPage method. Since it returns the row index which has default row height.
            if (DataGrid.CanQueryRowHeight())
            {
                for (int i = index + 1; i <= this.DataGrid.GetLastNavigatingRowIndex() && i >= this.DataGrid.GetFirstNavigatingRowIndex(); i++)
                {
                    double height = 0;
                    if (DataGrid.VisualContainer.RowsGenerator.QueryRowHeight(i, ref height))
                    {
                        DataGrid.VisualContainer.RowHeights.SetRange(i, i, height);
                        if (height == 0)
                            continue;
                    }
                    break;
                }
            }
            nextIndex = this.DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(index);// (this.CurrentCellIndex.RowIndex + 1);
            if (nextIndex == -1 || nextIndex >= (this.DataGrid.VisualContainer.RowCount - this.DataGrid.GetTableSummaryCount(TableSummaryRowPosition.Bottom)))
                nextIndex = index;
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended
                && nextIndex <= this.PressedRowColumnIndex.RowIndex)
            {
                //nextIndex = nextIndex < (this.DataGrid.VisualContainer.RowCount - this.DataGrid.TableSummaryRows.Count) ? nextIndex : (this.DataGrid.VisualContainer.RowCount - this.DataGrid.TableSummaryRows.Count - 1);
                //The below condition is used checked with while pressing the Shift+Down key the selection clears when the selection is in Last two Rows. 
                //so we have check with the nextIndex != index because if currentRowColumnIndex is LastRow then the nextIndex is -1.
                if (nextIndex != index)
                    nextIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(nextIndex);
                else if ((this.DataGrid is DetailsViewDataGrid) && nextIndex > index)
                    nextIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(nextIndex);
            }
            //In the Below Code-Snippet next Index is checked with the LastIndex because if the nextIndex is greater than the last RowIndex the selection will be maintained in the LastRowInwIndex.
            //While the DetailsViewGrid doesn't have the Record.
            if (this.DataGrid.IsInDetailsViewIndex(nextIndex) && nextIndex <= this.DataGrid.GetLastDataRowIndex()) 
            {
                //Added the code to get the next scroll line index to skip the recursive execution of the same method 
                //when DetailsDataRow is last row and not having no records.
                if (nextIndex == index && this.dataGrid.GetDetailsViewGridInView(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex) == null)
                    nextIndex = this.DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(index);
                else
                {
                    var lastParentIndex = DataGrid.GetLastNavigatingRowIndex();
                    var detailsViewGrid = this.DataGrid.GetDetailsViewGridInView(nextIndex);
                    //WPF-18257 Here we have check the condition, while the DetailsViewDataGrid has no Records to skip the selection for the corresponding Index value.
                    if (detailsViewGrid != null)
                    {
                        //Added the condition while navigating using keys without the DataRows in the DetailsViewGrid Exception raises. so checked with the lastDetailsViewGrid having records or not.
                        if (nextIndex > lastParentIndex)
                            return index;
                        //WPF-22527 if record count is zero and having addnewrow, need to provide support for navigation in child grid addnewrow.
                        nextIndex = (detailsViewGrid.GetRecordsCount() > 0) ||
                            (detailsViewGrid.HasView && (detailsViewGrid.FilterRowPosition != FilterRowPosition.None || detailsViewGrid.AddNewRowPosition != AddNewRowPosition.None)) 
                            ? nextIndex : this.GetNextRowIndex(nextIndex);
                    }
                }
            }
            return nextIndex;
        }

        /// <summary>
        /// Gets the index of previous row corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the previous row index.
        /// </param>
        /// <returns>
        /// Returns the index of previous row.
        /// </returns>
        protected virtual int GetPreviousRowIndex(int index)
        {
           //index value is compared with LastRowIndex of the grid WPF-20287
            if (index > this.DataGrid.GetLastNavigatingRowIndex())
                return this.DataGrid.GetLastNavigatingRowIndex();

            if (index <= this.DataGrid.GetFirstNavigatingRowIndex())
                return  this.DataGrid.GetFirstNavigatingRowIndex();

            int previousIndex = index;
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
            {
                //WPF-25769 When the pressed index is less than previousindex means, previousindex is calculated wrongly when having detailsview
                //so calculate the previousindex again based on the HasDetailsView.
                var tempPreviousIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
                if (previousIndex <= this.PressedRowColumnIndex.RowIndex || this.lastPressedKey == Key.Tab || (this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(tempPreviousIndex)))
                    previousIndex = tempPreviousIndex;
            }
            else
            {
                if (DataGrid.CanQueryRowHeight())
                {
                    for (int i = index - 1; i <= this.DataGrid.GetLastNavigatingRowIndex() && i >= this.DataGrid.GetFirstNavigatingRowIndex(); i--)
                    {
                        double height = 0;
                        if (DataGrid.VisualContainer.RowsGenerator.QueryRowHeight(i, ref height))
                        {
                            DataGrid.VisualContainer.RowHeights.SetRange(i, i, height);
                            if (height == 0)
                                continue;
                        }
                        break;
                    }
                }
                previousIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(index);
            }

            if (this.DataGrid.IsInDetailsViewIndex(previousIndex))
            {
                var detailsViewGrid = this.DataGrid.GetDetailsViewGridInView(previousIndex);
                //WPF-18257 Here we have check the condition, while the DetailsViewDataGrid has no Records to skip the selection for the corresponding Index value.
                if (detailsViewGrid != null)
                {
                    //The Below Codes has been find out the DetailsViewGrid has the Record or Not. so that we can get the Previous RowIndex while Navigating through keys.
                    previousIndex = (detailsViewGrid.GetRecordsCount() > 0) || ((detailsViewGrid.FilterRowPosition != FilterRowPosition.None || detailsViewGrid.AddNewRowPosition != AddNewRowPosition.None) 
                        && detailsViewGrid.HasView) ? previousIndex : this.GetPreviousRowIndex(previousIndex);
                }
            }
            return previousIndex;
        }

        /// <summary>
        /// Updates the current row index based on <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="skipIfNotDataRow"> 
        /// Indicates whether the current row index skipped other than the data row.
        /// </param>
        /// <param name="canFocusGrid">
        /// Indicates the SfDataGrid can be focused after the current row index is updated.
        /// </param>
        protected void UpdateCurrentRowIndex(bool skipIfNotDataRow = true, bool canFocusGrid = true)
        {
            //Added the to reset the current cell when view is null for Customer Issue WPF-17028
            if (this.DataGrid.View == null)
            {
                if (this.CurrentCellManager.CurrentCell != null)
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.ResetCurrentRowColumnIndex();
                return;
            }

            bool isAvailable = false;
            if (this.DataGrid.CurrentItem != null)
            {
                // Need to get the record or record index from DisplayElements when the Source is IQueryable. Since we will keep null record entries View.Records. Entries will be kept in Group.Records alone.
                var record = dataGrid.isIQueryable && dataGrid.GridModel.HasGroup ? dataGrid.View.TopLevelGroup.DisplayElements.GetItem(this.DataGrid.CurrentItem) : this.DataGrid.View.Records.GetRecord(this.DataGrid.CurrentItem);
                if (record != null)
                {
                    if ((this.DataGrid.View as CollectionViewAdv).IsGrouping)
                    {
                        var group = record.Parent as Group;
                        isAvailable = this.DataGrid.CheckGroupExpanded(group);
                    }
                    else
                        isAvailable = true;
                }
            }
            else if (this.CurrentCellManager.IsInNonDataRows())
                isAvailable = true;
            var rowIndex = -1;
            
            if (isAvailable)
            {
                if (this.CurrentCellManager.IsAddNewRow)
                    rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                else if (this.CurrentCellManager.IsFilterRow)
                    rowIndex = this.DataGrid.GetFilterRowIndex();
                else if (this.CurrentCellManager.IsUnBoundRow)
                    rowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(this.CurrentCellManager.CurrentUnboundRowInfo);
                else if (this.SelectedRows.Count > 0 && this.SelectedRows.LastOrDefault().NodeEntry is NestedRecordEntry)
                    rowIndex = this.DataGrid.ResolveToRowIndex(this.SelectedRows.LastOrDefault().NodeEntry);
                else
                    rowIndex = this.dataGrid.ResolveToRowIndex(this.DataGrid.CurrentItem);
            }
            else if (this.SelectedRows.Count > 0 && !skipIfNotDataRow)
            {
                rowIndex = this.SelectedRows.LastOrDefault().RowIndex;
            }

            //View.CurrentItem null check is added for the customer issue WPF-17028
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex != rowIndex ||
                (this.DataGrid.HasView && this.DataGrid.View.CurrentItem != null && this.DataGrid.CurrentItem != null &&
                 this.DataGrid.CurrentItem != ((RecordEntry) this.DataGrid.View.CurrentItem).Data))
            {
                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                var rowColumnIndex = new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                if (rowIndex != -1)
                {
                    CurrentCellManager.SelectCurrentCell(rowColumnIndex, canFocusGrid);
                }
                else
                    this.CurrentCellManager.UpdateGridProperties(rowColumnIndex);
            }
            //WPF-17816 , If CurrentCell Rowindex and CurrentItem is not changed while filtering, 
            //above conditon will fail and focus will remain in Header. Added code to Focus Grid in this case
            else if(canFocusGrid)
            {
#if WinRT
                this.dataGrid.Focus(FocusState.Programmatic);
#else
                this.dataGrid.Focus();
#endif
            }
        }

        #endregion

        #endregion

        #region Drag Operations

        /// <summary>
        /// Processes the row selection when mouse pointer is moved on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse related interaction.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point.    
        /// </param>
        protected virtual void ProcessPointerMoved(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
#if WPF
            this.isMousePressed = args.LeftButton == MouseButtonState.Pressed && isMousePressed;
#else
            if (args.Pointer.PointerDeviceType != PointerDeviceType.Mouse)
                return;
            
            this.isMousePressed = args.Pointer.IsInContact && isMousePressed;
#endif
            if (this.DataGrid.SelectionMode == GridSelectionMode.None || this.DataGrid.SelectionMode == GridSelectionMode.Single || this.PressedRowColumnIndex.RowIndex < 0 || rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex() || rowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() || !this.isMousePressed || this.DataGrid.IsAddNewIndex(this.PressedRowColumnIndex.RowIndex))
                return;
            
            if (this.CurrentCellManager.CurrentRowColumnIndex == rowColumnIndex && this.SelectedRows.Contains(rowColumnIndex.RowIndex))
                return;

            if ((rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() || isMouseCaptured) && this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                return;
#if !WPF
            //Point is calculated based on VisualContainer like WPF.
            Point pointerMovedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            Point pointerMovedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#endif

            double xPosChange = Math.Abs(pointerMovedPosition.X - pressedPosition.X);
            double yPosChange = Math.Abs(pointerMovedPosition.Y - pressedPosition.Y);

            if (!this.DataGrid.AutoScroller.InsideScrollBounds.Contains(pointerMovedPosition) && this.DataGrid.AutoScroller.InsideScrollBounds.Contains(pressedPosition))
                return;

            if (xPosChange < 10 && yPosChange < 10)
                return;

            if (this.CurrentCellManager.CurrentCell != null && this.CurrentCellManager.CurrentCell.IsEditing)
            {
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == this.PressedRowColumnIndex.RowIndex && rowColumnIndex.ColumnIndex == this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex)
                    return;
                else if (!CurrentCellManager.CheckValidationAndEndEdit())
                    return;
            }

            if (this.CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex)
            {
                if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                {
                    this.ProcessDetailsViewGridPointerMoved(args, rowColumnIndex);
                }

                int detailsViewIndex = this.DataGrid.GetSelectedDetailsViewGridRowIndex();
                if (this.DataGrid.SelectionMode != GridSelectionMode.Single && this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex != rowColumnIndex.RowIndex)
                {
                    //In Multiple mode selection, there is possible to move the current cell to one NestedGrid but the selection will 
                    //be in some other nestedGrid. Hence the current cell selection will be cleared by using correspondin rowColumn index
                    //particular NetedGrid.
                    var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                    if (detailsViewGrid != null)
                    {
                        if (!this.ClearDetailsViewGridSelections(detailsViewGrid))
                            return;
                        if (this.DataGrid.SelectedDetailsViewGrid == detailsViewGrid)
                            this.DataGrid.SelectedDetailsViewGrid = null;
                    }
                }
                //If the CurrentCell and Selection is in Differnt nested grids then the CurrentCell selection will be cleared in above code
                //the SelectedDetailsViewGrid will be cleared from below code.

                //When the SelectionMode as Multiple we have selected the first Nestedgrid and move the current cell to another nested grid, and 
                //again select the first nestedgrid means the previous selection is cleared and the new seletcion is maintained. So we need to 
                //Check the detailsViewIndex with rowindex and then clear the selection.
                if (this.DataGrid.SelectedDetailsViewGrid != null && detailsViewIndex != rowColumnIndex.RowIndex)
                {
                    if (!this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                        return;
                    this.DataGrid.SelectedDetailsViewGrid = null;
                }
            }

            this.ProcessDragSelection(args, rowColumnIndex);
        }

        /// <summary>
        /// Processes the row selection when mouse pointer moved on the DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse related interaction.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point in DetailsViewDataGrid.    
        /// </param>
        protected virtual void ProcessDetailsViewGridPointerMoved(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.OfType<DetailsViewDataRow>().FirstOrDefault(row => row.DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                                   ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                                   : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowcolIndex = new RowColumnIndex(detailsViewDataRow.RowIndex, colIndex);
            parentDataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Move, args), rowcolIndex);
            parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
        }

        /// <summary>
        /// Processes the row selection when the mouse pointer is dragged on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the mouse related operations.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The starting rowColumnIndex of the drag selection.
        /// </param>
        /// <remarks>
        /// This method will be raised when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> is Multiple or Extended.
        /// </remarks>      
        protected override void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (!this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
            {
                rowColumnIndex.ColumnIndex = this.DataGrid.AllowFocus(rowColumnIndex) ? rowColumnIndex.ColumnIndex : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                if (!CurrentCellManager.HandlePointerOperation(args, rowColumnIndex))
                    return;
                
                this.DataGrid.HideRowFocusBorder();
                if (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex))
                {
                    if (this.DataGrid.HasView && DataGrid.View.IsAddingNew)
                    {
                        this.SuspendUpdates();
                        this.DataGrid.GridModel.AddNewRowController.CommitAddNew();
                        this.ResumeUpdates();
                    }
                    this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                }

                this.ProcessShiftSelection(rowColumnIndex.RowIndex, SelectionReason.PointerMoved);
            }
        }
        #endregion
    
    }
}
