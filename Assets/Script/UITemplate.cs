using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class UITemplate : MonoBehaviour {

#if UNITY_EDITOR
    [HideInInspector] 
    public int m_GUID = 0;

    [HideInInspector]
    public string m_Path = "";

	[HideInInspector][System.NonSerialized]
    public List<GameObject> m_SearPrefabs = new List<GameObject>();


    public void InitGUID(string path)
    {
        if(m_GUID == 0)
        {
            m_GUID = Random.Range(int.MinValue, int.MaxValue);
            m_Path = path;
        }
    }

    public void DestroyChildTemp()
    {
        List<GameObject> list = GetAsComponentObj(gameObject, "UITemplateChild");
        foreach (GameObject cGo in list)
        {
            UITemplateChild temChild = cGo.GetComponent<UITemplateChild>();
            if (temChild.m_PartGUID == m_GUID)
            {
                DestroyImmediate(temChild);
            }
        }
        DestroyImmediate(gameObject.GetComponent<UITemplate>());
    }

    public void GetChild(GameObject go, List<GameObject> list)
    {
        list.Add(go);
        if (go.transform.childCount == 0)
            return;
        for (int i = 0; i < go.transform.childCount; i++)
        {
            GameObject childGo = go.transform.GetChild(i).gameObject;
            GetChild(childGo, list);
        }
    }

    public List<GameObject> GetAsComponentObj(GameObject go, string component)
    {
        List<GameObject> list = new List<GameObject>();
        List<GameObject> childList = new List<GameObject>();
        GetChild(go, childList);
        foreach (GameObject childGo in childList)
        {
            var com = childGo.GetComponent(component);
            if (com != null)
            {
                list.Add(childGo);
            }
        }
        var comPart = go.GetComponent(component);
        if (comPart)
        {
            list.Add(go);
        }
        return list;
    }

#endif

}
