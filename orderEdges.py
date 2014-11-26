#!/cygdrive/c/Python27/python.exe
# 
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-01-08
#
# Accepts a weighted edgelist from stdin
# and writes two columns.  A node identifier and it's
# maximum weight.  Results are ordered by maximum weight
#
# Status: In development
# Version: 1.0
# Language: Python
# Changelog:
#    + 2013-01-08: Created

import sys;
import networkx as nx;

def main():
	# Read the weighted edgelist
	G = nx.read_weighted_edgelist(sys.stdin)

	weights = [ (n1, n2, G[n1][n2]['weight']) for n1, n2 in G.edges_iter() ]
	
	# Sort nodes in decending order
	weights.sort(key=lambda x : x[2], reverse=True)
	
	# Write nodes and maxWeights out with UNIX-style line endings
	for t in weights:
		sys.stdout.write("%s\t%s\t%f\n" % t )

main()

