#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Collections.ObjectModel;
#if !WinRT && !UNIVERSAL
using System.Windows;
#else
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Class used to represent the data in to hierarchical format.
    /// </summary>
    public abstract class ViewDefinition : DependencyObject
    {
        /// <summary>
        /// Relational column name to represent the hierarchical data
        /// </summary>
        public string RelationalColumn
        {
            get { return (string) GetValue(RelationalColumnProperty); }
            set { SetValue(RelationalColumnProperty, value); }
        }


        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.ViewDefinition.RelationalColumn dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.ViewDefinition.RelationalColumn dependency property.
        /// </remarks>   
        public static readonly DependencyProperty RelationalColumnProperty =
            DependencyProperty.Register("RelationalColumn", typeof(string), typeof(ViewDefinition), new PropertyMetadata(default(string)));

    }

    public class GridViewDefinition : ViewDefinition
    {
        public GridViewDefinition()
        {
            DataGrid  = new SfDataGrid();
        }

        internal DetailsViewNotifyListener NotifyListener { get; set; }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridViewDefinition.DataGrid dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridViewDefinition.DataGrid dependency property.
        /// </remarks>   
        public static readonly DependencyProperty DataGridProperty =
            DependencyProperty.Register("DataGrid", typeof (SfDataGrid), typeof (GridViewDefinition), new PropertyMetadata(null));

        /// <summary>
        /// DataGrid used to set the properties for <see cref="Syncfusion.UI.Xaml.Grid.DetailsViewDataGrid"/>.
        /// </summary>
        public SfDataGrid DataGrid
        {
            get { return (SfDataGrid) GetValue(DataGridProperty); }
            set { SetValue(DataGridProperty, value); }
        }
    }


    /// <summary>
    /// Class used to represent the collection of <see cref="Syncfusion.UI.Xaml.Grid.ViewDefinition"/> that enables you to represent the data in to hierarchical format.
    /// </summary>
    public class DetailsViewDefinition : ObservableCollection<ViewDefinition>
    {
        
    }
}
