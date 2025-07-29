using System.Collections;
using System.Collections.Generic;
using NaughtyAttributes;
using UnityEngine;
using VaroniaBackOffice;

public class AddonsLoader : MonoBehaviour
{
    [SerializeField]
    public List<Addon> addons = new List<Addon>();

    [SerializeField]
    public bool loadOnStart = true;
    
    
    void Start()
    {
#if UNITY_EDITOR
        if(loadOnStart)
#endif
        GetComponent<VaroniaGlobal>().OnInitialized.AddListener(LoadAddons);
    }
    
  
      public void LoadAddons()
      {
         foreach (var item in addons)
         {
             
             var A = Instantiate(item.prefab, this.transform);
             A.name = A.name.Replace("(Clone)", "");
             
             var configurable = A.GetComponent<IAddonConfigurable>();
             if (configurable != null)
             {
                 configurable.ApplyScriptableConfig(item.config);
             }
         
         }
     }
    
}
