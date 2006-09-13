# Genera un fitxer .aff d'ispell a partir d'un de myspell
# Tamb� genera un fitxer .dic
# Fet per Joan Moratinos, 2002
# Suposa que tots els flags s�n lletres maj�scules (?)

package gen_ispell;

BEGIN { unshift @INC, './eines' }

use strict;

my %opcions = &llegeixOpcions('nom', 'mes');
&mostraUs unless ($opcions{'nom'} && ! $opcions{'mes'});

use sillabes;	# el paquet s�l�labes inclou la funci� majuscules()

my $ENT = $opcions{'nom'};

print <<FI;
Afixos: $ENT.aff -> ${ENT}_i.aff
Diccionari: $ENT.dic -> ${ENT}_i.dic

FI

my (@pre, @suf);	# arrays de prefixos i sufixos
					# s�n refer�ncies a hash, amb els seg�ents valors
					# 	flag: una lletra
					# 	comb: si �s combinable o no
					# 	desc: la descripci� del flag
					# 	ent: les entrades (una refer�ncia a un array)

my @desc;			# La descripci� del programa, llegit de *.aff

# aixeca la llista de prefixos i sufixos
&creaAfixos;
#~ print "Descripci�:\n" . join("\n", @desc);

open SORT, ">${ENT}_i.aff" or die "No es pot obrir ${ENT}_i.aff";

# escriu l'encap�alament del fitxer
&escriuCap;

# escriu els prefixos
&escriuPre;

# escriu els sufixos
&escriuSuf;

close SORT;

# genera el fitxer .dic
my $ignorats = 0;
open ENT, "<$ENT.dic" or die "No es pot obrir $ENT.dic";
open SORT, ">${ENT}_i.dic" or die "No es pot crear ${ENT}_i.dic";
<ENT>;     # salta la l�nia amb el nombre de l�nies
binmode SORT;
while (<ENT>) {
	chomp;
	if (/[0-9.]/) {
	   $ignorats++;
	   next;
	}
	print SORT "$_\n";
}
close SORT;
print "S'han ignorat $ignorats mots que contenien nombres o punts\n";

#open ENT, "<${SORT}_i.aff"; print while <ENT>; close ENT;

# ===================================================================

