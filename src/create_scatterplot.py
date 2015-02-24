#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# This code is adapted from the example available at
# http://pandasplotting.blogspot.com/2012/04/added-kde-to-scatter-matrix-diagonals.html

# Creates a scatterplot matrix (off-diagonals) with a kernal density estimate (KDE)
# of the distribution of (univariate) data on the diagonal

import numpy as np
import matplotlib.pyplot as plt
import pandas
import sys

infile=sys.argv[1]
outfile=sys.argv[2]

maindata = pandas.read_csv(infile, sep="\t")

plt.rcParams['patch.facecolor'] = 'k'	# Make the markers black

# Plot
ax = pandas.tools.plotting.scatter_matrix(maindata, alpha=0.1, marker='k.', figsize=(8,8), diagonal='kde', range_padding=0.1)

# Give a small inter-plot spacing
plt.subplots_adjust(wspace=.05, hspace=.05)

#Save the figure
plt.savefig(outfile, dpi=600)