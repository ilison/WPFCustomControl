#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller.AutoScrollerValueChanged"/> event.
    /// </summary>
    public sealed class AutoScrollerValueChangedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScrollerValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="isLineUp">
        /// Indicates when the mouse point is dragged upward direction.
        /// </param>
        /// <param name="isLineDown">
        /// Indicates when the mouse point is dragged downward direction.
        /// </param>
        /// <param name="isLineLeft">
        /// Indicates when the mouse point is dragged towards left direction.
        /// </param>
        /// <param name="isLineRight">
        /// Indicates when the mouse point is dragged towards right direction.
        /// </param>
        /// <param name="_rowColumnIndex">
        /// Contains the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> related to the mouse released point.        
        /// </param>
        /// <param name="originalSender">
        /// The source of the event.
        /// </param>
        public AutoScrollerValueChangedEventArgs(bool isLineUp, bool isLineDown, bool isLineLeft, bool isLineRight, RowColumnIndex _rowColumnIndex, object originalSender)
            : base(originalSender)
        {
            this.isLineUp = isLineUp;
            this.isLineDown = isLineDown;
            this.isLineLeft = isLineLeft;
            this.isLineRight = isLineRight;
            this.rowColumnIndex = _rowColumnIndex;
        }

        private RowColumnIndex rowColumnIndex;

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the mouse released point during dragging selection.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> related to the mouse released point during selection dragging operation.
        /// </value>        
        public RowColumnIndex RowColumnIndex
        {
            get { return rowColumnIndex; }
            set { rowColumnIndex = value; }
        }


        private bool isLineLeft;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse point is dragged towards left.
        /// </summary>
        /// <value>
        /// <b>true</b> if the mouse point is dragged towards left direction; otherwise, <b>false</b>.
        /// </value>
        public bool IsLineLeft
        {
            get { return isLineLeft; }
            private set { isLineLeft = value; }
        }

        private bool isLineRight;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse point is dragged towards right.
        /// </summary>
        /// <value>
        /// <b>true</b> if the mouse point is dragged towards right direction; otherwise, <b>false</b>.
        /// </value>
        public bool IsLineRight
        {
            get { return isLineRight; }
            private set { isLineRight = value; }
        }

        private bool isLineUp;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse point is dragged towards up.
        /// </summary>
        /// <value>
        /// <b>true</b> if the mouse point is dragged upward direction; otherwise, <b>false</b>.
        /// </value>
        public bool IsLineUp
        {
            get { return isLineUp; }
            private set { isLineUp = value; }
        }


        private bool isLineDown;

        /// <summary>
        /// Gets or sets a value indicating whether the mouse point is dragged towards down.
        /// </summary>
        /// <value>
        /// <b>true</b> if the mouse point is dragged downward direction; otherwise, <b>false</b>.
        /// </value>
        public bool IsLineDown
        {
            get { return isLineDown; }
            private set { isLineDown = value; }
        }
    }
}
