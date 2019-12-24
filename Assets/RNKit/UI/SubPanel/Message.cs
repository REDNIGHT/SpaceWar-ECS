using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace RN.UI
{
    public class Message : SubPanel<Message>
    {
        //showTime <= 0 时是永远显示不会隐藏
        public void show(string msg, float showTime = 1.5f)
        {
            StartCoroutine(showE(msg, showTime));
        }

        public IEnumerator showE(string msg, float showTime = 1.5f)
        {
            //Debug.LogWarning("message:" + this.autoGetTextString(), this);

            if (visible)
                Debug.Log($"message: {transform.Find("view/text").autoGetTextString()}");

            while (visible)
                yield return this;


            //
            this.autoSetText(msg);


            //
            yield return In();

            //
            if (showTime > 0)
            {
                yield return new WaitForSeconds(showTime);

                yield return Out();
            }
        }


        public void showMsgs(params string[] msgs)
        {
            var msg = "";
            foreach (var m in msgs)
                msg += m + "\n";

            show(msg);
        }

        public void showMsgs(float showTime, params string[] msgs)
        {
            var msg = "";
            foreach (var m in msgs)
                msg += m + "\n";

            show(msg, showTime);
        }

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
            //show("test Show Message!!!\n\ntest Show Message2!!!");
            show(strs[strsIndex]);
            strsIndex++;
            if (strsIndex >= strs.Length)
                strsIndex = 0;
        }
#endif
    }
}