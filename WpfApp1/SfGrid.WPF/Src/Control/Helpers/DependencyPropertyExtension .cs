#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Collections.Generic;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
#else
using System.Windows;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    public static class GridDependencyProperty 
    {
#if UWP
        private static Dictionary<DependencyProperty, GridPropertyMetadata> propertyMetadatas;

        static GridDependencyProperty()
        {
            propertyMetadatas = new Dictionary<DependencyProperty, GridPropertyMetadata>();
        }
  
        public static GridPropertyMetadata GetDependencyPropertyMetadata(DependencyProperty p)
        {
            return propertyMetadatas[p];
        }
#endif
        public static DependencyProperty Register(string name, Type propertyType, Type ownerType, GridPropertyMetadata propertyMetadata)
        {
#if UWP
            propertyMetadata.DependencyPropertyName = name;
#endif
            var dependencyProperty = DependencyProperty.Register(name, propertyType, ownerType, propertyMetadata);
#if UWP
            propertyMetadatas.Add(dependencyProperty, propertyMetadata);
#endif
            return dependencyProperty;
        }

    }

    public class GridPropertyMetadata : PropertyMetadata
    {
#if UWP
        public string DependencyPropertyName { get; set; }
#endif
        public PropertyChangedCallback DependencyPropertyChangedCallback { get; set; }

        public GridPropertyMetadata(object defaultValue)
            : base(defaultValue, OnInternalDependencyPropertyChanged)
        {
            
        }

        public GridPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback)
            : base(defaultValue, OnInternalDependencyPropertyChanged)
        {
            DependencyPropertyChangedCallback = propertyChangedCallback;
        }

#if WPF
        public GridPropertyMetadata(object defaultValue, PropertyChangedCallback propertyChangedCallback, CoerceValueCallback coerceValueCallback)
            : base(defaultValue, OnInternalDependencyPropertyChanged, coerceValueCallback)
        {
            DependencyPropertyChangedCallback = propertyChangedCallback;
        }
#endif

        private static void OnInternalDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
#if UWP
             var framework = e.Property.GetMetadata(d.GetType());
            var metadata =  GridDependencyProperty.GetDependencyPropertyMetadata(e.Property);
#else
            var metadata = e.Property.GetMetadata(d) as GridPropertyMetadata;
            if(metadata == null)
                return;
#endif
            metadata.OnDependencyPropertyChanged(d, e);
        }

        private void OnDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (DependencyPropertyChangedCallback != null)
                DependencyPropertyChangedCallback(d, e);
            
            if (d is INotifyDependencyPropertyChanged)
#if UWP
                (d as INotifyDependencyPropertyChanged).OnDependencyPropertyChanged(DependencyPropertyName, e);
#else
                (d as INotifyDependencyPropertyChanged).OnDependencyPropertyChanged(e.Property.Name, e);
#endif
        }
    }

    public interface INotifyDependencyPropertyChanged
    {
        void OnDependencyPropertyChanged(string propertyName, DependencyPropertyChangedEventArgs e);
    }
}
