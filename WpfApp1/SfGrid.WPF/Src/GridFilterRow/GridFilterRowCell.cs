#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Diagnostics;
#if WPF
using System.Windows.Automation.Peers;
using System.Windows.Data;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
#endif

namespace Syncfusion.UI.Xaml.Grid.RowFilter
{
#if WinRT || UNIVERSAL
    using Key = Windows.System.VirtualKey;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
    using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
    using Windows.UI.Xaml.Data;
    using ScrollAxis;
#endif

    /// <summary>
    /// Represents a class that loads FilteRowCell for FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    [TemplatePart(Name = "PART_PopupPresenter", Type = typeof(Border))]
    [TemplatePart(Name = "PART_FilterOptionButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_FilterOptionsList", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_FilterOptionPopup", Type = typeof(Popup))]
    public class GridFilterRowCell : GridCell, INotifyPropertyChanged, IGridFilterRowCell
    {

        /// <summary>
        /// Gets or sets the ToggleButton which opens the FilterOption popup.
        /// </summary>
        protected ToggleButton FilterOptionButton;

        /// <summary>
        /// Gets or sets the ListBox control to load the FilterType list.
        /// </summary>
        protected ListBox FilterOptionsList;

        /// <summary>
        /// Gets or sets the Popup conrol which contains the FilterOptionsList.
        /// </summary>
        protected internal Popup FilterOptionPopup;

        public event PropertyChangedEventHandler PropertyChanged;

        #region Ctr

        public GridFilterRowCell()
        {
            base.DefaultStyleKey = typeof(GridFilterRowCell);
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Returns whethter the FilterOption drop down is in open or not.
        /// </summary>
        public bool IsDropDownOpen
        {
            get
            {
                if (this.FilterOptionPopup != null)
                    return this.FilterOptionPopup.IsOpen;
                return false;
            }
        }

        /// <summary>
        /// Returns the DataColumn of the corresponding GridFilterRowCell.
        /// </summary>
        public DataColumnBase DataColumn
        {
            get { return this.ColumnBase; }
        }

        /// <summary>
        /// Gets or sets the Visibility of the FilterOptionButton.
        /// </summary>
        public Visibility FilterOptionButtonVisibility
        {
            get { return (Visibility)GetValue(FilterOptionButtonVisibilityProperty); }
            set { SetValue(FilterOptionButtonVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.RowFilter.GridFilterRowCell.FilterOptionButtonVisibility dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.RowFilter.GridFilterRowCell.FilterOptionButtonVisibility dependency property.
        /// </remarks>  
        public static readonly DependencyProperty FilterOptionButtonVisibilityProperty =
            DependencyProperty.Register("FilterOptionButtonVisibility", typeof(Visibility), typeof(GridFilterRowCell), new PropertyMetadata(Visibility.Visible));

#if !WPF
        #region HorizontalOffsetProperty

        /// <summary>
        /// Dependency property Registration for HorizontalOffset
        /// </summary>
        public static readonly DependencyProperty HorizontalOffsetProperty = DependencyProperty.Register(
              "HorizontalOffset", typeof(double), typeof(GridFilterControl), new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets HorizontalOffset for the Popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double HorizontalOffset
        {
            get { return (double)this.GetValue(GridFilterControl.HorizontalOffsetProperty); }
            set { this.SetValue(GridFilterControl.HorizontalOffsetProperty, value); }
        }

        #endregion

        #region VerticalOffsetProperty

        /// <summary>
        /// Dependency property Registration for VerticalOffset
        /// </summary>
        public static readonly DependencyProperty VerticalOffsetProperty = DependencyProperty.Register(
              "VerticalOffset", typeof(double), typeof(GridFilterControl), new PropertyMetadata(0d));

        /// <summary>
        /// Gets or sets the VerticalOffset for Popup.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public double VerticalOffset
        {
            get { return (double)this.GetValue(GridFilterControl.VerticalOffsetProperty); }
            set { this.SetValue(GridFilterControl.VerticalOffsetProperty, value); }
        }


        #endregion
#endif

        #endregion

        #region Protected Methods

        protected void UpdateBinding()
        {
            Binding binding = new Binding();
            binding.Source = FilterOptionButton;
            binding.Path = new PropertyPath("IsChecked");
            binding.Mode = BindingMode.TwoWay;
            FilterOptionPopup.SetBinding(Popup.IsOpenProperty, binding);
        }


        /// <summary>
        /// Populates the FilterOption list which will loaded in FilterOptionPopup.
        /// </summary>
        protected ObservableCollection<string> GetFilterOptionsList()
        {
            var list = new ObservableCollection<string>();
            list.Add(GridResourceWrapper.Equalss);
            list.Add(GridResourceWrapper.NotEquals);
            var column = this.ColumnBase.GridColumn;
            if (column.FilterRowEditorType == "Numeric")
            {
                list.Add(GridResourceWrapper.LessThan);
                list.Add(GridResourceWrapper.LessThanorEqual);
                list.Add(GridResourceWrapper.GreaterThan);
                list.Add(GridResourceWrapper.GreaterThanorEqual);
            }
            else if (column.FilterRowEditorType == "DateTime")
            {
                list.Add(GridResourceWrapper.Before);
                list.Add(GridResourceWrapper.BeforeOrEqual);
                list.Add(GridResourceWrapper.After);
                list.Add(GridResourceWrapper.AfterOrEqual);
            }
            else
            {
                list.Add(GridResourceWrapper.BeginsWith);
                list.Add(GridResourceWrapper.EndsWith);
                list.Add(GridResourceWrapper.Contains);
                list.Add(GridResourceWrapper.Empty);
                list.Add(GridResourceWrapper.NotEmpty);
            }
            if (column.AllowBlankFilters)
            {
                list.Add(GridResourceWrapper.Null);
                list.Add(GridResourceWrapper.NotNull);
            }
            return list;
        }

        /// <summary>
        /// Opens the FilterOptionPopup with the FilterOptionList.
        /// </summary>
        public virtual void OpenFilterOptionPopup()
        {
            if (this.FilterOptionButtonVisibility == Visibility.Collapsed)
                return;
            if (!this.ColumnBase.IsEditing)
                this.ColumnBase.Renderer.DataGrid.SelectionController.CurrentCellManager.BeginEdit();
            var list = this.GetFilterOptionsList();
            if (list.Count > 0)
            {
                this.FilterOptionsList.ItemsSource = list;
                this.FilterOptionsList.SelectedItem = FilterRowHelpers.GetResourceWrapper(this.ColumnBase.GridColumn.FilterRowCondition);
#if !WPF
                this.FilterOptionPopup.VerticalOffset = this.ActualHeight;
#endif
                FilterOptionPopup.IsOpen = true;
#if WPF
                this.FilterOptionsList.ItemContainerGenerator.StatusChanged += OnItemContainerGeneratorStatusChanged;
#endif
            }
        }

        public virtual void CloseFilterOptionPopup()
        {
            if (this.FilterOptionPopup != null)
                this.FilterOptionPopup.IsOpen = false;
            this.ColumnBase.Renderer.SetFocus(true);
        }

#if !WPF
        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if ((SelectionHelper.CheckAltKeyPressed() && e.Key == Key.Down) || (SelectionHelper.CheckAltKeyPressed() && e.Key == Key.Up))
            {
                if (!this.ColumnBase.IsEditing)
                    this.ColumnBase.Renderer.DataGrid.SelectionController.CurrentCellManager.BeginEdit();
                this.OpenFilterOptionPopup();
                e.Handled = true;
            }
            base.OnKeyDown(e);
        }
#endif

        #endregion

        #region Private Methods

        private void OnColumnPropertyChanged(GridColumn column, string propertyName)
        {
            //WPF-37891 We need to Reinitialize the edit element while FilterRowText changed.
            if (propertyName.Equals("FilterRowText"))
                (this.ColumnBase.Renderer as IGridFilterRowRenderer).ClearFilter(this.ColumnBase);

            if (propertyName.Equals("FilterRowOptionsVisibility"))
                UpdateFilterOptionButtonVisibility();
        }

        internal void UpdateFilterOptionButtonVisibility()
        {
            this.FilterOptionButtonVisibility = this.ColumnBase.GridColumn.GetFilterRowOptionsVisibility();
        }
#if WPF
        private void OnFilterOptionButtonPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.ColumnBase.Renderer.DataGrid.AllowSelectionOnPointerPressed)
                return;

            if (!this.IsDropDownOpen)
            {
                this.OpenFilterOptionPopup();
                e.Handled = true;
            }
            else
                CloseFilterOptionPopup();
        }

        private void OnFilterOptionButtonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {            
            if (!this.ColumnBase.Renderer.DataGrid.AllowSelectionOnPointerPressed)
                return;

            if (!this.IsDropDownOpen)
                this.OpenFilterOptionPopup();
            else
                this.CloseFilterOptionPopup();
            e.Handled = true;
        }

        private void OnFilterOptionsListMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            (this.ColumnBase.Renderer as IGridFilterRowRenderer).OnFilterRowConditionChanged(this.FilterOptionsList.SelectedItem.ToString());
            this.CloseFilterOptionPopup();
        }

#else

        private void OnFilterOptionsListLoaded(object sender, RoutedEventArgs e)
        {
            this.FilterOptionsList.Focus(FocusState.Programmatic);
        }

        private void OnFilterOptionsListTapped(object sender, TappedRoutedEventArgs e)
        {
            (this.ColumnBase.Renderer as IGridFilterRowRenderer).OnFilterRowConditionChanged(this.FilterOptionsList.SelectedItem.ToString());
            this.CloseFilterOptionPopup();
        }

        private void OnFilterOptionButtonClick(object sender, RoutedEventArgs e)
        {
            var rowColumnIndex = new RowColumnIndex(this.ColumnBase.RowIndex, this.ColumnBase.ColumnIndex);
            this.ColumnBase.Renderer.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Pressed, null), rowColumnIndex);
            this.ColumnBase.Renderer.DataGrid.SelectionController.HandlePointerOperations(new GridPointerEventArgs(PointerOperation.Released, null), rowColumnIndex);

            if ((bool)this.FilterOptionButton.IsChecked)
            {
                if (!this.ColumnBase.IsEditing)
                    this.ColumnBase.Renderer.DataGrid.SelectionController.CurrentCellManager.BeginEdit();
                this.OpenFilterOptionPopup();
            }
            else
                CloseFilterOptionPopup();
        }
#endif
        private void OnFilterOptionsListKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Down:
                    e.Handled = this.FilterOptionsList.SelectedIndex == (this.FilterOptionsList.ItemsSource as ObservableCollection<string>).Count - 1;
                    return;
                case Key.Up:
                    e.Handled = this.FilterOptionsList.SelectedIndex == 0;
                    return;
                case Key.Enter:
                    this.CloseFilterOptionPopup();
                    (this.ColumnBase.Renderer as IGridFilterRowRenderer).OnFilterRowConditionChanged(this.FilterOptionsList.SelectedItem.ToString());
                    e.Handled = true;
                    return;
            }
        }

#if WPF

