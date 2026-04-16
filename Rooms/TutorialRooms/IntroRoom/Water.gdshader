shader_type canvas_item;

uniform vec4 water_color = vec4(0.10, 0.25, 0.4, 0.8);
uniform vec4 foam_color = vec4(1, 1, 1, 0.8);

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Get main texture
	vec4 input_texture = texture(TEXTURE, UV);
	
	// Get current wave level, alternates each second
	bool foam_red = int(TIME) % 2 == 0;

	if (foam_red) {
		if (input_texture.g > 0.5) {
			COLOR = vec4(0);
		} else if (input_texture.b > 0.5) {
			COLOR = water_color;
		}
		else {
			COLOR = vec4(0);
		}
	} else {
		if (input_texture.g > 0.5) {
			COLOR = vec4(0);
		} else if (input_texture.b > 0.5 || input_texture.r > 0.5) {
			COLOR = water_color;
		} else {
			COLOR = vec4(0);
		}
		
	}
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
