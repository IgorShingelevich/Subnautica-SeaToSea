﻿/*
 * Created by SharpDevelop.
 * User: Reika
 * Date: 11/04/2022
 * Time: 4:11 PM
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Scripting;
using UnityEngine.UI;
using System.Collections.Generic;
using ReikaKalseki.DIAlterra;
using ReikaKalseki.SeaToSea;
using SMLHelper.V2.Handlers;
using SMLHelper.V2.Utility;

namespace ReikaKalseki.SeaToSea
{		
	internal class RandomPlant : PieceBase {
		
		protected readonly WeightedRandom<VanillaFlora> plants = new WeightedRandom<VanillaFlora>();
		
		protected bool preferLit = true;
		protected int count = 1;
		protected Vector3 fuzz = Vector3.zero;
		
		public RandomPlant(Vector3 vec) : base(vec) {
			
		}
		
		public override void generate(List<GameObject> li) {
			for (int i = 0; i < count; i++) {
				Vector3 vec = new Vector3(position.x, position.y, position.z);
				if (fuzz.magnitude > 0.05) {
					vec.x += UnityEngine.Random.Range(-fuzz.x, fuzz.x);
					vec.y += UnityEngine.Random.Range(-fuzz.y, fuzz.y);
					vec.z += UnityEngine.Random.Range(-fuzz.z, fuzz.z);
				}
				string type = plants.getRandomEntry().getRandomPrefab(preferLit);
				GameObject go = generatePlant(vec, type);
				li.Add(go);
			}
		}
		
		protected virtual GameObject generatePlant(Vector3 vec, string type) {
			GameObject go = SBUtil.createWorldObject(type);
			go.transform.position = vec;
			go.transform.rotation = Quaternion.AngleAxis(UnityEngine.Random.Range(0, 360F), Vector3.up);
			return go;
		}
		
		public override void loadFromXML(XmlElement e) {
			foreach (XmlElement e2 in e.getDirectElementsByTagName("plant")) {
				string name = e2.getProperty("name");
				string wt = e2.getProperty("weight");
				plants.addEntry(VanillaFlora.getByName(name), double.Parse(wt));
			}
			preferLit = e.getBoolean("lit");
			count = e.getInt("count", 1);
			Vector3? f = e.getVector("fuzz", true);
			if (f != null && f.HasValue)
				fuzz = f.Value;
		}
		
		public override void saveToXML(XmlElement e) {
			foreach (VanillaFlora f in plants.getValues()) {
				XmlElement e2 = e.OwnerDocument.CreateElement("plant");
				e2.addProperty("name", f.getName());
				e2.addProperty("weight", plants.getWeight(f));
				e.AppendChild(e2);
			}
			e.addProperty("lit", preferLit);
			e.addProperty("count", count);
			e.addProperty("fuzz", fuzz);
		}
		
	}
}
