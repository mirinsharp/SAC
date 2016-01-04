using System;
using System.Collections.Generic;
using System.Linq;
using LeagueSharp;
using LeagueSharp.Common;
using SCommon;
using SCommon.Database;
using SCommon.PluginBase;
using SCommon.Prediction;
using SCommon.Orbwalking;
using SUtility.Drawings;
using SharpDX;


namespace SAutoCarry.Champions
{
    public class Corki : Champion
    {
        public Corki()
            : base("Corki", "SAutoCarry - Corki")
        {
            OnUpdate += BeforeOrbwalk;
            OnCombo += Combo;
            OnHarass += Harass;
            OnLaneClear += LaneClear;
        }

        public override void CreateConfigMenu()
        {
            Menu combo = new Menu("Combo", "SAutoCarry.Corki.Combo");
            combo.AddItem(new MenuItem("SAutoCarry.Corki.Combo.UseQ", "Use Q").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Corki.Combo.UseE", "Use E").SetValue(true));
            combo.AddItem(new MenuItem("SAutoCarry.Corki.Combo.UseR", "Use R").SetValue(true));

            Menu harass = new Menu("Harass", "SAutoCarry.Corki.Harass");
            harass.AddItem(new MenuItem("SAutoCarry.Corki.Harass.UseQ", "Use Q").SetValue(true));
            harass.AddItem(new MenuItem("SAutoCarry.Corki.Harass.UseE", "Use E").SetValue(true));
            harass.AddItem(new MenuItem("SAutoCarry.Corki.Harass.UseR", "Use R").SetValue(true)).ValueChanged += (s, ar) => ConfigMenu.Item("SAutoCarry.Corki.Harass.RStacks").Show(ar.GetNewValue<bool>());
            harass.AddItem(new MenuItem("SAutoCarry.Corki.Harass.RStacks", "Keep R Stacks").SetValue(new Slider(3, 0, 7))).Show(harass.Item("SAutoCarry.Corki.Harass.UseR").GetValue<bool>());
            harass.AddItem(new MenuItem("SAutoCarry.Corki.Harass.MinMana", "Min Mana Percent").SetValue(new Slider(30, 100, 0)));

            Menu laneclear = new Menu("LaneClear/JungleClear", "SAutoCarry.Corki.LaneClear");
            laneclear.AddItem(new MenuItem("SAutoCarry.Corki.LaneClear.UseQ", "Use Q").SetValue(true));
            laneclear.AddItem(new MenuItem("SAutoCarry.Corki.LaneClear.UseE", "Use E").SetValue(true));
            laneclear.AddItem(new MenuItem("SAutoCarry.Corki.LaneClear.UseR", "Use R").SetValue(true));
            laneclear.AddItem(new MenuItem("SAutoCarry.Corki.LaneClear.MinMana", "Min Mana Percent").SetValue(new Slider(50, 100, 0)));

            Menu misc = new Menu("Misc", "SAutoCarry.Corki.Misc");
            misc.AddItem(new MenuItem("SAutoCarry.Corki.Misc.RKillSteal", "KS With R").SetValue(true));

            ConfigMenu.AddSubMenu(combo);
            ConfigMenu.AddSubMenu(harass);
            ConfigMenu.AddSubMenu(laneclear);

            ConfigMenu.AddToMainMenu();
        }

        public override void SetSpells()
        {
            Spells[Q] = new Spell(SpellSlot.Q, 825f);
            Spells[Q].SetSkillshot(0.35f, 250f, 1000f, true, SkillshotType.SkillshotCircle);

            Spells[W] = new Spell(SpellSlot.W, 0f);

            Spells[E] = new Spell(SpellSlot.E, 125f);

            Spells[R] = new Spell(SpellSlot.R, 1225f);
            Spells[R].SetSkillshot(0.20f, 75f, 2000f, true, SkillshotType.SkillshotLine);

        }

        public void BeforeOrbwalk()
        {
            if (ObjectManager.Player.HasBuff("corkimissilebarragecounterbig"))
                Spells[R].Width = 150f;
            else
                Spells[R].Range = 75f;

            if (KillStealR)
                KillSteal();
        }

        public void Combo()
        {

            if (Spells[Q].IsReady() && ComboUseQ)
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    Spells[Q].SPredictionCast(t, HitChance.High);
                }
            }

