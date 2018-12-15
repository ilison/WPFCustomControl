#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections;
using System.ComponentModel;
using Syncfusion.Data;
using System;
using System.Collections.Generic;
using System.Linq;
#if WinRT
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Printing;
using Windows.Graphics.Printing;
using Windows.UI.Xaml.Data;
using Windows.ApplicationModel.Core;
using System.Reflection;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Printing;
using System.Windows.Media;
using System.Globalization;
using System.Windows.Media.Imaging;
#endif
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid.Cells;
using Syncfusion.UI.Xaml.ScrollAxis;


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides the base implementation for printing related operation in SfDataGrid. 
    /// </summary>
    public class PrintManagerBase : INotifyPropertyChanged, IDisposable
    {
        #region Private Fields

        internal readonly List<PrintPageControl> printControls;        
        private const string GroupCaptionConstant = "{ColumnName} : {Key} - {ItemsCount} Items";       
        private Dictionary<int, List<RowInfo>> pageDictionary;
        private Thickness printPageMargin = new Thickness(96);
        private double equalcolumnWidth;
        // As Per ISO A4 sheet size is 210 mm × 297 mm which is equal to 793.700787402 x 1122.519685039 pixels
        private double printPageHeight = 1122.52;
        private double printPageWidth = 793.70;
        private double printPageHeaderHeight;
        private double printPageFooterHeight;
        private double printHeaderRowHeight = 28;
        private bool canPrintStackedHeaders = false;
        private double printRowHeight = 24;
        private double totalRecordsHeight = 0.0;
        private DataTemplate printPageHeaderTemplate;
        private DataTemplate printPageFooterTemplate;
        private PrintScaleOptions printScaleOption = PrintScaleOptions.NoScaling;
        private FlowDirection printFlowDirection = FlowDirection.LeftToRight;
        private bool isdisposed = false;
#if WPF
        private int panelBorderThickness = 2;
#endif
        private PrintOrientation printPageOrientation = PrintOrientation.Portrait;

#if WinRT
        internal IPrintDocumentSource printDocumentSource;
        internal PrintDocument printDocument;
        internal PrintTask printTask;
#endif

        #endregion Private Fields

        #region Protected

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.Data.ICollectionViewAdv"/> which manage the records,
        /// sorting, grouping, summaries and filtering in SfDataGrid.
        /// </summary>
        protected ICollectionViewAdv View { get; private set; }

        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.Data.IPropertyAccessProvider"/> to retrieve the formatted cell value of corresponding column.
        /// </summary>
        /// <value>
        /// The corresponding cell value of column.
        /// </value>
        protected internal IPropertyAccessProvider Provider { get; set; }

        private double _indentColumnWidth = 20d;
        /// <summary>
        /// Gets or sets the width for indent columns while printing. 
        /// </summary>
        /// <remarks>
        /// You can set zero when you don't want to display indent columns while printing selected list of records.
        /// </remarks>
        protected double IndentColumnWidth
        {
            get { return _indentColumnWidth; }
            set { _indentColumnWidth = value; }
        }

        #endregion Protected

        #region Internal

        internal IList source;
        internal int pagesPerRecord = 0;
        internal int pageCount = 0;
        internal int groupCount;

        /// <summary>
        /// Invalidates PrintPreviewPanel
        /// </summary>
        internal Action<bool> InValidatePreviewPanel = null;

        internal bool isSuspended;
        internal bool isPagesInitialized;
#if WPF
        //visual brushes for drawing checkbox column
        internal VisualBrush CheckedBrush;
        internal VisualBrush UnCheckedBrush;
        internal VisualBrush ThreeStateBrush;
#endif

        #endregion Internal

        #region Proeprties

        /// <summary>
        /// Gets or sets the page dictionary that holds the row information for each page.
        /// </summary>
        /// <value>
        /// The row info of each page.
        /// </value>
        protected Dictionary<int, List<RowInfo>> PageDictionary
        {
            get { return pageDictionary; }
            set { pageDictionary = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether equal width is set for all columns to fit in single page.
        /// </summary>
        /// <value>
        /// <b>true</b> if the column width is fit in single page; otherwise, <b>false</b>.
        /// </value>       
        protected bool AllowColumnWidthFitToPrintPage { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the column headers are repeated in each page .                
        /// </summary>
        /// <value>
        /// <b>true</b> if the column header is repeated in each page; otherwise, <b>false</b>.
        /// </value>
        protected bool AllowRepeatHeaders { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the style included for printing.      
        /// </summary>
        /// <value>
        /// <b>true</b> if the SfDataGrid is printed with style; otherwise, <b>false</b>.
        /// </value>
        protected bool AllowPrintStyles { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the stacked header included for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the SfDataGrid is printed with stacked header; otherwise, <b>false</b>.
        /// </value>
        protected bool CanPrintStackedHeaders
        {
            get { return canPrintStackedHeaders; }
            set
            {
                canPrintStackedHeaders = value;
            }
        }

#if WPF

        /// <summary>
        /// Gets or sets a value that indicates whether the SfDataGrid is printed using drawing concept instead of creating cell.
        /// </summary>
        /// <value>
        /// <b>true</b> if the print job is done by drawing concept; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        /// Printing using drawing concept provides better performance.
        /// </remarks>
        protected bool AllowPrintByDrawing { get; set; }

#endif

        /// <summary>
        /// Gets or sets the margin of page for printing.
        /// </summary>
        /// <value>
        /// Provides the margin value for the page.
        /// </value>
        public Thickness PrintPageMargin
        {
            get { return printPageMargin; }
            set
            {
                if (printPageMargin == value) return;
                printPageMargin = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageMargin");
            }
        }

       /// <summary>
       /// Gets or sets the height of page for printing.
       /// </summary>
       /// <value>
       /// The preferred height of the page.
       /// </value>
        public double PrintPageHeight
        {
            get { return printPageHeight; }
            set
            {
                if (printPageHeight == value) return;
                printPageHeight = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageHeight");
            }
        }

        /// <summary>
        /// Gets or sets the width of page for printing.
        /// </summary>
        /// <value>
        /// The preferred width of the page.
        /// </value>
        public double PrintPageWidth
        {
            get { return printPageWidth; }
            set
            {
                if (printPageWidth == value) return;
                printPageWidth = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageWidth");
            }
        }

        /// <summary>
        /// Gets or sets the height of page header for printing.
        /// </summary>
        /// <value>
        /// The preferred width of the page header.
        /// </value>
        public double PrintPageHeaderHeight
        {
            get { return printPageHeaderHeight; }
            set
            {
                if (printPageHeaderHeight == value) return;
                printPageHeaderHeight = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageHeaderHeight");
            }
        }

        /// <summary>
        /// Gets or sets the height of page footer for printing.
        /// </summary>
        /// <value>
        /// The preferred width of the page footer.
        /// </value>
        public double PrintPageFooterHeight
        {
            get { return printPageFooterHeight; }
            set
            {
                if (printPageFooterHeight == value) return;
                printPageFooterHeight = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageFooterHeight");
            }
        }

        /// <summary>
        /// Gets or sets the height of the header row for printing.
        /// </summary>
        /// <value>
        /// The preferred height of the header row.
        /// </value>
        public double PrintHeaderRowHeight
        {
            get { return printHeaderRowHeight; }
            set
            {
                if (printHeaderRowHeight == value) return;
                printHeaderRowHeight = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintHeaderRowHeight");
            }
        }

        /// <summary>
        /// Gets or sets the height of the DataRow for printing.
        /// </summary>
        /// <value>
        /// The preferred height of the DataRow.
        /// </value>
        public double PrintRowHeight
        {
            get { return printRowHeight; }
            set
            {
                if (printRowHeight == value) return;
                printRowHeight = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintRowHeight");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the page header for printing.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the page header. The default is <b>null</b>.
        /// </value>
        public DataTemplate PrintPageHeaderTemplate
        {
            get { return printPageHeaderTemplate; }
            set
            {
                if (printPageHeaderTemplate == value) return;
                printPageHeaderTemplate = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageHeaderTemplate");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the page footer for printing.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the page footer. The default is <b>null</b>.
        /// </value>>
        public DataTemplate PrintPageFooterTemplate
        {
            get { return printPageFooterTemplate; }
            set
            {
                if (printPageFooterTemplate == value) return;
                printPageFooterTemplate = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageFooterTemplate");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the page content is oriented for printing.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation"/> enumeration that specifies how the page content is oriented.
        /// The default orientation is <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation.Portrait"/>.
        /// </value>
        public PrintOrientation PrintPageOrientation
        {
            get { return printPageOrientation; }
            set
            {
                if (printPageOrientation == value) return;
                printPageOrientation = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintPageOrientation");
            }
        }

        /// <summary>
        /// Gets or sets a value indicating how the page content is scaled for printing.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions"/> enumeration that specifies how the page content is scaled.
        /// The default orientation is <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions.NoScaling"/>.
        /// </value>
        public PrintScaleOptions PrintScaleOption
        {
            get { return printScaleOption; }
            set
            {
                if (printScaleOption == value) return;
                printScaleOption = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintScaleOption");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates the direction of page content for printing.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.FlowDirection"/> that specifies the direction of page content. The default direction is <see cref="System.Windows.FlowDirection.LeftToRight"/>.
        /// </value>
        public FlowDirection PrintFlowDirection
        {
            get { return printFlowDirection; }
            set
            {
                if (printFlowDirection == value) return;
                printFlowDirection = value;
                OnPrintPropertyChanged();
                OnPropertyChanged("PrintFlowDirection");
            }
        }

        #endregion Proeprties

        #region Ctor

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class.
        /// </summary>
        /// <param name="view">
        /// The corresponding view to print.
        /// </param>
        public PrintManagerBase(ICollectionViewAdv view)
        {
            if (view == null)
                throw new ArgumentNullException("View cannot be as null");

            this.View = view;
#if WPF
            CheckedBrush = GetCheckBoxVisualBrush(true);
            UnCheckedBrush = GetCheckBoxVisualBrush(false);
            ThreeStateBrush = GetCheckBoxVisualBrush(null);
#endif
            if (printControls == null)
                printControls = new List<PrintPageControl>();
#if WinRT
            RegisterForPrinting();
#endif
        }

        #endregion Ctor

        #region private methods

        private void OnPrintPropertyChanged()
        {
            if (isSuspended) return;
            if (InValidatePreviewPanel != null)
                InValidatePreviewPanel(false);
        }

        private double GetScaleXValue()
        {
#if WPF
            return (PrintPageWidth - (PrintPageMargin.Left + PrintPageMargin.Right + panelBorderThickness)) /
                   GetColumnNames().Sum(columnName => GetColumnWidth(columnName));
#else
            return (PrintPageWidth - (PrintPageMargin.Left + PrintPageMargin.Right )) /
                   GetColumnNames().Sum(columnName => GetColumnWidth(columnName));
#endif
        }

        private double GetScaleYValue()
        {
            return (PrintPageHeight - (PrintPageMargin.Top + PrintPageMargin.Bottom + PrintPageFooterHeight + PrintHeaderRowHeight)) /
                   (PrintPageHeaderHeight + GetTotalRecordsHeight() +
                    (View.TableSummaryRows.Count * PrintRowHeight));
        }

        #endregion private methods

        #region internal methods

        internal void ProcessPrintPageScale(PrintPageControl pageControl)
        {
            switch (PrintScaleOption)
            {
                case PrintScaleOptions.NoScaling:
                    pageControl.Scale(1, 1);
                    break;

                case PrintScaleOptions.FitAllColumnsonOnePage:
                    pageControl.Scale(GetScaleXValue(), 1);
                    break;

                case PrintScaleOptions.FitAllRowsonOnePage:
                    pageControl.Scale(1, GetScaleYValue());
                    break;

                case PrintScaleOptions.FitViewonOnePage:
                    pageControl.Scale(GetScaleXValue(), GetScaleYValue());
                    break;
            }
        }

        /// <summary>
        /// Gets the corresponding data source for printing.
        /// </summary>
        /// <returns>
        /// Returns the corresponding data source.
        /// </returns>
        protected virtual IList GetSourceListForPrinting()
        {
            if (View is Syncfusion.Data.PagedCollectionView)
                source = (View as Syncfusion.Data.PagedCollectionView).GetInternalList();
            else if (View is Syncfusion.Data.VirtualizingCollectionView)
                source = (View as Syncfusion.Data.VirtualizingCollectionView).GetInternalSource() as IList;
            else if (View.GroupDescriptions.Any())
                source = ToIEnumerable(View.TopLevelGroup.GetEnumerator()).ToList();
            else
                source = View.Records.ToList();
            return source;
        }

        #endregion internal methods

        #region public methods

#if WPF

        /// <summary>
        /// Prints the content of SfDataGrid.
        /// </summary>
        public void Print()
        {
            InitializePrint(!isPagesInitialized);
            var printDialog = new PrintDialog
            {
                PrintTicket =
                {
                    PageOrientation =
                        PrintPageOrientation == PrintOrientation.Landscape
                            ? PageOrientation.Landscape
                            : PageOrientation.Portrait
                }
            };
            Print(printDialog, true);
        }

        /// <summary>
        /// Prints the content of SfDataGrid according to the print ticket and print queue configurations in the print dialog.
        /// </summary>
        /// <param name="printDialog">
        /// Configures the PrintTicket and PrintQueue according to the user input for printing.
        /// </param>
        public void Print(PrintDialog printDialog)
        {
            if (printDialog == null)
                throw new ArgumentNullException("printDialog", "printDialog can't be null");

            InitializePrint(!isPagesInitialized);
            Print(printDialog, printDialog.ShowDialog());
        }

        internal void PrintWithDialog()
        {
            var printDialog = new PrintDialog
            {
                UserPageRangeEnabled = true,
                PrintTicket =
                {
                    PageOrientation =
                        PrintPageOrientation == PrintOrientation.Landscape
                            ? PageOrientation.Landscape
                            : PageOrientation.Portrait
                },
            };

            Print(printDialog, printDialog.ShowDialog());
        }

        /// <summary>
        /// Prints the content of SfDataGrid according to the print ticket and print queue configures in the print dialog.
        /// </summary>
        /// <param name="printDialog">
        /// Configures the PrintTicket and PrintQueue according to the user input for printing.
        /// </param>
        /// <param name="canPrint">
        /// Decides whether the print can be performed or not.
        /// </param>
        protected virtual void Print(PrintDialog printDialog, bool? canPrint)
        {
            if (printDialog == null)
                printDialog = new PrintDialog();

            printDialog.PrintTicket.PageMediaSize = new PageMediaSize(PrintPageWidth,
                PrintPageHeight);

            if (canPrint != null && (bool)canPrint)
            {
                var fixedDoc = new FixedDocument();

                int start = 1;
                int end = pageCount;
                if (printDialog.PageRange.PageFrom != 0 && printDialog.PageRange.PageTo != 0)
                {
                    start = printDialog.PageRange.PageFrom;
                    end = printDialog.PageRange.PageTo > end ? end : printDialog.PageRange.PageTo;
                }
                for (var i = start; i <= end; i++)
                {
                    var printpageControl = printControls.FirstOrDefault(x => x.PageIndex == i) ?? CreatePage(i);
                    var pageContent = new PageContent();
                    var fixedPage = new FixedPage
                    {
                        Height = PrintPageHeight,
                        Width = PrintPageWidth,
                        PrintTicket = printDialog.PrintTicket
                    };
                    fixedPage.Children.Add(printpageControl);
                    ((IAddChild)pageContent).AddChild(fixedPage);
                    fixedDoc.Pages.Add(pageContent);
                }

                printDialog.PrintDocument(fixedDoc.DocumentPaginator, "Printing");
                foreach (var page in fixedDoc.Pages)
                {
                    page.Child.Children.Clear();
                }
            }
            else
            {
                printControls.Clear();
            }
        }

#endif

        /// <summary>
        /// Creates the page for the specified page index and arrange the SfDataGrid content with in that page.
        /// </summary>
        /// <param name="pageIndex">
        /// The corresponding page index to create page.
        /// </param>
        /// <returns>
        /// Returns the new <see cref="Syncfusion.UI.Xaml.Grid.PrintPageControl"/> for the specified index.
        /// </returns>
        protected internal virtual PrintPageControl CreatePage(int pageIndex)
        {
            return CreatePage(pageIndex, new PrintPageControl(this) 
            {
                PageIndex = pageIndex,
                DataContext = this,
                TotalPages = this.PageDictionary != null ? this.PageDictionary.Count : 1
            });
        }

        /// <summary>
        /// Creates the page corresponding to the specified page index and page control.
        /// </summary>
        /// <param name="pageIndex">
        /// The corresponding page index to create page.
        /// </param>
        /// <param name="pageControl">
        /// The corresponding page control to create page.
        /// </param>
        /// <returns></returns>
        protected internal virtual PrintPageControl CreatePage(int pageIndex, PrintPageControl pageControl)
        {
            if (pageControl.Content is PrintPagePanel)
                (pageControl.Content as PrintPagePanel).Children.Clear();

            if (pageDictionary == null || pageDictionary.Count <= 0)
                return pageControl;

            pageControl.DataContext = this;
            pageControl.TotalPages = pageDictionary.Count;
            pageControl.PageIndex = pageIndex;
            PrintPagePanel panel;
#if WPF
            if (AllowPrintByDrawing)
            {
                var onRender = new Action<DrawingContext, List<RowInfo>>(OnRender);
                panel = new PrintPagePanel(onRender, AllowPrintByDrawing)
                {
                    RowsInfoList = pageDictionary[pageIndex]
                };
                pageControl.Content = panel;
                ProcessPrintPageScale(pageControl);
                return pageControl;
            }
#endif
            panel = new PrintPagePanel();
            var RowsInfo = panel.RowsInfoList = pageDictionary[pageIndex];
            foreach (var rowInfo in RowsInfo)
            {
                AddRowToPrintPagePanel(panel, rowInfo, pageIndex);
            }

            pageControl.Content = panel;
            ProcessPrintPageScale(pageControl);
            return pageControl;
        }

        /// <summary>
        /// Converts an enumerator sequence to an enumerable sequence.
        /// </summary>
        /// <typeparam name="NodeEntry">
        /// The type of enumerator.
        /// </typeparam>
        /// <param name="enumerator">
        /// An enumerator node entry sequence to convert enumerable node entry.
        /// </param>
        /// <returns>
        /// The enumerable sequence containing the node entry in the enumerator sequence.
        /// </returns>
        public static IEnumerable<NodeEntry> ToIEnumerable<NodeEntry>(IEnumerator<NodeEntry> enumerator)
        {
            while (enumerator.MoveNext())
            {
                yield return enumerator.Current;
            }
            yield break;
        }

        #endregion public methods

        #region Virtual methods

        /// <summary>
        /// Initializes the print process such as print properties,settings and Construct PageDictonary,RowDictonary.
        /// </summary>
        /// <param name="needToInitProperties">
        /// Indicates whether the print properties and settings is initialized before printing.
        /// </param>
        protected internal virtual void InitializePrint(bool needToInitProperties)
        {
            isSuspended = true;
            if (needToInitProperties)
                InitializeProperties();

            var columnNames = GetColumnNames();
            if (!columnNames.Any())
                return;

            //Below if condition is executed  when we set print orientation using print settings
            if ((PrintPageOrientation == PrintOrientation.Portrait && PrintPageHeight < PrintPageWidth) ||
                        (PrintPageOrientation == PrintOrientation.Landscape && PrintPageHeight > PrintPageWidth))
            {
                var width = PrintPageWidth;
                PrintPageWidth = PrintPageHeight;
                PrintPageHeight = width;
            }

            //WPF-23379 need to reset the group count before it calculate.
            groupCount = 0;
            if (!(View is Syncfusion.Data.PagedCollectionView || View is Syncfusion.Data.VirtualizingCollectionView) && View.GroupDescriptions.Any())
                groupCount = View.GroupDescriptions.Count;

#if WPF
            
            equalcolumnWidth = (PrintPageWidth -
                                (PrintPageMargin.Left + PrintPageMargin.Right + panelBorderThickness +
                                 (groupCount * IndentColumnWidth))) / columnNames.Count;
            equalcolumnWidth = equalcolumnWidth < 0 ? 0 : equalcolumnWidth;
#else
            equalcolumnWidth = (PrintPageWidth -
                                (PrintPageMargin.Left + PrintPageMargin.Right +
                                 (groupCount * IndentColumnWidth))) / columnNames.Count;
            //WRT-5199 avoid decimal fractional value difference need to  roundoff the  value.
            equalcolumnWidth = equalcolumnWidth < 0 ? 0 : Math.Round(equalcolumnWidth, 2);
#endif
            Provider = View.GetPropertyAccessProvider();

            pageDictionary = new Dictionary<int, List<RowInfo>>();
            var avalHeight = PrintPageHeight -
                             (PrintPageMargin.Top + PrintPageMargin.Bottom + PrintPageHeaderHeight +
                              PrintPageFooterHeight);
            avalHeight = avalHeight < 0 ? 0 : avalHeight;
            var avalWidth = PrintPageWidth - (PrintPageMargin.Left + PrintPageMargin.Right);
            avalWidth = avalWidth < 0 ? 0 : avalWidth;

            source = GetSourceListForPrinting();

            ComputePages(source, new Size(avalWidth, avalHeight));
            pageCount = pageDictionary.Count;
            isSuspended = false;
            isPagesInitialized = true;
        }

        /// <summary>
        /// Initializes properties and settings for printing process.
        /// </summary>
        protected virtual void InitializeProperties()
        {
        }
        /// <summary>
        /// Get the Stacked Headers count.
        /// </summary>
        /// <returns>
        /// Return thecount of the stacked headers to be pinted.
        /// </returns>
        internal virtual int GetStackedHeaders()
        {
            return 0;
        }

        /// <summary>
        /// Gets the list of column names collection that need to be printed.
        /// </summary>
        /// <returns>
        /// Returns the collection of column name collection that need to be printed.
        /// </returns>
        protected virtual List<string> GetColumnNames()
        {
#if WPF
            return (from PropertyDescriptor prop in View.GetItemProperties() select prop.Name).ToList();
#else
            return (from KeyValuePair<string, PropertyInfo> prop in View.GetItemProperties() select prop.Value.Name).ToList();
#endif
        }

        /// <summary>
        /// Gets the column width for the specified mapping name of column.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its width.
        /// </param>
        /// <returns>
        /// Returns the column width of the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the column width based on the mapping name of the column.
        /// </remarks>
        protected virtual double GetColumnWidth(string mappingName)
        {
            return equalcolumnWidth;
        }

        /// <summary>
        /// Gets the header text of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its header text.
        /// </param>
        /// <returns>
        /// Returns the header text of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the header text of the column based on its mapping name.
        /// </remarks>
        protected virtual string GetColumnHeaderText(string mappingName)
        {
            return mappingName;
        }

        /// <summary>
        /// Gets the padding of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its padding.
        /// </param>
        /// <returns>
        /// Returns the padding of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the padding for column based on its mapping name.
        /// </remarks>
        protected virtual Thickness GetColumnPadding(string mappingName)
        {
            return new Thickness(5, 0, 0, 0);
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.TextAlignment"/> of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its <see cref="System.Windows.TextAlignment"/>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.TextAlignment"/> of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the <see cref="System.Windows.TextAlignment"/> for column based on its mapping name.
        /// </remarks>
        protected virtual TextAlignment GetColumnTextAlignment(string mappingName)
        {
            return TextAlignment.Left;
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.TextWrapping"/> of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its <see cref="System.Windows.TextWrapping"/>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.TextWrapping"/> of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the <see cref="System.Windows.TextWrapping"/> of column based on its mapping name.
        /// </remarks>
        protected virtual TextWrapping GetColumnTextWrapping(string mappingName)
        {
            return TextWrapping.NoWrap;
        }

        /// <summary>
        /// Gets the column element of the specified record and mapping name for printing each GridCell in a column.
        /// </summary>
        /// <param name="record">
        /// Specifies the corresponding record to get the column element.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its column element.
        /// </param>
        /// <returns>
        /// Returns the column element of the specified record and mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the column element based on its record and mapping name. 
        /// </remarks>
        protected virtual object GetColumnElement(object record, string mappingName)
        {
            var cellvalue = Provider.GetFormattedValue(((record is RecordEntry) ? (record as RecordEntry).Data : record), mappingName);
            if (cellvalue == null)
                return new TextBlock { };

            return new TextBlock
            {
                Text = cellvalue.ToString(),
                Padding = GetColumnPadding(mappingName),
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = GetColumnTextAlignment(mappingName),
                TextWrapping = GetColumnTextWrapping(mappingName)
            };
        }

        /// <summary>
        /// Gets the header UIElement of the column corresponding to its mapping name for printing the header cell of a column.
        /// </summary>        
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its header element.
        /// </param>
        /// <returns>
        /// Returns the header UIElement of the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the header UIElement of column based on its mapping name. 
        /// </remarks>
        protected virtual UIElement GetColumnHeaderElement(string mappingName)
        {
            return new TextBlock
            {
                Text = GetColumnHeaderText(mappingName),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = GetColumnTextWrapping(mappingName)
            };
        }

        /// <summary>
        /// Gets the StackedHeader UIElement of the column corresponding to its mapping name for printing the header cell of a column.
        /// </summary>        
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its header element.
        /// </param>
        /// <returns>
        /// Returns the Stacked Header UIElement of the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the header UIElement of column based on its mapping name.         /// </remarks> 
        protected virtual UIElement GetStackedColumnHeaderElement(string mappingName)
        {
            return new TextBlock
            {
                Text = mappingName,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = GetColumnTextWrapping(mappingName)
            };
        }

        /// <summary>
        /// Gets the string format of group caption.
        /// </summary>
        /// <returns>
        /// Returns the corresponding string format of group caption.
        /// </returns>
        /// <remarks>
        /// Override this method and customize string format of group caption.
        /// </remarks>
        protected virtual string GetGroupCaptionStringFormat()
        {
            return GroupCaptionConstant;
        }

        /// <summary>
        /// Gets a value that determines whether the corresponding row is CaptionSummaryRow for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the row is CaptionSummaryRow; otherwise , <b>false</b>.
        /// </value>
        protected virtual bool IsCaptionSummaryInRow
        {
            get
            {
                return true;
            }
        }
       
        /// <summary>
        /// Gets the row height for the specified record and row index.
        /// </summary>
        /// <param name="record">
        /// The corresponding record to get the row height.
        /// </param>
        /// <param name="rowindex">
        /// Index of the record in SourceList
        /// </param>
        /// <returns>
        /// Returns the row height of the specified record and row index. 
        /// </returns>
        /// <remarks>
        /// Override this method and customize the row height of the SfDataGrid for printing.
        /// </remarks>
        protected virtual double GetRowHeight(object record, int rowindex, RowType rowtype)
        {
            if (rowtype == RowType.StackedHeaderRow || rowtype == RowType.HeaderRow)
                return PrintHeaderRowHeight;
            return PrintRowHeight;
        }
        /*
        /// <summary>
        /// Invokes to get Pages per Record count.
        /// PagesPerRecord :
        /// if AllowColumnWidthFitToPrintPage API is false, and grid total width is greater than available page width then record columns are concatenated and  printed .
        /// If column is concatenated  same record will be printed more than one page with different column.
        /// This page count is mentioned as PagesPerRecord.
        /// </summary>
        /// <param name="columnNames">columnNames list</param>
        /// <param name="avaliableWidth">avaliableWidth</param>
        /// <returns>PagesPerRecord</returns>
        */

        /// <summary>
        /// Gets the number of records arranged to each page for the specified column names and the available page width.
        /// </summary>
        /// <param name="columnNames">
        /// The list of column name collection to get the pages per record count.
        /// </param>
        /// <param name="avaliableWidth">
        /// The available width of the page.
        /// </param>
        /// <returns>
        /// Returns the number of pages per record.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the page count option for printing.        
        /// </remarks>
        protected virtual int GetPagesPerRecord(List<string> columnNames, double avaliableWidth)
        {
            //WPF-20145  grouping case indent columns are considered when pagesperrecords are calculated.
            if (!AllowColumnWidthFitToPrintPage && (PrintScaleOption == PrintScaleOptions.NoScaling || PrintScaleOption == PrintScaleOptions.FitAllRowsonOnePage))
            {
                double totalwidth = groupCount * IndentColumnWidth;
                pagesPerRecord = 1;

                foreach (var name in columnNames)
                {
                    totalwidth += GetColumnWidth(name);
                    if (totalwidth > avaliableWidth)
                    {
                        totalwidth = GetColumnWidth(name) + (groupCount * IndentColumnWidth);
                        pagesPerRecord++;
                    }
                    else if (totalwidth == avaliableWidth)
                    {
                        totalwidth = 0;
                        pagesPerRecord++;
                    }
                }
            }
            else
                pagesPerRecord = 1;

            return pagesPerRecord;
        }

        /// <summary>
        /// Gets the record from the source list collection for the specified record index.
        /// </summary>
        /// <param name="recordIndex">
        /// The recordIndex to get the record from the source list collection.
        /// </param>
        /// <returns>
        /// Returns the record for the specified record index.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the record of the source list collection before printing.
        /// </remarks>
        protected virtual object GetRecordFromSourceList(int recordIndex)
        {
            var cellRects = new List<CellInfo>();
            object record = null;
            if ((recordIndex >= 0) && recordIndex < source.Count)
                record = source[recordIndex];

            return record;
        }

        /// <summary>
        /// Set DataTemplate content to PrintGridCell
        /// </summary>
        /// <param name="cell">cell</param>
        /// <param name="cellInfo">cellInfo</param>
        /// <param name="record">record</param>
        internal virtual void SetDataTemplateContentToPrintGridCell(ContentControl cell, CellInfo cellInfo, object record)
        {
            cell.Content = record;
        }

        /// <summary>
        /// Gets the total number of records count for printing.
        /// </summary>
        /// <returns>
        /// The total number of records count for printing.
        /// </returns>
        protected virtual int GetTotalRecordCount()
        {
            return source.Count;
        }

#if WPF     
        /// <summary>
        /// Gets the print document of SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the <see cref="System.Windows.Documents.FixedDocument"/> that contains the print document of SfDataGrid.
        /// </returns>
        public virtual FixedDocument GetPrintDocument()
        {
            this.InitializePrint(!this.isPagesInitialized);
            return GetPrintDocument(1, this.pageCount);
        }

        /// <summary>
        /// Gets the print document of SfDatGrid based on the specified start and end index of page.
        /// </summary>
        /// <param name="PageStartIndex">Start Index of Page</param>
        /// <param name="PageEndIndex">End index of Page</param>
        ///<return>FixedDocument</return>
        /// <summary>
        /// Gets the print document of SfDatGrid based on the specified start and end index of page.
        /// </summary>
        /// <param name="start">
        /// The start index of the page to get print document.
        /// </param>
        /// <param name="end">
        /// The end index of the page to get print document.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Documents.FixedDocument"/> for the specified start and end index of the page contains the print document of SfDataGrid.
        /// </returns>
        public virtual FixedDocument GetPrintDocument(int start, int end)
        {
            if (start < 1 || end > this.pageCount || end < 1)
                throw new ArgumentOutOfRangeException("Start Index and End Index must be greater than  1 and less than  or equal to" + this.pageCount);
            var fixedDocument = new FixedDocument();

            for (var i = start; i <= end; i++)
            {
                var printpageControl = this.printControls.FirstOrDefault(x => x.PageIndex == i) ?? this.CreatePage(i);
                var pageContent = new PageContent();
                var fixedPage = new FixedPage
                {
                    Height = this.PrintPageHeight,
                    Width = this.PrintPageWidth,
                };
                fixedPage.Children.Add(printpageControl);
                ((IAddChild)pageContent).AddChild(fixedPage);
                fixedDocument.Pages.Add(pageContent);
            }

            return fixedDocument;
        }

#endif

        #endregion Virtual methods

        #region Initial Page Computation for PageDictionary        
        /// <summary>
        /// Calculates the number of pages recommended to arrange the specified source with in the available size. 
        /// </summary>
        /// <param name="source">
        /// The list of source collection to compute page count.
        /// </param>
        /// <param name="avaliableSize">
        /// The desired size of the page.
        /// </param>
        /// <remarks>
        /// Override this method and customize page calculation for printing. 
        /// </remarks>
        protected virtual void ComputePages(IList source, Size avaliableSize)
        {
            var pageIndex = 1;
            var recordIndex = 0;
            var previoPageStartRecordIndex = 0;
            var totalRowHeight = 0.0;
            var rowDictionary = new List<RowInfo>();
            var columnNames = GetColumnNames();
            var totalColumnsWidth = columnNames.Sum(columnName => GetColumnWidth(columnName));
            if (totalColumnsWidth == 0.0)
                return;

            if (PrintFlowDirection.Equals(FlowDirection.RightToLeft))
                columnNames.Reverse();

            pagesPerRecord = GetPagesPerRecord(columnNames, avaliableSize.Width);
            pageDictionary.Add(pageIndex, rowDictionary);
            int startColumnIndex = 0, endColumnIndex;
            ComputeColumnForPage(pageIndex, columnNames, avaliableSize, out startColumnIndex, out endColumnIndex);
            int stackedHeaderCount = GetStackedHeaders();
            if (startColumnIndex < 0 || endColumnIndex < 0)
                return;
            if (canPrintStackedHeaders && stackedHeaderCount != 0)
            {
                totalRowHeight = GetRowHeight(null, -1, RowType.StackedHeaderRow);
                for (int start = stackedHeaderCount - 1; start >= 0; start--)
                {
                    InitializeStackedHeaderForPage(rowDictionary, pageIndex, startColumnIndex, endColumnIndex, start);
                    if (start > 0)
                        totalRowHeight += GetRowHeight(null, -1, RowType.StackedHeaderRow);
                }
            }
            totalRowHeight += GetRowHeight(null, -1, RowType.HeaderRow);
            InitializeHeadersForPage(rowDictionary, pageIndex, columnNames, startColumnIndex, endColumnIndex);
            do
            {
                object record = null;
                var RowHeight = 0.0;
                if (PrintScaleOption == PrintScaleOptions.NoScaling || PrintScaleOption == PrintScaleOptions.FitAllColumnsonOnePage ||
                    (PrintScaleOption == PrintScaleOptions.FitAllRowsonOnePage
                    && pageIndex + 1 <= pagesPerRecord && recordIndex == GetTotalRecordCount()))
                {
                    if (totalRowHeight + PrintRowHeight > Math.Round(avaliableSize.Height, 2) || recordIndex == GetTotalRecordCount())
                    {
                        pageIndex++;
                        if (pagesPerRecord != 1 && pageIndex % pagesPerRecord != 1)
                            recordIndex = previoPageStartRecordIndex;

                        if (recordIndex > GetTotalRecordCount())
                            break;

                        previoPageStartRecordIndex = recordIndex;
                        if (recordIndex < source.Count)
                        {
                            //Need to reset the total row height for the next page.
                            totalRowHeight = 0.0;
                            rowDictionary = new List<RowInfo>();
                            pageDictionary.Add(pageIndex, rowDictionary);
                            ComputeColumnForPage(pageIndex, columnNames, avaliableSize, out startColumnIndex, out endColumnIndex);
                            if (canPrintStackedHeaders && stackedHeaderCount != 0)
                            {
                                totalRowHeight = GetRowHeight(null, -1, RowType.StackedHeaderRow);
                                for (int start = stackedHeaderCount - 1; start >= 0; start--)
                                {
                                    InitializeStackedHeaderForPage(rowDictionary, pageIndex, startColumnIndex, endColumnIndex, start);
                                    if (start > 0)
                                        totalRowHeight += GetRowHeight(null, -1, RowType.StackedHeaderRow);
                                }
                            }
                            totalRowHeight += GetRowHeight(null, -1, RowType.HeaderRow);
                            InitializeHeadersForPage(rowDictionary, pageIndex, columnNames, startColumnIndex, endColumnIndex);
                            record = GetRecordFromSourceList(recordIndex);
                            if (record is SummaryRecordEntry)
                                RowHeight = GetRowHeight(record, recordIndex, RowType.TableSummaryRow);
                            else if (record is Group)
                                RowHeight = GetRowHeight(record, recordIndex, RowType.CaptionRow);
                            else
                                RowHeight = GetRowHeight(record, recordIndex, RowType.DefaultRow);
                            if (!CanPrintStackedHeaders)
                                totalRowHeight = pageIndex > pagesPerRecord ? (AllowRepeatHeaders ? GetRowHeight(null, -1, RowType.HeaderRow) : 0) : GetRowHeight(null, -1, RowType.HeaderRow);
                            else
                                totalRowHeight = pageIndex > pagesPerRecord ? (AllowRepeatHeaders ? totalRowHeight : 0) : totalRowHeight;
                            if (AddParentRecordInfoToDict(rowDictionary, record, columnNames, ref totalRowHeight, startColumnIndex, endColumnIndex))
                                recordIndex++;
                        }
                    }
                }

                var cellRects = new List<CellInfo>();
                record = GetRecordFromSourceList(recordIndex);
                if (record == null)
                    continue;

                if (record is NestedRecordEntry)
                {
                    recordIndex++;
                    continue;
                }
                if (record is SummaryRecordEntry)
                    RowHeight = GetRowHeight(record, recordIndex, RowType.TableSummaryRow);
                else if (record is Group)
                    RowHeight = GetRowHeight(record, recordIndex, RowType.CaptionRow);
                else
                    RowHeight = GetRowHeight(record, recordIndex, RowType.DefaultRow);
                cellRects = AddRowInformationToDictionary(record, columnNames, totalRowHeight, startColumnIndex, endColumnIndex);
                var needTopBorder = !rowDictionary.Any();
                if (!needTopBorder)
                {
                    var previousRow = rowDictionary.LastOrDefault();
                    if (previousRow != null && previousRow.CellsInfo[0].CellRect.X > cellRects[0].CellRect.X)
                    {
                        needTopBorder = true;
                        previousRow.NeedBottomBorder = false;
                    }
                }

                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellRects,
                    RecordIndex = recordIndex,
                    NeedBottomBorder = true,
                    Record = record,
                    NeedTopBorder = needTopBorder,
                    RowType = RowType.DefaultRow
                });
                totalRowHeight += RowHeight;
                recordIndex++;
            } while ((recordIndex < GetTotalRecordCount()) || (endColumnIndex != columnNames.Count - 1));
            SetTotalRecordsHeight(totalRowHeight);
        }
       
        /// <summary>
        /// Calculates the list of specified column names can be arranged in a particular page index with in the available size of page.
        /// </summary>
        /// <param name="pageIndex">
        /// The corresponding page index to arrange the number of column with in the page.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection to arrange the column with in the specified page index.
        /// </param>
        /// <param name="availableSize">
        /// The available size of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to calculate the column for page.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to calculate the column for page.
        /// </param>
        /// <remarks>
        /// Override this method and customize the column calculation for page during printing process.
        /// </remarks>
        protected virtual void ComputeColumnForPage(int pageIndex, List<string> columnNames, Size avaliableSize, out int startColumnIndex, out int endColumnIndex)
        {
            startColumnIndex = 0;
            endColumnIndex = columnNames.Count - 1;
            if (pagesPerRecord == 1)
                return;
            if (pageDictionary.Keys.Contains(pageIndex - 1))
            {
                //if AllowRepeatHeaders is false  and pagesper record is more than one  then
                //need to get  the previous page record for column start,end calculation to avoid improper calculation of columns start ,end value.
                var index = !CanPrintStackedHeaders ? 0 : GetStackedHeaders();
                var rowdetail = AllowRepeatHeaders ? pageDictionary[pageIndex - 1][index] : (pageIndex - 1 <= pagesPerRecord ? pageDictionary[pageIndex - 1][index] : pageDictionary[pageIndex - 1].Skip(groupCount).FirstOrDefault());
                if (rowdetail != null)
                {
                    var lastColumn = rowdetail.CellsInfo.LastOrDefault();
                    if (lastColumn == null)
                    {
                        startColumnIndex = -1;
                        endColumnIndex = -1;
                        return;
                    }
                    var lastColumnofPreviousPage = lastColumn.ColumnName;
                    var lastColIndex = columnNames.IndexOf(lastColumnofPreviousPage);
                    startColumnIndex = lastColIndex >= columnNames.Count - 1 ? 0 : lastColIndex + 1;
                }
            }

            endColumnIndex = columnNames.Count() - 1;
            double columnWidths = groupCount * IndentColumnWidth;
            int columnsCount = columnNames.Count();
            for (var colIndex = startColumnIndex; colIndex < columnsCount; colIndex++)
            {
                columnWidths += GetColumnWidth(columnNames[colIndex]);
                if (Math.Round(columnWidths, 2) > Math.Round(avaliableSize.Width, 2))
                {
                    endColumnIndex = colIndex - 1;
                    break;
                }
                else if (colIndex == columnsCount - 1)
                {
                    endColumnIndex = colIndex;
                    break;
                }
            }
        }

        /// <summary>
        /// Initializes the header of specified list of column name to the particular page corresponding to the start and end of the column index.
        /// </summary>
        /// <param name="rowDictionary">
        /// The rowDictionary to add the header cell info in page.
        /// </param>
        /// <param name="pageIndex">
        /// The corresponding index of page to initialize headers.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection to initialize its headers for the specified page index.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to initialize its headers on particular page.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to initialize its headers on particular page.
        /// </param>
        protected void InitializeHeadersForPage(List<RowInfo> rowDictionary, int pageIndex, List<string> columnNames, int startColumnIndex, int endColumnIndex)
        {
            List<CellInfo> cellRects = null;
            if (pageIndex > pagesPerRecord)
            {
                if (AllowRepeatHeaders)
                    cellRects = AddHeaderInfoToDict(columnNames, 0, startColumnIndex, endColumnIndex);
            }
            else
                cellRects = AddHeaderInfoToDict(columnNames, 0, startColumnIndex, endColumnIndex);

            if (cellRects != null)
            {
                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellRects,
                    RecordIndex = -1,
                    NeedTopBorder = true,
                    NeedBottomBorder = true,
                    RowType=RowType.HeaderRow
                });
            }

        }
        /// <summary>
        /// Initializes the header of specified list of column name to the particular page corresponding to the start and end of the column index.
        /// </summary>
        /// <param name="rowDictionary">
        /// The rowDictionary to add the header cell info in page.
        /// </param>
        /// <param name="pageIndex">
        /// The corresponding index of page to initialize headers.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection to initialize its headers for the specified page index.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to initialize its headers on particular page.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to initialize its headers on particular page.
        /// </param>
        /// <param name="start">
        /// The  index for the Stacked Header.
        /// </param>
        protected virtual void InitializeStackedHeaderForPage(List<RowInfo> rowDictionary, int pageIndex, int startColumnIndex, int endColumnIndex, int start)
        {
           
        }
 
        /// <summary>
        /// Adds the row information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record information of row is added to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of columns arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add row information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add row information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of cell info that is going to be printed.
        /// </returns>
        /// <remarks>
        /// Invoked to add rows such as normal row, Unbound row, group caption summary row, summary row, table summary row to dictionary for printing.
        /// </remarks>
        protected virtual List<CellInfo> AddRowInformationToDictionary(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            var cellRects = new List<CellInfo>();
            if (record is Group)
                cellRects = AddCaptionSummaryInfoToDict(record as Group, columnNames, yPos, startColumnIndex, endColumnIndex);
            else if (record is SummaryRecordEntry)
            {
                if ((record as SummaryRecordEntry).SummaryRow is GridTableSummaryRow)
                    cellRects = AddTableSummaryInfoToDict(record as SummaryRecordEntry, columnNames, yPos, startColumnIndex, endColumnIndex);
                else
                    cellRects = AddSummaryInfoToDict(record as SummaryRecordEntry, columnNames, yPos, startColumnIndex, endColumnIndex);
            }
            else
                cellRects = AddRecordInfoToDict(record is RecordEntry ? record as RecordEntry : record, columnNames, yPos, startColumnIndex, endColumnIndex);
            return cellRects;
        }

        /// <summary>
        /// Adds the header information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="columnsNames">
        /// The list of column names collection that is used to measures the number of header arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add header information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add header information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of header cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddHeaderInfoToDict(List<string> columnsNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            var cellRects = new List<CellInfo>();
            double columnWidths = 0;
            var RowHeight = GetRowHeight(null, -1,RowType.HeaderRow);
            for (var start = startColumnIndex; start <= endColumnIndex; start++)
            {
                var columnName = columnsNames[start];

                var width = GetColumnWidth(columnName);
                if (start == startColumnIndex)
                    width += groupCount * IndentColumnWidth;
                width = width < 0 ? 0 : width;
                cellRects.Add(new CellInfo
                {
                    CellRect = new Rect(columnWidths, yPos, width, RowHeight),
                    ColumnName = columnName
                });

                columnWidths += width;
            }

            return cellRects;
        }
        /// <summary>
        /// Adds the header information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="yPos">
        /// The y position for the Stacked Header in a page.
        /// </param>9
        /// <param name="startColumnIndex">
        /// The start index of the column to add header information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add header information for printing.
        /// </param>
        /// <param name="start">
        /// The index of the Stacked Header.
        /// <returns>
        /// Returns the list of Stacked Header cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddStackedHeaderInfotoDict(double yPos, int startColumnIndex, int endColumnIndex,int start)
        {
            return null;
        }

        /// <summary>
        /// Adds the summary information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="record">
        /// The corresponding record of summary information is added in to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add summary information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add summary information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of summary cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddSummaryInfoToDict(object record, List<string> columnNames,
                double yPos, int startColumnIndex, int endColumnIndex)
        {
            return GetSummaryRowCellsInfoList(record, columnNames, yPos, startColumnIndex, endColumnIndex);
        }

        /// <summary>
        /// Adds the table summary information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="record">
        /// The corresponding record of table summary information is added in to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add table summary information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add table summary information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of table summary cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddTableSummaryInfoToDict(object record, List<string> columnNames,
                double yPos, int startColumnIndex, int endColumnIndex)
        {
            return GetSummaryRowCellsInfoList(record,columnNames,yPos,startColumnIndex,endColumnIndex);
        }

        private List<CellInfo> GetSummaryRowCellsInfoList(object record, List<string> columnNames,
                double yPos, int startColumnIndex, int endColumnIndex)
        {
            List<CellInfo> cellRects = new List<CellInfo>();
            if (record == null)
                return cellRects;

            var summaryRecord = record as SummaryRecordEntry;
            var rowIndex = source.IndexOf(summaryRecord);
            var RowHeight = GetRowHeight(summaryRecord, rowIndex, RowType.TableSummaryRow);
            var xPos = summaryRecord.Level >= 0 ? summaryRecord.Level * IndentColumnWidth : 0;
            if (summaryRecord.SummaryRow.ShowSummaryInRow)
            {
                var columnWidths = groupCount * IndentColumnWidth;
                for (var startIndex = startColumnIndex; startIndex <= endColumnIndex; startIndex++)
                    columnWidths += GetColumnWidth(columnNames[startIndex]);
                cellRects.Add(new CellInfo
                {
                    CellRect = new Rect(xPos, yPos, columnWidths - xPos, RowHeight),
                    ColumnName = string.Empty
                });
            }
            else
            {
                for (var startindex = startColumnIndex; startindex <= endColumnIndex; startindex++)
                {
                    var name = columnNames[startindex];
                    var width = GetColumnWidth(name);
                    if (startindex == startColumnIndex)
                        width += ((groupCount * IndentColumnWidth) - xPos);
                    cellRects.Add(new CellInfo
                    {
                        CellRect = new Rect(xPos, yPos, width, RowHeight),
                        ColumnName = name
                    });
                    xPos += width;
                }
            }

            return cellRects;
        }

        /// <summary>
        /// Adds the caption summary information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="record">
        /// The corresponding record of caption summary information is added in to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add caption summary information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add caption summary information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of caption summary cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddCaptionSummaryInfoToDict(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            var cellRects = new List<CellInfo>();
            if (record == null)
                return cellRects;

            var rowIndex = source.IndexOf(record);
            var RowHeight = GetRowHeight(record,rowIndex,RowType.CaptionRow);
            var group = record as Group;
            var groupDescription = View.GroupDescriptions[group.Level - 1] as ColumnGroupDescription;
            var xPos = group.Level > 1 ? ((group.Level - 1) * IndentColumnWidth) : 0;
            if (groupDescription != null)
            {
                var groupName =
                    groupDescription.PropertyName;
                if (IsCaptionSummaryInRow)
                {
                    double columnWidths = groupCount * IndentColumnWidth;
                    for (int startIndex = startColumnIndex; startIndex <= endColumnIndex; startIndex++)
                        columnWidths += GetColumnWidth(columnNames[startIndex]);

                    cellRects.Add(new CellInfo
                    {
                        CellRect = new Rect(xPos, yPos, columnWidths - xPos, RowHeight),
                        ColumnName = groupName,
                    });
                }
                else
                {
                    if (group.SummaryDetails.SummaryRow != null)
                    {
                        for (var startindex = startColumnIndex; startindex <= endColumnIndex; startindex++)
                        {
                            var name = columnNames[startindex];
                            var width = GetColumnWidth(name);
                            if (startindex == startColumnIndex)
                                width += ((groupCount * IndentColumnWidth) - xPos);
                            cellRects.Add(new CellInfo
                            {
                                CellRect = new Rect(xPos, yPos, width, RowHeight),
                                ColumnName = name
                            });
                            xPos += width;
                        }
                    }
                }
            }

            return cellRects;
        }

        /// <summary>
        /// Adds the record information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="record">
        /// The corresponding record information that is added in to dictionary for printing.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add record information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add record information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of record cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddRecordInfoToDict(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            List<CellInfo> cellRects = new List<CellInfo>();
            if (record == null)
                return cellRects;

            var rowIndex = source.IndexOf(record);
            var RowHeight = GetRowHeight(record, rowIndex,RowType.DefaultRow);
            var xPos = record is RecordEntry && (record as RecordEntry).Level >= 0 
                ? (record as RecordEntry).Level * IndentColumnWidth
                : 0;
            for (int startIndex = startColumnIndex; startIndex <= endColumnIndex; startIndex++)
            {
                var columnName = columnNames[startIndex];
                var width = GetColumnWidth(columnName);

                cellRects.Add(new CellInfo
                {
                    CellRect = new Rect(xPos, yPos, width, RowHeight),
                    ColumnName = columnName
                });
                xPos += width;
            }

            return cellRects;
        }
      
        /// <summary>
        /// Adds the parent record information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="rowDictionary">
        /// The rowDictionary to add the parent record information in page for printing.
        /// </param>
        /// <param name="record">
        /// The corresponding record information of parent that is added in to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add the parent record information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add the parent record information for printing.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the parent record information is added in page; otherwise, <b>false</b>.
        /// </returns>
        protected bool AddParentRecordInfoToDict(List<RowInfo> rowDictionary, object record,
                 List<string> columnNames, ref double yPos, int startColumnIndex, int endColumnIndex)
        {
            var needTopBorder = true;
            var cellInfos = new List<CellInfo>();
            var rowIndex = source.IndexOf(record);
            var RowHeight = 0.0;
            if (record is SummaryRecordEntry)
                RowHeight = GetRowHeight(record, rowIndex, RowType.TableSummaryRow);
            else if (record is Group)
                RowHeight = GetRowHeight(record, rowIndex, RowType.CaptionRow);
            else
                RowHeight = GetRowHeight(record, rowIndex, RowType.DefaultRow);
            if (record is Group)
            {
                var group = record as Group;
                if (group.Level > 1)
                    AddParentRecordInfoToDict(rowDictionary, group.Parent, columnNames, ref yPos,
                        startColumnIndex, endColumnIndex);

                cellInfos = AddCaptionSummaryInfoToDict(record, columnNames, yPos, startColumnIndex, endColumnIndex);
                needTopBorder = !rowDictionary.Any();
                if (!needTopBorder)
                {
                    var previousRow = rowDictionary.LastOrDefault();
                    if (previousRow != null && previousRow.CellsInfo[0].CellRect.X > cellInfos[0].CellRect.X)
                    {
                        needTopBorder = true;
                        previousRow.NeedBottomBorder = false;
                    }
                }
                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellInfos,
                    RecordIndex = source.IndexOf(record),
                    NeedBottomBorder = true,
                    Record = group,
                    NeedTopBorder = needTopBorder,
                    RowType=RowType.DefaultRow
                });

                yPos += RowHeight;
                return true;
            }
            else if (record is RecordEntry)
            {
                var recordEntry = (record as RecordEntry);
                AddParentRecordInfoToDict(rowDictionary, recordEntry.Parent, columnNames, ref yPos,
                    startColumnIndex, endColumnIndex);

                cellInfos = AddRecordInfoToDict(record, columnNames, yPos, startColumnIndex, endColumnIndex);
                needTopBorder = !rowDictionary.Any();
                if (!needTopBorder)
                {
                    var previousRow = rowDictionary.LastOrDefault();
                    if (previousRow != null && previousRow.CellsInfo[0].CellRect.X > cellInfos[0].CellRect.X)
                    {
                        needTopBorder = true;
                        previousRow.NeedBottomBorder = false;
                    }
                }
                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellInfos,
                    RecordIndex = source.IndexOf(record),
                    NeedBottomBorder = true,
                    Record = recordEntry,
                    NeedTopBorder = needTopBorder,
                    RowType= RowType.DefaultRow
                });
                yPos += RowHeight;
                return true;
            }
            else if ((record is SummaryRecordEntry))
            {
                var recordEntry = (record as SummaryRecordEntry);
                AddParentRecordInfoToDict(rowDictionary, recordEntry.Parent, columnNames, ref yPos,
                    startColumnIndex, endColumnIndex);
                cellInfos = AddSummaryInfoToDict(record, columnNames, yPos, startColumnIndex, endColumnIndex);
                needTopBorder = !rowDictionary.Any();
                if (!needTopBorder)
                {
                    var previousRow = rowDictionary.LastOrDefault();
                    if (previousRow != null && previousRow.CellsInfo[0].CellRect.X > cellInfos[0].CellRect.X)
                    {
                        needTopBorder = true;
                        previousRow.NeedBottomBorder = false;
                    }
                }
                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellInfos,
                    RecordIndex = source.IndexOf(record),
                    NeedBottomBorder = true,
                    Record = recordEntry,
                    NeedTopBorder = needTopBorder
                });
                yPos += RowHeight;
                return true;
            }

            return false;
        }

        #endregion Inital Page Computation for PageDictionary

        #region Add Page Child Elements to PrintPagePanel
       
        /// <summary>
        /// Adds the row info to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the row that is to be printed.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row information that is added to panel.
        /// </param>
        /// <param name="pageIndex">
        /// The corresponding index of page to add the row to print page panel.
        /// </param>
        /// <remarks>
        /// Invoked to add Rows such as Header row,summary(group summary, table summary),Unboundrow, etc...  to PrintPagePanel.
        /// </remarks>
        protected virtual void AddRowToPrintPagePanel(PrintPagePanel panel, RowInfo rowInfo, int pageIndex)
        {
            if (rowInfo.RowType ==RowType.HeaderRow)
                AddHeaderRowToPanel(panel, rowInfo);
            else if (rowInfo.RowType ==RowType.StackedHeaderRow)
                AddStackedHeaderRowToPanel(panel, rowInfo);
            else
            {
                if (rowInfo.Record is Group)
                    AddCaptionSummaryRowToPanel(panel, rowInfo);
                else if ((rowInfo.Record is SummaryRecordEntry))
                {
                    if ((rowInfo.Record as SummaryRecordEntry).SummaryRow is GridTableSummaryRow)
                        AddTableSummaryRowToPanel(panel, rowInfo);
                    else
                        AddSummaryRowToPanel(panel, rowInfo);
                }
                else
                    AddDataRowToPanel(panel, rowInfo);
            }
        }

        /// <summary>
        /// Adds the header row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the header row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding header row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddHeaderRowToPanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var i = 0;
            foreach (var cellInfo in rowInfo.CellsInfo)
            {
                //WPF-20369 Basic style support in printing
                var cell = GetPrintHeaderCell(cellInfo.ColumnName);
                //The Header cell height may varied each other while printing the Stacked Header So we have to consider the height of each cell for adding to the panel.
                cell.Height = cellInfo.CellRect.Height;
                cell.Width = cellInfo.CellRect.Width;
                cell.BorderThickness = i == 0 ?
                    new Thickness(1, topThickNess, 1, bottomThickness) :
                    new Thickness(0, topThickNess, 1, bottomThickness);

                cell.Content = GetColumnHeaderElement(cellInfo.ColumnName);
                cellInfo.Element = cell;
                panel.Children.Add(cell);
                i++;
            }
        }

        /// <summary>
        /// Adds the header row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the header row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding header row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddStackedHeaderRowToPanel(PrintPagePanel panel, RowInfo rowinfo)
        {
            var topThickNess = rowinfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowinfo.NeedBottomBorder ? 1 : 0;
            var i = 0;

            foreach (var cellInfo in rowinfo.CellsInfo)
            {
                var cell = GetPrintHeaderCell(cellInfo.ColumnName);
                cell.Height = cellInfo.CellRect.Height;
                cell.Width = cellInfo.CellRect.Width;
                cell.BorderThickness = i == 0 ?
                    new Thickness(rowinfo.CellsInfo[0].CellRect.Left == 0 ? 1 : 0, topThickNess, 1, bottomThickness) :
                    new Thickness(0, topThickNess, 1, bottomThickness);

                cell.Content = GetStackedColumnHeaderElement(cellInfo.ColumnName);
                cellInfo.Element = cell;
                panel.Children.Add(cell);
                i++;
            }
        }

        /// <summary>
        /// Adds the caption summary row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the caption summary row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding caption summary row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddCaptionSummaryRowToPanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var cellsInfo = rowInfo.CellsInfo;
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var group = rowInfo.Record as Group;
            for (var start = 0; start < cellsInfo.Count; start++)
            {
                var cellInfo = cellsInfo[start];
                if (IsCaptionSummaryInRow)
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintCaptionSummaryCell(group, cellInfo.ColumnName);
                    cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness = new Thickness(1, topThickNess, 1, bottomThickness);
                    cell.Content = new TextBlock
                    {
                        Text =
                            View.TopLevelGroup.GetGroupCaptionText(group,
                                GetGroupCaptionStringFormat
                                    (), cellInfo.ColumnName),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    cellInfo.Element = cell;
                    panel.Children.Add(cell);
                }
                else
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintCaptionSummaryCell(group, cellInfo.ColumnName);
                    cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness =
                        start == 0
                            ? new Thickness(1, topThickNess, 0, bottomThickness)
                            : start == (cellsInfo.Count - 1)
                                ? new Thickness(0, topThickNess, 1, bottomThickness)
                                : new Thickness(0, topThickNess, 0, bottomThickness);
                    var summaryColumns = group.SummaryDetails.SummaryRow.SummaryColumns;
                    if (summaryColumns != null && summaryColumns.Any())
                    {
                        if (summaryColumns.Any(x => x.MappingName == cellInfo.ColumnName))
                        {
                            cell.Content = new TextBlock
                            {
                                Text =
                                    SummaryCreator.GetSummaryDisplayText(group.SummaryDetails,
                                        cellInfo.ColumnName,
                                        View),
                                Padding = new Thickness(2, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                        }
                        cellInfo.Element = cell;
                        panel.Children.Add(cell);
                    }
                }
            }
        }

        /// <summary>
        /// Adds the summary row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the summary row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding summary row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddSummaryRowToPanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var cellsInfo = rowInfo.CellsInfo;
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var summaryRecord = rowInfo.Record as SummaryRecordEntry;
            for (var start = 0; start < cellsInfo.Count; start++)
            {
                var cellInfo = cellsInfo[start];
                if (summaryRecord.SummaryRow.ShowSummaryInRow)
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintGroupSummaryCell(summaryRecord, cellInfo.ColumnName);
                    cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness = new Thickness(1, topThickNess, 1, bottomThickness);
                    cell.Content = new TextBlock
                    {
                        Text =
                            SummaryCreator.GetSummaryDisplayTextForRow(summaryRecord, View),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    cellInfo.Element = cell;
                    panel.Children.Add(cell);
                }
                else
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintGroupSummaryCell(summaryRecord, cellInfo.ColumnName);
                    //WPF-23379 Based on FlowDirection we already  changed the columnNames list in Computepages method. so no need to set here.
                    //cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness =
                        start == 0
                            ? new Thickness(1, topThickNess, 0, bottomThickness)
                            : start == (cellsInfo.Count - 1)
                                ? new Thickness(0, topThickNess, 1, bottomThickness)
                                : new Thickness(0, topThickNess, 0, bottomThickness);
                    var summaryColumns = summaryRecord.SummaryRow.SummaryColumns;
                    if (summaryColumns != null && summaryColumns.Any())
                    {
                        if (summaryColumns.Any(x => x.MappingName == cellInfo.ColumnName))
                        {
                            cell.Content = new TextBlock
                            {
                                Text =
                                    SummaryCreator.GetSummaryDisplayText(summaryRecord, cellInfo.ColumnName,
                                        View),
                                Padding = new Thickness(2, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                        }
                    }
                    cellInfo.Element = cell;
                    panel.Children.Add(cell);
                }
            }
        }

        /// <summary>
        /// Adds the table summary row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the table summary row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding table summary row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddTableSummaryRowToPanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var cellsInfo = rowInfo.CellsInfo;
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var summaryRecord = rowInfo.Record as SummaryRecordEntry;
            for (var start = 0; start < cellsInfo.Count; start++)
            {
                var cellInfo = cellsInfo[start];
                if (summaryRecord.SummaryRow.ShowSummaryInRow)
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintTableSummaryCell(summaryRecord, cellInfo.ColumnName);
                    cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness = new Thickness(1, topThickNess, 1, bottomThickness);
                    cell.Content = new TextBlock
                    {
                        Text =
                            SummaryCreator.GetSummaryDisplayTextForRow(summaryRecord, View),
                        HorizontalAlignment = HorizontalAlignment.Left,
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    cellInfo.Element = cell;
                    panel.Children.Add(cell);
                }
                else
                {
                    //WPF-20369 Basic style support in printing
                    var cell = GetPrintTableSummaryCell(summaryRecord, cellInfo.ColumnName);
                    //WPF-23379 Based on FlowDirection we already  changed the columnNames list in Computepages method. so no need to set here.
                    //cell.FlowDirection = PrintFlowDirection;
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Padding = new Thickness(2, 0, 0, 0);
                    cell.BorderThickness =
                          start == 0
                              ? new Thickness(1, topThickNess, 0, bottomThickness)
                              : start == (cellsInfo.Count - 1)
                                  ? new Thickness(0, topThickNess, 1, bottomThickness)
                                  : new Thickness(0, topThickNess, 0, bottomThickness);

                    var summaryColumns = summaryRecord.SummaryRow.SummaryColumns;
                    if (summaryColumns != null && summaryColumns.Any())
                    {
                        if (summaryColumns.Any(x => x.MappingName == cellInfo.ColumnName))
                        {
                            cell.Content = new TextBlock
                            {
                                Text =
                                    SummaryCreator.GetSummaryDisplayText(summaryRecord, cellInfo.ColumnName,
                                        View),
                                Padding = new Thickness(2, 0, 0, 0),
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Center
                            };
                        }
                    }
                    cellInfo.Element = cell;
                    panel.Children.Add(cell);
                }
            }
        }

        /// <summary>
        /// Adds the data row to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the data row for printing.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding data row information that is added to the panel for printing.
        /// </param>
        protected virtual void AddDataRowToPanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var record = (rowInfo.Record is RecordEntry) ? (rowInfo.Record as RecordEntry).Data : rowInfo.Record;
            var i = 0;
            foreach (var cellInfo in rowInfo.CellsInfo)
            {
                //WPF-20369 Basic style support in printing
                var cell = GetPrintGridCell(record, cellInfo.ColumnName);
                cell.Width = cellInfo.CellRect.Width;
                cell.Height = cellInfo.CellRect.Height;
                cell.BorderThickness = i == 0 ?
                    new Thickness(1, topThickNess, 1, bottomThickness) :
                    new Thickness(0, topThickNess, 1, bottomThickness);

                var content = GetColumnElement(record, cellInfo.ColumnName);
                if (content is DataTemplate)
                {
                    cell.ContentTemplate = content as DataTemplate;
                    SetDataTemplateContentToPrintGridCell(cell, cellInfo, record);
                }
                else
                    cell.Content = content;
                cellInfo.Element = cell;
                panel.Children.Add(cell);
                i++;
            }
        }
        
        /// <summary>
        /// Returns the grid cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print grid cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print grid cell.
        /// </param>
        /// <returns>
        /// Returns the GridCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintGridCell.
        /// </returns>
        public virtual ContentControl GetPrintGridCell(object record, string mappingName)
        {
            return new PrintGridCell();
        }

        /// <summary>
        /// Returns the caption summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print caption summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print caption summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridGridCaptionSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintCaptionSummaryCell.
        /// </returns>
        public virtual ContentControl GetPrintCaptionSummaryCell(object record, string mappingName)
        {
            return new PrintCaptionSummaryCell();
        }

        /// <summary>
        /// Returns the group summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print group summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print group summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridGroupSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintGroupSummaryCell.
        /// </returns>
        public virtual ContentControl GetPrintGroupSummaryCell(object record, string mappingName)
        {
            return new PrintGroupSummaryCell();
        }

        /// <summary>
        /// Returns the header cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print header cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print header cell.
        /// </param>
        /// <returns>
        /// Returns the GridHeaderCellControl if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintHeaderCell.
        /// </returns>
        public virtual ContentControl GetPrintHeaderCell(string mappingName)
        {
            return new PrintHeaderCell();
        }

        /// <summary>
        /// Returns the table summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print table summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print table summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridTableSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintTableSummaryCell.
        /// </returns>
        public virtual ContentControl GetPrintTableSummaryCell(object record, string mappingName)
        {
            return new PrintTableSummaryCell();
        }

        /// <summary>
        /// Returns the UnboundRow cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print UnboundRow cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print UnboundRow cell.
        /// </param>
        /// <returns>
        /// Returns the GridUnboundRowCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintUnboundRowCell.
        /// </returns>
        public virtual ContentControl GetPrintUnboundRowCell(object record, string mappingName)
        {
            return new PrintUnboundRowCell();
        }

        #endregion Add Page Child Elements to PrintPagePanel

        #region Other Protected  methods

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.Data.SummaryRecordEntry"/> to get the table summary based on its position for printing.
        /// </summary>
        /// <param name="position">
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowPosition"/> to get the table summary list.
        /// </param>
        /// <returns>
        /// Returns the list of <see cref="Syncfusion.Data.SummaryRecordEntry"/>.
        /// </returns>
        protected IList<SummaryRecordEntry> GetTableSummaryList(TableSummaryRowPosition position)
        {
            //WPF-20145 list the tablesummary position top and bottom seperatly for add to page.
            if (position == TableSummaryRowPosition.Top)
            {
                return View.Records.TableSummaries.Where(x =>
                {
                    var summary = x as SummaryRecordEntry;
                    var summaryrow = x.SummaryRow as GridTableSummaryRow;
                    if (summaryrow != null && summaryrow.Position == TableSummaryRowPosition.Top)
                        return true;
                    return false;
                }).ToList();
            }
            else
            {
                //UWP-729 (issue 5)
                return View.Records.TableSummaries.Where(x =>
                {
                    var summary = x as SummaryRecordEntry;
                    var summaryrow = x.SummaryRow as GridTableSummaryRow;
                    if (summaryrow != null && summaryrow.Position == TableSummaryRowPosition.Bottom)
                        return true;
                    else if (summaryrow == null)//Return true, when adding GridSummaryRow.
                        return true;
                    return false;
                }).ToList();
            }
        }

        /// <summary>
        /// Sets the height of all total records in SfDataGrid for printing.
        /// </summary>
        /// <param name="totalheight">
        /// The total height of all records in SfDataGrid.
        /// </param>
        protected void SetTotalRecordsHeight(double totalheight)
        {
            totalRecordsHeight = totalheight;
        }

        /// <summary>
        /// Gets the height of total records in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the height of total records in SfDataGrid.
        /// </returns>
        protected double GetTotalRecordsHeight()
        {
            return totalRecordsHeight;
        }

        #endregion Other Protected  methods

