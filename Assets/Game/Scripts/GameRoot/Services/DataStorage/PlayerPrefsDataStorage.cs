using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Scripts.GameRoot.Services.DataStorage
{
    /// <summary>
    /// An implementation of IDataStorage that uses PlayerPrefs for data persistence.
    /// </summary>
    /// <remarks>
    /// For complex types, it uses <see cref="JsonUtility"/> for serialization. <b>Important:</b> this imposes limitations on what can be serialized.
    /// <br/>
    /// <b>Note:</b> This implementation uses PlayerPrefs, which should only be called from the main Unity thread.
    /// </remarks>
    public class PlayerPrefsDataStorage : IDataStorage
    {
        /// <summary>
        /// List of Unity built-in types that JsonUtility can serialize/deserialize.
        /// </summary>
        private static readonly HashSet<Type> UnityBuiltinSerializableTypes = new()
        {
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Quaternion),
            typeof(Color),
            typeof(Color32),
            typeof(Rect),
            typeof(Bounds),
            typeof(AnimationCurve)
        };

        public void Set<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PlayerPrefsDataStorage] Set called with null or empty key.");
                return;
            }

            switch (value)
            {
                case int intValue:
                    PlayerPrefs.SetInt(key, intValue);
                    break;

                case float floatValue:
                    PlayerPrefs.SetFloat(key, floatValue);
                    break;

                case string stringValue:
                    PlayerPrefs.SetString(key, stringValue);
                    break;

                case bool boolValue:
                    PlayerPrefs.SetInt(key, boolValue ? 1 : 0);
                    break;

                case DateTime dateTimeValue:
                    PlayerPrefs.SetString(key, dateTimeValue.ToBinary().ToString());
                    break;

                default:
                    if (!CanSerialize<T>(out var failReason))
                    {
                        Debug.LogError($"[PlayerPrefsDataStorage] Value for key {key} not set. {failReason}");
                        return;
                    }

                    var json = JsonUtility.ToJson(value);
                    PlayerPrefs.SetString(key, json);
                    break;
            }
            PlayerPrefs.Save();
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PlayerPrefsDataStorage] Get called with null or empty key.");
                return defaultValue;
            }

            if (!HasKey(key))
                return defaultValue;

            var type = typeof(T);
            switch (type)
            {
                case Type t when t == typeof(int):
                    return (T)(object)PlayerPrefs.GetInt(key);
                case Type t when t == typeof(float):
                    return (T)(object)PlayerPrefs.GetFloat(key);
                case Type t when t == typeof(string):
                    return (T)(object)PlayerPrefs.GetString(key);
                case Type t when t == typeof(bool):
                    return (T)(object)(PlayerPrefs.GetInt(key) == 1);
                case Type t when t == typeof(DateTime):
                    var binary = Convert.ToInt64(PlayerPrefs.GetString(key));
                    return (T)(object)DateTime.FromBinary(binary);
                default:
                    if (!CanSerialize<T>(out var failReason))
                    {
                        Debug.LogError($"[PlayerPrefsDataStorage] Returning default value for key {key}. {failReason}");
                        return defaultValue;
                    }

                    var json = PlayerPrefs.GetString(key);
                    try
                    {
                        return JsonUtility.FromJson<T>(json);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[PlayerPrefsDataStorage] Failed to deserialize key {key}: {e}");
                        return defaultValue;
                    }
            }
        }

        public bool HasKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("[PlayerPrefsDataStorage] HasKey called with null or empty key.");
                return false;
            }

            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogWarning("[PlayerPrefsDataStorage] Attempted to delete with a null or empty key.");
                return;
            }

            PlayerPrefs.DeleteKey(key);
        }
        
        /// <summary>
        /// Checks if type T can be serialized by JsonUtility.
        /// </summary>
        private bool CanSerialize<T>(out string failReason)
        {
            var type = typeof(T);
            failReason = string.Empty;

            if (UnityBuiltinSerializableTypes.Contains(type))
            {
                return true;
            }
            
            // Must be a class or struct (not interface, enum, or delegate)
            if (!(type.IsClass || (type.IsValueType && !type.IsEnum)))
            {
                failReason = $"Type {type.Name} is not a class or struct.";
                return false;
            }

            if (typeof(System.Collections.IEnumerable).IsAssignableFrom(type))
            {
                failReason = $"Unsupported collection type {type.Name}. JsonUtility cannot serialize top-level collections.";
                return false;
            }

            if (type.IsPrimitive)
            {
                failReason = $"Unsupported primitive type {type.Name}.";
                return false;
            }

            if (!Attribute.IsDefined(type, typeof(SerializableAttribute)))
            {
                failReason = $"Non-serializable type {type.Name}.";
                return false;
            }

            return true;
        }
    }
}