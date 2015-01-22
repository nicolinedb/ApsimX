﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Models.Core;
using Models.Soils;
using System.Xml.Serialization;

namespace Models.PMF
{
    /// <summary>
    /// A model of a simple tree
    /// </summary>
    [Serializable]
    [ViewName("UserInterface.Views.GridView")]
    [PresenterName("UserInterface.Presenters.PropertyPresenter")]
    public class SimpleTree : Model, ICrop
    {
        /// <summary>The soil</summary>
        [Link]
        Soils.Soil Soil = null;
        /// <summary>Occurs when [nitrogen changed].</summary>
        public event NitrogenChangedDelegate NitrogenChanged;
        /// <summary>Occurs when [new canopy].</summary>
        public event NewCanopyDelegate NewCanopy;

        /// <summary>Provides canopy data to micromet.</summary>
        public NewCanopyType CanopyData
        {
            get
            {
                NewCanopyType LocalCanopyData = new NewCanopyType();
                LocalCanopyData.cover = CoverLive;
                LocalCanopyData.cover_tot = CoverLive;
                LocalCanopyData.height = Height;
                LocalCanopyData.depth = Height;
                LocalCanopyData.lai = LAI;
                LocalCanopyData.lai_tot = LAI;
                LocalCanopyData.sender = Name;
                return LocalCanopyData;
            }
        }


        /// <summary>
        /// Is the plant alive?
        /// </summary>
        public bool IsAlive
        {
            get { return true; }
        }

        /// <summary>LeafAreaIndex</summary>
        /// <value>The leaf area index.</value>
        [Description("Leaf Area Index (m^2/m^2)")]
        [Units("m^2/m^2")]
        public double LAI { get; set; }

        /// <summary>Height</summary>
        /// <value>The plant height.</value>
        [Description("Height (mm)")]
        [Units("mm")]
        public double Height { get; set; }

        /// <summary>Rooting Depth</summary>
        /// <value>The rooting depth.</value>
        [Description("Root Depth (m/m)")]
        [Units("mm")]
        public double RootDepth { get; set; }

        /// <summary>The daily N demand</summary>
        /// <value>The daily N demand.</value>
        [Description("N Demand (kg/ha)")]
        [Units("kg/ha")]
        public double NDemand { get; set; }


        /// <summary>Cover live</summary>
        /// <value>The cover live.</value>
        public double CoverLive {get { return 1.0 - Math.Exp(-0.5 * LAI); }}
        
        /// <summary>The plant_status</summary>
        [XmlIgnore]
        public string plant_status = "alive";

        /// <summary>The sw uptake</summary>
        double[] SWUptake;
        /// <summary>The no3 uptake</summary>
        double[] NO3Uptake;
        /// <summary>The nh4 uptake</summary>
        double[] NH4Uptake;

        /// <summary>A list of uptakes generated for the soil arbitrator</summary>
        [XmlIgnore]
        public List<ZoneWaterAndN> Uptakes;
        /// <summary>The actual uptake of the plant</summary>
        /// <value>The uptake.</value>
        [XmlIgnore]
        public double[] Uptake {get;set;}

        /// <summary>Constructor</summary>
        public SimpleTree()
        {
            Name = "SimpleTree";
        }

        /// <summary>The type of crop</summary>
        /// <value>Type of crop.</value>
        [Description("Crop Type")]
        [Units("")]
        public string CropType { get; set; }

        /// <summary>Frogger. Used for MicroClimate I think?</summary>
        public double FRGR { get { return 1; } }
        /// <summary>Gets a list of cultivar names</summary>
        public string[] CultivarNames
        {
            get
            {
                return null;
            }
        }

        /// <summary>MicroClimate supplies PotentialEP</summary>
        [XmlIgnore]
        public double PotentialEP { get; set; }
        /// <summary>MicroClimate supplies PotentialEP</summary>
        [XmlIgnore]
        public double EP { get; set; }

        /// <summary>MicroClimate supplies LightProfile</summary>
        [XmlIgnore]
        public CanopyEnergyBalanceInterceptionlayerType[] LightProfile { get; set; }

        /// <summary>Simulation start</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("Commencing")]
        private void OnSimulationCommencing(object sender, EventArgs e)
        {
            Uptakes = new List<ZoneWaterAndN>();
            NewCanopy.Invoke(CanopyData);
            EP = 0;
     
        }

        /// <summary>Run at start of day</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        [EventSubscribe("DoDailyInitialisation")]
        private void OnDoDailyInitialisation(object sender, EventArgs e)
        {
        }

        /// <summary>Calculate the potential sw uptake for today</summary>
        /// <param name="info"></param>
        /// <returns></returns>
        /// <exception cref="ApsimXException">Could not find root zone in Zone  + this.Parent.Name +  for SimpleTree</exception>
        public List<ZoneWaterAndN> GetSWUptakes(Models.Soils.SoilState soilstate)
        {
            ZoneWaterAndN MyZone = new ZoneWaterAndN();
            foreach (ZoneWaterAndN Z in soilstate.Zones)
                if (Z.Name == this.Parent.Name)
                    MyZone = Z;


            double[] PotSWUptake = new double[Soil.LL15.Length];
            SWUptake = new double[Soil.LL15.Length];

            SoilCrop soilCrop = Soil.Crop(this.Name) as SoilCrop;

            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                PotSWUptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * soilCrop.KL[j] * (MyZone.Water[j] - Soil.SoilWater.LL15mm[j]));

