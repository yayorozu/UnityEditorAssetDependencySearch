using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Yorozu.EditorTool.Dependency
{
	public class DependencyWindow : EditorWindow
	{
		[MenuItem("Tools/DependencySearch")]
		private static void ShowWindow()
		{
			DependencyWindow window = GetWindow<DependencyWindow>();
			window.titleContent = new GUIContent("DependencyWindow");
			window.Show();
		}

		[SerializeField]
		private DependencyInfo _info = new DependencyInfo();
		internal DependencyInfo Info => _info;

		[SerializeReference]
		private ModuleAbstract[] _modules;

		private int _currentIndex;
		private ModuleAbstract Current => _modules[_currentIndex];

		private void OnEnable()
		{
			if (_modules != null)
				return;

			_modules = AppDomain.CurrentDomain
				.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(t => t.IsSubclassOf(typeof(ModuleAbstract)))
				.Select(t => Activator.CreateInstance(t, true) as ModuleAbstract)
				.ToArray();

			foreach (var module in _modules)
				module.SetWindow(this);

			if (_modules.Length > 0)
				Current.OnActive();
		}

		private void OnGUI()
		{
			var rect = new Rect(0, 0, position.width, position.height);
			using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
			{
				if (DependencyXML.FileExists)
				{
					if (GUILayout.Button("Update", EditorStyles.toolbarButton))
					{
						_info.CreateDependencies();
						Current.OnActive();
					}
					if (GUILayout.Button("Rebuild", EditorStyles.toolbarButton))
					{
						_info.CreateDependencies(true);
						Current.OnActive();
					}
				}
				else
				{
					if (GUILayout.Button("Create", EditorStyles.toolbarButton))
					{
						_info.CreateDependencies();
						Current.OnActive();
					}
				}

				GUILayout.FlexibleSpace();
				GUILayout.Label("機能");
				using (var check = new EditorGUI.ChangeCheckScope())
				{
					_currentIndex = GUILayout.Toolbar(_currentIndex, _modules.Select(m => m.TabName).ToArray(), EditorStyles.toolbarButton);
					if (check.changed)
						Current.OnActive();
				}
			}

			rect.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

			using (new GUILayout.AreaScope(rect))
			{
				Current.Draw();
			}
		}
	}
}
