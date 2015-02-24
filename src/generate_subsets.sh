# Daniel J. Parente
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 20 June 2011
#
# generate_subsets.sh
#
# Generates N subsets of an input file
#
# Status of this file: Operational; 20 June 2011

# Read input data
input_file=$1;
num_in_subset=$2;
num_to_create=$3;

create_less_one=`expr $num_to_create - 1`;

# Make the output directory
outputDir="subsets/fodor/"
mkdir -p $outputDir;

baseInputName=`basename $input_file .aligned.fodor`;

for i in `seq 0 $create_less_one`;
do 
	sort $1 -R | head -n $num_in_subset > $outputDir`printf %04d $i`_$num_in_subset"_"$baseInputName.subset.aligned.fodor;
done