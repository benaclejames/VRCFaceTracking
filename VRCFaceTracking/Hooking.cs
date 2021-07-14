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
                var intPtr = (IntPtr) typeof(VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique)
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
            bool loaded)
        {
            _onAvatarInstantiatedDelegate(@this, avatarPtr, avatarDescriptorPtr, loaded);
            try
            {
                var avatarDescriptor = new VRC_AvatarDescriptor(avatarDescriptorPtr);
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0
                    ?.prop_VRCAvatarDescriptor_0 == null || avatarDescriptor != VRCPlayer
                    .field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0
                    .prop_VRCAvatarDescriptor_0) return;
                
                MainMod.ZeroParams();
                MainMod.ResetParams();
            }
            catch (Exception e) { MelonLogger.Error(e.ToString()); }
        }

        private delegate void AvatarInstantiatedDelegate(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded);
    }
}