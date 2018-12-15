#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
#if WinRT || UNIVERSAL
using Syncfusion.UI.Xaml.ScrollAxis;
using Windows.UI.Xaml;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows.Data;
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using EventArgs = PointerRoutedEventArgs;
#endif

    /// <summary>
    /// GridVirtualizingCellRenderer is an abstract base class for cell renderers
    /// that need live UIElement visuals displayed in a cell. You can derive from
    /// this class and provide the type of the UIElement you want to show inside cells
    /// as type parameter. The class provides strong typed virtual methods for 
    /// initializing content of the cell and arranging the cell visuals. See 
    /// <see>
    ///     <cref>GridVirtualizingCellRendererBase{T}</cref>
    /// </see>
    ///     for more details.
    /// <para/>
    /// The idea behind this class is to provide a place where we can 
    /// add general code that should be shared for all cell renderers in the tree derived
    /// from GridVirtualizingCellRendererBase. While this class does at
    /// the moment not add meaningful functionality to GridVirtualizingCellRendererBase
    /// we created this extra layer of inheritance to make it easy to share 
    /// code for the GridVirtualizingCellRendererBase base class between grid and
    /// common assemblies and keep grid control specific code
    /// out of the base class. It is currently not possible with C# to the base class as 
    /// template type parameter.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public abstract class GridVirtualizingCellRenderer<D, E> : GridVirtualizingCellRendererBase<D,E>
        where D : FrameworkElement, new()
        where E : FrameworkElement, new()
    {
        #region Property
        public Type EditorType { get; set; }
        #endregion

        #region Ctor

        protected GridVirtualizingCellRenderer()
        {
            this.EditorType = typeof(E);
        }

        #endregion

        #region Private Methods
        internal bool CheckControlKeyPressed()
        {
#if UWP
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.Control).HasFlag(CoreVirtualKeyStates.Down));
#else
            return (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl));
#endif
        }

        internal bool CheckAltKeyPressed()
        {
#if UWP
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.Menu).HasFlag(CoreVirtualKeyStates.Down));
#else
            return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
#endif
        }

#if WinRT
        internal bool CheckCapsKeyPressed()
        {
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.CapitalLock).HasFlag(CoreVirtualKeyStates.Down));
        }
#endif
        internal bool CheckShiftKeyPressed()
        {
#if WinRT
            return (Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift).HasFlag(CoreVirtualKeyStates.Down));
#else
            return (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));
#endif
        }

#if WinRT
        internal void ProcessPreviewTextInput(KeyEventArgs e)
        {
            if ((!char.IsLetterOrDigit(e.Key.ToString(), 0) || !DataGrid.AllowEditing || DataGrid.NavigationMode != NavigationMode.Cell) || CheckControlKeyPressed() || (!(e.Key >= Key.A && e.Key <= Key.Z) && !(e.Key >= Key.Number0 && e.Key <= Key.Number9) && !(e.Key >= Key.NumberPad0 && e.Key <= Key.NumberPad9)))
                return;
            if (DataGrid.SelectionController.CurrentCellManager.BeginEdit())
                PreviewTextInput(e);
        }
