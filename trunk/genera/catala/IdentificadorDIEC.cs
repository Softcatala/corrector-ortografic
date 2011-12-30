using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using xspell;

namespace catala
{
    /// <summary>
    /// Un identificador d'entrades i paradigmes a partir de línies tipus DIEC.
    /// Exemple: "ent=àbac^cat1=m."
    /// <remarks>Se suposa que de cada línia se n'extreu una entrada. Si això no ha de ser
    /// així, s'haurà de canviar el mètode Identifica().</remarks>
    /// </summary>
    public class IdentificadorDIEC : IdentificadorCat
    {
        /// <summary>
        /// Crea amb unes regles.
        /// </summary>
        /// <param name="regles">Les regles aplicables a una llista tipus DIEC.</param>
        public IdentificadorDIEC(string desc, Regles regles, string fitxerExcepcions)
            : base(desc, regles)
        {
            if (fitxerExcepcions != null)
            {
                int numLinia = 0;
                FitxerFont fitxerFont = FitxerFont.Crea(fitxerExcepcions);
                StreamReader fitxer = new StreamReader(fitxerExcepcions, Encoding.Default);
                while (!fitxer.EndOfStream)
                {
                    string linia = fitxer.ReadLine().Trim();
                    ++numLinia;
                    Match match = liniaIrr.Match(linia);
                    if (!match.Success)
                        continue;
                    string ent = match.Groups[1].Value;
                    string cont = match.Groups[2].Value;
                    LiniaMarques lm = LlegeixLiniaExc(cont);
                    NovaExcepcio(ent, lm, fitxerFont, numLinia);
                }
                fitxer.Close();
            }
        }

        public override Entrada IdentificaEntrada(string linia)
        {
            Dictionary<string, string> dades = new Dictionary<string,string>();
            Match match;
            foreach (string tros in trossosLinia.Split(linia))
            {
                match = clauIgualValor.Match(tros);
                if (!match.Success)
                    throw new Exception(String.Format("S'esperava xxx=yyy (rebut: \"{0}\")", tros));
                dades[match.Groups[1].Value] = match.Groups[2].Value;
            }
            if (!dades.ContainsKey("ent"))
                throw new Exception("No ent!");
            string ent = dades["ent"];
            if (ignoraEntrada.IsMatch(ent))
                return null;
            match = netejaFinal.Match(ent);
            string arrel = match.Success ? match.Groups[1].Value : ent;
            match = netejaMig.Match(ent);
            if (match.Success)
                arrel = match.Groups[1].Value + match.Groups[2].Value;
            dades["arrel"] = arrel;
            dades["línia"] = linia;
            Entrada entrada = new Entrada(this, dades);
            entrada.Excepcions = Excepcio(entrada.Ent);
            return entrada;
        }

        /// <summary>
        /// Interpreta una línia d'excepcions i crea una LiniaMarques.
        /// </summary>
        static public LiniaMarques LlegeixLiniaExc(string linia)
        {
            LiniaMarques lm = new LiniaMarques();
            Marques mm = new Marques(false, Marca.grup1);
            Marques excepte = new Marques(false);
            int iniTros = 0;
            int midaTros;
            foreach (Match match in condicions.Matches(linia))
            {
                midaTros = match.Index - iniTros;
                if (midaTros > 0)
                    lm.Nou(linia.Substring(iniTros, midaTros), mm, excepte.Buit ? null : excepte);
                if (match.Groups[2].Value == "<<")
                {
                    string grup = match.Groups[3].Value;
                    if (grup == "EXT")
                    {
                        lm.PosaMarca(Marca.grup1);
                        mm.Menys(Marca.grup1);
                        mm.Mes(Marca.grup2);
                    }
                    else
                    {
                        if (grup.StartsWith("!"))
                            excepte.Mes(Marca.Una(grup.Substring(1)));
                        else
                            mm.Mes(Marca.Una(grup));
                    }
                }
                else if (match.Groups[6].Value == ">>")
                {
                    string grup = match.Groups[5].Value;
                    if (grup == "EXT")
                    {
                        mm.Menys(Marca.grup2);
                        mm.Mes(Marca.grup1);
                        // PERFER: mirar si hi pot haver més de dos grups
                    }
                    else
                    {
                        if (grup.StartsWith("!"))
                            excepte.Menys(Marca.Una(grup.Substring(1)));
                        else
                            mm.Menys(Marca.Una(grup));
                    }
                }
                iniTros = match.Index + match.Length;
            }
            return lm;
        }

        override public int LlegeixEntrades(string nomFitxer, List<Entrada> entrades, int mod)
        {
            FitxerFont fitxerDades = FitxerFont.Crea(nomFitxer);
            StreamReader sr = new StreamReader(nomFitxer, Encoding.Default);
            int numEntrada = 0;
            int llegides = 0;
            int numLinia = 0;
            while (!sr.EndOfStream)
            {
                string linia = sr.ReadLine();
                ++numLinia;
                if (linia.StartsWith("//") || linia.Length == 0)
                    continue;
                if (numEntrada % mod == 0)
                {
                    Entrada ent = IdentificaEntrada(linia);
                    if (ent != null)
                    {
                        entrades.Add(ent);
                        ++llegides;
                        ent.FitxerDades = fitxerDades;
                        ent.LiniaFitxerDades = numLinia;
                    }
                }
                ++numEntrada;
            }
            sr.Close();
            return llegides;
        }

        static private Regex condicions = new Regex(@"((<<)(!?...))|((!?...)(>>))|$");
        static private Regex trossosLinia = new Regex(@"\^");
        static private Regex ignoraEntrada = new Regex(@"(^-)|(-[1-9]?$)");
        static private Regex netejaFinal = new Regex(@"^(.*?)(-se|'s)?[1-9]?$");
        static private Regex netejaMig = new Regex(@"^(.*)[1-9]( .*)$");
        static private Regex liniaIrr = new Regex(@"^(?!//)(.*?):\s+(.*)");
        static private Regex clauIgualValor = new Regex(@"(.*?)=(.*)");

    }
}
