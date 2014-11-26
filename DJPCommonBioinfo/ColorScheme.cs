/*
 * Daniel J. Parente
 * MD/PhD Candidate
 * Swint-Kruse Laboratory
 * University of Kansas Medical Center
 *
 * User: dparente
 * Date: 2012-08-06
 * Time: 14:13
 * 
 * 
 */
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;

namespace DJPCommonBioinfo
{
	public class ColorScheme
	{
		//Private variables
		private List<double> values = new List<double>();
		private List<Color> colors = new List<Color>();
		private int maxIndex;
		private Color noData;

		//Properties
		public Color NoData { get { return this.noData; } }
		public double MinValue { get { return values[0]; } }
		public double MaxValue { get { return values[maxIndex]; } }
		
		public ColorScheme(List<double> values, List<Color> colors, Color noData)
		{
			if( values.Count != colors.Count )
				throw new ArgumentException("Length of the the value stops and colors must be equal");
			
			this.values.AddRange(values);
			this.colors.AddRange(colors);
			
			//Values must be strictly ascending
			for(int i = 1; i < this.values.Count; i++)
			{
				if( this.values[i-1] >= this.values[i] )
					throw new ArgumentException("Values must strictly monotonically ascend");
			}
			
			if( this.values.Count < 2 )
				throw new ArgumentException("At least two values must be specified");
				
			
			this.maxIndex = this.values.Count - 1;
			
			//this.noData = colorToUint(noData);
			this.noData = noData;
		}
		
		public static ColorScheme getColorScheme(string descriptorPath)
		{
			System.IO.StreamReader r = new System.IO.StreamReader(descriptorPath);
			List<double> values = new List<double>();
			List<string> hex = new List<string>();
			
			Color noData = getColorListFromHex(new List<string> { r.ReadLine() } )[0];
			
			while( !r.EndOfStream )
			{
				string line = r.ReadLine();
				string[] fields = line.Split(' ');
				if( fields.Length != 2 )
					throw new ArgumentException("Wrong number of fields in color scheme descriptor line");
				
				values.Add(double.Parse(fields[0]));
				hex.Add(fields[1]);
			}
			
			return new ColorScheme(values, getColorListFromHex(hex), noData);
			
		}
		
		public static List<Color> getColorListFromHex(List<string> hexStrings)
		{
			List<Color> result = new List<Color>();
			for(int i = 0; i < hexStrings.Count; i++)
			{
				string canonHex = hexStrings[i].ToUpper();
				
				//Validate the input
				if( !Regex.IsMatch(canonHex, "[A-F0-9]{6}") )
					throw new ArgumentException("Invalid hexidecimal color string at index " + i + " found " + canonHex);
				
				//Otherwise parse the input
				
				byte R = byte.Parse(canonHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
				byte G = byte.Parse(canonHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
				byte B = byte.Parse(canonHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
				
				result.Add(Color.FromArgb(255, R, G, B));
			}
			
			return result;
		}
		
		public static Color getColorFromHex(string hex)
		{
			string canonHex = hex.ToUpper();
			
			//Validate the input
			if( !Regex.IsMatch(canonHex, "[A-F0-9]{6}") )
				throw new ArgumentException("Invalid hexidecimal color string");
			
			//Otherwise parse the input
			
			byte R = byte.Parse(canonHex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
			byte G = byte.Parse(canonHex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
			byte B = byte.Parse(canonHex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
			
			return Color.FromArgb(255, R, G, B);
		}
		
		//Returns and unsigned integer describing the color formatted.  Each 8 bytes (highest specified firsT)
		// corrospond to the alpha, red, green, blue
		public Color getColor(double data, bool allowOutOfRange)
		{		
			//Check for out of range
			if( data < values[0] )
			{
				if( allowOutOfRange )
					return colors[0];
				else 
					throw new ArgumentException("Data below minimum of scheme");
			}
			
			if( data > values[maxIndex] )
			{
				if( allowOutOfRange ) 
					return colors[maxIndex]; 
				else 
					throw new ArgumentException("Data above maximum of scheme");
			}
			
			//Otherwise scan through until determining the bounding color stops and interpolate the color
			for(int i = 0; i < maxIndex; i++)
			{
				if( data >= values[i] && data <= values[i+1] )
				{
					//Calculate the interpolation percentage
					double interp = (data - values[i])/(values[i+1] - values[i]);
					return interpolate(colors[i], colors[i+1], interp);
				}
			}
			
			//Shouldn't be possible due to the above logic, but compilation fails if no behavior is specified
			// for failure to match any of the ranges (or outside the ranges... I think that covers everything!)
			throw new ArgumentException("Failed to calculate a color using the color scheme");
			
		}
		
		//Return an unsigned int representing a color with bytes (highest to lowest):
		//	alpha, red, green, blue
		public static uint colorToUint(Color c)
		{
			uint result = c.A;
			result <<= 8;
			result |= c.R;
			result <<= 8;
			result |= c.G;
			result <<= 8;
			result |= c.B;
			
			return result;
		}
		
		public static uint bytesToUint(byte A, byte R, byte G, byte B)
		{
			uint result = A;
			result <<= 8;
			result |= R;
			result <<= 8;
			result |= G;
			result <<= 8;
			result |= B;
			
			return result;
		}
		
		public Color interpolate(Color x, Color y, double pctY)
		{
			if( pctY < 0 || pctY > 1.0 )
				throw new ArgumentException("Percent of Y must be on [0,1]");
				
			
			double pctX = 1.0-pctY;
			
			byte A = (byte)(x.A * pctX + y.A*pctY);
			byte R = (byte)(x.R * pctX + y.R*pctY);
			byte G = (byte)(x.G * pctX + y.G*pctY);
			byte B = (byte)(x.B * pctX + y.B*pctY);
			
			//return bytesToUint(A, R, G, B);
			return Color.FromArgb(A, R, G, B);
		}
	}

}
