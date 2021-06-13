using System;
using System.IO;
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
	}
}
