using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

using System.IO;
namespace Blackduck.Hub
{
	public class ScannerJsonBuilder
	{
		public static void Write(IEnumerable<Node> nodes, Stream outputStream)
		{
			using (StreamWriter sw = new StreamWriter(outputStream))
			using (JsonWriter writer = new JsonTextWriter(sw))
			{
				writer.WriteStartArray();
				foreach (Node node in nodes)
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
				}
				writer.WriteEndArray();
			}

		}
	}
}