            if (Spells[R].IsReady() && ComboUseR && Spells[R].Instance.Ammo > 0)
            {
                var t = TargetSelector.GetTarget(Spells[R].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                {
                    Spells[R].SPredictionCast(t, HitChance.High);
                }
            }

            if (Spells[E].IsReady() && ComboUseE)
            {
                if(TargetSelector.GetTarget(Spells[E].Range, TargetSelector.DamageType.Physical) != null)
                    Spells[E].Cast();
            }
        }

        public void Harass()
        {
            if (ObjectManager.Player.ManaPercent < HarassMinMana)
                return;

            if (Spells[Q].IsReady() && HarassUseQ)
            {
                var t = TargetSelector.GetTarget(Spells[Q].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    Spells[Q].SPredictionCast(t, HitChance.High);
            }

            if (Spells[E].IsReady() && HarassUseE)
            {
                var t = TargetSelector.GetTarget(Spells[E].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    Spells[E].Cast();
            }

            if (Spells[R].IsReady() && HarassUseR && Spells[R].Instance.Ammo > HarassRStack)
            {
                var t = TargetSelector.GetTarget(Spells[R].Range, TargetSelector.DamageType.Magical);
                if (t != null)
                    Spells[R].SPredictionCast(t, HitChance.High);
            }
        }

        public void LaneClear()
        {
            if (ObjectManager.Player.ManaPercent < LaneClearMinMana)
                return;

            var minion = MinionManager.GetMinions(Spells[Q].Range, MinionTypes.All, MinionTeam.NotAlly, MinionOrderTypes.MaxHealth).FirstOrDefault();
            if (minion != null)

                if (Spells[Q].IsReady() && LaneClearQ)
                {
                    if (MinionManager.GetMinions(Spells[Q].Range + 100, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).Count() > 4)
                        Spells[Q].Cast();
                }

            if (Spells[R].IsReady() && LaneClearR)
            {
                if (MinionManager.GetMinions(Spells[R].Range + 100, MinionTypes.All, MinionTeam.Enemy, MinionOrderTypes.MaxHealth).Count() > 4)
                    Spells[R].Cast();
            }
        }

        public void KillSteal()
        {
            if (!Spells[R].IsReady() || Spells[R].Instance.Ammo == 0)
                return;

            foreach (Obj_AI_Hero target in HeroManager.Enemies.Where(x => x.IsValidTarget(Spells[R].Range) && !x.HasBuffOfType(BuffType.Invulnerability)))
            {
                if ((ObjectManager.Player.GetSpellDamage(target, SpellSlot.R)) > target.Health + 20)
                    Spells[R].SPredictionCast(target, HitChance.High);
            }
        }

        public bool ComboUseQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Combo.UseQ").GetValue<bool>(); }
        }

        public bool ComboUseE
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Combo.UseE").GetValue<bool>(); }
        }

        public bool ComboUseR
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Combo.UseR").GetValue<bool>(); }
        }

        public bool HarassUseQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Harass.UseQ").GetValue<bool>(); }
        }

        public bool HarassUseE
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Harass.UseE").GetValue<bool>(); }
        }

        public bool HarassUseR
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Harass.UseR").GetValue<bool>(); }
        }

        public int HarassRStack
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Harass.RStacks").GetValue<Slider>().Value; }
        }

        public int HarassMinMana
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Harass.MinMana").GetValue<Slider>().Value; }
        }

        public bool LaneClearQ
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.LaneClear.UseQ").GetValue<bool>(); }
        }

        public bool LaneClearR
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.LaneClear.UseR").GetValue<bool>(); }
        }

        public int LaneClearMinMana
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.LaneClear.MinMana").GetValue<Slider>().Value; }
        }

        public bool KillStealR
        {
            get { return ConfigMenu.Item("SAutoCarry.Corki.Misc.RKillSteal").GetValue<bool>(); }
        }
    }
}
