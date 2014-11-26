#!/cygdrive/c/Python27/python.exe
# 
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-01-08
#
# Extracts all edge data associated with a particular node
#
# Status: Complete and spot-tested
# Version: 1.1
# Language: Python
# Changelog:
#    + 2013-01-08: Created
#    + 2013-01-08: Completed development.  Manually spot-checked node result to validate.
#    + 2013-01-08: Modified to perform a more detailed ordering.  Looking at maximum node
#                   first, but in cases of ties (which often occur, since edges belong to
#                   two different nodes), look to the next-highest score, etc...

import sys;
import networkx as nx;

def usage():
	sys.stderr.write("Usage: cat myWel.wel | extractNodeData [Node Identifier]\n")
	
def errorOut(message, code):
	sys.stderr.write("Error: %s" % message)
	exit(code)

def main():
	if len(sys.argv) < 2:
		usage()
		errorOut("Not enough arguments", 255)

	# Read the weighted edgelist
	G = nx.read_weighted_edgelist(sys.stdin)
	
	nid = sys.argv[1]

	# Get edge weights
	weights = [ (nid, n2, G[nid][n2]['weight']) for n2 in G.neighbors(nid)  ]
	
	# Sort nodes in decending order
	weights.sort(key=lambda x : x[2], reverse=True)
	
	# Write nodes and maxWeights out with UNIX-style line endings
	for t in weights:
		sys.stdout.write("%s\t%s\t%f\n" % t )

main()

