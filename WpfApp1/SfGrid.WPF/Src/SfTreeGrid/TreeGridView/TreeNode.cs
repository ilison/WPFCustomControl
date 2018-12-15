#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeNode : IDisposable, INotifyPropertyChanged
    {
        public TreeNode()
        {
            ChildNodes = new TreeNodes();
            isDirty = false;
            maxLevel = level;
        }

        internal TreeNode(object item, int level, bool isExpanded, TreeNode parentNode, Func<TreeGridView> getView)
        {
            Level = level;
            Item = item;
            ParentNode = parentNode;
            ChildNodes = new TreeNodes();
            this.isExpanded = isExpanded;
            isDirty = false;
            maxLevel = level;
            GetView = getView;
        }

        /// <summary>
        /// Flag which indicates whether node's IsChecked state is changed by (checkbox click/recursive check, node population) or by changing node's IsChecked state or through CheckBoxMappingName.
        /// It will be set as True only while clicking CheckBox and IsChecked state is changed on node's population and on recursive checking.
        /// </summary>
        internal bool isCheckedChanged = false;


        private INotifyCollectionChanged childItems;
        internal INotifyCollectionChanged ChildItems
        {
            get
            {
                return childItems;
            }
            set
            {
                var view = GetView();
                var nestedView = view as TreeGridNestedView;
                if (childItems != value)
                {
                    if (childItems != null)
                    {
                        if (!view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.CollectionChange))
                            childItems.CollectionChanged -= OnChildItemsCollectionChanged;
                        RemoveNotifyListeners((IEnumerable)childItems);
                    }
                    childItems = value;
                    if (!view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.CollectionChange))
                        childItems.CollectionChanged += OnChildItemsCollectionChanged;
                    AddNotifyListeners((IEnumerable)childItems);
                }
            }
        }

        internal void WireNotifyListeners()
        {
            if (childItems != null)
            {
                var nestedView = this.GetView() as TreeGridNestedView;
                RemoveNotifyListeners((IEnumerable)childItems);
                AddNotifyListeners((IEnumerable)childItems);
            }
        }

        bool isPropertyChangedEventWired = false;
        internal void RemoveNotifyListeners(IEnumerable collection)
        {
            var view = GetView();
            if (view != null && (view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange)))
                return;
            if (!isPropertyChangedEventWired)
                return;
            isPropertyChangedEventWired = false;
            foreach (var record in collection)
                RemoveNotifyListener(record);
        }

        internal void AddNotifyListeners(IEnumerable collection)
        {
            var view = GetView();
            if (view != null && (view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange)))
                return;
            if (isPropertyChangedEventWired)
                return;
            isPropertyChangedEventWired = true;
            foreach (var record in collection)
                AddNotifyListener(record);
        }
        private void AddNotifyListener(object record)
        {
#if WPF
            var notifyPropertyChanging = record as INotifyPropertyChanging;
            if (notifyPropertyChanging != null)
                notifyPropertyChanging.PropertyChanging += OnPropertyChanging;
#endif
            var notifyPropertyChanged = record as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged += OnPropertyChanged;
            }
        }

        private void RemoveNotifyListener(object record)
        {
#if WPF
            var notifyPropertyChanging = record as INotifyPropertyChanging;
            if (notifyPropertyChanging != null)
                notifyPropertyChanging.PropertyChanging -= OnPropertyChanging;
#endif
            var notifyPropertyChanged = record as INotifyPropertyChanged;
            if (notifyPropertyChanged != null)
            {
                notifyPropertyChanged.PropertyChanged -= OnPropertyChanged;
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var view = GetView();
            if (view == null || view.IsDisposed)
                return;
            view.NotifyPropertyChangedHandler(sender, e, this);
        }
#if WPF
        private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {

        }
#endif
        private void OnChildItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var view = GetView();
            var nestedView = view as TreeGridNestedView;
            if (nestedView != null)
            {
                nestedView.OnChildCollectionChanged(sender, e, this);
            }
        }

        int level = 0;
        public int Level
        {
            get { return level; }
            internal set { level = value; }
        }


        /// <summary>
        /// Gets the number of visible child nodes(Child nodes which match the filtering criteria).
        /// </summary>        
        public int VisibleNodesCount
        {
            get { return yAmountCache - 1; }
        }

        private int yAmountCache = 1;

        private int maxLevel;

        /// <summary>
        /// Sets the dirty. When this is set to true, the YAmountCache and maxLevel will be re-computed for the tree node.
        /// </summary>
        public void SetDirty()
        {
            if (!this.isDirty)
            {
                this.isDirty = true;
            }
        }

        /// <summary>
        /// Populates the child nodes without expanding it. 
        /// </summary>                
        public void PopulateChildNodes()
        {
            var view = GetView();
            view.RequestTreeItems(this);
        }

