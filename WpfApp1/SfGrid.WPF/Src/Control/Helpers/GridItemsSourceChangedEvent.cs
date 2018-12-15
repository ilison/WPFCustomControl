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
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.Data;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSourceChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GridItemsSourceChangedEventHandler(object sender,GridItemsSourceChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSourceChanged"/> event.
    /// </summary>
    public class GridItemsSourceChangedEventArgs:GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        /// <param name="oldItemsSource">
        /// The old ItemsSource of the SfDataGrid.
        /// </param>
        /// <param name="newItemsSource">
        /// The new ItemsSource of the SfDataGrid.
        /// </param>
        /// <param name="oldView">
        /// The old View of the SfDataGrid.
        /// </param>
        /// <param name="newView">
        /// The new View of the SfDataGrid.
        /// </param>
        public GridItemsSourceChangedEventArgs(object originalSource,object oldItemsSource, object newItemsSource, ICollectionViewAdv oldView, ICollectionViewAdv newView)
            : base(originalSource)
        {
            this.OldItemsSource = oldItemsSource;
            this.NewItemsSource = newItemsSource;
            this.OldView = oldView;
            this.NewView = newView;
        }
        
        /// <summary>
        /// Gets an old ItemsSource of SfDataGrid that is need to be replaced.
        /// </summary>
        /// <value>
        /// An object that contains the old ItemsSource of SfDataGrid.
        /// </value>
        public object OldItemsSource { get; internal set; }

        /// <summary>
        /// Gets the new ItemsSource of SfDataGrid.
        /// </summary>
        /// <value>
        /// An object that contains the new ItemsSource of SfDataGrid.
        /// </value>
        public object NewItemsSource { get; internal set; }

        /// <summary>
        /// Gets the old View of SfDataGrid.
        /// </summary>
        /// <value>
        /// An object that contains the old View of SfDataGrid.
        /// </value>
        public ICollectionViewAdv OldView { get; internal set; }

        /// <summary>
        /// Gets the new View of SfDataGrid.
        /// </summary>
        /// <value>
        /// An object that contains the new View of SfDataGrid.
        /// </value>
        public ICollectionViewAdv NewView { get; internal set; }
    }
}
