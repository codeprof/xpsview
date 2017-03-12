//Copyright (c) 2012 Stefan Moebius (mail@stefanmoebius.de)
using System;
using System.Collections.Generic;
using System.Text;

using SystemEx.FileIO.Logging;

namespace xpsview
{
    static class WndProtect
    {

        static private bool magInit = false;
        static private bool dwmDisabled = false;

        static public bool IsDWMEnabled()
        {
            bool dwmEnabled = false;
            try
            {
                NativeCode.DwmIsCompositionEnabled(out dwmEnabled);
            }
            catch (System.Exception ex)
            {
                //Windows XP...
                dwmEnabled = false;
                LogFile.Error("exception in IsDWMEnabled():", ex);
            }
            return dwmEnabled;
        }

        static public void Init()
        {
            try
            {
                NativeCode.RegisterHotKey(IntPtr.Zero, NativeCode.IDHOT_SNAPDESKTOP, 0, NativeCode.VK_SNAPSHOT);
                NativeCode.RegisterHotKey(IntPtr.Zero, NativeCode.IDHOT_SNAPWINDOW, NativeCode.MOD_ALT, NativeCode.VK_SNAPSHOT);
                NativeCode.RegisterHotKey(IntPtr.Zero, 0xC000, NativeCode.MOD_ALT | NativeCode.MOD_CONTROL, NativeCode.VK_SNAPSHOT);
                NativeCode.RegisterHotKey(IntPtr.Zero, 0xB000, NativeCode.MOD_CONTROL, NativeCode.VK_SNAPSHOT);
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in Init() while registering hotkeys:", ex);
            }

            try
            {
                magInit = NativeCode.MagInitialize();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in Init() while initializing magnification component:", ex);
            }
        }

