using System;
using System.ComponentModel;
using System.Reflection;
using FastXamlServices.Internal;

namespace FastXamlServices.MetadataProviderDynamic
{
	class Element
	{
		public string Name { get; }
		public Type Type { get; }

		public PropertyInfo _pi;

		public Element(PropertyInfo pi)
		{
			_pi = pi;
			Name = pi.Name;
			Type = pi.PropertyType;
			DefaultValueAttribute = pi.GetCustomAttribute<DefaultValueAttribute>(true);
		}

		public DefaultValueAttribute DefaultValueAttribute;

		private Func<object, object> _getter;
		private Action<object, object> _setter;

		public object GetValue(object instance)
		{
			if (_getter == null)
			{
				try
				{
					_getter = (Func<object, object>)_pi.GetGetMethod().CompileObject();
				}
				catch
				{
					_getter = i => _pi.GetValue(i);
				}
			}
			return _getter(instance);
		}

		public void SetValue(object instance, object value)
		{
			if (_setter == null)
			{
				try
				{
					_setter = (Action<object, object>)_pi.GetSetMethod().CompileObject();
				}
				catch
				{
					_setter = (i, v) => _pi.SetValue(i, v);
				}
			}
			_setter(instance, value);
		}


	}
}