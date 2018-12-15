#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
#if WPF
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Windows.Media;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
    public class TreeGridExpanderCell : TreeGridCell, IDisposable
    {
        public TreeGridExpanderCell()
        {
            this.DefaultStyleKey = typeof(TreeGridExpanderCell);
        }
        private TreeGridExpander PART_ExpanderCell;
        private CheckBox PART_SelectCheckBox;
#if WPF
        public override void OnApplyTemplate()
#else
        protected override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            PART_ExpanderCell = GetTemplateChild("PART_ExpanderCell") as TreeGridExpander;
            PART_SelectCheckBox = GetTemplateChild("PART_SelectCheckBox") as CheckBox;
            if (PART_SelectCheckBox != null)
            {
                PART_SelectCheckBox.IsChecked = (bool?)IsChecked;
            }
            if (PART_ExpanderCell != null)
                PART_ExpanderCell.treeGrid = this.ColumnBase.TreeGrid;
            UnWireCheckBoxEvent();
            WireCheckBoxEvent();
        }

        private void UnWireCheckBoxEvent()
        {
            if (PART_SelectCheckBox != null)
            {
                PART_SelectCheckBox.Click -= SelectCheckBox_Click;
#if UWP
                PART_SelectCheckBox.DoubleTapped -= SelectCheckBox_DoubleTapped;
                PART_SelectCheckBox.Tapped -= PART_SelectCheckBox_Tapped;
#endif
            }
        }

        private void WireCheckBoxEvent()
        {
            if (PART_SelectCheckBox != null)
            {
                PART_SelectCheckBox.Click += SelectCheckBox_Click;
#if UWP
                PART_SelectCheckBox.DoubleTapped += SelectCheckBox_DoubleTapped;
                PART_SelectCheckBox.Tapped += PART_SelectCheckBox_Tapped;
#endif
            }
        }

#if UWP
        private void SelectCheckBox_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            // while double tapping, cell goes to edit mode. to avoid this, it needs to be handled here.
            e.Handled = true;
        }

        private void PART_SelectCheckBox_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (this.ColumnBase == null || ColumnBase.TreeGrid == null)
                return;

            if (ColumnBase.TreeGrid.CheckBoxSelectionMode == CheckBoxSelectionMode.Default)
                e.Handled = true;
        }
#endif
        private void SelectCheckBox_Click(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;
#if UWP
            if (!this.ColumnBase.TreeGrid.ValidationHelper.CheckForValidation(true))
            {
                checkBox.IsChecked = (bool?)this.IsChecked;
                return;
            }
            this.ColumnBase.TreeGrid.SelectionController.CurrentCellManager.RaiseCellValidationAndEndEdit(true);
#endif
            if (this.ColumnBase.TreeGrid.CheckBoxSelectionMode != CheckBoxSelectionMode.Default && this.ColumnBase.TreeGrid.SelectionMode == GridSelectionMode.None)
            {
                checkBox.IsChecked = (bool?)this.IsChecked;
                return;
            }

            SetIsChecked(checkBox.IsChecked);
            ColumnBase.TreeGrid.NodeCheckBoxController.ChangeNodeState(this.ColumnBase.DataRow.Node, checkBox.IsChecked);
        }

        private void SetIsChecked(bool? isChecked)
        {
            if (!ColumnBase.TreeGrid.AllowTriStateChecking)
            {
                if (isChecked == null)
                    isChecked = false;
            }
            PART_SelectCheckBox.IsChecked = isChecked;
        }

        /// <summary>
        /// Set TreeGridExpander's IsExpanded while canceling expand/collapse operation in event.
        /// </summary>        
        internal void SetExpanderState()
        {
            if (PART_ExpanderCell != null && PART_ExpanderCell.IsExpanded != IsExpanded)
                PART_ExpanderCell.IsExpanded = IsExpanded;
        }
        public bool HasChildNodes
        {
            get { return (bool)GetValue(HasChildNodesProperty); }
            set { SetValue(HasChildNodesProperty, value); }
        }

        public static readonly DependencyProperty HasChildNodesProperty =
            DependencyProperty.Register("HasChildNodes", typeof(bool), typeof(TreeGridExpanderCell), new PropertyMetadata(false));

        internal int Level
        {
            get { return (int)GetValue(LevelProperty); }
            set { SetValue(LevelProperty, value); }
        }

        internal static readonly DependencyProperty LevelProperty =
            DependencyProperty.Register("Level", typeof(int), typeof(TreeGridExpanderCell), new PropertyMetadata(0, OnLevelChanged));

        private static void OnLevelChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGridCell = obj as TreeGridExpanderCell;
            if (treeGridCell == null || treeGridCell.ColumnBase == null)
                return;
            var expanderWidth = treeGridCell.ColumnBase.TreeGrid.TreeGridColumnSizer.ExpanderWidth;
            treeGridCell.IndentMargin = new Thickness(treeGridCell.Level * expanderWidth, 0, 0, 0);
        }

        public Thickness IndentMargin
        {
            get { return (Thickness)GetValue(IndentMarginProperty); }
            set { SetValue(IndentMarginProperty, value); }
        }

        public static readonly DependencyProperty IndentMarginProperty =
            DependencyProperty.Register("IndentMargin", typeof(Thickness), typeof(TreeGridExpanderCell), new PropertyMetadata(new Thickness(0)));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof(bool), typeof(TreeGridExpanderCell), new PropertyMetadata(false, OnIsExpandedChanged));

        /// <summary>
        /// Gets or sets a value which denotes whether node CheckBox enabled or not.
        /// </summary>
        /// <value>
        /// The default value is True.
        /// </value>
        /// <remarks>
        /// Using this property, you can conditionally enable or disable the node checkbox based on data.
        /// </remarks>
        public bool IsCheckBoxEnabled
        {
            get { return (bool)GetValue(IsCheckBoxEnabledProperty); }
            set { SetValue(IsCheckBoxEnabledProperty, value); }
        }


        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.TreeGridExpanderCell.IsCheckBoxEnabled dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.TreeGridExpanderCell.IsCheckBoxEnabled dependency property.
        /// </remarks>  
        public static DependencyProperty IsCheckBoxEnabledProperty =
            DependencyProperty.Register("IsCheckBoxEnabled", typeof(bool), typeof(TreeGridExpanderCell), new PropertyMetadata(true));

        internal object IsChecked
        {
            get { return (object)GetValue(IsCheckedProperty); }
            set { SetValue(IsCheckedProperty, value); }
        }

        internal static DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(object), typeof(TreeGridExpanderCell), new PropertyMetadata(null, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGridCell = obj as TreeGridExpanderCell;
            if (treeGridCell == null || treeGridCell.PART_SelectCheckBox == null)
                return;
            if (treeGridCell.PART_SelectCheckBox.IsChecked != (bool?)args.NewValue)
            {
                treeGridCell.PART_SelectCheckBox.IsChecked = (bool?)args.NewValue;
            }
        }

