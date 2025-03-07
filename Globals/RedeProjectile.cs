using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Redemption.BaseExtension;
using Terraria.DataStructures;
using System.Collections.Generic;
using Redemption.Effects.PrimitiveTrails;
using Terraria.ID;
using static Redemption.Globals.RedeNet;
using System.Linq;
using Terraria.Enums;

namespace Redemption.Globals
{
    public class RedeProjectile : GlobalProjectile
    {
        public override bool InstancePerEntity => true;
        public bool TechnicallyMelee;
        public bool IsHammer;
        public bool IsAxe;
        public bool RitDagger;
        public bool EnergyBased;
        public bool ParryBlacklist;
        public int DissolveTimer;
        public override void SetDefaults(Projectile projectile)
        {
            if (ProjectileLists.IsTechnicallyMelee.Contains(projectile.type))
                TechnicallyMelee = true;
        }

        private readonly int[] bannedArenaProjs = new int[]
        {
            ProjectileID.SandBallGun,
            ProjectileID.EbonsandBallGun,
            ProjectileID.PearlSandBallGun,
            ProjectileID.CrimsandBallGun,
            ProjectileID.SandBallFalling,
            ProjectileID.EbonsandBallFalling,
            ProjectileID.PearlSandBallFalling,
            ProjectileID.CrimsandBallFalling,
            ProjectileID.Bomb,
            ProjectileID.StickyBomb,
            ProjectileID.BouncyBomb,
            ProjectileID.Dynamite,
            ProjectileID.StickyDynamite,
            ProjectileID.BouncyDynamite,
            ProjectileID.SnowBallHostile,
            ProjectileID.IceBlock,
            ProjectileID.AntiGravityHook,
            ProjectileID.StaticHook,
            ProjectileID.PortalGunBolt,
            ProjectileID.PortalGunGate
        };
        public override void AI(Projectile projectile)
        {
            if (ArenaWorld.arenaActive && bannedArenaProjs.Any(x => x == projectile.type) && projectile.Hitbox.Intersects(new Rectangle((int)ArenaWorld.arenaTopLeft.X, (int)ArenaWorld.arenaTopLeft.Y, (int)ArenaWorld.arenaSize.X, (int)ArenaWorld.arenaSize.Y)))
                projectile.Kill();

            if (ArenaWorld.arenaActive && projectile.aiStyle == 7 && !projectile.Hitbox.Intersects(new Rectangle((int)ArenaWorld.arenaTopLeft.X, (int)ArenaWorld.arenaTopLeft.Y, (int)ArenaWorld.arenaSize.X, (int)ArenaWorld.arenaSize.Y)))
                projectile.Kill();
        }
        public override void ModifyHitNPC(Projectile projectile, Terraria.NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
            if (IsAxe && crit)
                damage += damage / 2;
        }
        public static void Decapitation(Terraria.NPC target, ref int damage, ref bool crit, int chance = 200)
        {
            if (target.life < target.lifeMax && NPCLists.SkeletonHumanoid.Contains(target.type))
            {
                if (Main.rand.NextBool(chance))
                {
                    CombatText.NewText(target.getRect(), Color.Orange, "Decapitated!");
                    target.Redemption().decapitated = true;
                    damage = damage < target.life ? target.life : damage;
                    crit = true;
                }
            }
        }
        public static Dictionary<int, (Entity entity, IEntitySource source)> projOwners = new();
        public override void OnSpawn(Projectile projectile, IEntitySource source)
        {
            Entity attacker = null;
            if (source is EntitySource_ItemUse item && projectile.friendly && !projectile.hostile)
            {
                attacker = item.Entity;
            }
            else if (source is EntitySource_Buff buff && projectile.friendly && !projectile.hostile)
            {
                attacker = buff.Entity;
            }
            else if (source is EntitySource_ItemUse_WithAmmo itemAmmo && projectile.friendly && !projectile.hostile)
            {
                attacker = itemAmmo.Entity;
            }
            else if (source is EntitySource_Mount mount && projectile.friendly && !projectile.hostile)
            {
                attacker = mount.Entity;
            }
            else if (source is EntitySource_Parent parent)
            {
                if (parent.Entity is Projectile proj)
                    attacker = Main.player[proj.owner];
                else
                    attacker = parent.Entity;
            }
            if (attacker != null)
            {
                if (projOwners.ContainsKey(projectile.whoAmI))
                    projOwners.Remove(projectile.whoAmI);
                projOwners.Add(projectile.whoAmI, (attacker, source));
            }

            if (projectile.ModProjectile is ITrailProjectile)
            {
                if (Main.netMode == NetmodeID.SinglePlayer)
                    (projectile.ModProjectile as ITrailProjectile).DoTrailCreation(RedeSystem.TrailManager);

                else
                    Redemption.WriteToPacket(Redemption.Instance.GetPacket(), (byte)ModMessageType.SpawnTrail, projectile.whoAmI).Send();
            }
        }
    }
    public abstract class TrueMeleeProjectile : ModProjectile
    {
        public float SetSwingSpeed(float speed)
        {
            Terraria.Player player = Main.player[Projectile.owner];
            return speed / player.GetAttackSpeed(DamageClass.Melee);
        }

