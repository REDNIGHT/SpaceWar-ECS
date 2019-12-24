using UnityEngine;
using System.Collections.Generic;

public struct TimeEquation
{
    //
    Equations.Function _equation;
    //public TimeEquation() { _equation = Equations.linear; }
    public TimeEquation(Equations.Enum e) { _equation = Equations.getEquation(e); }
    //public TimeEquation(Equations.Function f) { _equation = f; }

    public TimeEquation linear { get { _equation = Equations.getEquation(Equations.Enum.linear); return this; } }

    public TimeEquation sin { get { _equation = Equations.getEquation(Equations.Enum.sin); return this; } }
    public TimeEquation sinOnePi { get { _equation = Equations.getEquation(Equations.Enum.sinOnePi); return this; } }
    public TimeEquation cos { get { _equation = Equations.getEquation(Equations.Enum.cos); return this; } }
    public TimeEquation cosOnePi { get { _equation = Equations.getEquation(Equations.Enum.cosOnePi); return this; } }
    public TimeEquation spring { get { _equation = Equations.getEquation(Equations.Enum.spring); return this; } }

    public TimeEquation inQuad { get { _equation = Equations.getEquation(Equations.Enum.inQuad); return this; } }
    public TimeEquation outQuad { get { _equation = Equations.getEquation(Equations.Enum.outQuad); return this; } }
    public TimeEquation inOutQuad { get { _equation = Equations.getEquation(Equations.Enum.inOutQuad); return this; } }
    public TimeEquation outInQuad { get { _equation = Equations.getEquation(Equations.Enum.outInQuad); return this; } }

    public TimeEquation inCubic { get { _equation = Equations.getEquation(Equations.Enum.inCubic); return this; } }
    public TimeEquation outCubic { get { _equation = Equations.getEquation(Equations.Enum.outCubic); return this; } }
    public TimeEquation inOutCubic { get { _equation = Equations.getEquation(Equations.Enum.inOutCubic); return this; } }
    public TimeEquation outInCubic { get { _equation = Equations.getEquation(Equations.Enum.outInCubic); return this; } }

    public TimeEquation inQuart { get { _equation = Equations.getEquation(Equations.Enum.inQuart); return this; } }
    public TimeEquation outQuart { get { _equation = Equations.getEquation(Equations.Enum.outQuart); return this; } }
    public TimeEquation inOutQuart { get { _equation = Equations.getEquation(Equations.Enum.inOutQuart); return this; } }
    public TimeEquation outInQuart { get { _equation = Equations.getEquation(Equations.Enum.outInQuart); return this; } }

    public TimeEquation inQuint { get { _equation = Equations.getEquation(Equations.Enum.inQuint); return this; } }
    public TimeEquation outQuint { get { _equation = Equations.getEquation(Equations.Enum.outQuint); return this; } }
    public TimeEquation inOutQuint { get { _equation = Equations.getEquation(Equations.Enum.inOutQuint); return this; } }
    public TimeEquation outInQuint { get { _equation = Equations.getEquation(Equations.Enum.outInQuint); return this; } }

    public TimeEquation inSine { get { _equation = Equations.getEquation(Equations.Enum.inSine); return this; } }
    public TimeEquation outSine { get { _equation = Equations.getEquation(Equations.Enum.outSine); return this; } }
    public TimeEquation inOutSine { get { _equation = Equations.getEquation(Equations.Enum.inOutSine); return this; } }
    public TimeEquation outInSine { get { _equation = Equations.getEquation(Equations.Enum.outInSine); return this; } }

    public TimeEquation inExpo { get { _equation = Equations.getEquation(Equations.Enum.inExpo); return this; } }
    public TimeEquation outExpo { get { _equation = Equations.getEquation(Equations.Enum.outExpo); return this; } }
    public TimeEquation inOutExpo { get { _equation = Equations.getEquation(Equations.Enum.inOutExpo); return this; } }
    public TimeEquation outInExpo { get { _equation = Equations.getEquation(Equations.Enum.outInExpo); return this; } }

    public TimeEquation inCirc { get { _equation = Equations.getEquation(Equations.Enum.inCirc); return this; } }
    public TimeEquation outCirc { get { _equation = Equations.getEquation(Equations.Enum.outCirc); return this; } }
    public TimeEquation inOutCirc { get { _equation = Equations.getEquation(Equations.Enum.inOutCirc); return this; } }
    public TimeEquation outInCirc { get { _equation = Equations.getEquation(Equations.Enum.outInCirc); return this; } }

    public TimeEquation inBounce { get { _equation = Equations.getEquation(Equations.Enum.inBounce); return this; } }
    public TimeEquation outBounce { get { _equation = Equations.getEquation(Equations.Enum.outBounce); return this; } }
    public TimeEquation inOutBounce { get { _equation = Equations.getEquation(Equations.Enum.inOutBounce); return this; } }
    public TimeEquation outInBounce { get { _equation = Equations.getEquation(Equations.Enum.outInBounce); return this; } }

    public TimeEquation inElastic { get { _equation = Equations.getEquation(Equations.Enum.inElastic); return this; } }
    public TimeEquation outElastic { get { _equation = Equations.getEquation(Equations.Enum.outElastic); return this; } }
    public TimeEquation inOutElastic { get { _equation = Equations.getEquation(Equations.Enum.inOutElastic); return this; } }
    public TimeEquation outInElastic { get { _equation = Equations.getEquation(Equations.Enum.outInElastic); return this; } }

