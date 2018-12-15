#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Markup;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Syncfusion.SfGrid.WPF")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Syncfusion Inc.")]
[assembly: AssemblyProduct("Syncfusion Essential SfGrid WPF")]
[assembly: AssemblyCopyright("Copyright (c) 2001-2017 Syncfusion. Inc,")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("a9c681fd-9090-460b-ae62-3364b03553b0")]

//In order to begin building localizable applications, set 
//<UICulture>CultureYouAreCodingWith</UICulture> in your .csproj file
//inside a <PropertyGroup>.  For example, if you are using US english
//in your source files, set the <UICulture> to en-US.  Then uncomment
//the NeutralResourceLanguage attribute below.  Update the "en-US" in
//the line below to match the UICulture setting in the project file.

//[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.Satellite)]
[assembly: NeutralResourcesLanguage("en-US", UltimateResourceFallbackLocation.MainAssembly)]


[assembly: ThemeInfo(
    ResourceDictionaryLocation.None, //where theme specific resource dictionaries are located
    //(used if a resource is not found in the page, 
    // or application resource dictionaries)
    ResourceDictionaryLocation.SourceAssembly //where the generic resource dictionary is located
    //(used if a resource is not found in the page, 
    // app, or any theme specific resource dictionaries)
)]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]

#if SyncfusionFramework4_6
[assembly: AssemblyVersion("15.3460.0.26")]
#elif SyncfusionFramework4_5_1
[assembly: AssemblyVersion("15.3451.0.26")]
#elif SyncfusionFramework4_5
[assembly: AssemblyVersion("15.3450.0.26")]
#elif SyncfusionFramework4_0
[assembly: AssemblyVersion("15.3400.0.26")]
#elif SyncfusionFramework3_5
[assembly: AssemblyVersion("15.3350.0.26")]
#elif SyncfusionFramework2_0
[assembly: AssemblyVersion("15.3200.0.26")]
#else
[assembly: AssemblyVersion("15.3350.0.26")]
#endif




[assembly: AssemblyDelaySign(false)]
[assembly: AssemblyKeyFile(@"C:\Program Files (x86)\Syncfusion\Essential Studio\15.3.0.26\Common\Keys\SFKey.snk")]



[assembly: XmlnsPrefix("http://schemas.syncfusion.com/wpf", "Syncfusion")]
[assembly: XmlnsDefinition("http://schemas.syncfusion.com/wpf", "Syncfusion.UI.Xaml.Grid")]
[assembly: XmlnsDefinition("http://schemas.syncfusion.com/wpf", "Syncfusion.UI.Xaml.TreeGrid")]
[assembly: XmlnsDefinition("http://schemas.syncfusion.com/wpf", "Syncfusion.UI.Xaml.Grid.RowFilter")]
[assembly: XmlnsDefinition("http://schemas.syncfusion.com/wpf", "Syncfusion.UI.Xaml.Controls.DataPager")]
