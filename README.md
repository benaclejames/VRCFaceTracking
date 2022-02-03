# 👀 VRCFaceTracking

Provides real eye tracking and lip tracking in VRChat via the HTC Vive Pro Eye's SRanipal SDK. This MelonLoader mod modifies Avatar V3 Parameters according to data returned by the HMD's Eye Trackers and/or Lip Tracker.

[![Discord](https://discord.com/api/guilds/849300336128032789/widget.png)](https://discord.gg/Fh4FNehzKn)

## 🎥 Demo

[![](https://i.imgur.com/TKpyFVs.gif)](https://www.youtube.com/watch?v=KbbfYW-hnMk)

https://www.youtube.com/watch?v=KbbfYW-hnMk


## 💾 Installation

As is standard, just drag the `VRCFaceTracking.dll` into your mods folder!

## 🛠 Avatar Setup

For this mod to work, you'll need to be using an avatar with the correct parameters. The system is designed to control your avatar's eyes and lips via simple blend states but what the parameters control is completely up to you. The mod searches for the parameters which can be found in the parameter list in the wiki.

### [List of Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters)

## 👀 [Eye Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#eye)

### [Eye Tracking Setup Guide](https://github.com/benaclejames/VRCFaceTracking/wiki/Eye-Tracking-Setup)

It's not required to use all of these parameters. Similar to the setup of parameters with Unity Animation Controllers, these are all case-sensitive and must be copied **EXACTLY** as shown into your Avatar's base parameters. A typical setup might look something like this:<br>
![](https://i.imgur.com/KZRweT7.png)

**Please make sure you disable the built in simulated eye tracking in your avatar descriptor**. This will almost certainly mess with things if left on. Personally, I've also had some issues with blink blendshapes being overrided by my gesture layer so if you can see your eyes fine but others see them half closed, I would reccomend removing your Additive layer so the default is not applied. It should say "None (Runtime Animator Controller)" if it's removed correctly.

Feel free to [consult the wiki](https://github.com/benaclejames/VRCFaceTracking/wiki/Eye-Tracking-Setup) for a setup guide and more info as to what each parameter does

## :lips: [Lip Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#lip)

There are a large number of parameters you can use for lip tracking. 

### [Lip Tracking Setup Guide](https://github.com/benaclejames/VRCFaceTracking/wiki/Lip-Tracking-Setup) - Basic setup guide

### [Combined Lip Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#combined-lip) - Combined parameters to group mutually exclusive face shapes.

### [Blend Shape Setup](https://github.com/benaclejames/VRCFaceTracking/wiki/Blend-Shapes-Setup) - Reference of standard blend shapes for to be used with facetracking

You can also refer to this visual guide from NeosVR on what the following API parameters do: https://casuallydotcat.wordpress.com/2020/02/10/the-ultimate-neos-blend-shape-guide-february-2020/

## 📜 Disclaimer

As with all VRChat mods, modifying the game client can result in account termination, be it temporary or permanent. While this mod doesn't ruin the experience for others, using it may still be a bannable offence.<br>
**USE AT YOUR OWN RISK**. I will not be held responsible for any punishments you may recieve for using this mod.

## 👋 Credits

* [HerpDerpinstine/MelonLoader](https://github.com/HerpDerpinstine/MelonLoader)
* [VIVE](https://www.vive.com/) for the SRanipal SDK and their awesome hardware! ❤

![](https://i.imgur.com/PkYdCNX.png)
