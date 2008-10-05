namespace ProvaNetHunspell
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
            this.botoExecuta = new System.Windows.Forms.Button();
            this.resultats = new System.Windows.Forms.TextBox();
            this.botoSurt = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // botoExecuta
            // 
            this.botoExecuta.Location = new System.Drawing.Point(318, 242);
            this.botoExecuta.Name = "botoExecuta";
            this.botoExecuta.Size = new System.Drawing.Size(85, 26);
            this.botoExecuta.TabIndex = 0;
            this.botoExecuta.Text = "Prova";
            this.botoExecuta.UseVisualStyleBackColor = true;
            this.botoExecuta.Click += new System.EventHandler(this.botoExecuta_Click);
            // 
            // resultats
            // 
            this.resultats.Location = new System.Drawing.Point(17, 17);
            this.resultats.Multiline = true;
            this.resultats.Name = "resultats";
            this.resultats.Size = new System.Drawing.Size(818, 208);
            this.resultats.TabIndex = 1;
            // 
            // botoSurt
            // 
            this.botoSurt.Location = new System.Drawing.Point(409, 242);
            this.botoSurt.Name = "botoSurt";
            this.botoSurt.Size = new System.Drawing.Size(85, 26);
            this.botoSurt.TabIndex = 2;
            this.botoSurt.Text = "Surt";
            this.botoSurt.UseVisualStyleBackColor = true;
            this.botoSurt.Click += new System.EventHandler(this.botoSurt_Click);
            // 
            // Principal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(847, 280);
            this.Controls.Add(this.botoSurt);
            this.Controls.Add(this.resultats);
            this.Controls.Add(this.botoExecuta);
            this.Name = "Principal";
            this.Text = "Prova NetHunspell";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button botoExecuta;
        private System.Windows.Forms.TextBox resultats;
        private System.Windows.Forms.Button botoSurt;
    }
}

