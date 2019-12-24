using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RN.UI
{
    public class LoadingPanel : SubPanel<LoadingPanel>
    {
        public Text _text;
        public Image _progress;

        public Text _infoText;

        //
        public Transform _quit;
        public bool quit = false;

        public Transform _next;
        public Transform _back;


        //
        protected void onViewLoadComplete()
        {
            //
            if (_text == null)
                _text = transform.find<Text>("view/text");

            if (_progress == null)
                _progress = transform.find<Image>("view/progress");


            //
            if (_infoText == null)
                _infoText = transform.find<Text>("view/info");

            if (_quit == null)
                _quit = transform.Find("view/_quit");

            if (_next == null)
                _next = transform.Find("view/_next");

            if (_back == null)
                _back = transform.Find("view/_back");
        }

        protected void onPanelVisible(bool v)
        {
            //
            if (v)
            {
                if (_next != null)
                    _next.gameObject.SetActive(false);
                if (_back != null)
                    _back.gameObject.SetActive(false);

                //
                progress = 0;
            }


            //
            if (v)
            {
                quit = false;
            }
            else
            {
                if (_quit != null)
                    _quit.gameObject.SetActive(false);
            }
        }


        //
        //在加载前调用
        public void showQuitButton()
        {
            _quit.gameObject.SetActive(true);
        }
        protected void on_quit()
        {
            quit = true;
        }


        //在加载完成后调用
        public void showNextButton()
        {
            _next.gameObject.SetActive(true);
        }
        //在加载完成后调用
        public void showBackButton()
        {
            _back.gameObject.SetActive(true);
        }


        public string info
        {
            set
            {
                if (_infoText != null)
                    _infoText.text = value;
            }
        }


        //
        //结束后记得给progress赋值1f
        public string progressTextFormat = "Loading...  {0:P}";
        public float progress
        {
            set
            {
                _text.text = string.Format(progressTextFormat, value);
                _progress.fillAmount = value;
            }
        }


        public void setProgress(AsyncOperation async)
        {
            StartCoroutine(setProgressE(async));
        }
        IEnumerator setProgressE(AsyncOperation async)
        {
            while (async.isDone == false)
            {
                progress = async.progress;
                yield return this;
            }
        }
    }
}