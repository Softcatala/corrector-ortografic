using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Marca per a etiquetar informació relacionada amb el lèxic.
    /// Hi ha marques dialectals per a casos de regla i paraules.
    /// Es poden generar formes, fitxers d'afixos, diccionaris, etc. en funció de 
    /// les marques dels casos de les regles o de les paraules.
    /// Cada marca té una màscara (un nombre amb un sol bit activat).
    /// Hi ha una llista estàtica de marques, per assegurar que cada marca té una
    /// màscara diferent de les altres.
    /// </summary>
    public class Marca
    {
        /// <summary>
        /// Crea a partir d'un identificador i una descripció
        /// </summary>
        /// <param name="id">L'identificador de la marca. És una seqüència de tres dígits.</param>
        /// <param name="descripcio">La descripció de la marca.</param>
        private Marca(string id, string descripcio)
        {
            this.id = id;
            this.descripcio = descripcio;
            mascara = (1U << marques.Count);
            marques[id] = this;
            mascares[mascara] = this;
        }

        /// <summary>
        /// Crea una marca amb un identificador i una descripció.
        /// Si ja existia, torna la preexistent.
        /// </summary>
        /// <param name="id">L'identificador de la marca.</param>
        /// <param name="descripcio">La descripció de la marca.</param>
        /// <returns>Una marca amb l'identificador i la descripció donats.</returns>
        public static Marca Crea(string id, string descripcio)
        {
            if (marques == null)
            {
                marques = new Dictionary<string, Marca>();
                mascares = new Dictionary<uint, Marca>();
            }
            if (marques.ContainsKey(id))
            {
                if (marques[id].Descripcio != descripcio) throw new Exception("Marca redefinida: " + id);
                return marques[id];
            }
            if (marques.Count > sizeof(int) * 8) throw new Exception("Massa marques!");
            Marca marca = new Marca(id, descripcio);
            return marca;
        }

        public override string ToString()
        {
            return String.Format("{0}: {1}", id, descripcio);
        }

        public string Id { get { return id; } }
        public string Descripcio { get { return descripcio; } }
        public uint Mascara { get { return mascara; } }

        public static Marca Una(string id)
        {
            if (marques == null)
            {
                marques = new Dictionary<string, Marca>();
                mascares = new Dictionary<uint, Marca>();
            }
            if (!marques.ContainsKey(id)) throw new Exception("No existeix la marca " + id);
            return marques[id];
        }

        public static Marca Una(uint mascara)
        {
            if (mascares == null)
            {
                marques = new Dictionary<string, Marca>();
                mascares = new Dictionary<uint, Marca>();
            }
            if (!mascares.ContainsKey(mascara))
                return null;
            else
                return mascares[mascara];
        }

        static public Marca grup1 = Marca.Crea("_1_", "Grup 1");
        static public Marca grup2 = Marca.Crea("_2_", "Grup 2");
        static public Marques grups12 = new Marques(false, grup1, grup2);

        private string id;
        private string descripcio;
        private uint mascara;

        private static Dictionary<string, Marca> marques;
        private static Dictionary<uint, Marca> mascares;
    }
}
