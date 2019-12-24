// Copyright (c) 2012 Xilin Chen (RN)
// Please direct any bugs/comments/suggestions to http://rntween.blogspot.com/

using System;
using UnityEngine;

/// <summary>
/// Equation functions.
/// </summary>
public class Equations
{
    //
    /// <summary>
    /// define delegate type.
    /// </summary>
    /// <param name="percentage">The percentage.</param>
    /// <param name="start">The start.</param>
    /// <param name="offset">The offset.</param>
    /// <returns></returns>
    public delegate float Function(float percentage);

    //
    /// <summary>
    /// Equation enums.
    /// </summary>
    public enum Enum
    {
        linear,

        sin,
        sinOnePi,
        cos,
        cosOnePi,

        spring,

        inQuad,
        outQuad,
        inOutQuad,
        outInQuad,

        inCubic,
        outCubic,
        inOutCubic,
        outInCubic,

        inQuart,
        outQuart,
        inOutQuart,
        outInQuart,

        inQuint,
        outQuint,
        inOutQuint,
        outInQuint,

        inSine,
        outSine,
        inOutSine,
        outInSine,

        inExpo,
        outExpo,
        inOutExpo,
        outInExpo,

        inCirc,
        outCirc,
        inOutCirc,
        outInCirc,

        inBounce,
        outBounce,
        inOutBounce,
        outInBounce,

        inElastic,
        outElastic,
        inOutElastic,
        outInElastic,

        inBack,
        outBack,
        inOutBack,
        outInBack,

        inBack2,
        outBack2,
        inOutBack2,
        outInBack2,

        curve,

        //
        MaxCount,
    }
    
    //
    /// <summary>
    /// return the equation function by enum.
    /// </summary>
    public static Function getEquation(Enum e)
    {
        switch (e)
        {
            case Enum.linear: return linear;
                
            case Enum.sin: return sin;
            case Enum.sinOnePi: return sinOnePi;
            case Enum.cos: return cos;
            case Enum.cosOnePi: return cosOnePi;
            case Enum.spring: return spring;

            case Enum.inQuad: return inQuad;
            case Enum.outQuad: return outQuad;
            case Enum.inOutQuad: return inOutQuad;
            case Enum.outInQuad: return outInQuad;

            case Enum.inCubic: return inCubic;
            case Enum.outCubic: return outCubic;
            case Enum.inOutCubic: return inOutCubic;
            case Enum.outInCubic: return outInCubic;

            case Enum.inQuart: return inQuart;
            case Enum.outQuart: return outQuart;
            case Enum.inOutQuart: return inOutQuart;
            case Enum.outInQuart: return outInQuart;

            case Enum.inQuint: return inQuint;
            case Enum.outQuint: return outQuint;
            case Enum.inOutQuint: return inOutQuint;
            case Enum.outInQuint: return outInQuint;

            case Enum.inSine: return inSine;
            case Enum.outSine: return outSine;
            case Enum.inOutSine: return inOutSine;
            case Enum.outInSine: return outInSine;

            case Enum.inExpo: return inExpo;
            case Enum.outExpo: return outExpo;
            case Enum.inOutExpo: return inOutExpo;
            case Enum.outInExpo: return outInExpo;

            case Enum.inCirc: return inCirc;
            case Enum.outCirc: return outCirc;
            case Enum.inOutCirc: return inOutCirc;
            case Enum.outInCirc: return outInCirc;
                
            case Enum.inBounce: return inBounce;
            case Enum.outBounce: return outBounce;
            case Enum.inOutBounce: return inOutBounce;
            case Enum.outInBounce: return outInBounce;

            case Enum.inElastic: return inElastic;
            case Enum.outElastic: return outElastic;
            case Enum.inOutElastic: return inOutElastic;
            case Enum.outInElastic: return outInElastic;
                
            case Enum.inBack: return inBack;
            case Enum.outBack: return outBack;
            case Enum.inOutBack: return inOutBack;
            case Enum.outInBack: return outInBack;

            case Enum.inBack2: return inBack2;
            case Enum.outBack2: return outBack2;
            case Enum.inOutBack2: return inOutBack2;
            case Enum.outInBack2: return outInBack2;
        }
        Debug.LogError("unknow e.  e=" + e);
        return null;
    }


    //
    const float b = 0f;
    const float c = 1f;

    //
    public static float linear(float p)
    {
        return c * p + b;
    }
    
