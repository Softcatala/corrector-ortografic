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

        /// <summary>
        /// Crea fitxers .dic i .aff a partir dels fitxers d'entrades.
        /// </summary>
		private void Genera(VFO vfo)
        {
            List<string> log = new List<string>();
            Thread thread = new Thread(new ParameterizedThreadStart(vfo));
            thread.Start(log);
            string linia;
            while (!thread.Join(100))
            {
                while ((linia = LlegeixLiniaLog(log)) != null)
                    logGenera.AppendText(linia);
                Application.DoEvents();
            }
            while ((linia = LlegeixLiniaLog(log)) != null)
                logGenera.AppendText(linia);
        }

        private void GeneraPart(string prefix, Object olog, int mod)
        {
            List<string> log = (List<string>)olog;
            DateTime horaInici = DateTime.Now;
            AfegeixLiniaLog("Genera la llista completa", horaInici, log);
            Regles regles = CarregaRegles(true);
            List<Entrada> entrades = new List<Entrada>();
            Identificador identificador = null;
            identificador = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("DIEC2", regles, DirEntrades("irregulars_diec2.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC2", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec2.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("més paraules", regles, null);
            AfegeixLiniaLog("Llegeix més entrades tipus DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("mes_paraules.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("Termcat", regles, DirEntrades("irregulars_termcat.txt"));
            AfegeixLiniaLog("Llegeix entrades del Termcat", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("termcat.txt"), entrades, mod);
            identificador = new IdentificadorToponims("topònims", regles);
            AfegeixLiniaLog("Llegeix els topònims", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("topònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("topònims_estrangers.txt"), entrades, mod);
            identificador = new IdentificadorGentilicis("gentilicis", regles, null);
            AfegeixLiniaLog("Llegeix els gentilicis", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("gentilicis.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("gentilicis_estrangers.txt"), entrades, mod);
            identificador = new IdentificadorAntroponims("noms i llinatges", regles, null);
            AfegeixLiniaLog("Llegeix els noms i els llinatges", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("antropònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("llinatges.txt"), entrades, mod);
            identificador = new IdentificadorDiversos("diversos", regles);
            AfegeixLiniaLog("Llegeix entrades diverses", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("abreviatures_duarte.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("marques.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("AVL", regles, DirEntrades("irregulars_avl.txt"));
            AfegeixLiniaLog("Llegeix les entrades de l'AVL", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("avl.txt"), entrades, mod);
            List<VersioDiccionari> versions = VersioDiccionari.Versions();
            VersioDiccionari versio0 = versions[0];
            foreach (VersioDiccionari versio in versions)
            {
                string nomFitxer = prefix + versio.Nom;
                AfegeixLiniaLog(String.Format("Genera: {0} (Hunspell)", versio.Descripcio), horaInici, log);
                regles.GeneraAffDicHunspell(DirResultats(@"hunspell\" + nomFitxer), entrades, versio.Filtre, Cat.Cmp);
                AfegeixLiniaLog(String.Format("Genera: {0} (Myspell)", versio.Descripcio), horaInici, log);
                regles.GeneraAffDicMyspell(DirResultats(@"myspell\" + nomFitxer + ".myspell"), entrades, versio.Filtre, Cat.Cmp, IdentificadorCat.GetAfinaMyspell);
                Regles.CanviaString canvis = delegate(String que)
                {
                    switch (que)
                    {
                        case "%VERSION%": return versio.NumeroVersio;
                        case "%VARIANT%": return versio.Variant;
                        case "%FILENAME%": return versio.Nom;
                        case "%UPDATES%": return versio.LlocActualitzacions;
                        case "%NOTES_EN%": return versio.NotesVersions(false);
                        case "%NOTES_CA%": return versio.NotesVersions(true);
                        case "%NOTES_EN_RAW%": return versio.NotesVersionsRaw(false);
                        case "%NOTES_CA_RAW%": return versio.NotesVersionsRaw(true);
                        case "%COPYRIGHT%":
                            foreach (string lin in regles.Descripcio)
                                if (lin.StartsWith("copyright", StringComparison.OrdinalIgnoreCase))
                                    return lin;
                            return que;
                        case "%DATE%": return DateTime.Today.ToString();
                        default: return versio.Extra(que);
                    }
                };
                AfegeixLiniaLog(String.Format("Genera: {0} (OXT)", versio.Descripcio), horaInici, log);
                Regles.GeneraOXT(regles, DirResultats(@"hunspell\"), versio.Nom, canvis);
                AfegeixLiniaLog(String.Format("Genera: {0} (aspell)", versio.Descripcio), horaInici, log);
                if (versio == versio0)
                    Regles.BaseAspell(DirResultats(""), versio.Nom, canvis, regles.Descripcio);
                else
                    Regles.VariantAspell(DirResultats(""), versio.Nom, canvis, versio0.Nom, versio.Filtre.Menys(versio0.Filtre));
            }
            Regles.FinalAspell(DirResultats(""), versio0.Extra("%as_lng%"), versio0.NumeroVersio);
            List<string> excSenseEmprar = identificador.ExcepcionsSenseEmprar();
            if (excSenseEmprar.Count != 0)
                foreach (string exc in excSenseEmprar)
                    AfegeixLiniaLog(string.Format(">>> Excepció no emprada: {0}", exc), horaInici, log);
            AfegeixLiniaLog("Acaba la generació", horaInici, log);
        }

        private void GeneraTot(Object olog)
		{
            GeneraPart("", olog, 1);
        }

        private void GeneraMostra(Object olog)
        {
            GeneraPart("mostra_", olog, 20);
        }

    }
}