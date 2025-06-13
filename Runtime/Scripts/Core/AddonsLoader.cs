using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VaroniaBackOffice;

public class AddonsLoader : MonoBehaviour
{
    public List<Addon> addons = new List<Addon>();

    void Start()
    {
        GetComponent<VaroniaGlobal>().OnInitialized.AddListener(LoadAddons);
    }
    
    
      private void LoadAddons()
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
