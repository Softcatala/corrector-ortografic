using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Ionic.Utils.Zip;

namespace xspell
{
    /// <summary>
    /// Una llista de regles més la informació associada (marques, joc de caràcters...).
    /// Representa tota la informació continguda en un fitxer de regles.
    /// </summary>
    public class Regles
    {
        /// <summary>
        /// Llegeix un fitxer de regles.
        /// </summary>
        /// <param name="nomFitxer">El nom del fitxer amb les regles.</param>
        public Regles(string nomFitxer)
        {
            StreamReader fitxer = new StreamReader(nomFitxer, Encoding.Default);
            Marques filtre = new Marques(true);
            Llegeix(fitxer, nomFitxer, filtre);
            fitxer.Close();
        }

        /// <summary>
        /// Llegeix un fitxer de regles.
        /// </summary>
        /// <param name="nomFitxer">El fitxer des d'on llegirem les regles.</param>
        /// <param name="filtre">El filtre per a les marques admeses.</param>
        public Regles(string nomFitxer, Marques filtre)
        {
            StreamReader fitxer = new StreamReader(nomFitxer, Encoding.Default);
            Llegeix(fitxer, nomFitxer, filtre);
            fitxer.Close();
        }

        public Regles()
        {
            regles = new Dictionary<string, Regla>();
        }

        /// <summary>
        /// Crea un objecte Regles a partir d'un fitxer .aff.
        /// Tots els casos es fan pertanyents al grup 000 ("Sense condicions").
        /// </summary>
        /// <param name="nomFitxer">El fitxe .aff.</param>
        /// <returns>Un objecte Regles.</returns>
        public static Regles LlegeixAff(string nomFitxer)
        {
            StreamReader ent = new StreamReader(nomFitxer, Encoding.Default);
            MemoryStream mem = new MemoryStream();
            StreamWriter sort = new StreamWriter(mem, Encoding.Default);
            sort.WriteLine("GRUPS");
            sort.WriteLine("000 Sense condicions");
            sort.WriteLine("/GRUPS");
            Regex rePartsRegla = null;
            int casosRegla = 0;
            Match match;
            while (!ent.EndOfStream)
            {
                string linia = ent.ReadLine();
                if (linia.Length == 0 || linia.StartsWith("#"))
                    continue;
                if (linia.StartsWith("SET ") || linia.StartsWith("TRY "))
                {
                    sort.WriteLine(linia);
                    sort.WriteLine();
                }
                else if (linia.StartsWith("REP "))
                {
                    string[] trossos = linia.Split(' ');
                    if (trossos.Length == 3)
                        sort.WriteLine(string.Format("FIRST {0}/{1}", trossos[1], trossos[2]));
                }
                else if (linia.StartsWith("PFX ") || linia.StartsWith("SFX "))
                {
                    if (casosRegla > 0 && (match = rePartsRegla.Match(linia)).Success)
                    {
                        sort.WriteLine(string.Format("000 {0} {1} {2}", match.Groups[1].Value,
                            match.Groups[2].Value, match.Groups[3].Value));
                        --casosRegla;
                        if (casosRegla == 0)
                            sort.WriteLine("/REGLA");
                    }
                    else if ((match = reIniciRegla.Match(linia)).Success)
                    {
                        rePartsRegla = new Regex(string.Format(@"^{0}\s+{1}\s+(\S+)\s+(\S+)\s+(\S+)",
                            match.Groups[1].Value, match.Groups[2].Value));
                        casosRegla = int.Parse(match.Groups[4].Value);
                        sort.WriteLine();
                        sort.WriteLine(string.Format("REGLA {0} {1}{2} Regla {3}", match.Groups[2].Value,
                            match.Groups[1].Value, match.Groups[3].Value == "Y" ? "+" : "", match.Groups[2].Value));
                    }
                }
                else
                    throw new Exception("No sé què fer amb " + linia);
            }
            ent.Close();
            sort.Flush();
            //mem.Seek(0, SeekOrigin.Begin);
            //mem.WriteTo((new StreamWriter(@"..\..\resultats\prova2.txt", false, Encoding.Default)).BaseStream);
            mem.Seek(0, SeekOrigin.Begin);
            StreamReader fitxer = new StreamReader(mem, Encoding.Default);
            Regles regles = new Regles();
            regles.Llegeix(fitxer, nomFitxer, new Marques(true));
            fitxer.Close();
            return regles;
        }

        /// <summary>
        /// Torna una llista de regles que només conté els casos que surten a gros però no a petit.
        /// </summary>
        /// <param name="gros">La llista de regles més grossa.</param>
        /// <param name="petit">La llista de regles més petita.</param>
        /// <returns>La llista de regles diferència.</returns>
        public static Regles Diferencia(Regles gros, Regles petit)
        {
            //PER_FER: donar un error si un cas surt a petit però no a gros?
            Regles regles = new Regles();
            Dictionary<string, bool> dinsPetit = new Dictionary<string, bool>();
            foreach (Regla regla in petit.regles.Values)
                foreach (CasRegla cas in regla.Casos)
                    dinsPetit[cas.ToString()] = true;
            foreach (Regla regla in gros.regles.Values)
            {
                Regla reglaNova = null;
                foreach (CasRegla cas in regla.Casos)
                    if (!dinsPetit.ContainsKey(cas.ToString()))
                    {
                        if (reglaNova == null)
                        {
                            reglaNova = new Regla(regla.Id, regla.Descripcio, regla.EsSufix, regla.EsCombinable);
                            regles.regles.Add(reglaNova.Id, reglaNova);
                        }
                        reglaNova.NouCas(cas);
                    }
            }
            return regles;
        }

