using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.IO;
using System.Text;
using System.Collections.Generic;
namespace Frame
{
    public static class MenuOption
    {
        [UnityEditor.MenuItem("Style/VersionStyle", false, 1)]
        public static VersionStyle EditorCreateVersionStyle()
        {
            VersionStyle vs = EditorPath.GetVersionStyle();
            Selection.activeObject = vs;
            return vs;
        }
        [UnityEditor.MenuItem("Style/MapDataStyle", false, 1)]
        public static void EditorCreateMapDataStyle()
        {
            Selection.activeObject = EditorPath.mapDataStyle;
        }

        [UnityEditor.MenuItem("Style/VersionManager_PrintFileInfo", false, 1)]
        public static void PrintFileInfo()
        {
            VersionManager.PrintFileInfo();
        }

        [MenuItem("Assets/Create/Scriptalbe/VersionStyle", false, 100)]
        public static VersionStyle CreateVersionStyle()
        {
            return AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<VersionStyle>();
        }

        [MenuItem("Assets/Create/Scriptalbe/MapDataStyle", false, 100)]
        public static MapDataStyle Create_MapDataStyle()
        {
            return AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<MapDataStyle>();
        }
        //[MenuItem("Assets/Create/Scriptalbe/SpriteAsset", false, 100)]
        //public static SpriteAsset Create_SpriteAsset()
        //{
        //    SpriteAsset s = AssetDatabaseX.CreateNewAssetInCurrentProjectFolder<SpriteAsset>();
        //    //s.name = "sdkStyle.asset";
        //    return s;
        //}


        ///////////////////////////////OpenScene///////////////////////////////
        [MenuItem("Scene/Game")]
        public static void OpenScene_MainApp()
        {
            string path = "Assets/Game.unity";
            if (!File.Exists(path))
            {
                AssetDatabase.CopyAsset("Assets/Game.unity", path);
                EditorCreateVersionStyle();
            }
            EditorSceneManager.OpenScene(path);
        }
        [MenuItem("Scene/shaderTest")]
        public static void OpenScene_WorldMapTest()
        {
            EditorSceneManager.OpenScene("Assets/Scene/shaderTest.unity");
        }
        [MenuItem("Scene/shamoEditor")]
        public static void OpenSceneshamoEditor()
        {
            EditorSceneManager.OpenScene("Assets/BundleEditing/Scene/shamoEditor.unity");
        }
        [MenuItem("Scene/shamo_runtime")]
        public static void OpenScene_shamo_runtime()
        {
            EditorSceneManager.OpenScene("Assets/BundleResources/Scene/shamo_runtime.unity");
        }
        //[MenuItem("Scene/shamo")]
        //public static void OpenScene()
        //{
        //    MapWindow.OpenScene(null);
        //}
        [MenuItem("Scene/role")]
        public static void OpenSceneRole()
        {
            EditorSceneManager.OpenScene("Assets/Scene/role.unity");
        }
        [MenuItem("Scene/uiEditor")]
        public static void OpenuiEditor()
        {
            EditorSceneManager.OpenScene("Assets/Scene/uiEditor.unity");
        }
    }
}