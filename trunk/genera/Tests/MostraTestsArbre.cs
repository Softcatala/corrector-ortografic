using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tests
{
    /// <summary>
    /// Mostra els resultats de les proves dins un TreeView i el detall dins un TextBox.
    /// Empra colors per distingir els trets que han funcionat dels que han fallat.
    /// </summary>
    public class MostraTestsArbre : IMostraTest
    {
        /// <summary>
        /// Construeix amb el component que mostrarà l'arbre i el que mostrarà el detall.
        /// </summary>
        /// <param name="arbre">On es mostrarà l'arbre de tests.</param>
        /// <param name="detall">On es mostraran els detalls del test seleccionat.</param>
        public MostraTestsArbre(TreeView arbre, TextBox detall)
        {
            this.arbre = arbre;
            this.detall = detall;
            arbre.Tag = detall;
            arbre.AfterSelect += new TreeViewEventHandler(arbre_AfterSelect);
            nodes = new Dictionary<BaseTest, TreeNode>();
        }

        /// <summary>
        /// Afegeix un test a l'arbre.
        /// El test s'afegeix al pare, si en té. Si no, s'afegeix a l'arrel de l'arbre.
        /// </summary>
        /// <param name="test">El test que es vol afegir.</param>
        public void NouTest(BaseTest test)
        {
            GrupTest grup = test.Grup;
            TreeNodeCollection tnc;
            if (grup == null)
                tnc = arbre.Nodes;
            else
            {
                if (nodes.ContainsKey(grup))
                    tnc = nodes[grup].Nodes;
                else
                    throw new Exception("No es troba el grup de " + test.Descripcio);
            }
            TreeNode node = new TreeNode(test.Descripcio);
            node.Tag = test;
            tnc.Add(node);
            if (grup == null || test.Dolents > 0)
            {
                TreeNode tn = node;
                while (tn != null)
                {
                    tn.Expand();
                    tn = tn.Parent;
                }
            }
            nodes[test] = node;
            while (node != null)
            {
                PosaColor(node);
                grup = ((BaseTest)node.Tag).Grup;
                if (grup != null)
                    node = nodes[grup];
                else
                    node = null;
            }
        }

        private void PosaColor(TreeNode node)
        {
            node.ForeColor = ColorDeNode(node);
        }

        private System.Drawing.Color ColorDeNode(TreeNode node)
        {
            BaseTest test = (BaseTest)node.Tag;
            int bons = test.Bons;
            int dolents = test.Dolents;
            if (bons > 0 && dolents == 0)
                return System.Drawing.Color.FromArgb(0, 192, 0);
            else if (bons == 0 && dolents > 0)
                return System.Drawing.Color.Red;
            else
                return System.Drawing.Color.Black;
        }

        private void arbre_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TextBox tb = (TextBox)((TreeView)sender).Tag;
            BaseTest bt = (BaseTest)e.Node.Tag;
            tb.Clear();
            tb.ForeColor = ColorDeNode(e.Node);
            tb.AppendText(bt.Descripcio);
            String resultat = bt.Resultat;
            if (resultat != null)
                tb.AppendText(" -> " + resultat);
            List<String> notes = bt.Notes;
            if (notes != null)
                foreach (String s in notes)
                    tb.AppendText("\r\n    " + s);
        }

        private Dictionary<BaseTest, TreeNode> nodes;
        private TreeView arbre;
        private TextBox detall;
    }
}
