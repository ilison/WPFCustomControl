#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using Windows.System;    
#endif
#if WPF
    public class TreeGridCellHyperlinkRenderer : TreeGridVirtualizingCellRenderer<TextBlock, TextBlock>
#else
    public class TreeGridCellHyperlinkRenderer : TreeGridVirtualizingCellRenderer<TextBlock, HyperlinkButton>
#endif
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="TreeGridCellHyperlinkRenderer"/> class.
        /// </summary>
        public TreeGridCellHyperlinkRenderer()
        {
            IsFocusable = false;
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
            var uiElement = base.OnCreateEditUIElement();
#if WPF
            uiElement.FocusVisualStyle = null;
#endif
            return uiElement;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
#if WPF
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
#else
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, HyperlinkButton uiElement, object dataContext)
#endif
        {
            var column = dataColumn.TreeGridColumn;
            uiElement.SetValue(FrameworkElement.MarginProperty, column.Padding);
            uiElement.SetValue(Control.PaddingProperty, column.Padding);
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.VerticalAlignment);
            var hyperLinkColumn = (TreeGridHyperlinkColumn)column;
#if WPF
            // WPF-20330 - Reusing the content instead of create new hyperlink each time, to keep its stuffs like event, etc .
            Hyperlink content = null;
            var hyperlink = uiElement.Inlines.Cast<Hyperlink>().FirstOrDefault();
            content = hyperlink ?? new Hyperlink();
            content.FocusVisualStyle = null;
            uiElement.Inlines.Add(content);
            //WPF-24516 - To Navigate the hyperlink in the First Click by setting Foreground as null.
            uiElement.Foreground = null;

            var run = new Run();
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
            textBlock.SetBinding(TextBlock.TextProperty, column.ValueBinding);
            if (hyperLinkColumn == null)
                return;
            textBlock.SetValue(TextBlock.TextWrappingProperty, hyperLinkColumn.TextWrapping);
#endif
            uiElement.SetValue(TextBlock.TextWrappingProperty, hyperLinkColumn.TextWrapping);
            uiElement.SetValue(Control.HorizontalAlignmentProperty, hyperLinkColumn.HorizontalAlignment);   
        }

        protected override void InitializeCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            base.InitializeCellStyle(dataColumn, record);
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

#if UWP
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
            TreeGridCell gridcell = null;
#if UWP
            if (hyperlinkControl.Parent is TreeGridCell)
                gridcell = hyperlinkControl.Parent as TreeGridCell;
#else
            if (hyperlinkControl.Parent is TextBlock)
            {
                var textBlock = hyperlinkControl.Parent as TextBlock;
                gridcell = textBlock.Parent as TreeGridCell;
            }
#endif
            var navigateText = string.Empty;
            var rowColumnIndex = RowColumnIndex.Empty;
            rowColumnIndex.RowIndex = gridcell != null ? gridcell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
            rowColumnIndex.ColumnIndex = gridcell != null ? gridcell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;
            if (!rowColumnIndex.IsEmpty)
            {
                this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowColumnIndex);
                this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowColumnIndex);
            }
            if (hyperlinkControl.NavigateUri != null)
            {
                navigateText = hyperlinkControl.NavigateUri.ToString();
            }
            else
            {
                if (rowColumnIndex != null && !rowColumnIndex.IsEmpty)
                {
                    //Get the column from rowColumnIndex.
                    var column = this.TreeGrid.Columns[this.TreeGrid.ResolveToGridVisibleColumnIndex(rowColumnIndex.ColumnIndex)];
                    column.ColumnWrapper.DataContext = hyperlinkControl.DataContext;
                    if (column.ColumnWrapper.Value != null)
                        navigateText = column.ColumnWrapper.Value.ToString();
                }
            }

            if (TreeGrid.CurrentCellRequestNavigateEvent(new CurrentCellRequestNavigateEventArgs(TreeGrid) { NavigateText = navigateText, RowData = hyperlinkControl.DataContext, RowColumnIndex = rowColumnIndex }))
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
                if (e is KeyRoutedEventArgs && (e as KeyRoutedEventArgs).Key == Key.Space)
                    await Launcher.LaunchUriAsync(new Uri(hyperlinkControl.NavigateUri.AbsoluteUri));
#endif
            }
        }
#endregion
    }
}
