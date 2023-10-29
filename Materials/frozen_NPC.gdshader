shader_type spatial;
uniform vec3 baseColor = vec3(0.5, 0.6, 1.0);
void vertex() {
	float st = sin(TIME * 50.0 - VERTEX.y * 50.0);
	VERTEX += vec3(0, 0, st) * 0.01;
}
void fragment() {
	ALBEDO = baseColor;
	float scint = clamp(sin(TIME * 10.0 - VERTEX.y * 39.0) * 0.5 + 0.5, 0.0, 1.0);
	EMISSION = mix(ALBEDO, baseColor * 0.1f, scint);
}
