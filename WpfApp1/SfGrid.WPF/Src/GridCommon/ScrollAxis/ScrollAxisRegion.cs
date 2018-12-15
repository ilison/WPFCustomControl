#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion

using Syncfusion.UI.Xaml.Grid;
namespace Syncfusion.UI.Xaml.ScrollAxis
{
    /// <summary>
    /// A scroll axis has three regions: Header, Body and Footer.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public enum ScrollAxisRegion
    {
        /// <summary>
        /// The header (at top or left side)
        /// </summary>
        Header,
        /// <summary>
        /// The body (center between header and footer)
        /// </summary>
        Body,
        /// <summary>
        /// The footer (at bottom or right side)
        /// </summary>
        Footer
    }

}
