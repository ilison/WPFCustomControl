#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using Syncfusion.UI.Xaml.Collections.ComponentModel;
using Syncfusion.Data;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanging"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanged"/> events.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridFilterEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridFilterEventHandler(object sender, GridFilterEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanging"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanged"/> events.
    /// </summary>
    public class GridFilterEventArgs : GridHandledEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridFilterEventArgs"/> class.
        /// </summary>
        /// <param name="column">
        /// The column related to the filter operation.
        /// </param>
        /// <param name="filterpredicates">
        /// The list of filter predicates collection.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridFilterEventArgs(GridColumn column, List<FilterPredicate> filterpredicates, object originalSource)
            : base(originalSource)
        {
            this.column = column;
            this.filterpredicates = filterpredicates;
        }

        
        private GridColumn column;
        /// <summary>
        /// Gets the column related to the filter operation.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> that represents the column that is being filtered.
        /// </value>
        public GridColumn Column
        {
            get
            {
                return column;
            }
        }

        private List<FilterPredicate> filterpredicates;
        
        /// <summary>
        /// Gets the list of filter predicates that defines a set of criteria to filter the data.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.Data.FilterPredicate"/> collection.
        /// </value>
        public List<FilterPredicate> FilterPredicates
        {
            get
            {
                return filterpredicates;
            }
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterItemsPopulating"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridFilterItemsPopulatingEventHandler(object sender, GridFilterItemsPopulatingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterItemsPopulating"/> event.
    /// </summary>
    public class GridFilterItemsPopulatingEventArgs : GridHandledEventArgs
    {

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatingEventArgs"/> class.
        /// </summary>
        /// <param name="itemsSource">
        /// The collection of filter element that is being populated to the filter control.
        /// </param>
        /// <param name="column">
        /// The corresponding column related to the event.
        /// </param>
        /// <param name="filterControl">
        /// The corresponding filter control related to the event.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridFilterItemsPopulatingEventArgs(IEnumerable<FilterElement> itemsSource, GridColumn column, GridFilterControl filterControl, object originalSource)
            : base(originalSource)
        {
            this.ItemsSource = itemsSource;
            this.column = column;
            this.FilterControl = filterControl;
        }

      
        private GridColumn column;
        /// <summary>
        /// Gets the column related to the event.
        /// </summary>
        /// <value>
        /// A <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> related to the event.
        /// </value>
        public GridColumn Column
        {
            get
            {
                return column;
            }
        }

        /// <summary>
        /// Gets or sets the collection of filter element that is being populated as an ItemsSource of filter control.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.FilterElement"/> that is being populated as an ItemsSource of filter control.
        /// </value>
        public IEnumerable<FilterElement> ItemsSource { get; set; }

        /// <summary>
        /// Gets the filter control where the filter items are being loaded.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/> where the filter items are being loaded.
        /// </value>
        public GridFilterControl FilterControl { get; internal set; }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterItemsPopulated"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridFilterItemsPopulatedEventHandler(object sender, GridFilterItemsPopulatedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterItemsPopulated"/> event.
    /// </summary>
    public class GridFilterItemsPopulatedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatedEventArgs"/> class.
        /// </summary>
        /// <param name="itemsSource">
        /// The list of filter element that were loaded in filter control.
        /// </param>
        /// <param name="column">
        /// The corresponding column related to the event.
        /// </param>
        /// <param name="filterControl">
        /// The corresponding filter control where the filter items are loaded.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridFilterItemsPopulatedEventArgs(IEnumerable<FilterElement> itemsSource, GridColumn column, GridFilterControl filterControl, object originalSource)
            : base(originalSource)
        {
            this.ItemsSource = itemsSource;
            this.column = column;
            this.FilterControl = filterControl;
        }

       
        private GridColumn column;
        /// <summary>
        /// Gets the column related to the event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/>  related to the event.
        /// </value>
        public GridColumn Column
        {
            get
            {
                return column;
            }
        }

        /// <summary>
        /// Gets or sets the collection of filter element that were loaded as an ItemsSource of filter control.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.FilterElement"/> that were loaded in filter control.
        /// </value>
        public IEnumerable<FilterElement> ItemsSource { get; set; }

        /// <summary>
        /// Gets the filter control where the filter items are loaded.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/> where the filter items are loaded.
        /// </value>
        public GridFilterControl FilterControl { get; internal set; }
    }
}
