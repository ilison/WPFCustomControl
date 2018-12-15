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
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridRowHeaderCell : TreeGridCell
    {
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
            DependencyProperty.Register("RowIndex", typeof(int), typeof(TreeGridRowHeaderCell), new PropertyMetadata(0));

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

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowErrorMessage dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowErrorMessage dependency property.
        /// </remarks> 

        public static readonly DependencyProperty RowErrorMessageProperty =
            DependencyProperty.Register("RowErrorMessage", typeof(string), typeof(TreeGridRowHeaderCell), new PropertyMetadata(string.Empty));

        internal string State { get; set; }

        public TreeGridRowHeaderCell()
        {
            this.DefaultStyleKey = typeof(TreeGridRowHeaderCell);
            this.IsTabStop = false;
        }

        #region Overrides

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            ApplyVisualState(false);
        }

        #endregion

        public void ApplyVisualState(bool canApplyDefaultState = true)
        {
            switch (State)
            {
                case "CurrentRow":
                    VisualStateManager.GoToState(this, "CurrentRow", true);
                    break;
                case "EditingRow":
                    VisualStateManager.GoToState(this, "EditingRow", true);
                    break;
                case "Normal":
                    if (canApplyDefaultState)
                        VisualStateManager.GoToState(this, "Normal", true);
                    break;
                case "Error_CurrentRow":
                    VisualStateManager.GoToState(this, "Error_CurrentRow", true);
                    break;
                case "Error":
                    VisualStateManager.GoToState(this, "Error", true);
                    break;
            }
        }
    }

    public class TreeGridRowHeaderIndentCell : TreeGridCell
    {
        #region Ctor

        public TreeGridRowHeaderIndentCell()
        {
            this.DefaultStyleKey = typeof(TreeGridRowHeaderIndentCell);
            this.IsTabStop = false;
        }

        #endregion
    }
}
