shader_type spatial;

uniform sampler2D palette; // A texture containing the color palette
uniform int palette_size = 16; // Number of colors in the palette
uniform sampler2D SCREEN_TEXTURE : hint_screen_texture, filter_linear_mipmap;


vec3 find_closest_palette_color(vec3 color) {
    float closest_dist = 10000.0;
    vec3 closest_color = color;
    
    for (int i = 0; i < palette_size; i++) {
        float index = float(i) / float(palette_size - 1);
        vec3 palette_color = texture(palette, vec2(index, 0.5)).rgb;
        float dist = distance(color, palette_color);
        
        if (dist < closest_dist) {
            closest_dist = dist;
            closest_color = palette_color;
        }
    }
    return closest_color;
}

void fragment() {
    vec3 scene_color = texture(SCREEN_TEXTURE, SCREEN_UV).rgb;
    vec3 filtered_color = find_closest_palette_color(scene_color);
    
    ALBEDO = filtered_color;
}
