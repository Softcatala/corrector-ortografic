using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Fitxer del qual s'han extret dades.
    /// El feim servir per tra�ar l'origen dels resultats.
    /// Cada fitxer t� un identificador num�ric, per estalviar espai en els fitxers de sortida.
    /// </summary>
    public class FitxerFont
    {

        /// <summary>
        /// Crea un FitxerFont amb el nom de fitxer donat o en torna un de preexistent.
        /// </summary>
        /// <param name="nomFitxer">El nom del fitxer font.
        /// Si el nom cont� "_part_", suposam que �s una part d'un altre fitxer, i desam
        /// el nom original.
        /// </param>
        /// <returns>Un FitxerFont que fa refer�ncia al fitxer donat.</returns>
        public static FitxerFont Crea(string nomFitxer)
        {
            if (nomFitxer.Contains("_part_"))
                nomFitxer = nomFitxer.Replace("_part_", "");
            if (dic.ContainsKey(nomFitxer))
                return dic[nomFitxer];
            else
                return new FitxerFont(nomFitxer);
        }

        private FitxerFont(string nomFitxer)
        {
            this.nomFitxer = nomFitxer;
            id = sid;
            ++sid;
            llista.Add(this);
            dic.Add(nomFitxer, this);
        }

        /// <summary>
        /// Torna el nom de fitxer d'aquest fitxer font.
        /// </summary>
        public string NomFitxer { get { return nomFitxer; } }

        /// <summary>
        /// L'identificador d'aquest fitxer font.
        /// </summary>
        public int Id { get { return id; } }

        /// <summary>
        /// Torna la llista de fitxers font.
        /// </summary>
        static public List<FitxerFont> Llista { get { return llista; } }

        static FitxerFont()
        {
            sid = 0;
            llista = new List<FitxerFont>();
            dic = new Dictionary<string, FitxerFont>();
        }

        private int id;

        private static int sid;
        private string nomFitxer;
        private static List<FitxerFont> llista;
        private static Dictionary<string, FitxerFont> dic;
    }
}
