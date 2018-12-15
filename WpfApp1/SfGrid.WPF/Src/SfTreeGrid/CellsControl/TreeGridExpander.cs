#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
#if UWP
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridExpander : Control, IDisposable
    {
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TreeGridExpander), new PropertyMetadata(false, OnIsExpandedChanged));

        private static void OnIsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var expander = obj as TreeGridExpander;
            expander.SetExpanderState();
        }

        internal SfTreeGrid treeGrid;
        public TreeGridExpander()
        {
            this.DefaultStyleKey = typeof(TreeGridExpander);
            this.IsTabStop = false;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            SetExpanderState(false);
        }

        private void SetExpanderState(bool canApplyDefaultState = true)
        {
            if (IsExpanded)
                VisualStateManager.GoToState(this, "Expanded", false);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "Collapsed", false);
        }

#if WPF
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Handled) return;
            if (!this.treeGrid.ValidationHelper.CheckForValidation(true))
                return;
            IsExpanded = !IsExpanded;
            e.Handled = true;
            base.OnMouseUp(e);
        }
#else
        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (e.Handled) return;
            if (!this.treeGrid.ValidationHelper.CheckForValidation(true))
                return;
            IsExpanded = !IsExpanded;
            //To restrict the editing while clicking on expander
            e.Handled = true;
        }
        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            if (e.Handled) return;
            if (!this.treeGrid.ValidationHelper.CheckForValidation(true))
                return;
            IsExpanded = !IsExpanded;
            //To restrict the editing while clicking on expander
            e.Handled = true;
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (!treeGrid.AllowSelectionOnExpanderClick)
                e.Handled = true;
            base.OnPointerPressed(e);
        }
#endif

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridExpander"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridExpander"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.treeGrid = null;
            }
        }

    }
}
