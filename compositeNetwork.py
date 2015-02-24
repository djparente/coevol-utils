#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

# 2013-07-15
# Averages networks produced by divergent co-evolutionary strategies together

# Version: 0.1
# Changelog:
# 	+ 2013-07-15: Created, forked from alignMultiNetworks_new.py

import sys
import networkx as nx
import numpy
import scipy
import scipy.stats


#Thresholds edges based on a z-score		
def zNormalize(G):
	weightStats = scipy.stats.describe([ G[e1][e2]['weight'] for e1,e2 in G.edges_iter() ])
		
	mean = weightStats[2]
	stdev = scipy.sqrt(weightStats[3])
	
	for e1, e2 in G.edges_iter():
		G[e1][e2]['weight'] = (G[e1][e2]['weight'] - mean)/stdev
		
def main():
	if len(sys.argv) < 2: 
		sys.exit("Usage: [Weighted edgelist1] [Weighted edgelist2...]")

	#Read the graphs
	graphs = [ nx.read_weighted_edgelist(path) for path in sys.argv[1:] ]
	
	for g in graphs:
		zNormalize(g)

	graphDicts = [ { tuple(sorted([e1, e2])) : G[e1][e2]['weight'] for e1, e2 in G.edges_iter()} for G in graphs]
	allEdgePairs = set().union(*[G.keys() for G in graphDicts])
	
	#Write a header
	print "\t".join(['node1', 'node2', 'average'] + sys.argv[1:] + ['\n'])
	
	#For every edge pair, construct and write a line like: node1 (tab) node2 (tab) weight1 (tab) weight2...	
	for edge in allEdgePairs:
		scores = [ x for x in [ G.get(edge, '') for G in graphDicts] if x != '' ]
		avgScore = numpy.mean(scores)
		
		print str.format("{0!s}\t{1!s}\t{2!s}", edge[0], edge[1], avgScore)
		
		#print "\t".join([str(edge[0]), str(edge[1]), str(avgScore), "\t".join([str(G.get(edge,'')) for G in graphDicts])])
		
	
	

main()