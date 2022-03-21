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
using System.Diagnostics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;
using Terraria.ModLoader.Config.UI;
using Terraria.UI;
using Terraria.UI.Chat;
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
using System.Text.RegularExpressions;
using System.Collections.Concurrent;
using ReLogic.Graphics;
using System.Runtime;
using Microsoft.Xna.Framework.Input;

// all in one file lol
// you can already tell that this code is a nightmare :pe:
namespace ObamaCamera
{
	public class BabuGaming : GlobalTile
	{
		static Dictionary<int, int> PowerID = new Dictionary<int, int>
		{
			{ TileID.Demonite,55},
			{ TileID.Crimtane,55},
			{ TileID.DesertFossil,65},
			{ TileID.Meteorite, 50 },
			{ TileID.Obsidian, 65 },
			{ TileID.Ebonstone, 65 },
			{ TileID.Pearlstone, 65 },
			{ TileID.Hellstone, 65 },
			{ TileID.Crimstone, 65 },
			{ TileID.Cobalt, 100 },
			{ TileID.Palladium, 100 },
			{ TileID.Mythril, 110 },
			{ TileID.Orichalcum, 110 },
			{ TileID.Adamantite, 150 },
			{ TileID.Titanium, 150 },
			{ TileID.LihzahrdBrick, 210 },
			{ TileID.Chlorophyte, 200 }
		};
		public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem) {
			if (MyConfig.get.TileShake && Bebeq.pickX == i && Bebeq.pickY == j && fail) {
				ModTile mt = TileLoader.GetTile(type);
				string text = "";
				if (mt != null && Bebeq.pickPower > 0 && Bebeq.pickPower < mt.minPick)
					text = $"Require {mt.minPick}% Mining Power";
				else if ((PowerID.ContainsKey(type) || Main.tileDungeon[type]) && Bebeq.pickPower > 0) {
					int power = 0;
					if (Main.tileDungeon[type]) {
						power = 65;
					}
					else {power = PowerID[type];}
					if ((type == 22 || type == 204) && j < Main.worldSurface ){power = 0;}

					if (Main.tileDungeon[type] && Bebeq.pickPower < 65)
					{
						bool a = (double)i < (double)Main.maxTilesX * 0.35;
						bool b = (double)i > (double)Main.maxTilesX * 0.65;
						if (!a && !b){power = 0;}
					}
					if (Bebeq.pickPower < power) {
						text = $"Require {power}% Mining Power";
					}
				}

				if (text != "") {
					CombatText.NewText(Main.LocalPlayer.getRect(),Color.Red,text);
					if (MyConfig.get.ShakeLimit) {Bebeq.camerashake += MyConfig.get.ShakeInt;}
					else {Bebeq.camerashake = MyConfig.get.ShakeInt;}
				}
			}
		}
	}
	public class Moonlord : GlobalNPC
	{
		static bool ThereIsOtherBoss(int index) {
			if (MyConfig.get.BossIntroMult) {return false;}
			for (int i = 0; i < Main.maxNPCs; i++)
			{
				if (i != index) {
					NPC npc = Main.npc[i];
					if (npc.active && 
					(npc.boss || npc.type == NPCID.WallofFleshEye || npc.type == NPCID.EaterofWorldsHead) &&
					 npc.type != NPCID.WallofFlesh && npc.realLife < 0 && npc.type != NPCID.MoonLordHand && 
					 npc.type != NPCID.MoonLordHead) {
						return true;
					}
				}
			}
			return false;
		}
		public override void PostAI(NPC npc) {
			if (MyConfig.get.BossIntro && (npc.realLife < 0 || npc.type == NPCID.TheDestroyer) &&
				npc.type != NPCID.MoonLordHand && npc.type != NPCID.MoonLordHead &&
				!ObamaCamera.Moonlord && !ObamaCamera.bossEncounter.Contains(npc.type) && 
				(npc.boss || npc.type == NPCID.EaterofWorldsHead) 
				&& ObamaCamera.NPCFocus == -1 && !ThereIsOtherBoss(npc.whoAmI)) {

				ObamaCamera.NPCFocus = npc.whoAmI;
				string subtitle = "";
				string name = npc.FullName;
				Texture2D texture = null;
				if (npc.type == NPCID.KingSlime) {subtitle = "The King Of All Evil Slimes";}
				if (npc.type == NPCID.EaterofWorldsHead) {subtitle = "The Giant Worm Eater";}
				if (npc.type == NPCID.EyeofCthulhu) {subtitle = "The Giant Tortured Eye";}
				if (npc.type == NPCID.QueenBee) {subtitle = "The Queen Of All Bees";}
				if (npc.type == NPCID.WallofFleshEye || npc.type == NPCID.WallofFlesh) {subtitle = "The King Of The Underworld";}
				if (npc.type == NPCID.TheDestroyer) {subtitle = "The Giant Mechanical Eater";}
				if (npc.type == NPCID.SkeletronHead) {subtitle = "The Keeper Of Dungeons";}
				if (npc.type == NPCID.SkeletronPrime) {subtitle = "The Evil Mechanical Skull";}
				if (npc.type == NPCID.DukeFishron) {subtitle = "The Mutated Pig Fish";}
				if (npc.type == NPCID.Plantera) {subtitle = "The Cursed Plant";}
				if (npc.type == NPCID.Golem) {subtitle = "The Keeper Of Lihzhard Temples";}
				if (npc.type == NPCID.BrainofCthulhu) {subtitle = "The Tortured Brain Of Cthulhu";}
				if (npc.type == NPCID.CultistBoss) {subtitle = "The Moon Cultist";}
				if (npc.type == NPCID.MartianSaucer) {subtitle = "The Unknown Floating Object";}
				if (npc.type == NPCID.MoonLordCore) {
					name = "Moonlord";
					subtitle = "The Lord Of The Moon";
				}
				if (npc.type == NPCID.Retinazer || npc.type == NPCID.Spazmatism) {
					name = "Twins";
					subtitle = "The Duo Mechanical Eye";
					if (npc.type == NPCID.Retinazer) {ObamaCamera.bossEncounter.Add(NPCID.Spazmatism);}
					else {ObamaCamera.bossEncounter.Add(NPCID.Retinazer);}
				}
				bool save = true;
				var meme = ModLoader.GetMod("CalamityMod");
				if (meme != null) {
					if (npc.type == meme.NPCType("DesertScourgeHead")) {
						subtitle = "The Dried Gluttonous Husk";
					}
					if (npc.type == meme.NPCType("AquaticScourgeHead")) {
						subtitle = "The Polluted And Hungry Worm";
					}
					if (npc.type == meme.NPCType("AstrumAureus")) {
						subtitle = "The Titanic Infected Cyborg";
					}
					if (npc.type == meme.NPCType("AstrumDeusHeadSpectral")) {
						subtitle = "The Star Infected Destroyer";
					}
					if (npc.type == meme.NPCType("BrimstoneElemental")) {
						subtitle = "The Elemental Revenge";
					}
					if (npc.type == meme.NPCType("Birb")) {
						subtitle = "The Twisted Scientific Failure";
					}
					if (npc.type == meme.NPCType("Calamitas")) {
						subtitle = "The Brimstone Chaos";
					}
					if (npc.type == meme.NPCType("Yharon")) {
						name = "Yharon";
						subtitle = "The Jungle Dragon";
					}
					if (npc.type == meme.NPCType("SlimeGodCore")) {
						subtitle = "The God of Slimes";
					}
					if (npc.type == meme.NPCType("Signus")) {
						name = "Signus";
						subtitle = "The Envoy of the Devourer";
					}
					if (npc.type == meme.NPCType("StormWeaverHead") || npc.type == meme.NPCType("StormWeaverHeadNaked")) {
						ObamaCamera.bossEncounter.Add(meme.NPCType("StormWeaverHeadNaked"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("StormWeaverHead"));
						subtitle = "The Long Storm";
						save = false;
					}
					if (npc.type == meme.NPCType("Providence")) {
						name = "Providence";
						subtitle = "The Profaned Goddess";
					}
					if (npc.type == meme.NPCType("ProfanedGuardianBoss") || npc.type == meme.NPCType("ProfanedGuardianBoss2") || npc.type == meme.NPCType("ProfanedGuardianBoss3")) {
						ObamaCamera.bossEncounter.Add(meme.NPCType("ProfanedGuardianBoss"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("ProfanedGuardianBoss2"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("ProfanedGuardianBoss3"));
						subtitle = "The Guardian Of The Profaned Flame";
						save = false;
					}
					if (npc.type == meme.NPCType("PlaguebringerGoliath")) {
						subtitle = "The Amalgam of Infection";
					}
					if (npc.type == meme.NPCType("PerforatorHive")) {
						subtitle = "The Abomination of Flesh";
					}
					if (npc.type == meme.NPCType("OldDuke")) {
						subtitle = "The Mutant Terror of The Seas";
					}
					if (npc.type == meme.NPCType("Leviathan") && npc.type == meme.NPCType("Siren")) {
						ObamaCamera.bossEncounter.Add(meme.NPCType("Leviathan"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("Siren"));
						name = "Leviathan and Anahita";
						subtitle = "The Voices of Sea Monster";
						save = false;
					}
					if (npc.type == meme.NPCType("HiveMind")) {
						subtitle = "The Clustered Corrupted Hive";
					}
					if (npc.type == meme.NPCType("Draedon") || npc.type == meme.NPCType("Artemis") 
					|| npc.type == meme.NPCType("Apollo") || npc.type == meme.NPCType("ThanatosHead") 
					|| npc.type == meme.NPCType("AresBody")) {
						ObamaCamera.bossEncounter.Add(meme.NPCType("Draedon"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("Artemis"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("Apollo"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("ThanatosHead"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("AresBody"));
						name = "Exo Mechs";
						subtitle = "The Fruits of Masterful Craftsmanship";
						save = false;
					}
					if (npc.type == meme.NPCType("GreatSandShark")) {
						subtitle = "The Top King Of Shark Sand";
					}
					if (npc.type == meme.NPCType("DevourerofGodsHead")) {
						subtitle = "The Eater";
					}
					if (npc.type == meme.NPCType("Cryogen")) {
						subtitle = "The Archmage's Prison";
					}
					if (npc.type == meme.NPCType("Polterghast")) {
						subtitle = "The Soul Harvester";
					}
					if (npc.type == meme.NPCType("CeaselessVoid")) {
						subtitle = "The Void";
					}
					if (npc.type == meme.NPCType("CrabulonIdle")) {
						subtitle = "The Mushroom Crab";
					}
					if (npc.type == meme.NPCType("RavagerBody")) {
						subtitle = "The Necromanced Golem";
					}

					if (npc.type == meme.NPCType("SupremeCalamitas")) {
						subtitle = "The Brimstone Witch";
					}
				}
				meme = ModLoader.GetMod("ThoriumMod");
				if (meme != null) {
					if (npc.type == meme.NPCType("Aquaius") || npc.type == meme.NPCType("Omnicide") ||
						npc.type == meme.NPCType("SlagFury")) {
						name = "Primordials";
						subtitle = "The Doom Sayer";
						ObamaCamera.bossEncounter.Add(meme.NPCType("Aquaius"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("Omnicide"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("SlagFury"));
						save = false;
					}
				}
				if (save){ObamaCamera.bossEncounter.Add(npc.type);}
				if (ObamaCamera.titleData != null && ObamaCamera.titleData.Count > 0) {
					foreach (var item in ObamaCamera.titleData)
					{
						if (item.type == npc.type) {
							if (item.text != "") {
								name = item.text;
							}
							subtitle = item.subtext;
							texture = item.texture;
						}
					}
				}
				ObamaCamera.Title(name,subtitle,texture);
			}
			if (MyConfig.get.DeathAnim && npc.type == NPCID.MoonLordCore && npc.ai[0] == 2f && npc.ai[0] > 0f && npc.active) {
				ObamaCamera.Moonlord = true;
			}
			float distance = Vector2.Distance(npc.Center,Main.LocalPlayer.Center);
			if (distance < 1500f && MyConfig.get.WormShake) {
				Tile tile = Framing.GetTileSafely((int)(npc.position.X/16), (int)(npc.position.Y/16));
				int num = MyConfig.get.ShakeInt;
				if (wormShake(npc,ref num) && tile.active() && Main.tileSolid[tile.type]) {
					if (MyConfig.get.ShakeLimit) {
						Bebeq.camerashake += num;
					}
					else {
						Bebeq.camerashake = num;
					}
				}
			}
		}
		bool wormShake(NPC npc,ref int num) {

			if (npc.type == NPCID.TheDestroyer) {num += num/2;return true;}
			if (npc.type == NPCID.EaterofWorldsHead) {return true;}

			var meme = ModLoader.GetMod("CalamityMod");
			if (meme != null) {
				if (npc.type == meme.NPCType("DesertScourgeHead")) {
					num /= 2;
					return true;
				}
				if (npc.type == meme.NPCType("DevourerofGodsHead")) {
					num *= 2;
					return true;
				}
				if (npc.type == meme.NPCType("AquaticScourgeHead")) {
					num += num/2;
					return true;
				}
				if (npc.type == meme.NPCType("AstrumDeusHeadSpectral")) {return true;}
			}
			meme = ModLoader.GetMod("SpiritMod");
			if (meme != null) {
				if (npc.type == meme.NPCType("SteamRaiderHead")) {return true;}
			}
			return false;
		}
	}
	public class Bebeq : ModPlayer
	{
		public MyConfig config => ModContent.GetInstance<MyConfig>();

		static Vector2 screenCache = new Vector2(0,0);
		static Vector2 screenLock = new Vector2(0,0);

		public static int SourcePlayerIndex = -1;
		public static int SourceNPCIndex = -1;
		public static int SourceProjectileIndex = -1;

		public static int pickX;
		public static int pickY;
		public static int pickPower;

		public static int camerashake;

		public override void ResetEffects() {
			pickX = -1;
			pickY = -1;
		}
		public override void OnHitAnything(float x, float y, Entity victim) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.HitShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt/2;}
				else if (camerashake < config.ShakeInt*2) {
					camerashake += config.ShakeInt;
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.KillShake && proj.friendly && !proj.hostile) {
				if (target.life <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitPvp(Item item, Player target, int damage, bool crit) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.KillShake) {
				if (target.statLife <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitPvpWithProj(Projectile proj, Player target, int damage, bool crit) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.KillShake && proj.friendly && !proj.hostile) {
				if (target.statLife <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else {camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.KillShake) {
				if (target.life <= 0) {
					if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
					else{camerashake = config.ShakeInt*2;}
				}	
			}
		}
		public override void PostHurt(bool pvp, bool quiet, double damage, int hitDirection, bool crit) {
			if (player.whoAmI != Main.myPlayer) {return;}
			if (config.HurtShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt;}
				else {camerashake = config.ShakeInt;}
			}
		}
		public override void Kill(double damage, int hitDirection, bool pvp, PlayerDeathReason damageSource) {
			if (player.whoAmI != Main.myPlayer) {return;}
			SourcePlayerIndex = -1;
			SourceProjectileIndex = -1;
			SourceNPCIndex = -1;
			SourcePlayerIndex = damageSource.SourcePlayerIndex;
			SourceNPCIndex = damageSource.SourceNPCIndex;
			SourceProjectileIndex = damageSource.SourceProjectileIndex;
			if (SourcePlayerIndex == -1 && config.spectateNearest) {
				Vector2 pos = player.Center;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player npc = Main.player[i];
					if (npc.active && !npc.dead && i != Main.myPlayer) {
						float between = Vector2.Distance(npc.Center, pos);
						bool closest = Vector2.Distance(pos, targetCenter) > between;
						if (closest || SourcePlayerIndex == -1) {
							SourcePlayerIndex = i;
							targetCenter = npc.Center;
						}
					}
				}
			}
			if (config.HurtShake) {
				if (MyConfig.get.ShakeLimit) {camerashake += config.ShakeInt*2;}
				else{camerashake = config.ShakeInt*2;}
			}
		}
		public override void OnEnterWorld(Player p) {
			if (p.whoAmI != Main.myPlayer) {return;}
			if (config.SmoothCamera) {
				screenCache = p.Center - new Vector2(Main.screenWidth/2,Main.screenHeight/2);
			}
			ObamaCamera.nameMusicTime = 0;
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (ObamaCamera.BossLook.Current) {
				QuickLook = true;
			}
			if (ObamaCamera.LockCamera.JustPressed) {
				IsLockCamera = (!IsLockCamera);
				Color color = (IsLockCamera ? Color.LightGreen : Color.Pink);
				ObamaCamera.DisplayAwoken((IsLockCamera ? "Camera Lock":"Camera Unlock"));
				ObamaCamera.awokenColor = color;
				//CombatText.NewText(player.getRect(),(IsLockCamera ? Color.LightGreen : Color.Pink),(IsLockCamera ? "Camera Lock":"Camera Unlock"));
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
				//ObamaCamera.Title(config.CameraFollow);
				//string hex = "[c/"+Color.White.Hex3()+":";
				ObamaCamera.DisplayAwoken("Mode : "+config.CameraFollow);
				ObamaCamera.awokenColor = Color.White;
				//CombatText.NewText(player.getRect(),Color.White,config.CameraFollow);
			}
		}
		public List<string> biomeEncounter = new List<string>();
		public override TagCompound Save() {
			TagCompound tag = new TagCompound();
			tag.Add("biomeEncounter",biomeEncounter);
			return tag;
		}
		public override void Load(TagCompound tag) {
			biomeEncounter = tag.GetList<string>("biomeEncounter").ToList();
		}
		string Normalize(string names) {
			names = names.Replace("1","");
			names = names.Replace("2","Second");
			names = names.Replace("3","Third");
			names = names.Replace("4","Fourth");
			names = Regex.Replace(names, "([A-Z])", " $1").Trim();
			string[] baba = names.Split('_');
			return baba[baba.Length-1];
		}
		bool saytheline;
		public static string curSubworld = "";
		public override void PostUpdate() {
			if (config.NewBiome && ObamaCamera.titleTimer < 1) {
				string name = "";
				string subtitle = "";
				Color color = Color.White;
				if (player.ZoneJungle && !biomeEncounter.Contains("Jungle")) {
					name = "Jungle";
					subtitle = "The Man Eating Forrest";
					color = Color.Green;
				}
				if (player.ZoneDungeon && !biomeEncounter.Contains("Dungeon")) {
					name = "Dungeon";
					subtitle = "The Cursed Building";
					color = Color.LightBlue;
				}
				if (player.ZoneCorrupt && !biomeEncounter.Contains("Corruption")) {
					name = "Corruption";
					subtitle = "The Corrupted Land";
					color = Color.Purple;
				}
				if (player.ZoneCrimson && !biomeEncounter.Contains("Crimson")) {
					name = "Crimson";
					subtitle = "The Land of Flesh";
					color = Color.Red;
				}
				if (player.ZoneHoly && !biomeEncounter.Contains("Hallow")) {
					name = "Hallow";
					subtitle = "The Land of Magical Being";
					color = Color.Pink;
				}
				if (player.ZoneSnow && !biomeEncounter.Contains("Ice")) {
					name = "Ice";
					subtitle = "The Cold Land";
					color = Color.Blue;
				}
				if (player.ZoneMeteor && !biomeEncounter.Contains("Meteor")) {
					name = "Meteor";
					subtitle = "wtf happen here";
				}
				if (!player.ZoneBeach && !player.ZoneCorrupt&& !player.ZoneCrimson && player.ZoneDesert && !biomeEncounter.Contains("Desert")) {
					name = "Desert";
					subtitle = "The Deserted Land";
					color = Color.Yellow;
				}
				if (player.ZoneGlowshroom && !biomeEncounter.Contains("Glowing Mushroom")) {
					name = "Glowing Mushroom";
					subtitle = "The Home of Mutated Mushroom";
					color = Color.Blue;
				}
				if (player.ZoneBeach && !biomeEncounter.Contains("Beach")) {
					name = "Beach";
					subtitle = "The Edge of Island";
					color = Color.Blue;
				}
				if (player.ZoneUnderworldHeight && !biomeEncounter.Contains("Underworld")) {
					name = "Underworld";
					subtitle = "The Litteral Hell";
					color = Color.Orange;
				}
				if (player.ZoneSkyHeight && !biomeEncounter.Contains("Sky")) {
					name = "Sky";
					subtitle = "The Clouds Land";
					color = Color.LightBlue;
				}
				var meme = ModLoader.GetMod("CalamityMod");
				if (meme != null) {
					if (ClamModCall("crags") && !biomeEncounter.Contains("Brimstone Crag")) {
						name = "Brimstone Crag";
						subtitle = "The Underworld Elemental Land";
						color = Color.Red;
					}
					if (ClamModCall("astral") && !biomeEncounter.Contains("Astral Infection")) {
						name = "Astral Infection";
						subtitle = "The Twisted Dreamscape";
						color = Color.Magenta;
					}
					if (ClamModCall("sunkensea") && !biomeEncounter.Contains("Sunken Sea")) {
						name = "Sunken Sea";
						subtitle = "The Underground Sea";
						color = Color.LightBlue;
					}
					if (ClamModCall("sulphursea") && !biomeEncounter.Contains("Sulphurous Sea")) {
						name = "Sulphurous Sea";
						subtitle = "The Sea Wasteland";
						color = Color.Yellow;
					}
					if (ClamModCall("abyss") && !biomeEncounter.Contains("Abyss")) {
						name = "Abyss";
						subtitle = "The Ocean Depths";
						color = Color.Blue;
					}
				}
				meme = ModLoader.GetMod("ThoriumMod");
				if (meme != null) {
					if ((bool)meme.Call("GetZoneAquaticDepths",player) && !biomeEncounter.Contains("Aquatic Depths")) {
						name = "Aquatic Depths";
						subtitle = "The Underground Ocean";
						color = Color.Cyan;
					}
				}
				meme = ModLoader.GetMod("SubworldLibrary");
				if (meme != null && config.SubworldName) {
					object obj = meme.Call("Current");
					if (obj != null) {
						string sub = (string)obj;
						if (sub == "StarsAbove_RuinedSpaceship" && !biomeEncounter.Contains("Ruined Spaceship")) {
							name = "Ruined Spaceship";
							subtitle = "The Small Ruin Ship";
							color = Color.LightYellow;
						}
						if (sub == "StarsAbove_JungleTower" && !biomeEncounter.Contains("Jungle Tower")) {
							name = "Jungle Tower";
							subtitle = "The Tower of Plants";
							color = Color.LightGreen;
						}
						//ethereal floating island and the home of the Starfarers
						if (sub == "StarsAbove_Observatory" && !biomeEncounter.Contains("Observatory")) {
							name = "Observatory";
							subtitle = "The Ethereal Floating Island";
							color = Color.Purple;
						}
						if (sub == "StarsAbove_SeaOfStars1" || sub == "StarsAbove_SeaOfStars2" && !biomeEncounter.Contains("Sea of Stars")) {
							name = "Sea of Stars";
							subtitle = "The Stars Sea";
							color = Color.Yellow;
						}
						if (sub == "StarsAbove_GalacticMean" && !biomeEncounter.Contains("Galactic Mean")) {
							name = "Galactic Mean";
							subtitle = "The Hubworld";
							color = Color.Purple;
						}
						if (sub == "StarsAbove_SamuraiWar" && !biomeEncounter.Contains("Samurai War")) {
							name = "Samurai War";
							subtitle = "The Samurai Land";
							color = Color.Orange;
						}
						if (curSubworld != sub && player.whoAmI == Main.myPlayer) {
							string text = Normalize(sub);
							if (!text.Contains("The")) {text = "The "+text;}
							ObamaCamera.DisplayAwoken(text);
							ObamaCamera.awokenColor = Color.White;
							curSubworld = sub;
						}
					}
					else {
						curSubworld = "";
					}
				}
				if (name != "") {
					string hex = "[c/"+color.Hex3()+":";
					string text = player.name+$" has Discovered "+hex+name+" Biome]";
					if (Main.netMode == 0){
						Main.NewText(text,Color.LightYellow);
					}
					else if (Main.netMode == 2) {
						NetMessage.BroadcastChatMessage(NetworkText.FromKey(text), Color.LightYellow);
					}
					biomeEncounter.Add(name);
					ObamaCamera.Title(name,subtitle);
					ObamaCamera.titleColor = color;
				}
			}
			if (config.AhShit) {
				if (ObamaCamera.AnyBoss()) {
					if (!saytheline) {
						saytheline = true;
						CombatText.NewText(player.getRect(), Color.White, "Ah shit , here we go again");
						Main.PlaySound(mod.GetLegacySoundSlot(Terraria.ModLoader.SoundType.Custom, "Sounds/repeatablemoment"));
					}
				}else {saytheline = false;}
			}
		}
		bool ClamModCall(string biome) {
			var clam = ModLoader.GetMod("CalamityMod");
			if (clam != null) {
				object a = clam.Call("GetInZone",player,biome);
				if (a != null && a is bool flag) {
					return flag;
				}
			}
			return false;
		}
		public static bool IsLockCamera;
		public static bool QuickLook;
		public void CameraMod() {
			if (!ObamaCamera.Enable || MyConfig.get.DisableMod) {
				return;
			}
			Vector2 centerScreen = new Vector2(Main.screenWidth/2,Main.screenHeight/2);

			if (config.Demolitionist) {
				camerashake = config.ShakeInt*2;
			}
			if (IsLockCamera) {
				Main.screenPosition = screenLock;
				PostCameraUpdate();
				return;
			}
			screenLock = Main.screenPosition;

			bool flag1 = true;
			string type = config.CameraFollow;
			if (ObamaCamera.Moonlord || ObamaCamera.NPCFocus > 0) {
				type = "Boss only";
			}
			if (QuickLook) {
				type = "Boss Only 2";
			}
			ObamaCamera.ILEditReplacement.TypeEdit(ref type);
			if (!player.dead && type != "Player") {
				int index = -1;
				float speed = 0.05f;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxNPCs; i++) {
					if (!ObamaCamera.ILEditReplacement.LoopEdit(ref index)) {
						break;
					}
					NPC npc = Main.npc[i];
					float between = Vector2.Distance(npc.Center, player.Center);
					bool closest = Vector2.Distance(player.Center, targetCenter) > between;
					bool boss = npc.boss || npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.WallofFleshEye;
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
					if (ObamaCamera.NPCFocus > -1) {
						NPC n = Main.npc[ObamaCamera.NPCFocus];
						if (n.active) {
							index = ObamaCamera.NPCFocus;
							targetCenter = n.Center;
							type = "Boss only";
							speed = 0.1f;
							break;
						}
						else {
							ObamaCamera.NPCFocus = -1;
						}
					}
					if ((closest || index == -1) && boss && npc.active && ((type == "Boss Only 2") || Vector2.Distance(npc.Center,player.Center) < 2200f)) {
						index = i;
						targetCenter = npc.Center;
					}
				}
				ObamaCamera.ILEditReplacement.IndexEdit(ref index);
				if (index > -1) {
					NPC npc = Main.npc[index];
					if (type == "Enemy and Player") {type = "Boss and Player";}
					if (type == "Boss Only 2") {type = "Boss only";}
					if (type == "Boss and Player") {
						if (config.SmoothCamera){
							Vector2 pos = Vector2.Lerp(Main.screenPosition,npc.Center + (MyConfig.get.velocityBased ? npc.velocity*2 : Vector2.Zero),0.05f);
							screenCache = Vector2.Lerp(screenCache,pos,config.SmoothCameraInt);
						}
						else {Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center + (MyConfig.get.velocityBased ? npc.velocity*2 : Vector2.Zero),0.05f);}
						flag1 = false;
					}
					else {
						if (config.SmoothCamera){
							screenCache = Vector2.Lerp(screenCache,npc.Center + (MyConfig.get.velocityBased ? npc.velocity*2 : Vector2.Zero) - centerScreen,speed);
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
					if (config.SmoothCamera) {screenCache = Vector2.Lerp(screenCache,targetCenter - centerScreen,config.SmoothCameraInt);}
					else {Main.screenPosition = targetCenter - centerScreen;}
				}
			}
			var list = new List<int>() {ItemID.Binoculars,ItemID.SniperRifle,1254};
			bool hasScope = list.Contains(player.HeldItem.type) || player.scope;
			if (type != "Boss only" && (hasScope || config.OverhaulMouse) && Main.hasFocus && !Main.playerInventory && player.talkNPC < 0) {
				if (config.SmoothCamera) {
					screenCache = Vector2.Lerp(screenCache,Main.MouseWorld - centerScreen,(0.01f*(float)config.OverhaulDistance)*(hasScope && Main.mouseRight ? 2 : 1));
				}
				else {
					Main.screenPosition = Vector2.Lerp(Main.screenPosition,Main.MouseWorld - centerScreen,0.01f*(float)config.OverhaulDistance);
				}
			}
			if (player.dead && (config.DeathCam || config.spectateNearest)) {
				Vector2 pos = player.Center ;
				if (SourceNPCIndex > -1 && config.DeathCam) {
					if (Main.npc[SourceNPCIndex].active) {
						pos = Main.npc[SourceNPCIndex].Center;
					}
					else {SourceNPCIndex = -1;}
				}
				if (SourceProjectileIndex > -1 && config.DeathCam) {
					if (Main.projectile[SourceProjectileIndex].active) {
						pos = Main.projectile[SourceProjectileIndex].Center;
					}
					else {SourceProjectileIndex = -1;}
				}
				if (SourcePlayerIndex > -1) {
					if (Main.player[SourcePlayerIndex].active) {
						pos = Main.player[SourcePlayerIndex].Center;
					}
					else {SourcePlayerIndex = -1;}
				}
				pos -= centerScreen;
				if (config.SmoothCamera) {screenCache = Vector2.Lerp(screenCache,pos,config.SmoothCameraInt);}
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
				if (flag1){
					screenCache = Vector2.Lerp(screenCache,player.Center - centerScreen,config.SmoothCameraInt);
					if (config.velocityBased) {
						screenCache = Vector2.Lerp(screenCache,player.Center + (player.velocity*2f) - centerScreen,config.SmoothCameraInt);	
					}
					if (config.QuickSmooth && Vector2.Distance(screenCache,player.Center - centerScreen) > 1500f) {
						screenCache = player.Center - centerScreen;
					}
				}
			}
			else if (flag1 && config.velocityBased){
				Main.screenPosition += (player.velocity*2f);
			}
			PostCameraUpdate();
		}
		void PostCameraUpdate() {
			if (ObamaCamera.titleTimer < 1 && !ObamaCamera.NPCAlwaysFocus) {
				ObamaCamera.NPCFocus = -1;
				ObamaCamera.ResetTitle();
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
	public struct TitleData
	{
		public int type;
		public string text;
		public string subtext;
		public Texture2D texture;

		public TitleData(int type,string text,string subtext, Texture2D texture) {
			this.type = type;
			this.text = text;
			this.subtext = subtext;
			this.texture = texture;
		}
	}
	// as the title says. its a replacement for IL editing this mod
	public class ILEditReplacement
	{
		public List<Func<int>> indexModifier;
		public List<Func<string>> typeModifier;
		public List<KeyValuePair<Func<int>,Func<bool>>> loopModifier;

		public void Add(Func<int> func) => indexModifier.Add(func);
		public void Add(Func<string> func) => typeModifier.Add(func);
		public void Add(Func<int> func1,Func<bool> func2) => loopModifier.Add(new KeyValuePair<Func<int>,Func<bool>>(func1,func2));

		public void Load() {
			indexModifier = new List<Func<int>>();
			indexModifier.Add((Func<int>)(() => -2));
			typeModifier = new List<Func<string>>();
			typeModifier.Add((Func<string>)(() => "None"));
			loopModifier = new List<KeyValuePair<Func<int>,Func<bool>>>();
			Add((Func<int>)(() => -2),(Func<bool>)(() => true));
		}
		public void IndexEdit(ref int index) {
			if (indexModifier != null && indexModifier.Count > 0) {
				foreach (Func<int> item in indexModifier){
					int b = item();
					if (b != -2) {index = b;}
				}
			}
		}
		public bool LoopEdit(ref int index) {
			if (loopModifier != null && loopModifier.Count > 0) {
				foreach (var item in loopModifier){
					var method = item.Key;
					int b = method();
					if (b != -2) {index = b;}
					var method2 = item.Value;
					if (!method2()) {return false;}
				}
			}
			return true;
		}
		public void TypeEdit(ref string type) {
			if (typeModifier != null && typeModifier.Count > 0) {
				foreach (var item in typeModifier){
					string b = item();
					if (b != "None") {type = b;}
				}
			}
		}
	}
	public class ObamaCamera : Mod
	{
		public struct MusicRegister{
			internal int music;
			internal string name;
			internal string composer;
			public MusicRegister(int music,string name,string composer) {
				this.music = music;
				this.name = name;
				this.composer = composer;
			}
		}
		//boy thats a lot of static fields
		public static ModHotKey BossLook;
		public static ModHotKey SwitchFollow;
		public static ModHotKey LockCamera;
		public static List<MusicRegister> musList = new List<MusicRegister>();
		public static List<TitleData> titleData = new List<TitleData>();
		public static List<int> bossEncounter = new List<int>();
		public static ILEditReplacement ILEditReplacement;
		public static bool Moonlord;
		public static int NPCFocus = -1;
		public static bool NPCAlwaysFocus = false;
		public static bool Enable;

		public override object Call(params object[] args) {
			int argsLength = args.Length;
			Array.Resize(ref args, 5);
			try {
				string call = args[0] as string;
				if (call == "AddBossTexture") {
					int type = Convert.ToInt32(args[1]);
					string texture = args[2] as string;
					ObamaCamera.titleData.Add(new TitleData(
						type,
						"",
						"",
						ModContent.GetTexture(texture))
					);
					Logger.InfoFormat($"{Name} Succesfully added new title #{ObamaCamera.titleData.Count}, Type = {type}, Texture = {texture}");
				}
				else if (call == "AddBossTitle") {
					int type = Convert.ToInt32(args[1]);
					string name = args[2] as string;
					string subtitle = args[3] as string;
					ObamaCamera.titleData.Add(new TitleData(
						type,
						name,
						subtitle,
						null)
					);
					Logger.InfoFormat($"{Name} Succesfully added new title #{ObamaCamera.titleData.Count}, Type = {type}, Name = {name}, Sub = {subtitle}");
				}
				else if (call == "AddShake") {
					int shake = Convert.ToInt32(args[1]);
					Bebeq.camerashake += shake;
				}
				else if (call == "SetShake") {
					int shake = Convert.ToInt32(args[1]);
					Bebeq.camerashake = shake;
				}
				else if (call == "GetCameraStyle") {return MyConfig.get.CameraFollow;}
				else if (call == "SetCameraStyle") {
					string text = args[1] as string;
					MyConfig.get.CameraFollow = text;
				}
				else if (call == "Title") {
					string text = args[1] as string;
					string subtitle = args[2] as string;
					ObamaCamera.Title(text,subtitle);
				}
				else if (call == "TitleColor") {
					string text = args[1] as string;
					string subtitle = args[2] as string;
					Color? color = args[3] as Color?;
					ObamaCamera.Title(text,subtitle);
					titleColor = color ?? Color.White;
				}
				else if (call == "TitleTexture") {
					Texture2D text = args[1] as Texture2D;
					ObamaCamera.Title("","",text);
				}
				else if (call == "Announce") {
					string text = args[1] as string;
					ObamaCamera.DisplayAwoken(text);
				}

				else if (call == "SetNPCFocus") {NPCFocus = Convert.ToInt32(args[1]);}
				else if (call == "GetNPCFocus") {return NPCFocus;}
				else if (call == "AlwaysFocus") {NPCAlwaysFocus = true;}
				else if (call == "NotAlwaysFocus") { NPCAlwaysFocus = false;}
				else if (call == "indexModifier") { ILEditReplacement.Add(args[1] as Func<int>);}
				else if (call == "typeModifier") { ILEditReplacement.Add(args[1] as Func<string>);}
				else if (call == "loopModifier") { ILEditReplacement.Add(args[1] as Func<int>,args[2] as Func<bool>);}

				else if (call == "RegisterMusic") {
					int mus = Convert.ToInt32(args[1]);
					string name = args[2] as string;
					string composer = args[3] as string;
					musList.Add(new MusicRegister(mus,name,composer));
				}
				else if (call == "IsSmoothCamera") {return MyConfig.get.SmoothCamera;}
				else if (call == "IsOverride") {return MyConfig.get.Override;}
				else if (call == "IsEnabled") {return ObamaCamera.Enable;}
				else if (call == "SetDisable") {ObamaCamera.Enable = false;}
				else if (call == "SetEnable") {ObamaCamera.Enable = true;}
				else {Logger.Error($"Unknown mod calls '{call}'");}
			}
			catch (Exception e) {Logger.Error($"Call Error: You are screwed, {e.StackTrace} {e.Message}");}
			return null;
		}
		public static bool AnyBoss(int index = -1) {
			for (int i = 0; i < Main.maxNPCs; i++){
				NPC npc = Main.npc[i];
				bool flag = true;
				if (index > -1) {flag = (i != index);}
				if (flag && npc.active && (npc.boss || npc.type == NPCID.EaterofWorldsHead)) {return true;}
			}
			return false;
		}
		public static string CutOff(string name) {
			string names = name;
			names = names.Replace("Music","");
			names = names.Replace("music","");
			names = names.Replace("Box","");
			names = names.Replace("box","");
			names = names.Replace("Item","");
			names = names.Replace("item","");
			names = names.Replace("1","");
			names = names.Replace("2","Second");
			names = names.Replace("3","Third");
			names = names.Replace("4","Fourth");
			/*
			if (names == "" || names == null) {
				Logger.InfoFormat($"{Name} failed getting name [{names}]");
			}
			*/
			names = names.Replace("_"," ");
			names += "Theme";
			names = Regex.Replace(names, "([A-Z])", " $1").Trim();
			return names;
		}
		void AddTitle(Mod mod,string type,string name,string subtitle) {
			int num = mod.NPCType(type);
			if (num <= 1) {
				Logger.InfoFormat($"{Name} Error loading {type} from {mod.Name}");
				return;
			}
			ObamaCamera.titleData.Add(new TitleData(num,name,subtitle,null));
		}
		public override void PostSetupContent() {
			Logger.InfoFormat($"{Name} Manualy Loading title data");
			var meme = ModLoader.GetMod("ThoriumMod");
			if (meme != null) {
				AddTitle(meme,"Viscount","","The Draculist Bat");
				AddTitle(meme,"FallenDeathBeholder","Coznix","The Fallen Beholder");
				AddTitle(meme,"BoreanStrider","","The Snowy Strider");
				AddTitle(meme,"TheBuriedWarrior","","The Warrior of Greeks");
				AddTitle(meme,"Abyssion","Abyssion","The Forgotten One");
				AddTitle(meme,"Lich","","The Life Taker");
				AddTitle(meme,"QueenJelly","","The Queen Of Evil Jelly");
				AddTitle(meme,"ThePrimeScouter","","The Invader Scouter");
				AddTitle(meme,"TheGrandThunderBirdv2","","The Bird Of Thunder");
				AddTitle(meme,"RealityBreaker","","The Reality Breaker");
			}
			meme = ModLoader.GetMod("Split");
			if (meme != null) {
				AddTitle(meme,"CommandoBoss","","The Invader Commando");
				AddTitle(meme,"Insurgent","","The Troller");
				AddTitle(meme,"Menace","","The Funny Cloud");
				AddTitle(meme,"Mirage","","The Epic Witch");
				AddTitle(meme,"OneShot","","The Aimboter");
				AddTitle(meme,"Paraffin","","The Candle Clown");
				AddTitle(meme,"Seth","","The Knight");
				AddTitle(meme,"TheSpirit","","The Spirited Woman");
			}
			meme = ModLoader.GetMod("StarsAbove");
			if (meme != null) {
				AddTitle(meme,"Arbitration","","The Amalgamation of Order and Chaos");
				AddTitle(meme,"Nalhaun","Nalhaun","The Burnished King");
				AddTitle(meme,"Penthesilea","Penthesilea","The Witch of Ink");
				AddTitle(meme,"Tsukiyomi","Tsukiyomi","The First Starfarer");
				AddTitle(meme,"Tsukiyomi2","Tsukiyomi","The First Starfarer");
				AddTitle(meme,"VagrantOfSpaceAndTime","","The Ruler of Space And Time");
				AddTitle(meme,"WarriorOfLight","","The Everlasting Light");
			}
			Logger.InfoFormat($"{Name} Done !");
		}
		public override void PostAddRecipes() {
		//public override void PostSetupContent() {
			var addedMus = new List<int>();

			musList.Add(new MusicRegister(GetSoundSlot(Terraria.ModLoader.SoundType.Music, "Sounds/Music/SpaceBoyfriend"),"Space Boyfriends Tape","Jami Lynne"));
			addedMus.Add(GetSoundSlot(Terraria.ModLoader.SoundType.Music, "Sounds/Music/SpaceBoyfriend"));

			FieldInfo field = typeof(SoundLoader).GetField("musicToItem", BindingFlags.NonPublic | BindingFlags.Static);
 			IDictionary<int, int> musicToItem = (IDictionary<int, int>)field.GetValue(typeof(SoundLoader));

			Logger.InfoFormat($"{Name} start manually add music from {musicToItem.Count} music boxes");
			foreach (var item in musicToItem)
			{
				//Logger.InfoFormat($"{Name} Registering = [{item.Key}] [{item.Value}]");
				Item i = new Item();
				i.SetDefaults(item.Value);
				string names = i.Name;
				names = names.Replace("Music Box","");
				names = names.Replace("(","");
				names = names.Replace(")","");
				string composer = i.modItem.mod.DisplayName;
				if (i.modItem.mod.Name == "CalamityModMusic") {
					composer = "DM Dokuro";
				}
				if (names == "" || names == null) {
					Logger.InfoFormat($"{Name} failed getting name [{i.type}] [{i.modItem.Name}] , setting up internal name");
					names = CutOff(i.modItem.Name);
				}
				addedMus.Add(item.Key);
				musList.Add(new MusicRegister(item.Key,names,composer));
				//Logger.InfoFormat($"{Name} [{names}] succesfuly registered");
			}
			Logger.InfoFormat($"{Name} done !");

			Logger.InfoFormat($"{Name} start manually add music names from their files");
			foreach (Mod item in ModLoader.Mods){
				FieldInfo field2 = typeof(Mod).GetField("musics", BindingFlags.NonPublic | BindingFlags.Instance);
				var musics = (IDictionary<string, Terraria.ModLoader.Audio.Music>) field2.GetValue(item);
				foreach (var mus in musics){
					int id = item.GetSoundSlot(Terraria.ModLoader.SoundType.Music, mus.Key);
					if (!addedMus.Contains(id)) {
						string[] path = mus.Key.Split('/');
						string names = path[path.Length-1];
						names = CutOff(names);
						string composer = item.DisplayName;
						musList.Add(new MusicRegister(id,names,composer));
						Logger.InfoFormat($"{Name} registering [{mus.Key}] [{id}] [{composer}]");
					}
				}
			}
			Logger.InfoFormat($"{Name} done !");

			Logger.InfoFormat($"{Name} sorting musics");
			musList.Sort((n, t) => n.music > t.music ? 1 : -1);
			Logger.InfoFormat($"{Name} done !");
			/*
			for (int i = 0; i < musicToItem.Count; i++){
				Logger.InfoFormat($"{Name} music {i}");
				if (musicToItem[i] != null) {
					int num = (int)musicToItem[i];
					Item item = new Item();
					item.SetDefaults(num);
					Logger.InfoFormat($"{Name} registering");
					if (item.modItem != null) {
						string name = item.Name;
						name = name.Replace("Box","");
						name = name.Replace("Music","");
						musList.Add(new MusicRegister(i,name,item.modItem.mod.DisplayName));
						Logger.InfoFormat($"{Name} succesfuly registered");
					}
				}
			}
			*/
		}
		public override void Load() {

			Enable = true;
			NPCFocus = -1;
			Moonlord = false;

			//adding dummy values
			musList = new List<MusicRegister>();
			musList.Add(new MusicRegister(-1,"Unknown Music","tModLoader"));
			titleData = new List<TitleData>();
			titleData.Add(new TitleData(-1,"none","none",null));

			ILEditReplacement = new ILEditReplacement();
			ILEditReplacement.Load();

			bossEncounter = new List<int>();
			bossEncounter.Add(-1);

			BossLook = RegisterHotKey("Quick Look At Boss", "V");
			SwitchFollow = RegisterHotKey("Quick Switch Camera", "C");
			LockCamera = RegisterHotKey("Lock Camera", "Y");

			Hacc.Add();
		}
		public override void Unload() {
			ILEditReplacement = null;
			musList = null;
			BossLook = null;
			SwitchFollow = null;
			LockCamera = null;
			bossEncounter = null;
			titleData = null;
			title2D = null;

			Hacc.Remove();
		}
		public static void ResetTitle() {
			titleText = "";
			titleSubText = "";
			title2D = null;
			titleColor = Color.White;
		}
		public static int titleTimer;
		public static Color titleColor;
		static Texture2D title2D;
		static string titleText;
		static string titleSubText;

		static string awoken;
		public static string nameAwoken;
		static int awokenTime;
		public static Color awokenColor;

		static string nameMusic;
		public static int nameMusicTime;
		static int curMus;

		public static void Title(string text, string subtitle = "", Texture2D num = null) {
			titleText = text;
			titleTimer = 240;
			titleSubText = subtitle;
			title2D = num;
			titleColor = Color.White;
		}
		public override void PreSaveAndQuit() {
			nameMusicTime = 0;
			nameMusic = "";
			awokenTime = 0;
			awoken = "";
			titleTimer = 0;
			titleText = "";
			titleSubText = "";
			title2D = null;
			bossEncounter = new List<int>();
		}
		public static void ShowMusic() {
			string text = "Unknown";
			if (curMus < 42) {
				text = ShowMusicVanilla(curMus);
			}
			text += " Theme";
			Mod mod = ModLoader.GetMod("TerrariaOverhaul");
			if (mod != null) {

				// reflection terror :skull:
				// i hate how overhaul has a nested class inside a nested class

				Type OConfigtype = mod.GetType().Assembly.GetType("TerrariaOverhaul.Core.Systems.Config.OConfig");

				Type ClientsideConfig = OConfigtype.GetNestedType("ClientsideConfig",BindingFlags.Public);
				Type AudioConfig = ClientsideConfig.GetNestedType("AudioConfig",BindingFlags.Public);
				Type MusicConfig = AudioConfig.GetNestedType("MusicConfig",BindingFlags.Public);

				Type type = mod.GetType().Assembly.GetType("TerrariaOverhaul.Core.Systems.Config.ConfigSystem");
				FieldInfo field = type.GetField("local", BindingFlags.Public | BindingFlags.Static);
				FieldInfo theoneandtheonly = MusicConfig.GetField("enableMusicRemixes", BindingFlags.Public | BindingFlags.Instance);

				object local = field.GetValue(null);
				object clientside = OConfigtype.GetField("Clientside").GetValue(local);
				object audio = ClientsideConfig.GetField("Audio").GetValue(clientside);
				object music = AudioConfig.GetField("Music").GetValue(audio);

				bool isMusicRemix = (bool)theoneandtheonly.GetValue(music);

				if (isMusicRemix) {
					text += " Remix";
					text += "\nby Kirby Rocket";
				}
				else {
					text += "\nby Scott Lloyd Shelly";
				}
				//TerrariaOverhaul.Core.Systems.Config.ConfigSystem.local.Clientside.Audio.Music.enableMusicRemixes
			}
			else {
				text += "\nby Scott Lloyd Shelly";
			}
			foreach (var music in musList){
				//Main.NewText("cur mus = "+curMus+" / "+music.music);
				if (curMus == music.music) {
					text = music.name+"\n by "+music.composer;
				}
			}
			if (text == "Unknown Theme") {return;}
			nameMusic = text;
			nameMusicTime = 240;
		}
		public static void DisplayAwoken(string text) {
			awoken = text;
			awokenTime = 240;
			awokenColor = new Color(175, 75, 255);
			nameAwoken = "";
		}
		public static string ShowMusicVanilla(int num) {
			if (num == 1) {return "Overworld Day";}
			else if (num == 2) {return "Eerie";}
			else if (num == 3) {return "Night";}
			else if (num == 4) {return "Underground";}
			else if (num == 5) {return "Boss 1";}
			else if (num == 6) {return "Title";}
			else if (num == 7) {return "Jungle";}
			else if (num == 8) {return "Corruption";}
			else if (num == 9) {return "The Hallow";}
			else if (num == 10) {return "Underground Corruption";}
			else if (num == 11) {return "Underground Hallow";}
			else if (num == 12) {return "Boss 2";}
			else if (num == 13) {return "Boss 3";}
			else if (num == 14) {return "Snow";}
			else if (num == 15) {return "Space";}
			else if (num == 16) {return "Crimson";}
			else if (num == 17) {return "Boss 4";}
			else if (num == 18) {return "Alt Overworld Day";}
			else if (num == 19) {return "Rain";}
			else if (num == 20) {return "Ice";}
			else if (num == 21) {return "Desert";}
			else if (num == 22) {return "Ocean";}
			else if (num == 23) {return "Dungeon";}
			else if (num == 24) {return "Plantera";}
			else if (num == 25) {return "Boss 5";}
			else if (num == 26) {return "Temple";}
			else if (num == 27) {return "Eclipse";}
			//else if (num == 28) {return "Rain Sound Effect";}
			else if (num == 29) {return "Mushrooms";}
			else if (num == 30) {return "Pumpkin Moon";}
			else if (num == 31) {return "Alt Underground";}
			else if (num == 32) {return "FrostMoon";}
			else if (num == 33) {return "Underground Crimson";}
			else if (num == 34) {return "The Towers";}
			else if (num == 35) {return "Pirate Invasion";}
			else if (num == 36) {return "Hell";}
			else if (num == 37) {return "Martian Madness";}
			else if (num == 38) {return "Lunar Boss";}
			else if (num == 39) {return "Goblin Invasion";}
			else if (num == 40) {return "Sandstorm";}
			else if (num == 41) {return "Old Ones Army";}
			return "Unknown";
		}
		static int MusicToItemVanilla(int mus) {
			if (mus == 1) {return ItemID.MusicBoxOverworldDay;}
			if (mus == 2) {return ItemID.MusicBoxEerie;}
			if (mus == 3) {return ItemID.MusicBoxNight;}
			if (mus == 4) {return ItemID.MusicBoxUnderground;}
			if (mus == 5) {return ItemID.MusicBoxBoss1;}
			if (mus == 6) {return ItemID.MusicBoxTitle;}
			if (mus == 7) {return ItemID.MusicBoxJungle;}
			if (mus == 8) {return ItemID.MusicBoxCorruption;}
			if (mus == 9) {return ItemID.MusicBoxTheHallow;}
			if (mus == 10) {return ItemID.MusicBoxUndergroundCorruption;}
			if (mus == 11) {return ItemID.MusicBoxUndergroundHallow;}
			if (mus == 12) {return ItemID.MusicBoxBoss2;}
			if (mus == 13) {return ItemID.MusicBoxBoss3;}
			if (mus == 14) {return ItemID.MusicBoxSnow;}
			if (mus == 15) {return ItemID.MusicBoxSpace;}
			if (mus == 16) {return ItemID.MusicBoxCrimson;}
			if (mus == 17) {return ItemID.MusicBoxBoss4;}
			if (mus == 18) {return ItemID.MusicBoxAltOverworldDay;}
			if (mus == 19) {return ItemID.MusicBoxRain;}
			if (mus == 20) {return ItemID.MusicBoxIce;}
			if (mus == 21) {return ItemID.MusicBoxDesert;}
			if (mus == 22) {return ItemID.MusicBoxOcean;}
			if (mus == 23) {return ItemID.MusicBoxDungeon;}
			if (mus == 24) {return ItemID.MusicBoxPlantera;}
			if (mus == 25) {return ItemID.MusicBoxBoss5;}
			if (mus == 26) {return ItemID.MusicBoxTemple;}
			if (mus == 27) {return ItemID.MusicBoxEclipse;}
			//if (mus == 28) {return ItemID.;}
			if (mus == 29) {return ItemID.MusicBoxMushrooms;}
			if (mus == 30) {return ItemID.MusicBoxPumpkinMoon;}
			if (mus == 31) {return ItemID.MusicBoxAltUnderground;}
			if (mus == 32) {return ItemID.MusicBoxFrostMoon;}
			if (mus == 33) {return ItemID.MusicBoxUndergroundCrimson;}
			if (mus == 34) {return ItemID.MusicBoxTowers;}
			if (mus == 35) {return ItemID.MusicBoxPirates;}
			if (mus == 36) {return ItemID.MusicBoxHell;}
			if (mus == 37) {return ItemID.MusicBoxMartians;}
			if (mus == 38) {return ItemID.MusicBoxLunarBoss;}
			if (mus == 39) {return ItemID.MusicBoxGoblins;}
			if (mus == 40) {return ItemID.MusicBoxSandstorm;}
			if (mus == 41) {return ItemID.MusicBoxDD2;}
			return ItemID.MusicBox;
		}
		static int MusicToItem(int mus) {
			if (mus < 42) {
				return MusicToItemVanilla(mus);
			}
			FieldInfo field = typeof(SoundLoader).GetField("musicToItem", BindingFlags.NonPublic | BindingFlags.Static);
 			IDictionary<int, int> musicToItem = (IDictionary<int, int>)field.GetValue(typeof(SoundLoader));
			foreach (var item in musicToItem){
				if (item.Key == mus) {
					return item.Value;
				}
			}
			return ItemID.MusicBox;
		}
		static void DrawAwoken(SpriteBatch spriteBatch) {
			if (awokenTime > 0) {
				awokenTime--;
				float max = 30f;
				float num = awokenTime;
				float alpha = 0f;
				if (num > 210f) {
					alpha = (num - 210f)/30f;
					alpha = 1f - alpha;
				}
				else {
					if (num > max) {num = max;}
					alpha = (num/max);
				}
				if (awoken == "") {return;}
				string[] textList = awoken.Split('\n');
				Color color = awokenColor;
				int hover = 0;
				float offset = 0f;
				if (nameAwoken != "") {
					offset -= 10f;
					color = Color.White;
				}
				for (int i = 0; i < textList.Length; i++){	
					string text = textList[i];
					TextSnippet[] snippets = ChatManager.ParseMessage(text, (color*alpha)).ToArray();
					Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
					Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
					pos = pos.Floor();
					pos.Y += Main.screenHeight/4f;
					pos.Y += Main.screenHeight/8f;
					pos.Y += offset;
					//DrawBorderString(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f, float anchorx = 0f, float anchory = 0f, int maxCharactersDisplayed = -1)
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, messageSize/2f, Vector2.One/2f, out hover);
					offset += messageSize.Y/2f;
				}

				if (nameAwoken != "") {

					TextSnippet[] snippets = ChatManager.ParseMessage(awoken, (color*alpha)).ToArray();
					Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
					Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
					pos = pos.Floor();
					pos.Y += Main.screenHeight/4f;
					pos.Y += Main.screenHeight/8f;
					pos.Y -= messageSize.Y/3f;
					string text = nameAwoken;
					snippets = ChatManager.ParseMessage(text, (awokenColor*alpha)).ToArray();
					messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, messageSize/2f, Vector2.One/2f, out hover);
				}


				/*
				string[] lines = awoken.Split('\n');
				float offset = 0;
				foreach (var text in lines){
					TextSnippet[] snippets = ChatManager.ParseMessage(text, (new Color(175, 75, 255)*alpha)).ToArray();
					Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
					Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
					pos = pos.Floor();
					pos.Y += Main.screenHeight/4f;
					pos.Y += Main.screenHeight/8f;
					pos.Y += offset;
					//DrawBorderString(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f, float anchorx = 0f, float anchory = 0f, int maxCharactersDisplayed = -1)
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, messageSize/2f, Vector2.One/2f, out int hover);
					offset = messageSize.Y;
				}*/
			}
		}
		static void DrawTitle(SpriteBatch spriteBatch) {
			if (titleTimer > 0) {
				titleTimer--;
				float max = 30f;
				float num = titleTimer;
				float alpha = 0f;
				if (num > 210f) {
					alpha = (num - 210f)/30f;
					alpha = 1f - alpha;
				}
				else {
					if (num > max) {num = max;}
					alpha = (num/max);
				}
				if (title2D != null) {
					spriteBatch.Draw(title2D, new Vector2(Main.screenWidth/2,Main.screenHeight/2), null, Color.White*alpha, 0f, title2D.Size() * 0.5f, 1f, SpriteEffects.None, 0f);
					return;
				}
				if (titleText == "") {return;}
				TextSnippet[] snippets = ChatManager.ParseMessage(titleText, (titleColor*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
				float offset = messageSize.Y / 2f;
				pos = pos.Floor();
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, new Vector2(messageSize.X / 2f,messageSize.Y / 2f), new Vector2(1,1), out int hover);

				if (titleSubText != "") {
					snippets = ChatManager.ParseMessage(titleSubText, (Color.White*alpha)).ToArray();
					messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
					pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
					pos.Y += offset + 5f;
					pos = pos.Floor();
					ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, new Vector2(messageSize.X / 2f,messageSize.Y / 2f), new Vector2(0.5f,0.5f), out hover);
				}
			}
		}
		static void DrawMusic(SpriteBatch spriteBatch) {
			if (nameMusicTime > 0) {
				nameMusicTime--;
				float max = 30f;
				float num = nameMusicTime;
				float alpha = 0f;
				if (num > 210f) {
					alpha = (num - 210f)/30f;
					alpha = 1f - alpha;
				}
				else {
					if (num > max) {num = max;}
					alpha = (num/max);
				}
				if (nameMusic == "") {return;}
				List<TextSnippet> snippetList = ChatManager.ParseMessage(nameMusic, (Color.White*alpha));
				string funni = "";
				foreach (TextSnippet i in snippetList){
					/*

					Reflection reduced to atom

					Type itype = i.GetType();
					Type type1 = typeof(Mod).Assembly.GetType("Terraria.GameContent.UI.Chat.ItemTagHandler");
					Type type = type1.GetNestedType("ItemSnippet",BindingFlags.NonPublic | BindingFlags.Public);
					//both of those return null
					if (type == null) {
						Main.NewText("type ded");
						return;
					}
					if (!itype.IsSubclassOf(type)) {
						funni += i.Text;
					}
					*/
					if (!i.TextOriginal.Contains("[i")) {
						funni += i.Text;
					}
					/*
					Vector2 pes = new Vector2(Main.screenWidth/2f,Main.screenHeight/2f);
					pes.Y += a;
					DynamicSpriteFontExtensionMethods.DrawString(spriteBatch, Main.fontMouseText, i.TextOriginal, pes, Color.White);
					//i.Text
					a += 20;
					*/
				}
				TextSnippet[] snippets = ChatManager.ParseMessage(funni, (Color.White*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth,Main.screenHeight);
				pos.Y -= messageSize.Y/4f;
				pos.X -= messageSize.X/4f;
				pos = pos.Floor();
				int mus = MusicToItem(curMus);
				Texture2D texture = Main.itemTexture[mus];
				spriteBatch.Draw(texture, pos - new Vector2(messageSize.X/4f + texture.Width,0) + MyConfig.get.musicNameOffset, null, Color.White*alpha, 0f, texture.Size()/2f, 1f, SpriteEffects.None, 0f);
				//Utils.DrawBorderString(spriteBatch,$"[i:{mus}]",pos - new Vector2(messageSize.X*1.5f,0),Color.White*alpha);
				//pos -= messageSize/4f;
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos + MyConfig.get.musicNameOffset, 0f, messageSize/2f, new Vector2(0.5f,0.5f), out int hover);
			}
		}
		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			if (MyConfig.get.musicNameMove) {

				nameMusic = "Space Boyfriends Tape\nBy Jami Lynne";
				nameMusicTime = 210;

				if (Main.keyState.IsKeyDown(Keys.Right)) {
					MyConfig.get.musicNameOffset.X += 4;
					MyConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Left)) {
					MyConfig.get.musicNameOffset.X -= 4;
					MyConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Up)) {
					MyConfig.get.musicNameOffset.Y -= 4;
					MyConfig.SaveConfig();
				}
				if (Main.keyState.IsKeyDown(Keys.Down)) {
					MyConfig.get.musicNameOffset.Y += 4;
					MyConfig.SaveConfig();
				}
			}
			DrawAwoken(spriteBatch);
			DrawTitle(spriteBatch);
			DrawMusic(spriteBatch);
		}
		public static int MusicPlay = -1;
		public ushort MusicFadeTimerSpecifyForTerrariaOverhaul;
		public override void UpdateUI(GameTime gameTime) {
			//Main.musicFade[curMus] > 0f
			if (MyConfig.get.musicName && !Main.gameMenu) {
				//Main.NewText("fade : "+Main.musicFade[Main.curMusic]+" new mus : "+Main.instance.newMusic+" musicmode : "+curMus+" curMus :"+Main.curMusic);

				//terraria overhaul frick up my method :bruh:
				Mod mod = ModLoader.GetMod("TerrariaOverhaul");
				if (MyConfig.get.musicNameTimer || mod != null) {
					if (curMus != Main.curMusic && Main.curMusic > 0) {
						MusicFadeTimerSpecifyForTerrariaOverhaul++;
						if (MusicFadeTimerSpecifyForTerrariaOverhaul >= 60*3) {
							curMus = Main.curMusic;
							ShowMusic();
							MusicFadeTimerSpecifyForTerrariaOverhaul = 0;
						}
					}	
				}
				else {
					if (curMus != Main.curMusic && Main.curMusic > 0 && Main.musicFade[Main.curMusic] >= 1f) {
						curMus = Main.curMusic;
						ShowMusic();
					}
				}
			}
			/*
			Mod meme = ModLoader.GetMod("CalamityMod");
			if (meme != null && MyConfig.get.stealthCenter) {
				// this one aint that bad. unlike terraria overhaul that nested their class and make me suffer for over 6 hours
				Type type = meme.GetType().Assembly.GetType("CalamityMod.UI.StealthUI");
				if (type == null) {return;}
				FieldInfo field = type.GetField("Offset", BindingFlags.Public | BindingFlags.Static);
				if (field == null) {return;}
				Vector2 pos = (Vector2)field.GetValue(null);
				pos = Main.LocalPlayer.Center  + new Vector2(0,15 + Main.LocalPlayer.height) - Main.screenPosition;
			}
			*/
		}
		public override void UpdateMusic(ref int music, ref MusicPriority priority) {
			if (Main.gameMenu) {return;}
			if (ObamaCamera.MusicPlay > 0) {
				music = ObamaCamera.MusicPlay;
				priority = MusicPriority.BossHigh + 3;
			}
			if (MyConfig.get.musicNameMove) {
				music = GetSoundSlot(Terraria.ModLoader.SoundType.Music, "Sounds/Music/SpaceBoyfriend");
				priority = MusicPriority.BossHigh + 4;
			}
		}
		public override void Close() {
			if (MyConfig.get.musicReload && musList != null && musList.Count > 0) {
				foreach (var item in musList){
					int TitleMusic = item.music;
					if (Main.music.IndexInRange(TitleMusic) && (Main.music[TitleMusic]?.IsPlaying ?? false)){
						Main.music[TitleMusic].Stop(AudioStopOptions.Immediate);
					}
				}
			}
			base.Close();
		}
		//ObamaCamera.MusicPlay

		// net code moment
		//public override void HandlePacket(BinaryReader reader, int whoAmI) {}
	}
	[Label("Obama Camera")]
	public class MyConfig : ModConfig
	{
		public static void SaveConfig(){
			typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, new object[1] { get });
		}

		public override ConfigScope Mode => ConfigScope.ClientSide;
		// public override ConfigScope Mode => ConfigScope.ServerSide;
		public static MyConfig get => ModContent.GetInstance<MyConfig>();

		[Header("Camera")]

		[Label("Smooth camera")]
		[Tooltip("make the camera smooth")]
		[DefaultValue(true)]
		public bool SmoothCamera;

		[Label("Smooth camera intensity")]
		[Tooltip("The intensity of smooth camera\n [default is 0.1]")]
		[Range(0.01f, 0.4f)]
		[Increment(0.05f)]
		[DefaultValue(0.1f)]
		[DrawTicks]
		[Slider] 
		public float SmoothCameraInt;

		[Label("Spectate nearest player")]
		[Tooltip("Spectate nearest player when you died, this will be more prioritized than Death Cam")]
		[DefaultValue(false)]
		public bool spectateNearest;

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

		[Label("Camera Boss Intro")]
		[Tooltip("Make the camera lock on bosses when they first spawned \nand display the name of the boss")]
		[DefaultValue(true)]
		public bool BossIntro;

		[Label("Camera Boss Intro Multiple")]
		[Tooltip("Make camera boss applies multiple times")]
		[DefaultValue(true)]
		public bool BossIntroMult;

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

		[Label("Quick Smooth Camera")]
		[Tooltip("Immediately set the camera positon to player center when its too far away\n[enabled by default]")]
		[DefaultValue(true)]
		public bool QuickSmooth;

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

		[Label("Screen Shake On Roar and Explode Distance")]
		[Tooltip("The maximal distance between player and sound to screenshake\n [default is 700]")]
		[Range(100, 1000)]
		[Increment(10)]
		[DefaultValue(700)]
		[Slider] 
		public int RoarShakeInt;

		[Label("Screen Shake On Worm Boss")]
		[Tooltip("Shake the screen whenever a worm boss is digging")]
		[DefaultValue(false)]
		public bool WormShake;

		[Label("Screen Shake On Failed breaking tile")]
		[Tooltip("Shake the screen whenever the player failed at breaking tiles\nthis also display the required pick power")]
		[DefaultValue(true)]
		public bool TileShake;

		[Label("Screen Shake Limit Breaker")]
		[Tooltip("Allow the screen shake effect be stacked (Chaos)")]
		[DefaultValue(false)]
		public bool ShakeLimit;

		[Label("Screen Shake Intensity")]
		[Tooltip("the intensity of the screenshake \n [default is 4]")]
		[Range(0, 20)]
		[Increment(1)]
		[DefaultValue(4)]
		[Slider] 
		public int ShakeInt;

		[Header("Display Music")]

		[Label("Display music name")]
		[Tooltip("Display the name of currently played song in the right corner of the screen\n[enabled by default]")]
		[DefaultValue(true)]
		public bool musicName;

		[Label("Display music name custom timer")]
		[Tooltip("uses custom timer when displaying music name\n[enabled if terraria overhaul is active]")]
		[DefaultValue(false)]
		public bool musicNameTimer;

		[Label("Display music name move")]
		[Tooltip("Use arrows key to move the bar")]
		[DefaultValue(false)]
		public bool musicNameMove;

		[Label("Display music name offset position")]
		[Tooltip("The offset of Display music name ui")]
		public Vector2 musicNameOffset = Vector2.Zero;

		[Header("Experimental and Funny Stuff")]

		[Label("Multiple sound method check")]
		[Tooltip("Uses multiple sound check for more support to other mods\n[enabled by default]")]
		[DefaultValue(true)]
		public bool multipleSoundCheck;

		[Label("Display boss awoken")]
		[Tooltip("Display a text at the bottom of the screen if boss awokened instead of in the chat")]
		[DefaultValue(false)]
		public bool awokenDisplay;

		[Label("Display Subworld Name")]
		[Tooltip("Display a subworld name upon entering one\nfor subworldlib mod")]
		[DefaultValue(true)]
		public bool SubworldName;
		
		[Label("Biome Title")]
		[Tooltip("Display biome name and the description when discovering a new biome\nsupports calamity biome !")]
		[DefaultValue(true)]
		public bool NewBiome;

		[Label("Better modded boss dialog")]
		[Tooltip("Are you tired whenever a boss constantly spamming messages in chat that you cant read ?\nthen this config for you !\nthis will make those messages display better")]
		[DefaultValue(true)]
		public bool betterDialog;

		/*
		[Label("Biome Title Reset")]
		[Tooltip("Reset Local Player Discovered Biome \nChange this config to reset")]
		[DefaultValue(false)]
		public bool ResetBiome;
		*/

		[Label("Text to Combat Text")]
		[Tooltip("Turn Main text to combat text")]
		[DefaultValue(false)]
		public bool TextToCombatText;

		[Label("Screen Shake Forever")]
		[Tooltip("Make the screen shaked forever, funni")]
		[DefaultValue(false)]
		public bool Demolitionist;

		[Label("Camera follow nearest minion")]
		[Tooltip("Make the camera follow nearest player minion")]
		[DefaultValue(false)]
		public bool Demon;

		[Label("Ah Sh*t, here we go again")]
		[Tooltip("No Description")]
		[DefaultValue(false)]
		public bool AhShit;

		[Label("Music Reload Fix")]
		[Tooltip("Fixes some issue with music unloading\nSometimes doesnt work due to funny code")]
		[DefaultValue(false)]
		public bool musicReload;

		[Label("Simulate what most Linux users see")]
		[Tooltip("yes")]
		[DefaultValue(false)]
		public bool DemonBanner;

		[Label("Velocity based camera")]
		[Tooltip("Player camera also affected by player velocity")]
		[DefaultValue(false)]
		public bool velocityBased;

		[Label("Disable camera related feature")]
		[Tooltip("disable every camera related feature\nfor people that only like other feature in this mod")]
		[DefaultValue(false)]
		public bool DisableMod;

		[Label("Password")]
		[Tooltip("put something funny in here, idk")]
		[DefaultValue("Sus")]
		public string Password;

		public override void OnChanged() {
			if (Password == "BiomeEncounter" && !Main.gameMenu) {
				Main.NewText("Biome encounter reseted");
				Main.LocalPlayer.GetModPlayer<Bebeq>().biomeEncounter = new List<string>();
				Password = "Done";
			}
			if (Password == "BossIntro" && !Main.gameMenu) {
				Main.NewText("Boss intro reseted");
				ObamaCamera.bossEncounter = new List<int>();
				Password = "Done";
			}
			if (Password == "AmongAss" && !Main.gameMenu) {
				for (int i = 0; i < 25; i++){
					Main.NewText("Among Ass");
				}
				Password = "SUS";
			}
			if (Password == "Test" && !Main.gameMenu) {
				
				string fart = "you look very sussy my guy... did you have some lean on your back ?!?";

				// prevent things like i farted\n\n\n\n\n\n hello sorry yes
				char prev = '/';
				string build = "";
				int be = 0;
				foreach (char po in fart)
				{
					bool aba = true;
					if ((be + 1) < fart.Length) {
						if (fart[be+1] == '.') {aba = false;}
					}
					if (prev == '.' && po != '.' && aba) {build += "\n";}
					else {build += po;}

					if (po == '.' && prev != '.' && aba) {build += "\n";}

					prev = po;
					be++;
				}

				ObamaCamera.DisplayAwoken(build);
				ObamaCamera.awokenColor = Color.Red;
				ObamaCamera.nameAwoken = "Supreme Calamitas";
			}
		}

	}
	//int.TryParse(text, out var result)
	public class ObamaUndiscover : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "undiscoverall";

		public override string Usage
			=> "/undiscoverall";

		public override string Description
			=> "undiscover all already discovered biome";

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Player.GetModPlayer<Bebeq>().biomeEncounter = new List<string>();
			caller.Reply("succesfully undiscovered");
		}
	}
	public class ObamaMusicPlay : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "MusicPlay";

		public override string Usage
			=> "/MusicPlay <id>";

		public override string Description
			=> "play a music , set to -1 or 0 to disable";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (args.Length == 0) {
				caller.Reply($"Currently played music {ObamaCamera.MusicPlay}");
				return;
			}
			if (int.TryParse(args[0], out var result)) {
				if (result < 1) {
					ObamaCamera.MusicPlay = -1;
					return;
				}
				if (result > ObamaCamera.musList[ObamaCamera.musList.Count - 1].music) {
					caller.Reply($"ID out of bounds [ {result} ]",Color.Red);
					return;
				}
				ObamaCamera.MusicPlay = result;
				caller.Reply($"Played music {result}");
			}
			else {
				caller.Reply($"Currently played music {ObamaCamera.MusicPlay}");
			}
		}
	}
	public class ObamaMusicList : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "MusicList";

		public override string Usage
			=> "/MusicList";

		public override string Description
			=> "Display list of musics";

		public override void Action(CommandCaller caller, string input, string[] args) {
			caller.Reply("======= List of Music =========");
			for (int i = 1; i < 42; i++)
			{
				string text = ObamaCamera.ShowMusicVanilla(i);
				caller.Reply($"[ {i} ] {text} by Scott Lloyd Shelly");
			}
			foreach (var item in ObamaCamera.musList){
				caller.Reply($"[ {item.music} ] {item.name} by {item.composer}");
			}
			caller.Reply("===============================");
		}
	}
	public class ObamaTitle : ModCommand
	{
		public override CommandType Type
			=> CommandType.Chat;

		public override string Command
			=> "Title";

		public override string Usage
			=> "/Title title subtitle";

		public override string Description
			=> "Display a title";

		public override void Action(CommandCaller caller, string input, string[] args) {
			if (args.Length == 0) {
				Main.NewText("command false, require 2 argument '/Title title subtitle'");
				return;
			}
			if (args[0] == null || args[0] == "") {
				Main.NewText("command false");
				return;
			}
			if (args.Length == 1) {ObamaCamera.Title(args[0],"");}
			else {ObamaCamera.Title(args[0],args[1]);}
		}
	}
	public static class Hacc
	{
		public static void Add() {
			On.Terraria.Main.PlaySound_int_Vector2_int += OnPlaySound;
			On.Terraria.Main.PlaySound_LegacySoundStyle_Vector2 += OnPlaySound2;
			On.Terraria.Main.PlaySound_LegacySoundStyle_int_int += OnPlaySound3;
			On.Terraria.Main.PlaySound_int_int_int_int_float_float += OnPlaySound4;
			On.Terraria.Player.PickTile += TilePatch;
			On_ModifyScreenPosition += ScreenPatch;
			On.Terraria.Main.NewText_string_byte_byte_byte_bool += NewTextPatch;
			On.Terraria.NPC.SpawnOnPlayer += SpawnOnPlayerPatch;
			On_PostAI += PostAIPatch;
			On_PreAI += PreAIPatch;
		}
		public static void Remove() {
			On.Terraria.Main.PlaySound_int_Vector2_int -= OnPlaySound;
			On.Terraria.Main.PlaySound_LegacySoundStyle_Vector2 -= OnPlaySound2;
			On.Terraria.Main.PlaySound_LegacySoundStyle_int_int -= OnPlaySound3;
			On.Terraria.Main.PlaySound_int_int_int_int_float_float -= OnPlaySound4;
			On.Terraria.Player.PickTile -= TilePatch;
			On_ModifyScreenPosition -= ScreenPatch;
			On.Terraria.Main.NewText_string_byte_byte_byte_bool -= NewTextPatch;
			On.Terraria.NPC.SpawnOnPlayer += SpawnOnPlayerPatch;
			On_PostAI -= PostAIPatch;
			On_PreAI -= PreAIPatch;
		}
		// sound list
		// Item_14
		// Item_62
		// Zombie_20
		// Zombie_104
		// Zombie_92
		static void ScreenPatch(orig_ModifyScreenPosition orig, Player player) {
			if (!MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
			orig(player);
			if (MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
		}
		//sound
		static void SoundShake(int type, int Style, float volumeScale = 0f) {
			int num = 0;
			if ((type == SoundID.Roar && (Style == 0 || Style == 2)) || type == SoundID.ForceRoar) {
				num = (int)((float)MyConfig.get.ShakeInt*(2f + volumeScale));
			}
			if (type == SoundID.Zombie && (Style == 20 || Style == 104 || Style == 92)) {
				num = MyConfig.get.ShakeInt*2;
			}
			if (type == SoundID.Item && (Style == 14 || Style == 62)){
				num = MyConfig.get.ShakeInt*2;
			}
			if (num > 0) {
				if (MyConfig.get.ShakeLimit) {
					Bebeq.camerashake += num;
				}
				else {
					Bebeq.camerashake = num;
				}
			}
		}
		private static void OnPlaySound(On.Terraria.Main.orig_PlaySound_int_Vector2_int orig,int type, Vector2 position, int Style){
			if (MyConfig.get.RoarShake && !Main.gameMenu && MyConfig.get.multipleSoundCheck) {
				if (Vector2.Distance(position,Main.LocalPlayer.Center) < MyConfig.get.RoarShakeInt) {
					SoundShake(type,Style);
				}
			}
			orig(type,position,Style);
		}
		private static SoundEffectInstance OnPlaySound2(On.Terraria.Main.orig_PlaySound_LegacySoundStyle_Vector2 orig,LegacySoundStyle type, Vector2 position){
			if (MyConfig.get.RoarShake && !Main.gameMenu && MyConfig.get.multipleSoundCheck) {
				if (Vector2.Distance(position,Main.LocalPlayer.Center) < MyConfig.get.RoarShakeInt) {
					SoundShake(type.SoundId,type.Style);
				}
			}
			return orig(type,position);
		}
		private static SoundEffectInstance OnPlaySound3(On.Terraria.Main.orig_PlaySound_LegacySoundStyle_int_int orig,LegacySoundStyle type, int x, int y){
			if (MyConfig.get.RoarShake && !Main.gameMenu && MyConfig.get.multipleSoundCheck) {
				if (Vector2.Distance(new Vector2(x,y),Main.LocalPlayer.Center) < MyConfig.get.RoarShakeInt) {
					SoundShake(type.SoundId,type.Style);
				}
			}
			return orig(type,x,y);
		}
		private static SoundEffectInstance OnPlaySound4(On.Terraria.Main.orig_PlaySound_int_int_int_int_float_float orig,int type, int x, int y, int Style, float volumeScale, float pitchOffset){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(new Vector2(x,y),Main.LocalPlayer.Center) < MyConfig.get.RoarShakeInt) {
					SoundShake(type,Style,volumeScale);
				}
			}
			return orig(type, x, y, Style,  volumeScale, pitchOffset);
		}
		static void NewTextPatch(On.Terraria.Main.orig_NewText_string_byte_byte_byte_bool orig, string newText, byte R, byte G, byte B, bool force) {
			if (MyConfig.get.TextToCombatText) {
				CombatText.NewText(Main.LocalPlayer.getRect(),new Color(R,G,B),newText);
			}
			if (MyConfig.get.betterDialog && enemyAiRunned != -1) {

				NPC npc = Main.npc[enemyAiRunned];
				string fart = newText;
				char prev = '/';
				string build = "";
				int be = 0;

				// a pretty complicated system that prevents thing like "...." to be deleted
				foreach (char po in fart){
					bool aba = true;
					if ((be + 1) < fart.Length) {if (fart[be+1] == '.') {aba = false;}}
					if (prev == '.' && po != '.' && aba) {build += "\n";}
					else {build += po;}
					if (po == '.' && prev != '.' && aba) {build += "\n";}
					prev = po;
					be++;
				}
				ObamaCamera.DisplayAwoken(build);
				ObamaCamera.nameAwoken = npc.FullName;
				ObamaCamera.awokenColor = new Color(R,G,B);
				return;
			}
			if (PreventNewText) {
				PreventNewText = false;
				return;
			}
			orig(newText, R, G, B, force);
		}
		public static bool PreventNewText = false;
		static void SpawnOnPlayerPatch(On.Terraria.NPC.orig_SpawnOnPlayer orig,int plr, int type) {
			if (MyConfig.get.awokenDisplay) {
				PreventNewText = true;
			}
			orig(plr,type);
			if (MyConfig.get.awokenDisplay) {
				PreventNewText = true;
				NPC npc = new NPC();
				npc.SetDefaults(type);
				ObamaCamera.DisplayAwoken(Language.GetTextValue("Announcement.HasAwoken", npc.TypeName));
			}
		}
		static void TilePatch(On.Terraria.Player.orig_PickTile orig,Player self,int x, int y, int pickPower)
		{
			if (self.whoAmI == Main.myPlayer) {
				Bebeq.pickX = x;
				Bebeq.pickY = y;
				Bebeq.pickPower = pickPower;
			}
			orig(self,x,y,pickPower);
		}
		static int enemyAiRunned = -1;
		static bool PreAIPatch(orig_PreAI orig, NPC npc){
			enemyAiRunned = npc.whoAmI;
			return orig(npc);
		}

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

		public delegate bool orig_PreAI(NPC npc);
		public delegate bool Hook_PreAI(orig_PreAI orig, NPC npc);
		public static event Hook_PreAI On_PreAI {
			add => HookEndpointManager.Add<Hook_ModifyScreenPosition>(GetModMethod("NPCLoader","PreAI"), value);
			remove => HookEndpointManager.Remove<Hook_ModifyScreenPosition>(GetModMethod("NPCLoader","PreAI"), value);
		}

		static void PostAIPatch(orig_PostAI orig, NPC npc){
			orig(npc);
			enemyAiRunned = -1;
		}

		public delegate void orig_PostAI(NPC npc);
		public delegate void Hook_PostAI(orig_PostAI orig, NPC npc);
		public static event Hook_PostAI On_PostAI {
			add => HookEndpointManager.Add<Hook_ModifyScreenPosition>(GetModMethod("NPCLoader","PostAI"), value);
			remove => HookEndpointManager.Remove<Hook_ModifyScreenPosition>(GetModMethod("NPCLoader","PostAI"), value);
		}

		public static MethodBase GetModMethod(string loader,string method) {
			return typeof(Mod).Assembly.GetType("Terraria.ModLoader."+loader).GetMethod(method, BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
		}

	}
}