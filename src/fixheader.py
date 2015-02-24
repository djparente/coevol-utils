#!/cygdrive/c/Python27/python.exe

# Daniel J. Parente, Ph.D.
# Swint-Kruse Laboratory
# Physician Scientist Training Program
# University of Kansas Medical Center

# 2014-05-09

# Replaces the first line of a TSV file with header lines specified as
# space-separated command line arguments

import sys

def chomp(line):
	return line.replace("\r\n","").replace("\n","")

def main():
	lines = [ chomp(x) for x in sys.stdin ]
	header = "\t".join(sys.argv[1:])
	
	sys.stdout.write(header)
	sys.stdout.write("\n")
	
	for line in lines[1:]:
		sys.stdout.write(line)
		sys.stdout.write("\n")

if __name__ == '__main__':
	main()