    public static float sin(float p)
    {
        return Mathf.Sin(p * Mathf.PI * 2.0f) * c + b;
    }

    public static float cos(float p)
    {
        return Mathf.Cos(p * Mathf.PI * 2.0f) * c + b;
    }

    public static float sinOnePi(float p)
    {
        return Mathf.Sin(p * Mathf.PI) * c + b;
    }

    public static float cosOnePi(float p)
    {
        return Mathf.Cos(p * Mathf.PI) * c + b;
    }

    public static float spring(float p)
    {
        var v = Mathf.Clamp01(p);
        v = (Mathf.Sin(v * Mathf.PI * (0.2f + 2.5f * v * v * v)) * Mathf.Pow(1f - v, 2.2f) + v) * (1f + (1.2f * (1f - v)));
        return b + c * v;
    }

    public static float inQuad(float p)
    {
        return inQuad(p, b, c);
    }
    public static float inQuad(float p, float b, float c)
    {
        return c * p * p + b;
    }

    public static float outQuad(float p)
    {
        return outQuad(p, b, c);
    }
    public static float outQuad(float p, float b, float c)
    {
        return -c * p * (p - 2f) + b;
    }

    public static float inOutQuad(float p)
    {
        p *= 2f;
        if (p < 1f)
            return c / 2f * p * p + b;
        p -= 1f;
        return -c / 2f * (p * (p - 2f) - 1f) + b;
    }

