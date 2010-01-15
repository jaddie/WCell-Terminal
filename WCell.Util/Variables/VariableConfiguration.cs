using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NLog;
using WCell.Util.Strings;
using WCell.Util.Xml;
using System.Diagnostics;
using System.Threading;

namespace WCell.Util.Variables
{
	public abstract class VariableConfiguration<C, T> : VariableConfiguration<T>
		where C : VariableConfiguration<T>
		where T : TypeVariableDefinition, new()
	{
		protected VariableConfiguration(Action<string> onError)
			: base(onError)
		{
		}
	}

	public class VariableConfiguration<T> : IConfiguration
		where T : TypeVariableDefinition, new()
	{
		private static readonly Logger log = LogManager.GetCurrentClassLogger();
		protected string RootNodeName = "Config";
		private const string SettingsNodeName = "Settings";

		//private SerializablePairCollection m_definitions;
		public readonly StringTree<TypeVariableDefinition> Tree;
		public readonly List<IConfiguration> ChildConfigurations = new List<IConfiguration>();

		/// <summary>
		/// Holds an array of static variable fields
		/// </summary>
		[XmlIgnore]
		public readonly Dictionary<string, T> Definitions;

		[XmlIgnore]
		public readonly Dictionary<string, T> ByFullName =
			new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);

		[XmlIgnore]
		public Action<T> VariableDefinintionInitializor = DefaultDefinitionInitializor;

		public VariableConfiguration(Action<string> onError)
		{
			Tree = new StringTree<TypeVariableDefinition>(onError, "\t", '.');
			Definitions = new Dictionary<string, T>(StringComparer.InvariantCultureIgnoreCase);
			AutoSave = true;
		}

		public virtual string Filename
		{
			get;
			set;
		}

		//[XmlElement(DefinitionNodeName)]
		//public SerializablePairCollection Definitions
		//{
		//    get
		//    {
		//        if (m_definitions == null)
		//        {
		//            m_definitions = new SerializablePairCollection(DefinitionNodeName);
		//        }
		//        foreach (var def in ByFullName.Values)
		//        {
		//            m_definitions.Add(def.Name, def.Value.ToString());
		//        }
		//        return m_definitions;
		//    }
		//    set
		//    {
		//        m_definitions = value;
		//        InitDefs();
		//    }
		//}

		//private void InitDefs()
		//{
		//    T def;
		//    var used = new List<string>();
		//    foreach (var pair in m_definitions.Pairs)
		//    {
		//        if (!ByName.TryGetValue(pair.Key, out def))
		//        {
		//            throw new VariableException("Found invalid Variable-definition \"{0}\" with value: {1}", pair.Key, pair.Value);
		//        }
		//        if (!def.TrySet(pair.Value))
		//        {
		//            throw new VariableException("Unable to parse value of variable \"{0}\": {1}", pair.Key, pair.Value);
		//        }

		//        used.Add(def.Name);
		//        VariableDefinintionInitializor(def);
		//    }

		//    var unused = ByName.Keys.Except(used);
		//    if (unused.Count() > 0)
		//    {
		//        log.Warn("The following Config-values were not found in the config-file: " + unused.ToString(", "));
		//    }

		//    m_definitions.Pairs.Clear();
		//}

		public virtual bool AutoSave
		{
			get;
			set;
		}

		public virtual bool Load()
		{
			if (File.Exists(Filename))
			{
				Deserialize();
				return true;
			}
			return false;
			//else
			//{
			//    Save();
			//}
		}

		public void Deserialize()
		{
			XmlUtil.EnsureCulture();
			using (var reader = XmlReader.Create(Filename))
			{
				reader.ReadStartElement();
				reader.SkipEmptyNodes();

				try
				{
					Tree.ReadXml(reader);
				}
				catch (Exception e)
				{
					throw new Exception("Unable to load Configuration from: " + Filename, e);
				}
				finally
				{
					XmlUtil.ResetCulture();
				}
				//m_definitions = new SerializablePairCollection();
				//m_definitions.ReadXml(reader);
				//InitDefs();
			}
		}

		public void Save()
		{
			Save(true, false);
		}

		public bool Contains(string name)
		{
			return Definitions.ContainsKey(name);
		}

		public bool IsReadOnly(string name)
		{
			var def = GetDefinition(name);
			return def.IsReadOnly;
		}

		public virtual void Save(bool backupFirst, bool auto)
		{
			try
			{
				// don't backup empty files
				if (backupFirst && File.Exists(Filename) && new FileInfo(Filename).Length > 0)
				{
					Backup(".bak");
				}
				DoSave();
			}
			catch (Exception e)
			{
				throw new Exception("Unable to save Configuration to: " + Filename, e);
			}

			XmlUtil.EnsureCulture();
			try
			{
				foreach (var cfg in ChildConfigurations)
				{
					cfg.Save(backupFirst, auto);
				}
			}
			finally
			{
				XmlUtil.ResetCulture();
			}
		}

