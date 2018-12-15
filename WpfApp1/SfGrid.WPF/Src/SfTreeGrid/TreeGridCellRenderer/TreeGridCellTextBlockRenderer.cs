#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
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
    public class TreeGridCellTextBlockRenderer: TreeGridVirtualizingCellRenderer<TextBlock, TextBlock>
    {
        public TreeGridCellTextBlockRenderer()
        {
            SupportsRenderOptimization = false;           
            IsEditable = false;
            IsFocusable = false;
        }
        /// <summary>
        /// Method which is initialize the Renderer element Bindings with corresponding column values.
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext"></param>
        /// <remarks></remarks>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            uiElement.SetBinding(TextBlock.TextProperty, dataColumn.TreeGridColumn.ValueBinding);
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
        }
    }
}
