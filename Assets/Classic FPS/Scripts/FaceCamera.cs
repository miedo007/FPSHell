using UnityEngine;

namespace HellishBattle
{
    public class FaceCamera : MonoBehaviour
    {
        public Vector3 cameraDirection;

        void Update()
        {
            cameraDirection = Camera.main.transform.forward;
            cameraDirection.y = 0;
            transform.rotation = Quaternion.LookRotation(cameraDirection);
        }
    }
}