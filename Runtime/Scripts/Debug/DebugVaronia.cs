using M2MqttUnity.Examples;
using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using UnityEngine.EventSystems;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace VaroniaBackOffice
{
    public class DebugVaronia : MonoBehaviour
    {


        [Header("Items")]
        public GameObject DebugPanel;
        public GameObject Graphy;
        public Text SceneList;
        public GameObject FPS3D;
        public Text Latency;
        public GameObject LightDebugCanvas;



        [Header("Other")]
        public float Eye_FPS_Distance = 0.8f;
        public VaroniaGlobal VG;


        [Header("AlwaysDebugUI")]
        public GameObject PanelDebugInfo;
        public Text TextDebugInfo;




        [BoxGroup("Infos")] public bool InBoundary;
        [BoxGroup("Infos")] public bool IsNearbyBoundary;


        private GameObject GlobalServer;
        public static bool IsDebugMode;

        public static bool Is3DDebugMode;


        public static DebugVaronia Instance;


        private bool Has_Boundary_Info;

        [HideInInspector]
        public bool AdvDebugMove;



        private EventSystem _localEventSystems;
#if ENABLE_INPUT_SYSTEM
        private InputSystemUIInputModule _localInputSystemEventSystem;
#endif
       
        
        void CheckEventSystems()
        {
            if (EventSystem.current == null)
            {
                _localEventSystems = gameObject.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                _localInputSystemEventSystem = gameObject.AddComponent<InputSystemUIInputModule>();
                #endif
            }
            else if (_localEventSystems)
            {
#if ENABLE_INPUT_SYSTEM
                Destroy(_localInputSystemEventSystem);
#endif
                Destroy(_localEventSystems);
            }
        }


        void DebugInfoUpdate()
        {

            TextDebugInfo.text = "<color=white>----- Base Debug -----</color>\n";
            TextDebugInfo.text += "<color=white>Game Version : " + Application.version + "</color>\n";
            TextDebugInfo.text += "<color=white>VBO Version : " + VaroniaGlobal.VG.VBO_Version + "</color>\n\n";

            if (MqttClient.IsConnected__)
                TextDebugInfo.text += "<color=green>MQTT Status = OK</color>" + "\n";
            else
                TextDebugInfo.text += "<color=red>MQTT Status = FAIL</color>" + "\n";

            if (VaroniaGlobal.VG.MainCamera != null)
                TextDebugInfo.text += "Camera Size: " + Math.Round(VaroniaGlobal.VG.MainCamera.transform.localPosition.y + 0.1f, 3) + "\n";


            if (VaroniaGlobal.VG.LastLoadTime.Seconds < 10)
                TextDebugInfo.text += "Last Load Time : " + VaroniaGlobal.VG.LastLoadTime.Seconds + ":" + VaroniaGlobal.VG.LastLoadTime.Milliseconds + "\n";
            else
                TextDebugInfo.text += "<color=orange>Last Load Time : " + VaroniaGlobal.VG.LastLoadTime.Seconds + ":" + VaroniaGlobal.VG.LastLoadTime.Milliseconds + "</color>\n";

            if (Has_Boundary_Info)
            {
                string Has_B;
                string Has_N;


                if (InBoundary)
                    Has_B = "<color=green>true</color>";
                else
                    Has_B = "<color=red>false</color>";

                if (IsNearbyBoundary)
                    Has_N = "<color=red>true</color>";
                else
                    Has_N = "<color=green>false</color>";

                TextDebugInfo.text += "In Boundary: " + Has_B + " IsNear: " + Has_N + "\n";
            }



        }




        bool init = false;
        IEnumerator Start()
        {
            PanelDebugInfo.SetActive(false);

            Instance = this;

            LoadScenes();

            SceneManager.activeSceneChanged += SceneManager_activeSceneChanged;


            while (Config.VaroniaConfig == null)
            {
                yield return new WaitForSeconds(0.1f);
            }
            yield return new WaitForSeconds(1.4f);
            init = true;


            StartCoroutine(WhileGlobalServer());



        }


        IEnumerator WhileGlobalServer()
        {
            while (true)
            {
                yield return new WaitForFixedUpdate();

                if (GlobalServer == null)
                {
                    var A = GameObject.Find(VG.ChangeSceneGameobject);
                    if (A != null) GlobalServer = A;
                }

            }
        }



        private void SceneManager_activeSceneChanged(Scene arg0, Scene arg1)
        {
            DisplayScenes();
            StartCoroutine(AfterLoad());

            if (Config.VaroniaConfig.DebugMode)
            {
                ShowDebug();
                Config.VaroniaConfig.DebugMode = false;
            }
        }




        IEnumerator AfterLoad()
        {
            yield return new WaitForSeconds(1.5f);

            if (VG.MainCamera != null)
            {
                FPS3D.transform.parent = VG.MainCamera.transform; FPS3D.transform.localPosition = new Vector3(0, 0, Eye_FPS_Distance); FPS3D.transform.localEulerAngles = new Vector3();
            }


        }



        #region SceneManager



        public List<string> currentLoadedScenes = new List<string>();
        public int lastLoadedScene = 0;
        public int firstLoadedScene = 0;

        public Button nextScenesButton;
        public Button previousScenesButton;




        public void DisplayScenes()
        {
            SceneList.text = "";
            for (int i = 0; i < currentLoadedScenes.Count; i++)
            {
                if (SceneManager.GetActiveScene().path != currentLoadedScenes[i])
                    SceneList.text += "Press " + i + " for load scene (" + currentLoadedScenes[i] + ")";
                else
                    SceneList.text += "<color=orange>Press " + i + " for load scene (" + currentLoadedScenes[i] + ") Current Scene </color>";

                if (currentLoadedScenes[i].Contains(VG.LobbySceneName))
                    SceneList.text += " <color=red>Don't Reload Lobby</color>";

                if (currentLoadedScenes[i].Contains("Menu"))
                    SceneList.text += " <color=red>Don't Load Menu</color>";





                SceneList.text += "\n";
            }
            SceneList.text += "\n";


            if (firstLoadedScene != 0)
            {
                SceneList.text += "<color=red>Press - to display previous scenes</color> \n";
                previousScenesButton.interactable = true;
            }
            else
            {
                previousScenesButton.interactable = false;
            }


            if (lastLoadedScene != SceneManager.sceneCountInBuildSettings - 1)
            {
                SceneList.text += "<color=red>Press + to display next scenes</color> \n";
                nextScenesButton.interactable = true;
            }
            else
            {
                nextScenesButton.interactable = false;
            }

        }



        public void LoadScenes()
        {
            currentLoadedScenes.Clear();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                if (i <= 9)
                {
                    currentLoadedScenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                    //print("Loading scene " + i + " " + SceneUtility.GetScenePathByBuildIndex(i));
                }

            }
            lastLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.Last());
            firstLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.First());
            DisplayScenes();
        }

        public void LoadNextScenes()
        {
            if (lastLoadedScene != SceneManager.sceneCountInBuildSettings - 1)
            {
                currentLoadedScenes.Clear();
                for (int i = lastLoadedScene + 1; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    if (i <= lastLoadedScene + 10)
                    {
                        currentLoadedScenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                    }
                }
                lastLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.Last());
                firstLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.First());
                DisplayScenes();
            }
        }

        public void LoadPreviousScenes()
        {
            if (firstLoadedScene != 0)
            {
                currentLoadedScenes.Clear();

                for (int i = firstLoadedScene - 10; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    if (i < firstLoadedScene)
                    {
                        currentLoadedScenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                    }
                }

                lastLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.Last());
                firstLoadedScene = SceneUtility.GetBuildIndexByScenePath(currentLoadedScenes.First());
                DisplayScenes();
            }


        }



        void InputChangeScene()
        {
            for (int i = 0; i < currentLoadedScenes.Count; i++)
            {
                if (Input.GetKeyDown(i.ToString()) || Input.GetKeyDown("[" + i.ToString() + "]"))
                {
                    ChangeScene(currentLoadedScenes[i]);
                }
            }

            if (lastLoadedScene != SceneManager.sceneCountInBuildSettings - 1 && Input.GetKeyDown(KeyCode.KeypadPlus))
            {
                LoadNextScenes();
            }
            else if (firstLoadedScene != 0 && Input.GetKeyDown(KeyCode.KeypadMinus))
            {
                LoadPreviousScenes();
            }
        }



        #endregion



        public void ChangeScene(string SceneName)
        {
            GlobalServer.SendMessage(VG.FunctionName_ChangeScene, SceneName);
        }

        void Update()
        {

            if (!init) return;

            // Show or Hide Lite Debug Mod
            if (!Config.VaroniaConfig.HideLightDebug)
                LightDebugCanvas.SetActive(true);
            else
                LightDebugCanvas.SetActive(false);

            Config.VaroniaConfig.HideLightDebug = Config.VaroniaConfig.Movie;

            if (Config.VaroniaConfig.DeviceMode != DeviceMode.Server_Spectator && Config.VaroniaConfig.DeviceMode != DeviceMode.Client_Spectator)
                PanelDebugInfo.SetActive(!Config.VaroniaConfig.Movie);
            else
                PanelDebugInfo.SetActive(false);



            if (Application.isFocused)
            {

                try
                {

                    if (KeyboardHook.GetKeyDown(KeyCode.M)) // Minimize Game 
                    {
                        OnMinimizeButtonClick();
                    }


                    if (KeyboardHook.GetKeyDown(KeyCode.F1)) // Show advence Debug Mod
                        ShowDebug();

                    if (KeyboardHook.GetKeyDown(KeyCode.F7))
                        Show3DDebug();


                    if (KeyboardHook.GetKeyDown(KeyCode.F12))
                        AdvDebugMove = !AdvDebugMove;


                    if (AdvDebugMove)
                    {

                        int AddMul = 0;

                        if (KeyboardHook.GetKey(KeyCode.LeftShift) || KeyboardHook.GetKey(KeyCode.RightShift))
                            AddMul += 4;


                        if (!KeyboardHook.GetKey(KeyCode.RightAlt))
                        {
                            if (KeyboardHook.GetKey(KeyCode.UpArrow))
                                VaroniaGlobal.VG.Rig.position += (VaroniaGlobal.VG.MainCamera.transform.forward * 0.1f) * (Time.deltaTime * (8 + AddMul));

                            if (KeyboardHook.GetKey(KeyCode.DownArrow))
                                VaroniaGlobal.VG.Rig.position -= (VaroniaGlobal.VG.MainCamera.transform.forward * 0.1f) * (Time.deltaTime * (8 + AddMul));

                            if (KeyboardHook.GetKey(KeyCode.LeftArrow))
                                VaroniaGlobal.VG.Rig.position -= (VaroniaGlobal.VG.MainCamera.transform.right * 0.1f) * (Time.deltaTime * (8 + AddMul));

                            if (KeyboardHook.GetKey(KeyCode.RightArrow))
                                VaroniaGlobal.VG.Rig.position += (VaroniaGlobal.VG.MainCamera.transform.right * 0.1f) * (Time.deltaTime * (8 + AddMul));
                        }
                        else
                        {
                            if (KeyboardHook.GetKey(KeyCode.UpArrow))
                                VaroniaGlobal.VG.Rig.position += (VaroniaGlobal.VG.MainCamera.transform.up * 0.1f) * (Time.deltaTime * (8 + AddMul));

                            if (KeyboardHook.GetKey(KeyCode.DownArrow))
                                VaroniaGlobal.VG.Rig.position -= (VaroniaGlobal.VG.MainCamera.transform.up * 0.1f) * (Time.deltaTime * (8 + AddMul));

                            if (KeyboardHook.GetKey(KeyCode.LeftArrow))
                                VaroniaGlobal.VG.Rig.localEulerAngles -= (new Vector3(0, 1, 0)) * (Time.deltaTime * (15 + (AddMul * 2)));

                            if (KeyboardHook.GetKey(KeyCode.RightArrow))
                                VaroniaGlobal.VG.Rig.localEulerAngles += (new Vector3(0, 1, 0)) * (Time.deltaTime * (15 + (AddMul * 2)));
                        }
                    }
                }
                catch (Exception)
                {


                }
            }
            DebugInfoUpdate();




            if (!IsDebugMode)
                return;

            // If Advence Debug Mod Active

            InputChangeScene();

            if (KeyboardHook.GetKeyDown(KeyCode.F2)) // Force Start Game Whith Tuto (if have)
            {
                VaroniaGlobal.StartGame = true;
                VaroniaGlobal.VG.OnStartGame.Invoke();
            }


            if (KeyboardHook.GetKeyDown(KeyCode.F3)) // Force Start Game Whithout Tuto
            {
                VaroniaGlobal.SkipTuto = true;
                VaroniaGlobal.StartGame = true;
                VaroniaGlobal.VG.OnStartGame.Invoke();
            }


            if (KeyboardHook.GetKeyDown(KeyCode.F8)) // Active/Disable Movie Mod
            {
                Config.VaroniaConfig.Movie = !Config.VaroniaConfig.Movie;

            }


            if (KeyboardHook.GetKeyDown(KeyCode.KeypadPlus)) // add Time.scale
            {
                Time.timeScale += 0.1f;

            }

            if (KeyboardHook.GetKeyDown(KeyCode.KeypadMinus)) // less Time.scale
            {
                if (Time.timeScale - 0.1 < 0)
                    Time.timeScale = 0f;
                else
                    Time.timeScale -= 0.1f;

            }

            if (KeyboardHook.GetKeyDown(KeyCode.KeypadEnter)) // Reset Time.scale
            {
                Time.timeScale = 1f;

            }


        }



        void Show3DDebug()
        {
            Is3DDebugMode = !Is3DDebugMode;
            try
            {
                FPS3D.SetActive(Is3DDebugMode);
            }
            catch (System.Exception)
            {
            }
        }

        void ShowDebug()
        {
            CheckEventSystems();
            
            IsDebugMode = !IsDebugMode;
            Graphy.SetActive(IsDebugMode);
            DebugPanel.SetActive(IsDebugMode);

        }


#if VBO_Spatial
        public void Up_Player_B(int W)
        {
            Has_Boundary_Info = true;

            if (W == 2)
            {
                VaroniaGlobal.VG.OnPlayerEnterArea.Invoke();
                InBoundary = true;
            }
            else
            {
                VaroniaGlobal.VG.OnPlayerLeaveArea.Invoke();
                InBoundary = false;
            }

        }
#endif

        public void Up_Player_B_Near(bool N)
        {
            IsNearbyBoundary = N;
        }




        #region Minimize Game
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hwnd, int nCmdShow);
        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();

        public void OnMinimizeButtonClick()
        {
            ShowWindow(GetActiveWindow(), 2);
        }
        #endregion
    }

}
