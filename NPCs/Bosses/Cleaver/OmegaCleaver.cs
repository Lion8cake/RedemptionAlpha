using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Biomes;
using Redemption.Buffs.Debuffs;
using Redemption.Buffs.NPCBuffs;
using Redemption.Globals;
using Redemption.Items.Donator.Gonk;
using Redemption.Items.Donator.Uncon;
using Redemption.Items.Placeable.Trophies;
using Redemption.Items.Usable;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;
using Redemption.BaseExtension;
using Redemption.NPCs.Bosses.Obliterator;
using Redemption.Items.Armor.Vanity;
using Redemption.Items.Materials.HM;

namespace Redemption.NPCs.Bosses.Cleaver
{
    [AutoloadBossHead]
    public class OmegaCleaver : ModNPC
    {
        public float[] oldrot = new float[6];

        public enum ActionState
        {
            Intro,
            Begin,
            Idle,
            Attacks,
            Death
        }

        public ActionState AIState
        {
            get => (ActionState)NPC.ai[0];
            set => NPC.ai[0] = (int)value;
        }

        public ref float AITimer => ref NPC.ai[2];

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
            NPCID.Sets.TrailCacheLength[NPC.type] = 6;
            NPCID.Sets.TrailingMode[NPC.type] = 1;

            NPCID.Sets.MPAllowedEnemies[Type] = true;
            NPCID.Sets.BossBestiaryPriority.Add(Type);

            NPCDebuffImmunityData debuffData = new()
            {
                SpecificallyImmuneTo = new int[] {
                    BuffID.Confused,
                    BuffID.Poisoned,
                    BuffID.Venom,
                    ModContent.BuffType<InfestedDebuff>(),
                    ModContent.BuffType<NecroticGougeDebuff>(),
                    ModContent.BuffType<ViralityDebuff>(),
                    ModContent.BuffType<DirtyWoundDebuff>()
                }
            };
            NPCID.Sets.DebuffImmunitySets.Add(Type, debuffData);

