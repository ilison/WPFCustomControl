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
using System.Threading.Tasks;
using System.Threading;
namespace Syncfusion.UI.Xaml.Grid
{
#if UWP
    //Using MenuFlyout instead of ContextMenu Control in UWP
    using ContextMenu = Windows.UI.Xaml.Controls.MenuFlyout;
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    [TemplatePart(Name = "PART_ScrollViewer", Type = typeof(VisualContainer))]
#endif
    /// <summary>
    /// Represents a control that displays the data in a tabular format.
    /// </summary>
    /// <remarks>
    /// The SfDataGrid control provides a flexible way to manage data and the set
    /// built-in column types allows the data to be displayed in to appropriate editor.
    /// </remarks>
    [StyleTypedProperty(Property = "CellStyle", StyleTargetType = typeof(GridCell))]
    [StyleTypedProperty(Property = "UnBoundRowCellStyle", StyleTargetType = typeof(GridUnBoundRowCell))]
    [StyleTypedProperty(Property = "RowStyle", StyleTargetType = typeof(VirtualizingCellsControl))]
    [StyleTypedProperty(Property = "UnBoundRowStyle", StyleTargetType = typeof(UnBoundRowControl))]
    [StyleTypedProperty(Property = "HeaderStyle", StyleTargetType = typeof(GridHeaderCellControl))]
    [StyleTypedProperty(Property = "CaptionSummaryRowStyle", StyleTargetType = typeof(CaptionSummaryRowControl))]
    [StyleTypedProperty(Property = "TableSummaryRowStyle", StyleTargetType = typeof(TableSummaryRowControl))]
    [StyleTypedProperty(Property = "GroupSummaryRowStyle", StyleTargetType = typeof(GroupSummaryRowControl))]
    [StyleTypedProperty(Property = "GroupSummaryCellStyle", StyleTargetType = typeof(GridGroupSummaryCell))]
    [StyleTypedProperty(Property = "CaptionSummaryCellStyle", StyleTargetType = typeof(GridCaptionSummaryCell))]
    [StyleTypedProperty(Property = "TableSummaryCellStyle", StyleTargetType = typeof(GridTableSummaryCell))]
    [StyleTypedProperty(Property = "GroupDropAreaStyle", StyleTargetType = typeof(GroupDropArea))]
    [TemplatePart(Name = "PART_GroupDropArea", Type = typeof(GroupDropArea))]
    [TemplatePart(Name = "PART_VisualContainer", Type = typeof(VisualContainer))]
    [StyleTypedProperty(Property = "DetailsViewDataGridStyle", StyleTargetType = typeof(DetailsViewDataGrid))]
    [StyleTypedProperty(Property = "FilterPopupStyle", StyleTargetType = typeof(GridFilterControl))]
    
    public class SfDataGrid : SfGridBase, INotifyDependencyPropertyChanged, IDetailsViewNotifier, IDisposable
    {
        #region Fields
#if WPF        
        protected internal ItemPropertiesProvider Provider { get; set; }
#endif
        internal bool IsInDeserialize = false; // 是否 反序列化
        protected VisualContainer container;  // 视图容器 
        private IGridSelectionController selectionController; // grid选择控制器
        private RowGenerator rowGenerator; // 行生成器
        private GridColumnResizingController columnResizingController;  // 列调整大小控制器
        private GridColumnSizer gridColumnSizer;  // 列尺寸器
#if WPF
        private SearchHelper searchHelper;
#endif
        private GridColumnDragDropController gridColumnDragDropController; // 列拖放控制器
        private GridCellRendererCollection cellRenderers = null; // 单元格渲染器集合
        private GridCellRendererCollection unBoundRowCellRenderers = null; // 未绑定的行 单元格渲染器集合
        private GridCellRendererCollection filterRowCellRenderers = null; // 筛选行 单元格渲染器集合
        private GroupDropArea groupDropArea;  // 拖放组面积
        private ValidationHelper validationHelper; // 验证帮助
        internal CoveredCellInfoCollection coveredCells = null; // 覆盖单元信息集合
        internal int headerLineCount = 1; // 表头行个数
        internal bool isGridLoaded; // 网格是否已加载
        internal bool inRowHeaderChange; // 表头行是否更改
        // UWP-2055 - Expander cell need not to be added in DetailsViewDataGrid when set DetailsViewDefinition for parent DataGrid
        internal bool inDetailsViewIndentChange; // inDetailsView缩进更改
        private bool isViewPropertiesEnsured = false; // 确保视图属性
        internal bool isselectedindexchanged = false; // 所选索引已更改
        internal bool isselecteditemchanged = false; // 选定item已更改
        internal bool isSelectedItemsChanged = false; // 选定items已更改
        internal bool isCurrentItemChanged = false; // 当前item已更改
        private bool isdisposed = false; // 是否回收
#if !WPF
        internal bool IsLoaded = false;
#endif
        internal DetailsViewManager DetailsViewManager; // 详细视图管理
        // Flag used to indicate whether need to notify the NotifyListener (To sort the DetailsViewDataGrids in that level) on Sortcolumndescriptions change when this is SourceDataGrid        
        internal bool suspendNotification; // 暂停通知
        [Cloneable(false)]
        internal ValidationHelper ValidationHelper
        {
            get { return validationHelper; }
            set { validationHelper = value; }
        }

#if WPF
        // 得到字体
        private Typeface GetTypeface()
        {
            FontFamily fontFamily = new FontFamily("Segoe UI");            
            FontStyle fontStyle = new FontStyle();
            FontWeight fontWeight = new FontWeight();
            FontStretch fontStretch = new FontStretch();

            return new Typeface(fontFamily, fontStyle, fontWeight, fontStretch);
        }
        // 使用绘图
        internal bool useDrawing = false;
#endif


#if !WinRT && !UNIVERSAL
        //停止列移动
        internal static bool suspendForColumnMove;     
        // 老的移动下标
        internal int oldIndexForMove;
#endif

#if UWP
        internal static Brush GridSelectionForgroundBrush = new SolidColorBrush(Colors.White);
#else
        // 选择字体色
        internal static Brush GridSelectionForgroundBrush = new SolidColorBrush(Colors.Black);
#endif

        #endregion

        #region Internal Property

        //Flag to indicate, Columns.Reset action in GridModel as we will clear IndentColumns also in Reset
        // 要指示的标志，所有列重置操作在GridModel中，因为我们将在重置中清除缩进列
        internal bool isInColumnReset = false;
        // 组的说明文字
        internal string GroupCaptionConstant = "{ColumnName} : {Key} - {ItemsCount} Items";
        // 有没绑定的列
        internal bool HasUnboundColumns = false;

        //if IsInternalChange is false it does not allow to set the value to SfDataGrid.BindableView 
        // 如果IsInternalChange为false，则不允许将值设置为SfDataGrid.BindableView
        private bool IsInternalChange = false;

        // 内部视图集合
        private ICollectionViewAdv _internalView;

        //WPF-38150:Declare the oldView argument.
        //声明老的视图集合
        private ICollectionViewAdv oldView;

        internal int HeaderLineCount
        {
            get { return headerLineCount; }
        }

