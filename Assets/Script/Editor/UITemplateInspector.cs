using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/*
 * 从里面拉出来的预设会和UIPrefablTool建立关联，按一下模板预设UITemplate脚本上的apply就能把所有和他关联的预设同步修改。
 * 一键apply这个功能建议用在UIPrefablTool面板上的按钮、字体、UI模块，因为这些相对改动不会太大，容易规范。
 * 如果从UIPrefablTool拉出来的预设不想被关联，可以按下UITemplate脚本上delete这个按钮，不要直接remove component， 因为可能模板的子对象还有一个UITemplateChild的脚本，也要对应删除。
 * 放这些模板的文件夹是：PrefabToolbar，也可以右键场景上的预设，通过UITemplate里的选项创建到文件夹。
 */

[CustomEditor(typeof(UITemplate))]
public class UITemplateInspector : Editor
{

	//------------------------------------------------------------//

    //模板
    private UITemplate m_UITemplate;

	//模板存放的路径
    private static string TEMPLATE_PREFAB_PATH =  "";
    

    //Prefab存放的路径
    private static List<string> m_PrefabsPath = new List<string>()
    {
       "Assets/Resources/Prefabs/UI/",
       //"Assets/Resources/Prefabs/War/"
    };

    //模板存放的路径，需要按顺序
    private static string[] m_TemplatePath = 
    {
        "Assets/PrefabToolbar/按钮",
        "Assets/PrefabToolbar/字体",
        "Assets/PrefabToolbar/公共控件",
        "Assets/PrefabToolbar/界面",
        "Assets/PrefabToolbar/其他",
    };

	//------------------------------------------------------------//

    [MenuItem("GameObject/UITemplate/创建到按钮文件夹", false, 11)]
    static void CreateToButton(MenuCommand menuCommand)
    {
        TEMPLATE_PREFAB_PATH = m_TemplatePath[0];
        CreatToPrefab(menuCommand);
    }

    [MenuItem("GameObject/UITemplate/创建到字体文件夹", false, 11)]
    static void CreateToFont(MenuCommand menuCommand)
    {
        TEMPLATE_PREFAB_PATH = m_TemplatePath[1];
        CreatToPrefab(menuCommand);
    }

    [MenuItem("GameObject/UITemplate/创建到界面文件夹", false, 11)]
    static void CreateToUI(MenuCommand menuCommand)
    {
        TEMPLATE_PREFAB_PATH = m_TemplatePath[3];
        CreatToPrefab(menuCommand);
    }

    //不建议公共控件和其他文件里的预设使用uitemplate组件， 因为这些在实际开发中这些变动性太大了

    //[MenuItem("GameObject/UITemplate/创建到公共控件文件夹", false, 11)]
    //static void CreateToPublic(MenuCommand menuCommand)
    //{
    //    TEMPLATE_PREFAB_PATH = m_TemplatePath[2];
    //    CreatToPrefab(menuCommand);
    //}

    //[MenuItem("GameObject/UITemplate/创建到其他文件夹", false, 11)]
    //static void CreateToOther(MenuCommand menuCommand)
    //{
    //    TEMPLATE_PREFAB_PATH = m_TemplatePath[4];
    //    CreatToPrefab(menuCommand);
    //}

    static void CreatToPrefab(MenuCommand menuCommand)
    {
        if (menuCommand.context != null)
        {
			CreatDirectory();
            GameObject selectGameObject = menuCommand.context as GameObject;

            Create(selectGameObject);
            GameObject.DestroyImmediate(selectGameObject);
        }
        else
        {
            EditorUtility.DisplayDialog("错误！", "请选择一个GameObject", "OK");
        }
    }

    void OnEnable()
    {
        m_UITemplate = (UITemplate)target;

        if (IsTemplatePrefabInInProjectView(m_UITemplate.gameObject))
        {
            ShowHierarchy();
        }
		CreatDirectory();
    }

