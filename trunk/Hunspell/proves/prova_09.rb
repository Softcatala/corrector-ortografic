# Prova les paraules procedents d'AVL
require 'hunspell'

BASE = '../../resultats/hunspell'
avl = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
gen = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
p avl, gen

oerrors = []
errors = {}
proves = 0

File.open('avl.txt').each_line do |lin|
	next if lin.strip!.empty?
	next unless lin =~ /^([GV=]) (.*)/
	grup, mot = $1, $2
	mot.sub!(/^((es )|(o )|(2)|('s)|(l')|(;)|(d'))/, '')
	mot.sub!(';', '')
	mot.strip!
	next if mot.empty?
	proves += 1
	if grup == 'V'
		unless avl.OK(mot)
			oerrors << mot
			errors[mot] = avl.sugg(mot)
		end
	elsif grup == 'G'
		unless gen.OK(mot)
			oerrors << mot
			errors[mot] = gen.sugg(mot)
		end
	end
end

puts "S'han provat #{proves} paraules"
puts "Hi ha #{errors.length} errors"

oerrors.each do |k|
	v = errors[k]
	puts "\"#{k}\" (" + v.join(', ') + ')'
end
