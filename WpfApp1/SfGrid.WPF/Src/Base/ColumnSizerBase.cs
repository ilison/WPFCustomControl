#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.TreeGrid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Syncfusion.UI.Xaml.Grid.Cells;


#if UWP
using Windows.UI.Xaml.Controls.Primitives;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Text;
#else
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that provides the base implementation to calculate column widths based on different column sizer options for SfDataGrid(<see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/>) and SfTreeGrid(<see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer"/>).
    /// </summary>
    public class ColumnSizerBase<T> : IDisposable where T : SfGridBase
    {
        internal T GridBase { get; set; }
        private bool isdisposed = false;
        internal GridColumnBase cachedColumn;
        internal int textLength = 0;
        internal double prevColumnWidth = 0;

        internal bool isInSuspend = false;



#if UWP
        private double fontSize = 16;
        private Thickness margin = new Thickness(8, 4, 8, 4);
        private FontStretch fontStretch = FontStretch.Normal;
        private FontWeight fontWeight = FontWeights.Normal;
#else
        private double fontSize = 12;
         private bool allowMeasureByFormattedText = true;
        private Thickness margin = new Thickness(5, 1, 5, 1);
         private FontStretch fontStretch = new FontStretch();
        private FontWeight fontWeight = new FontWeight();
#endif
        private FontFamily fontFamily = new FontFamily("Segoe UI");

        internal AutoFitMode columnAutoFitMode;

        /// <summary>
        /// Gets or sets a value that indicates the mode for calculating the width of the cell based on cell content. <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode.SmartFit"/> calculates the column width in optimized way. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode"/> enumeration that specifies the way to measure the width of the corresponding column.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode.SmartFit"/>. 
        /// </value>
        public AutoFitMode AutoFitMode
        {
            get;
            set;
        }

        private double sortIconWidth = 25;
        /// <summary>
        /// Gets or sets the width of sort icon for column sizing calculation..
        /// </summary>
        /// <value>
        /// The width of the sort icon. The default value is 25.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToHeader"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> type of column sizer calculates the column width based on static sort icon width.
        /// When the sort icon width is customized, that is need to be initialized to this property for customizing column sizer calculation based on new sort icon width.
        /// </remarks>
        public double SortIconWidth
        {
            get { return sortIconWidth; }
            set { sortIconWidth = value; }
        }

        /// <summary>
        /// Gets or sets the font size to compute the column width.
        /// </summary>
        /// <value>
        /// The size of the font. The default value is 12.
        /// </value>
        /// <remarks>
        /// By default the column width calculation based on fixed font size.
        /// When the font size is customized, that is need to be initialized to this property for customizing column sizer calculation based on new font size.
        /// </remarks>
        public double FontSize
        {
            get { return fontSize; }
            set { fontSize = value; }
        }

        /// <summary>
        /// Gets or sets the FontFamily to compute the column width.
        /// </summary>
        /// <value>
        /// The FontFamily to compute the column width. The default value is <b>Segoe UI</b>.
        /// </value>
        /// <remarks>
        /// By default the column width calculation based on fixed font family.
        /// When the font family is customized, that is need to be initialized to this property for customizing column sizer calculation based on new font family.
        /// </remarks>
        public FontFamily FontFamily
        {
            get { return fontFamily; }
            set { fontFamily = value; }
        }

        /// <summary>
        /// Gets or sets the FontStretch to compute the column width.
        /// </summary>
        /// <value>
        /// The FontStretch to compute the column width. 
        /// </value>
        /// <remarks>
        /// By default the column width calculation based on fixed FontStretch.
        /// When the FontStretch is customized, that is need to be initialized to this property for customizing column sizer calculation based on new FontStretch.
        /// </remarks>
        public FontStretch FontStretch
        {
            get { return fontStretch; }
            set { fontStretch = value; }
        }

        /// <summary>
        /// Gets or sets the FontWeight to compute the column width.
        /// </summary>
        /// <value>
        /// The FontWeight to compute the column width. 
        /// </value>
        /// <remarks>
        /// By default the column width calculation based on fixed FontWeight.
        /// When the FontWeight is customized, that is need to be initialized to this property for customizing column sizer calculation based on new FontWeight.
        /// </remarks>
        public FontWeight FontWeight
        {
            get { return fontWeight; }
            set { fontWeight = value; }
        }

        /// <summary>
        /// Gets or sets the margin to compute the column width.
        /// </summary>
        /// <value>
        /// The margin to compute the column width. 
        /// </value>
        /// <remarks>
        /// By default the column width calculation based on fixed margin.
        /// When the margin is customized, that is need to be initialized to this property for customizing column sizer calculation based on new margin.
        /// </remarks>
        public Thickness Margin
        {
            get { return margin; }
            set { margin = value; }
        }
#if WPF
        /// <summary>
        /// Gets or sets a value that indicates whether the column width is measured based on formatted text.
        /// </summary>
        /// <value>
        /// <b>true</b> if the column width is calculated based on the formatted text; otherwise, <b>false</b>.
        /// </value>
        public bool AllowMeasureTextByFormattedText
        {
            get { return allowMeasureByFormattedText; }
            set { allowMeasureByFormattedText = value; }
        }
#endif


#if UWP
        internal Popup popup;
        internal Popup GetPopUp()
        {
            //Adding as Popup child and hiding the Popup outside of UWP app page from user
            if (popup == null)
                popup = new Popup()
                {
                    HorizontalOffset = -1000,
                    VerticalOffset = -1000,
                    IsOpen = true
                };

            return popup;
        }
#endif


        #region Public methods
        /// <summary>
        /// Resets the auto width for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to reset the auto width.
        /// </param>
        /// <remarks>
        /// The column width is reset to <b>double.Nan</b> ,if the column sizing is need to recalculate based on the data.
        /// </remarks>
        /// <exception cref="System.InvalidOperationException">Thrown when the column is null.</exception>
        public void ResetAutoCalculation(GridColumnBase column)
        {
            if (column == null)
                throw new InvalidOperationException("Column Should not be null to perform this operation");
            column.AutoWidth = double.NaN;
        }

        /// <summary>
        /// Sets the width for the specified column based on <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/> column sizer.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set Auto width.
        /// </param>
        /// <param name="width">
        /// The corresponding width set as Auto width.
        /// </param>
        /// <exception cref="System.InvalidOperationException">Thrown when the column is null.</exception>
        internal void SetAutoWidth(GridColumnBase column, double width)
        {
            if (column == null)
                throw new InvalidOperationException("Column Should not be null to perform this operation");
            column.AutoWidth = width;
        }

        /// <summary>
        /// Resets Auto width calculation for all the columns.
        /// </summary> 
        /// <remarks>
        /// The column width is reset to <b>double.Nan</b> for all columns ,if the column sizing is need to recalculate based on the data.
        /// </remarks>
        internal virtual void ResetAutoCalculations()
        {

        }

        /// <summary>
        /// Resets Auto width calculation for all the columns.
        /// </summary> 
        /// <remarks>
        /// The column width is reset to <b>double.Nan</b> for all columns ,if the column sizing is need to recalculate based on the data.
        /// </remarks>
        public void ResetAutoCalculationforAllColumns()
        {
            ResetAutoCalculations();
        }


        #endregion

        /// <summary>
        /// Measures the size of the template when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>        
        /// <param name="rect">
        /// The corresponding display rectangle to measure the template.
        /// </param>
        /// <param name="record">
        /// The corresponding record to measure the template.
        /// </param>
        /// <param name="column">
        /// The corresponding column to measure the template.
        /// </param>        
        /// <param name="bounds">
        /// Indicates whether the template is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of template.
        /// </returns>       
#if WPF
        internal Size MeasureTemplate(Size rect, object record, GridColumnBase column, GridQueryBounds bounds, ContentControl ctrl)
#else
        internal Size MeasureTemplate(Size rect, object record, GridColumnBase column, GridQueryBounds bounds, ContentPresenter ctrl)
#endif
        {
#if UWP
            var _popUp = GetPopUp();
            _popUp.Child = ctrl;
            ctrl.FontSize = column != null && column.hasFontSize ? GetFontSize(column) : FontSize;
            ctrl.ClearValue(ContentPresenter.MarginProperty);
#endif
            ctrl.Measure(new Size());
            if (column.GetType() == typeof(GridUnBoundColumn))
            {
                var contentValue = (GridBase as SfDataGrid).GetUnBoundCellValue(column as GridColumn, record);
                var dataContextHelper = GetDataContextHelper(column, record);
                dataContextHelper.Value = contentValue;
                ctrl.Content = dataContextHelper;
            }
            else
            {
                if (column.SetCellBoundValue)
                {
                    var dataContextHelper = GetDataContextHelper(column, record);
                    dataContextHelper.SetValueBinding(column.ValueBinding, record);
                    ctrl.Content = dataContextHelper;
                }
                else
                {
                    ctrl.DataContext = record;
                    ctrl.Content = record;
                }
            }

            if (column.hasCellTemplate)
                ctrl.ContentTemplate = column.CellTemplate;
            else if (column.hasCellTemplateSelector)
                ctrl.ContentTemplateSelector = column.CellTemplateSelector;
            else
            {
                if (column.IsTemplate && GridBase.hasCellTemplateSelector)
                    ctrl.ContentTemplateSelector = GridBase.CellTemplateSelector;
            }

            var controlsize = Size.Empty;
            if (bounds == GridQueryBounds.Height)
            {
                var tectangleWidth = column.IsHidden || column.Width == 0.0 ? GetDefaultLineSize() : rect.Width;
                ctrl.Measure(new Size(tectangleWidth, Double.PositiveInfinity));
                controlsize = ctrl.DesiredSize.Height == 0 ? new Size(ctrl.DesiredSize.Width, this.GridBase.RowHeight) : ctrl.DesiredSize;
            }
            else
            {
                ctrl.Measure(new Size(Double.PositiveInfinity, rect.Height));
                controlsize = ctrl.DesiredSize.Width == 0 ? new Size(150, ctrl.DesiredSize.Height) : ctrl.DesiredSize;
            }
            ctrl.Content = null;
            ctrl.ContentTemplate = null;
            ctrl.ContentTemplateSelector = null;
#if UWP
            _popUp.Child = null;
#endif
            return controlsize;
        }

        
        internal double CheckWidthConstraints(GridColumnBase column, double Width, double columnWidth)
        {
            if (!double.IsNaN(column.MinimumWidth) || !double.IsNaN(column.MaximumWidth))
            {
                if (!double.IsNaN(column.MaximumWidth))
                {
                    if (!double.IsNaN(Width) && column.MaximumWidth > Width)
                    {
                        columnWidth = Width;
                    }
                    else
                    {
                        columnWidth = column.MaximumWidth;
                    }
                }

                if (!double.IsNaN(column.MinimumWidth))
                {
                    if (!double.IsNaN(Width) && column.MinimumWidth < Width)
                    {
                        if (Width > column.MaximumWidth)
                            columnWidth = column.MaximumWidth;
                        else
                            columnWidth = Width;
                    }
                    else
                    {
                        columnWidth = column.MinimumWidth;
                    }
                }
            }
            else
            {
                if (!double.IsNaN(Width))
                    columnWidth = Width;
            }
            return columnWidth;
        }
        internal virtual DataContextHelper GetDataContextHelper(GridColumnBase column, object record)
        {
            return null;
        }


        /// <summary>
        /// Measures the size of the header template for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to measure the header template.
        /// </param>
        /// <param name="rect">
        /// The corresponding display rectangle to measure the template.
        /// </param>               
        /// <param name="bounds">
        /// Indicates whether the template is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the size of the header template for the specified column.
        /// </returns>
#if WPF
        internal Size MeasureHeaderTemplate(GridColumnBase column, Size rect, GridQueryBounds bounds, ContentControl ctrl)
#else
        internal Size MeasureHeaderTemplate(GridColumnBase column, Size rect, GridQueryBounds bounds, ContentPresenter ctrl)
#endif
        {
#if UWP
            var _popUp = GetPopUp();
            _popUp.Child = ctrl;
            if (bounds == GridQueryBounds.Height)
                ctrl.Margin = new Thickness(5, 3, 5, 3);
            else
                ctrl.Margin = new Thickness(10, 3, 10, 3);
#endif
            ctrl.Measure(new Size());
            ctrl.Content = column.HeaderText;
            ctrl.ContentTemplate = column.HeaderTemplate;
            var controlSize = Size.Empty;
            if (bounds == GridQueryBounds.Height)
            {
                double iconsWidth = 0;
                if (this.GridBase.SortColumnDescriptions.Any(item => item.ColumnName == column.MappingName))
                    iconsWidth += SortIconWidth;
                iconsWidth += column.GetFilterIconWidth();
                ctrl.Measure(new Size(rect.Width - iconsWidth, Double.PositiveInfinity));
                controlSize = ctrl.DesiredSize;
                ctrl.ContentTemplate = null;
#if UWP
                _popUp.Child = null;
#endif
                if (controlSize.Height == 0)
                    return new Size(controlSize.Width, this.GridBase.HeaderRowHeight);

                //When we calculate the height of the HeaderTemplate we need to add the  margin and padding
                return new Size(controlSize.Width, controlSize.Height + margin.Top + margin.Bottom);
            }
            else
            {
                ctrl.Measure(new Size(Double.PositiveInfinity, rect.Height));
                controlSize = ctrl.DesiredSize;
                ctrl.ContentTemplate = null;
#if UWP
                _popUp.Child = null;
#endif
                if (controlSize.Width == 0)
                    return new Size(150, controlSize.Height);
            }

            return controlSize;
        }


        internal virtual double GetDefaultLineSize()
        {
            return 0;
        }
        /// <summary>
        /// Temporarily suspends the updates for column sizing operation when the column property value changes.
        /// </summary>
        protected internal void Suspend()
        {
            isInSuspend = true;
        }

        /// <summary>
        /// Resumes usual column sizing operation in SfDataGrid.
        /// </summary>
        protected internal void Resume()
        {
            isInSuspend = false;
        }

#if WPF

        internal Size MeasureTextByFormattedText(Size rectangle, GridColumnBase column, object record, GridQueryBounds queryBound, FormattedText formattedtext)
        {
            formattedtext.Trimming = TextTrimming.None;
            if (queryBound == GridQueryBounds.Height)
            {
                formattedtext.MaxTextHeight = double.MaxValue;

                //if column is null, then stackedcolumn text is measured.
                if (column == null)
                {
                    formattedtext.MaxTextWidth = rectangle.Width;
                    //Here six is added to the formattedtext height, 
                    //because while calculating auto-height for headers with textblock ( AllowMeasureTextByFormattedText as False), we have measured the textblock by top and bottom margin as 3.                        
                    return new Size(formattedtext.Width, formattedtext.Height + 6);
                }
                else
                    formattedtext.MaxTextWidth = column.IsHidden || column.Width == 0.0 ? GetDefaultLineSize() : rectangle.Width;

                if (column.hasMargin)
                {
                    if (formattedtext.MaxTextWidth > (GetMargin(column).Left + GetMargin(column).Right))
                        formattedtext.MaxTextWidth -= (GetMargin(column).Left + GetMargin(column).Right);
                }

                else
                {
                    if (formattedtext.MaxTextWidth > (Margin.Left + Margin.Right))
                        formattedtext.MaxTextWidth -= (Margin.Left + Margin.Right);
                }

                //Here six is added to the formattedtext height, 
                //because while calculating auto-height for headers with textblock ( AllowMeasureTextByFormattedText as False), we have measured the textblock by top and bottom margin as 3.                    
                if (record == null)
                    return new Size(formattedtext.Width, formattedtext.Height + 6);

                return new Size(formattedtext.Width, formattedtext.Height);
            }
            else
            {
                //The MaxTextWidth value must be between '0' and '3579139.40666667'
                formattedtext.MaxTextWidth = 3579139;
                formattedtext.MaxTextHeight = double.MaxValue;
                if (column.hasMargin)
                    return new Size(formattedtext.Width + (GetMargin(column).Left + GetMargin(column).Right), formattedtext.Height);
                else
                    return new Size(formattedtext.Width + (Margin.Left + Margin.Right), formattedtext.Height);
            }
        }


        /// <summary>
        /// Gets the formatted text for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the formatted text.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the formatted text.
        /// </param>
        /// <param name="displaytext">
        /// The corresponding display text to get formatted text. 
        /// </param>
        /// <returns>
        /// Returns the formatted text for the specified column.
        /// </returns>
        internal FormattedText GetFormattedText(GridColumnBase column, object record, string displaytext)
        {
            //WPF-23198 While calculating the FormattedText, we have set the FontSize of GridColumnSizer as same as in view.
            //For Header we have FontSize as 14 so we need to calculate the formatted text by using 14.
            FormattedText formattedtext;
            if (record == null)
                formattedtext = new FormattedText(displaytext, System.Globalization.CultureInfo.CurrentCulture, GridBase.FlowDirection, new Typeface(FontFamily, new FontStyle(), FontWeights.Normal, FontStretch), 14, Brushes.Black);
            else
                formattedtext = new FormattedText(displaytext, System.Globalization.CultureInfo.CurrentCulture, GridBase.FlowDirection, new Typeface(column.hasFontFamily ? GetFontFamily(column) : FontFamily, new FontStyle(), column.hasFontWeight ? GetFontWeight(column) : FontWeight, column.hasFontStretch ? GetFontStretch(column) : FontStretch), column.hasFontSize ? GetFontSize(column) : FontSize, Brushes.Black);
            return formattedtext;
        }
#endif


        internal Size MeasureTextByTextBlock(Size rectangle, string displayText, GridColumnBase column, object record, GridQueryBounds queryBound, TextBlock textBlock)
        {
            textBlock.Text = displayText;
            var parentBorder = new Border { Child = textBlock };
            if (queryBound == GridQueryBounds.Height)
            {
                if (column == null)
                    textBlock.MaxWidth = rectangle.Width;
                else
                    textBlock.MaxWidth = column.IsHidden || column.Width == 0.0 ? GetDefaultLineSize() : rectangle.Width;

                textBlock.MaxHeight = double.PositiveInfinity;
            }
            else
            {
                textBlock.MaxHeight = rectangle.Height;
                textBlock.MaxWidth = double.PositiveInfinity;
                //Calculating MaxWidth for ParentBorder was created an issue when the Text size is Greater than VisualContainer.TotalExtent value. Hence we removing this code.
                //var totalWidth = this.dataGrid.VisualContainer.ColumnWidths.TotalExtent;
                //parentBorder.MaxWidth = totalWidth; 
            }
            //WPF-19471 To get correct text area size , Must pass textblock maximum width and maximum height for measure text area
            parentBorder.Measure(new Size(textBlock.MaxWidth, textBlock.MaxHeight));
            parentBorder.Child = null;
            return parentBorder.DesiredSize;
        }


        /// <summary>
        /// Gets or sets the TextBlock to measure the text for column sizing calculation.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Controls.TextBlock"/> to measure the text for column sizing calculation. 
        /// </value>
        protected virtual TextBlock TextBlock { get; set; }


        /// <summary>
        /// Gets the TextBlock to measure the text when the column sizer is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.SizeToCells"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.Auto"/>.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the TextBlock.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the TextBlock.
        /// </param>
        /// <param name="queryBounds">
        /// Indicates whether the text is measured based on the height or width of the cell.
        /// </param>
        /// <returns>
        /// Returns the TextBlock for the specified column and record.
        /// </returns>
        internal TextBlock GetTextBlock(GridColumnBase column, object record, GridQueryBounds queryBounds)
        {
            if (TextBlock == null)
                TextBlock = new TextBlock();

            if (record == null)
            {
                TextBlock.FontFamily = FontFamily;
                TextBlock.FontStretch = FontStretch;

                //auto-row height calculation for headers and stacked header.
                if (queryBounds == GridQueryBounds.Height)
                {
                    TextBlock.Margin = new Thickness(5, 3, 5, 3);
                }
                else
                {
#if WPF
                    TextBlock.Margin = new Thickness(10, 3, 2, 3);
#else
                    TextBlock.Margin = new Thickness(10, 3, 10, 3);
#endif
                }

#if WPF
                //WPF-23198 While calculating the TextBlock size, we have to set the FontSize of TextBlock as same as in view.
                //For Header we have FontSize as 14 so we need to calculate the TextBlock size for header by using 14.
                TextBlock.FontSize = 14;
                TextBlock.FontWeight = FontWeights.Normal;
#else
                TextBlock.FontSize = 16;
                TextBlock.FontWeight = FontWeights.SemiBold;
#endif

                if (column != null)
                    TextBlock.HorizontalAlignment = column.HorizontalHeaderContentAlignment;
                cachedColumn = null;
            }
            else
            {
                if (cachedColumn != column)
                {
                    cachedColumn = column;
                    TextBlock.FontFamily = column != null && column.hasFontFamily ? GetFontFamily(column) : FontFamily;
                    TextBlock.Margin = GetMargin(column, queryBounds);
                    TextBlock.FontSize = column != null && column.hasFontSize ? GetFontSize(column) : FontSize;
                    TextBlock.FontWeight = column != null && column.hasFontWeight ? GetFontWeight(column) : FontWeight;
                    TextBlock.FontStretch = column != null && column.hasFontStretch ? GetFontStretch(column) : FontStretch;
                }
            }
            TextBlock.TextWrapping = queryBounds == GridQueryBounds.Height ? TextWrapping.Wrap : TextWrapping.NoWrap;
            return TextBlock;
        }


        /// <summary>
        /// Returns the padding value for corresponding grid column.
        /// </summary>
        /// <param name="column">GridColumn</param>
        /// <returns>Thickness</returns>
        internal Thickness GetMargin(GridColumnBase column, GridQueryBounds queryBounds)
        {
            Thickness _margin = Margin;
            if (column != null && column.hasMargin)
                _margin = GetMargin(column);
            if (queryBounds == GridQueryBounds.Height)
#if WPF
                _margin.Left -= 0.5;
#else
                _margin.Left -= 6;
#endif
            return _margin;
        }
        #region Attached Property


        /// <summary>
        /// Sets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the font size to its text content.
        /// </param>
        /// <param name="value">
        /// The desired font size. 
        /// </param>
        /// <remarks>
        /// The specified font size is considered for column sizing calculation only.
        /// </remarks>
        public static void SetFontSize(GridColumnBase column, double value)
        {
            column.SetValue(FontSizeProperty, value);
            column.hasFontSize = true;
        }

        /// <summary>
        /// Gets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the font size.
        /// </param>       
        /// <remarks>
        /// The specified font size is considered for column sizing calculation only.
        /// </remarks>
        public static double GetFontSize(GridColumnBase column)
        {
            return (double)column.GetValue(FontSizeProperty);
        }


#if UWP
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize dependency property.
        /// </remarks>
        public static readonly DependencyProperty FontSizeProperty =
           DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(ColumnSizerBase<T>), new PropertyMetadata(16.00, OnFontSizeChanged));

#else
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontSize dependency property.
        /// </remarks>
        public static readonly DependencyProperty FontSizeProperty =
            DependencyProperty.RegisterAttached("FontSize", typeof(double), typeof(ColumnSizerBase<T>), new PropertyMetadata(12.00, OnFontSizeChanged));
#endif


        /// <summary>
        /// Sets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontWeight"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the font weight.
        /// </param>
        /// <param name="value">
        /// The desired font weight. 
        /// </param>
        /// <remarks>
        /// The specified font weight is considered for column sizing calculation only.
        /// </remarks>
        public static void SetFontWeight(GridColumnBase column, FontWeight value)
        {
            column.SetValue(FontWeightProperty, value);
            column.hasFontWeight = true;
        }

        /// <summary>
        /// Gets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontWeight"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the font weight.
        /// </param>       
        /// <remarks>
        /// The specified font weight is considered for column sizing calculation only.
        /// </remarks>
        public static FontWeight GetFontWeight(GridColumnBase column)
        {
            return (FontWeight)column.GetValue(FontWeightProperty);
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontWeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontWeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty FontWeightProperty =
            DependencyProperty.RegisterAttached("FontWeight", typeof(FontWeight), typeof(ColumnSizerBase<T>), new PropertyMetadata(new FontWeight(), OnFontWeightChanged));

        /// <summary>
        /// Sets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontStretch"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the font stretch.
        /// </param>
        /// <param name="value">
        /// The desired font stretch value. 
        /// </param>
        /// <remarks>
        /// The specified font stretch is considered for column sizing calculation only.
        /// </remarks>
        public static void SetFontStretch(GridColumnBase column, FontStretch value)
        {
            column.SetValue(FontStretchProperty, value);
            column.hasFontStretch = true;
        }

        /// <summary>
        /// Gets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontStretch"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the font stretch.
        /// </param>       
        /// <remarks>
        /// The specified font stretch is considered for column sizing calculation only.
        /// </remarks>
        public static FontStretch GetFontStretch(GridColumnBase column)
        {
            return (FontStretch)column.GetValue(FontStretchProperty);
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontStretch dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontStretch dependency property.
        /// </remarks>
        public static readonly DependencyProperty FontStretchProperty =
            DependencyProperty.RegisterAttached("FontStretch", typeof(FontStretch), typeof(ColumnSizerBase<T>), new PropertyMetadata(new FontStretch(), OnFontStretchChanged));

        /// <summary>
        /// Sets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontFamily"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the font family.
        /// </param>
        /// <param name="value">
        /// The desired font family to set. 
        /// </param>
        /// <remarks>
        /// The specified font family is considered for column sizing calculation only.
        /// </remarks>
        public static void SetFontFamily(GridColumnBase column, FontFamily value)
        {
            column.SetValue(FontFamilyProperty, value);
            column.hasFontFamily = true;
        }

        /// <summary>
        /// Gets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontFamily"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the font family.
        /// </param>       
        /// <remarks>
        /// The specified font family is considered for column sizing calculation only.
        /// </remarks>
        public static FontFamily GetFontFamily(GridColumnBase column)
        {
            return (FontFamily)column.GetValue(FontFamilyProperty);
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontFamily dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.FontFamily dependency property.
        /// </remarks>
        public static readonly DependencyProperty FontFamilyProperty =
              DependencyProperty.RegisterAttached("FontFamily", typeof(FontFamily), typeof(ColumnSizerBase<T>), new PropertyMetadata(new FontFamily("Segoe UI"), OnFontFamilyChanged));

        /// <summary>
        /// Sets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to set the margin.
        /// </param>
        /// <param name="value">
        /// The desired margin to set. 
        /// </param>
        /// <remarks>
        /// The specified margin is considered for column sizing calculation only.
        /// </remarks>
        public static void SetMargin(GridColumnBase column, Thickness value)
        {
            column.SetValue(MarginProperty, value);
            column.hasMargin = true;
        }

        /// <summary>
        /// Gets the value of <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin"/> property for the specified column.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the margin.
        /// </param>       
        /// <remarks>
        /// The specified margin is considered for column sizing calculation only.
        /// </remarks>
        public static Thickness GetMargin(GridColumnBase column)
        {
            return (Thickness)column.GetValue(MarginProperty);
        }

#if UWP
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin dependency property.
        /// </remarks>
        public static readonly DependencyProperty MarginProperty =
              DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(ColumnSizerBase<T>), new PropertyMetadata(new Thickness(8, 4, 8, 4), OnMarginChanged));
#else
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridColumnSizer.Margin dependency property.
        /// </remarks>
        public static readonly DependencyProperty MarginProperty =
              DependencyProperty.RegisterAttached("Margin", typeof(Thickness), typeof(ColumnSizerBase<T>), new PropertyMetadata(new Thickness(5, 1, 5, 1), OnMarginChanged));
#endif

        #endregion

        #region DependencyCallBack

        /// <summary>
        /// Dependency call back for FontSize property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnFontSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumnBase);
            if (column != null)
                column.OnColumnPropertyChanged("FontSize");
        }


        /// <summary>
        /// Dependency call back for FontWeight property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnFontWeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumnBase);
            if (column != null)
                column.OnColumnPropertyChanged("FontWeight");
        }



        /// <summary>
        /// Dependency call back for FontWeight property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnFontStretchChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumnBase);
            if (column != null)
                column.OnColumnPropertyChanged("FontStretch");
        }


        /// <summary>
        /// Dependency call back for FontFamily property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnFontFamilyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumnBase);
            if (column != null)
                column.OnColumnPropertyChanged("FontFamily");
        }

        /// <summary>
        /// Dependency call back for Margin property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = (d as GridColumnBase);
            if (column != null)
                column.OnColumnPropertyChanged("Margin");
        }


        #endregion

