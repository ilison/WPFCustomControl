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
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeNodeCollection : IList<TreeNode>, IDisposable
    {
        public TreeNodes RootNodes { get; set; }

        private TreeNodesTraversalHelper helper;

        public TreeNodeCollection()
        {
            RootNodes = new TreeNodes();
            helper = new TreeNodesTraversalHelper(this.RootNodes);
        }
        protected internal bool isDirty = true;

        private int yAmountCache = -1;
        private int maxLevel = 0;

        /// <summary>
        /// Sets the dirty. When this is set to true, the YAmountCache will be re-computed for the whole node.
        /// </summary>
        public void SetDirty()
        {
            if (!this.isDirty)
            {
                this.isDirty = true;
            }
        }

        public int Count
        {
            get
            {
                if (!isDirty)
                    return yAmountCache;
                GetMaxLevel();
                return GetCount();
            }
        }

        /// <summary>
        /// Get the maximum level of the tree.
        /// </summary>
        public int MaxLevel
        {
            get
            {
                if (!isDirty)
                    return maxLevel;
                GetCount();
                return GetMaxLevel();
            }
        }

        public int GetCount()
        {
            int count = 0;
            foreach (var node in RootNodes.nodeList)
            {
                if (!node.IsFiltered)
                    count += node.GetYAmountCache();
            }
            yAmountCache = count;
            isDirty = false;
            return count;
        }

        public int GetMaxLevel()
        {
            int count = 0;
            if (RootNodes.nodeList.Any(n => n.IsExpanded))
            {
                foreach (var node in RootNodes.nodeList)
                {
                    var level = node.GetMaxLevel();
                    if (level > count)
                        count = level;
                }
            }
            maxLevel = count;
            isDirty = false;
            return count;
        }

        /// <summary>
        /// Determines whether the  <see cref="Syncfusion.UI.Xaml.Grid.TreeGrid.TreeNode"/> is in view or not.
        /// </summary>
        /// <param name="node">TreeNode.</param>
        /// <returns>
        /// <b> true</b> if the node is in view; otherwise , <b>false</b>.
        /// </returns>  
        public bool IsNodeInView(TreeNode node)
        {
            if (node == null || node.IsFiltered)
                return false;
            if (node.ParentNode == null)
            {
                return RootNodes.Contains(node);
            }

            if (!node.ParentNode.IsExpanded)
                return false;

            return IsNodeInView(node.ParentNode);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodeCollection"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodeCollection"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            this.currentnode = null;
            if (this.helper != null)
            {
                this.helper.Dispose();
                this.helper = null;
            }

            if (this.RootNodes != null)
            {
                foreach (var node in this.RootNodes)
                    node.Dispose();
                this.RootNodes.Clear();
                this.RootNodes = null;
            }
        }
        public int IndexOf(TreeNode treeNode)
        {
            if (!IsNodeInView(treeNode))
                return -1;
            int index = 0;
            if (treeNode.ParentNode == null)
            {
                var rootNodes = RootNodes.Where(r => !r.IsFiltered).ToList();
                var rootNodeIndex = rootNodes.IndexOf(treeNode);
                if (rootNodeIndex == -1)
                    return -1;
                for (int i = 0; i < rootNodeIndex; i++)
                {
                    index += rootNodes[i].GetYAmountCache();
                }
                return index;
            }
            else
            {
                var childNodes = treeNode.ParentNode.ChildNodes.Where(r => !r.IsFiltered).ToList();
                var childIndex = childNodes.IndexOf(treeNode);
                index += childIndex + 1;
                if (childIndex != 0)
                {
                    for (int i = 0; i < childIndex; i++)
                    {
                        index += childNodes[i].GetYAmountCache() - 1;
                    }
                }
            }
            return index + IndexOf(treeNode.ParentNode);
        }

        public int GetIndexFromData(object data)
        {
            var index = -1;
            var node = GetNode(data);
            if (node != null)
            {
                index = IndexOf(node);
            }
            return index;
        }

        public TreeNode GetNode(object data)
        {
            TreeNode node = null;
            node = GetVisibleNode(this.RootNodes, data);
            return node;
        }

        /// <summary>
        /// Get the visible node which matches the given data.
        /// </summary>
        /// <param name="nodes">nodes collection</param>
        /// <param name="data">data.</param>
        /// <returns>visible node.</returns>
        internal TreeNode GetVisibleNode(TreeNodes nodes, object data)
        {
            TreeNode treeNode = null;
            foreach (var node in nodes)
            {
                if (node.Item == data)
                {
                    if (!node.IsFiltered)
                        treeNode = node;
                    break;
                }
                else
                {
                    if (!node.IsFiltered && node.IsExpanded)
                        treeNode = GetVisibleNode(node.ChildNodes, data);
                    if (treeNode != null)
                        break;
                }
            }
            return treeNode;
        }

        public void Insert(int index, TreeNode item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        public TreeNode this[int index]
        {
            get
            {
                return GetNodeAt(index);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        int currentIndex = -1;
        private bool resetCache = false;
        public bool ResetCache
        {
            get { return this.resetCache; }
            set { this.resetCache = value; }
        }

        private TreeNode currentnode = null;
        private TreeNode GetNodeAt(int index)
        {
            TreeNode node = null;
            if (index < 0 || index >= this.Count)
                return node;
            if (this.ResetCache)
            {
                this.currentIndex = -2;
                this.currentnode = null;
                this.ResetCache = false;
            }
            if (this.currentnode != null)
            {
                if (index == this.currentIndex)
                {
                    node = this.currentnode;
                }
                else if (index == this.currentIndex - 1)
                {
                    node = this.helper.GetPrevious(this.currentnode);
                }
                else if (index == this.currentIndex + 1)
                {
                    node = this.helper.GetNext(this.currentnode);
                }
            }
            if (node == null)
            {
                node = this.helper.GetNode(this.RootNodes, index);
            }

            this.currentnode = node;
            this.currentIndex = index;
            return node;
        }

        internal TreeNode GetRootNode(TreeNode node, ref List<TreeNode> changedNodes)
        {
            if (node.ParentNode != null)
            {
                changedNodes.Add(node.ParentNode);
                return GetRootNode(node.ParentNode, ref changedNodes);
            }
            return node.ParentNode;
        }

        public void Add(TreeNode item)
        {
            RootNodes.Add(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(TreeNode item)
        {
            if (item.ParentNode == null)
                return RootNodes.Contains(item);

            return FindNode(item, RootNodes);
        }

        private bool FindNode(TreeNode node, TreeNodes nodes)
        {
            foreach (var treeNode in nodes)
            {
                if (treeNode.Equals(node))
                    return true;
                if (treeNode.IsExpanded)
                {
                    var found = FindNode(node, treeNode.ChildNodes);
                    if (found)
                        return true;
                    else
                        continue;
                }
            }
            return false;
        }

        public void CopyTo(TreeNode[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TreeNode item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<TreeNode> GetEnumerator()
        {
            return new TreeNodesEnumerator(RootNodes);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return new TreeNodesEnumerator(RootNodes);
        }
    }
}
