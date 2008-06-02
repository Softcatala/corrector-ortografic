using System;
using System.Threading;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.IO;
using Tests;
using xspell;
using catala;

namespace Genera
{
    partial class Principal
    {

        /// <summary>
        /// Crea fitxers .dic i .aff a partir dels fitxers d'entrades.
        /// </summary>
		private void Genera(VFO vfo)
        {
            List<string> log = new List<string>();
            Thread thread = new Thread(new ParameterizedThreadStart(vfo));
            thread.Start(log);
            string linia;
            while (!thread.Join(100))
            {
                while ((linia = LlegeixLiniaLog(log)) != null)
                    logGenera.AppendText(linia);
                Application.DoEvents();
            }
            while ((linia = LlegeixLiniaLog(log)) != null)
                logGenera.AppendText(linia);
        }

        private void GeneraPart(string prefix, Object olog, int mod)
        {
            List<string> log = (List<string>)olog;
            DateTime horaInici = DateTime.Now;
            AfegeixLiniaLog("Genera la llista completa", horaInici, log);
            Regles regles = CarregaRegles(true);
            List<Entrada> entrades = new List<Entrada>();
            Identificador identificador = null;
            identificador = new IdentificadorDIEC("DIEC", regles, DirEntrades("irregulars_diec.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("DIEC2", regles, DirEntrades("irregulars_diec2.txt"));
            AfegeixLiniaLog("Llegeix les entrades del DIEC2", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("diec2.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("més paraules", regles, null);
            AfegeixLiniaLog("Llegeix més entrades tipus DIEC", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("mes_paraules.txt"), entrades, mod);
            identificador = new IdentificadorToponims("topònims", regles);
            AfegeixLiniaLog("Llegeix els topònims", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("topònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("topònims_estrangers.txt"), entrades, mod);
            identificador = new IdentificadorGentilicis("gentilicis", regles, null);
            AfegeixLiniaLog("Llegeix els gentilicis", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("gentilicis.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("gentilicis_estrangers.txt"), entrades, mod);
            identificador = new IdentificadorAntroponims("noms i llinatges", regles, null);
            AfegeixLiniaLog("Llegeix els noms i els llinatges", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("antropònims.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("llinatges.txt"), entrades, mod);
            identificador = new IdentificadorDiversos("diversos", regles);
            AfegeixLiniaLog("Llegeix entrades diverses", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("abreviatures_duarte.txt"), entrades, mod);
            identificador.LlegeixEntrades(DirEntrades("marques.txt"), entrades, mod);
            identificador = new IdentificadorDIEC("AVL", regles, DirEntrades("irregulars_avl.txt"));
            AfegeixLiniaLog("Llegeix les entrades de l'AVL", horaInici, log);
            identificador.LlegeixEntrades(DirEntrades("avl.txt"), entrades, 1);
            foreach (VersioDiccionari versio in VersioDiccionari.Versions())
            {
                string nomFitxer = prefix + versio.Nom;
                AfegeixLiniaLog(String.Format("Genera: {0} (Hunspell)", versio.Descripcio), horaInici, log);
                regles.GeneraAffDicHunspell(DirResultats(nomFitxer), entrades, versio.Filtre, Cat.Cmp);
                AfegeixLiniaLog(String.Format("Genera: {0} (Myspell)", versio.Descripcio), horaInici, log);
                regles.GeneraAffDicMyspell(DirResultats(nomFitxer + ".myspell"), entrades, versio.Filtre, Cat.Cmp, IdentificadorCat.GetAfinaMyspell);
            }
            List<string> excSenseEmprar = identificador.ExcepcionsSenseEmprar();
            if (excSenseEmprar.Count != 0)
                foreach (string exc in excSenseEmprar)
                    AfegeixLiniaLog(string.Format(">>> Excepció no emprada: {0}", exc), horaInici, log);
            AfegeixLiniaLog("Acaba la generació", horaInici, log);
        }

        private void GeneraTot(Object olog)
		{
            GeneraPart("", olog, 1);
        }

        private void GeneraMostra(Object olog)
        {
            GeneraPart("mostra_", olog, 20);
        }

    }
}