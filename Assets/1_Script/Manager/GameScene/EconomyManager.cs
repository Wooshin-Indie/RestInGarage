using UnityEngine;
using Unity.Netcode;
using IUtil;

namespace Garage.Manager
{
    public class EconomyManager : NetworkBehaviour
    {
        //서버에서만 관리
        private float balance;
        public float Balance { get => balance; }

        private void Awake()
        {
            balance = 0; // 나중에 저장한 곳에서 받아와야됨
        }

        //[Button()]
        [ServerRpc(RequireOwnership = false)]
        public void EarnMoneyServerRPC(float pay)
        {
            balance += pay;
            EarnMoneyClientRPC(balance);
        }

        [ClientRpc]
        private void EarnMoneyClientRPC(float bal)
        {
            if (IsHost) return;

            balance = bal;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseMoneyServerRPC(float fee)
        {
            balance -= fee;
            UseMoneyClientRPC(balance);
        }

        [ClientRpc]
        private void UseMoneyClientRPC(float bal)
        {
            if (IsHost) return;

            balance = bal;
        }
    }
}