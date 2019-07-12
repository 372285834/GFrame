// Version 2.1
// ©2013 Reindeer Games
// All rights reserved
// Redistribution of source code without permission not allowed

using UnityEditor;
using UnityEngine;

namespace PrimitivesPro.Editor
{
    class GridSettings : EditorWindow
    {
        private MeshEditor.Grid grid;

        public static void ShowWindow(PrimitivesPro.Editor.MeshEditor.Grid grid)
        {
            var window = EditorWindow.GetWindow(typeof (GridSettings)) as GridSettings;
            window.grid = grid;

            window.minSize = new Vector2(400, 300);
            window.position = new Rect(200, 200, window.minSize.x, window.minSize.y);
            window.ShowUtility();
        }

        private void OnGUI()
        {
            GUILayout.Space(20);

            var style = GUIStyle.none;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 16;
            style.fontStyle = FontStyle.Bold;

            Utils.Separator("Grid settings", 20, style);
            GUILayout.Space(40);

            style.fontSize = 12;
            style.fontStyle = FontStyle.Normal;
            style.alignment = TextAnchor.MiddleLeft;

            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            grid.Show = EditorGUILayout.Toggle("Show grid", grid.Show);
            GUILayout.EndHorizontal();

            if (grid.Show)
            {
                GUILayout.Space(20);
                GUILayout.BeginHorizontal();
                GUILayout.Space(30);
                grid.Snap = EditorGUILayout.Toggle("Snap on grid", grid.Snap);
                GUILayout.EndHorizontal();
            }
            else
            {
                grid.Snap = false;
            }

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            grid.Dim = EditorGUILayout.Slider("Size", grid.Dim, 1.0f, 100.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(20);
            GUILayout.BeginHorizontal();
            GUILayout.Space(30);
            grid.Size = (int)EditorGUILayout.Slider("Resolution", grid.Size, 1.0f, 100.0f);
            GUILayout.EndHorizontal();

            GUILayout.Space(40);
            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            GUILayout.Space(100);
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            GUILayout.Space(100);
            GUILayout.Space(100);
            GUILayout.EndHorizontal();
        }
    }
}
