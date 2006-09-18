# Comprova que les paraules d'un fitxer són correctes

#~ puts ARGV.join(", ")
#~ puts ARGV.length
#~ exit 1

unless ARGV.length == 1
	<<-FISINT .each_line { |lin| puts lin.sub(/^\t\t/, '') } 
		Comprova que les paraules d'un fitxer són correctes
		Sintaxi: ruby #{$0} fitxer_d'entrada
		Treu les paraules incorrectes
		Ignora les línies començades per "--"
	FISINT
end

require 'spell.dll'

dir = 'C:/documents/programes/ortoSC/'
aff = dir + 'resultats/catalan.aff'
dic = dir + 'resultats/catalan.dic'
ms = MySpell.new(aff, dic)

errors = []
norep = {}

begin
	File.open(ARGV[0]).each_line do |lin|
		lin.strip!
		next if lin =~ /^--/
		mots = lin.split(/[ ]+/)
		mots.each do |mot|
			next if ms.ok(mot)
			unless norep.has_key?(mot)
				norep[mot] = true
				errors << mot
			end
		end
	end
	puts errors.join("\n")
rescue Errno::ENOENT => e
	puts "No es pot obrir el fitxer #{ARGV[0]}"
rescue Exception => e
	raise e
end
