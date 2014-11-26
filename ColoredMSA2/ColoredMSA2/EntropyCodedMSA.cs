/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2013-07-22
 * Time: 13:29
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using DJPCommonBioinfo;

namespace ColoredMSA2
{
	struct ConfigParams
	{
		public MSA jointMSA;
		public int cellWidth;
		public int cellHeight;
		public int cellsPerRow;
		public double entropyCutoff;
		public SimilarityMatrix simMat;
		public List<string> jointMSANames;
		public List<string> subMSANames;
		public List<MSA> subMSAs;
	}
	
	class EntropyCodedMSA
	{
		public static void Main(string[] args)
		{
			ConfigParams param = readConfig(args[0]);
			
			if( param.jointMSA.Rows != param.subMSAs.Count )
				throw new ArgumentException("Mismatch in the number of joint MSA rows and the sub-MSAs provided");
			
			List<Dictionary<int, int>> maps = new List<Dictionary<int, int>>();
			for(int i = 0; i < param.subMSAs.Count; i++)
			{
				//The subMSAs must be declared in the order in which they occur int he joint MSA
				if( param.jointMSANames[i] != param.jointMSA.Names[i] )
					Console.WriteLine("Mismatch in joint MSA name");
				
				//Get a column-to-column mapping between the joint MSA and the sub MSAs
				maps.Add(getMapping(param.jointMSA, param.subMSAs[i], param.jointMSANames[i], param.subMSANames[i]));
			}
			
			//Extract data about the columns of each sub-MSA
			List<List<double>> entropies = new List<List<double>>();			
			List<List<char>> mostCommonGlyphs = new List<List<char>>();
			for(int i = 0; i < param.subMSAs.Count; i++)
			{
				entropies.Add(entropy(param.subMSAs[i]));
				mostCommonGlyphs.Add(mostCommon(param.subMSAs[i]));
			}
			
			//#DEBUG
			/*
			for(int i = 0; i < param.jointMSA.Cols; i++)
			{
				int localC;
				if( !maps[4].TryGetValue(i, out localC) )
					Console.WriteLine("{0}\tNone\tNone", i);
				else
					Console.WriteLine("{0}\t{1}\t{2}", i, localC, entropies[4][localC]);
			}
			*/
			//#ENDDEBUG
			
			//Determine the number of conserved positions
			List<int> numCons = new List<int>();	//Number of conserved cells in a column of the joint MSA
			List<bool> ncSubs = new List<bool>();	//Whether a column of the joint MSA contains any non-conservative substitutions between positions conserved within subfamilies
			for(int jointCol = 0; jointCol < param.jointMSA.Cols; jointCol++)	//Loop over joint columns
			{
				//At the beginning, we haven't seen anything be conserved, so set up the arrays
				numCons.Add(0);
				ncSubs.Add(false);
				
				for(int i = 0; i < param.subMSAs.Count; i++)					//Loop over sub-MSAs
				{
					//Get the i'th sub-MSA column
					int subICol;
					if( !maps[i].TryGetValue(jointCol, out subICol) )
						continue;	//Continue if not mapping from jointColumn to a column in the i'th sub-MSA
					
					//If the sub-MSA column (joint MSA cell) is not conserved, continue
					if( entropies[i][subICol] > param.entropyCutoff )
						continue;
					
					//Otherwise, this cell in the joint MSA is conserved, so keep track of how often this happens
					numCons[jointCol]++;
					
					char glyphI = mostCommonGlyphs[i][subICol];
					
					for(int j = i+1; j < param.subMSAs.Count; j++)				//And loop over all pairs of sub-MSAs...
					{
						//Same checking as in the exterior loop
						int subJCol;
						if (!maps[j].TryGetValue(jointCol, out subJCol))
							continue;
						
						if( entropies[j][subJCol] > param.entropyCutoff )
							continue;
						
						//Another cell in the joint MSA column is conserved; see if this is non-conservative by checking the
						// provided similarity matrix
						char glyphJ = mostCommonGlyphs[j][subJCol];
						
						if( glyphI != MSA.GAP && glyphJ != MSA.GAP && param.simMat[glyphI, glyphJ] <= 0 )
							ncSubs[jointCol] = true;
					}
				}
			}
			
			//Set up the Bitmap
			int groups = param.jointMSA.Cols / param.cellsPerRow;
			if( param.jointMSA.Cols % param.cellsPerRow != 0 )
				groups++;
			
			int groupHeight = param.jointMSA.Rows * param.cellHeight;
			int spacerHeight = 2*param.cellHeight + 1;
			
			int totalWidth = param.cellWidth * param.cellsPerRow + 1;
			int totalHeight = groups * groupHeight + (groups - 1) * spacerHeight + 1;
			
			Bitmap result = new Bitmap(totalWidth, totalHeight);
			Graphics g = Graphics.FromImage(result);
			g.FillRectangle(Brushes.White, 0, 0, result.Width, result.Height);
			
			//Determine font size maximum
			int maxSize = maxFontSize(param, maps, entropies, mostCommonGlyphs, g);
			
			//Declare brushes
			Brush[] conCatBrush = new Brush[] {
				new SolidBrush(Color.FromArgb(255,255,0,208)),	//Magenta(0)
				new SolidBrush(Color.FromArgb(255,255,0,208)), //Magenta (1)
				new SolidBrush(Color.FromArgb(255,108,255,158)) //Green (2)
			};
			
			//Draw the bitmap
			for(int r = 0; r < param.jointMSA.Rows; r++)
			{
				for(int c = 0; c < param.jointMSA.Cols; c++)
				{
					int x = (c % param.cellsPerRow) * param.cellWidth;
					int gr = (c / param.cellsPerRow);
					int y = gr * (groupHeight + spacerHeight) + r * param.cellHeight;
					
					
					//Get the r'th sub-MSA column
					int subICol;
					if( !maps[r].TryGetValue(c, out subICol) )
						continue;	//Continue if not mapping from jointColumn to a column in the i'th sub-MSA
					
					//If the sub-MSA column (joint MSA cell) is not conserved, continue
					if( entropies[r][subICol] > param.entropyCutoff )
						continue;
					
					//Otherwise, this cell is conserved and requires a color
					if( ncSubs[c]  )				//Variable amino acid choice
						g.FillRectangle(conCatBrush[1], x, y, param.cellWidth, param.cellHeight);
					
					else if( numCons[c] >= 2  )		//Consistent amino acid choice
						g.FillRectangle(conCatBrush[0], x, y, param.cellWidth, param.cellHeight);
					
					//Singlet (Green)
					else if( numCons[c] >= 1 )
						g.FillRectangle(conCatBrush[2], x, y, param.cellWidth, param.cellHeight);
					
					StringFormat format = new StringFormat();
					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;
					
					//Draw the character occuring at this point
					char glyphI = mostCommonGlyphs[r][subICol];
					g.DrawString(glyphI.ToString(), new Font("Courier",maxSize), Brushes.Black, new RectangleF(x,y,param.cellWidth, param.cellHeight), format);
				}
			}
			
			//Draw boxes
			Pen boxPen = new Pen(Color.Black,6);
			for(int r = 0; r < param.jointMSA.Rows; r++)
			{
				for(int c = 0; c < param.jointMSA.Cols; c++)
				{
					int x = (c % param.cellsPerRow) * param.cellWidth;
					int gr = (c / param.cellsPerRow);
					int y = gr * (groupHeight + spacerHeight) + r * param.cellHeight;
					
					g.DrawRectangle(boxPen, new Rectangle(x, y, param.cellWidth, param.cellHeight));
					
					//#DEBUG
					/*
					//Draw the character occuring at this point, even if non-conserved
					int subICol;
					if( !maps[r].TryGetValue(c, out subICol) )
						continue;	//Continue if not mapping from jointColumn to a column in the i'th sub-MSA
					
					StringFormat format = new StringFormat();
					format.Alignment = StringAlignment.Center;
					format.LineAlignment = StringAlignment.Center;
					
					char glyphI = mostCommonGlyphs[r][subICol];
					g.DrawString(glyphI.ToString(), new Font("Courier",maxSize), Brushes.Black, new RectangleF(x,y,param.cellWidth, param.cellHeight), format);
					*/
					//#ENDDEBUG
				}
			}
			boxPen.Dispose();
			
			g.Flush();
			g.Dispose();
			
			result.Save(args[1]);

		}
		
