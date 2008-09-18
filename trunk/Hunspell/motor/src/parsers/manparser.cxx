#include <cstdlib>
#include <cstring>
#include <cstdio>
#include <ctype.h>

#include "../hunspell/csutil.hxx"
#include "manparser.hxx"


#ifndef W32
using namespace std;
#endif

ManParser::ManParser() {
}

ManParser::ManParser(const char * wordchars)
{
	init(wordchars);
	// wordcharacters[('/' + 256) % 256] = 1; // for paths
}

ManParser::ManParser(struct unicode_info2 * uconv, unsigned short * wordchars, int len)
{
	init(uconv, wordchars, len);
}

ManParser::~ManParser() 
{
}

char * ManParser::next_token()
{
	for (;;) {
		switch (state)
		{
		case 1: // command arguments
			if (line[actual][head] == ' ') state = 2;
			break;
		case 0: // dot in begin of line
			if (line[actual][0] == '.') {
				state =  1;
				break;
			} else { 
				state = 2;
			}
			// no break
		case 2: // non word chars
			if (is_wordchar(line[actual] + head)) {
				state = 3;
				token = head;
			} else if ((line[actual][head] == '\\') && 
				   (line[actual][head + 1] == 'f') &&
				   (line[actual][head + 2] != '\0')) {
				head += 2;
			}
			break;
		case 3: // wordchar
			if (! is_wordchar(line[actual] + head)) {
				state = 2;
				char * t = (char *) malloc(head - token + 1);
				t[head - token] = '\0';
				if (t) return strncpy(t, line[actual] + token, head - token); 
				fprintf(stderr,"Error - Insufficient Memory\n");
			}
			break;
		}
                if (next_char(line[actual], &head)) {
			state = 0;
			return NULL;
		}
	}
}

