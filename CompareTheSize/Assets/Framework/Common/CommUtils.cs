using System;
using System.IO;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Framework
{
    public class CommUtils
    {
        public const string AssetsFolderName = "Assets";
        public const int UI_LAYER_TAG = 120336759;  // UI layer tag in Lua
        public const int TOP_LAYER_TAG = 120336765;

        public static readonly DateTime UtcStartTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static int BytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset + 3] & 0xFF)
                       | ((src[offset + 2] & 0xFF) << 8)
                       | ((src[offset + 1] & 0xFF) << 16)
                       | ((src[offset + 0] & 0xFF) << 24));
            return value;
        }

        public static ulong GetMillisecondsFrom1970()
        {
            double d = (DateTime.UtcNow - CommUtils.UtcStartTime).TotalMilliseconds;
            return (ulong)d;
        }

        // public static int GetUiLayerByLuaTag(int tag)
        // {
        //     if (tag == UI_LAYER_TAG)
        //     {
        //         return (int)Framework.UIShowLevel.UI_LAYER_TAG;
        //     }
        //     else if (tag == TOP_LAYER_TAG)
        //     {
        //         return (int)Framework.UIShowLevel.TOP_LAYER_TAG;
        //     }
        //     return tag;
        // }

        // public static int convertTagByUiLayer(int nUiLayer)
        // {
        //     if (nUiLayer == (int)Framework.UIShowLevel.UI_LAYER_TAG)
        //     {
        //         return UI_LAYER_TAG;
        //     }
        //     else if (nUiLayer == (int)Framework.UIShowLevel.TOP_LAYER_TAG)
        //     {
        //         return TOP_LAYER_TAG;
        //     }
        //     return nUiLayer;
        // }


        public static uint ParseFloatToUInt(float v)
        {
            return v >= 0 ? (uint)UnityEngine.Mathf.FloorToInt(v) : 0;
        }

        public static float ClampFloatToZero(float v)
        {
            return UnityEngine.Mathf.Clamp(v, 0, v);
        }

        public static long GetTickCount()
        {
            return DateTime.Now.Ticks / 10000;              // Return milliseconds
        }

        public static string UUid()                         // Return UUID
        {
            return System.Guid.NewGuid().ToString("N");
        }

        public static string FormatToUnityPath(string path)
        {
            return path.Replace("\\", "/");
        }

        public static string FormatToSysFilePath(string path)
        {
            return path.Replace("/", "\\");
        }

        public static string FullPathToAssetPath(string full_path)
        {
            full_path = FormatToUnityPath(full_path);
            if (!full_path.StartsWith(Application.dataPath))
            {
                return null;
            }
            string ret_path = full_path.Replace(Application.dataPath, "");
            return AssetsFolderName + ret_path;
        }

        public static string GetFileExtension(string path)
        {
            return Path.GetExtension(path).ToLower();
        }

        public static string[] GetSpecifyFilesInFolder(string path, string[] extensions = null, bool exclude = false)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (extensions == null)
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories);
            }
            else if (exclude)
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => !extensions.Contains(GetFileExtension(f))).ToArray();
            }
            else
            {
                return Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                    .Where(f => extensions.Contains(GetFileExtension(f))).ToArray();
            }
        }

        public static string[] GetSpecifyFilesInFolder(string path, string pattern)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            return Directory.GetFiles(path, pattern, SearchOption.AllDirectories);
        }

        public static string[] GetAllFilesInFolder(string path)
        {
            return GetSpecifyFilesInFolder(path);
        }

        public static string[] GetAllDirsInFolder(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        }

        public static void CheckFileAndCreateDirWhenNeeded(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return;
            }

            FileInfo file_info = new FileInfo(filePath);
            DirectoryInfo dir_info = file_info.Directory;
            if (!dir_info.Exists)
            {
                Directory.CreateDirectory(dir_info.FullName);
            }
        }

        public static void CheckDirAndCreateWhenNeeded(string folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                return;
            }

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }
        }

        public static bool SafeWriteAllBytes(string outFile, byte[] outBytes)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllBytes(outFile, outBytes);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllBytes failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllLines(string outFile, string[] outLines)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllLines(outFile, outLines);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllLines failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static bool SafeWriteAllText(string outFile, string text)
        {
            try
            {
                if (string.IsNullOrEmpty(outFile))
                {
                    return false;
                }

                CheckFileAndCreateDirWhenNeeded(outFile);
                if (File.Exists(outFile))
                {
                    File.SetAttributes(outFile, FileAttributes.Normal);
                }
                File.WriteAllText(outFile, text);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeWriteAllText failed! path = {0} with err = {1}", outFile, ex.Message));
                return false;
            }
        }

        public static byte[] SafeReadAllBytes(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }

                if (!File.Exists(inFile))
                {
                    return null;
                }

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllBytes(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllBytes failed! path = {0} with err = {1}", inFile, ex.Message));
                return null;
            }
        }

        public static string[] SafeReadAllLines(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }

                if (!File.Exists(inFile))
                {
                    return null;
                }

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllLines(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllLines failed! path = {0} with err = {1}", inFile, ex.Message));
                return null;
            }
        }

        public static string SafeReadAllText(string inFile)
        {
            try
            {
                if (string.IsNullOrEmpty(inFile))
                {
                    return null;
                }

                if (!File.Exists(inFile))
                {
                    return null;
                }

                File.SetAttributes(inFile, FileAttributes.Normal);
                return File.ReadAllText(inFile);
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeReadAllText failed! path = {0} with err = {1}", inFile, ex.Message));
                return null;
            }
        }

        public static void DeleteDirectory(string dirPath)
        {
            string[] files = Directory.GetFiles(dirPath);
            string[] dirs = Directory.GetDirectories(dirPath);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(dirPath, false);
        }

        public static bool SafeClearDir(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    return true;
                }

                if (Directory.Exists(folderPath))
                {
                    DeleteDirectory(folderPath);
                }
                Directory.CreateDirectory(folderPath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeClearDir failed! path = {0} with err = {1}", folderPath, ex.Message));
                return false;
            }
        }

        public static bool SafeDeleteDir(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath))
                {
                    return true;
                }

                if (Directory.Exists(folderPath))
                {
                    DeleteDirectory(folderPath);
                }
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteDir failed! path = {0} with err: {1}", folderPath, ex.Message));
                return false;
            }
        }

        public static bool SafeDeleteFile(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                {
                    return true;
                }

                if (!File.Exists(filePath))
                {
                    return true;
                }
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeDeleteFile failed! path = {0} with err: {1}", filePath, ex.Message));
                return false;
            }
        }

        public static bool SafeRenameFile(string sourceFileName, string destFileName)
        {
            try
            {
                if (string.IsNullOrEmpty(sourceFileName))
                {
                    return false;
                }

                if (!File.Exists(sourceFileName))
                {
                    return true;
                }
                SafeDeleteFile(destFileName);
                File.SetAttributes(sourceFileName, FileAttributes.Normal);
                File.Move(sourceFileName, destFileName);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeRenameFile failed! path = {0} with err: {1}", sourceFileName, ex.Message));
                return false;
            }
        }

        public static bool SafeCopyFile(string fromFile, string toFile)
        {
            try
            {
                if (string.IsNullOrEmpty(fromFile))
                {
                    return false;
                }

                if (!File.Exists(fromFile))
                {
                    return false;
                }
                CheckFileAndCreateDirWhenNeeded(toFile);
                SafeDeleteFile(toFile);
                File.Copy(fromFile, toFile, true);
                return true;
            }
            catch (System.Exception ex)
            {
                Debug.LogError(string.Format("SafeCopyFile failed! formFile = {0}, toFile = {1}, with err = {2}",
                    fromFile, toFile, ex.Message));
                return false;
            }
        }

        public static void SearchFile(string rootPath, string fileName, string fileExt, ref string outPath)
        {
            DirectoryInfo dir = new DirectoryInfo(rootPath);
            FileInfo[] fil = dir.GetFiles();
            DirectoryInfo[] dii = dir.GetDirectories();

            foreach (FileInfo f in fil)
            {
                string name = Path.GetFileName(f.FullName.ToString());

                if (f.Extension == fileExt && name == fileName)
                {       // Must exist in the full configuration
                    outPath = f.FullName;
                    return;
                }
            }
            // Get the list of files in the subfolder, recursively traverse
            foreach (DirectoryInfo d in dii)
            {
                SearchFile(d.FullName, fileName, fileExt, ref outPath);
                if (outPath != "") return;
            }
        }

        // Download file
        public static async UniTask<string> DownTextFile(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            var operation = request.SendWebRequest();
            await operation.ToUniTask();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("cfg download failed: " + url + "\n" + request.error);
            }
            else
            {
                string txt = request.downloadHandler.text;
         //       Debug.Log("Update remote config: \n" + txt);

                return txt;
            }
            return "";
        }

        public static async UniTask<byte[]> DownBinFile(string url)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);

            var operation = request.SendWebRequest();
            await operation.ToUniTask();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to download file: " + request.error);
            }
            else
            {
                byte[] bytes = request.downloadHandler.data;
                Debug.Log("file downloaded successfully:\n" + bytes.Length);

                return bytes;
            }
            return null;
        }

        public static string GetWebUrlParam(string url, string name)
        {
            #if UNITY_EDITOR
                //url = "https://thebots.fun/0725/index.html?config=https://thebots.fun/configs/";
            #endif
            string[] urlArray = url.Split('?');
            if (urlArray.Length < 2)
            {
                return "";
            }

            string[] paramArray = urlArray[1].Split('&');
            foreach (string param in paramArray)
            {
                string[] keyValue = param.Split('=');
                if (keyValue.Length == 2 && keyValue[0] == name)
                {
                    return keyValue[1];
                }
            }
            return "";
        }

        public static string FormatTimeMMSS(float secs)
        {
            int totalSeconds = (int)Math.Floor(secs);
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
}

