﻿using System;
using SAutoCarry.Champions;
using LeagueSharp;
using LeagueSharp.Common;

namespace SAutoCarry
{
    class Program
    {
        public static SCommon.PluginBase.Champion Champion; 
        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        static void Game_OnGameLoad(EventArgs args)
        {
            switch(ObjectManager.Player.CharData.BaseSkinName)
            {
                case "Vayne":
                    Champion = new Vayne();
                    break;

                case "Azir":
                    Champion = new Azir();
                    break;

                case "Rengar":
                    Champion = new Rengar();
                    break;

                case "Lucian":
                    Champion = new Lucian();
                    break;

                case "Riven":
                    Champion = new Riven();
                    break;

                case "Veigar":
                    Champion = new Veigar();
                    break;

                case "Pantheon":
                    Champion = new Pantheon();
                    break;

                case "Shyvana":
                    Champion = new Shyvana();
                    break;

                case "TwistedFate":
                    Champion = new TwistedFate();
                    break;

                case "Viktor":
                    Champion = new Viktor();
                    break;

                case "Twitch":
                    Champion = new Twitch();
                    break;

                case "Jax":
                    Champion = new Jax();
                    break;

                case "MasterYi":
                    Champion = new MasterYi();
                    break;

                case "Orianna":
                    Champion = new Orianna();
                    break;

                case "Blitzcrank":
                    Champion = new Blitzcrank();
                    break;
            }
        }
    }
}