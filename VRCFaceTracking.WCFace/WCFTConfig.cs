namespace VRCFaceTracking.WCFace;

/* Copied from run.bat
 * @ECHO OFF

echo Entering nothing in the following prompts will use the default option.


facetrackerNeos -l 1

echo Make sure that nothing is accessing your camera before you proceed.

set cameraNum=0
set /p cameraNum=Enter the number of the camera you'd like to use from the list above: 
echo.

facetrackerNeos -a %cameraNum%

set dcaps=-1
set /p dcaps=Enter the number of your preferred resolution or press enter for default settings. A lower resolution runs faster:
echo.

echo Tracking model options.
echo Higher numbers generally offer better tracking quality, but slower speed.
echo.
echo -1: Poorest quality tracking but great performance, blinking is disabled.
echo  0: Blinking is enabled in the following models.
echo  1: Better quality than 0, but slower speed.
echo  2: Better quality than 1, but slower speed.
echo  3: Best tracking quality of all the models, but also the slowest model.
echo  4: Like 3, but optimized for wink detection.
echo -3: Quality is between -1 and 0.
echo -2: Quality is roughly like 1, but is faster. Recommended unless you want better tracking accuracy, in which case use model 2 or 3.

set model=-2
set /p model=Select the tracking model (default -2): 
echo.

set smootht=0
set /p smootht=Enter 1 if you'd like the position data to be smoothed. (default 0): 
echo.

set smoothr=0
set /p smoothr=Enter 1 if you'd like the rotation data to be smoothed. (default 0): 
echo.

start facetrackerNeos -c %cameraNum% -D %dcaps% --model %model% --smooth-translation %smootht% --smooth-rotation %smoothr%
 */
internal class WCFTConfig
{
    public int cameraNum;
    public int resolution;
    public int model;
    public int smootht;
    public int smoothr;

    public WCFTConfig()
    {
        cameraNum = 0;
        resolution = -1;
        model = 4;
        smootht = 1;
        smoothr = 1;
    }
}
