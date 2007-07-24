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
                    resultat.Error("No s'ha impedit l'ús d'un flag il·legal");
                }
                catch (ItemDicException)
                {
                    resultat.Nota("S'ha impedit l'ús d'un flag il·legal");
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
                resultat.Assert(m1.Conte("005"), "Conté 005");
                resultat.Assert(!m1.Conte("000"), "No conté 000");
                resultat.Assert(m1.Conte(Marca.Una("001").Mascara), "Conté 001");
                resultat.Assert(!m1.Conte(Marca.Una("000").Mascara), "No conté 000");
                Marques m2 = new Marques(false, "101", "102", "103");
                resultat.Assert(m1.Conte(m2), "m1 >= m2");
                resultat.Assert(!m2.Conte(m1), "!(m2 >= m1)");
                resultat.Assert(m1 == m1bis, "Són iguals!");
                m1.Mes(Marca.Una("003"));
                resultat.Esperat("001, 002, 003, 005, 013, 014, 101, 102, 103", m1.ToString());
                m1.Menys(Marca.Una("013"));
                resultat.Esperat("001, 002, 003, 005, 014, 101, 102, 103", m1.ToString());
            });
            grup.NouTest("Línia amb condicions I", delegate(RTest resultat)
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
            grup.NouTest("Línia amb condicions II", delegate(RTest resultat)
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
                resultat.Esperat("aigües", Mot.LlistaPlana(mots, Cat.Cmp, false));
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
            GrupTest grup = arrel.NouGrup("Eines per al català");
            grup.NouTest("Comparacions simples", delegate(RTest resultat)
            {
                string[] mots = "a al alça alt cant canta cantà cantam cella cel·la celles cel·les".Split(" ".ToCharArray());
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
            grup.NouTest("Més comparacions", delegate(RTest resultat)
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
            grup.NouTest("Conversió a minúscules", delegate(RTest resultat)
            {
                string maj = "AÀBCÇDEÈÉFGHIÍÏJKLMNOÒÓPQRSTUÚÜVXYZaàbcçdeèéfghiíïjklmnoòópqrstuúüvxyz";
                string min = "aàbcçdeèéfghiíïjklmnoòópqrstuúüvxyzaàbcçdeèéfghiíïjklmnoòópqrstuúüvxyz";
                resultat.Esperat(min, Cat.Min(maj));
            });
            grup.NouTest("Divisió sil·làbica", delegate(RTest resultat)
            {
                string[] paraules = { "es/què/ieu", "hie/na", "ca/nya", "psi/cò/legs", "Àus/tri/a", "cui/nar", "xxx", "ha", "any", "i/ó", "i/ons", "i/o/nit/za/ci/ó", "" };
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
                string[] si = { "ara", "àrab", "humitat", "indi", "Henedina", "ARA", "ÀRAB", "HUMITAT", "INDI" };
                foreach (string forma in si)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(Paraula.TeVocalInicial(forma) && paraula.VocalInicial, String.Format("\"{0}\" comença per vocal", forma));
                }
                string[] no = { "casa", "hiena", "iode", "CASA", "HIENA", "IODE" };
                foreach (string forma in no)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!Paraula.TeVocalInicial(forma) && !paraula.VocalInicial, String.Format("\"{0}\" no comença per vocal", forma));
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
                string[] si = { "pa", "ratolí", "forat", "PA", "RATOLÍ", "FORAT" };
                foreach (string forma in si)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.Aguda, String.Format("\"{0}\" és aguda", forma));
                }
                string[] no = { "panera", "rata", "pàmpol", "mèrlera", "PANERA", "RATA", "PÀMPOL", "MÈRLERA" };
                foreach (string forma in no)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(!paraula.Aguda, String.Format("\"{0}\" no és aguda", forma));
                }
            });
            grup.NouTest("Síl·laba tònica", delegate(RTest resultat)
            {
                string[] paraules = { "aguiÀveu", "PA", "PÀMpol", "CAsa", "coLOR", "MÀquina" };
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
            grup.NouTest("Abans de la vocal tònica", delegate(RTest resultat)
            {
                string[] paraules = { "aguiÀVEU", "pA", "pÀMPOL", "cASA", "colOR", "mÀQUINA" };
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
                string[] noFemeninesSi = { "ara", "àrab", "indi", "unir" };
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
                string[] femeninesSi = { "índia", "alba", "hora", "ungla" };
                foreach (string forma in femeninesSi)
                {
                    Paraula paraula = new Paraula(forma);
                    resultat.Assert(paraula.PotApostrofar(true), String.Format("\"{0}\" es pot apostrofar", forma));
                }
                string[] femeninesNo = { "humitat", "casa", "il·lícita" };
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
                    resultat.Assert(Cat.EsMin(str), "És minúscula");
                foreach (string str in no)
                    resultat.Assert(!Cat.EsMin(str), "No és minúscula");
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
            grup.NouTest("Informació morfològica i gramatical", delegate(RTest resultat)
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
            grup.NouTest("Nom masculí, plural en -s, vocal inicial (àbac)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=àbac^cat1=m.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("àbac àbacs d'àbac d'àbacs l'àbac", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Nom masculí, plural en -s, consonant inicial (bosc)", delegate(RTest resultat)
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
            grup.NouTest("Nom masculí, plural en -s, consonant inicial (bacterial)", delegate(RTest resultat)
            {
                Entrada entrada = idDIEC.IdentificaEntrada("ent=bacterial^cat1=adj.");
                List<Mot> mots = entrada.GeneraMots(Marques.totes, false);
                resultat.Esperat("bacterial bacterials", Mot.LlistaPlana(mots, Cat.Cmp, false));
                foreach (Mot mot in mots)
                    resultat.Nota("\"{0}\": {1}", mot.Forma, mot.Info);
            });
            grup.NouTest("Nom masculí, plural en -os (cas)", delegate(RTest resultat)
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
            GrupTest grup = arrel.NouGrup("Generació de diccionaris");
            Regles regles = CarregaRegles(true);
            Identificador idDIEC = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            grup.NouTest(@"Noms", delegate(RTest resultat)
            {
                List<string> paraules = new List<string>();
                paraules.Add("ent=abietàcies^cat1=f. pl.");
                paraules.Add("ent=humitat^cat1=f. > d'humitat d'humitats humitat humitats");
                paraules.Add("ent=abacà^cat1=m.^com1=pl. -às");
                paraules.Add("ent=abadessa^cat1=f.");
                paraules.Add("ent=babutxa^cat1=f.");
                paraules.Add("ent=àbac^cat1=m.");
                paraules.Add("ent=bosc^cat1=m.");
                paraules.Add("ent=iarda^cat1=f.");
                paraules.Add("ent=camí^cat1=m.");
                paraules.Add("ent=cas1^cat1=m.");
                paraules.Add("ent=abaixallengües^cat1=m.");
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
                paraules.Add("ent=il·lustrat -ada^cat1=adj.^cat2=m. i f. > d'il·lustrada d'il·lustrades d'il·lustrat d'il·lustrats il·lustrada il·lustrades il·lustrat il·lustrats l'il·lustrat");
                paraules.Add("ent=abastador2 abastadora^cat1=m. i f. > abastador abastadora abastadores abastadors d'abastador d'abastadora d'abastadores d'abastadors l'abastador l'abastadora");
                paraules.Add("ent=feliç^cat1=adj. > feliç felices feliços");
                paraules.Add("ent=abacial^cat1=adj. > abacial abacials d'abacial d'abacials l'abacial");
                paraules.Add("ent=bacterial^cat1=adj. > bacterial bacterials");
                paraules.Add("ent=iacetà iacetana^cat1=m. i f. > iacetà iacetana iacetanes iacetans");
                paraules.Add("ent=ibèric -a^cat1=adj. > d'ibèric d'ibèrica d'ibèrics d'ibèriques ibèric ibèrica ibèrics ibèriques l'ibèric");
                paraules.Add("ent=suís -ïssa^cat1=adj. i m. i f.^cat2=adj.^cat3=m. > suís suïssa suïsses suïssos");
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
                    "ent=vèncer^cat1=v. tr.^cat2=intr. > VERB.PAR.M.SG=vençut",
                    "ent=retre^cat1=v. tr.^cat2=pron. > VERB.PAR.M.SG=retut",
                    "ent=rebre^cat1=v. tr. > VERB.PAR.M.SG=rebut VERB.SIM.P3=rebera|rebés"
                    );
            });
            grup.NouTest(@"Verbs en -er/re II", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=absoldre^cat1=v. tr.^com1=ger. absolent; p. p. absolt; ind. pr. 1 absolc; subj. pr. absolgui, etc.; subj. imperf. absolgués, etc. > VERB.INF=absoldre|absoldre'n|d'absoldre|d'absoldre'n VERB.GER=absolent|absolent-ne VERB.PAR.M.SG=absolt|d'absolt|l'absolt VERB.PAR.M.PL=absolts|d'absolts VERB.PAR.F.SG=absolta|d'absolta|l'absolta VERB.PAR.F.PL=absoltes|d'absoltes VERB.IPR.P1=absolc|n'absolc VERB.IPR.P2=absols|n'absols VERB.IPR.P3=absol|n'absol VERB.IPR.P4=absolem|n'absolem VERB.IPR.P5=absoleu|n'absoleu VERB.IPR.P6=absolen|n'absolen VERB.IIM.P1=absolia|n'absolia VERB.IIM.P2=absolies|n'absolies VERB.IIM.P3=absolia|n'absolia VERB.IIM.P4=absolíem|n'absolíem VERB.IIM.P5=absolíeu|n'absolíeu VERB.IIM.P6=absolien|n'absolien VERB.IPE.P1=absolguí|n'absolguí VERB.IPE.P2=absolgueres|n'absolgueres VERB.IPE.P3=absolgué|n'absolgué VERB.IPE.P4=absolguérem|n'absolguérem VERB.IPE.P5=absolguéreu|n'absolguéreu VERB.IPE.P6=absolgueren|n'absolgueren VERB.FUT.P1=absoldré|n'absoldré VERB.FUT.P2=absoldràs|n'absoldràs VERB.FUT.P3=absoldrà|n'absoldrà VERB.FUT.P4=absoldrem|n'absoldrem VERB.FUT.P5=absoldreu|n'absoldreu VERB.FUT.P6=absoldran|n'absoldran VERB.CON.P1=absoldria|n'absoldria VERB.CON.P2=absoldries|n'absoldries VERB.CON.P3=absoldria|n'absoldria VERB.CON.P4=absoldríem|n'absoldríem VERB.CON.P5=absoldríeu|n'absoldríeu VERB.CON.P6=absoldrien|n'absoldrien VERB.SPR.P1=absolga|absolgui|n'absolga|n'absolgui VERB.SPR.P2=absolgues|absolguis|n'absolgues|n'absolguis VERB.SPR.P3=absolga|absolgui|n'absolga|n'absolgui VERB.SPR.P4=absolguem|n'absolguem VERB.SPR.P5=absolgueu|n'absolgueu VERB.SPR.P6=absolguen|absolguin|n'absolguen|n'absolguin VERB.SIM.P1=absolguera|absolgués|n'absolguera|n'absolgués VERB.SIM.P2=absolgueres|absolguesses|absolguessis|n'absolgueres|n'absolguesses|n'absolguessis VERB.SIM.P3=absolguera|absolgués|n'absolguera|n'absolgués VERB.SIM.P4=absolguérem|absolguéssem|absolguéssim|n'absolguérem|n'absolguéssem|n'absolguéssim VERB.SIM.P5=absolguéreu|absolguésseu|absolguéssiu|n'absolguéreu|n'absolguésseu|n'absolguéssiu VERB.SIM.P6=absolgueren|absolguessen|absolguessin|n'absolgueren|n'absolguessen|n'absolguessin VERB.IMP.P2=absol|absol-ne VERB.IMP.P3=absolga|absolga'n|absolgui|absolgui'n VERB.IMP.P4=absolguem|absolguem-ne VERB.IMP.P5=absoleu|absoleu-ne VERB.IMP.P6=absolguen|absolguen-ne|absolguin|absolguin-ne",
                    "ent=aprendre^cat1=v. tr.^cat2=tr. pron.^com1=ger. aprenent; p. p. après; ind. pr. 1 aprenc, 3 aprèn; subj. pr. aprengui, etc.; subj. imperf. aprengués, etc. > VERB.INF=aprendre|aprendre'n|d'aprendre|d'aprendre'n VERB.GER=aprenent|aprenent-ne VERB.PAR.M.SG=après|d'après|l'après VERB.PAR.M.PL=apresos|d'apresos VERB.PAR.F.SG=apresa|d'apresa|l'apresa VERB.PAR.F.PL=apreses|d'apreses VERB.IPR.P1=aprenc|n'aprenc VERB.IPR.P2=aprens|n'aprens VERB.IPR.P3=aprèn|n'aprèn VERB.IPR.P4=aprenem|n'aprenem VERB.IPR.P5=apreneu|n'apreneu VERB.IPR.P6=aprenen|n'aprenen VERB.IIM.P1=aprenia|n'aprenia VERB.IIM.P2=aprenies|n'aprenies VERB.IIM.P3=aprenia|n'aprenia VERB.IIM.P4=apreníem|n'apreníem VERB.IIM.P5=apreníeu|n'apreníeu VERB.IIM.P6=aprenien|n'aprenien VERB.IPE.P1=aprenguí|n'aprenguí VERB.IPE.P2=aprengueres|n'aprengueres VERB.IPE.P3=aprengué|n'aprengué VERB.IPE.P4=aprenguérem|n'aprenguérem VERB.IPE.P5=aprenguéreu|n'aprenguéreu VERB.IPE.P6=aprengueren|n'aprengueren VERB.FUT.P1=aprendré|n'aprendré VERB.FUT.P2=aprendràs|n'aprendràs VERB.FUT.P3=aprendrà|n'aprendrà VERB.FUT.P4=aprendrem|n'aprendrem VERB.FUT.P5=aprendreu|n'aprendreu VERB.FUT.P6=aprendran|n'aprendran VERB.CON.P1=aprendria|n'aprendria VERB.CON.P2=aprendries|n'aprendries VERB.CON.P3=aprendria|n'aprendria VERB.CON.P4=aprendríem|n'aprendríem VERB.CON.P5=aprendríeu|n'aprendríeu VERB.CON.P6=aprendrien|n'aprendrien VERB.SPR.P1=aprenga|aprengui|n'aprenga|n'aprengui VERB.SPR.P2=aprengues|aprenguis|n'aprengues|n'aprenguis VERB.SPR.P3=aprenga|aprengui|n'aprenga|n'aprengui VERB.SPR.P4=aprenguem|n'aprenguem VERB.SPR.P5=aprengueu|n'aprengueu VERB.SPR.P6=aprenguen|aprenguin|n'aprenguen|n'aprenguin VERB.SIM.P1=aprenguera|aprengués|n'aprenguera|n'aprengués VERB.SIM.P2=aprengueres|aprenguesses|aprenguessis|n'aprengueres|n'aprenguesses|n'aprenguessis VERB.SIM.P3=aprenguera|aprengués|n'aprenguera|n'aprengués VERB.SIM.P4=aprenguérem|aprenguéssem|aprenguéssim|n'aprenguérem|n'aprenguéssem|n'aprenguéssim VERB.SIM.P5=aprenguéreu|aprenguésseu|aprenguéssiu|n'aprenguéreu|n'aprenguésseu|n'aprenguéssiu VERB.SIM.P6=aprengueren|aprenguessen|aprenguessin|n'aprengueren|n'aprenguessen|n'aprenguessin VERB.IMP.P2=aprèn|aprèn-ne VERB.IMP.P3=aprenga|aprenga'n|aprengui|aprengui'n VERB.IMP.P4=aprenguem|aprenguem-ne VERB.IMP.P5=apreneu|apreneu-ne VERB.IMP.P6=aprenguen|aprenguen-ne|aprenguin|aprenguin-ne",
                    "ent=atènyer^cat1=v. tr.^cat2=intr.^com1=p. p. atès > VERB.INF=atènyer|atènyer-ne|d'atènyer|d'atènyer-ne VERB.GER=atenyent|atenyent-ne VERB.PAR.M.SG=atès|d'atès|l'atès VERB.PAR.M.PL=atesos|d'atesos VERB.PAR.F.SG=atesa|d'atesa|l'atesa VERB.PAR.F.PL=ateses|d'ateses VERB.IPR.P1=ateny|atenyo|n'ateny|n'atenyo VERB.IPR.P2=atenys|n'atenys VERB.IPR.P3=ateny|n'ateny VERB.IPR.P4=atenyem|n'atenyem VERB.IPR.P5=atenyeu|n'atenyeu VERB.IPR.P6=atenyen|n'atenyen VERB.IIM.P1=atenyia|n'atenyia VERB.IIM.P2=atenyies|n'atenyies VERB.IIM.P3=atenyia|n'atenyia VERB.IIM.P4=atenyíem|n'atenyíem VERB.IIM.P5=atenyíeu|n'atenyíeu VERB.IIM.P6=atenyien|n'atenyien VERB.IPE.P1=atenyí|n'atenyí VERB.IPE.P2=atenyeres|n'atenyeres VERB.IPE.P3=atenyé|n'atenyé VERB.IPE.P4=atenyérem|n'atenyérem VERB.IPE.P5=atenyéreu|n'atenyéreu VERB.IPE.P6=atenyeren|n'atenyeren VERB.FUT.P1=atenyeré|n'atenyeré VERB.FUT.P2=atenyeràs|n'atenyeràs VERB.FUT.P3=atenyerà|n'atenyerà VERB.FUT.P4=atenyerem|n'atenyerem VERB.FUT.P5=atenyereu|n'atenyereu VERB.FUT.P6=atenyeran|n'atenyeran VERB.CON.P1=atenyeria|n'atenyeria VERB.CON.P2=atenyeries|n'atenyeries VERB.CON.P3=atenyeria|n'atenyeria VERB.CON.P4=atenyeríem|n'atenyeríem VERB.CON.P5=atenyeríeu|n'atenyeríeu VERB.CON.P6=atenyerien|n'atenyerien VERB.SPR.P1=atenya|atenyi|n'atenya|n'atenyi VERB.SPR.P2=atenyes|atenyis|n'atenyes|n'atenyis VERB.SPR.P3=atenya|atenyi|n'atenya|n'atenyi VERB.SPR.P4=atenyem|n'atenyem VERB.SPR.P5=atenyeu|n'atenyeu VERB.SPR.P6=atenyen|atenyin|n'atenyen|n'atenyin VERB.SIM.P1=atenyera|atenyés|n'atenyera|n'atenyés VERB.SIM.P2=atenyeres|atenyesses|atenyessis|n'atenyeres|n'atenyesses|n'atenyessis VERB.SIM.P3=atenyera|atenyés|n'atenyera|n'atenyés VERB.SIM.P4=atenyérem|atenyéssem|atenyéssim|n'atenyérem|n'atenyéssem|n'atenyéssim VERB.SIM.P5=atenyéreu|atenyésseu|atenyéssiu|n'atenyéreu|n'atenyésseu|n'atenyéssiu VERB.SIM.P6=atenyeren|atenyessen|atenyessin|n'atenyeren|n'atenyessen|n'atenyessin VERB.IMP.P2=ateny|ateny-ne VERB.IMP.P3=atenya|atenya'n|atenyi|atenyi'n VERB.IMP.P4=atenyem|atenyem-ne VERB.IMP.P5=atenyeu|atenyeu-ne VERB.IMP.P6=atenyen|atenyen-ne|atenyin|atenyin-ne",
                    "ent=beure1^cat1=v. tr.^cat2=tr. pron.^cat3=pron.^com1=ger. bevent; p. p. begut; ind. pr. bec, beus, beu, bevem, beveu, beuen; subj. pr. begui, etc.; subj. imperf. begués, etc.^com2=o beure a morro, o beure a broc d'ampolla^com3=o beure a raig^com4=o beure's l'enteniment^com5=o beure's amb la mirada > VERB.INF=beure|beure'n VERB.GER=bevent|bevent-ne VERB.PAR.M.SG=begut VERB.PAR.M.PL=beguts VERB.PAR.F.SG=beguda VERB.PAR.F.PL=begudes VERB.IPR.P1=bec VERB.IPR.P2=beus VERB.IPR.P3=beu VERB.IPR.P4=bevem VERB.IPR.P5=beveu VERB.IPR.P6=beuen VERB.IIM.P1=bevia VERB.IIM.P2=bevies VERB.IIM.P3=bevia VERB.IIM.P4=bevíem VERB.IIM.P5=bevíeu VERB.IIM.P6=bevien VERB.IPE.P1=beguí VERB.IPE.P2=begueres VERB.IPE.P3=begué VERB.IPE.P4=beguérem VERB.IPE.P5=beguéreu VERB.IPE.P6=begueren VERB.FUT.P1=beuré VERB.FUT.P2=beuràs VERB.FUT.P3=beurà VERB.FUT.P4=beurem VERB.FUT.P5=beureu VERB.FUT.P6=beuran VERB.CON.P1=beuria VERB.CON.P2=beuries VERB.CON.P3=beuria VERB.CON.P4=beuríem VERB.CON.P5=beuríeu VERB.CON.P6=beurien VERB.SPR.P1=bega|begui VERB.SPR.P2=begues|beguis VERB.SPR.P3=bega|begui VERB.SPR.P4=beguem VERB.SPR.P5=begueu VERB.SPR.P6=beguen|beguin VERB.SIM.P1=beguera|begués VERB.SIM.P2=begueres|beguesses|beguessis VERB.SIM.P3=beguera|begués VERB.SIM.P4=beguérem|beguéssem|beguéssim VERB.SIM.P5=beguéreu|beguésseu|beguéssiu VERB.SIM.P6=begueren|beguessen|beguessin VERB.IMP.P2=beu|beu-ne VERB.IMP.P3=bega|bega'n|begui|begui'n VERB.IMP.P4=beguem|beguem-ne VERB.IMP.P5=beveu|beveu-ne VERB.IMP.P6=beguen|beguen-ne|beguin|beguin-ne",
                    "ent=caure^cat1=v. intr.^com1=ger. caient; p. p. caigut; ind. pr. caic, caus, cau, caiem, caieu, cauen; ind. imperf. queia, etc.; subj. pr. caigui, etc.; subj. imperf. caigués, etc.^com2=o estar si cau no cau^com3=o caure una cosa del cel^com4=o caure del candeler^com5=o caure sota la seva mà > VERB.INF=caure|caure'n VERB.GER=caient|caient-ne VERB.PAR.M.SG=caigut VERB.PAR.M.PL=caiguts VERB.PAR.F.SG=caiguda VERB.PAR.F.PL=caigudes VERB.IPR.P1=caic VERB.IPR.P2=caus VERB.IPR.P3=cau VERB.IPR.P4=caiem VERB.IPR.P5=caieu VERB.IPR.P6=cauen VERB.IIM.P1=queia VERB.IIM.P2=queies VERB.IIM.P3=queia VERB.IIM.P4=quèiem VERB.IIM.P5=quèieu VERB.IIM.P6=queien VERB.IPE.P1=caiguí VERB.IPE.P2=caigueres VERB.IPE.P3=caigué VERB.IPE.P4=caiguérem VERB.IPE.P5=caiguéreu VERB.IPE.P6=caigueren VERB.FUT.P1=cauré VERB.FUT.P2=cauràs VERB.FUT.P3=caurà VERB.FUT.P4=caurem VERB.FUT.P5=caureu VERB.FUT.P6=cauran VERB.CON.P1=cauria VERB.CON.P2=cauries VERB.CON.P3=cauria VERB.CON.P4=cauríem VERB.CON.P5=cauríeu VERB.CON.P6=caurien VERB.SPR.P1=caiga|caigui VERB.SPR.P2=caigues|caiguis VERB.SPR.P3=caiga|caigui VERB.SPR.P4=caiguem VERB.SPR.P5=caigueu VERB.SPR.P6=caiguen|caiguin VERB.SIM.P1=caiguera|caigués VERB.SIM.P2=caigueres|caiguesses|caiguessis VERB.SIM.P3=caiguera|caigués VERB.SIM.P4=caiguérem|caiguéssem|caiguéssim VERB.SIM.P5=caiguéreu|caiguésseu|caiguéssiu VERB.SIM.P6=caigueren|caiguessen|caiguessin VERB.IMP.P2=cau|cau-ne VERB.IMP.P3=caiga|caiga'n|caigui|caigui'n VERB.IMP.P4=caiguem|caiguem-ne VERB.IMP.P5=caieu|caieu-ne VERB.IMP.P6=caiguen|caiguen-ne|caiguin|caiguin-ne",
                    "ent=confondre^cat1=pron.^cat2=v. tr.^com1=ger. confonent; p. p. confós; ind. pr. 1 confonc; subj. pr. confongui, etc.; subj. imperf. confongués, etc. > VERB.INF=confondre|confondre'n VERB.GER=confonent|confonent-ne VERB.PAR.M.SG=confós VERB.PAR.M.PL=confosos VERB.PAR.F.SG=confosa VERB.PAR.F.PL=confoses VERB.IPR.P1=confonc VERB.IPR.P2=confons VERB.IPR.P3=confon VERB.IPR.P4=confonem VERB.IPR.P5=confoneu VERB.IPR.P6=confonen VERB.IIM.P1=confonia VERB.IIM.P2=confonies VERB.IIM.P3=confonia VERB.IIM.P4=confoníem VERB.IIM.P5=confoníeu VERB.IIM.P6=confonien VERB.IPE.P1=confonguí VERB.IPE.P2=confongueres VERB.IPE.P3=confongué VERB.IPE.P4=confonguérem VERB.IPE.P5=confonguéreu VERB.IPE.P6=confongueren VERB.FUT.P1=confondré VERB.FUT.P2=confondràs VERB.FUT.P3=confondrà VERB.FUT.P4=confondrem VERB.FUT.P5=confondreu VERB.FUT.P6=confondran VERB.CON.P1=confondria VERB.CON.P2=confondries VERB.CON.P3=confondria VERB.CON.P4=confondríem VERB.CON.P5=confondríeu VERB.CON.P6=confondrien VERB.SPR.P1=confonga|confongui VERB.SPR.P2=confongues|confonguis VERB.SPR.P3=confonga|confongui VERB.SPR.P4=confonguem VERB.SPR.P5=confongueu VERB.SPR.P6=confonguen|confonguin VERB.SIM.P1=confonguera|confongués VERB.SIM.P2=confongueres|confonguesses|confonguessis VERB.SIM.P3=confonguera|confongués VERB.SIM.P4=confonguérem|confonguéssem|confonguéssim VERB.SIM.P5=confonguéreu|confonguésseu|confonguéssiu VERB.SIM.P6=confongueren|confonguessen|confonguessin VERB.IMP.P2=confon|confon-ne VERB.IMP.P3=confonga|confonga'n|confongui|confongui'n VERB.IMP.P4=confonguem|confonguem-ne VERB.IMP.P5=confoneu|confoneu-ne VERB.IMP.P6=confonguen|confonguen-ne|confonguin|confonguin-ne"
                    );
            });
            grup.NouTest(@"Verbs en -ir, incoatius", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=enaltir^cat1=v. tr. > VERB.INF=d'enaltir|d'enaltir-ne|enaltir|enaltir-ne VERB.GER=enaltint|enaltint-ne VERB.PAR.M.SG=d'enaltit|enaltit|l'enaltit VERB.PAR.M.PL=d'enaltits|enaltits VERB.PAR.F.SG=d'enaltida|enaltida|l'enaltida VERB.PAR.F.PL=d'enaltides|enaltides VERB.IPR.P1=enalteixo|enaltesc|enaltisc|n'enalteixo|n'enaltesc|n'enaltisc VERB.IPR.P2=enalteixes|enaltixes|n'enalteixes|n'enaltixes VERB.IPR.P3=enalteix|enaltix|n'enalteix|n'enaltix VERB.IPR.P4=enaltim|n'enaltim VERB.IPR.P5=enaltiu|n'enaltiu VERB.IPR.P6=enalteixen|enaltixen|n'enalteixen|n'enaltixen VERB.IIM.P1=enaltia|n'enaltia VERB.IIM.P2=enalties|n'enalties VERB.IIM.P3=enaltia|n'enaltia VERB.IIM.P4=enaltíem|n'enaltíem VERB.IIM.P5=enaltíeu|n'enaltíeu VERB.IIM.P6=enaltien|n'enaltien VERB.IPE.P1=enaltí|n'enaltí VERB.IPE.P2=enaltires|n'enaltires VERB.IPE.P3=enaltí|n'enaltí VERB.IPE.P4=enaltírem|n'enaltírem VERB.IPE.P5=enaltíreu|n'enaltíreu VERB.IPE.P6=enaltiren|n'enaltiren VERB.FUT.P1=enaltiré|n'enaltiré VERB.FUT.P2=enaltiràs|n'enaltiràs VERB.FUT.P3=enaltirà|n'enaltirà VERB.FUT.P4=enaltirem|n'enaltirem VERB.FUT.P5=enaltireu|n'enaltireu VERB.FUT.P6=enaltiran|n'enaltiran VERB.CON.P1=enaltiria|n'enaltiria VERB.CON.P2=enaltiries|n'enaltiries VERB.CON.P3=enaltiria|n'enaltiria VERB.CON.P4=enaltiríem|n'enaltiríem VERB.CON.P5=enaltiríeu|n'enaltiríeu VERB.CON.P6=enaltirien|n'enaltirien VERB.SPR.P1=enalteixi|enaltesqui|enaltisca|n'enalteixi|n'enaltesqui|n'enaltisca VERB.SPR.P2=enalteixis|enaltesquis|enaltisques|n'enalteixis|n'enaltesquis|n'enaltisques VERB.SPR.P3=enalteixi|enaltesqui|enaltisca|n'enalteixi|n'enaltesqui|n'enaltisca VERB.SPR.P4=enaltim|n'enaltim VERB.SPR.P5=enaltiu|n'enaltiu VERB.SPR.P6=enalteixin|enaltesquin|enaltisquen|n'enalteixin|n'enaltesquin|n'enaltisquen VERB.SIM.P1=enaltira|enaltís|n'enaltira|n'enaltís VERB.SIM.P2=enaltires|enaltisses|enaltissis|n'enaltires|n'enaltisses|n'enaltissis VERB.SIM.P3=enaltira|enaltís|n'enaltira|n'enaltís VERB.SIM.P4=enaltírem|enaltíssem|enaltíssim|n'enaltírem|n'enaltíssem|n'enaltíssim VERB.SIM.P5=enaltíreu|enaltísseu|enaltíssiu|n'enaltíreu|n'enaltísseu|n'enaltíssiu VERB.SIM.P6=enaltiren|enaltissen|enaltissin|n'enaltiren|n'enaltissen|n'enaltissin VERB.IMP.P2=enalteix|enalteix-ne|enaltix|enaltix-ne VERB.IMP.P3=enalteixi|enalteixi'n|enaltesqui|enaltesqui'n|enaltisca|enaltisca'n VERB.IMP.P4=enaltim|enaltim-ne VERB.IMP.P5=enaltiu|enaltiu-ne VERB.IMP.P6=enalteixin|enalteixin-ne|enaltesquin|enaltesquin-ne|enaltisquen|enaltisquen-ne",
                    "ent=partir^cat1=v. tr.^cat2=pron.^cat3=intr. > VERB.INF=partir|partir-ne VERB.GER=partint|partint-ne VERB.PAR.M.SG=partit VERB.PAR.M.PL=partits VERB.PAR.F.SG=partida VERB.PAR.F.PL=partides VERB.IPR.P1=parteixo|partesc|partisc VERB.IPR.P2=parteixes|partixes VERB.IPR.P3=parteix|partix VERB.IPR.P4=partim VERB.IPR.P5=partiu VERB.IPR.P6=parteixen|partixen VERB.IIM.P1=partia VERB.IIM.P2=parties VERB.IIM.P3=partia VERB.IIM.P4=partíem VERB.IIM.P5=partíeu VERB.IIM.P6=partien VERB.IPE.P1=partí VERB.IPE.P2=partires VERB.IPE.P3=partí VERB.IPE.P4=partírem VERB.IPE.P5=partíreu VERB.IPE.P6=partiren VERB.FUT.P1=partiré VERB.FUT.P2=partiràs VERB.FUT.P3=partirà VERB.FUT.P4=partirem VERB.FUT.P5=partireu VERB.FUT.P6=partiran VERB.CON.P1=partiria VERB.CON.P2=partiries VERB.CON.P3=partiria VERB.CON.P4=partiríem VERB.CON.P5=partiríeu VERB.CON.P6=partirien VERB.SPR.P1=parteixi|partesqui|partisca VERB.SPR.P2=parteixis|partesquis|partisques VERB.SPR.P3=parteixi|partesqui|partisca VERB.SPR.P4=partim VERB.SPR.P5=partiu VERB.SPR.P6=parteixin|partesquin|partisquen VERB.SIM.P1=partira|partís VERB.SIM.P2=partires|partisses|partissis VERB.SIM.P3=partira|partís VERB.SIM.P4=partírem|partíssem|partíssim VERB.SIM.P5=partíreu|partísseu|partíssiu VERB.SIM.P6=partiren|partissen|partissin VERB.IMP.P2=parteix|parteix-ne|partix|partix-ne VERB.IMP.P3=parteixi|parteixi'n|partesqui|partesqui'n|partisca|partisca'n VERB.IMP.P4=partim|partim-ne VERB.IMP.P5=partiu|partiu-ne VERB.IMP.P6=parteixin|parteixin-ne|partesquin|partesquin-ne|partisquen|partisquen-ne",
                    "ent=patir^cat1=v. tr.^cat2=intr. > VERB.INF=patir|patir-ne VERB.GER=patint|patint-ne VERB.PAR.M.SG=patit VERB.PAR.M.PL=patits VERB.PAR.F.SG=patida VERB.PAR.F.PL=patides VERB.IPR.P1=pateixo|patesc|patisc VERB.IPR.P2=pateixes|patixes VERB.IPR.P3=pateix|patix VERB.IPR.P4=patim VERB.IPR.P5=patiu VERB.IPR.P6=pateixen|patixen VERB.IIM.P1=patia VERB.IIM.P2=paties VERB.IIM.P3=patia VERB.IIM.P4=patíem VERB.IIM.P5=patíeu VERB.IIM.P6=patien VERB.IPE.P1=patí VERB.IPE.P2=patires VERB.IPE.P3=patí VERB.IPE.P4=patírem VERB.IPE.P5=patíreu VERB.IPE.P6=patiren VERB.FUT.P1=patiré VERB.FUT.P2=patiràs VERB.FUT.P3=patirà VERB.FUT.P4=patirem VERB.FUT.P5=patireu VERB.FUT.P6=patiran VERB.CON.P1=patiria VERB.CON.P2=patiries VERB.CON.P3=patiria VERB.CON.P4=patiríem VERB.CON.P5=patiríeu VERB.CON.P6=patirien VERB.SPR.P1=pateixi|patesqui|patisca VERB.SPR.P2=pateixis|patesquis|patisques VERB.SPR.P3=pateixi|patesqui|patisca VERB.SPR.P4=patim VERB.SPR.P5=patiu VERB.SPR.P6=pateixin|patesquin|patisquen VERB.SIM.P1=patira|patís VERB.SIM.P2=patires|patisses|patissis VERB.SIM.P3=patira|patís VERB.SIM.P4=patírem|patíssem|patíssim VERB.SIM.P5=patíreu|patísseu|patíssiu VERB.SIM.P6=patiren|patissen|patissin VERB.IMP.P2=pateix|pateix-ne|patix|patix-ne VERB.IMP.P3=pateixi|pateixi'n|patesqui|patesqui'n|patisca|patisca'n VERB.IMP.P4=patim|patim-ne VERB.IMP.P5=patiu|patiu-ne VERB.IMP.P6=pateixin|pateixin-ne|patesquin|patesquin-ne|patisquen|patisquen-ne"
                    );
            });
            grup.NouTest(@"Verbs en -ir, purs", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=munyir^cat1=v. tr.^com1=ind. pres. 3 muny",
                    "ent=collir^cat1=v. tr.^com1=ind. pr. cullo, culls, cull, collim, colliu, cullen; subj. pr. culli, cullis, culli, collim, colliu, cullin > VERB.INF=collir|collir-ne VERB.GER=collint|collint-ne VERB.PAR.M.SG=collit VERB.PAR.M.PL=collits VERB.PAR.F.SG=collida VERB.PAR.F.PL=collides VERB.IPR.P1=cull|cullo VERB.IPR.P2=culls VERB.IPR.P3=cull VERB.IPR.P4=collim VERB.IPR.P5=colliu VERB.IPR.P6=cullen VERB.IIM.P1=collia VERB.IIM.P2=collies VERB.IIM.P3=collia VERB.IIM.P4=collíem VERB.IIM.P5=collíeu VERB.IIM.P6=collien VERB.IPE.P1=collí VERB.IPE.P2=collires VERB.IPE.P3=collí VERB.IPE.P4=collírem VERB.IPE.P5=collíreu VERB.IPE.P6=colliren VERB.FUT.P1=colliré VERB.FUT.P2=colliràs VERB.FUT.P3=collirà VERB.FUT.P4=collirem VERB.FUT.P5=collireu VERB.FUT.P6=colliran VERB.CON.P1=colliria VERB.CON.P2=colliries VERB.CON.P3=colliria VERB.CON.P4=colliríem VERB.CON.P5=colliríeu VERB.CON.P6=collirien VERB.SPR.P1=culla|culli VERB.SPR.P2=culles|cullis VERB.SPR.P3=culla|culli VERB.SPR.P4=collim VERB.SPR.P5=colliu VERB.SPR.P6=cullen|cullin VERB.SIM.P1=collira|collís VERB.SIM.P2=collires|collisses|collissis VERB.SIM.P3=collira|collís VERB.SIM.P4=collírem|collíssem|collíssim VERB.SIM.P5=collíreu|collísseu|collíssiu VERB.SIM.P6=colliren|collissen|collissin VERB.IMP.P2=cull|cull-ne VERB.IMP.P3=culla|culla'n|culli|culli'n VERB.IMP.P4=collim|collim-ne VERB.IMP.P5=colliu|colliu-ne VERB.IMP.P6=cullen|cullen-ne|cullin|cullin-ne",
                    "ent=acollir^cat1=v. tr.^cat2=pron.^com1=quant a la flexió, com collir > VERB.INF=acollir|acollir-ne|d'acollir|d'acollir-ne VERB.GER=acollint|acollint-ne VERB.PAR.M.SG=acollit|d'acollit|l'acollit VERB.PAR.M.PL=acollits|d'acollits VERB.PAR.F.SG=acollida|d'acollida|l'acollida VERB.PAR.F.PL=acollides|d'acollides VERB.IPR.P1=acull|acullo|n'acull|n'acullo VERB.IPR.P2=aculls|n'aculls VERB.IPR.P3=acull VERB.IPR.P4=acollim|n'acollim VERB.IPR.P5=acolliu|n'acolliu VERB.IPR.P6=acullen|n'acullen VERB.IIM.P1=acollia|n'acollia VERB.IIM.P2=acollies|n'acollies VERB.IIM.P3=acollia|n'acollia VERB.IIM.P4=acollíem|n'acollíem VERB.IIM.P5=acollíeu|n'acollíeu VERB.IIM.P6=acollien|n'acollien VERB.IPE.P1=acollí|n'acollí VERB.IPE.P2=acollires|n'acollires VERB.IPE.P3=acollí|n'acollí VERB.IPE.P4=acollírem|n'acollírem VERB.IPE.P5=acollíreu|n'acollíreu VERB.IPE.P6=acolliren|n'acolliren VERB.FUT.P1=acolliré|n'acolliré VERB.FUT.P2=acolliràs|n'acolliràs VERB.FUT.P3=acollirà|n'acollirà VERB.FUT.P4=acollirem|n'acollirem VERB.FUT.P5=acollireu|n'acollireu VERB.FUT.P6=acolliran|n'acolliran VERB.CON.P1=acolliria|n'acolliria VERB.CON.P2=acolliries|n'acolliries VERB.CON.P3=acolliria|n'acolliria VERB.CON.P4=acolliríem|n'acolliríem VERB.CON.P5=acolliríeu|n'acolliríeu VERB.CON.P6=acollirien|n'acollirien VERB.SPR.P1=aculla|aculli|n'aculla|n'aculli VERB.SPR.P2=aculles|acullis|n'aculles|n'acullis VERB.SPR.P3=aculla|aculli|n'aculla|n'aculli VERB.SPR.P4=acollim|n'acollim VERB.SPR.P5=acolliu|n'acolliu VERB.SPR.P6=acullen|acullin|n'acullen|n'acullin VERB.SIM.P1=acollira|acollís|n'acollira|n'acollís VERB.SIM.P2=acollires|acollisses|acollissis|n'acollires|n'acollisses|n'acollissis VERB.SIM.P3=acollira|acollís|n'acollira|n'acollís VERB.SIM.P4=acollírem|acollíssem|acollíssim|n'acollírem|n'acollíssem|n'acollíssim VERB.SIM.P5=acollíreu|acollísseu|acollíssiu|n'acollíreu|n'acollísseu|n'acollíssiu VERB.SIM.P6=acolliren|acollissen|acollissin|n'acolliren|n'acollissen|n'acollissin VERB.IMP.P2=acull|acull-ne VERB.IMP.P3=aculla|aculla'n|aculli|aculli'n VERB.IMP.P4=acollim|acollim-ne VERB.IMP.P5=acolliu|acolliu-ne VERB.IMP.P6=acullen|acullen-ne|acullin|acullin-ne"
                    );
            });
            grup.NouTest(@"Més verbs irregulars", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC,
                    "ent=anar^cat1=v. intr.^com1=ind. pr. vaig, vas, va, anem, aneu, van; ind. fut. aniré o iré, etc.; subj. pr. 1 vagi, 2 vagis, 3 vagi, 6 vagin; imper. 2 vés > VERB.INF=anar|anar-ne|d'anar|d'anar-ne VERB.GER=anant|anant-ne VERB.PAR.M.SG=anat|d'anat|l'anat VERB.PAR.M.PL=anats|d'anats VERB.PAR.F.SG=anada|d'anada|l'anada VERB.PAR.F.PL=anades|d'anades VERB.IPR.P1=vaig VERB.IPR.P2=vas VERB.IPR.P3=va VERB.IPR.P4=anam|anem|n'anam|n'anem VERB.IPR.P5=anau|aneu|n'anau|n'aneu VERB.IPR.P6=van VERB.IIM.P1=anava|n'anava VERB.IIM.P2=anaves|n'anaves VERB.IIM.P3=anava|n'anava VERB.IIM.P4=anàvem|n'anàvem VERB.IIM.P5=anàveu|n'anàveu VERB.IIM.P6=anaven|n'anaven VERB.IPE.P1=aní|n'aní VERB.IPE.P2=anares|n'anares VERB.IPE.P3=anà|n'anà VERB.IPE.P4=anàrem|n'anàrem VERB.IPE.P5=anàreu|n'anàreu VERB.IPE.P6=anaren|n'anaren VERB.FUT.P1=aniré|iré|n'aniré|n'iré VERB.FUT.P2=aniràs|iràs|n'aniràs|n'iràs VERB.FUT.P3=anirà|irà|n'anirà|n'irà VERB.FUT.P4=anirem|irem|n'anirem|n'irem VERB.FUT.P5=anireu|ireu|n'anireu|n'ireu VERB.FUT.P6=aniran|iran|n'aniran|n'iran VERB.CON.P1=aniria|iria|n'aniria|n'iria VERB.CON.P2=aniries|iries|n'aniries|n'iries VERB.CON.P3=aniria|iria|n'aniria|n'iria VERB.CON.P4=aniríem|iríem|n'aniríem|n'iríem VERB.CON.P5=aniríeu|iríeu|n'aniríeu|n'iríeu VERB.CON.P6=anirien|irien|n'anirien|n'irien VERB.SPR.P1=vagi|vaja VERB.SPR.P2=vages|vagis VERB.SPR.P3=vagi|vaja VERB.SPR.P4=anem|n'anem VERB.SPR.P5=aneu|n'aneu VERB.SPR.P6=vagen|vagin VERB.SIM.P1=anara|anàs|anés|n'anara|n'anàs|n'anés VERB.SIM.P2=anares|anassis|anesses|anessis|n'anares|n'anassis|n'anesses|n'anessis VERB.SIM.P3=anara|anàs|anés|n'anara|n'anàs|n'anés VERB.SIM.P4=anàrem|anàssim|anéssem|anéssim|n'anàrem|n'anàssim|n'anéssem|n'anéssim VERB.SIM.P5=anàreu|anàssiu|anésseu|anéssiu|n'anàreu|n'anàssiu|n'anésseu|n'anéssiu VERB.SIM.P6=anaren|anassin|anessen|anessin|n'anaren|n'anassin|n'anessen|n'anessin VERB.IMP.P2=vés|vés-ne VERB.IMP.P3=vagi|vagi'n|vaja|vaja'n VERB.IMP.P4=anem|anem-ne VERB.IMP.P5=anau|anau-ne|aneu|aneu-ne VERB.IMP.P6=vagen|vagen-ne|vagin|vagin-ne",
                    "ent=donar^cat1=v. tr.^cat2=pron.^cat3=intr.^com1=ind. pr. 2 dónes, 3 dóna > VERB.INF=donar|donar-ne VERB.GER=donant|donant-ne VERB.PAR.M.SG=donat VERB.PAR.M.PL=donats VERB.PAR.F.SG=donada VERB.PAR.F.PL=donades VERB.IPR.P1=don|done|dono VERB.IPR.P2=dónes VERB.IPR.P3=dóna VERB.IPR.P4=donam|donem VERB.IPR.P5=donau|doneu VERB.IPR.P6=donen VERB.IIM.P1=donava VERB.IIM.P2=donaves VERB.IIM.P3=donava VERB.IIM.P4=donàvem VERB.IIM.P5=donàveu VERB.IIM.P6=donaven VERB.IPE.P1=doní VERB.IPE.P2=donares VERB.IPE.P3=donà VERB.IPE.P4=donàrem VERB.IPE.P5=donàreu VERB.IPE.P6=donaren VERB.FUT.P1=donaré VERB.FUT.P2=donaràs VERB.FUT.P3=donarà VERB.FUT.P4=donarem VERB.FUT.P5=donareu VERB.FUT.P6=donaran VERB.CON.P1=donaria VERB.CON.P2=donaries VERB.CON.P3=donaria VERB.CON.P4=donaríem VERB.CON.P5=donaríeu VERB.CON.P6=donarien VERB.SPR.P1=done|doni VERB.SPR.P2=dónes|donis VERB.SPR.P3=done|doni VERB.SPR.P4=donem VERB.SPR.P5=doneu VERB.SPR.P6=donen|donin VERB.SIM.P1=donara|donàs|donés VERB.SIM.P2=donares|donassis|donesses|donessis VERB.SIM.P3=donara|donàs|donés VERB.SIM.P4=donàrem|donàssim|donéssem|donéssim VERB.SIM.P5=donàreu|donàssiu|donésseu|donéssiu VERB.SIM.P6=donaren|donassin|donessen|donessin VERB.IMP.P2=dóna|dóna'n VERB.IMP.P3=done|done'n|doni|doni'n VERB.IMP.P4=donem|donem-ne VERB.IMP.P5=donau|donau-ne|doneu|doneu-ne VERB.IMP.P6=donen|donen-ne|donin|donin-ne",
                    "ent=estar^cat1=v. intr.^cat2=pron.^cat3=tr.^com1=ind. pr. 1 estic, 2 estàs, 3 està, 6 estan; subj. pr. estigui, etc.; subj. imperf. estigués, etc.; imper. estigues, estigui, estiguem, estigueu, estiguin^com2=o estar per alguna cosa > VERB.INF=d'estar|d'estar-ne|estar|estar-ne VERB.GER=estant|estant-ne VERB.PAR.M.SG=d'estat|estat|l'estat VERB.PAR.M.PL=d'estats|estats VERB.PAR.F.SG=d'estada|estada|l'estada VERB.PAR.F.PL=d'estades|estades VERB.IPR.P1=estic|n'estic VERB.IPR.P2=estàs|n'estàs VERB.IPR.P3=està|n'està VERB.IPR.P4=estam|estem|n'estam|n'estem VERB.IPR.P5=estau|esteu|n'estau|n'esteu VERB.IPR.P6=estan|n'estan VERB.IIM.P1=estava|n'estava VERB.IIM.P2=estaves|n'estaves VERB.IIM.P3=estava|n'estava VERB.IIM.P4=estàvem|n'estàvem VERB.IIM.P5=estàveu|n'estàveu VERB.IIM.P6=estaven|n'estaven VERB.IPE.P1=estiguí|n'estiguí VERB.IPE.P2=estigueres|n'estigueres VERB.IPE.P3=estigué|n'estigué VERB.IPE.P4=estiguérem|n'estiguérem VERB.IPE.P5=estiguéreu|n'estiguéreu VERB.IPE.P6=estigueren|n'estigueren VERB.FUT.P1=estaré|n'estaré VERB.FUT.P2=estaràs|n'estaràs VERB.FUT.P3=estarà|n'estarà VERB.FUT.P4=estarem|n'estarem VERB.FUT.P5=estareu|n'estareu VERB.FUT.P6=estaran|n'estaran VERB.CON.P1=estaria|n'estaria VERB.CON.P2=estaries|n'estaries VERB.CON.P3=estaria|n'estaria VERB.CON.P4=estaríem|n'estaríem VERB.CON.P5=estaríeu|n'estaríeu VERB.CON.P6=estarien|n'estarien VERB.SPR.P1=estiga|estigui|n'estiga|n'estigui VERB.SPR.P2=estigues|estiguis|n'estigues|n'estiguis VERB.SPR.P3=estiga|estigui|n'estiga|n'estigui VERB.SPR.P4=estiguem|n'estiguem VERB.SPR.P5=estigueu|n'estigueu VERB.SPR.P6=estiguen|estiguin|n'estiguen|n'estiguin VERB.SIM.P1=estiguera|estigués|n'estiguera|n'estigués VERB.SIM.P2=estigueres|estiguesses|estiguessis|n'estigueres|n'estiguesses|n'estiguessis VERB.SIM.P3=estiguera|estigués|n'estiguera|n'estigués VERB.SIM.P4=estiguérem|estiguéssem|estiguéssim|n'estiguérem|n'estiguéssem|n'estiguéssim VERB.SIM.P5=estiguéreu|estiguésseu|estiguéssiu|n'estiguéreu|n'estiguésseu|n'estiguéssiu VERB.SIM.P6=estigueren|estiguessen|estiguessin|n'estigueren|n'estiguessen|n'estiguessin VERB.IMP.P2=estigues|estigues-ne VERB.IMP.P3=estiga|estiga'n|estigui|estigui'n VERB.IMP.P4=estiguem|estiguem-ne VERB.IMP.P5=estigueu|estigueu-ne VERB.IMP.P6=estiguen|estiguen-ne|estiguin|estiguin-ne",
                    "ent=dir1^cat1=v. tr.^com1=ger. dient; p. p. dit; ind. pr. dic, dius, diu, diem, dieu, diuen; ind. imperf. deia, etc.; subj. pr. digui, etc.; subj. imperf. digués, etc.; imperat. 2 digues, 5 digueu^com2=o no trobar res a dir^com3=o tan aviat dit com fet^com4=o vol dir?, o voleu dir?^com5=o per dir-ho així, o com si diguéssim, o que diguéssim^com6=o dir-ne de tot color, o dir-ne de coents, o dir-ne de verdes i madures, o dir-ne per a salar, o dir de tot^com7=o dir de vós, o dir de vostè > VERB.INF=dir|dir-ne VERB.GER=dient|dient-ne VERB.PAR.M.SG=dit VERB.PAR.M.PL=dits VERB.PAR.F.SG=dita VERB.PAR.F.PL=dites VERB.IPR.P1=dic VERB.IPR.P2=dius VERB.IPR.P3=diu VERB.IPR.P4=deim|diem VERB.IPR.P5=deis|dieu VERB.IPR.P6=diuen VERB.IIM.P1=deia VERB.IIM.P2=deies VERB.IIM.P3=deia VERB.IIM.P4=dèiem VERB.IIM.P5=dèieu VERB.IIM.P6=deien VERB.IPE.P1=diguí VERB.IPE.P2=digueres VERB.IPE.P3=digué VERB.IPE.P4=diguérem VERB.IPE.P5=diguéreu VERB.IPE.P6=digueren VERB.FUT.P1=diré VERB.FUT.P2=diràs VERB.FUT.P3=dirà VERB.FUT.P4=direm VERB.FUT.P5=direu VERB.FUT.P6=diran VERB.CON.P1=diria VERB.CON.P2=diries VERB.CON.P3=diria VERB.CON.P4=diríem VERB.CON.P5=diríeu VERB.CON.P6=dirien VERB.SPR.P1=diga|digui VERB.SPR.P2=digues|diguis VERB.SPR.P3=diga|digui VERB.SPR.P4=diguem VERB.SPR.P5=digueu VERB.SPR.P6=diguen|diguin VERB.SIM.P1=diguera|digués VERB.SIM.P2=digueres|diguesses|diguessis VERB.SIM.P3=diguera|digués VERB.SIM.P4=diguérem|diguéssem|diguéssim VERB.SIM.P5=diguéreu|diguésseu|diguéssiu VERB.SIM.P6=digueren|diguessen|diguessin VERB.IMP.P2=digues|digues-ne VERB.IMP.P3=diga|diga'n|digui|digui'n VERB.IMP.P4=diguem|diguem-ne VERB.IMP.P5=digueu|digueu-ne VERB.IMP.P6=diguen|diguen-ne|diguin|diguin-ne",
                    "ent=obtenir^cat1=v. tr.^com1=quant a la flexió, com mantenir > VERB.INF=d'obtenir|d'obtenir-ne|obtenir|obtenir-ne VERB.GER=obtenint|obtenint-ne VERB.PAR.M.SG=d'obtengut|d'obtingut|l'obtengut|l'obtingut|obtengut|obtingut VERB.PAR.M.PL=d'obtenguts|d'obtinguts|obtenguts|obtinguts VERB.PAR.F.SG=d'obtenguda|d'obtinguda|l'obtenguda|l'obtinguda|obtenguda|obtinguda VERB.PAR.F.PL=d'obtengudes|d'obtingudes|obtengudes|obtingudes VERB.IPR.P1=n'obtenc|n'obtinc|obtenc|obtinc VERB.IPR.P2=n'obtens|obtens VERB.IPR.P3=n'obté|obté VERB.IPR.P4=n'obtenim|obtenim VERB.IPR.P5=n'obteniu|obteniu VERB.IPR.P6=n'obtenen|obtenen VERB.IIM.P1=n'obtenia|obtengués|obtenia VERB.IIM.P2=n'obtenies|obtenies VERB.IIM.P3=n'obtenia|obtenia VERB.IIM.P4=n'obteníem|obteníem VERB.IIM.P5=n'obteníeu|obteníeu VERB.IIM.P6=n'obtenien|obtenien VERB.IPE.P1=n'obtenguí|n'obtinguí|obtenguí|obtinguí VERB.IPE.P2=n'obtengueres|n'obtingueres|obtengueres|obtingueres VERB.IPE.P3=n'obtengué|n'obtingué|obtengué|obtingué VERB.IPE.P4=n'obtenguérem|n'obtinguérem|obtenguérem|obtinguérem VERB.IPE.P5=n'obtenguéreu|n'obtinguéreu|obtenguéreu|obtinguéreu VERB.IPE.P6=n'obtengueren|n'obtingueren|obtengueren|obtingueren VERB.FUT.P1=n'obtendré|n'obtindré|obtendré|obtindré VERB.FUT.P2=n'obtendràs|n'obtindràs|obtendràs|obtindràs VERB.FUT.P3=n'obtendrà|n'obtindrà|obtendrà|obtindrà VERB.FUT.P4=n'obtendrem|n'obtindrem|obtendrem|obtindrem VERB.FUT.P5=n'obtendreu|n'obtindreu|obtendreu|obtindreu VERB.FUT.P6=n'obtendran|n'obtindran|obtendran|obtindran VERB.CON.P1=n'obtendria|n'obtindria|obtendria|obtindria VERB.CON.P2=n'obtendries|n'obtindries|obtendries|obtindries VERB.CON.P3=n'obtendria|n'obtindria|obtendria|obtindria VERB.CON.P4=n'obtendríem|n'obtindríem|obtendríem|obtindríem VERB.CON.P5=n'obtendríeu|n'obtindríeu|obtendríeu|obtindríeu VERB.CON.P6=n'obtendrien|n'obtindrien|obtendrien|obtindrien VERB.SPR.P1=n'obtenga|n'obtengui|n'obtinga|n'obtingui|obtenga|obtengui|obtinga|obtingui VERB.SPR.P2=n'obtengues|n'obtenguis|n'obtingues|n'obtinguis|obtengues|obtenguis|obtingues|obtinguis VERB.SPR.P3=n'obtenga|n'obtengui|n'obtinga|n'obtingui|obtenga|obtengui|obtinga|obtingui VERB.SPR.P4=n'obtenguem|n'obtinguem|obtenguem|obtinguem VERB.SPR.P5=n'obtengueu|n'obtingueu|obtengueu|obtingueu VERB.SPR.P6=n'obtenguen|n'obtenguin|n'obtinguen|n'obtinguin|obtenguen|obtenguin|obtinguen|obtinguin VERB.SIM.P1=n'obtenguera|n'obtengués|n'obtinguera|n'obtingués|obtenguera|obtengués|obtinguera|obtingués VERB.SIM.P2=n'obtengueres|n'obtenguesses|n'obtenguessis|n'obtingueres|n'obtinguesses|n'obtinguessis|obtengueres|obtenguesses|obtenguessis|obtingueres|obtinguesses|obtinguessis VERB.SIM.P3=n'obtenguera|n'obtengués|n'obtinguera|n'obtingués|obtenguera|obtengués|obtinguera|obtingués VERB.SIM.P4=n'obtenguérem|n'obtenguéssem|n'obtenguéssim|n'obtinguérem|n'obtinguéssem|n'obtinguéssim|obtenguérem|obtenguéssem|obtenguéssim|obtinguérem|obtinguéssem|obtinguéssim VERB.SIM.P5=n'obtenguéreu|n'obtenguésseu|n'obtenguéssiu|n'obtinguéreu|n'obtinguésseu|n'obtinguéssiu|obtenguéreu|obtenguésseu|obtenguéssiu|obtinguéreu|obtinguésseu|obtinguéssiu VERB.SIM.P6=n'obtengueren|n'obtenguessen|n'obtenguessin|n'obtingueren|n'obtinguessen|n'obtinguessin|obtengueren|obtenguessen|obtenguessin|obtingueren|obtinguessen|obtinguessin VERB.IMP.P2=obtén|obtén-ne|obtingues|obtingues-ne VERB.IMP.P3=obtenga|obtenga'n|obtengui|obtengui'n|obtinga|obtinga'n|obtingui|obtingui'n VERB.IMP.P4=obtenguem|obtenguem-ne|obtinguem|obtinguem-ne VERB.IMP.P5=obteniu|obteniu-ne|obtingueu|obtingueu-ne VERB.IMP.P6=obtenguen|obtenguen-ne|obtenguin|obtenguin-ne|obtinguen|obtinguen-ne|obtinguin|obtinguin-ne"
                    );
            });
            grup.NouTest(@"Verbs filtrats", delegate(RTest resultat)
            {
                testParaules(resultat, idDIEC, new Marques(true, "102"),
                    "ent=dir1^cat1=v. tr. > VERB.INF=dir|dir-ne VERB.GER=dient|dient-ne VERB.PAR.M.SG=dit VERB.PAR.M.PL=dits VERB.PAR.F.SG=dita VERB.PAR.F.PL=dites VERB.IPR.P1=dic VERB.IPR.P2=dius VERB.IPR.P3=diu VERB.IPR.P4=diem VERB.IPR.P5=dieu VERB.IPR.P6=diuen VERB.IIM.P1=deia VERB.IIM.P2=deies VERB.IIM.P3=deia VERB.IIM.P4=dèiem VERB.IIM.P5=dèieu VERB.IIM.P6=deien VERB.IPE.P1=diguí VERB.IPE.P2=digueres VERB.IPE.P3=digué VERB.IPE.P4=diguérem VERB.IPE.P5=diguéreu VERB.IPE.P6=digueren VERB.FUT.P1=diré VERB.FUT.P2=diràs VERB.FUT.P3=dirà VERB.FUT.P4=direm VERB.FUT.P5=direu VERB.FUT.P6=diran VERB.CON.P1=diria VERB.CON.P2=diries VERB.CON.P3=diria VERB.CON.P4=diríem VERB.CON.P5=diríeu VERB.CON.P6=dirien VERB.SPR.P1=diga|digui VERB.SPR.P2=digues|diguis VERB.SPR.P3=diga|digui VERB.SPR.P4=diguem VERB.SPR.P5=digueu VERB.SPR.P6=diguen|diguin VERB.SIM.P1=diguera|digués VERB.SIM.P2=digueres|diguesses|diguessis VERB.SIM.P3=diguera|digués VERB.SIM.P4=diguérem|diguéssem|diguéssim VERB.SIM.P5=diguéreu|diguésseu|diguéssiu VERB.SIM.P6=digueren|diguessen|diguessin VERB.IMP.P2=digues|digues-ne VERB.IMP.P3=diga|diga'n|digui|digui'n VERB.IMP.P4=diguem|diguem-ne VERB.IMP.P5=digueu|digueu-ne VERB.IMP.P6=diguen|diguen-ne|diguin|diguin-ne"
                    );
                testParaules(resultat, idDIEC, new Marques(true, "001"),
                    "ent=donar^cat1=v. tr.^cat2=pron.^cat3=intr.^com1=ind. pr. 2 dónes, 3 dóna > VERB.INF=donar|donar-ne VERB.GER=donant|donant-ne VERB.PAR.M.SG=donat VERB.PAR.M.PL=donats VERB.PAR.F.SG=donada VERB.PAR.F.PL=donades VERB.IPR.P1=don|done|dono VERB.IPR.P2=dónes VERB.IPR.P3=dóna VERB.IPR.P4=donem VERB.IPR.P5=doneu VERB.IPR.P6=donen VERB.IIM.P1=donava VERB.IIM.P2=donaves VERB.IIM.P3=donava VERB.IIM.P4=donàvem VERB.IIM.P5=donàveu VERB.IIM.P6=donaven VERB.IPE.P1=doní VERB.IPE.P2=donares VERB.IPE.P3=donà VERB.IPE.P4=donàrem VERB.IPE.P5=donàreu VERB.IPE.P6=donaren VERB.FUT.P1=donaré VERB.FUT.P2=donaràs VERB.FUT.P3=donarà VERB.FUT.P4=donarem VERB.FUT.P5=donareu VERB.FUT.P6=donaran VERB.CON.P1=donaria VERB.CON.P2=donaries VERB.CON.P3=donaria VERB.CON.P4=donaríem VERB.CON.P5=donaríeu VERB.CON.P6=donarien VERB.SPR.P1=done|doni VERB.SPR.P2=dónes|donis VERB.SPR.P3=done|doni VERB.SPR.P4=donem VERB.SPR.P5=doneu VERB.SPR.P6=donen|donin VERB.SIM.P1=donara|donàs|donés VERB.SIM.P2=donares|donassis|donesses|donessis VERB.SIM.P3=donara|donàs|donés VERB.SIM.P4=donàrem|donàssim|donéssem|donéssim VERB.SIM.P5=donàreu|donàssiu|donésseu|donéssiu VERB.SIM.P6=donaren|donassin|donessen|donessin VERB.IMP.P2=dóna|dóna'n VERB.IMP.P3=done|done'n|doni|doni'n VERB.IMP.P4=donem|donem-ne VERB.IMP.P5=doneu|doneu-ne VERB.IMP.P6=donen|donen-ne|donin|donin-ne", 
                    "ent=cantar^cat1=v. tr.^cat2=intr. > VERB.INF=cantar|cantar-ne VERB.GER=cantant|cantant-ne VERB.PAR.M.SG=cantat VERB.PAR.M.PL=cantats VERB.PAR.F.SG=cantada VERB.PAR.F.PL=cantades VERB.IPR.P1=cant|cante|canto VERB.IPR.P2=cantes VERB.IPR.P3=canta VERB.IPR.P4=cantem VERB.IPR.P5=canteu VERB.IPR.P6=canten VERB.IIM.P1=cantava VERB.IIM.P2=cantaves VERB.IIM.P3=cantava VERB.IIM.P4=cantàvem VERB.IIM.P5=cantàveu VERB.IIM.P6=cantaven VERB.IPE.P1=cantí VERB.IPE.P2=cantares VERB.IPE.P3=cantà VERB.IPE.P4=cantàrem VERB.IPE.P5=cantàreu VERB.IPE.P6=cantaren VERB.FUT.P1=cantaré VERB.FUT.P2=cantaràs VERB.FUT.P3=cantarà VERB.FUT.P4=cantarem VERB.FUT.P5=cantareu VERB.FUT.P6=cantaran VERB.CON.P1=cantaria VERB.CON.P2=cantaries VERB.CON.P3=cantaria VERB.CON.P4=cantaríem VERB.CON.P5=cantaríeu VERB.CON.P6=cantarien VERB.SPR.P1=cante|canti VERB.SPR.P2=cantes|cantis VERB.SPR.P3=cante|canti VERB.SPR.P4=cantem VERB.SPR.P5=canteu VERB.SPR.P6=canten|cantin VERB.SIM.P1=cantara|cantàs|cantés VERB.SIM.P2=cantares|cantassis|cantesses|cantessis VERB.SIM.P3=cantara|cantàs|cantés VERB.SIM.P4=cantàrem|cantàssim|cantéssem|cantéssim VERB.SIM.P5=cantàreu|cantàssiu|cantésseu|cantéssiu VERB.SIM.P6=cantaren|cantassin|cantessen|cantessin VERB.IMP.P2=canta|canta'n VERB.IMP.P3=cante|cante'n|canti|canti'n VERB.IMP.P4=cantem|cantem-ne VERB.IMP.P5=canteu|canteu-ne VERB.IMP.P6=canten|canten-ne|cantin|cantin-ne"
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
                identificador = new IdentificadorDIEC("més paraules", regles, null);
                identificador.LlegeixEntrades(DirEntrades("mes_paraules.txt"), entrades, 1);
                identificador = new IdentificadorToponims("topònims", regles);
                identificador.LlegeixEntrades(DirEntrades("topònims.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("topònims_estrangers.txt"), entrades, 1);
                identificador = new IdentificadorGentilicis("gentilicis", regles, null);
                identificador.LlegeixEntrades(DirEntrades("gentilicis.txt"), entrades, 1);
                identificador.LlegeixEntrades(DirEntrades("gentilicis_estrangers.txt"), entrades, 1);
                identificador = new IdentificadorAntroponims("noms i llinatges", regles, null);
                identificador.LlegeixEntrades(DirEntrades("antropònims.txt"), entrades, 1);
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
                ee.Add(id.IdentificaEntrada("ent=alçar^cat1=v. tr.^cat2=pron.^cat3=intr."));
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