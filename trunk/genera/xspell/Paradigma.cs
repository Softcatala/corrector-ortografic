using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Generador per als mots corresponents a una entrada.
    /// </summary>
    public abstract class Paradigma
    {
        /// <summary>
        /// Crea amb una descripció.
        /// </summary>
        /// <param name="descripcio">La descripció del paradigma.</param>
        public Paradigma(string descripcio)
        {
            this.descripcio = descripcio;
        }

        /// <summary>
        /// Torna la llista de mots corresponents a una entrada.
        /// Es passa l'entrada com a paràmetre perquè moltes entrades poden compartir un paradigma.
        /// </summary>
        /// <param name="entrada">L'entrada de la qual volem generar els mots.</param>
        /// <param name="nomesAfixos">
        /// Si és true, només genera amb les regles que tenen la propietat EsAfix.
        /// Si és false, genera amb totes les regles possibles.
        /// </param>
        /// <returns>Una llista de mots.</returns>
        public virtual List<Mot> GeneraMots(Entrada entrada, Marques filtre, bool nomesAfixos)
        {
            throw new Exception("Sense implementar");
        }

        /// <summary>
        /// Torna la llista d'ítems del diccionari corresponents a una entrada.
        /// Es passa l'entrada com a paràmetre perquè moltes entrades poden compartir un paradigma.
        /// </summary>
        /// <param name="dades">Les dades de l'entrada.</param>
        /// <param name="excepcions">Informació sobre excepcions.</param>
        /// <param name="filtre">Les marques per filtrar el resultat</param>
        /// <param name="speller">El format del corrector.</param>
        /// <returns>Una llista amb els ítems.</returns>
        public virtual List<ItemDic> GeneraDic(Dictionary<string,string> dades, 
            Dictionary<string, string> excepcions, Marques filtre, Entrada.Speller speller)
        {
            throw new Exception("Sense implementar");
        }

        public override string ToString()
        {
            return String.Format("Paradigma: {0}", descripcio);
        }

        private string descripcio;
    }

}
