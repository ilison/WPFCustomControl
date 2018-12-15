#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
#if WPF
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    public class TreeGridRowHeaderCellRenderer : TreeGridVirtualizingCellRenderer<TreeGridRowHeaderCell, TreeGridRowHeaderCell>
    {
        public TreeGridRowHeaderCellRenderer()
        {
            this.SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
            IsFocusable = false;
            IsEditable = false;
        }

        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, TreeGridRowHeaderCell uiElement, object dataContext)
        {
            RowColumnIndex rowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            if (uiElement is TreeGridRowHeaderCell)
                (uiElement as TreeGridRowHeaderCell).RowIndex = rowColumnIndex.RowIndex;
        }

        protected override void InitializeCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var element = cell as TreeGridRowHeaderCell;
            if (element != null)
                element.RowIndex = cellRowColumnIndex.RowIndex;
            base.InitializeCellStyle(dataColumn, record);
        }
    }
}
