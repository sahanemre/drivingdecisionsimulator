//----------------------------------------------
//        Realistic Car Controller Pro
//
// Copyright © 2014 - 2024 BoneCracker Games
// https://www.bonecrackergames.com
// Ekrem Bugra Ozdoganlar
//
//----------------------------------------------

using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;

public class RTC_InitLoadForRP {

    [InitializeOnLoadMethod]
    public static void InitOnLoad() {

        EditorApplication.delayCall += EditorDelayedUpdate;
        EditorApplication.projectChanged += OnProjectChanged;
        EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;

    }

    public static void Check() {

        if (RTC_RPPackages.Instance.dontWarnAgain)
            return;

        bool installedRP = false;

#if RTC_RP
        installedRP = true;
#endif

        if (!installedRP) {

            int decision = EditorUtility.DisplayDialogComplex("Realistic Traffic Controller | Select Render Pipeline", "Which render pipeline will be imported?\n\nThis process is irreversible, once you select the render pipeline, compatible version of RTC will be imported. Switching between render pipelines is not supported.\n\nIf you want to change the render pipeline after this step, you'll need to delete ''Realistic Traffic Controller'' folder from the project and import the new render pipeline. Be sure your project has proper configuration setup for the selected render pipeline. ", "Import [Universal Render Pipeline] (URP)", "Import [Builtin Render Pipeline] (Standard)", "Import [High Definition Render Pipeline] (HDRP)");

            if (decision == 0) {

                SessionState.SetBool("RTC_INSTALLINGRP", true);
                AssetDatabase.ImportPackage(RTC_RPPackages.Instance.GetAssetPath(RTC_RPPackages.Instance.URP), true);

            }

            if (decision == 2) {

                SessionState.SetBool("RTC_INSTALLINGRP", true);
                AssetDatabase.ImportPackage(RTC_RPPackages.Instance.GetAssetPath(RTC_RPPackages.Instance.HDRP), true);

            }

            if (decision == 1) {

                SessionState.SetBool("RTC_INSTALLINGRP", true);
                AssetDatabase.ImportPackage(RTC_RPPackages.Instance.GetAssetPath(RTC_RPPackages.Instance.Builtin), true);

            }

            RTC_RPPackages.Instance.dontWarnAgain = true;

            EditorUtility.SetDirty(RTC_RPPackages.Instance);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

        }

    }

    private static void EditorApplication_playModeStateChanged(PlayModeStateChange obj) {

        if (RTC_RPPackages.Instance.dontWarnAgain)
            return;

        if (obj == PlayModeStateChange.EnteredEditMode)
            Check();

    }

    public static void EditorDelayedUpdate() {

        if (RTC_RPPackages.Instance.dontWarnAgain)
            return;

        if (EditorApplication.isPlayingOrWillChangePlaymode)
            return;

        Check();

    }

    public static void OnProjectChanged() {

        if (RTC_RPPackages.Instance.dontWarnAgain)
            return;

        if (SessionState.GetBool("RTC_INSTALLINGRP", false)) {

            SessionState.EraseBool("RTC_INSTALLINGRP");
            RTC_SetScriptingSymbolForRP.SetEnabled("RTC_RP", true);

        }

    }

}
