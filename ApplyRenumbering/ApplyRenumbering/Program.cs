/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-10-29
 * Time: 14:59
 * 
 * 
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace ApplyRenumbering
{
	class Program
	{
		public static void Main(string[] args)
		{
			if( args.Length != 2 )
			{
				usage();
				Environment.Exit(255);
			}
			
			//Read the dictionary
			string[] dictLines = File.ReadAllLines(args[0]);
			Dictionary<string,string> convDict =new Dictionary<string, string>();
			for(int i = 0; i < dictLines.Length; i++)
			{
				string[] fields = dictLines[i].Split('\t');
				if( fields.Length != 2 )
					errorOut("Invalid dictionary file; expected two fields on line " + (i+1) + " but found " + fields.Length,254);
				
				convDict.Add(fields[0], fields[1]);
			}
			
			int columnToAlter = Int32.Parse(args[1]);
			
			string input = Console.In.ReadToEnd();
			input.Replace("\r\n", "\n");
			string[] linesIn = input.Split('\n');
			
			int firstFieldCount = -1;
			for(int i = 0; i < linesIn.Length; i++)
			{
				//Ignore empty lines
				if( string.IsNullOrWhiteSpace(linesIn[i]) )
					continue;
				
				string[] fields = linesIn[i].Split('\t');
				
				//Check to make sure each line has the same number of fields
				if( i == 0 )
					firstFieldCount = fields.Length;
				if( fields.Length != firstFieldCount )
					errorOut("Invalid input file: Line " + (i+1) + " contained " + fields.Length + " fields, but all previous lines contained " + firstFieldCount,253);
				
				//Check to make sure we have enough fields to alter at least one
				if( fields.Length < columnToAlter)
					errorOut("Invalid input file: Command was to alter column " + (columnToAlter) + ", but line " + (i+1) + " contains only " + fields.Length + " fields",252);
				
				System.Text.StringBuilder b = new System.Text.StringBuilder();
				bool outputThisLine = true;
				for(int f = 0; f < fields.Length; f++)
				{
					if( f != 0 )
						b.Append("\t");
					
					if( f + 1 == columnToAlter )
					{	
						string converted;
						outputThisLine = convDict.TryGetValue(fields[f], out converted);
						if( outputThisLine )
							b.Append(converted);
					}
					else
						b.Append(fields[f]);
				}
				if( outputThisLine )
					Console.WriteLine(b.ToString());
			}
		}
		
		public static void errorOut(string message, int errorCode)
		{
			Console.Error.WriteLine("Error: " + message);
			Console.Error.WriteLine("Terminating abnormally");
			Environment.Exit(errorCode);
		}
		
		public static void usage()
		{
			Console.Error.WriteLine("Usage: ./ApplyRenumbering.exe [Dictionary file] [Column to alter]");
		}
	}
}