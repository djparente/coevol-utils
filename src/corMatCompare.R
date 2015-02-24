#!/usr/bin/Rscript

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# Calculates the Spearman R^2 matrix of 10 variables, with a
# relationship between the 1st and 6th, 2nd and 7th, etc. and displays the
# correlation between the related variable pairs in linear format

args <- commandArgs(trailingOnly = TRUE)
x = read.table(args[1], head=TRUE, sep="\t");
res = cor(x,method="spearman", use="pairwise.complete.obs")^2;


cat(res[1,6]); cat("\n");
cat(res[2,7]); cat("\n");
cat(res[3,8]); cat("\n");
cat(res[4,9]); cat("\n");
cat(res[5,10]); cat("\n");



