#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
#endif


namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    [ClassReference(IsReviewed = false)]
    public class GridCellCheckBoxRenderer : GridVirtualizingCellRenderer<CheckBox, CheckBox>
    {
        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="GridCellCheckBoxRenderer"/> class.
        /// </summary>
        public GridCellCheckBoxRenderer()
        {
            SupportsRenderOptimization = false;
            IsEditable = false;
        }
        #endregion

        #region Override Methods

        #region Render
#if WPF
        protected override void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell,object dataContext)
        {
            // Overridden to avoid the content to be drawn. Here, its loads  CheckBox as usual in UseLightweightTemplate true case also.
        }
#endif

        #endregion

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
            var column = dataColumn.GridColumn;
#if WPF
            uiElement.FocusVisualStyle = null;
            if (column.isValueMultiBinding)
            {
                var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
                if (!column.AllowEditing && !DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                    (binding as MultiBinding).Mode = BindingMode.OneWay;
                if (DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                    (binding as MultiBinding).Mode = BindingMode.TwoWay;
                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }
            else
            {
                var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
                if (!column.AllowEditing && !DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                    (binding as Binding).Mode = BindingMode.OneWay;
                if (DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                    (binding as Binding).Mode = BindingMode.TwoWay;
                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);
            }
#else
            var binding = column.ValueBinding.CreateEditBinding(column.GridValidationMode != GridValidationMode.None, column);
            if (!column.AllowEditing && !DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                (binding as Binding).Mode = BindingMode.OneWay;

            if (DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex))
                (binding as Binding).Mode = BindingMode.TwoWay;
            //UWP -718 Null value is not loaded in GridCheckBoxColumn
            if ((column as GridCheckBoxColumn).IsThreeState)
            {
                uiElement.SetBinding(GridCellCheckBoxRenderer.IsCheckedProperty, binding);
                SetIsChecked(uiElement, uiElement.IsChecked);
            }
            else
                uiElement.SetBinding(ToggleButton.IsCheckedProperty, binding);           
#endif
            BindingExpression = uiElement.GetBindingExpression(ToggleButton.IsCheckedProperty);
            uiElement.SetValue(Control.PaddingProperty, column.Padding);
            uiElement.SetValue(FrameworkElement.VerticalAlignmentProperty, column.VerticalAlignment);
#if UWP
            uiElement.MinWidth = 0;
            uiElement.MinHeight = 0;
#endif
            uiElement.IsEnabled = (column.AllowEditing || DataGrid.IsAddNewIndex(rowColumnIndex.RowIndex)) && DataGrid.SelectionMode != GridSelectionMode.None;
            var checkBoxColumn = column as GridCheckBoxColumn;
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
            DependencyProperty.RegisterAttached("IsChecked", typeof(object), typeof(GridCellCheckBoxRenderer), new PropertyMetadata(null,OnIsCheckedChanged));

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var checkBox = d as CheckBox;
            checkBox.IsChecked = (bool?)e.NewValue;          
        }

        /// <summary>
        /// Method which is used to update the Renderer element Bindings with corresponding column values.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="element">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for update the Binding
        /// <param name="dataContext"> dataContext of the row</param>
        /// <remarks></remarks>
        public override void OnUpdateEditBinding(DataColumnBase dataColumn, CheckBox element, object dataContext)
        {
            OnInitializeEditElement(dataColumn, element, dataContext);
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
        public override bool CanUpdateBinding(GridColumn column)
        {
            return column.GridValidationMode != GridValidationMode.None;
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
            GridCell gridCell = null;
            if (CurrentCellUIElement.Parent is GridCell)
                gridCell = CurrentCellUIElement.Parent as GridCell;

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

        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
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
        /// Invoked when the edit element(CheckBox) is loaded on the cell in column
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

                GridCell gridCell = null;

                var checkBox = sender as CheckBox;

                if (checkBox.Parent is GridCell)
                    gridCell = checkBox.Parent as GridCell;

                var rowColumnIndex = new RowColumnIndex();
                rowColumnIndex.RowIndex = gridCell != null ? gridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
                rowColumnIndex.ColumnIndex = gridCell != null ? gridCell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;

                if (!rowColumnIndex.IsEmpty)
                    DataGrid.RaiseCurrentCellValueChangedEvent(new CurrentCellValueChangedEventArgs(DataGrid) { RowColumnIndex = rowColumnIndex, Record = checkBox.DataContext, Column = gridCell.ColumnBase.GridColumn });
            }
        }
#endif
        
        // WPF-13902 -In AddNewRow - the Isclosed column checkbox not got checked.
        /// <summary>
        /// Method that used to decide whether AddNewRow check box column can get check at first click or not.
        /// </summary>
        /// <returns></returns>
        protected virtual bool CanCheckAddNewRowAtFirstClick()
        {
            return false;
        }

        /// <summary>
        /// Called when [CheckBox click].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        void OnCheckBoxClick(object sender, RoutedEventArgs e)
        {
            var checkBox = sender as CheckBox;

            GridCell gridCell = null;
            if (checkBox.Parent is GridCell)
                gridCell = checkBox.Parent as GridCell;            
           
            if (DataGrid.IsAddNewIndex(gridCell != null ? gridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex) && (DataGrid.HasView && !DataGrid.View.IsAddingNew))
            {
                DataGrid.GridModel.AddNewRowController.AddNew();
                checkBox.IsChecked = CanCheckAddNewRowAtFirstClick();
            }

            var rowcolumnIndex = new RowColumnIndex();
            rowcolumnIndex.RowIndex = gridCell != null ? gridCell.ColumnBase.RowIndex : new RowColumnIndex().RowIndex;
            rowcolumnIndex.ColumnIndex = gridCell != null ? gridCell.ColumnBase.ColumnIndex : new RowColumnIndex().ColumnIndex;
#if !WPF
            if(checkBox.IsThreeState)
                 SetIsChecked(checkBox, checkBox.IsChecked);

            if (!rowcolumnIndex.IsEmpty)
            {
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowcolumnIndex);
                this.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowcolumnIndex);
            }
#endif

            DataGrid.RaiseCurrentCellValueChangedEvent(new CurrentCellValueChangedEventArgs(DataGrid) { RowColumnIndex = rowcolumnIndex, Record = checkBox.DataContext, Column = gridCell.ColumnBase.GridColumn });

            if (HasCurrentCellState)
            {
                BindingExpression = this.CurrentCellRendererElement.GetBindingExpression(ToggleButton.IsCheckedProperty);
            }

            //when check or uncheck the checkbox, setter property of checkbox column fires two times in the Model class.
            //while loading, we set the updatesourcetrigger as explicit for checkboxcolumn if we set updatetrigger as PropertyChanged in sample for checkboxcolumn.
            //so skips the below call by checking the updatesourcetrigger.
#if WPF
            if (gridCell.ColumnBase.GridColumn.isValueMultiBinding)
            {
                if (BindingExpression != null && (gridCell != null && (gridCell.ColumnBase.GridColumn.ValueBinding as MultiBinding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                    BindingExpression.UpdateSource();
            }
            else if (BindingExpression != null && (gridCell != null && (gridCell.ColumnBase.GridColumn.ValueBinding as Binding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                BindingExpression.UpdateSource();
#else
            if (BindingExpression != null && (gridCell != null && (gridCell.ColumnBase.GridColumn.ValueBinding as Binding).UpdateSourceTrigger == UpdateSourceTrigger.Explicit))
                BindingExpression.UpdateSource();
#endif

            }
#endregion
        }
}
