float PixelSize;
float4 ColorA;
float4 ColorB;
float2 Center;
float Width;
float Height;
float Falloff;

float4 main(float2 uv : TEXCOORD) : COLOR
{
	float2 elementSize = float2(Width, Height);
    float2 pixelPos = uv * elementSize;

    float2 snapped = floor(pixelPos / PixelSize) * PixelSize;
    float2 snappedUV = snapped / elementSize;

    float2 diff = snappedUV - Center;
    float dist = length(diff);
    dist = saturate(dist * Falloff);

    return lerp(ColorA, ColorB, dist);
}