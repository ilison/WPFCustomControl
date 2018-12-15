#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.Grid.Helpers;
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
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls.Primitives;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    public class TreeGridCellComboBoxRenderer : TreeGridVirtualizingCellRenderer<ContentControl, ComboBox>
    {
        public Key? lastKeyPressed;
        public TreeGridCellComboBoxRenderer()
        {
            this.IsDropDownable = true;
        }

        #region Override Methods

        #region Display/Edit Binding Overrides

        protected override ContentControl OnCreateDisplayUIElement()
        {
            var contentControl = base.OnCreateDisplayUIElement();
#if WPF
            contentControl.FocusVisualStyle = null;
#endif
            return contentControl;
        }
        /// <summary>
        /// Called when [create edit unique identifier element].
        /// </summary>
        /// <returns></returns>
        protected override ComboBox OnCreateEditUIElement()
        {
            var comboBox = base.OnCreateEditUIElement();
#if WPF
            VisualContainer.SetWantsMouseInput(comboBox, true);
#endif
            return comboBox;
        }

        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">TreeDataColumn Which holds TreeGridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(TreeDataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            var column = dataColumn.TreeGridColumn;
            var gridcolumn = column as TreeGridComboBoxColumn;

            uiElement.SetBinding(ContentControl.ContentProperty, column.DisplayBinding);
            uiElement.SetValue(Control.HorizontalAlignmentProperty, TextAlignmentToHorizontalAlignment(gridcolumn.TextAlignment));
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.VerticalAlignment);
            if (gridcolumn.ItemTemplate != null)
                uiElement.ContentTemplate = gridcolumn.ItemTemplate;
            uiElement.Margin = gridcolumn.padding;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds TreeGridColumn, RowColumnIndex and TreeGridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// TreeGridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(TreeDataColumnBase dataColumn, ComboBox uiElement, object dataContext)
        {
            var column = dataColumn.TreeGridColumn;
            InitializeEditBinding(uiElement, column);
            var textAlignment = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.OneWay, Source = column, Converter = new TextAlignmentToHorizontalAlignmentConverter() };
            uiElement.SetBinding(Control.HorizontalContentAlignmentProperty, textAlignment);
            var verticalAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalAlignment);
#if UWP
            uiElement.HorizontalAlignment = HorizontalAlignment.Stretch;
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
#endif
        }
        #endregion

        #region Display/Edit Value
        /// <summary>
        /// Gets the control value.
        /// </summary>
        /// <returns></returns>
        public override object GetControlValue()
        {
            if (HasCurrentCellState)
                return CurrentCellRendererElement.GetValue(IsInEditing ? Selector.SelectedValueProperty : ContentControl.ContentProperty);
            return base.GetControlValue();
        }

        /// <summary>
        /// Sets the control value.
        /// </summary>
        /// <param name="value">The value.</param>
        public override void SetControlValue(object value)
        {
            if (!HasCurrentCellState) return;
            if (IsInEditing)
                ((ComboBox)CurrentCellRendererElement).SelectedValue = value;
            else
                throw new Exception("Value cannot be Set for Unloaded Editor");
        }

        #endregion

        #region Wire/UnWire UIElements Overrides

        protected override void OnEditElementLoaded(object sender, RoutedEventArgs e)
        {
            var combobox = ((ComboBox)sender);
#if WPF

            combobox.Focus();

            TextBox comboTextBox = (TextBox)GridUtil.FindDescendantChildByType(combobox, typeof(TextBox));

            if (combobox.IsEditable)
            {
                if (this.TreeGrid.EditorSelectionBehavior == EditorSelectionBehavior.SelectAll)
                {
                    comboTextBox.SelectAll();
                }
                else
                {
                    comboTextBox.Select(comboTextBox.SelectedText.Length, 0);
                }
            }
#endif
#if UWP
            combobox.Focus(FocusState.Programmatic);
#endif

            combobox.SelectionChanged += SelectionChangedinComboBox;
            //#if WPF
            //            combobox.PreviewMouseDown += PreviewMouseDown;
            //#endif

        }

        protected override void OnUnwireEditUIElement(ComboBox uiElement)
        {
            uiElement.SelectionChanged -= SelectionChangedinComboBox;
            //#if WPF
            //            uiElement.PreviewMouseDown -= PreviewMouseDown;
            //#endif
        }


        #endregion

        #region ShouldGridHandleKeyDown

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
            if (!HasCurrentCellState || !IsInEditing)
                return true;
#if WPF
            var combobox = (CurrentCellRendererElement as ComboBox);
            TextBox comboTextBox = (TextBox)GridUtil.FindDescendantChildByType(combobox, typeof(TextBox));

