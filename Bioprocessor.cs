﻿using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

using SMLHelper.V2.Handlers;
using SMLHelper.V2.Assets;
using SMLHelper.V2.Utility;

using ReikaKalseki.DIAlterra;

namespace ReikaKalseki.SeaToSea {
	
	public class Bioprocessor : CustomMachine<BioprocessorLogic> {
		
		internal static readonly Dictionary<TechType, BioRecipe> recipes = new Dictionary<TechType, BioRecipe>();
		
		internal static readonly Arrow leftArrow = new Arrow("arrowL", "", "", "");
		internal static readonly Arrow rightArrow = new Arrow("arrowR", "", "", "");
		
		internal static readonly float POWER_COST_IDLE = 2.0F; //per second; was 1.5 then 2.5
		internal static readonly float POWER_COST_ACTIVE = 18.0F; //per second
		
		private static readonly string MACHINE_GO_NAME = "MachineModel";
		
		static Bioprocessor() {
			leftArrow.Patch();
			rightArrow.Patch();
		}
		
		public static void addRecipes() {
			addRecipe(TechType.WhiteMushroom, TechType.HydrochloricAcid, 6, 20, 6);
			addRecipe(TechType.BloodOil, TechType.Benzene, 5, 45, 4);
			addRecipe(SeaToSeaMod.alkali.TechType, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 5, 30, 5);
			addRecipe(TechType.GasPod, CraftingItems.getItem(CraftingItems.Items.Chlorine).TechType, 1, 15, 2, 3);
			addRecipe(TechType.SnakeMushroomSpore, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2, 90, 2);
			addRecipe(TechType.HatchingEnzymes, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 4, 120, 6);
		}
		
		private static void addRecipe(TechType inp, TechType o, int salt = 5, float secs = 45, int inamt = 1, int outamt = 1) {
			BioRecipe r = new BioRecipe(salt, secs, inp, o);
			recipes[r.inputItem] = r;
			RecipeUtil.addRecipe(o, TechGroup.Uncategorized, TechCategory.Misc, 1, CraftTree.Type.None);
			RecipeUtil.addIngredient(o, SeaToSeaMod.processor.TechType, 1);
			RecipeUtil.addIngredient(o, leftArrow.TechType, 1);
			RecipeUtil.addIngredient(o, inp, inamt);
			RecipeUtil.addIngredient(o, TechType.Salt, salt);
			r.inputCount = inamt;
			r.outputCount = outamt;
		}
		
