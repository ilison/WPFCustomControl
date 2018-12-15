#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using Syncfusion.Dynamic;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

#if UWP
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Threading;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// The class which has the functionalities of node management and sorting.
    /// </summary>
    public class TreeGridView : ITreeGridViewNotifier, IEditableCollectionView, IDisposable
    {
        private TreeNodeCollection nodes;
        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNode"/>
        /// </summary>
        public TreeNodeCollection Nodes
        {
            get
            {
                if (nodes == null || (this.refreshMode == TreeViewRefreshMode.DeferRefresh && IsInEndeferal))
                {
                    EnsureInitialized();
                }
                return nodes;
            }
        }

        private bool isDynamicSourceEvaluated = false;
        private bool isDynamicBound = false;
        /// <summary>
        /// Gets a value indicating whether view is dynamic bound.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is dynamic bound; otherwise, <c>false</c>.
        /// </value>
        internal bool IsDynamicBound
        {
            get
            {
                if (!this.isDynamicSourceEvaluated)
                {
                    var record = GetFirstItem();
                    if (record == null)
                        return false;

                    this.isDynamicBound = DynamicHelper.CheckIsDynamicObject(record.GetType());
                    this.isDynamicSourceEvaluated = true;
                }
                return this.isDynamicBound;
            }
            set
            {
                if (value)
                {
                    this.isDynamicSourceEvaluated = true;
                    this.isDynamicBound = value;
                    if (this.propertyAccessProvider != null)
                        this.propertyAccessProvider = this.CreateItemPropertiesProvider();
                }
            }
        }

        /// <summary>
        /// Gets the corresponding property type for the specified row data and column.
        /// </summary>
        /// <param name="propertyName"> 
        /// The corresponding property name to get property type.
        /// </param>
        /// <param name="rowData">
        /// The corresponding row data to get property type.
        /// </param>
        /// <returns>
        /// Returns the corresponding property type.
        /// </returns>
        internal Type GetPropertyType(object rowData, string propertyName)
        {
            //Get the Type of particular column
            var provider = GetPropertyAccessProvider();
            if (!IsDynamicBound)
            {
#if WPF
                PropertyDescriptorCollection typeInfos = TypeDescriptor.GetProperties(rowData.GetType());
#else
                PropertyInfoCollection typeInfos = new PropertyInfoCollection(rowData.GetType());
#endif
                var typeInfo = typeInfos.GetPropertyDescriptor(propertyName);
                if (typeInfo != null)
                    return typeInfo.PropertyType;
            }

            var cellvalue = provider.GetValue(rowData, propertyName);
            return cellvalue != null ? cellvalue.GetType() : null;
        }


        internal object firstItem = null;

        /// <summary>
        /// Returns first item from SourceCollection by enumeration.
        /// </summary>
        /// <returns>Returns first item from SourceCollection by enumeration.</returns>
        /// <remarks>If the source is IQueryable returns the first item by querying the SourceCollection.</remarks>
        internal virtual object GetFirstItem()
        {
            try
            {
                if (firstItem == null)
                {
                    if (this.SourceCollection == null)
                        return null;
                    var enumerator = this.SourceCollection.GetEnumerator();
                    if (enumerator == null || !enumerator.MoveNext())
                        return null;

                    firstItem = enumerator.Current;
                    return firstItem;
                }
                return firstItem;
            }
            catch
            {
                return null;
            }
        }


        // flag which indiactes whether need to unwire property changed in TreeNode.
        internal bool needToUnwireListener = true;
        private void EnsureInitialized()
        {
            this.refreshMode = TreeViewRefreshMode.None;
            UnwireEvents();
            if (IsInEndeferal)
                UnwireNotifyPropertyChangedForUnderlyingSource();

            if (nodes != null)
            {
                nodes.Dispose();
                nodes = null;
            }
            nodes = new TreeNodeCollection();
            PopulateTree();
            WireNotifyPropertyChangedForUnderlyingSource(IsInEndeferal);
            WireEvents();
        }

        /// <summary>
        /// Initialize a new instance of the TreeGridView class.
        /// </summary>
        public TreeGridView()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="TreeGridView"/> class.
        /// </summary>
        /// <param name="_source">
        /// The source collection.
        /// </param>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridView(IEnumerable _source)
        {
            this.SetSource(_source);
            this.InitializeTreeView();
        }

        private void InitializeTreeView()
        {
            if (source != null)
                this.SetItemProperties(source);
        }

        private bool ItemPropertiesSet = false;

        private void SetItemProperties(IEnumerable dataSource)
        {
            var list = dataSource;
            var enumerator = list.GetEnumerator();
            var current = GetFirstItem();
            if (enumerator.MoveNext() && enumerator.Current != null)
            {
                var castType = EnumerableExtensions.GetGenericSourceType(list);
#if WPF
                this.itemProperties = TypeDescriptor.GetProperties(castType ?? enumerator.Current.GetType());
#else
                this.itemProperties = new PropertyInfoCollection(castType ?? enumerator.Current.GetType());
#endif
                ItemPropertiesSet = true;
            }
            else
            {
                if (list.GetType().IsGenericType())
                {
                    Type genericType = null;
#if UWP
                    if (list.GetType().GetGenericTypeDefinition() == typeof(Dictionary<,>))
                        genericType = typeof(KeyValuePair<object, object>);
                    else
#endif
                    genericType = list.GetType().GetGenericArguments().FirstOrDefault() as Type;

                    if (genericType != null)
                    {
                        ItemPropertiesSet = !(genericType == typeof(Object));
#if WPF
                        this.itemProperties = TypeDescriptor.GetProperties(genericType);
#else
                        this.itemProperties = new PropertyInfoCollection(genericType);
#endif
                    }
                }

                if (!ItemPropertiesSet)
                {
                    //var prop = list.GetType().GetProperty("Item");
                    var prop = list.GetItemPropertyInfo();
                    if (prop != null)
                    {
                        ItemPropertiesSet = !(prop.PropertyType == typeof(Object));
#if WPF
                        this.itemProperties = TypeDescriptor.GetProperties(prop.PropertyType);
#else
                        this.itemProperties = new PropertyInfoCollection(prop.PropertyType);
#endif
                    }
                }
            }

            this.propertyAccessProvider = this.CreateItemPropertiesProvider();
        }

        /// <summary>
        /// Occurs when record property is changed.
        /// </summary>
        public event PropertyChangedEventHandler RecordPropertyChanged;

        /// <summary>
        /// Occurs when node collection is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler NodeCollectionChanged;

        /// <summary>
        /// Occurs when source collection is changed.
        /// </summary>
        public event NotifyCollectionChangedEventHandler SourceCollectionChanged;

        #region INotifyPropertyChanged

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Raises the PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">propertyName.</param>
        protected void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies and property changes and do actions based on that.
        /// </summary>
        /// <param name="sender">the record.</param>
        /// <param name="e">PropertyChangedEventArgs.</param>
        /// <param name="treeNode">corresponding parent node.</param>
        internal virtual void NotifyPropertyChangedHandler(object sender, PropertyChangedEventArgs e, TreeNode treeNode = null)
        {
            if (IsInSuspend)
            {
                var view = this as TreeGridSelfRelationalView;
                if (view != null && e.PropertyName == view.parentPropertyName)
                {
                    view.CheckPrimaryKey();
                }
                return;
            }
            var propertyChangedArgs = e;
            var passesFilter = true;
            TreeNode childNode = null;
            if (liveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping && !sender.Equals(CurrentEditItem))
            {
                if (treeNode == null)
                    childNode = FindNodefromData(null, sender);
                else
                {
                    childNode = treeNode.ChildNodes.GetNode(sender);
                }
                if (childNode != null && CanFilterNode(childNode))
                {
                    passesFilter = this.FilterItem(sender);
                    ApplyFilter(childNode, !passesFilter);
                }
                // Property changed will be fired for child items also. In that case, node will be null.
                else if (CanFilter && childNode == null)
                {
                    var parentNode = treeNode;
                    // Self-relational view
                    if (parentNode == null)
                    {
                        var view = this as TreeGridSelfRelationalView;
                        if (view != null)
                        {
                            object childValue = propertyAccessProvider.GetValue(sender, view.childPropertyName);
                            parentNode = view.FindParentNode(this.Nodes.RootNodes, childValue);
                        }
                    }
                    if (parentNode != null)
                    {
                        // If FilterLevel is Root, no need to update HasVisibleChildNodes based on filtering.
                        if (FilterLevel != FilterLevel.Root && !UpdateHasVisibleChildNodesBasedOnChildItem(parentNode, sender))
                            parentNode.isVisibleChildNodeAvailabilityChecked = false;
                        UpdateParentNode(parentNode);
                    }
                }

                var hasSort = this.SortDescriptions != null && this.SortDescriptions.Count > 0 &&
                            this.SortDescriptions.FirstOrDefault(
                                s => s.ColumnName == propertyChangedArgs.PropertyName) != null;
                if (hasSort)
                {
                    if (childNode != null)
                    {
                        var parentNode = childNode.ParentNode;
                        TreeNodes nodes;
                        if (parentNode != null)
                            nodes = parentNode.ChildNodes;
                        else
                            nodes = Nodes.RootNodes;
                        var removeAtIndex = nodes.IndexOfNode(sender);
                        var comparerIndex = hasSort ? GetComparerIndex(nodes, childNode, removeAtIndex, true, SortComparer) : removeAtIndex;
                        if (comparerIndex != removeAtIndex)
                        {
                            MoveNodesOnPropertyChange(parentNode, removeAtIndex, comparerIndex);
                        }
                    }
                }
            }
            if (!sender.Equals(CurrentEditItem))
                UpdateNodesOnPropertyChange(sender, e, treeNode);
            if (CanFilter && childNode != null && !sender.Equals(CurrentEditItem) && liveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping)
            {
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            this.OnRecordPropertyChanged(sender, propertyChangedArgs);
        }
        #endregion

        /// <summary>
        /// Raises the node collection changed event.
        /// </summary>
        /// <param name="args">NotifyCollectionChangedEventArgs.</param>
        protected void RaiseNodeCollectionChangedEvent(NotifyCollectionChangedEventArgs args)
        {
            if (deferRefreshCount > -1 && !IsInEndeferal && isNodeCollectionChangedSuspended)
                return;
            if (this.NodeCollectionChanged != null)
                this.NodeCollectionChanged(this, args);
            isNodeCollectionChanged = true;
        }

        internal bool isNodeCollectionChanged = false;

        private void RaiseSourceCollectionChangedEvent(NotifyCollectionChangedEventArgs args)
        {
            if (this.SourceCollectionChanged != null)
                this.SourceCollectionChanged(this, args);
        }

        private void UnwireEvents()
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.CollectionChange))
                return;
            if (this.source is INotifyCollectionChanged)
            {
                var notifyCollectionChanged = source as INotifyCollectionChanged;
                notifyCollectionChanged.CollectionChanged -= SourceNotifyCollectionChanged;
            }
        }

        private void WireEvents()
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.CollectionChange))
                return;
            if (this.source is INotifyCollectionChanged)
            {
                var notifyCollectionChanged = source as INotifyCollectionChanged;
                notifyCollectionChanged.CollectionChanged += SourceNotifyCollectionChanged;
            }
        }

        private bool _isNotifyPropertyTagged = false;
        internal void WireNotifyPropertyChangedForUnderlyingSource(bool foreceupdate)
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                return;
            if (foreceupdate) _isNotifyPropertyTagged = false;
            if (_isNotifyPropertyTagged || SourceCollection == null)
                return;

            _isNotifyPropertyTagged = true;
            AddNotifyListeners(SourceCollection);
        }

        /// <summary>
        /// Add the property changed event to particular record.
        /// </summary>
        /// <param name="record">the record.</param>
        protected internal void AddNotifyListener(object record)
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

        /// <summary>
        /// Flag used to suspend node collection changed event on data manipulations when filter level is extended.
        /// </summary>
        internal bool isNodeCollectionChangedSuspended = false;

        /// <summary>
        /// Suspends the node collection changed event when filtering is applied and filter level is Extended.
        /// </summary>
        internal void SuspendNodeCollectionEvent()
        {
            if (CanFilter && FilterLevel == FilterLevel.Extended)
                isNodeCollectionChangedSuspended = true;
        }

        /// <summary>
        /// Reset isNodeCollectionChangedSuspended flag and raises NodeCollectionChanged event.
        /// </summary>
        internal void ResumeNodeCollectionEvent()
        {
            if (CanFilter && FilterLevel == FilterLevel.Extended)
            {
                isNodeCollectionChangedSuspended = false;
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
        }

        /// <summary>
        /// Method which handles the collection changes.
        /// </summary>
        /// <param name="sender">sender.</param>
        /// <param name="e">NotifyCollectionChangedEventArgs.</param>
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {

        }

        private void SourceNotifyCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (deferRefreshCount > -1 || IsInSuspend)
            {
                if (IsInSuspend && e.NewItems != null && e.NewItems.Count > 0)
                {
                    var view = this as TreeGridSelfRelationalView;
                    if (view != null)
                        view.CheckPrimaryKey();
                }
                return;
            }
            if (!ItemPropertiesSet)
                this.SetItemProperties(this.source);
#if WPF
            if (this.DispatchOwner != null)
            {
                if (DispatchOwner.Thread != Thread.CurrentThread)
                {
                    DispatchOwner.Invoke(new Action(() => SourceNotifyCollectionChanged(sender, e)));
                    return;
                }
            }
#endif
            RaiseSourceCollectionChangedEvent(e);
            OnCollectionChanged(sender, e);
        }

        protected internal TreeNode CreateTreeNode(object item, int level, bool isExpanded, TreeNode parentNode, Func<TreeGridView> getView)
        {
            var childNode = new TreeNode(item, level, isExpanded, parentNode, getView);
            Suspend();
            UpdateIsCheckedState(parentNode, childNode);
            Resume();
            if (CanFilterNode(childNode))
            {
                FilterNode(childNode);
            }
            return childNode;
        }

        private LiveNodeUpdateMode liveNodeUpdateMode = LiveNodeUpdateMode.Default;

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.TreeGrid.LiveNodeUpdateMode"/> to control data manipulation operations during data updates.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.LiveNodeUpdateMode"/> that indicates how data manipulation operations are handled during data updates. 
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.LiveNodeUpdateMode.Default"/>.
        /// </value>
        public LiveNodeUpdateMode LiveNodeUpdateMode
        {
            get { return liveNodeUpdateMode; }
            set { liveNodeUpdateMode = value; }
        }

        private bool enableRecursiveChecking = false;

        /// <summary>
        /// Gets or sets a value which specifies whether recursive node checking is enabled or not. If recursive node checking enabled, node check box state will be changed based on its child or parent check box state.
        /// </summary>
        /// <value>
        /// The default value is False.
        /// </value>
        public bool EnableRecursiveChecking
        {
            get { return enableRecursiveChecking; }
            set { enableRecursiveChecking = value; }
        }

        /// <summary>
        /// Gets or Sets a value that indicate whether source collection items can listen the NotifyPropertyChanging/Changed events and CollectionChanged event.
        /// </summary>
        internal NotificationSubscriptionMode NotificationSubscriptionMode { get; set; }

        internal FilterLevel FilterLevel { get; set; }

        internal string CheckBoxMappingName;

        private SortColumnDescriptions sortDescriptions = null;
        /// <summary>
        /// Gets the collection of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> objects to sort the data programmatically.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> object to sort the data programmatically.The default value is <b>null</b>.
        /// </value>
        public virtual SortColumnDescriptions SortDescriptions
        {
            get { return sortDescriptions; }
        }

        private SortComparers sortComparers = null;
        /// <summary>
        /// Gets the collection of comparer to sort the data based on custom logic .
        /// </summary>
        /// <remarks>
        /// A comparer that are added to <b>SortComparers</b> collection to apply custom Sorting based on the specified column name and sort direction.
        /// </remarks>
        public virtual SortComparers SortComparers
        {
            get { return sortComparers; }
        }

        private IComparer<TreeNode> indexComparer = null;
        internal IComparer<TreeNode> IndexComparer
        {
            get
            {
                if (indexComparer == null)
                    return new IndexComparer(this);
                return indexComparer;
            }
        }

        #region Filtering

        /// <summary>
        /// Returns true if filtering is applied in view.
        /// </summary>
        internal bool CanFilter
        {
            get { return !isInResetFilter && this.Filter != null; }
        }


        /// <summary>
        /// Returns true if filtering is applied in view and can apply filter on specific node.
        /// </summary>
        /// <param name="node">specific tree node.</param>
        /// <returns>specifies whether filtering can be applied.</returns>
        internal bool CanFilterNode(TreeNode node)
        {
            if (CanFilter && (this.FilterLevel != FilterLevel.Root || node.Level == 0))
                return true;
            return false;
        }

        private Predicate<object> filter;

        /// <summary>
        /// Gets or sets the func to apply custom filter on source programmatically.
        /// </summary>
        public Predicate<object> Filter
        {
            get { return filter; }
            set
            {
                filter = value;
            }
        }

        /// <summary>
        /// Returns true if item passes the filter conditions.
        /// </summary>
        /// <param name="item">The item to be filtered.</param>
        /// <returns>Returns true when the item matches the filter criteria. Otherwise false.</returns>
        internal bool FilterItem(object item)
        {
            return (!CanFilter || this.Filter(item));
        }

        /// <summary>
        /// Refreshes the view when the filtering is applied through  <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.Filter"/> delegate.
        /// </summary>
        public void RefreshFilter()
        {
            if (deferRefreshCount > -1 && !IsInEndeferal)
                return;

            suspendIsFilteredUpdate = true;
            ApplyFilter(Nodes.RootNodes);
            suspendIsFilteredUpdate = false;
            SetDirtyAndResetCache();
            RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgsExt(NotifyCollectionChangedAction.Reset) { IsProgrammatic = true });
        }

        /// <summary>
        /// Flag used to reset IsFiltered value while changing filter level.
        /// </summary>
        internal bool isInResetFilter = false;

        /// <summary>
        /// Refreshes the view when FilterLevel property is changed if filtering is applied through  <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.Filter"/> delegate.
        /// </summary>
        internal void ResetFilter()
        {
            if (!CanFilter)
                return;
            isInResetFilter = true;
            suspendIsFilteredUpdate = true;
            ApplyFilter(Nodes.RootNodes);
            suspendIsFilteredUpdate = false;
            isInResetFilter = false;
            RefreshFilter();
        }

        /// <summary>
        /// When the node is filtered, we need to set dirty for that node and its parent node(s). While filtering all the nodes, no need to set dirty for its parent node(s), since parent node already gets filtered.To skip that, this flag is maintained.
        /// </summary>
        internal bool suspendIsFilteredUpdate = false;

        /// <summary>
        /// Apply filter for all the nodes in nodes collection and also apply filter to all level child nodes.
        /// </summary>
        /// <param name="nodes">specific nodes collection.</param>
        internal void ApplyFilter(TreeNodes nodes)
        {
            foreach (var treeNode in nodes)
            {
                treeNode.IsFiltered = !FilterItem(treeNode.Item);
                if (FilterLevel == FilterLevel.Root)
                {
                    SetFilteredState(treeNode, treeNode.IsFiltered);
                    continue;
                }
                ApplyFilterOnChildNodes(treeNode);
            }
        }

        /// <summary>
        /// Update HasVisibleChildNodes property of specific tree node based on its child nodes and if it does not have child node, set isVisibleChildNodeAvailabilityChecked as false.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        internal void UpdateHasVisibleChildNodes(TreeNode treeNode)
        {
            if (treeNode.ChildNodes.Any())
            {
                if (treeNode.ChildNodes.Any(n => !n.IsFiltered))
                    treeNode.HasVisibleChildNodes = true;
                else
                    treeNode.HasVisibleChildNodes = false;
            }
            else
            {
                treeNode.isVisibleChildNodeAvailabilityChecked = false;
            }
        }

        /// <summary>
        /// Update HasVisibleChildNodes property of specific tree node based on its child nodes if filtering is applied.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        internal void UpdateHasVisibleChildNodesBasedOnChildNodes(TreeNode treeNode)
        {
            if (CanFilterNode(treeNode))
            {
                if (treeNode.ChildNodes.Any(n => !n.IsFiltered))
                    treeNode.HasVisibleChildNodes = true;
            }
            else
                treeNode.HasVisibleChildNodes = true;
        }

        /// <summary>
        /// Update HasVisibleChildNodes property of specific tree node based on child item.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="item">child item.</param>
        /// <returns>HasVisibleChildNodes.</returns>
        internal bool UpdateHasVisibleChildNodesBasedOnChildItem(TreeNode treeNode, object item)
        {
            if (FilterItem(item))
            {
                treeNode.HasVisibleChildNodes = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Update HasVisibleChildNodes property of specific tree node based on child items.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="items">child items.</param>
        /// <returns>HasVisibleChildNodes.</returns>
        internal bool UpdateHasVisibleChildNodesBasedOnChildItems(TreeNode treeNode, IEnumerable items)
        {
            if (CanFilterNode(treeNode) && FilterLevel != FilterLevel.Root)
            {
                foreach (var item in items)
                {
                    if (UpdateHasVisibleChildNodesBasedOnChildItem(treeNode, item))
                        return true;
                }
                return false;
            }
            return true;
        }

        /// <summary>
        /// Set IsFiltered State as True for all child nodes of the specific node recursively and update HasVisibleChildNodes.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        internal void SetIsFilteredState(TreeNode treeNode)
        {
            foreach (var node in treeNode.ChildNodes)
            {
                node.IsFiltered = true;
                SetIsFilteredState(node);
            }
            UpdateHasVisibleChildNodes(treeNode);
        }

        /// <summary>
        /// Set IsFiltered State as True for all child nodes of the specific node recursively when FilterLevel is Root.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        internal void SetFilteredState(TreeNode treeNode, bool isFiltered)
        {
            foreach (var node in treeNode.ChildNodes)
            {
                node.IsFiltered = isFiltered;
                SetFilteredState(node, isFiltered);
            }
            // While changing filter level, filter will be refreshed. So HasVisibleChildNodes needs to be refreshed here.
            if (treeNode.ChildNodes.Any())
                treeNode.HasVisibleChildNodes = true;
            else
                treeNode.isVisibleChildNodeAvailabilityChecked = false;
        }

        /// <summary>
        /// Set IsFiltered value to specific tree node and apply filter for child nodes also.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        /// <param name="isFiltered">isFiltered value.</param>
        internal void ApplyFilter(TreeNode treeNode, bool isFiltered)
        {
            treeNode.IsFiltered = isFiltered;
            SetDirtyAndResetCache();
            ApplyFilterOnChildNodes(treeNode);
            var parentNode = treeNode.ParentNode;
            if (parentNode != null)
            {
                if (treeNode.IsFiltered)
                {
                    UpdateParentNodeIsFilteredBasedOnChildNode(parentNode);
                }
                else
                {
                    parentNode.HasVisibleChildNodes = true;
                }
            }
        }

        /// <summary>
        /// Apply filter for child nodes of the specific tree node.
        /// </summary>
        /// <param name="treeNode">specific tree node.</param>
        internal void ApplyFilterOnChildNodes(TreeNode treeNode)
        {
            if (treeNode.IsFiltered)
            {
                if (FilterLevel != FilterLevel.Extended)
                {
                    SetIsFilteredState(treeNode);
                }
                else
                    ApplyFilter(treeNode.ChildNodes);
            }
            else
                ApplyFilter(treeNode.ChildNodes);
            UpdateHasVisibleChildNodes(treeNode);
        }

        #endregion

        /// <summary>
        /// Expand particular TreeNode.
        /// </summary>
        /// <param name="treeNode">Tree Node.</param>
        /// <param name="reExpand">if it is true, node may be in expanded state/collapsed state. And it will have child nodes. In this case, need to populate child nodes only.</param>
        /// <returns>number of nodes to be added.</returns>
        internal int ExpandNode(TreeNode treeNode, bool reExpand = false)
        {
            int itemcount = 0;
            if (!treeNode.IsExpanded || reExpand)
            {
                if (!reExpand)
                    treeNode.IsExpanded = true;
                RequestTreeItems(treeNode);
                SetDirtyAndResetCache();
                itemcount = treeNode.GetYAmountCache() - 1;
            }
            return itemcount;
        }

        internal int CollapseNode(TreeNode treeNode)
        {
            int itemcount = 0;
            if (!treeNode.IsExpanded)
                return itemcount;
            itemcount = treeNode.GetYAmountCache() - 1;
            treeNode.IsExpanded = false;
            SetDirtyAndResetCache();
            return itemcount;
        }

        internal int CollapseAllNodes(TreeNode treeNode)
        {
            int itemcount = 0;
            itemcount = treeNode.GetYAmountCache() - 1;
            treeNode.IsExpanded = false;
            CollapseChildNodes(treeNode.ChildNodes);
            SetDirtyAndResetCache();
            return itemcount;
        }

        internal int ExpandAllNodes(TreeNode treeNode, int level = -1)
        {
            int itemcount = 0;
            treeNode.IsExpanded = true;
            RequestTreeItems(treeNode);
            if ((level != -1 && level > 0) || level == -1)
                ExpandChildNodes(treeNode.ChildNodes, level);
            else
                CollapseChildNodes(treeNode.ChildNodes);
            SetDirtyAndResetCache();
            itemcount = treeNode.GetYAmountCache() - 1;
            return itemcount;
        }

        internal void RequestTreeItems(TreeNode treeNode)
        {
            var args = new TreeGridRequestTreeItemsEventArgs(treeNode, treeNode.Item, false, true);
            RequestTreeItems(args);
        }

        private void ExpandChildNodes(TreeNodes childNodes, int level = -1)
        {
            foreach (var childNode in childNodes)
            {
                if (!childNode.IsExpanded)
                    childNode.IsExpanded = true;
                RequestTreeItems(childNode);

                if (level != -1 && childNode.Level < level || level == -1)
                    ExpandChildNodes(childNode.ChildNodes, level);
                else
                    CollapseChildNodes(childNode.ChildNodes);
            }
        }

        private void CollapseChildNodes(TreeNodes childNodes)
        {
            foreach (var childNode in childNodes)
            {
                if (childNode.IsExpanded)
                    childNode.IsExpanded = false;
                CollapseChildNodes(childNode.ChildNodes);
            }
        }

        /// <summary>
        /// Removes the specified items from the child collection of the particular node.
        /// </summary>
        /// <param name="treeNode">parent Node.</param>
        /// <param name="items">items.</param>
        internal void RemoveNodes(TreeNode treeNode, IList items)
        {
            foreach (var item in items)
            {
                RemoveNode(treeNode, item);
            }
        }

        protected internal virtual void RemoveNode(TreeNode parentNode, object data)
        {
            TreeNodes childNodes;
            if (parentNode != null)
                childNodes = parentNode.ChildNodes;
            else
                childNodes = Nodes.RootNodes;
            var node = childNodes.GetNode(data);
            var index = this.Nodes.IndexOf(node);
            var changedItems = new List<TreeNode>();
            changedItems.Add(node);
            if (node.IsExpanded)
            {
                RemoveNodes(node.ChildNodes, ref changedItems);
            }
            childNodes.Remove(node);
            if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                RemoveNotifyListener(data);
            RaiseRemoveNotifyChangedEvent(changedItems, index, parentNode);
        }

        internal virtual void UpdateExpander(TreeNode node)
        {

        }

        internal virtual void UpdateParentNode(TreeNode node)
        {

        }

        /// <summary>
        /// Removed the filtered nodes (IsFiltered - True) from changed items.
        /// </summary>
        /// <param name="changedItems">changed items.</param>
        internal void RemoveFilteredNodes(List<TreeNode> changedItems)
        {
            if (CanFilter)
                changedItems.RemoveAll(n => n.IsFiltered);
        }

        internal void RemoveChildNodes(TreeNode treeNode)
        {
            if (!treeNode.ChildNodes.Any())
                return;
            var changedItems = new List<TreeNode>();
            var startIndex = this.Nodes.IndexOf(treeNode.ChildNodes[0]);
            if (treeNode.IsExpanded)
            {
                foreach (var node in treeNode.ChildNodes)
                {
                    changedItems.Add(node);
                    if (node.IsExpanded)
                    {
                        RemoveNodes(node.ChildNodes, ref changedItems, true);
                    }
                    if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                        RemoveNotifyListener(node.Item);
                }
            }
            treeNode.ChildNodes.Clear();
            UpdateExpander(treeNode);
            treeNode.SetDirtyOnExpandOrCollapse();
            SetDirtyAndResetCache();
            RemoveFilteredNodes(changedItems);
            if (startIndex >= 0)
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, startIndex));
        }

        internal void SetDirtyAndResetCache()
        {
            this.Nodes.ResetCache = true;
            this.Nodes.isDirty = true;
        }

        /// <summary>
        /// Moves the child node from an index to the another index in child nodes of particular tree node.
        /// </summary>
        /// <param name="treeNode">tree Node.</param>
        /// <param name="oldStartingIndex">oldStartingIndex.</param>
        /// <param name="newStartingIndex">newStartingIndex.</param>
        protected internal virtual void MoveNode(TreeNode treeNode, int oldStartingIndex, int newStartingIndex)
        {
            var changedItems = new List<TreeNode>();
            TreeNodes nodes = null;
            if (treeNode == null)
                nodes = this.Nodes.RootNodes;
            else
                nodes = treeNode.ChildNodes;
            var startIndex = this.Nodes.IndexOf(nodes[oldStartingIndex]);

            var node = nodes[oldStartingIndex];
            if (!this.SortDescriptions.Any())
            {
                nodes.MoveTo(oldStartingIndex, newStartingIndex);
            }
            else
            {
                nodes.sourceList.MoveTo(oldStartingIndex, newStartingIndex);
            }
            this.Nodes.ResetCache = true;
            var newStartIndex = this.Nodes.IndexOf(nodes[newStartingIndex]);
            changedItems.Add(node);
            RemoveFilteredNodes(changedItems);
            // need to skip this while applying sorting
            // if index is -1, exception will be thrown in Move case.
            if (!this.SortDescriptions.Any() && newStartIndex != -1 && startIndex != -1)
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItems, newStartIndex, startIndex));
        }

        /// <summary>
        /// Move nodes on property change based on Sorting 
        /// </summary>
        /// <param name="treeNode">treeNode</param>
        /// <param name="oldStartingIndex">oldStartingIndex</param>
        /// <param name="newStartingIndex">newStartingIndex</param>
        private void MoveNodesOnPropertyChange(TreeNode treeNode, int oldStartingIndex, int newStartingIndex)
        {
            var changedItems = new List<TreeNode>();
            TreeNodes nodes = null;
            if (treeNode == null)
                nodes = this.Nodes.RootNodes;
            else
                nodes = treeNode.ChildNodes;
            if (nodes.Count != 0)
            {
                var startIndex = this.Nodes.IndexOf(nodes[oldStartingIndex]);
                var node = nodes[oldStartingIndex];
                nodes.nodeList.MoveTo(oldStartingIndex, newStartingIndex);
                this.Nodes.ResetCache = true;
                var newStartIndex = this.Nodes.IndexOf(nodes[newStartingIndex]);
                changedItems.Add(node);
                RemoveFilteredNodes(changedItems);
                if (startIndex >= 0)
                    RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, changedItems, newStartIndex, startIndex));
            }
        }

        /// <summary>
        /// Replaces the items at specified index with the new items in child collection of the particular tree node. 
        /// </summary>
        /// <param name="treeNode">treeNode.</param>
        /// <param name="oldItems"></param>
        /// <param name="newItems"></param>
        /// <param name="newStartingIndex"></param>
        protected virtual void ReplaceNode(TreeNode treeNode, IList oldItems, IList newItems, int newStartingIndex)
        {
            var changedItems = new List<TreeNode>();
            TreeNodes nodes = null;
            if (treeNode == null)
                nodes = this.Nodes.RootNodes;
            else
                nodes = treeNode.ChildNodes;

            var oldNode = nodes.GetNode(oldItems[0]);
            int oldIndex = nodes.IndexOf(oldNode);
            var startIndex = this.Nodes.IndexOf(oldNode);
            if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                RemoveNotifyListener(oldNode.Item);
            var newNode = CreateTreeNode(newItems[0], treeNode != null ? treeNode.Level + 1 : 0, false, treeNode, GetView);
            if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                AddNotifyListener(newNode.Item);

            if (this.CanFilter || (this.SortDescriptions.Any() && LiveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping))
            {
                nodes.nodeList.RemoveAt(oldIndex);
                changedItems.Add(oldNode);
                if (oldNode.IsExpanded)
                {
                    RemoveNodes(oldNode.ChildNodes, ref changedItems);
                }
                if (treeNode != null)
                    treeNode.SetDirtyOnExpandOrCollapse();
                SetDirtyAndResetCache();
                RemoveFilteredNodes(changedItems);
                if (startIndex >= 0)
                    RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, startIndex));
                var nodeIndex = this.GetComparerIndex(nodes.nodeList, newNode, newStartingIndex, SortComparer);
                // While replacing node, we can remove and add node from nodeList only. In sourceList, we can update the item directly.           
                nodes.Insert(nodeIndex, newNode);
                nodes.sourceList[newStartingIndex] = newNode.Item;
                changedItems.Clear();
                changedItems.Add(newNode);
                if (treeNode != null)
                    treeNode.SetDirtyOnExpandOrCollapse();
                SetDirtyAndResetCache();
                startIndex = this.Nodes.IndexOf(newNode);
                RemoveFilteredNodes(changedItems);
                if (treeNode != null)
                {
                    if (treeNode.ChildNodes.Any(n => !n.IsFiltered))
                        treeNode.HasVisibleChildNodes = true;
                    else
                        treeNode.HasVisibleChildNodes = false;
                    UpdateParentNode(treeNode);
                }

                if (startIndex >= 0)
                    RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, startIndex));
            }
            else
            {
                nodes[newStartingIndex] = newNode;
                SetDirtyAndResetCache();
                startIndex = this.Nodes.IndexOf(newNode);
                if (startIndex >= 0)
                    RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newNode, oldNode, startIndex));
            }
            ValidateCheckBoxPropertyValue(nodes);
        }

        protected virtual void ResetNodes(TreeNode parentNode)
        {
            ResetNodes(parentNode, true);
        }

        /// <summary>
        /// Reset nodes.
        /// </summary>       
        internal void ResetNodes(TreeNode parentNode, bool removeListener = true)
        {
            TreeNodes nodes = null;
            if (parentNode == null)
                nodes = this.Nodes.RootNodes;
            else
                nodes = parentNode.ChildNodes;
            if (!nodes.Any())
                return;
            if (removeListener)
                foreach (var node in nodes)
                {
                    if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                        RemoveNotifyListener(node.Item);
                    ResetChildNodes(node);
                }
            nodes.Clear();
            if (parentNode != null)
            {
                UpdateExpander(parentNode);
                parentNode.SetDirtyOnExpandOrCollapse();
                //WPF-35839 - Update parent node IsFiltered value if filter level is Extended.
                UpdateParentNodeIsFilteredBasedOnChildNode(parentNode);
            }
            SetDirtyAndResetCache();
            RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
        internal void ResetChildNodes(TreeNode node)
        {
            if (node.ChildNodes.Any())
            {
                foreach (var childNode in node.ChildNodes)
                {
                    if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                        RemoveNotifyListener(childNode.Item);
                    ResetChildNodes(childNode);
                }
            }
            node.ChildNodes.Clear();
        }

        /// <summary>
        /// Remove nodes from nodes collection.
        /// </summary>
        /// <param name="nodes">nodes.</param>
        /// <param name="changedItems">changedItems.</param>
        /// <param name="completeRemove">if it is false, changedItems only will be updated. If it is true, nodes will be removed.</param>
        internal void RemoveNodes(TreeNodes nodes, ref List<TreeNode> changedItems, bool completeRemove = true)
        {
            foreach (var node in nodes)
            {
                changedItems.Add(node);
                if (node.IsExpanded)
                {
                    RemoveNodes(node.ChildNodes, ref changedItems, completeRemove);
                }
                if (completeRemove)
                {
                    if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                        RemoveNotifyListener(node.Item);
                }
            }
            //if (completeRemove)
            //    nodes.Dispose();
        }

        /// <summary>
        /// Add the data to the child collection of particular tree node at a specified index.
        /// </summary>
        /// <param name="treeNode">parentNode. If root nodes needs to added, parent node will be null.</param>
        /// <param name="newItems">newItems.</param>
        /// <param name="startingIndex">startingIndex.</param>      
        internal void AddNodes(TreeNode treeNode, IList newItems, int startingIndex)
        {
            int location = startingIndex;
            int newIndex = location;
            TreeNodes nodes;
            if (treeNode != null)
                nodes = treeNode.ChildNodes;
            else
                nodes = Nodes.RootNodes;

            var changedItems = new List<TreeNode>();
            var raiseEventForEachItem = this.SortDescriptions.Any() && LiveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping;
            foreach (var item in newItems)
            {
                if (raiseEventForEachItem)
                {
                    AddNode(treeNode, item, location);
                }
                else
                {
                    var node = AddNode(treeNode, item, location, ref newIndex);
                    changedItems.Add(node);
                }
                location++;
            }
            if (!raiseEventForEachItem)
                RaiseAddNotifyChangedEvent(changedItems, newIndex, treeNode, nodes);
        }

        /// <summary>
        /// Remove particular node from its parent node's child collection.
        /// </summary>
        /// <param name="treeNode">treeNode to be removed.</param>
        internal void RemoveNode(TreeNode treeNode)
        {
            TreeNodes nodes;
            var parentNode = treeNode.ParentNode;
            if (parentNode != null)
                nodes = parentNode.ChildNodes;
            else
                nodes = Nodes.RootNodes;
            var changedItems = new List<TreeNode>();
            var startIndex = this.Nodes.IndexOf(treeNode);
            nodes.Remove(treeNode);
            changedItems.Add(treeNode);
            if (treeNode.IsExpanded)
            {
                foreach (var node in treeNode.ChildNodes)
                {
                    changedItems.Add(node);
                    if (node.IsExpanded)
                    {
                        RemoveNodes(node.ChildNodes, ref changedItems, false);
                    }
                }
            }
            RaiseRemoveNotifyChangedEvent(changedItems, startIndex, parentNode);
        }

        /// <summary>
        /// Update parent node IsFiltered value based on its child node if filter level is Extended.
        /// </summary>        
        /// <param name="parentNode">parent node.</param>
        internal void UpdateParentNodeIsFilteredBasedOnChildNode(TreeNode parentNode)
        {
            if (parentNode.ChildNodes.Any(c => !c.IsFiltered))
                return;
            parentNode.HasVisibleChildNodes = false;
            if (FilterLevel == FilterLevel.Extended)
            {
                if (!parentNode.IsFiltered)
                {
                    if (CanFilterNode(parentNode))
                    {
                        var passesFilter = FilterItem(parentNode.Item);
                        if (!passesFilter)
                            ApplyFilter(parentNode, !passesFilter);
                    }
                }
            }
        }

        /// <summary>
        /// Update parent node's HasVisibleChildNodes and IsFiltered property based on child node's IsFiltered value.
        /// </summary>
        /// <param name="treeNode">child node.</param>
        /// <param name="parentNode">parent node.</param>
        internal void UpdateParentNodeOnPropertyChange(TreeNode treeNode, TreeNode parentNode)
        {
            if (treeNode.IsFiltered)
                return;
            parentNode.HasVisibleChildNodes = true;
            if (FilterLevel == FilterLevel.Extended)
            {
                parentNode.IsFiltered = treeNode.IsFiltered;
                SetDirtyAndResetCache();
            }
        }

        /// <summary>
        /// Add particular node as child of the parent node in the specified index.
        /// </summary>
        /// <param name="parentNode">parent node.</param>
        /// <param name="treeNode">node needs to be added as child.</param>
        /// <param name="nodeIndex">index to insert the node in nodeList.</param>
        /// <param name="sourceIndex">if AllowDataShaping should not be considered, sourceIndex will be passed. In sourceList, based on sourceIndex item will be inserted. In NodeList, based on nodeIndex, node will be inserted.</param>
        /// <param name="needDataShaping">specifies whether AllowDataShaping needs to be considered in sorting.</param>
        public void AddNode(TreeNode parentNode, TreeNode treeNode, int nodeIndex, int sourceIndex = -1, bool needDataShaping = true)
        {
            int location = nodeIndex;
            int newStartIndex = location;
            TreeNodes nodes;
            if (parentNode != null)
                nodes = parentNode.ChildNodes;
            else
                nodes = Nodes.RootNodes;
            var changedItems = new List<TreeNode>();

            treeNode.Level = parentNode != null ? parentNode.Level + 1 : 0;
            treeNode.ParentNode = parentNode;
            UpdateIsCheckedState(parentNode, treeNode);
            if (needDataShaping && this.SortDescriptions.Any() && LiveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping)
            {
                var newIndex = GetComparerIndex(nodes.nodeList, treeNode, nodeIndex, SortComparer);
                nodes.Insert(location, treeNode, newIndex);
                newStartIndex = newIndex;
            }
            else
            {
                int newIndex = location;
                var treeView = this as TreeGridSelfRelationalView;
                if (treeView != null)
                    newIndex = GetComparerIndex(nodes.nodeList, treeNode, location, treeView.IndexComparer);
                if (sourceIndex != -1)
                    nodes.Insert(sourceIndex, treeNode, newIndex);
                else
                    nodes.Insert(newIndex, treeNode, newIndex);
                newStartIndex = newIndex;
            }
            foreach (var n in treeNode.ChildNodes)
            {
                n.Level = treeNode.Level + 1;
                ResetLevel(n);
            }

            changedItems.Add(treeNode);
            if (treeNode.IsExpanded)
            {
                foreach (var node in treeNode.ChildNodes)
                {
                    changedItems.Add(node);
                    if (node.IsExpanded)
                    {
                        RemoveNodes(node.ChildNodes, ref changedItems, false);
                    }
                }
            }
            if (this.CanFilterNode(treeNode))
            {
                var passesFilter = this.FilterItem(treeNode.Item);
                ApplyFilter(treeNode, !passesFilter);
            }
            else
            {
                treeNode.IsFiltered = false;
            }

            RaiseAddNotifyChangedEvent(changedItems, newStartIndex, parentNode, nodes);
        }


        /// <summary>
        /// Add node to parent node from data.
        /// </summary>
        /// <param name="parentNode">parentNode.</param>
        /// <param name="childNodes">childNodes.</param>
        /// <param name="index">source index.</param>
        /// <param name="data">data.</param>
        /// <param name="newIndex">actual inserted index.</param>
        /// <param name="needDataShaping">specifies whether AllowDataShaping needs to be considered in sorting.</param>
        /// <returns>added tree node.</returns>
        private TreeNode AddNode(TreeNode parentNode, object data, int index, ref int newIndex, bool needDataShaping = true)
        {
            TreeNodes childNodes;
            if (parentNode != null)
                childNodes = parentNode.ChildNodes;
            else
                childNodes = Nodes.RootNodes;

            var node = CreateTreeNode(data, parentNode != null ? parentNode.Level + 1 : 0, false, parentNode, GetView);
            if (!NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) && NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                AddNotifyListener(data);
            if (needDataShaping && this.SortDescriptions.Any() && LiveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping)
            {
                var nodeIndex = this.GetComparerIndex(childNodes.nodeList, node, index, SortComparer);
                int sourceIndex = index;
                var treeView = this as TreeGridSelfRelationalView;
                if (treeView != null)
                    sourceIndex = GetComparerIndex(childNodes.nodeList, node, index, treeView.IndexComparer);
                //node = AddNode(childNodes, node, sourceIndex, ref nodeIndex);
                childNodes.Insert(sourceIndex, node, nodeIndex);
                newIndex = nodeIndex;
            }
            else
            {
                var treeView = this as TreeGridSelfRelationalView;
                newIndex = index;
                if (treeView != null)
                    newIndex = GetComparerIndex(childNodes.nodeList, node, index, treeView.IndexComparer);
                //node = AddNode(childNodes, node, newIndex, ref newIndex);
                childNodes.Insert(newIndex, node, newIndex);
            }
            ValidateCheckBoxPropertyValue(childNodes);
            return node;
        }

        /// <summary>
        /// Add node to parent node from data.
        /// </summary>
        /// <param name="parentNode">parentNode.</param>
        /// <param name="childNodes">childNodes.</param>
        /// <param name="index">Index to insert the node in child nodes collection of the specified parent node.</param>
        /// <param name="data">data.</param>
        /// <param name="needDataShaping">specifies whether AllowDataShaping needs to be considered in sorting.</param>
        protected internal virtual void AddNode(TreeNode parentNode, object data, int index, bool needDataShaping = true)
        {
            int newIndex = index;
            TreeNodes childNodes;
            if (parentNode != null)
                childNodes = parentNode.ChildNodes;
            else
                childNodes = Nodes.RootNodes;
            var changedItems = new List<TreeNode>();
            var node = AddNode(parentNode, data, index, ref newIndex, needDataShaping);
            changedItems.Add(node);
            RaiseAddNotifyChangedEvent(changedItems, newIndex, parentNode, childNodes);
        }

        /// <summary>
        /// Raise NodeCollectionChangedEvent while adding nodes.
        /// </summary>
        /// <param name="changedItems">added items.</param>
        /// <param name="index">start index.</param>
        /// <param name="parentNode">parent Node.</param>
        /// <param name="childNodes">childNodes collection.</param>
        internal void RaiseAddNotifyChangedEvent(List<TreeNode> changedItems, int index, TreeNode parentNode, TreeNodes childNodes)
        {
            bool needToUpdateExpander = false;
            if (parentNode != null)
            {
                if (parentNode.ChildNodes.Any())
                {
                    parentNode.HasChildNodes = true;
                    UpdateHasVisibleChildNodesBasedOnChildNodes(parentNode);
                    needToUpdateExpander = true;
                }
                parentNode.SetDirtyOnExpandOrCollapse();
            }
            SetDirtyAndResetCache();
            int nodeIndex = this.Nodes.IndexOf(childNodes[index]);
            if (needToUpdateExpander)
            {
                if (nodeIndex >= 0)
                    UpdateExpander(parentNode);
                else
                    UpdateParentNode(parentNode);
            }
            RemoveFilteredNodes(changedItems);
            if (nodeIndex >= 0)
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, changedItems, nodeIndex));
        }

        internal void RaiseRemoveNotifyChangedEvent(List<TreeNode> changedItems, int startIndex, TreeNode parentNode)
        {
            if (parentNode != null)
            {
                if (!parentNode.ChildNodes.Any())
                {
                    parentNode.SetHasChildNodes(false);
                }

                if (!parentNode.HasChildNodes || !parentNode.ChildNodes.Any(n => !n.IsFiltered))
                {
                    parentNode.HasVisibleChildNodes = false;
                    if (changedItems.Any() && !changedItems[0].IsFiltered)
                        UpdateParentNodeIsFilteredBasedOnChildNode(parentNode);
                    if (startIndex >= 0)
                        UpdateExpander(parentNode);
                    else
                        UpdateParentNode(parentNode);
                }
                parentNode.SetDirtyOnExpandOrCollapse();
            }
            SetDirtyAndResetCache();
            RemoveFilteredNodes(changedItems);
            if (startIndex >= 0)
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, changedItems, startIndex));
        }

        /// <summary>
        /// Reset child node levels based on new parent node
        /// </summary>
        /// <param name="parentNode">parentNode</param>
        internal void ResetLevel(TreeNode parentNode)
        {
            if (parentNode.ChildNodes.Any())
            {
                foreach (var childNode in parentNode.ChildNodes)
                {
                    childNode.Level = parentNode.Level + 1;
                    ResetLevel(childNode);
                }
            }
        }

        #region DeferRefresh

        private IDisposable endDeferDisposable;
        /// <summary>
        /// Gets a value indicating whether this instance is in end defer.
        /// </summary>
        /// <value>
        /// 	<b>true</b> if this instance is in end defer; otherwise, <b>false</b>.
        /// </value>
        protected bool IsInEndeferal;
        internal int deferRefreshCount = -1;
        private TreeViewRefreshMode refreshMode;

        /// <summary>
        /// Enters a defer cycle that you can use to merge changes to the view and delay automatic refresh.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.IDisposable"/> object that you can use to dispose of the calling object.
        /// </returns>
        public IDisposable DeferRefresh(TreeViewRefreshMode refreshMode)
        {
            this.refreshMode = refreshMode;
            deferRefreshCount++;
            return new DeferHelper(this);
        }

        /// <summary>
        /// Gets a value which indicates whether view is in DeferRefresh.
        /// </summary>
        public bool IsInDeferRefresh
        {
            get { return deferRefreshCount != -1; }
        }

        /// <summary>
        /// Suspends the all data operations in View. 
        /// </summary>
        public void BeginInit(TreeViewRefreshMode refreshMode)
        {
            this.refreshMode = refreshMode;
            if (this.deferRefreshCount == -1)
                this.endDeferDisposable = this.DeferRefresh(refreshMode);
            else
                deferRefreshCount++;
        }

        /// <summary>
        /// Resumes the data operations and reinitialize the view.
        /// </summary>
        public void EndInit()
        {
            if (deferRefreshCount == 0 && this.endDeferDisposable != null)
            {
                this.endDeferDisposable.Dispose();
            }
            else if (deferRefreshCount != -1)
                deferRefreshCount--;
        }

        internal void EndDefer()
        {
            if (deferRefreshCount == 0)
            {
                this.EndDeferInternal();
            }
            this.deferRefreshCount--;
        }

        internal void EndDeferInternal()
        {
            IsInEndeferal = true;
            if (refreshMode == TreeViewRefreshMode.NodeRefresh)
            {
                this.RefreshSorting();
                RefreshFilter();
            }
            else
            {
                Refresh();
            }
            IsInEndeferal = false;
        }

        private void EnsureNodesInitialized()
        {
            if (this.refreshMode == TreeViewRefreshMode.DeferRefresh)
                this.EnsureInitialized();
        }

        private class DeferHelper : IDisposable
        {
            // Fields
            private TreeGridView treeGridView;
            // Methods
            public DeferHelper(TreeGridView _treeGridView)
            {
                this.treeGridView = _treeGridView;
            }

            public void Dispose()
            {
                if (this.treeGridView != null)
                {
                    this.treeGridView.EndDefer();
                    this.treeGridView = null;
                }
            }
        }

        #endregion

        /// <summary>
        /// Recreates the view.
        /// </summary>
        public void Refresh()
        {
            refreshMode = TreeViewRefreshMode.DeferRefresh;
            EnsureNodesInitialized();
            RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        IComparer<TreeNode> sortComparer = null;
        /// <summary>
        /// Gets a value which indicates the comparer which is used for sorting.
        /// </summary>
        protected internal IComparer<TreeNode> SortComparer
        {
            get
            {
                if (sortComparer == null)
                {
                    sortComparer = new TreeNodeComparer(this);
                }
                else
                {
                    var comparer = sortComparer as TreeNodeComparer;
                    comparer.hasComparer = SortComparers.Any();
                }
                return sortComparer;
            }
        }

#if WPF
        internal Dispatcher DispatchOwner { get; set; }
#endif

        /// <summary>
        /// Get the index by comparing nodes.
        /// </summary>
        /// <param name="nodes">nodes.</param>
        /// <param name="node">Tree node.</param>
        /// <param name="removeAtIndex">removeAtIndex.</param>
        /// <param name="removeitembeforecheck">removeitembeforecheck denotes whether need to remove item before checking index.</param>
        /// <param name="comparer">SortComparer.</param>
        /// <returns>index.</returns>
        internal int GetComparerIndex(TreeNodes nodes, TreeNode node, int removeAtIndex, bool removeitembeforecheck, IComparer<TreeNode> comparer)
        {
            var list = nodes.nodeList.ToList();
            if (removeitembeforecheck)
            {
                if (removeAtIndex >= 0 && removeAtIndex < nodes.Count)
                    list.RemoveAt(removeAtIndex);
            }
            return GetComparerIndex(list, node, removeAtIndex, comparer);
        }

        /// <summary>
        /// Get the index by comparing nodes.
        /// </summary>
        /// <param name="nodes">nodes list.</param>
        /// <param name="node">Tree node.</param>
        /// <param name="removeAtIndex">removeAtIndex.</param>      
        /// <param name="comparer">SortComparer or IndexComparer.</param>
        /// <returns>index.</returns>
        internal int GetComparerIndex(List<TreeNode> nodes, TreeNode node, int removeAtIndex, IComparer<TreeNode> comparer)
        {
            int comparerindex = InternalBinarySearch(nodes, removeAtIndex, node, comparer);
            if (comparerindex < 0)
                comparerindex = ~comparerindex;
            return comparerindex;
        }

        private int InternalBinarySearch(List<TreeNode> internalList, int removeatIndex, TreeNode value, IComparer<TreeNode> comparer)
        {
            int num = 0;
            int num2 = (internalList.Count) - 1;
            while (num <= num2)
            {
                int num3 = num + ((num2 - num) >> 1);
                int num4 = comparer.Compare(internalList[num3], value);
                if (num4 == 0)
                {
                    num4 = num3 - removeatIndex;
                    if (num4 == 0)
                    {
                        return num3;
                    }
                }
                if (num4 < 0)
                {
                    num = num3 + 1;
                }
                else
                {
                    num2 = num3 - 1;
                }
            }
            return ~num;
        }

        private void UnwireNotifyPropertyChangedForUnderlyingSource()
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                return;
            if (!_isNotifyPropertyTagged || SourceCollection == null)
                return;

            _isNotifyPropertyTagged = false;

            RemoveNotifyListeners(SourceCollection);
        }

        internal void RemoveNotifyListeners(IEnumerable collection)
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                return;
            foreach (var record in collection)
                RemoveNotifyListener(record);
        }

        internal void AddNotifyListeners(IEnumerable collection)
        {
            if (NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.None) || !NotificationSubscriptionMode.HasFlag(NotificationSubscriptionMode.PropertyChange))
                return;
            foreach (var record in collection)
                AddNotifyListener(record);
        }

        /// <summary>
        /// Removes the property changed event of particular record.
        /// </summary>
        /// <param name="record">the record.</param>
        protected internal void RemoveNotifyListener(object record)
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

