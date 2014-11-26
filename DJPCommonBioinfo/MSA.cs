/*
 * Daniel J Parente
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * Date: 11/28/2011
 * Time: 10:11 AM
 * 
 */
using System;
using System.Collections.Generic;

namespace DJPCommonBioinfo
{
	/// <summary>
	/// Description of MSA.
	/// </summary>
	public class MSA
	{
		char[] data;
		string[] names;
		//Dictionary<string, int> nameRowMap;
		private int rows;
		private int cols;
		
		public const char GAP = '-';
		public const string GAPSTR = "-";
		
		static Random rng = new Random();
		
		public int Rows
		{
			get { return this.rows; }
		}
		
		public int Cols
		{
			get { return this.cols; }
		}
		
		public string[] Names
		{
			get { return names; }
		}
		
		//Direct access to the underlying data
		public char this[int index]
		{
			get { return data[index]; }
			set { data[index] = value; }
		}
		
		//Access by row and column to the underlying data
		public char this[int r, int c]
		{
			get { return data[r * cols + c]; }
			set { data[r* cols + c] = value; }
		}
		
		public MSA(string path)
		{
			string[] lines = System.IO.File.ReadAllLines(path);
			
			constructFromLines(lines);
		}
		
		public MSA(System.IO.TextReader reader)
		{
			string[] lines = reader.ReadToEnd().Replace("\r\n", "\n").Split('\n');
			constructFromLines(lines);
			
			
			//Old version, but hung program if last line did not end in EOL
//			List<string> lines = new List<string>();
//			string line;
//			while( true )
//			{
//				line = reader.ReadLine();
//				if( line == null )
//					break;
//				
//				lines.Add(line);
//			}
//			
//			constructFromLines(lines.ToArray());
		}
		
		private void constructFromLines(string[] lines)
		{
			List<string> sequences = new List<string>();
			List<string> names = new List<string>();
			
			parseSeqsAndNames(lines, sequences, names);
			lines = null;
			
			//Validate: all sequences must be of equal length
			for(int i = 1; i < sequences.Count; i++)
				if( sequences[0].Length != sequences[i].Length ) 
					throw new ArgumentException("Line lengths not equivalent throughout the MSA on line " + i);
			
			this.rows = sequences.Count;
			this.cols = sequences[0].Length;
			
			//this.nameRowMap = new Dictionary<string, int>(sequences.Count);
			
			data = new char[this.rows * this.cols];
			int pointer = 0;
			for(int r = 0; r < this.rows; r++)
			{
				for(int c = 0; c < this.cols; c++)
				{
					data[pointer] = sequences[r][c];
					pointer++;
				}
				
				//nameRowMap.Add(names[r], r);
			}
			
			this.names = names.ToArray();
		}
		
		//Copy constructor
		public MSA(MSA template)
		{
			this.data = new char[template.data.Length];
			this.names = new string[template.names.Length];
			this.rows = template.rows;
			this.cols = template.cols;
			
			Array.Copy(template.data, this.data, template.data.Length);
			Array.Copy(template.names, this.names, template.names.Length);
		}
		
		public MSA(char[,] sequences, string[] names)
		{
			this.rows = sequences.GetLength(0);
			this.cols = sequences.GetLength(1);
			
			//this.nameRowMap = new Dictionary<string, int>(this.rows);
			data = new char[this.rows * this.cols];
			int pointer = 0;
			for(int r = 0; r < this.rows; r++)
			{
				for(int c = 0; c < this.cols; c++)
				{
					data[pointer] = sequences[r,c];
					pointer++;
				}
				
				//nameRowMap.Add(names[r], r);
			}
			
			//Copy over the names
			this.names = new string[names.Length];
			Array.Copy(names, this.names, this.names.Length);
		}
		
		public MSA(List<string> sequences, List<string> names)
		{
			this.rows = sequences.Count;
			this.cols = sequences[0].Length;
			
			//this.nameRowMap = new Dictionary<string, int>(this.rows);
			data = new char[this.rows * this.cols];
			int pointer = 0;
			for(int r = 0; r < this.rows; r++)
			{
				if( sequences[r].Length != this.cols )
				   throw new ArgumentException("All aligned sequences were not of the same length");
				
				for(int c = 0; c < this.cols; c++)
				{
					data[pointer] = sequences[r][c];
					pointer++;
				}
				
				//nameRowMap.Add(names[r], r);
			}
			
			//Copy over the names
			this.names = new string[names.Count];
			for(int i = 0; i < names.Count; i++)
				this.names[i] = names[i];
		}
		
		public static MSA constructFromFodor(string path)
		{
			string[] delim = { "\t", " " };
			string[] lines = System.IO.File.ReadAllLines(path);
			
			List<string> names = new List<string>();
			List<string> seqs = new List<string>();
			
			for(int i = 0; i < lines.Length; i++)
			{
				//Collapse delimiters
				for(int k = 0; k < delim.Length; k++)
				{
					while( true )
					{
						int size = lines[i].Length;
						lines[i] = lines[i].Replace(delim[k] + delim[k], delim[k]);
						if( size == lines[i].Length )
							break;
					}
				}
				
				string[] fields = lines[i].Split(delim, StringSplitOptions.None);
				if( fields.Length != 2 )
				   throw new ArgumentException(string.Format("Error parsing fodor-formatted MSA: Line {0} does not contain two fields", (i+1)));
				   
				   names.Add(fields[0]);
				   seqs.Add(fields[1]);
			} 
			
			return new MSA(seqs, names);
		}
		
