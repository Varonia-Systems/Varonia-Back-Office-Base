using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VaroniaBackOffice
{


    public enum EventType { None = 0, Christmas = 1, Halloween = 2, Easter = 3 }


    public enum DeviceMode
    {
        Server_Spectator = 0,
        Server_Player = 1,
        Client_Spectator = 2,
        Client_Player = 3,
    }


    public enum MainHand
    {
        Right = 0,
        Left = 1,
    }


    public enum Controller
    {
        Unknown = -1,
        FOCUS3_VBS_CTRL = 4, //  Focus 3 Vive Business Streaming (VBS) controllers
        HandTracking = 5,  // OpenVR Skeleton
        PICO_VSVR_CTRL = 6, //  Pico Varonia Streamer Virtual Reality (VSVR) controllers


#if VBO_Input
   FOCUS3_VBS_VaroniaGun = 3,
   FOCUS3_VBS_Striker = 50,
   FOCUS3_VBS_HK416 = 101,

   PICO_VSVR_VaroniaGun = 70,


#endif

#if VBO_VORTEX
        VORTEX_WEAPON_FOCUS = 501,
#endif


    }

    public partial class VaroniaConfig
    {
        public string ServerIP;  // the game server IP address (the client connects to this IP)
        public string MQTT_ServerIP; // the MQTT server IP address
        public int MQTT_IDClient; // the MQTT ID
        public DeviceMode DeviceMode; // Type of client (Player Server, Spectator Server, Player Client, Spectator Client)
        public string Language; // Game language
        public MainHand MainHand; // Main hand (Left or Right)
        public string PlayerName; // Pseudo player
        public bool Movie; // If movie is set to "true", it disables overlays to allow screen recording.
        public bool Direct; //Direct mode becomes "true" in case of a crash — this allows skipping an intro, for example, to join faster.
        public bool DebugMode; //If "true", adds the debug overlay.
        public float GlobalVolume = 1; //Controls the game's global volume
        public Controller Controller; //Defines the type of controller the user has in the game.
        public bool HideLightDebug; //Small debug overlay — it is "true" by default.
        public EventType ForcedEvent;
    }



#if VBO_TEAMSPEAK
    public partial class VaroniaConfig
    {
        public string TeamSpeak_ServerIP = "";  // TeamSpeak server IP
        public int TeamSpeak_Channel = 1;  // TeamSpeak channel
        public int TeamSpeak_Amplification = 0; // Teamspeak sound amplification
        public int TeamSpeak_VoiceDetector = 0; // Teamspeak voice activation threshold
    }
#endif


#if VBO_Input
    public partial class VaroniaConfig
    {
        public string WeaponMAC;
    }
#endif

#if VBO_VORTEX
    public partial class VaroniaConfig
    {
        public bool UseVortexBackOffice;
    }
#endif

}
