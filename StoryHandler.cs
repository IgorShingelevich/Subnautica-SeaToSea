﻿using System;
using System.Collections.Generic;
using System.Linq;

using Story;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using UnityEngine;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea
{
	public class StoryHandler : IStoryGoalListener {
		
		public static readonly StoryHandler instance = new StoryHandler();
		
		private readonly Dictionary<ProgressionTrigger, DelayedProgressionEffect> triggers = new Dictionary<ProgressionTrigger, DelayedProgressionEffect>();
		
	    private readonly Vector3 pod12Location = new Vector3(1117, -268, 568);
	    private readonly Vector3 pod3Location = new Vector3(-33, -23, 409);
	    private readonly Vector3 pod6Location = new Vector3(363, -110, 309);
	    private readonly Vector3 dronePDACaveEntrance = new Vector3(-80, -79, 262);
	    
	    private readonly Vector3[] seacrownCaveEntrances = new Vector3[]{
	    	new Vector3(279, -140, 288),//new Vector3(300, -120, 288)/**0.67F+pod6Location*0.33F*/,
	    	//new Vector3(66, -100, -608), big obvious but empty one
	    	new Vector3(-621, -130, -190),//new Vector3(-672, -100, -176),
	    	//new Vector3(-502, -80, -102), //empty in vanilla, and right by pod 17
	    };
	    
	    private float lastDunesEntry = -1;
		
		private StoryHandler() {
			triggers[new StoryTrigger("AuroraRadiationFixed")] = new DelayedProgressionEffect(VoidSpikesBiome.instance.fireRadio, VoidSpikesBiome.instance.isRadioFired, 0.00003F);
			triggers[new TechTrigger(TechType.PrecursorKey_Orange)] = new DelayedStoryEffect(SeaToSeaMod.crashMesaRadio, 0.00004F);
			triggers[new ProgressionTrigger(ep => ep.GetVehicle() is SeaMoth)] = new DelayedProgressionEffect(SeaToSeaMod.treaderSignal.fireRadio, SeaToSeaMod.treaderSignal.isRadioFired, 0.000018F);
			
			
			StoryGoal pod12Radio = new StoryGoal("RadioKoosh26", Story.GoalType.Radio, 0);
			DelayedStoryEffect ds = new DelayedStoryEffect(pod12Radio, 0.00008F);
			triggers[new StoryTrigger("SunbeamCheckPlayerRange")] = ds;
			triggers[new TechTrigger(TechType.BaseNuclearReactor)] = ds;
			triggers[new TechTrigger(TechType.HighCapacityTank)] = ds;
			triggers[new TechTrigger(TechType.PrecursorKey_Purple)] = ds;
			triggers[new TechTrigger(TechType.BaseUpgradeConsole)] = ds;
			triggers[new TechTrigger(CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType)] = ds;
			triggers[new EncylopediaTrigger("SnakeMushroom")] = ds;
			
			addPDAPrompt(PDAMessages.Messages.KooshCavePrompt, ep => Vector3.Distance(pod12Location, ep.transform.position) <= 75);
			addPDAPrompt(PDAMessages.Messages.RedGrassCavePrompt, isNearSeacrownCave);
			PDAPrompt kelp = addPDAPrompt(PDAMessages.Messages.KelpCavePrompt, ep => MathUtil.isPointInCylinder(dronePDACaveEntrance.setY(-40), ep.transform.position, 60, 40) || (PDAMessages.isTriggered(PDAMessages.Messages.FollowRadioPrompt) && Vector3.Distance(pod3Location, ep.transform.position) <= 60));
			/*
			PDAPrompt kelpLate = addPDAPrompt(PDAMessages.Messages.KelpCavePromptLate, new TechTrigger(TechType.HighCapacityTank), 0.0001F);
			addPDAPrompt(kelpLate, new TechTrigger(TechType.StasisRifle));
			addPDAPrompt(kelpLate, new TechTrigger(TechType.BaseMoonpool));
			*/
			triggers[new PDAPromptCondition(new ProgressionTrigger(doDunesCheck))] = new DunesPrompt();
			
			addPDAPrompt(PDAMessages.Messages.FollowRadioPrompt, hasMissedRadioSignals);
		}
	    
	    private bool hasMissedRadioSignals(Player ep) {
	    	bool late = KnownTech.knownTech.Contains(TechType.StasisRifle) || KnownTech.knownTech.Contains(TechType.BaseMoonpool) || KnownTech.knownTech.Contains(TechType.HighCapacityTank);
	    	bool all = PDAMessages.isTriggered(PDAMessages.Messages.RedGrassCavePrompt) && PDAMessages.isTriggered(PDAMessages.Messages.KelpCavePrompt) && PDAMessages.isTriggered(PDAMessages.Messages.KooshCavePrompt);
	    	return late && !all;
	    }
	    
	    private bool isNearSeacrownCave(Player ep) {
	    	Vector3 pos = ep.transform.position;
		    foreach (Vector3 vec in seacrownCaveEntrances) {
				if (pos.y <= vec.y && MathUtil.isPointInCylinder(vec, pos, 30, 10)) {
	    			return true;
			    }
			}
	    	return false;
	    }
	    
	    private PDAPrompt addPDAPrompt(PDAMessages.Messages m, Func<Player, bool> condition, float ch = 0.01F) {
	    	return addPDAPrompt(m, new ProgressionTrigger(condition), ch);
	    }
	    
	    private PDAPrompt addPDAPrompt(PDAMessages.Messages m, ProgressionTrigger pt, float ch = 0.01F) {
	    	PDAPrompt p = new PDAPrompt(m, ch);
	    	addPDAPrompt(p, pt);
	    	return p;
	    }
	    
	    private void addPDAPrompt(PDAPrompt m, ProgressionTrigger pt) {
	    	triggers[new PDAPromptCondition(pt)] = m;
	    }
		
		public void tick(Player ep) {
			foreach (KeyValuePair<ProgressionTrigger, DelayedProgressionEffect> kvp in triggers) {
				if (kvp.Key.isReady(ep)) {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" is ready");
					DelayedProgressionEffect dt = kvp.Value;
					if (!dt.isFired() && UnityEngine.Random.Range(0, 1F) <= dt.chancePerTick*Time.timeScale) {
						//SNUtil.writeToChat("Firing "+dt);
						dt.fire();
					}
				}
				else {
					//SNUtil.writeToChat("Trigger "+kvp.Key+" condition is not met");
				}
			}
		}
	    
	    private bool doDunesCheck(Player ep) {
	    	if (ep.GetBiomeString() != null && ep.GetBiomeString().ToLowerInvariant().Contains("dunes")) {
	    		float time = DayNightCycle.main.timePassedAsFloat;
	    		if (lastDunesEntry < 0)
	    			lastDunesEntry = time;
	    		//SNUtil.writeToChat(lastDunesEntry+" > "+(time-lastDunesEntry));
	    		if (time-lastDunesEntry >= 90) { //in dunes for at least 90s
	    			return true;
	    		}
	    	}
	    	else {
	    		lastDunesEntry = -1;
	    	}
	    		return false;
	    }
		
		public void NotifyGoalComplete(string key) {
			//SNUtil.writeToChat("Story '"+key+"'");
			if (key.StartsWith("OnPlay", StringComparison.InvariantCultureIgnoreCase)) {
				if (key.Contains(SeaToSeaMod.treaderSignal.storyGate)) {
					SeaToSeaMod.treaderSignal.activate(20);
				}
				else if (key.Contains(VoidSpikesBiome.instance.getSignalKey())) {
					VoidSpikesBiome.instance.activateSignal();
				}
				else if (key.Contains(SeaToSeaMod.crashMesaRadio.key)) {
					Player.main.gameObject.EnsureComponent<CrashMesaCallback>().Invoke("trigger", 25);
				}
			}
			else if (key == PDAManager.getPage("voidpod").id) { //id is pda page story key
				SeaToSeaMod.voidSpikeDirectionHint.activate(4);
			}
			else {
				switch(key) {
					case "SunbeamCheckPlayerRange":
						Player.main.gameObject.EnsureComponent<AvoliteSpawner.TriggerCallback>().Invoke("trigger", 39);
					break;
					case "drfwarperheat":
						KnownTech.Add(SeaToSeaMod.cyclopsHeat.TechType);
					break;
				}
			}
		}
	
		internal bool canTriggerPDAPrompt(Player ep) {
	    	return SeaToSeaMod.config.getBoolean(C2CConfig.ConfigEntries.PROMPTS) && (ep.IsSwimming() || ep.GetVehicle() != null) && ep.currentSub == null;
		}
	}
	
	internal class PDAPromptCondition : ProgressionTrigger {
		
		private readonly ProgressionTrigger baseline;
		
		public PDAPromptCondition(ProgressionTrigger p) : base(ep => StoryHandler.instance.canTriggerPDAPrompt(ep) && p.isReady(ep)) {
			baseline = p;
		}
		
		public override string ToString() {
			return "Free-swimming "+baseline;
		}
		
	}
	
	internal class PDAPrompt : DelayedProgressionEffect {
		
		private readonly PDAMessages.Messages prompt;
		
		public PDAPrompt(PDAMessages.Messages m, float f) : base(() => PDAMessages.trigger(m), () => PDAMessages.isTriggered(m), f) {
			prompt = m;
		}
		
		public override string ToString() {
			return "PDA Prompt "+prompt;
		}
		
	}
	
	internal class DunesPrompt : DelayedProgressionEffect {
		
		private static readonly PDAManager.PDAPage page = PDAManager.getPage("dunearchhint");
		
		public DunesPrompt() : base(() => {PDAMessages.trigger(PDAMessages.Messages.DuneArchPrompt); page.unlock(false);}, () => PDAMessages.isTriggered(PDAMessages.Messages.DuneArchPrompt), 0.006F) {
			
		}
		
		public override string ToString() {
			return "Dunes Prompt";
		}
		
	}
	
	class CrashMesaCallback : MonoBehaviour {
			
		void trigger() {
			SNUtil.playSound("event:/tools/scanner/new_encyclopediea"); //triple-click
			SNUtil.playSound("event:/player/story/RadioShallows22NoSignalAlt"); //"signal coordinates corrupted"
			PDAManager.getPage("crashmesahint").unlock(false);
		}
		
	}
		
}
	