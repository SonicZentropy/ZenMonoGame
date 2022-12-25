﻿using Nez;

namespace LDtkMonogameExample;

using System;
using System.Collections.Generic;

using LDtk;
using LDtk.Renderer;

using LDtkMonogameExample.Entities;

using LDtkTypes.World;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

public class LDtkMonogameGame : Nez.Core
{
    // LDtk stuff
    private LDtkFile file;
    private LDtkWorld world;
    private LDtkRenderer renderer;
    private readonly List<EnemyEntity> enemies = new();
    private readonly List<BulletEntity> bullets = new();
    private PlayerEntity player;
    private GunEntity gun;
    private Camera camera;
    private Texture2D spriteSheet;

    // Monogame Stuff
    private SpriteBatch spriteBatch;
    private readonly GraphicsDeviceManager graphics;
    private float pixelScale = 1f;
    public static Texture2D Pixel { get; set; }

    public static bool DebugF1 { get; set; }
    public static bool DebugF2 { get; set; }
    public static bool DebugF3 { get; set; }

    private KeyboardState oldKeyboard;

    public LDtkMonogameGame()
    {
        Screen.SetSize(1280, 720);
        
    }

    private void MonogameInitialize()
    {
        Window.Title = "LDtkMonogame - Shooter";

        spriteBatch = new SpriteBatch(GraphicsDevice);

        Window.AllowUserResizing = true;
        IsMouseVisible = true;
        IsFixedTimeStep = false;


        Window.ClientSizeChanged += (o, e) => pixelScale = Math.Max(GraphicsDevice.Viewport.Height / 180, 1);

        pixelScale = Math.Max(GraphicsDevice.Viewport.Height / 180, 1);

        Pixel = new Texture2D(GraphicsDevice, 1, 1);
        Pixel.SetData(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF });
    }

    protected override void Initialize()
    {
        base.Initialize();
        Window.AllowUserResizing = true;
        MonogameInitialize();

        camera = new Camera(GraphicsDevice);

        renderer = new LDtkRenderer(spriteBatch);
        file = LDtkFile.FromFile("Content/fortloop/world.ldtk");
        spriteSheet = Texture2D.FromFile(GraphicsDevice, System.IO.Path.Combine(System.IO.Path.GetDirectoryName(file.FilePath), "tileset.png"));


        world = file.LoadWorld(Worlds.World.Iid);

        LDtkLevel level0 = world.LoadLevel("0000-Intro");
        LDtkLevel level1 = world.LoadLevel(Worlds.World.Level_1);

        CustomLevelDataName levelData = level1.GetCustomFields<CustomLevelDataName>();

        Console.WriteLine(levelData.Float);
        Console.WriteLine(levelData.Multilines);

        foreach (TilesetRectangle item in levelData.Tile)
        {
            Console.WriteLine(item.X);
        }

        RefTest[] entities = level0.GetEntities<RefTest>();

        RefTest test = level0.GetEntityRef<RefTest>(entities[0].Test);

        foreach (LDtkLevel level in world.Levels)
        {
            foreach (Enemy enemy in level.GetEntities<Enemy>())
            {
                enemies.Add(new EnemyEntity(enemy, spriteSheet, renderer));
            }

            renderer.PrerenderLevel(level);
        }

        Gun_Pickup gunData = world.GetEntity<Gun_Pickup>();
        gun = new GunEntity(gunData, spriteSheet, renderer);

        Player playerData = world.GetEntity<Player>();
        player = new PlayerEntity(playerData, spriteSheet, renderer, gun);

        player.OnShoot += () =>
        {
            BulletEntity b = new(spriteSheet, renderer)
            {
                Position = player.Position + new Vector2(player.Flip ? -23 : 7, -5.5f),
                Flip = player.Flip,
            };
            bullets.Add(b);
        };
    }

    protected override void Update(GameTime gameTime)
    {
        DebugInput(oldKeyboard);

        float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
        float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

        camera.Update();
        camera.Position = new Vector2(player.Position.X, 120);
        camera.Zoom = pixelScale;

        gun.Update(totalTime);

        foreach (LDtkLevel level in world.Levels)
        {
            if (level.Contains(player.Position))
            {
                player.Level = level;
                break;
            }
        }

        for (int i = 0; i < enemies.Count; i++)
        {
            enemies[i].Update(deltaTime);
        }

        for (int i = bullets.Count - 1; i >= 0; i--)
        {
            bullets[i].Update(deltaTime);

            for (int j = enemies.Count - 1; j >= 0; j--)
            {
                if (bullets[i].Collider.Contains(enemies[j].Collider))
                {
                    bullets.RemoveAt(i);
                    enemies[j].Kill(deltaTime);
                    break;
                }
            }
        }

        player.Update(deltaTime, totalTime);

        oldKeyboard = Keyboard.GetState();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        float totalTime = (float)gameTime.TotalGameTime.TotalSeconds;

        GraphicsDevice.Clear(file.BgColor);

        spriteBatch.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, transformMatrix: camera.Transform);
        {
            // Draw Levels layers
            foreach (LDtkLevel level in world.Levels)
            {
                renderer.RenderPrerenderedLevel(level);
            }

            player.Draw(totalTime);

            // Draw Entities
            for (int i = 0; i < enemies.Count; i++)
            {
                enemies[i].Draw(totalTime);
            }

            // Draw bullets
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].Draw();
            }
        }

        gun.Draw();

        spriteBatch.End();

        base.Draw(gameTime);
    }

    private static void DebugInput(KeyboardState old)
    {
        if (old.IsKeyUp(Keys.F1) && Keyboard.GetState().IsKeyDown(Keys.F1))
        {
            DebugF1 = !DebugF1;
        }

        if (old.IsKeyUp(Keys.F2) && Keyboard.GetState().IsKeyDown(Keys.F2))
        {
            DebugF2 = !DebugF2;
        }

        if (old.IsKeyUp(Keys.F3) && Keyboard.GetState().IsKeyDown(Keys.F3))
        {
            DebugF3 = !DebugF3;
        }
    }
}
