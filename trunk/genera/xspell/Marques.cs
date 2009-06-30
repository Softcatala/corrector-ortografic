using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Un conjunt de marques.
    /// La funció principal diu si una marca pertany al conjunt.
    /// Es pot crear un conjunt restant marques del conjunt univers, o bé sumant-ne
    /// al conjunt buit.
    /// </summary>
    public class Marques
    {

        /// <summary>
        /// Crea un conjunt sense excepcions.
        /// </summary>
        /// <param name="defecte">Valor de pertinença per defecte. Si és true, el conjunt conté totes les marques.
        /// Si és false, el conjunt és buit.</param>
        public Marques(bool defecte)
        {
            this.defecte = defecte;
            excepcions = 0;
        }

        /// <summary>
        /// Crea un conjunt.
        /// </summary>
        /// <param name="defecte">El valor de pertinença al conjunt per a les marques no especificades.</param>
        /// <param name="excepcio0">La primera marca que no segueix el defecte</param>
        /// <param name="excepcions">Les marques que no segueixen el defecte.</param>
        public Marques(bool defecte, string excepcio0, params string[] excepcions)
            : this(defecte)
        {
            this.excepcions |= Marca.Una(excepcio0).Mascara;
            foreach (string exc in excepcions)
                this.excepcions |= Marca.Una(exc).Mascara;
        }

        /// <summary>
        /// Crea un conjunt.
        /// </summary>
        /// <param name="defecte">El valor de pertinença al conjunt per a les marques no especificades.</param>
        /// <param name="excepcio0">La primera marca que no segueix el defecte</param>
        /// <param name="excepcions">Les marques que no segueixen el defecte.</param>
        public Marques(bool defecte, Marca excepcio0, params Marca[] excepcions)
            : this(defecte)
        {
            this.excepcions |= excepcio0.Mascara;
            foreach (Marca exc in excepcions)
                this.excepcions |= exc.Mascara;
        }

        public Marques(Marques altre)
        {
            defecte = altre.defecte;
            excepcions = altre.excepcions;
        }

        /// <summary>
        /// Diu si una marca pertany al conjunt.
        /// </summary>
        /// <param name="marca">La marca de la qual volem saber si pertany al conjunt.</param>
        /// <returns>true si la marca pertany al conjunt.</returns>
        public bool Conte(string marca)
        {
            return Conte(Marca.Una(marca).Mascara);
        }

        /// <summary>
        /// Diu si una marca pertany al conjunt.
        /// </summary>
        /// <param name="marca">La marca de la qual volem saber si pertany al conjunt.</param>
        /// <returns>true si la marca pertany al conjunt.</returns>
        public bool Conte(Marca marca)
        {
            return Conte(marca.Mascara);
        }

        /// <summary>
        /// Diu si un conjunt de marques pertany al conjunt.
        /// </summary>
        /// <param name="marques">El conjunt de marques del qual volem saber si pertany al conjunt.</param>
        /// <returns>true si totes les marques pertanyen al conjunt.</returns>
        public bool Conte(uint marques)
        {
            if (defecte)
                return (excepcions & marques) == 0;
            else
                return (excepcions & marques) == marques;
        }

        /// <summary>
        /// Torna true si el conjunt altre està contingut dins aquest.
        /// </summary>
        public bool Conte(Marques altre)
        {
            uint comuns = Llista & altre.Llista;
            return (altre.Llista & ~comuns) == 0;
        }

        public Marques Mes(Marca marca)
        {
            if (defecte)
                excepcions &= ~marca.Mascara;
            else
                excepcions |= marca.Mascara;
            return this;
        }

        public Marques Mes(Marques marques)
        {
            if (defecte)
                excepcions &= ~marques.Llista;
            else
                excepcions |= marques.Llista;
            return this;
        }

        public Marques Menys(Marca marca)
        {
            if (defecte)
                excepcions |= marca.Mascara;
            else
                excepcions &= ~marca.Mascara;
            return this;
        }

        public Marques Menys(Marques marques)
        {
            if (defecte)
                excepcions |= marques.Llista;
            else
                excepcions &= ~marques.Llista;
            return this;
        }

        public bool Defecte { get { return defecte; } }

        public uint Llista { get { return defecte ? ~excepcions : excepcions; } }

        public bool Buit { get { return Llista == 0; } }

        static public bool operator ==(Marques m1, Marques m2)
        {
            return m1.Llista == m2.Llista;
        }

        static public bool operator !=(Marques m1, Marques m2)
        {
            return m1.Llista != m2.Llista;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override string ToString()
        {
            List<string> ids = new List<string>();
            for (uint i = 1; i != 0; i *= 2)
                if (Conte(i))
                {
                    Marca una = Marca.Una(i);
                    if (una != null)
                        ids.Add(una.Id);
                }
            return string.Join(", ", ids.ToArray());
        }

        private bool defecte;
        private uint excepcions;

        /// <summary>
        /// El conjunt de totes les marques.
        /// </summary>
        static public Marques totes = new Marques(true);

    }
}
