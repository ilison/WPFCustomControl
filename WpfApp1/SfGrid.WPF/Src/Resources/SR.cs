#region Copyright Syncfusion Inc. 2001-2017.
// Copyright Syncfusion Inc. 2001-2017. All rights reserved.
// Use of this code is subject to the terms of our license.
// A copy of the current license can be obtained at any time by e-mailing
// licensing@syncfusion.com. Any infringement will be prosecuted under
// applicable laws. 
#endregion
using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Resources;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
#if WinRT || UNIVERSAL
using Windows.UI.Xaml;
using Windows.ApplicationModel.Resources;
#endif

namespace Syncfusion.UI.Xaml.Grid
{
    sealed class SR
    {
#if UWP
        private ResourceLoader resources;
#else
        private ResourceManager resources;
#endif
        private static SR loader = null;

        //WPF-36897- we have consider the assembly name and namespace while read the resource file from the another assembly file.
        private SR()
        {
            Assembly assembly = null;
            string _namespace = string.Empty;
#if WPF
            if (Application.Current != null)
            {
                assembly = Application.Current.GetType().Assembly;
                _namespace = assembly.FullName.Split(new char[] { ',' })[0];
            }
#endif
            CreateLoader(assembly, _namespace);
        }

        //WPF-36897- we have consider the assembly name and namespace while read the resource file from the another assembly file.
        private SR(Assembly assembly, string _namespace)
        {
            CreateLoader(assembly, _namespace);
        }


        private void CreateLoader(Assembly assembly, string _namespace)
        {
#if UWP
            //http://msdn.microsoft.com/en-us/library/windows/apps/xaml/Hh965329%28v=win.10%29.aspx
            ResourceLoader localizedManager = GetLocalizedResourceManager();
#else
            //WPF-21960 while testcases it is possible for Application.Current as Null.avoid exception need to check whether it is null or not. 
            System.Resources.ResourceManager localizedManager = null;
            if (assembly != null)
            {
                localizedManager = GetLocalizedResourceManager(assembly, _namespace, false);
                if (localizedManager == null)
                    localizedManager = GetLocalizedResourceManager(assembly, _namespace, true);
            }
#endif
            if (localizedManager == null)
            {
#if UWP
                if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    resources = null;
                    return;
                }
                //UWP-230 - There is no any current view, so GetForViewIndependentUse is used instead GetForCurrentView method.
                resources = ResourceLoader.GetForViewIndependentUse("Syncfusion.SfGrid.UWP/Syncfusion.SfGrid.UWP.Resources");
#else
                this.resources = Syncfusion.UI.Xaml.Grid.Resources.Syncfusion_SfGrid_Wpf.ResourceManager;
#endif
            }
            else
            {
                this.resources = localizedManager;
            }
        }
#if WPF
        
        /// <summary>
        /// SetResources method was added for perform the Localization when Assembly name was defer from default namespace name
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="_namespace"></param>
        public static void SetResources(Assembly assembly, string _namespace)
        {
            var loader = GetLoader(assembly,_namespace);

            System.Resources.ResourceManager localizedManager = loader.resources;

            if (localizedManager == null)
                localizedManager = GetLocalizedResourceManager(assembly, _namespace, true);

            if (localizedManager == null)
            {
                assembly = Application.Current.GetType().Assembly;
                _namespace = assembly.FullName.Split(new char[] { ',' })[0];
            }

            if (localizedManager == null)
                localizedManager = GetLocalizedResourceManager(assembly, _namespace, false);
            if (localizedManager == null)
                localizedManager = GetLocalizedResourceManager(assembly, _namespace, true);

            if (localizedManager == null)
            {
                loader.resources = Syncfusion.UI.Xaml.Grid.Resources.Syncfusion_SfGrid_Wpf.ResourceManager;
            }
            else
            {
                loader.resources = localizedManager;
            }
        }
#endif
        // Methods
        private static SR GetLoader()
        {
            lock (typeof(SR))
            {
                if (SR.loader == null)
                    SR.loader = new SR();
                return SR.loader;
            }
        }

