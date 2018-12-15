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
using System.ComponentModel.DataAnnotations;
using Syncfusion.UI.Xaml.Grid.Cells;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using Syncfusion.Data;
using System.Reflection;
using Syncfusion.UI.Xaml.Utility;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Text;
using System.Dynamic;
using Syncfusion.Dynamic;
using Syncfusion.Data.Helper;
using System.Resources;
using System.Globalization;
using Syncfusion.UI.Xaml.Grid;

using Syncfusion.UI.Xaml.TreeGrid.Helpers;
using Syncfusion.UI.Xaml.TreeGrid.Cells;
#if UWP
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Syncfusion.UI.Xaml.Controls.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using Windows.UI.Xaml.Markup;
using Windows.ApplicationModel.Resources;
#else
using System.Windows.Controls;
using System.Windows;
using System.Windows.Media;
using System.Windows.Data;
using System.Windows.Input;

using System.Windows.Markup;
using System.Data;
using Syncfusion.Windows.Shared;
using System.Windows.Automation.Peers;
using System.Windows.Documents;

#endif

namespace Syncfusion.UI.Xaml.TreeGrid
{
#if UWP
    using ContextMenu = Windows.UI.Xaml.Controls.MenuFlyout;
#endif
    [TemplatePart(Name = "PART_TreeGridPanel", Type = typeof(TreeGridPanel))]
    /// <summary>
    /// Represents a control that displays the data in a tree format.
    /// </summary>
    /// <remarks>
    /// The SfTreeGrid control provides a flexible way to display and manipulate
    /// self-relational and hierarchical data and the built-in column types
    /// allows the data to be displayed in to appropriate editor.
    /// </remarks>
    public class SfTreeGrid : SfGridBase
    {
        private TreeGridPanel _treeGridPanel = null;

        /// <summary>
        /// Instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowGenerator"/> of SfTreeGrid 
        /// </summary>
        protected internal TreeGridRowGenerator RowGenerator = null;

        /// <summary>
        /// Initialize a new instance of the SfTreeGrid class.
        /// </summary>
        static SfTreeGrid()
        {
#if WPF
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SfTreeGrid), new FrameworkPropertyMetadata(typeof(SfTreeGrid)));
#endif
        }

        /// <summary>
        /// Initialize a new instance of the SfTreeGrid class.
        /// </summary>
        public SfTreeGrid()
        {
#if !WPF
            base.DefaultStyleKey = typeof(SfTreeGrid);
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateRailsX |
                                  ManipulationModes.TranslateRailsY | ManipulationModes.TranslateInertia;
#endif
            InitializeCollections();
            SetRowGenerator();
            InitializeSelectionController();
            this.TreeGridModel = new TreeGridModel(this);
            this.ColumnResizingController = new TreeGridColumnResizingController(this);
            this.ColumnDragDropController = new TreeGridColumnDragDropController(this);
#if UWP
            this.RowDragDropController = new TreeGridRowDragDropController(this);
#endif
            this.TreeGridColumnSizer = new TreeGridColumnSizer(this);
            cellRenderers = new TreeGridCellRendererCollection(this);
            ValidationHelper = new TreeGridValidationHelper(this);
            NodeCheckBoxController = new NodeCheckBoxController(this);
            this.TreeGridCopyPaste = new TreeGridCutCopyPaste(this);
            this.InitializeCellRendererCollection();
        }
        internal bool isViewPropertiesEnsured = false;
        internal bool isselectedindexchanged = false;
        internal bool isselecteditemchanged = false;
        internal bool isSelectedItemsChanged = false;
        internal bool isCurrentItemChanged = false;

        /// <summary>
        /// Occurs when the Node is being expanded.
        /// </summary>
        /// <remarks>
        /// You can cancel the Node being expanded through <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeExpandedEventArgs"/> event argument.
        /// </remarks>
        public event NodeExpandingEventHandler NodeExpanding;

        /// <summary>
        /// Occurs after the Node is expanded.
        /// </summary>
        public event NodeExpandedEventHandler NodeExpanded;

        /// <summary>
        /// Occurs when the Node is being collapsed.
        /// </summary>
        /// <remarks>
        /// You can cancel the Node being collapsed through <see cref="Syncfusion.UI.Xaml.TreeGrid.NodeCollapsingEventArgs"/> event argument.
        /// </remarks>
        public event NodeCollapsingEventHandler NodeCollapsing;

        /// <summary>
        /// Occurs after the Node is collapsed.
        /// </summary>
        public event NodeCollapsedEventHandler NodeCollapsed;

        /// <summary>
        /// Occurs after expander node check box is checked. 
        /// </summary>
        public event NodeCheckStateChangedEventHandler NodeCheckStateChanged;


        /// <summary>
        /// Occurs when the column is being reordered in to a new position.
        /// </summary>
        /// <remarks>
        /// You can cancel the column being dragged through <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeQueryColumnDraggingEventArgs"/> event argument.
        /// </remarks>
        public event TreeGridColumnDraggingEventHandler ColumnDragging;

        /// <summary>
        /// Occurs when the column width changes during resizing.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being resized through <see cref="Syncfusion.UI.Xaml.Grid.ResizingColumnsEventArgs"/> event argument .
        /// </remarks>
        public event ResizingColumnsEventHandler ResizingColumns;

        internal NodeExpandingEventArgs RaiseNodeExpanding(NodeExpandingEventArgs e)
        {
            if (NodeExpanding != null)
                NodeExpanding(this, e);
            return e;
        }

        internal void RaiseNodeExpanded(NodeExpandedEventArgs e)
        {
            if (NodeExpanded != null)
                NodeExpanded(this, e);
        }

        internal bool RaiseNodeCollapsing(NodeCollapsingEventArgs e)
        {
            if (NodeCollapsing != null)
                NodeCollapsing(this, e);
            return !e.Cancel;
        }

        internal void RaiseNodeCollapsed(NodeCollapsedEventArgs e)
        {
            if (NodeCollapsed != null)
                NodeCollapsed(this, e);
        }

        internal void RaiseNodeCheckStateChanged(NodeCheckStateChangedEventArgs e)
        {
            if (NodeCheckStateChanged != null)
                NodeCheckStateChanged(this, e);
        }


        /// <summary>
        /// Method which helps to initialize all the collection in SfTreeGrid
        /// </summary>
        /// <remarks></remarks>
        private void InitializeCollections()
        {
            SetValue(ColumnsProperty, new TreeGridColumns());
            SetValue(SelectedItemsProperty, new ObservableCollection<object>());
            SetValue(SortColumnDescriptionsProperty, new SortColumnDescriptions());
            SetValue(SortComparersProperty, new SortComparers());
        }

        /// <summary>
        /// This event is used to request an IEnumerable object that holds the child item objects for a particular parent item in on demand loading.
        /// </summary>
        /// <remarks>
        /// In the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRequestTreeItemsEventArgs"/> parent object will be passed,  
        /// and your event handler needs to provide an IEnumerable object that contains the child objects for this parent object.
        /// If the parent object is null, you should provide the collection of root objects for the tree.
        /// </remarks>
        public event TreeGridRequestTreeItemsEventHandler RequestTreeItems;

        internal TreeGridRequestTreeItemsEventArgs RaiseRequestTreeItemsEvent(TreeGridRequestTreeItemsEventArgs args)
        {
            if (RequestTreeItems != null)
                this.RequestTreeItems(this, args);
            return args;
        }

        private TreeGridCellRendererCollection cellRenderers = null;

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView"/> instance which manage the nodes, expand, collapse operations
        /// and sorting in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// View will be created based on ItemsSource you are setting. Below are the list of
        /// CollectionViews available in SfTreeGrid. 
        /// <list type="number">
        /// 		<item>
        /// 			<description>TreeGridQueryableView</description>
        /// 		</item>
        /// 		<item>
        /// 			<description>TreeGridSelfRelationalView</description>
        /// 		</item>
        /// 		<item>
        /// 			<description>TreeGridUnboundReview</description>
        /// 		</item>
        /// 	</list>
        /// </remarks>
        public TreeGridView View { get; set; }

        private TreeGridColumnDragDropController columnDragDropController;
        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> which controls the column drag-and-drop operation in SfTreeGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnDragDropController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        public TreeGridColumnDragDropController ColumnDragDropController
        {
            get
            {
                return columnDragDropController;
            }
            set
            {
                if (columnDragDropController != null)
                    columnDragDropController.Dispose();
                columnDragDropController = value;
            }
        }

#if UWP
        private TreeGridRowDragDropController rowDragDropController;
        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> which controls the row drag-and-drop operation in SfTreeGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowDragDropController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        public TreeGridRowDragDropController RowDragDropController
        {
            get
            {
                return rowDragDropController;
            }
            set
            {
                if (rowDragDropController != null)
                    rowDragDropController.Dispose();
                rowDragDropController = value;
            }
        }
