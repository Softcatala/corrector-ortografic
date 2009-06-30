using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Diagnostics;
using xspell;

namespace catala
{
    abstract public class IdentificadorCat : Identificador
    {
        public IdentificadorCat(string descripcio, Regles regles)
            : base(descripcio)
        {
            this.regles = regles;
            ParadigmaCat.PosaRegles(regles);
            excepcions = new Dictionary<string, LiniaMarques>();
            excepcionsEmprades = new Dictionary<string, bool>();
        }

        public override Paradigma IdentificaParadigma(Dictionary<string, string> dades,
            Dictionary<string, string> excepcions)
        {
            if (excepcions != null && excepcions.ContainsKey("PRECALC"))
                return Precalc(excepcions["PRECALC"], dades);
            if (!dades.ContainsKey("ent"))
                throw new Exception("No es troba l'arrel");
            string ent = dades["ent"];
            string arrel = dades["arrel"];
            bool nocat = false;
            bool nom = false, pron = false, adj = false, masc = false, fem = false, sing = false, plural = false;
            bool verb = false;
            bool art = false, conj = false, adv = false, prep = false, interj = false, loc = false;
            #region Explora les categories
		    int idxCat = 1;
            while (true)
            {
                string nomCat = String.Format("cat{0}", idxCat);
                string cat = Dades(dades, nomCat);
                if (cat != null)
                {
                    switch (cat)
                    {
                        case "m.":
                        case "m. ant.":
                        case "m. pop.":
                            nom = true;
                            masc = true;
                            sing = true;
                            break;
                        case "f.":
                        case "f. pop.":
                        case "f. obs.":
                            nom = true;
                            fem = true;
                            sing = true;
                            break;
                        case "m. pl.":
                            nom = true;
                            masc = true;
                            plural = true;
                            break;
                        case "f. pl.":
                            nom = true;
                            fem = true;
                            plural = true;
                            break;
                        case "pl.":
                            nom = true;
                            plural = true;
                            break;
                        case "m. i f.":
                            masc = true;
                            fem = true;
                            nom = true;
                            sing = true;
                            break;
                        case "adj. i m.":
                            adj = true;
                            masc = true;
                            nom = true;
                            sing = true;
                            break;
                        case "adj. i f.":
                            adj = true;
                            fem = true;
                            nom = true;
                            sing = true;
                            break;
                        case "adj. i pron.":
                            adj = true;
                            pron = true;
                            sing = true;
                            break;
                        case "adj.":
                            adj = true;
                            sing = true;
                            break;
                        case "adj. i m. i f.":
                        case "adj. i m. i f. pop.":
                            adj = true;
                            nom = true;
                            masc = true;
                            fem = true;
                            sing = true;
                            break;
                        case "prep.":
                            prep = true;
                            break;
                        case "loc. prep.":
                            prep = true;
                            loc = true;
                            break;
                        case "loc. adj.":
                            adj = true;
                            loc = true;
                            break;
                        case "art.":
                            art = true;
                            break;
                        case "adv.":
                            adv = true;
                            break;
                        case "loc. adv.":
                            adv = true;
                            loc = true;
                            break;
                        case "loc. adj. i loc. adv.":
                            adj = true;
                            adv = true;
                            loc = true;
                            break;
                        case "interj.":
                            interj = true;
                            break;
                        case "conj.":
                            conj = true;
                            break;
                        case "pron.":
                            if (!verb)
                                pron = true;
                            break;
                        case "v. pron.":
                        case "v. tr.":
                        case "v. tr. ant.":
                        case "v. intr.":
                        case "intr.":
                        case "tr.":
                        case "tr. pron.":
                        case "v. intr. i pron.":
                        case "v. tr. i pron.":
                        case "v. tr. i intr.":
                        case "v. intr. i tr.":
                        case "v. intr. pron.":
                        case "v. tr. i intr. pron.":
                            verb = true;
                            break;
                        case "???":
                            nocat = true;
                            break;
                        case "sing.":
                            nom = true;
                            sing = true;
                            break;
                        default:
                            throw new Exception(String.Format("No sé què fer amb {0} (\"{1}\")", dades[nomCat], dades["arrel"]));
                    }
                    idxCat += 1;
                }
                else
                    break;
            }
 
	#endregion            
            List<ParadigmaCat> pars = new List<ParadigmaCat>();
            if (nocat)
                pars.Add(paradigmes["???"]);
            if (verb)
                pars.Add(IdVerb(arrel, dades, excepcions));
            if (loc && !(nom || adj))
            {
                if (prep)
                    pars.Add(paradigmes["LOC PREP"]);
                if (adj)
                    pars.Add(paradigmes["LOC ADJ"]);
                if (adv)
                    pars.Add(paradigmes["LOC ADV"]);
            }
            else 
            {
                // Si hi ha adjectius, tenim ha totes les formes generades
                if (adj)
                    pars.Add(Id4(arrel, ADJ, dades, excepcions));
                //else if ((masc && fem) || plural)
                else if (masc && fem)
                    pars.Add(Id4(arrel, N, dades, excepcions));
                else {
                    // PER_FER: generar les altres categories encara que hi hagi adjectius o m. i f.
                    if (nom)
                    {
                        if (plural && !sing)
                            pars.Add(paradigmes[fem ? "NPF" : "NPM"]);
                        else if (masc)
                            pars.Add(Id2(arrel, NM, dades, excepcions));
                        else if (fem)
                            pars.Add(Id2(arrel, NF, dades, excepcions));
                        else
                            pars.Add(Id2(arrel, N, dades, excepcions));
                    }
                    if (pron)
                    {
                        // PER_FER: Hi pot haver pronoms que no admetin "de"
                        if (Paraula.TeVocalInicial(arrel))
                            pars.Add(paradigmes["PRON, D+"]);
                        else
                            pars.Add(paradigmes["PRON, D-"]);
                    }
                    if (prep)
                        pars.Add(paradigmes["PREP"]);
                    if (conj)
                        pars.Add(paradigmes["CONJ"]);
                    if (adv)
                    {
                        if (arrel.EndsWith("ment") || !Paraula.TeVocalInicial(arrel))
                            pars.Add(paradigmes["ADV, D-"]);
                        else
                            pars.Add(paradigmes["ADV, D+"]);
                    }
                    if (interj)
                        pars.Add(paradigmes["INTERJ"]);
                    if (art)
                        pars.Add(paradigmes["ART"]);
                }
            }
            if (pars.Count == 0) throw new Exception(String.Format(@"Sense paradigma per a ""{0}""", ent));
            if (pars.Count == 1)
                return pars[0];
            else
            {
                PC_multi multi = new PC_multi();
                foreach (ParadigmaCat par in pars)
                    multi.Add(par);
                return multi;
            }
        }

