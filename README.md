# 👀 VRCFaceTracking

Provides real eye tracking and lip tracking in VRChat via the HTC Vive Pro Eye's SRanipal SDK.

[![Discord](https://discord.com/api/guilds/849300336128032789/widget.png)](https://discord.gg/Fh4FNehzKn)

## 🎥 Demo

[![](https://i.imgur.com/TKpyFVs.gif)](https://www.youtube.com/watch?v=KbbfYW-hnMk)

https://www.youtube.com/watch?v=KbbfYW-hnMk

## 🛠 Avatar Setup

For this app to work, you'll need to be using an avatar with the correct parameters or an avatar config file with the correct mappings. The system is designed to control your avatar's eyes and lips via simple blend states but what the parameters control is completely up to you.

### [List of Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters)

## 👀 [Eye Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#eye-tracking-parameters)

### [Eye Tracking Setup Guide](https://github.com/benaclejames/VRCFaceTracking/wiki/Eye-Tracking-Setup)

It's not required to use all of these parameters. Similar to the setup of parameters with Unity Animation Controllers, these are all case-sensitive and must be copied **EXACTLY** as shown into your Avatar's base parameters. A typical setup might look something like this:<br>
![](https://i.imgur.com/KZRweT7.png)

**Please make sure you disable the built in simulated eye tracking in your avatar descriptor**. This will almost certainly mess with things if left on. Personally, I've also had some issues with blink blendshapes being overrided by my gesture layer so if you can see your eyes fine but others see them half closed, I would reccomend removing your Additive layer so the default is not applied. It should say "None (Runtime Animator Controller)" if it's removed correctly.

Feel free to [consult the wiki](https://github.com/benaclejames/VRCFaceTracking/wiki/Eye-Tracking-Setup) for a setup guide and more info as to what each parameter does

## :lips: [Lip Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#lip-tracking-parameters)

There are a large number of parameters you can use for lip tracking. 

### [Lip Tracking Setup Guide](https://github.com/benaclejames/VRCFaceTracking/wiki/Lip-Tracking-Setup) - Basic setup guide

### [Combined Lip Parameters](https://github.com/benaclejames/VRCFaceTracking/wiki/Parameters#combined-lip-parameters) - Combined parameters to group mutually exclusive face shapes.

### [Blend Shape Setup](https://github.com/benaclejames/VRCFaceTracking/wiki/Blend-Shapes-Setup) - Reference of standard blend shapes for to be used with facetracking

You can also refer to this visual guide from NeosVR on what the following API parameters do: https://casuallydotcat.wordpress.com/2020/02/10/the-ultimate-neos-blend-shape-guide-february-2020/

## 👋 Credits

* [VIVE](https://www.vive.com/) for the SRanipal SDK and their awesome hardware! ❤

![](https://i.imgur.com/PkYdCNX.png)
