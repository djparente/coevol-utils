#!/cygdrive/c/Python27/python.exe
#
# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 2012-10-10

# Aligns a set of weighted edgelists, reporting their weights

import sys
import networkx as nx	

		
def main():
	if len(sys.argv) < 2: 
		sys.exit("Usage: [Weighted edgelist1] [Weighted edgelist2...]")

	#Read the graphs
	graphs = [ nx.read_weighted_edgelist(path) for path in sys.argv[1:] ]

	graphDicts = [ { tuple(sorted([e1, e2])) : G[e1][e2]['weight'] for e1, e2 in G.edges_iter()} for G in graphs]
	allEdgePairs = set().union(*[G.keys() for G in graphDicts])
	
	#Write a header
	print "\t".join(['node1', 'node2'] + sys.argv[1:] + ['\n'])
	
	#For every edge pair, construct and write a line like: node1 (tab) node2 (tab) weight1 (tab) weight2...
	print "\n".join(["\t".join([str(edge[0]), str(edge[1]), "\t".join([str(G.get(edge,'')) for G in graphDicts])]) for edge in allEdgePairs])
		
	
	

main()