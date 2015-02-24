#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

# Accepts a co-evolution and distance network as weighted edgelists and annotates the distance
# for the co-evolving pair

import sys
import networkx as nx

def main():
	coevnet = nx.read_weighted_edgelist(sys.argv[1])
	distnet = nx.read_weighted_edgelist(sys.argv[2])
	
	for e1, e2 in coevnet.edges_iter():
		print e1, e2, coevnet[e1][e2]['weight'], distnet[e1][e2]['weight']


main()