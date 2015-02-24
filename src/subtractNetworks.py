#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 2012-10-10

# Aligns a set of weighted edgelists, reporting their weights

import sys
import numpy as np
import scipy as sp
import scipy.linalg
from scipy import stats
import networkx as nx	

def threshold_edges(G, thresh):
	edges_below_threshold=[e for e in G.edges_iter() if G[e[0]][e[1]]['weight'] < thresh]
	for e1, e2 in edges_below_threshold:
		G.remove_edge(e1,e2)
		
def strip_isolates(G):
	isolated_nodes=[n for n in G.nodes_iter() if G.degree(n) == 0]
	#Remove isolates
	for n in isolated_nodes:
		G.remove_node(n)
		
#Normalizes weights to the interval [0,1]		
def unityWeightNormalize(G):
	sortedWeights = [ G[e1][e2]['weight'] for e1, e2 in G.edges_iter() ]
	sortedWeights.sort()
	
	minWeight=sortedWeights[0]
	maxWeight=sortedWeights[len(sortedWeights)-1]
	
	scaleFactor = 1.0 / (maxWeight - minWeight)
	
	for e1, e2 in G.edges_iter():
		G[e1][e2]['weight'] = (G[e1][e2]['weight'] - minWeight)*scaleFactor
		
def main():
	if len(sys.argv) != 3: 
		sys.exit("Usage: [Weighted edgelist1] [Weighted edgelist2]")

	#Read the graphs
	graphs = [ nx.read_weighted_edgelist(path) for path in sys.argv[1:] ]
	graphDicts = [ { (e1, e2) : G[e1][e2]['weight'] for e1, e2 in G.edges_iter()} for G in graphs]
	#allEdgePairs = set().union(*[G.keys() for G in graphDicts])
	
	#Write a header
	#print "\t".join(['node1', 'node2'] + sys.argv[1:] + ['\n'])
	
	#For every edge pair, construct and write a line like: node1 (tab) node2 (tab) weight1 (tab) weight2...
	#print "\n".join(["\t".join(map(str,[edge[0], edge[1], graphDicts[0][edge] - graphDicts[1][edge]])) for edge in graphDicts[0].keys()])
	for edge in graphs[0].edges_iter():
		if edge in graphDicts[1]:
			print "\t".join(map(str,[edge[0], edge[1], graphDicts[0][edge] - graphDicts[1][edge]]))
		
	
	

main()