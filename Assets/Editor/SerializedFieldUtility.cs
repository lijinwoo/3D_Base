using System;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace SystemicOverload.EditorTools
{
    /// <summary>
    /// 한국어 주석: PhaseValidation Scene 빌드 도구가 reflection 대신 사용할 SerializedObject 기반 필드 설정 헬퍼입니다.
    /// Undo/Dirty 처리와 [FormerlySerializedAs] 호환을 얻을 수 있고, 필드명 변경에 대한 오류 메시지가 명확해집니다.
    /// </summary>
    public static class SerializedFieldUtility
    {
        /// <summary>
        /// 한국어 주석: 단일 필드를 SerializedProperty로 설정하고 즉시 ApplyModifiedProperties로 반영합니다.
        /// 호환되지 않는 타입은 Debug.LogWarning을 통해 즉시 식별할 수 있게 합니다.
        /// </summary>
        public static void SetField(Object targetObject, string fieldName, object value)
        {
            if (targetObject == null)
            {
                Debug.LogWarning($"[SerializedFieldUtility] targetObject가 null입니다. fieldName={fieldName}");
                return;
            }

            using (SerializedObject serializedObject = new SerializedObject(targetObject))
            {
                SerializedProperty property = serializedObject.FindProperty(fieldName);
                if (property == null)
                {
                    Debug.LogWarning($"[SerializedFieldUtility] 필드를 찾지 못했습니다: {targetObject.GetType().Name}.{fieldName}");
                    return;
                }

                if (!TryAssignValue(property, value, out string failureReason))
                {
                    Debug.LogWarning($"[SerializedFieldUtility] 값 할당 실패({failureReason}): {targetObject.GetType().Name}.{fieldName}");
                    return;
                }

                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }
        }

        private static bool TryAssignValue(SerializedProperty property, object value, out string failureReason)
        {
            failureReason = string.Empty;

            switch (property.propertyType)
            {
                case SerializedPropertyType.Float:
                    property.floatValue = Convert.ToSingle(value);
                    return true;
                case SerializedPropertyType.Integer:
                    if (value is Enum enumValue)
                    {
                        property.intValue = Convert.ToInt32(enumValue);
                    }
                    else
                    {
                        property.intValue = Convert.ToInt32(value);
                    }
                    return true;
                case SerializedPropertyType.Boolean:
                    property.boolValue = Convert.ToBoolean(value);
                    return true;
                case SerializedPropertyType.String:
                    property.stringValue = Convert.ToString(value);
                    return true;
                case SerializedPropertyType.Enum:
                    property.enumValueIndex = Convert.ToInt32(value);
                    return true;
                case SerializedPropertyType.LayerMask:
                    property.intValue = value is LayerMask layerMask ? layerMask.value : Convert.ToInt32(value);
                    return true;
                case SerializedPropertyType.ObjectReference:
                    property.objectReferenceValue = value as Object;
                    return true;
                case SerializedPropertyType.Vector3:
                    if (value is Vector3 vector3Value)
                    {
                        property.vector3Value = vector3Value;
                        return true;
                    }
                    failureReason = "Vector3 expected";
                    return false;
                case SerializedPropertyType.Vector2:
                    if (value is Vector2 vector2Value)
                    {
                        property.vector2Value = vector2Value;
                        return true;
                    }
                    failureReason = "Vector2 expected";
                    return false;
                default:
                    failureReason = $"unsupported propertyType={property.propertyType}";
                    return false;
            }
        }
    }
}
