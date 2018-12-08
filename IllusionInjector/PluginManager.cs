using IllusionPlugin;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace IllusionInjector
{
    public static class PluginManager
    {
        private static List<IPlugin> _Plugins = null;

        /// <summary>
        /// Gets the list of loaded plugins and loads them if necessary.
        /// </summary>
        public static IEnumerable<IPlugin> Plugins
        {
            get
            {
                if(_Plugins == null)
                {
                    LoadPlugins();
                }
                return _Plugins;
            }
        }


        private static void LoadPlugins()
        {
            string pluginDirectory = Path.Combine(Environment.CurrentDirectory, "Plugins");

            // Process.GetCurrentProcess().MainModule crashes the game and Assembly.GetEntryAssembly() is NULL,
            // so we need to resort to P/Invoke
            string exeName = Path.GetFileNameWithoutExtension(AppInfo.StartupPath);
            Console.WriteLine(exeName);
            _Plugins = new List<IPlugin>();

            if (!Directory.Exists(pluginDirectory)) return;
            
            String[] files = Directory.GetFiles(pluginDirectory, "*.dll");
            foreach (var s in files)
            {
                _Plugins.AddRange(LoadPluginsFromFile(Path.Combine(pluginDirectory, s), exeName));
            }


            // DEBUG
            Console.WriteLine("Running on Unity {0} using {1}", UnityEngine.Application.unityVersion, GetFrameworkVersion());
            Console.WriteLine("-----------------------------");
            Console.WriteLine("Loading plugins from {0} and found {1}", pluginDirectory, _Plugins.Count);
            Console.WriteLine("-----------------------------");
            foreach (var plugin in _Plugins)
            {

                Console.WriteLine(" {0}: {1}", plugin.Name, plugin.Version);
            }
            Console.WriteLine("-----------------------------");
        }

        private static IEnumerable<IPlugin> LoadPluginsFromFile(string file, string exeName)
        {
            List<IPlugin> plugins = new List<IPlugin>();

            if (!File.Exists(file) || !file.EndsWith(".dll", true, null))
                return plugins;

            try
            {
                Assembly assembly = Assembly.LoadFrom(file);

                foreach (Type t in assembly.GetTypes())
                {
                    if (IsValidPlugin(t))
                    {
                        try
                        {

                            IPlugin pluginInstance = Activator.CreateInstance(t) as IPlugin;
                            string[] filter = null;

                            if (pluginInstance is IEnhancedPlugin)
                            {
                                filter = ((IEnhancedPlugin)pluginInstance).Filter;
                            }
                            
                            if(filter == null || Enumerable.Contains(filter, exeName, StringComparer.OrdinalIgnoreCase))
                                plugins.Add(pluginInstance);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("[WARN] Could not load plugin {0} in {1}! {2}", t.FullName, Path.GetFileName(file), e);
                        }
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine("[ERROR] Could not load {0}! {1}", Path.GetFileName(file), e);
            }

            return plugins;
        }

        private static bool IsValidPlugin(Type type)
        {
            return typeof(IPlugin).IsAssignableFrom(type)
                && !type.IsAbstract 
                && !type.IsInterface 
                && type.GetConstructor(Type.EmptyTypes) != null;
        }

        private static string GetFrameworkVersion()
        {
            var version = Environment.Version.ToString();

            try
            {
                switch (version)
                {
                    case "2.0.50727.1433":
                        return ".NET 3.5 Equivalent";
                    case "4.0.30319.17020":
                        // For reasons unknown, switching to netstandard seems to set this back to .NET 4.0
                        var netstandard = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a => a.GetName().Name == "netstandard");
                        if (netstandard != null)
                            return $".NET Standard {netstandard.GetName().Version.ToString(2)}";

                        goto default;
                    case "4.0.30319.42000":
                        return ".NET 4.x";
                    default:
                        return $".NET Framework {version}";
                }
            }
            catch
            {
                // In case something goes wrong, return the best we can guess
                return version;
            }
        }

        public class AppInfo
        {
            [DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = false)]
            private static extern int GetModuleFileName(HandleRef hModule, StringBuilder buffer, int length);
            private static HandleRef NullHandleRef = new HandleRef(null, IntPtr.Zero);
            public static string StartupPath
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder(260);
                    GetModuleFileName(NullHandleRef, stringBuilder, stringBuilder.Capacity);
                    return stringBuilder.ToString();
                }
            }
        }

    }
}
