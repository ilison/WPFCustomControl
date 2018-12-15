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

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridRequestTreeItemsEventArgs : EventArgs
    {
        public TreeGridRequestTreeItemsEventArgs(object parentItem)
        {
            this.parentItem = parentItem;
        }

        public TreeGridRequestTreeItemsEventArgs(TreeNode parentNode, object parentItem, bool resetChildAndRepopulate, bool addChildNode)
        {
            this.resetChildAndRepopulate = resetChildAndRepopulate;
            this.parentItem = parentItem;
            this.parentNode = parentNode;
            this.canAddChildNode = addChildNode;
        }

        TreeNode parentNode;

        public TreeNode ParentNode
        {
            get { return parentNode; }
            set { parentNode = value; }
        }

        bool resetChildAndRepopulate = false;

        internal bool ResetChildAndRepopulate
        {
            get { return resetChildAndRepopulate; }
            set { resetChildAndRepopulate = value; }
        }

        bool canAddChildNode = false;

        internal bool CanAddChildNode
        {
            get { return canAddChildNode; }
            set { canAddChildNode = value; }
        }
  
        private object parentItem;

        public object ParentItem
        {
            get { return parentItem; }
            set { parentItem = value; }
        }

        private IEnumerable childItems = null;

        public IEnumerable ChildItems
        {
            get { return childItems; }
            set { childItems = value; }
        }
    }

    public delegate void TreeGridRequestTreeItemsEventHandler(object sender, TreeGridRequestTreeItemsEventArgs e);

    public class NodeExpandingEventArgs : CancelEventArgs
    {
        public TreeNode Node { get; internal set; }
    }


    public delegate void NodeExpandingEventHandler(object sender, NodeExpandingEventArgs e);

    public delegate void NodeExpandedEventHandler(object sender, NodeExpandedEventArgs e);

    public class NodeCollapsingEventArgs : CancelEventArgs
    {
        public TreeNode Node { get; internal set; }
    }
    
    public delegate void NodeCollapsingEventHandler(object sender, NodeCollapsingEventArgs e);

    public class NodeCollapsedEventArgs : EventArgs
    {
        public TreeNode Node { get; internal set; }
    }

    public class NodeExpandedEventArgs : EventArgs
    {
        public TreeNode Node { get; internal set; }
    }

    public delegate void NodeCollapsedEventHandler(object sender, NodeCollapsedEventArgs e);
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NodeCheckStateChanged"/> event.  
    /// </summary>
    /// <param name="sender">The sender that contains the CheckBox.</param>
    /// <param name="args">The <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeCheckStateChangedEventArgs"/> that contains the event data.</param>
    public delegate void NodeCheckStateChangedEventHandler(object sender, NodeCheckStateChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NodeCheckStateChanged"/> event.
    /// </summary>
    public class NodeCheckStateChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the TreeNode associated with the clicked check box.
        /// </summary>      
        public TreeNode Node { get; internal set; }
    }
}
