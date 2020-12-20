using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	[Serializable]
	internal abstract class ModuleAbstract
	{
		protected DependencyWindow Window { get; private set; }
		internal abstract string TabName { get; }

		internal abstract void OnActive();
		internal abstract void Draw();

		internal void SetWindow(DependencyWindow window)
		{
			Window = window;
		}

		protected static Texture2D GetIconTexture(string path)
		{
			try
			{
				return (Texture2D) EditorGUIUtility.Load(path);
			}
			catch
			{
				string fileName = Path.GetFileName(path);
				return UnityEditorInternal.InternalEditorUtility.GetIconForFile(fileName);
			}
		}

	}
}
