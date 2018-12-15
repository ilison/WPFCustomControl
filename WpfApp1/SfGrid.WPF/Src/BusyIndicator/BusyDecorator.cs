#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Threading;

namespace Syncfusion.UI.Xaml.Grid
{
    #region ProgressRing

    /// <summary>
    /// The control which will be displayed to indicate the control is in busy state while loading data or doing some operations like Filtering, Grouping, Sorting.
    /// </summary>
    public class ProgressRing : Control
    {

        #region Constructor

        static ProgressRing()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressRing), new FrameworkPropertyMetadata(typeof(ProgressRing)));
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the text to be displayed in the <see cref="ProgressRing"/>
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the TextProperty dependency property
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ProgressRing), new PropertyMetadata(string.Empty));

        #endregion

    }

    #endregion

    #region BusyDecorator

    /// <summary>
    /// Class which is used to host the <see cref="ProgressRing"/> in visual using Thread.
    /// </summary>
    
    [StyleTypedProperty(Property = "BusyStyle", StyleTargetType = typeof(Control))]
    public class BusyDecorator : Decorator
    {
        #region Private Fields

        /// <summary>
        /// Helper used for running the Visual in background thread
        /// </summary>
        private ThreadedVisualHelper threadHelper;

        /// <summary>
        /// Represents a visual that can be connected to a parent visual tree
        /// </summary>
        private HostVisual hostVisual;

        #endregion      

        #region Dependency properties

        /// <summary>
        /// Gets or sets if the BusyIndicator is being shown.
        /// </summary>
        public bool IsBusyIndicatorShowing
        {
            get { return (bool)GetValue(IsBusyIndicatorShowingProperty); }
            set { SetValue(IsBusyIndicatorShowingProperty, value); }
        }

        /// <summary>
        /// Identifies the IsBusyIndicatorShowing dependency property.
        /// </summary>
        public static readonly DependencyProperty IsBusyIndicatorShowingProperty = DependencyProperty.Register(
            "IsBusyIndicatorShowing", 
            typeof(bool), 
            typeof(BusyDecorator), 
            new FrameworkPropertyMetadata(false,
                FrameworkPropertyMetadataOptions.AffectsMeasure, OnIsBusyIndicatorShowingProeprtyChanged));

        private static void OnIsBusyIndicatorShowingProeprtyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var container = (BusyDecorator)d;
            if (container.Content != null)
            {
                if ((bool)e.NewValue)
                {
                    container.ShowProgressRing();
                }
                else
                {
                    container.HideProgressRing();
                }
            }
        }                

        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Background.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(BusyDecorator), new PropertyMetadata(Brushes.Transparent));        
        
        ///<summary>
        /// Identifies the <see cref="BusyStyle" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyStyleProperty =
            DependencyProperty.Register(
            "BusyStyle",
            typeof(Style),
            typeof(BusyDecorator),
            new FrameworkPropertyMetadata(OnBusyStyleChanged));

        /// <summary>
        /// Gets or sets the Style to apply to the Control that is displayed as the busy indication.
        /// </summary>
        public Style BusyStyle
        {
            get { return (Style)GetValue(BusyStyleProperty); }
            set { SetValue(BusyStyleProperty, value); }
        }

        static void OnBusyStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BusyDecorator bd = (BusyDecorator)d;
            Style nVal = (Style)e.NewValue;
            var verticalAlignment = bd.BusyVerticalAlignment;
            var horizontalAlignment = bd.BusyHorizontalAlignment;
            bd.Content = () => new ProgressRing { Style = nVal, HorizontalAlignment = horizontalAlignment, VerticalAlignment = verticalAlignment};
        }        
        
        ///<summary>
        /// Identifies the <see cref="BusyHorizontalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyHorizontalAlignmentProperty = DependencyProperty.Register(
          "BusyHorizontalAlignment",
          typeof(HorizontalAlignment),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(HorizontalAlignment.Center));

        /// <summary>
        /// Gets or sets the HorizontalAlignment to use to layout the control that contains the busy indicator control.
        /// </summary>
        public HorizontalAlignment BusyHorizontalAlignment
        {
            get { return (HorizontalAlignment)GetValue(BusyHorizontalAlignmentProperty); }
            set { SetValue(BusyHorizontalAlignmentProperty, value); }
        }        
        
        ///<summary>
        /// Identifies the <see cref="BusyVerticalAlignment" /> property.
        /// </summary>
        public static readonly DependencyProperty BusyVerticalAlignmentProperty = DependencyProperty.Register(
          "BusyVerticalAlignment",
          typeof(VerticalAlignment),
          typeof(BusyDecorator),
          new FrameworkPropertyMetadata(VerticalAlignment.Center));

        /// <summary>
        /// Gets or sets the the VerticalAlignment to use to layout the control that contains the busy indicator.
        /// </summary>
        public VerticalAlignment BusyVerticalAlignment
        {
            get { return (VerticalAlignment)GetValue(BusyVerticalAlignmentProperty); }
            set { SetValue(BusyVerticalAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the CreateContent dependency property.
        /// </summary>
        internal static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(CreateContentFunction), typeof(BusyDecorator), null);

        /// <summary>
        /// Gets or sets the visual content to be displayed in a background thread.
        /// </summary>
        internal CreateContentFunction Content
        {
            get { return (CreateContentFunction)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        #endregion

        static BusyDecorator()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(BusyDecorator),
                new FrameworkPropertyMetadata(typeof(BusyDecorator)));
        }

        #region Ctor

        public BusyDecorator()
        {
            Content = () => new ProgressRing();
        }

        #endregion   

        #region override property
        /// <summary>
        /// Gets the count of Visual Children
        /// </summary>
        protected override int VisualChildrenCount
        {
            get
            {
                return IsBusyIndicatorShowing ? 1 : 0;
            }
        }

        #endregion

        #region override methods
        /// <summary>
        /// Gets the Visual Child
        /// </summary>
        /// <param name="index"></param>
        /// <returns>visual child which need to be displayed</returns>
        protected override Visual GetVisualChild(int index)
        {
            return hostVisual;
        }

        /// <summary>
        /// Measures the size of Busy Indicator
        /// </summary>
        /// <param name="constraint"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size constraint)
        {
            if (threadHelper != null)
                return threadHelper.DesiredSize;
            return Size.Empty;
        }

        #endregion    

        #region Private Methods

        /// <summary>
        /// Used to display the Busy Indicator
        /// </summary>
        private void ShowProgressRing()
        {
            threadHelper = new ThreadedVisualHelper(Content, InvalidatesMeasure);
            hostVisual = threadHelper.HostVisual;
        }

        /// <summary>
        /// Used to call the invalidate measure in dispatcher thread
        /// </summary>
        private void InvalidatesMeasure()
        {
            Dispatcher.BeginInvoke(new Action(InvalidateMeasure), DispatcherPriority.Loaded);
        }

        /// <summary>
        /// Used to hide the Busy Indicator that is currently displayed
        /// </summary>
        private void HideProgressRing()
        {
            if (threadHelper != null)
            {
                threadHelper.Exit();
                threadHelper = null;
                InvalidateMeasure();
            }
        }

        #endregion
    }

    #endregion

    #region VisualTargetPresentationSource

    /// <summary>
    /// Presentation source used to connect one visual tree to another visual tree
    /// </summary>    
    
    public class VisualTargetPresentationSource : PresentationSource
    {
        private VisualTarget _visualTarget;
        private bool _isDisposed = false;

        public VisualTargetPresentationSource(HostVisual hostVisual)
        {
            _visualTarget = new VisualTarget(hostVisual);
            AddSource();
        }

        public Size DesiredSize { get; private set; }

        public override Visual RootVisual
        {
            get { return _visualTarget.RootVisual; }
            set
            {
                Visual oldRoot = _visualTarget.RootVisual;

                // Set the root visual of the VisualTarget.  This visual will
                // now be used to visually compose the scene.
                _visualTarget.RootVisual = value;

                // Tell the PresentationSource that the root visual has
                // changed.  This kicks off a bunch of stuff like the
                // Loaded event.
                RootChanged(oldRoot, value);

                // Kickoff layout...
                UIElement rootElement = value as UIElement;
                if (rootElement != null)
                {
                    rootElement.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                    rootElement.Arrange(new Rect(rootElement.DesiredSize));

                    DesiredSize = rootElement.DesiredSize;
                }
                else
                    DesiredSize = new Size(0, 0);
            }
        }

        protected override CompositionTarget GetCompositionTargetCore()
        {
            return _visualTarget;
        }

        public override bool IsDisposed
        {
            get { return _isDisposed; }
        }

        internal void Dispose()
        {
            RemoveSource();
            _isDisposed = true;
        }
    }
    
    #endregion

    #region CreateContentFunction

    /// <summary>
    /// Represents a visual, to avoid threading exceptions delegate is used
    /// </summary>
    /// <returns></returns>
    public delegate Visual CreateContentFunction();

    #endregion

    #region ThreadedVisualHelper

    /// <summary>
    /// Performs background threading for the Visual
    /// </summary>
    
    internal class ThreadedVisualHelper : IDisposable
    {
        #region Fields

        /// <summary>
        /// Represents a visual that can be connected into parent visual tree
        /// </summary>
        private HostVisual hostVisual = null;

        /// <summary>
        /// Used to notify the waiting thread, that an event has occurred 
        /// </summary>
        private AutoResetEvent resetEvent = new AutoResetEvent(false);

        /// <summary>
        /// Content to be displayed in the visual
        /// </summary>
        private CreateContentFunction content;

        /// <summary>
        /// Used to run an action. InvalidateMeasure is called when the action is performed
        /// </summary>
        private Action invalidateMeasure;

        /// <summary>
        /// Used for running the thread in queue
        /// </summary>
        private Dispatcher Dispatcher { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Initialize instance for ThreadVisualHelper class
        /// </summary>
        /// <param name="content">Content to be displayed in visual</param>
        /// <param name="invalidateMeasure">InvalidateMeasure will be called when the action is performed</param>
        internal ThreadedVisualHelper(CreateContentFunction content, Action invalidateMeasure)
        {
            hostVisual = new HostVisual();
            this.content = content;
            this.invalidateMeasure = invalidateMeasure;

            Thread backgroundUi = new Thread(DisplayProgressRing);
            backgroundUi.SetApartmentState(ApartmentState.STA);
            backgroundUi.Name = "BackgroundVisualHostThread";
            backgroundUi.IsBackground = true;
            backgroundUi.Start();

            resetEvent.WaitOne();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Represents a visual object that can be connected to a parent visual tree
        /// </summary>
        internal HostVisual HostVisual { get { return hostVisual; } }

        /// <summary>
        /// Gets or sets the desired size of busy indicator
        /// </summary>
        internal Size DesiredSize { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Used to stop the currently running thread
        /// </summary>
        internal void Exit()
        {
            Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);
        }

        /// <summary>
        /// Used to display the busy indicator in visual using presentation source
        /// </summary>
        private void DisplayProgressRing()
        {
            Dispatcher = Dispatcher.CurrentDispatcher;
            VisualTargetPresentationSource source = new VisualTargetPresentationSource(hostVisual);
            resetEvent.Set();
            source.RootVisual = content();
            DesiredSize = source.DesiredSize;
            invalidateMeasure();

            Dispatcher.Run();
            source.Dispose();
        }

        #endregion

        #region IDisposable

        /// <summary>
        /// Used to dispose the instances that are maintained to avoid memory leaks
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if(isDisposing)
            {
                if (resetEvent != null)
                {
                    resetEvent.Dispose();
                    resetEvent = null;
                }
                hostVisual = null;
                content = null;
                resetEvent = null;
                invalidateMeasure = null;
                Dispatcher = null;
            }
        }

        #endregion

    }    

    #endregion
}
