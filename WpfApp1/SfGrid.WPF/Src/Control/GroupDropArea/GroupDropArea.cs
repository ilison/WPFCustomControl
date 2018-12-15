#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.ScrollAxis;
using System;
using System.Linq;
using Syncfusion.Data.Extensions;
using System.ComponentModel.DataAnnotations;
using System.Dynamic;
#if !WPF
using System.Reflection;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Syncfusion.Data;
using Windows.UI.Xaml.Data;
using Windows.Devices.Input;
using Windows.UI.Input;
using Windows.ApplicationModel.Resources;
#else
using System.Windows;
using System.Collections.ObjectModel;
using Syncfusion.Data;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using System.Windows.Data;
using System.Threading;
using Pointer = System.Object;
using System.Resources;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Specialized;
using System.Windows.Automation.Peers;
#endif


namespace Syncfusion.UI.Xaml.Grid
{ 
    
    [TemplatePart(Name = "PART_StackPanel", Type = typeof(StackPanel))]
#if WPF
    [TemplatePart(Name = "PART_GroupDropAreaGrid", Type = typeof(System.Windows.Controls.Grid))]
#else
    [TemplatePart(Name = "PART_GroupDropAreaGrid", Type = typeof(Windows.UI.Xaml.Controls.Grid))]
#endif
    public class GroupDropArea : Control, IDisposable
    {
        #region Fields

        internal SfDataGrid dataGrid;
        private bool isGroupDropAreaExpandedSetBeforeGridLoaded;
        internal StackPanel Panel;
        private bool isdisposed = false;
#if WPF
        internal System.Windows.Controls.Grid groupItemsGrid;
#else
        internal Windows.UI.Xaml.Controls.Grid groupItemsGrid;
#endif
        
        #endregion

        #region Constructor
        static GroupDropArea()

        {
#if WPF
            DefaultStyleKeyProperty.OverrideMetadata(typeof(GroupDropArea), new FrameworkPropertyMetadata(typeof(GroupDropArea)));
#endif
        }

        public GroupDropArea()
        {
#if !WPF
            this.DefaultStyleKey = typeof(GroupDropArea);
#endif      
        }

        #endregion

        #region DependencyProperty

        public bool IsExpanded
        {
            get { return (bool) GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public static readonly DependencyProperty IsExpandedProperty =
            DependencyProperty.Register("IsExpanded", typeof (bool), typeof (GroupDropArea),
                                        new PropertyMetadata(false, OnIsExpandedPropertyChanged));

        private static void OnIsExpandedPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs agrs)
        {
            var groupDropArea = obj as GroupDropArea;
            if (groupDropArea != null)
                (obj as GroupDropArea).ToggleExpanded((bool) agrs.NewValue);
        }

        /// <summary>
        /// Gets or sets the group drop area text.
        /// </summary>
        /// <value>The group drop area text.</value>
        public string GroupDropAreaText
        {
            get { return (string) GetValue(GroupDropAreaTextProperty); }
            set { SetValue(GroupDropAreaTextProperty, value); }
        }

        public static readonly DependencyProperty GroupDropAreaTextProperty =
            DependencyProperty.Register("GroupDropAreaText", typeof (string), typeof (GroupDropArea), new PropertyMetadata(GridResourceWrapper.GroupDropAreaText));

        public Visibility WatermarkTextVisibility
        {
            get { return (Visibility) GetValue(WatermarkTextVisibilityProperty); }
            set { SetValue(WatermarkTextVisibilityProperty, value); }
        }

        public static readonly DependencyProperty WatermarkTextVisibilityProperty =
            DependencyProperty.Register("WatermarkTextVisibility", typeof (Visibility), typeof (GroupDropArea),
                                        new PropertyMetadata(Visibility.Visible));

        #endregion

        #region Override Methods

        /// <summary>
        /// Opens the context menu at the specified position.
        /// </summary>
        /// <param name="position">The position to display context menu.</param>
        /// <returns>
        /// <b>true</b> If the context menu opened;Otherwise<b>false</b>
        /// </returns>
#if UWP
        protected virtual internal bool ShowContextMenu(Point position)
#else
        protected virtual internal bool ShowContextMenu()
#endif
        {
            if (this.dataGrid == null || dataGrid.GroupDropAreaContextMenu == null)
                return false;

            var menuinfo = new GridGroupDropAreaContextMenuInfo() { DataGrid = this.dataGrid };
            var args = new GridContextMenuEventArgs(dataGrid.GroupDropAreaContextMenu, menuinfo, RowColumnIndex.Empty, ContextMenuType.GroupDropArea, this.dataGrid);
            if (!dataGrid.RaiseGridContextMenuEvent(args))
            {
#if WPF
                if (args.ContextMenuInfo != null)
                    dataGrid.GroupDropAreaContextMenu.DataContext = args.ContextMenuInfo;
                dataGrid.GroupDropAreaContextMenu.PlacementTarget = this;
                dataGrid.GroupDropAreaContextMenu.IsOpen = true;
#else
                if (args.ContextMenuInfo != null)
                   foreach (var item in args.ContextMenu.Items)
                      item.DataContext = args.ContextMenuInfo;
                args.ContextMenu.ShowAt(this, position);
#endif
                return true;
            }
            return false;
        }

#if !WPF
        /// <summary>
        /// When long press on SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Group Drop Area.
        /// </summary>
        /// <param name="e">Holding event arguments</param>
        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (e.HoldingState == HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                if (ShowContextMenu(position))
                    e.Handled = true;
            }
            base.OnHolding(e);
        }

