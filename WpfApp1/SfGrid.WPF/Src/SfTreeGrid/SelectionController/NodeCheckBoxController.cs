#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Extensions;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Provides implementation for CheckBox selection.
    /// </summary>
    internal class NodeCheckBoxController : IDisposable
    {
        private SfTreeGrid treeGrid;

        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeCheckBoxController"/> class.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        public NodeCheckBoxController(SfTreeGrid treeGrid)
        {
            this.treeGrid = treeGrid;
        }

        #endregion


        /// <summary>
        /// Change node's IsChecked state based on CheckBox's IsChecked value.
        /// </summary>
        /// <param name="treeNode">specifies the treeNode. </param>
        /// <param name="isChecked">isChecked value.</param>
        internal void ChangeNodeState(TreeNode treeNode, bool? isChecked)
        {
            if (treeGrid.CheckBoxSelectionMode != CheckBoxSelectionMode.Default)
            {
                if (treeGrid.SelectionMode == GridSelectionMode.Single)
                    this.SuspendAndChangeIsCheckedState(treeNode, isChecked);
                else
                    this.SetIsCheckedState(treeNode, isChecked);

                treeGrid.RaiseNodeCheckStateChanged(new NodeCheckStateChangedEventArgs() { Node = treeNode });
                treeGrid.SelectionController.ProcessSelectionOnCheckedStateChange(treeNode);
            }
            else
            {
                SetIsCheckedState(treeNode, isChecked);
                treeGrid.RaiseNodeCheckStateChanged(new NodeCheckStateChangedEventArgs() { Node = treeNode });
            }
            treeNode.isCheckedChanged = true;
        }

        /// <summary>
        /// Validate SelectionMode and CheckBoxSelectionMode.
        /// </summary>
        internal void ValidateCheckBoxSelectionMode()
        {
            if (treeGrid.CheckBoxSelectionMode != CheckBoxSelectionMode.Default && treeGrid.SelectionMode == GridSelectionMode.Single && treeGrid.EnableRecursiveChecking)
                throw new InvalidOperationException("CheckBox selection can not be synchronized when selection mode is single and EnableRecursiveChecking is True");
        }

        /// <summary>
        /// Validates whether CheckBoxPropertyName exists in itemproperties and checks its type.
        /// </summary>
        internal void ValidateCheckBoxPropertyName()
        {
            var isCheckBoxPropertyValid = CheckPropertyNameInItemproperties();
            if (!isCheckBoxPropertyValid)
            {
                throw new Exception("CheckBoxPropertyName must be bool or nullable bool type");
            }         
        }

        internal bool CheckPropertyNameInItemproperties()
        {
            if (string.IsNullOrEmpty(treeGrid.CheckBoxMappingName))
                return true;
            var data = treeGrid.View.Nodes[0].Item;
            var type = treeGrid.View.GetPropertyType(data, treeGrid.CheckBoxMappingName);
            if (type == typeof(bool?) || type == typeof(bool))
                return true;
            else
                return false;
        }

        /// <summary>
        /// Change node's IsChecked state by suspending IsChecked update.
        /// </summary>
        /// <param name="treeNode">specifies the tree node.</param>
        /// <param name="isChecked">isChecked value.</param>
        internal void SuspendAndChangeIsCheckedState(TreeNode treeNode, bool? isChecked)
        {
            treeGrid.View.suspendIsCheckedRecursiveUpdate = true;
            SetIsCheckedState(treeNode, isChecked);
            treeGrid.View.suspendIsCheckedRecursiveUpdate = false;
        }

        /// <summary>
        /// Set node's IsChecked state.
        /// </summary>
        /// <param name="treeNode">specifies the tree node.</param>
        /// <param name="isChecked">isChecked value.</param>
        internal void SetIsCheckedState(TreeNode treeNode, bool? isChecked)
        {
            treeNode.IsChecked = isChecked;
        }

        /// <summary>
        /// Resets the node's IsChecked state.
        /// </summary>
        /// <param name="exceptCurrentRow">specifies whether need to reset current node IsChecked state.</param>
        internal void ResetIsCheckedState(bool exceptCurrentRow)
        {
            var nodes = treeGrid.GetCheckedNodes(true);
            if (nodes == null || !nodes.Any())
                return;
            treeGrid.View.suspendIsCheckedRecursiveUpdate = true;
            if (!exceptCurrentRow)
            {
                foreach (var node in nodes)
                {
                    SetIsCheckedState(node, false);
                }
            }
            else
            {
                if (treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex != -1)
                {
                    var currentNode = treeGrid.GetNodeAtRowIndex(treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
                    foreach (var node in nodes)
                    {
                        if (node != currentNode)
                            SetIsCheckedState(node, false);
                    }
                }
            }
            treeGrid.View.suspendIsCheckedRecursiveUpdate = false;
        }


        /// <summary>
        /// Set all the nodes IsChecked state as True.
        /// </summary>
        internal void SetIsCheckedStateForAllNodes()
        {
            treeGrid.View.suspendIsCheckedRecursiveUpdate = true;

            foreach (var node in treeGrid.View.Nodes)
            {
                SetIsCheckedState(node, true);
            }
            treeGrid.View.suspendIsCheckedRecursiveUpdate = false;
        }


        /// <summary>
        /// Validates the CheckBoxProperty value by considering selection mode.
        /// </summary>
        /// <param name="nodes">specifies the tree node collection.</param>
        internal void ValidateCheckBoxPropertyValue(TreeNodes nodes)
        {
            if (string.IsNullOrEmpty(treeGrid.CheckBoxMappingName))
                return;
            if (treeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.Default)
                return;

            var list = nodes.Where(i => i.IsChecked == true);
            if (treeGrid.SelectionMode == GridSelectionMode.Single)
            {
                if (list.Count() > 1)
                    throw new Exception("Not possible to have IsChecked as true for more than one item when selection mode is single");
                if (list.Any() && treeGrid.SelectionController.SelectedRows.Any(r => r.Node != list.FirstOrDefault()))
                    throw new Exception("Not possible to have IsChecked as true for more than one item when selection mode is single");
            }
            else if (treeGrid.SelectionMode == GridSelectionMode.None)
            {
                if (list.Any())
                    throw new Exception("Not possible to have IsChecked as true for more than one item when selection mode is None");
            }
        }

        /// <summary>
        /// Get the checked nodes in treeGrid.
        /// </summary>
        /// <param name="includeAllNodes">which specifies whether need to include all the nodes which are not in view.</param>
        /// <returns>collection of checked tree nodes.</returns>
        internal ObservableCollection<TreeNode> GetCheckedNodes(bool includeAllNodes = false)
        {
            if (treeGrid.View == null)
                return null;
            var checkedNodes = new ObservableCollection<TreeNode>();
            if (includeAllNodes)
            {
                foreach (var node in treeGrid.View.Nodes.RootNodes)
                {
                    if (node.IsChecked == true)
                    {
                        checkedNodes.Add(node);
                    }
                    GetCheckedChildNodes(node, ref checkedNodes);
                }
            }
            else
            {
                foreach (var node in treeGrid.View.Nodes)
                {
                    if (node.IsChecked == true)
                        checkedNodes.Add(node);
                }
            }
            return checkedNodes;
        }

        private void GetCheckedChildNodes(TreeNode node, ref ObservableCollection<TreeNode> checkedNodes)
        {
            foreach (var childNode in node.ChildNodes)
            {
                if (childNode.IsChecked == true)
                    checkedNodes.Add(childNode);
                GetCheckedChildNodes(childNode, ref checkedNodes);
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeCheckBoxController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeCheckBoxController"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            treeGrid = null;
        }

    }
}
