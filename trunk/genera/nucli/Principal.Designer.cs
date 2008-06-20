namespace Genera
{
    partial class Principal
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
            this.botoSurt = new System.Windows.Forms.Button();
            this.arbre = new System.Windows.Forms.TreeView();
            this.tbExplica = new System.Windows.Forms.TextBox();
            this.pestanyes = new System.Windows.Forms.TabControl();
            this.tpProvaGeneral = new System.Windows.Forms.TabPage();
            this.tpExemples = new System.Windows.Forms.TabPage();
            this.botoCopia = new System.Windows.Forms.Button();
            this.dadesEntrada = new System.Windows.Forms.RichTextBox();
            this.navegadorWeb = new System.Windows.Forms.WebBrowser();
            this.dadesDic = new System.Windows.Forms.RichTextBox();
            this.entrades = new System.Windows.Forms.ListBox();
            this.tpGenera = new System.Windows.Forms.TabPage();
            this.button1 = new System.Windows.Forms.Button();
            this.botoGeneraMostra = new System.Windows.Forms.Button();
            this.botoGenera = new System.Windows.Forms.Button();
            this.barraProgres = new System.Windows.Forms.ProgressBar();
            this.logGenera = new System.Windows.Forms.RichTextBox();
            this.tpControlQualitat = new System.Windows.Forms.TabPage();
            this.botoCmpHunspellMyspell = new System.Windows.Forms.Button();
            this.botoComparaAntic = new System.Windows.Forms.Button();
            this.logQualitat = new System.Windows.Forms.RichTextBox();
            this.pestanyes.SuspendLayout();
            this.tpProvaGeneral.SuspendLayout();
            this.tpExemples.SuspendLayout();
            this.tpGenera.SuspendLayout();
            this.tpControlQualitat.SuspendLayout();
            this.SuspendLayout();
            // 
            // botoSurt
            // 
            this.botoSurt.Location = new System.Drawing.Point(467, 690);
            this.botoSurt.Name = "botoSurt";
            this.botoSurt.Size = new System.Drawing.Size(75, 23);
            this.botoSurt.TabIndex = 2;
            this.botoSurt.Text = "Surt";
            this.botoSurt.UseVisualStyleBackColor = true;
            this.botoSurt.Click += new System.EventHandler(this.botoSurt_Click);
            // 
            // arbre
            // 
            this.arbre.BackColor = System.Drawing.Color.White;
            this.arbre.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.arbre.Location = new System.Drawing.Point(6, 6);
            this.arbre.Name = "arbre";
            this.arbre.Size = new System.Drawing.Size(382, 634);
            this.arbre.TabIndex = 3;
            // 
            // tbExplica
            // 
            this.tbExplica.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tbExplica.Location = new System.Drawing.Point(394, 6);
            this.tbExplica.Multiline = true;
            this.tbExplica.Name = "tbExplica";
            this.tbExplica.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbExplica.Size = new System.Drawing.Size(576, 634);
            this.tbExplica.TabIndex = 4;
            // 
            // pestanyes
            // 
            this.pestanyes.Controls.Add(this.tpProvaGeneral);
            this.pestanyes.Controls.Add(this.tpExemples);
            this.pestanyes.Controls.Add(this.tpGenera);
            this.pestanyes.Controls.Add(this.tpControlQualitat);
            this.pestanyes.Location = new System.Drawing.Point(12, 12);
            this.pestanyes.Name = "pestanyes";
            this.pestanyes.SelectedIndex = 0;
            this.pestanyes.Size = new System.Drawing.Size(984, 672);
            this.pestanyes.TabIndex = 5;
            this.pestanyes.Selected += new System.Windows.Forms.TabControlEventHandler(this.pestanyes_Selected);
            // 
            // tpProvaGeneral
            // 
            this.tpProvaGeneral.Controls.Add(this.arbre);
            this.tpProvaGeneral.Controls.Add(this.tbExplica);
            this.tpProvaGeneral.Location = new System.Drawing.Point(4, 22);
            this.tpProvaGeneral.Name = "tpProvaGeneral";
            this.tpProvaGeneral.Padding = new System.Windows.Forms.Padding(3);
            this.tpProvaGeneral.Size = new System.Drawing.Size(976, 646);
            this.tpProvaGeneral.TabIndex = 0;
            this.tpProvaGeneral.Text = "Proves generals";
            this.tpProvaGeneral.UseVisualStyleBackColor = true;
            // 
            // tpExemples
            // 
            this.tpExemples.Controls.Add(this.botoCopia);
            this.tpExemples.Controls.Add(this.dadesEntrada);
            this.tpExemples.Controls.Add(this.navegadorWeb);
            this.tpExemples.Controls.Add(this.dadesDic);
            this.tpExemples.Controls.Add(this.entrades);
            this.tpExemples.Location = new System.Drawing.Point(4, 22);
            this.tpExemples.Name = "tpExemples";
            this.tpExemples.Padding = new System.Windows.Forms.Padding(3);
            this.tpExemples.Size = new System.Drawing.Size(976, 646);
            this.tpExemples.TabIndex = 1;
            this.tpExemples.Text = "Exemples";
            this.tpExemples.UseVisualStyleBackColor = true;
            // 
            // botoCopia
            // 
            this.botoCopia.Location = new System.Drawing.Point(6, 611);
            this.botoCopia.Name = "botoCopia";
            this.botoCopia.Size = new System.Drawing.Size(261, 23);
            this.botoCopia.TabIndex = 4;
            this.botoCopia.Text = "Copia per a test";
            this.botoCopia.UseVisualStyleBackColor = true;
            this.botoCopia.Click += new System.EventHandler(this.botoCopia_Click);
            // 
            // dadesEntrada
            // 
            this.dadesEntrada.Location = new System.Drawing.Point(276, 6);
            this.dadesEntrada.Name = "dadesEntrada";
            this.dadesEntrada.Size = new System.Drawing.Size(454, 213);
            this.dadesEntrada.TabIndex = 3;
            this.dadesEntrada.Text = "";
            // 
            // navegadorWeb
            // 
            this.navegadorWeb.Location = new System.Drawing.Point(273, 226);
            this.navegadorWeb.MinimumSize = new System.Drawing.Size(20, 20);
            this.navegadorWeb.Name = "navegadorWeb";
            this.navegadorWeb.Size = new System.Drawing.Size(697, 408);
            this.navegadorWeb.TabIndex = 2;
            // 
            // dadesDic
            // 
            this.dadesDic.Location = new System.Drawing.Point(741, 6);
            this.dadesDic.Name = "dadesDic";
            this.dadesDic.Size = new System.Drawing.Size(229, 214);
            this.dadesDic.TabIndex = 1;
            this.dadesDic.Text = "";
            // 
            // entrades
            // 
            this.entrades.FormattingEnabled = true;
            this.entrades.Location = new System.Drawing.Point(6, 6);
            this.entrades.Name = "entrades";
            this.entrades.Size = new System.Drawing.Size(261, 589);
            this.entrades.TabIndex = 0;
            this.entrades.SelectedIndexChanged += new System.EventHandler(this.entrades_SelectedIndexChanged);
            // 
            // tpGenera
            // 
            this.tpGenera.Controls.Add(this.button1);
            this.tpGenera.Controls.Add(this.botoGeneraMostra);
            this.tpGenera.Controls.Add(this.botoGenera);
            this.tpGenera.Controls.Add(this.barraProgres);
            this.tpGenera.Controls.Add(this.logGenera);
            this.tpGenera.Location = new System.Drawing.Point(4, 22);
            this.tpGenera.Name = "tpGenera";
            this.tpGenera.Padding = new System.Windows.Forms.Padding(3);
            this.tpGenera.Size = new System.Drawing.Size(976, 646);
            this.tpGenera.TabIndex = 2;
            this.tpGenera.Text = "Genera";
            this.tpGenera.UseVisualStyleBackColor = true;
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(15, 81);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(237, 27);
            this.button1.TabIndex = 4;
            this.button1.Text = "Prova OXT";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // botoGeneraMostra
            // 
            this.botoGeneraMostra.Location = new System.Drawing.Point(15, 48);
            this.botoGeneraMostra.Name = "botoGeneraMostra";
            this.botoGeneraMostra.Size = new System.Drawing.Size(237, 27);
            this.botoGeneraMostra.TabIndex = 3;
            this.botoGeneraMostra.Text = "5% de la llista";
            this.botoGeneraMostra.UseVisualStyleBackColor = true;
            this.botoGeneraMostra.Click += new System.EventHandler(this.botoGeneraMostra_Click);
            // 
            // botoGenera
            // 
            this.botoGenera.Location = new System.Drawing.Point(15, 15);
            this.botoGenera.Name = "botoGenera";
            this.botoGenera.Size = new System.Drawing.Size(237, 27);
            this.botoGenera.TabIndex = 2;
            this.botoGenera.Text = "Llista completa";
            this.botoGenera.UseVisualStyleBackColor = true;
            this.botoGenera.Click += new System.EventHandler(this.botoGeneraTot_Click);
            // 
            // barraProgres
            // 
            this.barraProgres.Location = new System.Drawing.Point(24, 602);
            this.barraProgres.Name = "barraProgres";
            this.barraProgres.Size = new System.Drawing.Size(935, 30);
            this.barraProgres.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.barraProgres.TabIndex = 1;
            // 
            // logGenera
            // 
            this.logGenera.Location = new System.Drawing.Point(258, 15);
            this.logGenera.Name = "logGenera";
            this.logGenera.Size = new System.Drawing.Size(701, 569);
            this.logGenera.TabIndex = 0;
            this.logGenera.Text = "";
            // 
            // tpControlQualitat
            // 
            this.tpControlQualitat.Controls.Add(this.botoCmpHunspellMyspell);
            this.tpControlQualitat.Controls.Add(this.botoComparaAntic);
            this.tpControlQualitat.Controls.Add(this.logQualitat);
            this.tpControlQualitat.Location = new System.Drawing.Point(4, 22);
            this.tpControlQualitat.Name = "tpControlQualitat";
            this.tpControlQualitat.Padding = new System.Windows.Forms.Padding(3);
            this.tpControlQualitat.Size = new System.Drawing.Size(976, 646);
            this.tpControlQualitat.TabIndex = 3;
            this.tpControlQualitat.Text = "Control de qualitat";
            this.tpControlQualitat.UseVisualStyleBackColor = true;
            // 
            // botoCmpHunspellMyspell
            // 
            this.botoCmpHunspellMyspell.Location = new System.Drawing.Point(6, 43);
            this.botoCmpHunspellMyspell.Name = "botoCmpHunspellMyspell";
            this.botoCmpHunspellMyspell.Size = new System.Drawing.Size(283, 21);
            this.botoCmpHunspellMyspell.TabIndex = 2;
            this.botoCmpHunspellMyspell.Text = "Compara Hunspell amb Myspell";
            this.botoCmpHunspellMyspell.UseVisualStyleBackColor = true;
            this.botoCmpHunspellMyspell.Click += new System.EventHandler(this.botoCmpHunspellMyspell_Click);
            // 
            // botoComparaAntic
            // 
            this.botoComparaAntic.Location = new System.Drawing.Point(6, 16);
            this.botoComparaAntic.Name = "botoComparaAntic";
            this.botoComparaAntic.Size = new System.Drawing.Size(283, 21);
            this.botoComparaAntic.TabIndex = 1;
            this.botoComparaAntic.Text = "Compara amb la versió antiga";
            this.botoComparaAntic.UseVisualStyleBackColor = true;
            this.botoComparaAntic.Click += new System.EventHandler(this.botoComparaAntic_Click);
            // 
            // logQualitat
            // 
            this.logQualitat.Location = new System.Drawing.Point(295, 17);
            this.logQualitat.Name = "logQualitat";
            this.logQualitat.Size = new System.Drawing.Size(668, 616);
            this.logQualitat.TabIndex = 0;
            this.logQualitat.Text = "";
            // 
            // Principal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1009, 725);
            this.Controls.Add(this.pestanyes);
            this.Controls.Add(this.botoSurt);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Principal";
            this.Text = "Generació de diccionaris";
            this.pestanyes.ResumeLayout(false);
            this.tpProvaGeneral.ResumeLayout(false);
            this.tpProvaGeneral.PerformLayout();
            this.tpExemples.ResumeLayout(false);
            this.tpGenera.ResumeLayout(false);
            this.tpControlQualitat.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button botoSurt;
        private System.Windows.Forms.TreeView arbre;
        private System.Windows.Forms.TextBox tbExplica;
        private System.Windows.Forms.TabControl pestanyes;
        private System.Windows.Forms.TabPage tpProvaGeneral;
        private System.Windows.Forms.TabPage tpExemples;
        private System.Windows.Forms.RichTextBox dadesDic;
        private System.Windows.Forms.ListBox entrades;
        private System.Windows.Forms.WebBrowser navegadorWeb;
        private System.Windows.Forms.RichTextBox dadesEntrada;
        private System.Windows.Forms.Button botoCopia;
        private System.Windows.Forms.TabPage tpGenera;
        private System.Windows.Forms.RichTextBox logGenera;
        private System.Windows.Forms.ProgressBar barraProgres;
        private System.Windows.Forms.Button botoGenera;
        private System.Windows.Forms.TabPage tpControlQualitat;
        private System.Windows.Forms.Button botoComparaAntic;
        private System.Windows.Forms.RichTextBox logQualitat;
        private System.Windows.Forms.Button botoCmpHunspellMyspell;
        private System.Windows.Forms.Button botoGeneraMostra;
        private System.Windows.Forms.Button button1;
    }
}

