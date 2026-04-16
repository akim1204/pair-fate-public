shader_type canvas_item;

// If you intend to read from mipmaps with `textureLod()` LOD values greater than `0.0`,
// use `filter_nearest_mipmap` instead. This shader doesn't require it.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

uniform float rotation = 0;
const float screen_ratio = 1920.0f / 1080.0f;

// Vertical divisions of the screen
uniform float divisions = 5.0f;

// By how much divisions are offset
uniform int div_offset = 7;
uniform bool fragmented = false;
uniform bool flip_y = false;

uniform float div_burned = -1;
uniform float div_burned2 = -1;
uniform float burn = 0;

void fragment() {
	// Find the corresponding rotation position 
	vec2 position = vec2(SCREEN_UV.x - 0.5f, SCREEN_UV.y - 0.5f);
	position.x *= screen_ratio;
	position = vec2((position.x) * cos(rotation) - (position.y) * sin(rotation),
		(position.x) * sin(rotation) + (position.y) * cos(rotation));
	position.x /= screen_ratio;
	position = vec2(position.x + 0.5f, position.y + 0.5f);
        /* If out of bounds */
	if (position.y < 0.0f || position.x > 1.0f || position.y < 0.0f || position.y > 1.0f) {
    	        COLOR = vec4(0, 0, 0, 1);
	}
        /* Otherwise, find correponding mapping */
	else {
                // Calculate current division
                float div_current = floor(position.x * divisions);
                
                // Apply offset
                if (fragmented) {
                        div_current = mod((div_current + 1.0f) * float(div_offset), divisions);
                }
                
                // Flipping of screen
                vec2 fake_uv;
                fake_uv = vec2(div_current * 1.0f / divisions + mod(position.x, 1.0f / divisions), position.y);
                COLOR = textureLod(screen_texture, fake_uv, 0.0);

                // Burn rate
                if ((abs(div_current - div_burned) < 0.001f || abs(div_current - div_burned2) < 0.001f) && burn > 0.05f) {
                        COLOR = burn * vec4(0.0f, 0.0f, 0.0f, 1.0f) + (1.0f - burn) * COLOR;
                }
        }
}