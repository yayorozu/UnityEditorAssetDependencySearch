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
		}

		protected override TreeViewItem BuildRoot()
		{
			var root = _module.GetTreeView();
			SetupDepthsFromParentsAndChildren(root);
			return root;
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

		protected override void DoubleClickedItem(int id)
		{
			EditorGUIUtility.PingObject(EditorUtility.InstanceIDToObject(id));
		}
	}
}
