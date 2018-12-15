#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
    using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.UI.Xaml.Media;
    using Windows.UI.Xaml;
    using Windows.UI.Core;
    using Windows.Foundation;
    using Windows.Devices.Input;
#else
    using System.Windows.Media;
    using System.Windows.Input;
    using System.Windows;

    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif

    /*
     * Work Flow and Initialization Process
     1. Validation & EndEdit
     2. Removing Selections for other Grid's //If Selection is performed in Nested Grid
     * Adding Selection(DetailsViewDataRow) to Parent Grid as per the following order. //If Selection is performed in Nested Grid
     2. CurrentColumn
     3. CurrentCellInfo
     4. CurrentItem
     5. SelectedCells
      
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
    /// Represents a class that implements the selection behavior of cell in SfDataGrid.
    /// </summary>
    public class GridCellSelectionController : GridBaseSelectionController
    {
        #region Fields
        
        RowColumnIndex pressedIndex = RowColumnIndex.Empty;

        //Behavior: To maintain the selection while selecting the cells using CTRL key.
        /// <summary>
        /// Gets or sets the collection of previous selected cells while selecting the cells using CTRL key.
        /// </summary>
        protected List<GridCellInfo> previousSelectedCells;

        //Behavior: To get lastselectedIndex of the Cell while dragging.
        /// <summary>
        /// Gets or sets the index of last selected row.
        /// </summary>
        protected internal RowColumnIndex lastSelectedIndex = RowColumnIndex.Empty;

        /// <summary>
        /// Gets or sets a value that determines whether shift selection performed in SfDataGrid.
        /// </summary>
        protected bool isInShiftSelection = false;
        private bool isdisposed = false;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the pressed index in SfDataGrid.
        /// </summary>
        [Obsolete("This PressedIndex property is obsolete; use PressedRowColumnIndex instead")]
        protected internal RowColumnIndex PressedIndex
        {
            get { return pressedIndex; }
            protected set { pressedIndex = value; }
        }

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of  <see cref="Syncfusion.UI.Xaml.Grid.GridCellSelectionController"/> class.
        /// </summary>
        /// <param name="grid">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> .
        /// </param>
        public GridCellSelectionController(SfDataGrid grid) : base(grid)
        {   
            previousSelectedCells = new List<GridCellInfo>();
        }

        #endregion

        #region IGridCellSelectionController Members

        /// <summary>
        /// Selects all the cells in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public override void SelectAll()
        {
            if (this.DataGrid.View is VirtualizingCollectionView)
                return;

            if (this.DataGrid.SelectionMode != GridSelectionMode.None && this.DataGrid.SelectionMode != GridSelectionMode.Single)
            {
                var addedItems = new List<GridCellInfo>();
                var aItems = this.SelectedCells.ConvertToGridCellInfoList();
                int currentIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

                if (this.DataGrid.SelectedDetailsViewGrid != null)
                {
                    if (!ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                        return;
                    this.DataGrid.SelectedDetailsViewGrid = null;
                    currentIndex = -1;
                }
                if (this.DataGrid.NotifyListener != null && !this.ProcessParentGridSelection())
                    return;
                if (currentIndex < 0)
                    currentIndex = this.DataGrid.GetFirstDataRowIndex();

                CurrentCellManager.SelectCurrentCell(new RowColumnIndex(currentIndex,columnIndex));

                int firstRowIndex = this.DataGrid.GetFirstDataRowIndex();
                int lastRowColumnIndex = this.DataGrid.GetLastDataRowIndex();

                if (firstRowIndex <= lastRowColumnIndex)
                {
                    while (firstRowIndex <= lastRowColumnIndex)
                    {
                        if (!this.DataGrid.IsInDetailsViewIndex(firstRowIndex))
                        {
                            var rowData = this.DataGrid.GetRecordAtRowIndex(firstRowIndex);
                            var nodeEntry = this.DataGrid.GetNodeEntry(firstRowIndex);
                            GridCellInfo gridCellInfo = null;
                            if (rowData != null)
                            {
                                this.DataGrid.Columns.ForEach(column =>
                                {
                                    gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);

                                    if (column.AllowFocus && !column.IsHidden)
                                    {
                                        addedItems.Add(gridCellInfo);
                                    }
                                });
                            }
                            else
                            {
                                gridCellInfo = new GridCellInfo(this.DataGrid.CurrentColumn, null, nodeEntry, firstRowIndex);
                                addedItems.Add(gridCellInfo);
                            }
                        }
                        firstRowIndex = firstRowIndex == this.DataGrid.GetLastDataRowIndex() ? ++firstRowIndex : this.GetNextRowIndex(firstRowIndex);
                    }
                }

                var addedItem = addedItems.Except(aItems).Cast<object>().ToList<object>();

                this.AddCells(addedItem.Cast<object>().ToList<object>());
                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();
                this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                RaiseSelectionChanged(addedItem, new List<object>());
            }
        }
        
        /// <summary>
        /// Moves the current cell for the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current cell selection is cleared while moving the current cell.
        /// </param>     
        /// <remarks>
        /// This method is not applicable when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> is Row.
        /// </remarks> 
        public override void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true)
        {
            //WPF-18479 we have checked with this condition because of Exception arises while passing the RowIndex as -1.
            if (this.DataGrid.SelectionMode == GridSelectionMode.None || !CurrentCellManager.CanMoveCurrentCell(rowColumnIndex))            
                return;

            if (!ClearSelectedDetailsViewGrid())
                return;

            if (this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                throw new InvalidOperationException("Can't move the current cell to DetailsViewIndex.");

            if (!CurrentCellManager.ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Program))
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
                this.RemoveSelection(rowColumnIndex, this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>());

            //The below condition is used to check whether the RowColumnIndex is greaterthan the FirstRowColumnIndex or not Because in this case we could not maintain the SelectedCells.
            if (rowColumnIndex.RowIndex >= this.dataGrid.GetFirstNavigatingRowIndex() && rowColumnIndex.ColumnIndex >= this.dataGrid.GetFirstColumnIndex())
            {                
                this.SelectedCells.Add(this.DataGrid.CurrentCellInfo);
                if (this.DataGrid.CurrentCellInfo.IsDataRowCell || this.DataGrid.CurrentCellInfo.IsAddNewRow || this.DataGrid.CurrentCellInfo.IsUnBoundRow || this.DataGrid.CurrentCellInfo.IsFilterRow)
                    this.ShowCellSelection(this.DataGrid.GetDataColumnBase(rowColumnIndex));
                else
                    this.DataGrid.ShowRowSelection(rowColumnIndex.RowIndex);
                //WPF-25771 Reset the parentgrid selection when the selection in DetailsViewGrid
                ResetParentGridSelection();
                //WPF-25760 - Need to update the SelectedIndex and SelectedItem by RefreshSelectedIndex method
                RefreshSelectedIndex();
                if (this.DataGrid.CurrentCellInfo.IsAddNewRow)
                {
                    this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                }
            }           
            this.ResumeUpdates();
            ResetFlags();
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
            throw new InvalidOperationException("Rows will be selected when the Selection Unit is Row");
        }

         /// <summary>
        /// Handles the cell selection when the SelectedItems, Columns and DataSource collection changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for collection changes.
        /// </param>
        /// <param name="reason">
        /// Contains the corresponding <see cref="Syncfusion.UI.Xaml.Grid.CollectionChangedReason"/> for collection changes.
        /// </param>
        /// <remarks>
        /// This method is called when the collection changes on SelectedItems, Columns and DataSource properties in SfDataGrid.
        /// </remarks>
        public override void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, CollectionChangedReason reason)
        {
            /*
             * Need to remove SelectedCells when Column is removed.
             * */
            switch (reason)
            {
                case CollectionChangedReason.ColumnsCollection:
                    CurrentCellManager.HandleColumnsCollectionChanged(e);
#if !WinRT && !UNIVERSAL
                    if (e.Action == NotifyCollectionChangedAction.Remove && !SfDataGrid.suspendForColumnMove)
#else
                    if (e.Action == NotifyCollectionChangedAction.Remove)
#endif
                        ProcessOnColumnRemoved((GridColumn)e.OldItems[0]);
                    this.ResetFlags();
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
        /// Handles the cell selection when the group is expanded or collapsed in SfDataGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the group.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Decides whether the group is expanded or not.
        /// </param>
        /// <remarks>
        /// This method is invoked when the group is expanded or collapsed.
        /// </remarks>
        public override void HandleGroupExpandCollapse(int index, int itemsCount, bool isExpanded)
        {
            if (isExpanded)
                ProcessGroupExpanded(index, itemsCount);
            else
                ProcessGroupCollapsed(index, itemsCount);
        }

        /// <summary>
        /// Release all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridCellSelectionController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            base.Dispose(isDisposing);
            if (isDisposing)
            {
                if (this.SelectedCells != null)
                {
                    this.SelectedCells.Clear();
                    this.SelectedCells = null;
                }
            }
            isdisposed = true;
        }

        #endregion

        #region Protected Methods

        #region Pointer Operation Methods

        /// <summary>
        /// Handles the cell selection when any of <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc that are performed in SfDataGrid.
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
        public override void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex)
        {
            switch (args.Operation)
            {
                case PointerOperation.Pressed:
                    ProcessPointerPressed(args.OriginalEventArgs as MouseButtonEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.Released:
                    ProcessPointerReleased(args.OriginalEventArgs as MouseButtonEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.Tapped:
                    this.ProcessOnTapped(args.OriginalEventArgs as TappedEventArgs, rowColumnIndex);
                    break;
                case PointerOperation.DoubleTapped:
                    this.ProcessOnDoubleTapped(args.OriginalEventArgs as DoubleTappedEventArgs ,rowColumnIndex);
                    break;
                case PointerOperation.Move:
                    ProcessPointerMoved(args.OriginalEventArgs as MouseEventArgs, rowColumnIndex);
                    break;
#if WPF
                case PointerOperation.Wheel:
                    this.ProcessPointerWheel(args.OriginalEventArgs as MouseWheelEventArgs, rowColumnIndex);
                    break;
#endif
            }
        }

#if WPF
        /// <summary>
        /// Processes the cell selection while scrolling the SfDataGrid using mouse wheel.
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
        /// Processes the cell selection when the mouse pointer is pressed in SfDataGrid.
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

            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                return;

#if WPF
            if (args != null && this.DataGrid.AllowSelectionOnPointerPressed && this.SelectedCells.Contains(this.DataGrid.GetGridCellInfo(rowColumnIndex)) && args.ChangedButton == MouseButton.Right)
                return;

            pressedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            pressedPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
            
            pressedVerticalOffset = this.dataGrid.VisualContainer.ScrollOwner != null ? this.dataGrid.VisualContainer.ScrollOwner.VerticalOffset : 0;
#endif

            if (isInShiftSelection && (!SelectionHelper.CheckShiftKeyPressed() || SelectionHelper.CheckControlKeyPressed() || this.DataGrid.SelectionMode == GridSelectionMode.Multiple))
                isInShiftSelection = false;
            //WPF-29408 we need to set the Key.None only when we not pressed the shiftkey otherwise we should keep the lastPressedKey.
            if (!SelectionHelper.CheckShiftKeyPressed())
                lastPressedKey = Key.None;
            lastSelectedIndex = RowColumnIndex.Empty;
            //While dragging from footer UnBoundRow the Drag selection has been processed hence LastNavigatingRowIndex condition is added.
            if (!this.CurrentCellManager.AllowFocus(rowColumnIndex) || 
                rowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() ||
                rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex() || 
                (rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() && 
                !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex) &&
                ((this.DataGrid.DetailsViewManager.HasDetailsView && 
                this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex)) || 
                rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex()) || 
                (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))))
            {
                //This flag is set to skip the drag selection when drag from indent cell or AllowFocus false column.
                cancelDragSelection = true;
                return;
            }

#if WPF                                
            this.isMousePressed = true;                        
#endif

            cancelDragSelection = false;

            bool needToSelectWholeRow = DataGrid.showRowHeader ? DataGrid.SelectionUnit == GridSelectionUnit.Any && rowColumnIndex.ColumnIndex == 0 : false;

            if (rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() && !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
            {
                rowColumnIndex.ColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                bool isInSummary = false;
                if (this.DataGrid.GridModel.HasGroup)
                {
                    var nodeEntry = DataGrid.View.TopLevelGroup.DisplayElements[DataGrid.ResolveToRecordIndex(rowColumnIndex.RowIndex)];
                    isInSummary = nodeEntry is SummaryRecordEntry || nodeEntry is Group;
                }
                cancelDragSelection = !isInSummary  && !needToSelectWholeRow;
            }
                this.isMousePressed = true;

            if (!SelectionHelper.CheckShiftKeyPressed() || this.PressedRowColumnIndex.RowIndex < 0)
            {
                this.SetPressedIndex(rowColumnIndex);
            }

            if (CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex || this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
            {
                if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                {
                    this.ProcessDetailsViewGridPointerPressed(args, rowColumnIndex);
                }
            }
            
            if (this.DataGrid.AllowSelectionOnPointerPressed)
            {
                ProcessPointerSelection(args, rowColumnIndex, needToSelectWholeRow);
            }
        }

        /// <summary>
        /// Processes the cell selection when the mouse pointer is released in SfDataGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of mouse released point.
        /// </param>
        /// <remarks>
        /// The selection is initialized in pointer released state when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSelectionPointerPressed"/> set as false.
        /// </remarks>
        protected virtual void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            // UWP-5604 AutoScrolling started while sorting the column using touch
            this.isMousePressed = false;
            var selectedDetailsViewGrid = this.DataGrid.SelectedDetailsViewGrid;
            while (selectedDetailsViewGrid != null)
            {
                if (selectedDetailsViewGrid.SelectedDetailsViewGrid != null)
                    selectedDetailsViewGrid = selectedDetailsViewGrid.SelectedDetailsViewGrid;
                else
                {
                    this.DataGrid.SelectedDetailsViewGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
                    selectedDetailsViewGrid = null;
                }
            }

            this.DataGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;

            if (this.DataGrid.SelectionMode == GridSelectionMode.None || this.DataGrid.AllowSelectionOnPointerPressed || rowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex())
                return;

#if WPF
            Point pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);
#else
            //Point is calculated based on VisualContainer like WPF.
            Point pointerReleasedRowPosition = args == null ? new Point() : SelectionHelper.GetPointPosition(args, this.DataGrid.VisualContainer);

            var releasedVerticalOffset = this.dataGrid.VisualContainer.ScrollOwner != null ? this.dataGrid.VisualContainer.ScrollOwner.VerticalOffset : 0;

            //  get the difference of pressed and released vertical offset value.
            var verticalOffsetChange = Math.Abs(releasedVerticalOffset - pressedVerticalOffset);
#endif

            double xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            double yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);

             bool isValidated = ValidationHelper.IsCurrentRowValidated || ValidationHelper.IsCurrentCellValidated;

#if !WPF
            //Selection need to be skipped if the scrolling happens in SfDataGrid. In UWP while Scrolling whole VisualContainer panel has moved.
            //So pressed and released position of vertical offset value is same. In this case we need to skip selection.So that difference of pressed and released position vertical offset has checked.
            //if verticalOffsetChange is lessthan 10 then  allow selection other than consider it has verticaly scrolling in SfDataGrid.
            if (xPosChange < 20 && yPosChange < 20 && verticalOffsetChange < 10)            
            { 
#else
            if (xPosChange < 20 && yPosChange < 20)
            {
                if (args != null && (this.SelectedCells.Contains(this.DataGrid.GetGridCellInfo(rowColumnIndex)) || !isValidated) && args.ChangedButton == MouseButton.Right)
                {
                    //WPF-17423 Here we can handled the contextmenu when CurrentRow not Validated 
                    //WPF-25106 - Check rowColumnIndex with CurrentCellManager.CurrentRowColumnIndex to enable editor context menu.  (WPF-17423  break)
                    if (!isValidated && !rowColumnIndex.Equals((CurrentCellManager.CurrentRowColumnIndex)))
                        args.Handled = true;
                    return;
                }
#endif

                if ((rowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() && ((this.DataGrid.DetailsViewManager.HasDetailsView && this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex)) || rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex())) || !this.CurrentCellManager.AllowFocus(rowColumnIndex) || (this.lastSelectedIndex != RowColumnIndex.Empty && SelectionHelper.CheckControlKeyPressed()) || (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex)))
                    return;

                if (CurrentCellManager.CurrentRowColumnIndex != rowColumnIndex || this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                {
                    if (this.DataGrid is DetailsViewDataGrid && this.DataGrid.NotifyListener != null)
                    {
                        this.ProcessDetailsViewGridPointerReleased(args, rowColumnIndex);
                    }
                }

                bool needToSelectWholeRow = this.DataGrid.showRowHeader ? this.DataGrid.SelectionUnit == GridSelectionUnit.Any && rowColumnIndex.ColumnIndex == 0 : false;  
                //CheckRowColumnIndex is used here to check with the Pressed Index value because the rowColumnIndex value resolved here the EdingGets LostFocus  while pressing the Indent column.
                var checkRowColumnIndex = rowColumnIndex;
                //The column index will be less than first column index when clicking on Caption row which is resolved in this code.
                if (rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() && !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) && !this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
                {
                    checkRowColumnIndex.ColumnIndex = this.DataGrid.ResolveColumnIndex(rowColumnIndex);
                }

                //WPF-14410 Here we can handled the contextmenu when pressedIndex and rowColumnIndex is different 
                if (this.PressedRowColumnIndex != checkRowColumnIndex && !SelectionHelper.CheckShiftKeyPressed())
                {
#if WPF
                    if (args == null) return;
                    if (args.ChangedButton == MouseButton.Right)
                        args.Handled = true;
#endif
                    return;
                }
                ProcessPointerSelection(args, rowColumnIndex, needToSelectWholeRow);
                
            }
#if WPF
            else if ((!isValidated || PressedRowColumnIndex != rowColumnIndex) && args.ChangedButton == MouseButton.Right)
            {                
                args.Handled = true;
            }
#endif
        }
        
        /// <summary>
        /// Processes the cell selection when the mouse point is tapped on the grid cell. 
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
            if (!ValidationHelper.IsCurrentCellValidated || isSuspended || !this.DataGrid.AllowFocus(currentRowColumnIndex) || this.DataGrid.SelectionMode == GridSelectionMode.None || (this.lastSelectedIndex != RowColumnIndex.Empty && this.CurrentCellManager.CurrentRowColumnIndex != this.lastSelectedIndex))
                return;

#if WPF
            Point pointerReleasedRowPosition = e == null ? new Point() : SelectionHelper.GetPointPosition(e, this.DataGrid.VisualContainer);
#else
            Point pointerReleasedRowPosition = e == null ? new Point() : e.GetPosition(this.DataGrid.VisualContainer);
