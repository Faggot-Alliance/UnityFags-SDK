#region
using System.Collections.Generic;
using UnityEngine;
#endregion

namespace Av3Creator.AdvancedToggles
{
    public class Av3TogglesScriptableObject : ScriptableObject
    {
        [SerializeField] public List<Av3AdvancedToggle> ToggleList;
    }
}
