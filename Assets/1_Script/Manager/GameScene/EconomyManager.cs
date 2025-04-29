using UnityEngine;
using Unity.Netcode;
using IUtil;

namespace Garage.Manager
{
    public class EconomyManager : NetworkBehaviour
    {
        #region Singleton
        private static EconomyManager instance;
        public static EconomyManager Instance { get => instance; }

        void Awake()
        {
            Init();
        }

        private void Init()
        {
            if (null == instance)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
            }
            else
            {
                Destroy(this.gameObject);
            }
        }
        #endregion

        //서버에서만 관리
        private float balance;
        public float Balance { get => balance; }

        private void LoadBalance() // Host에서 저장된 값 로드
        {
            
        }

        [Button]
        public void TmpInitBalByServer()
        {
            SetBalanceServerRPC(0f);
        }

        [Button]
        public void TmpAddMoney()
        {
            EarnMoneyServerRPC(100f);
        }

        private void SetBalance(float bal)
        {
            balance = bal;
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetBalanceServerRPC(float bal)
        {
            SetBalance(bal);
            Debug.Log("Server Balance: " + balance);
            SetBalanceClientRPC(bal);
        }

        [ClientRpc]
        private void SetBalanceClientRPC(float bal)
        {
            if (IsHost) return;

            SetBalance(bal);
            Debug.Log("Client Balance: " + balance);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EarnMoneyServerRPC(float pay)
        {
            balance += pay;
            SetBalanceClientRPC(balance);
            Debug.Log("Server Balance: " + balance);
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseMoneyServerRPC(float fee)
        {
            balance -= fee;
            SetBalanceClientRPC(balance);
        }
    }
}