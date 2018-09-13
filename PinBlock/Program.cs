using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;

namespace PinBlock
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {

                string PB = "";
                string XOR_PIN_PAN = "";
                string PIN = args[0];
                string PAN = args[1];
                string llave = args[2];

                XOR_FORMAT_01(PIN, PAN, out XOR_PIN_PAN);

                XOR_PIN_PAN = XOR_PIN_PAN.Replace(":", "");

                //1234 4766840000704997 AAAABBBBCCCCDDDDEEEEFFFFAAAABBBB -> 501E9A300E5046B4
                var s = getPINBlock( XOR_PIN_PAN, llave );
                System.Console.Write(s);
         
            }
            catch (Exception e)
            {
                
                System.Console.Write( string.Format("Error NO esperado " + e.Message )  );
            }
        
            
        }

        private static bool XOR_FORMAT_01(string PIN, string PAN, out string XOR){
            /*
            Ejemplo de la documentacion thales, la tarjeta  puede ser inferior a 16 caracteres

            PIN   PAN
            92389 4000001234562

            0592 389F FFFF FFFF
            0000 4000 0012 3456
            -------------------
            05 92 78 9F FF ED CB A9 
            */
            bool result = false;
            int[] aArray = new int[8];
            string aPIN = "";
            string aPAN = "";

            XOR = "";
            
            aPIN = PIN.Length.ToString();

            if (aPIN.Length == 1) 
              aPIN = "0" + aPIN;

            aPIN = aPIN + PIN;

            while (aPIN.Length < 16 )
              aPIN = aPIN + "F";

            aPAN = getPAN12(PAN);

            while (aPAN.Length < 16)
                aPAN = "0" +aPAN;

            
            aArray[0] = Convert.ToByte(aPIN.Substring(0, 2), 16)  ^ Convert.ToByte(aPAN.Substring(0, 2), 16);
            aArray[1] = Convert.ToByte(aPIN.Substring(2, 2), 16)  ^ Convert.ToByte(aPAN.Substring(2, 2), 16);
            aArray[2] = Convert.ToByte(aPIN.Substring(4, 2), 16)  ^ Convert.ToByte(aPAN.Substring(4, 2), 16);
            aArray[3] = Convert.ToByte(aPIN.Substring(6, 2), 16)  ^ Convert.ToByte(aPAN.Substring(6, 2), 16);
            aArray[4] = Convert.ToByte(aPIN.Substring(8, 2), 16)  ^ Convert.ToByte(aPAN.Substring(8, 2), 16);
            aArray[5] = Convert.ToByte(aPIN.Substring(10, 2), 16) ^ Convert.ToByte(aPAN.Substring(10, 2), 16);
            aArray[6] = Convert.ToByte(aPIN.Substring(12, 2), 16) ^ Convert.ToByte(aPAN.Substring(12, 2), 16);
            aArray[7] = Convert.ToByte(aPIN.Substring(14, 2), 16) ^ Convert.ToByte(aPAN.Substring(14, 2), 16);
            

            for (int i = 0; i < 8; i++)
                XOR += ":" + (aArray[i] < 10 ? "0" : "") + aArray[i].ToString("X");
			 
            if ( XOR.StartsWith(":") )
                XOR = XOR.Substring(1);
			
            result = true;

            return result;

        }

        private static string getPAN12(string PAN)
        {

            string ZERO_PAD = "0000000000000000";
            string aPAN;
            int x;
            int i;
            string result = "";
            /*
            //Ejemplo
            //xxx			       x
            //4766840010336205 -> 684001033620

            Ejemplo de la documentacion thales, la tarjeta  puede ser inferior a 16 caracteres

            PIN   PAN
            92389 4000001234562

            0592 389F FFFF FFFF
            0000 4000 0012 3456
            -------------------
            05 92 78 9F FF ED CB A9
            */

            aPAN = ZERO_PAD + PAN;
            x = aPAN.Length;
            i = x - 13;
            result = aPAN.Substring(i);
            result = result.Substring(0, result.Length - 1);

            return result;
        }


        private static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

        private static string FromBase64ToHEX(string base64)
        {
            char[] c = new char[base64.Length * 2];
            byte b;
            for (int i = 0; i < base64.Length; ++i)
            {
                b = ((byte)(base64[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(base64[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }

        public static string getPINBlock(string input, string key)
		{

            var toEncryptArray = StringToByteArray(input); //UTF8Encoding.UTF8.GetBytes(input);

			
			var tdes = new TripleDESCryptoServiceProvider();
            tdes.Key = StringToByteArray(key); //keyArray;
			tdes.Mode = CipherMode.ECB;
			tdes.Padding = PaddingMode.None;
            ICryptoTransform transformation = tdes.CreateDecryptor();


			var cTransform = tdes.CreateEncryptor();
			byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);
            tdes.Clear();
            return BitConverter.ToString(resultArray).Replace("-", "");
		}
    }
}
