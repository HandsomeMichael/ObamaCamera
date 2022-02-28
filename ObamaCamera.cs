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
			Player player = Main.LocalPlayer;
			var p = player.GetModPlayer<Bebeq>();
			if (MyConfig.get.TileShake && p.pickX == i && p.pickY == j && fail) {
				ModTile mt = TileLoader.GetTile(type);
				string text = "";
				if (mt != null && p.pickPower > 0 && p.pickPower < mt.minPick)
					text = $"Require {mt.minPick}% Mining Power";
				else if ((PowerID.ContainsKey(type) || Main.tileDungeon[type]) && p.pickPower > 0) {
					int power = 0;
					if (Main.tileDungeon[type]) {
						power = 65;
					}
					else {power = PowerID[type];}
					if ((type == 22 || type == 204) && j < Main.worldSurface ){power = 0;}

					if (Main.tileDungeon[type] && p.pickPower < 65)
					{
						bool a = (double)i < (double)Main.maxTilesX * 0.35;
						bool b = (double)i > (double)Main.maxTilesX * 0.65;
						if (!a && !b){power = 0;}
					}
					if (p.pickPower < power) {
						text = $"Require {power}% Mining Power";
					}
				}

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
		public override bool InstancePerEntity => true;
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
			if (MyConfig.get.BossIntro && npc.realLife < 0 &&
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
				meme = ModLoader.GetMod("Split");
				if (meme != null) {
					if (npc.type == meme.NPCType("CommandoBoss")) {
						subtitle = "The Invader Commando";
					}
					if (npc.type == meme.NPCType("Insurgent")) {
						subtitle = "The Troller";
					}
					if (npc.type == meme.NPCType("Menace")) {
						subtitle = "The Funny Cloud";
					}
					if (npc.type == meme.NPCType("Mirage")) {
						subtitle = "The Epic Witch";
					}
					if (npc.type == meme.NPCType("OneShot")) {
						subtitle = "The Aimboter";
					}
					if (npc.type == meme.NPCType("Paraffin")) {
						subtitle = "The Candle Clown";
					}
					if (npc.type == meme.NPCType("Seth")) {
						subtitle = "The Knight";
					}
					if (npc.type == meme.NPCType("TheSpirit")) {
						subtitle = "The Spirited Woman (flushed)";
					}
				}
				meme = ModLoader.GetMod("ThoriumMod");
				if (meme != null) {
					if (npc.type == meme.NPCType("Viscount")) {
						subtitle = "The Draculist Bat";
					}
					if (npc.type == meme.NPCType("FallenDeathBeholder")) {
						name = "Coznix";
						subtitle = "The Fallen Beholder";
					}
					if (npc.type == meme.NPCType("BoreanStrider")) {
						subtitle = "The Snowy Strider";
					}
					if (npc.type == meme.NPCType("TheBuriedWarrior")) {
						subtitle = "The Warrior of Greeks";
					}
					if (npc.type == meme.NPCType("Abyssion")) {
						name = "Abyssion";
						subtitle = "The Forgotten One";
					}
					if (npc.type == meme.NPCType("Lich")) {
						subtitle = "The Life Taker";
					}
					if (npc.type == meme.NPCType("QueenJelly")) {
						subtitle = "The Queen Of Evil Jelly";
					}
					if (npc.type == meme.NPCType("ThePrimeScouter")) {
						subtitle = "The Invader Scouter";
					}
					if (npc.type == meme.NPCType("TheGrandThunderBirdv2")) {
						subtitle = "The Bird Of Thunder";
					}
					if (npc.type == meme.NPCType("Lich")) {
						subtitle = "The Draculist Bat";
					}
					if (npc.type == meme.NPCType("Aquaius") || npc.type == meme.NPCType("Omnicide") ||
						npc.type == meme.NPCType("SlagFury")) {
						name = "Primordials";
						subtitle = "The Doom Sayer";
						ObamaCamera.bossEncounter.Add(meme.NPCType("Aquaius"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("Omnicide"));
						ObamaCamera.bossEncounter.Add(meme.NPCType("SlagFury"));
						save = false;
					}
					if (npc.type == meme.NPCType("RealityBreaker")) {
						subtitle = "The Reality Breaker";
					}
				}
				if (save){ObamaCamera.bossEncounter.Add(npc.type);}
				if (ObamaCamera.titleData != null && ObamaCamera.titleData.Count > 0) {
					foreach (var item in ObamaCamera.titleData)
					{
						if (item != null && item.type == npc.type) {
							name = item.text;
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
						Main.player[npc.target].GetModPlayer<Bebeq>().camerashake += num;
					}
					else {
						Main.player[npc.target].GetModPlayer<Bebeq>().camerashake = num;
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

		Vector2 screenCache = new Vector2(0,0);
		Vector2 screenLock = new Vector2(0,0);

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
					camerashake += config.ShakeInt;
				}
			}
		}
		public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit) {
			if (config.KillShake && proj.friendly && !proj.hostile) {
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
			if (config.KillShake && proj.friendly && !proj.hostile) {
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
			SourcePlayerIndex = -1;
			SourceProjectileIndex = -1;
			SourceNPCIndex = -1;
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
			ObamaCamera.nameMusicTime = 0;
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (ObamaCamera.BossLook.Current) {
				QuickLook = true;
			}
			if (ObamaCamera.LockCamera.JustPressed) {
				IsLockCamera = (!IsLockCamera);
				Color color = (IsLockCamera ? Color.LightGreen : Color.Pink);
				string hex = "[c/"+color.Hex3()+":";
				ObamaCamera.DisplayAwoken(hex+(IsLockCamera ? "Camera Lock]":"Camera Unlock]"));
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
				string hex = "[c/"+Color.White.Hex3()+":";
				ObamaCamera.DisplayAwoken(hex+"Mode : "+config.CameraFollow+"]");
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
		bool saytheline;
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
		public bool IsLockCamera;
		public bool QuickLook;
		public void CameraMod() {
			if (!ObamaCamera.Enable) {
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
			if (!player.dead && type != "Player") {
				int index = -1;
				float speed = 0.05f;
				Vector2 targetCenter = player.Center;
				for (int i = 0; i < Main.maxNPCs; i++) {
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
				if (index > -1) {
					NPC npc = Main.npc[index];
					if (type == "Enemy and Player") {type = "Boss and Player";}
					if (type == "Boss Only 2") {type = "Boss only";}
					if (type == "Boss and Player") {
						if (config.SmoothCamera){
							Vector2 pos = Vector2.Lerp(Main.screenPosition,npc.Center,0.05f);
							screenCache = Vector2.Lerp(screenCache,pos,config.SmoothCameraInt);
						}
						else {Main.screenPosition = Vector2.Lerp(Main.screenPosition,npc.Center,0.05f);}
						flag1 = false;
					}
					else {
						if (config.SmoothCamera){
							screenCache = Vector2.Lerp(screenCache,npc.Center - centerScreen,speed);
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
			if (type != "Boss only" && config.OverhaulMouse && Main.hasFocus && !Main.playerInventory && player.talkNPC < 0) {
				if (config.SmoothCamera ) {
					screenCache = Vector2.Lerp(screenCache,Main.MouseWorld - centerScreen,0.01f*(float)config.OverhaulDistance);
				}
				else {
					Main.screenPosition = Vector2.Lerp(Main.screenPosition,Main.MouseWorld - centerScreen,0.01f*(float)config.OverhaulDistance);
				}
			}
			if (player.dead) {
				Vector2 pos = player.Center ;
				if (SourcePlayerIndex > -1) {
					if (Main.player[SourcePlayerIndex].active) {
						pos = Main.player[SourcePlayerIndex].Center;
					}
					else {SourcePlayerIndex = -1;}
				}
				if (SourceNPCIndex > -1) {
					if (Main.npc[SourceNPCIndex].active) {
						pos = Main.npc[SourceNPCIndex].Center;
					}
					else {SourceNPCIndex = -1;}
				}
				if (SourceProjectileIndex > -1) {
					if (Main.projectile[SourceProjectileIndex].active) {
						pos = Main.projectile[SourceProjectileIndex].Center;
					}
					else {SourceProjectileIndex = -1;}
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
					if (config.QuickSmooth && Vector2.Distance(screenCache,player.Center - centerScreen) > 1500f) {
						screenCache = player.Center - centerScreen;
					}
				}
			}
			PostCameraUpdate();
		}
		void PostCameraUpdate() {
			if (ObamaCamera.titleTimer < 1) {
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
	public class TitleData
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
		public static bool Moonlord;
		public static int NPCFocus = -1;
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
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake += shake;
				}
				else if (call == "SetShake") {
					int shake = Convert.ToInt32(args[1]);
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake = shake;
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
		string CutOff(string name) {
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
		public override void PostAddRecipes() {
		//public override void PostSetupContent() {

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
				musList.Add(new MusicRegister(item.Key,names,composer));
				//Logger.InfoFormat($"{Name} [{names}] succesfuly registered");
			}
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

			bossEncounter = new List<int>();
			bossEncounter.Add(-1);

			BossLook = RegisterHotKey("Quick Look At Boss", "V");
			SwitchFollow = RegisterHotKey("Quick Switch Camera", "C");
			LockCamera = RegisterHotKey("Lock Camera", "Y");

			Hacc.Add();
		}
		public override void Unload() {
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
		static int awokenTime;

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
			// idk i never liked switch case
			if (curMus == 1) {text = "Overworld Day";}
			else if (curMus == 2) {text = "Eerie";}
			else if (curMus == 3) {text = "Night";}
			else if (curMus == 4) {text = "Underground";}
			else if (curMus == 5) {text = "Boss 1";}
			else if (curMus == 6) {text = "Title";}
			else if (curMus == 7) {text = "Jungle";}
			else if (curMus == 8) {text = "Corruption";}
			else if (curMus == 9) {text = "The Hallow";}
			else if (curMus == 10) {text = "Underground Corruption";}
			else if (curMus == 11) {text = "Underground Hallow";}
			else if (curMus == 12) {text = "Boss 2";}
			else if (curMus == 13) {text = "Boss 3";}
			else if (curMus == 14) {text = "Snow";}
			else if (curMus == 15) {text = "Space";}
			else if (curMus == 16) {text = "Crimson";}
			else if (curMus == 17) {text = "Boss 4";}
			else if (curMus == 18) {text = "Alt Overworld Day";}
			else if (curMus == 19) {text = "Rain";}
			else if (curMus == 20) {text = "Ice";}
			else if (curMus == 21) {text = "Desert";}
			else if (curMus == 22) {text = "Ocean";}
			else if (curMus == 23) {text = "Dungeon";}
			else if (curMus == 24) {text = "Plantera";}
			else if (curMus == 25) {text = "Boss5";}
			else if (curMus == 26) {text = "Temple";}
			else if (curMus == 27) {text = "Eclipse";}
			//else if (curMus == 28) {text = "Rain Sound Effect";}
			else if (curMus == 29) {text = "Mushrooms";}
			else if (curMus == 30) {text = "Pumpkin Moon";}
			else if (curMus == 31) {text = "Alt Underground";}
			else if (curMus == 32) {text = "FrostMoon";}
			else if (curMus == 33) {text = "Underground Crimson";}
			else if (curMus == 34) {text = "The Towers";}
			else if (curMus == 35) {text = "Pirate Invasion";}
			else if (curMus == 36) {text = "Hell";}
			else if (curMus == 37) {text = "Martian Madness";}
			else if (curMus == 38) {text = "Lunar Boss";}
			else if (curMus == 39) {text = "Goblin Invasion";}
			else if (curMus == 40) {text = "Sandstorm";}
			else if (curMus == 41) {text = "Old Ones Army";}
			text += " Theme";
			text += "\nby Scott Lloyd Shelly";
			foreach (var music in musList){
				//Main.NewText("cur mus = "+curMus+" / "+music.music);
				if (curMus == music.music) {
					text = music.name+"\n by "+music.composer;
				}
			}
			if (text == "Unknown") {return;}
			nameMusic = text;
			nameMusicTime = 240;
		}
		public static void DisplayAwoken(string text) {
			awoken = text;
			awokenTime = 240;
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
				TextSnippet[] snippets = ChatManager.ParseMessage(awoken, (new Color(175, 75, 255)*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth/2,Main.screenHeight/2);
				pos = pos.Floor();
				pos.Y += Main.screenHeight/4f;
				pos.Y += Main.screenHeight/8f;
				//DrawBorderString(SpriteBatch sb, string text, Vector2 pos, Color color, float scale = 1f, float anchorx = 0f, float anchory = 0f, int maxCharactersDisplayed = -1)
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, new Vector2(messageSize.X / 2f,messageSize.Y / 2f), new Vector2(0.5f,0.5f), out int hover);
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
				TextSnippet[] snippets = ChatManager.ParseMessage(nameMusic, (Color.White*alpha)).ToArray();
				Vector2 messageSize = ChatManager.GetStringSize(Main.fontDeathText, snippets, Vector2.One);
				Vector2 pos = new Vector2(Main.screenWidth,Main.screenHeight);
				pos.Y -= messageSize.Y/4f;
				pos.X -= messageSize.X/4f;
				pos = pos.Floor();
				int mus = MusicToItem(curMus);
				Texture2D texture = Main.itemTexture[mus];
				spriteBatch.Draw(texture, pos - new Vector2(messageSize.X/4f + texture.Width,0), null, Color.White*alpha, 0f, texture.Size()/2f, 1f, SpriteEffects.None, 0f);
				//Utils.DrawBorderString(spriteBatch,$"[i:{mus}]",pos - new Vector2(messageSize.X*1.5f,0),Color.White*alpha);
				ChatManager.DrawColorCodedStringWithShadow(spriteBatch, Main.fontDeathText, snippets, pos, 0f, new Vector2(messageSize.X / 2f,messageSize.Y / 2f), new Vector2(0.5f,0.5f), out int hover);
			}
		}
		public override void PostDrawInterface(SpriteBatch spriteBatch) {
			DrawAwoken(spriteBatch);
			DrawTitle(spriteBatch);
			DrawMusic(spriteBatch);
		}
		public override void UpdateUI(GameTime gameTime) {
			//Main.musicFade[curMus] > 0f
			if (MyConfig.get.musicName && !Main.gameMenu) {
				if (curMus != Main.curMusic && Main.curMusic > 0 && Main.musicFade[Main.curMusic] >= 1f) {
					curMus = Main.curMusic;
					ShowMusic();
				}
			}
		}

		// net code moment
		//public override void HandlePacket(BinaryReader reader, int whoAmI) {}
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

		[Label("Smooth camera intensity")]
		[Tooltip("The intensity of smooth camera\n [default is 0.1]")]
		[Range(0.01f, 0.4f)]
		[Increment(0.05f)]
		[DefaultValue(0.1f)]
		[DrawTicks]
		[Slider] 
		public float SmoothCameraInt;

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
		[Increment(2)]
		[DefaultValue(4)]
		[Slider] 
		[DrawTicks]
		public int ShakeInt;
		

		[Header("Experimental and Funny Stuff")]

		[Label("Multiple sound method check")]
		[Tooltip("Uses multiple sound check for more support to other mods\n[enabled by default]")]
		[DefaultValue(true)]
		public bool multipleSoundCheck;

		[Label("Display boss awoken")]
		[Tooltip("Display a text at the bottom of the screen if any boss is awoken")]
		[DefaultValue(false)]
		public bool awokenDisplay;

		[Label("Display music name")]
		[Tooltip("Display the name of currently played song in the right corner of the screen\n[enabled by default]")]
		[DefaultValue(true)]
		public bool musicName;
		
		[Label("Biome Title")]
		[Tooltip("Display biome name and the description when discovering a new biome\nsupports calamity biome !")]
		[DefaultValue(true)]
		public bool NewBiome;

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

		[Label("Simulate what most Linux users see")]
		[Tooltip("yes")]
		[DefaultValue(false)]
		public bool DemonBanner;
		/*
		public override void OnChanged() {
			if (ResetBiome && !Main.gameMenu) {
				Main.LocalPlayer.GetModPlayer<Bebeq>().biomeEncounter = new List<string>();
			}
		}
		*/

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
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake += num;
				}
				else {
					Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake = num;
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
			orig(newText, R, G, B, force);
			if (MyConfig.get.TextToCombatText) {
				CombatText.NewText(Main.LocalPlayer.getRect(),new Color(R,G,B),newText);
			}
		}
		static void SpawnOnPlayerPatch(On.Terraria.NPC.orig_SpawnOnPlayer orig,int plr, int type) {
			orig(plr,type);
			if (MyConfig.get.awokenDisplay) {
				NPC npc = new NPC();
				npc.SetDefaults(type);
				ObamaCamera.DisplayAwoken(Language.GetTextValue("Announcement.HasAwoken", npc.TypeName));
			}
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
