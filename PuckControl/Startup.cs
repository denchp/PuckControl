using System;
using System.IO;
using System.Reflection;
using System.Threading;

[assembly: CLSCompliant(true)]
namespace PuckControl
{
    internal static class Startup
    {
        private static Thread SplashThread;

        [System.STAThreadAttribute()]
        public static void Main()
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            string folderpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (!Directory.Exists(folderpath + @"\PuckControl\"))
                Directory.CreateDirectory(folderpath+ @"\PuckControl\");
            
            PuckControl.App app = new PuckControl.App();
            app.InitializeComponent();

            @app.Run();
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            string folderpath = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + @"\PuckControl\";
            if (!Directory.Exists(folderpath))
                folderpath = @"C:\";

            using (StreamWriter w = File.AppendText(folderpath + "log.txt"))
            {
                var ex = (Exception)e.ExceptionObject;

                Log(ex.Message, w);
                Log("Stack trace:", w);
                Log(ex.StackTrace, w);

                if (ex.InnerException != null)
                {
                    Log("Inner Exceptions:", w);
                    Exception innerEx = ex;
                    while ((innerEx = innerEx.InnerException) != null)
                    {
                        Log(innerEx.Message, w);
                        if (innerEx is ReflectionTypeLoadException)
                        {
                            var error = innerEx as ReflectionTypeLoadException;
                            foreach(var exception in error.LoaderExceptions)
                                Log(exception.Message, w);
                        }
                            
                    }

                }
            }
        }

        public static void Log(string p, StreamWriter w)
        {
            w.WriteLine("{0} - {1}", DateTime.Now, p);
        }
    }
}