    public override void OnInspectorGUI()
    {
        TEMPLATE_PREFAB_PATH = m_UITemplate.m_Path;
 	    base.OnInspectorGUI();
	    bool isPrefabInProjectView = IsTemplatePrefabInInProjectView(m_UITemplate.gameObject);
        string descTxt = "";

        EditorGUILayout.LabelField("GUID:" + m_UITemplate.m_GUID);
        GUILayout.BeginHorizontal();
        
		if (isPrefabInProjectView)
        {

	        if (GUILayout.Button("Search"))
	        {
	            TrySearchPrefab(m_UITemplate.m_GUID, out m_UITemplate.m_SearPrefabs);
				return;
	        }

	        if (GUILayout.Button("Apply"))
	        {
	            if (IsTemplatePrefabInHierarchy(m_UITemplate.gameObject))
	            {

	                ApplyPrefab(m_UITemplate.gameObject,PrefabUtility.GetPrefabParent(m_UITemplate.gameObject), true);
	            }
	            else
	            {
	               ApplyPrefab(m_UITemplate.gameObject, m_UITemplate.gameObject, false);
	            }
	            return;
	        }
            descTxt = "Search：搜索用到此模板的预设\n" + "Apply：将修改应用到所有关联的预设上（主要用在按钮和字体上）";
		}
        else
        {
            if (GUILayout.Button("Select"))
            {
			    DirectoryInfo directiory = CreatDirectory();
                FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);
                for (int i = 0; i < infos.Length; i++)
                {
                    FileInfo file = infos[i];
                    GameObject prefab = AssetDatabase.LoadAssetAtPath(file.FullName.Substring(file.FullName.IndexOf("Assets")), typeof(GameObject)) as GameObject;
                    if(prefab.GetComponent<UITemplate>().m_GUID == m_UITemplate.m_GUID)
                    {
                        EditorGUIUtility.PingObject(prefab);
                        return;
                    }
                }
            }
            
	        if (GUILayout.Button("Delete"))
	        {
                m_UITemplate.DestroyChildTemp();
				return;
	        }
            descTxt = "Select：选中关联的预设\n" + "Delete：替代RemoveComponent，因为要将子组件里的UITemplateChild也删除";
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.HelpBox(descTxt, MessageType.Info, true);


		if (isPrefabInProjectView)
		{
	        if(m_UITemplate != null && m_UITemplate.m_SearPrefabs.Count > 0)
	        {
	            foreach (GameObject p in m_UITemplate.m_SearPrefabs)
	            {
	                EditorGUILayout.Space();
	                if (GUILayout.Button(AssetDatabase.GetAssetPath(p))) {
	                    EditorGUIUtility.PingObject(p);
	                }
	            }
	        }
        }
    
    }

    static private bool TrySearchPrefab(int guid,out List<GameObject> searchList )
    {
        List<GameObject> prefabs = new List<GameObject>();
        bool trySearch = false;
        foreach(string forder in m_PrefabsPath)
        {
            DirectoryInfo directiory = new DirectoryInfo(Application.dataPath + "/" + forder.Replace("Assets/", ""));
            FileInfo[] infos = directiory.GetFiles("*.prefab", SearchOption.AllDirectories);
            for (int i = 0; i < infos.Length; i++)
            {
                FileInfo file = infos[i];
                GameObject prefab = AssetDatabase.LoadAssetAtPath(file.FullName.Substring(file.FullName.IndexOf("Assets")), typeof(GameObject)) as GameObject;
                if (prefab.GetComponentsInChildren<UITemplate>(true).Length > 0)
                {
                    GameObject go = Instantiate(prefab) as GameObject;
                    UITemplate[] templates = go.GetComponentsInChildren<UITemplate>();
                    foreach (UITemplate template in templates)
                    {
                        if (template.m_GUID == guid && !prefabs.Contains(prefab))
                        {
                            prefabs.Add(prefab);
                        }
                    }
                    GameObject.DestroyImmediate(go);
                }
            }
        }

        searchList = prefabs;
        return !trySearch;
    }

