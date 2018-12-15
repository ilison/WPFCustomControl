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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Extensions;
using System.ComponentModel;
using Syncfusion.Data;
using System.Collections.Specialized;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeNodes : IList<TreeNode>, IDisposable
    {
        internal protected List<object> sourceList;
        internal protected List<TreeNode> nodeList;

        public TreeNodes()
        {
            sourceList = new List<object>();
            nodeList = new List<TreeNode>();
        }

        public int IndexOf(TreeNode item)
        {
            return nodeList.IndexOf(item);
        }

        public int IndexOfNode(object item)
        {
            var index = -1;
            var treeNode = nodeList.FirstOrDefault(node => node.Item == item);
            if (treeNode != null)
                index = IndexOf(treeNode);
            return index;
        }

        /// <summary>
        /// Insert the item in nodeList only.
        /// </summary>
        /// <param name="index">index.</param>
        /// <param name="item">item.</param>
        public void Insert(int index, TreeNode item)
        {
            nodeList.Insert(index, item);
        }

        /// <summary>
        /// Insert the item to sourceList and nodeList based on sourceIndex and nodeIndex respectively.
        /// </summary>
        /// <param name="sourceIndex">sourceIndex.</param>
        /// <param name="item">item.</param>
        /// <param name="nodeIndex">nodeIndex.</param>
        public void Insert(int sourceIndex, TreeNode item, int nodeIndex)
        {
            sourceList.Insert(sourceIndex, item.Item);
            Insert(nodeIndex, item);
        }

        public void RemoveRange(int index, int count)
        {
            for (int i = index + count - 1; i > index - 1; i--)
            {
                nodeList.RemoveAt(i);
                sourceList.RemoveAt(i);
            }
        }

        internal void Sort(SortColumnDescriptions descriptions, SortComparers comparers)
        {
            nodeList = SortNodes(descriptions, comparers);
        }

        private List<TreeNode> SortNodes(SortColumnDescriptions sortColumnDescriptions, SortComparers sortComparers)
        {
            var list = nodeList;
            IOrderedEnumerable<TreeNode> source = null;
            for (int i = 0; i < sortColumnDescriptions.Count; i++)
            {
                var sortDescription = sortColumnDescriptions[i];
                var customComparer = sortComparers[sortDescription.ColumnName];
                if (sortDescription.SortDirection == ListSortDirection.Ascending)
                {
                    if (i == 0)
                    {
                        if (customComparer == null)
                            source = list.OrderBy(node => TreeGridHelper.GetValue(node.Item, sortDescription.ColumnName));
                        else
                        {
                            source = list.OrderBy(node => node.Item, customComparer);
                        }
                    }
                    else
                    {
                        if (customComparer == null)
                            source = source.ThenBy(node => TreeGridHelper.GetValue(node.Item, sortDescription.ColumnName));
                        else
                        {
                            source = source.ThenBy(node => node.Item, customComparer);
                        }
                    }
                }
                else
                {
                    if (i == 0)
                    {
                        if (customComparer == null)
                            source = list.OrderByDescending(node => TreeGridHelper.GetValue(node.Item, sortDescription.ColumnName));
                        else
                        {
                            source = list.OrderByDescending(node => node.Item, customComparer);
                        }
                    }
                    else
                    {
                        if (customComparer == null)
                            source = source.ThenByDescending(node => TreeGridHelper.GetValue(node.Item, sortDescription.ColumnName));
                        else
                        {
                            source = source.ThenByDescending(node => node.Item, customComparer);
                        }
                    }
                }
            }
            return source.ToList();
        }
        /// <summary>
        /// Get the node from data.
        /// </summary>
        /// <param name="data">data.</param>
        /// <returns>the TreeNode.</returns>
        public TreeNode GetNode(object data)
        {
            return this.nodeList.FirstOrDefault(n => n.Item == data);
        }

        public void RemoveAt(int index)
        {
            sourceList.RemoveAt(index);
            nodeList.RemoveAt(index);
        }


        public void MoveTo(int sourceIndex, int destinationIndex)
        {
            sourceList.MoveTo(sourceIndex, destinationIndex);
            nodeList.MoveTo(sourceIndex, destinationIndex);
        }

        public TreeNode this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    return null;
                return nodeList[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                    throw new IndexOutOfRangeException("index is out of TreeNodes count or less than zero");
                nodeList[index] = value;
                sourceList[index] = (value as TreeNode).Item;
            }
        }

        public void Add(TreeNode item)
        {
            nodeList.Add(item);
            sourceList.Add(item.Item);
        }

        public void Clear()
        {
            if (sourceList != null)
                sourceList.Clear();
          
            if (nodeList != null)
            {
                if (nodeList.Any())
                {
                    var parentNode = nodeList[0].ParentNode;
                    if (parentNode != null)
                    {
                        parentNode.SetHasChildNodes(false);
                    }
                }
                nodeList.Clear();
            }
        }
        
        public bool Contains(TreeNode item)
        {
            return nodeList.Contains(item);
        }

        public void CopyTo(TreeNode[] array, int arrayIndex)
        {
            nodeList.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get
            {
                return nodeList.Count;
            }
        }

        /// <summary>
        /// Arrange nodeList based on sourceList order
        /// </summary>
        internal void RefreshNodes()
        {
            var orderedList = from item in sourceList
                              join node in nodeList
                              on item equals node.Item
                              select node;
            nodeList = orderedList.ToList();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TreeNode item)
        {
            sourceList.Remove(item.Item);
            return nodeList.Remove(item);
        }

        public virtual IEnumerator<TreeNode> GetEnumerator()
        {
            return this.nodeList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.nodeList.GetEnumerator();
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodes"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeNodes"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (nodeList != null)
            {
                foreach (var node in nodeList)
                    node.Dispose();
                nodeList.Clear();
                nodeList = null;
            }
            if (sourceList != null)
            {
                sourceList.Clear();
                sourceList = null;
            }
        }
    }
}
