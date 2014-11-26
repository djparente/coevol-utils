/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-07-23
 * Time: 16:08
 * 
 * 
 */
using DJPCommonBioinfo;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ZNMI
{
	public class ZNMIAlg
	{
		const double NO_DATA = double.MinValue;
		const double ZERO_THRESHOLD = .0000000001;
		
		public static void simpleMain(string[] args)
		{
			if( args.Length != 2 )
				errorOut("Usage: ./ZNMI.exe [MSA Path] [Output Path]", 255);
			
			string msaPath = args[0];
			string outpath = args[1];
			
			//Sanity check existance (though the real test is in loading)
			if( !System.IO.File.Exists(msaPath) )
				errorOut("Error: The MSA does not seem to exist", 254);
			
			//Refuse to clobber an existing output file
			if( System.IO.File.Exists(outpath) )
				errorOut("Error: Output file already exists; refusing to clobber", 200);
			
			FileInfo fInfo = new FileInfo(msaPath);
			string filename = fInfo.Name;
			
			MSA msa = null;
			
			try
			{
				msa = new MSA(msaPath);						//Fasta format
				//msa = MSA.constructFromFodor(msaPath);	//Fodor format
			}
			catch(Exception ex)
			{
				errorOut("This was a problem loading the MSA.  Exception text was: " + ex.Message, 253);
			}
			
			const double minEntropy = 0.198515243;
			const double maxGaps = 0.1;
			const bool ignoreGaps = true;
			const bool suppressNegative = true;
			
			double[] e = entropy(msa, ignoreGaps);
			double[,] je = jointEntropy(msa, ignoreGaps);
			double[] gaps = countPctGaps(msa);
			
			double[,] znmiValues = znmi(e, je, gaps, minEntropy, maxGaps, suppressNegative);
			//double[,] nmiValues = nmi(e, je, suppressNegative);
			
			//printArray(znmiValues);//, nmiValues, e, je, 0.198515243, gaps);
			//rasterArray(znmiValues);
			
			//ZNMI in XYZ output format
			// residue1 residue2 coevol_score
			string xyzData = getXYZFormat(znmiValues);
			//Console.WriteLine(xyzData);
			
			//Write out in gzipped format
			byte[] outBytes = System.Text.Encoding.UTF8.GetBytes(xyzData);
			FileStream fs = new FileStream(outpath, FileMode.Create);
			GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
			gz.Write(outBytes, 0, outBytes.Length);
			gz.Close();
			fs.Close();
		}
		
		public static void Main(string[] args)
		{			
			simpleMain(args);
			return;
			
			if( args.Length != 5 )
				errorOut("Usage: ./ZNMI.exe [MSA Path] [ExpNID] [SigNid] [SubSize] [SubSerial]", 255);
			
			string msaPath = args[0];
			
			//Sanity check existance (though the real test is in loading)
			if( !System.IO.File.Exists(msaPath) )
				errorOut("Error: The MSA does not seem to exist", 254);
			
			FileInfo fInfo = new FileInfo(msaPath);
			string filename = fInfo.Name;
			
			MSA msa = null;
			
			try
			{
				msa = new MSA(msaPath);						//Fasta format
				//msa = MSA.constructFromFodor(msaPath);	//Fodor format
			}
			catch(Exception ex)
			{
				errorOut("This was a problem loading the MSA.  Exception text was: " + ex.Message, 253);
			}
			
			int expnid = Int32.Parse(args[1]);
			int signid = Int32.Parse(args[2]);
			int subsize = Int32.Parse(args[3]);
			int subserial = Int32.Parse(args[4]);
			string outpath = filename.Replace(".fodor", "") + ".znmi";
			
			const double minEntropy = 0.198515243;
			const double maxGaps = 0.1;
			const bool ignoreGaps = true;
			const bool suppressNegative = true;
			
			double[] e = entropy(msa, ignoreGaps);
			double[,] je = jointEntropy(msa, ignoreGaps);
			double[] gaps = countPctGaps(msa);
			
			double[,] znmiValues = znmi(e, je, gaps, minEntropy, maxGaps, suppressNegative);
			//double[,] nmiValues = nmi(e, je, suppressNegative);
			
			//printArray(znmiValues);//, nmiValues, e, je, 0.198515243, gaps);
			//rasterArray(znmiValues);
			
			//ZNMI in XYZ output format
			// residue1 residue2 coevol_score
			string xyzData = getXYZFormat(znmiValues);
			//Console.WriteLine(xyzData);
			
			//Write out in gzipped format
			byte[] outBytes = System.Text.Encoding.UTF8.GetBytes(xyzData);
			FileStream fs = new FileStream("result/" + outpath + ".gz", FileMode.Create);
			GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
			gz.Write(outBytes, 0, outBytes.Length);
			gz.Close();
			fs.Close();
			
			//Calculate the results MD5sum
			MemoryStream ms = new MemoryStream(outBytes);
			System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
			byte[] md5HashBytes = md5.ComputeHash(ms);
			ms.Close();
			
			System.Text.StringBuilder md5Builder = new System.Text.StringBuilder(32);
			for(int i = 0; i < md5HashBytes.Length; i++)
				md5Builder.Append(md5HashBytes[i].ToString("x2"));
			string md5sum = md5Builder.ToString();
			md5.Dispose();
			
			Console.WriteLine(getMetaString(expnid, signid, subsize, subserial, "znmi", outpath, md5sum));
			
			List<string> sql = getSqlBatches(znmiValues, expnid);
			for(int i = 0; i < sql.Count; i++)
			{
				File.WriteAllText("sql/" + outpath + "_" + i.ToString().PadLeft(4, '0') + ".sql", sql[i]);
			}
		}
		
		public static void errorOut(string message, int errorCode)
		{
			Console.Error.WriteLine(message);
			Environment.Exit(errorCode);
		}
		
		/****************************************************************************
		  Methods handling display of data and upload to Parente coevol_data database
		 ****************************************************************************/
		public static string getXYZFormat(double[,] data)
		{
			int noCount = 0;
			int printCount =0;
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			result.AppendLine("i\tj\tscore");	//Header for compatibility with fodor format
			for(int r = 0; r < data.GetLength(0); r++)
			{
				for(int c = r+1; c < data.GetLength(1); c++)
				{
					if( data[r,c] != NO_DATA )
					{
						result.AppendLine(string.Format("{0}\t{1}\t{2}", r, c, data[r,c]));
						printCount++;
					}
					else
						noCount++;
				}
			}
						Console.Error.WriteLine("Notice: Printed {0} lines while suppressing {1} lines of no_data", printCount, noCount);
			
			return result.ToString();
		}
		
		public static string getXYZFormat(double[,] znmi, double[,] nmi, double[] e, double[,] je, double minEntropy, double[] gaps)
		{
			int noCount = 0;
			int printCount =0;
			System.Text.StringBuilder result = new System.Text.StringBuilder();
			for(int r = 0; r < znmi.GetLength(0); r++)
			{
				for(int c = r+1; c < znmi.GetLength(1); c++)
				{
					if( znmi[r,c] != NO_DATA )// && e[r] > 0.198515243 && e[c] > 0.198515243 )
					{
						//result.AppendLine(string.Format("{0}\t{1}\t{2}", r, c, znmi[r,c]));
						result.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", r, c, nmi[r,c], znmi[r,c], e[r], e[c], je[r,c], gaps[r],gaps[c]));
						printCount++;
					}
					else
						noCount++;
				}
			}
			
			Console.Error.WriteLine("Notice: Printed {0} lines while suppressing {1} lines of no_data", printCount, noCount);
			
			return result.ToString();
			
		}
		
		public static void rasterArray(double[,] data)
		{
			//System.Text.StringBuilder b = new System.Text.StringBuilder();
			for(int r = 0; r < data.GetLength(0); r++)
			{
				for(int c = 0; c < data.GetLength(1); c++)
				{
					string strOut = "";
					
					if( data[r,c] == NO_DATA )
					{
						strOut = "             null ";
					}
					else if(r == c)
					{
						strOut = "               id ";
					}
					else
					{
						strOut = data[r,c].ToString(" 0.000000000E+000 ;-0.000000000E+000 ");
					}
					
					Console.Write(strOut);
				}
				Console.WriteLine();
			}

		}
		
		public static string getMetaString(int expnid, int signid, int subsize, int subserial, string algorithm, string filename, string md5sum)
		{

			return string.Format("insert into coevol_meta values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','','');", expnid, signid, subsize, subserial, algorithm, filename, md5sum);
		}
		
		public static List<string> getSqlBatches(double[,] data, int expnid)
		{
			const int BATCH_SIZE = 5000;
			
			List<string> tuples = new List<string>();
			for(int r = 0; r < data.GetLength(0); r++)
			{
				for(int c = r+1; c < data.GetLength(1); c++)
				{
					if( data[r,c] != NO_DATA )
						tuples.Add(string.Format("('{0}','{1}','{2}','{3}')", expnid, r, c, data[r,c]));
				}
			}
			
			//Now write the tuple SQL batches
			List<string> results = new List<string>();
			int batchPtr = 0;
			while( batchPtr < tuples.Count )
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				sb.Append("insert into coevol_data values ");
				for(int i = 0; i < BATCH_SIZE && i + batchPtr < tuples.Count; i++)
				{
					sb.Append(tuples[batchPtr+i]);
					if( i != BATCH_SIZE-1 && i+batchPtr != tuples.Count - 1 )
						sb.Append(",");
				}
				batchPtr += BATCH_SIZE;
				
				sb.Append(";");
				results.Add(sb.ToString());
			}
			
			return results;
		}
				
		/****************************************************************************
		  Methods handling MI, NMI and ZNMI calculations and math helper functions
		 ****************************************************************************/
		
		public static double[] entropy(MSA msa, bool ignoreGaps)
		{
			double[] result = new double[msa.Cols];
			
			for(int c = 0; c < msa.Cols; c++)
			{
				//Keep track of all the glyphs seen
				Dictionary<char, int> glyphs = new Dictionary<char, int>();
				int totalGlyphs = 0;
				
				for(int r = 0; r < msa.Rows; r++)
				{
					char newGlyph = msa[r,c];
					if( ignoreGaps && newGlyph == '-')
						continue;	//Ignore gaps for consistent with Brown & Brown
					
					int curGlyphCount;
					if( glyphs.TryGetValue(newGlyph, out curGlyphCount) )
						glyphs[newGlyph]++;
					else
						glyphs[newGlyph] = 1;
					
					totalGlyphs++;
				}
				
				double totalNorm = 1.0 / (double)totalGlyphs;
				
				double curResult = 0;
				foreach(int gCount in glyphs.Values)
				{
					double prob = (double)gCount * totalNorm;
					curResult -= prob*Math.Log(prob);
				}
				
				//Save result
				result[c] = curResult;
			}
			
			return result;
		}
		
		public static double[] countPctGaps(MSA msa)
		{
			double[] result = new double[msa.Cols];
			
			for(int c = 0; c < msa.Cols; c++)
			{
				int count = 0;
				for(int r = 0; r < msa.Rows; r++)
					if( msa[r,c] == '-' )
						count++;
				
				result[c] = (double)count / (double)msa.Rows;
			}
			
			return result;
		}
		
		public static double[,] jointEntropy(MSA msa, bool ignoreGaps)
		{			
			double[,] result= new double[msa.Cols, msa.Cols];
			
			for(int c1 = 0; c1 < msa.Cols; c1++)
			{
				for(int c2 = c1+1; c2 < msa.Cols; c2++)
				{
					//Keep track of all the glyphs seen
					Dictionary<string, int> glyphs = new Dictionary<string, int>();
					int totalGlyphs = 0;
					
					
					for(int r = 0; r < msa.Rows; r++)
					{
						if( ignoreGaps && (msa[r,c1] == '-' || msa[r,c2] == '-') )
							continue;	//Ignore gaps for consistent with Brown & Brown
						
						string newGlyph = msa[r,c1].ToString() + msa[r,c2].ToString();
						int curGlyphCount;
						if( glyphs.TryGetValue(newGlyph, out curGlyphCount) )
							glyphs[newGlyph]++;
						else
							glyphs[newGlyph] = 1;
						
						totalGlyphs++;
					}
					
					double totalNorm = 1.0 / (double)totalGlyphs;
					
					double curResult = 0;
					foreach(int gCount in glyphs.Values)
					{
						double prob = (double)gCount * totalNorm;
						curResult -= prob*Math.Log(prob);
					}
					
					//Save symmetrically
					result[c1,c2] = curResult;
					result[c2,c1] = curResult;
				}
			}
			
			return result;
		}
		
		public static double[,] mi(MSA msa, bool ignoreGaps, bool suppressNegative)
		{
			double[,] result = new double[msa.Cols, msa.Cols];
			double[] e = entropy(msa, ignoreGaps);
			double[,] je = jointEntropy(msa, ignoreGaps);
			
			return mi(e, je, suppressNegative);
		}
		
		//Computes the mutual information given the entropy and the joint entropy
		//
		// If gaps were ignored using the Brown & Brown method, it is possible
		// for MI scores to be negative.  For consistently with their implementation
		// these scores can be filted out with the suppress
		public static double[,] mi(double[] e, double[,] je, bool suppressNegative)
		{
			//Consistency check
			if( e.Length != je.GetLength(0) )
				throw new ArgumentException("Entropy and Joint Entropy Dimension Mismatch");
					
			if( je.GetLength(0) != je.GetLength(1))
				throw new ArgumentException("Joint entropy matrix is not square");
			
			int cols = je.GetLength(0);
			
			double[,] result = new double[cols, cols];
			//Calculate the mutual information
			for(int c1 = 0; c1 < cols; c1++)
			{
				for(int c2 = c1+1; c2 < cols; c2++)
				{
					double res = e[c1] + e[c2] - je[c1,c2];
					if( suppressNegative && res < 0 )
						res = NO_DATA;
					
					result[c1, c2] = res;
					result[c2, c1] = res;
				}
			}
			
			return result;
		}
		
		public static double[,] nmi(MSA msa, bool ignoreGaps, bool suppressNegative)
		{			
			double[,] result = new double[msa.Cols, msa.Cols];
			double[] e = entropy(msa, ignoreGaps);
			double[,] je = jointEntropy(msa, ignoreGaps);
			
			return nmi(e, je, suppressNegative);
		}
		
		public static double[,] nmi(double[] e, double[,] je, bool suppressNegative)
		{
			//Consistency check
			if( e.Length != je.GetLength(0) )
				throw new ArgumentException("Entropy and Joint Entropy Dimension Mismatch");
					
			if( je.GetLength(0) != je.GetLength(1))
				throw new ArgumentException("Joint entropy matrix is not square");
			
			int cols = je.GetLength(0);
			
			double[,] result = new double[cols, cols];
			//Calculate the mutual information
			for(int c1 = 0; c1 < cols; c1++)
			{
				for(int c2 = c1+1; c2 < cols; c2++)
				{
					double res;
					
					//Don't divide by zero, if je = 0
					if(je[c1, c2] > ZERO_THRESHOLD )
						res = (e[c1] + e[c2] - je[c1,c2])/je[c1,c2];
					else
						res = NO_DATA;
					
					//Optionally suppress negative NMI scores (due to Brown & Brown approximation)
					if( suppressNegative && res < 0 )
						res = NO_DATA;
					
					result[c1, c2] = res;
					result[c2, c1] = res;
				}
			}
			
			return result;
			
		}
		
		public static double[,] znmi(MSA msa, double minEntropy, double maxGap, bool ignoreGaps, bool suppressNegativeNMI)
		{
			//Based on Brown and Brown PLoS 2010
			double[] e = entropy(msa, ignoreGaps);
			double[,] je = jointEntropy(msa, ignoreGaps);
			double[] gapPct = countPctGaps(msa);
			return znmi(e, je, gapPct, minEntropy, maxGap, suppressNegativeNMI);
		}
		
		public static double[,] znmi(double[] e, double[,] je, double[] gapPct, double minEntropy, double maxGap, bool suppressNegativeNMI)
		{
			double[,] result = nmi(e, je, suppressNegativeNMI);
			int cols = je.GetLength(0);
			
			//Set values to null where gap or entropy criterion fails
			for(int c = 0; c< cols; c++)
			{
				if( e[c] < minEntropy || gapPct[c] >= maxGap )
				{
					for(int r = 0; r < cols; r++)
					{
						if( r == c )
							continue;
						
						result[r,c] = NO_DATA;
						result[c,r] = NO_DATA;
					}
				}
			}
			
			//Calculate columnwise mean and var
			double[] mean = new double[e.Length];
			double[] vari = new double[e.Length];
			
			//Pre-calculate the distribution parameters
			
			for(int c = 0; c < cols; c++)
			{
				mean[c] = columnMean(result, c);
				vari[c] = columnVar(result, mean[c], c);
			}
			
			for(int c1 = 0; c1 < cols; c1++)
			{
				for(int c2 = c1+1; c2 < cols; c2++)
				{					
					double zmean = (mean[c1]*vari[c2]+mean[c2]*vari[c1])/(vari[c1]+vari[c2]);
					double zstde = Math.Sqrt((vari[c1]*vari[c2])/(vari[c1]+vari[c2]));
					
					//Divide by zero will occur here for conserved columns 
					//(they always obtain an NMI of 0 [if foreign column not 100% conserved] 
					//or NO_DATA [if foreign column conserved]					
					if( result[c1, c2] != NO_DATA && zstde > ZERO_THRESHOLD)
					{
						result[c1,c2] = (result[c1,c2] - zmean)/zstde;
						result[c2,c1] = result[c1,c2];
						
						//Console.WriteLine("{0}\t{1}\t{2:F11}\t{3:F11}\t{4:F11}\t{5:F11}\t{6:F11}\t{7:F11}\t{8:F11}", c1, c2, nmidata[c1,c2], result[c1,c2], e[c1], e[c2], je[c1,c2], zmean, zstde);
					}
					else
						result[c1,c2] = NO_DATA;
				}
			}
			
			return result;
		}
		
		public static double columnMean(double[,] data, int column)
		{
			double result = 0;
			int count = 0;
			
			for(int r = 0; r < data.GetLength(0); r++)
			{
				if( r == column )
					continue;
				
				if( data[r,column] == NO_DATA )
					continue;
				
				result += data[r,column];
				count++;
			}
			
			result /= count++;
			
			return result;
		}
		
		//Require the mean to be known before stdev calculation for better
		// numerical properties
		public static double columnVar(double[,] data, double mean, int column)
		{
			double result = 0;
			int count = 0;
			for(int r = 0; r < data.GetLength(0); r++)
			{
				if( r == column )
					continue;
				
				if( data[r,column] == NO_DATA )
					continue;
				
				double delta = data[r,column] - mean;
				
				result += delta*delta;
				count++;
			}
			
			//Minus 1 because it's sample variance
			//result /= (count - 1);
			
			//For compatibility with Brown & Brown reference implementation
			result /= count;
			
			return result;
		}
				
		//Sets the accumulator array (acc) to acc[i,j] = acc[i,j] + toAdd[i,j]
		public static void naiveAdd2DArrays(double[,] acc, double[,] toAdd)
		{
			int size0 = acc.GetLength(0);
			int size1 = acc.GetLength(1);
			
			if( toAdd.GetLength(0) != size0 || toAdd.GetLength(1) != size1 )
				throw new ArgumentException("Arrays not of equal dimension");
			
			//Otherwise add
			for(int i = 0; i < size0; i ++)
				for(int j = 0; j < size1; j++)
					acc[i,j] += toAdd[i,j];
		}
	}
}