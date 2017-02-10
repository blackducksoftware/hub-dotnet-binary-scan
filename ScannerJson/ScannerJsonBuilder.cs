using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;

using System.IO;
using System.Diagnostics;

namespace Blackduck.Hub
{
	public class ScannerJsonBuilder
	{
		private long counter = 0;
		private Stack<Node> parentDirs = new Stack<Node>();
		private IList<Node> allNodes = new List<Node>();
		private IList<JObject> problemList = new List<JObject>();

		private ScannerJsonBuilder()
		{
		}

		public static ScannerJsonBuilder NewInstance()
		{
			return new ScannerJsonBuilder();
		}

		public String ScannerVersion { private get; set; } = "0.0.0.0";
		public String SignatureVersion { private get; set; } = "7.0.0";
		public String HostName { private get; set; } = System.Environment.MachineName;
		public String ProjectName { get; set; }
		public String Release { private get; set; }

		private Node getCurrentDir()
		{
			if (parentDirs.Count == 0) return null;
			else return parentDirs.Peek();
		}


		/// <summary>
		/// Adds a subdirectory to the  current directory (or to the top level) and makes that directory current
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="path">Path.</param>
		public void AddDirectory(String name, String path)
		{
			Node newDirectory = Node.NewDirectory(counter++, getCurrentDir(), name, path);
			parentDirs.Push(newDirectory);
			allNodes.Add(newDirectory);
		}

		/// <summary>
		/// Logs a scan problem in the appropriate section of the scan file.
		/// </summary>
		/// <param name="problem">Problem.</param>
		public void AddScanProblem(Exception e)
		{
			var problemObjectInner = new JObject(
				new JProperty("message", e.Message),
				new JProperty("exceptionType", e.GetType().FullName),
				new JProperty("stack", e.StackTrace)
			);

			var problemObjectTop = new JObject(
			new JProperty("problem", problemObjectInner));

			problemList.Add(problemObjectTop);
		}

		/// <summary>
		/// Adds a file to the current directory.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="path">Path.</param>
		/// <param name="size">Size.</param>
		/// <param name="sha1">Sha1.</param>
		public void AddFile(String name, String path, long size, String sha1)
		{
			Node newFile = Node.NewFile(counter++, getCurrentDir(), name, path, size, "",
										new Signature(Signature.Type.File_Sha1, sha1));
			allNodes.Add(newFile);
		}


		public void Write(TextWriter outWriter)
		{

			using (JsonWriter writer = new JsonTextWriter(outWriter))
			{
				bool first = true;
				writer.Formatting = Formatting.Indented;
				writer.WriteStartObject();

				if (problemList.Count > 0)
				{
					writer.WritePropertyName("scanProblemList");
					writer.WriteStartArray();
					writer.WriteRaw(problemList.Select(prob => prob.ToString()).Aggregate((arg1, arg2) => arg1 + "," + arg2));
					writer.WriteEndArray();
				}

				writer.WritePropertyName("scanNodeList");

				writer.WriteStartArray();
				foreach (Node node in allNodes)
				{
					JObject signatures = new JObject();
					foreach (Signature signature in node.Signatures)
					{
						signatures.Add(new JProperty(signature.SignatureType.Value, signature.Value));
					}
					JObject jsonNode = new JObject(
						new JProperty("id", node.Id),
						new JProperty("type", node.NodeType.ToString().ToUpper()),
						new JProperty("name", node.Name ?? ""),
						new JProperty("path", node.Path ?? ""),
						new JProperty("size", node.Size),
						new JProperty("archiveUri", node.ArchiveUri ?? ""),
						new JProperty("signatures", signatures)
					);

					if (node.Parent != null)
						jsonNode.Add(new JProperty("parentId", node.Parent.Id));

					if (first)
						first = false;
					else writer.WriteRaw(",");

					writer.WriteRaw(jsonNode.ToString());



				}
				writer.WriteEndArray();
				writeMiscellaneousProperties(writer);
			}

		}

		private void writeMiscellaneousProperties(JsonWriter writer)
		{
			writer.WritePropertyName("signatureVersion");
			writer.WriteValue(this.SignatureVersion);

			writer.WritePropertyName("scannerVersion");
			writer.WriteValue(this.ScannerVersion);

			writer.WritePropertyName("hostName");
			writer.WriteValue(this.HostName);

			writer.WritePropertyName("ownerEntityKeyToken");
			writer.WriteValue($"SN#{this.HostName}-{this.ProjectName}");


			if (this.ProjectName != null)
			{
				writer.WritePropertyName("project");
				writer.WriteValue(this.ProjectName);
			}

			if (this.Release != null)
			{
				writer.WritePropertyName("release");
				writer.WriteValue(this.Release);
			}
		}
	}
}
