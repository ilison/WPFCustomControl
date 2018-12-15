#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
#endif


namespace Syncfusion.UI.Xaml.Grid.Cells
{
    [ClassReference(IsReviewed = false)]
    public class GridDataHeaderCellRenderer : GridVirtualizingCellRenderer<GridHeaderCellControl,GridHeaderCellControl>
    {
        public GridDataHeaderCellRenderer()
        {
            SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
        }

#if WPF
        protected override void OnRenderCell(System.Windows.Media.DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            
        }
#endif
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, GridHeaderCellControl uiElement,object dataContext)
        {
            
        }

        public override void OnUpdateDisplayBinding(DataColumnBase dataColumn, GridHeaderCellControl uiElement, object dataContext)
        {
            
        }

        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridHeaderCellControl uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
#if WPF
            if (!DataGrid.IsLoaded)
                column = column.Clone() as GridColumn;
#endif
            uiElement.Column = column;
            uiElement.DataGrid = this.DataGrid;
            if (column != null)
            {
                if (column.HeaderText == null)
                    column.HeaderText = column.MappingName;
                uiElement.SetValue(ContentControl.ContentProperty, column.HeaderText);
                //We need bind the HorizontalHeaderContentAlignment value if only column has HorizontalHeaderContentAlignment value
                if (column.ReadLocalValue(GridColumn.HorizontalHeaderContentAlignmentProperty) != DependencyProperty.UnsetValue)                                    
                    uiElement.SetValue(Control.HorizontalContentAlignmentProperty, column.HorizontalHeaderContentAlignment);                
                else
                    uiElement.ClearValue(Control.HorizontalContentAlignmentProperty);          
                uiElement.Update();
                uiElement.DataContext = column;
            }
        }

        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridHeaderCellControl element, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
#if WPF
            if (DataGrid != null && !DataGrid.IsLoaded)
                column = column.Clone() as GridColumn;
#endif
            element.ClearValue(GridHeaderCellControl.ContentProperty);
            element.Column = column;
            if (column.HeaderText == null)
                column.HeaderText = column.MappingName;
            element.SetValue(ContentControl.ContentProperty, column.HeaderText);
            //We need bind the HorizontalHeaderContentAlignment value if only column has HorizontalHeaderContentAlignment value
            if (column.ReadLocalValue(GridColumn.HorizontalHeaderContentAlignmentProperty) != DependencyProperty.UnsetValue)            
                element.SetValue(Control.HorizontalContentAlignmentProperty, column.HorizontalHeaderContentAlignment);            
            else
                element.ClearValue(Control.HorizontalContentAlignmentProperty); 

            element.Update();
            element.DataContext = column;
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {            
            var cell = dataColumn.ColumnElement;
            var column = dataColumn.GridColumn;
            var cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);

            var control = cell as GridHeaderCellControl;
            
            if (control != null && DataGrid != null && column != null)
            {
                if (this.DataGrid.AllowResizingColumns && this.DataGrid.AllowResizingHiddenColumns)
                {
                    var columnIndex = this.DataGrid.ResolveToGridVisibleColumnIndex(cellRowColumnIndex.ColumnIndex);
                    this.DataGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(-1, columnIndex);
                }

                // UWP-4721 Reset the style while resuing a column with HeaderStyle for GridHeaderCellControl.
                if (!column.hasHeaderStyle && !DataGrid.hasHeaderStyle)
                {
                    if (control.ReadLocalValue(GridHeaderCellControl.StyleProperty) != DependencyProperty.UnsetValue)
                        control.ClearValue(GridHeaderCellControl.StyleProperty);
                }
                else
                {
                    Style style = null;
                    if (column.hasHeaderStyle)
                        style = column.HeaderStyle;
                    else if (DataGrid.hasHeaderStyle)
                        style = DataGrid.HeaderStyle;
                    // WPF-35780 - When the Headerstyle is explicitly set null value for a column whether DataGrid Headerstyle is enabled,
                    // we need to clear the style for the column.
                    if (style != null)
                        control.Style = style;
                    else
                        control.ClearValue(GridHeaderCellControl.StyleProperty);
                }

                if (!column.hasHeaderTemplate && !DataGrid.hasHeaderTemplate)
                    control.ClearValue(GridHeaderCellControl.ContentTemplateProperty);
                else if (column.hasHeaderTemplate)
                    control.ContentTemplate = column.HeaderTemplate;
                else if (DataGrid.hasHeaderTemplate)
                    control.ContentTemplate = DataGrid.HeaderTemplate;
            }
        }
       

        public override bool CanUpdateBinding(GridColumn column)
        {
            return false;
        }
        //protected override void OnUnwireUIElement(GridHeaderCellControl uiElement)
        //{
        //    base.OnUnwireUIElement(uiElement);
        //    uiElement.ClearValue(GridHeaderCellControl.ContentProperty);
        //}
    }

    public class GridStackedHeaderCellRenderer : GridVirtualizingCellRenderer<GridStackedHeaderCellControl, GridStackedHeaderCellControl>
    {
        public GridStackedHeaderCellRenderer()
        {
            SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
        }

#if WPF
        protected override void OnRenderCell(System.Windows.Media.DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            
        }
#endif

        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, GridStackedHeaderCellControl uiElement, object dataContext)
        {
            throw new NotImplementedException();
        }

        public override void OnUpdateDisplayBinding(DataColumnBase dataColumn, GridStackedHeaderCellControl uiElement, object dataContext)
        {
            throw new NotImplementedException();
        }

        public override void OnInitializeEditElement(DataColumnBase dataColumn, GridStackedHeaderCellControl uiElement, object dataContext)
        {
            var bind = new Binding { Path = new PropertyPath("HeaderText"), Mode = BindingMode.TwoWay };
            uiElement.SetBinding(ContentControl.ContentProperty, bind);
            uiElement.DataContext = dataContext;
        }
        public override void OnUpdateEditBinding(DataColumnBase dataColumn, GridStackedHeaderCellControl element, object dataContext)
        {
            element.ClearValue(GridStackedHeaderCellControl.ContentProperty);
            var bind = new Binding { Path = new PropertyPath("HeaderText"), Mode = BindingMode.TwoWay };
            element.SetBinding(GridHeaderCellControl.ContentProperty, bind);
        }
        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            //Since We don't want to Initialize Custom Style for Stacked Header Style, We are blocking the Call to the Method.
            //base.InitializeRendererCellStyle(cellRowColumnIndex, record, cell, column);
        }
    }
}
