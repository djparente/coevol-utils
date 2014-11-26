/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-10-23
 * Time: 14:00
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace CompileMasterTable
{
	class Program
	{
		public static void Main(string[] args)
		{
			//Read all the data in
			List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();
			
			for(int i = 0; i < args.Length; i++)
				data.Add(readDatafile(args[i]));
			
			//Determine all keys ever seen
			Dictionary<string, string> allKeys = new Dictionary<string, string>();
			for(int i = 0; i < data.Count; i++)
			{
				foreach(string key in data[i].Keys)
					allKeys[key] = key;
			}
			
			//Write out the data
			List<string> outLines = new List<string>();
			foreach(string key in allKeys.Keys)
			{
				StringBuilder b = new StringBuilder();
				b.Append(key);
				
				string valstr;
				foreach(Dictionary<string, string> dataset in data)
				{
					b.Append("\t");
					if( dataset.TryGetValue(key, out valstr) )
						b.Append(valstr);
				}
				
				outLines.Add(b.ToString());
			}
			
			//Sort the data by number (plus insertion code... which complicates things, as you can see:)
			int parseVal;
			IEnumerable<string> orderedOut = outLines.OrderBy(x => 	{ 
										string fieldZero=x.Split('\t')[0]; 
										return Int32.TryParse(fieldZero, out parseVal) ? parseVal : Int32.Parse(fieldZero.Substring(0, fieldZero.Length -1)); 
				   					}
					).ThenBy(x => x);
			
			
			//Write the header
			StringBuilder header = new StringBuilder();
			header.Append("key");
			for(int i = 0; i < args.Length; i++)
			{
				header.Append("\t");
				header.Append(args[i]);
			}
			Console.WriteLine(header.ToString());
			
			//Write the datatable
			foreach(string os in orderedOut)
				Console.WriteLine(os);
		}
		
		public static Dictionary<string, string> readDatafile(string path)
		{
			string[] lines = File.ReadAllLines(path);
			Dictionary<string, string> result = new Dictionary<string, string>();
			
			for(int i = 0; i < lines.Length; i++)
			{
				string[] fields = lines[i].Split('\t');
				if( fields.Length != 2 )
					throw new ArgumentException("Expected, but did not find, exactly 2 fields on line " + (i+1) + " of file " + path);
				
				result.Add(fields[0], fields[1]);
			}
			
			return result;
		}
	}
}