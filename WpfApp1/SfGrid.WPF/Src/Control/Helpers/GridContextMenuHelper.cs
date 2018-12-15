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
using System.Linq;
using System.Text;
using System.Windows;
#if WPF
using System.Windows.Controls;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
#endif
namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
	//Due to unavailability of Context menu Control, Menu flyout used Instead of it
    using ContextMenu = Windows.UI.Xaml.Controls.MenuFlyout;
#endif
    /// <summary>
    /// Represents a class that provides information to the ContextMenu of DataGrid. 
    /// </summary>
    public class GridContextMenuInfo : INotifyPropertyChanged
    {
        SfDataGrid _DataGrid;
        SfDataGrid _SourceDataGrid;

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control.
        /// </summary>          
        public SfDataGrid DataGrid
        {
            get { return _DataGrid; }
            internal set
            {
                _DataGrid = value;
                this.OnPropertyChanged("DataGrid");
            }
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> control that is used as SourceDataGrid in DetailsView.
        /// </summary>  
        public SfDataGrid SourceDataGrid
        {
            get { return _SourceDataGrid; }
            internal set
            {
                _SourceDataGrid = value;
                this.OnPropertyChanged("SourceDataGrid");
            }
        }

        /// <summary>
        /// Occurs when the property value changes in <see cref="Syncfusion.UI.Xaml.Grid.GridContextMenuInfo"/> class.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        /// <param name="name">The corresponding property name.</param>
        protected void OnPropertyChanged(string name)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }


    /// <summary>
    /// Represents a class that provides information to the Header ContextMenu of DataGrid. 
    /// </summary>
    public class GridColumnContextMenuInfo : GridContextMenuInfo
    {
        GridColumn _Column;

        /// <summary>
        /// Gets the corresponding column where the context menu is opened.
        /// </summary>
        public GridColumn Column
        {
            get { return _Column; }
            internal set
            {
                _Column = value;
                this.OnPropertyChanged("Column");
            }
        }

    }

    /// <summary>
    /// Represents a class that provides information to the Record ContextMenu of DataGrid. 
    /// </summary>
    public class GridRecordContextMenuInfo : GridContextMenuInfo
    {
        object _Record;

        /// <summary>
        /// Gets the corresponding record where the context menu is opened.
        /// </summary>
        public object Record
        {
            get { return _Record; }
            internal set
            {
                _Record = value;
                this.OnPropertyChanged("Record");
            }
        }
    }

    /// <summary>
    /// Represents a class that provides information to GroupDropArea ContextMenu of DataGrid.
    /// </summary>
    public class GridGroupDropAreaContextMenuInfo : GridContextMenuInfo
    {

    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridContextMenuOpening"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// <see cref="Syncfusion.UI.Xaml.Grid.GridContextMenuEventArgs"/> that contains event data.
    /// </param>
    public delegate void GridContextMenuOpeningEventHandler(object sender, GridContextMenuEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridContextMenuOpening"/> event.
    /// </summary>
    public class GridContextMenuEventArgs : GridHandledEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridContextMenuEventArgs"/> class.
        /// </summary>
        /// <param name="_contextMenu">
        /// Contains the context menu for the SfDataGrid.
        /// </param>
        /// <param name="_contextMenuInfo">
        /// Contains the information about the content menu. 
        /// </param>
        /// <param name="_rowColumnIndex">
        /// The corresponding rowcolumnindex where the shortcut menu is opened.
        /// </param>
        /// <param name="_contextMenuType">
        /// The corresponding type of the context menu.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public GridContextMenuEventArgs(ContextMenu _contextMenu, object _contextMenuInfo, RowColumnIndex _rowColumnIndex, ContextMenuType _contextMenuType, object originalSource = null)
            : base(originalSource)
        {
            ContextMenu = _contextMenu;
            ContextMenuInfo = _contextMenuInfo;
            RowColumnIndex = _rowColumnIndex;
            ContextMenuType = _contextMenuType;
        }

        private ContextMenu contextMenu;
        /// <summary>
        /// Gets or sets the shortcut menu associated with SfDataGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.ContextMenu"/> that represents the shortcut menu associated with SfDataGrid.
        /// </value>
        public ContextMenu ContextMenu
        {
            get { return contextMenu; }
            set { contextMenu = value; }
        }

      
        private object contextMenuInfo;
        /// <summary>
        /// Gets or sets an object that contains an information about the context menu.
        /// </summary>
        /// <value>
        /// An object that contains an information about the context menu.  
        /// </value>
        public object ContextMenuInfo
        {
            get { return contextMenuInfo; }
            set { contextMenuInfo = value; }
        }

       
        private RowColumnIndex rowColumnIndex;
        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> where the context menu is opened.
        /// </summary>
        /// <value>
        /// The corresponding <see cref="Syncfusion.UI.Xaml.ScrollAxis.RowColumnIndex"/> of the context menu.
        /// </value>
        public RowColumnIndex RowColumnIndex
        {
            get
            {
                if (rowColumnIndex == null)
                    return RowColumnIndex.Empty;

                return rowColumnIndex;
            }
            set
            {
                rowColumnIndex = value;
            }
        }
     
        private ContextMenuType contextMenuType;
        /// <summary>
        /// Gets or sets the type of context menu.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.ContextMenuType"/> enumeration that specifies the type of context menu.
        /// </value>
        public ContextMenuType ContextMenuType
        {
            get { return contextMenuType; }
            set { contextMenuType = value; }
        }

    }


}
