//Copyright (c) 2012 Stefan Moebius (mail@stefanmoebius.de)

//x86 Taget for VS Express described on http://msdn.microsoft.com/en-gb/vstudio/aa718685.aspx. So it should be ok:
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Threading;
using System.IO;

using SystemEx.FileIO.Logging;

namespace xpsview
{
    static class Program
    {
        private static FormMain mainFrm;
        private static FormWait waitFrm;
        private static string fileToLoad = "";
        private static string title = "";
        private static bool debug = false;
        private static bool isProtected = false;
        private static bool allowPrint = true;
        private static bool allowCopy = true;
        private static bool allowSearch = true;
        private static bool allowPrevNext = true;
        private static bool allowQuitBtn = false;
        private static long pipesize = 0;
        private static string execProgram = "";
        private static string execParam = "";

        private static bool toggle = false;

        private static Thread thrdWait = null;
        private static volatile bool thrdWaitRunning = true;

        static private void WaitWindow()
        {
            try
            {
                waitFrm = new FormWait();
                waitFrm.Show();
                while (thrdWaitRunning)
                {
                    Application.DoEvents();
                    Thread.Sleep(1);
                }
                waitFrm.Close();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while running wait thread:\n", ex);
            }
        }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            bool showDWMError = false;
            bool stopLoading = false;
            Application.SetCompatibleTextRenderingDefault(false);
            Application.EnableVisualStyles();
            InitParameters();

