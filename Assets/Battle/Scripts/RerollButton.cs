using UnityEngine;
using UnityEngine.UI;

namespace Battle
{
    [RequireComponent(typeof(Button))]
    public class RerollButton : MonoBehaviour
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
                    diceSystem.OnRerollClicked();
                }
                m_button.OnDeselect(null);
            });
        }
    }
}