#if WPF

        #region Drawing Methods

        //Calculate Contentrect from Cell rect value
        private Rect AddBorderMargins(Rect cellRect, Thickness thickness)
        {
            if (cellRect.IsEmpty || cellRect.Width <= thickness.Left + thickness.Right || cellRect.Height <= thickness.Top + thickness.Bottom)
                return Rect.Empty;
            cellRect.X += thickness.Left;
            cellRect.Y += thickness.Top;
            cellRect.Width -= thickness.Left + thickness.Right;

            return cellRect;
        }

        /// <summary>
        /// Returns the formatted text for the specified cell information.
        /// </summary>
        /// <param name="rowInfo">
        /// The corresponding row information to get formatted text.
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cell information to get formatted text.
        /// </param>
        /// <param name="cellValue">
        /// The corresponding cell value to get formatted text.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Media.FormattedText"/> for the specified cell information.
        /// </returns>
        protected virtual FormattedText GetFormattedText(RowInfo rowInfo, CellInfo cellInfo, string cellValue)
        {
            var formattedtext = new FormattedText(cellValue, CultureInfo.CurrentCulture, PrintFlowDirection, this.GetTypeface(), 12.0, Brushes.Black);
            return formattedtext;
        }
        
        /// <summary>
        /// Draws the content of the specified drawingContext and list of row infos to page panel element. 
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw.
        /// </param>
        /// <param name="rowsInfoList">
        /// The list of row infos to draw. 
        /// </param>
        public void OnRender(DrawingContext drawingContext, List<RowInfo> rowsInfoList)
        {
            for (int n = 0; n < rowsInfoList.Count; n++)
            {
                OnRenderRow(drawingContext, rowsInfoList[n]);
            }
        }

        /// <summary>
        /// Draws the content of each row for the specified drawingContext and list of row infos to page panel element. 
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw the row info.
        /// </param>
        /// <param name="rowsInfoList">
        /// The list of row infos to draw the row info. 
        /// </param>
        protected virtual void OnRenderRow(DrawingContext drawingContext, RowInfo rowInfoList)
        {
            var cellsInfo = rowInfoList.CellsInfo;
            //when we draw the cell for PrintStackedHeader=True we can't able to print it by normal way because the Stacked Header size is varied from one to another.
            if ((rowInfoList.RowType == RowType.StackedHeaderRow || rowInfoList.RowType == RowType.HeaderRow) && CanPrintStackedHeaders)
            {
                for (int i = 0; i < cellsInfo.Count; i++)
                {
                    OnRenderCell(drawingContext, rowInfoList, cellsInfo[i]);
                    //Last Cell Right Border
                    if (cellsInfo[i].Equals(cellsInfo[cellsInfo.Count - 1]))
                        drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[i].CellRect.TopRight, cellsInfo[i].CellRect.BottomRight);   //Last cell Right border
                    if (rowInfoList.NeedTopBorder)
                        //Top border of the page
                        drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[i].CellRect.TopLeft, cellsInfo[i].CellRect.TopRight);
                    if (rowInfoList.NeedBottomBorder)
                        if (rowInfoList.CellsInfo[0].CellRect.Height > 0)
                            //Row Bottom Border
                            drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[i].CellRect.BottomLeft, cellsInfo[i].CellRect.BottomRight);
                }
            }
            else
            {
                foreach (var cellInfo in cellsInfo)
                {
                    OnRenderCell(drawingContext, rowInfoList, cellInfo);
                    //Last Cell Right Border
                    if (cellInfo.Equals(cellsInfo[cellsInfo.Count - 1]))
                        drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellInfo.CellRect.TopRight, cellsInfo[cellsInfo.Count - 1].CellRect.BottomRight);   //Last cell Right border
                }
                if (rowInfoList.NeedTopBorder)
                    //Top border of the page
                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[0].CellRect.TopLeft, cellsInfo[cellsInfo.Count - 1].CellRect.TopRight);
                if (rowInfoList.NeedBottomBorder)
                    if (rowInfoList.CellsInfo[0].CellRect.Height > 0)
                        //Row Bottom Border
                        drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[0].CellRect.BottomLeft, cellsInfo[cellsInfo.Count - 1].CellRect.BottomRight);
            }

        }

        /// <summary>
        /// Draws the content of each cell value and its border for the specified drawingContext, row info and cell info to page panel element. 
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw each cell content.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row info to draw each cell content. 
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cell info to draw each cell content. 
        /// </param>
        protected virtual void OnRenderCell(DrawingContext drawingContext, RowInfo rowInfo, CellInfo cellInfo)
        {
            var record = rowInfo.Record;
            var column = cellInfo.ColumnName;
            var cellvalue = string.Empty;
            var needleftborder = true;
            //canDrawCellValue - Based on this flag, we can decide to print the cell value or not. Because for image and checkbox columns, we implemented separate way to draw the cell values.
            bool canDrawCellValue = true;
            if (rowInfo.RowType==RowType.HeaderRow)
            {
                cellvalue = GetColumnHeaderText(cellInfo.ColumnName);
            }
            else
            {
                dynamic value = null;
                if (column != "" && Provider != null)
                    value = Provider.GetFormattedValue(((record is RecordEntry) ? (record as RecordEntry).Data : record), cellInfo.ColumnName);

                cellvalue = value != null ? value.ToString() : string.Empty;
            }

            if (canDrawCellValue)
                OnDrawText(drawingContext, rowInfo, cellInfo, cellvalue);
            //cell left border
            if (needleftborder)
                drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellInfo.CellRect.TopLeft, cellInfo.CellRect.BottomLeft);
        }
        
        /// <summary>
        /// Gets the <see cref="System.Windows.Media.System.Windows.Media.VisualBrush"/> to draw the CheckBox in GridCheckBoxColumn.
        /// </summary>
        /// <param name="isChecked">
        /// Determines the state of CheckBox.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.Media.System.Windows.Media.VisualBrush"/> to draw CheckBox.
        /// </returns>
        private VisualBrush GetCheckBoxVisualBrush(bool? isChecked)
        {
            VisualBrush visualBrush;
            CheckBox checkBox = new CheckBox();
            checkBox.BeginInit();
            checkBox.Content = null;
            checkBox.Width = 15;
            checkBox.Height = 15;
            checkBox.IsChecked = isChecked;
            checkBox.Background = Brushes.Transparent;
            if (PrintFlowDirection == System.Windows.FlowDirection.RightToLeft)
            {
                checkBox.FlowDirection = PrintFlowDirection;
                checkBox.LayoutTransform = new ScaleTransform(-1, 1);
            }
            checkBox.EndInit();
            checkBox.Measure(new Size(15, 15));
            checkBox.Arrange(new Rect(new Size(15, 15)));
            visualBrush = new VisualBrush();
            visualBrush.Visual = checkBox;
            return visualBrush;
        }

        /// <summary>
        /// Draws the CheckBox cell for the specified drawingContext, row info ,cell info and column to page panel element.  
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw the CheckBox cell.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding rowInfo to draw the CheckBox cell.
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cellInfo to draw the CheckBox cell.
        /// </param>
        /// <param name="Column">
        /// The corresponding column to draw the CheckBox cell.
        /// </param>
        protected virtual void OnDrawCheckBox(DrawingContext drawingContext, RowInfo rowInfo, CellInfo cellInfo, GridColumn Column)
        {
            var value = Provider != null ? Provider.GetFormattedValue(rowInfo.Record is RecordEntry? (rowInfo.Record as RecordEntry).Data:rowInfo.Record, cellInfo.ColumnName) : null;
            HorizontalAlignment horizontalAlignment = (Column as GridCheckBoxColumn).HorizontalAlignment;
            bool isThreestate = (Column as GridCheckBoxColumn).IsThreeState;
            VisualBrush checkboxStateBrush = (value == null && isThreestate) ? ThreeStateBrush : (Convert.ToBoolean(value) ? CheckedBrush : UnCheckedBrush);

            double thickness = cellInfo.CellRect.Height * 0.20;
            var elementRect = AddBorderMargins(cellInfo.CellRect, new Thickness(5, Math.Ceiling(thickness / 2), 5, Math.Ceiling(thickness / 2)));
            if (elementRect.Width > 0 && elementRect.Height > 0)
            {
                Point centerPoint = new Point(elementRect.X + (elementRect.Width / 2), elementRect.Y + (elementRect.Height / 2));
                if (horizontalAlignment == HorizontalAlignment.Center)
                {
                    elementRect.X = centerPoint.X - (13 / 2);
                }
                else if (horizontalAlignment == HorizontalAlignment.Right)
                {
                    elementRect.X += (elementRect.Width - 13);
                }
                elementRect.Y = centerPoint.Y - (10);
                elementRect.Width = 15;
                elementRect.Height = 15;
                drawingContext.DrawRectangle(checkboxStateBrush, null, elementRect);
            }
        }

        /// <summary>
        /// Draws the image cell for the specified drawing context, row info, cell info and image.
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawing context to draw the image cell.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row info to draw the image cell.
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cell info to draw the image cell.
        /// </param>
        /// <param name="image">
        /// The corresponding image to draw.
        /// </param>
        protected virtual void OnDrawImage(DrawingContext drawingContext, RowInfo rowInfo, CellInfo cellInfo, Image image)
        {
            // When iamge return as null from end user iamge will not have the null value. its properties like source, Uri will be having null value.
            if (image == null || image.Source == null)
                return;

            double thickness = cellInfo.CellRect.Height * 0.20;
            var contentrect = AddBorderMargins(cellInfo.CellRect, new Thickness(5, Math.Ceiling(thickness / 2), 5, Math.Ceiling(thickness / 2)));
            var textalignment = TextAlignment.Left;
            if (cellInfo.ColumnName != "" && !(rowInfo.Record is Group))
            {
                textalignment = GetColumnTextAlignment(cellInfo.ColumnName);
            }

            if (contentrect.Width > 0 && contentrect.Height > 0)
            {
                BitmapImage Image = null;
                if (image.Source is BitmapImage)
                    Image = image.Source as BitmapImage;
                else
                    //WPF-20144 avoid  URI format exception need to pass UriKind.RelativeOrAbsolute  while creation of Uri.
                    Image = new BitmapImage(new Uri(image.Source.ToString(), UriKind.RelativeOrAbsolute));
                //Modify the content rect value based on the text alignment
                if (textalignment == TextAlignment.Center && image.Stretch != Stretch.Fill)
                {
                    Point centerPoint = new Point(contentrect.X + (contentrect.Width / 2), contentrect.Y + (contentrect.Height / 2));
                    contentrect.X = centerPoint.X - (contentrect.Height / 2);
                    contentrect.Y = centerPoint.Y - (contentrect.Height / 2);
                }
                else if (textalignment == TextAlignment.Right && image.Stretch != Stretch.Fill)
                {
                    contentrect.X += contentrect.Width - (contentrect.Height);
                }

                // Modify the width and height of the image based on the Stretch type
                if (image.Stretch == Stretch.Uniform)
                {
                    contentrect.Width = contentrect.Height - 2;
                    contentrect.Height = contentrect.Height - (contentrect.Height * 0.3);
                }
                else if (image.Stretch == Stretch.Fill)
                {
                    contentrect.Width = contentrect.Width - 2;
                    contentrect.Height = contentrect.Height - 2;
                }
                else
                {
                    contentrect.Width = contentrect.Width <= image.Source.Width ? contentrect.Width - 2 : image.Source.Width;
                    contentrect.Height = contentrect.Height <= image.Source.Height ? contentrect.Height - 2 : image.Source.Height;
                }
                drawingContext.DrawImage(Image, contentrect);
            }
        }

        /// <summary>
        /// Draws the text for the specified drawing context, row info, cell info and cell value.
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawing context to draw the text.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row info to draw the text.
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cell info to draw the text.
        /// </param>
        /// <param name="cellValue">
        /// The corresponding cell value to draw the text.
        /// </param>
        protected virtual void OnDrawText(DrawingContext drawingContext, RowInfo rowInfo, CellInfo cellInfo, string cellValue)
        {
            if (cellValue == null)
                return;
            //WPF-29090 Condition to place the content with in the cell, when the row height is calculated based upon the content present in the record.
            double thickness = 0;
            //WPF-35141 Need to check the row type to avoid the auto row height condition is applied for the header region.
            if (rowInfo.RowType == RowType.HeaderRow || rowInfo.RowType == RowType.StackedHeaderRow)
                thickness = !(cellInfo.CellRect.Height > PrintHeaderRowHeight) ? cellInfo.CellRect.Height * 0.20 : 0;
            else
                thickness = !(cellInfo.CellRect.Height > PrintRowHeight) ? cellInfo.CellRect.Height * 0.20 : 0;
            var contentrect = AddBorderMargins(cellInfo.CellRect, new Thickness(5, Math.Ceiling(thickness / 2), 5, Math.Ceiling(thickness / 2)));
            var formattedText = GetFormattedText(rowInfo, cellInfo, cellValue);
            var textalignment = TextAlignment.Left;
            var textwrap = TextWrapping.NoWrap;
            //WPF-21386 By default  header column text alignment is center and text wrapping is wrap.
            if (rowInfo.RowType == RowType.HeaderRow || rowInfo.RowType == RowType.StackedHeaderRow)
            {
                textalignment = TextAlignment.Center;
                textwrap = TextWrapping.Wrap;
            }
            else if (cellInfo.ColumnName != "" && !(rowInfo.Record is Group))
            {
                textalignment = GetColumnTextAlignment(cellInfo.ColumnName);
                textwrap = GetColumnTextWrapping(cellInfo.ColumnName);
            }

            if (contentrect.Width > 0 && contentrect.Height > 0)
            {
                formattedText.MaxTextWidth = contentrect.Width;
                formattedText.MaxTextHeight = contentrect.Height;
                formattedText.TextAlignment = textalignment;
                if (textwrap != TextWrapping.NoWrap)
                    formattedText.Trimming = TextTrimming.None;
                else
                {
                    formattedText.Trimming = TextTrimming.CharacterEllipsis;
                    formattedText.MaxLineCount = 1;
                }

                 drawingContext.DrawText(formattedText, contentrect.TopLeft);
            }
        }

        private Typeface GetTypeface()
        {
            FontFamily fontFamily = new FontFamily("Segoe UI");
            FontStyle fontStyle = new FontStyle();
            FontWeight fontWeight = new FontWeight();
            FontStretch fontStretch = new FontStretch();

            return new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
        }

        #endregion Drawing Methods

