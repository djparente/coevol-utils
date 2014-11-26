/*
 * Daniel J. Parente
 * dparente@kumc.edu
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 * Date: 6/7/2011
 * Time: 5:31 PM
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace IdentifyBasisVector
{
	class Program
	{
		public static void Main(string[] args)
		{
			List<string> fileLines = readlines(args[0]);
			//List<string> fileLines = readlines(@"C:\Users\dparente\Documents\Example.csv");
			
			string[] names = new string[fileLines.Count];
			double[,] id = new double[fileLines.Count, fileLines.Count];
			
			
			
			for(int r = 0; r < fileLines.Count; r++)
			{
				string line = fileLines[r];
				string[] cols = line.Split(',');
				names[r] = cols[0];
				for(int c = 0; c < fileLines.Count; c++)
				{
					if( r == c )
						id[r,c] = 1;
					else
						id[r, c] = Double.Parse(cols[c+1]);
				}
			}
			
			//Example of needing to choose an initially global-best match, but upon expansion to 2 vectors allowed to optimize within clusters
			//double[,] id = { {1,.95, 0,.5,0}, {.95,1,0,.5,0}, {0,0,1,.95,.95}, {.5,.5,.95,1,.75}, {0,0,.95,.75,1} };
			
			//int[] result = findBasisVectors(id, Int32.Parse(args[1]), calcIDErrorSq);
			//int[] result = findBasisVectors(id, 5, maximalSeqIDSupportedError);
			
			int[] result = constructBasisVectors(id, Int32.Parse(args[1]), calcIDErrorSq);
			
			Console.WriteLine("Found basis vectors:");
			for(int i = 0; i < result.Length; i++)
			{
				Console.WriteLine(names[result[i]]);
			}
			
			double minSupport = minimalSeqIDSupported(id, result);
			//Console.WriteLine("Min supported seqID: {0}", minSupport);
			
			//Console.ReadLine();
		}
		
		public static List<string> readlines(string path)
		{
			List<string> result = new List<string>();
			
			StreamReader r = new StreamReader(path);
			while( !r.EndOfStream )
			{
				result.Add(r.ReadLine());
			}
			
			r.Close();
			
			return result;
		}
		
		public static int[] findBasisVectors(double[,] id, int numVectors, CalculateError errorFunction)
		{
			double bestError = double.MaxValue;
			int[] bestVectors = null;
			
			//Initialize
			int[] currentVector = new int[numVectors];
			for(int i = 0; i < currentVector.Length; i++)
				currentVector[i] = i;
			
			while( true )
			{
//				Console.Write("Checking: ");
//				for(int i = 0; i < currentVector.Length; i++)
//					Console.Write(currentVector[i] + " ");

			    double curError = errorFunction(id, currentVector);
				//Console.WriteLine("Got: {0}", curError);
				
				if( curError < bestError )
				{
					Console.WriteLine("   Found better error: {0} vs {1}", curError, bestError);
					
					bestError = curError;
					bestVectors = copyIntArray(currentVector);
				}
				
				
				if( !advanceChoose(currentVector, id.GetLength(0)) )
					break;
			}
			
			
			
			return bestVectors;
		}
		
		public static int[] constructBasisVectors(double[,] id, int numVectors, CalculateError errorFunction)
		{
			int[] basis = new int[numVectors];
			
			//Initialize
			if( numVectors == 1 )
				basis[0] = 0;
			else
			{
				//Otherwise, do something more complicated
				int s = numVectors - 1;
				int[] smallerBasis = constructBasisVectors(id, s, errorFunction);
				
				basis[0] = 0;
				for(int i = 0; i < s; i++)
					basis[i+1] = smallerBasis[i];
			}
			
			
			
			double error = errorFunction(id, basis);
			
			//Iterate and try to optimize
			bool improved = true;
			while( improved )
			{
				
				improved = false;
				for(int ptr = 0; ptr < numVectors; ptr++)
				{
					//Find the best error
					int bestIndex = basis[ptr];
					int oldIndex = basis[ptr];
					
					for(int i = 0; i < id.GetLength(0); i++)
					{
						basis[ptr] = i;
						double newError = errorFunction(id, basis);
						if( newError < error )
						{
							bestIndex = i;
							error = newError;
							improved = true;
						}
					}
					
					//update status
					bool displayMessage = (bestIndex != oldIndex);
					basis[ptr] = bestIndex;
					
					if( displayMessage )
					{
						Console.WriteLine("Improved error to {0}, {2} with {1}", error, arrayString(basis), numVectors);
					}
				}
			}
			
			Console.WriteLine("Returning: {0}", arrayString(basis));
			
			return basis;
		}
		
		public static string arrayString(int[] array)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			for(int i = 0; i < array.Length; i++)
			{
				sb.Append(array[i]);
				sb.Append(" ");
			}
			
			return sb.ToString();
		}
		
		//Returns true if there is another vector in the sequence
		public static bool advanceChoose(int[] vectors, int totalNum)
		{
			for(int v = 0; v < vectors.Length; v++)
			{
				int vPtr = vectors.Length - 1 - v;
				if( vectors[vPtr] != totalNum - 1 - v )
				{
					int update = vectors[vPtr] + 1;
					for(int vUpdate = vPtr; vUpdate < vectors.Length; vUpdate++)
					{
						vectors[vUpdate] = update;
						update++;
					}
					return true;
				}
			}
			return false;
			    
		}
				
		public static int[] copyIntArray(int[] input)
		{
			int[] result = new int[input.Length];
			for(int i = 0; i < input.Length; i++)
				result[i] = input[i];
			return result;
		}
		
		public delegate double CalculateError(double[,] id, int[] basisVector);
		
		public static double calcIDErrorSq(double[,] id, int[] basisVector)
		{
			return calcIDError(id, basisVector, 2);
		}
		
		public static double calcIDError(double[,] id, int[] basisVectors, int distanceOrder)
		{
			double error = 0;
			for(int i = 0; i < id.GetLength(0); i++)
			{
				double maxID = 0;
				for(int j = 0; j < basisVectors.Length; j++)
				{
					if( id[basisVectors[j],i] > maxID )
						maxID = id[basisVectors[j],i];
				}
				
				error += Math.Pow(1-maxID, distanceOrder);
			}
			
			return error;
		}
		
		public static double maximalSeqIDSupportedError(double[,] id, int[] basisVectors)
		{
		    return 1.0 - minimalSeqIDSupported(id, basisVectors);
		}
		
		public static double minimalSeqIDSupported(double[,] id, int[] basisVectors)
		{
		    double min = 1;
		    for(int i = 0; i < id.GetLength(0); i++)
		    {
		        //Figure how how well this one is supported
		        double maxID = 0;
				for(int j = 0; j < basisVectors.Length; j++)
				{
					if( id[basisVectors[j],i] > maxID )
						maxID = id[basisVectors[j],i];
				}
				
				if( maxID < min )
				    min = maxID;
		    }
		    
		    return min;
		}
	}
}