    public TimeEquation inBack { get { _equation = Equations.getEquation(Equations.Enum.inBack); return this; } }
    public TimeEquation outBack { get { _equation = Equations.getEquation(Equations.Enum.outBack); return this; } }
    public TimeEquation inOutBack { get { _equation = Equations.getEquation(Equations.Enum.inOutBack); return this; } }
    public TimeEquation outInBack { get { _equation = Equations.getEquation(Equations.Enum.outInBack); return this; } }

    public TimeEquation inBack2 { get { _equation = Equations.getEquation(Equations.Enum.inBack2); return this; } }
    public TimeEquation outBack2 { get { _equation = Equations.getEquation(Equations.Enum.outBack2); return this; } }
    public TimeEquation inOutBack2 { get { _equation = Equations.getEquation(Equations.Enum.inOutBack2); return this; } }
    public TimeEquation outInBack2 { get { _equation = Equations.getEquation(Equations.Enum.outInBack2); return this; } }


    //
    public IEnumerable<float> play(float time)
    {
        var t = 0f;
        while (t < time)
        {
            yield return _equation(t / time);

            t += Time.deltaTime;
        }

        yield return 1f;
    }
    public IEnumerable<float> playRealtime(float time)
    {
        var t = 0f;
        while (t < time)
        {
            yield return _equation(t / time);

            t += Time.unscaledDeltaTime;
        }

        yield return 1f;
    }


    //
    public IEnumerable<float> playInverse(float time)
    {
        var t = time;
        while (t > 0f)
        {
            yield return _equation(t / time);

            t -= Time.deltaTime;
        }

        yield return 0f;
    }
    public IEnumerable<float> playInverseRealtime(float time)
    {
        var t = time;
        while (t > 0f)
        {
            yield return _equation(t / time);

            t -= Time.unscaledDeltaTime;
        }

        yield return 0f;
    }

    //
    public IEnumerable<float> play(float time, bool inverse)
    {
        if (inverse)
            return playInverse(time);
        return play(time);
    }
    public IEnumerable<float> playRealtime(float time, bool inverse)
    {
        if (inverse)
            return playInverseRealtime(time);
        return playRealtime(time);
    }


    //
    public IEnumerable<float> playRepeat(float time)
    {
        var t = 0f;
        while (true)
        {
            yield return _equation(Mathf.Repeat(t, time) / time);

            t += Time.deltaTime;
        }
    }
    public IEnumerable<float> playPingPong(float time)
    {
        var t = 0f;
        while (true)
        {
            yield return _equation(Mathf.PingPong(t, time) / time);

            t += Time.deltaTime;
        }
    }
    public IEnumerable<float> playRepeatRealtime(float time)
    {
        var t = 0f;
        while (true)
        {
            yield return _equation(Mathf.Repeat(t, time) / time);

            t += Time.unscaledDeltaTime;
        }
    }
    public IEnumerable<float> playPingPongRealtime(float time)
    {
        var t = 0f;
        while (true)
        {
            yield return _equation(Mathf.PingPong(t, time) / time);

            t += Time.unscaledDeltaTime;
        }
    }

    //-------------------------------------------------------------------------
    public IEnumerable<float> playDelta(float time)
    {
        var t = 0f;
        var last = 0f;
        while (t < time)
        {
            var v = _equation(t / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.deltaTime;
        }

        yield return _equation(1f) - last;
    }
    public IEnumerable<float> playDeltaRealtime(float time)
    {
        var t = 0f;
        var last = 0f;
        while (t < time)
        {
            var v = _equation(t / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.unscaledDeltaTime;
        }

        yield return _equation(1f) - last;
    }


    //
    public IEnumerable<float> playInverseDelta(float time)
    {
        var t = time;
        var last = 0f;
        while (t > 0f)
        {
            var v = _equation(t / time);
            var d = v - last;
            last = v;
            yield return d;

            t -= Time.deltaTime;
        }

        yield return _equation(0f) - last;
    }
    public IEnumerable<float> playInverseDeltaRealtime(float time)
    {
        var t = time;
        var last = 0f;
        while (t > 0f)
        {
            var v = _equation(t / time);
            var d = v - last;
            last = v;
            yield return d;

            t -= Time.unscaledDeltaTime;
        }

        yield return _equation(0f) - last;
    }

    //
    public IEnumerable<float> playDelta(float time, bool inverse)
    {
        if (inverse)
            return playInverseDelta(time);
        return playDelta(time);
    }
    public IEnumerable<float> playDeltaRealtime(float time, bool inverse)
    {
        if (inverse)
            return playInverseDeltaRealtime(time);
        return playDeltaRealtime(time);
    }


    //
    public IEnumerable<float> playRepeatDelta(float time)
    {
        var t = 0f;
        var last = 0f;
        while (true)
        {
            var v = _equation(Mathf.Repeat(t, time) / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.deltaTime;
        }
    }
    public IEnumerable<float> playPingPongDelta(float time)
    {
        var t = 0f;
        var last = 0f;
        while (true)
        {
            var v = _equation(Mathf.PingPong(t, time) / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.deltaTime;
        }
    }
    public IEnumerable<float> playRepeatDeltaRealtime(float time)
    {
        var t = 0f;
        var last = 0f;
        while (true)
        {
            var v = _equation(Mathf.Repeat(t, time) / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.unscaledDeltaTime;
        }
    }
    public IEnumerable<float> playPingPongDeltaRealtime(float time)
    {
        var t = 0f;
        var last = 0f;
        while (true)
        {
            var v = _equation(Mathf.PingPong(t, time) / time);
            var d = v - last;
            last = v;
            yield return d;

            t += Time.unscaledDeltaTime;
        }
    }
}