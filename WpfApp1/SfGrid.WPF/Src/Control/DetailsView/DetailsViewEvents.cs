#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewExpanding"/> event.
    /// </summary>
    public class GridDetailsViewExpandingEventArgs : GridCancelEventArgs
    {
        public GridDetailsViewExpandingEventArgs(object dataGrid)
            : base(dataGrid)
        {

        }

        /// <summary>
        /// Gets the record.
        /// </summary>
        /// <value>The record.</value>
        public object Record { get; internal set; }

        /// <summary>
        /// Gets or sets the DetailsViewItemsSource.
        /// </summary>
        /// <value>Key: RelationalColumn.</value>
        /// <value>Value: DetailsViewItemsSource.</value>
        public Dictionary<string, IEnumerable> DetailsViewItemsSource { get; internal set; }
    }

    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewExpanding"/> event.
    /// </summary>
    public delegate void GridDetailsViewExpandingEventHandler(object sender, GridDetailsViewExpandingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewExpanded"/> event.
    /// </summary>
    public class GridDetailsViewExpandedEventArgs : GridEventArgs
    {
        public GridDetailsViewExpandedEventArgs(object dataGrid)
            : base(dataGrid)
        {

        }

        /// <summary>
        /// Gets the record.
        /// </summary>
        /// <value>The record.</value>
        public object Record { get; internal set; }

        /// <summary>
        /// Gets or sets the DetailsViewItemsSource.
        /// </summary>
        /// <value>Key: RelationalColumn.</value>
        /// <value>Value: DetailsViewItemsSource.</value>
        public Dictionary<string, IEnumerable> DetailsViewItemsSource { get; set; }
    }

    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewExpanded"/> event.
    /// </summary>
    public delegate void GridDetailsViewExpandedEventHandler(object sender, GridDetailsViewExpandedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewCollapsing"/> event.
    /// </summary>
    public class GridDetailsViewCollapsingEventArgs : GridCancelEventArgs
    {
        public GridDetailsViewCollapsingEventArgs(object dataGrid)
            : base(dataGrid)
        {

        }

        /// <summary>
        /// Gets the record.
        /// </summary>
        /// <value>The record.</value>
        public object Record { get; internal set; }
    }

    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewCollapsing"/> event.
    /// </summary>
    public delegate void GridDetailsViewCollapsingEventHandler(object sender, GridDetailsViewCollapsingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewCollapsed"/> event.
    /// </summary>
    public class GridDetailsViewCollapsedEventArgs : GridEventArgs
    {
        public GridDetailsViewCollapsedEventArgs(object dataGrid)
            : base(dataGrid)
        {

        }

        /// <summary>
        /// Gets the record.
        /// </summary>
        /// <value>The record.</value>
        public object Record { get; internal set; }
    }

    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewCollapsed"/> event.
    /// </summary>
    public delegate void GridDetailsViewCollapsedEventHandler(object sender, GridDetailsViewCollapsedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingRelations"/> event.
    /// </summary>
    public class AutoGeneratingRelationsArgs : GridCancelEventArgs
    {
        public GridViewDefinition GridViewDefinition { get; set; }

        public AutoGeneratingRelationsArgs(GridViewDefinition gridView, object originalSource)
            : base(originalSource)
        {
            GridViewDefinition = gridView;
        }
    }
    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingRelations"/> event.
    /// </summary>
    public delegate void AutoGeneratingRelationsEventHandler(object sender, AutoGeneratingRelationsArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewLoading"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewUnloading"/> events.
    /// </summary>    
    public class DetailsViewLoadingAndUnloadingEventArgs : GridEventArgs
    {
        public DetailsViewLoadingAndUnloadingEventArgs(object dataGrid, DetailsViewDataGrid grid)
            : base(dataGrid)
        {
            DetailsViewDataGrid = grid;
        }

        /// <summary>
        /// DetailsViewDataGrid present in the DetailsViewDataRow while expanding the parent record
        /// </summary>
        public DetailsViewDataGrid DetailsViewDataGrid { get; internal set; }
    }

    /// <summary>
    /// Provides the delegate for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewLoading"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewUnloading"/> events.
    /// </summary>
    public delegate void DetailsViewLoadingAndUnloadingEventHandler(object sender, DetailsViewLoadingAndUnloadingEventArgs e);
}
