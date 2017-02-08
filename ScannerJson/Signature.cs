using System;
namespace Blackduck.Hub
{
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
