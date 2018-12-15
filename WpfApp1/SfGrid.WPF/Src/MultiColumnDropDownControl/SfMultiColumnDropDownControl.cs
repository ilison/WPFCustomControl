#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Helpers;
using System.Data;
using Syncfusion.Data.Extensions;
#if WinRT || UNIVERSAL
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Syncfusion.Data;
using Windows.Devices.Input;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Syncfusion.Dynamic;
#else
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.ScrollAxis;
using System.Windows.Automation.Peers;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
#if !WPF
    using Key = Windows.System.VirtualKey;
    using KeyEventArgs = KeyRoutedEventArgs;
    using MouseButtonEventArgs = PointerRoutedEventArgs;
#endif
    #region Event Arguments and Handlers

    #region PopupOpening EventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.PopupOpeningEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.PopupOpeningEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PopupOpeningEventHandler(object sender, PopupOpeningEventArgs e);

    /// <summary>
    /// Provides data for pop-up opening event in <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
    /// </summary>
    /// <remarks>
    /// By setting args.Cancel as true, pop-up opening can be skipped.
    /// </remarks>
    public class PopupOpeningEventArgs : CancelEventArgs
    {

    }
    #endregion

    #region PopupOpened EventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.PopupopenedEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.PopupopenedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PopupopenedEventHandler(object sender, PopupopenedEventArgs e);

    /// <summary>
    /// Provides data for pop-up opened event in <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
    /// </summary>
    public class PopupopenedEventArgs : EventArgs
    {

    }
    #endregion

    #region PopupClosing EventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.PopupClosingEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.PopupClosingEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PopupClosingEventHandler(object sender, PopupClosingEventArgs e);

    /// <summaryp
    /// Provides data for Popup closing event in <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
    /// </summary>
    /// <remarks>
    /// By setting args.Cancel as true, pop-up closing can be skipped.
    /// </remarks>
    public class PopupClosingEventArgs : CancelEventArgs
    {

    }
    #endregion

    #region PopupClosed EventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.PopupClosedEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.PopupClosedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void PopupClosedEventHandler(object sender, PopupClosedEventArgs e);

    /// <summary>
    /// Provides data for pop-up closed event in <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
    /// </summary>
    public class PopupClosedEventArgs : EventArgs
    {

    }
    #endregion

    #region SelectionChanged EventHandler

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SelectionChangedEventArgs"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.SelectionChangedEventArgs"/> that contains the event data.
    /// </param>
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

    /// <summary>
    /// Provides data for SelectionChanged event in <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets the index of corresponding row in SfDataGrid.
        /// </summary>
        /// <value>
        /// The index of selected item.
        /// </value>                
        public int SelectedIndex { get; set; }

        /// <summary>
        /// Gets or sets the data item which is bound to the row that contains the selection.
        /// </summary>
        /// <value>
        /// The object that is currently selected in SfDataGrid.
        /// </value>
        public object SelectedItem { get; set; }
    }

    #endregion
    #endregion

    [TemplatePart(Name = "PART_TextBox", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_ToggleButton", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_Popup", Type = typeof(Popup))]
    [TemplatePart(Name = "PART_SfDataGrid", Type = typeof(SfDataGrid))]
    [TemplatePart(Name = "PART_ThumbGripper", Type = typeof(Thumb))]
