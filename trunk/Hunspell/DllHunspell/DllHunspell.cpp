// NetHunspell.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "DllHunspell.h"

#ifdef _MANAGED
#pragma managed(push, off)
#endif

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
    return TRUE;
}

#ifdef _MANAGED
#pragma managed(pop)
#endif

DLLHUNSPELL_API Hunspell *hunspell_initialize(const char *aff, const char *dic)
{
	return new Hunspell(aff, dic);
}

DLLHUNSPELL_API void hunspell_uninitialize(Hunspell *hunspell)
{
	delete hunspell;
}

DLLHUNSPELL_API int hunspell_spell(Hunspell *hunspell, const char *word)
{
	int resultat = hunspell->spell(word);
	return resultat;
}

DLLHUNSPELL_API int hunspell_suggest(Hunspell *hunspell, const char *word, char ***slst)
{
	char **sugg = 0;
	int quants = hunspell->suggest(&sugg, word);
	*slst = sugg;
	return quants;
}

DLLHUNSPELL_API void hunspell_suggest_free(Hunspell *hunspell, int mida, char ***slst)
{
	for (int i=0; i<mida; i++)
		delete (*slst)[i];
	delete *slst;
}
