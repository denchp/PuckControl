using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;

[assembly: CLSCompliant(true)]
namespace PuckControl
{
    internal static class Startup
    {
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
            var ex = (Exception)e.ExceptionObject;

            using (StreamWriter w = File.AppendText(folderpath + "log.txt"))
            {
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
                        ReflectionTypeLoadException reflectionError;
                        if ((reflectionError = innerEx as ReflectionTypeLoadException) != null)
                        {
                            foreach (var exception in reflectionError.LoaderExceptions)
                                Log(exception.Message, w);
                        }

                    }

                }
            }

            //Attempt to create a new issue on GitHub Repo.

            string bugReport = "";

            bugReport = "Message: " + ex.Message + "\r\n";
            bugReport += "Stack Trace:\r\n" + ex.StackTrace;
            if (ex.InnerException != null)
            {
                bugReport += "Inner Exceptions:";

                Exception innerEx = ex;
                while ((innerEx = innerEx.InnerException) != null)
                {
                    bugReport += innerEx.Message + "\r\n";
                   ReflectionTypeLoadException reflectionError;
                   if ((reflectionError = innerEx as ReflectionTypeLoadException) != null)
                    {
                        foreach (var exception in reflectionError.LoaderExceptions)
                            bugReport += exception.Message + "\r\n";
                    }

                }

            }
#if !DEBUG
            try
            {
                var encoding = new ASCIIEncoding();
                var bugRequest = (HttpWebRequest)WebRequest.Create("http://www.headsup.technology/BugReport/Create");

                string issueData = "title=Automated Bug Report";
                issueData += "&body=" + bugReport;
                issueData += "&labels=Bug";

                byte[] data = encoding.GetBytes(issueData);

                bugRequest.Method = "POST";
                bugRequest.ContentType = "application/x-www-form-urlencoded";
                bugRequest.ContentLength = data.Length;

                using (Stream stream = bugRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            catch { }
#endif
        }

        public static void Log(string p, StreamWriter w)
        {
            w.WriteLine("{0} - {1}", DateTime.Now, p);
        }
    }
}
