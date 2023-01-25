﻿// Decompiled with JetBrains decompiler
// Type: Androids.CustomizeAndroidWindow
// Assembly: Androids, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 60A64EA7-F267-4623-A880-9FF7EC14F1A0
// Assembly location: E:\CACHE\Androids-1.3hsk.dll

using AlienRace;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace Androids
{
    public class CustomizeAndroidWindow : Window
    {
        public Building_AndroidPrinter androidPrinter;
        public Pawn newAndroid;
        public List<ThingOrderRequest> finalCalculatedPrintingCost = new List<ThingOrderRequest>();
        public int finalExtraPrintingTimeCost;
        public bool refreshAndroidPortrait;
        public Vector2 upgradesScrollPosition;
        public Vector2 traitsScrollPosition;
        private List<Trait> allTraits = new List<Trait>();
        public PawnKindDef currentPawnKindDef;
        public BackstoryDef newChildhoodBackstory;
        public BackstoryDef newAdulthoodBackstory;
        public Trait replacedTrait;
        public Trait newTrait;
        public List<UpgradeCommand> appliedUpgradeCommands = new List<UpgradeCommand>();
        public List<AndroidUpgradeDef> upgradedDefsToDisable = new List<AndroidUpgradeDef>();
        public List<Trait> originalTraits = new List<Trait>();
        public Color originalHairColor;
        public HairDef originalHairDef;
        public static readonly float upgradesOffset = 640f;
        private static readonly Vector2 PawnPortraitSize = new Vector2(100f, 140f);
        private static readonly SimpleCurve LevelRandomCurve = new SimpleCurve()
    {
      {
        new CurvePoint(0.0f, 0.0f),
        true
      },
      {
        new CurvePoint(0.5f, 150f),
        true
      },
      {
        new CurvePoint(4f, 150f),
        true
      },
      {
        new CurvePoint(5f, 25f),
        true
      },
      {
        new CurvePoint(10f, 5f),
        true
      },
      {
        new CurvePoint(15f, 0.0f),
        true
      }
    };
        private static readonly SimpleCurve LevelFinalAdjustmentCurve = new SimpleCurve()
    {
      {
        new CurvePoint(0.0f, 0.0f),
        true
      },
      {
        new CurvePoint(10f, 10f),
        true
      },
      {
        new CurvePoint(20f, 16f),
        true
      },
      {
        new CurvePoint(27f, 20f),
        true
      }
    };
        public static List<Color> DefaultHairColors = new List<Color>((IEnumerable<Color>)new Color[13]
        {
      new Color(0.17f, 0.17f, 0.17f, 1f),
      new Color(0.02f, 0.02f, 0.02f, 1f),
      new Color(0.9f, 0.9f, 0.9f, 1f),
      new Color(0.51f, 0.25f, 0.25f, 1f),
      new Color(1f, 0.66f, 0.32f, 1f),
      new Color(0.0f, 0.5f, 1f, 1f),
      new Color(1f, 0.0f, 0.5f, 1f),
      new Color(1f, 0.0f, 0.0f, 1f),
      new Color(0.0f, 1f, 0.0f, 1f),
      new Color(0.0f, 1f, 1f, 1f),
      new Color(0.78f, 0.78f, 0.78f, 1f),
      new Color(0.92f, 0.92f, 0.29f, 1f),
      new Color(0.63f, 0.28f, 0.64f, 1f)
        });
        public List<HediffDef> _pawnHediffs;
        public List<Trait> _pawnTraits;
        public bool IsUpgrade;
        public Pawn clonedPawn;

        public override Vector2 InitialSize => new Vector2(898f, 608f);

        public IEnumerable<Color> HairColors
        {
            get
            {
                ThingDef_AlienRace alien = ThingDefOf.ChjAndroid as ThingDef_AlienRace;
                foreach (Color defaultHairColor in CustomizeAndroidWindow.DefaultHairColors)
                {
                    Color color = defaultHairColor;
                    yield return color;
                    color = new Color();
                }
            }
        }

        public IEnumerable<Color> SkinColors
        {
            get
            {
                yield break;
            }
        }

        public List<HediffDef> pawnHediffs()
        {
            if (!this.IsUpgrade)
                return (List<HediffDef>)null;
            Pawn_HealthTracker health = this.newAndroid.health;
            List<HediffDef> hediffDefList1;
            if (health == null)
            {
                hediffDefList1 = (List<HediffDef>)null;
            }
            else
            {
                HediffSet hediffSet = health.hediffSet;
                hediffDefList1 = hediffSet != null ? hediffSet.hediffs.Select<Hediff, HediffDef>((Func<Hediff, HediffDef>)(hediff => hediff.def)).ToList<HediffDef>() : (List<HediffDef>)null;
            }
            List<HediffDef> hediffDefList2 = hediffDefList1;
            if (this._pawnHediffs.NullOrEmpty<HediffDef>())
                this._pawnHediffs = hediffDefList2;
            return hediffDefList2;
        }

        public List<Trait> pawnTraits(Pawn pawn)
        {
            if (!this.IsUpgrade)
                return (List<Trait>)null;
            List<Trait> list = pawn.story.traits.allTraits.ToList<Trait>();
            if (this._pawnTraits.NullOrEmpty<Trait>())
                this._pawnTraits = list;
            return list;
        }

        public bool AlreadyUpgradedOnPawn(AndroidUpgradeDef upgrade)
        {
            this.pawnHediffs();
            List<HediffDef> pawnHediffs = this._pawnHediffs;
            return !pawnHediffs.NullOrEmpty<HediffDef>() && pawnHediffs.Contains(upgrade.hediffToApply);
        }

        public CustomizeAndroidWindow(Building_AndroidPrinter androidPrinter)
        {
            this.androidPrinter = androidPrinter;
            Pawn sourcePawn = (Pawn)androidPrinter.GetDirectlyHeldThings().Where<Thing>((Func<Thing, bool>)(p => p is Pawn)).FirstOrDefault<Thing>();
            if (sourcePawn != null)
            {
                this.currentPawnKindDef = sourcePawn.kindDef;
                if (this.clonedPawn == null)
                {
                    this.clonedPawn = this.GetClone(sourcePawn);
                    this.pawnTraits(this.clonedPawn);
                }
                this.IsUpgrade = true;
                this.newAndroid = sourcePawn;
            }
            else
            {
                this.currentPawnKindDef = PawnKindDef.Named("ChjAndroidColonist");
                this.newAndroid = this.GetNewPawn();
            }
            this.RefreshCosts();
        }

        public override void PostClose()
        {
            base.PostClose();
            if (this.androidPrinter.printerStatus != CrafterStatus.Idle)
                return;
            CustomizeAndroidWindow.PawnUpdate(this.newAndroid, this.clonedPawn);
        }

        public override void DoWindowContents(Rect inRect)
        {
            if (this.refreshAndroidPortrait)
            {
                this.newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                PortraitsCache.SetDirty(this.newAndroid);
                PortraitsCache.PortraitsCacheUpdate();
                this.refreshAndroidPortrait = false;
            }
            if (this.newChildhoodBackstory != null)
            {
                this.newAndroid.story.Childhood = (BackstoryDef)this.newChildhoodBackstory;
                this.newChildhoodBackstory = (BackstoryDef)null;
                this.RefreshPawn();
            }
            if (this.newAdulthoodBackstory != null)
            {
                this.newAndroid.story.Adulthood = (BackstoryDef)this.newAdulthoodBackstory;
                this.newAdulthoodBackstory = (BackstoryDef)null;
                this.RefreshPawn();
            }
            if (this.newTrait != null)
            {
                if (this.replacedTrait != null)
                {
                    this.newAndroid.story.traits.allTraits.Remove(this.replacedTrait);
                    this.replacedTrait = (Trait)null;
                }
                this.newAndroid.story.traits.allTraits.Add(new Trait(this.newTrait.def, this.newTrait.Degree));
                if (this.newAndroid.workSettings != null)
                    this.newAndroid.workSettings.EnableAndInitialize();
                if (this.newAndroid.skills != null)
                    this.newAndroid.skills.Notify_SkillDisablesChanged();
                if (this.newAndroid.RaceProps.Humanlike)
                    this.newAndroid.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
                this.RefreshPawn();
                this.newTrait = (Trait)null;
            }
            Rect rect1 = new Rect(inRect)
            {
                width = (CustomizeAndroidWindow.PawnPortraitSize.x + 16f),
                height = (CustomizeAndroidWindow.PawnPortraitSize.y + 16f)
            }.CenteredOnXIn(inRect).CenteredOnYIn(inRect);
            rect1.x += 16f;
            rect1.y += 16f;
            if (this.newAndroid != null)
            {
                Rect position = new Rect((float)((double)rect1.xMin + ((double)rect1.width - (double)CustomizeAndroidWindow.PawnPortraitSize.x) / 2.0 - 10.0), rect1.yMin + 20f, CustomizeAndroidWindow.PawnPortraitSize.x, CustomizeAndroidWindow.PawnPortraitSize.y);
                GUI.DrawTexture(position, (Texture)PortraitsCache.Get(this.newAndroid, CustomizeAndroidWindow.PawnPortraitSize, Rot4.South, new Vector3(), 1f, true, true, true, true, (Dictionary<Apparel, Color>)null, new Color?(), false));
                Widgets.InfoCardButton(position.xMax - 16f, position.y, (Thing)this.newAndroid);
                Verse.Text.Font = GameFont.Medium;
                Verse.Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(new Rect(0.0f, 0.0f, inRect.width, 32f), "AndroidCustomization".Translate());
                Verse.Text.Font = GameFont.Small;
                Verse.Text.Anchor = TextAnchor.MiddleLeft;
                float y1 = 32f;
                Rect rect2 = new Rect(32f, y1, 240f, 24f);
                if (this.newAndroid.Name is NameTriple name)
                {
                    Rect rect3 = new Rect(rect2);
                    rect3.width *= 0.333f;
                    Rect rect4 = new Rect(rect2);
                    rect4.width *= 0.333f;
                    rect4.x += rect4.width;
                    Rect rect5 = new Rect(rect2);
                    rect5.width *= 0.333f;
                    rect5.x += rect4.width * 2f;
                    string first = name.First;
                    string nick = name.Nick;
                    string last = name.Last;
                    CharacterCardUtility.DoNameInputRect(rect3, ref first, 12);
                    if (name.Nick == name.First || name.Nick == name.Last)
                        GUI.color = new Color(1f, 1f, 1f, 0.5f);
                    CharacterCardUtility.DoNameInputRect(rect4, ref nick, 9);
                    GUI.color = Color.white;
                    CharacterCardUtility.DoNameInputRect(rect5, ref last, 12);
                    if (name.First != first || name.Nick != nick || name.Last != last)
                        this.newAndroid.Name = (Name)new NameTriple(first, nick, last);
                    TooltipHandler.TipRegion(rect3, (TipSignal)"FirstNameDesc".Translate());
                    TooltipHandler.TipRegion(rect4, (TipSignal)"ShortIdentifierDesc".Translate());
                    TooltipHandler.TipRegion(rect5, (TipSignal)"LastNameDesc".Translate());
                }
                else
                {
                    rect2.width = 999f;
                    Verse.Text.Font = GameFont.Medium;
                    Widgets.Label(rect2, this.newAndroid.Name.ToStringFull);
                    Verse.Text.Font = GameFont.Small;
                }
                float num1 = (float)((double)rect1.x + (double)rect1.width + 16.0 + ((double)inRect.width - (double)CustomizeAndroidWindow.upgradesOffset));
                Rect source1 = new Rect((float)((double)rect1.x + (double)rect1.width + 16.0), rect1.y, inRect.width - num1, 24f);
                Rect rect6 = new Rect(source1);
                rect6.width = rect6.height;
                Widgets.DrawBoxSolid(rect6, this.newAndroid.story.HairColor);
                Widgets.DrawBox(rect6);
                Widgets.DrawHighlightIfMouseover(rect6);
                if (Widgets.ButtonInvisible(rect6))
                {
                    Func<Color, Action> func = (Func<Color, Action>)(color => (Action)(() =>
                  {
                      this.newAndroid.story.HairColor = color;
                      this.newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                      PortraitsCache.SetDirty(this.newAndroid);
                      PortraitsCache.PortraitsCacheUpdate();
                  }));
                    List<FloatMenuOption> options = new List<FloatMenuOption>();
                    foreach (Color hairColor1 in this.HairColors)
                    {
                        Color HairColor = hairColor1;
                        options.Add(new FloatMenuOption((string)"AndroidCustomizationChangeColor".Translate(), func(HairColor), extraPartWidth: 24f, extraPartOnGUI: ((Func<Rect, bool>)(rect =>
                      {
                          Rect rect7 = new Rect(rect);
                          rect7.x += 8f;
                          Widgets.DrawBoxSolid(rect7, HairColor);
                          Widgets.DrawBox(rect7);
                          this.RefreshUpgrades();
                          this.RefreshCosts();
                          return false;
                      }))));
                    }
                    Find.WindowStack.Add((Window)new FloatMenu(options));
                }
                Rect rect8 = new Rect(source1);
                rect8.width -= rect6.width;
                rect8.width -= 8f;
                rect8.x = (float)((double)rect6.x + (double)rect6.width + 8.0);
                if (Widgets.ButtonText(rect8, (string)(this.newAndroid?.story?.hairDef?.LabelCap ?? (TaggedString)"Bald"), true, true, true))
                {
                    IEnumerable<HairDef> objects = DefDatabase<HairDef>.AllDefs.Where<HairDef>((Func<HairDef, bool>)(hairdef =>
                   {
                       if (this.newAndroid.gender == Gender.Female && (hairdef.styleGender == StyleGender.Any || hairdef.styleGender == StyleGender.Female || hairdef.styleGender == StyleGender.FemaleUsually))
                           return true;
                       if (this.newAndroid.gender != Gender.Male)
                           return false;
                       return hairdef.styleGender == StyleGender.Any || hairdef.styleGender == StyleGender.Male || hairdef.styleGender == StyleGender.MaleUsually;
                   }));
                    if (objects != null)
                        FloatMenuUtility.MakeMenu<HairDef>(objects, (Func<HairDef, string>)(hairDef => (string)hairDef.LabelCap), (Func<HairDef, Action>)(hairDef => (Action)(() =>
                    {
                        this.newAndroid.story.hairDef = hairDef;
                        this.newAndroid.Drawer.renderer.graphics.ResolveAllGraphics();
                        PortraitsCache.SetDirty(this.newAndroid);
                        PortraitsCache.PortraitsCacheUpdate();
                        this.RefreshUpgrades();
                        this.RefreshCosts();
                    })));
                }
                Rect rect9 = new Rect((float)((double)rect1.x + (double)rect1.width + 16.0), rect1.y + 32f, inRect.width - num1, 32f);
                Verse.Text.Font = GameFont.Medium;
                string str1 = (string)"AndroidCustomizationPrint".Translate();
                if (Widgets.ButtonText(rect9, str1, true, true, true))
                {
                    if (!this.finalCalculatedPrintingCost.NullOrEmpty<ThingOrderRequest>())
                        this.androidPrinter.orderProcessor.requestedItems = this.finalCalculatedPrintingCost;
                    this.androidPrinter.extraTimeCost = this.finalExtraPrintingTimeCost;
                    this.androidPrinter.pawnToPrint = this.newAndroid;
                    this.androidPrinter.clonedPawnToPrint = this.clonedPawn;
                    this.androidPrinter.printerStatus = CrafterStatus.Filling;
                    this.Close();
                }
                Verse.Text.Font = GameFont.Small;
                if (RaceUtility.AlienRaceKinds.Count<PawnKindDef>() > 1)
                {
                    if (Widgets.ButtonText(new Rect(304f, y1, 240f, 24f), (string)this.currentPawnKindDef.race.LabelCap, true, true, true))
                        FloatMenuUtility.MakeMenu<PawnKindDef>(RaceUtility.AlienRaceKinds, (Func<PawnKindDef, string>)(raceKind => (string)raceKind.race.LabelCap), (Func<PawnKindDef, Action>)(raceKind => (Action)(() =>
                    {
                        this.currentPawnKindDef = raceKind;
                        Gender gender = Gender.Female;
                        if (this.currentPawnKindDef.race is ThingDef_AlienRace race2 && (double)race2.alienRace.generalSettings.maleGenderProbability >= 1.0)
                            gender = Gender.Male;
                        this.newAndroid = this.GetNewPawn(gender);
                        this.RefreshUpgrades();
                        this.RefreshCosts();
                    })));
                    y1 += 26f;
                }
                Rect rect10 = new Rect(304f, y1, 120f, 24f);
                if (this.androidPrinter.PawnInside == null)
                {
                    if (this.currentPawnKindDef.race is ThingDef_AlienRace race && (double)race.alienRace.generalSettings.maleGenderProbability < 1.0 && Widgets.ButtonText(rect10, (string)"AndroidCustomizationRollFemale".Translate(), true, true, true))
                    {
                        this.newAndroid.SetFactionDirect((Faction)null);
                        this.newAndroid.Destroy(DestroyMode.Vanish);
                        this.newAndroid = this.GetNewPawn();
                        this.RefreshUpgrades();
                        this.RefreshCosts();
                    }
                    rect10 = new Rect(424f, y1, 120f, 24f);
                    if (Widgets.ButtonText(rect10, (string)"AndroidCustomizationRollMale".Translate(), true, true, true))
                    {
                        this.newAndroid.SetFactionDirect((Faction)null);
                        this.newAndroid.Destroy(DestroyMode.Vanish);
                        this.newAndroid = this.GetNewPawn(Gender.Male);
                        this.RefreshUpgrades();
                        this.RefreshCosts();
                    }
                }
                float y2 = y1 + 26f;
                Rect rect11 = new Rect(32f, y2, 240f, 24f);
                Widgets.DrawBox(rect11);
                Widgets.DrawHighlightIfMouseover(rect11);
                string label1 = this.newAndroid.story.Childhood == null ? (string)("AndroidCustomizationFirstIdentity".Translate() + " " + "AndroidNone".Translate()) : (string)("AndroidCustomizationFirstIdentity".Translate() + " " + this.newAndroid.story.Childhood.TitleCapFor(this.newAndroid.gender));
                if (Widgets.ButtonText(rect11, label1))
                    FloatMenuUtility.MakeMenu<BackstoryDef>(DefDatabase<BackstoryDef>.AllDefs.ToList<BackstoryDef>().Select<BackstoryDef, BackstoryDef>((Func<BackstoryDef, BackstoryDef>)(backstoryDef => backstoryDef)).Where<BackstoryDef>((Func<BackstoryDef, bool>)(backstory => (backstory.spawnCategories.Any<string>((Predicate<string>)(category => this.currentPawnKindDef.backstoryCategories != null && this.currentPawnKindDef.backstoryCategories.Any<string>((Predicate<string>)(subCategory => subCategory == category)))) || backstory.spawnCategories.Contains("ChjAndroid")) && backstory.slot == BackstorySlot.Childhood)), (Func<BackstoryDef, string>)(backstory => backstory.TitleCapFor(this.newAndroid.gender)), (Func<BackstoryDef, Action>)(backstory => (Action)(() => this.newChildhoodBackstory = backstory)));
                if (this.newAndroid.story.Childhood != null)
                    TooltipHandler.TipRegion(rect11, (TipSignal)this.newAndroid.story.Childhood.FullDescriptionFor(this.newAndroid));
                Rect rect12 = new Rect(304f, y2, 240f, 24f);
                Widgets.DrawBox(rect12);
                Widgets.DrawHighlightIfMouseover(rect12);
                string label2 = this.newAndroid.story.Adulthood == null ? (string)("AndroidCustomizationSecondIdentity".Translate() + " " + "AndroidNone".Translate()) : (string)("AndroidCustomizationSecondIdentity".Translate() + " " + this.newAndroid.story.Adulthood.TitleCapFor(this.newAndroid.gender));
                if (Widgets.ButtonText(rect12, label2))
                    FloatMenuUtility.MakeMenu<BackstoryDef>(DefDatabase<BackstoryDef>.AllDefs.ToList<BackstoryDef>().Select<BackstoryDef, BackstoryDef>((Func<BackstoryDef, BackstoryDef>)(backstoryDef => backstoryDef)).Where<BackstoryDef>((Func<BackstoryDef, bool>)(backstory => (backstory.spawnCategories.Any<string>((Predicate<string>)(category => this.currentPawnKindDef.backstoryCategories != null && this.currentPawnKindDef.backstoryCategories.Any<string>((Predicate<string>)(subCategory => subCategory == category)))) || backstory.spawnCategories.Contains("ChjAndroid")) && backstory.slot == BackstorySlot.Adulthood)), (Func<BackstoryDef, string>)(backstory => backstory.TitleCapFor(this.newAndroid.gender)), (Func<BackstoryDef, Action>)(backstory => (Action)(() => this.newAdulthoodBackstory = backstory)));
                if (this.newAndroid.story.Adulthood != null)
                    TooltipHandler.TipRegion(rect12, (TipSignal)this.newAndroid.story.Adulthood.FullDescriptionFor(this.newAndroid));
                float y3 = y2 + 32f;
                Rect rect13 = new Rect(32f, y3, 256f, 27f);
                if (Widgets.ButtonText(rect13, (string)"AndroidCustomizationRerollSkills".Translate()))
                    this.RefreshSkills();

                //SkillUI
                SkillUI.DrawSkillsOf(this.newAndroid, new Vector2(32f, y3 + 27f), SkillUI.SkillDrawMode.Gameplay, rect13);
                float y4 = rect1.y + rect1.height;
                float xMax = rect13.xMax;
                Verse.Text.Anchor = TextAnchor.MiddleLeft;
                Verse.Text.Font = GameFont.Medium;
                Rect rect14 = new Rect(xMax, y4, 256f, 26f);
                Widgets.DrawTitleBG(rect14);
                Widgets.Label(rect14.ContractedBy(2f), "AndroidCustomizationCostLabel".Translate());
                float y5 = y4 + 26f;
                int num2 = 0;
                Verse.Text.Font = GameFont.Tiny;
                Verse.Text.Anchor = TextAnchor.LowerLeft;
                int num3 = this.IsUpgrade ? 0 : this.androidPrinter.PrinterProperties.ticksToCraft;
                Rect rect15 = new Rect((float)((double)xMax + 3.0 + (double)num2 * 32.0), y5, 26f, 26f);
                Widgets.DrawTextureFitted(rect15, (Texture)ContentFinder<Texture2D>.Get("UI/TimeControls/TimeSpeedButton_Superfast"), 1f);
                TooltipHandler.TipRegion(rect15, (TipSignal)("AndroidCustomizationTimeCost".Translate() + ": " + (num3 + this.finalExtraPrintingTimeCost).ToStringTicksToPeriodVerbose()));
                Widgets.DrawHighlightIfMouseover(rect15);
                Widgets.Label(rect15.ExpandedBy(8f), (num3 + this.finalExtraPrintingTimeCost).ToStringTicksToPeriodVerbose() ?? "");
                int num4 = num2 + 1;
                Verse.Text.Anchor = TextAnchor.LowerRight;

                //Material Cost
                foreach (ThingOrderRequest thingOrderRequest in this.finalCalculatedPrintingCost)
                {
                    Rect rect16 = new Rect((float)((double)xMax + 3.0 + (double)num4 * 32.0), y5, 26f, 26f);
                    if (thingOrderRequest.nutrition)
                    {
                        Widgets.ThingIcon(rect16, RimWorld.ThingDefOf.Meat_Human, (ThingDef)null, (ThingStyleDef)null, 1f, new Color?());
                        TooltipHandler.TipRegion(rect16, (TipSignal)"AndroidNutrition".Translate());
                    }
                    else
                    {
                        Widgets.ThingIcon(rect16, thingOrderRequest.thingDef, (ThingDef)null, (ThingStyleDef)null, 1f, new Color?());
                        TooltipHandler.TipRegion(rect16, (TipSignal)thingOrderRequest.thingDef.LabelCap);
                    }
                    Widgets.DrawHighlightIfMouseover(rect16);
                    Widgets.Label(rect16, thingOrderRequest.amount.ToString() ?? "");
                    ++num4;
                }
                Verse.Text.Anchor = TextAnchor.UpperLeft;
                Verse.Text.Font = GameFont.Small;
                float y6 = y5 + 32f;
                Verse.Text.Anchor = TextAnchor.MiddleLeft;
                Verse.Text.Font = GameFont.Medium;
                Rect rect17 = new Rect(xMax, y6, 256f, 26f);
                Widgets.DrawTitleBG(rect17);
                Widgets.Label(rect17.ContractedBy(2f), "AndroidCustomizationTraitsLabel".Translate());
                Verse.Text.Font = GameFont.Small;
                float y7 = y6 + 26f;
                Verse.Text.Anchor = TextAnchor.MiddleCenter;
                Trait trait1 = (Trait)null;
                float width = 256f;
                float height1 = 24f;
                float num5 = (float)(this.newAndroid.story.traits.allTraits.Count + 1) * height1;
                Rect rect18 = new Rect(rect17);
                rect18.y += 26f;
                rect18.height = inRect.height - rect18.y;
                rect18.width += 12f;
                Widgets.BeginScrollView(rect18, ref this.traitsScrollPosition, new Rect(rect18)
                {
                    height = num5 + 8f
                });
                foreach (Trait allTrait in this.newAndroid.story.traits.allTraits)
                {
                    Trait trait = allTrait;
                    Rect rect19 = new Rect(rect13.xMax, y7, width, height1);
                    Widgets.DrawBox(rect19);
                    Widgets.DrawHighlightIfMouseover(rect19);
                    Rect rect20 = new Rect(rect19);
                    rect20.width -= rect20.height;
                    Rect butRect = new Rect(rect19);
                    butRect.width = butRect.height;
                    butRect.x = rect20.xMax;
                    if (this.originalTraits.Any<Trait>((Predicate<Trait>)(otherTrait => otherTrait.def == trait.def && otherTrait.Degree == trait.Degree)))
                        Widgets.Label(rect20, "<" + trait.LabelCap + ">");
                    else
                        Widgets.Label(rect20, trait.LabelCap);
                    TooltipHandler.TipRegion(rect20, (TipSignal)trait.TipString(this.newAndroid));
                    if (Widgets.ButtonInvisible(rect20))
                        this.PickTraitMenu(trait);
                    if (Widgets.ButtonImage(butRect, TexCommand.ForbidOn))
                        trait1 = trait;
                    y7 += 26f;
                }
                Verse.Text.Anchor = TextAnchor.MiddleRight;
                Rect source2 = new Rect(rect13.xMax, y7, width, height1);
                Rect rect21 = new Rect(source2);
                rect21.width -= rect21.height;
                Rect butRect1 = new Rect(source2);
                butRect1.width = butRect1.height;
                butRect1.x = rect21.xMax;
                Widgets.Label(rect21, "AndroidCustomizationAddTraitLabel".Translate((NamedArgument)this.newAndroid.story.traits.allTraits.Count, (NamedArgument)AndroidCustomizationTweaks.maxTraitsToPick));
                if (Widgets.ButtonImage(butRect1, TexCommand.Install) && this.newAndroid.story.traits.allTraits.Count < AndroidCustomizationTweaks.maxTraitsToPick)
                    this.PickTraitMenu((Trait)null);
                Widgets.EndScrollView();
                Verse.Text.Anchor = TextAnchor.UpperLeft;
                if (trait1 != null)
                {
                    this.newAndroid.story.traits.allTraits.Remove(trait1);
                    this.RefreshPawn();
                }
                //Upgrades
                float y8 = 32f;
                float num6 = inRect.width - CustomizeAndroidWindow.upgradesOffset;
                float height2 = 32f;
                Rect rect22 = new Rect(CustomizeAndroidWindow.upgradesOffset, y8, num6, height2);
                Verse.Text.Font = GameFont.Medium;
                Verse.Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(rect22, "AndroidCustomizationUpgrades".Translate());
                Widgets.DrawLineHorizontal(rect22.x, rect22.y + 32f, rect22.width);
                Verse.Text.Font = GameFont.Small;
                Verse.Text.Anchor = TextAnchor.UpperLeft;
                float y9 = y8 + 35f;
                Rect upgradeSize = new Rect(0.0f, 0.0f, (float)AndroidCustomizationTweaks.upgradeBaseSize, (float)AndroidCustomizationTweaks.upgradeBaseSize);
                int num7 = (int)Math.Floor((double)num6 / (double)upgradeSize.width);
                Rect rect23 = new Rect(CustomizeAndroidWindow.upgradesOffset, y9, num6, inRect.height - height2);
                float num8 = 0.0f;
                foreach (AndroidUpgradeGroupDef allDef in DefDatabase<AndroidUpgradeGroupDef>.AllDefs)
                {
                    num8 += allDef.calculateNeededHeight(upgradeSize, num6);
                    num8 += 52f;
                }
                Widgets.BeginScrollView(rect23, ref this.upgradesScrollPosition, new Rect(rect23)
                {
                    height = num8
                });
                foreach (AndroidUpgradeGroupDef androidUpgradeGroupDef in (IEnumerable<AndroidUpgradeGroupDef>)DefDatabase<AndroidUpgradeGroupDef>.AllDefs.OrderBy<AndroidUpgradeGroupDef, int>((Func<AndroidUpgradeGroupDef, int>)(upgradeGroup => upgradeGroup.orderID)))
                {
                    Rect rect24 = new Rect(rect22);
                    rect24.y = y9;
                    rect24.height = 22f;
                    float num9 = y9 + 30f;
                    Verse.Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.DrawTitleBG(rect24);
                    Widgets.Label(rect24, androidUpgradeGroupDef.label);
                    Widgets.DrawLineHorizontal(rect24.x, rect24.y + 22f, rect24.width);
                    Verse.Text.Anchor = TextAnchor.UpperLeft;
                    float neededHeight = androidUpgradeGroupDef.calculateNeededHeight(upgradeSize, num6);
                    int num10 = 0;
                    float num11 = 0.0f;
                    foreach (AndroidUpgradeDef androidUpgradeDef1 in (IEnumerable<AndroidUpgradeDef>)androidUpgradeGroupDef.Upgrades.OrderBy<AndroidUpgradeDef, int>((Func<AndroidUpgradeDef, int>)(upgradeSubGroup => upgradeSubGroup.orderID)))
                    {
                        AndroidUpgradeDef upgrade = androidUpgradeDef1;
                        if (num10 >= num7)
                        {
                            num10 = 0;
                            num11 += upgradeSize.height;
                        }
                        Rect rect25 = new Rect(rect22.x + upgradeSize.width * (float)num10, num9 + num11, upgradeSize.width, upgradeSize.height);
                        bool needsFulfilled = false;
                        if (Mouse.IsOver(rect25))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            stringBuilder.AppendLine(upgrade.label);
                            stringBuilder.AppendLine();
                            stringBuilder.AppendLine(upgrade.description);
                            stringBuilder.AppendLine();
                            if (upgrade.hediffToApply != null && upgrade.hediffToApply.ConcreteExample != null)
                            {
                                //Using TipStringExtra will result in null reference hediff instance since there's no pawn have this hediff yet. Manually send null for Hediff instance solve this problem.
                                foreach (StatDrawEntry item in HediffStatsUtility.SpecialDisplayStats(upgrade.hediffToApply.ConcreteExample.CurStage, null))
                                {
                                    if (item.ShouldDisplay)
                                        stringBuilder.AppendLine("  - " + item.LabelCap + ": " + item.ValueString);
                                }
                                //stringBuilder.AppendLine(upgrade.hediffToApply.ConcreteExample.TipStringExtra.TrimEndNewlines());
                                stringBuilder.AppendLine();
                            }
                            if (upgrade.newBodyType != null)
                            {
                                stringBuilder.AppendLine((string)"AndroidCustomizationChangeBodyType".Translate());
                                stringBuilder.AppendLine();
                            }
                            if (upgrade.changeSkinColor)
                            {
                                stringBuilder.AppendLine((string)"AndroidCustomizationChangeSkinColor".Translate());
                                stringBuilder.AppendLine();
                            }
                            stringBuilder.AppendLine(this.androidPrinter.FormatIngredientCosts(out needsFulfilled, (IEnumerable<ThingOrderRequest>)upgrade.costList, false));
                            stringBuilder.AppendLine((string)("AndroidCustomizationTimeCost".Translate() + ": " + upgrade.extraPrintingTime.ToStringTicksToPeriodVerbose()));
                            if (upgrade.requiredResearch != null && !upgrade.requiredResearch.IsFinished)
                            {
                                stringBuilder.AppendLine();
                                stringBuilder.AppendLine((string)("AndroidCustomizationRequiredResearch".Translate() + ": " + upgrade.requiredResearch.LabelCap));
                            }
                            TooltipHandler.TipRegion(rect25, (TipSignal)stringBuilder.ToString());
                        }
                        bool flag = upgrade.requiredResearch == null ? this.appliedUpgradeCommands.Any<UpgradeCommand>((Predicate<UpgradeCommand>)(appUpgrade => appUpgrade.def != upgrade && appUpgrade.def.exclusivityGroups.Any<string>((Predicate<string>)(group => upgrade.exclusivityGroups.Contains(group))))) : !upgrade.requiredResearch.IsFinished || this.appliedUpgradeCommands.Any<UpgradeCommand>((Predicate<UpgradeCommand>)(appUpgrade => appUpgrade.def != upgrade && appUpgrade.def.exclusivityGroups.Any<string>((Predicate<string>)(group => upgrade.exclusivityGroups.Contains(group)))));
                        if (this.AlreadyUpgradedOnPawn(upgrade))
                        {
                            foreach (AndroidUpgradeDef androidUpgradeDef2 in (IEnumerable<AndroidUpgradeDef>)androidUpgradeGroupDef.Upgrades.OrderBy<AndroidUpgradeDef, int>((Func<AndroidUpgradeDef, int>)(upgradeSubGroup => upgradeSubGroup.orderID)))
                            {
                                if (androidUpgradeDef2 != upgrade && androidUpgradeDef2.exclusivityGroups.Any<string>((Predicate<string>)(group => upgrade.exclusivityGroups.Contains(group))))
                                {
                                    if (!this.upgradedDefsToDisable.NullOrEmpty<AndroidUpgradeDef>())
                                    {
                                        if (!this.upgradedDefsToDisable.Contains(androidUpgradeDef2))
                                            this.upgradedDefsToDisable.Add(androidUpgradeDef2);
                                    }
                                    else
                                        this.upgradedDefsToDisable.Add(androidUpgradeDef2);
                                }
                            }
                        }
                        if (!this.upgradedDefsToDisable.NullOrEmpty<AndroidUpgradeDef>() && this.upgradedDefsToDisable.Contains(upgrade))
                            flag = true;
                        if (flag)
                        {
                            Widgets.DrawRectFast(rect25, Color.red);
                        }
                        else
                        {
                            if (this.appliedUpgradeCommands.Any<UpgradeCommand>((Predicate<UpgradeCommand>)(upgradeCommand => upgradeCommand.def == upgrade)))
                                Widgets.DrawRectFast(rect25, Color.white);
                            if (this.AlreadyUpgradedOnPawn(upgrade))
                                Widgets.DrawRectFast(rect25, Color.white);
                        }
                        if (upgrade.iconTexturePath != null)
                            Widgets.DrawTextureFitted(rect25.ContractedBy(3f), (Texture)ContentFinder<Texture2D>.Get(upgrade.iconTexturePath), 1f);
                        Widgets.DrawHighlightIfMouseover(rect25);
                        UpgradeCommand upgradeCommand1 = this.appliedUpgradeCommands.FirstOrDefault<UpgradeCommand>((Func<UpgradeCommand, bool>)(upgradeCommand => upgradeCommand.def == upgrade));
                        if (!flag && Widgets.ButtonInvisible(rect25) && !this.AlreadyUpgradedOnPawn(upgrade))
                        {
                            if (upgradeCommand1 != null)
                            {
                                upgradeCommand1.Undo();
                                this.appliedUpgradeCommands.Remove(upgradeCommand1);
                            }
                            else
                            {
                                UpgradeCommand upgradeCommand2 = UpgradeMaker.Make(upgrade, this);
                                upgradeCommand2.Apply();
                                upgradeCommand2.Notify_UpgradeAdded();
                                this.appliedUpgradeCommands.Add(upgradeCommand2);
                            }
                            this.RefreshCosts();
                        }
                        upgradeCommand1?.ExtraOnGUI(rect25);
                        ++num10;
                    }
                    y9 = num9 + (neededHeight + 22f);
                }
                Widgets.EndScrollView();
            }
            Verse.Text.Anchor = TextAnchor.UpperLeft;
        }

        public void PickTraitMenu(Trait oldTrait)
        {
            this.allTraits.Clear();
            foreach (TraitDef def in DefDatabase<TraitDef>.AllDefsListForReading)
            {
                foreach (TraitDegreeData degreeData in def.degreeDatas)
                    this.allTraits.Add(new Trait(def, degreeData.degree));
            }
            if (this.newAndroid.def is ThingDef_AlienRace def1)
            {
                List<TraitDef> traitDefList1;
                if (def1 == null)
                {
                    traitDefList1 = (List<TraitDef>)null;
                }
                else
                {
                    ThingDef_AlienRace.AlienSettings alienRace = def1.alienRace;
                    if (alienRace == null)
                    {
                        traitDefList1 = (List<TraitDef>)null;
                    }
                    else
                    {
                        GeneralSettings generalSettings = alienRace.generalSettings;
                        if (generalSettings == null)
                        {
                            traitDefList1 = (List<TraitDef>)null;
                        }
                        else
                        {
                            List<AlienTraitEntry> disallowedTraits = generalSettings.disallowedTraits;
                            traitDefList1 = disallowedTraits != null ? disallowedTraits.Select<AlienTraitEntry, TraitDef>((Func<AlienTraitEntry, TraitDef>)(trait => trait.defName)).ToList<TraitDef>() : (List<TraitDef>)null;
                        }
                    }
                }
                List<TraitDef> traitDefList2 = traitDefList1;
                if (traitDefList2 != null)
                {
                    foreach (TraitDef traitDef in traitDefList2)
                    {
                        TraitDef trait = traitDef;
                        this.allTraits.RemoveAll((Predicate<Trait>)(thisTrait => trait.defName == thisTrait.def.defName));
                    }
                }
            }
            foreach (Trait allTrait in this.newAndroid.story.traits.allTraits)
            {
                Trait trait = allTrait;
                this.allTraits.RemoveAll((Predicate<Trait>)(aTrait => aTrait.def == trait.def));
                this.allTraits.RemoveAll((Predicate<Trait>)(aTrait => trait.def.conflictingTraits.Contains(aTrait.def)));
            }
            FloatMenuUtility.MakeMenu<Trait>((IEnumerable<Trait>)this.allTraits, (Func<Trait, string>)(labelTrait => this.originalTraits.Any<Trait>((Predicate<Trait>)(originalTrait => originalTrait.def == labelTrait.def && originalTrait.Degree == labelTrait.Degree)) ? (string)"AndroidCustomizationOriginalTraitFloatMenu".Translate((NamedArgument)labelTrait.LabelCap) : labelTrait.LabelCap), (Func<Trait, Action>)(theTrait => (Action)(() =>
     {
         this.replacedTrait = oldTrait;
         this.newTrait = theTrait;
     })));
        }

        public void RefreshUpgrades()
        {
            foreach (UpgradeCommand appliedUpgradeCommand in this.appliedUpgradeCommands)
                appliedUpgradeCommand.Apply();
            this.refreshAndroidPortrait = true;
        }

        public void RefreshCosts()
        {
            this.finalCalculatedPrintingCost.Clear();
            this.finalExtraPrintingTimeCost = 0;
            PawnCrafterProperties modExtension = this.androidPrinter.def.GetModExtension<PawnCrafterProperties>();
            if (!this.IsUpgrade)
            {
                foreach (ThingOrderRequest cost in modExtension.costList)
                    this.finalCalculatedPrintingCost.Add(new ThingOrderRequest()
                    {
                        amount = cost.amount,
                        nutrition = cost.nutrition,
                        thingDef = cost.thingDef
                    });
            }
            List<ThingDef> source = new List<ThingDef>();
            foreach (UpgradeCommand appliedUpgradeCommand in this.appliedUpgradeCommands)
            {
                foreach (ThingOrderRequest cost in appliedUpgradeCommand.def.costList)
                {
                    ThingOrderRequest upgradeCost = cost;
                    ThingOrderRequest thingOrderRequest = this.finalCalculatedPrintingCost.FirstOrDefault<ThingOrderRequest>((Func<ThingOrderRequest, bool>)(finalCost =>
                   {
                       if (finalCost.thingDef == upgradeCost.thingDef)
                           return true;
                       return finalCost.nutrition && upgradeCost.nutrition;
                   }));
                    if (thingOrderRequest != null)
                        thingOrderRequest.amount += upgradeCost.amount;
                    else
                        this.finalCalculatedPrintingCost.Add(new ThingOrderRequest()
                        {
                            amount = upgradeCost.amount,
                            nutrition = upgradeCost.nutrition,
                            thingDef = upgradeCost.thingDef
                        });
                }
                source.AddRange((IEnumerable<ThingDef>)appliedUpgradeCommand.def.costsNotAffectedByBodySize);
                this.finalExtraPrintingTimeCost += appliedUpgradeCommand.def.extraPrintingTime;
            }
            if (source.Count > 0)
                source = new List<ThingDef>(source.Distinct<ThingDef>());
            if (this._pawnTraits.NullOrEmpty<Trait>() && this.clonedPawn != null)
                this.pawnTraits(this.clonedPawn);
            //Hair Def/Color Change Extra Time
            int HairColorChangeExtratime = 5000;
            int HairDefChangeExtratime = 10000;
            if (this.newAndroid.story.hairDef != this.originalHairDef)
                this.finalExtraPrintingTimeCost += HairDefChangeExtratime;
            if (this.newAndroid.story.HairColor != this.originalHairColor)
                this.finalExtraPrintingTimeCost += HairColorChangeExtratime;
            //Trait Change Extra Time
            int num3 = 45000;
            int num4 = 0;
            foreach (Trait allTrait in this.newAndroid.story.traits.allTraits)
            {
                Trait trait = allTrait;
                num4 += num3;
                if (this._pawnTraits != null && this._pawnTraits.Contains(trait))
                    num4 -= num3;
                if (this.originalTraits.Any<Trait>((Predicate<Trait>)(originalTrait => originalTrait.def == trait.def && originalTrait.Degree == trait.Degree)))
                    num4 -= num3;
            }
            foreach (Trait originalTrait1 in this.originalTraits)
            {
                Trait originalTrait = originalTrait1;
                if ((this._pawnTraits == null || !this._pawnTraits.Contains(originalTrait)) && !this.newAndroid.story.traits.allTraits.Any<Trait>((Predicate<Trait>)(trait => originalTrait.def == trait.def && originalTrait.Degree == trait.Degree)))
                    num4 += num3;
            }
            this.finalExtraPrintingTimeCost += num4;
            foreach (ThingOrderRequest thingOrderRequest in this.finalCalculatedPrintingCost)
            {
                if (!source.Contains(thingOrderRequest.thingDef))
                    thingOrderRequest.amount = (float)Math.Ceiling((double)thingOrderRequest.amount * (double)this.newAndroid.def.race.baseBodySize);
            }
        }

        public void RefreshPawn()
        {
            this.newAndroid.Notify_DisabledWorkTypesChanged();
            this.newAndroid.skills.Notify_SkillDisablesChanged();
            this.RefreshSkills();
            this.RefreshUpgrades();
            this.RefreshCosts();
        }

        public void RefreshSkills(bool addBackstoryBonuses = false)
        {
            List<SkillDef> defsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int index = 0; index < defsListForReading.Count; ++index)
            {
                SkillDef skillDef = defsListForReading[index];
                int num1 = CustomizeAndroidWindow.FinalLevelOfSkill(this.newAndroid, skillDef);
                SkillRecord skill = this.newAndroid.skills.GetSkill(skillDef);
                skill.Level = num1;
                skill.passion = Passion.None;
                if (!skill.TotallyDisabled)
                {
                    float num2 = (float)num1 * 0.11f;
                    float num3 = Rand.Value;
                    if ((double)num3 < (double)num2)
                        skill.passion = (double)num3 >= (double)num2 * 0.20000000298023224 ? Passion.Minor : Passion.Major;
                    skill.xpSinceLastLevel = Rand.Range(skill.XpRequiredForLevelUp * 0.1f, skill.XpRequiredForLevelUp * 0.9f);
                }
            }
        }

        private static int FinalLevelOfSkill(Pawn pawn, SkillDef sk)
        {
            float x = !sk.usuallyDefinedInBackstories ? Rand.ByCurve(CustomizeAndroidWindow.LevelRandomCurve) : (float)Rand.RangeInclusive(0, 4);
            foreach (BackstoryDef backstory in pawn.story.AllBackstories.Where<BackstoryDef>((Func<BackstoryDef, bool>)(bs => bs != null)))
            {
                foreach (KeyValuePair<SkillDef, int> keyValuePair in backstory.skillGains)
                {
                    if (keyValuePair.Key == sk)
                        x += (float)keyValuePair.Value * Rand.Range(1f, 1.4f);
                }
            }
            for (int index = 0; index < pawn.story.traits.allTraits.Count; ++index)
            {
                int num = 0;
                if (pawn.story.traits.allTraits[index].CurrentData.skillGains.TryGetValue(sk, out num))
                    x += (float)num;
            }
            return Mathf.Clamp(Mathf.RoundToInt(CustomizeAndroidWindow.LevelFinalAdjustmentCurve.Evaluate(x)), 0, 20);
        }

        public Pawn GetNewPawn(Gender gender = Gender.Female)
        {
            Pawn pawn;
            if (this.currentPawnKindDef.race != ThingDefOf.ChjAndroid)
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                PawnKindDef currentPawnKindDef = this.currentPawnKindDef;
                Faction faction = this.androidPrinter.Faction;
                Gender? nullable = new Gender?(gender);
                float? minChanceToRedressWorldPawn = new float?();
                float? fixedBiologicalAge = new float?();
                float? fixedChronologicalAge = new float?();
                Gender? fixedGender = nullable;
                FloatRange? excludeBiologicalAgeRange = new FloatRange?();
                FloatRange? biologicalAgeRange = new FloatRange?();
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, faction, forceGenerateNewPawn: true, canGeneratePawnRelations: false, colonistRelationChanceFactor: 0.0f, allowGay: false, allowPregnant: true, allowAddictions: false, forceRedressWorldPawnIfFormerColonist: true, minChanceToRedressWorldPawn: minChanceToRedressWorldPawn, fixedBiologicalAge: fixedBiologicalAge, fixedChronologicalAge: fixedChronologicalAge, fixedGender: fixedGender, excludeBiologicalAgeRange: excludeBiologicalAgeRange, biologicalAgeRange: biologicalAgeRange));
                HarmonyPatches.bypassGenerationOfUpgrades = false;
                AndroidUtility.Androidify(pawn);
                long num = 64800000;
                pawn.ageTracker.AgeBiologicalTicks = num;
                pawn.ageTracker.AgeChronologicalTicks = num;
            }
            else
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                PawnKindDef currentPawnKindDef = this.currentPawnKindDef;
                Faction faction = this.androidPrinter.Faction;
                Gender? nullable1 = new Gender?(gender);
                float? nullable2 = new float?(20f);
                float? nullable3 = new float?(20f);
                float? minChanceToRedressWorldPawn = new float?();
                float? fixedBiologicalAge = nullable2;
                float? fixedChronologicalAge = nullable3;
                Gender? fixedGender = nullable1;
                FloatRange? excludeBiologicalAgeRange = new FloatRange?();
                FloatRange? biologicalAgeRange = new FloatRange?();
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(currentPawnKindDef, faction, forceGenerateNewPawn: true, canGeneratePawnRelations: false, colonistRelationChanceFactor: 0.0f, allowGay: false, allowPregnant: true, allowAddictions: false, forceRedressWorldPawnIfFormerColonist: true, minChanceToRedressWorldPawn: minChanceToRedressWorldPawn, fixedBiologicalAge: fixedBiologicalAge, fixedChronologicalAge: fixedChronologicalAge, fixedGender: fixedGender, excludeBiologicalAgeRange: excludeBiologicalAgeRange, biologicalAgeRange: biologicalAgeRange));
                HarmonyPatches.bypassGenerationOfUpgrades = false;
            }
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();
            pawn.apparel.DestroyAll();
            if (pawn.apparel.CanWearWithoutDroppingAnything(ThingDefOf.ChJAndroidThermalBandages))
                pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(ThingDefOf.ChJAndroidThermalBandages, ThingDef.Named("Synthread")));
            if (pawn.workSettings != null)
                pawn.workSettings.EnableAndInitialize();
            if (pawn.skills != null)
                pawn.skills.Notify_SkillDisablesChanged();
            if (!pawn.Dead && pawn.RaceProps.Humanlike)
                pawn.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();
            this.originalTraits.Clear();
            foreach (Trait allTrait in pawn.story.traits.allTraits)
                this.originalTraits.Add(new Trait(allTrait.def, allTrait.Degree, allTrait.ScenForced));
            return pawn;
        }

        public Pawn GetClone(Pawn sourcePawn)
        {
            PawnKindDef kindDef = sourcePawn.kindDef;
            Faction faction1 = FactionUtility.DefaultFactionFrom(kindDef.defaultFactionType);
            Pawn pawn;
            if (kindDef.race != ThingDefOf.ChjAndroid)
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef));
                HarmonyPatches.bypassGenerationOfUpgrades = false;
                AndroidUtility.Androidify(pawn);
                LifeStageAge lifeStageAge = pawn.RaceProps.lifeStageAges.Last<LifeStageAge>();
                if (lifeStageAge != null)
                {
                    long num = (long)Math.Ceiling((double)lifeStageAge.minAge) * 3600000L;
                    pawn.ageTracker.AgeBiologicalTicks = num;
                    pawn.ageTracker.AgeChronologicalTicks = num;
                }
                else
                {
                    long num = (long)((double)pawn.RaceProps.lifeExpectancy * 3600000.0 * 0.20000000298023224);
                    pawn.ageTracker.AgeBiologicalTicks = num;
                    pawn.ageTracker.AgeChronologicalTicks = num;
                }
            }
            else
            {
                HarmonyPatches.bypassGenerationOfUpgrades = true;
                pawn = PawnGenerator.GeneratePawn(new PawnGenerationRequest(kindDef));
                HarmonyPatches.bypassGenerationOfUpgrades = false;
            }
            pawn.needs.SetInitialLevels();
            pawn.ageTracker = sourcePawn.ageTracker;
            CustomizeAndroidWindow.PawnUpdate(pawn, sourcePawn);
            pawn?.equipment.DestroyAllEquipment();
            pawn?.inventory.DestroyAll();
            pawn.apparel.DestroyAll();
            if (pawn.apparel.CanWearWithoutDroppingAnything(ThingDefOf.ChJAndroidThermalBandages))
                pawn.apparel.Wear((Apparel)ThingMaker.MakeThing(ThingDefOf.ChJAndroidThermalBandages, ThingDef.Named("Synthread")));
            return pawn;
        }

        public static void PawnUpdate(Pawn pawn, Pawn sourcePawn)
        {
            CustomizeAndroidWindow.GenerateSkillsFromSourcePawn(pawn, sourcePawn);
            CustomizeAndroidWindow.GenerateBioFromSource(pawn, sourcePawn);
            pawn.story.Childhood = sourcePawn.story.Childhood;
            pawn.story.Adulthood = sourcePawn.story.Adulthood;
            pawn.story.headType = sourcePawn.story.headType;
            pawn.story.bodyType = sourcePawn.story.bodyType;
            pawn.story.hairDef = sourcePawn.story.hairDef;
            pawn.story.HairColor = sourcePawn.story.HairColor;
            CustomizeAndroidWindow.GenerateTraitsFromSourcePawn(pawn, sourcePawn);
            CustomizeAndroidWindow.GenerateHediffsFromSourcePawn(pawn, sourcePawn);
            if (pawn.skills != null)
                pawn.skills.Notify_SkillDisablesChanged();
            if (pawn.workSettings == null)
                return;
            pawn.workSettings.EnableAndInitialize();
        }

        public static void GenerateSkillsFromSourcePawn(Pawn pawn, Pawn sourcePawn)
        {
            List<SkillDef> defsListForReading = DefDatabase<SkillDef>.AllDefsListForReading;
            for (int index = 0; index < defsListForReading.Count; ++index)
            {
                SkillDef skillDef = defsListForReading[index];
                pawn.skills.GetSkill(skillDef).Level = sourcePawn.skills.GetSkill(skillDef).Level;
                pawn.skills.GetSkill(skillDef).passion = sourcePawn.skills.GetSkill(skillDef).passion;
            }
        }

        public static void GenerateTraitsFromSourcePawn(Pawn pawn, Pawn sourcePawn)
        {
            List<Trait> traitList1 = new List<Trait>();
            List<Trait> traitList2 = new List<Trait>();
            foreach (Trait allTrait1 in pawn.story.traits.allTraits)
            {
                foreach (Trait allTrait2 in sourcePawn.story.traits.allTraits)
                {
                    if (!pawn.story.traits.allTraits.Contains(allTrait2))
                        traitList2.Add(allTrait2);
                    if (!sourcePawn.story.traits.allTraits.Contains(allTrait1))
                        traitList1.Add(allTrait1);
                }
            }
            foreach (Trait trait in traitList1)
                pawn.story.traits.allTraits.Remove(trait);
            foreach (Trait trait in traitList2)
            {
                if (!pawn.story.traits.HasTrait(trait.def))
                    pawn.story.traits.allTraits.Add(trait);
            }
        }

        public static void GenerateHediffsFromSourcePawn(Pawn pawn, Pawn sourcePawn)
        {
            List<Hediff> hediffList1 = new List<Hediff>();
            List<Hediff> hediffList2 = new List<Hediff>();
            foreach (Hediff hediff1 in pawn.health.hediffSet.hediffs)
            {
                foreach (Hediff hediff2 in sourcePawn.health.hediffSet.hediffs)
                {
                    if (!pawn.health.hediffSet.HasHediff(hediff2.def))
                        hediffList2.Add(hediff2);
                    if (!sourcePawn.health.hediffSet.HasHediff(hediff1.def))
                        hediffList1.Add(hediff1);
                }
            }
            foreach (Hediff hediff in hediffList1)
                pawn.health.hediffSet.hediffs.Remove(hediff);
            foreach (Hediff hediff in hediffList2)
                pawn.health.hediffSet.hediffs.Add(hediff);
        }

        public static void GenerateBioFromSource(Pawn pawn, Pawn sourcePawn)
        {
            sourcePawn.Name = (Name)CustomizeAndroidWindow.CurPawnName(sourcePawn);
            NameTriple name = sourcePawn.Name as NameTriple;
            pawn.Name = (Name)name;
        }

        private static NameTriple CurPawnName(Pawn pawn)
        {
            if (pawn.Name is NameTriple name)
                return new NameTriple(name.First, name.Nick, name.Last);
            throw new InvalidOperationException();
        }
    }
}
