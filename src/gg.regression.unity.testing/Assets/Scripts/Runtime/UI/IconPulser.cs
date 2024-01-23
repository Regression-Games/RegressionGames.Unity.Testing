using UnityEngine;
using UnityEngine.UI;

namespace RegressionGames.Unity.UI
{
    [AddComponentMenu("Regression Games/Internal/Icon Pulser")]
    internal class IconPulser : MonoBehaviour
    {
        private Image image;

        public float pulseInterval = 3f;

        public float ogPulseInterval = 3f;

        public float pulseDuration = 1f;

        public int maxAlpha = 120;

        private float lastPulse = -1f;

        public void Fast()
        {
            pulseInterval = ogPulseInterval / 2;
        }

        public void Normal()
        {
            pulseInterval = ogPulseInterval;
        }

        public void Slow()
        {
            pulseInterval = ogPulseInterval * 2;
        }

        void Start()
        {
            if (pulseDuration > pulseInterval)
            {
                pulseInterval = pulseDuration;
            }
            // start in 1 second
            lastPulse = -1f * pulseInterval + 1f;
            image = GetComponent<Image>();
            ogPulseInterval = pulseInterval;
        }

        void LateUpdate()
        {
            float time = Time.time;
            float timeSinceLast = time - lastPulse;

            if (timeSinceLast > pulseInterval)
            {
                lastPulse = time;
            }

            // -1 -> 1
            float rangeVal = (timeSinceLast / pulseDuration) * 2 - 1;

            float alphaValue = ( Mathf.Abs(rangeVal) * -1 * maxAlpha + maxAlpha)/255f;

            image.color = new Color(image.color.r, image.color.g, image.color.b, alphaValue);
        }
    }
}