        public virtual void SetSafeDefaults() { }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Melee;
            Projectile.tileCollide = false;
            Projectile.ownerHitCheck = true;
            Projectile.Redemption().TechnicallyMelee = true;
            SetSafeDefaults();
        }
    }
    public abstract class LaserProjectile : ModProjectile
    {
        public float AITimer
        {
            get => Projectile.localAI[0];
            set => Projectile.localAI[0] = value;
        }
        public float Frame
        {
            get => Projectile.localAI[1];
            set => Projectile.localAI[1] = value;
        }
        public float LaserLength = 0;
        public float LaserScale = 0;
        public int LaserSegmentLength = 10;
        public int LaserWidth = 20;
        public int LaserEndSegmentLength = 22;

        public const float FirstSegmentDrawDist = 7;

        public int MaxLaserLength = 2000;
        public int maxLaserFrames = 1;
        public int LaserFrameDelay = 5;
        public bool StopsOnTiles = true;
        
        public virtual void SetSafeStaticDefaults() { }

        public override void SetStaticDefaults()
        {
            SetSafeStaticDefaults();
            ProjectileID.Sets.DrawScreenCheckFluff[Type] = 2400;
        }

        public virtual void SetSafeDefaults() { }

        public override void SetDefaults()
        {
            Projectile.width = LaserWidth;
            Projectile.height = LaserWidth;
            Projectile.Redemption().ParryBlacklist = true;
            SetSafeDefaults();
        }
        public virtual void EndpointTileCollision()
        {
            for (LaserLength = FirstSegmentDrawDist; LaserLength < MaxLaserLength; LaserLength += LaserSegmentLength)
            {
                Vector2 start = Projectile.Center + Vector2.UnitX.RotatedBy(Projectile.rotation) * LaserLength;
                if (!Collision.CanHitLine(Projectile.Center, 1, 1, start, 1, 1))
                {
                    LaserLength -= LaserSegmentLength;
                    break;
                }
            }
        }
        public override void CutTiles()
        {
            DelegateMethods.tilecut_0 = TileCuttingContext.AttackProjectile;
            Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation);
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + unit * LaserLength, (Projectile.width + 16) * Projectile.scale, DelegateMethods.CutTiles);
        }
        public virtual void CastLights(Vector3 color)
        {
            // Cast a light along the line of the Laser
            DelegateMethods.v3_1 = color;
            Utils.PlotTileLine(Projectile.Center, Projectile.Center + new Vector2(1f, 0).RotatedBy(Projectile.rotation) * LaserLength, 26, DelegateMethods.CastLight);
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            Vector2 unit = new Vector2(1.5f, 0).RotatedBy(Projectile.rotation);
            float point = 0f;
            // Run an AABB versus Line check to look for collisions
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(), targetHitbox.Size(), Projectile.Center,
                Projectile.Center + unit * LaserLength, Projectile.width * LaserScale, ref point))
                return true;
            return false;
        }

        public override void PostAI() //Lion8cake's pretty scuffed and shitty postai to make some iradiated tiles convert to normal tiles
		{
            if ((Projectile.type != 10 && Projectile.type != 145 && Projectile.type != 147 && Projectile.type != 149 && Projectile.type != 146) || Projectile.owner != Main.myPlayer)
			{
				return;
			}
			int x = (int)(Projectile.Center.X / 16f);
			int y = (int)(Projectile.Center.Y / 16f);

            Tile tile = Main.tile[x, y];
            int type = tile.TileType;
            int wallType = tile.WallType;

            for (int i = x - 1; i <= x + 1; i++)
			{
				for (int j = y - 1; j <= y + 1; j++)
				{
					if (Projectile.type == 145 || Projectile.type == 10)
					{
                        if (!WorldGen.InWorld(x, y, 1))
                        {
                            return;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                            {
                                tile.TileType = 0;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                            {
                                tile.TileType = 147;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                            {
                                tile.TileType = 191;
                            }
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                            {
                                Main.tile[x, y].WallType = 2;
                            }
                        }
                    }
					if (Projectile.type == 147)
					{
                        if (!WorldGen.InWorld(x, y, 1))
                        {
                            return;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                            {
                                tile.TileType = 0;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                            {
                                tile.TileType = 147;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                            {
                                tile.TileType = 191;
                            }
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                            {
                                Main.tile[x, y].WallType = 2;
                            }
                        }
                    }
					if (Projectile.type == 149)
					{
                        if (!WorldGen.InWorld(x, y, 1))
                        {
                            return;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                            {
                                tile.TileType = 0;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                            {
                                tile.TileType = 147;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                            {
                                tile.TileType = 191;
                            }
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                            {
                                Main.tile[x, y].WallType = 2;
                            }
                        }
                    }
					if (Projectile.type == 146)
					{
                        if (!WorldGen.InWorld(x, y, 1))
                        {
                            return;
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (type == ModContent.TileType<Tiles.Tiles.IrradiatedDirtTile>())
                            {
                                tile.TileType = 0;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedSnowTile>())
                            {
                                tile.TileType = 147;
                            }
                            else if (type == ModContent.TileType<Tiles.Tiles.IrradiatedLivingWoodTile>())
                            {
                                tile.TileType = 191;
                            }
                        }
                        if (Main.tile[x, y] != null)
                        {
                            if (wallType == ModContent.WallType<Walls.IrradiatedDirtWallTile>())
                            {
                                Main.tile[x, y].WallType = 2;
                            }
                        }
                    }
					NetMessage.SendTileSquare(-1, i, j, 1, 1);
				}
			}
		}
    }
}