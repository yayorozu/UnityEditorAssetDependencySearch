using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	[Serializable]
	internal class DependencyInfo
	{
		[SerializeField]
		private DependencyXML _map;

		internal DependencyInfo()
		{
			_map = DependencyXML.Load();
		}

		/// <summary>
		/// そのGUIDを参照しているアセットを探す
		/// </summary>
		internal List<string> FindAssetReference(string guid)
		{
			var ret = _map.ContainsKey(guid) ?
				_map[guid].GUIDs :
				new List<string>();

			return ret;
		}

		internal void CreateDependencies(bool isForce = false)
		{
			_map.CreateDependencies(isForce);
		}

		/// <summary>
		/// 指定したGUIDを削除
		/// </summary>
		internal void Removes(IEnumerable<string> guids)
		{
			foreach (var guid in guids)
			{
				if (!_map.ContainsKey(guid))
					continue;

				AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
				_map.Remove(guid);
			}
		}

		/// <summary>
		/// 依存が0のファイル一覧を取得
		/// </summary>
		internal AssetTreeViewItem GetNoDependencyTree(params string[] filters)
		{
			var paths = _map.Keys.Where(k => _map[k].GUIDs.Count <= 0)
				.Select(AssetDatabase.GUIDToAssetPath)
				// リソースフォルダとSceneは対象外
				.Where(path => !path.Contains("/Resources/") && !path.EndsWith(".unity"))
				// ファイルが消えてても対象外
				.Where(path => !string.IsNullOrEmpty(path))
			;

			// フィルタ指定があれば適応
			if (filters.Any())
			{
				paths = paths.Where(path =>
				{
					var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
					return filters.Contains(assetType.Name);
				});
			}

			var root = new AssetTreeViewItem
			{
				id = 0,
				depth = -1,
			};

			var cache = new Dictionary<string, AssetTreeViewItem>();
			cache.Add("", root);

			foreach (var path in paths)
			{
				var obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);
				if (obj == null)
					continue;

				try
				{
					var parent = CreateDirectory(path, ref cache);
					var fileName = Path.GetFileName(path);
					var guid = AssetDatabase.AssetPathToGUID(path);
					var item = new AssetTreeViewItem
					{
						id = obj.GetInstanceID(),
						displayName = fileName,
						icon = GetIconTexture(path),
						guid = guid,
					};
					parent.AddChild(item);
				}
				catch
				{
					// Fileが消えてたら例外でるかも
				}
			}

			return root;
		}

		/// <summary>
		/// Directory の TreeViewItem を作成
		/// </summary>
		/// <param name="filePath"></param>
		/// <param name="cache"></param>
		/// <returns>親のTreeViewItem</returns>
		private AssetTreeViewItem CreateDirectory(string filePath, ref Dictionary<string, AssetTreeViewItem> cache)
		{
			var builder = new System.Text.StringBuilder();
			var divide = filePath.Split('/');
			for (var i = 0; i < divide.Length - 1; i++)
			{
				var parentPath = builder.ToString();
				if (builder.Length > 0)
					builder.Append("/");
				builder.Append(divide[i]);

				var dirPath = builder.ToString();
				if (cache.ContainsKey(dirPath))
					continue;

				var dir = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(dirPath);
				if (dir == null)
					continue;

				var guid = AssetDatabase.AssetPathToGUID(dirPath);
				var item = new AssetTreeViewItem
				{
					id = dir.GetInstanceID(),
					displayName = divide[i],
					icon = GetIconTexture(dirPath),
					guid = guid,
				};

				cache[parentPath].AddChild(item);
				cache.Add(dirPath, item);
			}

			return cache[builder.ToString()];
		}

		private Texture2D GetIconTexture(string path)
		{
			return (Texture2D) AssetDatabase.GetCachedIcon(path);
		}
	}
}
