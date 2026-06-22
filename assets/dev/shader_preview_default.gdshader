shader_type canvas_item;

uniform vec2 resolution = vec2(800.0, 600.0);
uniform float time : hint_range(0.0, 100000.0) = 0.0;
uniform vec2 mouse = vec2(0.0, 0.0);

void fragment() {
	vec2 frag_coord = FRAGCOORD.xy;
	vec2 uv = frag_coord / resolution;
	vec2 mouse_uv = mouse / resolution;

	vec3 col = 0.5 + 0.5 * cos(time + uv.xyx * 6.28318 + vec3(0.0, 2.0, 4.0));
	float ring = smoothstep(0.02, 0.0, length(uv - mouse_uv) - 0.08);
	col += vec3(0.35, 0.75, 1.0) * ring;

	COLOR = vec4(col, 1.0);
}
