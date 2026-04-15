using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    [RequireComponent(typeof(Button))]
    public class StopButton : MonoBehaviour
    {
        private Button m_button;

        private void Start()
        {
            m_button = GetComponent<Button>();
            m_button.onClick.AddListener(() =>
            {
                var diceSystem = FindObjectOfType<DiceSystem>();
                if (diceSystem != null)
                {
                    diceSystem.OnStopClicked();
                }
                m_button.OnDeselect(null);
            });
        }
    }
}
