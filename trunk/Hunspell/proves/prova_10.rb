# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
gen = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
avl = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p gen, avl

mots = []
tros = <<-FITROS
allotjador allotjadora allotjadors allotjadores
al�lotell al�lotells
al�lotet al�lotets
ampl�ssim ampl�ssima ampl�ssims ampl�ssimes
antiesclavista antiesclavistes
antifranc�s antifrancesa antifrancesos antifranceses
antiinstitucional antiinstitucionals
antipsiquiatria antipsiquiatries
aset asets
atrezzista atrezzistes
autocomplaent autocomplaents
baixmedieval baixmedievals
baix�ssim baix�ssima baix�ssims baix�ssimes
bell�ssim bell�ssima bell�ssims bell�ssimes
berberisc berberisca berberiscs berberisques
ber�n
bloquet bloquets
borb� borbons
botiflerisme botiflerismes
bunyolada bunyolades
calentet calenteta calentets calentetes
calop calops
car�ssim car�ssima car�ssims car�ssimes
catalanoaragon�s catalanoaragonesa catalanoaragonesos catalanoaragoneses
catrastrofisme catrastrofismes
cintell cintells
clar�ssim clar�ssima clar�ssims clar�ssimes
comissariar comissariaven comissariarien
conegud�ssim conegud�ssima conegud�ssims conegud�ssimes
contacontes
conventualisme conventualismes
corresponsable corresponsables
cosenyor cosenyora cosenyors cosenyores
cossiolet cossiolets
craniosc�pia craniosc�pies
curatorial curatorials
c�nyom c�nyoms
desafiant desafiants
descoordinaci� descoordinacions
desdenyable desdenyables
desestacionalitzador desestacionalitzadora desestacionalitzadors desestacionalitzadores
desestacionalitzar desestacionalitzaven desestacionalitzarien
desmem�ria desmem�ries
destacad�ssim destacad�ssima destacad�ssims destacad�ssimes
dialectalment
dign�ssim dign�ssima dign�ssims dign�ssimes
discretori discretoris
discret�ssim discret�ssima discret�ssims discret�ssimes
discriminat�riament
diversificador diversificadora diversificadors diversificadores
dobleret doblerets
dolent�ssim dolent�ssima dolent�ssims dolent�ssimes
edafog�nesi edafog�nesis
educativament
electr�nicament
elevad�ssim elevad�ssima elevad�ssims elevad�ssimes
embullad�ssim embullad�ssima embullad�ssims embullad�ssimes
emocionadament
encimentar encimentaven encimentarien
enforcament
enrevoltar enrevoltaven enrevoltarien
esbucament
escanyadura escanyadures
escoleta escoletes
escriv�	?
esperp�nticament
estamental estamentals
estampeta estampetes
estilisme estilismes
estoneta estonetes
estudiad�ssim estudiad�ssima estudiad�ssims estudiad�ssimes
etnobot�nic etnobot�nica etnobot�nics etnobot�niques
exhaustivament
exigent�ssim exigent�ssima exigent�ssims exigent�ssimes
externalitzaci� externalitzacions
externalitzar externalitzaven externalitzarien
facilitador facilitadora facilitadors facilitadores
factibilitat factibilitats
fals�ssim fals�ssima fals�ssims fals�ssimes
famos�ssim famos�ssima famos�ssims famos�ssimes
fefaentment
feixet feixets
ferotgement
ficad�s ficadissa ficad�s ficadisses
formigonament
francoespanyol francoespanyola francoespanyols francoespanyoles
furibundament
f�lmicament
geneal�gicament
generacionalment
gen�ticament
geod�sicament
guarnicioneria guarnicioneries
guitarr�stic guitarr�stica guitarr�stics guitarr�stiques
hispanof�lia hispanof�lies
historicista historicistes
historicoart�stic historicoart�stica historicoart�stics historicoart�stiques
homot�rmia homot�rmies
hortet hortets
identificatiu identificativa identificatiu identificatives
illeta illetes
immediaci� immediacions
imm�tic imm�tica imm�tics imm�tiques
important�ssim important�ssima important�ssims important�ssimes
inercialment
infant� infantons
infografista infografistes
infrahabitatge infrahabitatges
inicialment
inquietantment
inq�estionat inq�estionada inq�estionat inq�estionades
intercultural interculturals
intermediaci� intermediacions
irosament
jovenet jovenets
laboralment
laurisilva laurisilves
ling��sticament
liorn�s liornesa liornesos liorneses
llargu�ssim llargu�ssima llargu�ssims llargu�ssimes
llimb llimbs
llunyanament
lul�l�stic lul�l�stica lul�l�stics lul�l�stiques
l�abans	?
l�aleshores	?
l�avui	?
l�dicament
macroesdeveniment
macroexposici� macroexposicions
medi�ticament
mete�ricament
microclim�tic microclim�tica microclim�tics microclim�tiques
microc�mera microc�meres
minicomputadora minicomputadores
missionar missionaven missionarien
mocadoret mocadorets
modest�ssim modest�ssima modest�ssims modest�ssimes
molt�ssim molt�ssima molt�ssims molt�ssimes
multiculturalitat multiculturalitats
muntanyer muntanyera muntanyers muntanyeres
musculoesquel�tic musculoesquel�tica musculoesquel�tics musculoesquel�tiques
naixeren	?
natural�stic natural�stica natural�stics natural�stiques
ornamentalment
pastanag� pastanagons
paternofilial paternofilials
perilloss�ssim perilloss�ssima perilloss�ssims perilloss�ssimes
perimetral perimetrals
pesad�ssim pesad�ssima pesad�ssims pesad�ssimes
piti�s piti�sa piti�s piti�ses
planet�riament
politicocultural politicoculturals
politicomilitar politicomilitars
politicosocial politicosocials
popular�ssim popular�ssima popular�ssims popular�ssimes
posader posaders
potet potets
predemocr�tic predemocr�tica predemocr�tics predemocr�tiques
presumptament
pretesament
previsorament
primer�ssim primer�ssima primer�ssims primer�ssimes
principat� principatina principat� principatines
priorit�riament
profranc�s profrancesa profrancesos profranceses
prosopogr�fic prosopogr�fica prosopogr�fics prosopogr�fiques
pseudodespatx pseudodespatxos
pseudofilos�fic pseudofilos�fica pseudofilos�fics pseudofilos�fiques
pseudofilos�fic pseudofilos�fica pseudofilos�fics pseudofilos�fiques
pseudoradicalisme pseudoradicalismes
psicoanal�ticament
ps�quicament
quadret quadrets
rarenc rarenca rarencs rarenques
redefinici� redefinicions
reemprar reempraven reemprarien
reim	?
reinstaurar reinstauraven reinstaurarien
reintroduir reintrodu�en reintroduirien
reinventar reinventaven reinventarien
repressivament
ressenyable ressenyables
revaloritzaci� revaloritzacions
riqu�ssim riqu�ssima riqu�ssims riqu�ssimes
ritualment
secret�ssim secret�ssima secret�ssims secret�ssimes
sectorialment
sever�ssim sever�ssima sever�ssims sever�ssimes
subtitulaci� subtitulacions
sus
tecnol�gicament
tem�ticament
territorialment
tudadissa tudadisses
tumbet tumbets
tur�sticament
valorable valorables
vast�ssim vast�ssima vast�ssims vast�ssimes
venerad�ssim venerad�ssima venerad�ssims venerad�ssimes
videoprojector videoprojectors
vital�ciament
zool�gicament
FITROS

absents = {}
absents.default = 0
total = 0
tros.split(/[ ,;:.\/"�()�>\[\]\r\n\t]/).each do |mot|
	next if mot =~ /^[-?]/
	next if mot =~ /^[-0-9%]+$/
	next if mot =~ /DGLC|oct|nov/
	total += 1
	if mot =~ /^\*(.*)/
		mot = $1
		next unless avl.OK(mot)
		absents["*#{mot}"] += 1
	else
		next if avl.OK(mot)
		absents[mot] += 1
	end
end
nabsents = absents.size
presents = total - nabsents
puts sprintf("Total = %d, presents = %d (%d%%), absents = %d (%d%%)", total, presents, presents * 100 / total, nabsents, nabsents * 100 / total)
absents.keys.sort.each do |mot|
	puts mot
end
