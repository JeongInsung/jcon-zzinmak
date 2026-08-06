using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Zzinmak.Core;
using Zzinmak.Data;

public class CardInfo : MonoBehaviour
{
    public Image icon;
    public TMP_Text NameTitle;
    public TMP_Text Description;

    [SerializeField] private float popInDuration = 0.25f;
    [SerializeField] private float clickPunchScale = 1.15f;
    [SerializeField] private float clickPunchDuration = 0.18f;

    private ICardOption option;
    private Action<ICardOption> onClicked;
    private Button button;

    public void Setup(ICardOption data, Action<ICardOption> callback, float appearDelay = 0f)
    {
        option = data;
        onClicked = callback;

        if (icon != null) icon.sprite = data.CardIcon;
        if (NameTitle != null) NameTitle.text = data.CardTitle;
        if (Description != null) Description.text = data.CardDescription;

        button = GetComponent<Button>();
        if (button != null)
        {
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        StopAllCoroutines();
        StartCoroutine(PopIn(appearDelay));
    }

    private void HandleClick()
    {
        if (button != null) button.interactable = false;
        if (UiClickSound.Instance != null) UiClickSound.Instance.Play();

        StopAllCoroutines();
        StartCoroutine(PunchThenSelect());
    }

    // Plays the punch feedback before invoking the callback so it's actually visible -
    // the level-up screen destroys/hides every card the instant a choice is confirmed.
    private IEnumerator PunchThenSelect()
    {
        yield return PunchScale();
        onClicked?.Invoke(option);
    }

    private IEnumerator PunchScale()
    {
        float half = clickPunchDuration * 0.5f;

        float elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(1f, clickPunchScale, elapsed / half);
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < half)
        {
            elapsed += Time.unscaledDeltaTime;
            transform.localScale = Vector3.one * Mathf.Lerp(clickPunchScale, 1f, elapsed / half);
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    // Uses unscaled time since the level-up screen pauses the game (Time.timeScale = 0),
    // and eases past 1 briefly for a springy "pop" instead of just growing linearly.
    private IEnumerator PopIn(float delay)
    {
        transform.localScale = Vector3.zero;

        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        float elapsed = 0f;
        while (elapsed < popInDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / popInDuration);

            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float eased = 1f + c3 * Mathf.Pow(t - 1f, 3f) + c1 * Mathf.Pow(t - 1f, 2f);

            transform.localScale = Vector3.one * eased;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }
}
