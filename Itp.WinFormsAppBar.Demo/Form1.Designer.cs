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
            btMinimize = new Button();
            dragThumb1 = new DragThumb();
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
            // btMinimize
            // 
            btMinimize.Location = new Point(12, 99);
            btMinimize.Name = "btMinimize";
            btMinimize.Size = new Size(121, 23);
            btMinimize.TabIndex = 3;
            btMinimize.Text = "Show in &Taskbar";
            btMinimize.UseVisualStyleBackColor = true;
            btMinimize.Click += btMinimize_Click;
            // 
            // dragThumb1
            // 
            dragThumb1.Name = "dragThumb1";
            dragThumb1.Text = "dragThumb1";
            dragThumb1.DragDelta += dragThumb1_DragDelta;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(300, 300);
            Controls.Add(dragThumb1);
            Controls.Add(btMinimize);
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
        private Button btMinimize;
        private DragThumb dragThumb1;
    }
}