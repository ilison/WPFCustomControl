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
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AddNewRowInitiating"/> event.
    /// </summary>
    public class AddNewRowInitiatingEventArgs : GridEventArgs
    {              
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The SfDataGrid.
        /// </param>
        public AddNewRowInitiatingEventArgs(object originalSource):base(originalSource)
        {

        }        
        
        /// <summary>
        /// Gets or sets the new data object initialized for AddNewRow.
        /// </summary>
        /// <value>
        /// An object that will be added.
        /// </value>
        public object NewObject { get;  set; }        
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void AddNewRowInitiatingEventHandler(object sender, AddNewRowInitiatingEventArgs e);
}
