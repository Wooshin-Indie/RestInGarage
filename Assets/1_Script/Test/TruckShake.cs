using UnityEngine;


public class TruckShake : MonoBehaviour
{
    [SerializeField] private Transform truckTransform; // 차체 Transform을 Inspector에서 할당
    [SerializeField] private float shakeIntensity = 0.05f; // 덜컹거림의 최대 강도 (이동 거리)
    [SerializeField] private float shakeRotationIntensity = 1.0f; // 덜컹거림 시 회전 강도 (각도)
    [SerializeField] private float shakeFrequency = 5f;    // 덜컹거림의 빈도

    private Vector3 originalLocalPosition;
    private Quaternion originalLocalRotation;

    // Perlin Noise를 위한 오프셋 (각 축마다 다른 노이즈를 사용하기 위함)
    private float xOffset, yOffset, zOffset;
    private float rotXOffset, rotYOffset, rotZOffset;

    void Start()
    {
        if (truckTransform == null)
        {
            Debug.LogError("Truck Transform is not assigned!");
            enabled = false;
            return;
        }
        originalLocalPosition = truckTransform.localPosition;
        originalLocalRotation = truckTransform.localRotation;

        // 각 축과 회전에 대해 랜덤한 Perlin Noise 시작점 설정
        xOffset = Random.Range(0f, 100f);
        yOffset = Random.Range(0f, 100f);
        zOffset = Random.Range(0f, 100f);
        rotXOffset = Random.Range(0f, 100f);
        rotYOffset = Random.Range(0f, 100f);
        rotZOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (truckTransform == null) return;

        // Perlin Noise를 사용하여 부드럽고 불규칙한 움직임 생성
        float timeParam = Time.time * shakeFrequency;

        // 위치 변위 (주로 Y축 덜컹거림)
        float xShake = (Mathf.PerlinNoise(timeParam + xOffset, 0f) * 2f - 1f) * shakeIntensity * 0.2f; // X축은 약하게
        float yShake = (Mathf.PerlinNoise(0f, timeParam + yOffset) * 2f - 1f) * shakeIntensity;         // Y축은 주된 덜컹거림
        float zShake = (Mathf.PerlinNoise(timeParam + zOffset, timeParam + zOffset) * 2f - 1f) * shakeIntensity * 0.1f; // Z축은 매우 약하게

        // 회전 변위 (주로 X축(피칭), Z축(롤링) 덜컹거림)
        float rotX = (Mathf.PerlinNoise(timeParam + rotXOffset, 0f) * 2f - 1f) * shakeRotationIntensity;
        float rotY = (Mathf.PerlinNoise(0f, timeParam + rotYOffset) * 2f - 1f) * shakeRotationIntensity * 0.2f; // Y축 회전은 약하게
        float rotZ = (Mathf.PerlinNoise(timeParam + rotZOffset, timeParam + rotZOffset) * 2f - 1f) * shakeRotationIntensity;

        // 원래 위치/회전에 변위를 더함
        truckTransform.localPosition = originalLocalPosition + new Vector3(xShake, yShake, zShake);
        truckTransform.localRotation = originalLocalRotation * Quaternion.Euler(rotX, rotY, rotZ);
    }
}