#endif

#if WinRT && !UNIVERSAL
         /// <summary>
        /// Prints the content of SfDataGrid.
        /// </summary>
        public async void Print()
        {
           await PrintManager.ShowPrintUIAsync();
        }

        internal void RegisterForPrinting()
        {
            printDocument = new PrintDocument();
            printDocumentSource = printDocument.DocumentSource;
            printDocument.Paginate += OnCreatePrintPreviewPages;
            printDocument.GetPreviewPage += OnGetPrintPreviewPage;
            printDocument.AddPages += OnAddPrintPages;
            var printMan = PrintManager.GetForCurrentView();
            printMan.PrintTaskRequested += OnPrintTaskRequested;
        }

        async private void UnRegisterForPrinting()
        {
            printDocument.Paginate -= OnCreatePrintPreviewPages;
            printDocument.GetPreviewPage -= OnGetPrintPreviewPage;
            printDocument.AddPages -= OnAddPrintPages;
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                  var printMan = PrintManager.GetForCurrentView();
                  printMan.PrintTaskRequested -= OnPrintTaskRequested;
                  printTask.Completed-=printTask_Completed;
           });
        }

        protected virtual void OnCreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            
        }

        private void OnGetPrintPreviewPage(object sender, GetPreviewPageEventArgs e)
        {
            var printpageControl = printControls.FirstOrDefault(x => x.PageIndex == e.PageNumber);
            if (printpageControl == null)
            {
                printpageControl = CreatePage(e.PageNumber);
                printControls.Add(printpageControl);
            }

            printDocument.SetPreviewPage(e.PageNumber, printpageControl);
        }

        private void OnAddPrintPages(object sender, AddPagesEventArgs e)
        {
            for (var i = 1; i <= pageCount; i++)
            {
                var printpageControl = printControls.FirstOrDefault(x => x.PageIndex == i) ?? CreatePage(i);
                printDocument.AddPage(printpageControl);
            }
            printDocument.AddPagesComplete();
        }

        internal virtual void OnPrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            printTask = args.Request.CreatePrintTask("Printing", sourceRequested => sourceRequested.SetSource(printDocumentSource));
            printTask.Completed += printTask_Completed;
        }

        // unwire the printing related task events once document was printed

       async void printTask_Completed(PrintTask sender, PrintTaskCompletedEventArgs args)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UnRegisterForPrinting();
            });
        }

        ~PrintManagerBase()
        {
            Dispose(false);
            UnRegisterForPrinting();
        }
