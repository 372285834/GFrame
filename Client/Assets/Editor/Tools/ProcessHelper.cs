using UnityEngine;
using System.Collections.Generic;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.IO;

    public static class ProcessHelper
    {
#if UNITY_EDITOR
        [UnityEditor.MenuItem("GameTools/Other/打印当前进程ID")]
        static void LogCurrentProcessId()
        {
            Debug.Log(GetCurrentProcessId());
        }
#endif

        public static int GetCurrentProcessId()
        {
            return Process.GetCurrentProcess().Id;
        }

        public static void StartProcess(string fileName, string arguments, bool waitForExit = true, string currentDirectory = "", Action<List<string>, List<string>> exitAction = null, Predicate<string> filterStandardOutput = null, Predicate<string> filterStandardError = null)
        {
            if (fileName.EndsWith(".bat") && Application.platform != RuntimePlatform.WindowsEditor)
            {
                var bash = "./" + Path.GetFileNameWithoutExtension(fileName) + ".sh";

                StartProcess("/bin/chmod", "+x " + bash, true, currentDirectory);

                fileName = "/bin/bash";
                arguments = bash + " " + arguments;
            }


#if UNITY_EDITOR
            if (waitForExit && !Application.isPlaying)
                EditorUtility.DisplayProgressBar("Hold on", fileName + " " + arguments, 0.5f);
#endif

            if (string.IsNullOrEmpty(currentDirectory))
                currentDirectory = Application.dataPath.Replace("Assets", "");

            var lastDirectory = Directory.GetCurrentDirectory();
            Directory.SetCurrentDirectory(currentDirectory);

            var processStartInfo = new ProcessStartInfo();

            processStartInfo.FileName = fileName;
            processStartInfo.Arguments = arguments;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.RedirectStandardInput = false;
            processStartInfo.RedirectStandardOutput = true;
            processStartInfo.RedirectStandardError = true;
            processStartInfo.UseShellExecute = false;

            var process = new Process();
            process.StartInfo = processStartInfo;
            process.EnableRaisingEvents = true;

            var standardOutput = new List<string>();
            process.OutputDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    standardOutput.Add(e.Data);
            };

            var standardError = new List<string>();
            process.ErrorDataReceived += (sender, e) =>
            {
                if (e.Data != null)
                    standardError.Add(e.Data);
            };

            process.Exited += (sender, e) =>
            {
                if (filterStandardOutput != null)
                    standardOutput = standardOutput.FindAll(filterStandardOutput);

                var log = "";
                foreach (var item in standardOutput)
                    log += item + "\n";
                if(!string.IsNullOrEmpty(log))
                    Debug.Log(log);

                if (filterStandardError != null)
                    standardError = standardError.FindAll(filterStandardError);

                var logError = "";
                foreach (var item in standardError)
                    logError += item + "\n";
                if (!string.IsNullOrEmpty(logError))
                    Debug.Log(logError);

                if (exitAction != null)
                    exitAction(standardOutput, standardError);
            };

            process.Start();

            Directory.SetCurrentDirectory(lastDirectory);

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (waitForExit)
            {
                process.WaitForExit();

#if UNITY_EDITOR
                if (!Application.isPlaying)
                    EditorUtility.ClearProgressBar();
#endif
            }
        }

        public static void StartProcessMac(string url,string param)
        {
            Process myCustomProcess = new Process();
            myCustomProcess.StartInfo.FileName = "osascript";
            myCustomProcess.StartInfo.Arguments = string.Format("-e 'tell application \"Terminal\" \n activate \n do script \"cd {0} && sh {1}\" \n end tell'",
                url, param);
            myCustomProcess.StartInfo.UseShellExecute = false;
            myCustomProcess.StartInfo.RedirectStandardOutput = false;
            myCustomProcess.Start();
            myCustomProcess.WaitForExit();  
        }
    }
