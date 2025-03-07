using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.GameContent;
using Terraria.ID;
using Redemption.Base;
using Redemption.Globals;

namespace Redemption.Projectiles.Misc
{
    public class InfectionShockwave_Proj : ModProjectile
    {
        public override string Texture => "Redemption/Textures/Shockwave";
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Infection Pulse");
        }
        public override void SetDefaults()
        {
            Projectile.width = 200;
            Projectile.height = 200;
            Projectile.penetrate = -1;
            Projectile.hostile = false;
            Projectile.friendly = false;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.scale = 0.1f;
            Projectile.timeLeft = 120;
        }

        public override void AI()
        {
            Player player = Main.player[Projectile.owner];
            Projectile.Center = player.Center;
            Projectile.localAI[0]++;
            if (Projectile.localAI[0] == 30)
            {
                for (int i = 0; i < Main.maxNPCs; i++)
                {
                    NPC target = Main.npc[i];
                    if (!target.active || !target.CanBeChasedBy() || target.dontCountMe || !target.NPCHasAnyDebuff())
                        continue;

                    if (Projectile.DistanceSQ(target.Center) > 200 * Projectile.scale * (200 * Projectile.scale))
                        continue;

                    for (int k = 0; k < 20; k++)
                    {
                        Dust dust = Dust.NewDustDirect(target.position, target.width, target.height, DustID.PoisonStaff, Scale: 2);
                        dust.velocity = -player.DirectionTo(dust.position) * 20;
                        dust.noGravity = true;
                    }
                    int maxSteal = target.lifeMax / 50;
                    if (maxSteal > 5000)
                        maxSteal = 5000;
                    if (maxSteal >= target.life)
                        maxSteal = target.life - 1;
                    if (maxSteal <= 0)
                        continue;

                    BaseAI.DamageNPC(target, maxSteal, 0, 1, player, false, true);
                    player.statLife += 10;
                    player.HealEffect(10);
                }
            }
            if (Projectile.localAI[0] < 60)
            {
                if (Projectile.localAI[0] < 30)
                    Projectile.alpha -= 5;
                else
                    Projectile.alpha += 5;
                Projectile.scale += 0.2f;
            }
            else
            {
                Projectile.alpha = 255;
                Projectile.Kill();
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
            Vector2 position = Projectile.Center - Main.screenPosition;
            Rectangle rect = new(0, 0, texture.Width, texture.Height);
            Vector2 origin = new(texture.Width / 2f, texture.Height / 2f);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            Main.EntitySpriteDraw(texture, position, new Rectangle?(rect), Projectile.GetAlpha(Color.DarkOliveGreen) * 0.7f, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0);
            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise, null, Main.GameViewMatrix.TransformationMatrix);
            return false;
        }
    }
}