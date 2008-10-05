using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using NetHunspell;

namespace ProvaNetHunspell
{
    public partial class Principal : Form
    {
        private ISpellCheck speller;

        public Principal()
        {
            InitializeComponent();
            string execPath = Path.GetDirectoryName(Application.ExecutablePath);
            speller = new Hunspell(Path.Combine(execPath, @"..\..\Hunspell\bin"), 
                Path.Combine(execPath, @"..\..\resultats\hunspell"), "catalan");
        }

        private void botoExecuta_Click(object sender, EventArgs e)
        {
            miraMot("selecció");
            miraMot("sel·lecció");
            miraMot("d'endívies");
        }

        private void miraMot(string mot)
        {
            if (speller.good(mot))
                resultats.AppendText(String.Format("{0} => OK\r\n", mot));
            else
                resultats.AppendText(String.Format("{0} => {1}\r\n", mot, 
                    String.Join(", ", speller.sugg(mot).ToArray())));
        }

        private void botoSurt_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}