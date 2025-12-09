using Game.Player;
using UnityEngine;
using UnityEngine.UI;

public class UIBatteryBar : MonoBehaviour
{
    Image bar;
    void Start()
    {
        bar = GetComponent<Image>();
        TopDownPlayerController.Instance.BatteryChanged += OnBatteryChanged;
    }

    void OnDisable()
    {
        if (TopDownPlayerController.Instance!=null)
            TopDownPlayerController.Instance.BatteryChanged -= OnBatteryChanged;
    }

    void OnBatteryChanged(float amount)
    {
        bar.fillAmount = amount;
    }
}
