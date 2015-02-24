/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-10-30
 * Time: 11:01
 * 
 * Reads in a set of co-evolution networks, matches equivalent (pos1,pos2) tuples
 * and writes summary statistics across the network for those positions
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace AverageNetwork
{
	class Program
	{
		const string PROGRAM_NAME = "AverageNetwork";
		
		public static void Main(string[] args)
		{
			List<Dictionary<string, Dictionary<string, double>>> networks = new List<Dictionary<string, Dictionary<string, double>>>();
			
			foreach(string path in args)
				networks.Add(readNetwork(path));
			
			Dictionary<string, byte> allKeys = new Dictionary<string, byte>();
			foreach(Dictionary<string, Dictionary<string, double>> network in networks)
			{
				foreach(string netKey in network.Keys)
				{
					allKeys[netKey] = 1;
					Dictionary<string, double> row = network[netKey];
					foreach(string subnetkey in row.Keys )
						allKeys[subnetkey] = 1;
				}
			}
			
			List<string> allKeyList = new List<string>(allKeys.Keys);
			
			List<string> keyOrder = new List<string>(orderByNumberPlusICode(allKeyList));
			
			double dat;
			Dictionary<string, double> intermed;
			for(int i = 0; i < keyOrder.Count; i++)
			{
				string keyI = keyOrder[i];
				for(int j = i+1; j < keyOrder.Count; j++)
				{
					string keyJ = keyOrder[j];
					//Calculate statistics by two-pass (for numerical stability)
					
					double average = 0;
					double n = 0;
					
					//Average loop
					for(int k = 0; k < networks.Count; k++)
					{
						if( networks[k].TryGetValue(keyI, out intermed) && intermed.TryGetValue(keyJ, out dat) )
						{
							n++;
							average += dat;
						}
					}
					
					average /= n;
					
					//Stdev loop
					double stdev = 0;
					for(int k = 0; k < networks.Count; k++)
					{
						if( networks[k].TryGetValue(keyI, out intermed) && intermed.TryGetValue(keyJ, out dat) )
						{
							double diff = (dat - average);
							stdev += diff*diff;
						}
					}
					
					stdev = Math.Sqrt(stdev / (n-1));
					
					//Write the data out
					Console.WriteLine("{0}\t{1}\t{2}\t{3}\t{4}", keyI, keyJ, average, stdev, n);
				}
			}
			
			
		}
		
		
		static IEnumerable<string> orderByNumberPlusICode(List<string> resnums)
		{
				//Sort the data by number (plus insertion code... which complicates things, as you can see:)
				int parseVal;
				IEnumerable<string> orderedResult = resnums.OrderBy(x => 	{ 
													return Int32.TryParse(x, out parseVal) ? parseVal : Int32.Parse(x.Substring(0, x.Length -1)); 
					   					}
										).ThenBy(x => x);
				
				return orderedResult;
		}
		
		static Dictionary<string, Dictionary<string, double>> readNetwork(string path)
		{
			Dictionary<string, Dictionary<string, double>> result = new Dictionary<string, Dictionary<string, double>>();
			
			IEnumerable<string> lines = File.ReadLines(path);
		
			int linesRead = 0;
			foreach(string line in lines)
			{
				linesRead++;
				
				//Ignore empty lines
				if( string.IsNullOrWhiteSpace(line) )
					continue;
				
				//Split into three fields
				string[] fields = line.Split('\t');
				if( fields.Length != 3 )
					errorOut("Expected 3 fields on line " + linesRead + " but found " + fields.Length + " in file " + path, 254);
				
				double data;
				if( !double.TryParse(fields[2], out data) )
					errorOut("Non-numeric data value found in field 3 on line " + linesRead + " in file " + path, 253);
				
				if( !result.ContainsKey(fields[0]) )
					result.Add(fields[0], new Dictionary<string, double>());
				
				if( !result.ContainsKey(fields[1]) )
					result.Add(fields[1], new Dictionary<string, double>());
				
				result[fields[0]][fields[1]] = data;
				result[fields[1]][fields[0]] = data;
			}
			
			return result;
		}
		
		public static void errorOut(string message, int errorCode)
		{
			Console.Error.WriteLine("[{0}] Error: {1}", PROGRAM_NAME, message);
			Console.Error.WriteLine("[{0}] Terminating with error code {1}", PROGRAM_NAME, errorCode);
			Environment.Exit(errorCode);
		}
	}
	
	
}