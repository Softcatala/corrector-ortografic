using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace xspell
{
    /// <summary>
    /// Un dels casos d'una regla.
    /// Conté la informació per generar una forma a partir d'una arrel, un poc
    /// d'informació gramatical i, eventualment, una marca dialectal.
    /// </summary>
    public class CasRegla
    {

        /// <summary>
        /// Crea un cas per a una regla.
        /// </summary>
        /// <param name="regla">La regla a la qual pertany aquest cas.</param>
        /// <param name="suprimir">Els caràcters que s'han de suprimir. S'eliminen del principi
        /// o del final, segons si és prefix o sufix. Si és "0", no se suprimeix res.</param>
        /// <param name="afegir">El que s'ha d'afegir a l'arrel després de suprimir-ne caràcters, si cal.
        /// Se suprimeix del principi o del final, segons si és prefix o sufix.</param>
        /// <param name="condicio">La condició que ha de complir l'arrel perquè aquest cas sigui aplicable.</param>
        /// <param name="sufix">Si és true, es modifica el començament de l'arrel. Si és false, es
        /// modifica el final de l'arrel.</param>
        /// <param name="marca">La marca dialectal associada a aquest cas.</param>
        /// <param name="info">Informació morfològica i gramatical.</param>
        /// <param name="mesRegles">Regles que es poden aplicar després d'haver aplicat aquest cas.
        /// Correspon al "twofold" introduït per Hunspell.</param>
        public CasRegla(Regla regla, string suprimir, string afegir, string condicio, bool sufix, Marca marca, MorfoGram info, List<string> mesRegles)
        {
            this.regla = regla;
            this.sSuprimir = suprimir;
            this.suprimir = (suprimir == "0") ? 0 : suprimir.Length;
            this.afegir = afegir;
            sCondicio = condicio;
            if (condicio == ".")
                this.condicio = null;
            else if (sufix)
                this.condicio = new Regex(condicio + "$");
            else
                this.condicio = new Regex("^" + condicio);
            this.sufix = sufix;
            this.marca = marca;
            this.info = info;
            if (mesRegles != null)
            {
                this.mesRegles = new List<string>(mesRegles);
                this.mesRegles.Sort(string.Compare);
            }
            else
                this.mesRegles = null;
        }

        /// <summary>
        /// Crea un cas a partir d'una línia del fitxer de regles.
        /// </summary>
        /// <param name="linia">Les dades del cas.
        /// Exemple: "000     ua     ües         [gq]ua           # aigües, siliqües".</param>
        /// <param name="regla">La regla a la qual pertany aquest cas.</param>
        /// <param name="marques">La llista de marques disponibles. Se'n selecciona una a partir de la línia.</param>
        /// <returns>El cas tot just creat.</returns>
        static public CasRegla Crea(string linia, Regla regla, Dictionary<string, Marca> marques)
        {
            Match match = rePartsCas.Match(linia);
            if (!match.Success)
                throw new Exception("Error de format: " + linia);
            Marca marca = marques[match.Groups[1].Value];
            string suprimir = match.Groups[2].Value;
            string afegir = match.Groups[3].Value;
            string condicio = match.Groups[4].Value;
            MorfoGram.eCat cat = MorfoGram.eCat.NOCAT;
            MorfoGram.eTemps temps = MorfoGram.eTemps.NOTEMPS;
            MorfoGram.ePers pers = MorfoGram.ePers.NOPERS;
            MorfoGram.eGen gen = MorfoGram.eGen.NOGEN;
            MorfoGram.eNbre nbre = MorfoGram.eNbre.NONBRE;
            List<CasRegla> anteriors = regla.Casos;
            if (anteriors.Count > 0) {
                CasRegla anterior = anteriors[anteriors.Count - 1];
                MorfoGram info = anterior.Info;
                if (info != null)
                {
                    cat = info.Cat;
                    temps = info.Temps;
                    pers = info.Pers;
                    gen = info.Gen;
                    nbre = info.Nbre;
                }
            }
            string nota = match.Groups[5].Value;
            AgafaInfo(ref cat, ref temps, ref gen, ref nbre, nota);
            #region Tenim un verb
		            if (cat == MorfoGram.eCat.VERB)
            {
                // Els verbs tenen un segon nivell d'informació, amb un nombre d'1 a 6
                match = rePers.Match(nota);
                if (match.Success)
                {
                    int p = int.Parse(match.Groups[1].Value);
                    // PERFER: posar el nombre a les formes temporals?
                    switch (p)
                    {
                        case 1:
                            if (temps != MorfoGram.eTemps.PAR)
                            {
                                pers = MorfoGram.ePers.P1;
                                gen = MorfoGram.eGen.NOGEN;
                                nbre = MorfoGram.eNbre.NONBRE;
                            }
                            else
                            {
                                pers = MorfoGram.ePers.NOPERS;
                                gen = MorfoGram.eGen.M;
                                nbre = MorfoGram.eNbre.SG;
                            }
                            break;
                        case 2:
                            if (temps != MorfoGram.eTemps.PAR)
                            {
                                pers = MorfoGram.ePers.P2;
                                gen = MorfoGram.eGen.NOGEN;
                                nbre = MorfoGram.eNbre.NONBRE;
                            }
                            else
                            {
                                pers = MorfoGram.ePers.NOPERS;
                                gen = MorfoGram.eGen.M;
                                nbre = MorfoGram.eNbre.PL;
                            }
                            break;
                        case 3:
                            if (temps != MorfoGram.eTemps.PAR)
                            {
                                pers = MorfoGram.ePers.P3;
                                gen = MorfoGram.eGen.NOGEN;
                                nbre = MorfoGram.eNbre.NONBRE;
                            }
                            else
                            {
                                pers = MorfoGram.ePers.NOPERS;
                                gen = MorfoGram.eGen.F;
                                nbre = MorfoGram.eNbre.SG;
                            }
                            break;
                        case 4:
                            if (temps != MorfoGram.eTemps.PAR)
                            {
                                pers = MorfoGram.ePers.P4;
                                gen = MorfoGram.eGen.NOGEN;
                                nbre = MorfoGram.eNbre.NONBRE;
                            }
                            else
                            {
                                pers = MorfoGram.ePers.NOPERS;
                                gen = MorfoGram.eGen.F;
                                nbre = MorfoGram.eNbre.PL;
                            }
                            break;
                        case 5:
                            pers = MorfoGram.ePers.P5;
                            gen = MorfoGram.eGen.NOGEN;
                            nbre = MorfoGram.eNbre.NONBRE;
                            break;
                        case 6:
                            pers = MorfoGram.ePers.P6;
                            gen = MorfoGram.eGen.NOGEN;
                            nbre = MorfoGram.eNbre.NONBRE;
                            break;
                    }
                } // Havíem trobat una persona
            } // Teníem un verb
	        #endregion            
            List<string> mesRegles = null;
            match = reMesRegles.Match(afegir);
            if (match.Success)
            {
                afegir = match.Groups[1].Value;
                string idRegles = match.Groups[2].Value;
                mesRegles = new List<string>();
                foreach (char idRegla in idRegles)
                    mesRegles.Add(idRegla.ToString());
            }
            CasRegla cas = new CasRegla(regla, suprimir, afegir, condicio, regla.EsSufix, marca, 
                new MorfoGram(cat, temps, pers, gen, nbre), mesRegles);
            return cas;
        }

        private static void AgafaInfo(ref MorfoGram.eCat cat, ref MorfoGram.eTemps temps, 
            ref MorfoGram.eGen gen, ref MorfoGram.eNbre nbre, string nota)
        {
            Match match = reEtiInfo.Match(nota);
            if (!match.Success)
                return;
            string etiqueta = match.Groups[1].Value;
            if (etisInfo.ContainsKey(etiqueta))
            {
                MorfoGram info = etisInfo[etiqueta];
                if (info.Cat != MorfoGram.eCat.NOCAT) cat = info.Cat;
                if (info.Temps != MorfoGram.eTemps.NOTEMPS) temps = info.Temps;
                if (info.Gen != MorfoGram.eGen.NOGEN) gen = info.Gen;
                if (info.Nbre != MorfoGram.eNbre.NONBRE) nbre = info.Nbre;
            }
            else
                throw new Exception("Etiqueta d'informació desconeguda: " + etiqueta);
        }

        /// <summary>
        /// Genera mots a partir d'una arrel. Els mots creats s'afegeixen a la llista donada.
        /// </summary>
        /// <param name="mots">La llista que recull els mots creats.</param>
        /// <param name="arrel">L'arrel a la qual s'ha d'aplicar el cas.</param>
        /// <param name="infoComuna">Informació morfològica que s'ha d'afegir a la del cas.
        /// Pot ser informació comuna a totes les formes d'una entrada.</param>
        /// <param name="regles">Llista de regles en vigor, per si està definit mesRegles</param>
        /// <param name="filtre">Només es generen mots que tenen marques contingudes en aquest filtre.</param>
        /// <param name="nomesAfixos">Si és true, només s'apliquen les regles amb la propietat EsAfix = true.</param>
        /// <returns>El nombre de mots afegits.</returns>
        public int Genera(string arrel, ref List<Mot> mots, MorfoGram infoComuna, Regles regles, Marques filtre, bool nomesAfixos)
        {
            if ((suprimir > arrel.Length) || (condicio != null && !condicio.IsMatch(arrel)) || !filtre.Conte(Marca))
                return 0;
            int afegits = 0;
            string forma;
            if (sufix)
                forma = arrel.Substring(0, arrel.Length - suprimir) + afegir;
            else
                forma = afegir + arrel.Substring(suprimir);
            Mot motBase = new Mot(forma, this, infoComuna);
            mots.Add(motBase);
            afegits += 1;
            if (mesRegles != null)
                foreach (string idRegla in mesRegles)
                {
                    Regla regla = regles.Llista[idRegla];
                    if (nomesAfixos && !regla.EsAfix)
                        continue;
                    List<Mot> nous = regla.Genera(forma, motBase.Info, regles, filtre, nomesAfixos);
                    if (mesRegles.Count > 1)
                    {
                        foreach (string idRegla2 in mesRegles)
                        {
                            if (idRegla == idRegla2)
                                continue;
                            Regla regla2 = regles.Llista[idRegla2];
                            if (nomesAfixos && !regla2.EsAfix)
                                continue;
                            List<Mot> nous2;
                            foreach (Mot mot in nous)
                            {
                                nous2 = regla2.Genera(mot.Forma, mot.Info, regles, filtre, nomesAfixos);
                                mots.AddRange(nous2);
                                afegits += nous2.Count;
                            }
                        }
                    }
                    mots.AddRange(nous);
                    afegits += nous.Count;
                }
            return afegits;
        }

        /// <summary>
        /// Genera la línia corresponent a aquest cas per al fitxer .aff.
        /// </summary>
        /// <param name="regla">La regla de la qual forma part aquest cas.</param>
        /// <param name="regles">Les regles en vigor (per als flags "twofold")</param>
        /// <returns>Una línia per al fitxer .aff.</returns>
        public string GeneraAff(Regla regla, Regles regles)
        {
            return GeneraAff(regla, regla.Id, regles);
        }

        /// <summary>
        /// Genera la línia corresponent a aquest cas per al fitxer .aff.
        /// </summary>
        /// <param name="regla">La regla de la qual forma part aquest cas.</param>
        /// <param name="nouId">L'identificador que farem servir per als casos.</param>
        /// <param name="regles">Les regles en vigor (per als flags "twofold")</param>
        /// <returns>Una línia per al fitxer .aff.</returns>
        public string GeneraAff(Regla regla, string nouId, Regles regles)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0}FX {1} {2} {3}", sufix ? "S" : "P", nouId, sSuprimir, afegir);
            if (mesRegles != null)
            {
                sb.Append("/");
                foreach (string id in mesRegles)
                    sb.Append(regles.Llista[id].IdCombinable);
            }
            sb.AppendFormat(" {0}", sCondicio);
            return sb.ToString();
        }

        /// <summary>
        /// Diu si aquest cas de regla és aplicable a una cadena.
        /// ("aplicable" s'ha d'interpretar en el sentit de passar de l'arrel a la forma modificada).
        /// </summary>
        /// <param name="str">L'arrel de la qual volem saber si li és aplicable el cas.</param>
        /// <returns>true si el cas és aplicable.</returns>
        public bool EsAplicable(string str)
        {
            return (condicio == null) || condicio.IsMatch(str);
        }

        public MorfoGram Info { get { return info; } }

        public List<string> MesRegles { get { return mesRegles; } }

        public Marca Marca { get { return marca; } }

        /// <summary>
        /// Diu si aquest cas i un altre tenen les mateixes regles extra (MesRegles).
        /// </summary>
        public bool MateixesMes(CasRegla altre)
        {
            if (mesRegles == null || altre.mesRegles == null)
                return (mesRegles == null && altre.mesRegles == null);
            if (MesRegles.Count != altre.MesRegles.Count)
                return false;
            for (int i = 0; i < MesRegles.Count; i++)
                if (MesRegles[i] != altre.MesRegles[i])
                    return false;
            return true;
        }

        public override string ToString()
        {
            return string.Format("+{0}, -{1}, si {2}", afegir, suprimir, sCondicio);
        }

        private static Dictionary<string, MorfoGram> creaEtisInfo()
        {
            Dictionary<string, MorfoGram> etis = new Dictionary<string, MorfoGram>();
            // flexió nominal
            etis["PLU"] = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eNbre.PL);
            etis["FEM"] = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.F, MorfoGram.eNbre.SG);
            etis["MPL"] = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.M, MorfoGram.eNbre.PL);
            etis["FPL"] = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.F, MorfoGram.eNbre.PL);
            // flexió verbal
            etis["INF"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.INF);
            etis["GER"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.GER);
            etis["PAR"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.PAR);
            etis["IPR"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPR);
            etis["IIM"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IIM);
            etis["IPE"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IPE);
            etis["FUT"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.FUT);
            etis["CON"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.CON);
            etis["SPR"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.SPR);
            etis["SIM"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.SIM);
            etis["IMP"] = new MorfoGram(MorfoGram.eCat.VERB, MorfoGram.eTemps.IMP);
            return etis;
        }

        public string Condicio { get { return sCondicio; } }
        public string Afegir { get { return afegir; } }
        public string Suprimir { get { return sSuprimir; } }

        private string sCondicio;
        private Regex condicio;
        private string afegir;
        private string sSuprimir;
        private int suprimir;
        private Marca marca;
        private bool sufix;
        private MorfoGram info;
        private List<string> mesRegles;
        private Regla regla;

        static private Dictionary<string, MorfoGram> etisInfo = creaEtisInfo();
        static private Regex rePartsCas = new Regex(@"^(\d{3})\s+(\S+)\s+(\S+)\s+(\S+)\s?(.*)");
        static private Regex reEtiInfo = new Regex(@"<([A-Z]{3})>");
        static private Regex rePers = new Regex(@"\s([1-6])\.");
        static private Regex reMesRegles = new Regex(@"(.*)/(.*)");
    }
}
