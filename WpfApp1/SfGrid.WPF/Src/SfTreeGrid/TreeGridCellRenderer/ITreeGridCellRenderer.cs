#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
#if UWP
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

#else
using System.Windows;
using System.Windows.Input;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid.Cells
{
#if UWP
    using KeyEventArgs = KeyRoutedEventArgs;
#endif
    /// <summary>
    /// Provides the functionality for all cell renderers in the SfTreeGrid.     
    /// </summary>  
    public interface ITreeGridCellRenderer : IDisposable
    {
        /// <summary>
        /// Gets or sets a value that indicates whether the current cell is editable or not.
        /// </summary>
        /// <value>
        /// <b>true</b> the current cell is editable; otherwise ,<b>false</b>.
        /// </value>
        bool IsEditable { get; set; }
        
        /// <summary>
        /// Gets or sets a value that indicates whether the cell is focusable.
        /// </summary>
        /// <value>
        /// <b>true</b> the current cell is focusable; otherwise ,<b>false</b>.
        /// </value>
        bool IsFocusable { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the cell that contains the drop-down control.
        /// </summary>
        /// <value>
        /// <b>true</b> if the cell is dropdownable; otherwise, <b>false</b>.
        /// </value>
        bool IsDropDownable { get; set; }

        bool UseOnlyRendererElement { get; set; }

        void UpdateCellStyle(TreeDataColumn treeDataColumn);

        /// <summary>
        /// Gets or sets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid"/> control.
        /// </summary>      
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid"/> control.
        /// </value>
        SfTreeGrid TreeGrid { get; set; }


        /// <summary>
        /// Gets a value that indicates whether the current cell state is maintained in SfTreeGrid.
        /// </summary>
        /// <value>
        /// Returns <b>true</b> if the current cell state is maintained; otherwise , <b>false</b>.
        /// </value>       
        bool HasCurrentCellState { get; }


        /// <summary>
        /// Gets the control value of the cell.
        /// </summary>
        /// <returns>
        /// Returns the control value as <c>null</c> by default .
        /// </returns>
        object GetControlValue();

        /// <summary>
        /// Sets the control value of the cell.
        /// </summary>
        /// <param name="value">
        /// Specifies the value to set the control value of the cell.
        /// </param>
        void SetControlValue(object value);

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
        bool ShouldGridTryToHandleKeyDown(KeyEventArgs e);

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
        void Arrange(RowColumnIndex cellRowColumnIndex,FrameworkElement uiElement, Rect cellRect);

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
        void Measure(RowColumnIndex cellRowColumnIndex,FrameworkElement uiElement, Size availableSize);

        /// <summary>
        /// Invoked when the UIElements are prepared for rendering in view .
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRendererBase"/> overrides this method and
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
        FrameworkElement PrepareUIElements(TreeDataColumnBase dataColumn, object record, bool isInEdit);

        /// <summary>
        /// Invoked when the cell is scrolled out of view or unloaded from the view.
        /// GridVirtualizingCellRendererBase&lt;D,E&gt; class overrides this method to remove the cell renderer visuals from the parent
        /// or hide them to reuse it later in same element depending on whether TreeGridVirtualizingCellRendererBase &lt; D,E &gt;.AllowRecycle was set.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to unload the cell UIElement.
        /// </param>        
        void UnloadUIElements(TreeDataColumnBase dataColumn);

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
        void UpdateBindingInfo(TreeDataColumnBase dataColumn, object record, bool isInEdit);

        /// <summary>
        /// Determines whether the cell validation is allowed. Implement this method to allow cell validation in particular renderer.        
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the cell validation is allowed.
        /// </returns>
        bool CanValidate();


        /// <summary>
        /// Determines whether the binding for the column can be updated. 
        /// Implement this method to update binding on particular renderer when the data context is set.
        /// </summary>
        /// <returns>
        /// Returns <b>true</b> if the binding is updated for the column.
        /// </returns>
        bool CanUpdateBinding(TreeGridColumn column);
        /// <summary>
        /// Updates the tool tip for the specified column.
        /// </summary>
        /// <param name="dataColumn">
        /// Specifies the column to update tool tip.
        /// </param> 
        void UpdateToolTip(TreeDataColumnBase dataColumn);
#if WPF
        /// <summary>
        /// Invoked when an unhanded PreviewTextInput attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/> that contains the event data.
        /// </param> 
        void PreviewTextInput(TextCompositionEventArgs args);
#else
        void PreviewTextInput(KeyEventArgs args);
#endif

        /// <summary>
        /// Sets the current cell state when the cell is activated.
        /// </summary>
        /// <param name="currentCellIndex">
        /// Specifies the index of cell.
        /// </param>
        /// <param name="currentCellElement">
        /// The corresponding current cell UIElement.
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
        void SetCurrentCellState(RowColumnIndex currentCellIndex, FrameworkElement currentCellElement, bool isInEditing, bool isFocused, TreeGridColumn column, TreeDataColumnBase dc);
     
        /// <summary>
        /// Resets the state of current cell when the cell is deactivated.
        /// </summary>
        void ResetCurrentCellState();

        /// <summary>
        /// Sets the focus to the current cell renderer element.
        /// </summary>
        /// <param name="setFocus">
        /// Specifies whether the current cell renderer element is focusable or not.
        /// </param>
        void SetFocus(bool setFocus);

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
        bool BeginEdit(RowColumnIndex cellRowColumnIndex, FrameworkElement cellElement, TreeGridColumn column, object record);

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
        bool EndEdit(TreeDataColumnBase dc,object record,bool canResetBinding = false);


        /// <summary>
        /// Updates the current binding target value to the binding source property in TwoWay or OneWayToSource bindings.
        /// </summary>
        /// <param name="cellElement">
        /// Specifies the corresponding cell element to update binding.
        /// </param>
        void UpdateSource(FrameworkElement cellElement);

        /// <summary>
        /// Clears the recycle bin.
        /// </summary>
        void ClearRecycleBin();

    }
}
