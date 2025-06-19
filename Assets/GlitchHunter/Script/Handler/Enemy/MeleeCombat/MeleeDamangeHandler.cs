using GlitchHunter.Constant;
using GlitchHunter.Enum;
using GlitchHunter.Manager;
using System.Runtime.InteropServices;
using UnityEngine;

namespace GlitchHunter.Handler.Enemy.MeleeCombat
{
    public class MeleeDamangeHandler : MonoBehaviour
    {
        [SerializeField]
        private float damangeAmount = 60;
        [SerializeField]
        private EnemyMeleeCombatSystem enemyMeleeCombatSystem;

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                enemyMeleeCombatSystem.OnAttackHit(damangeAmount, other.gameObject.GetComponent<PlayerHealthHandler>());
            }
        }
    }
}
