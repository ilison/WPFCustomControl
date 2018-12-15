#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Linq;
using System.Collections.Generic;
#if UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI;
#else
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
#endif


namespace Syncfusion.UI.Xaml.Grid
{
    [ClassReference(IsReviewed = false)]
    public class VisiblityConverter : IValueConverter
    {
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif

        {
            if (value is Visibility && ((Visibility)value) == Visibility.Visible)
                return true;
            return false;
        }
#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }



    public class CaseSensitiveConverter : IValueConverter
    {
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif

        {
            if ((bool) value)
#if WPF
                return Brushes.White;
            return Brushes.Gray;
#else
                return new SolidColorBrush(Colors.White);
            return new SolidColorBrush(Colors.Gray);
#endif

        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }

    [ClassReference(IsReviewed = false)]
    public class TextVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
        string text;
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value is string)
            {
                text = value.ToString();
                if (!(string.IsNullOrEmpty(value.ToString())) && parameter != null && parameter.Equals("searchIcon"))
                    return Visibility.Collapsed;
                else if ((string.IsNullOrEmpty(value.ToString())) && parameter != null && parameter.Equals("deletebtn"))
                    return Visibility.Collapsed;
            }
            return Visibility.Visible;
        }


#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class ItemsSourceCountConverter : IValueConverter
    {

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (Int32.Parse(value.ToString()) > 0)
                return Visibility.Collapsed;
            return Visibility.Visible;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

    }

    [ClassReference(IsReviewed = false)]
    public class ReverseVisibilityConverter : IValueConverter
    {

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value != null && (bool)value)
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }


#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }

#if WinRT || UNIVERSAL
    /// <summary>
    /// Convertor for empty string to visibility
    /// </summary>
    public class EmptyStringToVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts into visibility
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value.ToString() == string.Empty)
                return Visibility.Visible;
            else
                return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts the value back into the object
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
    public class ErrorToVisiblityConverter : IValueConverter
    {
        /// <summary>
        /// Converts the error value to Visiblity
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty(value.ToString()))
                return Visibility.Collapsed;
            else
                return Visibility.Visible;
        }

        /// <summary>
        /// Converts the visibity value to bool
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="language"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible)
                return true;
            return false;
        }
    }

#endif
    [ClassReference(IsReviewed = false)]
    public class LoadingVisiblityConverter : IValueConverter
    {
        // isRunning flag is introduced to clear source warning that occur when filter popup is opened.
        bool isRunning = false;
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
#if UWP
            return !((bool)value);
#else
            if (parameter != null && parameter is System.Windows.Media.Animation.Storyboard)
            {
                if (!(bool)value)
                {
                    (parameter as System.Windows.Media.Animation.Storyboard).Begin();
                    isRunning = true;
                    return Visibility.Visible;
                }
                else
                {
                    if (isRunning)
                        (parameter as System.Windows.Media.Animation.Storyboard).Stop();
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
#endif

        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }

    [ClassReference(IsReviewed = false)]
    public class ListItemsVisiblityConverter : IValueConverter
    {

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value != null)
            {
                if ((value as IEnumerable<object>).Any() && parameter != null && parameter.Equals("ItemsControl"))
                    return Visibility.Visible;
                else if (!(value as IEnumerable<object>).Any() && parameter != null && parameter.Equals("NoMatchText"))
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else
                return Visibility.Collapsed;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }

    [ClassReference(IsReviewed = false)]
    public class AdvancedFiltersButtonVisibiltyConverter : IValueConverter
    {
        #region IValueConverter Members
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            var filtermode = (FilterMode)value;
            if (parameter != null && parameter.ToString() == "reverse")
            {
                if (filtermode == FilterMode.Both || filtermode == FilterMode.CheckboxFilter)
                    return Visibility.Collapsed;
                return Visibility.Visible;
            }
            else if (filtermode == FilterMode.Both)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class FilteredFromCheckVisibilityConverter : IValueConverter
    {
        #region IValueConverter Members
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            var filteredfrom = (FilteredFrom)value;
            if (parameter != null && parameter.ToString() == "reverse")
            {
                if (filteredfrom == FilteredFrom.AdvancedFilter)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
            else if (filteredfrom == FilteredFrom.CheckboxFilter)
                return Visibility.Visible;
            return Visibility.Collapsed;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    [ClassReference(IsReviewed = false)]
    public class FilterValueComboEnableConverter : IValueConverter
    {
        #region IValueConverter Members
#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value != null && (value.ToString() == GridResourceWrapper.Null || value.ToString() == GridResourceWrapper.NotNull
                || value.ToString() == GridResourceWrapper.Empty || value.ToString() == GridResourceWrapper.NotEmpty))
                return false;
            else
                return true;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }

        #endregion
    }

#if WinRT || UNIVERSAL
    [ClassReference(IsReviewed = false)]
    public class SortBorderBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value != null && (bool)value && parameter != null && parameter.Equals("background"))
                return new SolidColorBrush(Colors.AliceBlue);
            else if (value != null && (bool)value && parameter != null && parameter.Equals("borderbrush"))
                return new SolidColorBrush(Colors.CadetBlue);
            else
                return new SolidColorBrush(Colors.Transparent);

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
#endif

    [ClassReference(IsReviewed = false)]
    public class HeightToMarginConverter : IValueConverter
    {

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value != null)
#if UWP
                return new Thickness(4, (double)value * 2.5, 4, 4);
#else
                //WPF-30131- Set the Top margin for ItemPresenter based on ActualHeigt of SelectAll CheckBox
                return new Thickness(4, (double)value + 12, 4, 4);
#endif
            return null;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }

    [ClassReference(IsReviewed = false)]
    public class ResourceNameConverter : IValueConverter
    {

#if UWP
        public object Convert(object value, Type targetType, object parameter, string language)
#else
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            if (value != null)
            {
                if ((bool)value)
                    return GridResourceWrapper.Done;
                return GridResourceWrapper.Cancel;
            }

            return null;
        }

#if UWP
        public object ConvertBack(object value, Type targetType, object parameter, string language)
#else
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
#endif
        {
            throw new NotImplementedException();
        }
    }
}
