#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.IO;
using System.Runtime.Serialization;
using Syncfusion.Data.Extensions;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.Grid.RowFilter;
using System.ComponentModel.DataAnnotations;
using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Text;
#if WinRT || UNIVERSAL
using Syncfusion.UI.Xaml.Utility;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Syncfusion.Data;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using Windows.UI.Xaml.Markup;
using Windows.ApplicationModel.Resources;
using Syncfusion.UI.Xaml.TreeGrid;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.Data;
using System.Windows.Markup;
using System.Resources;
#endif

#if WPF
using System.Data;
﻿using Syncfusion.Windows.Shared;
using System.Windows.Automation.Peers;
using System.Windows.Documents;
#endif
using System.Dynamic;
using Syncfusion.Dynamic;
using Syncfusion.Data.Helper;

namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    using ContextMenu = Windows.UI.Xaml.Controls.MenuFlyout;
#endif
    public class SfGridBase : Control, IDisposable
    {
        #region Internal fields
        internal bool suspendForColumnPopulation;
        internal bool showRowHeader = false;
        #endregion

        #region Dependency properties
        /// <summary>
        /// Gets or sets a value that indicates whether the user can edit the cells.
        /// </summary>
        /// <value>
        /// <b>true</b> if user can edit the cells; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBeginEdit"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellEndEdit"/> events occurs when editing starts for current cell and editing ends for current cell.
        /// </remarks>
        public bool AllowEditing
        {
            get { return (bool)this.GetValue(AllowEditingProperty); }
            set { this.SetValue(AllowEditingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowEditing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowEditing dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowEditingProperty =
            GridDependencyProperty.Register("AllowEditing", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnAllowEditingChanged));


        private static void OnAllowEditingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnAllowEditingChanged(args);
        }

        internal virtual void OnAllowEditingChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data by clicking on its header cell of the column in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the sorting is enabled ; otherwise, <b>false</b>. The default value is <b>true</b>.
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanging"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanged"/> events occurs , when the sorting operation is performed.
        /// You can cancel or customize the sorting operation through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanging"/> event handler.
        /// </remarks>
        public bool AllowSorting
        {
            get { return (bool)this.GetValue(AllowSortingProperty); }
            set { this.SetValue(AllowSortingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSorting dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSorting dependency property.
        /// </remarks>    
        public static readonly DependencyProperty AllowSortingProperty =
            GridDependencyProperty.Register("AllowSorting", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the selection should be present in the PointerPressed state.
        /// </summary>
        /// <value>
        /// <b>true</b> if the row or cell is selected in PointerPressed; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>        
        public bool AllowSelectionOnPointerPressed
        {
            get { return (bool)GetValue(AllowSelectionOnPointerPressedProperty); }
            set { SetValue(AllowSelectionOnPointerPressedProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSelectionOnPointerPressed dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowSelectionOnPointerPressed dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowSelectionOnPointerPressedProperty =
            GridDependencyProperty.Register("AllowSelectionOnPointerPressed", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnAllowSelectionOnPointerPressed));

        private static void OnAllowSelectionOnPointerPressed(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnAllowSelectionOnPointerPressed(args);
        }

        internal virtual void OnAllowSelectionOnPointerPressed(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets the value that indicates how the rows or cells are selected in SfDataGrid.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionMode"/> enumeration that specifies the selection behavior in SfDataGrid . 
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionMode.Single"/> .
        /// </value>        
        public GridSelectionMode SelectionMode
        {
            get { return (GridSelectionMode)GetValue(SelectionModeProperty); }
            set { SetValue(SelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionMode dependency property.
        /// </remarks>       
        public static readonly DependencyProperty SelectionModeProperty =
            GridDependencyProperty.Register("SelectionMode", typeof(GridSelectionMode), typeof(SfGridBase), new GridPropertyMetadata(GridSelectionMode.Single, OnSelectionModeChanged));

        /// <summary>
        /// Dependency call back for SelectionMode.
        /// If selection mode changed to Single current tow selection only maintained.
        /// If selection mode changed to None current cell selection only maintained all other selection shoud be cleared.
        /// For other two modes selection will be maintained.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnSelectionModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnSelectionModeChanged(args);
        }

        internal virtual void OnSelectionModeChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.NavigationMode"/> that indicates whether the navigation can be accomplished based on either cell or row in SfDataGrid.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.NavigationMode"/> enumeration that specifies the navigation based on either row or cell in SfDataGrid .
        /// The default registered mode is <see cref="Syncfusion.UI.Xaml.Grid.NavigationMode.Cell"/>. 
        /// </value>
        public NavigationMode NavigationMode
        {
            get { return (NavigationMode)GetValue(NavigationModeProperty); }
            set { SetValue(NavigationModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty NavigationModeProperty =
            GridDependencyProperty.Register("NavigationMode", typeof(NavigationMode), typeof(SfGridBase), new GridPropertyMetadata(NavigationMode.Cell, OnNavigationModeChanged));

        private static void OnNavigationModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnNavigationModeChanged(args);
        }

        internal virtual void OnNavigationModeChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets value that indicates the position of cursor and selection in edit element when entering edit mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.EditorSelectionBehavior"/> enumeration that denotes the position of cursor and selection in edit mode. 
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.EditorSelectionBehavior.SelectAll"/>.
        /// </value>      
        public EditorSelectionBehavior EditorSelectionBehavior
        {
            get { return (EditorSelectionBehavior)GetValue(EditorSelectionBehaviorProperty); }
            set { SetValue(EditorSelectionBehaviorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.EditorSelectionBehavior dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.EditorSelectionBehavior dependency property.
        /// </remarks>         
        public static readonly DependencyProperty EditorSelectionBehaviorProperty =
            GridDependencyProperty.Register("EditorSelectionBehavior", typeof(EditorSelectionBehavior), typeof(SfGridBase), new GridPropertyMetadata(EditorSelectionBehavior.SelectAll));

        /// <summary>
        /// Gets or sets a value that indicates the mouse action that triggers editing.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger"/> enumeration that specifies the mouse action that triggers the editing.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.EditTrigger.OnDoubleTap"/>. 
        /// </value>
        public EditTrigger EditTrigger
        {
            get
            {
                return (EditTrigger)GetValue(EditTriggerProperty);
            }
            set
            {
                SetValue(EditTriggerProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.EditTrigger dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.EditTrigger dependency property.
        /// </remarks>        
        public static readonly DependencyProperty EditTriggerProperty =
           GridDependencyProperty.Register("EditTrigger", typeof(EditTrigger), typeof(SfGridBase), new GridPropertyMetadata(EditTrigger.OnDoubleTap, null));

        /// <summary>
        /// Gets or sets the width for the boundaries of the current cell border.
        /// </summary>
        /// <value>
        /// The width of the current cell border.The default value is 2.
        /// </value>           
        public Thickness CurrentCellBorderThickness
        {
            get { return (Thickness)GetValue(CurrentCellBorderThicknessProperty); }
            set { SetValue(CurrentCellBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBorderThickness dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBorderThickness dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentCellBorderThicknessProperty =
            GridDependencyProperty.Register("CurrentCellBorderThickness", typeof(Thickness), typeof(SfGridBase), new GridPropertyMetadata(new Thickness(2), OnCurrentCellBorderThicknessPropertyChanged));

        private static void OnCurrentCellBorderThicknessPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnCurrentCellBorderThicknessPropertyChanged(e);
        }

        internal virtual void OnCurrentCellBorderThicknessPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

        }
        /// <summary>
        /// Gets or sets a brush that is used to draw the border color of the current cell.
        /// </summary>
        /// <value>
        /// The border color of the current cell.The default value is black.
        /// </value>         
        public Brush CurrentCellBorderBrush
        {
            get { return (Brush)GetValue(CurrentCellBorderBrushProperty); }
            set { SetValue(CurrentCellBorderBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBorderBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellBorderBrush dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentCellBorderBrushProperty =
            GridDependencyProperty.Register("CurrentCellBorderBrush", typeof(Brush), typeof(SfGridBase), new GridPropertyMetadata(new SolidColorBrush(Colors.Black), OnCurrentCellBorderBrushPropertyChanged));

        private static void OnCurrentCellBorderBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnCurrentCellBorderBrushPropertyChanged(e);
        }

        internal virtual void OnCurrentCellBorderBrushPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

        }


        /// <summary>
        /// Gets or sets the number of non-scrolling columns at left of the grid.
        /// </summary>
        /// <value>
        /// The number of non-scrolling columns at left. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Frozen columns are always displayed and it can't be scrolled out of visibility.
        /// </remarks>
        [Cloneable(false)]
        public int FrozenColumnCount
        {
            get { return (int)GetValue(FrozenColumnCountProperty); }
            set { SetValue(FrozenColumnCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.FrozenColumnCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.FrozenColumnCount dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FrozenColumnCountProperty =
            GridDependencyProperty.Register("FrozenColumnCount", typeof(int), typeof(SfGridBase), new GridPropertyMetadata(0, OnFrozenColumnCountPropertyChanged));



        private static void OnFrozenColumnCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnFrozenColumnCountChanged(e);
        }


        internal virtual void OnFrozenColumnCountChanged(DependencyPropertyChangedEventArgs e)
        {

        }



        /// <summary>
        /// Gets or sets a value that indicates the visibility of RowHeader column which is used to denote the status of row.
        /// </summary>
        /// <value>
        /// <b>true</b> if the RowHeader is visible; otherwise, <b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// RowHeader is a special column used to indicate the status of row (current row, editing status, errors in row, etc.) which is placed as first cell of each row.
        /// </remarks>
        public bool ShowRowHeader
        {
            get { return (bool)GetValue(ShowRowHeaderProperty); }
            set { SetValue(ShowRowHeaderProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowRowHeader dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowRowHeader dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ShowRowHeaderProperty =
            GridDependencyProperty.Register("ShowRowHeader", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnShowRowHeaderChanged));

        private static void OnShowRowHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnShowRowHeaderChanged(e);
        }

        internal virtual void OnShowRowHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            this.showRowHeader = (bool)e.NewValue;
        }


        /// <summary>
        /// Gets or sets the width of the RowHeader.
        /// </summary>
        /// <value>
        /// The width of the RowHeader.
        /// </value>
        /// <remarks>
        /// RowHeader can be enabled by setting <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowRowHeader"/> as true.
        /// </remarks>
        public double RowHeaderWidth
        {
            get { return (double)GetValue(RowHeaderWidthProperty); }
            set { SetValue(RowHeaderWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowHeaderWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowHeaderWidth dependency property.
        /// </remarks>        
        public static readonly DependencyProperty RowHeaderWidthProperty =
            GridDependencyProperty.Register("RowHeaderWidth", typeof(double), typeof(SfGridBase), new GridPropertyMetadata
                (
#if !WinRT && !UNIVERSAL
24d
#else
45d
#endif
, OnRowHeaderWidthChanged));

        private static void OnRowHeaderWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnRowHeaderWidthChanged(e);
        }

        internal virtual void OnRowHeaderWidthChanged(DependencyPropertyChangedEventArgs e)
        {

        }
#if UWP
        internal static double headerRowHeight = 45d;
        internal static double rowHeight = 45d;
#else
        internal static double headerRowHeight = 24d;
        internal static double rowHeight = 24d;
#endif
        internal bool hasCellTemplateSelector;
        /// <summary>
        /// Gets or sets the height of header row.
        /// </summary>
        /// <value>
        /// The height of the header row.
        /// </value>
        /// <remarks>
        /// Header row can be hide by setting HeaderRowHeight as zero.
        /// </remarks>
        public double HeaderRowHeight
        {
            get { return (double)GetValue(HeaderRowHeightProperty); }
            set { SetValue(HeaderRowHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.HeaderRowHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.HeaderRowHeight dependency property.
        /// </remarks>         
        public static readonly DependencyProperty HeaderRowHeightProperty =
            GridDependencyProperty.Register("HeaderRowHeight", typeof(double), typeof(SfGridBase), new GridPropertyMetadata(SfGridBase.headerRowHeight, OnHeaderRowHeightChanged));

        /// <summary>
        /// Dependency call back method of Header Row Height Property.
        /// Sets the Default row height of Header Row in SfDataGrid and SfTreeGrid.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>        
        private static void OnHeaderRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnHeaderRowHeightChanged(e);
        }

        internal virtual void OnHeaderRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets the height for record rows in SfDataGrid and SfTreeGrid.
        /// </summary>
        /// <value>
        /// The height of record row in SfDataGrid and SfTreeGrid.
        /// </value>        
        /// <remarks>
        /// Particular record row height can be changed by specifying row index in `VisualContainer.RowHeights` for SfDataGrid and `TreeGridPanel.RowHeights` for SfTreeGrid. 
        /// Once row heights are changed, need to call the InvalidateMeasureInfo method of <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> for SfDataGrid and <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridPanel"/> for SfTreeGrid to refresh the view.
        /// </remarks>
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        /// using Syncfusion.UI.Xaml.Grid.Helpers;
        /// var VisualContainer = this.dataGrid.GetVisualContainer();
        /// //Set RowHeight to 2'nd row
        /// VisualContainer.RowHeights[2] = 50;
        /// VisualContainer.InvalidateMeasure();
        /// ]]></code>
        /// </example>        
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        /// using Syncfusion.UI.Xaml.TreeGrid.Helpers;
        /// var treeGridPanel = this.treeGrid.GetTreePanel();
        /// //Set RowHeight to 2'nd row
        /// treeGridPanel.RowHeights[2] = 50;
        /// treeGridPanel.InvalidateMeasure();
        /// ]]></code>
        /// </example>      
        public double RowHeight
        {
            get { return (double)GetValue(RowHeightProperty); }
            set { SetValue(RowHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.RowHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.RowHeight dependency property.
        /// </remarks>         
        public static readonly DependencyProperty RowHeightProperty =
            GridDependencyProperty.Register("RowHeight", typeof(double), typeof(SfGridBase), new GridPropertyMetadata(SfGridBase.rowHeight, OnRowHeightChanged));


        private static void OnRowHeightChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnRowHeightChanged(e);
        }

        internal virtual void OnRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {

        }


        /// <summary>
        /// Gets or sets a value that indicates whether the column can be repositioned by using mouse or touch.
        /// </summary>
        /// <value>
        /// <b>true</b> if the column is repositioned by using mouse or touch; otherwise , <b>false</b> .The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The dragging operation can be customized through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryColumnDragging"/> event handler in SfDataGrid.
        /// </remarks>
        public bool AllowDraggingColumns
        {
            get { return (bool)GetValue(AllowDraggingColumnsProperty); }
            set { SetValue(AllowDraggingColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowDraggingColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowDraggingColumns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowDraggingColumnsProperty =
            GridDependencyProperty.Register("AllowDraggingColumns", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can adjust the column width by using the mouse.
        /// </summary>
        /// <value>        
        /// <b>true</b> if the columns are allowed for resizing;otherwise,<b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// You can cancel or customize the resizing operations through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ResizingColumns"/> event handler. 
        /// </remarks>
        public bool AllowResizingColumns
        {
            get { return (bool)GetValue(AllowResizingColumnsProperty); }
            set { SetValue(AllowResizingColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowResizingColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowResizingColumns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowResizingColumnsProperty =
            GridDependencyProperty.Register("AllowResizingColumns", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnAllowResisizingColumnsChanged));

        private static void OnAllowResisizingColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as SfGridBase).OnAllowResisizingColumnsChanged(e);
        }

        internal virtual void OnAllowResisizingColumnsChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets a value that indicates whether the hidden column can be resized using the mouse.
        /// </summary>
        /// <value> 
        /// <b>true</b> if the resizing can be enabled for the hidden columns in SfDataGrid; otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool AllowResizingHiddenColumns
        {
            get { return (bool)GetValue(AllowResizingHiddenColumnsProperty); }
            set { SetValue(AllowResizingHiddenColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowResizingHiddenColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowResizingHiddenColumns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowResizingHiddenColumnsProperty =
            GridDependencyProperty.Register("AllowResizingHiddenColumns", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnAllowResisizingHiddenColumnsChanged));

        private static void OnAllowResisizingHiddenColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            (obj as SfGridBase).OnAllowResisizingHiddenColumnsChanged(e);
        }
        internal virtual void OnAllowResisizingHiddenColumnsChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets a value that indicates whether the columns should be created automatically.
        /// </summary>
        /// <value> 
        /// <b>true</b> if the columns are automatically generated; otherwise , <b>false</b>. The default value is <b>true</b>.
        /// </value>
        /// <remarks>
        /// Each column gets notified in the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingColumn"/> event during its creation.
        /// You can cancel or customize the column being created by using this event.
        /// </remarks>
        public bool AutoGenerateColumns
        {
            get { return (bool)GetValue(AutoGenerateColumnsProperty); }
            set { SetValue(AutoGenerateColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            GridDependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(true, OnAutoGenerateColumnsChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether to generate columns automatically for the custom types in data object. 
        /// </summary>
        /// <value> 
        /// <b>true</b> if the custom type properties are automatically generated; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value> 
        /// <remarks>
        /// Setting AutoGenerateColumnsForCustomType to true, generate columns for inner properties.For example, OrderInfo class has Customer property of type Customer, then column generated like Customer.CustomerID.
        /// </remarks>
        public bool AutoGenerateColumnsForCustomType
        {
            get { return (bool)GetValue(AutoGenerateColumnsForCustomTypeProperty); }
            set { SetValue(AutoGenerateColumnsForCustomTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.AutoGenerateColumnsForCustomType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.AutoGenerateColumnsForCustomType dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AutoGenerateColumnsForCustomTypeProperty =
            GridDependencyProperty.Register("AutoGenerateColumnsForCustomType", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false));

        /// <summary>
        /// Dependency call backk for AutoGenerateColumns.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnAutoGenerateColumnsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnAutoGenerateColumnsChanged(args);
        }
        internal virtual void OnAutoGenerateColumnsChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets the <see cref="System.Windows.DataTemplate"/> by choosing a template conditionally based on data.
        /// </summary>
        /// <value>
        /// A custom <see cref="System.Windows.Controls.DataTemplateSelector"/> object that chooses the <see cref="System.Windows.DataTemplate"/> based on data. The default value is <b>null</b>.
        /// </value>   
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.GridColumn.CellTemplate"/>
        public DataTemplateSelector CellTemplateSelector
        {
            get { return (DataTemplateSelector)GetValue(CellTemplateSelectorProperty); }
            set { SetValue(CellTemplateSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellTemplateSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellTemplateSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CellTemplateSelectorProperty =
            GridDependencyProperty.Register("CellTemplateSelector", typeof(DataTemplateSelector), typeof(SfGridBase), new GridPropertyMetadata(null, OnCellTemplateSelectorChanged));


        /// <summary>
        /// Dependency call back for CellTemplateSelector property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnCellTemplateSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnCellTemplateSelectorChanged(e);
        }
        internal virtual void OnCellTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets a value that indicates how the columns are generated during automatic column generation.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoGenerateColumnsMode"/> enumeration that specifies how the columns are generated.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.AutoGenerateColumnsMode.None"/>.
        /// </value>        
        public AutoGenerateColumnsMode AutoGenerateColumnsMode
        {
            get { return (AutoGenerateColumnsMode)GetValue(AutoGenerateColumnsModeProperty); }
            set { SetValue(AutoGenerateColumnsModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumnsMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumnsMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AutoGenerateColumnsModeProperty =
            GridDependencyProperty.Register("AutoGenerateColumnsMode", typeof(AutoGenerateColumnsMode), typeof(SfGridBase), new GridPropertyMetadata(AutoGenerateColumnsMode.Reset, OnAutoGenerateColumnsModeChanged));

        /// <summary>
        /// Dependency call back for AutoGenerateColumnsMode.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnAutoGenerateColumnsModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnAutoGenerateColumnsModeChanged(args);
        }

        internal virtual void OnAutoGenerateColumnsModeChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data either at single or double click.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.SortClickAction"/> enumeration that specifies the sort click action.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.SortClickAction.SingleClick"/>.
        /// </value>            
        public SortClickAction SortClickAction
        {
            get { return (SortClickAction)GetValue(SortClickActionProperty); }
            set { SetValue(SortClickActionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortClickAction dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortClickAction dependency property.
        /// </remarks>       
        public static readonly DependencyProperty SortClickActionProperty =
            GridDependencyProperty.Register("SortClickAction", typeof(SortClickAction), typeof(SfGridBase), new GridPropertyMetadata(SortClickAction.SingleClick, null));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can sort the data to its initial order other than ascending or descending order.
        /// </summary>
        /// <value>
        /// <b>true</b> if the data is arranged to initial order; otherwise, <b>false</b>. The default value is <b>false</b>. 
        /// </value>        
        public bool AllowTriStateSorting
        {
            get { return (bool)this.GetValue(AllowTriStateSortingProperty); }
            set { this.SetValue(AllowTriStateSortingProperty, value); }
        }

        /// <summary>
        /// Gets or sets the shortcut menu that appears on each header cells.
        /// </summary>
        /// <value>
        /// The shortcut menu for each header cells. The default value is null.
        /// </value>  
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridColumnContextMenuInfo"/> for SfDataGrid and 
        /// MenuFlyoutItem receives <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnContextMenuInfo"/> for SfTreeGrid
        /// as command parameter which contains the corresponding record.
        /// </remarks>     
        public ContextMenu HeaderContextMenu
        {
            get { return (ContextMenu)GetValue(HeaderContextMenuProperty); }
            set { SetValue(HeaderContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.HeaderContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.HeaderContextMenu dependency property.
        /// </remarks>     
        public static readonly DependencyProperty HeaderContextMenuProperty =
           GridDependencyProperty.Register("HeaderContextMenu", typeof(ContextMenu), typeof(SfGridBase), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the shortcut menu that appears on each record cells.
        /// </summary>
        /// <value>
        /// The shortcut menu for each record. The default value is null.
        /// </value>     
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridRecordContextMenuInfo"/> for SfDataGrid and 
        /// MenuFlyoutItem receives <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridNodeContextMenuInfo"/> for SfTreeGrid
        /// as command parameter which contains the corresponding record.
        /// </remarks>
        public ContextMenu RecordContextMenu
        {
            get { return (ContextMenu)GetValue(RecordContextMenuProperty); }
            set { SetValue(RecordContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.RecordContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.RecordContextMenu dependency property.
        /// </remarks> 
        public static readonly DependencyProperty RecordContextMenuProperty =
         GridDependencyProperty.Register("RecordContextMenu", typeof(ContextMenu), typeof(SfGridBase), new GridPropertyMetadata(null));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowTriStateSorting dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowTriStateSorting dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowTriStateSortingProperty =
            GridDependencyProperty.Register("AllowTriStateSorting", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, null));

        /// <summary>
        /// Gets or sets a value that indicates whether the sequence number should be displayed on the header cell of sorted column during multi-column sorting.
        /// </summary>
        /// <value>
        /// <b>true</b> if the sequence number is displayed on the header cell of sorted column ; otherwise,<b>false</b>.The default value is <b>false</b> .
        /// </value>
        /// <remarks>
        /// The multi-column sorting can be applied by pressing CTRL key and the corresponding sequence number is displayed on its header cell of the column simultaneously.
        /// </remarks>
        public bool ShowSortNumbers
        {
            get { return (bool)this.GetValue(ShowSortNumbersProperty); }
            set { this.SetValue(ShowSortNumbersProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowSortNumbers dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowSortNumbers dependency property.
        /// </remarks>      
        public static readonly DependencyProperty ShowSortNumbersProperty =
            GridDependencyProperty.Register("ShowSortNumbers", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false, OnSortNumberPropertyChanged));

        /// <summary>
        /// Dependency Call back for Show Sort Numbers
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnSortNumberPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnSortNumberPropertyChanged(e);
        }

        internal virtual void OnSortNumberPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

        }

        /// <summary>
        /// Gets or sets a value to enable the built-in validation (IDataErrorInfo/DataAnnonations) to validate the user input and displays the error.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridValidationMode"/> enumeration that specifies how the row or cell is validated.
        /// The default mode is <see cref="Syncfusion.UI.Xaml.Grid.GridValidationMode.None"/>. 
        /// </value>        
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridValidationMode"/> process the built-in validations when data object implements <see cref="System.ComponentModel.IDataErrorInfo"/>,<see cref="System.ComponentModel.INotifyDataErrorInfo"/> and <see cref="System.ComponentModel.DataAnnotations"/>.
        /// The validation can also be done using CurrentCellValidating and RowValidating events in SfDataGrid and SfTreeGrid.
        /// </remarks>
        public GridValidationMode GridValidationMode
        {
            get { return (GridValidationMode)GetValue(GridValidationModeProperty); }
            set { SetValue(GridValidationModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GridValidationMode dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GridValidationMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GridValidationModeProperty =
            GridDependencyProperty.Register("GridValidationMode", typeof(GridValidationMode), typeof(SfGridBase), new GridPropertyMetadata(GridValidationMode.None, OnGridValidationPropertyChanded));


        private static void OnGridValidationPropertyChanded(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfGridBase;
            grid.OnGridValidationModePropertyChanded(e);
        }

        internal virtual void OnGridValidationModePropertyChanded(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets the index of corresponding selected cell or row.
        /// </summary>
        /// <value>
        /// The index of the selected item.The default value is -1.
        /// </value>        
        [Cloneable(false)]
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedIndex dependency property.
        /// </remarks>         
        public static readonly DependencyProperty SelectedIndexProperty =
            // WPF-35703 - Using FrameworkPropertyMetadataOpetions, to skip selectedindex changed opertion when the datacontext set as null value.
#if WPF
 DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SfGridBase),
                                        new FrameworkPropertyMetadata(-1, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedIndexChanged));
#else
 GridDependencyProperty.Register("SelectedIndex", typeof(int), typeof(SfGridBase),
                                        new GridPropertyMetadata(-1, OnSelectedIndexChanged));
#endif



        /// <summary>
        /// Dependency call back for Selected Index.
        /// User can set the SelectedIndex dynamically.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnSelectedIndexChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnSelectedIndexChanged(args);
        }

        internal virtual void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets or sets the data item bound to the row or cell that contains the current cell.
        /// </summary>
        /// <value>
        /// The object that is the currently selected item or null if there is no currently selected item.
        /// </value>        
        [Cloneable(false)]
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SelectedItemProperty =
#if WPF
 DependencyProperty.Register("SelectedItem", typeof(object), typeof(SfGridBase),
                                        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnSelectedItemChanged));
#else
 GridDependencyProperty.Register("SelectedItem", typeof(object), typeof(SfGridBase),
                                        new GridPropertyMetadata(null, OnSelectedItemChanged));
#endif

        /// <summary>
        /// Dependency call back for SelectedItem
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnSelectedItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            (obj as SfGridBase).OnSelectedItemChanged(args);
        }

        internal virtual void OnSelectedItemChanged(DependencyPropertyChangedEventArgs args)
        {

        }

        /// <summary>
        /// Gets the collection of object that contains data item of corresponding to the selected cells or rows in a SfDataGrid.
        /// </summary>
        /// <value>
        /// The collection of object that contains data item corresponding to the selected cells or rows. 
        /// </value>        
        [Cloneable(false)]
        public ObservableCollection<object> SelectedItems
        {
            get { return (ObservableCollection<object>)GetValue(SelectedItemsProperty); }
            set
            {
                SetValue(SelectedItemsProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItems dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItems dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SelectedItemsProperty =
            GridDependencyProperty.Register("SelectedItems", typeof(ObservableCollection<object>), typeof(SfGridBase), new GridPropertyMetadata(new ObservableCollection<object>(), OnSelectedItemsPropertyChanged));

        private static void OnSelectedItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnSelectedItemsPropertyChanged(e);
        }

        internal virtual void OnSelectedItemsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {

        }


        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> objects to sort the data programmatically.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.SortColumnDescription"/> object to sort the data programmatically.The default value is <b>null</b>.
        /// </value>
        [Cloneable(false)]
        public SortColumnDescriptions SortColumnDescriptions
        {
            get { return (SortColumnDescriptions)GetValue(SortColumnDescriptionsProperty); }
            set { SetValue(SortColumnDescriptionsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnDescriptions dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnDescriptions dependency property.
        /// </remarks>    
        public static readonly DependencyProperty SortColumnDescriptionsProperty =
            GridDependencyProperty.Register("SortColumnDescriptions", typeof(ObservableCollection<SortColumnDescription>), typeof(SfGridBase), new GridPropertyMetadata(null, OnSortColumnDescriptionsPropertyChanged));

        private static void OnSortColumnDescriptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as SfGridBase).OnSortColumnDescriptionsChanged(e);
        }

        internal virtual void OnSortColumnDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {

        }



        /// <summary>
        /// Gets or sets a value that indicates how the clipboard value is pasted into SfDataGrid and SfTreeGrid.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridPasteOption"/> enumeration that specifies how the clipboard value is pasted in to SfDataGrid.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridPasteOption.PasteData"/>. 
        /// </value>
        /// <remarks>
        /// You can customize or cancel the paste operations through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridPasteContent"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.PasteGridCellContent"/> event handlers in SfDataGrid. 
        /// You can cancel or customize the paste operations through <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.PasteContent"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.PasteCellContent"/> event handlers in SfTreeGrid.
        /// </remarks>
        public GridPasteOption GridPasteOption
        {
            get { return (GridPasteOption)GetValue(GridPasteOptionProperty); }
            set { SetValue(GridPasteOptionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.GridPasteOption dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.GridPasteOption dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GridPasteOptionProperty =
            GridDependencyProperty.Register("GridPasteOption", typeof(GridPasteOption), typeof(SfGridBase), new GridPropertyMetadata(GridPasteOption.PasteData));

        /// <summary>
        /// Gets or sets a value that indicates how the content is copied from SfDataGrid and SfTreeGrid control to the clipboard.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridCopyOption"/> enumeration that specifies how the content is copied from SfDataGrid control to the clipboard.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridCopyOption.CopyData"/>.
        /// </value>
        /// <remarks>
        /// You can cancel or customize the copy operation through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyContent"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CopyGridCellContent"/> event handlers in SfDataGrid.
        /// You can cancel or customize the copy operation through <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyContent"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CopyCellContent"/> event handlers in SfTreeGrid.
        /// </remarks>
        public GridCopyOption GridCopyOption
        {
            get { return (GridCopyOption)GetValue(GridCopyOptionProperty); }
            set { SetValue(GridCopyOptionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfGridBase.GridCopyOption dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfGridBase.GridCopyOption dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GridCopyOptionProperty =
            GridDependencyProperty.Register("GridCopyOption", typeof(GridCopyOption), typeof(SfGridBase), new GridPropertyMetadata(GridCopyOption.CopyData));

        /// <summary>
        /// Gets or sets value that indicates whether the tooltip should be displayed when mouse hovered on the cells in the control.
        /// </summary>
        /// <value>
        /// <b>true </b> if the tool tip is enabled in the column; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool ShowToolTip
        {
            get { return (bool)GetValue(ShowToolTipProperty); }
            set { SetValue(ShowToolTipProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Syncfusion.UI.Xaml.Grid.SfGridBase.ShowToolTip"/> dependency
        /// property.
        /// </summary>
        /// <remarks>
        /// The identifier for the <see cref="Syncfusion.UI.Xaml.Grid.SfGridBase.ShowToolTip"/> dependency property.
        /// </remarks>
        public static readonly DependencyProperty ShowToolTipProperty =
            GridDependencyProperty.Register("ShowToolTip", typeof(bool), typeof(SfGridBase), new GridPropertyMetadata(false));

        #endregion

        #region virtual methods
#if WPF
        internal virtual PropertyDescriptorCollection GetPropertyInfoCollection(object view)
#else
        internal virtual PropertyInfoCollection GetPropertyInfoCollection(object view)
#endif
        {
            return null;
        }

        /// <summary>
        /// Method generates Auto-generates columns for SfGridBase and returns the same.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="view"></param>
        /// <returns></returns>
        internal virtual ObservableCollection<GridColumnBase> GenerateGridColumns(ObservableCollection<GridColumnBase> columns, object view)
        {
            //To Developers
            //This method reflected and used in Excel exporting and PDF exporting. Avoid re-naming this method or Changing the functionality.
            if (columns == null || view == null)
                return null;

            var columnDisplayOrder = new List<KeyValuePair<int, GridColumnBase>>();
            var columnsWithoutOrder = new List<GridColumnBase>();
            var propertyCollection = GetPropertyInfoCollection(view);
            if (propertyCollection == null)
                return null;

            var columnscoll = GenerateColumns(propertyCollection, columns, string.Empty);
            foreach (var col in columnscoll)
            {
                columns.Add(col);
            }
            return columns;
        }
#if UWP
        internal virtual ObservableCollection<GridColumnBase> GenerateColumns(PropertyInfoCollection propertyCollection, ObservableCollection<GridColumnBase> gridColumns, string parentpropertyname)
#else
        internal virtual ObservableCollection<GridColumnBase> GenerateColumns(PropertyDescriptorCollection propertyCollection, ObservableCollection<GridColumnBase> gridColumns, string parentpropertyname)
#endif
        {
            ObservableCollection<GridColumnBase> columns = new ObservableCollection<GridColumnBase>();
            var columnDisplayOrder = new List<KeyValuePair<int, GridColumnBase>>();
            var columnsWithoutOrder = new List<GridColumnBase>();

#if UWP
            foreach (var keyvaluepair in propertyCollection)
            {
                var propertyinfo = keyvaluepair.Value;
                var displayAttribute = propertyinfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
                if (!propertyinfo.CanRead)
                    continue;
#else
            foreach (PropertyDescriptor propertyinfo in propertyCollection)
            {
                var displayAttribute = propertyinfo.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
                var attribute = propertyinfo.Attributes.ToList<Attribute>().FirstOrDefault(a => a is BindableAttribute);

                if (attribute != null)
                {
                    var bindableAttribute = attribute as BindableAttribute;
                    if (!bindableAttribute.Bindable)
                        continue;
                }
#endif
                if (displayAttribute != null)
                {
                    var canGenerate = displayAttribute.GetAutoGenerateField();
                    if (canGenerate.HasValue && !canGenerate.Value)
                        continue;
                    var displayOrder = displayAttribute.GetOrder();
                    var columnCollection = GenerateColumnwithComplexProperties(displayAttribute, propertyinfo, parentpropertyname, columns, gridColumns);
                    foreach (var column in columnCollection)
                    {
                        if (displayOrder.HasValue)
                            columnDisplayOrder.Add(new KeyValuePair<int, GridColumnBase>(displayOrder.Value, column));
                        else
                            columnsWithoutOrder.Add(column);
                    }
                }
                else
                {
                    columnsWithoutOrder.AddRange(GenerateColumnwithComplexProperties(displayAttribute, propertyinfo, parentpropertyname, columns, gridColumns));
                }
            }
            foreach (var keyValue in columnDisplayOrder.OrderBy(o => o.Key))
                columns.Add(keyValue.Value);
            foreach (var column in columnsWithoutOrder)
                columns.Add(column);
            return columns;
        }

#if UWP
        private ObservableCollection<GridColumnBase> GenerateColumnwithComplexProperties(DisplayAttribute displayAttribute, PropertyInfo propertyinfo,string parentpropertyname, ObservableCollection<GridColumnBase> columns, ObservableCollection<GridColumnBase> gridColumns)
#else
        private ObservableCollection<GridColumnBase> GenerateColumnwithComplexProperties(DisplayAttribute displayAttribute, PropertyDescriptor propertyinfo, string parentpropertyname, ObservableCollection<GridColumnBase> columns, ObservableCollection<GridColumnBase> gridColumns)
#endif
        {
            string mappingname = string.Empty;
            if (string.IsNullOrEmpty(parentpropertyname))
                mappingname = propertyinfo.Name;
            else
                mappingname = parentpropertyname + "." + propertyinfo.Name;

            if (propertyinfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyinfo.PropertyType)
#if WPF
 && !(propertyinfo.PropertyType.IsArray && propertyinfo.PropertyType.GetElementType().IsPrimitive))
#else
            && !(propertyinfo.PropertyType.IsArray && propertyinfo.PropertyType.GetElementType().IsPrimitive()))
#endif
            {
                return new ObservableCollection<GridColumnBase>();
            }

            if (this.AutoGenerateColumnsForCustomType && (GridUtil.IsComplexType(propertyinfo.PropertyType) || !typeof(byte[]).IsAssignableFrom(propertyinfo.PropertyType))
#if !SyncfusionFramework4_0
 && propertyinfo.PropertyType.GetTypeInfo().IsClass && propertyinfo.PropertyType != typeof(string))
#else            
                      && propertyinfo.PropertyType.IsClass && propertyinfo.PropertyType != typeof(string))
#endif

            {
#if UWP
                var itemProperties  = new PropertyInfoCollection(propertyinfo.PropertyType);
#else
                var itemProperties = TypeDescriptor.GetProperties(propertyinfo.PropertyType);
#endif
                return GenerateColumns(itemProperties, gridColumns, mappingname);
            }
            else
            {
                var getColumnsWithoutOrder = new ObservableCollection<GridColumnBase>();
                if (gridColumns.Any(c => c.MappingName == mappingname))
                    return getColumnsWithoutOrder;

                bool cancel = false;
                var column = CreateColumn(propertyinfo, displayAttribute, out cancel);
                column.MappingName = mappingname;
                RaiseAutoGeneratingEvent(ref column, ref cancel);
                column.IsAutoGenerated = true;
                if (!cancel)
                    getColumnsWithoutOrder.Add(column);

                return getColumnsWithoutOrder;
            }
        }

#if WPF
        private GridColumnBase CreateColumn(PropertyDescriptor propertyinfo, DisplayAttribute displayAttribute, out bool cancelArgs)
#else
        protected internal GridColumnBase CreateColumn(PropertyInfo propertyinfo, DisplayAttribute displayAttribute, out bool cancelArgs)
#endif
        {
#if WPF
            bool isReadOnly = propertyinfo.IsReadOnly;
#else
            bool isReadOnly = !propertyinfo.CanWrite;
#endif
            cancelArgs = false;

            var canAddColumn = propertyinfo.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(propertyinfo.PropertyType)
#if WPF
 && !(propertyinfo.PropertyType.IsArray && propertyinfo.PropertyType.GetElementType().IsPrimitive);
#else
            && !(propertyinfo.PropertyType.IsArray && propertyinfo.PropertyType.GetElementType().IsPrimitive());
#endif
            cancelArgs = canAddColumn;

            canAddColumn = (!GridUtil.IsComplexType(propertyinfo.PropertyType) || typeof(byte[]).IsAssignableFrom(propertyinfo.PropertyType));
            if (!canAddColumn)
                cancelArgs = true;

            GridColumnBase column = null;
            var dataType = false;
            if (cancelArgs)
            {
                column = CreateTextColumn(propertyinfo);
                if (isReadOnly)
                    column.AllowEditing = !isReadOnly;
                return column;
            }
#if WPF
            var datatypeAttribute = propertyinfo.Attributes.OfType<DataTypeAttribute>().FirstOrDefault();
            if (datatypeAttribute != null)
            {
                switch (datatypeAttribute.DataType)
                {
                    case DataType.PhoneNumber:
                        dataType = true;
                        column = CreateMaskColumn(propertyinfo);
                        break;
                    case DataType.Currency:
                        dataType = true;
                        column = CreateCurrencyColumn(propertyinfo);
                        break;
                }
            }
#endif
            if (!dataType)
            {

                //Issue Fix: WRT-1291
                //If the Bound value is of type Object 'IsAssignableFrom(typeof(DateTime))' condition passes for any of the conditions and DateTimeColumn is generated (Since it is the first if clause) when AutoGenerateColumns=True
                //Hence we have checked for object type as first condition and generated it as TextColumn with AllowEditing as False
                if (propertyinfo.PropertyType.IsAssignableFrom(typeof(System.Object)))
                {
                    column = CreateTextColumn(propertyinfo);
                    column.AllowEditing = false;
                }
                else if (propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTime)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTime?)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTimeOffset)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTimeOffset?)))
                    column = CreateDateTimeColumn(propertyinfo);
                else if (propertyinfo.PropertyType.IsAssignableFrom(typeof(bool)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(bool?)))
                    column = CreateCheckBoxColumn(propertyinfo);
                else if (propertyinfo.PropertyType.IsAssignableFrom(typeof(int)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(int?))
                    || propertyinfo.PropertyType.IsAssignableFrom(typeof(double)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(double?))
                    || propertyinfo.PropertyType.IsAssignableFrom(typeof(float)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(float?)) ||
                    propertyinfo.PropertyType.IsAssignableFrom(typeof(decimal)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(decimal?)))
                {
                    column = CreateNumericColumn(propertyinfo);
                }
                else if (ReflectionExtensions.IsEnum(propertyinfo.PropertyType))
                    column = CreateComboBoxColumn(propertyinfo);
                else if (propertyinfo.PropertyType.IsAssignableFrom(typeof(Uri)))
                    column = CreateHyperLinkColumn(propertyinfo);
#if WPF
                else if (propertyinfo.PropertyType.IsAssignableFrom(typeof(TimeSpan)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(TimeSpan?)))
                {
                    column = CreateTimeSpanColumn(propertyinfo);                   
                }
#endif
                else
                    column = CreateTextColumn(propertyinfo);  
                if (isReadOnly)
                    column.AllowEditing = !isReadOnly;
            }

            if (displayAttribute != null)
            {
#if WPF
                ResourceManager displayAttributeResourceManager = null;
#else
                ResourceLoader displayAttributeResourceLoader = null;
#endif
                if (displayAttribute.ResourceType != null)
                {
#if WPF
                    var resourceName = displayAttribute.ResourceType.FullName.Replace('_', '.');
                    Assembly assembly = displayAttribute.ResourceType.Assembly;
                    displayAttributeResourceManager = new ResourceManager(resourceName, assembly);
#else
                    var resourceName = displayAttribute.ResourceType.Name;
                    displayAttributeResourceLoader = ResourceLoader.GetForViewIndependentUse(resourceName);
#endif
                }

                if (displayAttribute.Name != null)
                {
#if WPF
                    if (displayAttributeResourceManager != null)
                        column.HeaderText = displayAttributeResourceManager.GetString(displayAttribute.Name, System.Globalization.CultureInfo.CurrentCulture);
#else
                    if (displayAttributeResourceLoader != null)
                        column.HeaderText = displayAttributeResourceLoader.GetString(displayAttribute.Name);
#endif
                    else
                        column.HeaderText = displayAttribute.Name;
                }
                if (displayAttribute.ShortName != null)
                {
#if WPF
                    if (displayAttributeResourceManager != null)
                        column.HeaderText = displayAttributeResourceManager.GetString(displayAttribute.ShortName, System.Globalization.CultureInfo.CurrentCulture);
#else
                    if (displayAttributeResourceLoader != null)
                        column.HeaderText = displayAttributeResourceLoader.GetString(displayAttribute.ShortName);
#endif
                    else
                        column.HeaderText = displayAttribute.ShortName;
                }

#if WPF
                if (displayAttribute.GetAutoGenerateFilter() != null)
                {
                    if (column is GridColumn)
                        (column as GridColumn).AllowFiltering = displayAttribute.GetAutoGenerateFilter().Value;
                }
#endif
                if (displayAttribute.Description != null)
                {
                    string description = null;
#if WPF
                    if (displayAttributeResourceManager != null)
                        description = displayAttributeResourceManager.GetString(displayAttribute.Description, System.Globalization.CultureInfo.CurrentCulture);
#else
                    if (displayAttributeResourceLoader != null)
                        description = displayAttributeResourceLoader.GetString(displayAttribute.Description);
#endif
                    else
                        description = displayAttribute.Description.ToString();
                    if (description != null)
                    {
                        var strBuilder = new StringBuilder();
                        strBuilder.Append("<DataTemplate ");
                        strBuilder.Append("xmlns='http://schemas.microsoft.com/winfx/");
                        strBuilder.Append("2006/xaml/presentation' ");
                        strBuilder.Append("xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml' ");
                        strBuilder.Append("> <TextBlock ");
                        strBuilder.Append("Text='");
                        strBuilder.Append(description);
                        strBuilder.Append("' />");
                        strBuilder.Append("</DataTemplate>");
#if WPF
                        var dt = (DataTemplate)XamlReader.Load(ToStream(strBuilder.ToString()));
#else
                        var dt = (DataTemplate)XamlReader.Load(strBuilder.ToString());
#endif
#if WPF
                        if (column is GridColumn)
                            (column as GridColumn).HeaderToolTipTemplate = dt;
#endif
                    }
                }

            }
#if WPF
            var editableAttribute = propertyinfo.Attributes.OfType<EditableAttribute>().FirstOrDefault();
#else
            var editableAttribute = propertyinfo.GetCustomAttributes(typeof(EditableAttribute), true).FirstOrDefault() as EditableAttribute;
#endif
            if (editableAttribute != null)
            {
                if (editableAttribute.AllowEdit)
                {
                    this.NavigationMode = NavigationMode.Cell;
                    column.AllowEditing = true;
                }
                else
                    column.AllowEditing = false;
            }
            column.IsAutoGenerated = true;
            return column;
        }

#if WPF
        internal Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
#endif

        internal virtual void RaiseAutoGeneratingEvent(ref GridColumnBase column, ref bool cancel)
        {

        }

#if WPF
        internal virtual GridColumnBase CreateTextColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateTextColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }


#if WPF
        internal virtual GridColumnBase CreateNumericColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateNumericColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }

#if WPF
        internal virtual GridColumnBase CreateDateTimeColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateDateTimeColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }
#if WPF
        internal virtual GridColumnBase CreateCheckBoxColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateCheckBoxColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }

#if WPF
        internal virtual GridColumnBase CreateHyperLinkColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateHyperLinkColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }
#if WPF
        internal virtual GridColumnBase CreateComboBoxColumn(PropertyDescriptor propertyinfo)
#else
        internal virtual GridColumnBase CreateComboBoxColumn(PropertyInfo propertyinfo)
#endif
        {
            return null;
        }
#if WPF
        internal virtual GridColumnBase CreateMaskColumn(PropertyDescriptor propertyinfo)
        {
            return null;
        }

        internal virtual GridColumnBase CreateTimeSpanColumn(PropertyDescriptor propertyinfo)
        {
            return null;
        }

        internal virtual GridColumnBase CreateCurrencyColumn(PropertyDescriptor propertyinfo)
        {
            return null;
        }
#endif

        internal virtual bool CanCellTapped()
        {
            return false;
        }

        internal virtual bool CanCellDoubleTapped()
        {
            return false;
        }
        internal virtual bool CanCellToolTipOpening()
        {
            return false;
        }
        #endregion

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {

        }
    }
}
