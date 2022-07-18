
using UnityEngine;
using VRC.SDK3.Avatars.ScriptableObjects;
using VRC.SDK3.Avatars.Components;


// hi, not a lot here huh...

namespace GabSith.PasswordGen
{
    //[ExecuteAlways]
    public class Password : MonoBehaviour
    {
        public VRCAvatarDescriptor descriptor;
        public VRCExpressionsMenu passwordBaseMenu;

        public AnimationClip lockAnimation;

        public int menuInt = 0;
        public int passwordToEdit = 123456;
        
        public bool passwordToggle = false;
        public bool passwordRequirements = true;
        public bool hasPasswordMenu = false;
        public bool useWriteDefaults = false;
        public bool makeBackup = true;
        public bool extrasFold = false;
        public bool useLockAnimation = false;
        public bool alsoLockToggles = true;

        public void DestroyScriptInstance()
        {
            Password inst = GetComponent<Password>();
            DestroyImmediate(inst);
        }

    }
}




