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
#endif
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    class GridRowHeaderCellRenderer : GridVirtualizingCellRenderer<GridRowHeaderCell, GridRowHeaderCell>
    {
        public GridRowHeaderCellRenderer()
        {
            
            this.SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
            IsFocusible = false;
            IsEditable = false;
        }
#if WPF
        protected override void OnRenderCell(System.Windows.Media.DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            
        }      
#endif

        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridRowHeaderCell uiElement, object dataContext)
        {
            RowColumnIndex rowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);         
            if(uiElement is GridRowHeaderCell)
                (uiElement as GridRowHeaderCell).RowIndex = rowColumnIndex.RowIndex; 
        }

        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridRowHeaderCell element,object dataContext)
        {

        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var element = cell as GridRowHeaderCell;
            if (element != null)
                element.RowIndex = cellRowColumnIndex.RowIndex;
            base.InitializeCellStyle(dataColumn,record);
        }
    }
}
