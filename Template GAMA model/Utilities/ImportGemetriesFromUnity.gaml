/**
* Name: importGeometriesFromUnity
* Author: Patrick Taillandier
* Description: A simple model allow to import geometries from Unity. To be used with the "Export geometries to GAMA"
* Tags: gis, shapefile, unity, geometry, 
*/
model importGeometriesFromUnity

import "../models/UnityLink.gaml"

global {
	
	shape_file buildings_shape_file <- shape_file("../Includes/buildings.shp");
	string output_file <- "generated/blocks.shp";
	
	geometry shape <- envelope(buildings_shape_file);	
	
	bool geometries_received <- false;
	//allow to create a player agent
	bool create_player <- false;
	
	
	action manage_message_from_unity(message s) {
		//write s.contents;
		if ("points" in s.contents) and not geometries_received{
			map answer <- map(s.contents);
			list<list<list>> objects <- answer["points"];
			list<int> heights <- answer["heights"];
			list<string> names <- answer["names"];
			list<point> pts;
			int cpt <- 0;
			loop coords over: objects {
				loop pt over: coords {
					if empty(pt) {
						float tol <- 0.0001;
						list<geometry> gs <- [];
						list<point> ps <- [];
						if not empty(pts)and length(pts) > 2 {
								
							list<point> ts;
							list<geometry> triangles;
							
							loop i from: 0 to: length(pts) -1 {
								ts << pts[i];
								if (length(ts) = 3) {
									triangles << polygon(ts);
									ts <- [];
									
								}
										
							}
							geometry g <- union(triangles collect (each+ tol));
							loop gg over: g.geometries {
								create object with:(shape: gg, name:names[cpt]);
							}
							
						}
					 
						cpt <- cpt +1;
						pts <- [];
					
					} else {
						pts << {float(pt[0])/precision ,float(pt[1]) /precision};
					}
				}
			}
			geometries_received <- true;
			save object to: output_file format: shp;
			
			if unity_client = nil {
				write "no client to send to";
			} else {
				do send to: unity_client contents:  'ok' + end_message_symbol;	
				
			}
	
		}
		
			
	}
	
	action send_world{
		connect_to_unity <- false;
	}
}


	//Species to represent the object imported
species object {

	aspect default {
		draw shape color: #white ;
	}

}

experiment importGeometriesFromUnity type: gui autorun: true  {
	float minimum_cycle_duration <- 0.1;
	output{
		display carte type: 3d axes: true background: #black {
			species object ;
		}

	}

}
