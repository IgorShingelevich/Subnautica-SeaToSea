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

namespace ReikaKalseki.SeaToSea
{
  [QModCore]
  public static class SeaToSeaMod
  {
    public const string MOD_KEY = "ReikaKalseki.SeaToSea";
    
    public static readonly Config<C2CConfig.ConfigEntries> config = new Config<C2CConfig.ConfigEntries>();
    public static readonly XMLLocale locale = new XMLLocale("XML/items.xml");
    public static readonly XMLLocale pdas = new XMLLocale("XML/pda.xml");
    public static readonly XMLLocale signals = new XMLLocale("XML/signals.xml");
    
    public static SeamothVoidStealthModule voidStealth;
    public static CyclopsHeatModule cyclopsHeat;
    public static SeamothDepthModule depth1300;
    public static CustomEquipable sealSuit;
    public static CustomEquipable rebreatherV2;
    public static CustomBattery t2Battery;
    
    public static AlkaliPlant alkali;
    
    public static Bioprocessor processor;

    [QModPatch]
    public static void Load()
    {
        config.load();
        
        Harmony harmony = new Harmony(MOD_KEY);
        Harmony.DEBUG = true;
        FileLog.Log("Ran mod register, started harmony (harmony log)");
        SBUtil.log("Ran mod register, started harmony");
        try {
        	harmony.PatchAll(System.Reflection.Assembly.GetExecutingAssembly());
        }
        catch (Exception e) {
			FileLog.Log("Caught exception when running patcher!");
			FileLog.Log(e.Message);
			FileLog.Log(e.StackTrace);
			FileLog.Log(e.ToString());
        }
        
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(WorldGenerator).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(PlacedObject).TypeHandle);
        System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(CustomPrefab).TypeHandle);
        
        locale.load();
        pdas.load();
        signals.load();
        
        processor = new Bioprocessor();
        processor.Patch();
        SBUtil.log("Registered custom machine "+processor);
        
        addFlora();
        addItemsAndRecipes();
                 
        WorldgenDatabase.instance.load();
        DataboxTypingMap.instance.load();
        
        addCommands();
        addPDAEntries();
        addOreGen();
        /*
        GenUtil.registerWorldgen("00037e80-3037-48cf-b769-dc97c761e5f6", new Vector3(622.7F, -250.0F, -1122F), new Vector3(0, 32, 0)); //lifepod 13 (khasar)
        spawnDatabox(TechType.SwimChargeFins, new Vector3(622.7F, -249.3F, -1122F));
        */
       
		//DamageSystem.acidImmune = DamageSystem.acidImmune.AddToArray<TechType>(TechType.Seamoth);
       
		VoidSpikesBiome.instance.register();
		//AvoliteSpawner.instance.register();
       
        /*
        for (int i = 0; i < 12; i++) {
        	double r = UnityEngine.Random.Range(1.5F, 12);
        	double ang = UnityEngine.Random.Range(0, 360F);
        	double cos = Math.Cos(ang*Math.PI/180D);
        	double sin = Math.Sin(ang*Math.PI/180D);
        	double rx = r*cos;
        	double rz = r*sin;
        	bool big = UnityEngine.Random.Range(0, 1F) < 0.2;
        	Vector3 pos2 = new Vector3((float)(pos.x+rx), pos.y, (float)(pos.z+rz));
        	GenUtil.registerWorldgen(big ? VanillaResources.LARGE_KYANITE.prefab : VanillaResources.KYANITE.prefab, pos2);
        }*/
        
