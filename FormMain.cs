using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SystemEx.FileIO.Logging;

namespace xpsview
{
    public partial class FormMain : Form
    {
        private bool isFullscreen = false;
        private FormWindowState lastState = FormWindowState.Normal;
        private bool toggleAfterShow = false;
        private bool shown_done = false;

        public FormMain()
        {
            InitializeComponent();
        }


        public bool IsFullscreen
        {
            get { return this.isFullscreen; }
        }

        public void ToggleFullscreen()
        {
            if (isFullscreen)
            {
                try
                {
                    if (Program.IsProtected)
                    {
                        //Protected window cannot set style with FormBorderStyle.Sizable, because this seems to remove layered style!
                        NativeCode.SetWindowLong(this.Handle, NativeCode.GWL_STYLE, NativeCode.WS_VISIBLE | NativeCode.WS_OVERLAPPEDWINDOW | NativeCode.WS_MAXIMIZE);
                        NativeCode.ShowWindow(this.Handle, NativeCode.SW_SHOWNORMAL);
                        //this.WindowState = lastState;
                        this.Invalidate();
                        this.Refresh();
                        //NativeCode.RedrawWindow(this.Handle, IntPtr.Zero, IntPtr.Zero, NativeCode.RDW_FRAME | NativeCode.RDW_UPDATENOW | NativeCode.RDW_INVALIDATE);
                        //NativeCode.UpdateWindow(this.Handle);
                        isFullscreen = false;
                    }
                    else
                    {
                        this.FormBorderStyle = FormBorderStyle.Sizable;
                        this.WindowState = lastState;
                        isFullscreen = false;
                    }
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("Error in ToggleScreen() 1", ex);
                }
                
            }
            else
            {

                try
                {
                    if (Program.IsProtected)
                    {
                        //Protected window cannot set style with FormBorderStyle.Sizable, because this seems to remove layered style!
                        NativeCode.SetWindowLong(this.Handle, NativeCode.GWL_STYLE, NativeCode.WS_VISIBLE | NativeCode.WS_POPUP);
                        NativeCode.ShowWindow(this.Handle, NativeCode.SW_SHOWNORMAL); //Makes sure that the taskbar is not visible!  
                        NativeCode.ShowWindow(this.Handle, NativeCode.SW_SHOWMAXIMIZED);
                        this.Invalidate();
                        this.Refresh();
                        //NativeCode.RedrawWindow(this.Handle, IntPtr.Zero, IntPtr.Zero, NativeCode.RDW_FRAME | NativeCode.RDW_UPDATENOW | NativeCode.RDW_INVALIDATE);
                        //NativeCode.UpdateWindow(this.Handle);
                        isFullscreen = true;
                    }
                    else
                    {
                        lastState = this.WindowState;
                        this.FormBorderStyle = FormBorderStyle.None;
                        this.WindowState = FormWindowState.Normal;//Makes sure that the taskbar is not visible!
                        this.WindowState = FormWindowState.Maximized;
                        isFullscreen = true;
                    }
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("Error in ToggleScreen() 2", ex);
                }
                
            }
        }

        public void SetFullscreenFlag()
        {
            toggleAfterShow = true;
        }

        public bool LoadFileSync()
        {
            return this.xpsViewerInstance.LoadFile();
        }


        private void FormMain_Shown(object sender, EventArgs e)
        {
            try
            {
                if (shown_done == false)
                {
                    //In Form_Load it seems to work not always (without DWM)
                    if (Program.IsProtected)
                    {
                        WndProtect.Protect(this.Handle);
                        timerProtection.Enabled = true;
                    }

                    if (toggleAfterShow)
                        ToggleFullscreen();

                    //ugly trick to make sure our window is in foreground!
                    this.TopMost = true;
                    this.BringToFront();
                    this.TopMost = false;
                    shown_done = true;
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Warn("Exception in FormMain_Shown(): " + ex.ToString());
            }
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            try
            {
                if (System.IO.File.Exists(Application.ExecutablePath + ".ico"))
                    this.Icon = new System.Drawing.Icon(Application.ExecutablePath + ".ico");
            }
            catch (System.Exception ex)
            {
                LogFile.Warn("Exception while setting icon: " + ex.ToString());
            }

            if (Program.IsProtected)
                WndProtect.Init();
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Program.IsProtected)
            {
                WndProtect.UnProtect(this.Handle);
                WndProtect.Free();
            }
        }

        private void timerProtection_Tick(object sender, EventArgs e)
        {
            if (Program.IsProtected)
            {
                //There are redraw bugs, after switching to Skin without DWM, but who cares...
                WndProtect.ChangeProtection(this.Handle);
            }
        }


    }
}
