using Garage.Manager;
using Garage.Utils;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace Garage.Environment
{
    public class PuddlePool : MonoBehaviour
    {

        [SerializeField] private GameObject oilPuddlePrefab; // 웅덩이 프리팹
        [SerializeField] private int poolSize = 20;       // 처음에 미리 만들어둘 개수

        private Queue<GameObject> poolQueue = new Queue<GameObject>();

        #region Singleton
        private static PuddlePool instance;
        public static PuddlePool Instance { get => instance; }

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

        public void InitializePool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                GameObject obj = Instantiate(oilPuddlePrefab, transform);
                obj.SetActive(false);
                poolQueue.Enqueue(obj);
            }
        }

        public GameObject GetPuddle()
        {
            if (poolQueue.Count > 0)
            {
                GameObject obj = poolQueue.Dequeue();
                obj.SetActive(true);
                return obj;
            }
            else
            {
                GameObject obj = Instantiate(oilPuddlePrefab, transform);
                obj.SetActive(true);
                return obj; // 큐에 넣지 않고 반환 (나중에 ReturnPuddle로 들어옴)
            }
        }

        public void ReturnPuddle(GameObject obj)
        {
            obj.SetActive(false);
            poolQueue.Enqueue(obj);
        }
    }
}
