#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
#if WinRT || UNIVERSAL
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls;
using Syncfusion.Windows.Shared;
using System.Windows.Media;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif

    /// <summary>
    /// Represents a class that provides default implementation of the <see cref="Syncfusion.UI.Xaml.Grid.Cells.IGridCellRenderer"/> interface for a cell renderer.
    /// You should derive from this class to implement custom cell renderer classes. 
    /// There is however no dependency on GridCellRendererBase inside of the control. 
    /// <para/>
    /// If you want to implement a renderer with support for live UIElements inside the cell you should derive from the 
    /// <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase{TD, TE}"/> or grid adapted GridVirtualizingCellRendererBase classes.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridCellRendererBase : IGridCellRenderer
    {
        #region Fields

        private bool hasCurrentCellState;
        private FrameworkElement currentCellElement;
        private RowColumnIndex currentCellIndex;
        private bool isEditable=true;
        private bool isFocusible=true;
        private bool isDropDownable;
        private bool supportsRenderOptimization = true;
        private SfDataGrid dataGrid;
        private FrameworkElement currentCellRendererElement;
        internal bool isfocused;
        private bool isInEditing;
        private bool isdisposed = false;
        private bool canFocus = false;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridCellRenderBase"/> class.
        /// </summary>
        public GridCellRendererBase()
        {
            SupportsRenderOptimization = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the input text for the Renderer.
        /// </summary>
        /// <value>
        /// The preview input text for the renderer.
        /// </value>
#if WPF
        protected dynamic PreviewInputText { get; set; }
#else
        protected string PreviewInputText { get; set; }
#endif

        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </value>
        public SfDataGrid DataGrid
        {
            get { return dataGrid; }
            set { dataGrid = value; }
        }
        /// <summary>
        /// Gets or sets the <see cref="System.Windows.Data.BindingExpression"/> that represents the binding for the cell.
        /// </summary>
        /// <value>
        /// The <see cref="System.Windows.Data.BindingExpression"/> that specifies the binding for the cell.
        /// </value>
        public BindingExpression BindingExpression { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the renderer supports rendering itself directly to the
        /// drawing context. 
        /// </summary>      
        /// <value>
        /// <b>true</b> if the renderer supports directly to the drawing context ; otherwise, <b>false</b>.
        /// </value>
        public bool SupportsRenderOptimization 
        {
            get
            {
                return supportsRenderOptimization;
            }
            set
            {
                supportsRenderOptimization = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the grid cell is placed inside the renderer element.
        /// </summary>      
        /// <value>
        /// <b>true</b> if the grid cell is placed inside the renderer element; otherwise, <b>false</b>.
        /// </value>
        public bool UseOnlyRendererElement { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the current cell state is maintained in SfDataGrid.
        /// </summary>
        /// <value>
        /// Returns <b>true</b> if the current cell state is maintained; otherwise , <b>false</b>.
        /// </value>        
        public bool HasCurrentCellState
        {
            get { return hasCurrentCellState; }
        }

        /// <summary>
        /// Returns the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"> 
        /// Thrown when the CurrentCellIndex is accessed before the current cell is maintained. 
        /// </exception>
        public RowColumnIndex CurrentCellIndex
        {
            get
            {
                if (!HasCurrentCellState)
                     throw new InvalidOperationException("CellRowColumnIndex is only accessible when renderer is current cell. Check GridRenderStyleInfo.CellRowColumnIndex instead.");
                return currentCellIndex;
            }
        }

        /// <summary>
        /// Returns the element of the current cell.
        /// </summary>
        /// <exception cref="System.InvalidOperationException"> 
        /// Thrown when the current cell element is accessed before the current cell state is maintained.
        /// </exception>
        public FrameworkElement CurrentCellElement
        {
            get
            {
                if (!HasCurrentCellState)
                    throw new InvalidOperationException("CurrentCell Element is only accessible when renderer is current cell.");
                return currentCellElement;
            }
        }

        /// <summary>
        /// Returns the content of the current cell element.
        /// </summary>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the renderer element of the current cell is accessed before the current cell state is maintained.
        /// </exception>
        public FrameworkElement CurrentCellRendererElement
        {
            get
            {
                if(!HasCurrentCellState)
                    throw new InvalidOperationException("CurrentCell Renderer Element is only accessible when renderer is current cell.");
                return currentCellRendererElement;
            }
        }

        /// <summary>
        /// Returns the current cell is in editing or not.
        /// </summary>        
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the current cell is edited before the current cell state is maintained.
        /// </exception>
        public bool IsInEditing
        {
            get
            {
                if (!HasCurrentCellState)
                    throw new InvalidOperationException("IsInEditing only accessible when renderer is current cell.");
                return isInEditing;
            }
            set
            {
                isInEditing = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is focused or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if the current cell is focused; otherwise , <b>false</b>.
        /// </value>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown when the cell is focused before the current cell state is maintained.
        /// </exception>
        public bool IsFocused
        {
            get
            {
                if (!HasCurrentCellState)
                    throw new InvalidOperationException("IsFocused is only accessible when renderer is current cell.");
                return isfocused;
            }
            set
            {
                if (HasCurrentCellState)
                    isfocused = value;
                //if (HasCurrentCellState)
                //    SetFocus(currentCellRendererElement, value);
            }
        }

        /// <summary>
        /// Can Focus to CurrentCellRendererElmement at loaded.
        /// </summary>
        internal bool CanFocus
        {
            get { return canFocus; }
            set { canFocus = value; }
        }

        #endregion

        #region virtual method           

        /// <summary>
        /// Invoked when the UIElement for cell is prepared to render it in view .
        /// <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase"/> overrides this method and
        /// creates new UIElements and wires them with the parent cells control.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding column of the element.
        /// </param>
        /// <param name="record">
        /// The corresponding Record for the element.
        /// </param>
        /// <param name="isInEdit">
        /// Specifies whether the element is editable or not.
        /// </param>
        /// <returns>
        /// Returns the new cell UIElement.
        /// </returns>
        protected internal virtual FrameworkElement OnPrepareUIElements(DataColumnBase dataColumn,object record, bool isInEdit)
        {
            return OnPrepareUIElements();
        }

        /// <summary>
        /// Invoked when the cell elements are prepared for rendering in view .
        /// </summary>
        /// <returns>
        /// Returns the new cell UIElement.
        /// </returns>
        protected virtual FrameworkElement OnPrepareUIElements()
        {
            var gridCell = this.DataGrid.RowGenerator.GetGridCell<GridCell>();
            return gridCell;
        }
        /// <summary>
        /// Invoked when an unhanded PreviewTextInput attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/> that contains the event data.
        /// </param>        
#if !WinRT && !UNIVERSAL
        protected virtual void OnPreviewTextInput(TextCompositionEventArgs e)
#else
        protected virtual void OnPreviewTextInput(KeyEventArgs e)
#endif
        {
#if !WinRT && !WP && !UNIVERSAL
            if (HasCurrentCellState)
                PreviewInputText = e.Text;
#endif
        }

        /// <summary>
        /// Determines whether the cell validation is allowed. Implement this method to allow cell validation in particular renderer.        
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell validation is allowed.
        /// </returns>
        public virtual bool CanValidate()
        {
            return true;
        }

        /// <summary>
        /// Determines whether the binding for the column can be updated. 
        /// Implement this method to update binding on particular renderer when the data context is set.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the binding is updated for the column.
        /// </returns>
        public virtual bool CanUpdateBinding(GridColumn column)
        {
            return (column != null && column.canUpdateBinding);
        }

        /// <summary>
        /// Commits the changes in the unbound cell where the renderer doesn't support value changed event.
        /// </summary>
        /// <param name="record">
        /// Specifies the corresponding record to commit the cell value.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column to commit the cell value.
        /// </param>
        /// <param name="value">
        /// Specifies the cell value to commit it.
        /// </param>
        public void CommitUnBoundCellValue(object record, GridColumn column, object value)
        {
            var args = new GridUnboundColumnEventsArgs(UnBoundActions.CommitData, value, column, record, this.DataGrid);
            DataGrid.RaiseQueryUnboundValue(args);
        }
        
        /// <summary>
        /// Updates the tool tip for the specified column.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to update tool tip.
        /// </param>        
        public virtual void UpdateToolTip(DataColumnBase dataColumn)
        {
                          
        }

        /// <summary>
        /// Invoked when the visual children of cell is arranged in view. 
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell.
        /// </param>
        /// <param name="uiElement">
        /// The corresponding UiElement that is to be arranged
        /// </param>
        /// <param name="cellRect">
        /// The corresponding size of cell element for arranging the UIElement
        /// </param>        
        protected virtual void OnArrange(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Rect cellRect)
        {

        }

        /// <summary>
        /// Invoked when the desired size for cell is measured.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell</param>
        /// <param name="uiElement">
        /// Specifies the corresponding UiElement to measure.
        /// </param>
        /// <param name="availableSize">
        /// The available size that a parent element can allocate the cell.
        /// </param>        
        protected virtual void OnMeasure(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Size availableSize)
        {

        }
#if WPF

        /// <summary>
        /// Invoked when the visual children of cells is render in view.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="dataColumnBase">The DataColumnBase that provides the cells details.</param>
        /// <param name="dataContext">The corresponding record to draw a text for GridCell</param>        
        protected virtual void OnRenderCell(DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {

        }
     
        /// <summary>
        /// Invoked to render the cells borders.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="clipGeometry">The geometry which apply Clip on drawing context.</param>
        /// <param name="dataColumnBase">The DataColumnBase that provides the cells details.</param>        
        /// <param name="gridCell"></param>                
        protected virtual void OnRenderCellBorder(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell)
        {

        }


        /// <summary>
        /// Invoked to render the cells content.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="clipGeometry">The geometry which apply Clip on drawing context.</param>
        /// <param name="dataColumnBase">The DataColumnBase that provides the cells details.</param>        
        /// <param name="gridCell"></param>
        /// <param name="dataContext">The corresponding data context to draw the content for cell.</param>        
        protected virtual void OnRenderContent(DrawingContext dc, Rect cellRect, Geometry clipGeometry, DataColumnBase dataColumnBase, GridCell gridCell, object dataContext)
        {

        }
       
        /// <summary>
        /// Invoked to render the selected cells background, borders.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="clipGeometry">The geometry which apply Clip on drawing context.</param>
        /// <param name="dataColumnBase">The DataColumnBase that provides the cells details.</param>        
        /// <param name="gridCell"></param>
        protected virtual void OnRenderCurrentCell(DrawingContext dc, Rect cellRect,Geometry clipGeometry,DataColumnBase dataColumnBase, GridCell gridCell)
        {

        }
#endif
        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// GridVirtualizingCellRendererBase&lt;D,E&gt; class overrides this method to remove the cell renderer visuals from the parent
        /// or hide them to reuse it later in same element depending on whether GridVirtualizingCellRendererBase &lt; D,E &gt;.AllowRecycle was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>        
        protected virtual void OnUnloadUIElements(DataColumnBase dataColumn)
        {

        }

        /// <summary>
        /// Updates the binding of the Cell UIElement for the specified column.
        /// Implement this method to update binding when the cell UIElement is reused during horizontal scrolling.        
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update binding.
        /// </param>
        /// <param name="record">
        /// The corresponding record to update binding.
        /// </param>
        /// <param name="isInEdit">
        /// Indicates the whether the cell is editable or not.
        /// </param>     
        protected internal virtual void OnUpdateBindingInfo(DataColumnBase dataColumn,object record, bool isInEdit)
        {

        }

        /// <summary>
        /// Updates the style for the particular column.
        /// Implement this method to update style when the cell UIElement is reused during scrolling.          
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update style.
        /// </param>        
        protected virtual void OnUpdateStyleInfo(DataColumnBase dataColumn, object dataContext)
        {

        }

        /// <summary>
        /// Decides whether the parent grid should allowed to handle keys and prevent
        /// the key event from being handled by the visual UIElement for this renderer.
        /// </summary>
        /// <param name="e">
        /// A <see cref="KeyEventArgs"/> that contains event data.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the parent grid should be allowed to handle keys; otherwise <b>false</b>.
        /// </returns>
        protected virtual bool ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            return true;
        }
       
     
        /* Work flow.           
           Cals From
           SetCurrentCellState - in non edit mode. 
           OnArrange of GridCellTemplateRenderer - edit mode.
           OnArrange of GridCellUnBoundTemplateRenderer- edit mode.
           DataRowBase while scrolling vertically - edit mode.
           DataRow while scrolling horizontally - edit mode.
           EndEdit from GridVirtualizingCellRendererBase and GridUnboundRowCellRenderer
         
           Process:
           Sets focus to DataGrid when cell is in non edit mode.
           Sets focus to with Focused element loaded within CellTemplate in non edit mode also. inedit mdoe with EditTemplate focused element.                    
        */

        /// <summary>
        /// Sets the focus to the specified current cell uielement.
        /// </summary>
        /// <param name="uiElement">
        /// Specifies the corresponding current cell uielement.
        /// </param>
        /// <param name="needToFocus">
        /// Decides whether the focus set to current cell uielement.
        /// </param>
        protected virtual void SetFocus(FrameworkElement uiElement, bool needToFocus)
        {
            //UWP-1450-after clearing columns,skip to access that column based on below condition.
            if (dataGrid.Columns.Count == 0)
                return;
            if (uiElement != null && IsFocusible)
            {
                UIElement uielement;
                
                var focusedElement = GetFocusedUIElement(CurrentCellRendererElement);
                uielement = focusedElement ?? uiElement;

                var columnIndex = DataGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex);
                var column = DataGrid.Columns[columnIndex];
                //SupportsRenderOptimization condition checked to move the Focus always to CheckBox instead of DataGrid - WPF-22403
                if (needToFocus && (IsInEditing || column.CanFocus() || !SupportsRenderOptimization))
                {
                    if (IsFocused)
                    {
#if WPF
                        //WPF-25805 - Sets the focus to uielement if IsFocused is true and uielement.IsFocused is false
                        if (!uielement.IsFocused)
                        {
                            uielement.Focusable = true;
                            Keyboard.Focus(uielement);
                        }
#endif
                        return;
                    }
#if WPF
                    uielement.Focus();
#else                
                    if (uielement is Control)
                    {
                        (uielement as Control).Focus(FocusState.Programmatic);
                        isfocused = false;
                        return;
                    }
#endif
                    isfocused = true;
                }
                else
                {
#if !WinRT && !UNIVERSAL
                    this.DataGrid.Focus();
#else
                    this.DataGrid.Focus(FocusState.Programmatic);
#endif
                    isfocused = false;
                }
            }
            else
            {
#if !WinRT && !UNIVERSAL
            DataGrid.Focus();
#else
            DataGrid.Focus(FocusState.Programmatic);
#endif
            isfocused = false;
            }
        }

        /// <summary>
        /// Gets a control that needs to get focus.
        /// </summary>
        /// <param name="uiElement">The FrameworkElement to set focus.</param>
        /// <returns>Returns the FramewrokElement which can get focus.</returns>
        /// <remarks>
        /// GridCellMultiColumnDropDownRenderer returns editor to set focus.
        /// </remarks>
        internal virtual Control GetFocusedUIElement(FrameworkElement uiElement)
        {
            return FocusManagerHelper.GetFocusedUIElement(CurrentCellRendererElement);                
        }

        #endregion

        #region IGridCellRenderer

        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is editable or not.
        /// </summary>
        /// <value>
        /// <b>true</b> the current cell is editable; otherwise ,<b>false</b>.
        /// </value>
        public bool IsEditable
        {
            get
            {
                return isEditable;
            }
            set
            {
                isEditable = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the cell is focusable.
        /// </summary>
        /// <value>
        /// <b>true</b> the current cell is focusable; otherwise ,<b>false</b>.
        /// </value>
        public bool IsFocusible
        {
            get
            {
                return isFocusible;
            }
            set
            {
                isFocusible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the cell that contains the drop-down control.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell is dropdownable; otherwise, <b>false</b>.
        /// </value>
        public bool IsDropDownable
        {
            get { return isDropDownable; }
            set { isDropDownable = value; }
        }
        /// <summary>
        /// Decides whether the parent grid should allowed to handle keys and prevent
        /// the key event from being handled by the visual UIElement for this renderer.
        /// </summary>
        /// <param name="e">A <see cref="KeyEventArgs"/> that contains event data.</param>
        /// <returns><b>true</b> if the parent grid should be allowed to handle keys; otherwise <b>false</b>.</returns>
        bool IGridCellRenderer.ShouldGridTryToHandleKeyDown(KeyEventArgs e)
        {
            if (!HasCurrentCellState) return true;
            return ShouldGridTryToHandleKeyDown(e);
        }

        /// <summary>
        /// Invoked when the visual children of cell is arranged in view. 
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell.
        /// </param>
        /// <param name="uiElement">
        /// The corresponding UiElement that is to be arranged
        /// </param>
        /// <param name="cellRect">
        /// The corresponding size of cell element for arranging the UIElement
        /// </param>   
        public void Arrange(RowColumnIndex cellRowColumnIndex,FrameworkElement uiElement, Rect cellRect)
        {
            OnArrange(cellRowColumnIndex,uiElement, cellRect);
        }
#if WPF
        /// <summary>
        /// Invoked when the visual children of cells is render in view.
        /// </summary>
        /// <param name="dc">The corresponding drawing context to draw the cell borders, cell content, cell background</param>
        /// <param name="cellRect">The corresponding size of cell element for arranging the UIElement</param>
        /// <param name="dataColumnBase"></param>
        /// <param name="dataContext">The corresponding record to draw a text for GridCell</param>                
        public void RenderCell(DrawingContext dc, Rect cellRect, DataColumnBase dataColumnBase, object dataContext)
        {
            OnRenderCell(dc, cellRect, dataColumnBase, dataContext);
        }     
#endif
        /// <summary>
        /// Invoked when the desired size for cell is measured.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// The corresponding row and column index of the cell</param>
        /// <param name="uiElement">
        /// Specifies the corresponding UiElement to measure.
        /// </param>
        /// <param name="availableSize">
        /// The available size that a parent element can allocate the cell.
        /// </param>   
        public void Measure(RowColumnIndex cellRowColumnIndex,FrameworkElement uiElement, Size availableSize)
        {
            OnMeasure(cellRowColumnIndex,uiElement, availableSize);
        }

        /// <summary>
        /// Invoked when the UIElement for cell is prepared to render it in view .
        /// <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRendererBase"/> overrides this method and
        /// creates new UIElements and wires them with the parent cells control.
        /// </summary>
        /// <param name="dataColumn">
        /// The corresponding column of the element.
        /// </param>
        /// <param name="record">
        /// The corresponding Record for the element.
        /// </param>
        /// <param name="isInEdit">
        /// Specifies whether the element is editable or not.
        /// </param>
        /// <returns>
        /// Returns the new cell UIElement.
        /// </returns>
        public FrameworkElement PrepareUIElements(DataColumnBase dataColumn,object record, bool isInEdit)
        {
            return OnPrepareUIElements(dataColumn,record,isInEdit);
        }

        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// GridVirtualizingCellRendererBase&lt;D,E&gt; class overrides this method to remove the cell renderer visuals from the parent
        /// or hide them to reuse it later in same element depending on whether GridVirtualizingCellRendererBase &lt; D,E &gt;.AllowRecycle was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>        
        public void UnloadUIElements(DataColumnBase dataColumn)
        {
            OnUnloadUIElements(dataColumn);
        }

        /// <summary>
        /// Updates the binding of the Cell UIElement for the specified column.
        /// Implement this method to update binding when the cell UIElement is reused during horizontal scrolling.        
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update binding.
        /// </param>
        /// <param name="record">
        /// The corresponding record to update binding.
        /// </param>
        /// <param name="isInEdit">
        /// Indicates the whether the cell is editable or not.
        /// </param>     
        void IGridCellRenderer.UpdateBindingInfo(DataColumnBase dataColumn,object record, bool isInEdit)
        {
            OnUpdateBindingInfo(dataColumn,record, isInEdit);
        }

        protected void UpdateBindingInfo(DataColumnBase dataColumn, object record, bool isInEdit)
        {
            OnUpdateBindingInfo(dataColumn, record, isInEdit);
        }
        /// <summary>
        /// Updates the cell style of the particular column.
        /// Implement this method to update style when the cell UIElement is reused during scrolling.          
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the corresponding column to update style.
        /// </param>
        public void UpdateCellStyle(DataColumnBase dataColumn, object dataContext)
        {
            OnUpdateStyleInfo(dataColumn,dataContext);
        }


        /// <summary>
        /// Invoked when an unhanded PreviewTextInput attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/> that contains the event data.
        /// </param>   
#if !WinRT && !UNIVERSAL
        public void PreviewTextInput(TextCompositionEventArgs args)
#else
        public void PreviewTextInput(KeyEventArgs args)
#endif
        {
            if (!HasCurrentCellState) 
                return;
            OnPreviewTextInput(args);
        }
        /// <summary>
        /// Gets the control value of the cell.
        /// </summary>
        /// <returns>
        /// Returns the control value as <c>null</c> by default .
        /// </returns>
        public virtual object GetControlValue()
        {
            return null;
        }
        /// <summary>
        /// Sets the control value of the cell.
        /// </summary>
        /// <param name="value">
        /// Specifies the value to set the control value of the cell.
        /// </param>
        public virtual void SetControlValue(object value)
        {
            
        }

        /// <summary>
        /// Sets the current cell state when the cell is activated.
        /// </summary>
        /// <param name="currentCellIndex">
        /// Specifies the index of cell.
        /// </param>
        /// <param name="currentCellElement">
        /// The corresponding current cell uielement.
        /// </param>
        /// <param name="isInEditing">
        /// Specifies whether the current cell is editable or not.
        /// </param>
        /// <param name="isFocused">
        /// Specifies whether the current cell is focused or not.
        /// </param>
        /// <param name="column">
        /// The corresponding column to set the current cell state.
        /// </param>
        /// <param name="dc">
        /// The corresponding data column to set the current cell state.
        /// </param>        
        public void SetCurrentCellState(RowColumnIndex currentCellIndex, FrameworkElement currentCellElement, bool isInEditing, bool isFocused, GridColumn column, DataColumnBase dc)
        {
            //if (hasCurrentCellState)
            //    throw new InvalidOperationException("Try to set duplicate current cell");
            hasCurrentCellState = true;
            this.currentCellIndex = currentCellIndex;
            if (UseOnlyRendererElement)
            {
                this.currentCellElement = null;
                currentCellRendererElement = currentCellElement;
            }
            else
            {
                this.currentCellElement = currentCellElement;
                if(dc.isUnBoundRowCell)
                {
                    if (!((this.currentCellElement as GridCell).ColumnBase.GridUnBoundRowEventsArgs.hasCellTemplate))
                        currentCellRendererElement = (currentCellElement as GridCell).Content as FrameworkElement;
                    else
                        currentCellRendererElement = currentCellElement as FrameworkElement;
                }
                else if (currentCellElement is GridCell)
                {
                    if (column.hasCellTemplate || column.hasCellTemplateSelector)
                        currentCellRendererElement = currentCellElement as FrameworkElement;
                    else
                        currentCellRendererElement = (currentCellElement as GridCell).Content as FrameworkElement; 
                        
                }

            }
            IsInEditing = isInEditing;
            if (isFocused)
            {
                this.SetFocus(this.IsFocusible);
                //IsFocused = this.IsFocusible;
            }
        }

        /// <summary>
        /// Updates the current cell state for the specified current cell renderer element.
        /// </summary>
        /// <param name="currentRendererElement">
        /// Specifies the current cell uielement to update the current cell state.
        /// </param>
        /// <param name="isInEdit">
        /// Specifies whether the current cell is editable.
        /// </param>
        protected void UpdateCurrentCellState(FrameworkElement currentRendererElement, bool isInEdit)
        {
            currentCellRendererElement = currentRendererElement;
            isInEditing = isInEdit;
        }

        /// <summary>
        /// Resets the state of current cell when the cell is deactivated.
        /// </summary>
        public void ResetCurrentCellState()
        {
            hasCurrentCellState = false;
            currentCellIndex = RowColumnIndex.Empty;
            currentCellElement = null;
            currentCellRendererElement = null;
            isfocused = false;
            isInEditing = false;
        }
        /// <summary>
        /// Sets the focus to the current cell renderer element.
        /// </summary>
        /// <param name="setFocus">
        /// Specifies whether the current cell renderer element is focusable or not.
        /// </param>
        public void SetFocus(bool setFocus)
        {
            SetFocus(currentCellRendererElement, setFocus);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event.
        /// </summary>
        /// <param name="dataGrid">
        /// Specifies the corresponding grid .
        /// </param>
        /// <param name="column">
        /// The corresponding column that was in edit mode.
        /// </param>
        /// <param name="oldValue">
        /// The cell value before edited.
        /// </param>
        /// <param name="newValue">
        /// The value of the current cell is being edited.
        /// </param>
        /// <param name="changedNewValue">
        /// The cell value after editing completed.
        /// </param>
        /// <returns>
        /// <b>true</b> if <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        public bool RaiseCurrentCellValidatingEvent(SfDataGrid dataGrid, GridColumn column, object oldValue, object newValue, out object changedNewValue)
        {
            var e = new CurrentCellValidatingEventArgs(dataGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column
            };
            bool isSuspendValidating = DataGrid.RaiseCurrentCellValidatingEvent(e);
            changedNewValue = e.NewValue;
            return isSuspendValidating;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidated"/> event.
        /// </summary>
        /// <param name="dataGrid">
        /// Specifies the corresponding grid .
        /// </param>
        /// <param name="column">
        /// The corresponding column that was in edit mode.
        /// </param>
        /// <param name="oldValue">
        /// The cell value before edited.
        /// </param>
        /// <param name="newValue">
        /// The cell value after editing completed.
        /// </param>      
        public void RaiseCurrentCellValidatedEvent(SfDataGrid dataGrid, GridColumn column,object oldValue, object newValue)
        {
            var e = new CurrentCellValidatedEventArgs(dataGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column
            };
            DataGrid.RaiseCurrentCellValidatedEvent(e);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValueChanged"/> event.
        /// </summary>
        /// <param name="dataGrid">
        /// The corresponding grid that contains the cell value changes .
        /// </param>
        /// <param name="dataColumn">
        /// The data column Which holds GridColumn, RowColumnIndex and GridCell.
        /// </param>
        public void RaiseCurrentCellValueChangedEvent( SfDataGrid dataGrid,DataColumnBase dataColumn)
        {
            var e = new CurrentCellValueChangedEventArgs(dataGrid)
            {
                RowColumnIndex = new RowColumnIndex() { RowIndex = dataColumn.RowIndex, ColumnIndex = dataColumn.ColumnIndex },
                Record = currentCellRendererElement.DataContext,
                Column=dataColumn.GridColumn
            };
            DataGrid.RaiseCurrentCellValueChangedEvent(e);
        }

        /// <summary>
        /// Starts an edit operation on a current cell.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// Specifies the row and column index of the cell to start an edit operation.
        /// </param>
        /// <param name="cellElement">
        /// Specifies the UIElement of the cell to start an edit operation.
        /// </param>
        /// <param name="column">
        /// The corresponding column to edit the cell.
        /// </param>
        /// <param name="record">
        /// The corresponding record to edit the cell.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the current cell starts an editing; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, GridColumn column, object record)
        {
            return HasCurrentCellState && IsInEditing;
        }

        /// <summary>
        /// Ends the edit occurring on the cell.
        /// </summary>
        /// <param name="dc">
        /// The corresponding data column to complete the edit operation.
        /// </param>
        /// <param name="record">
        /// The corresponding record to complete the edit operation.
        /// </param>
        /// <param name="canResetBinding">
        /// Specifies whether the binding is reset or not.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the editing is completed ; otherwise, <b>false</b>.
        /// </returns>
        public virtual bool EndEdit(DataColumnBase dc,object record,bool canResetBinding = false)
        {
            return HasCurrentCellState && !IsInEditing;
        }

        /// <summary>
        /// Updates the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.
        /// </summary>
        /// <param name="cellElement">
        /// Specifies the corresponding cell element to update binding.
        /// </param>
        public virtual void UpdateSource(FrameworkElement cellElement)
        {
            if (IsInEditing && BindingExpression != null 
#if WPF
                && BindingExpression.Status != BindingStatus.Detached
#endif
                )
                BindingExpression.UpdateSource();
        }
        /// <summary>
        /// Clears the recycle bin.
        /// </summary>
        public virtual void ClearRecycleBin()
        {
            
        }
        #endregion

        #region Dispose method

        /// <summary>
        /// Releases all resources used by the <see cref="T:Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererBase"/> class.
        /// </summary>       
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="T:Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                currentCellElement = null;
                currentCellRendererElement = null;
                dataGrid = null;
            }
            isdisposed = true;
        }

        #endregion

    }

    /// <summary>
    /// Provides classes and interface to renderer different cells in SfDataGrid 
    /// </summary>
    class NamespaceDoc
    { 
    }
}
