# Prova les paraules procedents del diccionari personalitzat dels Serveis Lingüístics de la UB
# http://www.ub.edu/sl/ca/alt/recursos/diccionari/index.htm

require 'hunspell'

HUNSPELL = '../../resultats/hunspell'
#~ avl = Hunspell.new("#{HUNSPELL}/avl.aff", "#{HUNSPELL}/avl.dic")
gen = Hunspell.new("#{HUNSPELL}/catalan.aff", "#{HUNSPELL}/catalan.dic")

errors = []
proves = 0

File.open('../../propostes/UB/ca_ES.dic').each_line do |mot|
	next if mot.strip!.empty?
	mot = $1 if mot =~ %r{(.*?)/}
	proves += 1
	next if gen.OK(mot)
	errors << mot
end

puts "S'han provat #{proves} paraules"
puts "Hi ha #{errors.length} errors"

errors.sort!

errors.each do |k|
	puts k
end
