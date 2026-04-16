shader_type canvas_item;

uniform sampler2D screen_texture_source : hint_screen_texture, repeat_disable, filter_nearest;
void fragment() {
	// Get main texture
	vec4 input_texture = texture(TEXTURE, UV);
	vec4 screen_texture = texture(screen_texture_source, SCREEN_UV);
	if (abs(screen_texture.r - screen_texture.g) >= 0.01f || abs(screen_texture.r - screen_texture.b) >= 0.01f) {
		//COLOR = vec4(min(screen_texture.r, 0.5f), min(screen_texture.g, 0.5f),min(screen_texture.b, 0.5f), 1);
		COLOR = input_texture;
	}
	else {
		COLOR = vec4(0, 0, 0, 0);
	}
}
