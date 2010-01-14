# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
gen = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
avl = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p gen, avl

mots = []
tros = <<-FITROS
Propostes de correcció
El corrector reconeix com a bones les formes: *nann i *instal·l, que són incorrectes. M'he mirat el codi i crec que el problema es troba a les següents línies del fitxer regles.txt:
El corrector dóna com a bones les formes incorrectes: *espaií, *remeií, *cruií, *enjoií. 

Propostes d'inclusió
Dusseldorf, Magdeburg, Marburg, Gelsenkirchen, Aquisgrà, Saarbrücken, Trèveris, Volga, Línux (?) i un llarg etcètera de noms propis. La quantitat de noms és enorme i suposo que ja s'està treballant mica en mica a incloure'n més. Crec que llistes com aquesta, aquesta o, en general, les llistes d'aquesta categoria poden ajudar a ampliar ràpidament (i controlada) aquest punt. De fet, un bot (supervisat) també podria fer un escaneig de les categories (de ciutats, estats, rius...) de la Viquipèdia i fer-ne una llista. 13:23, 3 set 2008 
Molts adverbis acabats en -ment, que tot i no ser al DIEC, són correctes. Exemples: simplificadament, discutidament, controladament... 13:23, 3 set 2008 
Diminutius, augmentatius i derivats: petitó/na/ns/nes, miqueta/es, petitet/a/s/es, petonet/s, abraçadeta/es, abraçadota, pantalleta, llibrot, casota, bosseta, finestrota, cadirota, monedeta/es/ota/otes, guantet, caminet i un llarguet etc. 13:23, 3 set 2008 
Marques: Microsoft, Windows, NIKE, Addidas, iPod, Apple, ... Aquest punt pot ser controvertit: estan incloses als diccionaris alemany i anglès, i no ho estan per exemple a l'italià. Jo crec que en general és interessant incloure-les. Si s'inclou (per exemple) Microsoft al diccionari, difícilment marcarà falsos positius (és a dir, difícilment ens trobarem en el cas de voler escriure una paraula en català (que no sigui "Microsoft") tal que quan ens equivoquem, escrivim "Microsoft" per error i per tant el diccionari no ens l'assenyala com a incorrecta, tot i que ho hauria de fer. En canvi, crec molt possible que molta gent faci un error tipogràfic i escrigui "*Micriosoft", de tal manera que el corrector en aquest cas sí que faci servei. És a dir, veig més avantatges en incloure-la, que no pas els falsos positius que pugui donar. Tanmateix, s'hauria d'avaluar cas per cas. Per exemple "Chupa" (de Chupa Chups) no la inclouria, ja que sí que és probable que algú s'equivoqui i escrigui Chupa quan vol escriure Xupa. 13:23, 3 set 2008 
Crec que només a la versió valenciana caldria afegir els adverbis: ahí, mentres, aïna, aïnes (potser també general)  02:51, 5 nov 2008 
Crec que només a la versió valenciana, les formes que falten amb o del verb vore.
Potser només a la valenciana: òbric
Potser només a la valenciana: òmplic òmpliga òmpligues òmpliga òmpliguen
Més vocabulari de l'àmbit valencià. Entre parèntesis si es troba també al GDLC  02:51, 5 nov 2008 
desvanit, desvanida
destrellat 
gesmil, gesmiler 
baül 
marmolar i marmoló
albergina, alberginera (general: albergínia, alberginiera)
renyó, renyons (general: ronyó, ronyons) 
pantaix, pantaixar (general: panteix) 
febra (general: febre) 
depòsit (general: dipòsit) 
tio (general: oncle)
vivenda
bambolla 
arredonir 
arxipèleg
bàlsem
clevill 
estómec
màsquera
nàufreg
alfàbega
desorde 
hòmens
jóvens
còvens
màrgens
órdens
òrfens
òrguens
ràvens
térmens
vèrgens
despús-ahir 
despús-anit 
despús-demà 
bollir
engrunsar-se
gronsar-se 
espentar 
tòs
xàrcia 
gemecar 
escopinyar
desnugar 
dessubstanciat
destrellatat 
encanar-se
escabussar 
escabussó 
escopinyada
puntelló 
regallar 
roín 
voladoret
ximenera
subsecretari
vicesecretari 
estall 
peluca, peluqueria, peluquer
 14:19, 17 des 2008 

Verb reeixir en formes valencianes mirar del verb eixir i adaptar-lo amb dièresi. reïsca, reïsquera...  13:39, 17 des 2008 
Versió del corrector de 19.oct.08
Incorpora aquestes propostes:

(Del DIEC2): Conjugació haver (2), falta quasi tota (haguem, hagueu, hegui, heguin, heguis, ...)
(Del DIEC2): Conjugació haver (1): hàgem, hàgeu... 13:23, 3 set 2008 
Essen, Duisburg, Dortmund, Stuttgart, Linux.
Nom propi català: Frederic. 20:17, 16 set 2008 
Molts adverbis acabats en -ment, que tot i no ser al DIEC, són correctes. Exemples: majoritàriament 13:23, 3 set 2008 
Viquipèdia, Viccionari, Viquillibres, Softcatalà 13:23, 3 set 2008 
Com a mínim per a la versió valenciana, caldria afegir les formes pròpies de verbs incoatius com aquests:  11:56, 9 oct 2008 
llegir: llig, lliges, llig, lligen; llija... Conjugació (llig es marca correcte ara per la forma balear de lligar -> lligo en central)
fregir: frig, friges, frig, frigen; frija... Conjugació
teixir: tix, tixes, tix, tixen; tixa... Conjugació
afegir: afig, afiges, afig, afigen; afija... Conjugació
engolir: engul, enguls, engul, engulen; engula... Conjugació
renyir: riny, rinys, riny; rinya... Conjugació
tenyir: tiny, tinys, tiny; tinya... Conjugació (tinya es marca correcte ara per la referència a la patologia i no al verb tenyir)
Com a mínim per a la versió valenciana caldria afegir l'infinitiu alternatiu de veure: vore.  12:06, 9 oct 2008 
Com a mínim per a la versió valenciana caldria afegir les formes de present de subjuntiu següents:  12:06, 9 oct 2008 
saber: sàpia, sàpies, sàpia, sapiem, sapieu, sàpien Conjugació
caber: càpia, càpies, càpia, capiem, capieu, càpien Conjugació
obrir: òbriga, òbrigues, òbriga, obrim, obriu, òbriguen Conjugació (obrim i obriu ja hi són)
Com a mínim per a la versió valenciana caldria afegir els participis següents:  18:53, 15 oct 2008 
oferit/oferida/oferits/oferides (oferir)
sofrit/sofrida/sofrits/sofrides (sofrir)
suplit/suplida/suplits/suplides (suplir)
reblit/reblida/reblits/reblides (reblir)
plangut/planguda/planguts/plangudes (plànyer)
fengut/fenguda/fenguts/fengudes (fényer)
rist/rista/ristos/ristes (riure)
Com a mínim per a la versió valenciana caldria afegir els noms de les lletres següents:  13:57, 10 oct 2008 
ef, efe, ele, eme, ene, er, esse (veure GNV)
FITROS

absents = {}
absents.default = 0
tros.split(/[ ,;:.\/"²()—>\[\]\r\n\t]/).each do |mot|
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