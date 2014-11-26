#!/bin/bash

# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center

scriptPath="."
createMapScriptPath=$scriptPath/createMap.py
composeRenumScriptPath=$scriptPath/composeRenumberings.py
invMapScriptPath=$scriptPath/invertMap.py

# Read command line arguments
if [ $# -ne 5 ]; then
	echo Usage: sh Get_Global_Map.sh [Subfamily MSA] [Reference MSA] [Subfam Ref Seq Name, In SF Align] [Subfam Ref Seq Name, In Ref Align] [Global Ref Seq Name, In Ref Align]
	exit -1;
fi

subMSAPath=$1
refMSAPath=$2
sfNameSub=$3
sfNameRef=$4
refNameRef=$5

# Declare temporary file path names
sfcol_to_bn=`mktemp -p .` || exit 1
bn_to_recol=`mktemp -p .` || exit 1
bn_to_bnlac=`mktemp -p .` || exit 1

# Create the three intermediate maps
$createMapScriptPath $subMSAPath $sfNameSub 0.198515243 .5 | dos2unix | grep -v "[^0-9]$" | dos2unix > $sfcol_to_bn
$createMapScriptPath $refMSAPath $sfNameRef -100 100 | dos2unix | $invMapScriptPath | dos2unix > $bn_to_recol
$createMapScriptPath $refMSAPath $refNameRef -100 100 | dos2unix > $bn_to_bnlac

# Compose the three maps into a single map (removing the intermediates)
$composeRenumScriptPath $sfcol_to_bn $bn_to_recol $bn_to_bnlac | sort -n | dos2unix

#Cleanup temporary files
rm $sfcol_to_bn
rm $bn_to_recol
rm $bn_to_bnlac