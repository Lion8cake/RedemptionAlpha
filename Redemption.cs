using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Redemption.Globals;
using Redemption.StructureHelper;
using Redemption.StructureHelper.ChestHelper.GUI;
using System.Collections.Generic;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;
using Terraria.UI;

namespace Redemption
{
    public class Redemption : Mod
    {
        public override void AddRecipes() => RecipeSystem.Load(this);

        public static Redemption Instance { get; private set; }

        public const string EMPTY_TEXTURE = "Redemption/Empty";
        public Vector2 cameraOffset;

        public Redemption()
        {
            Instance = this;
        }
    }
    public class RedeSystem : ModSystem
    {
        public static RedeSystem Instance { get; private set; }
        public RedeSystem()
        {
            Instance = this;
        }
        UserInterface GeneratorMenuUI;
        internal ManualGeneratorMenu GeneratorMenu;

        UserInterface ChestMenuUI;
        internal ChestCustomizerState ChestCustomizer;

        public override void Load()
        {
            if (!Main.dedServ)
            {
                GeneratorMenuUI = new UserInterface();
                GeneratorMenu = new ManualGeneratorMenu();
                GeneratorMenuUI.SetState(GeneratorMenu);

                ChestMenuUI = new UserInterface();
                ChestCustomizer = new ChestCustomizerState();
                ChestMenuUI.SetState(ChestCustomizer);
            }
        }
        public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
        {
            layers.Insert(layers.FindIndex(layer => layer.Name.Equals("Vanilla: Mouse Text")), new LegacyGameInterfaceLayer("GUI Menus",
                delegate
                {
                    if (ManualGeneratorMenu.Visible)
                    {
                        GeneratorMenuUI.Update(Main._drawInterfaceGameTime);
                        GeneratorMenu.Draw(Main.spriteBatch);
                    }

                    if (ChestCustomizerState.Visible)
                    {
                        ChestMenuUI.Update(Main._drawInterfaceGameTime);
                        ChestCustomizer.Draw(Main.spriteBatch);
                    }

                    return true;
                }, InterfaceScaleType.UI));
        }
        #region StructureHelper Draw
        public override void PostDrawInterface(SpriteBatch spriteBatch)
        {
            if (Main.LocalPlayer.HeldItem.ModItem is CopyWand)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                Texture2D tex = (Texture2D)ModContent.Request<Texture2D>("Redemption/StructureHelper/corner");
                Texture2D tex2 = (Texture2D)ModContent.Request<Texture2D>("Redemption/StructureHelper/box1");
                Point16 TopLeft = (Main.LocalPlayer.HeldItem.ModItem as CopyWand).TopLeft;
                int Width = (Main.LocalPlayer.HeldItem.ModItem as CopyWand).Width;
                int Height = (Main.LocalPlayer.HeldItem.ModItem as CopyWand).Height;

                float tileScale = 16 * Main.GameViewMatrix.Zoom.Length() * 0.707106688737f;
                Vector2 pos = (Main.MouseWorld / tileScale).ToPoint16().ToVector2() * tileScale - Main.screenPosition;
                pos = Vector2.Transform(pos, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix));
                pos = Vector2.Transform(pos, Main.UIScaleMatrix);

                spriteBatch.Draw(tex, pos, tex.Frame(), Color.White * 0.5f, 0, tex.Frame().Size() / 2, 1, 0, 0);

                if (Width != 0 && TopLeft != null)
                {
                    spriteBatch.Draw(tex2, new Rectangle((int)(TopLeft.X * 16 - Main.screenPosition.X), (int)(TopLeft.Y * 16 - Main.screenPosition.Y), Width * 16 + 16, Height * 16 + 16), tex2.Frame(), Color.White * 0.15f);
                    spriteBatch.Draw(tex, (TopLeft.ToVector2() + new Vector2(Width + 1, Height + 1)) * 16 - Main.screenPosition, tex.Frame(), Color.Red, 0, tex.Frame().Size() / 2, 1, 0, 0);
                }
                if (TopLeft != null) spriteBatch.Draw(tex, TopLeft.ToVector2() * 16 - Main.screenPosition, tex.Frame(), Color.Cyan, 0, tex.Frame().Size() / 2, 1, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
            }

            if (Main.LocalPlayer.HeldItem.ModItem is MultiWand)
            {
                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.GameViewMatrix.ZoomMatrix);

                Texture2D tex = (Texture2D)ModContent.Request<Texture2D>("Redemption/StructureHelper/corner");
                Texture2D tex2 = (Texture2D)ModContent.Request<Texture2D>("Redemption/StructureHelper/box1");
                Point16 TopLeft = (Main.LocalPlayer.HeldItem.ModItem as MultiWand).TopLeft;
                int Width = (Main.LocalPlayer.HeldItem.ModItem as MultiWand).Width;
                int Height = (Main.LocalPlayer.HeldItem.ModItem as MultiWand).Height;
                int count = (Main.LocalPlayer.HeldItem.ModItem as MultiWand).StructureCache.Count;

                float tileScale = 16 * Main.GameViewMatrix.Zoom.Length() * 0.707106688737f;
                Vector2 pos = (Main.MouseWorld / tileScale).ToPoint16().ToVector2() * tileScale - Main.screenPosition;
                pos = Vector2.Transform(pos, Matrix.Invert(Main.GameViewMatrix.ZoomMatrix));
                pos = Vector2.Transform(pos, Main.UIScaleMatrix);

                spriteBatch.Draw(tex, pos, tex.Frame(), Color.White * 0.5f, 0, tex.Frame().Size() / 2, 1, 0, 0);

                if (Width != 0 && TopLeft != null)
                {
                    spriteBatch.Draw(tex2, new Rectangle((int)(TopLeft.X * 16 - Main.screenPosition.X), (int)(TopLeft.Y * 16 - Main.screenPosition.Y), Width * 16 + 16, Height * 16 + 16), tex2.Frame(), Color.White * 0.15f);
                    spriteBatch.Draw(tex, (TopLeft.ToVector2() + new Vector2(Width + 1, Height + 1)) * 16 - Main.screenPosition, tex.Frame(), Color.Yellow, 0, tex.Frame().Size() / 2, 1, 0, 0);
                }
                if (TopLeft != null) spriteBatch.Draw(tex, TopLeft.ToVector2() * 16 - Main.screenPosition, tex.Frame(), Color.LimeGreen, 0, tex.Frame().Size() / 2, 1, 0, 0);

                spriteBatch.End();
                spriteBatch.Begin();
                Utils.DrawBorderString(spriteBatch, "Structures to save: " + count, Main.MouseScreen + new Vector2(0, 30), Color.White);

                spriteBatch.End();
                spriteBatch.Begin(default, default, default, default, default, default, Main.UIScaleMatrix);
            }
        }
        #endregion
    }
}