sub mostraUs
{
	# Mostra l'�s del programa
	print <<FINAL;
Genera fitxers d'ispell a partir de fitxers de myspell

Opcions: [-nom=nom_dels_fitxers]
Obre els fitxers xxx.dic i xxx.aff i genera xxx_i.dic i xxx_i.aff
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
sub creaAfixos
{
	open ENT, "<$ENT.aff" or die "No es pot obrir $ENT.aff";
	my ($lin, $tipus, $flag, $comb, $nlin, $coment, $dinsDesc);
	$dinsDesc = 1;
	while ($lin = <ENT>) {
		chomp $lin;
		if ($lin =~ /^#\s*(.*)/) {
			$coment = $1;
			push @desc, "# $coment" if $dinsDesc;
			next;
		}
		$dinsDesc = 0;
		next unless $lin =~ /^([SP]FX)\s+(\S)\s+([YN])\s+(\d+)/;
		($tipus, $flag, $comb, $nlin) = ($1, $2, $3, $4);
		my @ents;
		for (my $i=0; $i<$nlin; $i++) {
			$lin = <ENT>;
			chomp $lin;
			$lin =~ /^[PS]FX\s+\S\s+(.*)/;
			push @ents, $1;
		}
		if ($tipus eq 'PFX') {
			push @pre, { 'flag'=>$flag, 'comb'=>$comb, 'ent'=>\@ents, 'desc'=>$coment };
		}
		else {
			push @suf, { 'flag'=>$flag, 'comb'=>$comb, 'ent'=>\@ents, 'desc'=>$coment };
		}
	}
}

sub escriuPre
{
	print SORT "\nprefixes\n\n";
	for my $x(@pre) {
		print SORT 'flag ';
		print SORT ($x->{'comb'} eq 'Y' ? '*' : ' ');
		print SORT "$x->{flag}:    # $x->{desc}\n";
		for my $ent(@{$x->{'ent'}}) {
			# 0      l'          .                # l'home
			$ent =~ /(\S+)\s+(\S+)\s+(\S+)\s*#\s*(.*)/;
			my ($menys, $mes, $cond, $com) = ($1, $2, $3, $4);
			$cond = &midaFixa(&prepCond($cond), 20);
			$mes =~ s/-/\\-/g;
			$mes = &midaFixa(sillabes::majuscules($mes), 20);
			print SORT ' ' x 4, "$cond > $mes # $com\n";
		}
		print SORT "\n";
	}
}

sub escriuSuf
{
	print SORT "\nsuffixes\n\n";
	for my $x(@suf) {
		print SORT 'flag ';
		print SORT ($x->{'comb'} eq 'Y' ? '*' : ' ');
		print SORT "$x->{flag}:    # $x->{desc}\n";
		for my $ent(@{$x->{'ent'}}) {
			$ent =~ /(\S+)\s+(\S+)\s+(\S+)\s*#\s*(.*)/;
			my ($menys, $mes, $cond, $com) = ($1, $2, $3, $4);
			$cond = &midaFixa(&prepCond($cond), 20);
			$menys =~ s/-/\\-/g;
			$menys = ($menys eq '0' ? '' : ('-' . sillabes::majuscules($menys) . ','));
			$mes =~ s/-/\\-/g;
			$mes = &midaFixa(sillabes::majuscules("$menys$mes"), 20);
			print SORT ' ' x 4, "$cond > $mes # $com\n";
		}
		print SORT "\n";
	}
	print SORT "\n";
}

sub prepCond
{
	my ($cond) = @_;
	$cond = sillabes::majuscules($cond);
	my $dinsGrup = 0;
	my $res = '';
	for (my $i=0; $i<length($cond); $i++) {
		my $x = substr($cond, $i, 1);
		$res .= ' ' unless ($i == 0 or $dinsGrup);
		$res .= $x;
		$dinsGrup = 1 if $x eq '[';
		$dinsGrup = 0 if $x eq ']';
	}
	return $res;
}

sub midaFixa
{
	my ($cad, $mida) = @_;
	$cad .= ' ' x ($mida - length($cad)) if length($cad) < $mida;
	return $cad;
}
sub escriuCap
{
	my ($preMaj, $sufMaj, $lliMaj, $preMin, $sufMin, $lliMin);
	my (%usats, $i);
	for $i(@pre) { $usats{$i->{'flag'}} = 'P' }
	for $i(@suf) { $usats{$i->{'flag'}} = 'S' }
	for (my $x = ord('a'); $x <= ord('z'); $x++) {
		my ($min,$maj);
		$min = chr($x);
		$maj = uc($min);
		$preMaj .= (($usats{$maj} eq 'P') ? '* ' : '  ');
		$sufMaj .= (($usats{$maj} eq 'S') ? '* ' : '  ');
		$lliMaj .= (exists $usats{$maj} ? '  ' : "$maj ");
		$preMin .= (($usats{$min} eq 'P') ? '* ' : '  ');
		$sufMin .= (($usats{$min} eq 'S') ? '* ' : '  ');
		$lliMin .= (exists $usats{$min} ? '  ' : "$min ");
	}

	print SORT join("\n", @desc);

	print SORT <<FI_CAP;


# Sense afixaci� autom�tica

allaffixes off

# Definici� dels car�cters

defstringtype "list" "nroff" ".list"

wordchars	a	A
stringchar	�	�
stringchar	�	�			# Gonz�lez
stringchar	�	�			# S�o Paulo
wordchars	[b-c]	[B-C]
stringchar	�	�
wordchars	[d-e]	[D-E]
stringchar	�	�
stringchar	�	�
wordchars	[f-i]	[F-I]
stringchar	�	�
stringchar	�	�
wordchars	[j-o]	[J-O]
stringchar	�	�			# Mu�oz
stringchar	�	�
stringchar	�	�
stringchar	�	�			# r�ngten
wordchars	[p-u]	[P-U]
stringchar	�	�
stringchar	�	�
wordchars	[v-z]	[V-Z]
wordchars   [�]
wordchars   [-]
wordchars   [']

#
# TeX
#
altstringtype "tex" "TeX" ".tex" ".bib"

altstringchar	\\`a		�
altstringchar	\\'a		�
altstringchar	\\`A		�
altstringchar	\\'A		�
altstringchar	"\\c c"		�
altstringchar	"\\c C"		�
altstringchar	\\`e		�
altstringchar	\\`E		�
altstringchar	\\'e		�
altstringchar	\\'E		�
altstringchar	\\'\\i		�
altstringchar	\\'\\I		�
altstringchar	\\\"\\i		�
altstringchar	\\\"\\I		�
altstringchar	\\'o		�
altstringchar	\\'O		�
altstringchar	\\`o		�
altstringchar	\\`O		�
altstringchar	\\\"o		�
altstringchar	\\\"O		�
altstringchar	\\'u		�
altstringchar	\\'U		�
altstringchar	\\\"u		�
altstringchar	\\\"U		�


# Relaci� de flags emprats

# Flag      A B C D E F G H I J K L M N O P Q R S T U V W X Y Z
# Prefix    $preMaj
# Sufix     $sufMaj
# Lliure    $lliMaj

# Flag      a b c d e f g h i j k l m n o p q r s t u v w x y z
# Prefix    $preMin
# Sufix     $sufMin
# Lliure    $lliMin


# ------------------------------------------------------------

FI_CAP
}

