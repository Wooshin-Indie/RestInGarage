using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace Garage.Manager
{
    [System.Serializable]
    public class VFXData
    {
        public VFXType type;
        public GameObject prefab;
        [Tooltip("오브젝트 풀에 미리 생성해 둘 개수")]
        public int poolSize = 5;
        [Tooltip("풀이 비었을 때 추가 생성을 허용할지 여부")]
        public bool allowPoolExpansion = true;
        [Tooltip("이펙트의 예상 최대 지속 시간 (초). 자동 반환 로직에 사용됨.")]
        public float estimatedDuration = 2.0f; // 정확하지 않아도 됨, ParticleSystem.main.duration + startLifetime.constantMax 가 더 정확
    }
    public enum VFXType
    {
        None = -1,
        EngineSmoke,
        FireExtingusher,
        RepairSwing,
        AllPartsRepaired
    }
    // 활성 루핑 VFX 추적용 내부 클래스
    internal class ActiveLoopingVFX
    {
        public ParticleSystem ParticleInstance;
        public VFXType Type;
        public Coroutine StopCoroutineRef; // Stop 호출 시 관련 코루틴 중지용 (선택적 최적화)
    }


    public class VFXManager : MonoBehaviour
    {
        #region Singleton
        private static VFXManager instance;
        public static VFXManager Instance { get => instance; }

        private void Awake()
        {
            Init();

            InitPool();
        }

        private void Init()
        {
            if (instance == null)
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

        [Header("VFX Settings")]
        [SerializeField] private List<VFXData> vfxList; // Inspector에서 설정할 VFX 목록

        #region Object Pooling

        private Transform poolRoot;

        // Hierarchy 정리용
        private Dictionary<VFXType, Transform> vfxPoolTransform = new Dictionary<VFXType, Transform>();
        // 오브젝트 풀: VFXType별로 ParticleSystem 큐를 가짐
        private Dictionary<VFXType, Queue<ParticleSystem>> vfxPool = new Dictionary<VFXType, Queue<ParticleSystem>>();
        // 프리팹 원본을 저장하기 위한 딕셔너리 (풀 확장 시 사용)
        private Dictionary<VFXType, VFXData> vfxDataMap = new Dictionary<VFXType, VFXData>();
        private Dictionary<int, ActiveLoopingVFX> activeLoopingEffects = new Dictionary<int, ActiveLoopingVFX>();
        private int nextLoopingEffectId = 0;

        private void InitPool()
        {
            poolRoot = new GameObject { name = "_poolRoot" }.transform;
            poolRoot.parent = instance.transform;

            foreach (VFXData data in vfxList)
            {
                vfxPoolTransform[data.type] = new GameObject { name = $"poolQueue_{data.type}" }.transform;
                vfxPoolTransform[data.type].parent = poolRoot;

                vfxDataMap[data.type] = data; // 나중에 프리팹 참조를 위해 저장
                Queue<ParticleSystem> objectQueue = new Queue<ParticleSystem>();
                vfxPool[data.type] = objectQueue;

                for (int i = 0; i < data.poolSize; i++)
                {
                    GameObject instanceGO = Instantiate(data.prefab, vfxPoolTransform[data.type]); // Hierarchy 정리
                    instanceGO.SetActive(false);
                    ParticleSystem ps = instanceGO.GetComponent<ParticleSystem>();
                    // ParticleSystem이 없는 프리팹일 수도 있으므로 null 체크
                    if (ps != null)
                    {
                        objectQueue.Enqueue(ps);
                        CheckStopActionSetting(ps.main, data.type, data.prefab.name);
                    }
                }

            }
        }

        // StopAction 설정 확인 (경고용)
        private void CheckStopActionSetting(ParticleSystem.MainModule mainModule, VFXType type, string prefabName)
        {
            if (!mainModule.loop && mainModule.stopAction == ParticleSystemStopAction.Destroy)
            {
                Debug.LogWarning($"VFXManager: One-shot VFX '{type}' ('{prefabName}') has StopAction set to Destroy. Consider using None, Disable, or Callback for pooling.", this);
            }
            else if (mainModule.loop && mainModule.stopAction == ParticleSystemStopAction.Destroy)
            {
                Debug.LogWarning($"VFXManager: Looping VFX '{type}' ('{prefabName}') has StopAction set to Destroy. StopLoopingVFX might conflict. Consider using None or Disable.", this);
            }
        }

        #endregion

        #region Play Methods

        /// <summary>
        /// One-shot (Looping 비활성화) VFX를 재생합니다. 재생 완료 후 자동으로 풀에 반환됩니다.
        /// </summary>
        /// <returns>재생된 ParticleSystem 인스턴스 (제어 필요시 사용, null일 수 있음)</returns>
        public ParticleSystem PlayVFX(VFXType type, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            ParticleSystem instancePS = GetPooledInstance(type);
            if (instancePS == null) return null;

            // --- 루핑 확인 (One-shot용 함수이므로 경고) ---
            if (instancePS.main.loop)
            {
                Debug.LogWarning($"VFXManager: Attempting to play a looping VFX '{type}' ('{instancePS.name}') using PlayVFX (for one-shots). Use PlayLoopingVFX instead if you need to stop it later.", instancePS.gameObject);
                // 루핑이라도 일단 재생은 시키지만, 자동 반환 로직은 다를 수 있음
            }

            // --- 위치, 회전, 부모 설정 및 활성화 ---
            SetupAndActivate(instancePS.gameObject, position, rotation, parent);

            // --- 재생 및 자동 반환 코루틴 시작 ---
            instancePS.Play();
            StartCoroutine(ReturnToPoolAfterCompletion(instancePS, type)); // 완료 후 자동 반환

            return instancePS;
        }

        /// <summary>
        /// Looping VFX를 재생 시작하고 고유 ID를 반환합니다. StopLoopingVFX로 중지해야 합니다.
        /// </summary>
        /// <returns>활성화된 루핑 VFX의 고유 ID (오류 시 -1)</returns>
        public int PlayLoopingVFX(VFXType type, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            ParticleSystem instancePS = GetPooledInstance(type);
            if (instancePS == null) return -1;

            // --- 위치, 회전, 부모 설정 및 활성화 ---
            SetupAndActivate(instancePS.gameObject, position, rotation, parent, false);

            // --- 재생 및 활성 목록에 추가 ---
            instancePS.Play();

            int id = nextLoopingEffectId++;
            ActiveLoopingVFX activeVFX = new ActiveLoopingVFX { ParticleInstance = instancePS, Type = type };
            activeLoopingEffects.Add(id, activeVFX);

            // Debug.Log($"Started looping VFX '{type}' with ID: {id}");
            return id;
        }

        // --- 편의 오버로드 (기본 회전 사용) ---
        public ParticleSystem PlayVFX(VFXType type, Vector3 position, Transform parent = null)
        {
            Quaternion rotation = vfxDataMap.ContainsKey(type) ? vfxDataMap[type].prefab.transform.rotation : Quaternion.identity;
            return PlayVFX(type, position, rotation, parent);
        }

        public int PlayLoopingVFX(VFXType type, Vector3 position, Transform parent = null)
        {
            Quaternion rotation = vfxDataMap.ContainsKey(type) ? vfxDataMap[type].prefab.transform.rotation : Quaternion.identity;
            return PlayLoopingVFX(type, position, rotation, parent);
        }

        #endregion

        #region Stop Methods

        /// <summary>
        /// 지정된 ID의 활성 루핑 VFX를 중지하고 풀에 반환합니다.
        /// </summary>
        /// <param name="id">중지할 루핑 VFX의 ID</param>
        /// <param name="behavior">중지 방식 (즉시 제거 또는 방출만 중지)</param>
        public void StopLoopingVFX(int id, ParticleSystemStopBehavior behavior = ParticleSystemStopBehavior.StopEmittingAndClear)
        {
            if (activeLoopingEffects.TryGetValue(id, out ActiveLoopingVFX activeVFX))
            {
                // Debug.Log($"Attempting to stop looping VFX '{activeVFX.Type}' with ID: {id}");

                // 이미 Stop -> ReturnToPool 코루틴이 실행중이라면 중지 (선택적 최적화)
                if (activeVFX.StopCoroutineRef != null)
                {
                    StopCoroutine(activeVFX.StopCoroutineRef);
                }

                // 파티클 시스템 중지 및 풀 반환 처리
                ReturnInstanceToPool(activeVFX.ParticleInstance, activeVFX.Type, behavior);

                // 추적 목록에서 제거
                activeLoopingEffects.Remove(id);
            }
            else
            {
                // Debug.LogWarning($"VFXManager: No active looping VFX found with ID: {id}. Could not stop.");
            }
        }

        // 필요하다면 특정 타입의 모든 루핑 VFX 중지 함수 등 추가...
        public void StopAllLoopingVFXByType(VFXType type, ParticleSystemStopBehavior behavior = ParticleSystemStopBehavior.StopEmittingAndClear)
        {
            List<int> idsToStop = new List<int>();
            foreach (var pair in activeLoopingEffects)
            {
                if (pair.Value.Type == type)
                {
                    idsToStop.Add(pair.Key);
                }
            }
            foreach (int id in idsToStop)
            {
                StopLoopingVFX(id, behavior);
            }
            // Debug.Log($"Stopped {idsToStop.Count} looping VFX of type '{type}'.");
        }

        #endregion

        #region Pooling Internals

        // 풀에서 인스턴스를 가져오거나 새로 생성하는 내부 함수
        private ParticleSystem GetPooledInstance(VFXType type)
        {
            if (type == VFXType.None || !vfxDataMap.ContainsKey(type) || !vfxPool.ContainsKey(type))
            {
                Debug.LogError($"VFXManager: VFXType '{type}' not found or not initialized properly.", this);
                return null;
            }

            VFXData data = vfxDataMap[type];
            Queue<ParticleSystem> queue = vfxPool[type];
            ParticleSystem ps = null;

            if (queue.Count > 0)
            {
                ps = queue.Dequeue();
                if (ps == null) // 풀 안에 있던 객체가 파괴된 경우 (예외 상황)
                {
                    Debug.LogError($"VFXManager: A pooled object for '{type}' was destroyed unexpectedly. Trying to get another.", this);
                    return GetPooledInstance(type); // 재귀 호출로 다음 것 시도 (무한 루프 주의)
                }
            }
            else if (data.allowPoolExpansion)
            {
                ps = Instantiate(data.prefab, transform).GetComponent<ParticleSystem>();
                if (ps == null)
                {
                    Debug.LogError($"VFXManager: Failed to instantiate or get ParticleSystem from prefab '{data.prefab.name}' for '{type}'.", this);
                    return null;
                }
                Debug.Log($"VFXManager: Pool for '{type}' was empty, created new instance. Consider increasing pool size for '{data.prefab.name}'.", ps.gameObject);
                CheckStopActionSetting(ps.main, type, data.prefab.name); // 새로 만든 것도 검사
            }
            else
            {
                Debug.LogWarning($"VFXManager: Pool for '{type}' is empty and expansion is not allowed. VFX not played.", this);
                return null;
            }

            return ps;
        }

        // 인스턴스 활성화 및 설정 공통 로직
        private void SetupAndActivate(GameObject instanceGO, Vector3 position, Quaternion rotation,
                            Transform parent, bool worldPositionStays = true)
        {
            instanceGO.transform.position = position;
            instanceGO.transform.rotation = rotation;
            if (parent != null)
            {
                instanceGO.transform.SetParent(parent, worldPositionStays); // worldPositionStays = true
            }
            else
            {
                // instanceGO.transform.SetParent(transform); // Manager를 기본 부모로
            }
            instanceGO.gameObject.SetActive(true);
        }

        // One-shot VFX 자동 반환 코루틴
        private IEnumerator ReturnToPoolAfterCompletion(ParticleSystem ps, VFXType type)
        {
            // ParticleSystem이 재생 완료될 때까지 기다림
            // isPlaying은 방출이 멈춰도 파티클이 남아있으면 true일 수 있음
            // 따라서 실제로는 파티클이 모두 사라질 때까지 기다리는 것이 더 정확
            yield return new WaitWhile(() => ps.IsAlive(true)); // 모든 파티클(자식 포함)이 사라질 때까지 대기

            // 반환 처리
            ReturnInstanceToPool(ps, type, ParticleSystemStopBehavior.StopEmittingAndClear); // 완료 후에는 보통 Clear
        }

        // 인스턴스를 풀에 반환하는 내부 함수
        private void ReturnInstanceToPool(ParticleSystem ps, VFXType type, ParticleSystemStopBehavior behavior)
        {
            if (ps == null) return; // 이미 파괴되었을 수 있음

            if (ps.gameObject.activeSelf) // 아직 활성화 상태라면
            {
                ps.Stop(true, behavior);
                ps.gameObject.SetActive(false);
            }
            // 이미 비활성화된 경우라도 풀에는 다시 넣어야 함


            if (vfxPool.TryGetValue(type, out Queue<ParticleSystem> queue))
            {
                // 풀에 다시 넣기 전에 혹시 이미 들어있는지 확인 (선택적, Queue는 Contains 비효율적)
                queue.Enqueue(ps);
                ps.transform.SetParent(vfxPoolTransform[type]); // 부모 리셋
            }
            else
            {
                Debug.LogWarning($"VFXManager: Pool queue not found for type '{type}' when returning instance '{ps.name}'. Destroying instance.", ps.gameObject);
                Destroy(ps.gameObject); // 풀이 없으면 파괴
            }
        }

        #endregion

        // 게임 종료 또는 씬 전환 시 정리 (선택 사항)
        void OnDestroy()
        {
            // 활성 루핑 이펙트 중지 및 정리 (선택적)
            List<int> allActiveIds = new List<int>(activeLoopingEffects.Keys);
            foreach (int id in allActiveIds)
            {
                StopLoopingVFX(id, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
            activeLoopingEffects.Clear();

            // 풀 자체는 Manager가 파괴되면서 자식들도 함께 파괴될 수 있음
            // 명시적으로 풀 안의 객체를 파괴하고 싶다면 추가 로직 필요
            vfxPool.Clear();
            vfxDataMap.Clear();

            if (instance == this)
            {
                instance = null;
            }
        }
    }

}