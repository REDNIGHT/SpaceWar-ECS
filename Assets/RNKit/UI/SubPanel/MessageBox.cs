using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RN.UI
{
    /// <summary>
    ///     var box = MessageBox.singleton;
    ///     box.show("are you sure to delete?");
    ///    
    ///     while (box.dialoging)
    ///         yield return null;
    ///    
    ///     if (box.yes)
    ///     {
    ///         //todo...
    ///     }
    /// </summary>
    public class MessageBox : SubPanel<MessageBox>
    {
        protected void onPanelVisible(bool v)
        {
            if (v == false)
            {
                transform.Find("view/buttons/_yes/text").autoSetText("Yes");
                transform.Find("view/buttons/_no/text").autoSetText("No");
            }
            else
            {
                //wtf! 自动匹配的大小有问题
                /*刷新界面自动匹配的大小
                var tt = transform.Find("view/text").gameObject;
                tt.SetActive(false);
                yield return this;
                tt.SetActive(true);*/
            }
        }


        //
        public bool yes { get; protected set; }
        public bool no { get { return !yes; } }

        public bool dialoging
        {
            get { return visible; }
        }

        public MessageBox set(string msg, string yes, string no)
        {
            return Message(msg).Yes(yes).No(no);
        }

        public MessageBox Message(string msg)
        {
            transform.Find("view/text").autoSetText(msg);
            return this;
        }
        public MessageBox Yes(string yes)
        {
            if (string.IsNullOrEmpty(yes))
                transform.Find("view/buttons/_yes").gameObject.SetActive(false);
            else
                transform.Find("view/buttons/_yes/text").autoSetText(yes);

            return this;
        }
        public MessageBox No(string no)
        {
            if (string.IsNullOrEmpty(no))
                transform.Find("view/buttons/_no").gameObject.SetActive(false);
            else
                transform.Find("view/buttons/_no/text").autoSetText(no);

            return this;
        }
        public MessageBox autoBack(bool v)
        {
            GetComponent<BPanel>().autoBack = v;
            return this;
        }

        //
        protected void on_no()
        {
            yes = false;
        }

        protected void on_yes()
        {
            yes = true;
        }

        //
        public void Show(System.Action<MessageBox> onStart, System.Func<MessageBox, IEnumerator> onFinish)
        {
            StartCoroutine(showE(onStart, onFinish));
        }
        IEnumerator showE(System.Action<MessageBox> onStart, System.Func<MessageBox, IEnumerator> onFinish)
        {
            //等待当前对话结束
            var d = dialoging;
            while (dialoging)
                yield return null;
            //等待当前对话的onFinish结束
            if (d)
                yield return null;

            onStart(this);

            yield return thisPanel.In();
            yes = false;

            while (dialoging)
                yield return null;

            var e = onFinish(this);
            if (e != null)
                StartCoroutine(e);
        }

        //
#if UNITY_EDITOR
        string[] strs = new string[]
             { "test...\n............"
                , "0123456789zxcasdqwe"
                , "0123456789zxcasdqwe\n0123456789zxcasdqwe\n0123456789zxcasdqwe"
                , "0123456789zxcasdqwe\n0123456789zxcasdqwe\n0123456789zxcasdqwe\n0123456789zxcasdqwe"
                , "1230" };
        int strsIndex = 0;
        [RN._Editor.ButtonInEndArea]
        void test()
        {
            MessageBox
                .singleton
                .Show((m) => { m.Message(strs[strsIndex]); }, (m) => null);
        }
#endif
    }
}