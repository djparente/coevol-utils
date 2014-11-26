#!/cygdrive/c/Python27/python.exe
#
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-01-08
#
# Accepts a file with name of the form:
#    SubFam1_Alg1_SubFam2_Alg2.nja 
# and creates a plot showing the Jaccard indices as a function of increasing
# threshold values
#
# This variant is for jaccard NODAL analysis (not edge-wise analysis)
#
# Status: Complete
# Version: 1.1
# Language: Python
# Changelog:
#    + 2013-01-14: Forked from plotJaccardNodesError.py to allow a better
#                   shading of the error region
#    + 2013-01-14: Modified to show the upper limit of the Jaccard index as a
#                   function of N; mirroring a change in the NJA file format
#    + 2013-01-15: Added correct capitalization subroutine for algorithm and 
#                   subfamily names
#    + 2013-01-17: Added GalR and GalS to the capitalization list

import sys
import itertools
import math
import matplotlib.pyplot as plt
import os.path

def usage():
	sys.stderr.write("Usage: plotJaccardNode [Jaccard Analysis NJA file] [Output file name]")
	
def errorOut(message, code):
	sys.stderr.write("Error: %s" % message)
	exit(code)
	
# Declare correct capitalization of names
nameDict = { "elsc" : "ELSC", "omes" : "OMES", "mcbasc" : "McBASC", "sca" : "SCA", "znmi" : "ZNMI", "ccpa": "CcpA", "galrs": "GalRS", "gntr" : "GntR", "purr": "PurR", "rbsra": "RbsR-A", "trer" : "TreR", "galr" : "GalR", "gals" : "GalS" }
def capname(name):
	if name in nameDict:
		return nameDict[name]
	
	return name

def main():
	if len(sys.argv) != 3:
		usage()
		errorOut("Expected 2 arguments")

	# Read the file path
	path = sys.argv[1]
	outpath = sys.argv[2]

	# Parse the file name
	nameFields = map(capname,os.path.basename(path).replace(".nja","").split("_"))
	
	# Parse the data in the file
	lines = [ line.strip().split(" ") for line in open(path) ]
	
	# Extract threshold
	thresh = [ float(line[0]) for line in lines ]
	Jexpr = [ float(line[1]) for line in lines ]
	Jtheo = [ float(line[2]) for line in lines ]
	Jmax  = [ float(line[5]) for line in lines ]
	
	# Error assumes that the third field is variance (not stdev, as I originally
	#  had intended)
	JtheoErr = [ 1.96*math.sqrt(float(line[3])) for line in lines ]
	JtheoErrMin = [ v - e for v,e in itertools.izip(Jtheo, JtheoErr) ]
	JtheoErrMax = [ v + e for v,e in itertools.izip(Jtheo, JtheoErr) ]
	
	# Extend the error region back to zero-items included to eliminate ugly 
	# closure of the error region at items = 0 on the plot
	threshZero = list(itertools.chain([0], thresh))
	JtheoErrMinZero = list(itertools.chain([JtheoErrMin[0]], JtheoErrMin))
	JtheoErrMaxZero = list(itertools.chain([JtheoErrMax[0]], JtheoErrMax))
	
	plt.figure()
	
	plt.ylim((-0.2,1.1))
	
	plt.plot(thresh, Jmax, 'k--')
	plt.fill_between(threshZero, JtheoErrMinZero, JtheoErrMaxZero, facecolor="#ffb5b5", edgecolor="#ff4c4c", antialiased=True)
	plt.plot(thresh, Jtheo, 'k-')
	plt.plot(thresh, Jexpr, 'b-')
	
	plt.xlabel("Number of nodes included")
	plt.ylabel("Jaccard index")
	plt.title("Nodal Jaccard Analysis: %s %s vs %s %s" % tuple(nameFields))
	plt.savefig(outpath, dpi=600)
	
main()