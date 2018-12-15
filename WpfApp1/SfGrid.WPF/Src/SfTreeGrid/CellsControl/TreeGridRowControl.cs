#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using System;
using Syncfusion.Data;
using System.Collections.Generic;
using System.Linq;
#if UWP
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Data;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Shapes;
using System.Windows.Data;
using System.Windows.Media.Animation;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{

    /// <summary>
    /// Represents the class to provide the common functionalities for TreeGridCell and TreeGridRowControl. 
    /// </summary>
    public class TreeGridElement : ContentControl
    {
        /// <summary>
        /// Gets or sets a brush that will be applied as the foreground of selected row.
        /// </summary>
        /// <value>
        /// The brush that will be applied as the foreground of selected row.The default value is Black.
        /// </value>
        public Brush SelectionBackground
        {
            get { return (Brush)GetValue(SelectionBackgroundProperty); }
            set { SetValue(SelectionBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.TreeGridElement.SelectionBackground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.TreeGridElement.SelectionBackground dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SelectionBackgroundProperty =
            DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(TreeGridElement), new PropertyMetadata(new SolidColorBrush(Colors.Gray)));


        /// <summary>
        /// Gets or sets a brush that highlights the foreground of currently selected row.
        /// </summary>
        /// <value>
        /// The brush that highlights the foreground of selected row.The default value is Black.
        /// </value>
        public Brush SelectionForeground
        {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridElement.SelectionForeground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridElement.SelectionForeground dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(TreeGridElement), new PropertyMetadata(SfTreeGrid.GridSelectionForgroundBrush, OnSelectionForegroundChanged));

        private static void OnSelectionForegroundChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as TreeGridElement).OnSelectionForegroundChanged(e);
        }

        internal virtual void OnSelectionForegroundChanged(DependencyPropertyChangedEventArgs e)
        {

        }
    }

    public class TreeGridRowControlBase : TreeGridElement, IDisposable
    {
        internal protected Panel ItemsPanel;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TreeGridRowControlBase()
        {
            this.IsTabStop = false;
            SetContent();
            WireEvents();
        }

        /// <summary>
        /// Gets or sets the TreeDataRowBase which maintains the information about the row and it's level.
        /// </summary>
        public TreeDataRowBase DataRow
        {
            get { return (TreeDataRowBase)GetValue(DataRowProperty); }
            set { SetValue(DataRowProperty, value); }
        }

        public static readonly DependencyProperty DataRowProperty =
            DependencyProperty.Register("DataRow", typeof(TreeDataRowBase), typeof(TreeGridRowControlBase), new PropertyMetadata(null));

        protected virtual void WireEvents()
        {
            this.Loaded += OnLoaded;
            this.SizeChanged += OnSizeChanged;
        }

        protected virtual void UnwireEvents()
        {
            this.Loaded -= OnLoaded;
            this.SizeChanged -= OnSizeChanged;
        }

        protected virtual void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {

        }

        protected virtual void OnLoaded(object sender, RoutedEventArgs e)
        {

        }

        protected virtual void SetContent()
        {
            this.Content = this.ItemsPanel = new TreeGridRowPanel();
            (this.ItemsPanel as TreeGridRowPanel).GetDataRow = () => DataRow;
        }

        internal virtual void UpdateIndentMargin()
        {

        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowControlBase"/> class.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowControlBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isdisposing)
        {
            UnwireEvents();
            if (this.ItemsPanel != null)
            {
                if (ItemsPanel is IDisposable)
                    (this.ItemsPanel as IDisposable).Dispose();
                this.ItemsPanel = null;
            }
            this.DataRow = null;
        }

    }

    public class TreeGridRowControl : TreeGridRowControlBase
    {

        #region Ctor

        public TreeGridRowControl()
            : base()
        {
            this.DefaultStyleKey = typeof(TreeGridRowControl);
        }

#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()

#endif
        {
            base.OnApplyTemplate();
            this.UpdatedSelectionState(false);
            this.ApplyValidationVisualState(false);
        }

        #endregion

        internal bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        internal static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TreeGridRowControl), new PropertyMetadata(false,
                (obj, args) =>
                {
                    var rowcontrol = obj as TreeGridRowControl;
                    if (rowcontrol == null)
                        return;
                    rowcontrol.UpdatedSelectionState();
                }));

        protected void UpdatedSelectionState(bool canApplyDefaultState = true)
        {
            var isSelected = this.IsSelected;
            var isFocused = this.IsFocusedRow;

            if (isSelected)
            {
                this.UpdateIndentMargin();
                VisualStateManager.GoToState(this, "Selected", false);
            }
            else if (isFocused)
                VisualStateManager.GoToState(this, "Focused", false);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "Unselected", false);

            if (isSelected)
            {
                var treeGrid = DataRow.TreeGrid;
                if (SelectionForeground != SfTreeGrid.GridSelectionForgroundBrush)
                    Foreground = SelectionForeground;
                else if (treeGrid.SelectionForeground != SfTreeGrid.GridSelectionForgroundBrush)
                    Foreground = treeGrid.SelectionForeground;
#if UWP           
                else if (ForegroundProperty.GetMetadata(typeof(FrameworkElement)).DefaultValue == Foreground)
                    Foreground = SfTreeGrid.GridSelectionForgroundBrush;
#endif
            }
            else
                ClearValue(TreeGridRowControl.ForegroundProperty);
        }

        internal override void OnSelectionForegroundChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsSelected)
                Foreground = SelectionForeground;
        }

        internal bool IsFocusedRow
        {
            get { return (bool)GetValue(IsFocusedRowProperty); }
            set { SetValue(IsFocusedRowProperty, value); }
        }

        internal static readonly DependencyProperty IsFocusedRowProperty =
            DependencyProperty.Register("IsFocusedRow", typeof(bool), typeof(TreeGridRowControl), new PropertyMetadata(false, (obj, args) =>
            {
                var rowcontrol = obj as TreeGridRowControl;
                if (rowcontrol == null)
                    return;
                rowcontrol.UpdatedSelectionState();
            }));

        /// <summary>
        /// Gets or sets a margin value to avoid the overlapping in Indent Cell and excluding expander based on level.
        /// </summary>
        /// <remarks>
        /// It is used in Selection border and background border.
        /// </remarks>
        public Thickness IndentMargin
        {
            get { return (Thickness)GetValue(IndentMarginProperty); }
            set { SetValue(IndentMarginProperty, value); }
        }

        public static readonly DependencyProperty IndentMarginProperty =
            DependencyProperty.Register("IndentMargin", typeof(Thickness), typeof(TreeGridRowControl), new PropertyMetadata(new Thickness(0)));

        internal override void UpdateIndentMargin()
        {
            if (this.DataRow == null)
                return;

            double indentWidth = 0;
            if (this.DataRow.TreeGrid.RowIndentMode == RowIndentMode.Level)
            {
                var firstColumnIndex = this.DataRow.TreeGrid.ResolveToStartColumnIndex();
                var expanderColumnIndex = this.DataRow.TreeGrid.expanderColumnIndex;
                if (firstColumnIndex == expanderColumnIndex)
                {
                    indentWidth = (this.DataRow.Node.Level * this.DataRow.TreeGrid.TreeGridColumnSizer.ExpanderWidth) + this.DataRow.TreeGrid.TreeGridColumnSizer.ExpanderWidth + 3;
                    if (this.DataRow.TreeGrid.ShowCheckBox)
                        indentWidth += this.DataRow.TreeGrid.TreeGridColumnSizer.CheckBoxWidth;
#if WPF
                    else
                        indentWidth -= 1;
#endif


                    // To adjust the indentWidth while scrolling horizontally.
                    var line = DataRow.GetColumnVisibleLineInfo(expanderColumnIndex);
                    if (line != null)
                    {
                        if (line.IsClippedOrigin)
                        {
                            var originDifference = line.ClippedOrigin - line.Origin;
                            if (originDifference > indentWidth)
                                indentWidth = 0;
                            else
                                indentWidth -= originDifference;
                        }
                    }
                    else
                        indentWidth = 0;
                }
            }
            double origin = this.DataRow.GetVisibleLineOrigin();
            var margin = IndentMargin;
            margin.Left = origin + indentWidth;
            IndentMargin = margin;
        }

        private bool HasError;
        internal void SetError()
        {
            this.HasError = true;
            ApplyValidationVisualState();
        }

        internal void RemoveError()
        {
            this.HasError = false;
            ApplyValidationVisualState();
        }

        internal void ApplyValidationVisualState(bool canApplyDefaultState = true)
        {
            if (HasError)
                VisualStateManager.GoToState(this, "HasError", true);
            else if (canApplyDefaultState)
                VisualStateManager.GoToState(this, "NoError", true);
        }
    }
    public class TreeGridHeaderRowControl : TreeGridRowControlBase
    {
        #region Ctor

        public TreeGridHeaderRowControl()
            : base()
        {
            this.DefaultStyleKey = typeof(TreeGridHeaderRowControl);
        }

        #endregion
    }
}
