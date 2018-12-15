#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.ComponentModel;
using Syncfusion.Data;

namespace Syncfusion.UI.Xaml.Grid
{
    #region Event Args & Handlers
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupExpanding"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCollapsing"/> events.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GroupChangingEventHandler(object sender, GroupChangingEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupExpanding"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCollapsing"/> events.
    /// </summary>
    public class GroupChangingEventArgs: GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GroupChangingEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the corresponding group that is being expanded or collapsed in view.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Data.Group"/> that is being changed in view.
        /// </value>
        public Group Group
        {
            get;
            internal set;
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupExpanded"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCollapsed"/> events.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.GroupChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void GroupChangedEventHandler(object sender, GroupChangedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupExpanded"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCollapsed"/> events.
    /// </summary>
    public class GroupChangedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GroupChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GroupChangedEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the corresponding group that is expanded or collapsed in view.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Data.Group"/> that is being changed in view.
        /// </value>
        public Group Group
        {
            get;
            internal set;
        }
    }

    #endregion
}
