using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using xspell;

namespace catala
{
    /// <summary>
    /// Un identificador de paraules diverses.
    /// </summary>
    public class IdentificadorDiversos : IdentificadorDIEC
    {
        /// Crea amb unes regles.
        /// </summary>
        /// <param name="regles">Les regles aplicables a una llista tipus DIEC.</param>
        public IdentificadorDiversos(string desc, Regles regles)
            : base(desc, regles, null)
        {
        }
        
        /// <summary>
        /// Extreu una entrada de la línia.
        /// </summary>
        /// <param name="linia">
        /// La línia amb l'entrada.
        /// Si comença per "d'", es considera que l'entrada permet "d'".
        /// Si comença per "l'", es considera que l'entrada permet "d'" i "l'" (i "s'").
        /// </param>
        /// <returns>Una entrada, o null.</returns>
        public override Entrada IdentificaEntrada(string linia)
        {
            Dictionary<string, string> dades = new Dictionary<string, string>();
            string arrel = linia;
            bool admetD = false, admetL = false;
            Match match;
            match = reD.Match(linia);
            if (match.Success)
            {
                arrel = match.Groups[1].Value;
                admetD = true;
            }
            match = reL.Match(linia);
            if (match.Success)
            {
                arrel = match.Groups[1].Value;
                admetL = true;
                admetD = true;
            }
            dades["ent"] = arrel;
            dades["arrel"] = arrel;
            string idPar;
            if (admetD || admetL)
                idPar = String.Format("NP, D{0}, L{1}, N-", admetD ? "+" : "-", admetL ? "+" : "-");
            else
                idPar = "NP, D-, L-, N-";
            dades["par"] = idPar;
            return new Entrada(this, dades);
        }

        public override Paradigma IdentificaParadigma(Dictionary<string, string> dades, Dictionary<string, string> excepcions)
        {
            return UnParadigma(dades["par"]);
        }

        static private Regex reD = new Regex(@"^d'(.*)");
        static private Regex reL = new Regex(@"^l'(.*)");
    }
}
