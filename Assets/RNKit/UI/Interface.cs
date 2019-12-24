using System.Collections;
using UnityEngine;

namespace RN.UI
{
    public interface IOnLoadUI
    {
        IEnumerator onLoadUI();
    }


    public interface IUIElement
    {
        IEnumerator onEnableUI();

        void onDisableUI();
    }


    //用IUIElement代替
    public interface IVisible
    {
        bool visible { get; set; }
    }


    public interface IUIPanelFader
    {
        IEnumerator In(UIPanel panel);
        IEnumerator Out(UIPanel panel);

        //
        /*float fadeInTime { get; }

        //
        Coroutine Out(UIPanel panel);
        Coroutine In(UIPanel panel);

        Coroutine Out(int channel);

        IEnumerator InForce(UIPanel panel);

        //
        void inAudio(UIPanel panel);

        //
        UIPanel.FadeState getFadingState(int channel);
        void setFadingState(int channel, UIPanel.FadeState fadeState);*/
    }
}