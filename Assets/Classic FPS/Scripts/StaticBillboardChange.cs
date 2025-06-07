using UnityEngine;

namespace HellishBattle
{
    public class StaticBillboardChange : MonoBehaviour
    {
        [Tooltip("Sprite List\n[Forward, Backward, Right, Left]")]
        [SerializeField] Sprite[] sprites;
        [Tooltip("Animation List\n[Forward, Backward, Right, Left]")]
        [SerializeField] AnimationClip[] anims;
        [Tooltip("Specifies whether sprites or animations are to be displayed")]
        [SerializeField] bool isAnimated;

        // Privates
        Animator anim;
        SpriteRenderer sr;
        float angle;

        private void Awake()
        {
            anim = GetComponent<Animator>();
            sr = GetComponent<SpriteRenderer>();
        }

        // Based on Angle Displays the corresponding Sprite
        private void Update()
        {
            angle = GetAngle();
            if (angle >= 225.0f && angle <= 315.0f)
                ChangeSprite(0);
            else if (angle < 225.0f && angle > 135.0f)
                ChangeSprite(1);
            else if (angle <= 135.0f && angle >= 45.0f)
                ChangeSprite(2);
            else if ((angle < 45.0f && angle > 0.0f) || (angle > 315.0f && angle < 360.0f))
                ChangeSprite(3);
        }

        // It displays either Animations or Sprite
        void ChangeSprite(int index)
        {
            if (isAnimated)
                anim.Play(anims[index].name);
            else
                sr.sprite = sprites[index];
        }

        // Calculates the angle between the player and the dynamic Object
        float GetAngle()
        {
            Vector3 direction = Camera.main.transform.position - this.transform.position;
            float angleTemp = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg;
            angleTemp += 180.0f;
            return angleTemp;
        }

    }
}
