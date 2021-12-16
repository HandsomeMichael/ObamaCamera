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
using System.Reflection;
using MonoMod.RuntimeDetour.HookGen;

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
			if (config.HitShake) {
				if (camerashake < config.ShakeInt*2) {
					camerashake += config.ShakeInt/2;
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (config.KillShake) {
				if (target.life <= 0) {
					camerashake = config.ShakeInt*2;
				}	
			}
		}
		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
			if (config.KillShake) {
				if (target.life <= 0) {
					camerashake = config.ShakeInt*2;
				}	
			}
		}
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (config.HurtShake) {
				camerashake = config.ShakeInt;
			}
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			SourcePlayerIndex = damageSource.SourcePlayerIndex;
			SourceNPCIndex = damageSource.SourceNPCIndex;
			SourceProjectileIndex = damageSource.SourceProjectileIndex;
			if (config.HurtShake) {
				camerashake = config.ShakeInt*2;
			}
		}
		public override void OnEnterWorld(Player player) {
			if (config.SmoothCamera) {
				screenCache = player.Center - new Vector2(Main.screenWidth/2,Main.screenHeight/2);
			}
		}
		public override void OnRespawn(Player player) {
			if (config.QuickRespawn) {
				screenCache = player.Center - new Vector2(Main.screenWidth/2,Main.screenHeight/2);
			}
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (ObamaCamera.BossLook.Current) {
				QuickLook = true;
			}
		}
		public bool QuickLook;
		public void CameraMod() {
			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			bool flag1 = true;
			string type = config.CameraFollow;
			if (QuickLook) {
				type = "Boss only";
			}
			if (!player.dead && (type != "Player")) {
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
					if (type == "Boss and Player") {
						if (config.SmoothCamera){
							Vector2 pos = Vector2.Lerp(Main.screenPosition,npc.Center,0.05f);
							screenCache = Vector2.Lerp(screenCache,pos,0.1f);
						}
						else {
							Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center,0.05f);
						}
						flag1 = false;
					}
					else {
						if (config.SmoothCamera){
							screenCache = Vector2.Lerp(screenCache,npc.Center - centerScreen,0.05f);
						}
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

			if (config.HurtShake || config.KillShake || config.HitShake) {				
				if (camerashake > 0)
				{
					Main.screenPosition += new Vector2(Main.rand.Next(-camerashake, camerashake + 1), Main.rand.Next(-camerashake, camerashake + 1));
					camerashake -= 1;
				}
			}
			QuickLook = false;
		}
	}
	public class ObamaCamera : Mod
	{
		public static ModHotKey BossLook;
		public override void Load() {

			BossLook = RegisterHotKey("Quick Look At Boss", "V");

			ObamaHooks.On_ModifyScreenPosition += DetourBoomBom;
		}
		public override void Unload() {

			BossLook = null;

			ObamaHooks.On_ModifyScreenPosition -= DetourBoomBom;
		}
		static void DetourBoomBom(ObamaHooks.orig_ModifyScreenPosition orig, Player player) {
			if (!MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
			orig(player);
			if (MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
		}
		
	}
	[Label("Obama Camera")]
	public class MyConfig : ModConfig
	{
		public override ConfigScope Mode => ConfigScope.ClientSide;
		// public override ConfigScope Mode => ConfigScope.ServerSide;
		public static MyConfig get => ModContent.GetInstance<MyConfig>();

		[Header("Screen Shake")]

		[Label("Screen Shake On Hurt")]
		[Tooltip("Shake the screen whenever you get HURT")]
		[DefaultValue(true)]
		public bool HurtShake;

		[Label("Screen Shake On Kill")]
		[Tooltip("Shake the screen whenever you KILL enemy")]
		[DefaultValue(false)]
		public bool KillShake;

		[Label("Screen Shake On Hit")]
		[Tooltip("Shake the screen whenever you HIT enemy")]
		[DefaultValue(false)]
		public bool HitShake;

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

		[Label("Quick Camera Respawn")]
		[Tooltip("immediately set the camera positon to player center on respawn \nonly works when 'Smooth camera' activated")]
		[DefaultValue(false)]
		public bool QuickRespawn;

		[Label("Override Camera")]
		[Tooltip("Override modify screen position")]
		[DefaultValue(false)]
		public bool Override;

		//public override void OnChanged() {}
	}
	public static class ObamaHooks
	{
		public delegate void orig_ModifyScreenPosition(Player player);
		public delegate void Hook_ModifyScreenPosition(orig_ModifyScreenPosition orig, Player player);

		public static event Hook_ModifyScreenPosition On_ModifyScreenPosition {
			add {
				HookEndpointManager.Add<Hook_ModifyScreenPosition>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.PlayerHooks").GetMethod("ModifyScreenPosition", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
			}
			remove {
				HookEndpointManager.Remove<Hook_ModifyScreenPosition>(typeof(Mod).Assembly.GetType("Terraria.ModLoader.PlayerHooks").GetMethod("ModifyScreenPosition", BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic), value);
			}
		}
	}
}
