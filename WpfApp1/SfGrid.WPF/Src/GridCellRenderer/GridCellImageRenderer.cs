#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
    [ClassReference(IsReviewed = false)]
    public class GridCellImageRenderer : GridVirtualizingCellRenderer<Image, Image>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellImageRenderer"/> class.
        /// </summary>
        public GridCellImageRenderer()
        {
            IsFocusible = false;
            IsEditable = false;
            SupportsRenderOptimization = false;
        }
        #endregion

        #region Override Methods

#if WPF
        protected override void OnRenderContent(System.Windows.Media.DrawingContext dc, Rect cellRect, System.Windows.Media.Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads  Image control as usual in UseLightweightTemplate true case also.
        }
#endif
        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, Image uiElement, object dataContext)
        {
            InitializeEditUIElement(uiElement, (GridImageColumn)dataColumn.GridColumn);            
            uiElement.SetValue(FrameworkElement.MarginProperty, dataColumn.GridColumn.Padding);            
            uiElement.HorizontalAlignment = TextAlignmentToHorizontalAlignment(dataColumn.GridColumn.TextAlignment);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Initializes the edit unique identifier element.
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        /// <param name="gridImageColumn">The grid image column.</param>
        private void InitializeEditUIElement(Image uiElement, GridImageColumn gridImageColumn)
        {
            if (gridImageColumn == null) return;
            uiElement.SetBinding(Image.SourceProperty, gridImageColumn.ValueBinding);
            uiElement.SetValue(Image.StretchProperty, gridImageColumn.Stretch);                       
            if (!double.IsInfinity(gridImageColumn.ImageHeight))
                uiElement.SetValue(FrameworkElement.HeightProperty, gridImageColumn.ImageHeight);
            if (!double.IsInfinity(gridImageColumn.ImageWidth))
                uiElement.SetValue(FrameworkElement.WidthProperty, gridImageColumn.ImageWidth);
#if WPF
            uiElement.SetValue(Image.StretchDirectionProperty, gridImageColumn.StretchDirection);
#endif
        }


        #endregion
    }
}
