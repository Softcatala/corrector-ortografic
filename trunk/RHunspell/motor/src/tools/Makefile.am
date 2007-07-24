bin_PROGRAMS=munch unmunch example hunspell hunmorph hunstem

INCLUDES=-I${top_srcdir}/src/hunspell -I${top_srcdir}/src/parsers

munch_SOURCES=munch.c
unmunch_SOURCES=unmunch.c
include_HEADERS=munch.h unmunch.h

example_SOURCES=example.cxx
example_LDADD = ../hunspell/libhunspell.a

hunspell_SOURCES=hunspell.cxx
hunspell_LDADD = @LIBINTL@ ../hunspell/libhunspell.a \
	../parsers/libparsers.a @CURSESLIB@ @READLINELIB@

hunmorph_SOURCES=hunmorph.cxx
hunmorph_LDADD = ../hunspell/libhunspell.a
#hunmorph_INCLUDES=-I${top_srcdir}/src/hunspell

hunstem_SOURCES=hunstem.cxx
hunstem_LDADD = ../hunspell/libhunspell.a
#hunstem_INCLUDES=-I${top_srcdir}/src/hunspell

EXTRA_DIST=makealias
