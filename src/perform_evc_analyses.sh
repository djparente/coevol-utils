#!/bin/sh
# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# Requires files:
# 	+ nets/
#		+ std/
#			(...coevolutionary weighted edgelists...)
#		+ sub/
#			(...coevolutionary weighted edgelists...)

# CompileMasterTable should be either in the execution directory
# or in a directory in the system's bin path

# This script assumes that there are five co-evolution networks
# in nets/std/ and nets/sub/ (calculable with the CoevolUtils
# software released with Parente and Swint-Kruse (2013) Multiple
# Co-evolutionary Networks are Supported by the Common Tertiary
# Scaffold of the LacI/GalR proteins.  PLoS ONE).  

# Upon completion, results are summarized in summary/ in three files:
# (1) summary/Raw_EVC_Table.txt - Each line is an algorithm pair in alphabetical order
#     (e.g. E vs M, E vs O, E vs S, E vs Z, M vs O, M vs S ... S vs Z) and the four
#     columns give the R^2 for RAW edge agreement, RAWsub agreement, EVC agreement,
#     and EVCsub agreement
# 
# (2) summary/Raw_vs_MEW_vs_EVC.txt  - Each line is an algorithm pair in alphabetical order
#     (e.g. E vs M, E vs O, E vs S, E vs Z, M vs O, M vs S ... S vs Z) and the three
#     columns give the R^2 for RAW edge agreement, MEWsub agreement, EVCsub agreement

# (3) summary/EVC-vs-MEW.txt - Each line is an algorithm (ELSC, McBASC, OMES, SCA,
#         ZNMI) and the single column gives the R^2 for the correlation between
#         EVC and MEW

echo === Calculating scores ===

# Calculate network summary statistics
for series in evc max; do
	mkdir $series
	mkdir $series/std
	mkdir $series/sub

	echo = $series =
	# If there are already calculated, comment out the next two lines
	for i in nets/std/*; do echo $i; ./scripts/$series.py $i > $series/std/`basename $i`.$series; done
	for i in nets/sub/*; do echo $i; ./scripts/$series.py $i > $series/sub/`basename $i`.$series; done
done;

# compiling master EVC tables
echo === Compiling tables ===
echo Compiling network alignment 
mkdir nums
mkdir full

# Raw network master tables
mkdir nums/raw
./scripts/alignMultiNetworks_new.py nets/std/* | dos2unix |  cut -f3- | ./scripts/fixheader.py ELSC McBASC OMES SCA ZNMI | dos2unix > nums/raw/std.txt
./scripts/alignMultiNetworks_new.py nets/sub/* | dos2unix |  cut -f3- | ./scripts/fixheader.py ELSC McBASC OMES SCA ZNMI | dos2unix > nums/raw/sub.txt

# Data series master tables
for series in evc max; do
	mkdir nums/$series
	echo $series
	CompileMasterTable $series/std/* | dos2unix | cut -f2- | ./scripts/fixheader.py ELSC McBASC OMES SCA ZNMI | dos2unix > nums/$series/std.txt	
	CompileMasterTable $series/sub/* | dos2unix | cut -f2- | ./scripts/fixheader.py ELSC McBASC OMES SCA ZNMI | dos2unix > nums/$series/sub.txt	
	
	mkdir full/$series
	CompileMasterTable $series/std/* | dos2unix | > full/$series/std.txt	
	CompileMasterTable $series/sub/* | dos2unix | > full/$series/sub.txt	
done;

# Joint table: EVC vs MEW
echo Compiling EVC-MEW joint table
mkdir nums/evc-max
CompileMasterTable evc/sub/* max/sub/* | dos2unix | cut -f2- > nums/evc-max/sub.txt

# Running correlations
echo === Running correlations ===
mkdir res

# Data series correlations
for series in raw evc max; do
	mkdir res/$series
	echo $series
	./scripts/corMat.R nums/$series/std.txt | dos2unix > res/$series/std.txt
	./scripts/corMat.R nums/$series/sub.txt | dos2unix > res/$series/sub.txt
done;

# EVCsub vs MEWsub correlation
echo Subtracted EVC vs MEW
mkdir res/evc-max
./scripts/corMatCompare.R nums/evc-max/sub.txt | dos2unix > res/evc-max/sub.txt


#Create summary reports
echo === Generating reports ===
mkdir summary

# Summary: Raw vs EVC
echo Summarizing Raw and EVC results into table
paste res/raw/std.txt res/raw/sub.txt res/evc/std.txt res/evc/sub.txt | dos2unix > summary/Raw_EVC_Table.txt

#Summary: Raw vs MEW
echo Summarizing Raw, MEW and EVC into table 
paste res/raw/std.txt res/max/sub.txt res/evc/sub.txt | dos2unix > summary/Raw_vs_MEW_vs_EVC.txt

# Summary: EVC vs MEW
echo Summarizing EVC-vs-Max head-to-head
cat res/evc-max/sub.txt | dos2unix > summary/EVC-vs-MEW.txt


# Create figures
mkdir figures
mkdir figures/raw
echo Creating raw figures - std
./scripts/create_scatterplot.py nums/raw/std.txt figures/raw/std.png
echo Creating raw figures - sub
./scripts/create_scatterplot.py nums/raw/sub.txt figures/raw/sub.png

mkdir figures/evc
echo Creating EVC figures - std
./scripts/create_scatterplot.py nums/evc/std.txt figures/evc/std.png
echo Creating EVC figures - sub
./scripts/create_scatterplot.py nums/evc/sub.txt figures/evc/sub.png