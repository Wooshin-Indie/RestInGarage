using UnityEngine;

namespace Garage.Props
{
    public class OilGun : MonoBehaviour
    {
        [SerializeField] private ParticleSystem oilParticle;

        private void Awake()
        {
            oilParticle.Stop();
        }
        public void DelayedStartOilSpray()// 바로 쏘면 포즈 잡기전에 파티클 나옴
        {
            Invoke("StartOilSpray", 0.2f);
        }

        public void StartOilSpray()
        {
            oilParticle.Play();
        }

        public void StopOilSpray()
        {
            CancelInvoke();
            oilParticle.Stop();
        }

        private void OnDestroy()
        {
            Debug.Log("------------Oilgun Destroyed");
        }
    }
}
