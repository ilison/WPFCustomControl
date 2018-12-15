#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using System.ComponentModel;

namespace Syncfusion.UI.Xaml.ScrollAxis
{
    /// <summary>
    /// Defines an interface that provides all properties to configure a scrollbar.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public interface IScrollBar : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets or sets a value to be added to or subtracted from the value of the property when the scroll box is moved a large distance.
        /// </summary>
        double LargeChange { get; set; }

        /// <summary>
        /// Gets or sets the upper limit of values of the scrollable range.
        /// </summary>
        double Maximum { get; set; }

        /// <summary>
        /// Gets or sets the lower limit of values of the scrollable range.
        /// </summary>
        double Minimum { get; set; }

        /// <summary>
        /// Gets or sets the value to be added to or subtracted from the value of the property when the scroll box is moved a small distance.
        /// </summary>
        double SmallChange { get; set; }

        /// <summary>
        /// Gets or sets a numeric value that represents the current position of the scroll box on the scroll bar control.
        /// </summary>
        double Value { get; set; }

        /// <summary>
        /// Gets or sets a number that represents the current position of the scroll box on the scroll bar control.
        /// </summary>
        bool Enabled { get; set; }
    }
}
