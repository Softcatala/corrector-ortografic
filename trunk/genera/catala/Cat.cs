using System;
using System.Collections.Generic;
using System.Text;

namespace catala
{
    /// <summary>
    /// Mètodes estàtics per a diverses operacions relacionades amb el català:
    /// - ordenació lexicogràfica
    /// - ...
    /// </summary>
    public class Cat
    {
        /// <summary>
        /// Compara dues cadenes lexicogràficament.
        /// La comparació es fa per fases. En cada fase, es torna si es troben diferències. Si al final de
        /// les fases no s'han trobat diferències, es decideix que les cadenes són iguals.
        /// Aplicam aquestes fases:
        /// 1. Miram els caràcters alfabètics de s1 i s2, sense accents i en minúscula. 
        /// 2. Miram els dígits. Consideram les seqüències, de manera que X10 > X9.
        /// 3. Miram els accents. Es posen primer les formes "netes".
        /// 4. Miram els espais, els guionets i els apòstrofs. Un espai equival a molts d'espais.
        /// 5. Miram les majúscules i les minúscules. Es posen primer les minúscules.
        /// 
        /// Consideram equivalents als accents la cometa de la ç i el punt volat.
        /// Consideram el guionet i l'apòstrof com equivalents a l'espai.
        /// Els caràcters alfabètics inexistents en català es consideren com accentuats (ñ->n, ÿ->y, etc.).
        /// Els altres caràcters s'ignoren.
        /// </summary>
        /// <param name="s1">La primera cadena a comparar.</param>
        /// <param name="s2">La segona cadena a comparar.</param>
        /// <returns>
        /// Torna < 0 si s1 < s2.
        /// Torna > 0 si s1 > s2.
        /// Torna 0 si s1 == s2.
        /// </returns>
        static public int Cmp(string s1, string s2)
        {
            int i1, i2, v1, v2;
            bool fi1, fi2;
            foreach (FaseCmp fase in FasesCmp)
            {
                i1 = i2 = 0;
                fi1 = fi2 = false;
                while (true)
                {
                    v1 = fase(s1, ref i1, ref fi1);
                    v2 = fase(s2, ref i2, ref fi2);
                    if (fi1 && fi2)
                        break;
                    if (v1 != v2)
                        return v1 - v2;
                }
            }
            return 0;
        }

        delegate int FaseCmp(string str, ref int inici, ref bool esFinal);
        static FaseCmp[] FasesCmp = { segFase1, segFase2, segFase3 };

        /// <summary>
        /// Passa una cadena a minúscules.
        /// Passam a minúscules qualsevol lletra que tengui versió minúscula, sigui o no catalana.
        /// </summary>
        /// <param name="que">La cadena que volem convertir.</param>
        /// <returns>La cadena en lletra minúscula.</returns>
        static public string Min(string que)
        {
            StringBuilder sb = new StringBuilder(que);
            for (int i = 0; i < sb.Length; i++)
            {
                int codi = (int)sb[i];
                if (codi >= 0 && codi <= 255)
                    sb[i] = min[codi];
            }
            return sb.ToString();
        }

