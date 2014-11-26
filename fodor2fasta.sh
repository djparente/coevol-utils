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

sed 's/\([^ ]*\) *\([^ ]*\)/>\1\n\2/g' | fold --width=60 < /dev/stdin

