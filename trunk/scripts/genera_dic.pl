# genera_dic.pl
# Eines per generar el diccionari a partir d'entrades_diec.txt i dels afixos

# Regles
# A Primera conjugaci� regular
# B Quatre formes amb canvi t > d, u > v
# C Pronoms personals encl�tics darrere consonant
# D Pronoms personals encl�tics darrere vocal
# E Plurals afegint una s (eventualment amb canvis)
# F Quatre formes afegint a/s/es
# G Plurals afegint ns
# H Quatre formes afegint ns
# I Plurals afegint o
# J Quatre formes afegint a/os/es
# K Quatre formes amb canvi d'accent (accent greu)
# L Quatre formes amb canvi d'accent (accent agut)
# M Verbs en -ir, formes comunes als grups III, IV i V
# N Verbs en -ir, formes incoatives
# O Imperfet d'indicatiu de la segona conjugaci�
# P Formes de la segona conjugaci� semblants a SIM1 sense -c/-gu (perd�s)
# Q Formes de la segona conjugaci� semblants a SIM1 amb -c/-gu (conegu�s)
# R Formes de la segona conjugaci� semblants al gerundi
# S Formes de la segona conjugaci� semblants a "plaus", "absols", etc.
# T Futurs i condicionals a partir de FUT1
# U Formes pures dels verbs en -ir, a partir d'IPR3 (p.e. sent)
# V Article prefixat no combinable
# Y Preposici� "de" apostrofada
# Z Pronoms personals prefixats

package genera_dic;

BEGIN { unshift @INC, './eines' }

use strict;

use sillabes;

my %opcions = &llegeixOpcions(qw/aff dic morfo depura/, 'arrel');
&mostraUs unless (($opcions{'aff'} and $opcions{'dic'}) || $opcions{'depura'});
print "Opcions:\n";
map { print "    $_: $opcions{$_}\n" } sort keys %opcions;

# Defineix paquets d'opcions
# 	1. Crea el fitxer complet per a MySpell
# 	2. Processa nom�s el fitxer mostra2.txt (no per a MySpell)

my $DEPURA = ($opcions{'depura'} ? 1 : 0);
my $PAQUET = ($DEPURA ? '2' : '1');
my $MORFO = ($opcions{'morfo'} ? 1 : 0);

print "Crea el fitxer complet per a MySpell\n" if $PAQUET =~ /1/;
print "Depura; no crea el fitxer per a MySpell\n" if $PAQUET =~ /2/;

# Genera fitxers per a MySpell
my $MYSPELL = (($PAQUET =~ /[1]/) ? 1 : 0);

# Inclou verbs en -ar de Labastida
my $VERBSLB = 0;
$VERBSLB = 0 if $DEPURA;
print "Inclou els verbs en -ar de Labastida\n" if $VERBSLB;

# Inclou paraules extra
my $MESPARAULES = 1;
print "Inclou paraules extra (mes_paraules.txt)\n" if $MESPARAULES;

my $DIEC;
$DIEC = 'entrades/diec.txt';
my $fitxerAntroponims = "entrades/antrop�nims.txt";
my $fitxerLlinatges = 'entrades/llinatges.txt';
$fitxerAntroponims = '' if $DEPURA;
my @fitxersGentilicis = ("entrades/gentilicis.txt", "entrades/gentilicis_estrangers.txt");
@fitxersGentilicis = () if $DEPURA;
my @fitxersToponims = ("entrades/top�nims.txt", "entrades/top�nims_estrangers.txt");
@fitxersToponims = () if $DEPURA;
my @fitxersPropis = ("entrades/marques.txt");
@fitxersPropis = () if $DEPURA;
my @fitxersAbreviatures = ("entrades/abreviatures_duarte.txt");
@fitxersAbreviatures = () if $DEPURA;

my $AFF = $opcions{'aff'} || './catalan.aff';
$AFF .= '.aff' unless $AFF =~ /\.aff/;
$AFF =~ s/\.aff/_999.aff/;
my $REGLES = 'entrades/regles.txt';
my $DIC  = $opcions{'dic'} || 'resultats/catalan.dic';
$DIC .= '.dic' unless $DIC =~ /\.dic/;
$DIC =~ s/\.dic/_1.dic/ unless $MYSPELL;
my $IRR = 'entrades/irregulars.txt';
my $PROVES = 'entrades/proves.txt';

my $GENERADIC = 1;
my $FESPROVES = 0;

my $mostraFormes = $opcions{'arrel'};

print <<FI_LLISTA;
DIEC: $DIEC
Afixos: $AFF
Diccionari: $DIC
Irregulars: $IRR
MostraFormes: $mostraFormes
Fitxer antrop�nims: $fitxerAntroponims  
Fitxer de llinatges: $fitxerLlinatges  
Fitxers gentilicis: @fitxersGentilicis
(Les entrades poden ser un mot (en aquest cas, es calcula el plural i el
femen�), dos mots (el mascul� i el femen� o el singular i el plural))
Fitxers top�nims: @fitxersToponims 
Fitxers abreviatures: @fitxersAbreviatures 


FI_LLISTA

my $edatRegles = -M $REGLES;
my $edatAff    = -M $AFF || 1000;
#print "Edats: $edatAff > $edatRegles\n";
#do 'genera_aff.pl' if $edatAff > $edatRegles;

my @ent = &getDadesBrutes;	# @ent �s un array d'entrades (refer�ncies a un hash)
&mesDadesBrutes(\@ent, 'entrades/paraules_LB.txt') if $VERBSLB;
&mesDadesBrutes(\@ent, 'entrades/mes_paraules.txt') if $MESPARAULES;

my %procs = &preparaProcs;	# prepara la taula de procediments
my %condicions; # les condicions que estan actives
my %regles = &llegeixRegles;
print "Grups actius: ", join(", ", sort keys %condicions), "\n";
my %precalcIPR1;
&creaTaulesIPR1 if $condicions{'013'};	# crea taules de verbs precalculats
my %irregulars = &llegeixIrregulars;
my %termEsp = &prepTermEsp;	# terminacions especials

&comprovaAff if $FESPROVES;
&generaDic if $GENERADIC;

# Genera les formes (agafa els mateixos arguments d'aquest programa)
do 'formes.pl' if $mostraFormes;

#######################################################################

