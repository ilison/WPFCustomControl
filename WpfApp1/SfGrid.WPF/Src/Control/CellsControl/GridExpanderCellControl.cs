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
using System.Text;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Controls;
using System.Windows;
#endif
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class GridExpanderCellControl : Control
    {

        #region Dependency Region

        /// <summary>
        /// Gets or sets a value indicating whether this instance Expanded or Collapse.
        /// </summary>
        /// <value><see langword="true"/> if this instance ; otherwise, <see langword="false"/>.</value>
        /// <remarks></remarks>
        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(GridExpanderCellControl), new PropertyMetadata(false, OnIsExpandedChanged));

        #endregion

        #region Dependency Call Back

        /// <summary>
        /// Method will called when the property changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnIsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var expander = obj as GridExpanderCellControl;
            expander.SetExpanderState();
        }

        #endregion

        #region Ctor

        public GridExpanderCellControl()
        {
            this.DefaultStyleKey = typeof(GridExpanderCellControl);
            this.IsTabStop = false;
        }

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            SetExpanderState(false);
        }

        #endregion

        #region Helper Methods

        private void SetExpanderState(bool canApplyDefaultState = true)
        {
            if (IsExpanded)
            {
                VisualStateManager.GoToState(this, "Expanded", true);
            }
            else if (canApplyDefaultState)
            {
                VisualStateManager.GoToState(this, "Collapsed", true);
            }
        }

        #endregion

    }
}
