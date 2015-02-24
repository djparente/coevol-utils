#!/usr/bin/Rscript

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# Calculates the Spearman R^2 matrix of five variables
# and displays the 10 off-diagonal elements in linear
# format

args <- commandArgs(trailingOnly = TRUE)
x = read.table(args[1], head=TRUE, sep="\t");
res = cor(x,method="spearman", use="pairwise.complete.obs")^2;

#res
cat(res[1,2]); cat("\n");
cat(res[1,3]); cat("\n");
cat(res[1,4]); cat("\n");
cat(res[1,5]); cat("\n");
cat(res[2,3]); cat("\n");
cat(res[2,4]); cat("\n");
cat(res[2,5]); cat("\n");
cat(res[3,4]); cat("\n");
cat(res[3,5]); cat("\n");
cat(res[4,5]); cat("\n");


