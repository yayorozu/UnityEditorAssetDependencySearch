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

		private string[] _filters =
		{
			"Scene",
			"Prefab",
			"ScriptableObject",
			"Model",
			"Material",
			"AnimatorController",
			"Animation",
			"MonoScript",
			"Texture",
			"SpriteAtlas",
		};

		internal static bool FileExists => File.Exists(PATH);

		internal static DependencyXML Load()
		{
			var xml = new DependencyXML();
			try
			{
				if (File.Exists(PATH))
				{
					var serializer = new DataContractSerializer(typeof(DependencyXML));
					using (var xr = XmlReader.Create(PATH))
					{
						xml = (DependencyXML) serializer.ReadObject(xr);
					}

					foreach (var key in xml.Keys)
					{
						var path = AssetDatabase.GUIDToAssetPath(key);
						if (string.IsNullOrEmpty(path))
							xml.Remove(key);
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

			var filter = string.Join(" ", _filters.Select(f => $"t:{f}").ToArray());
			var findAssets = AssetDatabase.FindAssets(filter, new []{"Assets"});

			void Display(int count)
			{
				EditorUtility.DisplayProgressBar(
					"Processing",
					$"Create Dependency Map ({count} / {findAssets.Length})",
					count / (float) findAssets.Length
				);
			}

			Display(0);

			try
			{
				var divide = Mathf.Max(50, Mathf.FloorToInt(findAssets.Length / 100f));
				for (var i = 0; i < findAssets.Length; i++)
				{
					if (i % divide == 0)
						Display(i);

					CreateDependency(findAssets[i], isForce);
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

			var serializer = new DataContractSerializer(typeof(DependencyXML));
			var settings = new XmlWriterSettings();
			settings.Encoding = new UTF8Encoding(false);

			try
			{
				using (var xw = XmlWriter.Create(PATH, settings))
				{
					serializer.WriteObject(xw, this);
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
			{
				return true;
			}

			return !type.IsSubclassOf(typeof(MonoBehaviour));
		}
	}
}
