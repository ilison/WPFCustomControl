#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Windows.Shared;
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Input;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
using Syncfusion.UI.Xaml.TreeGrid;
using Syncfusion.UI.Xaml.Grid;


namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
    /// <summary>
    ///  Renderer for <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPercentColumn"/> which is used to display the percent value.
    /// </summary>
    public class TreeGridCellPercentageRenderer : TreeGridVirtualizingCellRenderer<TextBlock, PercentTextBox>
    {
        #region Override Methods

        #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            //Note: Padding cannot be binded for Display UI Element, since the padding will be irregular to match Edit/Display UI Elements
            //The Column will be refreshed via Dependency CallBack.
            uiElement.Padding = dataColumn.TreeGridColumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, PercentTextBox uiElement, object dataContext)
        {
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            InitializeEditUIElement(uiElement, dataColumn.TreeGridColumn);
            BindingExpression = uiElement.GetBindingExpression(PercentTextBox.PercentValueProperty);
        }
        #endregion

        #region

        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState)
                return base.GetControlValue();
            return CurrentCellRendererElement.GetValue(IsInEditing ? PercentTextBox.PercentValueProperty : TextBlock.TextProperty);
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState)
                return;

            if (IsInEditing)
                ((PercentTextBox)CurrentCellRendererElement).PercentValue = (double?)value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
        #endregion

        #region Wire/UnWire UIElements Overrides
        /// <summary>
        /// Invoked when the edit element(PercentTextBox) is loaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var uiElement = ((PercentTextBox)sender);
            uiElement.PercentValueChanged += OnValueChanged;
            uiElement.Focus();
            if (this.TreeGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll && PreviewInputText == null)
            {
                uiElement.SelectAll();
            }
            else
            {
                if (PreviewInputText == null || char.IsLetter(PreviewInputText.ToString(), 0))
                {
                    var index = uiElement.Text.Length;
                    uiElement.Select(index + 1, 0);
                    return;
                }
                double value;
                double.TryParse(PreviewInputText.ToString(), out value);
                uiElement.PercentValue = value;
                var caretIndex = (uiElement.Text).IndexOf(PreviewInputText.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal);
                uiElement.Select(caretIndex + 1, 0);
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// Invoked when the edit element(PercentTextBox) is unloaded on the cell in column.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        /// <summary>
        protected override void OnEditElementUnloaded(object sender, RoutedEventArgs e)
        {
            ((PercentTextBox)sender).PercentValueChanged -= OnValueChanged;           
        }

        /// Called when [unwire edit UI element].
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        protected override void OnUnwireEditUIElement(PercentTextBox uiElement)
        {
            uiElement.PercentValueChanged -= OnValueChanged;
        }
        #endregion

        #region ShouldGridTryToHandleKeyDown
        /// <summary>
        /// Shoulds the grid try automatic handle key down.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
        /// <returns></returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState || !IsInEditing)
                return true;

            var CurrentCellUIElement = (PercentTextBox)CurrentCellRendererElement;
            switch (e.Key)
            {
                case Key.Escape:
                    {
                        if (CurrentCellUIElement != null)
                            CurrentCellUIElement.ClearValue(PercentTextBox.PercentValueProperty);
                        return true;
                    }
                case Key.Left:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Right:
                    return (CurrentCellUIElement.SelectionStart >= CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.SelectionStart == CurrentCellUIElement.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed());
                //WPF-18339- When navigating by Up or Down key, should return here if IsScrollingOnCircle is true for that UIElement
                case Key.Up:
                case Key.Down:
                    return !CurrentCellUIElement.IsScrollingOnCircle;
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
        #endregion

        #region EndEdit

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (PercentTextBox)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(PercentTextBox.PercentValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }
        #endregion

        #endregion

        #region Event Handlers

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            base.CurrentRendererValueChanged();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The unique identifier element.</param>
        /// <param name="column">Grid Column for the Editor</param>
        private void InitializeEditUIElement(PercentTextBox uiElement, TreeGridColumn column)
        {
            uiElement.TextSelectionOnFocus = false;
            var percentColumn = (TreeGridPercentColumn)column;
            var bind = percentColumn.ValueBinding.CreateEditBinding(percentColumn.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(PercentTextBox.PercentValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowScrollingOnCircle"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(EditorBase.IsScrollingOnCircleProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValue"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.MinValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValue"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.MaxValueProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentEditMode"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentEditModeProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentDecimalDigits"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentDecimalDigitsProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentDecimalSeparator"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentDecimalSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentGroupSeparator"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentGroupSeparatorProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentGroupSizes"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentGroupSizesProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentNegativePattern"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentNegativePatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentPositivePattern"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentPositivePatternProperty, bind);
            bind = new Binding { Path = new PropertyPath("PercentSymbol"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.PercentageSymbolProperty, bind);
            bind = new Binding { Path = new PropertyPath("AllowNullValue"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(EditorBase.UseNullOptionProperty, bind);
            bind = new Binding { Path = new PropertyPath("MaxValidation"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(EditorBase.MaxValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("MinValidation"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(EditorBase.MinValidationProperty, bind);
            bind = new Binding { Path = new PropertyPath("NullValue"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.NullValueProperty, bind);
            uiElement.WatermarkTextIsVisible = true;
            bind = new Binding { Path = new PropertyPath("NullText"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.WatermarkTextProperty, bind);
#if WPF
            bind = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = percentColumn };
            uiElement.SetBinding(PercentTextBox.TextDecorationsProperty, bind);
#endif
        }

        #endregion
    }
}
