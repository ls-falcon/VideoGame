using TMPro;
using UnityEngine;
using System.Collections;

public class UpgradeBannerUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI messageText;

    [SerializeField] private float showTime = 2f;

    Coroutine currentRoutine;

    void Start()
    {
        root.SetActive(false);
    }

    public void ShowMessage(string msg)
    {
        Debug.Log("Mostrando banner: " + msg);

        if (currentRoutine != null)
        {
            StopCoroutine(currentRoutine);
        }

        currentRoutine = StartCoroutine(
            ShowRoutine(msg)
        );
    }

    IEnumerator ShowRoutine(string msg)
    {
        root.SetActive(true);

        messageText.text = msg;

        yield return new WaitForSeconds(showTime);

        root.SetActive(false);
    }
}