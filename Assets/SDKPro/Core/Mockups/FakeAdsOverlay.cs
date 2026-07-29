using System;
using UnityEngine;

namespace SDKPro.Core.Mockups
{
    public sealed class FakeAdsOverlay : MonoBehaviour
    {
        private enum FullscreenFormat
        {
            None,
            Interstitial,
            Rewarded,
            AppOpen
        }

        private FullscreenFormat _fullscreenFormat;
        private bool _bannerVisible;
        private bool _bannerCollapsible;
        private bool _mrecVisible;
        private Vector2 _mrecPosition = new Vector2(-1f, -1f);

        private Action _onFullscreenClick;
        private Action _onFullscreenClose;
        private Action _onReward;
        private Action _onBannerClick;
        private Action _onMrecClick;

        public void ShowInterstitial(Action onClick, Action onClose)
        {
            ShowFullscreen(FullscreenFormat.Interstitial, onClick, onClose, null);
        }

        public void ShowRewarded(Action onClick, Action onClose, Action onReward)
        {
            ShowFullscreen(FullscreenFormat.Rewarded, onClick, onClose, onReward);
        }

        public void ShowAppOpen(Action onClick, Action onClose)
        {
            ShowFullscreen(FullscreenFormat.AppOpen, onClick, onClose, null);
        }

        public void ShowBanner(bool collapsible, Action onClick)
        {
            _bannerCollapsible = collapsible;
            _bannerVisible = true;
            _onBannerClick = onClick;
        }

        public void HideBanner()
        {
            _bannerVisible = false;
        }

        public void ShowMrec(Action onClick)
        {
            _mrecVisible = true;
            _onMrecClick = onClick;
        }

        public void HideMrec()
        {
            _mrecVisible = false;
        }

        public void SetMrecPosition(Vector2 position)
        {
            _mrecPosition = position;
        }

        public void Clear()
        {
            _fullscreenFormat = FullscreenFormat.None;
            _bannerVisible = false;
            _mrecVisible = false;
            _onFullscreenClick = null;
            _onFullscreenClose = null;
            _onReward = null;
            _onBannerClick = null;
            _onMrecClick = null;
        }

        private void ShowFullscreen(
            FullscreenFormat format,
            Action onClick,
            Action onClose,
            Action onReward)
        {
            _fullscreenFormat = format;
            _onFullscreenClick = onClick;
            _onFullscreenClose = onClose;
            _onReward = onReward;
        }

        private void OnGUI()
        {
            if (_fullscreenFormat != FullscreenFormat.None)
            {
                DrawFullscreen();
                return;
            }

            if (_bannerVisible)
            {
                DrawBanner();
            }

            if (_mrecVisible)
            {
                DrawMrec();
            }
        }

        private void DrawFullscreen()
        {
            Rect area = new Rect(0f, 0f, Screen.width, Screen.height);
            Color previousColor = GUI.color;
            GUI.color = new Color(0.09f, 0.12f, 0.18f, 0.98f);
            GUI.Box(area, GUIContent.none);
            GUI.color = Color.white;

            GUILayout.BeginArea(new Rect(
                Screen.width * 0.15f,
                Screen.height * 0.18f,
                Screen.width * 0.7f,
                Screen.height * 0.64f));
            GUILayout.FlexibleSpace();
            GUILayout.Label(
                $"FAKE {_fullscreenFormat.ToString().ToUpperInvariant()} AD",
                CenteredLabel(26));
            GUILayout.Space(18f);
            GUILayout.Label(
                "No mediation SDK is installed. Use these controls to test gameplay callbacks.",
                CenteredLabel(16));
            GUILayout.Space(24f);

            if (GUILayout.Button("Simulate click", GUILayout.Height(48f)))
            {
                _onFullscreenClick?.Invoke();
            }

            if (_fullscreenFormat == FullscreenFormat.Rewarded &&
                GUILayout.Button("Earn reward and close", GUILayout.Height(48f)))
            {
                Action reward = _onReward;
                _onReward = null;
                reward?.Invoke();
                CloseFullscreen();
            }

            if (GUILayout.Button("Close ad", GUILayout.Height(48f)))
            {
                CloseFullscreen();
            }

            GUILayout.FlexibleSpace();
            GUILayout.EndArea();
            GUI.color = previousColor;
        }

        private void DrawBanner()
        {
            float height = _bannerCollapsible ? 120f : 72f;
            Rect rect = new Rect(0f, Screen.height - height, Screen.width, height);
            string label = _bannerCollapsible
                ? "FAKE COLLAPSIBLE BANNER - click to simulate callback"
                : "FAKE BANNER - click to simulate callback";

            Color previousColor = GUI.color;
            GUI.color = _bannerCollapsible
                ? new Color(0.25f, 0.68f, 0.95f, 0.97f)
                : new Color(0.2f, 0.78f, 0.48f, 0.97f);
            if (GUI.Button(rect, label))
            {
                _onBannerClick?.Invoke();
            }

            GUI.color = previousColor;
        }

        private void DrawMrec()
        {
            const float width = 300f;
            const float height = 250f;
            float x = _mrecPosition.x >= 0f
                ? _mrecPosition.x
                : (Screen.width - width) * 0.5f;
            float y = _mrecPosition.y >= 0f
                ? _mrecPosition.y
                : (Screen.height - height) * 0.5f;
            Rect rect = new Rect(x, y, width, height);

            Color previousColor = GUI.color;
            GUI.color = new Color(0.85f, 0.45f, 0.18f, 0.97f);
            if (GUI.Button(rect, "FAKE MREC\nclick to simulate callback"))
            {
                _onMrecClick?.Invoke();
            }

            GUI.color = previousColor;
        }

        private void CloseFullscreen()
        {
            Action close = _onFullscreenClose;
            _fullscreenFormat = FullscreenFormat.None;
            _onFullscreenClick = null;
            _onFullscreenClose = null;
            _onReward = null;
            close?.Invoke();
        }

        private static GUIStyle CenteredLabel(int fontSize)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = fontSize,
                wordWrap = true
            };
        }
    }
}
