using System;
using System.Collections.Generic;

namespace Yorozu.EditorTool.Dependency
{
	[Serializable]
	public class DependencyData
	{
		public List<string> GUIDs = new List<string>();
		public DateTime Timestamp;
	}
}
