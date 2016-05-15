using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;

namespace GCode
{
	/// <summary>
	/// Description of GCodeUtils.
	/// </summary>
	public static class GCodeUtils
	{
		/// <summary>
		/// Split the list of gcode instructions into gcode blocks.
		/// splits a G0 commands that includes either X and/or Y.
		/// Returns everything before the first G0, all G0 blocks and everything after last block
		/// </summary>
		/// <param name="instructions">list of gcode instructions</param>
		/// <returns>GCode split object</returns>
		public static GCodeSplitObject SplitGCodeInstructions(List<GCodeInstruction> instructions) {

			var allG0 = new List<Point3DBlock>();

			// temporary lists
			var priorToG0 = new List<GCodeInstruction>();
			var notG0 = new List<GCodeInstruction>();
			var eof = new List<GCodeInstruction>();
			
			foreach (var currentInstruction in instructions) {
				
				// check if this line is a G0 command
				if (currentInstruction.CommandEnum == CommandList.RapidMove) {

					// this line is a G0 command, get the X and Y values
					float? x = currentInstruction.X;
					float? y = currentInstruction.Y;

					// check if x or y exist for this line
					if (x.HasValue || y.HasValue) {
						
						// if x or y here is false we need to use the last coordinate from the previous G0 or G1 in followingLines as that is where the machine would be
						if (!y.HasValue && notG0.Count > 0) {
							
							// loop through allG0[-1].followingLines to find the most recent G0 or G1 with a y coordinate

							// We want to use the LINQ to Objects non-invasive
							// Reverse method, not List<T>.Reverse
							foreach (GCodeInstruction item in Enumerable.Reverse(notG0)) {
								if ((item.CanRender)
								    && item.Y.HasValue) {
									// set this y coordinate as y
									y = item.Y.Value;
									break;
								}
							}
							
						} else if (!x.HasValue && notG0.Count > 0) {
							// loop through allG0[-1].followingLines to find the most recent G0 or G1 with a x coordinate
							
							// We want to use the LINQ to Objects non-invasive
							// Reverse method, not List<T>.Reverse
							foreach (GCodeInstruction item in Enumerable.Reverse(notG0)) {
								if ((item.CanRender)
								    && item.X.HasValue) {
									// set this x coordinate as x
									x = item.X.Value;
									break;
								}
							}
						}
						
						// if y still have no value, force to 0
						if (!y.HasValue) {
							y = 0;
						}
						// if x still have no value, force to 0
						if (!x.HasValue) {
							x = 0;
						}
						
						if (allG0.Count > 0) {
							// allG0 has entries, so we need to add notG0 to the followingLines for the previous entry in allG0
							var lastElement = allG0.Last();
							lastElement.GCodeInstructions.AddRange(notG0);
						}

						// this G0 has a valid X or Y coordinate, add it to allG0 with itself (the G0) as the first entry in followingLines
						var point = new Point3DBlock(x.Value, y.Value);
						point.GCodeInstructions.Add(currentInstruction);
						allG0.Add(point);
						
						// reset notG0
						notG0.Clear();

					} else {
						// there is no X or Y coordinate for this G0, we can just add it as a normal line
						notG0.Add(currentInstruction);
					}

				} else {
					// add this line to notG0
					notG0.Add(currentInstruction);
				}

				if (allG0.Count == 0) {
					// this holds lines prior to the first G0 for use later
					priorToG0.Add(currentInstruction);
				}
			}
			
			// add notG0 to the followingLines for the last entry in allG0
			// this gets the lines after the last G0 in the file
			// we also need to check if the commands here are not G0, G1, G2, G3, or G4
			// because in this case they should be left at the end of the file, not put into the parent G0 block
			foreach (var currentInstruction in notG0) {

				// check if this line is a G0, G1, G2 or G3
				if (currentInstruction.CanRender) {
					// this should be added to the parent G0 block
					allG0.Last().GCodeInstructions.Add(currentInstruction);
				} else {
					// this should be added to the end of the file as it was already there
					eof.Add(currentInstruction);
				}
			}
			
			// add header and footer as special blocks
			var gcodeBlocks = new GCodeSplitObject();
			gcodeBlocks.MainBlocks = allG0;
			gcodeBlocks.Header = priorToG0;
			gcodeBlocks.Footer = eof;
			
			return gcodeBlocks;
		}
		
