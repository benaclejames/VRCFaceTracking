using System;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace VRCFaceTracking.Core.Validation;

public class ValidIpAddressAttribute : ValidationAttribute
{
    public override bool IsValid(object value)
    {
        if (value is string ipString)
        {

            // Check for empty string
            if (string.IsNullOrWhiteSpace(ipString))
            {
                return false;
            }

            // Attempt to parse the IP address
            return IPAddress.TryParse(ipString, out _);
        }

        return false; // Return false if the value is not a string
    }

    public override string FormatErrorMessage(string name)
    {
        return $"{name} must be a valid IP address.";
    }
}