        /// <summary>
        /// Torna la descripció.
        /// </summary>
        public List<string> Descripcio { get { return descripcio; } }

        static private Regex reIniciRegla = new Regex(@"^([PS]FX)\s+(\S)\s+([YN])\s+(\d+)");

        /// <summary>
        /// Llegeix d'un stream obert.
        /// </summary>
        /// <param name="lector">L'stream d'on llegirem les regles.</param>
        /// <param name="nomFitxer">El nom del fitxer de regles.</param>
        /// <param name="filtre">Les marques que admetem. Els casos de les regles que no passin
        /// el filtre es rebutgen.</param>
        private void Llegeix(StreamReader lector, string nomFitxer, Marques filtre)
        {
            regles = new Dictionary<string, Regla>();
            marques = new Dictionary<string, Marca>();
            descripcio = new List<string>();
            caracters = null;
            rep = new List<string>();
            set = null;
            Regla regla = null; // la regla que estam llegint
            int numLinia = 0;
            Llegint estat = Llegint.IGNORA;
            while (!lector.EndOfStream)
            {
                string liniaBruta = lector.ReadLine();
                string linia = liniaBruta.Trim();
                ++numLinia;
                if (linia.Length == 0 || linia.StartsWith("//"))
                    continue;
                switch (estat)
                {
                    case Llegint.IGNORA:
                        if (linia.StartsWith("DESC"))
                            estat = Llegint.DESC;
                        else if (linia.StartsWith("GRUPS"))
                            estat = Llegint.GRUPS;
                        else if (linia.StartsWith("REGLA"))
                        {
                            regla = Regla.Crea(linia);
                            regles[regla.Id] = regla;
                            estat = Llegint.REGLA;
                        }
                        else if (linia.StartsWith("SET"))
                            set = ContingutLinia(linia);
                        else if (linia.StartsWith("TRY"))
                            caracters = ContingutLinia(linia);
                        else if (linia.StartsWith("FIRST"))
                            NouFirst(ContingutLinia(linia));
                        break;
                    case Llegint.DESC:
                        if (linia.StartsWith("/DESC"))
                            estat = Llegint.IGNORA;
                        else
                            descripcio.Add(liniaBruta);
                        break;
                    case Llegint.GRUPS:
                        if (linia.StartsWith("/GRUPS"))
                            estat = Llegint.IGNORA;
                        else
                        {
                            Match match = PartsGrup.Match(linia);
                            if (match.Success)
                            {
                                String id = match.Groups[1].Value;
                                String desc = match.Groups[2].Value;
                                marques[id] = Marca.Crea(id, desc);
                            }
                            else
                                throw new Exception(String.Format("Error llegint marques dialectals: {0} [{1}.{2}]", linia, nomFitxer, numLinia));
                        }
                        break;
                    case Llegint.REGLA:
                        if (linia.StartsWith("/REGLA"))
                            estat = Llegint.IGNORA;
                        else
                        {
                            CasRegla cas = CasRegla.Crea(linia, regla, marques);
                            if (filtre.Conte(cas.Marca))
                                regla.NouCas(cas);
                        }
                        break;
                    default:
                        throw new Exception(String.Format("No sé que fer amb l'estat {0}", estat));
                }
            }
            foreach (KeyValuePair<string, Regla> sr in regles)
                sr.Value.CalculaCalCombinable(this);
            if (estat != Llegint.IGNORA)
                throw new Exception(String.Format("Error llegint regles, estat final = {0}", estat.ToString()));
        }

        /// <summary>
        /// Genera un fitxer d'afixos i un fitxer .dic per a Hunspell.
        /// </summary>
        /// <param name="nomFitxer">El nom base per als fitxers que generarem (.aff i .dic).</param>
        /// <param name="entrades">La llista d'entrades per a generar .dic.</param>
        /// <param name="filtre">Un filtre per posar només els casos de regla amb una certa marca.</param>
        /// <param name="comparador">La funció per comparar cadenes, per ordenar el fitxer .dic.</param>
        public void GeneraAffDicHunspell(string nomFitxer, List<Entrada> entrades, Marques filtre, 
            Comparison<string> comparador)
        {
            CreaFitxerAff(nomFitxer, filtre);
            CreaFitxerDic(nomFitxer, entrades, filtre, comparador);
        }