    public static float outInQuad(float p)
    {
        if (p < 0.5f)
            return outQuad(p * 2f, b, c / 2f);
        return inQuad((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inCubic(float p)
    {
        return inCubic(p, b, c);
    }
    public static float inCubic(float p, float b, float c)
    {
        return c * p * p * p + b;
    }

    public static float outCubic(float p)
    {
        return outCubic(p, b, c);
    }
    public static float outCubic(float p, float b, float c)
    {
        p -= 1f;
        return c * (p * p * p + 1f) + b;
    }

    public static float inOutCubic(float p)
    {
        p *= 2f;
        if (p < 1f)
            return c / 2f * p * p * p + b;
        p -= 2f;
        return c / 2f * (p * p * p + 2f) + b;
    }

    public static float outInCubic(float p)
    {
        if (p < 0.5f)
            return outCubic(p * 2f, b, c / 2f);
        return inCubic((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inQuart(float p)
    {
        return inQuart(p, b, c);
    }
    public static float inQuart(float p, float b, float c)
    {
        return c * p * p * p * p + b;
    }

    public static float outQuart(float p)
    {
        return outQuart(p, b, c);
    }
    public static float outQuart(float p, float b, float c)
    {
        p -= 1f;
        return -c * (p * p * p * p - 1f) + b;
    }

    public static float inOutQuart(float p)
    {
        p *= 2f;
        if (p < 1f)
            return c / 2f * p * p * p * p + b;
        p -= 2f;
        return -c / 2f * (p * p * p * p - 2f) + b;
    }

    public static float outInQuart(float p)
    {
        if (p < 0.5f)
            return outQuart(p * 2f, b, c / 2f);
        return inQuart((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inQuint(float p)
    {
        return inQuint(p, b, c);
    }
    public static float inQuint(float p, float b, float c)
    {
        return c * p * p * p * p * p + b;
    }

    public static float outQuint(float p)
    {
        return outQuint(p, b, c);
    }
    public static float outQuint(float p, float b, float c)
    {
        p -= 1f;
        return c * (p * p * p * p * p + 1f) + b;
    }

    public static float inOutQuint(float p)
    {
        p *= 2f;
        if (p < 1f)
            return c / 2f * p * p * p * p * p + b;
        p -= 2f;
        return c / 2f * (p * p * p * p * p + 2f) + b;
    }

    public static float outInQuint(float p)
    {
        if (p < 0.5f)
            return outQuint(p * 2f, b, c / 2f);
        return inQuint((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inSine(float p)
    {
        return inSine(p, b, c);
    }
    public static float inSine(float p, float b, float c)
    {
        return -c * Mathf.Cos(p * (Mathf.PI / 2)) + c + b;
    }

    public static float outSine(float p)
    {
        return outSine(p, b, c);
    }
    public static float outSine(float p, float b, float c)
    {
        return c * Mathf.Sin(p * (Mathf.PI / 2f)) + b;
    }

    public static float inOutSine(float p)
    {
        return -c / 2f * (Mathf.Cos(Mathf.PI * p) - 1f) + b;
    }

    public static float outInSine(float p)
    {
        if (p < 0.5f)
            return outSine(p * 2f, b, c / 2f);
        return inSine((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inExpo(float p)
    {
        return inExpo(p, b, c);
    }
    public static float inExpo(float p, float b, float c)
    {
        if (p == 0f)
            return b;
        else
            return c * Mathf.Pow(2f, 10 * (p - 1f)) + b - c * 0.001f;
    }

    public static float outExpo(float p)
    {
        return outExpo(p, b, c);
    }
    public static float outExpo(float p, float b, float c)
    {
        if (p == 1f)
            return b + c;
        else
            return c * 1.001f * (-Mathf.Pow(2f, -10 * p) + 1) + b;
    }

    public static float inOutExpo(float p)
    {
        if (p == 0f)
            return b;
        if (p == 1f)
            return b + c;

        p *= 2f;
        if (p < 1)
            return c / 2f * Mathf.Pow(2f, 10f * (p - 1f)) + b - c * 0.0005f;

        p--;
        return c / 2f * 1.0005f * (-Mathf.Pow(2f, -10f * p) + 2f) + b;
    }

    public static float outInExpo(float p)
    {
        if (p < 0.5f)
            return outExpo(p * 2f, b, c / 2f);
        return inExpo((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inCirc(float p)
    {
        return inCirc(p, b, c);
    }
    public static float inCirc(float p, float b, float c)
    {
        return -c * (Mathf.Sqrt(1f - p * p) - 1f) + b;
    }

    public static float outCirc(float p)
    {
        return outCirc(p, b, c);
    }
    public static float outCirc(float p, float b, float c)
    {
        p -= 1f;
        return c * Mathf.Sqrt(1f - p * p) + b;
    }

    public static float inOutCirc(float p)
    {
        p *= 2f;
        if (p < 1f)
            return -c / 2f * (Mathf.Sqrt(1f - p * p) - 1f) + b;
        p -= 2f;
        return c / 2f * (Mathf.Sqrt(1f - p * p) + 1f) + b;
    }

    public static float outInCirc(float p)
    {
        if (p < 0.5f)
            return outCirc(p * 2f, b, c / 2f);
        return inCirc((p * 2f) - 1f, b + c / 2f, c / 2f);
    }

    public static float inBounce(float p)
    {
        return inBounce(p, b, c);
    }
    public static float inBounce(float p, float b, float c)
    {
        return c - outBounce(1f - p, 0f, c) + b;
    }

    public static float outBounce(float p)
    {
        return outBounce(p, b, c);
    }
    public static float outBounce(float p, float b, float c)
    {
        if (p < (1.0f / 2.75f))
        {
            return c * (7.5625f * p * p) + b;
        }
        else if (p < (2.0f / 2.75f))
        {
            p -= (1.5f / 2.75f);
            return c * (7.5625f * p * p + .75f) + b;
        }
        else if (p < (2.5f / 2.75f))
        {
            p -= (2.25f / 2.75f);
            return c * (7.5625f * p * p + .9375f) + b;
        }
        else
        {
            p -= (2.625f / 2.75f);
            return c * (7.5625f * p * p + .984375f) + b;
        }
    }

    public static float inOutBounce(float p)
    {
        if (p < 0.5f)
            return inBounce(p * 2f, 0f, c) * .5f + b;
        else
            return outBounce(p * 2f - 1f, 0f, c) * .5f + c * .5f + b;
    }

    public static float outInBounce(float p)
    {
        if (p < 0.5f)
            return outBounce(p * 2.0f, b, c / 2.0f);
        return inBounce((p * 2.0f) - 1f, b + c / 2.0f, c / 2.0f);
    }

    public static float inElastic(float p, float b, float c)
    {
        return inElastic(p, b, c);
    }
    public static float inElastic(float p)
    {
        if (p == 0f)
            return b;
        if (p == 1f)
            return b + c;

        float g = 0.3f;
        float s;
        float a = 0.0f;
        if (a < Mathf.Abs(c))
        {
            a = c;
            s = g / 4f;
        }
        else
            s = g / (2.0f * Mathf.PI) * Mathf.Asin(c / a);

        return -(a * Mathf.Pow(2f, 10.0f * (p -= 1f)) * Mathf.Sin((p - s) * (2.0f * Mathf.PI) / g)) + b;
    }

    public static float outElastic(float p)
    {
        return outElastic(p, b, c);
    }
    public static float outElastic(float p, float b, float c)
    {
        if (p == 0f)
            return b;
        if (p == 1f)
            return b + c;

        float g = .3f;
        float s;
        float a = 0.0f;
        if (a < Mathf.Abs(c))
        {
            a = c;
            s = g / 4.0f;
        }
        else
            s = g / (2.0f * Mathf.PI) * Mathf.Asin(c / a);

        return (a * Mathf.Pow(2f, -10.0f * p) * Mathf.Sin((p - s) * (2.0f * Mathf.PI) / g) + c + b);
    }

    public static float inOutElastic(float p)
    {
        if (p == 0f)
            return b;
        if ((p /= 0.5f) == 2f)
            return b + c;

        float g = .3f * 1.5f;
        float s;
        float a = 0.0f;
        if (a < Mathf.Abs(c))
        {
            a = c;
            s = g / 4.0f;
        }
        else
            s = g / (2.0f * Mathf.PI) * Mathf.Asin(c / a);

        if (p < 1f)
            return -.5f * (a * Mathf.Pow(2f, 10.0f * (p -= 1f)) * Mathf.Sin((p - s) * (2.0f * Mathf.PI) / g)) + b;
        return a * Mathf.Pow(2f, -10.0f * (p -= 1f)) * Mathf.Sin((p - s) * (2.0f * Mathf.PI) / g) * .5f + c + b;
    }

    public static float outInElastic(float p)
    {
        if (p < 0.5f)
            return outElastic(p * 2.0f, b, c / 2.0f);
        return inElastic((p * 2.0f) - 1f, b + c / 2.0f, c / 2.0f);
    }
    

    public static float inBack(float p)
    {
        return inBack(p, b, c);
    }
    public static float inBack(float p, float b, float c)
    {
        float s = 1.70158f;
        return c * p * p * ((s + 1f) * p - s) + b;
    }

    public static float outBack(float p)
    {
        return outBack(p, b, c);
    }
    public static float outBack(float p, float b, float c)
    {
        p -= 1f;
        float s = 1.70158f;
        return c * (p * p * ((s + 1) * p + s) + 1f) + b;
    }

    public static float inOutBack(float p)
    {
        float s = 1.70158f;
        if ((p /= 0.5f) < 1)
            return c / 2f * (p * p * (((s *= (1.525f)) + 1f) * p - s)) + b;
        return c / 2f * ((p -= 2f) * p * (((s *= (1.525f)) + 1f) * p + s) + 2f) + b;
    }

    public static float outInBack(float p)
    {
        if (p < 0.5f)
            return outBack(p * 2.0f, b, c / 2.0f);
        return inBack((p * 2.0f) - 1f, b + c / 2.0f, c / 2.0f);
    }

    public static float inBack2(float p)
    {
        return inBack2(p, b, c);
    }
    public static float inBack2(float p, float b, float c)
    {
        float s = 1.70158f * 2;
        return c * p * p * ((s + 1f) * p - s) + b;
    }

    public static float outBack2(float p)
    {
        return outBack2(p, b, c);
    }
    public static float outBack2(float p, float b, float c)
    {
        p -= 1f;
        float s = 1.70158f * 2;
        return c * (p * p * ((s + 1) * p + s) + 1f) + b;
    }

    public static float inOutBack2(float p)
    {
        float s = 1.70158f * 2;
        if ((p /= 0.5f) < 1)
            return c / 2f * (p * p * (((s *= (1.525f)) + 1f) * p - s)) + b;
        return c / 2f * ((p -= 2f) * p * (((s *= (1.525f)) + 1f) * p + s) + 2f) + b;
    }

    public static float outInBack2(float p)
    {
        if (p < 0.5f)
            return outBack2(p * 2.0f, b, c / 2.0f);
        return inBack2((p * 2.0f) - 1f, b + c / 2.0f, c / 2.0f);
    }

    public static Function curveFunction(AnimationCurve curve)
    {
        Function fun = (percentage) =>
        {
            //Debug.Log("curveFunction=" + curve.GetHashCode());
            return c * curve.Evaluate(percentage) + b;
        };

        //Debug.Log("curveFunction=" + fun);
        return fun;
    }
}

