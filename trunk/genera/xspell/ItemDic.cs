using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace xspell
{
    /// <summary>
    /// Un element del diccionari.
    /// Conté una arrel i una llista de flags.
    /// En el futur pot contenir més informació (com la que usa Hunspell)
    /// <remarks>Aquesta classe pressuposa que els flags estan entre 'A' i 'Z' i
    /// consten d'una sola lletra. Si hi ha flags d'altres formes, s'haurà de 
    /// revisar la classe.</remarks>
    /// </summary>
    public class ItemDic
    {
        /// <summary>
        /// Crea a partir d'una arrel i uns flags.
        /// </summary>
        /// <param name="arrel">L'arrel de l'ítem</param>
        /// <param name="flags">Els flags de l'ítem. Les lletres han d'estar entre 'A' i 'Z'.</param>
        public ItemDic(string arrel, params string[] flags)
        {
            this.arrel = arrel;
            for (int i = 0; i < flags.Length; i++)
                this.flags[i] = 0;
            flagsComLletres = null;
            MesFlags(flags);
        }

        private ItemDic(string arrel, uint[] flags, MorfoGram mgComuna, MorfoGram mgArrel)
        {
            this.arrel = arrel;
            for (int i = 0; i < flags.Length; i++)
                this.flags[i] = flags[i];
            MesMorfoGram(mgComuna, mgArrel);
            flagsComLletres = null;
        }

        public static ItemDic LlegeixLiniaDic(string linia)
        {
            if (linia.Contains("/"))
            {
                string[] trossos = linia.Split('/');
                if (trossos.Length != 2) throw new Exception("S'esperaven dos trossos: " + linia);
                char[] cflags = trossos[1].ToCharArray();
                string[] flags = Array.ConvertAll<char, string>(cflags, delegate(char ch)
                {
                    return ch.ToString();
                });
                return new ItemDic(trossos[0], flags);
            }
            else
                return new ItemDic(linia);
        }

        /// <summary>
        /// Aplica un model a aquest ítem. Si és aplicable, torna un nou ítem amb els canvis.
        /// Si no és aplicable, torna null.
        /// </summary>
        /// <param name="vell">El tros que se cercarà al començament de l'arrel.</param>
        /// <param name="nou">El tros que substituirà el vell.</param>
        /// <returns>Un ítem modificat o null.</returns>
        public ItemDic AplicaModel(string vell, string nou)
        {
            if (!arrel.StartsWith(vell))
                return null;
            return new ItemDic(nou + arrel.Substring(vell.Length), flags, mgCom, mgArr);
        }

        /// <summary>
        /// Afegeix més flags a aquest ítem.
        /// </summary>
        /// <param name="flags">Els flags que volem afegir a aquest ítem.</param>
        public void MesFlags(params string[] flags)
        {
            foreach (string flag in flags)
            {
                if (flag.Length != 1) throw new ItemDicException("Flag inadmissible: " + flag);
                uint idx, mascara;
                MascaraFlag(flag[0], out idx, out mascara);
                this.flags[idx] |= mascara;
            }
            flagsComLletres = null;
        }

        /// <summary>
        /// Afegeix els flags d'un altre ítem.
        /// </summary>
        public void MesFlags(ItemDic altre)
        {
            for (int i = 0; i < flags.Length; i++)
                this.flags[i] |= altre.flags[i];
            flagsComLletres = null;
        }

        /// <summary>
        /// Mostra el contingut de l'ítem
        /// </summary>
        /// <returns>El contingut en el format myspell/ispell</returns>
        public override string ToString()
        {
            string flags = FlagsComLletres;
            if (flags != "")
                return String.Format("{0}/{1}", arrel, flags);
            else
                return arrel;
        }

        public string FlagsComLletres
        {
            get
            {
                if (flagsComLletres == null)
                {
                    StringBuilder lletres = new StringBuilder();
                    for (uint i = 0; i < flags.Length; i++)
                    {
                        uint ff = flags[i];
                        if (ff == 0)
                            continue;
                        for (int j = 0; j < 32; j++)
                            if ((ff & (0x1 << j)) != 0)
                                lletres.Append((char)(i * 32 + j));
                    }
                    flagsComLletres = lletres.ToString();
                }
                return flagsComLletres;
            }
        }

        public string Arrel { get { return arrel; } }

        public Entrada Entrada { get { return entrada; } set { entrada = value; } }

        public List<string> LlistaFlags
        {
            get 
            {
                List<string> llista = new List<string>();
                string lletres = FlagsComLletres;
                foreach (char c in lletres)
                    llista.Add(c.ToString());
                return llista;
            }
        }
        public MorfoGram mgComuna { get { return mgCom; } }
        public MorfoGram mgArrel { get { return mgArr; } }

        /// <summary>
        /// Diu si els flags d'aquest ítem són compatibles amb els d'un altre.
        /// Són compatibles si:
        /// - Són iguals
        /// </summary>
        public bool FlagsCompatibles(ItemDic altre, Regles regles)
        {
            return regles.ReglesCompatibles(FlagsComLletres, altre.FlagsComLletres);
        }

        /// <summary>
        /// Compacta una llista d'ítems.
        /// Torna una llista amb la longitud mínima i ordenada.
        /// Uneix els ítems que tenguin la mateixa arrel i siguin compatibles.
        /// El procés d'unió es repeteix fins que es pugui.
        /// La informació morfològica i gramatical dels ítems es perd.
        /// </summary>
        /// <param name="llista">La llista d'entrada, segurament amb repeticions i ítems que es poden unir.</param>
        /// <param name="regles">Les regles per interpretar els flags dels ítems.</param>
        /// <param name="comparador">Una funció per ordenar la llista resultant.</param>
        /// <returns>Una llista compactada.</returns>
        public static List<ItemDic> CompactaLlista(List<ItemDic> llista, Regles regles, Comparison<string> comparador)
        {
            Dictionary<string, List<ItemDic>> unics = new Dictionary<string, List<ItemDic>>();
            foreach (ItemDic item in llista)
            {
                if (!unics.ContainsKey(item.Arrel))
                    unics.Add(item.Arrel, new List<ItemDic>());
                ItemDic nou = new ItemDic(item.Arrel);
                nou.MesFlags(item);
                List<ItemDic> grup = unics[item.Arrel];
                grup.Add(nou);
                while (grup.Count > 1)
                {
                    bool canvis = false;
                    for(int i=0; i<grup.Count - 1; i++) 
                    {
                        for (int j = i + 1; j < grup.Count; j++)
                        {
                            ItemDic item1 = grup[i];
                            ItemDic item2 = grup[j];
                            if (item1.FlagsCompatibles(item2, regles))
                            {
                                item1.MesFlags(item2);
                                grup.Remove(item2);
                                canvis = true;
                                break;
                            }
                        }
                        if (canvis)
                            break;
                    }
                    break;
                }
            }
            List<ItemDic> llistaNova = new List<ItemDic>(unics.Count * 2);
            foreach (List<ItemDic> grup in unics.Values)
                llistaNova.AddRange(grup);
            llistaNova.Sort(delegate(ItemDic item1, ItemDic item2)
            {
                int cmp = comparador(item1.Arrel, item2.Arrel);
                return (cmp != 0) ? cmp : string.Compare(item1.FlagsComLletres, item2.FlagsComLletres);
            });
            return llistaNova;
        }

        /// <summary>
        /// Compacta una llista d'ítems.
        /// Torna una llista amb la longitud mínima i ordenada.
        /// Uneix els ítems que tenguin la mateixa arrel, idenpendentment dels flags.
        /// La informació morfològica i gramatical dels ítems es perd.
        /// </summary>
        /// <param name="llista">La llista d'entrada, segurament amb repeticions i ítems que es poden unir.</param>
        /// <param name="comparador">Una funció per ordenar la llista resultant.</param>
        /// <returns>Una llista compactada.</returns>
        public static List<ItemDic> CompactaLlista(List<ItemDic> llista, Comparison<string> comparador)
        {
            Dictionary<string, ItemDic> unics = new Dictionary<string, ItemDic>();
            foreach (ItemDic item in llista)
            {
                if (!unics.ContainsKey(item.Arrel))
                    unics.Add(item.Arrel, new ItemDic(item.Arrel));
                unics[item.Arrel].MesFlags(item);
            }
            List<ItemDic> llistaNova = new List<ItemDic>(unics.Values);
            llistaNova.Sort(delegate(ItemDic item1, ItemDic item2)
            {
                int cmp = comparador(item1.Arrel, item2.Arrel);
                return (cmp != 0) ? cmp : string.Compare(item1.Arrel, item2.Arrel);
            });
            return llistaNova;
        }

        /// <summary>
        /// Afegeix informació morfogramatical compartida per totes les formes generades
        /// per aquest ítem.
        /// </summary>
        /// <param name="novaComuna">La informació nova per a mgComuna.</param>
        /// <param name="novaArrel">La informació nova per a mgArrel.</param>
        public void MesMorfoGram(MorfoGram novaComuna, MorfoGram novaArrel)
        {
            if (novaComuna != null)
            {
                if (mgCom == null)
                    mgCom = new MorfoGram();
                mgCom |= novaComuna;
            }
            if (novaArrel != null)
            {
                if (mgArr == null)
                    mgArr = new MorfoGram();
                mgArr |= novaArrel;
            }
        }

        private void MascaraFlag(char flag, out uint idx, out uint mascara)
        {
            int f = (int)flag;
            if (f > 255) throw new Exception("Flag il·legal!");
            idx = (uint)(f >> 5);
            mascara = (uint)(1 << f);
        }

        /// <summary>
        /// Torna una llista d'ItemDic aplanada.
        /// Els ítems de la llista nova només contenen flags per a regles amb EsSufix = false.
        /// </summary>
        /// <param name="items">La llista d'ítems que volem aplanar.</param>
        /// <param name="regles">La llista de regles que emprarem.</param>
        /// <param name="filtre">Només es generen mots que tenen marques contingudes en aquest filtre.</param>
        /// <returns>La llista aplanada.</returns>
        public static List<ItemDic> Aplana(List<ItemDic> items, Regles regles, Marques filtre)
        {
            // PERFER: No desplegar els futurs i els condicionals, ja que són sempre regulars (?)
            List<ItemDic> llista = new List<ItemDic>();
            foreach (ItemDic item in items)
            {
                string arrel = item.Arrel;
                List<Regla> aff = null;
                List<string> noAff = new List<string>();
                foreach (string idRegla in item.LlistaFlags)
                {
                    Regla regla = regles.Llista[idRegla];
                    if (regla.EsAfix)
                    {
                        if (aff == null)
                            aff = new List<Regla>();
                        aff.Add(regla);
                    }
                    else
                    {
                        noAff.Add(idRegla);
                    }
                }
                if (aff == null)
                {
                    // PER_FER: mirar si no hem de posar totes les regles de noaff
                    llista.Add(item);
                }
                else // (aff != null)
                {
                    ItemDic nou = null;
                    // Afegim l'arrel nua
                    nou = new ItemDic(arrel, noAff.ToArray());
                    nou.MesMorfoGram(item.mgComuna, item.mgArrel);
                    llista.Add(nou);
                    // Afegim els mots generats per les regles d'aff
                    foreach (Regla regla in aff)
                    {
                        List<Mot> mots = new List<Mot>();
                        foreach (CasRegla cas in regla.Casos)
                        {
                            mots.Clear();
                            int generats = cas.Genera(arrel, ref mots, item.mgComuna, regles, filtre, true);
                            if (generats == 0)
                                continue;
                            if (generats > 1) throw new Exception("S'esperava un sol mot per al cas");
                            Mot mot = mots[0];
                            nou = new ItemDic(mot.Forma);
                            if (regla.EsCombinable)
                                foreach (string id in noAff)
                                {
                                    Regla reglaNoAff = regles.Llista[id];
                                    if (reglaNoAff.EsCombinable && reglaNoAff.EsAplicable(nou.Arrel))
                                        nou.MesFlags(id);
                                }
                            if (cas.MesRegles != null)
                            {
                                foreach (string id in cas.MesRegles)
                                {
                                    Regla reglaExtra = regles.Llista[id];
                                    if (reglaExtra.EsAplicable(nou.Arrel))
                                        nou.MesFlags(id);
                                }
                            }
                            nou.MesMorfoGram(item.mgComuna, mot.Info);
                            llista.Add(nou);
                        }
                    }
                }
            }
            return llista;
        }

        static public List<Mot> GeneraMots(List<ItemDic> items, Marques filtre, Regles regles, bool nomesAfixos)
        {
            List<Mot> llista = new List<Mot>();
            foreach (ItemDic item in items)
            {
                string arrel = item.Arrel;
                List<Regla> pre = null;
                List<Regla> suf = null;
                foreach (string idRegla in item.LlistaFlags)
                {
                    Regla regla = regles.Llista[idRegla];
                    if (regla.EsSufix)
                    {
                        if (suf == null)
                            suf = new List<Regla>();
                        suf.Add(regla);
                    }
                    else if (!nomesAfixos || regla.EsAfix)
                    {
                        if (pre == null)
                            pre = new List<Regla>();
                        pre.Add(regla);
                    }
                }
                if (pre == null && suf == null)
                {
                    llista.Add(new Mot(arrel, null, item.mgArrel | item.mgComuna));
                }
                else if (pre == null || suf == null)
                {
                    llista.Add(new Mot(arrel, null, item.mgArrel | item.mgComuna));
                    foreach (Regla regla in ((pre != null) ? pre : suf))
                        llista.AddRange(regla.Genera(arrel, regla.EsAfix ? item.mgComuna : item.mgComuna | item.mgArrel,
                            regles, filtre, nomesAfixos));
                }
                else // (pre != null && suf != null)
                {
                    // Afegim l'arrel nua
                    llista.Add(new Mot(arrel, null, item.mgArrel | item.mgComuna));
                    // Afegim l'arrel combinada amb els prefixos
                    // (com que no afegim sufixos a l'arrel, té els trets procedents de mgArrel)
                    foreach (Regla reglaPre in pre)
                        llista.AddRange(reglaPre.Genera(arrel, item.mgArrel | item.mgComuna, regles, filtre, nomesAfixos));
                    foreach (Regla reglaSuf in suf)
                    {
                        // Afegim l'arrel combinada amb els sufixos 
                        List<Mot> mots = reglaSuf.Genera(arrel, reglaSuf.EsAfix ? item.mgComuna : item.mgComuna | item.mgArrel,
                            regles, filtre, nomesAfixos);
                        llista.AddRange(mots);
                        if (!reglaSuf.EsCombinable)
                            continue;
                        // Afegim els prefixos a cadascun dels sufixos que ho permetin
                        // Els mots hereden la informació procedent dels sufixos
                        foreach (Regla reglaPre in pre)
                        {
                            if (!reglaPre.EsCombinable)
                                continue;
                            foreach (Mot mot in mots)
                                llista.AddRange(reglaPre.Genera(mot.Forma, mot.Info, regles, filtre, nomesAfixos));
                        }
                    }
                }
            }
            return llista;
        }

        public Paradigma Paradigma { set { paradigma = value; } get { return paradigma; } }

        private string arrel;
        private uint[] flags = new uint[256 / (8 * sizeof(uint))];
        private MorfoGram mgCom, mgArr;
        string flagsComLletres;
        private Entrada entrada;
        private Paradigma paradigma;
    }
}
