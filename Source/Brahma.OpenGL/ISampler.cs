namespace Brahma.OpenGL
{
    // Allows us to access OpenGL texture specific properties
    internal interface ISampler
    {
        string SamplingFunction
        {
            get;
        }

        Texture Texture
        {
            get;
        }

        int Width
        {
            get;
        }

        int Height
        {
            get;
        }
    }
}