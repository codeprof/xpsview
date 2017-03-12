//Copyright (c) 2012 Stefan Moebius (mail@stefanmoebius.de)

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace xpsview
{
    class Lang
    {
        public static string translate(string en, string de, string fr)
        {
            /*
            French	1036
            French (Belgium)	2060
            French (Canadian)	3084
            French (Luxembourg)	5132
            French (Swiss)	4108
            */
            /*
            German	1031
            German (Austrian)	3079
            German (Liechtenstein)	5127
            German (Luxembourg)	4103
            German (Swiss)	2055
            */
            int lcid = Thread.CurrentThread.CurrentUICulture.LCID;
            if (lcid == 1031 || lcid == 3079 || lcid == 5127 || lcid == 4103 || lcid == 2055) // German
                return de;
            if (lcid == 1036 || lcid == 2060 || lcid == 3084 || lcid == 5132 || lcid == 4108) // French
                return fr;
            return en;
        }
    }
}
