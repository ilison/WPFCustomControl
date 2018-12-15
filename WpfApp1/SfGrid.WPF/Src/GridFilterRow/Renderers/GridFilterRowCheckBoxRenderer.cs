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
using System.Windows.Input;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Data;
using Windows.UI.Core;
using Key = Windows.System.VirtualKey;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls.Primitives;
using Syncfusion.UI.Xaml.Controls.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
#endif
namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    
    /// <summary>
    /// Represents a class which handles the filter operation in CheckBoxColumn of a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowCheckBoxRenderer : GridFilterRowCellRenderer<CheckBox, CheckBox>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellCheckBoxRenderer"/> class.
        /// </summary>
        public GridFilterRowCheckBoxRenderer()
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
        public override void OnInitializeEditElement(DataColumnBase dataColumn, CheckBox uiElement, object dataContext)
        {
            RowColumnIndex rowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            GridColumn column = dataColumn.GridColumn;
#if WPF
            uiElement.FocusVisualStyle = null;
#else
            uiElement.UseSystemFocusVisuals = false;
#endif
            var binding = new Binding { Path = new PropertyPath("FilterRowText"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(CheckBox.IsCheckedProperty, binding);
            uiElement.IsThreeState = true;
            var paddingBind = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, paddingBind);
            var hAlignBind = new Binding { Path = new PropertyPath("HorizontalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(FrameworkElement.HorizontalAlignmentProperty, hAlignBind);
            var vAlignBind = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(FrameworkElement.VerticalAlignmentProperty, vAlignBind);
#if UWP
            uiElement.MinWidth = 0;
            uiElement.MinHeight = 0;
#endif
            uiElement.Tag = rowColumnIndex;
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
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    return true;
                case Key.Space:
                    {
                        if (!CurrentCellUIElement.IsEnabled) return true;
#if !WinRT
                        CurrentCellUIElement.Focus();
#else
                        CurrentCellUIElement.Focus(FocusState.Programmatic);
#endif
                        DataGrid.RaiseCurrentCellValueChangedEvent(new CurrentCellValueChangedEventArgs(DataGrid) { RowColumnIndex = CurrentCellIndex });
                        return true;
                    }
            }

            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion


        #region Wire/UnWire UIElements Overrides

        /// <summary>
        /// Called when [edit element loaded].
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
        /// UnWire the wired events.
        /// </summary>
        /// <param name="uiElement"></param>
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
            if (e.ChangedButton == MouseButton.Left)
            {
                if (HasCurrentCellState && !((CheckBox)sender).IsPressed && IsFocused)
                {
                    ((CheckBox)sender).IsChecked = !((CheckBox)sender).IsChecked;
                }
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

#if !WPF
            GridCell gridCell = null;
            if (checkBox.Parent is GridCell)
                gridCell = checkBox.Parent as GridCell;
            RowColumnIndex rowcolumnIndex = new RowColumnIndex();
            rowcolumnIndex.RowIndex = gridCell != null ? gridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
            rowcolumnIndex.ColumnIndex = gridCell != null ? gridCell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;

            if (!rowcolumnIndex.IsEmpty)
            {
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowcolumnIndex);
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowcolumnIndex);
            }
#endif
            if (HasCurrentCellState)
            {
                this.IsValueChanged = true;
                this.ProcessSingleFilter(GetControlValue());
#if !WPF
                checkBox.Focus(FocusState.Programmatic);
#else
                checkBox.Focus();
#endif
            }
        }

#endregion
    }
}
