using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using UnityEngine.SceneManagement;
//using UnityEngine.Rendering.Universal;
[RequireComponent(typeof(Stereo3DImageNew))]
public class Stereo3DImageConfigNew : MonoBehaviour
{
    private Stereo3DImageNew _stereo3D;
    private Stereo3DImageNew stereo3D
    {
        get
        {
            if (_stereo3D == null)
            {
                _stereo3D = GetComponent<Stereo3DImageNew>();
            }
            return _stereo3D;
        }
    }
    private StereoConfigData configData = null;
    private string configFile = "Stereo3DConfig.json";
    private string configFileFullPath => Path.Combine(Application.streamingAssetsPath, configFile);
    private void OnEnable()
    {
        LoadConfig();
    }
    private void OnDestroy()
    {
        ApplyAndSaveData();
    }


    private void LoadConfig()
    {
        if (!File.Exists(configFileFullPath))
        {
            configData = new StereoConfigData();
            ApplyPropertiesToConfig();
        }
        else
        {
            var json = File.ReadAllText(configFileFullPath);
            configData = JsonUtility.FromJson<StereoConfigData>(json);
            ApplyConfigToProperties();
        }
    }
    private void ApplyAndSaveData()
    {
        ApplyPropertiesToConfig();
        SaveConfig();
    }

    [ContextMenu("保存配置")]
    private void SaveConfig()
    {
        var json = JsonUtility.ToJson(configData);
        //Debug.Log(json);
        File.WriteAllText(configFileFullPath, json);
    }
    private void ApplyConfigToProperties()
    {
        foreach (var i in configData.data)
        {
            if (i.sceneName == SceneManager.GetActiveScene().name)
            {
                stereo3D.S3DEnabled = i.s3dEnable;
                stereo3D.stereoType = (StereoType)i.steoroType;
                stereo3D.isRevert = i.isRevert;
                stereo3D.hFOV = i.hFOV;
                stereo3D.IPA = i.IPA;
                stereo3D.IPD = i.IPD;
                stereo3D.height = i.height;
                stereo3D.distance = i.distance;
            }
        }

    }
    private void ApplyPropertiesToConfig()
    {
        ConfigData data = new ConfigData();
        if (configData.data != null)
            foreach (var i in configData.data)
            {
                if (i.sceneName == SceneManager.GetActiveScene().name)
                {
                    data = i;
                    i.s3dEnable = stereo3D.S3DEnabled;
                    i.isRevert = stereo3D.isRevert;
                    i.hFOV = stereo3D.hFOV;
                    i.IPA = stereo3D.IPA;
                    i.IPD = stereo3D.IPD;
                    i.GuiVisible = stereo3D.GuiVisible;
                    i.steoroType = (int)stereo3D.stereoType;
                    i.height = stereo3D.height;
                    i.distance = stereo3D.distance;
                }
            }

        if (string.IsNullOrEmpty(data.sceneName))
        {
            data.sceneName = SceneManager.GetActiveScene().name;
            data.s3dEnable = stereo3D.S3DEnabled;
            data.isRevert = stereo3D.isRevert;
            data.hFOV = stereo3D.hFOV;
            data.IPA = stereo3D.IPA;
            data.IPD = stereo3D.IPD;
            data.GuiVisible = stereo3D.GuiVisible;
            data.steoroType = (int)stereo3D.stereoType;
            data.height = stereo3D.height;
            data.distance = stereo3D.distance;
            if (configData == null)
                configData = new StereoConfigData();
            if (configData.data == null)
            {
                configData.data = new List<ConfigData>();
                configData.data.Add(data);
            }
            else
            {
                configData.data.Add(data);
            }
        }

    }
    [Serializable]
    private class StereoConfigData
    {
        public List<ConfigData> data;
    }
    [Serializable]
    public class ConfigData
    {
        //场景名
        public string sceneName;
        //是否反转画面
        public bool isRevert;
        //是否启用3D
        public bool s3dEnable;
        //3D类型:隔行 隔列
        public int steoroType;
        //视口大小
        public float hFOV = 28.24f;
        //视角偏移
        public float IPA = 0;
        //瞳距距离
        public float IPD = 0;
        public bool GuiVisible = true;
        public float distance = 3f;
        public float height = 0;
    }

}
public enum StereoType { Col, Row, SideBySide, TopBottom };
