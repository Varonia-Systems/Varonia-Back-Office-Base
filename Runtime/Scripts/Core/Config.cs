using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Events;



namespace VaroniaBackOffice
{


    public class Config : MonoBehaviour
    {

        public static Config Instance;

        public string GameFolder_Path;
        public static string VaroniaFolder_Path;

#if Game_Config
        public static GameConfig GameConfig;  // Game configuration
#endif
        public static VaroniaConfig VaroniaConfig; // Global configuration 
        public static Spatial Spatial;  // Spatial Informations (Offset,Boundary ....)

        public UnityEvent Initialized = new UnityEvent();

        public bool IsInitialized;




        [HideInInspector]
        public bool InitSpatialAwake = true;


        void Awake()
        {

            Instance = this;

            // Initializes the paths at startup
            GameFolder_Path = Application.persistentDataPath;
            VaroniaFolder_Path = Application.persistentDataPath.Replace(Application.companyName + "/" + Application.productName, "Varonia");



            Init(); // Calls the initialization method
        }
        void Init()
        {
            InitGlobalconfig();
            InitGameConfig();



            if (InitSpatialAwake) InitSpatial();

            Initialized.Invoke(); // Triggers the event once everything is loaded
            IsInitialized = true;
        }
        public void InitGlobalconfig()
        {
            string GC = "";  // Contents of the "base" JSON
            string GC_fdp = ""; // Override content of the file


          
            if (File.Exists(VaroniaFolder_Path + "/GlobalConfig.fdp")) // Search File Data Package
            {
                Debug.Log("Read FDP GlobalConfig");
                using (StreamReader sr = new StreamReader(VaroniaFolder_Path + "/GlobalConfig.fdp"))
                {
                    GC_fdp = sr.ReadToEnd();
                }
            }


            if (!File.Exists(VaroniaFolder_Path + "/GlobalConfig.json")) // If GlobalConfig.json Don't Exist Create File
                CreateBaseGlobalConfig();

#if HAS_MultiplayerPlayMode && UNITY_EDITOR
            string MP_Tag = "";
            string[] AllTags = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
            if (AllTags.Length > 0)
                MP_Tag=AllTags[0];

            if (File.Exists(VaroniaFolder_Path + "/GlobalConfig_"+ MP_Tag + ".json"))
            {
                using (StreamReader sr = new StreamReader(VaroniaFolder_Path + "/GlobalConfig_" + MP_Tag + ".json"))
                {
                    GC = sr.ReadToEnd();
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(VaroniaFolder_Path + "/GlobalConfig.json"))
                {
                    GC = sr.ReadToEnd();
                }
            }
#else
            using (StreamReader sr = new StreamReader(VaroniaFolder_Path + "/GlobalConfig.json"))
            {
                GC = sr.ReadToEnd();
            }
#endif

            try
            {
                VaroniaConfig = JsonMerger.MergeJson<VaroniaConfig>(GC, GC_fdp);
            }
            catch (Exception e)
            {
                PopUperrorManager.Instance.ShowError(ErrorType.JsonError, "JSON issue on the file 'Globalconfig.json'. A default file has been loaded.", e.Message);
                VaroniaConfig = new VaroniaConfig();
            }

            GlobalConfigNullSecure();

            void CreateBaseGlobalConfig()
            {
                VaroniaConfig _varoniaconfig = new VaroniaConfig();
                _varoniaconfig.Language = "Fr";
                _varoniaconfig.ServerIP = "";
                _varoniaconfig.MQTT_ServerIP = "";
                _varoniaconfig.MQTT_IDClient = 0;
                _varoniaconfig.MainHand = MainHand.Right;
                _varoniaconfig.DeviceMode = DeviceMode.Client_Player;
                _varoniaconfig.Movie = false;
                _varoniaconfig.PlayerName = "";
                _varoniaconfig.GlobalVolume = 1;


                var A = JsonConvert.SerializeObject(_varoniaconfig);

                using (StreamWriter sw = new StreamWriter(VaroniaFolder_Path + "/GlobalConfig.json"))
                {
                    sw.Write(JsonPrettify(A));
                }
            }
            void GlobalConfigNullSecure()
            {

                if (VaroniaConfig.ServerIP == null)
                {
                    VaroniaConfig.ServerIP = "The IP address cannot be null.";
                    PopUperrorManager.Instance.ShowError(ErrorType.JsonError, " The IP address cannot be null", "");
                }


                if (VaroniaConfig.MQTT_ServerIP == null)
                {
                    PopUperrorManager.Instance.ShowError(ErrorType.JsonError, " The MQTT Server IP address cannot be null", "");
                    VaroniaConfig.MQTT_ServerIP = "";
                }

            }
        }
        public void InitGameConfig()
        {
#if Game_Config
            string GC = "";  // Contents of the "base" JSON
            string GC_fdp = ""; // Override content of the file


            if (File.Exists(GameFolder_Path + "/Config.fdp")) // Search File Data Package
            {
                using (StreamReader sr = new StreamReader(GameFolder_Path + "/Config.fdp"))
                {
                    GC_fdp = sr.ReadToEnd();
                }
            }



            if (!File.Exists(GameFolder_Path + "/Config.json")) // If Config.json Don't Exist Create File
            {
                GameConfig gameconfig = new GameConfig();
                var A = JsonConvert.SerializeObject(gameconfig);
                using (StreamWriter sw = new StreamWriter(GameFolder_Path + "/Config.json"))
                {
                    sw.Write(JsonPrettify(A));
                }
                GameConfig = gameconfig;
            }

            
            
#if HAS_MultiplayerPlayMode && UNITY_EDITOR
            string MP_Tag = "";
            string[] AllTags = Unity.Multiplayer.Playmode.CurrentPlayer.ReadOnlyTags();
            if (AllTags.Length > 0)
                MP_Tag=AllTags[0];

            if (File.Exists(GameFolder_Path + "/Config"+ MP_Tag + ".json"))
            {
                using (StreamReader sr = new StreamReader(GameFolder_Path + "/Config" + MP_Tag + ".json"))
                {
                    GC = sr.ReadToEnd();
                }
            }
            else
            {
                using (StreamReader sr = new StreamReader(GameFolder_Path + "/Config.json"))
                {
                    GC = sr.ReadToEnd();
                }
            }
#else
            using (StreamReader sr = new StreamReader(GameFolder_Path + "/Config.json"))
            {
                GC = sr.ReadToEnd();
            }
#endif
            try
            {
                GameConfig = JsonMerger.MergeJson<GameConfig>(GC, GC_fdp);
            }
            catch (Exception e)
            {
                PopUperrorManager.Instance.ShowError(ErrorType.JsonError, "JSON issue on the file 'Config.json'. A default file has been loaded.", e.Message);
                GameConfig = new GameConfig();
            }

#endif
        }
        public void InitSpatial()
        {

#if VBO_VORTEX
    if(VaroniaConfig.UseVortexBackOffice)
    return;
#endif

            try
            {
                using (StreamReader sr = new StreamReader(VaroniaFolder_Path + "/NewSpatial.json"))
                {
                    Spatial = JsonConvert.DeserializeObject<Spatial>(sr.ReadToEnd());
                }
            }
            catch (Exception e)
            {
                PopUperrorManager.Instance.ShowError(ErrorType.JsonError, "JSON issue on the file 'NewSpatial.json'.", e.Message);
            }

        }
        public static string JsonPrettify(string json)
        {
            using (var stringReader = new StringReader(json))
            using (var stringWriter = new StringWriter())
            {
                var jsonReader = new JsonTextReader(stringReader);
                var jsonWriter = new JsonTextWriter(stringWriter) { Formatting = Formatting.Indented };
                jsonWriter.WriteToken(jsonReader);
                return stringWriter.ToString();
            }
        }
    }
}