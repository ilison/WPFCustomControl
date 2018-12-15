#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.ObjectModel;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Linq;
using System.Windows;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid
{    
    /// <summary>
    ///  Describes a grouping criterion.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GroupColumnDescription : DependencyObject
    {
        #region Dependency Properties

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// A string that specifies the name of the column. The default value is null.
        /// </value>
        public string ColumnName
        {
            get { return (string)GetValue(ColumnNameProperty); }
            set { SetValue(ColumnNameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.ColumnName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.ColumnName dependency property.
        /// </remarks>
        public static readonly DependencyProperty ColumnNameProperty =
            DependencyProperty.Register("ColumnName", typeof(string), typeof(GroupColumnDescription), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the converter for custom grouping.
        /// </summary>
        /// <value>
        /// The converter for custom grouping. The default value is null.
        /// </value>
        public IValueConverter Converter
        {
            get { return (IValueConverter)GetValue(ConverterProperty); }
            set { SetValue(ConverterProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.Converter dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.Converter dependency property.
        /// </remarks>
        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof(IValueConverter), typeof(GroupColumnDescription), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the comparer for the apply grouping based on custom logic.
        /// </summary>
        /// <value>
        /// The comparer for apply grouping based on custom logic. The default value is null.
        /// </value>
        public IComparer<object> Comparer
        {
            get { return (IComparer<object>)GetValue(ComparerProperty); }
            set { SetValue(ComparerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.Comparer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.Comparer dependency property.
        /// </remarks>
        public static readonly DependencyProperty ComparerProperty =
            DependencyProperty.Register("Comparer", typeof(IComparer<object>), typeof(GroupColumnDescription), new PropertyMetadata(null));        


        /// <summary>
        /// Gets or sets the value that indicates whether to sort the inner records of group while using custom grouping. 
        /// </summary>
        /// <value>
        /// The default value is false.
        /// </value>
        /// <remarks>        
        /// By default, grouped columns records are not sorted as the values of all the records in one group will be same. So, only groups will be sorted based on group key. 
        /// In custom grouping cases, grouped columns records value may differ. So in this case, you can sort the records of group by setting SortGroupRecords property to true. 
        /// </remarks>
        public bool SortGroupRecords
        {
            get { return (bool)GetValue(SortGroupRecordsProperty); }
            set { SetValue(SortGroupRecordsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.SortInnerElements dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GroupColumnDescription.SortInnerElements dependency property.
        /// </remarks>
        public static readonly DependencyProperty SortGroupRecordsProperty =
            DependencyProperty.Register("SortGroupRecords", typeof(bool), typeof(GroupColumnDescription), new PropertyMetadata(false));        


        #endregion
    }
    /// <summary>
    /// Represents a collection of group column description that raises notification for both collection and item changes.
    /// </summary>
    /// <remarks>
    /// You can add multiple <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescription"/> instance to perform grouping.
    /// </remarks>
    public class GroupColumnDescriptions : ObservableCollection<GroupColumnDescription>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescriptions"/> class.
        /// </summary>
        public GroupColumnDescriptions(): base()
        {
            
        }

        /// <summary>
        /// Gets the column from the <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescriptions"/> collection at the specified column name.
        /// </summary>
        /// <param name="columnName">
        /// The columnName to the retrieve the corresponding column.
        /// </param>
        /// <returns>
        /// Returns the corresponding column from the <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescriptions"/> collection.
        /// </returns>
        public GroupColumnDescription this[string columnName]
        {
            get
            {
                var column = this.FirstOrDefault(col => col.ColumnName == columnName);
                return column;
            }
        }

    }
}
