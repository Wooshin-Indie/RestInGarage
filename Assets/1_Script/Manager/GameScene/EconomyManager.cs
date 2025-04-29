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
        public NetworkVariable<int> Balance = new();

        private void SetBalance(int bal)
        {
            Balance.Value = bal;
		}

        [Button]
        public void TmpSetBalance()
        {
            SetBalanceServerRPC(100);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetBalanceServerRPC(int bal)
        {
            SetBalance(bal);
            Debug.Log("Server Balance: " + Balance.Value);
            SetBalanceClientRPC(bal);
        }

        [ClientRpc]
        private void SetBalanceClientRPC(int bal)
        {
            if (IsHost) return;

            SetBalance(bal);
        }

        [ServerRpc(RequireOwnership = false)]
        public void EarnMoneyServerRPC(int pay)
        {
			Balance.Value += pay;
            SetBalanceClientRPC(Balance.Value); // 결과만 ClientRPC로 뿌림
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseMoneyServerRPC(int fee)
        {
            float tmpBal = Balance.Value - fee;
            if (tmpBal < 0f)
                Debug.LogError("Exception: not enough balance");
            else
            {
				Balance.Value -= fee;
                SetBalanceClientRPC(Balance.Value);
            }
        }
    }
}