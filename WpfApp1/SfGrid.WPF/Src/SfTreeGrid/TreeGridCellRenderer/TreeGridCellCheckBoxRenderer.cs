#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
#if WPF
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
#else
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class TreeGridCellCheckBoxRenderer : TreeGridVirtualizingCellRenderer<CheckBox, CheckBox>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellCheckBoxRenderer"/> class.
        /// </summary>
        public TreeGridCellCheckBoxRenderer()
        {
            SupportsRenderOptimization = false;
            IsEditable = false;
        }
        #endregion

        #region Override Methods

        #region Edit Binding Overrides
        /// <summary>
        /// Method which is initialize the Renderer element Bindings with corresponding column values.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Elements
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext"></param>
        /// <remarks></remarks>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, CheckBox uiElement, object dataContext)
        {
            RowColumnIndex rowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            var column = dataColumn.TreeGridColumn;
            BindingExpression = uiElement.GetBindingExpression(ToggleButton.IsCheckedProperty);
#if WPF
            uiElement.FocusVisualStyle = null;
            if (column.isValueMultiBinding)
            {
                var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
                if (!column.AllowEditing)
                    (binding as MultiBinding).Mode = BindingMode.OneWay;

                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }
            else
            {
                var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
                if (!column.AllowEditing)
                    (binding as Binding).Mode = BindingMode.OneWay;

                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }
#else

            var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            if (!column.AllowEditing)
                (binding as Binding).Mode = BindingMode.OneWay;            
          
            if ((column as TreeGridCheckBoxColumn).IsThreeState)
            {
                uiElement.SetBinding(TreeGridCellCheckBoxRenderer.IsCheckedProperty, binding);
                SetIsChecked(uiElement, uiElement.IsChecked);
            }
            else
                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);

#endif
            BindingExpression = uiElement.GetBindingExpression(ToggleButton.IsCheckedProperty);
            uiElement.SetValue(Control.PaddingProperty, column.Padding);
            uiElement.SetValue(FrameworkElement.VerticalAlignmentProperty, column.VerticalAlignment);          
#if UWP
            uiElement.MinHeight = 0;
            uiElement.MinWidth = 0;
#endif
            uiElement.IsEnabled = (column.AllowEditing && TreeGrid.SelectionMode != GridSelectionMode.None);
            var checkBoxColumn = column as TreeGridCheckBoxColumn;
            if (checkBoxColumn == null)
                return;
            uiElement.SetValue(FrameworkElement.HorizontalAlignmentProperty, checkBoxColumn.HorizontalAlignment);
            uiElement.SetValue(ToggleButton.IsThreeStateProperty, checkBoxColumn.IsThreeState);

        }

        /// <summary>
        /// Gets the IsChecked attached property value of CheckBox which is bound with underlying data object. 
        /// </summary>
        /// <remarks>
        /// IsChecked attached property used only when GridCheckBoxColumn.IsThreeState is true.
        /// </remarks>
        public static object GetIsChecked(DependencyObject obj)
        {
            return (object)obj.GetValue(IsCheckedProperty);
        }

        /// <summary>
        /// Sets the IsChecked attached property value of CheckBoox which is bound with underlying data object. 
        /// </summary>
        /// <remarks>
        /// IsChecked attached property used only when GridCheckBoxColumn.IsThreeState is true.
        /// </remarks>
        public static void SetIsChecked(DependencyObject obj, object value)
        {
            obj.SetValue(IsCheckedProperty, value);
        }

        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.RegisterAttached("IsChecked", typeof(object), typeof(TreeGridCellCheckBoxRenderer), new PropertyMetadata(null, OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = d as CheckBox;
            checkBox.IsChecked = (bool?)e.NewValue;
        }

        /// <summary>
        /// Method which is used to update the Renderer element Bindings with corresponding column values.
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="element">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for update the Binding
        /// <param name="dataContext"> dataContext of the row</param>
        /// <remarks></remarks>
        public override void OnUpdateEditBinding(TreeDataColumnBase dataColumn, CheckBox element, object dataContext)
        {
            OnInitializeEditElement(dataColumn, element, dataContext);
        }

        /// <summary>
        /// Initalize the Cell Style
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="record"></param>
        protected override void InitializeCellStyle(TreeDataColumnBase dataColumn, object record)
        {
            base.InitializeCellStyle(dataColumn, record);
        }


        #endregion

        #region Display/Edit Value Overrides
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            return HasCurrentCellState
                   ? CurrentCellRendererElement.GetValue(ToggleButton.IsCheckedProperty)
                   : base.GetControlValue();
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (HasCurrentCellState)
                ((CheckBox)CurrentCellRendererElement).IsChecked = (bool)value;
        }

        //Fix for IdataError based error is not updated while scrolling
        public override bool CanUpdateBinding(TreeGridColumn column)
        {
            return true;
            //  return column.GridValidationMode != GridValidationMode.None;
        }
        #endregion

        #region ShouldGridTryToHandleKeyDown
        /// <summary>
        /// Shoulds the grid try automatic handle key down.
        /// </summary>
        /// <param name="e">The <see cref="Windows.UI.Core.KeyEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState)
                return true;

            var CurrentCellUIElement = (CheckBox)CurrentCellRendererElement;
            TreeGridCell treeGridCell = null;
            if (CurrentCellUIElement.Parent is TreeGridCell)
                treeGridCell = CurrentCellUIElement.Parent as TreeGridCell;

            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    return true;
                case Key.Space:
                    {
                        if (!CurrentCellUIElement.IsEnabled) return true;
                        if (BindingExpression != null)
                            BindingExpression.UpdateSource();
#if WPF
                        CurrentCellUIElement.Focus();
#else
                        CurrentCellUIElement.Focus(FocusState.Programmatic);
#endif                       
                        return true;
                    }
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion

        #region EndEdit

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (CheckBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(CheckBox.IsCheckedProperty);
            }
            // WPF-25028 Updates the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.

            if (BindingExpression != null
#if WPF
                && BindingExpression.Status != BindingStatus.Detached
#endif
                )
                BindingExpression.UpdateSource();
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Invoked when the edit element(CheckBox) is loaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            ((CheckBox)sender).Click += OnCheckBoxClick;
#if WPF
            ((CheckBox)sender).PreviewMouseUp += OnMouseUp;