#endif
        private TreeGridColumnResizingController columnResizingController;
        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> which controls the resizing operation in SfTreeGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> class.
        /// </value> 
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnResizingController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        public TreeGridColumnResizingController ColumnResizingController
        {
            get
            {
                return columnResizingController;
            }
            set
            {
                if (columnResizingController != null)
                    columnResizingController.Dispose();
                columnResizingController = value;
            }
        }

        private TreeGridColumnSizer treeGridColumnSizer;
        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnSizer"/> which controls columns sizing and column width calculation. 
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnSizer"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridColumnSizer"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks> 
        public TreeGridColumnSizer TreeGridColumnSizer
        {
            get
            {
                return treeGridColumnSizer;
            }
            set
            {
                if (treeGridColumnSizer != null)
                    treeGridColumnSizer.Dispose();
                treeGridColumnSizer = value;
                if (treeGridColumnSizer != null && treeGridColumnSizer.GridBase == null)
                    treeGridColumnSizer.GridBase = this;
            }
        }

        /// <summary>
        /// Method which will initiate the Cell Renderer's Collection.
        /// </summary>
        /// <remarks></remarks>
        private void InitializeCellRendererCollection()
        {
            cellRenderers.Add("Static", new TreeGridCellTextBlockRenderer());
            cellRenderers.Add("Header", new TreeGridHeaderCellRenderer());
            cellRenderers.Add("TextBlock", new TreeGridCellTextBlockRenderer());
            cellRenderers.Add("RowHeader", new TreeGridRowHeaderCellRenderer());
            cellRenderers.Add("TextBox", new TreeGridCellTextBoxRenderer());
            cellRenderers.Add("CheckBox", new TreeGridCellCheckBoxRenderer());
            cellRenderers.Add("ComboBox", new TreeGridCellComboBoxRenderer());
            cellRenderers.Add("Numeric", new TreeGridCellNumericRenderer());
            cellRenderers.Add("Template", new TreeGridCellTemplateRenderer());
            cellRenderers.Add("Hyperlink", new TreeGridCellHyperlinkRenderer());
            cellRenderers.Add("DateTime", new TreeGridCellDateTimeRenderer());
#if WPF
            cellRenderers.Add("Percent", new TreeGridCellPercentageRenderer());
            cellRenderers.Add("Currency", new TreeGridCellCurrencyRenderer());
            cellRenderers.Add("Mask", new TreeGridCellMaskRenderer());
#endif
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridCellRendererCollection"/> instance which
        /// holds the collection of all predefined cell renderers <see cref="Syncfusion.UI.Xaml.TreeGrid.Cells.TreeGridVirtualizingCellRenderer{D, E}"/>.
        /// </summary>
        /// <remarks>
        /// 	<para>The cell renderers provides various properties and virtual methods to
        /// customize its operations .When any of the predefined renderer is customized ,
        /// that should be replaced to the <b>CellRenderers</b> collection with its
        /// appropriate cell type. The below table shows the predefined renderers and its
        /// corresponding cell type associated with column. </para>
        /// 	<list type="table">
        /// 		<listheader>
        /// 			<term>Renderer  Class</term>
        /// 			<description>Cell Type</description>
        /// 			<description>Associated column</description>
        /// 		</listheader>
        /// 		<item>
        /// 			<term>TreeGridCellTextBoxRenderer</term>
        /// 			<description>TextBox</description>
        /// 			<description>TreeGridTextColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellNumericRenderer</term>
        /// 			<description>Numeric</description>
        /// 			<description>TreeGridNumericColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellCheckBoxRenderer</term>
        /// 			<description>CheckBox</description>
        /// 			<description>TreeGridCheckBoxColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellTemplateRenderer</term>
        /// 			<description>Template</description>
        /// 			<description>TreeGridTemplateColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellComboBoxRenderer</term>
        /// 			<description>ComboBox</description>
        /// 			<description>TreeGridComboBoxColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellDateTimeRenderer</term>
        /// 			<description>DateTime</description>
        /// 			<description>TreeGridDateTimeColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellHyperLinkRenderer</term>
        /// 			<description>HyperLink</description>
        /// 			<description>TreeGridHyperLinkColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridHeaderCellRenderer</term>
        /// 			<description>Header</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridCellTextBlockRenderer</term>
        /// 			<description>TextBlock</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>TreeGridRowHeaderCellRenderer</term>
        /// 			<description>RowHeader</description>
        /// 			<description>-</description>
        /// 		</item>  		
        /// 	</list>
        /// </remarks>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// //The customized TreeGridCellTextBoxRendererExt is replaced to CellRenderers collection after removed the default renderer of TreeGridTextColumn.
        /// this.treeGrid.CellRenderers.Remove("TextBox");
        /// this.treeGrid.CellRenderers.Add("TextBox",new TreeGridCellTextBoxRendererExt());
        /// ]]></code>
        /// </example>
        public TreeGridCellRendererCollection CellRenderers
        {
            get { return cellRenderers; }
        }

        #region Selection


        private TreeGridAutoScroller autoScroller;

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller"/> to perform horizontal or vertical scrolling automatically , 
        /// when the row/column is dragged outside of the visible boundaries in SfTreeGrid.
        /// </summary> 
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridAutoScroller"/>.
        /// </value>
        public TreeGridAutoScroller AutoScroller
        {
            get
            {
                if (this.autoScroller == null)
                {
                    this.autoScroller = new TreeGridAutoScroller();
                }

                return this.autoScroller;
            }
            set
            {
                this.autoScroller = value;
            }
        }


        private TreeGridBaseSelectionController selectionController;

        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController"/> which controls selection operations in SfTreeGrid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridBaseSelectionController"/>.
        /// </value>
        /// <remarks>
        /// The default behavior of row selection can be customized by assigning the class derived from <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowSelectionController"/> to <b>SelectionController</b> property. 
        /// </remarks>
        public TreeGridBaseSelectionController SelectionController
        {
            get
            {
                return selectionController;
            }
            set
            {
                if (selectionController != null)
                    selectionController.Dispose();
                selectionController = value;
            }
        }


        /// <summary>
        /// Gets or sets a value that indicates whether to listen <see cref="System.ComponentModel.INotifyPropertyChanging.PropertyChanging"/> and <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> events of data object and <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> event of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.SoureCollection"/>.
        /// </summary>     
        /// <remarks>
        /// By default, view listens to <see cref="System.ComponentModel.INotifyPropertyChanging.PropertyChanging"/> and <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> events of data object and <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> event of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.SoureCollection"/>.
        /// </remarks>
        public NotificationSubscriptionMode NotificationSubscriptionMode
        {
            get { return (NotificationSubscriptionMode)GetValue(NotificationSubscriptionModeProperty); }
            set { SetValue(NotificationSubscriptionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NotificationMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.NotificationMode dependency property.
        /// </remarks>  
        public static readonly DependencyProperty NotificationSubscriptionModeProperty =
            DependencyProperty.Register("NotificationSubscriptionMode ", typeof(NotificationSubscriptionMode), typeof(SfTreeGrid), new PropertyMetadata(NotificationSubscriptionMode.CollectionChange | NotificationSubscriptionMode.PropertyChange, OnNotificationSubscriptionModeChanged));

        private static void OnNotificationSubscriptionModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGrid = obj as SfTreeGrid;
            if (!treeGrid.isGridLoaded)
                return;
            if (treeGrid.View != null)
                throw new NotSupportedException("Cannot change the NotificationSubscriptionMode if view is already created");
        }
        internal override void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded || this.View == null)
            {
                this.isselectedindexchanged = true;
                return;
            }
            this.isselectedindexchanged = false;

            if ((int)args.NewValue == -1 || ((int)args.NewValue <= this.ResolveToNodeIndex(this.GetLastDataRowIndex()) && (int)args.NewValue >= 0))
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
                {
                    NewValue = args.NewValue,
                    OldValue = args.OldValue,
                    PropertyName = "SelectedIndex"
                });
            }
            else
                this.SelectedIndex = (int)args.OldValue;
        }

        internal override void OnSelectionModeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded)
                return;
            if (this.HasView)
                this.nodeCheckBoxController.ValidateCheckBoxSelectionMode();
            this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs() { NewValue = args.NewValue, OldValue = args.OldValue, PropertyName = "SelectionMode" });
            // WPF-36747 - Update CheckBox binding while changing SelectionMode.
            foreach (var column in this.Columns.OfType<TreeGridCheckBoxColumn>())
                this.RowGenerator.UpdateBinding(column);
        }

        internal override void OnSelectedItemsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= this.OnSelectedItemsCollectionChanged;
            if (e.NewValue is INotifyCollectionChanged)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += this.OnSelectedItemsCollectionChanged;
        }

        /// <summary>
        /// Method which is called when the SelectedItems Changed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnSelectedItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var grid = this as SfTreeGrid;
            if (grid == null || !grid.isGridLoaded || grid.View == null)
            {
                grid.isSelectedItemsChanged = true;
                return;
            }
            grid.isSelectedItemsChanged = false;
            this.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.SelectedItemsCollection);
        }

        internal override void OnSelectedItemChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded || this.View == null)
            {
                this.isselecteditemchanged = true;
                return;
            }
            this.isselecteditemchanged = false;

            this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
            {
                NewValue = args.NewValue,
                OldValue = args.OldValue,
                PropertyName = "SelectedItem"
            });
        }

        internal override void OnNavigationModeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded)
                return;
            this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs() { NewValue = args.NewValue, OldValue = args.OldValue, PropertyName = "NavigationMode" });
        }

        internal override void OnGridValidationModePropertyChanded(DependencyPropertyChangedEventArgs args)
        {
            if (this.isGridLoaded)
            {
                foreach (TreeGridColumn column in this.Columns)
                {
                    column.UpdateValidationMode();
                }
            }
        }

        internal override void OnHeaderRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            // If grid has no rows, RowHeights should not be updated
            if (!isGridLoaded || TreeGridPanel.RowCount == 0)
                return;
            TreeGridPanel.RowHeights[0] = (double)e.NewValue;
            TreeGridPanel.InvalidateMeasure();
        }

        internal override void OnRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!isGridLoaded)
                return;

            TreeGridPanel.RowHeights.DefaultLineSize = (double)e.NewValue;
            TreeGridPanel.InvalidateMeasure();
        }


        /// <summary>
        /// Scrolls the SfTreeGrid vertically and horizontally to display the cell for the specified RowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the rowColumnIndex of the cell to scroll into view.
        /// </param>
        public void ScrollInView(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.RowIndex < this.TreeGridPanel.ScrollRows.LineCount && rowColumnIndex.ColumnIndex < this.TreeGridPanel.ScrollColumns.LineCount)
            {
                if (rowColumnIndex.RowIndex >= 0)
                    this.TreeGridPanel.ScrollRows.ScrollInView(rowColumnIndex.RowIndex);
                if (rowColumnIndex.ColumnIndex >= 0)
                    this.TreeGridPanel.ScrollColumns.ScrollInView(rowColumnIndex.ColumnIndex);
                this.TreeGridPanel.InvalidateMeasureInfo();
            }
        }


        /// <summary>
        /// Selects all the rows in SfTreeGrid. 
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public void SelectAll()
        {
            this.SelectionController.SelectAll();
        }

        /// <summary>
        /// Clears all the selection present in SfTreeGrid.
        /// </summary>
        /// <param name="exceptCurrentRow">
        /// Indicates whether the selection should be cleared for the current row or not.
        /// </param>
        /// <remarks>
        /// This method helps to clear the selection programmatically.
        /// </remarks>
        public void ClearSelections(bool exceptCurrentRow)
        {
            this.SelectionController.ClearSelections(exceptCurrentRow);
        }
        /// <summary>
        /// Method which is helps to reset the selection based values.
        /// </summary>
        /// <remarks></remarks>
        private void ResetSelectionValues()
        {
            //Added condition to clear the selection when selection or current cell is maintained.
            if (this.SelectionController != null && (this.SelectionController.SelectedRows.Any() || this.SelectionController.CurrentCellManager.CurrentCell != null))
                this.SelectionController.ClearSelections(false);
        }
        /// <summary>
        /// Selects the row based on its specified the start and end index .                      
        /// </summary>
        /// <param name="startRowIndex">
        /// Specifies the start index of the row to select. 
        /// </param>
        /// <param name="endRowIndex">
        /// Specifies the end index of the row to select.
        /// </param>
        /// <remarks>
        /// This is applicable for Multiple and Extended selection mode.
        /// </remarks>
        public void SelectRows(int startRowIndex, int endRowIndex)
        {
            this.SelectionController.SelectRows(startRowIndex, endRowIndex);
        }

        /// <summary>
        /// Gets or sets the data item that corresponds to the focused row or cell.
        /// </summary>
        /// <value>
        /// The corresponding focused data item.The default registered value is null.
        /// </value>        
        public object CurrentItem
        {
            get { return (object)GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentItem dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(object), typeof(SfTreeGrid), new PropertyMetadata(null, OnCurrentItemChanged));

        private static void OnCurrentItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            if (!grid.isGridLoaded || grid.View == null)
            {
                grid.isCurrentItemChanged = true;
                return;
            }
            grid.isCurrentItemChanged = false;
            var handle = new SelectionPropertyChangedHandlerArgs()
            {
                NewValue = args.NewValue,
                OldValue = args.OldValue,
                PropertyName = "CurrentItem"
            };
            grid.SelectionController.HandleSelectionPropertyChanges(handle);
        }

        /// <summary>
        /// Gets or sets the column that contains the current cell.
        /// </summary>
        /// <value>
        /// The column that contains the current cell. The default value is null.
        /// </value>
        public TreeGridColumn CurrentColumn
        {
            get { return (TreeGridColumn)GetValue(CurrentColumnProperty); }
            set { SetValue(CurrentColumnProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentColumn dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CurrentColumn dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentColumnProperty =
            DependencyProperty.Register("CurrentColumn", typeof(TreeGridColumn), typeof(SfTreeGrid), new PropertyMetadata(null));



        /// <summary>
        ///  Occurs when the current cell is being activated in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// You can cancel the current cell is being activated through the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatingEventArgs"/> event argument.
        /// </remarks>
        public event CurrentCellActivatingEventHandler CurrentCellActivating;

        /// <summary>
        /// Occurs after the current cell is activated in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatingEventArgs"/> event if that event is not canceled.         
        /// </remarks>
        public event CurrentCellActivatedEventHandler CurrentCellActivated;

        /// <summary>
        /// Occurs when the edit mode starts for the current cell in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// You can cancel the current cell is being edited through the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs"/> event argument.
        /// </remarks>
        public event TreeGridCurrentCellBeginEditEventHandler CurrentCellBeginEdit;

        /// <summary>
        /// Occurs when the edit mode ends for the current cell.
        /// </summary>
        public event CurrentCellEndEditEventHandler CurrentCellEndEdit;

        /// <summary>
        /// Occurs when the current cell value Changes.
        /// </summary>
        public event TreeGridCurrentCellValueChangedEventHandler CurrentCellValueChanged;

        /// <summary>
        /// Occurs when the SelectedItem changed in the drop down of GridMultiColumnDropDownList or GridComboBoxColumn.
        /// </summary>
        public event CurrentCellDropDownSelectionChangedEventHandler CurrentCellDropDownSelectionChanged;

        /// <summary>
        /// Occurs when the GridHyperLinkColumn's cell is request for navigation.
        /// </summary>
        public event CurrentCellRequestNavigateEventHandler CurrentCellRequestNavigate;

        /// <summary>
        /// Occurs when column is generated for the properties in underlying data object.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being created using the <see cref="Syncfusion.UI.Xaml.TreeGrid.AutoGeneratingTreeGridColumnArgs"/> event argument.
        /// </remarks>
        public event TreeGridAutoGeneratingColumnEventHandler AutoGeneratingColumn;

        /// <summary>
        /// Occurs when the user clicks or touch the cell in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// CellTapped does not occurs when the end user clicks or touch the non-selectable cells. 
        /// </remarks>
        public event TreeGridCellTappedEventHandler CellTapped;
        /// <summary>
        /// Occurs when the user hovers the cell in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// CellToolTipOpening does not occurs when the tooltip not enabled for the cells. 
        /// </remarks>
        public event TreeGridCellToolTipOpeningEventHandler CellToolTipOpening;
        /// <summary>
        /// Occurs when the user double clicks or touch the cell in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// CellDoubleTapped does not occurs when the end user double clicks or touch the non-selectable cells. 
        /// </remarks>
        public event TreeGridCellDoubleTappedEventHandler CellDoubleTapped;

        /// <summary>
        /// Occurs while moving to other cells from edited cell for validating the user input.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the current cell being validated through the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellValidatingEventArgs"/> event argument.
        /// </remarks>
        public event TreeGridCurrentCellValidatingEventHandler CurrentCellValidating;


        /// <summary>
        /// Occurs after the current cell validated.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCurrentCellValidatedEventArgs"/> event if that event is not canceled.         
        /// </remarks>
        public event TreeGridCurrentCellValidatedEventHandler CurrentCellValidated;

        /// <summary>
        /// Occurs while the moving from the edited row to validate the user input.
        /// </summary>
        /// <remarks>
        /// You can cancel the row is being validated through the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowValidatingEventArgs"/> event argument.
        /// </remarks>
        public event TreeGridRowValidatingEventHandler RowValidating;

        /// <summary>
        /// Occurs after the row is validated.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowValidating"/> event if that event is not canceled.         
        /// </remarks>
        public event TreeGridRowValidatedEventHandler RowValidated;

        internal override void OnShowRowHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnShowRowHeaderChanged(e);
            SetExpanderColumnIndex();
            if (!this.isGridLoaded)
                return;

            (this.TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
            if ((bool)e.NewValue)
            {
                this.SelectionController.HandleGridOperations(new TreeGridOperationsHandlerArgs(TreeGridOperation.RowHeaderChanged, null));
                this.TreeGridPanel.InsertColumns(0, 1);
                this.TreeGridPanel.ColumnWidths[0] = this.RowHeaderWidth;
                this.TreeGridPanel.UpdateScrollBars();
                if (this.FrozenColumnCount > 0)
                    this.TreeGridPanel.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
                else
                    this.TreeGridPanel.FrozenColumns = 1;
            }
            else
            {
                this.SelectionController.HandleGridOperations(new TreeGridOperationsHandlerArgs(TreeGridOperation.RowHeaderChanged, null));
                this.TreeGridPanel.RemoveColumns(0, 1);
                if (this.FrozenColumnCount > 0)
                    this.TreeGridPanel.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
                else
                    this.TreeGridPanel.FrozenColumns = 0;
                this.TreeGridPanel.UpdateScrollBars();
            }
            this.TreeGridPanel.NeedToRefreshColumn = true;
            this.TreeGridPanel.InvalidateMeasure();
            (this.TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
            this.RowGenerator.UpdateRowIndentMargin();
            if (this.TreeGridPanel.ColumnCount > 0)
            {
                //WPF_33924 While setting ShowRowHeader using binding, the header row is not refreshed on loading initially.
                //So, the RefreshHeaders method is called here.
                this.RowGenerator.RefreshHeaders();
                this.TreeGridColumnSizer.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether expander should be displayed for all nodes even though it does not have child nodes
        /// </summary>
        /// <value>
        /// <b>true</b> if the expander is displayed for all nodes; otherwise, <b>false</b>.The default value is <b>false</b>.
        /// </value>       
        internal bool ShowExpanderAlways
        {
            get { return (bool)GetValue(ShowExpanderAlwaysProperty); }
            set { SetValue(ShowExpanderAlwaysProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowExpanderAlways dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowExpanderAlways dependency property.
        /// </remarks>        
        internal static readonly DependencyProperty ShowExpanderAlwaysProperty =
            DependencyProperty.Register("ShowExpanderAlways", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false, OnShowExpanderChanged));

        private static void OnShowExpanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
        }

        /// <summary>
        /// Gets or sets a value which specifies whether checkbox should be displayed next to expander.
        /// </summary>
        /// <value>
        /// The default value is false.
        /// </value>
        /// <seealso cref="SfTreeGrid.CheckBoxSelectionMode"/>
        /// <seealso cref="SfTreeGrid.EnableRecursiveChecking"/>
        /// <seealso cref="SfTreeGrid.AllowTriStateChecking"/>
        /// <seealso cref="SfTreeGrid.RecursiveCheckingMode"/>
        public bool ShowCheckBox
        {
            get { return (bool)GetValue(ShowCheckBoxProperty); }
            set { SetValue(ShowCheckBoxProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowCheckBox dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ShowCheckBox dependency property.
        /// </remarks>    
        public static readonly DependencyProperty ShowCheckBoxProperty =
            DependencyProperty.Register("ShowCheckBox", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false, OnShowCheckBoxChanged));

        private static void OnShowCheckBoxChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
            foreach (var row in grid.RowGenerator.Items)
            {
                if (row.RowType != TreeRowType.DefaultRow)
                    continue;
                (row as TreeDataRow).UpdateExpanderCell();
                row.RowElement.UpdateIndentMargin();
            }
            if (grid.TreeGridPanel.ColumnCount > 0)
            {
                //While setting ShowCheckBox using binding, the header row is not refreshed on loading initially.
                //So, the RefreshHeaders method is called here.
                grid.RowGenerator.RefreshHeaders();
                grid.TreeGridColumnSizer.Refresh();
            }
        }
        /// <summary>
        /// Returns the checked nodes in SfTreeGrid.
        /// </summary>
        /// <param name="includeAllNodes">which specifies whether need to include the nodes which are not in view.</param>
        /// <returns>Collection of <see cref="TreeNode"/>.</returns>
        public ObservableCollection<TreeNode> GetCheckedNodes(bool includeAllNodes = false)
        {
            return nodeCheckBoxController.GetCheckedNodes(includeAllNodes);
        }

        internal override void OnRowHeaderWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;

            if (this.showRowHeader)
            {
                this.TreeGridPanel.ColumnWidths[0] = (double)(e.NewValue);
                this.RowGenerator.UpdateRowIndentMargin();
                this.TreeGridColumnSizer.Refresh();
            }
        }

        /// <summary>
        /// Return true if the View doesn't Null.
        /// </summary>
        internal bool HasView
        {
            get { return this.View != null; }
        }

        #region Selection events
        /// <summary>
        /// Occurs when the current selection changes. 
        /// </summary>       
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionChanging"/> event if that event is not canceled. 
        /// It will be raised both UI and programmatic selection.
        /// </remarks>
        public event GridSelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Occurs when the selection is being changed in SfTreeGrid. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the selection being changed through the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs"/> event argument.        
        /// It will be raised both UI and programmatic selection. 
        /// </remarks>
        public event GridSelectionChangingEventHandler SelectionChanging;

        #endregion

        private void InitializeSelectionController()
        {
            if (CheckBoxSelectionMode == CheckBoxSelectionMode.Default)
                this.SelectionController = new TreeGridRowSelectionController(this);
            else
                this.SelectionController = new CheckBoxRowSelectionController(this);
        }

        internal static Brush GridSelectionForgroundBrush = new SolidColorBrush(Colors.Black);

        /// <summary>
        /// Gets or sets a brush that highlights the background of the currently selected row or cell.
        /// </summary> 
        /// <value>
        /// The brush that highlights the background of the selected row or cell.
        /// </value>
        public Brush SelectionBackground
        {
            get { return (Brush)GetValue(SelectionBackgroundProperty); }
            set { SetValue(SelectionBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionBackground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionBackground dependency property.
        /// </remarks>            
        public static readonly DependencyProperty SelectionBackgroundProperty =
            DependencyProperty.Register("SelectionBackground", typeof(Brush), typeof(SfTreeGrid), new PropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 128, 128, 128)), OnSelectionBackgroundChanged));


        private static void OnSelectionBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGrid = obj as SfTreeGrid;
            if (!treeGrid.isGridLoaded)
                return;

            foreach (var dataRow in treeGrid.RowGenerator.Items)
            {
                if (dataRow.RowType == TreeRowType.HeaderRow || dataRow.RowElement == null)
                    continue;

                dataRow.RowElement.SelectionBackground = treeGrid.SelectionBackground;
            }
        }

        /// <summary>
        /// Gets or sets a brush that will be applied as the foreground of selected row.
        /// </summary>
        /// <value>
        /// The brush that will be applied as the foreground of selected row.The default value is Black.
        /// </value>
        public Brush SelectionForeground
        {
            get { return (Brush)GetValue(SelectionForegroundProperty); }
            set { SetValue(SelectionForegroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionForeground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelectionForeground dependency property.
        /// </remarks>            
        public static readonly DependencyProperty SelectionForegroundProperty =
            DependencyProperty.Register("SelectionForeground", typeof(Brush), typeof(SfTreeGrid), new PropertyMetadata(GridSelectionForgroundBrush, OnSelectionForegroundChanged));


        private static void OnSelectionForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGrid = obj as SfTreeGrid;
            if (!treeGrid.isGridLoaded)
                return;
            foreach (var dataRow in treeGrid.RowGenerator.Items)
            {
                if (dataRow.IsSelectedRow && dataRow.RowElement.SelectionForeground == SfTreeGrid.GridSelectionForgroundBrush)
                    dataRow.RowElement.Foreground = treeGrid.SelectionForeground;
            }
        }


        #endregion
        /// <summary>
        /// Initialize a new instance of <see cref="TreeGridRowGenerator"/> .
        /// </summary>        
        private void SetRowGenerator()
        {
            this.RowGenerator = new TreeGridRowGenerator(this);
        }

        /// <summary>
        /// Builds the visual tree for the SfTreeGrid when a new template is applied.
        /// </summary>
#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            if (this.TreeGridPanel != null)
                this.TreeGridPanel.OnItemSourceChanged();
            _treeGridPanel = this.GetTemplateChild("PART_TreeGridPanel") as TreeGridPanel;
#if UWP
            if (this.TreeGridPanel != null)
            {
                this.TreeGridPanel.ScrollOwner = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
                this.TreeGridPanel.ScrollableOwner = GetTemplateChild("PART_ScrollViewer") as ScrollableContentViewer;
            }
#endif
            showRowHeader = this.ShowRowHeader;
            this.RefreshPanelAndView();
        }

#if WPF
        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            if (!e.Handled)
            {
                if (!this.SelectionController.CurrentCellManager.HasCurrentCell)
                    return;

                if (this.SelectionController.CurrentCellManager.CurrentCell.ColumnElement is TreeGridCell)
                    e.Handled = (this.SelectionController.CurrentCellManager.CurrentCell.ColumnElement as TreeGridCell).ShowContextMenu();
            }
        }

        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (!SelectionController.CurrentCellManager.HasCurrentCell)
            {
                base.OnTextInput(e);
                return;
            }
            var rowColumnIndex = SelectionController.CurrentCellManager.CurrentRowColumnIndex;
            var dataRow = RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow != null && dataRow is TreeDataRow)
            {
                var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
                char text;
                char.TryParse(e.Text, out text);

                if (dataColumn != null)
                {
                    if (((dataColumn.TreeGridColumn.IsTemplate && (dataColumn.TreeGridColumn as TreeGridTemplateColumn).hasEditTemplate)))
                        return;

                    if (!dataColumn.IsEditing && char.IsLetterOrDigit(text) && SelectionController.CurrentCellManager.BeginEdit())
                    {
                        dataColumn.Renderer.PreviewTextInput(e);
                        //WPF-37762- While EditeElement got focus need to handle it here.
                        e.Handled = true;
                    }
                }
            }
            base.OnTextInput(e);
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (!this.HasView)
            {
                base.OnPreviewKeyDown(e);
                return;
            }
            if (!e.OriginalSource.Equals(this))
            {
                var el = e.OriginalSource as DependencyObject;
                if (el != null)
                {
                    if (el is ComboBoxItem)
                    {
                        var sender = GridUtil.FindDescendant(el, typeof(ComboBox));
                        if (sender != null && VisualContainer.GetWantsMouseInput(sender, this) == true)
                            return;
                    }
                    if (VisualContainer.GetWantsMouseInput(el, this) == true)
                    {
                        var currentCell = this.SelectionController.CurrentCellManager.CurrentCell;
                        if (currentCell != null && currentCell.IsEditing && currentCell.Renderer.IsDropDownable)
                        {
                            // WPF-33982 - DataGrid can be hanlded Home and End key insteadof combobox, if the renderer is combobox.
                            if (e.Key != Key.Tab && e.Key != Key.Enter && e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Right && e.Key != Key.Left &&
                                e.Key != Key.Escape && e.Key != Key.F2 && e.Key != Key.Home && e.Key != Key.End)
                                return;
                        }
                        else
                            return;
                    }
                }
                // Editing case
            }
            SelectionController.HandleKeyDown(e);
            base.OnPreviewKeyDown(e);
        }
#else
        /// <summary>
        /// Invoked when the <c>ContainerKeyDown</c> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>        
        protected bool OnContainerKeyDown(KeyEventArgs e)
        {
            SelectionController.HandleKeyDown(e);
            return e.Handled;
        }

        /// <summary>
        /// Invoked when the <c>KeyDown</c> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e"> 
        /// The <see cref="T:Windows.UI.Xaml.Input.KeyRoutedEventArgs">KeyRoutedEventArgs</see> that contains the event data.
        /// </param>
        /// <remarks>
        /// Handling the keydown operations of SfTreeGrid
        /// </remarks>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool result = false;
            result = SelectionController.HandleKeyDown(e);
            if (result)
                e.Handled = true;
        }
#endif

        internal bool isGridLoaded;
#if WPF
        internal static bool suspendForColumnMove;
        internal int oldIndexForMove;
#endif
        private TreeGridValidationHelper validationHelper;
        internal TreeGridValidationHelper ValidationHelper
        {
            get { return validationHelper; }
            set { validationHelper = value; }
        }

        private NodeCheckBoxController nodeCheckBoxController;
        internal NodeCheckBoxController NodeCheckBoxController
        {
            get { return nodeCheckBoxController; }
            set { nodeCheckBoxController = value; }
        }


        /// <summary>
        /// Refreshes TreeGridPanel and View properties based on SfTreeGrid property settings.        
        /// </summary>
        /// <example>
        /// The View.CurrentItem updated based on SfTreeGrid.CurrentItem property.
        /// </example>
        protected virtual void RefreshPanelAndView()
        {
            if (this.TreeGridPanel == null || this.RowGenerator == null)
                return;
#if UWP
            this.TreeGridPanel.ContainerKeydown = OnContainerKeyDown;
#endif
            this.TreeGridPanel.DragBorderBrush = this.BorderBrush;
            this.TreeGridPanel.DragBorderThickness = this.BorderThickness;
            this.TreeGridPanel.SetRowGenerator(this.RowGenerator);
            if (!isGridLoaded)
            {
                this.UnWireEvents();
                this.SetSourceList(this.ItemsSource);
            }
            this.UpdateAutoScroller();
            UpdateRowandColumnCount();
            this.EnsureProperties();
            if (!isGridLoaded)
            {
                this.isGridLoaded = true;
                if (this.HasView)
                    this.EnsureViewProperties();
                // Need to raise ItemsSourceChanged event after updating row and column count.
                if (ItemsSource != null)
                {
                    this.RaiseItemsSourceChanged(null, this.ItemsSource);
                }
                this.WireEvents();
            }
        }


        /// <summary>        
        /// Updates the frozen rows count.
        /// </summary>
        protected internal virtual void RefreshHeaderLineCount()
        {
            headerLineCount = 1;
        }


        internal virtual void SetSourceList(object itemsSource)
        {
#if UWP
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;
#endif
            this.nodeCheckBoxController.ValidateCheckBoxSelectionMode();
            this.UnWireEvents();
            this.CreateTreeGridCollectionView(itemsSource);

            UpdateColumnSettings();

            if (View == null)
                return;
            View.LiveNodeUpdateMode = LiveNodeUpdateMode;

            if (AutoExpandMode == AutoExpandMode.AllNodesExpanded)
                this.TreeGridModel.ExpandAllNodes(false);
            else if (AutoExpandMode == AutoExpandMode.RootNodesExpanded)
            {
                this.TreeGridModel.ExpandAllNodes(0, false);
            }

            this.WireEvents();
        }

        internal void UpdateColumnSettings()
        {
            if (!Columns.Any())
                return;
            this.Columns.ForEach(column => column.SetGrid(this));
        }

        internal void CreateTreeGridCollectionView(object itemsSource)
        {
            this.View = this.CreateTreeGridView(this.GetSourceList(itemsSource), this);
            this.Columns.Suspend();
            if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.Reset)
            {
                foreach (var item in this.Columns.Where(c => c.IsAutoGenerated).ToList())
                {
                    this.Columns.Remove(item);
                    UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, item);
                }
            }
            else if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
            {
                var canreset = Columns.Count > 0;
                this.Columns.Clear();
                if (canreset)
                    UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Reset, null);
            }
            this.Columns.Resume();
            this.TreeGridColumnSizer.ResetAutoCalculationforAllColumns();

            if (View == null) return;
            if (TreeGridPanel != null && this.AutoGenerateColumns && ((!this.Columns.Any() && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.RetainOld) || (this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.Reset || this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)))
                this.GenerateColumns();

            this.View.LiveNodeUpdateMode = this.LiveNodeUpdateMode;
            this.View.EnableRecursiveChecking = EnableRecursiveChecking;
            this.View.RecursiveCheckingMode = RecursiveCheckingMode;
            this.View.CheckBoxMappingName = CheckBoxMappingName;
            this.View.NotificationSubscriptionMode = NotificationSubscriptionMode;
            this.View.FilterLevel = FilterLevel;
#if WPF
            this.View.DispatchOwner = this.Dispatcher;
#endif
            nodeCheckBoxController.ValidateCheckBoxPropertyName();
        }

        private void UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction action, TreeGridColumn column = null)
        {
            if (action == NotifyCollectionChangedAction.Remove)
            {
                if (column == null)
                    throw new InvalidOperationException("You must pass column");               
                if (this.SortColumnDescriptions.Any(desc => desc.ColumnName == column.MappingName))
                    this.SortColumnDescriptions.Remove(
                        this.SortColumnDescriptions.FirstOrDefault(
                            desc => desc.ColumnName == column.MappingName));
            }
            else if(action == NotifyCollectionChangedAction.Reset)
            {
                // WPF-19472 - if GroupColumnDescriptions is cleared first, sort column description not present in group column description added to SortDescription. But that column was not present in grid columns.
                // To skip this, need to clear SortColumnDescriptions first.
                if (this.SortColumnDescriptions.Count > 0)
                    this.SortColumnDescriptions.Clear();                
            }
            else
            {
                throw new NotImplementedException("Not implemented for action" + action.ToString());
            }
        }

        internal TreeGridView CreateTreeGridView(IEnumerable source, SfTreeGrid treeGrid)
        {
            TreeGridView view = null;
            if (source is TreeGridQueryableView)
            {
                if (source is TreeGridNestedView)
                {
                    view = source as TreeGridNestedView;
                }
                else if (source is TreeGridSelfRelationalView)
                {
                    view = source as TreeGridSelfRelationalView;
                }
                else
                    view = source as TreeGridQueryableView;
                view.AttachTreeView(this);
            }
            else if (source is TreeGridUnboundView)
            {
                view = source as TreeGridUnboundView;
                view.AttachTreeView(this);
            }
            else if (RequestTreeItems != null)
                view = new TreeGridUnboundView(treeGrid);
            else if (source != null)
            {
                if (!string.IsNullOrEmpty(ParentPropertyName) && !string.IsNullOrEmpty(ChildPropertyName))
                    view = new TreeGridSelfRelationalView(source, treeGrid);
                else if (!string.IsNullOrEmpty(ChildPropertyName))
                    view = new TreeGridNestedView(source, treeGrid);
                else
                    view = new TreeGridQueryableView(source, treeGrid);
            }
            return view;
        }


        internal IEnumerable GetSourceList(object source)
        {
            IEnumerable result = null;
            if (source != null)
            {
#if WPF
                if (!(source is DataTable) && !(source is DataView))
#endif
                    result = source as IEnumerable;

            }
            return result;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.PreviousSize.Width != 0 || (_treeGridPanel != null && _treeGridPanel.previousArrangeWidth != 0))
            {
                //Merged revision: 515113   
#if UWP
                if (_treeGridPanel != null && this.IsLoaded)
                    _treeGridPanel.InvalidateMeasureInfo();
#endif
                return;
            }
            if (TreeGridPanel != null)
            {
                TreeGridPanel.InvalidateMeasure();
            }
        }

        private bool isEventsFired = false;

        /// <summary>
        /// Wires the events associated with the SfDataGrid.
        /// </summary>
        protected virtual void WireEvents()
        {
            if (isEventsFired)
                return;

            WireTreeGridEvents();
            if (this.TreeGridModel != null)
                this.TreeGridModel.WireEvents();
            isEventsFired = true;
        }

        /// <summary>
        /// Unwires the events associated with the SfDataGrid.
        /// </summary>
        protected virtual void UnWireEvents()
        {
            if (!isEventsFired)
                return;

            if (this.TreeGridModel != null)
                this.TreeGridModel.UnwireEvents();
            isEventsFired = false;
        }
        private void WireTreeGridEvents()
        {
            this.SizeChanged += OnSizeChanged;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Method which is helps to Unhook the SfDataGrid based events.
        /// </summary>
        /// <remarks></remarks>
        private void UnWireTreeGridEvents()
        {
            this.SizeChanged -= OnSizeChanged;
            this.Loaded -= OnLoaded;
            this.Unloaded -= OnUnloaded;
        }

#if UWP
        internal bool IsLoaded = false;
#endif
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#if UWP
            IsLoaded = true;
#endif
            //WPF-23465 – Validation flags reset on loading the grid which is not equals to Validations ActiveGrid.

            if (this.ValidationHelper != null && this != TreeGridValidationHelper.ActiveGrid)
            {
                this.ValidationHelper.SetCurrentCellValidated(true);
                this.ValidationHelper.SetCurrentRowValidated(true);
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //WPF-22097 – Validation flags reset on unloading. 
            if (this.ValidationHelper != null && this == TreeGridValidationHelper.ActiveGrid)
                this.ValidationHelper.ResetValidations(true);
        }

        internal bool hasCellStyleSelector;
        internal bool hasExpanderCellStyleSelector;
        internal bool hasCellStyle;
        internal bool hasExpanderCellStyle;
        internal bool hasHeaderTemplate;
        internal bool hasHeaderStyle;

        internal bool hasRowStyleSelector;
        internal bool hasRowStyle;

        /// <summary>
        /// Gets or sets the style applied to all the record cells in SfTreeGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the record cells in SfTreeGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCell"/>.
        /// </remarks>
        public Style CellStyle
        {
            get { return (Style)GetValue(CellStyleProperty); }
            set
            {
                SetValue(CellStyleProperty, value);
                if (this.ReadLocalValue(CellStyleProperty) != DependencyProperty.UnsetValue)
                {
                    this.hasCellStyle = true;
                    this.UpdateCellStyles();
                }
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CellStyleProperty =
            DependencyProperty.Register("CellStyle", typeof(Style), typeof(SfTreeGrid), new PropertyMetadata(null, OnCellStyleChanged));


        /// <summary>
        /// Gets or sets the style applied to expander cells in SfTreeGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to expander cells in SfTreeGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridExpanderCell"/>.
        /// </remarks>
        public Style ExpanderCellStyle
        {
            get { return (Style)GetValue(ExpanderCellStyleProperty); }
            set { SetValue(ExpanderCellStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderCellStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty ExpanderCellStyleProperty =
            DependencyProperty.Register("ExpanderCellStyle", typeof(Style), typeof(SfTreeGrid), new PropertyMetadata(null, OnExpanderCellStyleChanged));


        /// <summary>
        /// Gets or sets the style applied to expander cells conditionally based on data in SfTreeGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to cell based on data. The default value is <b>null</b>.
        /// </value>  
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridExpanderCell"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderCellStyle"/>
        public StyleSelector ExpanderCellStyleSelector
        {
            get { return (StyleSelector)GetValue(ExpanderCellStyleSelectorProperty); }
            set { SetValue(ExpanderCellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderCellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderCellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty ExpanderCellStyleSelectorProperty =
            DependencyProperty.Register("ExpanderCellStyleSelector", typeof(StyleSelector), typeof(SfTreeGrid), new PropertyMetadata(null, OnExpanderCellStyleSelectorChanged));


        /// <summary>
        /// Dependency call back for CellStyleSelector property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnExpanderCellStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null) return;
            grid.hasExpanderCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                var expanderColumnIndex = grid.expanderColumnIndex;
                grid.UpdateColumnCellStyle(expanderColumnIndex);
            }
        }


        /// <summary>
        /// Gets or sets the style applied to the data rows in SfTreeGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all data rows in SfTreeGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowControl"/>.
        /// </remarks>
        public Style RowStyle
        {
            get { return (Style)GetValue(RowStyleProperty); }
            set { SetValue(RowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty RowStyleProperty =
            DependencyProperty.Register("RowStyle", typeof(Style), typeof(SfTreeGrid), new PropertyMetadata(null, OnRowStyleChanged));

        /// <summary>
        /// Dependency call back for RowStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null) return;
            grid.hasRowStyle = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }

        /// <summary>
        /// update row styles for grid
        /// </summary>
        /// <remarks></remarks>
        private void UpdateRowStyle()
        {
            this.RowGenerator.Items.ForEach(row => row.UpdateRowStyles(row.RowElement as ContentControl));
        }


        /// <summary>
        /// Gets or sets the style applied to data row conditionally based on data in SfTreeGrid.        
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to data row based on data. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridRowControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowStyle"/>
        public StyleSelector RowStyleSelector
        {
            get { return (StyleSelector)GetValue(RowStyleSelectorProperty); }
            set { SetValue(RowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty RowStyleSelectorProperty =
            DependencyProperty.Register("RowStyleSelector", typeof(StyleSelector), typeof(SfTreeGrid), new PropertyMetadata(null, OnRowStyleSelectorChanged));

        /// <summary>
        /// Dependency call back for RowStyleSelector property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null) return;
            grid.hasRowStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }

        /// <summary>
        /// Gets or sets the style applied to record cells conditionally based on data in SfTreeGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to cell based on data. The default value is <b>null</b>.
        /// </value>  
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCell"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellStyle"/>
        public StyleSelector CellStyleSelector
        {
            get { return (StyleSelector)GetValue(CellStyleSelectorProperty); }
            set { SetValue(CellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CellStyleSelectorProperty =
            DependencyProperty.Register("CellStyleSelector", typeof(StyleSelector), typeof(SfTreeGrid), new PropertyMetadata(null, OnCellStyleSelectorChanged));


        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the header cell in SfTreeGrid.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the header cell in SfTreeGrid. The default value is <b>null</b>.
        /// </value>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.HeaderTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.HeaderTemplate dependency property.
        /// </remarks>        
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(SfTreeGrid), new PropertyMetadata(null, OnHeaderTemplateChanged));


        /// <summary>
        /// Gets or sets the style applied to all the header cells in SfTreeGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the header cells in SfTreeGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for header cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridHeaderCell"/>.        
        /// </remarks>
        public Style HeaderStyle
        {
            get
            {
                return (Style)this.GetValue(HeaderStyleProperty);
            }
            set
            {
                SetValue(HeaderStyleProperty, value);
                if (this.ReadLocalValue(HeaderStyleProperty) != DependencyProperty.UnsetValue)
                {
                    hasHeaderStyle = true;
                    this.UpdateHeaderRowStyle();
                }
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.HeaderStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.HeaderStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty HeaderStyleProperty =
            DependencyProperty.Register("HeaderStyle", typeof(Style), typeof(SfTreeGrid), new PropertyMetadata(null, OnHeaderStyleChanged));

        /// <summary>
        /// Dependency call back for CellStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null)
                return;
            grid.hasCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateCellStyles();
            }
        }


        private static void OnExpanderCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null)
                return;
            grid.hasExpanderCellStyle = true;
            if (grid.isGridLoaded)
            {
                var expanderColumnIndex = grid.expanderColumnIndex;
                grid.UpdateColumnCellStyle(expanderColumnIndex);
            }
        }


        /// <summary>
        /// Dependency call back for CellStyleSelector property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnCellStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid == null) return;
            grid.hasCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateCellStyles();
            }
        }


        /// <summary>
        /// Dependency call back for HeaderTemplate property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnHeaderTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid != null)
                grid.hasHeaderTemplate = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateHeaderRowStyle();
            }
        }

        /// <summary>
        /// Dependency call back for HeaderStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnHeaderStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (grid != null)
                grid.hasHeaderStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateHeaderRowStyle();
            }
        }

        /// <summary>
        /// Gets or sets the collection that contains all the columns in SfTreeGrid.
        /// </summary>      
        /// <value>
        /// The collection that contains all the columns in SfTreeGrid. This property
        /// has no default value.
        /// </value>  
        /// <remarks>
        /// Each column associated with its own renderer and it controls the corresponding column related operations.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellRenderers"/>
        /// <exception cref="System.NullReferenceException">Thrown when the Columns value is set as null.</exception>
        public TreeGridColumns Columns
        {
            get { return (TreeGridColumns)GetValue(ColumnsProperty); }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.Columns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.Columns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register("Columns", typeof(TreeGridColumns), typeof(SfTreeGrid), new PropertyMetadata(new TreeGridColumns(), OnColumnsPropertyChanged));

        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var treeGrid = d as SfTreeGrid;

            if (e.OldValue != e.NewValue)
            {
                if (e.OldValue is TreeGridColumns)
                    (e.OldValue as INotifyCollectionChanged).CollectionChanged -= treeGrid.OnTreeColumnCollectionChanged;
                if (e.NewValue is TreeGridColumns)
                {
                    (e.NewValue as INotifyCollectionChanged).CollectionChanged += treeGrid.OnTreeColumnCollectionChanged;
                    treeGrid.SetExpanderColumnIndex();
                    treeGrid.UpdateColumnSettings();
                }
                if (!treeGrid.isGridLoaded)
                    return;
                treeGrid.UpdateRowandColumnCount();
                treeGrid.TreeGridColumnSizer.Refresh();
                //Set isDirty is true, In EnsureColumns to update the column even if column index is match in existing visible columns
                treeGrid.RowGenerator.Items.ForEach(dataRow => dataRow.isDirty = true);
                if (treeGrid.TreeGridPanel != null)
                    treeGrid.TreeGridPanel.InvalidateMeasureInfo();
            }
        }

        internal int expanderColumnIndex;


        /// <summary>
        /// TreeColumns collection changed event handling
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnTreeColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SetExpanderColumnIndex();
            if (!this.isGridLoaded)
                return;

            if (suspendForColumnPopulation || this.Columns.suspendUpdate)
                return;

            var newStartIndex = this.ResolveToScrollColumnIndex(e.NewStartingIndex);
            var oldStartIndex = this.ResolveToScrollColumnIndex(e.OldStartingIndex);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (TreeGridColumn column in e.NewItems)
                            column.SetGrid(this);
                        if (this.TreeGridPanel.ColumnCount == 0)
                        {
                            this.UpdateColumnCount(false);
                            this.UpdateIndentColumnWidths();
                        }
                        else
                        {

#if UWP
                            this.TreeGridPanel.InsertColumns(newStartIndex, 1);
#else
                            if (!suspendForColumnMove)
                                this.TreeGridPanel.InsertColumns(newStartIndex, 1);
                            else
                                goto case NotifyCollectionChangedAction.Move;
#endif
                            this.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.ColumnsCollection);
                        }
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
#if WPF
                    if (!suspendForColumnMove)
                    {
#endif
                        var removedColumn = e.OldItems[0] as TreeGridColumn;
                        removedColumn.ColumnPropertyChanged = null;
                        if (this.Columns.Count == 0)
                        {
                            if (this.SelectionController != null)
                                this.SelectionController.ClearSelections(false);
                        }
                        this.TreeGridPanel.RemoveColumns(oldStartIndex, 1);
                        this.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.ColumnsCollection);
                        UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, removedColumn);
#if WPF
                    }
                    else
                        this.oldIndexForMove = oldStartIndex;
#endif
                    break;
                case NotifyCollectionChangedAction.Move:
#if WPF
                    if (suspendForColumnMove)
                        oldStartIndex = this.oldIndexForMove;
#endif
                    this.TreeGridPanel.RemoveColumns(oldStartIndex, 1);
                    this.TreeGridPanel.InsertColumns(newStartIndex, 1);
#if WPF
                    var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems[0], this.ResolveToGridVisibleColumnIndex(newStartIndex), this.ResolveToGridVisibleColumnIndex(oldStartIndex));
                    this.SelectionController.HandleCollectionChanged(args, TreeGridCollectionChangedReason.ColumnsCollection);
#else
                    this.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.ColumnsCollection);
