using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Garage.Environment
{
    public class OilPuddle : MonoBehaviour
    {
        [SerializeField] private float lifeTime = 5.0f;
        [SerializeField] private float fadeDuration = 2.0f;

        private Renderer render;
        private Color originalColor;
        private float timer;

        private void Awake()
        {
            render = GetComponent<Renderer>();
            if (render != null) originalColor = render.material.color;
        }

        private void OnEnable()
        {
            timer = lifeTime;
            if (render != null)
            {
                Color resetColor = originalColor;
                resetColor.a = 1f;
                render.material.color = resetColor;
            }
        }

        private void Update()
        {
            timer -= Time.deltaTime;

            if (timer <= fadeDuration)
            {
                float alpha = Mathf.Clamp01(timer / fadeDuration);
                if (render != null)
                {
                    Color newColor = originalColor;
                    newColor.a = originalColor.a * alpha;
                    render.material.color = newColor;
                }

                if (timer <= 0)
                {
                    if (PuddlePool.Instance != null)
                        PuddlePool.Instance.ReturnPuddle(this.gameObject);
                    else
                        Destroy(gameObject);
                }
            }
        }
    }
}