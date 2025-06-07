using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;


namespace HellishBattle
{
    public class ProceduralAnimation : MonoBehaviour
    {
        [Tooltip("Sprite List")] public List<Sprite> sprites; 
        [Tooltip("Time between changes to the animation")] public float interval; 
        [Tooltip("If true, the object will be destroyed after one animation")] 
        public bool destroyAfter; 
        int index = 0; 
        float nextTimeChange = 0; 
        float timer = 0; 

        public void Awake()
        {
            nextTimeChange = interval;
            transform.GetComponent<SpriteRenderer>().sprite = sprites[index];
        }
        public void Update()
        {
            timer += Time.deltaTime;
            if (timer >= nextTimeChange)
            {
                index++;
                if (sprites.Count == index)
                {
                    if (destroyAfter)
                    {
                        Destroy(this.gameObject);
                    }
                    else
                    {
                        index = 0;
                    }
                }
                nextTimeChange += interval;
                if (sprites.Count != index)
                    transform.GetComponent<SpriteRenderer>().sprite = sprites[index];
            }
        }
    }
}

