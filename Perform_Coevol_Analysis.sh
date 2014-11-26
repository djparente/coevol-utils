# Daniel J. Parente
# Swint-Kruse Laboratory
# University of Kansas Medical Center
#
# Perform_Coevol_Analysis.sh
# Performs an experiment on a target input file on a subset thereof
#

if [ $# -ne 3 ]; then
	echo Usage: sh Perform_Coevol_Analysis.sh [input file] [subset size] [num subsets]
	exit -1;
fi

#Read arguments
input_file=$1
subset_size=$2
num_of_subsets=$3

# Set up for processing
mkdir subsets
mkdir subsets/fasta
mkdir subsets/fodor


# Generate subsets
echo Generating new subset
sh scripts/generate_subsets.sh $input_file $subset_size $num_of_subsets;

# Generate fodor-formatted files
echo Constructing FASTA-formatted subsets
for i in subsets/fodor/*.fodor;
do
	cat $i | dos2unix | sh scripts/fodor2fasta.sh | dos2unix > subsets/fasta/`basename $i .fodor`.fasta
done;

# Execute co-evolution analysis
sh scripts/mainscript.sh