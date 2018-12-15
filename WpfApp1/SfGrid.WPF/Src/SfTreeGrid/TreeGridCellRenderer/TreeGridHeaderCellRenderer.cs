#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    public class TreeGridHeaderCellRenderer : TreeGridVirtualizingCellRenderer<TreeGridHeaderCell, TreeGridHeaderCell>
    {
        public TreeGridHeaderCellRenderer()
        {
            SupportsRenderOptimization = false;
            this.UseOnlyRendererElement = true;
        }

        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, TreeGridHeaderCell uiElement, object dataContext)
        {

        }

        public override void OnUpdateDisplayBinding(TreeDataColumnBase dataColumn, TreeGridHeaderCell uiElement, object dataContext)
        {

        }

        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, TreeGridHeaderCell uiElement, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            //#if WPF
            //            if (!TreeGrid.IsLoaded)
            //                column = column.Clone() as TreeGridColumn;
            //#endif           
            uiElement.TreeGrid = this.TreeGrid;
            uiElement.Column = dataColumn.TreeGridColumn;
            if (column != null)
            {
                if (column.HeaderText == null)
                    column.HeaderText = column.MappingName;
                uiElement.SetValue(ContentControl.ContentProperty, column.HeaderText);
                //We need bind the HorizontalHeaderContentAlignment value if only column has HorizontalHeaderContentAlignment value
                if (column.ReadLocalValue(TreeGridColumn.HorizontalHeaderContentAlignmentProperty) != DependencyProperty.UnsetValue)
                    uiElement.SetValue(Control.HorizontalContentAlignmentProperty, column.HorizontalHeaderContentAlignment);
                else
                    uiElement.ClearValue(Control.HorizontalContentAlignmentProperty);  
                uiElement.Update();
                uiElement.DataContext = column;
            }
        }

        public override void OnUpdateEditBinding(TreeDataColumnBase dataColumn, TreeGridHeaderCell element, object dataContext)
        {
            TreeGridColumn column = dataColumn.TreeGridColumn;
            // WPF-37159 - No need to clone the column. So below line is commented.
//#if WPF
//            if (TreeGrid != null && !TreeGrid.IsLoaded)
//                column = column.Clone() as TreeGridColumn;
//#endif
            element.ClearValue(TreeGridHeaderCell.ContentProperty);
            element.Column = column;
            if (column.HeaderText == null)
                column.HeaderText = column.MappingName;
            element.SetValue(ContentControl.ContentProperty, column.HeaderText);
            //We need bind the HorizontalHeaderContentAlignment value if only column has HorizontalHeaderContentAlignment value
            if (column.ReadLocalValue(TreeGridColumn.HorizontalHeaderContentAlignmentProperty) != DependencyProperty.UnsetValue)
                element.SetValue(Control.HorizontalContentAlignmentProperty, column.HorizontalHeaderContentAlignment);
            else
                element.ClearValue(Control.HorizontalContentAlignmentProperty);

            element.Update();
            element.DataContext = column;
        }

        protected override void InitializeCellStyle(TreeDataColumnBase treeDataColumn, object record)
        {
            var cell = treeDataColumn.ColumnElement;
            var column = treeDataColumn.TreeGridColumn;
            var cellRowColumnIndex = new RowColumnIndex(treeDataColumn.RowIndex, treeDataColumn.ColumnIndex);

            var control = cell as TreeGridHeaderCell;

            if(control != null && TreeGrid != null && column != null)
            {
                if (this.TreeGrid.AllowResizingColumns && this.TreeGrid.AllowResizingHiddenColumns)
                {
                    var columnIndex = this.TreeGrid.ResolveToGridVisibleColumnIndex(cellRowColumnIndex.ColumnIndex);
                    this.TreeGrid.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(-1, columnIndex);
                }
                // UWP-4721 Reset the style while resuing a column with HeaderStyle for GridHeaderCellControl.
                if (!column.hasHeaderStyle && !TreeGrid.hasHeaderStyle)
                {
                    if (control.ReadLocalValue(GridHeaderCellControl.StyleProperty) != DependencyProperty.UnsetValue)
                        control.ClearValue(GridHeaderCellControl.StyleProperty);
                }
                else
                {
                    Style style = null;
                    if (column.hasHeaderStyle)
                        style = column.HeaderStyle;
                    else if (TreeGrid.hasHeaderStyle)
                        style = TreeGrid.HeaderStyle;
                    // WPF-35780 - When the Headerstyle is explicitly set null value for a column whether TreeGrid Headerstyle is enabled,
                    // we need to clear the style for the column.
                    if (style != null)
                        control.Style = style;
                    else
                        control.ClearValue(GridHeaderCellControl.StyleProperty);
                }

                if (!column.hasHeaderTemplate && !TreeGrid.hasHeaderTemplate)
                    control.ClearValue(GridHeaderCellControl.ContentTemplateProperty);
                else if (column.hasHeaderTemplate)
                    control.ContentTemplate = column.HeaderTemplate;
                else if (TreeGrid.hasHeaderTemplate)
                    control.ContentTemplate = TreeGrid.HeaderTemplate;
            }
        }

        public override bool CanUpdateBinding(TreeGridColumn column)
        {
            return false;
        }
    }

    /// <summary>
    /// Provides classes and interface to renderer different cells in SfTreeGrid. 
    /// </summary>
    class NamespaceDoc
    {

    }
}