        /// <summary>
        /// When Right click SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Group drop area cell.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }


        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            Panel = this.GetTemplateChild("PART_StackPanel") as StackPanel;
#if WPF
            groupItemsGrid = this.GetTemplateChild("PART_GroupDropAreaGrid") as System.Windows.Controls.Grid;
#else
            groupItemsGrid = this.GetTemplateChild("PART_GroupDropAreaGrid") as Windows.UI.Xaml.Controls.Grid;
#endif

            if (Panel != null)
            {
                //When we set ShowGroupDropArea as true after grouping the column, GroupDropAreaItem is not added in GroupDropArea.
                //Hence, we initialize the panel children if GroupColumnDescriptions Count is greater than zero.
                this.InitializeGroupDropAreaPanel();
            }

            if (isGroupDropAreaExpandedSetBeforeGridLoaded)
            {
                UpdateVisualState(true);
                isGroupDropAreaExpandedSetBeforeGridLoaded = false;
            }
            else
            {
                //GroupDropArea is collapsed when changing the theme in VisualStyle Sample
                if (this.IsExpanded)
                    this.ToggleExpanded(true);
            }
#if WPF
            this.ContextMenuOpening += OnContextMenuOpening;

            var bind = new Binding
            {
                Path = new PropertyPath("GroupDropAreaContextMenu"),
                Source = dataGrid,
                Mode = BindingMode.TwoWay,
            };
            this.SetBinding(GroupDropArea.ContextMenuProperty, bind);

#endif
        }

#if WPF
        #region ContenxtMenu Event
        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfDataGrid</param>
        /// <param name="e">Context menu event arguments</param>
        internal void OnContextMenuOpening(Pointer sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }
        #endregion
#endif


        #endregion

        #region Private Methods

        private void ToggleExpanded(bool isExpanded)
        {
            this.dataGrid.IsGroupDropAreaExpanded = isExpanded;
            if (this.Panel != null)
            {
                UpdateVisualState(true);
                this.dataGrid.InvalidateMeasure();
#if !WPF
                this.dataGrid.VisualContainer.InvalidateMeasure();
               
#endif
            }
            else
                isGroupDropAreaExpandedSetBeforeGridLoaded = true;
        }