        //WPF-36897 we have to consider the assembly and namespace.while load the dresource from the another assembly.
        private static SR GetLoader(Assembly assembly, string _namespace)
        {
            lock (typeof(SR))
            {
                if (SR.loader == null)
                    SR.loader = new SR(assembly, _namespace);
                return SR.loader;
            }
        }
#if UWP
        private static ResourceLoader GetLocalizedResourceManager()
        {
            try
            {
                if (Application.Current == null || Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                {
                    return null;
                }

                ResourceLoader manager = null;
                var sampleresourcename = "Syncfusion.SfGrid.UWP.Resources";

                //if (Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride != "")
                //    sampleresourcename += "." + Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride;

                //UWP-4494, Need to check the culture name, for using the various culture in a single application.
                if (CultureInfo.CurrentUICulture.Name != "en-US")
                {
                    sampleresourcename += "." + CultureInfo.CurrentUICulture.Name;
                }
                manager = ResourceLoader.GetForViewIndependentUse(sampleresourcename);

                return manager;
            }
            catch (Exception) { }
            return null;
        }

        public static string GetString(CultureInfo culture, string name)
        {
            SR sr = SR.GetLoader();
            if (sr == null)
                return null;
            if (sr.resources == null)
                return null;

            return sr.resources.GetString(name);
        }
#else
        //GetLocalizedResourceManager method parameter was changed for getting the namespace name and perform the Localization when Assembly name was defer from default namespace name
        private static System.Resources.ResourceManager GetLocalizedResourceManager(Assembly assembly, string _namespace, bool tryDiffBaseName)
        {
                try
                {
                    var controlAssembly = Assembly.GetExecutingAssembly();
                    System.Resources.ResourceManager manager = null;
                //WPF-#6897 while fetch the resource file from the another assembly. ManiFeast resource missing exception throws due to execution of the else condition
                //makes the inavlid format path. So the condition is revert back to oringinal.
                if (!tryDiffBaseName)
                    {
                        var found = false;
                        var sampleresourcename = string.Format("{0}.Resources.{1}.resources", assembly.FullName.Split(new char[] { ',' })[0], controlAssembly.FullName.Split(new char[] { ',' })[0]);
                        foreach (var resourceName in assembly.GetManifestResourceNames())
                        {
                            if (resourceName.Equals(string.Format("{0}.Resources.{1}.resources", _namespace,
                                                                  controlAssembly.FullName.Split(new char[] { ',' })[0])))
                            { found = true; break; }
                        }
                        // Here there is no need to check CurrentUICulture not is “en-US” . Already we have checked only found is true
                        if (found)
                        {
                            manager = new System.Resources.ResourceManager(string.Format("{0}.Resources.{1}", _namespace, controlAssembly.FullName.Split(new char[] { ',' })[0]), assembly);
                        }
                        else
                        {
                            var resourcemanagerName = string.Format("Syncfusion.UI.Xaml.Grid.Resources.{0}", controlAssembly.FullName.Split(new char[] { ',' })[0]);
                            manager = new System.Resources.ResourceManager(resourcemanagerName, controlAssembly);
                        }
                    }

                    else
                    {
                        manager = new System.Resources.ResourceManager(string.Format("{0}.{1}", _namespace,
                                                                                     controlAssembly.FullName.Split(new char[] { ',' })[0]), assembly);
                    }
                    if (manager != null)
                    {
                        var currentUICulture = CultureInfo.CurrentUICulture;
                        if (manager.GetResourceSet(currentUICulture, true, true) != null)
                        {
                            return manager;
                        }
                    }
                }
                catch (Exception)
                {
                    return null;
                }
            return null;
        }

        public static void ReleaseResources()
        {
            SR.loader.resources.ReleaseAllResources();
        }

        public static string GetString(CultureInfo culture, string name, params object[] args)
        {
            SR sr = SR.GetLoader();
            string value;

            if (sr == null)
                return null;

            try
            {
                value = sr.resources.GetString(name, culture);
                if (value != null && args != null && args.Length > 0)
                    return String.Format(value, args);

                return value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return name;
            }
        }
        
        public static string GetString(string name)
        {
            return SR.GetString(null, name);
        }
        
        public static string GetString(string name, params object[] args)
        {
            return SR.GetString(null, name, args);
        }
        
        public static string GetString(CultureInfo culture, string name)
        {
            SR sr = SR.GetLoader();
            if (sr == null)
                return null;
            string value = "";
            try
            {
                value = sr.resources.GetString(name, culture);
            }
            catch
            {
                value = Syncfusion.UI.Xaml.Grid.Resources.Syncfusion_SfGrid_Wpf.ResourceManager.GetString(name);
            }
            return value;
        }

        public static object GetObject(CultureInfo culture, string name)
        {
            SR sr = SR.GetLoader();
            if (sr == null)
                return null;
            return sr.resources.GetObject(name, culture);
        }
        
        public static object GetObject(string name)
        {
            return SR.GetObject(null, name);
        }
        
        public static bool GetBoolean(CultureInfo culture, string name)
        {
            bool value;
            SR sr = SR.GetLoader();
            object obj;
            value = false;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Boolean)
                    value = ((bool)obj);
            }
            return value;
        }
        
        public static bool GetBoolean(string name)
        {
            return SR.GetBoolean(name);
        }
        
        public static byte GetByte(CultureInfo culture, string name)
        {
            byte value;
            SR sr = SR.GetLoader();
            object obj;
            value = (byte)0;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Byte)
                    value = ((byte)obj);
            }
            return value;
        }
        
        public static byte GetByte(string name)
        {
            return SR.GetByte(null, name);
        }
        
        public static char GetChar(CultureInfo culture, string name)
        {
            char value;
            SR sr = SR.GetLoader();
            object obj;
            value = (char)0;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Char)
                    value = (char)obj;
            }
            return value;
        }
        
        public static char GetChar(string name)
        {
            return SR.GetChar(null, name);
        }
        
        public static double GetDouble(CultureInfo culture, string name)
        {
            double value;
            SR sr = SR.GetLoader();
            object obj;
            value = 0.0;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Double)
                    value = ((double)obj);
            }
            return value;
        }
        
        public static double GetDouble(string name)
        {
            return SR.GetDouble(null, name);
        }
        
        public static float GetFloat(CultureInfo culture, string name)
        {
            float value;
            SR sr = SR.GetLoader();
            object obj;
            value = 0.0f;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Single)
                    value = ((float)obj);
            }
            return value;
        }
        
        public static float GetFloat(string name)
        {
            return SR.GetFloat(null, name);
        }
        
        public static int GetInt(string name)
        {
            return SR.GetInt(null, name);
        }
        
        public static int GetInt(CultureInfo culture, string name)
        {
            int value;
            SR sr = SR.GetLoader();
            object obj;
            value = 0;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Int32)
                    value = ((int)obj);
            }
            return value;
        }
        
        public static long GetLong(string name)
        {
            return SR.GetLong(null, name);
        }
        
        public static long GetLong(CultureInfo culture, string name)
        {
            Int64 value;
            SR sr = SR.GetLoader();
            object obj;
            value = ((Int64)0);
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Int64)
                    value = ((Int64)obj);
            }
            return value;
        }

        public static short GetShort(CultureInfo culture, string name)
        {
            short value;
            SR sr = SR.GetLoader();
            object obj;
            value = (short)0;
            if (sr != null)
            {
                obj = sr.resources.GetObject(name, culture);
                if (obj is System.Int16)
                    value = ((short)obj);
            }
            return value;
        }
        
        public static short GetShort(string name)
        {
            return SR.GetShort(null, name);
        }
#endif

    }
}

