using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Serializable3DObject : MonoBehaviour
{

    public enum ObjectType
    {
        CUBE,
        SPHERE,
        ARROW,
        COFFEE_CUP,
        TEXT,
        ELECTRIC_BOX,
    };


    [System.Serializable]
    public class SerializableObjectData
    {
        public string objectName = "";
        public Vector3 position = Vector3.zero;
        public Quaternion rotation = Quaternion.identity;
        public Vector3 scale = Vector3.one;
        public ObjectType objectType = ObjectType.CUBE;
    };

    public ObjectType m_objectType = ObjectType.CUBE;

    public SerializableObjectData Serialize()
    {
        SerializableObjectData objectData = new SerializableObjectData();
        objectData.objectName = name;
        objectData.position = transform.localPosition;
        objectData.rotation = transform.localRotation;
        objectData.scale = transform.localScale;
        objectData.objectType = m_objectType;

        return objectData;
    }
}
