using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using xspell;

namespace catala
{
    /// <summary>
    /// Un identificador de gentilicis.
    /// Les l�nies del fitxer de gentilicis tenen una o dues paraules, com els adjectius del DIEC.
    /// Exemples: "arianyer arianyera", "�rab".
    /// S'ignoren les l�nies en blanc o comen�ades per "//"
    /// </summary>
    public class IdentificadorGentilicis : IdentificadorDIEC
    {
        /// <summary>
        /// Crea amb unes regles.
        /// </summary>
        /// <param name="regles">Les regles aplicables a una llista tipus DIEC.</param>
        public IdentificadorGentilicis(string desc, Regles regles, string fitxerExcepcions)
            : base(desc, regles, fitxerExcepcions)
        {
        }

        public override Entrada IdentificaEntrada(string linia)
        {
            return base.IdentificaEntrada(String.Format("ent={0}^cat1=adj.", linia));
        }

    }
}
