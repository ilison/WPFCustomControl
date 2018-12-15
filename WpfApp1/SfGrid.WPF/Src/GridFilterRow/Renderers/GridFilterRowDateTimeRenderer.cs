#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid.Helpers;
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
using Syncfusion.Windows.Shared;
#endif
namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using EventArgs = PointerRoutedEventArgs;
    using DateTimeEdit = SfDatePicker;
#endif
    /// <summary>
    /// Represents a class which handles the filter operation that loads the DateTimeEdit in a FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridFilterRowDateTimeRenderer : GridFilterRowCellRenderer<TextBlock, DateTimeEdit>
    {
    #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellDateTimeRenderer"/> class.
        /// </summary>
        public GridFilterRowDateTimeRenderer()
        {
#if WinRT || UNIVERSAL
            IsFocusible = false;
#endif
        }
    #endregion

    #region Override Methods

    #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, TextBlock uiElement, object dataContext)
        {
            base.OnInitializeDisplayElement(dataColumn, uiElement, dataContext);
            //Note: Padding cannot be binded for Display UI Element, since the padding will be irregular to match Edit/Display UI Elements
            //The Column will be refreshed via Dependency CallBack.
            uiElement.Padding = ProcessUIElementPadding(dataColumn.GridColumn);
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, DateTimeEdit uiElement, object filterValue)
        {
            GridColumn column = dataColumn.GridColumn;
            InitializeEditUIElement(uiElement, column, filterValue);
#if !WinRT && !UNIVERSAL
            base.OnInitializeEditElement(dataColumn, uiElement, filterValue);
#else
            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
            uiElement.HorizontalAlignment = HorizontalAlignment.Stretch;
#endif
            uiElement.HorizontalContentAlignment = TextAlignmentToHorizontalAlignment(column.TextAlignment);
        }

        /// <summary>
        /// When complete edit, we need to raise query again to provide entered value to customer.
        /// </summary>
        /// <param name="dataColumn">DataColumn which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="currentRendererElement">The UIElement that was loaded in edit mdoe</param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// GridColumn - Column which is providing the information for Binding
        protected override void OnEditingComplete(DataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
            base.OnEditingComplete(dataColumn, currentRendererElement);
            if (HasCurrentCellState && this.IsValueChanged && !this.FilterRowCell.DataColumn.GridColumn.ImmediateUpdateColumnFilter)
                    this.ProcessSingleFilter(GetControlValue());
        }

    #endregion

    #region Display/Edit Value Overrides
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (!HasCurrentCellState)
                return base.GetControlValue();
#if WinRT || UNIVERSAL
            return this.CurrentCellRendererElement.GetValue(IsInEditing ? SfDatePicker.ValueProperty : TextBlock.TextProperty);
#else
            if (IsInEditing)
            {
                var dateEdit = this.CurrentCellRendererElement as DateTimeEdit;
                DateTime dateValue;
                if(dateEdit.DateTime != System.DateTime.MaxValue && dateEdit.Text != string.Empty)
                {
                    DateTime.TryParse(dateEdit.Text, out dateValue);
                    return dateValue;
                }
                else
                    return null;
            }
            return CurrentCellRendererElement.GetValue(TextBlock.TextProperty);
#endif
        }

#if !WinRT && !UNIVERSAL
        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
            {
                DateTime? filterValue;
                if (value == null)
                    filterValue = null;
                else
                    filterValue = (DateTime?)value;
                ((DateTimeEdit)CurrentCellRendererElement).DateTime = filterValue;
            }
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }
#endif
    #endregion

    #region Wire/UnWire UIElement

#if WinRT || UNIVERSAL
        /// <summary>
        /// Called when [unwire edit unique identifier element].
        /// </summary>
        /// <param name="sender"></param>

        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            ((SfDatePicker)sender).ValueChanged += OnValueChanged;
            ((SfDatePicker)sender).KeyDown += OnKeyDown;
        }

        protected override void OnUnwireEditUIElement(SfDatePicker uiElement)
        {
            uiElement.ValueChanged -= OnValueChanged;
            uiElement.KeyDown -= OnKeyDown;
        }
