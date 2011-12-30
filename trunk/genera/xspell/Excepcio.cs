using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Dades sobre una excepció.
    /// </summary>
    public class InfoExcepcio
    {
        public InfoExcepcio(LiniaMarques contingut, FitxerFont fitxerFont, int liniaFitxerFont)
        {
            this.contingut = contingut;
            this.fitxerFont = fitxerFont;
            this.liniaFitxerFont = liniaFitxerFont;
            emprada = false;
        }

        /// <summary>
        /// El text de l'excepció (és del tipus LiniaMarques perquè pot tenir parts opcionals).
        /// </summary>
        public LiniaMarques Contingut { get { return contingut; } }

        /// <summary>
        /// El fitxer d'on s'ha agafat l'excepció.
        /// </summary>
        public FitxerFont FitxerFont { get { return fitxerFont; } }

        /// <summary>
        /// El número de línia dins el fitxer font.
        /// </summary>
        public int LiniaFitxerFont { get { return liniaFitxerFont; } }

        /// <summary>
        /// Indica si l'excepció s'ha emprat.
        /// Ho feim servir per detectar excepcions amb una entrada inexistent.
        /// </summary>
        public bool Emprada { get { return emprada; } set { emprada = value; } }

        private LiniaMarques contingut;
        private FitxerFont fitxerFont;
        private int liniaFitxerFont;
        private bool emprada;
    }
}
