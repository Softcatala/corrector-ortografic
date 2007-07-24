using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Tests
{
    /// <summary>
    /// Una prova at�mica.
    /// L'usuari aporta la descripci� de la prova i una funci� an�nima.
    /// </summary>
    public class Test : BaseTest
    {

        /// <summary>
        /// Crea amb una descripci�, un grup i una funci� de prova.
        /// Es considera que el test ha funcionat si la funci� torna true.
        /// Es considera que el test ha fallat si torna false o 
        /// </summary>
        /// <param name="descripcio">La descripci� del test</param>
        /// <param name="grup">El grup a qu� pertanyer� aquest test</param>
        /// <param name="test">El cos de la funci� de prova</param>
        public Test(string descripcio, GrupTest grup, FTest test) : base(descripcio, grup)
        {
            resultat = new RTest();
            try
            {
                test(resultat);
            }
            catch (Exception ex)
            {
                resultat.Error("No s'ha completat ({0}) [{1}]", ex.Message, ex.StackTrace);
            }
        }

        public override int Bons
        {
            get { return resultat.Exit ? 1 : 0; }
        }

        public override int Dolents
        {
            get { return resultat.Exit ? 0 : 1; }
        }

        public override List<string> Notes
        {
            get
            {
                return resultat.Notes;
            }
        }

        public override string Resultat
        {
            get
            {
                return resultat.Resultat;
            }
        }

        private RTest resultat;

    }

    /// <summary>
    /// La classe que s'envia als tests perqu� hi posin els resultats.
    /// </summary>
    public class RTest
    {
        /// <summary>
        /// Crea un resultat sense notes i amb �xit.
        /// </summary>
        public RTest()
        {
            notes = null;
            exit = true;
        }

        /// <summary>
        /// Afegeix una nota.
        /// L'estat de l'�xit del test no canvia.
        /// </summary>
        /// <param name="nota">La nota que es vol afegir.</param>
        public void Nota(string nota)
        {
            if (notes == null)
                notes = new List<string>();
            notes.Add(nota);
        }

        /// <summary>
        /// Afegeix una nota.
        /// L'estat de l'�xit no canvia.
        /// </summary>
        /// <param name="format">El format de la nota.</param>
        /// <param name="args">Els arguments per al format de la nota.</param>
        public void Nota(string format, params object[] args)
        {
            if (notes == null)
                notes = new List<string>();
            notes.Add(String.Format(format, args));
        }

        /// <summary>
        /// Comprova si s'ha rebut el que s'esperava.
        /// Si esperat != rebut, s'afegeix una nota d'error
        /// </summary>
        /// <param name="esperat">El que s'esperava.</param>
        /// <param name="rebut">El que s'ha rebut.</param>
        public void Esperat(string esperat, string rebut)
        {
            if (esperat != rebut)
                Error("S'esperava \"{0}\" i s'ha rebut \"{1}\"", esperat, rebut);
        }

        /// <summary>
        /// Crea una nota i marca el test com a fracassat.
        /// </summary>
        /// <param name="nota">El test de la nota.</param>
        public void Error(string nota)
        {
            exit = false;
            Nota(nota);
        }

        /// <summary>
        /// Crea una nota i marca el test com a fracassat.
        /// </summary>
        /// <param name="format">El format de la nota.</param>
        /// <param name="args">Els arguments de la nota.</param>
        public void Error(string format, params object[] args)
        {
            exit = false;
            Nota(format, args);
        }

        public void Assert(bool condicio, string format, params object[] args)
        {
            if (!condicio)
                Error(format, args);
        }

        public void Assert(bool condicio, string nota)
        {
            if (!condicio)
                Error(nota);
        }

        /// <summary>
        /// Siu si el test ha funcionat (true) o ha fallat (false).
        /// </summary>
        public bool Exit { get { return exit; } }

        /// <summary>
        /// Descriu el resultat del test amb una paraula.
        /// </summary>
        public String Resultat { get { return (exit ? "�XIT" : "FRAC�S"); } }

        /// <summary>
        /// Torna la llista de notes associades amb el test.
        /// Si no hi ha notes, torna null.
        /// </summary>
        public List<string> Notes { get { return notes; } }

        private List<string> notes;
        private bool exit;
    }

    /// <summary>
    /// Les funcions que executen els tests.
    /// </summary>
    /// <param name="resultat">L'objecte que recull el resultat del test.
    /// La funci� ha de cridar els m�todes d'aquest objecte per indicar l'�xit
    /// o el frac�s del test i per afegir notes que poden servir per a resoldre
    /// els fracassos.</param>
    public delegate void FTest(RTest resultat);

}
