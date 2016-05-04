/**
 * Copied from the SimpleGcodeParser file
 * Copyright (c) David-John Miller AKA Anoyomouse 2014
 *
 * See LICENCE in the project directory for licence information
 * Modified by perivar@nerseth.com
 **/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Linq;

namespace GCode
{
	public class QuickSettings
	{
		private static QuickSettings _inst;
		public static QuickSettings Get { get { return _inst; } }

		static QuickSettings()
		{
			_inst = new QuickSettings();
		}

		private QuickSettings()
		{
			// Load data here!
			LoadSavedData();
		}

		Dictionary<string, string> settingsData = new Dictionary<string, string>();
		private void LoadSavedData()
		{
			#region Code
			var application = new FileInfo(Application.ExecutablePath);
			var settingsFile = new FileInfo(Path.Combine(application.DirectoryName, "settings.xml"));


			if (settingsFile.Exists)
			{
				var stream = settingsFile.OpenRead();
				var xml = XDocument.Load(stream);
				stream.Close();
				var root = xml.FirstNode as XElement;

				foreach (XElement node in root.Nodes())
				{
					settingsData.Add(node.Name.LocalName, node.Value);
				}
			}
			#endregion
		}

		private void SaveSettings()
		{
			#region Code
			var application = new FileInfo(Application.ExecutablePath);
			var settingsFile = new FileInfo(Path.Combine(application.DirectoryName, "settings.xml"));

			var xml = new XDocument();
			var root = new XElement("root");
			xml.Add(root);
			foreach(var kvp in settingsData)
			{
				root.Add(new XElement(kvp.Key, new XText(kvp.Value)));
			}

			xml.Save(settingsFile.FullName);
			#endregion
		}

		public string this[string key]
		{
			get
			{
				#region Code
				if (!settingsData.ContainsKey(key))
				{
					settingsData.Add(key, GetDefaultValue(key));
				}

				return settingsData[key];
				#endregion
			}
			set
			{
				#region Code
				if (settingsData.ContainsKey(key))
					settingsData[key] = value;
				else
					settingsData.Add(key, value);

				SaveSettings();
				#endregion
			}
		}

		private string GetDefaultValue(string key)
		{
			if (string.IsNullOrWhiteSpace(string.Empty)) return string.Empty;
			return string.Empty;
		}
	}
}
