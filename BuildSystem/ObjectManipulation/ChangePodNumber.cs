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
	internal class ChangePodNumber : ManipulationBase {
		
		private static readonly string textureSeek = "life_pod_exterior_decals_";
		private static readonly string newTexBase = "lifepod_numbering_";
		
		private int targetNumber; 
		
		static ChangePodNumber() {
			
		}
		
		internal override void applyToObject(GameObject go) {
		 	foreach (Renderer r in go.GetComponentsInChildren<Renderer>()) {
			 	foreach (Material m in r.materials) {
			 		foreach (string n in m.GetTexturePropertyNames()) {
			 			Texture tex = m.GetTexture(n);
			 			if (tex is Texture2D) {
			 				string file = tex.name;
			 				if (file.Contains(textureSeek)) {
			 					string path = "Textures/"+newTexBase+targetNumber;
			 					Texture2D tex2 = TextureManager.getTexture(path);
			 					if (tex2 == null) {
			 						SBUtil.writeToChat("Could not find desired pod number texture @ "+path);
			 						continue;
			 					}
			 					//SBUtil.writeToChat("Replacing tex @ "+n+" >> "+file+" > "+tex2.name);
			 					m.SetTexture(n, tex2);
			 				}
			 			}
			 		}
			 	}
			 	//r.UpdateGIMaterials();
		 	}
		}
		
		internal override void applyToObject(PlacedObject go) {
			applyToObject(go.obj);
		}
		
		internal override void loadFromXML(XmlElement e) {
			targetNumber = int.Parse(e.InnerText);
		}
		
		internal override void saveToXML(XmlElement e) {
			e.InnerText = targetNumber.ToString();
		}
		
	}
}
