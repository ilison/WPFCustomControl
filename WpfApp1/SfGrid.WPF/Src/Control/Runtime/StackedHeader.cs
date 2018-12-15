#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
#if WinRT || UNIVERSAL
using Windows.UI;
using Windows.UI.Xaml;
using System.Threading.Tasks;
#endif

using System.Collections.ObjectModel;
using System.Windows;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a header row that contains the collection of stacked column to group the column under particular category.
    /// </summary>
    public class StackedHeaderRow : DependencyObject
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRow"/> class.
        /// </summary>
        public StackedHeaderRow()
        {
            SetValue(StackedColumnsProperty, new StackedColumns());
        }
        
        /// <summary>
        /// Gets or sets the name of the StackedHeaderRow.
        /// </summary>
        /// <value>
        /// A string that specifies the name of the StackedHeaderRow. The default value is <b>null</b>.
        /// </value>
        public string Name
        {
            get { return (string)GetValue(NameProperty); }
            set { SetValue(NameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.StackedHeaderRow.Name dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.StackedHeaderRow.Name dependency property.
        /// </remarks>  
        public static readonly DependencyProperty NameProperty =
            DependencyProperty.Register("Name", typeof(string), typeof(StackedHeaderRow), new PropertyMetadata(null));


        /// <summary>
        /// Gets the collection of the <see cref="Syncfusion.UI.Xaml.Grid.StackedColumn"/> to group under particular category.
        /// </summary>
        [Cloneable(false)]      
        public StackedColumns StackedColumns
        {
            get { return (StackedColumns)GetValue(StackedColumnsProperty); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.StackedHeaderRow.StackedColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.StackedHeaderRow.StackedColumns dependency property.
        /// </remarks>  
        public static readonly DependencyProperty StackedColumnsProperty =
            DependencyProperty.Register("StackedColumns", typeof(StackedColumns), typeof(StackedHeaderRow), new PropertyMetadata(new StackedColumns()));

    }

    internal delegate void ChildColumnsChanged();
    /// <summary>
    /// Represents a column that stacked across the specified child columns in it. 
    /// </summary>
    public class StackedColumn : DependencyObject
    {
        internal ChildColumnsChanged ChildColumnChanged;
        internal List<int> ChildColumnsIndex { get; set; }

        /// <summary>
        /// Gets or sets the name of child columns that need to be stacked under the specified stacked column.
        /// </summary>
        /// <value>
        /// A string that contains the column names to stacked under the particular category. The default value is <c>string.Empty</c>.
        /// </value>
        public string ChildColumns
        {
            get { return (string)GetValue(ChildColumnsProperty); }
            set { SetValue(ChildColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.StackedColumn.ChildColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.StackedColumn.ChildColumns dependency property.
        /// </remarks> 
        public static readonly DependencyProperty ChildColumnsProperty =
            DependencyProperty.Register("ChildColumns", typeof(string), typeof(StackedColumn), new PropertyMetadata(string.Empty,OnChildColumnsChanged));

        private static void OnChildColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var column = d as StackedColumn;
            if (column.ChildColumnChanged != null)
                column.ChildColumnChanged();
        }

        /// <summary>
        /// Gets or sets the header text of the stacked column.
        /// </summary>
        /// <value>
        /// A string that specifies the header text of the stacked column. The default value is null.
        /// </value>
        public string HeaderText
        {
            get { return (string)GetValue(HeaderTextProperty); }
            set { SetValue(HeaderTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.StackedColumn.HeaderText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.StackedColumn.HeaderText dependency property.
        /// </remarks> 
        public static readonly DependencyProperty HeaderTextProperty =
            DependencyProperty.Register("HeaderText", typeof(string), typeof(StackedColumn), new PropertyMetadata(null));

    }
    /// <summary>
    /// Represents a collection of <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRow"/> to add the stacked header row in SfDataGrid. 
    /// </summary>
    public class StackedHeaderRows : ObservableCollection<StackedHeaderRow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRows"/> class.
        /// </summary>
        public StackedHeaderRows()
        {

        }
    }

    /// <summary>
    /// Represents a collection of <see cref="Syncfusion.UI.Xaml.Grid.StackedColumn"/> to group the columns under particular category.
    /// </summary>
    public class StackedColumns : ObservableCollection<StackedColumn>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.StackedColumns"/> class.
        /// </summary>
        public StackedColumns()
        {
              
        }
    }
}
