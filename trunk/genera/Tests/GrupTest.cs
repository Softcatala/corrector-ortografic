using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tests
{
    /// <summary>
    /// Un grup de tests o d'altres grups
    /// </summary>
    public class GrupTest : BaseTest
    {

        /// <summary>
        /// Crea amb una descripció i un mostrador.
        /// Es crea sense pare, a l'arrel de l'arbre.
        /// </summary>
        /// <param name="descripcio">La descripció del grup de tests</param>
        /// <param name="mostrador">L'objecte que mostrarà els resultats</param>
        public GrupTest(string descripcio, IMostraTest mostrador)
            : base(descripcio, mostrador)
        {
            membres = new List<BaseTest>();
            mostrador.NouTest(this);
        }

        /// <summary>
        /// Afegeix un test final a aquest grup.
        /// </summary>
        /// <param name="descripcio">La descripció del test final</param>
        /// <param name="test">El cos del test</param>
        public void NouTest(string descripcio, FTest test)
        {
            Test nou = new Test(descripcio, this, test);
            membres.Add(nou);
            mostrador.NouTest(nou);
        }

        /// <summary>
        /// Afegeix un grup a aquest grup
        /// </summary>
        /// <param name="descripcio">La descripció del nou grup</param>
        /// <returns>El grup que s'ha creat</returns>
        public GrupTest NouGrup(string descripcio)
        {
            GrupTest nou = new GrupTest(descripcio, this);
            membres.Add(nou);
            return nou;
        }

        public override int Bons
        {
            get 
            {
                int bons = 0;
                foreach (BaseTest bt in membres)
                    bons += bt.Bons;
                return bons;
            }
        }

        public override int Dolents
        {
            get 
            {
                int dolents = 0;
                foreach (BaseTest bt in membres)
                    dolents += bt.Dolents;
                return dolents;
            }
        }

        private GrupTest(string descripcio, GrupTest pare)
            : base(descripcio, pare)
        {
            membres = new List<BaseTest>();
            mostrador.NouTest(this);
        }

        private List<BaseTest> membres;

    }
}
