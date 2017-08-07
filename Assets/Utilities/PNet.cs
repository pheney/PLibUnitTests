using UnityEngine;

namespace PLib.Network
{
    /// <summary>
    /// 2016-16-16
    /// Networking
    /// TODO -- Needs major overhaul. Pretty sure most of what is in here
    /// is obsolete.
    /// </summary>
    public static class PNet
    {
        public static string CHAT_GLOBAL = "GlobalChatRoom";
        public static string CHAT_TEAM = "TeamChatRoom";
        public static string CHAT_PRIVATE = "PrivateChatRoom";

        //	NetworkView	//

        public static string LongName(this NetworkView netView)
        {
            return netView.owner.ToString().ToLower() + "_" + netView.owner.externalIP.ToString();
        }

        public static int ToInteger(this NetworkViewID netViewId)
        {
            return int.Parse(netViewId.ToString().Split(' ')[1]);
        }

        public static NetworkView GetId(this NetworkView[] array, int id)
        {
            foreach (NetworkView nv in array)
            {
                if (nv.viewID.ToInteger() == id)
                {
                    return nv;
                }
            }
            return null;
        }
    }
}