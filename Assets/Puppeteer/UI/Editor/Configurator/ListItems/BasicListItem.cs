using Puppeteer.Core.Configuration;
using UnityEngine.UIElements;

namespace Puppeteer.UI
{
	internal class BasicListItem<TDesc> : ListItem where TDesc : BasicDescription
	{
		public BasicListItem(TDesc _description, bool _useDeleteContextManipulator = true, bool _useDuplicateManipulator = true)
		{
			m_Description = _description;
			m_Text = _description.DisplayName;
			Init();

			if (_useDeleteContextManipulator)
			{
				this.AddManipulator(new ContextualMenuManipulator(_menubuilder =>
				{
					_menubuilder.menu.AppendAction("Delete", _dropDownMenuAction => OnDelete?.Invoke(this), DropdownMenuAction.Status.Normal);
				}));
			}
			
			if (_useDuplicateManipulator)
			{
				this.AddManipulator(new ContextualMenuManipulator(_menubuilder =>
				{
					_menubuilder.menu.AppendAction("Duplicate", _dropDownMenuAction => OnDuplicate?.Invoke(this), DropdownMenuAction.Status.Normal);
				}));
			}
		}

		public TDesc GetDescription()
		{
			return m_Description;
		}

		public System.Action<ListItem> OnDelete;
		public System.Action<ListItem> OnDuplicate;
		private TDesc m_Description;
	}
}