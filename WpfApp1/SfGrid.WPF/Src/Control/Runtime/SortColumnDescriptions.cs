#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#else
using System.Windows;
using System.ComponentModel;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a collection of sorting column description that raises notification for both collection and item changes.
    /// </summary>
    /// <remarks>
    /// You can add many <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> instance to perform sorting.
    /// </remarks>
    [ClassReference(IsReviewed = false)]
    public class SortColumnDescriptions : ObservableCollection<SortColumnDescription>
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescriptions"/>  class.
        /// </summary>
        public SortColumnDescriptions()
        {

        }
        #region Property
        /// <summary>
        /// Gets the column from the <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescriptions"/> collection at the specified column name.
        /// </summary>
        /// <param name="columnName">
        /// The columnName to retrieve the corresponding column.
        /// </param>
        /// <returns>
        /// Returns the corresponding column from the <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescriptions"/> collection.
        /// </returns>
        public SortColumnDescription this[string columnName]
        {
            get
            {
                var column = this.FirstOrDefault(col => col.ColumnName == columnName);
                return column;
            }
        }

        #endregion
    }

    /// <summary>
    /// Describes a sorting criterion.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class SortColumnDescription : DependencyObject
    {
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SortColumnDescription.ColumnName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SortColumnDescription.ColumnName dependency property.
        /// </remarks>  
        public static readonly DependencyProperty ColumnNameProperty = DependencyProperty.Register("ColumnName", typeof(string), typeof(SortColumnDescription), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the name of the column.
        /// </summary>
        /// <value>
        /// A string that specifies the name of the column. The default value is null.
        /// </value>
        public string ColumnName
        {
            get
            {
                return (string)this.GetValue(ColumnNameProperty);
            }

            set
            {
                this.SetValue(ColumnNameProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SortColumnDescription.SortDirection dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SortColumnDescription.SortDirection dependency property.
        /// </remarks>  
        public static readonly DependencyProperty SortDirectionProperty = DependencyProperty.Register("SortDirection", typeof(ListSortDirection), typeof(SortColumnDescription), new PropertyMetadata(ListSortDirection.Ascending));

        /// <summary>
        /// Gets or sets the sort direction.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.ComponentModel.ListSortDirection"/> enumeration that specifies the sort direction.
        /// </value>
        public ListSortDirection SortDirection
        {
            get
            {
                return (ListSortDirection)this.GetValue(SortDirectionProperty);
            }

            set
            {
                this.SetValue(SortDirectionProperty, value);
            }
        }

        //public IComparer<object> CustomComparer
        //{
        //    get { return (IComparer<object>)GetValue(CustomComparerProperty); }
        //    set { SetValue(CustomComparerProperty, value); }
        //}

        //public static readonly DependencyProperty CustomComparerProperty = DependencyProperty.Register("CustomComparer", typeof(IComparer<object>), typeof(GridDataSortColumn), new PropertyMetadata(null));

    }
}
