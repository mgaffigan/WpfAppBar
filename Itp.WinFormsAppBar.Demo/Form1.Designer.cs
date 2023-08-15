namespace Itp.WinFormsAppBar.Demo
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btClose = new Button();
            cbMonitor = new ComboBox();
            cbEdge = new ComboBox();
            SuspendLayout();
            // 
            // btClose
            // 
            btClose.Location = new Point(12, 12);
            btClose.Name = "btClose";
            btClose.Size = new Size(75, 23);
            btClose.TabIndex = 0;
            btClose.Text = "&Close";
            btClose.UseVisualStyleBackColor = true;
            btClose.Click += btClose_Click;
            // 
            // cbMonitor
            // 
            cbMonitor.FormattingEnabled = true;
            cbMonitor.Location = new Point(12, 41);
            cbMonitor.Name = "cbMonitor";
            cbMonitor.Size = new Size(121, 23);
            cbMonitor.TabIndex = 1;
            // 
            // cbEdge
            // 
            cbEdge.FormattingEnabled = true;
            cbEdge.Location = new Point(12, 70);
            cbEdge.Name = "cbEdge";
            cbEdge.Size = new Size(121, 23);
            cbEdge.TabIndex = 2;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(200, 200);
            Controls.Add(cbEdge);
            Controls.Add(cbMonitor);
            Controls.Add(btClose);
            MinimumSize = new Size(100, 100);
            Name = "Form1";
            Text = "MainForm";
            ResumeLayout(false);
        }

        #endregion

        private Button btClose;
        private ComboBox cbMonitor;
        private ComboBox cbEdge;
    }
}