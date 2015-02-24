#!/cygdrive/c/Python27/python.exe
# 
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-01-10
#
# Motivation: Resmap cannot do ligand-protein contacts.  Weismann lab LPC server
# cannot use Resmap's selective contact (e.g. an HB is 3.5 A, while an ion pair, 
# is 5 A).
#
# This program accepts a Resmap-style configuration file (v 1.02), a PDB
# and another file containing ligand atoms with ATOM or HETATM records.
# N.B.: You will still need to use the Weismann server to obtain the 
# atom type codes (hydrophilic, acceptor, etc) for your heteroatom and add
# them to the Resmap configuraiton file.
#
# For example, 1efa contains heteroatoms for ligand NPF.  Performing Weismann
# LPC yields that atom C1 of NPF is of class V (5 - roman numerals), therefore,
# the resmap configuration file must include the following additional line:
#     DEF  C1  NPF  5
# And so on, for all the distinct atoms (C1 code) in NPF.
#
# Status: Complete, lightly tested
# Version: 1.0
# Language: Python
# Changelog:
#    + 2013-01-10: Created
#	 + 2013-01-10: Marked complete, light testing for reasonability
#                  in the course of use.

import sys
import math

#Returns (0) record name, (1) serial, (2) atom role, (3) residue name, (4) chain, (5) seqno, (6) x, (7) y, (8) z
def parseAtom(line):
	return (line[0:6], int(line[6:11]), line[12:16].replace(" ", ""), line[17:20], line[21:22], line[22:27].strip(' '), float(line[30:38]), float(line[38:46]), float(line[46:54]))

def getContactThresh(x,y,rDst, rDef, rCnt):
	typeX = rDef[(x[2],x[3])]
	typeY = rDef[(y[2],y[3])]
	
	bondType = rCnt[typeX][typeY-1]
	return rDst[bondType]
	
#Distance between two atom records
def distance(x, y):
	dist = 0
	delta = (x[6] - y[6])
	dist += delta*delta
	delta = (x[7] - y[7])
	dist += delta*delta
	delta = (x[8] - y[8])
	dist += delta*delta
	
	return math.sqrt(dist)
	
def interact(x, y, rDst, rDef, rCnt):
	return distance(x,y) < getContactThresh(x,y, rDst, rDef, rCnt)

def main():
	resmapConfigPath = sys.argv[1]
	pdbPath = sys.argv[2]
	ligPath = sys.argv[3]

	#Read the resmap configuration file
	confLines = [ line.strip() for line in open(resmapConfigPath) ]
	
	#Parse the lines of the resmap configuration file
	rDst = { line[5:7] : float(line[14:17]) for line in confLines if line[0:3] == "DST" }
	rDef = { (line[5:8].replace(" ", ""), line[9:12]) : int(line[13:15]) for line in confLines if line[0:3] == "DEF" }
	rCnt = { int(line[4:7]) : line[9:].split(' ') for line in confLines if line[0:3] == "CNT"}
	
	# Get ATOM records for protein
	prot = [ parseAtom(line) for line in open(pdbPath) if line[0:6] == "ATOM  " ]
	
	# GET ATOM and HETATM records for ligand
	liga = [ parseAtom(line) for line in open(ligPath) if line[0:6] == "ATOM  " or line[0:6] == "HETATM" ]
	
	for pAtom in prot:
		for lAtom in liga:
			if interact(pAtom, lAtom, rDst, rDef, rCnt):
				sys.stdout.write("%s\t%s\n" % (pAtom[4], pAtom[5]))

main()