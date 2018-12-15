#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Specialized;
using Syncfusion.Data;
using System.Reflection;
using Syncfusion.Data.Helper;
using System.Diagnostics;
using System.Collections.ObjectModel;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.Dynamic;
#if UWP
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls.Primitives;
using System.Windows;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    /// <summary>
    /// Represents a class that handles row drag-and-drop operations in SfTreeGrid.
    /// </summary>
    /// <remarks>
    /// It provides the set of public properties and virtual methods to handle the row drag-and-drop operations in SfTreeGrid.
    /// </remarks>
    public class TreeGridRowDragDropController : IDisposable
    {
        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>
        protected SfTreeGrid TreeGrid;
        private Popup upArrowIndicator;
        private Popup downArrowIndicator;

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> class.
        /// </summary>
        /// <param name="TreeGrid">The SfTreeGrid.</param>
        public TreeGridRowDragDropController(SfTreeGrid TreeGrid)
        {
            this.TreeGrid = TreeGrid;
        }

        /// <summary>
        /// Occurs when a drag operation is initiated.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragStartingEventArgs">DragStartingEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        internal protected virtual void ProcessOnDragStarting(DragStartingEventArgs args, RowColumnIndex rowColumnIndex)
        {
            var nodes = new ObservableCollection<TreeNode>();

            var node = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
            if (node == null)
                return;
            if (!TreeGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
            {
                args.Cancel = true;
                return;
            }

            if (TreeGrid.SelectedItems.Contains(node.Item))
            {
                foreach (var item in TreeGrid.SelectedItems)
                {
                    node = this.TreeGrid.View.Nodes.GetNode(item);
                    if (node == null)
                        continue;
                    nodes.Add(node);
                }
            }
            else
                nodes.Add(node);
            args.Data.Properties.Add("Nodes", nodes);
            args.Data.Properties.Add("SourceTreeGrid", TreeGrid);
            args.Data.SetText(StandardDataFormats.Text);
            args.DragUI.SetContentFromDataPackage();
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drag event with this element as the potential drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        internal protected virtual void ProcessOnDragOver(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            StopTimer();
            autoExpandNode = null;
            var node = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
            var draggingNodes = GetNodes(args);
            var dropPosition = GetDropPosition(args, rowColumnIndex);
            // when drag and drop nodes from two different treegrid and 
            //source treegrid has allow dragging settings is false we need to skip the droping rows in Targetted TreeGrid. 
            //in this case Dragging nodes has came as Null value.
            if (draggingNodes == null)
                return;

            if (dropPosition == DropPosition.None || dropPosition == DropPosition.Default)
            {
                CloseDragIndicators();
                args.AcceptedOperation = DataPackageOperation.None;
                args.DragUIOverride.Caption = "Can't drop here";
                return;
            }

            else if (dropPosition == DropPosition.DropAbove)
            {
                if (draggingNodes != null && draggingNodes.Count > 1)
                    args.DragUIOverride.Caption = "Drop these " + draggingNodes.Count + "  rows above";
                else
                    args.DragUIOverride.Caption = "Drop above";
            }
            else if (dropPosition == DropPosition.DropAsChild)
            {
                if (draggingNodes != null && draggingNodes.Count > 1)
                    args.DragUIOverride.Caption = "Drop these " + draggingNodes.Count + "  rows below";
                else
                    args.DragUIOverride.Caption = "Drop as child";
            }
            else
            {
                if (draggingNodes != null && draggingNodes.Count > 1)
                    args.DragUIOverride.Caption = "Drop these " + draggingNodes.Count + "  rows below";
                else
                    args.DragUIOverride.Caption = "Drop below";
            }
            args.AcceptedOperation = DataPackageOperation.Move;
            ShowDragIndicators(dropPosition, rowColumnIndex);
            args.Handled = true;
        }

        private ObservableCollection<TreeNode> GetNodes(DragEventArgs dragEventArgs)
        {
            if (dragEventArgs.DataView.Properties.ContainsKey("Nodes"))
                return dragEventArgs.DataView.Properties["Nodes"] as ObservableCollection<TreeNode>;
            else
                return null;
        }

        private SfTreeGrid GetSourceTreeGrid(DragEventArgs dragEventArgs)
        {
            if (dragEventArgs.DataView.Properties.ContainsKey("SourceTreeGrid"))
                return dragEventArgs.DataView.Properties["SourceTreeGrid"] as SfTreeGrid;
            else
                return null;
        }


        /// <summary>
        /// Occurs when the input system reports an underlying drag event with this element as the origin.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        internal protected virtual void ProcessOnDragLeave(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            StopTimer();
            autoExpandNode = null;
            if (rowColumnIndex.RowIndex == this.TreeGrid.TreeGridPanel.ScrollRows.LastBodyVisibleLineIndex
                || rowColumnIndex.RowIndex != this.TreeGrid.GetFirstDataRowIndex())
                this.TreeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.Vertical;

            this.TreeGrid.AutoScroller.MouseMovePosition = args.GetPosition(this.TreeGrid);
            CloseDragIndicators();
        }

        /// <summary>
        /// Occurs when the input system reports an underlying drop event with this element as the drop target.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">Specifies the row column index based on the mouse point.</param>
        internal protected virtual void ProcessOnDrop(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            StopTimer();
            autoExpandNode = null;
            if (!TreeGrid.SelectionController.CurrentCellManager.CheckValidationAndEndEdit())
            {
                return;
            }
            this.TreeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
            var dropPosition = GetDropPosition(args, rowColumnIndex);
            if (dropPosition != DropPosition.None && rowColumnIndex.RowIndex != -1)
            {
                var nodes = GetNodes(args);
                var sourceTreeGrid = GetSourceTreeGrid(args);
                if (this.TreeGrid.View is TreeGridNestedView)
                    ProcessDropOnNestedView(rowColumnIndex, dropPosition, nodes, sourceTreeGrid);
                else if (this.TreeGrid.View is TreeGridUnboundView)
                    ProcessDropOnUnboundView(rowColumnIndex, dropPosition, nodes, sourceTreeGrid);
                else if (this.TreeGrid.View is TreeGridSelfRelationalView)
                    ProcessDropOnSelfRelationalView(rowColumnIndex, dropPosition, nodes, sourceTreeGrid);
            }
            CloseDragIndicators();
        }

        /// <summary>
        /// Occurs when a drag-and-drop operation is ended.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DropCompletedEventArgs">DropCompletedEventArgs</see> that contains the event data.</param>
        internal protected virtual void ProcessOnDropCompleted(DropCompletedEventArgs args)
        {
            StopTimer();
            autoExpandNode = null;
            this.TreeGrid.AutoScroller.AutoScrolling = AutoScrollOrientation.None;
            CloseDragIndicators();
        }

        /// <summary>
        /// Process drop operations in nested view.
        /// </summary>
        /// <param name="rowColumnIndex">dropped row column index.</param>
        /// <param name="dropPosition">drop position.</param>
        /// <param name="nodes">the dragging nodes which needs to be dropped.</param>
        /// <param name="sourceTreeGrid">the grid from which drag operation in initiated.</param>
        protected virtual void ProcessDropOnNestedView(RowColumnIndex rowColumnIndex, DropPosition dropPosition, ObservableCollection<TreeNode> nodes, SfTreeGrid sourceTreeGrid)
        {
            var isSameGrid = (TreeGrid == sourceTreeGrid);
            var treeNode = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);

            if (treeNode == null)
                return;
            var data = treeNode.Item;
            TreeGrid.View.Suspend();
            TreeGrid.SelectionController.SuspendUpdates();
            ProcessDragSourceOnDrop(sourceTreeGrid, nodes);
            var itemIndex = -1;
            var nodeIndex = -1;
            TreeNode parentNode = null;
            if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
                parentNode = treeNode.ParentNode;
            else if (dropPosition == DropPosition.DropAsChild)
            {
                if (!treeNode.IsExpanded)
                    TreeGrid.ExpandNode(treeNode);
                parentNode = treeNode;
            }
            IList sourceCollection = null;
            if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
            {
                if (treeNode.ParentNode != null)
                {
                    var collection = TreeGrid.View.propertyAccessProvider.GetValue(treeNode.ParentNode.Item, TreeGrid.ChildPropertyName) as IEnumerable;
                    sourceCollection = TreeGridHelper.GetSourceListCollection(collection);
                }
                else
                {
                    sourceCollection = TreeGridHelper.GetSourceListCollection(TreeGrid.View.SourceCollection);
                }
                itemIndex = sourceCollection.IndexOf(data);
                if (parentNode != null)
                    nodeIndex = parentNode.ChildNodes.IndexOf(treeNode);
                else
                    nodeIndex = this.TreeGrid.View.Nodes.RootNodes.IndexOf(treeNode);
                if (dropPosition == DropPosition.DropBelow)
                {
                    itemIndex += 1;
                    nodeIndex += 1;
                }
            }
            else if (dropPosition == DropPosition.DropAsChild)
            {
                var item = treeNode.Item;
                var collection = TreeGrid.View.propertyAccessProvider.GetValue(item, TreeGrid.ChildPropertyName) as IEnumerable;
                sourceCollection = TreeGridHelper.GetSourceListCollection(collection);
                if (sourceCollection == null)
                {
                    // Create sourcecollection by getting item type.
                    if (!DynamicHelper.CheckIsDynamicObject(item.GetType()))
                    {
                        var list = item.GetType().GetProperty(TreeGrid.ChildPropertyName).PropertyType.CreateNew() as IList;
                        if (list != null)
                        {
                            TreeGrid.View.propertyAccessProvider.SetValue(treeNode.Item, TreeGrid.ChildPropertyName, list);
                            sourceCollection = list;
                        }
                    }
                    else
                    {
                        // If item is dynamic collection, customer needs to override and add record.
                        this.TreeGrid.View.Resume();
                        TreeGrid.SelectionController.ResumeUpdates();
                        (TreeGrid.SelectionController as TreeGridRowSelectionController).RefreshSelection();
                        return;
                    }
                }
                itemIndex = sourceCollection.Count;
                nodeIndex = parentNode.ChildNodes.Count;
            }

            foreach (var node in nodes)
            {
                sourceCollection.Insert(itemIndex, node.Item);
                this.TreeGrid.View.AddNode(parentNode, node, nodeIndex, itemIndex, false);
            }

            UpdateParentNodeExpanderCell(parentNode);
            this.TreeGrid.View.Resume();
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).RefreshSelection();
        }

        internal void UpdateParentNodeExpanderCell(TreeNode parentNode)
        {
            if (parentNode != null && !parentNode.HasChildNodes)
            {
                parentNode.HasChildNodes = true;
                TreeGrid.TreeGridModel.UpdateExpander(parentNode);
            }
        }

        /// <summary>
        /// Process drop operations in self relational view.
        /// </summary>
        /// <param name="rowColumnIndex">dropped row column index.</param>
        /// <param name="dropPosition">drop position.</param>
        /// <param name="nodes">the dragging nodes which needs to be dropped.</param>
        /// <param name="sourceTreeGrid">the grid from which drag operation in initiated.</param>
        protected virtual void ProcessDropOnSelfRelationalView(RowColumnIndex rowColumnIndex, DropPosition dropPosition, ObservableCollection<TreeNode> nodes, SfTreeGrid sourceTreeGrid)
        {
            var isSameGrid = (TreeGrid == sourceTreeGrid);
            var treeNode = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
            if (treeNode == null)
                return;

            var data = treeNode.Item;
            object parentPropertyValue = null;

            TreeGrid.View.Suspend();
            TreeGrid.SelectionController.SuspendUpdates();

            ProcessDragSourceOnDrop(sourceTreeGrid, nodes);


            TreeNode parentNode = null;

            if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
            {
                parentNode = treeNode.ParentNode;
            }
            else if (dropPosition == DropPosition.DropAsChild)
            {
                if (!treeNode.IsExpanded)
                    TreeGrid.ExpandNode(treeNode);
                parentNode = treeNode;
            }
            var itemIndex = -1;
            var nodeIndex = -1;
            IList sourceCollection = TreeGridHelper.GetSourceListCollection(TreeGrid.View.SourceCollection);
            if (dropPosition == DropPosition.DropAsChild)
            {
                itemIndex = sourceCollection.Count;
                nodeIndex = parentNode.ChildNodes.Count;
            }
            else if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
            {
                itemIndex = sourceCollection.IndexOf(data);
                if (parentNode != null)
                    nodeIndex = parentNode.ChildNodes.IndexOf(treeNode);
                else
                    nodeIndex = this.TreeGrid.View.Nodes.RootNodes.IndexOf(treeNode);
                if (dropPosition == DropPosition.DropBelow)
                {
                    itemIndex += 1;
                    nodeIndex += 1;
                }
            }

            var propertyAccessProvider = TreeGrid.View.GetPropertyAccessProvider();
            if (parentNode != null)
                parentPropertyValue = propertyAccessProvider.GetValue(parentNode.Item, TreeGrid.ParentPropertyName);
            else
            {
                if (TreeGrid.ReadLocalValue(SfTreeGrid.SelfRelationRootValueProperty) != DependencyProperty.UnsetValue)
                    parentPropertyValue = TreeGrid.SelfRelationRootValue;
            }

            foreach (var node in nodes)
            {
                if (parentNode == null && TreeGrid.ReadLocalValue(SfTreeGrid.SelfRelationRootValueProperty) == DependencyProperty.UnsetValue)
                {
                    if (isSameGrid)
                    {
                        parentPropertyValue = GetChildPropertyValue(node);
                    }
                    else
                        parentPropertyValue = GetChildPropertyValue(treeNode);
                }
                SetValue(node.Item, TreeGrid.ChildPropertyName, propertyAccessProvider, parentPropertyValue);
                sourceCollection.Insert(itemIndex, node.Item);

                if (isSameGrid)
                    this.TreeGrid.View.AddNode(parentNode, node, nodeIndex, needDataShaping: false);
                else
                {
                    (this.TreeGrid.View as TreeGridSelfRelationalView).CheckPrimaryKey();
                    this.TreeGrid.View.AddNode(parentNode, node.Item, nodeIndex, false);
                }
            }

            UpdateParentNodeExpanderCell(parentNode);

            this.TreeGrid.View.Resume();
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).RefreshSelection();
        }

        /// <summary>
        /// Remove dragged nodes from tree grid while dropping.
        /// </summary>
        /// <param name="sourceTreeGrid">the grid from which drag operation in initiated.</param>
        /// <param name="nodes">dragged nodes.</param>
        protected virtual void ProcessDragSourceOnDrop(SfTreeGrid sourceTreeGrid, ObservableCollection<TreeNode> nodes)
        {
            var isSameGrid = (TreeGrid == sourceTreeGrid);
            if (sourceTreeGrid.View is TreeGridNestedView)
            {
                foreach (var node in nodes)
                {
                    if (node.ParentNode == null)
                    {
                        sourceTreeGrid.View.Remove(node.Item);
                    }
                    else
                    {
                        var collection = sourceTreeGrid.View.propertyAccessProvider.GetValue(node.ParentNode.Item, sourceTreeGrid.ChildPropertyName) as IEnumerable;
                        var itemCollection = TreeGridHelper.GetSourceListCollection(collection);
                        if (itemCollection != null)
                            itemCollection.Remove(node.Item);
                    }
                    if (isSameGrid)
                        sourceTreeGrid.View.RemoveNode(node);
                }
            }
            else if (sourceTreeGrid.View is TreeGridSelfRelationalView)
            {
                foreach (var node in nodes)
                {
                    sourceTreeGrid.View.Remove(node.Item);
                    if (isSameGrid)
                        sourceTreeGrid.View.RemoveNode(node);
                }
            }
            else if (sourceTreeGrid.View is TreeGridUnboundView)
            {
                foreach (var node in nodes)
                {
                    sourceTreeGrid.View.RemoveNode(node);
                }
            }
        }

        private object GetChildPropertyValue(TreeNode node)
        {
            if (node.ParentNode == null)
                return TreeGrid.View.GetPropertyAccessProvider().GetValue(node.Item, TreeGrid.ChildPropertyName);
            return GetChildPropertyValue(node.ParentNode);
        }

        internal void SetValue(object rowData, string propertyName, IPropertyAccessProvider provider, object changedValue)
        {
            Type type = GetPropertyType(rowData, propertyName);
            if (type == null)
                return;
            var canconvert = CanConvertToType(changedValue, ref type);

            if (!canconvert && string.IsNullOrEmpty(changedValue.ToString()))
            {
                return;
            }

            if (!(canconvert || type == typeof(string) || type == typeof(object)))
                return;

            var pasteValue = Convert.ChangeType(changedValue, type);
            provider.SetValue(rowData, propertyName, pasteValue);
        }

        internal bool CanConvertToType(object value, ref Type type)
        {
            if (NullableHelperInternal.IsNullableType(type))
                type = NullableHelperInternal.GetUnderlyingType(type);

#if UWP
            var method = type.GetTypeInfo().DeclaredMethods.Where(x => x.Name.Equals("TryParse"));
#else
            var method = type.GetMethods().Where(x => x.Name.Equals("TryParse"));
#endif

            if (method.Count() == 0)
                return false;

            var methodinfo = method.FirstOrDefault();
            object[] args = { value.ToString(), null };
            return (bool)methodinfo.Invoke(null, args);
        }

        internal Type GetPropertyType(object rowData, string childPropertyName)
        {
#if WPF
                PropertyDescriptorCollection typeInfos = TreeGrid.View.GetItemProperties();
#else
            PropertyInfoCollection typeInfos = TreeGrid.View.GetItemProperties();
#endif
            var typeInfo = typeInfos.GetPropertyDescriptor(childPropertyName);
            if (typeInfo != null)
                return typeInfo.PropertyType;
            return null;
        }

        private bool canAutoExpand = true;
        /// <summary>
        /// Gets or sets a value which indicates whether need to auto expand the node if it is not expanded when DropPosition is DropAsChild.
        /// </summary>
        public bool CanAutoExpand
        {
            get { return canAutoExpand; }
            set { canAutoExpand = value; }
        }

        private TimeSpan autoExpandDelay = new TimeSpan(0, 0, 0, 3);
        /// <summary>
        /// Gets or sets the time delay to expand the node automatically.
        /// </summary>
        /// <value>
        /// The default value is 3 sec. 
        /// </value>
        public TimeSpan AutoExpandDelay
        {
            get { return autoExpandDelay; }
            set { autoExpandDelay = value; }
        }

        /// <summary>
        /// Process drop operations in unbound view.
        /// </summary>
        /// <param name="rowColumnIndex">dropped row column index.</param>
        /// <param name="dropPosition">drop position.</param>
        /// <param name="nodes">the dragging nodes which needs to be dropped.</param>
        /// <param name="sourceTreeGrid">the grid from which drag operation in initiated.</param>
        protected virtual void ProcessDropOnUnboundView(RowColumnIndex rowColumnIndex, DropPosition dropPosition, ObservableCollection<TreeNode> nodes, SfTreeGrid sourceTreeGrid)
        {
            var treeNode = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
            object data = null;
            if (treeNode != null)
                data = treeNode.Item;

            this.TreeGrid.View.Suspend();
            TreeGrid.SelectionController.SuspendUpdates();
            ProcessDragSourceOnDrop(sourceTreeGrid, nodes);
            var itemIndex = -1;
            if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
            {
                if (treeNode.ParentNode != null)
                    itemIndex = treeNode.ParentNode.ChildNodes.IndexOf(treeNode);
                else
                    itemIndex = this.TreeGrid.View.Nodes.RootNodes.IndexOf(treeNode);
            }
            else if (dropPosition == DropPosition.DropAsChild)
            {
                itemIndex = treeNode.ChildNodes.Count;
            }

            if (dropPosition == DropPosition.DropBelow)
                itemIndex += 1;
            TreeNode parentNode = null;
            if (dropPosition == DropPosition.DropBelow || dropPosition == DropPosition.DropAbove)
                parentNode = treeNode.ParentNode;
            else if (dropPosition == DropPosition.DropAsChild)
            {
                TreeGrid.ExpandNode(treeNode);
                parentNode = treeNode;
            }

            foreach (var node in nodes)
            {
                this.TreeGrid.View.AddNode(parentNode, node, itemIndex, needDataShaping: false);
            }

            UpdateParentNodeExpanderCell(parentNode);

            this.TreeGrid.View.Resume();
            TreeGrid.SelectionController.ResumeUpdates();
            (TreeGrid.SelectionController as TreeGridRowSelectionController).RefreshSelection();
        }

        /// <summary>
        /// Sets drag indicators in appropriate position.
        /// </summary>
        /// <param name="dropPosition">
        /// Indicates the drop position, based on that the indicators will shown.
        /// </param>
        /// <param name="rowColumnIndex">
        /// Specifies the rowColumn index based on mouse hover. 
        /// </param>
        protected virtual void ShowDragIndicators(DropPosition dropPosition, RowColumnIndex rowColumnIndex)
        {
            if (upArrowIndicator == null && downArrowIndicator == null)
            {
                upArrowIndicator = new Popup();
                downArrowIndicator = new Popup();
                upArrowIndicator.Child = new UpIndicatorContentControl();
                downArrowIndicator.Child = new DownIndicatorContentControl();
            }
            var lineinfo = TreeGrid.TreeGridPanel.ScrollRows.GetVisibleLineAtLineIndex(rowColumnIndex.RowIndex);

            var upHoffset = 0.0;
            var downHoffset = 0.0;
            var upVoffset = 0.0;
            var downVoffset = 0.0;

            double width = 0;
            if (TreeGrid.showRowHeader)
                width = this.TreeGrid.RowHeaderWidth / 2;
            else
                width = 22;
            upHoffset = downHoffset = this.TreeGrid.GetControlRect(this.TreeGrid).X + width;

            if (dropPosition == DropPosition.DropAbove)
            {
                CloseDragIndicators();
                upVoffset = lineinfo.Origin + 5;
                downVoffset = lineinfo.Origin - (22);
                //VisualStateManager.GoToState(GetRowControl(rowColumnIndex), "DropAbove", true);
            }
            else if (dropPosition == DropPosition.DropBelow)
            {
                CloseDragIndicators();
                upVoffset = lineinfo.Corner + 5;
                downVoffset = lineinfo.Corner - 22;
                //VisualStateManager.GoToState(GetRowControl(rowColumnIndex), "DropBelow", true);
            }
            else
            {
                CloseDragIndicators();
                //VisualStateManager.GoToState(GetRowControl(rowColumnIndex), "DropOver", true);
                return;
            }

            // Need to convert the values to their related screen coordinates
            upArrowIndicator.IsOpen = true;
            downArrowIndicator.IsOpen = true;
            var pt1 = this.TreeGrid.TransformToVisual(null).TransformPoint(new Point(upHoffset, upVoffset));
            var pt2 = this.TreeGrid.TransformToVisual(null).TransformPoint(new Point(downHoffset, downVoffset));

            upArrowIndicator.HorizontalOffset = pt1.X;
            downArrowIndicator.HorizontalOffset = pt2.X;
            upArrowIndicator.VerticalOffset = pt1.Y;
            downArrowIndicator.VerticalOffset = pt2.Y;
        }

        /// <summary>
        /// Gets the row control on which the mouse point is hovered.
        /// </summary>
        /// <param name="currentRowColumnIndex">
        /// Row control at this row column index will be taken.
        /// </param>
        /// <returns>
        /// Returns the row control for the specified row column index.
        /// </returns>
        private TreeGridRowControl GetRowControl(RowColumnIndex currentRowColumnIndex)
        {
            var rowinfo = this.TreeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == currentRowColumnIndex.RowIndex);
            if (rowinfo != null)
                return rowinfo.RowElement as TreeGridRowControl;
            return null;
        }

        protected DispatcherTimer timer = new DispatcherTimer();
        protected TreeNode autoExpandNode = null;

        /// <summary>
        /// Gets the dropping position of a dragging row.
        /// </summary>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DragEventArgs">DragEventArgs</see> that contains the event data.</param>
        /// <param name="rowColumnIndex">
        /// Specified index on which the mouse is hovered.
        /// </param>
        /// <returns>
        /// Returns appropriate drop position <see cref="Syncfusion.UI.Xaml.TreeGrid.DropPosition"/> if row can drop at the specified index otherwise none.
        /// </returns>
        protected virtual DropPosition GetDropPosition(DragEventArgs args, RowColumnIndex rowColumnIndex)
        {
            bool canDrop = true;
            var p = args.GetPosition(this.TreeGrid);
            var treeNode = this.TreeGrid.GetNodeAtRowIndex(rowColumnIndex.RowIndex);
            ScrollAxisRegion columnRegion = ScrollAxisRegion.Body;
            if (TreeGrid.TreeGridPanel.FrozenColumns > 0)
                columnRegion = ScrollAxisRegion.Header;
            var rowRect = this.TreeGrid.TreeGridPanel.RangeToRect(ScrollAxisRegion.Body, columnRegion, rowColumnIndex, true, false);
            var node = treeNode;
            var nodes = GetNodes(args);
            if (nodes != null)
            {
                foreach (var draggingNode in nodes)
                {
                    node = treeNode;
                    while (node != null)
                    {
                        if (node == draggingNode)
                        {
                            canDrop = false;
                            break;
                        }
                        node = node.ParentNode;
                    }
                    if (!canDrop)
                        break;
                }
            }

            if (!canDrop)
                return DropPosition.None;
            else if (p.Y > rowRect.Y + 15 && p.Y < rowRect.Y + 35)
            {
                if (!treeNode.IsExpanded)
                {
                    if (CanAutoExpand)
                    {
                        timer.Interval = autoExpandDelay;
                        timer.Tick -= Timer_Tick;
                        autoExpandNode = treeNode;
                        StartTimer();
                    }
                }
                return DropPosition.DropAsChild;
            }
            else if (p.Y < rowRect.Y + 15)
            {
                return DropPosition.DropAbove;
            }
            else if (p.Y > rowRect.Y + 35)
            {
                return DropPosition.DropBelow;
            }
            else
                return DropPosition.Default;
        }
        private void StartTimer()
        {
            timer.Tick += Timer_Tick;
            timer.Start();
        }
        private void StopTimer()
        {
            timer.Tick -= Timer_Tick;
            timer.Stop();
        }
        private void Timer_Tick(object sender, object e)
        {
            if (autoExpandNode != null)
                TreeGrid.ExpandNode(autoExpandNode);
            autoExpandNode = null;
            StopTimer();
        }

        /// <summary>
        /// Closes the Drag arrow indication and clears all the row's VSM applied.
        /// </summary>
        protected void CloseDragIndicators()
        {
            if (this.upArrowIndicator != null && this.downArrowIndicator != null)
            {
                this.upArrowIndicator.IsOpen = false;
                this.downArrowIndicator.IsOpen = false;
            }

            //this.TreeGrid.RowGenerator.Items.ForEach(row =>
            //{
            //    var wholeRowElement = row.RowElement as TreeGridRowControl;
            //    if (row.RowType != TreeRowType.HeaderRow)
            //    {
            //        VisualStateManager.GoToState(wholeRowElement, "NoDrop", true);
            //    }
            //});

        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> class.
        /// </summary>
        /// <param name="disposing"><see langword="true" /> to release all the resources. </param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.TreeGrid = null;
                if (timer != null)
                {
                    timer.Tick -= Timer_Tick;
                    timer = null;
                }
                autoExpandNode = null;
                if (this.upArrowIndicator != null)
                {
                    this.upArrowIndicator.IsOpen = false;
                    this.upArrowIndicator = null;
                }

                if (this.downArrowIndicator != null)
                {
                    this.downArrowIndicator.IsOpen = false;
                    this.downArrowIndicator = null;
                }
            }
        }
    }
}
