using System;
using System.Collections.Generic;
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
        private Texture2D previewTexture = null;
        private Texture2D baseTexture = null;

        private bool isDrawing = false;
        private Vector2 lastDrawPos = Vector2.zero;
        private bool isWaitingForSecondPoint = false;
        private Vector2 firstPoint = Vector2.zero;
        private int brushSize = 8;
        private Color brushColor = Color.red;

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

        private int circleRadius = 60;

        private enum ActionPointType { Spawn, Checkpoint, Danger, Trigger }
        private readonly Dictionary<ActionPointType, Texture2D> iconContents = new Dictionary<ActionPointType, Texture2D>();


        private List<(Vector2 uv, ActionPointType type)> actionPoints = new List<(Vector2 uv, ActionPointType type)>();
        private ActionPointType currentActionPointType = ActionPointType.Spawn;


        Texture2D iconSpawn;
        Texture2D iconCheckpoint;
        Texture2D iconDanger;
        Texture2D iconTrigger;


        private string description = "";


        void OnEnable()
        {
            iconContents[ActionPointType.Spawn] = Resources.Load<Texture2D>("OrthoIcons/spawn");
            iconContents[ActionPointType.Danger] = Resources.Load<Texture2D>("OrthoIcons/warning");
            iconContents[ActionPointType.Checkpoint] = Resources.Load<Texture2D>("OrthoIcons/flag");
            iconContents[ActionPointType.Trigger] = Resources.Load<Texture2D>("OrthoIcons/trigger");

        }



        [MenuItem("Varonia/Orthographic View")]
        public static void Ortho_()
        {
            Ortho window = GetWindow<Ortho>(false, "Orthographic View", true);
            window.minSize = new Vector2(1024, 1000); // taille fixe minimale
            window.maxSize = new Vector2(1024, 1000); // taille fixe maximale (identique = taille bloquée)
            window.autoRepaintOnSceneChange = true;
            window.Show();
            window.Focus();
        }

        void OnGUI()
        {

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = 14;
            buttonStyle.fixedHeight = 40;
            buttonStyle.normal.textColor = Color.white;

            Color originalColor = GUI.backgroundColor;

            GUILayout.Space(10);
            GUILayout.BeginVertical("box");



            if (CaptureCamera == null && previewTexture == null)
            {

                GUI.backgroundColor = new Color(0.2f, 0.6f, 1f);
                GUILayout.Label("Camera Setup", EditorStyles.boldLabel);
                if (GUILayout.Button("➕ Create Ortho Camera", buttonStyle))
                {
                    CaptureCamera = new GameObject("(TEMP) Ortho Camera").AddComponent<Camera>();
                    //  CaptureCamera.gameObject.hideFlags = HideFlags.HideAndDontSave;
                    CaptureCamera.orthographic = true;
                    CaptureCamera.transform.eulerAngles = new Vector3(90, 0, -90);
                    CaptureCamera.transform.position += new Vector3(0, 3, 0);
                }

                GUI.backgroundColor = originalColor;
            }
            else if (previewTexture != null)
            {
                Preview();


                description = EditorGUILayout.TextField("📝 Description", description);

                ClearDrawing();

                SaveWithLine();

            }
            else
            {
                GUI.backgroundColor = originalColor;
                GUILayout.Space(10);
                GUILayout.Label("Camera Settings", EditorStyles.boldLabel);

                CaptureCamera.orthographicSize = EditorGUILayout.IntField("Orthographic Size", (int)CaptureCamera.orthographicSize);
                CaptureCamera.transform.position = new Vector3(0, EditorGUILayout.FloatField("Height", CaptureCamera.transform.position.y), 0);

#if VBO_AutoBuild
                GameMode = EditorGUILayout.IntField("GameMode", GameMode);
#endif

                GUILayout.Space(10);
                GUI.backgroundColor = new Color(0.1f, 0.7f, 0.1f); // vert
                if (GUILayout.Button("📸 Capture Ortho View", buttonStyle))
                {
                    SetGoodPath();
                    Capture();


                    if (CaptureCamera != null)
                    {
                        DestroyImmediate(CaptureCamera.gameObject);
                        CaptureCamera = null;
                    }
                }


                //GUILayout.Space(10);
                //GUI.backgroundColor = new Color(1f, 0.3f, 0.3f); // rouge
                //if (GUILayout.Button("🗑 Delete Camera"))
                //{
                //    DestroyImmediate(CaptureCamera.gameObject);

                //}




            }



            GUI.backgroundColor = originalColor;
            GUILayout.EndVertical();
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

                previewTexture = new Texture2D(CaptureWidth, CaptureHeight, capture.format, false);
                previewTexture.SetPixels(capture.GetPixels());
                previewTexture.Apply();

                baseTexture = new Texture2D(CaptureWidth, CaptureHeight, capture.format, false);
                baseTexture.SetPixels(capture.GetPixels());
                baseTexture.Apply();

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

                previewTexture = new Texture2D(CaptureWidth, CaptureHeight, capture.format, false);
                previewTexture.SetPixels(capture.GetPixels());
                previewTexture.Apply();


                baseTexture = new Texture2D(CaptureWidth, CaptureHeight, capture.format, false);
                baseTexture.SetPixels(capture.GetPixels());
                baseTexture.Apply();

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

        void OnDestroy()
        {
            if (CaptureCamera != null)
            {
                DestroyImmediate(CaptureCamera.gameObject);
                CaptureCamera = null;
            }
        }

        private void DrawLine(Texture2D tex, Vector2 p1, Vector2 p2, Color color, int thickness = 6)
        {
            int x0 = (int)p1.x;
            int y0 = (int)p1.y;
            int x1 = (int)p2.x;
            int y1 = (int)p2.y;

            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int sx = x0 < x1 ? 1 : -1;
            int sy = y0 < y1 ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                DrawBrush(tex, x0, y0, color, thickness);

                if (x0 == x1 && y0 == y1) break;

                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x0 += sx; }
                if (e2 < dx) { err += dx; y0 += sy; }
            }
        }



        private void DrawBrush(Texture2D tex, int cx, int cy, Color color, int radius)
        {
            for (int y = -radius; y <= radius; y++)
            {
                for (int x = -radius; x <= radius; x++)
                {
                    if (x * x + y * y <= radius * radius)
                    {
                        int px = cx + x;
                        int py = cy + y;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                            tex.SetPixel(px, py, color);
                    }
                }
            }
        }


        void Preview()
        {
            currentActionPointType = (ActionPointType)EditorGUILayout.EnumPopup("🧭 Action Point", currentActionPointType);
            circleRadius = EditorGUILayout.IntSlider("📏 Point Radius", circleRadius, 40, 400);
            brushColor = EditorGUILayout.ColorField("Brush Color", brushColor);

            GUILayout.Space(10);
            GUILayout.Label("📷 Preview (Draw Mode)", EditorStyles.boldLabel);

            float maxPreviewWidth = position.width - 40;
            float ratio = (float)previewTexture.height / previewTexture.width;
            float previewHeight = maxPreviewWidth * ratio;

            Rect previewRect = GUILayoutUtility.GetRect(maxPreviewWidth, previewHeight, GUI.skin.box);

            Event e = Event.current;

            GUI.DrawTexture(previewRect, previewTexture, ScaleMode.ScaleToFit, false);


            if (e.type == UnityEngine.EventType.KeyDown && e.keyCode == KeyCode.Escape && isWaitingForSecondPoint)
            {
                isWaitingForSecondPoint = false;
                e.Use();
                Repaint();
                return;
            }


            if (previewRect.Contains(e.mousePosition))
            {

                Vector2 uv = (e.mousePosition - new Vector2(previewRect.x, previewRect.y));
                uv.x *= (float)previewTexture.width / previewRect.width;
                uv.y = previewTexture.height - (uv.y * (float)previewTexture.height / previewRect.height); // Y inversé

                if (e.type == UnityEngine.EventType.MouseDown && e.button == 1) // clic droit
                {
                    // Enregistrer et dessiner le point
                    actionPoints.Add(new ValueTuple<Vector2, ActionPointType>(uv, currentActionPointType));
                    DrawPoint(previewTexture, uv, currentActionPointType);
                    previewTexture.Apply();
                    e.Use();
                }


                if (isWaitingForSecondPoint)
                {
                    Vector2 screenP1 = new Vector2(
                        previewRect.x + (firstPoint.x / previewTexture.width) * previewRect.width,
                        previewRect.y + (1f - (firstPoint.y / previewTexture.height)) * previewRect.height
                    );

                    Vector2 screenP2 = e.mousePosition;

                    Handles.BeginGUI();
                    Handles.color = new Color(brushColor.r, brushColor.g, brushColor.b, 0.7f);
                    Handles.DrawLine(screenP1, screenP2);
                    Handles.EndGUI();
                }





                if (e.type == UnityEngine.EventType.MouseDown && e.button == 0)
                {
                    if (!isWaitingForSecondPoint)
                    {
                        firstPoint = uv;
                        isWaitingForSecondPoint = true;
                    }
                    else
                    {
                        DrawLine(previewTexture, firstPoint, uv, brushColor, brushSize);
                        previewTexture.Apply();
                        isWaitingForSecondPoint = false;
                    }
                    e.Use();
                }
                else if (e.type == UnityEngine.EventType.MouseUp && e.button == 0)
                {
                    isDrawing = false;
                    e.Use();
                }
            }

            Repaint();
        }


        void ClearDrawing()
        {
            GUILayout.Space(10);
            GUI.backgroundColor = Color.gray;
            if (GUILayout.Button("🧽 Clear Drawing", GUILayout.Height(30)))
            {
                if (baseTexture != null)
                {
                    previewTexture.SetPixels(baseTexture.GetPixels());
                    previewTexture.Apply();
                }
            }
        }

        void SetGoodPath()
        {
#if VBO_AutoBuild

            string GameId = "999";
            string Ortho = EditorPrefs.GetString("VBO_OrthoSourcePath");

            using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/GameID.txt"))
            {
                GameId = sr.ReadToEnd();
            }


            CapturePath = Ortho + "/" + GameId + "/" + GameMode;

            if (!Directory.Exists(Ortho + "/" + GameId + "/"))
                Directory.CreateDirectory(Ortho + "/" + GameId);


            if (!Directory.Exists(Ortho + "/" + GameId + "/" + GameMode))
                Directory.CreateDirectory(Ortho + "/" + GameId + "/" + GameMode);


#endif

#if !VBO_AutoBuild
            CapturePath = Application.dataPath + "/Ortho";
            if (!Directory.Exists(CapturePath))
                Directory.CreateDirectory(CapturePath);
#endif
        }



        void SaveWithLine()
        {
            if (GUILayout.Button("💾 Save", GUILayout.Height(30)))
            {
                if (previewTexture != null && !string.IsNullOrEmpty(CaptureLast))
                {
                    // Forcer l'extension en .jpg
                    string jpgPath = Path.ChangeExtension(CaptureLast, "jpg");

                    byte[] jpgData = previewTexture.EncodeToJPG(95); // Qualité 0–100
                    File.WriteAllBytes(jpgPath, jpgData);
                    AssetDatabase.Refresh();
                    Debug.Log("✅ Drawing saved as JPG : " + jpgPath);


                    string jsonPath = Path.ChangeExtension(jpgPath, "json");
                    var jsonData = new Dictionary<string, string> { { "description", description } };
                    File.WriteAllText(jsonPath, JsonUtility.ToJson(new SerializableDescription(description), true));



                    // Nettoyage
                    CaptureLast = jpgPath;
                    previewTexture = null;
                    baseTexture = null;
                    isDrawing = false;
                    isWaitingForSecondPoint = false;

                    // Fermeture de la fenêtre
                    Close();


                }
            }
        }



        [Serializable]
        public class SerializableDescription
        {
            public string description;
            public SerializableDescription(string desc) { description = desc; }
        }

        private void DrawPoint(Texture2D tex, Vector2 uv, ActionPointType type)
        {
            int x = (int)uv.x;
            int y = (int)uv.y;
            int radius = circleRadius;


            Color color;
            switch (type)
            {
                case ActionPointType.Spawn: color = Color.green; break;
                case ActionPointType.Checkpoint: color = Color.cyan; break;
                case ActionPointType.Danger: color = Color.red; break;
                case ActionPointType.Trigger: color = Color.yellow; break;
                default: color = Color.white; break;
            }




            for (int dy = -radius; dy <= radius; dy++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    int distSqr = dx * dx + dy * dy;
                    int outer = radius * radius;
                    int inner = (radius - 10) * (radius - 10);

                    if (distSqr <= outer && distSqr >= inner)
                    {
                        int px = x + dx;
                        int py = y + dy;
                        if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                            tex.SetPixel(px, py, color);
                    }
                }
            }


            float iconSize = Mathf.Clamp((float)circleRadius * 1.3f, 40, 400);

            if (iconContents.TryGetValue(type, out Texture2D icon))
            {
                StampIcon(tex, uv, icon, (int)iconSize);
            }

        }



        private void StampIcon(Texture2D tex, Vector2 uv, Texture2D icon, int size = 16)
        {
            int x = (int)uv.x - size / 2;
            int y = (int)uv.y - size / 2;

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int px = x + i;
                    int py = y + j;
                    if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    {
                        Color iconColor = icon.GetPixel(i * icon.width / size, j * icon.height / size);
                        if (iconColor.a > 0.1f) // ne pas dessiner les pixels complètement transparents
                        {
                            tex.SetPixel(px, py, iconColor);
                        }
                    }
                }
            }
        }



    }
}
