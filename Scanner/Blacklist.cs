using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;

namespace Blackduck.Hub
{
	public class Blacklist
	{
		private static Lazy<Blacklist> instance = new Lazy<Blacklist>(() => new Blacklist());
		private Dictionary<string, string> entries = new Dictionary<string, string>();

		public static Blacklist Instance
		{
			get
			{
				return instance.Value;
			}
		}

		/// <summary>
		/// Returns null if the black list does not contain paramref name="sha1"/> or the blacklisted file name, if it does.
		/// </summary>
		/// <returns>The contains.</returns>
		/// <param name="sha1">Sha1.</param>
		public string Contains(string sha1)
		{
			string value;
			if (!entries.TryGetValue(sha1, out value))
			{
				return null;
			}
			else
			{
				return value;
			}
		}

		private Blacklist()
		{
			using (Stream resourceStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Scanner.dll_blacklist.txt"))
			using (StreamReader reader = new StreamReader(resourceStream))	
			{
				while (!reader.EndOfStream)
				{
					String line = reader.ReadLine();
					if (!string.IsNullOrWhiteSpace(line))
					{
						string[] values = line.Split(new char[] { ' ' }, 2);
						if (values.Length != 2)
						{
							Console.Error.WriteLine("Illegal blacklist entry: " + line);
						}
						else
						{
							entries.Add(values[1], values[0]);
						}
					}
				}
			}
		}
	}
}
