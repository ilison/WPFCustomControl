#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncfusion.Data;
using System.Threading.Tasks;
using Syncfusion.UI.Xaml.ScrollAxis;
using Syncfusion.UI.Xaml.Grid.Helpers;
#if !WPF
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Automation.Peers;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    [TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
    public class GroupDropAreaItem : ContentControl, IDisposable
    {
        #region Fields
        
        GroupDropArea groupDropArea;
        bool isCloseButtonClicked = false;
        private bool isdisposed = false;
        
        internal GroupDropAreaItemTapped GroupDropAreaItemTapped;
        internal GroupDropAreaItemRemoved GroupDropAreaItemRemoved;

#if WPF
        bool isMouseButtonPressed = false;
#else
        private Point pointerDown;
#endif
        
        #endregion

        #region Constructor

        public GroupDropAreaItem() 
        {
            this.DefaultStyleKey = typeof(GroupDropAreaItem);
#if !WPF
            this.ManipulationMode = ManipulationModes.TranslateX | ManipulationModes.TranslateY;
#endif
        }

        #endregion

        #region Property

        public GroupDropArea GroupDropArea
        {
            get
            {
                return groupDropArea;
            }
            internal set
            {
                groupDropArea = value;
            }
        }

        #endregion

        #region Dependency Property

        public GridColumn GridColumn
        {
            get { return (GridColumn)GetValue(GridColumnProperty); }
            set { SetValue(GridColumnProperty, value); }
        }

        public static readonly DependencyProperty GridColumnProperty =
            DependencyProperty.Register("GridColumn", typeof(GridColumn), typeof(GroupDropAreaItem), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets Path direction (Ascending/Descending).
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public object SortDirection
        {
            get { return (ListSortDirection)this.GetValue(SortDirectionProperty); }
            set { this.SetValue(SortDirectionProperty, value); }
        }

        public static readonly DependencyProperty SortDirectionProperty =
            DependencyProperty.Register("SortDirection", typeof(object), typeof(GroupDropAreaItem), new PropertyMetadata(null));

        public string GroupName
        {
            get { return (string)GetValue(GroupNameProperty); }
            set { SetValue(GroupNameProperty, value); }
        }

        public static readonly DependencyProperty GroupNameProperty =
            DependencyProperty.Register("GroupName", typeof(string), typeof(GroupDropAreaItem), new PropertyMetadata(null));

        #endregion

        #region override methods

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
            if (this.GroupDropArea == null || this.GroupDropArea.dataGrid == null || this.GroupDropArea.dataGrid.GroupDropItemContextMenu == null)
                return false;

            var menuinfo = new GridColumnContextMenuInfo() { Column = this.GridColumn, DataGrid = this.GroupDropArea.dataGrid };
            var args = new GridContextMenuEventArgs(this.GroupDropArea.dataGrid.GroupDropItemContextMenu, menuinfo,
                       RowColumnIndex.Empty, ContextMenuType.GroupDropAreaItem, this.GroupDropArea.dataGrid);
            if (!GroupDropArea.dataGrid.RaiseGridContextMenuEvent(args))
            {
#if WPF
                if (args.ContextMenuInfo != null)
                    this.GroupDropArea.dataGrid.GroupDropItemContextMenu.DataContext = args.ContextMenuInfo;
                this.GroupDropArea.dataGrid.GroupDropAreaContextMenu.PlacementTarget = this;
                this.GroupDropArea.dataGrid.GroupDropItemContextMenu.IsOpen = true;
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
        protected override void OnTapped(TappedRoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (!isCloseButtonClicked)
            {
                if (this.GroupDropAreaItemTapped != null)
                    this.GroupDropAreaItemTapped(this.GridColumn, 1);
            }
            base.OnTapped(e);
        }
        /// <summary>
        /// When Right click the SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Group drop area item cells.
        /// </summary>
        /// <param name="e">Right tapped event arguments</param>
        protected override void OnRightTapped(RightTappedRoutedEventArgs e)
        {
            var position = e.GetPosition(this);
            if (ShowContextMenu(position))
                e.Handled = true;
            base.OnRightTapped(e);
        }

        protected override void OnDoubleTapped(DoubleTappedRoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (!isCloseButtonClicked)
            {
                if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(false))
                    return;
                if (this.GroupDropAreaItemTapped != null)
                    this.GroupDropAreaItemTapped(this.GridColumn, 2);
            }
            base.OnDoubleTapped(e);
        }

        protected override void OnPointerPressed(PointerRoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            pointerDown = e.GetCurrentPoint(this).Position;
            pointer = e.Pointer;
            base.OnPointerPressed(e);
        }

        Pointer pointer;
        protected override void OnManipulationStarted(ManipulationStartedRoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            var rect = this.GetControlRect(this.GroupDropArea.dataGrid);

            VisualStateManager.GoToState(this, "Normal", true);

            this.GroupDropArea.dataGrid.GridColumnDragDropController.ShowPopup(this.GridColumn, rect, false, pointerDown, true, false, pointer);
            e.Handled = true;
            base.OnManipulationStarted(e);
        }

        protected override void OnHolding(HoldingRoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (e.HoldingState == HoldingState.Started && e.PointerDeviceType != PointerDeviceType.Mouse && this.GroupDropArea.dataGrid.GroupDropItemContextMenu==null)
            {
                var rect = this.GetControlRect(this.GroupDropArea.dataGrid);
                this.GroupDropArea.dataGrid.GridColumnDragDropController.ShowPopup(this.GridColumn, rect, true, pointerDown, true, false, pointer);
                e.Handled = true;
            }
            // When long press on SfDataGrid Cell, Context menu appears for the selected cell. We are using this event for context menu support in Group Drop Area Item.
            if (e.HoldingState == HoldingState.Completed && e.PointerDeviceType != PointerDeviceType.Mouse)
            {
                var position = e.GetPosition(this);
                if (ShowContextMenu(position))
                    e.Handled = true;
            }
            base.OnHolding(e);
        }

        protected override void OnPointerEntered(PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                VisualStateManager.GoToState(this, "PointerOver", true);
            base.OnPointerEntered(e);
        }

        protected override void OnPointerExited(PointerRoutedEventArgs e)
        {
            if (e.Pointer.PointerDeviceType == PointerDeviceType.Mouse)
                VisualStateManager.GoToState(this, "Normal", true);
            base.OnPointerExited(e);
        }
#else 
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (!isCloseButtonClicked && isMouseButtonPressed)
            {
                Point mouseDown = e.GetPosition(this);
                if (this.groupDropArea == null || this.GroupDropArea.dataGrid.GridColumnDragDropController != null)
                {
                    var rect = this.GetControlRect(this.GroupDropArea.dataGrid);
                    this.GroupDropArea.dataGrid.GridColumnDragDropController.ShowPopup(this.GridColumn, rect, false, mouseDown, true, false, null);
                }
                isMouseButtonPressed = false;
                e.Handled = true;
            }
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (this.groupDropArea == null || !this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;

            isMouseButtonPressed = false;
            if (!isCloseButtonClicked)
            {
                if (this.GroupDropAreaItemTapped != null)
                {
                    this.GroupDropAreaItemTapped(this.GridColumn, e.ClickCount);
                }
            }
            base.OnMouseLeftButtonUp(e);
        }

        protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
        {
            if (this.groupDropArea == null || !this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(false))
                return;
            if (this.GroupDropAreaItemTapped != null)
            {
                this.GroupDropAreaItemTapped(this.GridColumn, 2);
            }
            base.OnMouseDoubleClick(e);
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            if (this.groupDropArea == null || !this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            isMouseButtonPressed = true;
            base.OnMouseLeftButtonDown(e);
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "MouseOver", true);
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            VisualStateManager.GoToState(this, "Normal", true);
            base.OnMouseLeave(e);
        }
#endif

        private Button PART_CloseButton;
#if !WPF
        protected override void OnApplyTemplate() 
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            if (PART_CloseButton != null)
            {
                PART_CloseButton.Click -= OnCloseClick;
            }
            PART_CloseButton = this.GetTemplateChild("PART_CloseButton") as Button;
            if (PART_CloseButton != null)
            {
                PART_CloseButton.Click += OnCloseClick;
            }
#if WPF
            if (this.GroupDropArea.dataGrid != null)
            {
                this.ContextMenuOpening += OnContextMenuOpening;
                var bind = new Binding
                {
                    Path = new PropertyPath("GroupDropItemContextMenu"),
                    Source = this.GroupDropArea.dataGrid,
                    Mode = BindingMode.TwoWay,
                };
                this.SetBinding(GroupDropArea.ContextMenuProperty, bind);
            }
#endif
            
        }


        #endregion

        #region private Methods

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            if (!this.GroupDropArea.dataGrid.ValidationHelper.CheckForValidation(true))
                return;
            if (this.GroupDropAreaItemRemoved != null)
                this.GroupDropAreaItemRemoved(this.GridColumn);
            isCloseButtonClicked = true;
        }

        #endregion

#if WPF
        #region ContextMenu Event
        /// <summary>
        /// Occurs when any context menu on the element is opened.
        /// </summary>
        /// <param name="sender">The sender which contains SfDataGrid</param>
        /// <param name="e">Context menu event arguments</param>
        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ShowContextMenu())
                e.Handled = true;
        }

        #endregion
#endif
        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropAreaItem"/> class.
        /// </summary>        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GroupDropAreaItem"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            if (isDisposing)
            {
                if (PART_CloseButton != null)
                {
                    PART_CloseButton.Click -= OnCloseClick;
                    this.PART_CloseButton = null;
                }
#if WPF
                if (this.GroupDropArea != null && this.GroupDropArea.dataGrid != null && this.GroupDropArea.dataGrid.GroupDropItemContextMenu != null)
                {
                    this.ContextMenuOpening -= OnContextMenuOpening;
                    this.ContextMenu = null;
                }
#endif
                this.GridColumn = null;
                this.groupDropArea = null;
                this.GroupDropAreaItemRemoved = null;
                this.GroupDropAreaItemTapped = null;
#if !WPF
            this.pointer = null;
#endif
            }
            isdisposed = true;
        }

#if WPF
        # region AutomationOverrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GroupDropAreaItemAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }

    internal delegate void GroupDropAreaItemTapped(GridColumn GridColumn, int ClickCount);

    internal delegate void GroupDropAreaItemRemoved(GridColumn GridColumn);
}