#endif

        #endregion

        #region Protected Methods

        /// <summary>
        /// Texts the alignment to horizontal alignment.
        /// </summary>
        /// <param name="textAlignment">The text alignment.</param>
        /// <returns></returns>
        protected HorizontalAlignment TextAlignmentToHorizontalAlignment(TextAlignment textAlignment)
        {
            HorizontalAlignment horizontalAlignment;

            switch (textAlignment)
            {
                case TextAlignment.Right:
                    horizontalAlignment = HorizontalAlignment.Right;
                    break;

                case TextAlignment.Center:
                    horizontalAlignment = HorizontalAlignment.Center;
                    break;

                case TextAlignment.Justify:
                    horizontalAlignment = HorizontalAlignment.Stretch;
                    break;
                default:
                    horizontalAlignment = HorizontalAlignment.Left;
                    break;
            }
            return horizontalAlignment;
        }
        #endregion

        #region Virtual Methods

        protected virtual void CurrentRendererValueChanged()
        {
            var column = (DataGrid.Columns[DataGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex)]);
            DataGrid.RaiseCurrentCellValueChangedEvent(new CurrentCellValueChangedEventArgs(DataGrid) { RowColumnIndex = CurrentCellIndex, Record = CurrentCellRendererElement.DataContext, Column = column });
            if (column.UpdateTrigger != UpdateSourceTrigger.PropertyChanged || BindingExpression == null) 
                return;
            if (BindingExpression.DataItem == null)
                return;
            object oldValue = DataGrid.View.GetPropertyAccessProvider()
                             .GetValue(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path);
            
            BindingExpression.UpdateSource();
            var Text = DataGrid.View.GetPropertyAccessProvider()
                       .GetValue(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path);
            object newValue;
            string errorMessage;
            if (!DataGrid.ValidationHelper.RaiseCurrentCellValidatingEvent(oldValue, Text, column, out newValue, CurrentCellIndex, CurrentCellElement, out errorMessage, BindingExpression.DataItem)) 
                return;
            if (!ReferenceEquals(newValue, Text))
                SetControlValue(newValue);

            if (DataGrid.GridValidationMode != GridValidationMode.None)
                DataGrid.ValidationHelper.ValidateColumn(BindingExpression.DataItem, BindingExpression.ParentBinding.Path.Path, (GridCell)CurrentCellElement, CurrentCellIndex);

            DataGrid.ValidationHelper.RaiseCurrentCellValidatedEvent(oldValue, Text, column, errorMessage, BindingExpression.DataItem);
        }
        #endregion

        #region Override Methods
        /// <summary>
        /// Handles the key interaction with editor of corresponding column.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>
        ///  Returns <b>true</b> handled by SfDataGrid. Returns <b>false</b> handled by editor of column.
        /// </returns>
        protected override bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Enter:
                case Key.Up:
                case Key.Down:
                case Key.F2:
                case Key.Escape:
                case Key.PageDown:
                case Key.PageUp:
              
                case Key.Delete:
                case Key.Left:
                case Key.Right:
                case Key.Home:
                case Key.End:
                case Key.Tab:
                //While pressing the Space key the WholeRow have to be select when the CurrentCell is in the DataRow.
                //Hence the Space key operation is missing in this Class.
                case Key.Space:
                {
                    //This code is removed for selection issue, when pressing any Down arrow key in Child gird, e.Handled is set to true
                    //hence the Down operation is continued in parent grid. So that, the below code is removed.
                    //e.Handled = true;
                    return true;
                }
                case Key.C:
                case Key.V:
                case Key.X:
                case Key.A:
                    return CheckControlKeyPressed() && !IsInEditing;
            }
            return false;
        }

        /// <summary>
        /// Initialize the binding for GridCell by Columns's CellTemplate and CellTemplateSelector.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
            if (column.hasCellTemplate)            
                uiElement.ContentTemplate = column.CellTemplate;            
            else if (column.hasCellTemplateSelector)
                uiElement.ContentTemplateSelector = column.CellTemplateSelector;
            OnUpdateTemplateBinding(dataColumn, uiElement,dataContext);
        }

        /// <summary>
        ///  Updates the binding for the GridCell by Column's CellTemplate and CellTemplateSelector.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;

            if (column.SetCellBoundValue)
            {              
                var dataContextHelper = new DataContextHelper { Record = dataContext };
                dataContextHelper.SetValueBinding(column.DisplayBinding, dataContext);
                uiElement.Content = dataContextHelper;                
            }
            else
                uiElement.SetBinding(ContentControl.ContentProperty, new Binding());
        }

        /// <summary>
        /// Initialize the binding for display element of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn, D uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
