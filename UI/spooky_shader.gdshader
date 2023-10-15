shader_type canvas_item;
uniform vec4 baseColor;
uniform sampler2D overlayTexture: filter_linear, repeat_enable;
uniform vec2 overlayScrollRate;
uniform float overlayAlpha;
uniform vec4 endFadeColor;
uniform float endFadeAlpha;
uniform float endFadeStart;
uniform float endFadeFinal;
uniform float filledAmount;
uniform vec4 unfilledColor;

void fragment() {
	vec4 texColor = texture(TEXTURE, UV); //read from texture
	COLOR = texColor * baseColor;
	vec4 overlay = texture(overlayTexture, UV + overlayScrollRate * TIME + 0.01 * vec2(0, cos(TIME * 4.0 + UV.y * 5.0)));
	COLOR = COLOR + COLOR.a * overlay * overlayAlpha;
	float scint = clamp(sin(TIME * 5.0 + UV.y * 3.0) * 0.5 + 0.5, 0.8, 1.0);
	COLOR = mix(COLOR, endFadeColor,  scint * clamp(endFadeAlpha * (((UV.x / filledAmount) - endFadeStart) / (endFadeFinal - endFadeStart)), 0.0, 1.0));
	COLOR = mix(COLOR, unfilledColor * texColor,float(UV.x > filledAmount));
}
