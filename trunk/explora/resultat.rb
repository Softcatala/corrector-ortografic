# Resultats de l'exploració

require './eines'
require './blocs'

class Resultat
	
	def initialize()
		@blocs = []
	end
	
	def <<(que)
		raise "S'esperava un bloc de text" unless que.is_a?(BlocText)
		@blocs << que
	end
	
	def tanca
		abstracte
	end

end

class ResultatFitxer < Resultat
	
	def initialize(nomFitxer)
		super()
		@nomFitxer = nomFitxer
	end
	
	def tanca
		File.open(@nomFitxer, 'w') do |fitxer|
			@blocs.each do |bloc|
				fitxer.puts bloc.inspect
			end
		end
	end
	
end

__END__

r = ResultatFitxer.new('prova.txt')
r << BlocText.new('Vilaweb', 'Les milícies de Hesbol·là s\'han apoderat de bona part dels edificis governamentals de la capital libanesa, Beirut, de les seus dels principals mitjans de comunicació i d\'algunes casernes de l\'exèrcit, institució que intenta mantenir-se neutral en aquesta crisi. El govern va decidir fa tres dies de clausurar la xarxa de telecomunicacions de Hesbol·là, que s\'ho va prendre com una declaració de guerra. Un mestre Ferran Canet, establert al Líban, segueix l\'actualitat des del seu bloc, Coses de Beirut.', true)
r.tanca
