using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Yorozu.EditorTool.Dependency
{
	/// <summary>
	/// 指定ファイルの依存ファイルを探す
	/// </summary>
	[Serializable]
	internal class SearchDependencyModule : ModuleAbstract
	{
		private string targetPath;
		private List<string> dependencyGUIDs = new List<string>();
		private Vector2 _scrollPosition;

		internal override string TabName => "Dependency Search";

		internal override void OnActive()
		{
		}

		internal override void Draw()
		{
			CheckDragAndDrop(new Rect(0, 0, Window.position.width, Window.position.height));

			if (string.IsNullOrEmpty(targetPath))
			{
				EditorGUILayout.HelpBox("Drag Search Dependency Asset", MessageType.Info);
				return;
			}

			EditorGUILayout.LabelField(targetPath, EditorStyles.boldLabel);

			// 使っているファイルがない場合
			if (dependencyGUIDs.Count <= 0)
			{
				using (new GUILayout.HorizontalScope())
				{
					GUILayout.FlexibleSpace();
					EditorGUILayout.LabelField("Dependency not found");
					GUILayout.FlexibleSpace();
				}

				return;
			}

			EditorGUILayout.Space();

			using (var scroll = new GUILayout.ScrollViewScope(_scrollPosition))
			{
				_scrollPosition = scroll.scrollPosition;
				foreach (var guid in dependencyGUIDs)
				{
					var path = AssetDatabase.GUIDToAssetPath(guid);
					if (GUILayout.Button(path, EditorStyles.label))
					{
						EditorGUIUtility.PingObject(AssetDatabase.LoadAssetAtPath<Object>(path));
					}
				}
			}
		}

		private void CheckDragAndDrop(Rect rect)
		{
			var ev = Event.current;
			if (ev.type == EventType.DragUpdated || ev.type == EventType.DragPerform)
			{
				if (DragAndDrop.paths.Length <= 0)
					return;

				DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
				if (ev.type == EventType.DragPerform)
				{
					if (rect.Contains(ev.mousePosition))
					{
						targetPath = DragAndDrop.paths[0];
						dependencyGUIDs = Window.Info.FindAssetReference(AssetDatabase.AssetPathToGUID(targetPath));
					}

					DragAndDrop.activeControlID = 0;
					DragAndDrop.AcceptDrag();
					GUI.changed = true;
				}
				else
				{
					DragAndDrop.activeControlID = GUIUtility.GetControlID(FocusType.Passive);
				}
				Event.current.Use();
			}
			else if (ev.type == EventType.DragExited)
			{
				if (GUI.changed)
					HandleUtility.Repaint();
			}
		}
	}
}
