using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using Tests;
using xspell;
using catala;

namespace Genera
{
    partial class Principal
    {
        private void test_01(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("ItemDic");
            ItemDic id1 = null, id2 = null;
            grup.NouTest("Crea amb arrel i flags (string)", delegate(RTest resultat)
            {
                id2 = new ItemDic("xyz", "C", "Z", "z");
                resultat.Esperat("xyz/CZz", id2.ToString());
            });
            grup.NouTest("Flags admissibles", delegate(RTest resultat)
            {
                try
                {
                    ItemDic id = new ItemDic("xyz", "ab");
                    resultat.Error("No s'ha impedit l'�s d'un flag il�legal");
                }
                catch (ItemDicException)
                {
                    resultat.Nota("S'ha impedit l'�s d'un flag il�legal");
                }
            });
            grup.NouTest("Crea amb arrel i flags (string)", delegate(RTest resultat)
            {
                id1 = new ItemDic("xyz", "C");
                resultat.Esperat("xyz/C", id1.ToString());
                id1.MesFlags("D");
                resultat.Esperat("xyz/CD", id1.ToString());
                id2 = new ItemDic("xyz", "E");
                id1.MesFlags(id2);
                resultat.Esperat("xyz/CDE", id1.ToString());
            });
        }

        private void test_02(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Fitxer de regles");
            Regles regles = null;
            grup.NouTest("Llegeix fitxer .txt", delegate(RTest resultat)
            {
                //regles = new Regles(DirEntrades("regles.txt");
                regles = new Regles(DirEntrades("regles.txt"));
            });
            grup.NouTest("Llegeix fitxer .aff", delegate(RTest resultat)
            {
                Regles regles2 = Regles.LlegeixAff(DirResultats("prova.aff"));
            });
            grup.NouTest("Marca de dialecte", delegate(RTest resultat)
            {
                Marca marca = regles.Marques["000"];
                resultat.Esperat("000: Sense condicions", marca.ToString());
            });
            grup.NouTest("Marques", delegate(RTest resultat)
            {
                Marques m1 = new Marques(false, "001", "002", "005", "013", "014", "101", "102", "103");
                Marques m1bis = new Marques(false, "001", "002", "005", "013", "014", "101", "102", "103");
                resultat.Esperat("001, 002, 005, 013, 014, 101, 102, 103", m1.ToString());
                resultat.Assert(m1.Conte("005"), "Cont� 005");
                resultat.Assert(!m1.Conte("000"), "No cont� 000");
                resultat.Assert(m1.Conte(Marca.Una("001").Mascara), "Cont� 001");
                resultat.Assert(!m1.Conte(Marca.Una("000").Mascara), "No cont� 000");
                Marques m2 = new Marques(false, "101", "102", "103");
                resultat.Assert(m1.Conte(m2), "m1 >= m2");
                resultat.Assert(!m2.Conte(m1), "!(m2 >= m1)");
                resultat.Assert(m1 == m1bis, "S�n iguals!");
                m1.Mes(Marca.Una("003"));
                resultat.Esperat("001, 002, 003, 005, 013, 014, 101, 102, 103", m1.ToString());
                m1.Menys(Marca.Una("013"));
                resultat.Esperat("001, 002, 003, 005, 014, 101, 102, 103", m1.ToString());
            });
            grup.NouTest("L�nia amb condicions I", delegate(RTest resultat)
            {
                // Suposam la cadena "abc <<001 def 001>> ghi"                
                LiniaMarques lm = new LiniaMarques();
                Marca.Crea("_1_", "Grup 1");
                Marca.Crea("_2_", "Grup 2");
                Marques m = new Marques(false, "_1_");
                lm.Nou("abc ", m);
                m.Mes(Marca.Una("001"));
                lm.Nou(" def ", m);
                m.Menys(Marca.Una("001"));
                lm.Nou(" ghi", m);
                resultat.Esperat("abc  def  ghi", lm.Valor(new Marques(false, "_1_", "001")));
                resultat.Esperat("abc  ghi", lm.Valor(new Marques(false, "_1_")));
                resultat.Esperat("", lm.Valor(new Marques(false)));
            });
            grup.NouTest("L�nia amb condicions II", delegate(RTest resultat)
            {
                regles = new Regles(DirEntrades("regles.txt"));
                LiniaMarques lm = IdentificadorDIEC.LlegeixLiniaExc("abc <<001 def 001>> ghi <<!002 jkl !002>>");
                resultat.Esperat("abc  def  ghi  jkl", lm.Valor(new Marques(false, "_1_", "001")).Trim());
                resultat.Esperat("abc  ghi  jkl", lm.Valor(new Marques(false, "_1_")).Trim());
                resultat.Esperat("abc  ghi", lm.Valor(new Marques(false, "_1_", "002")).Trim());
                resultat.Esperat("", lm.Valor(new Marques(false)).Trim());
            });
            grup.NouTest("Genera mots", delegate(RTest resultat)
            {
                Regla regla = regles.Llista["E"];
                List<Mot> mots = regla.Genera("aigua", null, regles, Marques.totes, true);
                resultat.Esperat("aig�es", Mot.LlistaPlana(mots, Cat.Cmp, false));
                for (int i = 0; i < mots.Count; i++)
                    resultat.Nota("{0}: {1}", i, mots[i]);
            });
            grup.NouTest("Regles aplicables", delegate(RTest resultat)
            {
                Regla regla = regles.Llista["A"];
                resultat.Assert(regla.EsAplicable("cantava"), "Es pot aplicar la regla A a 'cantava'");
                resultat.Assert(!regla.EsAplicable("cantar"), "No es pot aplicar la regla A a 'cantar'");
            });
        }

