shader_type canvas_item;

// If you intend to read from mipmaps with `textureLod()` LOD values greater than `0.0`,
// use `filter_nearest_mipmap` instead. This shader doesn't require it.
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;

// Vertical divisions of the screen
const float divisions = 5.0f;

// By how much divisions are offset
uniform int div_offset = 7;
uniform bool fragmented = false;
uniform bool flip_y = false;

uniform float div_burned = -1;
uniform float div_burned2 = -1;
uniform float burn = 0;

void fragment() {
        // Calculate current division
        float div_current = floor(SCREEN_UV.x * divisions);
        
        // Apply offset
        if (fragmented) {
                div_current = mod(div_current * float(div_offset), divisions);
        }
        
        // Flipping of screen
        vec2 fake_uv;
        if (flip_y) {
                fake_uv = vec2(div_current * 1.0f / divisions + mod(SCREEN_UV.x, 1.0f / divisions), 1.0f - SCREEN_UV.y);
        }      
        else {
                fake_uv = vec2(div_current * 1.0f / divisions + mod(SCREEN_UV.x, 1.0f / divisions), SCREEN_UV.y);
        }
        COLOR = textureLod(screen_texture, fake_uv, 0.0);

        // Burn rate
        if ((abs(div_current - div_burned) < 0.001f || abs(div_current - div_burned2) < 0.001f) && burn > 0.05f) {
                COLOR = burn * vec4(0.7f, 0.0f, 0.0f, 1.0f) + (1.0f - burn) * COLOR;
        }
}