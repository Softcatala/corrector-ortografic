using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace xspell
{
    /// <summary>
    /// Una regla per generar formes a partir d'arrels.
    /// També es pot decidir si una forma pertany al conjunt de formes generades.
    /// Està basada en les regles d'ispell.
    /// </summary>
    public class Regla
    {
        /// <summary>
        /// Crea una regla amb un identificador i una descripció
        /// </summary>
        /// <param name="id">L'identificador de la regla. Sol ser un caràcter.</param>
        /// <param name="descripcio">La descripció de la regla.</param>
        /// <param name="sufix">true si la regla és per a sufixos.</param>
        /// <param name="combinable">true si la regla es pot combinar amb una de l'altre tipus
        /// (sufixos amb prefixos i viceversa)</param>
        public Regla(string id, string descripcio, bool sufix, bool combinable)
        {
            this.id = id;
            this.descripcio = descripcio;
            this.sufix = sufix;
            this.combinable = combinable;
            casos = new List<CasRegla>();
            maxGenera = int.MaxValue;
            afix = sufix;
            calCombinable = false;
        }

        /// <summary>
        /// Crea una regla a partir d'una línia del fitxer de regles.
        /// Exemple: "REGLA C SFX+ Pronoms personals enclítics darrere consonant".
        /// Si hi ha problemes, es llança una excepció.
        /// </summary>
        /// <param name="linia">La línia del fitxer.</param>
        /// <returns>La regla tot just creada.</returns>
        static public Regla Crea(string linia)
        {
            Match match = IniciRegla.Match(linia);
            if (!match.Success)
                throw new Exception("Format incorrecte: " + linia);
            string id = match.Groups[1].Value;
            string descripcio = match.Groups[4].Value;
            bool sufix = match.Groups[2].Value == "S";
            bool combinable = match.Groups[3].Value == "+";
            return new Regla(id, descripcio, sufix, combinable);
        }

        /// <summary>
        /// Afegeix un nou cas a la regla.
        /// </summary>
        /// <param name="cas">El cas que volem afegir.</param>
        public void NouCas(CasRegla cas)
        {
            casos.Add(cas);
        }

        /// <summary>
        /// Genera els mots per a una arrel.
        /// </summary>
        /// <param name="arrel">L'arrel que es vol expandir.</param>
        /// <param name="infoComuna">
        /// Informació comuna a tots els mots que es generaran.
        /// Aquesta informació s'unifica amb la que va associada als casos.
        /// Per tant, només es generen els casos que tenen informació compatible.
        /// </param>
        /// <param name="regles">La llista completa de regles.</param>
        /// <param name="filtre">Només es generen mots que tenen marques contingudes en aquest filtre.</param>
        /// <param name="nomesAfixos">Si és true, només s'apliquen les regles amb la propietat EsAfix = true.</param>
        /// <returns>La llista generada de mots.</returns>
        public List<Mot> Genera(string arrel, MorfoGram infoComuna, Regles regles, Marques filtre, bool nomesAfixos)
        {
            List<Mot> mots = new List<Mot>();
            if (nomesAfixos && !EsAfix)
                return mots;
            int afegits = 0;
            foreach (CasRegla cas in casos)
            {
                if (!MorfoGram.Unificables(cas.Info, infoComuna) || !filtre.Conte(cas.Marca))
                    continue;
                afegits += cas.Genera(arrel, ref mots, infoComuna, regles, filtre, nomesAfixos);
                if (afegits >= maxGenera)
                        break;
            }
            return mots;
        }

        /// <summary>
        /// Diu si algun cas de la regla és aplicable a str.
        /// ("aplicable" vol dir que es poden derivar formes a partir de str).
        /// </summary>
        /// <param name="str">L'arrel de la qual volem saber si li és aplicable la regla.</param>
        /// <returns>true si la regla és aplicable a str.</returns>
        public bool EsAplicable(string str)
        {
            foreach (CasRegla cas in casos)
                if (cas.EsAplicable(str))
                    return true;
            return false;
        }

        public string Id { get { return id; } }
        public bool EsSufix { get { return sufix; } }
        public bool EsCombinable { get { return combinable; } }
        public List<CasRegla> Casos { get { return casos; } }

        /// <summary>
        /// Quants de casos s'han de generar com a màxim.
        /// Serveix per evitar llistes massa llargues.
        /// </summary>
        public int MaxGenera { set { maxGenera = value; } }

        /// <summary>
        /// És true si aquesta regla afegeix informació morfogramatical.
        /// Per defecte, és fals per als prefixos i vertader per als sufixos.
        /// </summary>
        public bool EsAfix { get { return afix; } set { afix = value; } }

        public string Descripcio { get { return descripcio; } }

        public override string ToString()
        {
            return descripcio;
        }

        /// <summary>
        /// Genera les línies per al fitxer .aff
        /// </summary>
        /// <param name="filtre">Només posarem els casos amb marques contingudes dins el filtre.</param>
        public string[] GeneraAff(Marques filtre, Regles regles)
        {
            return GeneraAff(filtre, id, combinable, regles);
        }

        /// <summary>
        /// Genera les línies per al fitxer .aff
        /// </summary>
        /// <param name="filtre">Només posarem els casos amb marques contingudes dins el filtre.</param>
        /// <param name="nouId">L'identificador que farem servir per a la regla.</param>
        /// <param name="nouCombinable">El valor nou per a combinable.</param>
        /// <param name="regles">Les regles a les quals pertany aquesta.</param>
        public string[] GeneraAff(Marques filtre, string nouId, bool nouCombinable, Regles regles)
        {
            List<string> liniesCasos = new List<string>();
            foreach (CasRegla cas in casos)
                if (filtre.Conte(cas.Marca))
                    liniesCasos.Add(cas.GeneraAff(this, nouId, regles));
            List<string> linies = new List<string>();
            linies.Add(string.Format("{0}FX {1} {2} {3}", sufix ? "S" : "P", nouId, nouCombinable ? "Y" : "N", liniesCasos.Count));
            linies.AddRange(liniesCasos);
            return linies.ToArray();
        }

        /// <summary>
        /// Genera una llista de GrupMyspell
        /// </summary>
        public List<ReglaMyspell> GrupsMyspell(Regles regles, Marques filtre, GetAfinaReglaMyspell getAfina)
        {
            List<ReglaMyspell> grups = new List<ReglaMyspell>();
            foreach (CasRegla cas in casos)
            {
                if (!filtre.Conte(cas.Marca))
                    continue;
                ReglaMyspell grup = grups.Find(delegate(ReglaMyspell g) { return g.CasPertany(cas); });
                if (grup == null)
                {
                    grup = new ReglaMyspell(this, regles, getAfina);
                    grups.Add(grup);
                }
                grup.NouCas(cas);
            }
            return grups;
        }

        /// <summary>
        /// Necessitam la versió combinable d'aquesta regla perquè la feim servir
        /// dins el cas d'una regla.
        /// Per defecte té el valor false. Només es pot posar a true.
        /// </summary>
        public bool CalCombinable 
        { 
            get { return calCombinable; } 
            set { calCombinable |= value; }  
        }

        /// <summary>
        /// Calcula els valors adequats de CalCombinable per a les regles que calgui.
        /// </summary>
        /// <param name="regles">Totes les regles.</param>
        public void CalculaCalCombinable(Regles regles)
        {
            foreach (CasRegla cas in casos)
            {
                List<string> mesRegles = cas.MesRegles;
                if (mesRegles != null)
                    foreach (string idRegla in cas.MesRegles)
                    {
                        Regla regla = regles.Llista[idRegla];
                        if (!regla.EsCombinable && !regla.CalCombinable)
                            regla.CalCombinable = true;
                    }
            }
        }

        /// <summary>
        /// L'identificador de la versió combinable d'aquesta regla.
        /// </summary>
        public string IdCombinable { get { return calCombinable ? id.ToLower() : id; } }

        private string id;
        private string descripcio;
        private bool sufix;
        private bool combinable;
        private bool afix;
        private bool calCombinable;
        private List<CasRegla> casos;
        private int maxGenera;
        private static Regex IniciRegla = new Regex(@"^REGLA\s+(\w)\s+([PS])FX(\+?)\s+(.*)");
    }

    /// <summary>
    /// Grup de casos amb comportament similar pel que fa a MySpell.
    /// És un subconjunt de casos d'una regla.
    /// Dins cada grup hi ha els casos que tenen les mateixes regles extra.
    /// </summary>
    public class ReglaMyspell
    {
        public ReglaMyspell(Regla reglaOriginal, Regles regles, GetAfinaReglaMyspell getAfinaGrup)
        {
            this.reglaOriginal = reglaOriginal;
            this.regles = regles;
            casos = new List<CasRegla>();
            afixosContraris = new List<Regla>();
            afixosIguals = new List<Regla>();
            getAfinador = getAfinaGrup;
            afinador = null;
            regla = null;
        }

        private Regla Crea()
        {
            regla = new Regla(Id, Descripcio, reglaOriginal.EsSufix, reglaOriginal.EsCombinable);
            regla.EsAfix = reglaOriginal.EsAfix;
            foreach (CasRegla cas in casos)
            {
                CasRegla nou = new CasRegla(regla, cas.Suprimir, cas.Afegir, cas.Condicio,
                    reglaOriginal.EsSufix, cas.Marca, cas.Info, null);
                regla.NouCas(nou);
            }
            return regla;
        }

        public Regla ReglaOriginal { get { return reglaOriginal; } }

        public Regla Regla { get { if (regla == null) regla = Crea(); return regla; } }

        public Regles Regles { get { return regles; } }

        public int NCasos { get { return casos.Count; } }

        public List<CasRegla> Casos { get { return casos; } }

        public bool CasPertany(CasRegla cas)
        {
            return cas.MateixesMes(casos[0]);
        }

        public bool EsRegla { get { return (afixosIguals.Count == 0) && (casos.Count > 1); } }

        public void NouCas(CasRegla cas)
        {
            if (casos.Count == 0)
            {
                afinador = getAfinador(reglaOriginal, cas);
                if (afinador != null)
                    cas = afinador.ProcessaCas(this, cas);
                if (cas.MesRegles != null)
                {
                    foreach (string id in cas.MesRegles)
                    {
                        Regla mes = regles.Llista[id];
                        if (reglaOriginal.EsSufix == mes.EsSufix)
                            afixosIguals.Add(mes);
                        else
                            afixosContraris.Add(mes);
                    }
                }
                casos.Add(cas);
            }
            else if (afinador != null)
                casos.Add(afinador.ProcessaCas(this, cas));
            else
                casos.Add(cas);
        }

        public AfinaReglaMyspell Afinador { get { return afinador; } }
        public List<Regla> AfixosContraris { get { return afixosContraris; } }
        public List<Regla> AfixosIguals { get { return afixosIguals; } }
        public string Id { get { return id; } set { id = value; } }
        public string Descripcio { get { return descripcio; } set { descripcio = value; } }

        private Regla reglaOriginal;
        private Regla regla;
        private Regles regles;
        private List<CasRegla> casos;
        private List<Regla> afixosContraris;
        private List<Regla> afixosIguals;
        private GetAfinaReglaMyspell getAfinador;
        private AfinaReglaMyspell afinador;
        private string id;
        private string descripcio;
    }

    /// <summary>
    /// Classe per crear una línia de diccionari per a Myspell, a partir d'un ItemDic.
    /// Cada objecte està especialitzat en una seqüència de flags i crea un ItemDic a partir
    /// d'un ItemDoc vell.
    /// </summary>
    public abstract class CreaDicMyspell
    {
        /// <summary>
        /// Crea un ItemDic equivalent a itemVell.
        /// </summary>
        /// <param name="itemVell">L'ítem que volem convertir.</param>
        /// <returns>Un nou ítem vell.</returns>
        public abstract ItemDic Converteix(ItemDic itemVell);

        protected CreaDicMyspell()
        {
            converteixArrel = null;
            nousFlags = new List<string>();
        }

        public static List<CreaDicMyspell> GeneraCreadors(ItemDic itemMostra,
            Dictionary<string, List<ReglaMyspell>> vellANou, Regles regles)
        {
            List<CreaDicMyspell> creadors = new List<CreaDicMyspell>();
            // Feim la llista de regles que han d'aparèixer a tots els creadors
            List<string> reglesComunes = new List<string>();
            foreach (string idRegla in itemMostra.LlistaFlags)
            {
                if (vellANou[idRegla].Count == 0) throw new Exception("Regla sense equivalència: " + idRegla);
                if (vellANou[idRegla].Count == 1 && vellANou[idRegla][0].EsRegla)
                    reglesComunes.Add(vellANou[idRegla][0].Id);
            }
            // Cream regles per als altres flags
            foreach (string idRegla in itemMostra.LlistaFlags)
            {
                foreach (ReglaMyspell regla in vellANou[idRegla])
                {
                    if (regla.EsRegla)
                    {
                        CreaDicMyspellRegla creador = new CreaDicMyspellRegla(regles);
                        creador.MesFlags(reglesComunes);
                        creador.MesFlags(regla.Id);
                        creador.ConverteixArrel = regla.Afinador;
                        foreach (Regla contraria in regla.AfixosContraris)
                            foreach (ReglaMyspell nova in vellANou[contraria.Id])
                                creador.MesFlags(nova.Id);
                        creadors.Add(creador);
                    }
                    else
                    {
                        foreach (CasRegla cas in regla.Casos)
                        {
                            CreaDicMyspellCas creador = new CreaDicMyspellCas(cas, regles);
                            creador.MesFlags(reglesComunes);
                            creador.ConverteixArrel = regla.Afinador;
                            foreach (Regla igual in regla.AfixosIguals)
                                foreach (ReglaMyspell nova in vellANou[igual.Id])
                                    creador.MesFlags(nova.Id); ;
                            foreach (Regla contraria in regla.AfixosContraris)
                                foreach (ReglaMyspell nova in vellANou[contraria.Id])
                                    creador.MesFlags(nova.Id); ;
                            creadors.Add(creador);
                        }
                    }
                }
            }
            // Si no hem afegit cap creador, tornam el vell
            if (creadors.Count == 0) 
            {
                CreaDicMyspellRegla creador = new CreaDicMyspellRegla(regles);
                creador.MesFlags(reglesComunes);
                creadors.Add(creador);
            }
            //
            return creadors;
        }

        public void MesFlags(string flag)
        {
            nousFlags.Add(flag);
        }

        public void MesFlags(List<string> flags)
        {
            nousFlags.AddRange(flags);
        }

        protected string Arrel(ItemDic itemVell)
        {
            string arrel = itemVell.Arrel;
            if (converteixArrel != null)
                arrel = converteixArrel.ProcessaArrel(arrel);
            return arrel;
        }

        public AfinaReglaMyspell ConverteixArrel
        {
            get { return converteixArrel; }
            set
            {
                if (converteixArrel != null) throw new Exception("No es pot redefinir ConverteixArrel");
                converteixArrel = value;
            }
        }

        protected AfinaReglaMyspell converteixArrel;
        protected List<string> nousFlags;

    }

    public class CreaDicMyspellRegla : CreaDicMyspell
    {
        override public ItemDic Converteix(ItemDic itemVell)
        {
            string arrel = Arrel(itemVell);
            ItemDic nou = new ItemDic(arrel);
            foreach (string idRegla in nousFlags)
                if (regles.Llista[idRegla].EsAplicable(arrel))
                    nou.MesFlags(idRegla);
            return nou;
        }

        public CreaDicMyspellRegla(Regles regles)
        {
            this.regles = regles;
        }

        private Regles regles;
    }

    public class CreaDicMyspellCas : CreaDicMyspell
    {
        public override ItemDic Converteix(ItemDic itemVell)
        {
            string arrel = Arrel(itemVell);
            if (!cas.EsAplicable(arrel))
                return null;
            List<Mot> mots = new List<Mot>();
            if (cas.Genera(arrel, ref mots, null, regles, Marques.totes, true) != 1) throw new Exception("S'esperava un mot");
            ItemDic nou = new ItemDic(mots[0].Forma);
            foreach (string idRegla in nousFlags)
                if (regles.Llista[idRegla].EsAplicable(nou.Arrel))
                    nou.MesFlags(idRegla);
            return nou;
        }

        public CreaDicMyspellCas(CasRegla cas, Regles regles)
        {
            this.cas = cas;
            this.regles = regles;
        }

        private CasRegla cas;
        private Regles regles;
    }

    /// <summary>
    /// Funció que retorna els afinadors per als grups MySpell.
    /// <param name="reglaBase">La regla de la qual s'extreu aquest grup.</param>
    /// <param name="cas">El primer cas que es passa al grup.</param>
    /// </summary>
    public delegate AfinaReglaMyspell GetAfinaReglaMyspell(Regla reglaBase, CasRegla cas);

    /// <summary>
    /// Afinador d'objectes GrupMyspell.
    /// En alguns moments del processament que fa GrupMyspell es criden mètodes d'aquesta
    /// classe. L'objectiu és poder fer ajustaments dependents de la llengua (que per això
    /// no poden estar posats aquí).
    /// </summary>
    public abstract class AfinaReglaMyspell
    {

        /// <summary>
        /// Processa un cas abans que s'afegesqui al grup.
        /// </summary>
        /// <param name="cas">El cas que està a punt d'afegir-se al grup.</param>
        /// <returns>El cas, eventualment modificat.</returns>
        abstract public CasRegla ProcessaCas(ReglaMyspell grup, CasRegla cas);

        /// <summary>
        /// Processa una arrel abans de ser afegida a un ítem.
        /// Serveix per modificar l'arrel per reflectir els canvis dels casos.
        /// </summary>
        /// <param name="grup">El grup a què pertany l'arrel.</param>
        /// <param name="arrel">L'arrel que es vol processar.</param>
        /// <returns>L'arrel eventualment modificada.</returns>
        abstract public string ProcessaArrel(string arrel);
    }
}
