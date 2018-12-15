#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Syncfusion.Windows.Controls;
using Syncfusion.Windows.Shared;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that provides set of options while printing the SfDataGrid.
    /// </summary>
    public class PrintOptionsControl : Control, IDisposable
    {

        #region Fields

        const double cmConst = 37.79527559055;
        private PrintManagerBase printManager;
        private PrintPreviewAreaControl printDataContext;
        private bool isWired;
        private bool isdisposed = false;
        //To maintain the standard page size for the individual pape kind.
        private Dictionary<string, Size> pagesizes = new Dictionary<string, Size>();
        //To maintain the the standard page size for the appropriate page type.
        private Dictionary<string, Thickness> pageThickness = new Dictionary<string, Thickness>();

        #endregion

        #region Ctor
        //To set Default value of Custom Margin.
        private Thickness DefaultPageMargin = new Thickness(2.5, 2.5, 2.5, 2.5);
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintOptionsControl"/> class.
        /// </summary>
        public PrintOptionsControl()
        {
            DefaultStyleKey = typeof(PrintOptionsControl);
            Loaded += OnPrintOptionsControlLoaded;
            DataContextChanged += OnPrintOptionsControlDataContextChanged;
            pagesizes.Add("Letter", new Size(21.59, 27.94));
            pagesizes.Add("Legal", new Size(21.59, 35.56));
            pagesizes.Add("Executive", new Size(18.41, 26.67));
            pagesizes.Add("A4", new Size(21, 29.7));
            pagesizes.Add("Envelope #10", new Size(10.48, 24.13));
            pagesizes.Add("Envelope DL", new Size(11, 22));
            pagesizes.Add("Envelope C5", new Size(16.2, 22.9));
            pagesizes.Add("Envelope B5", new Size(17.6, 25));
            pagesizes.Add("Envelope Monarch", new Size(9.84, 19.05));
            pageThickness.Add("Normal", DefaultPageMargin);
            pageThickness.Add("Narrow", new Thickness(1.27, 1.27, 1.27, 1.27));
            pageThickness.Add("Moderate", new Thickness(1.91, 2.54, 1.91, 2.54));
            pageThickness.Add("Wide", new Thickness(5.08, 2.54, 5.08, 2.54));
        }

        #endregion

        #region UIElements

        private ComboBox PapersCmbBox;
        private ComboBox MarginCmbBox;
        private ComboBox OrientationCmbBox;
        private Button PageSizeOkButton;
        private Button MarginOkButton;
        private UpDown PageWidthUpDown;
        private UpDown PageHeightUpDown;
        private UpDown LeftUpDown;
        private UpDown RightUpDown;
        private UpDown TopUpDown;
        private UpDown BottomUpDown;

        #endregion

        #region Override
        /// <summary>
        /// Builds the visual tree for the print options control when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PapersCmbBox = GetTemplateChild("PART_PapersComboBox") as ComboBox;
            MarginCmbBox = GetTemplateChild("PART_MarginComboBox") as ComboBox;
            OrientationCmbBox = GetTemplateChild("PART_OrientationComboBox") as ComboBox;
            PageSizeOkButton = GetTemplateChild("PART_PageSizeOkButton") as Button;
            PageWidthUpDown = GetTemplateChild("PART_PageWidthUpDown") as UpDown;
            PageHeightUpDown = GetTemplateChild("PART_PageHeightUpDown") as UpDown;
            MarginOkButton = GetTemplateChild("PART_MarginOkButton") as Button;
            LeftUpDown = GetTemplateChild("PART_LeftUpDown") as UpDown;
            RightUpDown = GetTemplateChild("PART_RightUpDown") as UpDown;
            TopUpDown = GetTemplateChild("PART_TopUpDown") as UpDown;
            BottomUpDown = GetTemplateChild("PART_BottomUpDown") as UpDown;
            WireEvents();
        }

        #endregion

        #region Wire/UnWire Events

        private void WireEvents()
        {
            if (PapersCmbBox != null)
                PapersCmbBox.SelectionChanged += OnPartComboPapersSelectionChanged;

            if (MarginCmbBox != null)
                MarginCmbBox.SelectionChanged += OnMarginCmbBoxSelectionChanged;

            if(OrientationCmbBox != null)
                OrientationCmbBox.SelectionChanged += OrientationCmbBox_SelectionChanged;

            if (PageSizeOkButton != null)
                PageSizeOkButton.Click += OnPageSizeOkButtonClick;

            if (MarginOkButton != null)
                MarginOkButton.Click += OnMarginOkButtonClick;

            Unloaded += OnPrintOptionsControlUnloaded;

            isWired = true;
        }

        private void OrientationCmbBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.UpdatePage(false);
        }

        private void UnWireEvents()
        {

            if (PapersCmbBox != null)
                PapersCmbBox.SelectionChanged -= OnPartComboPapersSelectionChanged;

            if (MarginCmbBox != null)
                MarginCmbBox.SelectionChanged -= OnMarginCmbBoxSelectionChanged;

            if (OrientationCmbBox != null)
                OrientationCmbBox.SelectionChanged -= OrientationCmbBox_SelectionChanged;

            if (PageSizeOkButton != null)
                PageSizeOkButton.Click -= OnPageSizeOkButtonClick;

            if (MarginOkButton != null)
                MarginOkButton.Click -= OnMarginOkButtonClick;

            isWired = false;
        }

        #endregion

        #region Events

        void OnPrintOptionsControlLoaded(object sender, RoutedEventArgs e)
        {
            if (!isWired)
                WireEvents();
        }

        void OnPrintOptionsControlUnloaded(object sender, RoutedEventArgs e)
        {
            if (isWired)
                UnWireEvents();
            Unloaded -= OnPrintOptionsControlUnloaded;
        }

        void OnPrintOptionsControlDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(!(e.NewValue is PrintPreviewAreaControl))
                return;
            printDataContext = e.NewValue as PrintPreviewAreaControl;
            printManager = printDataContext.PrintManagerBase;

        }

        private void OnPartComboPapersSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!(e.AddedItems[0] is ComboBoxItem))
                return;

            double width = 0;
            double height = 0;
            // WPF-34945 Assign the standard Width ad height for the page using dictionary,while the combo box selection is changed.
            if (!((e.AddedItems[0] as ComboBoxItem).Tag.ToString() == "Custom Size"))
            {
                Size page = pagesizes[(e.AddedItems[0] as ComboBoxItem).Tag.ToString()];
                width = page.Width;
                height = page.Height;
            }
            else
            {
                OnPageSizeOkButtonClick(null, null);
                if (PageHeightUpDown != null) height = (double)PageHeightUpDown.Value;
                if (PageWidthUpDown != null) width = (double)PageWidthUpDown.Value;
            }
            if (PageHeightUpDown != null) PageHeightUpDown.Value = height;
            if (PageWidthUpDown != null) PageWidthUpDown.Value = width;
            height *= cmConst;
            width *= cmConst;
            this.UpdatePage(false);
            //WPF-33625 Need to update the Zoom delta value while the page size is changed from the print preview window.
            printDataContext.PartPrintWindowPanel.Child.zoomHeightDelta = (double)(PageHeightUpDown.Value * cmConst) / 100;
            printDataContext.PartPrintWindowPanel.Child.zoomWidthDelta = (double)(PageWidthUpDown.Value * cmConst) / 100;
            printManager.isSuspended = true;
            printManager.PrintPageWidth = width;
            printManager.PrintPageHeight = height;
            printManager.isSuspended = false;
            printManager.InValidatePreviewPanel(false);
            printDataContext.OnZoomFactorChanged(printDataContext.ZoomFactor);
        }

        void OnPageSizeOkButtonClick(object sender, RoutedEventArgs e)
        {
            var height = (double)PageHeightUpDown.Value * cmConst;
            var width = (double)PageWidthUpDown.Value * cmConst;
            if (height < (printDataContext.PrintPageMargin.Top + printDataContext.PrintPageMargin.Bottom) || width < (printDataContext.PrintPageMargin.Left + printDataContext.PrintPageMargin.Right))
                return;

            printManager.isSuspended = true;
            printManager.PrintPageHeight = height;
            printManager.PrintPageWidth = width;
            OrientationCmbBox.SelectedIndex = height > width ? 0 : 1;

            printManager.isSuspended = false;
            printManager.InValidatePreviewPanel(false);
            //WPF-34945, Need to checks that the manually provided page sizes matches the Predefined paperkind kind sizes,
            //before setting the selection index for the combo box.
            string myKey = pagesizes.FirstOrDefault(x => x.Value == new Size((double)PageWidthUpDown.Value, (double)PageHeightUpDown.Value)).Key;
            if (myKey == null)
            {
                PapersCmbBox.SelectedIndex = PapersCmbBox.Items.Count - 1;
            }
            else
                PapersCmbBox.SelectedIndex = pagesizes.Keys.ToList().IndexOf(myKey);
            //WPF-33625 Need to update the Zoom delta value while the page size is changed from the print preview window.
            printDataContext.PartPrintWindowPanel.Child.zoomHeightDelta = (double)(PageHeightUpDown.Value * cmConst) / 100;
            printDataContext.PartPrintWindowPanel.Child.zoomWidthDelta = (double)(PageWidthUpDown.Value * cmConst) / 100;
            printDataContext.OnZoomFactorChanged(printDataContext.ZoomFactor);
        }

        void OnMarginCmbBoxSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!(e.AddedItems[0] is ComboBoxItem))
                return;

            double left = 0;
            double right = 0;
            double top = 0;
            double bottom = 0;
            //WPF-34945 Assign the standard margin size for the page, while the combo box selection changed.
            if (!((e.AddedItems[0] as ComboBoxItem).Tag.ToString() == "Custom Margin"))
            {
                Thickness pageThick = pageThickness[(e.AddedItems[0] as ComboBoxItem).Tag.ToString()];
                left = pageThick.Left;
                right = pageThick.Right;
                top = pageThick.Top;
                bottom = pageThick.Bottom;
                if (LeftUpDown != null) LeftUpDown.Value = left;
                if (RightUpDown != null) RightUpDown.Value = right;
                if (TopUpDown != null) TopUpDown.Value = top;
                if (BottomUpDown != null) BottomUpDown.Value = bottom;
            }
            else
            {
                if (LeftUpDown != null) left = (double)LeftUpDown.Value;
                if (RightUpDown != null) right = (double)RightUpDown.Value;
                if (TopUpDown != null) top = (double)TopUpDown.Value;
                if (BottomUpDown != null) bottom = (double)BottomUpDown.Value;
            }
            if (!this.UpdatePage(false))
            {
                var margin = new Thickness(left * cmConst, top * cmConst, right * cmConst, bottom * cmConst);
                printDataContext.PrintPageMargin = margin;
                printDataContext.OnZoomFactorChanged(printDataContext.ZoomFactor);
            }
        }

        void OnMarginOkButtonClick(object sender, RoutedEventArgs e)
        {
            this.UpdatePage(true);
        }

        /// <summary>
        /// This Method to Restore the Previous value when the Margin Sizes is Exceed than the Pagesizes and To set the Default value When to change the PageOrientation Selection.
        /// </summary>
        /// <param name="canupdate">
        /// Decides whether to update printDataContext.PrintPageMargin. We don't need to update this when Orientation or PageSiz changed until its margin values not within range.
        /// </param>
        private bool UpdatePage(bool canupdate = false)
        {
            bool canReset = false;
            //Based on MSWord Calculation MinimumPageWidthSize 0.50,MinimumPageHeightSize 0.90,CustomDefaultValue 2.50 to set if Constraints is not matched the margin size.
            double MinimumPageWidthSize = 0.50;
            double MinimumPageHeightSize = 0.90;
            if (printDataContext.PrintOrientation == PrintOrientation.Portrait)
            {
                //while the page margin exceeds the limit, need to reset the margin size in to default value
                if ((double)LeftUpDown.Value + (double)RightUpDown.Value > ((double)PageWidthUpDown.Value - MinimumPageWidthSize))
                    canReset = true;
                if ((double)BottomUpDown.Value + (double)TopUpDown.Value > ((double)PageHeightUpDown.Value - MinimumPageHeightSize))
                    canReset = true;
            }
            if (printDataContext.PrintOrientation == PrintOrientation.Landscape)
            {
                if ((double)LeftUpDown.Value + (double)RightUpDown.Value > ((double)PageHeightUpDown.Value - MinimumPageWidthSize))
                    canReset = true;
                
                if ((double)BottomUpDown.Value + (double)TopUpDown.Value > ((double)PageWidthUpDown.Value - MinimumPageHeightSize))
                    canReset = true;
            }

            if(canReset)
            {
                TopUpDown.Value = DefaultPageMargin.Top;
                BottomUpDown.Value = DefaultPageMargin.Bottom;
                LeftUpDown.Value = DefaultPageMargin.Left;
                RightUpDown.Value = DefaultPageMargin.Right;
            }

            if (canReset || canupdate)
            {
                var left = (double)LeftUpDown.Value * cmConst;
                var right = (double)RightUpDown.Value * cmConst;
                var top = (double)TopUpDown.Value * cmConst;
                var bottom = (double)BottomUpDown.Value * cmConst;
                var margin = new Thickness(left, top, right, bottom);
                printDataContext.PrintPageMargin = margin;
                //WPF-34945 Need to checks that the manually provided margin sizes matches the predefined margin size.
                string myKey = pageThickness.FirstOrDefault(x => x.Value == new Thickness((double)LeftUpDown.Value, (double)TopUpDown.Value, (double)RightUpDown.Value, (double)BottomUpDown.Value)).Key;
                if (myKey == null)
                {
                    MarginCmbBox.SelectedIndex = MarginCmbBox.Items.Count - 1;
                }
                else
                    MarginCmbBox.SelectedIndex = pageThickness.Keys.ToList().IndexOf(myKey);
                printDataContext.OnZoomFactorChanged(printDataContext.ZoomFactor);
                return true;
            }
            return false;
        }

        #endregion

        #region Dispose
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintOptionsControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintOptionsControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (pagesizes != null || pageThickness != null)
            {
                pagesizes.Clear();
                pageThickness.Clear();
            }
            if (isDisposing)
            {
                printManager = null;
                printDataContext = null;
            }
            isdisposed = true;
        }

        #endregion
    }

}
