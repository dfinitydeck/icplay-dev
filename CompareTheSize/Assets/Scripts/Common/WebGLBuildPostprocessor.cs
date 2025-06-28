#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class WebGLBuildPostprocessor : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.WebGL)
        {
            // Modify emcc compilation parameters
            PlayerSettings.WebGL.emscriptenArgs = "-s ASYNCIFY=1 -s \"ASYNCIFY_IMPORTS=['MyAsyncFunction']\"";
        }
    }
}
#endif