#if WPF
        /// <summary>
        /// Gets or sets the control to measure the template for column sizing calculation.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Controls.ContentControl"/> to measure the template for column sizing calculation. 
        /// </value>
        protected virtual ContentControl Control { get; set; }

        /// <summary>
        /// Gets the content control to measure the template when column sizer is SizeToCells.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the content control.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the content control.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Controls.ContentControl"/> for the specified column and record.
        /// </returns>
        internal ContentControl GetControl(GridColumnBase column, object record)
        {
            if (Control == null)
                Control = new ContentControl();
            return Control;
        }
#else
        protected virtual ContentPresenter Control { get; set; }
        /// <summary>
        /// Gets the content presenter to measure the template when column sizer is SizeToCells.
        /// </summary>
        /// <param name="column">
        /// The corresponding column to get the content presenter.
        /// </param>
        /// <param name="record">
        /// The corresponding record to get the content presenter.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Controls.ContentPresenter"/> for the specified column and record.
        /// </returns>
        internal ContentPresenter GetControl(GridColumnBase column, object record)
        {
            if (Control == null)
                Control = new ContentPresenter();
            return Control;
        }

#endif
        protected virtual void Dispose(bool disposing)
        {
            if (isdisposed) return;
            GridBase = null;
            cachedColumn = null;
            Control = null;
            TextBlock = null;
            isdisposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
