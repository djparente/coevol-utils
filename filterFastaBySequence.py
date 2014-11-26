#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

import sys
import Bio.SeqIO

# Filters a FASTA-formatted alignment (to stdin), retaining sequences
# only if the (ungapped) sequence occurs in the alignment specified
# as argument 1

def chomp(line):
	return line.rstrip("\n").rstrip("\r")

def main():
	includeMSA = list(Bio.SeqIO.parse(sys.argv[1], "fasta"))
	includeDict = { str(x.seq).replace("-","") : 0 for x in includeMSA }
	
	msa = list(Bio.SeqIO.parse(sys.stdin, "fasta"))
	msa = [x for x in msa if str(x.seq).replace("-","") in includeDict]
	
	for x in msa:
		print ">" + x.name
		print x.seq
	

if __name__=='__main__':
	main()