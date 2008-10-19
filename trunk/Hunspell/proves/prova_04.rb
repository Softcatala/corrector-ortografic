# Prova el corrector avl
require 'hunspell'

BASE = '../../resultats/hunspell'
hs = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p hs

mots = []
mots += %w{
	saber sàpia sàpies sàpia sapiem sapieu sàpien
	caber càpia càpies càpia capiem capieu càpien
	obrir òbriga òbrigues òbriga obrim obriu òbriguen
	oferit oferida oferits oferides  oferir 
	sofrit sofrida sofrits sofrides  sofrir 
	suplit suplida suplits suplides  suplir 
	reblit reblida reblits reblides  reblir 
	plangut planguda planguts plangudes  plànyer 
	fengut fenguda fenguts fengudes  fényer 
	rist rista ristos ristes  riure  
	admés aparéixer aprehés aprehén arremés atényer comés comparéixer compromés constrényer convéncer
	demés desaparéixer descompromés desconéixer desmeréixer destrényer emés empényer empés entremés
	espényer espés estrényer *iréixer-se malmés manumés meréixer omés paréixer permés promés readmés reaparéixer
	reconéixer remés restrényer retransmés *revéncer revén sotmés tramés transmés véncer imprés reimprés

	*ele *eme *ene *esse

	*deportiste *optimiste *pessimiste *dentiste *electriciste

	*hòmens *imàtgens *térmens *vèrgens *màrgens *jóvens *òrfens *ràvens

	*rapidea *naturalea *durea *tendrea *vellea *bellea *grandea *riquea

	*huitava *septuagèsim *octagèsim cinqué sisé seté huité vuité nové desé onzé dotzé vinté centé

	*febra *escabussó *depòsit *pantaix *payola *renyó *albergina *miqueta *baül *marmoló séquia *sét sépia interés estrés edén pésol terratrémol trévol cércol sémola café cafés vosté *gesmil

	*propenc *llegítim *llògic *setmesí térbol imprés atès atés *destrellat espés francés anglés romanés aragonés

	*mentres *aïna *ahí

	*deprendre *supondre *vore *regallar *escabussar *encanar-se *desnugar *gemecar
	*engrunsar *arrednir *pantaixar

	aparéixer conéixer paréixer convéncer véncer atényer estrényer meréixer

	*desvanit sofrit reblit oferit

	*escriví *asseem *asseeu *asseent *jaem *jaeu *jaent
	aprén comprén depén ofén *tingam afigen siga féiem féieu créiem créieu véiem véieu

	*perc *perga *pergues *perga *perguem *pergueu *perguen *perguera *pergueres
	*perguera *perguérem *perguéreu *pergueren *perguí *pergueres *pergué *perguérem
	*perguéreu *pergueren
	*òbric òbriga òbrigues òbriga òbriguen
	*òmplic *òmpliga *òmpligues *òmpliga *òmpliguen
	*dorc *dorga *cullc *cullga *córrec *córrega *engulc *fuigc *lligc *vullc
	*cusga *crusca *cullga *engulga *fuigga *morga
}

bons, dolents = 0, 0
resultat1 = []
resultat2 = []
mots.each do |mot|
	next if mot.empty?
	if mot.sub!(/^\*/, '')
		if hs.OK(mot)
			resultat1 << ">>> \"#{mot}\" no és correcte!" + " (" + hs.morph(mot).to_s + ")"
			dolents += 1
		else
			resultat2 << "\"#{mot}\" => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			bons += 1
		end
	else
		if hs.OK(mot)
			resultat2 << "\"#{mot}\" és correcte" + " (" + hs.morph(mot).to_s + ")"
			bons += 1
		else
			resultat1 << ">>> \"#{mot}\" és correcte!, però... => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			dolents += 1
		end
	end
end
puts ">>> #{dolents} errors / #{bons + dolents} intents"
puts resultat1
puts resultat2
