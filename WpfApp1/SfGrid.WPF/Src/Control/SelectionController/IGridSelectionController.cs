#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
#else
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;

#endif

#if WPF
using DoubleTappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
using TappedEventArgs = System.Windows.Input.MouseButtonEventArgs;
#endif


namespace Syncfusion.UI.Xaml.Grid
{

    /// <summary>
    /// Provides the common functionality of selection behavior in SfDataGrid.     
    /// </summary>    
    [ClassReference(IsReviewed = false)]
    public interface IGridSelectionController : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Returns the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridRowInfo"/> that contains the information of selected rows.
        /// </summary>
        GridSelectedRowsCollection SelectedRows { get; }

        /// <summary>
        /// Returns the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSelectedCellsInfo"/> that contains the information selected cells .
        /// </summary>
        GridSelectedCellsCollection SelectedCells { get; }

        /// <summary>
        /// Gets or sets a brush that highlights the background of the currently selected row or cell.
        /// </summary>
        /// <value>
        /// The brush that highlights the background of the selected row or cell.
        /// </value>
        [Obsolete]
        Brush RowSelectionBrush { get; set; }

        /// <summary>
        /// Gets or sets a brush that highlights the background of currently selected group caption and group summary rows.
        /// </summary>    
        /// <value>
        /// The brush that highlights the background of currently selected group row.
        /// </value>
        [Obsolete]
        Brush GroupRowSelectionBrush { get; set; }

        /// <summary>
        /// Gets or sets a brush that highlights the background of data row is being hovered. 
        /// </summary>
        /// <value>
        /// The brush that highlights the data row is being hovered.
        /// </value>
        [Obsolete]
        Brush RowHoverBackgroundBrush { get; set; }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.GridCurrentCellManager"/> instance which holds the current cell information.
        /// </summary>
        GridCurrentCellManager CurrentCellManager { get; }

        /// <summary>
        /// Handles the selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.GridOperation"/> such as Sorting,Filtering,Grouping and etc that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridOperationsHandlerArgs"/> that contains the type of grid operations and its arguments.
        /// </param>       
        void HandleGridOperations(GridOperationsHandlerArgs args);

        /// <summary>
        /// Handles the selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridPointerEventArgs"/> that contains the type of pointer operations and its arguments.
        /// </param>
        /// <param name="rowColumnIndex">
        /// The corresponding rowColumnIndex.
        /// </param>       
        void HandlePointerOperations(GridPointerEventArgs args, RowColumnIndex rowColumnIndex);

        /// <summary>
        /// Handles the selection when any of the selection property such as SelectedIndex,SelectedItem and SelectionMode values changed.
        /// </summary>
        /// <param name="handle">
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SelectionPropertyChangedHandlerArgs"/> that contains the corresponding property name and its value changes.
        /// </param>      
        void HandleSelectionPropertyChanges(SelectionPropertyChangedHandlerArgs handle);