#if UWP
        [Obsolete("This method is marked as Obsolete, use PopulateChildNodes method instead")]
        /// <summary>
        /// Populates the child nodes without expanding it. 
        /// </summary>   
        public void PopualateChildNodes()
        {
            PopulateChildNodes();
        }
#endif

        internal bool isDirty;
        public virtual int GetYAmountCache()
        {
            if (this.isDirty)
            {
                this.RecalculateYAmount();
                RecalculateMaxLevel();
                this.isDirty = false;
            }
            return this.yAmountCache;
        }

        internal int GetMaxLevel()
        {
            if (this.isDirty)
            {
                this.RecalculateMaxLevel();
                RecalculateYAmount();
                this.isDirty = false;
            }
            return this.maxLevel;
        }

        internal bool isChildNodeAvailabilityChecked = false;
        internal bool isVisibleChildNodeAvailabilityChecked = false;
        private void RecalculateYAmount()
        {
            var cachedCounter = 1;
            this.RecalculateYAmount(this, ref cachedCounter);
            this.yAmountCache = cachedCounter;
        }

        private void RecalculateYAmount(TreeNode node, ref int counter0)
        {
            if (node == null)
                counter0 += 0;
            else if (node.IsExpanded)
            {
                var childNodes = node.ChildNodes.Where(n => !n.IsFiltered);
                counter0 += childNodes.Count();
                foreach (var childNode in childNodes)
                {
                    counter0 += childNode.GetYAmountCache() - 1;
                }
            }
        }

        internal void RecalculateMaxLevel()
        {
            var cachedCounter = 1;
            this.RecalculateMaxLevel(this, ref cachedCounter);
            this.maxLevel = cachedCounter;
        }

        private void RecalculateMaxLevel(TreeNode node, ref int counter0)
        {
            if (node == null)
                counter0 += 0;
            else
            {
                counter0 = node.level;
                if (node.IsExpanded)
                {
                    foreach (var childNode in node.ChildNodes)
                    {
                        var level = childNode.GetMaxLevel();
                        if (level > counter0)
                            counter0 = level;
                    }
                }
            }
        }

        internal Func<TreeGridView> GetView;

        public object Item { get; internal set; }
        public TreeNodes ChildNodes { get; internal set; }

        private bool hasChildNodes = false;

        /// <summary>
        /// Gets the value which indicates whether the node has a child node(s) or not.
        /// </summary>
        public bool HasChildNodes
        {
            get
            {
                if (!isChildNodeAvailabilityChecked || !isVisibleChildNodeAvailabilityChecked)
                {
                    var view = GetView();
                    isChildNodeAvailabilityChecked = true;
                    view.IsChildNodeAvailable(this);
                }
                return hasChildNodes;
            }
            internal set
            {
                // In data manipulation cases, we can set hasChildNodes directly
                hasChildNodes = value;
                isChildNodeAvailabilityChecked = true;
                UpdateHasVisibleChildNodes(hasChildNodes);
            }
        }
        private bool hasVisibleChildNodes = false;

        /// <summary>
        /// Gets the value which indicates whether the node has a child node(s) displayed in a View(matches filtering criteria) or not.
        /// </summary>
        public bool HasVisibleChildNodes
        {
            get
            {
                if (!isVisibleChildNodeAvailabilityChecked)
                {
                    var view = GetView();
                    isVisibleChildNodeAvailabilityChecked = true;
                    view.IsChildNodeAvailable(this);
                }
                return hasVisibleChildNodes;
            }
            internal set
            {
                hasVisibleChildNodes = value;
                isVisibleChildNodeAvailabilityChecked = true;
                if (hasVisibleChildNodes && hasVisibleChildNodes != hasChildNodes)
                {
                    hasChildNodes = hasVisibleChildNodes;
                    isChildNodeAvailabilityChecked = true;
                }
                OnPropertyChanged("HasVisibleChildNodes");
            }
        }

        internal void SetHasChildNodes(bool value)
        {
            this.HasChildNodes = value;
            this.HasVisibleChildNodes = value;
        }

        internal void UpdateHasVisibleChildNodes(bool hasVisibleChildNodes)
        {
            var view = GetView();
            if (!view.CanFilterNode(this) || view.FilterLevel == FilterLevel.Root)
                HasVisibleChildNodes = hasVisibleChildNodes;
        }
        public TreeNode ParentNode { get; internal set; }

        private bool isExpanded;

        public bool IsExpanded
        {
            get
            {
                return isExpanded;
            }
            set
            {
                if (isExpanded != value)
                {
                    isExpanded = value;
                    this.SetDirtyOnExpandOrCollapse();
                }
            }
        }

        private bool isFiltered = false;

        /// <summary>
        /// Gets or sets a value that indicates whether node is filtered or not.
        /// </summary>
        /// <value>        
        /// <b>true</b>If the node is filtered (filter condition is not matched). It did not show in view;Otherwise,<b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.Filter"/>

        public bool IsFiltered
        {
            get
            {
                return isFiltered;
            }
            internal set
            {
                isFiltered = value;
                var view = this.GetView();
                if (view.suspendIsFilteredUpdate)
                    SetDirty();
                else
                    SetDirtyOnExpandOrCollapse();
                if (!isFiltered)
                {
                    if (view.FilterLevel == FilterLevel.Extended)
                        ChangeIsFilteredState();
                }
            }
        }

        internal bool HasChildItems { get; set; }

        #region CheckBox Selection
        private bool? isChecked = false;

        /// <summary>
        /// Gets a value which indicates whether node is checked or not.
        /// </summary>
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            internal set
            {
                this.SetCheckedState(value, true, true);
                isCheckedChanged = false;
            }
        }

        /// <summary>
        /// Sets the <see cref="TreeNode.IsChecked"/> property with additional parameter to handle recursive checking.
        /// </summary>
        /// <param name="value">New value for <see cref="TreeNode.IsChecked"/>.</param>
        /// <param name="canUpdateParentNode">specifies whether need to update parent node's state when EnableRecursiveChecking is true.
        /// </param>
        /// <param name="canUpdateChildNodes">specifies whether need to update child node's state when EnableRecursiveChecking is True.</param>
        public void SetCheckedState(bool? value, bool canUpdateParentNode = true, bool canUpdateChildNodes = true)
        {
            var view = this.GetView();
            if (view == null)
            {
                isChecked = value;
                return;
            }
            // When SetCheckedState is called from sample level, need to change isCheckedChanged here.
            if (view.RecursiveCheckingMode == RecursiveCheckingMode.Default)
                isCheckedChanged = true;
            if (isChecked != value)
            {
                isChecked = value;
                view.SetCheckBoxPropertyNameValue(this);
                OnPropertyChanged("IsChecked");

                if (!view.EnableRecursiveChecking)
                    return;

                if (!view.suspendIsCheckedRecursiveUpdate)
                {
                    view.suspendIsCheckedRecursiveUpdate = true;
                    UpdateCheckedState(canUpdateParentNode, canUpdateChildNodes);
                    view.suspendIsCheckedRecursiveUpdate = false;
                }
            }
            else
            {
                view.SetCheckBoxPropertyNameValue(this);
            }
        }

        private void UpdateCheckedState(bool updateParentState = true, bool updateChildState = true)
        {
            if (this.ParentNode == null)
            {
                if (updateChildState)
                    SetIsCheckedForChildNodes(this, IsChecked);
            }
            else
            {
                if (updateChildState)
                    SetIsCheckedForChildNodes(this, IsChecked);
                if (updateParentState)
                    SetIsCheckedForParentNode(this.ParentNode);
            }
        }

        private void SetIsCheckedForChildNodes(TreeNode node, bool? isChecked)
        {
            foreach (var childNode in node.ChildNodes)
            {
                childNode.IsChecked = isChecked;
                SetIsCheckedForChildNodes(childNode, isChecked);
                childNode.isCheckedChanged = true;
            }
        }
        private void SetIsCheckedForParentNode(TreeNode node)
        {
            if (node.ChildNodes.All(n => n.IsChecked == true))
            {
                node.IsChecked = true;
            }
            else if (node.ChildNodes.All(n => n.IsChecked == false))
            {
                node.IsChecked = false;
            }
            else
                node.IsChecked = null;
            if (node.ParentNode == null)
                return;
            SetIsCheckedForParentNode(node.ParentNode);
        }

        #endregion

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        internal void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        internal void SetDirtyOnExpandOrCollapse()
        {
            TreeNode node = this;
            while (node != null)
            {
                node.SetDirty();
                node = node.ParentNode;
            }
        }

        /// <summary>
        /// Change isFiltered state of the all its ancestor nodes when filter level is extended.
        /// </summary>
        internal void ChangeIsFilteredState()
        {
            TreeNode node = this.ParentNode;
            while (node != null)
            {
                node.HasVisibleChildNodes = true;
                node.isFiltered = false;
                node = node.ParentNode;
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNode"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNode"/> class.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release all the resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            var view = GetView();
            if (this.ChildNodes != null)
            {
                this.ChildNodes.Dispose();
                this.ChildNodes.Clear();
                this.ChildNodes = null;
            }
            if (this.childItems != null)
            {
                RemoveNotifyListeners((IEnumerable)childItems);
                if (!view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.CollectionChange))
                    childItems.CollectionChanged -= OnChildItemsCollectionChanged;
                childItems = null;
            }
            else
            {
                if (view != null && view.needToUnwireListener && !view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && view.NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                {
                    view.RemoveNotifyListener(Item);
                }
            }
            this.ParentNode = null;
            this.GetView = null;
        }
    }
}
