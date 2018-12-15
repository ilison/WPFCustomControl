#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.ComponentModel;
using Syncfusion.UI.Xaml.Utility;
using System.Windows;
using System.Windows.Controls;


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that displays the print preview of SfDataGrid content that is ready for printing operation.
    /// </summary>
    public class PrintPreviewAreaControl : Control, INotifyPropertyChanged, IDisposable
    {

        #region Fields

        private PrintManagerBase printManager;
        private bool isPageIndexSetFromOverride;
        private bool isdisposed = false;
        
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl"/> class.
        /// </summary>
        public PrintPreviewAreaControl()
        {
            DefaultStyleKey = typeof(PrintPreviewAreaControl);
        }
       

        #endregion

        #region UIElements

        internal PrintPreviewPanel PartPrintWindowPanel;

        #endregion

        #region Properties

        private int totalPages;
        /// <summary>
        /// Gets the total number of pages that the document contains for printing. 
        /// </summary>
        public int TotalPages
        {
            get
            {
                return totalPages;
            }
            internal set
            {
                totalPages = value;
                OnPropertyChanged("TotalPages");
            }
        }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> which manages the printing operation in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>    
        public PrintManagerBase PrintManagerBase
        {
            get
            {
                if (printManager != null && !printManager.isPagesInitialized)
                    printManager.InitializePrint(true);
                return printManager;
            }
            set
            {
                printManager = value;
                OnPropertyChanged("PrintManagerBase");
            }
        }

        #endregion

        #region Dependency Properties

        #region PrintPageMargins Property

        /// <summary>
        /// Gets or sets the margin of page for printing.
        /// </summary>
        /// <value>
        /// The Thickness of page for printing.
        /// </value>        
        public Thickness PrintPageMargin
        {
            get { return (Thickness)GetValue(PrintPageMarginProperty); }
            set { SetValue(PrintPageMarginProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageMargin dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageMargin dependency property.
        /// </remarks>   
        public static readonly DependencyProperty PrintPageMarginProperty =
            DependencyProperty.Register("PrintPageMargin", typeof(Thickness), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(new Thickness(96), OnPrintMarginChanged));

        private static void OnPrintMarginChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageMargin = (Thickness) e.NewValue;
        }

        #endregion

        #region PrintPageHeight Property


        /// <summary>
        /// Gets or sets the height of a page for printing.
        /// </summary>
        /// <value>
        /// The height of a page for printing.
        /// </value>
        public double PrintPageHeight
        {
            get { return (double)GetValue(PrintPageHeightProperty); }
            set { SetValue(PrintPageHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeight dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeight dependency property.
        /// </remarks>   
        public static readonly DependencyProperty PrintPageHeightProperty =
            DependencyProperty.Register("PrintPageHeight", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(1122.52, OnPrintPageHeightChanged));

        private static void OnPrintPageHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageHeight = (double) e.NewValue;
        }

        #endregion

        #region PrintPageWidth Property

        /// <summary>
        /// Gets or sets the width of a page for printing.
        /// </summary>
        /// <value>
        /// The width of a page for printing.
        /// </value>
        public double PrintPageWidth
        {
            get { return (double)GetValue(PrintPageWidthProperty); }
            set { SetValue(PrintPageWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageWidth dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageWidth dependency property.
        /// </remarks>   
        public static readonly DependencyProperty PrintPageWidthProperty =
            DependencyProperty.Register("PrintPageWidth", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(793.70, OnPrintPageWidthChanged));

        private static void OnPrintPageWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageWidth = (double) e.NewValue;
        }


        #endregion

        #region PrintPageHeaderHeight Property

        /// <summary>
        /// Gets or sets the height of the page header for printing.
        /// </summary>
        /// <value>
        /// The height of the page header.
        /// </value>        
        public double PrintPageHeaderHeight
        {
            get { return (double)GetValue(PrintPageHeaderHeightProperty); }
            set { SetValue(PrintPageHeaderHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeaderHeight dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeaderHeight dependency property.
        /// </remarks>   
        public static readonly DependencyProperty PrintPageHeaderHeightProperty =
            DependencyProperty.Register("PrintPageHeaderHeight", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(0.0, OnPrintPageHeaderHeightChanged));

        private static void OnPrintPageHeaderHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageHeaderHeight = (double) e.NewValue;
        }


        #endregion

        #region PrintPageFooterHeight Property


        /// <summary>
        /// Gets or sets the height of the page footer for printing.
        /// </summary>
        /// <value>
        /// The height of the page footer.
        /// </value>  
        public double PrintPageFooterHeight
        {
            get { return (double)GetValue(PrintPageFooterHeightProperty); }
            set { SetValue(PrintPageFooterHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageFooterHeight dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageFooterHeight dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintPageFooterHeightProperty =
            DependencyProperty.Register("PrintPageFooterHeight", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(0.0, OnPrintPageFooterHeightChanged));

        private static void OnPrintPageFooterHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageFooterHeight = (double) e.NewValue;
        }


        #endregion

        #region PrintHeaderTemplate Property

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the page header for printing.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the page header. The default is <b>null</b>.
        /// </value>
        public DataTemplate PrintPageHeaderTemplate
        {
            get { return (DataTemplate)GetValue(PrintPageHeaderTemplateProperty); }
            set { SetValue(PrintPageHeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeaderTemplate dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageHeaderTemplate dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintPageHeaderTemplateProperty =
            DependencyProperty.Register("PrintPageHeaderTemplate", typeof(DataTemplate), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(null, OnPrintPageHeaderTemplateChanged));

        private static void OnPrintPageHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageHeaderTemplate = (DataTemplate) e.NewValue;
        }


        #endregion

        #region PrintPageFooterTemplate


        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the page footer for printing.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the page footer. The default is <b>null</b>.
        /// </value>
        public DataTemplate PrintPageFooterTemplate
        {
            get { return (DataTemplate)GetValue(PrintPageFooterTemplateProperty); }
            set { SetValue(PrintPageFooterTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageFooterTemplate dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintPageFooterTemplate dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintPageFooterTemplateProperty =
            DependencyProperty.Register("PrintPageFooterTemplate", typeof(DataTemplate), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(null, OnPrintPageFooterTemplateChanged));

        private static void OnPrintPageFooterTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintPageFooterTemplate = (DataTemplate) e.NewValue;
        }



        #endregion

        #region PrintHeaderRowHeight Property

        /// <summary>
        /// Gets or sets the height of header row in SfDataGrid for printing.
        /// </summary>
        /// <value>
        /// The height of the header row in SfDataGrid.
        /// </value>
        public double PrintHeaderRowHeight
        {
            get { return (double)GetValue(PrintHeaderRowHeightProperty); }
            set { SetValue(PrintHeaderRowHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintHeaderRowHeight dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintHeaderRowHeight dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintHeaderRowHeightProperty =
            DependencyProperty.Register("PrintHeaderRowHeight", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(28d, OnPrintHeaderRowHeightChanged));

        private static void OnPrintHeaderRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintHeaderRowHeight = (double) e.NewValue;
        }


        #endregion

        #region PrintRowHeight Property

        /// <summary>
        /// Gets or sets the height of DataRow in SfDataGrid for printing.
        /// </summary>
        /// <value>
        /// The height of the DataRow in SfDataGrid.
        /// </value>
        public double PrintRowHeight
        {
            get { return (double)GetValue(PrintRowHeightProperty); }
            set { SetValue(PrintRowHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintRowHeight dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintRowHeight dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintRowHeightProperty =
            DependencyProperty.Register("PrintRowHeight", typeof(double), typeof(PrintPreviewAreaControl),
                new PropertyMetadata(24d, OnPrintPageRowHeight));

        private static void OnPrintPageRowHeight(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printCtrl = d as PrintPreviewAreaControl;
            if (printCtrl.PrintManagerBase != null)
                printCtrl.PrintManagerBase.PrintRowHeight = (double) e.NewValue;
        }

        #endregion
        
        #region PrintOrientation Property

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation"/> that indicates whether the documents are printed in portrait or landscape mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation"/> enumeration that specifies the orientation for printing. The default value is <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation.Portrait"/>.
        /// </value>
        public PrintOrientation PrintOrientation
        {
            get { return (PrintOrientation)GetValue(PrintOrientationProperty); }
            set { SetValue(PrintOrientationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintOrientation dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintOrientation dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintOrientationProperty =
            DependencyProperty.Register("PrintOrientation", typeof(PrintOrientation), typeof(PrintPreviewAreaControl), new PropertyMetadata(PrintOrientation.Portrait, OnPrintOrientationChanged));

        private static void OnPrintOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printAreaCtrl = d as PrintPreviewAreaControl;
            if (printAreaCtrl.PrintManagerBase != null)
            {
                
                if (printAreaCtrl.PrintManagerBase.isSuspended) return;
                printAreaCtrl.PrintManagerBase.isSuspended = true;
                if (((PrintOrientation)e.NewValue == PrintOrientation.Portrait && printAreaCtrl.PrintManagerBase.PrintPageHeight < printAreaCtrl.PrintManagerBase.PrintPageWidth) ||
                    ((PrintOrientation)e.NewValue == PrintOrientation.Landscape && printAreaCtrl.PrintManagerBase.PrintPageHeight > printAreaCtrl.PrintManagerBase.PrintPageWidth))
                {
                    var width = printAreaCtrl.PrintManagerBase.PrintPageWidth;
                    var height = printAreaCtrl.PrintManagerBase.PrintPageHeight;
                    printAreaCtrl.PrintManagerBase.PrintPageWidth = height;
                    printAreaCtrl.PrintManagerBase.PrintPageHeight = width;
                }
                if (printAreaCtrl.PrintManagerBase.PrintPageOrientation != (PrintOrientation)e.NewValue)
                {
                    printAreaCtrl.PrintManagerBase.PrintPageOrientation = (PrintOrientation)e.NewValue;
                    printAreaCtrl.PrintManagerBase.isSuspended = false;
                    if (printAreaCtrl.PrintManagerBase.InValidatePreviewPanel != null)
                        printAreaCtrl.PrintManagerBase.InValidatePreviewPanel(false);
                    printAreaCtrl.OnZoomFactorChanged(printAreaCtrl.ZoomFactor);
                }
                else
                {
                    printAreaCtrl.PrintManagerBase.isSuspended = false;
                }
            }
        }
        
        #endregion

        #region PrintScaleOption Property

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions"/> that indicates the number of rows or columns are scaled to fit the page when it is printed.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions"/> enumeration that specifies how the rows or columns are scaled to fit the page. The default value is <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions.NoScaling"/>.
        /// </value>
        public PrintScaleOptions PrintScaleOption
        {
            get { return (PrintScaleOptions)GetValue(PrintScaleOptionProperty); }
            set { SetValue(PrintScaleOptionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintScaleOption dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PrintScaleOption dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PrintScaleOptionProperty =
            DependencyProperty.Register("PrintScaleOption", typeof (PrintScaleOptions), typeof (PrintPreviewAreaControl),
                new PropertyMetadata(PrintScaleOptions.NoScaling, OnPrintScaleOptionChanged));

        private static void OnPrintScaleOptionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printAreaCtrl = d as PrintPreviewAreaControl;
            if (printAreaCtrl.PrintManagerBase != null)
                printAreaCtrl.PrintManagerBase.PrintScaleOption = (PrintScaleOptions) e.NewValue;
        }

        #endregion

        #region ZoomFactor Property

        /// <summary>
        /// Gets or sets a value that indicates how large the pages will appear.
        /// </summary>
        /// <value>
        /// A value indicating how large the pages will appear. The default zoom factor is 100.0.
        /// </value>
        public double ZoomFactor
        {
            get { return (double)GetValue(ZoomFactorProperty); }
            set { SetValue(ZoomFactorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.ZoomFactor dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.ZoomFactor dependency property.
        /// </remarks> 
        public static readonly DependencyProperty ZoomFactorProperty =
            DependencyProperty.Register("ZoomFactor", typeof (double), typeof (PrintPreviewAreaControl),
                new PropertyMetadata(100.0, OnZoomFactorDependencyPropertyChanged));

        private static void OnZoomFactorDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printAreaCtrl = d as PrintPreviewAreaControl;
            printAreaCtrl.OnZoomFactorChanged((double) e.NewValue);
        }

        
        #endregion

        #region PageIndex Property

        /// <summary>
        /// Gets or sets the index of the page.
        /// </summary>
        /// <value>
        /// An index of the page. The default index is 1.
        /// </value>
        public int PageIndex
        {
            get { return (int)GetValue(PageIndexProperty); }
            set { SetValue(PageIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PageIndex dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl.PageIndex dependency property.
        /// </remarks> 
        public static readonly DependencyProperty PageIndexProperty =
            DependencyProperty.Register("PageIndex", typeof (int), typeof (PrintPreviewAreaControl),
                new PropertyMetadata(1, OnPageIndexChanged));

        private static void OnPageIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var printAreaCtrl = d as PrintPreviewAreaControl;
            if (!printAreaCtrl.isPageIndexSetFromOverride && printAreaCtrl.PartPrintWindowPanel != null && printAreaCtrl.PrintManagerBase != null)
             {
                 printAreaCtrl.PartPrintWindowPanel.SetVerticalOffset(((int)e.NewValue - 1) *
                                                        (printAreaCtrl.PartPrintWindowPanel.ExtentHeight / printAreaCtrl.PrintManagerBase.pageCount));
             }
        }
        
        #endregion

        #endregion

        #region Private Methods

        internal void OnZoomFactorChanged(double value)
        {
            if (PartPrintWindowPanel != null)
            {
                PartPrintWindowPanel.Child.Zoom(value);
                //WPF -33625 Print preview page is automatically changing when zoom level is changed.
                //Need to update the scroll info before calculate the vertical offset while the zoom level of the page is increased.
                //Set the default viewport height as a page height while the Zoom level is changed below the default Zoom size of the page. 
                var pageheight = (this.PrintManagerBase.PrintPageHeight / 100) * value;
                if (pageheight < PartPrintWindowPanel.ViewportHeight)
                    pageheight = PartPrintWindowPanel.ActualHeight + (PartPrintWindowPanel.ScrollOwner.ComputedHorizontalScrollBarVisibility == Visibility.Visible ? SystemParameters.ScrollWidth : 0);
                var extentheight = PrintManagerBase.pageCount * pageheight;
                PartPrintWindowPanel.UpdateScrollInfo(new Size(PartPrintWindowPanel.ViewportWidth, pageheight), new Size(PartPrintWindowPanel.ViewportWidth, extentheight));
                PartPrintWindowPanel.SetVerticalOffset((PartPrintWindowPanel.Child.PageIndex - 1) * pageheight);
            }

        }
        
        #endregion

        #region Override
        /// <summary>
        /// Builds the visual tree for the PrintPreviewAreaControl when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            PartPrintWindowPanel = GetTemplateChild("PART_PrintWindowPanel") as PrintPreviewPanel;
            if (PartPrintWindowPanel == null || PrintManagerBase == null)
                return;
            PartPrintWindowPanel.SetPrintManagerBase(PrintManagerBase);
            InitializePreviewWindow();
            PartPrintWindowPanel.SetPageIndex = index =>
            {
                isPageIndexSetFromOverride = true;
                PageIndex = index;
                isPageIndexSetFromOverride = false;
            };
            PartPrintWindowPanel.InValidateParent = () =>
            {
                TotalPages = PrintManagerBase.pageCount;
            };
            TotalPages = PrintManagerBase.pageCount;
            PartPrintWindowPanel.Child.Loaded += OnPrintPreviewControlLoaded;
        }

        #endregion
        /// <summary>
        /// if change the print settings using datagrid.printsettings then need to update print settings options in preview window
        /// </summary>
        internal void InitializePreviewWindow()
        {
            PrintOrientation = (PrintOrientation != PrintManagerBase.PrintPageOrientation) ? PrintManagerBase.PrintPageOrientation : PrintOrientation;
            PrintScaleOption = (PrintScaleOption != PrintManagerBase.PrintScaleOption) ? PrintManagerBase.PrintScaleOption : PrintScaleOption;
            PrintPageMargin = (PrintPageMargin != PrintManagerBase.PrintPageMargin) ? PrintManagerBase.PrintPageMargin : PrintPageMargin;
            PrintHeaderRowHeight = (PrintHeaderRowHeight != PrintManagerBase.PrintHeaderRowHeight) ? PrintManagerBase.PrintHeaderRowHeight : PrintHeaderRowHeight;
            PrintPageFooterHeight = (PrintPageFooterHeight != PrintManagerBase.PrintPageFooterHeight) ? PrintManagerBase.PrintPageFooterHeight : PrintPageFooterHeight;
            PrintPageHeaderHeight = (PrintPageHeaderHeight != PrintManagerBase.PrintPageHeaderHeight) ? PrintManagerBase.PrintPageHeaderHeight : PrintPageHeaderHeight;
            PrintPageHeight = (PrintPageHeight != PrintManagerBase.PrintPageHeight) ? PrintManagerBase.PrintPageHeight : PrintPageHeight;
            PrintPageMargin = (PrintPageMargin != PrintManagerBase.PrintPageMargin) ? PrintManagerBase.PrintPageMargin : PrintPageMargin;
            PrintPageWidth = (PrintPageWidth != PrintManagerBase.PrintPageWidth) ? PrintManagerBase.PrintPageWidth : PrintPageWidth;
        }
        #region Events

        void OnPrintPreviewControlLoaded(object sender, RoutedEventArgs e)
        {
            var heightdelta = PrintManagerBase.PrintPageHeight / 100;
            ZoomFactor = (int)(PartPrintWindowPanel.ViewportHeight / heightdelta);
        }

        #endregion

        #region Commands

        #region PrintCommand

        BaseCommand printCommand;
        /// <summary>
        /// Gets the command to invoke the print process in SfDataGrid.
        /// </summary>
        public BaseCommand PrintCommand
        {
            get { return printCommand ?? (printCommand = new BaseCommand(OnprintCommandClicked, o => (PrintManagerBase!= null?PrintManagerBase.pageCount:0) > 0));}
        }

        private void OnprintCommandClicked(object obj)
        {
#if WPF
            PrintManagerBase.PrintWithDialog();
#else
            PrintManagerBase.Print();
#endif

        }

        #endregion

#if WPF

        #region QuickPrintCommand

        BaseCommand quickPrintCommand;
        /// <summary>        
        /// Gets the command to invoke the quick print process in SfDataGrid.        
        /// </summary>
        public BaseCommand QuickPrintCommand
        {
            get { return quickPrintCommand ?? (quickPrintCommand = new BaseCommand(OnQuickprintCommandClicked, o => (PrintManagerBase != null ? PrintManagerBase.pageCount : 0) > 0));}
        }

        private void OnQuickprintCommandClicked(object obj)
        {
            PrintManagerBase.Print();
        }

        #endregion

#endif

        #region FirstCommand

        BaseCommand firstCommand;
        /// <summary>
        /// Gets the command to navigate first page in print document.
        /// </summary>
        public BaseCommand FirstCommand
        {
            get
            {
                return firstCommand ??
                       (firstCommand =
                           new BaseCommand(OnFirstCommandClicked,
                               o =>
                                   PrintManagerBase != null && PrintManagerBase.pageCount > 0 && PartPrintWindowPanel != null &&
                                   PartPrintWindowPanel.Child.PageIndex != 1));
            }
        }

        private void OnFirstCommandClicked(object obj)
        {
            if (PartPrintWindowPanel != null) PartPrintWindowPanel.SetVerticalOffset(0);
        }

        #endregion

        #region PreviousCommand

        BaseCommand previousCommand;
        /// <summary>
        /// Gets the command to navigate previous page in print document.
        /// </summary>
        public BaseCommand PreviousCommand
        {
            get
            {
                return previousCommand ?? (previousCommand = new BaseCommand(OnPreviousCommandClicked,
                    o => PrintManagerBase != null && PrintManagerBase.pageCount > 0 && PartPrintWindowPanel != null &&
                                   PartPrintWindowPanel.Child.PageIndex != 1));
            }
        }

        private void OnPreviousCommandClicked(object obj)
        {
            if (PartPrintWindowPanel == null) return;
            var pageIndex = PartPrintWindowPanel.Child.PageIndex;
            if (pageIndex > 1 && pageIndex <= PrintManagerBase.pageCount)
                PartPrintWindowPanel.SetVerticalOffset((pageIndex - 2) * (PartPrintWindowPanel.ExtentHeight / PrintManagerBase.pageCount));
        }

        #endregion

        #region NextCommand

        BaseCommand nextCommand;
        /// <summary>
        /// Gets the command to navigate next page in print document.
        /// </summary>
        public BaseCommand NextCommand
        {
            get
            {
                return nextCommand ??
                       (nextCommand =
                           new BaseCommand(OnNextCommandClicked,
                               o => PrintManagerBase != null && PrintManagerBase.pageCount > 0 && PartPrintWindowPanel != null &&
                                    PartPrintWindowPanel.Child.PageIndex < PrintManagerBase.pageCount));
            }
        }

        private void OnNextCommandClicked(object obj)
        {
            if (PartPrintWindowPanel == null) return;
            var pageIndex =PartPrintWindowPanel.Child.PageIndex;
            if (pageIndex > 0 && pageIndex < PrintManagerBase.pageCount)
                PartPrintWindowPanel.SetVerticalOffset((pageIndex) * (PartPrintWindowPanel.ExtentHeight / PrintManagerBase.pageCount));
        }

        #endregion

        #region LastCommand

        BaseCommand lastCommand;
        /// <summary>
        /// Gets the command to navigate last page in print document.
        /// </summary>
        public BaseCommand LastCommand
        {
            get { return lastCommand ?? (lastCommand = new BaseCommand(OnLastCommandClicked, o => PrintManagerBase != null && PrintManagerBase.pageCount > 0 && PartPrintWindowPanel != null && PartPrintWindowPanel.Child.PageIndex < PrintManagerBase.pageCount)); }
        }

        private void OnLastCommandClicked(object obj)
        {
            if (PartPrintWindowPanel != null)
                PartPrintWindowPanel.SetVerticalOffset((PrintManagerBase.pageCount - 1)*
                                                       (PartPrintWindowPanel.ExtentHeight / PrintManagerBase.pageCount));
        }

        #endregion

        #endregion

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes in <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Dispose Member
        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewAreaControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>        
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
                printManager = null;
            isdisposed = true;
        }

        #endregion

    }
}
