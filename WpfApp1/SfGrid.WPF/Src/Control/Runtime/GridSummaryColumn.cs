#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Text;
using Syncfusion.Data;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Windows;
using System.ComponentModel;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    /// <summary>
    /// Represents a class that defines the summary calculation based on particular column.
    /// </summary>
    [ClassReference(IsReviewed = false)]
    public class GridSummaryColumn :DependencyObject, ISummaryColumn
    {
        #region Dependency Registration
        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.CustomAggregate dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.CustomAggregate dependency property.
        /// </remarks>
        public static readonly DependencyProperty CustomAggregateProperty = DependencyProperty.Register("CustomAggregate", typeof(ISummaryAggregate), typeof(GridSummaryColumn), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.Format dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.Format dependency property.
        /// </remarks>
        public static readonly DependencyProperty FormatProperty = DependencyProperty.Register("Format", typeof(string), typeof(GridSummaryColumn), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.MappingName dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.MappingName dependency property.
        /// </remarks>
        public static readonly DependencyProperty MappingNameProperty = DependencyProperty.Register("MappingName", typeof(string), typeof(GridSummaryColumn), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.Name dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.Name dependency property.
        /// </remarks>
        public static readonly DependencyProperty NameProperty = DependencyProperty.Register("Name", typeof(string), typeof(GridSummaryColumn), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Identifies the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.SummaryType dependency property.
        /// </summary>
        /// <remarks>
        /// The identifier for the Syncfusion.UI.Xaml.Grid.GridSummaryColumn.SummaryType dependency property.
        /// </remarks>
        public static readonly DependencyProperty SummaryTypeProperty = DependencyProperty.Register("SummaryType", typeof(SummaryType), typeof(GridSummaryColumn), new PropertyMetadata(SummaryType.CountAggregate));

        #endregion

        #region ISummaryColumn Members

        /// <summary>
        /// Gets or sets the instance of <see cref="Syncfusion.UI.Xaml.Grid.ISummaryAggregate"/> to implement the custom summary.
        /// </summary>
        /// <value>
        /// An instance of <see cref="Syncfusion.UI.Xaml.Grid.ISummaryAggregate"/>. The default value is null.
        /// </value>        
        public ISummaryAggregate CustomAggregate
        {
            get
            {
                return (ISummaryAggregate)this.GetValue(CustomAggregateProperty) ;
            }
            set
            {
                this.SetValue(CustomAggregateProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the string that indicates how the summary value is formatted in display.
        /// </summary>
        /// <value>
        /// A string that specifies the format of summary value.The default value is <c>string.Empty</c>.
        /// </value>
        /// <example>
        /// 	<code lang="C#"><![CDATA[        
        ///  this.dataGrid.TableSummaryRows.Add(new GridSummaryRow()
        ///  {
        ///     Name="Total Products",
        ///     ShowSummaryInRow = true,
        ///     Title = "Total Products Count: {ProductCount}",
        ///     SummaryColumns = new ObservableCollection<ISummaryColumn>()
        ///     {
        ///         new GridSummaryColumn()
        ///         {
        ///             Name="ProductCount",
        ///             MappingName="ProductName",
        ///             SummaryType=SummaryType.CountAggregate,
        ///             Format="{Count:d}"
        ///         },
        ///     }
        /// });
        /// ]]></code>
        /// </example>
#if !WinRT && !UNIVERSAL
        [TypeConverter(typeof(GridSummaryFormatConverter))]
#endif
        public string Format
        {
            get
            {
                return (string)this.GetValue(FormatProperty);
            }
            set
            {
#if !WPF
                var formattedValue = value.SummaryFormatedString();
                this.SetValue(FormatProperty, formattedValue);
#else
                this.SetValue(FormatProperty, value);
#endif
            }
        }

        /// <summary>
        /// Gets or sets the corresponding <see cref="Syncfusion.UI.Xaml.Grid.GridColumn.MappingName"/> of the column.
        /// </summary>
        /// <value>
        /// A string that specifies the valid mapping name of column. The default value is <c>string.Empty</c>.
        /// </value>
        public string MappingName
        {
            get
            {
                return (string)this.GetValue(MappingNameProperty);
            }
            set
            {
                this.SetValue(MappingNameProperty, value);
            }
        }
          
        /// <summary>
        /// Gets or sets the name of summary column.
        /// </summary>
        /// <value>
        /// A string that specifies the name of the summary column. The default value is <c>string.Empty</c>.
        /// </value>
        /// <remarks>
        /// The name of the summary column and <see cref="Syncfusion.UI.Xaml.Grid.GridSummaryRow.Title"/> should be same for displaying summary value with title.
        /// </remarks>
        /// <example>
        /// 	<code lang="C#"><![CDATA[       
        /// 	//Here, the Title and Name have the same ProductCount string.
        ///     this.dataGrid.TableSummaryRows.Add(new GridSummaryRow()
        ///     {
        ///         Name="Total Products",
        ///         ShowSummaryInRow = true,
        ///         Title = "Total Products Count: {ProductCount}",
        ///         SummaryColumns = new ObservableCollection<ISummaryColumn>()
        ///         {
        ///             new GridSummaryColumn()
        ///             {
        ///                 Name="ProductCount",
        ///                 MappingName="ProductName",
        ///                 SummaryType=SummaryType.CountAggregate,
        ///                 Format="{Count:d}"
        ///             },
        ///         }
        ///     });    
        /// ]]></code>
        /// </example>
        public string Name
        {
            get
            {
                return (string)this.GetValue(NameProperty);
            }
            set
            {
                this.SetValue(NameProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets the aggregate type for summary calculation.
        /// </summary>
        /// <value>
        /// One of the <see cref="Syncfusion.Data.SummaryType"/> enumeration to specifies the aggregate type for summary calculation. The default value is <see cref="Syncfusion.Data.SummaryType.CountAggregate"/>.
        /// </value>
        public SummaryType SummaryType
        {
            get
            {
                return (SummaryType)this.GetValue(SummaryTypeProperty);
            }
            set
            {
                this.SetValue(SummaryTypeProperty, value);
            }
        }

        #endregion
    }

    internal static class GridSummaryFormatterExtenstion
    {
        public static string SummaryFormatedString(this object value)
        {
            var formatString = value.ToString();
            if (formatString.Length > 1)
            {
                var sb = new StringBuilder();
                var startIdx = formatString.IndexOf("\'") + 1;
                var endIdx = formatString.LastIndexOf("\'");
                if (startIdx > 0)
                {
                    sb.Append(formatString.Substring(0, startIdx - 1));
                }
                for (int i = startIdx; i < endIdx; i++)
                {
                    char c = formatString[i];
                    sb.Append(c);
                }
                if (sb.Length > 0)
                {
                    return sb.ToString();
                }
            }
            return formatString;
        }
    }

#if !WinRT && !UNIVERSAL
    class GridSummaryFormatConverter : TypeConverter
    {
        public GridSummaryFormatConverter()
        {
        }

        public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
        {
            return value.SummaryFormatedString();
        }
    }
#endif
}
