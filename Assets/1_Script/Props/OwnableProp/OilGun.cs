using UnityEngine;

namespace Garage.Props
{
    public class OilGun : MonoBehaviour
    {
        [SerializeField] private ParticleSystem oilParticle;

        public void StartOilSpray()
        {
            oilParticle.Play();
        }

        public void StopOilSpray()
        {
            oilParticle.Stop();
        }
    }
}
