#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that manages the row heights operation in SfDataGrid control.
    /// </summary>
    public class RowHeightManager
    {
        internal Range Header= new Range();
        internal Range Body = new Range();
        internal Range Footer = new Range();        
        internal List<int> DirectoryRows = new List<int>();

        /// <summary>
        /// Determines whether the particular index is contained in specified region.
        /// </summary>
        /// <param name="index">
        /// The corresponding index to check whether it is contained in specified region.
        /// </param>
        /// <param name="region">
        /// The corresponding region.
        /// </param>
        /// <returns>
        /// Returns <b>true</b> if the index is present in specified region; otherwise, <b>false</b>.
        /// </returns>
        public bool Contains(int index, RowRegion region)
        {
            Range range = null;
            if(region == RowRegion.Header)
                range = Header;
            else if(region == RowRegion.Body)
                range = Body;
            else
                range = Footer;
            
            if(range.IsEmpty)
            {
                if (DirectoryRows.Contains(index))
                    DirectoryRows.Remove(index);
                return false;
            }

            if (index >= range.start && index <= range.end)
            {
                if (DirectoryRows.Contains(index)) 
                {
                    DirectoryRows.Remove(index);                                   
                    return false;                
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Mark the row height of the row index as dirty, when the row is invalidated at run time.
        /// </summary>
        /// <param name="rowIndex">index of the row</param>
        public void SetDirty(int rowIndex)
        {
            if (!DirectoryRows.Contains(rowIndex))
                DirectoryRows.Add(rowIndex);
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.RowHeightManager"/> class.
        /// </summary>
        public RowHeightManager()
        {
        }

        internal Range GetRange(int index)
        {
            if(index == 0) return Header;
            else if(index == 1) return Body;
            else return Footer;
        }

        /// <summary>
        /// Marks the row heights of all rows in a view as dirty.
        /// </summary>
        public void Reset()
        {
            Header.start = Header.end = Body.start = Body.end = Footer.start = Footer.end = -1;               
        }

        /// <summary>
        /// Marks the row heights of the body region as dirty, to refresh the row height when the grouping, sorting and the view is updated.
        /// </summary>
        public void ResetBody()
        {
            Body.start = Body.end = -1;
        }

        /// <summary>
        /// Marks the row heights of the footer region as dirty, to refresh the row height.
        /// </summary>
        public void ResetFooter()
        {
            Footer.start = Footer.end = -1;
        }

        /// <summary>
        /// Updates the rows in body region when the view is refreshed.
        /// </summary>
        /// <param name="index">
        /// The start index to update.
        /// </param>
        /// <param name="count">
        /// The count.
        /// </param>
        /// <param name="action">
        /// The action that caused to update the body region.
        /// </param>
        public void UpdateBody(int index, int count, NotifyCollectionChangedAction action)
        {
            //if (index > Body.end)
            //    return;
            if ((index + count) <= Body.start)
            {
                this.ResetBody();
                return;
            }
            else if (index > Body.end)
                return;
            else
                Body.end = index;
        }

        /// <summary>
        /// Updates the region of rows for the specified start and end index.
        /// </summary>
        /// <param name="start">
        /// The start index to update.
        /// </param>
        /// <param name="end">
        /// The end index to update.
        /// </param>
        /// <param name="region">
        /// The region that is to be updated.
        /// </param>
        public void UpdateRegion(int start, int end, RowRegion region)
        {
            Range range = null;
            if (region == RowRegion.Header)
                range = Header;
            else if (region == RowRegion.Body)
                range = Body;
            else
                range = Footer;

            range.start = start;
            range.end = end;
        }
       
    }

    /// <summary>
    /// Represents a class that maintains the range for rows.
    /// </summary>
    public class Range
    {
        internal int start;
        internal int end;  
        
        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.Range"/> class.
        /// </summary>
        public Range()
        {
        }
        /// <summary>
        /// Gets a value that indicates whether the range that contains an empty rows.
        /// </summary>
        public bool IsEmpty
        {
            get { return start < 0 || end < 0;}
        }
    }

    /// <summary>
    /// Represents the method that will handle the <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryRowHeight"/> event.
    /// </summary>
    /// <param name="sender">
    /// The source of the event.
    /// </param>
    /// <param name="e">
    /// A <see cref="Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs"/> that contains the event data.
    /// </param>
    public delegate void QueryRowHeightEventHandler(object sender, QueryRowHeightEventArgs e);

    /// <summary>
    /// Provides data for <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryRowHeight"/> event.
    /// </summary>
    /// <remarks>
    /// To change the height of the row, need to enable the Handled in <see cref="Syncfusion.UI.Xaml.Grid.GridHandledEventArgs"/> of <see cref="Syncfusion.UI.Xaml.Grid.SfDataGrid.QueryRowHeight"/> event.
    /// </remarks>
    public class QueryRowHeightEventArgs : GridHandledEventArgs
    {
        int rowIndex;
        double height;
        /// <summary>
        /// Initializes a new instance of the <see cref="Syncfusion.UI.Xaml.Grid.QueryRowHeightEventArgs"/> class.
        /// </summary>
        /// <param name="index">
        /// The index of the row.
        /// </param>
        /// <param name="height">
        /// The height of the row.
        /// </param>
        /// <param name="originalSource">
        /// The source of the event.
        /// </param>
        public QueryRowHeightEventArgs(int index, double height, object originalSource):base(originalSource)
        {
            this.rowIndex = index;
            this.height = height;    
        }

        /// <summary>
        /// Gets the index of the row to get the row height.
        /// </summary>
        public int RowIndex
        {
            get {return rowIndex;}
        }

        /// <summary>
        /// Gets or sets the height of the row.
        /// </summary>
        /// <value>
        /// The height of the row.
        /// </value>
        /// <remarks>
        /// Row height can be set to this property based on RowIndex.
        /// </remarks>
        public double Height
        {
            get {return height;}
            set { height = value; }
        }
    }

    //public enum ResizeToFitOptions
    //{
    //    Auto,
    //    NoShrinkSize
    //}
    /// <summary>
    /// Represents a class that provides options to customize the auto row sizing in SfDataGrid.
    /// </summary>
    public class GridRowSizingOptions
    {
        //// <summary>
        /// Gets or sets the list of columns that needs to be excluded during row height calculation based on cell content.
        /// </summary>
        public List<string> ExcludeColumns { get; set; }

        //public ResizeToFitOptions ResizeToFitOptions { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates whether the hidden columns can be included during auto row sizing operation.
        /// </summary>
        /// <value>
        /// <b>true</b> if the hidden columns are included; otherwise, <b>false</b>.
        /// </value>
        public bool CanIncludeHiddenColumns { get; set; }

        /// <summary>
        /// Gets or sets a value that indicates the mode for calculating the height of the cell based on cell content. <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode.SmartFit"/> calculates the column height in optimized way. 
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode"/> enumeration that specifies the way to measure the height of each cell in corresponding row.
        /// The default value is <see cref="Syncfusion.UI.Xaml.Grid.AutoFitMode.SmartFit"/>. 
        /// </value>
        public AutoFitMode AutoFitMode
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Syncfusion.UI.Xaml.Grid.GridRowSizingOptions"/> class.
        /// </summary>
        public GridRowSizingOptions()
        {
            CanIncludeHiddenColumns = false;
            ExcludeColumns = new List<string>();
            //ResizeToFitOptions = Grid.ResizeToFitOptions.Auto;
        }
    }
}

