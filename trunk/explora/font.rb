# Fonts de dades
# Segurament, una font sempre és un lloc web

class FontDades
	# Un proveïdor de texts, que pot ser explorat
	# Una font és representada per un conjunt de blocs (per exemple, articles d'un diari)

	# Fa yield dels diversos blocs que componen la font
	def blocs
		abstracte
	end
	
end

class LlocWeb < FontDades
	
	require './eines'
	require 'watir'

	@@ie = nil
	
	def LlocWeb.ie
		unless @@ie
			begin
				@@ie = Watir::IE.find(:title, /./)
				raise "No trobat" unless @@ie
			rescue => exc
				puts "Error cercant IE (#{exc})"
				@@ie = Watir::IE.new
			ensure
				p @@ie
			end
			@@ie.logger.level = Logger::ERROR
		end
		return @@ie
	end
	
	def initialize(url)
		@url = "http://#{url}"
		LlocWeb::ie.goto(url)
	end

	def processa(resultat)
		abstracte
	end
	
end

class Avui < LlocWeb

	require 'Date'
	
	def initialize(data = nil)
		@data = data || Date.today
		if data = Date.today
			super('paper.avui.cat')
		else
			raise "Només es pot mirar el dia d'avui"
		end
	end

	def processa(resultat)
		ie = LlocWeb::ie
		nav = ie.ul(:id, 'nav')
		p nav
		nav.lis.each do |li|
			puts li.text
			if li.text =~ /^Opin/
				li.links.each do |link|
					p link
				end
				li.methods.sort.each do |m|
					puts m
				end
			end
		end
		#~ nav.methods.sort.each do |m|
			#~ puts m
		#~ end
	end
	
end

#~ __END__

avui = Avui.new
avui.processa(nil)

