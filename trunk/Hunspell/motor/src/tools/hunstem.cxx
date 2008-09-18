
#include <cstring>
#include <cstdlib>
#include <cstdio>

#include "hunspell.hxx"


#ifndef W32
using namespace std;
#endif



int main(int argc, char **argv)
{

    FILE *wtclst;
    int i;
    int dp;
    char buf[101];
    Hunspell *pMS;

    /* first parse the command line options */

    for (i = 1; i < 3; i++)
	if (!argv[i]) {
	    fprintf(stderr, "correct syntax is:\nexample affix_file");
	    fprintf(stderr, " dictionary_file file_of_words_to_check\n");
	    exit(1);
	}

    /* open the words to check list */

    wtclst = fopen(argv[3], "r");
    if (!wtclst) {
	fprintf(stderr, "Error - could not open file to check\n");
	exit(1);
    }

    pMS = new Hunspell(argv[1], argv[2]);
    while (fgets(buf, 100, wtclst)) {
        *(buf + strlen(buf) - 1) = '\0';
        if (*buf == '\0') continue;
        dp = pMS->spell(buf);
        fprintf(stdout, "> %s\n", buf);
	if (dp) {
            char ** s;
            int n = pMS->stem(&s, buf);
            if (n) {
                // stemming is incomplete: search only one stem
                fprintf(stdout, "%s\n", s[0]);
                free(s[0]);
                free(s);
            }
        } else {
            fprintf(stdout, "Unknown word.\n");
            			
        }
    }
    delete pMS;
    return 0;
}