		private void Backup(string suffix)
		{
			var name = Filename + suffix;
			try
			{
				var file = new FileInfo(Filename);
				if (file.Length > 0)
				{
					File.Copy(Filename, name, true);
				}
			}
			catch (Exception e)
			{
				throw new Exception("Unable to create backup of Configuration \"" + name + "\"", e);
			}
		}

		private void DoSave()
		{
			using (var stream = new MemoryStream())
			{
				using (var writer = new XmlTextWriter(stream, Encoding.UTF8))
				{
					XmlUtil.EnsureCulture();
					try
					{
						writer.Formatting = Formatting.Indented;
						//writer.WriteWhitespace("\n");
						writer.WriteStartElement(RootNodeName);
						//writer.WriteWhitespace("\n\t");
						writer.WriteStartElement(SettingsNodeName);

						Tree.WriteXml(writer);

						writer.WriteEndElement();
						//writer.WriteWhitespace("\n");
						writer.WriteEndElement();
					}
					finally
					{
						XmlUtil.ResetCulture();
					}
				}

				File.WriteAllBytes(Filename, stream.ToArray());
			}
		}

		public static void DefaultDefinitionInitializor(T def)
		{
			// do nothing
		}

		public object Get(string name)
		{
			T def;
			if (Definitions.TryGetValue(name, out def))
			{
				return def.Value;
			}
			return null;
		}

		public T GetDefinition(string name)
		{
			T def;
			Definitions.TryGetValue(name, out def);
			return def;
		}

		public bool Set(string name, object value)
		{
			T def;
			if (Definitions.TryGetValue(name, out def))
			{
				def.Value = value;
				return true;
			}
			return false;
		}

		public bool Set(string name, string value)
		{
			T def;
			if (Definitions.TryGetValue(name, out def))
			{
				return def.TrySet(value);
			}
			return false;
		}

		public T CreateDefinition(string name, MemberInfo member, bool serialized, bool readOnly)
		{
			var def = new T { Name = name, Member = member, Serialized = serialized, IsReadOnly = readOnly };
			VariableDefinintionInitializor(def);
			return def;
		}

		public void AddVariablesOfAsm<A>(Assembly asm)
			where A : VariableAttribute
		{
			var types = asm.GetTypes();
			foreach (var type in types)
			{

				var members = type.GetMembers(BindingFlags.Public | BindingFlags.Static);
				InitMembers<A>(members);

				var varClassAttr = type.GetCustomAttributes(typeof(VariableClassAttribute), true).FirstOrDefault() as VariableClassAttribute;
				if (varClassAttr != null && varClassAttr.Inherit)
				{
					Type t;
					while ((t = type.BaseType) != null && !t.Namespace.StartsWith("System"))
					{
						var members2 = t.GetMembers(BindingFlags.Public | BindingFlags.Static);
						InitMembers<A>(members2);

						if (t == type.BaseType)
						{
							break;
						}
					}
				}
			}
		}

		public void Foreach(Action<IVariableDefinition> callback)
		{
			foreach (var def in Definitions.Values)
			{
				callback(def);
			}
		}

		void InitMembers<A>(MemberInfo[] members)
			where A : VariableAttribute
		{
			foreach (var member in members)
			{
				var notVarAttr = member.GetCustomAttributes<NotVariableAttribute>().FirstOrDefault();
				if (notVarAttr != null)
				{
					continue;
				}

				var varAttr = member.GetCustomAttributes(typeof(A), true).FirstOrDefault() as A;
				var readOnly = member.IsReadonly() || (varAttr != null && varAttr.IsReadOnly);

				Type memberType;
				if ((!readOnly || (varAttr != null && member.IsFieldOrProp())) &&
					((memberType = member.GetVariableType()).IsSimpleType() ||
					readOnly ||
					memberType.IsArray ||
					memberType.GetInterface(TypeVariableDefinition.GenericListType.Name) != null))
				{
					string name;
					var serialized = VariableAttribute.DefaultSerialized && !readOnly;

					if (varAttr != null)
					{
						name = varAttr.Name ?? member.Name;
						serialized = !readOnly && varAttr.Serialized;
					}
					else
					{
						name = member.Name;
					}

					Add(name, member, serialized, readOnly);
				}
				else if (varAttr != null)
				{
					throw new Exception(string.Format(
						"public static member \"{0}\" has VariableAttribute but invalid type.",
						member.GetMemberName()));
				}
			}
		}

		public T Add(string name, MemberInfo member, bool serialized, bool readOnly)
		{
			T existingDef;
			if (Definitions.TryGetValue(name, out existingDef))
			{
				throw new AmbiguousMatchException("Found Variable with name \"" + name + "\" twice (" + existingDef + "). " +
					"Either rename the Variable or add a VariableAttribute to it to specify a different name in the Configuration file. " +
				"(public static variables that are not read-only, are automatically added to the global variable collection)");
			}

			var def = CreateDefinition(name, member, serialized, readOnly);
			if (def != null)
			{
				Add(def, serialized);
			}
			return def;
		}

		public void Add(T def, bool serialize)
		{
			Definitions.Add(def.Name, def);
			ByFullName.Add(def.FullName, def);
			if (serialize)
			{
				Tree.AddChildInChain(def.FullName, def);
			}
		}
	}
}