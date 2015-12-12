﻿using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Stomt
{
	[RequireComponent(typeof(StomtAPI))]
	public class StomtPopup : MonoBehaviour
	{
		#region Inspector Variables
		[SerializeField]
		KeyCode _toggleKey = KeyCode.F1;

		[SerializeField]
		[HideInInspector]
		GameObject _ui;
		[SerializeField]
		[HideInInspector]
		Canvas _like;
		[SerializeField]
		[HideInInspector]
		Canvas _wish;
		[SerializeField]
		[HideInInspector]
		InputField _message;
		[SerializeField]
		[HideInInspector]
		Text _wouldBecauseText;
		[SerializeField]
		[HideInInspector]
		Text _characterLimit;
		[SerializeField]
		[HideInInspector]
		Text _targetText;
		[SerializeField]
		[HideInInspector]
		Toggle _screenshotToggle;
		#endregion
		StomtAPI _api;
		Texture2D _screenshot;

		void Awake()
		{
			_api = GetComponent<StomtAPI>();
			_screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			Reset();
		}
		void Start()
		{
			Hide();
		}
		void Update()
		{
			if (Input.GetKeyDown(_toggleKey))
			{
				if (_ui.activeSelf)
				{
					Hide();
				}
				else
				{
					StartCoroutine(Show());
				}
			}
		}

		IEnumerator Show()
		{
			yield return new WaitForEndOfFrame();

			// Capture screenshot
			_screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

			// Show UI
			Reset();
			_ui.SetActive(true);
		}
		void Hide()
		{
			// Hide UI
			_ui.SetActive(false);
		}
		void Reset()
		{
			_targetText.text = _api.TargetName;
			_message.text = string.Empty;
			_screenshotToggle.isOn = true;

			if (_like.sortingOrder == 2)
			{
				OnToggleButtonPressed();
			}
			else
			{
				OnMessageChanged();
			}
		}

		public void OnToggleButtonPressed()
		{
			var likeTransform = _like.GetComponent<RectTransform>();
			var wishTransform = _wish.GetComponent<RectTransform>();

			var temp = likeTransform.anchoredPosition;
			likeTransform.anchoredPosition = wishTransform.anchoredPosition;
			wishTransform.anchoredPosition = temp;

			if (_like.sortingOrder == 2)
			{
				// I wish
				_like.sortingOrder = 1;
				_wish.sortingOrder = 2;
				_wouldBecauseText.text = "would";
			}
			else
			{
				// I like
				_like.sortingOrder = 2;
				_wish.sortingOrder = 1;
				_wouldBecauseText.text = "because";
			}

			OnMessageChanged();
		}
		public void OnMessageChanged()
		{
			int limit = 101 - _wouldBecauseText.text.Length;
			int reverselength = limit - _message.text.Length;

			if (reverselength <= 0)
			{
				reverselength = 0;
				_message.text = _message.text.Substring(0, limit);
			}

			_characterLimit.text = reverselength.ToString();
		}
		public void OnPostButtonPressed()
		{
			if (_message.text.Length == 0)
			{
				return;
			}

			if (_screenshotToggle.isOn)
			{
				_api.CreateStomtWithImage(_like.sortingOrder == 2, _wouldBecauseText.text + " " + _message.text, _screenshot);
			}
			else
			{
				_api.CreateStomt(_like.sortingOrder == 2, _wouldBecauseText.text + " " + _message.text);
			}

			Hide();
		}
	}
}
