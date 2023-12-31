using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	/// <summary>
	/// PopupWindow.Show(position, new Popup(property, components));
	/// </summary>
	public class DependencyFilterPopup : PopupWindowContent
	{
		private readonly string[] _filters;
		private List<string> _actives;
		private readonly Action<List<string>> _action
			;

		public DependencyFilterPopup(string[] filters, string[] actives, Action<List<string>> applyEvent)
		{
			_filters = filters;
			_actives = new List<string>(actives);
			_action = applyEvent;
		}

		public override void OnGUI(Rect rect)
		{
			foreach (var filter in _filters)
			{
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					EditorGUILayout.Foldout(_actives.Contains(filter), filter);
					if (check.changed)
					{
						if (_actives.Contains(filter))
							_actives.Remove(filter);
						else
							_actives.Add(filter);
					}
				}
			}

			using (new EditorGUILayout.HorizontalScope())
			{
				if (GUILayout.Button("None"))
				{
					_actives.Clear();
				}
				if (GUILayout.Button("All"))
				{
					_actives = new List<string>(_filters);
				}
			}

			if (GUILayout.Button("Apply"))
			{
				_action?.Invoke(_actives);
				editorWindow.Close();
			}
		}

		public override Vector2 GetWindowSize()
		{
			return new Vector2(
				200f,
				_filters.Length * EditorGUIUtility.singleLineHeight
			);
		}
	}
}

