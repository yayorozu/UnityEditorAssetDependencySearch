using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Yorozu.EditorTool.Dependency
{
	[Serializable]
	internal class AssetTreeViewState : TreeViewState
	{
		private List<string> _deleteGUIDs;
		internal IEnumerable<string> DeleteGUIDs => _deleteGUIDs
			.Where(guid =>
			{
				var path = AssetDatabase.GUIDToAssetPath(guid);
				return !AssetDatabase.IsValidFolder(path);
			})
		;

		internal AssetTreeViewState()
		{
			_deleteGUIDs = new List<string>();
		}

		internal void Clear()
		{
			_deleteGUIDs.Clear();
		}

		internal bool Contains(AssetTreeViewItem item)
		{
			return _deleteGUIDs.Contains(item.guid);
		}

		internal void Add(AssetTreeViewItem item)
		{
			Recursive(item, true);
		}

		private void Recursive(AssetTreeViewItem root, bool isAdd)
		{
			void Control(string guid)
			{
				if (isAdd)
				{
					if (!_deleteGUIDs.Contains(guid))
						_deleteGUIDs.Add(guid);
				}
				else if (_deleteGUIDs.Contains(guid))
					_deleteGUIDs.Remove(guid);
			}

			Control(root.guid);
			if (!root.hasChildren)
				return;

			foreach (var child in root.children)
			{
				var item = child as AssetTreeViewItem;
				Recursive(item, isAdd);
			}
		}

		internal void Remove(AssetTreeViewItem item)
		{
			Recursive(item, false);
		}
	}
}
