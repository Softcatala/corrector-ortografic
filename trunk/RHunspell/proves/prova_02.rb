require 'hunspell'

BASE = '../../resultats'
hs = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")

LLENGUES = 'en|fr|it|la|de|ar|nl|pt|zh|ja|gd|ko|tr|af|el|si'

dolents = {}
File.open("entrades_neoloteca.txt").each_line do |linia|
	linia.strip!
	next if linia =~ /(\[(#{LLENGUES})\]|\(\*\))/
	lin = linia.gsub(/\[.*?\]/) { |tros| tros.gsub(' ', '_') }
	lin.split(/\s+/).each do |mot|
		next if mot.empty?
		mot.gsub!('_', ' ')
		next if mot =~ /(^-|\(\*\)| (#{LLENGUES})\]$)/
		mot.gsub!(/(^[dl]'|!$)/, '')
		unless hs.OK(mot)
			if dolents.has_key?(mot)
				dolents[mot] << linia
			else
				dolents[mot] = [ linia ]
			end
		end
	end
	#~ break if dolents.length >= 100
end

File.open("resultat.txt", "w") do |fitxer|
	dolents.keys.sort.each do |entrada|
		fitxer.puts "#{entrada}: " + dolents[entrada].join(', ')
	end
end