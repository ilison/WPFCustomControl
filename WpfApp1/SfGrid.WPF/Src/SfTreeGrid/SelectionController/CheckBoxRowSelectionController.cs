#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Xaml.Input;
#endif
using System.Collections.Specialized;

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
    using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
    using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
    using System.Collections.Specialized;
#else
    using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
    using System.Collections.Specialized;
    using System.Windows.Input;
#endif
    /// <summary>
    /// Represents a class that implements the selection behaviors of SfTreeGrid when <see cref="SfTreeGrid.ShowCheckBox">ShowCheckBox</see> is true/>. 
    /// </summary>
    public class CheckBoxRowSelectionController : TreeGridRowSelectionController
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.CheckBoxRowSelectionController"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> instance.
        /// </param>
        public CheckBoxRowSelectionController(SfTreeGrid treeGrid)
            : base(treeGrid)
        {

        }

        /// <summary>
        /// Process the selection while changing IsChecked state of the particular tree node.
        /// </summary>
        /// <param name="treeNode">treeNode - its IsChecked state is changed.</param>
        internal override void ProcessSelectionOnCheckedStateChange(TreeNode treeNode)
        {
            if (TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            var addedItems = new List<object>();
            var removedItems = new List<object>();
            var isChecked = treeNode.IsChecked;
            var rowIndex = TreeGrid.ResolveToRowIndex(treeNode);
            var currentRowColumnIndex = CurrentCellManager.CurrentRowColumnIndex;
            var rowColumnIndex = new RowColumnIndex(rowIndex, CurrentCellManager.CurrentRowColumnIndex.ColumnIndex);
            if (TreeGrid.SelectionMode == GridSelectionMode.Single)
            {
                if (isChecked == true)
                {
                    if (SelectedRows.Any())
                    {
                        removedItems.Add(SelectedRows[0]);
                    }
                    addedItems.Add(GetTreeGridSelectedRow(rowIndex));

                    if (RaiseSelectionChanging(addedItems, removedItems))
                    {
                        return;
                    }
                    SetCurrentCellAndCurrentRowColumnIndex(rowColumnIndex, currentRowColumnIndex);
                    RemoveSelection(removedItems);
                    if (removedItems.Any())
                        UpdateCheckedStateForSelectionRemovedRow(removedItems[0] as TreeGridRowInfo);
                    AddSelection(addedItems);
                    RaiseSelectionChanged(addedItems, removedItems);
                }
                else
                {
                    if (!SelectedRows.Any())
                        return;
                    if (SelectedRows[0].RowIndex != rowIndex)
                        return;
                    removedItems.Add(SelectedRows[0]);
                    if (RaiseSelectionChanging(addedItems, removedItems))
                    {
                        return;
                    }
                    if (TreeGrid.NavigationMode == NavigationMode.Cell)
                    {
                        CurrentCellManager.RemoveCurrentCell(currentRowColumnIndex);
                    }
                    CurrentCellManager.ResetCurrentRowColumnIndex();

                    RemoveSelection(removedItems);
                    UpdateCheckedStateForSelectionRemovedRow(removedItems[0] as TreeGridRowInfo);
                    this.SuspendUpdates();
                    RefreshSelectedIndexAndItem();
                    this.ResumeUpdates();
                    RaiseSelectionChanged(addedItems, removedItems);
                }
            }
            // Multiple and Extended Cases
            else
            {
                if (TreeGrid.EnableRecursiveChecking)
                {
                    SelectRowsOnRecursiveCheck(treeNode, rowIndex);
                    return;
                }

                if (isChecked == true)
                {
                    addedItems.Add(GetTreeGridSelectedRow(rowIndex));
                    if (RaiseSelectionChanging(addedItems, removedItems))
                    {
                        return;
                    }
                    SetCurrentCellAndCurrentRowColumnIndex(rowColumnIndex, currentRowColumnIndex);
                    AddSelection(addedItems);
                    RaiseSelectionChanged(addedItems, removedItems);
                }
                else
                {
                    if (!SelectedRows.Any())
                        return;
                    var selectedRow = SelectedRows.FirstOrDefault(r => r.RowIndex == rowIndex);
                    if (selectedRow == null)
                        return;
                    removedItems.Add(selectedRow);
                    if (RaiseSelectionChanging(addedItems, removedItems))
                    {
                        return;
                    }
                    var needToSetCurrentCell = false;
                    if (TreeGrid.NavigationMode == NavigationMode.Cell && selectedRow.RowIndex == currentRowColumnIndex.RowIndex)
                    {
                        CurrentCellManager.RemoveCurrentCell(currentRowColumnIndex);
                        needToSetCurrentCell = (SelectedRows.Count > 1);
                    }
                    RemoveSelection(removedItems, needToSetCurrentCell);
                    if (!SelectedRows.Any() || TreeGrid.NavigationMode == NavigationMode.Row)
                        CurrentCellManager.ResetCurrentRowColumnIndex();
                    UpdateCheckedStateForSelectionRemovedRow(removedItems[0] as TreeGridRowInfo);
                    this.SuspendUpdates();
                    RefreshSelectedIndexAndItem();
                    this.ResumeUpdates();
                    RaiseSelectionChanged(addedItems, removedItems);
                }
            }
        }

        /// <summary>
        /// Update the selection based on node's IsChecked state.
        /// </summary>        
        /// <param name="needToResetCurrentCell">specifies whether need to reset current cell or not.</param>
        internal void SelectRowsBasedOnCheckedNodes()
        {
            if (TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            this.SuspendUpdates();
            var addedItems = new List<object>();
            var removedItems = new List<object>();

            foreach (var treeNode in TreeGrid.View.Nodes.RootNodes)
            {
                UpdateSelectedItemsBasedOnChildNodes(treeNode, ref addedItems, ref removedItems);
            }
            UpdateSelection(addedItems, removedItems, -1, false);
            this.ResumeUpdates();
            RaiseSelectionChanged(addedItems, removedItems);
        }

        /// <summary>
        /// Update the selection based on node's IsChecked state.
        /// </summary>   
        internal override void SelectCheckedNodes()
        {
            if (TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            this.SuspendUpdates();
            var addedItems = new List<object>();
            var removedItems = new List<object>();
            foreach (var treeNode in TreeGrid.View.Nodes)
            {              
                if (treeNode.IsChecked == true)
                {
                    if (!TreeGrid.SelectedItems.Contains(treeNode.Item))
                    {
                        var rowIndex = this.TreeGrid.ResolveToRowIndex(treeNode);
                        var rowInfo = this.GetSelectedRow(rowIndex, treeNode.Item, treeNode);
                        if (rowInfo != null)
                        {
                            addedItems.Add(rowInfo);
                        }
                    }
                }
            }
            AddSelection(addedItems);
            if (addedItems.Count > 0)
            {
                this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(((TreeGridRowInfo)addedItems.LastOrDefault()).RowIndex, columnIndex), false);
            }
            this.RefreshSelectedIndexAndItem();
            this.ResumeUpdates();
            RaiseSelectionChanged(addedItems, removedItems);
        }

        /// <summary>
        /// Select/Deselect rows based on IsChecked state when EnableRecursiveChecking is True.
        /// </summary>
        /// <param name="treeNode">specific treeNode.</param>
        /// <param name="currentRowIndex"> specifies the current cell row index.</param>
        /// <param name="needToRemoveItems">whether need to consider the items to be removed from selected items.</param>
        internal void SelectRowsOnRecursiveCheck(TreeNode treeNode, int currentRowIndex = -1, bool needToRemoveItems = true)
        {
            if (TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            this.SuspendUpdates();
            var addedItems = new List<object>();
            var removedItems = new List<object>();
            UpdateSelectedItemsBasedOnParentNode(treeNode, ref addedItems, ref removedItems);
            UpdateSelectedItemsBasedOnChildNodes(treeNode, ref addedItems, ref removedItems, needToRemoveItems);
            UpdateSelection(addedItems, removedItems, currentRowIndex);
            this.ResumeUpdates();
            RaiseSelectionChanged(addedItems, removedItems);
        }

        /// <summary>
        /// Get Items to be selected and items to be removed from selected items based on IsChecked state of specific tree node and its child nodes.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="addedItems">items to be selected.</param>
        /// <param name="removedItems">items to be removed from selected items.</param>
        /// <param name="considerUncheckedNodes">indicates whether need to consider the unchecked nodes</param>
        internal void UpdateSelectedItemsBasedOnChildNodes(TreeNode treeNode, ref List<object> addedItems, ref List<object> removedItems, bool considerUncheckedNodes = true)
        {
            UpdateSelectedItems(treeNode, ref addedItems, ref removedItems, considerUncheckedNodes);
            foreach (var node in treeNode.ChildNodes)
                UpdateSelectedItemsBasedOnChildNodes(node, ref addedItems, ref removedItems, considerUncheckedNodes);
        }

        /// <summary>
        /// Get Items to be selected and items to be removed from selected items based on IsChecked state of specific tree node.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="addedItems">items to be selected.</param>
        /// <param name="removedItems">items to be removed from selected items.</param>
        /// <param name="considerUncheckedNodes">indicates whether need to consider the unchecked nodes</param>
        internal void UpdateSelectedItems(TreeNode treeNode, ref List<object> addedItems, ref List<object> removedItems, bool considerUncheckedNodes = true)
        {           
            if (treeNode.IsChecked == true)
            {
                if (!TreeGrid.SelectedItems.Contains(treeNode.Item))
                {
                    var rowIndex = this.TreeGrid.ResolveToRowIndex(treeNode);
                    var rowInfo = this.GetSelectedRow(rowIndex, treeNode.Item, treeNode);
                    if (rowInfo != null)
                    {
                        addedItems.Add(rowInfo);
                    }
                }
            }
            else if (considerUncheckedNodes)
            {
                if (TreeGrid.SelectedItems.Contains(treeNode.Item))
                {
                    removedItems.Add(this.SelectedRows.FindRowData(treeNode.Item));
                }
            }
        }

        /// <summary>
        /// Get Items to be selected and items to be removed from selected items based on IsChecked state of the parent node of specific tree node.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="addedItems">items to be selected.</param>
        /// <param name="removedItems">items to be removed from selected items.</param>        
        internal void UpdateSelectedItemsBasedOnParentNode(TreeNode treeNode, ref List<object> addedItems, ref List<object> removedItems)
        {
            var parentNode = treeNode.ParentNode;
            if (parentNode == null)
                return;
            UpdateSelectedItems(parentNode, ref addedItems, ref removedItems);
            UpdateSelectedItemsBasedOnParentNode(parentNode, ref addedItems, ref removedItems);
        }

        /// <summary>
        /// Update selection and current cell based on added items and removed items.
        /// </summary>
        /// <param name="addedItems">items to be seletced.</param>
        /// <param name="removedItems">items to be removed from selected items.</param>
        /// <param name="currentRowIndex">row index which needs to be set as current cell.</param>
        /// <param name="needToResetCurrentCell">indicates whether need to reset current cell</param>
        internal void UpdateSelection(List<object> addedItems, List<object> removedItems, int currentRowIndex = -1, bool needToResetCurrentCell = true)
        {
            if (removedItems.Count > 0)
            {
                bool needToSetCurrentCell = false;
                var needToRemoveCurrentCell = false;
                foreach (var row in removedItems)
                {
                    if ((row as TreeGridRowInfo).RowIndex == CurrentCellManager.CurrentRowColumnIndex.RowIndex)
                    {
                        needToRemoveCurrentCell = true;
                        break;
                    }
                }

                if (TreeGrid.NavigationMode == NavigationMode.Cell && needToRemoveCurrentCell)
                {
                    CurrentCellManager.RemoveCurrentCell(CurrentCellManager.CurrentRowColumnIndex);
                    if (addedItems.Count == 0)
                    {
                        if (SelectedRows.Count > removedItems.Count)
                            needToSetCurrentCell = true;
                    }
                }

                this.RemoveSelection(removedItems, needToSetCurrentCell);
            }
            AddSelection(addedItems);
            if (needToResetCurrentCell)
            {
                if (addedItems.Count > 0)
                {
                    this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                    int columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < this.CurrentCellManager.GetFirstCellIndex() ? this.CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
                    if (currentRowIndex == -1)
                    {
                        this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(((TreeGridRowInfo)addedItems.LastOrDefault()).RowIndex, columnIndex), false);
                    }
                    else
                        this.CurrentCellManager.SelectCurrentCell(new RowColumnIndex(currentRowIndex, columnIndex), false);
                }
                else
                {
                    if (!SelectedRows.Any())
                    {
                        this.CurrentCellManager.RemoveCurrentCell(this.CurrentCellManager.CurrentRowColumnIndex);
                        this.CurrentCellManager.UpdateGridProperties(new RowColumnIndex(-1, -1));
                    }
                }
            }
            this.RefreshSelectedIndexAndItem();
        }
        /// <summary>
        /// Processes the selection when the mouse point is double tapped on the particular cell in SfTreeGrid.
        /// </summary>        
        /// <param name="currentRowColumnIndex">
        /// The corresponding rowColumnIndex of the mouse point.
        /// </param>  
        /// <remarks>
        /// This method invoked to process selection and begin edit the cell when <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger"/> is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>.
        /// </remarks>          
        protected override void ProcessOnDoubleTapped(RowColumnIndex currentRowColumnIndex)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None || this.CurrentCellManager.CurrentRowColumnIndex != currentRowColumnIndex || this.TreeGrid.NavigationMode == NavigationMode.Row)
                return;

            if (this.TreeGrid.EditTrigger == EditTrigger.OnDoubleTap)
            {
                if (this.TreeGrid.SelectionMode != GridSelectionMode.Single)
                {
                    if (!SelectedRows.Any(r => r.RowIndex == currentRowColumnIndex.RowIndex))
                    {
                        var rowInfo = GetTreeGridSelectedRow(currentRowColumnIndex.RowIndex);
                        this.AddSelection(new List<object>() { rowInfo });
                        if (TreeGrid.EnableRecursiveChecking)
                            SelectRowsOnRecursiveCheck(rowInfo.Node, needToRemoveItems: false);
                    }
                }
                CurrentCellManager.ProcessOnDoubleTapped();
            }
        }

        /// <summary>
        /// Set IsChecked as False for selection removed row.
        /// </summary>
        /// <param name="rowInfo">specifies the row info. </param>
        private void UpdateCheckedStateForSelectionRemovedRow(TreeGridRowInfo rowInfo)
        {
            var selectionRemovedNode = TreeGrid.View.Nodes.GetNode(rowInfo.RowData);
            if (selectionRemovedNode == null)
                selectionRemovedNode = TreeGrid.View.FindNodefromData(null, rowInfo.RowData);
            if (selectionRemovedNode.IsChecked == null)
                return;
            TreeGrid.NodeCheckBoxController.SuspendAndChangeIsCheckedState(selectionRemovedNode, false);
        }


        internal void SetCurrentCellAndCurrentRowColumnIndex(RowColumnIndex rowColumnIndex, RowColumnIndex previousCurrentRowColumnIndex)
        {
            if (TreeGrid.NavigationMode == NavigationMode.Cell)
            {
                var currentColumnIndex = previousCurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() ? CurrentCellManager.GetFirstCellIndex() : previousCurrentRowColumnIndex.ColumnIndex;
                rowColumnIndex.ColumnIndex = currentColumnIndex;
                CurrentCellManager.UpdateCurrentCell(rowColumnIndex, previousCurrentRowColumnIndex, ActivationTrigger.Program);
            }
            else
            {
                CurrentCellManager.UpdateGridProperties(rowColumnIndex);
            }
        }

        public override void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex)
        {
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                return;
            base.HandlePointerOperations(args, rowColumnIndex);
        }

        protected override bool ProcessSelection(RowColumnIndex rowColumnIndex, RowColumnIndex previousCurrentCellIndex, SelectionReason reason)
        {
            var isSelectionAdded = base.ProcessSelection(rowColumnIndex, previousCurrentCellIndex, reason);
            if (isSelectionAdded)
                SelectRowsBasedOnCheckedNodes();
            return isSelectionAdded;
        }

        internal override void ProcessShiftSelection(RowColumnIndex newRowColumnIndex, RowColumnIndex previousRowColumnIndex, SelectionReason reason, Key key)
        {
            base.ProcessShiftSelection(newRowColumnIndex, previousRowColumnIndex, reason, key);
            SelectRowsBasedOnCheckedNodes();
        }

        public override bool HandleKeyDown(KeyEventArgs args)
        {
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                return args.Handled;
            base.HandleKeyDown(args);
            return args.Handled;
        }

        protected internal override void RefreshSelectedItems()
        {
            if (!this.TreeGrid.HasView)
                return;

            this.SuspendUpdates();
            int index = this.TreeGrid.SelectedItems.Count - 1;
            while (index >= 0)
            {
                object item = this.TreeGrid.SelectedItems[index];
                bool isAvailable = false;
                var treeNode = this.TreeGrid.View.Nodes.GetNode(item);
                if (treeNode != null)
                {
                    isAvailable = true;
                }
                if (!isAvailable)
                {
                    if (this.CurrentCellManager.CurrentCell != null && this.CurrentCellManager.CurrentCell.IsEditing)
                    {
                        this.CurrentCellManager.EndEdit();
                        this.ResetSelectionHandled();
                    }
                    treeNode = TreeGrid.View.FindNodefromData(null, item);
                    if (treeNode == null)
                        this.TreeGrid.SelectedItems.Remove(item);
                }
                index--;
            }

            this.SelectedRows.Clear();
            index = 0;
            while (index < this.TreeGrid.SelectedItems.Count)
            {
                var rowIndex = this.TreeGrid.ResolveToRowIndex(this.TreeGrid.SelectedItems[index]);
                var node = this.TreeGrid.GetNodeAtRowIndex(rowIndex);
                // If node is not in view, get node using below code.
                if (node == null)
                    node = TreeGrid.View.FindNodefromData(null, this.TreeGrid.SelectedItems[index]);
                this.SelectedRows.Add(this.GetSelectedRow(rowIndex, this.TreeGrid.SelectedItems[index], node));
                index++;
            }
            RefreshSelectedIndexAndItem();
            this.ResumeUpdates();
        }

        internal TreeGridRowInfo GetSelectedRow(int rowIndex, object data, TreeNode treeNode)
        {
            TreeGridRowInfo rowInfo = new TreeGridRowInfo(rowIndex, data, treeNode);
            return rowInfo;
        }

        protected override void ProcessNodeExpanded(int insertIndex, int count)
        {
            base.ProcessNodeExpanded(insertIndex, count);
            SelectRowsBasedOnCheckedNodes();
            UpdateCurrentRowColumnIndexOnNodeExpandAndCollapse();
        }

        protected override void ProcessNodeCollapsed(int removeAtIndex, int count)
        {
            base.ProcessNodeCollapsed(removeAtIndex, count);
            UpdateCurrentRowColumnIndexOnNodeExpandAndCollapse();
        }

        internal override void RefreshSelectedIndexAndItem()
        {
            this.TreeGrid.SelectedIndex = this.SelectedRows.Count > 0 ? this.TreeGrid.ResolveToNodeIndex(this.SelectedRows[0].RowIndex) : -1;
            this.TreeGrid.SelectedItem = this.TreeGrid.SelectedItems.Count > 0 && TreeGrid.SelectedIndex != -1 ? this.TreeGrid.SelectedItems[0] : null;
        }

        internal override void ResetSelectedRows(bool handleSelection = false, bool canFocusGrid = false)
        {
            base.ResetSelectedRows(handleSelection, canFocusGrid);
            SelectRowsBasedOnCheckedNodes();
            UpdateCurrentRowColumnIndexOnNodeExpandAndCollapse();
        }

        protected internal override void HandleNodeOperations(NodeOperation NodeOpeation)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            base.HandleNodeOperations(NodeOpeation);
            if (NodeOpeation == NodeOperation.NodesExpanded)
                SelectRowsBasedOnCheckedNodes();
            UpdateCurrentRowColumnIndexOnNodeExpandAndCollapse();
        }
        internal void UpdateCurrentRowColumnIndexOnNodeExpandAndCollapse()
        {
            if (!SelectedRows.Any(r => r.RowIndex != -1) || this.CurrentCellManager.CurrentRowColumnIndex.RowIndex != -1)
                return;
            var columnIndex = this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex < CurrentCellManager.GetFirstCellIndex() ? CurrentCellManager.GetFirstCellIndex() : this.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;
            var currentRowColumnIndex = new RowColumnIndex(SelectedRows.LastOrDefault(r => r.RowIndex != -1).RowIndex, columnIndex);
            SetCurrentCellAndCurrentRowColumnIndex(currentRowColumnIndex, CurrentCellManager.CurrentRowColumnIndex);
        }

        public override void SelectRows(int startRowIndex, int endRowIndex)
        {
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                throw new InvalidOperationException("It is not possible to select rows programmatically when CheckBoxSelectionMode is SelectOnlyOnCheckBoxClick");
            if (TreeGrid.EnableRecursiveChecking)
                throw new InvalidOperationException("It is not possible to select rows programmatically using SelectRows method when EnableRecursiveChecking is True");
            base.SelectRows(startRowIndex, endRowIndex);
        }

        public override void SelectAll()
        {
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                throw new InvalidOperationException("It is not possible to select rows programmatically when CheckBoxSelectionMode is SelectOnlyOnCheckBoxClick");
            base.SelectAll();
        }

        public override void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true)
        {
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                throw new InvalidOperationException("It is not possible to move current cell programmatically when CheckBoxSelectionMode is SelectOnlyOnCheckBoxClick");
            base.MoveCurrentCell(rowColumnIndex, needToClearSelection);
        }

        public override void HandleSelectionPropertyChanges(SelectionPropertyChangedHandlerArgs handle)
        {
            if (IsSuspended || (handle.NewValue == null && handle.OldValue == null))
                return;
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck && (handle.PropertyName == "SelectedItem" || handle.PropertyName == "SelectedIndex" || handle.PropertyName == "CurrentItem"))
                throw new InvalidOperationException("It is not possible to set selection using SelectedItem/SelectedIndex/CurrentItem programmatically when CheckBoxSelectionMode is SelectOnlyOnCheckBoxClick");
            base.HandleSelectionPropertyChanges(handle);
        }

        protected override void ProcessSelectedItemsChanged(NotifyCollectionChangedEventArgs e)
        {
            if (IsSuspended || this.TreeGrid.SelectionMode == GridSelectionMode.None)
                return;
            if (TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.SelectOnCheck)
                throw new InvalidOperationException("It is not possible to add selected items when CheckBoxSelectionMode is SelectOnlyOnCheckBoxClick");
            base.ProcessSelectedItemsChanged(e);
        }

        internal override void HideAllRowSelectionBorder(bool exceptCurrentRow)
        {
            base.HideAllRowSelectionBorder(exceptCurrentRow);
            TreeGrid.NodeCheckBoxController.ResetIsCheckedState(exceptCurrentRow);
        }
        internal override void ShowAllRowSelectionBorder()
        {
            base.ShowAllRowSelectionBorder();
            TreeGrid.NodeCheckBoxController.SetIsCheckedStateForAllNodes();
        }
    }
}