#endif
#if WPF
            if ((SelectionHelper.CheckAltKeyPressed() && e.SystemKey == Key.Down) || ((SelectionHelper.CheckAltKeyPressed() && e.SystemKey == Key.Up) || e.Key == Key.F4))
#else
            if ((SelectionHelper.CheckAltKeyPressed() && e.Key == Key.Down) || ((SelectionHelper.CheckAltKeyPressed() && e.Key == Key.Up) || e.Key == Key.F4))
#endif
            {
                var comboBox = ((ComboBox)CurrentCellRendererElement);
                comboBox.IsDropDownOpen = !comboBox.IsDropDownOpen;
#if UWP
                comboBox.Focus(FocusState.Programmatic);
#endif
                return false;
            }

            switch (e.Key)
            {
                case Key.End:
                case Key.Home:
                case Key.Enter:
                case Key.Escape:
                    return !((ComboBox)CurrentCellRendererElement).IsDropDownOpen;
                case Key.Down:
                case Key.Up:
#if UWP
                    return !((ComboBox)CurrentCellRendererElement).IsDropDownOpen;
#endif
#if WPF
                    // WPF-25803 - Up/Down Needs to be handle by UiElement itself to change the selection whether drop down open or not/ IsEditable set or not set.
                    return false;                                        
                case Key.Right:
                    return ((ComboBox)CurrentCellRendererElement).IsEditable ? (comboTextBox.SelectionStart >= comboTextBox.Text.Length && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed()) : true;
                case Key.Left:
                    return ((ComboBox)CurrentCellRendererElement).IsEditable ? (comboTextBox.SelectionStart == comboTextBox.SelectionLength && !SelectionHelper.CheckControlKeyPressed() && !SelectionHelper.CheckShiftKeyPressed()) : true;                    
#endif
            }
            return base.ShouldGridTryToHandleKeyDown(e);
        }

        #endregion

        #region EndEdit

        public override bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            if (canResetBinding)
            {
                var CurrentCellUIElement = (Selector)CurrentCellRendererElement;
                CurrentCellUIElement.ClearValue(Selector.SelectedValueProperty);
            }
            return base.EndEdit(dc, record, canResetBinding);
        }

        #endregion

        #endregion

        #region Private Methods

        private void InitializeEditBinding(ComboBox uiElement, TreeGridColumn column)
        {
            var comboBoxColumn = (TreeGridComboBoxColumn)column;
            var bind = column.ValueBinding.CreateEditBinding(comboBoxColumn.GridValidationMode != GridValidationMode.None, column);
            uiElement.SetBinding(ComboBox.SelectedValueProperty, bind);
            var itemsSourceBinding = new Binding { Path = new PropertyPath("ItemsSource"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.ItemsSourceProperty, itemsSourceBinding);
            var displayMemberBinding = new Binding { Path = new PropertyPath("DisplayMemberPath"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.DisplayMemberPathProperty, displayMemberBinding);
            var selectedValuePathBinding = new Binding { Path = new PropertyPath("SelectedValuePath"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.SelectedValuePathProperty, selectedValuePathBinding);
#if WPF
            var staysOpenOnEditBinding = new Binding { Path = new PropertyPath("StaysOpenOnEdit"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.StaysOpenOnEditProperty, staysOpenOnEditBinding);
            var isEditableBinding = new Binding { Path = new PropertyPath("IsEditable"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.IsEditableProperty, isEditableBinding);
#endif
            var itemTemplateBinding = new Binding { Path = new PropertyPath("ItemTemplate"), Mode = BindingMode.TwoWay, Source = comboBoxColumn };
            uiElement.SetBinding(ComboBox.ItemTemplateProperty, itemTemplateBinding);
        }
   

        protected override void OnEnteredEditMode(TreeDataColumnBase dataColumn, FrameworkElement currentRendererElement)
        {
#if WPF
            if ((currentRendererElement as ComboBox).StaysOpenOnEdit && (currentRendererElement as ComboBox).IsEditable)
#else
            if ((currentRendererElement as ComboBox).IsEditable)
#endif
                (currentRendererElement as ComboBox).IsDropDownOpen = true;

            base.OnEnteredEditMode(dataColumn, currentRendererElement);
        }

#if UWP
        private void SelectionChangedinComboBox(object sender, Windows.UI.Xaml.Controls.SelectionChangedEventArgs e)
#else
        private void SelectionChangedinComboBox(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
#endif
        {
            var comboBox = (ComboBox)sender;
            TreeGrid.RaiseCurrentCellDropDownSelectionChangedEvent(new CurrentCellDropDownSelectionChangedEventArgs(TreeGrid)
            {
                SelectedIndex = comboBox.SelectedIndex,
                SelectedItem = comboBox.SelectedItem,
                RowColumnIndex = CurrentCellIndex
            });
        }
        #endregion
    }
}