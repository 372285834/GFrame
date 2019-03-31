using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CompilerOptionsEditorScript
{
    static bool waitingForStop = false;
    public static bool LockReload = false;

    static CompilerOptionsEditorScript()
    {
        EditorApplication.update += OnEditorUpdate;
    }

    static void OnEditorUpdate()
    {
        if (EditorApplication.isCompiling)
        {
            if (!waitingForStop && EditorApplication.isPlaying)
            {
                EditorApplication.LockReloadAssemblies();
                EditorApplication.playmodeStateChanged
                     += PlaymodeChanged;
                waitingForStop = true;
            }
            //if(LockReload)
            //{
            //    EditorApplication.LockReloadAssemblies();
            //    LockReload = false;
            //}
            //else
            //{
            //    EditorApplication.UnlockReloadAssemblies();
            //}
        }
        
    }

    static void PlaymodeChanged()
    {
        if (EditorApplication.isPlaying)
            return;

        EditorApplication.UnlockReloadAssemblies();
        EditorApplication.playmodeStateChanged
             -= PlaymodeChanged;
        waitingForStop = false;
    }
}