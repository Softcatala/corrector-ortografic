# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
hs = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p hs

mots = []

File.open('../../entrades/avl_amb_�s_esa.txt').each_line do |lin|
	if lin =~ /^ent=(\S+)�s (\1)esa(\^.*)$/
		unless hs.OK("#{$1}�s") && hs.OK("#{$1}esa") && hs.OK("#{$1}esos") && hs.OK("#{$1}eses")
			puts "falta #{$1}�s!"
		end
	end
	#~ puts lin
end
