# 👀 VRCFaceTracking

Provides real eye tracking and lip tracking in VRChat via the HTC Vive Pro Eye's SRanipal SDK. This MelonLoader mod modifies Avatar V3 Parameters according to data returned by the HMD's Eye Trackers and/or Lip Tracker.

## 🎥 Demo

[![](https://i.imgur.com/TKpyFVs.gif)](https://www.youtube.com/watch?v=KbbfYW-hnMk)

https://www.youtube.com/watch?v=KbbfYW-hnMk


## 💾 Installation

As is standard, just drag the `VRCFaceTracking.dll` into your mods folder!

## 🛠 Avatar Setup

For this mod to work, you'll need to be using an avatar with the correct parameters. The system is designed to control your avatar's eyes and lips via simple blend states but what the parameters control is completely up to you. The mod searches for the following float parameters to modify:

### Eye Parameters

|Parameter Name (**Case Sensitive**)|Description|Eye|
|---------|-----------|---|
|`EyesX`|Gaze Direction X|Combined|
|`EyesY`|Gaze Direction Y|Combined|
|`LeftEyeLid`|Eyelid Open|Left|
|`RightEyeLid`|Eyelid Open|Right|
|`CombinedEyeLid`|Eyelid Open|Combined|
|`EyesWiden`|Eye Widen|Combined|
|`EyesDilation`|Pupil Dilation|Combined|
|`EyesSqueeze`|Eyelid Squeeze|Combined|
|`LeftEyeX`|Gaze Direction X|Left|
|`LeftEyeY`|Gaze Direction Y|Left|
|`RightEyeX`|Gaze Direction X|Right|
|`RightEyeY`|Gaze Direction Y|Right|
|`LeftEyeWiden`|Eye Widen|Left|
|`RightEyeWiden`|Eye Widen|Right|
|`LeftEyeSqueeze`|Eyelid Squeeze|Left|
|`RightEyeSqueeze`|Eyelid Squeeze|Right|
|`LeftEyeLidExpanded`|0.0-0.8 Eyelid Open. 0.8-1.0 Eye Widen|Left|
|`RightEyeLidExpanded`|0.0-0.8 Eyelid Open. 0.8-1.0 Eye Widen|Right|

It's not required to use all of these parameters. Similar to the setup of parameters with Unity Animation Controllers, these are all case-sensitive and must be copied **EXACTLY** as shown into your Avatar's base parameters. A typical setup might look something like this:<br>
![](https://i.imgur.com/KZRweT7.png)

**Please make sure you disable the built in simulated eye tracking in your avatar descriptor**. This will almost certainly mess with things if left on. Personally, I've also had some issues with blink blendshapes being overrided by my gesture layer so if you can see your eyes fine but others see them half closed, I would reccomend removing your Additive layer so the default is not applied. It should say "None (Runtime Animator Controller)" if it's removed correctly.

Feel free to [consult the wiki](https://github.com/benaclejames/VRCFaceTracking/wiki/Eye-Tracking-Setup) for a setup guide and more info as to what each parameter does

### Lip Parameters

There are a large number of parameters you can use for lip tracking. Consult the wiki for more information about what they do: [Avatar Setup](https://github.com/benaclejames/VRCFaceTracking/wiki/Lip-Tracking-Setup).

You can also refer to this visual guide from NeosVR on what the following API parameters do: https://casuallydotcat.wordpress.com/2020/02/10/the-ultimate-neos-blend-shape-guide-february-2020/

|Parameter Name (**Case Sensitive**)|Description|Range|
|---|---|---|
|`JawRight`|Jaw translation right|0.0 - 1.0|
|`JawLeft`|Jaw translation left|0.0 - 1.0|
|`JawForward`|Jaw translation jutting out|0.0 - 1.0|
|`JawOpen`|Jaw open|0.0 - 1.0|
|`MouthApeShape`|Jaw open, lips sealed closed|0.0 - 1.0|
|`MouthUpperRight`|Upper lip translate right, and not showing teeth|0.0 - 1.0|
|`MouthUpperLeft`|Upper lip translate left, and not showing teeth|0.0 - 1.0|
|`MouthLowerRight`|Lower lip translate right|0.0 - 1.0|
|`MouthLowerLeft`|Lower lip translate left|0.0 - 1.0|
|`MouthUpperOverturn`|Pushing top lip out|0.0 - 1.0|
|`MouthLowerOverturn`|Pouting out lower lip|0.0 - 1.0|
|`MouthPout`|Both lips pouting forward|0.0 - 1.0|
|`MouthSmileRight`|Smile right<sup>1</sup>|0.0 - 1.0|
|`MouthSmileLeft`|Smile left<sup>1</sup>|0.0 - 1.0|
|`MouthSadRight`|Sad Right<sup>1</sup>|0.0 - 1.0|
|`MouthSadLeft`|Sad Left<sup>1</sup>|0.0 - 1.0|
|`CheekPuffRight`|Cheek puffed out, right|0.0 - 1.0|
|`CheekPuffLeft`|Cheek puffed out, left|0.0 - 1.0|
|`CheekSuck`|Both cheeks sucked in|0.0 - 1.0|
|`MouthUpperUpRight`|Upper right lip drawn up to show teeth|0.0 - 1.0|
|`MouthUpperUpLeft`|Upper left lip drawn up to show teeth|0.0 - 1.0|
|`MouthLowerDownRight`|Bottom right lip drawn down to show teeth|0.0 - 1.0|
|`MouthLowerDownLeft`|Bottom left lip drawn down to show teeth|0.0 - 1.0|
|`MouthUpperInside`|Upper lip bitten by lower teeth|0.0 - 1.0|
|`MouthLowerInside`|Bottom lip bitten by upper teeth|0.0 - 1.0|
|`MouthLowerOverlay`|Upper lip out and over lower|0.0 - 1.0|
|`TongueLongStep1`|Seems to be an intermediate out|0.0 - 1.0|
|`TongueLongStep2`|Seems to be an intermediate out|0.0 - 1.0|
|`TongueDown`|Tongue tip angled down|0.0 - 1.0|
|`TongueUp`|Tongue tip angled up|0.0 - 1.0|
|`TongueRight`|Tongue tip angled right|0.0 - 1.0|
|`TongueLeft`|Tongue tip angled left|0.0 - 1.0|
|`TongueRoll`|Both sides of tongue brought up into "v"|0.0 - 1.0|
|`TongueUpLeftMorph`|Seems to deform upper left of tongue out of mouth|0.0 - 1.0|
|`TongueUpRightMorph`|Seems to deform upper right of tongue out of mouth|0.0 - 1.0|
|`TongueDownLeftMorph`|Seems to deform lower left of tongue out of mouth |0.0 - 1.0|
|`TongueDownRightMorph`|Seems to deform lower right of tongue out of mouth |0.0 - 1.0|

Additionally, the mod provides computed parameters that combine some of the above parameters, to save space:

|Parameter Name (**Case Sensitive**)|Description|Range|
|--|--|--|
|`JawX`|Jaw translation fully left to fully right|-1.0 - 1.0|
|`MouthUpper`|Top lip deflection (MouthUpperLeft/Right) full left to full right, with 0 being neutral|-1.0 - 1.0|
|`MouthLower`|Bottom lip deflection (MouthLowerLeft/Right) full left to full right, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUp`|Bottom lip deflection **with teeth** (MouthUpperUpLeft/Right) full left to full right, with 0 being neutral<sup>2</sup>|-1.0 - 1.0|
|`MouthLowerDown`|Bottom lip deflection **with teeth** (MouthLowerDownLeft/Right) full left to full right, with 0 being neutral<sup>2</sup>|-1.0 - 1.0|
|`SmileSadRight`|Left side full smile or full sad, with 0 being neutral<sup>1</sup>|-1.0 - 1.0|
|`SmileSadLeft`|Left side full smile or full sad, with 0 being neutral<sup>1</sup>|-1.0 - 1.0|
|`TongueY`|Tongue tip fully up to fully down, with 0 being neutral|-1.0 - 1.0|
|`TongueX`|Tongue tip fully left to fully right, with 0 being neutral|-1.0 - 1.0|

<sup>1</sup> **Note:** These computed parameters don't seem to work well in practice, at least in my experience (-InconsolableCellist)

<sup>2</sup> **Note:** **TODO:** Not yet implemented 5/4/21

### Setup Guide

The wiki contains a full avatar setup guide: [wiki](https://github.com/benaclejames/VRCFaceTracking/wiki).

## 📜 Disclaimer

As with all VRChat mods, modifying the game client can result in account termination, be it temporary or permanent. While this mod doesn't ruin the experience for others, using it may still be a bannable offence.<br>
**USE AT YOUR OWN RISK**. I will not be held responsible for any punishments you may recieve for using this mod.

## 👋 Credits

* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
* [VIVE](https://www.vive.com/) for the SRanipal SDK and their awesome hardware! ❤

![](https://i.imgur.com/WXcncO9.jpeg)
