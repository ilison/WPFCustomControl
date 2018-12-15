#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syncfusion.Data.Extensions;
using Syncfusion.Data;
using System.Collections;
using Syncfusion.UI.Xaml.Grid;

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeNodeComparer : IComparer<TreeNode>
    {
        internal TreeGridView treeGridView;
        internal bool hasComparer;

        public TreeNodeComparer(TreeGridView treeGridView)
        {
            this.treeGridView = treeGridView;
            hasComparer = treeGridView.SortComparers.Any();
        }

        public int Compare(TreeNode x, TreeNode y)
        {
            int c = 0;
            foreach (SortColumnDescription sortColumnDescription in treeGridView.SortDescriptions)
            {
                if (hasComparer)
                {
                    var comparer = this.treeGridView.SortComparers[sortColumnDescription.ColumnName];
                    if (comparer != null)
                    {
                        (comparer as ISortDirection).SortDirection = sortColumnDescription.SortDirection;
                        c = comparer.Compare(x.Item, y.Item);
                    }
                    else
                    {
                        c = Compare(x, y, sortColumnDescription);
                    }
                }
                else
                {
                    c = Compare(x, y, sortColumnDescription);
                }
                if (c != 0)
                    break;
                else
                    continue;
            }

            return c;
        }

        public int Compare(TreeNode x, TreeNode y, SortColumnDescription sortDescription)
        {
            int c = 0;
            IComparable xc = null;
            IComparable yc = null;
#if WPF
            PropertyDescriptorCollection descriptor1 = TypeDescriptor.GetProperties(x.Item.GetType());
#else
                                PropertyInfoCollection descriptor1 = new PropertyInfoCollection(x.Item.GetType());
#endif           
            xc = descriptor1.GetValue(x.Item, sortDescription.ColumnName) as IComparable;

#if WPF
            PropertyDescriptorCollection descriptor2 = TypeDescriptor.GetProperties(y.Item.GetType());
#else
                                PropertyInfoCollection descriptor2 = new PropertyInfoCollection(y.Item.GetType());
#endif           
            yc = descriptor1.GetValue(y.Item, sortDescription.ColumnName) as IComparable;

            if (xc != null)
            {
                c = xc.CompareTo(yc);              
            }
            else if (yc != null)
            {
                c = -1;
            }

            if (sortDescription.SortDirection == ListSortDirection.Descending)
                c = -c;
            return c;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class IndexComparer : IComparer<TreeNode>
    {
        IEnumerable<object> SourceCollection;

        public IndexComparer(TreeGridView treeGridView)
        {
            SourceCollection = treeGridView.SourceCollection.ToList<object>();
        }

        public int Compare(TreeNode x, TreeNode y)
        {
            int c = 0;
            var index1 = SourceCollection.IndexOf(x.Item);
            var index2 = SourceCollection.IndexOf(y.Item);
            c = index1.CompareTo(index2);
            return c;
        }
    }
}
