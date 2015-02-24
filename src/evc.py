#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center
# 2012-10-10

# Calculates the eigenvector centrality of every node in a protein co-evolution
# network outputting them to stdout in tab-separated format

import sys
from scipy import stats
import networkx as nx
		
def normalizeEdges(G):
	minWeight=1000000
	maxWeight=-1000000
	
	for e1, e2 in G.edges_iter():
		if G[e1][e2]['weight'] < minWeight:
			minWeight = G[e1][e2]['weight']
		if G[e1][e2]['weight'] > maxWeight:
			maxWeight = G[e1][e2]['weight']
	
	scaleFactor = 1.0 / (maxWeight - minWeight)
	
	for e1, e2 in G.edges_iter():
		G[e1][e2]['weight'] -= minWeight
		G[e1][e2]['weight'] *= scaleFactor
		
#Main body
def main():
	if len(sys.argv) != 2: 
		sys.exit("Usage: [Weighted edgelist]")

	#Read arguments
	path=sys.argv[1]
	
	#Read the graph
	G=nx.read_weighted_edgelist(path)
				
	#Normalized edges to [0,1]
	normalizeEdges(G)
	
	# Calculate EVC
	evcent=nx.eigenvector_centrality(G);
	for node in evcent:
		 sys.stdout.write(str(node) + "\t" + str(evcent[node]) + "\n")
	
main()