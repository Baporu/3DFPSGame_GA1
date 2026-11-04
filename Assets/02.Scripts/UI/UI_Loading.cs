using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Loading : MonoBehaviour
{
    // allowSceneActivation이 false이면 progress는 0.9에서 멈추므로, 0.9를 100%로 환산한다.
    private const float LoadCompleteProgress = 0.9f;
    // 표시 값이 초당 얼마나 따라갈지 (1이면 1초에 100%)
    private const float DisplaySpeed = 1f;

    [SerializeField] private Slider _progressSlider;
    [SerializeField] private TextMeshProUGUI _progressText;


    private void Start()
    {
        StartCoroutine(Load_Coroutine());
    }

    private IEnumerator Load_Coroutine()
    {
        Application.backgroundLoadingPriority = ThreadPriority.Low;

        // 비동기 씬 로드
        // 반환: 씬의 비동기 진행사항을 확인할 수 있는 객체
        AsyncOperation operation = SceneManager.LoadSceneAsync(SceneLoader.NextSceneName);
        // 수많은 객체의 생성을 쪼개서 하나하나 생성할 준비
        operation.allowSceneActivation = false;

        float displayValue = 0f;
        Refresh(displayValue);

        while (operation.isDone == false)
        {
            // 0과 1 사이의 값으로 보간
            float target = Mathf.Clamp01(operation.progress / LoadCompleteProgress);

            // 로딩이 순식간에 끝나도 바가 점프하지 않고 부드럽게 차오르도록 따라간다.
            displayValue = Mathf.MoveTowards(displayValue, target, DisplaySpeed * Time.deltaTime);
            Refresh(displayValue);

            // 실제 로딩이 끝났고 바도 100%까지 찼을 때 씬을 전환한다.
            if (operation.progress >= LoadCompleteProgress && displayValue >= 1f)
            {
                operation.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }

    private void Refresh(float value)
    {
        _progressSlider.value = value;
        _progressText.text = $"불러오는 중 {Mathf.RoundToInt(value * 100)}%";
    }
}