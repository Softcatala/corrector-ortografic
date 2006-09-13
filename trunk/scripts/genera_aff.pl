# genera fitxer d'afixos a partir d'un fitxer de regles

package genera_aff;

use strict;

my $DIR = '.';
my $ENT = 'entrades/regles.txt';
#~ print "DIR: $DIR\n";

# defineix els noms
my $nomDesc = 'DESC';
my $nomSet = 'SET';
my $nomGrups = 'GRUPS';
my $nomTry = 'TRY';
my $nomFirst = 'FIRST';
my $nomRegla = 'REGLA';

# aixeca la taula de procediments associats als noms
my %procs = (
	$nomDesc => \&procDesc,
	$nomSet => \&procSet,
	$nomGrups => \&procGrups,
	"/$nomGrups" => \&proc_Grups,
	$nomTry => \&procTry,
	$nomFirst => \&procFirst,
	$nomRegla => \&procRegla,
	"/$nomRegla" => \&proc_Regla,
);

my $aff;					# el nom del fitxer de sortida
my @desc = ();				# la descripció (poden ser diverses línies)
my $set = 'ISO8859-1';		# el joc de caràcters per defecte
my $try;					# els caràcters que es provaran
my @first;					# les substitucions preferides
my %inclosos;				# els grups inclosos
my %grups;					# la descripció dels grups
my @regles;					# un array [flag, tipus, desc, [línies], combina]
my $linia;

# Aixeca la llista de regles
&procFitxer;
die "Cal definir $nomTry" unless $try;

for my $RES(
			'resultats/catalan  (*)', 
			#~ 'resultats/cntr     (000                                                            )', 
			#~ 'resultats/bal      (000 001 002         005                         013 101 102 103)', 
			#~ 'resultats/val      (000 002     003 004     006 007 008 009 010 011                )',
			)
{
	for my $INC999('n', 's') {
		&calcAffInc($RES);
		if ($INC999 eq 's') {
			$aff =~ s/\.aff/_999.aff/;
			$inclosos{'999'} = 1;
		}
		print "S'ha generat el fitxer $aff\n";
		open SORT, ">$DIR/$aff" or die "No es pot obrir $DIR/$aff";
		map { print SORT "#$_\n" } @desc;
		print SORT "\n# Generat a partir de $DIR/$ENT\n\n";
		print SORT "# Regles\n";
		print SORT sort map { "#     $_->[0]  $_->[2]\n" } @regles;
		print SORT "\n";
		print SORT "# Inclou els següents grups\n";
		map { print SORT "#     $_: ",  $grups{$_}, "\n"; } sort keys %inclosos;
		print SORT '# grups: ', join(" ", map { exists $inclosos{$_} ? $_ : "!$_" } sort keys %grups), "\n";
		print SORT "\n";

		print SORT "SET $set\n\n";
		print SORT "TRY $try\n\n";
		#map { print SORT "FIRST $_\n" } @first;
		print SORT "REP ", scalar @first, "\n";
		map { print SORT "REP $_\n"; } @first;
		print SORT "\n";
		my $grup;
		my @llistaRegles;
		for $grup(@regles)
		{
			# print SORT "# ", join(", ", @$grup), "/n";
			push @llistaRegles, "# $$grup[0] $$grup[2]";
			print SORT "# $$grup[2]\n" if $$grup[2];
			my @linies;	# les línies que entraran a la regla
			map {
				#print "Mira $$_[0] ($$_[1])\n";
				push(@linies, "$$grup[1] $$grup[0]   $$_[1]") if exists $inclosos{$$_[0]}; 
			} @{$$grup[3]};
			if (scalar @linies) {
				print SORT sprintf("%s %s $$grup[4] %d\n", $$grup[1], $$grup[0], scalar @linies);
				map { print SORT "$_\n"; } @linies;
				print SORT "\n";
			}
		}

		close SORT;
	}
}