        /// <summary>
        /// Genera fitxers per al lloc web de manteniment.
        /// El programa de gestió del lloc web de manteniment no forma part d'aquest projecte.
        /// </summary>
        /// <param name="dirDesti">El directori on aniran els fitxers generats.</param>
        /// <param name="entrades">La llista d'entrades.</param>
        /// <param name="identificadors">La llista d'identificadors.</param>
        public void GeneraFitxersWeb(string dirDesti, List<Entrada> entrades, List<Identificador> identificadors,
            Comparison<string> comparador)
        {
            foreach (string fitxer in Directory.GetFiles(dirDesti))
                File.Delete(fitxer);
            StreamWriter fonts = new StreamWriter(dirDesti + @"\fonts.txt", false, Encoding.Default);
            foreach (FitxerFont ff in FitxerFont.Llista)
            {
                string nom = Path.GetFileName(ff.NomFitxer);
                fonts.WriteLine("id={0}\tnom={1}", ff.Id, nom);
                string dest = dirDesti + "\\" + nom;
                File.Copy(ff.NomFitxer, dest);
            }
            fonts.Close();
            Marques senseFiltre = new Marques(true);
            StreamWriter dic = new StreamWriter(dirDesti + @"\resultats.txt", false, Encoding.Default);
            foreach (Entrada ent in entrades)
            {
                List<ItemDic> items = Entrada.GeneraItemsDic(ent, senseFiltre, Entrada.Speller.WEB, comparador);
                foreach (ItemDic item in items)
                {
                    List<string> camps = new List<string>();
                    camps.Add(item.ToString());
                    camps.Add("D=" + ent.FontDades);
                    if (ent.Excepcions != null)
                        camps.Add(string.Format("E={0}:{1}", ent.Excepcions.FitxerFont.Id, ent.Excepcions.LiniaFitxerFont));
                    if (item.mgArrel != null)
                        camps.Add("A=" + item.mgArrel.ToString());
                    if (item.mgComuna != null)
                        camps.Add("C=" + item.mgComuna.ToString());
                    camps.Add("P=" + item.Paradigma.ToString());
                    dic.WriteLine(string.Join("\t", camps.ToArray()));
                }
                //break;
            }
            dic.Close();
        }

        public delegate String CanviaString(String que);

        private static Regex CercaMacro = new Regex("(.*?)(%[A-Za-z0-9_]+%)(.*)");

        private static string AplicaMacros(string linia, CanviaString canvis)
        {
            String inici = "";
            String resta = linia;
            while (true)
            {
                Match match = CercaMacro.Match(resta);
                if (!match.Success)
                    break;
                inici = inici + match.Groups[1].Value + canvis(match.Groups[2].Value);
                resta = match.Groups[3].Value;
            }
            return inici + resta;
        }

        private static string[] AdaptaFitxer(String fitxer, CanviaString canvis, Encoding encoding)
        {
            String[] linies = File.ReadAllLines(fitxer, encoding);
            for (int i = 0; i < linies.Length; i++)
                linies[i] = AplicaMacros(linies[i], canvis);
            return linies;
        }

        private static String AdaptaFitxer(String fitxer, CanviaString canvis, string finalLinia, Encoding encoding)
        {
            return String.Join(finalLinia, AdaptaFitxer(fitxer, canvis, encoding));
        }

        private static void EscriuLinies(string fitxer, string[] linies, Encoding encoding, string finalLinia)
        {
            using (StreamWriter sw = new StreamWriter(fitxer, false, encoding))
            {
                sw.NewLine = finalLinia;
                foreach (string linia in linies)
                    sw.WriteLine(linia);
            }
            
        }