sub mostraUs
{
	# Mostra l'�s del programa
	print <<FINAL;
Genera llistes de paraules per al corrector de MySpell

Opcions: [-aff=fitxer_d'afixos -dic=fitxer_generat [-morfo]] [-depura] arrel
Cal especificar -aff i -dic (opcionalment -morfo) o b� -depura.
Si s'especifica -morfo, es genera un diccionari adequat per a l'an�lisi morfol�gica
Si s'especifica -depura, es genera un diccionari a partir d'un fitxer
predeterminat i es generen les formes de l'arrel.
FINAL
	exit;
}

sub llegeixOpcions
{
	# Llegeix les opcions de la llista d'ordres
	# Espera un array amb el nom de les opcions permeses i el nom dels arguments 
	# que no comencin per -.
	my @args = @_;
	my $def = pop @args;
	my %permesos = map { $_ => 1 } @args;
	my (%valors, $valor);
	my @def;
	for my $un(@ARGV) {
		if ($un =~ /^-(.*?)(=|$)/) {
			my $id = $1;
			$valor = ($un =~ /=(.*)/ ? $1 : '1');
			if (exists $permesos{$id}) {
				$valors{$id} = $valor;
			}
			else {
				push @def, $valor;
			}
		}
		else {
			push @def, $un;
		}
	}
	$valors{$def} = join(' ', @def) if @def;
	return %valors;
}

sub extreuExt
{
	# Extreu trossos d'una cadena enclosos entre <<ID i ID>>
	# Torna un array, el primer element del qual �s la resta de la cadena
	my ($tot, $id) = @_;
	my @res;
	my $cerca = "<<$id\\s*(.*?)\\s*$id>>";
	push(@res,$1) while $tot =~ s/$cerca//;
	unshift @res, $tot;
	return @res;
}

sub mostraError
{
	# mostra un error
	my ($ent0, $desc) = @_;
	print "$ent0: *** Error *** $desc\n";
}

sub acabaVocTon
{
	# diu si una paraula acaba en vocal t�nica
	my ($mot) = @_;
	my ($ini,$fi) = sillabes::divVocTon($mot);
	return length($fi) == 1;
}

sub paraulaAguda
{
	# diu si una paraula �s aguda
	my ($mot) = @_;
	# miram si hi ha m�s d'una s�l�laba a partir de la vocal t�nica
	my ($ini,$fi) = sillabes::divVocTon($mot);
	my @sil = sillabes::sillab($fi);
	return $sil[0] == 1;
}

sub vocalInicial
{
	# diu si una paraula comen�a amb vocal (o amb h+vocal)
	my ($mot) = @_;
	my @sil = sillabes::sillab($mot);
	my $nSil = shift @sil;
	my $ini = $sil[$nSil];
	return 1 if (($ini =~ /^A/) or (($ini =~ /^PA/) and ($mot =~ /^h/i)));
	return 0;
}

sub iuAtones
{
	# diu si una paraula comen�a amb una i o una u �tones
	my ($mot) = @_;
	return 0 unless $mot =~ /^h?[iu]/i;
	my @dos = sillabes::divVocTon($mot);
	return 0 if $dos[0] =~ /^h?$/i;
	return 1;
}

sub notesIrr
{
	# torna les notes d'una entrada irregular
	my ($ent) = @_;
	my $tot = $irregulars{$ent};
	my %notes = map { /(.*?)=(.*)/; $1 => $2; } split(/\s*&\s*/,$tot);
	return %notes;
}

# =======================================
# Processa els diversos tipus de paraules
# =======================================

sub procVerb
{
	# processa els verbs
	my ($ent0, $cat, $irr, @trossos) = @_;
	my @items;
	if (scalar @trossos != 1)
	#~ if (scalar keys %$irr > 0)
	{
		my @ii = map { "$_:" . $irr->{$_}; } keys %$irr;
		my $ii = join('/', @ii);
		mostraError($ent0, "Massa trossos (ent0=$ent0, cat=$cat, irr=$ii, trossos=" . join(", ", @trossos) . ')' );
		return;
	}
	my @participi;	# els trossos que formaran el participi
	
	my $tros = $trossos[0];
	$tros =~ s/(-se|se\047n|\047s)$//;
	my $vocIni;
	#my $flags = '/';
	#$flags .= 'Z' if $tros =~ /^h?([aeou]|i[^aeiou])/;
	
	$vocIni = vocalInicial($tros);

	# cercam un m�nim de regles que generin totes les formes
	my @genera;

	# Processament diferent segons el model de verb

	if ($tros =~ /ar$/)
	{
		# verb del primer grup (-ar)
		# si $MORFO no generam ni infinitiu ni participi
		if (! $MORFO) {
			push @genera, "$tros<INF0>" . ($vocIni ? '/Y' : '');
		}
		# generam a partir de IIM1
		$tros =~ /(.*)r$/;
		push @genera, "$1va<IIM1>/A" . (($vocIni and not $MORFO) ? 'Z' : '');
		@participi = ("$1t", '-ada') unless $MORFO;	# participi per defecte
		if ($condicions{'013'}) {
			# Hem de posar les formes sense desin�ncia
			# miram si cal generar la forma d'IPR1
			if ($tros =~ /[ei]nar$/) {
				# el verb acaba en -inar o -enar
				my @sil = sillabes::sillab($tros);
				my $nsil = shift @sil;
				if ($nsil < 3) {
					# l'arrel t� una s�l�laba, el verb no du accent
					$tros =~ /(.*)ar$/;
					push @genera, "$1<IPR1>" . (($vocIni and not $MORFO)? '/Z' : ''); #"din"
				}
				else {
					# l'arrel t� m�s d'una s�l�laba, el verb dur� accent
					if ($tros =~ /(.*)inar$/) {
						# si acaba en -�n, segur que du accent
						push @genera, "$1�n<IPR1>"  . (($vocIni and not $MORFO) ? '/Z' : ''); # "cam�n"
					}
					else {
						# si acaba en -en, feim servir una taula
						push(@genera, "$precalcIPR1{$tros}<IPR1>" . (($vocIni and not $MORFO) ? '/Z' : '')) if exists $precalcIPR1{$tros};
					}
				}
			}
			if ($tros =~ /sar$/) {
				# el verb acaba en -sar o -ssar
				my @sil = sillabes::sillab($tros);
				my $nsil = shift @sil;
				if ($nsil < 3) {
					# l'arrel t� una s�l�laba, el verb no du accent
					# Si du dues "s", en suprimim una
					$tros =~ /(.*?s)s?ar$/;
					push @genera, "$1<IPR1>" . (($vocIni and not $MORFO) ? '/Z' : ''); # "pos", "pas"
				}
				else {
					# l'arrel t� m�s d'una s�l�laba, el verb dur� accent
					$tros =~ /(.*?s)s?ar$/;
					my $x = $1;
					# si acaba en -as, -is o -us, l'accent �s segur
					push(@genera, "$1�s<IPR1>"  . (($vocIni and not $MORFO) ? '/Z' : '')) if $x =~ /(.*)as$/; # "rep�s"
					push(@genera, "$1�s<IPR1>"  . (($vocIni and not $MORFO) ? '/Z' : '')) if $x =~ /(.*)is$/; # "av�s"
					push(@genera, "$1�s<IPR1>"  . (($vocIni and not $MORFO) ? '/Z' : '')) if $x =~ /(.*)us$/; # "ab�s"
					if ($x =~ /(.*)pos$/) {
						# tots els verbs en -posar, fan "-p�s"
						push(@genera, "$1p�s<IPR1>"  . (($vocIni and not $MORFO) ? '/Z' : ''))
					}
					else {
						# com a darrer recurs, miram la taula
						push(@genera, "$precalcIPR1{$tros}<IPR1>" . (($vocIni and not $MORFO) ? '/Z' : '')) if exists $precalcIPR1{$tros};
					}
				}
			}
		}
	}
	elsif ($tros =~ /(.*)ir$/)
	{
		# verbs en -ir
		@participi = (generaFormaNeta("${tros}�/M","PAR1"),generaFormaNeta("${tros}�/M","PAR2"));	# participi per defecte
		if ($irr->{'IPR3'})
		{
			push @genera, "${tros}�<FUT1>/M" . (($vocIni and not $MORFO) ? 'Z' : '');
			my $ipr3 = $irr->{IPR3};
			push @genera, "$ipr3<IPR3>/U" . ((vocalInicial($ipr3) and not $MORFO) ? 'Z' : '');
			push @genera, "$ipr3<IMP2>";
			push @genera, "$tros<INF0>" . (($vocIni and not $MORFO) ? '/Y' : '');
		}
		else
		{
			push @genera, "${tros}�<FUT1>/MN" . (($vocIni and not $MORFO) ? 'Z' : '');
			push @genera, "$tros<INF0>" . (($vocIni and not $MORFO) ? '/Y' : '');
		}
	}
	elsif ($tros =~ /(re|er)$/)
	{
		# l'infinitiu ha de sortir com una forma independent
		push @genera, "$tros<INF0>" . (($vocIni and not $MORFO) ? '/Y' : '');
		my ($arrel, $fut1);
		
		if ($tros =~ /re$/)
		{
			$tros =~ /(.*)re$/;
			$arrel = $1;
			$fut1 = "${arrel}r�";
		}
		else
		{
			# converteix a verb en -re si acaba en -er
			$tros =~ /(.*)er$/;
			$arrel = $1;
			$arrel =~ tr/�������/aeeioou/;
			if ($arrel =~ /c$/)
			{
				$arrel =~ /(.*)c/;
				$irr->{'PAR'} = "$1�ut" unless exists $irr->{'PAR'};
				$irr->{'IPR3'} = "$1�" unless exists $irr->{'IPR3'};
			}
			$fut1 = "${arrel}er�";
		}
		
		# miram si el gerundi �s regular o irregular
		# generam les formes IPR3 i IMP2 (i IPR1 per al balear i el valenci�)
		my $ger = $irr->{'GER'} || "${arrel}ent";
		push @genera, "$ger<GER0>/R";
		# si el verb no termina en -ure, generam IPR3, etc.
		unless ($arrel =~ /u$/)
		{
			my $ipr3 = $irr->{'IPR3'};
			unless ($ipr3)
			{
				# posam [ei]nt perqu� hi ha verbs amb el gerundi en -int
				# (per exemple, provenir)
				$ger =~ /(.*)[ei]nt/;
				$ipr3 = $1;
			}
			if ($ipr3 =~ /b$/)
			{
				push @genera, "${ipr3}en<IPR6>" . (($vocIni and not $MORFO) ? '/Z' : '');
				# si IPR3 termina en b, se substitueix per p
				$ipr3 =~ s/b$/p/;
				push @genera, "${ipr3}s<IPR2>" . (($vocIni and not $MORFO) ? '/Z' : '');
				push @genera, "$ipr3<IPR3>" . (($vocIni and not $MORFO) ? '/Z' : '');
				push @genera, "$ipr3<IMP2>";
				unless ($irr->{'SIM'})
				{
					# si hi ha SIM irregular, IPR1 el segueix; altrament, el generam
					push @genera, "!$ipr3<IPR1>" . (($vocIni and not $MORFO) ? '/Z' : '');		# !!!! segons grups !!!!
				}
			}
			else
			{
				push @genera, "$ipr3<IPR3>/S" . (($vocIni and not $MORFO) ? 'Z' : '');
				push @genera, "$ipr3<IMP2>";
				unless ($irr->{'SIM'})
				{
					# si hi ha SIM irregular, IPR1 el segueix; altrament, el generam
					push @genera, "!$ipr3<IPR1>" . (($vocIni and not $MORFO) ? '/Z' : '');		# !!!! segons grups !!!!
				}
			}
		}

		# si no hi ha participi irregular, en genera un en -ut
		@participi = ("${arrel}ut", '-uda');

		# per als verbs en -ure, posam la regla que genera IPR2, IPR6 i IMP2
		if ($arrel =~ /u$/)
		{
			die "Qu� fer amb IPR3?" if $irr->{'IPR3'};
			push @genera, "${arrel}<IPR3>/S" . (($vocIni and not $MORFO) ? 'Z' : '');
			push @genera, "${arrel}<IMP2>";
		}
		
		# miram si hi ha una forma per a l'imperfet d'indicatiu
		my $iim = $irr->{'IIM'};
		unless ($iim)
		{
			# si cal, generam l'imperfet a partir del gerundi, o de l'arrel
			if ($irr->{'GER'})
			{
				$irr->{'GER'} =~ /(.*)[ie]nt$/;
				$iim = $1 . 'ia';
				# si el gerundi t� una i, evitam -ii-
				$iim =~ s/iia$/ia/;
			}
			else
			{
				$iim = "${arrel}ia";
			}
		}
		push @genera, "$iim<IIM1>/O" . (($vocIni and not $MORFO) ? 'Z' : '');

		# miram si l'imperfet de subjuntiu �s regular o irregular
		my $sim = $irr->{'SIM'};
		if ($sim)
		{
			if ($sim =~ /[gq]u�s$/)
			{
				push @genera, "$sim<SIM1>/Q" . (($vocIni and not $MORFO) ? 'Z' : '');
			}
			else
			{
				push @genera, "$sim<SIM1>/P" . (($vocIni and not $MORFO) ? 'Z' : '');
			}
		}
		else
		{
			push @genera, "${arrel}�s<SIM1>/P" . (($vocIni and not $MORFO) ? 'Z' : ''); # formes semblants a SIM1 regular
		}

		# miram si el futur �s regular o irregular
		if ($irr->{'FUT'})
		{
			push @genera, "$irr->{FUT}<FUT1>/T" . (($vocIni and not $MORFO) ? 'Z' : '');
		}
		else
		{
			push @genera, "$fut1<FUT1>/T" . (($vocIni and not $MORFO) ? 'Z' : '');	# formes derivades de "perdr�", "temer�", etc.
		}
	}
	else
	{
		push @genera, "$tros" . (($vocIni and not $MORFO) ? '/Z' : '');
	}

	# A partir d'aqu�, es tornen processar tots els models
	
	# llevam les formes incloses a -=
	if ($irr->{'-'})
	{
		# genera totes les formes i deixa nom�s les que no surten a -=
		my $menys = $irr->{'-'};
		# si es vol llevar el participi, el llevam de @participi
		@participi = () if $menys =~ /PAR/;
		my @totes = generaFormes(@genera, '.');
		@genera = ();
		map {
			unless (/$menys/ || /PAR/) {
				if (/(INF|GER|IMP)/) {
					s/Z//;
					s/\/$//;
				}
				else {
					$_ .= ((vocalInicial($_) and not $MORFO)? ($_ =~ m!/! ? ($_ =~ m!/.*Z! ? '' : 'Z'): '/Z') : '');
				}
				push @genera, $_;
			}
		} @totes;
	}

	# afegim les formes irregulars generades per +=
	if ($irr->{'+'})
	{
		my $mes = $irr->{'+'};
		$mes =~ s/^\s+//; $mes =~ s/\s+$//; $mes =~ s/\s\s+/ /g;
		my @mes = split(/\s+/, $mes);
		my @nous;
		for my $un(@mes)
		{
			$vocIni = vocalInicial($un);
			if ($un =~ m!(.*?/.*?)/(.*)!)
			{
				my ($base, $cond) = ($1,$2);
				map {
					unless (/(INF|GER|PAR|IMP)/) {
						$_ .= '/Z' if (($vocIni and not $MORFO) && $_ !~ m!/Z!);
					}
					push @nous, $_;
				} generaFormes(($MYSPELL ? '' : '+') . $base, $cond);
			}
			elsif ($un =~ m!.*/.*!)
			{
				if ($un =~ /PAR/) {
					# si �s un participi potser hem de generar m�s d'una forma
					my @par = generaFormaNeta($un, '0');
					my $par0 = shift @par;
					@par = generaFormaNeta($un, '2');
					my $par2 = shift @par;
					#print "Participi $par0 / $par2\n";
					map { s!/!<PAR0>/!; push(@nous, $_) } procMFSP($par0, 'adj.', 0, $par0, $par2);
				}
				else {
					# si no �s un participi, no desplegam les formes
					# PER FER: assegurar-se que els flags s�n v�lids per a totes les formes
					push @nous, ($MYSPELL ? '' : '+') . $un . (($vocIni and not $MORFO) ? (($un =~ /(INF|GER)/) ? ('VY') : ('Z')) : (''));
				}
			}
			else
			{
				#push @nous, ($MYSPELL ? '' : '+') . $un . ((($vocIni and not $MORFO) && ($un !~ /(INF|GER|PAR|IMP)/)) ? '/Z' : (/INF/ ? '/Y' : ''));
				push @nous, ($MYSPELL ? '' : '+') . $un . (($vocIni and not $MORFO) ? (($un =~ /INF/) ? '/Y' : (($un =~ /(GER|PAR|IMP)/) ? '' : '/Z')) : '');
				#print 'Afegit: ' . $un . (($vocIni and not $MORFO) ? (($un =~ /INF/) ? '/Y' : (($un =~ /(GER|PAR|IMP)/) ? '' : '/Y')) : '') . "\n";
			}
		}
		#print "Afegeix nous: ", join(", ", @nous), "\n";
		push @genera, @nous;
	}
	
	# afegim les regles de @genera
	push @items, join(" ",@genera);

	unless ($MORFO)
	{
		# afegim les formes que poden dur pronoms darrere
		my @formes = generaFormes(@genera, '(INF|GER|IMP)');
		#print "Formes (INF|GER|IMP): ", join(", ", @formes), "\n";
		#print "Genera: ", join(", ",@genera), "\nFormes: ", join(", ",@formes), "\n";
		my ($nouFlag, $nou);
		my @nous = ();
		my $noAcc;
		map 
		{
			#s/<.*>//;
			$noAcc = $_;
			$noAcc =~ s/�</a</;
			$noAcc =~ s/[����]/c/g;
			$nouFlag = (/\// ? '' : '/') . ($noAcc =~ /([aei]\b|[^aeiou]u\b|[��]<)/ ? 'D' : 'C');
			$nou = "$_$nouFlag";
			$nou =~ s!(/.*)Z!$1!;
			push @nous, $nou; 
		} @formes;
		push @items, @nous;
	}

	# afegim els participis (amb els flags que toqui, com els adjectius)
	if ($irr->{'PAR'})
	{
		my $par = $irr->{'PAR'};
		$vocIni = vocalInicial($par);
		# els participis s�n com els adjectius
		if ($par =~ m!(.*)/(.*)!)
		{
			# si ja du flag, no cercam m�s
			#push @genera, "$1<PAR0>/$2" . (($vocIni and not $MORFO) ? 'Y' : '');
			@participi = ($1, generaFormaNeta($par, '2'));
		}
		elsif ($par =~ /[aiu]t$/)
		{
			# alternan�a t/d (flag B)
			#push @genera, "$par<PAR0>/B" . (($vocIni and not $MORFO) ? 'Y' : '');
			@participi = ($par, generaFormaNeta("$par/B", '2')); 
		}
		elsif ($par =~ /[�e��o��u�]s$/)
		{
			#push @genera, "$par<PAR0>/J" . (($vocIni and not $MORFO) ? 'Z' : '');
			@participi = ($par, generaFormaNeta("$par/J", '2')); 
		}
		else
		{
			# sempre t (flag F)
			# push @genera, "$par<PAR0>/F" . (($vocIni and not $MORFO) ? 'Z' : '');
			@participi = ($par, generaFormaNeta("$par/F", '2')); 
		}
	}
	if (@participi) {
		map { s!/!<PAR0>/!; push(@items, $_) } procMFSP("$participi[0]", 'adj.', $irr, @participi);
	}

	return @items;
}

sub procMFSP
{
	# paraules amb quatre formes ((m/f) x (s/p)) 
	my ($ent0, $cat, $irr, @trossos) = @_;
	my @items;
	my $flags;
	my @bons;
	my $tros;
	my $error;
	if (scalar @trossos > 1)
	{
		# si hi ha m�s d'un tros, hi ha informaci� per generar el femen�
		my $ent = $trossos[0];
		while ($tros = shift @trossos)
		{
			if ($tros =~ /^(-a)$/)
			{
				# (�s un femen� regular)
				# s'afegeix una -a al femen�
				if ($ent =~ /[s�x]$/) {
					push @bons, "${ent}a";	# ex. ter�a, immensa
				}
				else {
					push @bons, generaFormaNeta("$ent/F", '2');
				}
			}
			elsif ($tros =~ /^-(.*)/)
			{
				# (�s un femen� especial)
				# generam la paraula
				my $term = $1;
				if (vocalInicial($term))
				{
					my ($ini,$fi) = &sillabes::divVocTon($ent);
					push @bons, "$ini$term";
				}
				else {
					my $masc = $termEsp{$term};
					if (!$masc)
					{
						print "Terminaci� desconeguda: $term\n";
					}
					else
					{
						$masc .= '$';
						my $x = $ent;
						$x =~ s/$masc/$term/;
						push @bons, $x;
					}
				}
			}
			else
			{
				# (altres casos)
				$error = 1 if $tros =~ /^-/;
				push @bons, $tros;
			}
		}
	}
	else
	{
		# Nom�s hi ha un tros, s'ha de deduir tot
		push @bons, @trossos;
	}
	if ($error || scalar @bons > 2)
	{
		for $tros(@bons)
		{
			push @items, $MYSPELL ? '' : '>>>' . $tros;
		}
	}
	elsif (scalar @bons == 1)
	{
		# si nom�s hi ha un tros, nom�s t� singular i plural
		# per tant, funciona igual que un substantiu amb singular/plural
		if ($trossos[0] =~ /�$/) {
			# un adjectiu de tres terminacions
			push @items, procSP($ent0, $cat, $irr, $trossos[0]);
			my $fpl = $trossos[0];
			$fpl =~ s/�$/ces/;
			push @items, $fpl . ((vocalInicial($fpl) and not $MORFO) ? '/Y' : '');
			return @items;
		}
		else {
			#print ">>> Adjectiu S/P: $ent0\n";
			push @items, procSP($ent0, $cat, $irr, @trossos);
			return @items;
		}
	}
	else # (scalar @bons == 2)
	{
		# si queden dos mots, poden ser el mascul� i el femen�
		# Miram si comen�a per vocal, i si el femen� es pot apostrofar
		my $vocIni = (vocalInicial($bons[0]) and not $MORFO);
		# Si $MORFO, no posam informaci� sobre ap�strofs
		my $femNoApo = ($vocIni && iuAtones($bons[0])) ? 1 : 0;
		#print "$bons[0] -> vocIni=$vocIni, femNoApo=$femNoApo\n";
		# provam algunes regles
		my $fet;
		for my $regla('F', 'J', 'B', 'H', 'K', 'L')
		{
			my $fem = generaFormaNeta("$bons[0]/$regla", '2');
			if ($fem eq $bons[1])
			{
				# la regla H no �s v�lida si la darrera vocal no �s t�nica
				# print ">>> $bons[0] --($regla)--> $fem\n";
				next unless ($regla ne 'H' or acabaVocTon($bons[0]));
				if (!$vocIni) {
					# no comen�a per vocal
					push @items, "$bons[0]/$regla";
				}
				elsif ($femNoApo) {
					# comen�a per vocal, per� el femen� no pot dur ap�strof
					push @items, "$bons[0]/VY$regla";
				}
				else {
					# comen�a per vocal, i el femen� pot dur ap�strof
					# cream una entrada per al mascul� singular i plural
					my $rsp = $regla;
					$rsp =~ tr[FJBHKL]
							  [EIEGEE];
					# Original: if (generaFormaNeta("$bons[0]/$regla", '2')) !!!!!!!!!!
					if (generaFormaNeta("$bons[0]/$regla", '2'))
					{
						#push @items, "$bons[0]/VY$rsp";
						push @items, "$bons[0]/VY$regla";
					}
					else
					{
						push @items, "$bons[0]/VY";
						push @items, generaFormaNeta("$bons[0]/$regla", '3') . '/Y';
					}
					# cream una entrada per al femen� singular i plural
					$rsp = $regla;
					$rsp =~ tr[FJBHKL]
							  [EEEEEE];
					push @items, "$fem/VY$rsp";
				}
				$fet = 1;
				last;
			}
		}
		# si no hem trobat cap regla, posam totes les formes
		my $marca = $MYSPELL ? '' : '>>>';
		unless ($fet)
		{
			my $x;
			$x = $bons[0];
			push @items, "$marca$x/E" . ((vocalInicial($x) and not $MORFO) ? 'YV' : '');
			$x = $bons[1];
			push @items, "$marca$x/E" . ((vocalInicial($x) and not $MORFO) ? ('Y' . (iuAtones($x) ? '' : 'V')) : '');
		}
	}
	return @items;
}

sub procSP
{
	# paraules amb dues formes (s/p)
	my ($ent0, $cat, $irr, @trossos) = @_;
	my @items;
	my $flags = '/';
	my (@bons, $tros);
	if (scalar @trossos > 1)
	{
		return procDef($ent0, $cat, $irr, @trossos);
	}
	my $ent = $trossos[0];
	# Miram si comen�a per vocal, i si el femen� es pot apostrofar
	my $vocIni = (vocalInicial($ent) and not $MORFO);
	my $femNoApo = ($vocIni && $cat =~ /f\./ && iuAtones($ent)) ? 1 : 0;
	#print "$ent -> vocIni=$vocIni, femNoApo=$femNoApo\n";
	# El plural est� precalculat
	if (exists $irr->{'PLU'})
	{
		for my $unPlural(split(/\s+/,$irr->{'PLU'}))
		{
			next if $unPlural eq 'o';
			if ($unPlural !~ /^-/)
			{
				# (no comen�a amb un guionet)
				#~ print "$ent/E ($unPlural)-> ", generaFormaNeta("$ent/E", '.'), "\n";
				if (generaFormaNeta("$ent/E", '.') eq $unPlural)
				{
					#$flags .= 'E';
					# el plural est� precalculat, per� coincideix amb una regla
					push @items, "$ent/E" . ($vocIni ? 'Y' . ($femNoApo ? '' : 'V') : '');
				}
				else
				{
					# push @items, ">>> $ent + $irr->{PLU}";
					push @items, $ent . ($MORFO ? '<SNG>' : '') . ($vocIni ? '/Y' . ($femNoApo ? '' : 'V') : '');
					push @items, $unPlural . ($MORFO ? '<PLU>' : '') . ($vocIni ? '/Y' : '');
				}
			}
			else
			{
				# (comen�a amb un guionet)
				# Generam la forma
				my ($ini,$fi) = &sillabes::divVocTon($ent);
				$unPlural =~ /^-(.*)/;
				my $plu = "$ini$1";
				my @s1 = sillabes::sillab($ent);
				my @s2 = sillabes::sillab($plu);
				my ($ns1, $ns2) = (shift @s1, shift @s2);
				print "!!! $ent ($ns1) / $plu ($ns2)\n" if $ns1 gt $ns2;
				#~ print ">>> $ent -> $plu\n";
				my $trobat = 0;
				for my $flag qw/E G/ {
					my $forma = generaFormaNeta("$ent/$flag", '.');
					# print "$ent/$flag -> $forma\n";
					if ($forma eq $plu) {
						unless ($MORFO) {
							push @items, "$ent/$flag" . ($vocIni ? 'Y'  . ($femNoApo ? '' : 'V') : '');
						}
						else {
							push @items, "$ent/$flag";
						}
						$trobat = 1;
						last;
					}
				}
				unless ($trobat) {
					# Si no hem trobat cap regla, posam les dues formes
					push @items, $ent . ($vocIni ? '/Y' . ($femNoApo ? '' : 'V') : '');
					push @items, $plu . ($vocIni ? '/Y' : '');
				}
			}
		}
		return @items;
	}
	# El plural no est� precalculat
	if (acabaVocTon($trossos[0]) and $flags eq '/')
	{
		# si acaba en vocal t�nica, plural afegint -ns
		$flags .= 'G';
	}
	if ($trossos[0] =~ /[sx�]$/ and paraulaAguda($trossos[0]) and $cat !~ /f\./)
	{
		# si acaba en -s i �s aguda, plural afegint -os (si �s mascul�)
		$flags .= 'I';
	}
	if ($trossos[0] =~ /[ei]n$/ and ! paraulaAguda($trossos[0]))
	{
		print "!!! $trossos[0]\n";
	}
	# per defecte, plural afegint -s
	# PER FER: no aplicar regla E si la paraula acaba en -s i no �s aguda
	if ($vocIni and not $MORFO) {
		# primer la regla per fer els plurals
		$flags .= 'E' if $flags eq '/';
		# totes les formes poden dur "d'"
		$flags .= 'Y';
		$flags .= 'V' unless $femNoApo; 
	}
	else {
		$flags .= 'E' if $flags eq '/';
	}
	push @items, "$trossos[0]$flags";
	return @items;
}

sub procDef
{
	# proc�s per defecte
	my ($ent0, $cat, $irr, @trossos) = @_;
	return join("_", @trossos) if $MORFO;
	my @items;
	if (exists $irr->{'PLU'}) {
		my $plu = $irr->{'PLU'};
		$plu .= '/Y' if (&vocalInicial($plu) and not $MORFO);
		push @items, $plu;
	}
	for my $tros(@trossos)
	{
		my $flags = (&vocalInicial($tros) and not $MORFO) ? '/Y' : '';
		push(@items, "$tros$flags") unless $tros =~ /(^-|-$)/;
	}
	return @items;
}

sub preparaProcs
{
	# Prepara la taula de procediments
	my %taula = (
		'adj.' => \&procMFSP,
		'adj. i f.' => \&procMFSP,
		'adj. i m.' => \&procMFSP,
		'adj. i pron.' => \&procMFSP,
		'adv.' => \&procDef,
		'art.' => \&procDef,
		'conj.' => \&procDef,
		'f.' => \&procSP,
		'f. pl.' => \&procDef,
		'interj.' => \&procDef,
		'intr.' => \&procDef,
		'loc. adj.' => \&procDef,
		'loc. adj. i loc. adv.' => \&procDef,
		'loc. adv.' => \&procDef,
		'loc. prep.' => \&procDef,
		'm.' => \&procSP,
		'm. i f.' => \&procMFSP,
		'm. pl.' => \&procDef,
		'pl.' => \&procDef,
		'prep.' => \&procDef,
		'pron.' => \&procDef,
		'sing.' => \&procDef,
		'tr.' => \&procVerb,
		'tr. pron.' => \&procVerb,
		'xxx' => \&procDef,
		'v. intr.' => \&procVerb,
		'v. intr. i tr.' => \&procVerb,
		'v. intr. i pron.' => \&procVerb,
		'v. pron.' => \&procVerb,
		'v. tr.' => \&procVerb,
		'v. tr. i intr.' => \&procVerb,
		'v. tr. i pron.' => \&procVerb
	);
	return %taula;
}

sub getDadesBrutes
{
	# agafa les dades brutes del DIEC
	my @ent;
	my $linia;
	open ENT, "<$DIEC" or die "No es pot obrir $DIEC";
	while ($linia = <ENT>)
	{
		next if $linia =~ m!^\s*//!;
		next if $linia =~ /^\s*$/;
		my $dades = {};
		map { /(.*?)=(.*)/; $dades->{$1} = $2; } split(/\^/,$linia);
		#~ $dades->{'linia'} = $linia;
		push @ent, $dades;
	}
	close ENT;
	return @ent;
}

sub mesDadesBrutes
{
	# agafa m�s les dades brutes
	my ($ent,$fitxer) = @_;
	my $linia;
	open ENT, "<$fitxer" or die "No es pot obrir $fitxer";
	while ($linia = <ENT>)
	{
		next if $linia =~ m!^\s*//!;
		next if $linia =~ /^\s*$/;
		my $dades = {};
		map { /(.*?)=(.*)/; $dades->{$1} = $2; } split(/\^/,$linia);
		#~ $dades->{'linia'} = $linia;
		push @$ent, $dades;
	}
	close ENT;
}

sub generaDic
{
	# genera el diccionari
	my $nLin = 0;
	my $dades;
	my @items;
	while ($dades = shift @ent)
	{
		#~ print $dades->{'linia'};
		$nLin++;
		my $ent = $dades->{'ent'};
		my $ent0 = $ent;
		my $cat = $dades->{'cat1'};
		$cat = 'xxx' unless $cat;
		# lleva n�meros
		$ent =~ s/\d$//;
		$ent =~ s/\d / /;
		# si el mot �s un prefix, no el posa a la llista
		if ($ent =~ /(^-|-$)/ or ! $cat)
		{
			# print "Ignora \"$ent0\"\n";
			next;
		}
		push @items, ">ID=DIEC$nLin" if $MORFO;
		push @items, "~~ $ent0 (" . join(", ",map { $_ eq 'ent' ? () : ($dades->{$_}) } keys %$dades) . ")~~" unless $MYSPELL;
		&unaEnt(\@items, $ent0, $ent, $cat, $irregulars{$ent0}, $dades);
		push @items, '=' x 77 unless $MYSPELL;
		last if $nLin == -1000 && $MORFO;
	}
	unless ($MORFO) {
		# Afegeix els auxiliars 'vaig', 'vares', etc.
		push @items, qw/vagi vagis vagi v�gim v�giu vagin vaig vas vares va vam v�rem vau v�reu van varen/;
		# Afegeix els pronoms personals
		push @items, qw/em en es et hi ho la la'n l'en les l'hi li li'n me me'l me'ls me'n m'hi m'ho ne n'hi nos se se'l se'ls se'm se'n se'ns se't s'hi s'ho te te'l te'ls te'm te'n te'ns t'hi t'ho us vos/;
		# afegeix els nombres
		push @items, &llistaNombres;
		# afegeix els noms propis de persona i els llinatges
		push @items, &antroponims;
		# afegeix els gentilicis
		push @items, &gentilicis;
		# afegeix els top�nims
		push @items, &toponims;
		# afegeix els noms propis
		push @items, &propis;
		# afegeix les abreviatures
		push @items, &abreviatures;
	}
	# Compacta la llista
	my @compactat;
	if ($MYSPELL and !$MORFO) {
		@compactat = compactaItems(@items);
	}
	else {
		@compactat = map { if (/^(>|~~)/) { $_ } else { s/ +/\n/g; $_ } } @items;
	}
	open SORT, ">$DIC" or die "No es pot obrir $DIC";
	# posa una l�nia per a <S-F9>
	print SORT "//!perl c:/documents/programes/prepmyspell/formes.pl ! %l\n" unless $MYSPELL;
	print SORT scalar @compactat, "\n" if $MYSPELL;
	print SORT join("\n",@compactat);
	close SORT;
}

sub unaEnt
{
	# Processa una entrada
	my ($items, $ent0, $ent, $cat, $irr, $dades) = @_;
	if ($irr =~ /<<EXT/)
	{
		# processa recursivament els camps <<EXT ... EXT>>
		my @alt = &extreuExt($irr, 'EXT');
		for my $x(@alt) {
			&unaEnt($items, $ent0, $ent, $cat, $x, $dades);
		}
		return;
	}
	my %irr = map { /(.*?)\s*=\s*(.*)/; $1 => $2; } split(/\s*&\s*/,$irr);
	return if exists $irr{'IGNORA'};
	# si la paraula segueix un model, adaptam les irregularitats
	if (exists $irr{'MODEL'})
	{
		my @trossos = split(/\//, $irr{'MODEL'});
		my $model = shift @trossos;
		my %mod = &notesIrr($model);
		unless (%mod) {
			print "No es troba el model $irr{MODEL}\n" ;
			return;
		}
		my $un;
		my ($vell,$nou, %nouIrr);
		%nouIrr = %mod;
		while ($vell = shift @trossos)
		{
			$nou = shift @trossos;
			$vell = "\\b$vell";
			map 
			{
				$nouIrr{$_} =~ s/$vell/$nou/g;
			} keys %nouIrr;
		}
		# mesclam els valors addicionals
		map 
		{ 
			if ($_ eq '+')
			{
				if (exists $nouIrr{$_})
				{
					$nouIrr{$_} .= " $irr{$_}";
				}
				else
				{
					$nouIrr{$_} = $irr{$_};
				}
			}
			else
			{
				$nouIrr{$_} = $irr{$_};
			}
		} keys %irr;
		# %irr = %nouIrr;
		# processam el model
		my $nouIrr = join(" & ", map { /MODEL/ ? () : ("$_=$nouIrr{$_}") } keys %nouIrr);
		# print "MODEL: $irr -> $nouIrr\n";
		&unaEnt($items, $ent0, $ent, $cat, $nouIrr, $dades);
		return;
		# map { print "  $_: $irr{$_}\n" } sort keys %irr;
	} # ... if $irr{'MODEL'}
	# mira si est� precaulculat
	if ($irr{'PRECALC'}) {
		push @$items, split(/\s+/, $irr{'PRECALC'});
	}
	else {
		# mira si hi ha informaci� sobre el plural
		if (!$irr{'PLU'} and $dades->{'com1'} =~ /^pl\.\s+(.*)/) {
			$irr{'PLU'} = $1;
			#~ print "Irregulars: ";
			#~ map { print "$_=$irr{$_} " } keys %irr;
			#~ print "\n";
		}
		$ent = $irr{'ALT'} if exists $irr{'ALT'};
		# processa les diverses parts de l'entrada
		my @trossos = split(/\s+/,$ent);
		$cat = $irr{'NOVACAT'} if $irr{'NOVACAT'};
		push @$items, ">CAT=$cat" if $MORFO;
		push(@$items, $procs{$cat}->($ent0, $cat, \%irr, @trossos)) if $cat;
	}
}

sub compactaItems
{
	# compacta una llista d'�tems i els ordena
	my @llista = @_;
	my @items = map { s/!//g; s/<.+?>//g; split(/\s+/, $_) } @llista;
	@items = sort map { s!/! !; $_; } @items;
	# Posam un �tem nou per assegurar-nos que el darrer sortir�
	push @items, '__EOF__';
	my (@bons, $un, $arrel, $ant, $flags, $nous);
	$ant = $items[0];	# per evitar escriure un nom en blanc
	$ant =~ s/ .*//;
	for $un(@items)
	{
		($arrel,$nous) = ($un =~ m!(.*) (.*)!) ? ($1,$2) : ($un, '');
		if ($arrel ne $ant)
		{
			# ha aparegut un valor nou
			if (length $flags > 1)
			{
				# si hi ha m�s d'un flag, els ordena i suprimeix els repetits
				$flags = join('',sort split(/\B/,$flags));
				1 while $flags =~ s/(.)\1/$1/;
			}
			$flags = "/$flags" if $flags;
			push @bons, "$ant$flags" if $ant;
			$flags = $nous;
			$ant = $arrel;
		}
		else
		{
			# l'arrel es repeteix
			$flags .= $nous;
			# print "Arrel repetida: $arrel\n";
		}
	}
	return @bons;
}

sub llegeixRegles
{
	# llegeix les regles
	my %regles;
	open ENT, "<$AFF" or die "No es pot obrir $AFF";
	my $linia;
	my ($temps, $persona, $cat);
	while ($linia = <ENT>)
	{
		if ($linia =~ /^# grups: (.*)/) {
			%condicions = map { $_=>1 } split(/\s/, $1);
		}
		next unless $linia =~ /^([SP]FX)\+?\s+(.)\s+(.)\s+(\d+)/;
		my ($tipus, $flag, $cross, $quants) = ($1,$2,$3,$4);
		my $items = [];
		$regles{$flag} = {'tipus'=>$tipus, 'desc'=>'Sense descripci�', 'nItems'=>$quants };
		my $i = 0;
		$temps = 'XXX';
		$persona = '0';
		while ($i<$quants)
		{
			$linia = <ENT>;
			# exemple: SFX A ar o ar # 1. canto - indicatiu present <IPR>
			next unless $linia =~ /^[SP]FX\s+.\s+(\S+)\s+(\S+)\s+(\S+)\s+(.*)/;
			$i++;
			my ($sup, $afix, $cond, $desc) = ($1, $2, "$3\$", $4);
			$sup = length $sup unless $sup eq '0';
			$temps = $1 if $desc =~ /<(...)>/;
			$persona = $1 if ($desc =~ /(\d)\./); # && ($temps ne 'XXX'));
			push @$items, [$sup, $afix, $cond, $desc, "$temps$persona"];
		}
		$regles{$flag}->{'items'} = $items;
	}
	close ENT;
	return %regles;
}

sub generaFormes
{
	# genera les formes d'una arrel
	# arguments: @bases (les arrels), $forma (una expressi� regular per a temps/persona)
	# posa la informaci� gramatical de cada forma
	# afegeix els prefixos a les formes generades
	my $forma = pop @_;
	my @bases = @_;
	my $flags;
	my @res;
	my $tempsDef = 'XXX';
	for my $base(@bases)
	{
		$tempsDef = 'XXX';
		$flags = (($base =~ s!(.*)/(.*)!$1!) ? $2 : '');
		my ($i,$flag);
		my ($prefixos, $sufixos);
		for ($i=0; $i<length($flags); $i++) {
			$flag = substr($flags, $i, 1);
			if ($regles{$flag}->{'tipus'} =~ 'PFX') { $prefixos .= $flag }
			else { $sufixos .= $flag }
		}
		if ($prefixos) {
			$prefixos = "/$prefixos";
		}
		if ($base =~ s/<(....)>//)
		{
			# anotam el temps i la persona precalculats, si cal
			my $tp = $1;
			if ($tp =~ $forma)
			{
				push @res, "$base<$tp>$prefixos";
				$tp =~ /(...)./;
				$tempsDef = $1;
			}
		}
		for ($i=0; $i<length($sufixos); $i++)
		{
			$flag = substr($sufixos, $i, 1);
			my $nItems = $regles{$flag}->{'nItems'};
			my $j;
			my $antTemps = '';
			for ($j=0; $j<$nItems; $j++)
			{
				my $items = $regles{$flag}->{items};
				my $item = $$items[$j];
				my ($sup, $afix, $cond, $desc, $tp) = @$item;
				$tp =~ s/^XXX/$tempsDef/;
				my $ini = $sup ? substr($base, 0, -$sup) : $base;
				next unless $base =~ $cond;
				my $iatp = "$ini$afix<$tp>";
				next unless $iatp =~ $forma;
				push @res, "$iatp$prefixos";
			}
		}
	}
	return @res;
}

sub generaFormaNeta
{
	# Com genera formes, per� elimina la informaci� gramatical
	my @res = generaFormes(@_);
	my $net = $res[0];
	$net =~ s/<.*?>//;
	return $net;
}

sub llegeixIrregulars
{
	# llegeix les formes irregulars
	# elimina els trossos inclosos dins <<999 ... 999>>,
	# segons les condicions vigents
	my %irr;
	open ENT, "<$IRR" or die "No es pot obrir $IRR";
	my $linia;
	while ($linia = <ENT>)
	{
		chomp $linia;
		next if $linia =~ m!^\s*//!;
		next unless $linia =~ /(.*?):\s*(.*)/;
		my ($id,$cont) = ($1,$2);
		if ($cont =~ /<</) {
			#print "$cont -> ";
			$cont =~ s/<<(!?\d{3})(.*?)\1>>/&trosCond($1,$2)/eg;
			#print "\"$cont\"\n";
		}
		$cont =~ s/^\s+//;
		$cont =~ s/\s+$//;
		$irr{$id} = $cont;
	}
	return %irr;
}

sub trosCond
{
	# Decideix si un tros s'ha de mantenir o esborrar
	my ($grup,$cont) = @_;
	return $condicions{$grup} ? $cont : '';
}

sub comprovaAff
{
	# Fa les comprovacions del fitxer de proves
	open ENT, "<$PROVES" or die "No es pot obrir $PROVES";
	my ($linia, @exits, @errors, $nExits, $nErrors);
	($nExits, $nErrors) = (0,0);
	while ($linia = <ENT>)
	{
		chomp $linia;
		$linia =~ s!\s*//.*!!;
		next unless $linia;
		my ($arrel, $flags, $codi, $op, $formes);
		unless ($linia =~ m!(.*?)/(.*?):\s*(.*?)\s*(==)\s*(.*)!)
		{
			push(@errors, "Error: $linia");
			next;
		}
		($arrel, $flags, $codi, $op, $formes) = ($1,$2,$3,$4,$5);
		# push @errors, "Llegit: " . join(" + ", $arrel, $flags, $codi, $op, $formes);
		my @gen = map { s/<.*>//; $_; } generaFormes("$arrel/$flags",$codi);
		my $gen = join(", ",@gen);
		my $ordGen = join(", ",sort @gen);
		my $formes = join(", ", split(/,\s*/,$formes));
		my $ordFormes = join(", ", sort split(/,\s*/,$formes));
		# push @errors, "Compara \"$gen\" i \"$formes\"";
		if ($op eq '==')
		{
			if ($ordGen eq $ordFormes)
			{
				push @exits, "$arrel ($codi) == $gen";
				++$nExits;
			}
			else
			{
				push @errors, "$arrel ($codi):", "AFF: $gen", "Esp: $formes";
				++$nErrors;
			}
		}
		else
		{
			push @errors, "Error: " . join(" + ", $arrel, $flags, $codi, $op, $formes);
			++$nErrors;
		}
	}
	close ENT;
	print "Resultat de les proves ($PROVES)\n";
	print "$nErrors errors:\n", map { "\t$_\n" } @errors;
	print "$nExits �xits:\n";
	#print map { "\t$_\n" } @exits;
}

sub prepTermEsp
{
	# prepara la taula de terminacions especials
	return (
		'blanca' => 'blanc',
		'blava' => 'blau',
		'buida' => 'buit',
		'centena' => 'cent�',
		'centes' => 'cents',
		'curta' => 'curt',
		'llarga' => 'llarg',
		'reveixina' => 'reveix�',
		'rica' => 'ric',
		'riquenya' => 'riqueny',
		'robada' => 'robat',
		'rodona' => 'rod�',
		'rogenca' => 'rogenc',
		'roja' => 'roig',
		'rompuda' => 'romput',
		'rossenca' => 'rossenc',
		'rugada' => 'rugat',
		'rutllada' => 'rutllat',
		'seca' => 'sec',
		'segada' => 'segat',
		'serrada' => 'serrat',
		'xuclada' => 'xuclat',
	);
}

sub llistaNombres
{
	# torna la llista de nombres entre 0 i 99, i 100, 200, 300, 400, 500, 600, 700, 800, 900
	return qw/zero un una dos dues tres quatre cinc sis set vuit nou deu onze dotze tretze catorze quinze setze desset devuit denou vint vint-i-un vint-i-una vint-i-dos vint-i-dues vint-i-tres vint-i-quatre vint-i-cinc vint-i-sis vint-i-set vint-i-vuit vint-i-nou trenta trenta-un trenta-una trenta-dos trenta-dues trenta-tres trenta-quatre trenta-cinc trenta-sis trenta-set trenta-vuit trenta-nou quaranta quaranta-un quaranta-una quaranta-dos quaranta-dues quaranta-tres quaranta-quatre quaranta-cinc quaranta-sis quaranta-set quaranta-vuit quaranta-nou cinquanta cinquanta-un cinquanta-una cinquanta-dos cinquanta-dues cinquanta-tres cinquanta-quatre cinquanta-cinc cinquanta-sis cinquanta-set cinquanta-vuit cinquanta-nou seixanta seixanta-un seixanta-una seixanta-dos seixanta-dues seixanta-tres seixanta-quatre seixanta-cinc seixanta-sis seixanta-set seixanta-vuit seixanta-nou setanta setanta-un setanta-una setanta-dos setanta-dues setanta-tres setanta-quatre setanta-cinc setanta-sis setanta-set setanta-vuit setanta-nou vuitanta vuitanta-un vuitanta-una vuitanta-dos vuitanta-dues vuitanta-tres vuitanta-quatre vuitanta-cinc vuitanta-sis vuitanta-set vuitanta-vuit vuitanta-nou noranta noranta-un noranta-una noranta-dos noranta-dues noranta-tres noranta-quatre noranta-cinc noranta-sis noranta-set noranta-vuit noranta-nou dos-cents dues-centes tres-cents tres-centes quatre-cents quatre-centes cinc-cents cinc-centes sis-cents sis-centes set-cents set-centes vuit-cents vuit-centes nou-cents nou-centes/;
}

sub antroponims
{
	# afegeix els antrop�nims i els llinatges
	return () unless $fitxerAntroponims;
	my @res;
	for my $fitxer($fitxerAntroponims, $fitxerLlinatges) {
		open ENT, "<$fitxer" or die "No es pot obrir $fitxer";
		my ($lin, $vocIni, $fem, $artOK, $flags);
		while ($lin = <ENT>) {
			chomp $lin;
			$lin =~ s/^\s+//;  $lin =~ s/\s+$//;
			next unless $lin;
			next if $lin =~ m!^//!;
			$fem = ($lin =~ s/\s+f$//);
			$vocIni = &vocalInicial($lin);
			$artOK = ($vocIni && ! ($fem  && &iuAtones($lin)));
			$flags = ($vocIni ? ('/Y' . ($artOK ? 'W' : '')) : (''));
			push @res, "$lin$flags";
		}
		close ENT;
	}
	return @res;
}

sub propis
{
	# afegeix els noms propis (marques, etc.)
	return () unless @fitxersPropis;
	my @res;
	for my $fitxer(@fitxersPropis) {
		open ENT, "<$fitxer" or die "No es pot obrir $fitxer";
		my ($lin, $vocIni, $flags);
		while ($lin = <ENT>) {
			chomp $lin;
			$lin =~ s/^\s+//;  $lin =~ s/\s+$//;
			next unless $lin;
			next if $lin =~ m!^//!;
			$vocIni = &vocalInicial($lin);
			$flags = ($vocIni ? '/Y' : '');
			push @res, "$lin$flags";
		}
		close ENT;
	}
	return @res;
}

sub gentilicis
{
	# afegeix els gentilicis
	my @res;
	my ($lin, $masc, $fem);
	for my $fitxerGentilicis(@fitxersGentilicis) {
		print "Afegeix els gentilicis de $fitxerGentilicis\n";
		open ENT, "<$fitxerGentilicis" or die "No es pot obrir $fitxerGentilicis";
		while ($lin = <ENT>) {
			chomp $lin;
			$lin =~ s/^\s+//;  $lin =~ s/\s+$//;
			next if $lin =~ m!^//!;
			next unless $lin;
			if ($lin =~ /(.+)\s+(.+)/) {
				my ($un, $dos) = ($1, $2);
				if ($dos =~ /s$/) {
					# si tenim singular i plural, els posam
					push @res, $un, $dos;
				}
				else {
					# si tenim mascul� i femen�, generam els plurals corresponents
					push @res, &procMFSP($1, 'm. i f.', 0, $1, $2);
				}
			}
			else {
				push @res, &procSP($1, 'adj.', {}, $lin);
			}
		}
		close ENT;
	}
	return @res;
}

sub toponims
{
	# afegeix els top�nims
	my @res;
	my ($lin, @trossos, $tros, $vocIni, $artOK, $flags);
	for my $fitxerToponims(@fitxersToponims) {
		print "Afegeix els top�nims de $fitxerToponims\n";
		open ENT, "<$fitxerToponims" or die "No es pot obrir $fitxerToponims";
		while ($lin = <ENT>) {
			chomp $lin;
			$lin =~ s/^\s+//;  $lin =~ s/\s+$//;
			next if $lin =~ m!^//!;
			next unless $lin;
			@trossos = split(/\s+/, $lin);
			for $tros(@trossos) {
				next unless $tros =~ /[A-Z����������]/;	# ignoram els trossos en min�scules
				$tros =~ s/^d'//;
				$artOK = ($tros =~ s/^[slSL]'//);
				if ($tros =~ /'/) { push(@res, $tros); next; }
				$tros =~ s/[,]$//;
				$vocIni = &vocalInicial($tros);
				$flags = ($vocIni ? ('/Y' . ($artOK ? 'V' : '')) : (''));
				push @res, "$tros$flags";
			}
		}
		close ENT;
	}
	return @res;
}

sub abreviatures
{
	# afegeix les abreviatures
	my @res;
	my ($lin, @trossos, $tros, $vocIni, $artOK, $flags);
	for my $fitxerAbreviatures(@fitxersAbreviatures) {
		print "Afegeix les abreviatures de $fitxerAbreviatures\n";
		open ENT, "<$fitxerAbreviatures" or die "No es pot obrir $fitxerAbreviatures";
		while ($lin = <ENT>) {
			chomp $lin;
			$lin =~ s/^\s+//;
			next if $lin =~ m!^//!;
			$lin =~ s/\s+$//;
			next unless $lin;
			@trossos = split(/\s+/, $lin);
			for $tros(@trossos) {
				push @res, "$tros";
			}
		}
		close ENT;
	}
	return @res;
}

sub creaTaulesIPR1
{
	# crea la taula d'IPR1 que no es poden deduir de l'infinitiu
	%precalcIPR1 = ( 
		# verbs en -enar i -inar
		'adotzenar' => 'adotz�n',
		'alenar' => 'al�n',
		'alienar' => 'ali�n',
		'alquenar' => 'alqu�n',
		'alumenar' => 'alum�n',
		'amargenar' => 'amarg�n',
		'anomenar' => 'anom�n',
		'arenar' => 'ar�n',
		'asserenar' => 'asser�n',
		'atermenar' => 'aterm�n',
		'cadenar' => 'cad�n',
		'carenar' => 'car�n',
		'carmenar' => 'carm�n',
		'cognomenar' => 'cognom�n',
		'concatenar' => 'concat�n',
		'contraordenar' => 'contraord�n',
		'desalienar' => 'desali�n',
		'desarenar' => 'desar�n',
		'desembenar' => 'desemb�n',
		'desencadenar' => 'desencad�n',
		'desencovenar' => 'desencov�n',
		'desenfrenar' => 'desenfr�n',
		'desentrenar' => 'desentr�n',
		'desfrenar' => 'desfr�n',
		'deshidrogenar' => 'deshidrog�n',
		'desordenar' => 'desord�n',
		'desoxigenar' => 'desoxig�n',
		'destermenar' => 'desterm�n',
		'destrenar' => 'destr�n',
		'eixamenar' => 'eixam�n',
		'embenar' => 'emb�n',
		'emmenar' => 'emm�n',
		'emplenar' => 'empl�n',
		'encadenar' => 'encad�n',
		'encovenar' => 'encov�n',
		'enfrenar' => 'enfr�n',
		'enllumenar' => 'enllum�n',
		'enravenar' => 'enrav�n',
		'entrenar' => 'entr�n',
		'esblenar' => 'esbl�n',
		'escarmenar' => 'escarm�n',
		'esgramenar' => 'esgram�n',
		'esllemenar' => 'esllem�n',
		'esmargenar' => 'esmarg�n',
		'esmenar' => 'esm�n',
		'esquenar' => 'esqu�n',
		'estrenar' => 'estr�n',
		'gangrenar' => 'gangr�n',
		'hidrogenar' => 'hidrog�n',
		'malmenar' => 'malm�n',
		'margenar' => 'marg�n',
		'nitrogenar' => 'nitrog�n',
		'nomenar' => 'nom�n',
		'ofrenar' => 'ofr�n',
		'ordenar' => 'ord�n',
		'oxigenar' => 'oxig�n',
		'reestrenar' => 'reestr�n',
		'refrenar' => 'refr�n',
		'remenar' => 'rem�n',
		'reordenar' => 'reord�n',
		'sobrenomenar' => 'sobrenom�n',
		'sofrenar' => 'sofr�n',
		'termenar' => 'term�n',
		'terraplenar' => 'terrapl�n',
		# verbs en -esar, -osar, -essar i -essar
		'aburgesar' => 'aburg�s',
		'adossar' => 'ad�s',
		'afrancesar' => 'afranc�s',
		'anquilosar' => 'anquil�s',
		'apressar' => 'apr�s',
		'arrebossar' => 'arreb�s',
		'arrosar' => 'arr�s',
		'aterrossar' => 'aterr�s',
		'atrossar' => 'atr�s',
		'avesar' => 'av�s',
		'confessar' => 'conf�s',
		'contrapesar' => 'contrap�s',
		'desarnesar' => 'desarn�s',
		'desarrebossar' => 'desarreb�s',
		'desatrossar' => 'desatr�s',
		'desavesar' => 'desav�s',
		'desbrossar' => 'desbr�s',
		'descossar' => 'desc�s',
		'desempavesar' => 'desempav�s',
		'desempesar' => 'desemp�s',
		'desenllosar' => 'desenll�s',
		'desentravessar' => 'desentrav�s',
		'desglossar' => 'desgl�s',
		'desinteressar' => 'desinter�s',
		'desossar' => 'des�s',
		'desrosar' => 'desr�s',
		'desterrossar' => 'desterr�s',
		'destesar' => 'dest�s',
		'desvesar' => 'desv�s',
		'disfressar' => 'disfr�s',
		'embrossar' => 'embr�s',
		'emmaressar' => 'emmar�s',
		'empavesar' => 'empav�s',
		'empesar' => 'emp�s',
		'encendrosar' => 'encendr�s',
		'encerrosar' => 'encerr�s',
		'endossar' => 'end�s',
		'enfilosar' => 'enfil�s',
		'enllosar' => 'enll�s',
		'enrosar' => 'enr�s',
		'enterrossar' => 'enterr�s',
		'entravessar' => 'entrav�s',
		'entropessar' => 'entrop�s',
		'enverinosar' => 'enverin�s',
		'esbessar' => 'esb�s',
		'esbossar' => 'esb�s',
		'esbrossar' => 'esbr�s',
		'escossar' => 'esc�s',
		'espicossar' => 'espic�s',
		'esterrossar' => 'esterr�s',
		'estesar' => 'est�s',
		'estossar' => 'est�s',
		'expressar' => 'expr�s',
		'ingressar' => 'ingr�s',
		'interessar' => 'inter�s',
		'malavesar' => 'malav�s',
		'metamorfosar' => 'metamorf�s',
		'necrosar' => 'necr�s',
		'palesar' => 'pal�s',
		'posttesar' => 'postt�s',
		'pretesar' => 'pret�s',
		'processar' => 'proc�s',
		'professar' => 'prof�s',
		'progressar' => 'progr�s',
		'redossar' => 'red�s',
		'reembossar' => 'reemb�s',
		'regressar' => 'regr�s',
		'reingressar' => 'reingr�s',
		'repesar' => 'rep�s',
		'retrossar' => 'retr�s',
		'revessar' => 'rev�s',
		'sospesar' => 'sosp�s',
		'tondosar' => 'tond�s',
	);
}
