using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace catala
{
    /// <summary>
    /// Dóna informació relacionada amb la forma d'una paraula:
    /// - nombre de síl·labes
    /// - síl·laba tònica
    /// - vocal inicial
    /// - possibilitat d'apostrofar-la
    /// - ...
    /// </summary>
    public class Paraula
    {
        /// <summary>
        /// Crea a partir d'una paraula.
        /// </summary>
        /// <param name="paraula">La paraula que volem estudiar.</param>
        public Paraula(string forma)
        {
            this.forma = forma;
            sillabes = null;
            minuscula = null;
            tonica = -1;
        }

        /// <summary>
        /// Diu si la paraula comença per vocal.
        /// "començar per vocal" vol dir "pronunciar-se amb una vocal inicial".
        /// Comença per vocal "ara" o "ha". No hi comença "casa" o "iode".
        /// </summary>
        /// <param name="paraula">La paraula que volem examinar.</param>
        /// <returns>true si la paraula comença per vocal.</returns>
        public static bool TeVocalInicial(string forma)
        {
            return vocIni.IsMatch(forma);
        }

        /// <summary>
        /// Diu si la paraula acaba per vocal.
        /// Acaba per vocal "ara" o "cossi". No hi comença "moix" o "palau".
        /// </summary>
        /// <param name="paraula">La paraula que volem examinar.</param>
        /// <returns>true si la paraula acaba per vocal.</returns>
        public bool VocalFinal
        {
            get
            {
                if (prototip == null)
                    CreaPrototip();
                return prototip.EndsWith("A");
            }
        }

        /// <summary>
        /// Diu si la paraula comença per vocal.
        /// "començar per vocal" vol dir "pronunciar-se amb una vocal inicial".
        /// Comença per vocal "ara" o "ha". No hi comença "casa" o "iode".
        /// </summary>
        /// <returns>true si la paraula comença per vocal.</returns>
        public bool VocalInicial
        {
            get
            {
                if (prototip == null)
                    CreaPrototip();
                return (prototip.StartsWith("A") || (prototip.StartsWith("PA") && Minuscula.StartsWith("h")));
            }
        }

        public string Forma { get { return forma; } }

        /// <summary>
        /// Prototip de la paraula.
        /// Les vocals es converteixen en 'A', i les consonants en 'P' o 'L'
        /// </summary>
        public string Prototip
        {
            get
            {
                if (prototip == null)
                    CreaPrototip();
                return prototip;
            }
        }

        /// <summary>
        /// Array amb les síl·labes de la paraula.
        /// Considera el punt volat com si fos una consonant, fent síl·laba amb la 'l' anterior.
        /// </summary>
        public string[] Sillabes
        {
            get
            {
                if (sillabes == null)
                    CreaSillabes();
                return sillabes;
            }
        }

        /// <summary>
        /// Torna la part inicial de la paraula, inclosa la lletra que precedeix la vocal tònica.
        /// Aquesta funció serveix per a la notació del DIEC "abacà -às"
        /// </summary>
        public string PreTonica
        {
            get
            {
                int ini = 0;
                for (int i = 0; i < Sillabes.Length; i++)
                    if (i == Tonica)
                    {
                        int posA = Prototip.IndexOf('A', ini);
                        if (posA >= 0)
                            ini = posA;
                        break;
                    }
                    else
                        ini += Sillabes[i].Length;
                return Forma.Substring(0, ini);
            }
        }

            /// <summary>
            /// Diu quina és la síl·laba tònica
            /// </summary>
            public int Tonica
        {
            get
            {
                if (tonica < 0)
                {
                    Match match = accentuades.Match(forma);
                    if (match.Success) // Hem trobat una vocal accentuada: la seva síl·laba és la tònica
                    {
                        int idx = match.Groups[0].Index;
                        int iniSeg = 0;
                        string[] ss = Sillabes;
                        for (int i = 0; i < ss.Length; i++)
                        {
                            iniSeg += ss[i].Length;
                            if (idx < iniSeg)
                            {
                                tonica = i;
                                break;
                            }
                        }
                    }
                    else // No hi ha vocal accentuada, miram si la paraula és plana o aguda
                    {
                        if (Sillabes.Length == 1)
                            tonica = 0;
                        else
                        {
                            if ((Prototip.EndsWith("A") && esPlana1.IsMatch(forma)) || (Prototip.EndsWith("AP") && esPlana2.IsMatch(forma)))
                                tonica = Sillabes.Length - 2;
                            else
                                tonica = Sillabes.Length - 1;
                        }
                    }
                }
                return tonica;
            }
        }

        /// <summary>
        /// Versió en minúscules de la paraula
        /// </summary>
        public string Minuscula
        {
            get
            {
                if (minuscula == null)
                    minuscula = Cat.Min(forma);
                return minuscula;
            }
        }

        /// <summary>
        /// Diu si la paraula es pot apostrofar.
        /// Suposa que la paraula no té restriccions de gènere (com ara "unió").
        /// </summary>
        /// <returns>true si la paraula pot dur apòstrof.</returns>
        public bool PotApostrofar()
        {
            return PotApostrofar(false);
        }

        /// <summary>
        /// Diu si la paraula es pot apostrofar.
        /// </summary>
        /// <param name="femeni">
        /// Si és true, torna false si la paraula comença per 'i' o 'u' atones (o si no comença per vocal).
        /// Si és false, torna false  si no comença per vocal.
        /// </param>
        /// <returns>true si la paraula es pot apostrofar.</returns>
        public bool PotApostrofar(bool femeni)
        {
            if (!TeVocalInicial(forma))
                return false;
            // (la paraula comença per vocal)
            if (femeni && iuInicial.IsMatch(forma) && (Prototip.StartsWith("A") || Prototip.StartsWith("PA")) && Tonica != 0)
                return false;
            else
                return true;
        }

        /// <summary>
        /// Torna true si la paraula és aguda.
        /// </summary>
        public bool Aguda
        {
            get
            {
                if (sillabes == null)
                    CreaSillabes();
                int ns = sillabes.Length;
                return (ns >= 1) && (Tonica == ns - 1);
            }
        }

        /// <summary>
        /// Diu si la paraula acaba en 's', 'x' o 'ç'
        /// </summary>
        public bool SxcFinal
        {
            get
            {
                return sxcFinal.IsMatch(Minuscula);
            }
        }

        public override string ToString()
        {
            return forma;
        }

        private void CreaPrototip()
        {
            string p = Cat.Min(forma);
            if (especials.IsMatch(p))
                prototip = CreaPrototipEspecial(p);
            else
                prototip = CreaPrototip(p);
        }
        private string CreaPrototip(string p)
        {
            p = vocalSegura.Replace(p, "A");    // [aeoàáèéíïòóú]   => A
            p = semiVocal1.Replace(p, "APA");   // A[iu]A           => APA
            p = semiVocal2.Replace(p, "PLA");   // [qg][uü][Aiu]    => PLA
            p = vocalU1.Replace(p, "A");        // ^u               => A
            p = semiVocal3.Replace(p, "PA");    // ^iA              => PA
            p = semiVocal4.Replace(p, "PLA");   // ^hiA             => PLA
            p = semiVocal5.Replace(p, "AP");    // [Aiu][iu]        => AP
            p = p.Replace('i', 'A');            // i                => A
            p = vocalU2.Replace(p, "A");        // (?<!A)u          => A
            p = oclusivaL.Replace(p, "PL");     // [bcfgklp]l       => PL
            p = oclusivaR.Replace(p, "PL");     // [bcdfgkptv]r     => PL
            p = p.Replace("ny", "PL");          // ny               => PL
            p = altresCons.Replace(p, "P");     // [^PAL]           => P
            return p;
        }

        private string CreaPrototipEspecial(string p)
        {
            if (p == "ió")
                return "AA";
            else if (p.StartsWith("ion"))
                return "A" + CreaPrototip(p).Substring(1);
            return CreaPrototip(p);
        }

        private void CreaSillabes()
        {
            List<string> ss = new List<string>();
            Match match = divSillabes.Match(Prototip);
            if (!match.Success)
                ss.Add(forma);
            else
            {
                int ini = 0;
                while (match.Success)
                {
                    //string s = match.Groups[1].Value;
                    //ss.Add(paraula.Substring(ini, s.Length));
                    //ini += s.Length;
                    ss.Add(forma.Substring(ini, match.Length));
                    ini += match.Length;
                    match = divSillabes.Match(Prototip, ini);
                }
            }
            sillabes = new string[ss.Count];
            ss.CopyTo(sillabes);
        }

        private string forma;
        private string minuscula;
        private string prototip;
        private string[] sillabes;
        private int tonica;
        private static Dictionary<string, string> PrototipsPrecalculats;

        #region Expressions regulars
        static private Regex vocalSegura = new Regex("[aeoàáèéíïòóú]");
        static private Regex semiVocal1 = new Regex("A[iu]A");
        static private Regex semiVocal2 = new Regex("[qg][uü][Aiu]");
        static private Regex vocalU1 = new Regex("^u");
        static private Regex vocalU2 = new Regex("(?<!A)u");
        static private Regex semiVocal3 = new Regex("^iA");
        static private Regex semiVocal4 = new Regex("^hiA");
        static private Regex semiVocal5 = new Regex("[Aiu][iu]");
        //static private Regex vocalI = new Regex("i");
        static private Regex oclusivaL = new Regex("[bcfgklp]l");
        static private Regex oclusivaR = new Regex("[bcdfgkptv]r");
        //static private Regex digrafNY = new Regex("ny");
        static private Regex altresCons = new Regex("[^PAL]");
        static private Regex divSillabes = new Regex(@"([PL]*A[PL]*?)($|(?=P?L?A))");
        static private Regex vocIni = new Regex(@"^h?([aàeèéíoòóuú]|i(?![aàeèéíoòóuú]))", RegexOptions.IgnoreCase);
        static private Regex accentuades = new Regex(@"[àáèéìíòóùú]", RegexOptions.IgnoreCase);
        static private Regex esPlana1 = new Regex(@"[aeiou]$", RegexOptions.IgnoreCase);
        static private Regex esPlana2 = new Regex(@"([aeiou]s|[ei]n)$", RegexOptions.IgnoreCase);
        static private Regex iuInicial = new Regex(@"^h?[iu]", RegexOptions.IgnoreCase);
        static private Regex sxcFinal = new Regex(@"[sxç]$", RegexOptions.IgnoreCase);
        // Tractament especial per a: "ió" i derivats
        static private Regex especials = new Regex(@"(^i[óo])");
        #endregion
    }
}

