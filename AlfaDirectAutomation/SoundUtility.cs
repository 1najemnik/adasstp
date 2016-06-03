using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Windows.Media;

namespace AlfaDirectAutomation
{
    public class SoundUtility
    {
         public static void Play(string filePath)
        {
            var player = new MediaPlayer();
            player.Open(new Uri(filePath, UriKind.Relative));
           
            player.Play();
        }
    }
}
