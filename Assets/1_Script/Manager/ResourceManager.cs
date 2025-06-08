using Garage.Structs;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Garage.Manager
{
	public class ResourceManager
	{
		private readonly string vehicleDataPath = "Data/ScriptableObject/VehicleDatas";
		private readonly string mapDataPath = "Data/ScriptableObject/MapDatas";
		private readonly string itemDataPath = "Data/ScriptableObject/ItemDatas";

		private Dictionary<Type, ScriptableObject[]> soDatas;

		public void Init()
		{
			if (soDatas != null)
			{
				throw new Exception("Resource Manager - already loaded");
			}

			soDatas = new();
			soDatas[typeof(VehicleData)] = Resources.LoadAll<VehicleData>(vehicleDataPath);
			soDatas[typeof(MapData)] = Resources.LoadAll<MapData>(mapDataPath);
			soDatas[typeof(ItemData)] = Resources.LoadAll<ItemData>(itemDataPath);
		}

		public int GetDataLength<T>()
		{
			if(soDatas == null || !soDatas.ContainsKey(typeof(T)) || soDatas[typeof(T)] == null) 
				throw new NotSupportedException("Resource Manager - Not Supported Type");

			return soDatas[typeof(T)].Length;
		}

		public T GetData<T>(int index)
		{
			if (soDatas == null || !soDatas.ContainsKey(typeof(T)) || soDatas[typeof(T)] == null)
				throw new NotSupportedException("Resource Manager - Not Supported Type");

			return (T)(object)soDatas[typeof(T)][index];
		}
	}
}