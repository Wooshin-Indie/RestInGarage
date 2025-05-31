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

        public NetworkVariable<int> Balance = new();

		public override void OnNetworkSpawn()
		{
            base.OnNetworkSpawn();

            Balance.OnValueChanged += UIManager.Game.OnBalancedChanged;
            UIManager.Game.OnBalancedChanged(0, Balance.Value);
		}

        public bool HasEnoughMoney(int money)
        {
            return Balance.Value <= money;
        }

        public bool UseMoney_HostOnly(int pay)
        {
            if (!IsHost) return false;
            if (Balance.Value < pay) return false;

            Balance.Value -= pay;
            return true;
        }

        public void EraseMoney_HostOnly(int pay)
        {
            if (!IsHost) return;

            if (Balance.Value < pay) Balance.Value = 0;
			else Balance.Value -= pay;
		}

        public void EarnMoney_HostOnly(int money)
		{
            if (!IsHost) return;

			Balance.Value += money;
		}

		[Button]
        public void TmpSetBalance()
        {
			EarnMoneyServerRPC(100);
        }

        [ServerRpc(RequireOwnership = false)]
        private void SetBalanceServerRPC(int bal)
		{
			Balance.Value = bal;
			Debug.Log("Server Balance: " + Balance.Value);
        }


        [ServerRpc(RequireOwnership = false)]
        public void EarnMoneyServerRPC(int pay)
        {
			Balance.Value += pay;
        }

        [ServerRpc(RequireOwnership = false)]
        public void UseMoneyServerRPC(int fee)
        {
            float tmpBal = Balance.Value - fee;
            if (tmpBal < 0f)
                Debug.LogWarning("Exception: not enough balance");
            else
            {
				Balance.Value -= fee;
            }
        }
    }
}