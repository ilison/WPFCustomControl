#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using Syncfusion.Data;
using Syncfusion.Data.Extensions;
using System;
using System.Collections;
using System.Reflection;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
#if WinRT ||UNIVERSAL
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.Windows.Controls;
#if WPF
using System.Data;
#endif
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    public class GridDataTopLevelGroup : TopLevelGroup
    {
        SfDataGrid datagrid;

        public GridDataTopLevelGroup(SfDataGrid grid, CollectionViewAdv collectionView)
            : base(collectionView)
        {
            datagrid = grid;
        }

        public override void Invalidate(int index, int count)
        {
            var rowIndex = this.datagrid.ResolveToRowIndex(index);
            for (var i = 0; i < count; i++)
            {
                this.datagrid.GridModel.UpdateDataRow(rowIndex);
                rowIndex++;
            }
        }
        protected override void Dispose(bool disposing)
        {
            this.datagrid = null;
            base.Dispose(disposing);          
        }

        public override int RelationsCount
        {
            get
            {
                return this.datagrid != null && this.datagrid.DetailsViewDefinition != null ? this.datagrid.DetailsViewDefinition.Count : 0;
            }
        }
    }

    public class GridDataVirtualizingTopLevelGroup : VirtualizingTopLevelGroup
    {
        SfDataGrid datagrid;

        public GridDataVirtualizingTopLevelGroup(SfDataGrid grid, CollectionViewAdv collectionView)
            : base(collectionView)
        {
            datagrid = grid;
        }

        public override void Invalidate(int index, int count)
        {
            var rowIndex = this.datagrid.ResolveToRowIndex(index);
            for (var i = 0; i < count; i++)
            {
                this.datagrid.GridModel.UpdateDataRow(rowIndex);
                rowIndex++;
            }
        }
        protected override void Dispose(bool disposing)
        {
            this.datagrid = null;
            base.Dispose(disposing);
        }

        public override int RelationsCount
        {
            get
            {
                return this.datagrid.DetailsViewDefinition != null ? this.datagrid.DetailsViewDefinition.Count : 0;
            }
        }
    }
}