#if WPF
        private void OnPropertyChanging(object sender, PropertyChangingEventArgs e)
        {
            if (IsDisposed)
                return;
        }
#endif

        /// <summary>
        /// Check TreeGridView on property changes and do the actions.
        /// </summary>
        /// <param name="sender">updated record.</param>
        /// <param name="e">PropertyChangedEventArgs.</param>
        protected internal virtual void UpdateNodesOnPropertyChange(object sender, PropertyChangedEventArgs e, TreeNode treeNode = null)
        {

        }


        internal void UpdateNodesOnCheckBoxPropertyChange(object sender, PropertyChangedEventArgs e, TreeNode treeNode)
        {
            if (e != null && e.PropertyName != CheckBoxMappingName)
                return;

            if (string.IsNullOrEmpty(CheckBoxMappingName))
                return;
            var isCheckedValue = propertyAccessProvider.GetValue(sender, CheckBoxMappingName);
            // If recursiveCheckingMode is OnCheck, suspend IsChecked Update.
            if (recursiveCheckingMode == RecursiveCheckingMode.OnCheck)
                suspendIsCheckedRecursiveUpdate = true;
            SetIsCheckedState(treeNode, (bool?)isCheckedValue);
            suspendIsCheckedRecursiveUpdate = false;
            if (recursiveCheckingMode == RecursiveCheckingMode.Default)
                treeNode.isCheckedChanged = true;
            else
                treeNode.isCheckedChanged = false;
        }
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (IsDisposed)
                return;
