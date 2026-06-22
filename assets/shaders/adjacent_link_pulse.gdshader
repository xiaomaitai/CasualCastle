shader_type canvas_item;
render_mode blend_add;

uniform float progress : hint_range(0.0, 1.0) = 0.0;

void fragment() {
	vec2 uv = UV - 0.5;
	float dist = length(uv) * 2.0;
	float eased = progress * progress * (3.0 - 2.0 * progress);

	float ring_radius = mix(0.05, 0.68, eased);
	float ring_width = mix(0.02, 0.05, eased);

	float band = abs(dist - ring_radius);
	float ring = 1.0 - smoothstep(0.0, ring_width, band);
	float halo = exp(-band * band / (ring_width * ring_width * 3.0)) * 0.4;

	float brighten = mix(0.1, 2.8, pow(progress, 0.6));
	float fade = progress > 0.8 ? 1.0 - smoothstep(0.8, 1.0, progress) : 1.0;

	float intensity = (ring + halo) * brighten * fade;
	COLOR = vec4(vec3(intensity), 1.0);
}
