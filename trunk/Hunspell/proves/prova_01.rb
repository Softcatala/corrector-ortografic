require 'hunspell'

BASE = '../../resultats'
#~ hs = Hunspell.new("#{BASE}/catalan_myspell.aff", "#{BASE}/catalan_myspell.dic")
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p hs

mots = []
mots += %w(abstenir atenir captenir cartenir contenir detenir entretenir mantenir menystenir obtenir retenir sostenir viltenir)
mots += %w(abstindre atindre captindre cartindre contindre detindre entretindre mantindre menystindre obtindre retindre sostindre viltindre)
mots += %w(abstindré atindré captindré cartindré contindré detindré entretindré mantindré menystindré obtindré retindré sostindré viltindré)
mots += %w(acceptat acceptada acceptats acceptades l'acceptat l'acceptada *l'acceptats *l'acceptades d'acceptat d'acceptada d'acceptats d'acceptades)
mots += %w(formatatge formatació)
mots += %w(posiciona posicionat posicionaré)
mots += %w(delimitador delimitadora delimitadors delimitadores)
mots += %w(deshabilitat deshabilitava deshabilitaren)
mots += %w(actualitzable actualitzables)
mots += %w(posicionament posicionaments)
mots += %w(Avaluador avaluadora)
mots += %w(reiniciat reiniciades reiniciar)
mots += %w(Desinstal·lació)
mots += %w(Reanomena reanomenar Reanomenava)
mots += %w(digitalment)
mots += %w(confidencialitat confidencialitats)
mots += %w(organitzacional organitzacionals)
mots += %w(adaptatius adaptatives)
mots += %w(Desenvolupadors Desenvolupadores)
mots += %w(*aquet *aqueta *aquets *aquetes ió d'ió)
mots += %w(*d'abaten)
mots += %w(*adesc *adeixo)
mots += %w(infraroig infraroja *infrarroja)
mots += %w(pel pels pèls pèl nàixer del dels al als)
mots += %w(escrú escrús escrua escrues *escruns *escrunes)
mots += %w(cru crus crua crues *cruns *crunes)
mots += %w(l'Hongria acadià acadiana *l'acadians l'esporlerí s'àrab n'Andreu N'Andreu *n'andreu *N'andreu *n'Isaura Isaura)
mots += %w(ERC d'ERC dl. l'Internet *l'internet *d'internet)
mots += %w(embarbussaríem t'embarbussara condolia *dona'm dóna'm)
mots += %w(pírcing web)
mots += %w(l'agaf L'agaf l'Agaf L'Agaf)
mots += %w(abietàcia abietàcies d'abietàcies)
mots += %w(d'ara d'aquí d'allà *d'atentament)
mots += %w(entre d'entre enmig d'enmig elles d'elles)
mots += %w(escampavies *escampaviesos)
mots += %w(feréstec feréstega feréstecs feréstegues)
mots += %w(ambigu ambigua ambigus ambigües)
mots += %w(obtindre Viquipèdia desenvolupadors interactuar semiformal p.e. detectable interactuï aquestes)
bons, dolents = 0, 0
resultat = []
mots.each do |mot|
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
