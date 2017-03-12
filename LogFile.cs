//Copyright (c) 2012 Stefan Moebius (mail@stefanmoebius.de)

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace SystemEx.FileIO.Logging
{
    #pragma warning disable 1591 // do not show compiler warnings of the missing descriptions
    public enum LogLevel: int
    {
        NONE  = 0,
        ERROR = 1,
        WARN  = 2,
        DEBUG = 4
    }
    #pragma warning restore 1591

    /// <summary>
    /// Class which provides simple logging functions
    /// </summary>
    public static class LogFile // Simple logging functions
    {
        static StreamWriter sw = null;
        static private LogLevel logLevel = LogLevel.DEBUG;
        static string sApplicationName = "";
        static string sFile = "";
        static bool bCache = false;

        /// <summary>
        /// Opens a log file. 
        /// sFilename : If empty appname-yyyy-MM-dd.log (in temporary directory) is used.
        /// sAppName  : If empty, the name of the application is used.
        /// bCreate   : If true and the file already exits the file will be overwritten, otherwise it will be created.
        /// level     : Log level, the default is DEBUG. If it is set to WARN then only Warnings and errors are logged. If it is set to ERROR only errors are logged.
        /// bUseCache : The default is false. If true the log file may be not updated immediatly. For crashes this can be a probelm!
        /// </summary>
        static public bool OpenLog(string sFilename, string sAppName, bool bCreate, LogLevel level, bool bUseCache)
        {

            bool bResult = false;
            try
            {
                bCache = bUseCache;
                logLevel = level;
                sFile = sFilename;

                if (sw != null) // close old file, if any
                {
                    sw.Flush();
                    sw.Close();
                }

                sApplicationName = sAppName;
                if (sAppName == "")
                {
                    sApplicationName = Process.GetCurrentProcess().ProcessName;
                }

                if (sFilename == "")
                {
                    sFile = Path.GetTempPath() + "\\" + sApplicationName + "-" + System.DateTime.Today.ToString("yyyy-MM-dd") + ".log";
                }

                if (bCreate == true)
                {
                    sw = File.CreateText(sFile);
                }
                else
                {
                    sw = File.AppendText(sFile);
                }
                if (sw != null)
                {
                    sw.WriteLine();
                    sw.WriteLine("===============================================================");
                    sw.WriteLine("Logging for " + sApplicationName + " started at " + DateTime.Now.ToString());
                    sw.WriteLine("OS: " + System.Environment.OSVersion);
                    sw.WriteLine("LOGLEVEL: " + logLevel.ToString());
                    sw.WriteLine(Process.GetCurrentProcess().MainModule.FileVersionInfo);
                    //sw.WriteLine("===============================================================");
                    sw.Flush();
                    bResult = true;
                }

            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }


        /// <summary>
        /// Opens a log file. 
        /// sFilename : If empty appname-yyyy-MM-dd.log is used.
        /// sAppName  : If empty, the name of the application is used.
        /// bCreate   : If true and the file already exits the file will be overwritten, otherwise it will be created.
        /// level     : Log level, the default is DEBUG. If it is set to WARN then only Warnings and errors are logged. If it is set to ERROR only errors are logged.
        /// </summary>
        static public bool OpenLog(string sFilename, string sAppName, bool bCreate, LogLevel level)
        {
            return OpenLog(sFilename, sAppName, bCreate, level, false);
        }

        /// <summary>
        /// Opens a log file. 
        /// sFilename : If empty appname-yyyy-MM-dd.log is used.
        /// bCreate   : If true and the file already exits the file will be overwritten, otherwise it will be created.
        /// level     : Log level, the default is DEBUG. If it is set to WARN then only Warnings and errors are logged. If it is set to ERROR only errors are logged.
        /// </summary>
        static public bool OpenLog(string sFilename, bool bCreate, LogLevel level)
        {
            return OpenLog(sFilename, "", bCreate, level, false);
        }

        /// <summary>
        /// Opens a log file. 
        /// </summary>
        static public bool OpenLog()
        {
            return OpenLog("", "", false, LogLevel.DEBUG, false);
        }

        /// <summary>
        /// gets or sets the current log level
        /// </summary>
        public static LogLevel LogLevel
        {
            get
            {
                return logLevel;
            }
            set
            {
                logLevel = value;
            }
        }

        /// <summary>
        /// Flushes the write buffer. Only necessary if bUseCache is set to true.
        /// </summary>
        static public bool Flush()
        {
            bool bResult = false;
            try
            {
                if (sw != null)
                {
                    sw.Flush();
                    bResult = true;
                }
                
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Terminates the logging.
        /// </summary>
        static public bool Terminate()
        {
            bool bResult = false;
            try
            {
                if (sw != null)
                {
                    sw.WriteLine("===============================================================");
                    sw.WriteLine("Logging ended");
                    //sw.WriteLine("===============================================================");
                    sw.Flush();
                    sw.Close();
                    bResult = true;
                }                
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Writes the debug text to log file. Note: The log level must be DEBUG.
        /// </summary>
        static public bool Debug(string sDebugText)
        {
            bool bResult = false;
            try
            {
                if (sw != null)
                {
                    if (logLevel == LogLevel.DEBUG)
                    {
                        sw.WriteLine(DateTime.Now.ToString() + " [DEBUG] " + sDebugText);
                        if (bCache == false)
                        {
                            sw.Flush();
                        }
                    }
                    bResult = true;
                }
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Writes the warn text to log file. Note: The log level must be at least DEBUG or WARN.
        /// </summary>
        static public bool Warn(string sWarnText)
        {
            bool bResult = false;
            try
            {
                if (sw != null)
                {
                    if (logLevel == LogLevel.DEBUG || logLevel == LogLevel.WARN)
                    {
                        sw.WriteLine(DateTime.Now.ToString() + " [WARN] " + sWarnText);
                        if (bCache == false)
                        {
                            sw.Flush();
                        }
                    }
                    bResult = true;                 
                }
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Writes the error text to log file. Note: This does nothing if log level is NONE.
        /// </summary>
        static public bool Error(string sErrText, Exception exp)
        {
            bool bResult = false;
            try
            {
                if (sw != null)
                {
                    if ((logLevel == LogLevel.DEBUG) || (logLevel == LogLevel.WARN) || (logLevel == LogLevel.DEBUG))
                    {
                        sw.WriteLine(DateTime.Now.ToString() + " [ERROR] " + sErrText);
                        if (exp != null)
                        {
                            //sw.WriteLine("====== Exception ======");
                            sw.WriteLine("Message: " + exp.Message);
                            sw.WriteLine("Source: " + exp.Source);
                            sw.WriteLine(exp.StackTrace);
                            //sw.WriteLine("=======================");
                        }
                        if (bCache == false)
                        {
                            sw.Flush();
                        }
                    }
                    bResult = true;
                }
            }
            catch
            {
                bResult = false;
            }
            return bResult;
        }

        /// <summary>
        /// Writes the error text to log file. Note: This does nothing if log level is NONE.
        /// </summary>
        static public bool Error(string sErrText)
        {
            return Error(sErrText, null);
        }


    }
}
