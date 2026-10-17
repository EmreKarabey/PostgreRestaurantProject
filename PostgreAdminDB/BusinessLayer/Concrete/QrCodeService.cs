using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BusinessLayer.Abstract;
using QRCoder;

namespace BusinessLayer.Concrete
{
    public class QrCodeService : IQrCodeService
    {
        public string GenerateQrCode(string value)
        {
            using var memoryStream = new MemoryStream();

            QRCodeGenerator qRCodeGenerator = new QRCodeGenerator();

            QRCodeData qrCodeData = qRCodeGenerator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);

            QRCode qrCode = new QRCode(qrCodeData);

            using (Bitmap image = qrCode.GetGraphic(10))
            {
                image.Save(memoryStream, ImageFormat.Png);

                string qrcode = "data:image/png;base64," + Convert.ToBase64String(memoryStream.ToArray());

                return qrcode;

            }
        }
    }
}