#if WPF
            if (this.DispatchOwner != null)
            {
                if (DispatchOwner.Thread != Thread.CurrentThread)
                {
                    DispatchOwner.Invoke(new Action(() => OnPropertyChanged(sender, e)));
                    return;
                }
            }
#endif
            NotifyPropertyChangedHandler(sender, e);
        }

        /// <summary>
        /// node which is used for bringing updated item into view when self relational view is used.
        /// </summary>
        internal TreeNode tempNode;
        internal TreeNode FindNodefromData(TreeNodes nodes, object record, bool checkRootNodesOnly = false)
        {
            if (nodes == null)
                nodes = Nodes.RootNodes;
            var treeNode = nodes.GetNode(record);
            if (checkRootNodesOnly)
                return treeNode;
            if (treeNode == null)
            {
                foreach (var node in nodes)
                {
                    if (node.IsExpanded || node.ChildNodes.Any())
                    {
                        treeNode = FindNodefromData(node.ChildNodes, record);
                        if (treeNode != null)
                            return treeNode;
                    }
                }
            }
            return treeNode;
        }

        /// <summary>
        /// Raises record property changed event.
        /// </summary>
        /// <param name="sender">the record.</param>
        /// <param name="e">the PropertyChangedEventArgs.</param>
        protected virtual void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var handler = this.RecordPropertyChanged;
            if (handler != null)
            {
                handler(sender, e);
            }
        }
        /// <summary>
        /// Sets the source collection.
        /// </summary>
        /// <param name="_source">_source.</param>
        protected virtual void SetSource(IEnumerable _source)
        {
            this.source = _source;
        }

        private IEnumerable source;
        /// <summary>
        /// Gets the source collection.
        /// </summary>
        /// <returns>the source collection.</returns>
        protected virtual IEnumerable GetSource()
        {
            return source;
        }

        /// <summary>
        /// Denotes whether data manipulation operation is suspended or not.
        /// </summary>

        protected internal bool IsInSuspend = false;

        /// <summary>
        /// Gets the source collection.
        /// </summary>
        public IEnumerable SourceCollection
        {
            get { return GetSource(); }
        }

