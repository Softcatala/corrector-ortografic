# Cerca les paraules absents d'una llista
require 'hunspell'

BASE = '../../resultats/hunspell'
gen = Hunspell.new("#{BASE}/catalan.aff", "#{BASE}/catalan.dic")
avl = Hunspell.new("#{BASE}/avl.aff", "#{BASE}/avl.dic")
p gen, avl

mots = []
tros = <<-FITROS
allotjador allotjadora allotjadors allotjadores
al·lotell al·lotells
al·lotet al·lotets
amplíssim amplíssima amplíssims amplíssimes
antiesclavista antiesclavistes
antifrancès antifrancesa antifrancesos antifranceses
antiinstitucional antiinstitucionals
antipsiquiatria antipsiquiatries
aset asets
atrezzista atrezzistes
autocomplaent autocomplaents
baixmedieval baixmedievals
baixíssim baixíssima baixíssims baixíssimes
bellíssim bellíssima bellíssims bellíssimes
berberisc berberisca berberiscs berberisques
berén
bloquet bloquets
borbó borbons
botiflerisme botiflerismes
bunyolada bunyolades
calentet calenteta calentets calentetes
calop calops
caríssim caríssima caríssims caríssimes
catalanoaragonès catalanoaragonesa catalanoaragonesos catalanoaragoneses
catrastrofisme catrastrofismes
cintell cintells
claríssim claríssima claríssims claríssimes
comissariar comissariaven comissariarien
conegudíssim conegudíssima conegudíssims conegudíssimes
contacontes
conventualisme conventualismes
corresponsable corresponsables
cosenyor cosenyora cosenyors cosenyores
cossiolet cossiolets
cranioscòpia cranioscòpies
curatorial curatorials
cànyom cànyoms
desafiant desafiants
descoordinació descoordinacions
desdenyable desdenyables
desestacionalitzador desestacionalitzadora desestacionalitzadors desestacionalitzadores
desestacionalitzar desestacionalitzaven desestacionalitzarien
desmemòria desmemòries
destacadíssim destacadíssima destacadíssims destacadíssimes
dialectalment
digníssim digníssima digníssims digníssimes
discretori discretoris
discretíssim discretíssima discretíssims discretíssimes
discriminatòriament
diversificador diversificadora diversificadors diversificadores
dobleret doblerets
dolentíssim dolentíssima dolentíssims dolentíssimes
edafogènesi edafogènesis
educativament
electrònicament
elevadíssim elevadíssima elevadíssims elevadíssimes
embulladíssim embulladíssima embulladíssims embulladíssimes
emocionadament
encimentar encimentaven encimentarien
enforcament
enrevoltar enrevoltaven enrevoltarien
esbucament
escanyadura escanyadures
escoleta escoletes
escriví	?
esperpènticament
estamental estamentals
estampeta estampetes
estilisme estilismes
estoneta estonetes
estudiadíssim estudiadíssima estudiadíssims estudiadíssimes
etnobotànic etnobotànica etnobotànics etnobotàniques
exhaustivament
exigentíssim exigentíssima exigentíssims exigentíssimes
externalització externalitzacions
externalitzar externalitzaven externalitzarien
facilitador facilitadora facilitadors facilitadores
factibilitat factibilitats
falsíssim falsíssima falsíssims falsíssimes
famosíssim famosíssima famosíssims famosíssimes
fefaentment
feixet feixets
ferotgement
ficadís ficadissa ficadís ficadisses
formigonament
francoespanyol francoespanyola francoespanyols francoespanyoles
furibundament
fílmicament
genealògicament
generacionalment
genèticament
geodèsicament
guarnicioneria guarnicioneries
guitarrístic guitarrística guitarrístics guitarrístiques
hispanofília hispanofílies
historicista historicistes
historicoartístic historicoartística historicoartístics historicoartístiques
homotèrmia homotèrmies
hortet hortets
identificatiu identificativa identificatiu identificatives
illeta illetes
immediació immediacions
immòtic immòtica immòtics immòtiques
importantíssim importantíssima importantíssims importantíssimes
inercialment
infantó infantons
infografista infografistes
infrahabitatge infrahabitatges
inicialment
inquietantment
inqüestionat inqüestionada inqüestionat inqüestionades
intercultural interculturals
intermediació intermediacions
irosament
jovenet jovenets
laboralment
laurisilva laurisilves
lingüísticament
liornès liornesa liornesos liorneses
llarguíssim llarguíssima llarguíssims llarguíssimes
llimb llimbs
llunyanament
lul·lístic lul·lística lul·lístics lul·lístiques
l’abans	?
l’aleshores	?
l’avui	?
lúdicament
macroesdeveniment
macroexposició macroexposicions
mediàticament
meteòricament
microclimàtic microclimàtica microclimàtics microclimàtiques
microcàmera microcàmeres
minicomputadora minicomputadores
missionar missionaven missionarien
mocadoret mocadorets
modestíssim modestíssima modestíssims modestíssimes
moltíssim moltíssima moltíssims moltíssimes
multiculturalitat multiculturalitats
muntanyer muntanyera muntanyers muntanyeres
musculoesquelètic musculoesquelètica musculoesquelètics musculoesquelètiques
naixeren	?
naturalístic naturalística naturalístics naturalístiques
ornamentalment
pastanagó pastanagons
paternofilial paternofilials
perillossíssim perillossíssima perillossíssims perillossíssimes
perimetral perimetrals
pesadíssim pesadíssima pesadíssims pesadíssimes
pitiús pitiüsa pitiús pitiüses
planetàriament
politicocultural politicoculturals
politicomilitar politicomilitars
politicosocial politicosocials
popularíssim popularíssima popularíssims popularíssimes
posader posaders
potet potets
predemocràtic predemocràtica predemocràtics predemocràtiques
presumptament
pretesament
previsorament
primeríssim primeríssima primeríssims primeríssimes
principatí principatina principatí principatines
prioritàriament
profrancès profrancesa profrancesos profranceses
prosopogràfic prosopogràfica prosopogràfics prosopogràfiques
pseudodespatx pseudodespatxos
pseudofilosòfic pseudofilosòfica pseudofilosòfics pseudofilosòfiques
pseudofilosòfic pseudofilosòfica pseudofilosòfics pseudofilosòfiques
pseudoradicalisme pseudoradicalismes
psicoanalíticament
psíquicament
quadret quadrets
rarenc rarenca rarencs rarenques
redefinició redefinicions
reemprar reempraven reemprarien
reim	?
reinstaurar reinstauraven reinstaurarien
reintroduir reintroduïen reintroduirien
reinventar reinventaven reinventarien
repressivament
ressenyable ressenyables
revalorització revaloritzacions
riquíssim riquíssima riquíssims riquíssimes
ritualment
secretíssim secretíssima secretíssims secretíssimes
sectorialment
severíssim severíssima severíssims severíssimes
subtitulació subtitulacions
sus
tecnològicament
temàticament
territorialment
tudadissa tudadisses
tumbet tumbets
turísticament
valorable valorables
vastíssim vastíssima vastíssims vastíssimes
veneradíssim veneradíssima veneradíssims veneradíssimes
videoprojector videoprojectors
vitalíciament
zoològicament
FITROS

absents = {}
absents.default = 0
total = 0
tros.split(/[ ,;:.\/"²()—>\[\]\r\n\t]/).each do |mot|
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
