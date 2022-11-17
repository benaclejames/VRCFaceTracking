namespace VRCFaceTracking
{
    /// <summary>Structure that holds all necessary information for an image</summary>
    public class Image
    {
        /// <summary>
        /// Tuple that represents the horizontal and vertical size of the image (in pixels).
        /// </summary>
        public (int x, int y) ImageSize;

        /// <summary>
        /// Byte that contains the image's data.
        /// </summary>
        public byte[] ImageData;

        /// <summary>
        /// Used to let VRCFaceTracking know if an image is able to be supported.
        /// </summary>
        public bool SupportsImage;
    }
}