            NPCID.Sets.NPCBestiaryDrawModifiers value = new(0)
            {
                CustomTexturePath = "Redemption/Textures/Bestiary/OmegaCleaver_Bestiary"
            };
            NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, value);
        }

        public override void SetDefaults()
        {
            NPC.width = 98;
            NPC.height = 280;
            NPC.friendly = false;
            NPC.damage = 110;
            NPC.defense = 60;
            NPC.lifeMax = 55000;
            NPC.HitSound = SoundID.NPCHit4;
            NPC.DeathSound = SoundID.NPCDeath14;
            NPC.npcSlots = 10f;
            NPC.SpawnWithHigherTime(30);
            NPC.value = Item.buyPrice(0, 15, 0, 0);
            NPC.boss = true;
            NPC.knockBackResist = 0.0f;
            NPC.aiStyle = -1;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            if (!Main.dedServ)
                Music = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/BossOmega1");
            SpawnModBiomes = new int[2] { ModContent.GetInstance<LidenBiomeOmega>().Type, ModContent.GetInstance<LidenBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement> {
                new FlavorTextBestiaryInfoElement("A proof-of-concept weapon of war modified by Girus, the Omega Cleaver is a giant sword controlled by a small remote stick that was wielded by another machine.")
            });
        }

        public override void HitEffect(int hitDirection, double damage)
        {
            if (NPC.life <= 0)
            {
                if (!Main.dedServ)
                    SoundEngine.PlaySound(CustomSounds.MissileExplosion, NPC.position);
                RedeDraw.SpawnExplosion(NPC.Center, Color.OrangeRed);

                for (int i = 0; i < 80; i++)
                {
                    int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LifeDrain, 0f, 0f, 100, default, 3f);
                    Main.dust[dustIndex].velocity *= 1.9f;
                }
                for (int i = 0; i < 45; i++)
                {
                    int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.SparksMech, 0f, 0f, 100, default, 2f);
                    Main.dust[dustIndex].velocity *= 1.8f;
                }
                for (int i = 0; i < 25; i++)
                {
                    int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default, 2f);
                    Main.dust[dustIndex].velocity *= 1.8f;
                }
            }
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.LifeDrain, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
            Dust.NewDust(NPC.position + NPC.velocity, NPC.width, NPC.height, DustID.LifeDrain, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f);
        }

        public override void OnKill()
        {
            if (!RedeBossDowned.downedOmega1)
            {
                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, Vector2.Zero, ModContent.ProjectileType<OmegaCleaver_GirusTalk>(), 0, 0, Main.myPlayer);
                Item.NewItem(NPC.GetSource_Loot(), NPC.getRect(), ModContent.ItemType<UnconPetItem>());
            }

            NPC.SetEventFlagCleared(ref RedeBossDowned.downedOmega1, -1);
        }

        public override void ModifyNPCLoot(NPCLoot npcLoot)
        {
            npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<OmegaCleaverBag>()));
            npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<OmegaTrophy>(), 10));

            npcLoot.Add(ItemDropRule.MasterModeCommonDrop(ModContent.ItemType<CleaverRelic>()));

            LeadingConditionRule notExpertRule = new(new Conditions.NotExpert());

            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<SwordHeadband>(), 7));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<GonkPet>(), 10));
            notExpertRule.OnSuccess(ItemDropRule.Common(ModContent.ItemType<CorruptedXenomite>(), 1, 4, 8));

            npcLoot.Add(notExpertRule);
        }

        public override void BossLoot(ref string name, ref int potionType)
        {
            potionType = ItemID.GreaterHealingPotion;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            base.SendExtraAI(writer);
            if (Main.netMode == NetmodeID.Server || Main.dedServ)
            {
                writer.Write(rot);
                writer.Write(repeat);
            }
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            base.ReceiveExtraAI(reader);
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                rot = (float)reader.ReadDouble();
                repeat = reader.ReadInt32();
            }
        }

        public float rot;
        public int repeat;

        public override bool CanHitPlayer(Player target, ref int cooldownSlot) => false;

        public override void ScaleExpertStats(int numPlayers, float bossLifeScale)
        {
            NPC.lifeMax = (int)(NPC.lifeMax * 0.6f * bossLifeScale);
            NPC.damage = (int)(NPC.damage * 0.6f);
        }

        public override bool CheckActive()
        {
            Player player = Main.player[NPC.target];
            return !player.active || player.dead || Main.dayTime;
        }

        public override void AI()
        {
            if (AIState >= ActionState.Idle && AIState != ActionState.Death)
                NPC.dontTakeDamage = false;
            else
                NPC.dontTakeDamage = true;

            NPC host = Main.npc[(int)NPC.ai[3]];
            DespawnHandler();
            Player player = Main.player[NPC.target];
            if (NPC.target < 0 || NPC.target == 255 || player.dead || !player.active)
                NPC.TargetClosest();

            Vector2 DefaultPos = new(host.spriteDirection == 1 ? -180 : 180, -60);
            Vector2 PosPlayer = new(NPC.Center.X > player.Center.X ? 300 : -300, -80);
            Vector2 PosPlayer3 = new(NPC.Center.X > player.Center.X ? 200 : -200, -160);
            Vector2 PosPlayer3Check = new(NPC.Center.X > player.Center.X ? player.Center.X + 200 : player.Center.X - 200, player.Center.Y - 160);

            if (!RedeHelper.AnyProjectiles(ModContent.ProjectileType<CleaverHitbox>()))
                NPC.Shoot(NPC.Center, ModContent.ProjectileType<CleaverHitbox>(), NPC.damage, Vector2.Zero, false, SoundID.Item1, NPC.whoAmI);

            if (!player.active || player.dead)
                return;

            if (NPC.AnyNPCs(ModContent.NPCType<Wielder>()))
            {
                switch (AIState)
                {
                    case ActionState.Intro:
                        {
                            NPC.rotation = host.spriteDirection == 1 ? (float)-Math.PI / 2 : (float)Math.PI / 2;
                            NPC.velocity.X = host.spriteDirection == 1 ? 40 : -40;
                            if (NPC.Distance(host.Center) < 200)
                            {
                                for (int i = 0; i < 30; i++)
                                {
                                    int dustIndex = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.LifeDrain, 0f, 0f, 100, default, 3f);
                                    Main.dust[dustIndex].velocity *= 10f;
                                    Main.dust[dustIndex].noGravity = true;
                                }
                                SoundEngine.PlaySound(SoundID.Item74, NPC.position);
                                host.ai[3] = 1;
                                NPC.velocity *= 0;
                                AIState = ActionState.Begin;
                                NPC.netUpdate = true;
                            }
                        }
                        break;

                    case ActionState.Begin:
                        AITimer++;
                        if (!Main.dedServ)
                            RedeSystem.Instance.TitleCardUIElement.DisplayTitle("Omega Cleaver", 60, 90, 0.8f, 0, Color.Red,
                                "1st Omega Prototype");
                        player.RedemptionScreen().Rumble(20, 7);
                        rot = NPC.rotation;
                        if (AITimer > 20)
                        {
                            NPC.Shoot(new Vector2(NPC.Center.X - (120 * 16) - 10, NPC.Center.Y + 8), ModContent.ProjectileType<OOBarrier>(), 0, Vector2.Zero, false, SoundID.Item1, 0, 1);

                            NPC.Shoot(new Vector2(NPC.Center.X + (120 * 16) + 26, NPC.Center.Y + 8), ModContent.ProjectileType<OOBarrier>(), 0, Vector2.Zero, false, SoundID.Item1, 0, -1);

                            ArenaWorld.arenaBoss = "OC";
                            ArenaWorld.arenaTopLeft = new Vector2(NPC.Center.X - (120 * 16) + 8, NPC.Center.Y - (800 * 16) + 8);
                            ArenaWorld.arenaSize = new Vector2(240 * 16, 1600 * 16);
                            ArenaWorld.arenaMiddle = NPC.Center;
                            ArenaWorld.arenaActive = true;

                            AIState = ActionState.Idle;
                            AITimer = 0;
                            NPC.netUpdate = true;
                            if (Main.netMode == NetmodeID.Server && NPC.whoAmI < Main.maxNPCs)
                                NetMessage.SendData(MessageID.SyncNPC, number: NPC.whoAmI);
                        }
                        break;

                    case ActionState.Idle:
                        if (host.ai[0] == 3)
                        {
                            NPC.ai[1] = 0;
                            AITimer = 0;
                            NPC.MoveToNPC(host, DefaultPos, 24, 20);
                            if (NPC.Distance(host.Center) < 200 || host.ai[3] == 2)
                            {
                                host.ai[3] = 2;
                                rot.SlowRotation(0, (float)Math.PI / 60f);
                                NPC.rotation = rot;
                            }
                            else
                            {
                                NPC.rotation += NPC.velocity.X / 50;
                                rot = NPC.rotation;
                            }
                        }
                        if (host.ai[3] >= 3)
                            AIState = ActionState.Attacks;
                        break;

                    case ActionState.Attacks:
                        if (host.ai[0] == 3)
                            AIState = ActionState.Idle;
                        switch (host.ai[3])
                        {
                            case 3:
                                AITimer++;
                                if (AITimer < 80)
                                {
                                    NPC.MoveToNPC(host, DefaultPos, 8, 2);
                                    rot.SlowRotation(NPC.DirectionTo(host.Center).ToRotation() - 1.57f, (float)Math.PI / 30f);
                                    NPC.rotation = rot;
                                }
                                else
                                {
                                    NPC.MoveToNPC(host, DefaultPos, 16, 1);
                                    AITimer = 100;
                                    NPC.rotation = NPC.DirectionTo(host.Center).ToRotation() - 1.57f;
                                }
                                break;
                            #region Swing
                            case 4:
                                rot = NPC.rotation;
                                NPC.rotation = NPC.DirectionTo(host.Center).ToRotation() - 1.57f;
                                if (AITimer == 0 || AITimer >= 100)
                                {
                                    SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                                    NPC.velocity = NPC.DirectionTo(host.Center).RotatedBy(NPC.spriteDirection == 1 ? -Math.PI / 2 : Math.PI / 2) * 40;
                                    AITimer = 1;
                                }
                                else
                                {
                                    AITimer++;
                                    if (AITimer < 20)
                                    {
                                        NPC.Shoot(new Vector2(NPC.Center.X, NPC.Center.Y) + RedeHelper.PolarVector(134, NPC.rotation + (float)-Math.PI / 2), ModContent.ProjectileType<OmegaBlast>(), (int)(NPC.damage * 0.7f), RedeHelper.PolarVector(2, NPC.rotation + (float)-Math.PI / 2), false, SoundID.Item1);
                                        NPC.velocity -= NPC.velocity.RotatedBy(Math.PI / 2) * NPC.velocity.Length() / NPC.Distance(host.Center);
                                    }
                                    else
                                        NPC.velocity *= .7f;
                                }

                                break;
                            #endregion
                            #region Stab
                            case 5:
                                if (AITimer == 0 || AITimer >= 100)
                                {
                                    AITimer = 1;
                                    NPC.netUpdate = true;
                                }
                                else
                                {
                                    AITimer++;
                                    if (AITimer < 50)
                                    {
                                        NPC.velocity *= .94f;
                                        rot.SlowRotation(NPC.DirectionTo(player.Center).ToRotation() + 1.57f, (float)Math.PI / 30f);
                                        NPC.rotation = rot;
                                    }
                                    else if (AITimer <= 70)
                                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + 1.57f;
                                    if (AITimer == 50)
                                        NPC.Dash(40, true, SoundID.Item74, player.Center);
                                    if (AITimer == 70 && RedeHelper.Chance(.4f))
                                    {
                                        host.ai[2] = 60;
                                        AITimer = 0;
                                        NPC.netUpdate = true;
                                    }
                                    if (AITimer > 70)
                                        NPC.velocity *= .94f;
                                }
                                break;
                            #endregion
                            #region Speen I
                            case 6:
                                if (AITimer == 0 || AITimer >= 100)
                                {
                                    AITimer = 1;
                                    NPC.netUpdate = true;
                                }
                                else
                                {
                                    AITimer++;
                                    if (AITimer % 10 == 0)
                                    {
                                        NPC.Shoot(new Vector2(NPC.Center.X, NPC.Center.Y) + RedeHelper.PolarVector(134, NPC.rotation + (float)-Math.PI / 2), ModContent.ProjectileType<OmegaBlast>(), (int)(NPC.damage * 0.7f), RedeHelper.PolarVector(5, NPC.rotation + (float)-Math.PI / 2), false, SoundID.Item1);
                                    }
                                    if (AITimer < 60)
                                        NPC.MoveToNPC(host, RedeHelper.PolarVector(-200, host.rotation), 18, 20);
                                    else
                                        NPC.MoveToNPC(host, RedeHelper.PolarVector(200, host.rotation), 18, 20);
                                    if (AITimer > 120)
                                    {
                                        AITimer = 1;
                                        NPC.netUpdate = true;
                                    }
                                    NPC.rotation += NPC.velocity.X / 30;
                                }
                                break;
                            #endregion
                            #region Swing Spin
                            case 7:
                                rot = NPC.rotation;
                                NPC.rotation = NPC.DirectionTo(host.Center).ToRotation() - 1.57f;
                                if ((AITimer == 0 || AITimer >= 100) && NPC.ai[1] == 0)
                                {
                                    SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                                    NPC.velocity = NPC.DirectionTo(host.Center).RotatedBy(NPC.spriteDirection == 1 ? -Math.PI / 2 : Math.PI / 2) * 30;
                                    AITimer = 1;
                                    NPC.ai[1] = 1;
                                }
                                else
                                {
                                    AITimer++;
                                    if (AITimer < 40)
                                        NPC.velocity -= NPC.velocity.RotatedBy(Math.PI / 2) * NPC.velocity.Length() / NPC.Distance(host.Center);
                                    else
                                        NPC.velocity *= .7f;
                                }
                                break;
                            #endregion
                            #region Sword Burst
                            case 8:
                                if ((AITimer == 0 || AITimer >= 100) && NPC.ai[1] == 0)
                                {
                                    AITimer = 1;
                                    NPC.ai[1] = 1;
                                    NPC.netUpdate = true;
                                }
                                else
                                {
                                    AITimer++;
                                    rot.SlowRotation(NPC.DirectionTo(player.Center).ToRotation() + 1.57f, (float)Math.PI / 30f);
                                    NPC.rotation = rot;
                                    if (AITimer < 100)
                                    {
                                        Vector2 PosPlayer2 = new(NPC.Center.X > player.Center.X ? 600 : -600, -80);
                                        if (NPC.Distance(PosPlayer2) < 300 || AITimer > 80)
                                        {
                                            AITimer = 100;
                                            NPC.velocity *= .96f;
                                        }
                                        else
                                            NPC.Move(PosPlayer2, 15, 5, true);
                                    }
                                    else
                                    {
                                        NPC.velocity *= .96f;
                                        if (AITimer >= 130 && AITimer % 5 == 0 && AITimer < 200)
                                            NPC.Shoot(NPC.Center, ModContent.ProjectileType<PhantomCleaver>(), (int)(NPC.damage * 0.9f), new Vector2(Main.rand.NextFloat(-6, 7), Main.rand.NextFloat(-6, 7)), false, SoundID.Item1);
                                    }
                                }
                                break;
                            #endregion
                            #region Red Prism
                            case 9:
                                if (NPC.ai[1] == 0)
                                {
                                    AITimer = 0;
                                    if (player.Center.X > NPC.Center.X)
                                        NPC.ai[1] = 1;
                                    else
                                        NPC.ai[1] = 2;
                                    SoundEngine.PlaySound(CustomSounds.NebSound2, NPC.position);
                                }
                                else
                                {
                                    AITimer++;
                                    if (AITimer < 100)
                                    {
                                        rot.SlowRotation(0, (float)Math.PI / 40f);
                                        NPC.rotation = rot;
                                        if (NPC.Distance(PosPlayer) < 300 || AITimer > 40)
                                        {
                                            NPC.rotation = 0;
                                            NPC.Shoot(NPC.Center + RedeHelper.PolarVector(134, NPC.rotation + (float)-Math.PI / 2), ModContent.ProjectileType<RedPrism>(), (int)(NPC.damage * 0.9f), RedeHelper.PolarVector(9, NPC.rotation + (float)-Math.PI / 2), false, SoundID.Item1, ai0: NPC.whoAmI);
                                            AITimer = 100;
                                        }
                                        else
                                            NPC.Move(PosPlayer, 18, 5, true);
                                    }
                                    else
                                    {
                                        if (NPC.ai[1] == 1)
                                            NPC.rotation += 0.01f;
                                        else
                                            NPC.rotation -= 0.01f;
                                        NPC.rotation *= 1.01f;
                                        NPC.velocity *= .9f;
                                    }
                                }
                                break;
                            #endregion
                            #region Blade Pillars
                            case 10:
                                switch (NPC.ai[1])
                                {
                                    case 0:
                                        AITimer = 0;
                                        NPC.ai[1] = 1;
                                        NPC.netUpdate = true;
                                        break;
                                    case 1:
                                        AITimer++;
                                        if (AITimer < 60)
                                        {
                                            NPC.rotation += NPC.velocity.X / 30;
                                            rot = NPC.rotation;
                                        }
                                        else
                                        {
                                            rot.SlowRotation((float)Math.PI, (float)Math.PI / 20f);
                                            NPC.rotation = rot;
                                        }
                                        if (AITimer > 80)
                                        {
                                            NPC.rotation = (float)Math.PI;
                                            NPC.ai[1] = 2;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        else
                                            NPC.Move(new Vector2(0, -400), NPC.Distance(player.Center) < 700 ? 18 : 35, 3, true);
                                        break;
                                    case 2:
                                        AITimer++;
                                        if (AITimer < 50)
                                            NPC.velocity *= 0.9f;
                                        if (AITimer == 20)
                                        {
                                            SoundEngine.PlaySound(CustomSounds.OODashReady with { Pitch = 0.9f }, NPC.position);
                                            NPC.Shoot(NPC.Center, ModContent.ProjectileType<PhantomCleaver2_Spawner>(), (int)(NPC.damage * 0.9f), Vector2.Zero, false, SoundID.Item1);
                                        }

                                        if (AITimer == 50)
                                            NPC.velocity.Y = 20;

                                        if (NPC.Center.Y > player.Center.Y + 1000)
                                        {
                                            NPC.velocity *= 0f;
                                            host.ai[2] = 800;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                }
                                break;
                            #endregion
                            #region Twin Blade Slice
                            case 11:
                                switch (NPC.ai[1])
                                {
                                    case 0:
                                        AITimer = 0;
                                        NPC.ai[1] = 1;
                                        NPC.netUpdate = true;
                                        break;
                                    case 1:
                                        AITimer++;
                                        rot.SlowRotation(player.Center.X > NPC.Center.X ? 0.78f : 5.49f, (float)Math.PI / 20f);
                                        NPC.rotation = rot;
                                        if (NPC.Distance(PosPlayer3Check) < 30 || AITimer > 60)
                                        {
                                            SoundEngine.PlaySound(CustomSounds.OODashReady, NPC.position);
                                            NPC.rotation = player.Center.X > NPC.Center.X ? 0.78f : 5.49f;
                                            repeat = player.Center.X > NPC.Center.X ? 0 : 1;
                                            NPC.ai[1] = 2;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        else
                                            NPC.Move(PosPlayer3, NPC.Distance(player.Center) < 700 ? 16 : 35, 3, true);
                                        break;
                                    case 2:
                                        AITimer++;
                                        NPC.velocity *= 0f;
                                        if (AITimer > 10)
                                        {
                                            SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                                            host.ai[2] = 200;
                                            NPC.ai[1] = 3;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                    case 3:
                                        AITimer++;
                                        if (AITimer == 1)
                                            NPC.velocity.X = repeat == 0 ? 15 : -15;
                                        if (AITimer == 11)
                                            NPC.velocity.X = repeat == 0 ? -15 : 15;

                                        NPC.velocity.Y = 26;
                                        NPC.rotation += repeat == 0 ? 0.1f : -0.1f;
                                        if (AITimer > 20)
                                        {
                                            host.ai[2] = 300;
                                            SoundEngine.PlaySound(SoundID.Item74, NPC.position);
                                            rot = NPC.rotation;
                                            NPC.velocity *= 0f;
                                            NPC.ai[1] = 4;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                    case 4:
                                        AITimer++;
                                        NPC.velocity.X = repeat == 0 ? 40 : -40;
                                        NPC.rotation = (float)Math.Atan2(NPC.velocity.Y, NPC.velocity.X) + 1.57f;
                                        if (repeat == 0)
                                        {
                                            if (NPC.Center.X > player.Center.X + 200)
                                            {
                                                NPC.ai[1] = 5;
                                                AITimer = 0;
                                                NPC.netUpdate = true;
                                            }
                                        }
                                        else
                                        {
                                            if (NPC.Center.X < player.Center.X - 200)
                                            {
                                                NPC.ai[1] = 5;
                                                AITimer = 0;
                                                NPC.netUpdate = true;
                                            }
                                        }
                                        break;
                                    case 5:
                                        AITimer++;
                                        if (AITimer < 30)
                                        {
                                            NPC.rotation += NPC.velocity.X / 60;
                                            rot = NPC.rotation;
                                        }
                                        else
                                        {
                                            rot.SlowRotation(repeat == 0 ? 5.49f : 0.78f, (float)Math.PI / 20f);
                                            NPC.rotation = rot;
                                        }
                                        if (NPC.Distance(PosPlayer3Check) < 30 || AITimer > 60)
                                        {
                                            SoundEngine.PlaySound(CustomSounds.OODashReady, NPC.position);
                                            NPC.rotation = repeat == 0 ? 5.49f : 0.78f;
                                            repeat = player.Center.X > NPC.Center.X ? 0 : 1;
                                            NPC.ai[1] = 6;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        else
                                            NPC.Move(PosPlayer3, NPC.Distance(player.Center) < 700 ? 16 : 35, 3, true);
                                        break;
                                    case 6:
                                        AITimer++;
                                        NPC.velocity *= 0f;
                                        if (AITimer > 10)
                                        {
                                            host.ai[2] = 200;
                                            SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                                            NPC.ai[1] = 7;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                    case 7:
                                        AITimer++;
                                        if (AITimer == 1)
                                            NPC.velocity.X = repeat == 0 ? 15 : -15;
                                        if (AITimer == 11)
                                            NPC.velocity.X = repeat == 0 ? -15 : 15;

                                        NPC.velocity.Y = 26;
                                        NPC.rotation += repeat == 0 ? 0.1f : -0.1f;
                                        if (AITimer > 20)
                                        {
                                            if (NPC.life < (int)(NPC.lifeMax * 0.6f))
                                            {
                                                rot = NPC.rotation;
                                                NPC.velocity *= 0f;
                                                AITimer = 0;
                                                NPC.ai[1] = 8;
                                                NPC.netUpdate = true;
                                            }
                                            else
                                            {
                                                repeat = 0;
                                                NPC.velocity *= 0f;
                                                host.ai[2] = 1000;
                                                AITimer = 0;
                                                NPC.netUpdate = true;
                                            }
                                        }
                                        break;
                                    case 8:
                                        rot.SlowRotation(0, (float)Math.PI / 20f);
                                        NPC.rotation = rot;
                                        if (NPC.rotation == 0)
                                        {
                                            if (AITimer == 0)
                                            {
                                                SoundEngine.PlaySound(SoundID.Item74, NPC.position);
                                                AITimer = 1;
                                                NPC.netUpdate = true;
                                            }
                                            NPC.velocity.Y = -26;
                                            if (NPC.Center.Y < player.Center.Y - 160)
                                            {
                                                NPC.velocity *= 0;
                                                AITimer = 0;
                                                NPC.ai[1] = 9;
                                                NPC.netUpdate = true;
                                            }
                                        }
                                        break;
                                    case 9:
                                        AITimer++;
                                        rot.SlowRotation(repeat == 0 ? 0.78f : 5.49f, (float)Math.PI / 20f);
                                        NPC.rotation = rot;
                                        if (AITimer > 10)
                                        {
                                            host.ai[2] = 200;
                                            SoundEngine.PlaySound(SoundID.Item71, NPC.position);
                                            NPC.rotation = player.Center.X > NPC.Center.X ? 0.78f : 5.49f;
                                            NPC.ai[1] = 10;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                    case 10:
                                        AITimer++;
                                        if (AITimer == 1)
                                            NPC.velocity.X = repeat == 0 ? 17 : -17;
                                        if (AITimer == 16)
                                            NPC.velocity.X = repeat == 0 ? -17 : 17;

                                        NPC.velocity.Y = 20;
                                        NPC.rotation += repeat == 0 ? 0.12f : -0.12f;
                                        if (AITimer > 30)
                                        {
                                            repeat = 0;
                                            NPC.velocity *= 0f;
                                            host.ai[2] = 1000;
                                            AITimer = 0;
                                            NPC.netUpdate = true;
                                        }
                                        break;
                                }
                                break;
                            #endregion

                            case 20:
                                if (NPC.life < (int)(NPC.lifeMax * .75f))
                                    host.ai[2] = 1;
                                else
                                    host.ai[2] = -1;
                                break;
                            case 21:
                                if (NPC.life < (int)(NPC.lifeMax * .3f))
                                    host.ai[2] = 1;
                                else
                                    host.ai[2] = -1;
                                break;
                            case 22:
                                if (NPC.life < (int)(NPC.lifeMax * .9f))
                                    host.ai[2] = 1;
                                else
                                    host.ai[2] = -1;
                                break;


                        }
                        break;
                }
            }
            else
            {
                if (player.active && !player.dead)
                {
                    if (AIState != ActionState.Death)
                    {
                        AIState = ActionState.Death;
                        AITimer = 0;
                        rot = NPC.rotation;
                        NPC.netUpdate = true;
                    }
                    else
                    {
                        player.RedemptionScreen().ScreenFocusPosition = NPC.Center;
                        player.RedemptionScreen().lockScreen = true;
                        player.RedemptionScreen().cutscene = true;
                        AITimer++;
                        rot.SlowRotation(0, (float)Math.PI / 60f);
                        NPC.rotation = rot;
                        NPC.velocity *= .9f;
                        NPC.velocity = new Vector2(Main.rand.NextFloat(-2f, 2f), Main.rand.NextFloat(-2f, 2f));
                        if (Main.rand.NextBool(20))
                        {
                            SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                            RedeDraw.SpawnExplosion(new Vector2(NPC.position.X + Main.rand.Next(NPC.width), NPC.position.Y + Main.rand.Next(NPC.height)), Color.OrangeRed);
                        }
                        if (AITimer == 60)
                        {
                            NPC.life = 1;
                            NPC.netUpdate = true;
                        }
                        if (AITimer >= 120)
                        {
                            NPC.dontTakeDamage = false;
                            player.ApplyDamageToNPC(NPC, 9999, 0, 0, false);
                            NPC.netUpdate = true;
                        }
                    }
                }
            }
        }

        public override void FindFrame(int frameHeight)
        {
            for (int k = NPC.oldPos.Length - 1; k > 0; k--)
                oldrot[k] = oldrot[k - 1];
            oldrot[0] = NPC.rotation;

            NPC host = Main.npc[(int)NPC.ai[3]];
            if (host.ai[3] == 5 || host.ai[3] == 9 || host.ai[3] == 10 || (host.ai[3] == 11 && (NPC.ai[1] == 4 || NPC.ai[1] == 8)))
            {
                NPC.frameCounter++;
                if (NPC.frameCounter >= 10)
                {
                    NPC.frameCounter = 0;
                    NPC.frame.Y += frameHeight;
                    if (NPC.frame.Y >= 5 * frameHeight)
                        NPC.frame.Y = frameHeight;
                }
            }
            else
                NPC.frame.Y = 0;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[NPC.type].Value;
            Texture2D glowMask = ModContent.Request<Texture2D>(NPC.ModNPC.Texture + "_Glow").Value;
            Texture2D trail = ModContent.Request<Texture2D>(NPC.ModNPC.Texture + "_Trail").Value;
            var effects = NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally;
            Rectangle rectangle = NPC.frame;
            Vector2 origin2 = rectangle.Size() / 2f;
            if (!NPC.IsABestiaryIconDummy)
            {
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);

                for (int i = 0; i < NPCID.Sets.TrailCacheLength[NPC.type]; i++)
                {
                    Vector2 value4 = NPC.oldPos[i];
                    spriteBatch.Draw(trail, value4 + NPC.Size / 2f - screenPos + new Vector2(0, NPC.gfxOffY), new Rectangle?(rectangle), RedeColor.VlitchGlowColour * 0.5f, oldrot[i], origin2, NPC.scale, effects, 0);
                }

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            }

            spriteBatch.Draw(texture, NPC.Center - screenPos, NPC.frame, drawColor, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0f);
            spriteBatch.Draw(glowMask, NPC.Center - screenPos, NPC.frame, RedeColor.RedPulse, NPC.rotation, NPC.frame.Size() / 2, NPC.scale, effects, 0);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }

        public override void BossHeadRotation(ref float rotation)
        {
            rotation = NPC.rotation;
        }

        private void DespawnHandler()
        {
            Player player = Main.player[NPC.target];
            if (!player.active || player.dead)
            {
                NPC.velocity *= 0.96f;
                NPC.velocity.Y -= 1;
                if (NPC.timeLeft > 10)
                    NPC.timeLeft = 10;
                return;
            }
        }
    }
    public class CleaverHitbox : ModProjectile
    {
        public override string Texture => Redemption.EMPTY_TEXTURE;
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Omega Cleaver");
        }
        public override void SetDefaults()
        {
            Projectile.width = 58;
            Projectile.height = 58;
            Projectile.aiStyle = -1;
            Projectile.friendly = false;
            Projectile.hostile = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.tileCollide = false;
        }
        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            int boss = (int)Projectile.ai[0];
            if (boss < 0 || boss >= 200 || !Main.npc[boss].active || Main.npc[boss].type != ModContent.NPCType<OmegaCleaver>())
                Projectile.Kill();

            NPC host = Main.npc[(int)Projectile.ai[0]];
            Projectile.Center = host.Center;
            Projectile.rotation = host.rotation;
            Projectile.timeLeft = 10;
        }
        public override void OnHitPlayer(Player target, int damage, bool crit)
        {
            target.AddBuff(ModContent.BuffType<SnippedDebuff>(), Main.expertMode ? 400 : 200);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float collisionPoint = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center, Projectile.Center + (Projectile.rotation + -MathHelper.PiOver2).ToRotationVector2() * 140, projHitbox.Width, ref collisionPoint);
        }
    }
}