        private void test_03(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Eines per al catal�");
            grup.NouTest("Comparacions simples", delegate(RTest resultat)
            {
                string[] mots = "a al al�a alt cant canta cant� cantam cella cel�la celles cel�les".Split(" ".ToCharArray());
                for (int i = 0; i < mots.Length; i++)
                {
                    string m1 = mots[i];
                    resultat.Assert(Cat.Cmp(m1, m1) == 0, "{0} == {1}", m1, m1);
                    for (int j = i + 1; j < mots.Length; j++)
                    {
                        string m2 = mots[j];
                        int cmp1 = Cat.Cmp(m1, m2);
                        int cmp2 = Cat.Cmp(m2, m1);
                        resultat.Assert((cmp1 < 0) && (cmp1 + cmp2 == 0), "{0} < {1} ({2}, {3})", m1, m2, cmp1, cmp2);
                    }
                }
            });
            grup.NouTest("M�s comparacions", delegate(RTest resultat)
            {
                string[] mots = "a1 a2 a3 a10 a30".Split(" ".ToCharArray());
                for (int i = 0; i < mots.Length; i++)
                {
                    string m1 = mots[i];
                    resultat.Assert(Cat.Cmp(m1, m1) == 0, "{0} == {1}", m1, m1);
                    for (int j = i + 1; j < mots.Length; j++)
                    {
                        string m2 = mots[j];
                        int cmp1 = Cat.Cmp(m1, m2);
                        int cmp2 = Cat.Cmp(m2, m1);
                        resultat.Assert((cmp1 < 0) && (cmp1 + cmp2 == 0), "{0} < {1} ({2}, {3})", m1, m2, cmp1, cmp2);
                    }
                }
            });
            grup.NouTest("Conversi� a min�scules", delegate(RTest resultat)
            {
                string maj = "A�BC�DE��FGHI��JKLMNO��PQRSTU��VXYZa�bc�de��fghi��jklmno��pqrstu��vxyz";
                string min = "a�bc�de��fghi��jklmno��pqrstu��vxyza�bc�de��fghi��jklmno��pqrstu��vxyz";
                resultat.Esperat(min, Cat.Min(maj));
            });
            grup.NouTest("Divisi� sil�l�bica", delegate(RTest resultat)
            {
                string[] paraules = { "es/qu�/ieu", "hie/na", "ca/nya", "psi/c�/legs", "�us/tri/a", "cui/nar", "xxx", "ha", "any", "i/�", "i/ons", "i/o/nit/za/ci/�", "" };
                foreach (string s in paraules)
                {
                    string forma = s.Replace("/", "");
                    Paraula sillabes = new Paraula(forma);
                    string sil = String.Join("/", sillabes.Sillabes);
                    resultat.Nota("\"{0}\" => \"{1}\", {2}", sillabes.Forma, sillabes.Prototip, sil);
                    resultat.Esperat(s, sil);
                }
            });
            grup.NouTest("Vocal inicial", delegate(RTest resultat)
            {
                string[] si = { "ara", "�rab", "humitat", "indi", "Henedina", "ARA", "�RAB", "HUMITAT", "INDI" };
                foreach (string forma in si)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(Paraula.TeVocalInicial(forma) && paraula.VocalInicial, String.Format("\"{0}\" comen�a per vocal", forma));
                }
                string[] no = { "casa", "hiena", "iode", "CASA", "HIENA", "IODE" };
                foreach (string forma in no)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!Paraula.TeVocalInicial(forma) && !paraula.VocalInicial, String.Format("\"{0}\" no comen�a per vocal", forma));
                }
            });
            grup.NouTest("Vocal final", delegate(RTest resultat)
            {
                string[] si = { "ara", "indi", "pagui", "ARA", "INDI", "PAGUI" };
                foreach (string forma in si)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.VocalFinal, String.Format("\"{0}\" acaba per vocal", forma));
                }
                string[] no = { "cau", "moix", "seguiu", "adscriu", "virrei", "CAU", "MOIX", "SEGUIU" };
                foreach (string forma in no)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!paraula.VocalFinal, String.Format("\"{0}\" no acaba per vocal", forma));
                }
            });
            grup.NouTest("Aguda", delegate(RTest resultat)
            {
                string[] si = { "pa", "ratol�", "forat", "PA", "RATOL�", "FORAT" };
                foreach (string forma in si)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.Aguda, String.Format("\"{0}\" �s aguda", forma));
                }
                string[] no = { "panera", "rata", "p�mpol", "m�rlera", "PANERA", "RATA", "P�MPOL", "M�RLERA" };
                foreach (string forma in no)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!paraula.Aguda, String.Format("\"{0}\" no �s aguda", forma));
                }
            });
            grup.NouTest("S�l�laba t�nica", delegate(RTest resultat)
            {
                string[] paraules = { "agui�veu", "PA", "P�Mpol", "CAsa", "coLOR", "M�quina" };
                foreach (string forma in paraules)
                {
                    Paraula paraula = new Paraula(forma.ToLower());
                    int tonica = paraula.Tonica;
                    StringBuilder sb = new StringBuilder();
                    for (int i = 0; i < paraula.Sillabes.Length; i++)
                    {
                        string s = paraula.Sillabes[i];
                        if (i == tonica)
                            sb.Append(s.ToUpper());
                        else
                            sb.Append(s);
                    }
                    resultat.Nota("{0} => {1}", forma, sb);
                    resultat.Esperat(forma, sb.ToString());
                }
            });
            grup.NouTest("Abans de la vocal t�nica", delegate(RTest resultat)
            {
                string[] paraules = { "agui�VEU", "pA", "p�MPOL", "cASA", "colOR", "m�QUINA" };
                foreach (string forma in paraules)
                {
                    string minuscula = forma.ToLower();
                    Paraula paraula = new Paraula(minuscula);
                    string preTonica = paraula.PreTonica;
                    int mida = preTonica.Length;
                    string nova = minuscula.Substring(0, mida) + minuscula.Substring(mida).ToUpper();
                    resultat.Nota("{0} => {1}", forma, nova);
                    resultat.Esperat(forma, nova);
                }
            });
            grup.NouTest("Es poden apostrofar", delegate(RTest resultat)
            {
                string[] noFemeninesSi = { "ara", "�rab", "indi", "unir" };
                foreach (string forma in noFemeninesSi)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.PotApostrofar(), String.Format("\"{0}\" es pot apostrofar", forma));
                }
                string[] noFemeninesNo = { "casa", "iode", "hiatus" };
                foreach (string forma in noFemeninesNo)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!paraula.PotApostrofar(), String.Format("\"{0}\" no es pot apostrofar", forma));
                }
                string[] femeninesSi = { "�ndia", "alba", "hora", "ungla" };
                foreach (string forma in femeninesSi)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.PotApostrofar(true), String.Format("\"{0}\" es pot apostrofar", forma));
                }
                string[] femeninesNo = { "humitat", "casa", "il�l�cita" };
                foreach (string forma in femeninesNo)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!paraula.PotApostrofar(true), String.Format("\"{0}\" no es pot apostrofar", forma));
                }
            });
            grup.NouTest("EsMin()", delegate(RTest resultat)
            {
                string[] si = { "un", "dos", "tres", "quatre-cinc", "" };
                string[] no = { "Un", "DOS", "treS", "Quatre-cinc" };
                foreach (string str in si)
                    resultat.Assert(Cat.EsMin(str), "�s min�scula");
                foreach (string str in no)
                    resultat.Assert(!Cat.EsMin(str), "No �s min�scula");
            });
        }

        private void test_04(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Entrades i mots");
            Regles regles = CarregaRegles(true);
            Identificador idDIEC = new IdentificadorDIEC("*** buit ***", regles, null);
            grup.NouTest("Entrades ignorades", delegate(RTest resultat)
            {
                Entrada entrada;
                Regex trosseja = new Regex(@"\|\|");
                string entrades = "ent=-adelf -adelfa||ent=ab-";
                foreach (string tros in trosseja.Split(entrades))
                {
                    entrada = idDIEC.IdentificaEntrada(tros);
                    resultat.Assert(entrada == null, "S'hauria d'ignorar \"{0}\"", tros);
                }
            });
            grup.NouTest("Neteja d'entrades", delegate(RTest resultat)
            {
                Entrada entrada;
                entrada = idDIEC.IdentificaEntrada("ent=a1^cat1=f.^com1=pl. as");
                resultat.Esperat("a", entrada.Arrel);
                entrada = idDIEC.IdentificaEntrada("ent=abaltir-se^cat1=v. pron.");
                resultat.Esperat("abaltir", entrada.Arrel);
                entrada = idDIEC.IdentificaEntrada("ent=abastador2 abastadora^cat1=m. i f.");
                resultat.Esperat("abastador abastadora", entrada.Arrel);
            });
            grup.NouTest("Informaci� morfol�gica i gramatical", delegate(RTest resultat)
            {
                MorfoGram mf1 = new MorfoGram();
                resultat.Esperat("ND", mf1.ToString());
                MorfoGram mf2 = new MorfoGram(MorfoGram.eCat.NOM);
                resultat.Esperat("NOM", mf2.ToString());
                MorfoGram mf12 = new MorfoGram(mf1, mf2);
                resultat.Esperat("NOM", mf12.ToString());
                MorfoGram mf3 = new MorfoGram(MorfoGram.eCat.VERB);
                try
                {
                    MorfoGram mf23 = new MorfoGram(mf2, mf3);
                    resultat.Error("Error indetectat unint {0} i {1}", mf2, mf3);
                }
                catch (Exception exc)
                {
                    resultat.Nota("Error esperat intentant unir {0} i {1} ({2})", mf2, mf3, exc.Message);
                }
                MorfoGram mf4 = new MorfoGram(MorfoGram.eCat.NOCAT, MorfoGram.eTemps.IPR, MorfoGram.ePers.P3);
                resultat.Esperat("IPR.P3", mf4.ToString());
                MorfoGram mf34 = new MorfoGram(mf3, mf4);
                resultat.Esperat("VERB.IPR.P3", mf34.ToString());
            });
        }

        private void test_05(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Paradigmes");
            Regles regles = CarregaRegles(true);
            Identificador idDIEC = new IdentificadorDIEC("*** buit ***", regles, null);
            grup.NouTest("Nom mascul�, plural en -s, vocal inicial (�bac)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=�bac^cat1=m.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("�bac �bacs d'�bac d'�bacs l'�bac", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Nom mascul�, plural en -s, consonant inicial (bosc)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=bosc^cat1=m.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("bosc boscos boscs", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Adjectiu, plural en -s, vocal inicial (abacial)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=abacial^cat1=adj.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("abacial abacials d'abacial d'abacials l'abacial", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Nom mascul�, plural en -s, consonant inicial (bacterial)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=bacterial^cat1=adj.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("bacterial bacterials", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Nom mascul�, plural en -os (cas)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=cas1^cat1=m.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("cas casos", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
        }

        private void test_06(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Generaci� de diccionaris");
            Regles regles = CarregaRegles(true);
            Identificador idDIEC = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            grup.NouTest(@"Noms", delegate(RTest resultat)
            {
                List<string> paraules = new List<string>();
                paraules.Add("ent=abiet�cies^cat1=f. pl.");
                paraules.Add("ent=humitat^cat1=f. > d'humitat d'humitats humitat humitats");
                paraules.Add("ent=abac�^cat1=m.^com1=pl. -�s");
                paraules.Add("ent=abadessa^cat1=f.");
                paraules.Add("ent=babutxa^cat1=f.");
                paraules.Add("ent=�bac^cat1=m.");
                paraules.Add("ent=bosc^cat1=m.");
                paraules.Add("ent=iarda^cat1=f.");
                paraules.Add("ent=cam�^cat1=m.");
                paraules.Add("ent=cas1^cat1=m.");
                paraules.Add("ent=abaixalleng�es^cat1=m.");
                List<Entrada> entrades = new List<Entrada>();
                foreach (string paraula in paraules)
                {
                    string infoParaula = paraula;
                    string esperat = null;
                    int posFletxa = paraula.IndexOf(" > ");
                    if (posFletxa >= 0)
                    {
                        infoParaula = paraula.Substring(0, posFletxa);
                        esperat = paraula.Substring(posFletxa + 3);
                    }
                    Entrada entrada = idDIEC.IdentificaEntrada(infoParaula);
                    entrades.Add(entrada);
                    string llista = Mot.LlistaPlana(entrada.GeneraMots(Marques.totes, false), Cat.Cmp, false);
                    resultat.Nota("\"{0}\" => {1}", entrada.Arrel, llista);
                    if (esperat != null)
                        resultat.Esperat(esperat, llista);
                }
                resultat.Nota("");
                String[] dic = Entrada.GeneraLiniesDic(entrades, new Marques(true), Entrada.Speller.HUNSPELL, Cat.Cmp);
                foreach (string d in dic)
                    resultat.Nota(d);
            });
            grup.NouTest(@"Adjectius", delegate(RTest resultat)
            {
                List<string> paraules = new List<string>();
                paraules.Add("ent=il�lustrat -ada^cat1=adj.^cat2=m. i f. > d'il�lustrada d'il�lustrades d'il�lustrat d'il�lustrats il�lustrada il�lustrades il�lustrat il�lustrats l'il�lustrat");
                paraules.Add("ent=abastador2 abastadora^cat1=m. i f. > abastador abastadora abastadores abastadors d'abastador d'abastadora d'abastadores d'abastadors l'abastador l'abastadora");
                paraules.Add("ent=feli�^cat1=adj. > feli� felices feli�os");
                paraules.Add("ent=abacial^cat1=adj. > abacial abacials d'abacial d'abacials l'abacial");
                paraules.Add("ent=bacterial^cat1=adj. > bacterial bacterials");
                paraules.Add("ent=iacet� iacetana^cat1=m. i f. > iacet� iacetana iacetanes iacetans");
                paraules.Add("ent=ib�ric -a^cat1=adj. > d'ib�ric d'ib�rica d'ib�rics d'ib�riques ib�ric ib�rica ib�rics ib�riques l'ib�ric");
                paraules.Add("ent=su�s -�ssa^cat1=adj. i m. i f.^cat2=adj.^cat3=m. > su�s su�ssa su�sses su�ssos");
                List<Entrada> entrades = new List<Entrada>();
                foreach (string paraula in paraules)
                {
                    string infoParaula = paraula;
                    string esperat = null;
                    int posFletxa = paraula.IndexOf(" > ");
                    if (posFletxa >= 0)
                    {
                        infoParaula = paraula.Substring(0, posFletxa);
                        esperat = paraula.Substring(posFletxa + 3);
                    }
                    Entrada entrada = idDIEC.IdentificaEntrada(infoParaula);
                    entrades.Add(entrada);
                    string llista = Mot.LlistaPlana(entrada.GeneraMots(Marques.totes, false), Cat.Cmp, false);
                    resultat.Nota("\"{0}\" => {1}", entrada.Arrel, llista);
                    if (esperat != null)
                        resultat.Esperat(esperat, llista);
                }
                resultat.Nota("");
                String[] dic = Entrada.GeneraLiniesDic(entrades, new Marques(true), Entrada.Speller.HUNSPELL, Cat.Cmp);
                foreach (string d in dic)
                    resultat.Nota(d);
            });
            grup.NouTest(@"Invariables", delegate(RTest resultat)
            {
                List<string> paraules = new List<string>();
                paraules.Add("ent=a2^cat1=prep.");
                paraules.Add("ent=a posteriori^cat1=loc. adv.^cat2=loc. adj.");
                List<Entrada> entrades = new List<Entrada>();
                foreach (string paraula in paraules)
                {
                    Entrada entrada = idDIEC.IdentificaEntrada(paraula);
                    entrades.Add(entrada);
                    resultat.Nota("\"{0}\" => {1}", entrada.Arrel, Mot.LlistaPlana(entrada.GeneraMots(Marques.totes, false), Cat.Cmp, false));
                }
                resultat.Nota("");
                String[] dic = Entrada.GeneraLiniesDic(entrades, new Marques(true), Entrada.Speller.HUNSPELL, Cat.Cmp);
                foreach (string d in dic)
                    resultat.Nota(d);
            });
            grup.NouTest(@"Verbs en -ar", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=abastar^cat1=v. intr.^cat2=tr.",
                    "ent=cantar^cat1=v. tr.^cat2=intr."
                    );
            });
            grup.NouTest(@"Verbs en -er/re I", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=v�ncer^cat1=v. tr.^cat2=intr. > VERB.PAR.M.SG=ven�ut",
                    "ent=retre^cat1=v. tr.^cat2=pron. > VERB.PAR.M.SG=retut",
                    "ent=rebre^cat1=v. tr. > VERB.PAR.M.SG=rebut VERB.SIM.P3=rebera|reb�s"
                    );
            });
            grup.NouTest(@"Verbs en -er/re II", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=absoldre^cat1=v. tr.^com1=ger. absolent; p. p. absolt; ind. pr. 1 absolc; subj. pr. absolgui, etc.; subj. imperf. absolgu�s, etc. > VERB.INF=absoldre|absoldre'n|d'absoldre|d'absoldre'n VERB.GER=absolent|absolent-ne VERB.PAR.M.SG=absolt|d'absolt|l'absolt VERB.PAR.M.PL=absolts|d'absolts VERB.PAR.F.SG=absolta|d'absolta|l'absolta VERB.PAR.F.PL=absoltes|d'absoltes VERB.IPR.P1=absolc|n'absolc VERB.IPR.P2=absols|n'absols VERB.IPR.P3=absol|n'absol VERB.IPR.P4=absolem|n'absolem VERB.IPR.P5=absoleu|n'absoleu VERB.IPR.P6=absolen|n'absolen VERB.IIM.P1=absolia|n'absolia VERB.IIM.P2=absolies|n'absolies VERB.IIM.P3=absolia|n'absolia VERB.IIM.P4=absol�em|n'absol�em VERB.IIM.P5=absol�eu|n'absol�eu VERB.IIM.P6=absolien|n'absolien VERB.IPE.P1=absolgu�|n'absolgu� VERB.IPE.P2=absolgueres|n'absolgueres VERB.IPE.P3=absolgu�|n'absolgu� VERB.IPE.P4=absolgu�rem|n'absolgu�rem VERB.IPE.P5=absolgu�reu|n'absolgu�reu VERB.IPE.P6=absolgueren|n'absolgueren VERB.FUT.P1=absoldr�|n'absoldr� VERB.FUT.P2=absoldr�s|n'absoldr�s VERB.FUT.P3=absoldr�|n'absoldr� VERB.FUT.P4=absoldrem|n'absoldrem VERB.FUT.P5=absoldreu|n'absoldreu VERB.FUT.P6=absoldran|n'absoldran VERB.CON.P1=absoldria|n'absoldria VERB.CON.P2=absoldries|n'absoldries VERB.CON.P3=absoldria|n'absoldria VERB.CON.P4=absoldr�em|n'absoldr�em VERB.CON.P5=absoldr�eu|n'absoldr�eu VERB.CON.P6=absoldrien|n'absoldrien VERB.SPR.P1=absolga|absolgui|n'absolga|n'absolgui VERB.SPR.P2=absolgues|absolguis|n'absolgues|n'absolguis VERB.SPR.P3=absolga|absolgui|n'absolga|n'absolgui VERB.SPR.P4=absolguem|n'absolguem VERB.SPR.P5=absolgueu|n'absolgueu VERB.SPR.P6=absolguen|absolguin|n'absolguen|n'absolguin VERB.SIM.P1=absolguera|absolgu�s|n'absolguera|n'absolgu�s VERB.SIM.P2=absolgueres|absolguesses|absolguessis|n'absolgueres|n'absolguesses|n'absolguessis VERB.SIM.P3=absolguera|absolgu�s|n'absolguera|n'absolgu�s VERB.SIM.P4=absolgu�rem|absolgu�ssem|absolgu�ssim|n'absolgu�rem|n'absolgu�ssem|n'absolgu�ssim VERB.SIM.P5=absolgu�reu|absolgu�sseu|absolgu�ssiu|n'absolgu�reu|n'absolgu�sseu|n'absolgu�ssiu VERB.SIM.P6=absolgueren|absolguessen|absolguessin|n'absolgueren|n'absolguessen|n'absolguessin VERB.IMP.P2=absol|absol-ne VERB.IMP.P3=absolga|absolga'n|absolgui|absolgui'n VERB.IMP.P4=absolguem|absolguem-ne VERB.IMP.P5=absoleu|absoleu-ne VERB.IMP.P6=absolguen|absolguen-ne|absolguin|absolguin-ne",
                    "ent=aprendre^cat1=v. tr.^cat2=tr. pron.^com1=ger. aprenent; p. p. apr�s; ind. pr. 1 aprenc, 3 apr�n; subj. pr. aprengui, etc.; subj. imperf. aprengu�s, etc. > VERB.INF=aprendre|aprendre'n|d'aprendre|d'aprendre'n VERB.GER=aprenent|aprenent-ne VERB.PAR.M.SG=apr�s|d'apr�s|l'apr�s VERB.PAR.M.PL=apresos|d'apresos VERB.PAR.F.SG=apresa|d'apresa|l'apresa VERB.PAR.F.PL=apreses|d'apreses VERB.IPR.P1=aprenc|n'aprenc VERB.IPR.P2=aprens|n'aprens VERB.IPR.P3=apr�n|n'apr�n VERB.IPR.P4=aprenem|n'aprenem VERB.IPR.P5=apreneu|n'apreneu VERB.IPR.P6=aprenen|n'aprenen VERB.IIM.P1=aprenia|n'aprenia VERB.IIM.P2=aprenies|n'aprenies VERB.IIM.P3=aprenia|n'aprenia VERB.IIM.P4=apren�em|n'apren�em VERB.IIM.P5=apren�eu|n'apren�eu VERB.IIM.P6=aprenien|n'aprenien VERB.IPE.P1=aprengu�|n'aprengu� VERB.IPE.P2=aprengueres|n'aprengueres VERB.IPE.P3=aprengu�|n'aprengu� VERB.IPE.P4=aprengu�rem|n'aprengu�rem VERB.IPE.P5=aprengu�reu|n'aprengu�reu VERB.IPE.P6=aprengueren|n'aprengueren VERB.FUT.P1=aprendr�|n'aprendr� VERB.FUT.P2=aprendr�s|n'aprendr�s VERB.FUT.P3=aprendr�|n'aprendr� VERB.FUT.P4=aprendrem|n'aprendrem VERB.FUT.P5=aprendreu|n'aprendreu VERB.FUT.P6=aprendran|n'aprendran VERB.CON.P1=aprendria|n'aprendria VERB.CON.P2=aprendries|n'aprendries VERB.CON.P3=aprendria|n'aprendria VERB.CON.P4=aprendr�em|n'aprendr�em VERB.CON.P5=aprendr�eu|n'aprendr�eu VERB.CON.P6=aprendrien|n'aprendrien VERB.SPR.P1=aprenga|aprengui|n'aprenga|n'aprengui VERB.SPR.P2=aprengues|aprenguis|n'aprengues|n'aprenguis VERB.SPR.P3=aprenga|aprengui|n'aprenga|n'aprengui VERB.SPR.P4=aprenguem|n'aprenguem VERB.SPR.P5=aprengueu|n'aprengueu VERB.SPR.P6=aprenguen|aprenguin|n'aprenguen|n'aprenguin VERB.SIM.P1=aprenguera|aprengu�s|n'aprenguera|n'aprengu�s VERB.SIM.P2=aprengueres|aprenguesses|aprenguessis|n'aprengueres|n'aprenguesses|n'aprenguessis VERB.SIM.P3=aprenguera|aprengu�s|n'aprenguera|n'aprengu�s VERB.SIM.P4=aprengu�rem|aprengu�ssem|aprengu�ssim|n'aprengu�rem|n'aprengu�ssem|n'aprengu�ssim VERB.SIM.P5=aprengu�reu|aprengu�sseu|aprengu�ssiu|n'aprengu�reu|n'aprengu�sseu|n'aprengu�ssiu VERB.SIM.P6=aprengueren|aprenguessen|aprenguessin|n'aprengueren|n'aprenguessen|n'aprenguessin VERB.IMP.P2=apr�n|apr�n-ne VERB.IMP.P3=aprenga|aprenga'n|aprengui|aprengui'n VERB.IMP.P4=aprenguem|aprenguem-ne VERB.IMP.P5=apreneu|apreneu-ne VERB.IMP.P6=aprenguen|aprenguen-ne|aprenguin|aprenguin-ne",
                    "ent=at�nyer^cat1=v. tr.^cat2=intr.^com1=p. p. at�s > VERB.INF=at�nyer|at�nyer-ne|d'at�nyer|d'at�nyer-ne VERB.GER=atenyent|atenyent-ne VERB.PAR.M.SG=at�s|d'at�s|l'at�s VERB.PAR.M.PL=atesos|d'atesos VERB.PAR.F.SG=atesa|d'atesa|l'atesa VERB.PAR.F.PL=ateses|d'ateses VERB.IPR.P1=ateny|atenyo|n'ateny|n'atenyo VERB.IPR.P2=atenys|n'atenys VERB.IPR.P3=ateny|n'ateny VERB.IPR.P4=atenyem|n'atenyem VERB.IPR.P5=atenyeu|n'atenyeu VERB.IPR.P6=atenyen|n'atenyen VERB.IIM.P1=atenyia|n'atenyia VERB.IIM.P2=atenyies|n'atenyies VERB.IIM.P3=atenyia|n'atenyia VERB.IIM.P4=ateny�em|n'ateny�em VERB.IIM.P5=ateny�eu|n'ateny�eu VERB.IIM.P6=atenyien|n'atenyien VERB.IPE.P1=ateny�|n'ateny� VERB.IPE.P2=atenyeres|n'atenyeres VERB.IPE.P3=ateny�|n'ateny� VERB.IPE.P4=ateny�rem|n'ateny�rem VERB.IPE.P5=ateny�reu|n'ateny�reu VERB.IPE.P6=atenyeren|n'atenyeren VERB.FUT.P1=atenyer�|n'atenyer� VERB.FUT.P2=atenyer�s|n'atenyer�s VERB.FUT.P3=atenyer�|n'atenyer� VERB.FUT.P4=atenyerem|n'atenyerem VERB.FUT.P5=atenyereu|n'atenyereu VERB.FUT.P6=atenyeran|n'atenyeran VERB.CON.P1=atenyeria|n'atenyeria VERB.CON.P2=atenyeries|n'atenyeries VERB.CON.P3=atenyeria|n'atenyeria VERB.CON.P4=atenyer�em|n'atenyer�em VERB.CON.P5=atenyer�eu|n'atenyer�eu VERB.CON.P6=atenyerien|n'atenyerien VERB.SPR.P1=atenya|atenyi|n'atenya|n'atenyi VERB.SPR.P2=atenyes|atenyis|n'atenyes|n'atenyis VERB.SPR.P3=atenya|atenyi|n'atenya|n'atenyi VERB.SPR.P4=atenyem|n'atenyem VERB.SPR.P5=atenyeu|n'atenyeu VERB.SPR.P6=atenyen|atenyin|n'atenyen|n'atenyin VERB.SIM.P1=atenyera|ateny�s|n'atenyera|n'ateny�s VERB.SIM.P2=atenyeres|atenyesses|atenyessis|n'atenyeres|n'atenyesses|n'atenyessis VERB.SIM.P3=atenyera|ateny�s|n'atenyera|n'ateny�s VERB.SIM.P4=ateny�rem|ateny�ssem|ateny�ssim|n'ateny�rem|n'ateny�ssem|n'ateny�ssim VERB.SIM.P5=ateny�reu|ateny�sseu|ateny�ssiu|n'ateny�reu|n'ateny�sseu|n'ateny�ssiu VERB.SIM.P6=atenyeren|atenyessen|atenyessin|n'atenyeren|n'atenyessen|n'atenyessin VERB.IMP.P2=ateny|ateny-ne VERB.IMP.P3=atenya|atenya'n|atenyi|atenyi'n VERB.IMP.P4=atenyem|atenyem-ne VERB.IMP.P5=atenyeu|atenyeu-ne VERB.IMP.P6=atenyen|atenyen-ne|atenyin|atenyin-ne",
                    "ent=beure1^cat1=v. tr.^cat2=tr. pron.^cat3=pron.^com1=ger. bevent; p. p. begut; ind. pr. bec, beus, beu, bevem, beveu, beuen; subj. pr. begui, etc.; subj. imperf. begu�s, etc.^com2=o beure a morro, o beure a broc d'ampolla^com3=o beure a raig^com4=o beure's l'enteniment^com5=o beure's amb la mirada > VERB.INF=beure|beure'n VERB.GER=bevent|bevent-ne VERB.PAR.M.SG=begut VERB.PAR.M.PL=beguts VERB.PAR.F.SG=beguda VERB.PAR.F.PL=begudes VERB.IPR.P1=bec VERB.IPR.P2=beus VERB.IPR.P3=beu VERB.IPR.P4=bevem VERB.IPR.P5=beveu VERB.IPR.P6=beuen VERB.IIM.P1=bevia VERB.IIM.P2=bevies VERB.IIM.P3=bevia VERB.IIM.P4=bev�em VERB.IIM.P5=bev�eu VERB.IIM.P6=bevien VERB.IPE.P1=begu� VERB.IPE.P2=begueres VERB.IPE.P3=begu� VERB.IPE.P4=begu�rem VERB.IPE.P5=begu�reu VERB.IPE.P6=begueren VERB.FUT.P1=beur� VERB.FUT.P2=beur�s VERB.FUT.P3=beur� VERB.FUT.P4=beurem VERB.FUT.P5=beureu VERB.FUT.P6=beuran VERB.CON.P1=beuria VERB.CON.P2=beuries VERB.CON.P3=beuria VERB.CON.P4=beur�em VERB.CON.P5=beur�eu VERB.CON.P6=beurien VERB.SPR.P1=bega|begui VERB.SPR.P2=begues|beguis VERB.SPR.P3=bega|begui VERB.SPR.P4=beguem VERB.SPR.P5=begueu VERB.SPR.P6=beguen|beguin VERB.SIM.P1=beguera|begu�s VERB.SIM.P2=begueres|beguesses|beguessis VERB.SIM.P3=beguera|begu�s VERB.SIM.P4=begu�rem|begu�ssem|begu�ssim VERB.SIM.P5=begu�reu|begu�sseu|begu�ssiu VERB.SIM.P6=begueren|beguessen|beguessin VERB.IMP.P2=beu|beu-ne VERB.IMP.P3=bega|bega'n|begui|begui'n VERB.IMP.P4=beguem|beguem-ne VERB.IMP.P5=beveu|beveu-ne VERB.IMP.P6=beguen|beguen-ne|beguin|beguin-ne",
                    "ent=caure^cat1=v. intr.^com1=ger. caient; p. p. caigut; ind. pr. caic, caus, cau, caiem, caieu, cauen; ind. imperf. queia, etc.; subj. pr. caigui, etc.; subj. imperf. caigu�s, etc.^com2=o estar si cau no cau^com3=o caure una cosa del cel^com4=o caure del candeler^com5=o caure sota la seva m� > VERB.INF=caure|caure'n VERB.GER=caient|caient-ne VERB.PAR.M.SG=caigut VERB.PAR.M.PL=caiguts VERB.PAR.F.SG=caiguda VERB.PAR.F.PL=caigudes VERB.IPR.P1=caic VERB.IPR.P2=caus VERB.IPR.P3=cau VERB.IPR.P4=caiem VERB.IPR.P5=caieu VERB.IPR.P6=cauen VERB.IIM.P1=queia VERB.IIM.P2=queies VERB.IIM.P3=queia VERB.IIM.P4=qu�iem VERB.IIM.P5=qu�ieu VERB.IIM.P6=queien VERB.IPE.P1=caigu� VERB.IPE.P2=caigueres VERB.IPE.P3=caigu� VERB.IPE.P4=caigu�rem VERB.IPE.P5=caigu�reu VERB.IPE.P6=caigueren VERB.FUT.P1=caur� VERB.FUT.P2=caur�s VERB.FUT.P3=caur� VERB.FUT.P4=caurem VERB.FUT.P5=caureu VERB.FUT.P6=cauran VERB.CON.P1=cauria VERB.CON.P2=cauries VERB.CON.P3=cauria VERB.CON.P4=caur�em VERB.CON.P5=caur�eu VERB.CON.P6=caurien VERB.SPR.P1=caiga|caigui VERB.SPR.P2=caigues|caiguis VERB.SPR.P3=caiga|caigui VERB.SPR.P4=caiguem VERB.SPR.P5=caigueu VERB.SPR.P6=caiguen|caiguin VERB.SIM.P1=caiguera|caigu�s VERB.SIM.P2=caigueres|caiguesses|caiguessis VERB.SIM.P3=caiguera|caigu�s VERB.SIM.P4=caigu�rem|caigu�ssem|caigu�ssim VERB.SIM.P5=caigu�reu|caigu�sseu|caigu�ssiu VERB.SIM.P6=caigueren|caiguessen|caiguessin VERB.IMP.P2=cau|cau-ne VERB.IMP.P3=caiga|caiga'n|caigui|caigui'n VERB.IMP.P4=caiguem|caiguem-ne VERB.IMP.P5=caieu|caieu-ne VERB.IMP.P6=caiguen|caiguen-ne|caiguin|caiguin-ne",
                    "ent=confondre^cat1=pron.^cat2=v. tr.^com1=ger. confonent; p. p. conf�s; ind. pr. 1 confonc; subj. pr. confongui, etc.; subj. imperf. confongu�s, etc. > VERB.INF=confondre|confondre'n VERB.GER=confonent|confonent-ne VERB.PAR.M.SG=conf�s VERB.PAR.M.PL=confosos VERB.PAR.F.SG=confosa VERB.PAR.F.PL=confoses VERB.IPR.P1=confonc VERB.IPR.P2=confons VERB.IPR.P3=confon VERB.IPR.P4=confonem VERB.IPR.P5=confoneu VERB.IPR.P6=confonen VERB.IIM.P1=confonia VERB.IIM.P2=confonies VERB.IIM.P3=confonia VERB.IIM.P4=confon�em VERB.IIM.P5=confon�eu VERB.IIM.P6=confonien VERB.IPE.P1=confongu� VERB.IPE.P2=confongueres VERB.IPE.P3=confongu� VERB.IPE.P4=confongu�rem VERB.IPE.P5=confongu�reu VERB.IPE.P6=confongueren VERB.FUT.P1=confondr� VERB.FUT.P2=confondr�s VERB.FUT.P3=confondr� VERB.FUT.P4=confondrem VERB.FUT.P5=confondreu VERB.FUT.P6=confondran VERB.CON.P1=confondria VERB.CON.P2=confondries VERB.CON.P3=confondria VERB.CON.P4=confondr�em VERB.CON.P5=confondr�eu VERB.CON.P6=confondrien VERB.SPR.P1=confonga|confongui VERB.SPR.P2=confongues|confonguis VERB.SPR.P3=confonga|confongui VERB.SPR.P4=confonguem VERB.SPR.P5=confongueu VERB.SPR.P6=confonguen|confonguin VERB.SIM.P1=confonguera|confongu�s VERB.SIM.P2=confongueres|confonguesses|confonguessis VERB.SIM.P3=confonguera|confongu�s VERB.SIM.P4=confongu�rem|confongu�ssem|confongu�ssim VERB.SIM.P5=confongu�reu|confongu�sseu|confongu�ssiu VERB.SIM.P6=confongueren|confonguessen|confonguessin VERB.IMP.P2=confon|confon-ne VERB.IMP.P3=confonga|confonga'n|confongui|confongui'n VERB.IMP.P4=confonguem|confonguem-ne VERB.IMP.P5=confoneu|confoneu-ne VERB.IMP.P6=confonguen|confonguen-ne|confonguin|confonguin-ne"
                    );
            });
            grup.NouTest(@"Verbs en -ir, incoatius", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=enaltir^cat1=v. tr. > VERB.INF=d'enaltir|d'enaltir-ne|enaltir|enaltir-ne VERB.GER=enaltint|enaltint-ne VERB.PAR.M.SG=d'enaltit|enaltit|l'enaltit VERB.PAR.M.PL=d'enaltits|enaltits VERB.PAR.F.SG=d'enaltida|enaltida|l'enaltida VERB.PAR.F.PL=d'enaltides|enaltides VERB.IPR.P1=enalteixo|enaltesc|enaltisc|n'enalteixo|n'enaltesc|n'enaltisc VERB.IPR.P2=enalteixes|enaltixes|n'enalteixes|n'enaltixes VERB.IPR.P3=enalteix|enaltix|n'enalteix|n'enaltix VERB.IPR.P4=enaltim|n'enaltim VERB.IPR.P5=enaltiu|n'enaltiu VERB.IPR.P6=enalteixen|enaltixen|n'enalteixen|n'enaltixen VERB.IIM.P1=enaltia|n'enaltia VERB.IIM.P2=enalties|n'enalties VERB.IIM.P3=enaltia|n'enaltia VERB.IIM.P4=enalt�em|n'enalt�em VERB.IIM.P5=enalt�eu|n'enalt�eu VERB.IIM.P6=enaltien|n'enaltien VERB.IPE.P1=enalt�|n'enalt� VERB.IPE.P2=enaltires|n'enaltires VERB.IPE.P3=enalt�|n'enalt� VERB.IPE.P4=enalt�rem|n'enalt�rem VERB.IPE.P5=enalt�reu|n'enalt�reu VERB.IPE.P6=enaltiren|n'enaltiren VERB.FUT.P1=enaltir�|n'enaltir� VERB.FUT.P2=enaltir�s|n'enaltir�s VERB.FUT.P3=enaltir�|n'enaltir� VERB.FUT.P4=enaltirem|n'enaltirem VERB.FUT.P5=enaltireu|n'enaltireu VERB.FUT.P6=enaltiran|n'enaltiran VERB.CON.P1=enaltiria|n'enaltiria VERB.CON.P2=enaltiries|n'enaltiries VERB.CON.P3=enaltiria|n'enaltiria VERB.CON.P4=enaltir�em|n'enaltir�em VERB.CON.P5=enaltir�eu|n'enaltir�eu VERB.CON.P6=enaltirien|n'enaltirien VERB.SPR.P1=enalteixi|enaltesqui|enaltisca|n'enalteixi|n'enaltesqui|n'enaltisca VERB.SPR.P2=enalteixis|enaltesquis|enaltisques|n'enalteixis|n'enaltesquis|n'enaltisques VERB.SPR.P3=enalteixi|enaltesqui|enaltisca|n'enalteixi|n'enaltesqui|n'enaltisca VERB.SPR.P4=enaltim|n'enaltim VERB.SPR.P5=enaltiu|n'enaltiu VERB.SPR.P6=enalteixin|enaltesquin|enaltisquen|n'enalteixin|n'enaltesquin|n'enaltisquen VERB.SIM.P1=enaltira|enalt�s|n'enaltira|n'enalt�s VERB.SIM.P2=enaltires|enaltisses|enaltissis|n'enaltires|n'enaltisses|n'enaltissis VERB.SIM.P3=enaltira|enalt�s|n'enaltira|n'enalt�s VERB.SIM.P4=enalt�rem|enalt�ssem|enalt�ssim|n'enalt�rem|n'enalt�ssem|n'enalt�ssim VERB.SIM.P5=enalt�reu|enalt�sseu|enalt�ssiu|n'enalt�reu|n'enalt�sseu|n'enalt�ssiu VERB.SIM.P6=enaltiren|enaltissen|enaltissin|n'enaltiren|n'enaltissen|n'enaltissin VERB.IMP.P2=enalteix|enalteix-ne|enaltix|enaltix-ne VERB.IMP.P3=enalteixi|enalteixi'n|enaltesqui|enaltesqui'n|enaltisca|enaltisca'n VERB.IMP.P4=enaltim|enaltim-ne VERB.IMP.P5=enaltiu|enaltiu-ne VERB.IMP.P6=enalteixin|enalteixin-ne|enaltesquin|enaltesquin-ne|enaltisquen|enaltisquen-ne",
                    "ent=partir^cat1=v. tr.^cat2=pron.^cat3=intr. > VERB.INF=partir|partir-ne VERB.GER=partint|partint-ne VERB.PAR.M.SG=partit VERB.PAR.M.PL=partits VERB.PAR.F.SG=partida VERB.PAR.F.PL=partides VERB.IPR.P1=parteixo|partesc|partisc VERB.IPR.P2=parteixes|partixes VERB.IPR.P3=parteix|partix VERB.IPR.P4=partim VERB.IPR.P5=partiu VERB.IPR.P6=parteixen|partixen VERB.IIM.P1=partia VERB.IIM.P2=parties VERB.IIM.P3=partia VERB.IIM.P4=part�em VERB.IIM.P5=part�eu VERB.IIM.P6=partien VERB.IPE.P1=part� VERB.IPE.P2=partires VERB.IPE.P3=part� VERB.IPE.P4=part�rem VERB.IPE.P5=part�reu VERB.IPE.P6=partiren VERB.FUT.P1=partir� VERB.FUT.P2=partir�s VERB.FUT.P3=partir� VERB.FUT.P4=partirem VERB.FUT.P5=partireu VERB.FUT.P6=partiran VERB.CON.P1=partiria VERB.CON.P2=partiries VERB.CON.P3=partiria VERB.CON.P4=partir�em VERB.CON.P5=partir�eu VERB.CON.P6=partirien VERB.SPR.P1=parteixi|partesqui|partisca VERB.SPR.P2=parteixis|partesquis|partisques VERB.SPR.P3=parteixi|partesqui|partisca VERB.SPR.P4=partim VERB.SPR.P5=partiu VERB.SPR.P6=parteixin|partesquin|partisquen VERB.SIM.P1=partira|part�s VERB.SIM.P2=partires|partisses|partissis VERB.SIM.P3=partira|part�s VERB.SIM.P4=part�rem|part�ssem|part�ssim VERB.SIM.P5=part�reu|part�sseu|part�ssiu VERB.SIM.P6=partiren|partissen|partissin VERB.IMP.P2=parteix|parteix-ne|partix|partix-ne VERB.IMP.P3=parteixi|parteixi'n|partesqui|partesqui'n|partisca|partisca'n VERB.IMP.P4=partim|partim-ne VERB.IMP.P5=partiu|partiu-ne VERB.IMP.P6=parteixin|parteixin-ne|partesquin|partesquin-ne|partisquen|partisquen-ne",
                    "ent=patir^cat1=v. tr.^cat2=intr. > VERB.INF=patir|patir-ne VERB.GER=patint|patint-ne VERB.PAR.M.SG=patit VERB.PAR.M.PL=patits VERB.PAR.F.SG=patida VERB.PAR.F.PL=patides VERB.IPR.P1=pateixo|patesc|patisc VERB.IPR.P2=pateixes|patixes VERB.IPR.P3=pateix|patix VERB.IPR.P4=patim VERB.IPR.P5=patiu VERB.IPR.P6=pateixen|patixen VERB.IIM.P1=patia VERB.IIM.P2=paties VERB.IIM.P3=patia VERB.IIM.P4=pat�em VERB.IIM.P5=pat�eu VERB.IIM.P6=patien VERB.IPE.P1=pat� VERB.IPE.P2=patires VERB.IPE.P3=pat� VERB.IPE.P4=pat�rem VERB.IPE.P5=pat�reu VERB.IPE.P6=patiren VERB.FUT.P1=patir� VERB.FUT.P2=patir�s VERB.FUT.P3=patir� VERB.FUT.P4=patirem VERB.FUT.P5=patireu VERB.FUT.P6=patiran VERB.CON.P1=patiria VERB.CON.P2=patiries VERB.CON.P3=patiria VERB.CON.P4=patir�em VERB.CON.P5=patir�eu VERB.CON.P6=patirien VERB.SPR.P1=pateixi|patesqui|patisca VERB.SPR.P2=pateixis|patesquis|patisques VERB.SPR.P3=pateixi|patesqui|patisca VERB.SPR.P4=patim VERB.SPR.P5=patiu VERB.SPR.P6=pateixin|patesquin|patisquen VERB.SIM.P1=patira|pat�s VERB.SIM.P2=patires|patisses|patissis VERB.SIM.P3=patira|pat�s VERB.SIM.P4=pat�rem|pat�ssem|pat�ssim VERB.SIM.P5=pat�reu|pat�sseu|pat�ssiu VERB.SIM.P6=patiren|patissen|patissin VERB.IMP.P2=pateix|pateix-ne|patix|patix-ne VERB.IMP.P3=pateixi|pateixi'n|patesqui|patesqui'n|patisca|patisca'n VERB.IMP.P4=patim|patim-ne VERB.IMP.P5=patiu|patiu-ne VERB.IMP.P6=pateixin|pateixin-ne|patesquin|patesquin-ne|patisquen|patisquen-ne"
                    );
            });
            grup.NouTest(@"Verbs en -ir, purs", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=munyir^cat1=v. tr.^com1=ind. pres. 3 muny",
                    "ent=collir^cat1=v. tr.^com1=ind. pr. cullo, culls, cull, collim, colliu, cullen; subj. pr. culli, cullis, culli, collim, colliu, cullin > VERB.INF=collir|collir-ne VERB.GER=collint|collint-ne VERB.PAR.M.SG=collit VERB.PAR.M.PL=collits VERB.PAR.F.SG=collida VERB.PAR.F.PL=collides VERB.IPR.P1=cull|cullo VERB.IPR.P2=culls VERB.IPR.P3=cull VERB.IPR.P4=collim VERB.IPR.P5=colliu VERB.IPR.P6=cullen VERB.IIM.P1=collia VERB.IIM.P2=collies VERB.IIM.P3=collia VERB.IIM.P4=coll�em VERB.IIM.P5=coll�eu VERB.IIM.P6=collien VERB.IPE.P1=coll� VERB.IPE.P2=collires VERB.IPE.P3=coll� VERB.IPE.P4=coll�rem VERB.IPE.P5=coll�reu VERB.IPE.P6=colliren VERB.FUT.P1=collir� VERB.FUT.P2=collir�s VERB.FUT.P3=collir� VERB.FUT.P4=collirem VERB.FUT.P5=collireu VERB.FUT.P6=colliran VERB.CON.P1=colliria VERB.CON.P2=colliries VERB.CON.P3=colliria VERB.CON.P4=collir�em VERB.CON.P5=collir�eu VERB.CON.P6=collirien VERB.SPR.P1=culla|culli VERB.SPR.P2=culles|cullis VERB.SPR.P3=culla|culli VERB.SPR.P4=collim VERB.SPR.P5=colliu VERB.SPR.P6=cullen|cullin VERB.SIM.P1=collira|coll�s VERB.SIM.P2=collires|collisses|collissis VERB.SIM.P3=collira|coll�s VERB.SIM.P4=coll�rem|coll�ssem|coll�ssim VERB.SIM.P5=coll�reu|coll�sseu|coll�ssiu VERB.SIM.P6=colliren|collissen|collissin VERB.IMP.P2=cull|cull-ne VERB.IMP.P3=culla|culla'n|culli|culli'n VERB.IMP.P4=collim|collim-ne VERB.IMP.P5=colliu|colliu-ne VERB.IMP.P6=cullen|cullen-ne|cullin|cullin-ne",
                    "ent=acollir^cat1=v. tr.^cat2=pron.^com1=quant a la flexi�, com collir > VERB.INF=acollir|acollir-ne|d'acollir|d'acollir-ne VERB.GER=acollint|acollint-ne VERB.PAR.M.SG=acollit|d'acollit|l'acollit VERB.PAR.M.PL=acollits|d'acollits VERB.PAR.F.SG=acollida|d'acollida|l'acollida VERB.PAR.F.PL=acollides|d'acollides VERB.IPR.P1=acull|acullo|n'acull|n'acullo VERB.IPR.P2=aculls|n'aculls VERB.IPR.P3=acull VERB.IPR.P4=acollim|n'acollim VERB.IPR.P5=acolliu|n'acolliu VERB.IPR.P6=acullen|n'acullen VERB.IIM.P1=acollia|n'acollia VERB.IIM.P2=acollies|n'acollies VERB.IIM.P3=acollia|n'acollia VERB.IIM.P4=acoll�em|n'acoll�em VERB.IIM.P5=acoll�eu|n'acoll�eu VERB.IIM.P6=acollien|n'acollien VERB.IPE.P1=acoll�|n'acoll� VERB.IPE.P2=acollires|n'acollires VERB.IPE.P3=acoll�|n'acoll� VERB.IPE.P4=acoll�rem|n'acoll�rem VERB.IPE.P5=acoll�reu|n'acoll�reu VERB.IPE.P6=acolliren|n'acolliren VERB.FUT.P1=acollir�|n'acollir� VERB.FUT.P2=acollir�s|n'acollir�s VERB.FUT.P3=acollir�|n'acollir� VERB.FUT.P4=acollirem|n'acollirem VERB.FUT.P5=acollireu|n'acollireu VERB.FUT.P6=acolliran|n'acolliran VERB.CON.P1=acolliria|n'acolliria VERB.CON.P2=acolliries|n'acolliries VERB.CON.P3=acolliria|n'acolliria VERB.CON.P4=acollir�em|n'acollir�em VERB.CON.P5=acollir�eu|n'acollir�eu VERB.CON.P6=acollirien|n'acollirien VERB.SPR.P1=aculla|aculli|n'aculla|n'aculli VERB.SPR.P2=aculles|acullis|n'aculles|n'acullis VERB.SPR.P3=aculla|aculli|n'aculla|n'aculli VERB.SPR.P4=acollim|n'acollim VERB.SPR.P5=acolliu|n'acolliu VERB.SPR.P6=acullen|acullin|n'acullen|n'acullin VERB.SIM.P1=acollira|acoll�s|n'acollira|n'acoll�s VERB.SIM.P2=acollires|acollisses|acollissis|n'acollires|n'acollisses|n'acollissis VERB.SIM.P3=acollira|acoll�s|n'acollira|n'acoll�s VERB.SIM.P4=acoll�rem|acoll�ssem|acoll�ssim|n'acoll�rem|n'acoll�ssem|n'acoll�ssim VERB.SIM.P5=acoll�reu|acoll�sseu|acoll�ssiu|n'acoll�reu|n'acoll�sseu|n'acoll�ssiu VERB.SIM.P6=acolliren|acollissen|acollissin|n'acolliren|n'acollissen|n'acollissin VERB.IMP.P2=acull|acull-ne VERB.IMP.P3=aculla|aculla'n|aculli|aculli'n VERB.IMP.P4=acollim|acollim-ne VERB.IMP.P5=acolliu|acolliu-ne VERB.IMP.P6=acullen|acullen-ne|acullin|acullin-ne"
                    );
            });
            grup.NouTest(@"M�s verbs irregulars", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=anar^cat1=v. intr.^com1=ind. pr. vaig, vas, va, anem, aneu, van; ind. fut. anir� o ir�, etc.; subj. pr. 1 vagi, 2 vagis, 3 vagi, 6 vagin; imper. 2 v�s > VERB.INF=anar|anar-ne|d'anar|d'anar-ne VERB.GER=anant|anant-ne VERB.PAR.M.SG=anat|d'anat|l'anat VERB.PAR.M.PL=anats|d'anats VERB.PAR.F.SG=anada|d'anada|l'anada VERB.PAR.F.PL=anades|d'anades VERB.IPR.P1=vaig VERB.IPR.P2=vas VERB.IPR.P3=va VERB.IPR.P4=anam|anem|n'anam|n'anem VERB.IPR.P5=anau|aneu|n'anau|n'aneu VERB.IPR.P6=van VERB.IIM.P1=anava|n'anava VERB.IIM.P2=anaves|n'anaves VERB.IIM.P3=anava|n'anava VERB.IIM.P4=an�vem|n'an�vem VERB.IIM.P5=an�veu|n'an�veu VERB.IIM.P6=anaven|n'anaven VERB.IPE.P1=an�|n'an� VERB.IPE.P2=anares|n'anares VERB.IPE.P3=an�|n'an� VERB.IPE.P4=an�rem|n'an�rem VERB.IPE.P5=an�reu|n'an�reu VERB.IPE.P6=anaren|n'anaren VERB.FUT.P1=anir�|ir�|n'anir�|n'ir� VERB.FUT.P2=anir�s|ir�s|n'anir�s|n'ir�s VERB.FUT.P3=anir�|ir�|n'anir�|n'ir� VERB.FUT.P4=anirem|irem|n'anirem|n'irem VERB.FUT.P5=anireu|ireu|n'anireu|n'ireu VERB.FUT.P6=aniran|iran|n'aniran|n'iran VERB.CON.P1=aniria|iria|n'aniria|n'iria VERB.CON.P2=aniries|iries|n'aniries|n'iries VERB.CON.P3=aniria|iria|n'aniria|n'iria VERB.CON.P4=anir�em|ir�em|n'anir�em|n'ir�em VERB.CON.P5=anir�eu|ir�eu|n'anir�eu|n'ir�eu VERB.CON.P6=anirien|irien|n'anirien|n'irien VERB.SPR.P1=vagi|vaja VERB.SPR.P2=vages|vagis VERB.SPR.P3=vagi|vaja VERB.SPR.P4=anem|n'anem VERB.SPR.P5=aneu|n'aneu VERB.SPR.P6=vagen|vagin VERB.SIM.P1=anara|an�s|an�s|n'anara|n'an�s|n'an�s VERB.SIM.P2=anares|anassis|anesses|anessis|n'anares|n'anassis|n'anesses|n'anessis VERB.SIM.P3=anara|an�s|an�s|n'anara|n'an�s|n'an�s VERB.SIM.P4=an�rem|an�ssim|an�ssem|an�ssim|n'an�rem|n'an�ssim|n'an�ssem|n'an�ssim VERB.SIM.P5=an�reu|an�ssiu|an�sseu|an�ssiu|n'an�reu|n'an�ssiu|n'an�sseu|n'an�ssiu VERB.SIM.P6=anaren|anassin|anessen|anessin|n'anaren|n'anassin|n'anessen|n'anessin VERB.IMP.P2=v�s|v�s-ne VERB.IMP.P3=vagi|vagi'n|vaja|vaja'n VERB.IMP.P4=anem|anem-ne VERB.IMP.P5=anau|anau-ne|aneu|aneu-ne VERB.IMP.P6=vagen|vagen-ne|vagin|vagin-ne",
                    "ent=donar^cat1=v. tr.^cat2=pron.^cat3=intr.^com1=ind. pr. 2 d�nes, 3 d�na > VERB.INF=donar|donar-ne VERB.GER=donant|donant-ne VERB.PAR.M.SG=donat VERB.PAR.M.PL=donats VERB.PAR.F.SG=donada VERB.PAR.F.PL=donades VERB.IPR.P1=don|done|dono VERB.IPR.P2=d�nes VERB.IPR.P3=d�na VERB.IPR.P4=donam|donem VERB.IPR.P5=donau|doneu VERB.IPR.P6=donen VERB.IIM.P1=donava VERB.IIM.P2=donaves VERB.IIM.P3=donava VERB.IIM.P4=don�vem VERB.IIM.P5=don�veu VERB.IIM.P6=donaven VERB.IPE.P1=don� VERB.IPE.P2=donares VERB.IPE.P3=don� VERB.IPE.P4=don�rem VERB.IPE.P5=don�reu VERB.IPE.P6=donaren VERB.FUT.P1=donar� VERB.FUT.P2=donar�s VERB.FUT.P3=donar� VERB.FUT.P4=donarem VERB.FUT.P5=donareu VERB.FUT.P6=donaran VERB.CON.P1=donaria VERB.CON.P2=donaries VERB.CON.P3=donaria VERB.CON.P4=donar�em VERB.CON.P5=donar�eu VERB.CON.P6=donarien VERB.SPR.P1=done|doni VERB.SPR.P2=d�nes|donis VERB.SPR.P3=done|doni VERB.SPR.P4=donem VERB.SPR.P5=doneu VERB.SPR.P6=donen|donin VERB.SIM.P1=donara|don�s|don�s VERB.SIM.P2=donares|donassis|donesses|donessis VERB.SIM.P3=donara|don�s|don�s VERB.SIM.P4=don�rem|don�ssim|don�ssem|don�ssim VERB.SIM.P5=don�reu|don�ssiu|don�sseu|don�ssiu VERB.SIM.P6=donaren|donassin|donessen|donessin VERB.IMP.P2=d�na|d�na'n VERB.IMP.P3=done|done'n|doni|doni'n VERB.IMP.P4=donem|donem-ne VERB.IMP.P5=donau|donau-ne|doneu|doneu-ne VERB.IMP.P6=donen|donen-ne|donin|donin-ne",
                    "ent=estar^cat1=v. intr.^cat2=pron.^cat3=tr.^com1=ind. pr. 1 estic, 2 est�s, 3 est�, 6 estan; subj. pr. estigui, etc.; subj. imperf. estigu�s, etc.; imper. estigues, estigui, estiguem, estigueu, estiguin^com2=o estar per alguna cosa > VERB.INF=d'estar|d'estar-ne|estar|estar-ne VERB.GER=estant|estant-ne VERB.PAR.M.SG=d'estat|estat|l'estat VERB.PAR.M.PL=d'estats|estats VERB.PAR.F.SG=d'estada|estada|l'estada VERB.PAR.F.PL=d'estades|estades VERB.IPR.P1=estic|n'estic VERB.IPR.P2=est�s|n'est�s VERB.IPR.P3=est�|n'est� VERB.IPR.P4=estam|estem|n'estam|n'estem VERB.IPR.P5=estau|esteu|n'estau|n'esteu VERB.IPR.P6=estan|n'estan VERB.IIM.P1=estava|n'estava VERB.IIM.P2=estaves|n'estaves VERB.IIM.P3=estava|n'estava VERB.IIM.P4=est�vem|n'est�vem VERB.IIM.P5=est�veu|n'est�veu VERB.IIM.P6=estaven|n'estaven VERB.IPE.P1=estigu�|n'estigu� VERB.IPE.P2=estigueres|n'estigueres VERB.IPE.P3=estigu�|n'estigu� VERB.IPE.P4=estigu�rem|n'estigu�rem VERB.IPE.P5=estigu�reu|n'estigu�reu VERB.IPE.P6=estigueren|n'estigueren VERB.FUT.P1=estar�|n'estar� VERB.FUT.P2=estar�s|n'estar�s VERB.FUT.P3=estar�|n'estar� VERB.FUT.P4=estarem|n'estarem VERB.FUT.P5=estareu|n'estareu VERB.FUT.P6=estaran|n'estaran VERB.CON.P1=estaria|n'estaria VERB.CON.P2=estaries|n'estaries VERB.CON.P3=estaria|n'estaria VERB.CON.P4=estar�em|n'estar�em VERB.CON.P5=estar�eu|n'estar�eu VERB.CON.P6=estarien|n'estarien VERB.SPR.P1=estiga|estigui|n'estiga|n'estigui VERB.SPR.P2=estigues|estiguis|n'estigues|n'estiguis VERB.SPR.P3=estiga|estigui|n'estiga|n'estigui VERB.SPR.P4=estiguem|n'estiguem VERB.SPR.P5=estigueu|n'estigueu VERB.SPR.P6=estiguen|estiguin|n'estiguen|n'estiguin VERB.SIM.P1=estiguera|estigu�s|n'estiguera|n'estigu�s VERB.SIM.P2=estigueres|estiguesses|estiguessis|n'estigueres|n'estiguesses|n'estiguessis VERB.SIM.P3=estiguera|estigu�s|n'estiguera|n'estigu�s VERB.SIM.P4=estigu�rem|estigu�ssem|estigu�ssim|n'estigu�rem|n'estigu�ssem|n'estigu�ssim VERB.SIM.P5=estigu�reu|estigu�sseu|estigu�ssiu|n'estigu�reu|n'estigu�sseu|n'estigu�ssiu VERB.SIM.P6=estigueren|estiguessen|estiguessin|n'estigueren|n'estiguessen|n'estiguessin VERB.IMP.P2=estigues|estigues-ne VERB.IMP.P3=estiga|estiga'n|estigui|estigui'n VERB.IMP.P4=estiguem|estiguem-ne VERB.IMP.P5=estigueu|estigueu-ne VERB.IMP.P6=estiguen|estiguen-ne|estiguin|estiguin-ne",
                    "ent=dir1^cat1=v. tr.^com1=ger. dient; p. p. dit; ind. pr. dic, dius, diu, diem, dieu, diuen; ind. imperf. deia, etc.; subj. pr. digui, etc.; subj. imperf. digu�s, etc.; imperat. 2 digues, 5 digueu^com2=o no trobar res a dir^com3=o tan aviat dit com fet^com4=o vol dir?, o voleu dir?^com5=o per dir-ho aix�, o com si digu�ssim, o que digu�ssim^com6=o dir-ne de tot color, o dir-ne de coents, o dir-ne de verdes i madures, o dir-ne per a salar, o dir de tot^com7=o dir de v�s, o dir de vost� > VERB.INF=dir|dir-ne VERB.GER=dient|dient-ne VERB.PAR.M.SG=dit VERB.PAR.M.PL=dits VERB.PAR.F.SG=dita VERB.PAR.F.PL=dites VERB.IPR.P1=dic VERB.IPR.P2=dius VERB.IPR.P3=diu VERB.IPR.P4=deim|diem VERB.IPR.P5=deis|dieu VERB.IPR.P6=diuen VERB.IIM.P1=deia VERB.IIM.P2=deies VERB.IIM.P3=deia VERB.IIM.P4=d�iem VERB.IIM.P5=d�ieu VERB.IIM.P6=deien VERB.IPE.P1=digu� VERB.IPE.P2=digueres VERB.IPE.P3=digu� VERB.IPE.P4=digu�rem VERB.IPE.P5=digu�reu VERB.IPE.P6=digueren VERB.FUT.P1=dir� VERB.FUT.P2=dir�s VERB.FUT.P3=dir� VERB.FUT.P4=direm VERB.FUT.P5=direu VERB.FUT.P6=diran VERB.CON.P1=diria VERB.CON.P2=diries VERB.CON.P3=diria VERB.CON.P4=dir�em VERB.CON.P5=dir�eu VERB.CON.P6=dirien VERB.SPR.P1=diga|digui VERB.SPR.P2=digues|diguis VERB.SPR.P3=diga|digui VERB.SPR.P4=diguem VERB.SPR.P5=digueu VERB.SPR.P6=diguen|diguin VERB.SIM.P1=diguera|digu�s VERB.SIM.P2=digueres|diguesses|diguessis VERB.SIM.P3=diguera|digu�s VERB.SIM.P4=digu�rem|digu�ssem|digu�ssim VERB.SIM.P5=digu�reu|digu�sseu|digu�ssiu VERB.SIM.P6=digueren|diguessen|diguessin VERB.IMP.P2=digues|digues-ne VERB.IMP.P3=diga|diga'n|digui|digui'n VERB.IMP.P4=diguem|diguem-ne VERB.IMP.P5=digueu|digueu-ne VERB.IMP.P6=diguen|diguen-ne|diguin|diguin-ne",
                    "ent=obtenir^cat1=v. tr.^com1=quant a la flexi�, com mantenir > VERB.INF=d'obtenir|d'obtenir-ne|obtenir|obtenir-ne VERB.GER=obtenint|obtenint-ne VERB.PAR.M.SG=d'obtengut|d'obtingut|l'obtengut|l'obtingut|obtengut|obtingut VERB.PAR.M.PL=d'obtenguts|d'obtinguts|obtenguts|obtinguts VERB.PAR.F.SG=d'obtenguda|d'obtinguda|l'obtenguda|l'obtinguda|obtenguda|obtinguda VERB.PAR.F.PL=d'obtengudes|d'obtingudes|obtengudes|obtingudes VERB.IPR.P1=n'obtenc|n'obtinc|obtenc|obtinc VERB.IPR.P2=n'obtens|obtens VERB.IPR.P3=n'obt�|obt� VERB.IPR.P4=n'obtenim|obtenim VERB.IPR.P5=n'obteniu|obteniu VERB.IPR.P6=n'obtenen|obtenen VERB.IIM.P1=n'obtenia|obtengu�s|obtenia VERB.IIM.P2=n'obtenies|obtenies VERB.IIM.P3=n'obtenia|obtenia VERB.IIM.P4=n'obten�em|obten�em VERB.IIM.P5=n'obten�eu|obten�eu VERB.IIM.P6=n'obtenien|obtenien VERB.IPE.P1=n'obtengu�|n'obtingu�|obtengu�|obtingu� VERB.IPE.P2=n'obtengueres|n'obtingueres|obtengueres|obtingueres VERB.IPE.P3=n'obtengu�|n'obtingu�|obtengu�|obtingu� VERB.IPE.P4=n'obtengu�rem|n'obtingu�rem|obtengu�rem|obtingu�rem VERB.IPE.P5=n'obtengu�reu|n'obtingu�reu|obtengu�reu|obtingu�reu VERB.IPE.P6=n'obtengueren|n'obtingueren|obtengueren|obtingueren VERB.FUT.P1=n'obtendr�|n'obtindr�|obtendr�|obtindr� VERB.FUT.P2=n'obtendr�s|n'obtindr�s|obtendr�s|obtindr�s VERB.FUT.P3=n'obtendr�|n'obtindr�|obtendr�|obtindr� VERB.FUT.P4=n'obtendrem|n'obtindrem|obtendrem|obtindrem VERB.FUT.P5=n'obtendreu|n'obtindreu|obtendreu|obtindreu VERB.FUT.P6=n'obtendran|n'obtindran|obtendran|obtindran VERB.CON.P1=n'obtendria|n'obtindria|obtendria|obtindria VERB.CON.P2=n'obtendries|n'obtindries|obtendries|obtindries VERB.CON.P3=n'obtendria|n'obtindria|obtendria|obtindria VERB.CON.P4=n'obtendr�em|n'obtindr�em|obtendr�em|obtindr�em VERB.CON.P5=n'obtendr�eu|n'obtindr�eu|obtendr�eu|obtindr�eu VERB.CON.P6=n'obtendrien|n'obtindrien|obtendrien|obtindrien VERB.SPR.P1=n'obtenga|n'obtengui|n'obtinga|n'obtingui|obtenga|obtengui|obtinga|obtingui VERB.SPR.P2=n'obtengues|n'obtenguis|n'obtingues|n'obtinguis|obtengues|obtenguis|obtingues|obtinguis VERB.SPR.P3=n'obtenga|n'obtengui|n'obtinga|n'obtingui|obtenga|obtengui|obtinga|obtingui VERB.SPR.P4=n'obtenguem|n'obtinguem|obtenguem|obtinguem VERB.SPR.P5=n'obtengueu|n'obtingueu|obtengueu|obtingueu VERB.SPR.P6=n'obtenguen|n'obtenguin|n'obtinguen|n'obtinguin|obtenguen|obtenguin|obtinguen|obtinguin VERB.SIM.P1=n'obtenguera|n'obtengu�s|n'obtinguera|n'obtingu�s|obtenguera|obtengu�s|obtinguera|obtingu�s VERB.SIM.P2=n'obtengueres|n'obtenguesses|n'obtenguessis|n'obtingueres|n'obtinguesses|n'obtinguessis|obtengueres|obtenguesses|obtenguessis|obtingueres|obtinguesses|obtinguessis VERB.SIM.P3=n'obtenguera|n'obtengu�s|n'obtinguera|n'obtingu�s|obtenguera|obtengu�s|obtinguera|obtingu�s VERB.SIM.P4=n'obtengu�rem|n'obtengu�ssem|n'obtengu�ssim|n'obtingu�rem|n'obtingu�ssem|n'obtingu�ssim|obtengu�rem|obtengu�ssem|obtengu�ssim|obtingu�rem|obtingu�ssem|obtingu�ssim VERB.SIM.P5=n'obtengu�reu|n'obtengu�sseu|n'obtengu�ssiu|n'obtingu�reu|n'obtingu�sseu|n'obtingu�ssiu|obtengu�reu|obtengu�sseu|obtengu�ssiu|obtingu�reu|obtingu�sseu|obtingu�ssiu VERB.SIM.P6=n'obtengueren|n'obtenguessen|n'obtenguessin|n'obtingueren|n'obtinguessen|n'obtinguessin|obtengueren|obtenguessen|obtenguessin|obtingueren|obtinguessen|obtinguessin VERB.IMP.P2=obt�n|obt�n-ne|obtingues|obtingues-ne VERB.IMP.P3=obtenga|obtenga'n|obtengui|obtengui'n|obtinga|obtinga'n|obtingui|obtingui'n VERB.IMP.P4=obtenguem|obtenguem-ne|obtinguem|obtinguem-ne VERB.IMP.P5=obteniu|obteniu-ne|obtingueu|obtingueu-ne VERB.IMP.P6=obtenguen|obtenguen-ne|obtenguin|obtenguin-ne|obtinguen|obtinguen-ne|obtinguin|obtinguin-ne"
                    );
            });
            grup.NouTest(@"Verbs filtrats", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC, new Marques(true, "102"),
                    "ent=dir1^cat1=v. tr. > VERB.INF=dir|dir-ne VERB.GER=dient|dient-ne VERB.PAR.M.SG=dit VERB.PAR.M.PL=dits VERB.PAR.F.SG=dita VERB.PAR.F.PL=dites VERB.IPR.P1=dic VERB.IPR.P2=dius VERB.IPR.P3=diu VERB.IPR.P4=diem VERB.IPR.P5=dieu VERB.IPR.P6=diuen VERB.IIM.P1=deia VERB.IIM.P2=deies VERB.IIM.P3=deia VERB.IIM.P4=d�iem VERB.IIM.P5=d�ieu VERB.IIM.P6=deien VERB.IPE.P1=digu� VERB.IPE.P2=digueres VERB.IPE.P3=digu� VERB.IPE.P4=digu�rem VERB.IPE.P5=digu�reu VERB.IPE.P6=digueren VERB.FUT.P1=dir� VERB.FUT.P2=dir�s VERB.FUT.P3=dir� VERB.FUT.P4=direm VERB.FUT.P5=direu VERB.FUT.P6=diran VERB.CON.P1=diria VERB.CON.P2=diries VERB.CON.P3=diria VERB.CON.P4=dir�em VERB.CON.P5=dir�eu VERB.CON.P6=dirien VERB.SPR.P1=diga|digui VERB.SPR.P2=digues|diguis VERB.SPR.P3=diga|digui VERB.SPR.P4=diguem VERB.SPR.P5=digueu VERB.SPR.P6=diguen|diguin VERB.SIM.P1=diguera|digu�s VERB.SIM.P2=digueres|diguesses|diguessis VERB.SIM.P3=diguera|digu�s VERB.SIM.P4=digu�rem|digu�ssem|digu�ssim VERB.SIM.P5=digu�reu|digu�sseu|digu�ssiu VERB.SIM.P6=digueren|diguessen|diguessin VERB.IMP.P2=digues|digues-ne VERB.IMP.P3=diga|diga'n|digui|digui'n VERB.IMP.P4=diguem|diguem-ne VERB.IMP.P5=digueu|digueu-ne VERB.IMP.P6=diguen|diguen-ne|diguin|diguin-ne"
                    );
                testParaules(resultat, idDIEC, new Marques(true, "001"),
                    "ent=donar^cat1=v. tr.^cat2=pron.^cat3=intr.^com1=ind. pr. 2 d�nes, 3 d�na > VERB.INF=donar|donar-ne VERB.GER=donant|donant-ne VERB.PAR.M.SG=donat VERB.PAR.M.PL=donats VERB.PAR.F.SG=donada VERB.PAR.F.PL=donades VERB.IPR.P1=don|done|dono VERB.IPR.P2=d�nes VERB.IPR.P3=d�na VERB.IPR.P4=donem VERB.IPR.P5=doneu VERB.IPR.P6=donen VERB.IIM.P1=donava VERB.IIM.P2=donaves VERB.IIM.P3=donava VERB.IIM.P4=don�vem VERB.IIM.P5=don�veu VERB.IIM.P6=donaven VERB.IPE.P1=don� VERB.IPE.P2=donares VERB.IPE.P3=don� VERB.IPE.P4=don�rem VERB.IPE.P5=don�reu VERB.IPE.P6=donaren VERB.FUT.P1=donar� VERB.FUT.P2=donar�s VERB.FUT.P3=donar� VERB.FUT.P4=donarem VERB.FUT.P5=donareu VERB.FUT.P6=donaran VERB.CON.P1=donaria VERB.CON.P2=donaries VERB.CON.P3=donaria VERB.CON.P4=donar�em VERB.CON.P5=donar�eu VERB.CON.P6=donarien VERB.SPR.P1=done|doni VERB.SPR.P2=d�nes|donis VERB.SPR.P3=done|doni VERB.SPR.P4=donem VERB.SPR.P5=doneu VERB.SPR.P6=donen|donin VERB.SIM.P1=donara|don�s|don�s VERB.SIM.P2=donares|donassis|donesses|donessis VERB.SIM.P3=donara|don�s|don�s VERB.SIM.P4=don�rem|don�ssim|don�ssem|don�ssim VERB.SIM.P5=don�reu|don�ssiu|don�sseu|don�ssiu VERB.SIM.P6=donaren|donassin|donessen|donessin VERB.IMP.P2=d�na|d�na'n VERB.IMP.P3=done|done'n|doni|doni'n VERB.IMP.P4=donem|donem-ne VERB.IMP.P5=doneu|doneu-ne VERB.IMP.P6=donen|donen-ne|donin|donin-ne", 
                    "ent=cantar^cat1=v. tr.^cat2=intr. > VERB.INF=cantar|cantar-ne VERB.GER=cantant|cantant-ne VERB.PAR.M.SG=cantat VERB.PAR.M.PL=cantats VERB.PAR.F.SG=cantada VERB.PAR.F.PL=cantades VERB.IPR.P1=cant|cante|canto VERB.IPR.P2=cantes VERB.IPR.P3=canta VERB.IPR.P4=cantem VERB.IPR.P5=canteu VERB.IPR.P6=canten VERB.IIM.P1=cantava VERB.IIM.P2=cantaves VERB.IIM.P3=cantava VERB.IIM.P4=cant�vem VERB.IIM.P5=cant�veu VERB.IIM.P6=cantaven VERB.IPE.P1=cant� VERB.IPE.P2=cantares VERB.IPE.P3=cant� VERB.IPE.P4=cant�rem VERB.IPE.P5=cant�reu VERB.IPE.P6=cantaren VERB.FUT.P1=cantar� VERB.FUT.P2=cantar�s VERB.FUT.P3=cantar� VERB.FUT.P4=cantarem VERB.FUT.P5=cantareu VERB.FUT.P6=cantaran VERB.CON.P1=cantaria VERB.CON.P2=cantaries VERB.CON.P3=cantaria VERB.CON.P4=cantar�em VERB.CON.P5=cantar�eu VERB.CON.P6=cantarien VERB.SPR.P1=cante|canti VERB.SPR.P2=cantes|cantis VERB.SPR.P3=cante|canti VERB.SPR.P4=cantem VERB.SPR.P5=canteu VERB.SPR.P6=canten|cantin VERB.SIM.P1=cantara|cant�s|cant�s VERB.SIM.P2=cantares|cantassis|cantesses|cantessis VERB.SIM.P3=cantara|cant�s|cant�s VERB.SIM.P4=cant�rem|cant�ssim|cant�ssem|cant�ssim VERB.SIM.P5=cant�reu|cant�ssiu|cant�sseu|cant�ssiu VERB.SIM.P6=cantaren|cantassin|cantessen|cantessin VERB.IMP.P2=canta|canta'n VERB.IMP.P3=cante|cante'n|canti|canti'n VERB.IMP.P4=cantem|cantem-ne VERB.IMP.P5=canteu|canteu-ne VERB.IMP.P6=canten|canten-ne|cantin|cantin-ne"
                    );
            });
        }


        private void testParaules(RTest resultat, Identificador idPar, params string[] paraules)
        {
            testParaules(resultat, idPar, Marques.totes, paraules);
        }

        private void testParaules(RTest resultat, Identificador idPar, Marques filtre, params string[] paraules)
        {
            List<Entrada> entrades = new List<Entrada>();
            List<string> notes = new List<string>();
            foreach (string paraula in paraules)
            {
                string infoParaula = paraula;
                string[] esperat = null;
                int posFletxa = paraula.IndexOf(" > ");
                if (posFletxa >= 0)
                {
                    infoParaula = paraula.Substring(0, posFletxa);
                    esperat = paraula.Substring(posFletxa + 3).Split(' ');
                }
                Entrada entrada = idPar.IdentificaEntrada(infoParaula);
                entrades.Add(entrada);
                string llista = Mot.LlistaPlana(entrada.GeneraMots(filtre, false), Cat.Cmp, true);
                bool ok = true;
                if (esperat != null)
                    foreach (string esp in esperat)
                        if (!(llista + " ").Contains(esp + " "))
                        {
                            notes.Add(string.Format("No s'ha generat {0} ({1})", esp, entrada.Arrel));
                            ok = false;
                        }
                resultat.Nota("{0}: {1}", entrada.Ent, ok ? "OK" : "ERROR");
            }
            if (notes.Count > 0)
            {
                resultat.Assert(false, "Hi ha {0} error{1}", notes.Count, notes.Count == 1 ? "" : "s");
                foreach (string nota in notes)
                    resultat.Nota(nota);
            }
        }

        private void test_07(GrupTest arrel)
        {
            GrupTest grup = arrel.NouGrup("Processament de fitxers");
            Regles regles = CarregaRegles(true);
            List<Entrada> entrades = new List<Entrada>();
            Identificador identificador = null;
            grup.NouTest(@"Llegeix DIEC", delegate(RTest resultat)
            {
                identificador = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
                identificador.LlegeixEntrades(DirEntrades("diec.txt"), entrades, 100);
                identificador = new IdentificadorDIEC("DIEC2", regles, DirEntrades("irregulars_diec2.txt"));
                identificador.LlegeixEntrades(DirEntrades("diec2.txt"), entrades, 1);
                identificador = new IdentificadorDIEC("m�s paraules", regles, null);
                identificador.LlegeixEntrades(DirEntrades("mes_paraules.txt"), entrades, 1);
                identificador = new IdentificadorToponims("top�nims", regles);
                identificador.LlegeixEntrades(DirEntrades("top�nims.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("top�nims_estrangers.txt"), entrades, 1);
                identificador = new IdentificadorGentilicis("gentilicis", regles, null);
                identificador.LlegeixEntrades(DirEntrades("gentilicis.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("gentilicis_estrangers.txt"), entrades, 1);
                identificador = new IdentificadorAntroponims("noms i llinatges", regles, null);
                identificador.LlegeixEntrades(DirEntrades("antrop�nims.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("llinatges.txt"), entrades, 1);
                identificador = new IdentificadorDiversos("diversos", regles);
                identificador.LlegeixEntrades(DirEntrades("abreviatures_duarte.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("marques.txt"), entrades, 1);
            });
            grup.NouTest(@"Genera .aff i .dic (Hunspell)", delegate(RTest resultat)
            {
                regles.GeneraAffDicHunspell(DirResultats("prova"), entrades, new Marques(true, "013"), Cat.Cmp);
                List<string> excSenseEmprar = identificador.ExcepcionsSenseEmprar();
                if (excSenseEmprar.Count != 0)
                    foreach (string exc in excSenseEmprar)
                        resultat.Error("No emprada: {0}", exc);
            });
            grup.NouTest(@"Genera .aff i .dic (Myspell, I)", delegate(RTest resultat)
            {
                Identificador id = new IdentificadorDIEC("DIEC", regles, null);
                List<Entrada> ee = new List<Entrada>();
                ee.Add(id.IdentificaEntrada("ent=al�ar^cat1=v. tr.^cat2=pron.^cat3=intr."));
                ee.Add(id.IdentificaEntrada("ent=cantar^cat1=v. tr.^cat2=intr."));
                ee.Add(id.IdentificaEntrada("ent=abellir^cat1=v. intr.^cat2=pron."));
                ee.Add(id.IdentificaEntrada("ent=adduir^cat1=v. tr."));
                ee.Add(id.IdentificaEntrada("ent=abatre^cat1=v. tr.^cat2=pron."));
                regles.GeneraAffDicHunspell(DirResultats("prova"), ee, Marques.totes, Cat.Cmp); 
                regles.GeneraAffDicMyspell(DirResultats("prova_myspell"), ee, Marques.totes, Cat.Cmp, 
                    IdentificadorCat.GetAfinaMyspell);
            });
            //grup.NouTest(@"Genera .aff i .dic (Myspell, II)", delegate(RTest resultat)
            //{
            //    regles.GeneraAffDic(DirResultats("prova_myspell"), entrades, new Marques(true, "013"), Entrada.Speller.MYSPELL, Cat.Cmp);
            //});
            //grup.NouTest(@"Genera les formes d'un diccionari", delegate(RTest resultat)
            //{
            //    List<string> formes = regles.GeneraFormes(DirResultats("prova.dic"), Marques.totes, false);
            //    resultat.Nota("S'han generat {0} formes", formes.Count);
            //});
        }

    }
}