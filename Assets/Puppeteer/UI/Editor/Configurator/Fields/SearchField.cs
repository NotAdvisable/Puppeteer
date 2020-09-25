using System;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class SearchField : SearchFieldBase<TextField, string>
	{
		protected override void ClearTextField()
		{
			value = String.Empty;
		}

		protected override bool FieldIsEmpty(string _fieldValue)
		{
			return string.IsNullOrEmpty(_fieldValue);
		}
	}
}