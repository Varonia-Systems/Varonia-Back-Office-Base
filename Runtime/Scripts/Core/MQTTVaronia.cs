using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using M2MqttUnity;
using Newtonsoft.Json;
using System.IO;

namespace VaroniaBackOffice
{

    public class MQTTVaronia : M2MqttUnityClient
    {
        public DebugVaronia DebugVaronia;
        private List<string> eventMessages = new List<string>();
        public static MQTTVaronia instance;
        public EN.ESoftState SoftState;


        protected override void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(gameObject.transform.parent);
            instance = this;
            brokerAddress = Config.VaroniaConfig.MQTT_ServerIP;
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                base.Start();
                StartCoroutine(Check_Connection());
            }
        }


        public void SETDB_ADDEVENT(string Type, string JSONData, DateTime Begin = new DateTime(), DateTime End = new DateTime())
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                if (Begin.Year != 1)
                {
                    //   PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SETDB_ADDEVENT", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "JSONData", JSONData }, { "Type", Application.productName + "_" + Type }, { "Start", Begin.ToString("yyyy-MM-dd HH:mm:ss.fff") }, { "End", End.ToString("yyyy-MM-dd HH:mm:ss.fff") } } }));
                    PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SETDB_ADDEVENT", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "JSONData", JSONData }, { "Type", Application.productName + "_" + Type }, { "Start", Begin }, { "End", End } } }));

                }
                else
                    PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SETDB_ADDEVENT", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "JSONData", JSONData }, { "Type", Application.productName + "_" + Type } } }));
            }
        }



        public void SETDB_ADDEVENT(string Type)
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SETDB_ADDEVENT", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "Type", Application.productName + "_" + Type } } }));
            }

        }


        public string BaseJson(string Name, string Value)
        {
            string T = "";

            T = "{\"" + Name + "\" : \"" + Value + "\"}";


            return T;

        }






        private EN.ESoftState OLD_State;
        public void SetSoftState(EN.ESoftState eSoft)
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTSTATE", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "SoftState", eSoft } } }));
                if (SoftState != OLD_State)
                    SETDB_ADDEVENT("SOFT_STATE", BaseJson("SOFTSTATE", eSoft.ToString()));
            }

            SoftState = eSoft;
            OLD_State = SoftState;
        }

        public void SetSoftPartyStarted()
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPARTYSTARTED", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient }));
                SETDB_ADDEVENT("SOFT_PARTY_STARTED");
            }
        }

        public void SetSoftPartyFinished()
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPARTYFINISHED", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient }));
                SETDB_ADDEVENT("SOFT_PARTY_FINISHED");
            }
        }

        public void SetSoftPartyClosed()
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPARTYCLOSED", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient }));
                SETDB_ADDEVENT("SOFT_PARTY_CLOSED");
            }
        }


        public void SetSoftPiloteDevice(string Key, bool State)
        {
            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTPILOTDEVICE", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = { { "Key", Key }, { "State", State } } }));
                SETDB_ADDEVENT("SET_SOFTPILOTDEVICE");
            }
        }


#if Game_Score
        public void SetScore(Game_Score Score)
        {

            if (!String.IsNullOrEmpty(Config.VaroniaConfig.MQTT_ServerIP))
            {
                // Get Game ID

                int GameId = 9999;

                try
                {
                    using (StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/GameID.txt"))
                    {
                        GameId = int.Parse(sr.ReadToEnd());
                    }
                }
                catch (Exception)
                {

                }
                var D = new Dictionary<string, object>();
                D.Add("Data", Score);
                D.Add("GameValue", GameId);
                PublishMsg(JsonConvert.SerializeObject(new MQTT_Payload() { sMethod = "SET_SOFTSCORE", CallerDeviceID = Config.VaroniaConfig.MQTT_IDClient, Items = D }));
            }

    }
#endif


        public void PublishMsg(string Msg)
        {
            try
            {
                client.Publish("UnityToServer/" + Config.VaroniaConfig.MQTT_IDClient, System.Text.Encoding.UTF8.GetBytes(Msg), MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE, false);
            }
            catch (Exception)
            {

            }
        }

        public void Subscribe()
        {
            client.Subscribe(new string[] { "ServerToUnity/" + Config.VaroniaConfig.MQTT_IDClient }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });


        }


        protected override void OnConnecting()
        {
            base.OnConnecting();

        }


        Coroutine UpCo;





        protected override void OnConnected()
        {
            base.OnConnected();

            SoftState = EN.ESoftState.GAME_LAUNCHED;
            UpCo = StartCoroutine(UpConnection());
            Subscribe();



        }


        // Ping Serveur
        IEnumerator UpConnection()
        {
            while (true)
            {
                yield return new WaitForSeconds(1);
                SetSoftState(SoftState);
            }
        }






        IEnumerator Check_Connection()
        {
            yield return new WaitForSeconds(8);

            while (true)
            {
                if (MqttClient.IsConnected__)
                {
                    yield return new WaitForSeconds(1);
                }
                else if (!MqttClient.IsConnected__)
                {
                    StopCoroutine(UpCo);
                    Debug.LogError("MQTT Lost Connection");
                    yield return new WaitForSeconds(8);
                    Connect();
                    yield return new WaitForSeconds(5);
                }
            }



        }




        protected override void DecodeMessage(string topic, byte[] message) // Receive Message
        {
            var payload = JsonConvert.DeserializeObject<MQTT_Payload>(System.Text.Encoding.UTF8.GetString(message));

            // Ordre Start Partie
            if (payload.sMethod == "GET_SOFTPARTYSTART_RESULT")
            {
                VaroniaGlobal.StartGame = true;
                VaroniaGlobal.VG.OnStartGame.Invoke();
            }
            // Ordre Start Partie Sans Tutorial
            if (payload.sMethod == "GET_SOFTPARTYSKIPTUTOANDSTART_RESULT")
            {
                VaroniaGlobal.StartGame = true;
                VaroniaGlobal.SkipTuto = true;
                VaroniaGlobal.VG.OnStartGame.Invoke();
            }
            if (payload.sMethod == "GET_SOFTCHANGESCENE_RESULT")
            {
                string SceneName = payload.Items["SceneName"].ToString();
                DebugVaronia.ChangeScene(SceneName);
            }
        }


        protected override void OnConnectionFailed(string errorMessage)
        {
        }

        protected override void OnDisconnected()
        {
            StopCoroutine(UpCo);
        }

        protected override void OnConnectionLost()
        {
            StopCoroutine(UpCo);
        }

        private void OnDestroy()
        {
            Disconnect();
        }

    }





    public class MQTT_Payload
    {
        public int CallerDeviceID { get; set; }
        public int TargetDeviceID { get; set; }
        // public object Data { get; set; }
        public string sMethod { get; set; }
        public Dictionary<string, object> Items { get; set; }

        public MQTT_Payload()
        {
            Items = new Dictionary<string, object>();
        }
    }
}







