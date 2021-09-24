using Terraria;
using Terraria.ModLoader;
using Terraria.ID;
using Terraria.GameContent.Creative;
using Redemption.Items.Materials.PreHM;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Redemption.Globals.Player;
using System;

namespace Redemption.Items.Armor.PreHM
{
	[AutoloadEquip(EquipType.Head)]
	public class CommonGuardHelm1 : ModItem
	{
		public override void SetStaticDefaults()
		{
			DisplayName.SetDefault("Common Guard Helm");
			Tooltip.SetDefault("+2 increased melee damage");

            CreativeItemSacrificesCatalog.Instance.SacrificeCountNeededByItemId[Type] = 1;
        }

        public override void SetDefaults()
		{
			Item.width = 24;
			Item.height = 26;
			Item.sellPrice(silver: 30);
			Item.rare = ItemRarityID.Green;
			Item.defense = 3;
		}

		public override bool IsArmorSet(Item head, Item body, Item legs)
		{
			return body.type == ModContent.ItemType<CommonGuardPlateMail>() && legs.type == ModContent.ItemType<CommonGuardGreaves>();
		}

        public override void UpdateEquip(Player player)
        {
            player.GetModPlayer<BuffPlayer>().MeleeDamageFlat += 2;
        }

        public override void UpdateArmorSet(Player player)
		{
			player.setBonus = "12% increased melee speed";
			player.meleeSpeed += .12f;
            player.GetModPlayer<BuffPlayer>().MetalSet = true;

            if (Main.rand.NextBool(10) && Math.Abs(player.velocity.X) + Math.Abs(player.velocity.Y) > 1f && !player.rocketFrame)
            {
                int index = Dust.NewDust(new Vector2(player.position.X - player.velocity.X * 2f, player.position.Y - 2f - player.velocity.Y * 2f), player.width, player.height,
                    DustID.Web);
                Main.dust[index].noGravity = true;
                Dust dust = Main.dust[index];
                dust.velocity -= player.velocity * 0.5f;
            }
        }

        public override void DrawHair(ref bool drawHair, ref bool drawAltHair)
		{
			drawHair = drawAltHair = false;
		}

        public override void AddRecipes()
        {
            CreateRecipe()
                .AddIngredient(ModContent.ItemType<GraveSteelAlloy>(), 10)
                .AddIngredient(ItemID.Silk, 2)
                .AddTile(TileID.Anvils)
                .Register();
        }

        public override void ModifyTooltips(List<TooltipLine> tooltips)
        {
            if (Main.keyState.PressingShift())
            {
                TooltipLine line = new(Mod, "Lore",
                    "'Visored plate mail helm of the Common Guard unit of Anglon that was scavenged by skeletons.\n" +
                    "Originally shining steel, the metal has since dulled with time and coated with layers of dust.\n\n" +
                    "The Common Guard was founded when an Overlord's city was completely obliterated\n" +
                    "by a stray demon that sneaked through an unguarded portal to Demonhollow.\n\n" +
                    "They now guard cities and landmarks of great importance. Despite being stronger than the average\n" +
                    "knight, they don't get involved in wars.'")
                {
                    overrideColor = Color.LightGray
                };
                tooltips.Add(line);
            }
            else
            {
                TooltipLine line = new(Mod, "HoldShift", "Hold [Shift] to view lore")
                {
                    overrideColor = Color.Gray,
                };
                tooltips.Add(line);
            }
        }
    }
}