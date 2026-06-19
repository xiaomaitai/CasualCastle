shader_type canvas_item;

uniform float fill_amount : hint_range(0.0, 1.0) = 0.0;
uniform float brighten : hint_range(0.0, 1.0) = 0.35;

void fragment() {
	vec4 col = texture(TEXTURE, UV);
	float wave = 1.0 - fill_amount;
	float t = smoothstep(wave - 0.08, wave + 0.02, UV.y);
	col.rgb *= mix(1.0, 1.0 + brighten, t);
	COLOR = col;
}