#else
        /// <summary>
        /// Process the filtering when the FilterRowCondition is changed in corresponding column.
        /// </summary>
        /// <param name="filterRowCondition">The new FilterRowCondition that have been changed.</param>
        public override void OnFilterRowConditionChanged(string filterRowCondition)
        {
            base.OnFilterRowConditionChanged(filterRowCondition);
            if(this.HasCurrentCellState && this.IsInEditing)
            {
                var dateTimeEdit = (DateTimeEdit)this.CurrentCellRendererElement;
                dateTimeEdit.WatermarkVisibility = Visibility.Collapsed;
                dateTimeEdit.Focus();
            }
        }

        /// <summary>
        /// Called when [edit element loaded].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var dateTimeEdit = ((DateTimeEdit)sender);
            dateTimeEdit.TextChanged += OnTextChanged;
            dateTimeEdit.WatermarkVisibility = Visibility.Collapsed;
            dateTimeEdit.Focus();
            if ((this.DataGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll || this.DataGrid.IsAddNewIndex(this.CurrentCellIndex.RowIndex)) && PreviewInputText == null)
            {
                dateTimeEdit.SelectAll();
                return;
            }
            else
            {
                if (PreviewInputText == null)
                {
                    var index = dateTimeEdit.Text.Length;
                    dateTimeEdit.Select(index + 1, 0);
                    return;
                }
                if (dateTimeEdit.CanEdit)
                    ((DateTimeEdit)CurrentCellRendererElement).SelectedText = PreviewInputText.ToString();
                TextCompositionManager.StartComposition(new TextComposition(InputManager.Current, (DateTimeEdit)CurrentCellRendererElement, PreviewInputText.ToString()));
            }
            PreviewInputText = null;
        }

        /// <summary>
        /// UnWire the wired events.
        /// </summary>
        /// <param name="uiElement"></param>
        protected override void OnUnwireEditUIElement(DateTimeEdit uiElement)
        {
            uiElement.TextChanged -= OnTextChanged;
        }
#endif

    #endregion

    #region ShouldHandleKeyDown
        /// <summary>
        /// Let Renderer decide whether the parent grid should be allowed to handle keys and prevent
        /// the key event from being handled by the visual UIElement for this renderer. If this method
        /// returns true the parent grid will handle arrow keys and set the Handled flag in the event
        /// data. Keys that the grid does not handle will be ignored and be routed to the UIElement
        /// for this renderer.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs" /> object.</param>
        /// <returns>
        /// True if the parent grid should be allowed to handle keys; false otherwise.
        /// </returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
#if WinRT || UNIVERSAL
            if (!HasCurrentCellState)
                return true;
            if (!IsInEditing)
            {
                ProcessPreviewTextInput(e);
                if (!(CurrentCellRendererElement is SfDatePicker))
                    return true;
            }
#endif
#if WPF
            if ((CheckAltKeyPressed() && e.SystemKey == Key.Down) || (CheckAltKeyPressed() && e.SystemKey == Key.Up))
#else
            if ((CheckAltKeyPressed() && e.Key == Key.Down) || (CheckAltKeyPressed() && e.Key == Key.Up))
