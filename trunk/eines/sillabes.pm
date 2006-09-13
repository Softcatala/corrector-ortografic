# Eines relacionades amb la partici� sil�l�bica

package sillabes;

# Nota: Nom�s funcionen amb lletres min�scules

sub sillab;
# Divideix un mot en s�l�labes.
# Torna un array amb el nombre de s�l�labes [0], les s�l�labes
# i una representaci� de les s�l�labes.
# Exemple: sillab("guaitar") = (2 'guai' 'tar' 'PLAP' 'PAP')

sub divVocTon;
# Divideix un mot en dues parts: de l'inici a la vocal t�nica, (aquesta inclosa)
# i la resta.
# Exemple: divVocTon("partidari") = ('partid' 'ari')

sub minuscules;
# passa una paraula a min�scules

sub majuscules;
# passa una paraula a maj�scules

################################################################################

use strict;

#map 
#{
#	print "$_: ", join(", ", divVocTon($_)), "\n";
#} ('abusiu');	

sub divVocTon
{
	my ($mot) = @_;
	# si hi ha un accent gr�fic, ja ho tenim
	return ($1,$2) if $mot =~ /(.*)([�������].*)/;
	# el mot ja nom�s pot ser agut o pla
	my @info = sillab($mot);
	my @protos = reverse splice(@info, 1 + $info[0]);
	return $mot if scalar @protos < 1;
	my @sil = reverse splice(@info, 1);
	my $div;
	my $nSil = scalar @protos;
	$protos[0] =~ /(A.*)/;
	if ($nSil == 1)
	{
		# monos�l�labs: la t�nica �s l'�nica
		$div = '(.*)(' . '.' x length($1) . ')$';
	}
	else
	{
		# miram el final del mot
		my $protoFinal = $1;
		my $final = substr($mot, - length $protoFinal);
		if (($protoFinal =~ /A.$/ and $final =~ /([aeiou]s|[ei]n)$/)
		    or
		    ($protoFinal =~ /A$/  and $final =~ /([aeiou])$/))
		{
			# el mot �s pla
			$protos[1] =~ /(A.*)/;
			$div = '(.*)(' . '.' x length($protos[0] . $1) . ')$';
		}
		else
		{
			# el mot �s agut
			$div = '(.*)(' . '.' x length($final) . ')$';
		}
	}
	$mot =~ $div;
	# return $1, $2, $div, reverse(@protos), reverse(@sil);
	return $1, $2;
}

sub sillab 
{
	my ($mot) = @_;
	$mot = minuscules($mot);
	my $esq = $mot;
	# print "Entrada: $esq\n";
	$esq =~ tr/aeo���������/A/ ; 	# && print "Vocals segures: $esq\n";
	$esq =~ s/A[iu]A/APA/g ;		# && print "Semivocals I: $esq\n";
	$esq =~ s/[qg][u�][Aiu]/PLA/g ; 	# && print "Semivocals II: $esq\n";
	$esq =~ s/^u/A/g ; 				# && print "Vocal u I: $esq\n";
	$esq =~ s/([^A])u/"$1A"/eg;		# && print "Vocal u II: $esq\n";
	$esq =~ s/^iA/PA/g ; 			# && print "Semivocals III: $esq\n";
	$esq =~ s/A[iu]/AP/g ; 			# && print "Semivocals IV: $esq\n";
	$esq =~ s/i/A/g ; 				# && print "Vocal i: $esq\n";
	$esq =~ s/[bcfgklp]l/PL/g ; 	# && print "oclusiva+l i ll: $esq\n";
	$esq =~ s/[bcdfgkptv]r/PL/g ;	# && print "oclusiva+r: $esq\n";
	$esq =~ s/ny/PL/g ; 			# && print "D�graf ny: $esq\n";
	$esq =~ tr/PAL/P/c ; 			# && print "Altres consonants: $esq\n";
	my @silesq = ();
	while ($esq ne "")
	{
		if ($esq =~ s/(P?L?AP*)$//) { unshift(@silesq, $1); }
		else { $silesq[0] = $esq . $silesq[0]; $esq = ''; }
	}
	my (@sil, $offset);
	foreach my $sil(@silesq) 
	{
		push(@sil, substr($mot, $offset, length($sil)));
		$offset += length($sil);
	}
	for(my $i=0; $i < @sil; $i++) { $sil[$i] =~ s/�$//; }
	return (scalar(@sil),@sil,@silesq);
}

sub minuscules
{
	my ($mot) = @_;
	$mot = lc $mot;
	$mot =~ tr[�����������]
	          [�����������];
	return $mot;
}

sub majuscules
{
	my ($mot) = @_;
	$mot = uc $mot;
	$mot =~ tr[�����������]
	          [�����������];
	return $mot;
}

BEGIN { };

1;
