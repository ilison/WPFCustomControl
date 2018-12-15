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
using System.Threading.Tasks;

namespace Syncfusion.UI.Xaml.Grid
{
    public class GridSearchCellInfo
    {
        #region Fields
        private object record;
        private List<GridColumn> columnCollection;
        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets data object associated with cell. 
        /// </summary>
        /// <value>DataContext of cell. </value>
        public object Record
        {
            get { return record; }
            set { record = value; }
        }

        /// <summary>
        /// Gets a collection columns match with the search text in the associated data object. 
        /// </summary>
        /// <value> A collection of columns that match with search text in a row. </value>
        public List<GridColumn> ColumnCollection
        {
            get { return columnCollection; }
            set { columnCollection = value; }
        }

        #endregion

        /// <summary>
        /// Returns whether the specified column already added in ColumnCollection.
        /// </summary>
        /// <param name="column">Specifies the column to check.</param>
        /// <returns>Returns <b>true</b> if the Column is maintained in the ColumnCollection, otherwise <b>false</b>.</returns>
        public bool HasColumn(GridColumn column)
        {
            if (columnCollection == null)
                return false;
            return ColumnCollection.Any(col => col.MappingName == column.MappingName);
        }

        public GridSearchCellInfo()
        {
            columnCollection = new List<GridColumn>();
        }
    }
}
