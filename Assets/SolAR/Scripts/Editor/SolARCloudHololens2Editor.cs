/**
 * @copyright Copyright (c) 2022 B-com http://www.b-com.com/
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Bcom.Solar;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SolArCloudHololens2), true)]
public class SolARCloudHololens2Editor : Editor
{
    private SerializedProperty selectedSensorProp;
    private SerializedProperty pvParameters;
    private SerializedProperty leftFrontParameters;

    new SolArCloudHololens2 target => (SolArCloudHololens2)base.target;

    private bool showAdvancedCameraSettings = false;

    void OnEnable()
    {
        selectedSensorProp = serializedObject.FindProperty("selectedSensor");
        pvParameters = serializedObject.FindProperty("pvParameters");
        leftFrontParameters = serializedObject.FindProperty("leftFrontParameters");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        showAdvancedCameraSettings = EditorGUILayout.Foldout(showAdvancedCameraSettings, "Advanced Camera Settings");

        if (showAdvancedCameraSettings)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            if (GUILayout.Button("Reset"))
            {
                ResetDefaultCameraParameters(target.GetPvDefaultParameters(),
                                             pvParameters);
                ResetDefaultCameraParameters(target.GetLeftFrontDefaultParameters(),
                                             leftFrontParameters);
            }

            EditorGUILayout.PropertyField(selectedSensorProp);

            SolArCloudHololens2.Hl2SensorTypeEditor selectedSensor =
                (SolArCloudHololens2.Hl2SensorTypeEditor)selectedSensorProp.enumValueIndex;

            switch (selectedSensor)
            {
                case SolArCloudHololens2.Hl2SensorTypeEditor.PV:
                    updateCameraParameters(pvParameters);
                    target.selectedCameraParameter = target.pvParameters;
                    break;

                case SolArCloudHololens2.Hl2SensorTypeEditor.RM_LEFT_FRONT:
                    updateCameraParameters(leftFrontParameters);
                    target.selectedCameraParameter = target.leftFrontParameters;
                    break;
            }
        }

        serializedObject.ApplyModifiedProperties();
        serializedObject.Update();
        EditorUtility.SetDirty(target);

    }

    private void updateCameraParameters(SerializedProperty camParametersObject)
    {
        GUI.enabled = false;
        OnGuiIntCamParamProperty(camParametersObject, "width");
        OnGuiIntCamParamProperty(camParametersObject, "height");
        GUI.enabled = true;

        OnGuiDoubleCamParamProperty(camParametersObject, "focalX");
        OnGuiDoubleCamParamProperty(camParametersObject, "focalY");
        OnGuiDoubleCamParamProperty(camParametersObject, "centerX");
        OnGuiDoubleCamParamProperty(camParametersObject, "centerY");
        OnGuiDoubleCamParamProperty(camParametersObject, "distK1");
        OnGuiDoubleCamParamProperty(camParametersObject, "distK2");
        OnGuiDoubleCamParamProperty(camParametersObject, "distP1");
        OnGuiDoubleCamParamProperty(camParametersObject, "distP2");
        OnGuiDoubleCamParamProperty(camParametersObject, "distK3");
    }

    private void OnGuiIntCamParamProperty(SerializedProperty camParametersObject,
                                          string propName)
    {
        int value = EditorGUILayout.DelayedIntField(propName,
            camParametersObject.FindPropertyRelative(propName).intValue);

        camParametersObject.FindPropertyRelative(propName).intValue = value;
    }

    private void OnGuiDoubleCamParamProperty(SerializedProperty camParametersObject,
                                             string propName)
    {
        double value = EditorGUILayout.DelayedDoubleField(propName,
            camParametersObject.FindPropertyRelative(propName).doubleValue);

        camParametersObject.FindPropertyRelative(propName).doubleValue = value;
    }

    public void ResetDefaultCameraParameters(
        SolArCloudHololens2.CameraParameters camParametersObjectSrc,
        SerializedProperty camParametersObjectDest)
    {
        camParametersObjectDest.FindPropertyRelative("width").intValue =
            EditorGUILayout.DelayedIntField("width", (int)camParametersObjectSrc.width);
        camParametersObjectDest.FindPropertyRelative("height").intValue =
            EditorGUILayout.DelayedIntField("height", (int)camParametersObjectSrc.height);
        camParametersObjectDest.FindPropertyRelative("focalX").doubleValue =
            EditorGUILayout.DelayedDoubleField("focalX", camParametersObjectSrc.focalX);
        camParametersObjectDest.FindPropertyRelative("focalY").doubleValue =
            EditorGUILayout.DelayedDoubleField("focalY", camParametersObjectSrc.focalY);
        camParametersObjectDest.FindPropertyRelative("centerX").doubleValue =
            EditorGUILayout.DelayedDoubleField("centerX", camParametersObjectSrc.centerX);
        camParametersObjectDest.FindPropertyRelative("centerY").doubleValue =
            EditorGUILayout.DelayedDoubleField("centerY", camParametersObjectSrc.centerY);
        camParametersObjectDest.FindPropertyRelative("distK1").doubleValue =
            EditorGUILayout.DelayedDoubleField("distK1", camParametersObjectSrc.distK1);
        camParametersObjectDest.FindPropertyRelative("distK2").doubleValue =
            EditorGUILayout.DelayedDoubleField("distK2", camParametersObjectSrc.distK2);
        camParametersObjectDest.FindPropertyRelative("distP1").doubleValue =
            EditorGUILayout.DelayedDoubleField("distP1", camParametersObjectSrc.distP1);
        camParametersObjectDest.FindPropertyRelative("distP2").doubleValue =
            EditorGUILayout.DelayedDoubleField("distP2", camParametersObjectSrc.distP2);
        camParametersObjectDest.FindPropertyRelative("distK3").doubleValue =
            EditorGUILayout.DelayedDoubleField("distK3", camParametersObjectSrc.distK3);
    }
}
