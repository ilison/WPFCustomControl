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
using System.ComponentModel;
using System.Windows.Input;
#if !WPF
using Windows.Devices.Input;
using Windows.UI.Xaml.Controls;
#else
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    #region CellTappedEventHandler

    /// <summary>
    /// Provides data for the CellTapped event.
    /// </summary>
    public class CellTappedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CellTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CellTappedEventArgs(object originalSource)
            : base(originalSource)
        {

        }


        /// <summary>
        /// Gets the GridColumnBase of tapped cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase"/> of tapped cell.
        /// </value>
        public GridColumnBase Column
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the data context of tapped cell.
        /// </summary>
        /// <value>
        /// The data context of tapped cell.
        /// </value>
        public object Record
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the RowColumnIndex of tapped cell.
        /// </summary>
        /// <value>
        /// The RowColumnIndex of tapped cell.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }

#if WPF
        /// <summary>
        /// Gets the button associated with the event.
        /// </summary>
        /// <value>
        /// The button which was pressed.
        /// </value>
        public MouseButton ChangedButton
        {
            get;
            internal set;
        }
#else
        /// <summary>
        /// Gets the device type that associated with event.
        /// </summary>
        /// <value>
        /// The device which was used.
        /// </value>
        public PointerDeviceType PointerDeviceType
        {
            get;
            internal set;
        }

#endif
    }

    #endregion

    #region CellDoubleTappedEventHandler

    /// <summary>
    /// Provides data for the CellDoubleTapped event.
    /// </summary>
    public class CellDoubleTappedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CellDoubleTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CellDoubleTappedEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Gets the GridColumnBase of double tapped cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase"/> of double tapped cell.
        /// </value>
        public GridColumnBase Column
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the data context of double tapped cell.
        /// </summary>
        /// <value>
        /// The data context of double tapped cell.
        /// </value>
        public object Record
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the RowColumnIndex of double tapped cell.
        /// </summary>
        /// <value>
        /// The RowColumnIndex of double tapped cell.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }

#if WPF
        /// <summary>
        /// Gets the button associated with the event.
        /// </summary>
        /// <value>
        /// The button which was pressed.
        /// </value>
        public MouseButton ChangedButton
        {
            get;
            internal set;
        }
#else
        /// <summary>
        /// Gets the device type that associated with event.
        /// </summary>
        /// <value>
        /// The device which was used.
        /// </value>
        public PointerDeviceType PointerDeviceType
        {
            get;
            internal set;
        }
