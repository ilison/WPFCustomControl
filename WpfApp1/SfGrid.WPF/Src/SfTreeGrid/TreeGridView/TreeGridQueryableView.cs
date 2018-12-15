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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.Data;
using System.ComponentModel;
using System.Windows;
using System.Collections.Specialized;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridQueryableView : TreeGridView
    {
        protected internal SfTreeGrid TreeGrid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridQueryableView(IEnumerable source, SfTreeGrid treeGrid)
            : base(source)
        {
            this.TreeGrid = treeGrid;
            propertyAccessProvider = CreateItemPropertiesProvider();
        }

        public override void AttachTreeView(object treeGrid)
        {
            if (!(treeGrid is SfTreeGrid))
                throw new InvalidOperationException("Attached view is not type of SfTreeGrid");
            this.TreeGrid = treeGrid as SfTreeGrid;
        }

        protected override IPropertyAccessProvider CreateItemPropertiesProvider()
        {
            if (this.IsDynamicBound)
            {
                return new TreeGridDynamicPropertiesProvider(TreeGrid, this);
            }
            else
                return new TreeGridPropertiesProvider(TreeGrid, this);
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

        internal override void UpdateParentNode(TreeNode node)
        {
            var rowIndex = TreeGrid.ResolveToRowIndex(node);
            TreeGrid.UpdateDataRow(rowIndex);
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
                e.ChildItems = TreeGrid.ItemsSource as IEnumerable;
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

        internal override void ValidateCheckBoxPropertyValue(TreeNodes nodes)
        {
            TreeGrid.NodeCheckBoxController.ValidateCheckBoxPropertyValue(nodes);
        }

        protected override void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (IsInSuspend)
                return;
            TreeNode treeNode = null;
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        this.AddNodes(treeNode, e.NewItems, e.NewStartingIndex);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        this.RemoveNodes(treeNode, e.OldItems);
                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        ResetNodes(treeNode);
                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    {
                        MoveNode(treeNode, e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        ReplaceNode(treeNode, e.OldItems, e.NewItems, e.NewStartingIndex);
                        break;
                    }
            }
        }
    }

    public class TreeGridNestedView : TreeGridQueryableView
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridNestedView(IEnumerable source, SfTreeGrid treeGrid)
            : base(source, treeGrid)
        {
            childPropertyName = treeGrid.ChildPropertyName;
            propertyAccessProvider = CreateItemPropertiesProvider();
        }

        internal string childPropertyName;
        private void WireNotifyPropertyChange(IEnumerable list, TreeNode node)
        {
            if (list is INotifyCollectionChanged)
            {
                var childItems = list as INotifyCollectionChanged;
                if (node.ChildItems != childItems)
                    node.ChildItems = childItems;
                else
                    node.WireNotifyListeners();
            }
            else
            {
                if (node.ChildItems != null)
                    RemoveNotifyListeners((IEnumerable)node.ChildItems);
                AddNotifyListeners(list);
            }
        }

        protected override IPropertyAccessProvider CreateItemPropertiesProvider()
        {
            return new TreeGridPropertiesProvider(TreeGrid, this);
        }

        protected internal override void UpdateNodesOnPropertyChange(object sender, PropertyChangedEventArgs e, TreeNode treeNode = null)
        {
            if (treeNode == null)
                treeNode = FindNodefromData(null, sender);
            else
                treeNode = treeNode.ChildNodes.GetNode(sender);

            //  Property changed event will be wired for child items also. In that case, we can not get node since parent node is not expanded and its child nodes are not populated.
            if (treeNode == null)
                return;
            if (e != null && e.PropertyName == childPropertyName)
            {
                TreeGrid.SelectionController.SuspendUpdates();
                var collection = propertyAccessProvider.GetValue(sender, childPropertyName);
                if (collection is INotifyCollectionChanged)
                    treeNode.ChildItems = collection as INotifyCollectionChanged;
                if (treeNode.ChildNodes.Any())
                {
                    RemoveChildNodes(treeNode);
                    TreeGrid.TreeGridModel.ExpandNode(treeNode, true);
                }
                TreeGrid.SelectionController.ResumeUpdates();
                (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows();
            }
            if ((e == null || e.PropertyName == CheckBoxMappingName) && !suspendIsCheckedUpdate)
            {
                UpdateNodesOnCheckBoxPropertyChange(sender, e, treeNode);
                TreeGrid.SelectionController.ProcessSelectionOnCheckedStateChange(treeNode);
            }
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
                e.ChildItems = TreeGrid.ItemsSource as IEnumerable;
            }
            else
            {
                if (!e.ParentNode.HasChildItems || e.ResetChildAndRepopulate)
                {
                    e.ChildItems = GetChildSource(e.ParentItem, childPropertyName);
                    if (e.ChildItems is INotifyCollectionChanged)
                        e.ParentNode.HasChildItems = true;
                }
                else
                    e.ChildItems = (IEnumerable)e.ParentNode.ChildItems;
            }

            if (e.ChildItems != null && e.ParentNode != null)
            {
                WireNotifyPropertyChange(e.ChildItems, e.ParentNode);
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

        // For filtering, we need to wire property changed event also. so below flag usage is removed.
        // Before expanding the node, no need to wire property changed event for child items. Based on this flag, it will be done.
        //internal bool needToWirePropertyChangedEvent = true;

        /// <summary>
        /// Raises when child collection changed.
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">An <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the action which is performed</param>
        /// <param name="treeNode">treeNode</param>
        protected internal virtual void OnChildCollectionChanged(object sender, NotifyCollectionChangedEventArgs e, TreeNode treeNode)
        {
            if (deferRefreshCount > -1 || IsInSuspend)
                return;
            TreeGrid.SelectionController.SuspendUpdates();
            SuspendNodeCollectionEvent();
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        if (treeNode == null || (treeNode.ChildNodes.Any() || treeNode.IsExpanded))
                            this.AddNodes(treeNode, e.NewItems, e.NewStartingIndex);
                        else if (treeNode != null)
                        {
                            if (!treeNode.HasVisibleChildNodes)
                            {
                                treeNode.HasChildNodes = true;
                                UpdateHasVisibleChildNodesBasedOnChildItems(treeNode, e.NewItems);
                                UpdateParentNode(treeNode);
                            }
                        }
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
                        if (treeNode == null || (treeNode.ChildNodes.Any() || treeNode.IsExpanded))
                            this.RemoveNodes(treeNode, e.OldItems);
                        else if (treeNode != null)
                        {
                            if (treeNode.HasChildNodes)
                            {
                                if (treeNode.ChildItems == null || (treeNode.ChildItems as IEnumerable).AsQueryable().Count() == 0)
                                {
                                    treeNode.SetHasChildNodes(false);
                                    UpdateParentNode(treeNode);
                                }
                                else if (treeNode.HasVisibleChildNodes)
                                {
                                    if (!UpdateHasVisibleChildNodesBasedOnChildItems(treeNode, treeNode.ChildItems as IEnumerable))
                                    {
                                        treeNode.HasVisibleChildNodes = false;
                                        UpdateParentNode(treeNode);
                                    }
                                }
                            }
                        }

                        break;
                    }
                case NotifyCollectionChangedAction.Reset:
                    {
                        if (treeNode == null || (treeNode.ChildNodes.Any() || treeNode.IsExpanded))
                            ResetNodes(treeNode);
                        else if (treeNode != null)
                        {
                            treeNode.SetHasChildNodes(false);
                            UpdateParentNode(treeNode);
                        }
                        break;
                    }

                case NotifyCollectionChangedAction.Move:
                    {
                        if (treeNode == null || (treeNode.ChildNodes.Any() || treeNode.IsExpanded))
                            MoveNode(treeNode, e.OldStartingIndex, e.NewStartingIndex);
                        break;
                    }

                case NotifyCollectionChangedAction.Replace:
                    {
                        if (treeNode == null || (treeNode.ChildNodes.Any() || treeNode.IsExpanded))
                            ReplaceNode(treeNode, e.OldItems, e.NewItems, e.NewStartingIndex);
                        else if (treeNode != null)
                        {
                            if (!treeNode.HasVisibleChildNodes)
                            {
                                treeNode.HasChildNodes = true;
                                UpdateHasVisibleChildNodesBasedOnChildItems(treeNode, e.NewItems);
                                UpdateParentNode(treeNode);
                            }
                            else
                            {
                                if (!UpdateHasVisibleChildNodesBasedOnChildItems(treeNode, treeNode.ChildItems as IEnumerable))
                                {
                                    treeNode.HasVisibleChildNodes = false;
                                    UpdateParentNode(treeNode);
                                }
                            }
                        }
                        break;
                    }
            }
            ResumeNodeCollectionEvent();
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).ResetSelectedRows();
        }

        private IEnumerable GetChildSource(object record, string propertyName)
        {
            return propertyAccessProvider.GetValue(record, propertyName) as IEnumerable;
        }

        protected override void OnCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnChildCollectionChanged(sender, e, null);
        }

    }

    //public class TreePagedCollectionView : TreeGridView
    //{
    //    internal SfTreeGrid treeGrid;
    //    public TreePagedCollectionView(SfTreeGrid treeGrid)
    //        : base()
    //    {
    //        this.treeGrid = treeGrid;
    //    }
    //    public override void AttachTreeView(object treeGrid)
    //    {
    //        if (!(treeGrid is SfTreeGrid))
    //            throw new InvalidOperationException("Attached view is not type of SfTreeGrid");
    //        this.treeGrid = treeGrid as SfTreeGrid;
    //    }
    //    public override void DetachTreeView()
    //    {
    //        treeGrid = null;
    //    }
    //}

    //public class TreeDataTableView : TreeGridView
    //{
    //    internal SfTreeGrid treeGrid;
    //    public TreeDataTableView(SfTreeGrid treeGrid)
    //        : base()
    //    {
    //        this.treeGrid = treeGrid;
    //        propertyAccessProvider = new TreeItemPropertiesProvider(this);
    //    }
    //    public override void AttachTreeView(object treeGrid)
    //    {
    //        if (!(treeGrid is SfTreeGrid))
    //            throw new InvalidOperationException("Attached view is not type of SfTreeGrid");
    //        this.treeGrid = treeGrid as SfTreeGrid;
    //    }
    //    public override void DetachTreeView()
    //    {
    //        treeGrid = null;
    //    }
    //}
}