#if WP
            uiElement.SetBinding(TextBlock.TextProperty, column.DisplayBinding as Binding);
#else
               uiElement.SetBinding(TextBlock.TextProperty, column.DisplayBinding);
#endif
            uiElement.SetValue(TextBlock.TextAlignmentProperty, column.textAlignment);
            uiElement.SetValue(Control.VerticalAlignmentProperty, column.verticalAlignment);

            uiElement.SetValue(TextBlock.TextTrimmingProperty, column.textTrimming);
            uiElement.SetValue(TextBlock.TextWrappingProperty, column.textWrapping);            
#if WPF
            uiElement.SetValue(TextBlock.TextDecorationsProperty, column.textDecoration);            
#endif
        }

        /// <summary>
        /// Updates the binding for display element of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateDisplayBinding(DataColumnBase dataColumn, D uiElement, object dataContext)
        {
            OnInitializeDisplayElement(dataColumn, uiElement,dataContext);
        }

        /// <summary>
        /// Initialize the binding for editor control of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, E uiElement, object dataContext)
        {
            GridColumn column = dataColumn.GridColumn;
#if WPF
            if (DataGrid.useDrawing)            
                uiElement.SetValue(Control.BorderThicknessProperty, new Thickness(0));                        
#endif
            var textPadding = new Binding { Path = new PropertyPath("Padding"), Mode = BindingMode.OneWay, Source = column };
            uiElement.SetBinding(Control.PaddingProperty, textPadding);
            var textAlignBind = new Binding { Path = new PropertyPath("TextAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextAlignmentProperty, textAlignBind);
            var textWrappingBinding = new Binding { Path = new PropertyPath("TextWrapping"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextWrappingProperty, textWrappingBinding);
            var verticalContentAlignment = new Binding { Path = new PropertyPath("VerticalAlignment"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(Control.VerticalContentAlignmentProperty, verticalContentAlignment);
#if WPF
            var textDecorations = new Binding { Path = new PropertyPath("TextDecorations"), Mode = BindingMode.TwoWay, Source = column };
            uiElement.SetBinding(TextBox.TextDecorationsProperty, textDecorations);
#endif
            uiElement.VerticalAlignment = VerticalAlignment.Stretch;
        }

        /// <summary>
        /// Updates the binding for editor control of corresponding column.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="element">Specifies the data context of the particular row</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnUpdateEditBinding(DataColumnBase dataColumn, E element, object dataContext)
        {
            OnInitializeEditElement(dataColumn, element, dataContext);
        }

        /// <summary>
        /// Updates the tool tip for GridCell.
        /// </summary>
        /// <param name="dataColumn">Which holds GridColumn, Row Column Index and GridCell</param>
        public override void UpdateToolTip(DataColumnBase dataColumn)
        {
            var uiElement = dataColumn.ColumnElement;
            var column = dataColumn.GridColumn;
            if (dataColumn.IsEditing || column == null || !column.ShowToolTip|| 
                this is GridSummaryCellRenderer || this is GridCaptionSummaryCellRenderer || 
                this is GridTableSummaryCellRenderer)
            {
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
                return;
            }

            object dataContext = dataColumn.ColumnElement.DataContext;
            var obj = ToolTipService.GetToolTip(uiElement);
            ToolTip tooltip;
            if (obj is ToolTip)
                tooltip = obj as ToolTip;
            else
                tooltip = new ToolTip();
            if (column.hasToolTipTemplate || column.hasToolTipTemplateSelector)
            {
                if (column.SetCellBoundToolTip)
                {
                    var dataContextHelper = new DataContextHelper { Record = dataContext };
                    dataContextHelper.SetValueBinding(column.DisplayBinding, dataContext);
                    tooltip.Content = dataContextHelper;
                }
                else
                    tooltip.Content = dataContext;

                if (column.hasToolTipTemplate)
                    tooltip.ContentTemplate = column.ToolTipTemplate;
                else if (column.hasToolTipTemplateSelector)
                    tooltip.ContentTemplateSelector = column.ToolTipTemplateSelector;
            }
            else
            {
                //UWP-2846 - ToolTip value shown as namespace of the boolean instead of value either True or False for the CheckBoxColumn in UWP
                //So removed the code to set binding for ToolTip and assigned the cell value directly to the Content property
                //tooltip.SetBinding(ContentControl.ContentProperty, column.DisplayBinding);
                var provider = column.DataGrid.View.GetPropertyAccessProvider();
                if (provider != null)
                {
                    var displayText = Convert.ToString(provider.GetDisplayValue(dataColumn.ColumnElement.DataContext, column.MappingName, column.UseBindingValue));
                    tooltip.Content = displayText;
                    if (string.IsNullOrEmpty(displayText))
                        tooltip.IsEnabled = false;
                }
            }
            //WPF -23277 Unbound column display and value binding has been created based on its dummy mapping name. hence this content will be null.
            // so that have set the tooltip content directly for unbound column
            if (column.IsUnbound)
            {
                var unboundCellValue = Convert.ToString(this.DataGrid.GetUnBoundCellValue(column, dataColumn.ColumnElement.DataContext));
                tooltip.Content = unboundCellValue;
                if (string.IsNullOrEmpty(unboundCellValue))
                    tooltip.IsEnabled = false;
            }
            //Specifies to raise tooltip opening event for the corresponding cell
            if (dataColumn.RaiseCellToolTipOpening(tooltip))
                ToolTipService.SetToolTip(uiElement, tooltip);
            else
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
        }

        #endregion      
    }

    /// <summary>
    /// GridVirtualizingUnBoundRowCellRenderer is an abstract base class for UnBoundRow cell renderers
    /// that need live UIElement visuals displayed in a cell. You can derive from
    /// this class and provide the type of the UIElement you want to show inside cells
    /// as type parameter. The class provides strong typed virtual methods for 
    /// initializing content of the cell and arranging the cell visuals. See 
    /// <see>
    ///     <cref>GridVirtualizingCellRendererBase{T}</cref>
    /// </see>
    ///     for more details.
    /// <para/>
    /// The idea behind this class is to provide a place where we can 
    /// add general code that should be shared for all cell renderers in the tree derived
    /// from GridVirtualizingCellRendererBase. While this class does at
    /// the moment not add meaningful functionality to GridVirtualizingCellRendererBase
    /// we created this extra layer of inheritance to make it easy to share 
    /// code for the GridVirtualizingCellRendererBase base class between grid and
    /// common assemblies and keep grid control specific code
    /// out of the base class. It is currently not possible with C# to the base class as 
    /// template type parameter.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public abstract class GridUnBoundRowCellRenderer<D, E> : GridVirtualizingCellRenderer<D, E>
        where D : FrameworkElement, new()
        where E : FrameworkElement, new()
    {

        /// <summary>
        /// Creates the GridUnBounRowCell.
        /// </summary>
        /// <returns></returns>
        protected override FrameworkElement OnPrepareUIElements()
        {
            var unboundRowCell = this.DataGrid.RowGenerator.GetGridCell<GridUnBoundRowCell>();
            return unboundRowCell;
        }

        /// <summary>
        /// Called from <see>
        ///         <cref>IGridCellRenderer.PrepareUIElments</cref>
        ///     </see>
        ///     to
        /// prepare the UnBoundRow cells UIElement children.
        /// VirtualizingCellRendererBase overrides this method and
        /// creates new UIElements and wires them with the parent cells control.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// <param name="record">record of the row </param>
        /// <param name="isInEdit"></param>
        /// <returns></returns>        
        protected internal override FrameworkElement OnPrepareUIElements(DataColumnBase dataColumn, object record, bool isInEdit)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            FrameworkElement cellContainer = dataColumn.ColumnElement;                
            FrameworkElement cellcontent = null;
            GridUnBoundRowEventsArgs gridUnBoundRowEventsArgs = dataColumn.GridUnBoundRowEventsArgs;

            // Create GridCell only for editable columns
            // UseOnlyRendererElement for Non-Editable columns
            if (!UseOnlyRendererElement && cellContainer == null)
                cellContainer = OnPrepareUIElements();

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if ((gridUnBoundRowEventsArgs != null && !gridUnBoundRowEventsArgs.hasCellTemplate))
                {
                    // Cell Content will be created for Non Template cells.
                    cellcontent = CreateOrRecycleDisplayUIElement();
                    InitializeDisplayElement(dataColumn, (D)cellcontent, record);
                    WireDisplayUIElement((D)cellcontent);
                }
                else
                {
                    // We wont create Cell Content for Templated cells. 
                    // GridCell is used as RendererElement with template case.
                    InitializeTemplateElement(dataColumn, (ContentControl)cellContainer, record);
                    WireTemplateUIElement((ContentControl)cellContainer);
                }
                if (cellcontent != null)
                    (cellContainer as GridCell).Content = cellcontent;
            }
            else
            {
                cellcontent = CreateOrEditRecycleUIElement();
                if(dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn, (E)cellcontent, record);
                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = false;

                WireEditUIElement((E)cellcontent);
                // GridImageColumn, GridHyperLinkColumn and GridCheckBoxColumn are Non-editable columns. 
                //So content created and set to GridCell.      
                if (cellcontent != null && cellContainer is GridCell)
                    (cellContainer as GridCell).Content = cellcontent;
            }
            return UseOnlyRendererElement ? cellcontent : cellContainer;   
        }

#if WPF
        protected override void OnRenderContent(System.Windows.Media.DrawingContext dc, Rect cellRect, System.Windows.Media.Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {
            // Overridden to avoid the content to be drawn when unbound row cell has templates.
            if (dataColumnBase.GridUnBoundRowEventsArgs.hasCellTemplate || dataColumnBase.GridUnBoundRowEventsArgs.hasEditTemplate)
                return;

            base.OnRenderContent(dc, cellRect,clipGeometry, dataColumnBase, gridCell, dataContext);
        }
#endif
        /// <summary>
        /// Called when the UnBoundRow cell gets start edited.
        /// </summary>
        /// <param name="cellRowColumnIndex">Current cell Row and Column index</param>
        /// <param name="cellElement">Current UnBoudnRowCell</param>
        /// <param name="column">Current column of the row</param>
        /// <param name="record">DataContext of the row</param>
        /// <returns></returns>
        public override bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, GridColumn column, object record)
        {
            if (!this.HasCurrentCellState)
                return false;

            if (this.SupportsRenderOptimization)
            {
                E cellcontent = null;

                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");

                var dataColumn = (cellElement as GridCell).ColumnBase;
                GridUnBoundRowEventsArgs gridUnBoundRowEventsArgs = (cellElement as GridCell).ColumnBase.GridUnBoundRowEventsArgs;
                OnUnloadUIElements(dataColumn);

                // Cell content will be null for templated case always.

                if (gridUnBoundRowEventsArgs != null)
                    cellcontent = gridUnBoundRowEventsArgs.hasEditTemplate ? null : CreateOrEditRecycleUIElement();                    

                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = true;
                InitializeEditElement(dataColumn, (E)(cellcontent ?? cellElement), record);
                if (dataColumn.GridColumn != null)
                    dataColumn.GridColumn.IsInSuspend = false;

                WireEditUIElement((E)(cellcontent ?? cellElement));

                if (cellcontent != null)
                    (cellElement as GridCell).Content = cellcontent;

                OnEnteredEditMode(dataColumn, cellcontent ?? cellElement);
            }
            else
                OnEnteredEditMode(null, this.CurrentCellRendererElement);

            return this.IsInEditing;
        }

        /// <summary>
        /// Called when the UnBoundRow cell gets end edited.
        /// </summary>
        /// <param name="dc">DataColumnBase Which holds GridColumn, RowColumnIndex and GridCell</param>
        /// <param name="record">DataContext of the row</param>
        /// <param name="canResetBinding">Reset Binding the CurrentCell</param>
        /// <returns></returns>
        public override bool EndEdit(DataColumnBase dc, object record, bool canResetBinding = false)
        {
            var cellRowColumnIndex = new RowColumnIndex(dc.RowIndex, dc.ColumnIndex);
            var cellElement = dc.ColumnElement;
            var column = dc.GridColumn;
            if (!this.HasCurrentCellState)
                return false;
            if (!this.IsInEditing)
                return false;
            if (this.SupportsRenderOptimization)
            {
#if WPF
                E uiElement = null;
                if (!UseOnlyRendererElement && cellElement is GridCell)
                    uiElement = (E)((cellElement as GridCell).Content is FrameworkElement ? (cellElement as GridCell).Content as FrameworkElement : cellElement);
                else
                    uiElement = cellElement as E;
                uiElement.PreviewLostKeyboardFocus -= OnLostKeyboardFocus;
#endif
                //this.IsFocused = false;
                this.SetFocus(false);

                GridUnBoundRowEventsArgs gridUnBoundRowEventsArgs = (cellElement as GridCell).ColumnBase.GridUnBoundRowEventsArgs;
                var dataColumn = (cellElement as GridCell).ColumnBase;

                OnEditingComplete(dataColumn, CurrentCellRendererElement);
                if (!UseOnlyRendererElement && cellElement == null)
                    throw new Exception("Cell Element will not be get null for any case");
                OnUnloadUIElements(dataColumn);

                OnPrepareUIElements(dataColumn, record, false);

                if (gridUnBoundRowEventsArgs != null && !gridUnBoundRowEventsArgs.hasCellTemplate)                    
                    UpdateCurrentCellState((cellElement as GridCell).Content as FrameworkElement, false);
                else
                    UpdateCurrentCellState(cellElement as FrameworkElement, false);
            }
            else
                UpdateCurrentCellState(this.CurrentCellRendererElement, false);

            return !this.IsInEditing;
        }

        /// <summary>
        /// The method which updates the binding while reuse the UnBoundRow Cell's UIElement.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell</param>
        /// <param name="record">DataContext of the Row</param>
        /// <param name="isInEdit"></param>    
        protected internal override void OnUpdateBindingInfo(DataColumnBase dataColumn, object record, bool isInEdit)
        {
            RowColumnIndex cellRowColumnIndex = new RowColumnIndex(dataColumn.RowIndex, dataColumn.ColumnIndex);
            FrameworkElement uiElement = dataColumn.ColumnElement;
            GridColumn column = dataColumn.GridColumn;
            FrameworkElement rendererElement = null;

            if (UseOnlyRendererElement)
                rendererElement = uiElement as FrameworkElement;
            else if (uiElement is GridCell)
                rendererElement = (uiElement as GridCell).Content is FrameworkElement ? (uiElement as GridCell).Content as FrameworkElement : uiElement;

            if (this.SupportsRenderOptimization && !isInEdit)
            {
                if ((dataColumn.GridUnBoundRowEventsArgs != null && !dataColumn.GridUnBoundRowEventsArgs.hasCellTemplate))
                    OnUpdateDisplayBinding(dataColumn, (D)rendererElement, record);
                else
                    OnUpdateTemplateBinding(dataColumn, (ContentControl)rendererElement, record);
            }
            else
                OnUpdateEditBinding(dataColumn, (E)rendererElement, record);
        }

        /// <summary>
        /// Update Tool Tip which show tool tip when mouse enter on GridUnBoundRowCell.
        /// </summary>
        /// <param name="dataColumn">Which hold GridUnBoundRowCell, GridColumn and Cell's row Column index</param>
        public override void UpdateToolTip(DataColumnBase dataColumn)
        {
            var uiElement = dataColumn.ColumnElement;
            var column = dataColumn.GridColumn;
            if (column == null || !column.ShowToolTip)
            {
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
                return;
            }
            var obj = ToolTipService.GetToolTip(uiElement);
            ToolTip tooltip;
            if (obj is ToolTip)
                tooltip = obj as ToolTip;
            else
                tooltip = new ToolTip();
            var dataContext = dataColumn.GridUnBoundRowEventsArgs;
            if (column.hasToolTipTemplate || column.hasToolTipTemplateSelector)
            {
                if (column.SetCellBoundToolTip)
                {
                    var dataContextHelper = new DataContextHelper
                    {
                        Record = dataContext,
                        Value = dataContext.Value
                    };
                    tooltip.Content = dataContextHelper;

                    if (column.hasToolTipTemplate)
                        tooltip.ContentTemplate = column.ToolTipTemplate;
                    else if (column.hasToolTipTemplateSelector)
                        tooltip.ContentTemplateSelector = column.ToolTipTemplateSelector;
                }
                else
                    tooltip.Content = dataContext.Value;
            }
            else
            {
                var displayText = Convert.ToString(dataContext.Value);
                tooltip.Content = displayText;
                if (string.IsNullOrEmpty(displayText))
                    tooltip.IsEnabled = false;
            }
            //Specifies to raise tooltip opening event for the corresponding cell
            if (dataColumn.RaiseCellToolTipOpening(tooltip))
                ToolTipService.SetToolTip(uiElement, tooltip);
            else
                uiElement.ClearValue(ToolTipService.ToolTipProperty);
        }

        /// <summary>
        /// Initialize the binding for GridUnBoundRowCell by Columns's CellTemplate and CellTemplateSelector.
        /// </summary>
        /// <param name="dataColumn">Specifies the dataColumn which contain GridColumn, RowColumnIndex</param>
        /// <param name="uiElement">Specifies the display control to initialize binding</param>
        /// <param name="dataContext">Specifies the data context of the particular row</param>
        public override void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            DataTemplate template = dataColumn.GridUnBoundRowEventsArgs != null &&
                            dataColumn.GridUnBoundRowEventsArgs.hasCellTemplate ?
                            dataColumn.GridUnBoundRowEventsArgs.CellTemplate : null;

            uiElement.ContentTemplate = template;

            OnUpdateTemplateBinding(dataColumn, uiElement, dataContext);
        }

        /// <summary>
        /// Updates the content for the GridUnBoundRowCell.
        /// </summary>
        /// <param name="dataColumn"></param>
        /// <param name="uiElement"></param>
        /// <param name="dataContext"></param>
        public override void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            var contentValue = (dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.Value != null) ?
                                        dataColumn.GridUnBoundRowEventsArgs.Value : string.Empty;

            uiElement.Content = contentValue;
        }

        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            var cell = dataColumn.ColumnElement;
            var column = dataColumn.GridColumn;
            if (column == null)
                return;

            var gridCell = cell as GridCell;
            if (gridCell == null) return;
            Style style = null;
            if (!DataGrid.hasUnBoundRowCellStyle)
            {
                if (gridCell.ReadLocalValue(FrameworkElement.StyleProperty) != DependencyProperty.UnsetValue)
                    gridCell.ClearValue(FrameworkElement.StyleProperty);
                return;
            }
            else            
                style = DataGrid.UnBoundRowCellStyle;

            if (style != null)
                gridCell.Style = style;
            else
                gridCell.ClearValue(FrameworkElement.StyleProperty);         
        }
        #region Public override methods
        public override bool CanUpdateBinding(GridColumn column)
        {
            return true;
        }

        public override bool CanValidate()
        {
            return false;
        }  
        #endregion
    }
}
