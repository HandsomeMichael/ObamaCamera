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
			bool contain = false;
			if (MyConfig.get.BossIntro && npc.realLife < 0 &&
				npc.type != NPCID.MoonLordHand && npc.type != NPCID.MoonLordHead &&
				!ObamaCamera.Moonlord && !ObamaCamera.bossEncounter.Contains(npc.type) && 
				(npc.boss || npc.type == NPCID.WallofFleshEye || npc.type == NPCID.EaterofWorldsHead) 
				&& npc.type != NPCID.WallofFlesh && ObamaCamera.NPCFocus == -1 && !ThereIsOtherBoss(npc.whoAmI)) {

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
						subtitle = "The Woman";
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
		}
		public override void ProcessTriggers(TriggersSet triggersSet) {
			if (ObamaCamera.BossLook.Current) {
				QuickLook = true;
			}
			if (ObamaCamera.LockCamera.JustPressed) {
				IsLockCamera = (!IsLockCamera);
				CombatText.NewText(player.getRect(),(IsLockCamera ? Color.LightGreen : Color.Pink),(IsLockCamera ? "Camera Lock":"Camera Unlock"));
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
				CombatText.NewText(player.getRect(),Color.White,config.CameraFollow);
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
							screenCache = Vector2.Lerp(screenCache,pos,0.1f);
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
					if (config.SmoothCamera) {screenCache = Vector2.Lerp(screenCache,targetCenter - centerScreen,0.1f);}
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
				if (flag1){
					screenCache = Vector2.Lerp(screenCache,player.Center - centerScreen,0.1f);
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
		public static ModHotKey BossLook;
		public static ModHotKey SwitchFollow;
		public static ModHotKey LockCamera;

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
		public override void Load() {

			Enable = true;
			NPCFocus = -1;
			Moonlord = false;

			//adding dummy values
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
			BossLook = null;
			SwitchFollow = null;
			LockCamera = null;
			bossEncounter = null;
			titleData = null;
			titleText = null;
			titleSubText = null;
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

		public static void Title(string text, string subtitle = "", Texture2D num = null) {
			titleText = text;
			titleTimer = 240;
			titleSubText = subtitle;
			title2D = num;
			titleColor = Color.White;
		}
		public override void PreSaveAndQuit() {
			titleTimer = 0;
			titleText = "";
			titleSubText = "";
			title2D = null;
			bossEncounter = new List<int>();
		}
		public override void PostDrawInterface(SpriteBatch spriteBatch) {
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

		[Label("Camera Boss Intro")]
		[Tooltip("Make the camera lock on bosses when they first spawned \nand display the name of the boss")]
		[DefaultValue(true)]
		public bool BossIntro;

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
		[Tooltip("Immediately set the camera positon to player center when its too far away")]
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

		[Label("Screen Shake On Worm Boss")]
		[Tooltip("Shake the screen whenever a worm boss is digging")]
		[DefaultValue(false)]
		public bool WormShake;

		[Label("Screen Shake On Failed breaking tile")]
		[Tooltip("Shake the screen whenever the player failed at breaking tiles")]
		[DefaultValue(true)]
		public bool TileShake;

		[Label("Screen Shake Limit Breaker")]
		[Tooltip("Allow the screen shake effect be stacked")]
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

		[Label("Biome Title")]
		[Tooltip("Display biome name and the description when discovering a new biome")]
		[DefaultValue(false)]
		public bool NewBiome;

		[Label("Biome Title Reset")]
		[Tooltip("Reset Local Player Discovered Biome \nChange this config to reset")]
		[DefaultValue(false)]
		public bool ResetBiome;

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

		public override void OnChanged() {
			if (ResetBiome && !Main.gameMenu) {
				Main.LocalPlayer.GetModPlayer<Bebeq>().biomeEncounter = new List<string>();
			}
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
			On.Terraria.Main.PlaySound_int_int_int_int_float_float += SoundPatch;
			On.Terraria.Player.PickTile += TilePatch;
			On_ModifyScreenPosition += ScreenPatch;
			On.Terraria.Main.NewText_string_byte_byte_byte_bool += NewTextPatch;
		}
		public static void Remove() {
			On.Terraria.Main.PlaySound_int_int_int_int_float_float -= SoundPatch;
			On.Terraria.Player.PickTile -= TilePatch;
			On_ModifyScreenPosition -= ScreenPatch;
			On.Terraria.Main.NewText_string_byte_byte_byte_bool -= NewTextPatch;
		}
		// sound list
		// Item_14
		// Item_62
		// Zombie_20
		// Zombie_104
		// Zombie_92
		static void SoundShake(int type, int Style,float volumeScale) {
			int num = 0;
			if ((type == SoundID.Roar && (Style == 0 || Style == 2)) || type == SoundID.ForceRoar) {
				num = (int)((float)MyConfig.get.ShakeInt*(2f + volumeScale));
			}
			if ((type == SoundID.Zombie && (Style == 20 || Style == 104 || Style == 92)) ||
			 	(type == SoundID.Item && (Style == 14 || Style == 62))) {
				num = MyConfig.get.ShakeInt*2;
			}
			if (num > 0) {
				if (MyConfig.get.ShakeLimit) {Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake += num;}
				else {Main.LocalPlayer.GetModPlayer<Bebeq>().camerashake = num;}
			}
		}
		static void ScreenPatch(orig_ModifyScreenPosition orig, Player player) {
			if (!MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
			orig(player);
			if (MyConfig.get.Override) {
				player.GetModPlayer<Bebeq>().CameraMod();
			}
		}
		static SoundEffectInstance SoundPatch(On.Terraria.Main.orig_PlaySound_int_int_int_int_float_float orig,int type, int x, int y, int Style, float volumeScale, float pitchOffset){
			if (MyConfig.get.RoarShake && !Main.gameMenu) {
				if (Vector2.Distance(new Vector2(x,y),Main.LocalPlayer.Center) < 700f) {
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
