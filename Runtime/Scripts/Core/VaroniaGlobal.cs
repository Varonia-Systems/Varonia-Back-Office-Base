using NaughtyAttributes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


namespace VaroniaBackOffice
{
    public class Timegame
    {
        public DateTime dateTime;
        public float Time;
    }
    public class VaroniaGlobal : MonoBehaviour
    {
        public static VaroniaGlobal VG;

        [InfoBox("Fill in with the name of the GameObject that has the function to change the scene.", EInfoBoxType.Normal)]
        [Tooltip("Fill this with function Change Scene")]
        public string ChangeSceneGameobject;

        [InfoBox("Fill this with function Change Scene", EInfoBoxType.Normal)]
        [Tooltip("Fill this with function Change Scene")]
        public string FunctionName_ChangeScene;

        [InfoBox("Fill this with your 'lobby' Scene if you have 'lobby' Scene", EInfoBoxType.Normal)]
        [Tooltip("Fill this with your 'lobby' Scene if you have 'lobby' Scene")]
        public string LobbySceneName;

        [InfoBox("The tag of the Camera Rig — if left empty, it will use the root of the 'MainCamera'.", EInfoBoxType.Normal)]
        [Tooltip("The tag of the Camera Rig — if left empty, it will use the root of the 'MainCamera'.")]
        public string RigTag;

        [HideInInspector]
        public Camera MainCamera;

    
        [HideInInspector]
        public Transform Rig;




        public TimeSpan LastLoadTime;

        Timegame Timegame_;

        


        public UnityEvent OnInitialized = new UnityEvent();


        public UnityEvent OnStrangeTracking = new UnityEvent();
        public UnityEvent OnTrackingOk = new UnityEvent();
        [HideInInspector]
        public bool IsBadTracking;

#if VBO_Spatial
        public UnityEvent OnPlayerLeaveArea = new UnityEvent();
        public UnityEvent OnPlayerEnterArea = new UnityEvent();
        [HideInInspector]
        public bool IsOutArea;   
#endif


        public static bool SkipTuto;
        public static bool StartGame;

        public UnityEvent OnStartGame = new UnityEvent();



        [HideInInspector]
        public string VBO_Version = "";


        [InfoBox("Add Addons Prefabs here", EInfoBoxType.Normal)]
        public List<Addon> Addon;

        private void LateUpdate()
        {
            Timegame_.dateTime = DateTime.UtcNow;
            Timegame_.Time = Time.time; 
        }


        private IEnumerator Start()
        {
            Timegame_ = new Timegame();
            Timegame_.dateTime = DateTime.UtcNow;

            DontDestroyOnLoad(gameObject);

            VG = this;

            if (File.Exists(Application.streamingAssetsPath + "/VBO_Version.txt"))
                VBO_Version = File.ReadAllText(Application.streamingAssetsPath + "/VBO_Version.txt");


            StartCoroutine(CheckCam());

            yield return new WaitUntil(() => Config.VaroniaConfig != null);


            if (Config.VaroniaConfig.DeviceMode == DeviceMode.Client_Player || Config.VaroniaConfig.DeviceMode == DeviceMode.Server_Player)
            {

            }
            else
                Screen.fullScreen = true;






            yield return StartCoroutine(LoadAddons());


            OnInitialized.Invoke();
        }





        private IEnumerator LoadAddons()
        {
            foreach (var item in Addon)
            {
                var A = Instantiate(item, this.transform);
                A.name = A.name.Replace("(Clone)", "");
                yield return null;
            }

            yield return new WaitForFixedUpdate();
        }






        private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
        {
            try
            {
                Debug.Log("End Load Scene at " + DateTime.UtcNow.ToString("HH:mm:ss"));
                TimeSpan t = DateTime.UtcNow - Timegame_.dateTime;
                Debug.Log("Scene was loaded in " + t);
                LastLoadTime = t;
                MQTTVaronia.instance.SETDB_ADDEVENT("SOFT_CHANGESCENE", MQTTVaronia.instance.BaseJson("SCENE_NAME", arg0.name), Timegame_.dateTime, DateTime.UtcNow);
            }
            catch (Exception)
            {

            }
            StartCoroutine(CheckCam());
        }



        IEnumerator CheckCam()
        {
            while (MainCamera == null || Rig == null)
            {
                yield return new WaitForFixedUpdate();
                MainCamera = Camera.main;
                if (MainCamera != null)
                {
                    if (string.IsNullOrEmpty(RigTag))
                    {
                        Rig = MainCamera.transform.root;
                    }                 
                    else
                    {
                        var A = GameObject.FindGameObjectWithTag(RigTag);

                        if (A != null)
                            Rig = A.transform;
                    }

                  
                }
            }
        }


        private void OnEnable()
        {
            SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
        }






        [Button]
        public void WikiBackOffice()
        {
            Application.OpenURL("https://www.notion.so/varoniasystems/0cd3a1d12d2f4eaaac494bcbaef78381?v=fed1c39ac39741ef9b23e51b42d1510f");
        }

    }

}
