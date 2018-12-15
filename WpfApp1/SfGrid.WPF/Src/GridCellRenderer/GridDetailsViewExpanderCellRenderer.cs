#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
#if !WinRT && !UNIVERSAL
using System.Windows;
#else
using Windows.UI.Xaml;
#endif
using Syncfusion.UI.Xaml.ScrollAxis;


namespace Syncfusion.UI.Xaml.Grid.Cells
{
    public class GridDetailsViewExpanderCellRenderer : GridVirtualizingCellRenderer<GridDetailsViewExpanderCell, GridDetailsViewExpanderCell>
    {
        public GridDetailsViewExpanderCellRenderer()
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
        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridDetailsViewExpanderCell uiElement, object dataContext)
        {
            RowColumnIndex rowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            uiElement.DataGrid = DataGrid;
            uiElement.RowColumnIndex = rowColumnIndex;
        }

        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridDetailsViewExpanderCell element,  object dataContext)
        {
            RowColumnIndex cellRowcolumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            element.RowColumnIndex = cellRowcolumnIndex;
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var element = cell as GridDetailsViewExpanderCell;
            if (element != null) element.RowColumnIndex = cellRowColumnIndex;
            base.InitializeCellStyle(dataColumn,record);
        }
    }
}
