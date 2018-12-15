#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using System;
using System.Windows.Data;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Input;
#endif
using System.Linq;

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Windows.System;
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif

    [ClassReference(IsReviewed = false)]
#if WPF
    public class GridCellHyperlinkRenderer : GridVirtualizingCellRenderer<TextBlock, TextBlock>
#else
    public class GridCellHyperlinkRenderer : GridVirtualizingCellRenderer<TextBlock, HyperlinkButton>
#endif
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellHyperlinkRenderer"/> class.
        /// </summary>
        public GridCellHyperlinkRenderer()
        {
            IsFocusible = false;
            IsEditable = false;
            SupportsRenderOptimization = false;
        }
        #endregion

        #region Override Methods

#if WPF
        protected override TextBlock OnCreateEditUIElement() 	
#else
        protected override HyperlinkButton OnCreateEditUIElement() 	
#endif
        {
            var uiElement =  base.OnCreateEditUIElement();
#if WPF
            uiElement.FocusVisualStyle = null;
#endif
            return uiElement;
        }

#if WPF
        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads  Hyperlink as usual in UseLightweightTemplate true case also.
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
#if WPF
        public override void OnInitializeEditElement(DataColumnBase dataColumn, TextBlock uiElement, object dataContext)
#else 
        public override void OnInitializeEditElement(DataColumnBase dataColumn, HyperlinkButton uiElement, object dataContext)
#endif
        {
            var column = dataColumn.GridColumn;            
            uiElement.SetValue(FrameworkElement.MarginProperty, column.Padding);
            uiElement.SetValue(Control.PaddingProperty, column.Padding);
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.VerticalAlignment);
            var hyperLinkColumn = (GridHyperlinkColumn)column;
#if WPF
            // WPF-20330 - Reusing the content instead of create new hyperlink each time, to keep its stuffs like event, etc .
            Hyperlink content = null;
            var hyperlink = uiElement.Inlines.Cast<Hyperlink>().FirstOrDefault();
            content = hyperlink ?? new Hyperlink();
            content.FocusVisualStyle = null;
            uiElement.Inlines.Add(content);
            //WPF-24516 - To Navigate the hyperlink in the First Click by setting Foreground as null.
            uiElement.Foreground = null;

            var run=new Run();
            run.SetBinding(Run.TextProperty, column.DisplayBinding);
            //Set Value Binding to NavigateUriProperty in Hyperlink control.
            content.SetBinding(Hyperlink.NavigateUriProperty, column.ValueBinding);
            content.Tag = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            content.Inlines.Clear();
            content.Inlines.Add(run);
            
            if (hyperLinkColumn == null)
                return;
#else
            var textBlock = new TextBlock();
            uiElement.Content = textBlock;            
            //UWP - 6163 GridCell display value using DisplayBinding of GridHyperLinkColumn
            textBlock.SetBinding(TextBlock.TextProperty, column.DisplayBinding);
            if (hyperLinkColumn == null)
                return;
            textBlock.SetValue(TextBlock.TextWrappingProperty, hyperLinkColumn.TextWrapping);
#endif                    
            uiElement.SetValue(TextBlock.TextWrappingProperty, hyperLinkColumn.TextWrapping);                                 
            uiElement.SetValue(Control.HorizontalAlignmentProperty, hyperLinkColumn.HorizontalAlignment);   
        }

        /// <summary>
        /// Called when [wire edit UI element].
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
#if WPF
        protected override void OnWireEditUIElement(TextBlock uiElement)
#else
        protected override void OnWireEditUIElement(HyperlinkButton uiElement)
#endif
        {
#if WPF
            if (uiElement.Inlines.Count <= 0) return;
            //Content is TetxBlock, so we need to get the Hyperlink from Inlines of TextBlock
            var hyperlinkControl = uiElement.Inlines.Cast<Hyperlink>().FirstOrDefault();
#else
            var hyperlinkControl = uiElement as HyperlinkButton;
#endif
            if (hyperlinkControl != null) 
                hyperlinkControl.Click += OnHyperLinkClick;
        }

#if WPF     
        protected override void OnUnwireEditUIElement(TextBlock uiElement)
#else
        protected override void OnUnwireEditUIElement(HyperlinkButton uiElement)