            try
            {
                if (fileToLoad == "")
                {
                    Microsoft.Win32.OpenFileDialog dialog = new Microsoft.Win32.OpenFileDialog();
                    dialog.CheckFileExists = true;
                    dialog.Filter = Lang.translate("XPS documents|*.xps|All Files|*.*", "XPS Dokumente|*.xps|Alle Dateien|*.*", "XPS papiers|*.xps|Tous les fichiers|*.*");
                    dialog.RestoreDirectory = true;
                    dialog.AddExtension = true;
                    dialog.FilterIndex = 0;
                    dialog.Multiselect = false;
                    dialog.Title = Lang.translate("Choose an XPS file...", "Wählen Sie eine XPS-Datei aus...", "Choisissez un fichier XPS...");
                    dialog.ShowDialog();
                    if (File.Exists(dialog.FileName))
                        fileToLoad = dialog.FileName;
                    else
                        stopLoading = true;

                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while opening open file dialog:\n", ex);
            }



            if (Environment.OSVersion.Version.Major == 6 && Environment.OSVersion.Version.Minor == 2) //Windows 8
            {
                LogFile.Debug("Win 8 detected");
                if (Program.IsProtected)
                {
                    if (WndProtect.IsDWMEnabled() == false)
                    {
                        LogFile.Debug("Win 8 + no DWM + Protected....");
                        //Stop running the program under windows 8 without DWM -> no protection
                        stopLoading = true;
                        showDWMError = true;
                    }
                }
            }


            if (stopLoading == false)
            {
                try
                {
                    thrdWait = new Thread(new ThreadStart(WaitWindow));
                    thrdWait.SetApartmentState(ApartmentState.STA);
                    thrdWait.Start();
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("Error while starting wait thread:\n", ex);
                }

                mainFrm = new FormMain();
                bool run = mainFrm.LoadFileSync();
                thrdWaitRunning = false;
                try
                {
                    if (title != "")
                    {
                        mainFrm.Text = title;
                    }
                    else
                    {
                        if (fileToLoad != "")
                            mainFrm.Text = System.IO.Path.GetFileName(fileToLoad);
                    }
                    if (toggle)
                        mainFrm.SetFullscreenFlag();
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("Error while modifiying main form:\n", ex);
                }

                if (run)
                    Application.Run(mainFrm);
                else
                    MessageBox.Show(Lang.translate("The document cannot be opened!", "Das Dokument konnte nicht geöffnet werden!", "Le document ne peut pas être ouvert!"), "XPS Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);

            }
            else
            {
                if (showDWMError)
                {
                    LogFile.Error("Cannot continue, Win8 need DWM!");
                    MessageBox.Show(Lang.translate("This document cannot be shown when the Desktop Window Manager (DWM) is not active. To continue please activate it.", "Dieses Dokument kann nicht angezeigt werden, wenn der Desktop Window Manager (DWM) nicht aktiv ist. Um fortzufahren aktivieren Sie diesen bitte.", "Ce programme ne pas fonctionner lorsque le Desktop Window Manager (DWM) n'est pas actif. Pour continuer s'il vous plaît activer le Desktop Window Manager."), "XPS Viewer", MessageBoxButtons.OK, MessageBoxIcon.Error, MessageBoxDefaultButton.Button1, MessageBoxOptions.DefaultDesktopOnly);
                }
            }
        }

        public static void InitParameters()
        {
            string[] args = Environment.GetCommandLineArgs();
            try
            {
                int i = 1; // First parameter is the executable path!
                while (i < args.Length)
                {
                    string arg = args[i].ToUpper();
                    if (arg.Equals("/FULLSCREEN"))
                        toggle = true;
                    else if (arg.Equals("/DEBUG"))
                    {
                        LogFile.OpenLog("", "xpsviewer", true, LogLevel.DEBUG);
                        debug = true;
                    }
                    else if (arg.Equals("/PROTECT"))
                        isProtected = true;
                    else if (arg.Equals("/NOPRINT"))
                        allowPrint = false;
                    else if (arg.Equals("/NOCOPY"))
                        allowCopy = false;
                    else if (arg.Equals("/NOSEARCH"))
                        allowSearch = false;
                    else if (arg.Equals("/NONAVIGATION"))
                        allowPrevNext = false;
                    else if (arg.Equals("/QUITBTN"))
                        allowQuitBtn = true;
                    else if (arg.Equals("/EXEC"))
                    {
                        if (i < args.Length - 1)
                        {
                            execProgram = args[i + 1];
                            i++;
                        }
                    }
                    else if (arg.Equals("/EXECPARAM"))
                    {
                        if (i < args.Length - 1)
                        {
                            execParam = args[i + 1];
                            i++;
                        }
                    }
                    else if (arg.Equals("/SIZE"))
                    {
                        if (i < args.Length - 1)
                        {
                            try
                            {
                                pipesize = Convert.ToInt64(args[i + 1]);
                            }
                            catch (System.Exception ex)
                            {
                                LogFile.Error("exception wile converting size form string to int:", ex);
                            }
                            i++;
                        }
                    }
                    else if (arg.Equals("/TITLE"))
                    {
                        if (i < args.Length - 1)
                        {
                            title = args[i + 1];
                            i++;
                        }
                    }
                    else
                    {
                        if (fileToLoad.Equals(""))
                        {
                            fileToLoad = args[i];
                        }
                    }
                    i++;
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while parsing parameters:\n", ex);
            }
        }


        public static void Show()
        {
            try
            {
                if (mainFrm != null)
                    mainFrm.Show();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in Show():\n", ex);
            }
        }


        public static void Hide()
        {
            try
            {
                if (mainFrm != null)
                    mainFrm.Hide();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in Hide():\n", ex);
            }
        }


        public static void Quit()
        {
            try
            {
                if (mainFrm != null)
                    mainFrm.Close();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in Quit():\n", ex);
            }
        }

        public static void ToggleFullscreen()
        {
            try
            {
                if (mainFrm != null)
                    mainFrm.ToggleFullscreen();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in ToggleFullscreen():\n", ex);
            }
        }

        public static void CloseWaitWindow()
        {
            thrdWaitRunning = false;
        }

        public static bool IsFullscreen
        {
            get { return mainFrm.IsFullscreen; }
        }

        public static string FileToLoad
        {
            get { return fileToLoad; }
        }

        public static bool Debug
        {
            get { return debug; }
        }

        public static bool IsProtected
        {
            get { return isProtected; }
        }

        public static bool AllowPrint
        {
            get { return allowPrint; }
        }

        public static bool AllowCopy
        {
            get { return allowCopy; }
        }

        public static bool AllowSearch
        {
            get { return allowSearch; }
        }

        public static bool AllowPrevNext
        {
            get { return allowPrevNext; }
        }

        public static bool AllowQuitBtn
        {
            get { return allowQuitBtn; }
        }

        public static long PipeSize
        {
            get { return pipesize; }
        }

        public static string ExecParam
        {
            get { return execParam; }
        }

        public static string ExecProgram
        {
            get { return execProgram; }
        }

    }
}
