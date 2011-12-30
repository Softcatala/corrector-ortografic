using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using Tests;
using xspell;
using catala;

namespace Genera
{
    partial class Principal
    {
        private Dictionary<string, Identificador> identificadors = null;

        private void CarregaExemples(string nomFitxer)
        {
            Regles regles = CarregaRegles(true);
            identificadors = new Dictionary<string,Identificador>();
            identificadors["DIEC"] = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            identificadors["DIEC2"] = new IdentificadorDIEC("DIEC2", regles, DirEntrades("irregulars_diec2.txt"));
            identificadors["AVL"] = new IdentificadorDIEC("AVL", regles, DirEntrades("irregulars_avl_gen.txt"));
            StreamReader sr = new StreamReader(nomFitxer, Encoding.Default);
            int nEntrades = 0;
            entrades.DisplayMember = "Ent";
            while (!sr.EndOfStream)
            {
                string linia = sr.ReadLine();
                if (linia.StartsWith("//"))
                    continue;
                Match match = reIdentificador.Match(linia);
                if (!match.Success) throw new Exception("Error de format: " + linia);
                string ident = match.Groups[1].Value;
                linia = match.Groups[2].Value;
                if (linia.Contains("fideï"))
                    linia = linia + "";
                Entrada entrada = identificadors[ident].IdentificaEntrada(linia);
                nEntrades += 1;
                entrades.Items.Add(entrada);
            }
            entrades.SelectedIndex = 0;
            sr.Close();
        }

        static private Regex reIdentificador = new Regex(@"^\[([^\]]+)\]\s+(.*)");

        private void MostraExemple(Entrada entrada)
        {
            MostraDadesEntrada(entrada);
            MostraDadesDic(entrada);
            string[] tot = Mot.LlistaPlana(entrada.GeneraMots(Marques.totes, false), Cat.Cmp, true).Split(' ');
            string comu = null;
            foreach (string un in tot)
            {
                string[] trossos = un.Split('=');
                if (trossos.Length != 2)
                    continue;
                string info = trossos[0];
                if (comu == null)
                    comu = info;
                else
                {
                    int i = 0;
                    while (i < info.Length && i < comu.Length && info[i] == comu[i])
                        ++i;
                    comu = info.Substring(0, i);
                }
            }
            int midaComu = 0;
            Dictionary<string, string> dades = null;
            if (comu != null)
            {
                midaComu = comu.Length;
                dades = new Dictionary<string, string>();
                foreach (string un in tot)
                {
                    string[] trossos = un.Split('=');
                    string info = trossos[0].Substring(midaComu);
                    string formes = trossos[1];
                    dades[info] = formes;
                }
            }
            else
                comu = "";
            switch (comu)
            {
                case "VERB.":
                    MostraHtml("verb", dades);
                    break;
                case "NOM.F.":
                    MostraHtml("nomfem", dades);
                    break;
                case "NOM.M.":
                    MostraHtml("nommasc", dades);
                    break;
                case "NOM.":
                    MostraHtml("nommascfem", dades);
                    break;
                default:
                    navegadorWeb.DocumentText = string.Format("=== \"{0}\" ===<br>\r\n{1}", entrada.Arrel, String.Join("<br>\r\n", tot));
                    break;
            }
        }

        private void MostraDadesEntrada(Entrada entrada)
        {
            dadesEntrada.Clear();
            string[] claus;
            claus = new string[entrada.Dades.Count];
            entrada.Dades.Keys.CopyTo(claus, 0);
            Array.Sort(claus);
            foreach (string clau in claus)
            {
                if (clau == "línia")
                    continue;
                dadesEntrada.AppendText(string.Format("{0}: {1}\n", clau, entrada.Dades[clau]));
            }
            if (entrada.Excepcions != null)
            {
                dadesEntrada.AppendText("\n");
                Dictionary<string, string> exc = entrada.Excepcions.Contingut.Valors(new Marques(true));
                claus = new string[exc.Count];
                exc.Keys.CopyTo(claus, 0);
                Array.Sort(claus);
                foreach (string clau in claus)
                    dadesEntrada.AppendText(string.Format("{0}: {1}\n", clau, exc[clau]));
            }
        }

        private void MostraDadesDic(Entrada entrada)
        {
            dadesDic.Clear();
            dadesDic.AppendText("Línies dins .dic:\r\n");
            foreach (string linia in Entrada.GeneraLiniesDic(entrada, new Marques(true), Entrada.Speller.HUNSPELL, Cat.Cmp))
                dadesDic.AppendText(String.Format("    {0}\r\n", linia));
        }

        private void MostraHtml(string plantilla, Dictionary<string, string> dades)
        {
            StringBuilder sb = new StringBuilder();
            Regex tag = new Regex(@"\(\(([A-Z1-6\.]+)\)\)");
            Regex css = new Regex(@"(type=""text/css"" href="")(.*\.css"")");
            StreamReader sr = new StreamReader(String.Format(DirBase + @"genera\proves\html\{0}.html", plantilla), Encoding.Default);
            while (!sr.EndOfStream)
            {
                string linia = sr.ReadLine();
                linia = css.Replace(linia, delegate(Match match)
                {
                    return match.Groups[1].Value + DirBase + @"genera\proves\html\" + match.Groups[2].Value;
                });
                if (linia.Contains("((ERRORS))"))
                {
                    if (dades.Count == 0)
                        continue;
                    List<string> errors = new List<string>();
                    foreach (string id in dades.Keys)
                        errors.Add(string.Format("{0}={1}", id, dades[id]));
                    dades["ERRORS"] = String.Join(", ", errors.ToArray());
                }
                linia = tag.Replace(linia, delegate(Match match)
                {
                    string id = match.Groups[1].Value;
                    string resultat;
                    if (dades.ContainsKey(id))
                    {
                        resultat = dades[id];
                        dades.Remove(id);
                    }
                    else
                        resultat = "((" + id + "))";
                    return resultat;
                });
                sb.Append(linia);
            }
            navegadorWeb.DocumentText = sb.ToString();
            sr.Close();
        }

        private void entrades_SelectedIndexChanged(object sender, EventArgs e)
        {
            MostraExemple((Entrada)entrades.SelectedItem);
        }

        private void botoCopia_Click(object sender, EventArgs e)
        {
            Entrada entrada = (Entrada)entrades.SelectedItem;
            StringBuilder sb = new StringBuilder("\"");
            sb.Append(entrada.Dades["línia"]);
            sb.Append(" > ");
            sb.Append(Mot.LlistaPlana(entrada.GeneraMots(Marques.totes, false), Cat.Cmp, true));
            sb.Append("\"");
            Clipboard.SetText(sb.ToString());
        }

    }
}