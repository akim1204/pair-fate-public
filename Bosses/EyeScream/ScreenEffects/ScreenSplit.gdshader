shader_type canvas_item;

// If you intend to read from mipmaps with `textureLod()` LOD values greater than `0.0`,
// use `filter_nearest_mipmap` instead. This shader doesn't require it.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float cut_width = 0.0f;
uniform float cut_pos = 0.5f;
void fragment() {
		vec2 fake_uv;

		if (SCREEN_UV.x > cut_pos + cut_width) {
        	fake_uv = vec2(SCREEN_UV.x - cut_width, SCREEN_UV.y);
        COLOR = textureLod(screen_texture, fake_uv, 0.0);
		}
		else if (SCREEN_UV.x < cut_pos - cut_width) {
			fake_uv = vec2(SCREEN_UV.x + cut_width, SCREEN_UV.y);
        	COLOR = textureLod(screen_texture, fake_uv, 0.0);
		}
		else {
			COLOR = vec4(0, 0, 0, 1);
		}
}