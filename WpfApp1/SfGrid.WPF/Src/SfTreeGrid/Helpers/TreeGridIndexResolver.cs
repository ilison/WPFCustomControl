#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
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
    public static class TreeGridIndexResolver
    {
        public static TreeNode GetNodeAtRowIndex(this SfTreeGrid treeGrid, int rowIndex)
        {
            int nodeindex = treeGrid.ResolveToNodeIndex(rowIndex);
            if (nodeindex > -1 && treeGrid.View.Nodes.Count > nodeindex)
                return treeGrid.View.Nodes[nodeindex];
            return null;
        }
        public static int ResolveToRowIndex(this SfTreeGrid treeGrid, TreeNode node)
        {
            int nodeIndex = treeGrid.View.Nodes.IndexOf(node);
            if (nodeIndex != -1)
                return nodeIndex + treeGrid.HeaderLineCount;  // header index is added
            else
                return -1;
        }
        public static int ResolveToRowIndex(this SfTreeGrid treeGrid, object data)
        {
            int nodeIndex = treeGrid.View.Nodes.GetIndexFromData(data);
            if (nodeIndex != -1)
                return nodeIndex + treeGrid.HeaderLineCount;  // header index is added
            else
                return -1;
        }

        public static int ResolveToRowIndex(this SfTreeGrid treeGrid, int nodeIndex)
        {
            if (nodeIndex <= -1)
                return -1;
            return nodeIndex + treeGrid.HeaderLineCount;  // header index is added
        }

        public static int ResolveToNodeIndex(this SfTreeGrid treeGrid, int rowindex)
        {
            if (rowindex != -1)
                return rowindex - treeGrid.HeaderLineCount;
            return -1;
        }

        /// <summary>
        /// Gets the header index of SfTreeGrid.
        /// </summary>
        /// <param name="grid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>
        /// Returns the header index of the SfTreeGrid.
        /// </returns>
        public static int GetHeaderIndex(this SfTreeGrid treeGrid)
        {
            // To do: if stacked header is added
            return 0;
        }
        public static int ResolveToGridVisibleColumnIndex(this SfTreeGrid treeGrid, int columnIndex)
        {
            var indentColumnCount = treeGrid.showRowHeader ? 1 : 0;
            var visibleColumnIndex = columnIndex - indentColumnCount;
            return visibleColumnIndex;
        }

        /// <summary>
        /// Resolves the scroll column index for the specified column index in SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <param name="gridColumnIndex">
        /// The corresponding column index to get the scroll column index.
        /// </param>
        /// <returns>
        /// Returns the scroll column index for the specified column index.
        /// </returns>
        public static int ResolveToScrollColumnIndex(this SfTreeGrid treeGrid, int gridColumnIndex)
        {
            var indentColumnCount = treeGrid.showRowHeader ? 1 : 0;
            return indentColumnCount + gridColumnIndex;
        }

        /// <summary>
        /// Resolves the start column index of the SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <returns>
        /// Returns the start column index of the SfTreeGrid.
        /// </returns>
        public static int ResolveToStartColumnIndex(this SfTreeGrid treeGrid)
        {
            int startIndex = 0;
            if (treeGrid.showRowHeader)
                startIndex += 1;
            return startIndex;
        }

        /// <summary>
        /// Get the index value for the next focused TreeGridColumn.
        /// </summary>
        /// <param name="TreeGrid"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        internal static int GetNextFocusTreeGridColumnIndex(this SfTreeGrid TreeGrid, int columnIndex, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return GetPreviousFocusTreeGridColumnIndex(TreeGrid, columnIndex);
            var index = columnIndex;
            if (index < 0 || index >= TreeGrid.Columns.Count)
                return -1;
            var gridColumn = TreeGrid.Columns[index];

            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || double.IsNaN(gridColumn.ActualWidth))
            {
                if (columnIndex < TreeGrid.GetLastColumnIndex())
                    index = GetNextFocusTreeGridColumnIndex(TreeGrid, columnIndex + 1);
            }
            return index;
        }

        /// <summary>
        /// Get the Index value for the previous focused GridColumn
        /// </summary>
        /// <param name="TreeGrid"></param>
        /// <param name="columnIndex"></param>
        /// <returns></returns>
        internal static int GetPreviousFocusTreeGridColumnIndex(this SfTreeGrid TreeGrid, int columnIndex, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return GetNextFocusTreeGridColumnIndex(TreeGrid, columnIndex);
            var index = columnIndex;
            if (index < 0 || index >= TreeGrid.Columns.Count)
                return -1;
            var gridColumn = TreeGrid.Columns[index];

            if (gridColumn == null || !gridColumn.AllowFocus || gridColumn.ActualWidth == 0.0 || double.IsNaN(gridColumn.ActualWidth))
            {
                index = GetPreviousFocusTreeGridColumnIndex(TreeGrid, columnIndex - 1);
            }
            return index;
        }
    }
}
