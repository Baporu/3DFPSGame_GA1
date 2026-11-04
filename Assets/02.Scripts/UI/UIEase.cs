using UnityEngine;

// 목표: UI 연출에서 공통으로 쓰는 이징 함수를 모아둔다.
public static class UIEase
{
    private const float BackOvershoot = 1.70158f;

    // 처음엔 빠르고 끝에서 천천히 멈춘다.
    public static float OutCubic(float t)
    {
        float inverse = 1f - Mathf.Clamp01(t);
        return 1f - inverse * inverse * inverse;
    }

    // 목표값을 살짝 넘었다가 돌아오는 '쫄깃한' 곡선.
    public static float OutBack(float t)
    {
        float x = Mathf.Clamp01(t) - 1f;
        return 1f + (BackOvershoot + 1f) * x * x * x + BackOvershoot * x * x;
    }
}
