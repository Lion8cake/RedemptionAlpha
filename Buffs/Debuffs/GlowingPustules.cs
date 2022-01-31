using Microsoft.Xna.Framework;
using Redemption.Globals.Player;
using Terraria;
using Terraria.ModLoader;
using Redemption.BaseExtension;

namespace Redemption.Buffs.Debuffs
{
    public class GlowingPustulesDebuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Glowing Pustules");
            Description.SetDefault("... I have to carry on my work.");
            Main.debuff[Type] = true;
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = true;
        }
        public override void Update(Player player, ref int buffIndex)
        {
            player.RedemptionPlayerBuff().glowingPustules = true;
            player.statDefense -= 9;
            player.moveSpeed *= 0.80f;
            Lighting.AddLight(player.Center, Color.LimeGreen.ToVector3() * 0.55f);
        }
        public override void Update(NPC npc, ref int buffIndex)
        {
            npc.lifeRegen = -20;
        }
    }
}