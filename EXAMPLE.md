# Example Avatar Setup

## Parameters

Add the parameters to your VRCExpressionParameters object as described in [README.md](README.md)

In your FX Object, add those same parameters to the `Parameters` tab

## Layers

In the Layers tab in your FX Object, add animation layers corresponding to all the eye controls you want to add. For example, add the following layers: 

* Left Eye Movement

* Right Eye Movement

* Left Eye Blink

* Right Eye Blink

* Pupil Dilation

* Eyes Widen

You can also control both eyes together, which this guide will do for blinking.

Set the layers' weight to `1`

## Blinking

This guide will setup blinking on both eyes by controlling it with just the left one. You can instead repeat the below commands for just left/right eyes to do independent blinking, or use it as the basis of dynamically linking/unlinking the eyes using a gesture or control.

1. Create two new animations: `anim_blink` and `anim_unblink`. These animations should control the blendshape(s) or the bones to fully blink/unblink your avatar's eyes

2. Create a new Blend Tree as the default state in the blinking layer. It should be the only state and Entry should automatically transition to it 

3. Double click the blend tree to open it and make it a "1D" Blend Type. Set the Parameter to "LeftEyeLid" 

4. Add two motions: `anim_blink` and `anim_unblink`. The former should have a threshold closer to 0 and the latter to 1

5. You can keep the threshold values at `0` and `1., or you can experiment to find out what works best for your eyes. These values control at what point the animations begin to blend into one another, and you'll likely want some deadzone so your eyes aren't fluttering. I set it mine to `0.4` and `0.9`.

> **TIP** You can view the current values returned by the eye-tracking API by opening the Debug menu in VRChat and viewing the parameters live.

## Eye Movement

1. Create animation files for each eye or for both eyes. I created:

* `anim_left_eye_left`
* `anim_left_eye_right`
* `anim_left_eye_up`
* `anim_left_eye_down`
* `anim_left_eye_default`

and

* `anim_right_eye_left`
* `anim_right_eye_right`
* `anim_right_eye_up`
* `anim_right_eye_down`
* `anim_right_eye_default`

`anim_[left|right]_eye_default` should have the eye in the neutral position

2. Create a new Blend Tree as the default state in the eye motion layer. Double click it to open it and make it a "2D Freeform Directional" blend tree. Set the parameters to LeftEyeX and LeftEyeY (or RightEyeX/Y)

3. Add the five animations with the following magnitudes: 

|Motion|Pos X|Pos Y|
|---|---|---|
|anim_left_eye_left|-1|0|
|anim_left_eye_right|1|0|
|anim_left_eye_up|0|1|
|anim_left_eye_down|0|-1|
|anim_left_eye_default|0|0|

The idea being to map the correct animations to the correct positions in the map (left, up, right, down, center). Unity will then blend the animations together via 2D integration to produce the properly deformed eye.

I also recommend playing with these values as mentioned above with blinking. I use the following values:

|Motion|Pos X|Pos Y|
|---|---|---|
|anim_left_eye_left|-0.7|0|
|anim_left_eye_right|0.7|0|
|anim_left_eye_up|0|0.7|
|anim_left_eye_down|0|-0.7|
|anim_left_eye_default|0|0|

Repeat the same thing for the right eye, or haver the `anim` clips control both eyes at the same time.

## Other Eye Controls

Pupil dilation, eyes widening, etc., are all setup similar to the above: they either use a "1D" Blend Tree or a "2D Freeform Directional" Blend Tree. In each case you'll need to first create animation clips that manipulate bones or blendshapes to produce the desired effect, then map them to parmeters in an animation layer in your FX object using a Blend Tree state.

I found good pupil dilation values to be:

|Motion|Threshold
|---|---|
|anim_eyes_pupil_constrict|0.38
|anim_eyes_pupil_dilate|0.58

But you may want to use VRChat's Debug window to view your own values.
