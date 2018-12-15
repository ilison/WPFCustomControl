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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using Windows.Foundation;
#else
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif
    /// <summary>
    /// Provides the base implementation for selection in SfTreeGrid.
    /// </summary>
    public abstract class TreeGridBaseSelectionController : ITreeGridSelectionController, IDisposable
    {
        #region Fields
        internal bool isSuspended;
        private SfTreeGrid treeGrid;
        TreeGridCurrentCellManager currentCellManager;
        internal bool isMousePressed = false;
        internal bool isSourceCollectionChanged = false;

        /// <summary>
        /// Gets a value to suspend the selection operations that are performed internally.
        /// </summary>
        /// <value>
        /// <b>true</b> if the selection operations is suspended; otherwise , <b>false</b>.
        /// </value>
        protected bool IsSuspended
        {
            get { return isSuspended; }
        }


        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </value>
        protected internal SfTreeGrid TreeGrid
        {
            get { return treeGrid; }
            protected set { treeGrid = value; }
        }

        /// <summary>
        /// Gets or sets the currently pressed key.
        /// </summary>
        //protected Key CurrentKey = Key.None;

        internal Point pressedPosition;
        private RowColumnIndex _pressedRowColumnIndex;
        /// <summary>
        /// Gets or sets the pressed RowColumnIndex of SfTreeGrid.
        /// </summary>
        protected internal RowColumnIndex PressedRowColumnIndex
        {
            get { return _pressedRowColumnIndex; }
            set { _pressedRowColumnIndex = value; }
        }

#if UWP
        // To skip the selection while scrolling, pressedVerticalOffset is maintained.
        internal double pressedVerticalOffset;
#endif
        #endregion

        /// <summary>
        /// Sets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController.PressedRowColumnIndex"/> of SfTreeGird.
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

        #region ITreeGridSelectionController

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> instance which holds the current cell information.
        /// </summary>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> creates the current cell and manages the current cell related operations.
        /// </remarks>
        public TreeGridCurrentCellManager CurrentCellManager
        {
            get { return currentCellManager; }
            protected set { currentCellManager = value; }
        }

        /// <summary>
        /// Handles selection when any of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperation"/> such as Sorting performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperationsHandlerArgs"/> that contains the type of grid operations and its arguments.
        /// </param>
        /// <remarks>
        /// This method is called when any of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperation"/> are performed in SfTreeGrid.
        /// </remarks>
        public virtual void HandleGridOperations(TreeGridOperationsHandlerArgs args)
        {
            switch (args.Operation)
            {
                case TreeGridOperation.Sorting:
                    ProcessOnSortChanged(args.OperationArgs as SortColumnChangedHandle);
                    break;
                case TreeGridOperation.RowHeaderChanged:
                    ProcessOnRowHeaderChanged();
                    break;
            }
        }

        /// <summary>
        /// Handles selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridPointerEventArgs"/> that contains the type of pointer operations and its arguments.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the cell.
        /// </param>
        /// <remarks>
        /// This method is invoked when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> are performed in SfTreeGrid.
        /// </remarks>
        public virtual void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex)
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
                    this.ProcessOnDoubleTapped(rowColumnIndex);
                    break;

#if WPF
                case PointerOperation.Wheel:
                    this.ProcessPointerWheel(args.OriginalEventArgs as MouseWheelEventArgs, rowColumnIndex);
                    break;
