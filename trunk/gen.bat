echo off

rem Genera fitxers dic i aff

rem Primer, els afixos
perl scripts\genera_aff.pl
if ERRORLEVEL 2 goto FINAL

rem Totes les formes
REM ~ perl scripts\genera_dic.pl -dic=resultats/catalan -aff=resultats/catalan
REM ~ if ERRORLEVEL 2 goto FINAL
perl scripts\gen_ispell.pl -nom=resultats/catalan
if ERRORLEVEL 2 goto FINAL

echo Executau buildhash per generar els fitxers hash
echo a partir de catalan_i.aff i catalan_i.dic.
echo (Per adaptar buildhash per a AbiWord, vegeu el fitxer notes_ispell.txt)

:FINAL