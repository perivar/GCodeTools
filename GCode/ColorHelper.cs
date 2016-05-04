/**
 * Copied from the SimpleGcodeParser file
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace GCode
{
	public enum PenColorList
	{
		RapidMove,
		NormalMove,
		CWArc,
		CCWArc,
		RapidMoveHilight,
		LineHighlight,
		Background,
		GridLines
	}

	public static class ColorHelper
	{
		private static IDictionary<PenColorList, Pen> _penList = new Dictionary<PenColorList, Pen>();
		private static IDictionary<PenColorList, Color> _colorList = new Dictionary<PenColorList, Color>();

		private static Color GetDefaultColor(PenColorList list)
		{
			if (list == PenColorList.RapidMove) return Color.Red;
			if (list == PenColorList.NormalMove) return Color.DodgerBlue;
			if (list == PenColorList.CWArc) return Color.Lime;
			if (list == PenColorList.CCWArc) return Color.Yellow;
			if (list == PenColorList.RapidMoveHilight) return Color.Pink;
			if (list == PenColorList.LineHighlight) return Color.White;
			if (list == PenColorList.Background) return Color.FromArgb(0x20, 0x20, 0x20);
			if (list == PenColorList.GridLines) return Color.DimGray;
			return Color.White;
		}

		public static Color GetColor(PenColorList type)
		{
			if (!_colorList.ContainsKey(type))
			{
				var value = QuickSettings.Get[string.Format("Color{0}", type)];
				if (string.IsNullOrWhiteSpace(value))
				{
					_colorList[type] = GetDefaultColor(type);
				}
				else
				{
					try
					{
						if (value.Contains(','))
						{
							var bits = value.Split(',');
							var r = int.Parse(bits[0]);
							var g = int.Parse(bits[1]);
							var b = int.Parse(bits[2]);
							_colorList[type] = Color.FromArgb(r, g, b);
						}
						else
						{
							_colorList[type] = Color.FromName(value);
						}
					}
					catch (Exception)
					{
						_colorList[type] = GetDefaultColor(type);
					}
				}
			}

			return _colorList[type];
		}

		public static void SetColor(PenColorList type, Color newColor)
		{
			
			var value = Convert.ToString(newColor);
			if (_colorList.ContainsKey(type))
			{
				_colorList[type] = newColor;
				if (_penList.ContainsKey(type)) _penList[type] = new Pen(newColor, 1);
				else _penList.Add(type, new Pen(newColor, 1));
			}
			else
			{
				_colorList.Add(type, newColor);
				_penList.Add(type, new Pen(newColor, 1));
			}

			QuickSettings.Get[string.Format("Color{0}", type)] = string.Format("{0},{1},{2}", newColor.R, newColor.G, newColor.B);
		}

		public static Pen GetPen(PenColorList type)
		{
			if (!_penList.ContainsKey(type))
			{
				if (type == PenColorList.LineHighlight)
					_penList[type] = new Pen(GetColor(type), 2f);
				else
					_penList[type] = new Pen(GetColor(type), 1f);
			}

			return _penList[type];
		}
	}
}
