#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics.Printing;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public class PrintSettings : INotifyPropertyChanged
    {
        private bool allowColumnWidthFitToPrintPage = true;
        private bool allowRepeatHeaders = true;
        private bool allowPrintStyles = false;
        private double printPageHeight = 1122.52;
        private double printPageWidth = 793.70;
        private bool canPrintStackedHeaders = false;
#if WPF
        private double printRowHeight = 24;
        private double printHeaderRowHeight = 28;
#else
        private double printRowHeight = 45;
        private double printHeaderRowHeight = 45;
#endif
        private double printPageHeaderHeight;
        private double printPageFooterHeight;
        private Thickness printPageMargin = new Thickness(96);
        private DataTemplate printPageHeaderTemplate;
        private DataTemplate printPageFooterTemplate;
        private PrintManagerBase printManagerBase;
        private PrintScaleOptions printScaleOption = PrintScaleOptions.NoScaling;
        private FlowDirection printFlowDirection = FlowDirection.LeftToRight;
#if WPF

        //option for Print By Drawing
        private bool allowPrintByDrawing = true;

        private Style printPreviewWindowStyle;
#endif

        private PrintOrientation printPageOrientation = PrintOrientation.Portrait;

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings"/> class.
        /// </summary>
        public PrintSettings()
        {
        }

        #endregion Ctor

        #region Public Properties

#if WPF
        /// <summary>
        /// Get or sets a value that indicates whether the printing is enabled by drawing to improve performance.
        /// </summary>
        /// <value>
        /// <b>true</b> if the printing is enabled by drawing; otherwise , <b>false</b>.
        /// </value>
        public bool AllowPrintByDrawing
        {
            get { return allowPrintByDrawing; }
            set
            {
                allowPrintByDrawing = value;
                OnPropertyChanged("AllowPrintByDrawing");
            }
        }

#endif

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation"/> that indicates whether the pages are printed in portrait or landscape mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation"/> enumeration that specifies the orientation for page. The default value is <see cref="Syncfusion.UI.Xaml.Grid.PrintOrientation.Portrait"/>.
        /// </value>
        public PrintOrientation PrintPageOrientation
        {
            get { return printPageOrientation; }
            set
            {
                if (printPageOrientation == value) return;
                printPageOrientation = value;
                OnPropertyChanged("PrintPageOrientation");
            }
        }
        
        /// <summary>
        /// Gets or sets the height of header row in SfDataGrid for printing.
        /// </summary>
        /// <value>
        /// The height of the header row in SfDataGrid.
        /// </value>
        public double PrintHeaderRowHeight
        {
            get { return printHeaderRowHeight; }
            set
            {
                if (printHeaderRowHeight == value) return;
                printHeaderRowHeight = value;
                OnPropertyChanged("PrintHeaderRowHeight");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the stacked header included for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the SfDataGrid is printed with stacked header; otherwise, <b>false</b>.
        /// </value>
        public bool CanPrintStackedHeaders
        {
            get { return canPrintStackedHeaders; }
            set
            {
                canPrintStackedHeaders = value;
            }
        }
        /// <summary>
        /// Gets or sets the height of DataRow in SfDataGrid for printing.
        /// </summary>
        /// <value>
        /// The height of the DataRow in SfDataGrid.
        /// </value>
        public double PrintRowHeight
        {
            get { return printRowHeight; }
            set
            {
                if (printRowHeight == value) return;
                printRowHeight = value;
                OnPropertyChanged("PrintRowHeight");
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions"/> that indicates the number of rows or columns are scaled to fit the page when it is printed.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions"/> enumeration that specifies how the rows or columns are scaled to fit the page. The default value is <see cref="Syncfusion.UI.Xaml.Grid.PrintScaleOptions.NoScaling"/>.
        /// </value>
        public PrintScaleOptions PrintScaleOption
        {
            get { return printScaleOption; }
            set
            {
                if (printScaleOption == value) return;
                printScaleOption = value;
                OnPropertyChanged("PrintScaleOption");
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
            get { return printManagerBase; }
            set
            {
                printManagerBase = value;
                OnPropertyChanged("PrintManagerBase");
            }
        }

        /// <summary>
        /// Gets or sets the height of a page for printing.
        /// </summary>
        /// <value>
        /// The height of a page for printing.
        /// </value>
        public double PrintPageHeight
        {
            get { return printPageHeight; }
            set
            {
                printPageHeight = value;
                OnPropertyChanged("PrintPageHeight");
            }
        }

        /// <summary>
        /// Gets or sets the width of a page for printing.
        /// </summary>
        /// <value>
        /// The width of a page for printing.
        /// </value>
        public double PrintPageWidth
        {
            get { return printPageWidth; }
            set
            {
                printPageWidth = value;
                OnPropertyChanged("PrintPageWidth");
            }
        }

        /// <summary>
        /// Gets or sets the height of the page header for printing.
        /// </summary>
        /// <value>
        /// The height of the page header.
        /// </value>     
        public double PrintPageHeaderHeight
        {
            get { return printPageHeaderHeight; }
            set
            {
                printPageHeaderHeight = value;
                OnPropertyChanged("PrintPageHeaderHeight");
            }
        }

        /// <summary>
        /// Gets or sets the direction of text displayed in page for printing.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.FlowDirection"/> that specifies the direction of text in page. The default is <see cref="System.Windows.FlowDirection.LeftToRight"/>
        /// </value>
        public FlowDirection PrintFlowDirection
        {
            get { return printFlowDirection; }
            set
            {
                if (printFlowDirection == value) return;
                printFlowDirection = value;
                OnPropertyChanged("PrintFlowDirection");
            }
        }

        /// <summary>
        /// Gets or sets the height of the page footer for printing.
        /// </summary>
        /// <value>
        /// The height of the page footer.
        /// </value>  
        public double PrintPageFooterHeight
        {
            get { return printPageFooterHeight; }
            set
            {
                printPageFooterHeight = value;
                OnPropertyChanged("PrintPageFooterHeight");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the all the column width fit with in single page .        
        /// </summary>
        /// <value>
        /// <b>true</b> if all the column width fit with in single page; otherwise, <b>false</b>.
        /// </value>
        public bool AllowColumnWidthFitToPrintPage
        {
            get { return allowColumnWidthFitToPrintPage; }
            set
            {
                allowColumnWidthFitToPrintPage = value;
                OnPropertyChanged("AllowColumnWidthFitToPrintPage");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the column headers repeated for all pages for printing.        
        /// </summary>
        /// <value>
        /// <b>true</b> if the column headers repeated for all pages; otherwise, <b>false</b>.
        /// </value>
        public bool AllowRepeatHeaders
        {
            get { return allowRepeatHeaders; }
            set
            {
                allowRepeatHeaders = value;
                OnPropertyChanged("AllowRepeatHeaders");
            }
        }

        /// <summary>
        /// Gets or sets the margin of page for printing.
        /// </summary>
        /// <value>
        /// The Thickness of page for printing.
        /// </value>    
        public Thickness PrintPageMargin
        {
            get { return printPageMargin; }
            set
            {
                printPageMargin = value;
                OnPropertyChanged("PrintPageMargin");
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
                printPageHeaderTemplate = value;
                OnPropertyChanged("PrintPageHeaderTemplate");
            }
        }

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the page footer for printing.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the page footer. The default is <b>null</b>.
        /// </value>
        public DataTemplate PrintPageFooterTemplate
        {
            get { return printPageFooterTemplate; }
            set
            {
                printPageFooterTemplate = value;
                OnPropertyChanged("PrintPageFooterTemplate");
            }
        }

        /// <summary>
        /// Get or sets a value that indicates whether the style is allowed for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the style is allowed for printing; otherwise, <b>false</b>.
        /// </value>
        public bool AllowPrintStyles
        {
            get { return allowPrintStyles; }
            set
            {
                allowPrintStyles = value;
                OnPropertyChanged("AllowPrintStyles");
            }
        }

#if WPF
       
        /// <summary>
        /// Gets or sets the style applied to PrintPreviewWindow in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to PrintPreviewWindow.The default is null.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridCell"/>.
        /// </remarks>   
        public Style PrintPreviewWindowStyle
        {
            get { return printPreviewWindowStyle; }
            set
            {
                printPreviewWindowStyle = value;
                OnPropertyChanged("PrintPreviewWindowStyle");
            }
        }

#endif

        #endregion Public Properties

        #region INotifyPropertyChanged Members
        /// <summary>
        /// Occurs when a property value changes in <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion INotifyPropertyChanged Members
    }

    #region internal classes

    /// <summary>
    /// Represents a class that contains the information about the cell information for printing.    
    /// </summary>
    public class CellInfo
    {
        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// A string that represents the name of the column.
        /// </value>
        public string ColumnName { get; set; }
            
        /// <summary>
        /// Gets or sets the rect information of cell.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Rect"/> that contains the height,width, and location of cell.
        /// </value>
        public Rect CellRect { get; set; }

        /// <summary>
        /// Gets or sets the UIElement of the cell.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.UIElement"/> of the cell.
        /// </value>
        public UIElement Element { get; set; }
    }

    /// <summary>
    /// Represents a class that contains the information about the row information for printing.    
    /// </summary>
    public class RowInfo
    {
        /// <summary>
        /// Gets or sets the index of the record.
        /// </summary>
        /// <value>
        /// The index of the record.
        /// </value>
        public int RecordIndex { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="Syncfusion.UI.Xaml.Grid.CellInfo"/> collection in row.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.CellInfo"/> collection in row.
        /// </value>
        public List<CellInfo> CellsInfo { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the top border is enabled around the row for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the top border is enabled around the row; otherwise, <b>false</b>.
        /// </value>
        public bool NeedTopBorder { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the bottom border is enabled around the row for printing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the bottom border is enabled around the row; otherwise, <b>false</b>.
        /// </value>
        public bool NeedBottomBorder { get; set; }

        /// <summary>
        /// Defines the constants that specify the possible row type for each row.
        /// </summary>
        /// <value>
        /// Indicates that the current row type is <see cref="Syncfusion.UI.Xaml.Grid.RowType"/> row.
        /// </value>
        public RowType RowType { get; set; }

        /// <summary>
        /// Gets or sets the record for printing.
        /// </summary>
        /// <value>
        /// An object that contains the record for the row.
        /// </value>
        public object Record { get; set; }
    }

    #endregion internal classes

#if !WinRT

    /// <summary>
    /// Specifies how the pages of content are oriented on print area.
    /// </summary>
    public enum PrintOrientation
    {
        /// <summary>
        /// Standard orientation.
        /// </summary>
        Portrait,
        /// <summary>
        /// Content of page is rotated at 90 degrees counterclockwise from standard (portrait) orientation.
        /// </summary>
        Landscape
    }

#endif
}