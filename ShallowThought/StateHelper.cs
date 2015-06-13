using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace ShallowThought
{
	public static class StateHelper
	{
		public static void Save(object graph, string path)
		{
			var bf = new BinaryFormatter();
			var stream = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			try
			{
				bf.Serialize(stream, graph);
			}
			finally
			{
				stream.Close();
			}
		}

		public static object Load(string path)
		{
			var bf = new BinaryFormatter();
			var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			try
			{
				return bf.Deserialize(stream);
			}
			finally
			{
				stream.Close();
			}
		}

	}

}
