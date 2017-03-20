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
 * SPDX-License-Identifier: 
 */


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Blackduck.Hub
{
    static class _Extension
    {
        public static T _getOrDefault<S, T>(this IDictionary<S, T> dictionary, S key, T defaultValue)
        {
            T result;
            if (dictionary.TryGetValue(key, out result)) return result;
            else return defaultValue;
        }

    }
    sealed class Settings
    {
        private static readonly string propFileName = "scanner.ini";
        private static Lazy<Settings> instance = new Lazy<Settings>(() => new Settings());
        
        public static Settings Instance { get { return instance.Value; } }

       
        private Dictionary<string, string> settings = new Dictionary<string, string>();

        private Settings()
        {
            string filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), propFileName);
            if (!File.Exists(filePath)) return;
            Console.WriteLine("Using settings in " + filePath);
            string[] readLines = File.ReadAllLines(filePath);
            foreach (string line in readLines.Where(line => !string.IsNullOrWhiteSpace(line)))
            {
                string[] split = line.Split(new char[] { '=' }, 2);
                if (split.Length == 2)
                    settings.Add(split[0], split[1]);
            }
        }

        public string Url { get { return settings._getOrDefault("url", null); } }
        public string Username { get { return settings._getOrDefault("username", null); } }
        public string Password { get { return settings._getOrDefault("password", null); } }
    }
}
