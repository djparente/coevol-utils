#!/usr/bin/Rscript

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# Calculates the Spearman R^2 matrix on a matrix of six variables
# and lists the correlation of the first variable with the 2-6th
# variable in linear format

args <- commandArgs(trailingOnly = TRUE)
x = read.table(args[1], head=TRUE, sep="\t");
res = cor(x,method="spearman", use="pairwise.complete.obs")^2;

#res
cat(res[1,2]); cat("\n");
cat(res[1,3]); cat("\n");
cat(res[1,4]); cat("\n");
cat(res[1,5]); cat("\n");
cat(res[1,6]); cat("\n");
