using System;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VaroniaBackOffice
{


    [ExecuteInEditMode]
    public class Ortho : EditorWindow
    {

        public enum Extension { EXR, JPG, PNG, TGA }

        public int CaptureWidth = 3840;
        public int CaptureHeight = 2160;
        public Camera CaptureCamera = null;
        public bool CaptureHDR = false;
        public bool CaptureTransparent = false;


        public string CapturePath = null;
        public string CaptureLast = null;


        public Extension CaptureExtension = Extension.JPG;


        public Texture2D.EXRFlags CaptureEXRFlags = Texture2D.EXRFlags.CompressZIP;


        public bool FilenameCustomToggle = false;
        public string FilenameCustomValue = null;

        public int GameMode = 0;





        [MenuItem("Varonia/Orthographic View")]
        public static void Ortho_()
        {
            EditorWindow window = EditorWindow.GetWindow<Ortho>(false, "Orthographic View", true);
            window.autoRepaintOnSceneChange = true;
            window.Show();
            window.Focus();


            VaroniaInfoUI ui = new VaroniaInfoUI();
            var data = EditorPrefs.GetString(Application.productName + "config", " ");
            JsonUtility.FromJsonOverwrite(data, ui);
        }

        void OnGUI()
        {
            if (CaptureCamera == null)
            {
                if (GUILayout.Button("Create Camera"))
                {
                    CaptureCamera = new GameObject("(TEMP) Ortho Camera").AddComponent<Camera>();
                    CaptureCamera.orthographic = true;
                    CaptureCamera.transform.eulerAngles = new Vector3(90, 0, -90);
                }
            }
            else
            {
                CaptureCamera.orthographicSize = EditorGUILayout.IntField("Orthographic Size", (int)CaptureCamera.orthographicSize);
                CaptureCamera.transform.position = new Vector3(0, EditorGUILayout.FloatField("Height", CaptureCamera.transform.position.y), 0);
                GameMode = EditorGUILayout.IntField("GameMode", GameMode);

                if (GUILayout.Button("Capture Screenshot", GUILayout.MinHeight(40)))
                {


#if VBO_AutoBuild

                    string GameId = "999";
                    string Ortho = EditorPrefs.GetString("VBO_OrthoSourcePath");

                    using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/GameID.txt"))
                    {
                        GameId = sr.ReadToEnd();
                    }


                    CapturePath = Ortho + "/" + GameId + "/" + GameMode;

                    if (!Directory.Exists(Ortho + "/" + GameId + "/" + GameMode))
                        Directory.CreateDirectory(Ortho + "/" + GameId);
#endif

#if !VBO_AutoBuild
                   CapturePath = Application.dataPath + "/Ortho"; 
                    if (!Directory.Exists(CapturePath))
                        Directory.CreateDirectory(CapturePath);    
#endif
                    Capture();
                }

                EditorGUILayout.Space();

                if (GUILayout.Button("Delete Camera"))
                {
                    DestroyImmediate(CaptureCamera.gameObject);

                }
            }
        }

        public void Capture()
        {

            // URP - UniversalAdditionalCameraData / LWRPAdditionalCameraData
            Type URPCameraTypeA = Type.GetType(
                "UnityEngine.Rendering.Universal.UniversalAdditionalCameraData, " +
                "Unity.RenderPipelines.Universal.Runtime",
                false, true
            );
            Type URPCameraTypeB = Type.GetType(
                "UnityEngine.Experimental.Rendering.Universal.UniversalAdditionalCameraData, " +
                "Unity.RenderPipelines.Universal.Runtime",
                false, true
            );
            Type URPCameraTypeC = Type.GetType(
                "UnityEngine.Rendering.LWRP.LWRPAdditionalCameraData, " +
                "Unity.RenderPipelines.Lightweight.Runtime",
                false, true
            );
            Type URPCameraTypeD = Type.GetType(
                "UnityEngine.Experimental.Rendering.LWRP.LWRPAdditionalCameraData, " +
                "Unity.RenderPipelines.Lightweight.Runtime",
                false, true
            );
            Type URPCameraType = URPCameraTypeA ?? URPCameraTypeB ?? URPCameraTypeC ?? URPCameraTypeD;
            Component URPCameraInstance = URPCameraType == null ? null : CaptureCamera.gameObject.GetComponent(URPCameraType);

            // HDRP - HDAdditionalCameraData
            Type HDRPCameraTypeA = Type.GetType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData, " +
                "Unity.RenderPipelines.HighDefinition.Runtime",
                false, true
            );
            Type HDRPCameraTypeB = Type.GetType(
                "UnityEngine.Experimental.Rendering.HDPipeline.HDAdditionalCameraData, " +
                "Unity.RenderPipelines.HighDefinition.Runtime",
                false, true
            );
            Type HDRPCameraType = HDRPCameraTypeA ?? HDRPCameraTypeB;
            Component HDRPCameraInstance = HDRPCameraType == null ? null : CaptureCamera.gameObject.GetComponent(HDRPCameraType);
            FieldInfo HDRPCameraBackgroundColorHDR = HDRPCameraType == null ? null : HDRPCameraType.GetField("backgroundColorHDR");
            FieldInfo HDRPCameraClearMode = HDRPCameraType == null ? null : HDRPCameraType.GetField("clearColorMode");

            // HDRP - HDAdditionalCameraData.ClearColorMode
            Type HDRPClearModeEnumA = Type.GetType(
                "UnityEngine.Rendering.HighDefinition.HDAdditionalCameraData+ClearColorMode, " +
                "Unity.RenderPipelines.HighDefinition.Runtime",
                false, true
            );
            Type HDRPClearModeEnumB = Type.GetType(
                "UnityEngine.Experimental.Rendering.HDPipeline.HDAdditionalCameraData+ClearColorMode, " +
                "Unity.RenderPipelines.HighDefinition.Runtime",
                false, true
            );
            Type HDRPClearModeEnum = HDRPClearModeEnumA ?? HDRPClearModeEnumB;
            object HDRPClearModeColor = null;
            if (HDRPClearModeEnum != null && HDRPClearModeEnum != null)
            {
                try { HDRPClearModeColor = Enum.Parse(HDRPClearModeEnum, "Color"); } catch (Exception) { }
                if (HDRPClearModeColor == null)
                {
                    try { HDRPClearModeColor = Enum.Parse(HDRPClearModeEnum, "BackgroundColor"); } catch (Exception) { }
                }
            }

            // Temporary objects
            bool captureHDR = CaptureHDR || CaptureExtension == Extension.EXR;
            bool capturePipeline = HDRPCameraInstance != null || URPCameraInstance != null;
            bool captureTransparent = CaptureTransparent && CaptureExtension != Extension.JPG;
            int targetDepth = (captureTransparent && !capturePipeline) ? 32 : 24;
            TextureFormat formatRGB = captureHDR ? TextureFormat.RGBAFloat : TextureFormat.RGB24;
            TextureFormat formatRGBA = captureHDR ? TextureFormat.RGBAFloat : TextureFormat.ARGB32;
            RenderTextureFormat formatTexture = captureHDR ? RenderTextureFormat.DefaultHDR : RenderTextureFormat.Default;
            RenderTexture target = RenderTexture.GetTemporary(CaptureWidth, CaptureHeight, targetDepth, formatTexture);
            Texture2D capture = null;

            if (capturePipeline && captureTransparent)
            {

                // Remember current settings
                object preHDRPClearColorMode = HDRPCameraInstance == null ? null : HDRPCameraClearMode == null ? null : HDRPCameraClearMode.GetValue(HDRPCameraInstance);
                object preHDRPBackgroundColorHDR = HDRPCameraInstance == null ? null : HDRPCameraBackgroundColorHDR == null ? null : HDRPCameraBackgroundColorHDR.GetValue(HDRPCameraInstance);
                CameraClearFlags preClearFlags = CaptureCamera.clearFlags;
                RenderTexture preTargetTexture = CaptureCamera.targetTexture;
                RenderTexture preActiveTexture = RenderTexture.active;
                Color preBackgroundColor = CaptureCamera.backgroundColor;

                // Modify current settings
                if (HDRPCameraInstance != null && HDRPCameraClearMode != null) HDRPCameraClearMode.SetValue(HDRPCameraInstance, HDRPClearModeColor);
                CaptureCamera.clearFlags = CameraClearFlags.Color;
                CaptureCamera.targetTexture = target;
                RenderTexture.active = target;

                // Capture screenshot with black background
                if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, Color.black);
                CaptureCamera.backgroundColor = Color.black;
                CaptureCamera.Render();
                Texture2D captureBlack = new Texture2D(CaptureWidth, CaptureHeight, formatRGB, false);
                captureBlack.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                captureBlack.Apply();

                // Capture screenshot with white background
                if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, Color.white);
                CaptureCamera.backgroundColor = Color.white;
                CaptureCamera.Render();
                Texture2D captureWhite = new Texture2D(CaptureWidth, CaptureHeight, formatRGB, false);
                captureWhite.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                captureWhite.Apply();

                // Create capture with transparency
                capture = new Texture2D(CaptureWidth, CaptureHeight, formatRGBA, false);
                for (int x = 0; x < CaptureWidth; ++x)
                {
                    for (int y = 0; y < CaptureHeight; ++y)
                    {
                        Color lColorWhenBlack = captureBlack.GetPixel(x, y);
                        Color lColorWhenWhite = captureWhite.GetPixel(x, y);
                        float alphaR = 1 + lColorWhenBlack.r - lColorWhenWhite.r;
                        float alphaG = 1 + lColorWhenBlack.g - lColorWhenWhite.g;
                        float alphaB = 1 + lColorWhenBlack.b - lColorWhenWhite.b;
                        float colorR = alphaR > 0f ? lColorWhenBlack.r / alphaR : 0f;
                        float colorG = alphaG > 0f ? lColorWhenBlack.g / alphaG : 0f;
                        float colorB = alphaB > 0f ? lColorWhenBlack.b / alphaB : 0f;
                        float alpha = (alphaR + alphaG + alphaB) / 3f;
                        Color color = new Color(colorR, colorG, colorB, alpha);
                        capture.SetPixel(x, y, color);
                    }
                }
                capture.Apply();

                // Cleanup
                DestroyImmediate(captureBlack);
                DestroyImmediate(captureWhite);

                // Revert settings
                if (HDRPCameraInstance != null && HDRPCameraBackgroundColorHDR != null) HDRPCameraBackgroundColorHDR.SetValue(HDRPCameraInstance, preHDRPBackgroundColorHDR);
                if (HDRPCameraInstance != null && HDRPCameraClearMode != null) HDRPCameraClearMode.SetValue(HDRPCameraInstance, preHDRPClearColorMode);
                RenderTexture.active = preActiveTexture;
                CaptureCamera.targetTexture = preTargetTexture;
                CaptureCamera.backgroundColor = preBackgroundColor;
                CaptureCamera.clearFlags = preClearFlags;

            }
            else
            {

                // Remember current settings
                CameraClearFlags preClearFlags = CaptureCamera.clearFlags;
                RenderTexture preTargetTexture = CaptureCamera.targetTexture;
                RenderTexture preActiveTexture = RenderTexture.active;

                // Modify current settings
                if (captureTransparent) CaptureCamera.clearFlags = CameraClearFlags.Depth;
                CaptureCamera.targetTexture = target;
                RenderTexture.active = target;

                // Capture screenshot
                CaptureCamera.Render();
                capture = new Texture2D(CaptureWidth, CaptureHeight, captureTransparent ? formatRGBA : formatRGB, false);
                capture.ReadPixels(new Rect(0f, 0f, CaptureWidth, CaptureHeight), 0, 0, false);
                capture.Apply();

                // Revert settings
                RenderTexture.active = preActiveTexture;
                CaptureCamera.targetTexture = preTargetTexture;
                if (captureTransparent) CaptureCamera.clearFlags = preClearFlags;

            }

            // Convert color space if needed
            if (QualitySettings.activeColorSpace == ColorSpace.Linear && CaptureExtension != Extension.EXR && CaptureHDR)
            {
                for (int x = 0; x < CaptureWidth; ++x)
                {
                    for (int y = 0; y < CaptureHeight; ++y)
                    {
                        Color color = capture.GetPixel(x, y);
                        capture.SetPixel(x, y, color.gamma);
                    }
                }
            }
            else if (QualitySettings.activeColorSpace == ColorSpace.Gamma && CaptureExtension == Extension.EXR)
            {
                for (int x = 0; x < CaptureWidth; ++x)
                {
                    for (int y = 0; y < CaptureHeight; ++y)
                    {
                        Color color = capture.GetPixel(x, y);
                        capture.SetPixel(x, y, color.linear);
                    }
                }
            }

            // Encode texture to data
            byte[] data = Encode(capture);
            if (data != null)
            {

                // Create filename path
                CaptureLast = GetRenamedFilename(Path.Combine(CapturePath, GetFilename()));
                string filename = Path.GetFileName(CaptureLast);

                // Write data to path
                using (FileStream stream = new FileStream(CaptureLast, FileMode.Create))
                {
                    BinaryWriter writer = new BinaryWriter(stream);
                    writer.Write(data);
                }

                // Log
                Debug.Log("Screenshot '" + filename + "' saved to '" + CapturePath + "'", this);

            }

            // Cleanup
            DestroyImmediate(capture);
            RenderTexture.ReleaseTemporary(target);

        }

        private string GetRenamedFilename(string filename)
        {
            int count = 1;
            string file = Path.GetFileNameWithoutExtension(filename);
            string extension = Path.GetExtension(filename);
            string path = Path.GetDirectoryName(filename);
            string renamed = filename;
            while (File.Exists(renamed))
            {
                string temp = string.Format("{0} ({1})", file, count++);
                renamed = Path.Combine(path, temp + extension);
            }
            return renamed;
        }

        private string GetFilename()
        {
            //string extension = "." + CaptureExtension.ToString().ToLower();
            //if (FilenameCustomToggle && !string.IsNullOrEmpty(FilenameCustomValue))
            //{
            //    if (FilenameCustomValue.ToLower().EndsWith(extension))
            //    {
            //        return FilenameCustomValue;
            //    }
            //    else
            //    {
            //        return FilenameCustomValue + extension;
            //    }
            //}
            //else
            //{
            //    return DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + extension;
            //}


            string extension = "." + CaptureExtension.ToString().ToLower();

            return SceneManager.GetActiveScene().name + "_" + CaptureCamera.orthographicSize + extension;

        }

        private byte[] Encode(Texture2D texture)
        {
            if (CaptureExtension == Extension.TGA)
            {
                MethodInfo EncodeToTGA = typeof(ImageConversion).GetMethod("EncodeToTGA", BindingFlags.Static | BindingFlags.Public);
                if (EncodeToTGA == null)
                {
                    Debug.LogError("TGA Encoder not found.");
                    return null;
                }
                else
                {
                    return (byte[])EncodeToTGA.Invoke(null, new object[] { texture });
                }
            }
            else
            {
                switch (CaptureExtension)
                {
                    default:
                    case Extension.PNG: return texture.EncodeToPNG();
                    case Extension.EXR: return texture.EncodeToEXR(CaptureEXRFlags);
                    case Extension.JPG: return texture.EncodeToJPG();
                }
            }

        }
    }
}