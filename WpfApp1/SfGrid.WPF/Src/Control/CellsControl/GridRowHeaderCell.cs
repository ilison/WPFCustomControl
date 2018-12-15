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
using System.Windows;
#if UWP
using Windows.UI.Xaml;
#else
using System.Windows.Automation.Peers;
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class GridRowHeaderCell : GridCell
    {
        /// <summary>
        /// Gets or sets row validation error message.
        /// </summary>
        /// <value></value>
        /// <remarks>error message displayed while hover RowHeaderCell. </remarks>
        public string RowErrorMessage
        {
            get { return (string)GetValue(RowErrorMessageProperty); }
            set { SetValue(RowErrorMessageProperty, value); }
        }

        public static readonly DependencyProperty RowErrorMessageProperty =
            DependencyProperty.Register("RowErrorMessage", typeof(string), typeof(GridRowHeaderCell), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets RowIndex of the cell.
        /// </summary>
        /// <value></value>
        /// <remarks>can be used to number the row like excel</remarks>
        public int RowIndex
        {
            get { return (int)GetValue(RowIndexProperty); }
            set { SetValue(RowIndexProperty, value); }
        }

        public static readonly DependencyProperty RowIndexProperty =
            DependencyProperty.Register("RowIndex", typeof(int), typeof(GridRowHeaderCell), new PropertyMetadata(0));

        internal string State { get; set; }

        public GridRowHeaderCell()
        {
            this.DefaultStyleKey = typeof(GridRowHeaderCell);
            this.IsTabStop = false;
        }

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            ApplyVisualState();
            ApplyGridCellVisualStates(this.GridCellRegion, false);
#if WPF
            UnwireEvent();
            WireEvent();
#endif
        }

#if WPF

        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            e.Handled = false;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
#else

        protected override void OnTapped(Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            e.Handled = true;
        }

        protected override void OnDoubleTapped(Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs e)
        {
            e.Handled = true;
        }
#endif
#if WPF
        #region Automation Overrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridRowHeaderCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif

#endregion

        public void ApplyVisualState()
        {
            switch (State)
            {
                case "Error_CurrentRow":
                    VisualStateManager.GoToState(this, "Error_CurrentRow", true);
                    break;
                case "Error":
                    VisualStateManager.GoToState(this, "Error", true);
                    break;
                case "CurrentRow":
                    VisualStateManager.GoToState(this, "CurrentRow", true);
                    break;
                case "EditingRow":
                    VisualStateManager.GoToState(this, "EditingRow", true);
                    break;
                case "Normal":
                    VisualStateManager.GoToState(this, "Normal", true);
                    break;
                case "AddNewRow":
                    VisualStateManager.GoToState(this, "AddNewRow", true);
                    break;
                case "FilterRow":
                    VisualStateManager.GoToState(this, "FilterRow", true);
                    break;
            }
        }
    }

    [ClassReference(IsReviewed = false)]
    public class GridRowHeaderIndentCell : GridCell
    {
        #region Ctor

        public GridRowHeaderIndentCell()
        {
            this.DefaultStyleKey = typeof(GridRowHeaderIndentCell);
            this.IsTabStop = false;
        }

        #endregion
    }


}
