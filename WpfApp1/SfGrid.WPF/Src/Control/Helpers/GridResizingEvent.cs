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
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ResizingColumns"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.ResizingColumnsEventArgs"/> that contains the event data.
    /// </param>
    public delegate void ResizingColumnsEventHandler(object sender, ResizingColumnsEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ResizingColumns"/> event.
    /// </summary>
    public class ResizingColumnsEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.ResizingColumnsEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public ResizingColumnsEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the index of the column that is being resized.
        /// </summary>
        /// <value>
        /// An index of the column.
        /// </value>
        public int ColumnIndex
        {
            get;
            internal set;
        }

       /// <summary>
       /// Gets or sets the width of the column being resized.
       /// </summary>
       /// <value>
       /// The width of the column being resized.
       /// </value>
        public double Width
        {
            get;
            set;
        }
    }
}
