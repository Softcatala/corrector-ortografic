// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the RHUNSPELL_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// RHUNSPELL_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef DLLHUNSPELL_EXPORTS
#define DLLHUNSPELL_API __declspec(dllexport)
#else
#define DLLHUNSPELL_API __declspec(dllimport)
#endif

DLLHUNSPELL_API Hunspell *hunspell_initialize(const char *aff, const char *dic);
DLLHUNSPELL_API void hunspell_uninitialize(Hunspell *hunspell);
DLLHUNSPELL_API int hunspell_spell(Hunspell *hunspell, const char *word);
DLLHUNSPELL_API int hunspell_suggest(Hunspell *hunspell, const char *word, char ***slst);
DLLHUNSPELL_API void hunspell_suggest_free(Hunspell *hunspell, int mida, char ***slst);