#endif
            }
        }
        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowRowHeader"/> property value changes.
        /// </summary>
        protected virtual void ProcessOnRowHeaderChanged()
        {
            if (this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex >= 0)
            {
                if (this.treeGrid.showRowHeader)
                    this.CurrentCellManager.SetCurrentColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex + 1);
                else
                    this.CurrentCellManager.SetCurrentColumnIndex(this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex - 1);
            }
        }

        /// <summary>
        /// Handles selection when the selection property such as SelectedIndex,SelectedItem and SelectionMode value changes occurred.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> that contains the corresponding property name and its value changes.
        /// </param>
        /// <remarks>
        /// This method is invoked when the selection property values such as SelectedIndex,SelectedItem and SelectionMode are changed in SfTreeGrid.
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
        /// Processes the selection when the node is expanded/collapsed in SfTreeGrid.
        /// </summary>
        /// <param name="NodeOperation">
        /// Contains the value for collection changes during expanding/collapsing action.
        /// </param>
        /// <remarks>
        /// This method refreshes the selection while expanding/collapsing the node in SfTreeGrid.
        /// </remarks>
        protected internal abstract void HandleNodeOperations(NodeOperation NodeOperation);

        /// <summary>
        /// Processes the selection when the column is sorted in SfTreeGrid.
        /// </summary>
        /// <param name="sortColumnHandle">
        /// Contains information related to the sorting action.
        /// </param>        
        /// <remarks>
        /// This method refreshes the selection while sorting the column in SfTreeGrid.
        /// </remarks>
        protected abstract void ProcessOnSortChanged(SortColumnChangedHandle sortColumnHandle);

        /// <summary>
        /// Processes the row selection when the mouse pointer is pressed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the pressed point location.    
        /// </param>
        /// <remarks>
        /// This method will be invoked when the pointer is pressed using touch or mouse in SfTreeGrid.
        /// The selection is initialized in pointer pressed state when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionPointerPressed"/> set as true.
        /// </remarks>
        protected abstract void ProcessPointerPressed(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex);

        /// <summary>
        /// Processes the row selection when the mouse pointer is released from SfTreeGrid. 
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse pointer action.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse released point.
        /// </param>
        /// <remarks>
        /// The selection is initialized in pointer released state when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionPointerPressed"/> set as false.        
        /// </remarks>
        protected abstract void ProcessPointerReleased(MouseButtonEventArgs args, RowColumnIndex rowColumnIndex);

        /// <summary>
        /// Processes the row selection when mouse pointer is moved on the SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains the data for mouse related interaction.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex related to the mouse point.    
        /// </param>
        protected abstract void ProcessPointerMoved(MouseEventArgs args, RowColumnIndex rowColumnIndex);

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
        /// This method invoked to process selection and end edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnTap"/>.
        /// </remarks>
        protected abstract void ProcessOnTapped(TappedEventArgs e, RowColumnIndex currentRowColumnIndex);

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
        /// This method invoked to process selection and end edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>.
        /// </remarks>          
        protected abstract void ProcessOnDoubleTapped(RowColumnIndex currentRowColumnIndex);

#if WPF
        /// <summary>
        /// Processes the cell selection while scrolling the SfTreeGrid using mouse wheel.
        /// </summary>
        /// <param name="mouseButtonEventArgs">
        /// An <see cref="T:System.Windows.Input.MouseButtonEventArgs"/>that contains the event data.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex where the mouse wheel interaction occurs.    
        /// </param>  
        protected abstract void ProcessPointerWheel(MouseWheelEventArgs args, RowColumnIndex rowColumnIndex);
