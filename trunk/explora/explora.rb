#~ # Here we see a very simple WATIR script to drive to google and validate a page
 #~ require 'watir'                          # use watir gem
 #~ test_site = 'http://www.google.com'      # set a variable
 #~ ie = Watir::IE.new                       # open the IE browser
 #~ ie.goto(test_site)                       # load url, go to site
 #~ ie.text_field(:name, "q").set("pickaxe") # load text "pickaxe" into search field named "q"
 #~ ie.button(:name, "btnG").click           # "btnG" is the name of the Search button, click it
 
 #~ if ie.text.include?("Programming Ruby")  
   #~ puts "Test Passed. Found the test string: 'Programming Ruby'."
 #~ else
   #~ puts "Test Failed! Could not find: 'Programming Ruby'" 
 #~ end

require 'Date'
require './eines'
require './font'

avui = Avui.new(Date.today)
