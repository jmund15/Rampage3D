shader_type spatial;

uniform vec2 speed = vec2(0.01, 0.05); // Control the scrolling speed
uniform vec2 displace_speed = vec2(0.03, -0.01); // Control the displacement texture speed
uniform vec2 normal_speed = vec2(0.03, -0.01); // Control the normal map speed
uniform vec2 normal_speed2 = vec2(0.05, 0.05); // Control the normal map speed
uniform sampler2D albedo_texture: filter_nearest;
uniform sampler2D displacement_texture;
uniform sampler2D normal_texture;
uniform float displacement_strength = 0.245;

void fragment() {
    vec2 uv = UV + TIME * speed; // Move the texture over time
    vec2 displace_uv = UV + TIME * displace_speed; // Move the displacement texture over time
    vec2 normal_uv = UV + TIME * normal_speed; // Move the normal map over time
    vec2 normal_uv2 = UV + TIME * normal_speed2; // Move the normal map over time
    
    float displacement = texture(displacement_texture, displace_uv).r * displacement_strength;
    vec3 normal_offset = normalize(NORMAL) * displacement;
    vec4 tex_color = texture(albedo_texture, uv + normal_offset.xy);
    vec3 normal_map = texture(normal_texture, normal_uv).rgb * 2.0 - 1.0;
    NORMAL = normalize(NORMAL + normal_map);
    ALBEDO = tex_color.rgb;
}
