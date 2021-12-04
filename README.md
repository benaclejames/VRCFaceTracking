# 👀 VRCFaceTracking

Provides real eye tracking and lip tracking in VRChat via the HTC Vive Pro Eye's SRanipal SDK. This MelonLoader mod modifies Avatar V3 Parameters according to data returned by the HMD's Eye Trackers and/or Lip Tracker.

## 🎥 Demo

[![](https://i.imgur.com/TKpyFVs.gif)](https://www.youtube.com/watch?v=KbbfYW-hnMk)

https://www.youtube.com/watch?v=KbbfYW-hnMk


## 💾 Installation

As is standard, just drag the `VRCFaceTracking.dll` into your mods folder!

## 🛠 Avatar Setup

For this mod to work, you'll need to be using an avatar with the correct parameters. The system is designed to control your avatar's eyes and lips via simple blend states but what the parameters control is completely up to you. The mod searches for the following float parameters to modify:

### Quick Setup Guide

The wiki contains a full avatar setup guide: [wiki](https://github.com/benaclejames/VRCFaceTracking/wiki).

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

## Space Saving Advance Parameters

Additionally, the mod provides computed parameters that combine some of the parameters, to save space:

### Combined Lip Parameters

|Parameter Name (**Case Sensitive**)|Description|Range|
|--|--|--|
|`JawX`|Jaw translation fully left to fully right|-1.0 - 1.0|
|`MouthUpper`|MouthUpperLeft to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLower`|MouthLowerLeft to MouthLowerRight, with 0 being neutral|-1.0 - 1.0|
|`SmileSadRight`|MouthSadRight to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileSadLeft`|MouthSadLeft to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|
|`TongueY`|TongueDown to TongueUp, with 0 being neutral|-1.0 - 1.0|
|`TongueX`|TongueLeft to TongueRight, with 0 being neutral|-1.0 - 1.0|
|`PuffSuckRight`|CheekSuck to CheekPuffRight, with 0 being neutral|-1.0 - 1.0|
|`PuffSuckLeft`|CheekSuck to CheekPuffLeft, with 0 being neutral|-1.0 - 1.0|
|`JawOpenApe`|MouthApeShape to JawOpen, with 0 being neutral|-1.0 - 1.0|
|`JawOpenPuffRight`|CheekPuffRight to JawOpen, with 0 being neutral|-1.0 - 1.0|
|`JawOpenPuffLeft`|CheekPuffLeft to JawOpen, with 0 being neutral|-1.0 - 1.0|
|`JawOpenSuck`|CheekSuck to JawOpen, with 0 being neutral|-1.0 - 1.0|
|`JawOpenForward`|JawForward to JawOpen, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpRightUpperInside`|MouthUpperInside to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpRightPuffRight`|CheekPuffRight to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpRightApe`|MouthApeShape to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpRightPout`|MouthPout to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpRightOverlay`|MouthLowerOverlay Shape to MouthUpperRight, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpLeftUpperInside`|MouthUpperInside to MouthUpperLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpLeftPuffLeft`|CheekPuffLeft to MouthUpperLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpLeftApe`|MouthApeShape to MouthUpperLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpLeftPout`|MouthPout to MouthUpperLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthUpperUpLeftOverlay`|MouthLowerOverlay Shape to MouthUpperLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownRightLowerInside`|MouthLowerInside to MouthLowerDownRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownRightPuffRight`|CheekPuffRight to MouthLowerDownRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownRightApe`|MouthApeShape to MouthLowerDownRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownRightPout`|MouthPout to MouthLowerDownRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownRightOverlay`|MouthLowerOverlay Shape to MouthLowerDownRight, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownLeftLowerInside`|MouthLowerInside to MouthLowerDownLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownLeftPuffLeft`|CheekPuffLeft to MouthLowerDownLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownLeftApe`|MouthApeShape to MouthLowerDownLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownLeftPout`|MouthPout to MouthLowerDownLeft, with 0 being neutral|-1.0 - 1.0|
|`MouthLowerDownLeftOverlay`|MouthLowerOverlay Shape to MouthLowerDownLeft, with 0 being neutral|-1.0 - 1.0|
|`SmileRightUpperOverturn`|MouthUpperOverturn to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileRightLowerOverturn`|MouthLowerOverturn to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileRightApe`|MouthApeShape to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileRightOverlay`|MouthLowerOverlay to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileRightPout`|MouthPout to MouthSmileRight, with 0 being neutral|-1.0 - 1.0|
|`SmileLeftUpperOverturn`|MouthUpperOverturn to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|
|`SmileLeftLowerOverturn`|MouthLowerOverturn to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|
|`SmileLeftApe`|MouthApeShape to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|
|`SmileLeftOverlay`|MouthLowerOverlay to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|
|`SmileLeftPout`|MouthPout to MouthSmileLeft, with 0 being neutral|-1.0 - 1.0|

### Binary Eye Parameters (Bools)

Takes float value for eye lid control and converts to binary number system. 

![](https://i.imgur.com/VrvDczV.png)

|Parameter Name (**Case Sensitive**)|Description|
|--|--|
|`LeftEyeLid1`|LeftEyeLid 2<sup>0</sup>|
|`LeftEyeLid2`|LeftEyeLid 2<sup>1</sup>|
|`LeftEyeLid4`|LeftEyeLid 2<sup>2</sup>|
|`LeftEyeLid8`|LeftEyeLid 2<sup>3</sup>|
|`RightEyeLid1`|RightEyeLid 2<sup>0</sup>|
|`RightEyeLid2`|RightEyeLid 2<sup>1</sup>|
|`RightEyeLid4`|RightEyeLid 2<sup>2</sup>|
|`RightEyeLid8`|RightEyeLid 2<sup>3</sup>|
|`CombinedLid1`|CombinedEyeLid 2<sup>0</sup>|
|`CombinedLid2`|CombinedEyeLid 2<sup>1</sup>|
|`CombinedLid4`|CombinedEyeLid  2<sup>3</sup>|
|`CombinedLid8`|CombinedEyeLid  2<sup>3</sup>|
|`LeftEyeLidExpanded1`|LeftEyeLidExpanded 2<sup>0</sup>|
|`LeftEyeLidExpanded2`|LeftEyeLidExpanded 2<sup>1</sup>|
|`LeftEyeLidExpanded4`|LeftEyeLidExpanded 2<sup>2</sup>|
|`LeftEyeLidExpanded8`|LeftEyeLidExpanded 2<sup>3</sup>|
|`LeftEyeWidenToggle`|LeftEyeWiden Widen State|
|`RightEyeLidExpanded1`|RightEyeLidExpanded 2<sup>0</sup>|
|`RightEyeLidExpanded2`|RightEyeLidExpanded 2<sup>1</sup>|
|`RightEyeLidExpanded4`|RightEyeLidExpanded 2<sup>2</sup>|
|`RightEyeLidExpanded8`|RightEyeLidExpanded 2<sup>3</sup>|
|`RightEyeWidenToggle`|RightEyeWiden Widen State|
|`CombinedEyeLidExpanded1`|CombinedEyeLidExpanded 2<sup>0</sup>|
|`CombinedEyeLidExpanded2`|CombinedEyeLidExpanded 2<sup>1</sup>|
|`CombinedEyeLidExpanded4`|CombinedEyeLidExpanded 2<sup>2</sup>|
|`CombinedEyeLidExpanded8`|CombinedEyeLidExpanded 2<sup>3</sup>|
|`CombinedEyeWidenToggle`|CombinedEyeWiden Widen State|
|`LeftEyeLidExpandedSqueeze1`|LeftEyeLidExpandedSqueeze 2<sup>0</sup>|
|`LeftEyeLidExpandedSqueeze2`|LeftEyeLidExpandedSqueeze 2<sup>1</sup>|
|`LeftEyeLidExpandedSqueeze4`|LeftEyeLidExpandedSqueeze 2<sup>2</sup>|
|`LeftEyeLidExpandedSqueeze8`|LeftEyeLidExpandedSqueeze 2<sup>3</sup>|
|`LeftEyeSqueezeToggle`|LeftEyeSqueeze Squeeze State|
|`RightEyeLidExpandedSqueeze1`|RightEyeLidExpandedSqueeze 2<sup>0</sup>|
|`RightEyeLidExpandedSqueeze2`|RightEyeLidExpandedSqueeze 2<sup>1</sup>|
|`RightEyeLidExpandedSqueeze4`|RightEyeLidExpandedSqueeze 2<sup>2</sup>|
|`RightEyeLidExpandedSqueeze8`|RightEyeLidExpandedSqueeze 2<sup>3</sup>|
|`RightEyeSqueezeToggle`|RightEyeSqueeze Squeeze State|
|`CombinedEyeLidExpandedSqueeze1`|CombinedEyeLidExpandedSqueeze 2<sup>0</sup>|
|`CombinedEyeLidExpandedSqueeze2`|CombinedEyeLidExpandedSqueeze 2<sup>1</sup>|
|`CombinedEyeLidExpandedSqueeze4`|CombinedEyeLidExpandedSqueeze 2<sup>2</sup>|
|`CombinedEyeLidExpandedSqueeze8`|CombinedEyeLidExpandedSqueeze 2<sup>3</sup>|
|`CombinedEyeSqueezeToggle`|CombinedEyeSqueeze Squeeze State|


## 📜 Disclaimer

As with all VRChat mods, modifying the game client can result in account termination, be it temporary or permanent. While this mod doesn't ruin the experience for others, using it may still be a bannable offence.<br>
**USE AT YOUR OWN RISK**. I will not be held responsible for any punishments you may recieve for using this mod.

## 👋 Credits

* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
* [VIVE](https://www.vive.com/) for the SRanipal SDK and their awesome hardware! ❤

![](https://i.imgur.com/WXcncO9.jpeg)
