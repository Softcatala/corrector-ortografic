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
        private VersioDiccionari(string nom, string desc, Marques filtre)
        {
            this.nom = nom;
            this.desc = desc;
            this.filtre = filtre;
        }

        /// <summary>
        /// El nom del fitxer.
        /// No inclou els eventuals prefixos i sufixos ni les extensions.
        /// </summary>
        public string Nom { get { return nom; } }

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
        /// Torna la llista de versions que volem crear.
        /// </summary>
        static public List<VersioDiccionari> Versions()
        {
            List<VersioDiccionari> llista = new List<VersioDiccionari>();
            Marques avl = new Marques(false, "201");    // particularitats de l'AVL sense acceptació general
            llista.Add(new VersioDiccionari("catalan", "Versió general", new Marques(true).Menys(avl)));
            llista.Add(new VersioDiccionari("avl", "Versió AVL", Marques.totes));
            return llista;
        }

        private string nom, desc;
        private Marques filtre;
    }
}
