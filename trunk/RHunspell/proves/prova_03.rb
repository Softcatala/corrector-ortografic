require 'hunspell'

BASE = '../../resultats'
#~ hs = Hunspell.new("#{BASE}/catalan_myspell.aff", "#{BASE}/catalan_myspell.dic")
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p hs

mots = []
mots += %w{
	f�ra
	existencial
	Ra�l
	cont�nuum
	policial
	rodamons *rodam�ns
	angoixants
	equilibradament b�sicament
	desestabilitzador desestabilitzadors
	apocalipsi apocalipsis d'apocalipsi
	web
	internauta
	lliurepensador
	visualitzaci�
	escriv�s escrigu�s
	ciberespai clicar dom�tica emoticona encriptar gigaoctet m�dem multiplexar navegador p�xel videoconfer�ncia v�xel web xat
	malnutrici�
	condicionants
	divulgatiu
}

bons, dolents = 0, 0
resultat = []
mots.each do |mot|
	next if mot.empty?
	if mot.sub!(/^\*/, '')
		if hs.OK(mot)
			resultat << ">>> \"#{mot}\" no �s correcte!" + " (" + hs.morph(mot).to_s + ")"
			dolents += 1
		else
			resultat << "\"#{mot}\" => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			bons += 1
		end
	else
		if hs.OK(mot)
			resultat << "\"#{mot}\" �s correcte" + " (" + hs.morph(mot).to_s + ")"
			bons += 1
		else
			resultat << ">>> \"#{mot}\" �s correcte!, per�... => " + hs.sugg(mot).collect { |m| "\"#{m}\"" }.join(', ')
			dolents += 1
		end
	end
end
puts ">>> #{dolents} errors / #{bons + dolents} intents"
puts resultat
