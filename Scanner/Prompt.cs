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
using System.Text;
namespace Blackduck.Hub
{
	public class Prompt
	{
		public static string ReadPassword(string prompt)
		{
			Console.WriteLine(prompt);
			ConsoleKeyInfo keyInfo;
			var password = new StringBuilder();
			do
			{
				keyInfo = Console.ReadKey(true);
				if (keyInfo.Key == ConsoleKey.Backspace)
				{
					if (password.Length > 0)
					{
						password.Remove(password.Length - 1, 1);
						Console.Write('\b');
					}
				}
				else if (keyInfo.Key == ConsoleKey.Enter)
				{
					Console.WriteLine();
					break;
				}
				else if (char.IsSymbol(keyInfo.KeyChar) 
				         || char.IsLetterOrDigit(keyInfo.KeyChar) 
				         || char.IsWhiteSpace(keyInfo.KeyChar) 
				         || char.IsPunctuation(keyInfo.KeyChar))
				{
					password.Append(keyInfo.KeyChar);
					Console.Write('*');
				}

			} while (true);
			return password.ToString();
		}

		public static string Read(string prompt)
		{
			Console.WriteLine(prompt);
			return Console.ReadLine();
		}
	}
}