        static private MorfoGram NM = new MorfoGram(MorfoGram.eCat.NOM, MorfoGram.eGen.M);
        static private MorfoGram NF = new MorfoGram(MorfoGram.eCat.NOM, MorfoGram.eGen.F);
        static private MorfoGram N = new MorfoGram(MorfoGram.eCat.NOM);
        static private MorfoGram ADJ = new MorfoGram(MorfoGram.eCat.ADJ);
        static private MorfoGram NO_MG = new MorfoGram();

        private string Dades(Dictionary<string, string> dades, string id)
        {
            if (!dades.ContainsKey(id))
                return null;
            return dades[id];
        }

        private string ExcDef(Dictionary<string, string> excepcions, string id, string def)
        {
            if (excepcions != null && excepcions.ContainsKey(id))
                return excepcions[id];
            else
                return def;
        }

        /// <summary>
        /// Calcula el paradigma per a verbs.
        /// </summary>
        private ParadigmaCat IdVerb(string arrel, Dictionary<string, string> dades, Dictionary<string, string> exc)
        {
            if (arrel.EndsWith("ar"))
                return paradigmes["V, -ar"];
            if (arrel.EndsWith("ir"))
            {
                if (exc != null && exc.ContainsKey("IPR3"))
                    return paradigmes["V, -ir, P"];
                else
                    return paradigmes["V, -ir, I"];
            }
            if (arrel.EndsWith("re") || arrel.EndsWith("er"))
                return paradigmes["V, -re"];
            return paradigmes["???"];
        }

