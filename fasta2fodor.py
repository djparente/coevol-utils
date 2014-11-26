#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

import sys
import re
import Bio.SeqIO

# Strips whitespace from name (replace with underscore)
def safeName(name):
	return str(re.sub(r"\s+", "_", name))

def main():
	msa = list(Bio.SeqIO.parse(sys.stdin, "fasta"))
	
	for x in msa:
		sys.stdout.write(safeName(x.name) + " " + str(x.seq) + "\n")
	

if __name__ == '__main__':
	main()