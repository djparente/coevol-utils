# Daniel J. Parente
# MD/PhD Candidate
# Swint-Kruse Laboratory
# University of Kansas Medical Center
# 31 October 2012
#
# Performs co-evolution analysis in a database-independent manner

# Assumptions:
# That Fodor's java directory is ../covariance1_1_nofilter
# That ZNMI is in the ./bin directory
# The AverageNetworkPath program is in the ./bin directory
# That a translation file is available in data_files

fodorClassPath="../covariance1_1_nofilter"
ZNMIPath="./bin/ZNMI.exe"
ZNDAMIPath="./bin/ZNDAMI.exe"
translationFile="./data_files/col_to_lac_map.tsv"
AverageNetworkPath="./bin/AverageNetwork.exe"
ApplyRenumberingPath="./bin/ApplyRenumbering.exe"

declare -a algorithmName=(elsc omes mcbasc sca)
declare -a fodorname=(ELSCCovariance OmesCovariance McBASCCovariance JavaSCA)

mkdir result;
mkdir final_result;
mkdir raw_result;
mkdir mapped_result

#Perform coevoluion analysis for all but ZNMI
for aIndex in `seq 0 3`
do
	mkdir raw_result/${algorithmName[$aIndex]}
	mkdir mapped_result/${algorithmName[$aIndex]}

	avgResultPath=result/${algorithmName[$aIndex]}.avg.map.tsv
	avgResultWelPath=final_result/${algorithmName[$aIndex]}.avg.map.wel
	
	for i in subsets/fodor/*;
	do
		raw_output=raw_result/${algorithmName[$aIndex]}/`basename $i`.${algorithmName[$aIndex]}
		mapped_output=mapped_result/${algorithmName[$aIndex]}/`basename $i`.${algorithmName[$aIndex]}.map
	
		echo Performing ${algorithmName[$aIndex]} on $i
	
		java -classpath "$fodorClassPath" covariance.algorithms.${fodorname[$aIndex]} $i $raw_output;
		
		echo "   "Applying filtration and renumbering criteria...
		tail -n +2 $raw_output | dos2unix | $ApplyRenumberingPath $translationFile 1 | dos2unix | $ApplyRenumberingPath $translationFile 2 > $mapped_output
	done;
	
	echo Averaging the network for ${algorithmName[$aIndex]}
	$AverageNetworkPath mapped_result/${algorithmName[$aIndex]}/*.map > $avgResultPath
	cat $avgResultPath | cut -f1-3 > $avgResultWelPath
done;

#Perform coevolution analysis for ZNMI
mkdir raw_result/znmi
mkdir mapped_result/znmi

avgResultPath=result/znmi.avg.map.tsv
avgResultWelPath=final_result/znmi.avg.map.wel
	
for i in subsets/fasta/*;
do
	raw_output=raw_result/znmi/`basename $i`.znmi
	mapped_output=mapped_result/znmi/`basename $i`.znmi.map
	
	echo Performing znmi on $i

	$ZNMIPath $i $raw_output.gz
	gunzip $raw_output.gz
	
	echo "   "Applying filtration and renumbering criteria...
	tail -n +2 $raw_output | dos2unix | $ApplyRenumberingPath $translationFile 1 | dos2unix | $ApplyRenumberingPath $translationFile 2 > $mapped_output
done;
	
echo Averaging the network for znmi
$AverageNetworkPath mapped_result/znmi/*.map > $avgResultPath
cat $avgResultPath | grep -e "100$" -e "[6789].$" -e "5[123456789]$" | cut -f1-3 > $avgResultWelPath

#Perform coevolution analysis for ZNDAMI
mkdir raw_result/zndami
mkdir mapped_result/zndami

avgResultPath=result/zndami.avg.map.tsv
avgResultWelPath=final_result/zndami.avg.map.wel
	
for i in subsets/fasta/*;
do
	raw_output=raw_result/zndami/`basename $i`.zndami
	mapped_output=mapped_result/zndami/`basename $i`.zndami.map
	
	echo Performing zndami on $i

	$ZNDAMIPath $i $raw_output.gz
	gunzip $raw_output.gz
	
	echo "   "Applying filtration and renumbering criteria...
	tail -n +2 $raw_output | dos2unix | $ApplyRenumberingPath $translationFile 1 | dos2unix | $ApplyRenumberingPath $translationFile 2 > $mapped_output
done;
	
echo Averaging the network for zndami
$AverageNetworkPath mapped_result/zndami/*.map > $avgResultPath
cat $avgResultPath | grep -e "100$" -e "[6789].$" -e "5[123456789]$" | cut -f1-3 > $avgResultWelPath