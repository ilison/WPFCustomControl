#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.Generic;
using Syncfusion.UI.Xaml.ScrollAxis;

namespace Syncfusion.UI.Xaml.Grid
{
    #region Event Arguments & Handlers

    #region RecordDeleting Event Handler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleting"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.RecordDeletingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void RecordDeletingEventHandler(object sender, RecordDeletingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleting"/> event.
    /// </summary>
    public class RecordDeletingEventArgs : GridCancelEventArgs
    {
        #region Properties
        /// <summary>
        /// Gets or sets a list of object that is going to be removed from underlying source collection.
        /// </summary>
        /// <value>
        /// The list of object that is going to be removed from underlying source collection.
        /// </value>
        public List<object> Items
        {
            get;
            set;
        }
        #endregion
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="N:Syncfusion.UI.Xaml.Grid.RecordDeletingEventArgs"/> class. 
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>        
        public RecordDeletingEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        #endregion
    }
    #endregion

    #region RecordDeleted Event Handler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleted"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.RecordDeletedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void RecordDeletedEventHandler(object sender, RecordDeletedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleted"/> event.
    /// </summary>
    public class RecordDeletedEventArgs : GridEventArgs
    {
        #region Properties
        /// <summary>
        /// Gets or sets a list of object that were removed from underlying source collection.
        /// </summary>
        /// <value>
        /// The list of object that were removed from underlying source collection.
        /// </value>
        public List<object> Items
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the index of selected item.
        /// </summary>
        /// <value>
        /// The index of the selected item.
        /// </value>
        public int SelectedIndex
        {
            get;
            set;
        }
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="N:Syncfusion.UI.Xaml.Grid.RecordDeletedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public RecordDeletedEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        #endregion
    }
    #endregion
    #endregion
}
