using System.Collections;
using UnityEngine;

namespace HellishBattle
{
    public class CameraShake : MonoBehaviour
    {
        public float duration = 1f;
        public float shotDuration = 1f;
        public AnimationCurve curve;

        Vector3 _offset;

        private void Start()
        {
            _offset = transform.localPosition;
        }

        // Explosion Shake

        public void ShakeCamera()
        {
            StartCoroutine(Shaking());
        }

        IEnumerator Shaking()
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {

                elapsedTime += Time.deltaTime;
                float strength = curve.Evaluate(elapsedTime / duration);
                transform.localPosition = _offset + Random.insideUnitSphere * strength;
                yield return null;
            }
            transform.localPosition = _offset;
        }

        public void ShakeCamera(float Strength)
        {
            StartCoroutine(Shaking(Strength));
        }

        IEnumerator Shaking(float Strength)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {

                elapsedTime += Time.deltaTime;
                float strength = curve.Evaluate(elapsedTime / duration) * Strength;
                transform.localPosition = _offset + Random.insideUnitSphere * strength;
                yield return null;
            }
            transform.localPosition = _offset;
        }

        // Single Shake
        public void ShotShakeCamera()
        {
            StartCoroutine(ShotShaking());
        }

        IEnumerator ShotShaking()
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {

                elapsedTime += Time.deltaTime;
                float strength = curve.Evaluate(elapsedTime / duration);
                transform.localPosition = _offset + Random.insideUnitSphere * strength;
                yield return null;
            }
            transform.localPosition = _offset;
        }

        public void ShotShakeCamera(float Strength)
        {
            StartCoroutine(ShotShaking(Strength));
        }

        IEnumerator ShotShaking(float Strength)
        {
            float elapsedTime = 0f;

            while (elapsedTime < duration)
            {

                elapsedTime += Time.deltaTime;
                float strength = curve.Evaluate(elapsedTime / shotDuration) * Strength;
                transform.localPosition = _offset + Random.insideUnitSphere * 0.5f * strength;
                yield return null;
            }
            transform.localPosition = _offset;
        }
    }
}
