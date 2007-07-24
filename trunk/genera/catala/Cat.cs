using System;
using System.Collections.Generic;
using System.Text;

namespace catala
{
    /// <summary>
    /// M�todes est�tics per a diverses operacions relacionades amb el catal�:
    /// - ordenaci� lexicogr�fica
    /// - ...
    /// </summary>
    public class Cat
    {
        /// <summary>
        /// Compara dues cadenes lexicogr�ficament.
        /// La comparaci� es fa per fases. En cada fase, es torna si es troben difer�ncies. Si al final de
        /// les fases no s'han trobat difer�ncies, es decideix que les cadenes s�n iguals.
        /// Aplicam aquestes fases:
        /// 1. Miram els car�cters alfab�tics de s1 i s2, sense accents i en min�scula. 
        /// 2. Miram els d�gits. Consideram les seq��ncies, de manera que X10 > X9.
        /// 3. Miram els accents. Es posen primer les formes "netes".
        /// 4. Miram els espais, els guionets i els ap�strofs. Un espai equival a molts d'espais.
        /// 5. Miram les maj�scules i les min�scules. Es posen primer les min�scules.
        /// 
        /// Consideram equivalents als accents la cometa de la � i el punt volat.
        /// Consideram el guionet i l'ap�strof com equivalents a l'espai.
        /// Els car�cters alfab�tics inexistents en catal� es consideren com accentuats (�->n, �->y, etc.).
        /// Els altres car�cters s'ignoren.
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
        /// Passa una cadena a min�scules.
        /// Passam a min�scules qualsevol lletra que tengui versi� min�scula, sigui o no catalana.
        /// </summary>
        /// <param name="que">La cadena que volem convertir.</param>
        /// <returns>La cadena en lletra min�scula.</returns>
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
        /// Diu si una cadena est� en min�scules.
        /// </summary>
        /// <param name="que">La cadena que volem comprovar.</param>
        /// <returns>true si la cadena est� en min�scules.</returns>
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
        /// No tocam les tildes dels altres car�cters (�, �, etc.).
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
        /// Diu si un car�cter t� el flag donat.
        /// (Si el flag t� m�s d'un bit, vol dir tenir un dels bits)
        /// </summary>
        static private bool TeFlag(char ch, int flag)
        {
            uint ich = ch;
            if (ich > 256)
                return false;
            return (infoCar[ich] & flag) != 0;
        }

        /// <summary>
        /// Torna el seg�ent car�cter alfab�tic, sense accents.
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
        /// Torna el seg�ent grup de d�gits, convertits a enter.
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
        /// Torna un indicador dels accents del seg�ent car�cter alfab�tic.
        /// Si no hi ha accents, torna 0.
        /// La darrera 'l' del grup "l�l" es considera accentuada.
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
                    pos += 2;   // al car�cter que segueix la segona 'l'
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
        /// Crea una taula amb informaci� sobre els car�cters catalans
        /// Cada car�cter t� associada aquesta informaci�:
        /// - El car�cter en min�scules i sense accents.
        /// - Si el car�cter �s alfab�tic (ES_ALFA)
        /// </summary>
        private static int[] CreaInfoCar()
        {
            // PER_FER: com tractar �, etc.
            // "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz������������������������������������������������������������������";
            //String senseAccen = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyzs�zs�zyaaaaaa�ceeeeiiiidnoooooouuuuy�aaaaaa�ceeeeiiiinoooooouuuuyy";
            int[] info = new int[256];
            for (int i = 0; i < 256; i++)
                info[i] = i;
            string alfabetics = "Aa������������ �� Bb Cc�� Dd� Ee�������� Ff Gg Hh Ii�������� Jj Kk Ll Mm Nn�� Oo������������ �� Pp Qq Rr Ss�� � Tt Uu�������� Vv Ww Xx Yyݟ�� Zz��";
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
            DinsInfoCar(ref info, '�', ES_PUNT_VOLAT, 0);
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
            string e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz������������������������������������������������������������������";
            string s = "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz������������������������������������������������������������������";
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
            string e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz������������������������������������������������������������������";
            string s = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyzS�Zs�zYAAAAAA��EEEEIIII��OOOOO�UUUUY�aaaaaa��eeeeiiii�ooooo�uuuuyy";
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
