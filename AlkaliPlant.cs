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
	
	public class AlkaliPlant : BasicCustomPlant {
		
		public AlkaliPlant() : base(SeaToSeaMod.itemLocale.getEntry("ALKALI_PLANT"), VanillaFlora.REDWORT, "Samples") {
			glowIntensity = 2;
			//seed.sprite = TextureManager.getSprite("Textures/Items/"+ObjectUtil.formatFileName(this));
		}
		
		public override Vector2int SizeInInventory {
			get {return new Vector2int(2, 2);}
		}
		
		public override void prepareGameObject(GameObject go, Renderer r) {
			base.prepareGameObject(go, r);
			go.EnsureComponent<AlkaliPlantTag>();
			go.transform.localScale = Vector3.one*2;
			/*
			GameObject seedRef = ObjectUtil.lookupPrefab("daff0e31-dd08-4219-8793-39547fdb745e").GetComponent<Plantable>().model;
			p.pickupable = go.GetComponentInChildren<Pickupable>();
			p.model = UnityEngine.Object.Instantiate(seedRef);
			GrowingPlant grow = p.model.EnsureComponent<GrowingPlant>();
			grow.seed = p;
			RenderUtil.setModel(p.model, "coral_reef_plant_middle_05", ObjectUtil.getChildObject(go, "coral_reef_plant_middle_05"));
			*//*
			CapsuleCollider cu = go.GetComponentInChildren<CapsuleCollider>();
			if (cu != null) {
				CapsuleCollider cc = p.model.AddComponent<CapsuleCollider>();
				cc.radius = cu.radius*0.8F;
				cc.center = cu.center;
				cc.direction = cu.direction;
				cc.height = cu.height;
				cc.material = cu.material;
				cc.name = cu.name;
			}
			p.modelEulerAngles = new Vector3(270*0, UnityEngine.Random.Range(0, 360F), 0);*/
			r.materials[0].SetColor("_GlowColor", new Color(1, 1, 1, 1));
		}
		
		public virtual float getScaleInGrowbed(bool indoors) {
			return indoors ? 0.25F : 0.5F;
		}
		
	}
	
	class AlkaliPlantTag : MonoBehaviour {
		
		void Start() {
    		if (gameObject.transform.position.y > -10)
    			UnityEngine.Object.Destroy(gameObject);
    		else if (gameObject.GetComponent<GrownPlant>() != null) {
    			gameObject.SetActive(true);
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(0.8F, 1.2F);
    		}
    		else {
    			gameObject.transform.localScale = Vector3.one*UnityEngine.Random.Range(2, 2.5F);
    		}
		}
		
		void Update() {
			
		}
		
	}
}
