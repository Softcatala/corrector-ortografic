using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Informació morfològica i gramatical.
    /// És un conjunt de valors, que poden estar indefinits.
    /// </summary>
    public class MorfoGram
    {
        /// <summary>
        /// Crea amb tots els valors indefinits.
        /// </summary>
        public MorfoGram()
        {
        }

        /// <summary>
        /// Crea sumant els valors de dos objectes MorfoGram.
        /// Per als objectes inexistents, se suposa el valor NOCAT, NOTEMPS, etc.
        /// Si els valors són incompatibles, es provoca una excepció.
        /// </summary>
        /// <param name="mf1">El primer MorfoGram</param>
        /// <param name="mf2">El segon MorfoGram</param>
        public MorfoGram(MorfoGram mf1, MorfoGram mf2)
        {
            if (mf1 == null && mf2 == null)
                return;
            if (mf1 == null || mf2 == null)
            {
                MorfoGram mfx = (mf1 != null) ? mf1 : mf2;
                cat = mfx.Cat;
                temps = mfx.Temps;
                pers = mfx.Pers;
                gen = mfx.Gen;
                nbre = mfx.Nbre;
                return;
            }
            if (Unificables(mf1, mf2))
            {
                cat = (mf1.Cat != eCat.NOCAT) ? mf1.Cat : mf2.Cat;
                temps = (mf1.Temps != eTemps.NOTEMPS) ? mf1.Temps : mf2.Temps;
                pers = (mf1.Pers != ePers.NOPERS) ? mf1.Pers : mf2.Pers;
                gen = (mf1.Gen != eGen.NOGEN) ? mf1.Gen : mf2.Gen;
                nbre = (mf1.Nbre != eNbre.NONBRE) ? mf1.Nbre : mf2.Nbre;
            }
            else
            {
                if ((mf1.Cat != eCat.NOCAT) && (mf2.Cat != eCat.NOCAT) && (mf1.Cat != mf2.Cat))
                    throw new Exception(String.Format("Categories incompatibles ({0}, {1})", mf1.ToString(), mf2.ToString()));
                if ((mf1.Temps != eTemps.NOTEMPS) && (mf2.Temps != eTemps.NOTEMPS) && (mf1.Temps != mf2.Temps))
                    throw new Exception(String.Format("Temps incompatibles ({0}, {1})", mf1.ToString(), mf2.ToString()));
                if ((mf1.Pers != ePers.NOPERS) && (mf2.Pers != ePers.NOPERS) && (mf1.Pers != mf2.Pers))
                    throw new Exception(String.Format("Persones incompatibles ({0}, {1})", mf1.ToString(), mf2.ToString()));
                if ((mf1.Gen != eGen.NOGEN) && (mf2.Gen != eGen.NOGEN) && (mf1.Gen != mf2.Gen))
                    throw new Exception(String.Format("Gèneres incompatibles ({0}, {1})", mf1.ToString(), mf2.ToString()));
                if ((mf1.Nbre != eNbre.NONBRE) && (mf2.Nbre != eNbre.NONBRE) && (mf1.Nbre != mf2.Nbre))
                    throw new Exception(String.Format("Categories incompatibles ({0}, {1})", mf1.ToString(), mf2.ToString()));
            }
        }

        /// <summary>
        /// Crea copiant les dades d'un altre.
        /// </summary>
        /// <param name="mg">L'origen de les dades.</param>
        public MorfoGram(MorfoGram mg) : this(mg, null)
        {
        }

        /// <summary>
        /// Diu si dos conjunts de trets són unificables.
        /// Un valor és unificable amb ell mateix i amb el valor NOXXX
        /// </summary>
        /// <param name="mf1">Un primer conjunt de trets.</param>
        /// <param name="mf2">Un segon conjunt de trets.</param>
        /// <returns>True si els dos conjunts són unificables.</returns>
        static public bool Unificables(MorfoGram mf1, MorfoGram mf2)
        {
            if (mf1 == null || mf2 == null)
                return true;
            if ((mf1.Cat != eCat.NOCAT) && (mf2.Cat != eCat.NOCAT) && (mf1.Cat != mf2.Cat))
                return false;
            if ((mf1.Temps != eTemps.NOTEMPS) && (mf2.Temps != eTemps.NOTEMPS) && (mf1.Temps != mf2.Temps))
                return false;
            if ((mf1.Pers != ePers.NOPERS) && (mf2.Pers != ePers.NOPERS) && (mf1.Pers != mf2.Pers))
                return false;
            if ((mf1.Gen != eGen.NOGEN) && (mf2.Gen != eGen.NOGEN) && (mf1.Gen != mf2.Gen))
                return false;
            if ((mf1.Nbre != eNbre.NONBRE) && (mf2.Nbre != eNbre.NONBRE) && (mf1.Nbre != mf2.Nbre))
                return false;
            return true;
        }

        /// <summary>
        /// Crea amb una categoria.
        /// </summary>
        /// <param name="cat">La categoria</param>
        public MorfoGram(eCat cat)
        {
            this.cat = cat;
        }

        /// <summary>
        /// Crea amb una categoria i un temps.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="temps">El temps verbal</param>
        public MorfoGram(eCat cat, eTemps temps)
        {
            this.cat = cat;
            this.temps = temps;
        }

        /// <summary>
        /// Crea amb una categoria, un temps i una persona.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="temps">El temps verbal</param>
        /// <param name="pers">La persona</param>
        public MorfoGram(eCat cat, eTemps temps, ePers pers)
        {
            this.cat = cat;
            this.temps = temps;
            this.pers = pers;
        }

        /// <summary>
        /// Crea amb una categoria i un gènere.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="gen">El gènere</param>
        public MorfoGram(eCat cat, eGen gen)
        {
            this.cat = cat;
            this.gen = gen;
        }

        /// <summary>
        /// Crea amb un gènere.
        /// </summary>
        /// <param name="gen">El gènere</param>
        public MorfoGram(eGen gen)
        {
            this.gen = gen;
        }

        /// <summary>
        /// Crea amb una categoria i un nombre.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="nbre">El nombre</param>
        public MorfoGram(eCat cat, eNbre nbre)
        {
            this.cat = cat;
            this.nbre = nbre;
        }

        /// <summary>
        /// Crea amb un nombre.
        /// </summary>
        /// <param name="gen">El nombre</param>
        public MorfoGram(eNbre nbre)
        {
            this.nbre = nbre;
        }

        /// <summary>
        /// Crea amb una categoria, un gènere i un nombre.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="gen">El gènere</param>
        /// <param name="nbre">El nombre</param>
        public MorfoGram(eCat cat, eGen gen, eNbre nbre)
        {
            this.cat = cat;
            this.gen = gen;
            this.nbre = nbre;
        }

        /// <summary>
        /// Crea amb una categoria, un gènere i un nombre.
        /// </summary>
        /// <param name="cat">La categoria</param>
        /// <param name="temps">El temps verbal</param>
        /// <param name="pers">La persona</param>
        /// <param name="gen">El gènere</param>
        /// <param name="nbre">El nombre</param>
        public MorfoGram(eCat cat, eTemps temps, ePers pers, eGen gen, eNbre nbre)
        {
            this.cat = cat;
            this.temps = temps;
            this.pers = pers;
            this.gen = gen;
            this.nbre = nbre;
        }

        public static MorfoGram operator |(MorfoGram primer, MorfoGram segon)
        {
            return new MorfoGram(primer, segon);
        }

        public static int Cmp(MorfoGram mf1, MorfoGram mf2)
        {
            if ((mf1 == null) && (mf2 == null)) return 0;
            if (mf1 == null) return -1;
            if (mf2 == null) return 1;
            if (mf1.Cat != mf2.Cat) return (int)mf1.Cat - (int)mf2.Cat;
            if (mf1.Temps != mf2.Temps) return (int)mf1.Temps - (int)mf2.Temps;
            if (mf1.Pers != mf2.Pers) return (int)mf1.Pers - (int)mf2.Pers;
            if (mf1.Gen != mf2.Gen) return (int)mf1.Gen - (int)mf2.Gen;
            if (mf1.Nbre != mf2.Nbre) return (int)mf1.Nbre - (int)mf2.Nbre;
            return 0;
        }

        public eCat Cat { get { return cat; } set { cat = value;  } }
        public eTemps Temps { get { return temps; } set { temps = value; } }
        public ePers Pers { get { return pers; } set { pers = value; } }
        public eGen Gen { get { return gen; } set { gen = value; } }
        public eNbre Nbre { get { return nbre; } set { nbre = value; } }

        public override string ToString()
        {
            StringBuilder strb = new StringBuilder();
            if (cat != eCat.NOCAT) strb.Append("." + cat.ToString());
            if (temps != eTemps.NOTEMPS) strb.Append("." + temps.ToString());
            if (pers != ePers.NOPERS) strb.Append("." + pers.ToString());
            if (gen != eGen.NOGEN) strb.Append("." + gen.ToString());
            if (nbre != eNbre.NONBRE) strb.Append("." + nbre.ToString());
            if (strb.Length == 0)
                return "ND";
            else
                return strb.ToString(1, strb.Length - 1);
        }

        /// <summary>
        /// El temps i la persona en el format emprat a la llista d'irregularitats.
        /// Exemples: IPR3, IMP2.
        /// </summary>
        public string TempsPersona
        {
            get 
            {
                StringBuilder sb = new StringBuilder(temps == eTemps.NOTEMPS ? "???" : temps.ToString());
                sb.Append((int)pers);
                return sb.ToString();
            }
        }

        public static MorfoGram ParseTempsPersona(string tp)
        {
            if (!taulaTP.ContainsKey(tp)) throw new Exception("No es pot identificar " + tp);
            return taulaTP[tp];
        }

        private static Dictionary<string, MorfoGram> CreaTaulaTP()
        {
            Dictionary<string, MorfoGram> taula = new Dictionary<string,MorfoGram>();
            taula.Add("INF0", new MorfoGram(eCat.VERB, eTemps.INF));
            taula.Add("GER0", new MorfoGram(eCat.VERB, eTemps.GER));
            taula.Add("PAR0", new MorfoGram(eCat.VERB, eTemps.PAR, ePers.NOPERS, eGen.M, eNbre.SG));
            taula.Add("PAR1", new MorfoGram(eCat.VERB, eTemps.PAR, ePers.NOPERS, eGen.M, eNbre.SG));
            taula.Add("PAR2", new MorfoGram(eCat.VERB, eTemps.PAR, ePers.NOPERS, eGen.M, eNbre.PL));
            taula.Add("PAR3", new MorfoGram(eCat.VERB, eTemps.PAR, ePers.NOPERS, eGen.F, eNbre.SG));
            taula.Add("PAR4", new MorfoGram(eCat.VERB, eTemps.PAR, ePers.NOPERS, eGen.F, eNbre.PL));
            for (eTemps temps = eTemps.IPR; temps <= eTemps.IMP; ++temps)
            {
                int primera = ((temps == eTemps.IMP) ? 2 : 1);
                for (int p = primera; p <= 6; p++)
                {
                    string id = string.Format("{0}{1}", temps, p);
                    MorfoGram mg = new MorfoGram(eCat.VERB, temps, (ePers)p);
                    taula.Add(id, mg);
                }
            }
            return taula;
        }

        private static Dictionary<string, MorfoGram> taulaTP = CreaTaulaTP();

        public enum eCat { NOCAT, NOM, NOMPR, VERB, ADJ, LOCADJ, ART, PREP, LOCPREP, ADV, LOCADV, INTERJ, CONJ, PRON };
        public enum eTemps { NOTEMPS, INF, GER, PAR, IPR, IIM, IPE, FUT, CON, SPR, SIM, IMP };
        public enum ePers { NOPERS, P1, P2, P3, P4, P5, P6 }
        public enum eGen { NOGEN, M, F }
        public enum eNbre { NONBRE, SG, PL }

        private eCat cat = eCat.NOCAT;
        private eTemps temps = eTemps.NOTEMPS;
        private ePers pers = ePers.NOPERS;
        private eGen gen = eGen.NOGEN;
        private eNbre nbre = eNbre.NONBRE;
    }
}
