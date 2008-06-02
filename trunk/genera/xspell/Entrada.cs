using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace xspell
{
    /// <summary>
    /// Una unitat lèxica, com una entrada del diccionari.
    /// Té mètodes per generar informació per als diversos correctors.
    /// Una entrada té una arrel, un paradigma i, eventualment, informació sobre excepcions.
    /// Una entrada pot tenir una o més marques, que es fan servir a l'hora de generar 
    /// diccionaris (s'exclouen les entrades que tenen marques no incloses al filtre).
    /// </summary>
    public class Entrada
    {
        /// <summary>
        /// Crea una entrada, amb una arrel, un paradigma i informació extra.
        /// La informació extra es pot referir a excepcions, origen de la paraula, etc.
        /// Per defecte, les entrades tenen la marca '000'.
        /// </summary>
        /// <param name="identPar">L'objecte que identifica el paradigma al qual pertany l'entrada.</param>
        /// <param name="dades">Informació sobre l'entrada</param>
        public Entrada(Identificador identificador, Dictionary<string, string> dades)
        {
            this.dades = dades;
            this.excepcions = null;
            this.identificador = identificador;
            marques = null;
        }

        /// <summary>
        /// Llegeix entrades d'un fitxer, les identifica amb idPar i les afegeix a la llista.
        /// </summary>
        /// <param name="nomFitxer">El nom del fitxer amb les entrades. Hi ha una entrada per línia.
        /// S'ignoren les línies en blanc i les que comencen per "//".</param>
        /// <param name="identificador">Identificador de paradigmes. Es crida per a cada línia que pot contenir
        /// una entrada. Si idPar torna una Entrada, s'afegeix a la llista. Si torna null, no es
        /// fa res.</param>
        /// <param name="llista">La llista a la qual s'afegeixen les entrades que es troben.</param>
        /// <returns>El nombre d'entrades llegides del fitxer.</returns>
        public static int LlegeixEntrades(string nomFitxer, Identificador identificador, ref List<Entrada> llista)
        {
            int llegides = 0;
            return llegides;
        }

        /// <summary>
        /// Genera la llista de mots corresponent a aquesta entrada.
        /// </summary>
        /// <param name="filtre">Només es generen els mots que tenen marques contingudes en aquest filtre.</param>
        /// <param name="nomesAfixos">Només s'apliquen les regles que afegeixen informació morfològica i gramatical.</param>
        /// <returns>La llista dels mots generats.</returns>
        public List<Mot> GeneraMots(Marques filtre, bool nomesAfixos)
        {
            List<ItemDic> items = Entrada.GeneraItemsDic(this, filtre, Speller.HUNSPELL, String.Compare);
            return ItemDic.GeneraMots(items, filtre, identificador.Regles, nomesAfixos);
        }

        /// <summary>
        /// Genera les línies d'un fitxer .dic a partir de la llista d'entrades.
        /// No conté cap línia amb el nombre de línies.
        /// Només s'inclouen les entrades que tenen totes les marques incloses dins filtre.
        /// </summary>
        /// <param name="entrades">Les entrades que ha de contenir el fitxer .dic.</param>
        /// <param name="speller">El client per al qual generarem les línies .dic.</param>
        /// <param name="comparador">La funció de comparació.</param>
        /// <returns>Una llista de línies amb el format adequat, ordenada segons el comparador.</returns>
        public static string[] GeneraLiniesDic(IEnumerable<Entrada> entrades, Marques filtre, 
            Speller speller, Comparison<string> comparador)
        {
            List<ItemDic> llista = Entrada.GeneraItemsDic(entrades, filtre, speller, comparador);
            //Regles regles = null;
            Entrada entrada0 = null;
            foreach (Entrada ent in entrades)
            {
                entrada0 = ent;
                break;
            }
            Dictionary<string, List<ItemDic>> dic = new Dictionary<string, List<ItemDic>>();
            List<ItemDic> llistaCompactada = new List<ItemDic>(llista.Count);
            foreach (ItemDic id in llista)
            {
                bool crea = true, afegeix = true;
                if (dic.ContainsKey(id.Arrel))
                {
                    crea = false;
                    foreach(ItemDic item in dic[id.Arrel])
                        if (item.FlagsCompatibles(id, entrada0.Identificador.Regles))
                        {
                            item.MesFlags(id);
                            afegeix = false;
                        }
                }
                if (afegeix)
                {
                    if (crea)
                        dic[id.Arrel] = new List<ItemDic>();
                    dic[id.Arrel].Add(id);
                    llistaCompactada.Add(id);
                }
            }
            string[] linies = new string[llistaCompactada.Count];
            for (int i = 0; i < llistaCompactada.Count; i++)
                linies[i] = llistaCompactada[i].ToString();
            return linies;
        }

        public static List<ItemDic> GeneraItemsDic(IEnumerable<Entrada> entrades, Marques filtre,
            Speller speller, Comparison<string> comparador)
        {
            List<ItemDic> llista = new List<ItemDic>();
            Marques filtreExc = new Marques(filtre);
            Dictionary<string, string> dades;
            foreach (Entrada ent in entrades)
            {
                if (!filtre.Conte(ent.Marques))
                    continue;
                List<ItemDic> ids;
                Paradigma par = null;
                if (ent.Excepcions == null)
                {
                    par = ent.Identificador.IdentificaParadigma(ent.Dades, null);
                    ids = par.GeneraDic(ent.Dades, null, filtre, speller);
                    llista.AddRange(ids);
                }
                else
                {
                    for (int g = 1; g <= 2; g++)
                    {
                        Marca grup = (g == 1) ? Marca.grup1 : Marca.grup2;
                        if (!ent.Excepcions.Conte(grup))
                            continue;
                        filtreExc.Menys(Marca.grups12);
                        filtreExc.Mes(grup);
                        Dictionary<string, string> excepcions = ent.Excepcions.Valors(filtreExc);
                        dades = new Dictionary<string, string>(ent.Dades);
                        if (excepcions.ContainsKey("MODEL"))
                        {
                            string[] model = excepcions["MODEL"].Split('/');
                            if (excepcions.Count == 1)
                                AplicaModel(dades, out excepcions, model, ent.Identificador.Excepcio(model[0]), filtreExc);
                            else
                            {
                                // A part del model, hi ha més dades
                                Dictionary<string, string> excepcionsOriginals = new Dictionary<string,string>(excepcions);
                                AplicaModel(dades, out excepcions, model, ent.Identificador.Excepcio(model[0]), filtreExc);
                                foreach (KeyValuePair<string, string> kv in excepcionsOriginals)
                                    excepcions[kv.Key] = kv.Value;
                            }
                        }
                        if (excepcions.ContainsKey("IGNORA")) // sempre deu ser IGNORA=1
                            continue;
                        if (excepcions.ContainsKey("ALT"))
                            dades["arrel"] = excepcions["ALT"];
                        if (excepcions.ContainsKey("NOVACAT"))
                            dades["cat1"] = excepcions["NOVACAT"];
                        par = ent.Identificador.IdentificaParadigma(dades, excepcions);
                        ids = par.GeneraDic(dades, excepcions, filtre, speller);
                        llista.AddRange(ids);
                    }
                }
            }
            llista.Sort(delegate(ItemDic id1, ItemDic id2)
            {
                return comparador(id1.Arrel, id2.Arrel);
            });
            return llista;
        }

        private static void AplicaModel(Dictionary<string, string> dades, out Dictionary<string, string> excepcions,
            string[] model, LiniaMarques lmExcepcions, Marques filtre)
        {
            dades["ent"] = AplicaModel(dades["ent"], model);
            dades["arrel"] = AplicaModel(dades["arrel"], model);
            List<string> originals = new List<string>();
            for (int i = 1; i < model.Length; i += 2)
                originals.Add(model[i]);
            string oo = String.Join("|", originals.ToArray());
            Regex cerca = new Regex(string.Format("(^|[=/ ])({0})", oo));
            lmExcepcions = lmExcepcions.AplicaModel(cerca, model);
            excepcions = lmExcepcions.Valors(filtre);
        }

        private static string AplicaModel(string on, string[] model)
        {
            for (int i = 1; i < model.Length; i += 2)
            {
                string vell = model[i];
                string nou = model[i + 1];
                if (on.StartsWith(vell))
                    return nou + on.Substring(vell.Length);
            }
            return on;
        }

        /// <summary>
        /// Genera les línies d'un fitxer .dic per a una entrada.
        /// </summary>
        /// <param name="entrada">L'entrada a partir de la qual volem generar.</param>
        /// <param name="identificador">Un identificador per als paradigmes.</param>
        /// <param name="filtre">Les marques que volem incloure al resultat.</param>
        /// <param name="speller">El client per al qual generarem les línies .dic.</param>
        /// <param name="comparador">La funció de comparació.</param>
        /// <returns>Una llista de línies amb el format adequat, ordenada segons el comparador.</returns>
        public static string[] GeneraLiniesDic(Entrada entrada, Marques filtre, 
            Speller speller, Comparison<string> comparador)
        {
            List<Entrada> entrades = new List<Entrada>();
            entrades.Add(entrada);
            return GeneraLiniesDic(entrades, filtre, speller, comparador);
        }

        /// <summary>
        /// Genera les línies d'un fitxer .dic per a una entrada.
        /// </summary>
        /// <param name="entrada">L'entrada a partir de la qual volem generar.</param>
        /// <param name="filtre">Les marques que volem incloure al resultat.</param>
        /// <param name="speller">El client per al qual generarem les línies .dic.</param>
        /// <param name="comparador">La funció de comparació.</param>
        /// <returns>Una llista de línies amb el format adequat, ordenada segons el comparador.</returns>
        public static List<ItemDic> GeneraItemsDic(Entrada entrada, Marques filtre,
            Speller speller, Comparison<string> comparador)
        {
            List<Entrada> entrades = new List<Entrada>();
            entrades.Add(entrada);
            return GeneraItemsDic(entrades, filtre, speller, comparador);
        }

        /// <summary>
        /// Els correctors que tractarem.
        /// </summary>
        public enum Speller { MYSPELL, HUNSPELL };

        public string Arrel { get { return dades["arrel"]; } }

        public string Ent { get { return dades["ent"]; } }

        public Dictionary<string, string> Dades { get { return dades; } }

        public LiniaMarques Excepcions { get { return excepcions; } set { excepcions = value; } }

        public Identificador Identificador { get { return identificador; } }

        public Marques Marques { get {
            if ((Object)marques == null)
            {
                if (dades.ContainsKey("mrc1"))
                {
                    marques = new Marques(false);
                    int idx = 1;
                    while (dades.ContainsKey("mrc" + idx.ToString()))
                    {
                        marques.Mes(Marca.Una(dades["mrc" + idx.ToString()]));
                        ++idx;
                    }
                }
                else
                    marques = marquesDefecte;
            }
            return marques; 
        } }

        public override string ToString()
        {
            return String.Format("Entrada: \"{0}\"", Arrel);
        }

        private Dictionary<string, string> dades;
        private LiniaMarques excepcions;
        private Identificador identificador;
        private Marques marques;

        static private Marques marquesDefecte = new Marques(false, "000");
    }
}