    static private  void ApplyPrefab(GameObject prefab, Object targetPrefab, bool replace)
    {
        if (EditorUtility.DisplayDialog("注意！", "是否进行递归查找批量替换模板？", "ok", "cancel"))
        {
            GameObject replacePrefab;
            //int count = 0;
            if (replace)
            {
                PrefabUtility.ReplacePrefab(prefab, targetPrefab, ReplacePrefabOptions.ConnectToPrefab);
                Refresh();
                replacePrefab = targetPrefab as GameObject;
                //count = prefab.GetComponentsInChildren<UITemplate>().Length;
                
            }
            else
            {
                replacePrefab = AssetDatabase.LoadAssetAtPath(AssetDatabase.GetAssetPath(targetPrefab), typeof(GameObject)) as GameObject; ;
                //GameObject checkPrefab = PrefabUtility.InstantiatePrefab(replacePrefab) as GameObject;
                //count = checkPrefab.GetComponentsInChildren<UITemplate>().Length;
                //DestroyImmediate(checkPrefab);
            }


         
            //if (count != 1)
            //{
            //    EditorUtility.DisplayDialog("注意！", "无法批量替换，因为模板不支持嵌套。", "ok");
            //    return;
            //}

            UITemplate template =  replacePrefab.GetComponent<UITemplate>();

            if(template != null)
            {
               List<GameObject> references;
               if (TrySearchPrefab(template.m_GUID, out references))
               {
                   GameObject checkPrefab = PrefabUtility.InstantiatePrefab(replacePrefab) as GameObject;
                    for (int i = 0; i < references.Count; i++)
                    {
                        GameObject reference = references[i];
                        GameObject go = PrefabUtility.InstantiatePrefab(reference) as GameObject;
                        UITemplate[] instanceTemplates = go.GetComponentsInChildren<UITemplate>();
                        for (int j = 0; j < instanceTemplates.Length; j++)
                        {
                            UITemplate instance = instanceTemplates[j];
                            if (instance.m_GUID == template.m_GUID)
                            {
                                EditTools.CopyComponents(template.gameObject, instance.gameObject);
                                UITemplateChild[] instanceTemplatesChild = go.GetComponentsInChildren<UITemplateChild>();
                                UITemplateChild[] templateTemplatesChild = checkPrefab.gameObject.GetComponentsInChildren<UITemplateChild>();
                                for (int x = 0; x < templateTemplatesChild.Length; x++)
                                {
                                    UITemplateChild templateChild = templateTemplatesChild[x];
                                    for (int y = 0; y < instanceTemplatesChild.Length; y++)
                                    {
                                        UITemplateChild instanceChild = instanceTemplatesChild[y];
                                        if (instanceChild.m_GUID == templateChild.m_GUID)
                                        {
                                            EditTools.CopyComponents(templateChild.gameObject, instanceChild.gameObject);
                                        }
                                    }
                                }
                            }
                            PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go), ReplacePrefabOptions.ConnectToPrefab);
                        }
                        DestroyImmediate(go);
                    }
                    DestroyImmediate(checkPrefab);
                }
            }
            ClearHierarchy();
            Refresh();
        }
    }



    //static private void DeletePrefab(string path)
    //{
    //    if (EditorUtility.DisplayDialog("注意！", "是否进行递归查找批量删除模板？", "ok", "cancel"))
    //    {
    //        Debug.Log("DeletePrefab : " + path);
    //        GameObject deletePrefab =  AssetDatabase.LoadAssetAtPath<GameObject>(path);
    //        UITemplate template = deletePrefab.GetComponent<UITemplate>();
    //        if (template != null)
    //        {
    //            List<GameObject> references;
    //            if (TrySearchPrefab(template.m_GUID, out references))
    //            {
                    
    //                foreach (GameObject reference in references)
    //                {
    //                    GameObject go = PrefabUtility.InstantiatePrefab(reference) as GameObject;
    //                    UITemplate[] instanceTemplates = go.GetComponentsInChildren<UITemplate>();
    //                    for (int i = 0; i < instanceTemplates.Length; i++)
    //                    {
    //                        UITemplate instance = instanceTemplates[i];
    //                        if (instance.m_GUID == template.m_GUID)
    //                        {
    //                            DestroyImmediate(instance.gameObject);
    //                        }
    //                    }
    //                    PrefabUtility.ReplacePrefab(go, PrefabUtility.GetPrefabParent(go), ReplacePrefabOptions.ConnectToPrefab);
    //                    DestroyImmediate(go);
    //                }
    //            }
    //        }
    //        AssetDatabase.DeleteAsset(path);
    //        ClearHierarchy();
    //        Refresh();
    //    }
    //}

    static private void Create(GameObject prefab)
    {

        string creatPath = TEMPLATE_PREFAB_PATH + "/" + prefab.name + ".prefab";
        if (AssetDatabase.LoadAssetAtPath(creatPath, typeof(GameObject)) as GameObject == null)
        {
            CreatPrefab(prefab);
        }
        else
        {
            if (EditorUtility.DisplayDialog("注意！", prefab.name + "已创建是否覆盖原有预设？", "ok", "cancel"))
            {
                CreatPrefab(prefab);
            }
        }
        
    }

    static private void CreatPrefab(GameObject prefab)
    {
        if (!prefab.GetComponent<UITemplate>())
        {
            prefab.AddComponent<UITemplate>().InitGUID(TEMPLATE_PREFAB_PATH);
        }
        int prefabGuid = prefab.GetComponent<UITemplate>().m_GUID;
        List<GameObject> childList = new List<GameObject>();
        new EditTools().GetChild(prefab, childList);
        foreach (GameObject childGo in childList)
        {
            UITemplate tem = childGo.GetComponent<UITemplate>();
            UITemplateChild temc = childGo.GetComponent<UITemplateChild>();
            if (tem || temc)
                continue;
            childGo.AddComponent<UITemplateChild>().InitGUID(prefabGuid);
        }
        PrefabUtility.CreatePrefab(TEMPLATE_PREFAB_PATH + "/" + prefab.name + ".prefab", prefab);
        Refresh();
    }

    static private void Refresh()
    {
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorApplication.SaveScene();
    }


    static  private void ClearHierarchy()
    {
        Canvas canvas = GameObject.FindObjectOfType<Canvas>();

        if (canvas != null)
        {
			for(int i =0; i < canvas.transform.childCount; i++){

				Transform t = canvas.transform.GetChild(i);
				if(t.GetComponent<UITemplate>()!= null){
					GameObject.DestroyImmediate(t.gameObject);
				}

			}
        }
    }

    private void ShowHierarchy()
    {
        if (!IsTemplatePrefabInHierarchy(m_UITemplate.gameObject))
        {
            Canvas canvas = GameObject.FindObjectOfType<Canvas>();
			if (canvas != null)
            {
				if((canvas.transform.childCount == 0) ||
				 (canvas.transform.childCount == 1 && canvas.transform.GetChild((0)).GetComponent<UITemplate>() != null)){
					ClearHierarchy();
	                GameObject go = PrefabUtility.InstantiatePrefab(m_UITemplate.gameObject) as GameObject;
	                go.name = m_UITemplate.gameObject.name;

					GameObjectUtility.SetParentAndAlign(go, canvas.gameObject);
	                EditorGUIUtility.PingObject(go);
				}
            }
        }
    }

    static private bool IsTemplatePrefabInHierarchy(GameObject go)
    {
        return (PrefabUtility.GetPrefabParent(go) != null);
    }

    static private bool IsTemplatePrefabInInProjectView(GameObject go)
    {
        string path = AssetDatabase.GetAssetPath(go);
        if (!string.IsNullOrEmpty(path))
        {
            for (int i = 0; i < m_TemplatePath.Length; i++)
            {
                if (path.Contains(m_TemplatePath[i]))
                    return true;
            }
        }
        return false;
    }

	static private DirectoryInfo CreatDirectory()
	{
		DirectoryInfo directiory = new DirectoryInfo(Application.dataPath + "/" + TEMPLATE_PREFAB_PATH.Replace("Assets/",""));
		if(!directiory.Exists)
		{
			directiory.Create();
			Refresh();
		}
		return directiory;
   	}

    static private string GetPrefabPath(GameObject prefab)
    {
        Object prefabObj = PrefabUtility.GetPrefabParent(prefab);
        if (prefabObj != null)
        {
            return AssetDatabase.GetAssetPath(prefabObj);
        }
        return null;
    }

}
