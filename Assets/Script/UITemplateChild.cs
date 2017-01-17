using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class UITemplateChild : UITemplate {

#if UNITY_EDITOR
    [HideInInspector]
    public int m_PartGUID = 0;

    public void InitGUID(int partGUID)
    {
        if (m_GUID == 0)
        {
            m_PartGUID = partGUID;
            m_GUID = partGUID - Random.Range(int.MinValue, int.MaxValue);
        }
    }
#endif
}
