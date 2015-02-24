#!/bin/bash
# Daniel J. Parente
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 29 June 2010

#Fodor2fasta.sh
# Converts a file in the format:
# IDENTIFIER (spaces) ALIGNMENT
# into fasta format for proteins
#
# Input from standard input and output to standard output

dos2unix | sed 's/\([^ ]*\) *\([^ ]*\)/>\1\n\2/g' | perl -pe 's/(.{60,60}(?!$))/\1\n/g if not /^>/;' < /dev/stdin

