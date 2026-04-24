using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MusicDiaryBot
{
    public enum DialogState
    {
        None,
        AwaitingArtistname,
        AwaitingSongtitle,
    }

    public class UserState
    {
        public DialogState State {  get; set; } = DialogState.None;
        public string Artistname { get; set; } = ""; 
        public string Songtitle { get; set; } = "";
    }
}