#endif

            double xPosChange = Math.Abs(pointerReleasedRowPosition.X - pressedPosition.X);
            double yPosChange = Math.Abs(pointerReleasedRowPosition.Y - pressedPosition.Y);
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
        /// Processes the selection when the mouse point is double tapped on the grid cell.
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
            if (!ValidationHelper.IsCurrentCellValidated || isSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None || this.CurrentCellManager.CurrentRowColumnIndex != currentRowColumnIndex)
                return;

            if (this.DataGrid.EditTrigger == EditTrigger.OnDoubleTap)
                CurrentCellManager.ProcessOnDoubleTapped(e);
          
        }

        /// <summary>
        /// Processes the cell selection when the mouse pointer is pressed on the DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer pressed action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the pointer pressed location.
        /// </param>
        /// <remarks>
        /// This method invoked when the pointer pressed using touch or mouse in DetailsViewDataGrid.
        /// </remarks>
        protected virtual void ProcessDetailsViewGridPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.FirstOrDefault(row => (row is DetailsViewDataRow) && (row as DetailsViewDataRow).DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < 0
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
        /// Processes the cell selection when the mouse pointer is released from the DetailsViewDataGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for pointer released action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the pointer released location.
        /// </param>
        protected virtual void ProcessDetailsViewGridPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.FirstOrDefault(row => (row is DetailsViewDataRow) && (row as DetailsViewDataRow).DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < 0
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
        /// Processes the cell selection when the pointer pressed or released interactions performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse position.
        /// </param>
        protected internal void ProcessPointerSelection(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex, bool needToSelectWholeRow)
        {
            RowColumnIndex previousRowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;

            if (!CurrentCellManager.HandlePointerOperation(args, rowColumnIndex))
                return;

            rowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;            
            int detailsViewIndex = this.DataGrid.GetSelectedDetailsViewGridRowIndex();
            //In SelectionMode - Multiple, CurrentCell will be in some other NestedGrid or parentGrid when moving via Keyboard. So added condition to check detailsViewIndex and rowColumnIndex.RowIndex 
            //along with previousCurrentCellIndex.RowIndex != rowColumnIndex.RowIndex
            if ((detailsViewIndex > -1 && detailsViewIndex != rowColumnIndex.RowIndex) || previousRowColumnIndex.RowIndex != rowColumnIndex.RowIndex)
            {
               if (this.DataGrid.SelectionMode != GridSelectionMode.Single && this.DataGrid.IsInDetailsViewIndex(previousRowColumnIndex.RowIndex) && previousRowColumnIndex.RowIndex != rowColumnIndex.RowIndex)
                {
                    //In Multiple mode selection, there is possible to move the current cell to one NestedGrid but the selection will 
                    //be in some other nestedGrid. Hence the current cell selection will be cleared by using correspondin rowColumn index
                    //particular NetedGrid.
                    var detailsViewGrid = this.DataGrid.GetDetailsViewGrid(previousRowColumnIndex.RowIndex);
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

            if (ValidationHelper.IsCurrentCellValidated)
            {
                if (this.DataGrid.SelectionMode == GridSelectionMode.Extended && SelectionHelper.CheckShiftKeyPressed())
                    this.ProcessShiftSelection(this.CurrentCellManager.CurrentRowColumnIndex, previousRowColumnIndex, Key.None, needToSelectWholeRow);
                else
                    this.ProcessSelection(this.CurrentCellManager.CurrentRowColumnIndex, SelectionReason.PointerReleased, needToSelectWholeRow);
            }

            if (this.DataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            else if (DataGrid.IsAddNewIndex(previousRowColumnIndex.RowIndex))
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
        }

        #endregion

        #region Keyboard Navigation Metods

        /// <summary>
        /// Handles the cell selection when the keyboard interactions that are performed in DetailsViewDataGrid.
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
                case Key.Up:
                    {
                        if (firstRowIndex == -1)
                            return false;

                        if (this.CurrentCellManager.CurrentRowColumnIndex.IsEmpty || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == -1)
                        {
                            this.CurrentCellManager.SetCurrentRowColumnIndex(new RowColumnIndex(lastRowIndex + 1, this.CurrentCellManager.GetFirstCellIndex()));
                            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
                        }
                        var previousRowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        //WPF-32808 inRange property used for checking whether the current cell is merged cell or not.
                        CoveredCellInfo inRange = null;
                        if (dataGrid.CanQueryCoveredRange())
                            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, previousRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                        //WPF-35962 while moving child grid to parent grid, whether the currentcell is mergedcell also the mergedcell top index is 

                        if ((previousRowIndex == firstRowIndex && previousRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || (inRange != null && inRange.Top == firstRowIndex && inRange.Bottom >= this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                        {
                            //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                return false;

                            //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                            if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow)
                            {
                                if (this.DataGrid.View.IsAddingNew)
                                    CommitAddNew();

                                dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                            }

                            //Clears the selection by when the current row index in first row while the SelectionMode is not multiple.
                            if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                            {
                                this.ClearSelections(false);
                            }
                            else
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            }
                            return false;
                        }
                        this.ProcessKeyDown(args);
                        return args.Handled;
                    }
                case Key.Enter:
                case Key.Down:
                    {
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < firstRowIndex)
                            this.SetPressedIndex(new RowColumnIndex(firstRowIndex, this.CurrentCellManager.GetFirstCellIndex()));
                        var nextRowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < firstRowIndex
                                               ? firstRowIndex
                                               : this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        int nextIndex = this.CurrentCellManager.GetNextRowIndex(nextRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        //needToMoveToLastRow flag check with the RowIndex if it is LastRowIndex then it will be Handled by the ParentGrid. If the lastRowIndex is DetailsViewIndex means we have to move the selection to the ParentGrid.
                        bool needToMoveToLastRow = SelectionHelper.CheckControlKeyPressed() && nextRowIndex != this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && nextRowIndex == lastRowIndex && this.DataGrid.IsInDetailsViewIndex(nextRowIndex);
#if !WPF
                        if((((nextRowIndex == lastRowIndex && nextRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || nextRowIndex == -1 || nextRowIndex > lastRowIndex) && !this.CheckIsLastRow(this.DataGrid)) || needToMoveToLastRow)
#else
                        //WPF-23996(Issue 3) : We have clear the selevction when the Selection in before to lastrow 
                        //this is occurs because we have check the nextindex with lastindex, this is process for MergedCell,
                        //So check we have process the MergedCell or not
                        if (((nextRowIndex == lastRowIndex && nextRowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) || nextRowIndex == -1 || nextRowIndex > lastRowIndex) || needToMoveToLastRow || (dataGrid.CanQueryCoveredRange() && nextIndex == lastRowIndex))
#endif
                        {
                            bool isLastRow = CheckIsLastRow(this.DataGrid);
                            if (isLastRow && args.Key == Key.Enter && this.dataGrid.HasView && this.dataGrid.View.IsAddingNew)
                            {
                                this.ProcessKeyDown(args);
                                return args.Handled;
                            }

                            //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                            if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                return false;

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
                                this.ClearSelections(false);
                            else
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            }
                            return false;
                        }

                        this.ProcessKeyDown(args);
                        return args.Handled;
                    }
                case Key.Tab:
                    {
                        if (this.DataGrid.GetRecordsCount() < 1 && DataGrid.AddNewRowPosition == AddNewRowPosition.None)
                        {
                            this.ClearSelections(false);
                            return false;
                        }
                        if (SelectionHelper.CheckShiftKeyPressed())
                        {
                            if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex())
                                this.CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.GetLastCellIndex() + 1);
                            var columnIndex = CurrentCellManager.GetPreviousCellIndex(dataGrid.FlowDirection);
                            if (columnIndex == this.CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection) 
                                && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex)
                                {
                                var rowIndex = this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                                if (rowIndex <= this.DataGrid.GetFirstNavigatingRowIndex() && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                                {
                                    if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                        return false;

                                    //WPF-22527 Commit Addnew row if is in edit while navigation using up key 
                                    if (this.DataGrid.HasView && this.CurrentCellManager.IsAddNewRow)
                                    {
                                        if (this.DataGrid.View.IsAddingNew)
                                            CommitAddNew();

                                        dataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                    }

                                    //Clears the selection when the current row in First row of the DeatailsView grid.
                                    if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                    {
                                        this.ClearSelections(false);
                                    }
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

                            if (columnIndex == this.CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection)
                                && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex || this.dataGrid.IsInDetailsViewIndex(rowIndex))
                            {                                
                                if (rowIndex >= lastRowIndex && rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                                {

                                    bool isLastRow = CheckIsLastRow(this.DataGrid);
                                    if (isLastRow && this.dataGrid.HasView && this.dataGrid.View.IsAddingNew)
                                    {
                                        this.ProcessKeyDown(args);
                                        return args.Handled;
                                    }

                                    //Validation is checked before clearing the selecton in DetailsView when navigation is goes out.
                                    if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                                        return false;

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
                                    {
                                        this.ClearSelections(false);
                                    }
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
                case Key.Space:
                    {
                        this.ProcessKeyDown(args);
                        return false;
                    }
                default:
                    return false;
            }
        }

        /// <summary>
        /// Handles the cell selection when the keyboard interactions that are performed in SfDataGrid.
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
            if (!this.DataGrid.IsInDetailsViewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex))
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
        /// Processes the cell selection when the keyboard interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the information about the key that was pressed.
        /// </param>
        /// <remarks>
        /// This method helps to customize the keyboard interaction behavior in SfDataGrid.
        /// </remarks>
        protected virtual void ProcessKeyDown(KeyEventArgs args)
        {
            currentKey = args.Key;
            RowColumnIndex previousRowColumnIndex;
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
                    //WPF-28233 Need to navigate when grid has AddNewRow even record is set zero.
                    if ((detailsViewDataRow.DetailsViewDataGrid.GetRecordsCount() != 0
                        || ((detailsViewDataRow.DetailsViewDataGrid.FilterRowPosition != FilterRowPosition.None || detailsViewDataRow.DetailsViewDataGrid.AddNewRowPosition != AddNewRowPosition.None) 
                        && detailsViewDataRow.DetailsViewDataGrid.HasView))
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
                        if (currentKey != Key.Space && (!ValidationHelper.IsCurrentCellValidated || !ValidationHelper.IsCurrentRowValidated))
                            return;

                        if (currentKey == Key.Tab)
                        {
                            var colIndex = SelectionHelper.CheckShiftKeyPressed() ? this.CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection) : this.CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection);
                            this.CurrentCellManager.SetCurrentColumnIndex(colIndex);
                        }
                        //If already the selection is processed for Parent grid, need to skip the process.
                        else if (currentKey == Key.Space)
                        {
                            if (this.DataGrid.SelectedDetailsViewGrid == detailsViewDataRow.DetailsViewDataGrid)
                                return;
                        }
                    }
                }
            }

            if (DataGrid.FlowDirection == FlowDirection.RightToLeft)
                ChangeFlowDirectionKey(ref currentKey);

            int rowIndex, columnIndex;
            ValidationHelper.SetFocusSetBack(false);
            bool needToScroll = true;
            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;
            previousRowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;
            int firstNavigatingIndex = this.DataGrid.GetFirstNavigatingRowIndex();
            int lastNavigatingIndex = this.DataGrid.GetLastNavigatingRowIndex();

            switch (currentKey)
            {
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
                            rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex >= this.DataGrid.GetLastDataRowIndex()
                               ? this.CurrentCellManager.CurrentRowColumnIndex.RowIndex : this.DataGrid.GetLastDataRowIndex();
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
                        //Below comparerIndex has been used to get the next row index to process after committing the edtited row.
                        int comparerIndex = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetLastDataRowIndex() : this.GetNextRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        if (comparerIndex != rowIndex && rowInfo != null)
                        {
                            //In the below codes, the exact row index to process will be resolved using NodeEntry that have been stored GridRowInfo.
                            if (rowInfo.NodeEntry != null)
                            {
                                int rIndex = this.dataGrid.ResolveToRowIndex(rowInfo.NodeEntry);
                                if (rIndex < 0 && !rowInfo.IsDataRow)//Sets last rowIndex to skip CurrentRowIndex being set as -1 When NodeEntry does not exist in topLevelGroups (while grouping) or not in View.Records 
                                    rIndex = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetLastDataRowIndex() : this.GetNextRowIndex(rowInfo.RowIndex);
                                rowIndex = rIndex;
                            }
                            else if (isAddNewRow || rowInfo.IsAddNewRow)
                                rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                            else if (rowInfo.IsFilterRow)
                                rowIndex = this.DataGrid.GetFilterRowIndex();
                        }


                        if (isAddNewRow && (CurrentCellManager.CurrentRowColumnIndex.RowIndex != lastNavigatingIndex 
                            || (CurrentCellManager.CurrentRowColumnIndex.RowIndex == lastNavigatingIndex && currentKey == Key.Enter)))
                        {
                            if (this.DataGrid.HasView && DataGrid.View.IsAddingNew)
                            {
                                this.CommitAddNew();
                                //Row index will be changed when pressing the enter key in AddNewRow which is set in Bottom.
                                rowIndex = DataGrid.AddNewRowPosition == AddNewRowPosition.Bottom && currentKey == Key.Enter ? this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() : rowIndex;
                            }
                            // We have committed the new values entered in AddNewRow within ProcessOnAddNewRow method and we have removed the current cell also so there is no need to check CurrentCellManager.CurrentCell as null here 
                            if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple && DataGrid.AddNewRowPosition != AddNewRowPosition.Bottom)
                            {
                                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex, new List<object>() { this.DataGrid.GetGridCellInfo(this.CurrentCellManager.CurrentRowColumnIndex) });
                            }
                        }

                        int lastRowIndex = this.DataGrid.GetLastNavigatingRowIndex();

                        //The Below GetLastDataRowIndex() condition is added because the CurrentCell is in the LastRow when pressing the control + down key the currentcell moves to the Unbound row.
                        if (rowIndex > lastRowIndex || (rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && rowIndex == lastRowIndex) 
                            || (SelectionHelper.CheckShiftKeyPressed() && (this.DataGrid.IsAddNewIndex(rowIndex) || this.DataGrid.IsFilterRowIndex(rowIndex))) 
                            || (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == this.dataGrid.GetLastDataRowIndex() && SelectionHelper.CheckControlKeyPressed()))
                        {
                            args.Handled = true;
                            return;
                        }

                        //Below method will process the Key.Down operation in DetailsViewGrid when the next row index is in DetailsViewIndex.
                        //If the DetailsViewGrid itself handles the key then the further operation will be skipped.
                        if (!this.ProcessDetailsViewKeyDown(args, ref rowIndex, Key.Down) && args.Handled)
                            return;

                        int actualRowIndex = rowIndex;
                        //WPF-28233 - Need to get exact rowIndex when the detailsview grid doesnt has records. Hence the below
                        //code is added after DetailsViewKeyDown.
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex && !this.DataGrid.IsInDetailsViewIndex(rowIndex))
                            actualRowIndex = this.CurrentCellManager.GetNextRowIndex(rowIndex);

                        var rowColumnIndex = new RowColumnIndex(this.CurrentCellManager.GetNextRowIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
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
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !isAddNewRow)
                            {
                                this.ProcessShiftSelection(new RowColumnIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), previousRowColumnIndex, Key.Down);
                            }
                            else
                            {
                                this.ProcessSelection(new RowColumnIndex(rowColumnIndex.RowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), SelectionReason.KeyPressed);
                            }
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(rowIndex);

                        if (CurrentCellManager.IsAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

#if !WPF
                        if (this.DataGrid is DetailsViewDataGrid)
                            this.ParentGridScrollInView();

                        else
#endif
                           this.ScrollToRowIndex(this.CurrentCellManager.GetNextRowIndex(rowColumnIndex.RowIndex, rowColumnIndex.ColumnIndex, true));

                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !isAddNewRow)
                            this.lastPressedKey = Key.Down;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.Up:
                    {
                        // WPF-35962 - When moving currentcell selection from parentgrid to parentgrid, we have consider mergecell while calculating
                        // the previousrowindex whether the currentcell is mergedcell. 
                                                
                        CoveredCellInfo inRange = null;
                        if (dataGrid.CanQueryCoveredRange())
                            this.dataGrid.CoveredCells.GetCoveredCellInfo(out inRange, this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        if (SelectionHelper.CheckControlKeyPressed())
                        {
                            rowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetFirstDataRowIndex()
                                       ? this.CurrentCellManager.CurrentRowColumnIndex.RowIndex : this.DataGrid.GetFirstDataRowIndex();                          
                            //WPF-20498 While navigate the datagrid using Ctrl+Up key returns the GetFirstDataRowIndex to ScrollToRowIndex method when using FooterRows and FrozonRows 
                            //so scrolling is not happened properly also out of view records are not come to view,so returns the firstBodyVisibleIndex.                                                                                                         
                            var visibleLines = this.DataGrid.VisualContainer.ScrollRows.GetVisibleLines();
                            if (this.dataGrid.FrozenRowsCount > 0 && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > visibleLines.firstBodyVisibleIndex && rowIndex < visibleLines.firstBodyVisibleIndex)
                                rowIndex = visibleLines.firstBodyVisibleIndex;  
                        }                            
                        else
                            rowIndex = inRange != null ? this.GetPreviousRowIndex(inRange.Top) : this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        //The Record is getting out of view when Editing the Record,while the particular column is in sorting. So that the selection will be set to some other row because the edited row will
                        //rearranged in Grid. Hence using GetNextRowInfo the next row will stored in rowInfo, to get the exact next rowIndex.                       
                        GridRowInfo rowInfo = null;
                        //WPF-25028 Source has been updated When the value isinEditing. while the CheckBoxRenderer isinEditing gets false.
                        //hence the condition "(this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)" added.
                        if (this.DataGrid.HasView && (this.dataGrid.View.IsEditingItem || (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.Renderer is GridCellCheckBoxRenderer)) && this.dataGrid.LiveDataUpdateMode.HasFlag(LiveDataUpdateMode.AllowDataShaping))
                            rowInfo = this.DataGrid.GetPreviousRowInfo(rowIndex);

                        if ((!CurrentCellManager.CheckValidationAndEndEdit(true) || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.HeaderLineCount) && rowInfo == null)
                        {
                            args.Handled = true;
                            return;
                        }
                        //Below comparerIndex has been used to get the next row index to process after committing the edtited row.
                        int comparerIndex = SelectionHelper.CheckControlKeyPressed() ? this.DataGrid.GetFirstDataRowIndex() : this.GetPreviousRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        if (rowIndex != comparerIndex && rowInfo != null)
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

                        if (this.DataGrid.HasView && dataGrid.View.IsAddingNew
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != firstNavigatingIndex)
                        {
                            this.CommitAddNew();
                        }
                                                
                        //When Reached the FirstRow of the ParentGrid then pressed the Up arrow key there is no way to Move the Selection if the AddNewRow is not present.
                        //So here we have set the args.Handeled as true.
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
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex && !this.DataGrid.IsInDetailsViewIndex(rowIndex))
                            actualRowIndex = this.CurrentCellManager.GetPreviousRowIndex(rowIndex);

                        var rowColumnIndex = new RowColumnIndex(this.CurrentCellManager.GetPreviousRowIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

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
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !isAddNewRow)
                            {
                                this.ProcessShiftSelection(new RowColumnIndex(actualRowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), previousRowColumnIndex, Key.Up);
                            }
                            else
                            {
                                this.ProcessSelection(new RowColumnIndex(rowColumnIndex.RowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), SelectionReason.KeyPressed);
                            }
                        }
                        else
                        {
                            this.DataGrid.ShowRowFocusBorder(rowIndex);
                        }

                        if (CurrentCellManager.IsAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

#if !WPF
                        if (this.DataGrid is DetailsViewDataGrid)
                            this.ParentGridScrollInView();
                        else
#endif
                            this.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !isAddNewRow)
                            this.lastPressedKey = Key.Up;
                        else
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                        }
                        args.Handled = true;
                    }
                    break;
                case Key.PageDown:
                    {
                        //The GetNextPageIndex method returns the rowIndex after reordering the edited data which returns invalid rowIndex
                        //when the edited column is in sorting.
                        rowIndex = this.DataGrid.GetNextPageIndex();

                        if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (DataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex) 
                            && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == lastNavigatingIndex)
                            return;

                        var lastRowIndex = this.DataGrid.GetLastNavigatingRowIndex();

                        //The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0)
                            return;

                        if (rowIndex > lastRowIndex || (rowIndex == this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && rowIndex == lastRowIndex))
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
                        var rowColumnIndex = new RowColumnIndex(this.CurrentCellManager.GetNextPageIndex(rowIndex,colIndex), colIndex);

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
                                this.ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.PageDown);
                            }
                            else
                            {
                                this.ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed);
                                this.SetPressedIndex(rowColumnIndex);
                            }
                        }
                        else
                        {
                            this.DataGrid.ShowRowFocusBorder(rowIndex);
                        }

                        this.ScrollToRowIndex(this.CurrentCellManager.GetNextPageIndex(rowIndex, colIndex,true));
                        lastPressedKey = Key.PageDown;
                        args.Handled = true;
                    }
                    break;
                case Key.PageUp:
                    {
                        //The GetPreviousPageIndex method returns the rowIndex after reordering the edited data which returns invalid rowIndex
                        //when the edited column is in sorting.
                        rowIndex = this.DataGrid.GetPreviousPageIndex();

                        if (!CurrentCellManager.CheckValidationAndEndEdit(true))
                        {
                            args.Handled = true;
                            return;
                        }

                        if (DataGrid.IsAddNewIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex) && dataGrid.AddNewRowPosition == AddNewRowPosition.Top)                                                    
                            return;
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
                        var scrollRowIndex = this.CurrentCellManager.GetPreviousPageIndex(rowIndex, colIndex);
                        var rowColumnIndex = new RowColumnIndex(scrollRowIndex, colIndex);

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
                                this.ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.PageUp);
                            }
                            else
                            {
                                this.ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed);
                                this.SetPressedIndex(rowColumnIndex);
                            }
                        }
                        else
                        {
                            this.DataGrid.ShowRowFocusBorder(rowIndex);
                        }

                        this.ScrollToRowIndex(rowIndex);
                        lastPressedKey = Key.PageUp;
                        args.Handled = true;
                    }
                    break;
                case Key.Right:
                    {
                        if (!CurrentCellManager.RaiseValidationAndEndEdit())
                        {
                            args.Handled = true;
                            return;
                        }

                        rowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;

                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetLastColumnIndex() : this.CurrentCellManager.GetLastCellIndex();
                        else
                            columnIndex = this.GetNextCellIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        NodeEntry nodeEntry = null;
                        if (this.DataGrid.GridModel.HasGroup)
                        {
                            nodeEntry = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowIndex)];
                        }

                        int actualColumnIndex = columnIndex;
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex)
                        {
                            actualColumnIndex++;
                            //The AllowFocus method returns false for invalid column index, hence the actualColumnIndex condition is added.
                            while (actualColumnIndex <= this.DataGrid.GetLastColumnIndex() && !this.DataGrid.AllowFocus(new RowColumnIndex(rowIndex, actualColumnIndex)))
                            {
                                actualColumnIndex++;
                            }
                        }

                        if (nodeEntry != null && (nodeEntry is Group || nodeEntry is SummaryRecordEntry))
                        {
                            if (nodeEntry is Group)
                                this.ExpandOrCollapseGroup(nodeEntry as Group, true);
                            columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        }
                        else
                        {
                            var rowColumnIndex = new RowColumnIndex(rowIndex, actualColumnIndex);
                            if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }

                            var data = this.DataGrid.GetRecordAtRowIndex(rowIndex);
                            if (data != null || this.CurrentCellManager.IsInNonDataRows())
                            {
                                if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                {
                                    //The below condition is used to skip the Shift Selection for the AddNewRow. 
                                    if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !this.CurrentCellManager.IsAddNewRow)
                                    {
                                        ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.Right);
                                    }
                                    else
                                    {
                                        ProcessSelection(new RowColumnIndex(rowIndex, columnIndex), SelectionReason.KeyPressed);
                                    }
                                }
                            }
                        }
                        this.CurrentCellManager.ScrollInViewFromRight(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        //The below condition is used to update the pressedIndex.
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !this.CurrentCellManager.IsAddNewRow)
                            lastPressedKey = Key.Right;
                        else if (!isInShiftSelection)
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, columnIndex));
                        }
                        args.Handled = true;
                    }
                    break;

                case Key.Left:
                    {
                        if (!CurrentCellManager.RaiseValidationAndEndEdit())
                        {
                            args.Handled = true;
                            return;
                        }

                        rowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                        
                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetFirstColumnIndex() : this.CurrentCellManager.GetFirstCellIndex();
                        else
                            columnIndex = this.GetPreviousCellIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        NodeEntry nodeEntry = null;
                        if (this.DataGrid.GridModel.HasGroup)
                        {
                            nodeEntry = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowIndex)];
                        }

                        int actualColumnIndex = columnIndex;
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex)
                        {
                            actualColumnIndex--;
                            //The AllowFocus method returns false for invalid column index, hence the actualColumnIndex condition is added.
                            while (actualColumnIndex >= this.DataGrid.GetFirstColumnIndex() && !this.DataGrid.AllowFocus(new RowColumnIndex(rowIndex, actualColumnIndex)))
                            {
                                actualColumnIndex--;
                            }
                        }

                        if (nodeEntry != null && (nodeEntry is Group || nodeEntry is SummaryRecordEntry))
                        {
                            if (nodeEntry is Group)
                                this.ExpandOrCollapseGroup(nodeEntry as Group, false);
                            columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                        }
                        else
                        {
                            var rowColumnIndex = new RowColumnIndex(rowIndex, actualColumnIndex);
                            if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                            {
                                args.Handled = true;
                                return;
                            }

                            var data = this.DataGrid.GetRecordAtRowIndex(rowIndex);
                            if (data != null || this.CurrentCellManager.IsInNonDataRows())
                            {
                                if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                                {
                                    //The below condition is used to skip the Shift Selection for the AddNewRow.
                                    if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !this.dataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                                    {
                                        ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.Left);
                                    }
                                    else
                                    {
                                        ProcessSelection(new RowColumnIndex(rowIndex, columnIndex), SelectionReason.KeyPressed);
                                    }
                                }
                            }
                        }
                        this.CurrentCellManager.ScrollInViewFromLeft(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);

                        //The below condition is used to update the PressedIndex.
                        if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && !this.dataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                            lastPressedKey = Key.Left;
                        else if (!isInShiftSelection)
                        {
                            this.lastPressedKey = Key.None;
                            this.SetPressedIndex(new RowColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex, columnIndex));
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

                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex())
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

                        var validationResult = rowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex ? CurrentCellManager.RaiseValidationAndEndEdit() : CurrentCellManager.CheckValidationAndEndEdit();
                        if (!validationResult)
                            return;
                        if (SelectionHelper.CheckControlKeyPressed())
                            columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex(dataGrid.FlowDirection) : CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);
                        else
                            columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex() : CurrentCellManager.GetFirstCellIndex();

                        if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex == columnIndex && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex)
                        {
                            this.CurrentCellManager.ScrollInViewFromLeft(0);
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
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.Home);
                            }
                            else
                            {
                                needToScroll = ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed); this.ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed);
                                this.SetPressedIndex(rowColumnIndex);
                            }
                        }
                        else
                        {
                            this.DataGrid.ShowRowFocusBorder(rowIndex);
                        }

                        if (CurrentCellManager.IsAddNewRow)                        
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

                        // Added the Condition to Scrolling should be happen or not. when the Cancel Selection is set as true.
                        //Scrolling happened for the Control + Home key.
                        if (rowIndex != previousRowColumnIndex.RowIndex && needToScroll)
                            this.ScrollToRowIndex(rowIndex);
                        this.CurrentCellManager.ScrollInViewFromLeft(this.DataGrid.showRowHeader ? 1 : 0);
                        lastPressedKey = Key.Home;
                        args.Handled = true;
                    }
                    break;
                case Key.End:
                    {
                        //While AddNewRow is in Editing mode, pressing the Control+End key it will Commit the Edited row through CommitAddNew method.
                        if (this.DataGrid.HasView && DataGrid.View.IsAddingNew && SelectionHelper.CheckControlKeyPressed()
                            && CurrentCellManager.CurrentRowColumnIndex.RowIndex != lastNavigatingIndex)
                        {
                            this.CommitAddNew();
                        }
                        int lastDataRowIndex = this.DataGrid.GetLastDataRowIndex();
                        //Added the condition to check whether the CurrentCell is in AddNewRow and AddNewRow as Bottom 
                        //while pressing the Control+End key the selection should be in AddNewRow.
                        rowIndex = SelectionHelper.CheckControlKeyPressed() && lastDataRowIndex > 0
                            && !(CurrentCellManager.CurrentRowColumnIndex.RowIndex >= lastDataRowIndex)
                            ? lastDataRowIndex : CurrentCellManager.CurrentRowColumnIndex.RowIndex;

                        var validationResult = rowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex ? CurrentCellManager.RaiseValidationAndEndEdit() : CurrentCellManager.CheckValidationAndEndEdit();
                        if (!validationResult)
                            return;

                        //The BelowCondition will check cancel the Below process if the RowIndex is less than 0.
                        //When the AddNewRow is in view while pressing Control+End it clears the selection because of RowIndex is -1.
                        if (rowIndex < 0 || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex())
                            return;

                        // If the LastRow of the DataGrid is DetailViewIndex means the Scroll Bar distance only apply for the DetailsViewGrid only.so it will Move to the ParentGrid.
                        if (SelectionHelper.CheckControlKeyPressed() && this.dataGrid.DetailsViewManager.HasDetailsView)
                            rowIndex = this.DataGrid.GetLastParentRowIndex(rowIndex);
                       
                        columnIndex = this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetLastColumnIndex() : CurrentCellManager.GetLastCellIndex();

                        if (DataGrid.FlowDirection == FlowDirection.RightToLeft && SelectionHelper.CheckControlKeyPressed())
                            columnIndex =  this.DataGrid.IsAddNewIndex(rowIndex) ? this.DataGrid.GetFirstColumnIndex() : CurrentCellManager.GetFirstCellIndex();

                        var rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);                       

                        if (!CurrentCellManager.HandleKeyNavigation(args, rowColumnIndex))
                        {
                            args.Handled = true;
                            return;
                        }

                        this.DataGrid.HideRowFocusBorder();
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                            {
                                this.ProcessShiftSelection(rowColumnIndex, previousRowColumnIndex, Key.End);
                            }
                            else
                            {
                                needToScroll = ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed);
                                this.SetPressedIndex(rowColumnIndex);
                            }
                        }
                        else
                        {
                            this.DataGrid.ShowRowFocusBorder(rowIndex);
                        }

                        if (CurrentCellManager.IsAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        else if (isAddNewRow)
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);                        

                        // Added the Condition to Scrolling should be happen or not. when the Cancel Selection is set as true.
                        //Scrolling happened for the Control + End key.
                        if (needToScroll)
                        {
                            this.ScrollToRowIndex(rowIndex);
                            this.CurrentCellManager.ScrollInViewFromRight(columnIndex);
                        }
                        lastPressedKey = Key.End;
                        args.Handled = true;
                    }
                    break;
                case Key.Tab:
                    {
                        if (CurrentCellManager.CurrentRowColumnIndex.RowIndex <= this.DataGrid.GetHeaderIndex() && !(this.DataGrid is DetailsViewDataGrid))
                            return;
                        lastPressedKey = Key.Tab;                        
                        var previousCellEditStatus = this.CurrentCellManager.HasCurrentCell ? this.CurrentCellManager.CurrentCell.IsEditing : false;
                        if (!CurrentCellManager.HandleKeyNavigation(args, CurrentCellManager.CurrentRowColumnIndex) && args.Handled)
                            return;
                        var rowColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex;
                        if (!args.Handled)
                        {
                            rowIndex = CurrentCellManager.CurrentRowColumnIndex.RowIndex;
                            if (SelectionHelper.CheckShiftKeyPressed())
                            {                                
                                if (CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0)
                                    rowIndex = this.DataGrid.GetLastNavigatingRowIndex();
                                else
                                    rowIndex = this.GetPreviousRowIndex(rowIndex);
                                //The Grid is scrolled to LastColumn when the new rowIndex is in Summary or in Group row.
                                if (dataGrid.IsAddNewIndex(rowIndex))
                                    columnIndex = dataGrid.GetLastColumnIndex(dataGrid.FlowDirection);
                                else
                                    columnIndex = !this.DataGrid.IsInSummarryRow(rowIndex) ? CurrentCellManager.GetLastCellIndex(dataGrid.FlowDirection) : CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);

                                rowIndex = this.CurrentCellManager.GetPreviousRowIndex(rowIndex, columnIndex); 
                                if (!this.DataGrid.ValidationHelper.RaiseRowValidate(CurrentCellManager.CurrentRowColumnIndex))
                                {
                                    args.Handled = true;
                                    return;
                                }

                                if (this.DataGrid.HasView && DataGrid.View.IsEditingItem)                                
                                    DataGrid.View.CommitEdit();

                                if (this.DataGrid.View.IsAddingNew)
                                    this.CommitAddNew(firstNavigatingIndex != DataGrid.GridModel.addNewRowController.GetAddNewRowIndex());

                                if (rowIndex <= this.DataGrid.GetFirstNavigatingRowIndex() && CurrentCellManager.CurrentRowColumnIndex.RowIndex == rowIndex && !(this.DataGrid is DetailsViewDataGrid))
                                    return;

                                //When Records count is zero and the AddNewRow position is bottom and while pressing Shift+Tab the,
                                //the AddNewRowMode is changed hence the below code is used after the above condition.
                                if (isAddNewRow)
                                {
                                    //WPF-18329 AddNewRow watermark disappear when pressing tab key in AddNewRow.
                                    if (dataGrid.AddNewRowPosition == AddNewRowPosition.Bottom)
                                    {
                                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                                    }
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
                                
                                columnIndex =dataGrid.IsAddNewIndex(rowIndex) ? dataGrid.GetFirstColumnIndex(dataGrid.FlowDirection) : this.CurrentCellManager.GetFirstCellIndex(dataGrid.FlowDirection);

                                rowIndex = this.CurrentCellManager.GetNextRowIndex(rowIndex, columnIndex); 
                                if (!this.DataGrid.ValidationHelper.RaiseRowValidate(CurrentCellManager.CurrentRowColumnIndex))
                                {
                                    args.Handled = true;
                                    return;
                                }

                                if (this.DataGrid.HasView && DataGrid.View.IsEditingItem)
                                    DataGrid.View.CommitEdit();

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
                                    else if (dataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() == DataGrid.GetLastNavigatingRowIndex())
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

                            if (previousRowColumnIndex.RowIndex != this.CurrentCellManager.CurrentRowColumnIndex.RowIndex && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                                return;

                            rowColumnIndex = new RowColumnIndex(rowIndex, columnIndex);
                            if (!CurrentCellManager.ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Keyboard))
                                return;
                        }
                        if (isAddNewRow)
                        {
                            this.HideCellSelection(DataGrid.GetDataColumnBase(this.CurrentCellManager.previousRowColumnIndex));
                            if (!this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                                DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                        }

                        if (DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                        {
                            DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
                        }

                        this.DataGrid.HideRowFocusBorder();
                        bool suspendScrolling = false;
                        if (this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                        {
                            suspendScrolling = !ProcessSelection(rowColumnIndex, SelectionReason.KeyPressed);
                            this.SetPressedIndex(rowColumnIndex);
                        }
                        else
                            this.DataGrid.ShowRowFocusBorder(rowColumnIndex.RowIndex);

                        if (previousCellEditStatus && this.CurrentCellManager.IsAddNewRow)
                            this.CurrentCellManager.BeginEdit();

                        if (!suspendScrolling)
                        {
                            if (SelectionHelper.CheckShiftKeyPressed())
                                this.CurrentCellManager.ScrollInViewFromLeft(rowColumnIndex.ColumnIndex);
                            else
                            {
                                rowColumnIndex.ColumnIndex = rowColumnIndex.ColumnIndex == this.CurrentCellManager.GetFirstCellIndex() ? (this.DataGrid.showRowHeader ? 1 : 0) : rowColumnIndex.ColumnIndex;
                                CurrentCellManager.ScrollInViewFromRight(rowColumnIndex.ColumnIndex);
                            }
                            if (!(this.DataGrid is DetailsViewDataGrid))
                                this.ScrollToRowIndex(rowColumnIndex.RowIndex);
                        }
                        args.Handled = true;
                        break;
                    }
                case Key.Space:
                    {
                        if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > this.DataGrid.GetHeaderIndex())
                        {
                            var addedItems = new List<object>();
                            var removedItems = new List<object>();
                            var currentCell = this.CurrentCellManager.CurrentCell;
                            var currentRow = this.DataGrid.RowGenerator.Items.FirstOrDefault(item => item.IsCurrentRow);
                            if (currentRow != null)
                            {
                                //When selection in DetailsViewGrid and the current cell in another DetailsViewGrid, the selected DetailsViewGrid
                                //selection is not removed in parent grid. Hence the below flag is introduced.
                                bool needToClear = currentRow is DetailsViewDataRow || this.DataGrid.SelectedDetailsViewGrid != null;
                                if (this.DataGrid.SelectedDetailsViewGrid != null)
                                {
                                    //To clear the DetailsView selection when pressing space key in Parent grid or in another DetailsView.
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

                                GridCellInfo cellInfo = null;
                                bool needToRemove = false;
                                //When the selection is in DetailsViewGrid the Selection not be removed, hence the needToClear property is used.
                                if (needToClear)
                                {
                                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                                    cellInfo = this.DataGrid.GetGridCellInfo(this.CurrentCellManager.CurrentRowColumnIndex);
                                }
                                else
                                {
                                    cellInfo = this.DataGrid.GetGridCellInfo(this.CurrentCellManager.CurrentRowColumnIndex);
                                    needToRemove = currentCell.IsSelectedColumn || currentRow.IsSelectedRow;
                                }

                                if (needToRemove)
                                {
                                    removedItems.Add(cellInfo);

                                    if (RaiseSelectionChanging(addedItems, removedItems))
                                        return;

                                    RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex, removedItems);
                                    this.DataGrid.ShowRowFocusBorder(currentRow.RowIndex);
                                }
                                else
                                {
                                    addedItems.Add(cellInfo);

                                    if (RaiseSelectionChanging(addedItems, removedItems))
                                        return;
                                    //When the selection is in DetailsViewGrid the Selection not be removed, hence the needToRemove property is used.
                                    if (needToClear)
                                        RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex, removedItems);

                                    AddSelection(this.CurrentCellManager.CurrentRowColumnIndex, addedItems);
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
        /// Processes the cell selection when the keyboard interactions that are performed in DetailsViewDataGrid.
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
        protected bool ProcessDetailsViewKeyDown(KeyEventArgs args, ref int rowIndex, Key processKey)
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

        /// <summary>
        /// Processes cell selection while navigating the cell selection from parent grid to child grid .
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
                    var result = detailsViewDataRow.DetailsViewDataGrid.GetRecordsCount() > 0 
                        || (detailsViewDataRow.DetailsViewDataGrid.HasView && (detailsViewDataRow.DetailsViewDataGrid.FilterRowPosition != FilterRowPosition.None 
                        || detailsViewDataRow.DetailsViewDataGrid.AddNewRowPosition != AddNewRowPosition.None)) ? detailsViewDataRow.DetailsViewDataGrid.SelectionController.HandleKeyDown(args) : false;
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

#endregion

        #region Grid Operations Methods

        /// <summary>
        /// Processes the cell selection while navigating from one page in to another page in SfDataPager.
        /// </summary>
        protected override void ProcessOnPageChanged()
        {
            if (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
            {
                this.HideCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
            }
            ClearSelectedDetailsViewGrid();
            this.ClearSelections(false);
        }

        /// <summary>
        /// Processes the cell selection when filtering is applied in SfDataGrid.
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
            bool isInEdit = CurrentCellManager.IsFilterRow && CurrentCellManager.HasCurrentCell
                                   && CurrentCellManager.CurrentCell.IsEditing;

            if (this.DataGrid is DetailsViewDataGrid && this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex())
                return;

            ClearSelectedDetailsViewGrid();

            if (this.DataGrid.HasView && dataGrid.View.IsAddingNew)
                this.CommitAddNew(false);

            var removedItems = new List<object>();
            ResetSelectedCells(ref removedItems);
            if (isInEdit)
                this.CurrentCellManager.SetCurrentRowIndex(this.DataGrid.GetFilterRowIndex());
            else
                UpdateCurrentCell(canFocusGrid: !(args.PopupIsOpen || isInEdit) && !args.IsProgrammatic);

            //WPF 18781- Checked with the Records count value if it is 0.then there is no Record so we can return the value.
            if (!this.CurrentCellManager.IsAddNewRow && !this.CurrentCellManager.IsFilterRow && this.DataGrid.GetRecordsCount() == 0)
                return;
            //WPF 18238 - isValidRow added to retain old CurrentItem in case if it is in View, Otherwise select first item as Current/SelectedItem if SelectedRows empty. Otherwise selects items from SelectedRows
            //WPF 18781 - removedItems condtion added to select first item as selected item when the caption/group summary selected.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.dataGrid.GetFirstNavigatingRowIndex() && (isValidRow || removedItems.Count > 0))
            {
                // The Below condition is Added to check the SelectedItem is Maintaining in the Grid or Not.If it is Maintaining the selection will Move to the SelectedItem while the CurrentItem is Filtered.
                var rowIndex = -1;
                int columnIndex = -1;
                if (this.SelectedCells.Count != 0)
                {
                    var CellInfo = this.SelectedCells.First();
                    if(CellInfo.IsUnBoundRow)
                        rowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(CellInfo.GridUnboundRowInfo);
                    else
                        rowIndex = this.DataGrid.ResolveToRowIndex(CellInfo.RowData);
                    if (CellInfo.ColumnCollection.Count > 0)
                        columnIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(CellInfo.ColumnCollection.LastOrDefault()));
                }
                else
                    rowIndex = this.DataGrid.GetFirstDataRowIndex();

                //UWP-175(Issue 1)
                if (rowIndex == -1)
                    return;
                if(columnIndex < 0)
                    columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                // This Condition is Added while the CurrentCell is in AddNewRow we have to Maintain that Cell Not Add to the Selection.
                //WPF 18781 - removedItems condition is also added to select the group/caption summary Row when the first row is selected.
                if (isValidRow || removedItems.Count > 0)
                    // WPF-35275 - Focus maintained in grid based on filtering applied by programmatically or UI interaction.
                    ResetSelection(new RowColumnIndex(rowIndex, columnIndex), removedItems, setFocusGrid: !args.PopupIsOpen && !args.IsProgrammatic);
                else
                {
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                }
            }
            this.CurrentCellManager.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            this.ResetFlags();
        }

        /// <summary>
        /// Processes the cell selection when the filter popup is opened.
        /// </summary>
        /// <remarks>
        /// This method refreshes the cell selection while opening the filter popup in SfDataGrid.
        /// </remarks>
        protected override void ProcessOnFilterPopupOpened()
        {
            int rowIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex;

            if (!this.CurrentCellManager.CheckValidationAndEndEdit(true))
            {
                ValidationHelper.SetFocusSetBack(false);
                return;
            }
            if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew && this.DataGrid.IsAddNewIndex(rowIndex))
            {
                this.CommitAddNew(false);
                UpdateCurrentCell();
                RefreshSelectedCells();
            }
        }

        /// <summary>
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowRowHeader"/> property value changes.
        /// </summary>
        public override void ProcessOnRowHeaderChanged()
        {
            base.ProcessOnRowHeaderChanged();
            if (this.DataGrid.showRowHeader)
                this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex, this.PressedRowColumnIndex.ColumnIndex + 1));
            else
                this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex, this.PressedRowColumnIndex.ColumnIndex - 1));
        }

        /// <summary>
        /// Processes the cell selection when the column is sorted in SfDataGrid.
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
            ResetSelectedCells(ref removedItems);
            UpdateCurrentCell(canFocusGrid: !sortcolumnHandle.IsProgrammatic);

            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstDataRowIndex() && (this.CurrentCellManager.IsAddNewRow || this.SelectedCells.Count > 0 || this.dataGrid.View is VirtualizingCollectionView))
            {
                int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : DataGrid.GetFirstDataRowIndex();
                rowIndex = this.CurrentCellManager.IsAddNewRow ? this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex() : rowIndex;
                ResetSelection(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), removedItems);
            }

            if (this.CurrentCellManager.IsAddNewRow)
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);

            if (sortcolumnHandle.ScrollToCurrentItem)
                this.CurrentCellManager.ScrollToRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);

            this.ResetFlags();
        }

        /// <summary>
        /// Processes the cell selection when the column is grouped in SfDataGrid.
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

            if (this.CurrentCellManager.IsAddNewRow)
                this.HideCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));

            var removedItems = new List<object>();
            ResetSelectedCells(ref removedItems);
            //WPF-24301- If we group by the column when selection is in CaptionSummary row,
            //we need to scroll the HorizontalBar to ColumnIndex of CaptionSummary.So introduced 
            //new flag for store columnindex to scroll.
            int columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

            if ((this.DataGrid.GetRecordsCount() != 0 || this.CurrentCellManager.IsInNonDataRows()) && args != null)
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
                            CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentRowColumnIndex.ColumnIndex + args.CollectionChangedEventArgs.NewItems.Count);
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentRowColumnIndex.ColumnIndex - args.CollectionChangedEventArgs.OldItems.Count);
                            break;
                        //WPF-20955 - If we clear GroupColumnDescriptions means the NotifyCollectionChangedAction is Reset and
                        //we need to set the CurrentCell based on CurrentColumn index.
                        case NotifyCollectionChangedAction.Reset:
                            if (CurrentCellManager.HasCurrentCell)
                                CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.CurrentCell.ColumnIndex);
                            break;
                    }
                    columnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                }
                //Added the Condition Because the CurrentCell is maintained while Expanding all the Groups at run-Time.
                else if (CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.DataGrid.GetFirstColumnIndex())
                {
                    //WPF-24301- if the Selection is in CaptionSummary, CurrentColumnIndex is -1 so we 
                    // need to move the scroll bar to 0th index or 1st index based on ShowRowHeader
                    columnIndex = DataGrid.showRowHeader ? 1 : 0;
                    CurrentCellManager.SetCurrentColumnIndex(CurrentCellManager.GetFirstCellIndex());
                }
                UpdateCurrentCell(canFocusGrid: args != null && !args.IsProgrammatic);

                if (this.DataGrid.CurrentItem == null && !this.CurrentCellManager.IsInNonDataRows() && !(this.DataGrid is DetailsViewDataGrid))
                {
                    ResetSelection(new RowColumnIndex(this.DataGrid.GetFirstDataRowIndex(), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), removedItems, args != null && !args.IsProgrammatic);
                }
            }

            //When records count is zero and after grouping the AddNewRow mode is in Normal hence the below code is used after the
            //condition of recordCount > 0
            if (this.CurrentCellManager.IsAddNewRow)
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);

            //WPF-24301- Scroll the HorizontalScroll bar based on RowColumn index
            this.CurrentCellManager.ScrollInView(new RowColumnIndex(CurrentCellManager.CurrentRowColumnIndex.RowIndex, columnIndex));
            this.ResetFlags();
        }
        
        /// <summary>
        /// Processes the cell selection when the records is pasted from the clipboard into SfDataGrid.
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
                for (int i = 0;  i < records.Count; i++)
                {
                    var record = records[i] as GridCellInfo;
                   if(record.NodeEntry != null)
                       addedItems.Add(records[i]); 
                }
                this.AddCells(addedItems);
            }
            else
            {
                if (this.DataGrid.HasView && this.DataGrid.View.FilterRecord(records.FirstOrDefault()))
                {
                    //To move the selection to the first line of the pasted records.
                    var rowColumnIndex = new RowColumnIndex(this.DataGrid.ResolveToRowIndex(records.FirstOrDefault()), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
                    this.MoveCurrentCell(rowColumnIndex);
                }
            }
        }

        #endregion

        #region Selection PropertyChanged Method

        /// <summary>
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> property value changes.
        /// </param>
        protected override void ProcessSelectionModeChanged(SelectionPropertyChangedHandlerArgs handle)
        {

            if (handle.NewValue == null)
                return;

            this.SuspendUpdates();
            if (this.CurrentCellManager.HasCurrentCell && this.CurrentCellManager.CurrentCell.IsEditing)
                CurrentCellManager.EndEdit();

            switch ((GridSelectionMode)handle.NewValue)
            {
                case GridSelectionMode.None:
                    if (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                    {
                        if (this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                            this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);
                        this.HideCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                    }
                    this.ClearSelections(false);
                    this.DataGrid.RowGenerator.ResetSelection();
                    break;
                case GridSelectionMode.Single:
                    //After deleting all rows and changing selection mode throws exception 
                    //hence added the condition to check the current row is selected or not.
                    //The FirstNavigatingRowIndex returns negative value and also the CurrentRowIndex which satisfies the condition.
                    //Hence the below zero is added for the condition.
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
                        this.ClearSelections(true);
                    }
                    else
                        this.ClearSelections(false);
                    break;
                case GridSelectionMode.Extended:
                    {                        
                        var currentCellInfo = this.DataGrid.CurrentCellInfo;
                        if (currentCellInfo != null && !this.SelectedCells.Contains(currentCellInfo))
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
                                            if (parentDataGrid.SelectedDetailsViewGrid != null)
                                                this.ClearDetailsViewGridSelections(parentDataGrid.SelectedDetailsViewGrid);
                                            parentDataGrid.SelectedDetailsViewGrid = childGrid;
                                            parentDataGrid.ClearSelections(true);
                                        }
                                        childGrid = parentDataGrid;
                                    }
                                }
                            }

                            this.SelectedCells.Add(currentCellInfo);
                            if (currentCellInfo.IsDataRowCell || currentCellInfo.IsUnBoundRow)
                                this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                            else
                                this.DataGrid.ShowRowSelection(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                        }
                        break;
                    }
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex"/> property value changes.
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

            //Clear the EditItem when its having, by calling EndEdit method
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            if (this.DataGrid.GetRecordsCount(false) == 0)
                throw new InvalidOperationException("SelectedIndex " + dataGrid.SelectedIndex.ToString() + " doesn't fall with in expected range");

            int rowIndex = this.dataGrid.ResolveToRowIndex(newValue);

            //If the selection is any NestedGrid and clears the selection in the below method.
            if (!ClearSelectedDetailsViewGrid() || this.DataGrid.IsInDetailsViewIndex(rowIndex))
                return;
            if(newValue == -1)
            {
                this.ClearSelections(false);
                return;
            }

            var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowColumnIndex = new RowColumnIndex(rowIndex, currentColumnIndex);
            var cellInfo = this.DataGrid.GetGridCellInfo(rowColumnIndex);
            if (cellInfo != null && !this.SelectedCells.Contains(cellInfo))
            {
                //This code is added to clear the parent grid selections when directly changing the SelectedIndex in DetailsViewGrid.
                if (this.DataGrid is DetailsViewDataGrid)
                {
                    UpdateSelectedDetailsViewGrid();
                }
                this.SuspendUpdates();

                var addedItems = new List<object>();
                var removedItems = new List<object>();

                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(rowColumnIndex, false);

                //Selection should be maintain when the SelectionMode is Multiple, hence added the condition to check the SelectionMode.
                if (this.SelectedCells.Count > 0 && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                {
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                    this.RemoveSelection(rowColumnIndex, removedItems);
                }
                addedItems.Add(cellInfo);
                this.AddCells(addedItems);
                this.DataGrid.HideRowFocusBorder();
                this.DataGrid.SelectedItem = cellInfo.IsDataRowCell ? cellInfo.RowData : null;
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
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> that contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/> property value changes.
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
                if (this.SelectedCells.Count > 0)
                {
                    this.SuspendUpdates();
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                    CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1,CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                    this.RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex, removedItems);
                    this.RefreshSelectedIndex();
                    this.RaiseSelectionChanged(new List<object>(), removedItems);
                    this.ResumeUpdates();
                    //WPF-25047 Reset the PressedIndex
                    SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
                }
                return;
            }

            var currentColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowColumnIndex = new RowColumnIndex(rowIndex, currentColumnIndex);
            var cellInfo = this.DataGrid.GetGridCellInfo(rowColumnIndex);
            if (!this.SelectedCells.Contains(cellInfo))
            {
                if (!ClearSelectedDetailsViewGrid())
                    return;

                //This code is added to clear the parent grid selections when directly changing the SelectedItem in DetailsViewGrid.
                if (this.DataGrid is DetailsViewDataGrid)
                {
                    UpdateSelectedDetailsViewGrid();
                }

                this.DataGrid.HideRowFocusBorder();

                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(rowColumnIndex, false);

                this.SuspendUpdates();
                //Selection should be maintain when the SelectionMode is Multiple, hence added the condition to check the SelectionMode.
                if (this.SelectedCells.Count > 0 && this.DataGrid.SelectionMode != GridSelectionMode.Multiple)
                {
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                    this.RemoveSelection(rowColumnIndex, removedItems);
                }

                addedItems.Add(cellInfo);
                this.AddCells(addedItems);
                this.DataGrid.HideRowFocusBorder();
                this.DataGrid.SelectedIndex = this.DataGrid.ResolveToRecordIndex(rowIndex);                
                this.ResumeUpdates();
                this.RaiseSelectionChanged(addedItems, removedItems);
                //WPF-25047 Reset the PressedIndex
                SetPressedIndex(CurrentCellManager.CurrentRowColumnIndex);
            }
        }

        /// <summary>
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> that contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </param>
        protected internal override void ProcessCurrentItemChanged(SelectionPropertyChangedHandlerArgs handle)
        {
            if (isSuspended || this.DataGrid.SelectionMode == GridSelectionMode.None || handle.NewValue == null)
                return;

            var rowIndex = this.DataGrid.ResolveToRowIndex(handle.NewValue);
            if (rowIndex < this.DataGrid.GetHeaderIndex())
                return;
       
            //While changing the CurrentItem the EndEdit is not called so called the EndEdit after checking the EditItem 
            if (this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            //We have to clear the nested grid selection when selection mode is single and have to remove the current cell 
            //when selection mode is multiple or extended.
            if (this.DataGrid.SelectedDetailsViewGrid != null)
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

            //Added this code to clear the CurrentCell selection in parentgrid when changing current item directly in NestedGrid.
            if (this.DataGrid is DetailsViewDataGrid)
            {
                this.ProcessDetailsViewCurrentItemChanged();
            }

            var addedItems = new List<object>();
            var removedItems = new List<object>();

            var oldCurrentCellColumnIndex = CurrentCellManager.CurrentRowColumnIndex.ColumnIndex >= CurrentCellManager.GetFirstCellIndex() ? CurrentCellManager.CurrentRowColumnIndex.ColumnIndex : CurrentCellManager.GetFirstCellIndex();
            this.CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, oldCurrentCellColumnIndex), false);
            if (this.DataGrid.SelectionMode == GridSelectionMode.Single)
            {
                var nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                var column = this.DataGrid.CurrentColumn != null ? this.DataGrid.CurrentColumn : this.DataGrid.GetGridColumn(this.CurrentCellManager.GetFirstCellIndex());
                addedItems.Add(new GridCellInfo(column, handle.NewValue, nodeEntry));
                if (this.SelectedCells.Count > 0)
                {
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                    var item = this.SelectedCells.LastOrDefault();
                }
                if (removedItems.Count > 0)
                    RemoveSelection(CurrentCellManager.CurrentRowColumnIndex, removedItems);

                AddSelection(new RowColumnIndex(rowIndex, oldCurrentCellColumnIndex), addedItems);
                this.RaiseSelectionChanged(addedItems, removedItems);
            }            
        }

        #endregion

        #region Collection Changed Operation Methods

        /// <summary>
        /// Processes the cell selection when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSource"/> property value changes.
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
            var removedItems = new List<object>();
            this.SuspendUpdates();

            //When replacing the CurrentItem with editing the cell and the exception will be thrown when editing another item. Because
            //the previous edited item is not yet commit.
            if (this.CurrentCellManager.HasCurrentCell && this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            if (reason == CollectionChangedReason.SourceCollectionChanged && this.DataGrid.HasView)
            {
                if (DataGrid.View.IsAddingNew)
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

                            if (this.SelectedCells.Any(item => item.IsDirty) && newItem != null)
                            {
                                var index = this.SelectedCells.IndexOf(this.SelectedCells.FirstOrDefault(item => item.IsDirty && (item.IsDataRow || item.NodeEntry is NestedRecordEntry)));
                                var columns = this.SelectedCells[index].ColumnCollection;
                                var rowIndex = this.DataGrid.ResolveToRowIndex(newItem);
                                this.SelectedCells[index] = new GridSelectedCellsInfo()
                                {
                                    RowData = newItem,
                                    NodeEntry = this.DataGrid.GetNodeEntry(rowIndex),
                                    RowIndex = -1
                                };
                                columns.ForEach(column => this.SelectedCells[index].ColumnCollection.Add(column));
                                this.RefreshSelectedCells();
                                if (this.DataGrid.CurrentItem == null)
                                {
                                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                }
                                else
                                    UpdateCurrentCell(canFocusGrid: false);
                            }
                            else
                            {
                                this.RefreshSelectedCells();
                                UpdateCurrentCell(false, false);
                            }
                            this.RefreshSelectedIndex();
                            this.ResetFlags();
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    {
                        if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            if (isSourceCollectionChanged)//if (e.OldItems.Contains(this.dataGrid.SelectedItem))
                            {
                                this.RefreshSelectedCells();
                                UpdateCurrentCell(false, false);
                                this.RefreshSelectedIndex();
                                isSourceCollectionChanged = false;
                            }
                            else
                            {
                                foreach (var item in e.OldItems)
                                {
                                    GridSelectedCellsInfo cellsInfo;
                                    // item is NodeEntry, need to call corresponding Find method
                                    if (item is NodeEntry)
                                        cellsInfo = this.SelectedCells.Find(item as NodeEntry);
                                    else
                                        cellsInfo = this.SelectedCells.Find(item);
                                    if (cellsInfo != null)
                                        cellsInfo.IsDirty = true;
                                }
                            }
                        }
                        else
                        {
                            isSourceCollectionChanged = true;
                            object rowData = e.OldItems[0] is RecordEntry ? (e.OldItems[0] as RecordEntry).Data : e.OldItems[0];
                            var cellsInfo = this.SelectedCells.Find(e.OldItems[0]);
                            if (cellsInfo != null)
                            {
                                if (!ClearSelectedDetailsViewGrid())
                                    return;
                                removedItems = this.SelectedCells.ConvertToGridCellInfoList(cellsInfo).Cast<object>().ToList<object>();
                                CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.RemoveSelection(this.CurrentCellManager.CurrentRowColumnIndex, removedItems);
                                this.RefreshSelectedIndex();
                                this.RaiseSelectionChanged(new List<object>(), removedItems);
                            }
                        }
                        this.ResetFlags();
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        if (reason == CollectionChangedReason.SourceCollectionChanged)
                        {
                            GridSelectedCellsInfo cellsInfo = null;
                            //Sets the CurrentItem of DataGrid when replace the Parent of Selected NestedGrid. 
                            if (this.dataGrid.SelectedDetailsViewGrid != null)
                            {
                                if (!this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid))
                                    return;
                                this.DataGrid.SelectedDetailsViewGrid = null;
                                if (this.SelectedCells.Count > 0)
                                {
                                    cellsInfo = this.SelectedCells[0];
                                    if (cellsInfo != null)
                                    {
                                        var index = this.SelectedCells.IndexOf(cellsInfo);
                                        if (this.DataGrid.CurrentItem == ((cellsInfo.NodeEntry as NestedRecordEntry).Parent as RecordEntry).Data)
                                        {
                                            this.SelectedCells[index].IsDirty = true;
                                            DataGrid.CurrentItem = e.NewItems[0];
                                            DataGrid.CurrentCellInfo = new GridCellInfo(DataGrid.CurrentColumn, e.NewItems[0], null);
                                            this.RefreshSelectedIndex();
                                        }
                                    }
                                }
                            }
                            else
                            {
                                cellsInfo = this.SelectedCells.Find(e.OldItems[0]);
                                if (cellsInfo != null)
                                {
                                    var index = this.SelectedCells.IndexOf(cellsInfo);
                                    this.SelectedCells[index].IsDirty = true;
                                    if (this.DataGrid.CurrentItem == cellsInfo.RowData)
                                        DataGrid.CurrentItem = e.NewItems[0];
                                    this.RefreshSelectedIndex();
                                }
                            }
                        }
                        //When replacing the Data, the Replace (RecordCollectionChanged) is fired instead of Add hence the below code is added.
                        else if (reason == CollectionChangedReason.RecordCollectionChanged)
                        {
                            if (this.SelectedCells.Any(item => item.IsDirty))
                            {
                                var newItem = e.NewItems.ToList<object>().FirstOrDefault(item => item is RecordEntry || !(item is NodeEntry));
                                newItem = (newItem is RecordEntry) ? (newItem as RecordEntry).Data : newItem;
                                var index = this.SelectedCells.IndexOf(this.SelectedCells.FirstOrDefault(item => item.IsDirty && (item.IsDataRow || item.NodeEntry is NestedRecordEntry)));
                                var columns = this.SelectedCells[index].ColumnCollection;
                                var rowIndex = this.DataGrid.ResolveToRowIndex(newItem);

                                if (index < 0 || rowIndex < 0)
                                    return;

                                this.SelectedCells[index] = new GridSelectedCellsInfo()
                                {
                                    RowData = newItem,
                                    NodeEntry = this.DataGrid.GetNodeEntry(rowIndex),
                                    RowIndex = -1
                                };
                                columns.ForEach(column => this.SelectedCells[index].ColumnCollection.Add(column));
                                this.RefreshSelectedCells();
                                if (this.DataGrid.CurrentItem == null)
                                {
                                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                                }
                                else
                                    UpdateCurrentCell(canFocusGrid: false);

                                this.RefreshSelectedIndex();
                                this.ResetFlags();
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
                            if (isSourceCollectionChanged)
                            {
                                //WPF - 33944 DetailsViewGrid selection is not cleared when the source has been cleared.
                                if (this.DataGrid.SelectedDetailsViewGrid != null || this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                                    ClearChildGridSelections(this.CurrentCellManager.CurrentRowColumnIndex);
                                this.ResetSelectedCells(ref removedItems);
                                this.ResetFlags();
                                isSourceCollectionChanged = false;
                            }
                            else
                            {
                                // Refresh the selection while do batch update programmatically.
                                var args = e as NotifyCollectionChangedEventArgsExt;
                                if (args != null && args.IsProgrammatic)
                                {
                                    this.ResetSelectedCells(true);
                                    this.UpdateCurrentCell(canFocusGrid:false);
                                }
                            }
                        }
                    }
                    break;
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the cell selection when the data is reordered in SfDataGrid.
        /// </summary>
        /// <param name="value">
        /// The corresponding value to check whether the reordered data that contains cell selection.
        /// </param>
        /// <param name="action">
        /// Indicates the corresponding collection changed actions performed on the data.
        /// </param>
        protected virtual void ProcessDataReorder(object value, NotifyCollectionChangedAction action)
        {
            this.SuspendUpdates();

            if (action == NotifyCollectionChangedAction.Remove)
            {
                var selectedRow = this.SelectedCells.Find(value);
                if (selectedRow!=null)
                {
                    var removedIndex = DataGrid.ResolveToRowIndex(value);
                    List<object> removedItems = new List<object> {this.SelectedCells.ConvertToGridCellInfoList(selectedRow)};
                    this.SelectedCells.Remove(selectedRow);
                    this.UpdateCurrentCell();
                    this.RefreshSelectedIndex();
                    RaiseSelectionChanged(new List<object>(), removedItems);
                }
            }
            this.ResumeUpdates();
        }

        /// <summary>
        /// Processes the selection when the column is removed from the selected cells in SfDataGrid.
        /// </summary>
        /// <param name="column">
        /// The corresponding column that is removed from the selected cells.
        /// </param>
        protected virtual void ProcessOnColumnRemoved(GridColumn column)
        {
            int index = this.SelectedCells.Count - 1;
            while (index >= 0)
            {
                if (this.SelectedCells[index].HasColumn(column))
                {
                    this.SelectedCells[index].ColumnCollection.Remove(column);
                    if (this.SelectedCells[index].ColumnCollection.Count == 0)
                        this.SelectedCells.RemoveAt(index);
                }
                index--;
            }
        }

        #endregion
        
        #region Group Expand/Collapse

        /// <summary>
        /// Processes the cell selection when the group is expanded in SfDataGrid.
        /// </summary>
        /// <param name="insertIndex">
        /// The corresponding index of the group that is expanded in to view.
        /// </param>
        /// <param name="count">
        /// The number of expanded rows count of corresponding group.
        /// </param>      
        protected virtual void ProcessGroupExpanded(int insertIndex, int count)
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple || (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.SelectedCells.Count > 0))
            {
                this.RefreshSelectedCells();
                if(previousSelectedCells.Count > 0)
                {
                    this.previousSelectedCells.ForEach(item =>
                    {
                        var cellInfo = item as GridCellInfo;
                        if (!cellInfo.IsDataRowCell && cellInfo.RowIndex > (insertIndex - 1))
                            cellInfo.RowIndex = this.DataGrid.ResolveToRowIndex(cellInfo.NodeEntry);
                    });
                }

                if (this.PressedRowColumnIndex.RowIndex > (insertIndex - 1))
                    this.SetPressedIndex(new RowColumnIndex(PressedRowColumnIndex.RowIndex + count, this.PressedRowColumnIndex.ColumnIndex));
            }
            //When expanding the Group using button click, the cell selection is not updated which maintains in same cell.
            else
            {
                this.RefreshSelectedCells();
                //This condition is added to update current row index based on row count in expanding group,
                //when the current row index is greater than expanded index.
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > insertIndex)
                    this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex + count);
            }
        }

        /// <summary>
        /// Processes the cell selection when the group is collapsed.
        /// </summary>
        /// <param name="removeAtIndex">
        /// The corresponding index of the group that is collapsed from the view.
        /// </param>
        /// <param name="count">
        /// The number of collapsed rows count of corresponding group.
        /// </param>   
        protected virtual void ProcessGroupCollapsed(int removeAtIndex, int count)
        {
            //var updateCurrentIndex = this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1) &&
            //        this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < (removeAtIndex - 1 + count);
//#if !WP
//            //Clears the ChildGrid selection when the parentgrid has collapsed the group which contains the Child
////Need to change this condition.
//            if (updateCurrentIndex && !this.ClearChildGridSelections(this.CurrentCellManager.CurrentRowColumnIndex))
//                return;
//#endif

            this.RefreshSelectedCells();
            //UpdateCurrentCell method will update the current cell based on current item. When the current row is in Collapsing
            //Group which is need to be remove, hence this method is used to clear the current cell. 
            //WPF-23126 - Need to UpdateCurrentRowIndex for SelectedRow with in the Collapsed group,
            //if we have select the last record in group means row column index is not updated 
            //so change the condition by remove -1 from (removeAtIndex -1) because if we -1 from removeAtIndex
            //its skips the last record in the group.
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1) &&
                this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < (removeAtIndex + count))
                this.UpdateCurrentCell();
            //This condition is added to update current row index based on row count in collapsed group
            //when the current row index is greater than collapsed group index.
            else if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex > (removeAtIndex - 1 + count))
                this.CurrentCellManager.SetCurrentRowIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex - count);
            //WPF-23126 Pressed row index is upadted only for CheckShiftKeyPressed with Extended selection not for Muliple Selection.
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended && this.SelectedCells.Count > 0)
            {
                var removedItem = this.GetSelectedCells();

                previousSelectedCells = previousSelectedCells.Except(removedItem).ToList();
                if (previousSelectedCells.Count > 0)
                {
                    //List<GridCellInfo> selectedCells = new List<GridCellInfo>();
                    this.previousSelectedCells.ForEach(item =>
                    {
                        var cellInfo = (GridCellInfo)item;
                        if (!cellInfo.IsDataRowCell && cellInfo.RowIndex > (removeAtIndex - 1 + count))
                            cellInfo.RowIndex = this.DataGrid.ResolveToRowIndex(cellInfo.NodeEntry);
                    });
                }

                if (this.PressedRowColumnIndex.RowIndex > (removeAtIndex - 1 + count))
                    this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex - count, this.PressedRowColumnIndex.ColumnIndex));
                //WPF-23126 - Need to pressed RowIndex for 
                //so change the condition by remove -1 from (removeAtIndex -1) because if we -1 from removeAtIndex
                //its skips the last record in the group.
                else if (this.PressedRowColumnIndex.RowIndex < (removeAtIndex + count) && this.PressedRowColumnIndex.RowIndex > (removeAtIndex - 1))
                    this.SetPressedIndex(new RowColumnIndex(removeAtIndex - 1, this.PressedRowColumnIndex.ColumnIndex));
            }
        }

        #endregion

        #region TableSummary

        /// <summary>
        /// Processes the cell selection when the table summary row position has changed.
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

            //Checks whether the current row index is in AddNewRow.
            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;

            if (this.CurrentCellManager.HasCurrentCell && this.dataGrid.HasView && this.dataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedCells.Count > 0)
                RefreshSelectedCells();

            if (isAddNewRow)
            {
                this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex(), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), false);
                if (this.SelectedCells.Count > 0)
                    this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);
            }
            else if (args.NewPosition == TableSummaryRowPosition.Top)
            {
                bool needToAdd = (args.Action == NotifyCollectionChangedAction.Add || (args.Action == NotifyCollectionChangedAction.Add
                    && args.NewPosition == TableSummaryRowPosition.Top));
               
                this.CurrentCellManager.ProcessOnDataRowCollectionChanged(needToAdd, args.Count);
            }
            this.ResetFlags();
        }

        #endregion

        #region Add New Row
        
        /// <summary>
        /// Processes the cell selection when new row is initiated or committed and the position of AddNewRow is changed.
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
                            this.HideCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                        }
                    }
                    break;
                case AddNewRowOperation.PlacementChange:
                    {
                        //When changing the position without selecting any row the current row index is updated, hence added this condition.
                        if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                            return;

                        var args = (DependencyPropertyChangedEventArgs)handle.OperationArgs;

                        if (this.SelectedCells.Count > 0)
                            this.RefreshSelectedCells();

                        this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);
                        var newValue = (AddNewRowPosition)args.NewValue;
                        if ((this.CurrentCellManager.IsAddNewRow && newValue != AddNewRowPosition.None) || this.CurrentCellManager.IsFilterRow)
                        {
                            int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : this.DataGrid.GetAddNewRowIndex();
                            //When AddNewRow is in editing  and changing the position from bottom to top adds the selection in newly added row instead of in AddNewRow and current cell is also not maintained anywhere because the old current cell is not removed.
                            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                            this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                            if (this.SelectedCells.Count > 0)
                                this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
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
                this.RefreshSelectedCells();

            var newValue = (FilterRowPosition)args.NewValue;
            if ((CurrentCellManager.IsFilterRow && newValue != FilterRowPosition.None) || this.CurrentCellManager.IsAddNewRow)
            {
                int rowIndex = this.CurrentCellManager.IsFilterRow ? this.DataGrid.GetFilterRowIndex() : this.DataGrid.GetAddNewRowIndex();
                
                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                if (this.SelectedCells.Count > 0)
                    this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
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
        
		/// <summary>
        /// Processes the cell selection when the stacked header collection changes in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data for stacked header collection changes.
        /// </param>
        protected override void ProcessOnStackedHeaderRows(StackedHeaderCollectionChangedEventArgs args)
        {
            if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetHeaderIndex())
                return;

            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;
            if (this.CurrentCellManager.HasCurrentCell && DataGrid.HasView && DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedCells.Count > 0)
                RefreshSelectedCells();

            if (isAddNewRow)
            {
                this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex(), this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), false);
                if (this.SelectedCells.Count > 0)
                    this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
                this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(true);                                
            }
            else
                this.CurrentCellManager.ProcessOnDataRowCollectionChanged(args.Action == NotifyCollectionChangedAction.Add, args.Count);
            this.ResetFlags();
        }
        #endregion

        #region UnBoundDataRow

        /// <summary>
        /// Processes the cell selection when the UnBoundRow collection changes.
        /// </summary>
        /// <param name="args">
        /// Contains data for the UnBoundRow collection changes.
        /// </param>
        protected override void ProcessUnBoundRowChanged(UnBoundDataRowCollectionChangedEventArgs args)
        {
            if ((this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0 && this.SelectedCells.Count == 0) || args.Position == UnBoundRowsPosition.Bottom)
                return;

            bool isAddNewRow = this.CurrentCellManager.IsAddNewRow;

            if (this.CurrentCellManager.HasCurrentCell && this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit(false);

            if (isAddNewRow && this.DataGrid.HasView && this.DataGrid.View.IsAddingNew)
                this.DataGrid.GridModel.AddNewRowController.CommitAddNew(false);

            if (this.SelectedCells.Count > 0)
            {
                if (args.Action == NotifyCollectionChangedAction.Remove && this.SelectedCells.Any(item => item.IsUnBoundRow && item.RowIndex == args.RowIndex))
                {
                    var rowInfo = this.SelectedCells.FirstOrDefault(item => item.IsUnBoundRow && item.RowIndex == args.RowIndex);
                    this.RemoveCells(this.SelectedCells.ConvertToGridCellInfoList(rowInfo).ToList<object>());
                }
                RefreshSelectedCells();
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
                    if (this.SelectedCells.Count != 0)
                    {
                        var CellInfo = this.SelectedCells.LastOrDefault();
                        rowIndex = CellInfo.IsDataRow ? this.DataGrid.ResolveToRowIndex(CellInfo.RowData) : CellInfo.RowIndex;
                    }
                    this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(rowIndex, this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex));
                }
            }
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
        }

        #endregion

        #region Clear Selections

        /// <summary>
        /// Clears all the selected cells in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentCell">
        /// Decides whether the current cell selection need to be removed when the selections are cleared.
        /// </param>
        /// <remarks>
        /// This method helps to clear the selection programmatically.
        /// </remarks>
        public override void ClearSelections(bool exceptCurrentCell)
        {
            //While chaning the changing SelectionUnit as row from cell, the clear selection is called and the VisualContainer is null which is
            //checked in GetFirstNavigatingRowIndex
            if (this.DataGrid.VisualContainer == null || !this.DataGrid.HasView)
                return;

            this.SuspendUpdates();
            //When changing the SelectionMode to Single from Multiple with editing the any row, the EditItem is still maintianed in View,
            //Which throws exception when editing other cells. Hence the EndEdit is called.
            if (this.CurrentCellManager.HasCurrentCell && this.DataGrid.HasView && this.DataGrid.View.IsEditingItem)
                this.CurrentCellManager.EndEdit();

            var removedItems = new List<object>();
            removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
            HideAllSelection(exceptCurrentCell);
            if (!exceptCurrentCell)
            {
                if (!ClearSelectedDetailsViewGrid())
                    return;

                CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);

                //The Below Condition is check while the selection is in AddNewRow, When we clear the Selection Watermark is Disappeared.
                if (this.CurrentCellManager.IsAddNewRow)
                    this.DataGrid.GridModel.AddNewRowController.SetAddNewMode(false);

                this.CurrentCellManager.ResetCurrentRowColumnIndex();
                this.SelectedCells.Clear();
                this.RefreshSelectedIndex();
            }
            else
            {
                GridCellInfo cellInfo = this.DataGrid.CurrentCellInfo;
                this.SelectedCells.Clear();

                if (cellInfo != null)
                {
                    this.SelectedCells.Add(cellInfo);
                    removedItems.Remove(cellInfo);
                }
                //When changing from Multiple to single in DetailsViewGrid, the Selection is not updated.
                this.ShowCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));
            }
            this.RefreshSelectedIndex();
            if (removedItems.Count > 0)
                this.RaiseSelectionChanged(new List<object>(), removedItems);

            this.ResetFlags();
            //When changing the SelectionMode to single from Multiple the RowHeaderState has been changed, hence the below condition is added.
            if (!exceptCurrentCell)
                this.ClearRowHeader();
            this.ResumeUpdates();
        }

        #endregion

        #endregion

        #region Helper Methods

        #region General Helpers

        /// <summary>
        /// Updates the current cell based on <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="skipIfNotDataRow"> 
        /// Indicates whether the update for current cell is skipped other than data row.
        /// </param>
        /// <param name="canFocusGrid">
        /// If true, the Focus will be set to SfDataGrid.
        /// </param>
        protected void UpdateCurrentCell(bool skipIfNotDataRow = true, bool canFocusGrid = true)
        {
            this.SuspendUpdates();

            //Added the to reset the current cell when view is null for Customer Issue WPF-17028
            if (this.DataGrid.View == null)
            {
                if (this.CurrentCellManager.CurrentCell != null)
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                this.CurrentCellManager.ResetCurrentRowColumnIndex();
                return;
            }

            if (this.DataGrid.HasView && this.DataGrid.View.CurrentEditItem != null)
                CurrentCellManager.EndEdit();
            
            bool isAvailable = false;
            if (this.DataGrid.CurrentItem != null && this.DataGrid.HasView)
            {
                // Need to get the record or record index from DisplayElements when the Source is IQueryable. Since we will keep null record entries View.Records. Entries will be kept in Group.Records alone.
                var record = dataGrid.isIQueryable && dataGrid.GridModel.HasGroup? dataGrid.View.TopLevelGroup.DisplayElements.GetItem(this.DataGrid.CurrentItem) : this.DataGrid.View.Records.GetRecord(this.DataGrid.CurrentItem);
                if (record != null)
                {
                    if ((this.DataGrid.View as CollectionViewAdv).IsGrouping)
                    {
                        isAvailable = this.DataGrid.CheckGroupExpanded(record.Parent as Group);
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
                else if (this.DataGrid.CurrentCellInfo.NodeEntry is NestedRecordEntry)
                    rowIndex = this.DataGrid.ResolveToRowIndex(this.DataGrid.CurrentCellInfo.NodeEntry);
                else
                    rowIndex = this.dataGrid.ResolveToRowIndex(this.DataGrid.CurrentItem);
            }
            else if (this.DataGrid.CurrentCellInfo != null && !this.DataGrid.CurrentCellInfo.IsDataRowCell && !skipIfNotDataRow)
            {
                rowIndex = this.DataGrid.ResolveToRowIndex(this.DataGrid.CurrentCellInfo.NodeEntry);
            }
            
            if (rowIndex != this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
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
            else if (canFocusGrid)
            {
#if WinRT || UNIVERSAL
                this.dataGrid.Focus(FocusState.Programmatic);
#else
                this.dataGrid.Focus();
#endif
            }
            this.ResumeUpdates();
        }
        
        /// <summary>
        /// Method which reset the flags in the selection controller.
        /// </summary>
        private void ResetFlags()
        {
            this.SetPressedIndex(this.CurrentCellManager.CurrentRowColumnIndex);
            this.previousSelectedCells.Clear();
            this.lastPressedKey = Key.None;
            this.isInShiftSelection = false;
        }

        /// <summary>
        /// Commits the new item initialized on the AddNewRow to the underlying data source.
        /// </summary>
        /// <param name="changeState">
        /// Indicates whether watermark is enabled on the AddNewRow.
        /// </param>
        protected internal override void CommitAddNew(bool changeState = true)
        {
            this.SuspendUpdates();
            this.DataGrid.GridModel.AddNewRowController.CommitAddNew(changeState);
            this.RefreshSelectedCells();
            this.ResumeUpdates();
        }

        private void RefreshSelectedIndex()
        {
            if (this.SelectedCells.Count > 0)
            {
                var rowInfo = this.SelectedCells.FirstOrDefault();
                this.DataGrid.SelectedIndex = rowInfo.IsDataRow ? this.DataGrid.ResolveToRecordIndex(this.DataGrid.ResolveToRowIndex(rowInfo.RowData)) : this.DataGrid.ResolveToRecordIndex(rowInfo.RowIndex);
                this.DataGrid.SelectedItem = rowInfo.IsDataRow ? rowInfo.RowData : null;
            }
            else
            {
                this.DataGrid.SelectedIndex = -1;
                this.DataGrid.SelectedItem = null;
            }
        }

        /// <summary>
        /// Method which helps to add the new items to Selected cells.
        /// </summary>
        /// <param name="Items">Corresponding Items to Add</param>
        /// <remarks></remarks>
        private void AddCells(List<object> Items)
        {
            Items.ForEach(item =>
            {
                var cellInfo = item as GridCellInfo;
                if(!this.SelectedCells.Contains(cellInfo))
                {
                    this.SelectedCells.Add(cellInfo);
                    if (cellInfo.IsDataRowCell || cellInfo.IsUnBoundRow || cellInfo.IsAddNewRow || cellInfo.IsFilterRow)
                    {
                        var rowColumnIndex = cellInfo.IsAddNewRow || cellInfo.IsFilterRow || cellInfo.IsUnBoundRow ? 
                            new RowColumnIndex(cellInfo.RowIndex, this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(cellInfo.Column))) 
                            : GetRowColumnIndex(cellInfo.RowData, cellInfo.Column);
                        ShowCellSelection(DataGrid.GetDataColumnBase(rowColumnIndex));
                    }
                    else
                        this.DataGrid.ShowRowSelection(cellInfo.RowIndex);
                }
            });
        }

        /// <summary>
        /// Method which helps to remove the new items to Selected cells.
        /// </summary>
        /// <param name="Items">Corresponding Items to Remove</param>
        /// <remarks></remarks>
        private void RemoveCells(List<object> Items)
        {
            Items.ForEach(item =>
            {
                var cellInfo = item as GridCellInfo;
                if (this.SelectedCells.Remove(cellInfo))
                {
                    if (cellInfo.IsDataRowCell || cellInfo.IsUnBoundRow || cellInfo.IsAddNewRow || cellInfo.IsFilterRow)
                    {
                        var rowColumnIndex = cellInfo.IsAddNewRow || cellInfo.IsFilterRow || cellInfo.IsUnBoundRow ? new RowColumnIndex(cellInfo.RowIndex, this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(cellInfo.Column))) : GetRowColumnIndex(cellInfo.RowData, cellInfo.Column);
                        this.HideCellSelection(this.DataGrid.GetDataColumnBase(rowColumnIndex));
                    }
                    else
                        this.DataGrid.HideRowSelection(cellInfo.RowIndex);
                }
            });
        }

        /// <summary>
        /// Updates the SelectedCells based on row data and removes the cells from the SelectedCells collection that is not in currently in view.
        /// </summary>
        /// <param name="canUpdateCurrentCell">
        /// <b>true</b>, if the current cell is need to be updated during reset the SelectedCells.
        /// </param>
        /// <remarks>
        /// The SelectedCells collection updated only for record cells and other selection will be removed.
        /// </remarks>
        public void ResetSelectedCells(bool canUpdateCurrentCell = true)
        {
            
            if (canUpdateCurrentCell)
                this.UpdateCurrentCell(true, false);
            var removedItems = new List<object>();
            this.ResetSelectedCells(ref removedItems);
        }

        /// <summary>
        /// Refreshes the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedCells"/> collection in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method refresh the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedCells"/> collection when the grid related operations are performed.
        /// </remarks>
        protected void RefreshSelectedCells()
        {
            this.SuspendUpdates();
            int index = this.SelectedCells.Count - 1;
            while (index >= 0)
            {
                var nodeEntry = this.SelectedCells[index].NodeEntry;
                int rowIndex = -1;
                if (nodeEntry != null && !this.SelectedCells[index].IsDirty)
                    rowIndex = this.DataGrid.ResolveToRowIndex(SelectedCells[index].NodeEntry);
                else if (this.SelectedCells[index].IsAddNewRow && this.DataGrid.AddNewRowPosition != AddNewRowPosition.None)
                    rowIndex = this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();
                else if (this.SelectedCells[index].IsFilterRow && this.DataGrid.FilterRowPosition != FilterRowPosition.None)
                    rowIndex = this.DataGrid.GetFilterRowIndex();
                else if (SelectedCells[index].IsUnBoundRow)
                    //To update the rowIndex when the SelectedRow is GridUnBoundRow during Grid operations like Filtering, Sorting etc.
                    rowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(SelectedCells[index].GridUnboundRowInfo);

                if(rowIndex < 0)
                {
                    if (this.SelectedCells[index].IsDataRow || this.SelectedCells[index].IsAddNewRow || this.SelectedCells[index].IsFilterRow)
                    {
                        DataRowBase rowBase = null;
                        if (this.SelectedCells[index].IsFilterRow)
                            this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.FilterRow);
                        else
                            rowBase = this.SelectedCells[index].IsAddNewRow
                                ? this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.AddNewRow)
                                : this.DataGrid.RowGenerator.Items.FirstOrDefault(
                                    row => row.RowData == this.SelectedCells[index].RowData);
                        if (rowBase != null)
                            rowBase.VisibleColumns.ForEach(col => HideCellSelection(col));
                    }
                    else
                        this.DataGrid.HideRowSelection(this.SelectedCells[index].RowIndex);
                    this.SelectedCells.RemoveAt(index);

                }
                else if (!this.SelectedCells[index].IsDataRow)
                {
                    this.SelectedCells[index].RowIndex = rowIndex;
                }
                index--;
            }
            this.RefreshSelectedIndex();
            this.ResumeUpdates();
        }

        /// <summary>
        /// Resets the <see cref="Syncfusion.UI.Xaml.Grid.GridBaseSelectionController.SelectedCells"/> based on the selection added or removed in SfDataGrid .
        /// </summary>
        /// <param name="canUpdateCurrentRow">
        /// Indicates whether the current cell is updated based on CurrentItem.
        /// </param>
        /// <remarks>
        /// The SelectedCells collection updated only for record cells and other selection will be removed.
        /// </remarks>
        protected void ResetSelectedCells(ref List<object> removedItems)
        {
            this.SuspendUpdates();
            int index = 0;
            var addedCells = new GridSelectedCellsCollection();
            //Here Changed the Condition Because the First Item stored in the LastPosition
            while (index < this.SelectedCells.Count)
            {
                bool isAvailable = false;
                if (this.SelectedCells[index].IsDataRow)
                {
                    int rowIndex = this.DataGrid.ResolveToRowIndex(this.SelectedCells[index].RowData);
                    if (rowIndex >= 0)
                    {
                        var recordentry = this.DataGrid.GridModel.HasGroup ? this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(rowIndex)] : this.DataGrid.View.Records[this.DataGrid.ResolveToRecordIndex(rowIndex)];
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
                    }
                }
                //In this method the DataRow only been udated, hence added the below code update the RowIndex of UnBoundRow and AddNewRow
                else if (this.SelectedCells[index].IsUnBoundRow || this.SelectedCells[index].IsAddNewRow || this.SelectedCells[index].IsFilterRow)
                {
                    if (this.SelectedCells[index].IsUnBoundRow)
                        this.SelectedCells[index].RowIndex = this.DataGrid.ResolveUnboundRowToRowIndex(this.SelectedCells[index].GridUnboundRowInfo);
                    else
                        this.SelectedCells[index].RowIndex = this.SelectedCells[index].IsFilterRow
                            ? this.DataGrid.GetFilterRowIndex()
                        : this.DataGrid.GridModel.AddNewRowController.GetAddNewRowIndex();

                    isAvailable = true;
                }
                if (!isAvailable)
                {
                    if (this.SelectedCells[index].IsDataRow || this.SelectedCells[index].IsAddNewRow
                        || this.SelectedCells[index].IsFilterRow || this.SelectedCells[index].IsUnBoundRow)
                    {
                        DataRowBase rowBase = null;
                        if (this.SelectedCells[index].IsAddNewRow)
                            rowBase = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.AddNewRow);
                        else if (this.SelectedCells[index].IsFilterRow)
                            rowBase = this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowType == RowType.FilterRow);
                        else
                            rowBase = this.SelectedCells[index].IsDataRow ? this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowData == this.SelectedCells[index].RowData)
                                : this.DataGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.SelectedCells[index].RowIndex);
                        if (rowBase != null)
                            rowBase.VisibleColumns.ForEach(col => HideCellSelection(col));
                    }
                    else
                        this.DataGrid.HideRowSelection(this.SelectedCells[index].RowIndex);
                }
                else
                    addedCells.Add(this.SelectedCells[index]);
                index++;
            }
            var removedCells = this.SelectedCells.ConvertToGridCellInfoList();
            this.SelectedCells.Clear();
            var cellsInfo = addedCells.ConvertToGridCellInfoList();
            //WPF-28013 While grouping the nodeentry is not updated, hence the SelectedCells is cleared and refreshed using below code.
            cellsInfo.ForEach(cellInfo =>
            {
                if (cellInfo.IsUnBoundRow)
                    this.SelectedCells.Add(new GridCellInfo(cellInfo.Column, cellInfo.RowIndex, cellInfo.GridUnboundRowInfo));
                else
                    this.SelectedCells.Add(new GridCellInfo(cellInfo.Column, cellInfo.RowData,
                        this.DataGrid.GetNodeEntry(this.DataGrid.ResolveToRowIndex(cellInfo.RowData)), cellInfo.RowIndex, cellInfo.IsAddNewRow));
            });
            removedItems = removedCells.Where(item => !item.IsDataRowCell || !this.SelectedCells.Contains(item)).Cast<object>().ToList<object>();
            this.RefreshSelectedIndex();
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
        private void ExpandOrCollapseGroup(Group group, bool isExpanded)
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
        private bool CheckCanRemoveSameCell()
        {
            if (this.DataGrid.SelectionMode == GridSelectionMode.Extended)
            {
#if WinRT || UNIVERSAL
                if ((Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift).HasFlag(CoreVirtualKeyStates.Down)) && (Window.Current.CoreWindow.GetAsyncKeyState(Key.Home).HasFlag(CoreVirtualKeyStates.Down) || Window.Current.CoreWindow.GetAsyncKeyState(Key.End).HasFlag(CoreVirtualKeyStates.Down)))
#else
                if ((Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift)) && (Keyboard.IsKeyDown(Key.Home) || Keyboard.IsKeyDown(Key.End)))
