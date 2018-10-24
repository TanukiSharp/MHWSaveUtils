using System;
using System.Collections.Generic;
using System.Text;

namespace MHWSaveUtils
{
    public static class MiscUtils
    {
        public static string PlaytimeToGameString(uint playTime)
        {
            uint playtimeSeconds = playTime % 60;
            uint playtimeMinutes = playTime / 60 % 60;
            uint playtimeHours = playTime / 3600;

            return $"{playtimeHours:d02}:{playtimeMinutes:d02}:{playtimeSeconds:d02}";
        }
    }
}