        //GenUtil.registerWorldgen(VanillaResources.LARGE_DIAMOND.prefab, new Vector3(-1496, -325, -714), new Vector3(120, 60, 45));
    }
    
    private static void addFlora() {
		alkali = new AlkaliPlant();
		alkali.Patch();	
		alkali.addPDAEntry(locale.getEntry(alkali.ClassID).pda, 3);
		SBUtil.log(" > "+alkali);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_IslandCaveFloor, 1, 1F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Mountains_CaveFloor, 1, 0.5F);
		//GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.Dunes_CaveFloor, 1, 0.5F);
		GenUtil.registerSlotWorldgen(alkali.ClassID, alkali.PrefabFileName, alkali.TechType, false, BiomeType.KooshZone_CaveFloor, 1, 2F);
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
    	
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.DrillableMagnetite), BiomeType.UnderwaterIslands_Geyser, 0.2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Lithium), BiomeType.UnderwaterIslands_Geyser, 1.5F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Quartz), BiomeType.UnderwaterIslands_Geyser, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Diamond), BiomeType.UnderwaterIslands_Geyser, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Quartz), BiomeType.UnderwaterIslands_ValleyFloor, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Lithium), BiomeType.UnderwaterIslands_ValleyFloor, 1F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.DrillableQuartz), BiomeType.UnderwaterIslands_ValleyFloor, 0.2F, 1);
    	vent.registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 0.5F);
    	//CustomMaterials.getItem(CustomMaterials.Materials.).registerWorldgen(BiomeType.UnderwaterIslands_Geyser, 1, 8F);
    	/*
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Dunes_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.Mountains_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.GrandReef_ThermalVent, 2F, 1);
    	LootDistributionHandler.EditLootDistributionData(CraftData.GetClassIdForTechType(TechType.Magnetite), BiomeType.DeepGrandReef_ThermalVent, 2F, 1);*/
    }
    
    private static void addCommands() {
        BuildingHandler.instance.addCommand<string>("pfb", BuildingHandler.instance.spawnPrefabAtLook);
        //BuildingHandler.instance.addCommand<string>("btt", BuildingHandler.instance.spawnTechTypeAtLook);
        BuildingHandler.instance.addCommand<bool>("bden", BuildingHandler.instance.setEnabled);  
        BuildingHandler.instance.addCommand("bdsa", BuildingHandler.instance.selectAll);
        BuildingHandler.instance.addCommand("bdslp", BuildingHandler.instance.selectLastPlaced);
        BuildingHandler.instance.addCommand<string>("bdexs", BuildingHandler.instance.saveSelection);
        BuildingHandler.instance.addCommand<string>("bdexa", BuildingHandler.instance.saveAll);
        BuildingHandler.instance.addCommand<string>("bdld", BuildingHandler.instance.loadFile);
        BuildingHandler.instance.addCommand("bdinfo", BuildingHandler.instance.selectedInfo);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, bool>>("sound", SBUtil.playSound);
       // ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("voidsig", VoidSpikesBiome.instance.activateSignal);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action<string, string, string>>("exec", DebugExec.run);
        ConsoleCommandsHandler.Main.RegisterConsoleCommand<Action>("execTemp", DebugExec.tempCode);
    }
    
    private static void addItemsAndRecipes() {
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
        armor.addIngredient(TechType.PlasteelIngot, 2).addIngredient(TechType.Lead, 5).addIngredient(comb, 1);
        
        CraftingItems.addAll();
        
        voidStealth = new SeamothVoidStealthModule();
        voidStealth.addIngredient(lens, 1).addIngredient(comb, 2).addIngredient(TechType.Aerogel, 12);
        voidStealth.Patch();
        
        depth1300 = new SeamothDepthModule("SMDepth4", "Seamoth Depth Module MK4", "Increases crush depth to 1300m.", 1300);
        depth1300.addIngredient(TechType.VehicleHullModule3, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS), 4).addIngredient(TechType.Nickel, 6);
        depth1300.Patch();
        
        cyclopsHeat = new CyclopsHeatModule();
        cyclopsHeat.addIngredient(TechType.CyclopsThermalReactorModule, 1).addIngredient(TechType.CyclopsFireSuppressionModule, 1).addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.IRIDIUM).TechType, 12).addIngredient(alkali.TechType, 4);
        cyclopsHeat.Patch();
        /*
        CraftData.itemSizes[TechType.AcidMushroom] = new Vector2int(1, 2);
        CraftData.itemSizes[TechType.HydrochloricAcid] = new Vector2int(2, 2);
        RecipeUtil.modifyIngredients(TechType.HydrochloricAcid, i => i.amount = 12);
        */
		RecipeUtil.removeRecipe(TechType.HydrochloricAcid);
		RecipeUtil.removeRecipe(TechType.Benzene);
		
        sealSuit = new SealedSuit();
        sealSuit.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 9).addIngredient(CraftingItems.getItem(CraftingItems.Items.SealFabric), 6);
        sealSuit.Patch();
		
		t2Battery = new CustomBattery(locale.getEntry("t2battery"), 500);
        t2Battery.addIngredient(CraftingItems.getItem(CraftingItems.Items.DenseAzurite), 1).addIngredient(TechType.Polyaniline, 1).addIngredient(TechType.Lithium, 2).addIngredient(TechType.Silicone, 1);
		t2Battery.Patch();
		
        rebreatherV2 = new RebreatherV2();
        rebreatherV2.addIngredient(CustomMaterials.getItem(CustomMaterials.Materials.PLATINUM), 6).addIngredient(TechType.Benzene, 12).addIngredient(TechType.Silicone, 3).addIngredient(TechType.Rebreather, 1).addIngredient(t2Battery, 1);
        rebreatherV2.Patch();
        
        RecipeUtil.addIngredient(TechType.Polyaniline, TechType.Salt, 4);
        RecipeUtil.addIngredient(TechType.StasisRifle, CustomMaterials.getItem(CustomMaterials.Materials.PHASE_CRYSTAL).TechType, 4);
        RecipeUtil.removeIngredient(TechType.StasisRifle, TechType.Battery);
        RecipeUtil.addIngredient(TechType.StasisRifle, t2Battery.TechType, 2);
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, CustomMaterials.getItem(CustomMaterials.Materials.PRESSURE_CRYSTALS).TechType, 9);
        RecipeUtil.addIngredient(TechType.ReinforcedDiveSuit, sealSuit.TechType, 1);
        RecipeUtil.addIngredient(TechType.Cyclops, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 3);
        RecipeUtil.addIngredient(TechType.Cyclops, TechType.Nickel, 6);
        RecipeUtil.addIngredient(TechType.ExoHullModule1, TechType.Kyanite, 3);
        RecipeUtil.addIngredient(TechType.LaserCutter, TechType.AluminumOxide, 2);
        RecipeUtil.removeIngredient(TechType.LaserCutter, TechType.Battery);
        RecipeUtil.addIngredient(TechType.LaserCutter, t2Battery.TechType, 1);
        RecipeUtil.addIngredient(TechType.VehicleHullModule2, CraftingItems.getItem(CraftingItems.Items.HoneycombComposite).TechType, 1);
        RecipeUtil.addIngredient(TechType.VehicleHullModule3, CraftingItems.getItem(CraftingItems.Items.HullPlating).TechType, 2);
        RecipeUtil.addIngredient(TechType.PrecursorKey_Blue, CraftingItems.getItem(CraftingItems.Items.Luminol).TechType, 3);
    }
    
    public static void addPDAEntries() {
    	foreach (XMLLocale.LocaleEntry e in pdas.getEntries()) {
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
