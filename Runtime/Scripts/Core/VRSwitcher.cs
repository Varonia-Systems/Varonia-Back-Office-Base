
#if UNITY_EDITOR
using UnityEngine;
#if HAS_XR_MANAGEMENT
using UnityEngine.XR.Management;
#endif
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaughtyAttributes;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.XR;

namespace VaroniaBackOffice
{
    public class VRSwitcher : MonoBehaviour
    {



#if HAS_XR_MANAGEMENT
        [InfoBox("Press F11 for Switch Vr On/Off   ", EInfoBoxType.Normal)]
        [Label("Init VR on startup")]
        [InfoBox("The \"init VR on startup\" parameter is only useful in the editor", EInfoBoxType.Warning)]
        public bool initvrEnabled = true;
        
        
         public bool vrEnabled = true;

         public UnityEvent onVRSwitch = new UnityEvent();
        void Awake()
        {
            vrEnabled = XRSettings.enabled;

            Debug.LogWarning("VR Enabled : " + vrEnabled);

            if (!vrEnabled)
            {
         
                XRGeneralSettings.Instance.Manager.StopSubsystems();
                XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            }
            else if(!initvrEnabled)
            {
               
              StartCoroutine(StopXR());
                vrEnabled = false;
            }

        }


        void Update()
        {

            if ( KeyboardHook.GetKeyDown(KeyCode.F11) && Application.isFocused )
            {
             
                vrEnabled = !vrEnabled;

                if (vrEnabled)
                    StartCoroutine(StartXR());
                else
                    StartCoroutine(StopXR());
                
                onVRSwitch.Invoke();
            }
        }

        
        public List<T> GetAllComponentsOfType<T>() where T : Component
        {
            return FindObjectsOfType<T>(true).ToList();
        }

        
        IEnumerator StartXR()
        {


            XRGeneralSettings.Instance.Manager.InitializeLoaderSync();
            if (XRGeneralSettings.Instance.Manager.activeLoader == null)
            {
                Debug.LogError("Initializing XR Failed.");
                yield break;
            }

            XRGeneralSettings.Instance.Manager.StartSubsystems();
            Debug.LogWarning("XR started");
          
            GetAllComponentsOfType<PosMul>().ForEach(l=> l.enabled = true);
          

        }

        IEnumerator StopXR()
        {

            XRGeneralSettings.Instance.Manager.StopSubsystems();
            XRGeneralSettings.Instance.Manager.DeinitializeLoader();
            
            Camera.main.fieldOfView = 60;
            
            
            yield return new WaitForSeconds(1f);
            
          var A= GetAllComponentsOfType<PosMul>();
              A.ForEach(l=> l.enabled = false);
              A.ForEach(l=>l.transform.localPosition = Vector3.zero);

            
            Debug.LogWarning("XR stopped");
        }
    }
}
#else
        [InfoBox("com.unity.xr.management is not found", EInfoBoxType.Error)]
        [Label("Init VR on startup")][ReadOnly]
        public bool initvrEnabled = true;
    }
}
#endif
#endif