// export default class Utils extends FrameworkModule {

//     private static _instance: Utils = null
//     public static get instance() {
//         if (!Utils._instance) {
//             Utils._instance = new Utils()
//         }
//         return Utils._instance
//     }

//     /** Convert string to Int integer data */
//     public static toSafeInt(value: string | number, defValue?: number): number {
//         defValue = (defValue == undefined) ? 0 : defValue
//         let reValue = (value && (value.toString != undefined)) ? parseInt(value.toString() + "") : parseInt(value + "")
//         return isNaN(reValue) ? defValue : reValue
//     }

//     /** Convert string to Float floating point data */
//     public static toSafeFloat(value: string | number, defValue?: number): number {
//         defValue = (defValue == undefined) ? 0 : defValue
//         let reValue = (value && (value.toString != undefined)) ? parseFloat(value.toString() + "") : parseFloat(value + "")
//         return isNaN(reValue) ? defValue : reValue
//     }

//     /** 32-bit to high 16 and low 16 bits */
//     public static toHeighLow16(value32: number, defValue?: number): { heighValue16: number, lowValue16: number } {
//         if (typeof value32 == "number") {
//             const TWO_PWR_16_DBL = 16;
//             defValue = (defValue == undefined) ? 0 : defValue;
//             if (value32 != undefined) {
//                 if (value32 < 0) {//Signed/unsigned check
//                     return { heighValue16: (value32 >>> TWO_PWR_16_DBL), lowValue16: (value32 >>> 0) & 65535 }
//                 }
//                 else {
//                     return { heighValue16: ((value32) >> TWO_PWR_16_DBL), lowValue16: (value32) & 65535 }
//                 }
//             } else {
//                 console.warn("[Utils.toHeighLow16] this params type is undefined")
//                 return { heighValue16: defValue, lowValue16: defValue }
//             }
//         }
//         else {
//             console.warn("[Utils.toHeighLow16] this params type is not number")
//             return { heighValue16: defValue, lowValue16: defValue }
//         }
//     }

