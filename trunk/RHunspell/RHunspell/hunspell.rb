require 'xhunspell'

# Joan Moratinos, 2007

class Hunspell
	# Hunspell spellchecker by Németh László
	def initialize(aff, dic)
		@xhunspell = XHunspell.new(aff, dic)
	end
	def OK(mot)
		return @xhunspell.OK(mot)
	end
	def sugg(mot)
		return @xhunspell.sugg(mot)
	end
	def morph(mot)
		return @xhunspell.morph(mot)
	end
end
