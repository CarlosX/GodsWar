using System.Security.Cryptography;

namespace Framework.Cryptography
{
    public class SessionKeyGenerator
    {
        public SessionKeyGenerator(byte[] buff, int size)
        {
            int halfSize = size / 2;

            sh = SHA256.Create();
            sh.TransformFinalBlock(buff, 0, halfSize);
            o1 = sh.Hash;

            sh.Initialize();
            sh.TransformFinalBlock(buff, halfSize, size - halfSize);
            o2 = sh.Hash;

            FillUp();
        }

        public void Generate(byte[] buf, uint sz)
        {
            for (uint i = 0; i < sz; ++i)
            {
                if (taken == 32)
                    FillUp();

                buf[i] = o0[taken];
                taken++;
            }
        }

        void FillUp()
        {
            sh.Initialize();
            sh.TransformBlock(o1, 0, 32, o1, 0);
            sh.TransformBlock(o0, 0, 32, o0, 0);
            sh.TransformFinalBlock(o2, 0, 32);
            o0 = sh.Hash;

            taken = 0;
        }

        SHA256 sh;
        uint taken;
        byte[] o0 = new byte[32];
        byte[] o1 = new byte[32];
        byte[] o2 = new byte[32];
    }
}
