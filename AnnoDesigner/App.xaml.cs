using System.IO;
using System.Reflection;
using System.Windows;

namespace AnnoDesigner
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
        : Application
    {
        public static string ExecutablePath
        {
            get
            {
                return Assembly.GetEntryAssembly().Location;
            }

        }

        public static string ApplicationPath
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            }
        }

        public static string FilenameArgument
        {
            get;
            private set;
        }


        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            // retrieve file argument if given
            if (e.Args.Length > 0)
            {
                FilenameArgument = e.Args[0];
            }
        }
    }
}
