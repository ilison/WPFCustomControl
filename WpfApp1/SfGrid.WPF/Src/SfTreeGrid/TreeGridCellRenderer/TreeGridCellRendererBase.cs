#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using Syncfusion.UI.Xaml.Grid;
#if UWP
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Controls;
using Syncfusion.Windows.Shared;
#endif


namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
#endif

    /// <summary>
    /// Represents a class that provides default implementation of the <see cref="Syncfusion.UI.Xaml.Grid.Cells.IGridCellRenderer"/> interface for a cell renderer.
    /// You should derive from this class to implement custom cell renderer classes. 
    /// There is however no dependency on GridCellRendererBase inside of the control. 
    /// <para/>
    /// If you want to implement a renderer with support for live UIElements inside the cell you should derive from the 
    /// <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase{TD, TE}"/> or grid adapted GridVirtualizingCellRendererBase classes.
    /// </summary>    
    public class TreeGridCellRendererBase : ITreeGridCellRenderer
    {
        #region Fields

        private bool hasCurrentCellState;
        private FrameworkElement currentCellElement;
        private RowColumnIndex currentCellIndex;
        private bool isEditable = true;
        private bool isFocusable = true;
        private bool isDropDownable;
        private bool supportsRenderOptimization = true;
        private SfTreeGrid treeGrid;
        private FrameworkElement currentCellRendererElement;
        internal bool isfocused;
        private bool isInEditing;
        private bool canFocus = false;

        #endregion

        #region Ctor
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridCellRenderBase"/> class.
        /// </summary>
        public TreeGridCellRendererBase()
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
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </value>
        public SfTreeGrid TreeGrid
        {
            get { return treeGrid; }
            set { treeGrid = value; }
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
                if (!HasCurrentCellState)
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
        /// <see cref="Syncfusion.UI.Xaml.Grid.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase"/> overrides this method and
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
        protected internal virtual FrameworkElement OnPrepareUIElements(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            throw new NotImplementedException("OnPrepareUIElements not implemented");
        }

        /// <summary>
        /// Invoked when an unhanded PreviewTextInput attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/> that contains the event data.
        /// </param> 
#if UWP
        protected virtual void OnPreviewTextInput(KeyEventArgs e)
#else
     protected virtual void OnPreviewTextInput(TextCompositionEventArgs e)

#endif
        {
#if WPF
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
        public virtual bool CanUpdateBinding(TreeGridColumn column)
        {
            return (column != null && column.canUpdateBinding);
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
            OnArrange(cellRowColumnIndex, uiElement, cellRect);
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

        /// <summary>
        /// Updates the tool tip for the specified column.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to update tool tip.
        /// </param>
        public virtual void UpdateToolTip(TreeDataColumnBase dataColumn)
        {
        }
        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// GridVirtualizingCellRendererBase&lt;D,E&gt; class overrides this method to remove the cell renderer visuals from the parent
        /// or hide them to reuse it later in same element depending on whether GridVirtualizingCellRendererBase &lt; D,E &gt;.AllowRecycle was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>        
        protected virtual void OnUnloadUIElements(TreeDataColumnBase dataColumn)
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
        protected internal virtual void OnUpdateBindingInfo(TreeDataColumnBase dataColumn, object record, bool isInEdit)
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
            if (uiElement != null && IsFocusable)
            {
                UIElement uielement;
                var focusedElement = FocusManagerHelper.GetFocusedUIElement(CurrentCellRendererElement);
                uielement = focusedElement ?? uiElement;

                TreeGridColumn column = null;
                if (needToFocus)
                {
                    var columnIndex = TreeGrid.ResolveToGridVisibleColumnIndex(CurrentCellIndex.ColumnIndex);
                    column = TreeGrid.Columns[columnIndex];
                }

                //SupportsRenderOptimization condition checked to move the Focus always to CheckBox instead of DataGrid - WPF-22403
                if (needToFocus && (IsInEditing || column.CanFocus() || !SupportsRenderOptimization))
                {
                    if (IsFocused)
                    {
#if WPF
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
#if WPF
                    this.TreeGrid.Focus();
#else
                    this.treeGrid.Focus(FocusState.Programmatic);
#endif
                    isfocused = false;
                }
            }
            else
            {
#if WPF
                TreeGrid.Focus();
#else
                TreeGrid.Focus(FocusState.Programmatic);
#endif
                isfocused = false;
            }
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
        public bool IsFocusable
        {
            get
            {
                return isFocusable;
            }
            set
            {
                isFocusable = value;
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
        bool ITreeGridCellRenderer.ShouldGridTryToHandleKeyDown(KeyEventArgs e)
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
        public void Arrange(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Rect cellRect)
        {
            OnArrange(cellRowColumnIndex, uiElement, cellRect);
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
        public void Measure(RowColumnIndex cellRowColumnIndex, FrameworkElement uiElement, Size availableSize)
        {
            OnMeasure(cellRowColumnIndex, uiElement, availableSize);
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
        public FrameworkElement PrepareUIElements(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            return OnPrepareUIElements(dataColumn, record, isInEdit);
        }

        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// GridVirtualizingCellRendererBase&lt;D,E&gt; class overrides this method to remove the cell renderer visuals from the parent
        /// or hide them to reuse it later in same element depending on whether GridVirtualizingCellRendererBase &lt; D,E &gt;.AllowRecycle was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>        
        public void UnloadUIElements(TreeDataColumnBase dataColumn)
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
        void ITreeGridCellRenderer.UpdateBindingInfo(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            UpdateBindingInfo(dataColumn, record, isInEdit);
        }

        protected void UpdateBindingInfo(TreeDataColumnBase dataColumn, object record, bool isInEdit)
        {
            OnUpdateBindingInfo(dataColumn, record, isInEdit);
        }

        /// <summary>
        /// Invoked when an unhanded PreviewTextInput attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/> that contains the event data.
        /// </param>   
#if WPF
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
        public void SetCurrentCellState(RowColumnIndex currentCellIndex, FrameworkElement currentCellElement, bool isInEditing, bool isFocused, TreeGridColumn column, TreeDataColumnBase dc)
        {
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
                if (currentCellElement is TreeGridCell)
                {
                    if (column.hasCellTemplate || column.hasCellTemplateSelector)
                        currentCellRendererElement = currentCellElement as FrameworkElement;
                    else
                        currentCellRendererElement = (currentCellElement as TreeGridCell).Content as FrameworkElement;
                }
            }
            IsInEditing = isInEditing;
            if (isFocused)
                this.SetFocus(this.IsFocusable);
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
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidating"/> event.
        /// </summary>
        /// <param name="treeGrid">
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
        /// <b>true</b> if <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidating"/> event is raised; otherwise, <b>false</b>.
        /// </returns>
        protected bool RaiseCurrentCellValidatingEvent(SfTreeGrid treeGrid, TreeGridColumn column, object oldValue, object newValue, out object changedNewValue, object record, TreeNode treeNode)
        {
            var e = new TreeGridCurrentCellValidatingEventArgs(treeGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                RowData = record,
                Node = treeNode
            };
            bool isSuspendValidating = TreeGrid.RaiseCurrentCellValidatingEvent(e);
            changedNewValue = e.NewValue;
            return isSuspendValidating;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValidated"/> event.
        /// </summary>
        /// <param name="treeGrid">
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
        protected void RaiseCurrentCellValidatedEvent(SfTreeGrid treeGrid, TreeGridColumn column, object oldValue, object newValue, object record, TreeNode treeNode)
        {
            var e = new TreeGridCurrentCellValidatedEventArgs(treeGrid)
            {
                OldValue = oldValue,
                NewValue = newValue,
                Column = column,
                RowData = record,
                Node = treeNode
            };
            TreeGrid.RaiseCurrentCellValidatedEvent(e);
        }


        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentCellValueChanged"/> event.
        /// </summary>
        /// <param name="treeGrid">
        /// The corresponding grid that contains the cell value changes .
        /// </param>
        /// <param name="dataColumn">
        /// The data column Which holds TreeGridColumn, RowColumnIndex and TreeGridCell.
        /// </param>
        public void RaiseCurrentCellValueChangedEvent(SfTreeGrid treeGrid, TreeDataColumnBase dataColumn)
        {
            var e = new TreeGridCurrentCellValueChangedEventArgs(treeGrid)
            {
                RowColumnIndex = new RowColumnIndex() { RowIndex = dataColumn.RowIndex, ColumnIndex = dataColumn.ColumnIndex },
                Record = currentCellRendererElement.DataContext,
                Column = dataColumn.TreeGridColumn
            };
            TreeGrid.RaiseCurrentCellValueChangedEvent(e);
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
        public virtual bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, TreeGridColumn column, object record)
        {
            return HasCurrentCellState && IsInEditing;
        }

        /// <summary>
        /// Ends the edit occurring on the cell.
        /// </summary>
        /// <param name="dc">
        /// The corresponding datacolumn to complete the edit operation.
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
        public virtual bool EndEdit(TreeDataColumnBase dc, object record, bool canResetBinding = false)
        {
            return HasCurrentCellState && !IsInEditing;
        }

        public void UpdateSource(FrameworkElement cellElement)
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
        /// Releases all resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridCellRendererBase"/> class.
        /// </summary>  
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridCellRendererBase"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            currentCellElement = null;
            currentCellRendererElement = null;
            treeGrid = null;
        }

        /// <summary>
        /// Updates the cell style of the particular column.
        /// Implement this method to update style when the cell UIElement is reused during scrolling.          
        /// </summary>
        /// <param name="treeDataColumn">
        /// Specifies the corresponding column to update style.
        /// </param>
        public void UpdateCellStyle(TreeDataColumn treeDataColumn)
        {
            OnUpdateStyleInfo(treeDataColumn);
        }

        protected virtual void OnUpdateStyleInfo(TreeDataColumn treeDataColumn)
        {

        }

#endregion
    }
}