#endif
    }

    #endregion

    #region CellToolTipOpeningEventHandler

    /// <summary>
    /// Provides data for the CellToolTipOpening event.
    /// </summary>
    public class CellToolTipOpeningEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CellToolTipOpeningEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CellToolTipOpeningEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnBase"/> of hovered cell.
        /// </summary>
        public GridColumnBase Column
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the data context of hovered cell.
        /// </summary>
        public object Record
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of hovered cell.
        /// </summary>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the <see cref="ToolTip"/> of hovered cell.
        /// </summary>
        /// <value>
        /// The ToolTip of hovered cell.
        /// </value>
        public ToolTip ToolTip
        {
            get;
            internal set;
        }
    }

    #endregion
    #region Event Arguments and Handlers

    #region CurrentCellActivatingEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivating"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellActivatingEventHandler(object sender, CurrentCellActivatingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivating"/> event.
    /// </summary>
    public class CurrentCellActivatingEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellActivatingEventArgs(object originalSource)
            : base(originalSource)
        {

        }

        /// <summary>
        /// Returns the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of currently active cell.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the currently active cell.
        /// </value>
        public RowColumnIndex CurrentRowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of previously active cell.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of previously active cell.
        /// </value>
        public RowColumnIndex PreviousRowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.ActivationTrigger"/> that indicates whether the current cell is activated either by device or programmatic.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.ActivationTrigger"/> that specifies how the current cell is activated.
        /// </value>
        public ActivationTrigger ActivationTrigger
        {
            get;
            internal set;
        }
    }
    #endregion

    #region CurrentCellActivatedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivated"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellActivatedEventHandler(object sender, CurrentCellActivatedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivated"/> event.
    /// </summary>
    public class CurrentCellActivatedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellActivatedEventArgs(object originalSource)
            : base(originalSource)
        {            
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of currently active cell.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the currently active cell.
        /// </value>
        public RowColumnIndex CurrentRowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of previously active cell.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of previously active cell.
        /// </value>
        public RowColumnIndex PreviousRowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.Grid.ActivationTrigger"/> that indicates how the current cell is activated.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.ActivationTrigger"/> that specifies how the current cell is activated.
        /// </value>
        public ActivationTrigger ActivationTrigger
        {
            get;
            internal set;
        }
    }
    #endregion

    #region CurrentCellBeginEditEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBeginEdit"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellBeginEditEventHandler(object sender, CurrentCellBeginEditEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBeginEdit"/> event.
    /// </summary>
    public class CurrentCellBeginEditEventArgs : GridCancelEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellBeginEditEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the column that contains the current cell for which the event occurred.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> that contains the cell to be edited.
        /// </value>
        public GridColumn Column
        {
            get;
            internal set;
        }
    }
    #endregion

    #region CurrentCellEndEditEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellEndEdit"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellEndEditEventHandler(object sender, CurrentCellEndEditEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellEndEdit"/> event.
    /// </summary>
    public class CurrentCellEndEditEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellEndEditEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellEndEditEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the current cell for which the event occurred.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }
    }

    #endregion

    #region CurrentCellValidatingEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellValidatingEventHandler(object sender, CurrentCellValidatingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event.
    /// </summary>
    public class CurrentCellValidatingEventArgs : CurrentCellValidatingEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellValidatingEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the GridColumn of the cell triggers this event.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridColumn"/> of the cell which triggered this event. 
        /// </value>
        /// <remarks>
        /// GridTemplateColumn and GridUnboundColumn cells will not triggers this event.
        /// </remarks>
        public GridColumn Column
        {
            get;
            internal set;
        }        
    }

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event.
    /// </summary>
    public class CurrentCellValidatingEventArgsBase : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatingEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellValidatingEventArgsBase(object originalSource)
            : base(originalSource)
        {

        }
        

        /// <summary>
        /// Gets the old value of the cell triggers this event.
        /// </summary>
        /// <value>
        /// The old value of the cell.
        /// </value>
        public object OldValue
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the new value of the cell triggers this event.
        /// </summary>
        /// <value>
        /// The new value of the cell.
        /// </value>
        public object NewValue
        {
            get;
            set;
        }

        /// <summary>        
        /// Gets or sets the error message that is used to display the error information in-case of invalid data.
        /// </summary>
        /// <value>
        /// A string that is used to notify the type of validation error.
        /// </value>
        public string ErrorMessage
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value that indicates the validation status whether to allow newly entered value for committing.
        /// </summary>
        /// <value>
        /// <b>true</b> if the newly entered value is valid and it commits the new value; otherwise, <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidated"/> event not raised when the IsValid property set to false.
        /// </remarks>
        public bool IsValid
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the data object associated with the row which has cell triggered this event.
        /// </summary>
        /// <value>
        /// The data object of corresponding data row which has the cell validation occurred.
        /// </value>
        public object RowData
        {
            get;
            internal set;
        }
    }


    #endregion

    #region CurrentCellValidatedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidated"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidatedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellValidatedEventHandler(object sender, CurrentCellValidatedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidated"/> event.
    /// </summary>
    public class CurrentCellValidatedEventArgs : CurrentCellValidatedEventArgsBase
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellValidatedEventArgs(object originalSource)
            : base(originalSource)
        {
            
        }

        /// <summary>
        /// Gets the GridColumn of the cell which triggers this event.
        /// </summary>
        /// <value>
        /// The corresponding column that contains the current cell.
        /// </value>
        public GridColumn Column
        {
            get;
            internal set;
        }
    }

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidated"/> event.
    /// </summary>
    public class CurrentCellValidatedEventArgsBase : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellValidatedEventArgsBase(object originalSource)
            : base(originalSource)
        {

        }
      

        /// <summary>
        ///  Gets the old value of the cell which triggered this event.
        /// </summary>
        /// <value>
        /// The old value of the cell.
        /// </value>
        public object OldValue
        {
            get;
            internal set;
        }

        /// <summary>
        ///Gets or sets the new value of the cell which triggered this event.
        /// </summary>
        /// <value>
        /// The new value of the cell.
        /// </value>
        public object NewValue
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets or sets the error message that is used to display the error information in-case of invalid data.
        /// </summary>
        /// <value>
        /// A string that is used to notify the error in data.
        /// </value>
        public string ErrorMessage
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the data item of corresponding row where the cell validation occurs.
        /// </summary>
        /// <value>
        /// The data item of corresponding data row where the cell validation occurs.
        /// </value>
        public object RowData
        {
            get;
            internal set;
        }
    }
    #region CurrentCellSelectionChangedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellDropDownSelectionChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellDropDownSelectionChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellDropDownSelectionChangedEventHandler(object sender, CurrentCellDropDownSelectionChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellDropDownSelectionChanged"/> event.
    /// </summary>
    public class CurrentCellDropDownSelectionChangedEventArgs : GridEventArgs
    {

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellDropDownSelectionChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellDropDownSelectionChangedEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        /// <summary>
        /// Gets the data item that were selected from the drop-down control.
        /// </summary>
        /// <value>
        /// The data item that were selected from drop-down control.
        /// </value>
        public object SelectedItem { get; internal set; }

        /// <summary>
        /// Gets the index of the corresponding item that were selected from the drop-down control.
        /// </summary>
        /// <value>
        /// An index of corresponding selected item.
        /// </value>
        public int SelectedIndex { get; internal set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the corresponding item that were selected from the drop-down control.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of corresponding selected item.
        /// </value>
        public RowColumnIndex RowColumnIndex { get; internal set; }
    }


    #endregion


    #region CurrentCellValueChangedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValueChanged"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValueChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellValueChangedEventHandler(object sender, CurrentCellValueChangedEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValueChanged"/> event.
    /// </summary>
    public class CurrentCellValueChangedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValueChangedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellValueChangedEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        /// <summary>
        /// Gets a value indicating the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the cell that the event occurs for.
        /// </summary>
        public RowColumnIndex RowColumnIndex
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the record of the corresponding cell value changes occurs for.
        /// </summary>
        /// <value>
        /// The record that contains cell value changes occurs for.
        /// </value>
        public object Record
        {
            get;
            internal set;
        }
        /// <summary>
        /// Gets the column of the corresponding cell value changes occurs for.
        /// </summary>
        /// <value>
        /// The column that contains the cell value changes occurs for.
        /// </value>
        public GridColumn Column
        {
            get;
            internal set;
        }
    }

    #endregion

    #region CurrentCellRequestNavigateEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellRequestNavigate"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellRequestNavigateEventArgs"/> that contains the event data.
    /// </param>
    public delegate void CurrentCellRequestNavigateEventHandler(object sender, CurrentCellRequestNavigateEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellRequestNavigate"/> event.
    /// </summary>
    public class CurrentCellRequestNavigateEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellRequestNavigateEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public CurrentCellRequestNavigateEventArgs(object originalSource)
            : base(originalSource)
        {

        }
        /// <summary>
        /// Gets the navigation text when the hyperlink is activated.
        /// </summary>
        /// <value>
        /// A string that contains the text to navigate when the hyperlink is activated.
        /// </value>
        public string NavigateText { get; internal set; }

        /// <summary>
        /// Gets the data item of the corresponding cell navigation occurs for.
        /// </summary>
        /// <value>
        /// The data item of the corresponding cell navigation occurs for.
        /// </value>
        public object RowData { get; internal set; }

        /// <summary>
        /// Gets or sets the value that indicates whether the navigation should be handled for the particular cell.
        /// </summary>
        /// <value>
        /// <b>true</b> if the navigation is handled; otherwise, <b>false</b>.
        /// </value>
        public bool Handled { get; set; }

        /// <summary>
        /// Gets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the corresponding cell navigation occurs for.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/>of corresponding cell navigation occurs for.
        /// </value>
        public RowColumnIndex RowColumnIndex { get; internal set; }
    }

    #endregion

    #region RowValidatingEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidating"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void RowValidatingEventHandler(object sender, RowValidatingEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidating"/> event.
    /// </summary>
    public class RowValidatingEventArgs : GridEventArgs
    {    

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs"/> class.
        /// </summary>
        /// <param name="_rowData">
        /// The data object associated with the row which triggered this event.
        /// </param>       
        /// <param name="_rowIndex">
        /// The index of the row . 
        /// </param>
        /// <param name="errorMessages">
        /// Contains the error message to notify the validation error.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public RowValidatingEventArgs(object _rowData, int _rowIndex, Dictionary<string, string> errorMessages, object originalSource):base(originalSource)
        {
            RowData = _rowData;
            RowIndex = _rowIndex;
            ErrorMessages = errorMessages;
        }

        bool isValid = true;
        /// <summary>
        /// Gets or sets a value that indicates the  validation status whether to allow the newly entered value  for commuting.
        /// </summary>
        /// <value>
        /// <b>true</b> if the newly entered value is valid and it commits the new value; otherwise , <b>false</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidated"/> event is not raised when the IsValid property set to false.
        /// </remarks>
        public bool IsValid
        {
            get { return isValid; }
            set { isValid = value; }
        }

        object rowData;
        /// <summary>
        /// Gets the data object associated with the row triggered this event.        
        /// </summary>
        /// <value>
        /// The data item of corresponding row associated with the event.
        /// </value>
        public object RowData
        {
            get { return rowData; }
            internal set { rowData = value; }
        }

        int rowIndex;
        /// <summary>
        /// Gets or sets the RowIndex of DataRow which triggers this event.
        /// </summary>
        /// <value>
        /// The index of the row associated with the event.
        /// </value>
        public int RowIndex
        {
            get { return rowIndex; }
            internal set { rowIndex = value; }
        }
       
        /// <summary>
        /// Gets or sets the error message that is used to display the error information in-case of invalid data when mouse over on GridCell of particular column.        
        /// </summary>
        /// <value>
        /// The dictionary that holds the column name as its key and the error message as its value to notify the error.
        /// </value>
        public Dictionary<string, string> ErrorMessages
        {
            get;
            internal set;
        }
    }

    #endregion


    #region RowValidatedEventHandler
    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidated"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the SfDataGrid.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs"/> that contains the event data.</param>
    public delegate void RowValidatedEventHandler(object sender, RowValidatedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidated"/> event.
    /// </summary>
    public class RowValidatedEventArgs : GridEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs"/> class.
        /// </summary>
        /// <param name="_rowData">
        /// Gets the data object that associated with the row which triggers this event.
        /// </param>
        /// <param name="_rowIndex">
        /// The index of the row .
        /// </param>
        /// <param name="errorMessages">
        /// Contains the error message to notify the validation error.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public RowValidatedEventArgs(object _rowData, int _rowIndex, Dictionary<string, string> errorMessages, object originalSource):base(originalSource)
        {
            RowData = _rowData;
            RowIndex = _rowIndex;
            ErrorMessages = errorMessages;
        }

        
        object rowData;
        /// <summary>
        /// Gets the data object associated with row triggered this event.
        /// </summary>
        /// <value>
        /// The data item of corresponding row associated with the event.
        /// </value>
        public object RowData
        {
            get { return rowData; }
            internal set { rowData = value; }
        }

        int rowIndex;
        /// <summary>
        ///Gets or sets the RowIndex of DataRow which triggers this event.
        /// </summary>
        /// <value>
        /// The index of the row which triggered this event.
        /// </value>
        public int RowIndex
        {
            get { return rowIndex; }
            internal set { rowIndex = value; }
        }

        /// <summary>
        /// Gets or sets the error message that is used to display the error information in-case of invalid data when mouse over on GridCell of particular column.        
        /// </summary>
        /// <value>
        /// The dictionary that holds the column name as its key and the error message as its value to notify the error in data.
        /// </value>
        public Dictionary<string, string> ErrorMessages
        {
            get;
            internal set;
        }
    }

    #endregion

    #endregion

    #region CellTappedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellTapped"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the GridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.Grid.GridCellTappedEventArgs"/> that contains the event data.</param>
    public delegate void GridCellTappedEventHandler(object sender, GridCellTappedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellTapped"/> event.
    /// </summary>
    
    public class GridCellTappedEventArgs : CellTappedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridCellTappedEventArgs(object originalSource):base(originalSource)
        {

        }
    }

    #endregion


    #region CellDoubleTappedEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellDoubleTapped"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the GridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs"/> that contains the event data.</param>
    public delegate void GridCellDoubleTappedEventHandler(object sender, GridCellDoubleTappedEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellDoubleTapped"/> event.
    /// </summary>
    public class GridCellDoubleTappedEventArgs : CellDoubleTappedEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridCellDoubleTappedEventArgs(object originalSource)
            : base(originalSource)
        {

        }       
    }

    #endregion

    #region CellToolTipOpeningEventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellToolTipOpening"/> event.  
    /// </summary>
    /// <param name="sender">The sender the contains the GridCell.</param>
    /// <param name="e">The <see cref="Syncfusion.UI.Xaml.Grid.GridCellToolTipOpeningEventArgs"/> that contains the event data.</param>
    public delegate void GridCellToolTipOpeningEventHandler(object sender, GridCellToolTipOpeningEventArgs e);

    /// <summary>
    /// Provides data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellToolTipOpening"/> event.
    /// </summary>
    public class GridCellToolTipOpeningEventArgs : CellToolTipOpeningEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridCellToolTipOpeningEventArgs"/> class.
        /// </summary>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridCellToolTipOpeningEventArgs(object originalSource) : base(originalSource)
        {

        }
    }

    #endregion
    #endregion

}