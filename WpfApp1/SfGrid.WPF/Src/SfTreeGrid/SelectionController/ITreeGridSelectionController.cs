#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;

#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using MouseButtonEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
using DoubleTappedEventArgs = Windows.UI.Xaml.Input.DoubleTappedRoutedEventArgs;
using TappedEventArgs = Windows.UI.Xaml.Input.TappedRoutedEventArgs;
using MouseEventArgs = Windows.UI.Xaml.Input.PointerRoutedEventArgs;
#else
using System.Windows.Media;
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{   
    /// <summary>
    /// Provides the common functionality of selection behavior in SfTreeGrid.     
    /// </summary>        
    public interface ITreeGridSelectionController : IDisposable
    {
        /// <summary>
        /// Returns the collection of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowInfo"/> that contains the information of selected rows.
        /// </summary>
        TreeGridSelectedRowsCollection SelectedRows { get; }   

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellManager"/> instance which holds the current cell information.
        /// </summary>
        TreeGridCurrentCellManager CurrentCellManager { get; }

        /// <summary>
        /// Handles the selection when any of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperation"/> such as Sorting are performed in SfTreeGrid.
        /// </summary>
        /// <param name="args">
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperationsHandlerArgs"/> that contains the type of grid operations and its arguments.
        /// </param>       
        void HandleGridOperations(TreeGridOperationsHandlerArgs args);

        /// <summary>
        /// Handles the selection when any of the <see cref="Syncfusion.UI.Xaml.Grid.PointerOperation"/> such as pressed,released,moved,and etc that are performed in SfTreeGrid.
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
        void HandleCollectionChanged(NotifyCollectionChangedEventArgs e, TreeGridCollectionChangedReason reason);

        /// <summary>
        /// Handles the when the node is expanded or collapsed in SfTreeGrid.
        /// </summary>
        /// <param name="index">
        /// The corresponding index of the node.
        /// </param>
        /// <param name="count">
        /// The number of rows that are collapsed or expanded.
        /// </param>
        /// <param name="isExpanded">
        /// Specifies whether the node is expanded or not.
        /// </param>       
        void HandleNodeExpandCollapse(int index, int count, bool isExpanded);

      
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
        /// Selects all the rows in SfTreeGrid.
        /// </summary>    
        void SelectAll();

        /// <summary>
        /// Clears all the selected rows in SfTreeGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Decides whether the current row or cell selection should be removed while clearing the selections from SfTreeGrid.
        /// </param>       
        void ClearSelections(bool exceptCurrentRow);

        /// <summary>
        /// Moves the current cell to the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Decides whether the current row selection need to be cleared while moving the current cell.
        /// </param>               
        void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true);


        /// <summary>
        /// Handles the selection when the keyboard interactions that are performed in SfTreeGrid.
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
    /// Provides data for handling grid operations such as Sorting
    /// </summary>
    public class TreeGridOperationsHandlerArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperationsHandlerArgs"/> class.
        /// </summary>
        /// <param name="operation">
        /// The type of grid operation to handle.
        /// </param>
        /// <param name="operationArgs">
        /// The data for the grid operation.
        /// </param>
        public TreeGridOperationsHandlerArgs(TreeGridOperation operation, object operationArgs)
        {
            Operation = operation;
            OperationArgs = operationArgs;
        }
        /// <summary>
        /// Returns the type <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridOperation"/>  to be handled .
        /// </summary>
        public TreeGridOperation Operation { get; private set; }

        /// <summary>
        /// Returns the event argument for pointer operation changes.
        /// </summary>
        public object OperationArgs { get; private set; }
    }
}
