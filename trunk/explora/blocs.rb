# Blocs de text
# Els blocs s�n documents de text (per exemple un article)
# Segons el tipus, es poden dividir en frases o en paraules
# Les frases es poden dividir en paraules

class BlocText
	
	# Diu si el bloc cont� frases, o nom�s paraules
	def teFrases?
		return @teFrases
	end
	
	def initialize(id, contingut, teFrases)
		@id, @contingut, @teFrases = id, contingut, teFrases
		if @teFrases
			divideixEnFrases
			@mots = nil
		else
			@frases = nil
			divideixEnMots
		end
	end
	
	# Fa un yield de totes les frases que cont� el bloc
	def frases
		@frases.each do |frase|
			yield frase
		end
	end
	
	# Fa un yield de tots els mots que cont� el bloc
	def mots()
		if @mots
			@mots.each do |mot| yield mot end
		else
			@frases.each do |frase|
				frase.mots do |mot| yield mot end
			end
		end
	end

	# Identifica l'origen del bloc
	attr_reader :id

	# Divideix aquest bloc en frases
	def divideixEnFrases
		return if @frases || !@teFrases
		# PER_FER
		@frases = [ BlocText.new(@id, @contingut, false) ]
	end

	# Divideix en mots
	def divideixEnMots
		raise "No es pot dividir en mots" if @teFrases
		@mots = @contingut.gsub(/[,.;"]/, '').strip.split(/\s+/)
	end
	
	private :divideixEnFrases, :divideixEnMots
	
	def inspect
		if @contingut.length > 60
			@contingut =~ /(.{60}[^ ]+)/
			cont = $1 + '...'
		else
			cont = @contingut
		end
		if @teFrases
			ff = (@frases ? @frases.length.to_s : '?') + ' frases'
		else
			ff = 'no t� frases'
		end
		return "\"#{cont}\", #{ff}"
	end
	
end

__END__

bt = BlocText.new('Vilaweb', 'Les mil�cies de Hesbol�l� s\'han apoderat de bona part dels edificis governamentals de la capital libanesa, Beirut, de les seus dels principals mitjans de comunicaci� i d\'algunes casernes de l\'ex�rcit, instituci� que intenta mantenir-se neutral en aquesta crisi. El govern va decidir fa tres dies de clausurar la xarxa de telecomunicacions de Hesbol�l�, que s\'ho va prendre com una declaraci� de guerra. Un mestre Ferran Canet, establert al L�ban, segueix l\'actualitat des del seu bloc, Coses de Beirut.', true)
#~ bt.frases do |frase|
	#~ puts frase.inspect
#~ end
bt.mots do |mot|
	puts mot
end
