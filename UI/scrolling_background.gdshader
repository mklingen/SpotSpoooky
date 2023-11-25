shader_type canvas_item;
uniform sampler2D overlayTexture: filter_linear, repeat_enable;
uniform vec2 overlayScrollRate;
uniform vec4 texColorTint;
uniform vec2 texScale;
void fragment() {
	float aspect = SCREEN_PIXEL_SIZE.x / SCREEN_PIXEL_SIZE.y;
	vec2 normalized_uv =  SCREEN_UV.xy;
	normalized_uv.y *= aspect;
	// Place fragment code here.
	vec4 texColor = texColorTint * texture(overlayTexture, texScale * normalized_uv + overlayScrollRate * TIME); //read from texture
	float alpha = COLOR.a;
	COLOR = mix(COLOR, texColor, texColor.a);
	COLOR.a = 1.0f;
}
