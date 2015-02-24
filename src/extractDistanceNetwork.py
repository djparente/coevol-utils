#!/cygdrive/c/Python27/python.exe
#
# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 2012-10-10

import sys
import numpy as np
import scipy as sp
import scipy.linalg
from scipy import stats
import networkx as nx
import itertools
import math

def minDist(list1, list2):
	return min([dist(x,y) for x,y in itertools.product(list1, list2)])

def dist(x, y):
	return math.sqrt((x[0] - y[0])**2 + (x[1] - y[1])**2 + (x[2] - y[2])**2)
	
def print_weighted_edgelist(G):
	print "\n".join(["\t".join(map(str,[v1, v2, G[v1][v2]['weight']])) for v1, v2 in G.edges_iter()])

#Main body
def main_generate_contact_network():
	if len(sys.argv) != 3: 
		sys.exit("Usage: [PDB Atoms Path] [Allowed chains]")

	pdbPath=sys.argv[1]
	allowedChains = sys.argv[2]
		
	distNet = getDistanceNetwork(pdbPath, allowedChains)
			
	print_weighted_edgelist(distNet)

def getDistanceNetwork(pdbPath, allowedChains):
	chainDict = { c : 1 for c in allowedChains }

	pdbRecords = [ [ line[22:27].strip(), tuple(map(float,[line[30:38], line[38:46], line[46:54]])) ] for line in open(pdbPath) if line[0:4]=="ATOM" and line[21:22] in chainDict ]
	allAtoms = set([record[0] for record in pdbRecords])
	
	distNet = nx.Graph()

	atomGroups = { atom: [ record[1] for record in pdbRecords if record[0]==atom ] for atom in allAtoms }
	for atom1 in atomGroups:
		for atom2 in atomGroups:
			distNet.add_edge(atom1, atom2, weight=minDist(atomGroups[atom1], atomGroups[atom2]))
			
	return distNet	

def main():
	main_generate_contact_network()
	
main()