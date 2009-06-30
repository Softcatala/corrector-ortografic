# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p hs

mots = []

esborrats = 0

File.open('../../entrades/avl.txt').each_line do |lin|
	if lin =~ /^ent=(\S+)és (\1)esa(\^.*)$/
		#~ puts "$1: #{$1}, $2: #{$2}, $3: #{$3}"
		if hs.OK("#{$1}ès") && hs.OK("#{$1}esa") && hs.OK("#{$1}esos") && hs.OK("#{$1}eses")
			lin = "// #{$1}és = #{$1}ès/J"
			esborrats += 1
		end
	end
	puts lin
end

puts "S'han esborrat #{esborrats} línies"

#~ absents = {}
#~ absents.default = 0
#~ tros.split(/[ ,;:.\/"²()—>\[\]\r\n\t]/).each do |mot|
	#~ next if mot =~ /^[-0-9%]+$/
	#~ next if hs.OK(mot)
	#~ absents[mot] += 1
#~ end
#~ absents.keys.sort.each do |mot|
	#~ puts mot
#~ end