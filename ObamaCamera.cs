using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using System.IO;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ModLoader.IO;
using Terraria.Localization;
using Terraria.Utilities;

namespace ObamaCamera
{
	public class Bebeq : ModPlayer
	{
		public MyConfig config => ModContent.GetInstance<MyConfig>();

		Vector2 screenCache = new Vector2(0,0);

		public int SourcePlayerIndex = -1;
		public int SourceNPCIndex = -1;
		public int SourceProjectileIndex = -1;

		public int camerashake;

		public override void OnHitAnything(float x, float y, Entity victim) {
			if (!config.DOOM) {
				return;
			}
			if (Vector2.Distance(new Vector2(x,y), player.Center) < 700) {
				camerashake = config.ShakeInt;
				if (victim is NPC target) {
					if (target.life <= 0) {
						camerashake = config.ShakeInt*2;
					}
				}
				if (victim is Player asd) {
					if (asd.statLife <= 0) {
						camerashake = config.ShakeInt*2;
					}
				}
			}

		}
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (config.CameraShake) {
				camerashake = config.ShakeInt;
			}
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			SourcePlayerIndex = damageSource.SourcePlayerIndex;
			SourceNPCIndex = damageSource.SourceNPCIndex;
			SourceProjectileIndex = damageSource.SourceProjectileIndex;
			if (config.CameraShake) {
				camerashake = config.ShakeInt*2;
			}
		}

		public override void ModifyScreenPosition() {

			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			bool flag1 = true;
			if (!player.dead && config.CameraFollow != "Player") {
				int index = -1;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					float between = Vector2.Distance(npc.Center, player.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
					bool boss = npc.boss && npc.active && npc.realLife < 1;
					if ((closest || index == -1) && boss) {
						index = i;
						targetCenter = npc.Center;
					}
				}
				if (index > -1) {
					NPC npc = Main.npc[index];
					if (config.CameraFollow == "Boss and Player") {
						Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center,0.05f);
						if (config.SmoothCamera){screenCache = Main.screenPosition;}
						flag1 = false;
					}
					else {
						if (config.SmoothCamera){screenCache = Vector2.Lerp(screenCache,npc.Center - centerScreen,0.05f);}
						else {Main.screenPosition = npc.Center - centerScreen;}
						flag1 = false;
					}
				}
			}
			if (player.dead) {
				Vector2 pos = player.Center;
				if (SourcePlayerIndex > -1) {
					pos = Main.player[SourcePlayerIndex].Center - centerScreen;
				}
				if (SourceNPCIndex > -1) {
					pos = Main.npc[SourceNPCIndex].Center - centerScreen;
				}
				if (SourceProjectileIndex > -1) {
					pos = Main.projectile[SourceProjectileIndex].Center - centerScreen;
				}
				if (config.SmoothCamera) {screenCache = Vector2.Lerp(screenCache,pos,0.1f);}
				else {Main.screenPosition = pos;}

				flag1 = false;	
			}
			else {
				SourcePlayerIndex = -1;
				SourceNPCIndex = -1;
				SourceProjectileIndex = -1;
			}
			if (config.SmoothCamera) {
				Main.screenPosition = screenCache;
				if (flag1){screenCache = Vector2.Lerp(screenCache,player.Center - centerScreen,0.1f);}
			}

			if (config.CameraShake) {
				if (camerashake > 0)
				{
					Main.screenPosition += new Vector2(Main.rand.Next(-camerashake, camerashake + 1), Main.rand.Next(-camerashake, camerashake + 1));
					camerashake -= 1;
				}
			}
		}
	}
	public class ObamaCamera : Mod{}
	[Label("Obama Camera")]
	public class MyConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		// public override ConfigScope Mode => ConfigScope.ServerSide;
		//public static MyConfig get => ModContent.GetInstance<MyConfig>();

		[Header("Screen Shake")]

		[Label("Screen Shake On Hurt")]
		[Tooltip("Shake the screen whenever the you get hurt")]
		[DefaultValue(true)]
		public bool CameraShake;

		[Label("Screen Shake On Kill")]
		[Tooltip("Shake the screen whenever the you hit n kill enemy")]
		[DefaultValue(false)]
		public bool DOOM;

		[Label("Screen Shake Intensity")]
		[Tooltip("the intensity of the screenshake \n [default is 4]")]
		[Range(1, 15)]
		[Increment(1)]
		[DefaultValue(4)]
		[Slider] 
		public int ShakeInt;

		[Header("Camera")]

		[Label("Smooth camera")]
		[Tooltip("make the camera smooth")]
		[DefaultValue(true)]
		public bool SmoothCamera;

		[Label("Death cam")]
		[Tooltip("Allow you to see what kill you")]
		[DefaultValue(true)]
		public bool DeathCam;

		[DrawTicks]
		[OptionStrings(new string[] { "Player","Boss and Player", "Boss only"})]
		[Label("Camera follow ")]
		[Tooltip("Make the camera follow the player , boss and player or the boss only")]
		[DefaultValue("Boss and Player")]
		public string CameraFollow;

		//public override void OnChanged() {}
	}
}