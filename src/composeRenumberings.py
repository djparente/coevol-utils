#!/cygdrive/c/Python27/python.exe
#
# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 2012-12-12

# Composes multiple translation tables

import sys

def readTransTable(translatePath):
	transTable = {}
	for line in open(translatePath):	
		fields = line.strip().split('\t')
		transTable[fields[0]] = fields[1]
	return transTable
	
#Main body	
def main():
	if len(sys.argv) < 3: 
		sys.exit("Usage: [Translation table 1] [Translation table 2] ... [Translation table N]")
	
	
	
	#Read the translation tables
	tables = [ readTransTable(path) for path in sys.argv[1:] ]
	
	#Propagate the initial numberings through the translation table
	initialNumberings = [ k for k in tables[0].keys() ]
	finalNumberings = initialNumberings
	for table in tables:
		finalNumberings = [ table[entry] for entry in finalNumberings if entry in table ]
		
	result = zip(initialNumberings, finalNumberings)
	
	for entry in result:
		print "\t".join(entry)
	
main()