﻿﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon.Maths;
using SCommon.Prediction;
using SCommon.PluginBase;
using SharpDX;
//typedefs
using Prediction = SCommon.Prediction.Prediction;
using Collision = SCommon.Prediction.Collision;

namespace SAutoCarry.Champions.Helpers
{
    public static class BallMgr
    {
        public enum Command
        {
            Attack = 0,
            Dissonance = 1,
            Protect = 2,
            Shockwave = 3,
        }

        private static Champion s_Champion;
        private static ConcurrentQueue<Tuple<Command, Obj_AI_Hero>> s_WorkQueue;
        private static Vector3 s_Position;

        public static bool IsBallReady { get; set; }

        public static Vector3 Position
        {
            get { return s_Position; }
            set
            {
                if (s_Position != value)
                {
                    BallMgr_OnPositionChanged(s_Position, value);
                    s_Position = value;
                }
            }
        }

        public delegate void dOnProcessCommand(Command cmd, Obj_AI_Hero target);
        public static event dOnProcessCommand OnProcessCommand;

        public static void Initialize(Champion champ)
        {
            s_Champion = champ;
            s_WorkQueue = new ConcurrentQueue<Tuple<Command, Obj_AI_Hero>>();
            Position = ObjectManager.Player.ServerPosition;
            Game.OnUpdate += Game_OnUpdate;
            Obj_AI_Hero.OnCreate += Obj_AI_Hero_OnCreate;
            Obj_AI_Hero.OnProcessSpellCast += Obj_AI_Hero_OnProcessSpellCast;
        }

        public static void Post(Command cmd, Obj_AI_Hero t)
        {
            s_WorkQueue.Enqueue(new Tuple<Command, Obj_AI_Hero>(cmd, t));
        }

        public static void Process(int count = 1)
        {
            Tuple<Command, Obj_AI_Hero> cmd;
            for (int i = 0; i < count; i++)
            {
                if (s_WorkQueue.TryDequeue(out cmd))
                    OnProcessCommand(cmd.Item1, cmd.Item2);
            }
        }

        public static void ClearWorkQueue()
        {
            s_WorkQueue = new ConcurrentQueue<Tuple<Command, Obj_AI_Hero>>();
        }

        public static bool CheckHeroCollision(Vector3 to)
        {
            if (Position == to)
                return false;

            return Collision.CheckEnemyHeroCollision(Position.To2D(), to.To2D(), 130f, 0.25f);
        }

        private static void Obj_AI_Hero_OnCreate(GameObject sender, EventArgs args)
        {
            if (sender.IsAlly && sender.Name.Contains("DoomBall"))
                Position = sender.Position;
        }

        private static void Game_OnUpdate(EventArgs args)
        {
            if (ObjectManager.Player.HasBuff("OrianaGhostSelf"))
                Position = ObjectManager.Player.ServerPosition;
            else
            {
                foreach (var ally in HeroManager.Allies)
                {
                    if (ally.HasBuff("OrianaGhost"))
                    {
                        Position = ally.ServerPosition;
                        break;
                    }
                }
            }

            if (IsBallReady)
                Process();
        }

        private static void Obj_AI_Hero_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if (sender.IsMe && (args.SData.Name == "OrianaIzunaCommand" || args.SData.Name == "OrianaRedactCommand"))
                Position = Vector3.Zero;
        }

        private static void BallMgr_OnPositionChanged(Vector3 oldVal, Vector3 newVal)
        {
            IsBallReady = newVal != Vector3.Zero;
            for (int i = 0; i < 4; i++)
            {
                if (Program.Champion.Spells[i] != null)
                    Program.Champion.Spells[i].From = newVal;
            }
        }
    }
}