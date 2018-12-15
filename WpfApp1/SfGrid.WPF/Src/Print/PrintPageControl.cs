#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.ComponentModel;
#if WinRT
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that contains the <see cref="Syncfusion.UI.Xaml.Grid.PrintPagePanel"/> to display the SfDataGrid with in the page panel.
    /// </summary>
    public class PrintPageControl : ContentControl, IDisposable, INotifyPropertyChanged
    {

        #region Fields

        internal double zoomHeightDelta;
        internal double zoomWidthDelta;
        private PrintManagerBase printManagerBase;
        private bool isScaleSetBeforeControlLoaded;
        private bool isdisposed = false;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPageControl"/> class.
        /// </summary>
        /// <param name="printManagerBase">
        /// Specifies print related operations to initialize print page control.
        /// </param>
        public PrintPageControl(PrintManagerBase printManagerBase)
        {
            this.printManagerBase = printManagerBase;
            DefaultStyleKey = typeof(PrintPageControl);
            this.Loaded += OnLoaded;
            this.Unloaded += PrintPageControl_Unloaded;
        }

        void PrintPageControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= OnLoaded;
            this.Unloaded -= PrintPageControl_Unloaded;
        }

        #endregion

        #region Public Properties
        private int _pageIndex;
        /// <summary>
        /// Gets the index of the page.
        /// </summary>
        public int PageIndex
        {
            get { return _pageIndex; }
            internal set
            {
                _pageIndex = value;
                this.RaisePropertyChanged("PageIndex");
            }
        }

        /// <summary>
        /// Gets the total number of pages that the document contains. 
        /// </summary>
        public int TotalPages { get; internal set; }

        #endregion

        #region UIElements

        internal Viewbox PartViewbox;
        internal Border PartScalingBorder;

        #endregion

        #region Overrides
        /// <summary>
        /// Builds the visual tree for the print page control when a new template is applied.
        /// </summary>
#if WinRT || UNIVERSAL
        
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            PartViewbox = GetTemplateChild("PART_Viewbox") as Viewbox;
            PartScalingBorder = GetTemplateChild("PartScalingBorder") as Border;
            if (DataContext is PrintManagerBase)
            {
                var dataContext = (DataContext as PrintManagerBase);
                zoomHeightDelta = dataContext.PrintPageHeight/100;
                zoomWidthDelta = dataContext.PrintPageWidth/100;
                if (isScaleSetBeforeControlLoaded)
                    dataContext.ProcessPrintPageScale(this);
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (isScaleSetBeforeControlLoaded && DataContext is PrintManagerBase)
                (DataContext as PrintManagerBase).ProcessPrintPageScale(this);
        }

        #endregion

        #region Internal Methods

        internal void Zoom(double percent)
        {
            if (printManagerBase.PrintPageOrientation == PrintOrientation.Portrait)
            {
                PartViewbox.Height = percent * zoomHeightDelta;
                PartViewbox.Width = percent * zoomWidthDelta;
                //WPF-33625 Measure is called to recalculate the the page size based upon the Zoom value.
#if WPF
                PartViewbox.Measure(new Size((percent * zoomHeightDelta), (percent * zoomWidthDelta)));
#endif
            }
            else
            {
                PartViewbox.Height = percent * zoomWidthDelta;
                PartViewbox.Width = percent * zoomHeightDelta;
                //WPF-33625 Measure is called to recalculate the the page size based upon the Zoom value.
#if WPF
                PartViewbox.Measure(new Size((percent * zoomWidthDelta), (percent * zoomHeightDelta)));
#endif
            }
        }

        internal void Scale(double scaleX, double scaleY)
        {
            if (PartScalingBorder == null)
            {
                isScaleSetBeforeControlLoaded = true;
                return;
            }
            PartScalingBorder.RenderTransform = new ScaleTransform();
            (PartScalingBorder.RenderTransform as ScaleTransform).ScaleX = scaleX;
            (PartScalingBorder.RenderTransform as ScaleTransform).ScaleY = scaleY;
        }

        #endregion

        #region Dispose Member
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPageControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPageControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>                
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                printManagerBase = null;
                this.DataContext = null;
            }
            isdisposed = true;
        }

        #endregion

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Occurs when a property value changes in <see cref="Syncfusion.UI.Xaml.Grid.PrintPageControl"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
