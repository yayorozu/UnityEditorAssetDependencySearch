using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	/// <summary>
	/// 不要リソース表示
	/// </summary>
	[Serializable]
	internal class NoDependencyModule : ModuleAbstract
	{
		internal override string TabName => "Pick No Dependency Assets";

		private readonly List<string> _deleteGUIDs;

		[SerializeField]
		private TreeViewState _state;
		private ModuleTreeView _treeView;

		internal NoDependencyModule()
		{
			_deleteGUIDs = new List<string>();
			_state = new TreeViewState();
			_treeView = new ModuleTreeView(_state, this);
		}

		internal override void OnActive()
		{
			_treeView.SetGUIDs(Window.Info.GetNoDependenciesFiles());
		}

		internal override void Draw()
		{
			if (_treeView == null)
			{
				_treeView = new ModuleTreeView(_state, this);
				_treeView.SetGUIDs(Window.Info.GetNoDependenciesFiles());
			}

			var pos = new Rect(0, 0, Window.position.width, Window.position.height);
			pos.height -= EditorGUIUtility.singleLineHeight * 2 + 2f;

			_treeView.OnGUI(pos);

			pos.y += pos.height;
			pos.height = EditorGUIUtility.singleLineHeight;

			using (new EditorGUI.DisabledScope(_deleteGUIDs.Count <= 0))
			{
				pos.width /= 2f;
				if (GUI.Button(pos, "Delete Select Assets"))
				{
					if (EditorUtility.DisplayDialog(
						"Delete Select Assets?",
						string.Join("\n", _deleteGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray()),
						"Yes",
						"No")
					)
					{
						DeleteAssets();
					}
				}

				pos.x += pos.width;

				if (GUI.Button(pos, "Delete & Export UnityPackage"))
				{
					var exportPath = EditorUtility.SaveFilePanel("Select UnityPackage Save Directory", "Assets", "DeleteAssets", "unitypackage");

					if (!string.IsNullOrEmpty(exportPath))
					{
						AssetDatabase.ExportPackage(_deleteGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray(), exportPath);
						DeleteAssets();
					}
				}
			}
		}

		/// <summary>
		/// アセット削除
		/// </summary>
		private void DeleteAssets()
		{
			Window.Info.Removes(_deleteGUIDs);
			_deleteGUIDs.Clear();
			OnActive();
		}

		private class ModuleTreeView : TreeView
		{
			private List<string> _noDependencies;
			private NoDependencyModule _module;

			public ModuleTreeView(TreeViewState state, NoDependencyModule module) : base(state)
			{
				_module = module;
				showBorder = true;
				showAlternatingRowBackgrounds = true;
			}

			internal void SetGUIDs(List<string> getNoDependenciesFiles)
			{
				_noDependencies = getNoDependenciesFiles;
				Reload();
			}

			protected override TreeViewItem BuildRoot()
			{
				var root = new TreeViewItem(-1, -1);
				root.children = _noDependencies
					.Select((guid, i) => new TreeViewItem(i + 1, 0, guid))
					.ToList();

				return root;
			}

			protected override void RowGUI(RowGUIArgs args)
			{
				var rect = args.rowRect;
				var width = args.rowRect.width;
				var guid = args.item.displayName;
				var path = AssetDatabase.GUIDToAssetPath(args.item.displayName);

				rect.width = 15f;
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					var isDelete = _module._deleteGUIDs.Contains(guid);
					isDelete = EditorGUI.Toggle(rect, isDelete);
					if (check.changed)
					{
						if (isDelete)
							_module._deleteGUIDs.Add(guid);
						else
							_module._deleteGUIDs.Remove(guid);
					}
				}

				rect.x += rect.width;
				rect.width = rect.height;

				GUI.DrawTexture(rect, GetIconTexture(path));

				rect.x += rect.width;
				rect.width = width - rect.x;

				if (GUI.Button(rect, path, EditorStyles.label))
				{
					EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path));
				}
			}
		}
	}
}
