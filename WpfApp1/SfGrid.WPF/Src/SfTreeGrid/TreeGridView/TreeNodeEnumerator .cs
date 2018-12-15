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

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeNodesEnumerator : IEnumerator<TreeNode>
    {
        public TreeNodesEnumerator(TreeNodes nodes)
        {
            this.RootNodes = nodes;
            this.Helper = new TreeNodesTraversalHelper(RootNodes);

            if (nodes.Count == 0)
            {
                this.next = null;
            }
            else
            {
                next = nodes.FirstOrDefault(n => !n.IsFiltered);
            }
        }

        public TreeNodes RootNodes
        {
            get;
            private set;
        }

        private TreeNodesTraversalHelper Helper;

        private TreeNode current;
        private TreeNode next;
        public TreeNode Current
        {
            get
            {
                return ((IEnumerator)this).Current as TreeNode;
            }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodesEnumerator"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodesEnumerator"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                Helper.Dispose();
                Helper = null;
                this.current = null;
                this.RootNodes = null;
            }
        }
        object IEnumerator.Current
        {
            get
            {
                return this.current;
            }
        }

        public bool MoveNext()
        {
            if (this.next == null)
            {
                return false;
            }
            this.current = this.next;
            this.next = Helper.GetNext(this.next);
            return true;
        }

        public void Reset()
        {
            this.next = this.RootNodes[0];
            this.current = null;
        }
    }

    internal class TreeNodesTraversalHelper : IDisposable
    {
        public TreeNodesTraversalHelper(TreeNodes Nodes)
        {
            this.RootNodes = Nodes;
        }

        TreeNodes RootNodes;
        public TreeNode GetPrevious(TreeNode node)
        {
            TreeNode prevNode = null;
            var view = node.GetView();
            var parentNode = node.ParentNode;
            if (parentNode != null)
            {
                IList<TreeNode> childNodes = parentNode.ChildNodes;
                if (view != null && view.CanFilter)
                    childNodes = parentNode.ChildNodes.Where(n => !n.IsFiltered).ToList();
                var index = childNodes.IndexOf(node);
                if (index == 0)
                {
                    prevNode = parentNode;
                }
                else
                {
                    prevNode = childNodes[index - 1];
                    if (prevNode.IsExpanded)
                    {
                        prevNode = GetLastNode(prevNode);
                    }
                }
            }
            else
            {
                IList<TreeNode> rootNodes = RootNodes;
                if (view != null && view.CanFilter)
                    rootNodes = RootNodes.Where(n => !n.IsFiltered).ToList();
                int nodeIndex = rootNodes.IndexOf(node);
                prevNode = rootNodes[nodeIndex - 1];
                if (prevNode.IsExpanded)
                {
                    prevNode = GetLastNode(prevNode);
                }
            }
            return prevNode;
        }

        public TreeNode GetLastNode(TreeNode node)
        {
            if (!node.IsExpanded)
                return node;
            var lastChildNode = node.ChildNodes.LastOrDefault(n => !n.IsFiltered);
            // For leaf node, even though IsExpanded is true, it does not have child nodes. So lastChildNode will be null.
            if (lastChildNode == null)
                return node;
            return GetLastNode(lastChildNode);
        }

        public TreeNode GetNode(TreeNodes treeNodes, int index)
        {
            TreeNode node = null;
            var nodes = treeNodes.Where(n => !n.IsFiltered);
            var actualIndex = index;
            foreach (TreeNode rootNode in nodes)
            {
                if (index <= 0)
                {
                    node = rootNode;
                    break;
                }
                else
                {
                    if (rootNode != null && rootNode.IsExpanded)
                    {
                        var nodeCount = rootNode.GetYAmountCache();
                        if (index <= nodeCount - 1)
                        {
                            index--;
                            node = GetNode(rootNode.ChildNodes, index);
                            break;
                        }
                        else
                        {
                            index -= nodeCount;
                        }
                    }
                    else
                    {
                        index--;
                    }
                }
            }
            return node;
        }

        public TreeNode GetNext(TreeNode node)
        {
            TreeNode nextNode = null;
            if (node.IsExpanded)
            {
                nextNode = node.ChildNodes.FirstOrDefault(n => !n.IsFiltered);
            }
            if (nextNode != null)
                return nextNode;

            var parentNode = node.ParentNode;
            if (parentNode != null)
            {
                nextNode = FindNode(parentNode.ChildNodes, node);
                if (nextNode == null)
                    nextNode = GetNextNode(parentNode);
            }
            else
            {
                nextNode = FindNode(RootNodes, node);
            }
            return nextNode;
        }

        /// <summary>
        /// Finds the next visible node.
        /// </summary>
        /// <param name="nodes">nodes collection.</param>
        /// <param name="node">specific node.</param>
        /// <returns>visible node.</returns>
        private TreeNode FindNode(TreeNodes nodes, TreeNode node)
        {
            int nodeIndex = nodes.IndexOf(node);
            for (int i = nodeIndex + 1; i < nodes.Count; i++)
            {
                if (!nodes[i].IsFiltered)
                    return nodes[i];
            }
            return null;
        }

        private TreeNode GetNextNode(TreeNode node)
        {
            TreeNode treeNode = null;
            TreeNode parentNode = node.ParentNode;
            while (node != null && parentNode != null)
            {
                treeNode = FindNode(parentNode.ChildNodes, node);
                if (treeNode != null)
                    return treeNode;
                else
                {
                    parentNode = parentNode.ParentNode;
                    node = node.ParentNode;
                }
            }
            if (parentNode == null)
            {
                treeNode = FindNode(RootNodes, node);
            }
            return treeNode;
        }

        public void Dispose()
        {
            this.RootNodes = null;
        }
    }
}
