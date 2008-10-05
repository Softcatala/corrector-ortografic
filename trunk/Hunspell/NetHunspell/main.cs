using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace NetHunspell
{
    public interface ISpellCheck
    {
        /// <summary>
        /// Torna true si la paraula és correcta.
        /// </summary>
        /// <param name="word">La paraula a comprovar.</param>
        /// <returns>true si la paraula és correcta.</returns>
        bool good(string word);

        /// <summary>
        /// Torna suggeriments per a una paraula errada.
        /// </summary>
        /// <param name="word">La paraula per a la qual volem cercar suggeriments.</param>
        /// <returns>Una llista de suggeriments, que eventualment pot ser buida.</returns>
        List<String> sugg(string word);
    }

    public class Hunspell : ISpellCheck, IDisposable
    {

        public Hunspell(string dirDll, string dirDic, string llengua)
        {
            if (_library == IntPtr.Zero)
            {
                string dll = Path.Combine(dirDll, "hunspell.dll");
                if (!File.Exists(dll))
                    throw new Exception("No existeix el fitxer " + Path.GetFullPath(dll));
                _library = LoadLibrary(dll);
            }
            using (AnsiMarshaller aff = new AnsiMarshaller(Path.Combine(dirDic, llengua + ".aff")))
                using (AnsiMarshaller dic = new AnsiMarshaller(Path.Combine(dirDic, llengua + ".dic")))
                {
                    if (!File.Exists(aff.Value))
                        throw new Exception("No existeix el fitxer " + Path.GetFullPath(aff.Value));
                    if (!File.Exists(dic.Value))
                        throw new Exception("No existeix el fitxer " + Path.GetFullPath(dic.Value));
                    _hunspellSession = hunspell_initialize(aff.MarshalledValue, dic.MarshalledValue);
                    if (_hunspellSession == IntPtr.Zero)//review: what would a failed session give us?
                        throw new ApplicationException("Couldn't create hunspell session.");
                }
        }

        ~Hunspell()
        {
            Dispose(false);
        }

        [DllImport("kernel32.dll")]
        public static extern IntPtr LoadLibrary(string lpFileName);

        internal static IntPtr _library;

        private IntPtr _hunspellSession;

        [DllImport("hunspell.dll")]
        private static extern IntPtr hunspell_initialize(IntPtr aff_file, IntPtr dict_file);

        [DllImport("hunspell.dll")]
        private static extern void hunspell_uninitialize(IntPtr hunspellSession);

        private bool _isDisposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;
            if (disposing)
            {
                // release managed resources
            }
            // release unmanaged resources
            hunspell_uninitialize(_hunspellSession);
        }

        //spellcheck word, returns 1 if word ok otherwise 0
        [DllImport("hunspell.dll")]
        private static extern int hunspell_spell(IntPtr hunspellSession, IntPtr word);

        public bool good(string word)
        {
            bool result;
            using (AnsiMarshaller utf8Word = new AnsiMarshaller(word))
            {
                result = (hunspell_spell(_hunspellSession, utf8Word.MarshalledValue) == 1);
            }
            return result;
        }

        ////suggest words for word, returns number of words in slst
        //// YOU NEED TO CALL hunspell_suggest_free after you've done with words
        [DllImport("hunspell.dll")]
        private static extern int hunspell_suggest(IntPtr hunspellSession, IntPtr word, IntPtr slst);

        ////free slst array
        [DllImport("hunspell.dll")]
        private static extern void hunspell_suggest_free(IntPtr hunspellSession, int len, IntPtr slst);

        public List<String> sugg(string word)
        {
            List<String> llista;
            using (AnsiMarshaller utf8word = new AnsiMarshaller(word))
            {
                IntPtr slst = Marshal.AllocHGlobal(IntPtr.Size);
                int nSugg = hunspell_suggest(_hunspellSession, utf8word.MarshalledValue, slst);
                llista = AnsiMarshaller.MarshalFromAnsiArray(Marshal.ReadIntPtr(slst), nSugg);
                hunspell_suggest_free(_hunspellSession, nSugg, slst);
                Marshal.FreeHGlobal(slst);
            }
            return llista;
        }
    }
}
