#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Syncfusion.Data;
#if WinRT
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.Foundation;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Key = Windows.System.VirtualKey;
using KeyEventArgs = Windows.UI.Xaml.Input.KeyRoutedEventArgs;
using Windows.UI.Xaml.Data;
using System.Collections;
using Syncfusion.UI.Xaml.Utility;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections;
using Syncfusion.UI.Xaml.Utility;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a control that provides excel like filter interface with list of check box’s.
    /// </summary>
    public class CheckboxFilterControl : ContentControl, IDisposable
    {
        #region Private Members
        internal bool propertyChangedFromSelectAll = false;
        internal IEnumerable<FilterElement> FilterListBoxItem = new List<FilterElement>();
        // WPF-19874 - To maintain checked and unchecked items of FilterElement while entering text in search textBox
        internal List<FilterElement> previousItemSource = new List<FilterElement>();
        internal IEnumerable<FilterElement> searchedItems = new List<FilterElement>();
        internal bool isSourceChangedasSearchedItems = false;
        internal GridFilterControl gridFilterCtrl = null;
        private bool isdisposed = false;
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #region Dependency Properties
#if !WPF
        #region FilteredFrom
        /// <summary>
        /// DependencyProperty Registration for FilteredFrom of GridFilterControl
        /// </summary>
        /// <remarks></remarks>
        public static readonly DependencyProperty FilteredFromProperty = DependencyProperty.Register(
          "FilteredFrom", typeof(FilteredFrom), typeof(CheckboxFilterControl), new PropertyMetadata(FilteredFrom.None));

        /// <summary>
        /// Gets or sets FilteredFrom for GridFilterControl.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        public FilteredFrom FilteredFrom
        {
            get { return (FilteredFrom)this.GetValue(CheckboxFilterControl.FilteredFromProperty); }
            set { this.SetValue(CheckboxFilterControl.FilteredFromProperty, value); }
        }

        #endregion
#endif
        #region SearchOptionVisibilityProperty

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.SearchOptionVisibility dependency property.
        /// </summary>    
        public static readonly DependencyProperty SearchOptionVisibilityProperty = DependencyProperty.Register(
           "SearchOptionVisibility", typeof(Visibility), typeof(CheckboxFilterControl), new PropertyMetadata(Visibility.Visible));

        /// <summary>
        /// Gets or sets a value indicating the SearchOption Visibility.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the visibility of SearchOption.
        /// The default mode is <see cref="System.Windows.Visibility.Visible"/>. 
        /// </value>
        public Visibility SearchOptionVisibility
        {
            get { return (Visibility)this.GetValue(CheckboxFilterControl.SearchOptionVisibilityProperty); }
            set { this.SetValue(CheckboxFilterControl.SearchOptionVisibilityProperty, value); }
        }

        /// <summary>
        /// Gets or sets a value indicating the Search watermark Visibility.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies the visibility of Search watermark.
        /// The default mode is <see cref="System.Windows.Visibility.Visible"/>. 
        /// </value>
        public Visibility SearchTextBlockVisibility
        {
            get { return (Visibility)GetValue(SearchTextBlockVisibilityProperty); }
            set { SetValue(SearchTextBlockVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.SearchTextBlockVisibility dependency property.
        /// </summary> 
        public static readonly DependencyProperty SearchTextBlockVisibilityProperty =
            DependencyProperty.Register("SearchTextBlockVisibility", typeof(Visibility), typeof(CheckboxFilterControl), new PropertyMetadata(Visibility.Visible));

        #endregion

        #region ItemsSource
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.ItemsSource dependency property.
        /// </summary> 
        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(
          "ItemsSource", typeof(object), typeof(CheckboxFilterControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the collection that is used to generate the content of the CheckboxFilterControl.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of the CheckboxFilterControl.The default value is <b>null</b>.
        /// </value>
        public Object ItemsSource
        {
            get { return (object)this.GetValue(CheckboxFilterControl.ItemsSourceProperty); }
            set { this.SetValue(CheckboxFilterControl.ItemsSourceProperty, value); }
        }
        #endregion

        #region HasItemsSource
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.HasItemsSource dependency property.
        /// </summary> 
        public static readonly DependencyProperty HasItemsSourceProperty = DependencyProperty.Register(
          "HasItemsSource", typeof(bool), typeof(CheckboxFilterControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that indicates whether CheckboxFilterControl has ItemsSource.
        /// </summary>
        /// <value>
        /// <b>true</b> if CheckboxFilterControl has ItemsSource; otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool HasItemsSource
        {
            get { return (bool)this.GetValue(CheckboxFilterControl.HasItemsSourceProperty); }
            set { this.SetValue(CheckboxFilterControl.HasItemsSourceProperty, value); }
        }
        #endregion

        #region IsItemSourceLoaded
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.IsItemSourceLoaded dependency property.
        /// </summary> 
        public static readonly DependencyProperty IsItemSourceLoadedProperty = DependencyProperty.Register(
          "IsItemSourceLoaded", typeof(bool), typeof(CheckboxFilterControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether ItemsSource is loaded in CheckboxFilterControl.
        /// </summary>
        /// <value>
        /// <b>true</b> if ItemsSource is loaded in CheckboxFilterControl; otherwise,<b>false</b>. The default value is <b>false</b>.
        /// </value>
        public bool IsItemSourceLoaded
        {
            get { return (bool)this.GetValue(CheckboxFilterControl.IsItemSourceLoadedProperty); }
            set { this.SetValue(CheckboxFilterControl.IsItemSourceLoadedProperty, value); }
        }
        #endregion

        /// <summary>
        /// Gets or sets the template that is used to display the items in CheckBoxFilterControl.
        /// </summary>   
        /// <value>
        /// The template that is used to specify the visualization of the data objects.The default value is <c>null</c>.
        /// </value>       
        public DataTemplate ItemTemplate
        {
            get { return (DataTemplate)GetValue(ItemTemplateProperty); }
            set { SetValue(ItemTemplateProperty, value); }
        }
        
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.CheckboxFilterControl.ItemTemplate dependency property.
        /// </summary> 
        public static readonly DependencyProperty ItemTemplateProperty =
            DependencyProperty.Register("ItemTemplate", typeof(DataTemplate), typeof(CheckboxFilterControl), new PropertyMetadata(null));


        #endregion

        #region Ctor
        public CheckboxFilterControl()
        {
            this.DefaultStyleKey = typeof(CheckboxFilterControl);
            this.Loaded += OnCheckboxFilterControlLoaded;
        }

        #endregion

        #region Events

        #region SelectAllCheckBoxChecked

        /// <summary>
        /// Occurs when the SelectAllCheckBox is checked
        /// </summary>  
        public event SelectAllCheckBoxCheckedEventHandler SelectAllCheckBoxChecked;

        /// <summary>
        /// Raises the select all check box checked.
        /// </summary>
        /// <param name="FilterElements">The filter elements.</param>
        internal void RaiseSelectAllCheckBoxChecked(List<FilterElement> FilterElements)
        {
            OnSelectAllCheckBoxChecked(new SelectAllCheckBoxCheckedEventArgs() { FilterElements = FilterElements });
        }

        /// <summary>
        /// Raises the <see cref="E:SelectAllCheckBoxChecked"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Controls.Grid.SelectAllCheckBoxCheckedEventArgs"/> instance containing the event data.</param>
        private void OnSelectAllCheckBoxChecked(SelectAllCheckBoxCheckedEventArgs e)
        {
            if (this.SelectAllCheckBoxChecked != null)
                this.SelectAllCheckBoxChecked(this, e);
        }

        #endregion

        #region SelectAllUnCheckBoxChecked

        /// <summary>
        /// Occurs when the SelectAllCheckBox is unchecked
        /// </summary>  
        public event SelectAllCheckBoxUnCheckedEventHandler SelectAllUnCheckBoxChecked;

        /// <summary>
        /// Raises the select all un check box checked.
        /// </summary>
        /// <param name="FilterElements">The filter elements.</param>
        internal void RaiseSelectAllUnCheckBoxChecked(List<FilterElement> FilterElements)
        {
            OnSelectAllUnCheckBoxChecked(new SelectAllCheckBoxUnCheckedEventArgs() { FilterElements = FilterElements });
        }

        /// <summary>
        /// Raises the <see cref="E:SelectAllUnCheckBoxChecked"/> event.
        /// </summary>
        /// <param name="e">The <see cref="Syncfusion.Windows.Controls.Grid.SelectAllCheckBoxUnCheckedEventArgs"/> instance containing the event data.</param>
        private void OnSelectAllUnCheckBoxChecked(SelectAllCheckBoxUnCheckedEventArgs e)
        {
            if (this.SelectAllUnCheckBoxChecked != null)
                this.SelectAllUnCheckBoxChecked(this, e);
        }

        #endregion

        #region CheckboxFCLoaded

        void OnCheckboxFilterControlLoaded(object sender, RoutedEventArgs e)
        {
			//Set the scrollviewer when checkboxcontrol loaded instead itemscontrol loaded event. Because template is applied after itemscontrol loaded.
            if (this.PART_ItemsControl != null)
                PartScrollViewer = GridUtil.GetChildObject<ScrollViewer>(this.PART_ItemsControl, "");
            WireEvents();
        }

        void CheckboxFilterControl_Unloaded(object sender, RoutedEventArgs e)
        {
            UnWireEvents();
        }

        /// <summary>
        /// Delete button click
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SearchTextBox != null)
            {
                this.SearchTextBlockVisibility = Visibility.Visible;
                this.SearchTextBox.ClearValue(TextBox.TextProperty);
#if UWP
                this.SearchTextBox.Focus(FocusState.Keyboard);
#endif
            }
        }

        #region Search TextBox

        /// <summary>
        /// OnSearchTextBoxTextChanged Event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.Controls.TextChangedEventArgs">TextChangedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
#if UWP
        async void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
#else
        void OnSearchTextBoxTextChanged(object sender, TextChangedEventArgs e)
#endif
        {
            var textBox = sender as TextBox;
            var filterText = textBox.Text.ToLower();
            if (this.FilterListBoxItem == null)
                return;

            if (string.IsNullOrEmpty(filterText))
            {
                searchedItems = new List<FilterElement>();
                // Set FilterListBoxItems IsSelected value based on previousItemSource
                if (this.previousItemSource != null)
                {   
                    var checkedItemsCount = previousItemSource.Count(x => x.IsSelected);                    
                    bool isSelected = true;                                       
                    if (checkedItemsCount == 0)
                        isSelected = false;
                    propertyChangedFromSelectAll = true;
                    if (this.gridFilterCtrl.Column.ColumnFilter == ColumnFilter.Value)
                    {
                        foreach (var item in FilterListBoxItem)
                        {
                            var filterElement = previousItemSource.FirstOrDefault(i => item.ActualValue == i.ActualValue);
                            item.IsSelected = filterElement != null ? filterElement.IsSelected : !isSelected;
                        }
                    }
                    else
                    {
                        foreach (var item in FilterListBoxItem)
                        {                           
                            var filterElement = previousItemSource.FirstOrDefault(i => item.DisplayText == i.DisplayText);
                            item.IsSelected = filterElement != null ? filterElement.IsSelected : !isSelected;
                        }
                    }
                    propertyChangedFromSelectAll = false;
                }
                ItemsSource = FilterListBoxItem;
                isSourceChangedasSearchedItems = false;
                this.SelectAllCheckBox.IsEnabled = FilterListBoxItem.Any();
                this.MaintainSelectAllCheckBox();
                return;
            }

#if UWP
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
#endif
            searchedItems =
                this.FilterListBoxItem.Where(input => input.DisplayText.ToLower().Contains(filterText)).ToList();
#if UWP
            });
#endif

            propertyChangedFromSelectAll = true;
            searchedItems.ForEach(x => x.IsSelected = true);
            propertyChangedFromSelectAll = false;

            ItemsSource = searchedItems;
            isSourceChangedasSearchedItems = true;
            this.SelectAllCheckBox.IsEnabled = searchedItems.Any();
            this.MaintainSelectAllCheckBox();
        }

        /// <summary>
        /// SearchTextBox_KeyDown Event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.Input.KeyRoutedEventArgs">KeyRoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void SearchTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                var textBox = sender as TextBox;
                textBox.ClearValue(TextBox.TextProperty);
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                gridFilterCtrl.InvokeFilter();
                gridFilterCtrl.IsOpen = false;
            }

        }

        void SearchTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            this.SearchTextBlockVisibility = Visibility.Collapsed;
        }

        void SearchTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (this.SearchTextBox.Text == string.Empty)
                SearchTextBlockVisibility = Visibility.Visible;
        }

#if WPF
        void SearchTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            UIElement focusedelement = Keyboard.FocusedElement as UIElement;
            if (e.Key == Key.Down)
            {
                focusedelement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                focusedelement.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
        }
#endif
        #endregion

        #region Select All State Listner

        /// <summary>
        /// Called when [select all unchecked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnSelectAllUnchecked(object sender, RoutedEventArgs e)
        {
            propertyChangedFromSelectAll = true;
            this.FilterListBoxItem.ForEach(c => c.IsSelected = false);
            this.MaintainSelectAllCheckBox();
            propertyChangedFromSelectAll = false;
        }

        /// <summary>
        /// Called when [select all checked].
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
        void OnSelectAllChecked(object sender, RoutedEventArgs e)
        {
            propertyChangedFromSelectAll = true;
            this.FilterListBoxItem.ForEach(c => c.IsSelected = true);
            this.MaintainSelectAllCheckBox();
            propertyChangedFromSelectAll = false;
        }

        #endregion
        #endregion

        #endregion

        #region Methods

        internal void MaintainSelectAllCheckBox()
        {
            var itemsSource = this.ItemsSource as IEnumerable<FilterElement>;
            if (itemsSource == null || this.SelectAllCheckBox == null)
                return;

            this.SelectAllCheckBox.IsEnabled = itemsSource.Any();

            var uncheked = itemsSource.Where(y => !y.IsSelected).ToList();
            if (uncheked.Count == 0)
            {
                this.SelectAllCheckBox.IsThreeState = false;
                this.SelectAllCheckBox.IsChecked = true;
                this.SelectAllCheckBox.IsEnabled = true;
                if (gridFilterCtrl.OkButton != null && !gridFilterCtrl.IsAdvancedFilterVisible)
                    gridFilterCtrl.OkButton.IsEnabled = true;
            }
            else if (uncheked.Count == itemsSource.Count())
            {
                this.SelectAllCheckBox.IsThreeState = false;
                this.SelectAllCheckBox.IsChecked = false;
                if (gridFilterCtrl.OkButton != null && !gridFilterCtrl.IsAdvancedFilterVisible)
                    gridFilterCtrl.OkButton.IsEnabled = false;
            }
            else
            {
                this.SelectAllCheckBox.IsThreeState = true;
                if (gridFilterCtrl.OkButton != null)
                    gridFilterCtrl.OkButton.IsEnabled = true;
                this.SelectAllCheckBox.IsChecked = null;
            }

            if (gridFilterCtrl.OkButton != null && (!itemsSource.Any() || gridFilterCtrl.ImmediateUpdateColumnFilter || !this.HasItemsSource)
                                                 && !gridFilterCtrl.IsAdvancedFilterVisible)
                gridFilterCtrl.OkButton.IsEnabled = false;
        }

        internal void MaintainAPIChanges()
        {
            if (this.gridFilterCtrl.CheckboxFilterStyle != null)
                this.Style = this.gridFilterCtrl.CheckboxFilterStyle;
        }
        #endregion

        #region UIElements

        /// <summary>
        /// Gets or sets the select all check box.
        /// </summary>
        /// <value>The select all check box.</value>
        internal CheckBox SelectAllCheckBox;

        /// <summary>
        /// Gets or sets the search text box.
        /// </summary>
        /// <value>The search text box.</value>
        internal TextBox SearchTextBox;

        /// <summary>
        /// Gets or sets the ScrollViwer of ListPart Area.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        internal ScrollViewer PartScrollViewer;

        /// <summary>
        /// Gets or sets the ItemsControl.
        /// </summary>
        /// <value></value>
        /// <remarks></remarks>
        internal ItemsControl PART_ItemsControl;

        /// <summary>
        /// Gets or sets the delete button.
        /// </summary>
        /// <value>The cancel button.</value>
        internal Button DeleteButton;

        #endregion

        #region Wire/UnWire Events

        private void WireEvents()
        {
            this.Unloaded += CheckboxFilterControl_Unloaded;
            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.Checked += OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked += OnSelectAllUnchecked;
            }

            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.LostFocus += SearchTextBox_LostFocus;
                this.SearchTextBox.GotFocus += SearchTextBox_GotFocus;
#if WPF
                this.SearchTextBox.PreviewKeyDown += SearchTextBox_PreviewKeyDown;
#endif
                this.SearchTextBox.KeyDown += SearchTextBox_KeyDown;
                this.SearchTextBox.TextChanged += OnSearchTextBoxTextChanged;
            }
				//No need to fires the ItemsControlLoaded. It was loaded when checkbox control loaded 
            //if (this.PART_ItemsControl != null)
            //{
            //    // this.PART_ItemsControl.Loaded += PART_ItemsControl_Loaded;
            //}

            if (this.DeleteButton != null)
                this.DeleteButton.Click += DeleteButton_Click;
        }

        private void UnWireEvents()
        {
            this.Unloaded -= CheckboxFilterControl_Unloaded;
            if (this.SearchTextBox != null)
            {
                this.SearchTextBox.GotFocus -= SearchTextBox_GotFocus;
                this.SearchTextBox.LostFocus -= SearchTextBox_LostFocus;
#if WPF
                this.SearchTextBox.PreviewKeyDown -= SearchTextBox_PreviewKeyDown;
#endif
                this.SearchTextBox.KeyDown -= SearchTextBox_KeyDown;
                this.SearchTextBox.TextChanged -= OnSearchTextBoxTextChanged;
            }

            if (this.SelectAllCheckBox != null)
            {
                this.SelectAllCheckBox.Checked -= OnSelectAllChecked;
                this.SelectAllCheckBox.Unchecked -= OnSelectAllUnchecked;
            }

            //if (this.PART_ItemsControl != null)
            //    this.PART_ItemsControl.Loaded -= PART_ItemsControl_Loaded;

            if (this.DeleteButton != null)
                this.DeleteButton.Click -= DeleteButton_Click;

        }

        /// <summary>
        /// PART_ItemsControl_Loaded event.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An <see cref="T:Windows.UI.Xaml.RoutedEventArgs">RoutedEventArgs</see> that contains the event data.</param>
        /// <remarks></remarks>
        void PART_ItemsControl_Loaded(object sender, RoutedEventArgs e)
        {
            var itemsControl = sender as ItemsControl;
            PartScrollViewer = GridUtil.GetChildObject<ScrollViewer>(itemsControl, "");
        }

        #endregion

        #region Overrides
        /// <summary>
        /// Builds the visual tree for the CheckboxFilterControl when a new template is applied.
        /// </summary>
#if UWP
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
            base.OnApplyTemplate();
            SelectAllCheckBox = this.GetTemplateChild("PART_CheckBox") as CheckBox;
            this.DeleteButton = this.GetTemplateChild("PART_DeleteButton") as Button;
            this.PART_ItemsControl = this.GetTemplateChild("PART_ItemsControl") as ItemsControl;
            this.SearchTextBox = this.GetTemplateChild("PART_SearchTextBox") as TextBox;

            this.MaintainSelectAllCheckBox();
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.CheckboxFilterControl"/> class.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireEvents();
            if (isDisposing)
            {
                if (this.previousItemSource != null)
                {
                    previousItemSource.Clear();
                    previousItemSource = null;
                }
                this.Loaded -= OnCheckboxFilterControlLoaded;
                if (gridFilterCtrl != null)
                    gridFilterCtrl = null;
            }
            isdisposed = true;
        }

        #endregion

    }
}
