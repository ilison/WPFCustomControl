#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeDataColumn : TreeDataColumnBase
    {
        /// <summary>
        /// Prepares the Column element based on the Visible column 
        /// </summary>
        /// <remarks></remarks>
        protected override FrameworkElement OnInitializeColumnElement(object dataContext, bool isInEdit)
        {
            UIElement element = null;
            if (this.ColumnElement == null && !this.Renderer.UseOnlyRendererElement)
            {
                if (this.ColumnType == TreeColumnType.ExpanderColumn)
                    this.ColumnElement = new TreeGridExpanderCell();
                else
                    this.ColumnElement = new TreeGridCell();
            }
            this.IsEditing = isInEdit;
            
            element = this.Renderer.PrepareUIElements(this, dataContext, this.IsEditing);
            
            this.SetBindings(element);
            return (FrameworkElement)element;
        }

        /// <summary>
        /// When we scroll the Grid vertically row's will be recycled. 
        /// While recycling we need to update the style info of all the cell's in old row.
        /// This property change call back will update the style info of all the cell element when the row index changed.
        /// </summary>
        /// <remarks></remarks>
        public override void UpdateCellStyle()
        {
            if (this.ColumnElement != null && this.Renderer != null)
                this.Renderer.UpdateCellStyle(this);
        }
        /// <summary>
        /// Method which is update the binding and style information of the 
        /// cell when we recycle the cell for scrolling.
        /// </summary>
        /// <remarks></remarks>
        public override void UpdateBinding(object dataContext, bool updateCellStyle = true)
        {
            if (this.Renderer == null)
                return;
            this.Renderer.UpdateBindingInfo(this, dataContext, this.IsEditing);
            if (updateCellStyle)
                this.UpdateCellStyle();
        }
    }
}
