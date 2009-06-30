# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
hs = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p hs

mots = []

File.open('../../entrades/avl_amb_és_esa.txt').each_line do |lin|
	if lin =~ /^ent=(\S+)és (\1)esa(\^.*)$/
		unless hs.OK("#{$1}ès") && hs.OK("#{$1}esa") && hs.OK("#{$1}esos") && hs.OK("#{$1}eses")
			puts "falta #{$1}és!"
		end
	end
	#~ puts lin
end
