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
using Microsoft.Xna.Framework.Audio;
using Terraria.Audio;
using Terraria.Graphics.Capture;

namespace ObamaCamera
{
	public class BabuGaming : GlobalTile
	{

		static Dictionary<int, int> PowerID = new Dictionary<int, int>
		{
			{ 37, 50 },
			{ 56, 65 },
			{ 25, 65 },
			{ 117, 65 },
			{ 58, 65 },
			{ 203, 65 },
			{ 107, 100 },
			{ 221, 100 },
			{ 108, 110 },
			{ 222, 110 },
			{ 111, 150 },
			{ 223, 150 },
			{ 226, 210 },
			{ 211, 200 }
		};
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			Player player = Main.LocalPlayer;
			var p = player.GetModPlayer<Bebeq>();
			if (MyConfig.get.TileShake && p.pickX == i && p.pickY == j && fail) {
				ModTile mt = TileLoader.GetTile(type);
				string text = "";

				if (mt != null && p.pickPower > 0 && p.pickPower < mt.minPick)
					text = $"Require {mt.minPick}% Mining Power";
				else if (PowerID.ContainsKey(type) && p.pickPower > 0 && p.pickPower < PowerID[type])
					text = $"Require {PowerID[type]}% Mining Power";

				if (text != "") {
					CombatText.NewText(player.getRect(),Color.Red,text);
					if (MyConfig.get.ShakeLimit) {p.camerashake += MyConfig.get.ShakeInt;}
					else {p.camerashake = MyConfig.get.ShakeInt;}
				}
			}
		}
	}
	public class Moonlord : GlobalNPC
	{
		public override void PostAI(NPC npc) {
			if (MyConfig.get.DeathAnim && npc.type == NPCID.MoonLordCore && npc.ai[0] == 2f && npc.ai[0] > 0f && npc.active) {
				ObamaCamera.Moonlord = true;
			}
			float distance = Vector2.Distance(npc.Center,Main.LocalPlayer.Center);
			if (distance < 1500f && (npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.TheDestroyer) && MyConfig.get.WormShake) {
				Tile tile = Framing.GetTileSafely((int)(npc.position.X/16), (int)(npc.position.Y/16));
				if (tile.active() && Main.tileSolid[tile.type]) {
					int num = MyConfig.get.ShakeInt;
					if (npc.type == NPCID.TheDestroyer) {num += num/2;}
					if (MyConfig.get.ShakeLimit) {
						Main.player[npc.target].GetModPlayer<Bebeq>().camerashake += num;
					}
					else {
						Main.player[npc.target].GetModPlayer<Bebeq>().camerashake = num;
					}
				}
			}
		}
	}
	public class Bebeq : ModPlayer
	{
		public MyConfig config => ModContent.GetInstance<MyConfig>();

		Vector2 screenCache = new Vector2(0,0);

		public int SourcePlayerIndex = -1;
		public int SourceNPCIndex = -1;
		public int SourceProjectileIndex = -1;

		public int pickX;
		public int pickY;
		public int pickPower;

		public int camerashake;

		public override void ResetEffects() {
			pickX = -1;
			pickY = -1;
		}
		public override void OnHitAnything(float x, float y, Entity victim) {
			if (config.HitShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt/2;}
				else if (camerashake < config.ShakeInt*2) {
					camerashake += config.ShakeInt/2;
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (config.KillShake) {
				if (target.life <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitPvp(Item item, Player target, int damage, bool crit) {
			if (config.KillShake) {
				if (target.statLife <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
			if (config.KillShake) {
				if (target.statLife <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
			if (config.KillShake) {
				if (target.life <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else{camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (config.HurtShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt;}
				else {camerashake = config.ShakeInt;}
			}
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			SourcePlayerIndex = damageSource.SourcePlayerIndex;
			SourceNPCIndex = damageSource.SourceNPCIndex;
			SourceProjectileIndex = damageSource.SourceProjectileIndex;
			if (config.HurtShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
				else{camerashake = config.ShakeInt*2;}
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
			if (ObamaCamera.SwitchFollow.JustPressed) {
				if (config.CameraFollow == "Player") {
					config.CameraFollow = "Boss and Player";
				}
				else if (config.CameraFollow == "Boss and Player"){
					config.CameraFollow = "Boss only";
				}
				else if (config.CameraFollow == "Boss only"){
					config.CameraFollow = "Enemy and Player";
				}
				else {
					config.CameraFollow = "Player";
				}
				CombatText.NewText(player.getRect(),Color.White,config.CameraFollow);
			}
		}
		public bool QuickLook;
		public void CameraMod() {
			if (!ObamaCamera.Enable) {
				return;
			}
			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			if (config.Demolitionist) {
				camerashake = config.ShakeInt*2;
			}

			bool flag1 = true;
			string type = config.CameraFollow;
			if (QuickLook || ObamaCamera.Moonlord) {
				type = "Boss only";
			}
			if (!player.dead && type != "Player") {
				int index = -1;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxNPCs; i++) {
					NPC npc = Main.npc[i];
					float between = Vector2.Distance(npc.Center, player.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
					bool boss = npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.TheDestroyer;
					//we hack system
					if (type == "Enemy and Player") {
						boss = !npc.friendly && npc.damage > 0 && npc.life > 0 && !npc.townNPC && Vector2.Distance(npc.Center,player.Center) < 1100f;
					}
					if (npc.type == NPCID.MoonLordCore && npc.ai[0] == 2f && npc.ai[0] > 0f && npc.active) {
						index = i;
						targetCenter = npc.Center;
						type = "Boss only";
						break;
					}
					if ((closest || index == -1) && boss && npc.active) {
						index = i;
						targetCenter = npc.Center;
					}
				}
				if (index > -1) {
					NPC npc = Main.npc[index];
					if (type == "Enemy and Player") {type = "Boss and Player";}
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
			if (config.Demon) {
				int index = -1;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxProjectiles; i++)
				{
					Projectile proj = Main.projectile[i];
					float between = Vector2.Distance(proj.Center, player.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
					bool boss = Main.projPet[proj.type] && proj.damage > 0;
					if ((closest || index == -1) && boss) {
						index = i;
						targetCenter = proj.Center;
					}
				}
				if (index > 0) {
					if (config.SmoothCamera) {screenCache = Vector2.Lerp(screenCache,targetCenter - centerScreen,0.1f);}
					else {Main.screenPosition = targetCenter - centerScreen;}
				}
			}
			if (config.OverhaulMouse && Main.hasFocus && !Main.playerInventory && player.talkNPC < 0) {
				if (config.SmoothCamera ) {
					screenCache = Vector2.Lerp(screenCache,Main.MouseWorld - centerScreen,0.01f*(float)config.OverhaulDistance);
				}
				else {
					Main.screenPosition = Vector2.Lerp(Main.screenPosition,Main.MouseWorld - centerScreen,0.01f*(float)config.OverhaulDistance);
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

			if (camerashake > 0)
			{
				Main.screenPosition += new Vector2(Main.rand.Next(-camerashake, camerashake + 1), Main.rand.Next(-camerashake, camerashake + 1));
				camerashake -= 1;
			}
			QuickLook = false;
			ObamaCamera.Moonlord = false;

			// lol the lunix
			if (config.DemonBanner && !player.dead) {
				string[] lol = {"index out of bounds","spriteBatch didn't end correctly","System.ObjectDisposedException Cannot access a disposed object.","Object reference not set to an instance of an object"};
				Main.NewText(Main.rand.Next(lol)+". see more on client.log",Color.Red);
				player.Hurt(PlayerDeathReason.ByCustomReason($"{player.name} got spammed by error log to death"), 100, player.direction);
			}
		}
	}
	public class ObamaCamera : Mod
	{
		public static ModHotKey BossLook;
		public static ModHotKey SwitchFollow;

		public static bool Moonlord;
		public static bool Enable;

		public override object Call(params object[] args) {
			string github = ", read the instruction from github";
			if (args[0] is Player player) {
				if (args.Length == 1 ) {Main.NewText("Need More Argument"+github);}
				else if (args[1] is string call) {
					if (call == "GetShake") {
						return player.GetModPlayer<Bebeq>().camerashake;
					}
					else if (call == "SetShake") {
						if (args.Length != 3) {Main.NewText("Need More Argument"+github);}
						if (args[2] is int num) {
							player.GetModPlayer<Bebeq>().camerashake = num;
						}
						else {Main.NewText("argument 3 has to be intreger"+github);}
					}
					else if (call == "AddShake") {
						if (args[2] is int num) {
							player.GetModPlayer<Bebeq>().camerashake += num;
						}
						else {Main.NewText("argument 3 has to be intreger"+github);}
					}
					else if (call == "GetBossLook") {
						return player.GetModPlayer<Bebeq>().QuickLook;
					}
					else if (call == "SetBossLook") {
						if (args[2] is bool num) {
							player.GetModPlayer<Bebeq>().QuickLook = num;
						}
						else {Main.NewText("argument 3 has to be bool"+github);}
					}
					else {Main.NewText("Unknown player calls"+github);}
				}
				else {Main.NewText("argument 2 has to be string"+github);}
			}
			else if (args[0] is string call) {
				if (call == "CameraFollow") {
					return MyConfig.get.CameraFollow;
				}
				else if (call == "SmoothCamera") {
					return MyConfig.get.SmoothCamera;
				}
				else if (call == "Disable") {
					ObamaCamera.Enable = false;
				}
				else if (call == "Enable") {
					ObamaCamera.Enable = true;
				}
				else {Main.NewText("Unknown config"+github);}
			}
			else {Main.NewText("Unknown mod calls"+github);}
			return null;
		}
		public override void Load() {
			Enable = true;

			BossLook = RegisterHotKey("Quick Look At Boss", "V");
			SwitchFollow = RegisterHotKey("Quick Switch Camera", "C");

			ObamaHooks.On_ModifyScreenPosition += DetourBoomBom;
			Hacc.AddHacc();
		}
		public override void Unload() {

			BossLook = null;
			SwitchFollow = null;

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
		[OptionStrings(new string[] { "Player","Boss and Player", "Boss only","Enemy and Player"})]
		[Label("Camera follow ")]
		[Tooltip("Make the camera follow other things")]
		[DefaultValue("Boss and Player")]
		public string CameraFollow;

		[Label("Camera lock on moonlord death animation")]
		[Tooltip("Make the Camera lock on moonlord death animation")]
		[DefaultValue(true)]
		public bool DeathAnim;

		[Label("Camera Follow Mouse")]
		[Tooltip("Make the camera follow mouse \nlike Terraria overhaul mod")]
		[DefaultValue(false)]
		public bool OverhaulMouse;

		[Label("Camera Follow Mouse Distance")]
		[Tooltip("The Camera distance between player and mouse\n [default is 3]")]
		[Range(1, 10)]
		[Increment(1)]
		[DefaultValue(3)]
		[DrawTicks]
		[Slider] 
		public int OverhaulDistance;

		[Label("Quick Camera Respawn")]
		[Tooltip("immediately set the camera positon to player center on respawn \nonly works when 'Smooth camera' is activated")]
		[DefaultValue(false)]
		public bool QuickRespawn;

		[Label("Override Other Mod")]
		[Tooltip("Override any mod that modify screen position")]
		[DefaultValue(false)]
		public bool Override;

		[Header("Screen Shake")]

		[Label("Screen Shake On Hurt")]
		[Tooltip("Shake the screen whenever you get HURT")]
		[DefaultValue(true)]
		public bool HurtShake;

		[Label("Screen Shake On Kill Enemy")]
		[Tooltip("Shake the screen whenever you KILL enemy")]
		[DefaultValue(false)]
		public bool KillShake;

		[Label("Screen Shake On Hit Enemy")]
		[Tooltip("Shake the screen whenever you HIT enemy")]
		[DefaultValue(false)]
		public bool HitShake;

		[Label("Screen Shake On Roar and Explode")]
		[Tooltip("Shake the screen whenever there is a ROAR or EXPLODE")]
		[DefaultValue(true)]
		public bool RoarShake;

		[Label("Screen Shake On Worm Boss")]
		[Tooltip("Shake the screen whenever a worm boss is digging")]
		[DefaultValue(false)]
		public bool WormShake;

		[Label("Screen Shake On Failed breaking tile")]
		[Tooltip("Shake the screen whenever the the player failed at breaking tiles")]
		[DefaultValue(true)]
		public bool TileShake;

		[Label("Screen Shake Limit Breaker")]
		[Tooltip("Allow the screen shake effect be stacked")]
		[DefaultValue(false)]
		public bool ShakeLimit;

		[Label("Screen Shake Intensity")]
		[Tooltip("the intensity of the screenshake \n [default is 4]")]
		[Range(1, 15)]
		[Increment(1)]
		[DefaultValue(4)]
		[Slider] 
		[DrawTicks]
		public int ShakeInt;
		

		[Header("Experimental and Funny Stuff")]

		[Label("Screen Shake Forever")]
		[Tooltip("Make the screen shaked forever, funni")]
		[DefaultValue(false)]
		public bool Demolitionist;

		[Label("Camera follow nearest minion")]
		[Tooltip("Make the camera follow nearest player minion")]
		[DefaultValue(false)]
		public bool Demon;

		[Label("Simulate what most Linux users see")]
		[Tooltip("yes")]
		[DefaultValue(false)]
		public bool DemonBanner;

		//public override void OnChanged() {}
	}
	public class Hacc
	{
		//will be applied in mod class 
		// Item_14
		// Item_62
		// Zombie_20
		// Zombie_104
		// Zombie_92
		public static void AddHacc() {
			On.Terraria.Main.PlaySound_int_Vector2_int += OnPlaySound;
			On.Terraria.Main.PlaySound_LegacySoundStyle_Vector2 += OnPlaySound2;
			On.Terraria.Main.PlaySound_LegacySoundStyle_int_int += OnPlaySound3;
			On.Terraria.Main.PlaySound_int_int_int_int_float_float += OnPlaySound4;
			On.Terraria.Player.PickTile += TilePatch;
		}
		//sound
		static void SoundShake(int type, int Style) {
			int num = 0;
			if ((type == SoundID.Roar && (Style == 0 || Style == 2)) || type == SoundID.ForceRoar) {
				num = MyConfig.get.ShakeInt*2;
			}
			if (type == SoundID.Zombie && (Style == 20 || Style == 104 || Style == 92)) {
				num = MyConfig.get.ShakeInt*2;
			}
			if (type == SoundID.Item && (Style == 14 || Style == 62)){
				num = MyConfig.get.ShakeInt*2;
			}
			if (num > 0) {
				if (MyConfig.get.ShakeLimit) {
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake += num;
				}
				else {
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake = num;
				}
			}
		}
		//the code
		private static void OnPlaySound(On.Terraria.Main.orig_PlaySound_int_Vector2_int orig,int type, Vector2 position, int Style){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(position,Main.LocalPlayer.Center) < 700f) {
					SoundShake(type,Style);
				}
			}
			orig(type,position,Style);
		}
		private static SoundEffectInstance OnPlaySound2(On.Terraria.Main.orig_PlaySound_LegacySoundStyle_Vector2 orig,LegacySoundStyle type, Vector2 position){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(position,Main.LocalPlayer.Center) < 700f) {
					SoundShake(type.SoundId,type.Style);
				}
			}
			return orig(type,position);
		}
		private static SoundEffectInstance OnPlaySound3(On.Terraria.Main.orig_PlaySound_LegacySoundStyle_int_int orig,LegacySoundStyle type, int x, int y){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(new Vector2(x,y),Main.LocalPlayer.Center) < 700f) {
					SoundShake(type.SoundId,type.Style);
				}
			}
			return orig(type,x,y);
		}
		private static SoundEffectInstance OnPlaySound4(On.Terraria.Main.orig_PlaySound_int_int_int_int_float_float orig,int type, int x, int y, int Style, float volumeScale, float pitchOffset){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(new Vector2(x,y),Main.LocalPlayer.Center) < 700f) {
					SoundShake(type,Style);
				}
			}
			return orig(type, x, y, Style,  volumeScale, pitchOffset);
		}
		static void TilePatch(On.Terraria.Player.orig_PickTile orig,Player self,int x, int y, int pickPower)
		{
			if (self.whoAmI == Main.myPlayer) {
				self.GetModPlayer<Bebeq>().pickX = x;
				self.GetModPlayer<Bebeq>().pickY = y;
				self.GetModPlayer<Bebeq>().pickPower = pickPower;
			}
			orig(self,x,y,pickPower);
		}
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
