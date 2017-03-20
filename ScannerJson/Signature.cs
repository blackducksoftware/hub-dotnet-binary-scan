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
	/// <summary>
	/// Represents a signature or a hash of a file
	/// </summary>
	public sealed class Signature
	{
		public sealed class Type
		{
			public static readonly Type File_Sha1 = new Type("FILE_SHA1");

			readonly String value;
			public String Value
			{
				get
				{
					return value;
				}
			}

			private Type(String value)
			{
				this.value = value;
			}

		}

		public Type SignatureType { get; private set; }
		public String Value { get; private set; }

		public Signature(Type type, String value)
		{
			this.SignatureType = type;
			this.Value = value;
		}
	}
}
