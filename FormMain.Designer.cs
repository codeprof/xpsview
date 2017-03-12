namespace xpsview
{
    partial class FormMain
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.timerProtection = new System.Windows.Forms.Timer(this.components);
            this.elementHostXps = new System.Windows.Forms.Integration.ElementHost();
            this.xpsViewerInstance = new xpsview.XpsViewer();
            this.SuspendLayout();
            // 
            // timerProtection
            // 
            this.timerProtection.Interval = 16;
            this.timerProtection.Tick += new System.EventHandler(this.timerProtection_Tick);
            // 
            // elementHostXps
            // 
            this.elementHostXps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHostXps.Location = new System.Drawing.Point(0, 0);
            this.elementHostXps.Name = "elementHostXps";
            this.elementHostXps.Size = new System.Drawing.Size(624, 442);
            this.elementHostXps.TabIndex = 0;
            this.elementHostXps.Text = "elementHostXps";
            this.elementHostXps.Child = this.xpsViewerInstance;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(624, 442);
            this.Controls.Add(this.elementHostXps);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(200, 100);
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "XPS Document Viewer";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FormMain_FormClosing);
            this.Load += new System.EventHandler(this.FormMain_Load);
            this.Shown += new System.EventHandler(this.FormMain_Shown);
            this.ResumeLayout(false);

        }
        #endregion

        private System.Windows.Forms.Integration.ElementHost elementHostXps;
        private XpsViewer xpsViewerInstance;
        private System.Windows.Forms.Timer timerProtection;

    }
}

