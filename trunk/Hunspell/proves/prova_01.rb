require 'hunspell'

BASE = '../../resultats'
#~ hs = Hunspell.new("#{BASE}/catalan_myspell.aff", "#{BASE}/catalan_myspell.dic")
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p hs

mots = []
mots += %w(abstenir atenir captenir cartenir contenir detenir entretenir mantenir menystenir obtenir retenir sostenir viltenir)
mots += %w(abstindre atindre captindre cartindre contindre detindre entretindre mantindre menystindre obtindre retindre sostindre viltindre)
mots += %w(abstindr� atindr� captindr� cartindr� contindr� detindr� entretindr� mantindr� menystindr� obtindr� retindr� sostindr� viltindr�)
mots += %w(acceptat acceptada acceptats acceptades l'acceptat l'acceptada *l'acceptats *l'acceptades d'acceptat d'acceptada d'acceptats d'acceptades)
mots += %w(formatatge formataci�)
mots += %w(posiciona posicionat posicionar�)
mots += %w(delimitador delimitadora delimitadors delimitadores)
mots += %w(deshabilitat deshabilitava deshabilitaren)
mots += %w(actualitzable actualitzables)
mots += %w(posicionament posicionaments)
mots += %w(Avaluador avaluadora)
mots += %w(reiniciat reiniciades reiniciar)
mots += %w(Desinstal�laci�)
mots += %w(Reanomena reanomenar Reanomenava)
mots += %w(digitalment)
mots += %w(confidencialitat confidencialitats)
mots += %w(organitzacional organitzacionals)
mots += %w(adaptatius adaptatives)
mots += %w(Desenvolupadors Desenvolupadores)
mots += %w(*aquet *aqueta *aquets *aquetes i� d'i�)
mots += %w(*d'abaten)
mots += %w(*adesc *adeixo)
mots += %w(infraroig infraroja *infrarroja)
mots += %w(pel pels p�ls p�l n�ixer del dels al als)
mots += %w(escr� escr�s escrua escrues *escruns *escrunes)
mots += %w(cru crus crua crues *cruns *crunes)
mots += %w(l'Hongria acadi� acadiana *l'acadians l'esporler� s'�rab n'Andreu N'Andreu *n'andreu *N'andreu *n'Isaura Isaura)
mots += %w(ERC d'ERC dl. l'Internet *l'internet *d'internet)
mots += %w(embarbussar�em t'embarbussara condolia *dona'm d�na'm)
mots += %w(p�rcing web)
mots += %w(l'agaf L'agaf l'Agaf L'Agaf)
mots += %w(abiet�cia abiet�cies d'abiet�cies)
mots += %w(d'ara d'aqu� d'all� *d'atentament)
mots += %w(entre d'entre enmig d'enmig elles d'elles)
mots += %w(escampavies *escampaviesos)
mots += %w(fer�stec fer�stega fer�stecs fer�stegues)
mots += %w(ambigu ambigua ambigus ambig�es)
mots += %w(obtindre Viquip�dia desenvolupadors interactuar semiformal p.e. detectable interactu� aquestes)
bons, dolents = 0, 0
resultat = []
mots.each do |mot|
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
