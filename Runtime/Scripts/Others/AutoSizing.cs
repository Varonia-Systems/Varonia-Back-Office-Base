using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace VaroniaBackOffice
{
    public class AutoSizing : MonoBehaviour
    {
        public static float SharedPlayerSize;

        [Header("Mesures")]
        [Tooltip("Valeur calculée automatiquement")]
        public float playerSize;

        [Tooltip("Taille maximale détectée, ajustée dynamiquement")]
        public float playerMaxHeight = 1.2f;

        [HideInInspector]
        public int requiredSampleCount = 250;

        private bool hasSizedPlayer = false;
        private Queue<float> heightSamples = new Queue<float>();

     

        public  void  StartSizing()
        {
            if (hasSizedPlayer)
                return;

            hasSizedPlayer = true;
            StartCoroutine(MeasurePlayerHeightCoroutine());
        }

        private IEnumerator MeasurePlayerHeightCoroutine()
        {
            yield return new WaitForSeconds(1f);

            while (heightSamples.Count < requiredSampleCount)
            {
                yield return new WaitForSeconds(0.02f);

                float currentHeight = VaroniaGlobal.VG.MainCamera.transform.localPosition.y;

                if (currentHeight > playerMaxHeight)
                    playerMaxHeight = currentHeight;

                if (currentHeight > (playerMaxHeight - 0.15f))
                {
                    heightSamples.Enqueue(currentHeight + 0.1f);
                }

        
                while (heightSamples.Count > requiredSampleCount)
                    heightSamples.Dequeue();

                if (heightSamples.Count > 0)
                    playerSize = heightSamples.Average();
            }

            SharedPlayerSize = playerSize;
        }
    }
}
