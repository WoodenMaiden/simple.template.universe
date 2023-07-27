/**
* Name: sendGeometriesToUnity
* Author: Patrick Taillandier
* Description: A simple model allow to send geometries to Unity. To be used with the "Load geometries from GAMA"
* Tags: gis, shapefile, unity, geometry, 
*/
model traffic

import "../models/UnityLink.gaml"

global {
	
	
	//Shapefile of the bound
	shape_file bounds_shape_file <- shape_file("../Includes/bounds.shp");

	//Shapefile of the buildings
	file building_shapefile <- file("../includes/building.shp");
	//Shapefile of the roads
	file road_shapefile <- file("../includes/road.shp") ;
	//Shape of the environment

	geometry shape <- envelope(bounds_shape_file) ;
	
	
	
	init {
		//Initialization of the building using the shapefile of buildings
		create building from: building_shapefile;

		//Initialization of the road using the shapefile of roads
		create road from: road_shapefile {
			float dist <- (building closest_to self) distance_to self;
			width <- min(5.0, max(2.0,  dist - 0.5));
		}

			
		do add_background_data_with_names(building collect each.shape,  building collect each.name, 10.0, true);
		do add_background_data_with_names(road collect (each.shape buffer each.width), road collect each.name, 0.1, false);
	}
}

	//Species to represent the buildings
species building {

	aspect default {
		draw shape color: darker(#darkgray).darker depth: rnd(10) + 2;
	}

}
//Species to represent the roads
species road {

	float width;
	aspect default {
		draw (shape + width) color: #white;
	}

}

experiment sendGeometriesToUnity_batch type: batch until: cycle = 10;

experiment sendGeometriesToUnity type: gui autorun: true  {
	float minimum_cycle_duration <- 0.1;
	output{
		display carte type: 3d axes: false background: #black {
			species road refresh: false;
			species building refresh: false;
		}

	}

}
