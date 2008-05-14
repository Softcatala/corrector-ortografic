# Classe per per processar continguts
# Derivats d'aquesta classe són els que fan la feina, a partir d'un bloc de dades i una classe per recollir els resultats

require './eines'

class Proces
	
	def processa(bloc, resultat)
		abstracte
	end

end

class CercaMotsAbsents < Proces

	#~ require 'rhunspell'
	
	def processa(bloc, resultat)
		abstracte
	end

end
