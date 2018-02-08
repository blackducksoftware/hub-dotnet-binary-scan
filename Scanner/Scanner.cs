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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Linq;
using CommandLine;
using CommandLine.Text;
using System.Net;

namespace Blackduck.Hub
{
    static class Scanner
    {
        private static readonly SHA1Cng sha1Calculation = new SHA1Cng();

        private static void EnqueueAll<T>(this Queue<T> queue, IEnumerable<T> elements)
        {
            foreach (T element in elements)
                queue.Enqueue(element);
        }

        static void Main(string[] args)
        {
            bool hubUpload = false;
            string username = null;
            string password = null;
            bool ignoreSsl = false;
            string targetAssemblyPath = null;
            string outputFile = null;


            ParserResult<Options> parseResult = CommandLine.Parser.Default.ParseArguments<Options>(args).WithNotParsed(result =>
              {
                  Console.Error.WriteLine("Invalid usage");
                  Environment.Exit(1);
              }).WithParsed(options =>
              {
                  ignoreSsl = options.IgnoreSslErrors;
                  outputFile = options.OutputFile.FirstOrDefault();
                  targetAssemblyPath = Path.GetFullPath(options.AssemblyToScan);
              });

            if (string.IsNullOrWhiteSpace(outputFile))
            {
                //No output argument. Do we have a preconfigured hub instance?
                if (Settings.Instance.Url != null)
                {
                    username = Settings.Instance.Username;
                    while (string.IsNullOrEmpty(username))
                    {
                        username = Prompt.Read($"Please enter a username for the hub instance at {Settings.Instance.Url}");
                    }
                    password = Settings.Instance.Password;
                    while (string.IsNullOrEmpty(password))
                    {
                        password = Prompt.ReadPassword($"Please enter the password for user {Settings.Instance.Username} at the Hub instance at {Settings.Instance.Url}");
                    }
                    hubUpload = true;
                }
                else
                {
                    Console.Error.WriteLine("No URL specified in configuration file and no output file specified. Exiting.");
                    Environment.Exit(1);
                }
            }

            if (ignoreSsl)
            {
                Console.WriteLine("SSL errors will be ignored.");
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }

            Console.WriteLine("Scanning " + targetAssemblyPath + "...");


            ScannerJsonBuilder builder = scanAssembly(targetAssemblyPath);

            if (hubUpload)
                HubUpload.UploadScan(Settings.Instance.Url, username, password, builder);
            else
            {
                using (var fileWriter = new StreamWriter(outputFile, false))
                {
                    builder.Write(fileWriter);
                }
            }
        }


        private static ScannerJsonBuilder scanAssembly(string target)
        {
            ScannerJsonBuilder builder = ScannerJsonBuilder.NewInstance();
            Assembly targetAssembly = null;
            try
            {
                targetAssembly = Assembly.LoadFile(target);
            }
            catch (Exception e)
            {
                string message = $"Unable to examine Assembly: {target}. {e.GetType().Name}: {e.Message}";
                Console.Error.WriteLine(message);
                builder.AddScanProblem(message, e);
                return builder;
            }

            Console.WriteLine(targetAssembly.GetName().Name + " " + targetAssembly.GetName().GetPublicKey());

            //Prime the output model
            builder.ProjectName = targetAssembly.GetName().Name;
            builder.Release = targetAssembly.GetName().Version.ToString();
            builder.AddDirectory(targetAssembly.GetName().Name, new FileInfo(target).FullName);

            //The files already scanned
            ISet<string> scannedPaths = new HashSet<String>();

            //The assembly references found that need scanning
            //For each reference, get the assembly that contains that reference, so we can look for DLLs at the parent's location.
            var assembliesToScan = new Queue<Tuple<AssemblyName, Assembly>>();

            assembliesToScan.Enqueue(Tuple.Create(targetAssembly.GetName(), targetAssembly));
            assembliesToScan.EnqueueAll(targetAssembly.GetReferencedAssemblies().Select(assemblyName => Tuple.Create(assemblyName, targetAssembly)));

            while (assembliesToScan.Count > 0)
            {
                var assemblyQueueEntry = assembliesToScan.Dequeue();
                AssemblyName refAssemblyName = assemblyQueueEntry.Item1;
                Assembly parentAssembly = assemblyQueueEntry.Item2;


                string parentAssemblyDirectory = Path.GetDirectoryName(parentAssembly.Location);
                string targetLocation = Path.Combine(parentAssemblyDirectory, refAssemblyName.Name + ".dll");

                try
                {
                    var refAssembly = File.Exists(targetLocation) ? Assembly.LoadFrom(targetLocation) : Assembly.Load(refAssemblyName);

                    String path = Path.GetFullPath(refAssembly.Location);
                    if (scannedPaths.Contains(path))
                        continue;

                    scannedPaths.Add(path);
                    //Note the file informatoin
                    FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(path);
                    string name = string.IsNullOrWhiteSpace(fvi.ProductName) ? refAssembly.GetName().Name : fvi.ProductName;
                    string fileName = Path.GetFileName(path);
                    //We'll make our file names more descriptive than just the actual file name.
                    string fileEntry = ($"{fileName} - {name}[{fvi.ProductVersion}]");
                    Console.WriteLine("Found " + fileEntry);

                    bool blacklisted = false;
                    if (!string.IsNullOrWhiteSpace(path))
                    {
                        string sha1 = computeSha1(path);
                        if (fileName != Blacklist.Instance.Contains(sha1))
                            builder.AddFile(fileEntry, path, new FileInfo(path).Length, sha1);
                        else blacklisted = true;
                    }
                    if (!blacklisted)
                        assembliesToScan.EnqueueAll(refAssembly.GetReferencedAssemblies().Select(parentAssemblyName => Tuple.Create(parentAssemblyName, refAssembly)));


                    //Find and document native code invocations.
                    var pInvokePaths = PInvokeSearcher.findPInvokePaths(refAssembly);
                    foreach (string nativeDllPath in pInvokePaths.FoundPaths.Where(fp => !scannedPaths.Contains(fp)))
                    {
                        FileVersionInfo dllVersionInfo = FileVersionInfo.GetVersionInfo(nativeDllPath);
                        String dllFileName = ($"{Path.GetFileName(nativeDllPath)} - {dllVersionInfo.ProductName}[{dllVersionInfo.ProductVersion}]");
                        string sha1 = computeSha1(nativeDllPath);
                        if (!string.Equals(Blacklist.Instance.Contains(sha1), dllFileName))
                        {
                            Console.WriteLine("Found native: " + dllFileName);
                            builder.AddFile(dllFileName, nativeDllPath, new FileInfo(nativeDllPath).Length, sha1);
                        }
                        scannedPaths.Add(nativeDllPath);
                    }

                }
                catch (IOException e)
                {
                    string message = $"Unable to examine Assembly '{refAssemblyName.Name}' ({targetLocation}). It and its dependencies will be omitted. [{e.GetType().Name}: {e.Message}]";
                    builder.AddScanProblem(message, e);
                }
            }
            return builder;
        }

        private static String computeSha1(String path)
        {
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read))
            {
                byte[] hash = sha1Calculation.ComputeHash(fs);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }
    }
}
