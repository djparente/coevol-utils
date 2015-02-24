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
# Status: Complete
# Version: 1.1
# Language: Python
# Changelog:
#    + 2013-01-08: Created
#    + 2013-01-08: Completed development.  Manually spot-checked node result to validate.

import sys;
import networkx as nx;

def maxWeight(G, node):
	#return max( [ G[node][neighbor]['weight'] for neighbor in G.neighbors(node) ] )
	return sorted([ G[node][neighbor]['weight'] for neighbor in G.neighbors(node) ], reverse=True)

def main():
	# Read the weighted edgelist
	G = nx.read_weighted_edgelist(sys.stdin)

	# Calculate the maximum weight for each node
	maxWeights = [ (n, maxWeight(G,n)) for n in G.nodes_iter() ]
	
	# Sort nodes in decending order
	maxWeights.sort(key=lambda x : x[1], reverse=True)
	
	# Write nodes and maxWeights out with UNIX-style line endings
	for node, weight in maxWeights:
		sys.stdout.write("%s\t%f\n" % (node, weight[0]))

main()

