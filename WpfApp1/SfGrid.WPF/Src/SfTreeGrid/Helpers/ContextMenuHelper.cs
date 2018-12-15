#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.ComponentModel;
#if WPF
using System.Windows.Controls;
#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using ContextMenu = Windows.UI.Xaml.Controls.MenuFlyout;
#endif

    /// <summary>
    /// Represents a class that provides information to the ContextMenu of TreeGrid. 
    /// </summary>
    public class TreeGridContextMenuInfo : INotifyPropertyChanged
    {
        private SfTreeGrid _TreeGrid;

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> control.
        /// </summary>
        public SfTreeGrid TreeGrid
        {
            get { return _TreeGrid; }
            internal set
            {
                _TreeGrid = value;
                this.OnPropertyChanged("TreeGrid");
            }
        }

        /// <summary>
        /// Occurs when the property value changes in <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridContextMenuInfo"/> class.
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
    /// Represents a class that provides information to the Header ContextMenu of TreeGrid. 
    /// </summary>
    public class TreeGridColumnContextMenuInfo : TreeGridContextMenuInfo
    {
        private TreeGridColumn _Column;

        /// <summary>
        /// Gets the corresponding column where the context menu is opened.
        /// </summary>
        public TreeGridColumn Column
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
    /// Represents a class that provides information to the Record ContextMenu of TreeGrid. 
    /// </summary>
    public class TreeGridNodeContextMenuInfo : TreeGridContextMenuInfo
    {
        private TreeNode _TreeNode;

        /// <summary>
        /// Gets the corresponding node where the context menu is opened.
        /// </summary>
        public TreeNode TreeNode
        {
            get { return _TreeNode; }
            internal set
            {
                _TreeNode = value;
                this.OnPropertyChanged("TreeNode");
            }
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.TreeGridContextMenuOpening"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridContextMenuEventArgs"/> that contains event data.
    /// </param>
    public delegate void TreeGridContextMenuOpeningEventHandler(object sender, TreeGridContextMenuEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.TreeGridContextMenuOpening"/> event.
    /// </summary>
    public class TreeGridContextMenuEventArgs : GridHandledEventArgs
    {
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridContextMenuEventArgs"/> class.
        /// </summary>
        /// <param name="_contextMenu">
        /// Contains the context menu for the SfTreeGrid.
        /// </param>
        /// <param name="_contextMenuInfo">
        /// Contains the information about the context menu.
        /// </param>
        /// <param name="_rowColumnIndex">
        /// The corresponding index where the shortcut menu is opened.
        /// </param>
        /// <param name="_contextMenuType">
        /// The corresponding type of the context menu.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public TreeGridContextMenuEventArgs(ContextMenu _contextMenu, object _contextMenuInfo, RowColumnIndex _rowColumnIndex, ContextMenuType _contextMenuType, object originalSource = null)
            : base(originalSource)
        {
            ContextMenu = _contextMenu;
            ContextMenuInfo = _contextMenuInfo;
            RowColumnIndex = _rowColumnIndex;
            ContextMenuType = _contextMenuType;
        }

        private ContextMenu contextMenu;

        /// <summary>
        /// Gets or sets the shortcut menu associated with cell.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.ContextMenu"/> that represents the shortcut menu associated with cell.
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
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.ContextMenuType"/> enumeration that specifies the type of context menu.
        /// </value>
        public ContextMenuType ContextMenuType
        {
            get { return contextMenuType; }
            set { contextMenuType = value; }
        }
    }
}