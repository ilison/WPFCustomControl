#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.ComponentModel;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides data for SfDataGrid events.
    /// </summary>
    public class GridEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridEventArgs"/> class.
        /// </summary>
        /// <param name="originalSender">
        /// The original reporting sender that raised the event.
        /// </param>
        protected GridEventArgs(object originalSender)
        {
            OriginalSender = originalSender;
        }

        /// <summary>
        /// Gets the original reporting source that raised the event.
        /// </summary>
        public object OriginalSender { get; private set; }
    }

    /// <summary>
    /// Provides data for cancelable events in SfDataGrid.
    /// </summary>
    public class GridCancelEventArgs : CancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCancelEventArgs"/> class.
        /// </summary>
        /// <param name="originalSender">
        /// The original reporting source that raised the event.
        /// </param>
        protected GridCancelEventArgs(object originalSender)
        {
            OriginalSender = originalSender;
        }

        /// <summary>
        /// The original reporting source that raised the event.
        /// </summary>
        public object OriginalSender { get; private set; }
    }

    /// <summary>
    /// Provides data for events that can be handled completely in an event handler.
    /// </summary>
    public class GridHandledEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridHandledEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The original reporting source that raised the event.
        /// </param>
        public GridHandledEventArgs(object originalSource)
            : base(originalSource)
        {
            Handled = false;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridHandledEventArgs"/> class.
        /// </summary>
        /// <param name="handled">
        /// Indicates whether the event handled completely.
        /// </param>
        /// <param name="originalSource">
        /// The original reporting source that raised the event.
        /// </param>
        public GridHandledEventArgs(bool handled, object originalSource)
            : base(originalSource)
        {
            this.Handled = handled;
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the event handler has been handled completely and no further processing should be happened to the event.
        /// </summary>
        /// <value>
        /// <b>true</b> if the event handled completely; otherwise, <b>false</b>.
        /// </value>
        public bool Handled { get; set; }
    }
}
