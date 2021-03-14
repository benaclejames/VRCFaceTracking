//========= Copyright 2019, HTC Corporation. All rights reserved. ===========
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;

namespace ViveSR
{
    namespace anipal
    {
        namespace Lip
        {
            [CustomPropertyDrawer(typeof(LipShapeTable_v2))]
            public class SRanipal_LipShapeTableDrawer_v2 : PropertyDrawer
            {
                public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
                {
                    EditorGUI.BeginProperty(position, label, property);

                    Rect newFieldPosition = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
                    newFieldPosition.height = EditorGUIUtility.singleLineHeight;
                    Rect newLabelPosition = position;
                    newLabelPosition.width -= newFieldPosition.width;

                    SerializedProperty propSkinedMesh = property.FindPropertyRelative("skinnedMeshRenderer");
                    SerializedProperty propLipShapes = property.FindPropertyRelative("lipShapes");
                    EditorGUI.PropertyField(newFieldPosition, propSkinedMesh, GUIContent.none);
                    newFieldPosition.y += EditorGUIUtility.singleLineHeight;

                    SkinnedMeshRenderer skinnedMesh = propSkinedMesh.objectReferenceValue as SkinnedMeshRenderer;
                    if (skinnedMesh != null && skinnedMesh.sharedMesh.blendShapeCount > 0)
                    {
                        if (propLipShapes.arraySize != skinnedMesh.sharedMesh.blendShapeCount)
                        {
                            propLipShapes.arraySize = skinnedMesh.sharedMesh.blendShapeCount;
                            for (int i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; ++i)
                            {
                                SerializedProperty propLipShape = propLipShapes.GetArrayElementAtIndex(i);
                                string elementName = skinnedMesh.sharedMesh.GetBlendShapeName(i);

                                propLipShape.intValue = (int)LipShape_v2.None;
                                foreach (LipShape_v2 lipShape in (LipShape_v2[])Enum.GetValues(typeof(LipShape_v2)))
                                {
                                    if (elementName == lipShape.ToString())
                                        propLipShape.intValue = (int)lipShape;
                                }
                            }
                        }
                        for (int i = 0; i < skinnedMesh.sharedMesh.blendShapeCount; ++i)
                        {
                            SerializedProperty propLipShape = propLipShapes.GetArrayElementAtIndex(i);
                            newLabelPosition.y = newFieldPosition.y;
                            string elementName = skinnedMesh.sharedMesh.GetBlendShapeName(i);
                            EditorGUI.LabelField(newLabelPosition, "  " + elementName);
                            EditorGUI.PropertyField(newFieldPosition, propLipShape, GUIContent.none);
                            newFieldPosition.y += EditorGUIUtility.singleLineHeight;
                        }
                    }
                    EditorGUI.EndProperty();
                }

                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    int LineCount = 1;
                    SerializedProperty propSkinedMesh = property.FindPropertyRelative("skinnedMeshRenderer");
                    SkinnedMeshRenderer skinnedMesh = propSkinedMesh.objectReferenceValue as SkinnedMeshRenderer;
                    if (skinnedMesh != null) LineCount += skinnedMesh.sharedMesh.blendShapeCount;
                    return EditorGUIUtility.singleLineHeight * LineCount;
                }
            }
        }
    }
}
#endif