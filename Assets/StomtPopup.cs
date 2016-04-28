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
        public GameObject _typeObj;
        [SerializeField]
        [HideInInspector]
        public GameObject _targetNameObj;
        [SerializeField]
        [HideInInspector]
        public GameObject _messageObj;
        [SerializeField]
        [HideInInspector]
        public GameObject _errorMessage;
        
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
        [SerializeField]
        [HideInInspector]
        public GameObject placeholderText;
        [SerializeField]
        [HideInInspector]
        public GameObject messageText;
        [SerializeField]
        [HideInInspector]
        public Image TargetIcon;

        private WWW ImageDownload;
        private Texture2D ProfileImageTexture;
        private bool TargetImageApplied;
        private bool StartedTyping;

        
        public bool WouldBecauseText = true; // activates the would/because text
        public bool AutoImageDownload = true; // will automatically download the targetImage after %DelayTime Seconds;
        public int AutoImageDownloadDelay = 5; // %DelayTime
        public int CharLimit = 120;

		void Awake()
		{

            TargetImageApplied = false;

            if(placeholderText == null)
            {
                Debug.Log("PlaceholderText not found: Find(\"/Message/PlaceholderText\")");
            }

            

			_api = GetComponent<StomtAPI>();
			_screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			Reset();
            StartCoroutine(this.refreshTargetIcon(AutoImageDownloadDelay));
		}
		void Start()
		{
            StartedTyping = false;
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

            if( (_ui.activeSelf && _api.NetworkError) && !_errorMessage.activeSelf)
            {
                ShowError();
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

            ShowError();	
		}

        void ShowError()
        {
            if (_api.NetworkError)
            {
                // Diable GUI
                _messageObj.SetActive(false);
                _typeObj.SetActive(false);
                _targetNameObj.SetActive(false);
                // Enable Error MSG
                _errorMessage.SetActive(true);

            }
            else
            {
                // Diable GUI
                _messageObj.SetActive(true);
                _typeObj.SetActive(true);
                _targetNameObj.SetActive(true);
                // Enable Error MSG
                _errorMessage.SetActive(false);
            }
        }

		void Hide()
		{
			// Hide UI
			_ui.SetActive(false);
		}
		void Reset()
		{
			_targetText.text = _api.TargetName;


            if( !TargetImageApplied )
            {
                refreshTargetIcon();
            }
            

            if(StartedTyping)
            {
                //this.refreshStartText();
            }
            
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
            int limit = CharLimit;
			int reverselength = limit - _message.text.Length;

			if (reverselength <= 0)
			{
				reverselength = 0;
				_message.text = _message.text.Substring(0, limit);
			}

			_characterLimit.text = reverselength.ToString();


            /** Change Text **/
            if ( (!placeholderText.GetComponent<Text>().IsActive()) && _ui.activeSelf )
            {
                this.RefreshStartText();
            }
		}

        public void RefreshStartText()
        {
            if (this.StartedTyping && WouldBecauseText)
            {
                if (_like.sortingOrder == 1)
                {

                    // I wish
                    if (_message.text.Equals("") || _message.text.Equals("because "))
                    {
                        _message.text = "would ";
                    }
                }
                else
                {
                    // I like
                    if (_message.text.Equals("") || _message.text.Equals("would "))
                    {
                        _message.text = "because ";
                    }
                }
            }
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

        private void refreshTargetIcon()
        {
            StartCoroutine(refreshTargetIcon(0));
        }

        private IEnumerator refreshTargetIcon(float DelayTime)
        {

            // check wether download needed
            if (ImageDownload == null)
            {
                ImageDownload = _api.LoadTargetImage();
            }

            yield return new WaitForSeconds(DelayTime);

            if(DelayTime > 0)
            {
                this.refreshTargetIcon();
            }

            // check wether download finished
            if (ImageDownload != null && !TargetImageApplied)
            {

                if (ProfileImageTexture != null) // already loaded, apply now
                {
                    TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToJPG(), false);
                }
                else if (ImageDownload.texture != null) // scale now and apply
                {
                    ProfileImageTexture = TextureScaler.scaled(ImageDownload.texture, 1024, 1024, FilterMode.Trilinear);

                    TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToPNG(), false);
                    this.TargetImageApplied = true;
                }
            }
        }

        public void OnPointerEnter()
        {
            this.StartedTyping = true;

            this.RefreshStartText();
        }
	}
}
