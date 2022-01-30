using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using VRC.SDKBase;

namespace VRCFaceTracking
{
    public static class Hooking
    {
        private static AvatarInstantiatedDelegate _onAvatarInstantiatedDelegate;


        public static unsafe void SetupHooking()
        {
            try
            {
                var intPtr = (IntPtr) typeof(VRCAvatarManager.AvatarCreationCallback)
                    .GetField(
                        "NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_GameObject_VRC_AvatarDescriptor_Boolean_0",
                        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                MelonUtils.NativeHookAttach(intPtr, typeof(Hooking).GetMethod(nameof(OnAvatarInstantiated), BindingFlags.Static | BindingFlags.NonPublic).MethodHandle
                        .GetFunctionPointer());
                _onAvatarInstantiatedDelegate =
                    Marshal.GetDelegateForFunctionPointer<AvatarInstantiatedDelegate>(*(IntPtr*) (void*) intPtr);
            }
            catch (Exception ex)
            {
                MelonLogger.Error("Patch Failed " + ex);
            }
        }

        private static void OnAvatarInstantiated(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded, IntPtr methodInfo)
        {
            _onAvatarInstantiatedDelegate(@this, avatarPtr, avatarDescriptorPtr, loaded, methodInfo);
            try
            {
                var avatarDescriptor = new VRC_AvatarDescriptor(avatarDescriptorPtr);
                
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0
                        ?.prop_VRCAvatarManager_0?.prop_VRCAvatarDescriptor_0 == null   // Is our current avatar null?
                    || avatarDescriptor != VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0
                        .prop_VRCAvatarDescriptor_0)    // Is this avatar descriptor being assigned to our local player?
                {
                    //TODO: Add nameplate badge to indicate supported tracking types
                    //var av3Descriptor = avatarDescriptor.TryCast<VRCAvatarDescriptor>();
                    //if (!av3Descriptor) return;
                    
                    //var (supportsEye, supportsLip) = UnifiedLibManager.GetAvatarSupportedTracking(av3Descriptor);
                    //MelonLogger.Msg($"Player {avatarDescriptor.transform.parent.parent.GetComponent<VRCPlayer>().prop_String_0} : Lip {supportsLip}, Eye{supportsEye}");
                }
                else
                    foreach (var allParameter in UnifiedTrackingData.AllParameters)
                    {
                        allParameter.ZeroParam();
                        allParameter.ResetParam();
                    }
            }
            catch (Exception e) { MelonLogger.Error(e.ToString()); }
        }

        private delegate void AvatarInstantiatedDelegate(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded, IntPtr methodInfo);
    }
}