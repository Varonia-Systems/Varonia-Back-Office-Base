using System;
using System.Collections.Generic;
using System.Linq;


namespace VaroniaBackOffice
{
    public class EN
    {


        public enum EDeviceState
        {
            UNKNOWN = 0,
            READY = 1,
            REBOOTING = 5,
            OFFLINE = 10,
            DIESEL = 1000,


            SPATIAL_BACKUP = 21,
            SPATIAL_RESTORE = 22,
            SPATIAL_RESET = 23,
            SPATIAL_DISABLINGBOUNDARY = 24,

            READYTOJOIN = 105,
            JOINING = 110,
            INLOBBY = 115,
            INPARTY = 120,
            CRASH = 125,

            GAME_LOOKINGFORBOUNDARY = 1102,
            GAME_BOUNDARYFOUND = 1105,
            GAME_BOUNDARYDISABLED = 1108,


        }


        public enum ESoftState
        {
            UNKNOWN = 0,
            READY = 1,

            GAME_LAUNCHED = 112,
            GAME_INLOBBY = 110,
            GAME_INPARTY = 115,
            GAME_CHECKING = 122,
            GAME_SAFETYING = 125,
            GAME_HOSTCONNECTING = 128,



        }


        public enum EMQTTMethod
        {
            UNKNOWN = 0,

            GET_SOFTPARTYSTART = 20101,
            GET_SOFTPARTYSTART_RESULT = 20102,
            GET_SOFTPARTYSKIPTUTOANDSTART = 20111,
            GET_SOFTPARTYSKIPTUTOANDSTART_RESULT = 20112,


            SET_SOFTPARTYSTARTED = 20210,
            SET_SOFTPARTYFINISHED = 20215,
            SET_SOFTPARTYCLOSED = 20220,
            GET_SOFTCHANGESCENE = 20121,
            GET_SOFTCHANGESCENE_RESULT = 20122,

            SET_SOFTSTATE = 20201,
            SET_SOFTSCORE = 20205,
            SET_SOFTLAG = 20255,


        }

    }
}