#endif
#if WinRT || UNIVERSAL
        internal object TryFindResource(FrameworkElement element, object key)
        {
            if (element.Resources.ContainsKey(key))
                return element.Resources[key];
            else
            {
                if (element.Parent != null && element.Parent is FrameworkElement)
                    return this.TryFindResource((FrameworkElement)element.Parent, key);
                else
                {
                    if (Application.Current.Resources.ContainsKey(key))
                        return Application.Current.Resources[key];
                }
            }

            return null;
        }
#endif

        #region INotifyPropertyChanged Members

        /// <summary>
        /// Occurs when a property value changes in <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members

        #region Dispose
        /// <summary>
        /// Releases all  the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>        
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                Provider = null;
                source = null;
                printControls.Clear();
                if (PageDictionary != null)
                    PageDictionary.Clear();
            }
            isdisposed = true;
        }

        #endregion Dispose
    }

    /// <summary>
    /// 
    /// </summary>
    public class GridPrintManager : PrintManagerBase
    {
        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </value>
        protected SfDataGrid dataGrid { get; private set; }

        #region Private properties
        private CoveredCellInfoCollection coveredCells;
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridPrintManager"/> class.
        /// </summary>
        /// <param name="datagrid">
        /// The instance of the SfDataGrid.
        /// </param>
        public GridPrintManager(SfDataGrid datagrid)
            : base(datagrid.View)
        {
            dataGrid = datagrid;
        }

        #endregion Ctor

        #region Overrides
#if UWP

        protected override void OnCreatePrintPreviewPages(object sender, PaginateEventArgs e)
        {
            printControls.Clear();
            var printOptions = e.PrintTaskOptions;

            var pageDescrip = printOptions.GetPageDescription(0);
            PrintPageHeight = pageDescrip.PageSize.Height;
            PrintPageWidth = pageDescrip.PageSize.Width;
            PrintPageOrientation = printOptions.Orientation;
            dataGrid.PrintSettings.PrintPageOrientation = PrintPageOrientation;
            InitializePrint(true);
            printDocument.SetPreviewPageCount(pageCount, PreviewPageCountType.Intermediate);
        }
        internal override async void OnPrintTaskRequested(PrintManager sender, PrintTaskRequestedEventArgs args)
        {
            base.OnPrintTaskRequested(sender, args);
            //Need to consider the Orientation option provided in print settings, while load the content in the Preiew Panel.
            //To display the data as per the provided orientation.
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                printTask.Options.Orientation = dataGrid.PrintSettings.PrintPageOrientation;
            });
        }
