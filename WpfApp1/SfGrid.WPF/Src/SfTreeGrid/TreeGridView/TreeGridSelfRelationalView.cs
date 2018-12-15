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
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if UWP
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridSelfRelationalView : TreeGridQueryableView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridSelfRelationalView(IEnumerable source, SfTreeGrid treeGrid)
            : base(source, treeGrid)
        {
            propertyAccessProvider = CreateItemPropertiesProvider();
            childPropertyName = treeGrid.ChildPropertyName;
            parentPropertyName = treeGrid.ParentPropertyName;
            SelfRelationUpdateMode = treeGrid.SelfRelationUpdateMode;
            if (!string.IsNullOrEmpty(childPropertyName) && !string.IsNullOrEmpty(parentPropertyName))
            {
                CheckPrimaryKey();
                if (!IsParentPropertyAndChildPropertyValid())
                    throw new Exception("Parent property or Child property is invalid");
            }
            // while disposing, from SourceCollection itself we will unwire property changed event.so no need to unwire from tree node.
            needToUnwireListener = false;
        }

        internal string parentPropertyName;
        internal string childPropertyName;

        private SelfRelationUpdateMode selfRelationUpdateMode;

        /// <summary>
        /// Gets or sets a value that indicates how nodes should be arranged while changing  ChildPropertyName and  ParentPropertyName in Self Relational mode.
        /// </summary>
        public SelfRelationUpdateMode SelfRelationUpdateMode
        {
            get { return selfRelationUpdateMode; }
            internal set { selfRelationUpdateMode = value; }
        }

        /// <summary>
        /// Find whether ParentPropertyName has unique value
        /// </summary>
        /// <returns>returns true if duplicate items present</returns>
        internal bool IsParentPropertyUnique()
        {
            var collection = SourceCollection.ToList<object>();
            var list = collection.Select(i => propertyAccessProvider.GetValue(i, parentPropertyName)).Distinct();
            var count = list.Count();
            if (count != collection.Count())
                return false;
            return true;
        }


        internal bool IsParentPropertyAndChildPropertyValid()
        {
            var itemProperties = GetItemProperties();
            if (itemProperties == null)
                return false;
            if (!IsDynamicBound)
            {
                var childProperyDescriptor = itemProperties.GetPropertyDescriptor(childPropertyName);
                if (childProperyDescriptor == null)
                    return false;
                var parentProperyDescriptor = itemProperties.GetPropertyDescriptor(parentPropertyName);
                if (parentProperyDescriptor == null)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Find ParentItem from record.
        /// </summary>
        /// <param name="record">record.</param>
        /// <returns>parent record.</returns>
        internal object FindParentItemFromRecord(object record)
        {
            var childValue = propertyAccessProvider.GetValue(record, childPropertyName);
            return FindParentItem(childValue);
        }
        internal void AddNodes(IList newItems)
        {
            CheckPrimaryKey();
            // SelfRelationRoot value - Root nodes

            if (SelfRelationRootValue != DependencyProperty.UnsetValue)
            {
                var parentID = SelfRelationRootValue;
                foreach (var item in newItems)
                {
                    object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                    if (IsEqual(parentID, childValue, true))
                    {
                        AddNode(null, item, Nodes.RootNodes.Count);
                    }
                    else
                        AddNodesUnderRootNodes(item);
                }
            }
            else
            {
                foreach (var item in newItems)
                {
                    object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                    // to be added as root
                    if (!HasRecord(item, childValue))
                    {
                        object parentValue = propertyAccessProvider.GetValue(item, parentPropertyName);
                        AddNode(null, item, Nodes.RootNodes.Count);
                        var rootNode = Nodes.RootNodes.GetNode(item);
                        //When parent and child value are same, need to remove the nodes having child property value equal to new parent property value.
                        if (IsEqual(parentValue, childValue))
                            RemoveNodesHavingChildPropertyEqualsToNewParentProperty(parentValue, rootNode);
                    }
                    else
                        AddNodesUnderRootNodes(item);
                }
            }
        }

        internal bool AddRootNodesFromRemovedParent(TreeNode treeNode, bool reset = true)
        {
            bool isAdded = false;
            List<TreeNode> changedItems = new List<TreeNode>();
            if (treeNode != null && (treeNode.IsExpanded || treeNode.ChildNodes.Any()))
            {
                var nodes = CopyTreeNodes(treeNode.ChildNodes);
                if (reset)
                    ResetNodes(treeNode, false);
                AddTreeNodes(null, nodes);
                if (nodes.Any() && treeNode.IsExpanded)
                    isAdded = true;
            }
            else
            {
                var childItems = GetChildSourceFromParentID(DependencyProperty.UnsetValue, null);
                foreach (var childItem in childItems)
                {
                    if (!Nodes.RootNodes.sourceList.Contains(childItem))
                    {
                        AddNode(null, childItem, Nodes.RootNodes.Count);
                        isAdded = true;
                    }
                }
            }
            return isAdded;
        }

        private TreeNodes CopyTreeNodes(TreeNodes treeNodes)
        {
            var nodes = new TreeNodes();
            nodes.nodeList = treeNodes.nodeList.ToList();
            nodes.sourceList = treeNodes.sourceList.ToList();
            return nodes;
        }
        private void AddNodesUnderRootNodes(object item)
        {
            TreeNode parentNode = null;

            if (SortDescriptions.Count == 1 && SortDescriptions.Any(desc => desc.ColumnName == parentPropertyName))
            {
                object newAddItem = null;
                object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                var itemType = this.SourceCollection.GetItemType(true);
                if (itemType != null)
                {
                    if (itemType.IsInterface() || itemType.IsAbstract())
                        newAddItem = null;
                    else
                        newAddItem = itemType.CreateNew();
                    propertyAccessProvider.SetValue(newAddItem, parentPropertyName, childValue);
                }
                var tempNode = new TreeNode() { Item = newAddItem };
                parentNode = FindParentNode(this.Nodes.RootNodes, tempNode);
            }
            else
            {
                object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                parentNode = FindParentNode(this.Nodes.RootNodes, childValue);
            }
            if (parentNode == null)
                return;

            if (parentNode.IsExpanded || parentNode.ChildNodes.Any())
            {
                AddNode(parentNode, item, parentNode.ChildNodes.Count);
            }
            else
            {
                if (!parentNode.HasVisibleChildNodes)
                {
                    // If FilterLevel is Root, no need to update HasVisibleChildNodes based on filtering.
                    if (FilterLevel != FilterLevel.Root)
                        UpdateHasVisibleChildNodesBasedOnChildItem(parentNode, item);
                }
            }
            parentNode.HasChildNodes = true;
            ChangeParentNodeExpander(parentNode);
        }

        internal void AddTreeNodes(TreeNode parentNode, TreeNodes nodes)
        {
            foreach (var node in nodes)
                AddNode(parentNode, node, parentNode == null ? Nodes.RootNodes.Count : parentNode.ChildNodes.Count);
        }

        internal override void RequestTreeItems(TreeGridRequestTreeItemsEventArgs e)
        {
            bool canPopualate = (e.ParentNode != null && e.ParentNode.ChildNodes.Any() && e.ResetChildAndRepopulate) || (e.ParentNode != null && !e.ParentNode.ChildNodes.Any()) || e.ParentNode == null;
            if (!canPopualate)
            {
                if (e.ParentNode != null)
                {
                    if (SortDescriptions.Any())
                        e.ParentNode.ChildNodes.Sort(SortDescriptions, SortComparers);
                }
                return;
            }

            if (e.ParentItem == null)
            {
                e.ChildItems = GetChildSourceFromParentID(SelfRelationRootValue, e.ParentItem, true);
            }
            else
            {
                object parentValue = propertyAccessProvider.GetValue(e.ParentItem, parentPropertyName);
                e.ChildItems = GetChildSourceFromParentID(parentValue, e.ParentItem, e.CanAddChildNode);
            }

            if (e.CanAddChildNode)
            {
                PopulateTreeNodes(e);
                if (this.SortDescriptions.Any())
                {
                    SortNodes(e.ParentNode);
                }
            }
        }

        internal void ChangeParentNodeExpander(TreeNode node)
        {
            var rowIndex = TreeGrid.ResolveToRowIndex(node);
            TreeGrid.UpdateDataRow(rowIndex);
        }

        /// <summary>
        /// Expand nodes to get original parent node
        /// </summary>
        /// <param name="treeNode">Visible parent node</param>
        /// <param name="parentItem">parentItem</param>
        /// <param name="parentItemIndex">parentItemIndex in parentItemCollection</param>
        /// <returns>parent node</returns>
        internal TreeNode ExpandNodeRecursively(TreeNode treeNode, object parentItem, int parentItemIndex)
        {
            TreeNode parentNode = null;
            if (parentItemIndex < 0)
            {
                foreach (var node in treeNode.ChildNodes)
                {
                    if (node.Item == parentItem)
                        return node;
                }
            }
            foreach (var childNode in treeNode.ChildNodes)
            {
                if (childNode.Item == parentItemCollection[parentItemIndex])
                {
                    TreeGrid.ExpandNode(childNode);
                    if (parentItemIndex == 0)
                    {
                        foreach (var node in childNode.ChildNodes)
                        {
                            if (node.Item == parentItem)
                                return node;
                        }
                        break;
                    }
                    parentNode = ExpandNodeRecursively(childNode, parentItem, parentItemIndex - 1);
                    break;
                }
            }
            return parentNode;
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsInSuspend)
                return;

            TreeGrid.SelectionController.SuspendUpdates();
            SuspendNodeCollectionEvent();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        AddNodes(e.NewItems);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        RemoveNodes(e.OldItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        ResetNodes(null);
                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    {
                        throw new NotImplementedException("Move case is not implemented. Need to remove and add item separately");
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        CheckPrimaryKey();
                        RemoveNodes(e.OldItems);

                        object parentValue = propertyAccessProvider.GetValue(e.NewItems[0], parentPropertyName);
                        object childValue = propertyAccessProvider.GetValue(e.NewItems[0], childPropertyName);

                        // in remove method itself, new root nodes will be added. so skipped this
                        var node = this.Nodes.RootNodes.GetNode(e.NewItems[0]);
                        if (node == null)
                            AddNodes(e.NewItems);

                        if (IsEqual(parentValue, childValue))
                        {
                            if (node == null)
                                node = this.Nodes.RootNodes.GetNode(e.NewItems[0]);
                            //When parent and child value are same, need to remove the nodes having child property value equal to new parent property value.
                            if (SelfRelationRootValue == DependencyProperty.UnsetValue)
                                RemoveNodesHavingChildPropertyEqualsToNewParentProperty(parentValue, node);
                        }
                        else
                            RemoveNodesHavingChildPropertyEqualsToNewParentProperty(parentValue, null);
                        break;
                    }
            }
            ResumeNodeCollectionEvent();
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows();
        }

        protected internal override void UpdateNodesOnPropertyChange(object sender, PropertyChangedEventArgs e, TreeNode treeNode = null)
        {
            if (TreeGrid.ItemsSource == null)
                return;
            TreeGrid.SelectionController.SuspendUpdates();
            if (treeNode == null)
                treeNode = FindNodefromData(null, sender);
            else
                treeNode = treeNode.ChildNodes.GetNode(sender);
            if (e == null || e.PropertyName == parentPropertyName)
            {
                CheckPrimaryKey();
                UpdateNodesOnParentPropertyChange(sender, treeNode, e);
            }

            if (e == null || e.PropertyName == childPropertyName)
            {
                UpdateNodesOnChildPropertyChange(sender, treeNode, e);
            }

            if (treeNode != null && (e == null || e.PropertyName == CheckBoxMappingName) && !suspendIsCheckedUpdate)
            {
                UpdateNodesOnCheckBoxPropertyChange(sender, e, treeNode);
                TreeGrid.SelectionController.ProcessSelectionOnCheckedStateChange(treeNode);
            }

            TreeGrid.SelectionController.ResumeUpdates();
            if (isNodeCollectionChanged)
            {
                if (e == null && (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit) && this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.ScrollToUpdatedItem)))
                    (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows(true);
                else
                    (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows();
            }
            isNodeCollectionChanged = false;
        }

        private void UpdateNodesOnChildPropertyChange(object sender, TreeNode treeNode, PropertyChangedEventArgs e)
        {
            if (SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.None))
                return;
            TreeNode newParentNode = null;
            var childValue = propertyAccessProvider.GetValue(sender, childPropertyName);
            if (SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnPropertyChange))
            {
                if (treeNode == null)
                {
                    var rootAdded = AddRootNode(sender, childValue);
                    if (!rootAdded)
                    {
                        newParentNode = FindParentNode(Nodes.RootNodes, childValue);
                        if (newParentNode != null)
                        {
                            if (newParentNode.IsExpanded || newParentNode.ChildNodes.Any())
                            {
                                AddNode(newParentNode, sender, newParentNode.ChildNodes.Count);
                                return;
                            }
                            else
                            {
                                newParentNode.HasChildNodes = true;
                                // If FilterLevel is Root, no need to update HasVisibleChildNodes based on filtering.
                                if (FilterLevel != FilterLevel.Root)
                                    UpdateHasVisibleChildNodesBasedOnChildItem(newParentNode, sender);
                                ChangeParentNodeExpander(newParentNode);
                            }
                        }
                    }
                    return;
                }
            }
            var parentNode = treeNode.ParentNode;

            // Changing root node data
            if (parentNode == null)
            {
                var addAsRoot = AddRootNode(sender, childValue, isRoot: true, canAdd: false);
                if (addAsRoot)
                {
                    // no need to do anything
                    return;
                }
            }

            newParentNode = FindParentNode(Nodes.RootNodes, childValue);
            if (newParentNode == parentNode && parentNode != null)
                return;

            if (newParentNode != null)
            {
                // invalid case
                if (IsAncestor(newParentNode, treeNode))
                {
                    // UWP-3144 - When child property and parent property have same value and needs to be added as root, we should remove existing node and add as root.
                    var addAsRoot = AddRootNode(sender, childValue, canAdd: false);
                    if (addAsRoot)
                    {
                        RemoveNode(treeNode);
                        AddNode(null, treeNode, newParentNode.ChildNodes.Count);
                        return;
                    }
                    RemoveNode(parentNode, sender);
                    return;
                }
                else
                {
                    if (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnPropertyChange) || ((this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnEdit) || this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit)) && e == null))
                    {
                        RemoveNode(treeNode);
                        if (newParentNode.IsExpanded || newParentNode.ChildNodes.Any())
                        {
                            AddNode(newParentNode, treeNode, newParentNode.ChildNodes.Count);
                        }
                        else
                        {
                            ResetParentAndLevel(newParentNode, treeNode);
                            newParentNode.HasChildNodes = true;
                            if (CanFilterNode(treeNode))
                                UpdateParentNodeOnPropertyChange(treeNode, newParentNode);
                            ChangeParentNodeExpander(newParentNode);
                        }
                        if (e == null && this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit))
                        {
                            List<TreeNode> nodes = new List<TreeNode>();
                            tempNode = treeNode;
                            var rootNode = Nodes.GetRootNode(treeNode, ref nodes);
                            for (int i = nodes.Count - 1; i >= 0; i--)
                                TreeGrid.ExpandNode(nodes[i]);
                            BringUpdatedItemIntoView(treeNode, childPropertyName);
                            return;
                        }

                        if ((parentNode == null || parentNode.IsExpanded) || newParentNode.IsExpanded)
                            return;
                    }
                }
            }
            else
            {
                var parentItem = FindParentItem(childValue);
                if (parentItem == null)
                {
                    if (parentNode == null)
                        return;

                    if (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnPropertyChange) || ((this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnEdit) || this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit)) && e == null))
                    {
                        RemoveNode(treeNode);
                        // Check whether node can be added as root.
                        var addAsRoot = AddRootNode(sender, childValue, canAdd: false);
                        if (addAsRoot)
                        {
                            // Add updated tree node as root node
                            AddNode(null, treeNode, Nodes.RootNodes.Count);
                            // While using editing only, need to scroll to updated item.
                            if (e == null)
                                BringUpdatedItemIntoView(treeNode, childPropertyName);
                        }
                    }
                    else
                    {
                        RemoveNode(parentNode, sender);
                    }
                    return;
                }
                else
                {
                    var visibleParentNode = FindVisibleParentNode(parentItem);
                    if (IsAncestor(visibleParentNode, treeNode))
                    {
                        RemoveNode(parentNode, sender);
                        return;
                    }
                    else
                    {
                        if (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnPropertyChange) || ((this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnEdit) || this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit)) && e == null))
                        {
                            RemoveNode(treeNode);
                            if (visibleParentNode == null)
                                return;
                            if (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveAndExpandOnEdit) && e == null)
                            {
                                List<TreeNode> nodes = new List<TreeNode>();
                                tempNode = treeNode;
                                var visibleParentNodeLineCount = visibleParentNode.GetYAmountCache() - 1;
                                nodes.Add(visibleParentNode);
                                var rootNode = Nodes.GetRootNode(visibleParentNode, ref nodes);
                                TreeGrid.TreeGridModel.suspend = true;
                                for (int i = nodes.Count - 1; i >= 0; i--)
                                    TreeGrid.ExpandNode(nodes[i]);
                                var orgParentNode = ExpandNodeRecursively(visibleParentNode, parentItem, parentItemCollection.Count - 2);
                                if (orgParentNode != null)
                                    TreeGrid.ExpandNode(orgParentNode);
                                TreeGrid.TreeGridModel.suspend = false;
                                foreach (var n in treeNode.ChildNodes)
                                {
                                    n.Level = treeNode.Level + 1;
                                    ResetLevel(n);
                                }
                                tempNode = null;
                                TreeGrid.TreeGridModel.UpdateRows(nodes.FirstOrDefault(), visibleParentNodeLineCount);
                                parentItemCollection.Clear();
                                BringUpdatedItemIntoView(treeNode, childPropertyName);
                            }
                            return;
                        }
                    }
                }
            }
            return;
        }


        /// <summary>
        /// After editing, bring the updated node into view.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="propertyName">property name.</param>
        internal void BringUpdatedItemIntoView(TreeNode treeNode, string propertyName)
        {
            if (this.SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.ScrollToUpdatedItem))
                TreeGrid.TreeGridModel.BringUpdatedItemIntoView(treeNode, childPropertyName);
        }

        internal bool IsAncestor(TreeNode newParentNode, TreeNode treeNode)
        {
            if (newParentNode == treeNode)
                return true;
            if (newParentNode == null)
                return false;
            if (newParentNode.ParentNode == treeNode)
                return true;
            if (newParentNode.ParentNode == null)
                return false;
            return IsAncestor(newParentNode.ParentNode, treeNode);
        }


        private void UpdateNodesOnParentPropertyChange(object sender, TreeNode treeNode, PropertyChangedEventArgs e)
        {
            if (SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.None))
                return;

            var parentValue = propertyAccessProvider.GetValue(sender, parentPropertyName);
            if (treeNode != null && treeNode.ChildNodes.Any())
            {
                object childValue = propertyAccessProvider.GetValue(treeNode.ChildNodes[0].Item, childPropertyName);
                // checking parent property is changed or not (by comparing child and parent property values)
                if (IsEqual(parentValue, childValue))
                    return;
            }
            if ((e == null && SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnEdit)) || (e != null && SelfRelationUpdateMode.HasFlag(SelfRelationUpdateMode.MoveOnPropertyChange)))
            {
                TreeNodes nodeCollection = null;
                object childValue = propertyAccessProvider.GetValue(sender, childPropertyName);
                // if (!IsEqual(parentValue, childValue) || (IsEqual(parentValue, childValue) && SelfRelationRootValue == DependencyProperty.UnsetValue))
                if (!IsEqual(parentValue, childValue) || SelfRelationRootValue == DependencyProperty.UnsetValue)
                {
                    // Root value not set case. Remove root nodes if it has child property value equals to parent property value.
                    nodeCollection = RemoveNodesHavingChildPropertyEqualsToNewParentProperty(parentValue, treeNode);
                }

                // For property change case only

                if (treeNode == null)
                {
                    var childItems = GetChildSourceFromParentID(SelfRelationRootValue, null, true);

                    foreach (var childItem in childItems)
                    {
                        CanAddRootNode(childItem, true);
                    }
                }
                else
                {
                    if (SelfRelationRootValue == DependencyProperty.UnsetValue)
                    {
                        AddRootNodesFromRemovedParent(treeNode);
                        // When parent and child value both are same, need to add the child nodes(removed from root nodes) having child property equal to parent property.
                        if (nodeCollection != null && nodeCollection.Any() && IsEqual(parentValue, childValue) && SelfRelationRootValue == DependencyProperty.UnsetValue)
                        {
                            nodeCollection.Remove(treeNode);
                            if (treeNode.IsExpanded || treeNode.ChildNodes.Any())
                                AddTreeNodes(treeNode, nodeCollection);
                        }
                    }
                    else
                    {
                        ResetNodes(treeNode, true);
                    }

                    // After parent property name is changed, if node is expanded and dont have child node, need to populate child nodes based on new parent property value.
                    if (treeNode.IsExpanded && !treeNode.ChildNodes.Any())
                    {
                        TreeGrid.TreeGridModel.ExpandNode(treeNode, true);
                    }
                    else
                    {
                        IsChildNodeAvailable(treeNode);
                        var rowIndex = TreeGrid.ResolveToRowIndex(treeNode);
                        TreeGrid.UpdateDataRow(rowIndex);
                    }
                }
            }
        }

        private void RemoveNodesUnderRootNodes(object item)
        {
            TreeNode parentNode = null;
            // Remove nodes under Root nodes
            if (SortDescriptions.Count == 1 && SortDescriptions.Any(desc => desc.ColumnName == parentPropertyName))
            {
                object CurrentAddItem = null;
                object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                var itemType = this.SourceCollection.GetItemType(true);
                if (itemType != null)
                {
                    if (itemType.IsInterface() || itemType.IsAbstract())
                        CurrentAddItem = null;
                    else
                        CurrentAddItem = itemType.CreateNew();
                    propertyAccessProvider.SetValue(CurrentAddItem, parentPropertyName, childValue);
                }
                var tempNode = new TreeNode() { Item = CurrentAddItem };
                parentNode = FindParentNode(this.Nodes.RootNodes, tempNode);
            }
            else
            {
                object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                parentNode = FindParentNode(this.Nodes.RootNodes, childValue);
            }
            if (parentNode == null)
                return;

            if (parentNode.IsExpanded || parentNode.ChildNodes.Any())
            {
                RemoveNode(parentNode, item);
            }
            else
            {
                if (parentNode.HasChildNodes)
                {
                    parentNode.isVisibleChildNodeAvailabilityChecked = false;
                    ChangeParentNodeExpander(parentNode);
                }
            }
        }

        private void RemoveNodes(IList oldItems)
        {
            if (SelfRelationRootValue != DependencyProperty.UnsetValue)
            {
                var parentID = SelfRelationRootValue;
                foreach (var item in oldItems)
                {
                    object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                    if (IsEqual(parentID, childValue, true))
                    {
                        RemoveNode(null, item);
                    }
                    else
                    {
                        RemoveNodesUnderRootNodes(item);
                    }
                }
            }
            else
            {
                foreach (var item in oldItems)
                {
                    object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                    var treeNode = FindNodefromData(null, item);
                    AddRootNodesFromRemovedParent(treeNode, false);
                    // to be removed from root
                    //When parent and child property value are same, it will not be considered as root node in HasRecord method. So node is checked in root nodes collection.
                    if (!HasRecord(item, childValue) || this.Nodes.RootNodes.Contains(treeNode))
                    {
                        RemoveNode(null, item);
                    }
                    else
                    {
                        RemoveNodesUnderRootNodes(item);
                    }
                }
            }
        }


        internal object SelfRelationRootValue
        {
            get
            {
                var selfRelationRootValue = TreeGrid.ReadLocalValue(SfTreeGrid.SelfRelationRootValueProperty);
                if (selfRelationRootValue != DependencyProperty.UnsetValue)
                {
                    return TreeGrid.SelfRelationRootValue;
                }
                return DependencyProperty.UnsetValue;
            }
        }

        internal void CheckPrimaryKey()
        {
            if (!IsParentPropertyUnique())
                throw new Exception("Primary key should be unique");
        }


        /// <summary>
        /// While removing nodes from 1 node and add as child node of another node, need to reset level and parent node
        /// </summary>
        /// <param name="parentNode">parentNode</param>
        /// <param name="treeNode">treeNode</param>
        internal void ResetParentAndLevel(TreeNode parentNode, TreeNode treeNode)
        {
            treeNode.ParentNode = parentNode;
            treeNode.Level = parentNode.Level + 1;
            ResetLevel(treeNode);
        }

        /// <summary>
        /// Remove child nodes having child property equal to new parent property as parent property value is changed.
        /// </summary>
        /// <param name="parentValue">parentValue</param>
        /// <param name="treeNode">parent property value changed tree node.</param>
        /// <returns>removed node collection. It will be added as child nodes of modified tree node if some constraints are satisfied.</returns>

        internal TreeNodes RemoveNodesHavingChildPropertyEqualsToNewParentProperty(object parentValue, TreeNode treeNode)
        {
            var nodeCollection = GetNodeCollection(parentValue);
            if (nodeCollection.Any())
            {
                foreach (var node in nodeCollection)
                {
                    if (node != treeNode)
                    {
                        RemoveNode(node);
                    }
                }
            }
            return nodeCollection;
        }

        /// <summary>
        /// Check whether the data can be added to root node by checking source list.
        /// </summary>
        /// <param name="data">data.</param>
        /// <param name="childValue">child property value.</param>
        /// <param name="canAdd">if it is true, node will be added in root nodes.</param>
        /// <param name="isRoot">If it is true, check whether data already exists in root nodes.</param>
        /// <returns>returns true if data can be added as root node or already present in root nodes. </returns>
        internal bool AddRootNode(object data, object childValue, bool canAdd = true, bool isRoot = false)
        {
            if (SelfRelationRootValue != DependencyProperty.UnsetValue)
            {
                if (IsEqual(SelfRelationRootValue, childValue, true))
                {
                    if (isRoot)
                        return true;
                    return CanAddRootNode(data, canAdd);
                }
            }
            else
            {
                if (!HasRecord(data, childValue))
                {
                    if (isRoot)
                        return true;
                    return CanAddRootNode(data, canAdd);
                }
            }
            return false;
        }

        /// <summary>
        /// Check whether the data can be added to root node by checking source list.
        /// </summary>
        /// <param name="data">data.</param>
        /// <param name="canAdd">if it is true, node will be added in root nodes.</param>
        /// <returns>returns true if data can be added as root node.</returns>

        internal bool CanAddRootNode(object data, bool canAdd)
        {
            if (!Nodes.RootNodes.sourceList.Contains(data))
            {
                if (canAdd)
                {
                    AddNode(null, data, Nodes.RootNodes.Count);
                }
                return true;
            }
            return false;
        }

        internal TreeNode FindParentNode(TreeNodes nodes, object childValue)
        {
            TreeNode parentNode = null;
            foreach (var node in nodes)
            {
                var record = node.Item;
                object parentValue = propertyAccessProvider.GetValue(record, parentPropertyName);
                //  if (IsEqual(parentValue, childValue))
                if (!IsEqual(SelfRelationRootValue, childValue, true) && IsEqual(parentValue, childValue))
                {
                    parentNode = node;
                    break;
                }
            }
            if (parentNode == null)
            {
                foreach (var node in nodes)
                {
                    if (node.IsExpanded || node.ChildNodes.Any())
                    {
                        parentNode = FindParentNode(node.ChildNodes, childValue);
                        if (parentNode != null)
                            break;
                    }
                }
            }
            return parentNode;
        }

        internal TreeNode FindParentNode(TreeNodes nodes, TreeNode tempNode)
        {
            TreeNode parentNode = null;

            var index = nodes.nodeList.BinarySearch(tempNode, SortComparer);
            if (index < 0)
            {
                foreach (var node in nodes)
                {
                    if (node.IsExpanded || node.ChildNodes.Any())
                    {
                        parentNode = FindParentNode(node.ChildNodes, tempNode);
                        if (parentNode != null)
                        {
                            break;
                        }
                    }
                }
            }
            else
                parentNode = nodes[index];

            return parentNode;
        }


        private TreeNodes GetNodeCollection(object parentValue)
        {
            var nodes = new TreeNodes();
            foreach (object record in SourceCollection)
            {
                object childValue = propertyAccessProvider.GetValue(record, childPropertyName);
                if (IsEqual(parentValue, childValue))
                {
                    var node = FindNodefromData(null, record, true);
                    if (node != null)
                        nodes.Add(node);
                }
            }
            return nodes;
        }
        internal bool IsEqual(object parentValue, object childValue, bool checkType = false)
        {
            if (checkType && parentValue != null && parentValue.GetType() != childValue.GetType())
            {
                if (TypeConverterHelper.CanConvert(childValue.GetType(), parentValue.ToString()))
                    parentValue = Convert.ChangeType(parentValue, childValue.GetType());
            }

            if ((childValue != null && childValue.Equals(parentValue)) || (parentValue == null && childValue == null))
            {
                return true;
            }
            return false;
        }

        private IEnumerable GetChildSourceFromParentID(object parentID, object parentItem, bool addChildNode = true)
        {
            List<object> childSource = new List<object>();
            var itemsSource = (IEnumerable)this.TreeGrid.ItemsSource;
            var itemsCount = SourceCollection.AsQueryable().Count();

            if (SelfRelationRootValue == DependencyProperty.UnsetValue && parentItem == null)
            {
                foreach (object item in itemsSource)
                {
                    object childValue = propertyAccessProvider.GetValue(item, childPropertyName);
                    int i = 0;

                    foreach (object record in itemsSource)
                    {
                        object parentValue = propertyAccessProvider.GetValue(record, parentPropertyName);
                        if (!item.Equals(record) && ((parentValue != null && parentValue.Equals(childValue)) || parentValue == childValue))
                            break;
                        i++;
                        if (i == itemsCount)
                        {
                            childSource.Add(item);
                        }
                    }
                }
                return childSource as IEnumerable;
            }

            foreach (object record in itemsSource)
            {
                object childValue = propertyAccessProvider.GetValue(record, childPropertyName);
                if (IsEqual(parentID, childValue, parentItem == null) && record != parentItem)
                {
                    if (!Nodes.RootNodes.sourceList.Contains(record))
                    {
                        childSource.Add(record);
                    }
                }
                if (!addChildNode && childSource.Count > 0)
                {
                    if (CanFilter)
                    {
                        if (FilterItem(childSource.LastOrDefault()))
                            break;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return childSource as IEnumerable;
        }

        internal TreeNode FindVisibleParentNode(object record)
        {
            var childValue = propertyAccessProvider.GetValue(record, childPropertyName);
            var parentItem = FindParentItem(childValue);
            while (parentItem != null)
            {
                //  invalid case
                if (!parentItemCollection.Contains(parentItem))
                    parentItemCollection.Add(parentItem);
                else
                    return null;
                var node = FindNodefromData(null, parentItem);
                if (node != null)
                    return node;
                parentItem = FindParentItemFromRecord(parentItem);
            }
            return null;
        }

        List<object> parentItemCollection = new List<object>();


        //check whether child property value equals to any parent property
        internal bool HasRecord(object data, object value)
        {
            var collection = SourceCollection.ToList<object>();
            var count = collection.Where(c => c != data).Select(i => propertyAccessProvider.GetValue(i, parentPropertyName)).Count(c => (c == value) || (c != null && c.Equals(value)));
            return count > 0;
        }

        /// <summary>
        /// Find ParentItem from child value.
        /// </summary>
        /// <param name="childValue">childValue.</param>
        /// <returns>parent record.</returns>
        internal object FindParentItem(object childValue)
        {
            foreach (object record in SourceCollection)
            {
                object parentValue = propertyAccessProvider.GetValue(record, parentPropertyName);
                //if (IsEqual(parentValue, childValue))
                if (!IsEqual(SelfRelationRootValue, childValue, true) && IsEqual(parentValue, childValue))
                    return record;
            }
            return null;
        }
    }
}
