using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
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
		/// 依存が0のファイル一覧を取得
		/// </summary>
		internal List<string> GetNoDependenciesFiles()
		{
			return _map.Keys.Where(k => _map[k].GUIDs.Count <= 0)
				.Where(k =>
				{
					var path = AssetDatabase.GUIDToAssetPath(k);
					return !path.Contains("Resources/") && !path.EndsWith(".unity");
				})
				.ToList();
		}

		/// <summary>
		/// 指定したGUIDを削除
		/// </summary>
		internal void Removes(List<string> guids)
		{
			foreach (var guid in guids)
			{
				if (!_map.ContainsKey(guid))
					continue;

				AssetDatabase.DeleteAsset(AssetDatabase.GUIDToAssetPath(guid));
				_map.Remove(guid);
			}
		}
	}
}
