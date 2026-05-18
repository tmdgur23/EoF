using Cards.General;
using Misc.PopUp;
using UnityEngine;
using Utilities;

#pragma warning disable 0649
namespace Misc
{
	[RequireComponent(typeof(KeywordHandler))]
	public class HUDToolTip : MonoBehaviour, ISimplePointer
	{
		public bool IsActive { get; set; } = true;
		[SerializeField] private string m_header;
		[SerializeField, TextArea] private string m_text;
		[SerializeField] private RectTransform m_targetRect;
		[SerializeField] private RectAnchor m_rectAnchor;
		[SerializeField] private KeywordHandler KeywordHandler;
		private string m_textToDisplay;

		private void Start()
		{
			KeywordHandler = GetComponent<KeywordHandler>();

			// 한글 번역 처리
			if (m_header == "Enemy Soul")
			{
				m_header = "적의 영혼";
			}
			if (m_text != null)
			{
				if (m_text.Contains("Like you your enemies also have a soul"))
				{
					m_text = "당신과 마찬가지로 적들도 영혼을 가지고 있습니다.\n영혼이 정화되면 적들은 싸움을 멈출 것입니다.";
				}
				else if (m_text.Contains("Like your health, your soul will be attacked"))
				{
					m_text = "체력과 마찬가지로, 당신의 영혼 역시 수많은 적들로부터 공격을 받습니다. 영혼이 완전히 타락하면 게임에서 패배합니다.";
				}
			}

			m_textToDisplay = KeywordHandler.ParseKeywords(m_text);
			KeywordHandler.OverrideSettings(m_targetRect, m_rectAnchor);
		}

		public void OnEnter()
		{
			PopUpHandler.Instance.OpenTextPopUp(m_header, m_textToDisplay, m_targetRect,
												m_rectAnchor);
			KeywordHandler.EnableKeywords();
		}

		public void OnExit()
		{
			KeywordHandler.DisableKeywords();
		}
	}
}
#pragma warning restore 0649