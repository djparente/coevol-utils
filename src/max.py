#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center
# 2012-10-10

# Calculates the maximum edge weight (MEW) of a protein co-evolution network
# outputting them to stdout in tab-separated format

import sys
from scipy import stats
import networkx as nx

#Main body
def main():
	if len(sys.argv) != 2: 
		sys.exit("Usage: [Weighted edgelist]")

	#Read arguments
	path=sys.argv[1]
	
	#Read the graph
	G=nx.read_weighted_edgelist(path)
		
	# Calculate the maximum edge weight
	mew = { node : max([G[node][x]['weight'] for x in G.neighbors(node)]) for node in G.nodes_iter() }
		
	for node in mew:
		 sys.stdout.write(str(node) + "\t" + str(mew[node]) + "\n")
	

	
main()