#endif
        /// <summary>
        /// Initializes the header of specified list of column name to the particular page corresponding to the start and end of the column index.
        /// </summary>
        /// <param name="rowDictionary">
        /// The rowDictionary to add the header cell info in page.
        /// </param>
        /// <param name="pageIndex">
        /// The corresponding index of page to initialize headers.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to initialize its headers on particular page.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to initialize its headers on particular page.
        /// </param>
        /// <param name="start">
        /// The  index for the Stacked Header.
        /// </param>
        protected override void InitializeStackedHeaderForPage(List<RowInfo> rowDictionary, int pageIndex, int startColumnIndex, int endColumnIndex, int start)
        {
            List<CellInfo> cellRects = null;
            if (pageIndex > pagesPerRecord)
            {
                if (AllowRepeatHeaders)
                    cellRects = AddStackedHeaderInfotoDict( 0, startColumnIndex, endColumnIndex, start);
            }
            else
                cellRects = AddStackedHeaderInfotoDict(0, startColumnIndex, endColumnIndex, start);
            var needBottomBorder = !rowDictionary.Any();
            if (cellRects != null)
            {
                rowDictionary.Add(new RowInfo
                {
                    CellsInfo = cellRects,
                    RecordIndex = -1,
                    NeedTopBorder = true,
                    NeedBottomBorder = false,
                    RowType = RowType.StackedHeaderRow
                });
            }
        }

        /// <summary>
        /// Adds the header information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="columnsNames">
        /// The list of column names collection that is used to measures the number of header arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add header information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add header information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of header cell info that is going to be printed.
        /// </returns>
        protected override List<CellInfo> AddHeaderInfoToDict(List<string> columnsNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            if (!CanPrintStackedHeaders || dataGrid.StackedHeaderRows.Count == 0)
                return base.AddHeaderInfoToDict(columnsNames, yPos, startColumnIndex, endColumnIndex);

            var rowCount = GetStackedHeaders();
            var headerrowheight = GetRowHeight(null, -1, RowType.HeaderRow);
            yPos = headerrowheight * rowCount;
            var cellRects = new List<CellInfo>();
            double columnWidths = 0;
            for (var start = startColumnIndex; start <= endColumnIndex; start++)
            {
                int headerRowSpanValue = 0;
                double Yposition = yPos;
                bool hasstackedcolumn = false;
                var RowHeight = GetRowHeight(null, -1, RowType.HeaderRow);
                var columnName = columnsNames[start];
                for (int i = dataGrid.StackedHeaderRows.Count - 1; i >= 0; i--)
                {
                    var stckkedheader = dataGrid.StackedHeaderRows[i];
                    for (int k = 0; k < stckkedheader.StackedColumns.Count; k++)
                    {
                        var childColumn = stckkedheader.StackedColumns[k].ChildColumns;
                        hasstackedcolumn = childColumn.Split(',').Any(childcol => childcol == columnName);
                        if (hasstackedcolumn)
                        {
                            headerRowSpanValue = i;
                            break;
                        }
                    }
                    if (hasstackedcolumn)
                        break;
                }

                if (!hasstackedcolumn && headerRowSpanValue == 0)
                {
                    for (int cou = 0; cou < dataGrid.StackedHeaderRows.Count; cou++)
                        RowHeight += GetRowHeight(null, -1, RowType.StackedHeaderRow);
                    Yposition = yPos - RowHeight + GetRowHeight(null, -1, RowType.HeaderRow);
                }
                if (hasstackedcolumn)
                {
                    for (int j = 0; j < dataGrid.StackedHeaderRows.Count - headerRowSpanValue - 1; j++)
                        RowHeight += GetRowHeight(null, -1, RowType.StackedHeaderRow);
                    Yposition = yPos - RowHeight + GetRowHeight(null, -1, RowType.HeaderRow);
                }

                var width = GetColumnWidth(columnName);
                if (start == startColumnIndex)
                    width += groupCount * IndentColumnWidth;
                width = width < 0 ? 0 : width;
                cellRects.Add(new CellInfo
                {
                    CellRect = new Rect(columnWidths, Yposition, width, RowHeight),
                    ColumnName = columnName
                });
                columnWidths += width;
            }
            return cellRects;
        }

        /// <summary>
        /// Adds the header information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="yPos">
        /// The y position for the Stacked Header in a page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add header information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add header information for printing.
        /// </param>
        /// <param name="start">
        /// The index of the Stacked Header.
        /// <returns>
        /// Returns the list of Stacked Header cell info that is going to be printed.
        /// </returns>
        protected override List<CellInfo> AddStackedHeaderInfotoDict(double yPos, int startColumnIndex, int endColumnIndex, int rowStart)
        {
            double columnWidth = 0;
            var cellRects = new List<CellInfo>();
            int stackedheadercount = dataGrid.StackedHeaderRows.Count;
            int start = startColumnIndex;
            yPos = GetRowHeight(null, -1, RowType.StackedHeaderRow) * rowStart;
            while (start <= endColumnIndex)
            {
                double Yposition = yPos;
                int columnspan = 0;
                double spannedwidth = 0;
                double Height = 0.0;
                var stackedrow = dataGrid.StackedHeaderRows[rowStart];
                var col = dataGrid.Columns[start];
                var stackcol = stackedrow.StackedColumns.FirstOrDefault(scol => scol.ChildColumns.Contains(col.mappingName));
                bool hasstackedcolumn = false;
                if (stackcol == null)
                {
                    if (start == startColumnIndex)
                        columnWidth += GetColumnWidth(col.mappingName) + (groupCount * IndentColumnWidth);
                    else
                        columnWidth += GetColumnWidth(col.mappingName);
                    start++;
                    continue;
                }
                if (stackcol != null && !hasstackedcolumn)
                {
                    Height = 0.0;
                    int Count = 0;
                    if (!hasstackedcolumn && rowStart != 0)
                    {
                        for (int j = rowStart - 1; j >= 0; j--)
                        {
                            var endindex = dataGrid.StackedHeaderRows[j].StackedColumns.Count;
                            Count++;
                            for (int k = 0; k < endindex; k++)
                            {
                                var childColumn_1 = dataGrid.StackedHeaderRows[j].StackedColumns[k].ChildColumns;
                                if (childColumn_1.Contains(col.mappingName))
                                {
                                    Count++;
                                    hasstackedcolumn = true;
                                    break;
                                }
                            }
                            if (hasstackedcolumn)
                                break;
                        }
                        var range = hasstackedcolumn ? Count - 1 : Count + 1;
                        for (int k = 0; k < range; k++)
                            Height += GetRowHeight(null, -1, RowType.StackedHeaderRow);
                    }
                }
                if (rowStart == 0)
                    Height = GetRowHeight(null, -1, RowType.StackedHeaderRow);
                if (start == startColumnIndex)
                    spannedwidth += groupCount * IndentColumnWidth;
                if (stackcol != null)
                {
                    while (start <= endColumnIndex && stackcol.ChildColumns.Contains(dataGrid.Columns[start].mappingName))
                    {
                        columnspan++;
                        spannedwidth += this.GetColumnWidth(this.dataGrid.Columns[start].mappingName);
                        start++;
                    }
                }
                Yposition = yPos - Height + GetRowHeight(null, -1, RowType.StackedHeaderRow);
                CellInfo cellinfo = new CellInfo();
                cellinfo.ColumnName = stackcol.HeaderText;
                cellinfo.CellRect = new Rect(columnWidth, Yposition, spannedwidth, Height);
                cellRects.Add(cellinfo);
                columnWidth += spannedwidth;
            }
            return cellRects;
        }


        /// <summary>
        /// Initializes properties and settings for printing process.
        /// </summary>
        protected override void InitializeProperties()
        {
            // Intilaize CoveredCellInfo collection
            coveredCells = new CoveredCellInfoCollection(this.dataGrid);

            if (dataGrid.PrintSettings == null) return;
            var printSettings = dataGrid.PrintSettings;
            AllowColumnWidthFitToPrintPage = printSettings.AllowColumnWidthFitToPrintPage;
            PrintPageFooterHeight = printSettings.PrintPageFooterHeight;
            PrintPageFooterTemplate = printSettings.PrintPageFooterTemplate;
            PrintPageHeaderHeight = printSettings.PrintPageHeaderHeight;
            PrintPageHeaderTemplate = printSettings.PrintPageHeaderTemplate;
            PrintPageMargin = printSettings.PrintPageMargin;
            PrintPageWidth = printSettings.PrintPageWidth;
            PrintPageHeight = printSettings.PrintPageHeight;
            AllowRepeatHeaders = printSettings.AllowRepeatHeaders;
            PrintHeaderRowHeight = printSettings.PrintHeaderRowHeight;
            PrintRowHeight = printSettings.PrintRowHeight;
            PrintFlowDirection = printSettings.PrintFlowDirection;
            PrintScaleOption = printSettings.PrintScaleOption;
            AllowPrintStyles = printSettings.AllowPrintStyles;
            CanPrintStackedHeaders = printSettings.CanPrintStackedHeaders;
            PrintPageOrientation = printSettings.PrintPageOrientation;
#if WPF
            AllowPrintByDrawing = printSettings.AllowPrintByDrawing;
#endif
        }

        /// <summary>
        /// Gets the corresponding data source for printing.
        /// </summary>
        /// <returns>
        /// Returns the corresponding data source.
        /// </returns>
        protected override IList GetSourceListForPrinting()
        {
            var source = dataGrid.EnableDataVirtualization ? (View as CollectionViewAdv).GetInternalSource() as IList: base.GetSourceListForPrinting();
            List<object> OrderedSource = new List<object>();

            if (GetUnBoundRowCount(UnBoundRowsPosition.Top, false) > 0)
            {
                foreach (var row in dataGrid.UnBoundRows.Where(r => r.Position == UnBoundRowsPosition.Top && !r.ShowBelowSummary))
                    OrderedSource.Add(row);
            }

            if (GetTableSummaryList(TableSummaryRowPosition.Top).Count > 0)
            {
                foreach (var item in GetTableSummaryList(TableSummaryRowPosition.Top))
                    OrderedSource.Add(item);
            }

            if (GetUnBoundRowCount(UnBoundRowsPosition.Top, true) > 0)
            {
                foreach (var row in dataGrid.UnBoundRows.Where(r => r.Position == UnBoundRowsPosition.Top && r.ShowBelowSummary))
                    OrderedSource.Add(row);
            }

            foreach (var item in source)
                OrderedSource.Add(item);

            if (GetUnBoundRowCount(UnBoundRowsPosition.Bottom, false) > 0)
            {
                foreach (var row in dataGrid.UnBoundRows.Where(r => r.Position == UnBoundRowsPosition.Bottom && !r.ShowBelowSummary))
                    OrderedSource.Add(row);
            }

            if (GetTableSummaryList(TableSummaryRowPosition.Bottom).Count > 0)
            {
                foreach (var item in GetTableSummaryList(TableSummaryRowPosition.Bottom))
                    OrderedSource.Add(item);
            }

            if (GetUnBoundRowCount(UnBoundRowsPosition.Bottom, true) > 0)
            {
                foreach (var row in dataGrid.UnBoundRows.Where(r => r.Position == UnBoundRowsPosition.Bottom && r.ShowBelowSummary))
                    OrderedSource.Add(row);
            }

            return OrderedSource;
        }

        /// <summary>
        /// Gets the list of column names collection that need to be printed.
        /// </summary>
        /// <returns>
        /// Returns the collection of column name collection that need to be printed.
        /// </returns>
        protected override List<string> GetColumnNames()
        {
            return this.dataGrid.Columns.Where(col => !col.IsHidden).Select(x => x.MappingName).ToList();
        }


        ///<summary>
        ///Get the Stacked Headers count. 
        /// </summary>
        /// /// <returns>
        /// Return thecount of the stacked headers to be pinted.
        /// </returns>
        internal override int GetStackedHeaders()
        {
            return dataGrid.StackedHeaderRows.Count;
        }

        /// <summary>
        /// Gets the header text of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its header text.
        /// </param>
        /// <returns>
        /// Returns the header text of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the header text of the column based on its mapping name.
        /// </remarks>
        protected override string GetColumnHeaderText(string mappingName)
        {
            return !string.IsNullOrEmpty(this.dataGrid.Columns[mappingName].HeaderText)
                       ? this.dataGrid.Columns[mappingName].HeaderText
                       : mappingName;
        }

        /// <summary>
        /// Gets the column width for the specified mapping name of column.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its width.
        /// </param>
        /// <returns>
        /// Returns the column width of the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the column width based on the mapping name of the column.
        /// </remarks>
        protected override double GetColumnWidth(string mappingName)
        {
            if (!AllowColumnWidthFitToPrintPage && !double.IsNaN(this.dataGrid.Columns[mappingName].ActualWidth) && this.dataGrid.Columns[mappingName].ActualWidth < PrintPageWidth)
                return this.dataGrid.Columns[mappingName].ActualWidth;
            else
                return base.GetColumnWidth(mappingName);
        }

        /// <summary>
        /// Gets the padding of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its padding.
        /// </param>
        /// <returns>
        /// Returns the padding of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the padding for column based on its mapping name.
        /// </remarks>
        protected override Thickness GetColumnPadding(string mappingName)
        {
            return dataGrid.Columns[mappingName].Padding;
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.TextAlignment"/> of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its <see cref="System.Windows.TextAlignment"/>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.TextAlignment"/> of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the <see cref="System.Windows.TextAlignment"/> for column based on its mapping name.
        /// </remarks>
        protected override TextAlignment GetColumnTextAlignment(string mappingName)
        {
            return dataGrid.Columns[mappingName].TextAlignment;
        }

        /// <summary>
        /// Gets the <see cref="System.Windows.TextWrapping"/> of the column for the specified mapping name.
        /// </summary>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its <see cref="System.Windows.TextWrapping"/>.
        /// </param>
        /// <returns>
        /// Returns the <see cref="System.Windows.TextWrapping"/> of column for the specified mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the <see cref="System.Windows.TextWrapping"/> of column based on its mapping name.
        /// </remarks>
        protected override TextWrapping GetColumnTextWrapping(string mappingName)
        {
            var column = dataGrid.Columns[mappingName];
            if (column != null && column is GridTextColumn)
                return (column as GridTextColumn).TextWrapping;

            return TextWrapping.NoWrap;
        }

        /// <summary>
        /// Gets the column element of the specified record and mapping name for printing each GridCell in a column.
        /// </summary>
        /// <param name="record">
        /// Specifies the corresponding record to get the column element.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mappingName of the column to get its column element.
        /// </param>
        /// <returns>
        /// Returns the column element of the specified record and mapping name.
        /// </returns>
        /// <remarks>
        /// Override this method and customize the column element based on its record and mapping name. 
        /// </remarks>
        protected override object GetColumnElement(object record, string mappingName)
        {
            var column = dataGrid.Columns[mappingName];

            if (column.hasCellTemplate)
                return column.CellTemplate;

            if (column is GridCheckBoxColumn)
            {
                var ckb = new CheckBox
                {
                    IsEnabled = false,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    FlowDirection = PrintFlowDirection,
                    DataContext = record
                };
#if !WinRT && !UNIVERSAL
                var bind = column.ValueBinding.CreateEditBinding(column);
                ckb.SetBinding(ToggleButton.IsCheckedProperty, bind);
#else
                ckb.SetBinding(ToggleButton.IsCheckedProperty, column.ValueBinding);
#endif
                return ckb;
            }

            if (column is GridImageColumn)
            {
                var image = new Image
                {
                    DataContext = record
                };
                image.SetBinding(Image.SourceProperty, column.ValueBinding);

                var stretchBind = new Binding { Path = new PropertyPath("Stretch"), Mode = BindingMode.TwoWay, Source = column };
                image.SetBinding(Image.StretchProperty, stretchBind);

                //when image width, image height is not set, no need to bind that properties.
                var gridImageColumn = column as GridImageColumn;

                if (!double.IsInfinity(gridImageColumn.ImageHeight))
                {
                    var imageHeightBinding = new Binding { Path = new PropertyPath("ImageHeight"), Mode = BindingMode.TwoWay, Source = column };
                    image.SetBinding(FrameworkElement.HeightProperty, imageHeightBinding);
                }
                if (!double.IsInfinity(gridImageColumn.ImageWidth))
                {
                    var imageWidthBinding = new Binding { Path = new PropertyPath("ImageWidth"), Mode = BindingMode.TwoWay, Source = column };
                    image.SetBinding(FrameworkElement.WidthProperty, imageWidthBinding);
                }

#if WPF
                var stretchDirectionBind = new Binding { Path = new PropertyPath("StretchDirection"), Mode = BindingMode.TwoWay, Source = column };
                image.SetBinding(Image.StretchDirectionProperty, stretchDirectionBind);
#endif
                var paddingBind = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
                image.SetBinding(FrameworkElement.MarginProperty, paddingBind);
                image.HorizontalAlignment = TextAlignmentToHorizontalAlignment(column.TextAlignment);

                return image;
            }

            var tb = new TextBlock
            {
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = GetColumnTextAlignment(mappingName),
                TextWrapping = GetColumnTextWrapping(mappingName),
                FlowDirection = PrintFlowDirection,
                DataContext = record
            };
            if (!column.IsUnbound)
                tb.SetBinding(TextBlock.TextProperty, column.DisplayBinding);
            else
            {
                var unboundval = dataGrid.GetUnBoundCellValue(column, record);
                tb.Text = unboundval != null ? unboundval.ToString() : string.Empty;
            }
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
            tb.Padding = padding != DependencyProperty.UnsetValue
                             ? column.Padding
                             : new Thickness(4, 3, 3, 1);
            return tb;
        }

        /// <summary>
        /// Gets the string format of group caption.
        /// </summary>
        /// <returns>
        /// Returns the corresponding string format of group caption.
        /// </returns>
        /// <remarks>
        /// Override this method and customize string format of group caption.
        /// </remarks>
        protected override string GetGroupCaptionStringFormat()
        {
            return dataGrid.GroupCaptionTextFormat ?? dataGrid.GroupCaptionConstant;
        }

        /// <summary>
        /// Returns the header cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print header cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print header cell.
        /// </param>
        /// <returns>
        /// Returns the GridHeaderCellControl if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintHeaderCell.
        /// </returns>
        public override ContentControl GetPrintHeaderCell(string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintHeaderCell(mappingName);

            var gridCell = new GridHeaderCellControl();
            gridCell.IsHitTestVisible = false;
            var column = dataGrid.Columns[mappingName];
            gridCell.DataGrid = this.dataGrid;
            gridCell.DataContext = column;
            if (!dataGrid.hasHeaderStyle && !column.hasHeaderStyle)
                gridCell.Style = GetCellStyle(typeof(GridHeaderCellControl));
            else if (column.hasHeaderStyle)
                gridCell.Style = column.HeaderStyle;
            else if (dataGrid.hasHeaderStyle)
                gridCell.Style = dataGrid.HeaderStyle;

            return gridCell;
        }

        /// <summary>
        /// Returns the caption summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print caption summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print caption summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridGridCaptionSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintCaptionSummaryCell.
        /// </returns>
        public override ContentControl GetPrintCaptionSummaryCell(object group, string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintCaptionSummaryCell(group, mappingName);

            var gridCell = new GridCaptionSummaryCell();
            gridCell.IsHitTestVisible = false;
            gridCell.DataContext = group;
            if (!dataGrid.hasCaptionSummaryCellStyle && !dataGrid.hasCaptionSummaryCellStyleSelector)
                gridCell.Style = GetCellStyle(typeof(GridCaptionSummaryCell));
            else if (dataGrid.hasCaptionSummaryCellStyle && dataGrid.hasCaptionSummaryCellStyleSelector)
            {
                var newStyle = dataGrid.GroupSummaryCellStyleSelector.SelectStyle(group, gridCell);
                gridCell.Style = newStyle ?? dataGrid.CaptionSummaryCellStyle;
            }
            else if (dataGrid.hasCaptionSummaryCellStyleSelector)
                gridCell.Style = dataGrid.CaptionSummaryCellStyleSelector.SelectStyle(group, gridCell);
            else if (dataGrid.hasCaptionSummaryCellStyle)
                gridCell.Style = dataGrid.CaptionSummaryCellStyle;

            return gridCell;
        }

        /// <summary>
        /// Returns the grid cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print grid cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print grid cell.
        /// </param>
        /// <returns>
        /// Returns the GridCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintGridCell.
        /// </returns>
        public override ContentControl GetPrintGridCell(object record, string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintGridCell(record, mappingName);

            var gridCell = new GridCell();
            gridCell.IsHitTestVisible = false;
            var column = dataGrid.Columns[mappingName];
            gridCell.DataContext = record;
            if (column == null)
                return gridCell;

            if (!column.hasCellStyleSelector && !column.hasCellStyle && !dataGrid.hasCellStyle && !dataGrid.hasCellStyleSelector)
                gridCell.Style = GetCellStyle(typeof(GridCell));
            else if (column.hasCellStyleSelector && column.hasCellStyle)
            {
                var newStyle = column.CellStyleSelector.SelectStyle(record, gridCell);
                gridCell.Style = newStyle ?? column.CellStyle;
            }
            else if (column.hasCellStyleSelector)
                gridCell.Style = column.CellStyleSelector.SelectStyle(record, gridCell);
            else if (column.hasCellStyle)
                gridCell.Style = column.CellStyle;
            else if (dataGrid.hasCellStyleSelector && dataGrid.hasCellStyle)
            {
                var newStyle = dataGrid.CellStyleSelector.SelectStyle(record, gridCell);
                gridCell.Style = newStyle ?? dataGrid.CellStyle;
            }
            else if (dataGrid.hasCellStyleSelector)
                gridCell.Style = dataGrid.CellStyleSelector.SelectStyle(record, gridCell);
            else if (dataGrid.hasCellStyle)
                gridCell.Style = dataGrid.CellStyle;

            return gridCell;
        }

        /// <summary>
        /// Returns the table summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print table summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print table summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridTableSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintTableSummaryCell.
        /// </returns>
        public override ContentControl GetPrintTableSummaryCell(object summaryRecord, string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintTableSummaryCell(summaryRecord, mappingName);

            var gridcell = new GridTableSummaryCell();
            gridcell.IsHitTestVisible = false;
            gridcell.DataContext = summaryRecord;
            if (!dataGrid.hasTableSummaryCellStyleSelector && !dataGrid.hasTableSummaryCellStyle)
                gridcell.Style = GetCellStyle(typeof(GridTableSummaryCell));
            else if (dataGrid.hasTableSummaryCellStyleSelector && dataGrid.hasTableSummaryCellStyle)
            {
                var newStyle = dataGrid.TableSummaryCellStyleSelector.SelectStyle(summaryRecord, gridcell);
                gridcell.Style = newStyle ?? dataGrid.TableSummaryCellStyle;
            }
            else if (dataGrid.hasTableSummaryCellStyleSelector)
                gridcell.Style = dataGrid.TableSummaryCellStyleSelector.SelectStyle(summaryRecord, gridcell);
            else if (dataGrid.hasTableSummaryCellStyle)
                gridcell.Style = dataGrid.TableSummaryCellStyle;

            return gridcell;
        }

        /// <summary>
        /// Returns the UnboundRow cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print UnboundRow cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print UnboundRow cell.
        /// </param>
        /// <returns>
        /// Returns the GridUnboundRowCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintUnboundRowCell.
        /// </returns>
        public override ContentControl GetPrintUnboundRowCell(object record, string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintUnboundRowCell(record, mappingName);

            var gridcell = new GridUnBoundRowCell();
            gridcell.IsHitTestVisible = false;
            gridcell.DataContext = record;
            gridcell.Style = GetCellStyle(typeof(GridUnBoundRowCell));

            return gridcell;
        }

        /// <summary>
        /// Returns the group summary cell for the specified record and mapping name for printing. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record to print group summary cell.
        /// </param>
        /// <param name="mappingName">
        /// The corresponding mapping name to print group summary cell.
        /// </param>
        /// <returns>
        /// Returns the GridGroupSummaryCell if the <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings.AllowPrintStyles"/> is true; otherwise return the PrintGroupSummaryCell.
        /// </returns>
        public override ContentControl GetPrintGroupSummaryCell(object summaryRecord, string mappingName)
        {
            if (!AllowPrintStyles)
                return base.GetPrintGroupSummaryCell(summaryRecord, mappingName);

            var gridcell = new GridGroupSummaryCell();
            gridcell.IsHitTestVisible = false;
            gridcell.DataContext = summaryRecord;
            if (!dataGrid.hasGroupSummaryCellStyle && !dataGrid.hasGroupSummaryCellStyleSelector)
            {
#if WPF
                gridcell.Style = (Style)dataGrid.TryFindResource(typeof(GridGroupSummaryCell));
#else
                    gridcell.Style = (Style)this.TryFindResource(dataGrid, typeof(GridGroupSummaryCell));
#endif
            }
            else if (dataGrid.hasGroupSummaryCellStyle && dataGrid.hasGroupSummaryCellStyleSelector)
            {
                var newStyle = dataGrid.GroupSummaryCellStyleSelector.SelectStyle(summaryRecord, gridcell);
                gridcell.Style = newStyle ?? dataGrid.GroupSummaryCellStyle;
            }
            else if (dataGrid.hasGroupSummaryCellStyleSelector)
            {
                gridcell.Style = dataGrid.GroupSummaryCellStyleSelector.SelectStyle(summaryRecord, gridcell);
            }
            else if (dataGrid.hasGroupSummaryCellStyle)
            {
                gridcell.Style = dataGrid.GroupSummaryCellStyle;
            }
            return gridcell;
        }

        /// <summary>
        /// Gets a value that determines whether the corresponding row is CaptionSummaryRow for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the row is CaptionSummaryRow; otherwise , <b>false</b>.
        /// </value>
        protected override bool IsCaptionSummaryInRow
        {
            get
            {
                if (dataGrid.CaptionSummaryRow != null)
                    return dataGrid.CaptionSummaryRow.ShowSummaryInRow;
                else
                    return true;
            }
        }

        internal override void SetDataTemplateContentToPrintGridCell(ContentControl cell, CellInfo cellInfo, object record)
        {
            var column = dataGrid.Columns[cellInfo.ColumnName];
            if (column.GetType() == typeof(GridUnBoundColumn))
            {
                var contentValue = dataGrid.GetUnBoundCellValue(column, record);
                var dataContextHelper = new DataContextHelper { Record = record };
                dataContextHelper.Value = contentValue;
                cell.Content = dataContextHelper;
            }
            else
            {
                if (column.SetCellBoundValue)
                {
                    var dataContextHelper = new DataContextHelper { Record = record };
                    dataContextHelper.SetValueBinding(column.ValueBinding, record);
                    cell.Content = dataContextHelper;
                }
                else
                    cell.Content = record;
            }
        }       

        #region Initial page computation for pageDictonary
        /// <summary>
        /// Adds the row information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record information of row is added to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of columns arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add row information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add row information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of cell info that is going to be printed.
        /// </returns>
        /// <remarks>
        /// Invoked to add rows such as normal row, group caption summary row, summary row, table summary row to dictionary for printing.
        /// </remarks>
        protected override List<CellInfo> AddRowInformationToDictionary(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            if (record is GridUnBoundRow)
                return AddUnboundRowInfoToDict(record as GridUnBoundRow, columnNames, yPos, startColumnIndex, endColumnIndex);
            else
                return base.AddRowInformationToDictionary(record, columnNames, yPos, startColumnIndex, endColumnIndex);
        }

        /// <summary>
        /// Adds the UnBoundRow information to dictionary that is going to be printed. 
        /// </summary>
        /// <param name="record">
        /// The corresponding record information of UnboundRow is added to dictionary.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of columns arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add row information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add row information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of UnboundRow cell info that is going to be printed.
        /// </returns>
        protected virtual List<CellInfo> AddUnboundRowInfoToDict(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            List<CellInfo> cellRects = new List<CellInfo>();
            if (record == null)
                return cellRects;
            var xPos = 0.0;
            var rowIndex = source.IndexOf(record);
            var RowHeight = GetRowHeight(record, rowIndex,RowType.UnBoundRow);
            for (int startIndex = startColumnIndex; startIndex <= endColumnIndex; startIndex++)
            {
                var columnName = columnNames[startIndex];
                //adjust column width with group indent coulmn width
                var width = GetColumnWidth(columnName) + (startIndex == startColumnIndex ? groupCount * IndentColumnWidth : 0);
                var rect = GetRectInfo(record, columnName, startIndex, RowHeight, width, xPos, yPos, rowIndex);

                xPos += width;
                if (rect.Width == 0.0 && rect.Height == 0.0)
                    continue;

                cellRects.Add(new CellInfo
                {
                    CellRect = rect,
                    ColumnName = columnName
                });                                
            }

            return cellRects;
        }

        #endregion Intial page computation for pageDictonary

        #region Add page child elements in PrintPagePanel
        /// <summary>
        /// Adds the row info to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the row that is to be printed.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row information that is added to panel.
        /// </param>
        /// <param name="pageIndex">
        /// The corresponding index of page to add the row to print page panel.
        /// </param>
        /// <remarks>
        /// Invoked to add Rows such as Header row,summary(group summary, table summary),Unboundrow, etc...  to PrintPagePanel.
        /// </remarks>
        protected override void AddRowToPrintPagePanel(PrintPagePanel panel, RowInfo rowInfo, int pageIndex)
        {
            if (rowInfo.Record is GridUnBoundRow)
                AddUnBoundRowToPrintPagePanel(panel, rowInfo);
            else
                base.AddRowToPrintPagePanel(panel, rowInfo, pageIndex);
        }

        /// <summary>
        /// Adds the UnboundRow info to the specified print page panel that is going to be printed.
        /// </summary>
        /// <param name="panel">
        /// The corresponding panel to add the UnboundRow that is to be printed.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding UnboundRow information that is added to panel.
        /// </param>
        protected virtual void AddUnBoundRowToPrintPagePanel(PrintPagePanel panel, RowInfo rowInfo)
        {
            var topThickNess = rowInfo.NeedTopBorder ? 1 : 0;
            var bottomThickness = rowInfo.NeedBottomBorder ? 1 : 0;
            var record = rowInfo.Record as GridUnBoundRow;
            var i = 0;
            foreach (var cellInfo in rowInfo.CellsInfo)
            {
                var cell = GetPrintUnboundRowCell(record, cellInfo.ColumnName);
                if (i == 0)
                {
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Height = cellInfo.CellRect.Height;
                    cell.BorderThickness = new Thickness(1, topThickNess, 1, bottomThickness);
                }
                else
                {
                    cell.Width = cellInfo.CellRect.Width;
                    cell.Height = cellInfo.CellRect.Height;
                    cell.BorderThickness = new Thickness(0, topThickNess, 1, bottomThickness);
                }

                var tb = new TextBlock
                {
                    VerticalAlignment = VerticalAlignment.Center,
                    TextAlignment = GetColumnTextAlignment(cellInfo.ColumnName),
                    TextWrapping = GetColumnTextWrapping(cellInfo.ColumnName),
                    FlowDirection = PrintFlowDirection,
                    DataContext = record
                };
                var column = dataGrid.Columns[cellInfo.ColumnName];
                var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
                tb.Padding = padding != DependencyProperty.UnsetValue
                                 ? column.Padding
                                 : new Thickness(4, 3, 3, 1);

                object value;
                GridUnBoundRowEventsArgs queriedColumn = null;
                GridColumn gridColumn = dataGrid.Columns.First(item => item.MappingName.Equals(cellInfo.ColumnName));
                int columnIndex = dataGrid.ResolveToScrollColumnIndex(dataGrid.Columns.IndexOf(gridColumn));
                if (dataGrid.CanQueryUnBoundRow())
                    queriedColumn = dataGrid.RaiseQueryUnBoundRow(record as GridUnBoundRow, UnBoundActions.QueryData, null, gridColumn, gridColumn.CellType, new RowColumnIndex((record as GridUnBoundRow).RowIndex, columnIndex));
                value = queriedColumn != null ? queriedColumn.Value : null;
                tb.Text = value != null ? value.ToString() : string.Empty;

                cell.Content = tb;
                cellInfo.Element = cell;
                panel.Children.Add(cell);
                i++;
            }
        }

        /// <summary>
        /// Adds the record information to dictionary that is going to be printed.
        /// </summary>
        /// <param name="record">
        /// The corresponding record information that is added in to dictionary for printing.
        /// </param>
        /// <param name="columnNames">
        /// The list of column names collection that is used to measures the number of column arranged in a page.
        /// </param>
        /// <param name="yPos">
        /// The y position of the page.
        /// </param>
        /// <param name="startColumnIndex">
        /// The start index of the column to add record information for printing.
        /// </param>
        /// <param name="endColumnIndex">
        /// The end index of the column to add record information for printing.
        /// </param>
        /// <returns>
        /// Returns the list of record cell info that is going to be printed.
        /// </returns>
        protected override List<CellInfo> AddRecordInfoToDict(object record, List<string> columnNames, double yPos, int startColumnIndex, int endColumnIndex)
        {
            if(!this.dataGrid.CanQueryCoveredRange())
                return base.AddRecordInfoToDict(record, columnNames, yPos, startColumnIndex, endColumnIndex);

            List<CellInfo> cellRects = new List<CellInfo>();
            if (record == null)
                return cellRects;

            var rowIndex = source.IndexOf(record);
            var RowHeight = GetRowHeight(record, rowIndex,RowType.DefaultRow);
            var xPos = record is RecordEntry && (record as RecordEntry).Level >= 0
                ? (record as RecordEntry).Level * IndentColumnWidth
                : 0;
            for (int startIndex = startColumnIndex; startIndex <= endColumnIndex; startIndex++)
            {
                var columnName = columnNames[startIndex];
                var width = GetColumnWidth(columnName);
                var rect = GetRectInfo(record, columnName, startIndex, RowHeight, width, xPos, yPos, rowIndex);

                xPos += width;
                if (rect.Width == 0.0 && rect.Height == 0.0)                        
                    continue;                

                cellRects.Add(new CellInfo
                    {
                        CellRect = rect,
                        ColumnName = columnName
                    });                                
            }

            return cellRects;
        }

#if WPF
        /// <summary>
        /// Draws the content of each row for the specified drawingContext and list of row infos to page panel element. 
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw the row info.
        /// </param>
        /// <param name="rowsInfoList">
        /// The list of row infos to draw the row info. 
        /// </param>
        protected override void OnRenderRow(DrawingContext drawingContext, RowInfo rowInfoList)
        {
            if(!this.dataGrid.CanQueryCoveredRange())
                base.OnRenderRow(drawingContext, rowInfoList);
            else
            {
                var cellsInfo = rowInfoList.CellsInfo;

                foreach (var cellInfo in cellsInfo)
                {
                    if (!(cellInfo.CellRect.X == 0.0 && cellInfo.CellRect.Y == 0.0 && cellInfo.CellRect.Width == 0.0 && cellInfo.CellRect.Height == 0.0))
                    {
                        OnRenderCell(drawingContext, rowInfoList, cellInfo);
                        //Last Cell Right Border
                        if (cellInfo.Equals(cellsInfo[cellsInfo.Count - 1]))
                            drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellInfo.CellRect.TopRight, cellsInfo[cellsInfo.Count - 1].CellRect.BottomRight);   //Last cell Right border 
                    }
                }
                if (rowInfoList.NeedTopBorder)
                {
                    //Top border of the page
                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[0].CellRect.TopLeft, cellsInfo[cellsInfo.Count - 1].CellRect.TopRight);
                }
                if (rowInfoList.NeedBottomBorder)
                {
                    if (coveredCells.Count > 0)
                    {
                        var cellsinfo = cellsInfo.Where(cellInfo => cellInfo.CellRect.Height == 0);
                        if (cellsinfo.Any())
                        {
                            foreach (var cellInfo in cellsInfo)
                            {
                                if (cellInfo.CellRect.Height > 0)
                                    drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellInfo.CellRect.BottomLeft, cellInfo.CellRect.BottomRight);
                            }
                        }
                        else
                        {
                            if (rowInfoList.CellsInfo[0].CellRect.Height > 0)
                                //Row Bottom Border
                                drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[0].CellRect.BottomLeft, cellsInfo[cellsInfo.Count - 1].CellRect.BottomRight);
                        }
                    }
                    else
                    {
                        if (rowInfoList.CellsInfo[0].CellRect.Height > 0)
                            //Row Bottom Border
                            drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellsInfo[0].CellRect.BottomLeft, cellsInfo[cellsInfo.Count - 1].CellRect.BottomRight);
                    }
                }
            }
        }