        static public void Free()
        {
            try
            {
                if (dwmDisabled)
                {
                    NativeCode.DwmEnableComposition(NativeCode.DWM_EC_ENABLECOMPOSITION);
                    dwmDisabled = false;
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in Free() while enabling DWM:", ex);
            }

            try
            {
                NativeCode.UnregisterHotKey(IntPtr.Zero, NativeCode.IDHOT_SNAPDESKTOP);
                NativeCode.UnregisterHotKey(IntPtr.Zero, NativeCode.IDHOT_SNAPWINDOW);
                NativeCode.UnregisterHotKey(IntPtr.Zero, 0xB000);
                NativeCode.UnregisterHotKey(IntPtr.Zero, 0xC000);
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in Free() while unregistering hotkey:", ex);
            }

            try
            {
                NativeCode.MagUninitialize();
                magInit = false;
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in Free() while uninitalizing magnification component:", ex);
            }
        }

        static public void Protect(IntPtr hWnd)
        {
            bool isProtected = false;
            if (NativeCode.IsWindow(hWnd))
            {
                if (IsDWMEnabled())
                {
                    try
                    {
                        isProtected = NativeCode.SetWindowDisplayAffinity(hWnd, NativeCode.WDA_MONITOR);
                        if (isProtected)
                            NativeCode.SetProp(hWnd, "protect_affinity", (IntPtr)1);
                    }
                    catch (System.Exception ex)
                    {
                        //Possibly Windows Vista
                        LogFile.Warn("exception in Protect() while setting affinity:"+ ex.Message);
                    }
                }
                
                if (isProtected == false)
                {
                    try
                    {
                        //Disable DWM, that it cannot be enabled while the application is running
                        NativeCode.DwmEnableComposition(NativeCode.DWM_EC_DISABLECOMPOSITION);
                        dwmDisabled = true;
                    }
                    catch (System.Exception ex)
                    {
                        //Possibly Windows XP...
                        LogFile.Warn("exception in Protect() while disabling DWM:" + ex.Message);
                    }


                    try
                    {
                        if ((NativeCode.GetWindowLong(hWnd, NativeCode.GWL_EXSTYLE) & NativeCode.WS_EX_LAYERED) == 0)
                        {
                            NativeCode.SetProp(hWnd, "protect_layer_added", (IntPtr)1);
                        }
                        NativeCode.SetWindowLong(hWnd, NativeCode.GWL_EXSTYLE, NativeCode.GetWindowLong(hWnd, NativeCode.GWL_EXSTYLE) | NativeCode.WS_EX_LAYERED);
                        NativeCode.SetLayeredWindowAttributes(hWnd, 0, 255, NativeCode.LWA_ALPHA);
                    }
                    catch (System.Exception ex)
                    {
                        LogFile.Error("exception in Protect() while setting layered window:", ex);
                    }


                    try
                    {
                        if (magInit)
                        {
                            IntPtr magWnd = NativeCode.CreateWindow(0, NativeCode.WC_MAGNIFIER, "", NativeCode.WS_CHILD, 0, 0, 1, 1, hWnd, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
                            NativeCode.SetProp(hWnd, "protect_mag", (IntPtr)1);
                            NativeCode.SetProp(hWnd, "protect_mag_hwnd", magWnd);
                        }
                    }
                    catch (System.Exception ex)
                    {
                        LogFile.Error("exception in Protect() while creating maginification window:", ex);
                    }


                    try
                    {
                        NativeCode.RedrawWindow(hWnd, IntPtr.Zero, IntPtr.Zero, NativeCode.RDW_FRAME | NativeCode.RDW_UPDATENOW | NativeCode.RDW_INVALIDATE);
                        NativeCode.UpdateWindow(hWnd);
                    }
                    catch (System.Exception ex)
                    {
                        LogFile.Error("exception in Protect() while refreshing window:", ex);
                    }

                }
            }
            else
            {
                LogFile.Error("no vailid window handle forwarded to Protect()");
            }
        }



        static public void UnProtect(IntPtr hWnd)
        {
            if (NativeCode.IsWindow(hWnd))
            {
                try
                {
                    if ((int)NativeCode.GetProp(hWnd, "protect_affinity") == 1)
                    {
                        NativeCode.SetWindowDisplayAffinity(hWnd, 0);
                        NativeCode.RemoveProp(hWnd, "protect_affinity");
                    }
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("exception in UnProtect() while removing affinity:", ex);
                }


                try
                {
                    if ((int)NativeCode.GetProp(hWnd, "protect_layer_added") == 1)
                    {
                        NativeCode.SetWindowLong(hWnd, NativeCode.GWL_EXSTYLE, NativeCode.GetWindowLong(hWnd, NativeCode.GWL_EXSTYLE) & (~NativeCode.WS_EX_LAYERED));
                        NativeCode.RemoveProp(hWnd, "protect_layer_added");
                    }
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("exception in UnProtect() while removing layered attribute:", ex);
                }


                try
                {
                    if ((int)NativeCode.GetProp(hWnd, "protect_mag") == 1)
                    {

                        if (NativeCode.IsWindow(NativeCode.GetProp(hWnd, "protect_mag_hwnd")))
                            NativeCode.DestroyWindow(NativeCode.GetProp(hWnd, "protect_mag_hwnd"));
                        NativeCode.RemoveProp(hWnd, "protect_mag");
                        NativeCode.RemoveProp(hWnd, "protect_mag_hwnd");
                    }
                }
                catch (System.Exception ex)
                {
                    LogFile.Error("exception in UnProtect() while removing magnification component:", ex);
                }

            }
            else
            {
                LogFile.Error("no vailid window handle forwarded to UnProtect()");
            }
        }


        static public void ChangeProtection(IntPtr hWnd)
        {
            try
            {
                if (NativeCode.IsWindow(hWnd))
                {
                    if (dwmDisabled == false)
                    {
                        if ((int)NativeCode.GetProp(hWnd, "protect_affinity") == 1)
                        {
                            if (IsDWMEnabled() == false)
                            {
                                if ((int)NativeCode.GetProp(hWnd, "protect_change") == 0) //Make sure that we try it only once!
                                {
                                    NativeCode.SetProp(hWnd, "protect_change", (IntPtr)1); //Prop gets never removed, but who cares...
                                    UnProtect(hWnd);
                                    Protect(hWnd);
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception in ChangeProtection():", ex);
            }
        }


    }
}
