/*
 * Daniel J Parente
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * Date: 12/16/2011
 * Time: 10:49 AM
 * 
 */
using System;
using System.IO;
using System.Collections.Generic;

namespace DJPCommonBioinfo
{
	/// <summary>
	/// Description of SimilarityMatrix.
	/// </summary>
	public class SimilarityMatrix
	{
		private Dictionary<char, Dictionary<char, int>> data;
		private int min = Int32.MaxValue;
		private int max = Int32.MinValue;
		
		
		
		public int this[char residue1, char residue2]
		{
			get { return this.data[residue1][residue2]; }
		}
		
		public int Min { get { return this.min; } }
		public int Max { get { return this.max; } }
		
		public SimilarityMatrix(string path)
		{
			char[] delim = new char[] { ' ' };
			
			string[] lines = File.ReadAllLines(path);
			
			string[] symbols = lines[0].Split(delim, StringSplitOptions.RemoveEmptyEntries);
			
			//Parse each line
			data = new Dictionary<char, Dictionary<char, int>>(symbols.Length);
			for(int i = 1; i < lines.Length; i++)
			{
				string[] lineSplit = lines[i].Split(delim, StringSplitOptions.RemoveEmptyEntries);
				data.Add(symbols[i-1][0], new Dictionary<char, int>(symbols.Length));
				
				
				for(int j = 1; j < lineSplit.Length; j++)
				{
					int val = int.Parse(lineSplit[j]);
					if( val < min ) 
						min = val;
					if( val > max ) 
						max = val;
										
					data[symbols[i-1][0]].Add(symbols[j-1][0], val);
				}
			}
		}
		
		public bool tryGetScore(char res1, char res2, out int result)
		{
			Dictionary<char, int> mid;
			result = Int32.MinValue;
			
			if ( !this.data.TryGetValue(res1, out mid) )
				return false;
			
			if( !mid.TryGetValue(res2, out result) )
				return false;
			
			return true;
		}
	}
}