#endif
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the collection of selected cells in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the list of <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> collection.
        /// </returns>
        public List<GridCellInfo> GetSelectedCells()
        {
            return this.SelectedCells.ConvertToGridCellInfoList();
        }

        #endregion

        #region Selection Helpers

        /// <summary>
        /// Processes the cell selection for the specified row and column index.
        /// </summary>
        /// <param name="newRowColumnIndex">
        /// The corresponding rowcolumnindex to select the cell.     
        /// </param>        
        /// <param name="reason">
        /// Contains the selection reason to select the cell.
        /// </param>
        /// <param name="needWholeRowSelect">
        /// Indicates whether the whole row is selected while processing the cell selection.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the cell selection is processed to corresponding row and column index; otherwise, <b>false</b>.
        /// </returns>
        protected bool ProcessSelection(RowColumnIndex newRowColumnIndex, SelectionReason reason, bool needWholeRowSelect= false)
        {
            isInShiftSelection = false;
            var data = this.DataGrid.GetRecordAtRowIndex(newRowColumnIndex.RowIndex);
            GridCellInfo cellInfo = null;
            
            var addedItems = new List<object>();
            var removedItems = new List<object>();

            if (!needWholeRowSelect)
            {
                cellInfo = this.DataGrid.GetGridCellInfo(newRowColumnIndex);
                addedItems.Add(cellInfo);
            }
            else
            {
                GridUnBoundRow undBoundRow = null;
                NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(newRowColumnIndex.RowIndex);
                if (nodeEntry == null && DataGrid.IsUnBoundRow(newRowColumnIndex.RowIndex))
                    undBoundRow = DataGrid.GetUnBoundRow(newRowColumnIndex.RowIndex);

                if (data != null || undBoundRow != null)
                {
                    this.DataGrid.Columns.ForEach(column =>
                    {
                        if (column.AllowFocus && !column.IsHidden)
                        {
                            if (data != null)
                                cellInfo = new GridCellInfo(column, data, nodeEntry);
                            else
                                cellInfo = new GridCellInfo(column, newRowColumnIndex.RowIndex, undBoundRow);
                            addedItems.Add(cellInfo);
                        }
                    });
                }
                else
                {
                    cellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(newRowColumnIndex.ColumnIndex), null,
                        nodeEntry, newRowColumnIndex.RowIndex);
                    addedItems.Add(cellInfo);
                }
            }

            bool needToRemoveNonDataRows = (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex) 
                || this.DataGrid.IsFilterRowIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex)) 
                && ((this.DataGrid.SelectionMode == GridSelectionMode.Extended && (SelectionHelper.CheckControlKeyPressed() 
                || SelectionHelper.CheckShiftKeyPressed())) || this.DataGrid.SelectionMode == GridSelectionMode.Multiple);
            //Added the condition for removing the selection from addnewrow when SelectionMode is multiple. 
            //It is done by checking the previous current rowIndex whether it is in AddNewRow.
            //Added the condition in Multiple Selection mode to maintain the selection in parent grid when the current cell only moved to DetailsViewGrid
            bool needToRemove = this.DataGrid.SelectionMode == GridSelectionMode.Single || needToRemoveNonDataRows || (this.DataGrid.SelectionMode == GridSelectionMode.Extended && (this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex) || (!SelectionHelper.CheckControlKeyPressed() && (reason == SelectionReason.PointerReleased || reason == SelectionReason.PointerPressed)) || (reason == SelectionReason.KeyPressed && !SelectionHelper.CheckShiftKeyPressed()) || (SelectionHelper.CheckShiftKeyPressed() && (this.lastPressedKey == Key.Tab || this.lastPressedKey == Key.A)))) 
                || ((reason == SelectionReason.KeyPressed || this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex) || (this.DataGrid.IsAddNewIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex) && !this.CurrentCellManager.IsAddNewRow) || (this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex) && newRowColumnIndex.RowIndex != this.CurrentCellManager.previousRowColumnIndex.RowIndex && this.SelectedCells.Any(item=>item.NodeEntry is NestedRecordEntry))) && this.DataGrid.SelectionMode == GridSelectionMode.Multiple);
            bool needToRemoveSameCell = ((this.DataGrid.SelectionMode == GridSelectionMode.Extended && ((SelectionHelper.CheckControlKeyPressed() && reason != SelectionReason.KeyPressed) || (reason == SelectionReason.KeyPressed&& this.lastPressedKey!=Key.Tab && SelectionHelper.CheckShiftKeyPressed()))) || this.DataGrid.SelectionMode == GridSelectionMode.Multiple) && this.SelectedCells.Contains(cellInfo) && !this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex);

            if (needToRemoveSameCell)
                needToRemoveSameCell = CheckCanRemoveSameCell();

            //To Remove the selection on AddNewRow when the SelectionMode is Multiple.
            //Checked whether the rowIndex is in DetailViewGrid to clear the selection in multiple.
            if (needToRemoveNonDataRows && !this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex))
            {
                var selectedCells = this.SelectedCells.FirstOrDefault(item => item.IsAddNewRow || item.IsFilterRow);
                //In Multiple Selection, after unselecting the cell in AddNewRow and selecting the another cell in same row which clears the
                //other cells selection because the removedItems is filled with all cells when selectedCells is null.
                if (selectedCells != null)
                {
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList(selectedCells).Cast<object>().ToList<object>();
                    needToRemove = true;
                }
            }
            else
                removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();

            if (needToRemoveSameCell)
            {
                if (!needToRemoveNonDataRows)
                    removedItems.Clear();

                removedItems.AddRange(addedItems);

                if(this.RaiseSelectionChanging(new List<object>(), removedItems))
                {
                    CurrentCellManager.RemoveCurrentCell(newRowColumnIndex);
                    CurrentCellManager.SelectCurrentCell(CurrentCellManager.previousRowColumnIndex);
                    //When Cancel the Selection through the Selection Changing Event the pressed index will update when press any DataRow through MouseClick.
                    this.SetPressedIndex(CurrentCellManager.previousRowColumnIndex);
                    return false;
                }

                this.DataGrid.HideRowFocusBorder();
                RemoveSelection(newRowColumnIndex, removedItems);

                if (this.DataGrid.SelectionMode == GridSelectionMode.Multiple)
                    this.DataGrid.ShowRowFocusBorder(newRowColumnIndex.RowIndex);
                //WPF-25759 Need to update the SelectedIndex and SelectedItem by RefreshSelectedIndex method
                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();

                this.RaiseSelectionChanged(null, removedItems);
                return true;
            }

            

            if (!needToRemove)
            {
                removedItems.Clear();                
            }

            if (this.RaiseSelectionChanging(addedItems, removedItems))
            {
                CurrentCellManager.RemoveCurrentCell(newRowColumnIndex);
                CurrentCellManager.SelectCurrentCell(CurrentCellManager.previousRowColumnIndex);
                return false;
            }

            if (needToRemove)
            {
                this.RemoveSelection(newRowColumnIndex, removedItems);
            }
           
            AddSelection(newRowColumnIndex, addedItems);

            SuspendUpdates();
            this.RefreshSelectedIndex();
            ResumeUpdates();
            this.DataGrid.HideRowFocusBorder();
            this.previousSelectedCells.Clear();
            this.RaiseSelectionChanged(addedItems, removedItems);
            return true;
        }

        /// <summary>
        /// Processes the selection when the cells selected by using Shift key.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row and column index to perform shift cell selection.        
        /// </param>
        /// <param name="previousRowColumnIndex">
        /// Contains the previous row and column index.
        /// </param>
        /// <param name="key">
        /// Contains the key that was pressed.
        /// </param>
        /// <param name="needWholeRowSelect">
        /// Indicates whether the whole row is selected while processing the shift cell selection.
        /// </param>       
        protected void ProcessShiftSelection(RowColumnIndex newRowColumnIndex, RowColumnIndex previousRowColumnIndex, Key key, bool needToSelectWholeRow = false)
        {
            if (newRowColumnIndex == previousRowColumnIndex)
                return;

            if (newRowColumnIndex.RowIndex >= this.DataGrid.GetFirstRowIndex() && previousRowColumnIndex.RowIndex < this.DataGrid.GetFirstRowIndex())
            {
                this.ProcessSelection(newRowColumnIndex, SelectionReason.PointerPressed);
                this.SetPressedIndex(newRowColumnIndex);
                return;
            }

            if (lastPressedKey == Key.None && !isInShiftSelection)
                previousSelectedCells = this.GetSelectedCells();
            
            object rowData = null;
            isInShiftSelection = true;
            if (needToSelectWholeRow)
                newRowColumnIndex.ColumnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

            var currentRowData = this.DataGrid.GetRecordAtRowIndex(newRowColumnIndex.RowIndex);
            var currentColumn = this.DataGrid.GetGridColumn(newRowColumnIndex.ColumnIndex);
            List<GridCellInfo> addedItems = new List<GridCellInfo>();
            List<GridCellInfo> removedItems = new List<GridCellInfo>();
            
            int pressedRowIndex = this.PressedRowColumnIndex.RowIndex;
            int pressedColumnIndex = this.PressedRowColumnIndex.ColumnIndex;
            int columnIndex = 0;
            int comparerColumnIndex = 0;
            bool isInLargeSelection = false;
            bool needToRemove = !this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex) && ((newRowColumnIndex.ColumnIndex < previousRowColumnIndex.ColumnIndex && newRowColumnIndex.ColumnIndex >= pressedColumnIndex) || (newRowColumnIndex.ColumnIndex > previousRowColumnIndex.ColumnIndex && newRowColumnIndex.ColumnIndex <= pressedColumnIndex) || (newRowColumnIndex.RowIndex < previousRowColumnIndex.RowIndex && newRowColumnIndex.RowIndex >= pressedRowIndex) || (newRowColumnIndex.RowIndex > previousRowColumnIndex.RowIndex && newRowColumnIndex.RowIndex <= pressedRowIndex)) && this.SelectedCells.Count > 0;
            List<object> addedItem = new List<object>();
            List<object> removedItem = new List<object>();

            if (!this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex))
            {
                switch (key)
                {
                    case Key.None:
                    case Key.PageDown:
                    case Key.PageUp:
                    case Key.Home:
                    case Key.End:
                        var rItems = this.GetSelectedCells();
                        var aItems = new List<GridCellInfo>();
                        isInLargeSelection = true;

                        if (pressedRowIndex <= newRowColumnIndex.RowIndex)
                        {
                            while (pressedRowIndex <= newRowColumnIndex.RowIndex)
                            {
                                if (!this.DataGrid.IsInDetailsViewIndex(pressedRowIndex))
                                {
                                    GridUnBoundRow undBoundRow = null;
                                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                                    if (nodeEntry == null && DataGrid.IsUnBoundRow(pressedRowIndex))
                                    {
                                        undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                                        rowData = null;
                                    }
                                    else
                                        rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);

                                    columnIndex = pressedColumnIndex;
                                    GridCellInfo gridCellInfo = null;
                                    if (rowData != null || undBoundRow != null)
                                    {
                                        if (!needToSelectWholeRow)
                                        {
                                            if (columnIndex <= newRowColumnIndex.ColumnIndex)
                                            {
                                                while (columnIndex <= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        aItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex++;
                                                }
                                            }
                                            else
                                            {
                                                while (columnIndex >= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        aItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex--;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.DataGrid.Columns.ForEach(column =>
                                            {
                                                if (column.AllowFocus && !column.IsHidden)
                                                {
                                                    if (rowData != null)
                                                        gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                                    else
                                                        gridCellInfo = new GridCellInfo(column, pressedRowIndex, undBoundRow);
                                                    aItems.Add(gridCellInfo);
                                                }
                                            });
                                        }
                                    }
                                    else
                                    {
                                        gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                                        aItems.Add(gridCellInfo);
                                    }
                                }
                                pressedRowIndex = pressedRowIndex == this.DataGrid.GetLastRowIndex() ? ++pressedRowIndex : this.GetNextRowIndex(pressedRowIndex);
                            }
                        }
                        else
                        {
                            while (pressedRowIndex >= newRowColumnIndex.RowIndex)
                            {
                                if (!this.DataGrid.IsInDetailsViewIndex(pressedRowIndex))
                                {
                                    GridUnBoundRow undBoundRow = null;
                                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                                    if (nodeEntry == null && DataGrid.IsUnBoundRow(pressedRowIndex))
                                    {
                                        undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                                        rowData = null;
                                    }
                                    else
                                        rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);
                                    columnIndex = pressedColumnIndex;
                                    GridCellInfo gridCellInfo = null;
                                    if (rowData != null || undBoundRow != null)
                                    {
                                        if (!needToSelectWholeRow)
                                        {
                                            if (columnIndex >= newRowColumnIndex.ColumnIndex)
                                            {
                                                while (columnIndex >= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        aItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex--;
                                                }
                                            }
                                            else
                                            {
                                                while (columnIndex <= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        aItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex++;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            this.DataGrid.Columns.ForEach(column =>
                                            {
                                                if (column.AllowFocus && !column.IsHidden)
                                                {
                                                    if (rowData != null)
                                                        gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                                    else
                                                        gridCellInfo = new GridCellInfo(column, pressedRowIndex, undBoundRow);
                                                    aItems.Add(gridCellInfo);
                                                }
                                            });
                                        }
                                    }
                                    else
                                    {
                                        gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                                        aItems.Add(gridCellInfo);
                                    }
                                }
                                pressedRowIndex = pressedRowIndex == this.DataGrid.GetFirstRowIndex() ? --pressedRowIndex : this.GetPreviousRowIndex(pressedRowIndex);
                            }
                        }

                        var commonItems = aItems.Intersect(rItems).ToList();
                        removedItems = rItems.Except(commonItems).ToList();
                        addedItems = aItems.Except(commonItems).ToList();
                        removedItem = removedItems.Except(previousSelectedCells).Cast<object>().ToList<object>();

                        break;
                    case Key.Right:
                    case Key.Left:
                        if ((SelectionHelper.CheckControlKeyPressed() && SelectionHelper.CheckShiftKeyPressed()) || this.lastSelectedIndex != RowColumnIndex.Empty)
                        {
                            goto case Key.End;
                        }
                        else
                        {
                            if (pressedRowIndex <= newRowColumnIndex.RowIndex)
                            {
                                while (pressedRowIndex <= newRowColumnIndex.RowIndex)
                                {
                                    if (!this.DataGrid.IsInDetailsViewIndex(pressedRowIndex))
                                    {
                                        GridUnBoundRow undBoundRow = null;
                                        NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                                        if (nodeEntry == null && DataGrid.IsUnBoundRow(pressedRowIndex))
                                        {
                                            undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                                            rowData = null;
                                        }
                                        else
                                            rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);                                        
                                        if (!needToRemove)
                                        {
                                            columnIndex = newRowColumnIndex.ColumnIndex;
                                            comparerColumnIndex = previousRowColumnIndex.ColumnIndex;
                                        }
                                        else
                                        {
                                            columnIndex = previousRowColumnIndex.ColumnIndex;
                                            comparerColumnIndex = newRowColumnIndex.ColumnIndex;
                                        }
                                        GridCellInfo gridCellInfo = null;
                                        if (rowData != null || undBoundRow != null)
                                        {
                                            if (columnIndex < comparerColumnIndex)
                                            {
                                                while (columnIndex < comparerColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        addedItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex++;
                                                }
                                            }
                                            else
                                            {
                                                while (columnIndex > comparerColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        addedItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex--;
                                                }
                                            }
                                        }
                                        else if (!needToRemove)
                                        {
                                            gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                                            addedItems.Add(gridCellInfo);
                                        }
                                    }
                                    pressedRowIndex = pressedRowIndex == this.DataGrid.GetLastRowIndex() ? ++pressedRowIndex : this.GetNextRowIndex(pressedRowIndex);
                                }
                            }
                            else
                            {
                                while (pressedRowIndex >= newRowColumnIndex.RowIndex)
                                {
                                    if (!this.DataGrid.IsInDetailsViewIndex(pressedRowIndex))
                                    {
                                        GridUnBoundRow undBoundRow = null;
                                        NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                                        if (nodeEntry == null && DataGrid.IsUnBoundRow(pressedRowIndex))
                                        {
                                            undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                                            rowData = null;
                                        }
                                        else
                                            rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);
                                        if (!needToRemove)
                                        {
                                            columnIndex = newRowColumnIndex.ColumnIndex;
                                            comparerColumnIndex = previousRowColumnIndex.ColumnIndex;
                                        }
                                        else
                                        {
                                            columnIndex = previousRowColumnIndex.ColumnIndex;
                                            comparerColumnIndex = newRowColumnIndex.ColumnIndex;
                                        }
                                        GridCellInfo gridCellInfo = null;
                                        if (rowData != null || undBoundRow != null)
                                        {
                                            if (columnIndex > comparerColumnIndex)
                                            {
                                                while (columnIndex > comparerColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    { 
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                    
                                                            addedItems.Add(gridCellInfo);
                                                        columnIndex--;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                while (columnIndex < comparerColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                {
                                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                                        addedItems.Add(gridCellInfo);
                                                    }
                                                    columnIndex++;
                                                }
                                            }
                                        }
                                        else if (!needToRemove)
                                        {
                                            gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                                            addedItems.Add(gridCellInfo);
                                        }
                                    }
                                    pressedRowIndex = pressedRowIndex == this.DataGrid.GetFirstRowIndex() ? --pressedRowIndex : this.GetPreviousRowIndex(pressedRowIndex);
                                }
                            }
                        }
                        break;
                    case Key.Down:
                    case Key.Up:
                        if (this.DataGrid.HasView && this.DataGrid.View.GroupDescriptions.Count > 0 && key == Key.Down)
                        {
                            var group = this.DataGrid.View.TopLevelGroup.DisplayElements[this.DataGrid.ResolveToRecordIndex(previousRowColumnIndex.RowIndex)];
                            int itemsCount = 0;
                            if (group != null && group is Group)
                            {
                                this.DataGrid.View.TopLevelGroup.ComputeCount(group as Group, ref itemsCount);
                                if (group is Group && (group as Group).IsExpanded && SelectionHelper.CheckShiftKeyPressed() && this.PressedRowColumnIndex.RowIndex > (previousRowColumnIndex.RowIndex + itemsCount))
                                {
                                    needToRemove = false;
                                    goto case Key.End;
                                }
                            }
                        }
                        if ((SelectionHelper.CheckControlKeyPressed() && SelectionHelper.CheckShiftKeyPressed()) || this.lastSelectedIndex != RowColumnIndex.Empty)
                        {
                            goto case Key.End;
                        }
                        else
                        {
                            int rowIndex = 0;
                            int comparerRowIndex = 0;
                            if (!needToRemove)
                            {
                                rowIndex = newRowColumnIndex.RowIndex;
                                comparerRowIndex = previousRowColumnIndex.RowIndex;
                            }
                            else
                            {
                                rowIndex = previousRowColumnIndex.RowIndex;
                                comparerRowIndex = newRowColumnIndex.RowIndex;
                            }
                            if (rowIndex < comparerRowIndex)
                            {
                                while (rowIndex < comparerRowIndex)
                                {
                                    if (!this.DataGrid.IsInDetailsViewIndex(rowIndex))
                                    {
                                        GridUnBoundRow undBoundRow = null;
                                        NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                                        if (nodeEntry == null && DataGrid.IsUnBoundRow(rowIndex))
                                        {
                                            undBoundRow = DataGrid.GetUnBoundRow(rowIndex);
                                            rowData = null;
                                        }
                                        else
                                            rowData = this.DataGrid.GetRecordAtRowIndex(rowIndex);
                                        columnIndex = pressedColumnIndex;
                                        GridCellInfo gridCellInfo = null;
                                        if (rowData != null || undBoundRow != null)
                                        {
                                            if (!needToSelectWholeRow)
                                            {
                                                if (columnIndex <= newRowColumnIndex.ColumnIndex)
                                                {
                                                    while (columnIndex <= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                    {
                                                        if (this.DataGrid.AllowFocus(new RowColumnIndex(rowIndex, columnIndex)))
                                                        {
                                                            if (rowData != null)
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                            else
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowIndex, undBoundRow);
                                                            addedItems.Add(gridCellInfo);
                                                        }
                                                        columnIndex++;
                                                    }
                                                }
                                                else
                                                {
                                                    while (columnIndex >= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                    {
                                                        if (this.DataGrid.AllowFocus(new RowColumnIndex(rowIndex, columnIndex)))
                                                        {
                                                            if (rowData != null)
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                            else
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowIndex, undBoundRow);
                                                            addedItems.Add(gridCellInfo);
                                                        }
                                                        columnIndex--;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                this.DataGrid.Columns.ForEach(column =>
                                                {
                                                    if (column.AllowFocus && !column.IsHidden)
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(column, rowIndex, undBoundRow);
                                                        addedItems.Add(gridCellInfo);
                                                    }
                                                });
                                            }
                                        }
                                        else
                                        {
                                            gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, rowIndex);
                                            addedItems.Add(gridCellInfo);
                                        }
                                    }
                                    rowIndex = this.DataGrid.DetailsViewManager.HasDetailsView && rowIndex != this.DataGrid.GetLastRowIndex() ? this.GetNextRowIndex(rowIndex) : ++rowIndex;
                                }
                            }
                            else
                            {
                                while (rowIndex > comparerRowIndex)
                                {
                                    if (!this.DataGrid.IsInDetailsViewIndex(rowIndex))
                                    {
                                        GridUnBoundRow undBoundRow = null;
                                        NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(rowIndex);
                                        if (nodeEntry == null && DataGrid.IsUnBoundRow(rowIndex))
                                        {
                                            undBoundRow = DataGrid.GetUnBoundRow(rowIndex);
                                            rowData = null;
                                        }
                                        else
                                            rowData = this.DataGrid.GetRecordAtRowIndex(rowIndex);
                                        columnIndex = pressedColumnIndex;
                                        GridCellInfo gridCellInfo = null;
                                        if (rowData != null || undBoundRow != null)
                                        {
                                            if (!needToSelectWholeRow)
                                            {
                                                if (columnIndex >= newRowColumnIndex.ColumnIndex)
                                                {
                                                    while (columnIndex >= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                    {
                                                        if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                                        {
                                                            if (rowData != null)
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                            else
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowIndex, undBoundRow);
                                                            addedItems.Add(gridCellInfo);
                                                        }
                                                        columnIndex--;
                                                    }
                                                }
                                                else
                                                {
                                                    while (columnIndex <= newRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                                    {
                                                        if (this.DataGrid.AllowFocus(new RowColumnIndex(rowIndex, columnIndex)))
                                                        {
                                                            if (rowData != null)
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                                            else
                                                                gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowIndex, undBoundRow);
                                                            addedItems.Add(gridCellInfo);
                                                        }
                                                        columnIndex++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                this.DataGrid.Columns.ForEach(column =>
                                                {
                                                    if (column.AllowFocus && !column.IsHidden)
                                                    {
                                                        if (rowData != null)
                                                            gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                                        else
                                                            gridCellInfo = new GridCellInfo(column, rowIndex, undBoundRow);
                                                        addedItems.Add(gridCellInfo);
                                                    }
                                                });
                                            }
                                        }
                                        else
                                        {
                                            gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, rowIndex);
                                            addedItems.Add(gridCellInfo);
                                        }
                                    }
                                    rowIndex = this.DataGrid.DetailsViewManager.HasDetailsView && rowIndex != this.DataGrid.GetFirstRowIndex() ? this.GetPreviousRowIndex(rowIndex) : --rowIndex;
                                }
                            }
                        }
                        break;
                }
            }

            addedItem = addedItems.Except(previousSelectedCells).Cast<object>().ToList<object>();

            if (needToRemove && !isInLargeSelection)
            {
                removedItem = addedItem.ToList();
                addedItem.Clear();
            }
            else if (this.lastPressedKey == Key.A || this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex))
            {
                removedItems = this.GetSelectedCells();
                if (this.DataGrid.IsInDetailsViewIndex(newRowColumnIndex.RowIndex))
                {
                    var nodeEntry = this.DataGrid.GetNodeEntry(newRowColumnIndex.RowIndex);
                    removedItem = removedItems.Cast<object>().ToList<object>();
                    addedItem.Add(new GridCellInfo(currentColumn, null, nodeEntry, newRowColumnIndex.RowIndex));
                    this.previousSelectedCells.Clear();
                    this.SetPressedIndex(new RowColumnIndex(newRowColumnIndex.RowIndex, this.PressedRowColumnIndex.ColumnIndex));
                    isInLargeSelection = true;
                }
                else
                {
                    var cellInfo = this.SelectedCells.Find(this.DataGrid.GetNodeEntry(this.CurrentCellManager.previousRowColumnIndex.RowIndex));
                    if (SelectionHelper.CheckShiftKeyPressed() && key != Key.None && cellInfo != null)
                    {
                        removedItems.Remove(this.DataGrid.GetGridCellInfo(this.CurrentCellManager.previousRowColumnIndex));
                    }
                    //WPF-29408 If lastPressedKey is 'A' we should not use the removeItems. Because in this case removeItems holds the whole grid cells.
                    if (this.lastPressedKey != Key.A)
                        removedItem = removedItems.Except(previousSelectedCells).Cast<object>().ToList<object>();
                }
            }

            if (removedItem.Count > 0)
            {
                needToRemove = true;                
            }

            if (this.RaiseSelectionChanging(addedItem, removedItem))
                return;

            if (needToRemove)
            {
                RemoveSelection(newRowColumnIndex, removedItem);
            }

            if (isInLargeSelection || !needToRemove || this.lastPressedKey == Key.A)
                AddSelection(newRowColumnIndex, addedItem);

            SuspendUpdates();
            this.RefreshSelectedIndex();
            ResumeUpdates();

            RaiseSelectionChanged(addedItem, removedItem);
        }
   
        /// <summary>
        /// Adds the cell selection for the specified list of items and rowcolumnindex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to add cell selection.
        /// </param>
        /// <param name="addedItems">
        /// The corresponding list of items to add the selection.
        /// </param>
        protected void AddSelection(RowColumnIndex rowColumnIndex, List<object> addedItems)
        {
            if (rowColumnIndex.RowIndex < 0 || rowColumnIndex.ColumnIndex < 0 || (addedItems.Count < 1 && !isInShiftSelection && !this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex)))
                return;

            this.SuspendUpdates();
            if (addedItems.Count > 0)
                this.AddCells(addedItems);
            else if (this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                ShowCellSelection(this.DataGrid.GetDataColumnBase(rowColumnIndex));
            SuspendUpdates();
            this.RefreshSelectedIndex();
            ResumeUpdates();
            this.ResumeUpdates();
        }
       
        /// <summary>
        /// Removes the selection for the specified list of items and rowcolumnindex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to remove the selection.
        /// </param>
        /// <param name="removedItems">
        /// The corresponding list of items to remove the cell selection.
        /// </param>
        protected void RemoveSelection(RowColumnIndex rowColumnIndex, List<object> removedItems)
        {

            if (!ValidationHelper.IsCurrentCellValidated) 
                return;

            if ((this.SelectedCells.Count < 0 || removedItems.Count < 1) && !this.DataGrid.IsAddNewIndex(this.CurrentCellManager.previousRowColumnIndex.RowIndex))
                return;

            SuspendUpdates();
            if (removedItems.Count > 0)
                this.RemoveCells(removedItems);

            ResumeUpdates();

        }

        internal List<GridCellInfo> SelectMultipleCells(RowColumnIndex startRowColumnIndex, RowColumnIndex endRowColumnIndex)
        {
            int startRowIndex = startRowColumnIndex.RowIndex;
            var currentColumn = this.DataGrid.GetGridColumn(endRowColumnIndex.ColumnIndex);
            var items = new List<GridCellInfo>();
            if (startRowIndex <= endRowColumnIndex.RowIndex)
            {
                while (startRowIndex <= endRowColumnIndex.RowIndex)
                {
                    object rowData = null;
                    GridUnBoundRow undBoundRow = null;
                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(startRowIndex);
                    if (nodeEntry == null && DataGrid.IsUnBoundRow(startRowIndex))
                    {
                        undBoundRow = DataGrid.GetUnBoundRow(startRowIndex);
                        rowData = null;
                    }
                    else
                        rowData = this.DataGrid.GetRecordAtRowIndex(startRowIndex);
                    int columnIndex = startRowColumnIndex.ColumnIndex;
                    GridCellInfo gridCellInfo = null;

                    if (undBoundRow != null || (rowData != null && !this.DataGrid.IsInDetailsViewIndex(startRowIndex)))
                    {
                        if (columnIndex <= endRowColumnIndex.ColumnIndex)
                        {
                            while (columnIndex <= endRowColumnIndex.ColumnIndex && columnIndex >= this.CurrentCellManager.GetFirstCellIndex())
                            {
                                if (this.DataGrid.AllowFocus(new RowColumnIndex(startRowIndex, columnIndex)))
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), startRowIndex, undBoundRow);
                                    gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                    items.Add(gridCellInfo);
                                }
                                columnIndex++;
                            }
                        }
                        else
                        {
                            while (columnIndex >= endRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                            {
                                if (this.DataGrid.AllowFocus(new RowColumnIndex(startRowIndex, columnIndex)))
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), startRowIndex, undBoundRow);
                                    items.Add(gridCellInfo);
                                }
                                columnIndex--;
                            }
                        }
                    }
                    startRowIndex = startRowIndex == this.DataGrid.GetLastDataRowIndex() ? ++startRowIndex : this.GetNextRowIndex(startRowIndex);
                }
            }
            else
            {
                while (startRowIndex >= endRowColumnIndex.RowIndex)
                {
                    object rowData = null;
                    GridUnBoundRow undBoundRow = null;
                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(startRowIndex);
                    if (nodeEntry == null && DataGrid.IsUnBoundRow(startRowIndex))
                    {
                        undBoundRow = DataGrid.GetUnBoundRow(startRowIndex);
                        rowData = null;
                    }
                    else                        
                        rowData = this.DataGrid.GetRecordAtRowIndex(startRowIndex);
                    int columnIndex = startRowColumnIndex.ColumnIndex;
                    GridCellInfo gridCellInfo = null;

                    if (undBoundRow != null || (rowData != null && !this.DataGrid.IsInDetailsViewIndex(startRowIndex)))
                    {
                        if (columnIndex >= endRowColumnIndex.ColumnIndex)
                        {
                            while (columnIndex >= endRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                            {
                                if (this.DataGrid.AllowFocus(new RowColumnIndex(startRowIndex, columnIndex)))
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), startRowIndex, undBoundRow);
                                    items.Add(gridCellInfo);
                                }
                                columnIndex--;
                            }
                        }
                        else
                        {
                            while (columnIndex <= endRowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                            {
                                if (this.DataGrid.AllowFocus(new RowColumnIndex(startRowIndex, columnIndex)))
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), startRowIndex, undBoundRow);
                                    items.Add(gridCellInfo);
                                }
                                columnIndex++;
                            }
                        }
                    }
                    startRowIndex = startRowIndex == this.DataGrid.GetFirstDataRowIndex() ? --startRowIndex : this.GetPreviousRowIndex(startRowIndex);
                }
            }
            return items;
        }

        /// <summary>
        /// Resets the selection for the specified row and column index.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to reset the cell selection.
        /// </param>
        /// <param name="removedItems">
        /// The corresponding list of items to reset the cell selection. 
        /// </param>
        /// <param name="setFocuForGrid">
        /// Indicates whether the focus is set to SfDataGrid after reset the selection.
        /// </param>
        protected internal void ResetSelection(RowColumnIndex rowColumnIndex, List<object> removedItems, bool setFocusGrid = true)
        {            
            var addedItems = new List<object>();

            var cellInfo = this.DataGrid.GetGridCellInfo(rowColumnIndex); 

            addedItems.Add(cellInfo);
            this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
            CurrentCellManager.SelectCurrentCell(rowColumnIndex, setFocusGrid);
            if (removedItems != null && removedItems.Count > 0)
            {                
                this.RemoveSelection(rowColumnIndex, removedItems);
            }
            
            AddSelection(rowColumnIndex, addedItems);
            ResetFlags();
            //To cancel the Selection changed event when changing the CurrentItem.
            if (!cancelSelectionChangedEvent)
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

                var selectioncController = parentDataGrid.SelectionController as GridCellSelectionController;
                var detailsViewDataRow = parentDataGrid.RowGenerator.Items.FirstOrDefault(row => (row is DetailsViewDataRow) && (row as DetailsViewDataRow).DetailsViewDataGrid == this.DataGrid);
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
                    if (selectioncController.SelectedCells.Count > 0)                                              
                        rItems = selectioncController.SelectedCells.ConvertToGridCellInfoList().ToList<object>();
                    selectioncController.cancelSelectionChangedEvent = true;
                    //Adds the selection to the specified detailsViewIndex.
                    selectioncController.ResetSelection(new RowColumnIndex(detailsViewIndex, selectioncController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), rItems, false);
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
        /// Shows the cell selection background for the corresponding column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to show cell selection.
        /// </param>
        protected void ShowCellSelection(DataColumnBase column)
        {
            if (column != null)
                column.IsSelectedColumn = true;
        }

        /// <summary>
        /// Hides the cell selection background for the corresponding column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to hide the cell selection.
        /// </param>
        protected void HideCellSelection(DataColumnBase column)
        {
            if (column != null)
                column.IsSelectedColumn = false;
        }

        /// <summary>
        /// Hides all the cell selection background of all selected cells in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentCell">
        /// Indicates whether the selection should be hidden except for the current Cell.
        /// </param>
        protected void HideAllSelection(bool exceptCurrentCell)
        {
            if (!exceptCurrentCell && this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex))
                this.HideCellSelection(this.DataGrid.GetDataColumnBase(this.CurrentCellManager.CurrentRowColumnIndex));

            if (this.SelectedCells != null)
            {
                foreach (var item in SelectedCells)
                {
                    if (item.IsDataRow || item.IsUnBoundRow)
                    {
                        var rowBase = item.IsDataRow
                            ? this.DataGrid.RowGenerator.Items.Where(row => row.RowData == item.RowData || row.IsSpannedRow)
                            : this.DataGrid.RowGenerator.Items.Where(row => row.RowIndex == item.RowIndex || row.IsSpannedRow);                        
                        if (rowBase != null)
                        {
                            rowBase.ForEach(row =>
                            {
                                row.VisibleColumns.ForEach(dataColumn => 
                                {
                                    if (!exceptCurrentCell)
                                        HideCellSelection(dataColumn);
                                    else if(!(this.DataGrid.CurrentColumn == dataColumn.GridColumn && this.DataGrid.CurrentItem == item.RowData && !this.DataGrid.IsInDetailsViewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)))
                                        HideCellSelection(dataColumn);
                                });
                            });
                        }
                    }
                    else
                        this.DataGrid.HideRowSelection(item.RowIndex);
                }
            }
        }

        /// <summary>
        /// Clears the cell selection in the corresponding Group.
        /// </summary>
        /// <param name="group">
        /// The corresponding group to clear the cell selection.
        /// </param>
        protected void ClearGroupSelections(Group group)
        {
            if (group != null && group.IsExpanded)
            {
                if(group.Records!=null)
                {
                    int index = group.Records.Count - 1;
                    while (index >= 0)
                    {
                        //var recordIndex = this.DataGrid.View.TopLevelGroup.DisplayElements.IndexOf(group.Records[index]);
                        var cellsCollection = this.SelectedCells.Find(group.Records[index].Data);
                        if (cellsCollection != null)
                        {
                            cellsCollection.ColumnCollection.ForEach(column =>
                                {
                                    var cellInfo = new GridCellInfo(column, cellsCollection.RowData, cellsCollection.NodeEntry);
                                    if (this.previousSelectedCells.Contains(cellInfo))
                                        this.previousSelectedCells.Remove(cellInfo);
                                });
                            this.SelectedCells.Remove(cellsCollection);
                        }
                        index--;
                    }
                }
                else if(group.Groups != null)
                {
                    group.Groups.ForEach(item =>
                        {
                            this.ClearGroupSelections(item as Group);
                        });
                }
            }
        }


        #endregion

        #region Index Helper

        /// <summary>
        /// Gets the index of next cell corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the index of next cell.
        /// </param>
        /// <returns>
        /// Returns the index of next cell.
        /// </returns>
        protected virtual int GetNextCellIndex(int index)
        {
            int lastCellIndex = this.CurrentCellManager.GetLastCellIndex();
            int nextCellIndex = this.GetFocusedColumnIndex(index + 1, MoveDirection.Right);
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
            {
                if (nextCellIndex <= PressedRowColumnIndex.ColumnIndex && nextCellIndex != this.CurrentCellManager.GetLastCellIndex())
                    nextCellIndex = GetFocusedColumnIndex(nextCellIndex - 1, MoveDirection.Right);
                if (index == this.CurrentCellManager.GetFirstCellIndex() && index < nextCellIndex && index != PressedRowColumnIndex.ColumnIndex)
                    nextCellIndex = index;
            }
            nextCellIndex = (nextCellIndex > lastCellIndex && !this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex)) ? lastCellIndex : nextCellIndex;
            return nextCellIndex;
        }

        /// <summary>
        /// Gets the index of previous cell corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the previous cell index.
        /// </param>
        /// <returns>
        /// Returns the index of previous cell.
        /// </returns>
        protected virtual int GetPreviousCellIndex(int index)
        {
            int firsCellIndex = this.CurrentCellManager.GetFirstCellIndex();
            int previousCellIndex = index;
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
                previousCellIndex = previousCellIndex <= PressedRowColumnIndex.ColumnIndex ? GetFocusedColumnIndex(previousCellIndex - 1, MoveDirection.Left) : previousCellIndex;
            else
                previousCellIndex = this.GetFocusedColumnIndex(index - 1, MoveDirection.Left);
            previousCellIndex = previousCellIndex < firsCellIndex && !this.DataGrid.IsAddNewIndex(this.CurrentCellManager.CurrentRowColumnIndex.RowIndex) ? firsCellIndex : previousCellIndex;
            return previousCellIndex;
        }

        /// <summary>
        /// Method which return the next focused column index.
        /// </summary>
        /// <param name="index"></param>
        /// <param name="direction">To check whether in Right or Left.</param>
        /// <returns></returns>
        private int GetFocusedColumnIndex(int index, MoveDirection direction)
        {
            GridColumn column = this.CurrentCellManager.GetNextFocusGridColumn(index, direction);
            if (column != null)
                return this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
            return -1;
        }

        /// <summary>
        /// Gets the index of next row corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the index of next row.
        /// </param>
        /// <returns>
        /// Returns the index of next row.
        /// </returns>
        protected virtual int GetNextRowIndex(int index)
        {
            //When there is no records, then the index will be -1 and the LastNavigatinRowIndex also returns -1, hence the below code is added.
            //It is also used when passing the last rowIndex as the index the same index has to be return.
            if (index >= this.DataGrid.GetLastNavigatingRowIndex())
                return this.DataGrid.GetLastNavigatingRowIndex();

            //When View is null with AddNewRowPosition on Top and with UnboundRows, returns wrong value. Hence the GetFirstRowIndex
            //method is used.
            int firstRowIndex = this.DataGrid.GetFirstNavigatingRowIndex();
            if (index < firstRowIndex)
                return firstRowIndex;

            if (index >= this.DataGrid.VisualContainer.ScrollRows.LineCount)
                return -1;
            var nextIndex = 0;      
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

            nextIndex = (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended 
                && nextIndex <= PressedRowColumnIndex.RowIndex && nextIndex != this.DataGrid.GetLastNavigatingRowIndex() && !this.DataGrid.DetailsViewManager.HasDetailsView) ? this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(nextIndex) : nextIndex;
            if (this.DataGrid.IsInDetailsViewIndex(nextIndex))
            {
                //Added the code to get the next scroll line index to skip the recursive execution of the same method 
                //when DetailsDataRow is last row and not having no records.
                //WPF-17881 Here we have to check the corresponding RowColumnIndex Grid is DetailsViewGrid or Not
                if (nextIndex == index && this.dataGrid.GetDetailsViewGridInView(dataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex) == null)
                    nextIndex = this.DataGrid.VisualContainer.ScrollRows.GetNextScrollLineIndex(index);
                else
                {
                    var detailsViewGrid = this.DataGrid.GetDetailsViewGridInView(nextIndex);
                    //Added the condition while navigating using keys without the DataRows in the DetailsViewGrid Exception raises. so checked with the lastDetailsViewGrid having records or not.
                    if (nextIndex > DataGrid.GetLastNavigatingRowIndex())
                        return index;
                    //WPF-18257 Here we have check the condition, while the DetailsViewDataGrid has no Records to skip the selection for the corresponding Index value.
                    if (detailsViewGrid != null)
                    {
                        //WPF-22527 if record count is zero and having addnewrow, need to provide support for navigation in child grid addnewrow.
                        nextIndex = (detailsViewGrid.GetRecordsCount() > 0) || ((detailsViewGrid.FilterRowPosition != FilterRowPosition.None 
                            || detailsViewGrid.AddNewRowPosition != AddNewRowPosition.None) && detailsViewGrid.HasView) ? nextIndex : GetNextRowIndex(nextIndex);
                    }
                }
            }
            return nextIndex;
        }

        /// <summary>
        /// Gets the index of previous row corresponding to the specified index.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to get the index of previous row.
        /// </param>
        /// <returns>
        /// Returns the index of previous row.
        /// </returns>
        protected virtual int GetPreviousRowIndex(int index)
        {
            if (index <= this.DataGrid.GetFirstNavigatingRowIndex())
                return this.DataGrid.GetFirstNavigatingRowIndex();

            int previousIndex = index;
            if (SelectionHelper.CheckShiftKeyPressed() && this.DataGrid.SelectionMode == GridSelectionMode.Extended)
            {
                if (previousIndex <= PressedRowColumnIndex.RowIndex)
                    previousIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
                else if (previousIndex >= PressedRowColumnIndex.RowIndex && this.DataGrid.DetailsViewManager.HasDetailsView)
                    previousIndex = this.DataGrid.VisualContainer.ScrollRows.GetPreviousScrollLineIndex(previousIndex);
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
                    previousIndex = (detailsViewGrid.GetRecordsCount() > 0) || ((detailsViewGrid.FilterRowPosition != FilterRowPosition.None 
                        || detailsViewGrid.AddNewRowPosition != AddNewRowPosition.None) && detailsViewGrid.HasView) ? previousIndex : GetPreviousRowIndex(previousIndex);
                }
            }
            return previousIndex;
        }

        /// <summary>
        /// Gets the corresponding RowColumnIndex of the specified row data and column.
        /// </summary>
        /// <param name="rowData">
        /// The corresponding row data to get the RowColumnIndex.
        /// </param>
        /// <param name="column">
        /// The corresponding column to get the RowColumnIndex.
        /// </param>
        /// <returns>
        /// Returns the RowColumnIndex of the specified row data and column
        /// </returns>
        protected RowColumnIndex GetRowColumnIndex(object rowData, GridColumn column)
        {
            var rowIndex = this.DataGrid.ResolveToRowIndex(rowData);
            var colIndex = this.DataGrid.ResolveToScrollColumnIndex(this.DataGrid.Columns.IndexOf(column));
            return new RowColumnIndex(rowIndex, colIndex);
        }

        #endregion

        #endregion

        #region Selecting Cells Methods

        /// <summary>
        /// Selects the cell for the specified row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Specifies the corresponding rowData to select the cell.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column to select the cell.
        /// </param>
        public void SelectCell(object rowData, GridColumn column)
        {
            if(rowData != null || column != null)
            {
                var rowColumnIndex = this.GetRowColumnIndex(rowData, column);
                var nodeEntry = this.DataGrid.GetNodeEntry(rowColumnIndex.RowIndex);
                var cellInfo = new GridCellInfo(column, rowData, nodeEntry);
                if (rowColumnIndex.ColumnIndex < 0 || rowColumnIndex.RowIndex < 0 || !this.DataGrid.AllowFocus(rowColumnIndex) || this.SelectedCells.Contains(cellInfo))
                    return;

                var addedItems = new List<object>();
                var removedItems = new List<object>();

                addedItems.Add(cellInfo);

                if (this.DataGrid.SelectionMode == GridSelectionMode.Single || this.DataGrid.SelectedDetailsViewGrid != null)
                {
                    //Checked the CurrentRowColumnIndex is equal to the rowColumnIndex or Not because of the CurrentCell Could not be selected when passing the rowColumnIndex to the SelectCell Button.
                    if ( rowColumnIndex != this.CurrentCellManager.CurrentRowColumnIndex &&(!CurrentCellManager.ProcessCurrentCellSelection(rowColumnIndex, ActivationTrigger.Program)))
                        return;

                    if (this.DataGrid.SelectedDetailsViewGrid != null)
                    {
                        this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid);
                    }
                    removedItems = this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>();
                    //if there is no cell selected, when we cancel the Selection through the SelectionChanging Event 
                    //Then it will not hitted so that we have moved the selection changing event.
                    if (this.RaiseSelectionChanging(addedItems, removedItems))
                    {
                        CurrentCellManager.RemoveCurrentCell(rowColumnIndex);
                        CurrentCellManager.SelectCurrentCell(CurrentCellManager.previousRowColumnIndex);
                        return;
                    }
                    if (this.SelectedCells.Count > 0)
                    {                                                
                        this.RemoveSelection(rowColumnIndex, removedItems);
                    }                    
                    this.AddSelection(rowColumnIndex, addedItems);
                }
                else
                {
                    if (this.RaiseSelectionChanging(addedItems, removedItems))
                        return;

                    //CurrentCell has been selected when there is no current cell or current row in grid.
                    if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                        this.CurrentCellManager.SelectCurrentCell(rowColumnIndex);

                    AddCells(addedItems);
                }
                //WPF-25771 Reset the parentgrid selection when the selection in DetailsViewGrid
                ResetParentGridSelection();
                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();
                this.RaiseSelectionChanged(addedItems, removedItems);
            }
        }

        /// <summary>
        /// Selects the range of cells for the specified row data and column information .     
        /// </summary>
        /// <param name="startRowData">
        /// Specifies the starting row position to select.
        /// </param>
        /// <param name="startColumn">
        /// Specifies the starting column position to select.
        /// </param>
        /// <param name="endRowData">
        /// Specifies the ending row position to select.
        /// </param>
        /// <param name="endColumn">
        /// Specifies the ending column position to select.
        /// </param>
        /// <remarks>
        /// This method is not applicable for Single and None selection mode.
        /// </remarks>       
        public void SelectCells(object startRowData, GridColumn startColumn, object endRowData, GridColumn endColumn)
        {
            if (startRowData != null || startColumn != null || endRowData != null || endColumn != null)
            {
                var startRowColumnIndex = this.GetRowColumnIndex(startRowData, startColumn);
                var endRowColumnIndex = this.GetRowColumnIndex(endRowData, endColumn);
                if (startRowColumnIndex.ColumnIndex < 0 || startRowColumnIndex.RowIndex < 0 || endRowColumnIndex.ColumnIndex < 0 || endRowColumnIndex.RowIndex < 0)
                    return;
                if (this.DataGrid.SelectedDetailsViewGrid != null)
                {
                    this.ClearDetailsViewGridSelections(this.DataGrid.SelectedDetailsViewGrid);
                }
                //CurrentCell has been selected when there is no current cell or current row in grid.
                if (this.CurrentCellManager.CurrentRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex())
                {
                    this.CurrentCellManager.SelectCurrentCell(startRowColumnIndex);
                    this.PressedRowColumnIndex = endRowColumnIndex;
                }

                var selectedItems = this.GetSelectedCells();
                List<GridCellInfo> addedItems = this.SelectMultipleCells(startRowColumnIndex, endRowColumnIndex);
                List<object> addedItem = addedItems.Except(selectedItems).Cast<object>().ToList<object>();

                if (this.RaiseSelectionChanging(addedItem, new List<object>()))
                    return;
                this.AddCells(addedItem);
                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();
                this.RaiseSelectionChanged(addedItem, new List<object>());
                //WPF-25771 Reset the parentgrid selection when the selection in DetailsViewGrid
                ResetParentGridSelection();
            }
        }
        
        /// <summary>
        /// Reset the ParentDataGrid selection to the corresponding DetailsViewIndex when the selection is moved to DetailsViewDataGrid
        /// </summary>
        private void ResetParentGridSelection()
        {
            if (!(this.DataGrid is DetailsViewDataGrid))
                return;

            var parentDataGrid = this.DataGrid.GetParentDataGrid();
            var selectionController = parentDataGrid.SelectionController as GridCellSelectionController;
            var removedItems = selectionController.SelectedCells.ConvertToGridCellInfoList().ToList<object>();
            var rowIndex = parentDataGrid.GetGridDetailsViewRowIndex(this.DataGrid as DetailsViewDataGrid);
            selectionController.ResetSelection(new RowColumnIndex(rowIndex, parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex), removedItems, true);
            if (parentDataGrid.NotifyListener != null)
                selectionController.ResetParentGridSelection();
        }

        /// <summary>
        /// Unselects the cell for the specified row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Specifies the corresponding rowData to unselect.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column to unselect.
        /// </param>
        public void UnSelectCell(object rowData, GridColumn column)
        {
            if (rowData != null || column != null)
            {
                var rowColumnIndex = this.GetRowColumnIndex(rowData, column);
                if (rowColumnIndex.ColumnIndex < 0 && rowColumnIndex.RowIndex < 0 && !this.DataGrid.AllowFocus(rowColumnIndex))
                    return;

                var removedItems = new List<object>();
                var nodeEntry = this.DataGrid.GetNodeEntry(rowColumnIndex.RowIndex);
                var cellInfo = new GridCellInfo(column, rowData, nodeEntry);
                if (this.SelectedCells.Contains(cellInfo))
                {
                    removedItems.Add(cellInfo);
                    if (this.RaiseSelectionChanging(new List<object>(), removedItems))
                        return;
                    if (this.DataGrid.SelectionMode == GridSelectionMode.Single)
                        this.RemoveSelection(rowColumnIndex, this.SelectedCells.ConvertToGridCellInfoList().Cast<object>().ToList<object>());
                    else
                        this.RemoveCells(removedItems);
                    SuspendUpdates();
                    this.RefreshSelectedIndex();
                    ResumeUpdates();
                    this.RaiseSelectionChanged(new List<object>(), removedItems);
                }
            }
        }

        /// <summary>
        /// Unselects the range of cells for the specified row data and column information.       
        /// </summary>
        /// <param name="startRowData">
        /// Specifies the starting row position to unselect.
        /// </param>
        /// <param name="startColumn">
        /// Specifies the starting column position to unselect.
        /// </param>
        /// <param name="endRowData">
        /// Specifies the ending row position to unselect.
        /// </param>
        /// <param name="endColumn">
        /// Specifies the ending column position to unselect.
        /// </param>
        /// <remarks>
        /// This method is not applicable for Single and None selection mode.
        /// </remarks>       
        public void UnSelectCells(object startRowData, GridColumn startColumn, object endRowData, GridColumn endColumn)
        {
            if (startRowData != null || startColumn != null || endRowData != null || endColumn != null)
            {
                var startRowColumnIndex = this.GetRowColumnIndex(startRowData, startColumn);
                var endRowColumnIndex = this.GetRowColumnIndex(endRowData, endColumn);

                if (startRowColumnIndex.ColumnIndex < 0 && startRowColumnIndex.RowIndex < 0 && endRowColumnIndex.ColumnIndex < 0 && endRowColumnIndex.RowIndex < 0)
                    return;

                var selectedItems = this.GetSelectedCells();
                List<GridCellInfo> removedItems = this.SelectMultipleCells(startRowColumnIndex, endRowColumnIndex);
                List<object> removedItem = removedItems.Intersect(selectedItems).Cast<object>().ToList<object>();

                if (this.RaiseSelectionChanging(new List<object>(), removedItem))
                    return;

                this.RemoveCells(removedItem);
                SuspendUpdates();
                this.RefreshSelectedIndex();
                ResumeUpdates();
                this.RaiseSelectionChanged(new List<object>(), removedItem);
            }
        }

        #endregion

        #region Drag Operations

        /// <summary>
        /// Processes the cell selection when mouse pointer moved on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the mouse move action.
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
            this.isMousePressed = args.Pointer.IsInContact && this.isMousePressed;
