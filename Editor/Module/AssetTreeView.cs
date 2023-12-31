using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Yorozu.EditorTool.Dependency
{
	internal class AssetTreeView : TreeView
	{
		private NoDependencyModule _module;
		private AssetTreeViewItem _root;
		private AssetTreeViewState _state;

		public AssetTreeView(AssetTreeViewState state, NoDependencyModule module) : base(state)
		{
			_state = state;
			_module = module;
			showBorder = true;
			showAlternatingRowBackgrounds = true;
			baseIndent += 15f;
			Reload();
		}

		protected override TreeViewItem BuildRoot() => new TreeViewItem(0, -1, "root");

		protected override IList<TreeViewItem> BuildRows(TreeViewItem root)
		{
			var rows = base.BuildRows(root);
			var assetRoot = _module.GetTreeView();
			if (assetRoot.hasChildren)
			{
				foreach (var child in assetRoot.children)
					rows.Add(child);
			}

			SetupDepthsFromParentsAndChildren(root);
			return rows;
		}

		protected override void RowGUI(RowGUIArgs args)
		{
			var item = args.item as AssetTreeViewItem;
			base.RowGUI(args);
			var rect = args.rowRect;
			rect.width = 15f;

			using (var check = new EditorGUI.ChangeCheckScope())
			{
				var isDelete = _state.Contains(item);
				isDelete = EditorGUI.Toggle(rect, isDelete);
				if (check.changed)
				{
					if (isDelete)
						_state.Add(item);
					else
						_state.Remove(item);
				}
			}
		}

		protected override void SelectionChanged(IList<int> selectedIds)
		{
			
			base.SelectionChanged(selectedIds);
		}

		protected override void DoubleClickedItem(int id)
		{
			EditorGUIUtility.PingObject(EditorUtility.InstanceIDToObject(id));
		}
	}
}
