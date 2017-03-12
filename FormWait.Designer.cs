namespace xpsview
{
    partial class FormWait
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
            this.progressBarWait = new System.Windows.Forms.ProgressBar();
            this.labelWait = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBarWait
            // 
            this.progressBarWait.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.progressBarWait.Location = new System.Drawing.Point(0, 22);
            this.progressBarWait.MarqueeAnimationSpeed = 10;
            this.progressBarWait.Name = "progressBarWait";
            this.progressBarWait.Size = new System.Drawing.Size(384, 20);
            this.progressBarWait.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBarWait.TabIndex = 0;
            this.progressBarWait.Value = 50;
            // 
            // labelWait
            // 
            this.labelWait.AutoSize = true;
            this.labelWait.Location = new System.Drawing.Point(-3, 5);
            this.labelWait.Name = "labelWait";
            this.labelWait.Size = new System.Drawing.Size(70, 13);
            this.labelWait.TabIndex = 1;
            this.labelWait.Text = "Please wait...";
            // 
            // FormWait
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(384, 42);
            this.ControlBox = false;
            this.Controls.Add(this.labelWait);
            this.Controls.Add(this.progressBarWait);
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormWait";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "FormWait";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.White;
            this.Load += new System.EventHandler(this.FormWait_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBarWait;
        private System.Windows.Forms.Label labelWait;
    }
}