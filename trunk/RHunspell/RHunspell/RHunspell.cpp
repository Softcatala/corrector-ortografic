// RHunspell.cpp : Defines the entry point for the DLL application.
//

#include "stdafx.h"
#include "RHunspell.h"

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

// Inicialitza una instància de XHunspell
static VALUE xhunspell_initialize(VALUE self, VALUE aff, VALUE dic)
{
	return Qtrue;
}

// Destructor per a la classe XHunspell
void xhunspell_free(Hunspell *hs)
{
	delete hs;
}

// Constructor per a la classe XHunspell
static VALUE xhunspell_new(VALUE classe, VALUE aff, VALUE dic)
{
	const char *strAff = STR2CSTR(aff);
	const char *strDic = STR2CSTR(dic);
	Hunspell *hs = new Hunspell(strAff, strDic);
	VALUE xhunspell = Data_Wrap_Struct(classe, 0, xhunspell_free, hs);
	return xhunspell;
}

// Torna Qtrue si la paraula és correcta
// Torna QFalse si la paraula és incorrecta
static VALUE xhunspell_OK(VALUE self, VALUE mot)
{
	Hunspell *hs;
	Data_Get_Struct(self, Hunspell, hs);
	const char *strMot = STR2CSTR(mot);
	int spell = hs->spell(strMot);
	return (spell == 0) ? Qfalse : Qtrue;
}

// Cerca suggeriments per reemplaçar una paraula
// si no troba suggeriments, torna un array buit
static VALUE xhunspell_sugg(VALUE self, VALUE mot)
{
	Hunspell *hs;
	Data_Get_Struct(self, Hunspell, hs);
	const char *strMot = STR2CSTR(mot);
	char **slst;
	int nSugg = hs->suggest(&slst, strMot);
	VALUE sugg = rb_ary_new2(nSugg);
	for (int i=0; i<nSugg; i++)
		rb_ary_store(sugg, i, rb_str_new2(slst[i]));
	return sugg;
}

// Torna la informació morfològica d'una paraula
static VALUE xhunspell_morph(VALUE self, VALUE mot)
{
	Hunspell *hs;
	Data_Get_Struct(self, Hunspell, hs);
	const char *strMot = STR2CSTR(mot);
	char *m = hs->morph(strMot);
	if (m) {
		VALUE morph = rb_str_new2(m);
		delete m;
		return morph;
	}
	else
		return Qnil;
}


// Crea la classe Hunspell
RHUNSPELL_API void Init_xhunspell(void)
{
	VALUE XHunspell = rb_define_class("XHunspell", rb_cObject);
	rb_define_singleton_method(XHunspell, "new", (VALUE(*)(...)) xhunspell_new, 2);
	rb_define_method(XHunspell, "initialize", (VALUE(*)(...)) xhunspell_initialize, 2);
	rb_define_method(XHunspell, "OK", (VALUE(*)(...)) xhunspell_OK, 1);
	rb_define_method(XHunspell, "sugg", (VALUE(*)(...)) xhunspell_sugg, 1);
	rb_define_method(XHunspell, "morph", (VALUE(*)(...)) xhunspell_morph, 1);
}
