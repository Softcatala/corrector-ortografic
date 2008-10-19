# Prova el corrector avl
require 'hunspell'

BASE = '../../resultats/hunspell'
hs = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p hs

mots = []
mots += %w{
	saber s�pia s�pies s�pia sapiem sapieu s�pien
	caber c�pia c�pies c�pia capiem capieu c�pien
	obrir �briga �brigues �briga obrim obriu �briguen
	oferit oferida oferits oferides  oferir 
	sofrit sofrida sofrits sofrides  sofrir 
	suplit suplida suplits suplides  suplir 
	reblit reblida reblits reblides  reblir 
	plangut planguda planguts plangudes  pl�nyer 
	fengut fenguda fenguts fengudes  f�nyer 
	rist rista ristos ristes  riure  
	adm�s apar�ixer apreh�s apreh�n arrem�s at�nyer com�s compar�ixer comprom�s constr�nyer conv�ncer
	dem�s desapar�ixer descomprom�s descon�ixer desmer�ixer destr�nyer em�s emp�nyer emp�s entrem�s
	esp�nyer esp�s estr�nyer *ir�ixer-se malm�s manum�s mer�ixer om�s par�ixer perm�s prom�s readm�s reapar�ixer
	recon�ixer rem�s restr�nyer retransm�s *rev�ncer rev�n sotm�s tram�s transm�s v�ncer impr�s reimpr�s

	*ele *eme *ene *esse

	*deportiste *optimiste *pessimiste *dentiste *electriciste

	*h�mens *im�tgens *t�rmens *v�rgens *m�rgens *j�vens *�rfens *r�vens

	*rapidea *naturalea *durea *tendrea *vellea *bellea *grandea *riquea

	*huitava *septuag�sim *octag�sim cinqu� sis� set� huit� vuit� nov� des� onz� dotz� vint� cent�

	*febra *escabuss� *dep�sit *pantaix *payola *reny� *albergina *miqueta *ba�l *marmol� s�quia *s�t s�pia inter�s estr�s ed�n p�sol terratr�mol tr�vol c�rcol s�mola caf� caf�s vost� *gesmil

	*propenc *lleg�tim *ll�gic *setmes� t�rbol impr�s at�s at�s *destrellat esp�s franc�s angl�s roman�s aragon�s

	*mentres *a�na *ah�

	*deprendre *supondre *vore *regallar *escabussar *encanar-se *desnugar *gemecar
	*engrunsar *arrednir *pantaixar

	apar�ixer con�ixer par�ixer conv�ncer v�ncer at�nyer estr�nyer mer�ixer

	*desvanit sofrit reblit oferit

	*escriv� *asseem *asseeu *asseent *jaem *jaeu *jaent
	apr�n compr�n dep�n of�n *tingam afigen siga f�iem f�ieu cr�iem cr�ieu v�iem v�ieu

	*perc *perga *pergues *perga *perguem *pergueu *perguen *perguera *pergueres
	*perguera *pergu�rem *pergu�reu *pergueren *pergu� *pergueres *pergu� *pergu�rem
	*pergu�reu *pergueren
	*�bric �briga �brigues �briga �briguen
	*�mplic *�mpliga *�mpligues *�mpliga *�mpliguen
	*dorc *dorga *cullc *cullga *c�rrec *c�rrega *engulc *fuigc *lligc *vullc
	*cusga *crusca *cullga *engulga *fuigga *morga
}

bons, dolents = 0, 0
resultat1 = []
resultat2 = []
mots.each do |mot|
	next if mot.empty?
	if mot.sub!(/^\*/, '')
		if hs.OK(mot)
			resultat1 << ">>> \"#{mot}\" no �s correcte!" + " (" + hs.morph(mot).to_s + ")"
			dolents += 1
		else
			resultat2 << "\"#{mot}\" => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			bons += 1
		end
	else
		if hs.OK(mot)
			resultat2 << "\"#{mot}\" �s correcte" + " (" + hs.morph(mot).to_s + ")"
			bons += 1
		else
			resultat1 << ">>> \"#{mot}\" �s correcte!, per�... => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			dolents += 1
		end
	end
end
puts ">>> #{dolents} errors / #{bons + dolents} intents"
puts resultat1
puts resultat2