		public MSA stripGapCols()
		{
			//Determine which columns are only gaps
			bool[] onlyGaps = new bool[this.cols];
			int numOnlyGaps = 0;
			for(int c = 0; c < onlyGaps.Length; c++)
			{
				onlyGaps[c] = true;
				for(int r = 0; r < this.rows; r++)
				{
					//As soon as a gap is detected; break out of the inner loop
					if( this[r,c] != MSA.GAP )
					{
						onlyGaps[c] = false;
						break;
					}
				}
				
				if( onlyGaps[c] )
					numOnlyGaps++;
			}
			
			//Copy data over
			char[,] newSeqs = new char[this.rows,this.cols - numOnlyGaps];
			int colPtr = 0;
			int newColPtr = 0;
			while( true )
			{
				while( colPtr < this.cols && onlyGaps[colPtr] )
					colPtr++;
				
				if( colPtr == this.cols )
					break;
				
				for(int r = 0; r < this.rows; r++)
					newSeqs[r, newColPtr] = this[r,colPtr];
				
				//Update the pointers
				colPtr++;
				newColPtr++;
			}
			
			return new MSA(newSeqs, this.names);
		}
		
		public MSA stripRedundant(double threshold)
		{
			//Determine which rows are redundant
			
			bool[] redundant = new bool[this.Rows];	//Initialized to false
			int numRedundant = 0;
			
			for(int r1 = 0; r1 < this.rows; r1++)
			{
				if( redundant[r1] )
				   continue;
				
				for(int r2 = r1+1; r2 < this.rows; r2++)
				{
					if( redundant[r2] )
						continue;
					
					if( getRowIdentity(r1,r2) > threshold )
					{
						redundant[r2] = true;
						numRedundant++;
					}
				}
			}
			
			
			//Copy data over
			char[,] newSeqs = new char[this.rows - numRedundant,this.cols];
			string[] newNames = new String[ this.rows - numRedundant];
			int rowPtr = 0;
			int newRowPtr = 0;
			while( true )
			{
				while( rowPtr < this.rows && redundant[rowPtr] )
					rowPtr++;
				
				if( rowPtr == this.rows )
					break;
				
				for(int c = 0; c < this.cols; c++)
					newSeqs[newRowPtr,c] = this[rowPtr,c];
				
				newNames[newRowPtr] = this.names[rowPtr];
				
				//Update the pointers
				rowPtr++;
				newRowPtr++;
			}
			
			if( newRowPtr != this.rows - numRedundant )
				throw new InvalidOperationException("Did not correctly eliminate redundant rows");
			
			return new MSA(newSeqs, newNames);
		}
		
		//Ignores double gaps (but counts a missed-gap as wrong)
		public double getRowIdentity(int row1, int row2)
		{
			int hit = 0;
			int miss = 0;
			for(int c = 0; c < this.Cols; c++)
			{				
				if( this[row1, c] == this[row2,c] && this[row1,c] != '-' )
					hit++;
				else if ( this[row1, c] != this[row2,c] )
					miss++;
			}
			
			return ((double)hit)/(hit+miss);
		}
		
		
		private static void parseSeqsAndNames(string[] lines, List<string> sequences, List<string> names)
		{			
			string curName = null;
			string curSeq = "";
			bool hasReadSeq = false;
			for(int i = 0; i < lines.Length; i++)
			{
				string line = lines[i];
				if( line.StartsWith(">") )
				{
					if( hasReadSeq )
					{
						sequences.Add(curSeq);
						names.Add(curName);
					}
					
					curName = line.Substring(1);
					curSeq = "";
					hasReadSeq = true;
					continue;
				}
				
				if( !hasReadSeq )
					throw new ArgumentException("Attempted to read sequence without identifier");
				
				curSeq += line;
			}
			
			if( hasReadSeq )
			{
				sequences.Add(curSeq);
				names.Add(curName);
			}
		}
		
		public int getRowOffset(int row)
		{
			if( row >= this.rows )
				throw new ArgumentException("Invalid row");
			
			return row * this.cols;
		}
	
		public string extractSequence(int row)
		{
			int index = this.cols * row;
			
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for(int i = 0; i < this.cols; i++)
			{
				sb.Append(data[index]);
				index++;
			}
			
			return sb.ToString();
		}
		
		public string extractUngappedSequence(int row)
		{
			return extractSequence(row).Replace(GAPSTR, "");
			
		}
		
		public void displayConsole()
		{
			displayConsole(0);
		}
		
		public void displayConsole(int minNameLength)
		{
			Console.WriteLine(getLongString(minNameLength));
		}
		
		public string getLongString()
		{
			return getLongString(0);
		}
		
		public string getFastaString()
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for(int i = 0; i < names.Length; i++)
			{
				sb.Append(">");
				sb.AppendLine(names[i]);
				sb.AppendLine(extractSequence(i));
			}
			
			return sb.ToString();
		}
		
		public string getLongString(int minNameLength)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			int longNameLen = Math.Max(minNameLength, getLongestNameLength());
			for(int i = 0; i < names.Length; i++)
			{
				sb.Append(names[i].PadRight(longNameLen + 1, ' '));
				sb.Append(extractSequence(i));
				sb.AppendLine();
			}
			
			return sb.ToString();
		}
		
		public int getLongestNameLength()
		{
			int result = 0;
			for(int i = 0; i < names.Length; i++)
				if( names.Length > result )
					result = names.Length;
			
			return result;
		}
		
		public void shuffleAllColumns()
		{
			for(int c = 0; c < this.cols; c++)
				shuffleColumn(c);
		}
		
		public void shuffleColumn(int colNum)
		{
			char temp;
			for(int r = 0; r < rows; r++)
			{
				int rowToSwap = rng.Next(r,this.Rows);
				temp = this[r, colNum];
				this[r,colNum]=this[rowToSwap,colNum];
				this[rowToSwap,colNum] = temp;
			}
		}
	}
}
