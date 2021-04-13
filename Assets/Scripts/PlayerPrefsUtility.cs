using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class PlayerPrefsUtility
{
    public static void SetString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
        PlayerPrefs.Save();
    }

    public static void SetInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
        PlayerPrefs.Save();
    }

    public static void SetFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
        PlayerPrefs.Save();
    }

    public static void SetBool(string key, bool value)
    {
        PlayerPrefs.SetInt(key, value?1:0);
        PlayerPrefs.Save();
    }

    //The methods below are silly, but the above ones have a bit more validity, so I wanted to have everything wrapped at that point.
    public static string GetString(string key)
    {
        return PlayerPrefs.GetString(key);
    }

    public static float GetFloat(string key)
    {
        return PlayerPrefs.GetFloat(key);
    }

    public static int GetInt(string key)
    {
        return PlayerPrefs.GetInt(key);
    }

    public static bool GetBool(string key)
    {
        return PlayerPrefs.GetInt(key, 0) >= 1;
    }

    public static bool HasKey(string key)
    {
        return PlayerPrefs.HasKey(key);
    }
}