using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.Core.Helper
{
	public static class ExtensionMethods
	{
		public static string ToString<TKey, TValue>(this Dictionary<TKey, TValue> _source, string _keyValueSeparator, string _sequenceSeparator)
		{
			if (_source == null)
			{
				throw new ArgumentException("Parameter source can not be null.");
			}

			var pairs = _source.Select(_x => string.Format("{0}{1}{2}", _x.Key.ToString(), _keyValueSeparator, _x.Value.ToString()));

			return string.Join(_sequenceSeparator, pairs);
		}

		public static string RelativePath(this string _absolutePath)
		{
			if (_absolutePath.StartsWith(Application.dataPath))
			{
				return "Assets" + _absolutePath.Substring(Application.dataPath.Length);
			}
			else
			{
				return null;
			}
		}

		public static T TryConvertTo<T>(this object _source) where T : class
		{
			T result = null;
			try
			{
				result = Convert.ChangeType(_source, typeof(T)) as T;
			}
			catch
			{
			}

			return result;
		}
		public static T StructConvertTo<T>(this object _source) where T : struct
		{
			T result = new T();
			try
			{
				result = (T)Convert.ChangeType(_source, typeof(T));
			}
			catch
			{
			}

			return result;
		}

		public static Vector2 LocalToParent(this VisualElement _element, Vector2 _position, VisualElement _relativeParent)
		{
			return _element.LocalToWorld(_position) - _relativeParent.LocalToWorld(Vector2.zero);
		}
	}
}