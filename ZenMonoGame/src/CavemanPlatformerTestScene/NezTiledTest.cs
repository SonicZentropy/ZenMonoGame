﻿using Microsoft.Xna.Framework;
using Nez;
using Nez.Samples;
using Nez.Sprites;
using Nez.Textures;
using Nez.Tiled;

namespace LDtkMonogameExample;

public class NezTiledTestGame : Core {
	protected override void Initialize() {
		base.Initialize();

		Window.AllowUserResizing = true;
		Scene = new NezTiledTestScene();
	}
}

public class NezTiledTestScene : Scene {
	public override void Initialize()
		{
			// setup a pixel perfect screen that fits our map
			SetDesignResolution(640, 480, SceneResolutionPolicy.ShowAllPixelPerfect);
			Screen.SetSize(640 * 2, 480 * 2);

			// load up our TiledMap
			var map = Content.LoadTiledMap("Content/Platformer/tiledMap.tmx");
			var playerSpawn = map.GetObjectGroup("objects").Objects["spawn"];
			var playerSpawnPosition = new Vector2(playerSpawn.X, playerSpawn.Y);
			
			// cell "6" from the Tilemap will be used as our items sprite
			var itemSprite = new Sprite(map.Tilesets[0].Image.Texture, map.Tilesets[0].TileRegions[6]);

			var tiledEntity = CreateEntity("tiled-map-entity");
			tiledEntity.AddComponent(new TiledMapRenderer(map, "main"));

			// create our Player and add a TiledMapMover to handle collisions with the tilemap
			var playerEntity = CreateEntity("player", playerSpawnPosition);
			playerEntity.AddComponent(new Caveman());
			playerEntity.AddComponent(new BoxCollider(-8, -16, 16, 32));
			playerEntity.AddComponent(new TiledMapMover(map.GetLayer<TmxLayer>("main")));
			
			// create collectible items at the "item"-points from the TiledMap
			var itemSpawns = map.GetObjectGroup("collectibles").Objects;
			
			foreach (TmxObject itemSpawnPoint in itemSpawns)
			{
				var item = CreateEntity("item", new Vector2(itemSpawnPoint.X, itemSpawnPoint.Y));
				
				// add a Trigger-Collider to the item
				var collider = item.AddComponent(new BoxCollider(16, 16));
				collider.IsTrigger = true;
				
				item.AddComponent(new PlatformerItem());
				item.AddComponent(new SpriteRenderer(itemSprite));
			}
			
			// create the danger-zone polygons
			var dangerSpawns = map.GetObjectGroup("danger-zone").Objects;
			
			foreach (TmxObject danger in dangerSpawns)
			{
				var zone = CreateEntity("danger-zone", new Vector2(danger.X, danger.Y));
			
				// add a Trigger-Collider to the zone
				var collider = zone.AddComponent(new PolygonCollider(danger.Points));
				collider.IsTrigger = true;
				
				zone.AddComponent(new PlatformerDangerZone(playerSpawnPosition));
			}
		}
}