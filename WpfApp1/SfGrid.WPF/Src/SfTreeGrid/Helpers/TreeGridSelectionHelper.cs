#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if UWP
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Helpers
{
    public static class TreeGridSelectionHelper
    {
        /// <summary>
        /// Shows the selection background for the specified row index.
        /// </summary>
        /// <param name="treeGrid"> 
        /// The corresponding SfTreeGrid to enable the selection background.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to enable the selection background.
        /// </param>
        /// <remarks>
        /// The selection background applied based on the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionBackground"/> property.
        /// </remarks>
        internal static void ShowRowSelection(this SfTreeGrid treeGrid, int rowIndex, TreeNode treeNode = null)
        {
            TreeDataRowBase row = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null)
            {
                row.IsSelectedRow = true;
            }
            if (treeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.Default || treeNode == null)
                return;

            if (treeNode.IsChecked != true)
                treeGrid.NodeCheckBoxController.SetIsCheckedState(treeNode, true);
        }

        /// <summary>
        /// Hides the selection background for the specified row index.
        /// </summary>
        /// <param name="treeGrid"> 
        /// The corresponding treeGrid to hide the selection background.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to hide the selection background.
        /// </param>      
        internal static void HideRowSelection(this SfTreeGrid treeGrid, int rowIndex, TreeNode treeNode = null)
        {
            TreeDataRowBase row = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null)
            {
                row.IsSelectedRow = false;
            }
            if (treeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.Default || treeNode == null)
                return;

            if (treeNode.IsChecked == true)
                treeGrid.NodeCheckBoxController.SetIsCheckedState(treeNode, false);
        }

        /// <summary>
        /// Hides the row focus border for the specified row index.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to hide the row focus border.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to hide row focus border.
        /// </param>
        internal static void HideRowFocusBorder(this SfTreeGrid treeGrid)
        {
            TreeDataRowBase row = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.IsFocusedRow);
            if (row != null)
            {
                row.IsFocusedRow = false;
            }
        }


        /// <summary>
        /// Shows the row focus border for the specified row index.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to enable the row focus border.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding row index to enable row focus border.
        /// </param>
        internal static void ShowRowFocusBorder(this SfTreeGrid treeGrid, int rowIndex)
        {
            if (treeGrid.NavigationMode != NavigationMode.Row)
                return;
            TreeDataRowBase row = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowIndex);
            if (row != null && !row.IsSelectedRow)
            {
                row.IsFocusedRow = true;
            }
        }
        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataColumnBase"/> for the given RowColumnIndex.
        /// </summary>
        /// <param name="TreeGrid">
        /// The corresponding treeGrid to get TreeDataColumnBase.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to get the TreeDataColumnBase.
        /// </param>
        /// <returns>
        /// Returns the corresponding <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeDataColumnBase"/> for the specified rowcolumnindex.
        /// </returns>       
        public static TreeDataColumnBase GetTreeDataColumnBase(this SfTreeGrid treeGrid, RowColumnIndex rowColumnIndex)
        {
            // The current row column index cannot be decided while key down operations. will be ensured in Ensure rows and Ensure columns.
            if (rowColumnIndex.RowIndex == -1 || rowColumnIndex.ColumnIndex == -1)
                return null;

            var dataRow = treeGrid.RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);

            if (dataRow != null)
            {
                var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
                return dataColumn;
            }
            return null;
        }

        /// <summary>
        /// Gets the index of the DataRow positioned at start of the SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to get index of first DataRow.
        /// </param>
        /// <returns>
        /// Returns the index of first DataRow in SfTreeGrid.If the node count is zero, return -1.
        /// </returns>
        public static int GetFirstDataRowIndex(this SfTreeGrid treeGrid)
        {
            if (!treeGrid.HasView || treeGrid.View.Nodes.Count == 0)
                return -1;

            int index = treeGrid.HeaderLineCount;
            int count = 0;
            for (int start = index; start <= treeGrid.TreeGridPanel.RowCount; start++)
            {
                if (!treeGrid.TreeGridPanel.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }

        /// <summary>
        /// Gets the index of the DataRow positioned at end of the SfTreeGrid.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to get index of last DataRow.
        /// </param>
        /// <returns>
        /// Returns the index of last DataRow in SfTreeGrid.
        /// </returns>
        public static int GetLastDataRowIndex(this SfTreeGrid treeGrid)
        {
            if (!treeGrid.HasView || treeGrid.View.Nodes.Count == 0)
                return -1;
            int count = 0;
            int index = treeGrid.TreeGridPanel.RowCount - 1;
            var headerLineCount = treeGrid.HeaderLineCount;
            for (int start = index; start >= headerLineCount; start--)
            {
                if (!treeGrid.TreeGridPanel.RowHeights.GetHidden(start, out count))
                    return start;
            }
            return index;
        }


        /// <summary>
        /// Updates the visual state of the RowHeader based on the current cell or row changed.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to update the visual state of RowHeader.
        /// </param>
        public static void UpdateRowHeaderState(this SfTreeGrid treeGrid)
        {
            var currentRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.IsCurrentRow);
            if (currentRow != null)
            {
                currentRow.IsCurrentRow = false;
                (currentRow as TreeDataRowBase).ApplyRowHeaderVisualState();
            }

            if (treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex < 0)
                return;

            currentRow = treeGrid.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            if (currentRow != null)
            {
                currentRow.IsCurrentRow = true;
                (currentRow as TreeDataRowBase).ApplyRowHeaderVisualState();
            }
        }


        /// <summary>
        /// Gets the index of the row positioned at the end of next page that is not currently in view of SfTreeGrid.
        /// </summary>
        /// <returns>
        /// Returns the end row index of next page.
        /// </returns>
        public static int GetNextPageIndex(this SfTreeGrid treeGrid)
        {
            var rowIndex = treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex;
            if (rowIndex < treeGrid.GetFirstDataRowIndex())
                rowIndex = 0;
            var nextPageIndex = treeGrid.TreeGridPanel.ScrollRows.GetNextPage(rowIndex);
            var lastRowIndex = treeGrid.GetLastDataRowIndex();
            nextPageIndex = nextPageIndex <= lastRowIndex ? nextPageIndex : lastRowIndex;
            return nextPageIndex;
        }

        /// <summary>
        /// Gets the index of the row positioned at the start of the previous page that is not currently in view of SfTreeGrid.
        /// </summary>
        /// <returns>
        /// The start index of previous page.
        /// </returns>
        public static int GetPreviousPageIndex(this SfTreeGrid treeGrid)
        {
            int previousPageIndex = treeGrid.TreeGridPanel.ScrollRows.GetPreviousPage(treeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex);
            var firstRowIndex = treeGrid.GetFirstDataRowIndex();
            previousPageIndex = previousPageIndex < firstRowIndex ? firstRowIndex : previousPageIndex;
            return previousPageIndex;
        }

        /// <summary>
        /// Gets the next row info at the specified row index in SfTreeGrid.
        /// </summary>
        /// <param name="TreeGrid">
        /// The corresponding TreeGrid to get next row info.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of row to get next row info
        /// </param>
        /// <returns>
        /// Returns the next row info of the specified row index.
        /// </returns>
        public static TreeGridRowInfo GetNextRowInfo(this SfTreeGrid TreeGrid, int rowIndex)
        {
            if (rowIndex < 0)
                return null;

            if (rowIndex == TreeGrid.GetLastDataRowIndex() &&
               rowIndex == TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
            {
                rowIndex = TreeGrid.TreeGridPanel.ScrollRows.GetPreviousScrollLineIndex(rowIndex);
            }

            if (rowIndex > TreeGrid.GetLastDataRowIndex() || rowIndex < TreeGrid.GetFirstDataRowIndex())
                return null;

            var rowInfo = (TreeGrid.SelectionController as TreeGridBaseSelectionController).GetTreeGridSelectedRow(rowIndex);
            return rowInfo;
        }

        /// <summary>
        /// Gets the previous row info at the specified row index of SfTreeGrid.
        /// </summary>
        /// <param name="TreeGrid">
        /// The corresponding TreeGrid to get previous row info.
        /// </param>
        /// <param name="rowIndex">
        /// The corresponding index of row to get previous row info.
        /// </param>
        /// <returns>
        /// Returns the previous row info of specified row index.
        /// </returns>
        public static TreeGridRowInfo GetPreviousRowInfo(this SfTreeGrid TreeGrid, int rowIndex)
        {
            if (rowIndex < 0)
                throw new InvalidOperationException("Negative rowIndex in GetNextRecordEntry");

            if (rowIndex == TreeGrid.GetFirstDataRowIndex() &&
                rowIndex == TreeGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.RowIndex)
            {
                rowIndex = TreeGrid.TreeGridPanel.ScrollRows.GetNextScrollLineIndex(rowIndex);
            }

            //The Below condition is Checked with while Editing the Record if it gets the View then we have to give the Selection.
            if (rowIndex > TreeGrid.GetLastDataRowIndex() || rowIndex < TreeGrid.GetFirstDataRowIndex())
                return null;

            var rowInfo = (TreeGrid.SelectionController as TreeGridBaseSelectionController).GetTreeGridSelectedRow(rowIndex);
            return rowInfo;
        }
        /// <summary>
        /// Returns the TreeGridColumn for the given RowColumnIndex
        /// </summary>
        /// <param name="columnIndex">Corresponding ColumnIndex Value</param>
        /// <remarks></remarks>
        internal static TreeGridColumn GetTreeGridColumn(this SfTreeGrid treeGrid, int columnIndex)
        {
            var index = treeGrid.ResolveToGridVisibleColumnIndex(columnIndex);
            return index >= 0 && index < treeGrid.Columns.Count ? treeGrid.Columns[index] : null;
        }


        /// <summary>
        /// Gets the index of the first column corresponding to the specified flow direction.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to get the index of first column.
        /// </param>
        /// <param name="flowdirection">
        /// Corresponding direction to get the index of first column.
        /// </param>
        /// <returns>
        /// Returns the index of first column.
        /// </returns>
        public static int GetFirstColumnIndex(this SfTreeGrid treeGrid, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return treeGrid.GetLastColumnIndex();

            int firstColumnIndex = treeGrid.Columns.IndexOf(treeGrid.Columns.FirstOrDefault(col => col.ActualWidth != 0d && !double.IsNaN(col.ActualWidth)));
            //CurrentCell is updated when clicking on RowHeader when there is no columns in view, hence the below condition is added.
            if (firstColumnIndex < 0)
                return firstColumnIndex;
            firstColumnIndex = treeGrid.ResolveToScrollColumnIndex(firstColumnIndex);
            return firstColumnIndex;
        }

        /// <summary>
        /// Gets the index of the last column corresponding to the specified flow direction.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding treeGrid to get the index of last column.
        /// </param>
        /// <param name="flowdirection">
        /// Corresponding direction to get the index of last column.
        /// </param>
        /// <returns>
        /// Returns the index of last column.
        /// </returns>
        public static int GetLastColumnIndex(this SfTreeGrid treeGrid, FlowDirection flowdirection = FlowDirection.LeftToRight)
        {
            if (flowdirection == FlowDirection.RightToLeft)
                return treeGrid.GetFirstColumnIndex();

            int lastIndex = treeGrid.Columns.IndexOf(treeGrid.Columns.LastOrDefault(col => col.ActualWidth != 0d && !double.IsNaN(col.ActualWidth)));
            //CurrentCell is updated when clicking on RowHeader when there is no columns in view, hence the below condition is added.
            if (lastIndex < 0)
                return lastIndex;
            lastIndex = treeGrid.ResolveToScrollColumnIndex(lastIndex);
            return lastIndex;
        }

        /// <summary>
        /// Determines whether the corresponding cell in a column is focusable or not.
        /// </summary>
        /// <param name="treeGrid">
        /// The SfTreeGrid.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowcolumnindex to check whether the cells in a column is focusable or not. 
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the cells in a column is focusable; otherwise, <b>false</b>.
        /// </returns>
        public static bool AllowFocus(this SfTreeGrid treeGrid, RowColumnIndex rowColumnIndex)
        {
            var gridColumn = treeGrid.GetTreeGridColumn(rowColumnIndex.ColumnIndex);
            if (gridColumn != null)
                return gridColumn.ActualWidth != 0d && !double.IsNaN(gridColumn.ActualWidth) && gridColumn.AllowFocus;
            return false;
        }
    }
}
