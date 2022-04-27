using System;
using System.Runtime.InteropServices;
using MelonLoader;
using UnhollowerBaseLib;
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
                var originalMethodPtr = *(IntPtr*)(IntPtr)UnhollowerUtils.GetIl2CppMethodInfoPointerFieldForGeneratedMethod(
                    typeof(VRCAvatarManager.AvatarCreationCallback)
                        .GetMethod("Invoke")).GetValue(null);
                var detourDelegate = Marshal.GetFunctionPointerForDelegate<AvatarInstantiatedDelegate>(OnAvatarInstantiated);
                MelonUtils.NativeHookAttach((IntPtr)(&originalMethodPtr), detourDelegate);
                _onAvatarInstantiatedDelegate =
                    Marshal.GetDelegateForFunctionPointer<AvatarInstantiatedDelegate>(originalMethodPtr);
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
                        ?.prop_VRCAvatarManager_0?.prop_VRCAvatarDescriptor_0 == null // Is our current avatar null?
                    || avatarDescriptor != VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0
                        .prop_VRCAvatarDescriptor_0) // Is this avatar descriptor being assigned to our local player?
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
            catch (Exception e)
            {
                MelonLogger.Error(e.ToString());
            }
        }
        
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate void AvatarInstantiatedDelegate(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded, IntPtr methodInfo);
    }
}