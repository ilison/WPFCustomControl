#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a cell of a print job.
    /// </summary>
    public class PrintGridCell : ContentControl, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintGridCell"/> class.
        /// </summary>
        public PrintGridCell()
        {
            DefaultStyleKey = typeof(PrintGridCell);
        }

        #region Dispose
        /// <summary>
        /// Releases all the resources used by <see cref="Syncfusion.UI.Xaml.Grid.PrintGridCell"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by <see cref="Syncfusion.UI.Xaml.Grid.PrintGridCell"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            
        }

        #endregion
                
    }

    /// <summary>
    /// Represents a header cell of a print job.
    /// </summary>
    public class PrintHeaderCell : PrintGridCell
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintHeaderCell"/> class.
        /// </summary>
        public PrintHeaderCell()
        {
            DefaultStyleKey = typeof(PrintHeaderCell);
        }

        #endregion
    }

    /// <summary>
    /// Represents a caption summary cell of a print job.
    /// </summary>
    public class PrintCaptionSummaryCell : PrintGridCell
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintCaptionSummaryCell"/> class.
        /// </summary>
        public PrintCaptionSummaryCell()
        {
            DefaultStyleKey = typeof(PrintCaptionSummaryCell);
        }

        #endregion
    }

    /// <summary>
    /// Represents a group summary cell of a print job.
    /// </summary>
    public class PrintGroupSummaryCell : PrintGridCell
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintGroupSummaryCell"/> class.
        /// </summary>
        public PrintGroupSummaryCell()
        {
            DefaultStyleKey = typeof(PrintGroupSummaryCell);
        }

        #endregion
    }

    /// <summary>
    /// Represents a table summary cell of a print job.
    /// </summary>
    public class PrintTableSummaryCell : PrintGridCell
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintTableSummaryCell"/> class.
        /// </summary>
        public PrintTableSummaryCell()
        {
            DefaultStyleKey = typeof(PrintTableSummaryCell);
        }

        #endregion
    }

    /// <summary>
    /// Represents a UnboundRow cell of a print job.
    /// </summary>
    public class PrintUnboundRowCell :PrintGridCell
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintUnboundRowCell"/> class.
        /// </summary>
        public PrintUnboundRowCell()
        {
            DefaultStyleKey = typeof(PrintUnboundRowCell);
        }

        #endregion
    }
}
