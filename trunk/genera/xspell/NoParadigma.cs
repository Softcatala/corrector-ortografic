using System;
using System.Collections.Generic;
using System.Text;

namespace xspell
{
    /// <summary>
    /// Un paradigma que no fa res.
    /// Torna una llista amb un únic element: l'arrel de l'entrada.
    /// </summary>
    public class NoParadigma : Paradigma
    {
        public NoParadigma()
            : base("sense paradigma")
        { 
        }

        public override List<Mot> GeneraMots(Entrada entrada, Marques filtre, bool nomesAfixos)
        {
            List<Mot> llista = new List<Mot>();
            llista.Add(new Mot(entrada.Arrel, null, null));
            return llista;
        }

        public override List<ItemDic> GeneraDic(Dictionary<string, string> dades, 
            Dictionary<string, string> excepcions, Marques filtre, Entrada.Speller speller)
        {
            List<ItemDic> llista = new List<ItemDic>();
            llista.Add(new ItemDic(dades["arrel"]));
            return llista;
        }
    }
}
