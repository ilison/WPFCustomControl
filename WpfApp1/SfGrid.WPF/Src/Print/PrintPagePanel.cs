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
#if WinRT || UNIVERSAL
using Windows.UI.Xaml.Controls;
using Windows.Foundation;
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
#endif
#if WPF
using System.Windows.Media;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a panel that arranges the cell content of SfDataGrid for print job.
    /// </summary>
    public class PrintPagePanel : Panel, IDisposable
    {
        private bool isdisposed = false;

#if WPF 
        bool AllowPrintByDrawing;
        Action<DrawingContext, List<RowInfo>> OnRenderBase;
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPagePanel"/> class.
        /// </summary>
        /// <param name="OnRender">
        /// Contains the visual and cell content to arrange it in page panel.
        /// </param>
        /// <param name="allowPrintByDrawing">
        /// Indicates whether the printing is based on drawing.
        /// </param>
        public PrintPagePanel(Action<DrawingContext, List<RowInfo>> OnRender,bool allowPrintByDrawing)
        {
            AllowPrintByDrawing = allowPrintByDrawing;
            OnRenderBase = OnRender;
        }
#endif
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPagePanel"/> class.
        /// </summary>
        public PrintPagePanel()
        {
        }

        #region Fields

        readonly Size InfiniteSize =
     new Size(double.PositiveInfinity, double.PositiveInfinity);
        private const double IndentWidth = 20d;

        #endregion

        internal List<RowInfo> RowsInfoList { get; set; }

        #region Overrides
        /// <summary>
        /// Determines the desired size of the page panel for printing .
        /// </summary>
        /// <param name="availableSize">
        /// The size that the page panel can occupy.
        /// </param>
        /// <returns>
        /// The desired size of page panel. 
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
#if WPF
            if (!AllowPrintByDrawing)
            {
#endif
                double curY = 0, curLineHeight = 0, maxLineWidth = 0;
                foreach (var cellsInfo in RowsInfoList.Select(rowInfo => rowInfo.CellsInfo))
                {
                    curY += curLineHeight;
                    double curX = 0;
                    curLineHeight = 0;
                    for (var i = 0; i < cellsInfo.Count; i++)
                    {
                        var child = cellsInfo[i].Element;
                        child.Measure(InfiniteSize);

                        curX += child.DesiredSize.Width;

                        if (child.DesiredSize.Height > curLineHeight)
                            curLineHeight = child.DesiredSize.Height;
                    }

                    if (curX > maxLineWidth)
                        maxLineWidth = curX;

                    curY += curLineHeight;
                }

                var size = new Size
                {
                    Width = double.IsInfinity(availableSize.Width) ? maxLineWidth : availableSize.Width,
                    Height = double.IsInfinity(availableSize.Height) ? curY : availableSize.Height
                };

                return size;
#if WPF
            }
            return availableSize;
#endif
        }

        /// <summary>
        /// Arranges the content of the page panel.
        /// </summary>
        /// <param name="finalSize">
        /// The computed size that is used to arrange the content.
        /// </param>
        /// <returns>
        /// The size consumed by page panel.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
#if WPF
            if (!AllowPrintByDrawing)
            {
#endif
                if (this.Children == null || this.Children.Count == 0)
                { return finalSize; }
                foreach (var cellsInfo in RowsInfoList.Select(rowInfo => rowInfo.CellsInfo))
                {
                
                    foreach (var cellInfo in cellsInfo)
                    {
                        var child = cellInfo.Element;
                        child.Arrange(cellInfo.CellRect);
                    }
                }
#if WPF
            }
#endif
            return finalSize;

        }
#if WPF        
        /// <summary>
        /// Draws the content of the specified DrawingContext object to page panel element .
        /// </summary>
        /// <param name="Drawingcontext">
        /// The corresponding Drawingcontext to draw.
        /// </param>
        protected override void OnRender(DrawingContext Drawingcontext)
        {
             if (AllowPrintByDrawing)
            {
                OnRenderBase(Drawingcontext, RowsInfoList);
            }
            else
                base.OnRender(Drawingcontext);
         }

#endif
        #endregion

        #region Dispose Member
        /// <summary>
        /// Releases all  the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPagePanel"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.PrintPagePanel"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if(isDisposing)
                RowsInfoList = null;
            isdisposed = true;
        }

        #endregion

    }
}
