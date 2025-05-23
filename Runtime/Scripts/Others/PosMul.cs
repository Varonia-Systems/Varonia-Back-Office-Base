using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace VaroniaBackOffice
{

    public class PosMul : MonoBehaviour
    {



        public Transform CamTransform;

        public float CoefMul = 0.1f;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => Config.VaroniaConfig != null);

            if (Config.Spatial != null)
                CoefMul = (float)Config.Spatial.Multiplier;
            else
                CoefMul = 0.0f;





        }


        void Update()
        {
            if (CamTransform != null)
                transform.localPosition = new Vector3(CamTransform.localPosition.x * CoefMul, 0, CamTransform.localPosition.z * CoefMul);
            else if (VaroniaGlobal.VG.MainCamera != null) CamTransform = VaroniaGlobal.VG.MainCamera.transform;

        }
    }


}
