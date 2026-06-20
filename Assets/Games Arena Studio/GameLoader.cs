#if GamesArena_Firebase_RemoteConfig
using GamesArena.Firebase;
#endif
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameLoader : MonoBehaviour
{
    [Space(), Header("Scenes")]
    [SerializeField] private string sceneName;

    [Space(), Header("Panels")]
    [SerializeField] private GameObject LoadingPanel;
    [SerializeField] private GameObject PrivacyPolicyPanel;

    [SerializeField] private Image fill;
    [SerializeField] private bool FillAmountOnly;
    [SerializeField] private Text percent;
    [SerializeField] private bool waitForAdLoading;

    [SerializeField] private float loadingTime;
    public RectTransform loadingFillIcon; // The moving icon

    private bool JanayDe;

    private void Awake()
    {
        //  ShowLoading();
    }
    private void Start()
    {
        PlayerPrefs.SetInt("Session", PlayerPrefs.GetInt("Session") + 1);

#if GamesArena_Firebase_RemoteConfig
        FirebaseManager.Main.LogEvent($"s_{PlayerPrefs.GetInt("Session")}_loading_screen", this);
#endif

        ReloadScene();
    }

    private void ReloadScene()
    {

        StartCoroutine(ReloadTime());

        IEnumerator ReloadTime()
        {
            AnimateLoading(loadingTime);

            yield return new WaitForSeconds(loadingTime / 2);

#if All_Ads_Manager
            if (waitForAdLoading) yield return StartCoroutine(AllAdsController.instance.WaitForAdLoading());
#endif

            LogOnlineSession();

            LoadingPanel.SetActive(true);

            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            asyncOperation.allowSceneActivation = false;

            float progress = Mathf.Clamp01(asyncOperation.progress / 0.9f);

            yield return new WaitForSeconds(loadingTime / 2);

            asyncOperation.allowSceneActivation = true;


            yield return null;


        }
    }

    public void PrivacyPolicyOpen()
    {
        Time.timeScale = 0;
        LoadingPanel.SetActive(false);
        if (PrivacyPolicyPanel != null) PrivacyPolicyPanel.SetActive(true);
    }

    public void OpenURL(string Url)
    {
        Application.OpenURL(Url);
    }
    public void PrivacyPolicy()
    {
        Application.OpenURL("https://sites.google.com/view/varietyvista/home");
    }
    public void PrivacyPolicyClose()
    {
        Time.timeScale = 1;
        LoadingPanel.SetActive(true);
        if (PrivacyPolicyPanel != null) PrivacyPolicyPanel.SetActive(false);
    }
    public void AcceptPrivacy()
    {
        PlayerPrefs.SetInt("Privacy", 1);
        ShowLoading();
    }


    void AnimateLoading(float Duration)
    {
        // StartWheelieAnimation();

        StartCoroutine(Fill());
        IEnumerator Fill()
        {
            float width = fill.GetComponent<RectTransform>().rect.width;
            float currentTime = 0;
            while (currentTime < Duration)
            {
                fill.fillAmount = currentTime / Duration;

                if (percent != null) percent.text = (fill.fillAmount * 100).ToString("00") + "%";

                // Get the fill area's width

                if (loadingFillIcon != null)
                {
                    //  Update icon's local position along X
                    Vector2 newPos = loadingFillIcon.localPosition;
                    newPos.x = fill.fillAmount * width - width * 0.5f; // shift to center-based anchor
                    loadingFillIcon.localPosition = newPos;
                }

                currentTime += Time.deltaTime;
                yield return null;
            }
        }
    }

    public void ShowLoading()
    {
        if (PlayerPrefs.GetInt("Privacy", 0) == 1)
        {
            PrivacyPolicyClose();
        }
        else
        {
            PrivacyPolicyOpen();
        }
    }

    private void LogOnlineSession()
    {
#if GamesArena_Firebase_RemoteConfig
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            FirebaseManager.Main.LogEvent("Offline_Session", this);
        }
        else
        {
            FirebaseManager.Main.LogEvent("Online_Session", this);
        }
#endif
    }
}
