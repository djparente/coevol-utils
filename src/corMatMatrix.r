#!/usr/bin/Rscript

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# Calculates the Spearman R^2 matrix of any number of variables
# and displays on the command line

args <- commandArgs(trailingOnly = TRUE)
x = read.table(args[1], head=TRUE, sep="\t");
res = cor(x,method="spearman", use="pairwise.complete.obs")^2;

res



