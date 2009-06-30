using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using xspell;

namespace catala
{
    /// <summary>
    /// La base dels paradigmes catalans.
    /// </summary>
    /// <remarks>
    /// Hauríem d'assegurar que les funcions GeneraDic() i GeneraMots() generen els mateixos mots.
    /// Totes dues criden la funció GeneraFlags, però seria millor que GeneraMots() fes servir
    /// GeneraDic().
    /// </remarks>
    abstract public class ParadigmaCat : Paradigma
    {
        public ParadigmaCat(string descripcio)
            : base(descripcio)
        {
        }

        public override List<Mot> GeneraMots(Entrada entrada, Marques filtre, bool nomesAfixos)
        {
            List<ItemDic> items = Entrada.GeneraItemsDic(entrada, filtre, Entrada.Speller.HUNSPELL, Cat.Cmp);
            List<Mot> llista = GeneraMots(items, filtre, nomesAfixos);
            return llista;
        }

        public override List<ItemDic> GeneraDic(Dictionary<string, string> dades, 
            Dictionary<string, string> excepcions, Marques filtre, Entrada.Speller speller)
        {
            List<ItemDic> llista = new List<ItemDic>();
            Genera(dades, excepcions, filtre, llista);
            return llista;
        }

        protected void AfegeixItem(List<ItemDic> llista, string arrel, MorfoGram mgComuna, MorfoGram mgArrel, 
            params string[] flags)
        {
            ItemDic item = llista.Find(delegate(ItemDic i) { 
                return (i.Arrel == arrel) && MorfoGram.Unificables(i.mgComuna, mgComuna) && MorfoGram.Unificables(i.mgArrel, mgArrel);
            });
            if (item == null)
            {
                item = new ItemDic(arrel, flags);
                llista.Add(item);
            }
            else
                item.MesFlags(flags);
            item.MesMorfoGram(mgComuna, mgArrel);
        }

        protected List<Mot> GeneraMots(List<ItemDic> items, MorfoGram filtreMG, Marques filtreMRQ, bool nomesAfixos)
        {
            List<Mot> tot = GeneraMots(items, filtreMRQ, nomesAfixos);
            List<Mot> llista = tot.FindAll(delegate(Mot mot)
            {
                return MorfoGram.Unificables(mot.Info, filtreMG);
            });
            return llista;
        }

        protected List<Mot> GeneraMots(List<ItemDic> items, Marques filtre, bool nomesAfixos)
        {
            return ItemDic.GeneraMots(items, filtre, regles, nomesAfixos);
        }

        public virtual void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            throw new Exception("Sense implementar en aquesta classe!");
        }

        /// <summary>
        /// Fixa les regles que farem servir per als paradigmes.
        /// </summary>
        /// <param name="r">Les regles.</param>
        public static void PosaRegles(Regles r)
        {
            regles = r;
        }

        /// <summary>
        /// Si arrel té un espai, la divideix en dues parts.
        /// La primera es torna com a resultat de la funció i la segona es posa dins arrel1.
        /// Si arrel1 comença amb un guionet, aplica les convencions del DIEC per calcular-la
        /// a partir d'arrel0.
        /// Si arrel no té espais, la torna sense canvis.
        /// </summary>
        /// <param name="arrel">L'arrel o arrels que volem separar.</param>
        /// <param name="arrel1">Recull la segona arrel.</param>
        /// <returns>La primera arrel.</returns>
        public static string Arrel0(string arrel, out string arrel1)
        {
            int espai = arrel.IndexOf(" ");
            if (espai < 0)
            {
                arrel1 = arrel;
                return arrel;
            }
            else
            {
                string arrel0 = arrel.Substring(0, espai);
                arrel1 = arrel.Substring(espai + 1);
                if (arrel1.StartsWith("-"))
                {
                    if (arrel1.Contains(" ")) throw new Exception("S'esperaven dos trossos: " + arrel);
                    if (arrel1 == "-a")
                    {
                        if (arrel0.EndsWith("e") || arrel0.EndsWith("o"))
                            arrel1 = arrel0.Substring(0, arrel0.Length - 1) + "a";
                        else
                            arrel1 = arrel0 + "a";
                    }
                    else
                    {
                        if (arrel0.Contains("-"))
                        {
                            // Per a entrades com "barba-reveixí -reveixina"
                            string[] parts = arrel0.Split('-');
                            if (parts.Length != 2) throw new Exception("S'esperava xxx-yyy: " + arrel0);
                            arrel1 = parts[0] + arrel1;
                        }
                        else
                        {
                            Paraula pArrel0 = new Paraula(arrel0);
                            arrel1 = pArrel0.PreTonica + arrel1.Substring(1);
                        }
                    }
                }
                return arrel0;
            }
        }

        protected string ExcDef(Dictionary<string, string> excepcions, string id, string def)
        {
            if (excepcions != null && excepcions.ContainsKey(id))
                return excepcions[id];
            else
                return def;
        }