//     /** High 16 and low 16 bits to 32-bit */
//     public static heighLowTo32(heighValue16: number, lowValue16: number, defValue?: number): number {
//         if (typeof heighValue16 == "number" && typeof lowValue16 == "number") {
//             const TWO_PWR_16_DBL = 1 << 16;
//             defValue = (defValue == undefined) ? 0 : defValue;
//             if (heighValue16 != undefined && lowValue16 != undefined) {
//                 return (heighValue16 * TWO_PWR_16_DBL) | lowValue16
//             } else {
//                 console.warn("[Utils.heighLowTo32] this params type is undefined")
//                 return defValue
//             }
//         } else {
//             console.warn("[Utils.heighLowTo32] this params type is not number")
//             return heighValue16
//         }
//     }

//     // Format large numbers by units
//     public static formatBigNumToString(value: number, units: Array<number> = [100000000, 100000], str: Array<string> = ["M", "K"]): string {
//         if (value == 0) return "0"

//         let absValue = Math.abs(value)
//         let fixValue = value / absValue

//         if ((units.length === str.length) && (units.length > 0)) {
//             for (let i = 0; i < units.length; i++) {
//                 if (absValue >= units[i]) {
//                     return (Math.floor(absValue / (units[i] / 100)) * fixValue) + str[i]
//                 }
//             }
//         }

//         return (Math.floor(absValue) * fixValue) + ""
//     }

//     /**
//      * Random integer
//      * @param min
//      * @param max
//      * @returns
//      * 1.No parameters call, generates floating point random number between [0, 1).
//      * 2.One parameter n, generates integer between [1, n].
//      * 3.Two parameters, generates integer between [n, m].
//      */
//     public static randInt(min?: number, max?: number) {
//         if (min == undefined) {
//             return Math.round(Math.random())
//         }
//         else if (max != undefined) {
//             return Math.floor(Math.random() * (max - min + 1) + min)
//         }
//         else {
//             return Math.floor(Math.random() * min + 1)
//         }
//     }

//     // Random floating point number
//     public static randFloat(min: number, max: number) {
//         return Math.random() * (max - min + 1) + min;
//     }

//     // Get url parameters
//     public static getUrlParam(name: string): string {
//         let reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
//         let r = window.location.search.substring(1).match(reg);

//         if (r != undefined) return unescape(r[2]);
//         return "";
//     }

//     // String replacement

//     // ... rest of the file remains unchanged ...

// }