        /// <summary>
        /// Diu si una cadena està en minúscules.
        /// </summary>
        /// <param name="que">La cadena que volem comprovar.</param>
        /// <returns>true si la cadena està en minúscules.</returns>
        static public bool EsMin(string que)
        {
            for (int i = 0; i < que.Length; i++)
            {
                int codi = (int)que[i];
                if (codi >= 0 && codi <= 255 && codi != min[codi])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Lleva els accents d'una cadena.
        /// Consideram accents les tildes damunt les vocals [a, e, i, o, u, y].
        /// No tocam les tildes dels altres caràcters (ç, ñ, etc.).
        /// </summary>
        /// <param name="que">La cadena que volem convertir.</param>
        /// <returns>La cadena sense accents.</returns>
        static public string NoAcc(string que)
        {
            StringBuilder sb = new StringBuilder(que);
            for (int i = 0; i < sb.Length; i++)
            {
                int codi = (int)sb[i];
                if (codi >= 0 && codi <= 255)
                    sb[i] = noAcc[codi];
            }
            return sb.ToString();
        }

        /// <summary>
        /// Diu si un caràcter té el flag donat.
        /// (Si el flag té més d'un bit, vol dir tenir un dels bits)
        /// </summary>
        static private bool TeFlag(char ch, int flag)
        {
            uint ich = ch;
            if (ich > 256)
                return false;
            return (infoCar[ich] & flag) != 0;
        }

        /// <summary>
        /// Torna el següent caràcter alfabètic, sense accents.
        /// </summary>
        static private int segFase1(string str, ref int pos, ref bool fi)
        {
            if (fi) return 0;
            while (true)
            {
                if (pos >= str.Length)
                {
                    fi = true;
                    return 0;
                }
                char c = str[pos];
                ++pos;
                if (TeFlag(c, ES_ALFA))
                    return infoCar[(uint)c] & MASCARA_ALFA;
            }
        }

        /// <summary>
        /// Torna el següent grup de dígits, convertits a enter.
        /// </summary>
        static private int segFase2(string str, ref int pos, ref bool fi)
        {
            if (fi) return 0;
            bool llegintNombre = false;
            string nombre = "";
            while (true)
            {
                if (pos >= str.Length)
                {
                    if (llegintNombre)
                        return int.Parse(nombre);
                    else
                    {
                        fi = true;
                        return 0;
                    }
                }
                char c = str[pos];
                ++pos;
                if (TeFlag(c, ES_DIGIT))
                {
                    if (llegintNombre)
                        nombre = nombre + c;
                    else
                    {
                        nombre = c.ToString();
                        llegintNombre = true;
                    }
                }
                else if (llegintNombre)
                    return int.Parse(nombre);
            }
        }

        /// <summary>
        /// Torna un indicador dels accents del següent caràcter alfabètic.
        /// Si no hi ha accents, torna 0.
        /// La darrera 'l' del grup "l·l" es considera accentuada.
        /// </summary>
        static private int segFase3(string str, ref int pos, ref bool fi)
        {
            if (fi) return 0;
            while (true)
            {
                if (pos >= str.Length)
                {
                    fi = true;
                    return 0;
                }
                char c = str[pos];
                if (TeFlag(c, ES_PUNT_VOLAT) && (pos > 0) && TeFlag(str[pos - 1], ES_ELA) && 
                    (pos < str.Length) && TeFlag(str[pos + 1], ES_ELA))
                {
                    pos += 2;   // al caràcter que segueix la segona 'l'
                    return 1;
                }
                ++pos;
                if (TeFlag(c, ES_ALFA))
                    return infoCar[(uint)c] & MASCARA_ACCENT;
            }
        }

        static int ES_ALFA = 0x100;
        static int MASCARA_ALFA = 0xFF;
        static int ES_ACCENT = 0x200;
        static int MASCARA_ACCENT = 0x1C00;
        static int ES_ELA = 0x2000;
        static int ES_PUNT_VOLAT = 0x4000;
        static int ES_DIGIT = 0x8000;

        /// <summary>
        /// Crea una taula amb informació sobre els caràcters catalans
        /// Cada caràcter té associada aquesta informació:
        /// - El caràcter en minúscules i sense accents.
        /// - Si el caràcter és alfabètic (ES_ALFA)
        /// </summary>
        private static int[] CreaInfoCar()
        {
            // PER_FER: com tractar Œ, etc.
            // "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzŠŒŽšœžŸÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ";
            //String senseAccen = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzsœzsœzyaaaaaaæceeeeiiiidnoooooouuuuyßaaaaaaæceeeeiiiinoooooouuuuyy";
            int[] info = new int[256];
            for (int i = 0; i < 256; i++)
                info[i] = i;
            string alfabetics = "AaÀÁÂÃÄÅàáâãäå Ææ Bb CcÇç DdÐ EeÈÉÊËèéêë Ff Gg Hh IiÍÏÌÎíïìî Jj Kk Ll Mm NnÑñ OoÒÓÔÕÖØòóôõöø Œœ Pp Qq Rr SsŠš ß Tt UuÚÜÙÛúüùû Vv Ww Xx YyÝŸýÿ ZzŽž";
            string accentuats = "00123456123456 00 00 0011 001 0012341234 00 00 00 0012341234 00 00 00 00 0011 00123456123456 00 00 00 00 0011 0 00 0012341234 00 00 00 001212 0011";
            int valor = 1;
            for (int i = 0; i < alfabetics.Length; i++)
            {
                char ch = alfabetics[i];
                if (ch == ' ')
                    valor += 1;
                else
                {
                    DinsInfoCar(ref info, ch, valor | ES_ALFA, MASCARA_ALFA);
                    char accent = accentuats[i];
                    DinsInfoCar(ref info, ch, (int.Parse(accent.ToString()) << 10) | ES_ACCENT, 0);
                }
            }
            for (char ch = '0'; ch <= '9'; ch++)
                DinsInfoCar(ref info, ch, ES_DIGIT, 0);
            DinsInfoCar(ref info, '·', ES_PUNT_VOLAT, 0);
            DinsInfoCar(ref info, 'L', ES_ELA, 0);
            DinsInfoCar(ref info, 'l', ES_ELA, 0);
            return info;
        }

        private static void DinsInfoCar(ref int[] info, char ch, int posa, int lleva)
        {
            uint ich = ch;
            if (ich > 256)
                return;
            info[ich] &= ~lleva;
            info[ich] |= posa;
        }

        private static char[] CreaMin()
        {
            char[] min = new char[256];
            for (int i = 0; i < 256; i++)
                min[i] = (char) i;
            string e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzŠŒŽšœžŸÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ";
            string s = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzšœžšœžÿàáâãäåæçèéêëìíîïðñòóôõöøùúûüýßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ";
            for (int i = 0; i < e.Length; i++)
            {
                int codi = (int)e[i];
                if (codi < 0 || codi > 255)
                    continue;
                min[codi] = s[i];
            }
            return min;
        }

        private static char[] CreaNoAcc()
        {
            char[] noAcc = new char[256];
            for (int i = 0; i < 256; i++)
                noAcc[i] = (char)i;
            string e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzŠŒŽšœžŸÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖØÙÚÛÜÝßàáâãäåæçèéêëìíîïñòóôõöøùúûüýÿ";
            string s = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzSŒZsœzYAAAAAAÆÇEEEEIIIIÐÑOOOOOØUUUUYßaaaaaaæçeeeeiiiiñoooooøuuuuyy";
            for (int i = 0; i < e.Length; i++)
            {
                int codi = (int)e[i];
                if (codi < 0 || codi > 255)
                    continue;
                noAcc[codi] = s[i];
            }
            return noAcc;
        }

        private static char[] min = CreaMin();
        private static char[] noAcc = CreaNoAcc();
        private static int[] infoCar = CreaInfoCar();

    }
}
