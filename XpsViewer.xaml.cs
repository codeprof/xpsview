using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Xps.Packaging;
using System.IO;
using System.IO.Packaging;
using System.Windows.Interop;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Data;

using SystemEx.FileIO.Logging;

namespace xpsview
{

    /// <summary>
    /// Interaktionslogik für XpsViewer.xaml
    /// </summary>
    public partial class XpsViewer : UserControl
    {
        private volatile string document_uri = "";
        private volatile Hashtable index = new Hashtable();
        private volatile TextBox editPage = null;

        public XpsViewer()
        {
            InitializeComponent();
            /* Reduce quality, has no huge impact on performance
            System.Windows.Media.RenderOptions.SetBitmapScalingMode(this.documentViewer, System.Windows.Media.BitmapScalingMode.NearestNeighbor);
            System.Windows.Media.RenderOptions.SetEdgeMode(this.documentViewer, System.Windows.Media.EdgeMode.Aliased);
            System.Windows.Media.RenderOptions.SetCachingHint(this.documentViewer, System.Windows.Media.CachingHint.Unspecified);
            this.documentViewer.SnapsToDevicePixels = true;
            */
            try
            {
                if (Program.IsProtected && (WndProtect.IsDWMEnabled() == false)) //Only needed if DWM is not enabled
                {
                    //Fix for redraw bug if window is protected (http://social.msdn.microsoft.com/Forums/en-US/wpf/thread/6f88715b-b9ca-4d4f-974b-97b334d26347/)
                    this.Loaded += delegate
                    {
                        var source = PresentationSource.FromVisual(this);
                        var hwndTarget = source.CompositionTarget as System.Windows.Interop.HwndTarget;
                        if (hwndTarget != null)
                            hwndTarget.RenderMode = RenderMode.SoftwareOnly;
                    };
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("exception wile setting software rendering ", ex);
            }
        }


        private void CommandBinding_CanExecutePrint(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Program.AllowPrint == false)
                e.Handled = true;
        }

        private void CommandBinding_CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
        {
            if (Program.AllowCopy == false)
                e.Handled = true;
        }

        private void btnQuit_Click(object sender, RoutedEventArgs e)
        {
            Program.Quit();
        }

        private void btnToggle_Click(object sender, RoutedEventArgs e)
        {
            Program.ToggleFullscreen();
        }


