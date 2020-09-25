using Puppeteer.Core;
using Puppeteer.Core.Configuration;
using Puppeteer.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal static class PuppeteerEditorHelper
	{
		public static bool AddOrUpdateInList<TListItem, TDesc>(List<TDesc> _serialisedList, ListItem _item) where TDesc : BasicDescription where TListItem : BasicListItem<TDesc>
		{
			if (!_item.IsMarkedForUnsavedChanges())
			{
				return false;
			}
			if (_item is TListItem selectedListItem)
			{
				TDesc selectedDescription = selectedListItem.GetDescription();
				Guid selectedGUID = selectedDescription.GUID;

				TDesc savedSelected = _serialisedList.Find(_entry => _entry.GUID == selectedGUID);
				if (savedSelected != null)
				{
					savedSelected = selectedDescription;
				}
				else
				{
					_serialisedList.Add(selectedDescription);
				}

				return true;
			}

			return false;
		}

		public static int Append<T>(ref T[] _array, T _addedElement)
		{
			if (_array == null)
			{
				_array = new T[0];
			}

			int currentLength = _array.Length;
			Array.Resize(ref _array, currentLength + 1);
			_array[currentLength] = _addedElement;

			return currentLength;
		}

		public static void Apply(ref this Rect _rect, ref Rect _application)
		{
			_rect.x += _application.x;
			_rect.width += _application.width;
			_rect.y += _application.y;
			_rect.height -= _application.height;
		}

		public static void DeleteWorldStateItem<T>(PuppeteerView _view, BasicListItem<T> _selectedListItem,
			ref WorldStateDescription[] _worldStateDescriptions, WorldStateItem _worldStateItem, Action<WorldStateItem> _selectNeighbourFunc) where T : BasicDescription
		{
			_selectNeighbourFunc?.Invoke(_worldStateItem);
			_worldStateItem.RemoveFromHierarchy();

			Remove(ref _worldStateDescriptions, _worldStateItem.GetWorldStateDescription());

			_selectedListItem.MarkUnsavedChanges(true);
			_view.AddUnsavedMarker();
		}

		public static List<Type> GetActionTypes()
		{
			return TypeCache.GetTypesDerivedFrom<Core.PuppeteerExecutableAction>().ToList();
		}

		public static List<Guid> GetArchetypeSelectorGUIDs()
		{
			List<Guid> archetypes = new List<Guid>
			{
				Guid.Empty
			};

			foreach (var item in PuppeteerManager.Instance.GetArchetypeDescriptions())
			{
				archetypes.Add(item.GUID);
			}

			return archetypes;
		}

		public static List<Type> GetSensorTypes()
		{
			return TypeCache.GetTypesDerivedFrom<Core.PuppeteerExecutableSensor>().ToList();
		}

		public static Dictionary<UtilityOperators, string> GetUtilityOperatorStringPairs()
		{
			return OPERATOR_TO_STRING_DICT;
		}

		public static string GetVersion()
		{
			return "Version 1.0.0";
		}

		public static void Remove<T>(ref T[] _array, T _removedElement)
		{
			T[] newArray = new T[_array.Length - 1];
			for (int workingIt = 0, newIt = 0; workingIt < _array.Length; ++workingIt)
			{
				if (!_array[workingIt].Equals(_removedElement))
				{
					newArray[newIt++] = _array[workingIt];
				}
			}

			_array = newArray;
		}

		public static int RemoveDeletedItems<TListItem, TDesc>(List<TDesc> _serialisedList, List<ListItem> _workingList) where TDesc : BasicDescription where TListItem : BasicListItem<TDesc>
		{
			return _serialisedList.RemoveAll(_savedListEntry =>
			{
				return !_workingList.Any(_workingListEntry =>
				{
					if (_workingListEntry is TListItem workingListItem)
					{
						return workingListItem.GetDescription().GUID == _savedListEntry.GUID;
					}
					else
					{
						return false;
					}
				});
			});
		}

		public static void ReplaceArchetypeSelectorField(ref ArchetypeSelectorField _archetypeField, Guid _targetAgentGUID, EventCallback<ChangeEvent<Guid>> _changeEvent)
		{
			var parent = _archetypeField.parent;
			var classes = _archetypeField.GetClasses();

			_archetypeField.RemoveFromHierarchy();

			// Unity doesn't allow access to the choices in popupFields. Therefore we have to create a new field with the updated entries.
			_archetypeField = new ArchetypeSelectorField(_archetypeField.label) { name = _archetypeField.name };
			parent.Add(_archetypeField);

			foreach (var ussClass in classes)
			{
				_archetypeField.AddToClassList(ussClass);
			}

			_archetypeField.SetValueWithoutNotify(_targetAgentGUID);
			_archetypeField.RegisterValueChangedCallback(_changeEvent);
			_archetypeField.MarkDirtyRepaint();
		}

		public static bool UpdateDescriptionIfNecessary<TFieldValueType, TBaseFieldType>(TBaseFieldType _field, ref TFieldValueType _targetValue)
			where TBaseFieldType : BaseField<TFieldValueType> where TFieldValueType : IEquatable<TFieldValueType>
		{
			if (!_field.value.Equals(_targetValue))
			{
				_targetValue = _field.value;
				return true;
			}
			return false;
		}

		public static void UpdateSelectedWorldStateItem(ref WorldStateItem _selectedWorldStateItem, WorldStateItem _newWorldStateItem)
		{
			if (_selectedWorldStateItem != null)
			{
				_selectedWorldStateItem.RemoveFromClassList("selectedWorldStateItem");
				if (_newWorldStateItem == _selectedWorldStateItem)
				{
					_selectedWorldStateItem = null;
					return;
				}
			}
			_selectedWorldStateItem = _newWorldStateItem;
			_newWorldStateItem?.AddToClassList("selectedWorldStateItem");
		}

		public static string UtilityOperatorToString(UtilityOperators _operator)
		{
			return OPERATOR_TO_STRING_DICT[_operator];
		}

		private static readonly Dictionary<UtilityOperators, string> OPERATOR_TO_STRING_DICT = new Dictionary<UtilityOperators, string>
		{
			{ UtilityOperators.PLUS_EQUALS, "+=" },
			{ UtilityOperators.MINUS_EQUALS, "-=" },
			{ UtilityOperators.MULTIPLY_EQUALS, "*=" },
			{ UtilityOperators.DIVIDE_EQUALS, "/=" },
			{ UtilityOperators.MODULO_EQUALS, "%=" },
		};
	}
}