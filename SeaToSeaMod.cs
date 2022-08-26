﻿using UnityEngine;  //Needed for most Unity Enginer manipulations: Vectors, GameObjects, Audio, etc.
using System.IO;    //For data read/write methods
using System;    //For data read/write methods
using System.Collections.Generic;   //Working with Lists and Collections
using System.Linq;   //More advanced manipulation of lists/collections
using HarmonyLib;
using QModManager.API.ModLoading;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;
using SMLHelper.V2.Crafting;

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod {
  	
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    public static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    public static readonly XMLLocale itemLocale = new XMLLocale("XML/items.xml");
    public static readonly XMLLocale pdaLocale = new XMLLocale("XML/pda.xml");
    public static readonly XMLLocale signalLocale = new XMLLocale("XML/signals.xml");
    public static readonly XMLLocale miscLocale = new XMLLocale("XML/misc.xml");
    
    public static SeamothVoidStealthModule voidStealth;
    public static CyclopsHeatModule cyclopsHeat;
    public static SeamothDepthModule depth1300;
    public static CustomEquipable sealSuit;
    public static CustomEquipable rebreatherV2;
    public static CustomBattery t2Battery;
    
    public static BreathingFluid breathingFluid;
    public static CurativeBandage bandage;
    
    public static AlkaliPlant alkali;
    public static VentKelp kelp;
    public static HealingFlower healFlower;
    
    public static Bioprocessor processor;
    public static RebreatherRecharger rebreatherCharger;    
    public static readonly TechnologyFragment[] rebreatherChargerFragments = new TechnologyFragment[]{
    	new TechnologyFragment("f350b8ae-9ee4-4349-a6de-d031b11c82b1", go => go.transform.localScale = new Vector3(1, 3, 1)),
    	new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230", go => go.transform.localScale = new Vector3(1, 2, 1)),
  		//new TechnologyFragment("589bf5a6-6866-4828-90b2-7266661bb6ed"),
  		new TechnologyFragment("3c076458-505e-4683-90c1-34c1f7939a0f", go => go.transform.localScale = new Vector3(1, 1, 0.2F)),
    };
    public static readonly TechnologyFragment[] bioprocFragments = new TechnologyFragment[]{
    	new TechnologyFragment("85259b00-2672-497e-bec9-b200a1ab012f"),
    	//new TechnologyFragment("ba258aad-07e9-4c9b-b517-2ce7400db7b2"),
    	//new TechnologyFragment("cf4ca320-bb13-45b6-b4c9-2a079023e787"),
    	new TechnologyFragment("f4b3942e-02d8-4526-b384-677a2ad9ce58", go => go.transform.localScale = new Vector3(0.25F, 0.25F, 0.5F)),
    	new TechnologyFragment("f744e6d9-f719-4653-906b-34ed5dbdb230"),
    };
    
    public static TechnologyFragment lathingDroneFragment;
    
    public static SignalManager.ModSignal treaderSignal;
    public static SignalManager.ModSignal voidSpikeDirectionHint;
    
    public static Story.StoryGoal crashMesaRadio;
    //public static Story.StoryGoal mountainPodRadio;
    
    public static BrokenTablet brokenRedTablet;
    public static BrokenTablet brokenWhiteTablet;
    public static BrokenTablet brokenOrangeTablet;
    public static BrokenTablet brokenBlueTablet;
    
    public static DrillableMeteorite dunesMeteor;
    
    public static FMODAsset voidspikeLeviRoar;
    public static FMODAsset voidspikeLeviBite;
    public static FMODAsset voidspikeLeviFX;
    public static FMODAsset voidspikeLeviAmbient;
    
    private static readonly HashSet<TechType> gatedTechnologies = new HashSet<TechType>();

    [QModPatch]
    public static void Load() {
        config.load();
        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.logPath = Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "harmony-log.txt");
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SNUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        catch (Exception ex) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(ex.Message);
			FileLog.Log(ex.StackTrace);
			FileLog.Log(ex.ToString());
        }
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CustomPrefab).TypeHandle);
        
        itemLocale.load();
        pdaLocale.load();
        signalLocale.load();
        miscLocale.load();
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(VoidSpike).TypeHandle);
        
	    brokenRedTablet = new BrokenTablet(TechType.PrecursorKey_Red);
	    brokenWhiteTablet = new BrokenTablet(TechType.PrecursorKey_White);
	    brokenOrangeTablet = new BrokenTablet(TechType.PrecursorKey_Orange);
	    brokenBlueTablet = new BrokenTablet(TechType.PrecursorKey_Blue);
	    brokenRedTablet.Patch();
	    brokenWhiteTablet.Patch();
	    brokenOrangeTablet.Patch();
	    brokenBlueTablet.Patch();
	    
	    dunesMeteor = new DrillableMeteorite();
	    dunesMeteor.register();
	    
	    voidspikeLeviRoar = SoundManager.registerSound("voidspikelevi_roar", "Sounds/voidlevi-roar.ogg", SoundSystem.masterBus);
	    voidspikeLeviFX = SoundManager.registerSound("voidspikelevi_fx", "Sounds/voidlevi-fx1.ogg", SoundSystem.masterBus);
	    voidspikeLeviAmbient = SoundManager.registerSound("voidspikelevi_amb", "Sounds/voidlevi-longamb2.ogg", SoundSystem.masterBus);
	    voidspikeLeviBite = SoundManager.registerSound("voidspikelevi_bite", "Sounds/voidlevi-bite.ogg", SoundSystem.masterBus);
        
        addFlora();
        addItemsAndRecipes();

        BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
        lathingDroneFragment = TechnologyFragment.createFragment("6e0f4652-c439-4540-95be-e61384e27692", drone.TechType, drone.FriendlyName, 3, 2, go => {
        	ObjectUtil.removeComponent<Pickupable>(go);
        	//ObjectUtil.removeComponent<Collider>(go);
        	ObjectUtil.removeComponent<Rigidbody>(go);
        }); //it has its own model
        
        processor = new Bioprocessor();
        processor.Patch();       
        SNUtil.log("Registered custom machine "+processor);
        processor.addFragments(4, 5, bioprocFragments);
        Bioprocessor.addRecipes();
        rebreatherCharger = new RebreatherRecharger();
        rebreatherCharger.Patch();
        SNUtil.log("Registered custom machine "+rebreatherCharger);
        rebreatherCharger.addFragments(4, 10, rebreatherChargerFragments);
        
        addPDAEntries();
                 
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        addOreGen();
        
        GenUtil.registerWorldgen(new PositionedPrefab(VanillaCreatures.GHOST_LEVIATHAN.prefab, new Vector3(-125, -450, 980)));
        
        GenUtil.registerWorldgen(new PositionedPrefab(dunesMeteor.ClassID, new Vector3(-1125, -409, 1130)));
			
        XMLLocale.LocaleEntry e = SeaToSeaMod.signalLocale.getEntry("treaderpod");
		treaderSignal = SignalManager.createSignal(e);
		//treaderSignal.pdaEntry.addSubcategory("AuroraSurvivors");
		treaderSignal.addRadioTrigger(e.getField<string>("sound"));
		treaderSignal.register("32e48451-8e81-428e-9011-baca82e9cd32", new Vector3(-1239, -360, -1193));
		treaderSignal.addWorldgen();
		
        e = SeaToSeaMod.signalLocale.getEntry("voidspike");
		voidSpikeDirectionHint = SignalManager.createSignal(e);
		voidSpikeDirectionHint.setStoryGate(PDAManager.getPage("voidpod").id);
		voidSpikeDirectionHint.register("4c10bbd6-5100-4632-962e-69306b09222f", SpriteManager.Get(SpriteManager.Group.Pings, "Sunbeam"), VoidSpikesBiome.end500m);
		voidSpikeDirectionHint.addWorldgen();
		
		e = pdaLocale.getEntry("crashmesahint");
		crashMesaRadio = SNUtil.addRadioMessage("crashmesaradio", e.getField<string>("radio"), e.getField<string>("radioSound"), 1200);
		
		PDAMessages.addAll();
		
		KnownTech.onAdd += onTechUnlocked;
       
		//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
		
		gatedTechnologies.Add(TechType.Seamoth);
		gatedTechnologies.Add(TechType.Cyclops);
		gatedTechnologies.Add(TechType.Exosuit);
		gatedTechnologies.Add(TechType.ExosuitDrillArmModule);
		gatedTechnologies.Add(TechType.ExoHullModule1);
		gatedTechnologies.Add(TechType.ExoHullModule2);
		gatedTechnologies.Add(TechType.VehicleHullModule2);
		gatedTechnologies.Add(TechType.VehicleHullModule3);
		gatedTechnologies.Add(TechType.CyclopsHullModule2);
		gatedTechnologies.Add(TechType.CyclopsHullModule3);
		gatedTechnologies.Add(TechType.CyclopsThermalReactorModule);
		gatedTechnologies.Add(TechType.CyclopsFireSuppressionModule);
		gatedTechnologies.Add(TechType.StasisRifle);
		gatedTechnologies.Add(TechType.LaserCutter);
		gatedTechnologies.Add(TechType.ReinforcedDiveSuit);
		gatedTechnologies.Add(TechType.ReinforcedGloves);
		gatedTechnologies.Add(TechType.PrecursorKey_Blue);
		gatedTechnologies.Add(TechType.PrecursorKey_Red);
		gatedTechnologies.Add(TechType.PrecursorKey_White);
		gatedTechnologies.Add(TechType.PrecursorKey_Orange);
		gatedTechnologies.Add(TechType.PrecursorKey_Purple);
       
		VoidSpikesBiome.instance.register();
		VoidSpike.register();
		//AvoliteSpawner.instance.register();
    }
    
    private static void onTechUnlocked(TechType tech, bool vb) {
    	if (tech == TechType.PrecursorKey_Orange) {
    		Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    	}
    }
    
    public static bool isTechGated(TechType tt) {
    	return gatedTechnologies.Contains(tt);
    }
    
    private static void addFlora() {
		alkali = new AlkaliPlant();
		alkali.Patch();	
		alkali.addPDAEntry(itemLocale.getEntry(alkali.ClassID).pda, 3);
		SNUtil.log(" > "+alkali);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Dunes_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.KooshZone_CaveFloor, 1, 2F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.SeaTreaderPath_CaveFloor, 1, 1F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 0.5F);
		
		kelp = new VentKelp();
		kelp.Patch();	
		kelp.addPDAEntry(itemLocale.getEntry(kelp.ClassID).pda, 3);
		SNUtil.log(" > "+kelp);
		GenUtil.registerSlotWorldgen(kelp.ClassID, kelp.PrefabFileName, kelp.TechType, false, BiomeType.UnderwaterIslands_ValleyFloor, 1, 3.2F);
		//GenUtil.registerSlotWorldgen(kelp.ClassID, kelp.PrefabFileName, kelp.TechType, false, BiomeType.UnderwaterIslands_Geyser, 1, 2F);
		
		healFlower = new HealingFlower();
		healFlower.Patch();	
		healFlower.addPDAEntry(itemLocale.getEntry(healFlower.ClassID).pda, 5);
		SNUtil.log(" > "+healFlower);
		GenUtil.registerSlotWorldgen(healFlower.ClassID, healFlower.PrefabFileName, healFlower.TechType, false, BiomeType.GrassyPlateaus_CaveFloor, 1, 2.5F);
    }
    
    private static void addOreGen() {
    	BasicCustomOre vent = CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL);
    	vent.registerWorldgen(BiomeType.Dunes_ThermalVent, 1, 3F);
    	vent.registerWorldgen(BiomeType.Mountains_ThermalVent, 1, 1.2F);
    	//vent.registerWorldgen(BiomeType.JellyshroomCaves_Geyser, 1, 0.5F);
    	//vent.registerWorldgen(BiomeType.KooshZone_Geyser, 1, 1F);
    	//vent.registerWorldgen(BiomeType.GrandReef_ThermalVent, 1, 3F);
    	//vent.registerWorldgen(BiomeType.DeepGrandReef_ThermalVent, 1, 4F);
    	
    	BasicCustomOre irid = CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor, 1, 1.5F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Floor_Far, 1, 0.75F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Corridor_Wall, 1, 0.25F);
    	irid.registerWorldgen(BiomeType.InactiveLavaZone_Chamber_Ceiling, 1, 2F);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MAGNETITE.prefab, BiomeType.UnderwaterIslands_Geyser, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.DIAMOND.prefab, BiomeType.UnderwaterIslands_Geyser, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LITHIUM.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_QUARTZ.prefab, BiomeType.UnderwaterIslands_ValleyFloor, 0.2F, 1);
    	
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverCorridor_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.LostRiverJunction_LakeFloor, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Corridor_Stream, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_Lake_Floor, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Floor, 0.4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.BonesField_LakePit_Wall, 0.2F, 1);
    	//LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_SULFUR.prefab, BiomeType.SkeletonCave_Lake_Floor, 0.2F, 1);
    	vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.5F);
    	//CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
    	/*
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.LARGE_MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveSpecial, 4F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.MERCURY.prefab, BiomeType.KooshZone_CaveFloor, 0.75F, 1);
    	
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP1.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP2.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP3.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(VanillaResources.SCRAP4.prefab, BiomeType.CrashZone_Sand, 0.5F, 1);
    }
    
    private static void addCommands() {
        BuildingHandler.instance.addCommand<string, PlacedObject>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        BuildingHandler.instance.addCommand("bdtex", BuildingHandler.instance.dumpTextures);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SNUtil.playSound);
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        //ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string>>("signalUnlock", unlockSignal);
    }
    
    private static void unlockSignal(string name) {
    	switch(name) {
    		case "treaderpod":
    			treaderSignal.fireRadio();
    			break;
    		case "crashmesa":
    			Story.StoryGoal.Execute(SeaToSeaMod.crashMesaRadio.key, SeaToSeaMod.crashMesaRadio.goalType);
    			break;
    		case "voidpod":
    			VoidSpikesBiome.instance.fireRadio();
    			break;
    	}
    }
    
    private static void addItemsAndRecipes() {
        BasicCraftingItem baseGlass = CraftingItems.getItem(CraftingItems.Items.BaseGlass);
        baseGlass.craftingTime = 1.5F;
        baseGlass.numberCrafted = 2;
        baseGlass.addIngredient(TechType.Glass, 1).addIngredient(TechType.Titanium, 1);
        /*
        BasicCraftingItem byp = CraftingItems.getItem(CraftingItems.Items.TitaniumIngotFromScrap);
        byp.addIngredient(TechType.ScrapMetal, 2);
        byp.craftingTime = 5;
        byp.byproducts.Add(new PlannedIngredient(new TechTypeContainer(TechType.TitaniumIngot), 1));
        
        byp = CraftingItems.getItem(CraftingItems.Items.TitaniumFromIngot);
        byp.addIngredient(TechType.TitaniumIngot, 1);
        byp.craftingTime = 3;
        byp.byproducts.Add(new PlannedIngredient(new TechTypeContainer(TechType.Titanium), 10));
        *//*
      	TechData rec = new TechData();
      	rec.Ingredients.Add(new Ingredient(TechType.ScrapMetal, 2));
       	DuplicateRecipeDelegateWithRecipe item = new DuplicateRecipeDelegateWithRecipe(TechType.TitaniumIngot, rec);
       	item.craftTime = 5;
       	item.craftingType = CraftTree.Type.Fabricator;
       	//item.category = TechCategory.BasicMaterials;
       	//item.group = TechGroup.Resources;
       	item.craftingMenuTree = new string[]{"Resources", "BasicMaterials"};
       	item.setRecipe(1);
       	item.Patch();
       	*/
       	TechData rec = new TechData();
      	rec.Ingredients.Add(new Ingredient(TechType.TitaniumIngot, 1));
       	DuplicateRecipeDelegateWithRecipe item = new DuplicateRecipeDelegateWithRecipe(TechType.Titanium, rec);
       	item.craftTime = 3;
       	item.craftingType = CraftTree.Type.Fabricator;
       	//item.category = TechCategory.BasicMaterials;
       	//item.group = TechGroup.Resources;
       	item.craftingMenuTree = new string[]{"Resources", "BasicMaterials"};
       	item.setRecipe(10);
       	item.Patch();
       
        BasicCraftingItem enzyT = CraftingItems.getItem(CraftingItems.Items.TreaderEnzymes);
        enzyT.craftingTime = 2;
        enzyT.addIngredient(TechType.SeaTreaderPoop, 1);
       
        BasicCraftingItem enzy = CraftingItems.getItem(CraftingItems.Items.BioEnzymes);
        enzy.craftingTime = 4;
        enzy.numberCrafted = 3;
        enzy.addIngredient(TechType.Salt, 1).addIngredient(enzyT, 1).addIngredient(TechType.SeaCrownSeed, 2).addIngredient(TechType.DisinfectedWater, 1);
       
        BasicCraftingItem comb = CraftingItems.getItem(CraftingItems.Items.HoneycombComposite);
        comb.craftingTime = 12;
        comb.addIngredient(TechType.AramidFibers, 6).addIngredient(TechType.PlasteelIngot, 1);
        
        BasicCraftingItem gem = CraftingItems.getItem(CraftingItems.Items.DenseAzurite);
        gem.craftingTime = 4;
        gem.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL), 9).addIngredient(TechType.Diamond, 1).addIngredient(TechType.Magnetite, 5);
        
        BasicCraftingItem lens = CraftingItems.getItem(CraftingItems.Items.CrystalLens);
        lens.craftingTime = 20;
        lens.addIngredient(gem, 5).addIngredient(TechType.TitaniumIngot, 2).addIngredient(TechType.AdvancedWiringKit, 1).addIngredient(TechType.FiberMesh, 4);
        
        BasicCraftingItem sealedFabric = CraftingItems.getItem(CraftingItems.Items.SealFabric);
        sealedFabric.craftingTime = 4;
        sealedFabric.numberCrafted = 2;
        sealedFabric.addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 5).addIngredient(TechType.AramidFibers, 3).addIngredient(TechType.StalkerTooth, 1).addIngredient(TechType.Silicone, 2);
        
        BasicCraftingItem armor = CraftingItems.getItem(CraftingItems.Items.HullPlating);
        armor.craftingTime = 9;
        armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 5).addIngredient(comb, 1).addIngredient(TechType.Nickel, 2);
        
        BasicCraftingItem acid = CraftingItems.getItem(CraftingItems.Items.WeakAcid);
        acid.craftingTime = 1;
        acid.addIngredient(TechType.AcidMushroom, 4);
        
        BasicCraftingItem motor = CraftingItems.getItem(CraftingItems.Items.Motor);
        motor.craftingTime = 1;
        motor.addIngredient(TechType.CopperWire, 1).addIngredient(TechType.Titanium, 2).addIngredient(TechType.Lubricant, 1).addIngredient(TechType.Gold, 1);
        
        BasicCraftingItem drone = CraftingItems.getItem(CraftingItems.Items.LathingDrone);
        drone.craftingTime = 4;
        drone.addIngredient(motor, 1).addIngredient(TechType.Titanium, 1).addIngredient(TechType.ComputerChip, 1).addIngredient(TechType.PowerCell, 1);
        
        BasicCraftingItem chlorine = CraftingItems.getItem(CraftingItems.Items.Chlorine);
        chlorine.craftingTime = 3;
        chlorine.addIngredient(TechType.Salt, 2).addIngredient(TechType.GasPod, 2);
        
        CraftingItems.addAll();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        depth1300.addIngredient(TechType.VehicleHullModule3, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 4).addIngredient(CraftingItems.getItem(CraftingItems.Items.HullPlating), 3);
        depth1300.Patch();
        
        cyclopsHeat = new CyclopsHeatModule();
        cyclopsHeat.addIngredient(TechType.CyclopsThermalReactorModule, 1).addIngredient(TechType.CyclopsFireSuppressionModule, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM), 12).addIngredient(CraftingItems.getItem(CraftingItems.Items.Sealant), 4);
        cyclopsHeat.Patch();
        /*
        CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
        CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
        RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
        */
		RecipeUtil.removeRecipe(TechType.HydrochloricAcid, true);
		RecipeUtil.removeRecipe(TechType.Benzene, true);
		//do not remove creepvine, as lubricant is needed earlier than this
		
        sealSuit = new SealedSuit();
        sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 9).addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric), 6).addIngredient(TechType.Titanium, 1).addIngredient(TechType.CrashPowder, 3);
        sealSuit.Patch();
		
		t2Battery = new CustomBattery(itemLocale.getEntry("t2battery"), 500);
		t2Battery.unlockRequirement = TechType.Kyanite;//CustomMaterials.getItem(CustomMaterials.Materials.VENT_CRYSTAL).TechType;
		t2Battery.addIngredient(TechType.Battery, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 1).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.MercuryOre, 2).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Silicone, 1);
		t2Battery.Patch();
		
        rebreatherV2 = new RebreatherV2();
        rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 6).addIngredient(TechType.Benzene, 12).addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric), 3).addIngredient(TechType.Rebreather, 1).addIngredient(CraftingItems.getItem(CraftingItems.Items.Motor), 1).addIngredient(t2Battery, 1);
        rebreatherV2.Patch();
        
		breathingFluid = new BreathingFluid();
		breathingFluid.addIngredient(TechType.Benzene, 2).addIngredient(TechType.MembrainTreeSeed, 2).addIngredient(TechType.Eyeye, 3).addIngredient(TechType.PurpleRattleSpore, 1).addIngredient(TechType.OrangeMushroomSpore, 1).addIngredient(TechType.SpottedLeavesPlantSeed, 3);
		breathingFluid.Patch();
        
		bandage = new CurativeBandage();
		bandage.addIngredient(TechType.FirstAidKit, 1).addIngredient(healFlower.seed.TechType, 2).addIngredient(TechType.JellyPlant, 1);
		bandage.Patch();
		CraftData.useEatSound[bandage.TechType] = CraftData.useEatSound[TechType.FirstAidKit];
		//SurvivalHandler.GiveHealthOnConsume(bandage.TechType, 50, false);
        
        RecipeUtil.startLoggingRecipeChanges();
        
        RecipeUtil.modifyIngredients(TechType.Lubricant, i => {i.amount = 4; return false;});
        
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.Titanium, 3);
        //RecipeUtil.addIngredient(TechType.Rebreather, TechType.AdvancedWiringKit, 1);
        RecipeUtil.addIngredient(TechType.Rebreather, TechType.EnameledGlass, 1);
       // RecipeUtil.removeIngredient(TechType.Rebreather, TechType.WiringKit);
        
        RecipeUtil.modifyIngredients(TechType.Constructor, i => i.techType != TechType.TitaniumIngot);
        RecipeUtil.addIngredient(TechType.Constructor, TechType.WiringKit, 1);
        RecipeUtil.addIngredient(TechType.Constructor, TechType.Silicone, 3);
        RecipeUtil.addIngredient(TechType.Constructor, drone.TechType, 4);
        
        //RecipeUtil.addIngredient(TechType.Polyaniline, TechType.Salt, 2);
        
        RecipeUtil.addIngredient(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 4);
        RecipeUtil.removeIngredient(TechType.StasisRifle, TechType.Battery);
        RecipeUtil.addIngredient(TechType.StasisRifle, t2Battery.TechType, 2);
        
        RecipeUtil.modifyIngredients(TechType.ReinforcedDiveSuit, i => {if (i.techType == TechType.Diamond) i.amount = 4; return i.techType == TechType.Titanium;});
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 9);
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, sealSuit.TechType, 1);
        
        RecipeUtil.removeIngredient(TechType.Battery, TechType.AcidMushroom);
        RecipeUtil.addIngredient(TechType.Battery, acid.TechType, 3);
        
        RecipeUtil.addIngredient(TechType.PrecursorIonBattery, t2Battery.TechType, 1);
        
        RecipeUtil.modifyIngredients(TechType.BaseRoom, i => {if (i.techType == TechType.Titanium) i.amount = 4; return false;});
        RecipeUtil.modifyIngredients(TechType.BaseBulkhead, i => {if (i.techType == TechType.Titanium) i.amount = 2; return false;});
        RecipeUtil.modifyIngredients(TechType.PlanterBox, i => {if (i.techType == TechType.Titanium) i.amount = 3; return false;});
        RecipeUtil.modifyIngredients(TechType.BaseWaterPark, i => {if (i.techType == TechType.Titanium) i.amount = 1; return false;});
        RecipeUtil.addIngredient(TechType.BasePlanter, TechType.CreepvinePiece, 1);
        
        HashSet<TechType> set = new HashSet<TechType>{TechType.Spotlight, TechType.Techlight, TechType.Aquarium};
        for (TechType tt = TechType.BaseRoom; tt <= TechType.BaseNuclearReactor; tt++) {
        	set.Add(tt);
        }
        foreach (TechType tt in set) {
        	if (RecipeUtil.recipeExists(tt)) {
	        	Ingredient i = RecipeUtil.removeIngredient(tt, TechType.Glass);
	        	if (i != null) {
	        		RecipeUtil.addIngredient(tt, baseGlass.TechType, i.amount);
	        	}
        	}
        }
        
        RecipeUtil.removeIngredient(TechType.EnameledGlass, TechType.Glass);
        RecipeUtil.addIngredient(TechType.EnameledGlass, baseGlass.TechType, 1);
        
        Dictionary<TechType, int> addMotors = new Dictionary<TechType, int>(){
        	{TechType.BaseMoonpool, 2},
        	{TechType.Seamoth, 2},
        	{TechType.Seaglide, 1},
        	{TechType.Cyclops, 4},
        	{TechType.PipeSurfaceFloater, 1},
        	{TechType.BasePipeConnector, 1},
        	{TechType.RocketBaseLadder, 1},
        	{TechType.VendingMachine, 1},
        	{TechType.ExosuitDrillArmModule, 2},
        	{TechType.Exosuit, 3},
        };
        
        foreach (KeyValuePair<TechType, int> kvp in addMotors) {
        	int amt = -1;
        	RecipeUtil.modifyIngredients(kvp.Key, i => {if (i.techType == TechType.Lubricant){amt = i.amount; return true;} else {return false;}});
        	RecipeUtil.addIngredient(kvp.Key, motor.TechType, Math.Max(kvp.Value, amt));
        }
        
        RecipeUtil.addIngredient(TechType.Cyclops, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 3);
        RecipeUtil.addIngredient(TechType.Exosuit, CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 4);
        
        RecipeUtil.removeIngredient(TechType.ExoHullModule1, TechType.PlasteelIngot);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, TechType.Kyanite, 3);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 2);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 4);
        RecipeUtil.addIngredient(TechType.ExoHullModule2, CraftingItems.getItem(CraftingItems.Items.SmartPolymer).TechType, 3);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Kyanite);
        RecipeUtil.removeIngredient(TechType.ExoHullModule2, TechType.Titanium);
        
        RecipeUtil.addIngredient(TechType.LaserCutter, TechType.AluminumOxide, 2);
        RecipeUtil.removeIngredient(TechType.LaserCutter, TechType.Battery);
        RecipeUtil.addIngredient(TechType.LaserCutter, t2Battery.TechType, 1);
        
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 1);
        //RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 2);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);
        RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.PlasteelIngot);
        //RecipeUtil.removeIngredient(TechType.VehicleHullModule3, TechType.AluminumOxide);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, TechType.Diamond, 1);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.DenseAzurite).TechType, 1);
        
        RecipeUtil.modifyIngredients(TechType.EnameledGlass, i => {if (i.techType == TechType.Glass) i.amount *= 2; return false;});
       	RecipeUtil.getRecipe(TechType.EnameledGlass).craftAmount *= 2;
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.EnameledGlass, TechType.Diamond, 1);
        RecipeUtil.addIngredient(TechType.AdvancedWiringKit, TechType.MercuryOre, 1);
        
        RecipeUtil.addIngredient(TechType.Bleach, chlorine.TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.Bleach, 2);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, TechType.AdvancedWiringKit, 1);
        RecipeUtil.removeIngredient(TechType.BaseFiltrationMachine, TechType.CopperWire);
        RecipeUtil.addIngredient(TechType.BaseFiltrationMachine, CraftingItems.getItem(CraftingItems.Items.Sealant).TechType, 1);
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_Red, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.MercuryOre, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, TechType.Benzene, 1);
        CraftDataHandler.SetItemSize(TechType.PrecursorKey_Red, new Vector2int(2, 2));
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_Red, 6);        
        //SNUtil.addSelfUnlock(TechType.PrecursorKey_Red, PDAManager.createPage(locale.getEntry("redkey")));
        
        RecipeUtil.addRecipe(TechType.PrecursorKey_White, TechGroup.Personal, TechCategory.Equipment, 1, CraftTree.Type.Fabricator, new string[]{"Personal", "Equipment"});
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.PrecursorIonCrystal, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.Magnetite, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, TechType.UraniniteCrystal, 2);
        CraftDataHandler.SetCraftingTime(TechType.PrecursorKey_White, 8);        
        //SNUtil.addSelfUnlock(TechType.PrecursorKey_White, PDAManager.createPage(locale.getEntry("whitekey")));
        
        RecipeUtil.modifyIngredients(TechType.BaseReinforcement, i => true);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.PlasteelIngot, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 1);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.Lead, 2);
        RecipeUtil.addIngredient(TechType.BaseReinforcement, TechType.FiberMesh, 1);
        Base.FaceHullStrength[(int)Base.FaceType.Reinforcement] = 25; //from 7
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadOpened] = 6; //from 3
        Base.FaceHullStrength[(int)Base.FaceType.BulkheadClosed] = 6; //from 3
        Base.CellHullStrength[(int)Base.CellType.Foundation] = 5; //from 2
        
        brokenBlueTablet.register();
        brokenRedTablet.register();
        brokenWhiteTablet.register();
        brokenOrangeTablet.register();
        
        KnownTechHandler.Main.RemoveAllCurrentAnalysisTechEntry(TechType.VehicleHullModule3);
        
        RecipeUtil.addIngredient(TechType.PrecursorKey_Purple, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Orange, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 2);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 3);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Red, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 1);
        RecipeUtil.addIngredient(TechType.PrecursorKey_White, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 5);
        /*
        CraftDataHandler.SetItemSize(TechType.CreepvinePiece, new Vector2int(2, 2));
        CraftDataHandler.SetItemSize(TechType.CreepvineSeedCluster, new Vector2int(3, 3));
        CraftDataHandler.SetItemSize(TechType.BloodOil, new Vector2int(3, 3));
        CraftDataHandler.SetItemSize(TechType.AcidMushroom, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.WhiteMushroom, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.WhiteMushroomSpore, new Vector2int(1, 2));
        CraftDataHandler.SetItemSize(TechType.JellyPlant, new Vector2int(2, 2));
        CraftDataHandler.SetItemSize(TechType.JellyPlantSeed, new Vector2int(2, 2));
        */
       
       	//RecipeUtil.logChangedRecipes();
    }
    
    public static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdaLocale.getEntries()) {
			PDAManager.PDAPage page = PDAManager.createPage(e);
			if (e.hasField("audio"))
				page.setVoiceover(e.getField<string>("audio"));
			if (e.hasField("header"))
				page.setHeaderImage(TextureManager.getTexture("Textures/PDA/"+e.getField<string>("header")));
			page.register();
    	}
    }
   
	public static bool hasNoGasMask() {
   		return Inventory.main.equipment.GetCount(TechType.Rebreather) == 0 && Inventory.main.equipment.GetCount(rebreatherV2.TechType) == 0;
	}

  }
}
