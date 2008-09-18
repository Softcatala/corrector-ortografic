using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using Tests;
using xspell;

namespace Genera
{
    public partial class Principal : Form
    {
        public Principal()
        {
            InitializeComponent();
            //pestanyes.SelectedTab = tpProvaGeneral;
            //pestanyes.SelectedTab = tpExemples;
            pestanyes.SelectedTab = tpGenera;
            //pestanyes.SelectedTab = tpControlQualitat;
            MostraPestanya(pestanyes.SelectedTab);
        }

        private bool fetaProvaGeneral = false;
        private bool fetsExemples = false;

        private void pestanyes_Selected(object sender, TabControlEventArgs e)
        {
            MostraPestanya(e.TabPage);
        }

        private void MostraPestanya(TabPage pagina)
        {
            if (pagina == tpProvaGeneral)
                FesProvaGeneral();
            else if (pagina == tpExemples)
                FesExemples();
        }

        private void FesProvaGeneral()
        {
            if (fetaProvaGeneral)
                return;
            this.Cursor = Cursors.WaitCursor;
            MostraTestsArbre mostrador = new MostraTestsArbre(arbre, tbExplica);
            GrupTest grupTest = new GrupTest("Generació de diccionaris per a MySpell, Hunspell, ispell...", mostrador);
            test_01(grupTest);
            test_02(grupTest);
            test_03(grupTest);
            test_04(grupTest);
            test_05(grupTest);
            test_06(grupTest);
            test_07(grupTest);
            this.Cursor = Cursors.Default;
            fetaProvaGeneral = true;
        }

        private void FesExemples()
        {
            if (fetsExemples)
                return;
            this.Cursor = Cursors.WaitCursor;
            CarregaExemples(DirBase + @"\genera\proves\exemples.txt");
            this.Cursor = Cursors.Default;
            fetsExemples = true;
        }

        private Regles CarregaRegles(bool generaCurt, Marques filtre)
        {
            Regles regles = new Regles(DirEntrades("regles.txt"), filtre);
            if (generaCurt)
            {
                String[] generaUn = { "V", "W", "Y", "Z", "C", "D" };
                foreach (string g1 in generaUn)
                    regles.Llista[g1].MaxGenera = 1;
            }
            regles.Llista["C"].EsAfix = false;
            regles.Llista["D"].EsAfix = false;
            return regles;
        }

        private Regles CarregaRegles(bool generaCurt)
        {
            return CarregaRegles(generaCurt, new Marques(true));
        }

        private string DirBase 
        {
            get { return Application.StartupPath + @"\..\..\"; }
        }

        private string DirEntrades(string nom)
        {
            return DirBase + @"entrades\" + nom;
        }

        private string DirResultats(string nom)
        {
            return DirBase + @"resultats\" + nom;
        }

        private string LlegeixLiniaLog(List<string> llista)
        {
            string linia = null;
            Monitor.TryEnter(llista);
            if (llista.Count > 0)
            {
                linia = llista[0];
                llista.RemoveAt(0);
            }
            Monitor.Exit(llista);
            return linia;
        }

        private void AfegeixLiniaLog(string linia, DateTime inici, List<string> llista)
        {
            TimeSpan dif = DateTime.Now - inici;
            string tot = String.Format("[{0:d2}:{1:d2}.{2:d3}] {3}\r\n", dif.Minutes, dif.Seconds, dif.Milliseconds, linia);
            Monitor.TryEnter(llista);
            llista.Add(tot);
            Monitor.Exit(llista);
        }

        private void botoSurt_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Una funció que pren un argument de tipus Object i no torna res.
        /// </summary>
        private delegate void VFO(Object obj);

        private void botoGeneraTot_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Genera(GeneraTot);
            this.Cursor = Cursors.Default;
        }

        private void botoGeneraMostra_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            Genera(GeneraMostra);
            this.Cursor = Cursors.Default;
        }

    }

}