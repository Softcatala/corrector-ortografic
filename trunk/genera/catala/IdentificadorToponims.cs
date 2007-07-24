using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using xspell;

namespace catala
{
    /// <summary>
    /// Un identificador de topònims.
    /// Exemples de línies: "Atzeneta d'Albaida", "l'Alqueria de la Comtessa".
    /// S'afegeixen a la llista els trossos que estan en majúscules.
    /// Als trossos inicials que comencen per vocal se'ls posa el flag que permet "d'".
    /// Als trossos que contenen articles apostrofats ("l'" o "s'") se'ls posa el flag
    /// que permet "l'" o "s'".
    /// </summary>
    public class IdentificadorToponims : IdentificadorDIEC
    {
        /// <summary>
        /// Crea amb unes regles.
        /// </summary>
        /// <param name="regles">Les regles en vigor.</param>
        public IdentificadorToponims(string desc, Regles regles)
            : base(desc, regles, null)
        {
        }

        public override Entrada IdentificaEntrada(string linia)
        {
            Dictionary<string, string> dades = new Dictionary<string,string>();
            dades["ent"] = linia;
            return new Entrada(this, dades);
       }

        public override Paradigma IdentificaParadigma(Dictionary<string, string> dades, Dictionary<string, string> excepcions)
        {
            // PER_FER: Decidir si admetre l'article a tots els topònims amb vocal inicial (ja que es diu "l'Europa d'entreguerres")
            // PER_FER: Si es permeten articles, s'haurien de detectar els femenins (com "Itàlia")
            PC_multi pars = new PC_multi();
            string[] trossos = dades["ent"].Split(' ');
            for (int i = 0; i < trossos.Length; i++)
            {
                string tros = trossos[i];
                if (Cat.EsMin(tros))
                    continue;
                if (tros.EndsWith(","))
                    tros = tros.Substring(0, tros.Length - 1);
                bool admetD = false, admetL = false;
                Match match;
                if ((match = reArticleApostrofat.Match(tros)).Success)
                {
                    admetL = true;
                    tros = match.Groups[1].Value;
                }
                else if ((match = reDeApostrofada.Match(tros)).Success)
                {
                    admetD = true;
                    tros = match.Groups[1].Value;
                }
                else if (Paraula.TeVocalInicial(tros) && i == 0)
                {
                    admetD = true;
                    admetL = true;  // volem "l'Europa d'entreguerres"
                }
                pars.Add(new PC_toponim(tros, admetD, admetL));
            }
            return pars;
        }

        private static Regex reArticleApostrofat = new Regex(@"^[ls]'(.+)");
        private static Regex reDeApostrofada = new Regex(@"^d'(.+)");
    }

}
