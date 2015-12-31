using System;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon.Database;

namespace SCommon.Evade
{
    public static class TargetedSpellDetector
    {
        public delegate void dOnDetected(DetectedTargetedSpellArgs args);
        public static event dOnDetected OnDetected;

        static TargetedSpellDetector()
        {
            Obj_AI_Base.OnProcessSpellCast += Obj_AI_Base_OnProcessSpellCast;
            if(SpellDatabase.TargetedSpells == null)
                SpellDatabase.InitalizeSpellDatabase();
        }

        private static void Obj_AI_Base_OnProcessSpellCast(Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
        {
            if(OnDetected != null && sender.IsChampion() && !sender.IsMe)
            {
                var spells = SpellDatabase.TargetedSpells.Where(p => p.ChampionName == (sender as Obj_AI_Hero).ChampionName);
                if(spells != null && spells.Count() > 0)
                {
                    var spell = spells.Where(p => p.SpellName == args.SData.Name).FirstOrDefault();
                    if (spell != null)
                    {
                        if ((spell.IsTargeted && args.Target != null && args.Target.IsMe) ||
                            (!spell.IsTargeted && sender.Distance(ObjectManager.Player.ServerPosition) <= spell.Radius))
                            OnDetected(new DetectedTargetedSpellArgs { Caster = sender, SpellData = spell, SpellCastArgs = args });
                    }
                }
            }
        }
    }

    public class DetectedTargetedSpellArgs : EventArgs
    {
        public Obj_AI_Base Caster;
        public SCommon.Database.SpellData SpellData;
        public GameObjectProcessSpellCastEventArgs SpellCastArgs;
    }
}
