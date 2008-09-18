using System;
using System.Collections.Generic;
using System.Text;
using xspell;

namespace Genera
{
    /// <summary>
    /// Definicions per a cada versi� que farem del diccionari.
    /// Amb la informaci� que proporciona cada objecte d'aquesta classe
    /// crearem una versi� del diccionari.
    /// Un m�tode est�tic torna la llista de versions que volem crear.
    /// </summary>
    class VersioDiccionari
    {
        private VersioDiccionari(string nom, string desc, string variant, Marques filtre)
        {
            this.nom = nom;
            this.desc = desc;
            this.variant = variant;
            this.filtre = filtre;
        }

        /// <summary>
        /// Torna el n�mero de versi� dels diccionaris.
        /// </summary>
        public String NumeroVersio { get { return "2.1.0"; } }

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
        /// Descripci� de la versi�.
        /// </summary>
        public string Descripcio { get { return desc; } }

        /// <summary>
        /// El filtre que s'aplicar�.
        /// Els diccionari i els fitxers de regles nom�s inclouen els elements que tenen totes les marques
        /// incloses dins el filtre.
        /// </summary>
        public Marques Filtre { get { return filtre; } }

        /// <summary>
        /// Torna la llista de versions que volem crear.
        /// </summary>
        static public List<VersioDiccionari> Versions()
        {
            List<VersioDiccionari> llista = new List<VersioDiccionari>();
            Marques avl = new Marques(false, "201");    // particularitats de l'AVL sense acceptaci� general
            llista.Add(new VersioDiccionari("catalan", "Versi� general", "general", new Marques(true).Menys(avl)));
            llista.Add(new VersioDiccionari("avl", "Versi� AVL", "avl", Marques.totes));
            return llista;
        }

        private string nom, desc, variant;
        private Marques filtre;
    }
}
