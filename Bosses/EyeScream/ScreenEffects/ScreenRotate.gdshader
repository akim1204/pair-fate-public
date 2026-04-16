shader_type canvas_item;

// If you intend to read from mipmaps with `textureLod()` LOD values greater than `0.0`,
// use `filter_nearest_mipmap` instead. This shader doesn't require it.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float rotation = 0;
const float screen_ratio = 1920.0f / 1080.0f;

void fragment() {
	// Called for every pixel the material is visible on.
	vec2 position = vec2(SCREEN_UV.x - 0.5f, SCREEN_UV.y - 0.5f);
	position.x *= screen_ratio;
	position = vec2((position.x) * cos(rotation) - (position.y) * sin(rotation),
		(position.x) * sin(rotation) + (position.y) * cos(rotation));
	position.x /= screen_ratio;
	position = vec2(position.x + 0.5f, position.y + 0.5f);
	if (position.y < 0.0f || position.x > 1.0f || position.y < 0.0f || position.y > 1.0f) {
    	COLOR = vec4(0, 0, 0, 1);
	}
	else {
    	COLOR = textureLod(screen_texture, position, 0.0);
	}
}
