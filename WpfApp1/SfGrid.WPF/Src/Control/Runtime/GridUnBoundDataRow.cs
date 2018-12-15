#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
# if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents the UnboundRow which used to display additional rows which are not bound to data source in SfDataGrid.
    /// </summary>
    /// <remarks>SfDataGrid allows you to add additional rows at the top and also bottom of the SfDataGrid which are not bound with data object from underlying data source. You can add unbound rows using SfDataGrid.UnBoundRows collection property. You can add any no of unbound rows to SfDataGrid.</remarks>
    public class GridUnBoundRow : DependencyObject, INotifyDependencyPropertyChanged
    {
        internal int UnBoundRegionIndex = -1;
        private bool isdisposed = false;
        
        /// <summary>
        /// Gets the row index of the UnboundRow.
        /// </summary>
        /// <value>
        /// The corresponding row index of UnboundRow.
        /// </value>
        int rowIndex = -1;
        public int RowIndex
        {
            get
            {
                return rowIndex;
            }
            internal set
            {
                rowIndex = value;
            }
        }

        /// <summary>
        /// Gets the index of UnboundRow from the GridUnBoundRow collection.
        /// </summary>
        int unBoundRowIndex = -1;
        public int UnBoundRowIndex
        {
            get
            {
                return unBoundRowIndex;
            }
            internal set
            {
                unBoundRowIndex = value;
            }
        }
        /// <summary>
        /// For notifying DetailsViewDataGrid, DataGrid instance is maintained
        /// </summary>
        internal SfDataGrid DataGrid
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets a value that indicates whether the GridUnBoundRow should be displayed above or below the TableSummaryRow.
        /// </summary>
        /// <value>
        /// <b>true</b> if the UnboundRow is displayed below the TableSummaryRow; otherwise, <b>false</b>.
        /// The default value is <b>true</b>.
        /// </value>   
        /// <exception cref="System.NotSupportedException">
        /// When you change the placement of UnboundRow at run time.
        /// </exception>
        public bool ShowBelowSummary
        {
            get
            {
                return (bool)this.GetValue(ShowBelowSummaryProperty);
            }
            set
            {
                if (this.RowIndex == -1)
                    this.SetValue(ShowBelowSummaryProperty, value);
                else
                    throw new NotSupportedException("Cannot change the placement of UnBoundRow at run time");
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether the GridUnBoundRow is positioned at either top or bottom of SfDataGrid.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.UnBoundRowsPosition"/> enumeration that specifies the position of GridUnBoundRow in SfDataGrid.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.UnBoundRowsPosition.Top"/> .
        /// </value>
        public UnBoundRowsPosition Position
        {
            get
            {
                return (UnBoundRowsPosition)this.GetValue(PositionProperty);
            }
            set
            {
                if (this.RowIndex == -1)
                    this.SetValue(PositionProperty, value);
                else
                    throw new NotSupportedException("Cannot change the position of UnBoundRow at run time");
            }
        }

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUnBoundRow.ShowBelowSummary dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUnBoundRow.ShowBelowSummary dependency property.
        /// </remarks>       
        public static readonly DependencyProperty ShowBelowSummaryProperty = 
            GridDependencyProperty.Register("ShowBelowSummary", typeof(bool), typeof(GridUnBoundRow), new GridPropertyMetadata(true));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridUnBoundRow.Position dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridUnBoundRow.Position dependency property.
        /// </remarks>  
        public static readonly DependencyProperty PositionProperty = 
            GridDependencyProperty.Register("Position", typeof(UnBoundRowsPosition), typeof(GridUnBoundRow), new GridPropertyMetadata(UnBoundRowsPosition.Top));

        /// <summary>
        /// Invoked whenever the value of any dependency property in the UnBoundRow has been updated.
        /// </summary>
        /// <param name="propertyName">
        /// The property name that has changed in UnBoundRow.
        /// </param>
        /// <param name="e">
        /// <see cref="System.Windows.DependencyPropertyChangedEventArgs"/> that contains the data for various dependency property changed events.
        /// </param>
        public void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e)
        {
            if (this.DataGrid == null) return;
            var index = this.DataGrid.UnBoundRows.IndexOf(this);
            // Need to get NotifyListener from SourceDataGrid and call NotifyPropertyChanged method
            if (this.DataGrid.NotifyListener != null)
                this.DataGrid.NotifyListener.SourceDataGrid.NotifyListener.NotifyPropertyChanged(this, propertyName, e, datagrid => datagrid.UnBoundRows[index], DataGrid, typeof(GridUnBoundRow));
        }

        /// <summary>
        /// Releases all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> class.
        /// </summary>
       public void Dispose()
       {
           Dispose(true);
           GC.SuppressFinalize(this);
       }

       /// <summary>
       /// Disposes all the resources used by the <see cref="Syncfusion.UI.Xaml.Grid.GridUnBoundRow"/> class.
       /// </summary>
       /// <param name="isDisposing">Indicates whether the call is from Dispose method or from a finalizer.</param>
       protected virtual void Dispose(bool isDisposing)
       {
           if (isdisposed) return;
           if (isDisposing)
                this.DataGrid = null;
           isdisposed = true;
       }
    }    
}
