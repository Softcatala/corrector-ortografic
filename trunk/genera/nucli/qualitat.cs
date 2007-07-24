using System;
using System.Threading;
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

        private void botoComparaAntic_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            logQualitat.Clear();
            List<string> log = new List<string>();
            Thread thread = new Thread(new ParameterizedThreadStart(ComparaAnticModern));
            thread.Start(log);
            string linia;
            while (!thread.Join(100))
            {
                while ((linia = LlegeixLiniaLog(log)) != null)
                    logQualitat.AppendText(linia);
                Application.DoEvents();
            }
            while ((linia = LlegeixLiniaLog(log)) != null)
                logQualitat.AppendText(linia);
            this.Cursor = Cursors.Default;
        }

        private void botoCmpHunspellMyspell_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            logQualitat.Clear();
            List<string> log = new List<string>();
            Thread thread = new Thread(new ParameterizedThreadStart(ComparaHunspellMyspell));
            thread.Start(log);
            string linia;
            while (!thread.Join(100))
            {
                while ((linia = LlegeixLiniaLog(log)) != null)
                    logQualitat.AppendText(linia);
                Application.DoEvents();
            }
            while ((linia = LlegeixLiniaLog(log)) != null)
                logQualitat.AppendText(linia);
            this.Cursor = Cursors.Default;
        }

        // Torna la base dels diccionaris d'OOo.
        // Mira els directoris de les diverses màquines que feim servir.
        private string DiccionariOOo()
        {
            string[] llocs = { 
                DirBase + @"modern\proves\catalan",
                @"C:\Archivos de programa\OpenOffice.org 2.0\share\dict\ooo\ca_ES", 
                @"C:\Archivos de programa\OpenOffice.org 2.2\share\dict\ooo\catalan"
            };
            foreach(string lloc in llocs)
                if (File.Exists(lloc + ".dic"))
                    return lloc;
            throw new Exception("No es troba cap diccionari d'OOo");
        }

        private void ComparaAnticModern(object olog)
        {
            DateTime horaInici = DateTime.Now;
            List<string> log = (List<string>)olog;
            //
            string dirAnt = DiccionariOOo();
            AfegeixLiniaLog(string.Format("Llegeix regles antigues ({0})", dirAnt), horaInici, log);
            Regles reglesAnt = Regles.LlegeixAff(dirAnt + ".aff");
            AfinaRegles(reglesAnt);
            AfegeixLiniaLog("Genera les formes antigues", horaInici, log);
            List<string> formesAnt = reglesAnt.GeneraFormes(dirAnt + ".dic", Marques.totes, false);
            AfegeixLiniaLog(string.Format("S'han generat {0} formes", formesAnt.Count), horaInici, log);
            //
            AfegeixLiniaLog("Llegeix regles modernes", horaInici, log);
            string dirMod = DirResultats("catalan");
            Regles reglesMod = Regles.LlegeixAff(dirMod + ".aff");
            AfinaRegles(reglesMod);
            AfegeixLiniaLog("Genera les formes modernes", horaInici, log);
            List<string> formesMod = reglesMod.GeneraFormes(dirMod + ".dic", Marques.totes, false);
            AfegeixLiniaLog(string.Format("S'han generat {0} formes", formesMod.Count), horaInici, log);
            //
            AfegeixLiniaLog("Genera el diccionari de formes modernes", horaInici, log);
            Dictionary<string, int> dicMod = new Dictionary<string, int>(formesMod.Count);
            foreach (string str in formesMod)
                dicMod[str] = 1;
            int falten = 0;
            int mostra = 400;
            AfegeixLiniaLog("Paraules que falten a la versió moderna", horaInici, log);
            Regex errorsAnt = new Regex(@"((^[dn]'.*('n|-ne)$)|ïu'n$|^condold[-ií]|^[dl]'hi[aeo])");
            foreach(string str in formesAnt)
                while (true)
                {
                    if (dicMod.ContainsKey(str) || dicMod.ContainsKey(Cat.Min(str)))
                        break;
                    if (errorsAnt.IsMatch(str))
                        break;
                    if (str.EndsWith("ment") && str.StartsWith("d'") && dicMod.ContainsKey(str.Substring(2)))
                        break;
                    ++falten;
                    if (falten <= mostra)
                        AfegeixLiniaLog(string.Format("    {0}: {1}", falten, str), horaInici, log);
                    dicMod[str] = 1;
                    break;
                }
            if (falten > mostra)
                AfegeixLiniaLog(string.Format("    i {0} formes més", falten - mostra), horaInici, log);
        }

        private void ComparaHunspellMyspell(object olog)
        {
            DateTime horaInici = DateTime.Now;
            List<string> log = (List<string>)olog;
            //
            AfegeixLiniaLog("Llegeix regles de Myspell", horaInici, log);
            Regles reglesMyspell = Regles.LlegeixAff(DirResultats("catalan_myspell.aff"));
            AfinaRegles(reglesMyspell);
            AfegeixLiniaLog("Genera les formes de Myspell", horaInici, log);
            List<string> formesMyspell = reglesMyspell.GeneraFormes(DirResultats("catalan_myspell.dic"), Marques.totes, false);
            AfegeixLiniaLog(string.Format("S'han generat {0} formes", formesMyspell.Count), horaInici, log);
            //
            AfegeixLiniaLog("Llegeix regles Hunspell", horaInici, log);
            Regles reglesHunspell = Regles.LlegeixAff(DirResultats("catalan.aff"));
            AfinaRegles(reglesHunspell);
            AfegeixLiniaLog("Genera les formes Hunspell", horaInici, log);
            List<string> formesHunspell = reglesHunspell.GeneraFormes(DirResultats("catalan.dic"), Marques.totes, false);
            AfegeixLiniaLog(string.Format("S'han generat {0} formes", formesHunspell.Count), horaInici, log);
            //
            AfegeixLiniaLog("Genera el diccionari de formes Hunspell", horaInici, log);
            Dictionary<string, int> dicHunspell = new Dictionary<string, int>(formesHunspell.Count);
            foreach (string str in formesHunspell)
                dicHunspell[str] = 1;
            int falten = 0;
            int mostra = 400;
            AfegeixLiniaLog("Paraules que falten a la versió Hunspell", horaInici, log);
            foreach (string str in formesMyspell)
                while (true)
                {
                    if (dicHunspell.ContainsKey(str))
                        break;
                    ++falten;
                    if (falten <= mostra)
                        AfegeixLiniaLog(string.Format("    {0}: {1}", falten, str), horaInici, log);
                    dicHunspell[str] = 1;
                    break;
                }
            if (falten > mostra)
                AfegeixLiniaLog(string.Format("    i {0} formes més", falten - mostra), horaInici, log);
            //
            AfegeixLiniaLog("Genera el diccionari de formes Myspell", horaInici, log);
            Dictionary<string, int> dicMyspell = new Dictionary<string, int>(formesMyspell.Count);
            foreach (string str in formesMyspell)
                dicMyspell[str] = 1;
            falten = 0;
            mostra = 400;
            AfegeixLiniaLog("Paraules que falten a la versió Myspell", horaInici, log);
            foreach (string str in formesHunspell)
                while (true)
                {
                    if (dicMyspell.ContainsKey(str))
                        break;
                    ++falten;
                    if (falten <= mostra)
                        AfegeixLiniaLog(string.Format("    {0}: {1}", falten, str), horaInici, log);
                    dicMyspell[str] = 1;
                    break;
                }
            if (falten > mostra)
                AfegeixLiniaLog(string.Format("    i {0} formes més", falten - mostra), horaInici, log);
        }

        private void AfinaRegles(Regles regles)
        {
            String[] generaUn = { "V", "W", "Y", "Z", "C", "D" };
            foreach (string g1 in generaUn)
                regles.Llista[g1].MaxGenera = 1;
            regles.Llista["C"].EsAfix = false;
            regles.Llista["D"].EsAfix = false;
        }
    }
}