		public static int maxFontSize(ConfigParams param, List<Dictionary<int, int>> maps, List<List<double>> entropies, List<List<char>> mostCommonGlyphs, Graphics g)
		{
			int maxSize = Math.Max(param.cellHeight, param.cellWidth);	//Minimizing the maximum
			for(int jointCol = 0; jointCol < param.jointMSA.Cols; jointCol++)
			{
				for(int i = 0; i < param.jointMSA.Rows; i++)
				{
					//Get the i'th sub-MSA column
					int subICol;
					if( !maps[i].TryGetValue(jointCol, out subICol) )
						continue;	//Continue if not mapping from jointColumn to a column in the i'th sub-MSA
					
					//If the sub-MSA column (joint MSA cell) is not conserved, continue
					if( entropies[i][subICol] > param.entropyCutoff )
						continue;
					
					//Otherwise, this cell is conserved in the joint MSA
					char glyphI = mostCommonGlyphs[i][subICol];
					while( true )
					{
						SizeF charSize = g.MeasureString(glyphI.ToString(),
						                                 new Font("Courier",maxSize),
						                                 new SizeF(param.cellWidth, param.cellHeight));
						
						if( charSize.Width > param.cellWidth || charSize.Height > .9 * param.cellHeight )
							maxSize--;
						else
							break;
					}
				}
			}
			
			return maxSize;
		}
		