#if WPF
        private PropertyDescriptorCollection itemProperties;
        /// <summary>
        /// Gets the item properties.
        /// </summary>
        /// <value>The item properties.</value>
        public PropertyDescriptorCollection ItemProperties
        {
            get
            {
                return this.itemProperties;
            }
        }
#else
        private PropertyInfoCollection itemProperties;
        /// <summary>
        /// Gets the item properties.
        /// </summary>
        /// <value>The item properties.</value>
        public PropertyInfoCollection ItemProperties
        {
            get
            {
                return this.itemProperties;
            }
        }
#endif

#if WPF
        public virtual PropertyDescriptorCollection GetItemProperties()
#else
        /// <summary>
        /// Gets the item properties.
        /// </summary>
        /// <returns>PropertyInfoCollection.</returns>
        public virtual PropertyInfoCollection GetItemProperties()
#endif
        {
            return this.ItemProperties;
        }


        internal IPropertyAccessProvider propertyAccessProvider;

        /// <summary>
        /// Gets the PropertyAccessProvider.
        /// </summary>
        /// <returns>the IPropertyAccessProvider.</returns>
        public virtual IPropertyAccessProvider GetPropertyAccessProvider()
        {
            return this.propertyAccessProvider;
        }

        /// <summary>
        /// Creates the item properties provider. Override this method to have return custom <see cref="IPropertyAccessProvider"/> and have customizations.
        /// </summary>
        /// <returns>IPropertyAccessProvider.</returns>
        protected virtual IPropertyAccessProvider CreateItemPropertiesProvider()
        {
            if (this.IsDynamicBound)
            {
                return new TreeGridDynamicPropertiesProvider(this.GetTreeGrid() as SfTreeGrid, this);
            }
            else
                return new TreeGridPropertiesProvider(this.GetTreeGrid() as SfTreeGrid, this);
        }

        /// <summary>
        /// Suspends the data manipulation operations in SfTreeGrid.
        /// </summary>       
        public void Suspend()
        {
            IsInSuspend = true;
        }

        /// <summary>
        /// Resumes the data manipulation operations in SfTreeGrid.
        /// </summary>
        public void Resume()
        {
            IsInSuspend = false;
        }

        #region IEditableCollectionView
        public bool CanCancelEdit
        {
            get { return editItem is IEditableObject; }
        }

        /// <summary>
        /// Ends the edit transaction and, if possible, restores the original value to the item.
        /// </summary>
        public void CancelEdit()
        {
            if (editItem == null)
                throw new InvalidOperationException("EditItem is null to CancelEdit");

            if (editItem is IEditableObject)
                (editItem as IEditableObject).CancelEdit();

            this.editItem = null;
        }

        /// <summary>
        /// Ends the edit transaction.
        /// </summary>
        public virtual void EndEdit()
        {
            if (editItem == null)
                throw new InvalidOperationException("Already another Item is in EditMode");

            if (editItem is IEditableObject)
                (editItem as IEditableObject).EndEdit();
            editItem = null;
        }

        protected bool IsInCommitEdit = false;

        /// <summary>
        /// Ends the edit transaction and saves the pending changes.
        /// </summary>
        public virtual void CommitEdit()
        {
            if (IsInCommitEdit)
                return;

            if (this.editItem == null)
                throw new InvalidOperationException("IsEditingItem is not true to EndEdit");

            IsInCommitEdit = true;
            if (editItem is IEditableObject)
                (editItem as IEditableObject).EndEdit();
            var passesFilter = true;
            if (LiveNodeUpdateMode == LiveNodeUpdateMode.AllowDataShaping)
            {
                var treeNode = this.Nodes.GetNode(editItem);
                if (treeNode != null)
                {
                    if (this.CanFilterNode(treeNode))
                    {
                        passesFilter = this.FilterItem(editItem);
                        if (!passesFilter)
                        {
                            ApplyFilter(treeNode, !passesFilter);
                        }
                    }

                    var hasSort = this.SortDescriptions != null && this.SortDescriptions.Count > 0;
                    if (hasSort)
                    {
                        var parentNode = treeNode.ParentNode;
                        TreeNodes nodes;
                        if (parentNode != null)
                            nodes = parentNode.ChildNodes;
                        else
                            nodes = Nodes.RootNodes;
                        var removeAtIndex = nodes.IndexOf(treeNode);
                        if (this.SortDescriptions.Count > 0 && removeAtIndex >= 0)
                        {
                            var comparerIndex = GetComparerIndex(nodes, treeNode, removeAtIndex, true, SortComparer);
                            if (comparerIndex != removeAtIndex)
                            {
                                MoveNodesOnPropertyChange(parentNode, removeAtIndex, comparerIndex);
                            }
                        }
                    }
                }
            }
            var selfRelationalView = this as TreeGridSelfRelationalView;
            if (selfRelationalView != null)
            {
                isNodeCollectionChanged = false;
            }
            UpdateNodesOnPropertyChange(editItem, null);
            editItem = null;
            if (!passesFilter)
            {
                RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            IsInCommitEdit = false;
        }
        #endregion

        internal void PopulateTree()
        {
            TreeGridRequestTreeItemsEventArgs args = new TreeGridRequestTreeItemsEventArgs(null);
            args.CanAddChildNode = true;
            RequestTreeItems(args);
            SetDirtyAndResetCache();
        }

        internal TreeGridView GetView()
        {
            return this;
        }

        #region CurrentItem

        private object currentItem;
        /// <summary>
        /// Gets the current item in the view.
        /// </summary>
        /// <returns>
        /// The current item of the view or null if there is no current item.
        /// </returns>
        public object CurrentItem
        {
            get { return currentItem; }
        }

        /// <summary>
        /// Gets the item in the collection that is being edited.
        /// </summary>		
        /// <returns>
        /// The item in the collection that is being edited if <see cref="Syncfusion.Data.IEditableCollectionView.IsEditingItem"/> is true; otherwise, null.
        /// </returns>
        public object CurrentEditItem
        {
            get { return editItem; }
        }

        private object editItem;
        /// <summary>
        /// Gets a value that indicates whether an edit transaction is in progress.
        /// </summary>		
        /// <returns>true if an edit transaction is in progress; otherwise, false.
        /// </returns>
        public bool IsEditingItem
        {
            get { return editItem != null; }
        }

        /// <summary>
        /// Begins an edit transaction of the specified item.
        /// </summary>
        /// <param name="item">The item to edit.</param>
        public void EditItem(object item)
        {
            if (editItem != null)
                throw new InvalidOperationException("Not able to BeginEdit, EditItem is already set");
            editItem = item;
            if (item is IEditableObject)
                (item as IEditableObject).BeginEdit();
        }

#if UWP
        /// <summary>
        /// Occurs when current item is changed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event EventHandler<object> CurrentChanged;
#else
        public event EventHandler CurrentChanged;
#endif
        /// <summary>
        /// Occurs when current item is being changed.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1009:DeclareEventHandlersCorrectly")]
        public event CurrentChangingEventHandler CurrentChanging;
        private int currentPosition;
        /// <summary>
        /// Gets a value that indicates whether the CurrentItem is at the CurrentPosition.
        /// </summary>
        protected bool IsCurrentInSync
        {
            get
            {
                if (this.IsCurrentInView)
                {
                    return this.GetNodeAt(this.CurrentPosition) == this.CurrentItem;
                }

                return this.CurrentItem == null;
            }
        }
        /// <summary>
        /// Gets the original position of the CurrentItem within the (optionally sorted) view.
        /// </summary>
        public int CurrentPosition
        {
            get { return currentPosition; }
        }

        private bool IsCurrentInView
        {
            get
            {
                return (0 <= this.CurrentPosition) && (this.CurrentPosition < this.Nodes.Count);
            }
        }
        /// <summary>
        /// Sets the item at the specified index to be the CurrentItem in the view.
        /// </summary>
        /// <param name="index">The index to set the CurrentItem to.</param>
        /// <returns>true if the resulting CurrentItem is an item within the view; otherwise, false.</returns>
        public virtual bool MoveCurrentToPosition(int index)
        {
            var totalCount = this.Nodes.Count;
            if ((index < -1) || (index > totalCount))
            {
                throw new ArgumentOutOfRangeException("index");
            }

            if ((index != this.CurrentPosition) || !this.IsCurrentInSync)
            {
                var newItem = ((0 <= index) && (index < this.Nodes.Count)) ? this.GetNodeAt(index) : null;

                if (!this.RaiseCurrentChangingEvent())
                    return false;
                this.SetCurrent(newItem, index);
                this.RaiseCurrentChangedEvent();
                this.RaisePropertyChanged("CurrentPosition");
                this.RaisePropertyChanged("CurrentItem");
            }

            return this.IsCurrentInView;
        }

        /// <summary>
        /// Raises the CurrentChanged Event.
        /// </summary>
        protected void RaiseCurrentChangedEvent()
        {
            if (this.CurrentChanged != null)
            {
                this.CurrentChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Raises the CurrentChanging Event.
        /// </summary>
        protected bool RaiseCurrentChangingEvent()
        {
            if (this.CurrentChanging != null)
            {
                var args = new CurrentChangingEventArgs();
                this.CurrentChanging(this, new CurrentChangingEventArgs());
                return !args.Cancel;
            }
            return true;
        }

        /// <summary>
        /// Sets the current item of the View.
        /// </summary>
        /// <param name="newItem">The item to set as the CurrentItem.</param>
        /// <param name="newPosition">The value to set as the CurrentPosition property value.</param>
        protected virtual void SetCurrent(object newItem, int newPosition)
        {
            int count = (newItem != null) ? 0 : this.Nodes.Count;
            this.SetCurrent(newItem, newPosition, count);
        }

        /// <summary>
        /// Sets the current item of the View.
        /// </summary>
        /// <param name="newItem">The item to set as the CurrentItem.</param>
        /// <param name="newPosition">The value to set as the CurrentPosition property value.</param>
        /// <param name="count">The number of items in the View.</param>
        protected void SetCurrent(object newItem, int newPosition, int count)
        {
            this.currentItem = newItem;
            this.currentPosition = currentItem != null ? this.Nodes.IndexOf(newItem as TreeNode) : -1;
        }

        #endregion

        /// <summary>
        /// Gets the node at specified index.
        /// </summary>
        /// <param name="index">Node at this index will be returned</param>
        /// <returns>Returns the node at specified index</returns>
        public virtual TreeNode GetNodeAt(int index)
        {
            return this.Nodes[index];
        }

        internal void PopulateRootNodes(IEnumerable rootItems)
        {
            foreach (object item in rootItems)
            {
                var node = CreateTreeNode(item, 0, false, null, GetView);
                Nodes.RootNodes.Add(node);
            }
        }

        /// <summary>
        /// Checks whether child node will be available for particular tree node.
        /// </summary>
        /// <param name="treeNode">the tree node.</param>
        /// <returns>
        /// Returns <b>true</b> if the child node will be available; otherwise , <b>false</b> .
        /// </returns>
        public bool IsChildNodeAvailable(TreeNode treeNode)
        {
            TreeGridRequestTreeItemsEventArgs e = new TreeGridRequestTreeItemsEventArgs(treeNode, treeNode.Item, false, false);
            RequestTreeItems(e);
            // args.ChildItems = null , if we don't provide items it RequestTreeItems event.
            if (e.ChildItems != null && e.ChildItems.AsQueryable().Count() > 0)
            {
                e.ParentNode.HasChildNodes = true;
                if (UpdateHasVisibleChildNodesBasedOnChildItems(e.ParentNode, e.ChildItems))
                {
                    e.ParentNode.HasVisibleChildNodes = true;
                }
                else
                    e.ParentNode.HasVisibleChildNodes = false;
                return e.ParentNode.HasVisibleChildNodes;
            }
            else if (treeNode.ChildNodes != null && treeNode.ChildNodes.Any())
            {
                e.ParentNode.SetHasChildNodes(true);
            }
            else
            {
                e.ParentNode.SetHasChildNodes(false);
            }
            return e.ParentNode.HasChildNodes;
        }

        internal void FilterNode(TreeNode treeNode)
        {
            if (!FilterItem(treeNode.Item))
                treeNode.IsFiltered = true;
            else
                treeNode.IsFiltered = false;
        }
        internal virtual void RequestTreeItems(TreeGridRequestTreeItemsEventArgs e)
        {

        }


        private RecursiveCheckingMode recursiveCheckingMode;

        /// <summary>
        /// Gets or sets the value which indicates in which case recursive checking should be applied when <see cref="SfTreeGrid.EnableRecursiveChecking"/> is <value>true</value>.
        /// </summary>
        public RecursiveCheckingMode RecursiveCheckingMode
        {
            get { return recursiveCheckingMode; }
            set { recursiveCheckingMode = value; }
        }

        internal void PopulateTreeNodes(TreeGridRequestTreeItemsEventArgs args)
        {
            suspendIsCheckedRecursiveUpdate = true;
            if (args.ParentItem == null)
            {
                if (args.ChildItems != null)
                {
                    PopulateRootNodes(args.ChildItems as IEnumerable<object>);
                }
            }
            else if (args.ParentNode.ChildNodes.Count == 0 || args.ResetChildAndRepopulate)
            {
                RemoveNotifyListeners(args.ParentNode.ChildNodes.sourceList);
                args.ParentNode.ChildNodes.Clear();

                if (args.ChildItems != null)
                {
                    foreach (object item in args.ChildItems)
                    {
                        TreeNode childNode = null;
                        if (tempNode != null && tempNode.Item == item)
                        {
                            childNode = tempNode;
                            childNode.Level = args.ParentNode.Level + 1;
                            childNode.ParentNode = args.ParentNode;
                            UpdateIsCheckedState(args.ParentNode, childNode);
                            tempNode = null;
                        }
                        else
                            childNode = CreateTreeNode(item, args.ParentNode.Level + 1, false, args.ParentNode, GetView);
                        args.ParentNode.ChildNodes.Add(childNode);
                    }
                    if (args.ParentNode.ChildNodes.Any())
                    {
                        args.ParentNode.HasChildNodes = true;
                        UpdateHasVisibleChildNodesBasedOnChildNodes(args.ParentNode);
                    }
                }
            }

            suspendIsCheckedRecursiveUpdate = false;
            if (args.ParentNode != null)
                ValidateCheckBoxPropertyValue(args.ParentNode.ChildNodes);
            else
                ValidateCheckBoxPropertyValue(Nodes.RootNodes);
        }

        #region CheckBoxSelection
        internal virtual void ValidateCheckBoxPropertyValue(TreeNodes nodes)
        {

        }

        private void SetIsCheckedState(TreeNode treeNode, bool? state)
        {
            treeNode.IsChecked = state;
        }

        private void UpdateIsCheckedState(TreeNode parentNode, TreeNode childNode)
        {
            if (!string.IsNullOrEmpty(CheckBoxMappingName))
            {
                var isCheckedValue = (bool?)propertyAccessProvider.GetValue(childNode.Item, CheckBoxMappingName);
                if (parentNode != null && EnableRecursiveChecking && parentNode.isCheckedChanged)
                {
                    SetIsCheckedState(childNode, parentNode.IsChecked);
                    childNode.isCheckedChanged = true;
                }
                else
                {
                    SetIsCheckedState(childNode, isCheckedValue);
                }
            }
            else if (EnableRecursiveChecking && parentNode != null)
            {
                if (parentNode.isCheckedChanged)
                {
                    SetIsCheckedState(childNode, parentNode.IsChecked);
                    childNode.isCheckedChanged = true;
                }
            }
        }

        internal void SetCheckBoxPropertyNameValue(TreeNode node)
        {
            if (!string.IsNullOrEmpty(CheckBoxMappingName))
            {
                suspendIsCheckedUpdate = true;
                propertyAccessProvider.SetValue(node.Item, CheckBoxMappingName, node.IsChecked);
                suspendIsCheckedUpdate = false;
            }
        }
        /// <summary>
        /// temporarily suspends node's IsChecked update when EnableRecursiveChecking is true.
        /// </summary>
        internal bool suspendIsCheckedRecursiveUpdate = false;

        /// <summary>
        /// Suspends node's IsChecked update when CheckBoxMappingName value is changed from corresponding node's IsChecked.
        /// </summary>
        internal bool suspendIsCheckedUpdate = false;

        #endregion
        internal void RefreshSorting()
        {
            if (deferRefreshCount > -1 && !IsInEndeferal)
                return;
            if (this.SortDescriptions.Any())
            {
                Nodes.RootNodes.Sort(this.SortDescriptions, this.SortComparers);
                SortChildNodes(Nodes.RootNodes);
            }
            else
            {
                Nodes.RootNodes.RefreshNodes();
                RefreshChildNodes(Nodes.RootNodes);
            }
            Nodes.ResetCache = true;
            RaiseNodeCollectionChangedEvent(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        internal void SortNodes(TreeNode parentNode)
        {
            if (parentNode == null)
                Nodes.RootNodes.Sort(this.SortDescriptions, this.SortComparers);
            else
                parentNode.ChildNodes.Sort(this.SortDescriptions, this.SortComparers);
        }

        private void RefreshChildNodes(TreeNodes nodes)
        {
            foreach (var node in nodes)
            {
                if (node.ChildNodes.Any())
                {
                    node.ChildNodes.RefreshNodes();
                    RefreshChildNodes(node.ChildNodes);
                }
            }
        }
        internal void SortChildNodes(TreeNodes nodes)
        {
            if (nodes.Any(n => n.IsExpanded))
            {
                foreach (var node in nodes)
                {
                    if (node.IsExpanded)
                        if (node.ChildNodes.Any())
                        {
                            node.ChildNodes.Sort(SortDescriptions, SortComparers);
                            SortChildNodes(node.ChildNodes);
                        }
                }
            }
        }

        /// <summary>
        /// Detach the grid instance present in the view while disposing the view.
        /// </summary>
        public virtual void DetachTreeView()
        {

        }

        /// <summary>
        /// Associates treeGrid in view.
        /// </summary>
        public virtual void AttachTreeView(object treeGrid)
        {

        }

        /// <summary>
        /// Gets treeGrid associated in the view.
        /// </summary>
        /// <returns>treeGrid.</returns>
        public virtual object GetTreeGrid()
        {
            return null;
        }

        /// <summary>
        /// flag which indicates whether view is disposed or not.
        /// </summary>
        protected internal bool IsDisposed = false;

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            UnwireNotifyPropertyChangedForUnderlyingSource();
            UnwireEvents();
            this.itemProperties = null;
            this.sortComparer = null;
            this.indexComparer = null;
            if (this.sortDescriptions != null)
            {
                sortDescriptions.Clear();
                sortDescriptions = null;
            }
            if (this.sortComparers != null)
            {
                sortComparers.Clear();
                sortComparers = null;
            }
            if (nodes != null)
            {
                this.nodes.Dispose();
                this.nodes = null;
            }
            this.source = null;
            this.DetachTreeView();
            if (this.propertyAccessProvider != null)
            {
                this.propertyAccessProvider.Dispose();
                this.propertyAccessProvider = null;
            }
            IsDisposed = true;
        }
        public object AddNew()
        {
            throw new NotImplementedException();
        }
        public bool CanAddNew
        {
            get { return false; }
        }
        public bool CanRemove
        {
            get { return (TreeGridHelper.GetSourceListCollection(this.SourceCollection) != null); }
        }
        public void CancelNew()
        {
            throw new NotImplementedException();
        }
        public void CommitNew()
        {
            throw new NotImplementedException();
        }
        public object CurrentAddItem
        {
            get { return null; }
        }
        public bool IsAddingNew
        {
            get { return false; }
        }

        public NewItemPlaceholderPosition NewItemPlaceholderPosition { get; set; }

        internal void Add(object item)
        {
            var sourcelist = TreeGridHelper.GetSourceListCollection(this.SourceCollection);
            sourcelist.Add(item);
        }

        public void Remove(object item)
        {
            var sourcelist = TreeGridHelper.GetSourceListCollection(this.SourceCollection);
            if (CanRemove && sourcelist.Contains(item))
            {
                sourcelist.Remove(item);
            }
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Interface used to associate tree grid with view.
    /// </summary>
    public interface ITreeGridViewNotifier
    {
        /// <summary>
        /// Detach the grid instance present in the view while disposing the view.
        /// </summary>
        void DetachTreeView();

        /// <summary>
        /// Associates treeGrid in view.
        /// </summary>
        void AttachTreeView(object treeGrid);

        /// <summary>
        /// Gets treeGrid associated in the view.
        /// </summary>
        /// <returns>treeGrid.</returns>
        object GetTreeGrid();
    }
}
