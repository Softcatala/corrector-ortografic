using System;
using System.Collections.Generic;
using System.Text;
using xspell;

namespace Genera
{
    /// <summary>
    /// Definicions per a cada versió que farem del diccionari.
    /// Amb la informació que proporciona cada objecte d'aquesta classe
    /// crearem una versió del diccionari.
    /// Un mètode estàtic torna la llista de versions que volem crear.
    /// </summary>
    class VersioDiccionari
    {
        private VersioDiccionari(string nom, string desc, string variant, Marques filtre)
        {
            this.nom = nom;
            this.desc = desc;
            this.variant = variant;
            this.filtre = filtre;
            extra = new Dictionary<string, string>();
        }

        /// <summary>
        /// Torna el número de versió dels diccionaris.
        /// </summary>
        public String NumeroVersio { get { return InfoNumeroVersio.VersioActual.Numero; } }

        /// <summary>
        /// Torna el lloc web d'on agafarem les actualitzacions.
        /// </summary>
        public string LlocActualitzacions { 
            get 
            { 
                #if DEBUG
                		return @"http://softcatala.org/diccionaris/actualitzacions/OOo";
                    //return @"http://localhost/actualitzacions/OOo";
                #else
                    return @"http://softcatala.org/diccionaris/actualitzacions/OOo";
                #endif
            } 
        }

        /// <summary>
        /// Notes sobre la versió actual
        /// </summary>
        /// <param name="cat">Notes en català (true) o anglès (false)</param>
        public string NotesVersioActual(bool cat)
        {
            return string.Join("\n", InfoNumeroVersio.VersioActual.Notes(cat).ToArray());
        }

        /// <summary>
        /// Notes sobre totes les versions, en format html
        /// </summary>
        /// <param name="cat">Notes en català (true) o anglès (false)</param>
        public string NotesVersions(bool cat)
        {
            return string.Join("\n", InfoNumeroVersio.NotesHtml(cat).ToArray());
        }

        /// <summary>
        /// Notes sobre totes les versions, en format text
        /// </summary>
        /// <param name="cat">Notes en català (true) o anglès (false)</param>
        public string NotesVersionsRaw(bool cat)
        {
            return string.Join("\n", InfoNumeroVersio.NotesRaw(cat).ToArray());
        }

        /// <summary>
        /// El nom del fitxer.
        /// No inclou els eventuals prefixos i sufixos ni les extensions.
        /// </summary>
        public string Nom { get { return nom; } }

        /// <summary>
        /// Torna la variant d'aquest diccionari.
        /// </summary>
        public String Variant { get { return variant; } }

        /// <summary>
        /// Descripció de la versió.
        /// </summary>
        public string Descripcio { get { return desc; } }

        /// <summary>
        /// El filtre que s'aplicarà.
        /// Els diccionari i els fitxers de regles només inclouen els elements que tenen totes les marques
        /// incloses dins el filtre.
        /// </summary>
        public Marques Filtre { get { return filtre; } }

        /// <summary>
        /// Torna una opció extra. Si no es troba la clau, es torna la clau sense modificació.
        /// </summary>
        /// <param name="clau">La clau que cercam.</param>
        /// <returns>El valor de la clau.</returns>
        public string Extra(string clau)
        {
            if (extra.ContainsKey(clau))
                return extra[clau];
            else
                return clau;
        }

        private void Extra(string clau, string valor)
        {
            extra.Add(clau, valor);
            extra.Add(clau.ToLower(), valor.ToLower());
        }

        /// <summary>
        /// Torna la llista de versions que volem crear.
        /// </summary>
        static public List<VersioDiccionari> Versions()
        {
            List<VersioDiccionari> llista = new List<VersioDiccionari>();
            Marques nomesAvl = new Marques(false, "201", "202");    // particularitats de l'AVL sense acceptació general

            // Cream la versió general
            VersioDiccionari versio = new VersioDiccionari("catalan", "Versió general", "general", new Marques(true).Menys(nomesAvl));
            versio.Extra("%AS_LNG%", "ca");
            versio.Extra("%AS_LANG%", "ca-general");
            versio.Extra("%AS_LANGUAGE%", "Catalan");
            versio.Extra("%AS_LANGUAGE_CA%", "Català");
            llista.Add(versio);

            // Cream la versió AVL
            versio = new VersioDiccionari("avl", "Versió AVL", "avl", Marques.totes);
            versio.Extra("%AS_LNG%", "ca-valencia");
            versio.Extra("%AS_LANG%", "ca-valencia");
            versio.Extra("%AS_LANGUAGE%", "Catalan_valencia");
            versio.Extra("%AS_LANGUAGE_CA%", "Català_valencià");
            llista.Add(versio);

            return llista;
        }

