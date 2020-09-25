using Puppeteer.Core.Configuration;
using Puppeteer.Core.Helper;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	public static class ConfiguratorTypeConverter
	{
		public static VisualElement CreateFieldBasedOnType(ConfiguratorValueType _valueType, object _initialValue, string _label, System.Action _changeEventCallback)
		{
			switch (_valueType)
			{
				case ConfiguratorValueType.BOOL:
					Toggle newToggle = new Toggle(_label) { value = _initialValue.StructConvertTo<bool>() };
					newToggle.RegisterCallback<ChangeEvent<bool>>(_event => _changeEventCallback?.Invoke());
					return newToggle;

				case ConfiguratorValueType.INT:
					IntegerField newIntegerField = new IntegerField(_label) { value = _initialValue.StructConvertTo<int>() };
					newIntegerField.RegisterCallback<ChangeEvent<int>>(_event => _changeEventCallback?.Invoke());
					return newIntegerField;

				case ConfiguratorValueType.FLOAT:
					FloatField newFloatField = new FloatField(_label) { value = _initialValue.StructConvertTo<float>() };
					newFloatField.RegisterCallback<ChangeEvent<float>>(_event => _changeEventCallback?.Invoke());
					return newFloatField;

				case ConfiguratorValueType.STRING:
					string castValue = _initialValue.TryConvertTo<string>();
					TextField newTextField = new TextField(_label) { value = castValue ?? string.Empty };
					newTextField.RegisterCallback<ChangeEvent<string>>(_event => _changeEventCallback?.Invoke());
					return newTextField;

				case ConfiguratorValueType.VECTOR2:
					Vector2Field newVector2Field = new Vector2Field(_label) { value = _initialValue.StructConvertTo<Vector2>() };
					newVector2Field.RegisterCallback<ChangeEvent<Vector2>>(_event => _changeEventCallback?.Invoke());
					return newVector2Field;

				case ConfiguratorValueType.VECTOR3:
					Vector3Field newVector3Field = new Vector3Field(_label) { value = _initialValue.StructConvertTo<Vector3>() };
					newVector3Field.RegisterCallback<ChangeEvent<Vector3>>(_event => _changeEventCallback?.Invoke());
					return newVector3Field;

				case ConfiguratorValueType.VECTOR2INT:
					Vector2IntField newVector2IntField = new Vector2IntField(_label) { value = _initialValue.StructConvertTo<Vector2Int>() };
					newVector2IntField.RegisterCallback<ChangeEvent<Vector2Int>>(_event => _changeEventCallback?.Invoke());
					return newVector2IntField;

				case ConfiguratorValueType.VECTOR3INT:
					Vector3IntField newVector3IntField = new Vector3IntField(_label) { value = _initialValue.StructConvertTo<Vector3Int>() };
					newVector3IntField.RegisterCallback<ChangeEvent<Vector3Int>>(_event => _changeEventCallback?.Invoke());
					return newVector3IntField;

				default:
					return null;
			}
		}

		public static ConfiguratorValueType? GetTypeFromField(VisualElement _field)
		{
			if (_field is Toggle)
			{
				return ConfiguratorValueType.BOOL;
			}

			if (_field is FloatField)
			{
				return ConfiguratorValueType.FLOAT;
			}

			if (_field is IntegerField)
			{
				return ConfiguratorValueType.INT;
			}

			if (_field is TextField)
			{
				return ConfiguratorValueType.STRING;
			}

			if (_field is Vector2Field)
			{
				return ConfiguratorValueType.VECTOR2;
			}

			if (_field is Vector3Field)
			{
				return ConfiguratorValueType.VECTOR3;
			}

			if (_field is Vector2IntField)
			{
				return ConfiguratorValueType.VECTOR2INT;
			}

			if (_field is Vector3IntField)
			{
				return ConfiguratorValueType.VECTOR3INT;
			}

			return null;
		}

		public static ConfiguratorValueType? GetTypeFromObject(object _object)
		{
			if (_object is bool)
			{
				return ConfiguratorValueType.BOOL;
			}

			if (_object is float)
			{
				return ConfiguratorValueType.FLOAT;
			}

			if (_object is int)
			{
				return ConfiguratorValueType.INT;
			}

			if (_object is string)
			{
				return ConfiguratorValueType.STRING;
			}

			if (_object is Vector2)
			{
				return ConfiguratorValueType.VECTOR2;
			}

			if (_object is Vector3)
			{
				return ConfiguratorValueType.VECTOR3;
			}

			if (_object is Vector2Int)
			{
				return ConfiguratorValueType.VECTOR2INT;
			}

			if (_object is Vector3Int)
			{
				return ConfiguratorValueType.VECTOR3INT;
			}

			return null;
		}

		public static object GetValueFromField(VisualElement _field)
		{
			if (_field is Toggle toggle)
			{
				return toggle.value;
			}

			if (_field is FloatField floatField)
			{
				return floatField.value;
			}

			if (_field is IntegerField integerField)
			{
				return integerField.value;
			}

			if (_field is TextField textField)
			{
				return textField.value;
			}

			if (_field is Vector2Field vector2Field)
			{
				return vector2Field.value;
			}

			if (_field is Vector3Field vector3Field)
			{
				return vector3Field.value;
			}

			if (_field is Vector2IntField vector2IntField)
			{
				return vector2IntField.value;
			}

			if (_field is Vector3IntField vector3IntField)
			{
				return vector3IntField.value;
			}

			return null;
		}

		public static void SetWithoutNotifyForSpecifiedFieldType(ConfiguratorValueType _valueType, VisualElement _field, object _value)
		{
			switch (_valueType)
			{
				case ConfiguratorValueType.BOOL:
					(_field as Toggle).SetValueWithoutNotify(_value.StructConvertTo<bool>());
					break;

				case ConfiguratorValueType.INT:
					(_field as IntegerField).SetValueWithoutNotify(_value.StructConvertTo<int>());
					break;

				case ConfiguratorValueType.FLOAT:
					(_field as FloatField).SetValueWithoutNotify(_value.StructConvertTo<float>());
					break;

				case ConfiguratorValueType.STRING:
					(_field as TextField).SetValueWithoutNotify(_value.TryConvertTo<string>());
					break;

				case ConfiguratorValueType.VECTOR2:
					(_field as Vector2Field).SetValueWithoutNotify(_value.StructConvertTo<Vector2>());
					break;

				case ConfiguratorValueType.VECTOR3:
					(_field as Vector3Field).SetValueWithoutNotify(_value.StructConvertTo<Vector3>());
					break;

				case ConfiguratorValueType.VECTOR2INT:
					(_field as Vector2IntField).SetValueWithoutNotify(_value.StructConvertTo<Vector2Int>());
					break;

				case ConfiguratorValueType.VECTOR3INT:
					(_field as Vector3IntField).SetValueWithoutNotify(_value.StructConvertTo<Vector3Int>());
					break;

				default:
					break;
			}
		}
	}
}