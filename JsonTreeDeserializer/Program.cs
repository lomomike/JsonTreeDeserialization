using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonTreeDeserializer
{
	class Program
	{
		static void Main(string[] args)
		{
			var tree = System.IO.File.ReadAllText("tree.json");
			var reader = new JsonTextReader(new StringReader(tree));
			FileContent content = JsonSerializer.CreateDefault().Deserialize<FileContent>(reader);

			Console.WriteLine(JsonConvert.SerializeObject(content, new JsonSerializerSettings
			{
				Formatting = Formatting.Indented,
				TypeNameHandling = TypeNameHandling.All
			}));
			Console.ReadKey();
		}
	}

	class TreeNodeConverter : JsonConverter
	{
		public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
		{
			serializer.Serialize(writer, value);
		}

		public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
		{
			var array = JArray.Load(reader);
			var target = new List<ITreeNode>();

			
			if (array.HasValues)
			{
				foreach (var child in array.Children())
				{
					var node = CreateNode(child);
					
					serializer.Populate(child.CreateReader(), node);
					target.Add(node);
				}
			}
			return target;
		}

		private ITreeNode CreateNode(JToken obj)
		{
			var type = (string)obj["type"];

			switch (type)
			{
				case "Folder":
					return new Folder();
				case "File":
					return new File();
				default:
					throw new NotSupportedException();

			}
		}

		public override bool CanConvert(Type objectType)
		{
			return false;
		}
	}

	class FileContent
	{
		[JsonConverter(typeof(TreeNodeConverter))]
		public List<ITreeNode> Roots { get; set; }
	}

	interface ITreeNode
	{
		string Name { get; set; }
	}

	class Folder : ITreeNode
	{
		public string Name { get; set; }

		[JsonConverter(typeof(TreeNodeConverter))]
		public List<ITreeNode> NestedObjects { get; set; }
	}

	class File : ITreeNode
	{
		public string Name { get; set; }

		public int Size { get; set; }

		public string FileType { get; set; }
	}
}
