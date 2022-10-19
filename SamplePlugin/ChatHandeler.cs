using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.Text;
using Dalamud.Game;
using XivCommon.Functions;
using System.IO;


namespace ChatToAction
{
    public class ChatHandeler
    {
        public static void DoCommand(XivChatType type, string message)
        {
            string docPath =
             Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

            // Write the string array to a new file named "WriteLines.txt".
            using (StreamWriter outputFile = new StreamWriter("ffxivchat.txt"))
            {

                //string v = ActGlobals.ToString();
                //string output = v;
                //foreach (string line in lines

                outputFile.WriteLine(message);
            }

        }
        public static void OnChatMessage(
            XivChatType type,
            uint senderId,
            ref SeString sender,
            ref SeString message,
            ref bool isHandled)
        {
            if (isHandled)
                return;
            ChatHandler.DoCommand(type, ((object)message).ToString());
        }
    }
}
