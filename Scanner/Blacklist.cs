/**
 * Black Duck Hub .Net Binary Scanner
 *
 * Copyright (C) 2017 Black Duck Software, Inc.
 * http://www.blackducksoftware.com/
 *
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements. See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership. The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License. You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied. See the License for the
 * specific language governing permissions and limitations
 * under the License.
 * 
 * SPDX-License-Identifier: Apache-2.0
 */

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
