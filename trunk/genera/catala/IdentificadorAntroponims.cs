using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using xspell;

namespace catala
{
    /// <summary>
    /// Un identificador d'antropònims.
    /// Cada línia conté un nom o un cognom, i eventualment la marca " f".
    /// " f" indica que la paraula, encara que comenci per vocal no admet "n'".
    /// Exemples: "Isaura f", "Joan".
    /// S'ignoren les línies en blanc o començades per "//"
    /// </summary>
    public class IdentificadorAntroponims : IdentificadorDIEC
    {
        /// Crea amb unes regles.
        /// </summary>
        /// <param name="regles">Les regles aplicables a una llista tipus DIEC.</param>
        public IdentificadorAntroponims(string desc, Regles regles, string fitxerExcepcions)
            : base(desc, regles, fitxerExcepcions)
        {
        }
        /// <summary>
        public override Entrada IdentificaEntrada(string linia)
        {
            Dictionary<string, string> dades = new Dictionary<string, string>();
            string arrel = linia.Trim();
            bool femeni = false;
            if (linia.EndsWith(" f"))
            {
                arrel = linia.Substring(0, linia.Length - 2).Trim();
                femeni = true;
            }
            dades["ent"] = arrel;
            dades["arrel"] = arrel;
            string idPar = null;
            if (Paraula.TeVocalInicial(arrel))
                idPar = "NP, D+, L-, N" + (femeni ? "-" : "+");
            else
                idPar = "NP, D-, L-, N-";
            dades["par"] = idPar;
            return new Entrada(this, dades);
        }

        public override Paradigma IdentificaParadigma(Dictionary<string, string> dades, Dictionary<string, string> excepcions)
        {
            return UnParadigma(dades["par"]);
        }

    }
}