#endif

        #endregion
#if WPF

        #region Drawing override methods
        /// <summary>
        /// Draws the content of each cell value and its border for the specified drawingContext, row info and cell info to page panel element. 
        /// </summary>
        /// <param name="drawingContext">
        /// The corresponding drawingContext to draw each cell content.
        /// </param>
        /// <param name="rowInfo">
        /// The corresponding row info to draw each cell content. 
        /// </param>
        /// <param name="cellInfo">
        /// The corresponding cell info to draw each cell content. 
        /// </param>
        protected override void OnRenderCell(DrawingContext drawingContext, RowInfo rowInfo, CellInfo cellInfo)
        {
            var record = rowInfo.Record;
            var column = cellInfo.ColumnName;
            var cellvalue = string.Empty;
            var needleftborder = true;
            
            //canDrawCellValue - Based on this flag, we can decide to print the cell value or not. Because for image and checkbox columns, we implemented separate way to draw the cell values.
            bool canDrawCellValue = true;
            if (rowInfo.RecordIndex.Equals(-1))
            {
                if (rowInfo.RowType == RowType.StackedHeaderRow)
                    cellvalue = column;
                else
                    cellvalue = GetColumnHeaderText(cellInfo.ColumnName);
            }
            else
            {
                if (record is Group)
                {
                    if (IsCaptionSummaryInRow)
                        cellvalue = View.TopLevelGroup.GetGroupCaptionText((record as Group), GetGroupCaptionStringFormat(), cellInfo.ColumnName);
                    else
                    {
                        var columnname = dataGrid.Columns[cellInfo.ColumnName];
                        // if show summary is false then need to get each column value for display the caption summary
                        var value = SummaryCreator.GetSummaryDisplayText((record as Group).SummaryDetails, columnname.MappingName, View);
                        cellvalue = value != null ? value.ToString() : string.Empty;
                        //this condition is checked for while print  caption summary with show summary in row is false need to avoid left border of all the columns except 1st column.
                        if (rowInfo.CellsInfo[0] != cellInfo)
                            needleftborder = false;
                    }
                }
                else if (record is SummaryRecordEntry)
                {
                    if ((record as SummaryRecordEntry).SummaryRow.ShowSummaryInRow)
                    {
                        cellvalue = SummaryCreator.GetSummaryDisplayTextForRow((record as SummaryRecordEntry), View);
                    }
                    else
                    {
                        var summaryColumns = (record as SummaryRecordEntry).SummaryRow.SummaryColumns;
                        if (summaryColumns != null && summaryColumns.Any())
                        {
                            if (summaryColumns.Any(x => x.MappingName == cellInfo.ColumnName))
                            {
                                cellvalue = SummaryCreator.GetSummaryDisplayText((record as SummaryRecordEntry), cellInfo.ColumnName, View);
                            }
                        }
                        //WPF-19229 this condition is checked for while print  table summary with show summary in row is false need to avoid left border of all the columns except 1st column.
                        if (rowInfo.CellsInfo[0] != cellInfo)
                            needleftborder = false;
                    }
                }
                else if (record is GridUnBoundRow)
                {
                    object value;
                    GridUnBoundRowEventsArgs queriedColumn = null;
                    GridColumn gridColumn = dataGrid.Columns.Last(item => item.MappingName.Equals(column));
                    int columnIndex = dataGrid.ResolveToScrollColumnIndex(dataGrid.Columns.IndexOf(gridColumn));
                    if (dataGrid.CanQueryUnBoundRow())
                        queriedColumn = dataGrid.RaiseQueryUnBoundRow(record as GridUnBoundRow, UnBoundActions.QueryData, null, gridColumn, gridColumn.CellType, new RowColumnIndex((record as GridUnBoundRow).RowIndex, columnIndex));
                    value = queriedColumn != null ? queriedColumn.Value : null;
                    cellvalue = value != null ? value.ToString() : string.Empty;
                }
                else
                {
                    dynamic value = null;
                    if (column != "")
                    {
                        dynamic Column = string.Empty;
                        if (cellInfo.ColumnName != "")
                            Column = dataGrid.Columns[cellInfo.ColumnName];
                        if (Column.GetType() == typeof(GridCheckBoxColumn))
                        {
                            OnDrawCheckBox(drawingContext, rowInfo, cellInfo, Column);
                            canDrawCellValue = false;
                        }
                        else if (Column.GetType() == typeof(GridUnBoundColumn))
                        {
                            //WPF-20206 - if unbound column, we get it through GetUnboundCellValue.
                            var unboundColumn = Column as GridUnBoundColumn;
                            value = unboundColumn.DataGrid.GetUnBoundCellValue(unboundColumn, record is RecordEntry ? (record as RecordEntry).Data : record);
                        }
                        else if (Column.GetType() == typeof(GridImageColumn))
                        {
                            value = GetColumnElement(rowInfo.Record is RecordEntry ? (rowInfo.Record as RecordEntry).Data : rowInfo.Record, cellInfo.ColumnName);
                            OnDrawImage(drawingContext, rowInfo, cellInfo, (Image)value);
                            canDrawCellValue = false;
                        }
                        else
                        {
                            if (Provider != null)
                            {
                                value = Provider.GetFormattedValue(((record is RecordEntry) ? (record as RecordEntry).Data : record), cellInfo.ColumnName);
                            }
                        }
                    }
                    cellvalue = value != null ? value.ToString() : string.Empty;
                }
            }

            if (canDrawCellValue)
                OnDrawText(drawingContext, rowInfo, cellInfo, cellvalue);
            //cell left border
            if (needleftborder)
                drawingContext.DrawLine(new Pen(Brushes.Black, 1), cellInfo.CellRect.TopLeft, cellInfo.CellRect.BottomLeft);
        }

        #endregion Drawing override methods