        private void UpdateVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsExpanded ? "Expanded" : "Collapsed", useTransitions);
        }

        private void InitializeGroupDropAreaPanel()
        {
            //Here we check the IRowGenerator.Items count to avoid the null reference exception while iterate the HeaderRow.
            if (this.dataGrid != null && this.dataGrid.View != null && this.dataGrid.View.GroupDescriptions.Count > 0 && this.dataGrid.RowGenerator.Items.Count > 0)
            {
                foreach (var desc in this.dataGrid.View.GroupDescriptions)
                {
                    var headerRow =
                        this.dataGrid.RowGenerator.Items.FirstOrDefault(item => item.RowType == RowType.HeaderRow && item.RowIndex == this.dataGrid.GetHeaderIndex());
                    if (headerRow != null)
                    {
                        var headerColumn =
                            headerRow.VisibleColumns.FirstOrDefault(
                                col => col.GridColumn != null && col.GridColumn.MappingName == (desc as ColumnGroupDescription).PropertyName);
                        var headerCellControl = headerColumn.ColumnElement as GridHeaderCellControl;
                        this.AddGroupAreaItem(headerColumn.GridColumn, headerCellControl.SortDirection, false);
                    }
                }
            }
        }

        #endregion

        

        #region Draggable popup for Group

        #region private methods



        private void OnGroupDropAreaItemTapped(GridColumn column, int ClickCount)
        {
            if (dataGrid.SortClickAction == SortClickAction.DoubleClick)
            {
                if (ClickCount == 2)
                    this.dataGrid.GridModel.MakeSort(column);
            }
            else
                this.dataGrid.GridModel.MakeSort(column);
        }

        private void OnGroupDropAreaItemRemoved(GridColumn column)
        {
            this.RemoveGroupDropAreaItem(column);
        }

        private UIElement CreatePopupContent(GridColumn column)
        {
            var textblock = new TextBlock { Text = column.HeaderText };
            return textblock;
        }

        private GroupDropAreaItem CreateGroupDropAreaItem(GridColumn column)
        {
            if (column.HeaderText == null)
                column.HeaderText = column.MappingName;
            
            var groupItem = new GroupDropAreaItem
            {
                GroupDropAreaItemTapped = OnGroupDropAreaItemTapped,
                GroupDropAreaItemRemoved = OnGroupDropAreaItemRemoved,
                GridColumn = column,
                GroupDropArea = this,
                Margin = new Thickness(5, 0, 5, 0),
                MinWidth = 120
            };
            
            var binding = new Binding() { Path = new PropertyPath("HeaderText"), Source = groupItem.GridColumn };
            groupItem.SetBinding(GroupDropAreaItem.GroupNameProperty, binding);

            return groupItem;
        }

        internal void MoveGroupDropAreaItem(GridColumn column, ListSortDirection direction, int moveToIndex)
        {
            GroupDropAreaItem item = this.Panel.Children.ToList<GroupDropAreaItem>().FirstOrDefault(x => (x as GroupDropAreaItem).GridColumn == column) as GroupDropAreaItem;
            if (item != null)
            {
                var oldIndex = this.Panel.Children.IndexOf(item);
                this.Panel.Children.Remove(item);
                this.Panel.Children.Insert(moveToIndex, item);
                this.dataGrid.GroupColumnDescriptions.Move(oldIndex, moveToIndex);
            }
        }

        //Updates the selection when changing the theme in VisualStyle Sample hence needToGroup parameter is added.
        internal void AddGroupAreaItem(GridColumn column, object direction, bool needToGroup = true)
        {
            dataGrid.RunWork(new Action(() =>
                {
                    AddGroupDropAreaItem(column, direction, needToGroup);
                }));
        }

        internal void AddGroupAreaItem(GridColumn column, object direction, int insertAt)
        {
            dataGrid.RunWork(new Action(() =>
            {
                AddGroupDropAreaItem(column, direction, insertAt, true);
            }));
        }

        internal void AddGroupDropAreaItem(GridColumn column, object direction, bool needToGroup)
        {
            if (this.Panel != null && this.Panel.Children.ToList<GroupDropAreaItem>().All(item => (item as GroupDropAreaItem).GridColumn.MappingName != column.MappingName) && this.dataGrid.View != null)
            {
                if (column.MappingName == null)
                    throw new InvalidOperationException("MappingName is necessary for Sorting, Grouping and Filtering");

                //if the column is dummy, we need to restrict it and avoid to add that column in groupdroparea.
                if (!dataGrid.CheckColumnNameinItemProperties(column) && !column.IsUnbound)
                    return;

                var content = this.CreateGroupDropAreaItem(column);
                content.SortDirection = direction;
                this.Panel.Children.Add(content);
                if (needToGroup)
                    this.dataGrid.GroupBy(column.MappingName, null, null);
                this.WatermarkTextVisibility = Visibility.Collapsed;
            }
            else
#if !WPF
                this.dataGrid.GridColumnDragDropController.reverseAnimationStoryboard.Begin();
#else
                this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen = false;
#endif
        }
        
        internal void AddGroupDropAreaItem(GridColumn column, object direction, int insertAt,bool needToGroup)
        {
            if (this.Panel != null && this.Panel.Children.ToList<GroupDropAreaItem>().All(item => (item as GroupDropAreaItem).GridColumn.MappingName != column.MappingName) && this.dataGrid.View != null)
            {
                //WPF-20111-Argument out of bound exception while grouping the column
                //if the column is dummy, we need to restrict it and avoid to add that column in groupdroparea.
                if (!dataGrid.CheckColumnNameinItemProperties(column) && !column.IsUnbound)
                    return;

                var content = this.CreateGroupDropAreaItem(column);
                content.SortDirection = direction;
                this.Panel.Children.Insert(insertAt, content);
                if (needToGroup)
                    this.dataGrid.GroupBy(column.MappingName, insertAt, null, null);
                this.WatermarkTextVisibility = Visibility.Collapsed;
            }
            else if(this.dataGrid.GridColumnDragDropController != null)
            {
                
#if !WPF
                this.dataGrid.GridColumnDragDropController.reverseAnimationStoryboard.Begin();
#else
                this.dataGrid.GridColumnDragDropController.DraggablePopup.IsOpen = false;
#endif
            }
        }

        internal void RemoveGroupDropAreaItem(GridColumn column)
        {

            dataGrid.RunWork(new Action(() =>
            {
                if (this.Panel != null && this.Panel.Children.Count > 0)
                {
                    var element = this.Panel.Children.ToList<GroupDropAreaItem>().FirstOrDefault(item => (item as GroupDropAreaItem).GridColumn.MappingName == column.MappingName);
                    if (element != null)
                        this.Panel.Children.Remove(element);
                    var groupedcolumn = this.dataGrid.GroupColumnDescriptions.FirstOrDefault(groupcolumn => groupcolumn.ColumnName == column.MappingName);
                    if (groupedcolumn != null)
                        this.dataGrid.RemoveGroup(groupedcolumn.ColumnName);
                    if (Panel.Children.Count == 0)
                        this.WatermarkTextVisibility = Visibility.Visible;
                }
            }), dataGrid.ShowBusyIndicator);
        }

        internal void RemoveGroupDropItem(GridColumn column)
        {
            dataGrid.RunWork(new Action(() =>
            {
                if (this.Panel != null && this.Panel.Children.Count > 0)
                {
                    var element = this.Panel.Children.ToList<GroupDropAreaItem>().FirstOrDefault(item => (item as GroupDropAreaItem).GridColumn.MappingName == column.MappingName);
                    if (element != null)
                        this.Panel.Children.Remove(element);
                    if (Panel.Children.Count == 0)
                        this.WatermarkTextVisibility = Visibility.Visible;
                }
            }), dataGrid.ShowBusyIndicator);
        }             

        #endregion

        #region Internal Methods

        internal void RemoveAllGroupDropItems()
        {
            if (this.Panel != null && this.Panel.Children.Count > 0)
            {
                this.Panel.Children.Clear();
                this.WatermarkTextVisibility = Visibility.Visible;
            }
        }

        internal void UpdateGroupDropItemSortIcon(GridColumn column)
        {
            var sortedcolumn = this.dataGrid.View.SortDescriptions.FirstOrDefault(c => c.PropertyName == column.MappingName);

            if (this.Panel != null && this.Panel.Children.ToList<GroupDropAreaItem>().Any(item => (item as GroupDropAreaItem).GridColumn == column))
            {
                var groupDropAreaItem = this.Panel.Children.ToList<GroupDropAreaItem>().FirstOrDefault(item => (item as GroupDropAreaItem).GridColumn == column) as GroupDropAreaItem;
                if (groupDropAreaItem != null)
                {
                    if (sortedcolumn != default(SortDescription))
                        groupDropAreaItem.SortDirection = sortedcolumn.Direction;
                    else
                        groupDropAreaItem.SortDirection = null;
                }
            }
        }

        #endregion

        #endregion

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropArea"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
#if WPF
                if (this.dataGrid.GroupDropAreaContextMenu != null)
                {
                    this.ContextMenuOpening -= OnContextMenuOpening;
                    this.ContextMenu = null;
                }
#endif
                if (this.Panel != null)
                {
                    foreach (var item in this.Panel.Children)
                        (item as GroupDropAreaItem).Dispose();
                    if (this.Panel.Children != null)
                        this.Panel.Children.Clear();
                    this.Panel = null;
                }
                this.dataGrid = null;
            }
            isdisposed = true;
        }

# if WPF
        # region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GroupDropAreaAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }
}