        private void translageImage(Image img, string en, string de, string fr)
        {
            try
            {
                if (img.ToolTip.Equals(en))
                {
                    img.ToolTip = Lang.translate(en, de, fr);
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in translageImage()\n", ex);
            }
        }

        private void changeLanguage(object sender, RoutedEventArgs e)
        {
            try
            {
                Image img = (Image)sender;
                translageImage(img, "Quit Program (Alt + F4)", "Programm beenden (Alt + F4)", "Quitter le programme (Alt + F4)");
                translageImage(img, "Zoom Out (Ctrl + '-')", "Inhalt verkleinern (Strg + '-')", "Zoom arrière (Crtl + '-')");
                translageImage(img, "Zoom In (Ctrl + '+')", "Inhalt vergrößern (Strg + '+')", "Zoom avant (Crtl + '+')");

                translageImage(img, "Actual Size (Ctrl + 1)", "100% (Strg + 1)", "Taille réelle (Ctrl + 1)");
                translageImage(img, "Fit to Width (Ctrl + 2)", "Seitenbreite (Strg + 2)", "Ajuster à la largeur (Ctrl + 2)");
                translageImage(img, "Whole Page (Ctrl + 3)", "Ganze Seite (Strg + 3)", "Page entière (Ctrl + 3)");
                translageImage(img, "Two Pages (Ctrl + 4)", "Zwei Seiten (Strg + 4)", "deux Pages (Ctrl + 4)");
                translageImage(img, "Toggle Fullscreen (F11)", "Vollbildmodus umschalten (F11)", "basculer en plein écran (F11)");

                translageImage(img, "Previous Page (Alt + arrow key left)", "Vorherige Seite (Alt + Pfeiltaste links)", "Page précédente (Alt + flèche gauche)");
                translageImage(img, "Next Page (Alt + arrow key right)", "Nächste Seite (Alt + Pfeiltaste rechts)", "Page suivante (Alt + flèche droite)");
                translageImage(img, "Print Document (Ctrl + P)", "Dokument drucken (Strg + P)", "Imprimer le document (Ctrl + P)");

            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error in changeLanguage()\n", ex);
            }
        }

        private void BuildLinkList(FixedDocumentSequence fixDocSeq)
        {
            try
            {
                DocumentReference docReference = fixDocSeq.References[0];
                FixedDocument fixDoc = docReference.GetDocument(false);
                for (int i = 0; i < fixDoc.Pages.Count; i++)
                {
                    PageContent content = fixDoc.Pages[i];
                    for (int j = 0; j < content.LinkTargets.Count; j++)
                    {
                        index[content.LinkTargets[j].Name] = i;
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Exception in BuildLinkList():", ex);
            }
        }

        private void LoadXpsFromStream(Stream xpsStream, string packageUriString)
        {
            XpsDocument doc = null;
            Package package = Package.Open(xpsStream);
            //Remember to create URI for the package
            System.Uri packageUri = new Uri(packageUriString);
            //Need to add the Package to the PackageStore
            PackageStore.AddPackage(packageUri, package);
            //Create instance of XpsDocument 
            doc = new XpsDocument(package, CompressionOption.SuperFast, packageUriString);
            document_uri = doc.Uri.AbsoluteUri;
            //Do the operation on document here
            //Here I am viewing the document in the DocViewer
            FixedDocumentSequence fixedDocumentSequence = doc.GetFixedDocumentSequence();
            BuildLinkList(fixedDocumentSequence);
            //To view in the DocViewer
            this.documentViewer.Document = fixedDocumentSequence as IDocumentPaginatorSource;          
            //Remember to keep the Package object in PackageStore until all operations complete on document.
            //Remove the package from store
            // PackageStore.RemovePackage(packageUri);  //We will keep it forever!
            doc.Close();
        }

        private void LoadXpsFromFile(string filename)
        {
            XpsDocument doc = null;
            doc = new XpsDocument(Program.FileToLoad, FileAccess.Read);
            FixedDocumentSequence fixedDocumentSequence = doc.GetFixedDocumentSequence();
            BuildLinkList(fixedDocumentSequence);
            documentViewer.Document = fixedDocumentSequence;
            document_uri = doc.Uri.AbsoluteUri;
            doc.Close();
        }

        private void LoadXpsFromPipe(string pipeName)
        {
            int sz = (int)Program.PipeSize;
            if (sz <= 0)
                throw new ArgumentException("Invalid pipe size " + Program.PipeSize.ToString());                
            
            SafeFileHandle pipeHandle = NativeCode.CreateFile(pipeName, NativeCode.GENERIC_READ, 0, IntPtr.Zero, NativeCode.OPEN_EXISTING, 0, IntPtr.Zero);
            if (pipeHandle.IsInvalid)
                throw new IOException("pipe cannot be opened!");

            FileStream fStream = new FileStream(pipeHandle, FileAccess.Read, 4096, false);
            //we must copy it to a memorystream, oterwise it does not work! (Length cannot be evaluated!)
            MemoryStream fMem = new MemoryStream();  //FileStream fMem = new FileStream(@"D:\test.out", FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 1000, FileOptions.None);
            byte[] arr = new byte[sz];
            int offset = 0;
            int remaining = sz;
            int read = 0;
            int start = Environment.TickCount;
            while (remaining > 0)
            {
                read = fStream.Read(arr, offset, remaining); //Should we terminate when read returns 0 (end of stream)
                if (read > 0)
                    start = Environment.TickCount; // reset timer
                else
                {
                    if (Environment.TickCount > 25000) //Seit 10 sekunden konnte nichts gelesen werden -> abbrechen
                    {
                        LogFile.Error("Read timout reached, aborting...");
                        break;
                    }
                }
                LogFile.Debug("read " + read.ToString() + " bytes from pipe");
                offset += read;
                remaining -= read;
            }
            fMem.Write(arr, 0, sz);
            fStream.Close();
            LoadXpsFromStream(fMem, @"pack://xpsstream.xps/");
        }

        public bool LoadFile()
        {
            bool result = false;
            try
            {
                if (Program.FileToLoad.StartsWith(@"\\.\pipe\"))
                {
                    LoadXpsFromPipe(Program.FileToLoad);
                }
                else
                {
                    if (!File.Exists(Program.FileToLoad))
                        throw new IOException("file not found!");

                    LoadXpsFromFile(Program.FileToLoad);
                }

                result = true;
            }
            catch (System.Exception ex)
            {
                Program.CloseWaitWindow();
                LogFile.Error("Error while loading document '" + Program.FileToLoad + "'\n", ex);             
            }
            return result;
        }


        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void UserControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Program.IsFullscreen) //ESC only in fullscreen mode possible
                {
                    Program.ToggleFullscreen();
                }
            }
            if (e.Key == Key.F11)
            {
                Program.ToggleFullscreen();
            }      
         }


        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (((Keyboard.Modifiers & ModifierKeys.Alt) != 0) && Program.AllowPrevNext)
                {
                    if ((e.Key == Key.System) && (Keyboard.IsKeyDown(Key.Left)) )
                    {
                        GotoPrevPage();
                    }
                    if ((e.Key == Key.System) && (Keyboard.IsKeyDown(Key.Right)))
                    {
                        GotoNextPage();
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Exception in UserControl_KeyDown():", ex);
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                this.documentViewer.Print();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while printing the document:", ex);
                MessageBox.Show(Lang.translate("Error while printing the document!", "Fehler beim Drucken des Dokuments!", "Erreur lors de l'impression du document!"), "XPS Viewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void PrintSupport_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Program.AllowPrint)
                    ((Control)sender).Visibility = System.Windows.Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while setting UI for printer:" , ex);
            }
        }

        private void PART_FindToolBarHost_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Program.AllowSearch)
                    ((Control)sender).Visibility = System.Windows.Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while setting UI for search bar:", ex);
            }
        }

