using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// La suma d'una cadena de car�cters (la forma) i informaci� morfol�gica o gramatical.
    /// </summary>
    public class Mot
    {
        /// <summary>
        /// Crea un mot amb una forma, amb el cas de la regla que l'ha generat i amb informaci� morfol�gica
        /// i gramatical. 
        /// La informaci� morfol�gica i gramatical del mot �s la combinaci� de la informaci� provinent del
        /// cas de la regla (casRegla) i la d'info.
        /// </summary>
        /// <param name="forma">La forma del mot.</param>
        /// <param name="casRegla">El cas de la regla a partir del qual s'ha generat el mot.</param>
        /// <param name="extra">Informaci� morfol�gica i gramatical que s'afegeix a la del cas de la regla.</param>
        public Mot(string forma, CasRegla casRegla, MorfoGram extra)
        {
            this.forma = forma;
            this.casRegla = casRegla;
            this.extra = extra;
            this.info = null;
        }

        /// <summary>
        /// Torna una cadena amb les formes dels mots d'una llista ordenades i separades per un espai.
        /// </summary>
        /// <param name="mots">Una llista de mots.</param>
        /// <param name="comparador">El comparador que farem servir per ordenar la llista.</param>
        /// <param name="mostraInfo">Si �s true, mostra informaci� morfol�gica i gramatical.</param>
        /// <returns>Si mostraInfo una cadena amb la descripci� morfol�gica i gramatical seguida de les
        /// formes corresponents. Si no mostraInfo, la cadena amb la llista aplanada.</returns>
        static public string LlistaPlana(List<Mot> mots, Comparison<string> comparador, bool mostraInfo)
        {
            List<Mot> ordenats = new List<Mot>(mots);
            ordenats.Sort(delegate(Mot mot1, Mot mot2)
            {
                int cmp = mostraInfo ? MorfoGram.Cmp(mot1.Info, mot2.Info) : 0;
                if (cmp == 0)
                    cmp = comparador(mot1.Forma, mot2.Forma);
                return cmp;
            });
            Mot anterior;
            anterior = null;
            ordenats.RemoveAll(delegate(Mot actual) {
                bool lleva = (anterior == null) ? false : 
                    ((actual.Forma == anterior.Forma) && (!mostraInfo || MorfoGram.Cmp(actual.Info, anterior.Info) == 0));
                anterior = actual;
                return lleva;
            });
            if (mostraInfo)
            {
                StringBuilder sb = new StringBuilder();
                anterior = null;
                foreach (Mot mot in ordenats)
                {
                    if ((anterior == null) || MorfoGram.Cmp(anterior.Info, mot.Info) != 0)
                        sb.AppendFormat("{0}{1}={2}", (anterior == null) ? "" : " ", mot.Info, mot.Forma);
                    else
                        sb.AppendFormat("|{0}", mot.Forma);
                    anterior = mot;
                }
                return sb.ToString();
            }
            else
            {
                List<String> formes = ordenats.ConvertAll<String>(delegate(Mot mot)
                {
                    return mot.Forma;
                });
                return String.Join(" ", formes.ToArray());
            }
        }

        public string Forma { get { return forma; } }

        /// <summary>
        /// Informaci� morfol�gica sobre el mot.
        /// </summary>
        public MorfoGram Info { 
            get { 
                if (info != null) 
                    return info;
                if (casRegla != null)
                    info = new MorfoGram(casRegla.Info, extra);
                else if (extra != null)
                    info = extra;
                else
                    info = noInfo;
                return info;
            }
        }
        
        public override string ToString()
        {
            return forma;
        }

        private string forma;
        private CasRegla casRegla;
        private MorfoGram info;
        private MorfoGram extra;
        private static MorfoGram noInfo = new MorfoGram();
    }

}