        void OnItemContainerGeneratorStatusChanged(object sender, EventArgs e)
        {
            if (this.FilterOptionsList.ItemContainerGenerator.Status == GeneratorStatus.ContainersGenerated)
            {
                this.FilterOptionsList.ItemContainerGenerator.StatusChanged -= OnItemContainerGeneratorStatusChanged;
                Dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Input, new Action(() =>
                {
                    var listBoxItem = (ListBoxItem)this.FilterOptionsList.ItemContainerGenerator.ContainerFromItem(this.FilterOptionsList.SelectedItem);
                    if (listBoxItem != null)
                        listBoxItem.Focus();
                }));
            }
        }

        void OnFilterOptionButtonMouseLeave(object sender, MouseEventArgs e)
        {
            this.FilterOptionPopup.StaysOpen = false;
        }

        void OnFilterOptionButtonMouseEnter(object sender, MouseEventArgs e)
        {
            this.FilterOptionPopup.StaysOpen = true;
        }
#else
        private void OnFilterOptionPopupOpened(object sender, object e)
        {

        }
#endif
        #endregion

        #region Public Methods

        /// <summary>
        /// Notify property changed event.
        /// </summary>
        /// <param name="propertyName"></param>
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region Wire or UnWire

        /// <summary>
        /// Wires the required events.
        /// </summary>
        protected void WireEvents()
        {
            if (this.FilterOptionButton != null)
            {
#if WPF
                //WPF-28914 We need open the filter popup on mouse down when AllowSelectionOnPointerPressed property is enabled. Hence the below
                //event is used.
                this.FilterOptionButton.PreviewMouseLeftButtonUp += OnFilterOptionButtonPreviewMouseLeftButtonUp;
                this.FilterOptionButton.PreviewMouseLeftButtonDown += OnFilterOptionButtonPreviewMouseLeftButtonDown;
#else
                this.FilterOptionButton.Click += OnFilterOptionButtonClick;
#endif
            }

            if (FilterOptionsList != null)
            {
#if WPF
                FilterOptionsList.MouseLeftButtonUp += OnFilterOptionsListMouseLeftButtonUp;
                FilterOptionsList.KeyDown += OnFilterOptionsListKeyDown;
#else
                FilterOptionsList.Tapped += OnFilterOptionsListTapped;
                this.FilterOptionsList.Loaded += OnFilterOptionsListLoaded;
                FilterOptionsList.KeyUp += OnFilterOptionsListKeyDown;
#endif

            }
#if WPF
            if (this.FilterOptionButton != null)
            {
                this.FilterOptionButton.MouseEnter += OnFilterOptionButtonMouseEnter;
                this.FilterOptionButton.MouseLeave += OnFilterOptionButtonMouseLeave;
            }
#endif
            this.ColumnBase.GridColumn.ColumnPropertyChanged += OnColumnPropertyChanged;
        }

        /// <summary>
        /// UnWires the wired events.
        /// </summary>
        protected void UnWireEvents()
        {
            if (this.FilterOptionButton != null)
            {
#if WPF
                this.FilterOptionButton.PreviewMouseLeftButtonUp -= OnFilterOptionButtonPreviewMouseLeftButtonUp;
                this.FilterOptionButton.PreviewMouseLeftButtonDown -= OnFilterOptionButtonPreviewMouseLeftButtonDown;
#else
                this.FilterOptionButton.Click -= OnFilterOptionButtonClick;
                this.FilterOptionsList.Loaded -= OnFilterOptionsListLoaded;
#endif
                this.FilterOptionButton = null;
            }

            if (FilterOptionsList != null)
            {
#if WPF
                FilterOptionsList.MouseLeftButtonUp -= OnFilterOptionsListMouseLeftButtonUp;
#else
                FilterOptionsList.Tapped -= OnFilterOptionsListTapped;
#endif
                FilterOptionsList.KeyDown -= OnFilterOptionsListKeyDown;
                FilterOptionsList = null;
            }

#if WPF
            if (this.FilterOptionButton != null)
            {
                this.FilterOptionButton.MouseEnter -= OnFilterOptionButtonMouseEnter;
                this.FilterOptionButton.MouseLeave -= OnFilterOptionButtonMouseLeave;
            }
#endif

            this.ColumnBase.GridColumn.ColumnPropertyChanged -= OnColumnPropertyChanged;
        }

        #endregion

        #region Overrides