		/// <summary>
		/// Sort the list of Point3DBlock by Z-height so that the cutting is incremental
		/// </summary>
		/// <param name="bestPath">Original sequence of the point3d elements</param>
		/// <param name="points">Point elements</param>
		/// <returns>sorted sequence of the point3d elements</returns>
		public static List<int> SortBlocksByZDepth(List<int> bestPath, List<IPoint> points) {
			var sortedBestPath = new List<int>();
			
			float z = 0.0f;
			Point3DBlock previousBlock = null;
			
			if (points != null && points.Count > 0 && points[0] is Point3DBlock) {
				
				for (int c=0; c < bestPath.Count; c++) {
					var block = points[bestPath[c]] as Point3DBlock;
					
					if (block.EqualCoordinates(previousBlock)) {
						// find all blocks with the same X and Y
						
						// and then sort by z
					} else {
						// new block
					}
					
					var instructions = block.GCodeInstructions;
					
					foreach (var instruction in instructions) {
						if (instruction.CommandEnum == CommandList.NormalMove
						    && !instruction.X.HasValue
						    && !instruction.Y.HasValue
						    && instruction.Z.HasValue) {
							z = instruction.Z.Value;
							break;
						}
					}
					
					//
				}
				
			}
			
			return sortedBestPath;
		}
		
		/// <summary>
		/// Save the gcode instructions assuming the points are Point3DBlock elements
		/// </summary>
		/// <param name="bestPath">best sequence of the point3d elements</param>
		/// <param name="points">Point elements</param>
		/// <param name="filePath">filepath to save</param>
		/// <returns>succesful or </returns>
		public static bool SaveGCode(List<int> bestPath, List<IPoint> points, string filePath) {
			
			if (points != null && points.Count > 0 && points[0] is Point3DBlock) {
				
				// put all the lines back together in the best order
				var file = new FileInfo(filePath);
				var tw = new StreamWriter(file.OpenWrite());
				tw.WriteLine("(File built with GCodeTools)");
				tw.WriteLine("(Generated on " + DateTime.Now.ToString() + ")");
				tw.WriteLine();

				for (int c=0; c < bestPath.Count; c++) {
					tw.WriteLine(string.Format("(Start Block_{0})", c));
					
					var block = points[bestPath[c]] as Point3DBlock;
					var instructions = block.GCodeInstructions;
					foreach (var instruction in instructions) {
						tw.WriteLine(instruction);
					}

					tw.WriteLine(string.Format("(End Block_{0})", c));
					tw.WriteLine();
					tw.Flush();
				}

				tw.WriteLine("(Footer)");
				tw.WriteLine("(Footer end.)");
				tw.WriteLine();

				tw.Flush();
				tw.Close();
				
				return true;
				
			} else {
				return false;
			}
			
			/*
			for (FIXME_VAR_TYPE c=0; c<priorToG0.Length; c++) {
				fout += priorToG0[c] + '\n';
			}
			for (FIXME_VAR_TYPE c=0; c<best.Length; c++) {
				for (FIXME_VAR_TYPE n=0; n<points[best[c]].followingLines.Length; n++) {
					fout += points[best[c]].followingLines[n] + '\n';
				}
			}
			for (FIXME_VAR_TYPE c=0; c<eof.Length; c++) {
				fout += eof[c] + '\n';
			}
			 */
		}
	}
	
	public class GCodeSplitObject {
		
		public List<Point3DBlock> MainBlocks { get; set; }
		public List<GCodeInstruction> Header { get; set; }
		public List<GCodeInstruction> Footer { get; set; }
		
		public override string ToString()
		{
			return string.Format(CultureInfo.CurrentCulture,
			                     "Header: {0}, Blocks: {1}, Footer: {2}",
			                     this.Header.Count,
			                     this.MainBlocks.Count,
			                     this.Footer.Count
			                    );
		}
	}
}
