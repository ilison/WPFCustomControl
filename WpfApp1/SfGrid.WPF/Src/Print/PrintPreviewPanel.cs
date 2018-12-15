#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Syncfusion.UI.Xaml.Grid
{

    public sealed class PrintPreviewPanel : Panel, IScrollInfo
    {

        #region Fields

        readonly Size InfiniteSize =
      new Size(double.PositiveInfinity, double.PositiveInfinity);
        PrintManagerBase printBase;
        double yPosition;
        double xPosition;
        int previousPageIndex;
        internal Action<int> SetPageIndex;
        internal Action InValidateParent;
        
        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewPanel"/> class.
        /// </summary>
        public PrintPreviewPanel()
        {

        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.PrintPreviewPanel"/> class.
        /// </summary>
        /// <param name="printBase">
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.PrintManagerBase"/>.
        /// </param>
        public PrintPreviewPanel(PrintManagerBase printBase)
        {
            SetPrintManagerBase(printBase);
        }

        #endregion 

        #region Child Property

        internal PrintPageControl Child { get; set; }

        #endregion

        #region Override
        /// <summary>
        /// Determines the desired size of the PrintPreviewPanel.
        /// </summary>
        /// <param name="availableSize">
        /// The size that the PrintPreviewPanel can occupy.
        /// </param>
        /// <returns>
        /// The desired size of SfDataGrid. 
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (Child == null)
                return availableSize;
            Child.Measure(InfiniteSize);
            var elementSize = Child.DesiredSize;
            var size = new Size
                {
                    Width = double.IsInfinity(availableSize.Width) ? elementSize.Width : availableSize.Width,
                    Height = double.IsInfinity(availableSize.Height) ? elementSize.Height : availableSize.Height
                };
            if (Child.DesiredSize.Height > availableSize.Height)
                UpdateScrollInfo(size, new Size(Child.DesiredSize.Width, Child.DesiredSize.Height * printBase.pageCount));
            else
                UpdateScrollInfo(size, new Size(Child.DesiredSize.Width, availableSize.Height * printBase.pageCount));

            return base.MeasureOverride(size);
        }

        /// <summary>
        /// Arranges the content of the PrintPreviewPanel.
        /// </summary>
        /// <param name="finalSize">
        /// The computed size that is used to arrange the content.
        /// </param>
        /// <returns>
        /// The size consumed by PrintPreviewPanel.
        /// </returns>
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Child == null)
                return finalSize;

            var xpos = (finalSize.Width - Child.DesiredSize.Width) / 2; ;
            var yPos = (VerticalOffset - ((int)(VerticalOffset / Child.DesiredSize.Height) * Child.DesiredSize.Height));

            xPosition = xpos < 0 ? -HorizontalOffset : xpos;
            if (Child.DesiredSize.Height > finalSize.Height)
            {
                var pageIndex = ((VerticalOffset + finalSize.Height) % Child.DesiredSize.Height) > 0 ? 1 : 0;
                pageIndex = (int)((VerticalOffset + finalSize.Height) / Child.DesiredSize.Height) + pageIndex;
                if (previousPageIndex != pageIndex && pageIndex >= 1 && pageIndex <= printBase.pageCount)
                {
                    printBase.CreatePage((int)pageIndex, Child);
                    previousPageIndex = (int)pageIndex;
                }
            }
            else
            {
                //WPF-21455 need to round of the value to avoid improper page index calculation.
                var pageIndex = ((int)Math.Round((VerticalOffset + finalSize.Height) / finalSize.Height));
                if (pageIndex > 0 && previousPageIndex != pageIndex && pageIndex <= printBase.pageCount)
                {
                    printBase.CreatePage(pageIndex, Child);
                    previousPageIndex = pageIndex;
                }
                else if (Child.PageIndex != 1 && VerticalOffset == 0)
                {
                    printBase.CreatePage(1, Child);
                    previousPageIndex = pageIndex;
                }
            }

            yPosition = ((VerticalOffset -
                          ((int) (VerticalOffset/Child.DesiredSize.Height)*Child.DesiredSize.Height)) +
                         finalSize.Height) > Child.DesiredSize.Height
                ? finalSize.Height > Child.DesiredSize.Height
                    ? finalSize.Height/2 - Child.DesiredSize.Height/2
                    : 0
                : -yPos;

            Child.Arrange(new Rect(xPosition, yPosition, Child.DesiredSize.Width,
                                   Child.DesiredSize.Height));

            if (Child.DesiredSize.Height > finalSize.Height)
                UpdateScrollInfo(finalSize, new Size(Child.DesiredSize.Width, Child.DesiredSize.Height * printBase.pageCount));
            else
                UpdateScrollInfo(finalSize, new Size(Child.DesiredSize.Width, finalSize.Height * printBase.pageCount));

            if (SetPageIndex != null) SetPageIndex(Child.PageIndex);

            return base.ArrangeOverride(finalSize);
        }

        #endregion

        #region Internal Methods

        internal void SetPrintManagerBase(PrintManagerBase printBase)
        {
            if(printBase == null)
                return;
            this.printBase = printBase;
            printBase.InValidatePreviewPanel = InValidate;
            printBase.InitializePrint(false);
            Child = printBase.CreatePage(1);
            Children.Add(Child);
        }

        #endregion

        #region Private Methods

        internal void UpdateScrollInfo(Size viewport, Size extent)
        {
            if (double.IsInfinity(viewport.Width))
                viewport.Width = extent.Width;

            if (double.IsInfinity(viewport.Height))
                viewport.Height = extent.Height;

            _Extent = extent;
            _Viewport = viewport;

            _Offset.X = Math.Max(0, Math.Min(_Offset.X, ExtentWidth - ViewportWidth));
            _Offset.Y = Math.Max(0, Math.Min(_Offset.Y, ExtentHeight - ViewportHeight));

            if (ScrollOwner != null)
                ScrollOwner.InvalidateScrollInfo();
        }

        /// <summary> 
        /// Invalidates the print preview.
        /// </summary>
        /// <param name="needToInitProperties">
        /// Indicates whether the reinitialize the print properties.
        /// </param>
        public void InValidate(bool needToInitProperties)
        {
            printBase.InitializePrint(needToInitProperties);
            if (printBase.pageCount <= 0) return;
            Children.Clear();
            Child = printBase.CreatePage(1, Child);
            Children.Add(Child);
            if (InValidateParent != null) InValidateParent();
        }

        #endregion

        #region IScrollInfo Members

        #region Fields

        private const double LineSize = 16;
        private const double WheelSize = 3 * LineSize;
        private bool _CanHorizontallyScroll;
        private bool _CanVerticallyScroll;
        private ScrollViewer _ScrollOwner;
        private Point _Offset;
        private Size _Extent;
        private Size _Viewport;

        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value that indicates whether the print preview panel can scroll horizontally.
        /// </summary>
        /// <value>
        /// <b>true</b> if th print preview panel scrolled horizontally; otherwise, <b>false</b>;
        /// </value>
        public bool CanHorizontallyScroll
        {
            get { return _CanHorizontallyScroll; }
            set { _CanHorizontallyScroll = value; }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the print preview panel can scroll vertically.
        /// </summary>
        /// <value>
        /// <b>true</b> if th print preview panel scrolled vertically; otherwise, <b>false</b>;
        /// </value>
        public bool CanVerticallyScroll
        {
            get { return _CanVerticallyScroll; }
            set { _CanVerticallyScroll = value; }
        }

        /// <summary>
        /// Gets the vertical size of the extent.
        /// </summary>
        public double ExtentHeight
        {
            get { return _Extent.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the extent.
        /// </summary>
        public double ExtentWidth
        {
            get { return _Extent.Width; }
        }

        /// <summary>
        /// Gets the horizontal offset of the print content.
        /// </summary>
        public double HorizontalOffset
        {
            get { return _Offset.X; }
        }

        /// <summary>
        /// Gets the vertical offset of the print content.
        /// </summary>
        public double VerticalOffset
        {
            get { return _Offset.Y; }
        }

        /// <summary>
        /// Gets the vertical size of the print content's viewport.
        /// </summary>
        public double ViewportHeight
        {
            get { return _Viewport.Height; }
        }

        /// <summary>
        /// Gets the horizontal size of the print content's viewport.
        /// </summary>
        public double ViewportWidth
        {
            get { return _Viewport.Width; }
        }

        /// <summary>
        /// Gets or sets a value that identifies the container that controls the scrolling behavior in print preview panel.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Controls.ScrollViewer"/> that control scrolling behavior in print preview panel.
        /// </value>
        public ScrollViewer ScrollOwner
        {
            get { return _ScrollOwner; }
            set { _ScrollOwner = value; }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Scrolls the contents of the print preview panel down by one line.
        /// </summary>
        public void LineDown()
        {
            SetVerticalOffset(VerticalOffset + LineSize);
        }

        /// <summary>
        /// Scrolls the contents of the print preview panel upward by one line.
        /// </summary>
        public void LineUp()
        {
            SetVerticalOffset(VerticalOffset - LineSize);
        }

        /// <summary>
        /// Scrolls the contents of the print preview panel to the left by one line.
        /// </summary>
        public void LineLeft()
        {
            SetHorizontalOffset(HorizontalOffset - LineSize);
        }

        /// <summary>
        /// Scrolls the contents of the print preview panel to the right by one line.
        /// </summary>
        public void LineRight()
        {
            SetHorizontalOffset(HorizontalOffset + LineSize);
        }

        /// <summary>
        /// Scrolls the content of print preview panel logically downward in response to a downward click of the mouse wheel button.
        /// </summary>
        public void MouseWheelDown()
        {
            SetVerticalOffset(VerticalOffset + WheelSize);
        }

        /// <summary>
        /// Scrolls the content of print preview panel logically upward in response to an upward click of the mouse wheel button.
        /// </summary>
        public void MouseWheelUp()
        {
            SetVerticalOffset(VerticalOffset - WheelSize);
        }

        /// <summary>
        /// Scrolls the content of print preview panel logically to the left in response to a left click of the mouse wheel button.
        /// </summary>
        public void MouseWheelLeft()
        {
            SetHorizontalOffset(HorizontalOffset - WheelSize);
        }

        /// <summary>
        /// Scrolls the content of print preview panel logically to the right in response to a right click of the mouse wheel button.
        /// </summary>
        public void MouseWheelRight()
        {
            SetHorizontalOffset(HorizontalOffset + WheelSize);
        }

        /// <summary>
        /// Scrolls the content of print preview panel downward by one page.
        /// </summary>
        public void PageDown()
        {
            SetVerticalOffset(VerticalOffset + ViewportHeight);
        }

        /// <summary>
        /// Scrolls the content of print preview panel upward by one page.
        /// </summary>
        public void PageUp()
        {
            SetVerticalOffset(VerticalOffset - ViewportHeight);
        }

        /// <summary>
        /// Scrolls the content of print preview panel to the left by one page.
        /// </summary>
        public void PageLeft()
        {
            SetHorizontalOffset(HorizontalOffset - ViewportWidth);
        }

        /// <summary>
        /// Scrolls the content of print preview panel to the right by one page.
        /// </summary>
        public void PageRight()
        {
            SetHorizontalOffset(HorizontalOffset + ViewportWidth);
        }

#if WPF
        /// <summary>
        /// Scrolls to the specified coordinates and makes that part of a <see cref="System.Windows.Media.Visual"/> visible.
        /// </summary>
        /// <param name="visual">
        /// The <see cref="System.Windows.Media.Visual"/> that becomes visible.
        /// </param>
        /// <param name="rectangle">
        /// The <see cref="System.Windows.Rect"/> that represents coordinate space within a visual.
        /// </param>
        /// <returns></returns>
        public Rect MakeVisible(Visual visual, Rect rectangle)
#endif
        {
            return rectangle;
        }

        /// <summary>
        /// Sets the amount of horizontal offset.
        /// </summary>
        /// <param name="offset">
        /// The degree to which content is horizontally offset from the containing viewport.
        /// </param>
        public void SetHorizontalOffset(double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentWidth - ViewportWidth));
            if (offset != _Offset.Y)
            {
                _Offset.X = offset;
                InvalidateArrange();
            }
        }

        /// <summary>
        /// Sets the amount of vertical offset.
        /// </summary>
        /// <param name="offset">
        /// The degree to which content is horizontally offset from the containing viewport.
        /// </param>
        public void SetVerticalOffset(double offset)
        {
            offset = Math.Max(0, Math.Min(offset, ExtentHeight - ViewportHeight));
            if (offset != _Offset.Y)
            {
                _Offset.Y = offset;
                InvalidateArrange();
            }
        }

        #endregion

        #endregion

        

    }
    
}

