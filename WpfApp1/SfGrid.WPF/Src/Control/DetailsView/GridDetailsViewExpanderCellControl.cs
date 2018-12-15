#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
#if !WinRT && !UNIVERSAL
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Automation.Peers;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public class GridDetailsViewExpanderCell : Control, IDisposable
    {

        internal DataColumnBase columnBase;
        private bool useTransitions = false;
        private bool isdisposed = false;
#if WPF
        private bool canExpand = false;
#endif

        static GridDetailsViewExpanderCell()
        {
#if WPF
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GridDetailsViewExpanderCell), new FrameworkPropertyMetadata(typeof(GridDetailsViewExpanderCell)));
#endif
        }

        public GridDetailsViewExpanderCell()
        {
#if !WPF
            DefaultStyleKey = typeof (GridDetailsViewExpanderCell);
#endif
        }

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(GridDetailsViewExpanderCell), new PropertyMetadata(false, OnIsExpandedPropertyChanged));

        public Visibility ExpanderIconVisibility
        {
            get { return (Visibility)GetValue(ExpanderIconVisibilityProperty); }
            set { SetValue(ExpanderIconVisibilityProperty, value); }
        }

        public static readonly DependencyProperty ExpanderIconVisibilityProperty =
            DependencyProperty.Register("ExpanderIconVisibility", typeof(Visibility), typeof(GridDetailsViewExpanderCell), new PropertyMetadata(Visibility.Visible));

        public RowColumnIndex RowColumnIndex { get; internal set; }

        internal SfDataGrid DataGrid;

        private static void OnIsExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var expander = d as GridDetailsViewExpanderCell;
            if (expander != null) expander.SetExpanderState();
        }

        internal bool SuspendChangedAction;
        private void SetExpanderState()
        {
            if (DataGrid != null && DataGrid.DetailsViewManager != null && !SuspendChangedAction)
                DataGrid.DetailsViewManager.OnDetailsViewExpanderStateChanged(RowColumnIndex, IsExpanded);
            //WPF-19992 - Collapse operation may be cancelled. so Visual state change moved here
            VisualStateManager.GoToState(this, IsExpanded ? "Expanded" : "Collapsed", useTransitions);
        }

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            var lovalvalue = ReadLocalValue(IsExpandedProperty);
            if (lovalvalue != DependencyProperty.UnsetValue)
                SetExpanderState();
        }

#if UWP
        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            this.Focus(FocusState.Programmatic);
            if(columnBase!=null)
                this.columnBase.RaisePointerPressed(e);
            base.OnPointerPressed(e);
        }
        protected override void OnPointerReleased(PointerRoutedEventArgs e)
        {
            if(columnBase!=null)
                this.columnBase.RaisePointerReleased(e);
            base.OnPointerReleased(e);
        }

        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (e.Handled) return;
            if (ExpanderIconVisibility != Visibility.Visible) return;
            if (!this.DataGrid.ValidationHelper.CheckForValidation(true))
                return;
            useTransitions = true;
            IsExpanded = !IsExpanded;
            useTransitions = false;
            base.OnTapped(e);
        }

        protected override void OnPointerMoved(PointerRoutedEventArgs e)
        {
            if (columnBase != null)
                columnBase.RaisePointerMoved(e);
            base.OnPointerMoved(e);
        }
#else
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (columnBase != null)
                columnBase.RaisePointerPressed(e);
            canExpand = true;
            base.OnPreviewMouseDown(e);
        }

        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (columnBase != null)
                columnBase.RaisePointerReleased(e);
            base.OnPreviewMouseUp(e);
        }

        protected override void OnPreviewMouseMove(MouseEventArgs e)
        {
            if (columnBase != null)
                columnBase.RaisePointerMoved(e);
            base.OnPreviewMouseMove(e);
        }

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            if (ExpanderIconVisibility != Visibility.Visible) return;
            if (columnBase != null)
                this.columnBase.OnTapped(e);
            if (canExpand && (ValidationHelper.IsCurrentCellValidated || ValidationHelper.IsCurrentRowValidated))
            {
                useTransitions = true;
                IsExpanded = !IsExpanded;
                useTransitions = false;
                canExpand = false;
            }
            base.OnMouseUp(e);
        }
#endif

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridDetailsViewExpanderCell"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridDetailsViewExpanderCell"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                DataGrid = null;
                columnBase = null;
            }
            isdisposed = true;
        }

# if WPF
        # region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridDetailsViewExpanderCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }
}
