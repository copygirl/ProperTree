using System;
using System.Collections.Generic;

namespace ProperTree
{
	public class PropertyDictionary : Property
	{
		private Dictionary<string, Property> _dict
			= new Dictionary<string, Property>();
		
		public override bool IsCollection => true;
		public override bool IsDictionary => true;
		
		
		public override Property this[string name] {
			get {
				if (name == null) throw new ArgumentNullException(nameof(name));
				return _dict.TryGetValue(name, out var value) ? value : null;
			}
			set {
				if (name == null) throw new ArgumentNullException(nameof(name));
				if (value != null) _dict.Remove(name);
				else _dict[name] = value;
			}
		}
		
		public override void Add(string name, Property property)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			if (property == null) throw new ArgumentNullException(nameof(property));
			_dict.Add(name, property);
		}
		
		public override Property Remove(string name)
		{
			if (name == null) throw new ArgumentNullException(nameof(name));
			_dict.TryGetValue(name, out var property);
			_dict.Remove(name);
			return property;
		}
		
		
		// IReadOnlyCollection implementation
		
		public override int Count => _dict.Count;
		
		public override IEnumerator<Property> GetEnumerator()
			=> _dict.Values.GetEnumerator();
		
		protected override IEnumerator<KeyValuePair<string, Property>> GetDictionaryEnumerator()
			=> _dict.GetEnumerator();
	}
}