#endif
            {
                this.FilterRowCell.OpenFilterOptionPopup();
                return false;
            }
            else if (e.Key == Key.Down || e.Key == Key.Up || e.Key == Key.PageUp || e.Key == Key.PageDown)
                return !this.FilterRowCell.IsDropDownOpen;
            else if (e.Key == Key.Enter && this.FilterRowCell.IsDropDownOpen)
                return false;

            bool isDropDownOpen = this.FilterRowCell.IsDropDownOpen;
            if (isDropDownOpen)
                this.FilterRowCell.CloseFilterOptionPopup();

            if (!IsInEditing)
                return true;

            var CurrentCellUIElement = (DateTimeEdit)CurrentCellRendererElement;
            switch (e.Key)
            {
#if !WinRT && !UNIVERSAL
                case Key.F4:
                    {
                        if (CurrentCellUIElement.IsDropDownOpen)
                            CurrentCellUIElement.ClosePopup();
                        else
                            CurrentCellUIElement.OpenPopup();
                        return true;
                    }
#endif
                case Key.Escape:
                    {
                        this.IsValueChanged = false;
                        if (isDropDownOpen)
                        {
                            CurrentCellUIElement.WatermarkVisibility = Visibility.Collapsed;
                            CurrentCellUIElement.Focus();
                            e.Handled = true;
                        }
                        return !isDropDownOpen && !CurrentCellUIElement.IsDropDownOpen;
                    }
                case Key.Enter:
                    if (!CheckControlKeyPressed() && IsInEditing)
                    {
                        this.ProcessSingleFilter(this.GetControlValue());
                        this.SetFocus(true);
                        e.Handled = true;
                        return false;
                    }
                    return true;
#if WPF
                case Key.Left:
                    return (CurrentCellUIElement.CaretIndex <= 0 && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Right:
                    return ((CurrentCellUIElement.SelectionLength + CurrentCellUIElement.SelectionStart) >= CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.Home:
                    return (CurrentCellUIElement.SelectionStart == 0 && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
                case Key.End:
                    return (CurrentCellUIElement.CaretIndex == CurrentCellUIElement.Text.Length && !CheckControlKeyPressed() && !CheckShiftKeyPressed());
#else
                case Key.Left:
                case Key.Right:
                    return true;
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }
    #endregion

    #endregion

    #region Event Handlers

        /// <summary>
        /// Called when [text changed].
        /// </summary>
        /// <param name="d">sIts the corresponding Editor here </param>
        /// <param name="e">The <see cref="TextChangedEventArgs"/> instance containing the event data.</param>
#if WinRT || UNIVERSAL
        private void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
#else
        private void OnTextChanged(object sender, TextChangedEventArgs e)
#endif
        {
            base.CurrentRendererValueChanged();
            if (!HasCurrentCellState)
                return;

            this.IsValueChanged = true;
            if (this.FilterRowCell.DataColumn.GridColumn.ImmediateUpdateColumnFilter)
            {
                this.ProcessSingleFilter(GetControlValue());
                this.SetFocus(true);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Computes the Padding for the Display UIElement.
        /// </summary>
        /// <param name="column">The column.</param>
        /// <remarks>Padding Cannot be applied at RunTime, as Currently We dont haven't constructed Padding via Binding Expression</remarks>
        /// <returns></returns>
        private static Thickness ProcessUIElementPadding(GridColumn column)
        {
            var padLeft = column.Padding.Left;
            var padRight = column.Padding.Right;
            var padTop = column.Padding.Top;
            var padBotton = column.Padding.Bottom;
            var padding = column.ReadLocalValue(GridColumn.PaddingProperty);
#if WPF
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft, 6 + padTop, 3 + padRight, 5 + padBotton)
                           : new Thickness(3, 1, 3, 1);
#else
            return padding != DependencyProperty.UnsetValue
                           ? new Thickness(3 + padLeft,  padTop, 2 + padRight, padBotton)
                           : new Thickness(3, 1, 2, 0);
#endif
        }

        /// <summary>
        /// Processes the edit binding.
        /// </summary>
        /// <param name="uiElement">The UI element.</param>
        /// <param name="column"></param>
        private void InitializeEditUIElement(DateTimeEdit uiElement, GridColumn column, object dataContext)
        {
            var filterValue = column.FilteredFrom == FilteredFrom.FilterRow &&
              column.FilterPredicates.Count > 0 ? column.FilterPredicates.FirstOrDefault().FilterValue : null;
            if (column is GridDateTimeColumn)
            {
                var dateTimeColumn = (GridDateTimeColumn)column;
#if WinRT || UNIVERSAL

                //WRT-4266 Need to create the binding by using CreateEditBinding method available in BindingUtility helper
                //for Setting the Mode as Two Way for AddNewRowColumn if AllowEditing is False
                var bind = new Binding { Path = new PropertyPath("FormatString"), Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.FormatStringProperty, bind);
                bind = new Binding { Path = new PropertyPath("ShowDropDownButton"), Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.ShowDropDownButtonProperty, bind);
                bind = new Binding { Path = new PropertyPath("AccentBrush"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.AccentBrushProperty, bind);
                bind = new Binding { Path = new PropertyPath("DropDownHeight"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.DropDownHeightProperty, bind);
                bind = new Binding { Path = new PropertyPath("InputScope"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.InputScopeProperty, bind);
                bind = new Binding { Path = new PropertyPath("SelectorItemCount"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.SelectorItemCountProperty, bind);
                bind = new Binding { Path = new PropertyPath("SelectorItemHeight"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.SelectorItemHeightProperty, bind);
                bind = new Binding { Path = new PropertyPath("SelectorItemSpacing"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.SelectorItemSpacingProperty, bind);
                bind = new Binding { Path = new PropertyPath("SelectorItemWidth"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.SelectorItemWidthProperty, bind);
                bind = new Binding { Path = new PropertyPath("SelectorFormatString"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.SelectorFormatStringProperty, bind);
                uiElement.IsDropDownOpen = dateTimeColumn.IsDropDownOpen;
            }
            uiElement.AllowNull = true;
            uiElement.AllowInlineEditing = true;
            uiElement.Watermark = null;
            if (filterValue == null || filterValue.ToString() == string.Empty)
                uiElement.Value = null;
            else
                uiElement.Value = filterValue;
#else
                var bind = new Binding { Path = new PropertyPath("CustomPattern"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeBase.CustomPatternProperty, bind);
                bind = new Binding { Path = new PropertyPath("DateTimeFormat"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeBase.DateTimeFormatProperty, bind);
                bind = new Binding { Path = new PropertyPath("Pattern"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeBase.PatternProperty, bind);
                bind = new Binding { Path = new PropertyPath("EnableClassicStyle"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.EnableClassicStyleProperty, bind);
                bind = new Binding { Path = new PropertyPath("ShowRepeatButton"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeBase.IsVisibleRepeatButtonProperty, bind);
                bind = new Binding { Path = new PropertyPath("DisableDateSelection"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(DateTimeEdit.DisableDateSelectionProperty, bind);
                bind = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = dateTimeColumn };
                uiElement.SetBinding(TimeSpanEdit.TextDecorationsProperty, bind);
            }
            uiElement.CanEdit = true;
            uiElement.EnableBackspaceKey = true;
            uiElement.EnableDeleteKey = true;
            uiElement.IsEmptyDateEnabled = true;
            uiElement.NullValue = null;
            uiElement.NoneDateText = string.Empty;
            if (filterValue == null || filterValue.ToString() == string.Empty)
                uiElement.DateTime = null;
            else
                uiElement.DateTime = (DateTime?)filterValue;
#endif
        }

    #endregion
    }

}