/*
sub sillab 
{
	my ($mot) = @_;
	$mot = minuscules($mot);
	my $esq = $mot;
	# print "Entrada: $esq\n";
	$esq =~ tr/aeoàáèéíïòóú/A/ ; 	# && print "Vocals segures: $esq\n";
	$esq =~ s/A[iu]A/APA/g ;		# && print "Semivocals I: $esq\n";
	$esq =~ s/[qg][uü][Aiu]/PLA/g ; 	# && print "Semivocals II: $esq\n";
	$esq =~ s/^u/A/g ; 				# && print "Vocal u I: $esq\n";
	$esq =~ s/([^A])u/"$1A"/eg;		# && print "Vocal u II: $esq\n";
	$esq =~ s/^iA/PA/g ; 			# && print "Semivocals III: $esq\n";
	$esq =~ s/A[iu]/AP/g ; 			# && print "Semivocals IV: $esq\n";
	$esq =~ s/i/A/g ; 				# && print "Vocal i: $esq\n";
	$esq =~ s/[bcfgklp]l/PL/g ; 	# && print "oclusiva+l i ll: $esq\n";
	$esq =~ s/[bcdfgkptv]r/PL/g ;	# && print "oclusiva+r: $esq\n";
	$esq =~ s/ny/PL/g ; 			# && print "Dígraf ny: $esq\n";
	$esq =~ tr/PAL/P/c ; 			# && print "Altres consonants: $esq\n";
	my @silesq = ();
	while ($esq ne "")
	{
		if ($esq =~ s/(P?L?AP*)$//) { unshift(@silesq, $1); }
		else { $silesq[0] = $esq . $silesq[0]; $esq = ''; }
	}
	my (@sil, $offset);
	foreach my $sil(@silesq) 
	{
		push(@sil, substr($mot, $offset, length($sil)));
		$offset += length($sil);
	}
	for(my $i=0; $i < @sil; $i++) { $sil[$i] =~ s/·$//; }
	return (scalar(@sil),@sil,@silesq);
}
 */