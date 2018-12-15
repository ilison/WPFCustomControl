#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.UI.Xaml.Grid.Utility;
using Syncfusion.UI.Xaml.ScrollAxis;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Controls;
using System;
#else
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
#endif

namespace Syncfusion.UI.Xaml.Grid.Cells
{
    #region GridCellDataTemplateRenderer
    [Obsolete]
    public class GridCellDataTemplateRenderer : GridCellTemplateRenderer
    {
        #region Display/Edit Binding Overrides
        /// <summary>
        /// Called when [initialize display element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeDisplayElement(DataColumnBase dataColumn,ContentControl uiElement,object dataContext)
        {
            base.OnInitializeTemplateElement(dataColumn, uiElement, dataContext);
        }

        public override void OnInitializeTemplateElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            OnInitializeDisplayElement( dataColumn,uiElement, dataContext);
        }

        public override void OnUpdateTemplateBinding(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {            
            uiElement.ClearValue(ContentControl.ContentProperty);
            var dataContextHelper = new DataContextHelper { Record = dataContext };
            dataContextHelper.SetValueBinding(dataColumn.GridColumn.DisplayBinding, dataContext);
            uiElement.Content = dataContextHelper;
        }

        /// <summary>
        /// Called when [initialize edit element].
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// <param name="uiElement">Corresponding Renderer Element</param>
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="dataContext">The data context.</param>
        public override void OnInitializeEditElement(DataColumnBase dataColumn, ContentControl uiElement, object dataContext)
        {
            base.OnInitializeEditElement(dataColumn, uiElement, dataContext);
            uiElement.ClearValue(ContentControl.ContentProperty);
            var dataContextHelper = new DataContextHelper { Record = dataContext };
            dataContextHelper.SetValueBinding(dataColumn.GridColumn.ValueBinding, dataContext);
            uiElement.Content = dataContextHelper;
        }

        /// <summary>
        /// Initializes the cell style.
        /// </summary>
        /// <param name="dataColumn">DataColumn Which holds GridColumn, RowColumnIndex and GridCell </param>
        /// RowColumnIndex - RowColumnIndex for the Renderer Element
        /// GridColumn - Column which is providing the information for Binding
        /// <param name="record">The record.</param>        
        protected override void InitializeCellStyle(DataColumnBase dataColumn, object record)
        {
            base.InitializeCellStyle(dataColumn, record);
            var uiElement = (dataColumn.ColumnElement as GridCell).Content as ContentControl;
            var gridColumn = dataColumn.GridColumn as GridTemplateColumn;
            if (uiElement != null && gridColumn != null)
            {
                uiElement.ClearValue(ContentControl.ContentProperty);
                var dataContextHelper = new DataContextHelper { Record = record };
                dataContextHelper.SetValueBinding(gridColumn.ValueBinding, record);
                uiElement.Content = dataContextHelper;
            }
        }
        #endregion
    }
    #endregion
}