        private string nom, desc, variant;
        private Marques filtre;
        private Dictionary<string, string> extra;

        private class InfoNumeroVersio
        {
            /// <summary>
            /// Torna la versió amb el número més alt.
            /// </summary>
            public static InfoNumeroVersio VersioActual { 
                get 
                {
                    if (llista == null)
                        llista = CreaLlista();
                    return llista[0]; 
                } 
            }

            /// <summary>
            /// Torna les notes en una llengua.
            /// </summary>
            /// <param name="cat">Si és true, torna les notes en català.
            /// Si és false, torna les notes en anglès.</param>
            /// <returns></returns>
            public List<string> Notes(bool cat)
            {
                List<string> nn = new List<string>();
                foreach (NotaVersio nv in notes)
                    nn.Add(cat ? nv.Cat : nv.Eng);
                return nn;
            }

            /// <summary>
            /// Torna el número d'aquesta versió.
            /// </summary>
            public string Numero { get { return string.Format("{0}.{1}.{2}", n1, n2, n3); } }

            /// <summary>
            /// Notes de totes les versions en format html
            /// </summary>
            /// <param name="cat">Si és true, torna les notes en català.
            /// Si és false, torna les notes en anglès.</param>
            /// <returns>La llista de totes les versions, de més moderna a més antiga, en format html.</returns>
            public static List<String> NotesHtml(bool cat)
            {
                if (llista == null)
                    llista = CreaLlista();
                List<string> notes = new List<string>();
                foreach (InfoNumeroVersio inv in llista)
                {
                    notes.Add(inv.CapHtml(cat));
                    notes.Add("<br/>");
                    notes.Add("<ul>");
                    foreach (NotaVersio nota in inv.notes)
                        notes.Add(string.Format("<li>{0}</li>", cat ? nota.Cat : nota.Eng));
                    notes.Add("</ul>");
                }
                return notes;
            }

            /// <summary>
            /// Notes de totes les versions en format text
            /// </summary>
            /// <param name="cat">Si és true, torna les notes en català.
            /// Si és false, torna les notes en anglès.</param>
            /// <returns>La llista de totes les versions, de més moderna a més antiga, en format text.</returns>
            public static List<String> NotesRaw(bool cat)
            {
                if (llista == null)
                    llista = CreaLlista();
                List<string> notes = new List<string>();
                foreach (InfoNumeroVersio inv in llista)
                {
                    notes.Add(inv.CapRaw(cat));
                    foreach (NotaVersio nota in inv.notes)
                        notes.Add(string.Format("    {0}", cat ? nota.Cat : nota.Eng));
                    notes.Add("");
                }
                return notes;
            }

            private string CapHtml(bool cat)
            {
                if (cat)
                    return string.Format("<b>Versió {0}</b> ({1})", Numero, data.ToShortDateString());
                else
                    return string.Format("<b>Version {0}</b> ({1})", Numero, data.ToShortDateString());
            }

            private string CapRaw(bool cat)
            {
                if (cat)
                    return string.Format("Versió {0} ({1})", Numero, data.ToShortDateString());
                else
                    return string.Format("Version {0} ({1})", Numero, data.ToShortDateString());
            }
            private InfoNumeroVersio(int n1, int n2, int n3, DateTime data, params NotaVersio[] notes)
            {
                this.n1 = n1; this.n2 = n2; this.n3 = n3;
                this.data = data;
                this.notes = new List<NotaVersio>(notes);
            }

