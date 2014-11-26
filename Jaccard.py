#!/cygdrive/c/Python27/python.exe
# 
# Daniel J. Parente
# MD/PhD Candidate
# University of Kansas Medical Center
# 2013-01-08
#
# Accepts two lists of nodes, ordered by some score, and determines the
#   Jaccard index as a function of nodes included.
#
# Status: Complete
# Version: 2.0
# Language: Python
# Changelog:
#    + 2013-01-08: Created
#    + 2013-01-13: Pulled Jaccard calculation of of the main loop to facilitate
#                    inclusion of randomized calculations
#    + 2013-01-14: Modified to output the maximum Jaccard index too
#    + 2013-01-14: Corrected Z-score (used to div by variance, now by stdev); 
#                   incremented version number to 2.0

# Calculate the jaccard index of two sets of nodes

import sys
import math
import random
import scipy.stats

def usage():
	sys.stderr.write("Usage: jaccardNode [Ordered node file1] [Ordered node file 2]\n")
	
def errorOut(message, code):
	sys.stderr.write("Error: %s" % message)
	exit(code)
	
def readNodeList(path):
	return [ line.strip().split('\t')[0] for line in open(path) ]

# Calculate the jaccard index of two lists for minNode number of increasing
#  thresholds.
def jaccard(list1, list2, minNodes):
	jArr = []
	inter = 0
	union = 0
	unionDict = {}
	for i in xrange(minNodes):
		if list1[i] in unionDict:
			del unionDict[list1[i]]
			inter += 1
		else:
			unionDict[list1[i]] = 1
			union += 1
			
		if list2[i] in unionDict:
			del unionDict[list2[i]]
			inter += 1
		else:
			unionDict[list2[i]] = 1
			union += 1
		
		J = inter / float(union)
		jArr.append(J)
	return jArr
	
# Calculates the mean and stdev of jaccard scores
#  Warning: Destroys the ordering of each list.
def rndJaccard(list1, list2, minNodes, numTrials):
	# Perform shuffles and recalculations
	allJArr = []
	for i in xrange(numTrials):
		# Shuffle the lists
		random.shuffle(list1)
		random.shuffle(list2)
		
		# Calculate a Jaccard score
		jArr = jaccard(list1, list2, minNodes)
		allJArr.append(jArr)
		
	# Calculate statistics
	trialSeries = [ [ jArr[i] for jArr in allJArr ] for i in xrange(len(allJArr[0])) ]
	
	trialStats = [ scipy.stats.describe(ser) for ser in trialSeries ]
	
	# Extract mean, standard deviation and n for return
	retStats = [ (stat[2], stat[3], stat[0]) for stat in trialStats ]
	return retStats
	
def maxJaccard(nUnion, minNodes):
	return [ min(i, nUnion) / float( min(i, nUnion) + 2*max(0,i-nUnion) ) for i in xrange(1,minNodes+1) ]
	
def main():
	# Check arguments
	if len(sys.argv) < 3:
		usage()
		errorOut("Not enough arguments", 255)
		
	#Node sets
	list1 = readNodeList(sys.argv[1])
	list2 = readNodeList(sys.argv[2])
	
	minNodes = min(len(list1), len(list2))
	
	# Calculate the Jaccard index
	J = jaccard(list1, list2, minNodes)
	
	# Calculate scores due to random chance
	rJ = rndJaccard(list1, list2, minNodes, 1000)
	
	# Get the maximum Jaccard index
	nInter = len(set(list1).intersection(set(list2)))
	maxJ = maxJaccard(nInter, minNodes)	
	
	# Combine data, calculate Z score and write to console
	for index, dat in enumerate(zip(J, rJ, maxJ)):
		z = (dat[0] - dat[1][0])/math.sqrt(dat[1][1])
		print index+1, dat[0], dat[1][0], dat[1][1], z, dat[2]
	
	

main()