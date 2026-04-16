/*
	This code is just pulled from https://gdquest.mavenseed.com/lessons/outlines-for-2d-sprites
	Should probably be updated later.
*/

shader_type canvas_item;

uniform vec4 line_color : source_color = vec4(1.0); // White color
uniform float line_thickness : hint_range(0.0, 10.0) = 1.0;
uniform bool active = true;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
	// Called for every pixel the material is visible on.
	if (active) {
	    vec2 size = TEXTURE_PIXEL_SIZE * line_thickness;

	    float outline = texture(TEXTURE, UV + vec2(-size.x, 0)).a;
	    outline += texture(TEXTURE, UV + vec2(0, size.y)).a;
	    outline += texture(TEXTURE, UV + vec2(size.x, 0)).a;
	    outline += texture(TEXTURE, UV + vec2(0, -size.y)).a;
	    outline += texture(TEXTURE, UV + vec2(-size.x, size.y)).a;
	    outline += texture(TEXTURE, UV + vec2(size.x, size.y)).a;
	    outline += texture(TEXTURE, UV + vec2(-size.x, -size.y)).a;
	    outline += texture(TEXTURE, UV + vec2(size.x, -size.y)).a;
	    outline = min(outline, 1.0);

	    vec4 color = texture(TEXTURE, UV);
	    COLOR = mix(color, line_color, outline - color.a);
	}
}
