#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.Grid;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridUnboundView : TreeGridView
    {
        /// <summary>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> class.
        /// </summary>
        protected SfTreeGrid TreeGrid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridUnboundView(SfTreeGrid treeGrid)
            : base()
        {
            this.TreeGrid = treeGrid;
            propertyAccessProvider = CreateItemPropertiesProvider();
        }

        protected override IPropertyAccessProvider CreateItemPropertiesProvider()
        {
            return new TreeGridPropertiesProvider(TreeGrid, this);
        }

        public override void AttachTreeView(object treeGrid)
        {
            if (!(treeGrid is SfTreeGrid))
                throw new InvalidOperationException("Attached view is not type of SfTreeGrid");
            this.TreeGrid = treeGrid as SfTreeGrid;
        }

        public override void DetachTreeView()
        {
            TreeGrid = null;
        }

        public override SortColumnDescriptions SortDescriptions
        {
            get
            {
                return TreeGrid.SortColumnDescriptions;
            }
        }

        public override SortComparers SortComparers
        {
            get
            {
                return TreeGrid.SortComparers;
            }
        }

        internal override void UpdateExpander(TreeNode node)
        {
            TreeGrid.TreeGridModel.UpdateExpander(node);
        }


        internal override object GetFirstItem()
        {
            try
            {
                if (firstItem == null)
                {
                    var enumerator = this.Nodes.GetEnumerator();
                    if (enumerator == null || !enumerator.MoveNext())
                        return null;

                    firstItem = (enumerator.Current as TreeNode).Item;
                    return firstItem;
                }
                return firstItem;
            }
            catch
            {
                return null;
            }
        }

        internal override void RequestTreeItems(TreeGridRequestTreeItemsEventArgs e)
        {
            bool canPopualte = (e.ParentNode != null && e.ParentNode.ChildNodes.Any() && e.ResetChildAndRepopulate) || (e.ParentNode != null && !e.ParentNode.ChildNodes.Any()) || e.ParentNode == null;
            if (!canPopualte)
            {
                if (e.ParentNode != null && SortDescriptions.Any())
                    e.ParentNode.ChildNodes.Sort(SortDescriptions, SortComparers);
                return;
            }

            this.TreeGrid.RaiseRequestTreeItemsEvent(e);
            if (e.CanAddChildNode)
            {
                PopulateTreeNodes(e);
                if (e.ChildItems != null)
                    WireNotifyPropertyChange(e.ChildItems);
                if (this.SortDescriptions.Any())
                {
                    SortNodes(e.ParentNode);
                }
            }
        }

        private void WireNotifyPropertyChange(IEnumerable list)
        {
            AddNotifyListeners(list);
        }
        protected internal override void UpdateNodesOnPropertyChange(object sender, PropertyChangedEventArgs e, TreeNode treeNode = null)
        {
            TreeGrid.SelectionController.SuspendUpdates();
            if (treeNode == null)
                treeNode = FindNodefromData(null, sender);
            if (!suspendIsCheckedUpdate)
            {
                UpdateNodesOnCheckBoxPropertyChange(sender, e, treeNode);
                TreeGrid.SelectionController.ProcessSelectionOnCheckedStateChange(treeNode);
            }
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows();
        }

        internal override void ValidateCheckBoxPropertyValue(TreeNodes nodes)
        {
            TreeGrid.NodeCheckBoxController.ValidateCheckBoxPropertyValue(nodes);
        }

        internal override void UpdateParentNode(TreeNode node)
        {
            TreeGrid.TreeGridModel.UpdateParentNode(node);
        }

        /// <summary>
        /// Add the data to the child collection of particular tree node.
        /// </summary>
        /// <param name="node">The parent node. this is null if root node needs to be added.</param>
        /// <param name="data">the data.</param>
        public void AddNode(TreeNode node, object data)
        {
            SuspendNodeCollectionEvent();
            var count = node != null ? node.ChildNodes.Count : Nodes.RootNodes.Count;
            AddNode(node, data, count);
            ResumeNodeCollectionEvent();
        }

        /// <summary>
        /// Insert the data into the child collection of particular tree node at the specified index.
        /// </summary>
        /// <param name="node">The parent node. this is null if root node needs to be inserted.</param>
        /// <param name="data">the data.</param>
        /// /// <param name="index">the index.</param>
        public void InsertNode(TreeNode node, object data, int index)
        {
            SuspendNodeCollectionEvent();
            AddNode(node, data, index);
            ResumeNodeCollectionEvent();
        }

        /// <summary>
        /// Remove the data from the child collection of particular tree node.
        /// </summary>
        /// <param name="node">The parent node. this is null if root node needs to be removed.</param>
        /// <param name="data">the data.</param>
        protected internal override void RemoveNode(TreeNode node, object data)
        {
            SuspendNodeCollectionEvent();
            base.RemoveNode(node, data);
            ResumeNodeCollectionEvent();
        }

        /// <summary>
        /// Clear child nodes of the particular node.
        /// </summary>
        /// <param name="node">The parent node. this is null if root nodes need to be cleared.</param>
        protected override void ResetNodes(TreeNode node)
        {
            base.ResetNodes(node, true);
        }

        /// <summary>
        /// Moves the child node from an index to the another index in child nodes of particular tree node.
        /// </summary>
        /// <param name="node">The parent node. this is null if root node needs to be moved.</param>
        /// <param name="oldIndex">the oldIndex.</param>
        /// <param name="newIndex">the newIndex.</param>
        protected internal override void MoveNode(TreeNode node, int oldStartingIndex, int newStartingIndex)
        {
            base.MoveNode(node, oldStartingIndex, newStartingIndex);
        }

        /// <summary>
        /// Replaces the node at specified index with the data in child nodes of the particular tree node. 
        /// </summary>
        /// <param name="node">The parent node. this is null if root node needs to be replaced.</param>
        /// <param name="data">the data.</param>
        /// <param name="index">the index.</param>
        public void ReplaceNode(TreeNode node, object data, int index)
        {
            var list = new List<object>();
            list.Add(data);
            var oldItemList = new List<object>();
            if (node != null)
                oldItemList.Add(node.ChildNodes[index].Item);
            else
                oldItemList.Add(Nodes.RootNodes[index].Item);
            SuspendNodeCollectionEvent();
            base.ReplaceNode(node, oldItemList, list, index);
            ResumeNodeCollectionEvent();
        }
    }
}
