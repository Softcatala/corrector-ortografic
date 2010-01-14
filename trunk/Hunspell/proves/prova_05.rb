# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
gen = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
avl = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p gen, avl

mots = []
tros = <<-FITROS
Propostes de correcci�
El corrector reconeix com a bones les formes: *nann i *instal�l, que s�n incorrectes. M'he mirat el codi i crec que el problema es troba a les seg�ents l�nies del fitxer regles.txt:
El corrector d�na com a bones les formes incorrectes: *espai�, *remei�, *crui�, *enjoi�. 

Propostes d'inclusi�
Dusseldorf, Magdeburg, Marburg, Gelsenkirchen, Aquisgr�, Saarbr�cken, Tr�veris, Volga, L�nux (?) i un llarg etc�tera de noms propis. La quantitat de noms �s enorme i suposo que ja s'est� treballant mica en mica a incloure'n m�s. Crec que llistes com aquesta, aquesta o, en general, les llistes d'aquesta categoria poden ajudar a ampliar r�pidament (i controlada) aquest punt. De fet, un bot (supervisat) tamb� podria fer un escaneig de les categories (de ciutats, estats, rius...) de la Viquip�dia i fer-ne una llista. 13:23, 3 set 2008 
Molts adverbis acabats en -ment, que tot i no ser al DIEC, s�n correctes. Exemples: simplificadament, discutidament, controladament... 13:23, 3 set 2008 
Diminutius, augmentatius i derivats: petit�/na/ns/nes, miqueta/es, petitet/a/s/es, petonet/s, abra�adeta/es, abra�adota, pantalleta, llibrot, casota, bosseta, finestrota, cadirota, monedeta/es/ota/otes, guantet, caminet i un llarguet etc. 13:23, 3 set 2008 
Marques: Microsoft, Windows, NIKE, Addidas, iPod, Apple, ... Aquest punt pot ser controvertit: estan incloses als diccionaris alemany i angl�s, i no ho estan per exemple a l'itali�. Jo crec que en general �s interessant incloure-les. Si s'inclou (per exemple) Microsoft al diccionari, dif�cilment marcar� falsos positius (�s a dir, dif�cilment ens trobarem en el cas de voler escriure una paraula en catal� (que no sigui "Microsoft") tal que quan ens equivoquem, escrivim "Microsoft" per error i per tant el diccionari no ens l'assenyala com a incorrecta, tot i que ho hauria de fer. En canvi, crec molt possible que molta gent faci un error tipogr�fic i escrigui "*Micriosoft", de tal manera que el corrector en aquest cas s� que faci servei. �s a dir, veig m�s avantatges en incloure-la, que no pas els falsos positius que pugui donar. Tanmateix, s'hauria d'avaluar cas per cas. Per exemple "Chupa" (de Chupa Chups) no la inclouria, ja que s� que �s probable que alg� s'equivoqui i escrigui Chupa quan vol escriure Xupa. 13:23, 3 set 2008 
Crec que nom�s a la versi� valenciana caldria afegir els adverbis: ah�, mentres, a�na, a�nes (potser tamb� general)  02:51, 5 nov 2008 
Crec que nom�s a la versi� valenciana, les formes que falten amb o del verb vore.
Potser nom�s a la valenciana: �bric
Potser nom�s a la valenciana: �mplic �mpliga �mpligues �mpliga �mpliguen
M�s vocabulari de l'�mbit valenci�. Entre par�ntesis si es troba tamb� al GDLC  02:51, 5 nov 2008 
desvanit, desvanida
destrellat 
gesmil, gesmiler 
ba�l 
marmolar i marmol�
albergina, alberginera (general: alberg�nia, alberginiera)
reny�, renyons (general: rony�, ronyons) 
pantaix, pantaixar (general: panteix) 
febra (general: febre) 
dep�sit (general: dip�sit) 
tio (general: oncle)
vivenda
bambolla 
arredonir 
arxip�leg
b�lsem
clevill 
est�mec
m�squera
n�ufreg
alf�bega
desorde 
h�mens
j�vens
c�vens
m�rgens
�rdens
�rfens
�rguens
r�vens
t�rmens
v�rgens
desp�s-ahir 
desp�s-anit 
desp�s-dem� 
bollir
engrunsar-se
gronsar-se 
espentar 
t�s
x�rcia 
gemecar 
escopinyar
desnugar 
dessubstanciat
destrellatat 
encanar-se
escabussar 
escabuss� 
escopinyada
puntell� 
regallar 
ro�n 
voladoret
ximenera
subsecretari
vicesecretari 
estall 
peluca, peluqueria, peluquer
 14:19, 17 des 2008 

Verb reeixir en formes valencianes mirar del verb eixir i adaptar-lo amb di�resi. re�sca, re�squera...  13:39, 17 des 2008 
Versi� del corrector de 19.oct.08
Incorpora aquestes propostes:

(Del DIEC2): Conjugaci� haver (2), falta quasi tota (haguem, hagueu, hegui, heguin, heguis, ...)
(Del DIEC2): Conjugaci� haver (1): h�gem, h�geu... 13:23, 3 set 2008 
Essen, Duisburg, Dortmund, Stuttgart, Linux.
Nom propi catal�: Frederic. 20:17, 16 set 2008 
Molts adverbis acabats en -ment, que tot i no ser al DIEC, s�n correctes. Exemples: majorit�riament 13:23, 3 set 2008 
Viquip�dia, Viccionari, Viquillibres, Softcatal� 13:23, 3 set 2008 
Com a m�nim per a la versi� valenciana, caldria afegir les formes pr�pies de verbs incoatius com aquests:  11:56, 9 oct 2008 
llegir: llig, lliges, llig, lligen; llija... Conjugaci� (llig es marca correcte ara per la forma balear de lligar -> lligo en central)
fregir: frig, friges, frig, frigen; frija... Conjugaci�
teixir: tix, tixes, tix, tixen; tixa... Conjugaci�
afegir: afig, afiges, afig, afigen; afija... Conjugaci�
engolir: engul, enguls, engul, engulen; engula... Conjugaci�
renyir: riny, rinys, riny; rinya... Conjugaci�
tenyir: tiny, tinys, tiny; tinya... Conjugaci� (tinya es marca correcte ara per la refer�ncia a la patologia i no al verb tenyir)
Com a m�nim per a la versi� valenciana caldria afegir l'infinitiu alternatiu de veure: vore.  12:06, 9 oct 2008 
Com a m�nim per a la versi� valenciana caldria afegir les formes de present de subjuntiu seg�ents:  12:06, 9 oct 2008 
saber: s�pia, s�pies, s�pia, sapiem, sapieu, s�pien Conjugaci�
caber: c�pia, c�pies, c�pia, capiem, capieu, c�pien Conjugaci�
obrir: �briga, �brigues, �briga, obrim, obriu, �briguen Conjugaci� (obrim i obriu ja hi s�n)
Com a m�nim per a la versi� valenciana caldria afegir els participis seg�ents:  18:53, 15 oct 2008 
oferit/oferida/oferits/oferides (oferir)
sofrit/sofrida/sofrits/sofrides (sofrir)
suplit/suplida/suplits/suplides (suplir)
reblit/reblida/reblits/reblides (reblir)
plangut/planguda/planguts/plangudes (pl�nyer)
fengut/fenguda/fenguts/fengudes (f�nyer)
rist/rista/ristos/ristes (riure)
Com a m�nim per a la versi� valenciana caldria afegir els noms de les lletres seg�ents:  13:57, 10 oct 2008 
ef, efe, ele, eme, ene, er, esse (veure GNV)
FITROS

absents = {}
absents.default = 0
tros.split(/[ ,;:.\/"�()�>\[\]\r\n\t]/).each do |mot|
	next if mot =~ /^[-?]/
	next if mot =~ /^[-0-9%]+$/
	next if mot =~ /DGLC|oct|nov/
	if mot =~ /^\*(.*)/
		mot = $1
		next unless avl.OK(mot)
		absents["*#{mot}"] += 1
	else
		next if avl.OK(mot)
		absents[mot] += 1
	end
end
absents.keys.sort.each do |mot|
	puts mot
end