#endif
            base.OnEditElementLoaded(sender, e);
        }

        /// <summary>
        /// Invoked when the edit element(CheckBox) is unloaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((CheckBox)sender).Click -= OnCheckBoxClick;
#if WPF
            ((CheckBox)sender).PreviewMouseUp -= OnMouseUp;
#endif
        }

        protected override void OnUnwireEditUIElement(CheckBox uiElement)
        {
            uiElement.Click -= OnCheckBoxClick;
#if WPF
            uiElement.PreviewMouseUp -= OnMouseUp;
#endif
            base.OnUnwireEditUIElement(uiElement);
        }

        #endregion

        #endregion
        #region EventHandlers
#if WPF
        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
                return;


            if (HasCurrentCellState && !((CheckBox)sender).IsPressed && IsFocused)
            {
                ((CheckBox)sender).IsChecked = !((CheckBox)sender).IsChecked;

                TreeGridCell treeGridCell = null;

                var checkBox = sender as CheckBox;

                if (checkBox.Parent is TreeGridCell)
                    treeGridCell = checkBox.Parent as TreeGridCell;

                var rowColumnIndex = new RowColumnIndex();
                rowColumnIndex.RowIndex = treeGridCell != null ? treeGridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
                rowColumnIndex.ColumnIndex = treeGridCell != null ? treeGridCell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;

                if (!rowColumnIndex.IsEmpty)
                    TreeGrid.RaiseCurrentCellValueChangedEvent(new TreeGridCurrentCellValueChangedEventArgs(TreeGrid) { RowColumnIndex = rowColumnIndex, Record = checkBox.DataContext, Column = treeGridCell.ColumnBase.TreeGridColumn });
            }
        }
#endif

        /// <summary>
        /// Called when [CheckBox click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void OnCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            TreeGridCell treeGridCell = null;
            if (checkBox.Parent is TreeGridCell)
                treeGridCell = checkBox.Parent as TreeGridCell;


            var rowcolumnIndex = new RowColumnIndex();
            rowcolumnIndex.RowIndex = treeGridCell != null ? treeGridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
            rowcolumnIndex.ColumnIndex = treeGridCell != null ? treeGridCell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;

#if !WPF
            if (checkBox.IsThreeState)
                SetIsChecked(checkBox, checkBox.IsChecked);
            if (!rowcolumnIndex.IsEmpty)
            {
                this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowcolumnIndex);
                this.TreeGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowcolumnIndex);
            }
#endif
            TreeGrid.RaiseCurrentCellValueChangedEvent(new TreeGridCurrentCellValueChangedEventArgs(TreeGrid) { RowColumnIndex = rowcolumnIndex, Record = checkBox.DataContext, Column = treeGridCell.ColumnBase.TreeGridColumn });

            if (HasCurrentCellState)
            {
                BindingExpression = this.CurrentCellRendererElement.GetBindingExpression(ToggleButton.IsCheckedProperty);
            }
            //when check or uncheck the checkbox, setter property of checkbox column fires two times in the Model class.
            //while loading, we set the updatesourcetrigger as explicit for checkboxcolumn if we set updatetrigger as PropertyChanged in sample for checkboxcolumn.
            //so skips the below call by checking the updatesourcetrigger.
#if WPF
            if (treeGridCell.ColumnBase.TreeGridColumn.isValueMultiBinding)
            {
                if (BindingExpression != null && (treeGridCell != null && (treeGridCell.ColumnBase.TreeGridColumn.ValueBinding as MultiBinding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                    BindingExpression.UpdateSource();
            }
            else if (BindingExpression != null && (treeGridCell != null && (treeGridCell.ColumnBase.TreeGridColumn.ValueBinding as Binding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                BindingExpression.UpdateSource();
#else
            if (BindingExpression != null && (treeGridCell != null && (treeGridCell.ColumnBase.TreeGridColumn.ValueBinding as Binding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                BindingExpression.UpdateSource();
#endif
        }

        #endregion
    }
}