        /// <summary>
        /// Calcula el paradigma per a adjectius i masculins/femenins.
        /// El resultat pot tenir dues terminacions (abacial), tres (feliç) o quatre (blanc).
        /// </summary>
        private ParadigmaCat Id4(string arrel, MorfoGram morfoGram, Dictionary<string, string> dades, Dictionary<string, string> exc)
        {
            bool admetArticle = (exc == null || !exc.ContainsKey("NOART"));
            string fem;
            string masc = ParadigmaCat.Arrel0(arrel, out fem);
            if (exc != null && exc.ContainsKey("PLU") && exc["PLU"].Contains(" "))
            {
                string[] trossos = exc["PLU"].Split(' ');
                if (trossos.Length != 2) throw new Exception("S'esperaven dos trossos: " + exc["PLU"]);
                Dictionary<string, string> exc2 = new Dictionary<string, string>(exc);
                exc2["PLU"] = trossos[0];
                ParadigmaCat parMasc = Id2(masc, morfoGram | mgMasc, dades, exc2);
                exc2["PLU"] = trossos[1];
                ParadigmaCat parFem = Id2(fem, morfoGram | mgFem, dades, exc2);
                return new PC_multi(parMasc, parFem);
            }
            if (fem == masc)
            {
                // Si hi ha una forma, vol dir que és igual per al masculí i el femení, almenys en singular
                ParadigmaCat par = Id2(arrel, morfoGram, dades, exc);
                if (arrel.EndsWith("ç"))
                {
                    // Si termina en 'ç', hi ha dues formes per al plural
                    PC_multi pars = new PC_multi(par);
                    Paraula fpl = new Paraula(arrel.Substring(0, arrel.Length - 1) + "ces");
                    pars.Add(new PC_formaFixada("plural en -ces", new MorfoGram(morfoGram.Cat, MorfoGram.eGen.F, MorfoGram.eNbre.PL),
                        fpl, fpl.VocalInicial, false));
                    return pars;
                }
                else
                    return par;
            }
            else // (fem != null)
            {
                Paraula pMasc = new Paraula(masc);
                foreach (char id in "FJBHKL")
                {
                    if (id == 'H' && (!pMasc.Aguda || !pMasc.VocalFinal))
                        continue;
                    Regla regla = regles.Llista[id.ToString()];
                    List<Mot> femenins = regla.Genera(masc, mgFemSg, regles, Marques.totes, true);
                    if (femenins.Count == 0)
                        continue;
                    if (femenins.Exists(delegate(Mot mot) { return mot.Forma == fem; }))
                    {
                        string idPar = String.Format("{0}, MFSP, {1}, D{2}, L{3}",
                            morfoGram.Cat == MorfoGram.eCat.ADJ ? "A" : "N", 
                            id.ToString(),
                            pMasc.VocalInicial ? "+" : "-",
                            pMasc.VocalInicial && admetArticle ? "+" : "-");
                        ParadigmaCat par = paradigmes[idPar];
                        Paraula pFem = new Paraula(fem);
                        if (pFem.PotApostrofar(true)&& admetArticle)
                        {
                            MorfoGram mgFS = new MorfoGram(mgFemSg);
                            mgFS.Cat = morfoGram.Cat;
                            par = new PC_multi(par, new PC_formaFixada("femení singular, D+, L+", mgFS, pFem, true, true));
                            // PERFER: No hauríem d'admetre "d'" ja que està inclòs dins l'altre paradigma
                        }
                        return par;
                    }
                }
                return new PC_multi(Id2(masc, mgMascSg, dades, exc), Id2(fem, mgFemSg, dades, exc));
            }
        }

