using System.Collections.Generic;
using Cards.General;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

#pragma warning disable 0649
namespace Misc.PopUp
{
	public class PopUpHandler : MonoBehaviour
	{
		private static PopUpHandler m_instance;

		public static PopUpHandler Instance
		{
			get
			{
				if (m_instance == null)
				{
					m_instance = FindObjectOfType<PopUpHandler>();
				}

				return m_instance;
			}
		}

		[SerializeField] private RectTransform m_rect;
		[SerializeField] private VerticalLayoutGroup m_layoutGroup;
		[SerializeField] private TextPopUp m_textPopUp;
		private List<TextPopUp> m_textPopUps = new List<TextPopUp>();

		// 팝업 위치를 외부에서 직접 조정할 수 있도록 노출
		public RectTransform PopupRect => m_rect;

		private void Awake()
		{
			m_instance = this;
		}

		public void OpenTextPopUp(string header,
								  string txt,
								  RectTransform targetRect,
								  RectAnchor rectAnchor)
		{
			if (m_rect == null) return;
			SetTextAnchorBasedOnRectAnchor(rectAnchor);

			var textPopUp = GetPopUp();
			if (textPopUp == null) return;

			m_rect.transform.position =
				GeneralUtilities.RectPositionBesides(targetRect,
													 m_rect,
													 rectAnchor);
			
			SetPopUpContent(header, txt, textPopUp);
			ForceLayoutUpdate(textPopUp);
			PreventScreenCutting();

			// 팝업이 마우스 이벤트를 가로채지 않도록 raycastTarget 비활성화
			foreach (var graphic in m_rect.GetComponentsInChildren<UnityEngine.UI.Graphic>(true))
				graphic.raycastTarget = false;
		}

		private void PreventScreenCutting()
		{
			if (m_rect == null) return;

			var halfScreenHeight = Screen.height / 2f;
			var halfRectHeight = m_rect.rect.height / 2f;
			float edgePadding = 10f;

			var localPos = m_rect.localPosition;

			// Y축만 화면 잘림 방지 (X축은 아이콘 중심 고정 유지)
			if (localPos.y - halfRectHeight < -halfScreenHeight + edgePadding)
			{
				localPos.y = -halfScreenHeight + halfRectHeight + edgePadding;
			}
			else if (localPos.y + halfRectHeight > halfScreenHeight - edgePadding)
			{
				localPos.y = halfScreenHeight - halfRectHeight - edgePadding;
			}

			m_rect.localPosition = localPos;
		}

		private void SetPopUpContent(string header, string txt, TextPopUp textPopUp)
		{
			textPopUp.Header = header;
		#if UNITY_EDITOR
			// Carriage return filtering is handled safely below; silenced the noisy console error log.
		#endif
			string filteredText = txt.Replace("" + (char) 13, "");
			textPopUp.Text = filteredText;
			
			// Hide text object if empty for cleaner look
			var texts = textPopUp.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
			if (texts != null)
			{
				foreach(var t in texts)
				{
					if (t.name == "Text") // Hardcoded name check based on hierarchy inspection
					{
						t.gameObject.SetActive(!string.IsNullOrEmpty(filteredText));
					}
				}
			}
		}

		public void OpenTextPopUp(string header, string txt)
		{
			var textPopUp = GetPopUp();
			SetPopUpContent(header, txt, textPopUp);
			textPopUp.gameObject.SetActive(true);
		}

		public void CloseAll()
		{
			m_textPopUps.ForEach(x => x.gameObject.SetActive(false));
		}

		private TextPopUp GetPopUp()
		{
			if (m_textPopUp == null)
			{
				Debug.LogWarning("[PopUpHandler] m_textPopUp prefab is not assigned!");
				return null;
			}

			TextPopUp textPopUp = m_textPopUps.Find(
				 x => x.gameObject.activeInHierarchy == false
				);

			if (textPopUp)
			{
				textPopUp.gameObject.SetActive(true);
			}
			else
			{
				textPopUp = Instantiate(m_textPopUp, m_rect);
				m_textPopUps.Add(textPopUp);
			}

			return textPopUp;
		}

		private void ForceLayoutUpdate(TextPopUp textPopUp)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(textPopUp.RectTransform);
			LayoutRebuilder.ForceRebuildLayoutImmediate(m_rect);
		}

		private void SetTextAnchorBasedOnRectAnchor(RectAnchor rectAnchor)
		{
			if (m_layoutGroup == null) return;

			switch (rectAnchor)
			{
				case RectAnchor.Top:
					m_layoutGroup.childAlignment = TextAnchor.LowerCenter;
					break;
				case RectAnchor.Bottom:
					m_layoutGroup.childAlignment = TextAnchor.UpperCenter;
					break;
				case RectAnchor.Left:
				case RectAnchor.Right:
					m_layoutGroup.childAlignment = TextAnchor.MiddleCenter;
					break;
			}
		}
	}
}
#pragma warning restore 0649