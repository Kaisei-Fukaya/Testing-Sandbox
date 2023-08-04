using System.Diagnostics;
using UnityEngine;

namespace SSL.Data.Utils
{
    public class Preprocess
    {
        public static string tmp_path = $"{GAGenDataUtils.BasePath}Editor/Main/GraphicalAssetGeneratorData/Preprocessing/tmp";
        public static string PreprocessImage(string imagePath, string tmpID)
        {
            string toolPath = $"{Application.dataPath}/Scripts/SwordShapeLanguage/Editor/Main/GraphicalAssetGeneratorData/Preprocessing/preprocess_for_unity1file.exe";
            string arguments = $"--image_path {imagePath} --image_temp_path {tmp_path}/{tmpID}";
            
            Process process = new Process();
            process.StartInfo.FileName = toolPath;
            process.StartInfo.Arguments = arguments;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();

            // Read the output of the executable
            string result = process.StandardOutput.ReadToEnd();

            process.WaitForExit();

            return result;
        }
    }
}