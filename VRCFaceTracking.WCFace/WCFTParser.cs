namespace VRCFaceTracking.WCFace;

public static class WCFTParser
{
    public static string[] SplitMessage(string message)
    {
        /*
         * Messages are formatted something like this
         * true/false,[Quaternion],[Vector3],[double; double],[double; double],[double; double],[double; double],double
         * Here's what the parameters are:
         * 0 - Is the face tracking?
         * 1 - Rotation of Head (could be passed to a variable, but not sure it could be used)
         * 2 - Position of Head (could be passed to a variable, but not sure it could be used)
         * 3 - Blink (minimum blink - maximum blink)
         * 4 - Mouth (mouth open, mouth wide)
         * 5 - Brow Up (Brow down Left, Brow down Right)
         * 6 - Gaze Data
         * 7 - EyebrowSteepness (Sad/Angry)
        */
        return message.Split(',');
    }

    public static bool IsMessageValid(string[] splitMessage) => splitMessage.Length == 8;

    public static bool IsMessageValid(string Message) => SplitMessage(Message).Length == 8;

    public static T GetValueFromNeosArray<T>(string input, int index = 0)
    {
        T valueToReturn = default;
        var didContain = false;
        var phase3 = Array.Empty<string>();
        if (input.Contains("[") && input.Contains("]") && input.Contains(";"))
        {
            didContain = true;
            // Removes the [
            var phase1 = input.Split('[');
            // Removes the ]
            var phase2 = phase1[1].Split(']');
            // Removes the ;
            phase3 = phase2[0].Split(';');
            // Iterate through all the indexes and cast them to the input type
        }

        var val = didContain ? phase3[index] : input;

        // Get 0Harmony in here...
        switch (Type.GetTypeCode(typeof(T)))
        {
            case TypeCode.Boolean:
                try { valueToReturn = (T)Convert.ChangeType(val, typeof(T)); } catch { } // Debug($"Failed to cast value: {val} with exception of: {e}"); }
        break;
            case TypeCode.Double:
                try { valueToReturn = (T)Convert.ChangeType(val, typeof(T)); } catch { } // Error($"Failed to cast value: {val} with exception of: {e}"); }
                break;
            default:
                if (typeof(T) == typeof(float))
                {
                    try { valueToReturn = (T) Convert.ChangeType(val, typeof(T)); } catch { } // Error($"Failed to cast value: {val} with exception of: {e}"); }
                    break;
                }
                break;
        }

        return valueToReturn;
    }
}