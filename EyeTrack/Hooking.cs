using System;
using System.Reflection;
using System.Runtime.InteropServices;
using MelonLoader;
using VRC.Core;
using VRC.SDKBase;

namespace VRCEyeTracking
{
    public class Hooking
    {
        private static AvatarInstantiatedDelegate onAvatarInstantiatedDelegate;
        private static OnAvatarSwitchDelegate avatarSwitch;


        public static unsafe void SetupHooking()
        {
            try
            {
                var intPtr = (IntPtr) typeof(VRCAvatarManager.MulticastDelegateNPublicSealedVoGaVRBoUnique)
                    .GetField(
                        "NativeMethodInfoPtr_Invoke_Public_Virtual_New_Void_GameObject_VRC_AvatarDescriptor_Boolean_0",
                        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Imports.Hook(intPtr,
                    new Action<IntPtr, IntPtr, IntPtr, bool>(OnAvatarInstantiated).Method.MethodHandle
                        .GetFunctionPointer());
                onAvatarInstantiatedDelegate =
                    Marshal.GetDelegateForFunctionPointer<AvatarInstantiatedDelegate>(*(IntPtr*) (void*) intPtr);

                intPtr = (IntPtr) typeof(VRCAvatarManager)
                    .GetField(
                        "NativeMethodInfoPtr_Method_Public_Boolean_ApiAvatar_String_Single_MulticastDelegateNPublicSealedVoGaVRBoUnique_0",
                        BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
                Imports.Hook(intPtr,
                    new Action<IntPtr, IntPtr, string, float, IntPtr>(OnAvatarSwitch).Method.MethodHandle
                        .GetFunctionPointer());
                avatarSwitch = Marshal.GetDelegateForFunctionPointer<OnAvatarSwitchDelegate>(*(IntPtr*) (void*) intPtr);
            }
            catch (Exception ex)
            {
                MelonLogger.Msg("Patch Failed " + ex);
            }
        }

        private static void OnAvatarSwitch(IntPtr @this, IntPtr test1, string string1, float float1, IntPtr ptr1)
        {
            try
            {
                if (test1 != IntPtr.Zero)
                {
                    var avatar = new ApiAvatar(test1);
                    if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_ApiAvatar_0?.Pointer != IntPtr.Zero &&
                        avatar.Pointer != IntPtr.Zero && avatar.Pointer ==
                        VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_ApiAvatar_0?.Pointer)
                        MainMod.eyeTrackParams = MainMod.EmptyList();
                }
            }
            catch (Exception e)
            {
                MelonLogger.Error("Error on Avatar Switch: " + e);
            }

            avatarSwitch(@this, test1, string1, float1, ptr1);
        }

        private static void OnAvatarInstantiated(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded)
        {
            onAvatarInstantiatedDelegate(@this, avatarPtr, avatarDescriptorPtr, true);
            try
            {
                var avatarDescriptor = new VRC_AvatarDescriptor(avatarDescriptorPtr);
                if (VRCPlayer.field_Internal_Static_VRCPlayer_0?.prop_VRCAvatarManager_0
                        ?.prop_VRCAvatarDescriptor_0 !=
                    null && avatarDescriptor == VRCPlayer.field_Internal_Static_VRCPlayer_0.prop_VRCAvatarManager_0
                        .prop_VRCAvatarDescriptor_0)
                    MainMod.ScanForParamEnums();
            }
            catch (Exception e)
            {
                MelonLogger.Error(e.ToString());
            }
        }

        private delegate void AvatarInstantiatedDelegate(IntPtr @this, IntPtr avatarPtr, IntPtr avatarDescriptorPtr,
            bool loaded);


        private delegate void OnAvatarSwitchDelegate(IntPtr @this, IntPtr test1, string string1, float float1,
            IntPtr ptr1);
    }
}