#endif
        {
#if WPF
            if (uiElement.Inlines.Count <= 0) return;
            //Content is TetxBlock, so we need to get the Hyperlink from Inlines of TextBlock
            var hyperlinkControl = uiElement.Inlines.Cast<Hyperlink>().FirstOrDefault();
#else
            var hyperlinkControl = uiElement as HyperlinkButton;
#endif
            if (hyperlinkControl != null)
                hyperlinkControl.Click -= OnHyperLinkClick;

            base.OnUnwireEditUIElement(uiElement);
        }

        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    {
#if WPF
                        OnHyperLinkClick(((TextBlock)CurrentCellRendererElement), e);
#else
                        OnHyperLinkClick(CurrentCellRendererElement, e);
#endif
                        return false;
                    }
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion

        #region Event Handlers
#if WinRT || UNIVERSAL
        private async void OnHyperLinkClick(object sender, RoutedEventArgs e)
#else
        void OnHyperLinkClick(object sender, EventArgs e)
#endif
        {
#if WPF
            Hyperlink hyperlinkControl;
            //Content is TetxBlock, so we need to get the Hyperlink from Inlines of TextBlock
            if (sender is TextBlock)
                hyperlinkControl = (sender as TextBlock).Inlines.Cast<Hyperlink>().FirstOrDefault();
            else
                hyperlinkControl = (Hyperlink)sender;
#else
            var hyperlinkControl = (HyperlinkButton)sender;
#endif
            GridCell gridcell = null;
#if WinRT || UNIVERSAL
            if (hyperlinkControl.Parent is GridCell)
                gridcell = hyperlinkControl.Parent as GridCell;
#else
            if (hyperlinkControl.Parent is TextBlock)
            {
                var textBlock = hyperlinkControl.Parent as TextBlock;
                gridcell = textBlock.Parent as GridCell;
            }
#endif
            var navigateText = string.Empty;
            var rowColumnIndex = RowColumnIndex.Empty;
            rowColumnIndex.RowIndex = gridcell != null ? gridcell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
            rowColumnIndex.ColumnIndex = gridcell != null ? gridcell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;
            if (!rowColumnIndex.IsEmpty)
            {
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowColumnIndex);
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowColumnIndex);
            }
#if WPF
            if(hyperlinkControl.NavigateUri != null)
            {
                navigateText = hyperlinkControl.NavigateUri.ToString();
            }
            else
#endif
            {
                if (rowColumnIndex != null && !rowColumnIndex.IsEmpty)
                {
                    //Get the column from rowColumnIndex.
                    var column = this.DataGrid.Columns[this.DataGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex)];
                    column.ColumnWrapper.DataContext = hyperlinkControl.DataContext;
                    if (column.ColumnWrapper.Value != null)
                        navigateText = column.ColumnWrapper.Value.ToString();
                }
            }

            if (DataGrid.CurrentCellRequestNavigateEvent(new CurrentCellRequestNavigateEventArgs(DataGrid) { NavigateText = navigateText, RowData = hyperlinkControl.DataContext, RowColumnIndex = rowColumnIndex }))
                return;
            const string pattern = @"((http|https?|ftp|gopher|telnet|file|notes|ms-help):((//)|(\\\\))+[\w\d:#@%/;$()~_?\+-=\\\.&]*)";
            //the hyperlink value is stored in the variable navigateText. 
            if (navigateText != null)
            {
                var NavigateUri = Regex.IsMatch(navigateText.ToString(), pattern)
                                  ? new Uri(navigateText.ToString())
                                  : null;
                if (NavigateUri == null)
                    return;

                hyperlinkControl.NavigateUri = NavigateUri;
#if WPF
                Process.Start(new ProcessStartInfo(hyperlinkControl.NavigateUri.AbsoluteUri));
#else
                //UWP-154 - Hyper link is not navigate while clicking space key, so here launch the URI. In WPF, we have launched above.
                if (e is KeyRoutedEventArgs && (e as KeyRoutedEventArgs).Key == Key.Space)
                    await Launcher.LaunchUriAsync(new Uri(hyperlinkControl.NavigateUri.AbsoluteUri));
#endif
            }
        }


        #endregion
    }
}
