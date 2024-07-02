namespace DesignTechPLMIntegrationPro.WinForm
{
    partial class HomePage
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
            btnStart = new Button();
            listBox1 = new ListBox();
            menuStrip1 = new MenuStrip();
            başlatToolStripMenuItem = new ToolStripMenuItem();
            bağlantıAyarlarıToolStripMenuItem = new ToolStripMenuItem();
            sQLBağlantıAyarlarıToolStripMenuItem = new ToolStripMenuItem();
            windchillBağlantıAyarlarıToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripMenuItem();
            lOGAyarlarıToolStripMenuItem = new ToolStripMenuItem();
            logGeçmişiToolStripMenuItem = new ToolStripMenuItem();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // btnStart
            // 
            btnStart.Dock = DockStyle.Top;
            btnStart.Location = new Point(0, 28);
            btnStart.Name = "btnStart";
            btnStart.Size = new Size(900, 43);
            btnStart.TabIndex = 0;
            btnStart.Text = "Başlat";
            btnStart.UseVisualStyleBackColor = true;
            btnStart.Click += btnStart_Click;
            // 
            // listBox1
            // 
            listBox1.Dock = DockStyle.Fill;
            listBox1.FormattingEnabled = true;
            listBox1.Location = new Point(0, 71);
            listBox1.Name = "listBox1";
            listBox1.SelectionMode = SelectionMode.MultiSimple;
            listBox1.Size = new Size(900, 699);
            listBox1.TabIndex = 1;
            listBox1.SelectedIndexChanged += listBox1_SelectedIndexChanged;
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { başlatToolStripMenuItem, bağlantıAyarlarıToolStripMenuItem, toolStripMenuItem1, lOGAyarlarıToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(900, 28);
            menuStrip1.TabIndex = 2;
            menuStrip1.Text = "menuStrip1";
            // 
            // başlatToolStripMenuItem
            // 
            başlatToolStripMenuItem.Name = "başlatToolStripMenuItem";
            başlatToolStripMenuItem.Size = new Size(63, 24);
            başlatToolStripMenuItem.Text = "Başlat";
            başlatToolStripMenuItem.Click += başlatToolStripMenuItem_Click;
            // 
            // bağlantıAyarlarıToolStripMenuItem
            // 
            bağlantıAyarlarıToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { sQLBağlantıAyarlarıToolStripMenuItem, windchillBağlantıAyarlarıToolStripMenuItem });
            bağlantıAyarlarıToolStripMenuItem.Name = "bağlantıAyarlarıToolStripMenuItem";
            bağlantıAyarlarıToolStripMenuItem.Size = new Size(133, 24);
            bağlantıAyarlarıToolStripMenuItem.Text = "Bağlantı Ayarları";
            // 
            // sQLBağlantıAyarlarıToolStripMenuItem
            // 
            sQLBağlantıAyarlarıToolStripMenuItem.Name = "sQLBağlantıAyarlarıToolStripMenuItem";
            sQLBağlantıAyarlarıToolStripMenuItem.Size = new Size(268, 26);
            sQLBağlantıAyarlarıToolStripMenuItem.Text = "SQL Bağlantı Ayarları";
            sQLBağlantıAyarlarıToolStripMenuItem.Click += sQLBağlantıAyarlarıToolStripMenuItem_Click;
            // 
            // windchillBağlantıAyarlarıToolStripMenuItem
            // 
            windchillBağlantıAyarlarıToolStripMenuItem.Name = "windchillBağlantıAyarlarıToolStripMenuItem";
            windchillBağlantıAyarlarıToolStripMenuItem.Size = new Size(268, 26);
            windchillBağlantıAyarlarıToolStripMenuItem.Text = "Windchill Bağlantı Ayarları";
            windchillBağlantıAyarlarıToolStripMenuItem.Click += windchillBağlantıAyarlarıToolStripMenuItem_Click;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(14, 24);
            // 
            // lOGAyarlarıToolStripMenuItem
            // 
            lOGAyarlarıToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { logGeçmişiToolStripMenuItem });
            lOGAyarlarıToolStripMenuItem.Name = "lOGAyarlarıToolStripMenuItem";
            lOGAyarlarıToolStripMenuItem.Size = new Size(105, 24);
            lOGAyarlarıToolStripMenuItem.Text = "LOG Ayarları";
            lOGAyarlarıToolStripMenuItem.Click += lOGAyarlarıToolStripMenuItem_Click;
            // 
            // logGeçmişiToolStripMenuItem
            // 
            logGeçmişiToolStripMenuItem.Name = "logGeçmişiToolStripMenuItem";
            logGeçmişiToolStripMenuItem.Size = new Size(224, 26);
            logGeçmişiToolStripMenuItem.Text = "Log Geçmişi";
            // 
            // HomePage
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 770);
            Controls.Add(listBox1);
            Controls.Add(btnStart);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "HomePage";
            Text = "HomePage";
            Load += HomePage_Load;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStart;
        private ListBox listBox1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem bağlantıAyarlarıToolStripMenuItem;
        private ToolStripMenuItem sQLBağlantıAyarlarıToolStripMenuItem;
        private ToolStripMenuItem windchillBağlantıAyarlarıToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem başlatToolStripMenuItem;
        private ToolStripMenuItem lOGAyarlarıToolStripMenuItem;
        private ToolStripMenuItem logGeçmişiToolStripMenuItem;
    }
}