#endif
        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedItem"/> property value changes.
        /// </param>
        protected internal virtual void ProcessSelectedItemChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedIndex"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectedIndex"/> property value changes.
        /// </param>
        protected internal virtual void ProcessSelectedIndexChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionMode"/> property changes.
        /// </param>
        protected virtual void ProcessSelectionModeChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> property value changes.
        /// </param>
        protected virtual void ProcessNavigationModeChanged(SelectionPropertyChangedHandlerArgs handle) { }

        /// <summary>
        /// Processes the selection when the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem"/> property value changes.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> contains the data for the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem"/> property value changes.
        /// </param>
        protected internal virtual void ProcessCurrentItemChanged(SelectionPropertyChangedHandlerArgs handle) { }

        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of <see cref="TreeGridBaseSelectionController"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding <see cref="SfTreeGrid"/> instance.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridBaseSelectionController(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
            SelectedRows = new TreeGridSelectedRowsCollection();
            CurrentCellManager = CreateCurrentCellManager();
        }

        internal TreeGridRowInfo GetTreeGridSelectedRow(object data)
        {
            if (data != null)
            {
                var rowIndex = this.treeGrid.ResolveToRowIndex(data);
                var treeNode = this.treeGrid.View.Nodes.GetNode(data);
                if (treeNode != null)
                    return new TreeGridRowInfo(rowIndex, data, treeNode);
            }
            return null;
        }

        /// <summary>
        /// Gets or sets the last pressed key.
        /// </summary>
        protected Key LastPressedKey = Key.None;

        /// <summary>
        /// Gets the selected row information based on the specified row index.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to get selected row information.
        /// </param>
        /// <returns>
        /// Returns the selected row information.
        /// </returns>
        internal TreeGridRowInfo GetTreeGridSelectedRow(int rowIndex)
        {
            if (rowIndex == -1)
                return null;

            TreeGridRowInfo rowInfo = null;
            var treeNode = this.treeGrid.GetNodeAtRowIndex(rowIndex);
            if (treeNode != null)
            {
                object data = treeNode.Item;
                rowInfo = new TreeGridRowInfo(rowIndex, data, treeNode);
            }
            return rowInfo;
        }

        /// <summary>
        /// Changes the flow direction of key navigation based on the <see cref="Windows.UI.Xaml.FlowDirection"/> in SfTreeGrid.
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

        /// <summary>
        /// Creates the current cell and manages the current cell related operations in SfTreeGrid.
        /// </summary>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> instance.
        /// </returns>
        protected virtual TreeGridCurrentCellManager CreateCurrentCellManager()
        {
            var _currentCellManager = new TreeGridCurrentCellManager(this.treeGrid)
            {
                SuspendUpdates = SuspendUpdates,
                ResumeUpdates = ResumeUpdates
            };
            return _currentCellManager;
        }

        #endregion

        internal virtual void ProcessSelectionOnCheckedStateChange(TreeNode treeNode)
        {

        }

        internal virtual void SelectCheckedNodes()
        {

        }
      

        #region IDisposable

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.treeGrid = null;
                if (this.currentCellManager != null)
                {
                    this.currentCellManager.Dispose();
                    this.currentCellManager = null;
                }

                if (this.selectedRows != null)
                {
                    this.selectedRows.Clear();
                    this.selectedRows = null;
                }
            }
        }

        /// <summary>
        /// Scrolls the specified row index to view in SfTreeGrid.
        /// </summary>
        /// <param name="rowIndex">
        /// The corresponding row index to scroll the row.
        /// </param>       
        /// <remarks>
        /// This method helps to scroll the row into view when the row is not present in the view area of SfTreeGrid.       
        /// </remarks>
        protected void ScrollToRowIndex(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= treeGrid.TreeGridPanel.ScrollRows.LineCount)
                return;

            int firstBodyVisibleIndex = -1;
            var SelectionController = this.treeGrid.SelectionController as TreeGridBaseSelectionController;
            VisibleLineInfo lineInfo = this.treeGrid.TreeGridPanel.ScrollRows.GetVisibleLineAtLineIndex(rowIndex);
            if (lineInfo == null)
            {
                var visibleLines = this.treeGrid.TreeGridPanel.ScrollRows.GetVisibleLines();
                if (visibleLines.FirstBodyVisibleIndex < visibleLines.Count)
                    firstBodyVisibleIndex = visibleLines[visibleLines.FirstBodyVisibleIndex].LineIndex;
            }

            var isInOutOfView = rowIndex > this.treeGrid.TreeGridPanel.ScrollRows.LastBodyVisibleLineIndex + 1 || (firstBodyVisibleIndex >= 0 && rowIndex < firstBodyVisibleIndex - 1);
            if (isInOutOfView || lineInfo == null || (lineInfo != null && lineInfo.IsClipped))
            {
                this.treeGrid.TreeGridPanel.ScrollRows.ScrollInView(rowIndex);
                this.treeGrid.TreeGridPanel.InvalidateMeasureInfo();
            }
        }

        #endregion

        /// <summary>
        /// Handles the selection when the SelectedItems, Columns and DataSource property collection changes.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> that contains data for collection changes.
        /// </param>
        /// <param name="reason">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCollectionChangedReason"/> contains reason for the collection changes.
        /// </param>
        /// <remarks>
        /// This method is called when the collection changes on SelectedItems, Columns and DataSource properties in SfTreeGrid.
        /// </remarks>
        public abstract void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, TreeGridCollectionChangedReason reason);


        /// <summary>
        /// Handles the selection when the node is expanded or collapsed in SfTreeGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the node.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Specifies whether the node is expanded or not.
        /// </param>
        /// <remarks>
        /// This method is invoked when the node is expanded or collapsed.
        /// </remarks>
        public abstract void HandleNodeExpandCollapse(int index, int count, bool isExpanded);

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
        /// Selects all the rows in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public abstract void SelectAll();

        /// <summary>
        /// Clears all the selected rows in SfTreeGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current selection can be cleared or not.
        /// </param>        
        public abstract void ClearSelections(bool exceptCurrentRow);

        /// <summary>
        /// Moves the current cell for the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current selection is cleared while moving the current cell.
        /// </param>     
        /// <remarks>
        /// This method is not applicable when <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NavigationMode"/> is Row.
        /// </remarks>     
        public abstract void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true);

        private int suspendcount = -1;
        // In self-relational mode, If ScrollToUpdatedItem is specified, selection will be retained in committed row. So isSelectionHandled is set as false.
        internal bool isSelectionHandled = false;

        internal void ResetSelectionHandled()
        {
            this.isSelectionHandled = false;
        }

        /// <summary>
        /// Temporarily suspends the updates for the selection operation in SfTreeGrid.
        /// </summary>       
        public void SuspendUpdates()
        {
            suspendcount++;
            this.isSuspended = true;
        }

        /// <summary>
        /// Resumes usual selection operation in SfTreeGrid
        /// </summary>

        public void ResumeUpdates()
        {
            if (suspendcount == 0)
                this.isSuspended = false;
            suspendcount--;
        }

        #region Selection Changing Event Methods

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionChanging"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="addedItems">
        /// Contains the items that were selected.
        /// </param>
        /// <param name="removedItems">
        /// Contains the items that were unselected.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionChanging"/> is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseSelectionChanging(List<object> addedItems, List<object> removedItems)
        {
            var args = new GridSelectionChangingEventArgs(this.treeGrid)
            {
                AddedItems = addedItems,
                RemovedItems = removedItems,
            };
            return this.treeGrid.RaiseSelectionChagingEvent(args);
        }


        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionChanged"/> event.
        /// </summary>
        /// <param name="addedItems">
        /// Contains the items that were selected.
        /// </param>
        /// <param name="removedItems">
        /// Contains the items that were unselected.
        /// </param>
        protected void RaiseSelectionChanged(List<object> addedItems, List<object> removedItems)
        {
            var args = new GridSelectionChangedEventArgs(this.treeGrid)
            {
                AddedItems = addedItems,
                RemovedItems = removedItems,
            };
            this.treeGrid.RaiseSelectionChangedEvent(args);
        }

        /// <summary>
        /// Handles the selection for the keyboard interactions that are performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key should be handled in SfTreeGrid; otherwise, <b>false</b>.
        /// </returns>
        public abstract bool HandleKeyDown(KeyEventArgs args);

        #endregion

        private TreeGridSelectedRowsCollection selectedRows;
        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> that contains the information of selected rows.        
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> .
        /// </value>
        public TreeGridSelectedRowsCollection SelectedRows
        {
            get { return selectedRows; }
            protected set { selectedRows = value; }
        }
    }
}
