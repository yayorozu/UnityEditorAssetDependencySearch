using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	[Serializable]
	public class DependencyXML : Dictionary<string, DependencyData>
	{
		private static readonly string PATH = "Temp/AssetDependency.xml";

		internal static bool FileExists => File.Exists(PATH);

		/// <summary>
		/// 依存無しアセットの種類
		/// </summary>
		internal IEnumerable<string> NoDependencyAssetTypes => _noDependencyAssetTypes;
		private string[] _noDependencyAssetTypes = new string[0];

		internal static DependencyXML Load()
		{
			var xml = new DependencyXML();
			try
			{
				if (FileExists)
				{
					var serializer = new DataContractSerializer(typeof(DependencyXML));
					using (var xr = XmlReader.Create(PATH))
					{
						xml = (DependencyXML) serializer.ReadObject(xr);
					}

					var assetPaths = xml.Keys.Select(AssetDatabase.GUIDToAssetPath);
					var deleteAssets = assetPaths.Where(string.IsNullOrEmpty);

					// 削除されたファイルがあれば消す
					if (deleteAssets.Any())
					{
						foreach (var path in deleteAssets)
						{
							xml.Remove(AssetDatabase.AssetPathToGUID(path));
						}
						xml.CacheAssetType();
						WriteXML(xml);
					}
				}
			}
			catch
			{
				// ignored
			}

			return xml;
		}

		/// <summary>
		/// 依存を作成
		/// </summary>
		internal void CreateDependencies(bool isForce = false)
		{
			if (isForce)
				Clear();

			// 全アセットをなめる
			var allAssets = AssetDatabase.FindAssets("");

			void Display(int count)
			{
				EditorUtility.DisplayProgressBar(
					"Processing",
					$"Create Dependency Map ({count} / {allAssets.Length})",
					count / (float) allAssets.Length
				);
			}

			Display(0);

			try
			{
				var divide = Mathf.Max(50, Mathf.FloorToInt(allAssets.Length / 100f));
				for (var i = 0; i < allAssets.Length; i++)
				{
					if (i % divide == 0)
						Display(i);

					CreateDependency(allAssets[i], isForce);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message + "\n" + e.StackTrace);
			}
			finally
			{
				EditorUtility.ClearProgressBar();
			}

			if (Count <= 0)
				return;

			CacheAssetType();

			WriteXML(this);
		}

		/// <summary>
		/// ファイル書き出し
		/// </summary>
		private static void WriteXML(DependencyXML obj)
		{
			var serializer = new DataContractSerializer(typeof(DependencyXML));
			var settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);

			try
			{
				using (var xw = XmlWriter.Create(PATH, settings))
				{
					serializer.WriteObject(xw, obj);
				}
			}
			catch
			{
				// ignored
			}
		}

		private void CreateDependency(string guid, bool isForce)
		{
			var path = AssetDatabase.GUIDToAssetPath(guid);
			if (string.IsNullOrEmpty(path))
			{
				Remove(guid);
				return;
			}

			if (Skip(path))
				return;

			var timestamp = File.GetLastWriteTime(path);
			// 差分無し
			if (ContainsKey(guid) && !isForce && this[guid].Timestamp == timestamp)
				return;

			if (!ContainsKey(guid))
				Add(guid, new DependencyData());

			this[guid].Timestamp = timestamp;

			foreach (var dPath in AssetDatabase.GetDependencies(path, false))
			{
				if (string.IsNullOrEmpty(dPath))
					continue;

				var dGUID = AssetDatabase.AssetPathToGUID(dPath);
				if (!ContainsKey(dGUID))
					Add(dGUID, new DependencyData());

				this[dGUID].GUIDs.Add(guid);
			}
		}

		/// <summary>
		/// アセットの種類をキャッシュ
		/// </summary>
		private void CacheAssetType()
		{
			_noDependencyAssetTypes = Keys
				.Select(AssetDatabase.GUIDToAssetPath)
				.GroupBy(AssetDatabase.GetMainAssetTypeAtPath)
				.Select(g => g.Key.ToString())
				.ToArray();
		}

		/// <summary>
		/// スクリプトで Mono継承じゃないクラスはスキップ
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private bool Skip(string path)
		{
			var assetType = AssetDatabase.GetMainAssetTypeAtPath(path);
			if (assetType != typeof(MonoScript))
				return false;

			var mono = AssetDatabase.LoadAssetAtPath<MonoScript>(path);
			var type = mono.GetClass();
			if (type == null)
				return true;

			if (type.IsAbstract)
				return true;

			return !type.IsSubclassOf(typeof(MonoBehaviour));
		}
	}
}