#print join("\n", "Llista de regles", sort @llistaRegles), "\n\n";

exit;

#################################################################

sub procFitxer
{
	# processa el fitxer
	@desc = ();
	@first = ();
	@regles = ();
	die "No es troba $DIR/$ENT" unless -f "$DIR/$ENT";
	print "Processa el fitxer de regles $DIR/$ENT\n";
	open ENT, "<$DIR/$ENT" || die "No es pot obrir $ENT";
	while (my $linia = <ENT>)
	{
		next if $linia =~ m!^\s*(//|$)!;
		$linia =~ s/[\012\015]+$//;
		next unless ($linia =~ /^(\w+)\s*$/) || ($linia =~ /^(\w+)(\s+)(.*)/);
		my ($id, $arg) = ($1, $3);
		$arg = "$2$arg" if ($id eq 'DESC');
		my $proc = $procs{$id};
		die "Comanda desconeguda: \"$id\" -> \"$arg\"" unless $proc;
		$proc->($arg) if $proc;
		# print "$1 -> $2/n";
	}
	close ENT;
}

sub calcAffInc
{
	# calcula el nom del fitxer .aff i la llista de regles incloses
	my ($tot) = @_;
	%inclosos = ();
	$tot =~ /(.*?)\s+\((.*)\)/;
	$aff = "$1.aff";
	my $inc = $2;
	$inc =~ s/^\s+//;
	$inc =~ s/\s+$//;
	for my $un(split(/\s+/,$inc)) {
		if ($un eq '*') {
			map {
				$inclosos{$_} = 1 unless /999/;
			} keys %grups;
			next;
		}
		if ($un =~ /^!(...)$/) {
			delete $inclosos{$1};
			next;
		}
		if ($un =~ /^...$/) {
			$inclosos{$un} = 1;
			next;
		}
		die "Nom de grup inesperat: $un";
	}
}

sub procDesc
{
	#~ my ($arg) = @_;
	#~ push @desc, $arg;
	my $final = "^/$nomDesc";
	while ($linia = <ENT>)
	{
		return if $linia =~ $final;
		$linia =~ s/[\012\015]+$//;
		push @desc, " $linia";
	}
}

sub procSet
{
	my ($arg) = @_;
	$set = $arg;
}

sub procTry
{
	my ($arg) = @_;
	$try = $arg;
}

sub procFirst
{
	my ($arg) = @_;
	# push @first, $arg;
	$arg =~ s/^\s+//;
	$arg =~ s/\s+$//;
	my @reps = split(/\s+/, $arg);
	for my $rep(@reps) {
		my @trossos = split("/", $rep);
		my $ini = shift @trossos;
		map {
			push @first, "$ini $_";
		} @trossos;
	}
}

sub procGrups
{
	# no fa res dels grups
	my $final = "^/$nomGrups";
	while ($linia = <ENT>)
	{
		return if $linia =~ $final;
		$linia =~ s/[\012\015]+$//;
		$linia =~ /^(...)\s+(.*)/;
		$grups{$1} = $2;
	}
}

sub proc_Grups
{
	die "Ordre inesperada: $linia";
}

sub procRegla
{
	my ($arg) = @_;
	# exemple: REGLA A SFX 1a Conjugació regla general
	$arg =~ /(.)\s+(...\+?)\s*(.*)/;
	my ($flag, $tipus, $desc) = ($1,$2,$3);
	my $combina = ($tipus =~ s/\+$//) ? 'Y' : 'N';
	my $linies = [];
	my $final = "^/$nomRegla";
	while ($linia = <ENT>)
	{
		last if $linia =~ $final;
		$linia =~ s/[\012\015]+$//;
		next unless $linia =~ /^(...)\s+(.*)/;
		push @$linies, [$1, $2];
	}
	push @regles, [$flag, $tipus, $desc, $linies, $combina];
}

sub proc_Regla
{
	die "Ordre inesperada: $linia";
}