#if WPF
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            if (PART_SelectCheckBox != null)
            {
                // Need to do validation while clicking on CheckBox.
                if (PART_SelectCheckBox.GetControlRect(this).Contains(e.GetPosition(this)))
                {
                    //UWP-3325 To restrict the editing while double clicking on disabled checkbox  
                    if (!PART_SelectCheckBox.IsEnabled)
                        return;
                    if (!this.ColumnBase.TreeGrid.ValidationHelper.CheckForValidation(true))
                    {
                        e.Handled = true;
                        return;
                    }
                    this.ColumnBase.TreeGrid.SelectionController.CurrentCellManager.RaiseCellValidationAndEndEdit(true);
                    return;
                }
            }
            // To skip the selection if AllowSelectionOnExpanderClick is False.
            if (!CanAllowSelectionOnExpander(e))
                return;
            base.OnPreviewMouseDown(e);
        }

        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            // To skip the selection if AllowSelectionOnExpanderClick is False.
            if (!CanAllowSelectionOnExpander(e))
                return;
            base.OnMouseDown(e);
        }
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            if (IsCheckBoxClicked(e))
                return;
            // To skip the selection if AllowSelectionOnExpanderClick is False.
            if (!CanAllowSelectionOnExpander(e))
                return;
            base.OnPreviewMouseUp(e);
        }

        private bool CanAllowSelectionOnExpander(MouseButtonEventArgs e)
        {
            if (PART_ExpanderCell != null && this.ColumnBase != null && !this.ColumnBase.TreeGrid.AllowSelectionOnExpanderClick && PART_ExpanderCell.GetControlRect(this).Contains(e.GetPosition(this)))
                return false;
            return true;
        }

        /// <summary>
        /// Find whether CheckBox is clicked using MouseButtonEventArgs.
        /// </summary>
        /// <param name="e">MouseButtonEventArgs</param>
        /// <returns>true, if CheckBox is clicked. Else fales.</returns>

        private bool IsCheckBoxClicked(MouseButtonEventArgs e)
        {
            if (PART_SelectCheckBox != null)
            {
                // Need to skip selection if CheckBox is clicked.
                if (PART_SelectCheckBox.GetControlRect(this).Contains(e.GetPosition(this)))
                {
                    //UWP-3325 To restrict the editing while double clicking on disabled checkbox  
                    if (!PART_SelectCheckBox.IsEnabled)
                    {
                        e.Handled = true;
                    }
                    return true;
                }
            }
            return false;
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            // UWP-3325 To restrict the editing while double clicking on expander
            if (IsVisualObject(e) && GridUtil.FindDescendant(e.OriginalSource, typeof(TreeGridExpander)) != null)
                return;
            else if (IsCheckBoxClicked(e))
                return;
            base.OnMouseDoubleClick(e);
        }
        /// <summary>
        /// Check whether the Dependency object is Visual or Visual3D
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool IsVisualObject(MouseButtonEventArgs e)
        {
            DependencyObject depObj = e.OriginalSource as DependencyObject;
            return depObj is Visual || depObj is Visual3D;
        }
#endif

        internal bool SuspendChangedAction;

        /// <summary>
        /// Method will called when the property changed.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnIsExpandedChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGridCell = obj as TreeGridExpanderCell;
            if (treeGridCell.SuspendChangedAction)
                return;

            var isExpanded = (bool)args.NewValue;
            if (isExpanded)
                treeGridCell.ColumnBase.RaiseExpandNode();
            else
                treeGridCell.ColumnBase.RaiseCollapseNode();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (PART_ExpanderCell != null)
            {
                PART_ExpanderCell.Dispose();
                PART_ExpanderCell = null;
            }

            UnWireCheckBoxEvent();
        }
    }
}