#endif
                  
            if (cancelDragSelection || this.DataGrid.SelectionMode == GridSelectionMode.None || this.DataGrid.SelectionMode == GridSelectionMode.Single || rowColumnIndex.RowIndex > this.DataGrid.GetLastNavigatingRowIndex() || this.PressedRowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() || rowColumnIndex.RowIndex < this.DataGrid.GetFirstNavigatingRowIndex() || rowColumnIndex == lastSelectedIndex || !this.isMousePressed || this.DataGrid.IsAddNewIndex(PressedRowColumnIndex.RowIndex))
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

            if (this.CurrentCellManager.CurrentCell != null && this.CurrentCellManager.CurrentCell.IsEditing && this.CurrentCellManager.CurrentRowColumnIndex == this.PressedRowColumnIndex)
                return;

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
                    //be in some other nestedGrid. Hence the current cell selection will be cleared by using corresponding rowColumn index
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

            if (!isInShiftSelection && (SelectionHelper.CheckShiftKeyPressed() || SelectionHelper.CheckControlKeyPressed() || this.DataGrid.SelectionMode == GridSelectionMode.Multiple) && !this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                this.previousSelectedCells = this.GetSelectedCells();
            else if (!isInShiftSelection)
                this.previousSelectedCells.Clear();
            
            this.ProcessDragSelection(args, rowColumnIndex);
        }

        /// <summary>
        /// Processes the cell selection when mouse pointer moved on the DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the mouse move action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex where the mouse move interaction occurs.    
        /// </param>
        protected virtual void ProcessDetailsViewGridPointerMoved(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var parentDataGrid = this.DataGrid.NotifyListener.GetParentDataGrid();
            var detailsViewDataRow = parentDataGrid.RowGenerator.Items.FirstOrDefault(row => (row is DetailsViewDataRow) && (row as DetailsViewDataRow).DetailsViewDataGrid == this.DataGrid);
            var colIndex = parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < 0
                                   ? parentDataGrid.SelectionController.CurrentCellManager.GetFirstCellIndex()
                                   : parentDataGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var rowcolIndex = new RowColumnIndex(detailsViewDataRow.RowIndex, colIndex);
            parentDataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Move, args), rowcolIndex);
            parentDataGrid.SelectedDetailsViewGrid = this.DataGrid;
        }

        /// <summary>
        /// Processes the cell selection when the mouse pointer is dragged on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data related to the mouse action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The starting rowColumnIndex of the drag selection.
        /// </param>
        /// <remarks>
        /// This method will be raised when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode"/> is Multiple or Extended.
        /// </remarks>     
        protected override void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex)
        {
            
            bool needToSelectWholeRow = this.DataGrid.showRowHeader ? this.DataGrid.SelectionUnit == GridSelectionUnit.Any && rowColumnIndex.ColumnIndex == 0 : false;
            if (this.DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex) || this.DataGrid.IsFilterRowIndex(rowColumnIndex.RowIndex))
                return;
            
            rowColumnIndex.ColumnIndex = this.DataGrid.AllowFocus(rowColumnIndex) ? rowColumnIndex.ColumnIndex : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            RowColumnIndex previouCellIndex = this.CurrentCellManager.CurrentRowColumnIndex;

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
            ProcessDragSelection(args, rowColumnIndex, previouCellIndex, needToSelectWholeRow);
        }
        
        private List<GridCellInfo> GetSelectedCells(RowColumnIndex Pressedindex, RowColumnIndex rowColumnIndex, bool needToSelectWholeRow)
        {
            int pressedColumnIndex = Pressedindex.ColumnIndex;
            int pressedRowIndex = Pressedindex.RowIndex;
            int columnIndex = 0;
            var aItems = new List<GridCellInfo>();
            var currentColumn = this.DataGrid.GetGridColumn(rowColumnIndex.ColumnIndex);
            object rowData = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            if (pressedRowIndex <= rowColumnIndex.RowIndex)
            {
                while (pressedRowIndex <= rowColumnIndex.RowIndex)
                {
                    GridUnBoundRow undBoundRow = null;
                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                    if (nodeEntry == null && DataGrid.IsUnBoundRow(pressedRowIndex))
                    {
                        undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                        rowData = null;
                    }
                    else
                        rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);
                    columnIndex = pressedColumnIndex;
                    GridCellInfo gridCellInfo = null;
                    if (undBoundRow != null || (rowData != null && !this.DataGrid.IsInDetailsViewIndex(pressedRowIndex)))
                    {
                        if (!needToSelectWholeRow)
                        {
                            if (columnIndex <= rowColumnIndex.ColumnIndex)
                            {
                                while (columnIndex <= rowColumnIndex.ColumnIndex && columnIndex >= this.CurrentCellManager.GetFirstCellIndex())
                                {
                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                    {
                                        if (rowData != null)
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                        else
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                        aItems.Add(gridCellInfo);
                                    }
                                    columnIndex++;
                                }
                            }
                            else
                            {
                                while (columnIndex >= rowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                {
                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                    {
                                        if (rowData != null)
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                        else
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                        aItems.Add(gridCellInfo);
                                    }
                                    columnIndex--;
                                }
                            }
                        }
                        else
                        {
                            this.DataGrid.Columns.ForEach(column =>
                            {
                                if (column.AllowFocus && !column.IsHidden)
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(column, pressedRowIndex, undBoundRow);
                                    aItems.Add(gridCellInfo);
                                }
                            });
                        }
                    }
                    else
                    {
                        gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                        aItems.Add(gridCellInfo);
                    }
                    pressedRowIndex = pressedRowIndex == this.DataGrid.GetLastNavigatingRowIndex() ? ++pressedRowIndex : this.GetNextRowIndex(pressedRowIndex);
                }
            }
            else
            {
                while (pressedRowIndex >= rowColumnIndex.RowIndex)
                {
                    GridUnBoundRow undBoundRow = null;
                    NodeEntry nodeEntry = this.DataGrid.GetNodeEntry(pressedRowIndex);
                    if (nodeEntry == null || DataGrid.IsUnBoundRow(pressedRowIndex))
                    {
                        undBoundRow = DataGrid.GetUnBoundRow(pressedRowIndex);
                        rowData = null;
                    }
                    else
                        rowData = this.DataGrid.GetRecordAtRowIndex(pressedRowIndex);
                    columnIndex = pressedColumnIndex;
                    GridCellInfo gridCellInfo = null;
                    if (undBoundRow != null || (rowData != null && !this.DataGrid.IsInDetailsViewIndex(pressedRowIndex)))
                    {
                        if (!needToSelectWholeRow)
                        {
                            if (columnIndex >= rowColumnIndex.ColumnIndex)
                            {
                                while (columnIndex >= rowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                {
                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                    {
                                        if (rowData != null)
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                        else
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                        aItems.Add(gridCellInfo);
                                    }
                                    columnIndex--;
                                }
                            }
                            else
                            {
                                while (columnIndex <= rowColumnIndex.ColumnIndex && !(columnIndex < this.CurrentCellManager.GetFirstCellIndex()))
                                {
                                    if (this.DataGrid.AllowFocus(new RowColumnIndex(pressedRowIndex, columnIndex)))
                                    {
                                        if (rowData != null)
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), rowData, nodeEntry);
                                        else
                                            gridCellInfo = new GridCellInfo(this.DataGrid.GetGridColumn(columnIndex), pressedRowIndex, undBoundRow);
                                        aItems.Add(gridCellInfo);
                                    }
                                    columnIndex++;
                                }
                            }
                        }
                        else
                        {
                            this.DataGrid.Columns.ForEach(column =>
                            {
                                if (column.AllowFocus && !column.IsHidden)
                                {
                                    if (rowData != null)
                                        gridCellInfo = new GridCellInfo(column, rowData, nodeEntry);
                                    else
                                        gridCellInfo = new GridCellInfo(column, pressedRowIndex, undBoundRow);
                                    aItems.Add(gridCellInfo);
                                }
                            });
                        }
                    }
                    else
                    {
                        gridCellInfo = new GridCellInfo(currentColumn, null, nodeEntry, pressedRowIndex);
                        aItems.Add(gridCellInfo);
                    }
                    pressedRowIndex = pressedRowIndex == this.DataGrid.GetFirstNavigatingRowIndex() ? --pressedRowIndex : this.GetPreviousRowIndex(pressedRowIndex);
                }
            }

            return aItems;
        }
        /// <summary>
        /// Processes the cell selection when the mouse pointer is dragged on the SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains data related to the mouse action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding row and column index to perform drag selection.
        /// </param>
        /// <param name="previousCellIndex">
        /// Contains the previous cell index during drag operation.
        /// </param>
        /// <param name="needToSelectWholeRow">
        /// Indicates whether the whole row is selected while processing the drag selection on cells.
        /// </param>
        protected void ProcessDragSelection(MouseEventArgs args, RowColumnIndex rowColumnIndex, RowColumnIndex previousCellIndex, bool needToSelectWholeRow)
        {           

            isInShiftSelection = true;
            int firstCellIndex = this.CurrentCellManager.GetFirstCellIndex();
            object rowData = this.DataGrid.GetRecordAtRowIndex(rowColumnIndex.RowIndex);
            if (lastSelectedIndex.IsEmpty)
            {                           
                if (PressedRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() || rowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex())
                {
                    var colIndex = previousCellIndex.ColumnIndex < firstCellIndex ? firstCellIndex : previousCellIndex.ColumnIndex;
                    this.SetPressedIndex(new RowColumnIndex(this.PressedRowColumnIndex.RowIndex, colIndex));
                    rowColumnIndex.ColumnIndex = colIndex;
                }              
            }
            else if (rowColumnIndex.ColumnIndex < firstCellIndex || (rowData == null && !this.CurrentCellManager.IsUnBoundRow) || this.DataGrid.IsInDetailsViewIndex(rowColumnIndex.RowIndex))
                rowColumnIndex.ColumnIndex = lastSelectedIndex.ColumnIndex;

            var removedItems = new List<GridCellInfo>();
            var addedItems = new List<GridCellInfo>();

            //WPF-29687 - Previous Cell Selection maintained if Ctrl key pressed while performing a drag selection
            if ((this.DataGrid.SelectionMode != GridSelectionMode.Multiple && !SelectionHelper.CheckControlKeyPressed()) || (this.DataGrid.IsInDetailsViewIndex(this.PressedRowColumnIndex.RowIndex) && !this.dataGrid.AllowSelectionOnPointerPressed))
                removedItems = this.GetSelectedCells();
            else
            {
                //Get the removed items from pressed to last selected row otherwise removed items maintained from all selected rows 
                if (!lastSelectedIndex.IsEmpty)
                    removedItems = this.GetSelectedCells(this.PressedRowColumnIndex, lastSelectedIndex, needToSelectWholeRow);
            }
            
            addedItems = this.GetSelectedCells(this.PressedRowColumnIndex, rowColumnIndex, needToSelectWholeRow);

            List<GridCellInfo> commonItems = addedItems.Intersect(removedItems).ToList();
            var addedItem = addedItems.Except(commonItems).ToList();
            var selectedCells = this.GetSelectedCells();
            var removedItem = removedItems.Except(commonItems).ToList<object>();
            var newAddedItems = addedItem.Except(selectedCells.ToList<GridCellInfo>()).ToList<object>();

            if (removedItem.Count < 1 && newAddedItems.Count < 1)
                return;

            if (this.RaiseSelectionChanging(newAddedItems, removedItem))
                return;

            if (removedItem.Count > 0)
                this.RemoveCells(removedItem);

            if (addedItems.Count > 0)
                this.AddCells(newAddedItems);

            lastSelectedIndex = rowColumnIndex;
            SuspendUpdates();
            this.RefreshSelectedIndex();
            ResumeUpdates();
            RaiseSelectionChanged(newAddedItems, removedItem);
        }
        
        #endregion

    }
}