#endif

        #endregion Overrides

        #region Private methods

        private HorizontalAlignment TextAlignmentToHorizontalAlignment(TextAlignment textAlignment)
        {
            HorizontalAlignment horizontalAlignment;

            switch (textAlignment)
            {
                case TextAlignment.Right:
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;

                case TextAlignment.Center:
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;

                case TextAlignment.Justify:
                    horizontalAlignment = HorizontalAlignment.Stretch;
                    break;

                default:
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
            return horizontalAlignment;
        }

        private int GetUnBoundRowCount(UnBoundRowsPosition position, bool isbelowsummary)
        {
            int count = 0;

            if (position == UnBoundRowsPosition.Top && !isbelowsummary)
                count = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);
            else if (position == UnBoundRowsPosition.Top && isbelowsummary)
                count = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);
            else if (position == UnBoundRowsPosition.Bottom && isbelowsummary)
                count = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            else if (position == UnBoundRowsPosition.Bottom && !isbelowsummary)
                count = dataGrid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, false);

            return count;
        }

        private Style GetCellStyle(Type type)
        {
#if WPF
            return (Style)dataGrid.TryFindResource(type);
#else
            return (Style)this.TryFindResource(dataGrid, type);
#endif
        }

        private Rect GetRectInfo(object record, string column, int columnIndex, double RowHeight, double columnWidth, double xPos, double yPos, int rowIndex)
        {            
            Rect rect = new Rect();            
            var gridColumn = this.dataGrid.Columns[column];

            rect = new Rect(xPos, yPos, columnWidth, RowHeight);

            if (gridColumn.CellTemplate != null &&
                (gridColumn.CellTemplateSelector != null ||
                 (gridColumn is GridTemplateColumn &&
                  ((gridColumn as GridTemplateColumn).EditTemplateSelector != null ||
                   this.dataGrid.CellTemplateSelector != null))))
                return rect;

            CoveredCellInfo range = null;
            this.GetCoveredCellInfo(out range, rowIndex, columnIndex + 1);
            GridQueryCoveredRangeEventArgs e = null;
            if (range != null)
                return new Rect();
            else
                e = this.dataGrid.RaiseQueryCoveredRange(new RowColumnIndex(rowIndex, columnIndex + 1 + dataGrid.GroupColumnDescriptions.Count), gridColumn, record, this.dataGrid);

            if (e.Range != null && e.Handled)
            {
                if (e.Range.Contains(rowIndex, columnIndex + 1))
                {
                    this.coveredCells.Add(e.Range);
                    rect = new Rect(xPos, yPos, e.Range.Width * columnWidth, e.Range.Height * RowHeight);
                }
            }

            return rect;
        }

        /// <summary>
        /// Returns covered range based on row and column index
        /// </summary>
        /// <param name="range"></param>
        /// <param name="rowIndex"></param>
        /// <param name="columnIndex"></param>
        private void GetCoveredCellInfo(out CoveredCellInfo range, int rowIndex, int columnIndex)
        {
            range = null;
            foreach (CoveredCellInfo coveredCell in this.coveredCells)
            {
                if (coveredCell == null)
                    continue;
                // returns the covered range for the row and column index
                if (coveredCell.Contains(rowIndex, columnIndex))
                {
                    range = coveredCell;
                    break;
                }
            }
        }

        #endregion Private methods
    }
}