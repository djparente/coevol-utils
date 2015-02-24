/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-11-26
 * Time: 12:10
 * 
 * 
 */

using DJPCommonBioinfo;
using System;
using System.IO;
using System.IO.Compression;
using ZNMI;

namespace ZNDAMI
{
	class Program
	{
		public static void Main(string[] args)
		{
			if( args.Length != 2 )
				ZNMIAlg.errorOut("Usage: ./ZNDAMI.exe [MSA Path] [Output Path]", 255);
			
			string msaPath = args[0];
			string outpath = args[1];
			
			//Sanity check existance (though the real test is in loading)
			if( !System.IO.File.Exists(msaPath) )
				ZNMIAlg.errorOut("Error: The MSA does not seem to exist", 254);
			
			//Refuse to clobber an existing output file
			if( System.IO.File.Exists(outpath) )
				ZNMIAlg.errorOut("Error: Output file already exists; refusing to clobber", 200);
			
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
				ZNMIAlg.errorOut("This was a problem loading the MSA.  Exception text was: " + ex.Message, 253);
			}
			
			const double minEntropy = 0.198515243;
			const double maxGaps = 0.1;
			const bool ignoreGaps = true;
			const bool suppressNegative = false;
			
			double[,] zndamiValues = ZNMIAlg.zn_dami(msa, minEntropy, maxGaps, ignoreGaps, suppressNegative);
			
			
			//ZNMI in XYZ output format
			// residue1 residue2 coevol_score
			string xyzData = ZNMIAlg.getXYZFormat(zndamiValues);
			//Console.WriteLine(xyzData);
			
			//Write out in gzipped format
			byte[] outBytes = System.Text.Encoding.UTF8.GetBytes(xyzData);
			FileStream fs = new FileStream(outpath, FileMode.Create);
			GZipStream gz = new GZipStream(fs, CompressionMode.Compress);
			gz.Write(outBytes, 0, outBytes.Length);
			gz.Close();
			fs.Close();
		}
	}
}