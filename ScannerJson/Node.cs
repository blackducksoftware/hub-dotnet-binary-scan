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

namespace Blackduck.Hub
{

	public class Node
	{
		public enum Type { Directory, File };
		public long Id {get; private set;}
		public Node Parent {get; private set;}
		public Type NodeType {get; private set;}
		public String Name {get; private set;}
		public String Path{get; private set;}
		public long Size { get; private set; } = 0;
		public String ArchiveUri { get; private set; } = "";
		public Signature[] Signatures { get; private set; } = new Signature[] { };

		private Node ()
		{			
		}


		/// <summary>
		/// Creates a new directory node
		/// </summary>
		/// <returns>The directory.</returns>
		/// <param name="name">Name.</param>
		/// <param name="path">Path.</param>
		public static Node NewDirectory(long id, String name, String path)
		{
			return NewDirectory( id, null, name, path);
		}

		/// <summary>
		/// Creates a new directory node
		/// </summary>
		/// <returns>The directory.</returns>
		/// <param name="name">Name.</param>
		/// <param name="path">Path.</param>
		public static Node NewDirectory(long id, Node parent, String name, String path)
		{
			return new Node() { Id = id, Parent = parent, Name = name, Path = path, NodeType = Type.Directory, Size = 0 };
		}

		/// <summary>
		/// Creates a new file node
		/// </summary>
		/// <returns>The directory.</returns>
		/// <param name="name">Name.</param>
		/// <param name="path">Path.</param>
		public static Node NewFile(long id, Node parent, String name, String Path, long size, String archiveUri, params Signature[] signatures)
		{
				return new Node() { Id = id, Parent = parent, NodeType = Type.File, Name = name, Path = Path, Size = size, ArchiveUri = archiveUri, Signatures = signatures};
		}
		
	}
}
