using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GCode; // Point3D

namespace GeneticAlgorithm
{
	/// <summary>
	/// Description of DataProvider.
	/// </summary>
	public static class DataProvider
	{
		public static List<IPoint> GetPoints(string filePath, string dataSelection)
		{
			var regexObj = new Regex(@"\{""x"":(\d+),""y"":(\d+)\}");
			var result = new List<IPoint>();

			// read in the data file
			foreach (var line in File.ReadLines(filePath)) {
				if (string.IsNullOrEmpty(line))
					continue;
				var elements = line.Split(new String[] {
				                          	"="
				                          }, StringSplitOptions.RemoveEmptyEntries);
				
				string dataName = elements[0].Trim();
				string dataSet = elements[1].Trim();
				
				if (dataSelection.Equals(dataName)) {
					// {"x":780,"y":560}
					Match matchResult = regexObj.Match(dataSet);
					while (matchResult.Success) {
						string _x = matchResult.Groups[1].Value;
						string _y = matchResult.Groups[2].Value;
						int x = int.Parse(_x);
						int y = int.Parse(_y);
						result.Add(new Point3D(x,y));
						matchResult = matchResult.NextMatch();
					}
				}
			}

			return result;
		}
	}
}
