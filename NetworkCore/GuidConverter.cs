
namespace TopNetwork.Core
{
    public static class GuidConverter
    {
        public static Guid IntToGuid(int value)
        {
            byte[] bytes = new byte[16]; 
            BitConverter.GetBytes(value).CopyTo(bytes, 0); 
            return new Guid(bytes);
        }
        public static int GuidToInt(Guid guid)
        {
            byte[] bytes = guid.ToByteArray();
            return BitConverter.ToInt32(bytes, 0);
        }
    }
}