		public Bioprocessor() : base("bioprocessor", "Bioprocessor", "Decomposes and recombines organic matter into useful raw chemicals.", "6d71afaa-09b6-44d3-ba2d-66644ffe6a99") {
			addIngredient(TechType.TitaniumIngot, 1);
			addIngredient(TechType.Magnetite, 12);
			addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM).TechType, 6);
			addIngredient(TechType.CopperWire, 1);
			addIngredient(TechType.Glass, 3);
		}

		public override bool UnlockedAtStart {
			get {
				return false;
			}
		}
		
		//protected OrientedBounds[] GetBounds { get; }
		
		public override void initializeMachine(GameObject go) { //FIXME sky tint issues
			base.initializeMachine(go);
			foreach (Aquarium a in go.GetComponentsInParent<Aquarium>())
				UnityEngine.Object.Destroy(a);
			Transform t = go.transform.Find("Bubbles");
			if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);
			
			StorageContainer con = go.GetComponentInChildren<StorageContainer>();
			con.hoverText = "Use Bioprocessor";
			con.storageLabel = "BIOPROCESSOR";
			con.enabled = true;
			con.Resize(6, 6);
			//con.prefabRoot = go;
			BioprocessorLogic lgc = go.GetComponent<BioprocessorLogic>();
			lgc.storage = con;
		 	
			GameObject mdl = RenderUtil.setModel(go, "model", ObjectUtil.lookupPrefab("6ca93e93-5209-4c27-ba60-5f68f36a95fb").transform.Find("Starship_control_terminal_01").gameObject);
			mdl.transform.localPosition = new Vector3(0, 0, 0);
			mdl.transform.localEulerAngles = new Vector3(270, 0, 0);
			
		 	t = go.transform.Find(MACHINE_GO_NAME);
		 	GameObject machineMdl = null;
		 	if (t == null) {
			 	machineMdl = ObjectUtil.createWorldObject("02dfa77b-5407-4474-90c6-fcb0003ecf2d", true, false);
				machineMdl.name = MACHINE_GO_NAME;
			 	Vector3 vec = new Vector3(0, 1.41F, -0.975F);
			 	machineMdl.transform.localPosition = vec;
			 	machineMdl.transform.localScale = new Vector3(1, 1, 0.75F);
			 	machineMdl.transform.eulerAngles = new Vector3(90, 180, 0);
				machineMdl.transform.parent = go.transform;
		 	}
		 	else {
		 		machineMdl = t.gameObject;
		 	}
			
		 	foreach (Collider c in machineMdl.GetComponentsInChildren<Collider>()) {
				UnityEngine.Object.Destroy(c);
		 	}
			SphereCollider cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 0.25F, 0);
			cc = machineMdl.AddComponent<SphereCollider>();
			cc.radius = 1.2F;
			cc.center = new Vector3(0, 1F, 0);
			
			Renderer r = machineMdl.GetComponentInChildren<Renderer>();
			//SNUtil.dumpTextures(r);
			RenderUtil.swapToModdedTextures(r, this);
			RenderUtil.setEmissivity(r, 2, "GlowStrength");
			r.materials[0].EnableKeyword("MARMO_EMISSION");
			r.sharedMaterial.EnableKeyword("MARMO_EMISSION");
			r.materials[0].SetFloat("_Shininess", 8);
			r.materials[0].SetFloat("_Fresnel", 0.4F);
			lgc.mainRenderer = r;
			
			go.GetComponent<Constructable>().model = machineMdl;
			go.GetComponent<ConstructableBounds>().bounds.extents = new Vector3(1.5F, 0.5F, 1.5F);
			go.GetComponent<ConstructableBounds>().bounds.position = new Vector3(1, 0.5F, 0);
			
			t = go.transform.Find("SubDamageSounds");
			if (t != null)
				UnityEngine.Object.Destroy(t.gameObject);
			
			go.EnsureComponent<SkyApplier>();
			SkyApplier[] skies = go.GetComponentsInChildren<SkyApplier>();
			foreach (SkyApplier sky in skies)
				sky.renderers = go.GetComponentsInChildren<Renderer>();
			
			//ObjectUtil.removeComponent<PrefabIdentifier>(machineMdl);
			//ChildObjectIdentifier coi = machineMdl.EnsureComponent<ChildObjectIdentifier>();
			//coi.classId = ClassID+"_mdl";
			
			foreach (SkyApplier sky in skies) {
				sky.renderers = go.GetComponentsInChildren<Renderer>();
				sky.enabled = true;
				sky.RefreshDirtySky();
				sky.ApplySkybox();
			}
			
			setTerminalBox(go);
		}
		
		internal static void setTerminalBox(GameObject go) {
			BoxCollider box = go.transform.Find("Collider").gameObject.EnsureComponent<BoxCollider>();
			box.center = new Vector3(0, 0.5F, 0);
			box.size = new Vector3(0.5F, 1.5F, 0.5F);
		}
		
	}
		
	public class BioprocessorLogic : CustomMachineLogic {
		
		private BioRecipe currentOperation;
		private int saltRequired;
		private float nextSaltTimeRemaining;
		
		private static readonly Color offlineColor = new Color(0.1F, 0.1F, 0.1F);
		private static readonly Color noRecipeColor = new Color(1, 0, 0);
		private static readonly Color recipeStalledColor = new Color(1, 1, 0);
		private static readonly Color workingColor = new Color(0, 1, 0);
		private static readonly Color completeColor = new Color(0.25F, 0.7F, 1);
		
		private float lastColorChange = -1;
		private float colorCooldown = -1;
		private Color emissiveColor;
		
		private float lastWorkingSound = -1;
		
		internal StorageContainer storage;
		internal Renderer mainRenderer;
		private bool setCollision;
		
		void Start() {
			SNUtil.log("Reinitializing bioproc");
			SeaToSeaMod.processor.initializeMachine(gameObject);
			setEmissiveColor(new Color(0, 0, 1));
		}
		
		protected override void updateEntity(float seconds) {
			if (storage == null)
				storage = gameObject.GetComponentInChildren<StorageContainer>();
			if (mainRenderer == null)
				mainRenderer = gameObject.transform.Find("model").GetComponent<Renderer>();
			if (storage == null) {
				setEmissiveColor(new Color(1, 0, 1)); //error
				return;
			}
			//SNUtil.writeToChat("I am ticking @ "+go.transform.position);
			if (seconds <= 0)
				return;
			if (!setCollision) {
				Bioprocessor.setTerminalBox(gameObject);
				setCollision = true;
			}
			
			if (consumePower(seconds)) {
				setEmissiveColor(noRecipeColor);
				if (currentOperation != null) {
					setEmissiveColor(recipeStalledColor);
					nextSaltTimeRemaining -= seconds;
					//SNUtil.writeToChat("remaining: "+nextSaltTimeRemaining);
					if (nextSaltTimeRemaining <= 0 && consumePower(seconds*((Bioprocessor.POWER_COST_ACTIVE/Bioprocessor.POWER_COST_IDLE)-1))) {
						IList<InventoryItem> salt = storage.container.GetItems(TechType.Salt);
						if (salt != null && salt.Count >= 1) {
							storage.container.RemoveItem(salt[0].item);
							saltRequired--;
							SNUtil.playSoundAt(SNUtil.getSound("event:/loot/pickup_lubricant"), gameObject.transform.position);
							setEmissiveColor(workingColor, 1+currentOperation.secondsPerSalt);
						}
						else {
							setRecipe(null);
						}
						nextSaltTimeRemaining = currentOperation.secondsPerSalt;
						if (saltRequired <= 0) {
							//SNUtil.writeToChat("try craft");
							IList<InventoryItem> ing = storage.container.GetItems(currentOperation.inputItem);
							if (ing != null && ing.Count >= currentOperation.inputCount) {
								//SNUtil.writeToChat("success");
								for (int i = 0; i < currentOperation.inputCount; i++)
									storage.container.RemoveItem(ing[0].item); //list is updated in realtime
								for (int i = 0; i < currentOperation.outputCount; i++) {
									GameObject item = ObjectUtil.createWorldObject(CraftData.GetClassIdForTechType(currentOperation.outputItem), true, false);
									item.SetActive(false);
									storage.container.AddItem(item.GetComponent<Pickupable>());
									colorCooldown = -1;
									setEmissiveColor(completeColor, 4);
									SNUtil.playSoundAt(SNUtil.getSound("event:/tools/knife/heat_hit"), gameObject.transform.position);
									setRecipe(null);
								}
							}
							else {
								setRecipe(null);
							}
						}
					}
					else if (DayNightCycle.main.timePassedAsFloat-lastWorkingSound >= 1.0) {
						lastWorkingSound = DayNightCycle.main.timePassedAsFloat;
						//SNUtil.playSoundAt(SNUtil.getSound("event:/sub_module/workbench/working"), gameObject.transform.position);
					}
				}
				else {
					//SNUtil.writeToChat("Looking for recipe");
					foreach (BioRecipe r in Bioprocessor.recipes.Values) {
						if (canRunRecipe(r)) {
							//SNUtil.writeToChat("Found "+r);
							setRecipe(r);
							break;
						}
					}
				}
			}
			else {
				setRecipe(null);
				//SNUtil.writeToChat("Insufficient power");
				setEmissiveColor(offlineColor);
			}
		}
		
		private void setEmissiveColor(Color c, float cooldown = -1) {
			if (mainRenderer == null)
				return;
			float time = DayNightCycle.main.timePassedAsFloat;
			if (time-lastColorChange < colorCooldown && cooldown < colorCooldown)
				return;
			emissiveColor = c;
			colorCooldown = cooldown;
			Material m = mainRenderer.materials[0];
			m.SetColor("_GlowColor", emissiveColor);
			lastColorChange = time;
		}
		
		private bool consumePower(float sc = 1) {
			SubRoot sub = gameObject.GetComponentInParent<SubRoot>();
			if (sub == null)
				return false;
			float receive;
			sub.powerRelay.ConsumeEnergy(Bioprocessor.POWER_COST_IDLE*sc, out receive);
			receive += 0.0001F;
			//if (receive < Bioprocessor.POWER_COST_IDLE*sc)
			//	SNUtil.writeToChat("Wanted "+(Bioprocessor.POWER_COST_IDLE*sc)+", got "+receive);
			return receive >= Bioprocessor.POWER_COST_IDLE*sc;//Mathf.Approximately(Bioprocessor.POWER_COST*sc, receive);
		}
		
		private bool canRunRecipe(BioRecipe r) {
			if (!KnownTech.knownTech.Contains(r.inputItem) || !KnownTech.knownTech.Contains(r.outputItem))
				return false;
			IList<InventoryItem> ing = storage.container.GetItems(r.inputItem);
			IList<InventoryItem> salt = storage.container.GetItems(TechType.Salt);
			return ing != null && salt != null && salt.Count >= r.saltCount && ing.Count >= r.inputCount;
		}
		
		private void setRecipe(BioRecipe r) {
			currentOperation = r;
			saltRequired = r != null ? r.saltCount : -1;
			nextSaltTimeRemaining = r != null ? /*r.secondsPerSalt*/0.05F : -1;
			setEmissiveColor(r == null ? noRecipeColor : recipeStalledColor);
			SNUtil.playSoundAt(SNUtil.getSound(r == null ? "event:/sub/seamoth/seamoth_light_off" : "event:/sub/seamoth/seamoth_light_on"), gameObject.transform.position);
		}
		
	}
	
	class BioRecipe {
			
		internal readonly TechType inputItem;
		internal readonly TechType outputItem;
		internal readonly int saltCount;
		internal readonly float processTime;
		
		internal readonly float secondsPerSalt;
		
		internal int inputCount = 1;
		internal int outputCount = 1;
		
		internal BioRecipe(int s, float t, TechType inp, TechType o) {
			inputItem = inp;
			outputItem = o;
			saltCount = s;
			processTime = t;
			secondsPerSalt = processTime/(float)saltCount;
		}
		
		public override string ToString()
		{
			return string.Format("[BioRecipe InputItem={0}, OutputItem={1}, SaltCount={2}, ProcessTime={3}, SecondsPerSalt={4}, InputCount={5}, OutputCount={6}]", inputItem, outputItem, saltCount, processTime, secondsPerSalt, inputCount, outputCount);
		}

		
	}
}
