using UnityEngine;
using Panahon.Data;

namespace Panahon.Board
{
    public class TimeGatePortal : MonoBehaviour
    {
        [Header("Portal Settings")]
        [SerializeField] private EraType sourceEra;
        [SerializeField] private int baseEnergyCost = 2;
        [SerializeField] private ParticleSystem portalParticles;
        [SerializeField] private Light portalLight;

        public EraType SourceEra => sourceEra;
        public int BaseEnergyCost => baseEnergyCost;

        public void Setup(EraType era, int energyCost = 2)
        {
            sourceEra = era;
            baseEnergyCost = energyCost;
        }

        public void PlayPortalEffect()
        {
            if (portalParticles != null)
            {
                portalParticles.Play();
            }
            if (portalLight != null)
            {
                portalLight.intensity = 3.5f;
            }
        }
    }
}
