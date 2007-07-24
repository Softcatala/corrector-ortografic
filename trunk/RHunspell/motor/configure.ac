#                                               -*- Autoconf -*-
# Process this file with autoconf to produce a configure script.

m4_pattern_allow


AC_PREREQ(2.59)
AC_INIT([hunspell],[1.1.4],[nemethl@gyorsposta.hu])

AC_CANONICAL_SYSTEM
case "$host" in
arm*) XFAILED="flagutf8.test maputf.test";;
esac
AC_SUBST(XFAILED)

AM_INIT_AUTOMAKE(hunspell, 1.1.4)

AC_CONFIG_SRCDIR([config.h.in])
AC_CONFIG_HEADER([config.h])



# Checks for programs.
AC_PROG_CXX
AC_PROG_CC
AC_PROG_RANLIB

# Checks for libraries.

# Checks for header files.

AC_CHECK_HEADERS([fcntl.h libintl.h locale.h unistd.h error.h])

# Checks for typedefs, structures, and compiler characteristics.
AC_C_CONST
AC_C_INLINE

# Checks for library functions.
AC_FUNC_ERROR_AT_LINE
AC_FUNC_MALLOC
AC_HEADER_STDC
AC_CHECK_FUNCS([memchr setlocale strchr strstr])

dnl internationalization macros
AM_GNU_GETTEXT

CURSESLIB=""
AC_ARG_WITH(ui,[  --with-ui         support Curses user interface], [
   AC_CHECK_LIB(ncursesw,tparm,CURSESLIB=-lncursesw,
     AC_CHECK_LIB(curses,tparm,CURSESLIB=-lcurses,
       AC_CHECK_LIB(ncurses,tparm,CURSESLIB=-lncurses)))
   if test "$CURSESLIB" != "" ; then
     echo Compiling with curses user interface.
     AC_DEFINE(HAVE_CURSES_H,1,"Define if you have the <curses.h> header")
     if test "$CURSESLIB" != "-lncursesw" ; then
       echo "No Unicode support on interactive console. (Install Ncursesw library.)"
     else
       AC_DEFINE(HAVE_NCURSESW_H,1,"Define if you have the <ncursesw/curses.h> header")       
     fi
   fi
])

AC_SUBST(CURSESLIB)

rl=n
AC_ARG_WITH(readline,[  --with-readline         support fancy command input editing], [
   AC_CHECK_LIB(curses,tparm,TERMLIB=-lncurses,
     AC_CHECK_LIB(termcap,tgetent,TERMLIB=-ltermcap))
   LDSAVE=$LDFLAGS
   LDFLAGS="$LDFLAGS $TERMLIB"
   AC_CHECK_LIB(readline,readline,
      [AC_CHECK_HEADER(readline/readline.h,
      READLINELIB="-lreadline $TERMLIB";rl=y)],
      READLINELIB="")
   if test "$rl" = "y" ; then
     echo Using the readline library.
     AC_DEFINE(HAVE_READLINE,1,"Define if you have fancy command input editing with Readline")
   fi
   LDFLAGS=$LDSAVE
])

AC_SUBST(READLINELIB)

AC_CONFIG_FILES([Makefile
                 man/Makefile
                 man/hu/Makefile
                 intl/Makefile
                 po/Makefile.in
                 m4/Makefile
		 src/Makefile
		 src/hunspell/Makefile
		 src/parsers/Makefile
		 src/tools/Makefile
                 tests/Makefile])
AC_OUTPUT
