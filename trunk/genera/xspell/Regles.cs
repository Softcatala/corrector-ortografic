using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

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
            Llegeix(fitxer, nomFitxer);
            fitxer.Close();
        }

        public Regles()
        {
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
            regles.Llegeix(fitxer, nomFitxer);
            fitxer.Close();
            return regles;
        }

        static private Regex reIniciRegla = new Regex(@"^([PS]FX)\s+(\S)\s+([YN])\s+(\d+)");

        /// <summary>
        /// Llegeix d'un stream obert.
        /// </summary>
        /// <param name="lector">L'stream d'on llegirem les regles.</param>
        private void Llegeix(StreamReader lector, string nomFitxer)
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

        private static void CreaFitxerDic(string nomFitxer, List<Entrada> entrades, Marques filtre, Comparison<string> comparador)
        {
            string[] liniesDic = Entrada.GeneraLiniesDic(entrades, filtre, Entrada.Speller.HUNSPELL, comparador);
            StreamWriter sw = new StreamWriter(nomFitxer + ".dic", false, Encoding.Default);
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
