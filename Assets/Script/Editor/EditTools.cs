using UnityEngine;
using System.Collections;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.UI;

public class EditTools : EditorWindow {

    public static void SetPTString(int tab, string data)
    {
        string dataPath = Application.dataPath + "/PrefabToolbar/Config/";
        string txtPath = dataPath + "config" + tab + ".txt";
        File.WriteAllText(txtPath, data, Encoding.UTF8);
    }

    public static string GetPTString(int tab)
    {
        string dataPath = Application.dataPath + "/PrefabToolbar/Config/";
        string txtPath = dataPath + "config" + tab + ".txt";
        if (File.Exists(txtPath))
        {
            return File.ReadAllText(txtPath);
        }
        else
        {
            return "";
        }
    }

    /// <summary>
    /// 将目标label的值复制到目标label
    /// 并且由子模板的身份变成父模板
    /// </summary>
    /// <param name="sourceGo">要被复制属性的目标uilabel对象</param>
    public static void CopyTemplateLabelValue(GameObject sourceGo)
    {
        Text sourceLab = sourceGo.GetComponent<Text>();
        UITemplate sourceTem = sourceGo.GetComponent<UITemplate>();
        if (!sourceLab)
            return;
        UnityEngine.Object[] selectionAsset = Selection.GetFiltered(typeof(GameObject), SelectionMode.TopLevel); //获取当前鼠标选中的所有对象
        foreach (GameObject sGo in selectionAsset)
        {
            Text lab = sGo.GetComponent<Text>();
            if (lab)
            {
                CopyComponent(sourceLab, lab);

                UITemplate tem = sGo.GetComponent<UITemplate>();
                UITemplateChild temc = sGo.GetComponent<UITemplateChild>();

                if (tem)
                {
                }
                else if (temc)
                {
                    DestroyImmediate(temc);
                    tem = sGo.AddComponent<UITemplate>(); //由子模板的身份变成父模板
                }
                else
                {
                    tem = sGo.AddComponent<UITemplate>();
                }
                EditorUtility.CopySerialized(sourceTem, tem);
                EditorUtility.SetDirty(lab);
            }
        }
    }

    /// <summary>
    /// 将模板上的所有组件属性复制到已经实例的对象上， 如果实例没有对应组件则创建再复制目标的值
    /// 注意：以下这些无法apply同步
    /// 1：一个gameobject上不要有两个一样的组件， 例如有两个uisprite， 这样结果有可能导致两个组件的值对换了
    /// 2：支持给模板增加组件，但不支持删除组件，避免误删别人的组件
    /// 3：模板增删gameobject也是无法同步的
    /// 你想到什么好办法的话，以上三个问题都是可以解决的！
    /// </summary>
    /// <param name="source">模板对象</param>
    /// <param name="dest">目标对象</param>
    public static void CopyComponents(GameObject source, GameObject dest)
    {
        MonoBehaviour[] sourceList = source.GetComponents<MonoBehaviour>(); //获取所有的脚本
        MonoBehaviour[] destList = dest.GetComponents<MonoBehaviour>();
        for (int i = 0; i < sourceList.Length; i++)
        {
            bool isSet = false;
            System.Type sourceType = sourceList[i].GetType();
            if (sourceType.ToString() == "UITemplate" || sourceType.ToString() == "UITemplateChild")
            {
                continue;
            }
            for (int j = 0; j < destList.Length; j++)
            {
                isSet = CopyComponent(sourceList[i], destList[j]);
                if (isSet)
                {
                    break;
                }
            }
            if (!isSet)
            {
                Component newCom = dest.AddComponent(sourceType);
                EditorUtility.CopySerialized(sourceList[i], newCom);
            }
        }
    }

    /// <summary>
    /// 将模板组件的值复制到实例组件上
    /// 组件上部分属性的值不会修改，例如不会把目标uilabel上的text和size修改
    /// </summary>
    /// <param name="sourceMon">模板组件</param>
    /// <param name="destMon">目标组件</param>
    /// <returns></returns>
    public static bool CopyComponent(MonoBehaviour sourceMon, MonoBehaviour destMon)
    {
        System.Type sourceType = sourceMon.GetType();
        System.Type destType = destMon.GetType();
        if (sourceType == destType)
        {
            string labTxt = null;
            int labSize = 0;
            int depth = 0;

            if (destType.ToString() == "UnityEngine.UI.Text")
            {
                labTxt = (destMon as Text).text == (sourceMon as Text).text ? labTxt : (destMon as Text).text;
                labSize = (destMon as Text).fontSize == (sourceMon as Text).fontSize ? labSize : (destMon as Text).fontSize;
            }

            EditorUtility.CopySerialized(sourceMon, destMon); //类似于右键组件的Paste Component Values

            if (labTxt != null)
            {
                (destMon as Text).text = labTxt;
            }
            if (labSize != 0)
            {
                (destMon as Text).fontSize = labSize;
            }

            return true;
        }
        return false;
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
}