        /// <summary>
        /// Handles the selection when the collection is changed.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Collections.Specialized.NotifyCollectionChangedEventArgs"/> 
        /// </param>
        /// <param name="reason">
        /// Contains the <see cref="Syncfusion.UI.Xaml.Grid.CollectionChangedReason"/> for collection changes.
        /// </param>       
        void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, CollectionChangedReason reason);

        /// <summary>
        /// Handles the when the group is expanded or collapsed in SfDataGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the group.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Specifies whether the group is expanded or not.
        /// </param>       
        void HandleGroupExpandCollapse(int index, int count, bool isExpanded);

        /// <summary>
        /// Selects the rows corresponding to the specified start and end index of the row.
        /// </summary>
        /// <param name="startRowIndex">
        /// The start index of the row.
        /// </param>
        /// <param name="endRowIndex">
        /// The end index of the row.
        /// </param>
        void SelectRows(int startRowIndex, int endRowIndex);

        /// <summary>
        /// Selects all the rows or cells in SfDataGrid.
        /// </summary>    
        void SelectAll();

        /// <summary>
        /// Clears all the selected cells or rows in SfDataGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current row or cell selection should be removed while clearing the selections from SfDataGrid.
        /// </param>       
        void ClearSelections(bool exceptCurrentRow);

        /// <summary>
        /// Moves the current cell for the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current row selection need to be cleared while moving the current cell.
        /// </param>               
        void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection=true);

        /// <summary>
        /// Handles the selection when the keyboard interactions that are performed in DetailsViewDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key was processed; otherwise, <b>false</b>.
        /// </returns>       
        bool HandleDetailsViewGridKeyDown(KeyEventArgs args);

        /// <summary>
        /// Handles the selection when the keyboard interactions that are performed in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Contains information about the key that was pressed.
        /// </param>
        /// <returns>
        /// <b>true</b> if the key was processed; otherwise, <b>false</b>.
        /// </returns>
        bool HandleKeyDown(KeyEventArgs args);
    }

    /// <summary>
    /// Provides data for handling selection property value changes.
    /// </summary>
    public class SelectionPropertyChangedHandlerArgs
    {
        /// <summary>
        /// Gets or sets the value of property after the reported change.
        /// </summary>
        /// <value>
        /// An object that contains the new value of property.
        /// </value>
        public object NewValue { get; set; }

        /// <summary>
        /// Gets or sets the value of property before the reported change.
        /// </summary>
        /// <value>
        /// An object that contains the old value of property changes.
        /// </value>
        public object OldValue { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the property where the value change occurred. 
        /// </summary>
        public string PropertyName { get; set; }
    }

    /// <summary>
    /// Provides data for handling pointer operation changes.
    /// </summary>
    public class GridPointerEventArgs
    {
        public GridPointerEventArgs(PointerOperation operation, object eventArgs)
        {
            Operation = operation;
            OriginalEventArgs = eventArgs;
        }

        /// <summary>
        /// Returns the type <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> occurred .
        /// </summary>
        public PointerOperation Operation { get; private set; }

        /// <summary>
        /// Returns the event argument for pointer operation changes.
        /// </summary>
        public object OriginalEventArgs { get; private set; }
    }
    
    /// <summary>
    /// Provides data for handling grid operations such as Sorting,Filtering, Grouping, Summaries and etc.
    /// </summary>
    public class GridOperationsHandlerArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridOperationsHandlerArgs"/> class.
        /// </summary>
        /// <param name="operation">
        /// The type of grid operation to handle.
        /// </param>
        /// <param name="operationArgs">
        /// The data for the grid operation.
        /// </param>
        public GridOperationsHandlerArgs(GridOperation operation, object operationArgs)
        {
            Operation = operation;
            OperationArgs = operationArgs;
        }
        /// <summary>
        /// Returns the type <see cref="Syncfusion.UI.Xaml.Grid.GridOperation"/> is to be handled .
        /// </summary>
        public GridOperation Operation { get; private set; }

        /// <summary>
        /// Returns the event argument for pointer operation changes.
        /// </summary>
        public object OperationArgs { get; private set; }
    }


    /// <summary>
    /// Provides data for processing the TableSummaryRow position changes.
    /// </summary>
    public class TableSummaryPositionChangedEventArgs
    {
        internal TableSummaryPositionChangedEventArgs(TableSummaryRowPosition newPosition, NotifyCollectionChangedAction action, int count)
        {
            NewPosition = newPosition;
            Action = action;
            Count = count;
        }

        /// <summary>
        /// Returns the new position of TableSummaryRow.
        /// </summary>
        public TableSummaryRowPosition NewPosition { get; private set; }

        /// <summary>
        /// Returns the corresponding collection changed actions performed on the TableSummaryRow.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        /// Gets the number of TableSummaryRow caused by the action.
        /// </summary>
        public int Count { get; private set; }
    }

    /// <summary>
    /// Provides data for the Filtering Operations that are performed in SfDataGrid.
    /// </summary>
    public class GridFilteringEventArgs
    {
        /// <summary>
        /// Initializes new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridFilteringEventArgs"/> class.
        /// </summary>     
        /// <param name='isOpen'>
        /// Indicates the state of the filter Popup.
        /// </param>
        public GridFilteringEventArgs(bool isOpen)
        {
            PopupIsOpen = isOpen;
        }

        /// <summary>
        /// Gets a value indicates whether the Popup for the FilterControl is currently open.
        /// </summary>
        public bool PopupIsOpen { get; private set; }

        /// <summary>
        /// Get or set a value indicates whether the Filtering operation is performed from code behind.
        /// </summary>
        public bool IsProgrammatic { get; set; }
    }

    /// <summary>
    /// Provides data for the Grouping Operations that are performed in SfDataGrid.
    /// </summary>
    public class GridGroupingEventArgs 
    {
       /// <summary>
       /// Gets a value indicates NotifyCollectionChangedEventArgs.
       /// </summary>
       public NotifyCollectionChangedEventArgs CollectionChangedEventArgs { get; set; }

       /// <summary>
       /// Initializes new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridGroupingEventArgs"/> class.
       /// <Summary>
       /// <param name='args'>
       /// Indicates the NotifyCollectionChangedEventArgs.
       /// </param>
       public GridGroupingEventArgs(NotifyCollectionChangedEventArgs args)            
       {
           CollectionChangedEventArgs = args;
       }

       /// <summary>
       /// Gets or set a value indicates whether the Grouping operation is performed from code behind.
       /// </summary>
       public bool IsProgrammatic { get; set; }
    }

    /// <summary>
    /// Provides data for the UnBoundDataRow collection changes.
    /// </summary>
    public class UnBoundDataRowCollectionChangedEventArgs
    {
        internal UnBoundDataRowCollectionChangedEventArgs(UnBoundRowsPosition position, NotifyCollectionChangedAction action, int count,int index)
        {
            Position = position;
            Action = action;
            Count = count;
            RowIndex = index;
        }


        /// <summary>
        /// Returns the new position of UnBoundDataRow.
        /// </summary>
        public UnBoundRowsPosition Position { get; internal set; }

        /// <summary>
        /// Gets a value that indicates whether the UnBoundDataRow placed either above or below of the summary row.        
        /// </summary>
        public bool ShowBelowSummary { get; internal set; }

        /// <summary>
        /// Returns the corresponding collection changed actions performed on the UnBoundDataRow.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        /// Gets the number of TableSummaryRow caused by the action.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Gets or sets the index of the GridUnBoundRow.
        /// </summary> 
        /// <value>
        /// The index of the GridUnBoundRow.
        /// </value>
        public int RowIndex { get; set; }
    }


    /// <summary>
    /// Provides data for the stacked header collection changes in SfDataGrid.
    /// </summary>
    public class StackedHeaderCollectionChangedEventArgs
    {
        internal StackedHeaderCollectionChangedEventArgs(NotifyCollectionChangedAction action, int count)
        {            
            Action = action;
            Count = count;         
        }
        /// <summary>
        /// Returns the corresponding collection changed actions performed on the stacked header.
        /// </summary>
        public NotifyCollectionChangedAction Action { get; private set; }

        /// <summary>
        /// Gets the number of StackedHeaderRow caused by the action.
        /// </summary>
        public int Count { get; private set; }
    }
}