        public static void GeneraOXT(Regles regles, string dirFitxer, string nomFitxer, CanviaString canvis)
        {
            // genera .oxt
            String path = dirFitxer + nomFitxer + ".oxt";
            File.Delete(path);
            using (ZipFile zip = new ZipFile(path))
            {
                zip.AddFile(dirFitxer + nomFitxer + ".dic", "dictionaries");
                zip.AddFile(dirFitxer + nomFitxer + ".aff", "dictionaries");
                zip.AddFile(dirFitxer + @"..\..\OXT\" + "LICENSES-en.txt","");
                zip.AddFile(dirFitxer + @"..\..\OXT\" + "LLICENCIES-ca.txt", "");
                zip.AddStringAsFile(AdaptaFitxer(dirFitxer + @"..\..\OXT\" + "dictionaries.xcu", canvis, "\r\n", Encoding.UTF8), "dictionaries.xcu", "");
                zip.AddStringAsFile(AdaptaFitxer(dirFitxer + @"..\..\OXT\" + "description.xml", canvis, "\r\n", Encoding.UTF8), "description.xml", "");
                //zip.AddStringAsFile(AdaptaFitxer(dirFitxer + @"..\..\OXT\" + "release-notes_en.txt", canvia), "release-notes_en.txt", "");
                //zip.AddStringAsFile(AdaptaFitxer(dirFitxer + @"..\..\OXT\" + "release-notes_ca.txt", canvia), "release-notes_ca.txt", "");
                zip.AddFile(dirFitxer + @"..\..\OXT\META-INF\" + "manifest.xml", "META-INF/");
                zip.Save();
            }
            // genera update.xml, release-notes_en.txt i release-notes_ca.txt
            path = dirFitxer + nomFitxer + ".update.xml";
            using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8)) {
                sw.Write(AdaptaFitxer(dirFitxer + @"..\..\OXT\update.xml", canvis, "\r\n", Encoding.UTF8));
            }
            string[] llengues = { "ca", "en" };
            foreach (string llengua in llengues)
            {
                path = dirFitxer + "release-notes_" + llengua + ".html";
                using (StreamWriter sw = new StreamWriter(path, false, Encoding.UTF8))
                {
                    sw.Write(AdaptaFitxer(dirFitxer + @"..\..\OXT\" + "release-notes_" + llengua + ".html", canvis, "\r\n", Encoding.UTF8));
                }
            }
        }

        private static List<string> infoAspell;
        private static string baseAspell;

        /// <summary>
        /// Prepara per generar el fitxer d'aspell
        /// </summary>
        public static void BaseAspell(string dirResultats, string nom, CanviaString canvis, List<string> copyright)
        {
            NetejaDirAspell(dirResultats, true);
            // Posa les dades del fitxer info
            infoAspell = new List<string>();
            baseAspell = AplicaMacros("%as_lng%-common", canvis);
            infoAspell.Add("name_english Catalan");
            infoAspell.Add("lang ca");
            string nomAffix = "ca_affix.dat";
            infoAspell.Add("data-file " + nomAffix);
            string[] linies = File.ReadAllLines(dirResultats + @"myspell\" + nom + ".myspell.aff", Encoding.Default);
            EscriuLinies(dirResultats + @"aspell\" + nomAffix, linies, Encoding.Default, "\n");
            infoAspell.Add("copyright GPLv2");
            infoAspell.Add(AplicaMacros("version %VERSION%", canvis));
            infoAspell.Add("complete true");
            infoAspell.Add("accurate true");
            infoAspell.Add("author:");
            infoAspell.Add("  name Joan Moratinos");
            infoAspell.Add("  email jmo at softcatala org");
            infoAspell.Add("url http://www.softcatala.org/wiki/Projectes/Corrector_ortografic");
            CreaFitxerCwl(dirResultats + @"myspell\" + nom + ".myspell.dic", 
                AplicaMacros(dirResultats + @"aspell\%as_lng%-common.wl", canvis), Encoding.Default, false, 1);
            infoAspell.Add("");
            infoAspell.Add("alias ca catalan");
            infoAspell.Add("dict:");
            infoAspell.Add(AplicaMacros("  name %as_lang%", canvis));
            infoAspell.Add(AplicaMacros("  alias %as_lng%", canvis));
            infoAspell.Add(AplicaMacros("  add %as_lng%-common", canvis));
            string[] dat = AdaptaFitxer(dirResultats + @"..\aspell\%as_lng%.dat", canvis, Encoding.Default);
            //string nomDat = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\%as_lang%.dat"), canvis);
            string nomDat = Path.GetFullPath(AplicaMacros(dirResultats + @"aspell\%as_lng%.dat", canvis));
            EscriuLinies(nomDat, dat, Encoding.Default, "\n");
            //string[] readme = AdaptaFitxer(dirResultats + @"..\aspell\README", canvis);
            //EscriuLinies(dirResultats + @"aspell\README", readme, Encoding.Default, "\n");
            EscriuLinies(dirResultats + @"aspell\Copyright", copyright.ToArray(), Encoding.Default, "\n");
            string[] changes = AdaptaFitxer(dirResultats + @"..\aspell\CHANGES", canvis, Encoding.Default);
            string dirDoc = Path.GetFullPath(dirResultats + @"aspell\doc");
            if (!Directory.Exists(dirDoc))
                Directory.CreateDirectory(dirDoc);
            EscriuLinies(dirDoc + @"\ChangeLog", changes, Encoding.Default, "\n");
        }

        // Neteja el directori aspell
        private static void NetejaDirAspell(string dirResultats, bool esbBZ2)
        {
            // Els fitxers del directori base, excepte, eventualment "*.bz2"
            string dirBase = Path.GetFullPath(dirResultats + "\\aspell");
            foreach(string fitxer in Directory.GetFiles(dirBase))
                if (!fitxer.EndsWith(".bz2") || esbBZ2)
                    File.Delete(fitxer);
            // Els directoris, excepte els que comencen per "."
            string[] dirs = Directory.GetDirectories(dirBase);
            foreach (string dir in dirs)
            {
                if (Path.GetFileName(dir).StartsWith("."))
                    continue;
                foreach (string fitxer in Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories))
                    File.Delete(fitxer);
                Directory.Delete(dir, true);
            }
        }

        /// <summary>
        /// Afegeix una variant d'aspell.
        /// Genera un fitxer de paraules amb les entrades que no apareixen al fitxer de base.
        /// Els casos de les regles específiques per a aquesta variant no estan inclosos dins
        /// el fitxer d'afixos. Per tant (ja que només hi pot haver un fitxer d'afixos) s'han
        /// de posar totes les entrades desplegades emprant aquests casos.
        /// </summary>
        /// <param name="dirResultats">El directori base dels resultats.</param>
        /// <param name="nom">El nom base dels fitxers.</param>
        /// <param name="canvis">Els canvis que s'han aplicar als macros que apareixen als noms
        /// i als continguts dels fitxers</param>
        /// <param name="nomComuns">El nom dels fitxer amb les entrades comunes</param>
        /// <param name="filtre">Les marques que han de tenir les entrades i els casos de les regles per 
        /// ser incloses dins el fitxer de paraules.</param>
        public static void VariantAspell(string dirResultats, string nom, CanviaString canvis, string nomComuns, Marques filtre)
        {
            StreamReader sr;
            sr = new StreamReader(dirResultats + @"myspell\" + nomComuns + @".myspell.dic", Encoding.Default);
            Dictionary<string, int> comuns = new Dictionary<string, int>(int.Parse(sr.ReadLine()));
            while (!sr.EndOfStream)
                comuns.Add(sr.ReadLine(), 1);
            sr.Close();
            string affVar = Path.GetFullPath(dirResultats + @"myspell\" + nom + @".myspell.aff");
            string affCom = Path.GetFullPath(dirResultats + @"myspell\" + nomComuns + @".myspell.aff");
            Regles reglesDiff = Regles.Diferencia(Regles.LlegeixAff(affVar), Regles.LlegeixAff(affCom));
            string[] ids = new string[reglesDiff.regles.Count];
            reglesDiff.regles.Keys.CopyTo(ids, 0);
            Regex cercaReglesDiff = new Regex("/.*((" + String.Join(")|(", ids) + "))");
            Regex arrelIFlags = new Regex("^(.*)/(.*)$");
            Marques totesMarques = new Marques(true);
            List<string> nous = new List<string>();
            sr = new StreamReader(dirResultats + @"myspell\" + nom + @".myspell.dic", Encoding.Default);
            sr.ReadLine();
            while (!sr.EndOfStream)
            {
                string lin = sr.ReadLine();
                if (cercaReglesDiff.IsMatch(lin))
                {
                    Match match = arrelIFlags.Match(lin);
                    string arrel = match.Groups[1].Value, flags = match.Groups[2].Value;
                    foreach (string id in reglesDiff.regles.Keys)
                        if (flags.Contains(id))
                            foreach(CasRegla cas in reglesDiff.regles[id].Casos)
                                if (cas.EsAplicable(arrel))
                                {
                                    List<Mot> arrels = new List<Mot>();
                                    cas.Genera(arrel, ref arrels, null, reglesDiff, totesMarques, false);
                                    string nousFlags = (flags == id) ? "" : "/" + flags.Replace(id, "");
                                    foreach (Mot novaArrel in arrels)
                                        nous.Add(novaArrel.Forma + nousFlags);
                                }
                }
                if (!comuns.ContainsKey(lin))
                    nous.Add(lin);
            }
            sr.Close();
            string nomDiff = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\%as_lang%.diff.txt"), canvis);
            string nomAwl = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\%as_lang%-mes.wl"), canvis);
            StreamWriter sw = new StreamWriter(nomDiff, false, Encoding.Default);
            foreach (string lin in nous)
                if (lin.Length > 0)
                    sw.WriteLine(lin);
            sw.Close();
            CreaFitxerCwl(nomDiff, nomAwl, Encoding.Default, false, 1);
            File.Delete(nomDiff);
            infoAspell.Add("");
            infoAspell.Add("dict:");
            infoAspell.Add(AplicaMacros("  name %as_lang%", canvis));
            infoAspell.Add("  add " + baseAspell);
            infoAspell.Add(AplicaMacros("  add %as_lang%-mes", canvis));
            string[] dat = AdaptaFitxer(dirResultats + @"..\aspell\%as_lng%.dat", canvis, Encoding.UTF8);
            string nomDat = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\%as_lng%.dat"), canvis);
            EscriuLinies(nomDat, dat, Encoding.Default, "\n");
        }

        public static void FinalAspell(string dirResultats, string llengua, string versio)
        {
            File.WriteAllLines(dirResultats + @"aspell\info", infoAspell.ToArray());
            File.Copy(dirResultats + @"..\aspell\proc\proc", dirResultats + @"\aspell\proc");
            string bin = @"\cygwin\bin";
            ExecuteCommandSync(string.Format("cd {0}\\aspell & path={1};%path% & perl.exe proc", dirResultats, bin));
            ExecuteCommandSync(string.Format("cd {0}\\aspell & path={1};%path% & bash.exe configure", dirResultats, bin));
            ExecuteCommandSync(string.Format("cd {0}\\aspell & path={1};%path% & make dist", dirResultats, bin));
            NetejaDirAspell(dirResultats, false);
        }

        //public static void GeneraAspell(string dirResultats, string nomMyspell, string nomAspell, CanviaString canvis)
        //{
        //    Encoding encoding = Encoding.Default;
        //    string path = Path.GetFullPath(string.Format(@"{0}\aspell\{1}.aspell.zip", dirResultats, nomAspell));
        //    File.Delete(path);
        //    using (ZipFile zip = new ZipFile(path))
        //    {
        //        Queue<string> perEsborrar = new Queue<string>();
        //        foreach (string fitxer in Directory.GetFiles(dirResultats + @"..\aspell", "*.*", SearchOption.TopDirectoryOnly))
        //        {
        //            if (fitxer.EndsWith("_affix.dat"))
        //            {
        //                string[] linies = File.ReadAllLines(dirResultats + @"myspell\" + nomMyspell + ".myspell.aff", Encoding.Default);
        //                string nomAff = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\" + Path.GetFileName(fitxer)), canvis);
        //                EscriuLinies(nomAff, linies, encoding, "\n");
        //                zip.AddFile(nomAff, "aspell");
        //                perEsborrar.Enqueue(nomAff);
        //            }
        //            else if (fitxer.EndsWith(".cwl"))
        //            {
        //                string nomCwl = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\" + Path.GetFileName(fitxer)), canvis);
        //                CreaFitxerCwl(Path.GetFullPath(dirResultats + @"myspell\" + nomMyspell + ".myspell.dic"), nomCwl, encoding, true, 1);
        //                zip.AddFile(nomCwl, "aspell");
        //                perEsborrar.Enqueue(nomCwl);
        //            }
        //            else
        //            {
        //                string[] linies = AdaptaFitxer(fitxer, canvis);
        //                string nom = AplicaMacros(Path.GetFullPath(dirResultats + @"aspell\" + Path.GetFileName(fitxer)), canvis);
        //                EscriuLinies(nom, linies, encoding, "\n");
        //                zip.AddFile(nom, "aspell");
        //                perEsborrar.Enqueue(nom);
        //            }
        //        }
        //        zip.Save();
        //        while (perEsborrar.Count > 0)
        //            File.Delete(perEsborrar.Dequeue());
        //    }
        //}

        ///// <summary>
        ///// Crea un tar amb els fitxers d'aspell
        ///// </summary>
        ///// <param name="dirResultats">El directori de resultats</param>
        //public static void GeneraTarAspell(string dirResultats, string versio)
        //{
        //    string tar = Path.GetFullPath(dirResultats + "\\aspell\\aspell-ca-" + versio + ".tar");
        //    StringBuilder cmd = new StringBuilder();
        //    cmd.Append("\\\"Archivos de programa\\7-zip\\7z\" a -ttar " + tar);
        //    foreach (string fitxer in Directory.GetFiles(dirResultats + @"\aspell", "*.zip", SearchOption.TopDirectoryOnly))
        //        cmd.Append(" " + Path.GetFullPath(fitxer));
        //    foreach (string fitxer in Directory.GetFiles(dirResultats + @"..\aspell\readme_tar", "*.*", SearchOption.TopDirectoryOnly))
        //        cmd.Append(" " + Path.GetFullPath(fitxer));
        //    ExecuteCommandSync(cmd.ToString());
        //}

        private static char[] illegalsAspell = "0123456789".ToCharArray();
        //private static char[] illegalsAspell = "0123456789.".ToCharArray();

        private static void CreaFitxerCwl(string dic, string cwl, Encoding encoding, bool prezip, int unDeCada)
        {
            string exp = cwl + ".exp";
            using (StreamReader ent = new StreamReader(dic, Encoding.Default))
            {
                using (StreamWriter sort = new StreamWriter(exp, false, encoding))
                {
                    sort.NewLine = "\n";
                    ent.ReadLine();
                    int numLinia = 0;
                    while (!ent.EndOfStream)
                    {
                        string linia = ent.ReadLine();
                        if (linia.Length == 0)
                            continue;
                        ++numLinia;
                        if ((numLinia % unDeCada) != 0)
                            continue;
                        //if (linia.EndsWith("."))
                        //    linia = linia.TrimEnd('.');
                        int barra = linia.IndexOf('/');
                        if ((barra >= 0 && linia.IndexOfAny(illegalsAspell, 0, barra) != -1) || (barra < 0 && linia.IndexOfAny(illegalsAspell) != -1))
                            continue;
                        sort.WriteLine(linia);
                    }
                }
            }
            string bin = @"\cygwin\bin\";
            StringBuilder cmd = new StringBuilder();
            cmd.Append(bin + "cat.exe " + exp);
            cmd.Append(" | " + bin + "sort.exe -u");
            if (prezip)
                cmd.Append(" | " + bin + "prezip-bin.exe -z");
            cmd.Append(" > " + cwl);
            ExecuteCommandSync(cmd.ToString());
            File.Delete(exp);
        }

        /// <summary>
        /// Executes a shell command synchronously.
        /// </summary>
        /// <param name="command">string command</param>
        /// <returns>string, as output of the command.</returns>
        private static string ExecuteCommandSync(string command)
        {
            try
            {
                // create the ProcessStartInfo using "cmd" as the program to be run,
                // and "/c " as the parameters.
                // Incidentally, /c tells cmd that we want it to execute the command that follows,
                // and then exit.
                System.Diagnostics.ProcessStartInfo procStartInfo =
                    new System.Diagnostics.ProcessStartInfo(@"cmd", "/c " + command);

                // The following commands are needed to redirect the standard output.
                // This means that it will be redirected to the Process.StandardOutput StreamReader.
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                // Do not create the black window.
                procStartInfo.CreateNoWindow = true;
                // Now we create a process, assign its ProcessStartInfo and start it
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                // Get the output into a string
                string result = proc.StandardOutput.ReadToEnd();
                return result;
            }
            catch (Exception objException)
            {
                // Log the exception
                return "ERROR";
            }
        }

        private static void CreaFitxerDic(string nomFitxer, List<Entrada> entrades, Marques filtre, Comparison<string> comparador)
        {
            bool posaNum = true;
            string[] liniesDic = Entrada.GeneraLiniesDic(entrades, filtre, Entrada.Speller.HUNSPELL, comparador);
            StreamWriter sw = new StreamWriter(nomFitxer + ".dic", false, Encoding.Default);
            if (posaNum)
                sw.WriteLine(liniesDic.Length);
            foreach (string linia in liniesDic)
                sw.WriteLine(linia);
            sw.Close();
        }

        private void CreaFitxerAff(string nomFitxer, Marques filtre)
        {
            string[] ids = new string[regles.Count];
            regles.Keys.CopyTo(ids, 0);
            Array.Sort(ids);
            StreamWriter sw = new StreamWriter(nomFitxer + ".aff", false, Encoding.Default);
            foreach (string str in descripcio)
                sw.WriteLine("# " + str);
            sw.WriteLine();
            sw.WriteLine("# Regles:");
            foreach (string id in ids)
            {
                Regla regla = regles[id];
                sw.WriteLine(string.Format("#     {0} {1}{2}", regla.Id, regla.ToString(), 
                    regla.CalCombinable ? string.Format(" ({0}, versió combinable)", regla.IdCombinable) : ""));
            }
            sw.WriteLine();
            sw.WriteLine("SET {0}", set);
            sw.WriteLine();
            sw.WriteLine("TRY {0}", caracters);
            if (rep.Count > 0)
            {
                sw.WriteLine();
                sw.WriteLine("REP {0}", rep.Count);
                foreach (string r in rep)
                    sw.WriteLine("REP {0}", r);
            }
            foreach (string id in ids)
            {
                Regla regla = regles[id];
                sw.WriteLine();
                string[] liniesAff = regla.GeneraAff(filtre, this);
                foreach (string linia in liniesAff)
                    sw.WriteLine(linia);
                if (regla.CalCombinable)
                {
                    sw.WriteLine();
                    liniesAff = regla.GeneraAff(filtre, regla.IdCombinable, true, this);
                    foreach (string linia in liniesAff)
                        sw.WriteLine(linia);
                }
            }
            sw.Close();
        }

        public void GeneraAffDicMyspell(string nomFitxer, List<Entrada> entrades, Marques filtre, 
            Comparison<string> comparador, GetAfinaReglaMyspell afinaRegla)
        {
            List<ItemDic> itemsVells = Entrada.GeneraItemsDic(entrades, filtre, Entrada.Speller.HUNSPELL, comparador);
            Regles reglesMyspell = AMyspell(nomFitxer, itemsVells, filtre, comparador, afinaRegla);
            reglesMyspell.CreaFitxerAff(nomFitxer, filtre);
        }

        private Regles AMyspell(string nomFitxer, List<ItemDic> itemsVells, Marques filtre,
            Comparison<string> comparador, GetAfinaReglaMyspell afinaRegla)
        {
            Regles novesRegles = new Regles();
            novesRegles.regles = new Dictionary<string, Regla>();
            novesRegles.marques = new Dictionary<string, Marca>(marques);
            novesRegles.descripcio = new List<string>(descripcio);
            novesRegles.set = set;
            novesRegles.caracters = caracters;
            novesRegles.rep = rep;
            char nomNou = '1';
            char nomNouInv = 'a';
            Dictionary<string, List<ReglaMyspell>> vellANou = new Dictionary<string, List<ReglaMyspell>>();
            foreach (KeyValuePair<string, Regla> kvReglaVella in regles)
            {
                string idReglaVella = kvReglaVella.Key;
                Regla reglaVella = kvReglaVella.Value;
                vellANou.Add(idReglaVella, new List<ReglaMyspell>());
                List<ReglaMyspell> reglesMyspell = reglaVella.GrupsMyspell(this, filtre, afinaRegla);
                int maxCasos = 0;
                ReglaMyspell millorRegla = null;
                foreach (ReglaMyspell regla in reglesMyspell)
                    if (regla.EsRegla && regla.NCasos > maxCasos)
                    {
                        maxCasos = regla.NCasos;
                        millorRegla = regla;
                    }
                foreach (ReglaMyspell regla in reglesMyspell)
                {
                    string id;
                    string desc;
                    if (regla == millorRegla)
                    {
                        id = reglaVella.Id;
                        desc = reglaVella.Descripcio;
                    }
                    else
                    {
                        desc = string.Format("Part de la regla {0} ({1})", reglaVella.Id, 
                            reglaVella.Descripcio);
                        if (regla.EsRegla)
                        {
                            id = nomNou.ToString();
                            ++nomNou;
                        }
                        else
                        {
                            id = nomNouInv.ToString();
                            ++nomNouInv;
                        }
                    }
                    regla.Descripcio = desc;
                    regla.Id = id;
                    vellANou[idReglaVella].Add(regla);
                    if (regla.EsRegla)
                        novesRegles.regles[id] = regla.Regla;
                }
            }
            List<ItemDic> itemsNous = new List<ItemDic>();
            Dictionary<string, List<CreaDicMyspell>> creadors = new Dictionary<string, List<CreaDicMyspell>>();
            foreach (ItemDic itemVell in itemsVells)
            {
                string flags = itemVell.FlagsComLletres;
                if (!creadors.ContainsKey(flags))
                    creadors.Add(flags, CreaDicMyspell.GeneraCreadors(itemVell, vellANou, novesRegles));
                foreach (CreaDicMyspell creador in creadors[flags])
                {
                    ItemDic itemNou = creador.Converteix(itemVell);
                    if (itemNou != null)
                        itemsNous.Add(itemNou);
                }
            }
            // Unim tots els ítems que comparteixen la mateixa arrel
            // Ho feim per una limitació de MySpell, que no permet arrels repetides, 
            // a diferència d'Hunspell.
            itemsNous = ItemDic.CompactaLlista(itemsNous, comparador);
            StreamWriter sw = new StreamWriter(nomFitxer + ".dic", false, Encoding.Default);
            sw.WriteLine(itemsNous.Count);
            foreach (ItemDic item in itemsNous)
                sw.WriteLine(item.ToString());
            sw.Close();
            return novesRegles;
        }

        /// <summary>
        /// Genera tots les formes a partir d'un fitxer .dic, aplicant-li aquestes regles.
        /// </summary>
        /// <param name="fitxerDic">El fitxer .dic.</param>
        /// <returns>Una llista de mots, sense ordenar i segurament amb repeticions</returns>
        public List<string> GeneraFormes(string fitxerDic, Marques filtre, bool nomesAfixos)
        {
            StreamReader fitxer = new StreamReader(fitxerDic, Encoding.Default);
            string linia;
            linia = fitxer.ReadLine();
            int mida = int.Parse(linia);
            List<ItemDic> items = new List<ItemDic>(mida);
            while (!fitxer.EndOfStream)
            {
                linia = fitxer.ReadLine();
                items.Add(ItemDic.LlegeixLiniaDic(linia));
            }
            fitxer.Close();
            List<Mot> mots = ItemDic.GeneraMots(items, filtre, this, nomesAfixos);
            List<string> formes = mots.ConvertAll<string>(delegate(Mot mot)
            {
                return mot.Forma;
            });
            return formes;
        }

        /// <summary>
        /// Diu si dos grups de regles són compatibles.
        /// Són compatibles si:
        /// - els grups són iguals (és un cas especial de la condició següent)
        /// - les regles que són prefixos o les que són sufixos coincideixen en els dos grups
        /// - Un grup està inclòs dins l'altre
        /// </summary>
        public bool ReglesCompatibles(string grup1, string grup2)
        {
            string g12 = grup1 + " " + grup2;
            if (compatibles.ContainsKey(g12))
                return compatibles[g12];
            if (grup1 == grup2)
            {
                compatibles.Add(g12, true);
                return true;
            }
            string g21 = grup2 + " " + grup1;
            if (grup1.Contains(grup2) || grup2.Contains(grup1))
            {
                compatibles.Add(g12, true);
                compatibles.Add(g21, true);
                return true;
            }
            string s1 = "", p1 = "", s2 = "", p2 = "";
            foreach (char ch in grup1)
                if (Llista[ch.ToString()].EsSufix)
                    s1 += ch;
                else
                    p1 += ch;
            foreach (char ch in grup2)
                if (Llista[ch.ToString()].EsSufix)
                    s2 += ch;
                else
                    p2 += ch;
            if (s1 == s2 || p1 == p2)
            {
                compatibles.Add(g12, true);
                compatibles.Add(g21, true);
                return true;
            }
            return false;
        }

        private Dictionary<string, bool> compatibles = new Dictionary<string,bool>();

        /// <summary>
        /// La taula de marques, indexades per identificador.
        /// </summary>
        public Dictionary<string, Marca> Marques { get { return marques; } }

        /// <summary>
        /// La taula de regles, indexades per identificador.
        /// </summary>
        public Dictionary<string, Regla> Llista { get { return regles; } }

        /// <summary>
        /// Torna el contingut d'una línia de la forma "XXX xyz...".
        /// (a l'exemple, el contingut és "xyz...")
        /// </summary>
        private string ContingutLinia(string str)
        {
            int espai = str.IndexOf(' ');
            if (espai >= 0)
                return str.Substring(espai + 1).Trim();
            else
                return "";
        }

        private void NouFirst(string que)
        {
            string[] grups = que.Split(' ');
            foreach (string grup in grups)
            {
                string[] trossos = grup.Split('/');
                if (trossos.Length < 2)
                    continue;
                for (int i = 0; i < trossos.Length - 1; i++)
                    for (int j = i + 1; j < trossos.Length; j++)
                        rep.Add(String.Format("{0} {1}", trossos[i], trossos[j]));
            }
        }

        private enum Llegint { IGNORA, DESC, GRUPS, REGLA };

        private Dictionary<string, Regla> regles;
        private Dictionary<string, Marca> marques;
        private List<string> descripcio;
        private string set;
        private string caracters;
        private List<string> rep;
        private Regex PartsGrup = new Regex(@"^(\d{3})\s+(.*)");
    }


}
