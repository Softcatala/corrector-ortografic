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
            List<Identificador> identificadors = new List<Identificador>();
            List<Entrada> entrades = LlegeixEntrades(mod, log, horaInici, regles, identificadors);
            if (cbExportaWeb.Checked)
                // Genera fitxers per al programa de manteniment
                regles.GeneraFitxersWeb(Principal.DadesWeb, entrades, identificadors, Cat.Cmp);
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
            foreach (Identificador id in identificadors)
            {
                List<string> excSenseEmprar = id.ExcepcionsSenseEmprar();
                if (excSenseEmprar.Count != 0)
                    foreach (string exc in excSenseEmprar)
                        AfegeixLiniaLog(string.Format(">>> Excepció no emprada: {0}", exc), horaInici, log);
            }
            AfegeixLiniaLog("Acaba la generació", horaInici, log);
        }

        /// <summary>
        /// Llegeix un fitxer com el generat pel web de tractament de propostes.
        /// Conté entrades en diversos formats: DIEC, topònims, antropònims, etc.
        /// </summary>
        private void LlegeixEntradesWeb(int mod, List<string> log, DateTime horaInici, Regles regles, 
            List<Identificador> identificadors, List<Entrada> entrades)
        {
            var idDiec = new IdentificadorDIEC("web", regles, null);
            var idToponims = new IdentificadorToponims("web", regles);
            var idAntroponims = new IdentificadorAntroponims("web", regles, null);
            var idDiversos = new IdentificadorDiversos("web", regles);
            AfegeixLiniaLog("Llegeix les propostes del web", horaInici, log);
            var sr = new StreamReader(DirEntrades("web.txt"), Encoding.Default);
            var linies = new List<string>();
            while (!sr.EndOfStream)
                linies.Add(sr.ReadLine());
            var temp = DirEntrades("_part_web.txt");
            // llista de topònims
            linies = TriaLinies(linies, temp, delegate(string lin)
            {
                Match m;
                if ((m = reTopo.Match(lin)).Success)
                    return m.Groups["nom"].Value;
                else
                    return "";
            });
            idToponims.LlegeixEntrades(temp, entrades, mod);
            // llista d'antropònims
            linies = TriaLinies(linies, temp, delegate(string lin)
            {
                Match m;
                if ((m = reAntro.Match(lin)).Success)
                {
                    if (m.Groups["gen"].Value == "m")
                        return m.Groups["nom"].Value;
                    else
                    {
                        var nom = m.Groups["nom"].Value;
                        var p = new Paraula(nom);
                        return string.Format("{0}{1}", nom, p.PotApostrofar(true) ? " f" : "");
                    }
                }
                else
                    return "";
            });
            idAntroponims.LlegeixEntrades(temp, entrades, mod);
            // llista de diversos
            linies = TriaLinies(linies, temp, delegate(string lin)
            {
                Match m;
                if ((m = reDivers.Match(lin)).Success)
                    return m.Groups["nom"].Value;
                else
                    return "";
            });
            idDiversos.LlegeixEntrades(temp, entrades, mod);
            // la resta, té el format DIEC
            TriaLinies(linies, temp, lin => lin);
            idDiec.LlegeixEntrades(temp, entrades, mod);
            if (File.Exists(temp))
                File.Delete(temp);
            identificadors.Add(idToponims);
            identificadors.Add(idAntroponims);
            identificadors.Add(idDiversos);
            identificadors.Add(idDiec);
        }

        private Regex reAntro = new Regex(@"^ent=(?<nom>.*?)\^cat1=antropònim (?<gen>[mf])");
        //ent=Urà^cat1=topònim^proposta=516
        private Regex reTopo = new Regex(@"^ent=(?<nom>.*?)\^cat1=topònim");
        private Regex reDivers = new Regex(@"^ent=(?<nom>.*?)\^cat1=(marca|divers)");
        delegate string DInclouLinia(string linia);

        private List<string> TriaLinies(List<string> linies, string nomFitxer, DInclouLinia inclou)
        {
            var resta = new List<string>();
            var sw = new StreamWriter(nomFitxer, false, Encoding.Default);
            foreach (var lin in linies)
            {
                var linia = lin.Trim();
                if (linia.StartsWith("//") || lin.Length == 0)
                {
                    sw.WriteLine("");
                    resta.Add(linia);
                    continue;
                }
                var nova = inclou(linia);
                if (nova != "")
                {
                    sw.WriteLine(nova);
                    resta.Add("// " + linia);
                }
                else
                    resta.Add(linia);
            }
            sw.Close();
            return resta;
        }

        private List<Entrada> LlegeixEntrades(int mod, List<string> log, DateTime horaInici, Regles regles, List<Identificador> identificadors)
        {
            List<Entrada> entrades = new List<Entrada>();
            LlegeixEntradesWeb(mod, log, horaInici, regles, identificadors, entrades);
            Identificador identificador = null;
            identificador = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("DIEC2", regles, DirEntrades("irregulars_diec2.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC2", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec2.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("més paraules", regles, null);
            AfegeixLiniaLog("Llegeix més entrades tipus DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("mes_paraules.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("Termcat", regles, DirEntrades("irregulars_termcat.txt"));
            AfegeixLiniaLog("Llegeix entrades del Termcat", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("termcat.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorToponims("topònims", regles);
            AfegeixLiniaLog("Llegeix els topònims", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("topònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("topònims_estrangers.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorGentilicis("gentilicis", regles, null);
            AfegeixLiniaLog("Llegeix els gentilicis", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("gentilicis.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("gentilicis_estrangers.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorAntroponims("noms i llinatges", regles, null);
            AfegeixLiniaLog("Llegeix els noms i els llinatges", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("antropònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("llinatges.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDiversos("diversos", regles);
            AfegeixLiniaLog("Llegeix entrades diverses", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("abreviatures_duarte.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("marques.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("AVL", regles, DirEntrades("irregulars_avl.txt"));
            AfegeixLiniaLog("Llegeix les entrades de l'AVL", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("avl.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("AVL_gen", regles, DirEntrades("irregulars_avl_gen.txt"));
            AfegeixLiniaLog("Llegeix les entrades de l'AVL per a la versió general", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("avl_gen.txt"), entrades, mod);
            identificadors.Add(identificador);
            identificador = new IdentificadorDIEC("AVL_val", regles, DirEntrades("irregulars_avl_val.txt"));
            AfegeixLiniaLog("Llegeix les entrades de l'AVL per a la versió valenciana", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("avl_val.txt"), entrades, mod);
            identificadors.Add(identificador);
            return entrades;
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