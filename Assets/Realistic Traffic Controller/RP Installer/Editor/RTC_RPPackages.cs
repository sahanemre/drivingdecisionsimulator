//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2024 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// All RP packages.
/// </summary>
public class RTC_RPPackages : ScriptableObject {

    #region singleton
    private static RTC_RPPackages instance;
    public static RTC_RPPackages Instance { get { if (instance == null) instance = Resources.Load("RTC_RPPackages") as RTC_RPPackages; return instance; } }
    #endregion

    public Object Builtin;
    public Object URP;
    public Object HDRP;

    public bool dontWarnAgain = false;

    public string GetAssetPath(Object pathObject) {

        string path = UnityEditor.AssetDatabase.GetAssetPath(pathObject);
        return path;

    }

}
