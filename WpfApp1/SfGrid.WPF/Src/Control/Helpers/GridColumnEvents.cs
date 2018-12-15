#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingColumn"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.AutoGeneratingColumnArgs"/> that contains the event data.
    /// </param>
    public delegate void AutoGeneratingColumnEventHandler(object sender, AutoGeneratingColumnArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingColumn"/> event.
    /// </summary>
    public class AutoGeneratingColumnArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Gets or sets the auto-generated column.
        /// </summary>
        /// <value>
        /// The auto-generated column.
        /// </value>
        public GridColumn Column { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.AutoGeneratingColumnArgs"/> class.
        /// </summary>
        /// <param name="column">
        /// The generated column.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public AutoGeneratingColumnArgs(GridColumn column, object originalSource)
            : base(originalSource)
        {
            Column = column;
        }
    }
}
