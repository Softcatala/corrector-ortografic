using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tests
{
    /// <summary>
    /// La classe base de GrupTest i Test.
    /// Els tests s'organitzen en forma d'arbre, amb grups i tests finals.
    /// Un grup pot contenir tests finals i altres grups.
    /// Els tests tenen un pare (un grup) o estan a l'arrel de l'arbre.
    /// Per crear un test final i mostrar-lo és necessari posar-lo dins un grup.
    /// </summary>
    public abstract class BaseTest
    {
        /// <summary>
        /// Crea amb la descripció i el grup al qual ha de pertànyer.
        /// El test es mostrarà dins mateix mostrador del grup.
        /// </summary>
        /// <param name="descripcio">La descripció del test</param>
        /// <param name="grup">El grup al qual pertanyerà aquest test</param>
        protected BaseTest(string descripcio, GrupTest grup)
        {
            this.descripcio = descripcio;
            this.grup = grup;
            this.mostrador = grup.mostrador;
        }

        /// <summary>
        /// Crea amb la descripció i el mostrador.
        /// El test es posa a l'arrel del mostrador (sense pare).
        /// </summary>
        /// <param name="descripcio">La descripció del test</param>
        /// <param name="mostrador">El mostrador que mostrarà els tests.</param>
        protected BaseTest(string descripcio, IMostraTest mostrador)
        {
            this.descripcio = descripcio;
            this.grup = null;
            this.mostrador = mostrador;
        }

        /// <summary>
        /// Quants de tests han donat resultat positiu.
        /// Per a tests finals, 1 si han tingut èxit, 0 si no n'han tingut.
        /// Per a grups, la suma de Bons per als membres del grup.
        /// </summary>
        abstract public int Bons { get; }

        /// <summary>
        /// Quants de tests han donat resultat negatiu.
        /// Per a tests finals, 0 si han tingut èxit, 1 si no n'han tingut.
        /// Per a grups, la suma de Dolents per als membres del grup.
        /// </summary>
        abstract public int Dolents { get; }

        /// <summary>
        /// La descripció del test
        /// </summary>
        public String Descripcio { get { return descripcio; } }

        /// <summary>
        /// El grup a què pertany aquest test.
        /// Els tests que estan a l'arrel tenen Grup == null
        /// </summary>
        public GrupTest Grup { get { return grup; } }

        /// <summary>
        /// El resultat del test, en forma de cadena.
        /// Pot estar indefinit (Resultat == null)
        /// </summary>
        public virtual String Resultat { get { return null; } }

        /// <summary>
        /// Les notes associades al test.
        /// Pot estar indefinit (Resultat == null)
        /// </summary>
        public virtual List<String> Notes { get { return null; } }

        protected IMostraTest mostrador;
        protected bool exit;

        private string descripcio;
        private GrupTest grup;
    }
}