            double TotPotSWUptake = Utility.Math.Sum(PotSWUptake);
            
            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                SWUptake[j] = PotSWUptake[j] * Math.Min(1.0, PotentialEP / TotPotSWUptake);

            List<Soils.ZoneWaterAndN> Uptakes = new List<Soils.ZoneWaterAndN>();
            Soils.ZoneWaterAndN Uptake = new Soils.ZoneWaterAndN();

            Uptake.Name = this.Parent.Name;
            Uptake.Water = SWUptake;
            Uptake.NO3N = new double[SWUptake.Length];
            Uptake.NH4N = new double[SWUptake.Length];
            Uptakes.Add(Uptake);
            return Uptakes;

        }
        /// <summary>Placeholder for SoilArbitrator</summary>
        /// <param name="info"></param>
        /// <returns></returns>
        public List<Soils.ZoneWaterAndN> GetNUptakes(Models.Soils.SoilState soilstate)
        {
            ZoneWaterAndN MyZone = new ZoneWaterAndN();
            foreach (ZoneWaterAndN Z in soilstate.Zones)
                if (Z.Name == this.Parent.Name)
                    MyZone = Z;

            double[] PotNO3Uptake = new double[Soil.NO3N.Length];
            double[] PotNH4Uptake = new double[Soil.NH4N.Length];
            NO3Uptake = new double[Soil.NO3N.Length];
            NH4Uptake = new double[Soil.NH4N.Length];

            SoilCrop soilCrop = Soil.Crop(this.Name) as SoilCrop;

            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
            {
                PotNO3Uptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * soilCrop.KL[j] * MyZone.NO3N[j]);
                PotNH4Uptake[j] = Math.Max(0.0, RootProportion(j, RootDepth) * soilCrop.KL[j] * MyZone.NH4N[j]);
            }
            double TotPotNUptake = Utility.Math.Sum(PotNO3Uptake) + Utility.Math.Sum(PotNH4Uptake);

            for (int j = 0; j < Soil.NO3N.Length; j++)
            {
                NO3Uptake[j] = PotNO3Uptake[j] * Math.Min(1.0, NDemand / TotPotNUptake);
                NH4Uptake[j] = PotNH4Uptake[j] * Math.Min(1.0, NDemand / TotPotNUptake);
            }
            List<Soils.ZoneWaterAndN> Uptakes = new List<Soils.ZoneWaterAndN>();
            Soils.ZoneWaterAndN Uptake = new Soils.ZoneWaterAndN();

            Uptake.Name = this.Parent.Name;
            Uptake.NO3N = NO3Uptake;
            Uptake.NH4N = NH4Uptake;
            Uptake.Water = new double[NO3Uptake.Length];
            Uptakes.Add(Uptake);
            return Uptakes;
        }

        /// <summary>
        /// Set the sw uptake for today
        /// </summary>
        public void SetSWUptake(List<Soils.ZoneWaterAndN> info)
        {
            SWUptake = info[0].Water;
            EP = Utility.Math.Sum(SWUptake);

            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
                Soil.SoilWater.SetSWmm(j, Soil.SoilWater.SWmm[j] - SWUptake[j]);
        }
        /// <summary>
        /// Set the n uptake for today
        /// </summary>
        public void SetNUptake(List<Soils.ZoneWaterAndN> info)
        {
            NitrogenChangedType NUptakeType = new NitrogenChangedType();
            NUptakeType.Sender = Name;
            NUptakeType.SenderType = "Plant";
            NUptakeType.DeltaNO3 = new double[Soil.Thickness.Length];
            NUptakeType.DeltaNH4 = new double[Soil.Thickness.Length];
            NO3Uptake = info[0].NO3N;
            NH4Uptake = info[0].NH4N;

            for (int j = 0; j < Soil.SoilWater.LL15mm.Length; j++)
            {
                    NUptakeType.DeltaNO3[j] = -NO3Uptake[j];
                    NUptakeType.DeltaNH4[j] = -NH4Uptake[j];
            }
            
            if (NitrogenChanged != null)
                NitrogenChanged.Invoke(NUptakeType);
        }



       /// <summary>Sows the plant</summary>
        /// <param name="cultivar">The cultivar.</param>
        /// <param name="population">The population.</param>
        /// <param name="depth">The depth.</param>
        /// <param name="rowSpacing">The row spacing.</param>
        /// <param name="maxCover">The maximum cover.</param>
        /// <param name="budNumber">The bud number.</param>
        public void Sow(string cultivar, double population, double depth, double rowSpacing, double maxCover = 1, double budNumber = 1)
        {

        }
        /// <summary>Roots the proportion.</summary>
        /// <param name="layer">The layer.</param>
        /// <param name="root_depth">The root_depth.</param>
        /// <returns></returns>
        private double RootProportion(int layer, double root_depth)
        {
            double depth_to_layer_bottom = 0;   // depth to bottom of layer (mm)
            double depth_to_layer_top = 0;      // depth to top of layer (mm)
            double depth_to_root = 0;           // depth to root in layer (mm)
            double depth_of_root_in_layer = 0;  // depth of root within layer (mm)
            // Implementation Section ----------------------------------
            for (int i = 0; i <= layer; i++)
                depth_to_layer_bottom += Soil.Thickness[i];
            depth_to_layer_top = depth_to_layer_bottom - Soil.Thickness[layer];
            depth_to_root = Math.Min(depth_to_layer_bottom, root_depth);
            depth_of_root_in_layer = Math.Max(0.0, depth_to_root - depth_to_layer_top);

            return depth_of_root_in_layer / Soil.Thickness[layer];
        }
    }
}