        /// <summary>
        /// Return true if the View doesn't Null.
        /// </summary>
        internal bool HasView
        {
            get { return this.View != null; }
        }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator"/> of SfDataGrid.
        /// </summary>
        /// <remarks>
        /// The SfDataGrid will generate the DataRows which need to be display in UI and the rows will be maintained in <see cref="Syncfusion.UI.Xaml.Grid.RowGenerator.Items"/>.
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public RowGenerator RowGenerator
        {
            get
            {
                return rowGenerator;
            }
            set
            {
                if (this.IsLoaded || this.HasView)
                    throw new InvalidOperationException("RowGenerator should not set once the SfDataGrid is loaded");
                if (rowGenerator != null)
                    rowGenerator.Dispose();
                rowGenerator = value;
            }
        }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> which controls the resizing operation in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class.
        /// </value> 
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridColumnResizingController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public GridColumnResizingController ColumnResizingController
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

#if WPF
        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.UI.Xaml.Grid.SearchHelper"/> which controls the search operation in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.SearchHelper"/> class.
        /// </value>
        [Cloneable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public SearchHelper SearchHelper
        {
            get
            {
                return searchHelper;
            }
            set
            {
                if (searchHelper != null)
                    searchHelper.Dispose();
                searchHelper = value;
            }
        }
#endif

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> instance which manages interaction between SfDataGrid and <see cref="Syncfusion.Data.ICollectionViewAdv"/>.
        /// </summary>
        /// <value>
        /// The reference to the <see cref="Syncfusion.UI.Xaml.Grid.GridModel"/> instance .
        /// </value>
        [Cloneable(false)]
        protected internal GridModel GridModel { get; set; }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> which controls columns sizing, column width auto calculation and row height auto calculation. 
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>            
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public GridColumnSizer GridColumnSizer
        {
            get
            {
                if (gridColumnSizer == null)
                    this.gridColumnSizer = new GridColumnSizer(this);
                return gridColumnSizer;
            }
            set
            {
                if (gridColumnSizer != null)
                    gridColumnSizer.Dispose();
                gridColumnSizer = value;
                if (gridColumnSizer != null && gridColumnSizer.GridBase == null)
                    gridColumnSizer.GridBase = this;
            }
        }

        internal bool hasCaptionSummaryRowStyle;
        internal bool hasGroupSummaryRowStyle;
        internal bool hasTableSummaryRowStyle;

        internal bool hasCaptionSummaryRowStyleSelector;
        internal bool hasGroupSummaryRowStyleSelector;
        internal bool hasTableSummaryRowStyleSelector;

        internal bool hasGroupSummaryCellStyle;
        internal bool hasGroupSummaryCellStyleSelector;

        internal bool hasCaptionSummaryCellStyle;
        internal bool hasCaptionSummaryCellStyleSelector;

        internal bool hasTableSummaryCellStyle;
        internal bool hasTableSummaryCellStyleSelector;


        internal bool hasCellStyleSelector;
        internal bool hasCellStyle;
        internal bool hasUnBoundRowCellStyle;
        internal bool hasRowStyleSelector;
        internal bool hasRowStyle;
        internal bool hasUnBoundRowStyle;
        internal bool hasAlternatingRowStyle;

        internal bool hasAlternatingRowStyleSelector = false;
        internal bool hasHeaderTemplate;
        internal bool hasHeaderStyle;
        internal bool isIQueryable = false;

        #endregion

        #region Public Property

#if WPF
        /// <summary>
        /// 获取或设置值，该值有助于在使用LinqtoEn.es时对源中的项进行排序。
        /// Gets or sets the value that helps to sort the items in source while using Linq to Entities.
        /// </summary>
        /// <value>
        /// The key value that used to sort the Linq query when the query is about to process with Skip action.
        /// The default value is null.
        /// </value>
        public string KeyColumn { get; set; }
#endif
        /// <summary>
        /// 获取或设置<see cref="Sync..Data.ICollectionViewAdv"/>的实例，该实例管理SfDataGrid的记录、排序、分组、摘要和筛选。
        /// Gets or sets an instance of the <see cref="Syncfusion.Data.ICollectionViewAdv"/> which manage the records, sorting, grouping, summaries and filtering for SfDataGrid.       
        /// </summary>        
        /// <exception cref="System.InvalidOperationException"> Thrown when you set value for BindableView property explicitly.</exception>
        [Cloneable(false)]
        public ICollectionViewAdv BindableView
        {
            get { return (ICollectionViewAdv)GetValue(BindableViewProperty); }
            set
            {
                if (!IsInternalChange)
                    throw new InvalidOperationException("It is a read only property.You can't set the SfDataGrid BindableView as explicitly");
                SetValue(BindableViewProperty, value);
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.BindableView dependency property.
        /// </summary>        
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.BindableView dependency property.
        /// </remarks>        
        public static readonly DependencyProperty BindableViewProperty =
         GridDependencyProperty.Register("BindableView", typeof(ICollectionViewAdv), typeof(SfDataGrid), new GridPropertyMetadata(null,OnBindableViewChanged));       

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.Data.ICollectionViewAdv"/> instance which manage the records,
        /// sorting, grouping, summaries and filtering in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// View will be created based on ItemsSource you are setting. Below are the list of
        /// CollectionViews available in SfDataGrid. 
        /// <list type="number">
        /// 		<item>
        /// 			<description>DataTableCollectionView</description>
        /// 		</item>
        /// 		<item>
        /// 			<description>VirtualizingCollecitonView</description>
        /// 		</item>
        /// 		<item>
        /// 			<description>PagedCollectionView</description>
        /// 		</item>
        /// 		<item>
        /// 			<description>QueryableCollectionView</description>
        /// 		</item>
        /// 	</list>
        /// </remarks>
        [Cloneable(false)]
        public ICollectionViewAdv View
        {
            get
            {
                return _internalView;
            }
            internal set
            {
                _internalView = value;
                IsInternalChange = true;
                BindableView = value;
                IsInternalChange = false;
            }
        }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> which controls the column drag-and-drop operation in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.GridColumnDragDropController"/> class provides various properties and virtual methods to customize its operations.
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public GridColumnDragDropController GridColumnDragDropController
        {
            get
            {
                return gridColumnDragDropController;
            }
            set
            {
                if (gridColumnDragDropController != null)
                    gridColumnDragDropController.Dispose();
                gridColumnDragDropController = value;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the busy indicator should be displayed while fetching the large amount data in <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCollectionView"/>.
        /// </summary>
        /// <value>
        /// <b>true</b> if the busy indicator is enabled; otherwise , <b>false</b>. The default value is <b>true</b>.
        /// </value>
        public bool ShowBusyIndicator { get; set; }

        internal VisualContainer VisualContainer
        {
            get { return container; }
        }

        internal GroupDropArea GroupDropArea
        {
            get { return groupDropArea; }
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererCollection"/> instance which
        /// holds the collection of all predefined cell renderers <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridVirtualizingCellRenderer"/> .
        /// </summary>
        /// <remarks>
        /// <para>The cell renderers provides various properties and virtual methods to
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
        /// 			<term>GridCellTextBoxRenderer</term>
        /// 			<description>TextBox</description>
        /// 			<description>GridTextColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellNumericRenderer</term>
        /// 			<description>Numeric</description>
        /// 			<description>GridNumericColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellCheckBoxRenderer</term>
        /// 			<description>CheckBox</description>
        /// 			<description>GridCheckBoxColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellTemplateRenderer</term>
        /// 			<description>Template</description>
        /// 			<description>GridTemplateColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellImageRenderer</term>
        /// 			<description>Image</description>
        /// 			<description>GridImageColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridUnBoundCellTextBoxRenderer</term>
        /// 			<description>UnBoundTextColumn</description>
        /// 			<description>GridUnBoundColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridUnBoundCellTemplateRenderer</term>
        /// 			<description>UnBoundTemplateColumn</description>
        /// 			<description>GridUnBoundColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellComboBoxRenderer</term>
        /// 			<description>ComboBox</description>
        /// 			<description>GridComboBoxColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellDateTimeRenderer</term>
        /// 			<description>DateTime</description>
        /// 			<description>GridDateTimeColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellHyperLinkRenderer</term>
        /// 			<description>HyperLink</description>
        /// 			<description>GridHyperLinkColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellMaskRenderer</term>
        /// 			<description>Mask</description>
        /// 			<description>GridMaskColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellPercentageRenderer</term>
        /// 			<description>Percent</description>
        /// 			<description>GridPercentColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellCurrencyRenderer</term>
        /// 			<description>Currency</description>
        /// 			<description>GridCurrencyColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellMultiColumnDropDownRenderer</term>
        /// 			<description>MultiColumnDropDown</description>
        /// 			<description>GridMultiColumnDropDownList</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellTimeSpanRenderer</term>
        /// 			<description>TimeSpan</description>
        /// 			<description>GridTimeSpanColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridTableSummaryCellRenderer</term>
        /// 			<description>TableSummary</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCaptionSummaryCellRenderer</term>
        /// 			<description>CaptionSummary</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridSummaryCellRenderer</term>
        /// 			<description>GroupSummary</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridDataHeaderCellRenderer</term>
        /// 			<description>Header</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridStackedHeaderCellRenderer</term>
        /// 			<description>StackedHeader</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridCellTextBlockRenderer</term>
        /// 			<description>TextBlock</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridRowHeaderCellRenderer</term>
        /// 			<description>RowHeader</description>
        /// 			<description>-</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridDetailsViewExpanderCellRenderer</term>
        /// 			<description>DetailsViewExpander</description>
        /// 			<description>-</description>
        /// 		</item>        		
        /// 	</list>
        /// </remarks>
        /// <example>
        /// 	<code lang="C#"><![CDATA[
        /// //The customized GridCellTextBoxRendererExt is replaced to CellRenderers collection after removed the default renderer of GridTextColumn.
        /// this.dataGrid.CellRenderers.Remove("TextBox");
        /// this.dataGrid.CellRenderers.Add("TextBox",new GridCellTextBoxRendererExt());
        /// ]]></code>
        /// </example>
        public GridCellRendererCollection CellRenderers
        {
            get { return cellRenderers; }
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererCollection"/> instance which
        /// holds the renderer's for UnBoundRow Cell.
        /// </summary>
        /// <remarks>
        /// <para>The <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridUnBoundCellTextBoxRenderer"/> class provides various
        /// properties and virtual methods to customize its operations.The customized
        /// <b>GridUnBoundRowCellTextBoxRenderer</b> should be replaced to
        /// <b>UnBoundRowCellRenderers</b>. The below table shows the predefined renderers
        /// that associated with UnBoundRowCell and its corresponding cell type. </para> 	
        /// 	<list type="table">
        ///         <listheader>
        /// 			<term>Renderer Class</term>
        /// 			<description>Cell Type</description>        			
        /// 		</listheader>
        /// 		<item>
        /// 			<term>GridUnBoundRowCellTemplateRenderer</term>
        /// 			<description>UnBoundTemplateColumn</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridUnBoundRowCellTextBoxRenderer</term>
        /// 			<description>UnBoundTextColumn </description>
        /// 		</item>
        /// 	</list>
        /// </remarks>
        public GridCellRendererCollection UnBoundRowCellRenderers
        {
            get { return unBoundRowCellRenderers; }
        }

        /// <summary>
        /// Gets the reference to the <see cref="Syncfusion.UI.Xaml.Grid.Cells.GridCellRendererCollection"/> instance which
        /// holds the collection of all predefined cell renderers <see cref="Syncfusion.UI.Xaml.Grid.RowFilter.GridFilterRowCellRenderer"/>.
        /// </summary>
        /// <remarks>
        /// 	<para>The cell renderers provides various properties and virtual methods to
        /// customize its operations. When any of the predefined renderer is customized ,
        /// that should be replaced to the <b>FilterRowCellRenderers</b> collection with its
        /// appropriate renderer name. The below table shows the predefined renderers and its
        /// corresponding name. </para>
        /// 	<list type="table">
        /// 		<listheader>
        /// 			<term>Renderer Class</term>
        /// 			<description>Renderer Name</description>
        /// 		</listheader>
        /// 		<item>
        /// 			<term>GridFilterRowTextBoxRenderer</term>
        /// 			<description>TextBox</description>       
        /// 		</item>
        /// 		<item>
        /// 			<term>GridFilterRowNumericRenderer</term>
        /// 			<description>Numeric</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridFilterRowCheckBoxRenderer</term>
        /// 			<description>CheckBox</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridFilterRowComboBoxRenderer</term>
        /// 			<description>ComboBox</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridFilterRowComboBoxRenderer</term>
        /// 			<description>MultiSelectDropDown</description>
        /// 		</item>
        /// 		<item>
        /// 			<term>GridFilterRowDateTimeRenderer</term>
        /// 			<description>DateTime</description>
        /// 		</item>  		
        /// 	</list>
        /// </remarks>
        public GridCellRendererCollection FilterRowCellRenderers
        {
            get { return filterRowCellRenderers; }
        }

        /// <summary>
        /// Gets the list of <see cref="Syncfusion.UI.Xaml.Grid.CoveredCellInfo"/> collection which are queried using <see cref="Syncfusion.UI.Xaml.Grid.QueryConveredRange"/> event for the visible rows and columns.
        /// </summary>
        /// <value>
        /// The list of <see cref="Syncfusion.UI.Xaml.Grid.CoveredCellInfo"/> collection.
        /// </value>
        [Cloneable(false)]
        public CoveredCellInfoCollection CoveredCells
        {
            get
            {
                if (coveredCells == null)
                    return coveredCells = new CoveredCellInfoCollection(this);
                return coveredCells;
            }
        }

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.MergedCellManger"/> which controls the merging in SfDataGrid using <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/>  event.
        /// </summary>
        /// <value>
        /// The instance of the <see cref="Syncfusion.UI.Xaml.Grid.MergedCellManger"/>.
        /// </value>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public MergedCellManager MergedCellManager
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.UI.Xaml.Grid.IGridSelectionController"/> which controls selection operations in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.IGridSelectionController"/>.
        /// </value>
        /// <remarks>
        /// The default behavior of row or cell selection can be customized by assigning the class derived from <see cref="Syncfusion.UI.Xaml.Grid.GridCellSelectionController"/> or <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionController"/> to <b>SelectionController</b> property. 
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public IGridSelectionController SelectionController
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
                //WPF-31421 Need to update the SelectionController for VisibleColumns while setting the SelectionController
                if (this.RowGenerator != null && value != null)
                    this.RowGenerator.UpdateSelectionController();
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether to listen <see cref="System.ComponentModel.INotifyPropertyChanging.PropertyChanging"/> and <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> events of data object and <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> event of <see cref="Syncfusion.Data.CollectionViewAdv.SoureCollection"/>.
        /// </summary>     
        /// <remarks>
        /// By default, view listens to <see cref="System.ComponentModel.INotifyPropertyChanging.PropertyChanging"/> and <see cref="System.ComponentModel.INotifyPropertyChanged.PropertyChanged"/> events of data object and <see cref="System.Collections.Specialized.INotifyCollectionChanged.CollectionChanged"/> event of <see cref="Syncfusion.Data.CollectionViewAdv.SoureCollection"/>.
        /// </remarks>
        public NotificationSubscriptionMode NotificationSubscriptionMode
        {
            get { return (NotificationSubscriptionMode)GetValue(NotificationSubscriptionModeProperty); }
            set { SetValue(NotificationSubscriptionModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.NotificationMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.NotificationMode dependency property.
        /// </remarks>  
        public static readonly DependencyProperty NotificationSubscriptionModeProperty =
            DependencyProperty.Register("NotificationSubscriptionMode ", typeof(NotificationSubscriptionMode), typeof(SfDataGrid), new PropertyMetadata(NotificationSubscriptionMode.CollectionChange | NotificationSubscriptionMode.PropertyChange, OnNotificationSubscriptionModeChanged));


        private SerializationController serializationController;
        /// <summary>
        /// Gets or sets an instance of <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/>
        /// which controls the serialization operation in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/> class provides
        /// various properties and virtual methods to customize its operations.
        /// </remarks>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.SerializationController"/> class.
        /// </value>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public SerializationController SerializationController
        {
            get
            {
                return serializationController;
            }
            set
            {
                if (serializationController != null)
                    serializationController.Dispose();
                serializationController = value;
            }
        }

        private AutoScroller autoScroller;

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller"/> to perform horizontal or vertical scrolling automatically , 
        /// when the selection is dragged outside of the visible boundaries in SfDataGrid.
        /// </summary> 
        /// <value>
        /// An instance of the <see cref="Syncfusion.UI.Xaml.Grid.AutoScroller"/>.
        /// </value>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        //自动滚动
        public AutoScroller AutoScroller
        {
            get
            {
                if (this.autoScroller == null)
                {
                    this.autoScroller = new AutoScroller();
                }

                return this.autoScroller;
            }
            set
            {
                this.autoScroller = value;
            }
        }

        #endregion

        #region Dependency property        
        
        

        /// <summary>
        /// Gets or sets the number of non-scrolling columns at right of the SfDataGrid.
        /// </summary>
        /// <value>
        /// The number of non-scrolling columns at right. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Footer columns are always displayed and it can't be scrolled out of visibility.
        /// </remarks>
        [Cloneable(false)]
        public int FooterColumnCount
        {
            get { return (int)GetValue(FooterColumnCountProperty); }
            set { SetValue(FooterColumnCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FooterColumnCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FooterColumnCount dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FooterColumnCountProperty =
            GridDependencyProperty.Register("FooterColumnCount", typeof(int), typeof(SfDataGrid), new GridPropertyMetadata(0, OnFooterColumnCountPropertyChanged));
        
        /// <summary>
        /// Gets or sets the number of non-scrolling rows at bottom of the SfDataGrid.
        /// </summary>
        /// <value>
        /// The number of non-scrolling rows at bottom. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Footer rows are always displayed and it can’t be scrolled out of visibility.
        /// </remarks>
        [Cloneable(false)]
        public int FooterRowsCount
        {
            get { return (int)GetValue(FooterRowsCountProperty); }
            set { SetValue(FooterRowsCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FooterRowsCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FooterRowsCount dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FooterRowsCountProperty =
            GridDependencyProperty.Register("FooterRowsCount", typeof(int), typeof(SfDataGrid), new GridPropertyMetadata(0, OnFooterRowsCountPropertyChanged));

        /// <summary>
        /// Gets or sets the number of non-scrolling rows at top of the SfDataGrid.
        /// </summary>
        /// <value>
        /// The number of non-scrolling rows at top. The default value is <b>zero</b>.
        /// </value>
        /// <remarks>
        /// Frozen rows are always displayed and it cannot be scrolled out of visibility.
        /// </remarks>
        [Cloneable(false)]
        public int FrozenRowsCount
        {
            get { return (int)GetValue(FrozenRowsCountProperty); }
            set { SetValue(FrozenRowsCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FrozenRowsCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FrozenRowsCount dependency property.
        /// </remarks>       
        public static readonly DependencyProperty FrozenRowsCountProperty =
            GridDependencyProperty.Register("FrozenRowsCount", typeof(int), typeof(SfDataGrid), new GridPropertyMetadata(0, OnFrozenRowsCountPropertyChanged));

        /// <summary>
        /// Gets or sets the collection that is used to generate the content of the SfDataGrid.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of the SfDataGrid.The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSourceChanged"/> event will be raised when the <b>ItemsSource</b> property gets changed.
        /// </remarks>
        [Cloneable(false)]
        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSource dependency property.
        /// </remarks>           
        public static readonly DependencyProperty ItemsSourceProperty =
            GridDependencyProperty.Register("ItemsSource", typeof(object), typeof(SfDataGrid), new GridPropertyMetadata(null, OnItemsSourceChanged));

        /// <summary>
        /// Gets or sets the type of data object displayed in SfDataGrid. 
        /// </summary>
        /// <remarks>
        /// Used to specify the type of data object for column population and data (sorting,grouping and filtering) when your data object have multilevel inheritance.
        /// </remarks>        
        public Type SourceType
        {
            get { return (Type)GetValue(SourceTypeProperty); }
            set { SetValue(SourceTypeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SourceType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SourceType dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SourceTypeProperty = GridDependencyProperty.Register("SourceType", typeof(Type), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the value that indicates whether to enable Parallel LINQ while sorting, filtering, grouping and summary calculation to improve performance.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Parallel LINQ is enabled; otherwise,<b>false</b>. The default value is <b>false</b>. 
        /// </value>              
        public bool UsePLINQ
        {
            get { return (bool)GetValue(UsePLINQProperty); }
            set { SetValue(UsePLINQProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.UsePLINQ dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.UsePLINQ dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UsePLINQProperty =
            GridDependencyProperty.Register("UsePLINQ", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));


#if WPF                
        /// <summary>
        /// Gets or sets the value that indicates whether to load light weight template for GridCells to improve loading and scrolling performance.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.UseDrawing"/> enumeration that specifies how the grid cells are rendered.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.UseDrawing.None"/>.
        /// </value>   
        public Nullable<UseDrawing> UseDrawing
        {
            get { return (Nullable<UseDrawing>)GetValue(UseDrawingProperty); }
            set { SetValue(UseDrawingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.UseDrawing dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.UseDrawing dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UseDrawingProperty =
            GridDependencyProperty.Register("UseDrawing", typeof(Nullable<UseDrawing>), typeof(SfDataGrid), new GridPropertyMetadata(null, OnUseDrawingPropertyChanged));

        private static void OnUseDrawingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;            
            grid.useDrawing = e.NewValue != null;            

            if (grid.IsLoaded && e.OldValue == null)
                throw new NotSupportedException("Cannot change the rendering of SfDataGrid at run time");
                        
        }

#endif
        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRow"/> to add additional headers to group and display the column headers. 
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.StackedHeaderRow"/>. The default value is <b>null</b>.
        /// </value>
        [Cloneable(false)]
        public StackedHeaderRows StackedHeaderRows
        {
            get { return (StackedHeaderRows)GetValue(StackedHeaderRowsProperty); }
            set { SetValue(StackedHeaderRowsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.StackedHeaderRows dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.StackedHeaderRows dependency property.
        /// </remarks>        
        public static readonly DependencyProperty StackedHeaderRowsProperty =
            GridDependencyProperty.Register("StackedHeaderRows", typeof(StackedHeaderRows), typeof(SfDataGrid), new GridPropertyMetadata(null, OnStackedHeaderRowsPropertyChanged));

        /// <summary>
        /// Gets the collection of comparers to sort the data based on custom logic .
        /// </summary>
        /// <remarks>
        /// A comparer that are added to <b>SortComparers</b> collection to apply custom Sorting based on the specified column name and sort direction.
        /// </remarks>
        [Cloneable(false)]
        public SortComparers SortComparers
        {
            get { return (SortComparers)GetValue(SortComparersProperty); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortComparers dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SortComparers dependency property.
        /// </remarks>        
        public static readonly DependencyProperty SortComparersProperty =
            GridDependencyProperty.Register("SortComparers", typeof(SortComparers), typeof(SfDataGrid), new GridPropertyMetadata(new SortComparers()));

        /// <summary>
        /// Gets or sets the collection that contains all the columns in SfDataGrid.
        /// </summary>      
        /// <value>
        /// The collection that contains all the columns in SfDataGrid. This property
        /// has no default value.
        /// </value>  
        /// <remarks>
        /// Each column associated with its own renderer and it controls the corresponding column related operations.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellRenderers"/>
        /// <exception cref="System.NullReferenceException">Thrown when the Columns value is set as null.</exception>
        [Cloneable(false)]
        public Columns Columns
        {
            get
            {
#if UWP
                var _columns = (Columns)GetValue(ColumnsProperty);
                if (_columns == null)
                {
                    _columns = new Columns();
                    SetValue(ColumnsProperty, _columns);
                }
                return _columns;
#else
                return (Columns)GetValue(ColumnsProperty);
#endif
            }
            set { SetValue(ColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.Columns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.Columns dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ColumnsProperty =
#if WPF
            GridDependencyProperty.Register("Columns", typeof(Columns), typeof(SfDataGrid), new GridPropertyMetadata(new Columns(), OnColumnsPropertyChanged));
#else
            GridDependencyProperty.Register("Columns", typeof(Columns), typeof(SfDataGrid), new GridPropertyMetadata(null, OnColumnsPropertyChanged));
#endif

        private static void OnColumnsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.IsInDeserialize)
                return;

            if (e.OldValue != e.NewValue)
            {
                if (e.OldValue is Columns)
                    (e.OldValue as INotifyCollectionChanged).CollectionChanged -= grid.OnGridColumnCollectionChanged;
                if (e.NewValue is Columns)
                    (e.NewValue as INotifyCollectionChanged).CollectionChanged += grid.OnGridColumnCollectionChanged;

                if (grid.isViewPropertiesEnsured)
                    grid.EnsureColumnSettings();
                grid.UpdateRowAndColumnCount(false);
                //UWP-2124 Set isDirty is true, In EnsureColumns to update the column even if column index is match in existing visible columns
                if (grid.GridModel != null)
                    grid.RowGenerator.Items.ForEach(item => { item.isDirty = true; });
                if (grid.VisualContainer != null)
                    grid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        private static void OnListenNotificationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = obj as SfDataGrid;
            if ((bool)args.NewValue)
                dataGrid.NotificationSubscriptionMode = NotificationSubscriptionMode.CollectionChange | NotificationSubscriptionMode.PropertyChange;
            else
                dataGrid.NotificationSubscriptionMode = NotificationSubscriptionMode.None;

        }

        private static void OnNotificationSubscriptionModeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = obj as SfDataGrid;
            if (!dataGrid.isGridLoaded)
                return;
            if (dataGrid.View != null)
                throw new NotSupportedException("Cannot change the NotificationSubscriptionMode if view is already created");
        }
        internal override void OnSelectedItemsPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is INotifyCollectionChanged)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= this.OnSelectedItemsChanged;
            if (e.NewValue is INotifyCollectionChanged)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += this.OnSelectedItemsChanged;

            //WPF-38165:Un commented the below code due to SelectedItems not setting in Grid while we binding using SelectedItems property.
            //WPF-26003- Row will be get selection automatically while we scroll the Vertically 
            if (this == null || !this.isGridLoaded || this.View == null)
            {
                this.isSelectedItemsChanged = true;
                return;
            }
            this.isSelectedItemsChanged = false;

            //WPF-38165:SelectedItems not refresh while we setting through SelectedItems property at runtime
            this.SelectionController.HandleCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.SelectedItems, 0), CollectionChangedReason.SelectedItemsCollection);
        }
        internal override void OnAllowSelectionOnPointerPressed(DependencyPropertyChangedEventArgs args)
        {
            if (this.DetailsViewManager.HasDetailsView)
            {
                foreach (var detailsview in this.DetailsViewDefinition)
                {
                    var detailsViewGrid = (detailsview as GridViewDefinition).DataGrid;
                    detailsViewGrid.AllowSelectionOnPointerPressed = (bool)args.NewValue;
                }
            }
        }

        /// <summary>
        /// Gets or sets a brush that highlights the background of the currently selected row or cell.
        /// </summary> 
        /// <value>
        /// The brush that highlights the background of the selected row or cell.
        /// </value>
        public Brush RowSelectionBrush
        {
            get { return (Brush)GetValue(RowSelectionBrushProperty); }
            set { SetValue(RowSelectionBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowSelectionBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowSelectionBrush dependency property.
        /// </remarks>            
        public static readonly DependencyProperty RowSelectionBrushProperty =
            GridDependencyProperty.Register("RowSelectionBrush", typeof(Brush), typeof(SfDataGrid), new GridPropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 128, 128, 128)), OnRowSelectionBrushPropertyChanged));

        private static void OnRowSelectionBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as SfDataGrid;
            if (!dataGrid.isGridLoaded)
                return;

            foreach (var dataRow in dataGrid.RowGenerator.Items)
            {
                if (dataRow.RowType == Grid.RowType.HeaderRow || dataRow.RowType == Grid.RowType.DetailsViewRow || dataRow.WholeRowElement == null)
                    continue;

#if WPF
                if (dataGrid.useDrawing)
                    dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                dataRow.WholeRowElement.RowSelectionBrush = dataGrid.RowSelectionBrush;
                foreach (var column in dataRow.VisibleColumns)
                {
                    var columnElement = column.ColumnElement as GridCell;
                    if (columnElement == null)
                        continue;

                    columnElement.CellSelectionBrush = dataGrid.RowSelectionBrush;
                }
            }
        }

        /// <summary>
        /// Gets or sets a brush that highlights the foreground of currently selected row or cell.
        /// </summary>
        /// <value>
        /// The brush that highlights the foreground of selected row or cell.The default value is Black.
        /// </value>
        public Brush SelectionForegroundBrush
        {
            get { return (Brush)GetValue(SelectionForegroundBrushProperty); }
            set { SetValue(SelectionForegroundBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionForegroundBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionForegroundBrush dependency property.
        /// </remarks>    
        public static readonly DependencyProperty SelectionForegroundBrushProperty =
            GridDependencyProperty.Register("SelectionForegroundBrush", typeof(Brush), typeof(SfDataGrid), new GridPropertyMetadata(GridSelectionForgroundBrush, OnSelectionForegroundBrushChanged));

        private static void OnSelectionForegroundBrushChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = obj as SfDataGrid;
            if (!dataGrid.isGridLoaded)
                return;
            foreach (var dataRow in dataGrid.RowGenerator.Items)
            {
                if (dataRow.IsSelectedRow && dataRow.WholeRowElement.SelectionForegroundBrush == SfDataGrid.GridSelectionForgroundBrush)
                    dataRow.WholeRowElement.Foreground = dataGrid.SelectionForegroundBrush;
                if (dataGrid.SelectionUnit == GridSelectionUnit.Cell || dataGrid.SelectionUnit == GridSelectionUnit.Any)
                {

#if WPF
                    if (dataGrid.useDrawing && dataRow.RowType != Grid.RowType.HeaderRow && dataRow.RowType != Grid.RowType.DetailsViewRow && dataRow.WholeRowElement != null)
                        dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                    foreach (var column in dataRow.VisibleColumns)
                    {
                        if (column.ColumnElement is GridCell)
                        {
                            var ch = (column.ColumnElement as GridCell).Foreground;
                            var ch1 = (column.ColumnElement as GridCell).SelectionForegroundBrush;
                            if (!column.IsSelectedColumn)
                                continue;
                            if ((column.ColumnElement as GridCell).SelectionForegroundBrush == SfDataGrid.GridSelectionForgroundBrush)
                                (column.ColumnElement as GridCell).Foreground = dataGrid.SelectionForegroundBrush;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Gets or sets a brush that highlights the background of currently selected group caption and group summary rows.
        /// </summary>    
        /// <value>
        /// The brush that highlights the background of currently selected group row.
        /// </value>
        public Brush GroupRowSelectionBrush
        {
            get { return (Brush)GetValue(GroupRowSelectionBrushProperty); }
            set { SetValue(GroupRowSelectionBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupRowSelectionBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupRowSelectionBrush dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupRowSelectionBrushProperty =
            GridDependencyProperty.Register("GroupRowSelectionBrush", typeof(Brush), typeof(SfDataGrid), new GridPropertyMetadata(new SolidColorBrush(Color.FromArgb(100, 120, 120, 120)), OnGroupRowSelectionBrushPropertyChanged));

        private static void OnGroupRowSelectionBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as SfDataGrid;

            if (!dataGrid.isGridLoaded)
                return;


            foreach (var dataRow in dataGrid.RowGenerator.Items)
            {
                if (dataRow.RowType == RowType.HeaderRow || dataRow.RowType == Grid.RowType.DetailsViewRow || dataRow.WholeRowElement == null)
                    continue;
#if WPF
                if (dataGrid.useDrawing)
                    dataRow.WholeRowElement.ItemsPanel.InvalidateVisual();
#endif
                dataRow.WholeRowElement.GroupRowSelectionBrush = dataGrid.GroupRowSelectionBrush;
            }
        }

        /// <summary>
        /// Gets or sets the style applied to all the data rows in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all data rows in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/>.
        /// </remarks>
        public Style RowStyle
        {
            get { return (Style)GetValue(RowStyleProperty); }
            set { SetValue(RowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty RowStyleProperty =
            GridDependencyProperty.Register("RowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnRowStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to all the UnBoundRows in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the UnBoundRows in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a UnBoundRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.UnBoundRowControl"/>.
        /// </remarks>
        public Style UnBoundRowStyle
        {
            get { return (Style)GetValue(UnBoundRowStyleProperty); }
            set { SetValue(UnBoundRowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UnBoundRowStyleProperty =
            GridDependencyProperty.Register("UnBoundRowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnUnBoundRowStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to data row conditionally based on data in SfDataGrid.        
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to data row based on data. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowStyle"/>
        public StyleSelector RowStyleSelector
        {
            get { return (StyleSelector)GetValue(RowStyleSelectorProperty); }
            set { SetValue(RowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty RowStyleSelectorProperty =
            GridDependencyProperty.Register("RowStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnRowStyleSelectorChanged));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternationCount dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternationCount dependency property.
        /// </remarks>         
        public static readonly DependencyProperty AlternationCountProperty =
            DependencyProperty.Register("AlternationCount", typeof(int), typeof(SfDataGrid), new PropertyMetadata(2, OnAlternationCountChanged));

        private static void OnAlternationCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || !grid.isGridLoaded)
                return;
            grid.UpdateRowStyle();
        }

        /// <summary>
        /// Gets or sets the number of alternate data rows to have a unique appearance.
        /// </summary>
        /// <value>
        /// The number of alternate data rows in SfDataGrid. The default value is 2.
        /// </value>        
        public int AlternationCount
        {
            get { return (int)GetValue(AlternationCountProperty); }
            set { SetValue(AlternationCountProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternatingRowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternatingRowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty AlternatingRowStyleProperty =
            GridDependencyProperty.Register("AlternatingRowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnAlternatingRowStyleChanged));

        private static void OnAlternatingRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasAlternatingRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }
        /// <summary>
        /// Gets or sets the style applied to alternate data row in SfDataGrid.
        /// </summary>    
        /// <value>
        /// The style that is applied to each alternate data row in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for alternate row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/>.
        /// AlternateRowStyle will be applied based on <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternationCount"/>.
        /// </remarks>        
        public Style AlternatingRowStyle
        {
            get { return (Style)GetValue(AlternatingRowStyleProperty); }
            set { SetValue(AlternatingRowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternatingRowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternatingRowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty AlternatingRowStyleSelectorProperty =
            DependencyProperty.Register("AlternatingRowStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new PropertyMetadata(null, OnAlternatingRowStyleSelectorChanged));

        private static void OnAlternatingRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasAlternatingRowStyleSelector = e.NewValue != null; 
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }
        /// <summary>
        /// Gets or sets the style applied to alternate data row conditionally based on data in SfDataGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to alternate row based on data. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for alternate row, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.VirtualizingCellsControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AlternatingRowStyle"/>
        public StyleSelector AlternatingRowStyleSelector
        {
            get { return (StyleSelector)GetValue(AlternatingRowStyleSelectorProperty); }
            set { SetValue(AlternatingRowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Gets or sets the style applied to all the record cells in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the record cells in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridCell"/>.
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
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CellStyleProperty =
            GridDependencyProperty.Register("CellStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCellStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to all the cells in UnBoundRow.
        /// </summary>
        /// <value>
        /// The style that is applied to all the cells in UnBoundRow. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> to cell in UnBoundRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRowCell"/>.
        /// </remarks>
        public Style UnBoundRowCellStyle
        {
            get { return (Style)GetValue(UnBoundRowCellStyleProperty); }
            set
            {
                SetValue(UnBoundRowCellStyleProperty, value);
                if (this.ReadLocalValue(UnBoundRowCellStyleProperty) != DependencyProperty.UnsetValue)
                {
                    this.hasUnBoundRowCellStyle = true;
                    this.UpdateUnBoundRowCellStyle();
                }

            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRowCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRowCellStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UnBoundRowCellStyleProperty =
            GridDependencyProperty.Register("UnBoundRowCellStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnUnBoundRowCellStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to record cells conditionally based on data in SfDataGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to cell based on data. The default value is <b>null</b>.
        /// </value>  
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCell"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellStyle"/>
        public StyleSelector CellStyleSelector
        {
            get { return (StyleSelector)GetValue(CellStyleSelectorProperty); }
            set { SetValue(CellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CellStyleSelectorProperty =
            GridDependencyProperty.Register("CellStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCellStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to all the header cells in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the header cells in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for header cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridHeaderCellControl"/>.        
        /// </remarks>
        public Style HeaderStyle
        {
            get
            {
                return (Style)this.GetValue(SfDataGrid.HeaderStyleProperty);
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
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.HeaderStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.HeaderStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty HeaderStyleProperty =
            GridDependencyProperty.Register("HeaderStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnHeaderStyleChanged));

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the header cell in SfDataGrid.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the header cell in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.HeaderTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.HeaderTemplate dependency property.
        /// </remarks>        
        public static readonly DependencyProperty HeaderTemplateProperty =
            GridDependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(SfDataGrid), new GridPropertyMetadata(null, OnHeaderTemplateChanged));


        // For setting  SortColumnDescriptions from GroupColumnDescriptions(when GroupColumnDescriptions is cleared), need to maintain groupColumnDescriptionsCopy
        // groupColumnDescriptionsCopy will be set for SourceDataGrid only
        internal GroupColumnDescriptions groupColumnDescriptionsCopy = null;

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescription"/> object that describes how the column to be grouped in to view .
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GroupColumnDescription"/> object. The default value is null.
        /// </value>        
        [Cloneable(false)]
        public GroupColumnDescriptions GroupColumnDescriptions
        {
            get { return (GroupColumnDescriptions)GetValue(GroupColumnDescriptionsProperty); }
            set { SetValue(GroupColumnDescriptionsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupColumnDescriptions dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupColumnDescriptions dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupColumnDescriptionsProperty =
            GridDependencyProperty.Register("GroupColumnDescriptions", typeof(GroupColumnDescriptions), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupColumnDescriptionsPropertyChanged));


        private static void OnGroupColumnDescriptionsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;

            if (grid.IsInDeserialize)
                return;

            // From root DataGrid, need to set GroupColumnDescriptions for other DetailsViewDataGrids
            if (grid.IsSourceDataGrid)
            {
                var sortDescriptions = grid.GridModel.GetSortColumnDescriptionNotInGroupColumnDescription();
                grid.SortColumnDescriptions.Clear();
                foreach (var desc in sortDescriptions)
                    grid.SortColumnDescriptions.Add(desc);
                if (e.NewValue is GroupColumnDescriptions)
                {
                    grid.groupColumnDescriptionsCopy = new GroupColumnDescriptions();
                    foreach (var group in e.NewValue as GroupColumnDescriptions)
                    {
                        grid.groupColumnDescriptionsCopy.Add(new GroupColumnDescription() { ColumnName = group.ColumnName, Converter = group.Converter });
                    }
                }
                // Update SortColumnDescriptions
                foreach (var groupColumnDescription in grid.GroupColumnDescriptions)
                {
                    grid.SortColumnDescriptions.Insert(grid.GroupColumnDescriptions.IndexOf(groupColumnDescription), new SortColumnDescription() { SortDirection = ListSortDirection.Ascending, ColumnName = groupColumnDescription.ColumnName });
                }

                if (grid.NotifyListener != null)
                {
                    grid.NotifyListener.NotifyCollectionPropertyChanged(grid, typeof(GroupColumnDescriptions));
                }
            }

            if (e.NewValue == e.OldValue || grid.GridModel == null)
                return;
            if (e.OldValue != null && e.OldValue is GroupColumnDescriptions)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= grid.GridModel.OnGroupColumnDescriptionsChanged;
            if (e.NewValue == null)
                return;
            if (e.NewValue is GroupColumnDescriptions)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += grid.GridModel.OnGroupColumnDescriptionsChanged;


            if (grid.View == null)
                return;
            grid.GridModel.Suspend();
            var sortDescs = grid.GridModel.GetSortDescriptionNotInGroupDescription();
            var groupedColumns = grid.View.GroupDescriptions.ToList();
            grid.View.SortDescriptions.Clear();
            grid.View.GroupDescriptions.Clear();
            foreach (var desc in sortDescs)
                grid.View.SortDescriptions.Add(desc);
            grid.GridModel.RemoveAllGroupDropItems();
            if (!grid.ShowColumnWhenGrouped)
            {
                foreach (var item in groupedColumns)
                {
                    var groupedColumn = grid.Columns.FirstOrDefault(col => col.MappingName == ((ColumnGroupDescription)item).PropertyName);
                    if (groupedColumn != null)
                        groupedColumn.IsHidden = false;
                }
            }
            if (grid.GroupColumnDescriptions != null && grid.GroupColumnDescriptions.Any())
            {
                grid.View.BeginInit(false);
                // To update View.SortDescription and View.GroupDescription 
                grid.InitialGroup();
                grid.View.EndInit();
            }
            grid.GridModel.Resume();
            // To add grouped columns in GroupDropArea
            grid.GridModel.InitializeGrouping();
            grid.GridModel.ResetColumns(true);
        }

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays summary information at the footer of each group.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> to display the summary information at footer of each group.The default value is null.
        /// </value>
        /// <remarks>
        /// Each group can have more than one group summary row and the summary values is calculated over all the records with in the group.
        /// </remarks>
        [Cloneable(false)]
        public ObservableCollection<GridSummaryRow> GroupSummaryRows
        {
            get { return (ObservableCollection<GridSummaryRow>)GetValue(GroupSummaryRowsProperty); }
            set { SetValue(GroupSummaryRowsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRows dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRows dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupSummaryRowsProperty =
            GridDependencyProperty.Register("GroupSummaryRows", typeof(ObservableCollection<GridSummaryRow>), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupSummaryRowsPropertyChanged));

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays the summary information at the header of each group .
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> to display the summary information at the header of each group .The default value is null.
        /// </value>
        /// <remarks>
        /// Each group can have only one caption summary and the summary value is calculated over all the records with in the group.
        /// </remarks>
        public GridSummaryRow CaptionSummaryRow
        {
            get { return (GridSummaryRow)GetValue(CaptionSummaryRowProperty); }
            set { SetValue(CaptionSummaryRowProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRow dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRow dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CaptionSummaryRowProperty =
            GridDependencyProperty.Register("CaptionSummaryRow", typeof(GridSummaryRow), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCaptionSummaryRowChanged));

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> that displays the summary information either at top or bottom of SfDataGrid.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow"/> to display the summary information either at top or bottom of SfDataGrid. The default value is null.
        /// </value>
        /// <remarks>
        /// The table summary can have more than one summary rows and the summary value calculated overall the records in SfDataGrid.
        /// </remarks>
        [Cloneable(false)]
        public ObservableCollection<GridSummaryRow> TableSummaryRows
        {
            get { return (ObservableCollection<GridSummaryRow>)GetValue(TableSummaryRowsProperty); }
            set { SetValue(TableSummaryRowsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRows dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRows dependency property.
        /// </remarks>         
        public static readonly DependencyProperty TableSummaryRowsProperty =
            GridDependencyProperty.Register("TableSummaryRows", typeof(ObservableCollection<GridSummaryRow>), typeof(SfDataGrid), new GridPropertyMetadata(null, OnTableSummaryRowsPropertyChanged));

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> which denotes the count and position of additional rows at top and bottom of SfDataGrid. These additional rows are not bound to data source of SfDataGrid.
        /// </summary>
        /// <value>
        /// The collection of <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> to add additional rows to display the custom information in SfDataGrid. The default value is null.
        /// </value>
        /// <remarks>
        /// Populate unbound rows data by handling <see cref="Syncfusion.UI.Xaml.Grid.QueryUnBoundRow"/> event.
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public UnBoundRows UnBoundRows
        {
            get { return (UnBoundRows)GetValue(UnBoundRowsProperty); }
            set { SetValue(UnBoundRowsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRows dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.UnBoundRows dependency property.
        /// </remarks>         
        public static readonly DependencyProperty UnBoundRowsProperty =
            GridDependencyProperty.Register("UnBoundRows", typeof(UnBoundRows), typeof(SfDataGrid), new GridPropertyMetadata(null, OnUnBoundRowsPropertyChanged));

        /// <summary>
        /// Gets or sets the custom group comparer that sorts the group based on its summary values.
        /// </summary>
        /// <value>
        /// The custom group comparer to sort the group based on its summary values. The default value is null.
        /// </value>        
        public IComparer<Group> SummaryGroupComparer
        {
            get { return (IComparer<Group>)GetValue(SummaryGroupComparerProperty); }
            set { SetValue(SummaryGroupComparerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SummaryGroupComparer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SummaryGroupComparer dependency property.
        /// </remarks>          
        public static readonly DependencyProperty SummaryGroupComparerProperty =
            GridDependencyProperty.Register("SummaryGroupComparer", typeof(IComparer<Group>), typeof(SfDataGrid), new GridPropertyMetadata(null, OnSummaryGroupComparerChanged));

        /// <summary>
        /// Gets or sets a value that indicates how the column widths are determined.
        /// </summary>
        /// <remarks>
        /// The default behavior of ColumnSizer can be customized through
        /// <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridColumnSizer"/> property.
        /// </remarks>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/> enumeration that calculate the column
        /// width.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType.None"/>.
        /// </value>
        public GridLengthUnitType ColumnSizer
        {
            get { return (GridLengthUnitType)this.GetValue(ColumnSizerProperty); }
            set { this.SetValue(ColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ColumnSizer dependency property.
        /// </remarks>          
        public static readonly DependencyProperty ColumnSizerProperty =
            GridDependencyProperty.Register("ColumnSizer", typeof(GridLengthUnitType), typeof(SfDataGrid), new GridPropertyMetadata(GridLengthUnitType.None, OnColumnSizerChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the column is displayed on SfDataGrid after it is grouped.
        /// </summary>
        /// <value>
        /// <b>true</b> if the column is displayed in SfDataGrid ; otherwise ,<b>false</b>.The default value is <b>true</b>.
        /// </value>       
        public bool ShowColumnWhenGrouped
        {
            get { return (bool)GetValue(ShowColumnWhenGroupedProperty); }
            set { SetValue(ShowColumnWhenGroupedProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowColumnWhenGrouped dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowColumnWhenGrouped dependency property.
        /// </remarks>          
        public static readonly DependencyProperty ShowColumnWhenGroupedProperty =
            GridDependencyProperty.Register("ShowColumnWhenGrouped", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the group header remains fixed at the width of display area or scrolled(horizontal scrolling) out of its visibility.
        /// </summary>
        /// <value>
        /// <b>true</b>, if the group headers is fixed; otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool AllowFrozenGroupHeaders
        {
            get { return (bool)GetValue(AllowFrozenGroupHeadersProperty); }
            set { SetValue(AllowFrozenGroupHeadersProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowFrozenGroupHeaders dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowFrozenGroupHeaders dependency property.
        /// </remarks>          
        public static readonly DependencyProperty AllowFrozenGroupHeadersProperty =
            GridDependencyProperty.Register("AllowFrozenGroupHeaders", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false, OnAllowFixedGroupCaptionsChanged));

        /// <summary>
        /// Gets or sets the style applied to all the caption summary rows in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the caption summary rows in SfDataGrid .The default value is <b>null</b>. 
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a CaptionSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.CaptionSummaryRowControl"/>.
        /// </remarks>
        public Style CaptionSummaryRowStyle
        {
            get { return (Style)GetValue(CaptionSummaryRowStyleProperty); }
            set { SetValue(CaptionSummaryRowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRowStyle dependency property.
        /// </remarks>          
        public static readonly DependencyProperty CaptionSummaryRowStyleProperty =
            GridDependencyProperty.Register("CaptionSummaryRowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCaptionSummaryRowStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to all the group summary rows in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the group summary rows in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a GroupSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GroupSummaryRowControl"/>.
        /// </remarks>
        public Style GroupSummaryRowStyle
        {
            get { return (Style)GetValue(GroupSummaryRowStyleProperty); }
            set { SetValue(GroupSummaryRowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupSummaryRowStyleProperty =
            GridDependencyProperty.Register("GroupSummaryRowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupSummaryRowStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to all the table summary rows in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the table summary rows in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a TableSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowControl"/>.
        /// </remarks>
        public Style TableSummaryRowStyle
        {
            get { return (Style)GetValue(TableSummaryRowStyleProperty); }
            set { SetValue(TableSummaryRowStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRowStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRowStyle dependency property.
        /// </remarks>         
        public static readonly DependencyProperty TableSummaryRowStyleProperty =
            GridDependencyProperty.Register("TableSummaryRowStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnTableSummaryRowStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to caption summary row conditionally based on summary value in SfDataGrid.        
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to caption summary row based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a CaptionSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.CaptionSummaryRowControl "/>.
        /// </remarks>        
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRowStyle"/>
        public StyleSelector CaptionSummaryRowStyleSelector
        {
            get { return (StyleSelector)GetValue(CaptionSummaryRowStyleSelectorProperty); }
            set { SetValue(CaptionSummaryRowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryRowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CaptionSummaryRowStyleSelectorProperty =
            GridDependencyProperty.Register("CaptionSummaryRowStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCaptionSummaryRowStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to group summary row conditionally based on summary value in SfDataGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to group summary row based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a GroupSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GroupSummaryRowControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRowStyle"/>
        public StyleSelector GroupSummaryRowStyleSelector
        {
            get { return (StyleSelector)GetValue(GroupSummaryRowStyleSelectorProperty); }
            set { SetValue(GroupSummaryRowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryRowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupSummaryRowStyleSelectorProperty =
            GridDependencyProperty.Register("GroupSummaryRowStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupSummaryRowStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to table summary row conditionally based on summary value in SfDataGrid.        
        /// </summary>        
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to table summary row based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a TableSummaryRow, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.TableSummaryRowControl"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRowStyle"/>
        public StyleSelector TableSummaryRowStyleSelector
        {
            get { return (StyleSelector)GetValue(TableSummaryRowStyleSelectorProperty); }
            set { SetValue(TableSummaryRowStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRowStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryRowStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty TableSummaryRowStyleSelectorProperty =
            GridDependencyProperty.Register("TableSummaryRowStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnTableSummaryRowStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to group summary cell conditionally based on summary value in SfDataGrid.        
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to group summary cell based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a group summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridGroupSummaryCell"/>.
        /// </remarks>       
        /// <seealso cref="Syncfuion.UI.Xaml.Grid.SfDataGrid.GroupSummaryCellStyle"/>
        public StyleSelector GroupSummaryCellStyleSelector
        {
            get { return (StyleSelector)GetValue(GroupSummaryCellStyleSelectorProperty); }
            set { SetValue(GroupSummaryCellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryCellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryCellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty GroupSummaryCellStyleSelectorProperty =
            GridDependencyProperty.Register("GroupSummaryCellStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupSummaryCellStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to caption summary cell conditionally based on summary value in SfDataGrid.
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to caption summary cell based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a caption summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridCaptionSummaryCell"/>.
        /// </remarks>
        /// <seealso cref="CaptionSummaryCellStyle"/>
        public StyleSelector CaptionSummaryCellStyleSelector
        {
            get { return (StyleSelector)GetValue(CaptionSummaryCellStyleSelectorProperty); }
            set { SetValue(CaptionSummaryCellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryCellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryCellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CaptionSummaryCellStyleSelectorProperty =
            GridDependencyProperty.Register("CaptionSummaryCellStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCaptionSummaryCellStyleSelectorChanged));

        /// <summary>
        /// Gets or sets the style applied to table summary cell conditionally based on summary value in SfDataGrid.        
        /// </summary>
        /// <value>
        /// A <see cref="System.Windows.Controls.StyleSelector"/> object that chooses the style to table summary cell based on summary value. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a table summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridTableSummaryCell"/>.
        /// </remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryCellStyle"/>
        public StyleSelector TableSummaryCellStyleSelector
        {
            get { return (StyleSelector)GetValue(TableSummaryCellStyleSelectorProperty); }
            set { SetValue(TableSummaryCellStyleSelectorProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryCellStyleSelector dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryCellStyleSelector dependency property.
        /// </remarks>         
        public static readonly DependencyProperty TableSummaryCellStyleSelectorProperty =
            GridDependencyProperty.Register("TableSummaryCellStyleSelector", typeof(StyleSelector), typeof(SfDataGrid), new GridPropertyMetadata(null, OnTableSummaryCellStyleSelectorChanged));

        /// <summary>
        /// Gets or sets a value that denotes whether the underlying data object type is dynamic.
        /// </summary>
        /// <value>
        /// <b>true</b> if the underlying data object type is dynamic; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>       
        public bool IsDynamicItemsSource
        {
            get { return (bool)GetValue(IsDynamicItemsSourceProperty); }
            set { SetValue(IsDynamicItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.IsDynamicItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.IsDynamicItemsSource dependency property.
        /// </remarks>         
        public static readonly DependencyProperty IsDynamicItemsSourceProperty =
            GridDependencyProperty.Register("IsDynamicItemsSource", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the amount of data to fetch for virtualizing operations.
        /// </summary>
        /// <value>
        /// The amount of data fetched for virtualizing operations.The default value is 5.
        /// </value>
        public int DataFetchSize
        {
            get { return (int)GetValue(DataFetchSizeProperty); }
            set { SetValue(DataFetchSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.DataFetchSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.DataFetchSize dependency property.
        /// </remarks>         
        public static readonly DependencyProperty DataFetchSizeProperty =
            GridDependencyProperty.Register("DataFetchSize", typeof(int), typeof(SfDataGrid), new GridPropertyMetadata(5));

        /// <summary>
        /// Gets or sets the style applied all the group summary cells in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the group summary cells in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a group summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridGroupSummaryCell"/>.
        /// </remarks>
        public Style GroupSummaryCellStyle
        {
            get { return (Style)GetValue(GroupSummaryCellStyleProperty); }
            set { SetValue(GroupSummaryCellStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryCellStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupSummaryCellStyleProperty =
            GridDependencyProperty.Register("GroupSummaryCellStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupSummaryCellStyleChanged));

        /// <summary>
        /// Gets or sets the style applied to all the caption summary cells in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the caption summary cells in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a caption summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridCaptionSummaryCell"/>.
        /// </remarks>
        public Style CaptionSummaryCellStyle
        {
            get { return (Style)GetValue(CaptionSummaryCellStyleProperty); }
            set { SetValue(CaptionSummaryCellStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CaptionSummaryCellStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty CaptionSummaryCellStyleProperty =
            GridDependencyProperty.Register("CaptionSummaryCellStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnCaptionSummaryCellStyleChanged));


        /// <summary>
        /// Gets or sets the style applied all the table summary cells in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the table summary cells in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a table summary cell, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridTableSummaryCell"/>.
        /// </remarks>
        public Style TableSummaryCellStyle
        {
            get { return (Style)GetValue(TableSummaryCellStyleProperty); }
            set { SetValue(TableSummaryCellStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryCellStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryCellStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty TableSummaryCellStyleProperty =
            GridDependencyProperty.Register("TableSummaryCellStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnTableSummaryCellStyleChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the <see cref="Syncfusio.UI.Xaml.Grid.GroupDropArea"/> is enabled in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the group drop area is enabled in SfDataGrid; otherwise, <b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// To make the GroupDropArea always to be expanded through the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.IsGroupDropAreaExpanded"/> property.
        /// </remarks>
        [Cloneable(false)]
        public bool ShowGroupDropArea
        {
            get { return (bool)GetValue(ShowGroupDropAreaProperty); }
            set { SetValue(ShowGroupDropAreaProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowGroupDropArea dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ShowGroupDropArea dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ShowGroupDropAreaProperty =
            GridDependencyProperty.Register("ShowGroupDropArea", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets an instance of the <see cref="Syncfusion.UI.Xaml.Grid.IGridCopyPaste"/> interface which controls the copy and paste operations in SfDataGrid.
        /// </summary>
        /// <value>
        /// An instance of class that derives from <see cref="Syncfusion.UI.Xaml.Grid.IGridCopyPaste"/> interface.
        /// The default value is null. 
        /// </value>
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridCutCopyPaste"/> class provides various properties and virtual methods to customize its operations.        
        /// </remarks>
        [Cloneable(false)]
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public IGridCopyPaste GridCopyPaste
        {
            get { return (IGridCopyPaste)GetValue(GridCopyPasteProperty); }
            set { SetValue(GridCopyPasteProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyPaste dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GridCopyPaste dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GridCopyPasteProperty =
            GridDependencyProperty.Register("GridCopyPaste", typeof(IGridCopyPaste), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the format of group caption text.
        /// </summary>
        /// <value>
        /// A string that represents the format of group caption text.The default value is null.
        /// </value>
#if !WinRT && !UNIVERSAL
        [TypeConverter(typeof(GridSummaryFormatConverter))]
#endif
        public string GroupCaptionTextFormat
        {
            get { return (string)GetValue(GroupCaptionTextFormatProperty); }
            set
            {
#if !WPF
                if (value != null)
                {
                    var formattedValue = value.SummaryFormatedString();
                    SetValue(GroupCaptionTextFormatProperty, formattedValue);
                }
                else
                    SetValue(GroupCaptionTextFormatProperty, null);
#else
                SetValue(GroupCaptionTextFormatProperty, value);
#endif
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCaptionTextFormat dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCaptionTextFormat dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupCaptionTextFormatProperty =
            GridDependencyProperty.Register("GroupCaptionTextFormat", typeof(string), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupCaptionTextFormatPropertyChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the GroupDropArea can be expanded by default.
        /// </summary>
        /// <value>
        /// <b>true</b> if the GroupDropArea is expanded; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool IsGroupDropAreaExpanded
        {
            get { return (bool)GetValue(IsGroupDropAreaExpandedProperty); }
            set { SetValue(IsGroupDropAreaExpandedProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.IsGroupDropAreaExpanded dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.IsGroupDropAreaExpanded dependency property.
        /// </remarks>        
        public static readonly DependencyProperty IsGroupDropAreaExpandedProperty =
            GridDependencyProperty.Register("IsGroupDropAreaExpanded", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false, IsGroupDropAreaExpandedPropertyChanged));

        /// <summary>
        /// Gets or sets a value indicates whether the user can drag and drop the column to GroupDropArea.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can drag and drop the column to GroupDropArea; otherwise, <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool AllowGrouping
        {
            get { return (bool)GetValue(AllowGroupingProperty); }
            set { SetValue(AllowGroupingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowGrouping dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowGrouping dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowGroupingProperty =
            GridDependencyProperty.Register("AllowGrouping", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(true));

         /// <summary>
         /// Gets or sets a value that indicates the position of new record which is added using AddNewRow.
         /// </summary>
         /// <value>
         /// One of the <see cref="System.ComponentModel.NewItemPlaceholderPosition"/>  enumeration that specifies position of new record in collection.
         /// The default value is <see cref="System.ComponentModel.NewItemPlaceholderPosition.None"/>.
         /// </value>
         /// <remarks>
         /// This <see cref="System.ComponentModel.NewItemPlaceholderPosition"/> property won't work for grouping case.
         /// </remarks>
         public NewItemPlaceholderPosition NewItemPlaceholderPosition
         {
             get { return (NewItemPlaceholderPosition)GetValue(NewItemPlaceholderPositionProperty); }
             set { SetValue(NewItemPlaceholderPositionProperty, value); }
         }
 
         /// <summary>
         /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.NewItemPlaceholderPosition dependency property.
         /// </summary>
         /// <remarks>
         /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.NewItemPlaceholderPosition dependency property.
         /// </remarks>        
         public static readonly DependencyProperty NewItemPlaceholderPositionProperty =
            GridDependencyProperty.Register("NewItemPlaceholderPosition", typeof(NewItemPlaceholderPosition), typeof(SfDataGrid), new GridPropertyMetadata(NewItemPlaceholderPosition.None, OnNewItemPlaceholderPositionChanged));
      
        /// <summary>
        /// Gets or sets a value indicates whether to notify details view DataGrid or child data DataGrid events to parent DataGrid. So, it is not needed to handle events for each level. Events of all DataGrid’s can be listened from parent DataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the child DataGrid notifies the events to parent DataGrid; otherwise, <b>false</b>.
        /// The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// We can get the level of the DataGrid that firing an event from args.OriginalSender  
        /// 
        /// <code>
        /// private void DataGrid_RecordDeleting(object sender, RecordDeletingEventArgs args)
        /// {
        ///   if(args.OriginalSender is DetailsViewDataGrid)
        ///   {
        ///     //Event is fired for DetailsView DataGrid. 
        ///   }
        ///   else
        ///   {
        ///      //Event is fired for main DataGrid.
        ///   }
        /// }
        /// </code>
        /// <remarks> 
        public bool NotifyEventsToParentDataGrid
        {
            get { return (bool)GetValue(NotifyEventsToParentDataGridProperty); }
            set { SetValue(NotifyEventsToParentDataGridProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.NotifyEventsToParentDataGrid dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.NotifyEventsToParentDataGrid dependency property.
        /// </remarks>        
        public static readonly DependencyProperty NotifyEventsToParentDataGridProperty =
            GridDependencyProperty.Register("NotifyEventsToParentDataGrid", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));


        /// <summary>
        /// Gets or sets the style applied to GroupDropArea in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to GroupDropArea in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for GroupDropArea, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/>.
        /// </remarks>
        public Style GroupDropAreaStyle
        {
            get { return (Style)GetValue(GroupDropAreaStyleProperty); }
            set { SetValue(GroupDropAreaStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupDropAreaStyleProperty =
            GridDependencyProperty.Register("GroupDropAreaStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnGroupDropAreaStyleChanged));

        /// <summary>
        /// Gets or sets the string that is used to displayed on the GroupDropArea in SfDataGrid.
        /// </summary>
        /// <value>
        /// The string that is used to displayed on the GroupDropArea.
        /// </value>        
        public string GroupDropAreaText
        {
            get { return (string)GetValue(GroupDropAreaTextProperty); }
            set { SetValue(GroupDropAreaTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaText dependency property.
        /// </remarks>
        public static readonly DependencyProperty GroupDropAreaTextProperty =
            GridDependencyProperty.Register("GroupDropAreaText", typeof(string), typeof(SfDataGrid), new GridPropertyMetadata(GridResourceWrapper.GroupDropAreaText, OnGroupDropAreaTextChanged));
        private static void OnGroupDropAreaTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.groupDropArea != null)
                grid.groupDropArea.GroupDropAreaText = grid.GroupDropAreaText;
        }

        /// <summary>
        /// Gets or sets a value to control data manipulation operations during data updates.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Data.LiveDataUpdateMode"/> that indicates how data manipulation operations are handled during data updates. 
        /// The default value is <see cref="Syncfusion.Data.LiveDataUpdateMode.Default"/>.
        /// </value>
        public LiveDataUpdateMode LiveDataUpdateMode
        {
            get { return (LiveDataUpdateMode)GetValue(LiveDataUpdateModeProperty); }
            set { SetValue(LiveDataUpdateModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that denotes the mode of calculation for caption and group summaries for improved performance. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Data.CalculationMode"/> that indicates when to perform the summary calculation.
        /// The default value is<see cref="Syncfusion.Data.CalculationMode.Default"/>.
        /// </value>
        public CalculationMode SummaryCalculationMode
        {
            get { return (CalculationMode)GetValue(SummaryCalculationModeProperty); }
            set { SetValue(SummaryCalculationModeProperty, value); }
        }
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.LiveDataUpdateMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.LiveDataUpdateMode dependency property.
        /// </remarks>        
        public static readonly DependencyProperty LiveDataUpdateModeProperty =
            GridDependencyProperty.Register("LiveDataUpdateMode", typeof(LiveDataUpdateMode), typeof(SfDataGrid), new GridPropertyMetadata(LiveDataUpdateMode.Default, OnLiveDataUpdateModePropertyChanged));
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CalculationMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CalculationMode dependency property.
        /// </remarks>     
        public static readonly DependencyProperty SummaryCalculationModeProperty =
            GridDependencyProperty.Register("SummaryCalculationMode", typeof(CalculationMode), typeof(SfDataGrid), new GridPropertyMetadata(CalculationMode.Default, OnSummaryCalculationModePropertyChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether the group is expanded automatically , when the column is grouped.
        /// </summary>
        /// <value>
        /// <b>true</b> if the groups is expanded automatically; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool AutoExpandGroups
        {
            get { return (bool)GetValue(AutoExpandGroupsProperty); }
            set { SetValue(AutoExpandGroupsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoExpandGroups dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoExpandGroups dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AutoExpandGroupsProperty =
            GridDependencyProperty.Register("AutoExpandGroups", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false, OnAutoExpandGroupsChanged));
#if !WP
        /// <summary>
        /// Gets or sets a value that indicates whether the user can delete the rows using Delete Key.
        /// </summary>
        /// <value>
        /// <b>true</b> if the user can delete the rows; otherwise, <b>false</b>. 
        /// The default value is <b>false</b>.
        /// </value> 
        /// <remarks>
        /// The deleting operations can be handled through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleting"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleted"/> event handlers in SfDataGrid.
        /// </remarks>
        public bool AllowDeleting
        {
            get { return (bool)GetValue(AllowDeletingProperty); }
            set { SetValue(AllowDeletingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowDeleting dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowDeleting dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowDeletingProperty =
            GridDependencyProperty.Register("AllowDeleting", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the style applied to all the DetailsViewDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to all the DetailsViewDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a DetailsViewDataGrid, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewDataGrid"/>.
        /// </remarks>
        public Style DetailsViewDataGridStyle
        {
            get { return (Style)GetValue(DetailsViewDataGridStyleProperty); }
            set { SetValue(DetailsViewDataGridStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewDataGridStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewDataGridStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty DetailsViewDataGridStyleProperty =
            GridDependencyProperty.Register("DetailsViewDataGridStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null, OnDetailsViewDataGridStyleChanged));
        
        /// <summary>
        /// Gets or sets a value that indicates whether the filtering is enabled in SfDataGrid.
        /// </summary>
        /// <value>
        /// <b>true</b> if the filtering is enabled in SfDataGrid ; otherwise, <b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The filtering operation can be canceled or customized through <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanging"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanged"/> events in SfDataGrid.
        /// </remarks>
        public bool AllowFiltering
        {
            get { return (bool)GetValue(AllowFilteringProperty); }
            set { SetValue(AllowFilteringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowFiltering dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowFiltering dependency property.
        /// </remarks>        
        public static readonly DependencyProperty AllowFilteringProperty =
            GridDependencyProperty.Register("AllowFiltering", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false, OnAllowFiltersChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether to create view by considering ICollectionView.Filter and DataView.RowFilter.
        /// </summary>
        /// <value>
        /// <b>true</b> if View need to filter the rows in SfDataGrid, using DataView.RowFilter expression and ICollectionView.Filter method; otherwise ,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The default filter which created in DataView or ICollectionView can be applied or canceled through  <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CanUseRowFilter"/> property.
        /// </remarks>

        public bool CanUseViewFilter
        {
            get { return (bool)GetValue(CanUseViewFilterProperty); }
            set { SetValue(CanUseViewFilterProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CanUseRowFilter dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CanUseRowFilter dependency property.
        /// </remarks>  
        public static readonly DependencyProperty CanUseViewFilterProperty =
            GridDependencyProperty.Register("CanUseViewFilter", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicate whether to listen the INotifyPropertyChanging/Changed event which is implemented by <see cref="Syncfusion.Data.CollectionViewAdv.SoureCollection"/> items.
        /// </summary>
        /// <value>
        /// <b>false</b> to avoid listening the <see cref="System.ComponentModel.INotifyPropertyChanging"/>,<see cref="System.ComponentModel.INotifyPropertyChanged"/> events which is implemented by <see cref="Syncfusion.Data.CollectionViewAdv.SoureCollection"/> items.
        /// </value>
        /// <remarks>
        /// By default SourceCollection items listens the INotifyProeprtyChanging/Changed events when enumerating the SourceCollection.
        /// </remarks>
        [Obsolete("This Property is not used any more instead of this use NotificationSubscriptionMode", false)]
        public bool CanListenPropertyNotification
        {
            get { return (bool)GetValue(CanListenPropertyNotificationProperty); }
            set { SetValue(CanListenPropertyNotificationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CanListenPropertyNotification dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CanListenPropertyNotification dependency property.
        /// </remarks>  
        public static readonly DependencyProperty CanListenPropertyNotificationProperty =
            GridDependencyProperty.Register("CanListenPropertyNotification", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(true,OnListenNotificationChanged));

        /// <summary>
        /// Gets or sets the style applied to the filter pop-up in SfDataGrid.
        /// </summary>
        /// <value>
        /// The style that is applied to the filter pop-up in SfDataGrid. The default value is <b>null</b>.
        /// </value>
        /// <remarks>
        /// To define a <see cref="System.Windows.Style"/> for a filter pop-up, specify a TargetType of <see cref="Syncfusion.UI.Xaml.Grid.GridFilterControl"/>.
        /// </remarks>
        public Style FilterPopupStyle
        {
            get { return (Style)GetValue(FilterPopupStyleProperty); }
            set { SetValue(FilterPopupStyleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupStyle dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupStyle dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FilterPopupStyleProperty =
            GridDependencyProperty.Register("FilterPopupStyle", typeof(Style), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the filter pop-up in SfDataGrid.
        /// </summary>    
        /// <value>
        /// The object that defines the visual representation of the filter pop-up. The default value is <b>null</b>.
        /// </value>
        public DataTemplate FilterPopupTemplate
        {
            get { return (DataTemplate)GetValue(FilterPopupTemplateProperty); }
            set { SetValue(FilterPopupTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterPopupTemplate dependency property.
        /// </remarks>        
        public static readonly DependencyProperty FilterPopupTemplateProperty =
            GridDependencyProperty.Register("FilterPopupTemplate", typeof(DataTemplate), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that indicates the visibility and position of FilterRow in SfDataGrid. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.FilterRowPosition"/> enumeration that specifies the visibility and position of FilterRow. 
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.FilterRowPosition.None"/>.
        /// </value>
        public FilterRowPosition FilterRowPosition
        {
            get { return (FilterRowPosition)GetValue(FilterRowPositionProperty); }
            set { SetValue(FilterRowPositionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterRowPosition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterRowPosition dependency property.
        /// </remarks>  
        public static readonly DependencyProperty FilterRowPositionProperty =
            GridDependencyProperty.Register("FilterRowPosition", typeof(FilterRowPosition), typeof(SfDataGrid), new GridPropertyMetadata(FilterRowPosition.None, OnFilterRowPositionChanged));

        /// <summary>
        /// Gets or sets the collection of <see cref="Syncfusion.UI.Xaml.Grid.ViewDefinition"/> that enables you to represent the data in to hierarchical format.
        /// </summary>
        /// <value>The collection of <see cref="Syncfusion.UI.Xaml.Grid.ViewDefinition"/>.The default value is null. </value>
        /// <remarks>This property helps you to populate Master-Details View manually.</remarks>        
        [Cloneable(false)]
        public DetailsViewDefinition DetailsViewDefinition
        {
            get { return (DetailsViewDefinition)GetValue(DetailsViewDefinitionProperty); }
            set { SetValue(DetailsViewDefinitionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewDefinition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewDefinition dependency property.
        /// </remarks>        
        public static readonly DependencyProperty DetailsViewDefinitionProperty =
            GridDependencyProperty.Register("DetailsViewDefinition", typeof(DetailsViewDefinition), typeof(SfDataGrid), new GridPropertyMetadata(null, OnDetailsViewDefinitionChanged));

        /// <summary>
        /// Gets or sets a value that indicates whether to re-use rows and cells when ItemsSource gets changed at runtime. 
        /// </summary>        
        /// <value>
        /// <b>true</b> if rows can be reused and children will not be removed from VisualContainer ; otherwise , <b>false</b>. The default value is <b>true</b>.
        /// </value>
        public bool ReuseRowsOnItemssourceChange
        {
            get { return (bool)GetValue(ReuseRowsOnItemssourceChangeProperty); }
            set { SetValue(ReuseRowsOnItemssourceChangeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ReuseRowsOnItemssourceChange dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ReuseRowsOnItemssourceChange dependency property.
        /// </remarks>        
        public static readonly DependencyProperty ReuseRowsOnItemssourceChangeProperty =
            GridDependencyProperty.Register("ReuseRowsOnItemssourceChange", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(true, OnReuseRowsOnItemssourceChangeChanged));
#else
        /// <summary>
        /// Gets or sets a value that indicates whether the zooming is enabled in SfDataGrid.
        /// </summary>        
        /// <value>
        /// <b>true</b> if the zooming is enabled in SfDataGrid;otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool EnableZooming
        {
            get { return (bool)GetValue(EnableZoomingProperty); }
            set { SetValue(EnableZoomingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.EnableZooming dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.EnableZooming dependency property.
        /// </remarks>
        public static readonly DependencyProperty EnableZoomingProperty =
            GridDependencyProperty.Register("EnableZooming", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(false));

        /// <summary>
        /// Gets or sets the zoom scale.
        /// </summary>        
        /// <value>
        /// The zoom scale value.
        /// </value>
        public double ZoomScale
        {
			get { return (double)GetValue(ZoomScaleProperty); }
            set { SetValue(ZoomScaleProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ZoomScale dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ZoomScale dependency property.
        /// </remarks>
        public static readonly DependencyProperty ZoomScaleProperty =
            GridDependencyProperty.Register("ZoomScale", typeof(double), typeof(SfDataGrid), new GridPropertyMetadata(1.0, OnZoomScaleChanged));
#endif

#region ContextMenu DP

        /// <summary>
        /// Gets or sets the shortcut menu that appears on the group drop item of <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/>.
        /// </summary>
        /// <value>
        /// The shortcut menu for the group drop item in SfDataGrid.
        /// The default value is null.
        /// </value>  
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridColumnContextMenuInfo"/>
        /// as command parameter which provides GridColumn information.
        /// </remarks> 
        public ContextMenu GroupDropItemContextMenu
        {
            get { return (ContextMenu)GetValue(GroupDropItemContextMenuProperty); }
            set { SetValue(GroupDropItemContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropItemContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropItemContextMenu dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupDropItemContextMenuProperty =
            GridDependencyProperty.Register("GroupDropItemContextMenu", typeof(ContextMenu), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the shortcut menu that appears on the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/> of SfDataGrid.
        /// </summary>
        /// <value>
        /// The shortcut menu for the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/> in SfDataGrid. The default value is null.
        /// </value>
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GroupDropAreaContextMenuInfo"/> as command parameter which contains the SfDataGrid information.
        /// </remarks>
        public ContextMenu GroupDropAreaContextMenu
        {
            get { return (ContextMenu)GetValue(GroupDropAreaContextMenuProperty); }
            set { SetValue(GroupDropAreaContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupDropAreaContextMenu dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupDropAreaContextMenuProperty =
            GridDependencyProperty.Register("GroupDropAreaContextMenu", typeof(ContextMenu), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the shortcut menu that appears on the group summary of SfDataGrid.
        /// </summary>
        /// <value>
        /// The shortcut menu for the group summary in SfDataGrid. The default value is null.
        /// </value>  
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridRecordContextMenuInfo"/> 
        /// as command parameter which contains the corresponding summary record.
        /// </remarks>
        public ContextMenu GroupSummaryContextMenu
        {
            get { return (ContextMenu)GetValue(GroupSummaryContextMenuProperty); }
            set { SetValue(GroupSummaryContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupSummaryContextMenu dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupSummaryContextMenuProperty =
            GridDependencyProperty.Register("GroupSummaryContextMenu", typeof(ContextMenu), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the shortcut menu that appears on table summary of SfDataGrid.
        /// </summary>
        /// <value>
        /// The shortcut menu for the table summary in SfDataGrid. The default value is null.
        /// </value> 
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridRecordContextMenuInfo"/>
        /// as command parameter which contains the corresponding summary record.
        /// </remarks>
        public ContextMenu TableSummaryContextMenu
        {
            get { return (ContextMenu)GetValue(TableSummaryContextMenuProperty); }
            set { SetValue(TableSummaryContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.TableSummaryContextMenu dependency property.
        /// </remarks>        
        public static readonly DependencyProperty TableSummaryContextMenuProperty =
            GridDependencyProperty.Register("TableSummaryContextMenu", typeof(ContextMenu), typeof(SfDataGrid), new GridPropertyMetadata(null));

        /// <summary>
        /// Gets or sets the shortcut menu that appears on the group caption of SfDataGrid.
        /// </summary>
        /// <value>
        /// The shortcut menu for the group caption in SfDataGrid. The default value is null.
        /// </value>
        /// <remarks>
        /// Command bound with MenuItem receives <see cref="Syncfusion.UI.Xaml.Grid.GridRecordContextMenuInfo"/>
        /// as command parameter which contains the corresponding group.
        /// </remarks>
        public ContextMenu GroupCaptionContextMenu
        {
            get { return (ContextMenu)GetValue(GroupCaptionContextMenuProperty); }
            set { SetValue(GroupCaptionContextMenuProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCaptionContextMenu dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCaptionContextMenu dependency property.
        /// </remarks>        
        public static readonly DependencyProperty GroupCaptionContextMenuProperty =
            GridDependencyProperty.Register("GroupCaptionContextMenu", typeof(ContextMenu), typeof(SfDataGrid), new GridPropertyMetadata(null));

#endregion
        /// <summary>
        /// Gets or sets a brush that highlights the data row is being hovered. 
        /// </summary>
        /// <value>
        /// The brush that highlights the data row is being hovered. The default value is LightGray.
        /// </value>
        public Brush RowHoverHighlightingBrush
        {
            get { return (Brush)GetValue(RowHoverHighlightingBrushProperty); }
            set { SetValue(RowHoverHighlightingBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowHoverHighlightingBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.RowHoverHighlightingBrush dependency property.
        /// </remarks>       
        public static readonly DependencyProperty RowHoverHighlightingBrushProperty =
            DependencyProperty.Register("RowHoverHighlightingBrush", typeof(Brush), typeof(SfDataGrid), new PropertyMetadata(new SolidColorBrush(Colors.LightGray), OnRowHoverHighlightingBrushPropertyChanged));

        private static void OnRowHoverHighlightingBrushPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dataGrid = d as SfDataGrid;

            if (!dataGrid.isGridLoaded)
                return;


            foreach (var dataRow in dataGrid.RowGenerator.Items)
            {
                if (dataRow.RowType == RowType.HeaderRow || dataRow.WholeRowElement == null)
                    continue;

                dataRow.WholeRowElement.RowHoverBackgroundBrush = dataGrid.RowHoverHighlightingBrush;
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the user can highlight the row being hovered through mouse or touch.
        /// </summary>
        /// <value>
        /// <b>true</b> if the row is highlighted while mouse hovering on it; otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        /// <remarks>
        /// The highlighting color can be customized using <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowHoverHighlightingBrush"/> property.
        /// </remarks>
        public bool AllowRowHoverHighlighting
        {
            get { return (bool)GetValue(AllowRowHoverHighlightingProperty); }
            set { SetValue(AllowRowHoverHighlightingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowRowHoverHighlighting dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AllowRowHoverHighlighting dependency property.
        /// </remarks>       
        public static readonly DependencyProperty AllowRowHoverHighlightingProperty =
            DependencyProperty.Register("AllowRowHoverHighlighting", typeof(bool), typeof(SfDataGrid), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates the visibility and position of AddNewRow in SfDataGrid. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowPosition"/> enumeration that specifies the visibility and position of AddNewRow. 
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowPosition.None"/>.
        /// </value>
        public AddNewRowPosition AddNewRowPosition
        {
            get { return (AddNewRowPosition)GetValue(AddNewRowPositionProperty); }
            set { SetValue(AddNewRowPositionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AddNewRowPosition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AddNewRowPosition dependency property.
        /// </remarks>           
        public static readonly DependencyProperty AddNewRowPositionProperty =
            GridDependencyProperty.Register("AddNewRowPosition", typeof(AddNewRowPosition), typeof(SfDataGrid), new GridPropertyMetadata(AddNewRowPosition.None, OnAddNewRowPositionChanged));

        /// <summary>
        /// Gets or sets the Expander column width.
        /// </summary>
        /// <value>
        /// The width of Expander column.
        /// </value>
        /// <remarks>
        /// Details view can be enabled by setting <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewDefinition"/> of SfDataGrid.
        /// </remarks>
        public double ExpanderColumnWidth
        {
            get { return (double)GetValue(ExpanderColumnWidthProperty); }
            set { SetValue(ExpanderColumnWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ExpanderColumnWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ExpanderColumnWidth dependency property.
        /// </remarks> 
        public static readonly DependencyProperty ExpanderColumnWidthProperty =
          GridDependencyProperty.Register("ExpanderColumnWidth", typeof(double), typeof(SfDataGrid), new GridPropertyMetadata
              (
#if WPF
24d
#else
45d
#endif
, OnExpanderColumnWidthChanged));
        private static void OnExpanderColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (!grid.isGridLoaded || grid.IsInDeserialize || grid.View == null)
                return;
            if (grid.DetailsViewManager.HasDetailsView)
            {
                int startColumnIndex = grid.ShowRowHeader ? 1 : 0;
                int expanderIndex = grid.View.GroupDescriptions.Count + startColumnIndex;
                grid.VisualContainer.ColumnWidths[expanderIndex] = (double)(e.NewValue);
                grid.GridColumnSizer.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the indent column width.
        /// </summary>
        /// <value>
        /// The width of indent column.
        /// </value>
        /// <remarks>
        /// Grouping can be enabled by setting <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupColumnDescriptions"/> of SfDataGrid.
        /// </remarks>
        public double IndentColumnWidth
        {
            get { return (double)GetValue(IndentColumnWidthProperty); }
            set { SetValue(IndentColumnWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.IndentColumnWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.IndentColumnWidth dependency property.
        /// </remarks> 
        public static readonly DependencyProperty IndentColumnWidthProperty =
          GridDependencyProperty.Register("IndentColumnWidth", typeof(double), typeof(SfDataGrid), new GridPropertyMetadata
              (
#if WPF
24d
#else
45d
#endif
, OnIndentColumnWidthChanged));

        private static void OnIndentColumnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (!grid.isGridLoaded || grid.IsInDeserialize || grid.View == null)
                return;
            int startColumnIndex = grid.ShowRowHeader ? 1 : 0;
            int indentColumnCount = grid.View.GroupDescriptions.Count + startColumnIndex;
            if (indentColumnCount != 0)
            {
                for (int i = startColumnIndex; i < indentColumnCount; i++)
                {
                    if (i < grid.VisualContainer.ColumnCount)
                        grid.VisualContainer.ColumnWidths[i] = (double)(e.NewValue);
                }
                grid.GridColumnSizer.Refresh();
            }
        }

        /// <summary>
        /// Gets or sets the settings for print the SfDataGrid.
        /// </summary>
        /// <value>The settings for print operations.</value>
        /// <remarks>
        /// <see cref="Syncfusion.UI.Xaml.Grid.PrintSettings"/> class that provides various properties for print settings. 
        /// </remarks>
#if WPF
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
#endif
        public PrintSettings PrintSettings
        {
            get { return (PrintSettings)GetValue(PrintSettingsProperty); }
            set { SetValue(PrintSettingsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.PrintSettings dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.PrintSettings dependency property.
        /// </remarks>           
        public static readonly DependencyProperty PrintSettingsProperty =
            DependencyProperty.Register("PrintSettings", typeof(PrintSettings), typeof(SfDataGrid),
                                        new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the data item that corresponds to the focused row or cell.
        /// </summary>
        /// <value>
        /// The corresponding focused data item.The default registered value is null.
        /// </value>        
        [Cloneable(false)]
        public object CurrentItem
        {
            get { return (object)GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentItem dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(object), typeof(SfDataGrid), new PropertyMetadata(null, OnCurrentItemChanged));

        /// <summary>
        /// Gets or sets a value that decides the type of selection behavior to be performed in SfDataGrid.        
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionUnit"/> enumeration that specifies the type of selection behavior in SfDataGrid.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionUnit.Row"/>. 
        /// </value>        
        public GridSelectionUnit SelectionUnit
        {
            get { return (GridSelectionUnit)GetValue(SelectionUnitProperty); }
            set { SetValue(SelectionUnitProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit dependency property.
        /// </remarks>         
        public static readonly DependencyProperty SelectionUnitProperty =
            GridDependencyProperty.Register("SelectionUnit", typeof(GridSelectionUnit), typeof(SfDataGrid), new GridPropertyMetadata(GridSelectionUnit.Row, OnSelectionUnitChanged));

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> which holds the current cell information such as corresponding record ,index ,column, and etc. 
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.GridCellInfo"/> that holds the current cell information. The default value is null.
        /// </value>
        /// <remarks>
        /// The current cell information is maintained for cell selection only.
        /// </remarks>
        [Cloneable(false)]
        public GridCellInfo CurrentCellInfo
        {
            get { return (GridCellInfo)GetValue(CurrentCellInfoProperty); }
            set { SetValue(CurrentCellInfoProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellInfo dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellInfo dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentCellInfoProperty =
            DependencyProperty.Register("CurrentCellInfo", typeof(GridCellInfo), typeof(SfDataGrid), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the column that contains the current cell.
        /// </summary>
        /// <value>
        /// The column that contains the current cell. The default value is null.
        /// </value>
        [Cloneable(false)]
        public GridColumn CurrentColumn
        {
            get { return (GridColumn)GetValue(CurrentColumnProperty); }
            set { SetValue(CurrentColumnProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentColumn dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentColumn dependency property.
        /// </remarks>         
        public static readonly DependencyProperty CurrentColumnProperty =
            DependencyProperty.Register("CurrentColumn", typeof(GridColumn), typeof(SfDataGrid), new PropertyMetadata(null));


#if WPF
        /// <summary>
        /// Gets or sets a value that provides the different scrolling mode.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.ScrollMode"/> enumeration that specifies scrolling experience in usability.
        /// The default is <see cref="Syncfusion.UI.Xaml.Grid.ScrollMode.None"/>. 
        /// </value>        
        /// <remarks>
        /// The <see cref="Syncfusion.UI.Xaml.Grid.ScrollMode"/> process the binding for VirtualizingCellsControl.DataContext property.        
        /// </remarks>
        public ScrollMode ScrollMode
        {
            get { return (ScrollMode)GetValue(ScrollModeProperty); }
            set { SetValue(ScrollModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.ScrollMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.ScrollMode dependency property.
        /// </remarks>             
        public static readonly DependencyProperty ScrollModeProperty =
            DependencyProperty.Register("ScrollMode", typeof(ScrollMode), typeof(SfDataGrid), new PropertyMetadata(ScrollMode.None, OnScrollModePropertyChanged));


        private static void OnScrollModePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            
            if (!grid.isGridLoaded)
                return;

            grid.RowGenerator.Items.ForEach(dr =>
                {
                    if (dr.RowType != RowType.HeaderRow)
                    {
                        dr.RowIndex = -1;
                    }
                });
            grid.container.InvalidateMeasureInfo();         
        }       
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether to create RecordEntry for all the objects in SourceCollection.
        /// </summary>
        /// <value><b>True</b> to create RecordEntry only when try to access this index.</value>
        public bool EnableDataVirtualization
        {
            get { return (bool)GetValue(EnableDataVirtualizationProperty); }
            set { SetValue(EnableDataVirtualizationProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.EnableDataVirtualization dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.EnableDataVirtualization dependency property.
        /// </remarks>             
        public static readonly DependencyProperty EnableDataVirtualizationProperty =
            DependencyProperty.Register("EnableDataVirtualization", typeof(bool), typeof(SfDataGrid), new PropertyMetadata(false));

#endregion

#region Dependency Property Call back       

        /// <summary>
        /// Dependency call back for OnGroupCaptionTextFormatPropertyChanged.
        /// </summary>
        private static void OnGroupCaptionTextFormatPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (!grid.isGridLoaded || grid.CaptionSummaryRow != null)
                return;
            //Need to update CaptionCoveredRow only
            grid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType == RowType.CaptionCoveredRow && row.RowData != null)
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle(row.RowData));
            });
        }

        /// <summary>
        /// Dependency call back for IsGroupDropAreaExpandedPropertyChanged.
        /// </summary>
        private static void IsGroupDropAreaExpandedPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.groupDropArea != null)
                grid.GroupDropArea.IsExpanded = (bool)e.NewValue;
        }

        // Flag used to skip ItemsSourceChanged process
        internal bool suspendItemsSourceChanged = false;

        /// <summary>
        /// Dependency call back for ItemsSource property.
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnItemsSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (!grid.isGridLoaded || grid.suspendItemsSourceChanged)
                return;

            grid.UnWireEvents();
            //Below condition will clear the selection when changing the itemssource of the grid.
            //Selection will be maintained based on SelecedItem only when SelectionUnit - Row. 
            if (args.NewValue == null || grid.SelectionUnit != GridSelectionUnit.Row || grid.SelectedItem == null)
                grid.ResetSelectionValues();                            
#if WPF
            if (grid.useDrawing)
                grid.Provider = null;
            // Reset the SearchHelpr provider when changing the items source.
            grid.SearchHelper.Provider = null;
#endif

            if (grid.GridModel != null)
                grid.GridModel.EndEdit();
            if (grid.View != null && grid.View is IGridViewNotifier)
            {
                (grid.View as IGridViewNotifier).DetachGridView();
            }
            if (args.OldValue != null)
            {
                if (grid.GroupDropArea != null)
                    grid.GroupDropArea.RemoveAllGroupDropItems();
                grid.DisposeViewOnItemsSourceChanged();
                grid.GridModel.RefreshDataRow();
            }

            grid.Columns.ForEach(column =>
            {
                //WPF-23087 When the mapping name will not be mention and modify the item source at runtime modified the property null checking is modified from ColumnWrapper to _columnWrapper
                if (column._columnWrapper != null)
                    column._columnWrapper.DataContext = null;
                // WPF-23276 - While changing itemssource, need to reset UnBoundFunc, because we have already detached grid from old view.
                if (column is GridUnBoundColumn)
                    (column as GridUnBoundColumn).UnBoundFunc = null;
            });

            grid.SetSourceList(args.NewValue);

            //WPF-35703 - SelectedItem has been reset when selecteditem available in new ItemsSource when SelectionUnit-Row.
            if (grid.ItemsSource != null && grid.SelectedItem != null && grid.isViewPropertiesEnsured && grid.SelectionUnit == GridSelectionUnit.Row)
            {
                // Whether the selecteditem is not in new itemssource, we can clear the selection related operations in datagrid.
                // SelectedItem has been cleared whether the new itemssource is queryable and EnableDataVirtualization is enabled for grid. Considering performance.
                if (grid.EnableDataVirtualization == true || !grid.CheckSelectedItemAvailable(grid.ItemsSource))
                    grid.ResetSelectionValues();               
                grid.UpdateSelection();
            }
            grid.RowGenerator.OnItemSourceChanged(args);
            grid.RefreshHeaderLineCount();
            grid.UpdateRowAndColumnCount(false);
            if (grid.VisualContainer != null)
                grid.VisualContainer.OnItemSourceChanged();
            //WPF-23960 If we Set the SelectedIndex in XAML and we have TableSummaryRow with Paging, the CurrentCell
            //is not maintain properly because TableSummaryRow count is not added with HeaderLineCount, we have 
            //calculate TableSummaryRow count based on View, view is Created in SetSourceList method and we have 
            //Refresh the HeaderLineCount by using RefreshHeaderLineCount so after processing this two methods we have 
            //set the CurrentRowColumnIndex by uing EnsureViewProperties.
            //WPF-35703 SelectedItem has been reset when refreshing the view of datagrid.
            if (!grid.isViewPropertiesEnsured)
                grid.EnsureViewProperties();
            if (args.NewValue == null)
                grid.isViewPropertiesEnsured = false;
            grid.RaiseItemsSourceChanged(args.OldValue, args.NewValue);
            //WPF-20144(issue 2) while changing item source  at run time  need to create new instance od PrintManagerBase . so it need to be null  here.
            grid.PrintSettings.PrintManagerBase = null;
        }

        /// <summary>        
        /// Check whether the selected item available in new ItemsSource.
        /// </summary>  
        private bool CheckSelectedItemAvailable(object source)
        {
            foreach (var item in this.GetSourceList(source))
            {
                if (this.SelectedItem.Equals(item))
                    return true;
            }
            return false;
        }

        /// <summary>        
        /// Updates the SelectedItem in UI when ItemsSource gets changed to maintain the SelectedItem in UI.
        /// </summary>        
        /// <remarks>
        /// Its get called to maintain the SelectedItem when changing ItemsSource also.
        /// </remarks>
        private void UpdateSelection()
        {
            //WPF-35703 - To maintain the selected items while navigating between the tabs
            if (this.SelectedItem != null)
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs()
                {
                    NewValue = this.SelectedItem,
                    OldValue = null,
                    PropertyName = "SelectedItem"
                });
                isselecteditemchanged = false;
            }           
        }

        /// <summary>
        /// Dependency call back for BindableView property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnBindableViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (!grid.IsInternalChange)
                throw new InvalidOperationException("It is a read only property.You can't set the SfDataGrid BindableView as explicitly");
        }

        /// <summary>
        /// Releases all the view related operations in SfDataGrid , when the ItemsSource is changed.
        /// </summary>
        protected virtual void DisposeViewOnItemsSourceChanged()
        {
            if (this.View != null)
                this.View.Dispose();
        }

        internal override void OnSortNumberPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded || this.View == null)
                return;

            if ((bool)e.NewValue)
            {
                if (this.View.SortDescriptions.Count > 1)
                    this.GridModel.ShowSortNumbers();
            }
            else
                this.GridModel.CollapseSortNumber();
            this.GridColumnSizer.Refresh();
        }

        /// <summary>
        /// Dependency Call back for ColumnSizer
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnColumnSizerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (!grid.isGridLoaded || grid.IsInDeserialize)
                return;
            grid.GridColumnSizer.ResetAutoCalculationforAllColumns();
            grid.GridColumnSizer.Refresh();
            // WPF-14768 - To refresh detailsview data row while changing column sizer at runtime 
            if (grid is DetailsViewDataGrid && grid.NotifyListener != null)
            {
                grid.RowGenerator.LineSizeChanged();
            }
        }

        internal override void OnAutoGenerateColumnsChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None)
            {
                this.GenerateGridColumns();
                this.RefreshColumns();
            }
        }

        internal override void OnAutoGenerateColumnsModeChanged(DependencyPropertyChangedEventArgs args)
        {
            // ResetAll is not applicable for DetailsViewDataGrid, So Reset will be set
            if ((this is DetailsViewDataGrid || this.IsSourceDataGrid) &&
                this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
                this.AutoGenerateColumnsMode = AutoGenerateColumnsMode.Reset;
            if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None)
            {
                this.GenerateGridColumns();
                this.RefreshColumns();
            }
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

        internal override void OnSelectionModeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded)
                return;

            if (this.DetailsViewManager.HasDetailsView)
            {
                foreach (var detailsview in this.DetailsViewDefinition)
                {
                    var detailsViewGrid = (detailsview as GridViewDefinition).DataGrid;
                    detailsViewGrid.SelectionMode = (GridSelectionMode)args.NewValue;
                }
            }

            this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs() { NewValue = args.NewValue, OldValue = args.OldValue, PropertyName = "SelectionMode" });
            foreach (var column in this.Columns.OfType<GridCheckBoxColumn>())
                this.RowGenerator.UpdateBinding(column);
        }

        internal override void OnSelectedIndexChanged(DependencyPropertyChangedEventArgs args)
        {
            if (!this.isGridLoaded || this.View == null)
            {
                this.isselectedindexchanged = true;
                return;
            }
            this.isselectedindexchanged = false;
            //Added the condition to check whether the NewValue is valid index. This callback will invoked after setting the value SelectedIndex hence 
            //added the condition to reset the value to OldValue.

            //WPF-35996 StackOverflowException are thrown when removing the selected item in SfDataGrid
            int recordscount = this.GridModel.HasGroup ? this.View.TopLevelGroup.DisplayElements.Count : this.View.Records.Count;

            if ((int)args.NewValue == -1 || ((int)args.NewValue < recordscount && (int)args.NewValue >= 0))
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

        internal override void OnHeaderRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            // If grid has no rows, RowHeights should not be updated
            if (!isGridLoaded || VisualContainer.RowCount == 0 || IsInDeserialize)
                return;
            var endHeaderIndex = HeaderLineCount - (this.GetTableSummaryCount(TableSummaryRowPosition.Top)
                + (AddNewRowPosition == AddNewRowPosition.FixedTop ? 1 : 0)
                + (FilterRowPosition == FilterRowPosition.FixedTop ? 1 : 0)
                + this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false));
            for (int i = 0; i < endHeaderIndex; i++)
            {
                VisualContainer.RowHeights[i] = (double)e.NewValue;
            }

            if (this is DetailsViewDataGrid)
                this.DetailsViewManager.RefreshParentDataGrid(this);

            this.VisualContainer.InvalidateMeasureInfo();
        }


        internal override void OnRowHeightChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!isGridLoaded)
                return;

            VisualContainer.RowHeights.DefaultLineSize = (double)e.NewValue;
            VisualContainer.InvalidateMeasureInfo();

            // WPF-19367 - To refresh parent datagrid based on the new row height            
            DetailsViewManager.RefreshParentDataGrid(this);
        }

        internal override void OnCurrentCellBorderThicknessPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;

            foreach (var dataRow in this.RowGenerator.Items)
            {
                if (dataRow.RowType == RowType.HeaderRow || dataRow.RowType == Grid.RowType.DetailsViewRow)
                    continue;

                foreach (var column in dataRow.VisibleColumns)
                {
                    var columnElement = column.ColumnElement as GridCell;
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
                if (dataRow.RowType == RowType.HeaderRow || dataRow.RowType == RowType.DetailsViewRow)
                    continue;

                foreach (var column in dataRow.VisibleColumns)
                {
                    var columnElement = column.ColumnElement as GridCell;
                    if (columnElement == null) continue;
                    columnElement.CurrentCellBorderBrush = this.CurrentCellBorderBrush;
                }
            }
        }

        /// <summary>
        /// Dependency call back method of CaptionSummaryRow. Which is helpes to upadete the view while settinf the CaptionSummaryRowDynamically
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="args">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnCaptionSummaryRowChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var dataGrid = obj as SfDataGrid;
            if (dataGrid == null || !dataGrid.isGridLoaded || dataGrid.View == null)
                return;
            dataGrid.OnCaptionSummaryRowChanged(args.NewValue as GridSummaryRow);
        }

        /// <summary>
        /// Dependency call back for CellStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null)
                return;
            grid.hasCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateCellStyles();
            }
        }

        /// <summary>
        /// Dependency call back for UnBoundRowCellStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnUnBoundRowCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null)
                return;
            grid.hasUnBoundRowCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateUnBoundRowCellStyle();
            }
        }

        private static void OnGroupDropAreaStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || grid.GroupDropArea == null)
                return;
            if (e.NewValue != null)
                grid.GroupDropArea.Style = e.NewValue as Style;
            else
                grid.GroupDropArea.ClearValue(FrameworkElement.StyleProperty);
        }

        private static void OnDetailsViewDataGridStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || e.NewValue == null)
                return;
            //While changing style at run time, Update the style for already created DetailsViewDataGrids from ClonedDataGrid
            foreach (var gridViewDefintion in grid.DetailsViewDefinition)
            {
                if (gridViewDefintion is GridViewDefinition && (gridViewDefintion as GridViewDefinition).NotifyListener != null)
                {
                    foreach (var childgrid in (gridViewDefintion as GridViewDefinition).NotifyListener.ClonedDataGrid)
                    {
                        childgrid.Style = e.NewValue as Style;
                    }
                }
            }
        }

        private static void OnGroupSummaryCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasGroupSummaryCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
            }
        }

        private static void OnGroupSummaryCellStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasGroupSummaryCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
            }
        }

        private static void OnCaptionSummaryCellStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasCaptionSummaryCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
            }
        }

        private static void OnCaptionSummaryCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasCaptionSummaryCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
            }
        }

        private static void OnTableSummaryCellStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasTableSummaryCellStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
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
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateCellStyles();
            }
        }

        private static void OnTableSummaryCellStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasTableSummaryCellStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateSummariesCellStyle();
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
        /// Dependency call back for RowStyleSelector property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasRowStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }

        /// <summary>
        /// Dependency call back for RowStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }

        /// <summary>
        /// Dependency call back for UnBoundRowStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnUnBoundRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasUnBoundRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateRowStyle();
        }

        private static void OnGroupSummaryRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasGroupSummaryRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        private static void OnGroupSummaryRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasGroupSummaryRowStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        private static void OnCaptionSummaryRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasCaptionSummaryRowStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        private static void OnCaptionSummaryRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasCaptionSummaryRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        private static void OnTableSummaryRowStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasTableSummaryRowStyle = true;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        private static void OnTableSummaryRowStyleSelectorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null) return;
            grid.hasTableSummaryRowStyleSelector = e.NewValue != null;
            if (grid.isGridLoaded)
                grid.UpdateSummariesRowStyle();
        }

        /// <summary>
        /// Dependency call back for HeaderStyle property.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.DependencyPropertyChangedEventArgs">DependencyPropertyChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private static void OnHeaderStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid != null)
                grid.hasHeaderStyle = true;
            if (grid.isGridLoaded)
            {
                grid.UpdateHeaderRowStyle();
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
            var grid = d as SfDataGrid;
            if (grid != null)
                grid.hasHeaderTemplate = e.NewValue != null;
            if (grid.isGridLoaded)
            {
                grid.UpdateHeaderRowStyle();
            }
        }

        private static void OnLiveDataUpdateModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (grid.View != null)
                grid.View.LiveDataUpdateMode = grid.LiveDataUpdateMode;
        }
        private static void OnSummaryCalculationModePropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (grid.View != null)
                grid.View.SummaryCalculationMode = grid.SummaryCalculationMode;
        }
        private static void OnFrozenColumnCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.IsInDeserialize)
                return;

            if ((int)e.NewValue > 0 && grid.DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FreezePanes");

            if (grid.isGridLoaded && (int)e.NewValue >= 0 && grid.VisualContainer.ColumnCount >= grid.ResolveToScrollColumnIndex((int)e.NewValue))
            {
                grid.UpdateFreezePaneColumns();
            }
        }

        private static void OnFooterColumnCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.IsInDeserialize)
                return;

            if ((int)e.NewValue > 0 && grid.DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FreezePanes");

            if (grid.isGridLoaded && (int)e.NewValue >= 0 && grid.AllowFooterColumns((int)e.NewValue))
            {
                grid.UpdateFreezePaneColumns();
            }
        }

        private static void OnFrozenRowsCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.IsInDeserialize)
                return;

            if ((int)e.NewValue > 0 && grid.DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FreezePanes");

            if (grid.isGridLoaded && (int)e.NewValue >= 0 && !grid.AllowFrozenGroupHeaders && grid.container.RowCount >= grid.ResolveToRowIndex((int)e.NewValue))
            {
                grid.UpdateFreezePaneRows();
                grid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        private static void OnFooterRowsCountPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid.IsInDeserialize)
                return;

            if ((int)e.NewValue > 0 && grid.DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FreezePanes");

            if (grid.isGridLoaded && (int)e.NewValue >= 0 && grid.AllowFooterRows((int)e.NewValue))
            {
                grid.UpdateFreezePaneRows();
                grid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        private static void OnAutoExpandGroupsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (grid.View != null)
            {
                grid.View.AutoExpandGroups = (bool)args.NewValue;
            }
        }

        private static void OnAllowFixedGroupCaptionsChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;

            if ((bool)args.NewValue && grid.DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FrozenGroupHeaders");

            if ((bool)args.NewValue && grid.QueryUnBoundRow != null)
                throw new NotSupportedException("UnBoundRow is not supported with FrozenGroupHeaders");

            if ((bool)args.NewValue && grid.QueryCoveredRange != null)
                throw new NotSupportedException("Merged Cells not supported with FrozenGroupHeaders");

            if (grid.VisualContainer != null)
            {
                grid.VisualContainer.AllowFixedGroupCaptions = grid.AllowFrozenGroupHeaders;
                grid.UpdateFreezePaneRows();
                grid.VisualContainer.InvalidateMeasureInfo();
            }
        }

        private static void OnAllowFiltersChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (!grid.isGridLoaded)
                return;

            if (grid.Columns.Count > 0)
                grid.RefreshFilterIconVisibility();
        }

        internal override void OnAllowEditingChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.Columns.Count == 0)
                return;

            foreach (var column in this.Columns)
            {
                var AllowEditColumn = column.ReadLocalValue(GridColumnBase.AllowEditingProperty);
                if (AllowEditColumn == DependencyProperty.UnsetValue)
                    column.UpdateBindingBasedOnAllowEditing();
            }

            foreach (var column in this.Columns.OfType<GridCheckBoxColumn>())
                this.RowGenerator.UpdateBinding(column);

            if (!this.AllowEditing && this.SelectionController.CurrentCellManager.HasCurrentCell)
            {
                int columnIndex = this.ResolveToGridVisibleColumnIndex(this.SelectionController.CurrentCellManager.CurrentCell.ColumnIndex);
                if (!this.Columns[columnIndex].AllowEditing && this.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                    this.SelectionController.CurrentCellManager.EndEdit();
            }
        }

        private static void OnGroupSummaryRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || !grid.isGridLoaded || grid.View == null || grid.IsInDeserialize)
                return;

            var groupSummaryRows = e.NewValue as ObservableCollection<GridSummaryRow>;
            if (groupSummaryRows == null)
                return;

            grid.View.SummaryRows.Clear();
            grid.InitializeGroupSummaryRows();
        }
        /// <summary>
        /// Dependency call back of UnBoundRows collection.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnUnBoundRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || !grid.isGridLoaded || grid.IsInDeserialize)
                return;

            if (e.NewValue == e.OldValue || grid.GridModel == null)
                return;

            var UnBoundRows = e.NewValue as UnBoundRows;
            if (UnBoundRows == null)
                return;

            if (e.OldValue != null && e.OldValue is UnBoundRows)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= grid.GridModel.OnUnBoundRowsChanged;

            if (e.NewValue is UnBoundRows)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += grid.GridModel.OnUnBoundRowsChanged;

            // From root DataGrid, need to set SortColumnDescriptions for other DetailsViewDataGrids
            if (grid.IsSourceDataGrid)
            {
                if (grid.NotifyListener != null)
                {
                    grid.NotifyListener.NotifyCollectionPropertyChanged(grid, typeof(UnBoundRows));
                }
            }

            // Which initialize the UnBoundDatarow for the grid.                   
            grid.RefreshHeaderLineCount();
            // Reset the row indexes for rows.
            grid.RowGenerator.Items.ForEach(row => row.RowIndex = -1);
            grid.UpdateRowAndColumnCount(false);
        }

        private static void OnTableSummaryRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;
            if (grid == null || !grid.isGridLoaded || grid.View == null || grid.IsInDeserialize)
                return;

            var tableSummaryRows = e.NewValue as ObservableCollection<GridSummaryRow>;
            if (tableSummaryRows == null)
                return;

            grid.View.TableSummaryRows.Clear();
            grid.InitializeTableSummaries();
            grid.GridModel.InitializeGridTableSummaryRow();
            if (e.OldValue != null)
            {
                (e.OldValue as ObservableCollection<GridSummaryRow>).ForEach(row =>
                    {
                        if (row is GridTableSummaryRow)
                        {
                            (row as GridTableSummaryRow).Dispose();
                        }
                    });
            }

            grid.RefreshHeaderLineCount();
            // Reset the row indexes for rows.
            grid.RowGenerator.Items.ForEach(item => item.RowIndex = -1);
            grid.UpdateRowAndColumnCount(false);
        }

        private static void OnNewItemPlaceholderPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            var grid = obj as SfDataGrid;
            if (grid.View != null)
                grid.View.NewItemPlaceholderPosition = grid.NewItemPlaceholderPosition;
        }

        internal override void OnNavigationModeChanged(DependencyPropertyChangedEventArgs args)
        {
            if (this.SelectionUnit != GridSelectionUnit.Row && (NavigationMode)args.NewValue == NavigationMode.Row)
                throw new InvalidOperationException("NavigationMode should be Cell when SelectionUnit is Cell or Any");

            if (this.DetailsViewManager.HasDetailsView)
            {
                foreach (var detailsview in this.DetailsViewDefinition)
                {
                    var detailsViewGrid = (detailsview as GridViewDefinition).DataGrid;
                    detailsViewGrid.NavigationMode = (NavigationMode)args.NewValue;
                }
            }

            if (this.isGridLoaded)
            {
                this.SelectionController.HandleSelectionPropertyChanges(new SelectionPropertyChangedHandlerArgs() { NewValue = args.NewValue, OldValue = args.OldValue, PropertyName = "NavigationMode" });
            }
        }

        private static void OnDetailsViewDefinitionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;

            grid.CheckDetailsViewSupport();

            if (!grid.isGridLoaded)
                return;

            grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                {
                    row.CatchedRowIndex = -1;
                });
            if (grid.View != null)
            {
                grid.View.Records.SuspendUpdates();
                foreach (var rec in grid.View.Records)
                {
                    if (rec.ChildViews != null)
                    {
                        rec.IsExpanded = false;
                        rec.ChildViews.Clear();
                    }
                }
                grid.View.Records.ResumeUpdates();
                if (grid.View.TopLevelGroup != null)
                {
                    grid.View.TopLevelGroup.SetDirty();
                    grid.View.TopLevelGroup.Groups.SetDirty();
                    grid.View.TopLevelGroup.ResetCache = true;
                }
            }
            grid.GridModel.RefreshView(true);
            grid.DetailsViewManager.ResetExpandedDetailsView(true);
            grid.inDetailsViewIndentChange = true;
            if (grid.Columns.Count > 0)
                grid.GridModel.ResetColumns();
            grid.UpdateRowAndColumnCount(false);
            grid.inDetailsViewIndentChange = false;
            grid.VisualContainer.NeedToRefreshColumn = true;

            //(WPF - 37043) Two current cells maintained while changing DetailsViewDefinition at runtime is fixed.
            var oldCount = args.OldValue != null ? (args.OldValue as DetailsViewDefinition).Count : 0;
            var newCount = args.NewValue != null ? (args.NewValue as DetailsViewDefinition).Count : 0;
            var currentColumnIndex = grid.SelectionController.CurrentCellManager.CurrentRowColumnIndex.ColumnIndex;

            if (newCount > oldCount)
                grid.SelectionController.CurrentCellManager.SetCurrentColumnIndex(currentColumnIndex + (newCount - oldCount));
            else if (oldCount > newCount)
                grid.SelectionController.CurrentCellManager.SetCurrentColumnIndex(currentColumnIndex - (oldCount - newCount));

            //UWP-2648 - When the GridCell has selected and the columns get removed or changing the details view definition we need to commit the current row to avoid the crash.
            if (grid.GridModel != null)
                grid.GridModel.EndEdit();

            grid.VisualContainer.InvalidateMeasureInfo();
        }

        private static void OnReuseRowsOnItemssourceChangeChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            // Set ReuseRowsOnItemssourceChange to all SourceDataGrid present in GridViewDefintion
            if (grid.DetailsViewManager.HasDetailsView)
            {
                foreach (var detailsview in grid.DetailsViewDefinition)
                {
                    var sourceDataGrid = (detailsview as GridViewDefinition).DataGrid;
                    sourceDataGrid.ReuseRowsOnItemssourceChange = (bool)args.NewValue;
                }
            }
        }

        private static void OnHideEmptyGridViewDefinitionChanged(DependencyObject obj,
            DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (!grid.isGridLoaded)
                return;

            grid.CollapseAllDetailsView();
            // CheckForDetailsViewExpanderVisibilty will be called since we called CollapseAllDetailsView. so i have commented the following lines
            //if (grid.DetailsViewManager.HasDetailsView)
            //{
            //    grid.RowGenerator.Items.OfType<DataRow>().ForEach(row =>
            //    {
            //        if (row.RowIndex != -1)
            //            row.CheckForDetailsViewExpanderVisibilty();
            //    });
            //}
        }

        /// <summary>
        /// Method to handle the call back of StackedHeaderRows property changed.
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnStackedHeaderRowsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var grid = d as SfDataGrid;

            //是否为反序列化
            if (grid.IsInDeserialize)
                return;

            // From SourceDataGrid, need to set StackedHeaderRows for other DetailsViewDataGrids
            // 是否为DetailsViewDataGrids设置StackedHeader
            if (grid.IsSourceDataGrid)
            {
                if (grid.NotifyListener != null)
                {
                    grid.NotifyListener.NotifyCollectionPropertyChanged(grid, typeof(StackedHeaderRows));
                }
                return;
            }

            if (e.NewValue == e.OldValue)
                return;

            // WPF - 37863 Stacked header for first details view grid is not updating properly.
            // 第一个details view的堆栈头没有没有正确更新
            if (!grid.isGridLoaded)
                return;

            var stackedHeaderRows = e.NewValue as StackedHeaderRows;
            if (stackedHeaderRows == null)
                throw new NotSupportedException("Stacked Headers not support with null value");

            // GridModel 和 VisualContainer会在Template中应用()   code:92行
            if (grid.GridModel == null || grid.VisualContainer == null)
                return;

            //删除之前的表头
            grid.RowGenerator.RemoveStackedHeader();

            // Grid Model wiring and un wiring the events for collection.
            if (e.OldValue != null && e.OldValue is StackedHeaderRows)
                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= grid.GridModel.OnStackedHeaderRowsChanged;

            if (e.NewValue is StackedHeaderRows)
                //集合改变属性
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += grid.GridModel.OnStackedHeaderRowsChanged;

            //上面验证过一次这里又验证一次，可见此程序的严谨性
            if (grid.VisualContainer == null)
                return;

            // 新旧行的数量
            int oldItemsCount = e.OldValue is IEnumerable ? (e.OldValue as IEnumerable).AsQueryable().Count() : 0;
            int newItemsCount = e.NewValue is IEnumerable ? (e.NewValue as IEnumerable).AsQueryable().Count() : 0;

            // 取消SeackedHeader 中的单元格改变事件和删除旧的StackedHeaderRows
            if (e.OldValue != null && oldItemsCount != 0)
            {
                // 注意这里用的是临时变量，不是依赖属性StackedHeaderRows
                var stackedHeaders = (e.OldValue as StackedHeaderRows);
                stackedHeaders.ForEach(row => row.StackedColumns.ForEach(col =>
                {
                    if (col.ChildColumnChanged != null)
                        col.ChildColumnChanged = null;
                }));
                grid.VisualContainer.RemoveRows(0, oldItemsCount);
            }

            // 这里操作的是 依赖属性 StackedHeaderRows 
            // stacked单元格changed事件和创建行
            if (e.NewValue != null && newItemsCount != 0)
            {
                //Stacked表头中的单元格发生改变时触发
                grid.InitializeStackedColumnChildDelegate();
                //创建行
                grid.VisualContainer.InsertRows(0, newItemsCount);
            }

            // 可以查询行高
            if (grid.CanQueryRowHeight())
                grid.VisualContainer.RowHeightManager.Reset();

            // 可以查询覆盖面积
            if (grid.CanQueryCoveredRange())
                grid.CoveredCells.Clear();

            // 更新表头行计数
            grid.RefreshHeaderLineCount();
            // Update FreezePanes
            // 更新冻结行和页脚行
            if (grid.container != null)
                grid.UpdateFreezePaneRows();

            // To update catched row index based on newly added/removed row count
            // 根据新添加/删除的行计数更新 捕获的 行索引
            if (grid.DetailsViewManager.HasDetailsView)
            {
                if (oldItemsCount != 0)
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= oldItemsCount;
                    });
                if (newItemsCount != 0)
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += newItemsCount;
                    });
            }

            grid.RowGenerator.Items.ForEach(row => row.RowIndex = -1);
            // To handle the selection when add / remove stacked header rows.
            // 在添加/删除堆叠的标题行时处理选择。
            grid.SelectionController.HandleGridOperations(
                new GridOperationsHandlerArgs(GridOperation.StackedHeaderRow,
                    new StackedHeaderCollectionChangedEventArgs(
                        newItemsCount > 0 ? NotifyCollectionChangedAction.Add : NotifyCollectionChangedAction.Remove,
                        newItemsCount > 0 ? newItemsCount : oldItemsCount)));
            
            // 这个是UIlement方法，异步布局更新
            grid.VisualContainer.InvalidateMeasureInfo();

            // To refresh parent DataGrid
            // 刷新父DataGrid
            if (grid is DetailsViewDataGrid && grid.NotifyListener != null)
                grid.DetailsViewManager.RefreshParentDataGrid(grid);
        }

        internal override void OnGridValidationModePropertyChanded(DependencyPropertyChangedEventArgs args)
        {
            if (isGridLoaded || this.IsSourceDataGrid)
            {
                foreach (GridColumn column in this.Columns)
                {
                    column.UpdateValidationMode();
                }
            }
        }

        internal override void OnSortColumnDescriptionsChanged(DependencyPropertyChangedEventArgs e)
        {
            // From SourceDataGrid, need to set SortColumnDescriptions for other DetailsViewDataGrids
            if (IsSourceDataGrid)
            {
                if (NotifyListener != null)
                {
                    NotifyListener.NotifyCollectionPropertyChanged(this, typeof(SortColumnDescriptions));
                }
            }

            if (e.NewValue == e.OldValue || this.GridModel == null)
                return;

            if (e.OldValue != null && e.OldValue is SortColumnDescriptions)

                (e.OldValue as INotifyCollectionChanged).CollectionChanged -= this.GridModel.OnSortColumnsChanged;

            if (e.NewValue == null)
                return;

            if (e.NewValue is SortColumnDescriptions)
                (e.NewValue as INotifyCollectionChanged).CollectionChanged += this.GridModel.OnSortColumnsChanged;

            if (this.View == null)
                return;

            this.GridModel.IsInSort = true;
            if (this.View.SortDescriptions.Any())
            {
                foreach (var sortColumn in this.View.SortDescriptions)
                {
                    var column = this.Columns.FirstOrDefault(x => x.MappingName == sortColumn.PropertyName);
                    this.GridModel.ChangeSortIconVisibility(column, null, NotifyCollectionChangedAction.Reset);
                }
            }

            this.View.SortDescriptions.Clear();
            if (this.SortColumnDescriptions != null && this.SortColumnDescriptions.Any())
            {
                this.View.BeginInit();
                // To add View.SortDescriptions
                this.InitialSort();
                this.View.EndInit();
            }
            
            this.GridModel.IsInSort = false;
            if (this.View.SortDescriptions.Any())
            {
                foreach (var sortColumn in this.View.SortDescriptions)
                {
                    var column = this.Columns.FirstOrDefault(x => x.MappingName == sortColumn.PropertyName);
                    this.GridModel.ChangeSortIconVisibility(column, null, NotifyCollectionChangedAction.Add);
                }
            }
        }

        internal override void OnFrozenColumnCountChanged(DependencyPropertyChangedEventArgs e)
        {
            if (IsInDeserialize)
                return;

            if ((int)e.NewValue > 0 && DetailsViewManager.HasDetailsView)
                throw new NotSupportedException("DetailsView is not supported with FreezePanes");

            if (isGridLoaded && (int)e.NewValue >= 0 && VisualContainer.ColumnCount >= this.ResolveToScrollColumnIndex((int)e.NewValue))
            {
                UpdateFreezePaneColumns();
            }
        }

        internal override void OnShowRowHeaderChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnShowRowHeaderChanged(e);
            if (!this.isGridLoaded)
                return;

            (this.VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();
            if ((bool)e.NewValue)
            {
                this.inRowHeaderChange = true;
                this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.RowHeaderChanged, null));
                this.container.InsertColumns(0, 1);
                this.inRowHeaderChange = false;
                this.container.ColumnWidths[0] = this.RowHeaderWidth;
                this.container.UpdateScrollBars();
                if (this.FrozenColumnCount > 0)
                    this.VisualContainer.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
                else
                    this.VisualContainer.FrozenColumns = 1;
                this.container.NeedToRefreshColumn = true;
                this.container.InvalidateMeasureInfo();
            }
            else
            {
                this.inRowHeaderChange = true;
                this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.RowHeaderChanged, null));

                this.VisualContainer.RemoveColumns(0, 1);
                this.inRowHeaderChange = false;
                if (this.FrozenColumnCount > 0)
                    this.VisualContainer.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
                else
                    this.VisualContainer.FrozenColumns = 0;
                this.container.UpdateScrollBars();
                this.container.NeedToRefreshColumn = true;
                this.container.InvalidateMeasureInfo();
            }
                    (this.VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();
            if (this.VisualContainer.ColumnCount > 0)
            {
                var addNewRow = this.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
                if (addNewRow != null)
                    (addNewRow.WholeRowElement as AddNewRowControl).UpdateTextBorder();
                this.RowGenerator.RefreshStackedHeaders();
                //WPF_33924 While setting ShowRowHeader using binding, the header row is not refreshed on loading initially.
                //So, the RefreshHeaders method is called here.
                this.RowGenerator.RefreshHeaders();
                this.GridColumnSizer.Refresh();
            }
            // WPF-19367 - To refresh detailsview data row while changing RowHeader at runtime 
            if (this is DetailsViewDataGrid && this.NotifyListener != null)
                this.RowGenerator.LineSizeChanged();
        }

        internal override void OnRowHeaderWidthChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded || this.IsInDeserialize)
                return;

            if (this.ShowRowHeader)
            {
                this.VisualContainer.ColumnWidths[0] = (double)(e.NewValue);
                this.GridColumnSizer.Refresh();
            }
        }

        private static void OnSummaryGroupComparerChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (grid.View != null)
            {
                grid.View.BeginInit(false);
                grid.View.GroupComparer = args.NewValue as IComparer<Group>;
                grid.SelectionController.ClearSelections(false);
                grid.View.EndInit();
            }
        }

        private static void OnAddNewRowPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (!grid.isGridLoaded || grid.View == null || grid.IsInDeserialize)
                return;

            var oldValue = (AddNewRowPosition)args.OldValue;
            var newValue = (AddNewRowPosition)args.NewValue;
            if (oldValue != AddNewRowPosition.None)
            {
                grid.SelectionController.CurrentCellManager.EndEdit();
                if (grid.View.IsAddingNew)
                    grid.GridModel.AddNewRowController.CommitAddNew();
            }

            var addNewRow = grid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.AddNewRow);
            if (addNewRow != null)
                addNewRow.RowIndex = -1;

            var lineSizeCollection = grid.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();

            var footerCount = grid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);

            if (!oldValue.Equals(AddNewRowPosition.None))
            {
                if (oldValue.Equals(AddNewRowPosition.FixedTop))
                    grid.VisualContainer.RemoveRows(grid.HeaderLineCount - 1, 1);
                if (oldValue.Equals(AddNewRowPosition.Top))
                    grid.VisualContainer.RemoveRows(grid.HeaderLineCount, 1);
                if (oldValue.Equals(AddNewRowPosition.Bottom))
                {
                    int bottomIndex = grid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount;
                    grid.VisualContainer.RemoveRows(grid.VisualContainer.RowCount - bottomIndex, 1);
                }

            }

            if (newValue.Equals(AddNewRowPosition.FixedTop) || newValue.Equals(AddNewRowPosition.Top))
            {
                grid.VisualContainer.InsertRows(grid.HeaderLineCount, 1);
            }
            else if (newValue.Equals(AddNewRowPosition.Bottom))
            {
                int index = grid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount;
                grid.VisualContainer.InsertRows(grid.VisualContainer.RowCount - index, 1);
            }
            if (grid.CanQueryRowHeight())
                grid.VisualContainer.RowHeightManager.Reset();

            if (grid.CanQueryCoveredRange())
                grid.CoveredCells.Clear();

            lineSizeCollection.ResumeUpdates();

            grid.RefreshHeaderLineCount();
            if (grid.container != null)
            {
                grid.UpdateFreezePaneRows();
            }

            // To update catched row index based on newly added/removed row count
            if (grid.DetailsViewManager.HasDetailsView)
            {
                if (grid.GetAddNewRowPosition() == AddNewRowPosition.Top && oldValue != AddNewRowPosition.Top
                    && oldValue != AddNewRowPosition.FixedTop)
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                  {
                      if (row.CatchedRowIndex != -1)
                          row.CatchedRowIndex += 1;
                  });
                else if (grid.GetAddNewRowPosition() != AddNewRowPosition.Top && (oldValue == AddNewRowPosition.FixedTop
                    || oldValue == AddNewRowPosition.Top))
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= 1;
                    });
            }

            grid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowIndex >= grid.GetHeaderIndex())
                    row.RowIndex = -1;
            });
            grid.ResetUnBoundRowIndex();
            grid.SelectionController.HandleGridOperations(
                new GridOperationsHandlerArgs(GridOperation.AddNewRow,
                    new AddNewRowOperationHandlerArgs(AddNewRowOperation.PlacementChange, args)));

            grid.VisualContainer.InvalidateMeasureInfo();

            //UWP-706 Need to refresh parent grid after change the AddNewRowposition
            if (grid is DetailsViewDataGrid && grid.NotifyListener != null)
                grid.DetailsViewManager.RefreshParentDataGrid(grid);
        }

        private static void OnFilterRowPositionChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            if (!grid.isGridLoaded || grid.View == null || grid.IsInDeserialize || grid.container == null)
                return;

            var oldValue = (FilterRowPosition)args.OldValue;
            var newValue = (FilterRowPosition)args.NewValue;
            if (oldValue != FilterRowPosition.None && grid.SelectionController.CurrentCellManager.IsFilterRow
                && grid.SelectionController.CurrentCellManager.HasCurrentCell
                && grid.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                grid.SelectionController.CurrentCellManager.EndEdit();

            var filterRow = grid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.FilterRow);
            if (filterRow != null)
                filterRow.RowIndex = -1;

            var lineSizeCollection = grid.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();

            var footerCount = grid.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);

            if (!oldValue.Equals(FilterRowPosition.None))
            {
                if (oldValue.Equals(FilterRowPosition.FixedTop))
                    grid.VisualContainer.RemoveRows(grid.HeaderLineCount - 1, 1);
                if (oldValue.Equals(FilterRowPosition.Top))
                    grid.VisualContainer.RemoveRows(grid.HeaderLineCount, 1);
                if (oldValue.Equals(FilterRowPosition.Bottom))
                {
                    int bottomIndex = grid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount
                        + (grid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0);
                    grid.VisualContainer.RemoveRows(grid.VisualContainer.RowCount - bottomIndex, 1);
                }
            }

            if (newValue.Equals(FilterRowPosition.FixedTop) || newValue.Equals(FilterRowPosition.Top))
            {
                grid.VisualContainer.InsertRows(grid.HeaderLineCount, 1);
            }
            else if (newValue.Equals(FilterRowPosition.Bottom))
            {
                int index = grid.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + footerCount
                    + (grid.AddNewRowPosition == AddNewRowPosition.Bottom ? 1 : 0);
                grid.VisualContainer.InsertRows(grid.VisualContainer.RowCount - index, 1);
            }

            if (grid.CanQueryRowHeight())
                grid.VisualContainer.RowHeightManager.Reset();

            if (grid.CanQueryCoveredRange())
                grid.CoveredCells.Clear();

            lineSizeCollection.ResumeUpdates();

            grid.RefreshHeaderLineCount();
            grid.UpdateFreezePaneRows();
            
            // To update catched row index based on newly added/removed row count
            if (grid.DetailsViewManager.HasDetailsView)
            {
                if (grid.GetFilterRowPosition() == FilterRowPosition.Top && oldValue != FilterRowPosition.Top
                    && oldValue != FilterRowPosition.FixedTop)
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex += 1;
                    });
                else if (grid.GetFilterRowPosition() != FilterRowPosition.Top && (oldValue == FilterRowPosition.FixedTop
                    || oldValue == FilterRowPosition.Top))
                    grid.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                    {
                        if (row.CatchedRowIndex != -1)
                            row.CatchedRowIndex -= 1;
                    });
            }

            grid.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowIndex >= grid.GetHeaderIndex())
                    row.RowIndex = -1;
            });
            grid.ResetUnBoundRowIndex();
            grid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.FilterRow, args));
            grid.VisualContainer.InvalidateMeasureInfo();
        }

        internal override void OnAllowResisizingHiddenColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded)
                return;

            if (this.VisualContainer != null && this.AllowResizingColumns && (bool)e.NewValue)
            {
                this.Columns.ForEach(col =>
                {
                    if (col.IsHidden)
                        this.ColumnResizingController.ProcessResizeStateManager(col);
                });
            }
            else if (this.VisualContainer != null)
            {
                this.Columns.ForEach(col => this.ColumnResizingController.ProcessResizeStateManager(col));
            }
        }
        internal override void OnAllowResisizingColumnsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (!this.isGridLoaded || this.IsInDeserialize)
                return;
            if (this.VisualContainer != null && this.AllowResizingHiddenColumns && (bool)e.NewValue)
            {
                this.Columns.ForEach(col =>
                {
                    if (col.IsHidden)
                        this.ColumnResizingController.ProcessResizeStateManager(col);
                });
            }
            else if (this.VisualContainer != null)
            {
                this.Columns.ForEach(col => this.ColumnResizingController.ProcessResizeStateManager(col));
            }
        }
        private static void OnCurrentItemChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
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

        private static void OnSelectionUnitChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var grid = obj as SfDataGrid;
            // WPF-23125-Exception is not throw while Set SelectionUnit as Cell and NavigationMode as Row
            if (grid.NavigationMode == NavigationMode.Row && (GridSelectionUnit)args.NewValue != GridSelectionUnit.Row)
                throw new InvalidOperationException("GridSelectionUnit should be Row when NavigationMode is Row");

            if (grid.SelectionController != null)
            {
                if (grid.View != null && grid.SelectionController.CurrentCellManager.IsAddNewRow)
                {
                    if (grid.View.IsAddingNew)
                        grid.GridModel.AddNewRowController.CommitAddNew();
                    //When changing the SelectionUnit and if the selection in AddNewRow, the state not be changed. Hence the below code 
                    //has been added.
                    grid.GridModel.AddNewRowController.SetAddNewMode(false);
                }

                if (grid.SelectionController.CurrentCellManager.HasCurrentCell && grid.HasView && grid.View.IsEditingItem)
                    grid.SelectionController.CurrentCellManager.EndEdit();

                grid.SelectionController.ClearSelections(false);
                grid.SelectionController = null;
            }

            if ((GridSelectionUnit)args.NewValue == GridSelectionUnit.Row)
                grid.SelectionController = new GridSelectionController(grid);
            else
                grid.SelectionController = new GridCellSelectionController(grid);

            if (grid.DetailsViewManager.HasDetailsView)
            {
                foreach (var detailsview in grid.DetailsViewDefinition)
                {
                    var detailsViewGrid = (detailsview as GridViewDefinition).DataGrid;
                    detailsViewGrid.SelectionUnit = (GridSelectionUnit)args.NewValue;
                }
            }
        }

#endregion

#region Ctor

        static SfDataGrid()
        {
#if WPF
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SfDataGrid), new FrameworkPropertyMetadata(typeof(SfDataGrid)));
            FlowDirectionProperty.OverrideMetadata(typeof(SfDataGrid), new FrameworkPropertyMetadata(OnFlowDirectionChanged));
#endif
        }

        private static void OnFlowDirectionChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs args)
        {
            var grid = dpo as SfDataGrid;

            if (grid == null || !grid.IsLoaded || grid.VisualContainer == null)
                return;

            //Get the VisulaContainer for Remove the Children when the FlowDirection is changed.
            for (int i = 0; i < grid.VisualContainer.RowsGenerator.Items.Count; i++)
            {
                if (grid.VisualContainer.Children.Contains(grid.VisualContainer.RowsGenerator.Items[i].Element))
                    grid.VisualContainer.Children.Remove(grid.VisualContainer.RowsGenerator.Items[i].Element);
            }
        }
        /// <summary>
        /// Initialize a new instance of the SfDataGrid class.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public SfDataGrid()
        {
#if UWP
            base.DefaultStyleKey = typeof(SfDataGrid);
            ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY | ManipulationModes.TranslateRailsX |
                                  ManipulationModes.TranslateRailsY | ManipulationModes.TranslateInertia;
#endif
            this.InitializeCollections();
            // Sets row Generator 
            SetRowGenerator();
            this.MergedCellManager = new MergedCellManager(this);
            InitializeSelectionController();
            this.GridModel = new GridModel(this);
            //this.GridColumnSizer = new GridColumnSizer(this);
            this.ColumnResizingController = new GridColumnResizingController(this);
#if WPF
            this.SearchHelper = new SearchHelper(this);
#endif
            this.GridColumnDragDropController = new GridColumnDragDropController(this);

            this.DetailsViewManager = new DetailsViewGridManager(this);
            this.SerializationController = new SerializationController(this);
            this.GridCopyPaste = new GridCutCopyPaste(this);
            cellRenderers = new GridCellRendererCollection(this);
            ValidationHelper = new ValidationHelper(this);
            unBoundRowCellRenderers = new GridCellRendererCollection(this);
            filterRowCellRenderers = new GridCellRendererCollection(this);
            this.InitializeCellRendererCollection();
        }
        /// <summary>
        /// Unwires the events associated with the SfDataGrid.
        /// </summary>
        protected virtual void UnWireEvents()
        {
            if (!isEventsFired)
                return;

            if (this.SelectionController != null)
                ((GridBaseSelectionController)this.SelectionController).UnWireVisualContainerEvents();

            UnWireViewEvents();
            if (this.GridModel != null)
                this.GridModel.UnWireEvents();
            isEventsFired = false;
        }

        // Due to break issue, below code commented
        //When expanding the group with selection in DetailsViewDataGrid, the view is reused with old grid. Hence the
        //old grid View is dispathed with this method.
        //internal protected virtual void DispatchView()
        //{
        //    this.UnWireEvents();AllowDraggingColumns
        //}

        private bool isEventsFired = false;

        /// <summary>
        /// Wires the events associated with the SfDataGrid.
        /// </summary>
        protected virtual void WireEvents()
        {
            if (isEventsFired)
                return;

            WireDataGridEvents();

            if (this.VisualContainer != null && this.SelectionController != null)
                ((GridBaseSelectionController)this.SelectionController).WireVisualContainerEvents();

            WireViewEvents();
            if (this.GridModel != null)
                this.GridModel.WireEvents();
            isEventsFired = true;
        }

        #endregion

        #region Override methods

        #region OnApplyTemplate   
        /// <summary>
        /// Builds the visual tree for the SfDataGrid when a new template is applied.
        /// </summary>
#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        // 用此方法获取控件中PART_***的控件。
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
#if UWP
            //If visibility of SfDataGrid is Collapsed initially, IsLoaded property is not set true. 
            //therefore InvalidateMeasure is not called for DataRows. So IsLoaded is set true here. 
            this.IsLoaded = true;
#endif
            if (this.container != null)
                this.container.OnItemSourceChanged();
            this.container = GetTemplateChild("PART_VisualContainer") as VisualContainer;
            this.groupDropArea = base.GetTemplateChild("PART_GroupDropArea") as GroupDropArea;
#if WinRT || UNIVERSAL
            if (container != null)
            {
                this.container.ScrollOwner = GetTemplateChild("PART_ScrollViewer") as ScrollViewer;
                this.container.ScrollableOwner = GetTemplateChild("PART_ScrollViewer") as ScrollableContentViewer;
            }
#endif
            if (this.groupDropArea != null)
            {
                this.groupDropArea.dataGrid = this;
                // Apply style for GroupDropArea
                if (this.ReadLocalValue(SfDataGrid.GroupDropAreaStyleProperty) != DependencyProperty.UnsetValue)
                    this.groupDropArea.Style = this.GroupDropAreaStyle;
                else if(this.GroupDropAreaStyle != null)
                    this.groupDropArea.Style = this.GroupDropAreaStyle;
            }

            if (this.ReadLocalValue(SfDataGrid.HeaderStyleProperty) != DependencyProperty.UnsetValue)
                hasHeaderStyle = true;
            if (this.ReadLocalValue(SfDataGrid.CellStyleProperty) != DependencyProperty.UnsetValue)
                hasCellStyle = true;
            if (this.ReadLocalValue(SfDataGrid.UnBoundRowCellStyleProperty) != DependencyProperty.UnsetValue)
                hasUnBoundRowCellStyle = true;
            showRowHeader = this.ShowRowHeader;
            foreach (var column in this.Columns)
            {
                if (column.ReadLocalValue(GridColumn.HeaderStyleProperty) != DependencyProperty.UnsetValue)
                    column.hasHeaderStyle = true;
                if (column.ReadLocalValue(GridColumn.CellStyleProperty) != DependencyProperty.UnsetValue)
                    column.hasCellStyle = true;
            }

            this.RunWork(new Action(() =>
            {
                this.RefreshContainerAndView();
                this.RefreshUnBoundRows();
            }), this.ShowBusyIndicator);
        }

        /// <summary>
        /// Invoked when the dependency property on SfDataGrid has been updated.
        /// </summary>
        /// <param name="dependencyProperty"> 
        /// The dependencyproperty that describes the property that changed.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the dependency property on SfDataGrid has been updates ; otherwise , <b>false</b> .
        /// </returns>
        protected bool IsChanged(DependencyProperty dependencyProperty)
        {
            if (this.ReadLocalValue(dependencyProperty) == DependencyProperty.UnsetValue)
                return false;
            return true;
        }
        /// <summary>
        /// Resets the row index of UnBoundRow and its maintained for internal purpose.
        /// </summary>
        protected internal void ResetUnBoundRowIndex()
        {
            this.UnBoundRows.ForEach(uRow => uRow.RowIndex = -1);
        }

        /// <summary>
        /// method that used to order the unbound rows at UnBoundRows collection.
        /// </summary>
        internal virtual void RefreshUnBoundRows(bool resetUnBoundRowIndex = false)
        {
            int frozenIndex = 0;
            int footerIndex = 0;
            int topBoddyIndex = 0;
            int bottomBoddyIndex = 0;
            var topTableSummaryRowsCount = this.GetTableSummaryCount(TableSummaryRowPosition.Top);
            var bottomTableSummaryRowsCount = this.GetTableSummaryCount(TableSummaryRowPosition.Bottom);

            this.UnBoundRows.ForEach(
            item =>
            {
                switch (item.Position)
                {
                    case UnBoundRowsPosition.Top:
                        {
                            if (item.ShowBelowSummary)
                            {
                                if (item.UnBoundRowIndex == -1 || resetUnBoundRowIndex)
                                    item.UnBoundRowIndex = topBoddyIndex;
                                topBoddyIndex++;
                            }
                            else if (!item.ShowBelowSummary)
                            {
                                if (item.UnBoundRowIndex == -1 || resetUnBoundRowIndex)
                                    item.UnBoundRowIndex = frozenIndex;
                                frozenIndex++;
                            }
                        }
                        break;
                    case UnBoundRowsPosition.Bottom:
                        {
                            if (item.ShowBelowSummary)
                            {
                                if (item.UnBoundRowIndex == -1 || resetUnBoundRowIndex)
                                    item.UnBoundRowIndex = bottomBoddyIndex;
                                bottomBoddyIndex++;
                            }
                            else if (!item.ShowBelowSummary)
                            {
                                if (item.UnBoundRowIndex == -1 || resetUnBoundRowIndex)
                                    item.UnBoundRowIndex = footerIndex;
                                footerIndex++;
                            }
                        }
                        break;
                }
            }
            );
        }
        /// <summary>
        /// Refreshes VisualContainer and View properties based on SfDataGrid property settings.        
        /// </summary>
        /// <example>
        /// The View.CurrentItem updated based on SfDataGrid.CurrentItem property.
        /// </example>
        protected virtual void RefreshContainerAndView()
        {
            if (this.container == null || this.RowGenerator == null)
                return;
#if WinRT || UNIVERSAL
            this.container.ContainerKeydown = OnContainerKeyDown;
#endif
            this.container.DragBorderBrush = this.BorderBrush;
            this.container.DragBorderThickness = this.BorderThickness;
            this.container.AllowFixedGroupCaptions = this.AllowFrozenGroupHeaders;
            this.container.SetRowGenerator(this.RowGenerator);

            this.UnWireEvents();
            if (this.ItemsSource != null)
            {
                this.SetSourceList(this.ItemsSource);
                this.RaiseItemsSourceChanged(null, this.ItemsSource);
            }

            this.UpdateAutoScroller();
            this.RefreshHeaderLineCount();
            this.UpdateRowAndColumnCount(true);
            this.EnsureProperties();
            //WPF-26003- Row will be get selection automatically while we scroll the Vertically 
            this.isGridLoaded = true;
            if (this.View != null)
                this.EnsureViewProperties();
            this.WireEvents();
        }

        //For WPF-20151 ,RefreshHeaderLineCount() marked as protected internal for refresh HeaderLineCount when TableSummaryRow position at Top
        /// <summary>        
        /// Updates the frozen rows count when the internal rows such as AddNewRow, Header, Unbound Row and TableSummaryRow is created.
        /// </summary>
        protected internal virtual void RefreshHeaderLineCount()
        {
            headerLineCount = 1;
            if (StackedHeaderRows != null && StackedHeaderRows.Count > 0)
                headerLineCount += StackedHeaderRows.Count;
            if (AddNewRowPosition == AddNewRowPosition.FixedTop)
                headerLineCount += 1;
            if (FilterRowPosition == FilterRowPosition.FixedTop)
                headerLineCount += 1;
            headerLineCount += this.GetTableSummaryCount(TableSummaryRowPosition.Top);

            // need to update the headerline count with UnBoundDatarow if it is above sumamry at top position.
            var frozenCount = this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false);//, RowRegion.Header);
            headerLineCount += frozenCount;
        }

#endregion

#region MeasureOverride
        /// <summary>
        /// Determines the desired size of the SfDataGrid.
        /// </summary>
        /// <param name="availableSize">
        /// The size that the SfDataGrid can occupy.
        /// </param>
        /// <returns>
        /// The desired size of SfDataGrid. 
        /// </returns>
        protected override Size MeasureOverride(Size availableSize)
        {
#if WinRT || UNIVERSAL

            if (availableSize.Width != 0.0 && availableSize.Height != 0.0)
            {
                if (container != null)
                {
                    var groupDropAreaClipSize = 0d;
                    if (groupDropArea != null && ShowGroupDropArea)
                        groupDropAreaClipSize = groupDropArea.IsExpanded
                                              ? groupDropArea.MaxHeight
                                              : groupDropArea.MinHeight;
                    container.ViewPortSize = new Size(availableSize.Width - (BorderThickness.Left + BorderThickness.Right),
                                                      availableSize.Height - groupDropAreaClipSize);
                }
            }
#endif
            return base.MeasureOverride(availableSize);
        }

#endregion


        /// <summary>
        /// Initialize a new instance of <see cref="RowGenerator"/> .
        /// </summary>        
        protected virtual void SetRowGenerator()
        {
            this.RowGenerator = new RowGenerator(this);
        }

#endregion
#if WPF
       
#region AutomationOverrides
        /// <summary>
        /// Creates and returns an <see cref="T:Syncfusion.UI.Xaml.Grid.AutomationPeerHelper"/> object for the
        /// SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns new instance of <see cref="T:Syncfusion.UI.Xaml.Grid.SfDataGridAutomationPeer"/>
        /// for the SfDataGrid.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            
            if (AutomationPeerHelper.IsScreenReaderRunning == null)
                AutomationPeerHelper.IsScreenReaderRunning = ScreenReader.IsRunning;
            if ((AutomationPeerHelper.IsScreenReaderRunning??false) || AutomationPeerHelper.EnableCodedUI)
                return new SfDataGridAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }

#endregion
#endif
#region Handle selection Operation
#if !WPF
        /// <summary>
        /// Invoked when the <c>KeyDown</c> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e"> 
        /// The <see cref="T:Windows.UI.Xaml.Input.KeyRoutedEventArgs">KeyRoutedEventArgs</see> that contains the event data.
        /// </param>
        /// <remarks>
        /// Handling the keydown operations of SfDataGrid
        /// </remarks>
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            bool result = false;
            //In WinRT, when we are using UP, DOWN and TAB keys the DetailsViewGrid process the operation for the keys directly
            //instead of processing any operation in parent grid. In WPF, the process is done from parent grid, so we have process the
            //operation from Parent grid.
            if (this is DetailsViewDataGrid)
            {
                var parentGrid = this.GetTopLevelParentDataGrid();
                result = parentGrid.SelectionController.HandleKeyDown(e);
            }
            else
                result = SelectionController.HandleKeyDown(e);
            if (result)
                e.Handled = true;
        }
#endif

#if WPF

        protected override void OnContextMenuOpening(ContextMenuEventArgs e)
        {
            base.OnContextMenuOpening(e);
            if (!e.Handled)
            {
                //WPF-32918 Menu key is not working when current cell is null, now modified the code for NavigationMode as Row and
                //process the ContectMenu operation based on datacolumn.
                RowColumnIndex rowColumnIndex = this.SelectionController.CurrentCellManager.CurrentRowColumnIndex;
                if (rowColumnIndex.ColumnIndex < 0 || rowColumnIndex.RowIndex < 0)
                   return;
                if (this.NavigationMode == NavigationMode.Row)
                {                   
                    var dataColumn = this.GetDataColumnBase(rowColumnIndex);
                    if (dataColumn != null)
                        e.Handled = (dataColumn.ColumnElement as GridCell).ShowContextMenu();
                }
                else if(this.SelectionController.CurrentCellManager.CurrentCell != null)
                    e.Handled = (this.SelectionController.CurrentCellManager.CurrentCell.ColumnElement as GridCell).ShowContextMenu();
            }
        }

        /// <summary>
        /// Invoked when the <c>PreviewKeyDown</c> attached event reaches an element in its route that is derived from this class. 
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.KeyEventArgs"/> that contains the event data. 
        /// </param>
        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            // To skip key navigations when view is null
            if (this.View == null && !this.UnBoundRows.Any(item => (item.Position == UnBoundRowsPosition.Bottom && item.ShowBelowSummary == false)
                                                                  || (item.Position == UnBoundRowsPosition.Top && item.ShowBelowSummary == true)))
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
                             // WPF-33982 - DataGrid can be handled Home Key and End Key instead of combobox, if the renderer is ComboBox.
                            if (e.Key != Key.Tab && e.Key != Key.PageDown && e.Key != Key.PageUp && e.Key != Key.Enter && e.Key != Key.Up && e.Key != Key.Down && e.Key != Key.Right && e.Key != Key.Left &&
                                e.Key != Key.Escape && e.Key != Key.F2 && e.Key != Key.Home && e.Key != Key.End)
                                return;
                        }
                        else
                            return;
                    }
                }
            }
            //WPF-20229 - When editing the combox column and while pressing the Enter key the PreviewKeyDown is fired to DetailsViewDataGrid,
            //normally the parent grid will be fired. Hence the below conde is added to invoke the parentGrid preview key down.
            if (this is DetailsViewDataGrid &&  SelectionController.CurrentCellManager.HasCurrentCell && SelectionController.CurrentCellManager.CurrentCell.IsEditing && 
                (e.Key == Key.Tab || e.Key == Key.Enter || e.Key == Key.Up || e.Key == Key.Down))
            {
                var parentGrid = this.GetTopLevelParentDataGrid();
                parentGrid.SelectionController.HandleKeyDown(e);
            }
            else
                SelectionController.HandleKeyDown(e);
            base.OnPreviewKeyDown(e);
        }
        /// <summary>
        /// Invoked when the <c>PreviewMouseDown</c> attached event reaches an element in its route that is derived from this class. 
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs"/>that contains event data.
        /// </param>
        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseDown(e);
            if(this.VisualContainer!=null)
                this.VisualContainer.SuspendManipulationScroll = false;
        }
        /// <summary>
        /// Invoked when the <c>PreviewMouseUp</c> attached event reaches an element in its route that is derived from this class. 
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.MouseButtonEventArgs"/> that contains event data.
        /// </param>
        protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseUp(e);

            var isLeftButtonPressed = e.ChangedButton == MouseButton.Left;
            if (!this.IsKeyboardFocusWithin && isLeftButtonPressed)
                this.Focus();
        }

#endif


#if !WinRT && !WP && !UNIVERSAL
        /// <summary>
        /// Invoked when the <c>TextInput</c> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="System.Windows.Input.TextCompositionEventArgs"/>that contains event data.
        /// </param>
        protected override void OnTextInput(TextCompositionEventArgs e)
        {
            if (!SelectionController.CurrentCellManager.HasCurrentCell)
            {
                base.OnTextInput(e);
                return;
            }
            var rowColumnIndex = SelectionController.CurrentCellManager.CurrentRowColumnIndex;
            var dataRow = RowGenerator.Items.FirstOrDefault(item => item.RowIndex == rowColumnIndex.RowIndex);
            if (dataRow != null && dataRow is DataRow)
            {
                var dataColumn = dataRow.VisibleColumns.FirstOrDefault(column => column.ColumnIndex == rowColumnIndex.ColumnIndex);
                char text;
                char.TryParse(e.Text, out text);

                if(dataColumn != null)
                {
                    if (((!dataColumn.isUnBoundRowCell && dataColumn.GridColumn.IsTemplate && (dataColumn.GridColumn as GridTemplateColumn).hasEditTemplate)
                        ||                      
                        (dataColumn.isUnBoundRowCell && dataColumn.GridUnBoundRowEventsArgs != null && dataColumn.GridUnBoundRowEventsArgs.hasEditTemplate)))                        
                        return;
                
                    if(!dataColumn.IsEditing && char.IsLetterOrDigit(text) && SelectionController.CurrentCellManager.BeginEdit())
                    {
                        dataColumn.Renderer.PreviewTextInput(e);
                        //WPF-37762- While EditElement got focus, we need to handle it here.
                        e.Handled = true;
                    }                   
                }
            }
            base.OnTextInput(e);
        }
#endif

#if !WPF
        /// <summary>
        /// Invoked when the <c>ContainerKeyDown</c> attached event reaches an element in its route that is derived from this class. Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="KeyEventArgs"/> that contains the event data.
        /// </param>        
        protected bool OnContainerKeyDown(KeyEventArgs e)
        {
            //In WinRT, when we are using UP, DOWN and TAB keys the DetailsViewGrid process the operation for the keys directly
            //instead of processing any operation in parent grid. In WPF, the process is done from parent grid, so we have process the
            //operation from Parent grid.
            if (this is DetailsViewDataGrid)
            {
                var parentGrid = this.GetTopLevelParentDataGrid();
                parentGrid.SelectionController.HandleKeyDown(e);
            }
            else
                SelectionController.HandleKeyDown(e);
            return e.Handled;
        }
#endif

        /// <summary>
        /// Invoked when the <c>ManipulationStarted</c> attached event reaches an element in its route that is derived from this class. 
        /// Implement this method to add class handling for this event.
        /// </summary>
        /// <param name="e">
        /// The <see cref="T:Windows.UI.Xaml.Input.ManipulationStartedRoutedEventArgs"/>that contains the event data.
        /// </param>

#if WinRT || UNIVERSAL
        protected override void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
#else
        protected override void OnManipulationStarted(ManipulationStartedEventArgs e)
#endif
        {
            base.OnManipulationStarted(e);
            e.Handled = true;
        }

#endregion

#region Public methods

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
        /// Gets the cell value to populate the UnboundColumn by evaluating the expression or Format with the record.
        /// </summary>
        /// <param name="column">
        /// Specifies the corresponding column to get the cell value.
        /// </param>
        /// <param name="record">
        /// Specifies the corresponding record to get the cell value.
        /// </param>
        /// <returns>
        /// Returns the cell value of the specified column based on Expression or Format with record.
        /// </returns>
        /// <exception cref="System.NotSupportedException">
        /// Thrown when the GridUnBoundColumn is defined with Expression for DataTable .
        /// </exception>
        public object GetUnBoundCellValue(GridColumn column, object record)
        {
            var col = column as GridUnBoundColumn;
            if (col == null || record == null)
                return null;

#if WPF
            //WPF-20153 throws whether the expression used in unbound column with Datatable collection.
            if ((!string.IsNullOrEmpty(col.expression) || !string.IsNullOrEmpty(col.format)) && col.DataGrid.View.IsLegacyDataTable)
                throw new NotSupportedException("Unbound column is not supported in DataTable.So use Expression column in the DataTable");
#endif

            object value = null;

            if (!string.IsNullOrEmpty(col.expression))
                value = col.ComputedValue(record);
            else if (!string.IsNullOrEmpty(col.format))
            {
                value = col.format.FormatByName(null, (key) =>
                {
                    //WPF 20217 - while using unbound column with dynamic collection type, value gets from dynamic properties provider.
                    if (this.View.IsDynamicBound)
                    {
                        var unboundValue = this.View.GetPropertyAccessProvider().GetValue(record, key);
                        return unboundValue;
                    }
                    var itemProperties = this.View.GetItemProperties();
                    if (itemProperties == null)
                        return null;
                    var pd = itemProperties.GetPropertyDescriptor(key);
                    if (pd != null)
                    {
                        return pd.GetValue(record);
                    }
                    else
                    {
                        if (!col.CaseSensitive)
                        {
                            string s1 = key.ToLower();

#if !WPF
                            foreach (var kvp in itemProperties)
                            {
                                if (s1 == kvp.Value.Name.ToLower())
                                {
                                    key = kvp.Value.Name;
                                    break;
                                }
                            }
#else
                            foreach (PropertyDescriptor kvp in itemProperties)
                            {
                                if (s1 == kvp.Name.ToLower())
                                {
                                    key = kvp.Name;
                                    break;
                                }
                            }
#endif
                            if (itemProperties.GetPropertyDescriptor(key) != null)
                                return itemProperties.GetPropertyDescriptor(key).GetValue(record);

                        }
                        throw new NotImplementedException("Not able to evaluate value for " + key);
                    }
                });
                value = value.ToString().Substring(1, value.ToString().Length - 2);
            }
            else
                value = record;

            var args = RaiseQueryUnboundValue(UnBoundActions.QueryData, value, column, record);
            return args.Value;
        }

        /// <summary>
        /// Gets the collection of selected cells in SfDataGrid.
        /// </summary>
        /// <returns>
        /// Returns the collection of <see cref="GridCellInfo"/> that contains the selected cells in SfDataGrid.
        /// </returns>
        public List<GridCellInfo> GetSelectedCells()
        {
            if (this.SelectionUnit == GridSelectionUnit.Cell || this.SelectionUnit == GridSelectionUnit.Any)
            {
                return (this.SelectionController as GridCellSelectionController).GetSelectedCells();
            }
            return new List<GridCellInfo>();
        }
        // #136660 code changes suggested by Akuna Capital
        /// <summary>
        /// Gets the header cell element for the specified column.
        /// </summary>
        /// <param name="column">
        /// Specifies the corresponding column to get header cell element.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridHeaderCellControl"/> element for the specified column.
        /// </returns>
        public GridHeaderCellControl GetHeaderCell(GridColumn column)
        {
            if (RowGenerator.Items.Count > 0)
            {
                var dataRow = RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.GetHeaderIndex());
                if (dataRow != null)
                {
                    var dataColumn = dataRow.VisibleColumns.FirstOrDefault(x => x.GridColumn != null && x.GridColumn.MappingName == column.MappingName && x.GridColumn.Equals(column));
                    if (dataColumn != null)
                        return dataColumn.ColumnElement as GridHeaderCellControl;
                }
            }
            return null;
        }
        /// <summary>
        /// Scrolls the SfDataGrid vertically and horizontally to display the cell for the specified RowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the rowColumnIndex of the cell to scroll into view.
        /// </param>
        public void ScrollInView(RowColumnIndex rowColumnIndex)
        {
            if (rowColumnIndex.RowIndex >= this.VisualContainer.ScrollRows.LineCount || rowColumnIndex.ColumnIndex >= this.VisualContainer.ScrollColumns.LineCount)
                return;
            //No need to call query row height when scrolling from bottom to up. 
            if (!CanQueryRowHeight() || rowColumnIndex.RowIndex < this.VisualContainer.ScrollRows.LastBodyVisibleLineIndex)
                this.VisualContainer.ScrollRows.ScrollInView(rowColumnIndex.RowIndex);
            else
            {
                var height = this.VisualContainer.RowHeights[rowColumnIndex.RowIndex];
                if (this.VisualContainer.RowsGenerator.QueryRowHeight(rowColumnIndex.RowIndex, ref height))
                {
                    this.VisualContainer.RowHeights.SetRange(rowColumnIndex.RowIndex, rowColumnIndex.RowIndex, height);
                    if (height == 0)
                        return;
                }
                if (!this.VisualContainer.ScrollRows.IsLineVisible(rowColumnIndex.RowIndex) || this.VisualContainer.ScrollRows.GetVisibleLineAtLineIndex(rowColumnIndex.RowIndex).IsClipped)
                {
#if WPF
                    this.VisualContainer.SetRowHeights(rowColumnIndex.RowIndex, ExpandDirection.Up);
#else
                    this.VisualContainer.SetRowHeights(rowColumnIndex.RowIndex, Key.Up);
#endif
                    this.VisualContainer.ScrollRows.ScrollInView(rowColumnIndex.RowIndex);
                }
            }

            if (rowColumnIndex.ColumnIndex >= 0)
                this.VisualContainer.ScrollColumns.ScrollInView(rowColumnIndex.ColumnIndex);
            this.VisualContainer.InvalidateMeasureInfo();
        }
        /// <summary>
        /// Selects all the cells in SfDataGrid 
        /// </summary>
        /// <remarks>
        /// This method only works for Multiple and Extended mode selection.
        /// </remarks>
        public void SelectAll()
        {
            this.SelectionController.SelectAll();
        }

        /// <summary>
        /// Clears all the selection present in SfDataGrid.
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
        /// Selects the cell for the specified row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Specifies the corresponding rowData to select the cell.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column to select the cell.
        /// </param>
        public void SelectCell(object rowData, GridColumn column)
        {
            if (this.SelectionUnit != GridSelectionUnit.Row && this.SelectionMode != GridSelectionMode.None)
            {
                (this.SelectionController as GridCellSelectionController).SelectCell(rowData, column);
            }
        }

        /// <summary>
        /// Selects the range of cells for the specified row data and column information .     
        /// </summary>
        /// <param name="startRowData">
        /// Specifies the top position of the range to select the cells.
        /// </param>
        /// <param name="startColumn">
        /// Specifies the left position of the range to select the cells.
        /// </param>
        /// <param name="endRowData">
        /// Specifies the bottom position of the range to select the cells..
        /// </param>
        /// <param name="endColumn">
        /// Specifies the right position of the range to select the cells.
        /// </param>
        /// <remarks>
        /// This method is not applicable for Single and None selection mode.
        /// </remarks>
        public void SelectCells(object startRowData, GridColumn startColumn, object endRowData, GridColumn endColumn)
        {
            if (this.SelectionUnit != GridSelectionUnit.Row && this.SelectionMode != GridSelectionMode.None && this.SelectionMode != GridSelectionMode.Single)
            {
                (this.SelectionController as GridCellSelectionController).SelectCells(startRowData, startColumn, endRowData, endColumn);
            }
        }

        /// <summary>
        /// Unselects the cell with corresponding to the specified row data and column.
        /// </summary>
        /// <param name="rowData">
        /// Specifies the corresponding rowData to unselect.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column to unselect.
        /// </param>
        public void UnSelectCell(object rowData, GridColumn column)
        {
            if (this.SelectionUnit != GridSelectionUnit.Row && this.SelectionMode != GridSelectionMode.None)
            {
                (this.SelectionController as GridCellSelectionController).UnSelectCell(rowData, column);
            }
        }

        /// <summary>
        /// Unselects the range of cells for the specified  row data and column information.       
        /// </summary>
        /// <param name="startRowData">
        /// Specifies the top position of the range to unselect.
        /// </param>
        /// <param name="startColumn">
        /// Specifies the left position of the range to unselect.
        /// </param>
        /// <param name="endRowData">
        /// Specifies the bottom position of the range to unselect.
        /// </param>
        /// <param name="endColumn">
        /// Specifies the right position of the range to unselect.
        /// </param>
        /// <remarks>
        /// This method is not applicable for Single and None selection mode.
        /// </remarks>
        public void UnSelectCells(object startRowData, GridColumn startColumn, object endRowData, GridColumn endColumn)
        {
            if (this.SelectionUnit != GridSelectionUnit.Row && this.SelectionMode != GridSelectionMode.None && this.SelectionMode != GridSelectionMode.Single)
            {
                (this.SelectionController as GridCellSelectionController).UnSelectCells(startRowData, startColumn, endRowData, endColumn);
            }
        }

#if WPF
        /// <summary>
        /// Displays the print preview window that previews the SfDataGrid before that is ready for print operation.
        /// </summary>        
        public void ShowPrintPreview()
        {
            var window = new ChromelessWindow
            {
                Content = new GridPrintPreviewControl(this, PrintSettings.PrintManagerBase),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            SkinStorage.SetEnableOptimization(window, false);
            SkinStorage.SetVisualStyle(window, "Metro");
            if (PrintSettings.PrintPreviewWindowStyle != null)
                window.Style = PrintSettings.PrintPreviewWindowStyle;
            else
            {
                var resources = new ResourceDictionary
                {
                    Source =
                        new Uri("/Syncfusion.SfGrid.WPF;component/Print/Themes/Generic.xaml", UriKind.RelativeOrAbsolute)
                };

                window.Style = resources["ChromelessWindowStyle"] as Style;
            }
            window.ShowDialog();
        }
#endif

        /// <summary>
        /// Prints the SfDataGrid with default print settings.
        /// </summary>
        public void Print()
        {
            if (PrintSettings.PrintManagerBase == null)
                PrintSettings.PrintManagerBase = new GridPrintManager(this);
#if UWP
            else
                PrintSettings.PrintManagerBase.RegisterForPrinting();
#endif
            PrintSettings.PrintManagerBase.Print();
        }

#if WPF
            /// <summary>
            /// Gets the document that is ready for printing . 
            /// </summary>
            /// <return>
            /// Returns the FixedDocument that is ready for printing.
            ///</return>
        public FixedDocument GetPrintDocument()
        {
            try
            {
                if (PrintSettings.PrintManagerBase == null)
                    PrintSettings.PrintManagerBase = new GridPrintManager(this);
                PrintSettings.PrintManagerBase.InitializePrint(!PrintSettings.PrintManagerBase.isPagesInitialized);
                return PrintSettings.PrintManagerBase.GetPrintDocument(1, PrintSettings.PrintManagerBase.pageCount);
            }
            finally
            {
                if (PrintSettings.PrintManagerBase != null)
                    PrintSettings.PrintManagerBase = null;
            }
        }

        /// <summary>
        /// Gets the print document for the specified start and end index of page.
        /// </summary>
        /// <param name="pageStartIndex">
        /// Specifies the start index of page.
        /// </param>
        /// <param name="pageEndIndex">
        /// Specifies the end index of page.
        /// </param>
        /// <return>
        /// Returns the print document corresponding to the specified start and end index of page.
        /// </return>
        public FixedDocument GetPrintDocument(int pageStartIndex, int pageEndIndex)
        {
            try
            {
                if (PrintSettings.PrintManagerBase == null)
                    PrintSettings.PrintManagerBase = new GridPrintManager(this);
                PrintSettings.PrintManagerBase.InitializePrint(!PrintSettings.PrintManagerBase.isPagesInitialized);
                return PrintSettings.PrintManagerBase.GetPrintDocument(pageStartIndex, pageEndIndex);
            }
            finally
            {
                if (PrintSettings.PrintManagerBase != null)
                    PrintSettings.PrintManagerBase = null;

            }
        }
#endif
        /// <summary>
        /// Method which helps to make the Grouping by passing the column name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="converter"></param>
        /// <param name="comparer"></param>
        /// <remarks></remarks>
        internal void GroupBy(string columnName, IValueConverter converter, IComparer<Object> comparer)
        {
            //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
            this.GridModel.GroupBy(columnName, converter, comparer);

            //While made new group all the selection should clear and selection should maintained in 1st row.
            this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, new GridGroupingEventArgs(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, columnName, 0)) { IsProgrammatic = false }));
        }

        /// <summary>
        /// Method which helps to make the Grouping by passing the column name and its position.
        /// </summary>
        /// <param name="columnName"></param>
        /// <param name="converter"></param>
        /// <param name="comparer"></param>
        /// <remarks></remarks>
        internal void GroupBy(string columnName, int insertAt, IValueConverter converter, IComparer<object> comparer)
        {
            //Added the Comparer Property in GroupColumnDescription class to sort the column which is in Grouping.
            this.GridModel.GroupBy(columnName, insertAt, converter, comparer);

            //While made new group all the selection should clear and selection should maintained in 1st row.
            this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, new GridGroupingEventArgs(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, columnName, insertAt)) { IsProgrammatic = false }));
        }

        /// <summary>
        /// Methos which helps you to remove the grouping by passing the column name.
        /// </summary>
        /// <param name="columnName"></param>
        /// <remarks></remarks>
        internal void RemoveGroup(string columnName)
        {
            this.GridModel.RemoveGroup(columnName);
            this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, new GridGroupingEventArgs(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, columnName, 0)) { IsProgrammatic = false }));
        }

        /// <summary>
        /// Expands all the groups in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method expand all the groups in SfDataGrid programmatically.
        /// </remarks>
        public void ExpandAllGroup()
        {
            if (this.View != null && this.GridModel.HasGroup)
            {
                this.View.TopLevelGroup.ExpandAll();
                this.UpdateRowCountAndScrollBars();
                this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, null));
                this.GridModel.RefreshDataRow();
                //WPF-22497-To Refresh the parent grid when group is expanded
                this.DetailsViewManager.RefreshParentDataGrid(this);
            }
        }

        /// <summary>
        /// Collapses all the groups in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This method collapse all the groups in SfDataGrid programmatically.
        /// </remarks>
        public void CollapseAllGroup()
        {
            if (this.View != null && this.GridModel.HasGroup)
            {
                this.View.TopLevelGroup.CollapseAll();
                this.SelectionController.ClearSelections(false);
                this.UpdateRowCountAndScrollBars();
                this.GridModel.RefreshDataRow();
                //WPF-22497-To Refresh the parent grid when group is collapsed 
                this.DetailsViewManager.RefreshParentDataGrid(this);
            }
        }

        /// <summary>
        /// Expands the group based on its level.
        /// </summary>
        /// <param name="groupLevel">
        /// Specifies the group level to expand the group.
        /// </param>       
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        ///  this.dataGrid.ExpandGroupsAtLevel(2);
        /// ]]></code>
        /// </example>
        public void ExpandGroupsAtLevel(int groupLevel)
        {
            //WPF - 23367 - if TopLevelGroup is null, then skip to avoid exception.
            if (this.View.TopLevelGroup != null && groupLevel <= this.View.TopLevelGroup.GetMaxLevel())
            {
                this.GridModel.ExpandGroupsAtLevel(this.View.TopLevelGroup.Groups, groupLevel);
                this.UpdateRowCountAndScrollBars();
                this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, null));
                this.GridModel.RefreshDataRow();
                //WPF-22497-To Refresh the parent grid when group is expanded
                this.DetailsViewManager.RefreshParentDataGrid(this);
            }
        }

        /// <summary>
        /// Collapses the group based on its level.
        /// </summary>
        /// <param name="groupLevel">
        /// Specifies the group level to collapse the group.
        /// </param>     
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        ///  this.dataGrid.CollapseGroupsAtLevel(2);
        /// ]]></code>
        /// </example>
        public void CollapseGroupsAtLevel(int groupLevel)
        {
            //WPF - 23367 - if TopLevelGroup is null, then skip to avoid exception.
            if (this.View.TopLevelGroup != null && groupLevel <= this.View.TopLevelGroup.GetMaxLevel())
            {
                this.GridModel.CollapseGroupsAtLevel(this.View.TopLevelGroup.Groups, groupLevel);
                this.UpdateRowCountAndScrollBars();
                this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, null));
                this.GridModel.RefreshDataRow();
                //WPF-22497-To Refresh the parent grid when group is collapsed
                this.DetailsViewManager.RefreshParentDataGrid(this);
            }
        }

        /// <summary>
        /// Expands the specified group.
        /// </summary>
        /// <param name="group">
        /// Specifies the group to expand it from view.
        /// </param>   
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        ///  var group = (dataGrid.View.Groups[0] as Group);
        ///    this.dataGrid.ExpandGroup(group);
        /// ]]></code>
        /// </example>
        public void ExpandGroup(Group group)
        {
            if (this.GridModel != null)
            {
                this.GridModel.ExpandGroup(group);
                //this.UpdateRowCountAndScrollBars();
                //this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, null));
                this.GridModel.RefreshDataRow();
            }
        }

        /// <summary>
        /// Collapses the specified Group.
        /// </summary>
        /// <param name="group">
        /// Specifies the group to collapse it from view.
        /// </param>       
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        ///  var group = (dataGrid.View.Groups[0] as Group);
        ///    this.dataGrid.CollapseGroup(group);
        /// ]]></code>
        /// </example>
        public void CollapseGroup(Group group)
        {
            if (this.GridModel != null)
            {
                this.GridModel.CollapseGroup(group);
                //this.UpdateRowCountAndScrollBars();
                //this.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Grouping, null));
                this.GridModel.RefreshDataRow();
            }
        }

        /// <summary>
        /// Invalidates the height of the specified row to raise in the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryRowHeight"/> event programmatically.
        /// </summary>
        /// <param name="rowIndex">
        /// specifies the corresponding row index to invalidate its row height.
        /// </param>
        /// <remarks>
        /// Resets the particular row height.
        /// Once row heights are reset, need to call the InvalidateMeasureInfo method of <see cref="Syncfusion.UI.Xaml.Grid.VisualContainer"/> to refresh the view.
        /// </remarks>
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        /// using Syncfusion.UI.Xaml.Grid.Helpers;
        /// dataGrid.InvalidateRowHeight(2);
        /// dataGrid.GetVisualContainer().InvalidateMeasureInfo();
        /// ]]></code>
        /// </example>
        public void InvalidateRowHeight(int rowIndex)
        {
            if (this.container == null)
                return;
            this.container.RowHeightManager.SetDirty(rowIndex);
        }
        /// <summary>
        /// Invalidates the unbound row to refresh the data in View. 
        /// </summary>
        /// <param name="unBoundRow">
        /// Specifies the GridUnBoundRow to be invalidated.
        /// </param>
        /// <param name="canInvalidateColumn">
        /// Specifies to whether to in invalidate columns or not.
        /// </param>             
        public void InValidateUnBoundRow(GridUnBoundRow unBoundRow, bool canInvalidateColumn = true)
        {
            if (this.RowGenerator == null)
                return;

            if (this.UnBoundRows.Count == 0)
                return;

            var rowIndex = this.ResolveUnboundRowToRowIndex(unBoundRow);
            this.RowGenerator.Items.ForEach
            (item =>
                {
                    if (item.RowType != RowType.UnBoundRow || item.RowIndex != rowIndex)
                        return;

                    item.RowIndex = -1;
                    (item as UnBoundRow).isDirty = true;
                    if (!canInvalidateColumn)
                        return;

                    item.VisibleColumns.ForEach(column =>
                    {
                        if (!(column.ColumnElement is GridRowHeaderCell))
                            column.ColumnIndex = -1;
                    });
                }
            );
        }
        
        /// <summary>
        /// Serializes the SfDataGrid control to the XML document file that are stored in the specified Stream.
        /// </summary>
        /// <param name="stream">
        /// Specifies the stream used to write the XML document file.
        /// </param>
        public void Serialize(Stream stream)
        {
            var options = new SerializationOptions();
            this.SerializationController.Serialize(stream, options);
        }
        /// <summary>
        /// Serializes the SfDataGrid with <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> to the XML document file that are stored in the specified Stream         
        /// </summary>
        /// <param name="stream">
        /// Specifies stream used to write XML document file.
        /// </param>
        /// <param name="options">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/> to decide the type of operations such as sorting ,filtering ,and etc to be serialized.
        /// </param>
        public void Serialize(Stream stream, SerializationOptions options)
        {
            this.SerializationController.Serialize(stream, options);
        }

        /// <summary>
        /// Deserializes the SfDataGrid based on the XML document contained by the specified Stream.
        /// </summary>
        /// <param name="stream">
        /// Specifies the XML document to deserialize.
        /// </param>
        public void Deserialize(Stream stream)
        {
            var options = new DeserializationOptions();
            this.SerializationController.Deserialize(stream, options);
        }

        /// <summary>
        /// Deserializes the SfDataGrid based on the XML document of the specified Stream with <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/>.
        /// </summary>
        /// <param name="stream">
        /// Contains the XML document to deserialize.
        /// </param>
        /// <param name="options">
        /// Decides the type of operations such as sorting ,filtering ,and etc to be deserialized.
        /// </param>
        public void Deserialize(Stream stream, DeserializationOptions options)
        {
            this.SerializationController.Deserialize(stream, options);
        }

        /// <summary>
        /// Clears filters for all the columns in SfDataGrid
        /// </summary>
        /// <remarks>
        /// This method will clear the filters programmatically .
        /// </remarks>
        public void ClearFilters()
        {
            if (this.Columns != null)
                this.Columns.ForEach(column => this.GridModel.ClearFilters(column));
        }

        /// <summary>
        /// Clears filters for the particular column 
        /// </summary>
        /// <param name="columnName">
        /// The corresponding column's MappingName to clear filters
        /// </param>
        /// <remarks>
        /// This is the programmatic way to clear the filtering for the particular column.The columnName should be valid MappingName of the column.
        /// </remarks>
        public void ClearFilter(string columnName)
        {
            if (this.Columns != null)
                this.GridModel.ClearFilters(this.Columns.FirstOrDefault(column => column.MappingName == columnName));
        }

        /// <summary>
        /// Clears filters for the particular column
        /// </summary>
        /// <param name="column">
        /// The corresponding column to clear the filters
        /// </param>
        /// <remarks>This is the programmatic way to clear the filters for the particular column.</remarks>
        public void ClearFilter(GridColumn column)
        {
            ClearFilter(column.MappingName);
        }

#if WinRT || UNIVERSAL
        /// <summary>
        /// Serializes the SfDataGrid to the XML document file that are stored in the specified storageFile.
        /// </summary>
        /// <param name="storageFile">
        /// The storageFile used to write XML document. 
        /// </param>
        public async void Serialize(StorageFile storageFile)
        {
            using (var stream = await storageFile.OpenStreamForWriteAsync())
            {
                var serializationOptions = new SerializationOptions();
                Serialize(stream);
            }
        }
        /// <summary>
        /// Serializes the SfDataGrid control to the XML document file that are stored in the specified storageFile with <see cref="Syncfusion.UI.Xaml.Grid.SerializationOptions"/>.
        /// </summary>        
        /// <param name="storageFile">
        /// The storageFile used to write XML document.
        /// </param>
        /// <param name="serializationOptions">
        /// The options for specifying the type of operations such as sorting ,filtering ,and etc to be serialized.
        /// </param>
        public async void Serialize(StorageFile storageFile, SerializationOptions serializationOptions)
        {
            using (var stream = await storageFile.OpenStreamForWriteAsync())
            {
                Serialize(stream, serializationOptions);
            }
        }

        /// <summary>
        /// Deserializes the SfDataGrid based on the XML document in the specified storageFile.
        /// </summary>
        /// <param name="storageFile">
        /// The storageFile that contains the XML document to deserialize.
        /// </param>
        public async void Deserialize(StorageFile storageFile)
        {
            using (var stream = await storageFile.OpenStreamForReadAsync())
            {
                var deserializationOptions = new DeserializationOptions();
                Deserialize(stream);
            }
        }
        /// <summary>
        /// Deserializes the SfDataGrid based on the XML document specified in the Stream with the <see cref="Syncfusion.UI.Xaml.Grid.DeserializationOptions"/>.
        /// </summary>
        /// <param name="storageFile">
        /// The storageFile that contains the XML document to deserialize.
        /// </param>
        /// <param name="deserializationOptions">
        /// The options for specifying the type of operations such as sorting ,filtering ,and etc to be deserialized.
        /// </param>
        public async void Deserialize(StorageFile storageFile, DeserializationOptions deserializationOptions)
        {
            using (var stream = await storageFile.OpenStreamForReadAsync())
            {
                Deserialize(stream, deserializationOptions);
            }
        }
#endif
        /// <summary>
        /// Moves the current cell for the specified rowColumnIndex.
        /// </summary>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex to move the current cell.
        /// </param>
        /// <param name="needToClearSelection">
        /// Indicates whether the current selection to be cleared while moving the current cell.
        /// </param>
        /// <remarks>
        /// This method is not applicable when the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionUnit"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.NavigationMode"/> is Row.
        /// </remarks>   
        public void MoveCurrentCell(RowColumnIndex rowColumnIndex, bool needToClearSelection = true)
        {
            this.SelectionController.MoveCurrentCell(rowColumnIndex, needToClearSelection);
        }

#endregion

#region Public Events
        /// <summary>
        /// Occurs when the ItemsSource changed.
        /// </summary>        
        /// <remarks>
        /// The SfDataGrid.View can be accessed here.   
        /// </remarks>
        public event GridItemsSourceChangedEventHandler ItemsSourceChanged;

        /// <summary>
        /// Occurs when the column is being sorted in SfDataGrid .
        /// </summary>       
        /// <remarks>
        /// You can cancel or customize the column being sorted using the <see cref="Syncfusion.UI.Xaml.Grid.GridSortColumnsChangingEventArgs"/> event argument.
        /// It will be raised for UI based sorting only.
        /// </remarks>
        public event GridSortColumnsChangingEventHandler SortColumnsChanging;

        /// <summary>
        /// Occurs after the column is sorted in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SortColumnsChanging"/> event if that event is not cancelled.        
        /// </remarks>        
        public event GridSortColumnsChangedEventHandler SortColumnsChanged;

        /// <summary>
        /// Occurs when the current selection changes. 
        /// </summary>       
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectionChanging"/> event if that event is not canceled. 
        /// It will be raised both UI and programmatic selection.
        /// </remarks>
        public event GridSelectionChangedEventHandler SelectionChanged;

        /// <summary>
        /// Occurs when the selection is being changed in SfDataGrid. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the selection being changed through the <see cref="Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs"/> event argument.        
        /// It will be raised both UI and programmatic selection. 
        /// </remarks>
        public event GridSelectionChangingEventHandler SelectionChanging;
        
        /// <summary>
        /// Occurs when the record is being deleted using <b>Delete</b> key. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the record being deleted through the <see cref="Syncfusion.UI.Xaml.Grid.RecordDeletingEventArgs"/> event argument.
        /// </remarks>
        public event RecordDeletingEventHandler RecordDeleting;

        /// <summary>
        /// Occurs after the record is deleted using <b>Delete</b> key. 
        /// </summary>  
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RecordDeleting"/> event if that event is not canceled. 
        /// </remarks>
        public event RecordDeletedEventHandler RecordDeleted;

        /// <summary>
        /// Occurs when the group is being expanded.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the group being expanded through <see cref="Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs"/> event argument
        /// and it will not raised when the group is expanded programmatically. 
        /// </remarks>
        public event GroupChangingEventHandler GroupExpanding;

        /// <summary>
        /// Occurs when the group is being collapsed. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the group being collapsed through <see cref="Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs"/> event argument
        /// and it will not raise when the group is collapsed programmatically. 
        /// </remarks>
        public event GroupChangingEventHandler GroupCollapsing;

        /// <summary>
        /// Occurs after the group is expanded. 
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupExpanding"/> event if that event is not canceled 
        /// and it will not raised when the group is expanded programmatically.
        /// </remarks>
        public event GroupChangedEventHandler GroupExpanded;

        /// <summary>
        /// Occurs after the group is collapsed. 
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GroupCollapsing"/> event if that event is not canceled
        /// and it will not raise when the group is collapsed programmatically.
        /// </remarks>
        public event GroupChangedEventHandler GroupCollapsed;

        /// <summary>
        /// Occurs when the column width changes during resizing.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being resized through <see cref="Syncfusion.UI.Xaml.Grid.ResizingColumnsEventArgs"/> event argument .
        /// </remarks>
        public event ResizingColumnsEventHandler ResizingColumns;

        /// <summary>        
        /// Occurs to query or commit the values for GridUnBoundColumn when its cells is initialized or committed.
        /// Occurs when the value of each cell in GridUnBoundColumn being initialized or committed.
        /// </summary>
        /// <remarks>
        /// You can customize the value of GridUnBoundColumn being initialized in the <b>QueryUnboundColumnValue</b> event handler.
        /// </remarks>
        public event QueryUnbounColumnValueHandler QueryUnboundColumnValue;

        /// <summary>
        /// Occurs to query and commit the value and settings for cell in Unbound row.
        /// </summary>
        public event QueryUnBoundRowHandler QueryUnBoundRow;

        /// <summary>
        /// Occurs when the column is being reordered in to a new position.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being dragged through <see cref="Syncfusion.UI.Xaml.Grid.QueryColumnDraggingEventArgs"/> event argument.
        /// </remarks>
        public event QueryColumnDraggingEventHandler QueryColumnDragging;

        /// <summary>
        /// Occurs to query the range to merge, for particular cell.
        /// </summary>
        public event GridQueryCoveredRangeEventHandler QueryCoveredRange;

        /// <summary>
        /// Occurs when the user clicks or touch the cell in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// CellTapped does not occurs when the end user clicks or touch the non-selectable cells. 
        /// </remarks>
        public event GridCellTappedEventHandler CellTapped;

        /// <summary>
        /// Occurs when the user hover the mouse on the cell in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// CellToolTipOpening does not occurs when the tooltip not enabled for the cells. 
        /// </remarks>
        public event GridCellToolTipOpeningEventHandler CellToolTipOpening;
        /// <summary>
        /// Occurs when the user double clicks or touch the cell in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// CellDoubleTapped does not occurs when the end user double clicks or touch the non-selectable cells. 
        /// </remarks>
        public event GridCellDoubleTappedEventHandler CellDoubleTapped;

#if WPF
        /// <summary>
        /// Occurs when the database exception happened.
        /// </summary>                        
        public event ExceptionThrownEventHandler ExceptionThrown;
#endif
        
        /// <summary>
        /// Occurs when the selected cells or rows in SfDataGrid is being copied in to clipboard.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the content being copied from a SfDataGrid through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> event argument.
        /// </remarks>
        public event GridCopyPasteEventHandler GridCopyContent;

        /// <summary>
        /// Occurs when the clipboard value is being pasted to SfDataGrid. 
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the content is being pasted from clipboard to SfDataGrid through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteEventArgs"/> event argument.
        /// </remarks>
        public event GridCopyPasteEventHandler GridPasteContent;

        /// <summary>
        /// Occurs when each cell in the selected cells or rows being copied from SfDataGrid into clipboard.
        /// </summary>        
        /// <remarks>
        /// You can cancel or customize each cell is being copied from the selected cells or rows through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/> event argument. 
        /// </remarks>
        public event GridCopyPasteCellEventHandler CopyGridCellContent;

        /// <summary>
        /// Occurs when each cell is being pasted from clipboard to SfDataGrid control.
        /// </summary>        
        /// <remarks>
        /// You can cancel or customize each cell is being pasted from the clipboard through <see cref="Syncfusion.UI.Xaml.Grid.GridCopyPasteCellEventArgs"/> event argument. 
        /// </remarks>
        public event GridCopyPasteCellEventHandler PasteGridCellContent;

        /// <summary>
        /// Occurs after the column is filtered in SfDataGrid. 
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterChanging"/> event if that event is not canceled.         
        /// </remarks>
        public event GridFilterEventHandler FilterChanged;

        /// <summary>
        /// Occurs when the column is being filtered in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being filtered through <see cref="Syncfusion.UI.Xaml.Grid.GridFilterEventArgs"/> event argument.
        /// </remarks>
        public event GridFilterEventHandler FilterChanging;

        /// <summary>
        /// Occurs when any shortcut menu on the SfDataGrid is opening. 
        /// </summary>
        /// <remarks>
        /// You can handle or change the shortcut menu being opened through the <see cref="Syncfusion.UI.Xaml.Grid.GridContextMenuEventArgs"/> event argument.
        /// </remarks>
        public event GridContextMenuOpeningEventHandler GridContextMenuOpening;

        /// <summary>
        /// Occurs after the items is populated to the FilterControl in SfDataGrid. 
        /// </summary>     
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.FilterItemsPopulating"/> event if that event is not canceled.         
        /// </remarks>
        public event GridFilterItemsPopulatedEventHandler FilterItemsPopulated;

        /// <summary>
        /// Occurs when the items is being populated to the FilterControl in SfDataGrid. 
        /// </summary>
        /// <remarks>
        /// You can customize the data source of FilterControl being populated through the <see cref="Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatingEventArgs"/> event argument.
        /// </remarks>
        public event GridFilterItemsPopulatingEventHandler FilterItemsPopulating;

        /// <summary>
        ///  Occurs when the current cell is being activated in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// You can cancel the current cell is being activated through the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellActivatingEventArgs"/> event argument.
        /// </remarks>
        public event CurrentCellActivatingEventHandler CurrentCellActivating;

        /// <summary>
        /// Occurs after the current cell is activated in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellActivating"/> event if that event is not canceled.         
        /// </remarks>
        public event CurrentCellActivatedEventHandler CurrentCellActivated;

        /// <summary>
        /// Occurs when the edit mode starts for the current cell in SfDataGrid.
        /// </summary>
        /// <remarks>
        /// You can cancel the current cell is being edited through the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellBeginEditEventArgs"/> event argument.
        /// </remarks>
        public event CurrentCellBeginEditEventHandler CurrentCellBeginEdit;

        /// <summary>
        /// Occurs when the edit mode ends for the current cell.
        /// </summary>
        public event CurrentCellEndEditEventHandler CurrentCellEndEdit;

        /// <summary>
        /// Occurs while moving to other cells from edited cell for validating the user input.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the current cell being validated through the <see cref="Syncfusion.UI.Xaml.Grid.CurrentCellValidatingEventArgs"/> event argument.
        /// </remarks>
        public event CurrentCellValidatingEventHandler CurrentCellValidating;

        /// <summary>
        /// Occurs when the SelectedItem changed in the drop down of GridMultiColumnDropDownList or GridComboBoxColumn.
        /// </summary>
        public event CurrentCellDropDownSelectionChangedEventHandler CurrentCellDropDownSelectionChanged;

        /// <summary>
        /// Occurs when the GridHyperLinkColumn's cell is request for navigation.
        /// </summary>
        public event CurrentCellRequestNavigateEventHandler CurrentCellRequestNavigate;

        /// <summary>
        /// Occurs when the current cell value Changes.
        /// </summary>
        public event CurrentCellValueChangedEventHandler CurrentCellValueChanged;

        /// <summary>
        /// Occurs when the current cell is validated.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CurrentCellValidating"/> event if that event is not canceled.         
        /// </remarks>
        public event CurrentCellValidatedEventHandler CurrentCellValidated;

        /// <summary>
        /// Occurs while the moving from the edited row to validate the user input.
        /// </summary>
        /// <remarks>
        /// You can cancel the row is being validated through the <see cref="Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs"/> event argument.
        /// </remarks>
        public event RowValidatingEventHandler RowValidating;

        /// <summary>
        /// Occurs after the row is validated.
        /// </summary>
        /// <remarks>
        /// This event occurs after the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.RowValidating"/> event if that event is not canceled.         
        /// </remarks>
        public event RowValidatedEventHandler RowValidated;

        /// <summary>
        /// Occurs when the AddNewRow is being initiated.
        /// </summary>
        /// <remarks>
        /// You can set the default value for the AddNewRow is being initiated through the <see cref="Syncfusion.UI.Xaml.Grid.AddNewRowInitiatingEventArgs"/> event argument. 
        /// </remarks>
        public event AddNewRowInitiatingEventHandler AddNewRowInitiating;

        /// <summary>
        /// Occurs when column is generated for the properties in underlying data object.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the column being created using the <see cref="Syncfusion.UI.Xaml.Grid.AutoGeneratingColumnArgs"/> event argument.
        /// </remarks>
        public event AutoGeneratingColumnEventHandler AutoGeneratingColumn;

        /// <summary>
        /// Occurs to query the row height based on row index. This event triggered only for visible rows. 
        /// </summary>
        /// <remarks>
        /// You can return the row height based on cell content using <see cref="Syncfusion.UI.Xaml.Grid.GridColumnSizer.GetAutoRowHeight"/> method.
        /// </remarks>
        public event QueryRowHeightEventHandler QueryRowHeight;

        internal bool CanQueryRowHeight()
        {
            return QueryRowHeight != null && !this.DetailsViewManager.HasDetailsView;
        }
        /// <summary>
        /// Decides whether the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event can be wired or not.
        /// </summary>
        /// <returns>
        /// <b> true</b> if the QueryUnBoundRow event is wired ; otherwise , <b>false</b>.
        /// </returns>        
        public bool CanQueryUnBoundRow()
        {
            if (NotifyListener != null && !IsSourceDataGrid)
                // If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid. In this, OriginalSender will be actual DetailsViewDataGrid
                return NotifyListener.SourceDataGrid.QueryUnBoundRow != null;
            
            // its for the event in UI.
            return QueryUnBoundRow != null;
        }


        /// <summary>
        /// Decides whether the  <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event is wired or not.
        /// </summary>
        /// <returns>
        /// <b> true</b> if the QueryCoveredRange event is wired ; otherwise , <b>false</b>.
        /// </returns>  
        public bool CanQueryCoveredRange()
        {
            if (NotifyListener != null && !IsSourceDataGrid)
                // If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid. In this, OriginalSender will be actual DetailsViewDataGrid
                return NotifyListener.SourceDataGrid.QueryCoveredRange != null;
            
            // its for the event in UI.
            return this.QueryCoveredRange != null;
        }
        internal override bool CanCellTapped()
        {
            if (NotifyListener != null && !IsSourceDataGrid)
                // If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid. In this, OriginalSender will be actual DetailsViewDataGrid
                return NotifyListener.SourceDataGrid.CellTapped != null;
            // its for the event in UI.
            return this.CellTapped != null;
        }

        internal override bool CanCellDoubleTapped()
        {
            if (NotifyListener != null && !IsSourceDataGrid)
                // If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid. In this, OriginalSender will be actual DetailsViewDataGrid
                return NotifyListener.SourceDataGrid.CellDoubleTapped != null;
            // its for the event in UI.
            return this.CellDoubleTapped != null;
        }
        internal override bool CanCellToolTipOpening()
        {
            if (this.CellToolTipOpening != null)
                return true;
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                {
                    if (NotifyListener.SourceDataGrid.CanCellToolTipOpening())
                        return true;

                    //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                    if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                    {
                        var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                        if (parentGrid != null && parentGrid.CanCellToolTipOpening())
                            return true;
                    }
                }
            }
            return false;
        }

#if WPF
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ExceptionThrown"/> event in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.ExceptionThrownEventArgs"/> that contains the event data.        
        /// </param>
        internal void RaiseExceptionThrownEvent(ExceptionThrownEventArgs args)
        {
            this.ExceptionThrown(this, args);
        }

        /// <summary>
        /// Decides whether the  <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ExceptionThrown"/> event is wired or not.
        /// </summary>
        /// <returns>
        /// <b> true</b> if the ExceptionThrown event is wired ; otherwise , <b>false</b>.
        /// </returns>  
        internal bool CanRaiseExceptionThrownEvent()
        {
            return this.ExceptionThrown != null;
        }
#endif

#endregion

#region Internal Event Helper methods

        /// <summary>
        /// Helper method to raise the SortColumnChanging event
        /// </summary>
        /// <param name="addedColumns"></param>
        /// <param name="removedColumns"></param>
        /// <param name="action"></param>
        /// <param name="cancelScroll">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseSortColumnsChanging(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action, out bool cancelScroll)
        {
            var args = new GridSortColumnsChangingEventArgs(addedColumns, removedColumns, action, this);
            RaiseSortColumnsChanging(args);
            cancelScroll = args.CancelScroll;
            return !args.Cancel;
        }

        /// <summary>
        /// To check the whether the footer columns count is valid or not.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        internal bool AllowFooterColumns(int count)
        {
            //WPF-21152 Column count should not less than or equal to Frozen column count. Which leads to create
            //Visible line with wrong starting point.
            if (count >= 0 && this.container.ColumnCount > this.ResolveToScrollColumnIndex(this.FrozenColumnCount) + this.FooterColumnCount)
                return true;
            return false;
        }

        /// <summary>
        /// To check the whether the footer rows count is valid or not.
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        internal bool AllowFooterRows(int count)
        {
            // when we changes the FooterRowsCount count at run time, need to check up to FrozenRowsCount is 0
            var rowCounts = this.HeaderLineCount + this.FrozenRowsCount + this.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            if (count >= 0 && this.container.RowCount > rowCounts && this.FooterRowsCount < this.container.RowCount - rowCounts)
                return true;
            return false;
        }

        internal void RaiseSortColumnsChanging(GridSortColumnsChangingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseSortColumnsChanging(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseSortColumnsChanging(args);
                }
            }

            if (this.SortColumnsChanging != null)
            {
                this.SortColumnsChanging(this, args);
            }
        }

        /// <summary>
        /// Helper method to raise the SortColumn changed method
        /// </summary>
        /// <param name="addedColumns"></param>
        /// <param name="removedColumns"></param>
        /// <param name="action"></param>
        /// <remarks></remarks>
        internal void RaiseSortColumnsChanged(IList<SortColumnDescription> addedColumns, IList<SortColumnDescription> removedColumns, NotifyCollectionChangedAction action)
        {
            var args = new GridSortColumnsChangedEventArgs(addedColumns, removedColumns, action, this);
            RaiseSortColumnsChanged(args);
        }

        internal void RaiseSortColumnsChanged(GridSortColumnsChangedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseSortColumnsChanged(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseSortColumnsChanged(args);
                }
            }
            if (this.SortColumnsChanged != null)
            {
                this.SortColumnsChanged(this, args);
            }
        }

        /// <summary>
        /// Method which is used to raise the ItemsSourceChanged Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridItemsSourceChangedEventArgs">GridItemsSourceChangedEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal void RaiseItemsSourceChangedEvent(GridItemsSourceChangedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseItemsSourceChangedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseItemsSourceChangedEvent(args);
                }
            }

            if (this.ItemsSourceChanged != null)
            {
                ItemsSourceChanged(this, args);
            }
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSourceChanged"/> event in SfDataGrid.     
        /// </summary>
        /// <param name="oldItemsSource">
        /// Specifies a old ItemsSource.
        /// </param>
        /// <param name="newItemsSOurce">
        /// Specifies a new ItemsSource.
        /// </param>
        protected void RaiseItemsSourceChanged(object oldItemsSource, object newItemsSOurce)
        {
            var args = new GridItemsSourceChangedEventArgs(this, oldItemsSource, newItemsSOurce,oldView,this.View);
            this.RaiseItemsSourceChangedEvent(args);

            if(oldView!=null)
               oldView = null;
        }

        /// <summary>
        /// Method which is used to raise the SelectionChanging Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridSelectionChangingEventArgs">GridSelectionChangingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseSelectionChagingEvent(GridSelectionChangingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseSelectionChagingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseSelectionChagingEvent(args);
                }
                return args.Cancel;
            }
            if (this.SelectionChanging != null)
            {
                SelectionChanging(this, args);
                return args.Cancel;
            }
            return false;
        }

        /// <summary>
        /// Method which is used to rasie the selection changed event.
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridSelectionChangedEventArgs">GridSelectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseSelectionChangedEvent(GridSelectionChangedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseSelectionChangedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseSelectionChangedEvent(args);
                }
            }
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
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellActivatingEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellActivatingEvent(e);
                }
                return e.Cancel;
            }
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
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellActivatedEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellActivatedEvent(e);
                }
           }
            if (this.CurrentCellActivated != null)
                CurrentCellActivated(this, e);
        }

        internal void RaiseCurrentCellValueChangedEvent(CurrentCellValueChangedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellValueChangedEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellValueChangedEvent(e);
                }
            }
            if (CurrentCellValueChanged != null)
            {
                CurrentCellValueChanged(this, e);
            }
        }

        /// <summary>
        /// Helper method to raise Current Cell Begin Edit Event
        /// </summary>
        /// <param name="e"></param>
        internal bool RaiseCurrentCellBeginEditEvent(CurrentCellBeginEditEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellBeginEditEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellBeginEditEvent(e);
                }
                return e.Cancel;
            }
            if (this.CurrentCellBeginEdit != null)
            {
                CurrentCellBeginEdit(this, e);
                return e.Cancel;
            }
            return false;
        }


        /// <summary>
        /// Methods which is used to raise RecordDeleting Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.RecordDeletingEventArgs">RecordDeletingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseRecordDeletingEvent(RecordDeletingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseRecordDeletingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseRecordDeletingEvent(args);
                }
                return args.Cancel;
            }

            if (RecordDeleting == null)
                return false;
            RecordDeleting(this, args);
            return args.Cancel;
        }

        /// <summary>
        /// Method which is used to raise RecordDeleted Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.RecordDeletedEventArgs">RecordDeletedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseRecordDeletedEvent(RecordDeletedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseRecordDeletedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseRecordDeletedEvent(args);
                }
                
            }
            if (RecordDeleted != null)
                RecordDeleted(this, args);
        }

        /// <summary>
        /// Helper method to raise Current Cell End Edit Event
        /// </summary>
        /// <param name="e"></param>
        internal void RaiseCurrentCellEndEditEvent(CurrentCellEndEditEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellEndEditEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellEndEditEvent(e);
                }

            }
            if (this.CurrentCellEndEdit != null)
            {
                CurrentCellEndEdit(this, e);
            }
        }

#region Validation Events

        /// <summary>
        /// Helper method to raise Current Cell Validating Event
        /// </summary>
        internal bool RaiseCurrentCellValidatingEvent(CurrentCellValidatingEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellValidatingEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellValidatingEvent(e);
                }
                return e.IsValid;
            }
            if (this.CurrentCellValidating != null)
                CurrentCellValidating(this, e);
            return e.IsValid;
        }

        /// <summary>
        /// Helper method to raise Current Cell Validated Event
        /// </summary>
        internal void RaiseCurrentCellValidatedEvent(CurrentCellValidatedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellValidatedEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellValidatedEvent(e);
                }
            }
            if (this.CurrentCellValidated != null)
                CurrentCellValidated(this, e);
        }
        /// <summary>
        /// Helper method to raise Row validating event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.RowValidatingEventArgs">RowValidatingEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal bool RaiseRowValidatingEvent(RowValidatingEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseRowValidatingEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseRowValidatingEvent(e);
                }
                return e.IsValid;
            }
            if (this.RowValidating != null)
                RowValidating(this, e);
            return e.IsValid;
        }

        /// <summary>
        ///  Helper method to raise Row validated event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.RowValidatedEventArgs">RowValidatedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseRowValidatedEvent(RowValidatedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseRowValidatedEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseRowValidatedEvent(e);
                }
            }
            if (this.RowValidated != null)
                RowValidated(this, e);
        }

#endregion


        internal void RaiseCurrentCellDropDownSelectionChangedEvent(CurrentCellDropDownSelectionChangedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCurrentCellDropDownSelectionChangedEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCurrentCellDropDownSelectionChangedEvent(e);
                }

            }
            if (CurrentCellDropDownSelectionChanged != null)
                CurrentCellDropDownSelectionChanged(this, e);
        }

        internal bool CurrentCellRequestNavigateEvent(CurrentCellRequestNavigateEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.CurrentCellRequestNavigateEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.CurrentCellRequestNavigateEvent(e);
                }

            }
            if (CurrentCellRequestNavigate != null)
            {
                CurrentCellRequestNavigate(this, e);
                return e.Handled;
            }
            return false;
        }
        
        /// <summary>
        /// Helper method to raise UnboundColumn initializing and updating
        /// </summary>
        /// <param name="getAction">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="setAction">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <param name="column"></param>
        /// <param name="cell"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal GridUnboundColumnEventsArgs RaiseQueryUnboundValue(UnBoundActions action, object value, GridColumn column, object record)
        {
            var args = new GridUnboundColumnEventsArgs(action, value, column, record, this);
            return RaiseQueryUnboundValue(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event in SfDataGrid.
        /// </summary>
        /// <param name="action">
        /// Specifies the type of <see cref="Syncfusion.UI.Xaml.Grid.UnBoundActions"/> to be performed in GridUnBoundRow.
        /// </param>
        /// <param name="value">
        /// Specifies the value changes of GridUnBoundRow.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column .
        /// </param>
        /// <param name="cellType">
        /// Specifies the cellType of the column.</param>
        /// <param name="rowColumnIndex">
        /// Specifies the corresponding rowColumnIndex.</param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRowEventsArgs"/> that contains data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event.
        /// </returns>
        public GridUnBoundRowEventsArgs RaiseQueryUnBoundRow(GridUnBoundRow GridUnBoundRow, UnBoundActions action, object value, GridColumn column, string cellType, RowColumnIndex rowColumnIndex)
        {
            var args = new GridUnBoundRowEventsArgs(GridUnBoundRow, action, value, column, cellType, this, rowColumnIndex);
            return RaiseQueryUnBoundRow(args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryUnBoundRow"/> event in a SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Specifies the <see cref="GridUnBoundRowEventsArgs"/> that contains the event data.
        /// </param>        
        protected internal GridUnBoundRowEventsArgs RaiseQueryUnBoundRow(GridUnBoundRowEventsArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseQueryUnBoundRow(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseQueryUnBoundRow(args);
                }
                return args;
            }

            if (this.QueryUnBoundRow != null)
                this.QueryUnBoundRow(this, args);

            if (args.Handled)
            {
                args.hasCellTemplate = args.CellTemplate != null;
                args.hasEditTemplate = args.EditTemplate != null;
                return args;
            }

            return null;
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryRowHeight"/> event in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs"/> that contains the event data.        
        /// </param>        
        protected internal bool RaiseQueryRowHeight(QueryRowHeightEventArgs e)
        {
            if (this.QueryRowHeight != null)
                this.QueryRowHeight(this, e);
            return e.Handled;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellTapped"/> event in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.GridCellTappedEventArgs"/> that contains the event data.        
        /// </param>   
        protected internal void RaiseCellTappedEvent(GridCellTappedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCellTappedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCellTappedEvent(args);
                }
            }
            if (CellTapped != null)
                CellTapped(this, args);
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellDoubleTapped"/> event in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.GridCellDoubleTappedEventArgs"/> that contains the event data.        
        /// </param>   
        protected internal void RaiseCellDoubleTappedEvent(GridCellDoubleTappedEventArgs args)
        {
            if(NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCellDoubleTappedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCellDoubleTappedEvent(args);
                }
            }
            if (CellDoubleTapped != null)
                CellDoubleTapped(this, args);
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.CellToolTipOpening"/> event in SfDataGrid.
        /// </summary>
        /// <param name="e">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.GridCellToolTipOpeningEventArgs"/> that contains the event data.
        /// </param>   
        protected internal void RaiseCellToolTipOpeningEvent(GridCellToolTipOpeningEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCellToolTipOpeningEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCellToolTipOpeningEvent(args);
                }
            }
            if (CellToolTipOpening != null)
                CellToolTipOpening(this, args);
        }
        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event in SfDataGrid.
        /// </summary>
        /// <param name="cellRowColumnIndex">
        /// Specifies the RowColumnIndex of the cell.
        /// </param>
        /// <param name="column">
        /// Specifies the corresponding column of covered range.
        /// </param>
        /// <param name="record">
        /// Specifies the corresponding record with in the covered range.
        /// </param>
        /// <param name="originalSource">
        /// Specifies the source.
        /// </param>
        /// <returns>
        /// Returns the <see cref="Syncfusion.UI.Xaml.Grid.GridQueryCoveredRangeEventArgs"/> that contains the data for the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event.
        /// </returns>
        public GridQueryCoveredRangeEventArgs RaiseQueryCoveredRange(RowColumnIndex cellRowColumnIndex, GridColumn column, object record, object originalSource)
        {
            GridQueryCoveredRangeEventArgs args = new GridQueryCoveredRangeEventArgs(cellRowColumnIndex, column, record, originalSource);

            RaiseQueryCoveredRange(args);

            return args;
        }

        /// <summary>
        /// Raises the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryCoveredRange"/> event in SfDataGrid.
        /// </summary>
        /// <param name="args">
        /// Specifies the <see cref="Syncfusion.UI.Xaml.Grid.GridQueryCoveredRangeEventArgs"/> that contains the event data.
        /// </param>        
        protected internal void RaiseQueryCoveredRange(GridQueryCoveredRangeEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseQueryCoveredRange(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseQueryCoveredRange(args);
                }
            }
            if (QueryCoveredRange != null)
                this.QueryCoveredRange(this, args);
        }

        internal GridUnboundColumnEventsArgs RaiseQueryUnboundValue(GridUnboundColumnEventsArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseQueryUnboundValue(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseQueryUnboundValue(args);
                }
                return args;
            }
            if (this.QueryUnboundColumnValue != null)
            {
                this.QueryUnboundColumnValue(this, args);
            }
            return args;
        }

        /// <summary>
        /// Helper method to raise Drag and Drop function.
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.QueryColumnDraggingEventArgs">QueryColumnDraggingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseQueryColumnDragging(QueryColumnDraggingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseQueryColumnDragging(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseQueryColumnDragging(args);
                }
                return args.Cancel; ;
            }
            if (this.QueryColumnDragging != null)
            {
                this.QueryColumnDragging(this, args);
            }
            return args.Cancel;
        }

        /// <summary>
        /// Helper method to raise the Group Expanding Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs">GroupChangingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseGroupExpandingEvent(GroupChangingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseGroupExpandingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseGroupExpandingEvent(args);
                }
                return args.Cancel; 
            }
            if (this.GroupExpanding != null)
                this.GroupExpanding(this, args);
            return args.Cancel;
        }

        /// <summary>
        /// Helper method to raise the Group Collapsing Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GroupChangingEventArgs">GroupChangingEventArgs</see> that contains the event data.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        internal bool RaiseGroupCollapsingEvent(GroupChangingEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseGroupCollapsingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseGroupCollapsingEvent(args);
                }
                return args.Cancel;
            }

            if (this.GroupCollapsing != null)
                this.GroupCollapsing(this, args);
            return args.Cancel;
        }

        /// <summary>
        /// Helper method to raise the Group Expanded Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GroupChangedEventArgs">GroupChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseGroupExpandedEvent(GroupChangedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseGroupExpandedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseGroupExpandedEvent(args);
                }
            }

            if (this.GroupExpanded != null)
                this.GroupExpanded(this, args);
        }

        /// <summary>
        /// Helper method to raise the Group Collapsed Event
        /// </summary>
        /// <param name="args">An <see cref="T:Syncfusion.UI.Xaml.Grid.GroupChangedEventArgs">GroupChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseGroupCollapsedEvent(GroupChangedEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseGroupCollapsedEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseGroupCollapsedEvent(args);
                }
            }
            if (this.GroupCollapsed != null)
                this.GroupCollapsed(this, args);
        }

        /// <summary>
        /// Helper method to raise the Resize Columns Event
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        internal bool RaiseResizingColumnsEvent(ResizingColumnsEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseResizingColumnsEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseResizingColumnsEvent(args);
                }
                return args.Cancel;
            }
            if (ResizingColumns != null)
                this.ResizingColumns(this, args);
            return args.Cancel;
        }
        
        internal GridCopyPasteEventArgs RaisePasteContentEvent(GridCopyPasteEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaisePasteContentEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaisePasteContentEvent(args);
                }
                return args;
            }
            if (GridPasteContent != null)
                this.GridPasteContent(this, args);
            return args;
        }

        internal GridCopyPasteEventArgs RaiseCopyContentEvent(GridCopyPasteEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCopyContentEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCopyContentEvent(args);
                }
                return args;
            }
            if (GridCopyContent != null)
                this.GridCopyContent(this, args);
            return args;
        }

        internal GridCopyPasteCellEventArgs RaiseCopyGridCellContentEvent(GridCopyPasteCellEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseCopyGridCellContentEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseCopyGridCellContentEvent(args);
                }
                return args;
            }
            if (CopyGridCellContent != null)
                this.CopyGridCellContent(this, args);
            return args;
        }

        internal GridCopyPasteCellEventArgs RaisePasteGridCellContentEvent(GridCopyPasteCellEventArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaisePasteGridCellContentEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaisePasteGridCellContentEvent(args);
                }
                return args;
            }
            if (PasteGridCellContent != null)
                this.PasteGridCellContent(this, args);
            return args;
        }

        /// <summary>
        /// Helper method to raise the FilterChanging event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridFilterEventArgs">GridFilterEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal bool RaiseFilterChanging(GridFilterEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseFilterChanging(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseFilterChanging(e);
                }
                return e.Handled; 
            }

            if (this.FilterChanging != null)
                this.FilterChanging(this, e);

            return e.Handled;
        }

        /// <summary>
        /// Helper method to raise the FilterChanged event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridFilterEventArgs">GridFilterEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseFilterChanged(GridFilterEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseFilterChanged(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseFilterChanged(e);
                }
            }

            if (this.FilterChanged != null)
                this.FilterChanged(this, e);
        }

        /// <summary>
        /// Helper method to raise the FilterItemsPopulating eventr
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatingEventArgs">GridFilterListItemsPopulatingEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal bool RaiseFilterListItemsPopulating(GridFilterItemsPopulatingEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseFilterListItemsPopulating(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseFilterListItemsPopulating(e);
                }
                return e.Handled;
            }

            if (this.FilterItemsPopulating != null)
                this.FilterItemsPopulating(this, e);

            return e.Handled;
        }

        /// <summary>
        /// Helper method to raise the FilterItemsPopulated event
        /// </summary>
        /// <param name="e">An <see cref="T:Syncfusion.UI.Xaml.Grid.GridFilterItemsPopulatedEventArgs">GridFilterListItemsPopulatingEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void RaiseFilterListItemsPopulated(GridFilterItemsPopulatedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseFilterListItemsPopulated(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseFilterListItemsPopulated(e);
                }
             }

            if (this.FilterItemsPopulated != null)
                this.FilterItemsPopulated(this, e);
        }

        internal override void RaiseAutoGeneratingEvent(ref GridColumnBase column, ref bool cancel)
        {
            var args = RaiseAutoGeneratingEvent(new AutoGeneratingColumnArgs(column as GridColumn, this) { Cancel = cancel });
            column = args.Column;
            cancel = args.Cancel;
        }

        internal AutoGeneratingColumnArgs RaiseAutoGeneratingEvent(AutoGeneratingColumnArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseAutoGeneratingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseAutoGeneratingEvent(args);
                }
                return args;
            }

            if (AutoGeneratingColumn != null)
                this.AutoGeneratingColumn(this, args);
            return args;
        }

        internal bool RaiseGridContextMenuEvent(GridContextMenuEventArgs e)
        {
            // WPF-20188 ContextMenuOpeing event is invoked only for the Parent grid, its not invoked for DetailsViewGrid.
            // So we can invoke the ContextMenuEvent for SourceDataGrid using NotifyListener
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseGridContextMenuEvent(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseGridContextMenuEvent(e);
                }
                return e.Handled;
            }

            if (this.GridContextMenuOpening != null)
                this.GridContextMenuOpening(this, e);

            return e.Handled;
        }


        internal object RaiseAddNewRowInitiatingEvent(AddNewRowInitiatingEventArgs args)
        {
            // WPF-22527 execute AddNewRow Initiating Event if datagrid is DetailsViewGrid .
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseAddNewRowInitiatingEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseAddNewRowInitiatingEvent(args);
                }
                return args.NewObject;
            }
            if (AddNewRowInitiating != null)
                AddNewRowInitiating(this, args);
            return args.NewObject;
        }

#endregion

#region Private methods

        private void CheckDetailsViewSupport()
        {
            if (AutoGenerateRelations || (this.DetailsViewDefinition != null && this.DetailsViewDefinition.Any()))
            {
                if (this.ItemsSource is VirtualizingCollectionView)
                    throw new NotSupportedException("DetailsView is not supported with Data Virtualization");
                if (this.AllowFrozenGroupHeaders)
                    throw new NotSupportedException("DetailsView is not supported with FrozenGroupHeaders");

                if (this.FrozenRowsCount > 0 || this.FooterRowsCount > 0 || this.FrozenColumnCount > 0 || this.FooterColumnCount > 0)
                    throw new NotSupportedException("DetailsView is not supported with Freeze Panes support");
            }
        }
        
        //stacked表头发生改变时触发
        private void OnStackedColumnsChildChanged()
        {
            //this.RowGenerator.RefreshStackedHeaders();
            var headerRows = this.RowGenerator.Items.Where(row => row.RowType == RowType.HeaderRow);
            headerRows.ForEach(row =>
            {
                row.RowIndex = -1;
            });
            this.VisualContainer.InvalidateMeasureInfo();
        }

        internal void InitializeStackedColumnChildDelegate()
        {
            if (this.StackedHeaderRows == null)
                return;
            this.StackedHeaderRows.ForEach(row => row.StackedColumns.ForEach(col =>
            {
                if (col.ChildColumnChanged == null)
                    col.ChildColumnChanged = OnStackedColumnsChildChanged;
            }));
        }

        internal IEnumerable GetSourceList(object source)
        {
            IEnumerable result = null;
            if (source != null)
            {
#if WPF
                if (source is DataTable)
                {
                    result = ((DataTable)source).DefaultView;
                }
                else if (source is DataView)
                {
                    result = source as DataView;
                }
                else
#endif
                result = source as IEnumerable;
            }
            return result;
        }

        /// <summary>
        /// Add SortDescriptions and GroupDescriptions in View from DataGrid's GroupColumnDescriptions
        /// </summary>
        internal void InitialGroup()
        {
            if (this.GroupColumnDescriptions != null && this.GroupColumnDescriptions.Count <= 0)
                return;

            this.GridModel.Suspend();
            foreach (var column in this.GroupColumnDescriptions)
            {
                //if (!this.View.IsDynamicBound)
                //{
                //    if (!CheckColumnNameinItemProperties(this.Columns[column.ColumnName]))
                //        continue;
                //}

                if (this.View.GroupDescriptions != null && !this.View.GroupDescriptions.Any(desc => (desc as ColumnGroupDescription).PropertyName == column.ColumnName))
                {
                    //ColumnGroupDescription class is derived from the PropertyGroupDescription. Because in PropertyGroupDescription we cant pass the Comparer as argument.                    
                    this.View.GroupDescriptions.Add(new ColumnGroupDescription(column.ColumnName, column.Converter, column.Comparer) { SortGroupRecords = column.SortGroupRecords });
                    if (this.View.SortDescriptions.FirstOrDefault(desc => desc.PropertyName == column.ColumnName) == default(SortDescription))
                        this.View.SortDescriptions.Add(new SortDescription(column.ColumnName, ListSortDirection.Ascending));
                    //WPF-22441 Sample will be crashed on grouping when the DataGrid loads with initial Grouping. Because 
                    //the SortColumnDescriptions is not initialized in GridModel event which will be hooked after this method only.
                    if (this.SortColumnDescriptions.FirstOrDefault(s => s.ColumnName == column.ColumnName) == null)
                        this.SortColumnDescriptions.Add(new SortColumnDescription() { ColumnName = column.ColumnName, SortDirection = ListSortDirection.Ascending });
                }
                else
                    throw new InvalidOperationException("GroupColumnDescription already exist in DataGrid.GroupColumnDescriptions");
            }
            this.GridModel.Resume();
        }

        internal bool CheckColumnNameinItemProperties(GridColumn column)
        {
            if (column.UseBindingValue)
                return true;

            var ColumnName = column.MappingName;
            if (this.View != null && this.Columns.Any(col => col.MappingName != null && col.MappingName.Equals(ColumnName)))
            {
                var provider = View.GetItemProperties();
                if (provider == null)
                    return false;

                if (View.IsDynamicBound)
                {
                    if (View.Records.Count <= 0) return false;
                    var dynObj = View.Records[0].Data as IDynamicMetaObjectProvider;
                    if (dynObj != null)
                    {
                        var metaType = dynObj.GetType();
                        var metaData = dynObj.GetMetaObject(System.Linq.Expressions.Expression.Parameter(metaType, metaType.Name));
                        if (!metaData.GetDynamicMemberNames().Contains(ColumnName))
                            return false;
                    }
                }
                else
                {
#if WPF
                    if (View.IsLegacyDataTable && ColumnName.ContainsSpecialChars())
                    {
                        return View is PagedCollectionView ? (View.SourceCollection as DataView).Table.Columns.Contains(ColumnName) :
                                                                (View as DataTableCollectionView).ViewSource.Table.Columns.Contains(ColumnName);
                    }
#endif
                    //checks the column defined or not by using GetPropertyDescriptor.
                    var tempProperyDescriptor = provider.GetPropertyDescriptor(ColumnName);
                    if (tempProperyDescriptor == null)
                        return false;
                }
            }
            return true;
        }

        internal void InitialSort()
        {
            if (this.SortColumnDescriptions != null && this.SortColumnDescriptions.Count > 0)
            {
                foreach (var sortColumn in this.SortColumnDescriptions)
                {
                    if (sortColumn == null) throw new ArgumentNullException("Sort Column");

                    //if (!this.View.IsDynamicBound)
                    //{
                    //    if (!CheckColumnNameinItemProperties(this.Columns[sortColumn.ColumnName]))
                    //        continue;
                    //}

                    if (View.SortDescriptions.All(desc => desc.PropertyName != sortColumn.ColumnName))
                        View.SortDescriptions.Add(new SortDescription(sortColumn.ColumnName,
                                                                      sortColumn.SortDirection));
                    else
                        throw new InvalidOperationException("SortColumnDescription already exist in DataGrid.SortColumnDescriptions");
                }
            }

            if (this.SortComparers != null && this.SortComparers.Count > 0)
            {
                foreach (var sortcomparer in this.SortComparers)
                {
                    if (!View.SortComparers.Contains(sortcomparer))
                        View.SortComparers.Add(sortcomparer);
                }
            }
        }

        internal virtual void SetSourceList(object itemsSource)
        {
#if UWP
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;
#endif
            CheckDetailsViewSupport();

            this.UnWireEvents();
            CreateCollectionView(itemsSource);
            //While invoking DeferRefresh after WireEvents which leads to Raise the events for two times, hence the invoked before WireEvents method.

            DeferRefresh();
            this.WireEvents();
        }

        internal void CreateCollectionView(object itemsSource)
        {
            //WPF-38150:We have assigned the this.View to oldView.
            oldView = this.View;
            this.View = this.CreateCollectionViewAdv(this.GetSourceList(itemsSource), this);

#if WPF
            if (this.View != null && this.useDrawing)            
                Provider = View.GetPropertyAccessProvider();                            
#endif
            this.Columns.Suspend();
            if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.Reset)
            {
                if (!(this is DetailsViewDataGrid))
                {
                    foreach (var item in this.Columns.Where(c => c.IsAutoGenerated).ToList())
                    {
                        this.Columns.Remove(item);
                        UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, item);
                    }
                }
            }
            else if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
            {
                var canreset = Columns.Count > 0;
                this.Columns.Clear();
                if (canreset)
                    UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Reset, null);
            }
            else if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.SmartReset)
            {
                if (!(this is DetailsViewDataGrid) && this.View != null)
                {
                    var newcolumns = this.View.GetItemProperties();
                    List<GridColumn> temp = new List<GridColumn>();

                    foreach (var item in this.Columns.Where(c => c.IsAutoGenerated).ToList())
                    {
                        var property = newcolumns.Find(item.MappingName, false);
                        if (property == null)
                            temp.Add(item);
                    }
                    foreach (var item in temp)
                    {
                        this.Columns.Remove(item);
                        UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, item);
                    }
                }
            }
            this.Columns.Resume();
            this.GridColumnSizer.ResetAutoCalculationforAllColumns();
            if (View == null) return;
            // WPF-20007 - When details view contains no records and dynamic collection is used, need to set IsDynamicBound
            this.View.IsDynamicBound = this.IsDynamicItemsSource;
            if (VisualContainer != null && this.AutoGenerateColumns && ((!this.Columns.Any() && this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.RetainOld) || (this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.Reset || this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll || this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.SmartReset)))
                this.GenerateGridColumns();

            EnsureColumnProperty();
            if (AutoGenerateRelations)
                GenerateGridRelations();

            this.View.NewItemPlaceholderPosition = this.NewItemPlaceholderPosition;
            this.View.GroupComparer = this.SummaryGroupComparer;
            this.View.LiveDataUpdateMode = this.LiveDataUpdateMode;
            this.View.AutoExpandGroups = this.AutoExpandGroups;
            this.View.SummaryCalculationMode = this.SummaryCalculationMode;
#if WPF
            (this.View as CollectionViewAdv).DispatchOwner = this.Dispatcher;
#endif
        }

        private void EnsureColumnProperty()
        {
#if WPF
            if (View.IsLegacyDataTable)
                return;
#endif
            var itemproperties = View.GetItemProperties();
            if (itemproperties == null)
                return;

            foreach (var column in this.Columns)
            {
                if (column.MappingName == null)
                    continue;
#if WPF
                var propDesc = PropertyDescriptorExtensions.GetPropertyDescriptor(itemproperties, column.MappingName);
                if (propDesc != null && propDesc.IsReadOnly)
                {
                    if (column.ReadLocalValue(GridColumn.AllowEditingProperty) == DependencyProperty.UnsetValue)
                        column.AllowEditing = !propDesc.IsReadOnly;
                }
#else
                var propDesc = Syncfusion.Data.PropertyInfoExtensions.GetPropertyDescriptor(itemproperties, column.MappingName);
                if (propDesc != null && (propDesc.SetMethod == null || propDesc.SetMethod.IsPrivate))
                    column.AllowEditing = propDesc.SetMethod == null ? propDesc.CanWrite : propDesc.SetMethod.IsPublic;
#endif
            }
        }

        internal void DeferRefresh()
        {
            if (View == null)
                return;
            using ((View as CollectionViewAdv).DeferRefresh(false))
            {
                InitialSort();
                InitialGroup();

                //WPF-20773 avoid designer crash issue
                if (this.GroupSummaryRows != null && this.View.SummaryRows != null)
                    foreach (var summaryRow in this.GroupSummaryRows)
                    {
                        this.View.SummaryRows.Add(summaryRow);
                    }

                //WPF-20773 avoid designer crash issue
                if (this.TableSummaryRows != null && this.View.TableSummaryRows != null)
                    foreach (var summaryRow in this.TableSummaryRows)
                    {
                        this.View.TableSummaryRows.Add(summaryRow);
                    }

                if (this.CaptionSummaryRow != null)
                {
                    this.View.CaptionSummaryRow = this.CaptionSummaryRow;
                }
            }
        }

        internal ICollectionViewAdv CreateCollectionViewAdv(IEnumerable source, SfDataGrid dataGrid)
        {
            ICollectionViewAdv view = null;
            if (source == null)
                return null;

#if WPF
            if (!dataGrid.CheckIsLegacyDataTable(source))
            {
#endif
            if (source is GridQueryableCollectionViewWrapper)
            {
                view = source as GridQueryableCollectionViewWrapper;
                (view as GridQueryableCollectionViewWrapper).AttachGridView(this);
            }
            else if (source is PagedCollectionView)
            {
                view = source as PagedCollectionView;
                if (view is GridPagedCollectionViewWrapper)
                    (view as GridPagedCollectionViewWrapper).AttachGridView(this);
            }
            else if (source is VirtualizingCollectionView)
            {
                view = source as VirtualizingCollectionView;
                if (view is GridVirtualizingCollectionView)
                    (view as GridVirtualizingCollectionView).AttachGridView(this);
                dataGrid.ShowBusyIndicator = true;
            }
#if WPF
                else if (source is CollectionView)
                {
                    view = new GridQueryableCollectionViewWrapper((source as CollectionView).SourceCollection, dataGrid);
                    //WPF-30785 Default Filter value applied to the data in SfDataGrid
                    if (CanUseViewFilter)
                        view.Filter = (source as CollectionView).Filter;
                }
#endif
            else if (dataGrid.EnableDataVirtualization && source is IQueryable)
            {
                view = new GridVirtualizingCollectionView(source);
                (view as GridVirtualizingCollectionView).AttachGridView(this);
                dataGrid.ShowBusyIndicator = true;
            }
            else
                view = new GridQueryableCollectionViewWrapper(source, dataGrid);

            if (dataGrid.SourceType != null && view is QueryableCollectionView)
                (view as CollectionViewAdv).SetSourceType(dataGrid.SourceType);

            (view as CollectionViewAdv).NotificationSubscriptionMode = this.NotificationSubscriptionMode;
            if (view is QueryableCollectionView)
                ((QueryableCollectionView)view).UsePLINQ = dataGrid.UsePLINQ;
#if WPF
            }
            else if (source is GridDataTableCollectionViewWrapper)
            {
                view = source as GridDataTableCollectionViewWrapper;
                (view as GridDataTableCollectionViewWrapper).AttachGridView(this);
            }
            else
            {
                view = new GridDataTableCollectionViewWrapper(source, dataGrid);
                //WPF - 30785 Default Filter value applied to the data in SfDataGrid
                if (CanUseViewFilter)
                    (view as DataTableCollectionView).RowFilter = (source as DataView).RowFilter;

                if (dataGrid.SourceType != null)
                    (view as DataTableCollectionView).SetSourceType(dataGrid.SourceType);
            }
#endif
            return view;
        }
        
        private void RefreshFilterIconVisibility()
        {
            if (this.View is Syncfusion.Data.PagedCollectionView)
            {
                if ((this.View as Syncfusion.Data.PagedCollectionView).UseOnDemandPaging)
                    return;
            }
            if (this.VisualContainer != null && this.RowGenerator != null)
            {
                var headerRowBase = this.RowGenerator.Items.FirstOrDefault(row => row.RowIndex == this.GetHeaderIndex());
                foreach (var column in this.Columns)
                {
                    if (headerRowBase != null)
                    {
                        var columnBase = (headerRowBase as DataRowBase).VisibleColumns.FirstOrDefault(col => col.GridColumn != null && col.GridColumn.MappingName.Equals(column.MappingName) && col.GridColumn.Equals(column));
                        if (columnBase != null && CanSetAllowFilters(column))
                            (columnBase.ColumnElement as GridHeaderCellControl).FilterIconVisiblity = Visibility.Visible;
                        else if (columnBase != null && !CanSetAllowFilters(column))
                            (columnBase.ColumnElement as GridHeaderCellControl).FilterIconVisiblity = Visibility.Collapsed;

                    }
                }
            }
        }

#if WPF
        internal bool CheckIsLegacyDataTable(IEnumerable source)
        {
            return source is DataTable || source is DataView || source is DataTableCollectionView;
        }

        internal bool IsLegacyDataTable
        {
            get
            {
                return this.ItemsSource is DataView || this.ItemsSource is DataTable;
            }
        }
#endif

        /// <summary>
        /// Updates cell style for given column index
        /// </summary>
        /// <param name="visibleColumnIndex"></param>
        /// <remarks></remarks>
        private void UpdateColumnCellStyle(int visibleColumnIndex)
        {
            this.RowGenerator.Items.ForEach(row =>
                {
                    if (row.RowData != null || row.RowType == RowType.AddNewRow || row.RowType == RowType.FilterRow)
                    {
                        row.VisibleColumns.ForEach(col =>
                        {
                            if (col.ColumnIndex == visibleColumnIndex)
                            {
                                col.UpdateCellStyle(row.RowData);
                            }
                        });
                    }
                });
        }

        /// <summary>
        /// Update style for all cells except header cells.
        /// </summary>
        /// <remarks></remarks>
        private void UpdateCellStyles()
        {
            this.RowGenerator.Items.OfType<DataRow>().ForEach(row =>
            {
                if (row.RowIndex != -1 && (row.RowData != null || row.RowType == RowType.AddNewRow || row.RowType == RowType.FilterRow))
                    row.VisibleColumns.ForEach(
                        col => 
                            {
#if WPF
                                if (this.useDrawing)
                                    col.GridColumn.drawingTypeface = null;
#endif
                                col.UpdateCellStyle(row.RowData);                                
                            }
                        );
            });
        }
        

        private void UpdateSummariesCellStyle()
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if(row.RowData != null && row.RowType != RowType.DefaultRow && row.RowType != RowType.DetailsViewRow)                
                {
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle(row.RowData));
                }
            });
        }

        private void UpdateSummariesRowStyle()
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType != RowType.DefaultRow && row.RowType != RowType.DetailsViewRow)
                {
                    row.UpdateRowStyles(row.WholeRowElement as ContentControl);
                }
            });
        }

        /// <summary>
        /// Updates column header cell style
        /// </summary>
        /// <param name="visibleColumnIndex"></param>
        /// <remarks></remarks>
        private void UpdateColumnHeaderStyle(int visibleColumnIndex)
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType == RowType.HeaderRow)
                {
                    row.VisibleColumns.ForEach(col =>
                    {
                        if (col.ColumnIndex == visibleColumnIndex)
                        {
                            col.UpdateCellStyle(row.RowData);
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Updates filter row cell style for given column index.
        /// </summary>
        /// <param name="visibleColumnIndex"></param>
        /// <remarks></remarks>
        private void UpdateFilterRowCellStyle(int visibleColumnIndex)
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType == RowType.FilterRow)
                {
                    row.VisibleColumns.ForEach(col =>
                    {
                        if (col.ColumnIndex == visibleColumnIndex)
                        {
                            col.UpdateCellStyle(row.RowData);
                        }
                    });
                }
            });
        }

        /// <summary>
        /// Updates unbound row cell style for all cells in Unbound Row.
        /// </summary>
        private void UpdateUnBoundRowCellStyle()
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowType == RowType.UnBoundRow)
                {
                    row.VisibleColumns.ForEach(col =>
                    {
                        col.UpdateCellStyle(row.RowData);                        
                    });
                }
            });
        }

        /// <summary>
        /// Updates row header row style
        /// </summary>
        /// <remarks></remarks>
        private void UpdateHeaderRowStyle()
        {
            this.RowGenerator.Items.ForEach(row =>
            {
                if (row.RowRegion == RowRegion.Header)
                {
                    row.VisibleColumns.ForEach(col => col.UpdateCellStyle(row.RowData));
                }
            });
        }

        /// <summary>
        /// Updates row styles for grid
        /// </summary>
        /// <remarks></remarks>
        private void UpdateRowStyle()
        {
            this.RowGenerator.Items.ForEach(row => row.UpdateRowStyles(row.WholeRowElement as ContentControl));
        }
        
        /// <summary>
        /// Updates style for given column
        /// </summary>
        /// <param name="column">
        /// The corresponding column on which the property value changes.
        /// </param>
        /// <param name="propertyName">
        /// The name of property that value has been changed.
        /// </param>

        internal void OnColumnStyleChanged(GridColumn column,string propertyName)
        {
            var visibleColumnIndex = this.ResolveToScrollColumnIndex(this.Columns.IndexOf(column));
            if (propertyName == "CellStyle" || propertyName == "CellStyleSelector")
            {
                if ((column.ReadLocalValue(GridColumn.CellStyleProperty) != DependencyProperty.UnsetValue) ||
                    ((column.ReadLocalValue(GridColumn.CellStyleSelectorProperty) != DependencyProperty.UnsetValue)))
                    this.UpdateColumnCellStyle(visibleColumnIndex);
            }
            else if (propertyName == "CellTemplate" || propertyName == "CellTemplateSelector")
            {
                if ((column.ReadLocalValue(GridColumn.CellTemplateProperty) != DependencyProperty.UnsetValue) ||
                    (column.ReadLocalValue(GridColumn.CellTemplateSelectorProperty) != DependencyProperty.UnsetValue))
                    this.UpdateColumnCellStyle(visibleColumnIndex);
            }
            else if (propertyName == "HeaderStyle" || propertyName == "HeaderTemplate")
            {
                if ((column.ReadLocalValue(GridColumn.HeaderStyleProperty) != DependencyProperty.UnsetValue) ||
                    ((column.ReadLocalValue(GridColumn.HeaderTemplateProperty) != DependencyProperty.UnsetValue)))
                    this.UpdateColumnHeaderStyle(visibleColumnIndex);                
            }
            else if(propertyName == "FilterRowCellStyle")
            {
                if (column.ReadLocalValue(GridColumn.FilterRowCellStyleProperty) != DependencyProperty.UnsetValue)                  
                    this.UpdateFilterRowCellStyle(visibleColumnIndex);
            }
        }
        
        internal void GenerateGridColumns()
        {
            if (this is DetailsViewDataGrid && this.NotifyListener != null)
            {
                this.NotifyListener.SourceDataGrid.GenerateGridColumns(this.NotifyListener.SourceDataGrid.Columns, this.View);
                suspendForColumnPopulation = true;
                // WPF-18292 - Since we have already cloned columns from SourceDataGrid, need to clear the columns when new columns are added to SourceDataGrid,
                this.Columns.Clear();
                CloneHelper.CloneCollection(this.NotifyListener.SourceDataGrid.Columns, this.Columns, typeof(GridColumnBase));
                // WPF-20100 - When stackedheaders are added through GroupName attribute, need to copy stacked headers here
                if (this.StackedHeaderRows != null && this.StackedHeaderRows.Count == 0)
                {
                    CloneHelper.CloneCollection(this.NotifyListener.SourceDataGrid.StackedHeaderRows, this.StackedHeaderRows, typeof(StackedHeaderRow));
                    foreach (var targetRow in this.StackedHeaderRows)
                    {
                        var sourceRow = this.NotifyListener.SourceDataGrid.StackedHeaderRows.ElementAt(this.StackedHeaderRows.IndexOf(targetRow));
                        if (sourceRow != null)
                            CloneHelper.CloneCollection(sourceRow.StackedColumns, targetRow.StackedColumns, typeof(StackedColumn));
                    }
                }
                suspendForColumnPopulation = false;
                //WPF-18642 - Copying the filter predicates from SourceDataGrid to DetailsViewDataGrid
                suspendNotification = true;
                foreach (var targetColumn in this.Columns)
                {
                    var sourceColumn = this.NotifyListener.SourceDataGrid.Columns.FirstOrDefault(x => x.MappingName == targetColumn.MappingName);
                    if (sourceColumn != null)
                        CloneHelper.CloneCollection(sourceColumn.FilterPredicates, targetColumn.FilterPredicates, typeof(FilterPredicate));
                }
                suspendNotification = false;
            }
            else
            {
                GenerateGridColumns(this.Columns, this.View);
            }
        }

        private void GenerateGridColumns(Columns columns, ICollectionViewAdv view)
        {
            if (columns == null || view == null)
                return;

            columns.Suspend(); 
            if (view.IsDynamicBound)
            {
                this.GenerateGridColumnsForDynamic(columns, view);
                columns.Resume();
                return;
            }

            var propertyCollection = view.GetItemProperties();
            if (propertyCollection == null)
                return;
            if (StackedHeaderRows.Count <= 0)
                CreateStackedHeader(propertyCollection);

            var gridColumns = new ObservableCollection<GridColumnBase>();
            foreach (var column in this.Columns)
                gridColumns.Add(column);

            this.GenerateGridColumns(gridColumns, view);
            this.Columns.Clear();

            foreach (var column in gridColumns)
                this.Columns.Add(column as GridColumn);

            columns.Resume();
        }

#if WPF
        private void CreateStackedHeader(PropertyDescriptorCollection propertyCollection)
#else
        private void CreateStackedHeader(PropertyInfoCollection propertyCollection)
#endif
        {
            var listOfStackedRows = new List<StackedHeaderRow>();
#if WPF
            foreach (PropertyDescriptor propertyinfo in propertyCollection)
            {
                var displayAttribute = propertyinfo.Attributes.OfType<DisplayAttribute>().FirstOrDefault();
#else
            foreach (var keyvaluepair in propertyCollection)
            {
                var propertyinfo = keyvaluepair.Value;
                var displayAttribute = propertyinfo.GetCustomAttributes(typeof(DisplayAttribute), true).FirstOrDefault() as DisplayAttribute;
#endif
                if (displayAttribute != null && displayAttribute.GroupName != null)
                {
#if !WinRT && !UNIVERSAL
                    ResourceManager displayAttributeResourceManager = null;
#else
                    ResourceLoader displayAttributeResourceLoader = null;
#endif
                    if (displayAttribute.ResourceType != null)
                    {
#if !WinRT && !UNIVERSAL
                        var resourceName = displayAttribute.ResourceType.FullName.Replace('_', '.');
                        Assembly assembly = displayAttribute.ResourceType.Assembly;
                        displayAttributeResourceManager = new ResourceManager(resourceName, assembly);
#else
                        var resourceName = displayAttribute.ResourceType.Name;
                        displayAttributeResourceLoader = ResourceLoader.GetForViewIndependentUse(resourceName);
#endif
                    }
                    string groupName = string.Empty;
#if !WinRT && !UNIVERSAL
                    if (displayAttributeResourceManager != null)
                        groupName = displayAttributeResourceManager.GetString(displayAttribute.GroupName, System.Globalization.CultureInfo.CurrentCulture);
#else
                    if (displayAttributeResourceLoader != null)
                        groupName = displayAttributeResourceLoader.GetString(displayAttribute.GroupName);
#endif
                    else
                        groupName = displayAttribute.GroupName;
                    if (groupName != null)
                    {
                        if (groupName.Contains("/"))
                        {
                            var headerName = groupName.Split('/');
                            var index = 0;
                            for (int i = headerName.Length - 1; i >= 0; i--)
                            {
                                var headerGroupName = headerName[i];
                                StackedHeaderRow stackedHeaderRow;
                                if (listOfStackedRows.Count > index)
                                {
                                    stackedHeaderRow = listOfStackedRows[index];
                                }
                                else
                                {
                                    stackedHeaderRow = new StackedHeaderRow();
                                    listOfStackedRows.Add(stackedHeaderRow);
                                }

                                var stackedColumn = stackedHeaderRow.StackedColumns.FirstOrDefault(column => column.HeaderText == headerGroupName);
                                if (stackedColumn == null)
                                {
                                    stackedColumn = new StackedColumn { HeaderText = headerGroupName, ChildColumns = propertyinfo.Name };
                                    stackedHeaderRow.StackedColumns.Add(stackedColumn);
                                }
                                else
                                    stackedColumn.ChildColumns += "," + propertyinfo.Name;
                                index++;
                            }
                        }
                        else
                        {
                            var index = 0;
                            StackedHeaderRow stackedHeaderRow;
                            if (listOfStackedRows.Count > index)
                            {
                                stackedHeaderRow = listOfStackedRows[index];
                            }
                            else
                            {
                                stackedHeaderRow = new StackedHeaderRow();
                                listOfStackedRows.Add(stackedHeaderRow);
                            }
                            var stackedColumn = stackedHeaderRow.StackedColumns.FirstOrDefault(column => column.HeaderText == groupName);
                            if (stackedColumn == null)
                            {
                                stackedColumn = new StackedColumn { HeaderText = groupName, ChildColumns = propertyinfo.Name };
                                stackedHeaderRow.StackedColumns.Add(stackedColumn);
                            }
                            else
                                stackedColumn.ChildColumns += "," + propertyinfo.Name;
                        }
                    }
                }
            }

            if (listOfStackedRows.Any())
            {
                var stackedHeaderRows = new StackedHeaderRows();
                for (var i = listOfStackedRows.Count - 1; i >= 0; i--)
                {
                    var stackedHeaderRow = listOfStackedRows[i];
                    stackedHeaderRows.Add(stackedHeaderRow);
                }
                this.StackedHeaderRows = stackedHeaderRows;
            }
        }
#if WPF
        internal override PropertyDescriptorCollection GetPropertyInfoCollection(object view)
#else
        internal override PropertyInfoCollection GetPropertyInfoCollection(object view)
#endif
        {
            return (view as ICollectionViewAdv).GetItemProperties();
        }
#if WPF
        internal override GridColumnBase CreateNumericColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateNumericColumn(PropertyInfo propertyinfo)
#endif
        {
            var column = new GridNumericColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
            };

            var allownull = this.CanAllowNull(propertyinfo);
#if !WPF
            if (propertyinfo.PropertyType.IsAssignableFrom(typeof(double)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(double?)))
                column.ParsingMode = Parsers.Double;
            column.AllowNullInput = allownull;
#else
            if (propertyinfo.PropertyType.IsAssignableFrom(typeof(int)) || propertyinfo.PropertyType.IsAssignableFrom(typeof(int?)))
                column.NumberDecimalDigits = 0;
            column.AllowNullValue = allownull;
#endif
            return column;
        }

#if WPF
        internal override GridColumnBase CreateTextColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateTextColumn(PropertyInfo propertyinfo)
#endif
        {
            return new GridTextColumn { MappingName = propertyinfo.Name, HeaderText = propertyinfo.Name };
        }

#if WPF
        internal override GridColumnBase CreateDateTimeColumn(PropertyDescriptor propertyinfo)
#else
        internal override GridColumnBase CreateDateTimeColumn(PropertyInfo propertyinfo)
#endif
        {
            var column = new GridDateTimeColumn { MappingName = propertyinfo.Name, HeaderText = propertyinfo.Name };
            column.AllowNullValue = this.CanAllowNull(propertyinfo);
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
            return new GridCheckBoxColumn
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
            return new GridComboBoxColumn
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
            return new GridHyperlinkColumn { MappingName = propertyinfo.Name, HeaderText = propertyinfo.Name };
        }

#if WPF
        internal override GridColumnBase CreateTimeSpanColumn(PropertyDescriptor propertyinfo)
        {
            return new GridTimeSpanColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                AllowNull = this.CanAllowNull(propertyinfo)
            };
        }

        internal override GridColumnBase CreateCurrencyColumn(PropertyDescriptor propertyinfo)
        {
            return new GridCurrencyColumn
            {
                MappingName = propertyinfo.Name,
                HeaderText = propertyinfo.Name,
                AllowNullValue = this.CanAllowNull(propertyinfo)
        };
        }
        
        internal override GridColumnBase CreateMaskColumn(PropertyDescriptor propertyinfo)
        {
            return new GridMaskColumn
            {
                MappingName = propertyinfo.Name,
                Mask = "(999)999-9999",
                HeaderText = propertyinfo.Name
            };
        }
#endif
        
        private void GenerateGridColumnsForDynamic(Columns columns, ICollectionViewAdv view)
        {
            if (view.SourceCollection == null)
                return;
            // If there is no records in grid, columns could not be extracted. So skipped here
            if (view.SourceCollection.AsQueryable().Count() <= 0)
                return;
            //WPF-22024 - Column will not be generated when using dynamic collection at runtime. since view.Records count is zero. 
            //so have proceed this from source collection
            var enumerator = view.SourceCollection.GetEnumerator();
            var dynObj = (enumerator.MoveNext() && enumerator.Current != null) ? enumerator.Current as IDynamicMetaObjectProvider : null;
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
                    GridColumn column = new GridTextColumn
                    {
                        MappingName = prop,
                        HeaderText = prop
                    };
                    column.IsAutoGenerated = true;
                    var args = RaiseAutoGeneratingEvent(new AutoGeneratingColumnArgs(column, this));
                    if (!args.Cancel)
                        columns.Add(args.Column);
                }
            }
        }


        /// <summary>
        /// Method which will initiate the Cell Renderers Collection.
        /// </summary>
        /// <remarks></remarks>
        private void InitializeCellRendererCollection()
        {
            cellRenderers.Add("Static", new GridCellTextBlockRenderer());
            cellRenderers.Add("Header", new GridDataHeaderCellRenderer());
            cellRenderers.Add("StackedHeader", new GridStackedHeaderCellRenderer());
            cellRenderers.Add("GroupSummary", new GridSummaryCellRenderer());
            cellRenderers.Add("CaptionSummary", new GridCaptionSummaryCellRenderer());
            cellRenderers.Add("TableSummary", new GridTableSummaryCellRenderer());
            cellRenderers.Add("TextBlock", new GridCellTextBlockRenderer());
            cellRenderers.Add("TextBox", new GridCellTextBoxRenderer());
            cellRenderers.Add("CheckBox", new GridCellCheckBoxRenderer());
            cellRenderers.Add("Template", new GridCellTemplateRenderer());
            cellRenderers.Add("Image", new GridCellImageRenderer());
            cellRenderers.Add("UnBoundTemplateColumn", new GridUnBoundCellTemplateRenderer());
            cellRenderers.Add("UnBoundTextColumn", new GridUnBoundCellTextBoxRenderer());
            cellRenderers.Add("RowHeader", new GridRowHeaderCellRenderer());
            cellRenderers.Add("DetailsViewExpander", new GridDetailsViewExpanderCellRenderer());
            cellRenderers.Add("MultiColumnDropDown", new GridCellMultiColumnDropDownRenderer());
            cellRenderers.Add("ComboBox", new GridCellComboBoxRenderer());
            cellRenderers.Add("Numeric", new GridCellNumericRenderer());
            cellRenderers.Add("DateTime", new GridCellDateTimeRenderer());
            cellRenderers.Add("Hyperlink", new GridCellHyperlinkRenderer());
#if !WPF
            cellRenderers.Add("UpDown", new GridCellUpDownRenderer());
            cellRenderers.Add("ToggleSwitch", new GridCellToggleSwitchRenderer());
#else
            cellRenderers.Add("Currency", new GridCellCurrencyRenderer());
            cellRenderers.Add("Percent", new GridCellPercentageRenderer());
            cellRenderers.Add("TimeSpan", new GridCellTimeSpanRenderer());
            cellRenderers.Add("Mask", new GridCellMaskRenderer());
#endif
            unBoundRowCellRenderers.Add("UnBoundTemplateColumn", new GridUnBoundRowCellTemplateRenderer());
            unBoundRowCellRenderers.Add("UnBoundTextColumn", new GridUnBoundRowCellTextBoxRenderer());

            filterRowCellRenderers.Add("TextBox", new GridFilterRowTextBoxRenderer());
            filterRowCellRenderers.Add("Numeric", new GridFilterRowNumericRenderer());
            filterRowCellRenderers.Add("CheckBox", new GridFilterRowCheckBoxRenderer());
#if WPF
            filterRowCellRenderers.Add("MultiSelectComboBox", new GridFilterRowMultiSelectRenderer());
            filterRowCellRenderers.Add("ComboBox", new GridFilterRowComboBoxRenderer());
            filterRowCellRenderers.Add("DateTime", new GridFilterRowDateTimeRenderer());
#endif
        }

        private void InitializeSelectionController()
        {
            if (this.SelectionUnit == GridSelectionUnit.Row)
                this.SelectionController = new GridSelectionController(this);
            else
                this.SelectionController = new GridCellSelectionController(this);
        }

        /// <summary>
        /// Method which is helps to reset the selection based values.
        /// </summary>
        /// <remarks></remarks>
        private void ResetSelectionValues()
        {
            // WPF-36993- No need to clear selection whether SelectedItem is not available when changing ItemsSource of SfDataGrid.
            //if (this.SelectionController != null && (this.SelectionController.SelectedCells.Any() || this.SelectionController.SelectedRows.Any() || this.SelectionController.CurrentCellManager.CurrentCell != null))
            if (this.SelectionController != null)
                this.SelectionController.ClearSelections(false);
        }

        /// <summary>
        /// Method which is helps to hook the CollectionView based events.
        /// </summary>
        /// <remarks></remarks>
        private void WireViewEvents()
        {
            //var virtualView = View as VirtualizingCollectionView;
            //if (View == null || (View.SourceCollection == null && virtualView == null)) return;
            //if (this.View.SourceCollection is INotifyCollectionChanged)
            //{
            //    var notifyCollectionchanged = this.View.SourceCollection as INotifyCollectionChanged;
            //    notifyCollectionchanged.CollectionChanged += OnSourceCollectionChanged;
            //}
        }

        /// <summary>
        /// Method which is helps to hook the SfDataGrid based events.
        /// </summary>
        /// <remarks></remarks>
        private void WireDataGridEvents()
        {
            this.SizeChanged += OnSizeChanged;
            this.Loaded += OnLoaded;
            this.Unloaded += OnUnloaded;
        }
        
        private void OnLoaded(object sender, RoutedEventArgs e)
        {
#if !WPF
            IsLoaded = true;
#else
            if (this.RowGenerator.Items.Count > 0)
                this.GridModel.UpdateHeaderCells(false);

            if (this.SearchHelper == null)
                this.SearchHelper = new SearchHelper(this);
#endif
            //WPF-23465 – Validation flags reset on loading the grid which is not equals to Validations ActiveGrid.
            //WPF-24297 - To Avoid the SetCurrentCellValidated Method while the MultiColumnDrop-Down Column is in Editing in the ChildGrid.
            //While pressing Control+Homekey the Selection is maintained in both ParentGrid and ChldGrid.
            var skipValidation = SfMultiColumnDropDownControl.GetSkipValidation(this);
            if (this.ValidationHelper != null && this != ValidationHelper.ActiveGrid && !skipValidation)
            {
                //To avoid resetting of validation flags checked GetTopLevelParentDataGrid conditions. 
                if (ValidationHelper.ActiveGrid == null || (ValidationHelper.ActiveGrid != null && this.GetTopLevelParentDataGrid() != ValidationHelper.ActiveGrid.GetTopLevelParentDataGrid()))
                {
                    this.ValidationHelper.SetCurrentCellValidated(true);
                    this.ValidationHelper.SetCurrentRowValidated(true);
                }
            }
            //WPF-31262 - Apply the filter after the view is created
            if (GridModel.isFilterApplied)
            {
                foreach (var item in this.Columns.Where(c => c.FilterPredicates.Count > 0))
                    GridModel.ApplyFilterRowText(item);
                GridModel.isFilterApplied = false;
            }
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            //WPF-22097 – Validation flags reset on unloading. 
            if (this.ValidationHelper != null && this == ValidationHelper.ActiveGrid)
                this.ValidationHelper.ResetValidations(true);
        }

        /// <summary>
        /// Called when [size changed].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SizeChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //WPF-21074: Column Sizer Issue occurs in SfDataGrid when using inside SfMultiColumnDropDownControl or PopUp, 
            //since the SfDataGrid is measured before it came into view( Even with out opening the PopUp). 
            //Here the constraint size has been changed while opening the pop up due to new measurements and need to recalculate the column sizer once again. 
            //Hence i have recalculated the column sizer by previousArrangeWidth
            if (e.PreviousSize.Width != 0 || (container != null && container.previousArrangeWidth != 0))
            {
#if UWP
                //Its not need to invalidate when SfDataGrid is not loaded case. but in detailsview cases we need to invalidate either is loaded or not loaded.
                var isLoaded = this is DetailsViewDataGrid ? true : this.IsLoaded;
                //WPF-31886 In UWP when Window size has changed it did not trigger VisualContainer's MeaureOverride .So we manully invalidate the container in this case.
                if (container != null && isLoaded)
                    container.InvalidateMeasureInfo();
#endif
                if (this.CanQueryRowHeight() && this.IsLoaded && e.PreviousSize.Width != e.NewSize.Width)
                    this.container.RowHeightManager.Reset();
                return;
            }
            if (container != null)
                container.InvalidateMeasureInfo();
        }

        /// <summary>
        /// GridColumns collection changed event handling
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        internal void OnGridColumnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems.Count > 0)
                {
                    var column = e.NewItems[0] as GridColumn;
                    if (column.HeaderText == null)
                        column.HeaderText = column.MappingName;
                }
            }
            if (GridModel != null)
                this.GridModel.OnGridColumnCollectionChanged(sender, e);

            if (suspendForColumnPopulation || this.Columns.suspendUpdate)
                return;

            // Since ClonedDataGrid is maintained in SourceDataGrid NotifyListener, need to call SourceDataGrid's NotifyPropertyChanged to apply the changes to other grids
            if (this.NotifyListener != null)
                this.NotifyListener.SourceDataGrid.NotifyListener.NotifyCollectionChanged(this.Columns, e, datagrid => datagrid.Columns, this, typeof(GridColumn));

            //  In detailsview grid, if column is added at runtime, visual container will be null. So visual container check is moved here.
            if (!this.isGridLoaded)
                return;

            var newStartIndex = this.ResolveToScrollColumnIndex(e.NewStartingIndex);
            var oldStartIndex = this.ResolveToScrollColumnIndex(e.OldStartingIndex);
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    if (e.NewItems.Count > 0)
                    {
                        if (e.NewItems[0] is GridUnBoundColumn)
                            HasUnboundColumns = true;
                    }
                    // WPF-19238 - Need to refresh table summary row while adding new columns
#if WPF
                    if (!SfDataGrid.suspendForColumnMove)
                    {
                        this.RowGenerator.Items.ForEach(row =>
                            {
                                // WPF-19472 - After clearing all the columns, if we add new column, need to refresh TableSummaryCoveredRow
                                if (row.RowType == RowType.TableSummaryRow || (row.RowType == RowType.TableSummaryCoveredRow && this.Columns.Count == 1))
                                    row.RowIndex = -1;                               
                            });
                    }
#endif
                    // WPF-18333 - After clearing the columns at runtime, if new column is added, need to add indent columns also. so column count is updated here
                    if (this.VisualContainer.ColumnCount == 0)
                    {
                        this.UpdateColumnCount(false);
                        this.UpdateIndentColumnWidths();
                    }
                    else
                    {

#if WPF
                        if (!SfDataGrid.suspendForColumnMove)
#endif
                        this.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.ColumnsCollection);
#if UWP
                        this.container.InsertColumns(newStartIndex, 1);
#else
                        if (!SfDataGrid.suspendForColumnMove)
                            this.container.InsertColumns(newStartIndex, 1);
                        else
                            goto case NotifyCollectionChangedAction.Move;
#endif
                    }
                    break;
                case NotifyCollectionChangedAction.Replace:
                    {
                        var column = e.OldItems[0] as GridColumn;
                        column.ColumnPropertyChanged = null;

                        //WPF-29857 EndEdit the CurrentCell if the CurrentCell is in EditMode
                        if (this.SelectionController.CurrentCellManager.HasCurrentCell &&
                                         this.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                            this.SelectionController.CurrentCellManager.EndEdit(true);

                        this.GridModel.ResetColumnIndex(oldStartIndex);

                        this.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.ColumnsCollection);

                        column.IsHidden = false;
                        UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, column);
                        this.GridModel.ApplyFilter();

                        //WPF-29857 Remove from ColumnChooser if the Column is available in ColumnChooser
                        this.GridColumnDragDropController.ColumnHiddenChanged(column);
                        break;
                    }

                case NotifyCollectionChangedAction.Remove:
                    {
#if WPF
                        if (!SfDataGrid.suspendForColumnMove)
                        {
#endif
                        var column = e.OldItems[0] as GridColumn;
                        column.ColumnPropertyChanged = null;
                        // WPF-18761 - Need to clear selection and current cell while clearing the columns
                        if (this.Columns.Count == 0)
                        {
                            if (this.SelectionController != null)
                                this.SelectionController.ClearSelections(false);
                        }
                        //WPF-29857 EndEdit the CurrentCell if the CurrentCell is in EditMode
                        else if (this.SelectionController.CurrentCellManager.HasCurrentCell &&
                                     this.SelectionController.CurrentCellManager.CurrentCell.IsEditing)
                            this.SelectionController.CurrentCellManager.EndEdit(true);

                        //WPF-29408 We should handle the selection process after remove the columns. Otherwise the selection does not maintained properly.                                                      
                        this.container.RemoveColumns(oldStartIndex, 1);
                        this.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.ColumnsCollection);
                        column.IsHidden = false;

                        UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction.Remove, column);

                        //WPF-30402 Apply Filtering if FilterPredicates is available 
                        if (column.FilterPredicates.Count > 0)
                            this.GridModel.ApplyFilter(column);

                        //WPF-29857 Remove from ColumnChooser if the Column is available in ColumnChooser
                        this.GridColumnDragDropController.ColumnHiddenChanged(column);
#if WPF
                        }
                        else
                            this.oldIndexForMove = oldStartIndex;
#endif
                        HasUnboundColumns = this.Columns.Any(col => col.IsUnbound);

                    }
                    break;

                case NotifyCollectionChangedAction.Move:
                    //We should not refresh the ColumnSizer when removing the Column
                    //this.container.RemoveColumns(oldStartIndex, 1, true);
#if WPF
                    if (SfDataGrid.suspendForColumnMove)
                        oldStartIndex = this.oldIndexForMove;
#endif
                    this.container.RemoveColumns(oldStartIndex, 1);
                    this.container.InsertColumns(newStartIndex, 1);
#if WPF
                    var args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, e.NewItems[0], this.ResolveToGridVisibleColumnIndex(newStartIndex), this.ResolveToGridVisibleColumnIndex(oldStartIndex));
                    this.SelectionController.HandleCollectionChanged(args, CollectionChangedReason.ColumnsCollection);
#else
                    this.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.ColumnsCollection);
#endif
                    break;
                case NotifyCollectionChangedAction.Reset:
                    // WPF-18761 - Need to clear selection and current cell while clearing the columns
                    if (this.SelectionController != null)
                        this.SelectionController.ClearSelections(false);
                    this.container.RemoveColumns(0, container.ColumnCount);
                    UpdateDataOperationOnColumnCollectionChange(e.Action, null);
                    if (this.GroupDropArea != null)
                        this.GroupDropArea.RemoveAllGroupDropItems();
                    HasUnboundColumns = false;
                    break;
            }
            //WPF-24869 - Freeze columns updated when adding and removing columns
            this.UpdateFreezePaneColumns();
            this.container.UpdateScrollBars();
            this.container.NeedToRefreshColumn = true;
            this.container.InvalidateMeasureInfo();
            if (this.VisualContainer.ColumnCount > 0)
            {
                this.RowGenerator.RefreshStackedHeaders();
#if WPF
                if (!SfDataGrid.suspendForColumnMove)
#endif
                this.GridColumnSizer.Refresh();
                if (this.AllowResizingHiddenColumns && this.AllowResizingColumns)
                {
                    this.ColumnResizingController.EnsureVSMOnColumnCollectionChanged(e.OldStartingIndex, e.NewStartingIndex);
                }
            }
#if WPF
            // WPF-20488 - To refresh detailsview data row while changing column collection at runtime 
            if (!SfDataGrid.suspendForColumnMove)            
                this.RowGenerator.LineSizeChanged();            
#endif
        }
        /// <summary>
        /// Update the groupcolumndescriptions and sortcolumndesccriptions while changing the column collection in datagrid.
        /// </summary>
        /// <param name="action"></param>
        /// <param name="column"></param>
        private void UpdateDataOperationOnColumnCollectionChange(NotifyCollectionChangedAction action, GridColumn column = null)
        {
            if (action == NotifyCollectionChangedAction.Remove)
            {
                if (column == null)
                    throw new InvalidOperationException("You must pass column");

                if (this.GroupColumnDescriptions.Any(desc => desc.ColumnName == column.MappingName))
                {
                    this.GroupColumnDescriptions.Remove(
                        this.GroupColumnDescriptions.FirstOrDefault(
                            desc => desc.ColumnName == column.MappingName));
                    if (this.GroupDropArea != null)
                        this.GroupDropArea.RemoveGroupDropItem(column);
                }
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
                // While clearing the columns, all columns will be removed from visual container. So no need to reset columns while changing GroupColumnDescriptions
                isInColumnReset = true;
                if (this.GroupColumnDescriptions.Count > 0)
                    this.GroupColumnDescriptions.Clear();
                isInColumnReset = false;
            }
            else
            {
                throw new NotImplementedException("Not implemented for action" + action.ToString());
            }
        }

        /// <summary>
        /// Method which is helps to Unhook the CollectionView based events.
        /// </summary>
        /// <remarks></remarks>
        private void UnWireViewEvents()
        {
            // Commented due to issue in WP
            //if (View == null || View.SourceCollection == null) return;

            //if (this.View.SourceCollection is INotifyCollectionChanged)
            //{
            //    var notifyCollectionchanged = this.View.SourceCollection as INotifyCollectionChanged;
            //    notifyCollectionchanged.CollectionChanged -= OnSourceCollectionChanged;
            //}
        }

        /// <summary>
        /// Method which is helps to Unhook the SfDataGrid based events.
        /// </summary>
        /// <remarks></remarks>
        private void UnWireDataGridEvents()
        {
            this.SelectedItems.CollectionChanged -= OnSelectedItemsChanged;
            this.SizeChanged -= OnSizeChanged;
            this.Loaded -= OnLoaded;
            this.Unloaded -= OnUnloaded;
        }

        /// <summary>
        /// Ensures RowHeight,HeaderRowHeight,GroupDropAreaText,GridValidationMode,AllowFiltering  properties associated with SfDataGrid.
        /// </summary>
        /// <remarks></remarks>
        protected void EnsureProperties()
        {
            if (this.RowHeight != SfGridBase.rowHeight)
            {
                this.VisualContainer.RowHeights.DefaultLineSize = this.RowHeight;
                if (this.VisualContainer.RowCount > 0)
                {
                    this.VisualContainer.RowHeights.SetRange(0, this.GetHeaderIndex(), this.HeaderRowHeight);
                }
            }
            else if (this.HeaderRowHeight != SfGridBase.headerRowHeight && this.VisualContainer.RowCount > 0)
            {
                this.VisualContainer.RowHeights.SetRange(0, this.GetHeaderIndex(), this.HeaderRowHeight);
            }

            if (this.GroupDropAreaText != GridResourceWrapper.GroupDropAreaText && this.groupDropArea != null)
            {
                this.groupDropArea.GroupDropAreaText = this.GroupDropAreaText;
            }

            if (this.groupDropArea != null && this.groupDropArea.IsExpanded != this.IsGroupDropAreaExpanded)
            {
                this.groupDropArea.IsExpanded = this.IsGroupDropAreaExpanded;
            }
            if (IsChanged(AllowFilteringProperty) && this.Columns.Count > 0)
            {
                this.RefreshFilterIconVisibility();
            }
            if (this.IsChanged(SfDataGrid.GridValidationModeProperty))
            {
                foreach (GridColumn column in this.Columns)
                {
                    column.UpdateValidationMode();
                }
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
                    this.SelectionController.HandleCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, this.SelectedItems, 0), CollectionChangedReason.SelectedItemsCollection);
                }
                isSelectedItemsChanged = false;
            }

            isViewPropertiesEnsured = true;
        }

        /// <summary>
        /// Method which helps to initialize all the collection in SfDataGrid
        /// </summary>
        /// <remarks></remarks>
        private void InitializeCollections()
        {
#if WPF
            SetValue(ColumnsProperty, new Columns());
#endif
            SetValue(SelectedItemsProperty, new ObservableCollection<object>());
            SetValue(SortColumnDescriptionsProperty, new SortColumnDescriptions());
            SetValue(GroupColumnDescriptionsProperty, new GroupColumnDescriptions());
            SetValue(GroupSummaryRowsProperty, new ObservableCollection<GridSummaryRow>());
            SetValue(TableSummaryRowsProperty, new ObservableCollection<GridSummaryRow>());
            SetValue(UnBoundRowsProperty, new UnBoundRows());
            SetValue(StackedHeaderRowsProperty, new StackedHeaderRows());
            SetValue(SortComparersProperty, new SortComparers());
            SetValue(PrintSettingsProperty, new PrintSettings());
            SetValue(DetailsViewDefinitionProperty, new DetailsViewDefinition());
        }

        /// <summary>
        /// Method which helps to initialize the Table Summary rows
        /// </summary>
        /// <remarks></remarks>
        internal void InitializeTableSummaries()
        {
            if (this.View == null) return;
            foreach (var summaryRow in this.TableSummaryRows)
                this.View.TableSummaryRows.Add(summaryRow);
        }

        internal void InitializeGroupSummaryRows()
        {
            if (this.View == null) return;
            foreach (var summaryRow in this.GroupSummaryRows)
                this.View.SummaryRows.Add(summaryRow);
        }

        /// <summary>
        /// Method which initializes CaptionSummaryRow in View
        /// </summary>       
        internal void InitializeCaptionSummaryRow()
        {
            if (this.View != null)
            {
                this.View.CaptionSummaryRow = this.CaptionSummaryRow;
            }
        }

#endregion

#region Internal Methods

        /// <summary>
        /// Method which helps to update the rowcolumn count initially
        /// </summary>
        /// <param name="canGenerateVisibleColumns">If set to <see langword="true"/>, then ; otherwise, .</param>
        /// <remarks></remarks>
        internal void UpdateRowAndColumnCount(bool canGenerateVisibleColumns)
        {
            if (this.container == null)
                return;

            (VisualContainer.ColumnWidths as LineSizeCollection).SuspendUpdates();

            this.UpdateColumnCount(canGenerateVisibleColumns);

            if (AutoGenerateRelations)
                GenerateGridRelations();

            if (this.StackedHeaderRows != null && this.StackedHeaderRows.Count > 0)
            {
                this.InitializeStackedColumnChildDelegate();
            }

            if (this.View != null)
                this.UpdateRowCount();

            if (this.container.RowCount > 0)
            {
                for (int i = 0; i <= this.GetHeaderIndex(); i++)
                {
                    this.container.RowHeights[i] = this.HeaderRowHeight;
                }
            }
            this.HasUnboundColumns = Columns.Any(col => col.IsUnbound);

            // When change the item source at runtime the hidden columns are maintained in hidden state. So this method is called to reset the hidden columns
            (VisualContainer.ColumnWidths as LineSizeCollection).ResetHiddenState();

            for (var i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].Width == 0)
                    Columns[i].IsHidden = true;
                if (!Columns[i].IsHidden) continue;
                var index = i;
                if (ShowRowHeader)
                    index += 1;

                if (this.DetailsViewManager.HasDetailsView)
                    index += 1;

                if (View != null && View.GroupDescriptions.Count > 0)
                    index += GroupColumnDescriptions.Count;

                VisualContainer.ColumnWidths.SetHidden(index, index, true);
            }
            this.UpdateIndentColumnWidths();
            (VisualContainer.ColumnWidths as LineSizeCollection).ResumeUpdates();
            this.container.UpdateScrollBars();

            if (this.View == null)
            {
                if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None && canGenerateVisibleColumns && this.Columns.Count <= 0)
                {
                    this.container.ColumnCount = 0;
                    this.container.FooterColumns = 0;
                    this.container.FrozenColumns = 0;
                    this.container.RowCount = 0;
                    this.container.FrozenRows = 0;
                }
                else
                {
                    if (this.Columns.Count > 0)
                    {
                        this.container.RowCount = this.GetHeaderIndex() + 1 + this.UnBoundRows.Count;
                        this.container.FrozenRows = this.StackedHeaderRows != null ? this.StackedHeaderRows.Count + 1 + this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false) : this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, false) + 1;
                    }
                    else
                    {
                        this.container.RowCount = 0;
                        this.container.FrozenRows = 0;
                    }

                }
                if (this.Columns.Count > 0)
                    this.container.FooterRows = this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
                else
                    this.container.FooterRows = 0;
                this.container.UpdateScrollBars();
            }
            else
                UpdateFreezePaneColumns();
        }

        /// <summary>
        /// Set width for indent columns
        /// </summary>
        internal void UpdateIndentColumnWidths()
        {
            var firstIndex = ShowRowHeader ? 1 : 0;
            if (ShowRowHeader && this.container.ColumnCount > 0)
                VisualContainer.ColumnWidths[0] = this.RowHeaderWidth;
            if (View != null)
                this.View.GroupDescriptions.ForEach(
                    desc => VisualContainer.ColumnWidths[firstIndex++] = this.IndentColumnWidth);

            if (this.DetailsViewManager.HasDetailsView)
                VisualContainer.ColumnWidths[firstIndex] = this.ExpanderColumnWidth;

        }

        /// <summary>
        /// Updates the Frozen Rows and Footer Rows
        /// </summary>
        internal void UpdateFreezePaneRows()
        {
            if (this.FrozenRowsCount > 0 && !this.AllowFrozenGroupHeaders && this.container.RowCount >= this.ResolveToRowIndex(this.FrozenRowsCount) + this.GetTableSummaryCount(TableSummaryRowPosition.Bottom))
                this.container.FrozenRows = this.FrozenRowsCount + headerLineCount;
            else
                this.container.FrozenRows = headerLineCount;

            if (AllowFooterRows(this.FooterRowsCount))
                this.container.FooterRows = this.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + this.FooterRowsCount + this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
            else
                this.container.FooterRows = this.GetTableSummaryCount(TableSummaryRowPosition.Bottom) + this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom, true);
        }

        /// <summary>
        /// Updates the Frozen Column and Footer Columns
        /// </summary>
        internal void UpdateFreezePaneColumns()
        {
            if (this.FrozenColumnCount > 0 && this.container.ColumnCount >= this.ResolveToScrollColumnIndex(this.FrozenColumnCount))
                this.container.FrozenColumns = this.ResolveToScrollColumnIndex(this.FrozenColumnCount);
            else if (this.ShowRowHeader && this.container.ColumnCount > 1)
                this.container.FrozenColumns = 1;
            else
                this.container.FrozenColumns = 0;

            if (AllowFooterColumns(FooterColumnCount))
                this.container.FooterColumns = this.FooterColumnCount;
            else
                this.container.FooterColumns = 0;
        }
        
        /// <summary>
        /// Updates the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoScroller"/> settings .
        /// </summary>
        protected internal void UpdateAutoScroller()
        {
            this.AutoScroller.VisualContainer = this.VisualContainer;
            this.AutoScroller.AutoScrollBounds = this.container.GetClipRect(ScrollAxisRegion.Header, ScrollAxisRegion.Footer);
            this.AutoScroller.IntervalTime = new TimeSpan(0, 0, 0, 0, 40);
#if WinRT || UNIVERSAL
            this.AutoScroller.InsideScrollMargin = new Size(20, 20);
#else
            this.AutoScroller.InsideScrollMargin = new Size(0, 0);
#endif
        }

        internal void UpdateColumnCount(bool canGenerateVisibleColumns)
        {
            int columnCount;
            if (this.AutoGenerateColumns && this.AutoGenerateColumnsMode != AutoGenerateColumnsMode.None && canGenerateVisibleColumns && (this.Columns.Count <= 0 || this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.Reset))
            {
                this.GenerateGridColumns();
                columnCount = this.Columns.Count;
            }
            else
                columnCount = this.Columns.Count;
            if (this.ShowRowHeader)
                columnCount += 1;
            if (View != null && View.GroupDescriptions.Count > 0)
                columnCount += View.GroupDescriptions.Count;

            if (this.DetailsViewManager.HasDetailsView)
                columnCount += 1;

            this.container.ColumnCount = columnCount;
        }


        internal void RemoveLine(int removeAt, int count)
        {
            var lineSizeCollection = this.VisualContainer.RowHeights as LineSizeCollection;
            int level = count;

            if (!this.GridModel.HasGroup)
            {
                if (this.DetailsViewManager.HasDetailsView)
                    level += this.DetailsViewDefinition.Count;
            }

            lineSizeCollection.RemoveLines(removeAt, level);
            if (this.DetailsViewManager.HasDetailsView)
            {
                this.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                {
                    if (row.CatchedRowIndex > removeAt)
                        row.CatchedRowIndex -= level;
                });
            }
        }

        internal void InsertLine(int insertAt, int count, int recordStartIndex = 0, int recordCount = 0)
        {
            var lineSizeCollection = this.VisualContainer.RowHeights as LineSizeCollection;
            int level = count;

            if (!this.GridModel.HasGroup)
            {
                if (this.DetailsViewManager.HasDetailsView)
                    level += this.DetailsViewDefinition.Count;
            }

            lineSizeCollection.InsertLines(insertAt, level);
            
            if (this.DetailsViewManager.HasDetailsView)
            {
                this.RowGenerator.Items.OfType<DetailsViewDataRow>().ForEach(row =>
                {
                    if (row.CatchedRowIndex > insertAt)
                        row.CatchedRowIndex += level;
                });
                if (!this.GridModel.HasGroup)
                {
                    for (int i = 1; i < level; i++)
                    {
                        this.VisualContainer.RowHeights.SetHidden(insertAt + i, insertAt + i, true);
                        this.VisualContainer.RowHeights.SetNestedLines(insertAt + i, null);
                    }
                }
                else
                {
                    if (recordCount != 0)
                    {
                        var endIndex = recordStartIndex + recordCount + (recordCount * DetailsViewDefinition.Count);
                        lineSizeCollection.SetHiddenIntervalWithState(recordStartIndex, endIndex, this.DetailsViewManager.GetHiddenPattern());
                    }
                }
            }
        }

        /// <summary>
        /// Method which helps to update the row count while grouping and filtering....
        /// </summary>
        /// <remarks></remarks>
        internal void UpdateRowCount()
        {
            var lineSizeCollection = this.VisualContainer.RowHeights as LineSizeCollection;
            lineSizeCollection.SuspendUpdates();
            if (this.CanQueryRowHeight())
            {
                lineSizeCollection.RemoveLines(0, lineSizeCollection.LineCount);
            }
            //if AutoGenerateColumnsMode is None, we should calculate the rowcount whether the columns in added to grid column collections.
            //WPF-28454 For avoiding the DetailsViewGrid Rendering issue we  have to check ItemsSource as not equal to null       
            if (this.ItemsSource != null || this.AutoGenerateColumnsMode != Grid.AutoGenerateColumnsMode.None || this.Columns.Count > 0)
            {
                int rowCount = 0;

                if (this.View.GroupDescriptions.Count > 0)
                    rowCount = this.View.TopLevelGroup.DisplayElements.Count;
                else
                    rowCount = this.DetailsViewManager.HasDetailsView ? this.View.Records.Count * (DetailsViewDefinition.Count + 1) : this.View.Records.Count;
                
                rowCount = rowCount + headerLineCount;
                rowCount += this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom) + this.GetUnBoundRowsCount(UnBoundRowsPosition.Top, true);

                rowCount += this.GetTableSummaryCount(TableSummaryRowPosition.Bottom);
                if (this.FilterRowPosition == FilterRowPosition.Bottom || this.FilterRowPosition == FilterRowPosition.Top)
                    rowCount += 1;

                if (AddNewRowPosition == Grid.AddNewRowPosition.Bottom || AddNewRowPosition == AddNewRowPosition.Top)
                    rowCount += 1;
                this.container.RowCount = rowCount;
                this.UpdateFreezePaneRows();
            }

            lineSizeCollection.ResetHiddenState();
            if (this.CanQueryRowHeight())
            {
                this.VisualContainer.RowHeightManager.Reset();
                //Set Header Height          
                if (this.VisualContainer.RowCount > 0)
                {
                    this.VisualContainer.RowHeights.SetRange(0, headerLineCount - 1, this.HeaderRowHeight);
                }
            }
            lineSizeCollection.ResumeUpdates();

            if (!this.DetailsViewManager.HasDetailsView) return;

            if (this.View.GroupDescriptions.Count > 0)
                SetExpandedState(this.View.TopLevelGroup);
            else
            {
                var hiddenPattern = new List<bool> { false };
                DetailsViewDefinition.ForEach(r => hiddenPattern.Add(true));
                var startIdx = this.ResolveStartIndexBasedOnPosition();
                var endindex = this.VisualContainer.RowHeights.LineCount -
                    ((this.GetTableSummaryCount(TableSummaryRowPosition.Bottom) +
                    this.GetUnBoundRowsCount(UnBoundRowsPosition.Bottom)));
                lineSizeCollection.SetHiddenInterval(startIdx, endindex, hiddenPattern.ToArray());
            }
        }

        /// <summary>
        /// Methos which helps to update the view after group operation has done
        /// </summary>
        /// <remarks></remarks>
        internal void UpdateRowCountAndScrollBars()
        {
            if (this.container != null && this.View != null)
            {
                this.UpdateRowCount();
                this.container.UpdateScrollBars();
                if (this.container.ScrollOwner != null)
                    this.container.ScrollOwner.InvalidateScrollInfo();
                if (this.container.ScrollableOwner != null)
                    this.container.ScrollableOwner.InvalidateScrollInfo();
            }
        }

        internal bool CanSetAllowFilters(GridColumn column)
        {
            if (this.View is Syncfusion.Data.PagedCollectionView)
            {
                if ((this.View as Syncfusion.Data.PagedCollectionView).UseOnDemandPaging)
                    return false;
            }
            return column.AllowFiltering;
        }

        internal void SetBusyState(string stateName)
        {
            if (ShowBusyIndicator)
            {
                VisualStateManager.GoToState(this, stateName, true);
            }
        }

#endregion

#region Event Call Back Methods

        /// <summary>
        /// Method which is called when the SelectedItems Changed
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:System.Collections.Specialized.NotifyCollectionChangedEventArgs">NotifyCollectionChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        private void OnSelectedItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            //WPF-26003- Row will be get selection automatically while we scroll the Vertically 
            var grid = this as SfDataGrid;
            if (grid == null || !grid.isGridLoaded || grid.View == null)
            {
                grid.isSelectedItemsChanged = true;
                return;
            }
            grid.isSelectedItemsChanged = false;
            this.SelectionController.HandleCollectionChanged(e, CollectionChangedReason.SelectedItemsCollection);
        }

        /// <summary>
        /// Helper methods to update the view when the CaptionSummary row changed.
        /// </summary>
        /// <param name="row"></param>
        /// <remarks></remarks>
        private void OnCaptionSummaryRowChanged(GridSummaryRow row)
        {
            if (this.IsInDeserialize)
                return;
            InitializeCaptionSummaryRow();
            this.GridModel.OnCaptionSummaryRowChanged(row);
        }

#endregion


#region Details View

#region Fields

        internal IDetailsViewNotifyListener notifyListener;
        internal bool AllowDetailsViewPadding = true;
        private bool isListenerSuspended;
        /// <summary>
        /// Flag indicate the DataGrid is wrapper for DetailsViewGrid's. 
        /// </summary>
        /// <remarks>
        /// This is not the DataGrid which is displayed in UI. It's a wrapper for DataGrid's  DetailsView
        /// </remarks>
        internal bool IsSourceDataGrid;

#endregion

#region Public DP, Methods and Events

        /// <summary>
        /// Gets or sets a value that indicates whether the relations for Master-Details View  is generated automatically.
        /// </summary>
        /// <value> 
        /// <b>true</b> if relations is auto-generated; otherwise, <b>false</b>.The default value is <b>false</b>.
        /// </value>
        /// <remarks>        
        /// You can cancel or customize the relation being created in the<see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGeneratingRelations"/> event handler. 
        /// </remarks>
        public bool AutoGenerateRelations
        {
            get { return (bool)GetValue(AutoGenerateRelationsProperty); }
            set { SetValue(AutoGenerateRelationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the DetailsViewDataGrid that are currently selected .
        /// </summary>        
        [Cloneable(false)]
        public SfDataGrid SelectedDetailsViewGrid
        {
            get { return (SfDataGrid)GetValue(SelectedDetailsViewGridProperty); }
            set { SetValue(SelectedDetailsViewGridProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the expander should be hidden when relational column property has an empty collection or null.
        /// </summary>
        /// <value>
        /// <b>true</b> if the expander is hidden from the view; otherwise , <b>false</b>. The default value is <b>false</b>.
        /// </value>        
        public bool HideEmptyGridViewDefinition
        {
            get { return (bool)GetValue(HideEmptyGridViewDefinitionProperty); }
            set { SetValue(HideEmptyGridViewDefinitionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the padding of the DetailsViewDataGrid
        /// </summary>
        /// <value>
        /// The padding for the DetailsViewDataGrid.
        /// </value>
        /// <remarks>
        /// The DetailsViewPadding needs to be defined to its parent DataGrid , when the child grid is required padding.
        /// </remarks>        
        [Cloneable(false)]
        public Thickness DetailsViewPadding
        {
            get { return (Thickness)GetValue(DetailsViewPaddingProperty); }
            set { SetValue(DetailsViewPaddingProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateRelations dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateRelations dependency property.
        /// </remarks>         
        public static readonly DependencyProperty AutoGenerateRelationsProperty =
            GridDependencyProperty.Register("AutoGenerateRelations", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(default(bool)));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedDetailsViewGrid dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedDetailsViewGrid dependency property.
        /// </remarks>         
        public static readonly DependencyProperty SelectedDetailsViewGridProperty =
            GridDependencyProperty.Register("SelectedDetailsViewGrid", typeof(SfDataGrid), typeof(SfDataGrid), new GridPropertyMetadata(default(SfDataGrid)));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.HideEmptyGridViewDefinition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.HideEmptyGridViewDefinition dependency property.
        /// </remarks>         
        public static readonly DependencyProperty HideEmptyGridViewDefinitionProperty =
            GridDependencyProperty.Register("HideEmptyGridViewDefinition", typeof(bool), typeof(SfDataGrid), new GridPropertyMetadata(default(bool), OnHideEmptyGridViewDefinitionChanged));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewPadding dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfDataGrid.DetailsViewPadding dependency property.
        /// </remarks>         
        public static readonly DependencyProperty DetailsViewPaddingProperty =
            GridDependencyProperty.Register("DetailsViewPadding", typeof(Thickness), typeof(SfDataGrid), new GridPropertyMetadata(new Thickness(6, 6, 2, 6)));

        /// <summary>
        /// Expands all the DetailsViewDataGrids in the SfDataGrid.
        /// </summary>
        public void ExpandAllDetailsView()
        {
            this.DetailsViewManager.ExpandAllDetailsView();
        }

        /// <summary>
        /// Expands the DetailsViewDataGrid corresponding up to specified level.
        /// </summary>
        public void ExpandAllDetailsView(int level)
        {
            if (level <= 0)
                return;
            this.DetailsViewManager.ExpandAllDetailsView(level);
        }

        /// <summary>
        /// Collapses all the DetailsViewDataGrids in SfDataGrid.
        /// </summary>
        public void CollapseAllDetailsView()
        {
            this.DetailsViewManager.CollapseAllDetailsView();
        }

        /// <summary>
        /// Expands the DetailsViewDataGrid corresponding to the specified record index.
        /// </summary>
        /// <param name="recordIndex">Index of the record to expand the Details View.</param>
        /// <returns>Returns <b>true</b> if the record is expanded.otherwise <b>false</b>.</returns>
        public bool ExpandDetailsViewAt(int recordIndex)
        {
            return this.DetailsViewManager.ExpandDetailsViewAt(recordIndex);
        }

        /// <summary>
        /// Collapses the DetailsViewDataGrids corresponding to the specified record index.
        /// </summary>
        /// <param name="recordIndex">Index of the record to collapse the Details View.</param>
        public void CollapseDetailsViewAt(int recordIndex)
        {
            this.DetailsViewManager.CollapseDetailsViewAt(recordIndex);
        }

        /// <summary>
        /// Occurs when the DetailsViewDataGrid is being expanded.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the  DetailsViewDataGrid being expanded through <see cref="Syncfusion.UI.Xaml.Grid.GridDetailsViewExpandingEventArgs"/> event argument.
        /// </remarks>
        public event GridDetailsViewExpandingEventHandler DetailsViewExpanding;

        /// <summary>
        /// Occurs after the DetailsViewDataGrid is expanded.
        /// </summary>
        public event GridDetailsViewExpandedEventHandler DetailsViewExpanded;

        /// <summary>
        /// Occurs when the DetailsViewDataGrid is being collapsed.
        /// </summary>
        /// <remarks>
        /// You can cancel or customize the details view being collapsed through <see cref="Syncfusion.UI.Xaml.Grid.GridDetailsViewCollapsingEventArgs"/> event argument.
        /// </remarks>
        public event GridDetailsViewCollapsingEventHandler DetailsViewCollapsing;

        /// <summary>
        /// Occurs after the DetailsViewDataGrid is collapsed.
        /// </summary>
        public event GridDetailsViewCollapsedEventHandler DetailsViewCollapsed;

        /// <summary>
        /// Occurs when the DetailsViewDataGrid is being loaded in to view. 
        /// </summary>
        /// <remarks>
        /// You can set the custom renderer, SelectionController ResizingController ,GridColumnDragDropController and GridColumnSizer to the DetailsViewDataGrid through the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewLoadingAndUnloadingEventArgs.DetailsViewDataGrid"/>  argument in the <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewLoadingAndUnloadingEventArgs"/> class.
        /// </remarks>
        public event DetailsViewLoadingAndUnloadingEventHandler DetailsViewLoading;

        /// <summary>
        ///  Occurs when the DetailsViewDataGrid is being unloaded from the view.
        /// </summary>
        public event DetailsViewLoadingAndUnloadingEventHandler DetailsViewUnloading;

        /// <summary>
        /// Occurs when the relations for Master-Details View is generated automatically.
        /// </summary>
        /// <remarks>
        /// This event will be raised when the <see cref="AutoGeneateRelations"/> is set to true and relation is auto generated.You can cancel or customize the Master-Details View relation being generated through the <see cref="Syncfusion.UI.Xaml.Grid.AutoGeneratingRelationsArgs"/> event argument.
        /// </remarks>
        public event AutoGeneratingRelationsEventHandler AutoGeneratingRelations;


#endregion

#region Private Methods

        private void GenerateGridRelations()
        {
            if (!AutoGenerateRelations)
                return;

            if (this.Columns == null || this.View == null) return;

            if (DetailsViewDefinition != null && (DetailsViewDefinition == null || DetailsViewDefinition.Count != 0))
                return;

            if (DetailsViewDefinition == null)
                DetailsViewDefinition = new DetailsViewDefinition();
            else
                DetailsViewDefinition.Clear();

            var properties = this.View.GetItemProperties();
#if !WPF
            foreach (var keyvaluepair in properties)
            {
                var isRelationalType = GridUtil.IsComplexType(keyvaluepair.Value.PropertyType) &&
                                          !typeof(byte[]).IsAssignableFrom(keyvaluepair.Value.PropertyType);
#else
            foreach (PropertyDescriptor pd in properties)
            {
                if (!this.IsLegacyDataTable)
                {
                    var isRelationalType = GridUtil.IsComplexType(pd.PropertyType) &&
                                           !typeof(byte[]).IsAssignableFrom(pd.PropertyType);
#endif
                if (isRelationalType)
                {
                    var canGenerateRelation = true;
#if WPF
                        var attr = pd.Attributes.ToList<Attribute>().FirstOrDefault(a => a is DisplayAttribute);
#else
                    var attr = keyvaluepair.Value.GetCustomAttributes(typeof(Attribute), true).FirstOrDefault(a => a is DisplayAttribute);
#endif
                    if (attr != null)
                    {
                        var displayAttribute = attr as DisplayAttribute;
                        if (displayAttribute != null && displayAttribute.GetAutoGenerateField() != null)
                            canGenerateRelation = displayAttribute.AutoGenerateField;
                    }
                    if (!canGenerateRelation)
                        continue;
#if WPF
                        ExtractRelationalColumn(pd);
#else
                    ExtractRelationalColumn(keyvaluepair.Value);
#endif
                }
#if WPF
                }
                else
                {
                    var datarelation = GridUtil.GetDataRelation(pd);
                    if (datarelation != null)
                    {
                        var cancel = false;
                        var gridView = new GridViewDefinition { RelationalColumn = datarelation.RelationName };
                        var args = this.RaiseAutoGeneratingRelationsEvent(new AutoGeneratingRelationsArgs(gridView, this) { Cancel = cancel });
                        if (!args.Cancel)
                            this.DetailsViewDefinition.Add(args.GridViewDefinition);
                        // If AutoGenerateRelations is True, need to add DetailsViewDefinition for SourceDataGrid here
                        if (this.NotifyListener != null && this.DetailsViewDefinition.Any())
                            CloneHelper.CloneCollection(this.DetailsViewDefinition, this.NotifyListener.SourceDataGrid.DetailsViewDefinition, typeof(ViewDefinition));
                    }
                }
#endif
            }
        }

#if WPF
        private void ExtractRelationalColumn(PropertyDescriptor pd)
#else
        private void ExtractRelationalColumn(PropertyInfo pd)
#endif
        {
            ////sometimes we get a byte[] array for a column that has images
            var isNestedCollection = pd.PropertyType != typeof(string) && typeof(IEnumerable).IsAssignableFrom(pd.PropertyType)
#if !WinRT && !UNIVERSAL
 && !(pd.PropertyType.IsArray && pd.PropertyType.GetElementType().IsPrimitive);
#else
 && !(pd.PropertyType.IsArray && pd.PropertyType.GetElementType().IsPrimitive());
#endif
            if (!isNestedCollection) return;
            var cancel = false;
            var gridView = new GridViewDefinition { RelationalColumn = pd.Name };
            var args = this.RaiseAutoGeneratingRelationsEvent(new AutoGeneratingRelationsArgs(gridView, this) { Cancel = cancel });
            if (!args.Cancel)
                this.DetailsViewDefinition.Add(args.GridViewDefinition);
            // If AutoGenerateRelations is True, need to add DetailsViewDefinition for SourceDataGrid here
            if (this.NotifyListener != null && this.DetailsViewDefinition.Any())
                CloneHelper.CloneCollection(this.DetailsViewDefinition, this.NotifyListener.SourceDataGrid.DetailsViewDefinition, typeof(ViewDefinition));
        }

        internal void SetExpandedState(Group group)
        {
            var lineSizeCollection = this.VisualContainer.RowHeights as LineSizeCollection;
            if (!group.IsBottomLevel)
            {
                foreach (var childGroup in group.Groups)
                    if (childGroup.IsExpanded)
                        SetExpandedState(childGroup);
            }
            else
            {
                if (group.IsExpanded)
                {
                    var startIdx = this.ResolveStartIndexOfGroup(group) + 1;
                    var recordCount = group.GetRecordCount();
                    var endIndex = startIdx + recordCount;
                    var hiddenPattern = new List<bool> { false };
                    DetailsViewDefinition.ForEach(r => hiddenPattern.Add(true));
                    lineSizeCollection.SetHiddenIntervalWithState(startIdx, endIndex, hiddenPattern.ToArray());
                    startIdx++;
                    foreach (var record in group.Records)
                    {
                        if (record.IsExpanded)
                            lineSizeCollection.SetHidden(startIdx, startIdx, false);
                        startIdx += 2;
                    }
                }
            }
        }

#endregion

#region internal Methods

        internal void ForceInitializeDetailsViewGrid()
        {
            IsSourceDataGrid = true;
            this.groupColumnDescriptionsCopy = new GroupColumnDescriptions();
            // need to set tempGroupColumnDescriptions based on GroupColumnDescriptions
            if (this.GroupColumnDescriptions != null && this.GroupColumnDescriptions.Any())
            {
                foreach (var groupColumn in this.GroupColumnDescriptions)
                {
                    this.groupColumnDescriptionsCopy.Add(groupColumn);
                    var sortColumn = this.SortColumnDescriptions.FirstOrDefault(desc => desc.ColumnName == groupColumn.ColumnName);
                    // Add column in SortColumnDescriptions
                    if (sortColumn == default(SortColumnDescription))
                        this.SortColumnDescriptions.Add(new SortColumnDescription() { ColumnName = groupColumn.ColumnName, SortDirection = ListSortDirection.Ascending });
                }
            }
            // ResetAll is not applicable for DetailsViewDataGrid, So Reset will be set
            if (this.AutoGenerateColumnsMode == AutoGenerateColumnsMode.ResetAll)
                this.AutoGenerateColumnsMode = AutoGenerateColumnsMode.Reset;
            WireEvents();
        }

        private void EnsureColumnSettings()
        {
            if (this.Columns == null)
                return;
            this.Columns.ForEach(x => this.GridModel.WireColumnDescriptor(x));
        }

        internal bool RaiseDetailsViewExpanding(GridDetailsViewExpandingEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseDetailsViewExpanding(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseDetailsViewExpanding(e);
                }
                return !e.Cancel;
            }

            if (DetailsViewExpanding != null) DetailsViewExpanding(this, e);
            return !e.Cancel;
        }

        internal void RaiseDetailsViewExpanded(GridDetailsViewExpandedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseDetailsViewExpanded(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseDetailsViewExpanded(e);
                }
             }

            if (DetailsViewExpanded != null) DetailsViewExpanded(this, e);
        }

        internal bool RaiseDetailsViewCollapsing(GridDetailsViewCollapsingEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseDetailsViewCollapsing(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseDetailsViewCollapsing(e);
                }
                return !e.Cancel;
            }
            if (DetailsViewCollapsing != null) DetailsViewCollapsing(this, e);
            return !e.Cancel;
        }


        internal void RaiseDetailsViewLoading(DetailsViewLoadingAndUnloadingEventArgs e)
        {
            // we need to skip, If the event is already fired for detailsViewgrid
            if (e.DetailsViewDataGrid.IsLoadedEventFired)
                return;
            // If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid. In this, OriginalSender will be actual DetailsViewDataGrid
            if (NotifyListener != null && !IsSourceDataGrid && NotifyListener.SourceDataGrid.DetailsViewLoading != null)
            {
                this.NotifyListener.SourceDataGrid.RaiseDetailsViewLoading(e);
                return;
            }
            if (DetailsViewLoading != null)
                DetailsViewLoading(this, e);
            e.DetailsViewDataGrid.IsLoadedEventFired = true;
        }

        internal void RaiseDetailsViewUnloading(DetailsViewLoadingAndUnloadingEventArgs e)
        {
            e.DetailsViewDataGrid.IsLoadedEventFired = false;
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseDetailsViewUnloading(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseDetailsViewUnloading(e);
                }
            }

            if (DetailsViewUnloading != null)
                DetailsViewUnloading(this, e);
        }

        internal void RaiseDetailsViewCollapsed(GridDetailsViewCollapsedEventArgs e)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseDetailsViewCollapsed(e);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseDetailsViewCollapsed(e);
                }
            }
            if (DetailsViewCollapsed != null) DetailsViewCollapsed(this, e);
        }

        internal AutoGeneratingRelationsArgs RaiseAutoGeneratingRelationsEvent(AutoGeneratingRelationsArgs args)
        {
            if (NotifyListener != null && !IsSourceDataGrid)
            {
                //If the grid is DetailsViewDataGrid, event will be raised for its SourceDataGrid.In this, OriginalSender will be actual DetailsViewDataGrid
                if (NotifyListener.SourceDataGrid != null)
                    NotifyListener.SourceDataGrid.RaiseAutoGeneratingRelationsEvent(args);

                //The MainGrid event will be fired while we enable the EnableEventListenerfromParentDataGrid property in SourceDataGrid
                if (NotifyListener.SourceDataGrid.NotifyEventsToParentDataGrid)
                {
                    var parentGrid = SelectionHelper.GetTopLevelParentDataGrid(this);
                    if (parentGrid != null)
                        parentGrid.RaiseAutoGeneratingRelationsEvent(args);
                }
                return args;
            }
            if (AutoGeneratingRelations != null)
                this.AutoGeneratingRelations(this, args);
            return args;
        }
#endregion

#region IDetailsViewNotifyListener

        [Cloneable(false)]
        public IDetailsViewNotifyListener NotifyListener
        {
            get { return notifyListener; }
            internal set { this.notifyListener = value; }
        }
      
		[Cloneable(false)]      
        public bool IsListenerSuspended
        {
            get { return isListenerSuspended; }
        }
    
        public void SetNotifierListener(IDetailsViewNotifyListener notifyListener)
        {
            this.notifyListener = notifyListener;
        }

        public void SuspendNotifyListener()
        {
            isListenerSuspended = true;
        }

        public void ResumeNotifyListener()
        {
            isListenerSuspended = false;
        }

#endregion

#endregion

#region INotifyDependencyPropertyChanged

        public void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e)
        {
            // Since ClonedDataGrid is maintained in SourceDataGrid's NotifyListener, need to call SourceDataGrid's NotifyPropertyChanged to apply the changes to other grids
            if (this.NotifyListener != null && this.NotifyListener.SourceDataGrid.NotifyListener != null)
                this.NotifyListener.SourceDataGrid.NotifyListener.NotifyPropertyChanged(this, propertyName, e, dataGrid => dataGrid, this, typeof(SfGridBase));
        }

#endregion

#region IDisposable Member
        
        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid"/> class.
        /// </summary>
        protected override void Dispose(bool isDisposing)
        {
            if (isdisposed)
                return;
            if (!(this is DetailsViewDataGrid))
                ReuseRowsOnItemssourceChange = false;
            this.UnWireEvents();
            this.UnWireDataGridEvents();
            this.isGridLoaded = false;
            if (isDisposing)
            {
                if (this.GridModel != null)
                {
                    this.GridModel.Dispose();
                    this.GridModel = null;
                }
                if (this.groupDropArea != null)
                {
                    this.groupDropArea.Dispose();
                    this.groupDropArea = null;
                }
                if (this.GridColumnDragDropController != null)
                {
                    this.GridColumnDragDropController.Dispose();
                    this.GridColumnDragDropController = null;
                }

                if (this.container != null)
                {
#if !WPF
                    this.container.ContainerKeydown = null;
#endif
                    this.container.Dispose();
                    this.container = null;
                }

                if (this.PrintSettings.PrintManagerBase != null)
                {
                    this.PrintSettings.PrintManagerBase.Dispose();
                    this.PrintSettings.PrintManagerBase = null;
                }

                if (this.PrintSettings != null)
                {
                    this.PrintSettings = null;
                }

                if (this.RowGenerator != null)
                {
                    this.RowGenerator.Dispose();
                    this.rowGenerator = null;
                }
                if (this.GridColumnSizer != null)
                {
                    this.GridColumnSizer.Dispose();
                    this.GridColumnSizer = null;
                }
                if (needToDisposeView && this.View != null)
                {
                    this.View.Dispose();
                    this.View = null;
                }
                if (this.cellRenderers != null)
                {
                    this.cellRenderers.Dispose();
                    this.cellRenderers = null;
                }
                if (this.unBoundRowCellRenderers != null)
                {
                    this.unBoundRowCellRenderers.Dispose();
                    this.unBoundRowCellRenderers = null;
                }
                if (this.GroupColumnDescriptions != null)
                {
                    this.GroupColumnDescriptions.Clear();
                    this.ClearValue(SfDataGrid.GroupColumnDescriptionsProperty);
                }

                if (this.SortColumnDescriptions != null)
                {
                    this.SortColumnDescriptions.Clear();
                    this.ClearValue(SfDataGrid.SortColumnDescriptionsProperty);
                }

                if (this.SortComparers != null)
                {
                    this.SortComparers.Clear();
                    this.ClearValue(SfDataGrid.SortComparersProperty);
                }

                if (this.GroupSummaryRows != null)
                {
                    this.GroupSummaryRows.Clear();
                    this.GroupSummaryRows = null;
                }

                if (this.SelectedItems != null)
                {
                    this.SelectedItems.CollectionChanged -= OnSelectedItemsChanged;
                    this.SelectedItems.Clear();
                }

                if (this.Columns != null)
                {
                    (this.Columns as INotifyCollectionChanged).CollectionChanged -= OnGridColumnCollectionChanged;
                    this.Columns.Dispose();
                    this.Columns = null;
                }

                if (this.TableSummaryRows != null)
                {
                    foreach (var summaryRow in this.TableSummaryRows)
                    {
                        if (summaryRow is GridTableSummaryRow)
                            (summaryRow as GridTableSummaryRow).Dispose();
                    }
                    this.TableSummaryRows.Clear();
                    this.TableSummaryRows = null;
                }

                if (this.UnBoundRows != null)
                {
                    foreach (var unBoundDataRow in this.UnBoundRows)
                        unBoundDataRow.Dispose();
                    this.UnBoundRows.Clear();
                    this.UnBoundRows = null;
                }

                if (this.StackedHeaderRows != null)
                {
                    foreach (StackedHeaderRow stackedHeaderRow in this.StackedHeaderRows)
                        stackedHeaderRow.StackedColumns.Clear();
                    this.StackedHeaderRows.Clear();
                    //this.StackedHeaderRows = null;
                }

                if (this.CoveredCells != null)
                {
                    this.CoveredCells.Clear();
                    this.coveredCells = null;
                }

                if (this.SelectionController != null)
                {
                    this.SelectionController.Dispose();
                    this.SelectionController = null;
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

                if (this.DetailsViewManager != null)
                {
                    this.DetailsViewManager.Dispose();
                    this.DetailsViewManager = null;
                }

                if (this.GridCopyPaste is GridCutCopyPaste)
                {
                    (this.GridCopyPaste as GridCutCopyPaste).Dispose();
                }

                if (this.container != null)
                {
                    this.container.Dispose();
                    this.container = null;
                }

#if WPF
                if (this.SearchHelper != null)
                {
                    this.SearchHelper.Dispose();
                    this.SearchHelper = null;
                }
#endif
                if (this.MergedCellManager != null)
                {
                    this.MergedCellManager.Dispose();
                    this.MergedCellManager = null;
                }
                if (serializationController != null)
                {
                    serializationController.Dispose();
                    serializationController = null;
                }

                if (this.CurrentColumn != null)
                {
                    this.CurrentColumn = null;
                }

                if (this.filterRowCellRenderers != null)
                {
                    this.filterRowCellRenderers.Dispose();
                    this.filterRowCellRenderers = null;
                }

                if (this.DetailsViewDefinition != null)
                {
                    this.DetailsViewDefinition.Clear();
                    this.DetailsViewDefinition = null;
                }
#if WPF
                if (this.useDrawing)                
                    Provider = null;
#endif
            }
            isdisposed = true;
        }

        private bool needToDisposeView = true;

        /// <summary>
        /// While reusing DetailsViewDataRow, need to dispose the nested grid present in the DetailsViewDataGrid. But view should be reused.
        /// </summary>
        internal void DisposeAllExceptView()
        {
            needToDisposeView = false;
            Dispose();
            needToDisposeView = true;
        }

#endregion

    }

#if WPF 
    public static class AutomationPeerHelper
    {
        internal static bool? IsScreenReaderRunning = null;
        public static bool EnableCodedUI = false;
    }
#endif

    /// <summary>
    /// Represents the collection of UnboundRow which used to display additional rows which are not bound to data source in SfDataGrid
    /// </summary>
    public class UnBoundRows : ObservableCollection<GridUnBoundRow>
    {
        private int _frozenUnboundRowCount = 0;
        public int FrozenUnboundRowCount
        {
            get { return _frozenUnboundRowCount; }
            set { _frozenUnboundRowCount = value; }
        }

        private int _topBodyUnboundRowCount = 0;
        public int TopBodyUnboundRowCount
        {
            get { return _topBodyUnboundRowCount; }
            set { _topBodyUnboundRowCount = value; }
        }

        private int _bottomBodyUnboundRowCount = 0;
        public int BottomBodyUnboundRowCount
        {
            get { return _bottomBodyUnboundRowCount; }
            set { _bottomBodyUnboundRowCount = value; }
        }

        private int _footerUnboundRowCount = 0;
        public int FooterUnboundRowCount
        {
            get { return _footerUnboundRowCount; }
            set { _footerUnboundRowCount = value; }
        }



        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                base.OnCollectionChanged(e);
                this.FrozenUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Top && !unr.ShowBelowSummary).Count();
                this.TopBodyUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Top && unr.ShowBelowSummary).Count();
                this.BottomBodyUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Bottom && !unr.ShowBelowSummary).Count();
                this.FooterUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Bottom && unr.ShowBelowSummary).Count();
                return;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                var unboundrow = e.NewItems[0] as GridUnBoundRow;
                if (unboundrow.Position == UnBoundRowsPosition.Top)
                {
                    FrozenUnboundRowCount += !unboundrow.ShowBelowSummary ? 1 : 0;
                    TopBodyUnboundRowCount += unboundrow.ShowBelowSummary ? 1 : 0;
                }
                else
                {
                    BottomBodyUnboundRowCount += !unboundrow.ShowBelowSummary ? 1 : 0;
                    FooterUnboundRowCount += unboundrow.ShowBelowSummary ? 1 : 0;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                var unboundrow = e.OldItems[0] as GridUnBoundRow;
                if (unboundrow.Position == UnBoundRowsPosition.Top)
                {
                    FrozenUnboundRowCount -= !unboundrow.ShowBelowSummary ? 1 : 0;
                    TopBodyUnboundRowCount -= unboundrow.ShowBelowSummary ? 1 : 0;
                }
                else
                {
                    BottomBodyUnboundRowCount -= !unboundrow.ShowBelowSummary ? 1 : 0;
                    FooterUnboundRowCount -= unboundrow.ShowBelowSummary ? 1 : 0;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Replace)
                throw new NotImplementedException("Replace not supported in Unbound rows");
            base.OnCollectionChanged(e);
        }

        internal void ReCalculateUnboundRowsCount()
        {
            this.FrozenUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Top && !unr.ShowBelowSummary).Count();
            this.TopBodyUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Top && unr.ShowBelowSummary).Count();
            this.BottomBodyUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Bottom && !unr.ShowBelowSummary).Count();
            this.FooterUnboundRowCount = this.Select(unr => unr.Position == UnBoundRowsPosition.Bottom && unr.ShowBelowSummary).Count();
        }


    }

    /// <summary>
    ///Provides classes, interfaces and enumerators to create SfDataGrid, that enable a user to interact with a SfDataGrid.
    ///The grid classes are allow a user to manipulate the data and performs the SfDataGrid operations like filtering, 
    ///sorting, grouping and editing in SfDataGrid.
    /// </summary>
    class NamespaceDoc
    {
    }
}

