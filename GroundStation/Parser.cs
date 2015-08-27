using BKSystem.IO;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace GroundStation
{
    public class Parser
    {

       public static byte[] StringToByte(string hexValues)
        {

            int i = 0;
            string[] hexValuesSplit = hexValues.Split(' ');
            byte[] res = new byte[hexValuesSplit.Length];
            foreach (String hex in hexValuesSplit)
            {
                if (i < 256)
                {
                    // Convert the number expressed in base-16 to an integer. 
                    int value = Convert.ToInt32(hex, 16);
                    // Get the character corresponding to the integral value. 
                    string stringValue = Char.ConvertFromUtf32(value);
                    char charValue = (char)value;
                    res[i] = (byte)charValue;
                    i++;
                }
            }
            return res;
        }

       public static string GetFitterMessage(BitStream raw)
       {

          string msg = System.Text.Encoding.ASCII.GetString(raw.ToByteArray());
          msg = msg.Replace("\0", "");
          return msg;
       }

       public static Color OnOffElement(string value)
       {
           if (value == "1")
               return Color.Green;
           else
               return Color.Red;

       }

       public static Color OnOffElement(bool val)
       {
           if (val)
               return Color.Green;
           return Color.Red;

       }

    }

    
}
