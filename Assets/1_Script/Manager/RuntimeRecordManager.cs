using Garage.Utils;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Manager
{

	public class RuntimeRecordManager
	{
		private Dictionary<ulong, GameplayRecordData> playerRecords = new();

		public void Init()
		{
		}

		public void Start()
		{
			GameManagerEx.Instance.OnStartGameAction += ResetData;
		}

		public void RecordData(ulong netId, RuntimeRecordType type, float value)
		{
			Debug.Log("RECORD : " + netId + ", " + type + ", " + value);	
			if (!playerRecords.ContainsKey(netId))
				playerRecords[netId] = new GameplayRecordData(netId);

			playerRecords[netId].AddValue(type, value);	
		}
		
		public void RecordData(ulong netId, RuntimeRecordType type, int value)
		{
			RecordData(netId, type, (float)value);
		}

		public void ResetData(int stageIdx)
		{
			Debug.Log("[Runtime Record Manager] - Data Reset");

			playerRecords.Clear();
			foreach(var netId in NetworkManager.Singleton.ConnectedClientsIds)
			{
				playerRecords[netId] = new GameplayRecordData(netId);
			}
		}

		// TODO - 원하는 데이터를 가공 및 집계해서 결과를 반환해야됨
		// 해당 결과를 반환할 클래스는 GameplayRecordData
		[Obsolete("미구현")]
		public List<GameplayRecordData> GetData()
		{
			return null;
		}

	}
}