using System;

namespace Brahma.DirectX
{
    public static class output
    {
        public static int Current
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
            }
        }
        
        public static int CurrentX
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
            }
        }
        
        public static int CurrentY
        {
            get
            {
                throw new InvalidOperationException("This value can never be accessed");
            }
        }
    }
}