using FastEndpoints;
using QRCoder;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Fonts;


namespace QRGenerator.Features;

public record GenerateQrCodeRequest(string Data, string FrameText);

public class GenerateQrCodeEndpoint : Endpoint<GenerateQrCodeRequest>
{
  public override void Configure()
  {
    Post("/api/qrcode/generate");
    AllowAnonymous();
  }

  public override async Task HandleAsync(GenerateQrCodeRequest req, CancellationToken ct)
  {
    using var qrGenerator = new QRCodeGenerator();
    var qrCodeData = qrGenerator.CreateQrCode(req.Data, QRCodeGenerator.ECCLevel.Q);
    var qrCode = new Base64QRCode(qrCodeData);

    using var qrCodeImage = Image.Load<Rgba32>(Convert.FromBase64String(qrCode.GetGraphic(20)));
    using var finalImage = AddFrameTextToQrCodeAsync(qrCodeImage, req.FrameText);
    using var stream = new MemoryStream();
    await finalImage.SaveAsync(stream, new PngEncoder(), ct);
    await SendStreamAsync(new MemoryStream(stream.ToArray()), contentType: "image/png", fileName:"qrcode.png", cancellation: ct);
  }
  
  private static Image<Rgba32> AddFrameTextToQrCodeAsync(Image qrCodeImage, string frameText)
  {
    var padding = 20;
    var textHeight = 40;
    var fonts = new FontCollection();
    var family = fonts.Add("Fonts/OpenSans-Regular.ttf"); // Path to your font file
    var font = family.CreateFont(14, FontStyle.Regular);

    var newWidth = qrCodeImage.Width + padding * 2;
    var newHeight = qrCodeImage.Height + padding * 2 + textHeight;

    var finalImage = new Image<Rgba32>(newWidth, newHeight);
    finalImage.Mutate(ctx =>
    {
      ctx.Fill(Color.White);
      ctx.DrawImage(qrCodeImage, new Point(padding, padding), 1);

      var textGraphicsOptions = new DrawingOptions
      {
        GraphicsOptions = new GraphicsOptions
        {
          Antialias = true
        }
      };

      var textSize = TextMeasurer.MeasureSize(frameText, new TextOptions(font));

      var textX = (newWidth - textSize.Width) / 2;
      var textY = qrCodeImage.Height + padding;

      ctx.DrawText(frameText, font, Color.Black, new PointF(textX, textY));
    });

    return finalImage;
  }
}