		//Reads the input parameter file lines, stripping comments, leading/trailing whitespace and
		// blank lines
		public static List<string> readCanonInput(string path)
		{
			List<string> rawLines = new List<string>(System.IO.File.ReadLines(path));
			
			List<string> lines = new List<string>();
			//Canonicalize the lines
			for(int i = 0; i < rawLines.Count; i++)
			{
				string canon = canonicalizeInputLine(rawLines[i]);
				if( !string.IsNullOrWhiteSpace(canon) )
					lines.Add(canon);
			}
			
			return lines;
		}
		
		//Strips comments and leading/trailing white space from an individual line
		public static string canonicalizeInputLine(string line)
		{
			int commentIndex = line.IndexOf('#');
			if( commentIndex >= 0 )
				line = line.Substring(0, commentIndex);
			
			return line.Trim();
		}
	
		//Reads and interprets the configuration parameter file
		public static ConfigParams readConfig(string path)
		{
			List<string> lines = readCanonInput(path);
		
			ConfigParams param = new ConfigParams();
			
			param.jointMSA = new MSA(lines[0]);
			param.cellWidth = Int32.Parse(lines[1]);
			param.cellHeight = Int32.Parse(lines[2]);
			param.cellsPerRow = Int32.Parse(lines[3]);
			param.entropyCutoff = Double.Parse(lines[4]);
			param.simMat = new SimilarityMatrix(lines[5]);
			
			//Read the Sub-MSA descriptor lines
			param.subMSAs = new List<MSA>();
			param.jointMSANames = new List<string>();
			param.subMSANames = new List<string>();
			
			for(int i = 6; i < lines.Count; i++)
			{
				string[] fields = lines[i].Split('\t');
				if( fields.Length != 3 )
					throw new ArgumentException("Expected exactly three arguments on sub MSA line in the parameter file");
				
				string subMSAPath = fields[0];
				string subMSAName = fields[1];
				string jointMSAName = fields[2];
				
				param.subMSAs.Add(new MSA(subMSAPath));
				param.subMSANames.Add(subMSAName);
				param.jointMSANames.Add(jointMSAName);
			}
			
			return param;
			
		}
		
		//Calcualtes entropy for every column in an MSA
		public static List<double> entropy(MSA msa)
		{
			List<double> result = new List<double>();
			for(int c = 0; c < msa.Cols; c++)
				result.Add(entropy(msa, c));
			
			return result;
		}
		
