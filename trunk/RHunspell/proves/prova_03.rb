require 'hunspell'

BASE = '../../resultats'
#~ hs = Hunspell.new("#{BASE}/catalan_myspell.aff", "#{BASE}/catalan_myspell.dic")
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p hs

mots = []
mots += %w{
	fóra
	existencial
	Raül
	contínuum
	policial
	rodamons *rodamóns
	angoixants
	equilibradament bàsicament
	desestabilitzador desestabilitzadors
	apocalipsi apocalipsis d'apocalipsi
	web
	internauta
	lliurepensador
	visualització
	escrivís escrigués
	ciberespai clicar domòtica emoticona encriptar gigaoctet mòdem multiplexar navegador píxel videoconferència vòxel web xat
	malnutrició
	condicionants
	divulgatiu
}

bons, dolents = 0, 0
resultat = []
mots.each do |mot|
	next if mot.empty?
	if mot.sub!(/^\*/, '')
		if hs.OK(mot)
			resultat << ">>> \"#{mot}\" no és correcte!" + " (" + hs.morph(mot).to_s + ")"
			dolents += 1
		else
			resultat << "\"#{mot}\" => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			bons += 1
		end
	else
		if hs.OK(mot)
			resultat << "\"#{mot}\" és correcte" + " (" + hs.morph(mot).to_s + ")"
			bons += 1
		else
			resultat << ">>> \"#{mot}\" és correcte!, però... => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			dolents += 1
		end
	end
end
puts ">>> #{dolents} errors / #{bons + dolents} intents"
puts resultat
