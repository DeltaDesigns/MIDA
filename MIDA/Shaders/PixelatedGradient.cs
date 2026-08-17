using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace MIDA;

public class PixelGradientEffect : ShaderEffect
{
    private static readonly PixelShader _shader = new PixelShader
    {
        UriSource = MakePackUri("Shaders/pixelated_gradient.fx")
    };

    public PixelGradientEffect()
    {
        PixelShader = _shader;

        UpdateShaderValue(PixelSizeProperty);
        UpdateShaderValue(ColorAProperty);
        UpdateShaderValue(ColorBProperty);
        UpdateShaderValue(CenterProperty);
    }

    public static readonly DependencyProperty PixelSizeProperty =
        DependencyProperty.Register("PixelSize", typeof(Point), typeof(PixelGradientEffect),
            new UIPropertyMetadata(new Point(40, 12), PixelShaderConstantCallback(0)));

    public Point PixelSize
    {
        get => (Point)GetValue(PixelSizeProperty);
        set => SetValue(PixelSizeProperty, value);
    }

    public static readonly DependencyProperty ColorAProperty =
        DependencyProperty.Register("ColorA", typeof(Color), typeof(PixelGradientEffect),
            new UIPropertyMetadata(Colors.Yellow, PixelShaderConstantCallback(1)));

    public Color ColorA
    {
        get => (Color)GetValue(ColorAProperty);
        set => SetValue(ColorAProperty, value);
    }

    public static readonly DependencyProperty ColorBProperty =
        DependencyProperty.Register("ColorB", typeof(Color), typeof(PixelGradientEffect),
            new UIPropertyMetadata(Colors.Black, PixelShaderConstantCallback(2)));

    public Color ColorB
    {
        get => (Color)GetValue(ColorBProperty);
        set => SetValue(ColorBProperty, value);
    }

    public static readonly DependencyProperty CenterProperty =
    DependencyProperty.Register("Center", typeof(Point), typeof(PixelGradientEffect),
        new UIPropertyMetadata(new Point(0.5, 0.5), PixelShaderConstantCallback(3)));
    public Point Center
    {
        get => (Point)GetValue(CenterProperty);
        set => SetValue(CenterProperty, value);
    }

    public static readonly DependencyProperty WidthProperty =
    DependencyProperty.Register("Width", typeof(float), typeof(PixelGradientEffect),
        new UIPropertyMetadata(16f, PixelShaderConstantCallback(4)));

    public float Width
    {
        get => (float)GetValue(WidthProperty);
        set => SetValue(WidthProperty, value);
    }

    public static readonly DependencyProperty HeightProperty =
    DependencyProperty.Register("Height", typeof(float), typeof(PixelGradientEffect),
        new UIPropertyMetadata(16f, PixelShaderConstantCallback(5)));

    public float Height
    {
        get => (float)GetValue(HeightProperty);
        set => SetValue(HeightProperty, value);
    }

    public static readonly DependencyProperty FalloffProperty =
    DependencyProperty.Register(nameof(Falloff), typeof(double), typeof(PixelGradientEffect),
        new UIPropertyMetadata(3.0, PixelShaderConstantCallback(6)));

    public double Falloff
    {
        get => (double)GetValue(FalloffProperty);
        set => SetValue(FalloffProperty, value);
    }

    public static System.Uri MakePackUri(string relativeFile)
    {
        System.Reflection.Assembly a = typeof(PixelGradientEffect).Assembly;
        string assemblyShortName = a.ToString().Split(',')[0];
        string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
        return new System.Uri(uriString);
    }
}