            private static List<InfoNumeroVersio> CreaLlista()
            {
                List<InfoNumeroVersio> llista = new List<InfoNumeroVersio>();
                // Aquí ha d'anar la informació sobre els números de versió
                // PER_FER: agafar la informació d'un fitxer
                // En el fitxer regles.txt es repeteix la informació del número i la data de la versió
                llista.Add(new InfoNumeroVersio(2, 5, 0,
                    new DateTime(2013, 2, 15),
                    new NotaVersio("Afegides i corregides entrades diverses", "Added and corrected several entries")
                    ));
                llista.Add(new InfoNumeroVersio(2, 4, 0,
                    new DateTime(2012, 6, 17),
                    new NotaVersio("Afegides entrades que mancaven del DIEC2 (http://dlc.iec.cat/)", "Added missing entries from DIEC2 (http://dlc.iec.cat/)"),
                    new NotaVersio("Informació extra al fitxer .aff, per funcionar correctament en mode shell", 
                        "Extra information in .aff file, to enable correct working in shell mode")
                    ));
                llista.Add(new InfoNumeroVersio(2, 3, 0,
                    new DateTime(2011, 12, 30),
                    new NotaVersio("Afegides entrades recollides de fonts diverses", "Added entries from several sources"),
                    new NotaVersio("Aplicades correccions i propostes del wiki de SC", "Applied some corrections and proposals from SoftCatala wiki")
                    ));
                llista.Add(new InfoNumeroVersio(2, 2, 0,
                    new DateTime(2010, 1, 13),
                    new NotaVersio("Afegides entrades recollides del Diccionari ortogràfic i de pronunciació del valencià (AVL)", "Added entries collected by Valencian Academy of Language (AVL)"),
                    new NotaVersio("Afegides entrades recollides de fonts diverses", "Added entries from several sources"),
                    new NotaVersio("Aplicades correccions i propostes del wiki de SC", "Applied some corrections and proposals from SoftCatala wiki")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 6,
                    new DateTime(2009, 10, 6),
                    new NotaVersio("Moltes paraules invariables (sg=pl) eren tractades com si fossin variables (detectat per Joan Montané)", "Several invariable words (sg=pl) were treated as variable (reported by Joan Montané)")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 5,
                    new DateTime(2009, 06, 27),
                    new NotaVersio("Algunes correccions a les regles", "Some corrections in the rules"),
                    new NotaVersio("Generació d'un fitxer \"oficial\" d'aspell", "Generate an \"official\" dictionary file for aspell")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 4,
                    new DateTime(2009, 03, 11),
                    new NotaVersio("Algunes correccions i addicions", "Some corrections and additions"),
                    new NotaVersio("Correccions: bytes, malveure", "Corrections: bytes, malveure")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 3,
                    new DateTime(2009, 03, 02),
                    new NotaVersio("Generació de fitxers per a aspell", "Generate files for aspell")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 2,
                    new DateTime(2008, 10, 27),
                    new NotaVersio("Informació per a actualització remota", "Information for remote update")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 1,
                    new DateTime(2008, 10, 19),
                    new NotaVersio("Addicions valencianes i correccions d'errors", "Valencian additions and correction of mistakes")
                    ));
                llista.Add(new InfoNumeroVersio(2, 1, 0,
                    new DateTime(2008, 6, 3),
                    new NotaVersio("Creada versió per a AVL", "Created Valencian specific version"),
                    new NotaVersio("Noves entrades procedents del diccionari AVL", "New entries from AVL dictionary")
                    ));
                llista.Add(new InfoNumeroVersio(2, 0, 0,
                    new DateTime(2007, 7, 14),
                    new NotaVersio("Afegides les entrades del DIEC 2", "Added the entries from the DIEC 2"),
                    new NotaVersio("Correcció en regles i en moltes entrades", "Correction in rules and several entries"),
                    new NotaVersio("Afegits topònims i altres paraules", "Added names of places and other words")
                    ));
                llista.Add(new InfoNumeroVersio(1, 0, 3,
                    new DateTime(2006, 10, 16),
                    new NotaVersio("Afegits gentilicis valencians", "Added names of Valencian places")
                    ));
                llista.Add(new InfoNumeroVersio(1, 0, 2,
                    new DateTime(2006, 9, 29),
                    new NotaVersio("Afegides combinacions valencianes de pronoms personals", "Added Valencian combinations of personal pronouns")
                    ));
                llista.Add(new InfoNumeroVersio(1, 0, 1,
                    new DateTime(2006, 9, 19),
                    new NotaVersio("Afegides paraules que faltaven", "Added missing words")
                    ));
                llista.Add(new InfoNumeroVersio(1, 0, 0,
                    new DateTime(2002, 10, 10),
                    new NotaVersio("Primera versió: DIEC 1, topònims, noms propis", "First version: DIEC 1, names of places, person names")
                    ));
                llista.Sort(delegate(InfoNumeroVersio nv1, InfoNumeroVersio nv2)
                {
                    if (nv1.n1 != nv2.n1)
                        return -(nv1.n1 - nv2.n1);
                    if (nv1.n2 != nv2.n2)
                        return -(nv1.n2 - nv2.n2);
                    if (nv1.n3 != nv2.n3)
                        return -(nv1.n3 - nv2.n3);
                    return 0;
                });
                return llista;
            }

            static private List<InfoNumeroVersio> llista;

            private int n1, n2, n3;
            private DateTime data;
            private List<NotaVersio> notes;
        }

        private class NotaVersio
        {
            public NotaVersio(string cat, string eng)
            {
                this.cat = cat;
                this.eng = eng;
            }

            public string Cat { get { return cat; } }
            public string Eng { get { return eng; } }

            private string cat, eng;
        }

    }

}

