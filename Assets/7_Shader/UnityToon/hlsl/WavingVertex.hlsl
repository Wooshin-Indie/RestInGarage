#ifndef WAVING_VERTEX_INCLUDED
#define WAVING_VERTEX_INCLUDED

// Properties에서 선언한 변수들을 사용하기 위해 CBUFFER에 등록합니다.
CBUFFER_START(UnityPerMaterial)
float _WaveSpeed;
float _WaveRange;
float _WaveFrequency;
CBUFFER_END

// 정점 위치를 변형시키는 함수
float3 ApplyWaveAnimation(float3 positionOS, float2 uv) // positionOS == positionObjectSpace
{
    float mask = uv.y;

    // 월드 위치는 계산 시작과 끝에서 모두 필요하므로 먼저 구해둡니다.
    float3 worldPos = mul(unity_ObjectToWorld, float4(positionOS, 1.0)).xyz;
    
    // 이 오브젝트의 위치와 시간에 따른 고유한 흔들림 값을 계산합니다.
    float waveOffset = sin(worldPos.z * _WaveFrequency + _Time.y * _WaveSpeed) * _WaveRange;
    
    //    월드 공간에서 움직임 추가
    //    흔들릴 방향을 월드 공간 기준으로 정의합니다. float3(0, 0, 0)은 월드 Z축 방향
    float3 windDirectionWS = float3(0.0, 0.0, 1.0); 

    //    원래 월드 위치에 (바람 방향 * 흔들림 양 * 마스크)를 더해 새로운 월드 위치를 계산
    float3 newWorldPos = worldPos + (windDirectionWS * waveOffset * mask);
    
    //    월드 -> 로컬
    //    새로운 월드 위치를 다시 오브젝트의 로컬 공간으로 변환합니다.
    //    unity_WorldToObject는 unity_ObjectToWorld의 정반대 역할을 하는 마법의 행렬입니다.
    float3 newPositionOS = mul(unity_WorldToObject, float4(newWorldPos, 1.0)).xyz;
    
    //    최종적으로 계산된 새로운 로컬 위치를 반환합니다.
    return newPositionOS;
}

#endif // WAVING_VERTEX_INCLUDED