#if WinRT || UNIVERSAL
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            UnWireEvents();
            this.FilterOptionButton = GetTemplateChild("PART_FilterOptionButton") as ToggleButton;
            FilterOptionsList = GetTemplateChild("PART_FilterOptionsList") as ListBox;
            FilterOptionPopup = GetTemplateChild("PART_FilterOptionPopup") as Popup;
            WireEvents();
#if WPF
            //WPF-28914 We need open the filter popup on mouse down when AllowSelectionOnPointerPressed property is enabled. Hence the binding
            //is skiped using AllowSelectionOnPointerPressed property.
            if (FilterOptionPopup != null && !this.ColumnBase.Renderer.DataGrid.AllowSelectionOnPointerPressed)
#else
            if (FilterOptionPopup != null)
#endif
                UpdateBinding();
            this.UpdateFilterOptionButtonVisibility();
            this.ApplyGridCellVisualStates(this.GridCellRegion, false);
        }

        /// <summary>
        /// Updates the Visual State of the GridCell based on the cell validation applied on SfDataGrid.
        /// </summary>
        protected internal override void ApplyValidationVisualState(bool canApplyDefaultState = true)
        {
            return;
        }

        /// <summary>
        /// Invoked when ColumnBase properties are changed.
        /// </summary>
        /// <param name="propertyName">The name of the property which is changed.</param>
        protected internal override void OnDataColumnPropertyChanged(string propertyName)
        {
            if (propertyName == "GridColumn")
                this.UpdateFilterOptionButtonVisibility();
        }
        #endregion
#if WPF
        #region Automation Overrides
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new GridFilterRowCellAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
        #endregion
#endif
    }

    /// <summary>
    /// Represents a class that loads row control for FilterRow.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class FilterRowControl : VirtualizingCellsControl
    {
#region Ctr

        public FilterRowControl()
        {
            this.DefaultStyleKey = typeof(FilterRowControl);
        }

#endregion
    }

    /// <summary>
    /// Provides the required functionalities that have to be used in renderers.
    /// </summary>
    public interface IGridFilterRowCell
    {

        /// <summary>
        /// True if the FilterOptionPopup has been in Open.
        /// </summary>
        bool IsDropDownOpen { get; }

        /// <summary>
        /// Returns the DataColumn that holds the corresponding GridCell.
        /// </summary>
        DataColumnBase DataColumn { get; }

        /// <summary>
        /// Opens the FilterOptionPopup.
        /// </summary>
        void OpenFilterOptionPopup();

        /// <summary>
        /// Close the FilterOptionPopup.
        /// </summary>
        void CloseFilterOptionPopup();
    }

}
