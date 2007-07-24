using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace xspell
{
    /// <summary>
    /// Una línia de text (string) amb un contingut variable en funció de les marques.
    /// </summary>
    public class LiniaMarques
    {

        public LiniaMarques()
        {
            cont = new TrossosMarques();
            totes = new Marques(false);
        }

        /// <summary>
        /// Crea una nova LiniaMarques aplicant un model a aquesta.
        /// </summary>
        /// <param name="cerca">Una expressió regular per cercar el que s'ha de canviar.</param>
        /// <param name="model">Una llista de parells original => transformat. 
        /// Les cadenes d'index parell són els originals. Les d'índex senar, són les còpies.</param>
        /// <returns>Un nou objecte amb els canvis aplicats</returns>
        public LiniaMarques AplicaModel(Regex cerca, string[] model)
        {
            LiniaMarques lm = new LiniaMarques();
            foreach (TrosMarques tros in cont)
            {
                string nou = cerca.Replace(tros.Str, delegate(Match match)
                {
                    return match.Groups[1].Value + AplicaModel(match.Groups[2].Value, model);
                });
                lm.Nou(nou, tros.Marques, tros.Excepte);
            }
            return lm;
        }

        private string AplicaModel(string on, string[] model)
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
        /// Torna el valor de la línia, segons les marques actives.
        /// </summary>
        /// <param name="marques">Les marques que condicionen el contingut de la línia.</param>
        /// <returns>Una línia depenent de les marques.</returns>
        public string Valor(Marques filtre)
        {
            return cont.Cont(filtre);
        }

        /// <summary>
        /// Torna el contingut com un diccionari de valors.
        /// La línia té el format "A=xxx & B=yyy"
        /// </summary>
        /// <param name="filtre">Les marques que condicionen el contingut del diccionari.</param>
        /// <returns>Un diccionari amb els identificadors i els valors.</returns>
        public Dictionary<string, string> Valors(Marques filtre)
        {
            Dictionary<string, string> valors = new Dictionary<string, string>();
            string linia = Valor(filtre);
            foreach (string tros in separadorIrr.Split(linia))
            {
                string cv = tros.Trim();
                if (cv.Length == 0)
                    continue;
                Match match = clauIgualValor.Match(cv);
                if (!match.Success)
                    throw new Exception(String.Format("S'esperava xxx=yyy (llegit: \"{0}\")", cv));
                valors[match.Groups[1].Value] = match.Groups[2].Value;
            }
            return valors;
        }

        public void Nou(string str, Marques marques)
        {
            totes.Mes(marques);
            cont.Nou(str, marques, null);
        }

        public void Nou(string str, Marques marques, Marques excepte)
        {
            totes.Mes(marques);
            if ((object)excepte != null)
                totes.Mes(excepte);
            cont.Nou(str, marques, excepte);
        }

        /// <summary>
        /// Torna true si alguna part de la línia conté la marca.
        /// </summary>
        public bool Conte(Marca marca)
        {
            return totes.Conte(marca);
        }

        /// <summary>
        /// Afegeix una marca com si una part de la línia la contingués.
        /// Serveix per si hi ha una marca que no estigui representada per cap tros.
        /// </summary>
        public void PosaMarca(Marca marca)
        {
            totes.Mes(marca);
        }

        private TrossosMarques cont;
        private Marques totes;
        static private Regex separadorIrr = new Regex(@"\s*&\s*");
        static private Regex clauIgualValor = new Regex(@"(.*?)=(.*)");


    }

    /// <summary>
    /// Contingut variable segons les marques.
    /// </summary>
    abstract class ContMarques
    {
        /// <summary>
        /// Torna el contingut en funció de les marques.
        /// </summary>
        abstract public string Cont(Marques marques);
    }

    /// <summary>
    /// Una cadena simple dependent d'una marca.
    /// </summary>
    class TrosMarques : ContMarques
    {
        public TrosMarques(string cont, Marques marques, Marques excepte)
        {
            this.cont = cont;
            this.marques = new Marques(marques);
            if ((object)excepte == null)
                this.excepte = null;
            else
                this.excepte = new Marques(excepte);
        }

        public void NouStr(string str)
        {
            cont = cont + str;
        }

        public override string Cont(Marques filtre)
        {
            if (filtre.Conte(marques) && ((object)excepte == null || !filtre.Conte(excepte)))
                return cont;
            else
                return null;
        }

        public override string ToString()
        {
            if (excepte == null)
                return string.Format("\"{0}\"/{1}", cont, marques);
            else
                return string.Format("\"{0}\"/{1}/!{2}", cont, marques, excepte);
        }

        public Marques Marques { get { return marques; } }
        public Marques Excepte { get { return excepte; } }
        public string Str { get { return cont; } }

        private string cont;
        private Marques marques, excepte;
    }

    /// <summary>
    /// Una llista ordenada de trossos, depenents de les marques.
    /// </summary>
    class TrossosMarques : ContMarques
    {
        public TrossosMarques()
        {
            trossos = new List<TrosMarques>();
        }

        public void Nou(string str, Marques marques, Marques excepte)
        {
            if (trossos.Count > 0)
            {
                TrosMarques darrer = trossos[trossos.Count - 1];
                if (darrer.Marques == marques && (object) darrer.Excepte == (object) excepte)
                {
                    trossos[trossos.Count - 1].NouStr(str);
                    return;
                }
            }
            TrosMarques nou = new TrosMarques(str, marques, excepte);
            trossos.Add(nou);
        }

        public IEnumerator<TrosMarques> GetEnumerator()
        {
            foreach (TrosMarques tros in trossos)
                yield return tros;
        }

        public override string Cont(Marques marques)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ContMarques tros in trossos)
            {
                string str = tros.Cont(marques);
                if (str != null)
                    sb.Append(str);
            }
            return sb.ToString();
        }

        private List<TrosMarques> trossos;
    }

}
