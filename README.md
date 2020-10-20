# 👀 VRCEyeTracking

Provides real eye tracking in VRChat via the HTC Vive Pro Eye's eye tracking SDK. This MelonLoader mod modifies Avatar V3 Parameters according to data returned by the HMD's Eye Trackers. 

## 💾 Installation

Installation of the mod may be slightly different to most MelonLoader mods due to SRanipal's requirement for additional dependency DLLs.
**The DLLs included in the "Extras" folder of the zip need to be in the same folder as VRChat.exe**<br>
As standard, the main "EyeTrack.dll" file will need to be put in your Mods folder

## 🛠 Avatar Setup

For this mod to work, you'll need to be using an avatar with the correct parameters. The system is designed to control your avatar's eyes via simple blend states but what the parameters control is completely up to you. The mod searches for the following float parameters to modify:

|Parameter Name (**Case Sensitive**)|Description|Eye|
|---------|-----------|---|
|`EyesX`|Gaze Direction X|Combined|
|`EyesY`|Gaze Direction Y|Combined|
|`LeftEyeLid`|Eyelid Open|Left|
|`RightEyeLid`|Eyelid Open|Right|
|`EyesWiden`|Eye Widen|Combined|
|`EyesDilation`|Pupil Dilation|Combined|
|`LeftEyeX`|Gaze Direction X|Left|
|`LeftEyeY`|Gaze Direction Y|Left|
|`RightEyeX`|Gaze Direction X|Right|
|`RightEyeY`|Gaze Direction Y|Right|
|`LeftEyeWiden`|Eye Widen|Left|
|`RightEyeWiden`|Eye Widen|Right|

It's not required to use all of these parameters. Similar to the setup of parameters with Unity Animation Controllers, these are all case-sensitive and must be copied **EXACTLY** as shown into your Avatar's base parameters. A typical setup might look something like this:<br>
![](https://i.imgur.com/KZRweT7.png)

**Please make sure you disable the built in simulated eye tracking in your avatar descriptor**. This will almost certainly mess with things if left on. Personally, I've also had some issues with blink blendshapes being overrided by my gesture layer so if you can see your eyes fine but others see them half closed, I would reccomend removing your Additive layer so the default is not applied. It should say "None (Runtime Animator Controller)" if it's removed correctly.

## 📜 Disclaimer

As with all VRChat mods, modifying the game client can result in account termination, be it temporary or permanent. While this mod doesn't ruin the experience for others, using it may still be a bannable offence.<br>
**USE AT YOUR OWN RISK**. I will not be held responsible for any punishments you may recieve for using this mod.

## 👋 Credits

* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
* [VIVE](https://www.vive.com/) for the SRanipal SDK and their awesome hardware! ❤
