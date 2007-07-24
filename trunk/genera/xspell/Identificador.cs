using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Una classe per identificar entrades i paradigmes.
    /// </summary>
    abstract public class Identificador
    {
        /// <summary>
        /// Crea l'identificador amb una descripció.
        /// </summary>
        /// <param name="descripcio">La descripció de l'identificador.</param>
        public Identificador(string descripcio)
        {
            this.descripcio = descripcio;
        }

        /// <summary>
        /// Identifica una entrada.
        /// </summary>
        /// <param name="linia">La línia a partir de la qual volem identificar l'entrada.</param>
        /// <returns>Un nou objecte Entrada o null.</returns>
        public abstract Entrada IdentificaEntrada(string linia);

        /// <summary>
        /// Identifica el paradigma que correspon a les dades d'una entrada.
        /// Eventualment, afegeix informació a l'entrada, per ajudar a generar les formes del paradigma.
        /// </summary>
        /// <param name="dades">Les dades de l'entrada per a la qual volem identificar el paradigma.</param>
        /// <param name="excepcions">Les excepcions per a l'entrada.</param>
        /// <returns>Un paradigma per a l'entrada.</returns>
        public virtual Paradigma IdentificaParadigma(Dictionary<string, string> dades, 
            Dictionary<string, string> excepcions)
        {
            throw new Exception("Sense implementar");
        }

        /// <summary>
        /// Llegeix les entrades contingudes en un fitxer i les afegeix a una llista.
        /// </summary>
        /// <param name="nomFitxer">El nom del fitxer amb les entrades.</param>
        /// <param name="entrades">La llista que recull les entrades.</param>
        /// <param name="mod">
        /// Només s'afegeixen les entrades que tenen un índex múltiple de mod.
        /// Per llegir-les totes s'ha de fer mod = 1.
        /// </param>
        /// <returns>El nombre d'entrades recuperades.</returns>
        public abstract int LlegeixEntrades(string nomFitxer, List<Entrada> entrades, int mod);

        public abstract Regles Regles { get; }
        public abstract LiniaMarques Excepcio(string ent);
        public abstract void NovaExcepcio(string ent, LiniaMarques contingut);
        public abstract List<string> ExcepcionsSenseEmprar();

        public override string ToString()
        {
            return String.Format("Identificador d'entrades i paradigmes: {0}", descripcio);
        }

        private string descripcio;
    }
}
