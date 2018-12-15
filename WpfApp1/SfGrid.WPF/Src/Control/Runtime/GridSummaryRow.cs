#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.ObjectModel;
using Syncfusion.Data;
using System;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#else
using System.Windows;
using System.ComponentModel;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that defines the summary information of summary row.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridSummaryRow : DependencyObject, ISummaryRow
    {
        #region Fields

        private ObservableCollection<ISummaryColumn> summaryColumns;

        #endregion

        #region Dependency Registration

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryRow.Name dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryRow.Name dependency property.
        /// </remarks>   
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(GridSummaryRow), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryRow.ShowSummaryInRow dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryRow.ShowSummaryInRow dependency property.
        /// </remarks>   
        public static readonly DependencyProperty ShowSummaryInRowProperty = DependencyProperty.Register("ShowSummaryInRow", typeof(bool), typeof(GridSummaryRow), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title dependency property.
        /// </remarks>   
        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(GridSummaryRow), new PropertyMetadata(string.Empty));

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> class.
        /// </summary>
        public GridSummaryRow()
        {
            this.summaryColumns = new ObservableCollection<ISummaryColumn>();
        }

        #endregion

        #region ISummaryRow Members

        /// <summary>
        /// Gets or sets a value indicating whether this instance .
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        //public bool IsVisible
        //{
        //    get
        //    {
        //        return (bool)this.GetValue(IsVisibleProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(IsVisibleProperty, value);
        //    }
        //}

        /// <summary>
        /// Gets or sets the name of the summary row.
        /// </summary>
        /// <value>
        /// A string that specifies the name of the summary row. The default value is <c>string.Empty</c>.
        /// </value>                     
        public string Name
        {
            get
            {
                return (string)this.GetValue(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the value that indicates whether the summary value should be displayed in row or based on column.
        /// </summary>
        /// <value>
        /// <b>true</b> if the summary value displayed in a row based on <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title"/> property; otherwise, summary value is displayed in column based on <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryColumn.MappingName"/>. The default value is <b>true</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title"/> and <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Name"/> should have the same name if you want to display the summary value in row basis.
        /// </remarks>
        /// <example>
        ///    <code lang="C#"><![CDATA[        
        ///  this.dataGrid.TableSummaryRows.Add(new GridSummaryRow()
        ///  {
        ///     Name="Total Products",
        ///     ShowSummaryInRow = true,
        ///     Title = "Total Products Count: {ProductCount}",
        ///     SummaryColumns = new ObservableCollection<ISummaryColumn>()
        ///     {
        ///         new GridSummaryColumn()
        ///         {
        ///             Name="ProductCount",
        ///             MappingName="ProductName",
        ///             SummaryType=SummaryType.CountAggregate,
        ///             Format="{Count:d}"
        ///         },
        ///     }
        /// });
        /// ]]></code>
        /// </example>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Name"/>
        public bool ShowSummaryInRow
        {
            get
            {
                return (bool)this.GetValue(ShowSummaryInRowProperty);
            }
            set
            {
                this.SetValue(ShowSummaryInRowProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.Data.ISummaryColumn"/>.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.Data.ISummaryColumn"/>.
        /// </value>        
        public ObservableCollection<ISummaryColumn> SummaryColumns
        {
            get { return this.summaryColumns; }
            set { this.summaryColumns = value; }
        }

        /// <summary>
        /// Gets or sets the string that has the format and summary column information to be displayed in row. 
        /// </summary>
        /// <value>
        /// A string that specifies format and summary column name to parse and format summary display text for summary row.The default value is <c>string.Empty</c>.
        /// </value>
        /// <remarks>
        /// The summary row value is displayed based on <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title"/> when the <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.ShowSummaryInRow"/> is true and <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title"/> maps the summary value based on <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Name"/> property.
        /// </remarks>
        /// <seealso cref="yncfusion.UI.Xaml.Grid.GridSummaryRow.Name"/>
#if !WinRT && !UNIVERSAL
        [TypeConverter(typeof(GridSummaryFormatConverter))]
#endif
        public string Title
        {
            get
            {
                return (string)this.GetValue(TitleProperty);
            }
            set
            {
#if !WPF
                var formattedValue = value.SummaryFormatedString();
                this.SetValue(TitleProperty, formattedValue);
#else
                this.SetValue(TitleProperty, value);
#endif
            }
        }

        //public int TitleColumnCount
        //{
        //    get
        //    {
        //        return (int)this.GetValue(TitleColumnCountProperty);
        //    }
        //    set
        //    {
        //        this.SetValue(TitleColumnCountProperty, value);
        //    }
        //}

        #endregion
    }

    /// <summary>
    /// Represents a class that defines summary information of table summary row in SfDataGrid. 
    /// </summary>
    public class GridTableSummaryRow : GridSummaryRow,INotifyDependencyPropertyChanged
    {
        internal Action<GridTableSummaryRow, TableSummaryRowPosition> TableSummaryPositionChanged;
        private bool isdisposed = false;

        /// <summary>
        /// Gets or sets a value that indicates the position of table summary row in SfDataGrid. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowPosition"/> enumeration that specifies the position of table summary row.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowPosition.Bottom"/>.
        /// </value>
        public TableSummaryRowPosition Position
        {
            get { return (TableSummaryRowPosition)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        /// <summary>
        /// For notifying DetailsViewDataGrid, DataGrid instance is maintained
        /// </summary>
        internal SfDataGrid DataGrid
        {
            get;
            set;
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridTableSummaryRow.Position dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridTableSummaryRow.Position dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PositionProperty =
            GridDependencyProperty.Register("Position", typeof(TableSummaryRowPosition), typeof(GridTableSummaryRow), new GridPropertyMetadata(TableSummaryRowPosition.Bottom, OnTableSummaryPositionChanged));

        private static void OnTableSummaryPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var row = d as GridTableSummaryRow;
            if (row.TableSummaryPositionChanged != null)
                row.TableSummaryPositionChanged(row, (TableSummaryRowPosition)e.NewValue);
        }

        /// <summary>
        /// Invoked when the value of any dependency property in the table summary row has been changed.
        /// </summary>
        /// <param name="propertyName">
        /// The property name that has changed in table summary row.
        /// </param>
        /// <param name="e">
        /// <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the data for various dependency property changed events.
        /// </param>
        public void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataGrid == null) return;
            var index = this.DataGrid.TableSummaryRows.IndexOf(this);
            // Need to get NotifyListener from SourceDataGrid and call NotifyPropertyChanged method
            if (this.DataGrid.NotifyListener != null)
                this.DataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyPropertyChanged(this, propertyName, e, datagrid => datagrid.TableSummaryRows[index], DataGrid, typeof(GridTableSummaryRow));
        }

        /// <summary>
        /// Releases all the resources used by <see cref="Syncfusion.UI.Xaml.Grid.GridTableSummaryRow"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by <see cref="Syncfusion.UI.Xaml.Grid.GridTableSummaryRow"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                this.DataGrid = null;
                this.TableSummaryPositionChanged = null;
            }
            isdisposed = true;
        }
    }

}
