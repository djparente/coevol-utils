#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 2012-11-27

# Creates a column-to-residue map file

import Bio.SeqIO
import math
import sys

def colGapPct(column):
	return float(column.count('-')) / len(column)
	
def entropy(column):
	colLen = len(column)
	
	allGlyphs=set(column)
	countDict = { glyph : 0 for glyph in allGlyphs }
	
	for glyph in column:
		countDict[glyph] += 1
	
	glyphFreq = [ float(countDict[glyph]) / colLen for glyph in allGlyphs ]
	
	return -sum([p * math.log(p) for p in glyphFreq])
	
def canonName(row):
	num = 0
	iCode = 'A'
	
	result=[]
	for glyph in row:
		if glyph == '-':
			result.append(str(num) + iCode)
			iCode=chr(ord(iCode)+1)
		else:
			num += 1
			result.append(str(num))
			iCode = 'A'
	return result

def main():
	path = sys.argv[1]
	canonID = sys.argv[2]
	
	minEntropy = float(sys.argv[3])
	minGaps = float(sys.argv[4])

	msa = list(Bio.SeqIO.parse(path, "fasta"))
	
	rows = len(msa)
	cols = len(msa[0].seq)
	
	columns = [ [ row.seq[c] for row in msa] for c in range(cols) ]
	
	#Get the canonical row
	cRow = [ row for row in msa if row.id == canonID ]
	if len(cRow) < 1:
		sys.exit("Canonical row not found")
		
	if len(cRow) > 1:
		sys.exit("Duplicate canonical rows found")
		
	cRow = cRow[0]
	
	cName = canonName(cRow.seq)
	
	gapPct = [ colGapPct(column) for column in columns ]
	colEnt = [ entropy(column) for column in columns ]
	
	meta = zip(range(cols), cName, cRow, gapPct, colEnt)
	
	meta = [ m for m in meta if float(m[3]) < minGaps and float(m[4]) >= minEntropy ]
	
	#Print out column -> canonical name
	print "\n".join(str(m[0])+"\t"+str(m[1]) for m in meta)
	
	#Print out canonical name -> entropy
	#print "\n".join(str(m[1])+"\t"+str(m[4]) for m in meta)
	
		


main()