        private static MorfoGram mgFem = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.F);
        private static MorfoGram mgMasc = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.M);
        private static MorfoGram mgFemSg = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.F, MorfoGram.eNbre.SG);
        private static MorfoGram mgMascSg = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eGen.M, MorfoGram.eNbre.SG);

        /// <summary>
        /// Calcula el paradigma per a paraules amb singular i plural.
        /// </summary>
        private ParadigmaCat Id2(string arrel, MorfoGram morfoGram, Dictionary<string, string> dades, 
            Dictionary<string, string> exc)
        {
            // PERFER: mirar "dijous"
            bool admetArticle = (exc == null || !exc.ContainsKey("NOART"));
            string arrel0, arrel1;
            arrel0 = ParadigmaCat.Arrel0(arrel, out arrel1);
            if (arrel1 != arrel0)
            {
                // PER_FER: unificar amb Precalc()
                // Tenim una paraula amb trossos
                PC_multi par = new PC_multi();
                string[] trossos = arrel.Split(' ');
                for (int i = 0; i < trossos.Length; i++)
                {
                    if (trossos.Length == 0)
                        continue;
                    // PER_FER: la paraula pot tenir articles i preposicions
                    Paraula ptros = new Paraula(trossos[i]);
                    if (i == 0)
                        par.Add(new PC_formaFixada("Entrada amb espais, primer tros", morfoGram, ptros,
                            ptros.PotApostrofar(), ptros.PotApostrofar(morfoGram.Gen == MorfoGram.eGen.F)));
                    else
                        par.Add(new PC_formaFixada("Entrada amb espais, tros interior", morfoGram, ptros, false, false));
                }
                return par;
            }
            Paraula paraula = new Paraula(arrel0);
            Debug.Assert(!paraula.Forma.Contains(" "), "No hi ha d'haver espais a l'arrel");
            if (exc != null)
            {
                // suposam que el singular tenen el mateix valor de VocalInicial
                string plural = Dades(exc, "PLU");
                if (plural != null)
                {
                    if (plural.Contains(" "))
                    {
                        // Hi ha més d'un plural
                        string[] trossos = plural.Split(' ');
                        PC_multi par = new PC_multi();
                        foreach (string tros in trossos)
                            par.Add(new PC_plural_precalc(morfoGram, paraula, tros, paraula.VocalInicial, 
                                paraula.VocalInicial && admetArticle));
                        return par;
                    }
                    return new PC_plural_precalc(morfoGram, paraula,
                        plural, paraula.VocalInicial, paraula.VocalInicial && admetArticle);
                }
            }
            string admet = String.Format("D{0}, L{1}", paraula.VocalInicial ? "+" : "-",
                paraula.VocalInicial && admetArticle ? "+" : "-");
            if (morfoGram.Cat == MorfoGram.eCat.ADJ)
            {
                string id = null;
                if (paraula.VocalFinal && paraula.Aguda)
                    id = "A2T, -ns, " + admet;
                else if (paraula.SxcFinal && paraula.Aguda)
                    id = "A2T, -os, " + admet;
                else
                    id = "A2T, -s, " + admet;
                return paradigmes[id];
            }
            else // (no és un adjectiu, ho tractam com un nom)
            {
                string id = null;
                if (morfoGram.Gen == MorfoGram.eGen.F)
                {
                    // exemples: mà, casa
                    if (paraula.VocalFinal && paraula.Aguda)
                        id = "NF, -ns, " + admet;
                    else if (paraula.SxcFinal && paraula.Aguda)
                        id = "NF, -s, " + admet;
                    // PERFER: comprovar que el plural de "falç" és "falçs"
                    else
                        id = "NF, -s, " + admet;
                }
                else // (la paraula és masculina o no té gènere)
                {
                    // exemples: ca, moix, peu
                    if (paraula.VocalFinal && paraula.Aguda)
                        id = "NM, -ns, " + admet;
                    else if (paraula.SxcFinal && paraula.Aguda)
                        id = "NM, -os, " + admet;
                    else
                        id = "NM, -s, " + admet;
                }
                return paradigmes[id];
            }
            //throw new Exception("No sé què fer amb " + arrel);
        }

        private Paradigma Precalc(string precalc, Dictionary<string, string> dades)
        {
            string[] trossos = precalc.Split(' ');
            PC_multi pars = new PC_multi();
            foreach (string tros in trossos)
            {
                if (tros.Contains("/"))
                {
                    string[] parts = tros.Split('/');
                    if (parts.Length != 2) throw new Exception("Error de format: " + tros);
                    string forma = parts[0];
                    string[] flags = new string[parts[1].Length];
                    for (int i = 0; i < parts[1].Length; i++)
                        flags[i] = parts[1][i].ToString();
                    pars.Add(new PC_precalc(forma, null, flags));
                }
                else
                    pars.Add(new PC_formaFixada("tros precalculat", NO_MG, new Paraula(tros), false, false));
            }
            return pars;
        }

        public override LiniaMarques Excepcio(string ent)
        {
            if (!excepcions.ContainsKey(ent)) return null;
            excepcionsEmprades[ent] = true;
            return excepcions[ent];
        }

        public override void NovaExcepcio(string ent, LiniaMarques contingut)
        {
            if (excepcions.ContainsKey(ent)) throw new Exception("Excepció repetida: " + ent);
            excepcions[ent] = contingut;
            excepcionsEmprades[ent] = false;
        }

        public override Regles Regles { get { return regles; } }

        public override List<string> ExcepcionsSenseEmprar()
        {
            List<string> llista = new List<string>();
            foreach (KeyValuePair<string, bool> kv in excepcionsEmprades)
                if (!kv.Value)
                    llista.Add(kv.Key);
            return llista;
        }

        protected Regles regles;

        protected ParadigmaCat UnParadigma(string id)
        {
            return paradigmes[id];
        }

        private Dictionary<string, LiniaMarques> excepcions;
        private Dictionary<string, bool> excepcionsEmprades; 

        #region CreaParadigmes()
        private static Dictionary<string, ParadigmaCat> CreaParadigmes()
        {
            Dictionary<string, ParadigmaCat> llista = new Dictionary<string, ParadigmaCat>();
            for (int tp = 1; tp <= 3; ++tp) // tipus de plural: -s, -ns o -os
            {
                string descTP = null;
                switch (tp)
                {
                    case 1:
                        descTP = "-s";
                        break;
                    case 2:
                        descTP = "-ns";
                        break;
                    case 3:
                        descTP = "-os";
                        break;
                }
                for (int cat = 1; cat <= 3; ++cat) // categoria: NM, NF, ADJ (2 terminacions)
                {
                    string descCat = null;
                    MorfoGram mg = new MorfoGram();
                    switch (cat)
                    {
                        case 1:
                            descCat = "NM";
                            mg.Cat = MorfoGram.eCat.NOM;
                            mg.Gen = MorfoGram.eGen.M;
                            break;
                        case 2:
                            descCat = "NF";
                            mg.Cat = MorfoGram.eCat.NOM;
                            mg.Gen = MorfoGram.eGen.F;
                            break;
                        case 3:
                            descCat = "A2T";
                            mg.Cat = MorfoGram.eCat.ADJ;
                            break;
                    }
                    for (int admetD = 0; admetD <= 1; ++admetD)
                        for (int admetL = 0; admetL <= 1; ++admetL)
                        {
                            string desc = String.Format("{0}, {1}, D{2}, L{3}",
                                descCat, descTP,
                                admetD == 0 ? "-" : "+", admetL == 0 ? "-" : "+");
                            switch (tp)
                            {
                                case 1:
                                    llista[desc] = new PC_plural_s(mg, admetD == 1, admetL == 1, desc);
                                    break;
                                case 2:
                                    llista[desc] = new PC_plural_ns(mg, admetD == 1, admetL == 1, desc);
                                    break;
                                case 3:
                                    llista[desc] = new PC_plural_os(mg, admetD == 1, admetL == 1, desc);
                                    break;
                            }
                        }
                }
            }
            //
            llista["NPM"] = new PC_plural(true);
            llista["NPF"] = new PC_plural(false);
            //
            llista["???"] = new PC_trossos(new MorfoGram());
            llista["CONJ"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.CONJ));
            llista["PREP"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.PREP));
            llista["ART"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.ART));
            llista["PRON, D-"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.PRON), false, false, true);
            llista["PRON, D+"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.PRON), true, false, true);
            llista["LOC PREP"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.LOCPREP));
            llista["LOC ADJ"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.LOCADJ));
            llista["LOC ADV"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.LOCADV));
            llista["ADV, D-"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.ADV));
            llista["ADV, D+"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.ADV), true, false, true);
            llista["INTERJ"] = new PC_trossos(new MorfoGram(MorfoGram.eCat.INTERJ));
            //
            foreach (char ch in "FJBHKL")
            {
                string regla = ch.ToString();
                for (int an = 0; an <= 1; ++an)
                {
                    MorfoGram mg = new MorfoGram(an == 0 ? MorfoGram.eCat.ADJ : MorfoGram.eCat.NOM);
                    for (int admetD = 0; admetD <= 1; ++admetD)
                        for (int admetL = 0; admetL <= 1; ++admetL)
                        {
                            string desc = String.Format("{0}, MFSP, {1}, D{2}, L{3}",
                                an == 0 ? "A" : "N", regla,
                                admetD == 0 ? "-" : "+", admetL == 0 ? "-" : "+");
                            llista[desc] = new PC_MFSP(regla, mg, admetD == 1, admetL == 1, desc);
                        }
                }
            }
            //
            MorfoGram mgNomPropi = new MorfoGram(MorfoGram.eCat.NOMPR);
            for (int admetD = 0; admetD <= 1; ++admetD)
                for (int admetL = 0; admetL <= 1; ++admetL)
                    for (int admetN = 0; admetN <= 1; ++admetN)
                    {
                        string desc = String.Format("NP, D{0}, L{1}, N{2}",
                            admetD == 0 ? "-" : "+", admetL == 0 ? "-" : "+", admetN == 0 ? "-" : "+");
                        llista[desc] = new PC_nomPropi(desc, mgNomPropi, admetD == 1, admetL == 1, admetN == 1);
                    }
            //
            llista["V, -ar"] = new PC_verb_ar();
            llista["V, -re"] = new PC_verb_re();
            llista["V, -ir, P"] = new PC_verb_ir(true, false);
            llista["V, -ir, I"] = new PC_verb_ir(false, true);
            //
            return llista;
        }
        #endregion

        private Dictionary<string, ParadigmaCat> paradigmes = CreaParadigmes();

        public static AfinaReglaMyspell GetAfinaMyspell(Regla reglaBase, CasRegla cas)
        {
            if (reglaBase.Id == "A" && cas.Info.Temps == MorfoGram.eTemps.PAR)
                return new AfinaParticipiAR();
            if (reglaBase.Id == "M" && cas.Info.Temps == MorfoGram.eTemps.PAR)
                return new AfinaParticipiIR();
            if (reglaBase.Id == "R" && cas.Info.Temps == MorfoGram.eTemps.IPR)
                return new AfinaGerundiER();
            return null;
        }

        private class AfinaParticipiAR : AfinaReglaMyspell
        {
            public override string ProcessaArrel(string arrel)
            {
                return arrel.Substring(0, arrel.Length - 3) + "at";
            }
            public override CasRegla ProcessaCas(ReglaMyspell grup, CasRegla cas)
            {
                return new CasRegla(grup.ReglaOriginal, "at", cas.Afegir, "at", grup.ReglaOriginal.EsSufix,
                    cas.Marca, cas.Info, cas.MesRegles);
            }
        }

        private class AfinaParticipiIR : AfinaReglaMyspell
        {
            public override string ProcessaArrel(string arrel)
            {
                List<Mot> mots = new List<Mot>();
                foreach (CasRegla cas in casosMascSg)
                    if (0 < cas.Genera(arrel, ref mots, null, grup.Regles, Marques.totes, true))
                        return mots[0].Forma;
                throw new Exception("No es pot generar el participi corresponent a " + arrel);
            }
            public override CasRegla ProcessaCas(ReglaMyspell grup, CasRegla cas)
            {
                AfinaParticipiIR.grup = grup;
                if (cas.Info.Nbre == MorfoGram.eNbre.SG && cas.Info.Gen == MorfoGram.eGen.M)
                    casosMascSg.Add(cas);
                string condicio = cas.Condicio.Substring(0, cas.Condicio.Length - 3) + cas.Afegir[0] + "t";
                return new CasRegla(grup.ReglaOriginal, cas.Afegir[0] + "t", cas.Afegir, condicio, grup.ReglaOriginal.EsSufix,
                    cas.Marca, cas.Info, cas.MesRegles);
            }
            static List<CasRegla> casosMascSg = new List<CasRegla>();
            static ReglaMyspell grup;
        }

        private class AfinaGerundiER : AfinaReglaMyspell
        {
            public override string ProcessaArrel(string arrel)
            {
                return arrel.Substring(0, arrel.Length - 2) + "m";
            }
            public override CasRegla ProcessaCas(ReglaMyspell grup, CasRegla cas)
            {
                return new CasRegla(grup.ReglaOriginal, "m", cas.Afegir.Substring(1), cas.Suprimir[0] + "m",
                    grup.ReglaOriginal.EsSufix, cas.Marca, cas.Info, cas.MesRegles);
            }
        }

    }


}