#if !WPF
    [TemplatePart(Name = "PART_ContentControl", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_PopupBorder", Type = typeof(Border))]
#endif
    public class SfMultiColumnDropDownControl : Control, IDisposable
    {
        #region Private Fields
        private static readonly SolidColorBrush PopUpBackGround = new SolidColorBrush(Colors.Gainsboro);
        private static readonly SolidColorBrush PopUpSfDataGridBackGround = new SolidColorBrush(Colors.White);

        private IEnumerable<object> appendSource;
        private static double DefaultPopupMinHeight = 300.0;
        private static double DefaultPopupMinWidth = 400.0;
#if !WPF
        private bool IsLoaded = false;
#endif
        private bool isResized = false;
        private bool isInTextChange = false;
        private bool isSelectedIndexLoadedBeforeGridLoaded = false;
        private bool isSelectedItemLoadedBeforeGridLoaded = false;
        private bool isSelectedValueLoadedBeforeGridLoaded = false;
        private string filterText = string.Empty;
        private IPropertyAccessProvider reflector = null;
        private bool isdisposed = false;
        #endregion

        #region Internal Fields

        internal ToggleButton DropDownButton;
        internal Popup InternalPopup;
        internal Thumb ResizeThumb;
        internal object PreviousSelectedItem;
        internal bool IsInSuspend;
#if !WPF
        //Loaded in UWP and WinRT for ReadOnlyMode.
        internal ContentControl ContentControl;

        //denotes the popupwidth and popupheight.
        internal Border PopupBorder;
#endif
        #endregion

        #region Protected Fields

        /// <summary>
        /// Gets or sets the SfDataGrid which is loaded in the pop-up.
        /// </summary>
        protected internal SfDataGrid InternalGrid { get; set; }

        /// <summary>
        /// Gets or sets the TextBox which is loaded as editor in SfMultiColumnDropDownControl.
        /// </summary>
        protected internal TextBox Editor { get; set; }

        #endregion

        #region Dependency Properties

        #region SfMultiColumnProperties

        /// <summary>
        /// Gets or sets the horizontal alignment of the text displayed in the Textbox.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.TextAlignment"/> enumeration that specifies the horizontal alignment of the Textbox text. 
        /// The default value is <b>TextAlignment.Left</b>.
        /// </value>
        public TextAlignment TextAlignment
        {
            get { return (TextAlignment)GetValue(TextAlignmentProperty); }
            set { SetValue(TextAlignmentProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.TextAlignment dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.TextAlignment dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextAlignmentProperty =
            DependencyProperty.Register("TextAlignment", typeof(TextAlignment), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(TextAlignment.Left));


        /// <summary>
        /// Gets or sets a value that indicates whether the textbox text is need to auto append rather than typing the entire text in Textbox.
        /// </summary>
        /// <value>
        /// <b>true</b> if the auto completion is enabled; otherwise, <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool AllowAutoComplete
        {
            get { return (bool)GetValue(AllowAutoCompleteProperty); }
            set { SetValue(AllowAutoCompleteProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowAutoComplete dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowAutoComplete dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowAutoCompleteProperty =
            DependencyProperty.Register("AllowAutoComplete", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the null values are allowed in the Textbox.
        /// </summary>
        /// <value>
        /// <b>true</b> if the null values are allowed; otherwise, <b>false</b>. 
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowNullInput
        {
            get { return (bool)GetValue(AllowNullInputProperty); }
            set { SetValue(AllowNullInputProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowNullInput dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowAutoComplete dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowNullInputProperty =
            DependencyProperty.Register("AllowNullInput", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets a value that indicates whether the pop-up is open while typing the value in Textbox itself.
        /// </summary>
        /// <value>
        /// <c>true</c> if the pop-up is opened while typing the text in Textbox; otherwise, <c>false</c>.
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowImmediatePopup
        {
            get { return (bool)GetValue(AllowImmediatePopupProperty); }
            set { SetValue(AllowImmediatePopupProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowImmediatePopup dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowImmediatePopup dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowImmediatePopupProperty =
            DependencyProperty.Register("AllowImmediatePopup", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false));


        /// <summary>
        /// Gets or sets a value that indicates whether the pop-up height and width is automatically adjusted based on rows and columns count.
        /// </summary>
        /// <value>
        /// <b>true</b> if the pop-up height and width is automatically adjusted based on rows and column count; otherwise, <b>false</b>. 
        /// The default value is <b>true</b>.
        /// </value>        
        /// <remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupHeight"/>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupWidth"/>
        /// </remarks>
        public bool IsAutoPopupSize
        {
            get { return (bool)GetValue(IsAutoPopupSizeProperty); }
            set { SetValue(IsAutoPopupSizeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsAutoPopupSizeProperty =
            DependencyProperty.Register("IsAutoPopupSize", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true, OnIsAutoPopupSizeChanged));

        private static void OnIsAutoPopupSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDownControl = (d as SfMultiColumnDropDownControl);

            if (!sfMultiColumnDropDownControl.IsLoaded)
                return;

            if (!(bool)e.NewValue)
            {
                sfMultiColumnDropDownControl.PopupWidth = DefaultPopupMinWidth;
                sfMultiColumnDropDownControl.PopupHeight = DefaultPopupMinHeight;
            }
            sfMultiColumnDropDownControl.isResized = false;
        }

        /// <summary>
        /// Gets the text which is entered in the Textbox.
        /// </summary>
        /// <value>
        /// The text which is entered in the Textbox.
        /// The default value is <b>String.Empty</b>.
        /// </value>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            private set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SearchText dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SearchText dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register("SearchText", typeof(string), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets the filtered items from the SfDataGrid which is present in the pop-up.
        /// </summary>
        /// <value>
        /// The filtered items.
        /// The default value is <b>null</b>.
        /// </value>
        public IEnumerable FilteredItems
        {
            get { return (IEnumerable)GetValue(FilteredItemsProperty); }
            private set { SetValue(FilteredItemsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.FilteredItems dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.FilteredItems dependency property.
        /// </remarks>
        public static readonly DependencyProperty FilteredItemsProperty =
            DependencyProperty.Register("FilteredItems", typeof(IEnumerable), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets a value that indicates whether the SfDataGrid records are filter while entering the text in Textbox.
        /// </summary>
        /// <value>
        /// <b>true</b> if the SfDataGrid records are filter based on the entered text; otherwise, <b>false</b>.
        /// </value>
        /// The default value is <b>true</b>.
        /// <remarks>
        /// <remarks>Records are filtered based on <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/> when <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering"/> is enabled.
        /// </remarks>
        public bool AllowIncrementalFiltering
        {
            get { return (bool)GetValue(AllowIncrementalFilteringProperty); }
            set { SetValue(AllowIncrementalFilteringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowIncrementalFilteringProperty =
            DependencyProperty.Register("AllowIncrementalFiltering", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates how the records are filter when <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering"/> is enabled.
        /// </summary>
        /// <value>One of the enumeration <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/> that denotes the filtering condition.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition.StartsWith"/>.
        /// </value>
        /// <remarks>Records are filtered based on <see cref="Syncfusion.UI.Xaml.Grid.SearchCondition"/> when <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering"/> is enabled.
        /// </remarks>
        public SearchCondition SearchCondition
        {
            get { return (SearchCondition)GetValue(SearchConditionProperty); }
            set { SetValue(SearchConditionProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SearchCondition dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SearchCondition dependency property.
        /// </remarks>
        public static readonly DependencyProperty SearchConditionProperty =
            DependencyProperty.Register("SearchCondition", typeof(SearchCondition), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(SearchCondition.StartsWith));

        /// <summary>
        /// Gets or sets a value that indicates whether the Textbox text is selected or not while focusing it.
        /// </summary>
        /// <value>
        /// <b>true</b>if select the text in Textbox; otherwise, <b>false</b>.</value>
        /// The default value is <b>true</b>.
        public bool TextSelectionOnFocus
        {
            get { return (bool)GetValue(TextSelectionOnFocusProperty); }
            set { SetValue(TextSelectionOnFocusProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.TextSelectionOnFocus dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.TextSelectionOnFocus dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextSelectionOnFocusProperty =
            DependencyProperty.Register("TextSelectionOnFocus", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates the case-sensitive when <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowIncrementalFiltering"/> and <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowAutoComplete"/> is enabled.
        /// </summary>
        /// <value>
        /// <b>true</b> if the case-sensitive is enabled for AllowIncrementalFiltering, AllowAutoComplete; otherwise, <b>false</b>.
        /// The default value is <b>false</b>.
        /// </value>
        public bool AllowCaseSensitiveFiltering
        {
            get { return (bool)GetValue(AllowCaseSensitiveFilteringProperty); }
            set { SetValue(AllowCaseSensitiveFilteringProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowCaseSensitiveFiltering dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowCaseSensitiveFiltering dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowCaseSensitiveFilteringProperty =
            DependencyProperty.Register("AllowCaseSensitiveFiltering", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the collection that contains all the columns in SfMultiColumnDropDownControl.
        /// </summary>
        /// <value>
        /// The collection that contains all the columns in SfMultiColumnDropDownControl.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.Columns"/>
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
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.Columns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.Columns dependency property.
        /// </remarks>
        public static readonly DependencyProperty ColumnsProperty =
#if WPF
            DependencyProperty.Register("Columns", typeof(Columns), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(new Columns(), OnColumnPropertyChanged));
#else
            DependencyProperty.Register("Columns", typeof(Columns), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null, OnColumnPropertyChanged));
#endif

        /// <summary>        
        /// Gets or sets a SelectedItem when mouse wheel over the control.
        /// </summary>
        /// <value>
        /// <b>true</b> if the <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedItem"/> gets changed when mouse wheel over the control; otherwise, <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>
        public bool AllowSpinOnMouseWheel
        {
            get { return (bool)GetValue(AllowSpinOnMouseWheelProperty); }
            set { SetValue(AllowSpinOnMouseWheelProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowSpinOnMouseWheel dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AllowSpinOnMouseWheel dependency property.
        /// </remarks>
        public static readonly DependencyProperty AllowSpinOnMouseWheelProperty =
            DependencyProperty.Register("AllowSpinOnMouseWheel", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates whether the Textbox is allowed to edit or not.
        /// </summary>
        /// <value>
        /// <b>true</b> if the Textbox is read-only; otherwise, <b>false</b>. 
        /// The default value is <b>false</b>.
        /// </value>
        public bool ReadOnly
        {
            get { return (bool)GetValue(ReadOnlyProperty); }
            set { SetValue(ReadOnlyProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ReadOnly dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ReadOnly dependency property.
        /// </remarks>
#if WPF
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false));
#else
        public static readonly DependencyProperty ReadOnlyProperty =
            DependencyProperty.Register("ReadOnly", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false, OnReadOnlyChanged));

        private static void OnReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDownControl = (d as SfMultiColumnDropDownControl);

            if (!sfMultiColumnDropDownControl.IsLoaded || (sfMultiColumnDropDownControl.Editor == null && sfMultiColumnDropDownControl.ContentControl == null))
                return;

            if ((bool)e.NewValue)
            {
                if (sfMultiColumnDropDownControl.Editor != null)
                    sfMultiColumnDropDownControl.UnWireEditorEvents();
                sfMultiColumnDropDownControl.WireReadOnlyEvents();
            }
            else
            {
                if (sfMultiColumnDropDownControl.ContentControl != null)
                    sfMultiColumnDropDownControl.UnWireReadOnlyEvents();
                sfMultiColumnDropDownControl.WireEditorEvents();
            }
        }
#endif

        /// <summary>
        /// Gets or sets a value that indicates whether the drop-down is need to open while loading itself.
        /// </summary>
        /// <value>
        /// <c>true</c> if the drop-down in opened state; otherwise, <c>false</c>.
        /// The default value is <b>false</b>.
        /// </value>
        public bool IsDropDownOpen
        {
            get { return (bool)GetValue(IsDropDownOpenProperty); }
            set { SetValue(IsDropDownOpenProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsDropDownOpen dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsDropDownOpen dependency property.
        /// </remarks>
        public static readonly DependencyProperty IsDropDownOpenProperty =
            DependencyProperty.Register("IsDropDownOpen", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false, OnIsDropDownOpenChanged));

        /// <summary>
        /// Gets or sets a string that specifies the text displayed in the Textbox.
        /// </summary>
        /// <value>
        /// A string that specifies the text displayed in the Textbox.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.Text dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.Text dependency property.
        /// </remarks>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the path in underlying data object which is used to display the visual presentation of the control.
        /// </summary>
        /// <value>
        /// A string specifying the name of an object in underlying data object which is used to display the text in control.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public string DisplayMember
        {
            get { return (string)GetValue(DisplayMemberProperty); }
            set { SetValue(DisplayMemberProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.DisplayMember dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.DisplayMember dependency property.
        /// </remarks>
        public static readonly DependencyProperty DisplayMemberProperty =
            DependencyProperty.Register("DisplayMember", typeof(string), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the path in underlying data object which is used to get the SelectedValue.
        /// </summary>
        /// <value>
        /// A string specifying the name of an object in underlying data object which is used to get the SelectedValue from the SelectedItem.
        /// The default value is <b>string.Empty</b>.
        /// </value>
        public string ValueMember
        {
            get { return (string)GetValue(ValueMemberProperty); }
            set { SetValue(ValueMemberProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ValueMember dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ValueMember dependency property.
        /// </remarks>
        public static readonly DependencyProperty ValueMemberProperty =
            DependencyProperty.Register("ValueMember", typeof(string), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets or sets the background to pop-up.
        /// </summary>
        /// <value>
        /// The pop-up background.
        /// The default value is <b>Colors.Gainsboro</b>.
        /// </value>
        public Brush PopupBackground
        {
            get { return (Brush)GetValue(PopupBackgroundProperty); }
            set { SetValue(PopupBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBackground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBackground dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupBackgroundProperty =
            DependencyProperty.Register("PopupBackground", typeof(Brush), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(PopUpBackGround));

        /// <summary>
        /// Gets or sets the background to the SfDataGrid displayed in the pop-up.
        /// </summary>
        /// <value>
        /// The SfDataGrid background.
        /// The default value is <b>Colors.White</b>.
        /// </value>
        public Brush PopupDropDownGridBackground
        {
            get { return (Brush)GetValue(PopupDropDownGridBackgroundProperty); }
            set { SetValue(PopupDropDownGridBackgroundProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupDropDownGridBackground dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupDropDownGridBackground dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupDropDownGridBackgroundProperty =
            DependencyProperty.Register("PopupDropDownGridBackground", typeof(Brush), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(PopUpSfDataGridBackGround));

        /// <summary>
        /// Gets or sets the index of selected row in SfDataGrid which is present in pop-up.
        /// </summary>
        /// <value>
        /// Index of the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.SelectedItem"/>
        /// The default value is <b>-1</b>.
        /// </value>
        public int SelectedIndex
        {
            get { return (int)GetValue(SelectedIndexProperty); }
            set { SetValue(SelectedIndexProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedIndex dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedIndex dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectedIndexProperty =
            DependencyProperty.Register("SelectedIndex", typeof(int), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(-1, OnSelectedIndexChanged));

        /// <summary>
        /// Gets or sets the data item which is bound to the row that contains the selection.
        /// </summary>
        /// <value>
        /// The object that is currently selected in the SfDataGrid.
        /// The default value is <b>null</b>.
        /// </value>
        public object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedItem dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedItem dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register("SelectedItem", typeof(object), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null, OnSelectedItemChanged));

        /// <summary>
        /// Gets or sets the value based on ValueMember from SelectedItem.
        /// </summary>
        /// <value>
        /// The selected value will be set based on <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ValueMember"/>. 
        /// The default value is <b>null</b>.
        /// </value>
        public object SelectedValue
        {
            get { return GetValue(SelectedValueProperty); }
            set { SetValue(SelectedValueProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedValue dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SelectedValue dependency property.
        /// </remarks>
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register("SelectedValue", typeof(object), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null, OnSelectedValueChanged));

        /// <summary>
        /// Gets or sets a value that indicates the visibility of resizing thumb which is used to resize the pop-up.
        /// </summary>
        /// <value>
        /// One of the <see cref="System.Windows.Visibility"/> enumeration that specifies visibility of resizing thumb. The default value is <b>Visibility.Visible</b>.
        /// </value>
        public Visibility ResizingThumbVisibility
        {
            get { return (Visibility)GetValue(ResizingThumbVisibilityProperty); }
            set { SetValue(ResizingThumbVisibilityProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ResizingThumbVisibility dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ResizingThumbVisibility dependency property.
        /// </remarks>
        public static readonly DependencyProperty ResizingThumbVisibilityProperty =
            GridDependencyProperty.Register("ResizingThumbVisibility", typeof(Visibility), typeof(SfMultiColumnDropDownControl), new GridPropertyMetadata(Visibility.Visible));

#endregion

#region SfDataGrid Properties

        /// <summary>
        /// Gets or sets a value that indicates whether the columns are created automatically.
        /// </summary>
        /// <value> 
        /// <b>true</b> if the columns are automatically generated; otherwise, <b>false</b>. The default value is <b>true</b>.
        /// </value>  
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumns"/>
        public bool AutoGenerateColumns
        {
            get { return (bool)GetValue(AutoGenerateColumnsProperty); }
            set { SetValue(AutoGenerateColumnsProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AutoGenerateColumns dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AutoGenerateColumns dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoGenerateColumnsProperty =
            DependencyProperty.Register("AutoGenerateColumns", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(true));

        /// <summary>
        /// Gets or sets a value that indicates how the columns are generated when AutoGenerateColumns is enabled.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoGenerateColumnsMode"/> enumeration that specifies the mode of automatic column generation. The default value is <b>Syncfusion.UI.Xaml.SfDataGrid.AutoGenerateColumnsMode.None</b>.
        /// </value> 
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.AutoGenerateColumnsMode"/>
        public AutoGenerateColumnsMode AutoGenerateColumnsMode
        {
            get { return (AutoGenerateColumnsMode)GetValue(AutoGenerateColumnsModeProperty); }
            set { SetValue(AutoGenerateColumnsModeProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AutoGenerateColumnsMode dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.AutoGenerateColumnsMode dependency property.
        /// </remarks>
        public static readonly DependencyProperty AutoGenerateColumnsModeProperty =
            DependencyProperty.Register("AutoGenerateColumnsMode", typeof(AutoGenerateColumnsMode), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(AutoGenerateColumnsMode.Reset));

        /// <summary>
        /// Gets or sets a value that indicates how the columns widths are determined in SfDataGrid which is present in the pop-up.
        /// </summary>       
        /// <value>
        /// One of the enumeration in <see cref="Syncfusion.UI.Xaml.Grid.GridLengthUnitType"/>that adjust the column width. The default value is
        /// <b>Syncfusion.UI.Xaml.Grid.GridLengthUnitType.None</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.GridColumnSizer"/>
        public GridLengthUnitType GridColumnSizer
        {
            get { return (GridLengthUnitType)GetValue(GridColumnSizerProperty); }
            set { SetValue(GridColumnSizerProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.GridColumnSizer dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.GridColumnSizer dependency property.
        /// </remarks>
        public static readonly DependencyProperty GridColumnSizerProperty =
            DependencyProperty.Register("GridColumnSizer", typeof(GridLengthUnitType), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(GridLengthUnitType.None));

        /// <summary>
        /// Gets or sets the collection that is used to generate the content of SfDataGrid which is present in the pop-up.
        /// </summary>
        /// <value>
        /// The collection that is used to generate the content of SfDataGrid in pop-up.
        /// The default value is <b>null</b>.
        /// </value>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.ItemsSource"/>
        public object ItemsSource
        {
            get { return GetValue(ItemsSourceProperty); }
            set { SetValue(ItemsSourceProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ItemsSource dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ItemsSource dependency property.
        /// </remarks>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register("ItemsSource", typeof(object), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null, OnItemsSourceChanged));

#endregion

#region Customization Properties

        /// <summary>
        /// Gets or sets the border brush to the pop-up.
        /// </summary>
        /// <value>
        /// The pop-up border brush.
        /// </value>
        public Brush PopupBorderBrush
        {
            get { return (Brush)GetValue(PopupBorderBrushProperty); }
            set { SetValue(PopupBorderBrushProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBorderBrush dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBorderBrush dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupBorderBrushProperty =
            DependencyProperty.Register("PopupBorderBrush", typeof(Brush), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        /// <summary>
        /// Gets or sets the border thickness to the pop-up.
        /// </summary>
        /// <value>
        /// The pop-up border thickness.
        /// </value>
        public Thickness PopupBorderThickness
        {
            get { return (Thickness)GetValue(PopupBorderThicknessProperty); }
            set { SetValue(PopupBorderThicknessProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBorderThickness dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupBorderThickness dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupBorderThicknessProperty =
            DependencyProperty.Register("PopupBorderThickness", typeof(Thickness), typeof(SfMultiColumnDropDownControl),
                                        new PropertyMetadata(new Thickness(1)));
#endregion

#region Pop-up Properties

        /// <summary>
        /// Gets or sets the maximum height constraint of the pop-up.
        /// </summary>
        /// <value>
        /// The maximum height of the pop-up.
        /// </value>
        /// <remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupHeight"/>
        /// </remarks>
        public double PopupMaxHeight
        {
            get { return (double)GetValue(PopupMaxHeightProperty); }
            set { SetValue(PopupMaxHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopUpMaxHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopUpMaxHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupMaxHeightProperty =
            DependencyProperty.Register("PopupMaxHeight", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(double.MaxValue));

        /// <summary>
        /// Gets or sets the maximum width constraint of the pop-up.
        /// </summary>
        /// <value>
        /// The maximum width of the pop-up.
        /// </value>
        /// <remarks>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupWidth"/>
        /// </remarks>
        public double PopupMaxWidth
        {
            get { return (double)GetValue(PopupMaxWidthProperty); }
            set { SetValue(PopupMaxWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMaxWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMaxWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupMaxWidthProperty =
            DependencyProperty.Register("PopupMaxWidth", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(double.MaxValue));

        /// <summary>
        /// Gets or sets the minimum height constraint of the pop-up.
        /// </summary>
        /// <value>
        /// The minimum height of the pop-up. The default value is <b>300.0</b>.
        /// </value>
        public double PopupMinHeight
        {
            get { return (double)GetValue(PopupMinHeightProperty); }
            set { SetValue(PopupMinHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinHeight dependency property.
        /// </remarks>
        public static DependencyProperty PopupMinHeightProperty =
            DependencyProperty.Register("PopupMinHeight", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(DefaultPopupMinHeight));

        /// <summary>
        /// Gets or sets the minimum width constraint of the pop-up.
        /// </summary>
        /// <value>
        /// The minimum width of the pop-up. The default value is <b>200.0</b>.
        /// </value>
        public double PopupMinWidth
        {
            get { return (double)GetValue(PopupMinWidthProperty); }
            set { SetValue(PopupMinWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinWidth dependency property.
        /// </remarks>
        public static DependencyProperty PopupMinWidthProperty =
            DependencyProperty.Register("PopupMinWidth", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(DefaultPopupMinWidth - 200));

        /// <summary>
        /// Gets or sets the height of pop-up.
        /// </summary>
        /// <value>
        /// The height of the pop-up. The <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinHeight"/> is set as the default height of the pop-up.
        /// </value>
        /// <remarks>
        /// When <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize"/> is true, the given pop-up height won't considered. Pop-up height will be calculated based on number of rows in a pop-up.
        /// When <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize"/> is false, then the given Pop-up height will be considered.
        /// if the given pop-up height is greater than the available height, then the pop-up height will set based on the available maximum height by considering top and bottom side of the window.
        /// if the given pop-up height is less than the PopupMinHeight, then <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinHeight"/> will set as Pop-up height by not considering <see cref="Syncfusion.UI.Xaml.Grid.GridMultiColumnDropDownList.IsAutoPopupSize"/>.
        /// </remarks>
        public double PopupHeight
        {
            get { return (double)GetValue(PopupHeightProperty); }
            set { SetValue(PopupHeightProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupHeightProperty =
            DependencyProperty.Register("PopupHeight", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(DefaultPopupMinHeight));

        /// <summary>
        /// Gets or sets the width of the pop-up.
        /// </summary>
        /// <value>
        /// The width of the pop-up. The <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinWidth"/> is set as the default width of the pop-up.
        /// </value>
        /// <remarks>
        /// When <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize"/> is true, the given Pop-up Width won't considered. Pop-up width will be calculated based on the number of columns in a SfDataGrid.
        /// When <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize"/> is false, then the given Pop-up width will be considered.
        /// if the given pop-up width is greater than the available width, then the pop-up width will be set based on the available maximum width by considering left and right side of the window.
        /// if the given pop-up width is less than the PopupMinWidth, then <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupMinWidth"/> will set as Pop-up Width by not considering <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.IsAutoPopupSize"/>.
        /// </remarks>
        public double PopupWidth
        {
            get { return (double)GetValue(PopupWidthProperty); }
            set { SetValue(PopupWidthProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty PopupWidthProperty =
            DependencyProperty.Register("PopupWidth", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(DefaultPopupMinWidth));

#endregion

#region Static property

        /// <summary>
        /// Attached property To Skip Cell validation 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>

        public static bool GetSkipValidation(DependencyObject obj)
        {
            return (bool)obj.GetValue(SkipValidationProperty);
        }

        public static void SetSkipValidation(DependencyObject obj, bool value)
        {
            obj.SetValue(SkipValidationProperty, value);
        }

        public static readonly DependencyProperty SkipValidationProperty =
            DependencyProperty.RegisterAttached("SkipValidation", typeof(bool), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(false));

#endregion

#endregion

#region Public Events

        /// <summary>
        /// Occurs when the Pop-up is Opening.
        /// </summary>
        /// <remarks>
        /// Pop-up opening can be skipped through <see cref="Syncfusion.UI.Xaml.Grid.PopupOpeningEventArgs"/> event argument.
        /// </remarks>
        public event PopupOpeningEventHandler PopupOpening;

        /// <summary>
        /// Occurs when the Pop-up is Opened. 
        /// </summary>
        public event PopupOpenedEventHandler PopupOpened;

        /// <summary>
        /// Occurs when the Pop-up is Closing. 
        /// </summary>
        /// Pop-up closing can be skipped through <see cref="Syncfusion.UI.Xaml.Grid.PopupClosingEventArgs"/> event argument.
        public event PopupClosingEventHandler PopupClosing;

        /// <summary>
        /// Occurs when the Pop-up is Closed. 
        /// </summary>
        public event PopupClosedEventHandler PopupClosed;

        /// <summary>
        /// Occurs when the Selection is Changed. 
        /// </summary>
        /// <remarks>
        /// Selected index and Selected item can be accessed through <see cref="Syncfusion.UI.Xaml.Grid.SelectionChangedEventArgs"/> event argument.
        /// </remarks>
        public event SelectionChangedEventHandler SelectionChanged;

#endregion

#region Events Helper Methods
        private bool RaisePopupOpeningEvent(PopupOpeningEventArgs e)
        {
            if (PopupOpening != null)
            {
                PopupOpening(this, e);
                return e.Cancel;
            }
            return false;
        }

        private void RaisePopupOpenedEvent(PopupOpenedEventArgs e)
        {
            if (PopupOpened != null)
            {
                PopupOpened(this, e);
            }
        }

        private bool RaisePopupClosingEvent(PopupClosingEventArgs e)
        {
            if (PopupClosing != null)
            {
                PopupClosing(this, e);
                return e.Cancel;
            }
            return false;
        }

        private void RaisePopupClosedEvent(PopupClosedEventArgs e)
        {
            if (PopupClosed != null)
            {
                PopupClosed(this, e);
            }
        }

        private void RaiseSelectionChangedEvent(SelectionChangedEventArgs e)
        {
            if (SelectionChanged != null)
            {
                SelectionChanged(this, e);
            }
        }
#endregion

#region Dependency CallBack

        /// <summary>
        /// Called when columns changed
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" />instance containing the event data</param>
        private static void OnColumnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfmultiColumnDropDownControl = d as SfMultiColumnDropDownControl;
            if (sfmultiColumnDropDownControl.InternalGrid != null)
                sfmultiColumnDropDownControl.InternalGrid.Columns = sfmultiColumnDropDownControl.Columns;
        }

        /// <summary>
        /// Called when ItemsSource is changed.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnControl = (SfMultiColumnDropDownControl)d;
            sfMultiColumnControl.isInTextChange = true;
            sfMultiColumnControl.Text = string.Empty;
            sfMultiColumnControl.isInTextChange = false;
            //WPF-36876 Not able to convert DataTable as IEnumerable so that Append Source null for the DataTable collection
#if WPF
            sfMultiColumnControl.appendSource =
                          ((e.NewValue is DataTable) || (e.NewValue is DataView)) ?
                           (e.NewValue as IList).ToList<object>() : e.NewValue as IEnumerable<object>;
#else
            sfMultiColumnControl.appendSource = e.NewValue as IEnumerable<object>;
#endif
        }

        /// <summary>
        /// Called when [selected value changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private static void OnSelectedValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDown = (SfMultiColumnDropDownControl)d;

            if (sfMultiColumnDropDown.InternalGrid == null || sfMultiColumnDropDown.InternalGrid.View == null)
            {
                sfMultiColumnDropDown.isSelectedValueLoadedBeforeGridLoaded = true;
                return;
            }
          
            if (sfMultiColumnDropDown.IsInSuspend)
                return;

            if (e.NewValue == null)
            {
                // Reset the Selected Index and Selected Item to its default value when Seleceted value is null.
                sfMultiColumnDropDown.SelectedItem = null;
                sfMultiColumnDropDown.SelectedIndex = -1;
                return;
            }

            object record;
            if (!string.IsNullOrEmpty(sfMultiColumnDropDown.ValueMember))
            {
                record = sfMultiColumnDropDown.appendSource.FirstOrDefault
                    (o => (sfMultiColumnDropDown.GetSelectedValue(o)) != null &&
                            (sfMultiColumnDropDown.GetSelectedValue(o).Equals(e.NewValue)));
            }
            else
                record = sfMultiColumnDropDown.appendSource.FirstOrDefault(o => (o.Equals(e.NewValue)));

            sfMultiColumnDropDown.ProcessSelecteValue(record);
        }

        /// <summary>
        /// Called When [Selected Item changed]
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The  <see cref="DependencyPropertyChangedEventArgs"/> instance containing event data</param>
        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDown = (SfMultiColumnDropDownControl)d;

            if (sfMultiColumnDropDown.InternalGrid == null || sfMultiColumnDropDown.InternalGrid.View == null)
            {
                sfMultiColumnDropDown.isSelectedItemLoadedBeforeGridLoaded = true;
                return;
            }

            if (sfMultiColumnDropDown.IsInSuspend)
                return;

            sfMultiColumnDropDown.ProcessSelectedItem(e.NewValue);
        }

        /// <summary>
        /// Called When [Selected Index changed]
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The  <see cref="DependencyPropertyChangedEventArgs"/> instance containing event data</param>
        private static void OnSelectedIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDown = ((SfMultiColumnDropDownControl)d);
            var internalGrid = sfMultiColumnDropDown.InternalGrid;

            if (internalGrid == null || internalGrid.View == null)
            {
                sfMultiColumnDropDown.isSelectedIndexLoadedBeforeGridLoaded = true;
                return;
            }

            if (sfMultiColumnDropDown.IsInSuspend)
                return;

            sfMultiColumnDropDown.ProcessSelectedIndex((int)e.NewValue);
        }

        /// <summary>
        /// Called when [is drop down open changed].
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        private static void OnIsDropDownOpenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var sfMultiColumnDropDown = (SfMultiColumnDropDownControl)d;
            if ((bool)e.NewValue)
            {
                var isCancel = sfMultiColumnDropDown.RaisePopupOpeningEvent(new PopupOpeningEventArgs());
                sfMultiColumnDropDown.IsDropDownOpen = !isCancel;
                if (!isCancel)
                {
                    sfMultiColumnDropDown.RaisePopupOpenedEvent(new PopupOpenedEventArgs());
                    sfMultiColumnDropDown.ScrollInView();
                }
            }
            else
            {
                //sfMultiColumnDropDown.CommitValue();
                sfMultiColumnDropDown.ClearFilter();
                var isCancel = sfMultiColumnDropDown.RaisePopupClosingEvent(new PopupClosingEventArgs());
                sfMultiColumnDropDown.IsDropDownOpen = isCancel;
                if (!isCancel)
                    sfMultiColumnDropDown.RaisePopupClosedEvent(new PopupClosedEventArgs());
            }
        }

        /// <summary>
        /// Calculates the remaining height of the window whether in top or bottom.
        /// </summary>
        /// <param name="loadBottom">if it is true, calculate the remaining bottom height else calculate the remaining top height.</param>
        /// <returns>calcualted remaining window height.</returns>
        private double GetRemainingWindowHeight(bool loadBottom = true)
        {
#if WPF
            var targetPoints = this.PointToScreen(new Point(0, 0));
            //WPF-32277-The pop up opening position is changed for the various screen resolution.To avoid this we need to convert fetched system coordinates value by TrnsformFromDevice method.
            PresentationSource source = PresentationSource.FromVisual(this);
            Point locationfromScreen = source.CompositionTarget.TransformFromDevice.Transform(targetPoints);
            var ypos = locationfromScreen.Y;
            if(loadBottom)            
                return SystemParameters.WorkArea.Height - ypos;
          
            return ypos;
#else
            var windowHeight = Window.Current.Bounds.Height;
            //locationfromScreen - denotes where the control is loaded with X, Y axis of the window.         
            var locationfromScreen = this.TransformToVisual(null).TransformPoint(new Point(0, 0));
            var ypos = locationfromScreen.Y;
            if (loadBottom)
                return windowHeight - ypos;

            return ypos;
#endif
        }

        /// <summary>
        /// Calculates the remaining width of the window whether in right or left side.
        /// </summary>
        /// <param name="isRight">if it is true, calculate the remaining right side width else calculate the remaining left side width.</param>
        /// <returns>calcualted remaining window width.</returns>
        private double GetRemainingWindowWidth(bool isRight = true)
        {
#if WPF
            var targetPoints = this.PointToScreen(new Point(0, 0));
            //WPF-32277-The pop up opening position is changed for the various screen resolution.To avoid this we need to convert fetched system coordinates value by TrnsformFromDevice method.
            PresentationSource source = PresentationSource.FromVisual(this);
            Point locationfromScreen = source.CompositionTarget.TransformFromDevice.Transform(targetPoints);
            var xpos = locationfromScreen.X;
            if (isRight)
                return SystemParameters.WorkArea.Width - xpos;

            return xpos;
#else
            var windowWidth = Window.Current.Bounds.Width;
            //locationfromScreen - denotes where the control is loaded with X, Y axis of the window.         
            var locationfromScreen = this.TransformToVisual(null).TransformPoint(new Point(0, 0));
            var xpos = locationfromScreen.X;
            if (isRight)
                return windowWidth - xpos;

            return xpos;
#endif
        }

#endregion

#region Ctor

        /// <summary>
        /// Initializes a new instance of the <see cref="SfMultiColumnDropDownControl"/> class.
        /// </summary>
        public SfMultiColumnDropDownControl()
        {
#if UWP
            base.DefaultStyleKey = typeof(SfMultiColumnDropDownControl);
#else
            SetValue(ColumnsProperty, new Columns());
#endif
        }

        /// <summary>
        /// Initializes the <see cref="SfMultiColumnDropDownControl"/> class.
        /// </summary>
        static SfMultiColumnDropDownControl()
        {
#if WPF
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SfMultiColumnDropDownControl), new FrameworkPropertyMetadata(typeof(SfMultiColumnDropDownControl)));
#endif
        }
#endregion

#region Virtual Methods

        /// <summary>
        /// Occurs when a key is pressed when the control has focus.
        /// </summary>
        /// <param name="args">The event data</param>
        protected virtual void ProcessKeyDown(KeyEventArgs args)
        {
#if WPF
            var isCtrlKey = (args.KeyboardDevice.Modifiers & ModifierKeys.Control) != ModifierKeys.None;
            var isAltKey = (args.KeyboardDevice.Modifiers & ModifierKeys.Alt) != ModifierKeys.None;
            var isShiftKey = (args.KeyboardDevice.Modifiers & ModifierKeys.Shift) != ModifierKeys.None;
            if (isAltKey && args.SystemKey == Key.Down || isAltKey && args.SystemKey == Key.Up || args.Key == Key.F4)
#else
            var Alt = Window.Current.CoreWindow.GetAsyncKeyState(Key.Menu);
            var isAltKey = Alt.HasFlag(CoreVirtualKeyStates.Down);
            var Shift = Window.Current.CoreWindow.GetAsyncKeyState(Key.Shift);
            var isShiftKey = Shift.HasFlag(CoreVirtualKeyStates.Down);
            if (isAltKey && args.Key == Key.Down || isAltKey && args.Key == Key.Up || args.Key == Key.F4)
#endif
            {
                IsDropDownOpen = !IsDropDownOpen;
                ProcessEditorFocus();
                args.Handled = true;
                return;
            }
            //WRT-6472 ( Issue 2) - SelectionStart always zero while pressing home key and TextBox not having CaretIndex property in WinRT.
            //so while pressing Home key from any of the place in Editor when AllowAutoComplete is true, Editor text will be selected.
#if WPF
            if (AllowAutoComplete && Editor != null && Editor.CaretIndex == Editor.Text.Length && (isShiftKey && args.Key == Key.Home || args.Key == Key.Home))
#else
            if (Editor != null && AllowAutoComplete && (isShiftKey && args.Key == Key.Home || args.Key == Key.Home))
#endif
            {
                if (!ReadOnly)
                    Editor.SelectAll();
                args.Handled = true;
            }

            switch (args.Key)
            {
                case Key.Down:
#if WPF
                    if (isCtrlKey || isShiftKey)
#else
                    if (isShiftKey)
#endif
                        break;
                    //The Below Condition is Added while the SelectedItem is Null and we have select the Value through the InternalGrid Based on the Key operations.
                    if (SelectedIndex >= 0 && SelectedIndex < InternalGrid.ResolveToRecordIndex(InternalGrid.GetLastDataRowIndex()) || SelectedIndex < 0)
                    {
                        if (IsDropDownOpen)
                            IsInSuspend = true;
                        SelectedIndex = GetNextRowIndex();
                        if (IsDropDownOpen)
                            IsInSuspend = false;
                        if (!ReadOnly && Editor != null)
                            Editor.SelectAll();
                        ScrollInView();
                        args.Handled = true;
                    }
                    break;
                case Key.Up:
#if WPF
                    if (isCtrlKey || isShiftKey)
#else
                    if (isShiftKey)
#endif
                        break;

                    if (SelectedIndex > 0 && SelectedIndex <= InternalGrid.ResolveToRecordIndex(InternalGrid.GetLastDataRowIndex()))
                    {
                        if (IsDropDownOpen)
                            IsInSuspend = true;
                        SelectedIndex = GetPreviousRowIndex();
                        if (IsDropDownOpen)
                            IsInSuspend = false;
                        if (!ReadOnly && Editor != null)
                            Editor.SelectAll();
                        ScrollInView();
                        args.Handled = true;
                    }
                    break;
                //In Allow AutoComplete The Left Key is Used for Selection.
                //In Allow AutoComplete The Right-Key is Used for De selection.
                case Key.Left:
                case Key.Right:
#if WPF
                    if (!AllowAutoComplete || isCtrlKey || isShiftKey)
                        break;

                    if (Editor != null && Editor.Text.Length > 0)
                    {
                        if (Editor.SelectionStart + Editor.SelectionLength > Editor.Text.Length - 1)
                        {
                            if (args.Key == Key.Left)
                            {
                                if (Editor.CaretIndex == Editor.Text.Length || (Editor.SelectedText.Length > 0 && Editor.SelectionStart > 0 && Editor.CaretIndex > 0))
                                    Editor.Select(Editor.CaretIndex - 1, Editor.Text.Length);
                            }
                            else
                            {
                                if (Editor.SelectedText.Length > 0)
                                    Editor.Select(Editor.CaretIndex + 1, Editor.SelectedText.Length);
                            }
                            args.Handled = true;
                        }
                    }
#endif
                    //WRT-4225 For WinRt and Silverlight we can't able to get the selected text so while pressing left navigation key we break the process
                    break;
                case Key.PageDown:
                case Key.PageUp:
                    if (IsDropDownOpen)
                        IsInSuspend = true;
                    InternalGrid.SelectionController.HandleKeyDown(args);
                    if (IsDropDownOpen)
                        IsInSuspend = false;
                    args.Handled = true;
                    break;
                case Key.Escape:
                    if (IsDropDownOpen)
                        IsDropDownOpen = false;
                    ProcessEditorFocus();
                    break;
                case Key.Tab:
                case Key.Enter:
                    //In read only state, the pop up will open while pressing Enter key.
                    if (ReadOnly && !IsDropDownOpen && args.Key == Key.Enter)
                    {
                        IsDropDownOpen = !IsDropDownOpen;
                        ProcessEditorFocus();
                        args.Handled = true;
                        return;
                    }
                    if (args.Key == Key.Enter || (args.Key == Key.Tab && AllowAutoComplete))
                    {
                        //WRT-6081 To update the SelectedValue in OnSelectedItemChanged handler                            
                        isInTextChange = false;
                        SelectedItem = InternalGrid.SelectedItem;
                    }
                    CommitValue(true);
                    IsDropDownOpen = false;
                    ProcessEditorFocus();
                    if (args.Key == Key.Enter)
                        args.Handled = true;
                    break;
                case Key.Back:
                case Key.Delete:
                    if (ReadOnly)
                        break;

                    if (AllowAutoComplete)
                    {
#if WPF
                        if (args.Key == Key.Back && (Editor.CaretIndex - 1 > 0))
#else
                        if (args.Key == Key.Back && Editor.SelectionStart - 1 >= 0)
#endif
                        {
#if WPF
                            if ((Editor.SelectedText.Length > 0) && Editor.SelectionStart > 0)
#endif
                            {
#if WPF
                                Editor.Select(Editor.CaretIndex - 1, Editor.Text.Length);
                                SearchText = Editor.Text.Substring(0, (Editor.Text.Length - Editor.SelectedText.Length));
#else
                                //WRT-5050 Based on the SearchText length, we can find the whether the text is selected or not while pressing backspace.
                                if (InternalGrid.SelectedItem != null && Editor.Text.Length != SearchText.Length - 1)
                                    Editor.Select(Editor.SelectionStart - 1, Editor.Text.Length);
#endif
                                args.Handled = true;
                            }
                        }
                        else if (Editor.SelectionStart == (Editor.Text.Length - Editor.SelectedText.Length))
                        {
#if WPF
                            if (Editor.CaretIndex <= 1)
#else
                            if (Editor.SelectionStart <= 1)
#endif
                                //ClearSelection();
                                Editor.SelectAll();
                        }
                    }
                    if (AllowIncrementalFiltering)
                        ProcessOnEditorTextChanged(Editor, null);
                    break;
                case Key.Space:
                    if (!ReadOnly)
                        break;
                    //In read only state, the pop up will open while pressing Space key.
                    if (!IsDropDownOpen)
                    {
                        IsDropDownOpen = true;
                        ProcessEditorFocus();
                    }
                    else
                    {
                        SelectedItem = InternalGrid.SelectedItem;
                        CommitValue(true);
                        IsDropDownOpen = false;
                        ProcessEditorFocus();
                    }
                    break;
                default:
                    if (AllowImmediatePopup)
                    {
#if WPF
                        if (!(args.Key == Key.Home || args.Key == Key.End || args.Key == Key.LeftAlt || args.Key == Key.LeftCtrl || args.Key == Key.LeftShift || args.Key == Key.RightAlt || args.Key == Key.RightCtrl || args.Key == Key.RightShift || args.Key == Key.System))
#else
                        if (!(args.Key == Key.Home || args.Key == Key.End || args.Key == Key.Menu || args.Key == Key.Control || args.Key == Key.Shift))
#endif
                        {
                            IsDropDownOpen = true;
#if !WPF
                            ProcessEditorFocus();
#endif
                        }
                    }
                    if (ReadOnly && IsDropDownOpen && ((args.Key == Key.Home) || args.Key == Key.End))
                    {
                        InternalGrid.SelectionController.HandleKeyDown(args);
                        args.Handled = true;
                    }
                    break;
            }
        }

        /// <summary>
        /// Set the text to the Textbox.
        /// </summary>
        /// <param name="selectedItem">Selected item of the SfDataGrid. </param>
        protected virtual void SetDisplayText(object selectedItem = null)
        {
            selectedItem = selectedItem == null ?
                            (InternalGrid.SelectedItem != null ? InternalGrid.SelectedItem : null)
                            : selectedItem;

            if (selectedItem == null)
            {
                if (string.IsNullOrEmpty(SearchText))
                    Text = string.Empty;
                return;
            }

            if (!string.IsNullOrEmpty(DisplayMember))
            {
                var displayValue = GetDisplayValue(selectedItem);
                Text = displayValue == null ? string.Empty : displayValue.ToString();
            }
            else
                Text = selectedItem.ToString();
        }

        /// <summary>
        /// Set the selected value to the SfMultiColumnDropDownControl based on the selected item.
        /// </summary>
        /// <param name="selectedItem">SelectedItem of the SfDataGrid.</param>
        protected virtual void SetSelectedValue(object selectedItem)
        {
            if (selectedItem == null)
            {
                SelectedValue = null;
                return;
            }

            if (!string.IsNullOrEmpty(ValueMember))
            {
                var value = GetSelectedValue(selectedItem);
                if ((SelectedValue == null && value != null) || (SelectedValue != null && !SelectedValue.Equals(value)))
                    SelectedValue = value;
            }
            else
            {
                if (SelectedValue != selectedItem)
                    SelectedValue = selectedItem;
            }
        }

        /// <summary>
        /// Commit the given value in the editor.
        /// </summary>
        /// <param name="autoCommit"> If it is true, set the selected item directly otherwise set the selected item based on DisplayMember.</param>
        protected virtual void CommitValue(bool autoCommit = false)
        {
            object _selectedItem = null;

            bool canRaiseSelectionChanged = PreviousSelectedItem != null
                ? !PreviousSelectedItem.Equals(InternalGrid.SelectedItem)
                : PreviousSelectedItem != InternalGrid.SelectedItem;

            if (autoCommit)
            {
                PreviousSelectedItem = InternalGrid.SelectedItem;
                _selectedItem = InternalGrid.SelectedItem;
            }
            else
            {
                var displayText = string.Empty;
                if (!string.IsNullOrEmpty(DisplayMember) && InternalGrid.SelectedItem != null)
                {
                    var displayValue = GetDisplayValue(InternalGrid.SelectedItem);
                    displayText = displayValue == null ? string.Empty : displayValue.ToString();
                }

                _selectedItem = string.Equals(displayText, Editor.Text)
                                        ? InternalGrid.SelectedItem
                                        : PreviousSelectedItem;
            }
            if (_selectedItem != null)
            {
#if !WPF
                if (AllowNullInput && (Editor != null && string.IsNullOrEmpty(Editor.Text)))
#else
                if (AllowNullInput && string.IsNullOrEmpty(Editor.Text))
#endif
                {
                    InternalGrid.SelectedItems.Clear();
                    SelectedItem = null;
                    ///When Setting Allow Null Input is true The Internal Grid Maintains the Selected Value 
                    ///Until the Internal Grid.Selected Items Getting clear.
                    if (PreviousSelectedItem != SelectedItem)
                    {
                        canRaiseSelectionChanged = true;
                        PreviousSelectedItem = SelectedItem;
                    }
                    ScrollInView();
                    isInTextChange = true;
                    Text = string.Empty;
                    isInTextChange = false;
                }
                else
                {
                    SelectedItem = _selectedItem;
                    if (!autoCommit)
                        PreviousSelectedItem = SelectedItem;
                    isInTextChange = true;
                    SetDisplayText(SelectedItem);
                    isInTextChange = false;
                }
                if (canRaiseSelectionChanged)
                    RaiseSelectionChangedEvent(new SelectionChangedEventArgs
                    {
                        SelectedIndex = InternalGrid.SelectedIndex,
                        SelectedItem = SelectedItem
                    });
            }
            else
                ClearSelection();

            SearchText = string.Empty;
        }

        /// <summary>
        /// Checks whether the given exact value is starts with the filter text.
        /// </summary>
        /// <param name="item">The record</param>
        /// <param name="exactValue">The original value</param>
        /// <param name="filterText">The entered text in the editor.</param>
        /// <returns>
        /// <c>true</c> if the exactValue is begins with filterText; otherwise, <c>false</c>.</returns>
        protected virtual bool ProcessAppendText(object item, string exactValue, string filterText)
        {
#if WPF
            if (string.IsNullOrEmpty(filterText))
                return false;
#endif

            return exactValue.StartsWith(filterText);
        }

        /// <summary>
        /// Returns the new string list for the given value.
        /// </summary>
        /// <param name="item"> The record</param>
        /// <param name="_value"> New string list will be created based on the value.</param>
        /// <returns>Created string list</returns>
        protected virtual List<string> ProcessAppendStringList(object item, string _value)
        {
            var _newList = new List<string> { _value };
            return _newList;
        }

        /// <summary>
        /// Processes the immediate filtering when typing the text in editor.
        /// </summary>
        protected virtual void ProcessIncrementalFiltering()
        {
            if (AllowIncrementalFiltering && InternalGrid.VisualContainer != null)
            {
                IsInSuspend = true;
                InternalGrid.View.Filter += FilterRecord;
                InternalGrid.View.RefreshFilter();
                InternalGrid.SelectionController.HandleGridOperations(new GridOperationsHandlerArgs(GridOperation.Filtering, new GridFilteringEventArgs(IsDropDownOpen)));
                FilteredItems = InternalGrid.View.Records;
                IsInSuspend = false;
            }
        }

        /// <summary>
        /// Returns true if the given item contains in record lists, else returns FALSE
        /// </summary>
        /// <param name="item">The item to be filter.</param>
        /// <returns>
        /// <c>true</c> if the item is filtered; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// The filtering operation are done based on <cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.SearchCondition"/>.
        /// </remarks>
        protected virtual bool FilterRecord(object item)
        {
            if (item == null || DisplayMember == null)
                return false;

            var displayValue = GetDisplayValue(item);

            if (displayValue == null)
                return false;

            var exactValue = displayValue.ToString();
            if (!AllowCaseSensitiveFiltering)
                exactValue = exactValue.ToLower();

            if (SearchCondition == SearchCondition.StartsWith)
                return exactValue.StartsWith(filterText);
            else if (SearchCondition == SearchCondition.Contains)
                return exactValue.Contains(filterText);
            return exactValue.Equals(filterText);
        }

#endregion

#region Public Methods

        /// <summary>
        /// Returns the Embedded DropDown DataGrid in the Pop-up.
        /// </summary>
        /// <returns>SfDataGrid which is loaded in pop-up.</returns>
        /// <remarks></remarks>
        public SfDataGrid GetDropDownGrid()
        {
            return InternalGrid;
        }
#endregion

#region Override Methods

        /// <summary>
        /// Builds the visual tree for the SfMultiColumnDropDownControl when a new template is applied.
        /// </summary>
#if !WPF
        protected override void OnApplyTemplate()
#else
        public override void OnApplyTemplate()
#endif
        {
#if !WPF
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                DefaultPopupMinWidth = 200;
            }
#endif
            base.OnApplyTemplate();
            UnWireEvents();
            UnWireEditorEvents();
#if !WPF
            UnWireReadOnlyEvents();
            PopupBorder = GetTemplateChild("PART_PopupBorder") as Border;
            if (ReadOnly)
                ContentControl = GetTemplateChild("PART_Control") as ContentControl;
            else
#endif
            Editor = GetTemplateChild("PART_TextBox") as TextBox;
            DropDownButton = GetTemplateChild("PART_ToggleButton") as ToggleButton;
            InternalPopup = GetTemplateChild("PART_Popup") as Popup;
            InternalGrid = GetTemplateChild("PART_SfDataGrid") as SfDataGrid;
            ResizeThumb = GetTemplateChild("PART_ThumbGripper") as Thumb;
            WireEvents();
#if WPF
            if(Editor != null)
                WireEditorEvents();
#else
            if (!ReadOnly && Editor != null)
                WireEditorEvents();
            else
                WireReadOnlyEvents();
            IsLoaded = true;
#endif
            if (InternalGrid != null)
            {
                SetSkipValidation(InternalGrid, true);
                ProcessInitialization();
            }
        }

        /// <summary>
        /// Arranges the content of the pop-up.
        /// </summary>
        /// <param name="arrangeBounds">
        /// The computed size that is used to arrange the content in pop-up.
        /// </param>
        /// <returns>
        /// The size consumed by the content in pop-up.
        /// </returns>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            if (DefaultPopupMinWidth < arrangeBounds.Width)
                PopupMinWidth = arrangeBounds.Width;
            return base.ArrangeOverride(arrangeBounds);
        }

        /// <summary>
        /// Calculate the pop-up height and Width when IsAutoPopupSize is enabled.
        /// </summary>
        /// <remarks>
        /// Based on maximum available height in Top and bottom of the window from control, the pop-up height will be calculated.
        /// Likewise, pop-up width also calculated based on available width in left and right of the control.
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupHeight"/>
        /// <seealso cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupWidth"/>
        /// </remarks>
        protected virtual void DropDownAutoSize()
        {
            var gridWidth = this.InternalGrid.VisualContainer.ScrollColumns.ScrollBar.Maximum;
            var gridHeight = this.InternalGrid.VisualContainer.ScrollRows.ScrollBar.Maximum;
            //remaining bottom height of the window.
            var remainWinHeight_bottom = GetRemainingWindowHeight();
            //remaining right side width of the window.
            var remainWinWidth_right = GetRemainingWindowWidth();

#if !WPF
            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                if (gridWidth > remainWinWidth_right)
                    this.PopupWidth = remainWinWidth_right - 10;
                else
                    this.PopupWidth = gridWidth;
            }
            else
            {
#endif
                //PopupWidth sets as DefaultPopupMinWidth when the PopupWidth, remaining window window is greater than the DefaultPopupMinWidth.
                if ((gridWidth > DefaultPopupMinWidth && remainWinWidth_right > DefaultPopupMinWidth) ||

                (remainWinWidth_right < DefaultPopupMinWidth && gridWidth > DefaultPopupMinWidth)
                
                )
                this.PopupWidth = DefaultPopupMinWidth;
            else if (gridWidth < remainWinWidth_right)
                this.PopupWidth = gridWidth;
            else
                this.PopupWidth = remainWinWidth_right;
#if !WPF
            }
#endif
            if (this.PopupWidth < this.ActualWidth)
                this.PopupWidth = this.ActualWidth;

            //PopupHeight sets as DefaultPopupMinHeight when the PopupHeight, remaining window height is greater than the DefaultPopupMinHeight.
            if ((gridHeight > DefaultPopupMinHeight && remainWinHeight_bottom > DefaultPopupMinHeight) ||
                
                (remainWinHeight_bottom < DefaultPopupMinHeight && gridHeight > DefaultPopupMinHeight)

                )
                this.PopupHeight = DefaultPopupMinHeight;
            else if (gridHeight < remainWinHeight_bottom)
                this.PopupHeight = gridHeight;
            else
                this.PopupHeight = remainWinHeight_bottom;
        }

#if WPF
        /// <summary>Pop-up positions ( Horizontal and Vertical offsets) are calculated based on Pop-up height and width.</summary>
        /// <remarks>calculate the pop-up position based on the height of the window and the location of the control in window.
        /// if AutoPopupSize is enabled, then PopupVerticalOffset is calculated based on remaining height of the editor in both top and bottom.
        /// </remarks>
#else
        /// <summary>Pop-up positions ( Horizontal and Vertical offsets) are calculated based on Pop-up height and width.</summary>
        /// <remarks>calculate the pop-up position based on the height of the window, location of the control in window. and its mode ( Whether it is Editable or Non-Editable).
        /// In Editable mode,Pop-up will be loaded in below the editor, if the pop-up height is greater than remaining below height of the window ( below space is not enough to load the pop-up,
        /// then the pop-up will open in above the editor.
        /// In Non-Editable, ContentControl is loaded instead of TextBox.
        /// In Non-Editable mode, Pop will be loaded behind the editor ( like the behavior of Combo-box ).
        /// if AutoPopupSize is enabled, then the VerticalOffset and Horizontal offset are calculated. .
        /// if AutoPopupSize is not enabled, then the pop-up height will be the DefaultMinHeight and pop-up width will be the DefaultMinWidth.
        /// if the IsAutoPopupSize is false, then the HorizontalOffset is -2.
        /// </remarks>
#endif
        protected virtual void ProcessOnPopupPositionCalculation()
        {
            //if the pop-up is resized, then the Height and Widths are re-calculated when the PopupHeight and Width is more than the available width, heights.
            var remainWinHeight_bottom = GetRemainingWindowHeight();
            var remainWinWidth_right = GetRemainingWindowWidth();
            if (remainWinHeight_bottom < this.PopupHeight)
            {
                var remainWinHeight_top = GetRemainingWindowHeight(false);
                //ActualHeight subtracted from remainingwindowheight because the pop-up need to show above the task-bar.
                if (remainWinHeight_top < this.PopupHeight)
                    this.PopupHeight = Math.Max(remainWinHeight_bottom - this.ActualHeight, remainWinHeight_top);
            }

            if (remainWinWidth_right < this.PopupWidth)
            {
                var remainWinWidth_left = GetRemainingWindowWidth(false);
                if (remainWinWidth_left < this.PopupHeight)
                    this.PopupWidth = Math.Max(remainWinWidth_right, remainWinWidth_left);
            }
#if WPF
            var locationfromWindow = this.TranslatePoint(new Point(0, 0), this);
            var targetPoints = this.PointToScreen(locationfromWindow);
            //WPF-32277-The pop up opening position is changed for the various screen resolution.To avoid this we need to convert fetched system coordinates value by TrnsformFromDevice method.
            PresentationSource source = PresentationSource.FromVisual(this);
            Point locationfromScreen = source.CompositionTarget.TransformFromDevice.Transform(targetPoints);
            if (remainWinHeight_bottom > this.PopupHeight)
                this.InternalPopup.VerticalOffset = locationfromScreen.Y + this.ActualHeight - this.BorderThickness.Bottom;
            else
                this.InternalPopup.VerticalOffset = this.PopupHeight < DefaultPopupMinHeight ? locationfromScreen.Y - DefaultPopupMinHeight : locationfromScreen.Y - PopupHeight;

            if (remainWinWidth_right > this.PopupWidth)
                this.InternalPopup.HorizontalOffset = locationfromScreen.X;
            else
            {
                var controlEndPosition = locationfromScreen.X + this.ActualWidth;
                var popupWidth = this.PopupWidth < PopupMinWidth ? PopupMinWidth : PopupWidth;
                this.InternalPopup.HorizontalOffset = controlEndPosition - PopupWidth;
            }
#else
            //windowHeight - height of the window.
            var windowHeight = Window.Current.Bounds.Height;

            //locationfromScreen - denotes where the control is loaded with X, Y axis of the window.         
            var locationfromScreen = this.TransformToVisual(null).TransformPoint(new Point(0, 0));                       

            var actualPopupHeight = this.PopupHeight < DefaultPopupMinHeight ? DefaultPopupMinHeight : this.PopupHeight;

            if (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                //code for UWP_Mobile alone.
                if (this.ReadOnly)
                {
                    if (locationfromScreen.Y < (windowHeight / 2))
                        this.InternalPopup.VerticalOffset = -(locationfromScreen.Y - 5);
                    else if (remainWinHeight_bottom > actualPopupHeight)
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight - 80);
                    else
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight - 12);
                }
                else
                {
                    if (remainWinHeight_bottom >= actualPopupHeight)
                        this.InternalPopup.VerticalOffset = -5;
                    else
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight + 30);
                }
            }
            else
            {
                //values are added with actual-height, because the editor will clip without adding the values.
                if (this.ReadOnly)
                {
                    bool canLoadPopupTop = locationfromScreen.Y < (windowHeight / 3);

                    if (canLoadPopupTop)
                        this.InternalPopup.VerticalOffset = -(locationfromScreen.Y - 5);
                    else if (remainWinHeight_bottom > actualPopupHeight)
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight - 80);
                    else
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight - 15);
                }
                else
                {
                    if (remainWinHeight_bottom >= actualPopupHeight)
                        this.InternalPopup.VerticalOffset = -2;
                    else
                        this.InternalPopup.VerticalOffset = -(actualPopupHeight + this.ActualHeight - 10);
                }
            }

            if (!this.IsAutoPopupSize || !this.ReadOnly)
            {
                //UWP-4520 Change HorizontalOffset of SfMultiColumnDropDownControl Popup based on FlowDirection.
                if ((this.FlowDirection == FlowDirection.LeftToRight && remainWinWidth_right > this.PopupWidth)
                    || (this.FlowDirection == FlowDirection.RightToLeft && remainWinWidth_right < this.PopupWidth))
                    this.InternalPopup.HorizontalOffset = -2;
                else
                {
                    //load the popup left to the control when the remaining window width is less than the PopupWidth.
                    var offSetValue = this.PopupWidth - this.ActualWidth;                
                    this.InternalPopup.HorizontalOffset = -offSetValue;                  
                }
            }
            else
            {
                //windowWidth - height of the window.
                //Here calculation made to load the pop-up as center like MS-Combobox in Readonly mode.
                var windowWidth = Window.Current.Bounds.Width;
                this.InternalPopup.HorizontalOffset = -2;
                //UWP-4520 Change HorizontalOffset of SfMultiColumnDropDownControl Popup based on FlowDirection when Readonly mode is true.
                if (this.PopupWidth - this.ActualWidth > 0)
                {
                    var offSetValue = this.PopupWidth - this.ActualWidth;
                    if (this.FlowDirection == FlowDirection.LeftToRight)
                    {
                        if (locationfromScreen.X > (offSetValue / 2))
                            this.InternalPopup.HorizontalOffset = -(offSetValue / 2);
                        // when the popupwidth is greater than remaining window width from the SfMultiColumnDropDownControl,                         
                        // then we have to reset the horizontaloffset value is offsetvalue value. Otherwise, it will be clipped in view.                     
                        if (this.PopupWidth > remainWinWidth_right)
                            this.InternalPopup.HorizontalOffset = -offSetValue;
                    }
                    else
                    {
                        // when SfMultiColumnDropDownControl position is less than the value of differ between the windowwidth and
                        // half of the offsetvalue, then we have to reset the horizontaloffset based on offsetvalue. Otherwise, set horizontaloffset value is -2 to 
                        // avoid clipping popup in view.
                        if (locationfromScreen.X < (windowWidth - (offSetValue / 2)))
                        {
                            if (locationfromScreen.X > (offSetValue / 2))
                                this.InternalPopup.HorizontalOffset = -(offSetValue / 2);
                            else
                                this.InternalPopup.HorizontalOffset = -offSetValue;
                        }
                    }
                }
            }
#endif
        }



#endregion

#region Private methods

        /// <summary>
        /// Get the value for a particular cell by using record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private object GetSelectedValue(object record)
        {
            if (this.InternalGrid == null || this.InternalGrid.View == null || this.reflector == null)
                return null;
            return this.reflector.GetValue(record, ValueMember);
        }

        /// <summary>
        /// Get the value for a particular cell by using record
        /// </summary>
        /// <param name="record"></param>
        /// <returns></returns>
        private object GetDisplayValue(object record)
        {
            if (this.InternalGrid == null || this.InternalGrid.View == null || this.reflector == null)
                return null;
            return this.reflector.GetValue(record, DisplayMember);
        }

        /// <summary>
        /// Gets the value of cotrol based on editable mode
        /// </summary>
        /// <returns></returns>
        private string GetControlValue()
        {
            return Editor.Text;
        }

        private void ProcessEditorFocus()
        {
#if !WPF
            if (!ReadOnly && Editor != null)
                Editor.Focus(FocusState.Programmatic);
            else if (ReadOnly && ContentControl != null)
                ContentControl.Focus(FocusState.Programmatic);
#else
            if (Editor != null)
            {
                if (!ReadOnly)
                    Editor.SelectAll();
                Editor.Focus();
            }
#endif
            VisualStateManager.GoToState(this, "Focused", false);
        }

        private void ProcessInitialization()
        {
#if !WPF
            InternalPopup.Margin = !double.IsNaN(Height) ? new Thickness(0, Height, 0, 0) : new Thickness(0, MinHeight, 0, 0);
#else
            if (Columns.IsFrozen)
                Columns = (Columns)Columns.Clone();
#endif
            InternalGrid.Columns = Columns;
            InternalGrid.AutoGenerateColumns = this.AutoGenerateColumns;
            InternalGrid.AutoGenerateColumnsMode = this.AutoGenerateColumnsMode;
            InternalGrid.Measure(new Size(PopupWidth, PopupHeight));
        }

        /// <summary>
        /// Clears the selection.
        /// </summary>
        private void ClearSelection()
        {
            IsInSuspend = true;
            SelectedValue = null;
            SelectedItem = null;
            SelectedIndex = -1;
            IsInSuspend = false;

            isInTextChange = true;
            Text = string.Empty;
            isInTextChange = false;
        }

        private void ClearFilter()
        {
            if (InternalGrid == null || InternalGrid.View == null)
                return;

            if (InternalGrid.View.Filter != null)
            {
                InternalGrid.View.Filter = null;
                InternalGrid.View.RefreshFilter();
            }

            FilteredItems = InternalGrid.View.Records;

            if (SelectedItem == null)
                return;

            if (InternalGrid.View.GroupDescriptions.Count > 0)
            {
                var record = InternalGrid.View.Records.GetRecord(SelectedItem);
                InternalGrid.SelectedIndex = InternalGrid.View.TopLevelGroup.DisplayElements.IndexOf(record);
            }
            else
                InternalGrid.SelectedIndex = InternalGrid.View.Records.IndexOfRecord(SelectedItem);
        }

        /// <summary>
        /// Gets the index of the previous row from the Internal Grid.
        /// </summary>
        /// <returns></returns>
        private int GetPreviousRowIndex()
        {
            return (SelectedIndex > 0 ? SelectedIndex - 1 : SelectedIndex);
        }

        /// <summary>
        /// Gets the index of the next row from the Internal Grid.
        /// </summary>
        /// <returns></returns>
        private int GetNextRowIndex()
        {
            return (SelectedIndex < InternalGrid.VisualContainer.RowCount - InternalGrid.headerLineCount - 1 ? SelectedIndex + 1 : SelectedIndex);
        }

        /// <summary>
        /// Scrolls the Row in View.
        /// </summary>
        private void ScrollInView()
        {
            if (InternalGrid == null || InternalGrid.ItemsSource == null || InternalGrid.VisualContainer == null ||
                InternalGrid.SelectedIndex < 0) 
                return;

            var rowindex = InternalGrid.ResolveToRowIndex(InternalGrid.SelectedIndex);
            if (rowindex > InternalGrid.VisualContainer.RowCount) return;

            InternalGrid.VisualContainer.ScrollRows.ScrollInView(rowindex);
            InternalGrid.VisualContainer.InvalidateMeasureInfo();
        }

        private void UpdateSelection()
        {
            if (this.reflector == null)
                return;

            object record = null;

            if (isSelectedItemLoadedBeforeGridLoaded)
                ProcessSelectedItem(SelectedItem);
            else if (isSelectedValueLoadedBeforeGridLoaded)
            {
                if (!string.IsNullOrEmpty(ValueMember))
                {
                    record = appendSource.FirstOrDefault
                            (o => (GetSelectedValue(o) != null) &&
                                                (GetSelectedValue(o)
                                                .Equals(this.SelectedValue)));
                }
                else
                    record = this.SelectedValue;

                // Reset the Selected Index and Selected Item to its default value when Seleceted value is null.                
                ProcessSelecteValue(record);
            }
            else if (isSelectedIndexLoadedBeforeGridLoaded)
                ProcessSelectedIndex(SelectedIndex);

            isSelectedValueLoadedBeforeGridLoaded = false;
            isSelectedItemLoadedBeforeGridLoaded = false;
            isSelectedIndexLoadedBeforeGridLoaded = false;

            ScrollInView();
        }

        private void ProcessSelectedIndex(int rowIndex)
        {
            IsInSuspend = true;
            SelectedItem = InternalGrid.GetRecordAtRowIndex(InternalGrid.ResolveToRowIndex(rowIndex));
            SetSelectedValue(SelectedItem);
            isInTextChange = true;
            SetDisplayText(SelectedItem);
            isInTextChange = false;
            IsInSuspend = false;
        }

        private void ProcessSelectedItem(object selectedItem)
        {
            IsInSuspend = true;
            SelectedIndex = InternalGrid.ResolveToRecordIndex(InternalGrid.ResolveToRowIndex(selectedItem));
            SetSelectedValue(selectedItem);
            isInTextChange = true;
            SetDisplayText(selectedItem);
            isInTextChange = false;
            IsInSuspend = false;
        }

        private void ProcessSelecteValue(object record)
        {
            IsInSuspend = true;
            SelectedItem = record;
            // Reset the Selected Index and Selected Item to its default value when Seleceted value is null.
            if (record == null)
                SelectedIndex = -1;
            else
                SelectedIndex = InternalGrid.ResolveToRecordIndex(InternalGrid.ResolveToRowIndex(SelectedItem));
            isInTextChange = true;
            SetDisplayText(SelectedItem);
            isInTextChange = false;
            IsInSuspend = false;
        }

#endregion

#region ResizeThumb Events
#if !WPF
        private void ProcessDragCompleted(object sender, DragCompletedEventArgs e)
        {
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
        }
#endif

#if !WPF
        private void ProcessResizeThumbOnMouseLeave(object sender, MouseButtonEventArgs e)
#else
        private void ProcessResizeThumbOnMouseLeave(object sender, MouseEventArgs e)
#endif
        {
#if !WPF
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.Arrow, 1);
#else
            ResizeThumb.Cursor = Cursors.Arrow;
#endif
        }

#if !WPF
        private void ProcessResizeThumbOnMouseEnter(object sender, MouseButtonEventArgs e)
#else
        private void ProcessResizeThumbOnMouseEnter(object sender, MouseEventArgs e)
#endif
        {
#if !WPF
            Window.Current.CoreWindow.PointerCursor = new CoreCursor(CoreCursorType.SizeNorthwestSoutheast, 1);
#else
            ResizeThumb.Cursor = Cursors.SizeNWSE;
#endif
        }

        private void ProcessResizeDragDelta(object sender, DragDeltaEventArgs e)
        {
            var gridWidth = this.InternalGrid.VisualContainer.ScrollColumns.ScrollBar.Maximum;
            var gridHeight = this.InternalGrid.VisualContainer.ScrollRows.ScrollBar.Maximum;
            //gets the resized popupheight.
            var popupHeight = PopupHeight + e.VerticalChange;
            var remainingWindowHeight = GetRemainingWindowHeight();
            var remainingWindowWidth = GetRemainingWindowWidth();
            //while resizing, if the resized popupheight is less than the PopupMinHeight, then the PopupHeight should not set.
            //30 is subtracted from remainingWindowHeight to check with PopupHeight for considering the Taskbar.
            if (popupHeight > 0 && popupHeight >= PopupMinHeight && popupHeight < gridHeight + 35 && popupHeight < remainingWindowHeight - 30)
                PopupHeight += e.VerticalChange;

            //gets the resized popupwidth.
            var popupWidth = PopupWidth + e.HorizontalChange;

            //while resizing, if the resized popupwidth is less than the PopupMinWidth, then the PopupWidth should not set.
            if (popupWidth > 0 && popupWidth >= PopupMinWidth && popupWidth < gridWidth + 25)
                PopupWidth += e.HorizontalChange;
#if WPF
            this.InternalPopup.Width = this.PopupWidth;
            this.InternalPopup.Height = this.PopupHeight;
#else
            this.PopupBorder.Width = this.PopupWidth;
            this.PopupBorder.Height = this.PopupHeight;
#endif
            isResized = true;
        }
#endregion

#region DropDownButton Events

#if !WPF
        private void OnDropDownButtonClicked(object sender, TappedRoutedEventArgs e)
#else
        private void OnDropDownButtonClicked(object sender, RoutedEventArgs e)
#endif
        {
            if (!ReadOnly && Editor != null)
                Editor.SelectAll();

            IsDropDownOpen = (bool)this.DropDownButton.IsChecked;
            if (IsDropDownOpen)
            {
                ProcessEditorFocus();
            }
        }

#endregion

#region Editor Events

        private void OnEditorKeyDown(object sender, KeyEventArgs e)
        {
            ProcessKeyDown(e);
        }

        /// <summary>
        /// Occurs while doing mouse wheel in the control.
        /// </summary>
        /// <param name="sender">The source of the event</param>
        /// <param name="e">The event data.</param>
#if !WPF
        protected virtual void ProcessOnMouseWheelSpin(object sender, MouseButtonEventArgs e)
#else
        protected virtual void ProcessOnMouseWheelSpin(object sender, MouseWheelEventArgs e)
#endif
        {
#if !WPF
            var point = e.GetCurrentPoint(this);
            var delta = point.Properties.MouseWheelDelta;
#else
            var delta = e.Delta;
#endif
            if (!AllowSpinOnMouseWheel) return;
            SelectedIndex = delta < 0 ? GetNextRowIndex() : GetPreviousRowIndex();
            if (!ReadOnly)
                Editor.SelectAll();
        }

        /// <summary>
        /// Occurs when the editor text gets changed.
        /// </summary>
        /// <param name="sender">The sender of the event</param>
        /// <param name="e">The event data</param>
        /// <remarks>
        /// This event is raised if the editor text has been changed by either a programmatic modification or user interaction.
        /// </remarks>
        protected virtual void ProcessOnEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if (isInTextChange || ReadOnly)
                return;

            var collection = new ObservableCollection<object>();
            var textBoxEditor = sender as TextBox;
            if (textBoxEditor != null)
            {
                filterText = textBoxEditor.Text;
                filterText = filterText.Substring(0, (textBoxEditor.Text.Length - textBoxEditor.SelectedText.Length));
#if !WPF
                //Initially if any text pressed in Editor means, the text will selected through OnEditorGotFocus event. So filterText becomes empty in above when the textBoxEditor text length is one.
                //Here added the code to set the filterText when textBoxEditor text length is 1 and filterText is empty.
                if (string.IsNullOrEmpty(filterText) && AllowImmediatePopup && Text.Length == 1)
                    filterText = textBoxEditor.Text;
#endif
                SearchText = filterText;
            }
            appendSource = ItemsSource as IEnumerable<object>;

            if (appendSource == null)
                return;

            var textLength = filterText.Length;
            var iterationCount = 0;
            var exactValue = string.Empty;
            foreach (var item in appendSource)
            {
                ++iterationCount;

                if (string.IsNullOrEmpty(DisplayMember))
                    return;

                var displayValue = GetDisplayValue(item);

                if (displayValue == null)
                    continue;

                exactValue = displayValue.ToString();
                if (!AllowCaseSensitiveFiltering)
                {
                    filterText = filterText.ToLower();
                    exactValue = exactValue.ToLower();
                }
                if (ProcessAppendText(item, exactValue, filterText))
                {
                    if (!string.IsNullOrEmpty(Editor.Text))
                    {
                        if (!string.IsNullOrEmpty(filterText))
                        {
                            if ((AllowIncrementalFiltering && IsDropDownOpen) || AllowAutoComplete)
                            {
                                IsInSuspend = true;
                                InternalGrid.SelectedItem = null;
                                InternalGrid.SelectedItem = item;
                                IsInSuspend = false;
                            }
                        }
                    }
                    var _value = GetDisplayValue(item).ToString();
                    var _source = ProcessAppendStringList(item, _value);
                    foreach (var _item in _source)
                        collection.Add(_item as string);
                }

                if (collection.Count > 0)
                    break;
            }

            if (collection.Count < 1)
            {
                IsInSuspend = true;
                InternalGrid.SelectedItem = null;
                IsInSuspend = false;
            }

            if (AllowAutoComplete)
            {
                if (collection.Count > 0)
                {
#if !WPF
                    if (!string.IsNullOrEmpty(Text) && Editor.SelectionLength == Text.Length - textLength)
                        return;
#endif
                    var firstOrDefault = collection.FirstOrDefault();
                    var append = firstOrDefault != null && Editor.Text != string.Empty ? firstOrDefault.ToString() : string.Empty;
                    isInTextChange = true;
                    Text = string.Empty;
#if WPF
                    Text = append;
#else
                    if (!string.IsNullOrEmpty(filterText))
                        Text = append;
#endif
                    isInTextChange = false;
                    Editor.SelectionStart = textLength;
                    Editor.SelectionLength = Text.Length - textLength;
                }
            }
            if (AllowIncrementalFiltering && IsDropDownOpen)
                ProcessIncrementalFiltering();
            ScrollInView();
        }

        private void OnEditorGotFocus(object sender, RoutedEventArgs e)
        {
            if (TextSelectionOnFocus && !ReadOnly)
                Editor.SelectAll();
            VisualStateManager.GoToState(this, "Focused", false);
        }

#if !WPF

        private void SfMultiColumnDropDownControl_LostFocus(object sender, RoutedEventArgs e)
        {
            VisualStateManager.GoToState(this, "Unfocused", false);
        }

        private void SfMultiColumnDropDownControl_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (ReadOnly && !IsDropDownOpen)
            {
                IsDropDownOpen = true;
                ProcessEditorFocus();
                VisualStateManager.GoToState(this, "Focused", false);
            }
        }
               
#endif

        private void OnEditorLostFocus(object sender, RoutedEventArgs e)
        {
            SearchText = string.Empty;
            if (!IsDropDownOpen)
                CommitValue();
#if WPF
            else
                this.Focus();
#else
            // UWP-5128 we can set back the focus to edit element when dropdown is closed.
            else
                this.Focus(FocusState.Programmatic);
#endif
            VisualStateManager.GoToState(this, "Unfocused", false);
        }

#endregion

#region Pop-up Events

#if !WPF
        private void ProcessPopupClosed(object sender, object e)
#else
        private void ProcessPopupClosed(object sender, EventArgs e)
#endif
        {
            if (!ReadOnly && Editor != null)
                Editor.SelectAll();
            if (!DropDownButton.IsPressed)
                DropDownButton.IsChecked = false;
            if (!ReadOnly && string.IsNullOrEmpty(Editor.Text))
            {
                IsInSuspend = true;
                SelectedIndex = -1;
                IsInSuspend = false;
            }
            VisualStateManager.GoToState(this, "Focused", false);
        }

#if !WPF
        void ProcessPopupOpened(object sender, object e)
#else
        private void ProcessPopupOpened(object sender, EventArgs e)
#endif
        {
            if (ReadOnly)
                return;

            if (!AllowIncrementalFiltering || InternalGrid.SelectedItem == null || string.IsNullOrEmpty(DisplayMember))
                return;

            var _value = GetDisplayValue(InternalGrid.SelectedItem);

            if (_value == null || string.Equals(GetControlValue(), _value.ToString()))
                return;

            ProcessIncrementalFiltering();
        }

#endregion

#region InternalGrid Events

#if !WPF
        private void OnInternalGridTapped(object sender, TappedRoutedEventArgs e)
#else
        private void OnInternalGridTapped(object sender, MouseButtonEventArgs e)
#endif
        {
#if WPF
            var point = e.GetPosition(InternalGrid);
            if (point.Y < InternalGrid.HeaderRowHeight)
            {
                //While sorting, need to focus Editor. Because after sorting, if pressing enter key means ProcessOnKeyDown need to fire.
                ProcessEditorFocus();
                return;
            }
#else
            var pp = e.GetPosition(null);
            var uielements = VisualTreeHelper.FindElementsInHostCoordinates(pp, InternalGrid);
            if (uielements.Count() > 0 && uielements.Any(element => element is GridHeaderCellControl))
            {
                ProcessEditorFocus();
                return;
            }
#endif
            SelectedItem = InternalGrid.SelectedItem;
            CommitValue(true);
            IsDropDownOpen = false;
            ProcessEditorFocus();
        }

        private void InternalGrid_ItemsSourceChanged(object sender, GridItemsSourceChangedEventArgs e)
        {
            if (InternalGrid == null || InternalGrid.View == null)
            {
                // WPF-34079 - Changing template clear the view and reset the selection should not happen when its ItemsSource is not null.
                if (this.ItemsSource != null)
                    return;
                // While changing InternalGrid items-source as null, need to clear SfMultiColumnDropDownControl selection
                this.ClearSelection();
                return;
            }
            InternalGrid.View.RecordPropertyChanged += OnRecordPropertyChanged;
            this.reflector = this.InternalGrid.View.GetPropertyAccessProvider();
            UpdateSelection();
        }

        private void OnInternalGridLoaded(object sender, RoutedEventArgs e)
        {
            //if view is null, then Pop-up height and width calculation is skipped. So condition made to calculate the 
            //Pop-up Width and height based on the IsDropDownOpen.
            if (IsDropDownOpen)
            {
                if (!isResized && this.IsAutoPopupSize && InternalGrid != null && InternalGrid.HasView)
                    DropDownAutoSize();

                ProcessOnPopupPositionCalculation();
            }
#if WPF
            if (this.InternalPopup != null)
            {
                this.InternalPopup.Width = this.PopupWidth;
                this.InternalPopup.Height = this.PopupHeight;
            }
#else
            if (!IsAutoPopupSize &&  Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile")
            {
                this.PopupWidth = DefaultPopupMinWidth;
            }
            if (this.PopupBorder != null)
            {
                this.PopupBorder.Width = this.PopupWidth;
                this.PopupBorder.Height = this.PopupHeight;
            }
#endif
        }

#endregion

#region Events

        private void OnRecordPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender == InternalGrid.SelectedItem && !IsInSuspend)
                ProcessSelectedItem(InternalGrid.SelectedItem);
        }

        private void ProcessLoaded(object sender, RoutedEventArgs e)
        {
            if (ResizeThumb != null)
            {
#if WPF
                ResizeThumb.AddHandler(MouseEnterEvent, (MouseEventHandler)ProcessResizeThumbOnMouseEnter);
                ResizeThumb.AddHandler(MouseLeaveEvent, (MouseEventHandler)ProcessResizeThumbOnMouseLeave);
                ResizeThumb.AddHandler(Thumb.DragDeltaEvent, (DragDeltaEventHandler)ProcessResizeDragDelta);
#else
                ResizeThumb.PointerEntered += ProcessResizeThumbOnMouseEnter;
                ResizeThumb.PointerExited += ProcessResizeThumbOnMouseLeave;
                ResizeThumb.DragDelta += ProcessResizeDragDelta;
                ResizeThumb.DragCompleted += ProcessDragCompleted;
#endif
            }
            PreviousSelectedItem = InternalGrid != null && PreviousSelectedItem != InternalGrid.SelectedItem ?
                InternalGrid.SelectedItem : PreviousSelectedItem;
        }

        private void ProcessUnloaded(object sender, RoutedEventArgs e)
        {
            if (ResizeThumb != null)
            {
#if WPF
                ResizeThumb.RemoveHandler(MouseEnterEvent, (MouseEventHandler)ProcessResizeThumbOnMouseEnter);
                ResizeThumb.RemoveHandler(MouseLeaveEvent, (MouseEventHandler)ProcessResizeThumbOnMouseLeave);
                ResizeThumb.RemoveHandler(Thumb.DragDeltaEvent, (DragDeltaEventHandler)ProcessResizeDragDelta);
#else
                ResizeThumb.PointerEntered -= ProcessResizeThumbOnMouseEnter;
                ResizeThumb.PointerExited -= ProcessResizeThumbOnMouseLeave;
                ResizeThumb.DragDelta -= ProcessResizeDragDelta;
                ResizeThumb.DragCompleted -= ProcessDragCompleted;
#endif
            }           
        }

#endregion

#region WireEvents
        /// <summary>
        /// Wires the events in SfMultiColumnDropDown Control.
        /// </summary>
        private void WireEvents()
        {
            if (InternalGrid != null)
            {
                InternalGrid.Loaded += OnInternalGridLoaded;
                InternalGrid.ItemsSourceChanged += InternalGrid_ItemsSourceChanged;
#if !WPF
                InternalGrid.Tapped += OnInternalGridTapped;
#else
                InternalGrid.MouseLeftButtonUp += OnInternalGridTapped;
#endif
            }
            if (DropDownButton != null)
            {
#if WPF
                DropDownButton.AddHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnDropDownButtonClicked);
#else
                DropDownButton.AddHandler(TappedEvent, (TappedEventHandler)OnDropDownButtonClicked, true);
#endif
            }          
            if (InternalPopup != null)
            {
                InternalPopup.Closed += ProcessPopupClosed;
                InternalPopup.Opened += ProcessPopupOpened;
            }
            this.Loaded += ProcessLoaded;
            this.Unloaded += ProcessUnloaded;
        }     

        private void WireEditorEvents()
        {
#if WPF
            Editor.AddHandler(PreviewKeyDownEvent, (KeyEventHandler)OnEditorKeyDown);
            Editor.AddHandler(MouseWheelEvent, (MouseWheelEventHandler)ProcessOnMouseWheelSpin);
            Editor.AddHandler(TextBoxBase.TextChangedEvent, (TextChangedEventHandler)ProcessOnEditorTextChanged);
            Editor.AddHandler(GotFocusEvent, (RoutedEventHandler)OnEditorGotFocus);
#else
            Editor.AddHandler(KeyDownEvent, (KeyEventHandler)OnEditorKeyDown, true);
            Editor.TextChanged += ProcessOnEditorTextChanged;
            Editor.PointerWheelChanged += ProcessOnMouseWheelSpin;
            Editor.GotFocus += OnEditorGotFocus;
#endif
            Editor.LostFocus += OnEditorLostFocus;
        }

#if !WPF
        private void WireReadOnlyEvents()
        {
            if (ReadOnly && ContentControl == null)
                ContentControl = GetTemplateChild("PART_ContentControl") as ContentControl;
            else if (!ReadOnly && Editor == null)
                Editor = GetTemplateChild("PART_TextBox") as TextBox;

            this.Tapped += SfMultiColumnDropDownControl_Tapped;
            this.AddHandler(KeyDownEvent, (KeyEventHandler)OnEditorKeyDown, true);
            this.LostFocus += SfMultiColumnDropDownControl_LostFocus;
        }
        
#endif
#endregion

#region UnWireEvents
        /// <summary>
        /// Unsubscribes the Wired events in SfMultiColumnDropDown Control.
        /// </summary>
        private void UnWireEvents()
        {
            if (InternalGrid != null)
            {
                InternalGrid.Loaded -= OnInternalGridLoaded;
                InternalGrid.ItemsSourceChanged -= InternalGrid_ItemsSourceChanged;
#if !WPF
                InternalGrid.Tapped -= OnInternalGridTapped;
#else
                InternalGrid.MouseLeftButtonUp -= OnInternalGridTapped;
#endif
                if (InternalGrid.View != null)
                    InternalGrid.View.RecordPropertyChanged -= OnRecordPropertyChanged;
            }
            if (DropDownButton != null)
            {
#if WPF
                DropDownButton.RemoveHandler(ButtonBase.ClickEvent, (RoutedEventHandler)OnDropDownButtonClicked);
#else
                DropDownButton.RemoveHandler(TappedEvent, (TappedEventHandler)OnDropDownButtonClicked);
#endif
            }           

            if (InternalPopup != null)
            {
                InternalPopup.Closed -= ProcessPopupClosed;
                InternalPopup.Opened -= ProcessPopupOpened;
            }
            this.Loaded -= ProcessLoaded;
            this.Unloaded -= ProcessUnloaded;
        }

        private void UnWireEditorEvents()
        {
            if (Editor != null)
            {
#if WPF
                Editor.RemoveHandler(PreviewKeyDownEvent, (KeyEventHandler)OnEditorKeyDown);
                Editor.RemoveHandler(MouseWheelEvent, (MouseWheelEventHandler)ProcessOnMouseWheelSpin);
                Editor.RemoveHandler(TextBoxBase.TextChangedEvent, (TextChangedEventHandler)ProcessOnEditorTextChanged);
                Editor.RemoveHandler(GotFocusEvent, (RoutedEventHandler)OnEditorGotFocus);
#else
                Editor.RemoveHandler(KeyDownEvent, (KeyEventHandler)OnEditorKeyDown);
                Editor.TextChanged -= ProcessOnEditorTextChanged;
                Editor.PointerWheelChanged -= ProcessOnMouseWheelSpin;
                Editor.GotFocus -= OnEditorGotFocus;
#endif
                Editor.LostFocus -= OnEditorLostFocus;
            }
        }

#if !WPF
        private void UnWireReadOnlyEvents()
        {
            this.Tapped -= SfMultiColumnDropDownControl_Tapped;
            this.RemoveHandler(KeyDownEvent, (KeyEventHandler)OnEditorKeyDown);
            this.LostFocus -= SfMultiColumnDropDownControl_LostFocus;
        }
#endif
#endregion

#region IDisposable

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl"/>.
        /// </summary>
        /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
        protected virtual void Dispose(bool isDisposing)
        {
            if (isdisposed) return;
            UnWireEvents();
#if !WPF
            if (ReadOnly)
                UnWireReadOnlyEvents();
            else
#endif
            UnWireEditorEvents();
            if (isDisposing)
            {
                if (InternalGrid != null)
                {
                    InternalGrid.Dispose();
                    InternalGrid = null;
                }

                if (appendSource != null)
                    appendSource = null;

                if (SelectedItem != null)
                    SelectedItem = null;

                if (ItemsSource != null)
                    ItemsSource = null;

                if (InternalPopup != null)
                    InternalPopup = null;

                if (reflector != null)
                    reflector = null;
            }
            isdisposed = true;
        }

#endregion

#if WPF
#region AutomationOverrides

        /// <summary>
        /// Creates and returns an <see cref="T:Syncfusion.UI.Xaml.Grid.AutomationPeerHelper"/> object for the
        /// SfMultiColumnDropDownControl.
        /// </summary>
        /// <returns>
        /// Returns new instance of <see cref="T:Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControlAutomationPeer"/>
        /// for the SfMultiColumnDropDownControl.
        /// </returns>
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            if ((AutomationPeerHelper.IsScreenReaderRunning ?? false) || AutomationPeerHelper.EnableCodedUI)
                return new SfMultiColumnDropDownControlAutomationPeer(this);
            return base.OnCreateAutomationPeer();
        }
#endregion


#region Obsolete Properties

        /// <summary>
        /// Gets the actual height of the pop-up.
        /// </summary>
        /// <value>
        /// The actual height of the pop-up.
        /// </value>
        [Obsolete]
        public double ActualPopupHeight
        {
            get { return (double)GetValue(ActualPopupHeightProperty); }
            internal set { SetValue(ActualPopupHeightProperty, null); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ActualPopupHeight dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ActualPopupHeight dependency property.
        /// </remarks>
        public static readonly DependencyProperty ActualPopupHeightProperty =
            DependencyProperty.Register("ActualPopupHeight", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets the actual width of the pop-up.
        /// </summary>
        /// <value>
        /// The actual width of the pop-up.
        /// </value>
        [Obsolete]
        public double ActualPopupWidth
        {
            get { return (double)GetValue(ActualPopupWidthProperty); }
            internal set { SetValue(ActualPopupWidthProperty, null); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ActualPopupWidth dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.ActualPopupWidth dependency property.
        /// </remarks>
        public static readonly DependencyProperty ActualPopupWidthProperty =
            DependencyProperty.Register("ActualPopupWidth", typeof(double), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(double.NaN));

        /// <summary>
        /// Gets or sets the template to the pop-up.
        /// </summary
        /// <value>
        /// The <see cref="System.Windows.DataTemplate"/> that defines the visual representation of the pop-up.
        /// </value>
        [Obsolete]
        public ControlTemplate PopupContentTemplate
        {
            get { return (ControlTemplate)GetValue(PopupContentTemplateProperty); }
            set { SetValue(PopupContentTemplateProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupContentTemplate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.PopupContentTemplate dependency property.
        /// </remarks>
        public static DependencyProperty PopupContentTemplateProperty =
            DependencyProperty.Register("PopupContentTemplate", typeof(ControlTemplate), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the corner radius.
        /// </summary>
        /// <value>
        /// The corner radius.
        /// </value>
        [Obsolete]
        public Thickness CornerRadius
        {
            get { return (Thickness)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.CornerRadius dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.SfMultiColumnDropDownControl.CornerRadius dependency property.
        /// </remarks>
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(Thickness), typeof(SfMultiColumnDropDownControl), new PropertyMetadata(new Thickness(2)));

#endregion
#endif
    }
}