#endif
                    break;
                case NotifyCollectionChangedAction.Replace:
                    this.TreeGridPanel.RemoveColumns(oldStartIndex, 1);
                    this.TreeGridPanel.InsertColumns(newStartIndex, 1);
                    this.SelectionController.HandleCollectionChanged(e, TreeGridCollectionChangedReason.ColumnsCollection);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    if (this.SelectionController != null)
                        this.SelectionController.ClearSelections(false);
                    this.TreeGridPanel.RemoveColumns(0, TreeGridPanel.ColumnCount);
                    if (this.SortColumnDescriptions.Count > 0)
                        this.SortColumnDescriptions.Clear();
                    break;
            }
            this.TreeGridPanel.UpdateScrollBars();
            this.TreeGridPanel.InvalidateMeasure();
            this.RowGenerator.UpdateRowIndentMargin();
            if (this.TreeGridPanel.ColumnCount > 0)
            {
#if WPF
                if (!SfTreeGrid.suspendForColumnMove)
                {
#endif
                    this.TreeGridColumnSizer.Refresh();
                    if (this.AllowResizingHiddenColumns && this.AllowResizingColumns)
                    {
                        this.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(e.OldStartingIndex, e.NewStartingIndex);
                    }
#if WPF
                }
#endif
            }

        }

        /// <summary>
        /// Ensures selection related operations in View.
        /// </summary>
        protected void EnsureViewProperties()
        {
            if (isViewPropertiesEnsured)
                return;

            if (isselectedindexchanged)
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
                {
                    NewValue = this.SelectedIndex,
                    OldValue = -1,
                    PropertyName = "SelectedIndex"
                });
                isselectedindexchanged = false;
            }

            if (isselecteditemchanged)
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
                {
                    NewValue = this.SelectedItem,
                    OldValue = null,
                    PropertyName = "SelectedItem"
                });
                isselecteditemchanged = false;
            }

            if (isCurrentItemChanged)
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
                {
                    NewValue = this.CurrentItem,
                    OldValue = null,
                    PropertyName = "CurrentItem"
                });
                isCurrentItemChanged = false;
            }

            if (isSelectedItemsChanged)
            {
                if (this.SelectedItems.Count > 0)
                {
                    this.SelectionController.HandleCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.SelectedItems, 0), TreeGridCollectionChangedReason.SelectedItemsCollection);
                }
                isSelectedItemsChanged = false;
            }

            SelectionController.SelectCheckedNodes();

            isViewPropertiesEnsured = true;
        }
        /// <summary>
        /// Ensures RowHeight and HeaderRowHeight properties associated with SfTreeGrid.
        /// </summary>
        /// <remarks></remarks>
        internal void EnsureProperties()
        {
            if (this.RowHeight != SfGridBase.rowHeight)
            {
                this.TreeGridPanel.RowHeights.DefaultLineSize = this.RowHeight;
            }
            else if (this.HeaderRowHeight != SfGridBase.headerRowHeight && this.TreeGridPanel.RowCount > 0)
            {
                this.TreeGridPanel.RowHeights.SetRange(0, this.GetHeaderIndex(), this.HeaderRowHeight);
            }
        }
        internal void UpdateColumnCount(bool canGenerateVisibleColumns)
        {
            int columnCount;
            columnCount = this.Columns.Count;
            if (this.showRowHeader)
                columnCount += 1;
            this.TreeGridPanel.ColumnCount = columnCount;
        }

        internal void UpdateFreezePaneColumns()
        {
            if (this.FrozenColumnCount > 0 && this.TreeGridPanel.ColumnCount >= this.ResolveToScrollColumnIndex(this.FrozenColumnCount))
                this.TreeGridPanel.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
            else if (this.showRowHeader && this.TreeGridPanel.ColumnCount > 1)
                this.TreeGridPanel.FrozenColumns = 1;
            else
                this.TreeGridPanel.FrozenColumns = 0;
            this.TreeGridPanel.FooterColumns = 0;
        }

        internal override void OnAllowResisizingColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;
            if (this.TreeGridPanel != null && this.AllowResizingColumns && (bool)e.NewValue)
            {
                this.Columns.ForEach(col =>
                {
                    if (col.IsHidden)
                        this.ColumnResizingController.ProcessResizeStateManager(col);
                });
            }
            else if (this.TreeGridPanel != null)
            {
                this.Columns.ForEach(col => this.ColumnResizingController.ProcessResizeStateManager(col));
            }
        }

        internal override void OnAllowResisizingHiddenColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;
            if (this.TreeGridPanel != null && this.AllowResizingColumns && (bool)e.NewValue)
            {
                this.Columns.ForEach(col =>
                {
                    if (col.IsHidden)
                        this.ColumnResizingController.ProcessResizeStateManager(col);
                });
            }
            else if (this.TreeGridPanel != null)
            {
                this.Columns.ForEach(col => this.ColumnResizingController.ProcessResizeStateManager(col));
            }
        }

        internal override void OnAllowEditingChanged(DependencyPropertyChangedEventArgs args)
        {
            foreach (var column in this.Columns)
            {
                var AllowEditColumn = column.ReadLocalValue(TreeGridColumn.AllowEditingProperty);
                if (AllowEditColumn == DependencyProperty.UnsetValue)
                    column.UpdateBindingBasedOnAllowEditing();
            }

            foreach (var column in this.Columns.OfType<TreeGridCheckBoxColumn>())
                this.RowGenerator.UpdateBinding(column);


            if (!this.AllowEditing && this.SelectionController.CurrentCellManager.HasCurrentCell)
            {
                int columnIndex = this.ResolveToGridVisibleColumnIndex(this.SelectionController.CurrentCellManager.CurrentCell.ColumnIndex);
                if (!this.Columns[columnIndex].AllowEditing && this.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                {
                    this.SelectionController.CurrentCellManager.EndEdit();
                    this.SelectionController.ResetSelectionHandled();
                }
            }
        }

        internal override void OnAutoGenerateColumnsChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.isGridLoaded && this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None)
            {
                this.GenerateColumns();
                this.UpdateColumnSettings();
                this.RefreshColumns();
            }
        }

        internal override void OnCellTemplateSelectorChanged(DependencyPropertyChangedEventArgs e)
        {
            if (this == null) return;
            this.hasCellTemplateSelector = e.NewValue != null;
            if (this.isGridLoaded)
            {
                this.UpdateCellStyles();
            }
        }

        /// <summary>
        /// Occurs before the shortcut menu/context menu opened.
        /// </summary>
        /// <remarks>
        /// To handle or change the shortcut menu being opened through the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridContextMenuEventArgs"/> event argument.
        /// </remarks>
        public event TreeGridContextMenuOpeningEventHandler TreeGridContextMenuOpening;

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridModel"/> instance which manages interaction between SfTreeGrid and <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView"/>
        /// </summary>
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridModel"/> instance .
        /// </value>
        protected internal TreeGridModel TreeGridModel { get; set; }

        internal override void OnAutoGenerateColumnsModeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.isGridLoaded && this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None)
            {
                this.GenerateColumns();
                this.UpdateColumnSettings();
                this.RefreshColumns();
            }
        }

        /// <summary>
        /// Gets or sets the collection that is used to generate the content of the SfTreeGrid.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of the SfTreeGrid.The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSourceChanged"/> event will be raised when the <b>ItemsSource</b> property gets changed.
        /// </remarks>
        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ItemsSource dependency property.
        /// </remarks>    
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(SfTreeGrid), new PropertyMetadata(null, OnItemsSourceChanged));

        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var treeGrid = obj as SfTreeGrid;
            if (!treeGrid.isGridLoaded)
                return;
            treeGrid.RepopulateTree();
            treeGrid.RaiseItemsSourceChanged(args.OldValue, args.NewValue);
        }

        /// <summary>
        /// Repopulate the tree in on demand loading.
        /// </summary>
        /// <remarks>
        /// You can use this event when <see cref="Syncfusion.UI.Xaml.TreeGrid.RequestTreeItems"/> event is used to populate the tree.
        /// </remarks>
        public void RepopulateTree()
        {
            ResetSelectionValues();
            if (TreeGridModel != null)
                TreeGridModel.EndEdit();
            if (View != null)
            {
                View.Dispose();
                View = null;
            }
            Columns.ForEach(column =>
            {
                if (column._columnWrapper != null)
                    column._columnWrapper.DataContext = null;
            });
            SetSourceList(this.ItemsSource);
            RowGenerator.OnItemSourceChanged();
            RefreshHeaderLineCount();
            UpdateRowandColumnCount();
            if (TreeGridPanel != null)
                TreeGridPanel.OnItemSourceChanged();
            if (this.HasView)
                EnsureViewProperties();
        }

        /// <summary>
        /// Occurs when the ItemsSource changed.
        /// </summary>        
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.View"/> can be accessed here.   
        /// </remarks>
        public event TreeGridItemsSourceChangedEventHandler ItemsSourceChanged;

        protected void RaiseItemsSourceChanged(object oldItemsSource, object newItemsSOurce)
        {
            var args = new TreeGridItemsSourceChangedEventArgs(this, oldItemsSource, newItemsSOurce);
            this.RaiseItemsSourceChangedEvent(args);
        }

        internal void RaiseItemsSourceChangedEvent(TreeGridItemsSourceChangedEventArgs args)
        {
            if (this.ItemsSourceChanged != null)
            {
                ItemsSourceChanged(this, args);
            }
        }

        /// <summary>
        /// Method which is used to raise the SelectionChanging Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs">GridSelectionChangingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseSelectionChagingEvent(GridSelectionChangingEventArgs args)
        {
            if (this.SelectionChanging != null)
            {
                SelectionChanging(this, args);
                return args.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Method which is used to raise the selection changed event.
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs">GridSelectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseSelectionChangedEvent(GridSelectionChangedEventArgs args)
        {
            if (this.SelectionChanged != null)
            {
                SelectionChanged(this, args);
            }
        }


        /// <summary>
        /// Helper method to raise Current Cell Changing Event
        /// </summary>
        /// <param name="e"></param>
        internal bool RaiseCurrentCellActivatingEvent(CurrentCellActivatingEventArgs e)
        {
            if (this.CurrentCellActivating != null)
            {
                CurrentCellActivating(this, e);
                return e.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Helper method to raise Current Cell Changed Event
        /// </summary>
        /// <param name="e"></param>
        internal void RaiseCurrentCellActivatedEvent(CurrentCellActivatedEventArgs e)
        {
            if (this.CurrentCellActivated != null)
                CurrentCellActivated(this, e);
        }

        internal void RaiseCurrentCellValueChangedEvent(TreeGridCurrentCellValueChangedEventArgs e)
        {
            if (CurrentCellValueChanged != null)
            {
                CurrentCellValueChanged(this, e);
            }
        }
        #region Validation Events

        /// <summary>
        /// Helper method to raise Current Cell Validating Event
        /// </summary>
        internal bool RaiseCurrentCellValidatingEvent(TreeGridCurrentCellValidatingEventArgs e)
        {
            if (this.CurrentCellValidating != null)
                CurrentCellValidating(this, e);
            return e.IsValid;
        }

        /// <summary>
        /// Helper method to raise Current Cell Validated Event
        /// </summary>
        internal void RaiseCurrentCellValidatedEvent(TreeGridCurrentCellValidatedEventArgs e)
        {
            if (this.CurrentCellValidated != null)
                CurrentCellValidated(this, e);
        }
        /// <summary>
        /// Helper method to raise Row validating event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs">RowValidatingEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal bool RaiseRowValidatingEvent(TreeGridRowValidatingEventArgs e)
        {
            if (this.RowValidating != null)
                RowValidating(this, e);
            return e.IsValid;
        }

        /// <summary>
        ///  Helper method to raise Row validated event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs">RowValidatedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseRowValidatedEvent(TreeGridRowValidatedEventArgs e)
        {
            if (this.RowValidated != null)
                RowValidated(this, e);
        }

        #endregion
        internal void RaiseCurrentCellDropDownSelectionChangedEvent(CurrentCellDropDownSelectionChangedEventArgs e)
        {
            if (CurrentCellDropDownSelectionChanged != null)
                CurrentCellDropDownSelectionChanged(this, e);
        }

        internal bool CurrentCellRequestNavigateEvent(CurrentCellRequestNavigateEventArgs e)
        {
            if (CurrentCellRequestNavigate != null)
            {
                CurrentCellRequestNavigate(this, e);
                return e.Handled;
            }
            return false;
        }

        /// <summary>
        /// Helper method to raise Current Cell Begin Edit Event
        /// </summary>
        /// <param name="e"></param>
        internal bool RaiseCurrentCellBeginEditEvent(TreeGridCurrentCellBeginEditEventArgs e)
        {
            if (this.CurrentCellBeginEdit != null)
            {
                CurrentCellBeginEdit(this, e);
                return e.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Helper method to raise Current Cell End Edit Event
        /// </summary>
        /// <param name="e"></param>
        internal void RaiseCurrentCellEndEditEvent(CurrentCellEndEditEventArgs e)
        {
            if (this.CurrentCellEndEdit != null)
            {
                CurrentCellEndEdit(this, e);
            }
        }

        internal override void RaiseAutoGeneratingEvent(ref GridColumnBase column, ref bool cancel)
        {
            var args = RaiseAutoGeneratingEvent(new TreeGridAutoGeneratingColumnEventArgs(column as TreeGridColumn, this) { Cancel = cancel });
            column = args.Column;
            cancel = args.Cancel;
        }

        internal TreeGridAutoGeneratingColumnEventArgs RaiseAutoGeneratingEvent(TreeGridAutoGeneratingColumnEventArgs args)
        {
            if (AutoGeneratingColumn != null)
                this.AutoGeneratingColumn(this, args);
            return args;
        }

        /// <summary>
        /// Gets or sets a value to control data manipulation operations during data updates.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.LiveNodeUpdateMode"/> that indicates how data manipulation operations are handled during data updates. 
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.LiveNodeUpdateMode.Default"/>.
        /// </value>
        public LiveNodeUpdateMode LiveNodeUpdateMode
        {
            get { return (LiveNodeUpdateMode)GetValue(LiveNodeUpdateModeProperty); }
            set { SetValue(LiveNodeUpdateModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.LiveNodeUpdateMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.LiveNodeUpdateMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty LiveNodeUpdateModeProperty =
            DependencyProperty.Register("LiveNodeUpdateMode", typeof(LiveNodeUpdateMode), typeof(SfTreeGrid), new PropertyMetadata(LiveNodeUpdateMode.Default, OnLiveNodeUpdateModePropertyChanged));

        private static void OnLiveNodeUpdateModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            if (grid.View != null)
                grid.View.LiveNodeUpdateMode = grid.LiveNodeUpdateMode;
        }

        /// <summary>
        /// Gets or sets a value that indicates how nodes should be arranged while changing  <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ChildPropertyName"/> and  <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ParentPropertyName"/> in Self Relational mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.SelfRelationUpdateMode"/> enumeration that specifies how the nodes should be arranged.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.SelfRelationUpdateMode.MoveOnEdit"/> and <see cref="Syncfusion.UI.Xaml.TreeGrid.SelfRelationUpdateMode.MoveOnPropertyChange"/>.
        /// </value>      
        public SelfRelationUpdateMode SelfRelationUpdateMode
        {
            get { return (SelfRelationUpdateMode)GetValue(SelfRelationUpdateModeProperty); }
            set { SetValue(SelfRelationUpdateModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelfRelationUpdateMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelfRelationUpdateMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SelfRelationUpdateModeProperty =
            DependencyProperty.Register("SelfRelationUpdateMode", typeof(SelfRelationUpdateMode), typeof(SfTreeGrid), new PropertyMetadata(SelfRelationUpdateMode.MoveOnEdit | SelfRelationUpdateMode.MoveOnPropertyChange, OnSelfRelationUpdateModePropertyChanged));


        private static void OnSelfRelationUpdateModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            if (grid.HasView)
            {
                var selfRelationalView = grid.View as TreeGridSelfRelationalView;
                selfRelationalView.SelfRelationUpdateMode = grid.SelfRelationUpdateMode;
            }
        }

        /// <summary>
        /// Gets or sets a value that specifies how selection should be processed when node CheckBox is checked or unchecked.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.CheckBoxSelectionMode"/> enumeration which specify how selection should be processed when node CheckBox is checked or unchecked.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.CheckBoxSelectionMode.Default"/>
        /// </value> 
        /// <seealso cref="SfTreeGrid.ShowCheckBox"/>
        public CheckBoxSelectionMode CheckBoxSelectionMode
        {
            get { return (CheckBoxSelectionMode)GetValue(CheckBoxSelectionModeProperty); }
            set { SetValue(CheckBoxSelectionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CheckBoxSelectionMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CheckBoxSelectionMode dependency property.
        /// </remarks>     
        public static readonly DependencyProperty CheckBoxSelectionModeProperty =
            DependencyProperty.Register("CheckBoxSelectionMode", typeof(CheckBoxSelectionMode), typeof(SfTreeGrid), new PropertyMetadata(CheckBoxSelectionMode.Default, OnCheckBoxSelectionModePropertyChanged));


        private static void OnCheckBoxSelectionModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            // WPF-35424 - Before loading the grid if SelectionMode is single and EnableRecursiveChecking is True, exception is thrown. To avoid this, below check is added.
            if (grid.HasView)
                grid.nodeCheckBoxController.ValidateCheckBoxSelectionMode();
            if (grid.SelectionController != null)
            {
                if (grid.SelectionController.CurrentCellManager.HasCurrentCell && grid.HasView && grid.View.IsEditingItem)
                    grid.SelectionController.CurrentCellManager.EndEdit();
                grid.SelectionController.ClearSelections(false);
                grid.nodeCheckBoxController.ResetIsCheckedState(false);
                grid.SelectionController = null;
            }
            grid.InitializeSelectionController();
        }

        internal override void OnSortNumberPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded || this.View == null)
                return;

            if ((bool)e.NewValue)
            {
                if (this.SortColumnDescriptions.Count > 1)
                    this.TreeGridModel.ShowSortNumbers();
            }
            else
                this.TreeGridModel.CollapseSortNumber();
        }

        /// <summary>
        /// Occurs when the column is being sorted in SfTreeGrid.
        /// </summary>       
        /// <remarks>
        /// You can cancel or customize the column being sorted using the <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangingEventArgs"/> event argument.
        /// It will be raised for UI based sorting only.
        /// </remarks>
        public event GridSortColumnsChangingEventHandler SortColumnsChanging;

        /// <summary>
        /// Occurs after the column is sorted in SfTreeGrid.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SortColumnsChanging"/> event if that event is not canceled.        
        /// </remarks> 
        public event GridSortColumnsChangedEventHandler SortColumnsChanged;

        /// <summary>
        /// Gets or sets the value which indicates how the nodes to be expanded while loading.
        /// </summary>
        /// <value>
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.AutoExpandMode.None"/>.
        /// </value>
        public AutoExpandMode AutoExpandMode
        {
            get { return (AutoExpandMode)GetValue(AutoExpandModeProperty); }
            set { SetValue(AutoExpandModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AutoExpandMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AutoExpandMode dependency property.
        /// </remarks>  
        public static readonly DependencyProperty AutoExpandModeProperty =
            DependencyProperty.Register("AutoExpandMode", typeof(AutoExpandMode), typeof(SfTreeGrid), new PropertyMetadata(AutoExpandMode.None));


        /// <summary>
        /// Gets or sets the value which indicates in which case recursive checking should be applied when <see cref="SfTreeGrid.EnableRecuriveChecking"/> is <value>true</value>.
        /// </summary>
        /// <value>
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.RecursiveCheckingMode.Default"/>.
        /// </value>
        public RecursiveCheckingMode RecursiveCheckingMode
        {
            get { return (RecursiveCheckingMode)GetValue(RecursiveCheckingModeProperty); }
            set { SetValue(RecursiveCheckingModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RecursiveCheckingMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RecursiveCheckingMode dependency property.
        /// </remarks>  

        public static readonly DependencyProperty RecursiveCheckingModeProperty =
            DependencyProperty.Register("RecursiveCheckingMode", typeof(RecursiveCheckingMode), typeof(SfTreeGrid), new PropertyMetadata(RecursiveCheckingMode.Default, OnRecursiveCheckingModePropertyChanged));


        private static void OnRecursiveCheckingModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
            if (grid.HasView)
                grid.View.RecursiveCheckingMode = (RecursiveCheckingMode)args.NewValue;
        }

        /// <summary>
        /// Gets or sets a value that specifies the selection and row background indentation in row, when expander column is first column.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.RowIndentMode"/> enumeration the selection and background indentation in row.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.RowIndentMode.Default"/>
        /// </value> 
        public RowIndentMode RowIndentMode
        {
            get { return (RowIndentMode)GetValue(RowIndentModeProperty); }
            set { SetValue(RowIndentModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowIndentMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.RowIndentMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty RowIndentModeProperty =
            DependencyProperty.Register("RowIndentMode", typeof(RowIndentMode), typeof(SfTreeGrid), new PropertyMetadata(RowIndentMode.Default));


        /// <summary>
        /// Gets or sets a value that indicates how nodes should be filtered using <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridView.Filter"/>.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.FilterLevel"/> enumeration that specifies how nodes should be filtered.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.FilterLevel.All"/>.
        /// </value> 
        public FilterLevel FilterLevel
        {
            get { return (FilterLevel)GetValue(FilterLevelProperty); }
            set { SetValue(FilterLevelProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.FilterLevel dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.FilterLevel dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FilterLevelProperty =
            DependencyProperty.Register("FilterLevel", typeof(FilterLevel), typeof(SfTreeGrid), new PropertyMetadata(FilterLevel.All, OnFilterLevelPropertyChanged));

        private static void OnFilterLevelPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
            if (grid.HasView)
            {
                grid.View.FilterLevel = grid.FilterLevel;
                grid.View.ResetFilter();
            }
        }

        internal int headerLineCount = 1;

        internal int HeaderLineCount
        {
            get { return headerLineCount; }
        }

        internal void UpdateRowandColumnCount()
        {
            if (this.TreeGridPanel == null)
                return;

            if (this.View == null)
            {
                if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None && this.Columns.Count <= 0)
                {
                    this.TreeGridPanel.ColumnCount = 0;
                    this.TreeGridPanel.FooterColumns = 0;
                    this.TreeGridPanel.FrozenColumns = 0;
                    this.TreeGridPanel.RowCount = 0;
                    this.TreeGridPanel.FrozenRows = 0;
                }
                else
                {
                    if (this.Columns.Count > 0)
                    {
                        this.TreeGridPanel.RowCount = this.GetHeaderIndex() + 1;
                        this.TreeGridPanel.FrozenRows = 1;
                    }
                    else
                    {
                        this.TreeGridPanel.RowCount = 0;
                        this.TreeGridPanel.FrozenRows = 0;
                    }

                }
                UpdateColumnCount(false);
                this.TreeGridPanel.UpdateScrollBars();
            }
            else
            {
                (TreeGridPanel.ColumnWidths as LineSizeCollection).SuspendUpdates();
                (TreeGridPanel.ColumnWidths as LineSizeCollection).ResetHiddenState();
                UpdateColumnCount(false);
                for (int i = 0; i < Columns.Count; i++)
                {
                    if (Columns[i].Width == 0)
                        Columns[i].IsHidden = true;
                    if (!Columns[i].IsHidden) continue;

                    var index = i;
                    if (ShowRowHeader)
                        index += 1;

                    TreeGridPanel.ColumnWidths.SetHidden(index, index, true);
                }
                this.UpdateIndentColumnWidths();
                UpdateRowCount();
                UpdateFreezePaneColumns();
                (TreeGridPanel.ColumnWidths as LineSizeCollection).ResumeUpdates();
                this.TreeGridPanel.UpdateScrollBars();
            }
            if (this.TreeGridPanel.RowCount > 0)
                this.TreeGridPanel.RowHeights[0] = this.HeaderRowHeight;
        }

        internal void UpdateIndentColumnWidths()
        {
            if (showRowHeader && this.TreeGridPanel.ColumnCount > 0)
                TreeGridPanel.ColumnWidths[0] = this.RowHeaderWidth;
        }

        /// <summary>
        /// Gets or sets the property name for the parent object where <see cref="ItemsSource"/> is used to 
        /// define the items for this tree when a self-relation is determining the tree structure.
        /// </summary>
        /// <value>The parent property name.</value>
        /// <remarks>
        /// Use this property only if you do not want to use the <see cref="RequestTreeItems"/> event to populate
        /// the tree nodes directly. In self relational mode, <see cref="ChildPropertyName"/> and <see cref="ParentPropertyName"/> form the relation. It should be unique.
        /// </remarks>
        public string ParentPropertyName
        {
            get { return (string)GetValue(ParentPropertyNameProperty); }
            set { SetValue(ParentPropertyNameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.ParentPropertyName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.ParentPropertyName dependency property.
        /// </remarks>  
        public static readonly DependencyProperty ParentPropertyNameProperty =
            DependencyProperty.Register("ParentPropertyName", typeof(string), typeof(SfTreeGrid), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Gets or sets the property name for the child object where <see cref="ItemsSource"/> is used to 
        /// define the items for this tree. 
        /// </summary>
        /// <value>The child property name.</value>
        /// <remarks>
        /// Use this property only if you do not want to use the <see cref="RequestTreeItems"/> event to populate
        /// the tree nodes directly. You need to set this property to be the IEnumerable collection of root nodes for this
        /// tree if Nested collection is used. In self relational mode, <see cref="ChildPropertyName"/> and <see cref="ParentPropertyName"/> form the relation.
        /// </remarks>
        public string ChildPropertyName
        {
            get { return (string)GetValue(ChildPropertyNameProperty); }
            set { SetValue(ChildPropertyNameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ChildPropertyName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ChildPropertyName dependency property.
        /// </remarks>  
        public static readonly DependencyProperty ChildPropertyNameProperty =
            DependencyProperty.Register("ChildPropertyName", typeof(string), typeof(SfTreeGrid), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Gets or sets the name of a property in data object associated with node check box.
        /// </summary>
        /// <seealso cref="SfTreeGrid.ShowCheckBox"/>
        public string CheckBoxMappingName
        {
            get { return (string)GetValue(CheckBoxMappingNameProperty); }
            set { SetValue(CheckBoxMappingNameProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CheckBoxMappingName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CheckBoxMappingName dependency property.
        /// </remarks>  
        public static readonly DependencyProperty CheckBoxMappingNameProperty =
            DependencyProperty.Register("CheckBoxMappingName", typeof(string), typeof(SfTreeGrid), new PropertyMetadata(string.Empty));


        /// <summary>
        /// Gets or sets whether to change the width of expander column while expanding or collapsing based on maximum no of levels expanded.
        /// </summary>
        public bool AllowAutoSizingExpanderColumn
        {
            get { return (bool)GetValue(AllowAutoSizingExpanderColumnProperty); }
            set { SetValue(AllowAutoSizingExpanderColumnProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowAutoSizingExpanderColumn dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowAutoSizingExpanderColumn dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowAutoSizingExpanderColumnProperty =
           DependencyProperty.Register("AllowAutoSizingExpanderColumn", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(true));

        [Obsolete("This property is marked as Obsolete, use EnableRecursiveChecking property instead")]
        /// <summary>
        /// Gets or sets a value which specifies whether recursive node checking is enabled or not. If recursive node checking enabled, node check box state will be changed based on its child or parent check box state.
        /// </summary>
        /// <value>
        /// The default value is false.  
        /// </value>
        /// <remarks>
        /// RecursiveChecking behavior can be changed by setting <see cref="SfTreeGrid.RecursiveCheckingMode" property/> .
        /// </remarks>
        public bool EnableRecuriveChecking
        {
#pragma warning disable 0618
            get { return (bool)GetValue(EnableRecuriveCheckingProperty); }
            set { SetValue(EnableRecuriveCheckingProperty, value); }
#pragma warning restore 0618
        }

        [Obsolete("This is marked as Obsolete, use EnableRecursiveCheckingProperty instead")]
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EnableRecuriveChecking dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EnableRecuriveChecking dependency property.
        /// </remarks>  
        public static readonly DependencyProperty EnableRecuriveCheckingProperty =
           DependencyProperty.Register("EnableRecuriveChecking", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false, OnEnableRecursiveCheckingChanged));


        /// <summary>
        /// Gets or sets a value which specifies whether recursive node checking is enabled or not. If recursive node checking enabled, node check box state will be changed based on its child or parent check box state.
        /// </summary>
        /// <value>
        /// The default value is false.  
        /// </value>
        /// <remarks>
        /// RecursiveChecking behavior can be changed by setting <see cref="SfTreeGrid.RecursiveCheckingMode" property/> .
        /// </remarks>
        public bool EnableRecursiveChecking
        {
            get { return (bool)GetValue(EnableRecursiveCheckingProperty); }
            set { SetValue(EnableRecursiveCheckingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EnableRecursiveChecking dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.EnableRecursiveChecking dependency property.
        /// </remarks>  
        public static readonly DependencyProperty EnableRecursiveCheckingProperty =
           DependencyProperty.Register("EnableRecursiveChecking", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false, OnEnableRecursiveCheckingChanged));

        private static void OnEnableRecursiveCheckingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
            if (grid.HasView)
            {
                grid.nodeCheckBoxController.ValidateCheckBoxSelectionMode();
                grid.View.EnableRecursiveChecking = (bool)e.NewValue;
            }
        }

        /// <summary>
        /// Gets or sets a value which specifies IsThreeState property value of Node CheckBox.
        /// </summary>
        /// <value>
        /// The default value is false.
        /// </value>
        /// <remarks>
        /// Node check can be displayed in expander by setting <see cref="SfTreeGrid.ShowCheckBox"/>
        /// </remarks>
        public bool AllowTriStateChecking
        {
            get { return (bool)GetValue(AllowTriStateCheckingProperty); }
            set { SetValue(AllowTriStateCheckingProperty, value); }
        }


        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowTriStateChecking dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowTriStateChecking dependency property.
        /// </remarks>  
        public static readonly DependencyProperty AllowTriStateCheckingProperty =
           DependencyProperty.Register("AllowTriStateChecking", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false));


        /// <summary>
        /// Gets or sets the column name which will be shown as expander column.
        /// </summary>
        public string ExpanderColumn
        {
            get { return (string)GetValue(ExpanderColumnProperty); }
            set { SetValue(ExpanderColumnProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderColumn dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderColumn dependency property.
        /// </remarks>
        public static readonly DependencyProperty ExpanderColumnProperty =
            DependencyProperty.Register("ExpanderColumn", typeof(string), typeof(SfTreeGrid), new PropertyMetadata(string.Empty));

        internal bool IsExpanderColumnValid()
        {
            if (ReadLocalValue(SfTreeGrid.ExpanderColumnProperty) == DependencyProperty.UnsetValue)
                return false;
            var isValid = this.Columns.Any(c => c.MappingName == ExpanderColumn);
            return isValid;
        }


        internal void SetExpanderColumnIndex()
        {
            int columnIndex = 0;
            if (IsExpanderColumnValid())
            {
                var expanderColumn = Columns.FirstOrDefault(c => c.MappingName == ExpanderColumn);
                if (expanderColumn == null)
                {
                    expanderColumnIndex = -1;
                    return;
                }
                columnIndex = Columns.IndexOf(expanderColumn);
            }
            else
            {
                columnIndex = 0;
            }
            var scrollColumnIndex = this.ResolveToScrollColumnIndex(columnIndex);
            expanderColumnIndex = scrollColumnIndex;
        }
        /// <summary>
        /// Gets or sets a value which specifies whether selection should be added while clicking expander for expanding/collapsing the nodes.
        /// </summary>
        /// <value>
        /// The default value is True.
        /// </value>
        public bool AllowSelectionOnExpanderClick
        {
            get { return (bool)GetValue(AllowSelectionOnExpanderClickProperty); }
            set { SetValue(AllowSelectionOnExpanderClickProperty, value); }
        }


        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionOnExpanderClick dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowSelectionOnExpanderClick dependency property.
        /// </remarks>  
        public static readonly DependencyProperty AllowSelectionOnExpanderClickProperty =
           DependencyProperty.Register("AllowSelectionOnExpanderClick", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(true));


        #region AllowDraggingRows

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowDraggingRows dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AllowDraggingRows dependency property.
        /// </remarks>     
        public static readonly DependencyProperty AllowDraggingRowsProperty =
           DependencyProperty.Register("AllowDraggingRows", typeof(bool), typeof(SfTreeGrid), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the user can drag and drop rows.
        /// </summary>
        /// <value>        
        /// <b>true</b>the row can be drag and drop;Otherwise,<b>false</b>.The default value is <b>false</b>.
        /// </value>
        public bool AllowDraggingRows
        {
            get { return (bool)GetValue(AllowDraggingRowsProperty); }
            set { SetValue(AllowDraggingRowsProperty, value); }
        }

        #endregion

        #region ContextMenu
        /// <summary>
        /// Gets or sets the shortcut menu that appears on each expander column cells in SfTreeGrid.
        /// </summary>
        /// <value>
        /// The shortcut menu for each expander column cells in SfTreeGrid. The default value is null.
        /// </value>     
        /// <remarks>
        /// Command bound with MenuFlyoutItem receives <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridNodeContextMenuInfo"/> 
        /// as command parameter which contains the corresponding expander column cells with Child nodes.
        /// </remarks>
        public ContextMenu ExpanderContextMenu
        {
            get { return (ContextMenu)GetValue(ExpanderContextMenuProperty); }
            set { SetValue(ExpanderContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ExpanderContextMenu dependency property.
        /// </remarks> 
        public static readonly DependencyProperty ExpanderContextMenuProperty =
         GridDependencyProperty.Register("ExpanderContextMenu", typeof(ContextMenu), typeof(SfTreeGrid), new GridPropertyMetadata(null));

        #endregion

        /// <summary>
        /// Gets or sets a value that indicates how the column widths are determined.
        /// </summary>
        /// <remarks>
        /// The default behavior of ColumnSizer can be customized through
        /// <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.TreeGridColumnSizer"/> property.
        /// </remarks>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer"/> enumeration that calculate the column
        /// width.
        /// The default value is <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeColumnSizer.None"/>.
        /// </value>
        public TreeColumnSizer ColumnSizer
        {
            get { return (TreeColumnSizer)this.GetValue(ColumnSizerProperty); }
            set { this.SetValue(ColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.ColumnSizer dependency property.
        /// </remarks>          
        public static readonly DependencyProperty ColumnSizerProperty =
            DependencyProperty.Register("ColumnSizer", typeof(TreeColumnSizer), typeof(SfTreeGrid), new PropertyMetadata(TreeColumnSizer.None, OnColumnSizerChanged));


        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.TreeGrid.ITreeGridCopyPaste"/> interface which controls the copy and paste operations in SfTreeGrid.
        /// </summary>
        /// <value>
        /// An instance of class that derives from <see cref="Syncfusion.UI.Xaml.TreeGrid.ITreeGridCopyPaste/> interface.
        /// The default value is null. 
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCutCopyPaste"/> class provides various properties and virtual methods to customize its operations.        
        /// </remarks>
        public TreeGridCutCopyPaste TreeGridCopyPaste
        {
            get { return (TreeGridCutCopyPaste)GetValue(TreeGridCopyPasteProperty); }
            set { SetValue(TreeGridCopyPasteProperty, value); }
        }

        public static readonly DependencyProperty TreeGridCopyPasteProperty =
            DependencyProperty.Register("TreeGridCopyPaste", typeof(TreeGridCutCopyPaste), typeof(SfTreeGrid), new PropertyMetadata(null));


        /// <summary>
        /// Occurs when the selected cells or rows in SfTreeGrid is being copied to clipboard.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the content being copied from a SfTreeGrid through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> event argument.
        /// </remarks>
        public event GridCopyPasteEventHandler CopyContent;
        /// <summary>
        /// Occurs when the clipboard value is being pasted to SfTreeGrid. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the content is being pasted from clipboard to SfTreeGrid through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> event argument.
        /// </remarks>
        public event GridCopyPasteEventHandler PasteContent;
        /// <summary>
        /// Occurs when each cell in the selected cells or rows being copied from SfTreeGrid into clipboard.
        /// </summary>        
        /// <remarks>
        /// You can cancel or customize each cell is being copied from the selected cells or rows through <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/> event argument. 
        /// </remarks>
        public event TreeGridCopyPasteCellEventHandler CopyCellContent;
        /// <summary>
        /// Occurs when each cell is being pasted from clipboard to SfTreeGrid control.
        /// </summary>        
        /// <remarks>
        /// You can cancel or customize each cell is being pasted from the clipboard through <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCopyPasteCellEventArgs"/> event argument. 
        /// </remarks>
        public event TreeGridCopyPasteCellEventHandler PasteCellContent;

        internal GridCopyPasteEventArgs RaisePasteContentEvent(GridCopyPasteEventArgs args)
        {
            if (PasteContent != null)
                this.PasteContent(this, args);
            return args;
        }

        internal GridCopyPasteEventArgs RaiseCopyContentEvent(GridCopyPasteEventArgs args)
        {
            if (CopyContent != null)
                this.CopyContent(this, args);
            return args;
        }

        internal TreeGridCopyPasteCellEventArgs RaiseCopyTreeGridCellContentEvent(TreeGridCopyPasteCellEventArgs args)
        {
            if (CopyCellContent != null)
                this.CopyCellContent(this, args);
            return args;
        }

        internal TreeGridCopyPasteCellEventArgs RaisePasteTreeGridCellContentEvent(TreeGridCopyPasteCellEventArgs args)
        {
            if (PasteCellContent != null)
                this.PasteCellContent(this, args);
            return args;
        }

        /// <summary>
        /// Dependency Call back for ColumnSizer
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnColumnSizerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfTreeGrid;
            if (!grid.isGridLoaded)
                return;
            grid.TreeGridColumnSizer.ResetAutoCalculationforAllColumns();
            grid.TreeGridColumnSizer.Refresh();
        }

        /// <summary>
        /// Generate the columns in SfTreeGrid.
        /// </summary>
        internal void GenerateColumns()
        {
            suspendForColumnPopulation = true;

            if (Columns == null || this.View == null)
                return;
            if (View.IsDynamicBound)
            {
                this.GenerateGridColumnsForDynamic(Columns, View);
                suspendForColumnPopulation = false;
                return;
            }
            if (this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
                this.Columns.Clear();

            var columns = new ObservableCollection<GridColumnBase>();
            foreach (var column in this.Columns)
                columns.Add(column);

            this.GenerateGridColumns(columns, this.View);
            this.Columns.Clear();

            foreach (var column in columns)
                this.Columns.Add(column as TreeGridColumn);

            suspendForColumnPopulation = false;
        }

        internal bool CheckColumnNameinItemProperties(TreeGridColumn column)
        {
            var propertyName = column.MappingName;
            if (this.View != null && this.Columns.Any(col => col.MappingName != null && col.MappingName.Equals(propertyName)))
            {
                var provider = View.GetItemProperties();
                if (provider == null && !(View is TreeGridUnboundView))
                {
                    return false;
                }

                if (View.IsDynamicBound)
                {
                    if (View.Nodes.Count <= 0) return false;
                    var dynObj = View.Nodes[0].Item as IDynamicMetaObjectProvider;
                    if (dynObj != null)
                    {
                        var metaType = dynObj.GetType();
                        var metaData = dynObj.GetMetaObject(System.Linq.Expressions.Expression.Parameter(metaType, metaType.Name));
                        if (!metaData.GetDynamicMemberNames().Contains(propertyName))
                            return false;
                    }
                }
                else if (View is TreeGridUnboundView)
                {
                    if (View.Nodes.Count <= 0) return false;
#if WPF
                    PropertyDescriptorCollection pdc = TypeDescriptor.GetProperties(View.Nodes[0].Item.GetType());
#else
                    PropertyInfoCollection pdc = new PropertyInfoCollection(View.Nodes[0].Item.GetType());
#endif
                    var pinfo = pdc.GetPropertyDescriptor(propertyName);
                    if (pinfo == null)
                        return false;
                }
                else
                {
                    //checks the column defined or not by using GetPropertyDescriptor.
                    var tempProperyDescriptor = provider.GetPropertyDescriptor(propertyName);
                    if (tempProperyDescriptor == null)
                        return false;
                }
            }
            return true;
        }

        private void GenerateGridColumnsForDynamic(TreeGridColumns columns, TreeGridView view)
        {
            // If there is no records in grid, columns could not be extracted. So skipped here
            if (view.SourceCollection != null && view.SourceCollection.AsQueryable().Count() <= 0)
                return;
            //WPF-22024 - Column will not be generated when using dynamic collection at runtime. since view.Records count is zero. 
            //so have proceed this from source collection
            IDynamicMetaObjectProvider dynObj;
            if (view.SourceCollection == null && View != null)
            {
                dynObj = View.Nodes[0].Item as IDynamicMetaObjectProvider;
            }
            else
            {
                var enumerator = view.SourceCollection.GetEnumerator();
                dynObj = (enumerator.MoveNext() && enumerator.Current != null) ? enumerator.Current as IDynamicMetaObjectProvider : null;
            }
            if (dynObj != null)
            {
                var metaType = dynObj.GetType();
                var metaData =
                    dynObj.GetMetaObject(System.Linq.Expressions.Expression.Parameter(metaType, metaType.Name));

                foreach (var prop in metaData.GetDynamicMemberNames())
                {
                    if (DynamicHelper.IsPythonType(prop) || DynamicHelper.IsComplexCollection(dynObj, prop))
                    {
                        continue;
                    }
                    if (columns.Any(c => c.MappingName == prop)) continue;
                    TreeGridColumn column = new TreeGridTextColumn
                    {
                        MappingName = prop,
                        HeaderText = prop
                    };
                    column.IsAutoGenerated = true;
                    var args = RaiseAutoGeneratingEvent(new TreeGridAutoGeneratingColumnEventArgs(column, this));
                    if (!args.Cancel)
                        columns.Add(args.Column);
                }
            }
        }

#if WPF
        internal override PropertyDescriptorCollection GetPropertyInfoCollection(object view)
#else
        internal override PropertyInfoCollection GetPropertyInfoCollection(object view)
#endif
        {
            var propertyCollection = (view as TreeGridView).GetItemProperties();
            if (propertyCollection == null)
            {
                // To generate columns based on Root Node property type when RequestTreeItems event is used.
                if (View.Nodes.RootNodes.Any())
                {
#if WPF
                    propertyCollection = TypeDescriptor.GetProperties(View.Nodes.RootNodes[0].Item.GetType());
#else
                    propertyCollection = new PropertyInfoCollection(View.Nodes.RootNodes[0].Item.GetType());
#endif
                }
                else
                    return null;
            }
            return propertyCollection;
        }

#if WPF
        internal override GridColumnBase CreateNumericColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateNumericColumn(PropertyInfo propertyinfo)
#endif
        {
            var numericColumn = new TreeGridNumericColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
            };
            var canallownull = this.CanAllowNull(propertyinfo);
#if WPF
            if (propertyinfo.PropertyType.IsAssignableFrom(typeof(int)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(int?)))
                numericColumn.NumberDecimalDigits = 0;
            numericColumn.AllowNullValue = canallownull;
#else
            numericColumn.AllowNull = canallownull;
#endif
            return numericColumn;
        }

#if WPF
        internal override GridColumnBase CreateTextColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateTextColumn(PropertyInfo propertyinfo)
#endif
        {
            return new TreeGridTextColumn { MappingName = propertyinfo.Name, HeaderText = propertyinfo.Name };
        }

#if WPF
        internal override GridColumnBase CreateDateTimeColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateDateTimeColumn(PropertyInfo propertyinfo)
#endif
        {
            var column = new TreeGridDateTimeColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                AllowNullValue = this.CanAllowNull(propertyinfo)
            };
            //if (propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTime)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTime?)))
            //    column.AllowNullValue = propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTime?));
            //else
            //    column.AllowNullValue = propertyinfo.PropertyType.IsAssignableFrom(typeof(DateTimeOffset?));
            return column;
        }
#if WPF
        internal override GridColumnBase CreateCheckBoxColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateCheckBoxColumn(PropertyInfo propertyinfo)
#endif
        {
            return new TreeGridCheckBoxColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                IsThreeState = this.CanAllowNull(propertyinfo)
            };
        }
#if WPF
        internal override GridColumnBase CreateComboBoxColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateComboBoxColumn(PropertyInfo propertyinfo)
#endif
        {
            return new TreeGridComboBoxColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                ItemsSource = Enum.GetValues(propertyinfo.PropertyType)
            };
        }
#if WPF
        internal override GridColumnBase CreateHyperLinkColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateHyperLinkColumn(PropertyInfo propertyinfo)
#endif
        {
            return new TreeGridHyperlinkColumn { MappingName = propertyinfo.Name, HeaderText = propertyinfo.Name };
        }
#if WPF
        internal override GridColumnBase CreateTimeSpanColumn(PropertyDescriptor propertyinfo)
        {
            return CreateTextColumn(propertyinfo);
        }
#endif
        internal override void OnCurrentCellBorderThicknessPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;

            foreach (var dataRow in this.RowGenerator.Items)
            {
                if (dataRow.RowType == TreeRowType.HeaderRow)
                    continue;

                foreach (var column in dataRow.VisibleColumns)
                {
                    var columnElement = column.ColumnElement as TreeGridCell;
                    if (columnElement == null) continue;
                    columnElement.CurrentCellBorderThickness = this.CurrentCellBorderThickness;
                }
            }
        }


        internal override void OnCurrentCellBorderBrushPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;

            foreach (var dataRow in this.RowGenerator.Items)
            {
                if (dataRow.RowType == TreeRowType.HeaderRow)
                    continue;

                foreach (var column in dataRow.VisibleColumns)
                {
                    var columnElement = column.ColumnElement as TreeGridCell;
                    if (columnElement == null) continue;
                    columnElement.CurrentCellBorderBrush = this.CurrentCellBorderBrush;
                }
            }
        }

#if WPF

        internal override GridColumnBase CreateCurrencyColumn(PropertyDescriptor propertyinfo)
        {
            return new TreeGridCurrencyColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                AllowNullValue = this.CanAllowNull(propertyinfo)
            };
        }

        internal override GridColumnBase CreateMaskColumn(PropertyDescriptor propertyinfo)
        {
            return new TreeGridMaskColumn
            {
                MappingName = propertyinfo.Name,
                Mask = "(999)999-9999",
                HeaderText = propertyinfo.Name
            };
        }
#endif
        /// <summary>
        /// Resets the child nodes of the particular node and repopulate child nodes.
        /// </summary>
        /// <param name="treeNode">
        /// Specifies the node to be reset.
        /// </param>
        public void ResetNode(TreeNode treeNode)
        {
            if (View == null)
                return;

            View.RemoveChildNodes(treeNode);
            var rowIndex = this.ResolveToRowIndex(treeNode);
            if (treeNode.IsExpanded)
            {
                TreeGridModel.ExpandNode(treeNode, rowIndex, true);
            }
            else
            {
                TreeGridRequestTreeItemsEventArgs e = new TreeGridRequestTreeItemsEventArgs(treeNode, treeNode.Item, true, false);
                View.RequestTreeItems(e);
                // args.ChildItems = null , if we don't provide items it RequestTreeItems event.
                if (e.ChildItems != null && e.ChildItems.AsQueryable().Count() > 0)
                {
                    e.ParentNode.HasChildNodes = true;
                    if (!View.UpdateHasVisibleChildNodesBasedOnChildItems(e.ParentNode, e.ChildItems))
                        e.ParentNode.HasVisibleChildNodes = false;
                }
                else if (treeNode.ChildNodes != null && treeNode.ChildNodes.Any())
                {
                    e.ParentNode.SetHasChildNodes(true);
                }
                else
                {
                    e.ParentNode.SetHasChildNodes(false);
                }
                this.UpdateDataRow(rowIndex);
            }
        }

        /// <summary>
        /// Expands the node in particular rowIndex.
        /// </summary>
        /// <param name="rowIndex">
        /// Specifies the row index which contains the node to be expanded.
        /// </param>
        public void ExpandNode(int rowIndex)
        {
            if (View == null || rowIndex <= 0 || rowIndex >= TreeGridPanel.RowCount)
                return;
            var node = this.GetNodeAtRowIndex(rowIndex);
            TreeGridModel.ExpandNode(node, rowIndex);
        }

        /// <summary>
        /// Expands the particular node in SfTreeGrid.
        /// </summary>
        /// <param name="node">
        /// Specifies which node to be expanded
        /// </param>
        public void ExpandNode(TreeNode node)
        {
            if (View == null)
                return;
            var rowIndex = this.ResolveToRowIndex(node);
            TreeGridModel.ExpandNode(node, rowIndex);
        }

        /// <summary>
        /// Collapses the node in particular rowIndex.
        /// </summary>
        /// <param name="rowIndex">
        /// Specifies the row index which contains the node to be collapsed.
        /// </param>
        public void CollapseNode(int rowIndex)
        {
            if (View == null || rowIndex <= 0 || rowIndex >= TreeGridPanel.RowCount)
                return;
            var node = this.GetNodeAtRowIndex(rowIndex);
            TreeGridModel.CollapseNode(node, rowIndex);
        }

        /// <summary>
        /// Collapses the particular node in SfTreeGrid.
        /// </summary>
        /// <param name="node">
        /// Specifies which node to be collapsed.
        /// </param>
        public void CollapseNode(TreeNode node)
        {
            if (View == null)
                return;
            var rowIndex = this.ResolveToRowIndex(node);
            if (rowIndex <= 0 || rowIndex >= TreeGridPanel.RowCount)
                return;
            TreeGridModel.CollapseNode(node, rowIndex);
        }

        /// <summary>
        /// Expands all the nodes in SfTreeGrid
        /// </summary>
        public void ExpandAllNodes()
        {
            if (View == null)
                return;
            TreeGridModel.ExpandAllNodes();
        }

        /// <summary>
        /// Collapses all the nodes in SfTreeGrid
        /// </summary>
        public void CollapseAllNodes()
        {
            if (View == null)
                return;
            TreeGridModel.CollapseAllNodes();
        }

        /// <summary>
        /// Expands the particular node and all its child nodes in SfTreeGrid.
        /// </summary>
        /// <param name="node">
        /// Specifies which node to be expanded.
        /// </param>
        public void ExpandAllNodes(TreeNode node)
        {
            if (View == null)
                return;
            TreeGridModel.ExpandAllNodes(node, true);
        }

        /// <summary>
        /// Expands all the nodes in SfTreeGrid up to given level.
        /// </summary>
        /// <param name="level">
        /// Specifies how many levels to be expanded
        /// </param>
        public void ExpandAllNodes(int level)
        {
            if (View == null)
                return;
            TreeGridModel.ExpandAllNodes(level);
        }

        /// <summary>
        /// Collapses the particular node and all its child nodes in SfTreeGrid.
        /// </summary>
        /// <param name="node">
        /// Specifies which node to be collapsed.
        /// </param>
        public void CollapseAllNodes(TreeNode node)
        {
            if (View == null)
                return;
            TreeGridModel.CollapseAllNodes(node, true);
        }

        internal void UpdateRowCount()
        {
            this.TreeGridPanel.RowCount = this.View.Nodes.Count + this.HeaderLineCount;
            UpdateFreezePaneRows();
        }

        internal void UpdateFreezePaneRows()
        {
            this.TreeGridPanel.FrozenRows = this.HeaderLineCount;
        }

        /// <summary>
        /// Updates the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.AutoScroller"/> settings .
        /// </summary>
        internal void UpdateAutoScroller()
        {
            this.AutoScroller.TreeGridPanel = this.TreeGridPanel;
            this.AutoScroller.AutoScrollBounds = this.TreeGridPanel.GetClipRect(ScrollAxisRegion.Body, ScrollAxisRegion.Body);
            this.AutoScroller.IntervalTime = new TimeSpan(0, 0, 0, 0, 100);
#if UWP
            this.AutoScroller.InsideScrollMargin = new Size(20, 20);
#else
            this.AutoScroller.InsideScrollMargin = new Size(0, 0);
#endif
        }

        /// <summary>
        /// update cell style for given column index
        /// </summary>
        /// <param name="visibleColumnIndex"></param>
        /// <remarks></remarks>
        private void UpdateColumnCellStyle(int visibleColumnIndex)
        {
            this.RowGenerator.Items.ForEach(row => row.VisibleColumns.ForEach(col =>
            {
                if (col.ColumnIndex == visibleColumnIndex)
                {
                    col.UpdateCellStyle();
                }
            }));
        }

        /// <summary>
        /// Update style for all cells except header cells.
        /// </summary>
        /// <remarks></remarks>
        private void UpdateCellStyles()
        {
            this.RowGenerator.Items.OfType<TreeDataRow>().ForEach(row =>
            {
                if (row.RowIndex != -1)
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle());
            });
        }

        /// <summary>
        /// update row header row style
        /// </summary>
        /// <remarks></remarks>
        private void UpdateHeaderRowStyle()
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType == TreeRowType.HeaderRow)
                {
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle());
                }
            });
        }

        /// <summary>
        /// Gets or sets the value that defines the root object in a self-relational mode when 
        /// <see cref="ItemsSource"/> is used to define the underlying tree data.
        /// </summary>
        public object SelfRelationRootValue
        {
            get { return GetValue(SelfRelationRootValueProperty); }
            set { SetValue(SelfRelationRootValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelfRelationRootValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SelfRelationRootValue dependency property.
        /// </remarks> 
        public static readonly DependencyProperty SelfRelationRootValueProperty =
            DependencyProperty.Register("SelfRelationRootValue", typeof(object), typeof(SfTreeGrid), new PropertyMetadata(null));

        internal override void OnSortColumnDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == e.OldValue || this.TreeGridModel == null)
                return;

            if (e.OldValue is SortColumnDescriptions)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= this.TreeGridModel.OnSortColumnsChanged;

            if (e.NewValue == null)
                return;

            if (e.NewValue is SortColumnDescriptions)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += this.TreeGridModel.OnSortColumnsChanged;
            if (this.isGridLoaded)
                this.TreeGridModel.RefreshAfterSorting(false);
        }

        internal override void OnFrozenColumnCountChanged(DependencyPropertyChangedEventArgs e)
        {
            if ((int)e.NewValue < 0)
                throw new InvalidOperationException(String.Format("FrozenColumnCount {0} doesn't fall with in expected range", FrozenColumnCount));
            if (isGridLoaded && TreeGridPanel.ColumnCount >= this.ResolveToScrollColumnIndex((int)e.NewValue))
            {
                UpdateFreezePaneColumns();
            }
        }

        /// <summary>
        /// Gets the collection of comparers to sort the data based on custom logic .
        /// </summary>
        /// <remarks>
        /// A comparer that are added to <b>SortComparers</b> collection to apply custom Sorting based on the specified column name and sort direction.
        /// </remarks>
        public SortComparers SortComparers
        {
            get { return (SortComparers)GetValue(SortComparersProperty); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SortComparers dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.SortComparers dependency property.
        /// </remarks> 
        public static readonly DependencyProperty SortComparersProperty =
         DependencyProperty.Register("SortComparers", typeof(SortComparers), typeof(SfTreeGrid), new PropertyMetadata(new SortComparers()));


        internal bool RaiseSortColumnsChanging(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action, out bool cancelScroll)
        {
            var args = new GridSortColumnsChangingEventArgs(addedColumns, removedColumns, action, this);
            RaiseSortColumnsChanging(args);
            cancelScroll = args.CancelScroll;
            return !args.Cancel;
        }

        internal void RaiseSortColumnsChanging(GridSortColumnsChangingEventArgs args)
        {
            if (this.SortColumnsChanging != null)
            {
                this.SortColumnsChanging(this, args);
            }
        }

        internal void RaiseSortColumnsChanged(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action)
        {
            var args = new GridSortColumnsChangedEventArgs(addedColumns, removedColumns, action, this);
            RaiseSortColumnsChanged(args);
        }

        internal void RaiseSortColumnsChanged(GridSortColumnsChangedEventArgs args)
        {
            if (this.SortColumnsChanged != null)
            {
                this.SortColumnsChanged(this, args);
            }
        }

        /// <summary>
        /// Helper method to raise Drag and Drop function.
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.QueryColumnDraggingEventArgs">QueryColumnDraggingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseQueryColumnDragging(TreeGridColumnDraggingEventArgs args)
        {
            if (this.ColumnDragging != null)
            {
                this.ColumnDragging(this, args);
            }
            return args.Cancel;
        }

        /// <summary>
        /// Helper method to raise the Resize Columns Event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal bool RaiseResizingColumnsEvent(ResizingColumnsEventArgs args)
        {
            if (ResizingColumns != null)
                this.ResizingColumns(this, args);
            return args.Cancel;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.TreeGridContextMenuOpening"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="e">
        ///  Specifies the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridContextMenuEventArgs"/> that contains the event data.        
        /// </param>
        /// <returns>
        ///  <b>true</b> If the event is handled;Otherwise<b>false</b>
        /// </returns>
        internal bool RaiseTreeGridContextMenuEvent(TreeGridContextMenuEventArgs e)
        {
            if (this.TreeGridContextMenuOpening != null)
                this.TreeGridContextMenuOpening(this, e);
            return e.Handled;
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellToolTipOpening"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellToolTipOpeningEventArgs"/> that contains the event data.        
        /// </param>   
        protected internal void RaiseCellToolTipOpeningEvent(TreeGridCellToolTipOpeningEventArgs args)
        {
            if (CellToolTipOpening != null)
                CellToolTipOpening(this, args);
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellTapped"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellTappedEventArgs"/> that contains the event data.        
        /// </param>   
        protected internal void RaiseCellTappedEvent(TreeGridCellTappedEventArgs args)
        {
            if (CellTapped != null)
                CellTapped(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid.CellDoubleTapped"/> event in SfTreeGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.TreeGrid.TreeGridCellDoubleTappedEventArgs"/> that contains the event data.        
        /// </param>   
        protected internal void RaiseCellDoubleTappedEvent(TreeGridCellDoubleTappedEventArgs args)
        {
            if (CellDoubleTapped != null)
                CellDoubleTapped(this, args);
        }

        internal override bool CanCellTapped()
        {
            return this.CellTapped != null;
        }

        internal override bool CanCellDoubleTapped()
        {
            return this.CellDoubleTapped != null;
        }
        internal override bool CanCellToolTipOpening()
        {
            return this.CellToolTipOpening != null;
        }
        /// <summary>
        /// update style for given column
        /// </summary>
        /// <param name="column"></param>
        /// <remarks></remarks>
        internal void OnColumnStyleChanged(TreeGridColumn column, string property)
        {
            var visibleColumnIndex = this.ResolveToScrollColumnIndex(this.Columns.IndexOf(column));
            switch (property)
            {
                case "HeaderStyle":
                    if (column.HeaderStyle != null)
                    {
                        this.UpdateColumnHeaderStyle(visibleColumnIndex);
                    }
                    break;
                case "HeaderTemplate":
                    if (column.HeaderTemplate != null)
                    {
                        this.UpdateColumnHeaderStyle(visibleColumnIndex);
                    }
                    break;
                case "CellStyle":
                    if (column.ReadLocalValue(TreeGridColumn.CellStyleProperty) != DependencyProperty.UnsetValue)
                    {
                        this.UpdateColumnCellStyle(visibleColumnIndex);
                    }
                    break;

                case "CellStyleSelector":
                    if (column.CellStyleSelector != null)
                    {
                        this.UpdateColumnCellStyle(visibleColumnIndex);
                    }
                    break;

                case "CellTemplate":
                    if (column.IsTemplate && (column as TreeGridTemplateColumn).hasCellTemplate)
                    {
                        this.UpdateColumnCellStyle(visibleColumnIndex);
                    }
                    break;
                case "CellTemplateSelector":
                    if (column.IsTemplate && (column as TreeGridTemplateColumn).hasCellTemplateSelector)
                    {
                        this.UpdateColumnCellStyle(visibleColumnIndex);
                    }
                    break;
            }
        }

        /// <summary>
        /// update column header cell style
        /// </summary>
        /// <param name="visibleColumnIndex"></param>
        /// <remarks></remarks>
        private void UpdateColumnHeaderStyle(int visibleColumnIndex)
        {
            var headerRow = this.RowGenerator.Items.FirstOrDefault(r => r.RowType == TreeRowType.HeaderRow);
            if (headerRow == null)
                return;
            headerRow.VisibleColumns.ForEach(col =>
            {
                if (col.ColumnIndex == visibleColumnIndex)
                {
                    col.UpdateCellStyle();
                }
            });
        }

        /// <summary>
        /// Determines the desired size of the SfTreeGrid.
        /// </summary>
        /// <param name="availableSize">
        /// The size that the SfTreeGrid can occupy.
        /// </param>
        /// <returns>
        /// The desired size of SfTreeGrid. 
        /// </returns>
        protected override Size MeasureOverride(Size constraint)
        {
#if UWP

            if (constraint.Width != 0.0 && constraint.Height != 0.0)
            {
                if (TreeGridPanel != null)
                {
                    TreeGridPanel.ViewPortSize = new Size(constraint.Width - (BorderThickness.Left + BorderThickness.Right),
                                                      constraint.Height);
                }
            }
#endif
            return base.MeasureOverride(constraint);
        }

        /// <summary>
        /// Arranges the content of the SfTreeGrid.
        /// </summary>
        /// <param name="arrangeBounds">
        /// The computed size that is used to arrange the content.
        /// </param>
        /// <returns>
        /// The size consumed by SfTreeGrid.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// Gets or sets the TreeGridPanel to arrange the data in panel.
        /// </summary>
        protected internal TreeGridPanel TreeGridPanel
        {
            get { return _treeGridPanel; }
            set { _treeGridPanel = value; }
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.TreeGrid.SfTreeGrid"/> class.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            this.isGridLoaded = false;
            this.UnWireEvents();
            this.UnWireTreeGridEvents();
            if (this.TreeGridModel != null)
            {
                this.TreeGridModel.Dispose();
                this.TreeGridModel = null;
            }
            if (this.TreeGridPanel != null)
            {
#if UWP
                this.TreeGridPanel.ContainerKeydown = null;
#endif
                this.TreeGridPanel.Dispose();
                this._treeGridPanel = null;
            }
            if (this.CurrentColumn != null)
            {
                this.CurrentColumn = null;
            }

            if (this.RowGenerator != null)
            {
                this.RowGenerator.Dispose();
                this.RowGenerator = null;
            }
            if (this.TreeGridColumnSizer != null)
            {
                this.TreeGridColumnSizer.Dispose();
                this.TreeGridColumnSizer = null;
            }

            if (this.View != null)
            {
                this.View.Dispose();
                this.View = null;
            }
            if (this.cellRenderers != null)
            {
                this.cellRenderers.Dispose();
                this.cellRenderers = null;
            }

            if (this.SelectedItems != null)
            {
                this.SelectedItems.CollectionChanged -= OnSelectedItemsCollectionChanged;
                this.SelectedItems.Clear();
            }

            if (this.Columns != null)
            {
                (this.Columns as INotifyCollectionChanged).CollectionChanged -= OnTreeColumnCollectionChanged;
                this.Columns.Dispose();
                this.Columns = null;
            }

            if (this.SelectionController != null)
            {
                this.SelectionController.Dispose();
                this.SelectionController = null;
            }
            if (this.ColumnDragDropController != null)
            {
                this.ColumnDragDropController.Dispose();
                this.ColumnDragDropController = null;
            }

            if (ColumnResizingController != null)
            {
                ColumnResizingController.Dispose();
                ColumnResizingController = null;
            }

            if (this.ValidationHelper != null)
            {
                this.ValidationHelper.Dispose();
                this.ValidationHelper = null;
            }


            if (this.TreeGridCopyPaste != null)
            {
                this.TreeGridCopyPaste.Dispose();
                this.TreeGridCopyPaste = null;
            }

            if (this.nodeCheckBoxController != null)
            {
                this.nodeCheckBoxController.Dispose();
                this.nodeCheckBoxController = null;
            }

            if (this.SortColumnDescriptions != null)
            {
                this.SortColumnDescriptions.Clear();
                this.ClearValue(SfTreeGrid.SortColumnDescriptionsProperty);
            }

            if (this.SortComparers != null)
            {
                this.SortComparers.Clear();
                this.ClearValue(SfTreeGrid.SortComparersProperty);
            }

#if UWP
            if (this.RowDragDropController != null)
            {
                this.RowDragDropController.Dispose();
                this.RowDragDropController = null;
            }
#endif

            if (this.AutoScroller != null)
            {
                if (this.autoScroller != null)
                {
                    this.autoScroller.Dispose();
                    this.autoScroller = null;
                }
                this.AutoScroller.Dispose();
                this.AutoScroller = null;
            }
        }
    }

    /// <summary>
    ///Provides classes, interfaces and enumerators to create SfTreeGrid, that enable a user to interact with a SfTreeGrid.
    ///The grid classes are allow a user to manipulate the data and performs the SfTreeGrid operations like sorting, editing
    ///drag and drop and resizing in SfTreeGrid.
    /// </summary>
    class NamespaceDoc
    {

    }
}
