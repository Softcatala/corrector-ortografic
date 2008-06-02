# Fa proves diverses de control de qualitat
# PER_FER: Accedir al corrector des de la pestanya de control de qualitat i reescriure les proves en C#

class String
	def bool
		return true if self =~ /^[Ss]$/
		return false if self =~ /^[Nn]$/
		raise "S'esperava S o N (#{self})"
	end
end

class Prova
	def initialize(nom)
		@nom = nom
		@encerts, @errors = 0, 0
	end
	attr_reader :nom, :encerts, :errors
	def nou_encert(quants = 1)
		@encerts += quants
	end
	def nou_error(quants = 1)
		@errors += quants
	end
	def resum
		"#{nom} => #{errors == 0 ? 'OK' : 'ERROR'}, #{encerts} encerts, #{errors} errors"
	end
end

class Qualitat
	def initialize
		require 'hunspell'
		# La llista de diccionaris que farem servir
		@dics = []
		@@RESULTATS = '../../resultats'
		@dics << Hunspell.new("#{@@RESULTATS}/catalan.aff", "#{@@RESULTATS}/catalan.dic")
		@dics << Hunspell.new("#{@@RESULTATS}/avl.aff", "#{@@RESULTATS}/avl.dic")
		@proves = []
	end
	
	attr_reader :dics
	
	def nova_prova(prova)
		@proves << prova
	end
	
	def mostra_resum
		resum = Prova.new("RESUM")
		@proves.each do |prova|
			puts prova.resum
			resum.nou_error(prova.errors)
			resum.nou_encert(prova.encerts)
		end
		puts resum.resum
	end

end

class ProvaContingut < Prova
	def initialize(qualitat)
		super('Prova contingut de les versions')
		qualitat.nova_prova(self)
		executa(qualitat.dics)
	end
	def executa(dics)
		# Comprova que les versions contenen els mots donats
		mots = <<-FIMOTS
			casa				S	S
			anglès			S	S
			anglés			N	S
			l'anglès			S	S
			l'anglés			N	S
			anglesa			S	S
			l'anglesa			S	S
		FIMOTS
		mots.each do |lin|
			raise "Línia incorrecta (#{lin})" unless lin =~ /\s*(\S+)\s+([SN])\s+([SN])/
			mot, dins = $1, [$2.bool, $3.bool]
			dins.each_index do |i|
				if dics[i].OK(mot) == dins[i]
					nou_encert
				else
					nou_error
				end
			end
		end
	end
end

qualitat = Qualitat.new
ProvaContingut.new(qualitat)
qualitat.mostra_resum
