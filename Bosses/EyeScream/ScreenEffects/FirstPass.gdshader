shader_type canvas_item;

uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
void fragment() {
	// Get main texture
	vec4 input_texture = texture(screen_texture, SCREEN_UV);
	if (input_texture.r >= 0.98f) {
		COLOR = vec4(0, 0, 0, 1);
	}
	else {
		COLOR = texture(screen_texture, SCREEN_UV);
	}
}
