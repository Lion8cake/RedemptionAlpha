using Microsoft.Xna.Framework.Graphics;
using Redemption.Biomes;
using Redemption.Items.Donator.Lizzy;
using Redemption.Items.Donator.Uncon;
using Redemption.Projectiles.Ranged;
using System.Collections.Generic;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Redemption.BaseExtension;
using Terraria.DataStructures;
using Microsoft.Xna.Framework;
using Redemption.Items.Placeable.Furniture.Lab;

namespace Redemption.Globals.Player
{
    public class RedePlayer : ModPlayer
    {
        public int spiritLevel = 0;
        public int maxSpiritLevel = 3;
        public bool foundHall;
        public int hitTarget = -1;
        public int hitTarget2 = -1;
        public bool medKit;

        public override void ResetEffects()
        {
            hitTarget = -1;
            hitTarget2 = -1;
        }
        public override void Initialize()
        {
            foundHall = false;
            medKit = false;
        }

        public override void OnHitNPC(Item item, Terraria.NPC target, int damage, float knockback, bool crit)
        {
            if (Player.RedemptionPlayerBuff().hardlightBonus == 3 && item.DamageType == DamageClass.Melee)
            {
                hitTarget = target.whoAmI;
                hitTarget2 = target.whoAmI;
            }
        }
        public override void OnHitNPCWithProj(Projectile proj, Terraria.NPC target, int damage, float knockback, bool crit)
        {
            if (Player.RedemptionPlayerBuff().hardlightBonus == 3 && proj.DamageType == DamageClass.Melee && proj.type != ModContent.ProjectileType<MiniSpaceship_Laser>())
            {
                hitTarget = target.whoAmI;
                hitTarget2 = target.whoAmI;
            }
        }

        public override void PostUpdateMiscEffects()
        {
            Player.statLifeMax2 += medKit ? 50 : 0;

            if (Main.netMode != NetmodeID.Server && Player.whoAmI == Main.myPlayer)
            {
                ReLogic.Content.Asset<Texture2D> rain = ModContent.Request<Texture2D>("Redemption/Textures/RainOriginal", ReLogic.Content.AssetRequestMode.ImmediateLoad);
                ReLogic.Content.Asset<Texture2D> rainWasteland = ModContent.Request<Texture2D>("Redemption/Textures/Rain2", ReLogic.Content.AssetRequestMode.ImmediateLoad);

                if (Main.bloodMoon)
                    TextureAssets.Rain = rain;
                else if (Main.raining && Player.InModBiome(ModContent.GetInstance<WastelandPurityBiome>()))
                    TextureAssets.Rain = rainWasteland;
                else
                    TextureAssets.Rain = rain;
            }
        }
        public override void CatchFish(FishingAttempt attempt, ref int itemDrop, ref int npcSpawn, ref AdvancedPopupRequest sonar, ref Vector2 sonarPosition)
        {
            if (Main.rand.Next(100) < (10 + (Player.cratePotion ? 10 : 0)))
            {
                if (Player.InModBiome(ModContent.GetInstance<LabBiome>()) && Terraria.NPC.downedMechBoss1 && Terraria.NPC.downedMechBoss2 && Terraria.NPC.downedMechBoss3)
                    itemDrop = ModContent.ItemType<LabCrate>();
            }
        }
        public override IEnumerable<Item> AddStartingItems(bool mediumCoreDeath)
        {
            if (!mediumCoreDeath && (Player.name.Contains("Liz") || Player.name.Contains("Lizzy") || Player.name.Contains("Elizabeth")))
            {
                return new[] {
                    new Item(ModContent.ItemType<LizzyCookie>())
                };
            }
            if (!mediumCoreDeath && (Player.name == "Uncon" || Player.name == "Dahlia"))
            {
                return new[] {
                    new Item(ModContent.ItemType<UnconHead>()),
                    new Item(ModContent.ItemType<UnconBody>()),
                    new Item(ModContent.ItemType<UnconLegs>()),
                    new Item(ModContent.ItemType<UnconPatreon_CapeAcc>()),
                    new Item(ModContent.ItemType<UnconPetItem>())
                };
            }
            return base.AddStartingItems(mediumCoreDeath);
        }

        public override void SaveData(TagCompound tag)
        {
            var saveS = new List<string>();
            if (foundHall) saveS.Add("foundHall");
            if (medKit) saveS.Add("medKit");

            tag["saveS"] = saveS;
        }

        public override void LoadData(TagCompound tag)
        {
            var saveS = tag.GetList<string>("saveS");
            foundHall = saveS.Contains("foundHall");
            medKit = saveS.Contains("medKit");
        }
    }
}