        private void seperatorNextPrev_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Program.AllowPrevNext)
                    ((Control)sender).Visibility = System.Windows.Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while setting UI for prev/next button bar:", ex);
            }
        }


        public void GotoPrevPage()
        {
            try
            {
                if (this.documentViewer.CanGoToPreviousPage)
                    this.documentViewer.PreviousPage();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while going to prev page:", ex);
            }
        }

        public void GotoNextPage()
        {
            try
            {
                if (this.documentViewer.CanGoToNextPage)
                    this.documentViewer.NextPage();
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while going to next page:", ex);
            }
        }


        private void btnPrev_Click(object sender, RoutedEventArgs e)
        {
            GotoPrevPage();
        }

        private void btnNext_Click(object sender, RoutedEventArgs e)
        {
            GotoNextPage();
        }

        private void btnQuit_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (Program.AllowQuitBtn)
                    ((Control)sender).Visibility = System.Windows.Visibility.Visible;
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Error while setting UI for quit button:", ex);
            }
        }

       
        /*
        private bool walk_child_nodes_for_BringIntoView(string uri, DependencyObject parent)
        {
            bool found = false;
            try
            {

                for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
                {
                    System.Windows.Media.Visual child = null;
                    try
                    {
                        child = (System.Windows.Media.Visual)System.Windows.Media.VisualTreeHelper.GetChild(parent, i);                   
                        if (child != null)
                        {
                            string res = "";
                            res = child.GetValue(System.Windows.Documents.Glyphs.NameProperty).ToString();
                            if (res.Equals(uri))
                            {
                                ((FrameworkElement)child).BringIntoView();
                                found = true;
                            }
                        }
                    }
                    catch //(System.Exception ex)
                    {
                        //...
                    }
                    if (found == false && child != null)
                        walk_child_nodes_for_BringIntoView(uri, child);
                }
            }
            catch //(System.Exception ex)
            {
                //...
            }
            return found;
        }
        */

        private bool BringIntoView_ByURI(string url_to_search, int pageIdx)
        {
            bool found = false;
            try
            {
                DocumentPaginator pag = documentViewer.Document.DocumentPaginator;
                FixedDocumentSequence seq = (FixedDocumentSequence)documentViewer.Document;
                System.Windows.Media.Visual page = pag.GetPage(pageIdx).Visual;

                FixedPage fp = page as FixedPage;
                Canvas can = fp.Children[0] as Canvas;
                object obj = can.FindName(url_to_search);
                if (obj != null)
                {
                    ((FrameworkElement)obj).BringIntoView();
                    found = true;
                }
            }
            catch (Exception ex)
            {
                LogFile.Error("Error in BringIntoView_ByURI()", ex);
            }
            return found;
        }



        private void documentViewer_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {

            try
            {
                string url = e.Uri.AbsoluteUri;
                string lower_url = url.ToLower();
                string package_url = "";
                LogFile.Debug("Requested URI: " + url);
                try
                {
                    if (url.ToLower().StartsWith("pack://") ) //sonst exception bei exec:// z.B.
                    {
                        package_url = PackUriHelper.GetPackageUri(e.Uri).AbsoluteUri;
                    }
                }
                catch (Exception ex)
                {
                    LogFile.Error("exception in RequestNavigate() while analyzing uri:", ex);
                }

                if (document_uri.Equals(package_url) && document_uri != "" && package_url != "")
                {
                    string fragment_to_find = e.Uri.Fragment.Replace("#", "");
                    if (index.Contains(fragment_to_find))
                    {
                        BringIntoView_ByURI(fragment_to_find, (int)index[fragment_to_find]);
                    }                                 
                }
                else if (url.ToLower().StartsWith("exec://"))
                {
                    string cmd = url.Remove(0, "exec://".Length);
                    if (cmd.EndsWith("/"))
                        cmd = cmd.Substring(0, cmd.Length - 1);
                    if (cmd.StartsWith("/"))
                        cmd = cmd.Substring(1, cmd.Length - 1);
                    cmd = cmd.Replace("/", @"\");
                    string param = Program.ExecParam.Replace("$1", "\"" + cmd + "\"");
                    if (Program.ExecProgram != "")
                    {
                        System.Diagnostics.Process.Start(Program.ExecProgram, param);
                    }
                }
                else if (url.ToLower().StartsWith("execx://"))
                {
                    string cmd = url.Remove(0, "execx://".Length);
                    if (cmd.EndsWith("/"))
                        cmd = cmd.Substring(0, cmd.Length - 1);
                    if (cmd.StartsWith("/"))
                        cmd = cmd.Substring(1, cmd.Length - 1);
                    cmd = cmd.Replace("/", @"\");
                    string param = Program.ExecParam.Replace("$1", "\"" + cmd + "\"");
                    if (Program.ExecProgram != "")
                    {
                        System.Diagnostics.Process.Start(Program.ExecProgram, param);
                    }
                    Program.Quit();
                }
                else if (url.ToLower().Equals("document://thumbnails/"))
                {
                    this.documentViewer.ViewThumbnails();
                }
                else if (url.ToLower().Equals("document://last-page/"))
                {
                    this.documentViewer.LastPage();
                }
                else if (url.ToLower().Equals("document://first-page/"))
                {
                    this.documentViewer.FirstPage();
                }
                else if (url.ToLower().Equals("document://next-page/"))
                {
                    this.GotoNextPage();
                }
                else if (url.ToLower().Equals("document://prev-page/"))
                {
                    this.GotoPrevPage();
                }
                else if (url.ToLower().Equals("document://close/"))
                {
                    Program.Quit();
                }
                else if (url.ToLower().StartsWith("document://page-"))
                {
                    string page = url.Remove(0, "document://page-".Length);
                    if (page.EndsWith("/"))
                        page = page.Substring(0, page.Length - 1);
                    if (page.StartsWith("/"))
                        page = page.Substring(1, page.Length - 1);

                    int pageIdx = Convert.ToInt32(page);
                    if (pageIdx >= 1)
                    {
                        if (documentViewer.CanGoToPage(pageIdx))
                            documentViewer.GoToPage(pageIdx);
                    }
                }
                else
                {
                    //All protocol, exe, and similar
                    System.Diagnostics.Process.Start(url);  //Can raise exception!
                }
                //URL is always handled
                e.Handled = true;
            }
            catch (Exception ex)
            {
                LogFile.Error("exception in RequestNavigate:", ex);
            }
        }

        private void documentViewer_TargetUpdated(object sender, DataTransferEventArgs e)
        {
            try
            {
                if (editPage != null)
                    editPage.Text = documentViewer.MasterPageNumber.ToString();
            }
            catch (Exception ex)
            {
                LogFile.Error("Exception in documentViewer_TargetUpdated()", ex);
            }
        }

        private void editPage_KeyUp(object sender, KeyEventArgs e)
        {
            try
            {
                if (e.Key == Key.Enter)
                {
                    if (editPage != null)
                    {
                        int pageIdx = Convert.ToInt32(editPage.Text.Trim());
                        if (documentViewer.CanGoToPage(pageIdx))
                        {
                            documentViewer.GoToPage(pageIdx);
                        }
                        else
                        {
                            editPage.Text = documentViewer.MasterPageNumber.ToString();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogFile.Error("Exception in editPage_KeyUp()", ex);
            }
        }

        private void editPage_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            try
            {
                int result = 0;
                if (!Int32.TryParse(e.Text.Trim(), out result))
                {
                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                LogFile.Error("Exception in editPage_PreviewTextInput()", ex);
            }
        }

        private void editPage_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                Control ctrl = sender as Control;
                if (ctrl.Name == "editPage")
                {
                    editPage = sender as TextBox;
                    if (editPage != null)
                        editPage.Text = "1";
                }
            }
            catch (System.Exception ex)
            {
                LogFile.Error("Exception in editPage_Loaded():", ex);
            }
        }


    }
}

