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