        protected static Regles regles;
        protected static MorfoGram mgFemeni = new MorfoGram(MorfoGram.eGen.F);
    }

    /// <summary>
    /// Mots que fan el plural segons la regla E (-s), G (-ns) o I (-os).
    /// Poden admetre "d'" o "l'"
    /// </summary>
    class PC_plural_EGI : ParadigmaCat
    {
        /// <summary>
        /// Crea amb una categoria gramatical i informació sobre l'inici de la paraula.
        /// </summary>
        /// <param name="regla">La regla que aplicarem per al plural.</param>
        /// <param name="mgComuna">La informació morfogramatical comuna a totes les formes.</param>
        /// <param name="admetD">Admet "d'".</param>
        /// <param name="admetL">Admet "l'".</param>
        /// <param name="desc">Descripció del paradigma.</param>
        protected PC_plural_EGI(string regla, MorfoGram mgComuna, bool admetD, bool admetL, string desc)
            : base(desc)
        {
            this.regla = regla;
            this.mgComuna = mgComuna;
            mgArrel = new MorfoGram(MorfoGram.eNbre.SG);
            this.admetD = admetD;
            this.admetL = admetL;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            string arrel0, arrel1;
            arrel0 = Arrel0(dades["arrel"], out arrel1);
            int numArrel = (mgComuna.Gen == MorfoGram.eGen.F) ? 1 : 0;
            ItemDic item = new ItemDic((numArrel == 1) ? arrel1 : arrel0, regla);
            item.MesMorfoGram(mgComuna, mgArrel);
            if (admetD)
                item.MesFlags("Y");
            if (admetL)
            {
                if (numArrel == 0)
                    item.MesFlags("V");
                else
                {
                    Paraula paraula = new Paraula(arrel1);
                    if (paraula.PotApostrofar(true))
                        item.MesFlags("V");
                }
            }
            items.Add(item);
        }

        private bool admetD, admetL;
        private MorfoGram mgComuna, mgArrel;
        private string regla;
    }

    /// <summary>
    /// Mots que fan el plural amb -s (regla E)
    /// </summary>
    class PC_plural_s : PC_plural_EGI
    {
        public PC_plural_s(MorfoGram mgComuna, bool admetD, bool admetL, string desc)
            : base("E", mgComuna, admetD, admetL, desc)
        {
        }
    }

    /// <summary>
    /// Mots que fan el plural amb -s (regla G)
    /// </summary>
    class PC_plural_ns : PC_plural_EGI
    {
        public PC_plural_ns(MorfoGram mgComuna, bool admetD, bool admetL, string desc)
            : base("G", mgComuna, admetD, admetL, desc)
        {
        }
    }

    /// <summary>
    /// Mots que fan el plural amb -os (regla I)
    /// </summary>
    class PC_plural_os : PC_plural_EGI
    {
        public PC_plural_os(MorfoGram mgComuna, bool admetD, bool admetL, string desc)
            : base("I", mgComuna, admetD, admetL, desc)
        {
        }
    }

    /// <summary>
    /// Paraules amb quatre formes (M/F x S/P) creades a partir d'una regla.
    /// </summary>
    class PC_MFSP : ParadigmaCat
    {
        /// <summary>
        /// Crea amb una regla, informació morfogramatical i informació sobre apostrofació.
        /// No funciona per a les paraules començades amb 'i' o 'u' àtones, ja que el femení i
        /// el masculí no funcionen igual pel que fa a l'article.
        /// </summary>
        /// <param name="regla">La regla per generar formes a partir del masculí singular.</param>
        /// <param name="mgComuna">Informació morfogramatical comuna a totes les formes.</param>
        /// <param name="admetD">Les formes admeten "d'".</param>
        /// <param name="admetL">Les formes admeten "l'".</param>
        /// <param name="desc">Descripció del paradigma.</param>
        public PC_MFSP(string regla, MorfoGram mgComuna, bool admetD, bool admetL, string desc)
            : base(desc)
        {
            this.regla = regla;
            this.mgComuna = mgComuna;
            this.admetD = admetD;
            this.admetL = admetL;
            mgArrel = mgComuna | mgMascSg;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            string arrel0, arrel1;
            arrel0 = Arrel0(dades["arrel"], out arrel1);
            int numArrel = (mgComuna.Gen == MorfoGram.eGen.F) ? 1 : 0;
            if (admetD)
            {
                if (admetL)
                    AfegeixItem(items, ((numArrel == 1) ? arrel1 : arrel0), mgComuna, mgArrel, regla, "V", "Y");
                else
                    AfegeixItem(items, ((numArrel == 1) ? arrel1 : arrel0), mgComuna, mgArrel, regla, "Y");
            }
            else // (!admetD)
            {
                if (admetL)
                    AfegeixItem(items, ((numArrel == 1) ? arrel1 : arrel0), mgComuna, mgArrel, regla, "V");
                else
                    AfegeixItem(items, ((numArrel == 1) ? arrel1 : arrel0), mgComuna, mgArrel, regla);
            }
        }

        private static MorfoGram mgMascSg = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.M, MorfoGram.eNbre.SG);
        private string regla;
        private bool admetD, admetL;
        private MorfoGram mgComuna, mgArrel;
    }

    /// <summary>
    /// Mots que tenen el plural precalculat, poden ser començats per vocal
    /// </summary>
    class PC_plural_precalc : ParadigmaCat
    {
        /// <summary>
        /// Crea a partir del plural i amb informació d'apostrofació.
        /// </summary>
        /// <param name="mgComuna">Informació morfològica i gramatical comuna a totes les formes.</param>
        /// <param name="singular">Paraula en singular</param>
        /// <param name="plural">Plural precalculat.</param>
        /// <param name="admetD">Totes les formes admeten la preposició "de" apostrofada.</param>
        /// <param name="admetL">El singular admet l'article apostrofat.</param>
        public PC_plural_precalc(MorfoGram mgComuna, Paraula singular, string plural, bool admetD, bool admetL)
            : base(mgComuna.ToString() + " " + singular + "/" + plural + (admetD ? ", d'" : "") + (admetL ? ", l'" : ""))
        {
            this.mgComuna = mgComuna;
            mgArrel = null;
            this.singular = singular.Forma;
            this.plural = plural;
            // Interpretam la notació "abacà -às"
            if (plural.StartsWith("-"))
                this.plural = singular.PreTonica + plural.Substring(1);
            // PERFER: mirar si és aplicable la regla 'E'
            this.admetD = admetD;
            this.admetL = admetL;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            if (admetD && admetL)
            {
                AfegeixItem(items, singular, mgComuna | mgSingular, mgArrel, "V", "Y");
                AfegeixItem(items, plural, mgComuna | mgPlural, mgArrel, "Y");
            }
            else if (admetD) // !admetL
            {
                AfegeixItem(items, singular, mgComuna | mgSingular, mgArrel, "Y");
                AfegeixItem(items, plural, mgComuna | mgPlural, mgArrel, "Y");
            }
            else if (admetL) // !admetD
            {
                AfegeixItem(items, singular, mgComuna | mgSingular, mgArrel, "V");
                AfegeixItem(items, plural, mgComuna | mgPlural, mgArrel);
            }
            else
            {
                AfegeixItem(items, singular, mgComuna | mgSingular, mgArrel);
                AfegeixItem(items, plural, mgComuna | mgPlural, mgArrel);
            }
        }

        private string singular, plural;
        private bool admetD, admetL;
        private MorfoGram mgComuna, mgArrel;
        static private MorfoGram mgPlural = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eNbre.PL);
        static private MorfoGram mgSingular = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eNbre.SG);
    }

    /// <summary>
    /// Una paraula que només té plural.
    /// Pot ser masculina o femenina.
    /// </summary>
    class PC_plural : ParadigmaCat
    {
        public PC_plural(bool masc)
            : base(String.Format("Nom {0} plural", masc ? "masculí" : "femení"))
        {
            this.masc = masc;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            string arrel = dades["arrel"];
            if (masc)
            {
                if (Paraula.TeVocalInicial(arrel))
                    //AfegeixItem(items, arrel, mgMascPl, mgMascPl, "V", "Y");  // No admet article!
                    AfegeixItem(items, arrel, mgMascPl, mgMascPl, "Y");
                else
                    AfegeixItem(items, arrel, mgMascPl, mgMascPl);
            }
            else // (femení)
            {
                Paraula pArrel = new Paraula(arrel);
                if (pArrel.VocalInicial)
                {
                    //if (pArrel.PotApostrofar(true))   // No admet article!
                    //    AfegeixItem(items, arrel, mgFemPl, mgFemPl, "V", "Y");
                    //else
                    //    AfegeixItem(items, arrel, mgFemPl, mgFemPl, "Y");
                    AfegeixItem(items, arrel, mgFemPl, mgFemPl, "Y");
                }
                else
                    AfegeixItem(items, arrel, mgFemPl, mgFemPl);
            }
        }

        private bool masc;
        private static MorfoGram mgMascPl = new MorfoGram(MorfoGram.eCat.NOM, MorfoGram.eGen.M, MorfoGram.eNbre.PL);
        private static MorfoGram mgFemPl = new MorfoGram(MorfoGram.eCat.NOM, MorfoGram.eGen.F, MorfoGram.eNbre.PL);
    }

    class PC_verb : ParadigmaCat
    {
        public PC_verb(string desc)
            : base(desc)
        {
        }

        /// <summary>
        /// Afegeix la forma femenina singular del participi amb el flag que permet posar-li l'article "l'",
        /// sempre que unaForma no comenci en "i" o "u" àtones.
        /// El paràmetre unaForma no ha de ser necessàriament el participi, sinó una paraula que es comporti 
        /// igual que el el participi femení singular pel que fa a l'apostrofació.
        /// Si s'ha d'afegir la forma (femení singular del participi) a la llista, s'agafa dels ítems que ja hi
        /// a la llista.
        /// </summary>
        protected void AfegeixParFemSg(List<ItemDic> items, string unaForma, Marques filtre)
        {
            Paraula pForma = new Paraula(unaForma);
            if (pForma.PotApostrofar(true))
            {
                List<Mot> parFem = GeneraMots(items, mgPartFemSg, filtre, true);
                foreach (Mot mot in parFem)
                    AfegeixItem(items, mot.Forma, mgVerb, mot.Info, "V", "Y");
            }
        }

        /// <summary>
        /// Afegeix un participi a la llista d'ítems.
        /// Si comença per vocal, li afegeix el flag que permet "d'" i el que permet "l'".
        /// Si comença per vocal, crida AfegeixParFemSg per si la forma femenina singular també pot dur l'article
        /// apostrofat.
        /// </summary>
        protected void AfegeixParticipi(List<ItemDic> items, string mascSg, MorfoGram mgComuna, MorfoGram mgArrel,
            Marques filtre, params string[] flags)
        {
            AfegeixItem(items, mascSg, mgComuna, mgArrel, flags);
            if (Paraula.TeVocalInicial(mascSg))
            {
                AfegeixItem(items, mascSg, mgComuna, mgArrel, "V", "Y");
                AfegeixParFemSg(items, mascSg, filtre);
            }
        }

        /// <summary>
        /// Aplica les instruccions "+" i "-", si existeixen dins la llista d'irregularitats.
        /// </summary>
        protected void AplicaMesMenys(List<ItemDic> items, Dictionary<string, string> excepcions, Marques filtre)
        {
            string menys = ExcDef(excepcions, "-", null);
            string mes = ExcDef(excepcions, "+", null);
            if (mes == null && menys == null)
                return;
            List<ItemDic> llistaPlana;
            if (menys == null)
                llistaPlana = new List<ItemDic>(items);
            else
                llistaPlana = ItemDic.Aplana(items, regles, filtre);
            if (menys != null)
            {
                if (menys.Contains("/")) throw new Exception("No sé què fer amb " + menys);
                menys = menys.Replace("<", "$");
                Regex reMenys = new Regex(menys);
                llistaPlana = llistaPlana.FindAll(delegate(ItemDic item)
                {
                    return !(reMenys.IsMatch(item.Arrel) || reMenys.IsMatch(item.mgArrel.TempsPersona));
                });
            }
            if (mes != null)
            {
                foreach (string un in mes.Split(' '))
                {
                    if (un.Length == 0) continue;
                    if (un.StartsWith("<<") || un.EndsWith(">>")) continue;
                    string[] trossos = un.Split('/');
                    switch (trossos.Length)
                    {
                        case 1:
                            // Afegeix una sola forma, eventualment amb informació morfogramatical
                            llistaPlana.Add(MesUn(un));
                            break;
                        case 2:
                            // Afegeix una arrel amb un flag, eventualment amb informació morfogramatical
                            llistaPlana.AddRange(MesDos(trossos[0], trossos[1], filtre));
                            break;
                        case 3:
                            // Afegeix una arrel amb un flag i filtra les formes obtingudes
                            llistaPlana.AddRange(MesTres(trossos[0], trossos[1], trossos[2], filtre));
                            break;
                        default:
                            throw new Exception("No sé què fer amb " + un);
                            //break;
                    }
                }
            }
            items.Clear();
            items.AddRange(llistaPlana);
        }

        /// <summary>
        /// Afegeix un participi, a partir del masculí singular
        /// </summary>
        protected void ParticipiIrregular(string par, Marques filtre, List<ItemDic> items)
        {
            if (par.Contains(" ")) throw new Exception("No sé què fer amb \"" + par + "\"");
            Match match = parRegla.Match(par);
            if (match.Success)
            {
                if (match.Groups[2].Value.Contains("/")) throw new Exception("No sé què fer amb " + par);
                AfegeixParticipi(items, match.Groups[1].Value, mgPar, mgMascSg, filtre, match.Groups[2].Value);
            }
            else if (parReglaB.IsMatch(par))
                AfegeixParticipi(items, par, mgPar, mgMascSg, filtre, "B");
            else if (parReglaJ.IsMatch(par))
                AfegeixParticipi(items, par, mgPar, mgMascSg, filtre, "J");
            else
                AfegeixParticipi(items, par, mgPar, mgMascSg, filtre, "F");
        }

        private Regex parRegla = new Regex(@"^(.*)/(.*)$");
        private Regex parReglaB = new Regex(@"[aiu]t$");
        private Regex parReglaJ = new Regex(@"[àeèéoòóuú]s$");

        private ItemDic MesUn(string arrel)
        {
            MorfoGram mg;
            SeparaArrelMG(arrel, out arrel, out mg);
            ItemDic item = null;
            if (mg.Temps == MorfoGram.eTemps.IMP)
            {
                Paraula pArrel = new Paraula(arrel);
                item = new ItemDic(arrel, pArrel.VocalFinal ? "D" : "C");
            }
            else if (mg.Temps == MorfoGram.eTemps.INF)
            {
                Paraula pInf = new Paraula(arrel);
                string flagFinal = pInf.VocalFinal ? "D" : "C";
                if (pInf.VocalInicial)
                    item = new ItemDic(arrel, "Y", flagFinal);
                else
                    item = new ItemDic(arrel, flagFinal);
            }
            else if (mg.Temps != MorfoGram.eTemps.NOTEMPS)
            {
                if (Paraula.TeVocalInicial(arrel))
                    item = new ItemDic(arrel, "Z");
            }
            if (item == null)
                item = new ItemDic(arrel);
            item.MesMorfoGram(null, mg);
            return item;
        }

        private List<ItemDic> MesDos(string arrel, string flag, Marques filtre)
        {
            MorfoGram mg;
            List<ItemDic> llista = new List<ItemDic>();
            SeparaArrelMG(arrel, out arrel, out mg);
            if (mg.Temps == MorfoGram.eTemps.PAR)
            {
                AfegeixParticipi(llista, arrel, mgPar, mgPartMascSg, filtre, flag);
            }
            else if (mg.Temps == MorfoGram.eTemps.FUT)
            {
                llista.Add(new ItemDic(arrel, flag));
                llista[0].MesMorfoGram(mgVerb, mgFut1);
            }
            else
            {
                llista.Add(new ItemDic(arrel, flag));
                llista[0].MesMorfoGram(mgVerb, mg);
            }
            return llista;
        }

        private List<ItemDic> MesTres(string arrel, string regla, string condicio, Marques filtre)
        {
            if (arrel.Contains("<") || condicio.Contains("<")) throw new Exception(string.Format("No sé què fer amb {0} o {1}", arrel, condicio));
            List<ItemDic> compacta = new List<ItemDic>();
            compacta.Add(new ItemDic(arrel, regla));
            List<ItemDic> plana = ItemDic.Aplana(compacta, regles, filtre);
            Regex reCondicio = new Regex(condicio);
            List<ItemDic> mes = plana.FindAll(delegate(ItemDic item)
            {
                return reCondicio.IsMatch(item.Arrel) || 
                    ((item.mgArrel != null) && reCondicio.IsMatch(item.mgArrel.TempsPersona));
            });
            return mes;
        }

        private void SeparaArrelMG(string tot, out string arrel, out MorfoGram mg)
        {
            Match match = reArrelMG.Match(tot);
            if (match.Success)
            {
                arrel = match.Groups[1].Value;
                mg = MorfoGram.ParseTempsPersona(match.Groups[2].Value);
            }
            else
            {
                arrel = tot;
                mg = new MorfoGram();
            }
        }

        static Regex reArrelMG = new Regex(@"^(.+)<(.+)>$");

        protected static MorfoGram mgVerb = new MorfoGram(MorfoGram.eCat.VERB);
        protected static MorfoGram mgInf = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.INF, MorfoGram.ePers.NOPERS);
        protected static MorfoGram mgGer = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.GER, MorfoGram.ePers.NOPERS);
        protected static MorfoGram mgPar = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.PAR, MorfoGram.ePers.NOPERS);
        protected static MorfoGram mgPartMascSg = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.PAR, MorfoGram.ePers.NOPERS,
            MorfoGram.eGen.M, MorfoGram.eNbre.SG);
        protected static MorfoGram mgPartFemSg = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.PAR, MorfoGram.ePers.NOPERS,
            MorfoGram.eGen.F, MorfoGram.eNbre.SG);
        protected static MorfoGram mgIpr1 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPR, MorfoGram.ePers.P1);
        protected static MorfoGram mgIpr2 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPR, MorfoGram.ePers.P2);
        protected static MorfoGram mgIpr3 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPR, MorfoGram.ePers.P3);
        protected static MorfoGram mgIpr6 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPR, MorfoGram.ePers.P6);
        protected static MorfoGram mgIim1 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IIM, MorfoGram.ePers.P1);
        protected static MorfoGram mgFut1 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.FUT, MorfoGram.ePers.P1);
        protected static MorfoGram mgSim1 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.SIM, MorfoGram.ePers.P1);
        protected static MorfoGram mgImp2 = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IMP, MorfoGram.ePers.P2);
        protected static MorfoGram mgMascSg = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.M, MorfoGram.eNbre.SG);

    }

    /// <summary>
    /// Verbs de la primera conjugació, com "cantar"
    /// </summary>
    class PC_verb_ar : PC_verb
    {
        public PC_verb_ar()
            : base("Verb en -ar")
        {
            mgVerb = new MorfoGram(MorfoGram.eCat.VERB);
            mgArrel = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IIM, MorfoGram.ePers.P3);
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            string arrel = dades["arrel"];
            string imp = arrel.Substring(0, arrel.Length - 2) + "ava";
            AfegeixItem(items, imp, mgVerb, mgArrel, "A");
            // Si el participi femení singular es pot apostrofar, l'afegim a la llista,
            // ja que la regla "A" no posa l'article en aquest cas, per evitar "l'ignorada".
            AfegeixParFemSg(items, imp, filtre);
            if (filtre.Conte("013") && ipr1BalAcc.IsMatch(arrel))
            {
                Paraula pArrel = new Paraula(arrel);
                if (pArrel.Sillabes.Length >= 3)
                {
                    // La forma resultant té més d'una síl·laba, per tant du accent ("supòs")
                    string ipr1 = null;
                    if (ProvaIpr1(arrel, ref ipr1, "às", "asar", "assar") ||
                        ProvaIpr1(arrel, ref ipr1, "ín", "inar") ||
                        ProvaIpr1(arrel, ref ipr1, "ís", "isar", "issar") ||
                        ProvaIpr1(arrel, ref ipr1, "pòs", "posar") ||
                        ProvaIpr1(arrel, ref ipr1, "ús", "usar", "ussar")
                        )
                    {
                        if (pArrel.VocalInicial)
                            AfegeixItem(items, ipr1, mgVerb, mgIpr1, "Z");
                        else
                            AfegeixItem(items, ipr1, mgVerb, mgIpr1);
                    }
                }
                else
                {
                    // La forma resultant té una sola síl·laba, no du accent ("pos")
                    string ipr1 = null;
                    if (ProvaIpr1(arrel, ref ipr1, "as", "asar", "assar") ||
                        ProvaIpr1(arrel, ref ipr1, "en", "enar") ||
                        ProvaIpr1(arrel, ref ipr1, "es", "esar", "essar") ||
                        ProvaIpr1(arrel, ref ipr1, "in", "inar") ||
                        ProvaIpr1(arrel, ref ipr1, "is", "isar", "issar") ||
                        ProvaIpr1(arrel, ref ipr1, "os", "osar", "ossar") ||
                        ProvaIpr1(arrel, ref ipr1, "us", "usar", "ussar"))
                    {
                        if (pArrel.VocalInicial)
                            AfegeixItem(items, ipr1, mgVerb, mgIpr1, "Z");
                        else
                            AfegeixItem(items, ipr1, mgVerb, mgIpr1);
                    }
                }
            }
            AplicaMesMenys(items, excepcions, filtre);
        }

        private bool ProvaIpr1(string arrel, ref string ipr1, string nou, params string[] vells)
        {
            foreach (string vell in vells)
                if (arrel.EndsWith(vell))
                {
                    ipr1 = arrel.Substring(0, arrel.Length - vell.Length) + nou;
                    return true;
                }
            return false;
        }

        // Detecta verbs que tenen accent segur en IPR1 (sense desinència, com "afín"), si són polisíl·labs
        // S'inclouen els acabats en -esar, -enar, etc., encara que només es genera la forma per als monosíl·labs
        // (els polisíl·labs en -ès, -és, etc. estan precalculats)
        private static Regex ipr1BalAcc = new Regex(@"(([aeiou]ss?)|[ei]n)ar$");
        private MorfoGram mgArrel;
    }

    /// <summary>
    /// Verbs de la segona conjugació, com "perdre" o "vèncer"
    /// </summary>
    class PC_verb_re : PC_verb
    {
        public PC_verb_re()
            : base("Verb en -re")
        {
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            Match match;
            match = reInf.Match(dades["arrel"]);
            if (!match.Success)
                throw new Exception(String.Format("S'esperava un verb en -er o -re ({0})", dades["arrel"]));
            string arrel = Cat.NoAcc(match.Groups[1].Value);
            string term = match.Groups[2].Value;
            string fut1 = ExcDef(excepcions, "FUT", (term == "er") ? arrel + "eré" : arrel + "ré");
            string ger = ExcDef(excepcions, "GER", arrel + "ent");
            string par = arrel + "ut";
            string iim1 = arrel + "ia";
            //if (!arrel.EndsWith("u"))
            {
                string ipr3;
                if (dades["arrel"].EndsWith("cer"))
                {
                    ipr3 = ExcDef(excepcions, "IPR3", arrel.Substring(0, arrel.Length - 1) + "ç");
                    par = ipr3 + "ut";
                }
                else
                {
                    match = reGer.Match(ger);
                    if (!match.Success)
                        throw new Exception(String.Format("Gerundi inesperat: {0}", ger));
                    ipr3 = ExcDef(excepcions, "IPR3", arrel.EndsWith("u") ? arrel : match.Groups[1].Value);
                    iim1 = ExcDef(excepcions, "IIM", match.Groups[1].Value + "ia");
                    if (iim1.EndsWith("iia"))
                        iim1 = match.Groups[1].Value + "a"; // estalviam una 'i', com a "seia"
                }
                if (ipr3.EndsWith("b"))
                {
                    AfegeixItem(items, ipr3 + "en", mgVerb, mgIpr6, "Z");
                    string ipr3p = ipr3.Substring(0, ipr3.Length - 1) + "p";
                    if (Paraula.TeVocalInicial(ipr3p))
                    {
                        AfegeixItem(items, ipr3p + "s", mgVerb, mgIpr2, "Z");
                        AfegeixItem(items, ipr3p, mgVerb, mgIpr3, "Z");
                    }
                    else
                    {
                        AfegeixItem(items, ipr3p + "s", mgVerb, mgIpr2);
                        AfegeixItem(items, ipr3p, mgVerb, mgIpr3);
                    }
                    AfegeixItem(items, ipr3p, mgVerb, mgImp2, "C");    // PERFER: sempre "C"?
                    if (ExcDef(excepcions, "SIM", null) == null)
                        AfegeixItem(items, ipr3p, mgVerb, mgIpr1, "Z");
                }
                else
                {
                    AfegeixItem(items, ipr3, mgVerb, mgIpr3, "S");
                    if (Paraula.TeVocalInicial(ipr3))
                        AfegeixItem(items, ipr3, mgVerb, mgIpr3, "Z");
                    Paraula pIpr3 = new Paraula(ipr3);
                    AfegeixItem(items, ipr3, mgVerb, mgImp2, pIpr3.VocalFinal ? "D" : "C");
                    if (ExcDef(excepcions, "SIM", null) == null)
                        AfegeixItem(items, ipr3, mgVerb, mgIpr1, "Z");
                }
            }
            if (excepcions == null || !excepcions.ContainsKey("PAR"))
                AfegeixParticipi(items, par, mgPar, mgMascSg, filtre, "B");
            else
                ParticipiIrregular(ExcDef(excepcions, "PAR", null), filtre, items);
            // PERFER: verbs en -ure (?)
            string sim1 = ExcDef(excepcions, "SIM", arrel + "és");
            Paraula pInf = new Paraula(dades["arrel"]);
            AfegeixItem(items, dades["arrel"], mgVerb, mgInf, pInf.VocalFinal ? "D" : "C");
            AfegeixItem(items, ger, mgVerb, mgGer, "R");
            AfegeixItem(items, fut1, mgVerb, mgFut1, "T");
            AfegeixItem(items, iim1, mgVerb, mgIim1, "O");
            AfegeixItem(items, sim1, mgVerb, mgSim1, simGQ.IsMatch(sim1) ? "Q" : "P");
            if (pInf.VocalInicial)
                AfegeixItem(items, dades["arrel"], mgVerb, mgInf, "Y");
            AplicaMesMenys(items, excepcions, filtre);
        }

        private Regex reInf = new Regex(@"(.*)(re|er)$");
        private Regex reGer = new Regex(@"(.*)[ei]nt");
        private Regex simGQ = new Regex(@"[gq]ués$");
    }

    /// <summary>
    /// Verbs de la tercera conjugació, com "enaltir"
    /// </summary>
    class PC_verb_ir : PC_verb
    {
        public PC_verb_ir(bool pur, bool inc)
            : base(string.Format("Verb en -ir{0}{1}", pur ? ", pur" : "", inc ? ", incoatiu" : ""))
        {
            mgVerb = new MorfoGram(MorfoGram.eCat.VERB);
            mgArrel = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.FUT, MorfoGram.ePers.P1);
            this.pur = pur;
            this.inc = inc;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            string fut = dades["arrel"] + 'é';
            AfegeixItem(items, fut, mgVerb, mgArrel, "M");
            if (inc)
                AfegeixItem(items, fut, mgVerb, mgArrel, "N");
            if (pur)
            {
                string ipr3 = excepcions["IPR3"];
                Paraula pIpr3 = new Paraula(ipr3);
                AfegeixItem(items, ipr3, mgVerb, mgIpr3, "U");
                if (pIpr3.VocalInicial)
                    AfegeixItem(items, ipr3, mgVerb, mgIpr1, "Z");
                else
                    AfegeixItem(items, ipr3, mgVerb, mgIpr1);
                AfegeixItem(items, ipr3, mgVerb, mgImp2, pIpr3.VocalFinal ? "D" : "C");
            }
            if (excepcions != null && excepcions.ContainsKey("PAR"))
            {
                List<ItemDic> llistaPlana = ItemDic.Aplana(items, regles, filtre);
                llistaPlana = llistaPlana.FindAll(delegate(ItemDic item)
                {
                    return item.mgArrel.Temps != MorfoGram.eTemps.PAR;
                });
                ParticipiIrregular(ExcDef(excepcions, "PAR", null), filtre, llistaPlana);
                items.Clear();
                items.AddRange(llistaPlana);
            }
            else
                AfegeixParFemSg(items, fut, filtre);
            AplicaMesMenys(items, excepcions, filtre);
        }

        private bool pur, inc;
        private MorfoGram mgArrel;
    }

    /// <summary>
    /// Parts d'un topònim.
    /// Pot admetre l'article o la preposició apostrofats.
    /// </summary>
    class PC_toponim : ParadigmaCat
    {
        public PC_toponim(string forma, bool admetD, bool admetL)
            : base(String.Format(@"""{0}""{1}{2}", forma, admetD ? " + d'" : "", admetL ? " + l'" : ""))
        {
            this.forma = forma;
            this.admetD = admetD;
            this.admetL = admetL;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            ItemDic item = new ItemDic(forma);
            if (admetD)
                item.MesFlags("Y");
            if (admetL)
                item.MesFlags("V");
            items.Add(item);
        }

        private string forma;
        private bool admetD, admetL;
    }

    /// <summary>
    /// Mots per als quals s'ha calcultat l'arrel i els flags aplicables.
    /// </summary>
    class PC_precalc : ParadigmaCat
    {
        public PC_precalc(string forma, MorfoGram mg, params string[] flags)
            : base(string.Format(@"""{0}"" x {{{1}}}", forma, String.Join(", ", flags)))
        {
            this.forma = forma;
            this.mg = mg;
            this.flags = flags;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            ItemDic item = new ItemDic(forma, flags);
            if (mg != null)
                item.MesMorfoGram(mg, mg);
            items.Add(item);
        }

        private string forma;
        private string[] flags;
        private MorfoGram mg;
    }

    /// <summary>
    /// Mots amb una forma prefixada, poden anar precedits de "d'" o "l'"
    /// Pot ser una part d'un altre mot.
    /// </summary>
    class PC_formaFixada : ParadigmaCat
    {
        /// <summary>
        /// Crea a partir de la forma i amb informació d'apostrofació.
        /// </summary>
        /// <param name="desc">Descripció del paradigma.</param>
        /// <param name="mgComuna">Informació morfològica i gramatical comuna a totes les formes.</param>
        /// <param name="paraula">La paraula.</param>
        /// <param name="admetD">Totes les formes admeten la preposició "de" apostrofada.</param>
        /// <param name="admetL">El singular admet l'article apostrofat.</param>
        public PC_formaFixada(string desc, MorfoGram mgComuna, Paraula paraula, bool admetD, bool admetL)
            : base(desc)
        {
            this.mgComuna = mgComuna;
            mgArrel = null;
            this.forma = paraula.Forma;
            this.admetD = admetD;
            this.admetL = admetL;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            if (admetD && admetL)
                AfegeixItem(items, forma, mgComuna, mgArrel, "V", "Y");
            else if (admetD) // !admetL
                AfegeixItem(items, forma, mgComuna, mgArrel, "Y");
            else if (admetL) // !admetD
                AfegeixItem(items, forma, mgComuna, mgArrel, "V");
            else
                AfegeixItem(items, forma, mgComuna, mgArrel);
        }

        private string forma;
        private bool admetD, admetL;
        private MorfoGram mgComuna, mgArrel;
    }

    /// <summary>
    /// Noms propis. Van en majúscula i poden dur "d'", "l'", o "n'"
    /// </summary>
    class PC_nomPropi : ParadigmaCat
    {
        /// <summary>
        /// Crea a partir de la forma i amb informació d'apostrofació.
        /// </summary>
        /// <param name="desc">Descripció del paradigma.</param>
        /// <param name="forma">La forma del nom.</param>
        /// <param name="admetD">Admet la preposició "de" apostrofada.</param>
        /// <param name="admetL">Admet l'article apostrofat.</param>
        /// <param name="admetN">Admet l'article personal apostrofat</param>
        public PC_nomPropi(string desc, MorfoGram mgComuna, bool admetD, bool admetL, bool admetN)
            : base(desc)
        {
            this.mgComuna = mgComuna;
            this.admetD = admetD;
            this.admetL = admetL;
            this.admetN = admetN;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            ItemDic item = new ItemDic(dades["arrel"]);
            item.MesMorfoGram(mgComuna, null);
            if (admetD)
                item.MesFlags("Y");
            if (admetL)
                item.MesFlags("V");
            if (admetN)
                item.MesFlags("W");
            items.Add(item);
        }

        private bool admetD, admetL, admetN;
        private MorfoGram mgComuna;
    }

    /// <summary>
    /// Genera els trossos que formen l'arrel, sense morfologia.
    /// Els trossos s'obtenen dividint l'arrel amb els espais que conté.
    /// Hi ha els flags admetD i admetL, per permetre "d'" i "l'".
    /// El flag esLlista permet considerar l'arrel com una llista (de formes equivalents)
    /// o com una locució. Si és així, admetD i admetL només afecten el primer tros. Altrament,
    /// afecten tots els trossos.
    /// Si els trossos comencen per "d'" o "l'", s'afegeixen els flags corresponents.
    /// </summary>
    class PC_trossos : ParadigmaCat
    {
        /// <summary>
        /// Crea amb informació morfogramatical.
        /// No admet a cap tros.
        /// </summary>
        /// <param name="mgComuna">Informació morfològica i gramatical comuna a totes les formes.</param>
        public PC_trossos(MorfoGram mgComuna)
            : base(mgComuna.ToString() + ", D-, L-")
        {
            this.mgComuna = mgComuna;
            admetD = false;
            admetL = false;
            esLlista = false;
            mgArrel = null;
        }

        /// <summary>
        /// Crea amb informació morfogramatical.
        /// </summary>
        /// <param name="mgComuna">Informació morfològica i gramatical comuna a totes les formes.</param>
        /// <param name="admetD">Admet "d'".</param>
        /// <param name="admetL">Admet "l'".</param>
        /// <param name="esLlista">Si els trossos són una llista. Si és true, admetD i admetL
        /// afecten tots els trossos. Si és false, només afecten el primer tros.</param>
        public PC_trossos(MorfoGram mgComuna, bool admetD, bool admetL, bool esLlista)
            : base(mgComuna.ToString() + (admetD ? ", D+" : ", D-"))
        {
            this.mgComuna = mgComuna;
            this.admetD = admetD;
            this.admetL = admetL;
            this.esLlista = esLlista;
            mgArrel = null;
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            if (dades["arrel"].EndsWith(" -a"))
            {
                string arrel0, arrel1;
                arrel0 = Arrel0(dades["arrel"], out arrel1);
                Afegeix(arrel0, items, true);
                Afegeix(arrel1, items, true);
            }
            else if (dades["arrel"].Contains(" "))
            {
                string[] trossos = dades["arrel"].Split(' ');
                for (int i = 0; i < trossos.Length; i++)
                {
                    string tros = trossos[i];
                    if (tros.Length != 0)
                        Afegeix(tros, items, (i == 0) || esLlista);
                }
            }
            else
                Afegeix(dades["arrel"], items, true);
        }

        private void Afegeix(string que, List<ItemDic> items, bool admetOK)
        {
            if (admetOK && (admetD || admetL))
            {
                if (admetD && admetL)
                    AfegeixItem(items, que, mgComuna, mgArrel, "V", "Y");
                else if (admetD)
                    AfegeixItem(items, que, mgComuna, mgArrel, "Y");
                else
                    AfegeixItem(items, que, mgComuna, mgArrel, "V");
                return;
            }
            // PER_FER: generar amb els flags o fer que l'entrada contengui "d'" o "l'". Així, evitaríem permetre formes que no funcionen aïlladament.
            Match match = reD.Match(que);
            if (match.Success)
            {
                AfegeixItem(items, match.Groups[1].Value, mgComuna, mgArrel, "Y");
                return;
            }
            match = reL.Match(que);
            if (match.Success)
            {
                AfegeixItem(items, match.Groups[1].Value, mgComuna, mgArrel, "V");
                return;
            }
            AfegeixItem(items, que, mgComuna, mgArrel);
        }

        private bool admetD, admetL, esLlista;
        private MorfoGram mgComuna, mgArrel;
        private static Regex reD = new Regex(@"^d'(.*)");
        private static Regex reL = new Regex(@"^l'(.*)");
    }

    /// <summary>
    /// Un conjunt de paradigmes aplicats a la mateixa entrada.
    /// </summary>
    class PC_multi : ParadigmaCat
    {
        /// <summary>
        /// Crea sense elements.
        /// </summary>
        public PC_multi()
            : base("paradigma múltiple")
        {
            paradigmes = new List<ParadigmaCat>();
        }

        /// <summary>
        /// Crea amb un element.
        /// </summary>
        /// <param name="par">El primer paradigma de la llista.</param>
        public PC_multi(ParadigmaCat par)
            : base("paradigma múltiple")
        {
            paradigmes = new List<ParadigmaCat>();
            paradigmes.Add(par);
        }

        /// <summary>
        /// Crea amb dos elements.
        /// </summary>
        /// <param name="par1">El primer paradigma de la llista.</param>
        /// <param name="par2">El segon paradigma de la llista.</param>
        public PC_multi(ParadigmaCat par1, ParadigmaCat par2)
            : base("paradigma múltiple")
        {
            paradigmes = new List<ParadigmaCat>();
            paradigmes.Add(par1);
            paradigmes.Add(par2);
        }

        /// <summary>
        /// Afegeix un paradigma a la llista.
        /// </summary>
        /// <param name="nou">El nou paradigma.</param>
        public void Add(ParadigmaCat nou)
        {
            paradigmes.Add(nou);
        }

        /// <summary>
        /// Suprimeix un paradigma de la llista.
        /// </summary>
        /// <param name="par">El paradigma que s'ha de suprimir.</param>
        public void Remove(ParadigmaCat par)
        {
            paradigmes.Remove(par);
        }

        public override void Genera(Dictionary<string, string> dades, Dictionary<string, string> excepcions, Marques filtre, List<ItemDic> items)
        {
            foreach (ParadigmaCat par in paradigmes)
                par.Genera(dades, excepcions, filtre, items);
        }

        private List<ParadigmaCat> paradigmes;
    }

}
