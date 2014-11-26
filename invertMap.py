#!/cygdrive/c/Python27/python.exe
#
# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

# Inverts a translation table

import sys

def readTransTable(translatePath):
	transTable = []
	for line in translatePath:
		fields = line.strip().split('\t')
		transTable.append((fields[0], fields[1]))
	return transTable
	
#Main body	
def main():	
	#Read the translation tables
	table = readTransTable(sys.stdin)
	
	for x in table:
		sys.stdout.write(str(x[1]) + "\t" + str(x[0]) + "\n")
	
main()