		//Calculates entropy for a specific column in an MSA
		public static double entropy(MSA msa, int column)
		{			
			Dictionary<char, int> glyphCounts = getGlyphCounts(msa, column);
			
			double norm = 1.0 / (double)msa.Rows;
			
			//Calculate the entropy
			double result = 0;
			foreach(char glyph in glyphCounts.Keys)
			{
				double freq = glyphCounts[glyph] * norm;
				result -= freq * Math.Log(freq);
			}
			
			return result;
		}
		
		//Returns the most common amino acid in each column of an MSA
		// In the case of a tie, the choice returned is arbitrary
		// (But since we are using this for highly conserved positions,
		// this detail won't matter)
		public static List<char> mostCommon(MSA msa)
		{
			List<char> result = new List<char>();
			for(int c = 0; c < msa.Cols; c++)
			{
				result.Add(mostCommon(msa, c)[0]);
			}
			
			return result;
		}
		
		//Calculates the most commonly occuring amino acids in a column
		public static List<char> mostCommon(MSA msa, int column)
		{
			Dictionary<char, int> glyphCounts = getGlyphCounts(msa, column);
			int maxOccurance = Int32.MinValue;
			List<char> result = new List<char>();
			
			foreach(char glyph in glyphCounts.Keys)
			{
				int occCount = glyphCounts[glyph];
				
				if( occCount > maxOccurance )
				{
					result.Clear();
					maxOccurance = occCount;
				}
				
				if(occCount == maxOccurance )
					result.Add(glyph);
					
			}
			
			return result;
		}
		
		//Extracts statistics for a column in the MSA
		public static Dictionary<char, int> getGlyphCounts(MSA msa, int column)
		{
			//Build up statistics on how often letters occur in an MSA column
			Dictionary<char, int> glyphCounts = new Dictionary<char, int>();
			for(int r = 0; r < msa.Rows; r++)
			{
				char glyph = msa[r,column];
				
				int curCount;
				if( !glyphCounts.TryGetValue(glyph, out curCount) )
					glyphCounts.Add(glyph, 1);
				else
					glyphCounts[glyph] = curCount + 1;
			}
			
			return glyphCounts;
		}
		
		//Returns a dictionary that maps the 0-based MSA column of X to a 0-based MSA column of Y
		public static Dictionary<int, int> getMapping(MSA x, MSA y, string nameX, string nameY)
		{
			int rX = getFirstRowWithName(x, nameX);
			int rY = getFirstRowWithName(y, nameY);
			
			//Check that these sequences are identical
			if( x.extractUngappedSequence(rX) != y.extractUngappedSequence(rY) )
				throw new ArgumentException("Ungapped sequences are not equivalent");
			
			List<int> colsX = getUngapedColumnIndicesOnRow(x, rX);
			List<int> colsY = getUngapedColumnIndicesOnRow(y, rY);
			
			if( colsX.Count != colsY.Count )
				throw new ArgumentException("number of columns do not match exactly");
			
			//Construct the dictionary
			Dictionary<int, int> result = new Dictionary<int, int>();
			for(int i = 0; i < colsX.Count; i++)
				result.Add(colsX[i], colsY[i]);
			
			return result;
		}
		
		//Returns a new dictionary with the inverse mapping (input value -> input key)
		public static Dictionary<int, int> invertDictionary(Dictionary<int, int> inputDict)
		{
			Dictionary<int, int> result = new Dictionary<int, int>();
			foreach(int key in inputDict.Keys)
				result.Add(inputDict[key], key);
			
			return result;
		}
		
		public static List<int> getUngapedColumnIndicesOnRow(MSA msa, int row)
		{
			List<int> result = new List<int>();
			for(int c = 0; c < msa.Cols; c++)
			{
				if( msa[row,c] != MSA.GAP )
					result.Add(c);
			}
			
			return result;
		}
		
		public static int getFirstRowWithName(MSA msa, string name)
		{
			for(int i = 0; i < msa.Rows; i++)
				if( msa.Names[i] == name )
					return i;
			
			throw new ArgumentException("Name not found in MSA");
		}
	}
}