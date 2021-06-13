using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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

		[SerializeField]
		private AssetTreeViewState _state;
		private AssetTreeView _treeView;

		internal NoDependencyModule()
		{

			_state = new AssetTreeViewState();
		}

		internal override void OnActive()
		{
			_treeView = new AssetTreeView(_state, this);
			_treeView.Reload();
		}

		internal override void Draw()
		{
			if (_treeView == null)
			{
				_treeView = new AssetTreeView(_state, this);
				_treeView.Reload();
			}

			var pos = new Rect(0, 0, Window.position.width, Window.position.height);
			pos.height -= EditorGUIUtility.singleLineHeight * 2 + 2f;

			_treeView.OnGUI(pos);

			pos.y += pos.height;
			pos.height = EditorGUIUtility.singleLineHeight;

			using (new EditorGUI.DisabledScope(!_state.DeleteGUIDs.Any()))
			{
				pos.width /= 2f;
				if (GUI.Button(pos, "Delete Select Assets"))
				{

					if (EditorUtility.DisplayDialog(
						"Delete Select Assets?",
						string.Join("\n", _state.DeleteGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray()),
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
						AssetDatabase.ExportPackage(_state.DeleteGUIDs.Select(AssetDatabase.GUIDToAssetPath).ToArray(), exportPath);
						DeleteAssets();
					}
				}
			}
		}

		internal AssetTreeViewItem GetTreeView()
		{
			return Window.Info.GetNoDependencyTree();
		}

		/// <summary>
		/// アセット削除
		/// </summary>
		private void DeleteAssets()
		{
			Window.Info.Removes(_state.DeleteGUIDs);
			_state.Clear();
			OnActive();
		}
	}
}
